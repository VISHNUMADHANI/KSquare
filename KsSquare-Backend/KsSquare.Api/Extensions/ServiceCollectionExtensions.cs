using KsSquare.Infrastructure.Payments;
using KsSquare.Application.Abstractions.Persistence;
using KsSquare.Application.Features.Categories.Services;
using KsSquare.Application.Features.Products.Services;
using KsSquare.Application.Features.ProductOptions.Services;
using KsSquare.Application.Features.CustomJewelryRequests.Services;
using KsSquare.Application.Abstractions.Authentication;
using KsSquare.Application.Features.Authentication;
using KsSquare.Infrastructure.Authentication;
using KsSquare.Api.Services;
using KsSquare.Infrastructure.Repositories;
using KsSquare.Infrastructure.Storage;
using KsSquare.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace KsSquare.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKsSquareServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(BuildConnectionString(configuration)).UseSnakeCaseNamingConvention());
        services.AddScoped<KsSquare.Application.Features.Orders.IOrderService, OrderService>();
        services.AddSingleton(PayPalOptions.Read(configuration));
        services.AddHttpClient<KsSquare.Application.Features.Orders.IPayPalGateway,PayPalGateway>(client=>client.Timeout=TimeSpan.FromSeconds(30));
        services.AddScoped<PayPalCheckoutService>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICustomJewelryRequestRepository, CustomJewelryRequestRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductOptionService, ProductOptionService>();
        services.AddScoped<ICustomJewelryRequestService, CustomJewelryRequestService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordService, Pbkdf2PasswordService>();
        services.AddSingleton<IOtpSecurityService, OtpSecurityService>();
        services.AddSingleton<IOtpEmailSender, DevelopmentOtpEmailSender>();
        services.AddR2ObjectStorage(configuration);
        services.AddHttpClient("EtsyReviews", client => client.Timeout = TimeSpan.FromSeconds(12));
        services.AddSingleton<KsSquare.Api.EtsyReviewsService>();
        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var values = new Dictionary<string, string?> { ["DATABASE_HOST"] = configuration["DATABASE_HOST"], ["DATABASE_PORT"] = configuration["DATABASE_PORT"], ["DATABASE_NAME"] = configuration["DATABASE_NAME"], ["DATABASE_USER"] = configuration["DATABASE_USER"], ["DATABASE_PASSWORD"] = configuration["DATABASE_PASSWORD"] };
        var missingKeys = values.Where(entry => string.IsNullOrWhiteSpace(entry.Value)).Select(entry => entry.Key).ToArray();
        if (missingKeys.Length > 0) throw new InvalidOperationException($"Missing required database configuration: {string.Join(", ", missingKeys)}");
        if (!int.TryParse(values["DATABASE_PORT"], out var port)) throw new InvalidOperationException("DATABASE_PORT must be a valid integer.");
        return new NpgsqlConnectionStringBuilder { Host = values["DATABASE_HOST"], Port = port, Database = values["DATABASE_NAME"], Username = values["DATABASE_USER"], Password = values["DATABASE_PASSWORD"], IncludeErrorDetail = false }.ConnectionString;
    }
}
