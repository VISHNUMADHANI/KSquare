namespace KsSquare.Domain.Entities;
public sealed class PayPalAttempt
{
 public Guid Id{get;set;}=Guid.NewGuid();
 public Guid CustomerId{get;set;}
 public Guid CheckoutKey{get;set;}
 public string RequestHash{get;set;}="";
 public string CheckoutJson{get;set;}="";
 public string ItemsJson{get;set;}="";
 public string ShippingJson{get;set;}="";
 public string CustomerName{get;set;}="";
 public string CustomerEmail{get;set;}="";
 public string CustomerPhone{get;set;}="";
 public decimal Total{get;set;}
 public string Currency{get;set;}="USD";
 public string Environment{get;set;}="sandbox";
 public Guid? CustomRequestId{get;set;}
 public string? PayPalOrderId{get;set;}
 public string? ApprovalUrl{get;set;}
 public string? CaptureId{get;set;}
 public Guid? OrderId{get;set;}
 public string State{get;set;}="Created";
 public DateTimeOffset CreatedAt{get;set;}=DateTimeOffset.UtcNow;
}
