namespace EduTrack.WebApp.Areas.Teacher.Views.Shared;

public class PageTitleSectionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? TitleIcon { get; set; }
    public string? Description { get; set; }
    public List<PageTitleBreadcrumbItem>? BreadcrumbItems { get; set; }
    public List<PageTitleAction>? Actions { get; set; }
}

public class PageTitleBreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = false;
}

public class PageTitleAction
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsModal { get; set; } = false;
    public string? ModalTarget { get; set; }
}
