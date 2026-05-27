using Microsoft.AspNetCore.Authorization;

namespace UrlShortener.Infrastructure.Auth;

public class ApiKeyRequirement : IAuthorizationRequirement
{
}
