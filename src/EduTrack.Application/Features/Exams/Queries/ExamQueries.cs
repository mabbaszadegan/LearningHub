using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.Exams.Queries;

public record GetExamsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsActive = null) : IRequest<PaginatedList<ExamDto>>;

public record GetExamByIdQuery(int Id) : IRequest<Result<ExamDto>>;

public record GetQuestionsByExamIdQuery(int ExamId) : IRequest<Result<List<QuestionDto>>>;

public record GetQuestionsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsActive = null) : IRequest<PaginatedList<QuestionDto>>;

public record GetQuestionByIdQuery(int Id) : IRequest<Result<QuestionDto>>;

public record GetAttemptByIdQuery(int Id) : IRequest<Result<AttemptDto>>;

public record GetAttemptsByStudentQuery(
    string StudentId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<AttemptDto>>;

public record GetAttemptsByExamQuery(
    int ExamId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PaginatedList<AttemptDto>>;
