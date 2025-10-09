using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class ListTeachingSessionReportsQueryHandler : IRequestHandler<ListTeachingSessionReportsQuery, Result<List<TeachingSessionReportDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ICurrentUserService _currentUserService;

    public ListTeachingSessionReportsQueryHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        ICurrentUserService currentUserService)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _sessionReportRepository = sessionReportRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<TeachingSessionReportDto>>> Handle(ListTeachingSessionReportsQuery request, CancellationToken cancellationToken)
    {
        var teacherId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(teacherId))
        {
            return Result<List<TeachingSessionReportDto>>.Failure("User is not authenticated.");
        }

        var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
        if (teachingPlan == null)
        {
            return Result<List<TeachingSessionReportDto>>.Failure($"Teaching Plan with ID {request.TeachingPlanId} not found.");
        }

        if (teachingPlan.TeacherId != teacherId)
        {
            return Result<List<TeachingSessionReportDto>>.Failure("You don't have permission to view session reports for this teaching plan.");
        }

        // Get session reports for the teaching plan
        var sessionReports = await _sessionReportRepository.GetByPlanIdAsync(request.TeachingPlanId, cancellationToken);
        
        var sessionReportDtos = sessionReports.Select(sr => new TeachingSessionReportDto
        {
            Id = sr.Id,
            TeachingPlanId = sr.TeachingPlanId,
            TeachingPlanTitle = teachingPlan.Title,
            Title = sr.Title,
            SessionDate = sr.SessionDate,
            Mode = sr.Mode,
            Location = sr.Location,
            TopicsJson = string.Empty, // Will be populated from completion data
            Notes = sr.Notes,
            StatsJson = string.Empty, // Will be populated from completion data
            AttachmentsJson = string.Empty, // Will be populated from completion data
            CreatedByTeacherId = sr.CreatedByTeacherId,
            CreatedByTeacherName = "Teacher", // TODO: Get from user service
            CreatedAt = sr.CreatedAt,
            UpdatedAt = sr.UpdatedAt,
            AttendanceCount = sr.Attendance.Count,
            PresentCount = sr.Attendance.Count(a => a.Status == AttendanceStatus.Present),
            AbsentCount = sr.Attendance.Count(a => a.Status == AttendanceStatus.Absent)
        }).ToList();

        return Result<List<TeachingSessionReportDto>>.Success(sessionReportDtos);
    }
}
