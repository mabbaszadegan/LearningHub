using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.StudentProfiles.Commands;

public record ArchiveStudentProfileCommand(int ProfileId, string UserId) : IRequest<Result<StudentProfileDto>>;

public record RestoreStudentProfileCommand(int ProfileId, string UserId) : IRequest<Result<StudentProfileDto>>;

public class ArchiveStudentProfileCommandHandler : IRequestHandler<ArchiveStudentProfileCommand, Result<StudentProfileDto>>,
    IRequestHandler<RestoreStudentProfileCommand, Result<StudentProfileDto>>
{
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveStudentProfileCommandHandler(IStudentProfileRepository studentProfileRepository, IUnitOfWork unitOfWork)
    {
        _studentProfileRepository = studentProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentProfileDto>> Handle(ArchiveStudentProfileCommand request, CancellationToken cancellationToken)
        => await UpdateStateAsync(request.ProfileId, request.UserId, profile => profile.Archive(), cancellationToken);

    public async Task<Result<StudentProfileDto>> Handle(RestoreStudentProfileCommand request, CancellationToken cancellationToken)
        => await UpdateStateAsync(request.ProfileId, request.UserId, profile => profile.Restore(), cancellationToken);

    private async Task<Result<StudentProfileDto>> UpdateStateAsync(int profileId, string userId, Action<Domain.Entities.StudentProfile> mutation, CancellationToken cancellationToken)
    {
        var profile = await _studentProfileRepository.GetByIdForUserAsync(profileId, userId, cancellationToken);
        if (profile == null)
        {
            return Result<StudentProfileDto>.Failure("پروفایل مورد نظر یافت نشد.");
        }

        mutation(profile);

        await _studentProfileRepository.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StudentProfileDto>.Success(StudentProfileMapping.ToDto(profile));
    }
}


