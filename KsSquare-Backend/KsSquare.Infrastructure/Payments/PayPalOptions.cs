using Microsoft.Extensions.Configuration;
namespace KsSquare.Infrastructure.Payments;
public sealed class PayPalOptions
{
 public string Environment{get;init;}="sandbox";
 public string ClientId{get;init;}="";
 public string ClientSecret{get;init;}="";
 public string WebhookId{get;init;}="";
 public string MerchantId{get;init;}="";
 public string ReturnBaseUrl{get;init;}="http://localhost:4200";
 public bool Sandbox=>Environment=="sandbox";
 public bool Configured=>Environment is "sandbox" or "live" && !string.IsNullOrWhiteSpace(ClientId)&&!string.IsNullOrWhiteSpace(ClientSecret)&&ValidReturnUrl && (Sandbox || (!string.IsNullOrWhiteSpace(WebhookId)&&!string.IsNullOrWhiteSpace(MerchantId)));
 private bool ValidReturnUrl=>Uri.TryCreate(ReturnBaseUrl,UriKind.Absolute,out var u) && string.IsNullOrEmpty(u.Query)&&string.IsNullOrEmpty(u.Fragment)&&string.IsNullOrEmpty(u.UserInfo)&&(u.Scheme=="https" || (Sandbox && u.Scheme=="http" && u.IsLoopback));
 public string ApiUrl=>Sandbox?"https://api-m.sandbox.paypal.com/":"https://api-m.paypal.com/";
 public static PayPalOptions Read(IConfiguration c)=>new(){Environment=(c["PAYPAL_ENVIRONMENT"]??"sandbox").Trim().ToLowerInvariant(),ClientId=c["PAYPAL_CLIENT_ID"]??"",ClientSecret=c["PAYPAL_CLIENT_SECRET"]??"",WebhookId=c["PAYPAL_WEBHOOK_ID"]??"",MerchantId=c["PAYPAL_MERCHANT_ID"]??"",ReturnBaseUrl=(c["PAYPAL_RETURN_BASE_URL"]??c["FRONTEND_ORIGIN"]??"http://localhost:4200").TrimEnd('/')};
}
