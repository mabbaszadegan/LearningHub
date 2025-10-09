using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Commands;

// Base step completion command
public record SaveStepCompletionCommand(
    int SessionId,
    int StepNumber,
    string StepName,
    string CompletionData,
    bool IsCompleted) : IRequest<Result<bool>>;

// Step 1: Attendance
public record SaveAttendanceStepCommand(
    int SessionId,
    AttendanceStepDataDto AttendanceData) : IRequest<Result<bool>>;

// Step 2: Feedback
public record SaveFeedbackStepCommand(
    int SessionId,
    FeedbackStepDataDto FeedbackData) : IRequest<Result<bool>>;

// Step 3: Topic Coverage
public record SaveTopicCoverageStepCommand(
    int SessionId,
    TopicCoverageStepDataDto TopicCoverageData) : IRequest<Result<bool>>;


// Get completion progress
public record GetSessionCompletionProgressQuery(int SessionId) : IRequest<Result<SessionCompletionProgressDto>>;

// Get step data
public record GetStepCompletionDataQuery(int SessionId, int StepNumber) : IRequest<Result<StepCompletionDataDto>>;
