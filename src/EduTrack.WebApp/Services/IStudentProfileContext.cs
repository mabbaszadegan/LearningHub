using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EduTrack.Application.Features.StudentProfiles;

namespace EduTrack.WebApp.Services;

public interface IStudentProfileContext
{
    Task<int?> GetActiveProfileIdAsync(CancellationToken cancellationToken = default);
    Task<string?> GetActiveProfileNameAsync(CancellationToken cancellationToken = default);
    Task SetActiveProfileAsync(int? profileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentProfileDto>> GetProfilesForCurrentUserAsync(bool includeArchived = false, CancellationToken cancellationToken = default);
}

