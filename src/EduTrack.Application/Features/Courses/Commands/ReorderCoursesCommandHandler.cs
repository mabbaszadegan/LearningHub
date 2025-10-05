using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.Courses.Commands;

public class ReorderCoursesCommandHandler : IRequestHandler<ReorderCoursesCommand, Result<bool>>
{
    private readonly IRepository<Course> _courseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderCoursesCommandHandler(
        IRepository<Course> courseRepository,
        IUnitOfWork _unitOfWork)
    {
        _courseRepository = courseRepository;
        this._unitOfWork = _unitOfWork;
    }

    public async Task<Result<bool>> Handle(ReorderCoursesCommand request, CancellationToken cancellationToken)
    {
        if (request.CourseIds == null || !request.CourseIds.Any())
        {
            return Result<bool>.Failure("لیست دوره‌ها خالی است");
        }

        // Get all courses
        var courses = await _courseRepository.GetAll()
            .Where(c => request.CourseIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (courses.Count != request.CourseIds.Count)
        {
            return Result<bool>.Failure("برخی از دوره‌ها یافت نشدند");
        }

        // Update order for each course
        for (int i = 0; i < request.CourseIds.Count; i++)
        {
            var courseId = request.CourseIds[i];
            var course = courses.FirstOrDefault(c => c.Id == courseId);
            
            if (course != null)
            {
                course.UpdateOrder(i);
                await _courseRepository.UpdateAsync(course, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
