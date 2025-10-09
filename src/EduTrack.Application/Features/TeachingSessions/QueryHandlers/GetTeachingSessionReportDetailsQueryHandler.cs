using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetTeachingSessionReportDetailsQueryHandler : IRequestHandler<GetTeachingSessionReportDetailsQuery, Result<TeachingSessionReportDto>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTeachingSessionReportDetailsQueryHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ICurrentUserService currentUserService)
    {
        _sessionReportRepository = sessionReportRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TeachingSessionReportDto>> Handle(GetTeachingSessionReportDetailsQuery request, CancellationToken cancellationToken)
    {
        var teacherId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(teacherId))
        {
            return Result<TeachingSessionReportDto>.Failure("User is not authenticated.");
        }

        var sessionReport = await _sessionReportRepository.GetByIdAsync(request.ReportId, cancellationToken);
        if (sessionReport == null)
        {
            return Result<TeachingSessionReportDto>.Failure("Session report not found.");
        }

        // Check if the teacher has permission to view this session report
        if (sessionReport.CreatedByTeacherId != teacherId)
        {
            return Result<TeachingSessionReportDto>.Failure("You don't have permission to view this session report.");
        }

        var sessionReportDto = new TeachingSessionReportDto
        {
            Id = sessionReport.Id,
            TeachingPlanId = sessionReport.TeachingPlanId,
            TeachingPlanTitle = sessionReport.TeachingPlan?.Title ?? "Unknown Plan",
            Title = sessionReport.Title,
            SessionDate = sessionReport.SessionDate,
            Mode = sessionReport.Mode,
            Location = sessionReport.Location,
            TopicsJson = string.Empty, // Will be populated from completion data
            Notes = sessionReport.Notes,
            StatsJson = string.Empty, // Will be populated from completion data
            AttachmentsJson = string.Empty, // Will be populated from completion data
            CreatedByTeacherId = sessionReport.CreatedByTeacherId,
            CreatedByTeacherName = "Teacher", // TODO: Get from user service
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt,
            AttendanceCount = sessionReport.Attendance.Count,
            PresentCount = sessionReport.Attendance.Count(a => a.Status == AttendanceStatus.Present),
            AbsentCount = sessionReport.Attendance.Count(a => a.Status == AttendanceStatus.Absent)
        };

        return Result<TeachingSessionReportDto>.Success(sessionReportDto);
    }
}
