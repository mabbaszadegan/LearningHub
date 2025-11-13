using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.ScheduleItems;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.Queries;

public record GetScheduleItemsByTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemsByCourseQuery(int CourseId, bool CourseScopeOnly = true) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemByIdQuery(int Id) : IRequest<Result<ScheduleItemDto>>;

public record GetScheduleItemsByGroupQuery(int GroupId) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemsByTypeQuery(ScheduleItemType Type) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetUpcomingScheduleItemsQuery(DateTimeOffset FromDate) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetActiveScheduleItemsQuery : IRequest<Result<List<ScheduleItemDto>>>;

public record GetOverdueScheduleItemsQuery : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemStatsQuery(int TeachingPlanId) : IRequest<Result<ScheduleItemStatsDto>>;

public record GetScheduleItemContentQuery(int Id) : IRequest<Result<object>>;

public record GetScheduleItemsAccessibleToStudentQuery(string StudentId, int? StudentProfileId = null) : IRequest<Result<List<ScheduleItemDto>>>;