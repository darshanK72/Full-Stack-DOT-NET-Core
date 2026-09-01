/*
 * FILE ROLE: Demonstrates the foundational controller concepts — the Controller base
 *            class hierarchy, naming conventions, action method signature rules, the
 *            complete IActionResult family with every helper method, action attributes
 *            ([NonAction], [ActionName]), accessing HttpContext/Request/Response/RouteData,
 *            and a preview of ViewData / TempData.
 *
 * SECTIONS IN THIS FILE:
 *    1. Controller base class hierarchy and naming conventions
 *    2. [Controller] / [NonController] attributes
 *    3. Action method signature rules
 *    4. IActionResult return type family — full reference table
 *    5. ViewResult and PartialViewResult
 *    6. ContentResult and JsonResult
 *    7. Redirect family
 *    8. Status code result family
 *    9. [NonAction] and [ActionName] attributes
 *   10. Accessing HttpContext / Request / Response / RouteData
 *   11. ViewData / TempData — PREVIEW
 */

using Microsoft.AspNetCore.Mvc;

namespace ControllersActions.Controllers;

/*
 * SECTION 1: CONTROLLER BASE CLASS HIERARCHY & NAMING CONVENTIONS
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core provides two base classes for controllers:
 *
 *   ControllerBase (Microsoft.AspNetCore.Mvc)
 *     The minimal base. Provides:
 *       • All HTTP result helpers: Ok(), NotFound(), BadRequest(), Created(), etc.
 *       • HttpContext, Request, Response, RouteData properties
 *       • ModelState, User, Url, ControllerContext
 *     Use ControllerBase for Web API controllers that return JSON/data — no Razor engine.
 *
 *   Controller : ControllerBase
 *     Extends ControllerBase with MVC-specific members:
 *       • View() / PartialView() — invokes the Razor view engine
 *       • ViewData (ViewDataDictionary) — key/value bag shared with the view
 *       • ViewBag (dynamic) — dynamic wrapper around ViewData
 *       • TempData (ITempDataDictionary) — survives exactly one redirect
 *     Use Controller for MVC controllers that render Razor views (HTML responses).
 *
 * NAMING CONVENTIONS (affect routing discovery):
 *   Rule 1  Class name MUST end in "Controller" (e.g. HomeController → "Home")
 *           unless decorated with [Controller] (see SECTION 2).
 *   Rule 2  The routing segment is the name WITHOUT the suffix: "Home", "Products".
 *   Rule 3  Namespace does NOT affect the routing name — only the class name matters.
 *   Rule 4  Controllers must be public, concrete (non-abstract), non-generic classes.
 *   Rule 5  The class must live in an assembly the app is configured to scan
 *           (AddControllersWithViews / AddControllers in Program.cs).
 *
 * INHERITANCE CHAIN:
 *   HomeController
 *     └─ Controller
 *          └─ ControllerBase
 *               └─ object
 *
 * HomeController inherits Controller because it renders Razor views (HTML responses).
 * See Controllers/ApiController.cs (SECTION 15) for a ControllerBase example.
 */

/*
 * SECTION 2: [Controller] AND [NonController] ATTRIBUTES
 * ─────────────────────────────────────────────────────────────────────────────
 * These two attributes let you opt in or out of MVC routing discovery, overriding
 * the naming convention from SECTION 1.
 *
 * [Controller]
 *   Marks a class as a controller even when its name does NOT end in "Controller".
 *   The routing name is the full class name (e.g. class Catalog → route segment "Catalog").
 *   Usage: add [Controller] and [Route("...")] on a class named without the suffix.
 *
 *   Example (not in this project; illustrative):
 *     [Controller]                     // opt-in: MVC scans this class for actions
 *     [Route("api/catalog")]
 *     public class Catalog : ControllerBase { ... }
 *
 * [NonController]
 *   Prevents MVC routing from treating a class as a controller even when:
 *     • Its name ends in "Controller", OR
 *     • It inherits from ControllerBase / Controller
 *   Use on base/helper classes placed in the Controllers/ folder to avoid
 *   accidental routing of shared infrastructure code.
 *
 *   Example (not in this project; illustrative):
 *     [NonController]
 *     public class BaseAuditController : Controller  // MVC routing ignores this class
 *     {
 *         protected void LogAccess(string action) { ... }
 *     }
 *
 * See Controllers/ApiController.cs for [NonController] applied to a concrete helper class.
 */
public class HomeController : Controller // Controller for Razor views; ControllerBase for APIs
{
    /*
     * SECTION 3: ACTION METHOD SIGNATURE RULES
     * ─────────────────────────────────────────────────────────────────────────
     * MVC uses a set of rules to identify which methods on a controller are
     * "actions" (routable HTTP handlers) vs ordinary class members.
     *
     * An action method MUST:
     *   • Be public           (private/protected/internal methods are skipped)
     *   • Be an instance method (static methods are skipped by default)
     *   • Not be decorated with [NonAction] (see SECTION 9)
     *   • Not be a constructor, destructor, property, event, or operator
     *
     * An action method SHOULD:
     *   • Return IActionResult (or Task<IActionResult> for async)
     *   • Use parameters for model binding (route values, query string, body)
     *
     * Return type options:
     *   IActionResult          synchronous; any result type
     *   Task<IActionResult>    async (see SECTION 11 in ProductsController.cs)
     *   ValueTask<IActionResult>  async; lower allocation than Task for hot paths
     *   void                   returns 200 OK with empty body (rarely used)
     *   string                 returns 200 OK with the string as text/plain
     *   Concrete type (e.g. ViewResult, JsonResult)  valid but less flexible
     *
     * Parameters with ref/out/in are NOT supported — binding source attributes
     * ([FromRoute], [FromBody], [FromQuery]) are used instead.
     *
     * COVERED IN DETAIL LATER → 06. Model Binding in MVC
     */

    /*
     * SECTION 4: IActionResult RETURN TYPE FAMILY — FULL REFERENCE
     * ─────────────────────────────────────────────────────────────────────────
     * IActionResult is the abstraction every result type implements. The framework
     * calls IActionResult.ExecuteResultAsync(ActionContext) to produce the HTTP response.
     *
     * Result type              Helper method(s)                    HTTP status   Body
     * ───────────────────────  ──────────────────────────────────  ────────────  ──────────────
     * ViewResult               View() / View(model)                200           Rendered HTML
     * PartialViewResult        PartialView("_Name")                200           Partial HTML
     * JsonResult               Json(obj)                           200           JSON
     * ContentResult            Content("txt","mime")               200           text/mime
     * RedirectResult           Redirect(url)                       302 / 301     empty
     * RedirectToActionResult   RedirectToAction(action)            302 / 301     empty
     * RedirectToRouteResult    RedirectToRoute(values)             302 / 301     empty
     * StatusCodeResult         StatusCode(code)                    any           empty
     * OkResult                 Ok()                                200           empty
     * OkObjectResult           Ok(payload)                         200           JSON
     * NotFoundResult           NotFound()                          404           empty
     * NotFoundObjectResult     NotFound(obj)                       404           JSON
     * BadRequestResult         BadRequest()                        400           empty
     * BadRequestObjectResult   BadRequest(obj)                     400           JSON/errors
     * CreatedResult            Created(uri, obj)                   201           JSON
     * NoContentResult          NoContent()                         204           empty
     * EmptyResult              new EmptyResult()                   200           empty
     * FileContentResult        File(byte[], mime, name)            200           binary
     * FileStreamResult         File(stream, mime)                  200           streamed
     * PhysicalFileResult       PhysicalFile(path, mime)            200           file on disk
     * ObjectResult             (base of Ok/Bad/NotFound objects)   any           JSON/negotiated
     *
     * All result types inherit from ActionResult, which implements IActionResult.
     * Content negotiation: ObjectResult picks the format (JSON, XML, …) based on
     * the Accept header and registered formatters. Json() always returns JSON.
     *
     * COVERED IN DETAIL LATER:
     *   FileResult depth  → 09. Routing & Attribute Routing (streaming / range)
     *   ObjectResult / content negotiation → ASP.NET Core APIs module
     */

    /* ── SECTION 5: ViewResult AND PartialViewResult ──────────────────────── */
    /*
     * View() invokes the Razor view engine. Without arguments it looks for:
     *   Views/{ControllerName}/{ActionName}.cshtml
     * e.g. HomeController.Index() → Views/Home/Index.cshtml
     *
     * View(model) does the same but passes a typed model to the view, enabling
     * @model <Type> in the .cshtml file and strongly typed Model access.
     *
     * PartialView("_Name") renders a partial view — a reusable HTML fragment with
     * no layout. The view engine searches Views/{Controller}/ then Views/Shared/.
     * Partial views are used for AJAX partial-page updates and reusable components.
     *
     * COVERED IN DETAIL LATER:
     *   Razor syntax & @model     → 03. Views & Razor Syntax
     *   Layouts and partial views → 04. Layouts, Sections & Partial Views
     */
    public IActionResult Index()
    {
        return View(); // renders Views/Home/Index.cshtml (no model)
    }

    public IActionResult About()
    {
        ViewData["Heading"] = "About This Tutorial";        // pass data to view via ViewData
        ViewData["Message"] = "Demonstrates every IActionResult type and controller convention.";
        return View(); // renders Views/Home/About.cshtml; view reads ViewData["Heading"] etc.
    }

    public IActionResult Summary()
    {
        // PartialViewResult — renders Views/Shared/_Summary.cshtml (or Views/Home/_Summary.cshtml)
        // File not included in this project; navigating to /Home/Summary causes runtime 404.
        // In practice, partial views are returned in response to AJAX requests that update
        // a section of the page without a full reload.
        return PartialView("_Summary"); // looks for _Summary.cshtml in View search paths
    }

    /* ── SECTION 6: ContentResult AND JsonResult ──────────────────────────── */
    /*
     * Content(string content, string contentType)
     *   Returns raw text with the specified MIME type and 200 OK.
     *   Common types: "text/plain", "text/html", "application/xml".
     *
     * Json(object data)
     *   Serialises data to JSON using System.Text.Json and returns 200 OK.
     *   Note: Json() always returns JSON regardless of Accept header.
     *   For content-negotiated responses (JSON or XML based on client preference),
     *   use Ok(payload) in an API controller with ObjectResult (see ApiController.cs).
     */
    public IActionResult Plain()
    {
        return Content("Hello from ContentResult!", "text/plain"); // 200 + plain text body
    }

    public IActionResult JsonSample()
    {
        var payload = new { name = "Alice", role = "Developer", score = 100 };
        return Json(payload); // 200 + {"name":"Alice","role":"Developer","score":100}
    }

    /* ── SECTION 7: REDIRECT FAMILY ───────────────────────────────────────── */
    /*
     * Redirect(url)           → 302 Found (temporary) to an ABSOLUTE external URL
     * RedirectPermanent(url)  → 301 Moved Permanently to an external URL
     *
     * RedirectToAction(actionName)
     *   → 302 to another action on the SAME controller.
     *     Generates the URL via the route table — safe if routes change.
     *
     * RedirectToAction(actionName, controllerName)
     *   → 302 to an action on a DIFFERENT controller.
     *
     * RedirectToAction(actionName, routeValues)
     *   → 302 with additional route data (id, page, filter, …).
     *     See ProductsController.cs (SECTION 13) for route-value examples.
     *
     * RedirectToRoute(routeValues)
     *   → 302 by supplying the raw route value dictionary.
     *     Less common; use RedirectToAction for named actions.
     *
     * PITFALL: Redirect() accepts a user-supplied URL → open redirect vulnerability.
     *   Always validate the URL is local before redirecting based on user input.
     *   Use LocalRedirect() to enforce same-origin restriction.
     */
    public IActionResult GoExternal()
    {
        return Redirect("https://learn.microsoft.com/aspnet/core/mvc/controllers/actions");
    }

    public IActionResult GoHome()
    {
        return RedirectToAction(nameof(Index)); // 302 → /Home/Index (same controller)
    }

    public IActionResult GoProducts()
    {
        return RedirectToAction("Index", "Products"); // 302 → /Products/Index
    }

    public IActionResult GoRoute()
    {
        // RedirectToRoute with explicit route value dictionary
        return RedirectToRoute(new { controller = "Home", action = "About" }); // 302 → /Home/About
    }

    /* ── SECTION 8: STATUS CODE RESULT FAMILY ────────────────────────────── */
    /*
     * StatusCode(int statusCode)
     *   Returns any HTTP status with an empty body. Use for non-standard codes
     *   (e.g. 418 I'm a Teapot, 429 Too Many Requests).
     *
     * Ok() / Ok(payload)
     *   200 OK. Ok() has no body; Ok(payload) serialises payload to JSON.
     *   In MVC controllers, plain Ok() signals success with nothing to display.
     *   In APIs, Ok(payload) is the primary success response.
     *
     * NotFound() / NotFound(obj)
     *   404 Not Found. NotFound(obj) includes a JSON body with error detail.
     *
     * BadRequest() / BadRequest(obj)
     *   400 Bad Request. BadRequest(ModelState) is common — returns validation errors.
     *
     * Created(uri, obj)
     *   201 Created. Adds a Location header pointing to the new resource URI.
     *   Used after successful POST that creates a resource.
     *
     * NoContent()
     *   204 No Content. Signals success with nothing to return (e.g. after DELETE).
     *
     * EmptyResult (new EmptyResult())
     *   200 with a truly empty body. Rarely needed; prefer NoContent() for 204.
     */
    public IActionResult Teapot()
    {
        return StatusCode(418); // 418 I'm a Teapot — demonstrates arbitrary status code
    }

    public IActionResult Missing()
    {
        return NotFound(); // 404 Not Found — no body
    }

    public IActionResult MissingWithDetail()
    {
        return NotFound(new { message = "The requested resource was not found." }); // 404 + JSON
    }

    public IActionResult Ping()
    {
        return Ok(); // 200 OK — no body; signals success with no data to return
    }

    public IActionResult Reject()
    {
        return BadRequest(new { error = "Invalid request parameters." }); // 400 + error body
    }

    public IActionResult SaveResult()
    {
        var created = new { id = 99, name = "New Item" };
        return Created("/products/99", created); // 201 + Location: /products/99 + JSON body
    }

    public IActionResult Clear()
    {
        return NoContent(); // 204 — success, nothing to return (typical after DELETE or PUT)
    }

    public IActionResult Nothing()
    {
        return new EmptyResult(); // 200 with empty body — different from NoContent() (204)
    }

    /* ── SECTION 9: [NonAction] AND [ActionName] ──────────────────────────── */
    /*
     * [NonAction]
     *   Marks a public instance method so MVC routing IGNORES it.
     *   Use when a public method is needed for other reasons (interface implementation,
     *   testability, inheritance) but should never be reachable as an HTTP endpoint.
     *   Without [NonAction], any public instance method is treated as a potential action.
     *
     * [ActionName("Name")]
     *   Overrides the action name used by the routing system and URL helpers.
     *   The METHOD name (GetContact) is irrelevant to routing; only the attribute value
     *   ("Contact") determines the URL segment and how RedirectToAction / Url.Action work.
     *
     *   Use cases:
     *     • When the desired URL segment is a C# keyword (e.g. [ActionName("delete")])
     *     • When multiple overloads serve the same action (GET vs POST)
     *     • When legacy URLs must be preserved after a method rename
     *
     *   Example: this action is reachable at /Home/Contact (not /Home/GetContact).
     *   RedirectToAction("Contact") targets this action correctly.
     */
    [NonAction]                    // public but NOT an HTTP endpoint — routing skips it
    public string GetVersion()
    {
        return "1.0.0";           // callable by other code; never hit via HTTP
    }

    [ActionName("Contact")]        // route: /Home/Contact — method name GetContact is ignored
    public IActionResult GetContact()
    {
        return Content("Contact: contact@example.com"); // reachable at /Home/Contact
    }

    /* ── SECTION 10: ACCESSING HttpContext, Request, Response, RouteData ──── */
    /*
     * ControllerBase exposes shortcuts to the current HTTP context:
     *
     *   Property        Type                    What it provides
     *   ─────────────   ─────────────────────   ─────────────────────────────────────────
     *   HttpContext     HttpContext              Full request + response wrapper for the request
     *   Request         HttpRequest             URL, headers, body, query string, cookies
     *   Response        HttpResponse            Status code, headers, response body stream
     *   RouteData       RouteData               Route values matched by the routing system
     *   User            ClaimsPrincipal         Authenticated user's identity and claims
     *   ModelState      ModelStateDictionary    Validation errors from model binding
     *
     * These are properties set by the framework before the action runs.
     * Avoid passing HttpContext into services — inject IHttpContextAccessor instead
     * so the service does not depend on the controller.
     *
     * Request.Path returns PathString (implicit string? conversion via .ToString()).
     * Request.Headers["Key"] returns StringValues (.ToString() gives first value).
     * RouteData.Values["key"] returns object? (use ?. and .ToString() for string).
     */
    public IActionResult Info()
    {
        string method = Request.Method;                               // "GET", "POST", etc.
        string path = Request.Path.ToString();                        // e.g. "/Home/Info"
        string userAgent = Request.Headers["User-Agent"].ToString();  // browser / client identifier
        string? controller = RouteData.Values["controller"]?.ToString(); // "Home"
        string? action = RouteData.Values["action"]?.ToString();         // "Info"
        string scheme = Request.Scheme;                               // "https" or "http"
        return Content(
            $"Method={method} | Path={path} | Scheme={scheme} | " +
            $"Controller={controller} | Action={action} | UA={userAgent}");
    }

    /* ── SECTION 11: ViewData / TempData — PREVIEW ───────────────────────── */
    /*
     * Controllers pass data to views through three mechanisms:
     *
     *   ViewData (ViewDataDictionary)
     *     Key-value dictionary available in the view via @ViewData["Key"].
     *     Values survive only the CURRENT request — they are cleared after the
     *     response is sent. Type is object?; cast in the view if needed.
     *
     *   ViewBag (dynamic)
     *     Dynamic wrapper around ViewData. ViewBag.Heading == ViewData["Heading"].
     *     Convenient but loses compile-time type safety. Prefer ViewData or
     *     strongly typed models for production code.
     *
     *   TempData (ITempDataDictionary)
     *     Survives exactly ONE redirect. Set before RedirectToAction(); read once
     *     in the destination action or its view. Values are marked for deletion
     *     after the first read. Backed by the session cookie by default.
     *     Common use: "flash messages" (success/error banners after POST-Redirect-GET).
     *
     * COVERED IN DETAIL LATER → 12. TempData, ViewData & ViewBag
     */
    public IActionResult Preview()
    {
        ViewData["Heading"] = "Preview — ViewData set by controller";    // read in About.cshtml
        ViewData["Message"] = "ViewData lasts for the current request.";
        TempData["Flash"] = "Saved! This message survives one redirect."; // survives redirect
        return View("About"); // reuse About.cshtml to show ViewData["Heading"] and ["Message"]
    }
}
