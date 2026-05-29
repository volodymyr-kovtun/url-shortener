using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core;

namespace UrlShortener.Core.Tests;

public sealed class UrlShortenerServiceTests
{
    private const string ShortCodePattern = "^[a-f0-9]{8}$";
    private const string PrefixedShortCodePattern = "^[a-zA-Z0-9]+-[a-f0-9]{8}$";

    [Fact]
    public async Task ShortenUrlAsync_WithoutPrefix_ReturnsBareEightCharLowercaseHexCode()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var code = await service.ShortenUrlAsync("https://example.com", prefix: null, ct);

        //Assert
        code.ShouldMatch(ShortCodePattern);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ShortenUrlAsync_WithBlankPrefix_ReturnsBareCode(string? prefix)
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var code = await service.ShortenUrlAsync("https://example.com", prefix, ct);

        //Assert
        code.ShouldMatch(ShortCodePattern);
    }

    [Fact]
    public async Task ShortenUrlAsync_WithPrefix_PrependsPrefixToCode()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var code = await service.ShortenUrlAsync("https://example.com", "demo", ct);

        //Assert
        code.ShouldStartWith("demo-");
        code.ShouldMatch(PrefixedShortCodePattern);
    }

    [Fact]
    public async Task ShortenUrlAsync_Always_PersistsMappingToStorage()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var code = await service.ShortenUrlAsync("https://example.com/long", "demo", ct);

        //Assert
        await storage.Received(1).AddAsync(
            Arg.Is<UrlMapping>(m => m.OriginalUrl == "https://example.com/long" && m.ShortUrl == code),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShortenUrlAsync_CalledTwice_ProducesDistinctCodes()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var first = await service.ShortenUrlAsync("https://example.com", null, ct);
        var second = await service.ShortenUrlAsync("https://example.com", null, ct);

        //Assert
        first.ShouldNotBe(second);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_WhenStorageReturnsMapping_ReturnsOriginalUrl()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        storage.GetAsync("abc12345", Arg.Any<CancellationToken>())
            .Returns(new UrlMapping("https://example.com/target", "abc12345"));
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var result = await service.GetOriginalUrlAsync("abc12345", ct);

        //Assert
        result.ShouldBe("https://example.com/target");
    }

    [Fact]
    public async Task GetOriginalUrlAsync_WhenStorageReturnsNull_ReturnsNull()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        storage.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((UrlMapping?)null);
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var result = await service.GetOriginalUrlAsync("missing", ct);

        //Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetOriginalUrlAsync_OnSecondCallAfterHit_DoesNotQueryStorage()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        storage.GetAsync("abc12345", Arg.Any<CancellationToken>())
            .Returns(new UrlMapping("https://example.com/target", "abc12345"));
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;
        await service.GetOriginalUrlAsync("abc12345", ct);

        //Act
        var result = await service.GetOriginalUrlAsync("abc12345", ct);

        //Assert
        result.ShouldBe("https://example.com/target");
        await storage.Received(1).GetAsync("abc12345", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOriginalUrlAsync_OnSecondCallAfterMiss_QueriesStorageAgain()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        storage.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((UrlMapping?)null);
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;
        await service.GetOriginalUrlAsync("missing", ct);

        //Act
        await service.GetOriginalUrlAsync("missing", ct);

        //Assert
        await storage.Received(2).GetAsync("missing", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOriginalUrlAsync_TwoDifferentShortUrls_ResolvedIndependently()
    {
        //Arrange
        var storage = Substitute.For<IStorageProvider<UrlMapping, string>>();
        storage.GetAsync("aaaa1111", Arg.Any<CancellationToken>())
            .Returns(new UrlMapping("https://a.example", "aaaa1111"));
        storage.GetAsync("bbbb2222", Arg.Any<CancellationToken>())
            .Returns(new UrlMapping("https://b.example", "bbbb2222"));
        var service = CreateService(storage);
        var ct = TestContext.Current.CancellationToken;

        //Act
        var a = await service.GetOriginalUrlAsync("aaaa1111", ct);
        var b = await service.GetOriginalUrlAsync("bbbb2222", ct);

        //Assert
        a.ShouldBe("https://a.example");
        b.ShouldBe("https://b.example");
    }

    private static UrlShortenerService CreateService(IStorageProvider<UrlMapping, string> storage)
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddLogging();
        var cache = services.BuildServiceProvider().GetRequiredService<HybridCache>();
        return new UrlShortenerService(storage, cache);
    }
}
