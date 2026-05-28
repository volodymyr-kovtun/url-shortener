using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using UrlShortener.Core;
using UrlShortener.Host;
using UrlShortener.Host.Configuration;
using UrlShortener.Host.Controllers;
using UrlShortener.Host.Storage;
using UrlShortener.Host.Swagger;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Infrastructure.Auth.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ShortenerSettings>()
    .Bind(builder.Configuration.GetSection("Shortener"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<AuthSettings>()
    .Bind(builder.Configuration.GetSection("Auth"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddStorage(builder.Configuration)
    .AddScoped<IUrlShortenerService, UrlShortenerService>()
    .AddHttpContextAccessor()
    .AddAuth();

builder.Services.AddHybridCache();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", b =>
{
    b.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services
    .AddAuthentication(options => options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>
    {
        policy.AddAuthenticationSchemes([JwtBearerDefaults.AuthenticationScheme]);
        policy.Requirements.Add(new ApiKeyRequirement());
    });
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<ApiKeyHeaderDocumentTransformer>();
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("URL Shortener"));

app.Run();
