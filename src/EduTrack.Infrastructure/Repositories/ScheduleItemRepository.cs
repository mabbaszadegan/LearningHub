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
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Where(si => si.TeachingPlanId == teachingPlanId)
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Where(si => si.GroupId == groupId || si.GroupAssignments.Any(ga => ga.StudentGroupId == groupId))
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
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Where(si => si.StartDate <= now && (!si.DueDate.HasValue || si.DueDate.Value >= now))
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsBySubChapterAsync(int subChapterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Where(si => si.SubChapterAssignments.Any(sca => sca.SubChapterId == subChapterId))
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsByGroupAndSubChapterAsync(int groupId, int subChapterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Where(si => (si.GroupId == groupId || si.GroupAssignments.Any(ga => ga.StudentGroupId == groupId)) &&
                        si.SubChapterAssignments.Any(sca => sca.SubChapterId == subChapterId))
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);
    }

    public override async Task<ScheduleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Include(si => si.StudentAssignments)
                .ThenInclude(sa => sa.Student)
            .Include(si => si.TeachingPlan)
            .Include(si => si.Group)
            .Include(si => si.Lesson)
            .FirstOrDefaultAsync(si => si.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ScheduleItemStudentAssignment>> GetStudentAssignmentsAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ScheduleItemStudentAssignment>()
            .Where(sa => sa.ScheduleItemId == scheduleItemId)
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveStudentAssignmentsAsync(IEnumerable<ScheduleItemStudentAssignment> assignments, CancellationToken cancellationToken = default)
    {
        if (assignments.Any())
        {
            _context.Set<ScheduleItemStudentAssignment>().RemoveRange(assignments);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
