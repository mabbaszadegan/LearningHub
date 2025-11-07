using System.Threading;
using System.Threading.Tasks;
using EduTrack.WebApp.Areas.Student.Models;
using EduTrack.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EduTrack.WebApp.ViewComponents;

public class StudentProfileSelectorViewComponent : ViewComponent
{
    private readonly IStudentProfileContext _studentProfileContext;
    private readonly ILogger<StudentProfileSelectorViewComponent> _logger;

    public StudentProfileSelectorViewComponent(
        IStudentProfileContext studentProfileContext,
        ILogger<StudentProfileSelectorViewComponent> logger)
    {
        _studentProfileContext = studentProfileContext;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Content(string.Empty);
        }

        if (!User.IsInRole("Student"))
        {
            return Content(string.Empty);
        }

        var profiles = await _studentProfileContext.GetProfilesForCurrentUserAsync(includeArchived, cancellationToken);
        var activeProfileId = await _studentProfileContext.GetActiveProfileIdAsync(cancellationToken);
        var activeProfileName = await _studentProfileContext.GetActiveProfileNameAsync(cancellationToken);

        var model = new StudentProfileSelectorViewModel
        {
            Profiles = profiles,
            ActiveProfileId = activeProfileId,
            ActiveProfileName = activeProfileName
        };

        return View(model);
    }
}

