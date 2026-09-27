using KsSquare.Application.Features.CustomJewelryRequests.DTOs;
using KsSquare.Application.Features.CustomJewelryRequests.Services;
using Microsoft.AspNetCore.Mvc;

namespace KsSquare.Api.Controllers;

[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
[Route("api/admin/custom-requests")]
public sealed class AdminCustomJewelryRequestsController(ICustomJewelryRequestService requests) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<IReadOnlyList<CustomJewelryRequestDto>>> GetAll(CancellationToken cancellationToken) => Ok(await requests.GetAdminAllAsync(cancellationToken));
    [HttpGet("{id:guid}")] public async Task<ActionResult<CustomJewelryRequestDto>> Get(Guid id, CancellationToken cancellationToken) { var request = await requests.GetAdminAsync(id, cancellationToken); return request is null ? NotFound() : Ok(request); }
    [HttpPut("{id:guid}/quote")] public async Task<ActionResult<CustomJewelryRequestDto>> Quote(Guid id, SetCustomRequestQuoteRequest body, CancellationToken cancellationToken) { var request = await requests.SetQuoteAsync(id, body.FixedPrice, cancellationToken); return request is null ? NotFound() : Ok(request); }
    [HttpPut("{id:guid}/status")] public async Task<ActionResult<CustomJewelryRequestDto>> Status(Guid id, UpdateCustomRequestStatusRequest body, CancellationToken cancellationToken) { var request = await requests.UpdateStatusAsync(id, body.Status, cancellationToken); return request is null ? NotFound() : Ok(request); }
}
