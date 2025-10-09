using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ITeachingPlanProgressRepository
{
    Task<TeachingPlanProgress?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TeachingPlanProgress?> GetByPlanSubTopicAndGroupAsync(int planId, int subtopicId, int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingPlanProgress>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingPlanProgress>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingPlanProgress>> GetBySubTopicIdAsync(int subtopicId, CancellationToken cancellationToken = default);
    Task AddAsync(TeachingPlanProgress progress, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeachingPlanProgress progress, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
