using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

/// <summary>
/// Repository interface for StudySession entity
/// </summary>
public interface IStudySessionRepository
{
    Task<StudySession?> GetByIdAsync(int id);
    Task<IEnumerable<StudySession>> GetByStudentIdAsync(string studentId);
    Task<IEnumerable<StudySession>> GetByEducationalContentIdAsync(int educationalContentId);
    Task<IEnumerable<StudySession>> GetByStudentAndContentAsync(string studentId, int educationalContentId);
    Task<StudySession?> GetActiveSessionAsync(string studentId, int educationalContentId);
    Task<IEnumerable<StudySession>> GetCompletedSessionsByStudentAsync(string studentId);
    Task<IEnumerable<StudySession>> GetCompletedSessionsByContentAsync(int educationalContentId);
    Task<StudySession> AddAsync(StudySession studySession);
    Task UpdateAsync(StudySession studySession);
    Task DeleteAsync(int id);
    Task<int> GetTotalStudyTimeAsync(string studentId, int educationalContentId);
    Task<int> GetStudySessionsCountAsync(string studentId, int educationalContentId);
    Task<IEnumerable<StudySession>> GetRecentSessionsAsync(string studentId, int count = 10);
}
