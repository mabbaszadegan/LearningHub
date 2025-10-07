using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class SubmitWorkCommandValidator : AbstractValidator<SubmitWorkCommand>
{
    public SubmitWorkCommandValidator()
    {
        RuleFor(x => x.ScheduleItemId)
            .GreaterThan(0).WithMessage("Schedule Item ID must be greater than 0");

        RuleFor(x => x.PayloadJson)
            .NotEmpty().WithMessage("Payload JSON is required");
    }
}

public class SubmitWorkCommandHandler : IRequestHandler<SubmitWorkCommand, Result<SubmissionDto>>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitWorkCommandHandler(
        ISubmissionRepository submissionRepository,
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _submissionRepository = submissionRepository;
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SubmissionDto>> Handle(SubmitWorkCommand request, CancellationToken cancellationToken)
    {
        var studentId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(studentId))
        {
            return Result<SubmissionDto>.Failure("User not authenticated");
        }

        // Verify schedule item exists
        var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.ScheduleItemId, cancellationToken);
        if (scheduleItem == null)
        {
            return Result<SubmissionDto>.Failure("Schedule item not found");
        }

        // Check if submission already exists
        var existingSubmission = await _submissionRepository.GetSubmissionByStudentAndItemAsync(studentId, request.ScheduleItemId, cancellationToken);
        
        Submission submission;
        if (existingSubmission != null)
        {
            // Update existing submission
            existingSubmission.UpdatePayload(request.PayloadJson);
            if (!string.IsNullOrEmpty(request.AttachmentsJson))
            {
                existingSubmission.UpdateAttachments(request.AttachmentsJson);
            }
            existingSubmission.Submit();
            submission = existingSubmission;
        }
        else
        {
            // Create new submission
            submission = Submission.Create(request.ScheduleItemId, studentId, request.PayloadJson, request.AttachmentsJson);
            submission.Start();
            submission.Submit();
            await _submissionRepository.AddAsync(submission, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var submissionDto = new SubmissionDto
        {
            Id = submission.Id,
            ScheduleItemId = submission.ScheduleItemId,
            ScheduleItemTitle = scheduleItem.Title,
            StudentId = submission.StudentId,
            StudentName = _currentUserService.UserName ?? "Unknown",
            SubmittedAt = submission.SubmittedAt,
            Status = submission.Status,
            Grade = submission.Grade,
            FeedbackText = submission.FeedbackText,
            TeacherId = submission.TeacherId,
            PayloadJson = submission.PayloadJson,
            AttachmentsJson = submission.AttachmentsJson,
            ReviewedAt = submission.ReviewedAt,
            UpdatedAt = submission.UpdatedAt,
            PercentageScore = submission.GetPercentageScore(),
            IsPassing = submission.IsPassing()
        };

        return Result<SubmissionDto>.Success(submissionDto);
    }
}
