using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class UpdateTeachingPlanCommandValidator : AbstractValidator<UpdateTeachingPlanCommand>
{
    public UpdateTeachingPlanCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Teaching Plan ID must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Objectives)
            .MaximumLength(2000).WithMessage("Objectives cannot exceed 2000 characters");
    }
}

public class UpdateTeachingPlanCommandHandler : IRequestHandler<UpdateTeachingPlanCommand, Result<TeachingPlanDto>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTeachingPlanCommandHandler(
        ITeachingPlanRepository teachingPlanRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TeachingPlanDto>> Handle(UpdateTeachingPlanCommand request, CancellationToken cancellationToken)
    {
        var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.Id, cancellationToken);
        if (teachingPlan == null)
        {
            return Result<TeachingPlanDto>.Failure("Teaching plan not found");
        }

        // Verify user is the teaching plan creator or has permission
        if (teachingPlan.TeacherId != _currentUserService.UserId)
        {
            return Result<TeachingPlanDto>.Failure("You don't have permission to update this teaching plan");
        }

        teachingPlan.UpdateTitle(request.Title);
        teachingPlan.UpdateDescription(request.Description);
        teachingPlan.UpdateObjectives(request.Objectives);

        await _teachingPlanRepository.UpdateAsync(teachingPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var teachingPlanDto = new TeachingPlanDto
        {
            Id = teachingPlan.Id,
            CourseId = teachingPlan.CourseId,
            CourseTitle = teachingPlan.Course?.Title ?? "Unknown Course",
            TeacherId = teachingPlan.TeacherId,
            TeacherName = _currentUserService.UserName ?? "Unknown",
            Title = teachingPlan.Title,
            Description = teachingPlan.Description,
            Objectives = teachingPlan.Objectives,
            CreatedAt = teachingPlan.CreatedAt,
            UpdatedAt = teachingPlan.UpdatedAt,
            GroupCount = teachingPlan.GetTotalGroups(),
            ScheduleItemCount = teachingPlan.GetTotalScheduleItems(),
            TotalStudents = teachingPlan.GetTotalStudents()
        };

        return Result<TeachingPlanDto>.Success(teachingPlanDto);
    }
}
