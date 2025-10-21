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

    public Task RemoveStudentAssignmentsAsync(IEnumerable<ScheduleItemStudentAssignment> assignments, CancellationToken cancellationToken = default)
    {
        if (assignments.Any())
        {
            _context.Set<ScheduleItemStudentAssignment>().RemoveRange(assignments);
            // Note: SaveChangesAsync is called by the calling code to maintain transaction integrity
        }
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ScheduleItemGroupAssignment>> GetGroupAssignmentsAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ScheduleItemGroupAssignment>()
            .Where(ga => ga.ScheduleItemId == scheduleItemId)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveGroupAssignmentsAsync(IEnumerable<ScheduleItemGroupAssignment> assignments, CancellationToken cancellationToken = default)
    {
        if (assignments.Any())
        {
            _context.Set<ScheduleItemGroupAssignment>().RemoveRange(assignments);
            // Note: SaveChangesAsync is called by the calling code to maintain transaction integrity
        }
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ScheduleItemSubChapterAssignment>> GetSubChapterAssignmentsAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<ScheduleItemSubChapterAssignment>()
            .Where(sca => sca.ScheduleItemId == scheduleItemId)
            .ToListAsync(cancellationToken);
    }

    public Task RemoveSubChapterAssignmentsAsync(IEnumerable<ScheduleItemSubChapterAssignment> assignments, CancellationToken cancellationToken = default)
    {
        if (assignments.Any())
        {
            _context.Set<ScheduleItemSubChapterAssignment>().RemoveRange(assignments);
            // Note: SaveChangesAsync is called by the calling code to maintain transaction integrity
        }
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ScheduleItem>> GetScheduleItemsAccessibleToStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        // Get all schedule items that the student has access to based on the rules:
        // 1. Direct assignment to the student
        // 2. Assignment to student's group AND no individual students in that group are assigned
        // 3. No group assignments (accessible to all students)
        
        var accessibleItems = await _dbSet
            .Include(si => si.GroupAssignments)
                .ThenInclude(ga => ga.StudentGroup)
            .Include(si => si.SubChapterAssignments)
                .ThenInclude(sca => sca.SubChapter)
            .Include(si => si.StudentAssignments)
                .ThenInclude(sa => sa.Student)
            .Include(si => si.TeachingPlan)
            .Include(si => si.Group)
            .Include(si => si.Lesson)
            .Where(si => 
                // Rule 1: Direct assignment to student
                si.StudentAssignments.Any(sa => sa.StudentId == studentId) ||
                
                // Rule 2: Assignment to student's group AND no individual students in that group are assigned
                si.GroupAssignments.Any(ga => 
                    ga.StudentGroup.Members.Any(m => m.StudentId == studentId) &&
                    !si.StudentAssignments.Any(sa => 
                        ga.StudentGroup.Members.Any(m => m.StudentId == sa.StudentId)
                    )
                ) ||
                
                // Rule 3: No group assignments (accessible to all students)
                !si.GroupAssignments.Any()
            )
            .OrderBy(si => si.StartDate)
            .ToListAsync(cancellationToken);

        return accessibleItems;
    }
}
