using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Views.Shared.Components.SessionOverview;

public class SessionOverviewViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<OverviewItem> items)
    {
        var model = new SessionOverviewViewModel
        {
            Items = items
        };
        
        return View(model);
    }
}
