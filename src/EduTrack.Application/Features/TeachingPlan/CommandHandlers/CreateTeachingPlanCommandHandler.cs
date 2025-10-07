using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class CreateTeachingPlanCommandValidator : AbstractValidator<CreateTeachingPlanCommand>
{
    public CreateTeachingPlanCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID must be greater than 0");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
    }
}

public class CreateTeachingPlanCommandHandler : IRequestHandler<CreateTeachingPlanCommand, Result<TeachingPlanDto>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTeachingPlanCommandHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TeachingPlanDto>> Handle(CreateTeachingPlanCommand request, CancellationToken cancellationToken)
    {
        // Verify course exists
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<TeachingPlanDto>.Failure("Course not found");
        }

        // Verify user is the course creator or has permission
        if (course.CreatedBy != _currentUserService.UserId)
        {
            return Result<TeachingPlanDto>.Failure("You don't have permission to create teaching plans for this course");
        }

        var teachingPlan = EduTrack.Domain.Entities.TeachingPlan.Create(
            request.CourseId,
            _currentUserService.UserId ?? "system",
            request.Title,
            request.Description);

        await _teachingPlanRepository.AddAsync(teachingPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var teachingPlanDto = new TeachingPlanDto
        {
            Id = teachingPlan.Id,
            CourseId = teachingPlan.CourseId,
            CourseTitle = course.Title,
            TeacherId = teachingPlan.TeacherId,
            TeacherName = _currentUserService.UserName ?? "Unknown",
            Title = teachingPlan.Title,
            Description = teachingPlan.Description,
            CreatedAt = teachingPlan.CreatedAt,
            UpdatedAt = teachingPlan.UpdatedAt,
            GroupCount = 0,
            ScheduleItemCount = 0,
            TotalStudents = 0
        };

        return Result<TeachingPlanDto>.Success(teachingPlanDto);
    }
}
