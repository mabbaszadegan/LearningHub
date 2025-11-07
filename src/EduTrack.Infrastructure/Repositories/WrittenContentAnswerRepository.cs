using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for StudentAnswer entity (used for Written Content)
/// </summary>
public class WrittenContentAnswerRepository : Repository<StudentAnswer>, IWrittenContentAnswerRepository
{
    public WrittenContentAnswerRepository(AppDbContext context) : base(context)
    {
    }

    private static IQueryable<StudentAnswer> FilterByStudent(
        IQueryable<StudentAnswer> query,
        string studentId,
        int? studentProfileId)
    {
        query = query.Where(sa => sa.StudentId == studentId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(sa => sa.StudentProfileId == studentProfileId.Value);
        }

        return query;
    }

    public async Task<StudentAnswer?> GetAnswerByStudentAndScheduleItemAsync(string studentId, int scheduleItemId, int? studentProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(sa => sa.Student), studentId, studentProfileId);

        return await query.FirstOrDefaultAsync(sa => sa.InteractiveQuestionId == scheduleItemId, cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersByScheduleItemAsync(int scheduleItemId, int? studentProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(sa => sa.Student)
            .Where(sa => sa.InteractiveQuestionId == scheduleItemId);

        if (studentProfileId.HasValue)
        {
            query = query.Where(sa => sa.StudentProfileId == studentProfileId.Value);
        }

        return await query
            .OrderBy(sa => sa.AnsweredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersByStudentAsync(string studentId, int? studentProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = FilterByStudent(_dbSet.Include(sa => sa.InteractiveQuestion), studentId, studentProfileId);

        return await query
            .OrderByDescending(sa => sa.AnsweredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersNeedingGradingAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sa => sa.Student)
            .Include(sa => sa.InteractiveQuestion)
            .Where(sa => !sa.GradedAt.HasValue)
            .OrderBy(sa => sa.AnsweredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersByTeacherAsync(string teacherId, CancellationToken cancellationToken = default)
    {
        // Since we're using StudentAnswer for written content, we need to filter by ScheduleItem
        // For now, return all answers that need grading
        return await _dbSet
            .Include(sa => sa.Student)
            .Include(sa => sa.InteractiveQuestion)
            .Where(sa => !sa.GradedAt.HasValue)
            .OrderByDescending(sa => sa.AnsweredAt)
            .ToListAsync(cancellationToken);
    }
}
