using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using UrlShortener.Infrastructure.Auth;

namespace UrlShortener.Infrastructure.Auth.Tests;

public sealed class ApiKeyHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidApiKeyHeader_Succeeds()
    {
        //Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-key").Returns(true);
        var context = BuildAuthorizationContext(headerValue: "valid-key");
        var handler = new ApiKeyHandler(BuildHttpContextAccessor(headerValue: "valid-key"), validator);

        //Act
        await handler.HandleAsync(context);

        //Assert
        context.HasSucceeded.ShouldBeTrue();
        context.HasFailed.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidApiKeyHeader_Fails()
    {
        //Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(Arg.Any<string>()).Returns(false);
        var context = BuildAuthorizationContext(headerValue: "bad-key");
        var handler = new ApiKeyHandler(BuildHttpContextAccessor(headerValue: "bad-key"), validator);

        //Act
        await handler.HandleAsync(context);

        //Assert
        context.HasSucceeded.ShouldBeFalse();
        context.HasFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithoutApiKeyHeader_Fails()
    {
        //Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        var context = BuildAuthorizationContext(headerValue: null);
        var handler = new ApiKeyHandler(BuildHttpContextAccessor(headerValue: null), validator);

        //Act
        await handler.HandleAsync(context);

        //Assert
        context.HasSucceeded.ShouldBeFalse();
        context.HasFailed.ShouldBeTrue();
        validator.DidNotReceive().IsValid(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenHttpContextIsNull_Fails()
    {
        //Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var context = BuildAuthorizationContext(headerValue: null);
        var handler = new ApiKeyHandler(accessor, validator);

        //Act
        await handler.HandleAsync(context);

        //Assert
        context.HasSucceeded.ShouldBeFalse();
        context.HasFailed.ShouldBeTrue();
        validator.DidNotReceive().IsValid(Arg.Any<string>());
    }

    private static IHttpContextAccessor BuildHttpContextAccessor(string? headerValue)
    {
        var httpContext = new DefaultHttpContext();
        if (headerValue is not null)
        {
            httpContext.Request.Headers["X-Api-Key"] = headerValue;
        }

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(httpContext);
        return accessor;
    }

    private static AuthorizationHandlerContext BuildAuthorizationContext(string? headerValue)
    {
        var requirement = new ApiKeyRequirement();
        return new AuthorizationHandlerContext([requirement], user: new(), resource: null);
    }
}
