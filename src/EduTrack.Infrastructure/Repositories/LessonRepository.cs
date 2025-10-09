using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;

    public LessonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Include(l => l.Module)
            .Include(l => l.Resources)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Lesson>> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Include(l => l.Resources)
            .Where(l => l.ModuleId == moduleId && l.IsActive)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Include(l => l.Module)
            .Include(l => l.Resources)
            .Where(l => l.Module.CourseId == courseId && l.IsActive)
            .OrderBy(l => l.Module.Order)
            .ThenBy(l => l.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _context.Lessons.AddAsync(lesson, cancellationToken);
    }

    public async Task UpdateAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        _context.Lessons.Update(lesson);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await GetByIdAsync(id, cancellationToken);
        if (lesson != null)
        {
            _context.Lessons.Remove(lesson);
        }
    }
}
