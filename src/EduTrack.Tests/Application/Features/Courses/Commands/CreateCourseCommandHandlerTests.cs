using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.Courses.Commands;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EduTrack.Tests.Application.Features.Courses.Commands;

public class CreateCourseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateCourse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        using var context = new AppDbContext(options);
        var command = new CreateCourseCommand(
            "Test Course",
            "Test Description",
            "test-thumbnail.jpg",
            1);

        var handler = new CreateCourseCommandHandler(
            new Repository<Course>(context),
            new UnitOfWork(context),
            new TestClock(),
            new TestCurrentUserService());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Test Course");
        result.Value.Description.Should().Be("Test Description");
        result.Value.Thumbnail.Should().Be("test-thumbnail.jpg");
        result.Value.Order.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldReturnFailure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase2")
            .Options;

        using var context = new AppDbContext(options);
        var command = new CreateCourseCommand(
            "", // Invalid empty title
            "Test Description",
            "test-thumbnail.jpg",
            1);

        var handler = new CreateCourseCommandHandler(
            new Repository<Course>(context),
            new UnitOfWork(context),
            new TestClock(),
            new TestCurrentUserService());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }
}

public class TestClock : EduTrack.Application.Common.Interfaces.IClock
{
    public DateTimeOffset Now => new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
    public DateTimeOffset UtcNow => new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
}

public class TestCurrentUserService : EduTrack.Application.Common.Interfaces.ICurrentUserService
{
    public string? UserId => "test-user-id";
    public string? UserName => "test@example.com";
    public EduTrack.Domain.Enums.UserRole? Role => EduTrack.Domain.Enums.UserRole.Admin;
    public bool IsAuthenticated => true;
}
