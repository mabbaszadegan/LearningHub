using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ITeachingSessionTopicCoverageRepository
{
    Task<TeachingSessionTopicCoverage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionTopicCoverage>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionTopicCoverage>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionTopicCoverage>> GetByTopicAsync(string topicType, int? topicId, CancellationToken cancellationToken = default);
    Task AddAsync(TeachingSessionTopicCoverage coverage, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeachingSessionTopicCoverage coverage, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
