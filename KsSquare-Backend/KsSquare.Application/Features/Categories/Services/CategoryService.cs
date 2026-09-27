using System.Text;
using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Features.Categories.DTOs;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Domain.Entities;

namespace KsSquare.Application.Features.Categories.Services;

public sealed class CategoryService(ICatalogRepository repository, IObjectStorageService storage) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetCategoriesAsync(cancellationToken)).Select(Map).ToArray();

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetCategoryAsync(id, cancellationToken);
        return category is null ? null : Map(category);
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name);
        ValidateCustomVisibility(request);
        await ValidateParent(request.ParentId, null, cancellationToken);
        var slug = await UniqueSlug(request.Name, null, cancellationToken);
        var category = new Category(request.Name, slug, request.ParentId, request.IsActive, request.ShowInCustom);
        repository.AddCategory(category);
        await repository.SaveChangesAsync(cancellationToken);
        return Map((await repository.GetCategoryAsync(category.Id, cancellationToken))!);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        ValidateName(request.Name);
        ValidateCustomVisibility(request);
        var category = await repository.GetCategoryAsync(id, cancellationToken);
        if (category is null) return null;
        if (request.ParentId.HasValue && category.Children.Count > 0) throw new CatalogValidationException("A category with subcategories cannot become a subcategory.");
        await ValidateParent(request.ParentId, id, cancellationToken);
        var slug = await UniqueSlug(request.Name, id, cancellationToken);
        category.Update(request.Name, slug, request.ParentId, request.IsActive, request.ShowInCustom);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetCategoryAsync(id, cancellationToken);
        if (category is null) return false;
        if (await repository.CategoryHasDependentsAsync(id, cancellationToken)) throw new CatalogConflictException("Deactivate this category or remove its products and subcategories before deleting it.");
        if (!string.IsNullOrWhiteSpace(category.ImageObjectKey)) await storage.DeleteAsync(category.ImageObjectKey, StorageAccess.Public, cancellationToken);
        repository.RemoveCategory(category);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CategoryDto?> UploadImageAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetCategoryAsync(id, cancellationToken);
        if (category is null) return null;
        var extension = StorageFileValidator.ValidateAndGetExtension(contentType, contentLength);
        var oldKey = category.ImageObjectKey;
        var key = ObjectKeyBuilder.ForCategory(id, extension);
        var upload = await storage.UploadAsync(new ObjectUploadRequest(content, key, contentType, contentLength, StorageAccess.Public), cancellationToken);
        category.SetImage(upload.ObjectKey, upload.ContentType, upload.ContentLength);
        await repository.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(oldKey)) await storage.DeleteAsync(oldKey, StorageAccess.Public, cancellationToken);
        return Map(category);
    }

    public async Task<bool> DeleteImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetCategoryAsync(id, cancellationToken);
        if (category is null) return false;
        if (!string.IsNullOrWhiteSpace(category.ImageObjectKey)) await storage.DeleteAsync(category.ImageObjectKey, StorageAccess.Public, cancellationToken);
        category.RemoveImage();
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateParent(Guid? parentId, Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue) return;
        if (parentId == categoryId) throw new CatalogValidationException("A category cannot be its own parent.");
        var parent = await repository.GetCategoryAsync(parentId.Value, cancellationToken);
        if (parent is null) throw new CatalogValidationException("The selected parent category does not exist.");
        if (parent.ParentId.HasValue) throw new CatalogValidationException("Only one subcategory level is supported. Select a top-level category.");
    }

    private async Task<string> UniqueSlug(string name, Guid? excludingId, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        for (var suffix = 2; await repository.CategorySlugExistsAsync(slug, excludingId, cancellationToken); suffix++) slug = $"{baseSlug}-{suffix}";
        return slug;
    }

    private static string Slugify(string value)
    {
        var result = new StringBuilder();
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character)) result.Append(character);
            else if (result.Length > 0 && result[^1] != '-') result.Append('-');
        }
        return result.ToString().Trim('-');
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120) throw new CatalogValidationException("Category name is required and cannot exceed 120 characters.");
    }

    private static void ValidateCustomVisibility(SaveCategoryRequest request)
    {
        if (request.ParentId.HasValue && request.ShowInCustom) throw new CatalogValidationException("Only top-level categories can be shown in the Custom section.");
    }

    private CategoryDto Map(Category category) => new(category.Id, category.Name, category.Slug, category.ParentId, category.Parent?.Name, category.IsActive, category.ShowInCustom,
        string.IsNullOrWhiteSpace(category.ImageObjectKey) ? null : storage.GetPublicUrl(category.ImageObjectKey),
        category.ParentId.HasValue ? category.SubcategoryProducts.Count : category.Products.Count, category.Children.Count);
}
