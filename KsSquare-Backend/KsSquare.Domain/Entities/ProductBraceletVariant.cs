using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class ProductBraceletVariant : AuditableEntity
{
    private ProductBraceletVariant() { }

    public ProductBraceletVariant(Guid productId, Guid? braceletSizeOptionId, Guid? braceletStoneSizeOptionId, decimal originalPrice, bool isAvailable)
    {
        ProductId = productId;
        BraceletSizeOptionId = braceletSizeOptionId;
        BraceletStoneSizeOptionId = braceletStoneSizeOptionId;
        CombinationKey = $"{braceletSizeOptionId?.ToString("N") ?? "none"}:{braceletStoneSizeOptionId?.ToString("N") ?? "none"}";
        Update(originalPrice, isAvailable);
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? BraceletSizeOptionId { get; private set; }
    public ProductOption? BraceletSizeOption { get; private set; }
    public Guid? BraceletStoneSizeOptionId { get; private set; }
    public ProductOption? BraceletStoneSizeOption { get; private set; }
    public string CombinationKey { get; private set; } = string.Empty;
    public decimal OriginalPrice { get; private set; }
    public bool IsAvailable { get; private set; }

    public void Update(decimal originalPrice, bool isAvailable)
    {
        OriginalPrice = originalPrice;
        IsAvailable = isAvailable;
    }
}
