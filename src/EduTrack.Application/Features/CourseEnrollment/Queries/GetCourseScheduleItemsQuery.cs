using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingPlans;
using MediatR;

namespace EduTrack.Application.Features.CourseEnrollment.Queries;

/// <summary>
/// Query to get schedule items for a specific course and student
/// </summary>
public record GetCourseScheduleItemsQuery(int CourseId, string StudentId, int? StudentProfileId = null) : IRequest<Result<List<ScheduleItemDto>>>;
