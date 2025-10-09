using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class SubChapterRepository : ISubChapterRepository
{
    private readonly AppDbContext _context;

    public SubChapterRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SubChapter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.SubChapters
            .Include(s => s.Chapter)
            .Include(s => s.EducationalContents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SubChapter>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken = default)
    {
        return await _context.SubChapters
            .Include(s => s.EducationalContents)
            .Where(s => s.ChapterId == chapterId && s.IsActive)
            .OrderBy(s => s.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SubChapter>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.SubChapters
            .Include(s => s.Chapter)
            .Include(s => s.EducationalContents)
            .Where(s => s.Chapter.CourseId == courseId && s.IsActive)
            .OrderBy(s => s.Chapter.Order)
            .ThenBy(s => s.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SubChapter subChapter, CancellationToken cancellationToken = default)
    {
        await _context.SubChapters.AddAsync(subChapter, cancellationToken);
    }

    public async Task UpdateAsync(SubChapter subChapter, CancellationToken cancellationToken = default)
    {
        _context.SubChapters.Update(subChapter);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subChapter = await GetByIdAsync(id, cancellationToken);
        if (subChapter != null)
        {
            _context.SubChapters.Remove(subChapter);
        }
    }
}
