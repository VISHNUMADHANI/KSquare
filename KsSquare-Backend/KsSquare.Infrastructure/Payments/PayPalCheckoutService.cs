using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace KsSquare.Infrastructure.Payments;
public sealed class PayPalCheckoutService(AppDbContext db,IOrderService orders,IPayPalGateway gateway,PayPalOptions options)
{
 private void RequireConfigured(){if(!options.Configured)throw new PaymentUnavailableException("PayPal checkout is not available yet. Please try again later.");}
 public static PaymentAddress ValidateAddress(PaymentAddress? a)
 {
  if(a==null)throw new CatalogValidationException("Enter a delivery address.");
  var result=new PaymentAddress(a.AddressLine1?.Trim()??"",a.AddressLine2?.Trim()??"",a.City?.Trim()??"",a.State?.Trim()??"",a.PostalCode?.Trim()??"",a.CountryCode?.Trim().ToUpperInvariant()??"");
  if(result.AddressLine1.Length is <3 or >150 || result.AddressLine2!.Length>150 || result.City.Length is <1 or >80 || result.State!.Length>80 || result.PostalCode!.Length>30 || result.CountryCode.Length!=2 || result.CountryCode.Any(c=>c<'A'||c>'Z') || result.Format().Length>600)throw new CatalogValidationException("Check your delivery address and two-letter country code.");
  return result;
 }
 public async Task<PayPalAttemptDto?> Pending(Guid customerId,CancellationToken ct)
 {
  var attempt=await db.Set<PayPalAttempt>().AsNoTracking().Where(a=>a.CustomerId==customerId && a.State!="Completed" && a.State!="Abandoned").OrderByDescending(a=>a.CreatedAt).FirstOrDefaultAsync(ct);
  return attempt==null?null:await View(attempt,ct);
 }
 public async Task<PayPalAttemptDto> Get(Guid customerId,Guid key,CancellationToken ct)
 {
  var a=await db.Set<PayPalAttempt>().AsNoTracking().SingleOrDefaultAsync(a=>a.CustomerId==customerId&&a.CheckoutKey==key,ct)??throw new CatalogValidationException("This payment reference is unavailable for your account.");
  return await View(a,ct);
 }
 public async Task<PayPalAttemptDto> Create(Guid customerId,CreatePayPalRequest request,CancellationToken ct)
 {
  RequireConfigured();if(request.CheckoutKey==Guid.Empty||request.Checkout==null)throw new CatalogValidationException("A checkout reference is required.");
  var address=ValidateAddress(request.Address);
  var hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new{request.Checkout,request.ExpectedTotal,Address=address}))));
  var attempt=await db.Set<PayPalAttempt>().SingleOrDefaultAsync(a=>a.CustomerId==customerId&&a.CheckoutKey==request.CheckoutKey,ct);
  if(attempt!=null){if(attempt.RequestHash!=hash)throw new CatalogConflictException("This checkout reference has different details. Resume the saved payment or cancel it first.");}
  else{
   var preview=await orders.PreviewAsync(customerId,request.Checkout,ct);if(preview.Total!=request.ExpectedTotal)throw new CatalogConflictException("Prices changed. Review your checkout again.");
   if(preview.Total<=0||preview.Total!=decimal.Round(preview.Total,2))throw new CatalogValidationException("Invalid payment total.");
   var user=await db.Users.SingleAsync(u=>u.Id==customerId,ct);
   attempt=new PayPalAttempt{CustomerId=customerId,CheckoutKey=request.CheckoutKey,RequestHash=hash,CheckoutJson=JsonSerializer.Serialize(request.Checkout),ItemsJson=JsonSerializer.Serialize(preview.Items),ShippingJson=JsonSerializer.Serialize(address),Total=preview.Total,Currency=preview.Currency,Environment=options.Environment,CustomRequestId=request.Checkout.CustomRequestId,CustomerName=user.FullName,CustomerEmail=user.Email,CustomerPhone=user.Phone};db.Add(attempt);
   try{await db.SaveChangesAsync(ct);}catch(DbUpdateException e)when(e.InnerException is PostgresException{SqlState:PostgresErrorCodes.UniqueViolation}){db.ChangeTracker.Clear();var same=await db.Set<PayPalAttempt>().SingleOrDefaultAsync(a=>a.CustomerId==customerId&&a.CheckoutKey==request.CheckoutKey,ct);if(same==null||same.RequestHash!=hash)throw new CatalogConflictException("You already have a payment in progress. Reload checkout to resume it.");attempt=same;}
  }
  return await Continue(customerId,attempt.CheckoutKey,false,false,ct);
 }
 public async Task<PayPalAttemptDto> Continue(Guid customerId,Guid key,bool capture,bool cancel,CancellationToken ct)
 {
  RequireConfigured();
  // A row lock serializes retries, callbacks and webhooks. A successful remote capture
  // can be recovered by retrieving the PayPal order if the local commit was interrupted.
  await using var tx=await db.Database.BeginTransactionAsync(ct);
  db.ChangeTracker.Clear();
  var rows=await db.Set<PayPalAttempt>().FromSqlInterpolated($"SELECT * FROM paypal_attempts WHERE customer_id={customerId} AND checkout_key={key} FOR UPDATE").ToArrayAsync(ct);
  var a=rows.SingleOrDefault()??throw new CatalogValidationException("This payment reference is unavailable for your account.");
  if(a.Environment!=options.Environment)throw new CatalogConflictException("This checkout belongs to a different PayPal environment. Contact the store before continuing.");
  if(a.State=="Abandoned"){await tx.CommitAsync(ct);return await View(a,ct);}
  if(a.OrderId.HasValue){await tx.CommitAsync(ct);return await View(a,ct);}
  JsonElement? remote=null;
  if(a.PayPalOrderId!=null)remote=await gateway.GetAsync(a.PayPalOrderId,ct);
  if(remote is {} existing && PayPalCaptureValidator.Text(existing,"status")=="COMPLETED")await Complete(a,existing,ct);
  else if(cancel){a.State="Abandoned";a.ApprovalUrl=null;await db.SaveChangesAsync(ct);}
  else{
   if(a.PayPalOrderId==null){
    var root=options.ReturnBaseUrl+"/checkout?attempt="+a.CheckoutKey;
    remote=await gateway.CreateAsync(new(a.Id,a.Total,a.Currency,a.CustomerName,JsonSerializer.Deserialize<PaymentAddress>(a.ShippingJson)!,root+"&paypalReturn=1",root+"&paypalCancel=1"),ct);
    a.PayPalOrderId=PayPalCaptureValidator.Text(remote.Value,"id")??throw new PaymentUnavailableException("PayPal did not return an order reference. Retry this payment.");
    if(a.PayPalOrderId.Length>64)throw new PaymentUnavailableException("Invalid PayPal reference.");
    a.ApprovalUrl=Approval(remote.Value);a.State="AwaitingApproval";await db.SaveChangesAsync(ct);
   }
   if(remote==null)throw new CatalogConflictException("This PayPal checkout expired. Cancel the saved checkout and try again.");
   if(capture && PayPalCaptureValidator.Text(remote.Value,"status")=="APPROVED"){
    var captured=await gateway.CaptureAsync(a.PayPalOrderId!,a.Id,ct);
    await Complete(a,captured,ct);
   }
  }
  await tx.CommitAsync(ct);return await View(a,ct);
 }
 private string Approval(JsonElement remote)
 {
  var link=remote.GetProperty("links").EnumerateArray().FirstOrDefault(l=>PayPalCaptureValidator.Text(l,"rel") is "payer-action" or "approve");
  var href=link.ValueKind==JsonValueKind.Object?PayPalCaptureValidator.Text(link,"href"):null;
  if(!Uri.TryCreate(href,UriKind.Absolute,out var url)||url.Scheme!="https"||!string.IsNullOrEmpty(url.UserInfo)||!(url.Host=="paypal.com"||url.Host.EndsWith(".paypal.com",StringComparison.OrdinalIgnoreCase)))throw new PaymentUnavailableException("PayPal did not return a valid checkout link.");
  return href!;
 }
 private async Task Complete(PayPalAttempt a,JsonElement remote,CancellationToken ct)
 {
  var capture=PayPalCaptureValidator.CompletedCapture(remote,a,options.MerchantId);
  if(capture==null){a.State="Pending";await db.SaveChangesAsync(ct);return;}
  var existing=await db.CustomerOrders.SingleOrDefaultAsync(o=>o.CustomerId==a.CustomerId&&o.CheckoutKey==a.CheckoutKey,ct);
  if(existing!=null){if(existing.PaymentReference!=capture)throw new PaymentUnavailableException("The payment requires reconciliation. Contact the store.");a.OrderId=existing.Id;}
  else{
   var order=new CustomerOrder(a.CustomerId,a.CheckoutKey,a.RequestHash,a.CustomerName,a.CustomerEmail,a.CustomerPhone,JsonSerializer.Deserialize<PaymentAddress>(a.ShippingJson)!.Format(),a.ItemsJson,a.Total,a.CustomRequestId);order.SetPayPalPayment(capture,a.Environment=="sandbox");db.Add(order);a.OrderId=order.Id;
  }
  a.CaptureId=capture;a.State="Completed";a.ApprovalUrl=null;await db.SaveChangesAsync(ct);
 }
 public async Task Webhook(JsonElement payload,IReadOnlyDictionary<string,string> headers,CancellationToken ct)
 {
  RequireConfigured();if(!await gateway.VerifyWebhookAsync(payload,headers,ct))throw new CatalogValidationException("Invalid PayPal webhook signature.");
  if(PayPalCaptureValidator.Text(payload,"event_type")!="PAYMENT.CAPTURE.COMPLETED")return;
  string? id=null;if(payload.TryGetProperty("resource",out var resource)&&resource.TryGetProperty("supplementary_data",out var supplementary)&&supplementary.TryGetProperty("related_ids",out var related))id=PayPalCaptureValidator.Text(related,"order_id");
  if(id==null)throw new CatalogValidationException("Missing PayPal order reference.");
  var a=await db.Set<PayPalAttempt>().AsNoTracking().SingleOrDefaultAsync(a=>a.PayPalOrderId==id,ct);if(a==null)return;
  await Continue(a.CustomerId,a.CheckoutKey,false,false,ct);
 }
 private async Task<PayPalAttemptDto> View(PayPalAttempt a,CancellationToken ct)=>new(a.CheckoutKey,a.PayPalOrderId,a.ApprovalUrl,a.State,a.Environment,JsonSerializer.Deserialize<CheckoutRequest>(a.CheckoutJson)!,new(JsonSerializer.Deserialize<OrderItemDto[]>(a.ItemsJson)!,a.Total,a.Currency),JsonSerializer.Deserialize<PaymentAddress>(a.ShippingJson)!,a.OrderId.HasValue?await orders.GetAsync(a.OrderId.Value,a.CustomerId,ct):null);
}
