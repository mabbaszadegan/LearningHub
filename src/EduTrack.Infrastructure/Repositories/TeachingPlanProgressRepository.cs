using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class TeachingPlanProgressRepository : ITeachingPlanProgressRepository
{
    private readonly AppDbContext _context;

    public TeachingPlanProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingPlanProgress?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingPlanProgresses
            .Include(p => p.TeachingPlan)
            .Include(p => p.SubTopic)
            .Include(p => p.StudentGroup)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<TeachingPlanProgress?> GetByPlanSubTopicAndGroupAsync(int planId, int subtopicId, int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingPlanProgresses
            .Include(p => p.TeachingPlan)
            .Include(p => p.SubTopic)
            .Include(p => p.StudentGroup)
            .FirstOrDefaultAsync(p => p.TeachingPlanId == planId && p.SubTopicId == subtopicId && p.StudentGroupId == groupId, cancellationToken);
    }

    public async Task<IEnumerable<TeachingPlanProgress>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingPlanProgresses
            .Include(p => p.SubTopic)
            .Include(p => p.StudentGroup)
            .Where(p => p.TeachingPlanId == planId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingPlanProgress>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingPlanProgresses
            .Include(p => p.TeachingPlan)
            .Include(p => p.SubTopic)
            .Where(p => p.StudentGroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingPlanProgress>> GetBySubTopicIdAsync(int subtopicId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingPlanProgresses
            .Include(p => p.TeachingPlan)
            .Include(p => p.StudentGroup)
            .Where(p => p.SubTopicId == subtopicId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TeachingPlanProgress progress, CancellationToken cancellationToken = default)
    {
        await _context.TeachingPlanProgresses.AddAsync(progress, cancellationToken);
    }

    public async Task UpdateAsync(TeachingPlanProgress progress, CancellationToken cancellationToken = default)
    {
        _context.TeachingPlanProgresses.Update(progress);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var progress = await GetByIdAsync(id, cancellationToken);
        if (progress != null)
        {
            _context.TeachingPlanProgresses.Remove(progress);
        }
    }
}
