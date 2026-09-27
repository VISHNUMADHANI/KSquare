namespace KsSquare.Application.Features.Products.DTOs;

public sealed record ProductCatalogQuery(
    string? Search,
    IReadOnlyCollection<Guid> CategoryIds,
    IReadOnlyCollection<Guid> SubcategoryIds,
    IReadOnlyCollection<Guid> OptionIds,
    bool? IsAvailable,
    decimal? MinimumPrice,
    decimal? MaximumPrice,
    decimal? MinimumDiscount,
    string Sort,
    int Page,
    int PageSize);

public sealed record PagedProductResult(IReadOnlyList<ProductDto> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
