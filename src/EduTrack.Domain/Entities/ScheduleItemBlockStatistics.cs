using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItemBlockStatistics entity - aggregate statistics for student attempts on a specific block
/// Provides fast querying for review pages and progress tracking
/// </summary>
public class ScheduleItemBlockStatistics
{
    public int Id { get; private set; }
    public int ScheduleItemId { get; private set; }
    public ScheduleItemType ScheduleItemType { get; private set; }
    public string BlockId { get; private set; } = string.Empty;
    public string StudentId { get; private set; } = string.Empty;
    public int? StudentProfileId { get; private set; }
    
    // Aggregate statistics
    public int TotalAttempts { get; private set; }
    public int CorrectAttempts { get; private set; }
    public int IncorrectAttempts { get; private set; }
    
    // For tracking repeated mistakes
    public int ConsecutiveIncorrectAttempts { get; private set; }
    public int ConsecutiveCorrectAttempts { get; private set; }
    
    public DateTimeOffset? FirstAttemptAt { get; private set; }
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public DateTimeOffset? LastCorrectAt { get; private set; }
    
    // Metadata
    public string? BlockInstruction { get; private set; }
    public int? BlockOrder { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    
    // Navigation properties
    public ScheduleItem ScheduleItem { get; private set; } = null!;
    public User Student { get; private set; } = null!;
    public StudentProfile? StudentProfile { get; private set; }
    
    // Private constructor for EF Core
    private ScheduleItemBlockStatistics() { }
    
    public static ScheduleItemBlockStatistics Create(
        int scheduleItemId,
        ScheduleItemType scheduleItemType,
        string blockId,
        string studentId,
        int? studentProfileId = null,
        string? blockInstruction = null,
        int? blockOrder = null)
    {
        if (scheduleItemId <= 0)
            throw new ArgumentException("Schedule Item ID must be greater than 0", nameof(scheduleItemId));
        
        if (string.IsNullOrWhiteSpace(blockId))
            throw new ArgumentException("Block ID cannot be null or empty", nameof(blockId));
        
        if (string.IsNullOrWhiteSpace(studentId))
            throw new ArgumentException("Student ID cannot be null or empty", nameof(studentId));

        if (studentProfileId.HasValue && studentProfileId.Value <= 0)
            throw new ArgumentException("Student profile ID must be greater than 0", nameof(studentProfileId));
        
        var now = DateTimeOffset.UtcNow;
        
        return new ScheduleItemBlockStatistics
        {
            ScheduleItemId = scheduleItemId,
            ScheduleItemType = scheduleItemType,
            BlockId = blockId,
            StudentId = studentId,
            StudentProfileId = studentProfileId,
            BlockInstruction = blockInstruction,
            BlockOrder = blockOrder,
            TotalAttempts = 0,
            CorrectAttempts = 0,
            IncorrectAttempts = 0,
            ConsecutiveIncorrectAttempts = 0,
            ConsecutiveCorrectAttempts = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
    
    public void RecordAttempt(bool isCorrect, DateTimeOffset attemptedAt)
    {
        TotalAttempts++;
        
        if (isCorrect)
        {
            CorrectAttempts++;
            ConsecutiveCorrectAttempts++;
            ConsecutiveIncorrectAttempts = 0; // Reset consecutive incorrect
            LastCorrectAt = attemptedAt;
        }
        else
        {
            IncorrectAttempts++;
            ConsecutiveIncorrectAttempts++;
            ConsecutiveCorrectAttempts = 0; // Reset consecutive correct
        }
        
        if (FirstAttemptAt == null)
        {
            FirstAttemptAt = attemptedAt;
        }
        
        LastAttemptAt = attemptedAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public void UpdateMetadata(string? blockInstruction, int? blockOrder)
    {
        BlockInstruction = blockInstruction;
        BlockOrder = blockOrder;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    public decimal SuccessRate => TotalAttempts > 0 
        ? (decimal)CorrectAttempts / TotalAttempts * 100 
        : 0;
    
    public bool HasNeverBeenCorrect => TotalAttempts > 0 && CorrectAttempts == 0;
    
    public bool HasRecentMistakes => ConsecutiveIncorrectAttempts >= 3;
}

