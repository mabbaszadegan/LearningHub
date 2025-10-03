using EduTrack.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EduTrack.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email@domain.co.uk")]
    [InlineData("user+tag@example.org")]
    [InlineData("user123@test-domain.com")]
    public void Create_WithValidEmail_ShouldCreateEmail(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    [InlineData("user..name@example.com")]
    [InlineData("user@example..com")]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        var action = () => Email.Create(email);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldConvertToLowerCase()
    {
        // Arrange
        var email = "USER@EXAMPLE.COM";

        // Act
        var result = Email.Create(email);

        // Assert
        result.Value.Should().Be("user@example.com");
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        // Arrange
        var emailString = "user@example.com";

        // Act
        Email email = emailString;

        // Assert
        email.Value.Should().Be(emailString.ToLowerInvariant());
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var email = Email.Create("user@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("user@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var email = Email.Create("user@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("user@example.com");
    }

    [Fact]
    public void Equality_WithSameEmail_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("user@example.com");
        var email2 = Email.Create("user@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentCase_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("USER@EXAMPLE.COM");
        var email2 = Email.Create("user@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentEmail_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("user1@example.com");
        var email2 = Email.Create("user2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        email1.GetHashCode().Should().NotBe(email2.GetHashCode());
    }
}
