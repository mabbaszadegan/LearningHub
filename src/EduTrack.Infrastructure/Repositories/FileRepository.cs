using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class FileRepository : Repository<Domain.Entities.File>, IFileRepository
{
    public FileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Domain.Entities.File?> GetByMD5HashAsync(string md5Hash, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.MD5Hash == md5Hash, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.File>> GetFilesByCreatorAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.CreatedBy == createdBy)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.File>> GetUnreferencedFilesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.ReferenceCount == 0)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
