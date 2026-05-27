namespace UrlShortener.Core;

public sealed class UrlMapping(string originalUrl, string shortUrl)
{
    public string OriginalUrl { get; init; } = originalUrl;
    public string ShortUrl { get; init; } = shortUrl;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
