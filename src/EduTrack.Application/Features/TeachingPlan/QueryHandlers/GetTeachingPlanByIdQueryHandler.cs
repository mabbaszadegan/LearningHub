using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetTeachingPlanByIdQueryHandler : IRequestHandler<GetTeachingPlanByIdQuery, Result<TeachingPlanDto>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;

    public GetTeachingPlanByIdQueryHandler(ITeachingPlanRepository teachingPlanRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
    }

    public async Task<Result<TeachingPlanDto>> Handle(GetTeachingPlanByIdQuery request, CancellationToken cancellationToken)
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
                TotalStudents = teachingPlan.GetTotalStudents()
            };

            return Result<TeachingPlanDto>.Success(teachingPlanDto);
        }
        catch (Exception ex)
        {
            return Result<TeachingPlanDto>.Failure($"Error retrieving teaching plan: {ex.Message}");
        }
    }
}
