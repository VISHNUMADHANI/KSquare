using KsSquare.Domain.Entities;
using KsSquare.Application.Features.Products.DTOs;

namespace KsSquare.Application.Abstractions.Persistence;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Category>> GetCategoriesByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<bool> CategorySlugExistsAsync(string slug, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> CategoryHasDependentsAsync(Guid id, CancellationToken cancellationToken);
    void AddCategory(Category category);
    void RemoveCategory(Category category);

    Task<IReadOnlyList<ProductOption>> GetProductOptionsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<ProductOption?> GetProductOptionAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductOption>> GetProductOptionsByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<bool> ProductOptionExistsAsync(ProductOptionType type, string name, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> ProductOptionIsUsedAsync(Guid id, CancellationToken cancellationToken);
    void AddProductOption(ProductOption option);
    void RemoveProductOption(ProductOption option);

    Task<IReadOnlyList<Product>> GetProductsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetCustomProductsAsync(CancellationToken cancellationToken);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> QueryProductsAsync(ProductCatalogQuery query, CancellationToken cancellationToken);
    Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string sku, Guid? excludingId, CancellationToken cancellationToken);
    void AddProduct(Product product);
    void RemoveProduct(Product product);
    void AddMedia(ProductMedia media);
    Task<ProductMedia?> GetMediaAsync(Guid productId, Guid mediaId, CancellationToken cancellationToken);
    void RemoveMedia(ProductMedia media);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
