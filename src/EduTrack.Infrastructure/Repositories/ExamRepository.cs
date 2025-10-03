using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Exam entity
/// </summary>
public class ExamRepository : Repository<Exam>, IExamRepository
{
    public ExamRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Exam>> GetActiveExamsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Exam>> GetExamsByCreatorAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CreatedBy == createdBy)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Exam?> GetExamWithQuestionsAsync(int examId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.ExamQuestions)
                .ThenInclude(eq => eq.Question)
            .FirstOrDefaultAsync(e => e.Id == examId, cancellationToken);
    }

    public async Task<bool> TitleExistsAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(e => e.Title == title, cancellationToken);
    }
}
