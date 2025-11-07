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

    private static IQueryable<Submission> FilterByStudent(
        IQueryable<Submission> query,
        string studentId,
        int? studentProfileId)
    {
        query = query.Where(s => s.StudentId == studentId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(s => s.StudentProfileId == studentProfileId.Value);
        }

        return query;
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(string studentId, int? studentProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.AsQueryable(), studentId, studentProfileId)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .Include(s => s.Teacher)
            .Include(s => s.StudentProfile)
            .OrderByDescending(s => s.UpdatedAt);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByScheduleItemAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.ScheduleItemId == scheduleItemId)
            .Include(s => s.Student)
            .Include(s => s.StudentProfile)
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
            .Include(s => s.StudentProfile)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Submission?> GetSubmissionByStudentAndItemAsync(string studentId, int scheduleItemId, int? studentProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.AsQueryable(), studentId, studentProfileId)
            .Where(s => s.ScheduleItemId == scheduleItemId)
            .Include(s => s.ScheduleItem)
                .ThenInclude(si => si.TeachingPlan)
                .ThenInclude(tp => tp.Course)
            .Include(s => s.Student)
            .Include(s => s.Teacher)
            .Include(s => s.StudentProfile);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.Status == SubmissionStatus.Submitted)
            .Include(s => s.Student)
            .Include(s => s.StudentProfile)
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
            .Include(s => s.StudentProfile)
            .Include(s => s.ScheduleItem)
            .ThenInclude(si => si.TeachingPlan)
            .ThenInclude(tp => tp.Course)
            .OrderByDescending(s => s.ReviewedAt ?? s.SubmittedAt)
            .ToListAsync(cancellationToken);
    }
}
