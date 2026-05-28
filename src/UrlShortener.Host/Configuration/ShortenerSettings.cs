using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Host.Configuration;

public sealed class ShortenerSettings
{
    [Required(AllowEmptyStrings = false)]
    public required string Domain { get; init; }
}
