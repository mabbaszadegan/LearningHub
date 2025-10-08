using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// TeachingPlan entity - represents an instructor-guided teaching plan for a course
/// </summary>
public class TeachingPlan
{
    private readonly List<StudentGroup> _groups = new();
    private readonly List<ScheduleItem> _scheduleItems = new();

    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string TeacherId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Objectives { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Course Course { get; private set; } = null!;
    public User Teacher { get; private set; } = null!;
    public IReadOnlyCollection<StudentGroup> Groups => _groups.AsReadOnly();
    public IReadOnlyCollection<ScheduleItem> ScheduleItems => _scheduleItems.AsReadOnly();

    // Private constructor for EF Core
    private TeachingPlan() { }

    public static TeachingPlan Create(int courseId, string teacherId, string title, string? description = null, string? objectives = null)
    {
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));
        
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new ArgumentException("Teacher ID cannot be null or empty", nameof(teacherId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        return new TeachingPlan
        {
            CourseId = courseId,
            TeacherId = teacherId,
            Title = title,
            Description = description,
            Objectives = objectives,
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

    public void UpdateObjectives(string? objectives)
    {
        Objectives = objectives;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddGroup(StudentGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        if (_groups.Any(g => g.Id == group.Id))
            throw new InvalidOperationException("Group already exists in this teaching plan");

        _groups.Add(group);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveGroup(StudentGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        var groupToRemove = _groups.FirstOrDefault(g => g.Id == group.Id);
        if (groupToRemove != null)
        {
            _groups.Remove(groupToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddScheduleItem(ScheduleItem scheduleItem)
    {
        if (scheduleItem == null)
            throw new ArgumentNullException(nameof(scheduleItem));

        if (_scheduleItems.Any(s => s.Id == scheduleItem.Id))
            throw new InvalidOperationException("Schedule item already exists in this teaching plan");

        _scheduleItems.Add(scheduleItem);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveScheduleItem(ScheduleItem scheduleItem)
    {
        if (scheduleItem == null)
            throw new ArgumentNullException(nameof(scheduleItem));

        var itemToRemove = _scheduleItems.FirstOrDefault(s => s.Id == scheduleItem.Id);
        if (itemToRemove != null)
        {
            _scheduleItems.Remove(itemToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalGroups() => _groups.Count;
    public int GetTotalScheduleItems() => _scheduleItems.Count;
    public int GetTotalStudents() => _groups.Sum(g => g.GetTotalMembers());
}