using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KsSquare.Api.Controllers;

[ApiController, Route("api/admin"), Authorize(Roles = "Admin")]
public sealed class AdminActivityController(AppDbContext db) : ControllerBase
{
 [HttpGet("overview")]
 public async Task<IActionResult> Overview(CancellationToken ct, DateOnly? from = null, DateOnly? to = null)
 {
  if(from.HasValue != to.HasValue || (from.HasValue && (from>to || to==DateOnly.MaxValue))) return BadRequest(new{error="Choose a valid start and end date."});
  DateTimeOffset? start=from.HasValue?new DateTimeOffset(from.Value.ToDateTime(TimeOnly.MinValue),TimeSpan.Zero):null;
  DateTimeOffset? end=to.HasValue?new DateTimeOffset(to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),TimeSpan.Zero):null;
  var orders=db.CustomerOrders.AsNoTracking();
  var customers=db.Users.Where(u=>u.Role==UserRole.Customer);
  var payments=db.Set<PayPalAttempt>().AsNoTracking();
  if(start.HasValue){orders=orders.Where(o=>o.CreatedAt>=start.Value && o.CreatedAt<end!.Value);customers=customers.Where(u=>u.CreatedAt>=start.Value && u.CreatedAt<end!.Value);payments=payments.Where(a=>a.CreatedAt>=start.Value && a.CreatedAt<end!.Value);}

  var revenue=await orders.Where(o=>!o.IsDemo && o.PaymentStatus=="Paid").GroupBy(o=>o.Currency)
   .Select(g=>new{Currency=g.Key,Amount=g.Sum(o=>o.Total),PaidOrders=g.Count()}).ToListAsync(ct);
  var statuses=await orders.GroupBy(o=>o.Status).Select(g=>new{Status=g.Key,Count=g.Count()}).ToListAsync(ct);
  var recentOrders=await orders.OrderByDescending(o=>o.CreatedAt).Take(6).Select(o=>new{o.Id,o.OrderNumber,o.CustomerName,o.Total,o.Currency,o.Status,o.IsDemo,o.CreatedAt}).ToListAsync(ct);
  return Ok(new {
   Revenue=revenue,TotalOrders=await orders.CountAsync(ct),TestOrders=await orders.CountAsync(o=>o.IsDemo,ct),
   Customers=await customers.CountAsync(ct),
   PendingPayments=await payments.CountAsync(a=>a.State=="Created"||a.State=="AwaitingApproval"||a.State=="Pending",ct),
   CancelledPayments=await payments.CountAsync(a=>a.State=="Abandoned",ct),
   PendingReviews=await db.Set<ProductReview>().CountAsync(r=>r.Status=="Pending",ct),
   Statuses=statuses,RecentOrders=recentOrders,UpdatedAt=DateTimeOffset.UtcNow
  });
 }
 [HttpGet("customers")]
 public async Task<IActionResult> Customers(string? search, int page = 1, CancellationToken ct = default)
 {
  var query = db.Users.AsNoTracking().Where(u => u.Role == UserRole.Customer);
  if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLower(); query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term) || u.Phone.Contains(term)); }
  var total = await query.CountAsync(ct); page = Math.Clamp(page, 1, Math.Max(1, (total + 24) / 25));
  var items = await query.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id).Skip((page-1)*25).Take(25)
   .Select(u => new { u.Id, u.FullName, u.Email, u.Phone, u.IsEmailVerified, u.CreatedAt,
    OrderCount = db.CustomerOrders.Count(o => o.CustomerId == u.Id) }).ToListAsync(ct);
  return Ok(new { items, total, page, pageSize = 25 });
 }
 [HttpGet("customers/{id:guid}")]
 public async Task<IActionResult> Customer(Guid id, CancellationToken ct)
 {
  var customer = await db.Users.AsNoTracking().Where(u => u.Id == id && u.Role == UserRole.Customer)
   .Select(u => new { u.Id, u.FullName, u.Email, u.Phone, u.IsEmailVerified, u.CreatedAt }).SingleOrDefaultAsync(ct);
  if (customer is null) return NotFound();
  var orders = await db.CustomerOrders.AsNoTracking().Where(o => o.CustomerId == id).OrderByDescending(o => o.CreatedAt).Take(50)
   .Select(o => new { o.Id, o.OrderNumber, o.CreatedAt, o.Total, o.Currency, o.Status, o.PaymentStatus, o.DeliveryAddress }).ToListAsync(ct);
  return Ok(new { customer, orders });
 }
 [HttpGet("payments")]
 public async Task<IActionResult> Payments(string? search, string? status, int page = 1, CancellationToken ct = default)
 {
  var attempts = db.Set<PayPalAttempt>().AsNoTracking().Select(a => new PaymentRow {
   Id=a.Id, CustomerName=a.CustomerName, CustomerEmail=a.CustomerEmail, Total=a.Total, Currency=a.Currency,
   Status=a.State == "Abandoned" ? "Cancelled" : a.State == "Completed" ? "Completed" : a.State == "Created" || a.State == "AwaitingApproval" || a.State == "Pending" ? "Pending" : a.State,
   ProviderState=a.State, Environment=a.Environment, Reference=a.CaptureId ?? a.PayPalOrderId ?? "", OrderId=a.OrderId,
   CreatedAt=a.CreatedAt });
  var legacy = db.CustomerOrders.AsNoTracking().Where(o => !db.Set<PayPalAttempt>().Any(a => a.OrderId == o.Id))
   .Select(o => new PaymentRow { Id=o.Id, CustomerName=o.CustomerName, CustomerEmail=o.CustomerEmail, Total=o.Total,
    Currency=o.Currency, Status=o.PaymentStatus == "Paid" || o.PaymentStatus == "SandboxPaid" || o.PaymentStatus == "DemoPaid" ? "Completed" : o.PaymentStatus,
    ProviderState=o.PaymentStatus, Environment=o.PaymentStatus == "DemoPaid" ? "demo" : o.IsDemo ? "sandbox" : "live", Reference=o.PaymentReference, OrderId=o.Id, CreatedAt=o.CreatedAt });
  var query=attempts.Concat(legacy);
  if(!string.IsNullOrWhiteSpace(status)) query=query.Where(p=>p.Status==status);
  if(!string.IsNullOrWhiteSpace(search)){var term=search.Trim().ToLower();query=query.Where(p=>p.CustomerName.ToLower().Contains(term)||p.CustomerEmail.ToLower().Contains(term)||p.Reference.ToLower().Contains(term));}
  var total=await query.CountAsync(ct);page=Math.Clamp(page,1,Math.Max(1,(total+24)/25));
  var items=await query.OrderByDescending(p=>p.CreatedAt).ThenBy(p=>p.Id).Skip((page-1)*25).Take(25).ToListAsync(ct);
  return Ok(new {items,total,page,pageSize=25});
 }
 public sealed class PaymentRow {
  public Guid Id {get;set;} public string CustomerName {get;set;}=""; public string CustomerEmail {get;set;}="";
  public decimal Total {get;set;} public string Currency {get;set;}=""; public string Status {get;set;}="";
  public string ProviderState {get;set;}=""; public string Environment {get;set;}=""; public string Reference {get;set;}="";
  public Guid? OrderId {get;set;} public DateTimeOffset CreatedAt {get;set;}
 }
}
