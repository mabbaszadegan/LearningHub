namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the progress status of a student for a lesson or exam
/// </summary>
public enum ProgressStatus
{
    /// <summary>
    /// Student has not started the lesson/exam
    /// </summary>
    NotStarted = 0,
    
    /// <summary>
    /// Student is currently working on the lesson/exam
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Student has completed the lesson/exam
    /// </summary>
    Done = 2,
    
    /// <summary>
    /// Student has mastered the lesson/exam (high performance)
    /// </summary>
    Mastered = 3
}
