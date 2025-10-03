using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Course entity
/// </summary>
public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Course>> GetActiveCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetCoursesByCreatorAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CreatedBy == createdBy)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetCoursesByOrderAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetCourseWithModulesAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Modules)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    public async Task<Course?> GetCourseWithChaptersAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Chapters)
            .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken);
    }

    public async Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(c => c.Title == title, cancellationToken);
    }
}
