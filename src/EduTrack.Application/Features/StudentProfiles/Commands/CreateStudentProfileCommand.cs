using EduTrack.Application.Common.Models;
using EduTrack.Domain.Repositories;
using EduTrack.Domain.Entities;
using MediatR;

namespace EduTrack.Application.Features.StudentProfiles.Commands;

public record CreateStudentProfileCommand(
    string UserId,
    string DisplayName,
    string? GradeLevel = null,
    DateTimeOffset? DateOfBirth = null,
    string? Notes = null,
    string? AvatarUrl = null
) : IRequest<Result<StudentProfileDto>>;

public class CreateStudentProfileCommandHandler : IRequestHandler<CreateStudentProfileCommand, Result<StudentProfileDto>>
{
    private readonly IStudentProfileRepository _studentProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateStudentProfileCommandHandler(
        IStudentProfileRepository studentProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _studentProfileRepository = studentProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<StudentProfileDto>> Handle(CreateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<StudentProfileDto>.Failure("شناسه کاربر مشخص نشده است.");
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Result<StudentProfileDto>.Failure("نام نمایشی لازم است.");
        }

        var exists = await _studentProfileRepository.ExistsWithDisplayNameAsync(request.UserId, request.DisplayName.Trim(), cancellationToken);
        if (exists)
        {
            return Result<StudentProfileDto>.Failure("پروفایلی با این نام برای این کاربر وجود دارد.");
        }

        var profile = StudentProfile.Create(
            request.UserId,
            request.DisplayName,
            request.GradeLevel,
            request.DateOfBirth);

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            profile.UpdateNotes(request.Notes);
        }

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
        {
            profile.UpdateAvatar(request.AvatarUrl);
        }

        await _studentProfileRepository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<StudentProfileDto>.Success(StudentProfileMapping.ToDto(profile));
    }
}

