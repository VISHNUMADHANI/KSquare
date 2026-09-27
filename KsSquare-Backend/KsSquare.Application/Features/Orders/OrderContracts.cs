namespace KsSquare.Application.Features.Orders;

public sealed record OrderLineRequest(Guid ProductId, int Quantity, Guid[] OptionIds, string? PersonalizedName, string? PersonalizationNote = null);
public sealed record CheckoutRequest(OrderLineRequest[] Lines, Guid? CustomRequestId);
public sealed record PlaceDemoOrderRequest(Guid CheckoutKey, CheckoutRequest Checkout, decimal ExpectedTotal, string DeliveryAddress);
public sealed record OrderItemDto(Guid? ProductId, string Name, int Quantity, decimal UnitPrice, string[] Details, bool? IsReturnable = null, string? ReturnRestriction = null, bool CanRequestReturn = false, bool CanIncludeInReturn = false, string? PersonalizationNote = null)
{ public decimal Total => UnitPrice * Quantity; }
public sealed record CheckoutDto(OrderItemDto[] Items, decimal Total, string Currency);
public sealed record OrderDto(Guid Id, string OrderNumber, Guid CustomerId, string CustomerName, string CustomerEmail, string CustomerPhone,
    string DeliveryAddress, OrderItemDto[] Items, decimal Total, string Currency, string Status, string PaymentStatus,
    bool IsDemo, string PaymentReference, DateTimeOffset CreatedAt, int Version, DateTimeOffset? DeliveredAt = null, DateTimeOffset? ReturnDeadline = null, ReturnRequestDto[]? Returns = null, string? Courier = null, string? TrackingNumber = null, string? TrackingUrl = null);
public sealed record OrderPageDto(OrderDto[] Items, int TotalCount, int Page, int PageSize);
public sealed record UpdateOrderStatusRequest(string Status, int Version, string? Courier = null, string? TrackingNumber = null, DateTimeOffset? DeliveredAt = null);
public interface IOrderService
{
    Task<OrderPageDto> ListReturnsAsync(string? status, int page, CancellationToken ct);
    Task<OrderDto?> RequestReturnAsync(Guid customerId, Guid orderId, CreateReturnRequest request, ReturnFileInput[] files, CancellationToken ct);
    Task<OrderDto?> ReviewReturnAsync(Guid adminId, Guid orderId, Guid returnId, ReviewReturnRequest request, CancellationToken ct);
    Task<CheckoutDto> PreviewAsync(Guid customerId, CheckoutRequest request, CancellationToken ct);
    Task<OrderDto> PlaceDemoAsync(Guid customerId, PlaceDemoOrderRequest request, CancellationToken ct);
    Task<OrderPageDto> ListAsync(Guid? customerId, string? status, int page, CancellationToken ct);
    Task<OrderDto?> GetAsync(Guid id, Guid? customerId, CancellationToken ct);
    Task<OrderDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct);
}
