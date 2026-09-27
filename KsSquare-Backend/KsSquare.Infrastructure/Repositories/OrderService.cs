using KsSquare.Application.Abstractions.Storage;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Authentication;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace KsSquare.Infrastructure.Repositories;

public sealed partial class OrderService(AppDbContext db, IObjectStorageService? storage = null) : IOrderService
{
    public async Task<CheckoutDto> PreviewAsync(Guid customerId, CheckoutRequest request, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == customerId && u.Role == UserRole.Customer && u.IsEmailVerified, ct)
            ?? throw new AuthUnauthorizedException("Sign in with a verified customer account.");
        var lines = request.Lines ?? [];
        if (request.CustomRequestId.HasValue)
        {
            if (lines.Length != 0) throw new CatalogValidationException("Checkout a custom quote separately from your cart.");
            var quote = await db.CustomJewelryRequests.Include(q => q.Category).Include(q => q.Options)
                .SingleOrDefaultAsync(q => q.Id == request.CustomRequestId && q.Email == user.Email, ct)
                ?? throw new CatalogValidationException("This quote is unavailable for your account. Sign in using the email on the request.");
            if (quote.Status is not (CustomJewelryRequestStatus.Quoted or CustomJewelryRequestStatus.Approved) || quote.FixedPrice is null or <= 0)
                throw new CatalogValidationException("This request does not have an active quote.");
            if (await db.CustomerOrders.AnyAsync(o => o.CustomRequestId == quote.Id, ct)) throw new CatalogConflictException("This quote already has an order. Check My orders.");
            return new([new(null, quote.Category.Name + " custom design", 1, quote.FixedPrice.Value,
                new[] { "Quote: " + quote.RequestNumber }.Concat(quote.Options.Select(o => o.Name)).ToArray(), false, "Custom-made orders are final sale.")], quote.FixedPrice.Value, "USD");
        }
        if (lines.Length is < 1 or > 30) throw new CatalogValidationException("Choose between 1 and 30 cart items.");
        var ids = lines.Select(l => l.ProductId).Distinct().ToArray();
        var products = await db.Products.Include(p => p.Category).Include(p => p.Options).Include(p => p.ChainVariants).Include(p => p.BraceletVariants).Include(p => p.PendantVariants)
            .AsSplitQuery().Where(p => ids.Contains(p.Id) && p.Category.IsActive).ToDictionaryAsync(p => p.Id, ct);
        var items = lines.Select(line => OrderPricing.Price(products.GetValueOrDefault(line.ProductId)
            ?? throw new CatalogValidationException("A product was removed or is unavailable. Please update your cart."), line)).ToArray();
        if (lines.GroupBy(l => l.ProductId).Any(g => g.Sum(l => l.Quantity) > 20)) throw new CatalogValidationException("The maximum quantity is 20 per product.");
        return new(items, items.Sum(i => i.Total), "USD");
    }

    public async Task<OrderDto> PlaceDemoAsync(Guid customerId, PlaceDemoOrderRequest request, CancellationToken ct)
    {
        if (request.CheckoutKey == Guid.Empty) throw new CatalogValidationException("A checkout reference is required.");
        var address = request.DeliveryAddress?.Trim() ?? "";
        if (address.Length is < 20 or > 600) throw new CatalogValidationException("Enter your full delivery address, 20 to 600 characters.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { request.Checkout, request.ExpectedTotal, Address = address }))));
        var existing = await db.CustomerOrders.SingleOrDefaultAsync(o => o.CustomerId == customerId && o.CheckoutKey == request.CheckoutKey, ct);
        if (existing is not null) return SameAttempt(existing, hash);
        var preview = await PreviewAsync(customerId, request.Checkout, ct);
        if (request.ExpectedTotal != preview.Total) throw new CatalogConflictException("Prices have changed. Review the updated checkout before paying.");
        var user = await db.Users.SingleAsync(u => u.Id == customerId, ct);
        var order = new CustomerOrder(customerId, request.CheckoutKey, hash, user.FullName, user.Email, user.Phone, address,
            JsonSerializer.Serialize(preview.Items), preview.Total, request.Checkout.CustomRequestId);
        db.CustomerOrders.Add(order);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.ChangeTracker.Clear();
            existing = await db.CustomerOrders.SingleOrDefaultAsync(o => o.CustomerId == customerId && o.CheckoutKey == request.CheckoutKey, ct);
            if (existing is not null) return SameAttempt(existing, hash);
            throw new CatalogConflictException("This quote already has an order. Check My orders.");
        }
        return Map(order);
    }
    public async Task<OrderPageDto> ListAsync(Guid? customerId, string? status, int page, CancellationToken ct)
    {
        page = Math.Max(1, page); const int size = 20;
        var query = db.CustomerOrders.AsNoTracking().AsQueryable();
        if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        var count = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).Skip((page - 1) * size).Take(size).ToArrayAsync(ct);
        return new(rows.Select(Map).ToArray(), count, page, size);
    }
    public async Task<OrderDto?> GetAsync(Guid id, Guid? customerId, CancellationToken ct)
    {
        var order = await db.CustomerOrders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id && (!customerId.HasValue || o.CustomerId == customerId), ct);
        return order is null ? null : Map(order);
    }
    public async Task<OrderDto?> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await db.CustomerOrders.SingleOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return null;
        if (order.Version != request.Version) throw new CatalogConflictException("This order was updated. Refresh and try again.");
        (string Courier,string Number)? shipment=null;
        if(request.Status=="Shipped") shipment=CourierTracking.Validate(request.Courier,request.TrackingNumber);
        if(request.Status=="Delivered" && (request.DeliveredAt is null || request.DeliveredAt > DateTimeOffset.UtcNow || request.DeliveredAt < order.CreatedAt)) throw new CatalogValidationException("Enter the actual delivery date, between the order date and now.");
        try { order.ChangeStatus(request.Status); }
        catch (InvalidOperationException e) { throw new CatalogValidationException(e.Message); }
        if(shipment is {} tracking) order.SetTracking(tracking.Courier,tracking.Number);
        if(request.Status=="Delivered" && request.DeliveredAt is {} delivered) order.SetDeliveryDate(delivered);

        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new CatalogConflictException("This order was updated. Refresh and try again."); }
        return Map(order);
    }
    private OrderDto SameAttempt(CustomerOrder order, string hash) => order.RequestHash == hash ? Map(order) : throw new CatalogConflictException("This checkout reference was already used. Start a new checkout.");
    private OrderDto Map(CustomerOrder o)
    {
        var returns = JsonSerializer.Deserialize<ReturnRequestDto[]>(o.ReturnsJson) ?? [];
        var items = (JsonSerializer.Deserialize<OrderItemDto[]>(o.ItemsJson) ?? []).Select((item, index) => {
            var restriction = ReturnRules.Restriction(o, item, DateTimeOffset.UtcNow);
            return item with { CanRequestReturn = restriction is null && returns.Length == 0, ReturnRestriction = restriction, CanIncludeInReturn = ReturnRules.Restriction(o, item, returns.FirstOrDefault()?.RequestedAt ?? DateTimeOffset.UtcNow) is null };
        }).ToArray();
        return new(o.Id, o.OrderNumber, o.CustomerId, o.CustomerName, o.CustomerEmail, o.CustomerPhone,
        o.DeliveryAddress, items, o.Total, o.Currency, o.Status, o.PaymentStatus,
        o.IsDemo, o.PaymentReference, o.CreatedAt, o.Version, o.DeliveredAt, o.DeliveredAt?.AddDays(7),
            returns.Select(r => r with { Photos = r.Photos.Select(p => p with { Url = storage?.GetSignedUrl(p.ObjectKey, TimeSpan.FromHours(1)) }).ToArray() }).ToArray(), o.Courier, o.TrackingNumber, CourierTracking.Url(o.Courier,o.TrackingNumber));
    }
}
