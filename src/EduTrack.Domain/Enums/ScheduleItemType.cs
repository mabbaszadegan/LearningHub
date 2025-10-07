namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of schedule item in a teaching plan
/// </summary>
public enum ScheduleItemType
{
    /// <summary>
    /// Textual reminder or learning note
    /// </summary>
    Reminder = 0,
    
    /// <summary>
    /// Writing exercise with free text and word limit
    /// </summary>
    Writing = 1,
    
    /// <summary>
    /// Audio exercise (listen/record & respond)
    /// </summary>
    Audio = 2,
    
    /// <summary>
    /// Gap fill (cloze) exercise
    /// </summary>
    GapFill = 3,
    
    /// <summary>
    /// Multiple choice question (single or multi-select)
    /// </summary>
    MultipleChoice = 4,
    
    /// <summary>
    /// Matching exercise (pair items)
    /// </summary>
    Match = 5,
    
    /// <summary>
    /// Error finding and correction exercise
    /// </summary>
    ErrorFinding = 6,
    
    /// <summary>
    /// Code exercise for programming courses
    /// </summary>
    CodeExercise = 7,
    
    /// <summary>
    /// Quiz with multiple question types
    /// </summary>
    Quiz = 8
}
