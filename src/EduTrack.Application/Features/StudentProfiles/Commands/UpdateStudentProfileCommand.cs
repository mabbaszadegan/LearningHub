using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.StudentProfiles.Commands;

public record UpdateStudentProfileCommand(
    int ProfileId,
    string UserId,
    string DisplayName,
    string? GradeLevel = null,
    DateTimeOffset? DateOfBirth = null,
    string? Notes = null,
    string? AvatarUrl = null
) : IRequest<Result<StudentProfileDto>>;

public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand, Result<StudentProfileDto>>
{
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentProfileCommandHandler(
        IStudentProfileRepository studentProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _studentProfileRepository = studentProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentProfileDto>> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _studentProfileRepository.GetByIdForUserAsync(request.ProfileId, request.UserId, cancellationToken);
        if (profile == null)
        {
            return Result<StudentProfileDto>.Failure("پروفایل مورد نظر یافت نشد.");
        }

        var trimmedDisplayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedDisplayName))
        {
            return Result<StudentProfileDto>.Failure("نام نمایشی لازم است.");
        }

        if (!trimmedDisplayName.Equals(profile.DisplayName, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _studentProfileRepository.ExistsWithDisplayNameAsync(request.UserId, trimmedDisplayName, cancellationToken);
            if (exists)
            {
                return Result<StudentProfileDto>.Failure("پروفایلی با این نام برای این کاربر وجود دارد.");
            }
        }

        profile.UpdateProfile(trimmedDisplayName, request.GradeLevel, request.DateOfBirth);
        profile.UpdateNotes(request.Notes);
        profile.UpdateAvatar(request.AvatarUrl);

        await _studentProfileRepository.UpdateAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StudentProfileDto>.Success(StudentProfileMapping.ToDto(profile));
    }
}

