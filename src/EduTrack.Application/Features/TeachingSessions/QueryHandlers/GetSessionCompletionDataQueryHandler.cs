using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetSessionCompletionDataQueryHandler : IRequestHandler<GetSessionCompletionDataQuery, Result<SessionCompletionDataDto>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly ISubChapterRepository _subChapterRepository;
    private readonly ILessonRepository _lessonRepository;

    public GetSessionCompletionDataQueryHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingPlanRepository teachingPlanRepository,
        IStudentGroupRepository studentGroupRepository,
        ISubChapterRepository subChapterRepository,
        ILessonRepository lessonRepository)
    {
        _sessionReportRepository = sessionReportRepository;
        _teachingPlanRepository = teachingPlanRepository;
        _studentGroupRepository = studentGroupRepository;
        _subChapterRepository = subChapterRepository;
        _lessonRepository = lessonRepository;
    }

    public async Task<Result<SessionCompletionDataDto>> Handle(GetSessionCompletionDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<SessionCompletionDataDto>.Failure("جلسه یافت نشد.");
            }

            // Get teaching plan
            var teachingPlan = await _teachingPlanRepository.GetByIdAsync(sessionReport.TeachingPlanId, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<SessionCompletionDataDto>.Failure("پلن آموزشی یافت نشد.");
            }

            // Get groups
            var groups = await _studentGroupRepository.GetGroupsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken);
            
            // Debug: Log groups count
            System.Diagnostics.Debug.WriteLine($"Found {groups.Count()} groups for teaching plan {sessionReport.TeachingPlanId}");
            
            var groupDtos = groups.Select(g => new GroupDataDto
            {
                Id = g.Id,
                Name = g.Name,
                MemberCount = g.GetTotalMembers(),
                Members = g.Members?.Select(m => new GroupMemberDto
                {
                    Id = m.Id,
                    StudentGroupId = m.StudentGroupId,
                    StudentId = m.StudentId,
                    StudentName = (m.Student?.FirstName + " " + m.Student?.LastName)?.Trim() ?? "Unknown Student",
                    StudentEmail = m.Student?.Email ?? ""
                }).ToList() ?? new List<GroupMemberDto>()
            }).ToList();

            // Get available subtopics and lessons
            var subtopics = await _subChapterRepository.GetByCourseIdAsync(teachingPlan.CourseId, cancellationToken);
            var subtopicDtos = subtopics.Select(s => new SubTopicDto
            {
                Id = s.Id,
                ChapterId = s.ChapterId,
                Title = s.Title,
                Description = s.Description,
                Objective = s.Objective,
                IsActive = s.IsActive,
                Order = s.Order,
                ChapterTitle = s.Chapter?.Title ?? "Unknown Chapter"
            }).ToList();

            var lessons = await _lessonRepository.GetByCourseIdAsync(teachingPlan.CourseId, cancellationToken);
            var lessonDtos = lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                ModuleId = l.ModuleId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                IsActive = l.IsActive,
                Order = l.Order,
                DurationMinutes = l.DurationMinutes,
                ModuleTitle = l.Module?.Title ?? "Unknown Module"
            }).ToList();

            // Check if session has planning data
            var hasPlan = await _sessionReportRepository.HasPlanningDataAsync(request.SessionId, cancellationToken);
            
            // Get planned items if exists
            List<PlannedItemDto>? plannedItems = null;
            if (hasPlan)
            {
                var plannedData = await _sessionReportRepository.GetPlannedItemsAsync(request.SessionId, cancellationToken);
                plannedItems = plannedData.Select(p => new PlannedItemDto
                {
                    StudentGroupId = p.StudentGroupId,
                    PlannedObjectives = p.PlannedObjectives,
                    PlannedSubTopics = ParseJsonArray(p.PlannedSubTopicsJson),
                    PlannedLessons = ParseJsonArray(p.PlannedLessonsJson),
                    AdditionalTopics = p.AdditionalTopics
                }).ToList();
            }

            var result = new SessionCompletionDataDto
            {
                SessionId = sessionReport.Id,
                SessionTitle = sessionReport.Title ?? "جلسه بدون عنوان",
                SessionDate = sessionReport.SessionDate,
                TeachingPlanId = sessionReport.TeachingPlanId,
                TeachingPlanTitle = teachingPlan.Title,
                CourseTitle = teachingPlan.Course?.Title ?? "Unknown Course",
                HasPlan = hasPlan,
                Groups = groupDtos,
                AvailableSubTopics = subtopicDtos,
                AvailableLessons = lessonDtos,
                PlannedItems = plannedItems
            };

            return Result<SessionCompletionDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SessionCompletionDataDto>.Failure($"خطا در بارگذاری اطلاعات: {ex.Message}");
        }
    }

    private static List<int> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<int>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }
}
