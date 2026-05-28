namespace UrlShortener.Infrastructure.SqliteStorage.Persistence;

internal sealed class UrlMappingRecord
{
    public string ShortUrl { get; set; } = default!;
    public string OriginalUrl { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
