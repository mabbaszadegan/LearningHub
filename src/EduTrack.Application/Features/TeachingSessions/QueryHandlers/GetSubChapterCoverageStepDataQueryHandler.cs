using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetSubChapterCoverageStepDataQueryHandler : IRequestHandler<GetSubChapterCoverageStepDataQuery, Result<SubChapterCoverageStepDataDto>>
{
    private readonly IRepository<Domain.Entities.TeachingSessionReport> _sessionRepository;
    private readonly IRepository<Domain.Entities.StudentGroup> _groupRepository;
    private readonly IRepository<Domain.Entities.Chapter> _chapterRepository;
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;

    public GetSubChapterCoverageStepDataQueryHandler(
        IRepository<Domain.Entities.TeachingSessionReport> sessionRepository,
        IRepository<Domain.Entities.StudentGroup> groupRepository,
        IRepository<Domain.Entities.Chapter> chapterRepository,
        ITeachingSessionTopicCoverageRepository topicCoverageRepository)
    {
        _sessionRepository = sessionRepository;
        _groupRepository = groupRepository;
        _chapterRepository = chapterRepository;
        _topicCoverageRepository = topicCoverageRepository;
    }

    public async Task<Result<SubChapterCoverageStepDataDto>> Handle(GetSubChapterCoverageStepDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get session details
            var session = await _sessionRepository.GetAll()
                .Include(s => s.TeachingPlan)
                .ThenInclude(tp => tp.Course)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

            if (session == null)
            {
                return Result<SubChapterCoverageStepDataDto>.Failure("جلسه آموزشی یافت نشد.");
            }

            // Get groups for this teaching plan
            var groups = await _groupRepository.GetAll()
                .Where(g => g.TeachingPlanId == session.TeachingPlanId)
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);

            // Get chapters and subchapters for the course
            var chapters = await _chapterRepository.GetAll()
                .Include(c => c.SubChapters.Where(sc => sc.IsActive))
                .Where(c => c.CourseId == session.TeachingPlan.CourseId && c.IsActive)
                .OrderBy(c => c.Order)
                .ToListAsync(cancellationToken);

            // Get existing coverages
            var existingCoverages = await _topicCoverageRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);

            var result = new SubChapterCoverageStepDataDto
            {
                SessionId = request.SessionId,
                GroupCoverages = groups.Select(g => new GroupSubChapterCoverageDto
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    SubChapterCoverages = chapters
                        .SelectMany(c => c.SubChapters.OrderBy(sc => sc.Order).Select(sc => new { sc, c }))
                        .Select(item => 
                        {
                            var existingCoverage = existingCoverages
                                .FirstOrDefault(ec => ec.StudentGroupId == g.Id && 
                                                   ec.TopicType == "SubTopic" && 
                                                   ec.TopicId == item.sc.Id);
                            
                            return new SubChapterCoverageItemDto
                            {
                                SubChapterId = item.sc.Id,
                                SubChapterTitle = item.sc.Title,
                                ChapterTitle = item.c.Title,
                                WasPlanned = existingCoverage?.WasPlanned ?? false,
                                WasCovered = existingCoverage?.WasCovered ?? false,
                                CoveragePercentage = existingCoverage?.CoveragePercentage ?? 0,
                                CoverageStatus = existingCoverage?.CoverageStatus ?? 0,
                                TeacherNotes = existingCoverage?.TeacherNotes,
                                Challenges = existingCoverage?.Challenges
                            };
                        }).ToList()
                }).ToList()
            };

            return Result<SubChapterCoverageStepDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SubChapterCoverageStepDataDto>.Failure($"خطا در بارگذاری اطلاعات پوشش زیرمباحث: {ex.Message}");
        }
    }
}
