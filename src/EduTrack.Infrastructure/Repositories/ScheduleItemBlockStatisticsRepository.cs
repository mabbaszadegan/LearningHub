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

    private static IQueryable<ScheduleItemBlockStatistics> FilterByStudent(
        IQueryable<ScheduleItemBlockStatistics> query,
        string studentId,
        int? studentProfileId)
    {
        query = query.Where(s => s.StudentId == studentId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(s => s.StudentProfileId == studentProfileId.Value);
        }

        return query;
    }

    public async Task<ScheduleItemBlockStatistics?> GetByStudentAndBlockAsync(
        string studentId, 
        int scheduleItemId, 
        string blockId, 
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId && s.BlockId == blockId);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetByStudentAsync(
        string studentId,
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId);

        return await query
            .OrderByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetByStudentAndScheduleItemAsync(
        string studentId, 
        int scheduleItemId, 
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId);

        return await query
            .OrderBy(s => s.BlockOrder)
            .ThenBy(s => s.BlockId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksWithMostErrorsAsync(
        string studentId, 
        int? studentProfileId = null,
        int count = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId)
            .Where(s => s.IncorrectAttempts > 0);

        return await query
            .OrderByDescending(s => s.IncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksNeverCorrectAsync(
        string studentId,
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId)
            .Where(s => s.TotalAttempts > 0 && s.CorrectAttempts == 0);

        return await query
            .OrderByDescending(s => s.IncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemBlockStatistics>> GetBlocksWithRecentMistakesAsync(
        string studentId,
        int? studentProfileId = null,
        CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(s => s.ScheduleItem).Include(s => s.Student), studentId, studentProfileId)
            .Where(s => s.ConsecutiveIncorrectAttempts >= 3);

        return await query
            .OrderByDescending(s => s.ConsecutiveIncorrectAttempts)
            .ThenByDescending(s => s.LastAttemptAt)
            .ToListAsync(cancellationToken);
    }
}

