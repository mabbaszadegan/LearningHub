using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Common.Models.TeachingPlans;
using EduTrack.Application.Common.Models.Users;
using EduTrack.Application.Features.CourseEnrollment.DTOs;
using EduTrack.Domain.Enums;
using MediatR;

namespace EduTrack.Application.Features.TeachingPlan.Queries;

// TeachingPlan Queries
public record GetTeachingPlansByCourseQuery(int CourseId) : IRequest<Result<List<TeachingPlanDto>>>;

public record GetTeachingPlansByTeacherQuery(string TeacherId) : IRequest<Result<List<TeachingPlanDto>>>;

public record GetTeachingPlanByIdQuery(int Id) : IRequest<Result<TeachingPlanDto>>;

public record GetTeachingPlanWithGroupsQuery(int Id) : IRequest<Result<TeachingPlanDto>>;

public record GetTeachingPlanWithScheduleItemsQuery(int Id) : IRequest<Result<TeachingPlanDto>>;

public record GetTeachingPlanWithAllQuery(int Id) : IRequest<Result<TeachingPlanDto>>;

// StudentGroup Queries
public record GetStudentGroupsByTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<StudentGroupDto>>>;

public record GetStudentGroupByIdQuery(int Id) : IRequest<Result<StudentGroupDto>>;

public record GetStudentGroupWithMembersQuery(int Id) : IRequest<Result<StudentGroupDto>>;

// ScheduleItem Queries
public record GetScheduleItemsByTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemsByGroupQuery(int GroupId) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetScheduleItemByIdQuery(int Id) : IRequest<Result<ScheduleItemDto>>;

public record GetUpcomingScheduleItemsQuery(DateTimeOffset FromDate) : IRequest<Result<List<ScheduleItemDto>>>;

public record GetOverdueScheduleItemsQuery() : IRequest<Result<List<ScheduleItemDto>>>;

public record GetActiveScheduleItemsQuery() : IRequest<Result<List<ScheduleItemDto>>>;

// Submission Queries
public record GetSubmissionsByStudentQuery(string StudentId) : IRequest<Result<List<SubmissionDto>>>;

public record GetSubmissionsByScheduleItemQuery(int ScheduleItemId) : IRequest<Result<List<SubmissionDto>>>;

public record GetSubmissionByIdQuery(int Id) : IRequest<Result<SubmissionDto>>;

public record GetSubmissionByStudentAndItemQuery(string StudentId, int ScheduleItemId) : IRequest<Result<SubmissionDto>>;

public record GetSubmissionsNeedingReviewQuery() : IRequest<Result<List<SubmissionDto>>>;

public record GetSubmissionsByTeacherQuery(string TeacherId) : IRequest<Result<List<SubmissionDto>>>;

// Student Agenda Queries
public record GetStudentAgendaQuery(string StudentId, int? CourseId = null, int? StudentProfileId = null) : IRequest<Result<StudentAgendaDto>>;

// Group Progress Queries
public record GetGroupProgressQuery(int TeachingPlanId, int? GroupId = null) : IRequest<Result<List<GroupProgressDto>>>;

// Course Queries
public record GetCourseStudentsQuery(int CourseId) : IRequest<Result<List<UserDto>>>;

// Lesson Queries
public record GetLessonsByCourseIdQuery(int CourseId) : IRequest<Result<List<LessonDto>>>;

// Student Enrollment Queries
public record GetStudentCourseEnrollmentsQuery(string StudentId, int? StudentProfileId = null) : IRequest<Result<List<CourseEnrollmentDto>>>;

// Available Students Query
public record GetAvailableStudentsForTeachingPlanQuery(int TeachingPlanId) : IRequest<Result<List<UserDto>>>;