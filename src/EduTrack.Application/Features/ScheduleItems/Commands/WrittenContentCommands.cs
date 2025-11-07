using EduTrack.Application.Common.Models.ScheduleItems;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Commands;

// Submit written content answer by student
public record SubmitWrittenContentAnswerCommand(
    int ScheduleItemId,
    string StudentId,
    List<WrittenQuestionAnswer> QuestionAnswers,
    int? StudentProfileId = null
) : IRequest<int>;

// Grade written content answer by teacher
public record GradeWrittenContentAnswerCommand(
    int AnswerId,
    List<WrittenQuestionAnswer> GradedAnswers,
    string? TeacherFeedback,
    decimal TotalScore
) : IRequest;

// Return graded answer to student
public record ReturnWrittenContentAnswerCommand(
    int AnswerId,
    string? AdditionalFeedback
) : IRequest;
