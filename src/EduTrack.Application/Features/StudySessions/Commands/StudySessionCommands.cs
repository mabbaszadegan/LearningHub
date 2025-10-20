using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using MediatR;

namespace EduTrack.Application.Features.StudySessions.Commands;

/// <summary>
/// Command to start a new study session
/// </summary>
public class StartStudySessionCommand : IRequest<Result<StudySessionDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int EducationalContentId { get; set; }

    public StartStudySessionCommand(string studentId, int educationalContentId)
    {
        StudentId = studentId;
        EducationalContentId = educationalContentId;
    }
}

/// <summary>
/// Command to complete a study session
/// </summary>
public class CompleteStudySessionCommand : IRequest<Result<StudySessionDto>>
{
    public int StudySessionId { get; set; }
    public int DurationSeconds { get; set; }

    public CompleteStudySessionCommand(int studySessionId, int durationSeconds)
    {
        StudySessionId = studySessionId;
        DurationSeconds = durationSeconds;
    }
}

/// <summary>
/// Command to update study session duration
/// </summary>
public class UpdateStudySessionDurationCommand : IRequest<Result<StudySessionDto>>
{
    public int StudySessionId { get; set; }
    public int DurationSeconds { get; set; }

    public UpdateStudySessionDurationCommand(int studySessionId, int durationSeconds)
    {
        StudySessionId = studySessionId;
        DurationSeconds = durationSeconds;
    }
}
