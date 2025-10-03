namespace EduTrack.Domain.Events;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTimeOffset OccurredOn { get; }
    
    /// <summary>
    /// Version of the aggregate that raised the event
    /// </summary>
    int Version { get; }
}

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public int Version { get; }

    protected DomainEvent(int version)
    {
        Version = version;
    }
}
