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

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAndScheduleItemAsync(
        string studentId, 
        int scheduleItemId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ScheduleItem)
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId && a.ScheduleItemId == scheduleItemId)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ScheduleItem)
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId && a.ScheduleItemId == scheduleItemId && a.BlockId == blockId)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetByStudentAsync(
        string studentId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ScheduleItem)
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockAttempt>> GetRecentAttemptsByStudentAsync(
        string studentId, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ScheduleItem)
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AttemptedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<ScheduleItemBlockAttempt?> GetLastAttemptByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.ScheduleItem)
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId && a.ScheduleItemId == scheduleItemId && a.BlockId == blockId)
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

