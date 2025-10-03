using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Progress entity - represents a student's progress on a lesson or exam
/// </summary>
public class Progress
{
    public int Id { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int? LessonId { get; private set; }
    public int? ExamId { get; private set; }
    public ProgressStatus Status { get; private set; }
    public int CorrectCount { get; private set; }
    public int Streak { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public User Student { get; private set; } = null!;
    public Lesson? Lesson { get; private set; }
    public Exam? Exam { get; private set; }

    // Private constructor for EF Core
    private Progress() { }

    public static Progress Create(string studentId, int? lessonId, int? examId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (!lessonId.HasValue && !examId.HasValue)
            throw new ArgumentException("Either LessonId or ExamId must be provided");
        
        if (lessonId.HasValue && examId.HasValue)
            throw new ArgumentException("Cannot have both LessonId and ExamId");

        return new Progress
        {
            StudentId = studentId,
            LessonId = lessonId,
            ExamId = examId,
            Status = ProgressStatus.NotStarted,
            CorrectCount = 0,
            Streak = 0,
            StartedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Start()
    {
        if (Status != ProgressStatus.NotStarted)
            throw new InvalidOperationException("Progress is already started");

        Status = ProgressStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        if (Status == ProgressStatus.NotStarted)
            throw new InvalidOperationException("Progress must be started before completion");

        Status = ProgressStatus.Done;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Master()
    {
        if (Status == ProgressStatus.NotStarted)
            throw new InvalidOperationException("Progress must be started before mastering");

        Status = ProgressStatus.Mastered;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementCorrectCount()
    {
        CorrectCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DecrementCorrectCount()
    {
        CorrectCount = Math.Max(0, CorrectCount - 3);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementStreak()
    {
        Streak++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetStreak()
    {
        Streak = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(ProgressStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsCompleted => Status == ProgressStatus.Done || Status == ProgressStatus.Mastered;
    public bool IsMastered => Status == ProgressStatus.Mastered;

    public double GetProgressPercentage()
    {
        if (Status == ProgressStatus.NotStarted)
            return 0.0;
        
        if (Status == ProgressStatus.InProgress)
            return 50.0;
        
        if (Status == ProgressStatus.Done)
            return 100.0;
        
        if (Status == ProgressStatus.Mastered)
            return 100.0;

        return 0.0;
    }

    public bool IsForLesson()
    {
        return LessonId.HasValue;
    }

    public bool IsForExam()
    {
        return ExamId.HasValue;
    }
}
