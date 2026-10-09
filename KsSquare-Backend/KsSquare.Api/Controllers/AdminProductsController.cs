using KsSquare.Application.Features.Products.DTOs;
using KsSquare.Application.Features.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public sealed class AdminProductsController(IProductService products, IHttpClientFactory httpClients) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken) => Ok(await products.GetAllAsync(true, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(SaveProductRequest request, CancellationToken cancellationToken)
    {
        var product = await products.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, SaveProductRequest request, CancellationToken cancellationToken)
    {
        var product = await products.UpdateAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await products.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/media")]
    [RequestSizeLimit(26 * 1024 * 1024)]
    public async Task<ActionResult<ProductMediaDto>> UploadMedia(Guid id, [FromForm] ProductMediaUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File is null) return BadRequest(new { error = "An image file is required." });
        await using var stream = form.File.OpenReadStream();
        var media = await products.UploadMediaAsync(id, stream, form.File.ContentType, form.File.Length, form.AltText ?? string.Empty, cancellationToken);
        return media is null ? NotFound() : Ok(media);
    }

    [HttpGet("{id:guid}/media/{mediaId:guid}/download")]
    public async Task<IActionResult> DownloadMedia(Guid id, Guid mediaId, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        var media = product?.Media.FirstOrDefault(item => item.Id == mediaId);
        if (media is null) return NotFound();
        // URL comes from stored product media, never from the request.
        using var client = httpClients.CreateClient();
        using var response = await client.GetAsync(media.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode) return StatusCode(502, new { error = "A product photo or video could not be copied. Please try again." });
        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (output.Length + read > 26 * 1024 * 1024) return BadRequest(new { error = "This product file is too large to duplicate." });
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        return File(output.ToArray(), media.ContentType);
    }

    [HttpDelete("{id:guid}/media/{mediaId:guid}")]
    public async Task<IActionResult> DeleteMedia(Guid id, Guid mediaId, CancellationToken cancellationToken) =>
        await products.DeleteMediaAsync(id, mediaId, cancellationToken) ? NoContent() : NotFound();
}

public sealed class ProductMediaUploadForm
{
    public IFormFile? File { get; init; }
    public string? AltText { get; init; }
}
