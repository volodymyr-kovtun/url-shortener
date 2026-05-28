namespace UrlShortener.Infrastructure.SqliteStorage.Configuration;

public sealed class SqliteStorageSettings
{
    public string ConnectionString { get; init; } = default!;
}
