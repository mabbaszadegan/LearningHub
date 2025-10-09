using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class CreateSubChapterCommandValidator : AbstractValidator<CreateSubChapterCommand>
{
    public CreateSubChapterCommandValidator()
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

public class CreateSubChapterCommandHandler : IRequestHandler<CreateSubChapterCommand, Result<SubChapterDto>>
{
    private readonly IRepository<SubChapter> _subChapterRepository;
    private readonly IRepository<Chapter> _chapterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateSubChapterCommandHandler(
        IRepository<SubChapter> subChapterRepository,
        IRepository<Chapter> chapterRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _subChapterRepository = subChapterRepository;
        _chapterRepository = chapterRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<SubChapterDto>> Handle(CreateSubChapterCommand request, CancellationToken cancellationToken)
    {
        // Verify chapter exists
        var chapter = await _chapterRepository.GetByIdAsync(request.ChapterId, cancellationToken);
        if (chapter == null)
        {
            return Result<SubChapterDto>.Failure("Chapter not found");
        }

        var subChapter = SubChapter.Create(
            request.ChapterId,
            request.Title,
            request.Description,
            request.Objective,
            request.Order);

        await _subChapterRepository.AddAsync(subChapter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var subChapterDto = new SubChapterDto
        {
            Id = subChapter.Id,
            ChapterId = subChapter.ChapterId,
            Title = subChapter.Title,
            Description = subChapter.Description,
            Objective = subChapter.Objective,
            IsActive = subChapter.IsActive,
            Order = subChapter.Order,
            CreatedAt = subChapter.CreatedAt,
            UpdatedAt = subChapter.UpdatedAt
        };

        return Result<SubChapterDto>.Success(subChapterDto);
    }
}
