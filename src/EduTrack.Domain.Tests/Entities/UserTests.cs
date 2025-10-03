using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EduTrack.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var role = UserRole.Student;

        // Act
        var user = User.Create(firstName, lastName, email, role);

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(email);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "Doe", "john@example.com")]
    [InlineData("John", "", "john@example.com")]
    [InlineData("John", "Doe", "")]
    [InlineData(null, "Doe", "john@example.com")]
    [InlineData("John", null, "john@example.com")]
    [InlineData("John", "Doe", null)]
    public void Create_WithInvalidParameters_ShouldThrowArgumentException(string firstName, string lastName, string email)
    {
        // Act & Assert
        var action = () => User.Create(firstName, lastName, email, UserRole.Student);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithValidParameters_ShouldUpdateProfile()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        user.UpdateProfile(newFirstName, newLastName);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
    }

    [Theory]
    [InlineData("", "Smith")]
    [InlineData("Jane", "")]
    [InlineData(null, "Smith")]
    [InlineData("Jane", null)]
    public void UpdateProfile_WithInvalidParameters_ShouldThrowArgumentException(string firstName, string lastName)
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act & Assert
        var action = () => user.UpdateProfile(firstName, lastName);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateLastLogin_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
        var beforeUpdate = DateTimeOffset.UtcNow;

        // Act
        user.UpdateLastLogin();

        // Assert
        user.LastLoginAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
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
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void FullName_WithEmptyLastName_ShouldReturnOnlyFirstName()
    {
        // Arrange
        var user = User.Create("John", "", "john@example.com", UserRole.Student);

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be("John");
    }
}
