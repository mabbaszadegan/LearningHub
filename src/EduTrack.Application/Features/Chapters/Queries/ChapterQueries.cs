using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.Chapters.Queries;

public record GetChaptersByCourseIdQuery(int CourseId) : IRequest<Result<List<ChapterDto>>>;

public record GetChapterByIdQuery(int Id) : IRequest<Result<ChapterDto>>;

public record GetSubChaptersByChapterIdQuery(int ChapterId) : IRequest<Result<List<SubChapterDto>>>;

public record GetSubChapterByIdQuery(int Id) : IRequest<Result<SubChapterDto>>;
