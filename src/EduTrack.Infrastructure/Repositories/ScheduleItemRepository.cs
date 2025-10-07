using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for ScheduleItem entity
/// </summary>
public class ScheduleItemRepository : Repository<ScheduleItem>, IScheduleItemRepository
{
    public ScheduleItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsByTeachingPlanAsync(int teachingPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(si => si.TeachingPlanId == teachingPlanId)
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(si => si.GroupId == groupId)
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsByTypeAsync(ScheduleItemType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(si => si.Type == type)
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetUpcomingScheduleItemsAsync(DateTimeOffset fromDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(si => si.StartDate >= fromDate)
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetOverdueScheduleItemsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbSet
            .Where(si => si.DueDate.HasValue && si.DueDate.Value < now)
            .OrderBy(si => si.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetActiveScheduleItemsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbSet
            .Where(si => si.StartDate <= now && (!si.DueDate.HasValue || si.DueDate.Value >= now))
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }
}
