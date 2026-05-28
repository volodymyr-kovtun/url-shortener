using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core;
using UrlShortener.Infrastructure.AzureTableStorage.Configuration;

namespace UrlShortener.Infrastructure.AzureTableStorage;

public static class AzureTableStorageRegistry
{
    public const string ProviderName = "AzureTableStorage";
    private const string ConfigSection = "Storage:AzureTableStorage";

    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(ConfigSection).Get<AzureTableStorageSettings>()
            ?? throw new InvalidOperationException($"'{ConfigSection}' configuration section is missing.");

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException($"'{ConfigSection}:ConnectionString' must be set.");
        }

        services.AddScoped<IStorageProvider<UrlMapping, string>>(_ => new AzureTableStorageProvider(settings.ConnectionString));

        return services;
    }
}
