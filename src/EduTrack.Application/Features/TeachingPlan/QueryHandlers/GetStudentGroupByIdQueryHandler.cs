using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetStudentGroupByIdQueryHandler : IRequestHandler<GetStudentGroupByIdQuery, Result<StudentGroupDto>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;

    public GetStudentGroupByIdQueryHandler(IStudentGroupRepository studentGroupRepository)
    {
        _studentGroupRepository = studentGroupRepository;
    }

    public async Task<Result<StudentGroupDto>> Handle(GetStudentGroupByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _studentGroupRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (group == null)
            {
                return Result<StudentGroupDto>.Failure("Student group not found");
            }

            var groupDto = new StudentGroupDto
            {
                Id = group.Id,
                TeachingPlanId = group.TeachingPlanId,
                Name = group.Name,
                MemberCount = group.GetTotalMembers()
            };

            return Result<StudentGroupDto>.Success(groupDto);
        }
        catch (Exception ex)
        {
            return Result<StudentGroupDto>.Failure($"Error retrieving student group: {ex.Message}");
        }
    }
}
