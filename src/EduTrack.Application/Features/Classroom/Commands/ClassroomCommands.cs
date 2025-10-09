using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Classroom.Commands;

public record CreateClassCommand(
    int CourseId,
    string Name,
    string? Description,
    string? TeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate) : IRequest<Result<ClassDto>>;

public record UpdateClassCommand(
    int Id,
    int CourseId,
    string Name,
    string? Description,
    string? TeacherId,
    DateTimeOffset StartDate,
    DateTimeOffset? EndDate,
    bool IsActive) : IRequest<Result<ClassDto>>;

public record DeleteClassCommand(int Id) : IRequest<Result<bool>>;

public record EnrollStudentCommand(
    int ClassId,
    string StudentId) : IRequest<Result<bool>>;

public record UnenrollStudentCommand(
    int ClassId,
    string StudentId) : IRequest<Result<bool>>;

public record CompleteEnrollmentCommand(
    int ClassId,
    string StudentId) : IRequest<Result<bool>>;
