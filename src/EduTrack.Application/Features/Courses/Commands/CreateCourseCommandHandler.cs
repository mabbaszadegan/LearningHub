using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
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

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CourseDto>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly ICurrentUserService _currentUserService;

    public CreateCourseCommandHandler(
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork,
        IClock clock,
        ICurrentUserService currentUserService)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CourseDto>> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var course = Course.Create(
            request.Title,
            request.Description,
            ResolveThumbnailUrl(request.Thumbnail, request.ThumbnailFileId),
            request.Order,
            _currentUserService.UserId ?? "system",
            request.DisciplineType,
            request.ThumbnailFileId);

        await _courseRepository.AddAsync(course, cancellationToken);
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
                // ModuleCount = 0,
                // LessonCount = 0
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
