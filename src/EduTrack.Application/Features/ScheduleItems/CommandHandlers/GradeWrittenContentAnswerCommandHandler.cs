using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class GradeWrittenContentAnswerCommandHandler : IRequestHandler<GradeWrittenContentAnswerCommand>
{
    private readonly IWrittenContentAnswerRepository _answerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GradeWrittenContentAnswerCommandHandler(
        IWrittenContentAnswerRepository answerRepository,
        IUnitOfWork unitOfWork)
    {
        _answerRepository = answerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(GradeWrittenContentAnswerCommand request, CancellationToken cancellationToken)
    {
        var answer = await _answerRepository.GetByIdAsync(request.AnswerId, cancellationToken);
        if (answer == null)
        {
            throw new ArgumentException("Answer not found");
        }

        // Grade the answer using the existing Grade method
        answer.Grade(
            isCorrect: request.TotalScore > 0,
            pointsEarned: (int)request.TotalScore,
            feedback: request.TeacherFeedback
        );

        await _answerRepository.UpdateAsync(answer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
