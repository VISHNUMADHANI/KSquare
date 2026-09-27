namespace KsSquare.Application.Features.Products.Services;

public sealed class CatalogValidationException(string message) : Exception(message);
public sealed class CatalogConflictException(string message) : Exception(message);
