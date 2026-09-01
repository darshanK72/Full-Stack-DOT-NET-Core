/*
 * FILE ROLE: Non-area home controller — the default entry point for the root of the
 *   application. Demonstrates that non-area controllers coexist with area controllers
 *   in the same MVC project and are reached via the default (non-area) route.
 *
 * SECTIONS IN THIS FILE:
 *   6. Non-Area Controller — structure, no [Area] attribute, default route matching
 */

using Microsoft.AspNetCore.Mvc;

namespace Areas.Controllers;

/*
 * SECTION 6: NON-AREA CONTROLLER
 * ─────────────────────────────────────────────────────────────────────────────
 * A non-area controller lives in the root Controllers/ folder and carries NO
 * [Area] attribute. It is matched by the default route registered last in
 * Program.cs:
 *   pattern: "{controller=Home}/{action=Index}/{id?}"
 *
 * ROUTE MATCHING:
 *   /           → HomeController.Index()  (controller + action defaults apply)
 *   /Home       → HomeController.Index()  (action default applies)
 *   /Home/Index → HomeController.Index()  (explicit match)
 *
 * KEY DISTINCTION — area vs. non-area controllers:
 *
 *   Non-area controller (this file):     Area controller (DashboardController.cs):
 *   ─────────────────────────────────    ──────────────────────────────────────────
 *   Folder:  Controllers/                Folder: Areas/Admin/Controllers/
 *   [Area]:  none                        [Area("Admin")] required
 *   Route:   matched by default route    Route: matched by MapAreaControllerRoute
 *   URL:     /Home/Index                 URL:   /Admin/Dashboard/Index
 *
 * CONTROLLER INHERITANCE:
 *   : Controller (Microsoft.AspNetCore.Mvc)
 *   Inherits from Controller, which provides:
 *     View()           — return a ViewResult (renders .cshtml)
 *     Redirect()       — HTTP 302 redirect
 *     RedirectToAction("Action", "Controller", new { area = "..." })
 *     Json()           — return JSON response
 *     this.HttpContext — access to request/response
 *     this.ModelState  — validation state from model binding
 *     this.TempData    — cross-request data storage
 *     this.ViewData    — loosely typed ViewData dictionary
 *     this.ViewBag     — dynamic wrapper over ViewData
 *
 * VIEW RESOLUTION FOR HomeController.Index():
 *   The framework searches in order:
 *     1. Views/Home/Index.cshtml       ← found here (non-area view)
 *     2. Views/Shared/Index.cshtml
 *   Area views (Areas/Admin/Views/...) are NOT searched for non-area controllers.
 */
public class HomeController : Controller
{
    // GET /  or  /Home  or  /Home/Index
    public IActionResult Index()
    {
        return View(); // resolves to Views/Home/Index.cshtml
    }
}
