using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.InteractiveLesson.Queries;
using EduTrack.Application.Features.InteractiveLesson.DTOs;
using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MediatR;
using EduTrack.Domain.Repositories;

namespace EduTrack.Application.Features.InteractiveLesson.QueryHandlers;

public class GetInteractiveLessonsByCourseQueryHandler : IRequestHandler<GetInteractiveLessonsByCourseQuery, Result<List<InteractiveLessonDto>>>
{
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;

    public GetInteractiveLessonsByCourseQueryHandler(IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository)
    {
        _interactiveLessonRepository = interactiveLessonRepository;
    }

    public async Task<Result<List<InteractiveLessonDto>>> Handle(GetInteractiveLessonsByCourseQuery request, CancellationToken cancellationToken)
    {
        var lessons = await _interactiveLessonRepository.GetAll()
            .Where(il => il.CourseId == request.CourseId && il.IsActive)
            .OrderBy(il => il.Order)
            .Include(il => il.ContentItems)
                .ThenInclude(ci => ci.EducationalContent)
            .Include(il => il.ContentItems)
                .ThenInclude(ci => ci.InteractiveQuestion!)
                    .ThenInclude(iq => iq.Choices)
            .ToListAsync(cancellationToken);

        var dtos = lessons.Select(lesson => new InteractiveLessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            IsActive = lesson.IsActive,
            Order = lesson.Order,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            CreatedBy = lesson.CreatedBy,
            ContentItems = lesson.ContentItems
                .Where(ci => ci.IsActive)
                .OrderBy(ci => ci.Order)
                .Select(ci => new InteractiveContentItemDto
                {
                    Id = ci.Id,
                    InteractiveLessonId = ci.InteractiveLessonId,
                    Order = ci.Order,
                    IsActive = ci.IsActive,
                    CreatedAt = ci.CreatedAt,
                    EducationalContentId = ci.EducationalContentId,
                    InteractiveQuestionId = ci.InteractiveQuestionId,
                    EducationalContent = ci.EducationalContent != null ? new EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto
                    {
                        Id = ci.EducationalContent.Id,
                        SubChapterId = ci.EducationalContent.SubChapterId,
                        Title = ci.EducationalContent.Title,
                        Description = ci.EducationalContent.Description,
                        Type = ci.EducationalContent.Type,
                        TextContent = ci.EducationalContent.TextContent,
                        FileId = ci.EducationalContent.FileId,
                        ExternalUrl = ci.EducationalContent.ExternalUrl,
                        IsActive = ci.EducationalContent.IsActive,
                        Order = ci.EducationalContent.Order,
                        CreatedAt = ci.EducationalContent.CreatedAt,
                        UpdatedAt = ci.EducationalContent.UpdatedAt,
                        CreatedBy = ci.EducationalContent.CreatedBy
                    } : null,
                    InteractiveQuestion = ci.InteractiveQuestion != null ? new InteractiveQuestionDto
                    {
                        Id = ci.InteractiveQuestion.Id,
                        QuestionText = ci.InteractiveQuestion.QuestionText,
                        Description = ci.InteractiveQuestion.Description,
                        Type = ci.InteractiveQuestion.Type,
                        ImageFileId = ci.InteractiveQuestion.ImageFileId,
                        CorrectAnswer = ci.InteractiveQuestion.CorrectAnswer,
                        Points = ci.InteractiveQuestion.Points,
                        IsActive = ci.InteractiveQuestion.IsActive,
                        CreatedAt = ci.InteractiveQuestion.CreatedAt,
                        UpdatedAt = ci.InteractiveQuestion.UpdatedAt,
                        Choices = ci.InteractiveQuestion.Choices
                            .OrderBy(c => c.Order)
                            .Select(c => new QuestionChoiceDto
                            {
                                Id = c.Id,
                                InteractiveQuestionId = c.InteractiveQuestionId,
                                Text = c.Text,
                                IsCorrect = c.IsCorrect,
                                Order = c.Order,
                                CreatedAt = c.CreatedAt
                            }).ToList()
                    } : null
                }).ToList()
        }).ToList();

        return Result<List<InteractiveLessonDto>>.Success(dtos);
    }
}

public class GetInteractiveLessonsByClassQueryHandler : IRequestHandler<GetInteractiveLessonsByClassQuery, Result<List<InteractiveLessonDto>>>
{
    private readonly IRepository<Domain.Entities.InteractiveLessonAssignment> _assignmentRepository;

    public GetInteractiveLessonsByClassQueryHandler(IRepository<Domain.Entities.InteractiveLessonAssignment> assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<Result<List<InteractiveLessonDto>>> Handle(GetInteractiveLessonsByClassQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _assignmentRepository.GetAll()
            .Where(a => a.ClassId == request.ClassId && a.IsActive)
            .Include(a => a.InteractiveLesson)
                .ThenInclude(il => il.ContentItems)
                    .ThenInclude(ci => ci.EducationalContent)
            .Include(a => a.InteractiveLesson)
                .ThenInclude(il => il.ContentItems)
                    .ThenInclude(ci => ci.InteractiveQuestion!)
                        .ThenInclude(iq => iq.Choices)
            .ToListAsync(cancellationToken);

        var dtos = assignments.Select(assignment => new InteractiveLessonDto
        {
            Id = assignment.InteractiveLesson.Id,
            CourseId = assignment.InteractiveLesson.CourseId,
            Title = assignment.InteractiveLesson.Title,
            Description = assignment.InteractiveLesson.Description,
            IsActive = assignment.InteractiveLesson.IsActive,
            Order = assignment.InteractiveLesson.Order,
            CreatedAt = assignment.InteractiveLesson.CreatedAt,
            UpdatedAt = assignment.InteractiveLesson.UpdatedAt,
            CreatedBy = assignment.InteractiveLesson.CreatedBy,
            ContentItems = assignment.InteractiveLesson.ContentItems
                .Where(ci => ci.IsActive)
                .OrderBy(ci => ci.Order)
                .Select(ci => new InteractiveContentItemDto
                {
                    Id = ci.Id,
                    InteractiveLessonId = ci.InteractiveLessonId,
                    Order = ci.Order,
                    IsActive = ci.IsActive,
                    CreatedAt = ci.CreatedAt,
                    EducationalContentId = ci.EducationalContentId,
                    InteractiveQuestionId = ci.InteractiveQuestionId,
                    EducationalContent = ci.EducationalContent != null ? new EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto
                    {
                        Id = ci.EducationalContent.Id,
                        SubChapterId = ci.EducationalContent.SubChapterId,
                        Title = ci.EducationalContent.Title,
                        Description = ci.EducationalContent.Description,
                        Type = ci.EducationalContent.Type,
                        TextContent = ci.EducationalContent.TextContent,
                        FileId = ci.EducationalContent.FileId,
                        ExternalUrl = ci.EducationalContent.ExternalUrl,
                        IsActive = ci.EducationalContent.IsActive,
                        Order = ci.EducationalContent.Order,
                        CreatedAt = ci.EducationalContent.CreatedAt,
                        UpdatedAt = ci.EducationalContent.UpdatedAt,
                        CreatedBy = ci.EducationalContent.CreatedBy
                    } : null,
                    InteractiveQuestion = ci.InteractiveQuestion != null ? new InteractiveQuestionDto
                    {
                        Id = ci.InteractiveQuestion.Id,
                        QuestionText = ci.InteractiveQuestion.QuestionText,
                        Description = ci.InteractiveQuestion.Description,
                        Type = ci.InteractiveQuestion.Type,
                        ImageFileId = ci.InteractiveQuestion.ImageFileId,
                        CorrectAnswer = ci.InteractiveQuestion.CorrectAnswer,
                        Points = ci.InteractiveQuestion.Points,
                        IsActive = ci.InteractiveQuestion.IsActive,
                        CreatedAt = ci.InteractiveQuestion.CreatedAt,
                        UpdatedAt = ci.InteractiveQuestion.UpdatedAt,
                        Choices = ci.InteractiveQuestion.Choices
                            .OrderBy(c => c.Order)
                            .Select(c => new QuestionChoiceDto
                            {
                                Id = c.Id,
                                InteractiveQuestionId = c.InteractiveQuestionId,
                                Text = c.Text,
                                IsCorrect = c.IsCorrect,
                                Order = c.Order,
                                CreatedAt = c.CreatedAt
                            }).ToList()
                    } : null
                }).ToList()
        }).ToList();

        return Result<List<InteractiveLessonDto>>.Success(dtos);
    }
}

public class GetInteractiveLessonByIdQueryHandler : IRequestHandler<GetInteractiveLessonByIdQuery, Result<InteractiveLessonDto>>
{
    private readonly IRepository<Domain.Entities.InteractiveLesson> _interactiveLessonRepository;

    public GetInteractiveLessonByIdQueryHandler(IRepository<Domain.Entities.InteractiveLesson> interactiveLessonRepository)
    {
        _interactiveLessonRepository = interactiveLessonRepository;
    }

    public async Task<Result<InteractiveLessonDto>> Handle(GetInteractiveLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await _interactiveLessonRepository.GetAll()
            .Where(il => il.Id == request.Id)
            .Include(il => il.ContentItems)
                .ThenInclude(ci => ci.EducationalContent)
            .Include(il => il.ContentItems)
                .ThenInclude(ci => ci.InteractiveQuestion!)
                    .ThenInclude(iq => iq.Choices)
            .FirstOrDefaultAsync(cancellationToken);

        if (lesson == null)
        {
            return Result<InteractiveLessonDto>.Failure("Interactive lesson not found");
        }

        var dto = new InteractiveLessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Description = lesson.Description,
            IsActive = lesson.IsActive,
            Order = lesson.Order,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            CreatedBy = lesson.CreatedBy,
            ContentItems = lesson.ContentItems
                .Where(ci => ci.IsActive)
                .OrderBy(ci => ci.Order)
                .Select(ci => new InteractiveContentItemDto
                {
                    Id = ci.Id,
                    InteractiveLessonId = ci.InteractiveLessonId,
                    Order = ci.Order,
                    IsActive = ci.IsActive,
                    CreatedAt = ci.CreatedAt,
                    EducationalContentId = ci.EducationalContentId,
                    InteractiveQuestionId = ci.InteractiveQuestionId,
                    EducationalContent = ci.EducationalContent != null ? new EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto
                    {
                        Id = ci.EducationalContent.Id,
                        SubChapterId = ci.EducationalContent.SubChapterId,
                        Title = ci.EducationalContent.Title,
                        Description = ci.EducationalContent.Description,
                        Type = ci.EducationalContent.Type,
                        TextContent = ci.EducationalContent.TextContent,
                        FileId = ci.EducationalContent.FileId,
                        ExternalUrl = ci.EducationalContent.ExternalUrl,
                        IsActive = ci.EducationalContent.IsActive,
                        Order = ci.EducationalContent.Order,
                        CreatedAt = ci.EducationalContent.CreatedAt,
                        UpdatedAt = ci.EducationalContent.UpdatedAt,
                        CreatedBy = ci.EducationalContent.CreatedBy
                    } : null,
                    InteractiveQuestion = ci.InteractiveQuestion != null ? new InteractiveQuestionDto
                    {
                        Id = ci.InteractiveQuestion.Id,
                        QuestionText = ci.InteractiveQuestion.QuestionText,
                        Description = ci.InteractiveQuestion.Description,
                        Type = ci.InteractiveQuestion.Type,
                        ImageFileId = ci.InteractiveQuestion.ImageFileId,
                        CorrectAnswer = ci.InteractiveQuestion.CorrectAnswer,
                        Points = ci.InteractiveQuestion.Points,
                        IsActive = ci.InteractiveQuestion.IsActive,
                        CreatedAt = ci.InteractiveQuestion.CreatedAt,
                        UpdatedAt = ci.InteractiveQuestion.UpdatedAt,
                        Choices = ci.InteractiveQuestion.Choices
                            .OrderBy(c => c.Order)
                            .Select(c => new QuestionChoiceDto
                            {
                                Id = c.Id,
                                InteractiveQuestionId = c.InteractiveQuestionId,
                                Text = c.Text,
                                IsCorrect = c.IsCorrect,
                                Order = c.Order,
                                CreatedAt = c.CreatedAt
                            }).ToList()
                    } : null
                }).ToList()
        };

        return Result<InteractiveLessonDto>.Success(dto);
    }
}

public class GetAvailableEducationalContentQueryHandler : IRequestHandler<GetAvailableEducationalContentQuery, Result<List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto>>>
{
    private readonly IRepository<Domain.Entities.EducationalContent> _educationalContentRepository;
    private readonly IRepository<Course> _courseRepository;

    public GetAvailableEducationalContentQueryHandler(
        IRepository<Domain.Entities.EducationalContent> educationalContentRepository,
        IRepository<Course> courseRepository)
    {
        _educationalContentRepository = educationalContentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<Result<List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto>>> Handle(GetAvailableEducationalContentQuery request, CancellationToken cancellationToken)
    {
        var courseEntity = await _courseRepository.GetAll()
            .Include(c => c.Chapters)
                .ThenInclude(chapter => chapter.SubChapters)
            .FirstOrDefaultAsync(c => c.Id == request.CourseId, cancellationToken);

        if (courseEntity == null)
        {
            return Result<List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto>>.Failure("Course not found");
        }

        var subChapterIds = courseEntity.Chapters
            ?.SelectMany(chapter => chapter.SubChapters)
            ?.Select(subChapter => subChapter.Id)
            ?.ToList() ?? new List<int>();

        var educationalContents = await _educationalContentRepository.GetAll()
            .Where(ec => subChapterIds.Contains(ec.SubChapterId) && ec.IsActive)
            .OrderBy(ec => ec.Order)
            .ToListAsync(cancellationToken);

        var dtos = educationalContents.Select(ec => new EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto
        {
            Id = ec.Id,
            SubChapterId = ec.SubChapterId,
            Title = ec.Title,
            Description = ec.Description,
            Type = ec.Type,
            TextContent = ec.TextContent,
            FileId = ec.FileId,
            ExternalUrl = ec.ExternalUrl,
            IsActive = ec.IsActive,
            Order = ec.Order,
            CreatedAt = ec.CreatedAt,
            UpdatedAt = ec.UpdatedAt,
            CreatedBy = ec.CreatedBy
        }).ToList();

        return Result<List<EduTrack.Application.Features.InteractiveLesson.DTOs.EducationalContentDto>>.Success(dtos);
    }
}