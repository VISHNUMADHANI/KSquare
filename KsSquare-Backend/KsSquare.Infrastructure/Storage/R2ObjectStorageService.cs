using Amazon.S3;
using Amazon.S3.Model;
using KsSquare.Application.Abstractions.Storage;
namespace KsSquare.Infrastructure.Storage;

public sealed class R2ObjectStorageService(IAmazonS3 client, R2Options options) : IObjectStorageService
{
    public async Task<ObjectUploadResult> UploadAsync(ObjectUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request.Content);
        ValidateKey(request.ObjectKey);
        if (request.ObjectKey.StartsWith("reviews/", StringComparison.Ordinal)) ReviewMediaValidator.Validate(request.ContentType, request.ContentLength);
        else if (request.ObjectKey.StartsWith("products/", StringComparison.Ordinal)) ProductMediaValidator.Validate(request.ContentType, request.ContentLength);
        else StorageFileValidator.ValidateAndGetExtension(request.ContentType, request.ContentLength);
        if (!request.Content.CanRead) throw new StorageValidationException("The upload stream is not readable.");
        var bucket = Bucket(request.Access);
        var upload = new PutObjectRequest
        {
            BucketName = bucket,
            Key = request.ObjectKey,
            InputStream = request.Content,
            ContentType = request.ContentType.Split(';', 2)[0].Trim(),
            AutoCloseStream = false,
            UseChunkEncoding = false,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true
        };
        upload.Headers.ContentLength = request.ContentLength;
        try { await client.PutObjectAsync(upload, cancellationToken); }
        catch (Exception exception) when (exception is HttpRequestException or AmazonS3Exception)
        {
            throw new StorageUnavailableException("Media storage is currently unavailable. Please try the upload again.", exception);
        }
        return new ObjectUploadResult(request.ObjectKey, bucket, upload.ContentType, request.ContentLength, request.Access, request.Access == StorageAccess.Public ? GetPublicUrl(request.ObjectKey) : null);
    }
    public async Task DeleteAsync(string objectKey, StorageAccess access, CancellationToken cancellationToken = default)
    {
        ValidateKey(objectKey);
        try { await client.DeleteObjectAsync(Bucket(access), objectKey, cancellationToken); }
        catch (Exception exception) when (exception is HttpRequestException or AmazonS3Exception)
        {
            throw new StorageUnavailableException("Media storage is currently unavailable. Please try again.", exception);
        }
    }
    public string GetPublicUrl(string objectKey)
    {
        ValidateKey(objectKey);
        return $"{options.PublicBaseUrl.ToString().TrimEnd('/')}/{string.Join('/', objectKey.Split('/').Select(Uri.EscapeDataString))}";
    }
    public string GetSignedUrl(string objectKey, TimeSpan validity)
    {
        ValidateKey(objectKey);
        if (validity <= TimeSpan.Zero || validity > TimeSpan.FromHours(24)) throw new StorageValidationException("Signed URL validity must be between 1 second and 24 hours.");
        return client.GetPreSignedURL(new GetPreSignedUrlRequest { BucketName = options.PrivateBucketName, Key = objectKey, Verb = HttpVerb.GET, Expires = DateTime.UtcNow.Add(validity) });
    }
    private string Bucket(StorageAccess access) => access switch { StorageAccess.Public => options.PublicBucketName, StorageAccess.Private => options.PrivateBucketName, _ => throw new ArgumentOutOfRangeException(nameof(access)) };
    private static void ValidateKey(string key) { if (string.IsNullOrWhiteSpace(key) || key.StartsWith('/') || key.Contains("..", StringComparison.Ordinal)) throw new StorageValidationException("The object key is invalid."); }
}
