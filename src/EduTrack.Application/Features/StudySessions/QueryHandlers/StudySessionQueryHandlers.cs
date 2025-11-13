using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Features.StudySessions.Queries;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
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
            StudentProfileId = studySession.StudentProfileId,
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
            var activeSession = await _studySessionRepository.GetActiveSessionAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.StudentProfileId);
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
            StudentProfileId = studySession.StudentProfileId,
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
            var sessions = await _studySessionRepository.GetByStudentAndScheduleItemAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.StudentProfileId);
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
            StudentProfileId = studySession.StudentProfileId,
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
            var totalTime = await _studySessionRepository.GetTotalStudyTimeAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.StudentProfileId);
            var sessionsCount = await _studySessionRepository.GetStudySessionsCountAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.StudentProfileId);
            
            // Get all sessions for this specific schedule item
            var allItemSessions = await _studySessionRepository.GetByStudentAndScheduleItemAsync(
                request.StudentId,
                request.ScheduleItemId,
                request.StudentProfileId);
            var itemRecentSessions = allItemSessions
                .Where(s => s.IsCompleted)
                .OrderByDescending(s => s.EndedAt ?? s.StartedAt)
                .Take(5)
                .ToList();

            var lastStudyDate = itemRecentSessions.FirstOrDefault()?.EndedAt ?? itemRecentSessions.FirstOrDefault()?.StartedAt;

            var statistics = new StudySessionStatisticsDto
            {
                TotalStudyTimeSeconds = totalTime,
                StudySessionsCount = sessionsCount,
                LastStudyDate = lastStudyDate,
                RecentSessions = itemRecentSessions.Select(MapToDto).ToList()
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
            StudentProfileId = studySession.StudentProfileId,
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
/// Handler for getting the last study sessions for a student
/// </summary>
public class GetLastStudySessionsQueryHandler : IRequestHandler<GetLastStudySessionsQuery, Result<List<StudySessionHistoryDto>>>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IMediator _mediator;

    public GetLastStudySessionsQueryHandler(IStudySessionRepository studySessionRepository, IMediator mediator)
    {
        _studySessionRepository = studySessionRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<StudySessionHistoryDto>>> Handle(GetLastStudySessionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the most recent study sessions for the student
            var recentSessions = await _studySessionRepository.GetRecentSessionsAsync(
                request.StudentId,
                request.StudentProfileId,
                request.Count);
            
            if (!recentSessions.Any())
            {
                return Result<List<StudySessionHistoryDto>>.Success(new List<StudySessionHistoryDto>());
            }

            var studyHistoryDtos = new List<StudySessionHistoryDto>();

            foreach (var session in recentSessions)
            {
                // Get schedule item details
                var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(session.ScheduleItemId));
                if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
                    continue;

                var scheduleItem = scheduleItemResult.Value;

                // Get teaching plan to get course ID
                TeachingPlanDto? teachingPlan = null;
                int? courseId = scheduleItem.CourseId;

                if (scheduleItem.TeachingPlanId.HasValue)
                {
                    var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.TeachingPlanId.Value));
                    if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
                    {
                        teachingPlan = teachingPlanResult.Value;
                        courseId ??= teachingPlan.CourseId;
                    }
                    else if (!courseId.HasValue)
                    {
                        continue;
                    }
                }

                if (!courseId.HasValue)
                {
                    continue;
                }

                // Get course information
                var courseResult = await _mediator.Send(new EduTrack.Application.Features.Courses.Queries.GetCourseByIdQuery(courseId.Value));
                var courseTitle = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Title : "دوره آموزشی";
                var courseThumbnail = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Thumbnail : null;

                var studyHistoryDto = new StudySessionHistoryDto
                {
                    Id = session.Id,
                    StudentId = session.StudentId,
                    ScheduleItemId = session.ScheduleItemId,
                    StudentProfileId = session.StudentProfileId,
                    ScheduleItemTitle = scheduleItem.Title,
                    ScheduleItemDescription = scheduleItem.Description,
                    CourseId = courseId.Value,
                    CourseTitle = courseTitle,
                    CourseThumbnail = courseThumbnail,
                    StartedAt = session.StartedAt,
                    EndedAt = session.EndedAt,
                    DurationSeconds = session.DurationSeconds,
                    IsCompleted = session.IsCompleted,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt
                };

                studyHistoryDtos.Add(studyHistoryDto);
            }

            return Result<List<StudySessionHistoryDto>>.Success(studyHistoryDtos);
        }
        catch (Exception ex)
        {
            return Result<List<StudySessionHistoryDto>>.Failure($"خطا در دریافت تاریخچه مطالعه: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for getting all study sessions for a student (for statistics)
/// </summary>
public class GetAllStudySessionsQueryHandler : IRequestHandler<GetAllStudySessionsQuery, Result<List<StudySessionHistoryDto>>>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IMediator _mediator;

    public GetAllStudySessionsQueryHandler(IStudySessionRepository studySessionRepository, IMediator mediator)
    {
        _studySessionRepository = studySessionRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<StudySessionHistoryDto>>> Handle(GetAllStudySessionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all study sessions for the student
            var allSessions = await _studySessionRepository.GetByStudentIdAsync(
                request.StudentId,
                request.StudentProfileId);
            
            if (!allSessions.Any())
            {
                return Result<List<StudySessionHistoryDto>>.Success(new List<StudySessionHistoryDto>());
            }

            var studyHistoryDtos = new List<StudySessionHistoryDto>();

            foreach (var session in allSessions)
            {
                // Get schedule item to find course
                var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(session.ScheduleItemId));
                if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
                    continue;

                var scheduleItem = scheduleItemResult.Value;

                // Get teaching plan to get course ID
                TeachingPlanDto? teachingPlan = null;
                int? courseId = scheduleItem.CourseId;

                if (scheduleItem.TeachingPlanId.HasValue)
                {
                    var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.TeachingPlanId.Value));
                    if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
                    {
                        teachingPlan = teachingPlanResult.Value;
                        courseId ??= teachingPlan.CourseId;
                    }
                    else if (!courseId.HasValue)
                    {
                        continue;
                    }
                }

                if (!courseId.HasValue)
                {
                    continue;
                }

                // Get course information
                var courseResult = await _mediator.Send(new EduTrack.Application.Features.Courses.Queries.GetCourseByIdQuery(courseId.Value));
                var courseTitle = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Title : "دوره آموزشی";
                var courseThumbnail = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Thumbnail : null;

                var studyHistoryDto = new StudySessionHistoryDto
                {
                    Id = session.Id,
                    StudentId = session.StudentId,
                    ScheduleItemId = session.ScheduleItemId,
                    StudentProfileId = session.StudentProfileId,
                    ScheduleItemTitle = scheduleItem.Title,
                    ScheduleItemDescription = scheduleItem.Description,
                    CourseId = courseId.Value,
                    CourseTitle = courseTitle,
                    CourseThumbnail = courseThumbnail,
                    StartedAt = session.StartedAt,
                    EndedAt = session.EndedAt,
                    DurationSeconds = session.DurationSeconds,
                    IsCompleted = session.IsCompleted,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt
                };

                studyHistoryDtos.Add(studyHistoryDto);
            }

            return Result<List<StudySessionHistoryDto>>.Success(studyHistoryDtos);
        }
        catch (Exception ex)
        {
            return Result<List<StudySessionHistoryDto>>.Failure($"خطا در دریافت تمام جلسات مطالعه: {ex.Message}");
        }
    }
}

/// <summary>
/// Handler for getting the last courses with study activity for a student
/// </summary>
public class GetLastStudyCoursesQueryHandler : IRequestHandler<GetLastStudyCoursesQuery, Result<List<CourseStudyHistoryDto>>>
{
    private readonly IStudySessionRepository _studySessionRepository;
    private readonly IMediator _mediator;

    public GetLastStudyCoursesQueryHandler(IStudySessionRepository studySessionRepository, IMediator mediator)
    {
        _studySessionRepository = studySessionRepository;
        _mediator = mediator;
    }

    public async Task<Result<List<CourseStudyHistoryDto>>> Handle(GetLastStudyCoursesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all study sessions for the student
            var allSessions = await _studySessionRepository.GetByStudentIdAsync(
                request.StudentId,
                request.StudentProfileId);
            
            if (!allSessions.Any())
            {
                return Result<List<CourseStudyHistoryDto>>.Success(new List<CourseStudyHistoryDto>());
            }

            // Group sessions by course and get course statistics
            var courseGroups = new Dictionary<int, List<Domain.Entities.StudySession>>();
            
            foreach (var session in allSessions)
            {
                // Get schedule item to find course
                var scheduleItemResult = await _mediator.Send(new EduTrack.Application.Features.ScheduleItems.Queries.GetScheduleItemByIdQuery(session.ScheduleItemId));
                if (!scheduleItemResult.IsSuccess || scheduleItemResult.Value == null)
                    continue;

                var scheduleItem = scheduleItemResult.Value;

                // Get teaching plan to get course ID
                int? courseId = scheduleItem.CourseId;
                if (scheduleItem.TeachingPlanId.HasValue)
                {
                    var teachingPlanResult = await _mediator.Send(new GetTeachingPlanByIdQuery(scheduleItem.TeachingPlanId.Value));
                    if (teachingPlanResult.IsSuccess && teachingPlanResult.Value != null)
                    {
                        courseId ??= teachingPlanResult.Value.CourseId;
                    }
                    else if (!courseId.HasValue)
                    {
                        continue;
                    }
                }

                if (!courseId.HasValue)
                {
                    continue;
                }

                if (!courseGroups.ContainsKey(courseId.Value))
                {
                    courseGroups[courseId.Value] = new List<Domain.Entities.StudySession>();
                }
                courseGroups[courseId.Value].Add(session);
            }

            var courseHistoryDtos = new List<CourseStudyHistoryDto>();

            foreach (var courseGroup in courseGroups.Take(request.Count))
            {
                var courseId = courseGroup.Key;
                var sessions = courseGroup.Value;

                var lastStudyDate = sessions.Max(s => s.StartedAt);
                var totalStudyTime = sessions.Sum(s => s.DurationSeconds);
                var sessionsCount = sessions.Count;

                // Get course information
                var courseResult = await _mediator.Send(new EduTrack.Application.Features.Courses.Queries.GetCourseByIdQuery(courseId));
                var courseTitle = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Title : "دوره آموزشی";
                var courseThumbnail = courseResult.IsSuccess && courseResult.Value != null ? courseResult.Value.Thumbnail : null;

                var courseHistoryDto = new CourseStudyHistoryDto
                {
                    CourseId = courseId,
                    CourseTitle = courseTitle,
                    CourseThumbnail = courseThumbnail,
                    LastStudyDate = lastStudyDate,
                    TotalStudyTimeSeconds = totalStudyTime,
                    StudySessionsCount = sessionsCount,
                    CompletedItems = sessions.Count(s => s.IsCompleted),
                    TotalItems = sessions.Count // This should be total items in course
                };

                courseHistoryDtos.Add(courseHistoryDto);
            }

            // Sort by last study date
            courseHistoryDtos = courseHistoryDtos.OrderByDescending(c => c.LastStudyDate).ToList();

            return Result<List<CourseStudyHistoryDto>>.Success(courseHistoryDtos);
        }
        catch (Exception ex)
        {
            return Result<List<CourseStudyHistoryDto>>.Failure($"خطا در دریافت تاریخچه دوره‌ها: {ex.Message}");
        }
    }
}