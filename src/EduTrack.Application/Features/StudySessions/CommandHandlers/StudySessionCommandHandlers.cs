using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.StudySessions;
using EduTrack.Application.Features.StudySessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.StudySessions.CommandHandlers;

/// <summary>
/// Handler for starting a new study session
/// </summary>
public class StartStudySessionCommandHandler : IRequestHandler<StartStudySessionCommand, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public StartStudySessionCommandHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(StartStudySessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if there's already an active session for this student and content
            var activeSession = await _studySessionRepository.GetActiveSessionAsync(request.StudentId, request.EducationalContentId);
            if (activeSession != null)
            {
                return Result<StudySessionDto>.Success(MapToDto(activeSession));
            }

            // Create new study session
            var studySession = StudySession.Create(request.StudentId, request.EducationalContentId);
            var createdSession = await _studySessionRepository.AddAsync(studySession);

            return Result<StudySessionDto>.Success(MapToDto(createdSession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در شروع جلسه مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            EducationalContentId = studySession.EducationalContentId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for completing a study session
/// </summary>
public class CompleteStudySessionCommandHandler : IRequestHandler<CompleteStudySessionCommand, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public CompleteStudySessionCommandHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(CompleteStudySessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var studySession = await _studySessionRepository.GetByIdAsync(request.StudySessionId);
            if (studySession == null)
            {
                return Result<StudySessionDto>.Failure("جلسه مطالعه یافت نشد");
            }

            if (studySession.IsCompleted)
            {
                return Result<StudySessionDto>.Failure("جلسه مطالعه قبلاً تکمیل شده است");
            }

            studySession.Complete(request.DurationSeconds);
            await _studySessionRepository.UpdateAsync(studySession);

            return Result<StudySessionDto>.Success(MapToDto(studySession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در تکمیل جلسه مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            EducationalContentId = studySession.EducationalContentId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for updating study session duration
/// </summary>
public class UpdateStudySessionDurationCommandHandler : IRequestHandler<UpdateStudySessionDurationCommand, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public UpdateStudySessionDurationCommandHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(UpdateStudySessionDurationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var studySession = await _studySessionRepository.GetByIdAsync(request.StudySessionId);
            if (studySession == null)
            {
                return Result<StudySessionDto>.Failure("جلسه مطالعه یافت نشد");
            }

            if (studySession.IsCompleted)
            {
                return Result<StudySessionDto>.Failure("نمی‌توان مدت زمان جلسه تکمیل شده را تغییر داد");
            }

            studySession.UpdateDuration(request.DurationSeconds);
            await _studySessionRepository.UpdateAsync(studySession);

            return Result<StudySessionDto>.Success(MapToDto(studySession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در به‌روزرسانی مدت زمان جلسه مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            EducationalContentId = studySession.EducationalContentId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}
