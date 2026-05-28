using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        services
            .AddOptions<SqliteStorageSettings>()
            .Bind(configuration.GetSection(ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<UrlShortenerDbContext>((sp, options) =>
        {
            var settings = sp.GetRequiredService<IOptions<SqliteStorageSettings>>().Value;
            options.UseSqlite(settings.ConnectionString);
        });

        services.AddScoped<IStorageProvider<UrlMapping, string>, SqliteStorageProvider>();
        services.AddHostedService<SqliteDatabaseInitializer>();

        return services;
    }
}
