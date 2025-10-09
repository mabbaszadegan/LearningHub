using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Commands;

public record SaveSubChapterCoverageStepCommand(
    int SessionId,
    SubChapterCoverageStepDataDto CoverageData) : IRequest<Result>;
