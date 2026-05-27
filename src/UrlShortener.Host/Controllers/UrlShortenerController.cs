using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UrlShortener.Core;
using UrlShortener.Host.Configuration;

namespace UrlShortener.Host.Controllers;

public record CreateShortUrlRequest
{
    [Required]
    public string Url { get; init; } = default!;

    public string? Prefix { get; init; }
}

public record CreateShortUrlResponse(string OriginalUrl, string ShortUrl);

public class UrlShortenerController(IUrlShortenerService urlShortenerService, IOptions<ShortenerSettings> settings, ILogger<UrlShortenerController> logger) : ControllerBase
{
    [HttpPost("api/shorten")]
    [Authorize(Policy = "ApiKeyPolicy")]
    public async Task<IActionResult> ShortenUrl([FromBody] CreateShortUrlRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest("The original URL cannot be empty.");
            }

            if (!Uri.IsWellFormedUriString(request.Url, UriKind.Absolute))
            {
                return BadRequest("The provided URL is not a valid absolute URL.");
            }

            var shortUrlCode = await urlShortenerService.ShortenUrlAsync(request.Url, request.Prefix);

            return Ok(new CreateShortUrlResponse(request.Url, string.Concat(settings.Value.Domain, "/", shortUrlCode)));
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while shortening the URL.");
            return StatusCode(500, "An error occurred while shortening the URL: " + e.Message);
        }
    }

    [HttpGet("{shortUrl}")]
    [AllowAnonymous]
    public async Task<IActionResult> ResolveShortUrl(string shortUrl)
    {
        try
        {
            var originalUrl = await urlShortenerService.GetOriginalUrlAsync(shortUrl);
            if (originalUrl is null)
            {
                return NotFound();
            }

            return Redirect(originalUrl);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving the original URL.");
            return StatusCode(500, "An error occurred while retrieving the original URL: " + e.Message);
        }
    }
}
