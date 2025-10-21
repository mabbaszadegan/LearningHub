using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class SubmitWrittenContentAnswerCommandHandler : IRequestHandler<SubmitWrittenContentAnswerCommand, int>
{
    private readonly IWrittenContentAnswerRepository _answerRepository;
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitWrittenContentAnswerCommandHandler(
        IWrittenContentAnswerRepository answerRepository,
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork)
    {
        _answerRepository = answerRepository;
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(SubmitWrittenContentAnswerCommand request, CancellationToken cancellationToken)
    {
        // Check if schedule item exists and is of Writing type
        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.ScheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            throw new ArgumentException("Schedule item not found");
        }

        if (scheduleItem.Type != Domain.Enums.ScheduleItemType.Writing)
        {
            throw new ArgumentException("Schedule item is not of Writing type");
        }

        // Check if student already submitted an answer
        var existingAnswer = await _answerRepository.GetAnswerByStudentAndScheduleItemAsync(
            request.StudentId, request.ScheduleItemId, cancellationToken);

        if (existingAnswer != null)
        {
            throw new InvalidOperationException("Student has already submitted an answer for this schedule item");
        }

        // Create new answer using StudentAnswer entity
        var answer = StudentAnswer.Create(
            interactiveQuestionId: request.ScheduleItemId, // Using ScheduleItemId as InteractiveQuestionId
            studentId: request.StudentId,
            answerText: string.Join("\n---\n", request.QuestionAnswers.Select(qa => $"{qa.QuestionBlockId}: {qa.AnswerText}"))
        );

        await _answerRepository.AddAsync(answer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return answer.Id;
    }
}
