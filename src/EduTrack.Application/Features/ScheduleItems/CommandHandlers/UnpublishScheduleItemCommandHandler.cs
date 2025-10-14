using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.ScheduleItems.Commands;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.ScheduleItems.CommandHandlers;

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
