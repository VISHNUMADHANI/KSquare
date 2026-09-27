namespace KsSquare.Application.Features.Products.DTOs;

public sealed record SaveProductRequest(
    string Name, string? Sku, string Description, decimal OriginalPrice,
    decimal DiscountPercentage, Guid CategoryId, bool IsAvailable, bool IsActive, bool SupportsNamePersonalization, decimal NameFixedPrice, decimal NamePricePerLetter,
    IReadOnlyCollection<Guid>? SubcategoryIds, IReadOnlyCollection<Guid>? OptionIds,
    IReadOnlyCollection<SaveProductChainVariantRequest>? ChainVariants,
    IReadOnlyCollection<SaveProductBraceletVariantRequest>? BraceletVariants,
    bool ShowInCustom = false, bool IsFinalSale = false, IReadOnlyCollection<SaveProductPendantVariantRequest>? PendantVariants = null, int IncludedNameLetters = 1);

public sealed record SaveProductChainVariantRequest(
    Guid ChainSizeOptionId, Guid? ChainWidthOptionId, Guid? ChainDiamondSizeOptionId,
    decimal OriginalPrice, bool IsAvailable);

public sealed record SaveProductBraceletVariantRequest(
    Guid? BraceletSizeOptionId, Guid? BraceletStoneSizeOptionId,
    decimal OriginalPrice, bool IsAvailable);

public sealed record SaveProductPendantVariantRequest(Guid PendantSizeOptionId, decimal OriginalPrice, bool IsAvailable);
