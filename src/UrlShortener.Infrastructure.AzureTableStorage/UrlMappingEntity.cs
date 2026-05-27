using Azure;
using Azure.Data.Tables;

namespace UrlShortener.Infrastructure.AzureTableStorage;

public sealed class UrlMappingEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string OriginalUrl { get; set; } = default!;

    public string ShortUrl { get; set; } = default!;
}
