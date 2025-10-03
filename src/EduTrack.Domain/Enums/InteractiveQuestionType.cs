namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of interactive question
/// </summary>
public enum InteractiveQuestionType
{
    /// <summary>
    /// Multiple choice question with single correct answer
    /// </summary>
    MultipleChoice = 1,
    
    /// <summary>
    /// Multiple choice question with multiple correct answers
    /// </summary>
    MultipleSelect = 2,
    
    /// <summary>
    /// Text input question
    /// </summary>
    TextInput = 3,
    
    /// <summary>
    /// Image selection question
    /// </summary>
    ImageSelection = 4,
    
    /// <summary>
    /// True or false question
    /// </summary>
    TrueFalse = 5
}

