/*
 * FILE ROLE: Provides the two controller actions for this chapter:
 *   Index    — uses the shared layout, defines @section blocks, calls partials
 *   NoLayout — opts out of the layout entirely (Layout = null in the view)
 *
 * SECTIONS IN THIS FILE:
 *   1. Controller ↔ View ↔ Layout relationship
 *   2. Index action — PageViewModel + ViewData
 *   3. NoLayout action — bare response with no outer shell
 */

using Microsoft.AspNetCore.Mvc;
using LayoutsSectionsPartials.Models;

namespace LayoutsSectionsPartials.Controllers;

/*
 * SECTION 1: CONTROLLER ↔ VIEW ↔ LAYOUT RELATIONSHIP
 *
 * The controller is NOT responsible for selecting the layout.
 * Request flow for Index():
 *
 *   Browser GET /
 *     → PageController.Index()        (builds model + ViewData)
 *     → ViewResult returned
 *     → Razor engine starts rendering Views/Page/Index.cshtml
 *     → _ViewStart.cshtml runs first  (sets Layout = "_Layout")
 *     → Index.cshtml body rendered
 *     → _Layout.cshtml wraps output at @RenderBody()
 *     → @RenderSection("Scripts") injected at the layout's marker
 *     → Final HTML response sent
 *
 * The controller only calls View(model).  Layout selection, section
 * population, and partial rendering all happen in the view layer.
 */
public sealed class PageController : Controller
{
    /*
     * SECTION 2: Index ACTION
     *
     * Demonstrates:
     *   - Passing a PageViewModel for strongly-typed access in the view
     *   - Setting ViewData["SidebarContent"] to show how controller-set
     *     ViewData flows automatically into partial views
     *
     * ViewData vs ViewBag:
     *   ViewData["Key"]  — dictionary; requires casting on retrieval
     *   ViewBag.Key      — dynamic wrapper over ViewData; no cast needed
     *   Both share the same underlying store.  Prefer ViewData for type safety.
     *
     * URL: GET /  (default route) or GET /Page/Index
     */
    public IActionResult Index()
    {
        // ViewData set here is automatically available in the view and all
        // partial views rendered during that request — no explicit passing needed.
        ViewData["SidebarContent"] = "This text arrived via ViewData from the controller.";

        PageViewModel vm = new PageViewModel
        {
            Title    = "Layouts, Sections & Partial Views Demo",
            Body     = "Welcome to the layout chapter. Scroll the view source to see the outer shell.",
            NavItems = new[]
            {
                new NavItem { Label = "Home",     Href = "/",               IsActive = true  },
                new NavItem { Label = "About",    Href = "/page/about",     IsActive = false },
                new NavItem { Label = "No Layout",Href = "/page/noLayout",  IsActive = false },
            },
        };

        return View(vm);    // → Views/Page/Index.cshtml  (layout set by _ViewStart)
    }

    /*
     * SECTION 3: NoLayout ACTION
     *
     * The view (NoLayout.cshtml) overrides the layout set by _ViewStart.cshtml
     * with @{ Layout = null; } — the Razor engine renders ONLY the view's own
     * markup with no outer HTML shell.
     *
     * Use cases for Layout = null:
     *   - AJAX partial-page responses (return a fragment, not a full page)
     *   - Email templates (own DOCTYPE, no site chrome)
     *   - Integration test assertions on raw HTML fragments
     *   - Print-only views or PDF generation
     *
     * URL: GET /Page/NoLayout
     */
    public IActionResult NoLayout()
    {
        return View();      // → Views/Page/NoLayout.cshtml  (sets Layout = null itself)
    }
}
