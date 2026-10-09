using System.Net;
using System.Text.Json;
using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Domain.Entities;
using KsSquare.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
namespace KsSquare.Storage.Tests;
public class ResendOtpEmailSenderTests
{
 [Theory]
 [InlineData(OtpPurpose.VerifyEmail,"Verify")]
 [InlineData(OtpPurpose.ResetPassword,"Reset")]
 public async Task SendsCorrectPurposeAndEncodesName(OtpPurpose purpose,string subject)
 {
  var handler=new Handler(HttpStatusCode.OK);
  await Sender(handler).SendAsync("customer@example.com","<script>","123456",purpose,default);
  Assert.Equal("https://api.resend.com/emails",handler.Url);
  Assert.Equal("Bearer test-key",handler.Authorization);
  using var json=JsonDocument.Parse(handler.Body!);
  Assert.Contains(subject,json.RootElement.GetProperty("subject").GetString());
  Assert.Contains("&lt;script&gt;",json.RootElement.GetProperty("html").GetString());
  Assert.DoesNotContain("<script>",json.RootElement.GetProperty("html").GetString());
  Assert.Contains("123456",json.RootElement.GetProperty("text").GetString());
  Assert.Equal("customer@example.com",json.RootElement.GetProperty("to")[0].GetString());
 }
 [Theory]
 [InlineData(HttpStatusCode.Unauthorized)]
 [InlineData(HttpStatusCode.TooManyRequests)]
 [InlineData(HttpStatusCode.InternalServerError)]
 public async Task ProviderErrorsAreSafe(HttpStatusCode status){
  var ex=await Assert.ThrowsAsync<OtpDeliveryException>(()=>Sender(new Handler(status)).SendAsync("a@example.com","A","123456",OtpPurpose.VerifyEmail,default));
  Assert.DoesNotContain("123456",ex.Message);Assert.DoesNotContain("test-key",ex.Message);
 }
 [Fact] public async Task MissingConfigurationDoesNotSend(){
  var handler=new Handler(HttpStatusCode.OK);
  var sender=new ResendOtpEmailSender(new HttpClient(handler),new ConfigurationBuilder().Build(),NullLogger<ResendOtpEmailSender>.Instance);
  await Assert.ThrowsAsync<OtpDeliveryException>(()=>sender.SendAsync("a@example.com","A","123456",OtpPurpose.VerifyEmail,default));
  Assert.Null(handler.Url);
 }
 private static ResendOtpEmailSender Sender(Handler handler)=>new(new HttpClient(handler),new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"RESEND_API_KEY","test-key"},{"OTP_EMAIL_FROM","K Square <noreply@example.com>"}}).Build(),NullLogger<ResendOtpEmailSender>.Instance);
 private sealed class Handler(HttpStatusCode status):HttpMessageHandler{
  public string? Body,Url,Authorization;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct){Body=await request.Content!.ReadAsStringAsync(ct);Url=request.RequestUri!.ToString();Authorization=request.Headers.Authorization!.ToString();return new HttpResponseMessage(status){Content=new StringContent("{}")};}
 }
}
