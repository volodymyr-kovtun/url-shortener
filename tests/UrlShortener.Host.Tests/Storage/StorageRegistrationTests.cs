using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Core;
using UrlShortener.Host.Storage;
using UrlShortener.Infrastructure.AzureTableStorage;
using UrlShortener.Infrastructure.SqliteStorage;

namespace UrlShortener.Host.Tests.Storage;

public sealed class StorageRegistrationTests
{
    [Fact]
    public void AddStorage_WithSqliteProvider_RegistersSqliteStorageProvider()
    {
        //Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new()
        {
            ["Storage:Provider"] = "Sqlite",
            ["Storage:Sqlite:ConnectionString"] = "Data Source=test.db",
        });

        //Act
        services.AddStorage(configuration);

        //Assert
        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IStorageProvider<UrlMapping, string>>();
        storage.ShouldBeOfType<SqliteStorageProvider>();
    }

    [Fact]
    public void AddStorage_WithAzureTableStorageProvider_RegistersAzureProvider()
    {
        //Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new()
        {
            ["Storage:Provider"] = "AzureTableStorage",
            ["Storage:AzureTableStorage:ConnectionString"] = "UseDevelopmentStorage=true",
        });

        //Act
        services.AddStorage(configuration);

        //Assert
        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IStorageProvider<UrlMapping, string>>();
        storage.ShouldBeOfType<AzureTableStorageProvider>();
    }

    [Fact]
    public void AddStorage_WithNoProviderConfigured_DefaultsToSqlite()
    {
        //Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new()
        {
            ["Storage:Sqlite:ConnectionString"] = "Data Source=test.db",
        });

        //Act
        services.AddStorage(configuration);

        //Assert
        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IStorageProvider<UrlMapping, string>>();
        storage.ShouldBeOfType<SqliteStorageProvider>();
    }

    [Theory]
    [InlineData("sqlite")]
    [InlineData("SQLITE")]
    [InlineData("SqLiTe")]
    public void AddStorage_WithCaseInsensitiveProviderName_RegistersCorrectly(string providerName)
    {
        //Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new()
        {
            ["Storage:Provider"] = providerName,
            ["Storage:Sqlite:ConnectionString"] = "Data Source=test.db",
        });

        //Act
        services.AddStorage(configuration);

        //Assert
        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IStorageProvider<UrlMapping, string>>();
        storage.ShouldBeOfType<SqliteStorageProvider>();
    }

    [Fact]
    public void AddStorage_WithUnknownProvider_ThrowsInvalidOperationException()
    {
        //Arrange
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new()
        {
            ["Storage:Provider"] = "MongoDB",
        });

        //Act
        var act = () => services.AddStorage(configuration);

        //Assert
        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("MongoDB");
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
