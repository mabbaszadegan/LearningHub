using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetTeachingSessionReportDetailsQueryHandler : IRequestHandler<GetTeachingSessionReportDetailsQuery, Result<TeachingSessionReportDto>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStudentGroupRepository _studentGroupRepository;

    public GetTeachingSessionReportDetailsQueryHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ICurrentUserService currentUserService,
        IStudentGroupRepository studentGroupRepository)
    {
        _sessionReportRepository = sessionReportRepository;
        _currentUserService = currentUserService;
        _studentGroupRepository = studentGroupRepository;
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

        // Get all students from groups in this teaching plan
        var groups = await _studentGroupRepository.GetGroupsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken);
        var allStudents = groups.SelectMany(g => g.Members).ToList();
        
        // Create attendance DTOs with student information
        var attendanceDtos = allStudents.Select(member => 
        {
            var existingAttendance = sessionReport.Attendance.FirstOrDefault(a => a.StudentId == member.StudentId);
            return new TeachingSessionAttendanceDto
            {
                Id = existingAttendance?.Id ?? 0,
                TeachingSessionReportId = sessionReport.Id,
                StudentId = member.StudentId,
                StudentName = (member.Student?.FirstName + " " + member.Student?.LastName)?.Trim() ?? "Unknown Student",
                Status = existingAttendance?.Status ?? AttendanceStatus.Absent,
                ParticipationScore = existingAttendance?.ParticipationScore,
                Comment = existingAttendance?.Comment
            };
        }).ToList();

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
            AbsentCount = sessionReport.Attendance.Count(a => a.Status == AttendanceStatus.Absent),
            Attendance = attendanceDtos
        };

        return Result<TeachingSessionReportDto>.Success(sessionReportDto);
    }
}
