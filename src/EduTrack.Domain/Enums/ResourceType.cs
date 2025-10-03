namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of a resource in a lesson
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// PDF document
    /// </summary>
    PDF = 1,
    
    /// <summary>
    /// Video file
    /// </summary>
    Video = 2,
    
    /// <summary>
    /// Image file
    /// </summary>
    Image = 3,
    
    /// <summary>
    /// External URL link
    /// </summary>
    URL = 4,
    
    /// <summary>
    /// General document file
    /// </summary>
    Document = 5
}
