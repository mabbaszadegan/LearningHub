namespace EduTrack.Domain.Entities;

/// <summary>
/// Chapter entity - represents a chapter within a course
/// </summary>
public class Chapter
{
    private readonly List<SubChapter> _subChapters = new();

    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Objective { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Course Course { get; private set; } = null!;
    public IReadOnlyCollection<SubChapter> SubChapters => _subChapters.AsReadOnly();

    // Private constructor for EF Core
    private Chapter() { }

    public static Chapter Create(int courseId, string title, string? description, 
        string objective, int order)
    {
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("Objective cannot be null or empty", nameof(objective));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Chapter
        {
            CourseId = courseId,
            Title = title,
            Description = description,
            Objective = objective,
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

    public void UpdateObjective(string objective)
    {
        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("Objective cannot be null or empty", nameof(objective));

        Objective = objective;
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

    public void AddSubChapter(SubChapter subChapter)
    {
        if (subChapter == null)
            throw new ArgumentNullException(nameof(subChapter));

        if (_subChapters.Any(sc => sc.Id == subChapter.Id))
            throw new InvalidOperationException("SubChapter already exists in this chapter");

        _subChapters.Add(subChapter);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSubChapter(SubChapter subChapter)
    {
        if (subChapter == null)
            throw new ArgumentNullException(nameof(subChapter));

        var subChapterToRemove = _subChapters.FirstOrDefault(sc => sc.Id == subChapter.Id);
        if (subChapterToRemove != null)
        {
            _subChapters.Remove(subChapterToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalSubChapters()
    {
        return _subChapters.Count;
    }

    public bool HasSubChapters()
    {
        return _subChapters.Any();
    }
}
