using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ITeachingSessionAttendanceRepository
{
    Task<TeachingSessionAttendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionAttendance>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TeachingSessionAttendance>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task AddAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default);
    Task UpdateAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default);
    Task DeleteAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
