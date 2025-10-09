using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Commands;

public record CreateTeachingSessionReportCommand(
    int TeachingPlanId,
    string? Title,
    DateTimeOffset SessionDate,
    SessionMode Mode,
    string? Location,
    string? Notes) : IRequest<Result<TeachingSessionReportDto>>;

public record UpdateTeachingSessionReportCommand(
    int Id,
    string? Title,
    DateTimeOffset SessionDate,
    SessionMode Mode,
    string? Location,
    string? Notes) : IRequest<Result<TeachingSessionReportDto>>;

public record RecordAttendanceCommand(
    int TeachingSessionReportId,
    List<TeachingSessionAttendanceDto> Attendance) : IRequest<Result<bool>>;

public record CreateTargetedScheduleItemsFromReportCommand(
    int ReportId,
    ScheduleItemType Type,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    string ContentJson,
    decimal? MaxScore,
    List<int>? TargetStudentIds,
    List<int>? TargetGroupIds) : IRequest<Result<List<ScheduleItemDto>>>;
