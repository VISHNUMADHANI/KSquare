using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace KsSquare.Persistence.DesignTime;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<Context.AppDbContext>
{
    public Context.AppDbContext CreateDbContext(string[] args)
    {
        Env.TraversePath().Load();

        var host = GetRequiredValue("DATABASE_HOST");
        var database = GetRequiredValue("DATABASE_NAME");
        var username = GetRequiredValue("DATABASE_USER");
        var password = GetRequiredValue("DATABASE_PASSWORD");

        if (!int.TryParse(GetRequiredValue("DATABASE_PORT"), out var port))
        {
            throw new InvalidOperationException("DATABASE_PORT must be a valid integer.");
        }

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            IncludeErrorDetail = false
        }.ConnectionString;

        var options = new DbContextOptionsBuilder<Context.AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new Context.AppDbContext(options);
    }

    private static string GetRequiredValue(string key) =>
        Environment.GetEnvironmentVariable(key) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing required database configuration: {key}");
}
