using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record GetPlannedItemsQuery(int SessionId) : IRequest<Result<List<PlannedItemDto>>>;
