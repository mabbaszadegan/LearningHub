using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for EducationalContent entity
/// </summary>
public class EducationalContentRepository : Repository<EducationalContent>, IEducationalContentRepository
{
    public EducationalContentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EducationalContent>> GetContentBySubChapterAsync(int subChapterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ec => ec.SubChapterId == subChapterId)
            .OrderBy(ec => ec.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EducationalContent>> GetContentByTypeAsync(EducationalContentType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ec => ec.Type == type && ec.IsActive)
            .OrderBy(ec => ec.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EducationalContent>> GetActiveContentAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ec => ec.IsActive)
            .OrderBy(ec => ec.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
