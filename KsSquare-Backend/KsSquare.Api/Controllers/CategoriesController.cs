using KsSquare.Application.Features.Categories.DTOs;
using KsSquare.Application.Features.Categories.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll([FromQuery] bool custom, CancellationToken cancellationToken)
    {
        var items = (await categories.GetAllAsync(cancellationToken)).Where(category => category.IsActive && (!custom || category.ShowInCustom)).ToArray();
        return Ok(items);
    }
}
