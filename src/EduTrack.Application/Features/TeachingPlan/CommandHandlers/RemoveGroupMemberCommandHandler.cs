using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class RemoveGroupMemberCommandHandler : IRequestHandler<RemoveGroupMemberCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveGroupMemberCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IRepository<GroupMember> groupMemberRepository,
        IUnitOfWork unitOfWork)
    {
        _studentGroupRepository = studentGroupRepository;
        _groupMemberRepository = groupMemberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.GroupId, cancellationToken);
            if (group == null)
            {
                return Result<bool>.Failure("Student group not found");
            }

            // Verify user has permission to manage this group
            // Note: This should be validated at the controller level with current user context
            // For now, we'll trust that the controller has already validated permissions

            var member = group.Members.FirstOrDefault(m => m.StudentProfileId == request.StudentProfileId);
            if (member == null)
            {
                return Result<bool>.Failure("Student is not a member of this group");
            }

            await _groupMemberRepository.DeleteAsync(member, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing student from group: {ex.Message}");
        }
    }
}
