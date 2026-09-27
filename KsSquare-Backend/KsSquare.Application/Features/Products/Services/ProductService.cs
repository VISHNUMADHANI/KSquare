using System.Text.RegularExpressions;
using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Application.Features.Products.DTOs;
using KsSquare.Domain.Entities;

namespace KsSquare.Application.Features.Products.Services;

public sealed partial class ProductService(ICatalogRepository repository, IObjectStorageService storage) : IProductService
{
    private const int MaximumImageCount = 6;
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        (await repository.GetProductsAsync(includeInactive, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<ProductDto>> GetCustomProductsAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetCustomProductsAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<PagedProductResult> QueryCatalogAsync(ProductCatalogQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = query with
        {
            Search = query.Search?.Trim(),
            CategoryIds = query.CategoryIds?.Distinct().ToArray() ?? Array.Empty<Guid>(),
            SubcategoryIds = query.SubcategoryIds?.Distinct().ToArray() ?? Array.Empty<Guid>(),
            OptionIds = query.OptionIds?.Distinct().ToArray() ?? Array.Empty<Guid>(),
            MinimumPrice = query.MinimumPrice is >= 0 ? query.MinimumPrice : null,
            MaximumPrice = query.MaximumPrice is >= 0 ? query.MaximumPrice : null,
            MinimumDiscount = query.MinimumDiscount is >= 0 and <= 100 ? query.MinimumDiscount : null,
            Sort = query.Sort?.ToLowerInvariant() is "price-asc" or "price-desc" or "discount-desc" ? query.Sort.ToLowerInvariant() : "newest",
            Page = Math.Max(1, query.Page),
            PageSize = Math.Clamp(query.PageSize, 1, 48)
        };
        var (items, totalCount) = await repository.QueryProductsAsync(normalized, cancellationToken);
        return new PagedProductResult(items.Select(Map).ToArray(), totalCount, normalized.Page, normalized.PageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetProductAsync(id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        if (request.IsActive) throw new CatalogValidationException("A new product must be saved as an inactive draft, then activated after at least one image is uploaded.");
        var sku = await ResolveSku(request.Sku, request.Name, null, cancellationToken);
        var product = new Product(request.Name, sku, request.Description, request.OriginalPrice, request.DiscountPercentage, request.CategoryId, request.IsAvailable, request.IsActive, request.SupportsNamePersonalization, request.NameFixedPrice, request.NamePricePerLetter, request.ShowInCustom, request.IsFinalSale);
        var category = await ValidateAndSetCategories(product, request.CategoryId, request.SubcategoryIds, cancellationToken);
        await SetOptions(product, request.OptionIds, category.Name, cancellationToken);
        SetChainVariants(product, request.ChainVariants, category.Name, request.IsActive, request.IsAvailable);
        SetBraceletVariants(product, request.BraceletVariants, category.Name, request.IsActive, request.IsAvailable);
        SetPendantVariants(product, request.PendantVariants, category.Name, request.IsActive, request.IsAvailable);
        product.SetIncludedNameLetters(request.IncludedNameLetters);
        repository.AddProduct(product);
        await repository.SaveChangesAsync(cancellationToken);
        return Map((await repository.GetProductAsync(product.Id, cancellationToken))!);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, SaveProductRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var product = await repository.GetProductAsync(id, cancellationToken);
        if (product is null) return null;
        if (request.IsActive && !product.Media.Any(m => m.ContentType.StartsWith("image/"))) throw new CatalogValidationException("At least one product image is required before activating the product.");
        var category = await ValidateAndSetCategories(product, request.CategoryId, request.SubcategoryIds, cancellationToken);
        var sku = await ResolveSku(request.Sku, request.Name, id, cancellationToken);
        product.Update(request.Name, sku, request.Description, request.OriginalPrice, request.DiscountPercentage, request.CategoryId, request.IsAvailable, request.IsActive, request.SupportsNamePersonalization, request.NameFixedPrice, request.NamePricePerLetter, request.ShowInCustom, request.IsFinalSale);
        await SetOptions(product, request.OptionIds, category.Name, cancellationToken);
        SetChainVariants(product, request.ChainVariants, category.Name, request.IsActive, request.IsAvailable);
        SetBraceletVariants(product, request.BraceletVariants, category.Name, request.IsActive, request.IsAvailable);
        SetPendantVariants(product, request.PendantVariants, category.Name, request.IsActive, request.IsAvailable);
        product.SetIncludedNameLetters(request.IncludedNameLetters);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetProductAsync(id, cancellationToken);
        if (product is null) return false;
        foreach (var media in product.Media.ToArray()) await storage.DeleteAsync(media.ObjectKey, StorageAccess.Public, cancellationToken);
        repository.RemoveProduct(product);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductMediaDto?> UploadMediaAsync(Guid productId, Stream content, string contentType, long contentLength, string altText, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetProductAsync(productId, cancellationToken);
        if (product is null) return null;
        if (product.Media.Count >= MaximumImageCount) throw new CatalogConflictException($"A product can have a maximum of {MaximumImageCount} photos and videos combined.");
        var extension = ProductMediaValidator.Validate(contentType, contentLength);
        if (!content.CanSeek) throw new CatalogValidationException("The upload stream must support seeking.");
        var position = content.Position;
        var header = new byte[12];
        var read = await content.ReadAtLeastAsync(header, 12, false, cancellationToken);
        content.Position = position;
        if (read < 12 || !ReviewMediaValidator.Matches(contentType, header)) throw new CatalogValidationException("The file contents do not match its media format.");
        var objectKey = ObjectKeyBuilder.ForProduct(productId, "gallery", extension);
        var upload = await storage.UploadAsync(new ObjectUploadRequest(content, objectKey, contentType, contentLength, StorageAccess.Public), cancellationToken);
        var media = new ProductMedia(productId, upload.ObjectKey, upload.ContentType, upload.ContentLength, string.IsNullOrWhiteSpace(altText) ? product.Name : altText, product.Media.Count);
        try
        {
            repository.AddMedia(media);
            await repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(upload.ObjectKey, StorageAccess.Public, cancellationToken);
            throw;
        }
        return MapMedia(media);
    }

    public async Task<bool> DeleteMediaAsync(Guid productId, Guid mediaId, CancellationToken cancellationToken = default)
    {
        var product = await repository.GetProductAsync(productId, cancellationToken);
        if (product is null) return false;
        var media = product.Media.SingleOrDefault(item => item.Id == mediaId);
        if (media is null) return false;
        if (product.IsActive && media.ContentType.StartsWith("image/") && product.Media.Count(m => m.ContentType.StartsWith("image/")) <= 1) throw new CatalogConflictException("An active product must keep at least one image. Deactivate the product before removing its last image.");
        await storage.DeleteAsync(media.ObjectKey, StorageAccess.Public, cancellationToken);
        repository.RemoveMedia(media);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Category> ValidateAndSetCategories(Product? product, Guid categoryId, IReadOnlyCollection<Guid>? requestedSubcategoryIds, CancellationToken cancellationToken)
    {
        var category = await repository.GetCategoryAsync(categoryId, cancellationToken);
        if (category is null) throw new CatalogValidationException("The selected category does not exist.");
        if (category.ParentId.HasValue) throw new CatalogValidationException("Select a category, not a subcategory, as the product category.");
        var ids = (requestedSubcategoryIds ?? Array.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToArray();
        var subcategories = await repository.GetCategoriesByIdsAsync(ids, cancellationToken);
        if (subcategories.Count != ids.Length) throw new CatalogValidationException("One or more selected subcategories do not exist.");
        if (subcategories.Any(subcategory => subcategory.ParentId != categoryId)) throw new CatalogValidationException("Every selected subcategory must belong to the selected category.");
        if (product is not null) { product.Subcategories.Clear(); foreach (var subcategory in subcategories) product.Subcategories.Add(subcategory); }
        return category;
    }

    private async Task SetOptions(Product product, IReadOnlyCollection<Guid>? requestedIds, string categoryName, CancellationToken cancellationToken)
    {
        var ids = (requestedIds ?? Array.Empty<Guid>()).Where(id => id != Guid.Empty).Distinct().ToArray();
        var options = await repository.GetProductOptionsByIdsAsync(ids, cancellationToken);
        if (options.Count != ids.Length) throw new CatalogValidationException("One or more selected size/color options do not exist.");
        var name = categoryName.ToLowerInvariant();
        var allowed = name.Contains("brace") ? new[] { ProductOptionType.BraceletSize, ProductOptionType.BraceletStoneSize, ProductOptionType.Color }
            : name.Contains("ring") ? new[] { ProductOptionType.RingSize, ProductOptionType.Color }
            : CatalogCategoryNames.IsPendant(categoryName) ? new[] { ProductOptionType.PendantSize, ProductOptionType.Color }
            : name.Contains("chain") ? new[] { ProductOptionType.ChainWidth, ProductOptionType.ChainSize, ProductOptionType.ChainDiamondSize }
            : Array.Empty<ProductOptionType>();
        if (options.Any(option => !allowed.Contains(option.Type))) throw new CatalogValidationException("One or more size/color values do not apply to the selected category.");
        product.Options.Clear();
        foreach (var option in options.Where(o => !product.SupportsNamePersonalization || o.Type != ProductOptionType.PendantSize)) product.Options.Add(option);
    }

    private static void SetChainVariants(Product product, IReadOnlyCollection<SaveProductChainVariantRequest>? requestedVariants, string categoryName, bool isActive, bool isAvailable)
    {
        var variants = (requestedVariants ?? Array.Empty<SaveProductChainVariantRequest>()).ToArray();
        var isChain = categoryName.Contains("chain", StringComparison.OrdinalIgnoreCase);
        if (!isChain)
        {
            if (variants.Length > 0) throw new CatalogValidationException("Chain price combinations can be added only to a Chain product.");
            product.ChainVariants.Clear();
            return;
        }

        var sizes = product.Options.Where(option => option.Type == ProductOptionType.ChainSize).Select(option => option.Id).ToHashSet();
        var widths = product.Options.Where(option => option.Type == ProductOptionType.ChainWidth).Select(option => option.Id).ToHashSet();
        var diamonds = product.Options.Where(option => option.Type == ProductOptionType.ChainDiamondSize).Select(option => option.Id).ToHashSet();
        if (variants.Any(variant => !sizes.Contains(variant.ChainSizeOptionId)
            || (widths.Count == 0 ? variant.ChainWidthOptionId.HasValue : !variant.ChainWidthOptionId.HasValue || !widths.Contains(variant.ChainWidthOptionId.Value))
            || (diamonds.Count == 0 ? variant.ChainDiamondSizeOptionId.HasValue : !variant.ChainDiamondSizeOptionId.HasValue || !diamonds.Contains(variant.ChainDiamondSizeOptionId.Value))))
            throw new CatalogValidationException("Every chain price combination must use values selected for this product.");
        if (variants.Any(variant => variant.OriginalPrice <= 0)) throw new CatalogValidationException("Every configured chain combination price must be greater than zero.");
        if (variants.Select(variant => (variant.ChainSizeOptionId, variant.ChainWidthOptionId, variant.ChainDiamondSizeOptionId)).Distinct().Count() != variants.Length)
            throw new CatalogValidationException("Duplicate chain price combinations are not allowed.");

        var expectedCount = sizes.Count * Math.Max(1, widths.Count) * Math.Max(1, diamonds.Count);
        if (isActive && (expectedCount == 0 || variants.Length != expectedCount))
            throw new CatalogValidationException("An active Chain product requires at least one Chain size and a price for every generated combination.");
        if (isActive && isAvailable && variants.Length > 0 && variants.All(variant => !variant.IsAvailable))
            throw new CatalogValidationException("An available Chain product must have at least one available price combination.");

        var remaining = variants.ToList();
        foreach (var existing in product.ChainVariants.ToArray())
        {
            var requested = remaining.FirstOrDefault(variant =>
                variant.ChainSizeOptionId == existing.ChainSizeOptionId &&
                variant.ChainWidthOptionId == existing.ChainWidthOptionId &&
                variant.ChainDiamondSizeOptionId == existing.ChainDiamondSizeOptionId);
            if (requested is null)
            {
                product.ChainVariants.Remove(existing);
                continue;
            }

            existing.Update(requested.OriginalPrice, requested.IsAvailable);
            remaining.Remove(requested);
        }

        foreach (var variant in remaining)
            product.ChainVariants.Add(new ProductChainVariant(product.Id, variant.ChainSizeOptionId, variant.ChainWidthOptionId, variant.ChainDiamondSizeOptionId, variant.OriginalPrice, variant.IsAvailable));
    }

    private static void SetBraceletVariants(Product product, IReadOnlyCollection<SaveProductBraceletVariantRequest>? requestedVariants, string categoryName, bool isActive, bool isAvailable)
    {
        var variants = (requestedVariants ?? Array.Empty<SaveProductBraceletVariantRequest>()).ToArray();
        var isBracelet = categoryName.Contains("brace", StringComparison.OrdinalIgnoreCase);
        if (!isBracelet)
        {
            if (variants.Length > 0) throw new CatalogValidationException("Bracelet price combinations can be added only to a Bracelet product.");
            product.BraceletVariants.Clear();
            return;
        }

        var sizes = product.Options.Where(option => option.Type == ProductOptionType.BraceletSize).Select(option => option.Id).ToHashSet();
        var stones = product.Options.Where(option => option.Type == ProductOptionType.BraceletStoneSize).Select(option => option.Id).ToHashSet();
        if (variants.Any(variant =>
            (sizes.Count == 0 ? variant.BraceletSizeOptionId.HasValue : !variant.BraceletSizeOptionId.HasValue || !sizes.Contains(variant.BraceletSizeOptionId.Value)) ||
            (stones.Count == 0 ? variant.BraceletStoneSizeOptionId.HasValue : !variant.BraceletStoneSizeOptionId.HasValue || !stones.Contains(variant.BraceletStoneSizeOptionId.Value))))
            throw new CatalogValidationException("Every bracelet price combination must use values selected for this product.");
        if (variants.Any(variant => !variant.BraceletSizeOptionId.HasValue && !variant.BraceletStoneSizeOptionId.HasValue))
            throw new CatalogValidationException("A bracelet price combination must include a Bracelet size or Stone size.");
        if (variants.Any(variant => variant.OriginalPrice <= 0)) throw new CatalogValidationException("Every configured bracelet combination price must be greater than zero.");
        if (variants.Select(variant => (variant.BraceletSizeOptionId, variant.BraceletStoneSizeOptionId)).Distinct().Count() != variants.Length)
            throw new CatalogValidationException("Duplicate bracelet price combinations are not allowed.");

        var expectedCount = sizes.Count == 0 && stones.Count == 0 ? 0 : Math.Max(1, sizes.Count) * Math.Max(1, stones.Count);
        if (isActive && variants.Length != expectedCount)
            throw new CatalogValidationException("An active Bracelet product requires a price for every selected Bracelet size and Stone size combination.");
        if (isActive && isAvailable && variants.Length > 0 && variants.All(variant => !variant.IsAvailable))
            throw new CatalogValidationException("An available Bracelet product must have at least one available price combination.");

        var remaining = variants.ToList();
        foreach (var existing in product.BraceletVariants.ToArray())
        {
            var requested = remaining.FirstOrDefault(variant =>
                variant.BraceletSizeOptionId == existing.BraceletSizeOptionId &&
                variant.BraceletStoneSizeOptionId == existing.BraceletStoneSizeOptionId);
            if (requested is null)
            {
                product.BraceletVariants.Remove(existing);
                continue;
            }

            existing.Update(requested.OriginalPrice, requested.IsAvailable);
            remaining.Remove(requested);
        }

        foreach (var variant in remaining)
            product.BraceletVariants.Add(new ProductBraceletVariant(product.Id, variant.BraceletSizeOptionId, variant.BraceletStoneSizeOptionId, variant.OriginalPrice, variant.IsAvailable));
    }

    private async Task<string> ResolveSku(string? requestedSku, string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedSku))
        {
            var normalized = requestedSku.Trim().ToUpperInvariant();
            if (!SkuPattern().IsMatch(normalized)) throw new CatalogValidationException("SKU must contain 3-64 letters, numbers, hyphens, or underscores.");
            if (await repository.SkuExistsAsync(normalized, excludingId, cancellationToken)) throw new CatalogConflictException("This SKU is already in use.");
            return normalized;
        }

        var prefix = new string(name.Where(char.IsLetterOrDigit).Take(8).ToArray()).ToUpperInvariant();
        if (prefix.Length < 3) prefix = "ITEM";
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var generated = $"{prefix}-{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 7, 64)].ToUpperInvariant();
            if (!await repository.SkuExistsAsync(generated, null, cancellationToken)) return generated;
        }
        throw new CatalogConflictException("A unique SKU could not be generated. Please enter one manually.");
    }

    private static void Validate(SaveProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 180) throw new CatalogValidationException("Product name is required and cannot exceed 180 characters.");
        if (request.Description?.Length > 4000) throw new CatalogValidationException("Description cannot exceed 4000 characters.");
        if (request.OriginalPrice <= 0) throw new CatalogValidationException("Original price must be greater than zero.");
        if (request.DiscountPercentage is < 0 or > 100) throw new CatalogValidationException("Discount percentage must be between 0 and 100.");
        if (request.CategoryId == Guid.Empty) throw new CatalogValidationException("Category is required.");
        if (request.IncludedNameLetters is < 1 or > 8) throw new CatalogValidationException("Letters included in the fixed price must be between 1 and 8.");
        if (request.SupportsNamePersonalization && request.NameFixedPrice <= 0) throw new CatalogValidationException("Fixed price must be greater than zero.");
        if (request.SupportsNamePersonalization && request.NamePricePerLetter < 0) throw new CatalogValidationException("Price per letter cannot be negative.");
    }

    private ProductDto Map(Product product) => new(product.Id, product.Name, product.Sku, product.Description, product.SupportsNamePersonalization ? product.NameFixedPrice : product.OriginalPrice, product.DiscountPercentage,
        product.SupportsNamePersonalization ? decimal.Round(product.NameFixedPrice * (100m - product.DiscountPercentage) / 100m, 2, MidpointRounding.AwayFromZero) : product.FinalPrice, "USD", product.CategoryId, product.Category.Name,
        product.Subcategories.OrderBy(category => category.Name).Select(category => new ProductSubcategoryDto(category.Id, category.Name, category.Slug)).ToArray(), product.IsAvailable, product.IsActive, product.SupportsNamePersonalization, product.NameFixedPrice, product.NamePricePerLetter,
        product.Media.OrderBy(media => media.ContentType.StartsWith("video/")).ThenBy(media => media.DisplayOrder).Select(MapMedia).ToArray(),
        product.Options.Where(o => !product.SupportsNamePersonalization || o.Type != ProductOptionType.PendantSize).OrderBy(option => option.Type).ThenBy(option => option.DisplayOrder).ThenBy(option => option.Name)
            .Select(option => new ProductOptionSummaryDto(option.Id, option.Type.ToString(), option.Name,
                string.IsNullOrWhiteSpace(option.ImageObjectKey) ? null : storage.GetPublicUrl(option.ImageObjectKey), option.DisplayOrder)).ToArray(),
        product.ChainVariants.OrderBy(variant => variant.OriginalPrice).Select(variant => new ProductChainVariantDto(
            variant.Id, variant.ChainSizeOptionId, variant.ChainWidthOptionId, variant.ChainDiamondSizeOptionId,
            variant.OriginalPrice, decimal.Round(variant.OriginalPrice * (100m - product.DiscountPercentage) / 100m, 2, MidpointRounding.AwayFromZero), variant.IsAvailable)).ToArray(),
        product.BraceletVariants.OrderBy(variant => variant.OriginalPrice).Select(variant => new ProductBraceletVariantDto(
            variant.Id, variant.BraceletSizeOptionId, variant.BraceletStoneSizeOptionId,
            variant.OriginalPrice, decimal.Round(variant.OriginalPrice * (100m - product.DiscountPercentage) / 100m, 2, MidpointRounding.AwayFromZero), variant.IsAvailable)).ToArray(),
        product.ShowInCustom, product.CreatedAt, product.UpdatedAt, product.IsFinalSale, product.PendantVariants.Where(v=>!product.SupportsNamePersonalization).OrderBy(v=>v.OriginalPrice).Select(v=>new ProductPendantVariantDto(v.Id,v.PendantSizeOptionId,v.OriginalPrice,decimal.Round(v.OriginalPrice*(100m-product.DiscountPercentage)/100m,2,MidpointRounding.AwayFromZero),v.IsAvailable)).ToArray(), product.IncludedNameLetters);
    private ProductMediaDto MapMedia(ProductMedia media) => new(media.Id, media.ObjectKey, storage.GetPublicUrl(media.ObjectKey), media.ContentType, media.ContentLength, media.AltText, media.DisplayOrder);

    [GeneratedRegex("^[A-Z0-9][A-Z0-9_-]{2,63}$")]
    private static partial Regex SkuPattern();
}
