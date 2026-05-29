using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Core;
using UrlShortener.Infrastructure.SqliteStorage;
using UrlShortener.Infrastructure.SqliteStorage.Persistence;

namespace UrlShortener.Infrastructure.SqliteStorage.Tests;

public sealed class SqliteStorageProviderTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private UrlShortenerDbContext _dbContext = null!;
    private SqliteStorageProvider _provider = null!;

    public async ValueTask InitializeAsync()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new UrlShortenerDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
        _provider = new SqliteStorageProvider(_dbContext);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_NewMapping_PersistsToDatabase()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var mapping = new UrlMapping("https://example.com", "abc12345");

        //Act
        await _provider.AddAsync(mapping, ct);

        //Assert
        var stored = await _dbContext.UrlMappings.SingleAsync(ct);
        stored.ShortUrl.ShouldBe("abc12345");
        stored.OriginalUrl.ShouldBe("https://example.com");
    }

    [Fact]
    public async Task AddAsync_PreservesCreatedAtTimestamp()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        var createdAt = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var mapping = new UrlMapping("https://example.com", "abc12345") { CreatedAt = createdAt };

        //Act
        await _provider.AddAsync(mapping, ct);

        //Assert
        var stored = await _dbContext.UrlMappings.SingleAsync(ct);
        stored.CreatedAt.ShouldBe(createdAt);
    }

    [Fact]
    public async Task GetAsync_WhenMappingExists_ReturnsMapping()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        await _provider.AddAsync(new UrlMapping("https://example.com", "abc12345"), ct);

        //Act
        var result = await _provider.GetAsync("abc12345", ct);

        //Assert
        result.ShouldNotBeNull();
        result.OriginalUrl.ShouldBe("https://example.com");
        result.ShortUrl.ShouldBe("abc12345");
    }

    [Fact]
    public async Task GetAsync_WhenMappingDoesNotExist_ReturnsNull()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;

        //Act
        var result = await _provider.GetAsync("nonexistent", ct);

        //Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsync_DuplicateShortUrl_ThrowsDbUpdateException()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        await _provider.AddAsync(new UrlMapping("https://first.example", "abc12345"), ct);
        _dbContext.ChangeTracker.Clear();

        //Act
        var act = async () => await _provider.AddAsync(new UrlMapping("https://second.example", "abc12345"), ct);

        //Assert
        await act.ShouldThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task GetAsync_DoesNotTrackEntity()
    {
        //Arrange
        var ct = TestContext.Current.CancellationToken;
        await _provider.AddAsync(new UrlMapping("https://example.com", "abc12345"), ct);
        _dbContext.ChangeTracker.Clear();

        //Act
        await _provider.GetAsync("abc12345", ct);

        //Assert
        _dbContext.ChangeTracker.Entries().ShouldBeEmpty();
    }
}
