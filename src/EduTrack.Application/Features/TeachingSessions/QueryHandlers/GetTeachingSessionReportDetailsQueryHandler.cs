using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

        // Get topics from TeachingSessionTopicCoverages
        var topics = sessionReport.TopicCoverages
            .Where(tc => !string.IsNullOrEmpty(tc.TopicTitle))
            .Select(tc => tc.TopicTitle)
            .Distinct()
            .ToList();
        
        var jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        var topicsJson = topics.Any() ? JsonConvert.SerializeObject(topics, jsonSettings) : string.Empty;
        
        // Parse step completions to get additional session summary data
        var statsJson = string.Empty;
        var attachmentsJson = string.Empty;
        
        // Also try to get data from step completions if available
        if (!string.IsNullOrEmpty(sessionReport.StepCompletionsJson))
        {
            try
            {
                var stepCompletions = JsonConvert.DeserializeObject<Dictionary<int, string>>(sessionReport.StepCompletionsJson);
                if (stepCompletions != null)
                {
                    // Get topics from step completions if not already available
                    if (string.IsNullOrEmpty(topicsJson) && stepCompletions.ContainsKey(1))
                    {
                        var step1Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(stepCompletions[1]);
                        if (step1Data != null && step1Data.ContainsKey("topics"))
                        {
                            topicsJson = step1Data["topics"]?.ToString() ?? string.Empty;
                        }
                    }
                    
                    // Get stats from step completions if not already available
                    if (string.IsNullOrEmpty(statsJson))
                    {
                        foreach (var step in stepCompletions)
                        {
                            if (step.Value.Contains("stats") || step.Value.Contains("summary"))
                            {
                                statsJson = step.Value;
                                break;
                            }
                        }
                    }
                    
                    // Get attachments from step completions if not already available
                    if (string.IsNullOrEmpty(attachmentsJson) && stepCompletions.ContainsKey(1))
                    {
                        var step1Data = JsonConvert.DeserializeObject<Dictionary<string, object>>(stepCompletions[1]);
                        if (step1Data != null && step1Data.ContainsKey("attachments"))
                        {
                            attachmentsJson = step1Data["attachments"]?.ToString() ?? string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with existing values
                Console.WriteLine($"Error parsing step completions: {ex.Message}");
            }
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
            TopicsJson = topicsJson,
            Notes = sessionReport.Notes,
            StatsJson = statsJson,
            AttachmentsJson = attachmentsJson,
            CreatedByTeacherId = sessionReport.CreatedByTeacherId,
            CreatedByTeacherName = "Teacher", // TODO: Get from user service
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt,
            AttendanceCount = attendanceDtos.Count,
            PresentCount = attendanceDtos.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late),
            AbsentCount = attendanceDtos.Count(a => a.Status == AttendanceStatus.Absent || a.Status == AttendanceStatus.Excused),
            Attendance = attendanceDtos
        };

        return Result<TeachingSessionReportDto>.Success(sessionReportDto);
    }
}
