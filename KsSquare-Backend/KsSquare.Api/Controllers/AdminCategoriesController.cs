using KsSquare.Application.Features.Categories.DTOs;
using KsSquare.Application.Features.Categories.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController(ICategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken) => Ok(await categories.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, SaveCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.UpdateAsync(id, request, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await categories.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<CategoryDto>> UploadImage(Guid id, [FromForm] CategoryImageUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File is null) return BadRequest(new { error = "A category image is required." });
        await using var stream = form.File.OpenReadStream();
        var category = await categories.UploadImageAsync(id, stream, form.File.ContentType, form.File.Length, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken cancellationToken) =>
        await categories.DeleteImageAsync(id, cancellationToken) ? NoContent() : NotFound();
}

public sealed class CategoryImageUploadForm { public IFormFile? File { get; init; } }
