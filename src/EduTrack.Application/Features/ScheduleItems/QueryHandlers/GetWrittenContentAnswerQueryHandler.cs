using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

public class GetWrittenContentAnswerQueryHandler : IRequestHandler<GetWrittenContentAnswerQuery, WrittenContentAnswer?>
{
    private readonly IWrittenContentAnswerRepository _answerRepository;

    public GetWrittenContentAnswerQueryHandler(IWrittenContentAnswerRepository answerRepository)
    {
        _answerRepository = answerRepository;
    }

    public async Task<WrittenContentAnswer?> Handle(GetWrittenContentAnswerQuery request, CancellationToken cancellationToken)
    {
        var answer = await _answerRepository.GetAnswerByStudentAndScheduleItemAsync(
            request.StudentId, request.ScheduleItemId, cancellationToken);

        if (answer == null)
        {
            return null;
        }

        // Map StudentAnswer to WrittenContentAnswer
        return new WrittenContentAnswer
        {
            Id = answer.Id,
            ScheduleItemId = answer.InteractiveQuestionId, // Using InteractiveQuestionId as ScheduleItemId
            StudentId = answer.StudentId,
            SubmittedAt = answer.AnsweredAt,
            GradedAt = answer.GradedAt,
            TotalScore = answer.PointsEarned,
            TeacherFeedback = answer.Feedback,
            Status = answer.IsGraded() ? WrittenAnswerStatus.Graded : WrittenAnswerStatus.Submitted,
            QuestionAnswers = new List<WrittenQuestionAnswer>
            {
                new WrittenQuestionAnswer
                {
                    StudentAnswerId = answer.Id,
                    QuestionBlockId = "main",
                    AnswerText = answer.AnswerText ?? "",
                    Score = answer.PointsEarned,
                    TeacherFeedback = answer.Feedback,
                    IsGraded = answer.IsGraded()
                }
            }
        };
    }
}
