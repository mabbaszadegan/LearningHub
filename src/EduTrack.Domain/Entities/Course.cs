namespace EduTrack.Domain.Entities;

/// <summary>
/// Course aggregate root - represents a course in the system
/// </summary>
public class Course
{
    private readonly List<Module> _modules = new();
    private readonly List<Chapter> _chapters = new();
    private readonly List<Class> _classes = new();

    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Thumbnail { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();
    public IReadOnlyCollection<Chapter> Chapters => _chapters.AsReadOnly();
    public IReadOnlyCollection<Class> Classes => _classes.AsReadOnly();

    // Private constructor for EF Core
    private Course() { }

    public static Course Create(string title, string? description, string? thumbnail, 
        int order, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Course
        {
            Title = title,
            Description = description,
            Thumbnail = thumbnail,
            Order = order,
            CreatedBy = createdBy,
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

    public void UpdateThumbnail(string? thumbnail)
    {
        Thumbnail = thumbnail;
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

    public void AddModule(Module module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        if (_modules.Any(m => m.Id == module.Id))
            throw new InvalidOperationException("Module already exists in this course");

        _modules.Add(module);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveModule(Module module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        var moduleToRemove = _modules.FirstOrDefault(m => m.Id == module.Id);
        if (moduleToRemove != null)
        {
            _modules.Remove(moduleToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddChapter(Chapter chapter)
    {
        if (chapter == null)
            throw new ArgumentNullException(nameof(chapter));

        if (_chapters.Any(c => c.Id == chapter.Id))
            throw new InvalidOperationException("Chapter already exists in this course");

        _chapters.Add(chapter);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveChapter(Chapter chapter)
    {
        if (chapter == null)
            throw new ArgumentNullException(nameof(chapter));

        var chapterToRemove = _chapters.FirstOrDefault(c => c.Id == chapter.Id);
        if (chapterToRemove != null)
        {
            _chapters.Remove(chapterToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        if (_classes.Any(c => c.Id == classEntity.Id))
            throw new InvalidOperationException("Class already exists for this course");

        _classes.Add(classEntity);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveClass(Class classEntity)
    {
        if (classEntity == null)
            throw new ArgumentNullException(nameof(classEntity));

        var classToRemove = _classes.FirstOrDefault(c => c.Id == classEntity.Id);
        if (classToRemove != null)
        {
            _classes.Remove(classToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalModules()
    {
        return _modules.Count;
    }

    public int GetTotalLessons()
    {
        return _modules.Sum(m => m.Lessons.Count);
    }

    public int GetTotalChapters()
    {
        return _chapters.Count;
    }

    public int GetTotalClasses()
    {
        return _classes.Count;
    }
}
