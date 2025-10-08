using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class TransferGroupMemberCommandHandler : IRequestHandler<TransferGroupMemberCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferGroupMemberCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IRepository<GroupMember> groupMemberRepository,
        IUnitOfWork unitOfWork)
    {
        _studentGroupRepository = studentGroupRepository;
        _groupMemberRepository = groupMemberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(TransferGroupMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the source group
            var fromGroup = await _studentGroupRepository.GetGroupWithMembersAsync(request.FromGroupId, cancellationToken);
            if (fromGroup == null)
            {
                return Result<bool>.Failure("Source group not found");
            }

            // Get the target group
            var toGroup = await _studentGroupRepository.GetGroupWithMembersAsync(request.ToGroupId, cancellationToken);
            if (toGroup == null)
            {
                return Result<bool>.Failure("Target group not found");
            }

            // Verify both groups belong to the same teaching plan
            if (fromGroup.TeachingPlanId != toGroup.TeachingPlanId)
            {
                return Result<bool>.Failure("Cannot transfer students between groups from different teaching plans");
            }

            // Find the member in the source group
            var member = fromGroup.Members.FirstOrDefault(m => m.StudentId == request.StudentId);
            if (member == null)
            {
                return Result<bool>.Failure("Student not found in source group");
            }

            // Check if student is already in the target group
            if (toGroup.Members.Any(m => m.StudentId == request.StudentId))
            {
                return Result<bool>.Failure("Student is already in the target group");
            }

            // Remove the member from the repository
            await _groupMemberRepository.DeleteAsync(member, cancellationToken);

            // Create a new member in the target group
            var newMember = GroupMember.Create(request.ToGroupId, request.StudentId);
            await _groupMemberRepository.AddAsync(newMember, cancellationToken);

            // Save all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error transferring student: {ex.Message}");
        }
    }
}