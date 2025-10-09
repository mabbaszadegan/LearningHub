using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record GetSessionCompletionDataQuery(int SessionId) : IRequest<Result<SessionCompletionDataDto>>;
