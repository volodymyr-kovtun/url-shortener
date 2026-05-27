using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Infrastructure.Auth;

public static class AuthRegistry
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, ApiKeyHandler>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

        return services;
    }
}
