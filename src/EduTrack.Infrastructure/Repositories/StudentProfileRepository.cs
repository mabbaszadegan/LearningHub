using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using EduTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Infrastructure.Repositories;

public class StudentProfileRepository : Repository<StudentProfile>, IStudentProfileRepository
{
    public StudentProfileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StudentProfile>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(profile => profile.UserId == userId)
            .OrderBy(profile => profile.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentProfile?> GetByIdForUserAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(profile => profile.Id == id && profile.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsWithDisplayNameAsync(string userId, string displayName, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            profile => profile.UserId == userId && profile.DisplayName == displayName,
            cancellationToken);
    }
}


