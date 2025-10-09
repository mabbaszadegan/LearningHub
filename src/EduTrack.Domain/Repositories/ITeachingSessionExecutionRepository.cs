using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ITeachingSessionExecutionRepository
{
    Task<TeachingSessionExecution?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionExecution>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionExecution>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task AddAsync(TeachingSessionExecution execution, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeachingSessionExecution execution, CancellationToken cancellationToken = default);
    Task DeleteAsync(TeachingSessionExecution execution, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
