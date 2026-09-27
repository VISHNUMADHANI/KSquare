using KsSquare.Application.Features.ProductOptions.DTOs;

namespace KsSquare.Application.Features.ProductOptions.Services;

public interface IProductOptionService
{
    Task<IReadOnlyList<ProductOptionDto>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<ProductOptionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductOptionDto> CreateAsync(SaveProductOptionRequest request, CancellationToken cancellationToken = default);
    Task<ProductOptionDto?> UpdateAsync(Guid id, SaveProductOptionRequest request, CancellationToken cancellationToken = default);
    Task<ProductOptionDto?> UploadSizeGuideAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default);
    Task<bool> DeleteSizeGuideAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductOptionDto?> UploadImageAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(Guid id, CancellationToken cancellationToken = default);
}
