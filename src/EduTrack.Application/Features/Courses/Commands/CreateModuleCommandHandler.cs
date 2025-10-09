using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

public class CreateModuleCommandHandler : IRequestHandler<CreateModuleCommand, Result<ModuleDto>>
{
    private readonly IRepository<Module> _moduleRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateModuleCommandHandler(
        IRepository<Module> moduleRepository,
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _moduleRepository = moduleRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ModuleDto>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<ModuleDto>.Failure("Course not found");
        }

        var module = Module.Create(
            request.CourseId,
            request.Title,
            request.Description,
            request.Order);

        await _moduleRepository.AddAsync(module, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var moduleDto = new ModuleDto
        {
            Id = module.Id,
            CourseId = module.CourseId,
            Title = module.Title,
            Description = module.Description,
            IsActive = module.IsActive,
            Order = module.Order,
            CreatedAt = module.CreatedAt,
            UpdatedAt = module.UpdatedAt,
            LessonCount = 0
        };

        return Result<ModuleDto>.Success(moduleDto);
    }
}
