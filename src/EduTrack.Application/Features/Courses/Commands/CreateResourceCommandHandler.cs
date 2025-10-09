using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0).WithMessage("Lesson ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("File path is required")
            .MaximumLength(1000).WithMessage("File path cannot exceed 1000 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Result<ResourceDto>>
{
    private readonly IRepository<Resource> _resourceRepository;
    private readonly IRepository<Lesson> _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateResourceCommandHandler(
        IRepository<Resource> resourceRepository,
        IRepository<Lesson> lessonRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _resourceRepository = resourceRepository;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ResourceDto>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.LessonId, cancellationToken);
        if (lesson == null)
        {
            return Result<ResourceDto>.Failure("Lesson not found");
        }

        var resource = Resource.Create(
            request.LessonId,
            request.Title,
            request.Type,
            request.FilePath,
            request.Url,
            request.FileSizeBytes,
            request.MimeType,
            request.Order);

        await _resourceRepository.AddAsync(resource, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resourceDto = new ResourceDto
        {
            Id = resource.Id,
            LessonId = resource.LessonId,
            Title = resource.Title,
            Description = resource.Description,
            Type = resource.Type,
            FilePath = resource.FilePath,
            Url = resource.Url,
            FileSizeBytes = resource.FileSizeBytes,
            MimeType = resource.MimeType,
            IsActive = resource.IsActive,
            Order = resource.Order,
            CreatedAt = resource.CreatedAt
        };

        return Result<ResourceDto>.Success(resourceDto);
    }
}
