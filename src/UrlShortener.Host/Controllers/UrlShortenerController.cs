using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UrlShortener.Core;
using UrlShortener.Host.Configuration;

namespace UrlShortener.Host.Controllers;

public sealed record CreateShortUrlRequest
{
    public required string Url { get; init; }
    public string? Prefix { get; init; }
}

public sealed record CreateShortUrlResponse(string OriginalUrl, string ShortUrl);

[ApiController]
public class UrlShortenerController(
    IUrlShortenerService urlShortenerService,
    IOptions<ShortenerSettings> settings) : ControllerBase
{
    [HttpPost("api/shorten")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<Results<Ok<CreateShortUrlResponse>, BadRequest<string>>> ShortenUrl(
        [FromBody] CreateShortUrlRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return TypedResults.BadRequest("The original URL cannot be empty.");
        }

        if (!Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
        {
            return TypedResults.BadRequest("The provided URL is not a valid absolute URL.");
        }

        var shortUrlCode = await urlShortenerService.ShortenUrlAsync(request.Url, request.Prefix, ct);
        var shortUrl = $"{settings.Value.Domain}/{shortUrlCode}";
        return TypedResults.Ok(new CreateShortUrlResponse(request.Url, shortUrl));
    }

    [HttpGet("{shortUrl}")]
    [AllowAnonymous]
    public async Task<Results<RedirectHttpResult, NotFound>> ResolveShortUrl(string shortUrl, CancellationToken ct)
    {
        var originalUrl = await urlShortenerService.GetOriginalUrlAsync(shortUrl, ct);
        return originalUrl is null
            ? TypedResults.NotFound()
            : TypedResults.Redirect(originalUrl);
    }
}
