using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.Queries;

public record GetLessonsByTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<LessonDto>>>;
