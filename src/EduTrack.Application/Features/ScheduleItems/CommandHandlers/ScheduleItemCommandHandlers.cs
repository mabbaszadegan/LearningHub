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
                request.LessonId,
                request.DisciplineHint
            );

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

public class DeleteScheduleItemCommandHandler : IRequestHandler<DeleteScheduleItemCommand, Result>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.Id, cancellationToken);
            if (scheduleItem == null)
            {
                return Result.Failure("آیتم آموزشی یافت نشد.");
            }

            await _scheduleItemRepository.DeleteAsync(scheduleItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در حذف آیتم آموزشی: {ex.Message}");
        }
    }
}

public class PublishScheduleItemCommandHandler : IRequestHandler<PublishScheduleItemCommand, Result>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublishScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PublishScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.Id, cancellationToken);
            if (scheduleItem == null)
            {
                return Result.Failure("آیتم آموزشی یافت نشد.");
            }

            // Logic to publish the item (you might need to add a method to ScheduleItem entity)
            // For now, we'll just update the UpdatedAt field
            scheduleItem.UpdateTitle(scheduleItem.Title); // This will update UpdatedAt

            await _scheduleItemRepository.UpdateAsync(scheduleItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در انتشار آیتم آموزشی: {ex.Message}");
        }
    }
}

public class UnpublishScheduleItemCommandHandler : IRequestHandler<UnpublishScheduleItemCommand, Result>
{
    private readonly IScheduleItemRepository _scheduleItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnpublishScheduleItemCommandHandler(
        IScheduleItemRepository scheduleItemRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleItemRepository = scheduleItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UnpublishScheduleItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var scheduleItem = await _scheduleItemRepository.GetByIdAsync(request.Id, cancellationToken);
            if (scheduleItem == null)
            {
                return Result.Failure("آیتم آموزشی یافت نشد.");
            }

            // Logic to unpublish the item
            scheduleItem.UpdateTitle(scheduleItem.Title); // This will update UpdatedAt

            await _scheduleItemRepository.UpdateAsync(scheduleItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"خطا در لغو انتشار آیتم آموزشی: {ex.Message}");
        }
    }
}
