using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveLessonStage entity - represents a stage within an interactive lesson
/// </summary>
public class InteractiveLessonStage
{
    private readonly List<StageContentItem> _contentItems = new();

    public int Id { get; private set; }
    public int InteractiveLessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public InteractiveLessonStageType StageType { get; private set; }
    public ContentArrangementType ArrangementType { get; private set; }
    public string? TextContent { get; private set; }
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public InteractiveLesson InteractiveLesson { get; private set; } = null!;
    public IReadOnlyCollection<StageContentItem> ContentItems => _contentItems.AsReadOnly();

    // Private constructor for EF Core
    private InteractiveLessonStage() { }

    public static InteractiveLessonStage Create(int interactiveLessonId, string title, 
        InteractiveLessonStageType stageType, ContentArrangementType arrangementType,
        string? description = null, string? textContent = null, int order = 0)
    {
        if (interactiveLessonId <= 0)
            throw new ArgumentException("InteractiveLesson ID must be greater than 0", nameof(interactiveLessonId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        // Validate text content for text-only stages
        if (stageType == InteractiveLessonStageType.TextOnly && string.IsNullOrWhiteSpace(textContent))
            throw new ArgumentException("TextContent is required for TextOnly stage type", nameof(textContent));

        return new InteractiveLessonStage
        {
            InteractiveLessonId = interactiveLessonId,
            Title = title,
            Description = description,
            StageType = stageType,
            ArrangementType = arrangementType,
            TextContent = textContent,
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

    public void UpdateTextContent(string? textContent)
    {
        if (StageType == InteractiveLessonStageType.TextOnly && string.IsNullOrWhiteSpace(textContent))
            throw new ArgumentException("TextContent cannot be empty for TextOnly stage type");

        TextContent = textContent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateStageType(InteractiveLessonStageType stageType)
    {
        // If changing to TextOnly, ensure text content exists
        if (stageType == InteractiveLessonStageType.TextOnly && string.IsNullOrWhiteSpace(TextContent))
            throw new InvalidOperationException("Cannot change to TextOnly stage type without text content");

        StageType = stageType;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateArrangementType(ContentArrangementType arrangementType)
    {
        ArrangementType = arrangementType;
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

    public void AddContentItem(StageContentItem contentItem)
    {
        if (contentItem == null)
            throw new ArgumentNullException(nameof(contentItem));

        if (_contentItems.Any(ci => ci.Id == contentItem.Id))
            throw new InvalidOperationException("Content item already exists in this stage");

        _contentItems.Add(contentItem);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveContentItem(StageContentItem contentItem)
    {
        if (contentItem == null)
            throw new ArgumentNullException(nameof(contentItem));

        var contentItemToRemove = _contentItems.FirstOrDefault(ci => ci.Id == contentItem.Id);
        if (contentItemToRemove != null)
        {
            _contentItems.Remove(contentItemToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool HasContentItems()
    {
        return _contentItems.Any();
    }

    public int GetTotalContentItems()
    {
        return _contentItems.Count;
    }

    public bool IsTextOnly()
    {
        return StageType == InteractiveLessonStageType.TextOnly;
    }

    public bool UsesSubChapterContent()
    {
        return StageType == InteractiveLessonStageType.SubChapterContent || 
               StageType == InteractiveLessonStageType.Mixed;
    }

    public bool IsQuestionStage()
    {
        return StageType == InteractiveLessonStageType.Question;
    }

    public bool HasTextContent()
    {
        return !string.IsNullOrWhiteSpace(TextContent);
    }
}
