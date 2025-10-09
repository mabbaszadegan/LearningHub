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

    public GetSubChapterCoverageDataQueryHandler(
        IRepository<Domain.Entities.TeachingSessionReport> sessionRepository,
        IRepository<Domain.Entities.StudentGroup> groupRepository,
        IRepository<Domain.Entities.Chapter> chapterRepository,
        IRepository<Domain.Entities.SubChapter> subChapterRepository,
        ITeachingSessionTopicCoverageRepository topicCoverageRepository)
    {
        _sessionRepository = sessionRepository;
        _groupRepository = groupRepository;
        _chapterRepository = chapterRepository;
        _subChapterRepository = subChapterRepository;
        _topicCoverageRepository = topicCoverageRepository;
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
                Chapters = chapters.Select(c => new EduTrack.Application.Common.Models.Courses.ChapterDto
                {
                    Id = c.Id,
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Objective = c.Objective,
                    IsActive = c.IsActive,
                    Order = c.Order,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    SubChapterCount = c.SubChapters.Count,
                    SubChapters = c.SubChapters.OrderBy(sc => sc.Order).Select(sc => new EduTrack.Application.Common.Models.Courses.SubChapterDto
                    {
                        Id = sc.Id,
                        ChapterId = sc.ChapterId,
                        Title = sc.Title,
                        Description = sc.Description,
                        Objective = sc.Objective,
                        IsActive = sc.IsActive,
                        Order = sc.Order,
                        CreatedAt = sc.CreatedAt,
                        UpdatedAt = sc.UpdatedAt,
                        ContentCount = 0, // We don't need this for coverage
                        ChapterTitle = c.Title
                    }).ToList()
                }).ToList(),
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
                        CoverageStatus = tc.CoverageStatus,
                        TeacherNotes = tc.TeacherNotes,
                        Challenges = tc.Challenges,
                        CreatedAt = tc.CreatedAt
                    }).ToList()
            };

            // Fill chapter titles for existing coverages
            foreach (var coverage in result.ExistingCoverages)
            {
                var chapter = chapters.FirstOrDefault(c => c.SubChapters.Any(sc => sc.Id == coverage.SubChapterId));
                if (chapter != null)
                {
                    coverage.ChapterTitle = chapter.Title;
                }
            }

            return Result<SubChapterCoverageDataDto>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<SubChapterCoverageDataDto>.Failure($"خطا در بارگذاری اطلاعات پوشش زیرمباحث: {ex.Message}");
        }
    }
}
