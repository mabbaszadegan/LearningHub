using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class TeachingSessionReportRepository : ITeachingSessionReportRepository
{
    private readonly AppDbContext _context;

    public TeachingSessionReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingSessionReport?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionReports
            .Include(r => r.TeachingPlan)
            .Include(r => r.Attendance)
                .ThenInclude(a => a.Student)
            .Include(r => r.Plans)
            .Include(r => r.Executions)
            .Include(r => r.TopicCoverages)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionReport>> GetByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionReports
            .Include(r => r.Attendance)
            .Where(r => r.TeachingPlanId == planId)
            .OrderByDescending(r => r.SessionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionReport>> GetByTeacherIdAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionReports
            .Include(r => r.TeachingPlan)
            .Include(r => r.Attendance)
            .Where(r => r.CreatedByTeacherId == teacherId)
            .OrderByDescending(r => r.SessionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TeachingSessionReport report, CancellationToken cancellationToken = default)
    {
        await _context.TeachingSessionReports.AddAsync(report, cancellationToken);
    }

    public async Task UpdateAsync(TeachingSessionReport report, CancellationToken cancellationToken = default)
    {
        _context.TeachingSessionReports.Update(report);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var report = await GetByIdAsync(id, cancellationToken);
        if (report != null)
        {
            _context.TeachingSessionReports.Remove(report);
        }
    }

    public async Task<bool> HasPlanningDataAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionPlans
            .AnyAsync(p => p.TeachingSessionReportId == sessionId, cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionPlan>> GetPlannedItemsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionPlans
            .Include(p => p.StudentGroup)
            .Where(p => p.TeachingSessionReportId == sessionId)
            .ToListAsync(cancellationToken);
    }
}
