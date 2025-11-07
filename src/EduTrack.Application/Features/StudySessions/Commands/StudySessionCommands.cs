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
    public int? StudentProfileId { get; set; }

    public StartStudySessionCommand(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Command to create and complete a study session in one operation
/// </summary>
public class CreateAndCompleteStudySessionCommand : IRequest<Result<StudySessionDto>>
{
    public string StudentId { get; set; } = string.Empty;
    public int ScheduleItemId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset EndedAt { get; set; }
    public int? StudentProfileId { get; set; }

    public CreateAndCompleteStudySessionCommand(string studentId, int scheduleItemId, DateTimeOffset startedAt, DateTimeOffset endedAt, int? studentProfileId = null)
    {
        StudentId = studentId;
        ScheduleItemId = scheduleItemId;
        StartedAt = startedAt;
        EndedAt = endedAt;
        StudentProfileId = studentProfileId;
    }
}

/// <summary>
/// Command to delete a study session (for exit without saving)
/// </summary>
public class DeleteStudySessionCommand : IRequest<Result<bool>>
{
    public int StudySessionId { get; set; }

    public DeleteStudySessionCommand(int studySessionId)
    {
        StudySessionId = studySessionId;
    }
}

