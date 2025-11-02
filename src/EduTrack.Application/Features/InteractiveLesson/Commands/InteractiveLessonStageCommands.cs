using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.InteractiveLesson.Commands;

// Commands for InteractiveLessonStage
public class CreateInteractiveLessonStageCommand : IRequest<Result<InteractiveLessonStageDto>>
{
    public int InteractiveLessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InteractiveLessonStageType StageType { get; set; }
    public ContentArrangementType ArrangementType { get; set; }
    public string? TextContent { get; set; }
    public int Order { get; set; }
}

public class UpdateInteractiveLessonStageCommand : IRequest<Result<InteractiveLessonStageDto>>
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InteractiveLessonStageType StageType { get; set; }
    public ContentArrangementType ArrangementType { get; set; }
    public string? TextContent { get; set; }
    public int Order { get; set; }
}

public class DeleteInteractiveLessonStageCommand : IRequest<Result>
{
    public int Id { get; set; }
}

// Commands for StageContentItem
public class AddStageContentItemCommand : IRequest<Result<StageContentItemDto>>
{
    public int InteractiveLessonStageId { get; set; }
    public int Order { get; set; }
    public int InteractiveQuestionId { get; set; } // EducationalContent removed - only questions supported
}

public class UpdateStageContentItemCommand : IRequest<Result<StageContentItemDto>>
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public class RemoveStageContentItemCommand : IRequest<Result>
{
    public int Id { get; set; }
}

// Commands for InteractiveLessonSubChapter
public class AddSubChapterToInteractiveLessonCommand : IRequest<Result<InteractiveLessonSubChapterDto>>
{
    public int InteractiveLessonId { get; set; }
    public int SubChapterId { get; set; }
    public int Order { get; set; }
}

public class UpdateInteractiveLessonSubChapterCommand : IRequest<Result<InteractiveLessonSubChapterDto>>
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public class RemoveSubChapterFromInteractiveLessonCommand : IRequest<Result>
{
    public int Id { get; set; }
}

// Enhanced InteractiveLesson Commands
public class CreateInteractiveLessonWithStagesCommand : IRequest<Result<InteractiveLessonDto>>
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<int> SubChapterIds { get; set; } = new();
    public List<CreateInteractiveLessonStageCommand> Stages { get; set; } = new();
}