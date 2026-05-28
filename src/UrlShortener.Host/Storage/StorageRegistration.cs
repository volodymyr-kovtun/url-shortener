using UrlShortener.Infrastructure.AzureTableStorage;
using UrlShortener.Infrastructure.SqliteStorage;

namespace UrlShortener.Host.Storage;

public static class StorageRegistration
{
    private const string ProviderConfigKey = "Storage:Provider";
    private const string DefaultProvider = SqliteStorageRegistry.ProviderName;

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var providerName = configuration[ProviderConfigKey];
        if (string.IsNullOrWhiteSpace(providerName))
        {
            providerName = DefaultProvider;
        }

        return providerName switch
        {
            _ when providerName.Equals(SqliteStorageRegistry.ProviderName, StringComparison.OrdinalIgnoreCase) => services.AddSqliteStorage(configuration),
            _ when providerName.Equals(AzureTableStorageRegistry.ProviderName, StringComparison.OrdinalIgnoreCase) => services.AddAzureTableStorage(configuration),
            _ => throw new InvalidOperationException(
                $"Unknown storage provider '{providerName}'. Configure '{ProviderConfigKey}' to one of: " +
                $"'{SqliteStorageRegistry.ProviderName}', '{AzureTableStorageRegistry.ProviderName}'.")
        };
    }
}
