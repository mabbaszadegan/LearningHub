using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public class UpdateSubChapterCommandValidator : AbstractValidator<UpdateSubChapterCommand>
{
    public UpdateSubChapterCommandValidator()
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

public class UpdateSubChapterCommandHandler : IRequestHandler<UpdateSubChapterCommand, Result<SubChapterDto>>
{
    private readonly IRepository<SubChapter> _subChapterRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public UpdateSubChapterCommandHandler(
        IRepository<SubChapter> subChapterRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _subChapterRepository = subChapterRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<SubChapterDto>> Handle(UpdateSubChapterCommand request, CancellationToken cancellationToken)
    {
        var subChapter = await _subChapterRepository.GetByIdAsync(request.Id, cancellationToken);
        if (subChapter == null)
        {
            return Result<SubChapterDto>.Failure("SubChapter not found");
        }

        subChapter.Title = request.Title;
        subChapter.Description = request.Description;
        subChapter.Objective = request.Objective;
        subChapter.IsActive = request.IsActive;
        subChapter.Order = request.Order;
        subChapter.UpdatedAt = _clock.UtcNow;

        await _subChapterRepository.UpdateAsync(subChapter, cancellationToken);
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
