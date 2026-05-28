using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Infrastructure.AzureTableStorage.Configuration;

public sealed class AzureTableStorageSettings
{
    [Required(AllowEmptyStrings = false)]
    public required string ConnectionString { get; init; }
}
