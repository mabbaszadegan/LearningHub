using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class CreateChapterCommandValidator : AbstractValidator<CreateChapterCommand>
{
    public CreateChapterCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Objective)
            .NotEmpty().WithMessage("Objective is required")
            .MaximumLength(2000).WithMessage("Objective cannot exceed 2000 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

public class CreateChapterCommandHandler : IRequestHandler<CreateChapterCommand, Result<ChapterDto>>
{
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateChapterCommandHandler(
        IRepository<Chapter> chapterRepository,
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _chapterRepository = chapterRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ChapterDto>> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
    {
        // Verify course exists
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<ChapterDto>.Failure("Course not found");
        }

        var chapter = Chapter.Create(
            request.CourseId,
            request.Title,
            request.Description,
            request.Objective,
            request.Order);

        await _chapterRepository.AddAsync(chapter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var chapterDto = new ChapterDto
        {
            Id = chapter.Id,
            CourseId = chapter.CourseId,
            Title = chapter.Title,
            Description = chapter.Description,
            Objective = chapter.Objective,
            IsActive = chapter.IsActive,
            Order = chapter.Order,
            CreatedAt = chapter.CreatedAt,
            UpdatedAt = chapter.UpdatedAt,
            SubChapterCount = 0
        };

        return Result<ChapterDto>.Success(chapterDto);
    }
}
