using Amazon.Runtime;
using Amazon.S3;
using KsSquare.Application.Abstractions.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace KsSquare.Infrastructure.Storage;

public static class StorageServiceCollectionExtensions
{
    public static IServiceCollection AddR2ObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var options = R2OptionsFactory.Create(configuration);
        var clientConfig = new AmazonS3Config { ServiceURL = options.Endpoint.ToString(), ForcePathStyle = true, AuthenticationRegion = "auto" };
        services.AddSingleton(options);
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey), clientConfig));
        services.AddSingleton<IObjectStorageService, R2ObjectStorageService>();
        return services;
    }
}
