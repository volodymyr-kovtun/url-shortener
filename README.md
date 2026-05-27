# URL Shortener

A small ASP.NET Core 8 service that turns long URLs into short slugs and redirects them back.
Storage is pluggable behind an `IStorageProvider<UrlMapping, string>` abstraction; the included
provider targets Azure Table Storage (and Azurite for local development).

## Quick start (Docker)

The fastest way to run the service locally is via `docker compose`, which spins up Azurite (the
Azure Table Storage emulator) alongside the API.

```bash
cp docker/.env.example docker/.env
# edit docker/.env and set API_KEY to a random string
docker compose -f docker/docker-compose.yaml --env-file docker/.env up --build
```

Then open <http://localhost:8080/swagger>.

## Running without Docker

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download).
2. Start Azurite (Table service on port `10002`) — e.g. `docker run -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite-table --tableHost 0.0.0.0`.
3. Set the API key as a user-secret:

   ```bash
   dotnet user-secrets init --project src/UrlShortener.Host
   dotnet user-secrets set "Auth:ApiKey" "your-dev-key" --project src/UrlShortener.Host
   ```

4. Run the API:

   ```bash
   dotnet run --project src/UrlShortener.Host
   ```

   Swagger is at <http://localhost:5194/swagger>.

## Using the API

Create a short URL (requires the API key in the `X-Api-Key` header):

```bash
curl -X POST http://localhost:8080/api/shorten \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: $API_KEY" \
  -d '{"url":"https://example.com/some/very/long/path","prefix":"demo"}'
```

Response:

```json
{ "originalUrl": "https://example.com/some/very/long/path", "shortUrl": "http://localhost:8080/demo-1a2b3c4d" }
```

Resolve a short URL (no auth required) — issues a `302` redirect:

```bash
curl -i http://localhost:8080/demo-1a2b3c4d
```

## Project structure

```
src/
├── UrlShortener.Core/                            Domain types and the IStorageProvider abstraction
├── UrlShortener.Infrastructure.Auth/             API key authorization handler + DI registration
├── UrlShortener.Infrastructure.AzureTableStorage/ Azure Table Storage implementation
└── UrlShortener.Host/                            ASP.NET Core Web API (controllers, Swagger, composition root)
docker/
├── docker-compose.yaml                            Brings up Azurite + the API together
└── .env.example                                   Template for the API key
.github/workflows/
├── ci.yml                                         Build, test, and docker-build on push/PR
└── azure-deploy.yml                               Manual-only reference workflow for Azure App Service
```

## Configuration

All settings can be supplied via `appsettings.json`, environment variables, or user-secrets.
The double-underscore form is required for environment variables.

| Key | Env var | Required | Description |
| --- | --- | --- | --- |
| `Auth:ApiKey` | `Auth__ApiKey` | yes | Shared secret required in the `X-Api-Key` header to create short URLs |
| `ConnectionStrings:AzureTableStorageConnectionString` | `ConnectionStrings__AzureTableStorageConnectionString` | yes | Azure Table Storage connection string (Azurite for local) |
| `Shortener:Domain` | `Shortener__Domain` | yes | Base URL prepended to generated short codes in the response |

The app fails fast on startup if either required value is missing.

## Deployment

- **Docker:** the `Dockerfile` at `src/UrlShortener.Host/Dockerfile` produces a runnable
  `aspnet:8.0` image. The compose file in `docker/` is intended for local development;
  for production you would replace Azurite with a real Azure Storage account (or another
  `IStorageProvider` implementation) and run the image behind your reverse proxy of choice.
- **Azure App Service:** the workflow at `.github/workflows/azure-deploy.yml` is preserved
  as a reference and is disabled by default — see the comment block at the top of that file
  for how to enable it.

## License

[MIT](./LICENSE)
