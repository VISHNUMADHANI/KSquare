using KsSquare.Domain.Entities;
using KsSquare.Application.Features.Products.Services;
namespace KsSquare.Application.Features.Orders;

public sealed record ReturnItemSelection(int ItemIndex, int Quantity);
public sealed record ReturnFileInput(Stream Content, string ContentType, long ContentLength);
public sealed record ReturnPhoto(string ObjectKey, string ContentType, string? Url = null);
public sealed record ReturnEvent(string Status, DateTimeOffset At, Guid? AdminId, string Note, decimal? RefundAmount, int[]? ItemIndices = null, ReturnItemSelection[]? Items = null);
public sealed record ReturnRequestDto(Guid Id, int ItemIndex, string Reason, string Description, DateTimeOffset RequestedAt,
    string Status, string? AdminNote, decimal? RefundAmount, string? RefundReference, ReturnPhoto[] Photos, ReturnEvent[] History, int[]? ItemIndices = null, int[]? CustomerItemIndices = null, ReturnItemSelection[]? Items = null, ReturnItemSelection[]? CustomerItems = null);
public sealed record CreateReturnRequest(int ItemIndex, string Reason, string Description, bool ConfirmCondition, int Version, int[]? ItemIndices = null, ReturnItemSelection[]? Items = null);
public sealed record ReviewReturnRequest(string Action, string Note, decimal? RefundAmount, bool Inspected, int Version, bool ContactConfirmed = false, int[]? ItemIndices = null, ReturnItemSelection[]? Items = null);
public static class ReturnRules
{
    public static ReturnItemSelection[] SelectedLines(ReturnRequestDto entry, OrderItemDto[] items) => entry.Items ?? SelectedItems(entry).Select(i => new ReturnItemSelection(i, items[i].Quantity)).ToArray();
    public static ReturnItemSelection[] ValidateLines(ReturnItemSelection[] lines, OrderItemDto[] items, CustomerOrder order, DateTimeOffset at)
    {
        ValidateItems(lines.Select(l => l.ItemIndex).ToArray(), items, order, at);
        if (lines.Any(l => l.Quantity < 1 || l.Quantity > items[l.ItemIndex].Quantity))
            throw new CatalogValidationException("Return quantity must be between one and the purchased quantity.");
        return lines.OrderBy(l => l.ItemIndex).ToArray();
    }
    public static decimal ReturnTotal(ReturnItemSelection[] lines, OrderItemDto[] items) => lines.Sum(l => items[l.ItemIndex].UnitPrice * l.Quantity);
    public static int[] SelectedItems(ReturnRequestDto entry) => entry.ItemIndices ?? [entry.ItemIndex];
    public static int[] ValidateItems(int[] indices, OrderItemDto[] items, CustomerOrder order, DateTimeOffset at)
    {
        if (indices.Length == 0 || indices.Distinct().Count() != indices.Length || indices.Any(i => i < 0 || i >= items.Length))
            throw new CatalogValidationException("Select one or more different items from this order.");
        foreach (var index in indices)
            if (Restriction(order, items[index], at) is string restriction) throw new CatalogValidationException(restriction);
        return indices.Order().ToArray();
    }
    public static readonly string[] Reasons = ["Change of mind", "Size or fit", "Damaged or defective", "Incorrect item"];
    public static string? Restriction(CustomerOrder order, OrderItemDto item, DateTimeOffset now)
    {
        if (order.CustomRequestId.HasValue) return "Custom-made orders are final sale.";
        if (item.Details.Any(detail => detail.TrimStart().StartsWith("Name:", StringComparison.OrdinalIgnoreCase))) return "Personalized name and letter pieces are final sale.";
        if (item.IsReturnable == false && item.ReturnRestriction != "This item is final sale.") return item.ReturnRestriction ?? "Contact us to confirm return eligibility for this item.";
        if (order.Status != "Delivered") return "Returns can be requested after delivery.";
        // Legacy delivered orders may request review; do not invent a delivery date.
        if (!order.DeliveredAt.HasValue) return null;
        if (now < order.DeliveredAt || now > order.DeliveredAt.Value.AddDays(7)) return "The 7-day return request window has closed.";
        return null;
    }
    public static ReturnRequestDto Review(ReturnRequestDto current, ReviewReturnRequest request, decimal maximum, bool isDemo, Guid adminId, DateTimeOffset now)
    {
        var note = request.Note?.Trim() ?? "";
        if (note.Length is < 5 or > 2000) throw new CatalogValidationException("Enter a customer-facing note of 5 to 2000 characters.");
        string status; decimal? amount = null; string? reference = null;
        switch (request.Action)
        {
            case "UpdateItems" when current.Status is "Requested" or "Approved":
                status = "Requested"; break;
            case "Accept" when current.Status is "Requested" or "Approved":
                if (request.RefundAmount is not decimal accepted || accepted < 0 || accepted > maximum || decimal.Round(accepted, 2) != accepted)
                    throw new CatalogValidationException("Enter a refund amount between zero and the selected items total, with at most two decimals.");
                amount = accepted; status = "Approved"; break;
            case "Approve" when current.Status == "Requested": status = "Approved"; break;
            case "Decline" when current.Status is "Requested" or "Approved": status = "Declined"; break;
            case "AgreeRefund" when current.Status == "Approved":
                if (!request.ContactConfirmed) throw new CatalogValidationException("Confirm that you discussed and agreed the amount with the customer.");
                if (request.RefundAmount is not decimal agreed || agreed < 0 || agreed > maximum || decimal.Round(agreed, 2) != agreed)
                    throw new CatalogValidationException("Enter an agreed amount between zero and the item total, with at most two decimals.");
                amount = agreed; status = "Approved"; break;
            case "Refund" when current.Status == "Approved":
                if (current.RefundAmount is null || request.RefundAmount != current.RefundAmount) throw new CatalogValidationException("Agree the refund amount with the customer before recording the refund.");
                if (!request.Inspected) throw new CatalogValidationException("Confirm that the returned item has been received and inspected.");
                if (!isDemo) throw new CatalogValidationException("Live refunds are not connected. No refund has been issued.");
                if (request.RefundAmount is not decimal value || value < 0 || value > maximum || decimal.Round(value, 2) != value)
                    throw new CatalogValidationException("Choose a refund amount between zero and the returned line total, with at most two decimals.");
                amount = value; status = "Refunded"; reference = "DEMO-REFUND-" + current.Id.ToString("N"); break;
            default: throw new CatalogConflictException("This return has changed or the requested action is not available. Reopen the order.");
        }
        return current with { Status = status, AdminNote = note, RefundAmount = amount, RefundReference = reference,
            History = [..current.History, new(status, now, adminId, note, amount, request.Action is "UpdateItems" or "Accept" ? request.ItemIndices : SelectedItems(current), request.Action is "UpdateItems" or "Accept" ? request.Items : current.Items)] };
    }
}
