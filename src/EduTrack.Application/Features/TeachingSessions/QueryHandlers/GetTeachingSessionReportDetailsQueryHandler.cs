using System.Collections.Generic;
using System.Linq;
using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Extensions;
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
    private readonly IScheduleItemRepository _scheduleItemRepository;

    public GetTeachingSessionReportDetailsQueryHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ICurrentUserService currentUserService,
        IStudentGroupRepository studentGroupRepository,
        IScheduleItemRepository scheduleItemRepository)
    {
        _sessionReportRepository = sessionReportRepository;
        _currentUserService = currentUserService;
        _studentGroupRepository = studentGroupRepository;
        _scheduleItemRepository = scheduleItemRepository;
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
        var groups = (await _studentGroupRepository
                .GetGroupsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken))
            .ToList();
        var groupLookup = groups.ToDictionary(g => g.Id, g => g.Name);
        var allStudents = groups.SelectMany(g => g.Members).ToList();
        
        // Create attendance DTOs with student information
        var attendanceDtos = allStudents.Select(member => 
        {
            var existingAttendance = sessionReport.Attendance.FirstOrDefault(a => a.StudentId == member.StudentId);
            var profile = member.StudentProfile;
            var user = profile?.User;
            var displayName = profile?.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = $"{user?.FirstName} {user?.LastName}".Trim();
            }
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = user?.UserName ?? "Unknown Student";
            }

            return new TeachingSessionAttendanceDto
            {
                Id = existingAttendance?.Id ?? 0,
                TeachingSessionReportId = sessionReport.Id,
                StudentId = member.StudentId,
                StudentName = displayName,
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
        
        // Generate stats from related tables
        var attendanceStats = new
        {
            totalStudents = sessionReport.Attendance.Count,
            presentCount = sessionReport.Attendance.Count(a => a.Status == AttendanceStatus.Present),
            absentCount = sessionReport.Attendance.Count(a => a.Status == AttendanceStatus.Absent),
            participationAverage = sessionReport.Attendance.Where(a => a.ParticipationScore.HasValue).Any() 
                ? sessionReport.Attendance.Where(a => a.ParticipationScore.HasValue).Average(a => a.ParticipationScore!.Value) 
                : 0
        };

        var coverageStats = new
        {
            totalTopics = sessionReport.TopicCoverages.Count,
            coveredTopics = sessionReport.TopicCoverages.Count(tc => tc.WasCovered),
            averageCoveragePercentage = sessionReport.TopicCoverages.Any() 
                ? sessionReport.TopicCoverages.Average(tc => tc.CoveragePercentage) 
                : 0
        };

        var executionStats = new
        {
            totalGroups = sessionReport.Executions.Count,
            averageUnderstandingLevel = sessionReport.Executions.Any() 
                ? sessionReport.Executions.Average(e => e.UnderstandingLevel) 
                : 0,
            averageParticipationLevel = sessionReport.Executions.Any() 
                ? sessionReport.Executions.Average(e => e.ParticipationLevel) 
                : 0,
            averageTeacherSatisfaction = sessionReport.Executions.Any() 
                ? sessionReport.Executions.Average(e => e.TeacherSatisfaction) 
                : 0
        };

        var combinedStats = new
        {
            attendance = attendanceStats,
            coverage = coverageStats,
            execution = executionStats
        };

        statsJson = JsonConvert.SerializeObject(combinedStats, jsonSettings);

        var scheduleItems = await _scheduleItemRepository
            .GetScheduleItemsByTeachingPlanAsync(sessionReport.TeachingPlanId, cancellationToken);
        var assignments = scheduleItems
            .Where(item => item.SessionReportId == sessionReport.Id)
            .Select(item => CreateAssignmentDto(item, groupLookup))
            .OrderByDescending(a => a.StartDate)
            .ToList();

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
            Attendance = attendanceDtos,
            Assignments = assignments
        };

        return Result<TeachingSessionReportDto>.Success(sessionReportDto);
    }

    private static TeachingSessionAssignmentDto CreateAssignmentDto(
        ScheduleItem item,
        IReadOnlyDictionary<int, string> groupLookup)
    {
        var now = DateTimeOffset.UtcNow;
        var status = GetStatus(item);

        var groupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (item.GroupId.HasValue && groupLookup.TryGetValue(item.GroupId.Value, out var primaryGroupName))
        {
            groupNames.Add(primaryGroupName);
        }

        foreach (var groupAssignment in item.GroupAssignments)
        {
            if (!string.IsNullOrWhiteSpace(groupAssignment.StudentGroup?.Name))
            {
                groupNames.Add(groupAssignment.StudentGroup.Name);
            }
        }

        var studentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var studentAssignment in item.StudentAssignments)
        {
            var displayName = GetStudentDisplayName(studentAssignment);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                studentNames.Add(displayName);
            }
        }

        var subChapterTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var subChapterAssignment in item.SubChapterAssignments)
        {
            var title = subChapterAssignment.SubChapter?.Title;
            if (!string.IsNullOrWhiteSpace(title))
            {
                subChapterTitles.Add(title);
            }
        }

        return new TeachingSessionAssignmentDto
        {
            ScheduleItemId = item.Id,
            TeachingPlanId = item.TeachingPlanId,
            CourseId = item.CourseId ?? item.TeachingPlan?.CourseId,
            Title = item.Title,
            Type = item.Type,
            TypeName = item.Type.GetDisplayName(),
            Status = status,
            StatusText = GetStatusText(status),
            StartDate = item.StartDate,
            DueDate = item.DueDate,
            IsMandatory = item.IsMandatory,
            MaxScore = item.MaxScore,
            CurrentStep = item.CurrentStep,
            IsCompleted = item.IsCompleted,
            IsOverdue = item.DueDate.HasValue && item.DueDate.Value < now && !item.IsCompleted,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt,
            AssignedGroups = groupNames.OrderBy(name => name).ToList(),
            AssignedStudents = studentNames.OrderBy(name => name).ToList(),
            AssignedSubChapters = subChapterTitles.OrderBy(title => title).ToList()
        };
    }

    private static string GetStudentDisplayName(ScheduleItemStudentAssignment assignment)
    {
        var profile = assignment.StudentProfile;
        if (profile == null)
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            return profile.DisplayName;
        }

        var user = profile.User;
        if (user == null)
        {
            return string.Empty;
        }

        var parts = new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .ToArray();

        if (parts.Length > 0)
        {
            return string.Join(' ', parts);
        }

        return user.UserName ?? string.Empty;
    }

    private static ScheduleItemStatus GetStatus(ScheduleItem item)
    {
        var now = DateTimeOffset.UtcNow;

        if (item.IsCompleted)
        {
            return ScheduleItemStatus.Completed;
        }

        if (item.DueDate.HasValue && now > item.DueDate.Value)
        {
            return ScheduleItemStatus.Expired;
        }

        if (now >= item.StartDate && (!item.DueDate.HasValue || now <= item.DueDate.Value))
        {
            return ScheduleItemStatus.Active;
        }

        if (now < item.StartDate)
        {
            return ScheduleItemStatus.Published;
        }

        return ScheduleItemStatus.Draft;
    }

    private static string GetStatusText(ScheduleItemStatus status)
    {
        return status switch
        {
            ScheduleItemStatus.Draft => "پیش‌نویس",
            ScheduleItemStatus.Published => "منتشر شده",
            ScheduleItemStatus.Active => "فعال",
            ScheduleItemStatus.Completed => "تکمیل شده",
            ScheduleItemStatus.Expired => "منقضی شده",
            _ => "نامشخص"
        };
    }
}
