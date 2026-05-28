using Microsoft.EntityFrameworkCore;
using UrlShortener.Core;
using UrlShortener.Infrastructure.SqliteStorage.Persistence;

namespace UrlShortener.Infrastructure.SqliteStorage;

internal sealed class SqliteStorageProvider(UrlShortenerDbContext db) : IStorageProvider<UrlMapping, string>
{
    public async Task AddAsync(UrlMapping entity, CancellationToken ct = default)
    {
        var record = new UrlMappingRecord
        {
            ShortUrl = entity.ShortUrl,
            OriginalUrl = entity.OriginalUrl,
            CreatedAt = entity.CreatedAt,
        };

        db.UrlMappings.Add(record);
        await db.SaveChangesAsync(ct);
    }

    public async Task<UrlMapping?> GetAsync(string key, CancellationToken ct = default)
    {
        var record = await db.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShortUrl == key, ct);

        return record is null
            ? null
            : new UrlMapping(record.OriginalUrl, record.ShortUrl) { CreatedAt = record.CreatedAt };
    }
}
