using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Lesson entity - represents a learning unit within a module
/// </summary>
public class Lesson
{
    private readonly List<Resource> _resources = new();
    private readonly List<Progress> _progresses = new();

    public int Id { get; private set; }
    public int ModuleId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Content { get; private set; }
    public string? VideoUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Module Module { get; private set; } = null!;
    public IReadOnlyCollection<Resource> Resources => _resources.AsReadOnly();
    public IReadOnlyCollection<Progress> Progresses => _progresses.AsReadOnly();

    // Private constructor for EF Core
    private Lesson() { }

    public static Lesson Create(int moduleId, string title, string? content, string? videoUrl, 
        int durationMinutes, int order)
    {
        if (moduleId <= 0)
            throw new ArgumentException("Module ID must be greater than 0", nameof(moduleId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be greater than 0", nameof(durationMinutes));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Lesson
        {
            ModuleId = moduleId,
            Title = title,
            Content = content,
            VideoUrl = videoUrl,
            DurationMinutes = durationMinutes,
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

    public void UpdateContent(string? content)
    {
        Content = content;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateVideoUrl(string? videoUrl)
    {
        VideoUrl = videoUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDuration(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ArgumentException("Duration must be greater than 0", nameof(durationMinutes));

        DurationMinutes = durationMinutes;
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

    public void AddResource(Resource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        if (_resources.Any(r => r.Id == resource.Id))
            throw new InvalidOperationException("Resource already exists in this lesson");

        _resources.Add(resource);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveResource(Resource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        var resourceToRemove = _resources.FirstOrDefault(r => r.Id == resource.Id);
        if (resourceToRemove != null)
        {
            _resources.Remove(resourceToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalResources()
    {
        return _resources.Count;
    }

    public bool HasVideo()
    {
        return !string.IsNullOrWhiteSpace(VideoUrl);
    }

    public bool HasContent()
    {
        return !string.IsNullOrWhiteSpace(Content);
    }
}
