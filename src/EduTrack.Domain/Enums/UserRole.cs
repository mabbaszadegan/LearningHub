namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the role of a user in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// System administrator with full access
    /// </summary>
    Admin = 0,
    
    /// <summary>
    /// Teacher who can create courses and manage classes
    /// </summary>
    Teacher = 1,
    
    /// <summary>
    /// Student who can enroll in courses and take exams
    /// </summary>
    Student = 2
}
