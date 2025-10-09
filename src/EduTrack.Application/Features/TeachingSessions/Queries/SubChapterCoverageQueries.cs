using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record GetSubChapterCoverageDataQuery(int SessionId) : IRequest<Result<SubChapterCoverageDataDto>>;

public record GetSubChapterCoverageStepDataQuery(int SessionId) : IRequest<Result<SubChapterCoverageStepDataDto>>;
