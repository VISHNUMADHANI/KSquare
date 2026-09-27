using KsSquare.Domain.Common;
namespace KsSquare.Domain.Entities;

public sealed class CustomerOrder : AuditableEntity
{
    private CustomerOrder() { }
    public CustomerOrder(Guid customerId, Guid checkoutKey, string requestHash, string customerName, string email, string phone,
        string deliveryAddress, string itemsJson, decimal total, Guid? customRequestId = null)
    {
        if (total <= 0) throw new ArgumentException("Order amount must be positive.");
        CustomerId = customerId; CheckoutKey = checkoutKey; RequestHash = requestHash;
        CustomerName = customerName; CustomerEmail = email; CustomerPhone = phone;
        DeliveryAddress = deliveryAddress; ItemsJson = itemsJson; Total = total; CustomRequestId = customRequestId;
        OrderNumber = $"KS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";
        PaymentReference = $"DEMO-{Id}";
    }
    public Guid CustomerId { get; private set; }
    public Guid CheckoutKey { get; private set; }
    public string RequestHash { get; private set; } = "";
    public string OrderNumber { get; private set; } = "";
    public string CustomerName { get; private set; } = "";
    public string CustomerEmail { get; private set; } = "";
    public string CustomerPhone { get; private set; } = "";
    public string DeliveryAddress { get; private set; } = "";
    public string ItemsJson { get; private set; } = "[]";
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = "USD";
    public bool IsDemo { get; private set; } = true;
    public string PaymentStatus { get; private set; } = "DemoPaid";
    public string PaymentReference { get; private set; } = "";
    public string Status { get; private set; } = "Confirmed";
    public Guid? CustomRequestId { get; private set; }
    public DateTimeOffset? DeliveredAt { get; private set; }
    public string? Courier { get; private set; }
    public string? TrackingNumber { get; private set; }
    public void SetTracking(string courier, string number) { Courier=courier; TrackingNumber=number; Version++; }
    public void SetDeliveryDate(DateTimeOffset date) { DeliveredAt=date; }
    public string ReturnsJson { get; private set; } = "[]";
    public void SaveReturns(string json) { ReturnsJson = json; Version++; }
    public int Version { get; private set; }

    public void SetPayPalPayment(string captureId, bool sandbox)
    {
        if(string.IsNullOrWhiteSpace(captureId))throw new ArgumentException("Capture reference is required.");
        IsDemo=sandbox; PaymentStatus=sandbox?"SandboxPaid":"Paid"; PaymentReference=captureId;
    }
    public void ChangeStatus(string next)
    {
        if (next == Status) return;
        var allowed = Status switch
        {
            "Confirmed" => next is "Processing" or "Cancelled",
            "Processing" => next is "Shipped" or "Cancelled",
            "Shipped" => next is "Delivered",
            _ => false
        };
        if (!allowed) throw new InvalidOperationException($"Cannot change {Status} to {next}.");
        Status = next; if (next == "Delivered") DeliveredAt = DateTimeOffset.UtcNow; Version++;
    }
}
