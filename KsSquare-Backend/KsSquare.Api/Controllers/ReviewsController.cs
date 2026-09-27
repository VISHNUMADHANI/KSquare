using System.Security.Claims;
using System.Text.Json;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Application.Features.Orders;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace KsSquare.Api.Controllers;

[ApiController, Route("api/reviews")]
public sealed class ReviewsController(AppDbContext db, IObjectStorageService storage, ILogger<ReviewsController> logger) : ControllerBase
{
 private Guid CustomerId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 private static bool ContainsProduct(CustomerOrder o, Guid productId) => (JsonSerializer.Deserialize<OrderItemDto[]>(o.ItemsJson) ?? []).Any(i=>i.ProductId==productId);
 private IQueryable<ProductReview> Reviews => db.Set<ProductReview>().AsNoTracking();
 private object View(ProductReview r, bool admin=false, string? productName=null) => new { r.Id,r.ProductId,r.CategoryId,IsImported=r.ImportedBy!=null,ProductName=productName,r.DisplayName,r.Rating,r.Title,r.Body,r.VerifiedPurchase,r.CreatedAt,
  Status=admin?r.Status:null, ModerationNote=admin?r.ModerationNote:null,r.Version,
  Media=(JsonSerializer.Deserialize<ReviewMedia[]>(r.MediaJson)??[]).Select(m=>new{m.Type,Url=storage.GetSignedUrl(m.Key,TimeSpan.FromHours(1))}) };

 [HttpGet("product/{productId:guid}")]
 public async Task<IActionResult> List(Guid productId, int page=1, string sort="newest", bool mediaOnly=false, CancellationToken ct=default)
 {
  var product=await db.Products.AsNoTracking().Where(p=>p.Id==productId).Select(p=>new{p.CategoryId,CategoryName=p.Category.Name}).FirstOrDefaultAsync(ct);
  if(product==null)return NotFound();
  var categoryProducts=db.Products.Where(p=>p.CategoryId==product.CategoryId).Select(p=>p.Id);
  var all=Reviews.Where(r=>(r.CategoryId==product.CategoryId || (r.ProductId.HasValue && categoryProducts.Contains(r.ProductId.Value))) && r.Status=="Published");
  var groups=await all.GroupBy(r=>r.Rating).Select(g=>new{Rating=g.Key,Count=g.Count()}).ToArrayAsync(ct);
  var total=groups.Sum(g=>g.Count); var filtered=mediaOnly?all.Where(r=>r.MediaJson!="[]"):all;
  var count=await filtered.CountAsync(ct); page=Math.Clamp(page,1,100000);
  var ordered=sort=="highest"?filtered.OrderByDescending(r=>r.Rating).ThenByDescending(r=>r.CreatedAt):sort=="lowest"?filtered.OrderBy(r=>r.Rating).ThenByDescending(r=>r.CreatedAt):filtered.OrderByDescending(r=>r.CreatedAt);
  var rows=await ordered.Skip((page-1)*10).Take(10).ToArrayAsync(ct);
  var productIds=rows.Select(r=>r.ProductId).Where(id=>id.HasValue).Select(id=>id!.Value).Distinct().ToArray();
  var productNames=await db.Products.Where(p=>productIds.Contains(p.Id)).ToDictionaryAsync(p=>p.Id,p=>p.Name,ct);
  return Ok(new{product.CategoryId,product.CategoryName,Total=total,Average=total==0?0:Math.Round(groups.Sum(g=>g.Rating*g.Count)/(double)total,1),Breakdown=Enumerable.Range(1,5).Reverse().Select(r=>new{Rating=r,Count=groups.FirstOrDefault(g=>g.Rating==r)?.Count??0}),Count=count,Page=page,Items=rows.Select(r=>View(r,productName:productNames.GetValueOrDefault(r.ProductId??Guid.Empty,"Category review")))});
 }
 [Authorize(Roles="Customer"),HttpGet("order/{orderId:guid}/submitted")]
 public async Task<IActionResult> Submitted(Guid orderId,CancellationToken ct)
 {
  var order=await db.CustomerOrders.AsNoTracking().FirstOrDefaultAsync(o=>o.Id==orderId && o.CustomerId==CustomerId,ct);
  if(order==null)return NotFound();
  var ids=(JsonSerializer.Deserialize<OrderItemDto[]>(order.ItemsJson)??[]).Where(i=>i.ProductId.HasValue).Select(i=>i.ProductId!.Value).Distinct().ToArray();
  return Ok(await Reviews.Where(r=>r.CustomerId==CustomerId && r.ProductId.HasValue && ids.Contains(r.ProductId.Value)).Select(r=>r.ProductId).ToArrayAsync(ct));
 }
 [Authorize(Roles="Customer"),HttpGet("product/{productId:guid}/eligibility")]
 public async Task<IActionResult> Eligibility(Guid productId,Guid orderId,CancellationToken ct)
 {
  var existing=await Reviews.FirstOrDefaultAsync(r=>r.ProductId==productId && r.CustomerId==CustomerId,ct);
  if(existing!=null)return Ok(new{Eligible=false,Message=$"Your review is {existing.Status.ToLowerInvariant()}.",Note=existing.ModerationNote,Review=View(existing,true)});
  var order=await db.CustomerOrders.AsNoTracking().FirstOrDefaultAsync(o=>o.Id==orderId && o.CustomerId==CustomerId && o.Status=="Delivered",ct);
  return Ok(new{Eligible=order!=null && ContainsProduct(order,productId),Message="You can review only items from your own delivered order.",Note=""});
 }
 [Authorize(Roles="Customer"),HttpPost("product/{productId:guid}"),RequestSizeLimit(46*1024*1024)]
 public async Task<IActionResult> Create(Guid productId,[FromForm] ReviewForm form,CancellationToken ct)
 {
  if(form.Rating<1 || form.Rating>5 || string.IsNullOrWhiteSpace(form.Body) || form.Body.Trim().Length<10 || form.Body.Length>2000 || !form.Consent)
   return BadRequest(new{error="Choose 1-5 stars, a review of 10-2,000 characters and permission to publish your review."});
  if(await Reviews.AnyAsync(r=>r.ProductId==productId && r.CustomerId==CustomerId,ct))return Conflict(new{error="You have already reviewed this product."});
  if(!await db.Products.AnyAsync(p=>p.Id==productId,ct))return NotFound();
  var order=await db.CustomerOrders.AsNoTracking().FirstOrDefaultAsync(o=>o.Id==form.OrderId && o.CustomerId==CustomerId && o.Status=="Delivered",ct);
  if(order==null || !ContainsProduct(order,productId))return BadRequest(new{error="You can review this product after your order is delivered."});
  if(form.Files.Count>5 || form.Files.Count(f=>f.ContentType.StartsWith("video/"))>1 || form.Files.Count(f=>f.ContentType.StartsWith("image/"))>4)
   return BadRequest(new{error="Add up to 4 photos and 1 video."});
  foreach(var file in form.Files){ReviewMediaValidator.Validate(file.ContentType,file.Length);await using var stream=file.OpenReadStream();var header=new byte[12];var read=await stream.ReadAtLeastAsync(header,12,false,ct);if(read<12 || !ReviewMediaValidator.Matches(file.ContentType,header))return BadRequest(new{error="One file does not match its media format."});}
  var fullName=await db.Users.Where(u=>u.Id==CustomerId).Select(u=>u.FullName).SingleAsync(ct);
  var firstName=fullName.Split((char[]?)null,StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()??"Customer";
  if(firstName.Length>80)firstName=firstName[..80];
  var review=new ProductReview{ProductId=productId,CustomerId=CustomerId,OrderId=order.Id,DisplayName=firstName,Rating=form.Rating,Title="",Body=form.Body.Trim(),VerifiedPurchase=!order.IsDemo};
  var media=new List<ReviewMedia>();
  try {
   foreach(var file in form.Files){var key=$"reviews/{review.Id}/{Guid.NewGuid()}{ReviewMediaValidator.Validate(file.ContentType,file.Length)}";await using var stream=file.OpenReadStream();await storage.UploadAsync(new(stream,key,file.ContentType,file.Length,StorageAccess.Private),ct);media.Add(new(key,file.ContentType.StartsWith("video/")?"video":"photo"));}
   review.MediaJson=JsonSerializer.Serialize(media);db.Add(review);await db.SaveChangesAsync(ct);
  }catch(Exception ex){foreach(var m in media){try{await storage.DeleteAsync(m.Key,StorageAccess.Private,CancellationToken.None);}catch(Exception cleanup){logger.LogWarning(cleanup,"Review upload cleanup failed for {Key}",m.Key);}}if(ex is DbUpdateException {InnerException:PostgresException{SqlState:"23505"}})return Conflict(new{error="You have already reviewed this product."});throw;}
  return Ok(new{review.Id,Message="Thank you. Your review has been submitted for moderation."});
 }
 [Authorize(Roles="Admin"),HttpPost("admin"),RequestSizeLimit(21*1024*1024)]
 public async Task<IActionResult> Import([FromForm] ImportedReviewForm form,CancellationToken ct)
 {
  if(form.Rating is <1 or >5 || form.DisplayName.Trim().Length is <1 or >80 || form.Body.Trim().Length is <10 or >2000 || !form.Consent)
   return BadRequest(new{error="Enter a customer name, 1-5 stars, 10-2,000 characters and confirm permission to publish."});
  if(!await db.Categories.AnyAsync(c=>c.Id==form.CategoryId && c.ParentId==null,ct))return BadRequest(new{error="Choose an existing category."});
  if(form.Files.Count>4 || form.Files.Any(f=>!f.ContentType.StartsWith("image/")))return BadRequest(new{error="Add up to 4 photos (JPG, PNG or WebP)."});
  foreach(var file in form.Files){ReviewMediaValidator.Validate(file.ContentType,file.Length);await using var stream=file.OpenReadStream();var header=new byte[12];var read=await stream.ReadAtLeastAsync(header,12,false,ct);if(read<12||!ReviewMediaValidator.Matches(file.ContentType,header))return BadRequest(new{error="A photo does not match its file format."});}
  var review=new ProductReview{CategoryId=form.CategoryId,ImportedBy=CustomerId,DisplayName=form.DisplayName.Trim(),Rating=form.Rating,Body=form.Body.Trim(),VerifiedPurchase=false};
  var media=new List<ReviewMedia>();
  try{
   foreach(var file in form.Files){var key=$"reviews/{review.Id}/{Guid.NewGuid()}{ReviewMediaValidator.Validate(file.ContentType,file.Length)}";await using var stream=file.OpenReadStream();await storage.UploadAsync(new(stream,key,file.ContentType,file.Length,StorageAccess.Private),ct);media.Add(new(key,"photo"));}
   review.MediaJson=JsonSerializer.Serialize(media);db.Add(review);await db.SaveChangesAsync(ct);
  }catch{foreach(var m in media){try{await storage.DeleteAsync(m.Key,StorageAccess.Private,CancellationToken.None);}catch(Exception ex){logger.LogWarning(ex,"Review upload cleanup failed");}}throw;}
  return Ok(new{review.Id});
 }
 [Authorize(Roles="Admin"),HttpGet("admin")]
 public async Task<IActionResult> Admin(string status="Pending",int page=1,CancellationToken ct=default)
 {
  var query=Reviews.Where(r=>r.Status==status);var count=await query.CountAsync(ct);
  var rows=await query.OrderByDescending(r=>r.CreatedAt).Skip((Math.Clamp(page,1,100000)-1)*20).Take(20).ToArrayAsync(ct);
  var ids=rows.Select(r=>r.ProductId).Where(id=>id.HasValue).Select(id=>id!.Value).Distinct().ToArray();var names=await db.Products.Where(p=>ids.Contains(p.Id)).ToDictionaryAsync(p=>p.Id,p=>p.Name,ct);
  var categoryIds=rows.Where(r=>r.CategoryId.HasValue).Select(r=>r.CategoryId!.Value).Distinct().ToArray();var categories=await db.Categories.Where(c=>categoryIds.Contains(c.Id)).ToDictionaryAsync(c=>c.Id,c=>c.Name,ct);
  return Ok(new{Count=count,Items=rows.Select(r=>new{Review=View(r,true),ProductName=r.CategoryId.HasValue?categories.GetValueOrDefault(r.CategoryId.Value,"Category review"):names.GetValueOrDefault(r.ProductId??Guid.Empty,"Product")})});
 }
 [Authorize(Roles="Admin"),HttpPut("admin/{id:guid}")]
 public async Task<IActionResult> Moderate(Guid id,ModerateReview form,CancellationToken ct)
 {
  if(form.Status is not ("Published" or "Rejected") || form.Note.Length>500 || (form.Status=="Rejected" && form.Note.Trim().Length<5))return BadRequest(new{error="Choose publish or reject, and give a reason when rejecting a review."});
  var review=await db.Set<ProductReview>().FirstOrDefaultAsync(r=>r.Id==id,ct);if(review==null)return NotFound();
  if(review.Version!=form.Version)return Conflict(new{error="This review changed. Refresh and try again."});
  review.Status=form.Status;review.ModerationNote=form.Note.Trim();review.ReviewedAt=DateTimeOffset.UtcNow;review.ReviewedBy=CustomerId;review.Version++;
  try{await db.SaveChangesAsync(ct);}catch(DbUpdateConcurrencyException){return Conflict(new{error="This review changed. Refresh and try again."});}
  return Ok(new{review.Status});
 }
}
public sealed record ReviewMedia(string Key,string Type);
public sealed record ModerateReview(string Status,string Note,int Version);
public sealed class ReviewForm
{
 public Guid OrderId{get;set;}
 public int Rating{get;set;}


 public string Body{get;set;}="";
 public bool Consent{get;set;}
 public List<IFormFile> Files{get;set;}=[];
}

public sealed class ImportedReviewForm { public Guid CategoryId{get;set;} public string DisplayName{get;set;}=""; public int Rating{get;set;} public string Body{get;set;}=""; public bool Consent{get;set;} public List<IFormFile> Files{get;set;}=[]; }
