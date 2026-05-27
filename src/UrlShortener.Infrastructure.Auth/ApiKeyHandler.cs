using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace UrlShortener.Infrastructure.Auth;

public class ApiKeyHandler(IHttpContextAccessor contextAccessor, IApiKeyValidator validator) : AuthorizationHandler<ApiKeyRequirement>
{
    private const string ApiKeyHeaderName = "X-Api-Key";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        if (!contextAccessor?.HttpContext?.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey) ?? false)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (!validator.IsValid(apiKey!))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
