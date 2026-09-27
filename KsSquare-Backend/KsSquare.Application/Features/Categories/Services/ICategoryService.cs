using KsSquare.Application.Features.Categories.DTOs;

namespace KsSquare.Application.Features.Categories.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(SaveCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto?> UpdateAsync(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto?> UploadImageAsync(Guid id, Stream content, string contentType, long contentLength, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(Guid id, CancellationToken cancellationToken = default);
}
