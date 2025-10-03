using EduTrack.Domain.Enums;
using EduTrack.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Domain.Entities;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Profile? Profile { get; set; }
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public class Profile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}

public class Module
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

public class Lesson
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Module Module { get; set; } = null!;
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public ICollection<Progress> Progresses { get; set; } = new List<Progress>();
}

public class Resource
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Lesson Lesson { get; set; } = null!;
}

public class Class
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public DateTimeOffset EnrolledAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Class Class { get; set; } = null!;
    public User Student { get; set; } = null!;
}

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class Choice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }

    // Navigation properties
    public Question Question { get; set; } = null!;
}

public class Exam
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; } = 75;
    public bool ShowSolutions { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}

public class ExamQuestion
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public int Order { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public Question Question { get; set; } = null!;
}

public class Attempt
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }
    public bool IsPassed { get; set; }
    public TimeSpan? Duration { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public User Student { get; set; } = null!;
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}

public class Answer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public string? TextAnswer { get; set; }
    public int? SelectedChoiceId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTimeOffset AnsweredAt { get; set; }

    // Navigation properties
    public Attempt Attempt { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public Choice? SelectedChoice { get; set; }
}

public class Progress
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public int? ExamId { get; set; }
    public ProgressStatus Status { get; set; }
    public int CorrectCount { get; set; }
    public int Streak { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public User Student { get; set; } = null!;
    public Lesson? Lesson { get; set; }
    public Exam? Exam { get; set; }
}

public class Chapter
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Course Course { get; set; } = null!;
    public ICollection<SubChapter> SubChapters { get; set; } = new List<SubChapter>();
}

public class SubChapter
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Chapter Chapter { get; set; } = null!;
    public ICollection<EducationalContent> EducationalContents { get; set; } = new List<EducationalContent>();
}

public class File
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MD5Hash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int ReferenceCount { get; set; } = 1;

    // Navigation properties
    public ICollection<EducationalContent> EducationalContents { get; set; } = new List<EducationalContent>();
}

public class EducationalContent
{
    public int Id { get; set; }
    public int SubChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EducationalContentType Type { get; set; }
    public string? TextContent { get; set; }
    public int? FileId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public SubChapter SubChapter { get; set; } = null!;
    public File? File { get; set; }
}

public class ActivityLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
