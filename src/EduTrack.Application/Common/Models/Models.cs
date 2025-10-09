using EduTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Items = items;
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public object? Data { get; }

    private Result(bool isSuccess, string? error, object? data = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Data = data;
    }

    public static Result Success() => new(true, null);
    public static Result Success(object data) => new(true, null, data);
    public static Result Failure(string error) => new(false, error);
}

public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DisciplineType DisciplineType { get; set; }
    public int ModuleCount { get; set; }
    public int LessonCount { get; set; }
    public int ChapterCount { get; set; }
    public int ClassCount { get; set; }
    public List<ModuleDto> Modules { get; set; } = new();
    public List<ChapterDto> Chapters { get; set; } = new();
}

public class ModuleDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int LessonCount { get; set; }
    public List<LessonDto> Lessons { get; set; } = new();
}

public class LessonDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<ResourceDto> Resources { get; set; } = new();
    public ProgressStatus? ProgressStatus { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
}

public class ResourceDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourceType Type { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? Url { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ClassDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int StudentCount { get; set; }
}

public class ExamDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool ShowSolutions { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<ChoiceDto> Choices { get; set; } = new();
}

public class ChoiceDto
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

public class AttemptDto
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? Score { get; set; }
    public int? TotalQuestions { get; set; }
    public int? CorrectAnswers { get; set; }
    public bool IsPassed { get; set; }
    public TimeSpan? Duration { get; set; }
}

public class ProgressDto
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public int? ExamId { get; set; }
    public string? ExamTitle { get; set; }
    public ProgressStatus Status { get; set; }
    public int CorrectCount { get; set; }
    public int Streak { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class ClassSummaryDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int CompletedStudents { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public class ChapterDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int SubChapterCount { get; set; }
    public List<SubChapterDto> SubChapters { get; set; } = new();
}

public class SubChapterDto
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int ContentCount { get; set; }
    public List<EducationalContentDto> EducationalContents { get; set; } = new();
}

public class FileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string MD5Hash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int ReferenceCount { get; set; }
    public string? FileUrl { get; set; }
}

public class EducationalContentDto
{
    public int Id { get; set; }
    public int SubChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EducationalContentType Type { get; set; }
    public string? TextContent { get; set; }
    public int? FileId { get; set; }
    public string? ExternalUrl { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public FileDto? File { get; set; }
}

public class DashboardViewModel
{
    public PaginatedList<CourseDto> Courses { get; set; } = null!;
    public PaginatedList<ExamDto> RecentExams { get; set; } = null!;
    public PaginatedList<ProgressDto>? UserProgress { get; set; }
    public int TotalCourses { get; set; }
    public int TotalLessons { get; set; }
    public int TotalModules { get; set; }
    public bool IsUserLoggedIn { get; set; }
    public int CompletedCourses { get; set; }
    public int CompletedLessons { get; set; }
    public int CompletedExams { get; set; }
    public double OverallProgress { get; set; }
}

// Teaching Plan DTOs
public class TeachingPlanDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Objectives { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int GroupCount { get; set; }
    public int ScheduleItemCount { get; set; }
    public int TotalStudents { get; set; }
    public List<StudentGroupDto> Groups { get; set; } = new();
    public List<ScheduleItemDto> ScheduleItems { get; set; } = new();
}

public class StudentGroupDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
}

public class ScheduleItemDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public int? LessonId { get; set; }
    public string? LessonTitle { get; set; }
    public ScheduleItemType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsMandatory { get; set; }
    public DisciplineType? DisciplineHint { get; set; }
    public string ContentJson { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsActive { get; set; }
    public TimeSpan? TimeUntilDue { get; set; }
    public int SubmissionCount { get; set; }
    public int CompletedSubmissions { get; set; }
}

public class SubmissionDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public string ScheduleItemTitle { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTimeOffset? SubmittedAt { get; set; }
    public SubmissionStatus Status { get; set; }
    public decimal? Grade { get; set; }
    public string? FeedbackText { get; set; }
    public string? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public string? AttachmentsJson { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public decimal? PercentageScore { get; set; }
    public bool IsPassing { get; set; }
    public bool IsOverdue { get; set; }
}

public class StudentAgendaDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public LearningMode LearningMode { get; set; }
    public List<ScheduleItemDto> UpcomingItems { get; set; } = new();
    public List<ScheduleItemDto> InProgressItems { get; set; } = new();
    public List<ScheduleItemDto> OverdueItems { get; set; } = new();
    public List<ScheduleItemDto> CompletedItems { get; set; } = new();
    public int TotalItems { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public double CompletionPercentage { get; set; }
}

public class GroupProgressDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int TotalScheduleItems { get; set; }
    public int CompletedSubmissions { get; set; }
    public int OverdueSubmissions { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public List<StudentProgressDto> StudentProgress { get; set; } = new();
}

public class StudentProgressDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int CompletedSubmissions { get; set; }
    public int TotalSubmissions { get; set; }
    public double CompletionPercentage { get; set; }
    public double AverageScore { get; set; }
    public int OverdueCount { get; set; }
    public DateTimeOffset? LastActivity { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class TeachingSessionReportDto
{
    public int Id { get; set; }
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTimeOffset SessionDate { get; set; }
    public SessionMode Mode { get; set; }
    public string? Location { get; set; }
    public string TopicsJson { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? StatsJson { get; set; }
    public string? AttachmentsJson { get; set; }
    public string CreatedByTeacherId { get; set; } = string.Empty;
    public string CreatedByTeacherName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int AttendanceCount { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public List<TeachingSessionAttendanceDto> Attendance { get; set; } = new();
}

public class TeachingSessionAttendanceDto
{
    public int Id { get; set; }
    public int TeachingSessionReportId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }
}

public class ScheduleItemAssignmentDto
{
    public int Id { get; set; }
    public int ScheduleItemId { get; set; }
    public int? StudentId { get; set; }
    public string? StudentName { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}

public class SubTopicDto
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Objective { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Order { get; set; }
    public string ChapterTitle { get; set; } = string.Empty;
}

public class GroupDataDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public GroupFeedbackDto? ExistingFeedback { get; set; }
}

public class GroupMemberDto
{
    public int Id { get; set; }
    public int StudentGroupId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public StudentAttendanceDto? ExistingAttendance { get; set; }
}

public class SessionCompletionDataDto
{
    public int SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTimeOffset SessionDate { get; set; }
    public int TeachingPlanId { get; set; }
    public string TeachingPlanTitle { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public bool HasPlan { get; set; }
    public List<GroupDataDto> Groups { get; set; } = new();
    public List<SubTopicDto> AvailableSubTopics { get; set; } = new();
    public List<LessonDto> AvailableLessons { get; set; } = new();
    public List<PlannedItemDto>? PlannedItems { get; set; }
    public SessionCompletionProgressDto? CompletionProgress { get; set; }
}

public class SessionCompletionProgressDto
{
    public int SessionId { get; set; }
    public int CurrentStep { get; set; }
    public bool IsCompleted { get; set; }
    public List<StepCompletionDto> Steps { get; set; } = new();
}

public class StepCompletionDto
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepTitle { get; set; } = string.Empty;
    public string StepDescription { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CompletionData { get; set; } // JSON data for this step
}

public class StepCompletionDataDto
{
    public int SessionId { get; set; }
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string CompletionData { get; set; } = string.Empty; // JSON data
    public bool IsCompleted { get; set; }
}

public class TabCompletionDataDto
{
    public int SessionId { get; set; }
    public int GroupId { get; set; }
    public int TabIndex { get; set; }
    public string CompletionData { get; set; } = string.Empty; // JSON data
    public bool IsCompleted { get; set; }
}

public class UpdateAttendanceRequest
{
    public int SessionId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int AttendanceId { get; set; }
    public int Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }
}

// Step-specific data models
public class AttendanceStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupAttendanceDto> GroupAttendances { get; set; } = new();
}

public class GroupAttendanceDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<StudentAttendanceDto> Students { get; set; } = new();
}

public class StudentAttendanceDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public decimal? ParticipationScore { get; set; }
    public string? Comment { get; set; }
}

public class FeedbackStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupFeedbackDto> GroupFeedbacks { get; set; } = new();
}

public class GroupFeedbackDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public int UnderstandingLevel { get; set; }
    public int ParticipationLevel { get; set; }
    public int TeacherSatisfaction { get; set; }
    public string? GroupFeedback { get; set; }
    public string? Challenges { get; set; }
    public string? NextSessionRecommendations { get; set; }
}

public class TopicCoverageStepDataDto
{
    public int SessionId { get; set; }
    public List<GroupTopicCoverageDto> GroupTopicCoverages { get; set; } = new();
}

public class GroupTopicCoverageDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public List<TopicCoverageItemDto> SubTopicCoverages { get; set; } = new();
    public List<TopicCoverageItemDto> LessonCoverages { get; set; } = new();
}

public class TopicCoverageItemDto
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public bool WasPlanned { get; set; }
    public bool WasCovered { get; set; }
    public int CoveragePercentage { get; set; }
    public string? TeacherNotes { get; set; }
    public string? Challenges { get; set; }
}

public class PlannedItemDto
{
    public int StudentGroupId { get; set; }
    public string? PlannedObjectives { get; set; }
    public List<int> PlannedSubTopics { get; set; } = new();
    public List<int> PlannedLessons { get; set; } = new();
    public string? AdditionalTopics { get; set; }
}
