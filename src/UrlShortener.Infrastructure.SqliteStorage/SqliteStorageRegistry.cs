using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core;
using UrlShortener.Infrastructure.SqliteStorage.Configuration;
using UrlShortener.Infrastructure.SqliteStorage.Persistence;

namespace UrlShortener.Infrastructure.SqliteStorage;

public static class SqliteStorageRegistry
{
    public const string ProviderName = "Sqlite";
    private const string ConfigSection = "Storage:Sqlite";

    public static IServiceCollection AddSqliteStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(ConfigSection).Get<SqliteStorageSettings>()
            ?? throw new InvalidOperationException($"'{ConfigSection}' configuration section is missing.");

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException($"'{ConfigSection}:ConnectionString' must be set.");
        }

        services.AddDbContext<UrlShortenerDbContext>(options => options.UseSqlite(settings.ConnectionString));
        services.AddScoped<IStorageProvider<UrlMapping, string>, SqliteStorageProvider>();
        services.AddHostedService<SqliteDatabaseInitializer>();

        return services;
    }
}
