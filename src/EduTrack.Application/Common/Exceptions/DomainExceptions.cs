using EduTrack.Application.Common.Models;

namespace EduTrack.Application.Common.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId) 
        : base($"Entity of type '{entityType}' with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : DomainException
{
    public List<string> Errors { get; }

    public ValidationException(List<string> errors) 
        : base($"Validation failed: {string.Join(", ", errors)}")
    {
        Errors = errors;
    }

    public ValidationException(string error) 
        : base($"Validation failed: {error}")
    {
        Errors = new List<string> { error };
    }
}

/// <summary>
/// Exception thrown when an operation is not allowed
/// </summary>
public class OperationNotAllowedException : DomainException
{
    public OperationNotAllowedException(string message) : base(message)
    {
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs
/// </summary>
public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
