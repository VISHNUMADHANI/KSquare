namespace KsSquare.Application.Abstractions.Storage;
public sealed record ObjectUploadRequest(Stream Content, string ObjectKey, string ContentType, long ContentLength, StorageAccess Access);
