using EduTrack.Application.Common.Models;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.Progress.Commands;

public record UpdateProgressCommand(
    string StudentId,
    int? LessonId,
    int? ExamId,
    ProgressStatus Status,
    int CorrectCount,
    int Streak) : IRequest<Result<ProgressDto>>;

public record CompleteLessonCommand(
    string StudentId,
    int LessonId,
    bool Passed) : IRequest<Result<ProgressDto>>;

public record CompleteExamCommand(
    string StudentId,
    int ExamId,
    int Score,
    int TotalQuestions,
    int CorrectAnswers,
    bool Passed) : IRequest<Result<ProgressDto>>;
