using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class CustomJewelryRequest : AuditableEntity
{
    private CustomJewelryRequest() { }

    public CustomJewelryRequest(string requestNumber, string accessTokenHash, Guid categoryId, string customerName, string email, string phone, string description)
    {
        RequestNumber = requestNumber;
        AccessTokenHash = accessTokenHash;
        CategoryId = categoryId;
        CustomerName = customerName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        Description = description.Trim();
        Status = CustomJewelryRequestStatus.Submitted;
    }

    public string RequestNumber { get; private set; } = string.Empty;
    public string AccessTokenHash { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string CustomerName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CustomJewelryRequestStatus Status { get; private set; }
    public decimal? FixedPrice { get; private set; }
    public ICollection<ProductOption> Options { get; private set; } = new List<ProductOption>();
    public ICollection<CustomJewelryRequestMedia> Media { get; private set; } = new List<CustomJewelryRequestMedia>();

    public void MarkContacted()
    {
        if (Status == CustomJewelryRequestStatus.Contacted) return;
        if (Status != CustomJewelryRequestStatus.Submitted) throw new InvalidOperationException("Only a submitted request can be marked as contacted.");
        Status = CustomJewelryRequestStatus.Contacted;
    }

    public void SetQuote(decimal fixedPrice)
    {
        if (Status is CustomJewelryRequestStatus.Cancelled or CustomJewelryRequestStatus.Approved) throw new InvalidOperationException("This request can no longer be quoted.");
        FixedPrice = fixedPrice;
        Status = CustomJewelryRequestStatus.Quoted;
    }

    public void Cancel()
    {
        if (Status == CustomJewelryRequestStatus.Approved) throw new InvalidOperationException("An approved request cannot be cancelled.");
        Status = CustomJewelryRequestStatus.Cancelled;
    }
}
