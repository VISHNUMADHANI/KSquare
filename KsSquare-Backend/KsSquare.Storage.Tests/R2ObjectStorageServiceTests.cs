using Amazon.Runtime;
using Amazon.S3;
using KsSquare.Application.Abstractions.Storage;
using KsSquare.Infrastructure.Storage;
namespace KsSquare.Storage.Tests;

public sealed class R2ObjectStorageServiceTests
{
    private static readonly R2Options Options = new()
    {
        AccountId = "account", AccessKeyId = "access", SecretAccessKey = "secret",
        Endpoint = new Uri("https://account.r2.cloudflarestorage.com"),
        PublicBucketName = "public", PublicBaseUrl = new Uri("https://media.example.com"), PrivateBucketName = "private"
    };

    [Fact]
    public void GetPublicUrl_EncodesEachPathSegment()
    {
        using var client = Client();
        var service = new R2ObjectStorageService(client, Options);
        Assert.Equal("https://media.example.com/products/item%20one/image.webp", service.GetPublicUrl("products/item one/image.webp"));
    }

    [Fact]
    public void GetSignedUrl_UsesPrivateBucket()
    {
        using var client = Client();
        var service = new R2ObjectStorageService(client, Options);
        var url = service.GetSignedUrl("custom-requests/request/image.jpg", TimeSpan.FromMinutes(5));
        Assert.Contains("/private/custom-requests/request/image.jpg", url, StringComparison.Ordinal);
    }

    [Fact]
    public void GetSignedUrl_RejectsExcessiveValidity()
    {
        using var client = Client();
        var service = new R2ObjectStorageService(client, Options);
        Assert.Throws<StorageValidationException>(() => service.GetSignedUrl("private/image.jpg", TimeSpan.FromDays(2)));
    }

    private static AmazonS3Client Client() => new(new BasicAWSCredentials("access", "secret"), new AmazonS3Config { ServiceURL = Options.Endpoint.ToString(), ForcePathStyle = true, AuthenticationRegion = "auto" });
}
