using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Domain.Entities;
using KsSquare.Infrastructure.Payments;
using Xunit;
namespace KsSquare.Storage.Tests;
public sealed class PayPalTests
{
 private readonly PayPalAttempt attempt=new(){PayPalOrderId="ORDER",Total=100,Currency="USD"};
 private JsonElement Capture(string amount="100.00",string currency="USD",string status="COMPLETED",string merchant="SELLER",string id="ORDER",string? reference=null)=>JsonSerializer.SerializeToElement(new{id,status="COMPLETED",purchase_units=new[]{new{custom_id=reference??attempt.Id.ToString(),reference_id=attempt.Id.ToString(),payee=new{merchant_id=merchant},payments=new{captures=new[]{new{id="CAPTURE",status,amount=new{value=amount,currency_code=currency}}}}}}});
 [Fact]public void CompletedPaymentAccepted()=>Assert.Equal("CAPTURE",PayPalCaptureValidator.CompletedCapture(Capture(),attempt,"SELLER"));
 [Theory][InlineData("99.99","USD")][InlineData("100.00","EUR")][InlineData("invalid","USD")]
 public void IncorrectAmountRejected(string amount,string currency)=>Assert.Throws<PaymentUnavailableException>(()=>PayPalCaptureValidator.CompletedCapture(Capture(amount,currency),attempt,"SELLER"));
 [Fact]public void WrongMerchantRejected()=>Assert.Throws<PaymentUnavailableException>(()=>PayPalCaptureValidator.CompletedCapture(Capture(merchant:"OTHER"),attempt,"SELLER"));
 [Fact]public void WrongOrderRejected()=>Assert.Throws<PaymentUnavailableException>(()=>PayPalCaptureValidator.CompletedCapture(Capture(id:"OTHER"),attempt,"SELLER"));
 [Fact]public void WrongAttemptRejected()=>Assert.Throws<PaymentUnavailableException>(()=>PayPalCaptureValidator.CompletedCapture(Capture(reference:Guid.NewGuid().ToString()),attempt,"SELLER"));
 [Theory][InlineData("PENDING")][InlineData("DECLINED")][InlineData("REFUNDED")]
 public void UncompletedCaptureIsNotPaid(string state)=>Assert.Null(PayPalCaptureValidator.CompletedCapture(Capture(status:state),attempt,"SELLER"));
 [Fact]public void MissingConfigurationDisabled()=>Assert.False(new PayPalOptions().Configured);
 [Fact]public void SandboxCredentialsEnableCheckout()=>Assert.True(new PayPalOptions{ClientId="test",ClientSecret="test"}.Configured);
 [Fact]public void LiveRequiresHttpsMerchantAndWebhook()=>Assert.False(new PayPalOptions{Environment="live",ClientId="test",ClientSecret="test"}.Configured);
 [Fact]public void LiveCompleteConfig()=>Assert.True(new PayPalOptions{Environment="live",ClientId="test",ClientSecret="test",WebhookId="hook",MerchantId="merchant",ReturnBaseUrl="https://example.test"}.Configured);
 [Fact]public void ForeignHttpReturnDisabled()=>Assert.False(new PayPalOptions{ClientId="test",ClientSecret="test",ReturnBaseUrl="http://example.test"}.Configured);
 [Fact]public void SandboxPaymentIsIdentified(){var order=new CustomerOrder(Guid.NewGuid(),Guid.NewGuid(),"hash","Name","mail","phone","address","[]",100);order.SetPayPalPayment("CAPTURE",true);Assert.True(order.IsDemo);Assert.Equal("SandboxPaid",order.PaymentStatus);}
}
