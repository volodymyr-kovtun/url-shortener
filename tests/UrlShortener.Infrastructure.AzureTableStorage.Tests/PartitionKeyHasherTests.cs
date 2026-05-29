using UrlShortener.Infrastructure.AzureTableStorage;

namespace UrlShortener.Infrastructure.AzureTableStorage.Tests;

public sealed class PartitionKeyHasherTests
{
    [Fact]
    public void Hash_GivenAnyInput_ReturnsFourLowercaseHexCharacters()
    {
        //Arrange
        const string shortUrl = "abc12345";

        //Act
        var result = PartitionKeyHasher.Hash(shortUrl);

        //Assert
        result.ShouldMatch("^[a-f0-9]{4}$");
    }

    [Fact]
    public void Hash_GivenSameInputTwice_IsDeterministic()
    {
        //Arrange
        const string shortUrl = "abc12345";

        //Act
        var first = PartitionKeyHasher.Hash(shortUrl);
        var second = PartitionKeyHasher.Hash(shortUrl);

        //Assert
        first.ShouldBe(second);
    }

    [Fact]
    public void Hash_GivenDifferentInputs_ReturnsDifferentValuesMostOfTheTime()
    {
        //Arrange
        var inputs = Enumerable.Range(0, 200).Select(i => $"url-{i}").ToArray();

        //Act
        var hashes = inputs.Select(PartitionKeyHasher.Hash).ToHashSet();

        //Assert
        hashes.Count.ShouldBeGreaterThan(100);
    }

    [Fact]
    public void Hash_GivenEmptyString_ProducesValidHash()
    {
        //Arrange
        var input = string.Empty;

        //Act
        var result = PartitionKeyHasher.Hash(input);

        //Assert
        result.ShouldMatch("^[a-f0-9]{4}$");
    }

    [Theory]
    [InlineData("abc", "ba78")]
    [InlineData("demo-1a2b3c4d", "5727")]
    public void Hash_GivenKnownInput_ReturnsExpectedHash(string input, string expected)
    {
        //Arrange
        _ = input;

        //Act
        var result = PartitionKeyHasher.Hash(input);

        //Assert
        result.ShouldBe(expected);
    }
}
