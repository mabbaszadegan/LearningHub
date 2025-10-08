using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class AddGroupMembersCommandHandler : IRequestHandler<AddGroupMembersCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddGroupMembersCommandHandler(
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

    public async Task<Result<bool>> Handle(AddGroupMembersCommand request, CancellationToken cancellationToken)
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

        var addedCount = 0;
        foreach (var studentId in request.StudentIds)
        {
            // Check if student is already in the group
            if (!group.HasStudent(studentId))
            {
                var member = GroupMember.Create(request.GroupId, studentId);
                await _groupMemberRepository.AddAsync(member, cancellationToken);
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
