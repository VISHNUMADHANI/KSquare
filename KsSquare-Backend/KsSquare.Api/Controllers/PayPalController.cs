using System.Security.Claims;
using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Infrastructure.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace KsSquare.Api.Controllers;
[ApiController,Route("api/paypal")]
public sealed class PayPalController(PayPalCheckoutService checkout,PayPalOptions options):ControllerBase
{
 private Guid CustomerId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 [HttpGet("config")]
 public IActionResult Config()=>Ok(new{Enabled=options.Configured,Environment=options.Environment,Currency="USD"});
 [Authorize(Roles="Customer"),HttpGet("pending")]
 public async Task<IActionResult> Pending(CancellationToken ct)=>Ok(await checkout.Pending(CustomerId,ct));
 [Authorize(Roles="Customer"),HttpGet("attempts/{key:guid}")]
 public async Task<IActionResult> Get(Guid key,CancellationToken ct)=>Ok(await checkout.Get(CustomerId,key,ct));
 [Authorize(Roles="Customer"),HttpPost("orders")]
 public async Task<IActionResult> Create(CreatePayPalRequest request,CancellationToken ct)=>Ok(await checkout.Create(CustomerId,request,ct));
 [Authorize(Roles="Customer"),HttpPost("attempts/{key:guid}/capture")]
 public async Task<IActionResult> Capture(Guid key,CancellationToken ct)=>Ok(await checkout.Continue(CustomerId,key,true,false,ct));
 [Authorize(Roles="Customer"),HttpPost("attempts/{key:guid}/resume")]
 public async Task<IActionResult> Resume(Guid key,CancellationToken ct)=>Ok(await checkout.Continue(CustomerId,key,false,false,ct));
 [Authorize(Roles="Customer"),HttpPost("attempts/{key:guid}/cancel")]
 public async Task<IActionResult> Cancel(Guid key,CancellationToken ct)=>Ok(await checkout.Continue(CustomerId,key,false,true,ct));
 [AllowAnonymous,HttpPost("webhook"),RequestSizeLimit(256*1024)]
 public async Task<IActionResult> Webhook([FromBody] JsonElement payload,CancellationToken ct){var headers=Request.Headers.ToDictionary(h=>h.Key,h=>h.Value.ToString(),StringComparer.OrdinalIgnoreCase);await checkout.Webhook(payload,headers,ct);return Ok();}
}
