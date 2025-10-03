namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of interactive lesson stage
/// </summary>
public enum InteractiveLessonStageType
{
    /// <summary>
    /// Text-only educational stage
    /// </summary>
    TextOnly = 1,
    
    /// <summary>
    /// Stage that uses content from sub-chapters
    /// </summary>
    SubChapterContent = 2,
    
    /// <summary>
    /// Mixed stage with both text and sub-chapter content
    /// </summary>
    Mixed = 3,
    
    /// <summary>
    /// Question-based stage
    /// </summary>
    Question = 4
}
