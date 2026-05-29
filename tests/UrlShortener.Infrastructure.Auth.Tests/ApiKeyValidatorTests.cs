using Microsoft.Extensions.Options;
using UrlShortener.Infrastructure.Auth;
using UrlShortener.Infrastructure.Auth.Configuration;

namespace UrlShortener.Infrastructure.Auth.Tests;

public sealed class ApiKeyValidatorTests
{
    [Fact]
    public void IsValid_WhenUserKeyMatchesConfiguredKey_ReturnsTrue()
    {
        //Arrange
        var validator = CreateValidator(configured: "secret-123");

        //Act
        var result = validator.IsValid("secret-123");

        //Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WhenUserKeyDoesNotMatch_ReturnsFalse()
    {
        //Arrange
        var validator = CreateValidator(configured: "secret-123");

        //Act
        var result = validator.IsValid("wrong-key");

        //Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WhenUserKeyIsBlank_ReturnsFalse(string? userKey)
    {
        //Arrange
        var validator = CreateValidator(configured: "secret-123");

        //Act
        var result = validator.IsValid(userKey!);

        //Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WhenConfiguredKeyIsBlank_ReturnsFalse(string? configured)
    {
        //Arrange
        var validator = CreateValidator(configured);

        //Act
        var result = validator.IsValid("any-user-key");

        //Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WhenConfiguredKeyChanges_ReflectsLatestValue()
    {
        //Arrange
        var monitor = Substitute.For<IOptionsMonitor<AuthSettings>>();
        monitor.CurrentValue.Returns(new AuthSettings { ApiKey = "first-key" });
        var validator = new ApiKeyValidator(monitor);
        validator.IsValid("first-key").ShouldBeTrue();
        monitor.CurrentValue.Returns(new AuthSettings { ApiKey = "second-key" });

        //Act
        var result = validator.IsValid("second-key");

        //Assert
        result.ShouldBeTrue();
        validator.IsValid("first-key").ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithCaseDifference_ReturnsFalse()
    {
        //Arrange
        var validator = CreateValidator(configured: "Secret-Key");

        //Act
        var result = validator.IsValid("secret-key");

        //Assert
        result.ShouldBeFalse();
    }

    private static ApiKeyValidator CreateValidator(string? configured)
    {
        var monitor = Substitute.For<IOptionsMonitor<AuthSettings>>();
        monitor.CurrentValue.Returns(new AuthSettings { ApiKey = configured! });
        return new ApiKeyValidator(monitor);
    }
}
