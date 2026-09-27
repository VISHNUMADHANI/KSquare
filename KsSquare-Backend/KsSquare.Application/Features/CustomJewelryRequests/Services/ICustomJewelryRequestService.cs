using KsSquare.Application.Features.CustomJewelryRequests.DTOs;

namespace KsSquare.Application.Features.CustomJewelryRequests.Services;

public interface ICustomJewelryRequestService
{
    Task<CreateCustomJewelryRequestResult> CreateAsync(CreateCustomJewelryRequestRequest request, IReadOnlyList<CustomRequestFileInput> files, CancellationToken cancellationToken = default);
    Task<CustomJewelryRequestDto?> GetCustomerAsync(Guid id, string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomJewelryRequestDto>> GetAdminAllAsync(CancellationToken cancellationToken = default);
    Task<CustomJewelryRequestDto?> GetAdminAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomJewelryRequestDto?> SetQuoteAsync(Guid id, decimal fixedPrice, CancellationToken cancellationToken = default);
    Task<CustomJewelryRequestDto?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
}
