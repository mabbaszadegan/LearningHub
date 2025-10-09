namespace EduTrack.WebApp.Views.Shared.Components.PageNavigation;

public class PageNavigationViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public List<NavigationAction> Actions { get; set; } = new();
}

public class NavigationAction
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public string? Icon { get; set; }
}
