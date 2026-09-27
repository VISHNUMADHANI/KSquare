using KsSquare.Application.Features.Products.DTOs;
using KsSquare.Application.Features.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet("custom")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetCustom(CancellationToken cancellationToken) => Ok(await products.GetCustomProductsAsync(cancellationToken));

    [HttpGet]
    public async Task<ActionResult<PagedProductResult>> GetAll(
        [FromQuery] string? search, [FromQuery] Guid[]? categoryIds, [FromQuery] Guid[]? subcategoryIds,
        [FromQuery] Guid[]? optionIds, [FromQuery] bool? isAvailable, [FromQuery] decimal? minimumPrice,
        [FromQuery] decimal? maximumPrice, [FromQuery] decimal? minimumDiscount, [FromQuery] string sort = "newest",
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default) =>
        Ok(await products.QueryCatalogAsync(new ProductCatalogQuery(search, categoryIds ?? [], subcategoryIds ?? [], optionIds ?? [], isAvailable, minimumPrice, maximumPrice, minimumDiscount, sort, page, pageSize), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        return product is null || !product.IsActive ? NotFound() : Ok(product);
    }
}
