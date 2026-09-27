namespace KsSquare.Domain.Entities;
public static class CatalogCategoryNames
{
 public static bool IsPendant(string name) => name.ToLowerInvariant().Split([' ', '-', '_'],StringSplitOptions.RemoveEmptyEntries).Any(word => word is "pendant" or "pendants" or "pendate" or "pendates" or "pandate" or "pandates" or "pandant" or "pandants");
}
