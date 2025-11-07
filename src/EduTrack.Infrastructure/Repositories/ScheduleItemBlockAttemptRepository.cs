using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ScheduleItemBlockAttempt entity
/// </summary>
public class ScheduleItemBlockAttemptRepository : Repository<ScheduleItemBlockAttempt>, IScheduleItemBlockAttemptRepository
{
    public ScheduleItemBlockAttemptRepository(AppDbContext context) : base(context)
    {
    }

    private static IQueryable<ScheduleItemBlockAttempt> FilterByStudent(
        IQueryable<ScheduleItemBlockAttempt> query,
        string studentId,
        int? studentProfileId)
    {
        query = query.Where(a => a.StudentId == studentId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(a => a.StudentProfileId == studentProfileId.Value);
        }

        return query;
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAndScheduleItemAsync(
        string studentId, 
        int scheduleItemId, 
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(a => a.ScheduleItem).Include(a => a.Student), studentId, studentProfileId)
            .Where(a => a.ScheduleItemId == scheduleItemId);

        return await query
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(a => a.ScheduleItem).Include(a => a.Student), studentId, studentProfileId)
            .Where(a => a.ScheduleItemId == scheduleItemId && a.BlockId == blockId);

        return await query
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAsync(
        string studentId,
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(a => a.ScheduleItem).Include(a => a.Student), studentId, studentProfileId);

        return await query
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetRecentAttemptsByStudentAsync(
        string studentId, 
        int? studentProfileId = null,
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(a => a.ScheduleItem).Include(a => a.Student), studentId, studentProfileId);

        return await query
            .OrderByDescending(a => a.AttemptedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduleItemBlockAttempt?> GetLastAttemptByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(a => a.ScheduleItem).Include(a => a.Student), studentId, studentProfileId)
            .Where(a => a.ScheduleItemId == scheduleItemId && a.BlockId == blockId);

        return await query
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

