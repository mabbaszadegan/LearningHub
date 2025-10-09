using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class CreateTeachingSessionReportCommandValidator : AbstractValidator<CreateTeachingSessionReportCommand>
{
    public CreateTeachingSessionReportCommandValidator()
    {
        RuleFor(x => x.TeachingPlanId)
            .GreaterThan(0).WithMessage("Teaching Plan ID must be greater than 0");
        RuleFor(x => x.SessionDate)
            .NotEmpty().WithMessage("Session Date is required");
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
        RuleFor(x => x.Location)
            .MaximumLength(500).WithMessage("Location cannot exceed 500 characters");
        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithMessage("Notes cannot exceed 4000 characters");
    }
}

public class CreateTeachingSessionReportCommandHandler : IRequestHandler<CreateTeachingSessionReportCommand, Result<TeachingSessionReportDto>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateTeachingSessionReportCommandHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _sessionReportRepository = sessionReportRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TeachingSessionReportDto>> Handle(CreateTeachingSessionReportCommand request, CancellationToken cancellationToken)
    {
        var teacherId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(teacherId))
        {
            return Result<TeachingSessionReportDto>.Failure("User is not authenticated.");
        }

        var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
        if (teachingPlan == null)
        {
            return Result<TeachingSessionReportDto>.Failure($"Teaching Plan with ID {request.TeachingPlanId} not found.");
        }

        if (teachingPlan.TeacherId != teacherId)
        {
            return Result<TeachingSessionReportDto>.Failure("You don't have permission to create session reports for this teaching plan.");
        }

        var sessionReport = new TeachingSessionReport
        {
            TeachingPlanId = request.TeachingPlanId,
            Title = request.Title,
            SessionDate = request.SessionDate,
            Mode = request.Mode,
            Location = request.Location,
            Notes = request.Notes,
            CreatedByTeacherId = teacherId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Add to repository and save
        await _sessionReportRepository.AddAsync(sessionReport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sessionReportDto = new TeachingSessionReportDto
        {
            Id = sessionReport.Id,
            TeachingPlanId = sessionReport.TeachingPlanId,
            TeachingPlanTitle = teachingPlan.Title,
            Title = sessionReport.Title,
            SessionDate = sessionReport.SessionDate,
            Mode = sessionReport.Mode,
            Location = sessionReport.Location,
            TopicsJson = string.Empty, // Will be populated from completion data
            Notes = sessionReport.Notes,
            StatsJson = string.Empty, // Will be populated from completion data
            AttachmentsJson = string.Empty, // Will be populated from completion data
            CreatedByTeacherId = sessionReport.CreatedByTeacherId,
            CreatedByTeacherName = _currentUserService.UserName ?? "Unknown",
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt,
            AttendanceCount = 0,
            PresentCount = 0,
            AbsentCount = 0
        };

        return Result<TeachingSessionReportDto>.Success(sessionReportDto);
    }
}
