using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class TeachingSessionTopicCoverageRepository : ITeachingSessionTopicCoverageRepository
{
    private readonly AppDbContext _context;

    public TeachingSessionTopicCoverageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingSessionTopicCoverage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionTopicCoverages
            .Include(c => c.TeachingSessionReport)
            .Include(c => c.StudentGroup)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionTopicCoverage>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionTopicCoverages
            .Include(c => c.StudentGroup)
            .Where(c => c.TeachingSessionReportId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionTopicCoverage>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionTopicCoverages
            .Include(c => c.TeachingSessionReport)
            .Where(c => c.StudentGroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionTopicCoverage>> GetByTopicAsync(string topicType, int? topicId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionTopicCoverages
            .Include(c => c.TeachingSessionReport)
            .Include(c => c.StudentGroup)
            .Where(c => c.TopicType == topicType && c.TopicId == topicId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TeachingSessionTopicCoverage coverage, CancellationToken cancellationToken = default)
    {
        await _context.TeachingSessionTopicCoverages.AddAsync(coverage, cancellationToken);
    }

    public async Task UpdateAsync(TeachingSessionTopicCoverage coverage, CancellationToken cancellationToken = default)
    {
        _context.TeachingSessionTopicCoverages.Update(coverage);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var coverage = await GetByIdAsync(id, cancellationToken);
        if (coverage != null)
        {
            _context.TeachingSessionTopicCoverages.Remove(coverage);
        }
    }
}
