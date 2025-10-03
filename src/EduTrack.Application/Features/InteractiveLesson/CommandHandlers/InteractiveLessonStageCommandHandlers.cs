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

// Validators
public class CreateInteractiveLessonStageCommandValidator : AbstractValidator<CreateInteractiveLessonStageCommand>
{
    public CreateInteractiveLessonStageCommandValidator()
    {
        RuleFor(x => x.InteractiveLessonId)
            .GreaterThan(0).WithMessage("InteractiveLesson ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");

        RuleFor(x => x.TextContent)
            .NotEmpty().When(x => x.StageType == InteractiveLessonStageType.TextOnly)
            .WithMessage("TextContent is required for TextOnly stage type");
    }
}

public class AddStageContentItemCommandValidator : AbstractValidator<AddStageContentItemCommand>
{
    public AddStageContentItemCommandValidator()
    {
        RuleFor(x => x.InteractiveLessonStageId)
            .GreaterThan(0).WithMessage("InteractiveLessonStage ID is required");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");

        RuleFor(x => x)
            .Must(x => x.EducationalContentId.HasValue || x.InteractiveQuestionId.HasValue)
            .WithMessage("Either EducationalContentId or InteractiveQuestionId must be specified");

        RuleFor(x => x)
            .Must(x => !(x.EducationalContentId.HasValue && x.InteractiveQuestionId.HasValue))
            .WithMessage("Cannot specify both EducationalContentId and InteractiveQuestionId");
    }
}

public class AddSubChapterToInteractiveLessonCommandValidator : AbstractValidator<AddSubChapterToInteractiveLessonCommand>
{
    public AddSubChapterToInteractiveLessonCommandValidator()
    {
        RuleFor(x => x.InteractiveLessonId)
            .GreaterThan(0).WithMessage("InteractiveLesson ID is required");

        RuleFor(x => x.SubChapterId)
            .GreaterThan(0).WithMessage("SubChapter ID is required");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

// Command Handlers
public class CreateInteractiveLessonStageCommandHandler : IRequestHandler<CreateInteractiveLessonStageCommand, Result<InteractiveLessonStageDto>>
{
    private readonly IRepository<InteractiveLessonStage> _stageRepository;
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInteractiveLessonStageCommandHandler(
        IRepository<InteractiveLessonStage> stageRepository,
        IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository,
        IUnitOfWork unitOfWork)
    {
        _stageRepository = stageRepository;
        _interactiveLessonRepository = interactiveLessonRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveLessonStageDto>> Handle(CreateInteractiveLessonStageCommand request, CancellationToken cancellationToken)
    {
        var interactiveLesson = await _interactiveLessonRepository.GetByIdAsync(request.InteractiveLessonId, cancellationToken);
        if (interactiveLesson == null)
            return Result<InteractiveLessonStageDto>.Failure("Interactive lesson not found");

        var stage = InteractiveLessonStage.Create(
            request.InteractiveLessonId,
            request.Title,
            request.StageType,
            request.ArrangementType,
            request.Description,
            request.TextContent,
            request.Order
        );

        await _stageRepository.AddAsync(stage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InteractiveLessonStageDto>.Success(MapToDto(stage));
    }

    private static InteractiveLessonStageDto MapToDto(InteractiveLessonStage stage)
    {
        return new InteractiveLessonStageDto
        {
            Id = stage.Id,
            InteractiveLessonId = stage.InteractiveLessonId,
            Title = stage.Title,
            Description = stage.Description,
            StageType = stage.StageType,
            ArrangementType = stage.ArrangementType,
            TextContent = stage.TextContent,
            Order = stage.Order,
            IsActive = stage.IsActive,
            CreatedAt = stage.CreatedAt,
            UpdatedAt = stage.UpdatedAt,
            ContentItems = stage.ContentItems.Select(MapContentItemToDto).ToList()
        };
    }

    private static StageContentItemDto MapContentItemToDto(StageContentItem item)
    {
        return new StageContentItemDto
        {
            Id = item.Id,
            InteractiveLessonStageId = item.InteractiveLessonStageId,
            Order = item.Order,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            EducationalContentId = item.EducationalContentId,
            InteractiveQuestionId = item.InteractiveQuestionId,
            ContentType = item.GetContentType()
        };
    }
}

public class UpdateInteractiveLessonStageCommandHandler : IRequestHandler<UpdateInteractiveLessonStageCommand, Result<InteractiveLessonStageDto>>
{
    private readonly IRepository<InteractiveLessonStage> _stageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInteractiveLessonStageCommandHandler(
        IRepository<InteractiveLessonStage> stageRepository,
        IUnitOfWork unitOfWork)
    {
        _stageRepository = stageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveLessonStageDto>> Handle(UpdateInteractiveLessonStageCommand request, CancellationToken cancellationToken)
    {
        var stage = await _stageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (stage == null)
            return Result<InteractiveLessonStageDto>.Failure("Stage not found");

        stage.UpdateTitle(request.Title);
        stage.UpdateDescription(request.Description);
        stage.UpdateStageType(request.StageType);
        stage.UpdateArrangementType(request.ArrangementType);
        stage.UpdateTextContent(request.TextContent);
        stage.UpdateOrder(request.Order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InteractiveLessonStageDto>.Success(MapToDto(stage));
    }

    private static InteractiveLessonStageDto MapToDto(InteractiveLessonStage stage)
    {
        return new InteractiveLessonStageDto
        {
            Id = stage.Id,
            InteractiveLessonId = stage.InteractiveLessonId,
            Title = stage.Title,
            Description = stage.Description,
            StageType = stage.StageType,
            ArrangementType = stage.ArrangementType,
            TextContent = stage.TextContent,
            Order = stage.Order,
            IsActive = stage.IsActive,
            CreatedAt = stage.CreatedAt,
            UpdatedAt = stage.UpdatedAt,
            ContentItems = stage.ContentItems.Select(MapContentItemToDto).ToList()
        };
    }

    private static StageContentItemDto MapContentItemToDto(StageContentItem item)
    {
        return new StageContentItemDto
        {
            Id = item.Id,
            InteractiveLessonStageId = item.InteractiveLessonStageId,
            Order = item.Order,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            EducationalContentId = item.EducationalContentId,
            InteractiveQuestionId = item.InteractiveQuestionId,
            ContentType = item.GetContentType()
        };
    }
}

public class DeleteInteractiveLessonStageCommandHandler : IRequestHandler<DeleteInteractiveLessonStageCommand, Result>
{
    private readonly IRepository<InteractiveLessonStage> _stageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInteractiveLessonStageCommandHandler(
        IRepository<InteractiveLessonStage> stageRepository,
        IUnitOfWork unitOfWork)
    {
        _stageRepository = stageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteInteractiveLessonStageCommand request, CancellationToken cancellationToken)
    {
        var stage = await _stageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (stage == null)
            return Result.Failure("Stage not found");

        await _stageRepository.DeleteAsync(stage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public class AddStageContentItemCommandHandler : IRequestHandler<AddStageContentItemCommand, Result<StageContentItemDto>>
{
    private readonly IRepository<StageContentItem> _contentItemRepository;
    private readonly IRepository<InteractiveLessonStage> _stageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddStageContentItemCommandHandler(
        IRepository<StageContentItem> contentItemRepository,
        IRepository<InteractiveLessonStage> stageRepository,
        IUnitOfWork unitOfWork)
    {
        _contentItemRepository = contentItemRepository;
        _stageRepository = stageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StageContentItemDto>> Handle(AddStageContentItemCommand request, CancellationToken cancellationToken)
    {
        var stage = await _stageRepository.GetByIdAsync(request.InteractiveLessonStageId, cancellationToken);
        if (stage == null)
            return Result<StageContentItemDto>.Failure("Stage not found");

        var contentItem = StageContentItem.Create(
            request.InteractiveLessonStageId,
            request.Order,
            request.EducationalContentId,
            request.InteractiveQuestionId
        );

        await _contentItemRepository.AddAsync(contentItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StageContentItemDto>.Success(MapToDto(contentItem));
    }

    private static StageContentItemDto MapToDto(StageContentItem item)
    {
        return new StageContentItemDto
        {
            Id = item.Id,
            InteractiveLessonStageId = item.InteractiveLessonStageId,
            Order = item.Order,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            EducationalContentId = item.EducationalContentId,
            InteractiveQuestionId = item.InteractiveQuestionId,
            ContentType = item.GetContentType()
        };
    }
}

public class AddSubChapterToInteractiveLessonCommandHandler : IRequestHandler<AddSubChapterToInteractiveLessonCommand, Result<InteractiveLessonSubChapterDto>>
{
    private readonly IRepository<InteractiveLessonSubChapter> _subChapterRepository;
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;
    private readonly IRepository<SubChapter> _subChapterRepo;
    private readonly IUnitOfWork _unitOfWork;

    public AddSubChapterToInteractiveLessonCommandHandler(
        IRepository<InteractiveLessonSubChapter> subChapterRepository,
        IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository,
        IRepository<SubChapter> subChapterRepo,
        IUnitOfWork unitOfWork)
    {
        _subChapterRepository = subChapterRepository;
        _interactiveLessonRepository = interactiveLessonRepository;
        _subChapterRepo = subChapterRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<InteractiveLessonSubChapterDto>> Handle(AddSubChapterToInteractiveLessonCommand request, CancellationToken cancellationToken)
    {
        var interactiveLesson = await _interactiveLessonRepository.GetByIdAsync(request.InteractiveLessonId, cancellationToken);
        if (interactiveLesson == null)
            return Result<InteractiveLessonSubChapterDto>.Failure("Interactive lesson not found");

        var subChapter = await _subChapterRepo.GetByIdAsync(request.SubChapterId, cancellationToken);
        if (subChapter == null)
            return Result<InteractiveLessonSubChapterDto>.Failure("Sub-chapter not found");

        var interactiveLessonSubChapter = InteractiveLessonSubChapter.Create(
            request.InteractiveLessonId,
            request.SubChapterId,
            request.Order
        );

        await _subChapterRepository.AddAsync(interactiveLessonSubChapter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<InteractiveLessonSubChapterDto>.Success(MapToDto(interactiveLessonSubChapter));
    }

    private static InteractiveLessonSubChapterDto MapToDto(InteractiveLessonSubChapter item)
    {
        return new InteractiveLessonSubChapterDto
        {
            Id = item.Id,
            InteractiveLessonId = item.InteractiveLessonId,
            SubChapterId = item.SubChapterId,
            Order = item.Order,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
