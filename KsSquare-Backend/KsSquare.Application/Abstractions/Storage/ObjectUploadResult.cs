namespace KsSquare.Application.Abstractions.Storage;
public sealed record ObjectUploadResult(string ObjectKey, string BucketName, string ContentType, long ContentLength, StorageAccess Access, string? PublicUrl);
