/*
 * TOPIC: Controllers & Actions
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Controllers are the entry point for every MVC request. The routing system
 *   maps each incoming HTTP request to a controller action, which runs business
 *   logic and returns an IActionResult that shapes the HTTP response. Knowing
 *   the full IActionResult family — ViewResult, JsonResult, RedirectResult,
 *   FileResult, status code results — lets you express every HTTP scenario
 *   correctly. Understanding the base class hierarchy, action attributes, model
 *   binding, and context access is the foundation every later MVC chapter builds on.
 *
 * WHAT YOU WILL LEARN:
 *    1. Controller base class vs ControllerBase — when to use each
 *    2. [Controller] / [NonController] attributes — opt-in / opt-out of routing
 *    3. Action method signature rules — public, non-static, no special signatures
 *    4. IActionResult return type family — every concrete result type
 *    5. View() / PartialView() / Content() / Json() helper methods
 *    6. Redirect family — Redirect, RedirectToAction, RedirectToRoute
 *    7. Status code results — Ok, NotFound, BadRequest, Created, NoContent, StatusCode
 *    8. [NonAction] and [ActionName] attributes
 *    9. Accessing HttpContext, Request, Response, and RouteData from a controller
 *   10. ViewData / TempData — PREVIEW (depth in 12. TempData, ViewData & ViewBag)
 *   11. Async action methods — Task<IActionResult> pattern
 *   12. RedirectToAction with route values
 *   13. FileResult and the File() helper
 *   14. ControllerBase vs Controller — deep dive with [ApiController]
 *   15. ObjectResult family — Ok<T>, BadRequest, NotFound, Created, NoContent
 *   16. Product view model used by controller actions
 *
 * CHAPTER MAP (open files in this order):
 *   SECTIONS  1–11  → Controllers/HomeController.cs    Base class, IActionResult family
 *   SECTIONS 12–14  → Controllers/ProductsController.cs  Async, redirects, FileResult
 *   SECTIONS 15–16  → Controllers/ApiController.cs     ControllerBase, ObjectResult family
 *   SECTION  17     → Models/Product.cs                View model
 *   SECTION  18     → Program.cs (below)               MVC pipeline registration
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * SECTION 18: MVC PIPELINE REGISTRATION
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers three layers in the DI container:
 *   1. Controllers   — MVC routing, action invocation, model binding
 *   2. Views         — Razor view engine (needed for ViewResult, PartialViewResult)
 *   3. Validation    — Data Annotations support ([Required], [Range], etc.)
 *
 * Compare:
 *   AddControllers()           → API-only; no Razor engine (use with ControllerBase)
 *   AddControllersWithViews()  → Full MVC; Razor views + API helpers
 *   AddRazorPages()            → Razor Pages only; no controller routing
 *
 * MapControllerRoute() registers the conventional route template.
 * The default pattern {controller=Home}/{action=Index}/{id?} means:
 *   /                      → HomeController.Index()
 *   /Products              → ProductsController.Index()
 *   /Products/Details/3    → ProductsController.Details(id: 3)
 *   /api/catalog           → (attribute-routed; conventional route is also available)
 *
 * Pipeline order matters:
 *   UseHttpsRedirection → UseStaticFiles → UseRouting → MapControllerRoute
 *   Security middleware (UseAuthentication, UseAuthorization) would go after UseRouting.
 *
 * COVERED IN DETAIL LATER:
 *   Conventional vs attribute routing → 09. Routing & Attribute Routing
 *   DI lifetimes, scopes             → ASP.NET Core 04. Dependency Injection
 */
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(); // registers controllers + Razor engine + validation

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // detailed exception page; never expose in Production
}

app.UseHttpsRedirection(); // redirect HTTP → HTTPS
app.UseStaticFiles();      // serve files from wwwroot/
app.UseRouting();          // match URL to controller + action via route table

app.MapControllerRoute(    // conventional routing: {controller}/{action}/{id?}
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run(); // start Kestrel; blocks until shutdown signal

/*
 * QUICK REFERENCE — Controllers & Actions
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * BASE CLASSES
 *   Controller : ControllerBase   MVC — adds View(), PartialView(), ViewData, TempData
 *   ControllerBase                API-only — Http result helpers, no Razor engine
 *
 * IACTIONRESULT HELPERS (ControllerBase + Controller)
 *   View()                        ViewResult (200 + renders .cshtml)
 *   View(model)                   ViewResult with typed model
 *   PartialView("_Name")          PartialViewResult
 *   Json(obj)                     JsonResult (200 + JSON body)
 *   Content("text", "mime")       ContentResult
 *   Redirect(url)                 RedirectResult (302, external URL)
 *   RedirectPermanent(url)        RedirectResult (301)
 *   RedirectToAction("Action")    RedirectToActionResult (302, within app)
 *   RedirectToAction("A","Ctrl")  RedirectToActionResult to another controller
 *   RedirectToAction("A",values)  RedirectToActionResult with route data
 *   RedirectToRoute(routeValues)  RedirectToRouteResult
 *   File(bytes, mime, name)       FileContentResult (download)
 *   File(stream, mime)            FileStreamResult
 *   PhysicalFile(path, mime)      PhysicalFileResult
 *   StatusCode(code)              StatusCodeResult (any status)
 *   Ok()                          OkResult (200, no body)
 *   Ok(payload)                   OkObjectResult (200 + JSON body)
 *   NotFound()                    NotFoundResult (404)
 *   NotFound(obj)                 NotFoundObjectResult (404 + body)
 *   BadRequest()                  BadRequestResult (400)
 *   BadRequest(obj)               BadRequestObjectResult (400 + body)
 *   Created(uri, obj)             CreatedResult (201 + Location header + body)
 *   NoContent()                   NoContentResult (204, no body)
 *   new EmptyResult()             EmptyResult (200, empty body)
 *
 * ACTION ATTRIBUTES
 *   [NonAction]                   public method excluded from routing
 *   [ActionName("Name")]          override action name for routing + URL generation
 *   [HttpGet] / [HttpPost] / …    restrict to HTTP verb
 *   [Route("pattern")]            attribute route (overrides conventional)
 *
 * CONTROLLER ATTRIBUTES
 *   [Controller]                  opt-in: class is a controller even without suffix
 *   [NonController]               opt-out: ignore class even if name ends "Controller"
 *   [ApiController]               API behaviours: auto-400, binding inference, ProblemDetails
 *
 * CONTEXT (available in any ControllerBase subclass)
 *   this.HttpContext               full request + response context
 *   this.Request                   HttpContext.Request shortcut (method, path, headers)
 *   this.Response                  HttpContext.Response shortcut (status, headers, body)
 *   this.RouteData                 current route values (controller, action, id, …)
 *   this.User                      ClaimsPrincipal (authenticated user)
 *   this.ModelState                validation state from model binding
 *
 * NEXT CHAPTERS:
 *   03. Views & Razor Syntax               — ViewResult depth, @model, Razor directives
 *   06. Model Binding in MVC               — [FromBody], [FromQuery], [FromRoute] in detail
 *   09. Routing & Attribute Routing        — [Route], [HttpGet("{id}")], constraints
 *   11. Action Filters in MVC              — IActionFilter, result filters, exception filters
 *   12. TempData, ViewData & ViewBag       — full depth of controller-to-view data passing
 */
