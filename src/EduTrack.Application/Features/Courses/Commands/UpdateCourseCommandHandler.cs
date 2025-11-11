using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
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

        RuleFor(x => x.DisciplineType)
            .IsInEnum().WithMessage("Discipline type is invalid");

        RuleFor(x => x.ThumbnailFileId)
            .GreaterThan(0).When(x => x.ThumbnailFileId.HasValue)
            .WithMessage("Thumbnail file id must be positive");
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

        course.UpdateTitle(request.Title);
        course.UpdateDescription(request.Description);
        course.UpdateThumbnail(ResolveThumbnailUrl(request.Thumbnail, request.ThumbnailFileId), request.ThumbnailFileId);
        course.UpdateOrder(request.Order);
        course.UpdateDisciplineType(request.DisciplineType);
        
        if (request.IsActive)
            course.Activate();
        else
            course.Deactivate();

        await _courseRepository.UpdateAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var courseDto = new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Thumbnail = course.Thumbnail,
            ThumbnailFileId = course.ThumbnailFileId,
            IsActive = course.IsActive,
            Order = course.Order,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            CreatedBy = course.CreatedBy,
            DisciplineType = course.DisciplineType,
                // ModuleCount and LessonCount removed - Modules removed
                // ModuleCount = 0, // Modules removed
                // LessonCount = 0 // Lessons removed (using Chapters/SubChapters instead)
        };

        return Result<CourseDto>.Success(courseDto);
    }

    private static string? ResolveThumbnailUrl(string? thumbnail, int? thumbnailFileId)
    {
        if (!string.IsNullOrWhiteSpace(thumbnail))
        {
            return thumbnail;
        }

        if (thumbnailFileId.HasValue)
        {
            return $"/FileUpload/GetFile/{thumbnailFileId.Value}";
        }

        return null;
    }
}
