using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class ReturnWrittenContentAnswerCommandHandler : IRequestHandler<ReturnWrittenContentAnswerCommand>
{
    private readonly IWrittenContentAnswerRepository _answerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReturnWrittenContentAnswerCommandHandler(
        IWrittenContentAnswerRepository answerRepository,
        IUnitOfWork unitOfWork)
    {
        _answerRepository = answerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReturnWrittenContentAnswerCommand request, CancellationToken cancellationToken)
    {
        var answer = await _answerRepository.GetByIdAsync(request.AnswerId, cancellationToken);
        if (answer == null)
        {
            throw new ArgumentException("Answer not found");
        }

        if (!answer.IsGraded())
        {
            throw new InvalidOperationException("Answer must be graded before it can be returned");
        }

        // Add additional feedback if provided
        if (!string.IsNullOrEmpty(request.AdditionalFeedback))
        {
            var currentFeedback = answer.Feedback ?? "";
            answer.UpdateFeedback(string.IsNullOrEmpty(currentFeedback) 
                ? request.AdditionalFeedback 
                : $"{currentFeedback}\n\n{request.AdditionalFeedback}");
        }

        await _answerRepository.UpdateAsync(answer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
