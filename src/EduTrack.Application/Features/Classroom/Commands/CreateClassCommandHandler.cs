using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EduTrack.Application.Features.Classroom.Commands;

public class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("انتخاب دوره الزامی است");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام کلاس الزامی است")
            .MaximumLength(200).WithMessage("نام کلاس نمی‌تواند بیش از 200 کاراکتر باشد");

        RuleFor(x => x.TeacherId)
            .NotEmpty().WithMessage("انتخاب معلم الزامی است");

        RuleFor(x => x.StartDate)
            .Must(date => date > DateTimeOffset.UtcNow.AddDays(-1))
            .WithMessage("تاریخ شروع باید در آینده باشد");
    }
}

public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Result<ClassDto>>
{
    private readonly IRepository<Class> _classRepository;
    private readonly IRepository<Course> _courseRepository;
    private readonly IUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly UserManager<User> _userManager;

    public CreateClassCommandHandler(
        IRepository<Class> classRepository,
        IRepository<Course> courseRepository,
        IUserService userService,
        IUnitOfWork unitOfWork,
        IClock clock,
        UserManager<User> userManager)
    {
        _classRepository = classRepository;
        _courseRepository = courseRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _userManager = userManager;
    }

    public async Task<Result<ClassDto>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var course = await _courseRepository.GetByIdAsync(request.CourseId, cancellationToken);
        if (course == null)
        {
            return Result<ClassDto>.Failure("دوره مورد نظر یافت نشد");
        }

        if (string.IsNullOrWhiteSpace(request.TeacherId))
        {
            return Result<ClassDto>.Failure("معلم مورد نظر یافت نشد یا نقش صحیح ندارد");
        }

        var teacher = await _userService.GetUserByIdAsync(request.TeacherId, cancellationToken);
        if (teacher == null)
        {
            return Result<ClassDto>.Failure("معلم مورد نظر یافت نشد");
        }

        var teacherRoles = await _userManager.GetRolesAsync(teacher);
        if (!teacherRoles.Contains("Teacher"))
        {
            return Result<ClassDto>.Failure("کاربر انتخاب شده نقش معلم ندارد");
        }

        var classEntity = Class.Create(
            request.CourseId,
            request.Name,
            request.Description,
            request.TeacherId,
            request.StartDate,
            request.EndDate);

        await _classRepository.AddAsync(classEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var classDto = new ClassDto
        {
            Id = classEntity.Id,
            CourseId = classEntity.CourseId,
            Name = classEntity.Name,
            Description = classEntity.Description,
            TeacherId = classEntity.TeacherId,
            TeacherName = teacher.FullName,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            IsActive = classEntity.IsActive,
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt,
            StudentCount = 0
        };

        return Result<ClassDto>.Success(classDto);
    }
}
