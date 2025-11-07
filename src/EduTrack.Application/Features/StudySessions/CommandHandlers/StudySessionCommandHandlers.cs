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
            var studySession = StudySession.Create(request.StudentId, request.ScheduleItemId, request.StudentProfileId);
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
            StudentProfileId = studySession.StudentProfileId,
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
/// Handler for creating and completing a study session in one operation
/// </summary>
public class CreateAndCompleteStudySessionCommandHandler : IRequestHandler<CreateAndCompleteStudySessionCommand, Result<StudySessionDto>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public CreateAndCompleteStudySessionCommandHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<StudySessionDto>> Handle(CreateAndCompleteStudySessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if there's a previous completed session and validate that the new session's start time
            // is not before the last session's end time
            var lastEndedAt = await _studySessionRepository.GetLastEndedAtAsync(request.StudentId, request.ScheduleItemId, request.StudentProfileId);
            
            if (lastEndedAt.HasValue && request.StartedAt < lastEndedAt.Value)
            {
                return Result<StudySessionDto>.Failure(
                    $"زمان شروع جلسه جدید نمی‌تواند قبل از آخرین زمان پایان مطالعه ({lastEndedAt.Value:yyyy-MM-dd HH:mm:ss}) باشد. زمان شروع: {request.StartedAt:yyyy-MM-dd HH:mm:ss}");
            }

            // Create a completed study session directly
            var studySession = StudySession.CreateCompleted(request.StudentId, request.ScheduleItemId, request.StartedAt, request.EndedAt, request.StudentProfileId);
            var createdSession = await _studySessionRepository.AddAsync(studySession);

            return Result<StudySessionDto>.Success(MapToDto(createdSession));
        }
        catch (Exception ex)
        {
            return Result<StudySessionDto>.Failure($"خطا در ایجاد و تکمیل جلسه مطالعه: {ex.Message}");
        }
    }

    private static StudySessionDto MapToDto(StudySession studySession)
    {
        return new StudySessionDto
        {
            Id = studySession.Id,
            StudentId = studySession.StudentId,
            ScheduleItemId = studySession.ScheduleItemId,
            StudentProfileId = studySession.StudentProfileId,
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
/// Handler for deleting a study session
/// </summary>
public class DeleteStudySessionCommandHandler : IRequestHandler<DeleteStudySessionCommand, Result<bool>>
{
    private readonly IStudySessionRepository _studySessionRepository;

    public DeleteStudySessionCommandHandler(IStudySessionRepository studySessionRepository)
    {
        _studySessionRepository = studySessionRepository;
    }

    public async Task<Result<bool>> Handle(DeleteStudySessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var studySession = await _studySessionRepository.GetByIdAsync(request.StudySessionId);
            if (studySession == null)
            {
                return Result<bool>.Failure("جلسه مطالعه یافت نشد");
            }

            await _studySessionRepository.DeleteAsync(request.StudySessionId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در حذف جلسه مطالعه: {ex.Message}");
        }
    }
}