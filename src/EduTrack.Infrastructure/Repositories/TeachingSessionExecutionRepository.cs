using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class TeachingSessionExecutionRepository : ITeachingSessionExecutionRepository
{
    private readonly AppDbContext _context;

    public TeachingSessionExecutionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingSessionExecution?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionExecutions
            .Include(e => e.TeachingSessionReport)
            .Include(e => e.StudentGroup)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionExecution>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionExecutions
            .Include(e => e.StudentGroup)
            .Where(e => e.TeachingSessionReportId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionExecution>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionExecutions
            .Include(e => e.TeachingSessionReport)
            .Where(e => e.StudentGroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TeachingSessionExecution execution, CancellationToken cancellationToken = default)
    {
        await _context.TeachingSessionExecutions.AddAsync(execution, cancellationToken);
    }

    public async Task UpdateAsync(TeachingSessionExecution execution, CancellationToken cancellationToken = default)
    {
        _context.TeachingSessionExecutions.Update(execution);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var execution = await GetByIdAsync(id, cancellationToken);
        if (execution != null)
        {
            _context.TeachingSessionExecutions.Remove(execution);
        }
    }
}
