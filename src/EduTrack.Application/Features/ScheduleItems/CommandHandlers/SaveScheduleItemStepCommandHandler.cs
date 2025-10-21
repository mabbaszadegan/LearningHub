using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Helpers;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

public class SaveScheduleItemStepCommandHandler : IRequestHandler<SaveScheduleItemStepCommand, Result<int>>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SaveScheduleItemStepCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        ITeachingPlanRepository teachingPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _teachingPlanRepository = teachingPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> Handle(SaveScheduleItemStepCommand request, CancellationToken cancellationToken)
    {
        try
        {
            ScheduleItem scheduleItem;

            if (request.Id.HasValue)
            {
                // Update existing item
                var existingItem = await _scheduleItemRepository.GetByIdAsync(request.Id.Value, cancellationToken);
                if (existingItem == null)
                {
                    return Result<int>.Failure("آیتم آموزشی یافت نشد.");
                }
                scheduleItem = existingItem;

                // Update fields based on step
                await UpdateScheduleItemFromStep(scheduleItem, request, cancellationToken);
            }
            else
            {
                // Create new item for first step
                if (request.Step != 1)
                {
                    return Result<int>.Failure("برای ایجاد آیتم جدید باید از مرحله اول شروع کنید.");
                }

                if (!request.Type.HasValue || string.IsNullOrWhiteSpace(request.Title))
                {
                    return Result<int>.Failure("نوع آیتم و عنوان برای مرحله اول الزامی است.");
                }

                // Verify teaching plan exists
                var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
                if (teachingPlan == null)
                {
                    return Result<int>.Failure("برنامه آموزشی یافت نشد.");
                }

                // Convert Persian dates to DateTimeOffset
                var startDate = ConvertPersianDateToDateTimeOffset(request.PersianStartDate, request.StartTime) 
                    ?? DateTimeOffset.UtcNow;
                var dueDate = ConvertPersianDateToDateTimeOffset(request.PersianDueDate, request.DueTime);

                // Create with minimal required data
                scheduleItem = ScheduleItem.Create(
                    request.TeachingPlanId,
                    request.Type.Value,
                    request.Title,
                    request.Description,
                    startDate,
                    dueDate,
                    request.IsMandatory ?? false,
                    request.ContentJson ?? "{}",
                    request.MaxScore,
                    request.GroupId,
                    null, // lessonId
                    null // DisciplineHint
                );
            }

            // Update current step
            scheduleItem.UpdateCurrentStep(request.Step);

            // Handle group and subchapter assignments for step 3 (assignment step)
            if (request.Step == 3)
            {
                // Validate subchapter selection (mandatory)
                if (request.SubChapterIds == null || !request.SubChapterIds.Any())
                {
                    return Result<int>.Failure("انتخاب حداقل یک زیرمبحث اجباری است.");
                }

                // Clear in-memory collections (database removal will be handled later)
                scheduleItem.GroupAssignments.Clear();
                scheduleItem.SubChapterAssignments.Clear();
                scheduleItem.StudentAssignments.Clear();

                // Add new group assignments if provided
                if (request.GroupIds != null && request.GroupIds.Any())
                {
                    foreach (var groupId in request.GroupIds)
                    {
                        var groupAssignment = ScheduleItemGroupAssignment.Create(scheduleItem.Id, groupId);
                        scheduleItem.GroupAssignments.Add(groupAssignment);
                    }
                }

                // Add new subchapter assignments if provided
                if (request.SubChapterIds != null && request.SubChapterIds.Any())
                {
                    foreach (var subChapterId in request.SubChapterIds)
                    {
                        var subChapterAssignment = ScheduleItemSubChapterAssignment.Create(scheduleItem.Id, subChapterId);
                        scheduleItem.SubChapterAssignments.Add(subChapterAssignment);
                    }
                }

                // Add new student assignments if provided
                if (request.StudentIds != null && request.StudentIds.Any())
                {
                    // Remove duplicates to prevent unique constraint violations
                    var uniqueStudentIds = request.StudentIds.Distinct().ToList();
                    
                    foreach (var studentId in uniqueStudentIds)
                    {
                        var studentAssignment = ScheduleItemStudentAssignment.Create(scheduleItem.Id, studentId);
                        scheduleItem.StudentAssignments.Add(studentAssignment);
                    }
                }
            }

            if (request.Id.HasValue)
            {
                // For updates, handle assignments separately to avoid concurrency issues
                if (request.Step == 3)
                {
                    // Remove existing assignments from database first
                    var existingGroupAssignments = await _scheduleItemRepository.GetGroupAssignmentsAsync(scheduleItem.Id, cancellationToken);
                    if (existingGroupAssignments.Any())
                    {
                        await _scheduleItemRepository.RemoveGroupAssignmentsAsync(existingGroupAssignments, cancellationToken);
                    }

                    var existingSubChapterAssignments = await _scheduleItemRepository.GetSubChapterAssignmentsAsync(scheduleItem.Id, cancellationToken);
                    if (existingSubChapterAssignments.Any())
                    {
                        await _scheduleItemRepository.RemoveSubChapterAssignmentsAsync(existingSubChapterAssignments, cancellationToken);
                    }

                    var existingStudentAssignments = await _scheduleItemRepository.GetStudentAssignmentsAsync(scheduleItem.Id, cancellationToken);
                    if (existingStudentAssignments.Any())
                    {
                        await _scheduleItemRepository.RemoveStudentAssignmentsAsync(existingStudentAssignments, cancellationToken);
                    }

                    // Save assignment removals first
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                // Update the main entity
                await _scheduleItemRepository.UpdateAsync(scheduleItem, cancellationToken);
            }
            else
            {
                await _scheduleItemRepository.AddAsync(scheduleItem, cancellationToken);
            }

            // Save all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(scheduleItem.Id);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // Handle concurrency conflicts - try to reload and retry once
            try
            {
            if (request.Id.HasValue)
            {
                // Clear the current context to avoid tracking conflicts
                _unitOfWork.ClearChangeTracker();
                
                // Reload the entity and try again
                var reloadedItem = await _scheduleItemRepository.GetByIdAsync(request.Id.Value, cancellationToken);
                    if (reloadedItem != null)
                    {
                        // Update the reloaded entity with new data
                        await UpdateScheduleItemFromStep(reloadedItem, request, cancellationToken);
                        reloadedItem.UpdateCurrentStep(request.Step);
                        
                        // Handle step 3 assignments on reloaded entity
                        if (request.Step == 3)
                        {
                            // Clear and add new assignments
                            reloadedItem.GroupAssignments.Clear();
                            reloadedItem.SubChapterAssignments.Clear();
                            reloadedItem.StudentAssignments.Clear();

                            if (request.GroupIds != null && request.GroupIds.Any())
                            {
                                foreach (var groupId in request.GroupIds)
                                {
                                    var groupAssignment = ScheduleItemGroupAssignment.Create(reloadedItem.Id, groupId);
                                    reloadedItem.GroupAssignments.Add(groupAssignment);
                                }
                            }

                            if (request.SubChapterIds != null && request.SubChapterIds.Any())
                            {
                                foreach (var subChapterId in request.SubChapterIds)
                                {
                                    var subChapterAssignment = ScheduleItemSubChapterAssignment.Create(reloadedItem.Id, subChapterId);
                                    reloadedItem.SubChapterAssignments.Add(subChapterAssignment);
                                }
                            }

                            if (request.StudentIds != null && request.StudentIds.Any())
                            {
                                var uniqueStudentIds = request.StudentIds.Distinct().ToList();
                                foreach (var studentId in uniqueStudentIds)
                                {
                                    var studentAssignment = ScheduleItemStudentAssignment.Create(reloadedItem.Id, studentId);
                                    reloadedItem.StudentAssignments.Add(studentAssignment);
                                }
                            }
                        }

                        await _scheduleItemRepository.UpdateAsync(reloadedItem, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        
                        return Result<int>.Success(reloadedItem.Id);
                    }
                }
                
                return Result<int>.Failure("داده‌ها توسط کاربر دیگری تغییر کرده است. لطفا صفحه را رفرش کنید و دوباره تلاش کنید.");
            }
            catch (Exception retryEx)
            {
                return Result<int>.Failure($"خطا در تلاش مجدد: {retryEx.Message}");
            }
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Handle duplicate key violations
            return Result<int>.Failure("خطا: تخصیص تکراری دانش‌آموز یا مبحث");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"خطا در ذخیره مرحله آیتم آموزشی: {ex.Message}");
        }
    }

    private async Task UpdateScheduleItemFromStep(ScheduleItem scheduleItem, SaveScheduleItemStepCommand request, CancellationToken cancellationToken)
    {
        switch (request.Step)
        {
            case 1:
                if (!string.IsNullOrEmpty(request.Title)) scheduleItem.UpdateTitle(request.Title);
                if (!string.IsNullOrEmpty(request.Description)) scheduleItem.UpdateDescription(request.Description);
                break;
            case 2:
                // Convert Persian dates to DateTimeOffset
                var startDate = ConvertPersianDateToDateTimeOffset(request.PersianStartDate, request.StartTime) 
                    ?? scheduleItem.StartDate;
                var dueDate = ConvertPersianDateToDateTimeOffset(request.PersianDueDate, request.DueTime);
                
                scheduleItem.UpdateDates(startDate, dueDate);
                if (request.MaxScore.HasValue) scheduleItem.UpdateMaxScore(request.MaxScore);
                if (request.IsMandatory.HasValue) scheduleItem.UpdateMandatory(request.IsMandatory.Value);
                break;
            case 3:
                // Update group assignment
                scheduleItem.UpdateAssignment(request.GroupId, null);
                
                // Update student assignments if provided
                if (request.StudentIds != null)
                {
                    // Always clear and update student assignments
                    // First, remove existing assignments from database
                    var existingStudentAssignments = await _scheduleItemRepository.GetStudentAssignmentsAsync(scheduleItem.Id, cancellationToken);
                    if (existingStudentAssignments.Any())
                    {
                        await _scheduleItemRepository.RemoveStudentAssignmentsAsync(existingStudentAssignments, cancellationToken);
                    }
                    
                    // Clear in-memory collection
                    scheduleItem.StudentAssignments.Clear();

                    // Add new student assignments if provided
                    if (request.StudentIds.Any())
                    {
                        // Remove duplicates to prevent unique constraint violations
                        var uniqueStudentIds = request.StudentIds.Distinct().ToList();
                        
                        foreach (var studentId in uniqueStudentIds)
                        {
                            // Double-check that this assignment doesn't already exist
                            if (!scheduleItem.StudentAssignments.Any(sa => sa.StudentId == studentId))
                            {
                                var studentAssignment = ScheduleItemStudentAssignment.Create(scheduleItem.Id, studentId);
                                scheduleItem.StudentAssignments.Add(studentAssignment);
                            }
                        }
                    }
                }
                break;
            case 4:
                if (!string.IsNullOrEmpty(request.ContentJson)) scheduleItem.UpdateContent(request.ContentJson);
                break;
        }
    }

    private DateTimeOffset? ConvertPersianDateToDateTimeOffset(string? persianDate, string? timeString)
    {
        if (string.IsNullOrWhiteSpace(persianDate))
            return null;

        try
        {
            return PersianDateHelper.PersianToDateTimeOffset(persianDate, timeString);
        }
        catch
        {
            return null;
        }
    }
}
