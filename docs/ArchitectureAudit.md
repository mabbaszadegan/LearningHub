# EduTrack Architecture & Frontend Audit — 2025-11-15

## Summary
| # | Issue | Layer | Severity |
|---|-------|-------|----------|
| 1 | WebApp controllers call `AppDbContext` and Domain entities directly, bypassing Application layer abstractions | WebApp | High |
| 2 | Runtime auto-migrations and heavy seeding inside `Program.cs` risk startup instability and mix infra concerns into presentation layer | WebApp | High |
| 3 | Public uploads pipeline enables `ServeUnknownFileTypes` without validation, allowing arbitrary file downloads | WebApp | High |
| 4 | `Study.cshtml` embeds inline `<script>` and large `@functions` block, violating view rules and duplicating business logic | WebApp Frontend | Medium |
| 5 | `study-page.js` is a 500+ line monolith tied to global DOM state with ad-hoc event handling | WebApp Frontend | Medium |
| 6 | Duplicate client/server shaping of schedule data bloats payloads and makes filtering logic inconsistent | WebApp Frontend | Medium |

---

### 1. Controllers bypass the Application layer
The Admin controllers construct aggregates and hit EF Core directly, mixing presentation with domain/data access responsibilities and making cross-cutting policies (validation, logging, caching) impossible to reuse.

```15:145:src/EduTrack.WebApp/Areas/Admin/Controllers/CoursesController.cs
    private readonly AppDbContext _context;
    ...
    var course = Course.Create(...);
    _context.Courses.Add(course);
    await _context.SaveChangesAsync();
```

**Impact**: Tight coupling to Infrastructure, duplicated transactional logic, hard-to-test controllers, and future inability to move to APIs/microservices.

**Remediation**:
- Introduce Application-layer command/query handlers (e.g., MediatR) that encapsulate course creation, status toggling, and logging.
- Replace direct `_context` usage with `ICourseService`/`ICourseRepository` abstractions registered in `EduTrack.Application`.
- Map controller DTOs to application commands and keep controllers thin (input validation + response formatting only).

---

### 2. Runtime migrations & seeding inside `Program.cs`
`Program.cs` opens connections, applies migrations, and seeds demo data during every startup.

```176:205:src/EduTrack.WebApp/Program.cs
if (autoMigrate)
{
    await context.Database.MigrateAsync();
}
await SeedData.InitializeAsync(context, userManager, roleManager);
```

**Impact**: Web front-end now owns database lifecycle. Startup can crash under transient DB faults, and there is no way to run the app without full seed data. This also breaks the layered rule that WebApp should not depend on Infrastructure internals.

**Remediation**:
- Move migrations/seeding to dedicated CLI or background job (e.g., `dotnet ef database update`, hosted service gated by environment, or deployment pipeline step).
- Keep WebApp startup focused on presentation concerns; inject only application services.
- Wrap any remaining initialization in resilience policies with cancellation tokens if absolutely needed.

---

### 3. Static uploads allow arbitrary file types
Uploads directory is exposed with `ServeUnknownFileTypes = true` and minimal header hardening.

```134:150:src/EduTrack.WebApp/Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".mp3", ...))
```

**Impact**: Attackers can upload/serve executable files or HTML, leading to XSS or malware distribution. There is no authorization check or MIME validation.

**Remediation**:
- Disable `ServeUnknownFileTypes` and whitelist explicit content types (audio/video/image).
- Route downloads through a controller that enforces ownership/authorization and sets safe headers.
- Store uploads outside `wwwroot` and stream them via FileResult endpoints with antivirus scanning.

---

### 4. Inline scripts and business logic inside view
`Study.cshtml` injects JSON via inline `<script>` and hosts a large `@functions` block duplicating DTO logic (status, color, formatting), violating the “no inline script/style” rule.

```241:333:src/EduTrack.WebApp/Areas/Student/Views/Course/Study.cshtml
@section Scripts {
    <script>
        window.studyPageConfig = { ... };
    </script>
    <script src="~/js/areas/student/course/study-page.js" asp-append-version="true"></script>
}
```

```262:358:src/EduTrack.WebApp/Areas/Student/Views/Course/Study.cshtml
@functions {
    string GetItemStatus(...)
    string GetItemTypeColor(...)
    string FormatTime(...)
```

**Impact**: Hard to test, lots of duplicated calculations versus Application DTOs, and CSP/inlining rules are broken—blocking security headers adoption.

**Remediation**:
- Move the `window.studyPageConfig` hydration into `wwwroot/js/areas/student/course/study-page.bootstrap.js`.
- Relocate helper logic into dedicated ViewModels or AutoMapper DTO projections so Razor remains markup-only.
- Expose localized strings and enums through resource files or application services instead of Razor helpers.

---

### 5. `study-page.js` monolith w/out modular lifecycle
The single file (~530 lines) handles state, DOM queries, filtering, sorting, and teardown manually without modules, namespaces, or reusable utilities.

```6:205:src/EduTrack.WebApp/wwwroot/js/areas/student/course/study-page.js
class StudyPage {
    constructor() {
        this.config = window.studyPageConfig || {};
        ...
        this.bindEvents();
        this.initializeCategoryTabs();
        this.initializeFilters();
        this.initializeSorting();
```

**Impact**: Difficult to reason about, no unit tests, and every event listener is re-bound whenever the page is re-rendered. Any future requirement (e.g., real-time updates) will require a full rewrite.

**Remediation**:
- Split into smaller ES modules (e.g., `sidebar.js`, `filters.js`, `sorting.js`) and bundle with Vite/Webpack.
- Replace manual DOM traversal with data attributes + delegated events, and encapsulate state in a store (e.g., Alpine/Stimulus).
- Provide explicit `init/destroy` hooks per module tested via Jest or Playwright.

---

### 6. Duplicated schedule shaping between Razor and JS
Schedule items are serialized twice: Razor builds `categories`, calculates stats, and also emits the full `scheduleItems` collection for JS filtering, replicating status/category logic on both sides.

```18:233:src/EduTrack.WebApp/Areas/Student/Views/Course/Study.cshtml
var categories = new List<(string Key, string Label, int Count)>();
...
<a ... data-category="@category" data-subchapter-ids="...">
```

```241:257:src/EduTrack.WebApp/Areas/Student/Views/Course/Study.cshtml
window.studyPageConfig = {
    scheduleItems: @Html.Raw(JsonSerializer.Serialize(...)),
    chapters: ...
};
```

**Impact**: Page payload grows with every schedule item, and data drift occurs when server helpers (e.g., `GetItemCategory`) disagree with `study-page.js` filtering. Any change must be replicated in two places, increasing bugs.

**Remediation**:
- Move filtering/sorting to the client completely by returning a compact JSON payload (API endpoint) and rendering cards via a JS template, or move it server-side (paged Razor components) but avoid redundant hydration.
- Define shared DTOs in Application layer that already contain computed fields (category, colors, stats) so both server and client rely on the same contract.
- Apply pagination and lazy-loading to reduce DOM size and initial JS config.

