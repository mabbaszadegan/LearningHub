using EduTrack.Application.Common.Interfaces;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EduTrack.Infrastructure.Services;

/// <summary>
/// Domain Event Service implementation using Observer pattern
/// </summary>
public class DomainEventService : IDomainEventService
{
    private readonly ILogger<DomainEventService> _logger;

    public DomainEventService(ILogger<DomainEventService> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        _logger.LogInformation("Publishing domain event: {EventType} with ID: {EventId}", 
            typeof(T).Name, domainEvent.Id);

        // In a real implementation, this would publish to a message bus or event store
        // For now, we'll just log the event
        await Task.CompletedTask;
    }

    public async Task PublishManyAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        var eventsList = domainEvents.ToList();
        _logger.LogInformation("Publishing {Count} domain events of type: {EventType}", 
            eventsList.Count, typeof(T).Name);

        foreach (var domainEvent in eventsList)
        {
            await PublishAsync(domainEvent, cancellationToken);
        }
    }
}
