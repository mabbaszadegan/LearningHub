namespace EduTrack.Domain.Entities;

/// <summary>
/// Module entity - represents a section within a course
/// </summary>
public class Module
{
    private readonly List<Lesson> _lessons = new();

    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Course Course { get; private set; } = null!;
    public IReadOnlyCollection<Lesson> Lessons => _lessons.AsReadOnly();

    // Private constructor for EF Core
    private Module() { }

    public static Module Create(int courseId, string title, string? description, int order)
    {
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Module
        {
            CourseId = courseId,
            Title = title,
            Description = description,
            Order = order,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsActive = true
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

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddLesson(Lesson lesson)
    {
        if (lesson == null)
            throw new ArgumentNullException(nameof(lesson));

        if (_lessons.Any(l => l.Id == lesson.Id))
            throw new InvalidOperationException("Lesson already exists in this module");

        _lessons.Add(lesson);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveLesson(Lesson lesson)
    {
        if (lesson == null)
            throw new ArgumentNullException(nameof(lesson));

        var lessonToRemove = _lessons.FirstOrDefault(l => l.Id == lesson.Id);
        if (lessonToRemove != null)
        {
            _lessons.Remove(lessonToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalLessons()
    {
        return _lessons.Count;
    }

    public int GetTotalDurationMinutes()
    {
        return _lessons.Sum(l => l.DurationMinutes);
    }
}
