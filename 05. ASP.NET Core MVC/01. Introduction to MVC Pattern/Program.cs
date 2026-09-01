/*
 * TOPIC: Introduction to MVC Pattern
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Model-View-Controller (MVC) is the foundational architectural pattern for
 *   full-stack, server-rendered web applications in ASP.NET Core. Before writing
 *   controllers, views, or models you need to understand WHY the pattern separates
 *   concerns the way it does, HOW each part fits into the ASP.NET Core pipeline,
 *   and WHAT conventions the framework provides so you write less configuration
 *   code and spend more time on features.
 *
 *   MVC is the backbone of enterprise web applications, e-commerce platforms,
 *   admin dashboards, and content sites built with .NET.
 *
 * WHAT YOU WILL LEARN:
 *    1. What the MVC pattern is and why separation of concerns matters
 *    2. MVC vs Razor Pages vs Minimal APIs vs Web API (comparison table)
 *    3. The role of each component: Model, View, Controller
 *    4. Convention over configuration — folder and naming conventions
 *    5. Service registration: AddControllersWithViews vs AddControllers vs AddRazorPages
 *    6. How MVC plugs into the ASP.NET Core middleware pipeline
 *    7. The full request lifecycle: routing → controller → action → view → response
 *    8. Controller discovery rules (naming and folder conventions)
 *    9. View discovery rules (Views/{Controller}/{Action}.cshtml fallback chain)
 *   10. Action results overview: IActionResult, ViewResult, JsonResult (PREVIEW → 02)
 *   11. Routing overview: conventional routing via MapControllerRoute (PREVIEW → 09)
 *   12. Minimal controller wiring in practice
 *   13. ViewModel — the Model layer of MVC
 *   14. View — Razor syntax and view discovery
 *   15. Layout — shared page shell
 *
 * CHAPTER MAP (multi-file project — open files in this order):
 *   SECTION  1  → Program.cs (this file)              What Is MVC
 *   SECTION  2  → Program.cs                          MVC vs Razor Pages vs Minimal APIs vs Web API
 *   SECTION  3  → Program.cs                          The Role of Each Component
 *   SECTION  4  → Program.cs                          Convention over Configuration
 *   SECTION  5  → Program.cs                          Service Registration
 *   SECTION  6  → Program.cs                          MVC in the ASP.NET Core Pipeline
 *   SECTION  7  → Program.cs                          Request Lifecycle through MVC
 *   SECTION  8  → Program.cs                          Controller Discovery Rules
 *   SECTION  9  → Program.cs                          View Discovery Rules
 *   SECTION 10  → Program.cs                          Action Results Overview (PREVIEW)
 *   SECTION 11  → Program.cs                          Routing Overview (PREVIEW)
 *   SECTION 12  → Controllers/HomeController.cs       Controller + Action Method
 *   SECTION 13  → Models/ProductViewModel.cs          ViewModel: M in MVC
 *   SECTION 14  → Views/Home/Index.cshtml             View: Razor Syntax + View Discovery
 *   SECTION 15  → Views/Shared/_Layout.cshtml         Layout: Shared Page Shell
 */

/*
 * SECTION 1: WHAT IS MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * Model-View-Controller (MVC) is a software architectural pattern that divides
 * a web application into three distinct, collaborating components:
 *
 *   Model      — the data and business logic layer
 *   View       — the presentation layer (Razor HTML templates)
 *   Controller — the orchestration layer (handles HTTP requests, coordinates flow)
 *
 * WHY SEPARATION OF CONCERNS MATTERS:
 *   Testability      Controllers and models can be unit-tested without a browser.
 *   Maintainability  Each layer evolves independently — swapping the view engine
 *                    does not change business logic; changing a model does not
 *                    require rewriting HTML templates.
 *   Team parallelism Front-end and back-end developers work in different layers
 *                    simultaneously with well-defined contracts.
 *   Reuse            A model tested once can back multiple views (web + API).
 *
 * HISTORICAL CONTEXT:
 *   MVC was introduced in ASP.NET MVC 1 (2009) as an alternative to Web Forms,
 *   which mixed business logic and HTML in .aspx code-behind files. ASP.NET Core
 *   MVC is its cross-platform successor — same pattern, rebuilt on the ASP.NET
 *   Core host and DI container introduced in .NET Core 1.0 (2016).
 *
 * The pattern is language-agnostic: Rails (Ruby), Django (Python), Spring MVC
 * (Java), and Laravel (PHP) use the same separation. Concepts transfer broadly.
 */

/*
 * SECTION 2: MVC VS RAZOR PAGES VS MINIMAL APIS VS WEB API
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core supports four major programming models. Choose based on the
 * type of application you are building.
 *
 * Model             Best for                        Key characteristic
 * ────────────────  ──────────────────────────────  ──────────────────────────────────────
 * MVC               Server-rendered web apps with   Controllers orchestrate; Views render;
 *                   complex workflows, deep reuse,   Models carry data. Explicit per-feature
 *                   or large team separation        controller class.
 *
 * Razor Pages       CRUD-heavy page-focused apps    One PageModel class per page —
 *                   where one page = one feature    simpler mental model; less boilerplate
 *                                                   for small to mid-size web sites.
 *
 * Minimal APIs      Lightweight REST/JSON services, No controller class; route handlers
 *                   microservices, BFF endpoints    are inline lambdas or methods.
 *                                                   Maximum throughput; least ceremony.
 *
 * Web API (MVC)     JSON/XML REST APIs consumed by  Uses MVC controller classes but
 *                   SPA, mobile, or other services  returns data (JSON), not HTML views.
 *                                                   Built on AddControllers().
 *
 * CAN YOU MIX THEM?
 *   Yes. A single ASP.NET Core app can run MVC controllers, Razor Pages, and
 *   Minimal API routes simultaneously. Call AddControllersWithViews() +
 *   AddRazorPages() and mix app.MapGet() alongside app.MapControllerRoute().
 *   Each model handles the routes it is mapped to.
 *
 * THIS MODULE (05. ASP.NET Core MVC) focuses on full MVC — controllers, Razor
 * views, view models, layouts, and form handling. Minimal APIs are separate.
 */

/*
 * SECTION 3: THE ROLE OF EACH COMPONENT
 * ─────────────────────────────────────────────────────────────────────────────
 * Understanding each component's responsibility boundary prevents the most
 * common MVC anti-patterns: fat controllers, logic in views, and direct entity
 * rendering (over-posting).
 *
 * ── MODEL ─────────────────────────────────────────────────────────────────────
 *   What it is: Represents data and the rules that govern that data.
 *   Sub-types in practice:
 *     Entity / Domain model   maps to database rows; used in the data layer
 *     ViewModel               shaped for a specific view; avoids over-posting
 *     DTO                     data transfer object across service boundaries
 *     Input model             form submission payload; carries [Required] etc.
 *
 *   Responsibilities:
 *     ✓ Hold data properties
 *     ✓ Carry data annotations for validation and display
 *     ✓ Express business rules as methods or validation logic
 *     ✗ Must NOT know about HTTP, HTML, controllers, or views
 *
 *   See Models/ProductViewModel.cs (SECTION 13) for a concrete example.
 *
 * ── VIEW ──────────────────────────────────────────────────────────────────────
 *   What it is: A .cshtml Razor template that renders HTML from a model.
 *   Responsibilities:
 *     ✓ Display data provided by the controller
 *     ✓ Build HTML structure, loops over collections, conditional markup
 *     ✗ Must NOT query databases, call services, or contain business logic
 *     ✗ Must NOT perform significant computation
 *
 *   Razor syntax: C# embedded in HTML via @{...} blocks and @expression.
 *   See Views/Home/Index.cshtml (SECTION 14) for a concrete example.
 *
 * ── CONTROLLER ────────────────────────────────────────────────────────────────
 *   What it is: A C# class with action methods that handle HTTP requests.
 *   Responsibilities:
 *     ✓ Accept and validate input (route values, query string, form, JSON)
 *     ✓ Call services / repositories for data or business operations
 *     ✓ Construct the ViewModel and pass it to the View
 *     ✓ Return an IActionResult (View, Json, Redirect, NotFound, etc.)
 *     ✗ Must NOT contain business rules or data-access logic
 *     ✗ Should be thin — delegate heavy lifting to injected services
 *
 *   See Controllers/HomeController.cs (SECTION 12) for a concrete example.
 *
 * ── DATA FLOW ─────────────────────────────────────────────────────────────────
 *   Browser request
 *     ↓  URL matched by router
 *   Controller action method is called
 *     ↓  calls injected service / repository
 *   Service returns domain data
 *     ↓  controller maps to ViewModel
 *   View renders ViewModel to HTML
 *     ↓
 *   Browser receives HTML response
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
 * SECTION 4: CONVENTION OVER CONFIGURATION
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core MVC follows the "Convention over Configuration" (CoC) principle:
 * write less configuration code by following naming and folder conventions that
 * the framework understands automatically.
 *
 * KEY CONVENTIONS (these remove explicit configuration for most applications):
 *
 *   Convention              Rule                                    Example
 *   ──────────────────────  ──────────────────────────────────────  ────────────────────────────
 *   Controller naming       Class name ends with "Controller"       HomeController, ProductController
 *   Controller folder       Lives in Controllers/ (convention)      Controllers/HomeController.cs
 *   View folder structure   Views/{ControllerName}/{ActionName}     Views/Home/Index.cshtml
 *   Shared views            Views/Shared/{ViewName}                 Views/Shared/_Layout.cshtml
 *   Layout file             _Layout.cshtml in Views/Shared/         referenced via Layout property
 *   Action name             Public method on a Controller class     public IActionResult Index()
 *   Default route segment   {controller=Home}/{action=Index}/{id?}  /  →  HomeController.Index()
 *
 * OVERRIDE WHEN NEEDED:
 *   [Route("custom-path")]          override routing convention per action/controller
 *   return View("../Shared/Error")  override view location with a relative path
 *   [ActionName("Alias")]           rename the action from the URL's perspective
 *   ViewLocationExpander            add custom view search paths globally (Startup)
 *
 * CoC is a design philosophy, not a constraint. Every convention can be overridden
 * explicitly — but you only write that override when your case genuinely differs
 * from the convention. The default covers the vast majority of applications.
 */
var builder = WebApplication.CreateBuilder(args); // phase 1: configure services and host

/*
 * SECTION 5: SERVICE REGISTRATION — AddControllersWithViews vs Alternatives
 * ─────────────────────────────────────────────────────────────────────────────
 * MVC is registered with the DI container as a set of services.
 * Choose the right registration method based on what your app renders:
 *
 *   Method                          Includes                         Use when
 *   ──────────────────────────────  ───────────────────────────────  ─────────────────────────────
 *   AddControllersWithViews()       Controllers + Razor Views        Full MVC web app (this chapter)
 *   AddControllers()                Controllers only (no Views)      Web API / JSON-only service
 *   AddRazorPages()                 Razor Pages only                 Page-centric CRUD apps
 *   AddMvc()                        Controllers + Views + Pages      Full stack; rarely needed today
 *
 * WHAT AddControllersWithViews() REGISTERS:
 *   IActionDescriptorCollectionProvider   discovers controller action descriptors
 *   IModelMetadataProvider                model binding + validation metadata
 *   IViewEngine (Razor)                   discovers and compiles .cshtml views
 *   IActionResultExecutor variants        executes ViewResult, JsonResult, etc.
 *   Antiforgery services                  CSRF token generation and validation
 *   Model binders                         binds request data to action parameters
 *   Data annotation validation            runs [Required], [MaxLength], [Range] etc.
 *   Tag helper support                    enables <form asp-action="...">, etc.
 *
 * DOES NOT REGISTER (add separately if needed):
 *   Authentication / Authorization        builder.Services.AddAuthentication()
 *   Identity                              builder.Services.AddDefaultIdentity<T>()
 *   Entity Framework Core                 builder.Services.AddDbContext<T>()
 *   Your own services                     builder.Services.AddScoped<MyService>()
 *
 * COVERED IN DETAIL LATER:
 *   Tag Helpers  → 08. Tag Helpers
 *   Validation   → 07. Data Annotations & Validation
 *   Model Binding → 06. Model Binding in MVC
 */
builder.Services.AddControllersWithViews(); // register MVC controllers + Razor view engine

var app = builder.Build(); // phase 2: DI container sealed; build the host

/*
 * SECTION 6: MVC IN THE ASP.NET CORE PIPELINE
 * ─────────────────────────────────────────────────────────────────────────────
 * MVC is not a special host — it is an endpoint registered in the same middleware
 * pipeline as any other ASP.NET Core component. Canonical MVC pipeline order:
 *
 *   Request arrives at Kestrel
 *     ↓
 *   [1] UseDeveloperExceptionPage / UseExceptionHandler   error handling
 *     ↓
 *   [2] UseHttpsRedirection                               HTTP → HTTPS redirect
 *     ↓
 *   [3] UseStaticFiles                                    serve wwwroot/ (short-circuits)
 *     ↓
 *   [4] UseRouting                                        match URL to endpoint descriptor
 *     ↓
 *   [5] UseAuthentication                                 read/validate identity token or cookie
 *     ↓
 *   [6] UseAuthorization                                  apply [Authorize] policies
 *     ↓
 *   [7] MapControllerRoute                                register MVC endpoints
 *     ↓
 *   MVC: controller action executes → View renders → HTML response sent
 *
 * ORDER IS CRITICAL:
 *   UseStaticFiles before UseRouting      short-circuits early for static assets
 *   UseAuthentication before UseAuthorization  identity must be established before policy checks
 *   UseRouting before UseAuthorization    route metadata (e.g. endpoint [Authorize] attribute)
 *                                         must be available when authorization runs
 *
 * In .NET 6+, UseRouting and UseEndpoints are called implicitly when you call
 * MapControllerRoute(). Explicit calls are only needed when you must place
 * middleware between routing and endpoint execution (rare).
 *
 * COVERED IN DETAIL LATER:
 *   Middleware Pipeline → ASP.NET Core / 03. Middleware Pipeline
 *   Authentication      → ASP.NET Core / 15. Authentication & Authorization
 */
if (app.Environment.IsDevelopment())            // guard: dev-only tooling
{
    app.UseDeveloperExceptionPage();             // full stack trace in browser; never expose in Production
}
app.UseHttpsRedirection();                       // redirect HTTP → HTTPS
app.UseStaticFiles();                            // serve wwwroot/ files early (short-circuits pipeline)
app.UseRouting();                                // match incoming URL to an endpoint descriptor
app.UseAuthorization();                          // apply [Authorize] policies after routing

/*
 * SECTION 7: REQUEST LIFECYCLE THROUGH MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * Once a request passes through middleware and reaches the MVC endpoint, the
 * following steps execute in order:
 *
 *   Step 1 — ROUTING
 *     The URL pattern is matched against registered routes.
 *     /Home/Index → controller = "Home" → HomeController, action = "Index"
 *     /products/details/5 → controller = "products" → ProductsController,
 *                           action = "details", id = 5
 *
 *   Step 2 — CONTROLLER ACTIVATION
 *     The framework instantiates the controller class via DI.
 *     Constructor-injected services (IProductService, ILogger<T>, etc.)
 *     are resolved from the DI container scoped to this HTTP request.
 *
 *   Step 3 — MODEL BINDING
 *     Request data (route values, query string, form fields, JSON body) is
 *     mapped to action method parameters automatically.
 *     int id ← route value "5"; string search ← query "?search=widget"
 *     COVERED IN DETAIL LATER → 06. Model Binding in MVC
 *
 *   Step 4 — ACTION FILTERS (before the action runs)
 *     [Authorize], [ValidateAntiForgeryToken], custom IActionFilter run
 *     before the action body executes. Can short-circuit with a result.
 *     COVERED IN DETAIL LATER → 11. Action Filters in MVC
 *
 *   Step 5 — ACTION EXECUTION
 *     The action method body runs: calls services, builds a ViewModel, and
 *     returns an IActionResult (typically View(model)).
 *
 *   Step 6 — RESULT FILTERS (after the action returns)
 *     IResultFilter.OnResultExecuting fires before the result is rendered.
 *
 *   Step 7 — RESULT EXECUTION
 *     IActionResult.ExecuteResultAsync() is called.
 *     For ViewResult: the Razor view engine locates the .cshtml template,
 *     renders the model to HTML, and writes the HTML to the response stream.
 *
 *   Step 8 — RESPONSE
 *     The rendered HTML flows back through middleware (in reverse order)
 *     and Kestrel sends the response to the client.
 *
 * The full cycle from URL to HTML typically takes < 5 ms for a simple CRUD page.
 */

/*
 * SECTION 8: CONTROLLER DISCOVERY RULES
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core MVC discovers controllers automatically during startup. A class
 * is treated as a controller if ALL of the following hold:
 *
 *   Rule                                   Example satisfying the rule
 *   ─────────────────────────────────────  ──────────────────────────────────────────
 *   Class is public                        public class HomeController
 *   Class is not abstract                  concrete class (not abstract)
 *   Class name ends with "Controller"      HomeController, ProductController
 *   OR inherits Controller/ControllerBase  public class Products : Controller
 *   Not decorated with [NonController]     no attribute suppressing discovery
 *   Not a generic open type                HomeController, not HomeController<T>
 *
 * FOLDER: Controllers/ is a team convention, NOT a framework requirement.
 *   A public class named ProductController placed anywhere in the project
 *   assembly is discovered as a controller. The Controllers/ folder is the
 *   default because conventions guide where readers look for controllers.
 *
 * APPLICATION PARTS:
 *   Controllers can be discovered from external assemblies registered as
 *   Application Parts — useful for plugin architectures and modular monoliths:
 *     builder.Services.AddControllersWithViews()
 *         .AddApplicationPart(typeof(ExternalController).Assembly);
 *
 * EXCLUDING A CLASS OR METHOD:
 *   [NonController]  exclude a class that matches naming rules from discovery
 *   [NonAction]      exclude a public method from being treated as an action
 *
 * COVERED IN DETAIL LATER → 02. Controllers & Actions
 */

/*
 * SECTION 9: VIEW DISCOVERY RULES
 * ─────────────────────────────────────────────────────────────────────────────
 * When an action returns View() with no explicit path, the Razor view engine
 * searches for the .cshtml file using this fallback chain:
 *
 *   Search order (first file found wins):
 *   1. Views/{ControllerName}/{ActionName}.cshtml
 *      (specific: controller + action match)
 *      e.g. Views/Home/Index.cshtml  for  HomeController.Index()
 *
 *   2. Views/Shared/{ActionName}.cshtml
 *      (shared: available to all controllers)
 *      e.g. Views/Shared/Index.cshtml
 *
 *   3. Pages/Shared/{ActionName}.cshtml  (only if Razor Pages is also registered)
 *
 * If none is found: InvalidOperationException lists all searched paths.
 *
 * OVERRIDE DISCOVERY — pass an explicit path to View():
 *   return View("~/Views/Home/Index.cshtml");   // absolute from project root
 *   return View("../Shared/Error");             // relative from current view folder
 *
 * RETURNING A DIFFERENT VIEW from an action:
 *   return View("Details", model);   // renders Views/Home/Details.cshtml from Index action
 *
 * VIEWS/SHARED/ SPECIAL FILES:
 *   _Layout.cshtml       page shell (header, nav, footer) used by most views
 *   _ViewStart.cshtml    runs before every view; typically sets Layout = "_Layout"
 *   _ViewImports.cshtml  global @using and @addTagHelper directives for all views
 *   Error.cshtml         shown by UseExceptionHandler on unhandled errors
 *
 * AREA VIEWS (when Areas feature is used):
 *   Areas/{AreaName}/Views/{ControllerName}/{ActionName}.cshtml
 *   COVERED IN DETAIL LATER → 10. Areas
 */

/*
 * SECTION 10: ACTION RESULTS OVERVIEW — PREVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * Action methods return IActionResult (or Task<IActionResult> for async).
 * The framework calls ExecuteResultAsync() to convert the result to HTTP.
 *
 *   IActionResult type       Returned by                   HTTP outcome
 *   ───────────────────────  ────────────────────────────  ───────────────────────────────
 *   ViewResult               View(model)                   200 + rendered HTML from .cshtml
 *   PartialViewResult        PartialView(viewName, model)  200 + partial HTML (no layout)
 *   JsonResult               Json(object)                  200 + JSON body
 *   RedirectResult           Redirect(url)                 302 Found → external URL
 *   RedirectToActionResult   RedirectToAction(action)      302 Found → MVC action
 *   RedirectToRouteResult    RedirectToRoute(name)         302 → named route
 *   ContentResult            Content(text, mediaType)      200 + raw text / HTML / XML
 *   FileResult               File(bytes, mediaType)        200 + binary file download
 *   StatusCodeResult         StatusCode(code)              any HTTP status code
 *   NotFoundResult           NotFound()                    404 Not Found
 *   BadRequestResult         BadRequest()                  400 Bad Request
 *   UnauthorizedResult       Unauthorized()                401 Unauthorized
 *   NoContentResult          NoContent()                   204 No Content
 *   OkObjectResult           Ok(object)                    200 + JSON (preferred in Web API)
 *
 * Controller base class provides factory methods — call View() not new ViewResult()
 * — keeping action methods concise and the return type intention explicit.
 *
 * COVERED IN DETAIL LATER → 02. Controllers & Actions
 */

/*
 * SECTION 11: ROUTING OVERVIEW — PREVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core MVC supports two routing styles. Both can coexist in one app.
 *
 *   Style                Registration                       Example URL
 *   ───────────────────  ─────────────────────────────────  ──────────────────────
 *   Conventional         MapControllerRoute(pattern)        /products/details/5
 *   Attribute routing    [Route], [HttpGet], [HttpPost]     /api/v1/products/5
 *
 * CONVENTIONAL ROUTING (used in this file):
 *   The route template  {controller=Home}/{action=Index}/{id?}  decodes as:
 *
 *   Token               Meaning
 *   ──────────────────  ────────────────────────────────────────────────────
 *   {controller}        Maps to a controller class (minus "Controller" suffix)
 *   {action}            Maps to a public method name on that controller
 *   {id?}               Optional route parameter; becomes method parameter (int? id)
 *   =Home               Default value — used when the URL segment is absent
 *   =Index              Default value — used when the URL segment is absent
 *
 *   /                   → controller=Home  → HomeController.Index()
 *   /Home/Index         → same as /
 *   /Product/List       → ProductController.List()
 *   /Product/Details/3  → ProductController.Details(id: 3)
 *
 * ATTRIBUTE ROUTING (more explicit control, preferred for Web API):
 *   [Route("products/{id:int}")]
 *   public IActionResult Details(int id) { ... }
 *
 * ROUTE CONSTRAINTS:
 *   {id:int}            must be an integer
 *   {id:guid}           must be a GUID
 *   {id:minlength(3)}   string at least 3 characters
 *   {id:range(1,100)}   integer within inclusive range
 *
 * COVERED IN DETAIL LATER → 09. Routing & Attribute Routing
 */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Home/Index is the default landing

app.Run(); // start Kestrel; this call blocks until the application shuts down

/*
 * QUICK REFERENCE — Introduction to MVC Pattern
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * 1. REGISTER MVC SERVICES
 *    builder.Services.AddControllersWithViews()   // full MVC (controllers + Razor views)
 *    builder.Services.AddControllers()            // API only, no views
 *    builder.Services.AddRazorPages()             // Razor Pages only
 *
 * 2. MINIMAL MVC PIPELINE (order matters)
 *    app.UseDeveloperExceptionPage()              // dev only
 *    app.UseHttpsRedirection()
 *    app.UseStaticFiles()
 *    app.UseRouting()
 *    app.UseAuthentication()                      // if using auth
 *    app.UseAuthorization()
 *    app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
 *    app.Run();
 *
 * 3. CONTROLLER CONVENTIONS
 *    public class HomeController : Controller     // : Controller adds View(), Json() etc.
 *    public IActionResult Index() { ... }         // public method = discoverable action
 *    [NonAction]                                  // opt a public method out of routing
 *    [NonController]                              // opt a class out of controller discovery
 *
 * 4. VIEW DISCOVERY CHAIN (return View() with no path)
 *    Views/{Controller}/{Action}.cshtml  →  Views/Shared/{Action}.cshtml
 *    Override: return View("~/Views/Home/Custom.cshtml");
 *    Different view: return View("Details", model);
 *
 * 5. ACTION RESULT QUICK PICKS
 *    return View(model);               // render .cshtml with typed model → 200 HTML
 *    return Json(data);                // 200 + JSON
 *    return RedirectToAction("Index"); // 302 to another action in same controller
 *    return NotFound();                // 404
 *    return BadRequest(ModelState);    // 400 + validation errors
 *
 * 6. COMMON ROUTE TEMPLATE TOKENS
 *    {controller=Home}   default controller when URL segment is missing
 *    {action=Index}      default action when URL segment is missing
 *    {id?}               optional id (int? parameter in method)
 *    {id:int}            constrained to integers only
 *
 * NEXT CHAPTERS:
 *   02. Controllers & Actions              full controller depth, action results, filters
 *   03. Views & Razor Syntax               Razor syntax, Html helpers, Tag helpers
 *   04. Layouts Sections & Partial Views   layouts, sections, partial views
 *   05. ViewModels & Strongly Typed Views  ViewModel patterns, display attributes
 *   06. Model Binding in MVC               binding sources, complex types, collections
 *   07. Data Annotations & Validation      [Required], [MaxLength], custom validators
 *   09. Routing & Attribute Routing        route constraints, attribute routing, areas
 */
