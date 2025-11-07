using System.Linq;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingPlan.Commands;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CourseEnrollmentEntity = EduTrack.Domain.Entities.CourseEnrollment;

namespace EduTrack.Application.Features.TeachingPlan.CommandHandlers;

public class UpdateLearningModeCommandHandler : IRequestHandler<UpdateLearningModeCommand, Result<bool>>
{
    private readonly IRepository<CourseEnrollmentEntity> _courseEnrollmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLearningModeCommandHandler(
        IRepository<CourseEnrollmentEntity> courseEnrollmentRepository,
        IUnitOfWork unitOfWork)
    {
        _courseEnrollmentRepository = courseEnrollmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdateLearningModeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId))
        {
            return Result<bool>.Failure("شناسه دانش‌آموز مشخص نشده است.");
        }

        var enrollmentQuery = _courseEnrollmentRepository.GetAll()
            .Where(e => e.CourseId == request.CourseId && e.StudentId == request.StudentId && e.IsActive);

        if (request.StudentProfileId.HasValue)
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.StudentProfileId == request.StudentProfileId);
        }
        else
        {
            enrollmentQuery = enrollmentQuery.Where(e => e.StudentProfileId == null);
        }

        var enrollment = await enrollmentQuery.FirstOrDefaultAsync(cancellationToken);
        if (enrollment == null)
        {
            return Result<bool>.Failure("ثبت‌نام فعال برای این پروفایل یافت نشد.");
        }

        enrollment.UpdateLearningMode(request.LearningMode);
        await _courseEnrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

