namespace KsSquare.Application.Abstractions.Storage;

public static class ObjectKeyBuilder
{
    public static string ForProduct(Guid productId, string variant, string extension, Guid? objectId = null) => $"products/{productId:D}/{NormalizeSegment(variant)}/{objectId ?? Guid.NewGuid():D}{NormalizeExtension(extension)}";
    public static string ForCategory(Guid categoryId, string extension, Guid? objectId = null) => $"categories/{categoryId:D}/{objectId ?? Guid.NewGuid():D}{NormalizeExtension(extension)}";
    public static string ForProductOption(Guid optionId, string extension, Guid? objectId = null) => $"product-options/{optionId:D}/{objectId ?? Guid.NewGuid():D}{NormalizeExtension(extension)}";
    public static string ForCustomRequest(Guid requestId, string extension, Guid? objectId = null) => $"custom-requests/{requestId:D}/{objectId ?? Guid.NewGuid():D}{NormalizeExtension(extension)}";
    private static string NormalizeSegment(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Any(c => !char.IsLetterOrDigit(c) && c is not '-' and not '_')) throw new StorageValidationException("The object-key segment is invalid.");
        return normalized;
    }
    private static string NormalizeExtension(string extension)
    {
        var normalized = extension.Trim().ToLowerInvariant();
        return normalized.StartsWith('.') ? normalized : $".{normalized}";
    }
}
