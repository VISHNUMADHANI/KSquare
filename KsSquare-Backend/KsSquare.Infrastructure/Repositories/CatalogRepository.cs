using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Features.Products.DTOs;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace KsSquare.Infrastructure.Repositories;

public sealed class CatalogRepository(AppDbContext context) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await context.Categories.AsNoTracking().AsSplitQuery().Include(category => category.Parent).Include(category => category.Children).Include(category => category.Products).Include(category => category.SubcategoryProducts)
            .OrderBy(category => category.ParentId != null).ThenBy(category => category.Name).ToListAsync(cancellationToken);

    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        context.Categories.AsSplitQuery().Include(category => category.Parent).Include(category => category.Children).Include(category => category.Products).Include(category => category.SubcategoryProducts).SingleOrDefaultAsync(category => category.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetCategoriesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        ids.Count == 0 ? Array.Empty<Category>() : await context.Categories.Where(category => ids.Contains(category.Id)).ToListAsync(cancellationToken);

    public Task<bool> CategorySlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Categories.AnyAsync(category => category.Slug == slug && (!excludingId.HasValue || category.Id != excludingId), cancellationToken);

    public async Task<bool> CategoryHasDependentsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await context.Categories.AnyAsync(category => category.ParentId == id, cancellationToken)) return true;
        return await context.Products.AnyAsync(product => product.CategoryId == id || product.Subcategories.Any(category => category.Id == id), cancellationToken);
    }

    public void AddCategory(Category category) => context.Categories.Add(category);
    public void RemoveCategory(Category category) => category.DeletedAt = DateTimeOffset.UtcNow;

    public async Task<IReadOnlyList<ProductOption>> GetProductOptionsAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = context.ProductOptions.AsNoTracking().Include(option => option.Products).AsQueryable();
        if (!includeInactive) query = query.Where(option => option.IsActive);
        return await query.OrderBy(option => option.Type).ThenBy(option => option.DisplayOrder).ThenBy(option => option.Name).ToListAsync(cancellationToken);
    }

    public Task<ProductOption?> GetProductOptionAsync(Guid id, CancellationToken cancellationToken) =>
        context.ProductOptions.Include(option => option.Products).SingleOrDefaultAsync(option => option.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ProductOption>> GetProductOptionsByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        ids.Count == 0 ? Array.Empty<ProductOption>() : await context.ProductOptions.Where(option => ids.Contains(option.Id)).ToListAsync(cancellationToken);

    public Task<bool> ProductOptionExistsAsync(ProductOptionType type, string name, Guid? excludingId, CancellationToken cancellationToken) =>
        context.ProductOptions.AnyAsync(option => option.Type == type && option.Name.ToLower() == name.ToLower() && (!excludingId.HasValue || option.Id != excludingId), cancellationToken);

    public async Task<bool> ProductOptionIsUsedAsync(Guid id, CancellationToken cancellationToken)
    {
        if (await context.Products.AnyAsync(product => product.Options.Any(option => option.Id == id), cancellationToken)) return true;
        if (await context.ProductChainVariants.AnyAsync(variant => variant.ChainSizeOptionId == id || variant.ChainWidthOptionId == id || variant.ChainDiamondSizeOptionId == id, cancellationToken)) return true;
        if (await context.ProductPendantVariants.AnyAsync(v=>v.PendantSizeOptionId==id,cancellationToken)) return true;
        return await context.ProductBraceletVariants.AnyAsync(variant => variant.BraceletSizeOptionId == id || variant.BraceletStoneSizeOptionId == id, cancellationToken);
    }

    public void AddProductOption(ProductOption option) => context.ProductOptions.Add(option);
    public void RemoveProductOption(ProductOption option) => option.DeletedAt = DateTimeOffset.UtcNow;

    public async Task<IReadOnlyList<Product>> GetProductsAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = context.Products.AsNoTracking().AsSplitQuery().Include(product => product.Category).Include(product => product.Subcategories).Include(product => product.Media).Include(product => product.Options).Include(product => product.ChainVariants).Include(product => product.BraceletVariants).Include(product => product.PendantVariants).AsQueryable();
        if (!includeInactive) query = query.Where(product => product.IsActive);
        return await query.OrderByDescending(product => product.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetCustomProductsAsync(CancellationToken cancellationToken) => await context.Products.AsNoTracking().AsSplitQuery()
        .Where(product => product.IsActive && (product.ShowInCustom || product.Category.Slug == "custom"))
        .Include(product => product.Category).Include(product => product.Subcategories).Include(product => product.Media).Include(product => product.Options).Include(product => product.ChainVariants).Include(product => product.BraceletVariants).Include(product => product.PendantVariants)
        .OrderByDescending(product => product.CreatedAt).ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> QueryProductsAsync(ProductCatalogQuery request, CancellationToken cancellationToken)
    {
        var query = context.Products.AsNoTracking().Where(product => product.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(product => product.Name.ToLower().Contains(search) || product.Description.ToLower().Contains(search));
        }
        if (request.CategoryIds.Count > 0) query = query.Where(product => request.CategoryIds.Contains(product.CategoryId));
        if (request.SubcategoryIds.Count > 0) query = query.Where(product => product.Subcategories.Any(category => request.SubcategoryIds.Contains(category.Id)));
        if (request.OptionIds.Count > 0) query = query.Where(product => product.Options.Any(option => request.OptionIds.Contains(option.Id)));
        if (request.IsAvailable.HasValue) query = query.Where(product => product.IsAvailable == request.IsAvailable.Value);
        if (request.MinimumPrice.HasValue) query = query.Where(product => product.OriginalPrice * (100m - product.DiscountPercentage) / 100m >= request.MinimumPrice.Value);
        if (request.MaximumPrice.HasValue) query = query.Where(product => product.OriginalPrice * (100m - product.DiscountPercentage) / 100m <= request.MaximumPrice.Value);
        if (request.MinimumDiscount.HasValue) query = query.Where(product => product.DiscountPercentage >= request.MinimumDiscount.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        query = request.Sort switch
        {
            "price-asc" => query.OrderBy(product => product.OriginalPrice * (100m - product.DiscountPercentage) / 100m).ThenBy(product => product.Name),
            "price-desc" => query.OrderByDescending(product => product.OriginalPrice * (100m - product.DiscountPercentage) / 100m).ThenBy(product => product.Name),
            "discount-desc" => query.OrderByDescending(product => product.DiscountPercentage).ThenByDescending(product => product.CreatedAt),
            _ => query.OrderByDescending(product => product.CreatedAt)
        };
        var items = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
            .AsSplitQuery().Include(product => product.Category).Include(product => product.Subcategories).Include(product => product.Media).Include(product => product.Options).Include(product => product.ChainVariants).Include(product => product.BraceletVariants).Include(product => product.PendantVariants)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken) =>
        context.Products.AsSplitQuery().Include(product => product.Category).Include(product => product.Subcategories).Include(product => product.Media).Include(product => product.Options).Include(product => product.ChainVariants).Include(product => product.BraceletVariants).Include(product => product.PendantVariants).SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken) =>
        context.Products.AnyAsync(product => product.Sku == sku && (!excludingId.HasValue || product.Id != excludingId), cancellationToken);

    public void AddProduct(Product product) => context.Products.Add(product);
    public void RemoveProduct(Product product) => product.DeletedAt = DateTimeOffset.UtcNow;
    public void AddMedia(ProductMedia media) => context.ProductMedia.Add(media);
    public Task<ProductMedia?> GetMediaAsync(Guid productId, Guid mediaId, CancellationToken cancellationToken) =>
        context.ProductMedia.SingleOrDefaultAsync(media => media.ProductId == productId && media.Id == mediaId, cancellationToken);
    public void RemoveMedia(ProductMedia media) => media.DeletedAt = DateTimeOffset.UtcNow;
    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
