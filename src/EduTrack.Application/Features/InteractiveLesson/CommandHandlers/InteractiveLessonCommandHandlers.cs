using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.Commands;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using FluentValidation;
using MediatR;
using EduTrack.Domain.Repositories;

namespace EduTrack.Application.Features.InteractiveLesson.CommandHandlers;

public class CreateInteractiveLessonCommandValidator : AbstractValidator<CreateInteractiveLessonCommand>
{
    public CreateInteractiveLessonCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");
    }
}

public class CreateInteractiveLessonCommandHandler : IRequestHandler<CreateInteractiveLessonCommand, Result<InteractiveLessonDto>>
{
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInteractiveLessonCommandHandler(
        IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository,
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork)
    {
        _interactiveLessonRepository = interactiveLessonRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveLessonDto>> Handle(CreateInteractiveLessonCommand request, CancellationToken cancellationToken)
    {
        var courseEntity = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (courseEntity == null)
        {
            return Result<InteractiveLessonDto>.Failure("Course not found");
        }

        var interactiveLesson = Domain.Entities.InteractiveLesson.Create(
            request.CourseId,
            request.Title,
            request.Description,
            request.Order,
            request.CreatedBy);

        await _interactiveLessonRepository.AddAsync(interactiveLesson, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InteractiveLessonDto
        {
            Id = interactiveLesson.Id,
            CourseId = interactiveLesson.CourseId,
            Title = interactiveLesson.Title,
            Description = interactiveLesson.Description,
            IsActive = interactiveLesson.IsActive,
            Order = interactiveLesson.Order,
            CreatedAt = interactiveLesson.CreatedAt,
            UpdatedAt = interactiveLesson.UpdatedAt,
            CreatedBy = interactiveLesson.CreatedBy,
            ContentItems = new List<InteractiveContentItemDto>()
        };

        return Result<InteractiveLessonDto>.Success(dto);
    }
}

// AddContentToInteractiveLessonCommandHandler removed - EducationalContent entity removed

public class CreateInteractiveQuestionCommandHandler : IRequestHandler<CreateInteractiveQuestionCommand, Result<InteractiveQuestionDto>>
{
    private readonly IRepository<Domain.Entities.InteractiveQuestion> _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInteractiveQuestionCommandHandler(
        IRepository<Domain.Entities.InteractiveQuestion> questionRepository,
        IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveQuestionDto>> Handle(CreateInteractiveQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = Domain.Entities.InteractiveQuestion.Create(
            request.QuestionText,
            request.Type,
            request.Description,
            request.ImageFileId,
            request.CorrectAnswer,
            request.Points);

        await _questionRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InteractiveQuestionDto
        {
            Id = question.Id,
            QuestionText = question.QuestionText,
            Description = question.Description,
            Type = question.Type,
            ImageFileId = question.ImageFileId,
            CorrectAnswer = question.CorrectAnswer,
            Points = question.Points,
            IsActive = question.IsActive,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Choices = new List<QuestionChoiceDto>()
        };

        return Result<InteractiveQuestionDto>.Success(dto);
    }
}

public class AddQuestionToInteractiveLessonCommandHandler : IRequestHandler<AddQuestionToInteractiveLessonCommand, Result<InteractiveContentItemDto>>
{
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;
    private readonly IRepository<Domain.Entities.InteractiveQuestion> _questionRepository;
    private readonly IRepository<Domain.Entities.InteractiveContentItem> _contentItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddQuestionToInteractiveLessonCommandHandler(
        IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository,
        IRepository<Domain.Entities.InteractiveQuestion> questionRepository,
        IRepository<Domain.Entities.InteractiveContentItem> contentItemRepository,
        IUnitOfWork unitOfWork)
    {
        _interactiveLessonRepository = interactiveLessonRepository;
        _questionRepository = questionRepository;
        _contentItemRepository = contentItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveContentItemDto>> Handle(AddQuestionToInteractiveLessonCommand request, CancellationToken cancellationToken)
    {
        var interactiveLesson = await _interactiveLessonRepository.GetByIdAsync(request.InteractiveLessonId, cancellationToken);
        if (interactiveLesson == null)
        {
            return Result<InteractiveContentItemDto>.Failure("Interactive lesson not found");
        }

        var question = Domain.Entities.InteractiveQuestion.Create(
            request.QuestionText,
            request.Type,
            request.Description,
            request.ImageFileId,
            request.CorrectAnswer,
            request.Points);

        await _questionRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var contentItem = Domain.Entities.InteractiveContentItem.Create(
            request.InteractiveLessonId,
            request.Order,
            interactiveQuestionId: question.Id);

        await _contentItemRepository.AddAsync(contentItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new InteractiveContentItemDto
        {
            Id = contentItem.Id,
            InteractiveLessonId = contentItem.InteractiveLessonId,
            Order = contentItem.Order,
            IsActive = contentItem.IsActive,
            CreatedAt = contentItem.CreatedAt,
            InteractiveQuestionId = contentItem.InteractiveQuestionId,
            InteractiveQuestion = new InteractiveQuestionDto
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Description = question.Description,
                Type = question.Type,
                ImageFileId = question.ImageFileId,
                CorrectAnswer = question.CorrectAnswer,
                Points = question.Points,
                IsActive = question.IsActive,
                CreatedAt = question.CreatedAt,
                UpdatedAt = question.UpdatedAt,
                Choices = new List<QuestionChoiceDto>()
            }
        };

        return Result<InteractiveContentItemDto>.Success(dto);
    }
}