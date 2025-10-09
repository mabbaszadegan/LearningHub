using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lesson>> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
    Task UpdateAsync(Lesson lesson, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
