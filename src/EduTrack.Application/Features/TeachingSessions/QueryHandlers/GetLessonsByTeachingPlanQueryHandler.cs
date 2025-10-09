using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.Courses;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetLessonsByTeachingPlanQueryHandler : IRequestHandler<GetLessonsByTeachingPlanQuery, Result<List<LessonDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ILessonRepository _lessonRepository;

    public GetLessonsByTeachingPlanQueryHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ILessonRepository lessonRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _lessonRepository = lessonRepository;
    }

    public async Task<Result<List<LessonDto>>> Handle(GetLessonsByTeachingPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<List<LessonDto>>.Failure("پلن آموزشی یافت نشد.");
            }

            var lessons = await _lessonRepository.GetByCourseIdAsync(teachingPlan.CourseId, cancellationToken);
            
            var lessonDtos = lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                ModuleId = l.ModuleId,
                Title = l.Title,
                Content = l.Content,
                VideoUrl = l.VideoUrl,
                IsActive = l.IsActive,
                Order = l.Order,
                DurationMinutes = l.DurationMinutes,
                ModuleTitle = l.Module?.Title ?? "Unknown Module"
            }).ToList();

            return Result<List<LessonDto>>.Success(lessonDtos);
        }
        catch (Exception ex)
        {
            return Result<List<LessonDto>>.Failure($"خطا در بارگذاری دروس: {ex.Message}");
        }
    }
}
