namespace EduTrack.Domain.Enums;

/// <summary>
/// Represents the type of a question in an exam
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Multiple choice question with single correct answer
    /// </summary>
    MultipleChoice = 0,
    
    /// <summary>
    /// True or false question
    /// </summary>
    TrueFalse = 1,
    
    /// <summary>
    /// Short answer question requiring text input
    /// </summary>
    ShortAnswer = 2,
    
    /// <summary>
    /// Multiple choice question with multiple correct answers
    /// </summary>
    MultipleSelect = 3
}
