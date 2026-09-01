/*
 * TOPIC: Areas in ASP.NET Core MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   As MVC applications grow, a flat Controllers/ folder becomes unwieldy.
 *   Areas let you partition a large application into self-contained functional
 *   segments — each with its own controllers, views, and optionally models —
 *   without breaking the rest of the application. Real-world uses:
 *     · Admin portal vs. customer-facing site in one deployment
 *     · Multi-tenant sections with distinct layouts
 *     · Feature teams owning independent slices of a large product
 *
 * WHAT YOU WILL LEARN:
 *    1. What areas are and how they logically group MVC components
 *    2. The required folder structure (Areas/{Name}/Controllers/, Views/, Models/)
 *    3. The [Area] attribute on controllers
 *    4. Registering area routes with MapAreaControllerRoute
 *    5. Default area route template and parameter defaults
 *    6. View discovery order in areas (area-specific → area-shared → root-shared)
 *    7. _ViewStart and _ViewImports scoping inside areas
 *    8. Linking to area actions with the asp-area tag helper
 *    9. Url.Action and RedirectToAction with the area route value
 *   10. Cross-area links (asp-area="" for non-area; asp-area="Other" for another area)
 *   11. Area-specific layouts vs. shared root layout
 *   12. Organising large apps — Admin area + Customer area example
 *
 * CHAPTER MAP (multi-file project — open files in this order):
 *   SECTION  1 → Program.cs                                         What Are Areas
 *   SECTION  2 → Program.cs                                         Area Folder Structure
 *   SECTION  3 → Program.cs                                         MVC Service Registration
 *   SECTION  4 → Program.cs                                         Area Route Registration (MapAreaControllerRoute)
 *   SECTION  5 → Program.cs                                         Default Non-Area Route
 *   SECTION  6 → Controllers/HomeController.cs                      Non-Area Controller
 *   SECTION  7 → Areas/Admin/Controllers/DashboardController.cs     [Area] Attribute on Controllers
 *   SECTION  8 → Areas/Customer/Controllers/AccountController.cs    Multiple Areas — [Area("Customer")]
 *   SECTION  9 → Models/AdminDashboardViewModel.cs                  Area ViewModel (shared Models/)
 *   SECTION 10 → Views/_ViewImports.cshtml                          Root Tag Helper Registration
 *   SECTION 11 → Views/Shared/_Layout.cshtml                        Root Layout + asp-area Links
 *   SECTION 12 → Views/Home/Index.cshtml                            Root View — Cross-Area Navigation
 *   SECTION 13 → Areas/Admin/Views/_ViewStart.cshtml                Area _ViewStart.cshtml
 *   SECTION 14 → Areas/Admin/Views/_ViewImports.cshtml              Area _ViewImports.cshtml
 *   SECTION 15 → Areas/Admin/Views/Shared/_Layout.cshtml            Area-Specific Layout
 *   SECTION 16 → Areas/Admin/Views/Dashboard/Index.cshtml           Admin Area View
 *   SECTION 17 → Areas/Customer/Views/Account/Profile.cshtml        Customer Area + Cross-Area Links
 */

/*
 * SECTION 1: WHAT ARE AREAS
 * ─────────────────────────────────────────────────────────────────────────────
 * An Area is a logical partition of an ASP.NET Core MVC application. It groups
 * related controllers, views, and models into a named segment that is isolated
 * from the rest of the application.
 *
 * Think of areas as mini-MVC applications nested inside the main application:
 *   · Each area has its own Controllers/, Views/, and optionally Models/ folders
 *   · Each area has its own route prefix (e.g., /Admin/..., /Customer/...)
 *   · Controllers inside an area must be decorated with [Area("AreaName")]
 *   · Views inside an area are discovered first in the area, then fall back
 *     to the root Views/Shared/ folder
 *
 * WITHOUT AREAS (flat app — gets messy fast):
 *   Controllers/
 *     HomeController.cs
 *     AdminDashboardController.cs          ← ambiguous ownership
 *     AdminUsersController.cs
 *     CustomerAccountController.cs
 *   Views/
 *     AdminDashboard/, AdminUsers/,        ← mixed concerns in one folder
 *     CustomerAccount/
 *
 * WITH AREAS (clean, scalable):
 *   Controllers/                           ← only non-area controllers
 *     HomeController.cs
 *   Areas/
 *     Admin/
 *       Controllers/
 *         DashboardController.cs           ← [Area("Admin")]
 *       Views/
 *         Dashboard/Index.cshtml           ← isolated from Customer views
 *     Customer/
 *       Controllers/
 *         AccountController.cs            ← [Area("Customer")]
 *       Views/
 *         Account/Profile.cshtml
 *
 * The three-part route for an area action is: {area}/{controller}/{action}
 *   /Admin/Dashboard/Index
 *   /Customer/Account/Profile
 *
 * COVERED IN DETAIL LATER:
 *   Deep routing → 09. Routing & Attribute Routing
 */

/*
 * SECTION 2: AREA FOLDER STRUCTURE
 * ─────────────────────────────────────────────────────────────────────────────
 * The physical folder structure follows a strict convention that ASP.NET Core's
 * view engine and route system recognise automatically.
 *
 * REQUIRED LAYOUT:
 *   Areas/
 *     {AreaName}/                        ← area root (name must match [Area("...")])
 *       Controllers/
 *         {Name}Controller.cs            ← must carry [Area("AreaName")]
 *       Views/
 *         _ViewStart.cshtml              ← area-level layout assignment
 *         _ViewImports.cshtml            ← tag helpers & using directives for area views
 *         {ControllerName}/
 *           {ActionName}.cshtml          ← action view
 *         Shared/
 *           _Layout.cshtml              ← (optional) area-specific layout
 *       Models/                          ← (optional) area-specific view models
 *
 * VIEW DISCOVERY ORDER (for an area view):
 *   When the MVC view engine searches for "{action}.cshtml":
 *     1. Areas/{area}/Views/{controller}/{action}.cshtml   ← area-specific
 *     2. Areas/{area}/Views/Shared/{action}.cshtml         ← area shared
 *     3. Views/Shared/{action}.cshtml                      ← root shared fallback
 *
 * LAYOUT DISCOVERY ORDER (when _ViewStart sets Layout = "_Layout"):
 *     1. Areas/{area}/Views/{controller}/_Layout.cshtml
 *     2. Areas/{area}/Views/Shared/_Layout.cshtml          ← area overrides root
 *     3. Views/Shared/_Layout.cshtml                       ← root fallback
 *
 *   · Admin area has Areas/Admin/Views/Shared/_Layout.cshtml → uses its own layout
 *   · Customer area has NO area layout → falls back to Views/Shared/_Layout.cshtml
 *   This is deliberately asymmetric in this chapter to demonstrate both cases.
 *
 * _VIEWIMPORTS DISCOVERY ORDER (for area views):
 *   The Razor engine walks UP the physical directory from the view file:
 *     1. Areas/{area}/Views/{controller}/_ViewImports.cshtml
 *     2. Areas/{area}/Views/_ViewImports.cshtml             ← area-level imports
 *     3. Areas/{area}/_ViewImports.cshtml
 *     4. Areas/_ViewImports.cshtml
 *     5. _ViewImports.cshtml (project root)
 *   NOTE: Views/_ViewImports.cshtml is NOT on this path for area views.
 *   Each area needs its own _ViewImports.cshtml if it uses tag helpers or namespaces.
 *
 * MODELS PLACEMENT — two valid conventions:
 *   · Root Models/ — shared models reused across areas (this chapter's approach)
 *   · Areas/{area}/Models/ — area-isolated view models; good for large teams
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * SECTION 3: MVC SERVICE REGISTRATION — AddControllersWithViews
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers the full MVC stack needed for areas:
 *
 *   What it registers:
 *     · Controller infrastructure (action discovery, model binding, filters)
 *     · Razor view engine (view discovery, layout resolution, tag helpers)
 *     · Antiforgery services (CSRF tokens for forms)
 *     · TempData provider
 *
 * Compare with related registration methods:
 *
 *   Method                         Controllers  Views  Razor Pages  APIs
 *   ────────────────────────────   ───────────  ─────  ───────────  ────
 *   AddControllers()                   ✓          ✗        ✗         ✓
 *   AddControllersWithViews()          ✓          ✓        ✗         ✓
 *   AddRazorPages()                    ✗          ✓        ✓         ✗
 *   AddMvc()                           ✓          ✓        ✓         ✓
 *
 * Areas require AddControllersWithViews() (or AddMvc()) because they need both
 * the controller infrastructure AND the Razor view engine.
 *
 * Area registration is automatic — no explicit "UseAreas()" call exists.
 * The framework discovers area controllers at startup by scanning assemblies
 * for types decorated with [Area("...")] that also inherit from ControllerBase.
 */
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(); // enables controllers + Razor view engine

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage(); // detailed error pages in development

app.UseHttpsRedirection();  // redirect HTTP → HTTPS
app.UseStaticFiles();       // serve wwwroot/ (CSS, JS, images)
app.UseRouting();           // match URL to endpoint
app.UseAuthorization();     // check access policies

/*
 * SECTION 4: AREA ROUTE REGISTRATION — MapAreaControllerRoute
 * ─────────────────────────────────────────────────────────────────────────────
 * MapAreaControllerRoute() registers a route that is ONLY satisfied when the
 * matched controller's [Area] attribute equals the areaName parameter.
 *
 * SIGNATURE:
 *   app.MapAreaControllerRoute(
 *       name:      string,     ← unique route name (used in link generation)
 *       areaName:  string,     ← must match the [Area("...")] value exactly
 *       pattern:   string,     ← URL template for this area
 *       defaults:  object?,    ← default route values (optional)
 *       constraints: object?); ← route constraints (optional)
 *
 * DEFAULT AREA ROUTE TEMPLATE CONVENTION:
 *   "{areaName}/{controller}/{action}/{id?}"
 *   e.g., "Admin/{controller=Dashboard}/{action=Index}/{id?}"
 *
 * ALTERNATIVE — MapControllerRoute with area token in defaults:
 *   app.MapControllerRoute(
 *       name:    "AdminArea",
 *       pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
 *       defaults: new { area = "Admin" },
 *       constraints: new { area = "Admin" });
 *
 * MapAreaControllerRoute is shorthand that sets the area default and constraint
 * for you — prefer it over the verbose MapControllerRoute approach.
 *
 * ROUTE ORDER MATTERS:
 *   Register more-specific area routes BEFORE the default catch-all route.
 *   Routes are evaluated in registration order; the first match wins.
 *   If the default route is registered first, it would match /Admin/... before
 *   the Admin area route has a chance to run its [Area] constraint check.
 *
 * URL EXAMPLES PRODUCED BY THESE ROUTES:
 *   /Admin/Dashboard/Index        → Admin area, DashboardController.Index()
 *   /Admin/Dashboard              → Admin area, DashboardController.Index() (action default)
 *   /Admin                        → Admin area, DashboardController.Index() (controller + action defaults)
 *   /Customer/Account/Profile     → Customer area, AccountController.Profile()
 *   /Customer/Account             → Customer area, AccountController.Profile() (action default)
 */
app.MapAreaControllerRoute(     // Admin area route
    name: "AdminArea",          // unique name used by URL helpers (Url.RouteUrl, RedirectToRoute)
    areaName: "Admin",          // matches [Area("Admin")] on DashboardController
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}"); // /Admin → Dashboard/Index

app.MapAreaControllerRoute(     // Customer area route
    name: "CustomerArea",
    areaName: "Customer",       // matches [Area("Customer")] on AccountController
    pattern: "Customer/{controller=Account}/{action=Profile}/{id?}"); // /Customer → Account/Profile

/*
 * SECTION 5: DEFAULT NON-AREA ROUTE
 * ─────────────────────────────────────────────────────────────────────────────
 * The standard MapControllerRoute registers the catch-all route for controllers
 * that are NOT inside an area (no [Area] attribute).
 *
 * NON-AREA ROUTE PATTERN:
 *   {controller=Home}/{action=Index}/{id?}
 *   / → HomeController.Index()
 *
 * WHY THIS MUST COME LAST:
 *   Area routes use [Area] constraints — a URL like /Admin/Dashboard/Index only
 *   satisfies the Admin area route because DashboardController has [Area("Admin")].
 *   The default route has no area constraint, so it could match any URL pattern;
 *   registering it last ensures it only runs when no area route matched.
 *
 * ROUTE-TO-CONTROLLER MATCHING SUMMARY:
 *   URL                           Route matched     Controller executed
 *   ─────────────────────────     ─────────────     ──────────────────────────────
 *   /                             default           Controllers/HomeController.Index()
 *   /Home/Index                   default           Controllers/HomeController.Index()
 *   /Admin                        AdminArea         Areas/Admin/.../DashboardController.Index()
 *   /Admin/Dashboard/Index        AdminArea         Areas/Admin/.../DashboardController.Index()
 *   /Customer/Account/Profile     CustomerArea      Areas/Customer/.../AccountController.Profile()
 *
 * IMPORTANT: Non-area controllers (HomeController) are placed in the root
 * Controllers/ folder WITHOUT an [Area] attribute. Area controllers are placed
 * inside Areas/{name}/Controllers/ WITH the matching [Area] attribute.
 * Mixing these up is the most common Area mistake:
 *   · Area controller missing [Area] → 404 from area route (constraint fails)
 *   · Area controller in wrong folder → still works (folder is convention, not enforced)
 *   · Non-area controller with [Area] → never reachable via default route
 */
app.MapControllerRoute(         // non-area catch-all (must be last)
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // / → Home/Index

app.Run(); // start Kestrel; blocks until SIGTERM or Ctrl+C

/*
 * QUICK REFERENCE — Areas in ASP.NET Core MVC
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * REGISTER MVC WITH VIEWS (required for areas):
 *   builder.Services.AddControllersWithViews();
 *
 * REGISTER AREA ROUTES (before default route):
 *   app.MapAreaControllerRoute(name, areaName, pattern);
 *   // or equivalently:
 *   app.MapControllerRoute(name, pattern,
 *       defaults: new { area = "Admin" },
 *       constraints: new { area = "Admin" });
 *
 * MARK A CONTROLLER AS BELONGING TO AN AREA:
 *   [Area("Admin")]
 *   public class DashboardController : Controller { ... }
 *
 * LINK TO AN AREA ACTION (Razor tag helper):
 *   <a asp-area="Admin" asp-controller="Dashboard" asp-action="Index">Admin</a>
 *   <a asp-area=""      asp-controller="Home"      asp-action="Index">Home</a>  ← clear area
 *   <a asp-area="Customer" asp-controller="Account" asp-action="Profile">Profile</a>  ← cross-area
 *
 * LINK TO AN AREA ACTION (Url.Action helper):
 *   @Url.Action("Index", "Dashboard", new { area = "Admin" })
 *   @Url.Action("Index", "Home",      new { area = "" })   ← non-area
 *
 * REDIRECT TO AN AREA ACTION (from a controller):
 *   return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
 *   return RedirectToAction("Index", "Home",      new { area = "" });  ← non-area
 *
 * VIEW DISCOVERY ORDER (area views):
 *   Areas/{area}/Views/{controller}/{action}.cshtml  ← first
 *   Areas/{area}/Views/Shared/{action}.cshtml
 *   Views/Shared/{action}.cshtml                     ← root fallback
 *
 * LAYOUT DISCOVERY ORDER (area _ViewStart Layout = "_Layout"):
 *   Areas/{area}/Views/Shared/_Layout.cshtml         ← area-specific (overrides root)
 *   Views/Shared/_Layout.cshtml                      ← root fallback
 *
 * _VIEWIMPORTS DISCOVERY FOR AREA VIEWS:
 *   Areas/{area}/Views/_ViewImports.cshtml           ← area-level
 *   _ViewImports.cshtml (project root)               ← project-level
 *   NOTE: Views/_ViewImports.cshtml does NOT apply to area views
 *
 * NEXT CHAPTERS:
 *   11. Action Filters in MVC    — attribute-based cross-cutting concerns
 *   12. TempData, ViewData, ViewBag — passing data between actions and views
 */
