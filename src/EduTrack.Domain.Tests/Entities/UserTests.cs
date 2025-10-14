using EduTrack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EduTrack.Domain.Tests.Entities;

/// <summary>
/// Unit tests for User entity following TDD principles
/// </summary>
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = User.Create(firstName, lastName, email);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => User.Create("John", "Doe", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateProfile()
    {
        // Arrange
        var user = User.Create("John", "Doe", "test@example.com");
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("John", "Doe", "test@example.com");
        user.Deactivate(); // First deactivate

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "test@example.com");

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }
}
