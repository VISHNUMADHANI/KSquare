using Microsoft.Extensions.Configuration;
namespace KsSquare.Infrastructure.Storage;

public static class R2OptionsFactory
{
    public static R2Options Create(IConfiguration configuration)
    {
        string Required(string key) => !string.IsNullOrWhiteSpace(configuration[key]) ? configuration[key]!.Trim() : throw new InvalidOperationException($"Missing required R2 configuration: {key}");
        var accountId = Required("R2_ACCOUNT_ID");
        var endpoint = ParseHttps(Required("R2_ENDPOINT"), "R2_ENDPOINT");
        var publicBucket = Required("R2_PUBLIC_BUCKET_NAME");
        var privateBucket = Required("R2_PRIVATE_BUCKET_NAME");
        if (publicBucket.Equals(privateBucket, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("R2 public and private buckets must be different.");
        if (!endpoint.Host.Equals($"{accountId}.r2.cloudflarestorage.com", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("R2_ENDPOINT does not match R2_ACCOUNT_ID.");
        return new R2Options { AccountId = accountId, AccessKeyId = Required("R2_ACCESS_KEY_ID"), SecretAccessKey = Required("R2_SECRET_ACCESS_KEY"), Endpoint = endpoint, PublicBucketName = publicBucket, PublicBaseUrl = ParseHttps(Required("R2_PUBLIC_BASE_URL"), "R2_PUBLIC_BASE_URL"), PrivateBucketName = privateBucket };
    }
    private static Uri ParseHttps(string value, string key) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? uri : throw new InvalidOperationException($"{key} must be a valid HTTPS URL.");
}
