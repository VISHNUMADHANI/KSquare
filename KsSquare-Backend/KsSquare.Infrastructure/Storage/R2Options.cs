namespace KsSquare.Infrastructure.Storage;
public sealed class R2Options
{
    public required string AccountId { get; init; }
    public required string AccessKeyId { get; init; }
    public required string SecretAccessKey { get; init; }
    public required Uri Endpoint { get; init; }
    public required string PublicBucketName { get; init; }
    public required Uri PublicBaseUrl { get; init; }
    public required string PrivateBucketName { get; init; }
}
