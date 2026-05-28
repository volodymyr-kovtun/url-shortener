# URL Shortener

A small ASP.NET Core 8 service that turns long URLs into short slugs and redirects them back.
Storage is pluggable behind an `IStorageProvider<UrlMapping, string>` abstraction; bundled
providers target **SQLite** (the default, zero-setup local) and **Azure Table Storage** (with
Azurite for local emulation). Adding a new backend is a new project plus a one-line dispatch
entry — see *Adding a storage provider* below.

## Quick start (Docker)

The fastest way to run the service locally is via `docker compose`. The default profile uses
SQLite stored in a Docker volume — no external services required.

```bash
cp docker/.env.example docker/.env
# edit docker/.env and set API_KEY to a random string
docker compose -f docker/docker-compose.yaml --env-file docker/.env up --build
```

Then open <http://localhost:8080/swagger>.

To exercise the Azure Table Storage backend locally (via the Azurite emulator) instead,
use the alternative compose file:

```bash
docker compose -f docker/docker-compose.azure.yaml --env-file docker/.env up --build
```

## Running without Docker

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download).
2. Set the API key as a user-secret:

   ```bash
   dotnet user-secrets init --project src/UrlShortener.Host
   dotnet user-secrets set "Auth:ApiKey" "your-dev-key" --project src/UrlShortener.Host
   ```

3. Run the API. SQLite is the default — a `urlshortener-dev.db` file is created in the working
   directory on first startup:

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
├── UrlShortener.Infrastructure.SqliteStorage/    SQLite (EF Core) storage implementation — the default
├── UrlShortener.Infrastructure.AzureTableStorage/ Azure Table Storage implementation
└── UrlShortener.Host/                            ASP.NET Core Web API (controllers, Swagger, composition root)
docker/
├── docker-compose.yaml                            Default — API + SQLite (volume)
├── docker-compose.azure.yaml                      Alternative — API + Azurite (Azure Table Storage emulator)
└── .env.example                                   Template for the API key
.github/workflows/
├── ci.yml                                         Build, test, and docker-build on push/PR
└── azure-deploy.yml                               Manual-only reference workflow for Azure App Service
```

## Configuration

All settings can be supplied via `appsettings.json`, environment variables, or user-secrets.
The double-underscore form is required for environment variables.

| Key | Required | Description |
| --- | --- | --- |
| `Auth:ApiKey` | yes | Shared secret required in the `X-Api-Key` header to create short URLs |
| `Shortener:Domain` | yes | Base URL prepended to generated short codes in the response |
| `Storage:Provider` | no (default `Sqlite`) | Which storage backend to use: `Sqlite` or `AzureTableStorage` |
| `Storage:Sqlite:ConnectionString` | when provider is `Sqlite` | EF Core SQLite connection string (e.g. `Data Source=urlshortener.db`) |
| `Storage:AzureTableStorage:ConnectionString` | when provider is `AzureTableStorage` | Azure Storage account connection string |

The app fails fast on startup if a required value is missing.

## Architecture

The composition root lives in `Host/Storage/StorageRegistration.cs`. It reads
`Storage:Provider` and dispatches to the matching `Add<Provider>Storage(IConfiguration)`
extension method exposed by each infrastructure project. Each provider owns its own settings
type, its own connection lifecycle, and its own bootstrap concerns (the SQLite provider
registers a hosted service that runs `EnsureCreated` on startup).

Dependencies flow inward: `Host` and `Infrastructure.*` depend on `Core`, never the reverse.
Application code only ever touches `IStorageProvider<UrlMapping, string>` — the concrete
backend is invisible to the controller and service layer.

### Adding a storage provider

1. Create `src/UrlShortener.Infrastructure.<Name>Storage/` with:
   - An implementation of `IStorageProvider<UrlMapping, string>`.
   - A `<Name>StorageSettings` class for its configuration.
   - A `<Name>StorageRegistry` static class exposing `Add<Name>Storage(this IServiceCollection, IConfiguration)`
     and a `ProviderName` constant matching the value users will set in `Storage:Provider`.
2. Add a `ProjectReference` from `UrlShortener.Host` to the new project.
3. Add a case to the switch in `Host/Storage/StorageRegistration.cs`.
4. Document the new `Storage:<Name>:*` keys in this README.

No other code needs to change — the domain, controllers, and existing providers are untouched.

## Deployment

- **Docker:** the `Dockerfile` at `src/UrlShortener.Host/Dockerfile` produces a runnable
  `aspnet:8.0` image. The image is backend-agnostic; supply `Storage:Provider` and the
  matching connection string via environment variables.
- **Azure App Service:** the workflow at `.github/workflows/azure-deploy.yml` is preserved
  as a reference and is disabled by default — see the comment block at the top of that file
  for how to enable it.

## License

[MIT](./LICENSE)
