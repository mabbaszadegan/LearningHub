namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the status of a student's submission
/// </summary>
public enum SubmissionStatus
{
    /// <summary>
    /// Student has not started the assignment
    /// </summary>
    NotStarted = 0,
    
    /// <summary>
    /// Student is currently working on the assignment
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Student has submitted the assignment
    /// </summary>
    Submitted = 2,
    
    /// <summary>
    /// Teacher has reviewed the submission
    /// </summary>
    Reviewed = 3,
    
    /// <summary>
    /// Teacher has graded the submission
    /// </summary>
    Graded = 4
}
