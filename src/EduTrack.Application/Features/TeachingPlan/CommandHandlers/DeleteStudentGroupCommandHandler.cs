using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class DeleteStudentGroupCommandHandler : IRequestHandler<DeleteStudentGroupCommand, Result<bool>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteStudentGroupCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteStudentGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.Id, cancellationToken);
        if (group == null)
        {
            return Result<bool>.Failure("Student group not found");
        }

        // Verify user has permission to delete this group
        if (group.TeachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<bool>.Failure("You don't have permission to delete this group");
        }

        await _studentGroupRepository.DeleteAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
