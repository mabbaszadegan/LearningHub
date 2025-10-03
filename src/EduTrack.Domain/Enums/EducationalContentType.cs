namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of educational content
/// </summary>
public enum EducationalContentType
{
    /// <summary>
    /// Text content
    /// </summary>
    Text = 1,
    
    /// <summary>
    /// Image content
    /// </summary>
    Image = 2,
    
    /// <summary>
    /// Video content
    /// </summary>
    Video = 3,
    
    /// <summary>
    /// Audio content
    /// </summary>
    Audio = 4,
    
    /// <summary>
    /// PDF document
    /// </summary>
    PDF = 5,
    
    /// <summary>
    /// External URL content
    /// </summary>
    ExternalUrl = 6,
    
    /// <summary>
    /// General file content
    /// </summary>
    File = 7
}
