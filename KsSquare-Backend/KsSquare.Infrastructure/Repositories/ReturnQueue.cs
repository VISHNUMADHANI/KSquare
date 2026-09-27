using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Products.Services;
using Microsoft.EntityFrameworkCore;
namespace KsSquare.Infrastructure.Repositories;
public sealed partial class OrderService
{
 public async Task<OrderPageDto> ListReturnsAsync(string? status,int page,CancellationToken ct)
 {
  if(!string.IsNullOrEmpty(status)&&!new[]{"Requested","Approved","Declined","Refunded"}.Contains(status))throw new CatalogValidationException("Select a valid return status.");
  page=Math.Max(1,page);const int size=20;
  var query=db.CustomerOrders.AsNoTracking().Where(o=>o.ReturnsJson!="[]");
  if(!string.IsNullOrEmpty(status)){var match=JsonSerializer.Serialize(new[]{new{Status=status}});query=query.Where(o=>EF.Functions.JsonContains(o.ReturnsJson,match));}
  var count=await query.CountAsync(ct);var rows=await query.OrderByDescending(o=>o.UpdatedAt??o.CreatedAt).ThenBy(o=>o.Id).Skip((page-1)*size).Take(size).ToArrayAsync(ct);
  return new(rows.Select(Map).ToArray(),count,page,size);
 }
}
