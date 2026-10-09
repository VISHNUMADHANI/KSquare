using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace KsSquare.Infrastructure.Authentication;

public sealed class ResendOtpEmailSender(HttpClient http, IConfiguration configuration, ILogger<ResendOtpEmailSender> logger) : IOtpEmailSender
{
 public async Task SendAsync(string email,string fullName,string code,OtpPurpose purpose,CancellationToken cancellationToken)
 {
  var key=configuration["RESEND_API_KEY"]?.Trim();
  var from=configuration["OTP_EMAIL_FROM"]?.Trim();
  if(string.IsNullOrWhiteSpace(key)||string.IsNullOrWhiteSpace(from)||!MailAddress.TryCreate(from,out _)){
   logger.LogWarning("OTP email configuration is incomplete.");throw new OtpDeliveryException();
  }
  var reset=purpose==OtpPurpose.ResetPassword;
  var subject=reset?"Reset your K Square password":"Verify your K Square email";
  var action=reset?"reset your password":"verify your email address";
  var name=WebUtility.HtmlEncode(fullName);var safeCode=WebUtility.HtmlEncode(code);
  using var request=new HttpRequestMessage(HttpMethod.Post,"https://api.resend.com/emails");
  request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",key);
  request.Content=JsonContent.Create(new {
   from,to=new[]{email},subject,
   text=$"Hello {fullName},\n\nYour K Square verification code is {code}. Use it to {action}. This code expires in 5 minutes. Do not share it with anyone. If you did not request this, ignore this email.",
   html=$"<div style='background:#f7f3e9;padding:32px;font-family:Arial,sans-serif;color:#29251d'><div style='max-width:520px;margin:auto;background:white;padding:32px;border-radius:12px'><p style='color:#92713a;letter-spacing:3px'>K SQUARE</p><h2>{subject}</h2><p>Hello {name},</p><p>Use this code to {action}:</p><p style='font-size:32px;letter-spacing:8px;font-weight:bold;padding:20px;background:#f7f3e9'>{safeCode}</p><p>This code expires in <strong>5 minutes</strong>. Do not share it with anyone.</p><p style='font-size:12px;color:#777'>If you did not request this, you can ignore this email.</p></div></div>"
  });
  try{
   using var response=await http.SendAsync(request,cancellationToken);
   if(!response.IsSuccessStatusCode){logger.LogWarning("Resend OTP delivery failed with HTTP {Status}.",(int)response.StatusCode);throw new OtpDeliveryException();}
  }catch(HttpRequestException){logger.LogWarning("Resend OTP connection failed.");throw new OtpDeliveryException();}
  catch(TaskCanceledException)when(!cancellationToken.IsCancellationRequested){logger.LogWarning("Resend OTP request timed out.");throw new OtpDeliveryException();}
 }
}
