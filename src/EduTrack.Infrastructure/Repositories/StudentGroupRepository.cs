using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for StudentGroup entity
/// </summary>
public class StudentGroupRepository : Repository<StudentGroup>, IStudentGroupRepository
{
    public StudentGroupRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StudentGroup>> GetGroupsByTeachingPlanAsync(int teachingPlanId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(sg => sg.TeachingPlanId == teachingPlanId)
            .Include(sg => sg.Members)
                .ThenInclude(m => m.StudentProfile)
                .ThenInclude(sp => sp.User)
            .OrderBy(sg => sg.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentGroup?> GetGroupWithMembersAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sg => sg.Members)
                .ThenInclude(m => m.StudentProfile)
                .ThenInclude(sp => sp.User)
            .Include(sg => sg.TeachingPlan)
            .FirstOrDefaultAsync(sg => sg.Id == groupId, cancellationToken);
    }

    public async Task<bool> IsStudentProfileInGroupAsync(int groupId, int studentProfileId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(sg => sg.Id == groupId)
            .SelectMany(sg => sg.Members)
            .AnyAsync(m => m.StudentProfileId == studentProfileId, cancellationToken);
    }
}
