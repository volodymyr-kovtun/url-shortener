namespace UrlShortener.Infrastructure.AzureTableStorage.Configuration;

public sealed class AzureTableStorageSettings
{
    public string ConnectionString { get; init; } = default!;
}
