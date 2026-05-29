using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Infrastructure.SqliteStorage.Persistence;

namespace UrlShortener.Infrastructure.SqliteStorage.Tests;

public sealed class SqliteDatabaseInitializerTests
{
    [Fact]
    public async Task StartAsync_WhenSchemaMissing_CreatesUrlMappingsTable()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(ct);

        var services = new ServiceCollection();
        services.AddDbContext<UrlShortenerDbContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();

        var initializer = new SqliteDatabaseInitializer(provider.GetRequiredService<IServiceScopeFactory>());

        //Act
        await initializer.StartAsync(ct);

        //Assert
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();
        var canQuery = async () => await db.UrlMappings.AnyAsync(ct);
        await canQuery.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_AlwaysCompletes()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddDbContext<UrlShortenerDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        await using var provider = services.BuildServiceProvider();
        var initializer = new SqliteDatabaseInitializer(provider.GetRequiredService<IServiceScopeFactory>());

        //Act
        var act = async () => await initializer.StopAsync(ct);

        //Assert
        await act.ShouldNotThrowAsync();
    }
}
