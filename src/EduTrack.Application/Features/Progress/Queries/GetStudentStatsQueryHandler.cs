using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Progress.Queries;

public class GetStudentStatsQueryHandler : IRequestHandler<GetStudentStatsQuery, Result<StudentStatsDto>>
{
    private readonly IRepository<EduTrack.Domain.Entities.Progress> _progressRepository;
    private readonly IRepository<Attempt> _attemptRepository;
    private readonly IUserService _userService;

    public GetStudentStatsQueryHandler(
        IRepository<EduTrack.Domain.Entities.Progress> progressRepository,
        IRepository<Attempt> attemptRepository,
        IUserService userService)
    {
        _progressRepository = progressRepository;
        _attemptRepository = attemptRepository;
        _userService = userService;
    }

    public async Task<Result<StudentStatsDto>> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
    {
        var student = await _userService.GetUserByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            return Result<StudentStatsDto>.Failure("Student not found");
        }

        var progresses = await _progressRepository.GetAll()
            .Where(p => p.StudentId == request.StudentId)
            .ToListAsync(cancellationToken);

        var attempts = await _attemptRepository.GetAll()
            .Where(a => a.StudentId == request.StudentId)
            .ToListAsync(cancellationToken);

        var lessonProgresses = progresses.Where(p => p.LessonId.HasValue).ToList();
        var examProgresses = progresses.Where(p => p.ExamId.HasValue).ToList();

        var completedLessons = lessonProgresses.Count(p => p.Status == ProgressStatus.Done);
        var completedExams = examProgresses.Count(p => p.Status == ProgressStatus.Done);
        var passedExams = attempts.Count(a => a.IsPassed);

        var averageScore = attempts.Any() ? attempts.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0;

        var currentStreak = CalculateCurrentStreak(progresses);
        var longestStreak = CalculateLongestStreak(progresses);

        var overallStatus = CalculateOverallStatus(progresses);

        var stats = new StudentStatsDto
        {
            StudentId = request.StudentId,
            StudentName = student.FullName,
            TotalLessons = lessonProgresses.Count,
            CompletedLessons = completedLessons,
            TotalExams = examProgresses.Count,
            CompletedExams = completedExams,
            PassedExams = passedExams,
            AverageScore = averageScore,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            OverallStatus = overallStatus
        };

        return Result<StudentStatsDto>.Success(stats);
    }

    private static int CalculateCurrentStreak(List<EduTrack.Domain.Entities.Progress> progresses)
    {
        var recentProgresses = progresses
            .Where(p => p.Status == ProgressStatus.Done)
            .OrderByDescending(p => p.CompletedAt)
            .ToList();

        int streak = 0;
        DateTimeOffset? lastDate = null;

        foreach (var progress in recentProgresses)
        {
            if (!progress.CompletedAt.HasValue) break;

            if (lastDate == null)
            {
                streak = 1;
                lastDate = progress.CompletedAt.Value.Date;
            }
            else
            {
                var daysDiff = (lastDate.Value.Date - progress.CompletedAt.Value.Date).Days;
                if (daysDiff <= 1)
                {
                    streak++;
                    lastDate = progress.CompletedAt.Value.Date;
                }
                else
                {
                    break;
                }
            }
        }

        return streak;
    }

    private static int CalculateLongestStreak(List<EduTrack.Domain.Entities.Progress> progresses)
    {
        var completedProgresses = progresses
            .Where(p => p.Status == ProgressStatus.Done && p.CompletedAt.HasValue)
            .OrderBy(p => p.CompletedAt)
            .ToList();

        if (!completedProgresses.Any()) return 0;

        int maxStreak = 1;
        int currentStreak = 1;
        DateTimeOffset lastDate = completedProgresses.First().CompletedAt!.Value.Date;

        for (int i = 1; i < completedProgresses.Count; i++)
        {
            var currentDate = completedProgresses[i].CompletedAt!.Value.Date;
            var daysDiff = (currentDate - lastDate).Days;

            if (daysDiff <= 1)
            {
                currentStreak++;
                maxStreak = Math.Max(maxStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }

            lastDate = currentDate;
        }

        return maxStreak;
    }

    private static ProgressStatus CalculateOverallStatus(List<EduTrack.Domain.Entities.Progress> progresses)
    {
        if (!progresses.Any()) return ProgressStatus.NotStarted;

        var completedCount = progresses.Count(p => p.Status == ProgressStatus.Done);
        var inProgressCount = progresses.Count(p => p.Status == ProgressStatus.InProgress);

        if (completedCount == progresses.Count) return ProgressStatus.Done;
        if (inProgressCount > 0 || completedCount > 0) return ProgressStatus.InProgress;

        return ProgressStatus.NotStarted;
    }
}
