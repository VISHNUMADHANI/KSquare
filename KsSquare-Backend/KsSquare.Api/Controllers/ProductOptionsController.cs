using KsSquare.Application.Features.ProductOptions.DTOs;
using KsSquare.Application.Features.ProductOptions.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Route("api/product-options")]
public sealed class ProductOptionsController(IProductOptionService options) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductOptionDto>>> GetAll(CancellationToken cancellationToken) => Ok(await options.GetAllAsync(false, cancellationToken));
}
