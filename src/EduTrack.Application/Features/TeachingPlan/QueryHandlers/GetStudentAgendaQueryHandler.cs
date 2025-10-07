using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetStudentAgendaQueryHandler : IRequestHandler<GetStudentAgendaQuery, Result<StudentAgendaDto>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStudentAgendaQueryHandler(
        IScheduleItemRepository scheduleItemRepository,
        ISubmissionRepository submissionRepository,
        ICourseRepository courseRepository,
        ICurrentUserService currentUserService)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _submissionRepository = submissionRepository;
        _courseRepository = courseRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentAgendaDto>> Handle(GetStudentAgendaQuery request, CancellationToken cancellationToken)
    {
        var studentId = request.StudentId;
        if (string.IsNullOrEmpty(studentId))
        {
            return Result<StudentAgendaDto>.Failure("Student ID is required");
        }

        // Get all schedule items for the student (either assigned to their groups or all groups)
        var allScheduleItems = await _scheduleItemRepository.GetActiveScheduleItemsAsync(cancellationToken);
        
        // Filter by course if specified
        if (request.CourseId.HasValue)
        {
            allScheduleItems = allScheduleItems.Where(si => si.TeachingPlan.CourseId == request.CourseId.Value).ToList();
        }

        // Get student's submissions
        var submissions = await _submissionRepository.GetSubmissionsByStudentAsync(studentId, cancellationToken);
        var submissionLookup = submissions.ToDictionary(s => s.ScheduleItemId, s => s);

        var now = DateTimeOffset.UtcNow;
        var upcomingItems = new List<ScheduleItemDto>();
        var inProgressItems = new List<ScheduleItemDto>();
        var overdueItems = new List<ScheduleItemDto>();
        var completedItems = new List<ScheduleItemDto>();

        foreach (var item in allScheduleItems)
        {
            var submission = submissionLookup.GetValueOrDefault(item.Id);
            var itemDto = MapToScheduleItemDto(item, submission);

            if (submission?.IsCompleted == true)
            {
                completedItems.Add(itemDto);
            }
            else if (item.IsOverdue())
            {
                overdueItems.Add(itemDto);
            }
            else if (item.IsUpcoming())
            {
                upcomingItems.Add(itemDto);
            }
            else
            {
                inProgressItems.Add(itemDto);
            }
        }

        // Get course info if specified
        var courseTitle = "All Courses";
        if (request.CourseId.HasValue)
        {
            var course = await _courseRepository.GetByIdAsync(request.CourseId.Value, cancellationToken);
            courseTitle = course?.Title ?? "Unknown Course";
        }

        var agenda = new StudentAgendaDto
        {
            StudentId = studentId,
            StudentName = _currentUserService.UserName ?? "Unknown",
            CourseId = request.CourseId ?? 0,
            CourseTitle = courseTitle,
            LearningMode = Domain.Enums.LearningMode.SelfStudy, // TODO: Get from enrollment
            UpcomingItems = upcomingItems.OrderBy(i => i.StartDate).ToList(),
            InProgressItems = inProgressItems.OrderBy(i => i.DueDate ?? DateTimeOffset.MaxValue).ToList(),
            OverdueItems = overdueItems.OrderBy(i => i.DueDate ?? DateTimeOffset.MinValue).ToList(),
            CompletedItems = completedItems.OrderByDescending(i => i.UpdatedAt).ToList(),
            TotalItems = allScheduleItems.Count(),
            CompletedCount = completedItems.Count,
            OverdueCount = overdueItems.Count,
            CompletionPercentage = allScheduleItems.Any() ? (double)completedItems.Count / allScheduleItems.Count() * 100 : 0
        };

        return Result<StudentAgendaDto>.Success(agenda);
    }

    private static ScheduleItemDto MapToScheduleItemDto(Domain.Entities.ScheduleItem item, Domain.Entities.Submission? submission)
    {
        return new ScheduleItemDto
        {
            Id = item.Id,
            TeachingPlanId = item.TeachingPlanId,
            GroupId = item.GroupId,
            GroupName = item.Group?.Name,
            LessonId = item.LessonId,
            LessonTitle = item.Lesson?.Title,
            Type = item.Type,
            Title = item.Title,
            Description = item.Description,
            StartDate = item.StartDate,
            DueDate = item.DueDate,
            IsMandatory = item.IsMandatory,
            DisciplineHint = item.DisciplineHint,
            ContentJson = item.ContentJson,
            MaxScore = item.MaxScore,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            IsOverdue = item.IsOverdue(),
            IsUpcoming = item.IsUpcoming(),
            IsActive = item.IsActive(),
            TimeUntilDue = item.GetTimeUntilDue()
        };
    }
}
