using KsSquare.Application.Features.ProductOptions.DTOs;
using KsSquare.Application.Features.ProductOptions.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
[Route("api/admin/product-options")]
public sealed class AdminProductOptionsController(IProductOptionService options) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductOptionDto>>> GetAll(CancellationToken cancellationToken) => Ok(await options.GetAllAsync(true, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductOptionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var option = await options.GetByIdAsync(id, cancellationToken);
        return option is null ? NotFound() : Ok(option);
    }

    [HttpPost]
    public async Task<ActionResult<ProductOptionDto>> Create(SaveProductOptionRequest request, CancellationToken cancellationToken)
    {
        var option = await options.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductOptionDto>> Update(Guid id, SaveProductOptionRequest request, CancellationToken cancellationToken)
    {
        var option = await options.UpdateAsync(id, request, cancellationToken);
        return option is null ? NotFound() : Ok(option);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await options.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ProductOptionDto>> UploadImage(Guid id, [FromForm] ProductOptionImageUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File is null) return BadRequest(new { error = "A photo is required." });
        await using var stream = form.File.OpenReadStream();
        var option = await options.UploadImageAsync(id, stream, form.File.ContentType, form.File.Length, cancellationToken);
        return option is null ? NotFound() : Ok(option);
    }

    [HttpPost("{id:guid}/size-guide")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<ActionResult<ProductOptionDto>> UploadSizeGuide(Guid id, [FromForm] ProductOptionImageUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File is null) return BadRequest(new { error = "A size-guide photo is required." });
        await using var stream = form.File.OpenReadStream();
        var option = await options.UploadSizeGuideAsync(id, stream, form.File.ContentType, form.File.Length, cancellationToken);
        return option is null ? NotFound() : Ok(option);
    }
    [HttpDelete("{id:guid}/size-guide")]
    public async Task<IActionResult> DeleteSizeGuide(Guid id, CancellationToken cancellationToken) =>
        await options.DeleteSizeGuideAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}/image")]
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken cancellationToken) =>
        await options.DeleteImageAsync(id, cancellationToken) ? NoContent() : NotFound();
}

public sealed class ProductOptionImageUploadForm { public IFormFile? File { get; init; } }
