using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Progress entity
/// </summary>
public class ProgressRepository : Repository<Progress>, IProgressRepository
{
    public ProgressRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Progress>> GetProgressByStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Progress>> GetProgressByLessonAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.LessonId == lessonId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Progress>> GetProgressByExamAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.ExamId == examId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Progress?> GetStudentProgressForLessonAsync(string studentId, int lessonId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId, cancellationToken);
    }

    public async Task<Progress?> GetStudentProgressForExamAsync(string studentId, int examId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.ExamId == examId, cancellationToken);
    }

    public async Task<double> GetStudentOverallProgressAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var progresses = await _dbSet
            .Where(p => p.StudentId == studentId)
            .ToListAsync(cancellationToken);

        if (!progresses.Any())
            return 0.0;

        var completedCount = progresses.Count(p => p.IsCompleted);
        return (double)completedCount / progresses.Count * 100;
    }
}
