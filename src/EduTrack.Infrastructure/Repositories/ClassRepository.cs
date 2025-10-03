using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Class entity
/// </summary>
public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Class>> GetClassesByTeacherAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.TeacherId == teacherId)
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetClassesByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseId == courseId)
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Class>> GetActiveClassesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Class?> GetClassWithEnrollmentsAsync(int classId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
    }

    public async Task<bool> IsStudentEnrolledAsync(int classId, string studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Id == classId)
            .SelectMany(c => c.Enrollments)
            .AnyAsync(e => e.StudentId == studentId && e.IsActive, cancellationToken);
    }
}
