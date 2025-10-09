using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Exams;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.Exams.Commands;

public record CreateExamCommand(
    string Title,
    string? Description,
    int DurationMinutes,
    int PassingScore,
    bool ShowSolutions) : IRequest<Result<ExamDto>>;

public record UpdateExamCommand(
    int Id,
    string Title,
    string? Description,
    int DurationMinutes,
    int PassingScore,
    bool ShowSolutions,
    bool IsActive) : IRequest<Result<ExamDto>>;

public record DeleteExamCommand(int Id) : IRequest<Result<bool>>;

public record AddQuestionToExamCommand(
    int ExamId,
    int QuestionId) : IRequest<Result<bool>>;

public record RemoveQuestionFromExamCommand(
    int ExamId,
    int QuestionId) : IRequest<Result<bool>>;

public record StartExamCommand(
    int ExamId,
    string StudentId) : IRequest<Result<AttemptDto>>;

public record SubmitExamCommand(
    int AttemptId,
    List<AnswerSubmissionDto> Answers) : IRequest<Result<AttemptDto>>;

public record CreateQuestionCommand(
    string Text,
    QuestionType Type,
    string? Explanation,
    int Points,
    List<ChoiceDto> Choices) : IRequest<Result<QuestionDto>>;

public record UpdateQuestionCommand(
    int Id,
    string Text,
    QuestionType Type,
    string? Explanation,
    int Points,
    bool IsActive,
    List<ChoiceDto> Choices) : IRequest<Result<QuestionDto>>;

public record DeleteQuestionCommand(int Id) : IRequest<Result<bool>>;

public record AnswerSubmissionDto(
    int QuestionId,
    string? TextAnswer,
    int? SelectedChoiceId);
