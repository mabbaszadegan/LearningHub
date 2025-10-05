using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.Courses.Commands;

public class ToggleCourseActiveCommandHandler : IRequestHandler<ToggleCourseActiveCommand, Result<bool>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleCourseActiveCommandHandler(
        IRepository<Course> courseRepository,
        IUnitOfWork unitOfWork)
    {
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ToggleCourseActiveCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (course == null)
        {
            return Result<bool>.Failure("دوره یافت نشد");
        }

        if (course.IsActive)
        {
            course.Deactivate();
        }
        else
        {
            course.Activate();
        }

        await _courseRepository.UpdateAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(course.IsActive);
    }
}
