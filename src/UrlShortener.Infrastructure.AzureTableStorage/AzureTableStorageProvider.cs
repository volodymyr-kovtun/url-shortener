using Azure.Data.Tables;
using UrlShortener.Core;

namespace UrlShortener.Infrastructure.AzureTableStorage;

public sealed class AzureTableStorageProvider(string connectionString) : IStorageProvider<UrlMapping, string>
{
    private const string UrlMappingTableName = "UrlMapping";

    private readonly TableClient _tableClient = new(connectionString, UrlMappingTableName);

    public async Task AddAsync(UrlMapping entity, CancellationToken ct = default)
    {
        var dbEntity = new UrlMappingEntity
        {
            OriginalUrl = entity.OriginalUrl,
            RowKey = entity.ShortUrl,
            ShortUrl = entity.ShortUrl,
            PartitionKey = PartitionKeyHasher.Hash(entity.ShortUrl),
            Timestamp = entity.CreatedAt,
        };

        await _tableClient.CreateIfNotExistsAsync(ct);
        await _tableClient.AddEntityAsync(dbEntity, ct);
    }

    public async Task<UrlMapping?> GetAsync(string key, CancellationToken ct = default)
    {
        var partitionKey = PartitionKeyHasher.Hash(key);

        var dbEntity = await _tableClient.GetEntityAsync<UrlMappingEntity>(partitionKey, key, cancellationToken: ct);

        return dbEntity is null
            ? null
            : new UrlMapping(dbEntity.Value.OriginalUrl, dbEntity.Value.ShortUrl)
            {
                CreatedAt = dbEntity.Value.Timestamp ?? DateTimeOffset.UtcNow,
            };
    }
}
