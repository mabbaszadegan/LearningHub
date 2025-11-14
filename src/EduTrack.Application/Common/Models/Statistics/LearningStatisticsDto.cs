using System.Linq;

namespace EduTrack.Application.Common.Models.Statistics;

public class LearningStatisticsDto
{
    public StudyTimeSummaryDto StudyTimeSummary { get; init; } = new();
    public StudyChartDto WeeklyChart { get; init; } = StudyChartDto.CreateEmpty("week", "هفته اخیر");
    public StudyChartDto MonthlyChart { get; init; } = StudyChartDto.CreateEmpty("month", "ماه اخیر");
    public QuestionPerformanceDto QuestionPerformance { get; init; } = new();
    public List<TopicStudyDto> RecentTopics { get; init; } = new();
    public List<TopicErrorDto> MostIncorrectTopics { get; init; } = new();
}

public class StudyTimeSummaryDto
{
    public int TodayMinutes { get; init; }
    public int WeekMinutes { get; init; }
    public int MonthMinutes { get; init; }
}

public class StudyChartDto
{
    public string RangeKey { get; init; } = string.Empty;
    public string RangeTitle { get; init; } = string.Empty;
    public List<StudyChartPointDto> Points { get; init; } = new();

    public int TotalMinutes => Points.Sum(point => point.Minutes);

    public static StudyChartDto CreateEmpty(string key, string title)
    {
        return new StudyChartDto
        {
            RangeKey = key,
            RangeTitle = title,
            Points = new List<StudyChartPointDto>()
        };
    }
}

public class StudyChartPointDto
{
    public DateTimeOffset Date { get; init; }
    public string Label { get; init; } = string.Empty;
    public int Minutes { get; init; }
}

public class QuestionPerformanceDto
{
    public int TotalAnswered { get; init; }
    public int CorrectAnswers { get; init; }
    public int IncorrectAnswers { get; init; }
    public double AccuracyPercentage { get; init; }
}

public class TopicStudyDto
{
    public int? SubChapterId { get; init; }
    public int? ScheduleItemId { get; init; }
    public string ChapterTitle { get; init; } = string.Empty;
    public string SubChapterTitle { get; init; } = string.Empty;
    public DateTimeOffset LastStudiedAt { get; init; }
    public int TotalMinutes { get; init; }
}

public class TopicErrorDto
{
    public int? SubChapterId { get; init; }
    public int? ScheduleItemId { get; init; }
    public string ChapterTitle { get; init; } = string.Empty;
    public string SubChapterTitle { get; init; } = string.Empty;
    public int IncorrectAttempts { get; init; }
    public int CorrectAttempts { get; init; }
    public int TotalAttempts { get; init; }
    public decimal SuccessRate { get; init; }
}

