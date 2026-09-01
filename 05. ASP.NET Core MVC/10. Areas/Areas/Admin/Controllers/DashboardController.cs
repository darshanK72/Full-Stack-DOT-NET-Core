/*
 * FILE ROLE: Admin area dashboard controller — demonstrates the [Area("Admin")] attribute
 *   that links this controller to the Admin area route, builds a typed ViewModel,
 *   and shows how to generate cross-area redirect URLs.
 *
 * SECTIONS IN THIS FILE:
 *   7. [Area] Attribute on Controllers — requirement, naming rules, route matching
 */

using Areas.Models;
using Microsoft.AspNetCore.Mvc;

namespace Areas.Admin.Controllers;

/*
 * SECTION 7: [Area] ATTRIBUTE ON CONTROLLERS
 * ─────────────────────────────────────────────────────────────────────────────
 * The [Area] attribute is the critical link between a controller and an area route.
 * Without it, the controller is invisible to MapAreaControllerRoute — URL generation
 * would fail and the controller would be unreachable via area URLs.
 *
 * HOW [Area] WORKS:
 *   MapAreaControllerRoute adds an implicit route constraint:
 *     constraints: new { area = "Admin" }
 *   When a request arrives at /Admin/Dashboard/Index, the route matches
 *   DashboardController only if the controller carries [Area("Admin")].
 *   A controller without [Area("Admin")] in the same namespace would not match.
 *
 * NAMING RULES:
 *   · The string passed to [Area("...")] must match the areaName parameter of
 *     MapAreaControllerRoute() EXACTLY (case-insensitive comparison at runtime,
 *     but use consistent casing: "Admin" everywhere).
 *   · The controller class name DOES NOT need to include the area name.
 *     "DashboardController" is correct; "AdminDashboardController" is redundant.
 *   · The physical folder (Areas/Admin/Controllers/) is convention, not enforced.
 *     The [Area] attribute is the authoritative membership declaration.
 *
 * COMMON MISTAKES:
 *   · Missing [Area] attribute → controller unreachable via area route (404)
 *   · [Area("admin")] (wrong case) → works at runtime but breaks string comparisons
 *     in link generation (asp-area="Admin" won't match "admin")
 *   · Duplicate controller name without area disambiguation:
 *       Controllers/AccountController.cs          (no [Area])
 *       Areas/Admin/Controllers/AccountController.cs  ([Area("Admin")])
 *     → Both exist; resolved correctly by their routes. No conflict.
 *   · Two controllers with the SAME name and SAME area → AmbiguousMatchException
 *
 * APPLYING [Area] TO A BASE CLASS:
 *   [Area("Admin")]
 *   public class AdminBaseController : Controller { }
 *   public class DashboardController : AdminBaseController { }   ← inherits [Area]
 *   This pattern avoids repeating [Area] across every controller in the area.
 *
 * REDIRECTING TO ANOTHER AREA FROM THIS CONTROLLER:
 *   return RedirectToAction("Profile", "Account", new { area = "Customer" });
 *   // Produces: /Customer/Account/Profile
 *
 * REDIRECTING TO A NON-AREA ROUTE:
 *   return RedirectToAction("Index", "Home", new { area = "" });
 *   // area = "" (empty string) explicitly clears the area; produces: /
 *
 * URL GENERATION VIA Url.Action (usable from controllers or views):
 *   Url.Action("Index", "Dashboard", new { area = "Admin" })   → "/Admin/Dashboard/Index"
 *   Url.Action("Index", "Home",      new { area = "" })        → "/"
 */
[Area("Admin")] // links this controller to the "Admin" area route
public class DashboardController : Controller
{
    // GET /Admin/Dashboard/Index  (or /Admin or /Admin/Dashboard — defaults apply)
    public IActionResult Index()
    {
        // Build the ViewModel with data the Admin Dashboard view needs.
        // In a real app these values would come from a service or database.
        var viewModel = new AdminDashboardViewModel
        {
            WelcomeMessage = "Welcome to the Admin Dashboard",
            TotalUsers     = 42,   // simulated user count
            TotalOrders    = 15,   // simulated order count
            AreaName       = "Admin"
        };
        return View(viewModel); // resolves to Areas/Admin/Views/Dashboard/Index.cshtml
    }
}
