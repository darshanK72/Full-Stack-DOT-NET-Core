/*
 * FILE ROLE: Demonstrates mixed conventional + attribute routing on a single controller
 *            and provides a preview of Areas routing.
 * SECTIONS IN THIS FILE:
 *   18. Mixed Conventional and Attribute Routing
 *   19. Areas Routing (PREVIEW → 10. Areas)
 */

using Microsoft.AspNetCore.Mvc;

namespace RoutingAttributeRouting.Controllers;

/*
 * SECTION 18: MIXED CONVENTIONAL AND ATTRIBUTE ROUTING
 * ─────────────────────────────────────────────────────────────────────────────
 * A controller with NO class-level [Route] attribute participates in BOTH
 * conventional routing (for actions without [Route]) AND attribute routing
 * (for actions that DO have [Route] or verb attributes).
 *
 * Routing mode per action:
 *
 *   Action method              [Route] present?   Routing mode
 *   ─────────────────────────  ─────────────────  ─────────────────────────────────
 *   Index()                    No                 Conventional only
 *                                                 Matched by: /blog or /blog/index
 *   Post(slug)                 Yes                Attribute only
 *                                                 Matched by: /blog/post/{slug}
 *   PostByDate(...)            Yes                Attribute only
 *                                                 Matched by: /blog/{year}/{month}/{slug}
 *
 * WHY MIXED ROUTING:
 *   Common when evolving a conventional controller to expose new friendly URLs.
 *   Legacy actions keep their conventional routes; new actions get custom templates.
 *
 * PITFALL — action reachability:
 *   An action with [Route] is ONLY reachable via that attribute route, not via the
 *   conventional route.  The action Index() without [Route] is ONLY reachable via
 *   conventional routing, not via any attribute route template.
 *
 * PITFALL — route ambiguity:
 *   If two routes (conventional + attribute) could both match the same URL, the
 *   attribute route wins in ASP.NET Core 3.1+.  In earlier versions this was
 *   ambiguous.  Prefer unambiguous templates.
 *
 * HTTP VERB CONSTRAINT ON ATTRIBUTE ROUTES:
 *   [Route("blog/post/{slug}")] accepts ALL HTTP methods.
 *   [HttpGet("blog/post/{slug}")] constrains to GET only.
 *   Use HTTP verb attributes when the action is method-specific (CRUD).
 */
public class BlogController : Controller
{
    // Conventional routing — matched via /blog or /blog/index
    // No [Route] attribute; falls through to the default conventional route pattern
    public IActionResult Index()
    {
        return View(); // renders Views/Blog/Index.cshtml (not created — routing demo)
    }

    // Attribute routing — matched ONLY by the pattern below
    // Conventional route /blog/post will NOT reach this action
    [HttpGet("blog/post/{slug}")]
    public IActionResult Post(string slug)
    {
        ViewData["Slug"] = slug;
        return View(); // renders Views/Blog/Post.cshtml (not created — routing demo)
    }

    // Attribute routing with multiple constraints — year and month must be integers
    // URL example: /blog/2024/11/my-article-slug
    [HttpGet("blog/{year:int}/{month:int:range(1,12)}/{slug}")]
    public IActionResult PostByDate(int year, int month, string slug)
    {
        ViewData["Year"]  = year;
        ViewData["Month"] = month;
        ViewData["Slug"]  = slug;
        return View(); // renders Views/Blog/PostByDate.cshtml (not created — routing demo)
    }

    /*
     * SECTION 19: AREAS ROUTING — PREVIEW
     * ─────────────────────────────────────────────────────────────────────────────
     * Areas are a way to partition a large MVC application into smaller functional
     * groups.  Each area has its own Controllers, Views, and Models folders nested
     * under an "Areas" directory.
     *
     *   Areas/
     *   └── Admin/
     *       ├── Controllers/
     *       │   └── DashboardController.cs   ← [Area("Admin")]
     *       ├── Views/
     *       │   └── Dashboard/
     *       │       └── Index.cshtml
     *       └── Models/
     *
     * AREA ROUTE REGISTRATION (Program.cs):
     *   app.MapControllerRoute(
     *       name: "areas",
     *       pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
     *
     *   The :exists constraint ensures the segment matches an actual registered area.
     *
     * MARKING A CONTROLLER AS BELONGING TO AN AREA:
     *   [Area("Admin")]
     *   public class DashboardController : Controller { ... }
     *
     *   The [Area] attribute sets the "area" route data token.  Without it, the area
     *   route pattern matches the URL segment but does not reach this controller.
     *
     * NAVIGATING ACROSS AREAS from a view:
     *   <a asp-area="Admin" asp-controller="Dashboard" asp-action="Index">Admin</a>
     *   @Url.Action("Index", "Dashboard", new { area = "Admin" })
     *
     *   The area tag helper requires asp-area="Admin" to switch areas.  Omitting
     *   asp-area when the current request is already in an area reuses the ambient
     *   area value — which may produce incorrect links.
     *
     * COVERED IN DETAIL LATER → 10. Areas
     *
     * The AdminDashboard action below shows what an area controller action looks like.
     * It is reachable via conventional routing (/blog/admindashboard) in THIS project
     * because BlogController has no class-level [Route] and no [Area] attribute.
     * In a real area setup, the [Area("Admin")] attribute would be on the class.
     */
    public IActionResult AdminDashboard()
    {
        // In a real Area controller this would be decorated with [Area("Admin")] at class level
        // and live in Areas/Admin/Controllers/BlogController.cs
        return View();
    }
}
