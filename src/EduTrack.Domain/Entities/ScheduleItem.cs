using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// ScheduleItem entity - represents a scheduled assignment or activity in a teaching plan
/// </summary>
public class ScheduleItem
{
    public int Id { get; private set; }
    public int TeachingPlanId { get; private set; }
    public int? GroupId { get; private set; }
    public int? LessonId { get; private set; }
    public ScheduleItemType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? DueDate { get; private set; }
    public bool IsMandatory { get; private set; }
    public DisciplineType? DisciplineHint { get; private set; }
    public string ContentJson { get; private set; } = string.Empty;
    public decimal? MaxScore { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public TeachingPlan TeachingPlan { get; private set; } = null!;
    public StudentGroup? Group { get; private set; }
    public Lesson? Lesson { get; private set; }

    // Private constructor for EF Core
    private ScheduleItem() { }

    public static ScheduleItem Create(int teachingPlanId, ScheduleItemType type, string title, 
        string? description, DateTimeOffset startDate, DateTimeOffset? dueDate, bool isMandatory,
        string contentJson, decimal? maxScore = null, int? groupId = null, int? lessonId = null,
        DisciplineType? disciplineHint = null)
    {
        if (teachingPlanId <= 0)
            throw new ArgumentException("Teaching Plan ID must be greater than 0", nameof(teachingPlanId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("Content JSON cannot be null or empty", nameof(contentJson));
        
        if (maxScore.HasValue && maxScore.Value < 0)
            throw new ArgumentException("Max score cannot be negative", nameof(maxScore));

        return new ScheduleItem
        {
            TeachingPlanId = teachingPlanId,
            GroupId = groupId,
            LessonId = lessonId,
            Type = type,
            Title = title,
            Description = description,
            StartDate = startDate,
            DueDate = dueDate,
            IsMandatory = isMandatory,
            DisciplineHint = disciplineHint,
            ContentJson = contentJson,
            MaxScore = maxScore,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDates(DateTimeOffset startDate, DateTimeOffset? dueDate)
    {
        StartDate = startDate;
        DueDate = dueDate;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMandatory(bool isMandatory)
    {
        IsMandatory = isMandatory;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateContent(string contentJson)
    {
        if (string.IsNullOrWhiteSpace(contentJson))
            throw new ArgumentException("Content JSON cannot be null or empty", nameof(contentJson));

        ContentJson = contentJson;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMaxScore(decimal? maxScore)
    {
        if (maxScore.HasValue && maxScore.Value < 0)
            throw new ArgumentException("Max score cannot be negative", nameof(maxScore));

        MaxScore = maxScore;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsOverdue()
    {
        return DueDate.HasValue && DateTimeOffset.UtcNow > DueDate.Value;
    }

    public bool IsUpcoming()
    {
        return DateTimeOffset.UtcNow < StartDate;
    }

    public bool IsActive()
    {
        return DateTimeOffset.UtcNow >= StartDate && (!DueDate.HasValue || DateTimeOffset.UtcNow <= DueDate.Value);
    }

    public TimeSpan? GetTimeUntilDue()
    {
        if (!DueDate.HasValue) return null;
        return DueDate.Value - DateTimeOffset.UtcNow;
    }
}