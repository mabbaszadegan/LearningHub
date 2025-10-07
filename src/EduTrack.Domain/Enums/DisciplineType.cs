namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the discipline type of a course
/// </summary>
public enum DisciplineType
{
    /// <summary>
    /// Language courses (English, Spanish, etc.)
    /// </summary>
    Language = 0,
    
    /// <summary>
    /// Mathematics courses
    /// </summary>
    Math = 1,
    
    /// <summary>
    /// Programming and computer science courses
    /// </summary>
    Programming = 2,
    
    /// <summary>
    /// Science courses (Physics, Chemistry, Biology, etc.)
    /// </summary>
    Science = 3,
    
    /// <summary>
    /// Other disciplines not covered by specific types
    /// </summary>
    Other = 4
}
