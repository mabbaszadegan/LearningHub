using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Application.Features.TeachingPlan.CommandHandlers;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EduTrack.Application.Tests.Features.TeachingPlan;

public class CreateTeachingPlanCommandHandlerTests
{
    private readonly Mock<ITeachingPlanRepository> _teachingPlanRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateTeachingPlanCommandHandler _handler;

    public CreateTeachingPlanCommandHandlerTests()
    {
        _teachingPlanRepositoryMock = new Mock<ITeachingPlanRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new CreateTeachingPlanCommandHandler(
            _teachingPlanRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var courseId = 1;
        var teacherId = "teacher-123";
        var command = new CreateTeachingPlanCommand(courseId, "Test Plan", "Test Description");

        var course = Course.Create("Test Course", "Test Description", null, 1, teacherId, DisciplineType.Language);
        course.GetType().GetProperty("Id")?.SetValue(course, courseId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(teacherId);
        _currentUserServiceMock.Setup(x => x.UserName).Returns("Test Teacher");
        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);
        _teachingPlanRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TeachingPlan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Test Plan");
        result.Value.Description.Should().Be("Test Description");
        result.Value.CourseId.Should().Be(courseId);
        result.Value.TeacherId.Should().Be(teacherId);

        _teachingPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TeachingPlan>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CourseNotFound_ReturnsFailureResult()
    {
        // Arrange
        var courseId = 1;
        var teacherId = "teacher-123";
        var command = new CreateTeachingPlanCommand(courseId, "Test Plan", "Test Description");

        _currentUserServiceMock.Setup(x => x.UserId).Returns(teacherId);
        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Course not found");

        _teachingPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TeachingPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailureResult()
    {
        // Arrange
        var courseId = 1;
        var teacherId = "teacher-123";
        var otherTeacherId = "other-teacher-456";
        var command = new CreateTeachingPlanCommand(courseId, "Test Plan", "Test Description");

        var course = Course.Create("Test Course", "Test Description", null, 1, teacherId, DisciplineType.Language);
        course.GetType().GetProperty("Id")?.SetValue(course, courseId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(otherTeacherId);
        _courseRepositoryMock.Setup(x => x.GetByIdAsync(courseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(course);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You don't have permission to create teaching plans for this course");

        _teachingPlanRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TeachingPlan>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
