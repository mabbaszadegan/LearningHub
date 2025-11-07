using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.StudentProfiles.Queries;

public record GetStudentProfilesQuery(string UserId, bool IncludeArchived = false) : IRequest<Result<List<StudentProfileDto>>>;

public record GetStudentProfileByIdQuery(int ProfileId, string UserId) : IRequest<Result<StudentProfileDto>>;

public class GetStudentProfilesQueryHandler :
    IRequestHandler<GetStudentProfilesQuery, Result<List<StudentProfileDto>>>,
    IRequestHandler<GetStudentProfileByIdQuery, Result<StudentProfileDto>>
{
    private readonly IStudentProfileRepository _studentProfileRepository;

    public GetStudentProfilesQueryHandler(IStudentProfileRepository studentProfileRepository)
    {
        _studentProfileRepository = studentProfileRepository;
    }

    public async Task<Result<List<StudentProfileDto>>> Handle(GetStudentProfilesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<List<StudentProfileDto>>.Failure("شناسه کاربر مشخص نشده است.");
        }

        var profiles = await _studentProfileRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (!request.IncludeArchived)
        {
            profiles = profiles.Where(p => !p.IsArchived);
        }

        var dtos = profiles
            .OrderBy(p => p.DisplayName)
            .Select(StudentProfileMapping.ToDto)
            .ToList();

        return Result<List<StudentProfileDto>>.Success(dtos);
    }

    public async Task<Result<StudentProfileDto>> Handle(GetStudentProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _studentProfileRepository.GetByIdForUserAsync(request.ProfileId, request.UserId, cancellationToken);
        if (profile == null)
        {
            return Result<StudentProfileDto>.Failure("پروفایل یافت نشد.");
        }

        return Result<StudentProfileDto>.Success(StudentProfileMapping.ToDto(profile));
    }
}

