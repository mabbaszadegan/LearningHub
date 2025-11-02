using EduTrack.Application.Common.Models;
using EduTrack.Application.Common.Models.TeachingSessions;
using EduTrack.Application.Features.TeachingSessions.Queries;
using EduTrack.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Application.Features.TeachingSessions.QueryHandlers;

public class GetSubChapterCoverageDataQueryHandler : IRequestHandler<GetSubChapterCoverageDataQuery, Result<SubChapterCoverageDataDto>>
{
    private readonly IRepository<Domain.Entities.TeachingSessionReport> _sessionRepository;
    private readonly IRepository<Domain.Entities.StudentGroup> _groupRepository;
    private readonly IRepository<Domain.Entities.Chapter> _chapterRepository;
    private readonly IRepository<Domain.Entities.SubChapter> _subChapterRepository;
    private readonly ITeachingSessionTopicCoverageRepository _topicCoverageRepository;
    private readonly ITeachingPlanProgressRepository _progressRepository;

    public GetSubChapterCoverageDataQueryHandler(
        IRepository<Domain.Entities.TeachingSessionReport> sessionRepository,
        IRepository<Domain.Entities.StudentGroup> groupRepository,
        IRepository<Domain.Entities.Chapter> chapterRepository,
        IRepository<Domain.Entities.SubChapter> subChapterRepository,
        ITeachingSessionTopicCoverageRepository topicCoverageRepository,
        ITeachingPlanProgressRepository progressRepository)
    {
        _sessionRepository = sessionRepository;
        _groupRepository = groupRepository;
        _chapterRepository = chapterRepository;
        _subChapterRepository = subChapterRepository;
        _topicCoverageRepository = topicCoverageRepository;
        _progressRepository = progressRepository;
    }

    public async Task<Result<SubChapterCoverageDataDto>> Handle(GetSubChapterCoverageDataQuery request, CancellationToken cancellationToken)
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
                return Result<SubChapterCoverageDataDto>.Failure("جلسه آموزشی یافت نشد.");
            }

            // Get groups for this teaching plan
            var groups = await _groupRepository.GetAll()
                .Include(g => g.Members)
                .ThenInclude(m => m.Student)
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

            var result = new SubChapterCoverageDataDto
            {
                SessionId = session.Id,
                SessionTitle = session.Title ?? $"جلسه {session.SessionDate:yyyy/MM/dd}",
                SessionDate = session.SessionDate,
                TeachingPlanId = session.TeachingPlanId,
                TeachingPlanTitle = session.TeachingPlan.Title,
                CourseTitle = session.TeachingPlan.Course.Title,
                Groups = groups.Select(g => new GroupDataDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    MemberCount = g.Members.Count,
                    Members = g.Members.Select(m => new GroupMemberDto
                    {
                        Id = m.Id,
                        StudentGroupId = m.StudentGroupId,
                        StudentId = m.StudentId,
                        StudentName = m.Student.FullName,
                        StudentEmail = m.Student.Email ?? string.Empty
                    }).ToList()
                }).ToList(),
                Chapters = new List<EduTrack.Application.Common.Models.Courses.ChapterDto>(),
                ExistingCoverages = existingCoverages
                    .Where(tc => tc.TopicType == "SubTopic")
                    .Select(tc => new SubChapterCoverageDto
                    {
                        Id = tc.Id,
                        TeachingSessionReportId = tc.TeachingSessionReportId,
                        StudentGroupId = tc.StudentGroupId,
                        GroupName = groups.FirstOrDefault(g => g.Id == tc.StudentGroupId)?.Name ?? "",
                        SubChapterId = tc.TopicId ?? 0,
                        SubChapterTitle = tc.TopicTitle ?? "",
                        ChapterTitle = "", // Will be filled from chapters data
                        WasPlanned = tc.WasPlanned,
                        WasCovered = tc.WasCovered,
                        CoveragePercentage = tc.CoveragePercentage,
                        TeacherNotes = tc.TeacherNotes,
                        Challenges = tc.Challenges,
                        CreatedAt = tc.CreatedAt
                    }).ToList(),
                GroupCoverageStats = new List<GroupCoverageStatsDto>()
            };

            // Populate chapters sequentially to avoid DbContext concurrency issues
            foreach (var chapter in chapters)
            {
                var totalCoverageCount = await _progressRepository.GetTotalCoverageCountForChapterAsync(chapter.Id, cancellationToken);
                var averageProgressPercentage = await _progressRepository.GetAverageProgressForChapterAsync(chapter.Id, cancellationToken);

                var subChapters = new List<EduTrack.Application.Common.Models.Courses.SubChapterDto>();
                foreach (var subChapter in chapter.SubChapters.OrderBy(sc => sc.Order))
                {
                    var coverageCount = await _progressRepository.GetCoverageCountForSubTopicAsync(subChapter.Id, cancellationToken);
                    var subChapterAverageProgress = await _progressRepository.GetAverageProgressForSubTopicAsync(subChapter.Id, cancellationToken);

                    subChapters.Add(new EduTrack.Application.Common.Models.Courses.SubChapterDto
                    {
                        Id = subChapter.Id,
                        ChapterId = subChapter.ChapterId,
                        Title = subChapter.Title,
                        Description = subChapter.Description,
                        Objective = subChapter.Objective,
                        IsActive = subChapter.IsActive,
                        Order = subChapter.Order,
                        CreatedAt = subChapter.CreatedAt,
                        UpdatedAt = subChapter.UpdatedAt,
                        // ContentCount removed - EducationalContent entity removed
                        ChapterTitle = chapter.Title,
                        CoverageCount = coverageCount,
                        AverageProgressPercentage = subChapterAverageProgress
                    });
                }

                result.Chapters.Add(new EduTrack.Application.Common.Models.Courses.ChapterDto
                {
                    Id = chapter.Id,
                    CourseId = chapter.CourseId,
                    Title = chapter.Title,
                    Description = chapter.Description,
                    Objective = chapter.Objective,
                    IsActive = chapter.IsActive,
                    Order = chapter.Order,
                    CreatedAt = chapter.CreatedAt,
                    UpdatedAt = chapter.UpdatedAt,
                    SubChapterCount = chapter.SubChapters.Count,
                    TotalCoverageCount = totalCoverageCount,
                    AverageProgressPercentage = averageProgressPercentage,
                    SubChapters = subChapters
                });
            }

            // Fill chapter titles for existing coverages
            foreach (var coverage in result.ExistingCoverages)
            {
                var chapter = chapters.FirstOrDefault(c => c.SubChapters.Any(sc => sc.Id == coverage.SubChapterId));
                if (chapter != null)
                {
                    coverage.ChapterTitle = chapter.Title;
                }
            }

            // Calculate group-based coverage statistics
            foreach (var group in groups)
            {
                var groupStats = new GroupCoverageStatsDto
                {
                    GroupId = group.Id,
                    GroupName = group.Name,
                    ChapterStats = new List<ChapterGroupStatsDto>()
                };

                foreach (var chapter in chapters)
                {
                    var chapterGroupStats = new ChapterGroupStatsDto
                    {
                        ChapterId = chapter.Id,
                        ChapterTitle = chapter.Title,
                        TotalCoverageCount = await _progressRepository.GetTotalCoverageCountForChapterByGroupAsync(chapter.Id, group.Id, cancellationToken),
                        AverageProgressPercentage = await _progressRepository.GetAverageProgressForChapterByGroupAsync(chapter.Id, group.Id, cancellationToken),
                        SubChapterStats = new List<SubChapterGroupStatsDto>()
                    };

                    foreach (var subChapter in chapter.SubChapters.OrderBy(sc => sc.Order))
                    {
                        chapterGroupStats.SubChapterStats.Add(new SubChapterGroupStatsDto
                        {
                            SubChapterId = subChapter.Id,
                            SubChapterTitle = subChapter.Title,
                            CoverageCount = await _progressRepository.GetCoverageCountForSubTopicByGroupAsync(subChapter.Id, group.Id, cancellationToken),
                            AverageProgressPercentage = await _progressRepository.GetAverageProgressForSubTopicByGroupAsync(subChapter.Id, group.Id, cancellationToken)
                        });
                    }

                    groupStats.ChapterStats.Add(chapterGroupStats);
                }

                result.GroupCoverageStats.Add(groupStats);
            }

            return Result<SubChapterCoverageDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SubChapterCoverageDataDto>.Failure($"خطا در بارگذاری اطلاعات پوشش زیرمباحث: {ex.Message}");
        }
    }
}
