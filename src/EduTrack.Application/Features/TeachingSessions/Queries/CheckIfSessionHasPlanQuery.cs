using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record CheckIfSessionHasPlanQuery(int SessionId) : IRequest<Result<bool>>;
