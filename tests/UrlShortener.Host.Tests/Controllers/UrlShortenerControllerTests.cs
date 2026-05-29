using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using UrlShortener.Core;
using UrlShortener.Host.Configuration;
using UrlShortener.Host.Controllers;

namespace UrlShortener.Host.Tests.Controllers;

public sealed class UrlShortenerControllerTests
{
    [Fact]
    public async Task ShortenUrl_WithValidRequest_ReturnsOkWithFullShortUrl()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        service.ShortenUrlAsync("https://example.com/long", "demo", ct).Returns("demo-1a2b3c4d");
        var controller = CreateController(service, domain: "https://short.example");
        var request = new CreateShortUrlRequest { Url = "https://example.com/long", Prefix = "demo" };

        //Act
        var result = await controller.ShortenUrl(request, ct);

        //Assert
        var ok = result.Result.ShouldBeOfType<Ok<CreateShortUrlResponse>>();
        ok.Value!.OriginalUrl.ShouldBe("https://example.com/long");
        ok.Value.ShortUrl.ShouldBe("https://short.example/demo-1a2b3c4d");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ShortenUrl_WithBlankUrl_ReturnsBadRequest(string url)
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        var controller = CreateController(service);
        var request = new CreateShortUrlRequest { Url = url };

        //Act
        var result = await controller.ShortenUrl(request, ct);

        //Assert
        var badRequest = result.Result.ShouldBeOfType<BadRequest<string>>();
        badRequest.Value.ShouldBe("The original URL cannot be empty.");
        await service.DidNotReceive().ShortenUrlAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("/relative/path")]
    [InlineData("ftp://")]
    public async Task ShortenUrl_WithNonAbsoluteUrl_ReturnsBadRequest(string url)
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        var controller = CreateController(service);
        var request = new CreateShortUrlRequest { Url = url };

        //Act
        var result = await controller.ShortenUrl(request, ct);

        //Assert
        var badRequest = result.Result.ShouldBeOfType<BadRequest<string>>();
        badRequest.Value.ShouldBe("The provided URL is not a valid absolute URL.");
    }

    [Fact]
    public async Task ShortenUrl_WithoutPrefix_PassesNullPrefixToService()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        service.ShortenUrlAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns("1a2b3c4d");
        var controller = CreateController(service);
        var request = new CreateShortUrlRequest { Url = "https://example.com" };

        //Act
        await controller.ShortenUrl(request, ct);

        //Assert
        await service.Received(1).ShortenUrlAsync("https://example.com", null, ct);
    }

    [Fact]
    public async Task ResolveShortUrl_WhenServiceReturnsUrl_ReturnsRedirect()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        service.GetOriginalUrlAsync("abc12345", ct).Returns("https://example.com/target");
        var controller = CreateController(service);

        //Act
        var result = await controller.ResolveShortUrl("abc12345", ct);

        //Assert
        var redirect = result.Result.ShouldBeOfType<RedirectHttpResult>();
        redirect.Url.ShouldBe("https://example.com/target");
    }

    [Fact]
    public async Task ResolveShortUrl_WhenServiceReturnsNull_ReturnsNotFound()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var service = Substitute.For<IUrlShortenerService>();
        service.GetOriginalUrlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((string?)null);
        var controller = CreateController(service);

        //Act
        var result = await controller.ResolveShortUrl("missing", ct);

        //Assert
        result.Result.ShouldBeOfType<NotFound>();
    }

    private static UrlShortenerController CreateController(
        IUrlShortenerService service,
        string domain = "http://localhost:8080")
    {
        var settings = Options.Create(new ShortenerSettings { Domain = domain });
        return new UrlShortenerController(service, settings);
    }
}
