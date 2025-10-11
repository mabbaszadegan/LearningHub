using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Commands;

public record CreateScheduleItemCommand(
    int TeachingPlanId,
    int? GroupId,
    int? LessonId,
    ScheduleItemType Type,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    DisciplineType? DisciplineHint,
    string ContentJson,
    decimal? MaxScore
) : IRequest<Result<int>>;

public record UpdateScheduleItemCommand(
    int Id,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    string ContentJson,
    decimal? MaxScore
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
    int? GroupId,
    int? LessonId
) : IRequest<Result<int>>;

public record CompleteScheduleItemCommand(int Id) : IRequest<Result>;
