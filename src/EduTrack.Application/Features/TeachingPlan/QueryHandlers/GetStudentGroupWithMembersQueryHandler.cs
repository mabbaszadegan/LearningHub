using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.QueryHandlers;

public class GetStudentGroupWithMembersQueryHandler : IRequestHandler<GetStudentGroupWithMembersQuery, Result<StudentGroupDto>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;

    public GetStudentGroupWithMembersQueryHandler(IStudentGroupRepository studentGroupRepository)
    {
        _studentGroupRepository = studentGroupRepository;
    }

    public async Task<Result<StudentGroupDto>> Handle(GetStudentGroupWithMembersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.Id, cancellationToken);
            
            if (group == null)
            {
                return Result<StudentGroupDto>.Failure("Student group not found");
            }

            var groupDto = new StudentGroupDto
            {
                Id = group.Id,
                TeachingPlanId = group.TeachingPlanId,
                Name = group.Name,
                MemberCount = group.GetTotalMembers(),
                Members = group.Members.Select(m => new GroupMemberDto
                {
                    Id = m.Id,
                    StudentGroupId = m.StudentGroupId,
                    StudentId = m.StudentId,
                    StudentName = m.Student?.FirstName + " " + m.Student?.LastName ?? "Unknown Student",
                    StudentEmail = m.Student?.Email ?? ""
                }).ToList()
            };

            return Result<StudentGroupDto>.Success(groupDto);
        }
        catch (Exception ex)
        {
            return Result<StudentGroupDto>.Failure($"Error retrieving student group: {ex.Message}");
        }
    }
}
