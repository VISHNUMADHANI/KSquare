namespace KsSquare.Domain.Entities;

public sealed class ProductReview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ImportedBy { get; set; }
    public string DisplayName { get; set; } = "";
    public int Rating { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public bool VerifiedPurchase { get; set; }
    public string Status { get; set; } = "Pending";
    public string ModerationNote { get; set; } = "";
    public string MediaJson { get; set; } = "[]";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public int Version { get; set; }
}
