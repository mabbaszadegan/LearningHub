using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Chapter entity
/// </summary>
public class ChapterRepository : Repository<Chapter>, IChapterRepository
{
    public ChapterRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Chapter>> GetChaptersByCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseId == courseId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Chapter>> GetActiveChaptersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chapter?> GetChapterWithSubChaptersAsync(int chapterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.SubChapters)
            .FirstOrDefaultAsync(c => c.Id == chapterId, cancellationToken);
    }
}
