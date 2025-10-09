namespace EduTrack.WebApp.Areas.Teacher.Views.Shared.Components.PageNavigation;

public class PageNavigationViewModel
{
    public string Title { get; set; } = string.Empty;
    public List<BreadcrumbItem> BreadcrumbItems { get; set; } = new();
    public List<NavigationAction> Actions { get; set; } = new();
}

public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = false;
}

public class NavigationAction
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
