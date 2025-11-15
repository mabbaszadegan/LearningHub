using EduTrack.Application.Common.Models;

namespace EduTrack.Application.Common.Interfaces;

/// <summary>
/// Service for managing page title sections across the application
/// </summary>
public interface IPageTitleSectionService
{
    /// <summary>
    /// Creates a page title section for a specific page type
    /// </summary>
    Task<PageTitleSectionViewModel> CreatePageTitleSectionAsync(
        PageType pageType, 
        object? context = null, 
        string? baseUrl = null);
}

/// <summary>
/// Factory for creating page title sections
/// </summary>
public interface IPageTitleSectionFactory
{
    /// <summary>
    /// Creates a page title section based on page type and context
    /// </summary>
    Task<PageTitleSectionViewModel> CreateAsync(PageType pageType, object? context = null, string? baseUrl = null);
}

/// <summary>
/// Enum representing different page types in the teacher area
/// </summary>
public enum PageType
{
    // Home pages
    TeacherDashboard,
    
    // Course pages
    CoursesIndex,
    CourseCreate,
    CourseEdit,
    CourseDetails,
    
    // Teaching Plan pages
    TeachingPlansIndex,
    TeachingPlanCreate,
    TeachingPlanEdit,
    TeachingPlanDetails,
    
    // Schedule Item pages
    ScheduleItemsIndex,
    ScheduleItemCreate,
    ScheduleItemEdit,
    ScheduleItemDetails,
    
    // Student Group pages
    StudentGroupsIndex,
    StudentGroupCreate,
    StudentGroupEdit,
    StudentGroupDetails,
    StudentGroupManageMembers,
    
    // Chapter pages
    ChaptersIndex,
    ChapterCreate,
    ChapterEdit,
    ChapterDetails,
    
    // SubChapter pages
    SubChaptersIndex,
    SubChapterCreate,
    SubChapterEdit,
    SubChapterDetails,
    
    // Educational Content pages
    EducationalContentIndex,
    EducationalContentCreate,
    EducationalContentEdit,
    EducationalContentDetails,
    
    // Teaching Sessions pages
    TeachingSessionsIndex,
    TeachingSessionCreate,
    TeachingSessionEdit,
    TeachingSessionDetails,
    TeachingSessionDashboard,
    
    // Class pages
    ClassesIndex,
    ClassCreate,
    ClassEdit,
    ClassDetails,
    
    // Student pages
    StudentsIndex,
    StudentDetails,
    
    // Exam pages
    ExamsIndex,
    ExamCreate,
    ExamEdit,
    ExamDetails,
    
    // Progress pages
    ProgressIndex,
    ProgressDetails,
    
    // Default
    Default
}
