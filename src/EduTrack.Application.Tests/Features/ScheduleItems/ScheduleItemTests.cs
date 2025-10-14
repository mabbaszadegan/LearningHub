using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EduTrack.Application.Tests.Features.ScheduleItems;

/// <summary>
/// Unit tests for Schedule Item functionality
/// </summary>
public class ScheduleItemTests
{
    [Fact]
    public void CreateScheduleItem_WithValidData_ShouldCreateEntity()
    {
        // Arrange
        var teachingPlanId = 1;
        var type = ScheduleItemType.Writing;
        var title = "Test Assignment";
        var description = "Test Description";
        var startDate = DateTimeOffset.UtcNow;
        var dueDate = DateTimeOffset.UtcNow.AddDays(7);
        var isMandatory = true;
        var contentJson = "{}";
        var maxScore = 100m;

        // Act
        var scheduleItem = ScheduleItem.Create(
            teachingPlanId,
            type,
            title,
            description,
            startDate,
            dueDate,
            isMandatory,
            contentJson,
            maxScore,
            null,
            null,
            null
        );

        // Assert
        scheduleItem.Should().NotBeNull();
        scheduleItem.TeachingPlanId.Should().Be(teachingPlanId);
        scheduleItem.Type.Should().Be(type);
        scheduleItem.Title.Should().Be(title);
        scheduleItem.Description.Should().Be(description);
        scheduleItem.StartDate.Should().Be(startDate);
        scheduleItem.DueDate.Should().Be(dueDate);
        scheduleItem.IsMandatory.Should().Be(isMandatory);
        scheduleItem.ContentJson.Should().Be(contentJson);
        scheduleItem.MaxScore.Should().Be(maxScore);
    }

    [Fact]
    public void CreateScheduleItem_WithInvalidTitle_ShouldThrowException()
    {
        // Act & Assert
        var act = () => ScheduleItem.Create(
            1,
            ScheduleItemType.Writing,
            "", // Invalid title
            "Test Description",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            false,
            "{}",
            null,
            null,
            null,
            null
        );
        
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateScheduleItem_WithInvalidContentJson_ShouldThrowException()
    {
        // Act & Assert
        var act = () => ScheduleItem.Create(
            1,
            ScheduleItemType.Writing,
            "Test Title",
            "Test Description",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(7),
            false,
            "", // Invalid content JSON
            null,
            null,
            null,
            null
        );
        
        act.Should().Throw<ArgumentException>();
    }
}
