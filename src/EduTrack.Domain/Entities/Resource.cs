using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// Resource entity - represents a file or media resource associated with a lesson
/// </summary>
public class Resource
{
    public int Id { get; private set; }
    public int LessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ResourceType Type { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string? Url { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public string? MimeType { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Lesson Lesson { get; private set; } = null!;

    // Private constructor for EF Core
    private Resource() { }

    public static Resource Create(int lessonId, string title, ResourceType type, 
        string filePath, string? url, long? fileSizeBytes, string? mimeType, int order)
    {
        if (lessonId <= 0)
            throw new ArgumentException("Lesson ID must be greater than 0", nameof(lessonId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath cannot be null or empty", nameof(filePath));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new Resource
        {
            LessonId = lessonId,
            Title = title,
            Type = type,
            FilePath = filePath,
            Url = url,
            FileSizeBytes = fileSizeBytes,
            MimeType = mimeType,
            Order = order,
            CreatedAt = DateTimeOffset.UtcNow,
            IsActive = true
        };
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void UpdateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("FilePath cannot be null or empty", nameof(filePath));

        FilePath = filePath;
    }

    public void UpdateUrl(string? url)
    {
        Url = url;
    }

    public void UpdateFileSize(long? fileSizeBytes)
    {
        FileSizeBytes = fileSizeBytes;
    }

    public void UpdateMimeType(string? mimeType)
    {
        MimeType = mimeType;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        Order = order;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
