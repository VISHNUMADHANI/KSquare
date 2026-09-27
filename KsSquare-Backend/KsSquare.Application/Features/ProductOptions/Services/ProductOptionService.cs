using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Application.Features.ProductOptions.DTOs;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;

namespace KsSquare.Application.Features.ProductOptions.Services;

public sealed class ProductOptionService(ICatalogRepository repository, IObjectStorageService storage) : IProductOptionService
{
    public async Task<IReadOnlyList<ProductOptionDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default) =>
        (await repository.GetProductOptionsAsync(includeInactive, cancellationToken)).Select(Map).ToArray();

    public async Task<ProductOptionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        return option is null ? null : Map(option);
    }

    public async Task<ProductOptionDto> CreateAsync(SaveProductOptionRequest request, CancellationToken cancellationToken = default)
    {
        var type = Validate(request);
        if (await repository.ProductOptionExistsAsync(type, request.Name.Trim(), null, cancellationToken)) throw new CatalogConflictException("This option already exists in the selected group.");
        var option = new ProductOption(type, request.Name, request.DisplayOrder, request.IsActive);
        repository.AddProductOption(option);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(option);
    }

    public async Task<ProductOptionDto?> UpdateAsync(Guid id, SaveProductOptionRequest request, CancellationToken cancellationToken = default)
    {
        var type = Validate(request);
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return null;
        if (await repository.ProductOptionExistsAsync(type, request.Name.Trim(), id, cancellationToken)) throw new CatalogConflictException("This option already exists in the selected group.");
        option.Update(type, request.Name, request.DisplayOrder, request.IsActive);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(option);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return false;
        if (await repository.ProductOptionIsUsedAsync(id, cancellationToken)) throw new CatalogConflictException("Remove this option from products before deleting it. You can deactivate it instead.");
        if (!string.IsNullOrWhiteSpace(option.ImageObjectKey)) await storage.DeleteAsync(option.ImageObjectKey, StorageAccess.Public, cancellationToken);
        if (!string.IsNullOrWhiteSpace(option.SizeGuideObjectKey)) await storage.DeleteAsync(option.SizeGuideObjectKey, StorageAccess.Public, cancellationToken);
        repository.RemoveProductOption(option);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductOptionDto?> UploadImageAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return null;
        if (option.Type is not (ProductOptionType.Color or ProductOptionType.CustomPendantStyle)) throw new CatalogValidationException("Photos can only be added to colors or custom pendant styles.");
        var extension = StorageFileValidator.ValidateAndGetExtension(contentType, contentLength);
        var oldKey = option.ImageObjectKey;
        var upload = await storage.UploadAsync(new ObjectUploadRequest(content, ObjectKeyBuilder.ForProductOption(id, extension), contentType, contentLength, StorageAccess.Public), cancellationToken);
        option.SetImage(upload.ObjectKey, upload.ContentType, upload.ContentLength);
        await repository.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(oldKey)) await storage.DeleteAsync(oldKey, StorageAccess.Public, cancellationToken);
        return Map(option);
    }

    public async Task<bool> DeleteImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return false;
        if (!string.IsNullOrWhiteSpace(option.ImageObjectKey)) await storage.DeleteAsync(option.ImageObjectKey, StorageAccess.Public, cancellationToken);
        option.RemoveImage();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ProductOptionDto?> UploadSizeGuideAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return null;
        if (option.Type != ProductOptionType.CustomPendantStyle) throw new CatalogValidationException("Size guides are only available for custom pendant styles.");
        var extension = StorageFileValidator.ValidateAndGetExtension(contentType, contentLength);
        var oldKey = option.SizeGuideObjectKey;
        var upload = await storage.UploadAsync(new ObjectUploadRequest(content, ObjectKeyBuilder.ForProductOption(id, extension), contentType, contentLength, StorageAccess.Public), cancellationToken);
        option.SetSizeGuide(upload.ObjectKey);
        await repository.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(oldKey)) await storage.DeleteAsync(oldKey, StorageAccess.Public, cancellationToken);
        return Map(option);
    }
    public async Task<bool> DeleteSizeGuideAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var option = await repository.GetProductOptionAsync(id, cancellationToken);
        if (option is null) return false;
        var oldKey = option.SizeGuideObjectKey;
        option.SetSizeGuide(null);
        await repository.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(oldKey)) await storage.DeleteAsync(oldKey, StorageAccess.Public, cancellationToken);
        return true;
    }
    private static ProductOptionType Validate(SaveProductOptionRequest request)
    {
        if (!Enum.TryParse<ProductOptionType>(request.Type, true, out var type) || !Enum.IsDefined(type)) throw new CatalogValidationException("Choose a valid option group.");
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100) throw new CatalogValidationException("Option name is required and cannot exceed 100 characters.");
        if (request.DisplayOrder < 0) throw new CatalogValidationException("Display order cannot be negative.");
        return type;
    }

    private ProductOptionDto Map(ProductOption option) => new(option.Id, option.Type.ToString(), option.Name,
        string.IsNullOrWhiteSpace(option.ImageObjectKey) ? null : storage.GetPublicUrl(option.ImageObjectKey), option.DisplayOrder, option.IsActive, option.Products.Count, string.IsNullOrWhiteSpace(option.SizeGuideObjectKey) ? null : storage.GetPublicUrl(option.SizeGuideObjectKey));
}
