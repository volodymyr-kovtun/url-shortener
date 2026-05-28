using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Infrastructure.SqliteStorage.Configuration;

public sealed class SqliteStorageSettings
{
    [Required(AllowEmptyStrings = false)]
    public required string ConnectionString { get; init; }
}
