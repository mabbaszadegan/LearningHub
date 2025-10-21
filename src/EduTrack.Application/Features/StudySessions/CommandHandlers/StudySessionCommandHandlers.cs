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
            // Create a completed study session directly
            var studySession = StudySession.CreateCompleted(request.StudentId, request.ScheduleItemId, request.StartedAt, request.EndedAt);
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