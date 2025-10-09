using Microsoft.AspNetCore.Mvc;

namespace EduTrack.WebApp.Areas.Teacher.Views.Shared.Components.PageNavigation;

public class PageNavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string title, List<BreadcrumbItem>? breadcrumbItems = null, List<NavigationAction>? actions = null)
    {
        var model = new PageNavigationViewModel
        {
            Title = title,
            BreadcrumbItems = breadcrumbItems ?? new List<BreadcrumbItem>(),
            Actions = actions ?? new List<NavigationAction>()
        };
        
        return View(model);
    }
}
