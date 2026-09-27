namespace KsSquare.Application.Features.Categories.DTOs;

public sealed record CategoryDto(Guid Id, string Name, string Slug, Guid? ParentId, string? ParentName, bool IsActive, bool ShowInCustom, string? ImageUrl, int ProductCount, int ChildCount);
public sealed record SaveCategoryRequest(string Name, Guid? ParentId, bool IsActive, bool ShowInCustom);
