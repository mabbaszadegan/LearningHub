using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using MediatR;

namespace EduTrack.Application.Features.InteractiveLesson.Queries;

public record GetInteractiveLessonsByCourseQuery(int CourseId) : IRequest<Result<List<InteractiveLessonDto>>>;

public record GetInteractiveLessonsByClassQuery(int ClassId) : IRequest<Result<List<InteractiveLessonDto>>>;

public record GetInteractiveLessonByIdQuery(int Id) : IRequest<Result<InteractiveLessonDto>>;

public record GetInteractiveLessonWithContentQuery(int Id) : IRequest<Result<InteractiveLessonWithContentDto>>;

public record GetAvailableEducationalContentQuery(int CourseId) : IRequest<Result<List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto>>>;

public record GetStudentAnswersQuery(int InteractiveQuestionId, string StudentId) : IRequest<Result<List<StudentAnswerDto>>>;

public record GetStudentProgressQuery(int InteractiveLessonId, string StudentId) : IRequest<Result<StudentProgressDto>>;

// Assignment Queries
public record GetAssignmentsByClassQuery(int ClassId) : IRequest<Result<List<InteractiveLessonAssignmentDto>>>;

public record GetAssignmentsByInteractiveLessonQuery(int InteractiveLessonId) : IRequest<Result<List<InteractiveLessonAssignmentDto>>>;

public record GetAvailableInteractiveLessonsForAssignmentQuery(int CourseId) : IRequest<Result<List<InteractiveLessonDto>>>;

public class InteractiveLessonWithContentDto
{
    public InteractiveLessonDto Lesson { get; set; } = null!;
    public List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto> AvailableContent { get; set; } = new();
}

public class StudentProgressDto
{
    public int InteractiveLessonId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int AnsweredQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalPoints { get; set; }
    public int EarnedPoints { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTimeOffset? LastAnsweredAt { get; set; }
}
