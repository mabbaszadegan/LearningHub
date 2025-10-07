using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for TeachingPlan entity
/// </summary>
public class TeachingPlanRepository : Repository<TeachingPlan>, ITeachingPlanRepository
{
    public TeachingPlanRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TeachingPlan>> GetTeachingPlansByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(tp => tp.CourseId == courseId)
            .OrderBy(tp => tp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingPlan>> GetTeachingPlansByTeacherAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(tp => tp.TeacherId == teacherId)
            .OrderBy(tp => tp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TeachingPlan?> GetTeachingPlanWithGroupsAsync(int teachingPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tp => tp.Groups)
            .ThenInclude(g => g.Members)
            .ThenInclude(m => m.Student)
            .FirstOrDefaultAsync(tp => tp.Id == teachingPlanId, cancellationToken);
    }

    public async Task<TeachingPlan?> GetTeachingPlanWithScheduleItemsAsync(int teachingPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tp => tp.ScheduleItems)
            .ThenInclude(si => si.Group)
            .Include(tp => tp.ScheduleItems)
            .ThenInclude(si => si.Lesson)
            .FirstOrDefaultAsync(tp => tp.Id == teachingPlanId, cancellationToken);
    }

    public async Task<TeachingPlan?> GetTeachingPlanWithAllAsync(int teachingPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(tp => tp.Course)
            .Include(tp => tp.Teacher)
            .Include(tp => tp.Groups)
            .ThenInclude(g => g.Members)
            .ThenInclude(m => m.Student)
            .Include(tp => tp.ScheduleItems)
            .ThenInclude(si => si.Group)
            .Include(tp => tp.ScheduleItems)
            .ThenInclude(si => si.Lesson)
            .FirstOrDefaultAsync(tp => tp.Id == teachingPlanId, cancellationToken);
    }
}
