using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.EducationalContent.Queries;
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
            EducationalContentId = studySession.EducationalContentId,
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
public class GetActiveStudySessionQueryHandler : IRequestHandler<GetActiveStudySessionQuery, Result<StudySessionDto?>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetActiveStudySessionQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto?>> Handle(GetActiveStudySessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var studySession = await _studySessionRepository.GetActiveSessionAsync(request.StudentId, request.EducationalContentId);
            
            if (studySession == null)
            {
                return Result<StudySessionDto?>.Success(null);
            }

            return Result<StudySessionDto?>.Success(MapToDto(studySession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto?>.Failure($"خطا در دریافت جلسه فعال مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            EducationalContentId = studySession.EducationalContentId,
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
/// Handler for getting study sessions by student and content
/// </summary>
public class GetStudySessionsByStudentAndContentQueryHandler : IRequestHandler<GetStudySessionsByStudentAndContentQuery, Result<IEnumerable<StudySessionDto>>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public GetStudySessionsByStudentAndContentQueryHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<IEnumerable<StudySessionDto>>> Handle(GetStudySessionsByStudentAndContentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var studySessions = await _studySessionRepository.GetByStudentAndContentAsync(request.StudentId, request.EducationalContentId);
            var dtos = studySessions.Select(MapToDto);

            return Result<IEnumerable<StudySessionDto>>.Success(dtos);
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
            EducationalContentId = studySession.EducationalContentId,
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
            var totalStudyTime = await _studySessionRepository.GetTotalStudyTimeAsync(request.StudentId, request.EducationalContentId);
            var sessionsCount = await _studySessionRepository.GetStudySessionsCountAsync(request.StudentId, request.EducationalContentId);
            var recentSessions = await _studySessionRepository.GetRecentSessionsAsync(request.StudentId, 5);

            var statistics = new StudySessionStatisticsDto
            {
                TotalStudyTimeSeconds = totalStudyTime,
                StudySessionsCount = sessionsCount,
                RecentSessions = recentSessions
                    .Where(s => s.EducationalContentId == request.EducationalContentId)
                    .Select(MapToDto)
                    .ToList()
            };

            if (statistics.RecentSessions.Any())
            {
                statistics.LastStudyDate = statistics.RecentSessions.Max(s => s.EndedAt);
            }

            return Result<StudySessionStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<StudySessionStatisticsDto>.Failure($"خطا در دریافت آمار جلسات مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(Domain.Entities.StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            EducationalContentId = studySession.EducationalContentId,
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
/// Handler for getting educational content with study statistics
/// </summary>
public class GetEducationalContentWithStudyStatsQueryHandler : IRequestHandler<GetEducationalContentWithStudyStatsQuery, Result<EducationalContentWithStudyStatsDto>>
{
    private readonly IMediator _mediator;
    private readonly IStudySessionRepository _studySessionRepository;

    public GetEducationalContentWithStudyStatsQueryHandler(IMediator mediator, IStudySessionRepository studySessionRepository)
    {
        _mediator = mediator;
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<EducationalContentWithStudyStatsDto>> Handle(GetEducationalContentWithStudyStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get educational content
            var contentResult = await _mediator.Send(new GetEducationalContentByIdQuery(request.EducationalContentId));
            if (!contentResult.IsSuccess || contentResult.Value == null)
            {
                return Result<EducationalContentWithStudyStatsDto>.Failure("محتوا یافت نشد");
            }

            var content = contentResult.Value;

            // Get study statistics
            var statisticsResult = await _mediator.Send(new GetStudySessionStatisticsQuery(request.StudentId, request.EducationalContentId));
            if (!statisticsResult.IsSuccess)
            {
                return Result<EducationalContentWithStudyStatsDto>.Failure("خطا در دریافت آمار مطالعه");
            }

            var result = new EducationalContentWithStudyStatsDto
            {
                Id = content.Id,
                SubChapterId = content.SubChapterId,
                Title = content.Title,
                Description = content.Description,
                Type = content.Type,
                TextContent = content.TextContent,
                FileId = content.FileId,
                ExternalUrl = content.ExternalUrl,
                IsActive = content.IsActive,
                Order = content.Order,
                CreatedAt = content.CreatedAt,
                UpdatedAt = content.UpdatedAt,
                CreatedBy = content.CreatedBy,
                File = content.File,
                StudyStatistics = statisticsResult.Value!
            };

            return Result<EducationalContentWithStudyStatsDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<EducationalContentWithStudyStatsDto>.Failure($"خطا در دریافت محتوا با آمار مطالعه: {ex.Message}");
        }
    }
}
