using System.Text.Json;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Application.Features.Orders;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace KsSquare.Infrastructure.Repositories;
public sealed partial class OrderService
{
    public async Task<OrderDto?> RequestReturnAsync(Guid customerId, Guid orderId, CreateReturnRequest request, ReturnFileInput[] files, CancellationToken ct)
    {
        var order = await db.CustomerOrders.SingleOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId, ct);
        if (order is null) return null;
        CheckVersion(order, request.Version);
        var items = JsonSerializer.Deserialize<OrderItemDto[]>(order.ItemsJson) ?? [];
        var selectedLines = request.Items;
        if (selectedLines is null) {
            var indices = ReturnRules.ValidateItems(request.ItemIndices ?? [request.ItemIndex], items, order, DateTimeOffset.UtcNow);
            selectedLines = indices.Select(i => new ReturnItemSelection(i, items[i].Quantity)).ToArray();
        }
        selectedLines = ReturnRules.ValidateLines(selectedLines, items, order, DateTimeOffset.UtcNow);
        var selected = selectedLines.Select(l => l.ItemIndex).ToArray();
        var returns = JsonSerializer.Deserialize<ReturnRequestDto[]>(order.ReturnsJson) ?? [];
        if (returns.Length > 0) throw new CatalogConflictException("A return request already exists for this order. Please follow the existing request.");
        var description = request.Description?.Trim() ?? "";
        if (!ReturnRules.Reasons.Contains(request.Reason) || description.Length is < 10 or > 2000 || !request.ConfirmCondition)
            throw new CatalogValidationException("Select a reason, describe it in 10 to 2000 characters, and confirm the return conditions.");
        if (files.Length > 4) throw new CatalogValidationException("Upload at most 4 photos.");
        if (request.Reason is "Damaged or defective" or "Incorrect item" && files.Length == 0) throw new CatalogValidationException("Include a clear photo of the issue.");
        foreach (var file in files) StorageFileValidator.ValidateAndGetExtension(file.ContentType, file.ContentLength);
        if (files.Length > 0 && storage is null) throw new StorageUnavailableException("Photo uploads are unavailable. Please try again.", new InvalidOperationException("Storage is not configured."));
        var id = Guid.NewGuid(); var photos = new List<ReturnPhoto>();
        try
        {
            foreach (var file in files)
            {
                var extension = StorageFileValidator.ValidateAndGetExtension(file.ContentType, file.ContentLength);
                var key = $"returns/{order.Id}/{id}/{Guid.NewGuid():N}{extension}";
                var uploaded = await storage!.UploadAsync(new(file.Content, key, file.ContentType, file.ContentLength, StorageAccess.Private), ct);
                photos.Add(new(uploaded.ObjectKey, uploaded.ContentType));
            }
            var now = DateTimeOffset.UtcNow;
            var entry = new ReturnRequestDto(id, selected[0], request.Reason, description, now, "Requested", null, null, null, photos.ToArray(), [new("Requested", now, null, "Request submitted", null, selected, selectedLines)], selected, selected, selectedLines, selectedLines);
            order.SaveReturns(JsonSerializer.Serialize(returns.Append(entry)));
            await SaveReturnChanges(ct);
        }
        catch
        {
            foreach (var photo in photos) try { await storage!.DeleteAsync(photo.ObjectKey, StorageAccess.Private, CancellationToken.None); } catch { }
            throw;
        }
        return Map(order);
    }
    public async Task<OrderDto?> ReviewReturnAsync(Guid adminId, Guid orderId, Guid returnId, ReviewReturnRequest request, CancellationToken ct)
    {
        var order = await db.CustomerOrders.SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null) return null;
        CheckVersion(order, request.Version);
        var returns = JsonSerializer.Deserialize<ReturnRequestDto[]>(order.ReturnsJson) ?? [];
        var index = Array.FindIndex(returns, r => r.Id == returnId);
        if (index < 0) return null;
        var items = JsonSerializer.Deserialize<OrderItemDto[]>(order.ItemsJson) ?? [];
        var current = returns[index];
        var selectedLines = ReturnRules.SelectedLines(current, items);
        var selected = selectedLines.Select(l => l.ItemIndex).ToArray();
        if (request.Action is "UpdateItems" or "Accept")
        {
            selectedLines = ReturnRules.ValidateLines(request.Items ?? [], items, order, current.RequestedAt);
            selected = selectedLines.Select(l => l.ItemIndex).ToArray();
            if (returns.Where(r => r.Id != current.Id).Any(r => ReturnRules.SelectedItems(r).Intersect(selected).Any()))
                throw new CatalogConflictException("An item is already part of another return request.");
            request = request with { ItemIndices = selected, Items = selectedLines };
        }
        var reviewed = ReturnRules.Review(current, request, ReturnRules.ReturnTotal(selectedLines, items), order.IsDemo, adminId, DateTimeOffset.UtcNow);
        returns[index] = reviewed with { ItemIndex = selected[0], ItemIndices = selected, CustomerItemIndices = current.CustomerItemIndices ?? ReturnRules.SelectedItems(current), Items = selectedLines, CustomerItems = current.CustomerItems ?? ReturnRules.SelectedLines(current, items) };
        order.SaveReturns(JsonSerializer.Serialize(returns));
        await SaveReturnChanges(ct);
        return Map(order);
    }
    private static void CheckVersion(CustomerOrder order, int version)
    { if (order.Version != version) throw new CatalogConflictException("This order was updated. Reopen it to see the latest information."); }
    private async Task SaveReturnChanges(CancellationToken ct)
    { try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { throw new CatalogConflictException("This order was updated. Reopen it to see the latest information."); } }
}
