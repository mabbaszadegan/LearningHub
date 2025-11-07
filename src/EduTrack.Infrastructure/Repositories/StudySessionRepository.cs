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

    private static IQueryable<StudySession> FilterByStudent(IQueryable<StudySession> query, string studentId, int? studentProfileId)
    {
        query = query.Where(s => s.StudentId == studentId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(s => s.StudentProfileId == studentProfileId.Value);
        }

        return query;
    }

    public async Task<StudySession?> GetByIdAsync(int id)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Include(s => s.ScheduleItem)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<StudySession>> GetByStudentIdAsync(string studentId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.Include(s => s.ScheduleItem), studentId, studentProfileId);

        return await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetByScheduleItemIdAsync(int scheduleItemId)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Where(s => s.ScheduleItemId == scheduleItemId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetByStudentAndScheduleItemAsync(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.Include(s => s.ScheduleItem), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId);

        return await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();
    }

    public async Task<StudySession?> GetActiveSessionAsync(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.Include(s => s.ScheduleItem), studentId, studentProfileId);

        return await query
            .FirstOrDefaultAsync(s => s.ScheduleItemId == scheduleItemId && 
                                     !s.IsCompleted && s.EndedAt == null);
    }

    public async Task<IEnumerable<StudySession>> GetCompletedSessionsByStudentAsync(string studentId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.Include(s => s.ScheduleItem), studentId, studentProfileId)
            .Where(s => s.IsCompleted);

        return await query
            .OrderByDescending(s => s.EndedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudySession>> GetCompletedSessionsByScheduleItemAsync(int scheduleItemId)
    {
        return await _context.StudySessions
            .Include(s => s.Student)
            .Where(s => s.ScheduleItemId == scheduleItemId && s.IsCompleted)
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

    public async Task<int> GetTotalStudyTimeAsync(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.AsQueryable(), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId && s.IsCompleted);

        return await query
            .SumAsync(s => s.DurationSeconds);
    }

    public async Task<int> GetStudySessionsCountAsync(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.AsQueryable(), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId && s.IsCompleted);

        return await query.CountAsync();
    }

    public async Task<IEnumerable<StudySession>> GetRecentSessionsAsync(string studentId, int? studentProfileId = null, int count = 10)
    {
        var query = FilterByStudent(_context.StudySessions.Include(s => s.ScheduleItem), studentId, studentProfileId)
            .Where(s => s.IsCompleted);

        return await query
            .OrderByDescending(s => s.EndedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<DateTimeOffset?> GetLastEndedAtAsync(string studentId, int scheduleItemId, int? studentProfileId = null)
    {
        var query = FilterByStudent(_context.StudySessions.AsQueryable(), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId && s.IsCompleted && s.EndedAt != null);

        return await query
            .OrderByDescending(s => s.EndedAt)
            .Select(s => s.EndedAt!.Value)
            .FirstOrDefaultAsync();
    }
}