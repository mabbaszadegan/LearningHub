using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetTeachingPlansByTeacherQueryHandler : IRequestHandler<GetTeachingPlansByTeacherQuery, Result<List<TeachingPlanDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;

    public GetTeachingPlansByTeacherQueryHandler(ITeachingPlanRepository teachingPlanRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
    }

    public async Task<Result<List<TeachingPlanDto>>> Handle(GetTeachingPlansByTeacherQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachingPlans = await _teachingPlanRepository.GetTeachingPlansByTeacherAsync(request.TeacherId, cancellationToken);
            
            var teachingPlanDtos = teachingPlans.Select(tp => new TeachingPlanDto
            {
                Id = tp.Id,
                CourseId = tp.CourseId,
                CourseTitle = tp.Course?.Title ?? "Unknown Course",
                TeacherId = tp.TeacherId,
                TeacherName = tp.Teacher?.FirstName + " " + tp.Teacher?.LastName ?? "Unknown Teacher",
                Title = tp.Title,
                Description = tp.Description,
                Objectives = tp.Objectives,
                CreatedAt = tp.CreatedAt,
                UpdatedAt = tp.UpdatedAt,
                GroupCount = tp.GetTotalGroups(),
                ScheduleItemCount = tp.GetTotalScheduleItems(),
                TotalStudents = tp.GetTotalStudents()
            }).ToList();

            return Result<List<TeachingPlanDto>>.Success(teachingPlanDtos);
        }
        catch (Exception ex)
        {
            return Result<List<TeachingPlanDto>>.Failure($"Error retrieving teaching plans: {ex.Message}");
        }
    }
}
