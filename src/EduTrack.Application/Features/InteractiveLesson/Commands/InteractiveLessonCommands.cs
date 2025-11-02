using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.InteractiveLesson.Commands;

public record CreateInteractiveLessonCommand(
    int CourseId,
    string Title,
    string? Description,
    int Order,
    string CreatedBy) : IRequest<Result<InteractiveLessonDto>>;

public record UpdateInteractiveLessonCommand(
    int Id,
    string Title,
    string? Description,
    int Order,
    bool IsActive) : IRequest<Result<InteractiveLessonDto>>;

public record DeleteInteractiveLessonCommand(int Id) : IRequest<Result<bool>>;

// AddContentToInteractiveLessonCommand removed - EducationalContent entity removed

public record AddQuestionToInteractiveLessonCommand(
    int InteractiveLessonId,
    string QuestionText,
    string? Description,
    InteractiveQuestionType Type,
    int? ImageFileId,
    string? CorrectAnswer,
    int Points,
    int Order) : IRequest<Result<InteractiveContentItemDto>>;

public record RemoveContentFromInteractiveLessonCommand(
    int InteractiveContentItemId) : IRequest<Result<bool>>;

public record ReorderInteractiveContentCommand(
    int InteractiveLessonId,
    List<int> ContentItemIds) : IRequest<Result<bool>>;

public record CreateInteractiveQuestionCommand(
    string QuestionText,
    string? Description,
    InteractiveQuestionType Type,
    int? ImageFileId,
    string? CorrectAnswer,
    int Points) : IRequest<Result<InteractiveQuestionDto>>;

public record AddQuestionChoiceCommand(
    int InteractiveQuestionId,
    string Text,
    bool IsCorrect,
    int Order) : IRequest<Result<QuestionChoiceDto>>;

public record SubmitStudentAnswerCommand(
    int InteractiveQuestionId,
    string StudentId,
    string? AnswerText,
    int? SelectedChoiceId,
    bool? BooleanAnswer) : IRequest<Result<StudentAnswerDto>>;

// Assignment Commands
public record AssignInteractiveLessonToClassCommand(
    int InteractiveLessonId,
    int ClassId,
    string AssignedBy,
    DateTimeOffset? DueDate = null) : IRequest<Result<InteractiveLessonAssignmentDto>>;

public record UnassignInteractiveLessonFromClassCommand(
    int InteractiveLessonId,
    int ClassId) : IRequest<Result<bool>>;

public record UpdateAssignmentDueDateCommand(
    int AssignmentId,
    DateTimeOffset? DueDate) : IRequest<Result<InteractiveLessonAssignmentDto>>;
