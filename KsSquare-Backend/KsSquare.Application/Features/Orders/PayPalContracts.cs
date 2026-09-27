using System.Text.Json;
namespace KsSquare.Application.Features.Orders;
public sealed record PaymentAddress(string AddressLine1,string? AddressLine2,string City,string? State,string? PostalCode,string CountryCode)
{
 public string Format()=>string.Join(", ",new[]{AddressLine1,AddressLine2,City,State,PostalCode,CountryCode}.Where(v=>!string.IsNullOrWhiteSpace(v)));
}
public sealed record CreatePayPalRequest(Guid CheckoutKey,CheckoutRequest Checkout,decimal ExpectedTotal,PaymentAddress Address);
public sealed record PayPalAttemptDto(Guid CheckoutKey,string? PayPalOrderId,string? ApprovalUrl,string State,string Environment,CheckoutRequest Checkout,CheckoutDto Preview,PaymentAddress Address,OrderDto? Order);
public sealed record PayPalCreateInput(Guid Id,decimal Total,string Currency,string CustomerName,PaymentAddress Address,string ReturnUrl,string CancelUrl);
public interface IPayPalGateway
{
 Task<JsonElement> CreateAsync(PayPalCreateInput request,CancellationToken ct);
 Task<JsonElement?> GetAsync(string id,CancellationToken ct);
 Task<JsonElement> CaptureAsync(string id,Guid attemptId,CancellationToken ct);
 Task<bool> VerifyWebhookAsync(JsonElement payload,IReadOnlyDictionary<string,string> headers,CancellationToken ct);
}
public sealed class PaymentUnavailableException(string message):Exception(message);
