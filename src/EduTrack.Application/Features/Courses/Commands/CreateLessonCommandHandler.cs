using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .GreaterThan(0).WithMessage("Module ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duration must be greater than 0");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Result<LessonDto>>
{
    private readonly IRepository<Lesson> _lessonRepository;
    private readonly IRepository<Module> _moduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateLessonCommandHandler(
        IRepository<Lesson> lessonRepository,
        IRepository<Module> moduleRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _lessonRepository = lessonRepository;
        _moduleRepository = moduleRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<LessonDto>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var module = await _moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);
        if (module == null)
        {
            return Result<LessonDto>.Failure("Module not found");
        }

        var lesson = Lesson.Create(
            request.ModuleId,
            request.Title,
            request.Content,
            request.VideoUrl,
            request.DurationMinutes,
            request.Order);

        await _lessonRepository.AddAsync(lesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var lessonDto = new LessonDto
        {
            Id = lesson.Id,
            ModuleId = lesson.ModuleId,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            IsActive = lesson.IsActive,
            Order = lesson.Order,
            DurationMinutes = lesson.DurationMinutes,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            Resources = new List<ResourceDto>()
        };

        return Result<LessonDto>.Success(lessonDto);
    }
}
