namespace KsSquare.Application.Abstractions.Storage;

public static class StorageFileValidator
{
    public const long MaximumFileSizeBytes = 10 * 1024 * 1024;
    public const int MaximumCustomRequestFiles = 6;
    private static readonly IReadOnlyDictionary<string, string> Extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    { ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp" };

    public static string ValidateAndGetExtension(string contentType, long contentLength)
    {
        if (contentLength <= 0) throw new StorageValidationException("The file is empty.");
        if (contentLength > MaximumFileSizeBytes) throw new StorageValidationException("The file exceeds the 10 MB limit.");
        var normalized = contentType.Split(';', 2)[0].Trim();
        return Extensions.TryGetValue(normalized, out var extension) ? extension : throw new StorageValidationException("Only JPEG, PNG, and WebP images are allowed.");
    }
}
