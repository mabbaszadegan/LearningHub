using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class CreateScheduleItemCommandHandler : IRequestHandler<CreateScheduleItemCommand, Result<int>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ITeachingSessionReportRepository _sessionReportRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        ITeachingPlanRepository teachingPlanRepository,
        ICourseRepository courseRepository,
        ITeachingSessionReportRepository sessionReportRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _teachingPlanRepository = teachingPlanRepository;
        _courseRepository = courseRepository;
        _sessionReportRepository = sessionReportRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var contextValidation = await ResolveContextAsync(request, cancellationToken);
            if (!contextValidation.IsSuccess)
            {
                return Result<int>.Failure(contextValidation.Error ?? "اطلاعات زمینه آیتم معتبر نیست.");
            }

            var (courseId, teachingPlanId, sessionReportId) = contextValidation.Value;

            // Note: SubChapter validation is handled in step 3, not during initial creation
            // This allows creating a basic item first and then adding assignments later

            // Create schedule item
            var scheduleItem = ScheduleItem.Create(
                teachingPlanId,
                request.Type,
                request.Title,
                request.Description,
                request.StartDate,
                request.DueDate,
                request.IsMandatory,
                request.ContentJson,
                request.MaxScore,
                request.GroupId,
                null, // lessonId
                request.DisciplineHint,
                courseId,
                sessionReportId
            );

            // Add group assignments if specified
            if (request.GroupIds != null && request.GroupIds.Any())
            {
                foreach (var groupId in request.GroupIds)
                {
                    var groupAssignment = ScheduleItemGroupAssignment.Create(scheduleItem.Id, groupId);
                    scheduleItem.AddGroupAssignment(groupAssignment);
                }
            }

            // Add subchapter assignments if specified
            if (request.SubChapterIds != null && request.SubChapterIds.Any())
            {
                foreach (var subChapterId in request.SubChapterIds)
                {
                    var subChapterAssignment = ScheduleItemSubChapterAssignment.Create(scheduleItem.Id, subChapterId);
                    scheduleItem.AddSubChapterAssignment(subChapterAssignment);
                }
            }

            // Add student assignments if specified
            if (request.StudentProfileIds != null && request.StudentProfileIds.Any())
            {
                // Remove duplicates to prevent unique constraint violations
                var uniqueStudentProfileIds = request.StudentProfileIds.Distinct().ToList();
                
                foreach (var studentProfileId in uniqueStudentProfileIds)
                {
                    var studentAssignment = ScheduleItemStudentAssignment.Create(scheduleItem.Id, studentProfileId);
                    scheduleItem.AddStudentAssignment(studentAssignment);
                }
            }

            await _scheduleItemRepository.AddAsync(scheduleItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(scheduleItem.Id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"خطا در ایجاد آیتم آموزشی: {ex.Message}");
        }
    }

    private async Task<Result<(int? CourseId, int? TeachingPlanId, int? SessionReportId)>> ResolveContextAsync(
        CreateScheduleItemCommand request,
        CancellationToken cancellationToken)
    {
        int? resolvedCourseId = request.CourseId;
        int? resolvedTeachingPlanId = request.TeachingPlanId;
        int? resolvedSessionReportId = request.SessionReportId;

        if (resolvedSessionReportId.HasValue)
        {
            var sessionReport = await _sessionReportRepository.GetByIdAsync(resolvedSessionReportId.Value, cancellationToken);
            if (sessionReport == null)
            {
                return Result<(int?, int?, int?)>.Failure("گزارش جلسه آموزشی یافت نشد.");
            }

            resolvedTeachingPlanId ??= sessionReport.TeachingPlanId;

            if (!resolvedCourseId.HasValue)
            {
                if (sessionReport.TeachingPlan != null)
                {
                    resolvedCourseId = sessionReport.TeachingPlan.CourseId;
                }
                else if (resolvedTeachingPlanId.HasValue)
                {
                    var reportPlan = await _teachingPlanRepository.GetByIdAsync(resolvedTeachingPlanId.Value, cancellationToken);
                    if (reportPlan == null)
                    {
                        return Result<(int?, int?, int?)>.Failure("برنامه آموزشی مرتبط با جلسه یافت نشد.");
                    }
                    resolvedCourseId = reportPlan.CourseId;
                }
            }
        }

        if (resolvedTeachingPlanId.HasValue)
        {
            var teachingPlan = await _teachingPlanRepository.GetByIdAsync(resolvedTeachingPlanId.Value, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<(int?, int?, int?)>.Failure("برنامه آموزشی یافت نشد.");
            }
            resolvedCourseId ??= teachingPlan.CourseId;
        }
        else if (!resolvedCourseId.HasValue)
        {
            return Result<(int?, int?, int?)>.Failure("شناسه برنامه آموزشی یا دوره باید مشخص باشد.");
        }

        if (resolvedCourseId.HasValue)
        {
            var course = await _courseRepository.GetByIdAsync(resolvedCourseId.Value, cancellationToken);
            if (course == null)
            {
                return Result<(int?, int?, int?)>.Failure("دوره مورد نظر یافت نشد.");
            }
        }

        return Result<(int?, int?, int?)>.Success((resolvedCourseId, resolvedTeachingPlanId, resolvedSessionReportId));
    }
}
