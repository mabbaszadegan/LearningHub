namespace EduTrack.Domain.Entities;

/// <summary>
/// ActivityLog entity - represents user activity logs
/// </summary>
public class ActivityLog
{
    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public int? EntityId { get; private set; }
    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    // Navigation properties
    public User User { get; private set; } = null!;

    // Private constructor for EF Core
    private ActivityLog() { }

    public static ActivityLog Create(string userId, string action, string? entityType = null, 
        int? entityId = null, string? details = null, string? ipAddress = null, 
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or empty", nameof(action));

        return new ActivityLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    public void UpdateDetails(string? details)
    {
        Details = details;
    }

    public bool IsRecent(TimeSpan timeSpan)
    {
        return DateTimeOffset.UtcNow - Timestamp <= timeSpan;
    }

    public string GetFormattedTimestamp()
    {
        return Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
