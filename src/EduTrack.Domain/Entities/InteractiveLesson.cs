namespace EduTrack.Domain.Entities;

/// <summary>
/// InteractiveLesson entity - represents an interactive lesson that combines educational content and questions
/// </summary>
public class InteractiveLesson
{
    private readonly List<InteractiveContentItem> _contentItems = new();
    private readonly List<InteractiveLessonStage> _stages = new();
    private readonly List<InteractiveLessonSubChapter> _subChapters = new();

    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public Course Course { get; private set; } = null!;
    public IReadOnlyCollection<InteractiveContentItem> ContentItems => _contentItems.AsReadOnly();
    public IReadOnlyCollection<InteractiveLessonStage> Stages => _stages.AsReadOnly();
    public IReadOnlyCollection<InteractiveLessonSubChapter> SubChapters => _subChapters.AsReadOnly();

    // Private constructor for EF Core
    private InteractiveLesson() { }

    public static InteractiveLesson Create(int courseId, string title, string? description, 
        int order, string createdBy)
    {
        if (courseId <= 0)
            throw new ArgumentException("Course ID must be greater than 0", nameof(courseId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        return new InteractiveLesson
        {
            CourseId = courseId,
            Title = title,
            Description = description,
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

    public void AddContentItem(InteractiveContentItem contentItem)
    {
        if (contentItem == null)
            throw new ArgumentNullException(nameof(contentItem));

        if (_contentItems.Any(ci => ci.Id == contentItem.Id))
            throw new InvalidOperationException("Content item already exists in this lesson");

        _contentItems.Add(contentItem);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveContentItem(InteractiveContentItem contentItem)
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

    public int GetTotalQuestions()
    {
        return _contentItems.Count(ci => ci.InteractiveQuestionId.HasValue);
    }

    // EducationalContent removed - using InteractiveQuestion only

    // Stage management methods
    public void AddStage(InteractiveLessonStage stage)
    {
        if (stage == null)
            throw new ArgumentNullException(nameof(stage));

        if (_stages.Any(s => s.Id == stage.Id))
            throw new InvalidOperationException("Stage already exists in this lesson");

        _stages.Add(stage);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveStage(InteractiveLessonStage stage)
    {
        if (stage == null)
            throw new ArgumentNullException(nameof(stage));

        var stageToRemove = _stages.FirstOrDefault(s => s.Id == stage.Id);
        if (stageToRemove != null)
        {
            _stages.Remove(stageToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public bool HasStages()
    {
        return _stages.Any();
    }

    public int GetTotalStages()
    {
        return _stages.Count;
    }

    // Sub-chapter management methods
    public void AddSubChapter(InteractiveLessonSubChapter subChapter)
    {
        if (subChapter == null)
            throw new ArgumentNullException(nameof(subChapter));

        if (_subChapters.Any(sc => sc.Id == subChapter.Id))
            throw new InvalidOperationException("Sub-chapter already exists in this lesson");

        _subChapters.Add(subChapter);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSubChapter(InteractiveLessonSubChapter subChapter)
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

    public bool HasSubChapters()
    {
        return _subChapters.Any();
    }

    public int GetTotalSubChapters()
    {
        return _subChapters.Count;
    }

    public bool UsesStages()
    {
        return HasStages();
    }

    public bool UsesLegacyContentItems()
    {
        return HasContentItems() && !HasStages();
    }
}
