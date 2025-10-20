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
    public int ScheduleItemId { get; set; }

    public StartStudySessionCommand(string studentId, int scheduleItemId)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
    }
}

/// <summary>
/// Command to complete a study session
/// </summary>
public class CompleteStudySessionCommand : IRequest<Result<StudySessionDto>>
{
    public int StudySessionId { get; set; }

    public CompleteStudySessionCommand(int studySessionId)
    {
        StudySessionId = studySessionId;
    }
}

