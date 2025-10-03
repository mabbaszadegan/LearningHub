using EduTrack.Domain.Enums;

namespace EduTrack.Domain.Entities;

/// <summary>
/// EducationalContent entity - represents educational content within a sub-chapter
/// </summary>
public class EducationalContent
{
    public int Id { get; private set; }
    public int SubChapterId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public EducationalContentType Type { get; private set; }
    public string? TextContent { get; private set; }
    public int? FileId { get; private set; }
    public string? ExternalUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int Order { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;

    // Navigation properties
    public SubChapter SubChapter { get; private set; } = null!;
    public File? File { get; private set; }

    // Private constructor for EF Core
    private EducationalContent() { }

    public static EducationalContent Create(int subChapterId, string title, EducationalContentType type, 
        string? textContent, int? fileId, string? externalUrl, int order, string createdBy)
    {
        if (subChapterId <= 0)
            throw new ArgumentException("SubChapter ID must be greater than 0", nameof(subChapterId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be null or empty", nameof(createdBy));

        // Validate content based on type
        switch (type)
        {
            case EducationalContentType.Text:
                if (string.IsNullOrWhiteSpace(textContent))
                    throw new ArgumentException("TextContent is required for Text type", nameof(textContent));
                break;
            case EducationalContentType.ExternalUrl:
                if (string.IsNullOrWhiteSpace(externalUrl))
                    throw new ArgumentException("ExternalUrl is required for ExternalUrl type", nameof(externalUrl));
                break;
            case EducationalContentType.Image:
            case EducationalContentType.Video:
            case EducationalContentType.Audio:
            case EducationalContentType.PDF:
            case EducationalContentType.File:
                if (!fileId.HasValue)
                    throw new ArgumentException("FileId is required for file-based content types", nameof(fileId));
                break;
        }

        return new EducationalContent
        {
            SubChapterId = subChapterId,
            Title = title,
            Type = type,
            TextContent = textContent,
            FileId = fileId,
            ExternalUrl = externalUrl,
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

    public void UpdateTextContent(string? textContent)
    {
        if (Type == EducationalContentType.Text && string.IsNullOrWhiteSpace(textContent))
            throw new ArgumentException("TextContent cannot be empty for Text type");

        TextContent = textContent;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateExternalUrl(string? externalUrl)
    {
        if (Type == EducationalContentType.ExternalUrl && string.IsNullOrWhiteSpace(externalUrl))
            throw new ArgumentException("ExternalUrl cannot be empty for ExternalUrl type");

        ExternalUrl = externalUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFileId(int? fileId)
    {
        if (Type != EducationalContentType.Text && Type != EducationalContentType.ExternalUrl && !fileId.HasValue)
            throw new ArgumentException("FileId is required for file-based content types");

        FileId = fileId;
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

    public bool IsTextContent()
    {
        return Type == EducationalContentType.Text;
    }

    public bool IsFileContent()
    {
        return Type == EducationalContentType.Image ||
               Type == EducationalContentType.Video ||
               Type == EducationalContentType.Audio ||
               Type == EducationalContentType.PDF ||
               Type == EducationalContentType.File;
    }

    public bool IsExternalUrl()
    {
        return Type == EducationalContentType.ExternalUrl;
    }

    public bool HasFile()
    {
        return FileId.HasValue;
    }

    public bool HasText()
    {
        return !string.IsNullOrWhiteSpace(TextContent);
    }

    public bool HasExternalUrl()
    {
        return !string.IsNullOrWhiteSpace(ExternalUrl);
    }
}
