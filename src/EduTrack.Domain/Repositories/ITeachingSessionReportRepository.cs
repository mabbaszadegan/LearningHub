using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ITeachingSessionReportRepository
{
    Task<TeachingSessionReport?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionReport>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionReport>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default);
    Task AddAsync(TeachingSessionReport report, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeachingSessionReport report, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasPlanningDataAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionPlan>> GetPlannedItemsAsync(int sessionId, CancellationToken cancellationToken = default);
}
