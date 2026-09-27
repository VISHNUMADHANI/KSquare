using KsSquare.Domain.Common;

namespace KsSquare.Domain.Entities;

public sealed class Product : AuditableEntity
{
    private Product() { }

    public Product(string name, string sku, string description, decimal originalPrice, decimal discountPercentage, Guid categoryId, bool isAvailable, bool isActive, bool supportsNamePersonalization = false, decimal nameFixedPrice = 0, decimal namePricePerLetter = 0, bool showInCustom = false, bool isFinalSale = false)
    {
        Update(name, sku, description, originalPrice, discountPercentage, categoryId, isAvailable, isActive, supportsNamePersonalization, nameFixedPrice, namePricePerLetter, showInCustom, isFinalSale);
    }

    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal OriginalPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal FinalPrice => decimal.Round(OriginalPrice * (100m - DiscountPercentage) / 100m, 2, MidpointRounding.AwayFromZero);
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    public bool IsActive { get; private set; }
    public bool SupportsNamePersonalization { get; private set; }
    public int IncludedNameLetters { get; private set; } = 1;
    public void SetIncludedNameLetters(int count) { if(count is < 1 or > 8) throw new ArgumentOutOfRangeException(nameof(count)); IncludedNameLetters=count; if(SupportsNamePersonalization) OriginalPrice=NameFixedPrice; }
    public decimal NameFixedPrice { get; private set; }
    public decimal NamePricePerLetter { get; private set; }
    public bool IsFinalSale { get; private set; }
    public bool ShowInCustom { get; private set; }
    public ICollection<ProductPendantVariant> PendantVariants { get; private set; } = new List<ProductPendantVariant>();
    public void SetStartingPrice(decimal price) { OriginalPrice = price; }
    public ICollection<ProductMedia> Media { get; private set; } = new List<ProductMedia>();
    public ICollection<ProductOption> Options { get; private set; } = new List<ProductOption>();
    public ICollection<ProductChainVariant> ChainVariants { get; private set; } = new List<ProductChainVariant>();
    public ICollection<ProductBraceletVariant> BraceletVariants { get; private set; } = new List<ProductBraceletVariant>();
    public ICollection<Category> Subcategories { get; private set; } = new List<Category>();

    public void Update(string name, string sku, string description, decimal originalPrice, decimal discountPercentage, Guid categoryId, bool isAvailable, bool isActive, bool supportsNamePersonalization, decimal nameFixedPrice, decimal namePricePerLetter, bool showInCustom = false, bool isFinalSale = false)
    {
        Name = name.Trim();
        Sku = sku.Trim().ToUpperInvariant();
        Description = description.Trim();
        OriginalPrice = originalPrice;
        DiscountPercentage = discountPercentage;
        CategoryId = categoryId;
        IsAvailable = isAvailable;
        IsActive = isActive;
        SupportsNamePersonalization = supportsNamePersonalization;
        NameFixedPrice = supportsNamePersonalization ? nameFixedPrice : 0;
        NamePricePerLetter = supportsNamePersonalization ? namePricePerLetter : 0;
        ShowInCustom = showInCustom;
        IsFinalSale = isFinalSale;
    }
}
