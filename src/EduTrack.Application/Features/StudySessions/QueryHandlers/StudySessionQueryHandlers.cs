using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.StudySessions.QueryHandlers;

/// <summary>
/// Handler for getting study session by ID
/// </summary>
public class GetStudySessionByIdQueryHandler : IRequestHandler<GetStudySessionByIdQuery, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetStudySessionByIdQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(GetStudySessionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var studySession = await _studySessionRepository.GetByIdAsync(request.Id);
            if (studySession == null)
            {
                return Result<StudySessionDto>.Failure("جلسه مطالعه یافت نشد");
            }

            return Result<StudySessionDto>.Success(MapToDto(studySession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در دریافت جلسه مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            ScheduleItemId = studySession.ScheduleItemId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting active study session
/// </summary>
public class GetActiveStudySessionQueryHandler : IRequestHandler<GetActiveStudySessionQuery, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetActiveStudySessionQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(GetActiveStudySessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var activeSession = await _studySessionRepository.GetActiveSessionAsync(request.StudentId, request.ScheduleItemId);
            if (activeSession == null)
            {
                return Result<StudySessionDto>.Failure("جلسه فعال یافت نشد");
            }

            return Result<StudySessionDto>.Success(MapToDto(activeSession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در دریافت جلسه فعال: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            ScheduleItemId = studySession.ScheduleItemId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting study sessions by student and schedule item
/// </summary>
public class GetStudySessionsByStudentAndScheduleItemQueryHandler : IRequestHandler<GetStudySessionsByStudentAndScheduleItemQuery, Result<IEnumerable<StudySessionDto>>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetStudySessionsByStudentAndScheduleItemQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<IEnumerable<StudySessionDto>>> Handle(GetStudySessionsByStudentAndScheduleItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var sessions = await _studySessionRepository.GetByStudentAndScheduleItemAsync(request.StudentId, request.ScheduleItemId);
            var sessionDtos = sessions.Select(MapToDto);

            return Result<IEnumerable<StudySessionDto>>.Success(sessionDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<StudySessionDto>>.Failure($"خطا در دریافت جلسات مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            ScheduleItemId = studySession.ScheduleItemId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting study session statistics
/// </summary>
public class GetStudySessionStatisticsQueryHandler : IRequestHandler<GetStudySessionStatisticsQuery, Result<StudySessionStatisticsDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetStudySessionStatisticsQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionStatisticsDto>> Handle(GetStudySessionStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var totalTime = await _studySessionRepository.GetTotalStudyTimeAsync(request.StudentId, request.ScheduleItemId);
            var sessionsCount = await _studySessionRepository.GetStudySessionsCountAsync(request.StudentId, request.ScheduleItemId);
            var recentSessions = await _studySessionRepository.GetRecentSessionsAsync(request.StudentId, 5);

            var lastStudyDate = recentSessions.FirstOrDefault()?.EndedAt;

            var statistics = new StudySessionStatisticsDto
            {
                TotalStudyTimeSeconds = totalTime,
                StudySessionsCount = sessionsCount,
                LastStudyDate = lastStudyDate,
                RecentSessions = recentSessions.Select(MapToDto).ToList()
            };

            return Result<StudySessionStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<StudySessionStatisticsDto>.Failure($"خطا در دریافت آمار مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            ScheduleItemId = studySession.ScheduleItemId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting schedule item with study statistics
/// </summary>
public class GetScheduleItemWithStudyStatsQueryHandler : IRequestHandler<GetScheduleItemWithStudyStatsQuery, Result<ScheduleItemWithStudyStatsDto>>
{
    private readonly IMediator _mediator;
    private readonly IStudySessionRepository _studySessionRepository;

    public GetScheduleItemWithStudyStatsQueryHandler(IMediator mediator, IStudySessionRepository studySessionRepository)
    {
        _mediator = mediator;
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<ScheduleItemWithStudyStatsDto>> Handle(GetScheduleItemWithStudyStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get schedule item
            var scheduleItemResult = await _mediator.Send(new GetScheduleItemByIdQuery(request.ScheduleItemId));
            if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
            {
                return Result<ScheduleItemWithStudyStatsDto>.Failure("آیتم آموزشی یافت نشد");
            }

            var scheduleItem = scheduleItemResult.Value;

            // Get study statistics
            var statisticsResult = await _mediator.Send(new GetStudySessionStatisticsQuery(request.StudentId, request.ScheduleItemId));
            if (!statisticsResult.IsSuccess)
            {
                return Result<ScheduleItemWithStudyStatsDto>.Failure(statisticsResult.Error ?? "خطا در دریافت آمار مطالعه");
            }

            var statistics = statisticsResult.Value!;

            // Map to DTO
            var result = new ScheduleItemWithStudyStatsDto
            {
                Id = scheduleItem.Id,
                TeachingPlanId = scheduleItem.TeachingPlanId,
                Title = scheduleItem.Title,
                Description = scheduleItem.Description,
                ContentJson = scheduleItem.ContentJson,
                Type = scheduleItem.Type,
                IsActive = true, // Default value since ScheduleItemDto doesn't have this property
                Order = 0, // Default value since ScheduleItemDto doesn't have this property
                CreatedAt = scheduleItem.CreatedAt,
                UpdatedAt = scheduleItem.UpdatedAt,
                CreatedBy = "System", // Default value since ScheduleItemDto doesn't have this property
                StudyStatistics = statistics
            };

            return Result<ScheduleItemWithStudyStatsDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<ScheduleItemWithStudyStatsDto>.Failure($"خطا در دریافت آیتم آموزشی: {ex.Message}");
        }
    }
}