using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EduTrack.Application.Features.TeachingSessions.CommandHandlers;

public class SaveStepCompletionCommandHandler : IRequestHandler<SaveStepCompletionCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveStepCompletionCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SaveStepCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Update session report step tracking
            sessionReport.CurrentStep = request.IsCompleted ? request.StepNumber + 1 : request.StepNumber;
            sessionReport.IsCompleted = request.IsCompleted && request.StepNumber >= 3; // Assuming 3 steps total
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در ذخیره مرحله: {ex.Message}");
        }
    }
}

public class SaveAttendanceStepCommandHandler : IRequestHandler<SaveAttendanceStepCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingSessionAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveAttendanceStepCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingSessionAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SaveAttendanceStepCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Get all existing attendance records for this session (once)
            var existingAttendance = await _attendanceRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

            // Collect all student IDs that will be updated
            var allStudentIds = request.AttendanceData.GroupAttendances
                .SelectMany(g => g.Students)
                .Select(s => s.StudentId)
                .ToHashSet();

            // Delete existing records only for students that will be updated
            var recordsToDelete = existingAttendance.Where(a => allStudentIds.Contains(a.StudentId)).ToList();
            foreach (var attendance in recordsToDelete)
            {
                await _attendanceRepository.DeleteAsync(attendance, cancellationToken);
            }

            // Add new attendance records for all groups
            foreach (var groupAttendance in request.AttendanceData.GroupAttendances)
            {
                foreach (var studentAttendance in groupAttendance.Students)
                {
                    var attendance = new TeachingSessionAttendance
                    {
                        TeachingSessionReportId = request.SessionId,
                        StudentId = studentAttendance.StudentId,
                        Status = studentAttendance.Status,
                        ParticipationScore = studentAttendance.ParticipationScore,
                        Comment = studentAttendance.Comment
                    };

                    await _attendanceRepository.AddAsync(attendance, cancellationToken);
                }
            }

            // Update session report
            // Update session report step tracking
            sessionReport.CurrentStep = 1;
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

public class SaveFeedbackStepCommandHandler : IRequestHandler<SaveFeedbackStepCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingSessionExecutionRepository _executionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveFeedbackStepCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingSessionExecutionRepository executionRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _executionRepository = executionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SaveFeedbackStepCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Get existing execution records for this session
            var existingExecutions = await _executionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
            // Collect all group IDs from the current request
            var requestGroupIds = request.FeedbackData.GroupFeedbacks.Select(gf => gf.GroupId).ToHashSet();
            
            // Delete only executions for groups that are in the current request
            var executionsToDelete = existingExecutions.Where(e => requestGroupIds.Contains(e.StudentGroupId)).ToList();
            foreach (var execution in executionsToDelete)
            {
                await _executionRepository.DeleteAsync(execution, cancellationToken);
            }

            // Add new execution records for the groups in the current request
            foreach (var groupFeedback in request.FeedbackData.GroupFeedbacks)
            {
                var execution = new TeachingSessionExecution
                {
                    TeachingSessionReportId = request.SessionId,
                    StudentGroupId = groupFeedback.GroupId,
                    GroupFeedback = groupFeedback.GroupFeedback,
                    UnderstandingLevel = groupFeedback.UnderstandingLevel,
                    ParticipationLevel = groupFeedback.ParticipationLevel,
                    TeacherSatisfaction = groupFeedback.TeacherSatisfaction,
                    Challenges = groupFeedback.Challenges,
                    NextSessionRecommendations = groupFeedback.NextSessionRecommendations,
                    CompletedAt = DateTimeOffset.UtcNow
                };

                await _executionRepository.AddAsync(execution, cancellationToken);
            }

            // Update session report
            // Update session report step tracking
            sessionReport.CurrentStep = 2; 
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در ذخیره بازخورد: {ex.Message}");
        }
    }
}

public class SaveTopicCoverageStepCommandHandler : IRequestHandler<SaveTopicCoverageStepCommand, Result<bool>>
{
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveTopicCoverageStepCommandHandler(
        ITeachingSessionReportRepository sessionReportRepository,
        ITeachingSessionTopicCoverageRepository topicCoverageRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionReportRepository = sessionReportRepository;
        _topicCoverageRepository = topicCoverageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(SaveTopicCoverageStepCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session report
            var sessionReport = await _sessionReportRepository.GetByIdAsync(request.SessionId, cancellationToken);
            if (sessionReport == null)
            {
                return Result<bool>.Failure("جلسه یافت نشد.");
            }

            // Clear existing topic coverage records for this session
            var existingCoverages = await _topicCoverageRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            foreach (var coverage in existingCoverages)
            {
                await _topicCoverageRepository.DeleteAsync(coverage, cancellationToken);
            }

            // Add new topic coverage records
            foreach (var groupCoverage in request.TopicCoverageData.GroupTopicCoverages)
            {
                // Add subtopic coverages
                foreach (var subtopicCoverage in groupCoverage.SubTopicCoverages)
                {
                    var coverage = new TeachingSessionTopicCoverage
                    {
                        TeachingSessionReportId = request.SessionId,
                        StudentGroupId = groupCoverage.GroupId,
                        TopicType = "SubTopic",
                        TopicId = subtopicCoverage.TopicId,
                        TopicTitle = subtopicCoverage.TopicTitle,
                        WasCovered = subtopicCoverage.WasCovered,
                        CoveragePercentage = subtopicCoverage.CoveragePercentage,
                        TeacherNotes = subtopicCoverage.TeacherNotes,
                        Challenges = subtopicCoverage.Challenges,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _topicCoverageRepository.AddAsync(coverage, cancellationToken);
                }

                // Add lesson coverages
                foreach (var lessonCoverage in groupCoverage.LessonCoverages)
                {
                    var coverage = new TeachingSessionTopicCoverage
                    {
                        TeachingSessionReportId = request.SessionId,
                        StudentGroupId = groupCoverage.GroupId,
                        TopicType = "Lesson",
                        TopicId = lessonCoverage.TopicId,
                        TopicTitle = lessonCoverage.TopicTitle,
                        WasCovered = lessonCoverage.WasCovered,
                        CoveragePercentage = lessonCoverage.CoveragePercentage,
                        TeacherNotes = lessonCoverage.TeacherNotes,
                        Challenges = lessonCoverage.Challenges,
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    await _topicCoverageRepository.AddAsync(coverage, cancellationToken);
                }
            }

            // Update session report
            // Update session report step tracking
            sessionReport.CurrentStep = 3; 
            sessionReport.IsCompleted = true;
            sessionReport.UpdatedAt = DateTimeOffset.UtcNow;

            await _sessionReportRepository.UpdateAsync(sessionReport, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"خطا در ذخیره پوشش موضوعات: {ex.Message}");
        }
    }
}

