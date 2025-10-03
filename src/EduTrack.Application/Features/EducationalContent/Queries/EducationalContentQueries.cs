using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.EducationalContent.Queries;

public record GetEducationalContentsBySubChapterIdQuery(int SubChapterId) : IRequest<Result<List<EducationalContentDto>>>;

public record GetEducationalContentByIdQuery(int Id) : IRequest<Result<EducationalContentDto>>;
