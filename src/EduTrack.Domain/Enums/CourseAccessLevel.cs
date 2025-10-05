namespace EduTrack.Domain.Enums;

/// <summary>
/// CourseAccessLevel enum - defines the level of access a student has to course content
/// </summary>
public enum CourseAccessLevel
{
    /// <summary>
    /// No access to course content
    /// </summary>
    None = 0,

    /// <summary>
    /// Can view course information and description only
    /// </summary>
    ViewOnly = 1,

    /// <summary>
    /// Can access lessons and educational content
    /// </summary>
    Lessons = 2,

    /// <summary>
    /// Can access lessons and take exams
    /// </summary>
    Exams = 3,

    /// <summary>
    /// Can access lessons, exams, and download resources
    /// </summary>
    Resources = 4,

    /// <summary>
    /// Full access to all course content and features
    /// </summary>
    Full = 5
}
