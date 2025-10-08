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
    private readonly ICurrentUserService _currentUserService;

    public RemoveGroupMemberCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IRepository<GroupMember> groupMemberRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _groupMemberRepository = groupMemberRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.GroupId, cancellationToken);
        if (group == null)
        {
            return Result<bool>.Failure("Student group not found");
        }

        // Verify user has permission to manage this group
        if (group.TeachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<bool>.Failure("You don't have permission to manage this group");
        }

        var member = group.Members.FirstOrDefault(m => m.StudentId == request.StudentId);
        if (member == null)
        {
            return Result<bool>.Failure("Student is not a member of this group");
        }

        await _groupMemberRepository.DeleteAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
