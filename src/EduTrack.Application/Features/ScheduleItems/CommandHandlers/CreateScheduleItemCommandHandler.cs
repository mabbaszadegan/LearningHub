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
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        ITeachingPlanRepository teachingPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _teachingPlanRepository = teachingPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(CreateScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify teaching plan exists
            var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<int>.Failure("برنامه آموزشی یافت نشد.");
            }

            // Note: SubChapter validation is handled in step 3, not during initial creation
            // This allows creating a basic item first and then adding assignments later

            // Create schedule item
            var scheduleItem = ScheduleItem.Create(
                request.TeachingPlanId,
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
                request.DisciplineHint
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
}
