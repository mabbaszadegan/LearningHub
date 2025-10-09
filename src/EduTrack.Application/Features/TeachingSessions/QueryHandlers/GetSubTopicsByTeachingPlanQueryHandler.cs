using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetSubTopicsByTeachingPlanQueryHandler : IRequestHandler<GetSubTopicsByTeachingPlanQuery, Result<List<SubTopicDto>>>
{
    private readonly ITeachingPlanRepository _teachingPlanRepository;
    private readonly ISubChapterRepository _subChapterRepository;

    public GetSubTopicsByTeachingPlanQueryHandler(
        ITeachingPlanRepository teachingPlanRepository,
        ISubChapterRepository subChapterRepository)
    {
        _teachingPlanRepository = teachingPlanRepository;
        _subChapterRepository = subChapterRepository;
    }

    public async Task<Result<List<SubTopicDto>>> Handle(GetSubTopicsByTeachingPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var teachingPlan = await _teachingPlanRepository.GetByIdAsync(request.TeachingPlanId, cancellationToken);
            if (teachingPlan == null)
            {
                return Result<List<SubTopicDto>>.Failure("پلن آموزشی یافت نشد.");
            }

            var subtopics = await _subChapterRepository.GetByCourseIdAsync(teachingPlan.CourseId, cancellationToken);
            
            var subtopicDtos = subtopics.Select(s => new SubTopicDto
            {
                Id = s.Id,
                ChapterId = s.ChapterId,
                Title = s.Title,
                Description = s.Description,
                Objective = s.Objective,
                IsActive = s.IsActive,
                Order = s.Order,
                ChapterTitle = s.Chapter?.Title ?? "Unknown Chapter"
            }).ToList();

            return Result<List<SubTopicDto>>.Success(subtopicDtos);
        }
        catch (Exception ex)
        {
            return Result<List<SubTopicDto>>.Failure($"خطا در بارگذاری زیرمباحث: {ex.Message}");
        }
    }
}
