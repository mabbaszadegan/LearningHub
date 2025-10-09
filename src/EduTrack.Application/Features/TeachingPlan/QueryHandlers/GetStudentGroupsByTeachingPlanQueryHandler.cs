using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetStudentGroupsByTeachingPlanQueryHandler : IRequestHandler<GetStudentGroupsByTeachingPlanQuery, Result<List<StudentGroupDto>>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;

    public GetStudentGroupsByTeachingPlanQueryHandler(IStudentGroupRepository studentGroupRepository)
    {
        _studentGroupRepository = studentGroupRepository;
    }

    public async Task<Result<List<StudentGroupDto>>> Handle(GetStudentGroupsByTeachingPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var groups = await _studentGroupRepository.GetGroupsByTeachingPlanAsync(request.TeachingPlanId, cancellationToken);
            
            var groupDtos = groups.Select(g => new StudentGroupDto
            {
                Id = g.Id,
                TeachingPlanId = g.TeachingPlanId,
                Name = g.Name,
                MemberCount = g.GetTotalMembers(),
                Members = g.Members.Select(m => new GroupMemberDto
                {
                    Id = m.Id,
                    StudentGroupId = m.StudentGroupId,
                    StudentId = m.StudentId,
                    StudentName = m.Student?.FirstName + " " + m.Student?.LastName ?? "Unknown Student",
                    StudentEmail = m.Student?.Email ?? ""
                }).ToList()
            }).ToList();

            return Result<List<StudentGroupDto>>.Success(groupDtos);
        }
        catch (Exception ex)
        {
            return Result<List<StudentGroupDto>>.Failure($"Error retrieving student groups: {ex.Message}");
        }
    }
}
