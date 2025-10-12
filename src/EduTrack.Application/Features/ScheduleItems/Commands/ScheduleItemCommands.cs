using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Commands;

    public record CreateScheduleItemCommand(
        int TeachingPlanId,
        int? GroupId, // Legacy single group assignment
        ScheduleItemType Type,
        string Title,
        string? Description,
        DateTimeOffset StartDate,
        DateTimeOffset? DueDate,
        bool IsMandatory,
        DisciplineType? DisciplineHint,
        string ContentJson,
        decimal? MaxScore,
        List<int>? GroupIds = null, // New multi-group assignment
        List<int>? SubChapterIds = null // New subchapter assignment
    ) : IRequest<Result<int>>;

public record UpdateScheduleItemCommand(
    int Id,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    string ContentJson,
    decimal? MaxScore,
    List<int>? GroupIds = null, // New multi-group assignment
    List<int>? SubChapterIds = null // New subchapter assignment
) : IRequest<Result>;

public record DeleteScheduleItemCommand(int Id) : IRequest<Result>;

public record PublishScheduleItemCommand(int Id) : IRequest<Result>;

public record UnpublishScheduleItemCommand(int Id) : IRequest<Result>;

public record SaveScheduleItemStepCommand(
    int? Id,
    int TeachingPlanId,
    int Step,
    ScheduleItemType? Type,
    string? Title,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? DueDate,
    bool? IsMandatory,
    string? ContentJson,
    decimal? MaxScore,
    int? GroupId, // Legacy single group assignment
    string? PersianStartDate,
    string? PersianDueDate,
    string? StartTime,
    string? DueTime,
    List<int>? GroupIds = null, // New multi-group assignment
    List<int>? SubChapterIds = null // New subchapter assignment
) : IRequest<Result<int>>;

public record CompleteScheduleItemCommand(int Id) : IRequest<Result>;
