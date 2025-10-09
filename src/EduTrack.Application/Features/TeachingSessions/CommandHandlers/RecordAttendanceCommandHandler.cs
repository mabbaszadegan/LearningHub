using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class RecordAttendanceCommandHandler : IRequestHandler<RecordAttendanceCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingSessionAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordAttendanceCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingSessionAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.TeachingSessionReportId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Process each attendance record
            foreach (var attendanceDto in request.Attendance)
            {
                if (attendanceDto.Id > 0)
                {
                    // Update existing record
                    var existingAttendance = await _attendanceRepository.GetByIdAsync(attendanceDto.Id, cancellationToken);
                    if (existingAttendance != null)
                    {
                        existingAttendance.Status = attendanceDto.Status;
                        existingAttendance.ParticipationScore = attendanceDto.ParticipationScore;
                        existingAttendance.Comment = attendanceDto.Comment;
                        
                        await _attendanceRepository.UpdateAsync(existingAttendance, cancellationToken);
                    }
                }
                else
                {
                    // Create new record
                    var newAttendance = new TeachingSessionAttendance
                    {
                        TeachingSessionReportId = request.TeachingSessionReportId,
                        StudentId = attendanceDto.StudentId,
                        Status = attendanceDto.Status,
                        ParticipationScore = attendanceDto.ParticipationScore,
                        Comment = attendanceDto.Comment
                    };

                    await _attendanceRepository.AddAsync(newAttendance, cancellationToken);
                }
            }

            // Update session report timestamp
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;
            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در ذخیره حضور و غیاب: {ex.Message}");
        }
    }
}
