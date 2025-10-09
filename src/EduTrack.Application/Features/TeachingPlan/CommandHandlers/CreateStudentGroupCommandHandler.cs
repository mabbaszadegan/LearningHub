using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class CreateStudentGroupCommandValidator : AbstractValidator<CreateStudentGroupCommand>
{
    public CreateStudentGroupCommandValidator()
    {
        RuleFor(x => x.TeachingPlanId)
            .GreaterThan(0).WithMessage("Teaching Plan ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
    }
}

public class CreateStudentGroupCommandHandler : IRequestHandler<CreateStudentGroupCommand, Result<StudentGroupDto>>
{
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateStudentGroupCommandHandler(
        IStudentGroupRepository studentGroupRepository,
        ITeachingPlanRepository teachingPlanRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _studentGroupRepository = studentGroupRepository;
        _teachingPlanRepository = teachingPlanRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentGroupDto>> Handle(CreateStudentGroupCommand request, CancellationToken cancellationToken)
    {
        // Verify teaching plan exists and user has permission
        var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
        if (teachingPlan == null)
        {
            return Result<StudentGroupDto>.Failure("Teaching plan not found");
        }

        if (teachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<StudentGroupDto>.Failure("You don't have permission to create groups for this teaching plan");
        }

        var studentGroup = StudentGroup.Create(request.TeachingPlanId, request.Name);

        await _studentGroupRepository.AddAsync(studentGroup, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var studentGroupDto = new StudentGroupDto
        {
            Id = studentGroup.Id,
            TeachingPlanId = studentGroup.TeachingPlanId,
            Name = studentGroup.Name,
            MemberCount = 0
        };

        return Result<StudentGroupDto>.Success(studentGroupDto);
    }
}
