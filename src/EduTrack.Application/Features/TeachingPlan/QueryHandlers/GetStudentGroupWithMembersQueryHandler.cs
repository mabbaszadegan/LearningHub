using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.TeachingSessions;
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
                Members = group.Members.Select(m =>
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
            };

            return Result<StudentGroupDto>.Success(groupDto);
        }
        catch (Exception ex)
        {
            return Result<StudentGroupDto>.Failure($"Error retrieving student group: {ex.Message}");
        }
    }
}
