using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.QueryHandlers;

public class GetWrittenContentAnswerByIdQueryHandler : IRequestHandler<GetWrittenContentAnswerByIdQuery, WrittenContentAnswer?>
{
    private readonly IWrittenContentAnswerRepository _answerRepository;

    public GetWrittenContentAnswerByIdQueryHandler(IWrittenContentAnswerRepository answerRepository)
    {
        _answerRepository = answerRepository;
    }

    public async Task<WrittenContentAnswer?> Handle(GetWrittenContentAnswerByIdQuery request, CancellationToken cancellationToken)
    {
        var answer = await _answerRepository.GetByIdAsync(request.AnswerId, cancellationToken);

        if (answer == null)
        {
            return null;
        }

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
