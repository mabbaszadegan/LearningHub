using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class UpdateChapterCommandValidator : AbstractValidator<UpdateChapterCommand>
{
    public UpdateChapterCommandValidator()
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

public class UpdateChapterCommandHandler : IRequestHandler<UpdateChapterCommand, Result<ChapterDto>>
{
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public UpdateChapterCommandHandler(
        IRepository<Chapter> chapterRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _chapterRepository = chapterRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ChapterDto>> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByIdAsync(request.Id, cancellationToken);
        if (chapter == null)
        {
            return Result<ChapterDto>.Failure("Chapter not found");
        }

        chapter.Title = request.Title;
        chapter.Description = request.Description;
        chapter.Objective = request.Objective;
        chapter.IsActive = request.IsActive;
        chapter.Order = request.Order;
        chapter.UpdatedAt = _clock.UtcNow;

        await _chapterRepository.UpdateAsync(chapter, cancellationToken);
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
            SubChapterCount = chapter.SubChapters.Count
        };

        return Result<ChapterDto>.Success(chapterDto);
    }
}
