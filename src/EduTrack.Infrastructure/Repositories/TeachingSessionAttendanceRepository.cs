using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class TeachingSessionAttendanceRepository : ITeachingSessionAttendanceRepository
{
    private readonly AppDbContext _context;

    public TeachingSessionAttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeachingSessionAttendance?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionAttendances
            .Include(a => a.TeachingSessionReport)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionAttendance>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionAttendances
            .Where(a => a.TeachingSessionReportId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TeachingSessionAttendance>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _context.TeachingSessionAttendances
            .Include(a => a.TeachingSessionReport)
            .Where(a => a.StudentId == studentId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default)
    {
        await _context.TeachingSessionAttendances.AddAsync(attendance, cancellationToken);
    }

    public async Task UpdateAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default)
    {
        _context.TeachingSessionAttendances.Update(attendance);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(TeachingSessionAttendance attendance, CancellationToken cancellationToken = default)
    {
        _context.TeachingSessionAttendances.Remove(attendance);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var attendance = await GetByIdAsync(id, cancellationToken);
        if (attendance != null)
        {
            _context.TeachingSessionAttendances.Remove(attendance);
        }
    }
}
