using Microsoft.AspNetCore.Mvc;
using EduTrack.WebApp.Areas.Teacher.Views.Shared;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EduTrack.WebApp.Extensions;

public static class PageTitleSectionExtensions
{
    /// <summary>
    /// Sets up the page title section data for the current view
    /// </summary>
    public static void SetPageTitleSection(this Controller controller, 
        string title, 
        string? titleIcon = null, 
        string? description = null, 
        List<PageTitleBreadcrumbItem>? breadcrumbItems = null, 
        List<PageTitleAction>? actions = null)
    {
        var viewModel = new PageTitleSectionViewModel
        {
            Title = title,
            TitleIcon = titleIcon,
            Description = description,
            BreadcrumbItems = breadcrumbItems ?? new List<PageTitleBreadcrumbItem>(),
            Actions = actions ?? new List<PageTitleAction>()
        };

        controller.ViewData["PageTitleSection"] = viewModel;
    }

    /// <summary>
    /// Gets the page title section view model from ViewData
    /// </summary>
    public static PageTitleSectionViewModel? GetPageTitleSection(this ViewDataDictionary viewData)
    {
        return viewData["PageTitleSection"] as PageTitleSectionViewModel;
    }
}

/// <summary>
/// Helper class for creating PageTitleSection components
/// </summary>
public static class PageTitleSectionHelper
{
    /// <summary>
    /// Creates a breadcrumb item
    /// </summary>
    public static PageTitleBreadcrumbItem CreateBreadcrumbItem(string text, string? url = null, string? icon = null, bool isActive = false)
    {
        return new PageTitleBreadcrumbItem
        {
            Text = text,
            Url = url,
            Icon = icon,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Creates a page action button
    /// </summary>
    public static PageTitleAction CreatePageAction(string text, string url, string cssClass = "btn-primary", string? icon = null, bool isModal = false, string? modalTarget = null)
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
}
