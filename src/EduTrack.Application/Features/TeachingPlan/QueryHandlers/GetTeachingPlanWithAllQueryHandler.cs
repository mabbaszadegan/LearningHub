using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetTeachingPlanWithAllQueryHandler : IRequestHandler<GetTeachingPlanWithAllQuery, Result<TeachingPlanDto>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;

    public GetTeachingPlanWithAllQueryHandler(ITeachingPlanRepository teachingPlanRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
    }

    public async Task<Result<TeachingPlanDto>> Handle(GetTeachingPlanWithAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachingPlan = await _teachingPlanRepository.GetTeachingPlanWithAllAsync(request.Id, cancellationToken);
            
            if (teachingPlan == null)
            {
                return Result<TeachingPlanDto>.Failure("Teaching plan not found");
            }

            var teachingPlanDto = new TeachingPlanDto
            {
                Id = teachingPlan.Id,
                CourseId = teachingPlan.CourseId,
                CourseTitle = teachingPlan.Course?.Title ?? "Unknown Course",
                TeacherId = teachingPlan.TeacherId,
                TeacherName = teachingPlan.Teacher?.FirstName + " " + teachingPlan.Teacher?.LastName ?? "Unknown Teacher",
                Title = teachingPlan.Title,
                Description = teachingPlan.Description,
                Objectives = teachingPlan.Objectives,
                CreatedAt = teachingPlan.CreatedAt,
                UpdatedAt = teachingPlan.UpdatedAt,
                GroupCount = teachingPlan.GetTotalGroups(),
                ScheduleItemCount = teachingPlan.GetTotalScheduleItems(),
                TotalStudents = teachingPlan.GetTotalStudents(),
                Groups = teachingPlan.Groups.Select(g => new StudentGroupDto
                {
                    Id = g.Id,
                    TeachingPlanId = g.TeachingPlanId,
                    Name = g.Name,
                    MemberCount = g.GetTotalMembers(),
                    Members = g.Members.Select(m =>
                    {
                        var profile = m.StudentProfile;
                        var user = profile?.User;
                        var displayName = profile?.DisplayName;
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = $"{user?.FirstName} {user?.LastName}".Trim();
                        }
                        if (string.IsNullOrWhiteSpace(displayName))
                        {
                            displayName = user?.UserName ?? "Unknown Student";
                        }

                        return new GroupMemberDto
                        {
                            Id = m.Id,
                            StudentGroupId = m.StudentGroupId,
                            StudentProfileId = m.StudentProfileId,
                            StudentId = profile?.UserId ?? string.Empty,
                            StudentName = displayName,
                            StudentEmail = user?.Email ?? string.Empty
                        };
                    }).ToList()
                }).ToList(),
                ScheduleItems = teachingPlan.ScheduleItems.Select(si => new ScheduleItemDto
                {
                    Id = si.Id,
                    TeachingPlanId = si.TeachingPlanId,
                    GroupId = si.GroupId,
                    GroupName = si.Group?.Name,
                    LessonId = si.LessonId,
                    LessonTitle = si.Lesson?.Title,
                    Type = si.Type,
                    Title = si.Title,
                    Description = si.Description,
                    StartDate = si.StartDate,
                    DueDate = si.DueDate,
                    IsMandatory = si.IsMandatory,
                    ContentJson = si.ContentJson,
                    MaxScore = si.MaxScore,
                    CreatedAt = si.CreatedAt,
                    UpdatedAt = si.UpdatedAt
                }).ToList()
            };

            return Result<TeachingPlanDto>.Success(teachingPlanDto);
        }
        catch (Exception ex)
        {
            return Result<TeachingPlanDto>.Failure($"Error retrieving teaching plan with all data: {ex.Message}");
        }
    }
}
