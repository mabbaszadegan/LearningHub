using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
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

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Result<CourseDto>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public UpdateCourseCommandHandler(
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<CourseDto>> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (course == null)
        {
            return Result<CourseDto>.Failure("Course not found");
        }

        course.Title = request.Title;
        course.Description = request.Description;
        course.Thumbnail = request.Thumbnail;
        course.IsActive = request.IsActive;
        course.Order = request.Order;
        course.UpdatedAt = _clock.UtcNow;

        await _courseRepository.UpdateAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var courseDto = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Thumbnail = course.Thumbnail,
            IsActive = course.IsActive,
            Order = course.Order,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            CreatedBy = course.CreatedBy,
            ModuleCount = course.Modules.Count,
            LessonCount = course.Modules.SelectMany(m => m.Lessons).Count()
        };

        return Result<CourseDto>.Success(courseDto);
    }
}
