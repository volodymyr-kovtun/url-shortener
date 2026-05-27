FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UrlShortener.sln ./
COPY UrlShortener.Core/UrlShortener.Core.csproj UrlShortener.Core/
COPY UrlShortener.Infrastructure.Auth/UrlShortener.Infrastructure.Auth.csproj UrlShortener.Infrastructure.Auth/
COPY UrlShortener.Infrastructure.AzureTableStorage/UrlShortener.Infrastructure.AzureTableStorage.csproj UrlShortener.Infrastructure.AzureTableStorage/
COPY UrlShortener.Host/UrlShortener.Host.csproj UrlShortener.Host/
RUN dotnet restore UrlShortener.sln

COPY . .
RUN dotnet publish UrlShortener.Host/UrlShortener.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "UrlShortener.Host.dll"]
