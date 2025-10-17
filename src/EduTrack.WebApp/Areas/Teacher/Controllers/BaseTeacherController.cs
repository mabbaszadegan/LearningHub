using EduTrack.Application.Common.Interfaces;
using EduTrack.Application.Common.Models;
using EduTrack.WebApp.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Controllers;

/// <summary>
/// Base controller for Teacher area with PageTitleSection support
/// </summary>
public abstract class BaseTeacherController : Controller
{
    private readonly IPageTitleSectionService _pageTitleSectionService;

    protected BaseTeacherController(IPageTitleSectionService pageTitleSectionService)
    {
        _pageTitleSectionService = pageTitleSectionService;
    }

    /// <summary>
    /// Sets up page title section for the current view
    /// </summary>
    protected async Task SetPageTitleSectionAsync(PageType pageType, object? context = null)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}/Teacher";
        var pageTitleSection = await _pageTitleSectionService.CreatePageTitleSectionAsync(pageType, context, baseUrl);
        ViewData["PageTitleSection"] = pageTitleSection;
    }

    /// <summary>
    /// Sets up page title section for the current view (synchronous version)
    /// </summary>
    protected void SetPageTitleSection(PageType pageType, object? context = null)
    {
        // For synchronous operations, we'll create a simple page title section
        var pageTitleSection = CreateSimplePageTitleSection(pageType, context);
        ViewData["PageTitleSection"] = pageTitleSection;
    }

    /// <summary>
    /// Creates a simple page title section for synchronous operations
    /// </summary>
    private PageTitleSectionViewModel CreateSimplePageTitleSection(PageType pageType, object? context)
    {
        return pageType switch
        {
            PageType.TeacherDashboard => new PageTitleSectionViewModel
            {
                Title = "داشبورد معلم",
                TitleIcon = "fas fa-tachometer-alt",
                Description = "نمای کلی از فعالیت‌ها و آمار آموزشی شما",
                BreadcrumbItems = new List<PageTitleBreadcrumbItem>
                {
                    PageTitleSectionHelper.CreateBreadcrumbItem("داشبورد", null, "fas fa-tachometer-alt", true)
                },
                Actions = new List<PageTitleAction>()
            },
            PageType.CoursesIndex => new PageTitleSectionViewModel
            {
                Title = "مدیریت دوره‌ها",
                TitleIcon = "fas fa-book",
                Description = "مدیریت و سازماندهی دوره‌های آموزشی شما",
                BreadcrumbItems = new List<PageTitleBreadcrumbItem>
                {
                    PageTitleSectionHelper.CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
                    PageTitleSectionHelper.CreateBreadcrumbItem("دوره‌ها", null, "fas fa-book", true)
                },
                Actions = new List<PageTitleAction>
                {
                    PageTitleSectionHelper.CreatePageAction("دوره جدید", Url.Action("Create") ?? "#", "btn-primary", "fas fa-plus")
                }
            },
            _ => new PageTitleSectionViewModel
            {
                Title = "صفحه",
                TitleIcon = "fas fa-file",
                Description = "صفحه",
                BreadcrumbItems = new List<PageTitleBreadcrumbItem>
                {
                    PageTitleSectionHelper.CreateBreadcrumbItem("خانه", Url.Action("Index", "Home"), "fas fa-home"),
                    PageTitleSectionHelper.CreateBreadcrumbItem("صفحه", null, "fas fa-file", true)
                },
                Actions = new List<PageTitleAction>()
            }
        };
    }
}
