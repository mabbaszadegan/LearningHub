using System.Globalization;
using System.Linq;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Statistics;
using EduTrack.Application.Features.Statistics.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Statistics.QueryHandlers;

public class GetStudentLearningStatisticsQueryHandler : IRequestHandler<GetStudentLearningStatisticsQuery, Result<LearningStatisticsDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IScheduleItemBlockAttemptRepository _blockAttemptRepository;
    private readonly IScheduleItemBlockStatisticsRepository _blockStatisticsRepository;

    public GetStudentLearningStatisticsQueryHandler(
        IStudySessionRepository studySessionRepository,
        IScheduleItemRepository scheduleItemRepository,
        IScheduleItemBlockAttemptRepository blockAttemptRepository,
        IScheduleItemBlockStatisticsRepository blockStatisticsRepository)
    {
        _studySessionRepository = studySessionRepository;
        _scheduleItemRepository = scheduleItemRepository;
        _blockAttemptRepository = blockAttemptRepository;
        _blockStatisticsRepository = blockStatisticsRepository;
    }

    public async Task<Result<LearningStatisticsDto>> Handle(GetStudentLearningStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var completedSessions = (await _studySessionRepository
                    .GetCompletedSessionsByStudentAsync(request.StudentId, request.StudentProfileId))
                ?.Where(session => session.IsCompleted)
                .ToList() ?? new List<StudySession>();

            var blockAttempts = (await _blockAttemptRepository
                    .GetByStudentAsync(request.StudentId, request.StudentProfileId, cancellationToken))
                .ToList();

            var blockStatistics = (await _blockStatisticsRepository
                    .GetByStudentAsync(request.StudentId, request.StudentProfileId, cancellationToken))
                .ToList();

            var scheduleItemMap = await LoadScheduleItemsAsync(completedSessions, blockStatistics, cancellationToken);
            var culture = new CultureInfo("fa-IR");

            var (studySummary, weeklyChart, monthlyChart) = BuildStudyTimeSummaries(completedSessions, culture);
            var questionPerformance = BuildQuestionPerformance(blockAttempts);
            var recentTopics = BuildRecentTopics(completedSessions, scheduleItemMap, request.RecentTopicsLimit);
            var incorrectTopics = BuildTopicsWithMostErrors(blockStatistics, scheduleItemMap, request.MostIncorrectTopicsLimit);

            var response = new LearningStatisticsDto
            {
                StudyTimeSummary = studySummary,
                WeeklyChart = weeklyChart,
                MonthlyChart = monthlyChart,
                QuestionPerformance = questionPerformance,
                RecentTopics = recentTopics,
                MostIncorrectTopics = incorrectTopics
            };

            return Result<LearningStatisticsDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LearningStatisticsDto>.Failure($"خطا در دریافت آمار یادگیری دانش‌آموز: {ex.Message}");
        }
    }

    private async Task<Dictionary<int, ScheduleItem>> LoadScheduleItemsAsync(
        List<StudySession> sessions,
        List<ScheduleItemBlockStatistics> blockStatistics,
        CancellationToken cancellationToken)
    {
        var scheduleItemIds = sessions
            .Select(session => session.ScheduleItemId)
            .Concat(blockStatistics.Select(stat => stat.ScheduleItemId))
            .Distinct()
            .ToList();

        if (!scheduleItemIds.Any())
        {
            return new Dictionary<int, ScheduleItem>();
        }

        return await _scheduleItemRepository
            .GetAll()
            .Where(item => scheduleItemIds.Contains(item.Id))
            .Include(item => item.SubChapterAssignments)
                .ThenInclude(assignment => assignment.SubChapter)
                    .ThenInclude(subChapter => subChapter.Chapter)
            .ToDictionaryAsync(item => item.Id, cancellationToken);
    }

    private static (StudyTimeSummaryDto Summary, StudyChartDto WeeklyChart, StudyChartDto MonthlyChart) BuildStudyTimeSummaries(
        List<StudySession> sessions,
        CultureInfo culture)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var weekStart = todayUtc.AddDays(-6);
        var monthStart = todayUtc.AddDays(-29);

        var weekBuckets = Enumerable.Range(0, 7)
            .ToDictionary(offset => weekStart.AddDays(offset), _ => 0);

        var monthBuckets = Enumerable.Range(0, 30)
            .ToDictionary(offset => monthStart.AddDays(offset), _ => 0);

        foreach (var session in sessions)
        {
            var referenceDate = (session.EndedAt ?? session.StartedAt).UtcDateTime.Date;
            var minutes = ConvertSecondsToMinutes(session.DurationSeconds);

            if (referenceDate >= weekStart && referenceDate <= todayUtc)
            {
                weekBuckets[referenceDate] += minutes;
            }

            if (referenceDate >= monthStart && referenceDate <= todayUtc)
            {
                monthBuckets[referenceDate] += minutes;
            }
        }

        var summary = new StudyTimeSummaryDto
        {
            TodayMinutes = weekBuckets.TryGetValue(todayUtc, out var todayMinutes) ? todayMinutes : 0,
            WeekMinutes = weekBuckets.Sum(bucket => bucket.Value),
            MonthMinutes = monthBuckets.Sum(bucket => bucket.Value)
        };

        var weeklyChart = new StudyChartDto
        {
            RangeKey = "week",
            RangeTitle = "هفته اخیر",
            Points = weekBuckets
                .OrderBy(bucket => bucket.Key)
                .Select(bucket => new StudyChartPointDto
                {
                    Date = new DateTimeOffset(bucket.Key, TimeSpan.Zero),
                    Label = bucket.Key.ToString("ddd", culture),
                    Minutes = bucket.Value
                })
                .ToList()
        };

        var monthlyChart = new StudyChartDto
        {
            RangeKey = "month",
            RangeTitle = "ماه اخیر",
            Points = monthBuckets
                .OrderBy(bucket => bucket.Key)
                .Select(bucket => new StudyChartPointDto
                {
                    Date = new DateTimeOffset(bucket.Key, TimeSpan.Zero),
                    Label = bucket.Key.ToString("dd MMM", culture),
                    Minutes = bucket.Value
                })
                .ToList()
        };

        return (summary, weeklyChart, monthlyChart);
    }

    private static QuestionPerformanceDto BuildQuestionPerformance(List<ScheduleItemBlockAttempt> attempts)
    {
        var total = attempts.Count;
        var correct = attempts.Count(attempt => attempt.IsCorrect);
        var incorrect = total - correct;
        var accuracy = total > 0
            ? Math.Round((double)correct / total * 100, 1, MidpointRounding.AwayFromZero)
            : 0;

        return new QuestionPerformanceDto
        {
            TotalAnswered = total,
            CorrectAnswers = correct,
            IncorrectAnswers = incorrect,
            AccuracyPercentage = accuracy
        };
    }

    private static List<TopicStudyDto> BuildRecentTopics(
        List<StudySession> sessions,
        IReadOnlyDictionary<int, ScheduleItem> scheduleItems,
        int limit)
    {
        if (limit <= 0)
        {
            return new List<TopicStudyDto>();
        }

        var aggregates = new Dictionary<string, TopicStudyAggregate>();

        foreach (var session in sessions)
        {
            if (!scheduleItems.TryGetValue(session.ScheduleItemId, out var scheduleItem))
            {
                continue;
            }

            var minutes = ConvertSecondsToMinutes(session.DurationSeconds);
            var referenceTime = session.EndedAt ?? session.StartedAt;
            var assignments = scheduleItem.SubChapterAssignments;

            if (assignments != null && assignments.Any())
            {
                foreach (var assignment in assignments)
                {
                    var subChapter = assignment.SubChapter;
                    var chapterTitle = subChapter?.Chapter?.Title ?? scheduleItem.Title;
                    var subChapterTitle = subChapter?.Title ?? scheduleItem.Title;
                    var key = $"subchapter-{assignment.SubChapterId}";

                    var aggregate = GetOrCreateStudyAggregate(aggregates, key);
                    aggregate.SubChapterId = assignment.SubChapterId;
                    aggregate.ScheduleItemId = scheduleItem.Id;
                    aggregate.ChapterTitle = chapterTitle;
                    aggregate.SubChapterTitle = subChapterTitle;
                    aggregate.TotalMinutes += minutes;

                    if (referenceTime > aggregate.LastStudiedAt)
                    {
                        aggregate.LastStudiedAt = referenceTime;
                    }
                }
            }
            else
            {
                var key = $"schedule-{scheduleItem.Id}";
                var aggregate = GetOrCreateStudyAggregate(aggregates, key);
                aggregate.ScheduleItemId = scheduleItem.Id;
                aggregate.ChapterTitle = scheduleItem.Title;
                aggregate.SubChapterTitle = scheduleItem.Description ?? scheduleItem.Title;
                aggregate.TotalMinutes += minutes;

                if (referenceTime > aggregate.LastStudiedAt)
                {
                    aggregate.LastStudiedAt = referenceTime;
                }
            }
        }

        return aggregates.Values
            .OrderByDescending(aggregate => aggregate.LastStudiedAt)
            .ThenByDescending(aggregate => aggregate.TotalMinutes)
            .Take(limit)
            .Select(aggregate => new TopicStudyDto
            {
                SubChapterId = aggregate.SubChapterId,
                ScheduleItemId = aggregate.ScheduleItemId,
                ChapterTitle = aggregate.ChapterTitle,
                SubChapterTitle = aggregate.SubChapterTitle,
                LastStudiedAt = aggregate.LastStudiedAt,
                TotalMinutes = aggregate.TotalMinutes
            })
            .ToList();
    }

    private static List<TopicErrorDto> BuildTopicsWithMostErrors(
        List<ScheduleItemBlockStatistics> blockStatistics,
        IReadOnlyDictionary<int, ScheduleItem> scheduleItems,
        int limit)
    {
        if (limit <= 0)
        {
            return new List<TopicErrorDto>();
        }

        var aggregates = new Dictionary<string, TopicErrorAggregate>();

        foreach (var stat in blockStatistics)
        {
            if (!scheduleItems.TryGetValue(stat.ScheduleItemId, out var scheduleItem))
            {
                continue;
            }

            var assignments = scheduleItem.SubChapterAssignments;

            if (assignments != null && assignments.Any())
            {
                foreach (var assignment in assignments)
                {
                    AddOrUpdateErrorAggregate(
                        aggregates,
                        $"subchapter-{assignment.SubChapterId}",
                        assignment.SubChapter,
                        scheduleItem,
                        stat);
                }
            }
            else
            {
                AddOrUpdateErrorAggregate(
                    aggregates,
                    $"schedule-{scheduleItem.Id}",
                    null,
                    scheduleItem,
                    stat);
            }
        }

        return aggregates.Values
            .Where(aggregate => aggregate.IncorrectAttempts > 0)
            .OrderByDescending(aggregate => aggregate.IncorrectAttempts)
            .ThenBy(aggregate => aggregate.SuccessRate)
            .Take(limit)
            .Select(aggregate => new TopicErrorDto
            {
                SubChapterId = aggregate.SubChapterId,
                ScheduleItemId = aggregate.ScheduleItemId,
                ChapterTitle = aggregate.ChapterTitle,
                SubChapterTitle = aggregate.SubChapterTitle,
                IncorrectAttempts = aggregate.IncorrectAttempts,
                CorrectAttempts = aggregate.CorrectAttempts,
                TotalAttempts = aggregate.TotalAttempts,
                SuccessRate = aggregate.SuccessRate
            })
            .ToList();
    }

    private static TopicStudyAggregate GetOrCreateStudyAggregate(
        IDictionary<string, TopicStudyAggregate> aggregates,
        string key)
    {
        if (!aggregates.TryGetValue(key, out var aggregate))
        {
            aggregate = new TopicStudyAggregate();
            aggregates[key] = aggregate;
        }

        return aggregate;
    }

    private static void AddOrUpdateErrorAggregate(
        IDictionary<string, TopicErrorAggregate> aggregates,
        string key,
        SubChapter? subChapter,
        ScheduleItem scheduleItem,
        ScheduleItemBlockStatistics stat)
    {
        if (!aggregates.TryGetValue(key, out var aggregate))
        {
            aggregate = new TopicErrorAggregate
            {
                SubChapterId = subChapter?.Id,
                ScheduleItemId = scheduleItem.Id
            };

            aggregates[key] = aggregate;
        }

        aggregate.SubChapterId ??= subChapter?.Id;
        aggregate.ScheduleItemId ??= scheduleItem.Id;
        aggregate.ChapterTitle = subChapter?.Chapter?.Title ?? scheduleItem.Title;
        aggregate.SubChapterTitle = subChapter?.Title ?? scheduleItem.Title;
        aggregate.IncorrectAttempts += stat.IncorrectAttempts;
        aggregate.CorrectAttempts += stat.CorrectAttempts;
        aggregate.TotalAttempts += stat.TotalAttempts;
    }

    private static int ConvertSecondsToMinutes(int seconds)
    {
        if (seconds <= 0)
        {
            return 0;
        }

        var minutes = (int)Math.Round(seconds / 60d, MidpointRounding.AwayFromZero);
        return Math.Max(minutes, 1);
    }

    private sealed class TopicStudyAggregate
    {
        public int? SubChapterId { get; set; }
        public int? ScheduleItemId { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
        public string SubChapterTitle { get; set; } = string.Empty;
        public DateTimeOffset LastStudiedAt { get; set; } = DateTimeOffset.MinValue;
        public int TotalMinutes { get; set; }
    }

    private sealed class TopicErrorAggregate
    {
        public int? SubChapterId { get; set; }
        public int? ScheduleItemId { get; set; }
        public string ChapterTitle { get; set; } = string.Empty;
        public string SubChapterTitle { get; set; } = string.Empty;
        public int IncorrectAttempts { get; set; }
        public int CorrectAttempts { get; set; }
        public int TotalAttempts { get; set; }
        public decimal SuccessRate => TotalAttempts > 0
            ? Math.Round((decimal)CorrectAttempts / TotalAttempts * 100, 1, MidpointRounding.AwayFromZero)
            : 0;
    }
}

