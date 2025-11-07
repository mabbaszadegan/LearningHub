using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

/// <summary>
/// Repository abstraction for working with student learner profiles.
/// </summary>
public interface IStudentProfileRepository : IRepository<StudentProfile>
{
    Task<IEnumerable<StudentProfile>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<StudentProfile?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithDisplayNameAsync(string userId, string displayName, CancellationToken cancellationToken = default);
}

