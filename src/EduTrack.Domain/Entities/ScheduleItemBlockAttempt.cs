using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemBlockAttempt entity - represents a student's attempt to answer a block/question within a schedule item
/// Supports all schedule item types (Ordering, MultipleChoice, GapFill, Match, etc.)
/// </summary>
public class ScheduleItemBlockAttempt
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public ScheduleItemType ScheduleItemType { get; private set; }
    public string BlockId { get; private set; } = string.Empty; // Generic: can be Id, Index, or "main"
    public string StudentId { get; private set; } = string.Empty;
    
    // Student's submitted answer (JSON - supports all types of answer structures)
    public string SubmittedAnswerJson { get; private set; } = string.Empty;
    
    // Correct answer (for comparison and display)
    public string CorrectAnswerJson { get; private set; } = string.Empty;
    
    public bool IsCorrect { get; private set; }
    public decimal PointsEarned { get; private set; }
    public decimal MaxPoints { get; private set; }
    
    // Metadata for better display
    public string? BlockInstruction { get; private set; } // Block/question instruction
    public int? BlockOrder { get; private set; } // Order of block in ScheduleItem
    
    public DateTimeOffset AttemptedAt { get; private set; }
    
    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    
    // Private constructor for EF Core
    private ScheduleItemBlockAttempt() { }
    
    public static ScheduleItemBlockAttempt Create(
        int scheduleItemId,
        ScheduleItemType scheduleItemType,
        string blockId,
        string studentId,
        string submittedAnswerJson,
        string correctAnswerJson,
        bool isCorrect,
        decimal pointsEarned,
        decimal maxPoints,
        string? blockInstruction = null,
        int? blockOrder = null)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (string.IsNullOrWhiteSpace(blockId))
            throw new ArgumentException("Block ID cannot be null or empty", nameof(blockId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));
        
        if (string.IsNullOrWhiteSpace(submittedAnswerJson))
            throw new ArgumentException("Submitted answer JSON cannot be null or empty", nameof(submittedAnswerJson));
        
        if (pointsEarned < 0)
            throw new ArgumentException("Points earned cannot be negative", nameof(pointsEarned));
        
        if (maxPoints < 0)
            throw new ArgumentException("Max points cannot be negative", nameof(maxPoints));
        
        if (pointsEarned > maxPoints)
            throw new ArgumentException("Points earned cannot exceed max points", nameof(pointsEarned));
        
        return new ScheduleItemBlockAttempt
        {
            ScheduleItemId = scheduleItemId,
            ScheduleItemType = scheduleItemType,
            BlockId = blockId,
            StudentId = studentId,
            SubmittedAnswerJson = submittedAnswerJson,
            CorrectAnswerJson = correctAnswerJson,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            MaxPoints = maxPoints,
            BlockInstruction = blockInstruction,
            BlockOrder = blockOrder,
            AttemptedAt = DateTimeOffset.UtcNow
        };
    }
    
    public void UpdateResult(bool isCorrect, decimal pointsEarned)
    {
        if (pointsEarned < 0)
            throw new ArgumentException("Points earned cannot be negative", nameof(pointsEarned));
        
        if (pointsEarned > MaxPoints)
            throw new ArgumentException("Points earned cannot exceed max points", nameof(pointsEarned));
        
        IsCorrect = isCorrect;
        PointsEarned = pointsEarned;
    }
}

