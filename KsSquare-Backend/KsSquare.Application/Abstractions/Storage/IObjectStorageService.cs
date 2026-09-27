namespace KsSquare.Application.Abstractions.Storage;
public interface IObjectStorageService
{
    Task<ObjectUploadResult> UploadAsync(ObjectUploadRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string objectKey, StorageAccess access, CancellationToken cancellationToken = default);
    string GetPublicUrl(string objectKey);
    string GetSignedUrl(string objectKey, TimeSpan validity);
}
