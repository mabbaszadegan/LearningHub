using EduTrack.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EduTrack.Domain.Tests.Entities;

public class CourseTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCourse()
    {
        // Arrange
        var title = "Introduction to Programming";
        var description = "A beginner course in programming";
        var thumbnail = "course-thumbnail.jpg";
        var order = 1;
        var createdBy = "teacher@example.com";

        // Act
        var course = Course.Create(title, description, thumbnail, order, createdBy);

        // Assert
        course.Should().NotBeNull();
        course.Title.Should().Be(title);
        course.Description.Should().Be(description);
        course.Thumbnail.Should().Be(thumbnail);
        course.Order.Should().Be(order);
        course.CreatedBy.Should().Be(createdBy);
        course.IsActive.Should().BeTrue();
        course.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("", "Description", "thumbnail.jpg", 1, "teacher@example.com")]
    [InlineData("Title", "Description", "thumbnail.jpg", 1, "")]
    [InlineData(null, "Description", "thumbnail.jpg", 1, "teacher@example.com")]
    [InlineData("Title", "Description", "thumbnail.jpg", 1, null)]
    public void Create_WithInvalidParameters_ShouldThrowArgumentException(
        string title, string description, string thumbnail, int order, string createdBy)
    {
        // Act & Assert
        var action = () => Course.Create(title, description, thumbnail, order, createdBy);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Create_WithNegativeOrder_ShouldThrowArgumentException(int order)
    {
        // Act & Assert
        var action = () => Course.Create("Title", "Description", "thumbnail.jpg", order, "teacher@example.com");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateTitle_WithValidTitle_ShouldUpdateTitle()
    {
        // Arrange
        var course = Course.Create("Old Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var newTitle = "New Title";

        // Act
        course.UpdateTitle(newTitle);

        // Assert
        course.Title.Should().Be(newTitle);
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateTitle_WithInvalidTitle_ShouldThrowArgumentException(string title)
    {
        // Arrange
        var course = Course.Create("Old Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");

        // Act & Assert
        var action = () => course.UpdateTitle(title);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription()
    {
        // Arrange
        var course = Course.Create("Title", "Old Description", "thumbnail.jpg", 1, "teacher@example.com");
        var newDescription = "New Description";

        // Act
        course.UpdateDescription(newDescription);

        // Assert
        course.Description.Should().Be(newDescription);
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldUpdateDescription()
    {
        // Arrange
        var course = Course.Create("Title", "Old Description", "thumbnail.jpg", 1, "teacher@example.com");

        // Act
        course.UpdateDescription(null);

        // Assert
        course.Description.Should().BeNull();
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateOrder_WithValidOrder_ShouldUpdateOrder()
    {
        // Arrange
        var course = Course.Create("Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        var newOrder = 5;

        // Act
        course.UpdateOrder(newOrder);

        // Assert
        course.Order.Should().Be(newOrder);
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void UpdateOrder_WithNegativeOrder_ShouldThrowArgumentException(int order)
    {
        // Arrange
        var course = Course.Create("Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");

        // Act & Assert
        var action = () => course.UpdateOrder(order);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var course = Course.Create("Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");
        course.Deactivate(); // First deactivate

        // Act
        course.Activate();

        // Assert
        course.IsActive.Should().BeTrue();
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var course = Course.Create("Title", "Description", "thumbnail.jpg", 1, "teacher@example.com");

        // Act
        course.Deactivate();

        // Assert
        course.IsActive.Should().BeFalse();
        course.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
