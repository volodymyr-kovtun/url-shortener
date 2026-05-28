using Microsoft.Extensions.Caching.Hybrid;

namespace UrlShortener.Core;

public interface IUrlShortenerService
{
    Task<string> ShortenUrlAsync(string url, string? prefix, CancellationToken ct = default);
    Task<string?> GetOriginalUrlAsync(string shortUrl, CancellationToken ct = default);
}

public sealed class UrlShortenerService(
    IStorageProvider<UrlMapping, string> storageProvider,
    HybridCache cache) : IUrlShortenerService
{
    private const string CacheKeyPrefix = "short-url:";

    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };

    private static readonly HybridCacheEntryOptions CacheReadOnlyOptions = new()
    {
        Flags = HybridCacheEntryFlags.DisableUnderlyingData
    };

    public async Task<string> ShortenUrlAsync(string url, string? prefix, CancellationToken ct = default)
    {
        var shortCode = GenerateShortCode();
        shortCode = string.IsNullOrWhiteSpace(prefix) ? shortCode : $"{prefix}-{shortCode}";

        await storageProvider.AddAsync(new UrlMapping(url, shortCode), ct);
        return shortCode;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortUrl, CancellationToken ct = default)
    {
        var cacheKey = CacheKeyPrefix + shortUrl;
        var cachedUrl = await cache.GetOrCreateAsync<string?>(
            cacheKey,
            static _ => ValueTask.FromResult<string?>(null),
            CacheReadOnlyOptions,
            cancellationToken: ct);

        if (cachedUrl is not null)
        {
            return cachedUrl;
        }

        var entity = await storageProvider.GetAsync(shortUrl, ct);
        if (entity is null)
        {
            return null;
        }

        await cache.SetAsync(cacheKey, entity.OriginalUrl, CacheOptions, cancellationToken: ct);
        return entity.OriginalUrl;
    }

    private static string GenerateShortCode() => Guid.NewGuid().ToString("N")[..8];
}
