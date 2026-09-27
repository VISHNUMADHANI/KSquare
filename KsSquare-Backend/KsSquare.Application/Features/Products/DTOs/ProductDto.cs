namespace KsSquare.Application.Features.Products.DTOs;

public sealed record ProductMediaDto(Guid Id, string ObjectKey, string Url, string ContentType, long ContentLength, string AltText, int DisplayOrder);
public sealed record ProductOptionSummaryDto(Guid Id, string Type, string Name, string? ImageUrl, int DisplayOrder);
public sealed record ProductSubcategoryDto(Guid Id, string Name, string Slug);
public sealed record ProductChainVariantDto(
    Guid Id, Guid ChainSizeOptionId, Guid? ChainWidthOptionId, Guid? ChainDiamondSizeOptionId,
    decimal OriginalPrice, decimal FinalPrice, bool IsAvailable);
public sealed record ProductBraceletVariantDto(
    Guid Id, Guid? BraceletSizeOptionId, Guid? BraceletStoneSizeOptionId,
    decimal OriginalPrice, decimal FinalPrice, bool IsAvailable);

public sealed record ProductDto(
    Guid Id, string Name, string Sku, string Description, decimal OriginalPrice,
    decimal DiscountPercentage, decimal FinalPrice, string Currency, Guid CategoryId,
    string CategoryName, IReadOnlyList<ProductSubcategoryDto> Subcategories, bool IsAvailable, bool IsActive, bool SupportsNamePersonalization, decimal NameFixedPrice, decimal NamePricePerLetter,
    IReadOnlyList<ProductMediaDto> Media, IReadOnlyList<ProductOptionSummaryDto> Options,
    IReadOnlyList<ProductChainVariantDto> ChainVariants, IReadOnlyList<ProductBraceletVariantDto> BraceletVariants, bool ShowInCustom,
    DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, bool IsFinalSale = false, IReadOnlyList<ProductPendantVariantDto>? PendantVariants = null, int IncludedNameLetters = 1);

public sealed record ProductPendantVariantDto(Guid Id, Guid PendantSizeOptionId, decimal OriginalPrice, decimal FinalPrice, bool IsAvailable);
