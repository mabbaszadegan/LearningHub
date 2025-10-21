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

    public async Task<StudentAnswer?> GetAnswerByStudentAndScheduleItemAsync(string studentId, int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sa => sa.Student)
            .FirstOrDefaultAsync(sa => sa.StudentId == studentId && sa.InteractiveQuestionId == scheduleItemId, cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersByScheduleItemAsync(int scheduleItemId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sa => sa.Student)
            .Where(sa => sa.InteractiveQuestionId == scheduleItemId)
            .OrderBy(sa => sa.AnsweredAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentAnswer>> GetAnswersByStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(sa => sa.InteractiveQuestion)
            .Where(sa => sa.StudentId == studentId)
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
