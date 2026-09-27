using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KsSquare.Application.Features.Orders;
namespace KsSquare.Infrastructure.Payments;
public sealed class PayPalGateway(HttpClient http,PayPalOptions options):IPayPalGateway
{
 private async Task<string> Token(CancellationToken ct)
 {
  if(!options.Configured)throw new PaymentUnavailableException("PayPal checkout is not available yet.");
  using var req=new HttpRequestMessage(HttpMethod.Post,options.ApiUrl+"v1/oauth2/token");req.Headers.Authorization=new("Basic",Convert.ToBase64String(Encoding.UTF8.GetBytes(options.ClientId+":"+options.ClientSecret)));req.Content=new FormUrlEncodedContent(new Dictionary<string,string>{{"grant_type","client_credentials"}});
  using var response=await http.SendAsync(req,ct);if(!response.IsSuccessStatusCode)throw new PaymentUnavailableException("Unable to connect to PayPal. Please try again later.");
  var json=await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken:ct);return json.GetProperty("access_token").GetString()!;
 }
 private async Task<JsonElement?> Send(HttpMethod method,string path,object? body,string? requestId,CancellationToken ct,bool allowMissing=false)
 {
  try{using var req=new HttpRequestMessage(method,options.ApiUrl+path);req.Headers.Authorization=new("Bearer",await Token(ct));if(requestId!=null)req.Headers.Add("PayPal-Request-Id",requestId);req.Headers.Add("Prefer","return=representation");if(body!=null)req.Content=JsonContent.Create(body);
  using var response=await http.SendAsync(req,ct);if(allowMissing && response.StatusCode==HttpStatusCode.NotFound)return null;
  if(!response.IsSuccessStatusCode)throw new PaymentUnavailableException("PayPal could not confirm this request. Check payment status before starting another payment.");
  return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken:ct);
  }catch(Exception e)when(e is HttpRequestException or TaskCanceledException or JsonException){throw new PaymentUnavailableException("PayPal confirmation is temporarily unavailable. Your payment reference has been saved; please check its status again.");}
 }
 public async Task<JsonElement> CreateAsync(PayPalCreateInput r,CancellationToken ct)=> (await Send(HttpMethod.Post,"v2/checkout/orders",new{intent="CAPTURE",purchase_units=new[]{new{reference_id=r.Id.ToString(),custom_id=r.Id.ToString(),amount=new{currency_code=r.Currency,value=r.Total.ToString("F2",CultureInfo.InvariantCulture)},shipping=new{name=new{full_name=r.CustomerName},address=new{address_line_1=r.Address.AddressLine1,address_line_2=r.Address.AddressLine2,admin_area_2=r.Address.City,admin_area_1=r.Address.State,postal_code=r.Address.PostalCode,country_code=r.Address.CountryCode}}}},payment_source=new{paypal=new{experience_context=new{brand_name="K Square",shipping_preference="SET_PROVIDED_ADDRESS",user_action="PAY_NOW",return_url=r.ReturnUrl,cancel_url=r.CancelUrl}}}},r.Id.ToString("N")+"c",ct))!.Value;
 public Task<JsonElement?> GetAsync(string id,CancellationToken ct)=>Send(HttpMethod.Get,"v2/checkout/orders/"+Uri.EscapeDataString(id),null,null,ct,true);
 public async Task<JsonElement> CaptureAsync(string id,Guid attemptId,CancellationToken ct)=>(await Send(HttpMethod.Post,"v2/checkout/orders/"+Uri.EscapeDataString(id)+"/capture",new{},attemptId.ToString("N")+"p",ct))!.Value;
 public async Task<bool> VerifyWebhookAsync(JsonElement payload,IReadOnlyDictionary<string,string> h,CancellationToken ct)
 {
  if(string.IsNullOrWhiteSpace(options.WebhookId))throw new PaymentUnavailableException("PayPal webhook is not configured.");
  string[] keys=["PAYPAL-AUTH-ALGO","PAYPAL-CERT-URL","PAYPAL-TRANSMISSION-ID","PAYPAL-TRANSMISSION-SIG","PAYPAL-TRANSMISSION-TIME"];if(keys.Any(k=>!h.ContainsKey(k)))return false;
  var result=await Send(HttpMethod.Post,"v1/notifications/verify-webhook-signature",new{auth_algo=h[keys[0]],cert_url=h[keys[1]],transmission_id=h[keys[2]],transmission_sig=h[keys[3]],transmission_time=h[keys[4]],webhook_id=options.WebhookId,webhook_event=payload},null,ct);
  return result?.GetProperty("verification_status").GetString()=="SUCCESS";
 }
}
