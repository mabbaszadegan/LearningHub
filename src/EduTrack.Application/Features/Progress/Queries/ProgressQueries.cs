using EduTrack.Application.Common.Models;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.Progress.Queries;

public record GetProgressByStudentQuery(
    string StudentId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<ProgressDto>>;

public record GetProgressByLessonQuery(
    string StudentId,
    int LessonId) : IRequest<Result<ProgressDto>>;

public record GetProgressByExamQuery(
    string StudentId,
    int ExamId) : IRequest<Result<ProgressDto>>;

public record GetStudentStatsQuery(string StudentId) : IRequest<Result<StudentStatsDto>>;

public record StudentStatsDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalExams { get; set; }
    public int CompletedExams { get; set; }
    public int PassedExams { get; set; }
    public double AverageScore { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public ProgressStatus OverallStatus { get; set; }
}
