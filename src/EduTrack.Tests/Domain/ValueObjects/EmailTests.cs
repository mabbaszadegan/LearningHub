using EduTrack.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EduTrack.Tests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("admin+tag@company.org")]
    public void Constructor_WithValidEmail_ShouldCreateEmail(string email)
    {
        // Act
        var result = new Email(email);

        // Assert
        result.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var action = () => new Email(email);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    public void Constructor_WithMalformedEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var action = () => new Email(email);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        string emailString = "test@example.com";

        // Act
        Email email = emailString;

        // Assert
        email.Value.Should().Be(emailString.ToLowerInvariant());
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string result = email;

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }
}
