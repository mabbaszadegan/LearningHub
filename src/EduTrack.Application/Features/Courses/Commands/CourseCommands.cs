using EduTrack.Application.Common.Models;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public record CreateCourseCommand(
    string Title,
    string? Description,
    string? Thumbnail,
    int Order) : IRequest<Result<CourseDto>>;

public record UpdateCourseCommand(
    int Id,
    string Title,
    string? Description,
    string? Thumbnail,
    bool IsActive,
    int Order) : IRequest<Result<CourseDto>>;

public record DeleteCourseCommand(int Id) : IRequest<Result<bool>>;

public record CreateModuleCommand(
    int CourseId,
    string Title,
    string? Description,
    int Order) : IRequest<Result<ModuleDto>>;

public record UpdateModuleCommand(
    int Id,
    string Title,
    string? Description,
    bool IsActive,
    int Order) : IRequest<Result<ModuleDto>>;

public record DeleteModuleCommand(int Id) : IRequest<Result<bool>>;

public record CreateLessonCommand(
    int ModuleId,
    string Title,
    string? Content,
    string? VideoUrl,
    int Order,
    int DurationMinutes) : IRequest<Result<LessonDto>>;

public record UpdateLessonCommand(
    int Id,
    string Title,
    string? Content,
    string? VideoUrl,
    bool IsActive,
    int Order,
    int DurationMinutes) : IRequest<Result<LessonDto>>;

public record DeleteLessonCommand(int Id) : IRequest<Result<bool>>;

public record CreateResourceCommand(
    int LessonId,
    string Title,
    string? Description,
    ResourceType Type,
    string FilePath,
    string? Url,
    long? FileSizeBytes,
    string? MimeType,
    int Order) : IRequest<Result<ResourceDto>>;

public record UpdateResourceCommand(
    int Id,
    string Title,
    string? Description,
    ResourceType Type,
    string FilePath,
    string? Url,
    long? FileSizeBytes,
    string? MimeType,
    bool IsActive,
    int Order) : IRequest<Result<ResourceDto>>;

public record DeleteResourceCommand(int Id) : IRequest<Result<bool>>;
