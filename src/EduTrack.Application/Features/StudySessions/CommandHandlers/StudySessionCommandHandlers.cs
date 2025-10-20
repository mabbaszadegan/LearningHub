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
            // Always create a new study session - no need to check for active sessions
            var studySession = StudySession.Create(request.StudentId, request.ScheduleItemId);
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
            ScheduleItemId = studySession.ScheduleItemId,
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

            // Complete the session - duration will be calculated automatically
            studySession.Complete();
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
            ScheduleItemId = studySession.ScheduleItemId,
            StartedAt = studySession.StartedAt,
            EndedAt = studySession.EndedAt,
            DurationSeconds = studySession.DurationSeconds,
            IsCompleted = studySession.IsCompleted,
            CreatedAt = studySession.CreatedAt,
            UpdatedAt = studySession.UpdatedAt
        };
    }
}
