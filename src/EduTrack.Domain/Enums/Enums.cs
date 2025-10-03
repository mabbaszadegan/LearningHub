namespace EduTrack.Domain.Enums;

public enum QuestionType
{
    MultipleChoice = 1,
    TrueFalse = 2,
    ShortAnswer = 3
}

public enum ProgressStatus
{
    NotStarted = 0,
    InProgress = 1,
    Done = 2,
    Mastered = 3
}

public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Student = 3
}

public enum ResourceType
{
    PDF = 1,
    Video = 2,
    Image = 3,
    URL = 4,
    Document = 5
}

public enum EducationalContentType
{
    Text = 1,
    Image = 2,
    Video = 3,
    Audio = 4,
    PDF = 5,
    ExternalUrl = 6,
    File = 7
}