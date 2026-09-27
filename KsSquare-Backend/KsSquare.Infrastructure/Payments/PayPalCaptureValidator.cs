using System.Globalization;
using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Domain.Entities;
namespace KsSquare.Infrastructure.Payments;
public static class PayPalCaptureValidator
{
 public static string? CompletedCapture(JsonElement order,PayPalAttempt attempt,string merchantId)
 {
  if(Text(order,"id")!=attempt.PayPalOrderId)throw new PaymentUnavailableException("PayPal order reference does not match. Contact support.");
  if(Text(order,"status")!="COMPLETED")return null;
  try{
   var units=order.GetProperty("purchase_units");if(units.GetArrayLength()!=1)throw new InvalidOperationException();var unit=units[0];
   if(Text(unit,"custom_id")!=attempt.Id.ToString() || Text(unit,"reference_id")!=attempt.Id.ToString())throw new InvalidOperationException();
   if(!string.IsNullOrWhiteSpace(merchantId)&&Text(unit.GetProperty("payee"),"merchant_id")!=merchantId)throw new InvalidOperationException();
   var captures=unit.GetProperty("payments").GetProperty("captures");if(captures.GetArrayLength()!=1)throw new InvalidOperationException();var capture=captures[0];
   if(Text(capture,"status")!="COMPLETED")return null;
   var amount=capture.GetProperty("amount");if(Text(amount,"currency_code")!=attempt.Currency || !decimal.TryParse(Text(amount,"value"),NumberStyles.Number,CultureInfo.InvariantCulture,out var total)||total!=attempt.Total)throw new InvalidOperationException();
   var id=Text(capture,"id");if(string.IsNullOrWhiteSpace(id)||id.Length>64)throw new InvalidOperationException();return id;
  }catch(Exception e)when(e is KeyNotFoundException or InvalidOperationException or FormatException){throw new PaymentUnavailableException("PayPal payment details need to be checked. Your order has not been marked paid. Contact support with your checkout reference.");}
 }
 public static string? Text(JsonElement element,string name)=>element.TryGetProperty(name,out var p)&&p.ValueKind==JsonValueKind.String?p.GetString():null;
}
