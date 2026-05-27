namespace UrlShortener.Core;

public interface IUrlShortenerService
{
    Task<string> ShortenUrlAsync(string url, string? prefix, CancellationToken ct = default);
    Task<string?> GetOriginalUrlAsync(string shortUrl, CancellationToken ct = default);
}

public sealed class UrlShortenerService(IStorageProvider<UrlMapping, string> storageProvider) : IUrlShortenerService
{
    public async Task<string> ShortenUrlAsync(string url, string? prefix, CancellationToken ct = default)
    {
        var shortCode = GenerateShortCode();
        shortCode = string.IsNullOrWhiteSpace(prefix) ? shortCode : $"{prefix}-{shortCode}";
        var entity = new UrlMapping(url, shortCode);

        await storageProvider.AddAsync(entity, ct);
        return shortCode;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortUrl, CancellationToken ct = default)
    {
        var entity = await storageProvider.GetAsync(shortUrl, ct);
        return entity?.OriginalUrl;
    }

    private static string GenerateShortCode() => Guid.NewGuid().ToString().Substring(0, 8).ToLower();
}
