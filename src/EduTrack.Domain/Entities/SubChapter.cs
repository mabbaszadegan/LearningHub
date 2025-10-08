using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// SubChapter entity - represents a sub-chapter within a chapter
/// </summary>
public class SubChapter
{
    private readonly List<EducationalContent> _educationalContents = new();

    public int Id { get; private set; }
    public int ChapterId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Objective { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    // Navigation properties
    public Chapter Chapter { get; private set; } = null!;
    public IReadOnlyCollection<EducationalContent> EducationalContents => _educationalContents.AsReadOnly();
    
    // Navigation properties for new entities
    public ICollection<TeachingSessionTopicCoverage> TopicCoverages { get; set; } = new List<TeachingSessionTopicCoverage>();
    public ICollection<TeachingPlanProgress> PlanProgresses { get; set; } = new List<TeachingPlanProgress>();

    // Private constructor for EF Core
    private SubChapter() { }

    public static SubChapter Create(int chapterId, string title, string? description, 
        string objective, int order)
    {
        if (chapterId <= 0)
            throw new ArgumentException("Chapter ID must be greater than 0", nameof(chapterId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(objective))
            throw new ArgumentException("Objective cannot be null or empty", nameof(objective));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new SubChapter
        {
            ChapterId = chapterId,
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

    public void AddEducationalContent(EducationalContent content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        if (_educationalContents.Any(ec => ec.Id == content.Id))
            throw new InvalidOperationException("EducationalContent already exists in this sub-chapter");

        _educationalContents.Add(content);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveEducationalContent(EducationalContent content)
    {
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var contentToRemove = _educationalContents.FirstOrDefault(ec => ec.Id == content.Id);
        if (contentToRemove != null)
        {
            _educationalContents.Remove(contentToRemove);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public int GetTotalEducationalContent()
    {
        return _educationalContents.Count;
    }

    public bool HasEducationalContent()
    {
        return _educationalContents.Any();
    }
}
