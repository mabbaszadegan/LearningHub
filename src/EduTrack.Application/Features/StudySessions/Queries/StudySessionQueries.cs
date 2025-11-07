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
    public int? StudentProfileId { get; set; }

    public GetActiveStudySessionQuery(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Query to get study sessions for student and schedule item
/// </summary>
public class GetStudySessionsByStudentAndScheduleItemQuery : IRequest<Result<IEnumerable<StudySessionDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public int? StudentProfileId { get; set; }

    public GetStudySessionsByStudentAndScheduleItemQuery(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Query to get study session statistics for student and schedule item
/// </summary>
public class GetStudySessionStatisticsQuery : IRequest<Result<StudySessionStatisticsDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public int? StudentProfileId { get; set; }

    public GetStudySessionStatisticsQuery(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Query to get the last study sessions for a student
/// </summary>
public class GetLastStudySessionsQuery : IRequest<Result<List<StudySessionHistoryDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
    public int? StudentProfileId { get; set; }

    public GetLastStudySessionsQuery(string studentId, int count = 5, int? studentProfileId = null)
    {
        StudentId = studentId;
        Count = count;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Query to get all study sessions for a student (for statistics)
/// </summary>
public class GetAllStudySessionsQuery : IRequest<Result<List<StudySessionHistoryDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int? StudentProfileId { get; set; }

    public GetAllStudySessionsQuery(string studentId, int? studentProfileId = null)
    {
        StudentId = studentId;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Query to get the last courses with study activity for a student
/// </summary>
public class GetLastStudyCoursesQuery : IRequest<Result<List<CourseStudyHistoryDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int Count { get; set; } = 5;
    public int? StudentProfileId { get; set; }

    public GetLastStudyCoursesQuery(string studentId, int count = 5, int? studentProfileId = null)
    {
        StudentId = studentId;
        Count = count;
        StudentProfileId = studentProfileId;
    }
}