using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class UpdateScheduleItemCommandHandler : IRequestHandler<UpdateScheduleItemCommand, Result>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.Id, cancellationToken);
            if (scheduleItem == null)
            {
                return Result.Failure("آیتم آموزشی یافت نشد.");
            }

            scheduleItem.UpdateTitle(request.Title);
            scheduleItem.UpdateDescription(request.Description);
            scheduleItem.UpdateDates(request.StartDate, request.DueDate);
            scheduleItem.UpdateMandatory(request.IsMandatory);
            scheduleItem.UpdateContent(request.ContentJson);
            scheduleItem.UpdateMaxScore(request.MaxScore);

            // Update group assignments
            if (request.GroupIds != null)
            {
                // Remove existing group assignments
                var existingGroupIds = scheduleItem.GroupAssignments.Select(ga => ga.StudentGroupId).ToList();
                foreach (var existingGroupId in existingGroupIds)
                {
                    scheduleItem.RemoveGroupAssignment(existingGroupId);
                }

                // Add new group assignments
                foreach (var groupId in request.GroupIds)
                {
                    var groupAssignment = ScheduleItemGroupAssignment.Create(scheduleItem.Id, groupId);
                    scheduleItem.AddGroupAssignment(groupAssignment);
                }
            }

            // Update subchapter assignments
            if (request.SubChapterIds != null)
            {
                // Remove existing subchapter assignments
                var existingSubChapterIds = scheduleItem.SubChapterAssignments.Select(sca => sca.SubChapterId).ToList();
                foreach (var existingSubChapterId in existingSubChapterIds)
                {
                    scheduleItem.RemoveSubChapterAssignment(existingSubChapterId);
                }

                // Add new subchapter assignments
                foreach (var subChapterId in request.SubChapterIds)
                {
                    var subChapterAssignment = ScheduleItemSubChapterAssignment.Create(scheduleItem.Id, subChapterId);
                    scheduleItem.AddSubChapterAssignment(subChapterAssignment);
                }
            }

            await _scheduleItemRepository.UpdateAsync(scheduleItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در به‌روزرسانی آیتم آموزشی: {ex.Message}");
        }
    }
}
