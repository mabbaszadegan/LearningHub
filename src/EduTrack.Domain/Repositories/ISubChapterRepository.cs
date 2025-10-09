using EduTrack.Domain.Entities;

namespace EduTrack.Domain.Repositories;

public interface ISubChapterRepository
{
    Task<SubChapter?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubChapter>> GetByChapterIdAsync(int chapterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubChapter>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task AddAsync(SubChapter subChapter, CancellationToken cancellationToken = default);
    Task UpdateAsync(SubChapter subChapter, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
