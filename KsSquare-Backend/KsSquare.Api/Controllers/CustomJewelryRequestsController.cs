using KsSquare.Application.Features.CustomJewelryRequests.DTOs;
using KsSquare.Application.Features.CustomJewelryRequests.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Route("api/custom-requests")]
public sealed class CustomJewelryRequestsController(ICustomJewelryRequestService requests) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(60 * 1024 * 1024)]
    public async Task<ActionResult<CreateCustomJewelryRequestResult>> Create([FromForm] CustomJewelryRequestForm form, CancellationToken cancellationToken)
    {
        var streams = form.Files.Select(file => file.OpenReadStream()).ToArray();
        try
        {
            var files = form.Files.Select((file, index) => new CustomRequestFileInput(streams[index], file.ContentType, file.Length)).ToArray();
            var result = await requests.CreateAsync(new CreateCustomJewelryRequestRequest(form.CategoryId, form.OptionIds, form.CustomerName, form.Email, form.Phone, form.Description), files, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Request.Id }, result);
        }
        finally { foreach (var stream in streams) await stream.DisposeAsync(); }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomJewelryRequestDto>> Get(Guid id, [FromHeader(Name = "X-Custom-Request-Token")] string token, CancellationToken cancellationToken)
    {
        var request = await requests.GetCustomerAsync(id, token, cancellationToken); return request is null ? NotFound() : Ok(request);
    }
}

public sealed class CustomJewelryRequestForm
{
    public Guid CategoryId { get; init; }
    public List<Guid> OptionIds { get; init; } = [];
    public string CustomerName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<IFormFile> Files { get; init; } = [];
}
