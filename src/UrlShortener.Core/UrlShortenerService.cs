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

    public async Task<string> ShortenUrlAsync(string url, string? prefix, CancellationToken ct = default)
    {
        var shortCode = GenerateShortCode();
        shortCode = string.IsNullOrWhiteSpace(prefix) ? shortCode : $"{prefix}-{shortCode}";

        await storageProvider.AddAsync(new UrlMapping(url, shortCode), ct);
        return shortCode;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortUrl, CancellationToken ct = default) =>
        await cache.GetOrCreateAsync(
            CacheKeyPrefix + shortUrl,
            (storageProvider, shortUrl),
            static async (state, cancellationToken) =>
            {
                var entity = await state.storageProvider.GetAsync(state.shortUrl, cancellationToken);
                return entity?.OriginalUrl;
            },
            CacheOptions,
            cancellationToken: ct);

    private static string GenerateShortCode() => Guid.NewGuid().ToString("N")[..8];
}
