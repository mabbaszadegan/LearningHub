using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Interfaces;

/// <summary>
/// Service for getting current user information
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}

/// <summary>
/// Service for getting current time
/// </summary>
public interface IClock
{
    DateTimeOffset Now { get; }
    DateTimeOffset UtcNow { get; }
}

/// <summary>
/// Service for file storage operations
/// </summary>
public interface IFileStorageService
{
    Task<(string FilePath, string MD5Hash, long FileSize)> SaveFileAsync(
        Stream fileStream, 
        string originalFileName, 
        string contentType, 
        CancellationToken cancellationToken = default);
    
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for user operations
/// </summary>
public interface IUserService
{
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<bool> IsUserActiveAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for domain event handling
/// </summary>
public interface IDomainEventService
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : Domain.Events.IDomainEvent;
    Task PublishManyAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : Domain.Events.IDomainEvent;
}
