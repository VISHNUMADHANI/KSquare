namespace KsSquare.Application.Features.ProductOptions.DTOs;

public sealed record ProductOptionDto(Guid Id, string Type, string Name, string? ImageUrl, int DisplayOrder, bool IsActive, int ProductCount, string? SizeGuideUrl = null);
public sealed record SaveProductOptionRequest(string Type, string Name, int DisplayOrder, bool IsActive);
