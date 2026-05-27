using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using UrlShortener.Core;
using UrlShortener.Host.Configuration;
using UrlShortener.Host.Controllers;
using UrlShortener.Host.Swagger;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Infrastructure.Auth.Configuration;
using UrlShortener.Infrastructure.AzureTableStorage;

var builder = WebApplication.CreateBuilder(args);

var azureTableStorageConnectionString = builder.Configuration.GetConnectionString("AzureTableStorageConnectionString");

if (string.IsNullOrWhiteSpace(azureTableStorageConnectionString))
{
    throw new InvalidOperationException("Azure Table Storage connection string is not configured (ConnectionStrings:AzureTableStorageConnectionString).");
}

var apiKey = builder.Configuration.GetSection("Auth")["ApiKey"];

if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException("API key is not configured (Auth:ApiKey). Set it via environment variable Auth__ApiKey or user-secrets.");
}

builder.Services
    .Configure<ShortenerSettings>(builder.Configuration.GetRequiredSection("Shortener"))
    .Configure<AuthSettings>(builder.Configuration.GetRequiredSection("Auth"))
    .AddScoped<IStorageProvider<UrlMapping, string>>(_ => new AzureTableStorageProvider(azureTableStorageConnectionString))
    .AddScoped<IUrlShortenerService, UrlShortenerService>()
    .AddHttpContextAccessor()
    .AddAuth();

builder.Services.AddCors(options => options.AddPolicy("AllowAll", b =>
{
    b.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(new[] { JwtBearerDefaults.AuthenticationScheme });
        policy.Requirements.Add(new ApiKeyRequirement());
    });
});

builder.Services
    .AddEndpointsApiExplorer().AddSwaggerGen(
        options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "URL Shortener", Version = "v1" });

            options.OperationFilter<AddHeaderOperationFilter>();

            var apiXml = $"{typeof(UrlShortenerController).Assembly.GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiXml));
        });

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener"));

app.Run();
