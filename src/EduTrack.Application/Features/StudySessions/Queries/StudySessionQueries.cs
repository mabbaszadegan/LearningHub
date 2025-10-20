using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using MediatR;

namespace EduTrack.Application.Features.StudySessions.Queries;

/// <summary>
/// Query to get study session by ID
/// </summary>
public class GetStudySessionByIdQuery : IRequest<Result<StudySessionDto>>
{
    public int Id { get; set; }

    public GetStudySessionByIdQuery(int id)
    {
        Id = id;
    }
}

/// <summary>
/// Query to get active study session for student and schedule item
/// </summary>
public class GetActiveStudySessionQuery : IRequest<Result<StudySessionDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }

    public GetActiveStudySessionQuery(string studentId, int scheduleItemId)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
    }
}

/// <summary>
/// Query to get study sessions for student and schedule item
/// </summary>
public class GetStudySessionsByStudentAndScheduleItemQuery : IRequest<Result<IEnumerable<StudySessionDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }

    public GetStudySessionsByStudentAndScheduleItemQuery(string studentId, int scheduleItemId)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
    }
}

/// <summary>
/// Query to get study session statistics for student and schedule item
/// </summary>
public class GetStudySessionStatisticsQuery : IRequest<Result<StudySessionStatisticsDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }

    public GetStudySessionStatisticsQuery(string studentId, int scheduleItemId)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
    }
}

/// <summary>
/// Query to get schedule item with study statistics
/// </summary>
public class GetScheduleItemWithStudyStatsQuery : IRequest<Result<ScheduleItemWithStudyStatsDto>>
{
    public int ScheduleItemId { get; set; }
    public string StudentId { get; set; } = string.Empty;

    public GetScheduleItemWithStudyStatsQuery(int scheduleItemId, string studentId)
    {
        ScheduleItemId = scheduleItemId;
        StudentId = studentId;
    }
}