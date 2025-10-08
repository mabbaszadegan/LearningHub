using EduTrack.Application.Common.Models;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.Commands;

// TeachingPlan Commands
public record CreateTeachingPlanCommand(
    int CourseId,
    string Title,
    string? Description,
    string? Objectives) : IRequest<Result<TeachingPlanDto>>;

public record UpdateTeachingPlanCommand(
    int Id,
    string Title,
    string? Description,
    string? Objectives) : IRequest<Result<TeachingPlanDto>>;

public record DeleteTeachingPlanCommand(int Id) : IRequest<Result<bool>>;

// StudentGroup Commands
public record CreateStudentGroupCommand(
    int TeachingPlanId,
    string Name) : IRequest<Result<StudentGroupDto>>;

public record UpdateStudentGroupCommand(
    int Id,
    string Name) : IRequest<Result<StudentGroupDto>>;

public record DeleteStudentGroupCommand(int Id) : IRequest<Result<bool>>;

public record AddGroupMembersCommand(
    int GroupId,
    List<string> StudentIds) : IRequest<Result<bool>>;

public record RemoveGroupMemberCommand(
    int GroupId,
    string StudentId) : IRequest<Result<bool>>;

public record TransferGroupMemberCommand(
    int FromGroupId,
    int ToGroupId,
    string StudentId) : IRequest<Result<bool>>;

// ScheduleItem Commands
public record CreateScheduleItemCommand(
    int TeachingPlanId,
    ScheduleItemType Type,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    string ContentJson,
    decimal? MaxScore = null,
    int? GroupId = null,
    int? LessonId = null,
    DisciplineType? DisciplineHint = null) : IRequest<Result<ScheduleItemDto>>;

public record UpdateScheduleItemCommand(
    int Id,
    string Title,
    string? Description,
    DateTimeOffset StartDate,
    DateTimeOffset? DueDate,
    bool IsMandatory,
    string ContentJson,
    decimal? MaxScore = null,
    int? GroupId = null,
    int? LessonId = null) : IRequest<Result<ScheduleItemDto>>;

public record DeleteScheduleItemCommand(int Id) : IRequest<Result<bool>>;

// Submission Commands
public record SubmitWorkCommand(
    int ScheduleItemId,
    string PayloadJson,
    string? AttachmentsJson = null) : IRequest<Result<SubmissionDto>>;

public record UpdateSubmissionStatusCommand(
    int Id,
    SubmissionStatus Status) : IRequest<Result<SubmissionDto>>;

public record GradeSubmissionCommand(
    int Id,
    decimal Grade,
    string? FeedbackText = null) : IRequest<Result<SubmissionDto>>;

// Learning Mode Commands
public record UpdateLearningModeCommand(
    int CourseId,
    LearningMode LearningMode) : IRequest<Result<bool>>;
