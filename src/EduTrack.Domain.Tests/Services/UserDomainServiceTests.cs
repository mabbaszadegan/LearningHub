using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Services;
using FluentAssertions;
using Xunit;

namespace EduTrack.Domain.Tests.Services;

public class UserDomainServiceTests
{
    private readonly IUserDomainService _userDomainService;

    public UserDomainServiceTests()
    {
        _userDomainService = new UserDomainService();
    }

    [Fact]
    public void CanAssignRole_WithValidRole_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act
        var result = _userDomainService.CanAssignRole(user, UserRole.Teacher);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAssignRole_WithAdminRole_AndNonAdminUser_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act
        var result = _userDomainService.CanAssignRole(user, UserRole.Admin);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignRole_WithDemotion_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Teacher);

        // Act
        var result = _userDomainService.CanAssignRole(user, UserRole.Student);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanAssignRole_WithNullUser_ShouldReturnFalse()
    {
        // Act
        var result = _userDomainService.CanAssignRole(null!, UserRole.Student);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnrollInClass_WithValidStudentAndClass_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
        var course = Course.Create("Test Course", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var classEntity = Class.Create("Test Class", "Description", course.Id, "teacher@example.com", DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var result = _userDomainService.CanEnrollInClass(user, classEntity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEnrollInClass_WithNonStudent_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Teacher);
        var course = Course.Create("Test Course", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var classEntity = Class.Create("Test Class", "Description", course.Id, "teacher@example.com", DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var result = _userDomainService.CanEnrollInClass(user, classEntity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnrollInClass_WithInactiveUser_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
        user.Deactivate();
        var course = Course.Create("Test Course", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var classEntity = Class.Create("Test Class", "Description", course.Id, "teacher@example.com", DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var result = _userDomainService.CanEnrollInClass(user, classEntity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEnrollInClass_WithFutureClass_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);
        var course = Course.Create("Test Course", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var classEntity = Class.Create("Test Class", "Description", course.Id, "teacher@example.com", DateTimeOffset.UtcNow.AddDays(1));

        // Act
        var result = _userDomainService.CanEnrollInClass(user, classEntity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateOverallProgress_WithNoProgress_ShouldReturnZero()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", UserRole.Student);

        // Act
        var result = _userDomainService.CalculateOverallProgress(user);

        // Assert
        result.Should().Be(0.0);
    }

    [Fact]
    public void CalculateOverallProgress_WithNullUser_ShouldReturnZero()
    {
        // Act
        var result = _userDomainService.CalculateOverallProgress(null!);

        // Assert
        result.Should().Be(0.0);
    }
}
