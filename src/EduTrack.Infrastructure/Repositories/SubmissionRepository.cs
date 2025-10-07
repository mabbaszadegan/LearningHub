using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Submission entity
/// </summary>
public class SubmissionRepository : Repository<Submission>, ISubmissionRepository
{
    public SubmissionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.StudentId == studentId)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .Include(s => s.Teacher)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByScheduleItemAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ScheduleItemId == scheduleItemId)
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .OrderBy(s => s.Student.FirstName)
            .ThenBy(s => s.Student.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByStatusAsync(SubmissionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == status)
            .Include(s => s.Student)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Submission?> GetSubmissionByStudentAndItemAsync(string studentId, int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.StudentId == studentId && s.ScheduleItemId == scheduleItemId, cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == SubmissionStatus.Submitted)
            .Include(s => s.Student)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .OrderBy(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByTeacherAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.TeacherId == teacherId)
            .Include(s => s.Student)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .OrderByDescending(s => s.ReviewedAt ?? s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
