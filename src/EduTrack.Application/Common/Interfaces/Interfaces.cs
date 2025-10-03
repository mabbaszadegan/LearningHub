using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;

namespace EduTrack.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    IQueryable<T> GetAll();
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTimeOffset Now { get; }
    DateTimeOffset UtcNow { get; }
}

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}

public interface IFileStorageService
{
    Task<(string FilePath, string MD5Hash, long FileSize)> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
}