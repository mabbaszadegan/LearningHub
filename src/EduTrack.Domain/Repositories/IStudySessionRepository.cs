using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

/// <summary>
/// Repository interface for StudySession entity
/// </summary>
public interface IStudySessionRepository
{
    Task<StudySession?> GetByIdAsync(int id);
    Task<IEnumerable<StudySession>> GetByStudentIdAsync(string studentId, int? studentProfileId = null);
    Task<IEnumerable<StudySession>> GetByScheduleItemIdAsync(int scheduleItemId);
    Task<IEnumerable<StudySession>> GetByStudentAndScheduleItemAsync(string studentId, int scheduleItemId, int? studentProfileId = null);
    Task<StudySession?> GetActiveSessionAsync(string studentId, int scheduleItemId, int? studentProfileId = null);
    Task<IEnumerable<StudySession>> GetCompletedSessionsByStudentAsync(string studentId, int? studentProfileId = null);
    Task<IEnumerable<StudySession>> GetCompletedSessionsByScheduleItemAsync(int scheduleItemId);
    Task<StudySession> AddAsync(StudySession studySession);
    Task UpdateAsync(StudySession studySession);
    Task DeleteAsync(int id);
    Task<int> GetTotalStudyTimeAsync(string studentId, int scheduleItemId, int? studentProfileId = null);
    Task<int> GetStudySessionsCountAsync(string studentId, int scheduleItemId, int? studentProfileId = null);
    Task<IEnumerable<StudySession>> GetRecentSessionsAsync(string studentId, int? studentProfileId = null, int count = 10);
    Task<DateTimeOffset?> GetLastEndedAtAsync(string studentId, int scheduleItemId, int? studentProfileId = null);
}