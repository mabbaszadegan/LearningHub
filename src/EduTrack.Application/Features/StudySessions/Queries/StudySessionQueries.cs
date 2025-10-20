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
/// Query to get active study session for student and content
/// </summary>
public class GetActiveStudySessionQuery : IRequest<Result<StudySessionDto?>>
{
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }

    public GetActiveStudySessionQuery(string studentId, int educationalContentId)
    {
        StudentId = studentId;
        EducationalContentId = educationalContentId;
    }
}

/// <summary>
/// Query to get study sessions for student and content
/// </summary>
public class GetStudySessionsByStudentAndContentQuery : IRequest<Result<IEnumerable<StudySessionDto>>>
{
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }

    public GetStudySessionsByStudentAndContentQuery(string studentId, int educationalContentId)
    {
        StudentId = studentId;
        EducationalContentId = educationalContentId;
    }
}

/// <summary>
/// Query to get study session statistics for student and content
/// </summary>
public class GetStudySessionStatisticsQuery : IRequest<Result<StudySessionStatisticsDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }

    public GetStudySessionStatisticsQuery(string studentId, int educationalContentId)
    {
        StudentId = studentId;
        EducationalContentId = educationalContentId;
    }
}

/// <summary>
/// Query to get educational content with study statistics
/// </summary>
public class GetEducationalContentWithStudyStatsQuery : IRequest<Result<EducationalContentWithStudyStatsDto>>
{
    public int EducationalContentId { get; set; }
    public string StudentId { get; set; } = string.Empty;

    public GetEducationalContentWithStudyStatsQuery(int educationalContentId, string studentId)
    {
        EducationalContentId = educationalContentId;
        StudentId = studentId;
    }
}
