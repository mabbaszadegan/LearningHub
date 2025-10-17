using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.Application.Features.Courses.Queries;
using EduTrack.Application.Features.TeachingPlan.Queries;
using MediatR;

namespace EduTrack.Application.Common.Services;

/// <summary>
/// Service for managing page title sections across the application
/// </summary>
public class PageTitleSectionService : IPageTitleSectionService
{
    private readonly IPageTitleSectionFactory _factory;

    public PageTitleSectionService(IPageTitleSectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<PageTitleSectionViewModel> CreatePageTitleSectionAsync(
        PageType pageType, 
        object? context = null, 
        string? baseUrl = null)
    {
        return await _factory.CreateAsync(pageType, context, baseUrl);
    }
}

/// <summary>
/// Factory for creating page title sections with specific configurations
/// </summary>
public class PageTitleSectionFactory : IPageTitleSectionFactory
{
    private readonly IMediator _mediator;

    public PageTitleSectionFactory(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<PageTitleSectionViewModel> CreateAsync(PageType pageType, object? context = null, string? baseUrl = null)
    {
        return pageType switch
        {
            // Home pages
            PageType.TeacherDashboard => CreateTeacherDashboardPage(baseUrl),
            
            // Course pages
            PageType.CoursesIndex => CreateCoursesIndexPage(baseUrl),
            PageType.CourseCreate => CreateCourseCreatePage(baseUrl),
            PageType.CourseEdit => await CreateCourseEditPage(context, baseUrl),
            PageType.CourseDetails => await CreateCourseDetailsPage(context, baseUrl),
            
            // Teaching Plan pages
            PageType.TeachingPlansIndex => await CreateTeachingPlansIndexPage(context, baseUrl),
            PageType.TeachingPlanCreate => await CreateTeachingPlanCreatePage(context, baseUrl),
            PageType.TeachingPlanEdit => await CreateTeachingPlanEditPage(context, baseUrl),
            PageType.TeachingPlanDetails => await CreateTeachingPlanDetailsPage(context, baseUrl),
            
            // Schedule Item pages
            PageType.ScheduleItemsIndex => await CreateScheduleItemsIndexPage(context, baseUrl),
            PageType.ScheduleItemCreate => await CreateScheduleItemCreatePage(context, baseUrl),
            PageType.ScheduleItemEdit => await CreateScheduleItemEditPage(context, baseUrl),
            PageType.ScheduleItemDetails => CreateScheduleItemDetailsPage(context, baseUrl),
            
            // Student Group pages
            PageType.StudentGroupsIndex => await CreateStudentGroupsIndexPage(context, baseUrl),
            PageType.StudentGroupCreate => await CreateStudentGroupCreatePage(context, baseUrl),
            PageType.StudentGroupEdit => CreateStudentGroupEditPage(context, baseUrl),
            PageType.StudentGroupDetails => CreateStudentGroupDetailsPage(context, baseUrl),
            PageType.StudentGroupManageMembers => CreateStudentGroupManageMembersPage(context, baseUrl),
            
            // Chapter pages
            PageType.ChaptersIndex => await CreateChaptersIndexPage(context, baseUrl),
            PageType.ChapterCreate => await CreateChapterCreatePage(context, baseUrl),
            PageType.ChapterEdit => CreateChapterEditPage(context, baseUrl),
            PageType.ChapterDetails => CreateChapterDetailsPage(context, baseUrl),
            
            // SubChapter pages
            PageType.SubChaptersIndex => CreateSubChaptersIndexPage(context, baseUrl),
            PageType.SubChapterCreate => CreateSubChapterCreatePage(context, baseUrl),
            PageType.SubChapterEdit => CreateSubChapterEditPage(context, baseUrl),
            PageType.SubChapterDetails => CreateSubChapterDetailsPage(context, baseUrl),
            
            // Educational Content pages
            PageType.EducationalContentIndex => CreateEducationalContentIndexPage(context, baseUrl),
            PageType.EducationalContentCreate => CreateEducationalContentCreatePage(context, baseUrl),
            PageType.EducationalContentEdit => CreateEducationalContentEditPage(context, baseUrl),
            PageType.EducationalContentDetails => CreateEducationalContentDetailsPage(context, baseUrl),
            
            // Teaching Sessions pages
            PageType.TeachingSessionsIndex => await CreateTeachingSessionsIndexPage(context, baseUrl),
            PageType.TeachingSessionCreate => await CreateTeachingSessionCreatePage(context, baseUrl),
            PageType.TeachingSessionEdit => CreateTeachingSessionEditPage(context, baseUrl),
            PageType.TeachingSessionDetails => CreateTeachingSessionDetailsPage(context, baseUrl),
            PageType.TeachingSessionDashboard => CreateTeachingSessionDashboardPage(baseUrl),
            
            // Class pages
            PageType.ClassesIndex => CreateClassesIndexPage(baseUrl),
            PageType.ClassCreate => CreateClassCreatePage(baseUrl),
            PageType.ClassEdit => CreateClassEditPage(context, baseUrl),
            PageType.ClassDetails => CreateClassDetailsPage(context, baseUrl),
            
            // Default fallback
            _ => CreateDefaultPage(baseUrl)
        };
    }

    #region Helper Methods
    private PageTitleBreadcrumbItem CreateBreadcrumbItem(string text, string? url = null, string? icon = null, bool isActive = false)
    {
        return new PageTitleBreadcrumbItem
        {
            Text = text,
            Url = url,
            Icon = icon,
            IsActive = isActive
        };
    }

    private PageTitleAction CreatePageAction(string text, string url, string cssClass = "btn-primary", string? icon = null, bool isModal = false, string? modalTarget = null)
    {
        return new PageTitleAction
        {
            Text = text,
            Url = url,
            CssClass = cssClass,
            Icon = icon,
            IsModal = isModal,
            ModalTarget = modalTarget
        };
    }
    #endregion

    #region Home Pages
    private PageTitleSectionViewModel CreateTeacherDashboardPage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "داشبورد معلم",
            TitleIcon = "fas fa-tachometer-alt",
            Description = "نمای کلی از فعالیت‌ها و آمار آموزشی شما",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("داشبورد", null, "fas fa-tachometer-alt", true)
            },
            Actions = new List<PageTitleAction>()
        };
    }
    #endregion

    #region Course Pages
    private PageTitleSectionViewModel CreateCoursesIndexPage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "مدیریت دوره‌ها",
            TitleIcon = "fas fa-book",
            Description = "مدیریت و سازماندهی دوره‌های آموزشی شما",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", null, "fas fa-book", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("دوره جدید", $"{baseUrl}/Courses/Create", "btn-primary", "fas fa-plus")
            }
        };
    }

    private PageTitleSectionViewModel CreateCourseCreatePage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "ایجاد دوره جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = "ایجاد دوره آموزشی جدید",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem("ایجاد دوره جدید", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Courses", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateCourseEditPage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش دوره: {course.Value.Title}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش اطلاعات دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, null, "fas fa-graduation-cap", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Courses", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateCourseDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = course.Value.Title,
            TitleIcon = "fas fa-graduation-cap",
            Description = $"جزئیات دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, null, "fas fa-graduation-cap", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش", $"{baseUrl}/Courses/Edit/{courseId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/Courses", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Teaching Plan Pages
    private async Task<PageTitleSectionViewModel> CreateTeachingPlansIndexPage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = $"برنامه‌های آموزشی: {course.Value.Title}",
            TitleIcon = "fas fa-calendar-alt",
            Description = $"مدیریت برنامه‌های آموزشی برای دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, null, "fas fa-graduation-cap", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("برنامه جدید", $"{baseUrl}/TeachingPlan/Create?courseId={courseId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateTeachingPlanCreatePage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد برنامه آموزشی جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد برنامه آموزشی جدید برای دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("ایجاد برنامه آموزشی", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingPlan?courseId={courseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateTeachingPlanEditPage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش برنامه آموزشی: {teachingPlan.Value.Title}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش برنامه آموزشی \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(teachingPlan.Value.CourseTitle ?? "دوره", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, null, "fas fa-calendar-alt", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateTeachingPlanDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = teachingPlan.Value.Title,
            TitleIcon = "fas fa-calendar-alt",
            Description = $"جزئیات برنامه آموزشی \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(teachingPlan.Value.CourseTitle ?? "دوره", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, null, "fas fa-calendar-alt", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش", $"{baseUrl}/TeachingPlan/Edit/{teachingPlanId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("آیتم‌های آموزشی", $"{baseUrl}/ScheduleItem?teachingPlanId={teachingPlanId}", "btn-success", "fas fa-tasks"),
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Schedule Item Pages
    private async Task<PageTitleSectionViewModel> CreateScheduleItemsIndexPage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "مدیریت آیتم‌های آموزشی",
            TitleIcon = "fas fa-tasks",
            Description = $"مدیریت و سازماندهی آیتم‌های آموزشی برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(teachingPlan.Value.CourseTitle ?? "دوره", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, null, "fas fa-calendar-alt"),
                CreateBreadcrumbItem("مدیریت آیتم‌های آموزشی", null, "fas fa-tasks", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("آیتم جدید", $"{baseUrl}/ScheduleItem/CreateOrEdit?teachingPlanId={teachingPlanId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateScheduleItemCreatePage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد آیتم آموزشی جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد آیتم آموزشی جدید برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(teachingPlan.Value.CourseTitle ?? "دوره", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("آیتم‌های آموزشی", $"{baseUrl}/ScheduleItem?teachingPlanId={teachingPlanId}", "fas fa-tasks"),
                CreateBreadcrumbItem("ایجاد آیتم آموزشی جدید", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/ScheduleItem?teachingPlanId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateScheduleItemEditPage(object? context, string? baseUrl)
    {
        if (context is not (int teachingPlanId, int scheduleItemId)) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ویرایش آیتم آموزشی",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش آیتم آموزشی برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(teachingPlan.Value.CourseTitle ?? "دوره", $"{baseUrl}/TeachingPlan?courseId={teachingPlan.Value.CourseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("آیتم‌های آموزشی", $"{baseUrl}/ScheduleItem?teachingPlanId={teachingPlanId}", "fas fa-tasks"),
                CreateBreadcrumbItem("ویرایش آیتم آموزشی", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/ScheduleItem?teachingPlanId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateScheduleItemDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int scheduleItemId) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "جزئیات آیتم آموزشی",
            TitleIcon = "fas fa-info-circle",
            Description = "جزئیات آیتم آموزشی",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("جزئیات آیتم آموزشی", null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش", $"{baseUrl}/ScheduleItem/Edit/{scheduleItemId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/ScheduleItem", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Student Group Pages
    private async Task<PageTitleSectionViewModel> CreateStudentGroupsIndexPage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "گروه‌های دانش‌آموزی",
            TitleIcon = "fas fa-users",
            Description = $"مدیریت گروه‌های دانش‌آموزی برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{course.Value.Id}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={course.Value.Id}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("گروه‌های دانش‌آموزی", null, "fas fa-users", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("گروه جدید", $"{baseUrl}/StudentGroup/Create?planId={teachingPlanId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateStudentGroupCreatePage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد گروه دانش‌آموزی جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد گروه دانش‌آموزی جدید برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{course.Value.Id}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={course.Value.Id}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("گروه‌های دانش‌آموزی", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "fas fa-users"),
                CreateBreadcrumbItem("ایجاد گروه", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateStudentGroupEditPage(object? context, string? baseUrl)
    {
        if (context is not int groupId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the group details here
        string groupTitle = "گروه دانش‌آموزی"; // Placeholder
        int teachingPlanId = 0; // Placeholder
        string teachingPlanTitle = "برنامه آموزشی"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش گروه دانش‌آموزی: {groupTitle}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات گروه دانش‌آموزی \"{groupTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlanTitle, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("گروه‌های دانش‌آموزی", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "fas fa-users"),
                CreateBreadcrumbItem($"ویرایش {groupTitle}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateStudentGroupDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int groupId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the group details here
        string groupTitle = "جزئیات گروه دانش‌آموزی"; // Placeholder
        int teachingPlanId = 0; // Placeholder
        string teachingPlanTitle = "برنامه آموزشی"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات گروه دانش‌آموزی: {groupTitle}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات و مدیریت اعضای گروه \"{groupTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlanTitle, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("گروه‌های دانش‌آموزی", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "fas fa-users"),
                CreateBreadcrumbItem(groupTitle, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("مدیریت اعضا", $"{baseUrl}/StudentGroup/ManageMembers/{groupId}", "btn-primary", "fas fa-user-cog"),
                CreatePageAction("ویرایش گروه", $"{baseUrl}/StudentGroup/Edit/{groupId}", "btn-info", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateStudentGroupManageMembersPage(object? context, string? baseUrl)
    {
        if (context is not int groupId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the group details here
        string groupTitle = "مدیریت اعضای گروه"; // Placeholder
        int teachingPlanId = 0; // Placeholder
        string teachingPlanTitle = "برنامه آموزشی"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"مدیریت اعضای گروه: {groupTitle}",
            TitleIcon = "fas fa-user-cog",
            Description = $"مدیریت و سازماندهی اعضای گروه دانش‌آموزی \"{groupTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlanTitle, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("گروه‌های دانش‌آموزی", $"{baseUrl}/StudentGroup?planId={teachingPlanId}", "fas fa-users"),
                CreateBreadcrumbItem(groupTitle, $"{baseUrl}/StudentGroup/Details/{groupId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("مدیریت اعضا", null, "fas fa-user-cog", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/StudentGroup/Details/{groupId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Chapter Pages
    private async Task<PageTitleSectionViewModel> CreateChaptersIndexPage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = $"مباحث دوره: {course.Value.Title}",
            TitleIcon = "fas fa-list-alt",
            Description = $"مدیریت و سازماندهی مباحث دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", null, "fas fa-list-alt", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("مبحث جدید", $"{baseUrl}/Chapters/Create?courseId={courseId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateChapterCreatePage(object? context, string? baseUrl)
    {
        if (context is not int courseId) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(courseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد مبحث جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد مبحث جدید برای دوره \"{course.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem("ایجاد مبحث", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Chapters?courseId={courseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateChapterEditPage(object? context, string? baseUrl)
    {
        if (context is not int chapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the chapter details here
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش مبحث: {chapterTitle}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات مبحث \"{chapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem($"ویرایش {chapterTitle}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Chapters?courseId={courseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateChapterDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int chapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the chapter details here
        string chapterTitle = "جزئیات مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات مبحث: {chapterTitle}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات و مدیریت زیرمباحث مبحث \"{chapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "btn-primary", "fas fa-list"),
                CreatePageAction("ویرایش مبحث", $"{baseUrl}/Chapters/Edit/{chapterId}", "btn-info", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/Chapters?courseId={courseId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region SubChapter Pages
    private PageTitleSectionViewModel CreateSubChaptersIndexPage(object? context, string? baseUrl)
    {
        if (context is not int chapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the chapter details here
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"زیرمباحث مبحث: {chapterTitle}",
            TitleIcon = "fas fa-list",
            Description = $"مدیریت و سازماندهی زیرمباحث مبحث \"{chapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", null, "fas fa-list", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("زیرمبحث جدید", $"{baseUrl}/SubChapters/Create?chapterId={chapterId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private PageTitleSectionViewModel CreateSubChapterCreatePage(object? context, string? baseUrl)
    {
        if (context is not int chapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the chapter details here
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد زیرمبحث جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد زیرمبحث جدید برای مبحث \"{chapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem("ایجاد زیرمبحث", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/SubChapters?chapterId={chapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateSubChapterEditPage(object? context, string? baseUrl)
    {
        if (context is not int subChapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the subchapter details here
        string subChapterTitle = "زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش زیرمبحث: {subChapterTitle}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات زیرمبحث \"{subChapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem($"ویرایش {subChapterTitle}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/SubChapters?chapterId={chapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateSubChapterDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int subChapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the subchapter details here
        string subChapterTitle = "جزئیات زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات زیرمبحث: {subChapterTitle}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات و مدیریت محتوای آموزشی زیرمبحث \"{subChapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem(subChapterTitle, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("محتوای آموزشی", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "btn-primary", "fas fa-book"),
                CreatePageAction("ویرایش زیرمبحث", $"{baseUrl}/SubChapters/Edit/{subChapterId}", "btn-info", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/SubChapters?chapterId={chapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Educational Content Pages
    private PageTitleSectionViewModel CreateEducationalContentIndexPage(object? context, string? baseUrl)
    {
        if (context is not int subChapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the subchapter details here
        string subChapterTitle = "زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"مدیریت محتوای آموزشی: {subChapterTitle}",
            TitleIcon = "fas fa-book-open",
            Description = $"مدیریت و سازماندهی محتوای آموزشی زیرمبحث \"{subChapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem(subChapterTitle, $"{baseUrl}/SubChapters/Details/{subChapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("محتوای آموزشی", null, "fas fa-book-open", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("محتوای جدید", $"{baseUrl}/EducationalContent/Create?subChapterId={subChapterId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private PageTitleSectionViewModel CreateEducationalContentCreatePage(object? context, string? baseUrl)
    {
        if (context is not int subChapterId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the subchapter details here
        string subChapterTitle = "زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد محتوای آموزشی جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد محتوای آموزشی جدید برای زیرمبحث \"{subChapterTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem(subChapterTitle, $"{baseUrl}/SubChapters/Details/{subChapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("محتوای آموزشی", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "fas fa-book-open"),
                CreateBreadcrumbItem("ایجاد محتوا", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateEducationalContentEditPage(object? context, string? baseUrl)
    {
        if (context is not int contentId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the content details here
        string contentTitle = "محتوای آموزشی"; // Placeholder
        int subChapterId = 0; // Placeholder
        string subChapterTitle = "زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش محتوای آموزشی: {contentTitle}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات محتوای آموزشی \"{contentTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem(subChapterTitle, $"{baseUrl}/SubChapters/Details/{subChapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("محتوای آموزشی", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "fas fa-book-open"),
                CreateBreadcrumbItem($"ویرایش {contentTitle}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateEducationalContentDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int contentId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the content details here
        string contentTitle = "جزئیات محتوای آموزشی"; // Placeholder
        int subChapterId = 0; // Placeholder
        string subChapterTitle = "زیرمبحث"; // Placeholder
        int chapterId = 0; // Placeholder
        string chapterTitle = "مبحث"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات محتوای آموزشی: {contentTitle}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات محتوای آموزشی \"{contentTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("مباحث", $"{baseUrl}/Chapters?courseId={courseId}", "fas fa-list-alt"),
                CreateBreadcrumbItem(chapterTitle, $"{baseUrl}/Chapters/Details/{chapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("زیرمباحث", $"{baseUrl}/SubChapters?chapterId={chapterId}", "fas fa-list"),
                CreateBreadcrumbItem(subChapterTitle, $"{baseUrl}/SubChapters/Details/{subChapterId}", "fas fa-info-circle"),
                CreateBreadcrumbItem("محتوای آموزشی", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "fas fa-book-open"),
                CreateBreadcrumbItem(contentTitle, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش محتوا", $"{baseUrl}/EducationalContent/Edit/{contentId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/EducationalContent?subChapterId={subChapterId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Teaching Sessions Pages
    private async Task<PageTitleSectionViewModel> CreateTeachingSessionsIndexPage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = $"جلسات آموزشی: {teachingPlan.Value.Title}",
            TitleIcon = "fas fa-chalkboard-teacher",
            Description = $"مدیریت و سازماندهی جلسات آموزشی برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{course.Value.Id}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={course.Value.Id}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("جلسات آموزشی", null, "fas fa-chalkboard-teacher", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("جلسه جدید", $"{baseUrl}/TeachingSessions/Create?planId={teachingPlanId}", "btn-primary", "fas fa-plus")
            }
        };
    }

    private async Task<PageTitleSectionViewModel> CreateTeachingSessionCreatePage(object? context, string? baseUrl)
    {
        if (context is not int teachingPlanId) return CreateDefaultPage(baseUrl);

        var teachingPlan = await _mediator.Send(new GetTeachingPlanByIdQuery(teachingPlanId));
        if (!teachingPlan.IsSuccess || teachingPlan.Value == null) return CreateDefaultPage(baseUrl);

        var course = await _mediator.Send(new GetCourseByIdQuery(teachingPlan.Value.CourseId));
        if (!course.IsSuccess || course.Value == null) return CreateDefaultPage(baseUrl);

        return new PageTitleSectionViewModel
        {
            Title = "ایجاد جلسه آموزشی جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = $"ایجاد جلسه آموزشی جدید برای برنامه \"{teachingPlan.Value.Title}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(course.Value.Title, $"{baseUrl}/Courses/Details/{course.Value.Id}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={course.Value.Id}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlan.Value.Title, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("جلسات آموزشی", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "fas fa-chalkboard-teacher"),
                CreateBreadcrumbItem("ایجاد جلسه", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateTeachingSessionEditPage(object? context, string? baseUrl)
    {
        if (context is not int sessionId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the session details here
        string sessionTitle = "جلسه آموزشی"; // Placeholder
        int teachingPlanId = 0; // Placeholder
        string teachingPlanTitle = "برنامه آموزشی"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش جلسه آموزشی: {sessionTitle}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات جلسه آموزشی \"{sessionTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlanTitle, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("جلسات آموزشی", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "fas fa-chalkboard-teacher"),
                CreateBreadcrumbItem($"ویرایش {sessionTitle}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateTeachingSessionDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int sessionId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the session details here
        string sessionTitle = "جزئیات جلسه آموزشی"; // Placeholder
        int teachingPlanId = 0; // Placeholder
        string teachingPlanTitle = "برنامه آموزشی"; // Placeholder
        int courseId = 0; // Placeholder
        string courseTitle = "دوره"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات جلسه آموزشی: {sessionTitle}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات جلسه آموزشی \"{sessionTitle}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("دوره‌ها", $"{baseUrl}/Courses", "fas fa-book"),
                CreateBreadcrumbItem(courseTitle, $"{baseUrl}/Courses/Details/{courseId}", "fas fa-graduation-cap"),
                CreateBreadcrumbItem("برنامه‌های آموزشی", $"{baseUrl}/TeachingPlan?courseId={courseId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem(teachingPlanTitle, $"{baseUrl}/TeachingPlan/Details/{teachingPlanId}", "fas fa-calendar-alt"),
                CreateBreadcrumbItem("جلسات آموزشی", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "fas fa-chalkboard-teacher"),
                CreateBreadcrumbItem(sessionTitle, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش جلسه", $"{baseUrl}/TeachingSessions/Edit/{sessionId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/TeachingSessions?planId={teachingPlanId}", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateTeachingSessionDashboardPage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "داشبورد جلسات آموزشی",
            TitleIcon = "fas fa-chalkboard-teacher",
            Description = "بررسی کلی جلسات و فعالیت‌های آموزشی",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("داشبورد جلسات", null, "fas fa-chalkboard-teacher", true)
            },
            Actions = new List<PageTitleAction>()
        };
    }
    #endregion

    #region Class Pages
    private PageTitleSectionViewModel CreateClassesIndexPage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "مدیریت کلاس‌ها",
            TitleIcon = "fas fa-users",
            Description = "مدیریت و سازماندهی کلاس‌های آموزشی شما",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("کلاس‌ها", null, "fas fa-users", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("کلاس جدید", $"{baseUrl}/Classes/Create", "btn-primary", "fas fa-plus")
            }
        };
    }

    private PageTitleSectionViewModel CreateClassCreatePage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "ایجاد کلاس جدید",
            TitleIcon = "fas fa-plus-circle",
            Description = "ایجاد کلاس آموزشی جدید",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("کلاس‌ها", $"{baseUrl}/Classes", "fas fa-users"),
                CreateBreadcrumbItem("ایجاد کلاس", null, "fas fa-plus", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Classes", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateClassEditPage(object? context, string? baseUrl)
    {
        if (context is not int classId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the class details here
        string className = "کلاس"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"ویرایش کلاس: {className}",
            TitleIcon = "fas fa-edit",
            Description = $"ویرایش جزئیات کلاس \"{className}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("کلاس‌ها", $"{baseUrl}/Classes", "fas fa-users"),
                CreateBreadcrumbItem($"ویرایش {className}", null, "fas fa-edit", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("بازگشت", $"{baseUrl}/Classes", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }

    private PageTitleSectionViewModel CreateClassDetailsPage(object? context, string? baseUrl)
    {
        if (context is not int classId) return CreateDefaultPage(baseUrl);

        // In a real scenario, you would fetch the class details here
        string className = "جزئیات کلاس"; // Placeholder

        return new PageTitleSectionViewModel
        {
            Title = $"جزئیات کلاس: {className}",
            TitleIcon = "fas fa-info-circle",
            Description = $"مشاهده جزئیات و مدیریت دانش‌آموزان کلاس \"{className}\"",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("کلاس‌ها", $"{baseUrl}/Classes", "fas fa-users"),
                CreateBreadcrumbItem(className, null, "fas fa-info-circle", true)
            },
            Actions = new List<PageTitleAction>
            {
                CreatePageAction("ویرایش کلاس", $"{baseUrl}/Classes/Edit/{classId}", "btn-primary", "fas fa-edit"),
                CreatePageAction("بازگشت", $"{baseUrl}/Classes", "btn-secondary", "fas fa-arrow-right")
            }
        };
    }
    #endregion

    #region Default
    private PageTitleSectionViewModel CreateDefaultPage(string? baseUrl)
    {
        return new PageTitleSectionViewModel
        {
            Title = "صفحه",
            TitleIcon = "fas fa-file",
            Description = "صفحه",
            BreadcrumbItems = new List<PageTitleBreadcrumbItem>
            {
                CreateBreadcrumbItem("خانه", $"{baseUrl}/Home", "fas fa-home"),
                CreateBreadcrumbItem("صفحه", null, "fas fa-file", true)
            },
            Actions = new List<PageTitleAction>()
        };
    }
    #endregion
}