using Microsoft.Extensions.Options;
using UrlShortener.Infrastructure.Auth.Configuration;

namespace UrlShortener.Infrastructure.Auth;

public interface IApiKeyValidator
{
    bool IsValid(string userApiKey);
}

public sealed class ApiKeyValidator(IOptionsMonitor<AuthSettings> settings) : IApiKeyValidator
{
    public bool IsValid(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            return false;
        }

        var apiKey = settings.CurrentValue.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        return userApiKey == apiKey;
    }
}
