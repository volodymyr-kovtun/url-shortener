using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UrlShortener.Core;
using UrlShortener.Infrastructure.AzureTableStorage.Configuration;

namespace UrlShortener.Infrastructure.AzureTableStorage;

public static class AzureTableStorageRegistry
{
    public const string ProviderName = "AzureTableStorage";
    private const string ConfigSection = "Storage:AzureTableStorage";

    public static IServiceCollection AddAzureTableStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<AzureTableStorageSettings>()
            .Bind(configuration.GetSection(ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IStorageProvider<UrlMapping, string>>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureTableStorageSettings>>().Value;
            return new AzureTableStorageProvider(settings.ConnectionString);
        });

        return services;
    }
}
