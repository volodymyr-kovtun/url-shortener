using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UrlShortener.Host;

namespace UrlShortener.Host.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_GivenException_ReturnsTrue()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var handler = new GlobalExceptionHandler(Substitute.For<ILogger<GlobalExceptionHandler>>());
        var httpContext = CreateHttpContext();

        //Act
        var handled = await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), ct);

        //Assert
        handled.ShouldBeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_GivenException_SetsStatusCodeTo500()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var handler = new GlobalExceptionHandler(Substitute.For<ILogger<GlobalExceptionHandler>>());
        var httpContext = CreateHttpContext();

        //Act
        await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), ct);

        //Assert
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_GivenException_WritesProblemDetailsBody()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var handler = new GlobalExceptionHandler(Substitute.For<ILogger<GlobalExceptionHandler>>());
        var httpContext = CreateHttpContext();

        //Act
        await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), ct);

        //Assert
        httpContext.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(httpContext.Response.Body, cancellationToken: ct);
        problem.ShouldNotBeNull();
        problem.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problem.Title.ShouldBe("An unexpected error occurred.");
    }

    [Fact]
    public async Task TryHandleAsync_GivenException_LogsErrorWithRequestPath()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = CreateHttpContext();
        httpContext.Request.Path = "/api/shorten";
        var exception = new InvalidOperationException("boom");

        //Act
        await handler.TryHandleAsync(httpContext, exception, ct);

        //Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }
}
