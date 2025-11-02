using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using EduTrack.Domain.Entities;
using MediatR;
using EduTrack.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.InteractiveLesson.Queries;

// Queries for InteractiveLessonStage
public class GetInteractiveLessonStagesQuery : IRequest<Result<List<InteractiveLessonStageDto>>>
{
    public int InteractiveLessonId { get; set; }
}

public class GetInteractiveLessonStageByIdQuery : IRequest<Result<InteractiveLessonStageDto>>
{
    public int Id { get; set; }
}

// Queries for StageContentItem
public class GetStageContentItemsQuery : IRequest<Result<List<StageContentItemDto>>>
{
    public int InteractiveLessonStageId { get; set; }
}

// Queries for InteractiveLessonSubChapter
public class GetInteractiveLessonSubChaptersQuery : IRequest<Result<List<InteractiveLessonSubChapterDto>>>
{
    public int InteractiveLessonId { get; set; }
}

// Enhanced InteractiveLesson Queries
public class GetInteractiveLessonWithStagesQuery : IRequest<Result<InteractiveLessonWithStagesDto>>
{
    public int Id { get; set; }
}

public class GetInteractiveLessonsWithStagesQuery : IRequest<Result<List<InteractiveLessonWithStagesDto>>>
{
    public int? CourseId { get; set; }
    public bool? IsActive { get; set; }
}

// Query Handlers
public class GetInteractiveLessonStagesQueryHandler : IRequestHandler<GetInteractiveLessonStagesQuery, Result<List<InteractiveLessonStageDto>>>
{
    private readonly IRepository<InteractiveLessonStage> _stageRepository;

    public GetInteractiveLessonStagesQueryHandler(IRepository<InteractiveLessonStage> stageRepository)
    {
        _stageRepository = stageRepository;
    }

    public async Task<Result<List<InteractiveLessonStageDto>>> Handle(GetInteractiveLessonStagesQuery request, CancellationToken cancellationToken)
    {
        var stages = await _stageRepository.GetAll()
            .Where(s => s.InteractiveLessonId == request.InteractiveLessonId && s.IsActive)
            .OrderBy(s => s.Order)
            .Include(s => s.ContentItems.Where(ci => ci.IsActive))
                // EducationalContent removed
            .Include(s => s.ContentItems.Where(ci => ci.IsActive))
                .ThenInclude(ci => ci.InteractiveQuestion!)
                    .ThenInclude(iq => iq!.Choices)
            .Include(s => s.ContentItems.Where(ci => ci.IsActive))
                .ThenInclude(ci => ci.InteractiveQuestion!)
                    .ThenInclude(iq => iq!.ImageFile)
            .ToListAsync(cancellationToken);

        var stageDtos = stages.Select(MapToDto).ToList();
        return Result<List<InteractiveLessonStageDto>>.Success(stageDtos);
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
            InteractiveQuestionId = item.InteractiveQuestionId,
            ContentType = item.GetContentType(),
            // EducationalContent removed - using InteractiveQuestion only
            InteractiveQuestion = item.InteractiveQuestion != null ? MapInteractiveQuestionToDto(item.InteractiveQuestion) : null
        };
    }

    // MapEducationalContentToDto removed - EducationalContent entity removed

    private static InteractiveQuestionDto MapInteractiveQuestionToDto(InteractiveQuestion question)
    {
        return new InteractiveQuestionDto
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
            Choices = question.Choices.Select(MapQuestionChoiceToDto).ToList(),
            ImageFile = question.ImageFile != null ? MapFileToDto(question.ImageFile) : null
        };
    }

    private static QuestionChoiceDto MapQuestionChoiceToDto(QuestionChoice choice)
    {
        return new QuestionChoiceDto
        {
            Id = choice.Id,
            InteractiveQuestionId = choice.InteractiveQuestionId,
            Text = choice.Text,
            IsCorrect = choice.IsCorrect,
            Order = choice.Order
        };
    }

    private static DTOs.FileDto MapFileToDto(Domain.Entities.File file)
    {
        return new DTOs.FileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            FilePath = file.FilePath,
            MimeType = file.MimeType,
            FileSize = file.FileSizeBytes,
            MD5Hash = file.MD5Hash,
            CreatedAt = file.CreatedAt,
            CreatedBy = file.CreatedBy
        };
    }
}

public class GetInteractiveLessonWithStagesQueryHandler : IRequestHandler<GetInteractiveLessonWithStagesQuery, Result<InteractiveLessonWithStagesDto>>
{
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;

    public GetInteractiveLessonWithStagesQueryHandler(IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository)
    {
        _interactiveLessonRepository = interactiveLessonRepository;
    }

    public async Task<Result<InteractiveLessonWithStagesDto>> Handle(GetInteractiveLessonWithStagesQuery request, CancellationToken cancellationToken)
    {
        var interactiveLesson = await _interactiveLessonRepository.GetAll()
            .Where(il => il.Id == request.Id)
            .Include(il => il.Stages.Where(s => s.IsActive))
                .ThenInclude(s => s.ContentItems.Where(ci => ci.IsActive))
                    // EducationalContent removed
            .Include(il => il.Stages.Where(s => s.IsActive))
                .ThenInclude(s => s.ContentItems.Where(ci => ci.IsActive))
                    .ThenInclude(ci => ci.InteractiveQuestion!)
                        .ThenInclude(iq => iq!.Choices)
            .Include(il => il.Stages.Where(s => s.IsActive))
                .ThenInclude(s => s.ContentItems.Where(ci => ci.IsActive))
                    .ThenInclude(ci => ci.InteractiveQuestion!)
                        .ThenInclude(iq => iq!.ImageFile)
            .Include(il => il.SubChapters.Where(sc => sc.IsActive))
                .ThenInclude(sc => sc.SubChapter)
                    // EducationalContents removed
            .FirstOrDefaultAsync(cancellationToken);

        if (interactiveLesson == null)
            return Result<InteractiveLessonWithStagesDto>.Failure("Interactive lesson not found");

        var dto = MapToDto(interactiveLesson);
        return Result<InteractiveLessonWithStagesDto>.Success(dto);
    }

    private static InteractiveLessonWithStagesDto MapToDto(Domain.Entities.InteractiveLesson lesson)
    {
        return new InteractiveLessonWithStagesDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            IsActive = lesson.IsActive,
            Order = lesson.Order,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            CreatedBy = lesson.CreatedBy,
            Stages = lesson.Stages.Select(MapStageToDto).ToList(),
            SubChapters = lesson.SubChapters.Select(MapSubChapterToDto).ToList()
        };
    }

    private static InteractiveLessonStageDto MapStageToDto(InteractiveLessonStage stage)
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

    private static InteractiveLessonSubChapterDto MapSubChapterToDto(InteractiveLessonSubChapter item)
    {
        return new InteractiveLessonSubChapterDto
        {
            Id = item.Id,
            InteractiveLessonId = item.InteractiveLessonId,
            SubChapterId = item.SubChapterId,
            Order = item.Order,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            SubChapter = item.SubChapter != null ? MapSubChapterToDto(item.SubChapter) : null
        };
    }

    private static DTOs.SubChapterDto MapSubChapterToDto(Domain.Entities.SubChapter subChapter)
    {
        return new DTOs.SubChapterDto
        {
            Id = subChapter.Id,
            ChapterId = subChapter.ChapterId,
            Title = subChapter.Title,
            Description = subChapter.Description,
            Objective = subChapter.Objective,
            IsActive = subChapter.IsActive,
            Order = subChapter.Order,
            CreatedAt = subChapter.CreatedAt,
            UpdatedAt = subChapter.UpdatedAt,
            // EducationalContents removed - EducationalContent entity removed
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
            InteractiveQuestionId = item.InteractiveQuestionId,
            ContentType = item.GetContentType(),
            // EducationalContent removed - using InteractiveQuestion only
            InteractiveQuestion = item.InteractiveQuestion != null ? MapInteractiveQuestionToDto(item.InteractiveQuestion) : null
        };
    }

    // MapEducationalContentToDto removed - EducationalContent entity removed

    private static InteractiveQuestionDto MapInteractiveQuestionToDto(InteractiveQuestion question)
    {
        return new InteractiveQuestionDto
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
            Choices = question.Choices.Select(MapQuestionChoiceToDto).ToList(),
            ImageFile = question.ImageFile != null ? MapFileToDto(question.ImageFile) : null
        };
    }

    private static QuestionChoiceDto MapQuestionChoiceToDto(QuestionChoice choice)
    {
        return new QuestionChoiceDto
        {
            Id = choice.Id,
            InteractiveQuestionId = choice.InteractiveQuestionId,
            Text = choice.Text,
            IsCorrect = choice.IsCorrect,
            Order = choice.Order
        };
    }

    private static DTOs.FileDto MapFileToDto(Domain.Entities.File file)
    {
        return new DTOs.FileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            FilePath = file.FilePath,
            MimeType = file.MimeType,
            FileSize = file.FileSizeBytes,
            MD5Hash = file.MD5Hash,
            CreatedAt = file.CreatedAt,
            CreatedBy = file.CreatedBy
        };
    }
}
