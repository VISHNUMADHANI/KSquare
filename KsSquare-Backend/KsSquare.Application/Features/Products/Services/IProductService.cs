using KsSquare.Application.Features.Products.DTOs;

namespace KsSquare.Application.Features.Products.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<PagedProductResult> QueryCatalogAsync(ProductCatalogQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> GetCustomProductsAsync(CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(Guid id, SaveProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductMediaDto?> UploadMediaAsync(Guid productId, Stream content, string contentType, long contentLength, string altText, CancellationToken cancellationToken = default);
    Task<bool> DeleteMediaAsync(Guid productId, Guid mediaId, CancellationToken cancellationToken = default);
}
