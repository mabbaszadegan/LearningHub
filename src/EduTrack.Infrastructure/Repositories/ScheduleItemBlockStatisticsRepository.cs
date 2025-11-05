using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ScheduleItemBlockStatistics entity
/// </summary>
public class ScheduleItemBlockStatisticsRepository : Repository<ScheduleItemBlockStatistics>, IScheduleItemBlockStatisticsRepository
{
    public ScheduleItemBlockStatisticsRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<ScheduleItemBlockStatistics?> GetByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(
                s => s.StudentId == studentId && s.ScheduleItemId == scheduleItemId && s.BlockId == blockId, 
                cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetByStudentAsync(
        string studentId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetByStudentAndScheduleItemAsync(
        string studentId, 
        int scheduleItemId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .Where(s => s.StudentId == studentId && s.ScheduleItemId == scheduleItemId)
            .OrderBy(s => s.BlockOrder)
            .ThenBy(s => s.BlockId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksWithMostErrorsAsync(
        string studentId, 
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .Where(s => s.StudentId == studentId && s.IncorrectAttempts > 0)
            .OrderByDescending(s => s.IncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksNeverCorrectAsync(
        string studentId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .Where(s => s.StudentId == studentId && s.TotalAttempts > 0 && s.CorrectAttempts == 0)
            .OrderByDescending(s => s.IncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksWithRecentMistakesAsync(
        string studentId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .Include(s => s.Student)
            .Where(s => s.StudentId == studentId && s.ConsecutiveIncorrectAttempts >= 3)
            .OrderByDescending(s => s.ConsecutiveIncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }
}

