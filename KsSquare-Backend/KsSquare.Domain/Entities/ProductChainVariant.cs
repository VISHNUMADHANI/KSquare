using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class ProductChainVariant : AuditableEntity
{
    private ProductChainVariant() { }

    public ProductChainVariant(Guid productId, Guid chainSizeOptionId, Guid? chainWidthOptionId, Guid? chainDiamondSizeOptionId, decimal originalPrice, bool isAvailable)
    {
        ProductId = productId;
        ChainSizeOptionId = chainSizeOptionId;
        ChainWidthOptionId = chainWidthOptionId;
        ChainDiamondSizeOptionId = chainDiamondSizeOptionId;
        CombinationKey = $"{chainSizeOptionId:N}:{chainWidthOptionId?.ToString("N") ?? "none"}:{chainDiamondSizeOptionId?.ToString("N") ?? "none"}";
        Update(originalPrice, isAvailable);
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid ChainSizeOptionId { get; private set; }
    public ProductOption ChainSizeOption { get; private set; } = null!;
    public Guid? ChainWidthOptionId { get; private set; }
    public ProductOption? ChainWidthOption { get; private set; }
    public Guid? ChainDiamondSizeOptionId { get; private set; }
    public ProductOption? ChainDiamondSizeOption { get; private set; }
    public string CombinationKey { get; private set; } = string.Empty;
    public decimal OriginalPrice { get; private set; }
    public bool IsAvailable { get; private set; }

    public void Update(decimal originalPrice, bool isAvailable)
    {
        OriginalPrice = originalPrice;
        IsAvailable = isAvailable;
    }
}
