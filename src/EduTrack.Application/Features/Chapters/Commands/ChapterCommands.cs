using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Commands;

public record CreateChapterCommand(
    int CourseId,
    string Title,
    string? Description,
    string Objective,
    int Order) : IRequest<Result<ChapterDto>>;

public record UpdateChapterCommand(
    int Id,
    string Title,
    string? Description,
    string Objective,
    bool IsActive,
    int Order) : IRequest<Result<ChapterDto>>;

public record DeleteChapterCommand(int Id) : IRequest<Result<bool>>;

public record CreateSubChapterCommand(
    int ChapterId,
    string Title,
    string? Description,
    string Objective,
    int Order) : IRequest<Result<SubChapterDto>>;

public record UpdateSubChapterCommand(
    int Id,
    string Title,
    string? Description,
    string Objective,
    bool IsActive,
    int Order) : IRequest<Result<SubChapterDto>>;

public record DeleteSubChapterCommand(int Id) : IRequest<Result<bool>>;
