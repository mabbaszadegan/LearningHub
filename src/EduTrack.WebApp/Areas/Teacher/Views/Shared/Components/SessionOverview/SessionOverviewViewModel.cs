namespace EduTrack.WebApp.Areas.Teacher.Views.Shared.Components.SessionOverview;

public class SessionOverviewViewModel
{
    public List<OverviewItem> Items { get; set; } = new();
}

public class OverviewItem
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
