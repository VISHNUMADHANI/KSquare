using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace KsSquare.Infrastructure.Repositories;

public sealed class CustomJewelryRequestRepository(AppDbContext context) : ICustomJewelryRequestRepository
{
    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken cancellationToken) => context.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);
    public async Task<IReadOnlyList<ProductOption>> GetOptionsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) => ids.Count == 0 ? Array.Empty<ProductOption>() : await context.ProductOptions.Where(option => ids.Contains(option.Id)).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<CustomJewelryRequest>> GetAllAsync(CancellationToken cancellationToken) => await Query().AsNoTracking().OrderByDescending(request => request.CreatedAt).ToListAsync(cancellationToken);
    public Task<CustomJewelryRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Query().SingleOrDefaultAsync(request => request.Id == id, cancellationToken);
    public void Add(CustomJewelryRequest request) => context.CustomJewelryRequests.Add(request);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
    private IQueryable<CustomJewelryRequest> Query() => context.CustomJewelryRequests.AsSplitQuery().Include(request => request.Category).Include(request => request.Options).Include(request => request.Media);
}
