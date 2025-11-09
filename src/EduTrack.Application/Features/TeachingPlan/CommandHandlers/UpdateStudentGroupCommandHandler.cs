using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class UpdateStudentGroupCommandValidator : AbstractValidator<UpdateStudentGroupCommand>
{
    public UpdateStudentGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Student Group ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
    }
}

public class UpdateStudentGroupCommandHandler : IRequestHandler<UpdateStudentGroupCommand, Result<StudentGroupDto>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateStudentGroupCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentGroupDto>> Handle(UpdateStudentGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _studentGroupRepository.GetGroupWithMembersAsync(request.Id, cancellationToken);
        if (group == null)
        {
            return Result<StudentGroupDto>.Failure("Student group not found");
        }

        // Verify user has permission to update this group
        if (group.TeachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<StudentGroupDto>.Failure("You don't have permission to update this group");
        }

        group.UpdateName(request.Name);

        await _studentGroupRepository.UpdateAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
