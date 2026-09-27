using System.Security.Claims;
using KsSquare.Application.Features.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KsSquare.Api.Controllers;

[ApiController, Route("api/orders"), Authorize(Roles = "Customer")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    private Guid CustomerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpPost("{id:guid}/returns"), RequestSizeLimit(41 * 1024 * 1024)]
    public async Task<ActionResult<OrderDto>> Return(Guid id, [FromForm] ReturnRequestForm form, CancellationToken ct)
    {
        var streams = form.Files.Select(f => f.OpenReadStream()).ToArray();
        try {
            var files = form.Files.Select((f,i) => new ReturnFileInput(streams[i], f.ContentType, f.Length)).ToArray();
            return await orders.RequestReturnAsync(CustomerId, id, new(form.ItemIndex, form.Reason, form.Description, form.ConfirmCondition, form.Version, form.ItemIndices, form.Items), files, ct) is { } order ? Ok(order) : NotFound();
        } finally { foreach (var stream in streams) await stream.DisposeAsync(); }
    }
    [HttpPost("preview")]
    public async Task<ActionResult<CheckoutDto>> Preview(CheckoutRequest request, CancellationToken ct) => Ok(await orders.PreviewAsync(CustomerId, request, ct));
    [HttpGet]
    public async Task<ActionResult<OrderPageDto>> List([FromQuery] string? status, [FromQuery] int page = 1, CancellationToken ct = default) => Ok(await orders.ListAsync(CustomerId, status, page, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct) => await orders.GetAsync(id, CustomerId, ct) is { } order ? Ok(order) : NotFound();
}

[ApiController, Route("api/admin/orders"), Authorize(Roles = "Admin")]
public sealed class AdminOrdersController(IOrderService orders, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("returns")]
    public async Task<ActionResult<OrderPageDto>> Returns([FromQuery] string? status, [FromQuery] int page = 1, CancellationToken ct = default) => Ok(await orders.ListReturnsAsync(status,page,ct));
    [HttpGet]
    public async Task<ActionResult<OrderPageDto>> List([FromQuery] string? status, [FromQuery] int page = 1, CancellationToken ct = default) => Ok(await orders.ListAsync(null, status, page, ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id, CancellationToken ct) => await orders.GetAsync(id, null, ct) is { } order ? Ok(order) : NotFound();
    [HttpPut("{id:guid}/returns/{returnId:guid}")]
    public async Task<ActionResult<OrderDto>> ReviewReturn(Guid id, Guid returnId, ReviewReturnRequest request, CancellationToken ct)
    {
        if (request.Action == "Refund" && !environment.IsDevelopment()) return BadRequest(new { error = "Live refunds are not connected. No refund has been issued." });
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await orders.ReviewReturnAsync(adminId, id, returnId, request, ct) is { } order ? Ok(order) : NotFound();
    }
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> Status(Guid id, UpdateOrderStatusRequest request, CancellationToken ct) => await orders.UpdateStatusAsync(id, request, ct) is { } order ? Ok(order) : NotFound();
}

public sealed class ReturnRequestForm
{
    public int ItemIndex { get; init; }
    public int[]? ItemIndices { get; init; }
    public ReturnItemSelection[]? Items { get; init; }
    public string Reason { get; init; } = "";
    public string Description { get; init; } = "";
    public bool ConfirmCondition { get; init; }
    public int Version { get; init; }
    public List<IFormFile> Files { get; init; } = [];
}
