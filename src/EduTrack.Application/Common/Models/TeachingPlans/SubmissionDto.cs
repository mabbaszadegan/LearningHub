using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Models.TeachingPlans;

public class SubmissionDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public string ScheduleItemTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public decimal? Grade { get; set; }
    public string? FeedbackText { get; set; }
    public string? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public decimal? PercentageScore { get; set; }
    public bool IsPassing { get; set; }
    public bool IsOverdue { get; set; }
}
