using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Infrastructure.Auth.Configuration;

public sealed class AuthSettings
{
    [Required(AllowEmptyStrings = false)]
    public required string ApiKey { get; init; }
}
