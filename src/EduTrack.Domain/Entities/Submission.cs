using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Submission entity - represents a student's submission for a schedule item
/// </summary>
public class Submission
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public string StudentId { get; private set; } = string.Empty;
    public int? StudentProfileId { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public SubmissionStatus Status { get; private set; } = SubmissionStatus.NotStarted;
    public decimal? Grade { get; private set; }
    public string? FeedbackText { get; private set; }
    public string? TeacherId { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public string? AttachmentsJson { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    public User? Teacher { get; private set; }
    public StudentProfile? StudentProfile { get; private set; }

    // Private constructor for EF Core
    private Submission() { }

    public static Submission Create(int scheduleItemId, string studentId, string payloadJson, 
        string? attachmentsJson = null, int? studentProfileId = null)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Payload JSON cannot be null or empty", nameof(payloadJson));

        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        return new Submission
        {
            ScheduleItemId = scheduleItemId,
            StudentId = studentId,
            StudentProfileId = studentProfileId,
            PayloadJson = payloadJson,
            AttachmentsJson = attachmentsJson,
            Status = SubmissionStatus.NotStarted,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void AssignToStudentProfile(int? studentProfileId)
    {
        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));

        StudentProfileId = studentProfileId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        if (Status != SubmissionStatus.NotStarted)
            throw new InvalidOperationException("Can only start a submission that hasn't been started");

        Status = SubmissionStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Submit()
    {
        if (Status == SubmissionStatus.NotStarted)
            throw new InvalidOperationException("Must start submission before submitting");

        Status = SubmissionStatus.Submitted;
        SubmittedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Review(string? teacherId = null)
    {
        if (Status != SubmissionStatus.Submitted)
            throw new InvalidOperationException("Can only review submitted submissions");

        Status = SubmissionStatus.Reviewed;
        TeacherId = teacherId;
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetGrade(decimal grade, string? feedbackText = null, string? teacherId = null)
    {
        if (Status != SubmissionStatus.Submitted && Status != SubmissionStatus.Reviewed)
            throw new InvalidOperationException("Can only grade submitted or reviewed submissions");

        if (grade < 0)
            throw new ArgumentException("Grade cannot be negative", nameof(grade));

        Status = SubmissionStatus.Graded;
        Grade = grade;
        FeedbackText = feedbackText;
        TeacherId = teacherId;
        ReviewedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePayload(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Payload JSON cannot be null or empty", nameof(payloadJson));

        if (Status == SubmissionStatus.Graded)
            throw new InvalidOperationException("Cannot update payload of graded submission");

        PayloadJson = payloadJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAttachments(string? attachmentsJson)
    {
        if (Status == SubmissionStatus.Graded)
            throw new InvalidOperationException("Cannot update attachments of graded submission");

        AttachmentsJson = attachmentsJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsCompleted => Status == SubmissionStatus.Graded;
    public bool IsSubmitted => Status == SubmissionStatus.Submitted || Status == SubmissionStatus.Reviewed || Status == SubmissionStatus.Graded;
    public bool IsGraded => Status == SubmissionStatus.Graded;

    public decimal? GetPercentageScore()
    {
        if (!Grade.HasValue || !ScheduleItem.MaxScore.HasValue || ScheduleItem.MaxScore.Value == 0)
            return null;

        return (Grade.Value / ScheduleItem.MaxScore.Value) * 100;
    }

    public bool IsPassing(decimal passThreshold = 75m)
    {
        var percentage = GetPercentageScore();
        return percentage.HasValue && percentage.Value >= passThreshold;
    }
}