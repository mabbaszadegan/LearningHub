using EduTrack.Application.Common.Models;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record GetSubTopicsByTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<SubTopicDto>>>;
