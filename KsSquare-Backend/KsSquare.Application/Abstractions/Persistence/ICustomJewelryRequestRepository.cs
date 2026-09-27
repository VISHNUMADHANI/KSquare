using KsSquare.Domain.Entities;

namespace KsSquare.Application.Abstractions.Persistence;

public interface ICustomJewelryRequestRepository
{
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductOption>> GetOptionsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<IReadOnlyList<CustomJewelryRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<CustomJewelryRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    void Add(CustomJewelryRequest request);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
