using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

/// <summary>
/// Specific repository implementation for Question entity
/// </summary>
public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Question>> GetQuestionsByTypeAsync(QuestionType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.Type == type && q.IsActive)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Question>> GetActiveQuestionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.IsActive)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Question>> GetQuestionsByCreatorAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.CreatedBy == createdBy)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Question?> GetQuestionWithChoicesAsync(int questionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
    }
}
