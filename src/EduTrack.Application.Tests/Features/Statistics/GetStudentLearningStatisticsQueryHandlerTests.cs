using EduTrack.Application.Features.Statistics.QueryHandlers;
using EduTrack.Application.Features.Statistics.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using EduTrack.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EduTrack.Application.Tests.Features.Statistics;

public class GetStudentLearningStatisticsQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly ScheduleItemRepository _scheduleItemRepository;
    private readonly Mock<IStudySessionRepository> _studySessionRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IScheduleItemBlockAttemptRepository> _blockAttemptRepositoryMock = new(MockBehavior.Strict);
    private readonly Mock<IScheduleItemBlockStatisticsRepository> _blockStatisticsRepositoryMock = new(MockBehavior.Strict);

    public GetStudentLearningStatisticsQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _scheduleItemRepository = new ScheduleItemRepository(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldAggregateStudyDurationsAndQuestionStats()
    {
        // Arrange
        const string studentId = "student-1";
        var (scheduleItemId, subChapterTitle, chapterTitle) = await SeedScheduleItemAsync();

        var sessions = new List<StudySession>
        {
            StudySession.CreateCompleted(studentId, scheduleItemId, DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1)),
            StudySession.CreateCompleted(studentId, scheduleItemId, DateTimeOffset.UtcNow.AddDays(-1).AddHours(-1), DateTimeOffset.UtcNow.AddDays(-1))
        };

        _studySessionRepositoryMock
            .Setup(repo => repo.GetCompletedSessionsByStudentAsync(studentId, null))
            .ReturnsAsync(sessions);

        var blockAttempts = new List<ScheduleItemBlockAttempt>
        {
            ScheduleItemBlockAttempt.Create(scheduleItemId, ScheduleItemType.Reminder, "block-1", studentId, "{}", "{}", true, 1, 1),
            ScheduleItemBlockAttempt.Create(scheduleItemId, ScheduleItemType.Reminder, "block-1", studentId, "{}", "{}", false, 0, 1)
        };

        _blockAttemptRepositoryMock
            .Setup(repo => repo.GetByStudentAsync(studentId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blockAttempts);

        var blockStat = ScheduleItemBlockStatistics.Create(scheduleItemId, ScheduleItemType.Reminder, "block-1", studentId);
        blockStat.RecordAttempt(false, DateTimeOffset.UtcNow.AddHours(-3));
        blockStat.RecordAttempt(true, DateTimeOffset.UtcNow.AddHours(-2));

        _blockStatisticsRepositoryMock
            .Setup(repo => repo.GetByStudentAsync(studentId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScheduleItemBlockStatistics> { blockStat });

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetStudentLearningStatisticsQuery(studentId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        var stats = result.Value!;

        stats.StudyTimeSummary.TodayMinutes.Should().BeGreaterThan(0);
        stats.StudyTimeSummary.WeekMinutes.Should().BeGreaterThan(stats.StudyTimeSummary.TodayMinutes - 1);

        stats.QuestionPerformance.TotalAnswered.Should().Be(2);
        stats.QuestionPerformance.CorrectAnswers.Should().Be(1);
        stats.QuestionPerformance.IncorrectAnswers.Should().Be(1);

        stats.RecentTopics.Should().NotBeEmpty();
        stats.RecentTopics.First().SubChapterTitle.Should().Be(subChapterTitle);
        stats.RecentTopics.First().ChapterTitle.Should().Be(chapterTitle);

        stats.MostIncorrectTopics.Should().NotBeEmpty();
        stats.MostIncorrectTopics.First().IncorrectAttempts.Should().BeGreaterThan(0);

        _studySessionRepositoryMock.VerifyAll();
        _blockAttemptRepositoryMock.VerifyAll();
        _blockStatisticsRepositoryMock.VerifyAll();
    }

    private GetStudentLearningStatisticsQueryHandler CreateHandler()
    {
        return new GetStudentLearningStatisticsQueryHandler(
            _studySessionRepositoryMock.Object,
            _scheduleItemRepository,
            _blockAttemptRepositoryMock.Object,
            _blockStatisticsRepositoryMock.Object);
    }

    private async Task<(int scheduleItemId, string subChapterTitle, string chapterTitle)> SeedScheduleItemAsync()
    {
        var chapter = Chapter.Create(1, "فصل اول", "توضیح", "هدف", 1);
        _dbContext.Chapters.Add(chapter);
        await _dbContext.SaveChangesAsync();

        var subChapter = SubChapter.Create(chapter.Id, "مبحث اول", "توضیح", "هدف", 1);
        _dbContext.SubChapters.Add(subChapter);
        await _dbContext.SaveChangesAsync();

        var scheduleItem = ScheduleItem.Create(
            teachingPlanId: 1,
            type: ScheduleItemType.Reminder,
            title: "درس نمونه",
            description: "توضیح",
            startDate: DateTimeOffset.UtcNow,
            dueDate: DateTimeOffset.UtcNow.AddDays(1),
            isMandatory: true,
            contentJson: "{}",
            maxScore: null,
            groupId: null,
            lessonId: null,
            disciplineHint: null,
            courseId: 1,
            sessionReportId: null);

        _dbContext.ScheduleItems.Add(scheduleItem);
        await _dbContext.SaveChangesAsync();

        var assignment = ScheduleItemSubChapterAssignment.Create(scheduleItem.Id, subChapter.Id);
        _dbContext.Set<ScheduleItemSubChapterAssignment>().Add(assignment);
        await _dbContext.SaveChangesAsync();

        return (scheduleItem.Id, subChapter.Title, chapter.Title);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

