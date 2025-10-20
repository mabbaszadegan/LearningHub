using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for StudySession entity
/// </summary>
public class StudySessionRepository : IStudySessionRepository
{
    private readonly AppDbContext _context;

    public StudySessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StudySession?> GetByIdAsync(int id)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Include(s => s.EducationalContent)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<StudySession>> GetByStudentIdAsync(string studentId)
    {
        return await _context.StudySessions
            .Include(s => s.EducationalContent)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetByEducationalContentIdAsync(int educationalContentId)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Where(s => s.EducationalContentId == educationalContentId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetByStudentAndContentAsync(string studentId, int educationalContentId)
    {
        return await _context.StudySessions
            .Include(s => s.EducationalContent)
            .Where(s => s.StudentId == studentId && s.EducationalContentId == educationalContentId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<StudySession?> GetActiveSessionAsync(string studentId, int educationalContentId)
    {
        return await _context.StudySessions
            .Include(s => s.EducationalContent)
            .FirstOrDefaultAsync(s => s.StudentId == studentId && 
                                     s.EducationalContentId == educationalContentId && 
                                     !s.IsCompleted && s.EndedAt == null);
    }

    public async Task<IEnumerable<StudySession>> GetCompletedSessionsByStudentAsync(string studentId)
    {
        return await _context.StudySessions
            .Include(s => s.EducationalContent)
            .Where(s => s.StudentId == studentId && s.IsCompleted)
            .OrderByDescending(s => s.EndedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetCompletedSessionsByContentAsync(int educationalContentId)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Where(s => s.EducationalContentId == educationalContentId && s.IsCompleted)
            .OrderByDescending(s => s.EndedAt)
            .ToListAsync();
    }

    public async Task<StudySession> AddAsync(StudySession studySession)
    {
        _context.StudySessions.Add(studySession);
        await _context.SaveChangesAsync();
        return studySession;
    }

    public async Task UpdateAsync(StudySession studySession)
    {
        _context.StudySessions.Update(studySession);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var studySession = await _context.StudySessions.FindAsync(id);
        if (studySession != null)
        {
            _context.StudySessions.Remove(studySession);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalStudyTimeAsync(string studentId, int educationalContentId)
    {
        return await _context.StudySessions
            .Where(s => s.StudentId == studentId && 
                       s.EducationalContentId == educationalContentId && 
                       s.IsCompleted)
            .SumAsync(s => s.DurationSeconds);
    }

    public async Task<int> GetStudySessionsCountAsync(string studentId, int educationalContentId)
    {
        return await _context.StudySessions
            .CountAsync(s => s.StudentId == studentId && 
                           s.EducationalContentId == educationalContentId && 
                           s.IsCompleted);
    }

    public async Task<IEnumerable<StudySession>> GetRecentSessionsAsync(string studentId, int count = 10)
    {
        return await _context.StudySessions
            .Include(s => s.EducationalContent)
            .Where(s => s.StudentId == studentId && s.IsCompleted)
            .OrderByDescending(s => s.EndedAt)
            .Take(count)
            .ToListAsync();
    }
}
