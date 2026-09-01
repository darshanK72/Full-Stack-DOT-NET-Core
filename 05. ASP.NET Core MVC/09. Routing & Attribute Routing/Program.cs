/*
 * TOPIC: Routing & Attribute Routing in ASP.NET Core MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Routing maps every incoming HTTP request URL to a specific controller action.
 *   Without routing, ASP.NET Core cannot dispatch GET /products/5 to
 *   ProductsController.Details(5).  Two complementary strategies share the same
 *   underlying routing engine, introduced as "endpoint routing" in ASP.NET Core 3.0:
 *
 *     • Conventional routing — centralized URL templates in Program.cs
 *                              (MapControllerRoute); easy to audit; good for MVC apps
 *     • Attribute routing    — templates on controllers/actions via [Route] and HTTP
 *                              verb attributes; co-located with code; good for REST APIs
 *
 *   Both strategies share the same engine for route selection, constraint evaluation,
 *   named routes, and URL generation.
 *
 * WHAT YOU WILL LEARN:
 *    1. Routing overview — conventional vs attribute strategies
 *    2. Endpoint routing architecture — UseRouting, phase 1 vs phase 2
 *    3. MVC service registration — AddControllersWithViews
 *    4. Routing middleware pipeline — order and responsibility of each call
 *    5. Default conventional route — MapControllerRoute, {controller=Home}/{action=Index}/{id?}
 *    6. Multiple conventional routes & ordering — most specific first
 *    7. Route constraints in conventional routing — inline and separate parameter
 *    8. Areas route registration — PREVIEW
 *    9. Conventional routing target — HomeController participates, ProductsController does not
 *   10. Accessing RouteData.Values inside a controller action
 *   11. URL generation in controllers — Url.Action, Url.RouteUrl, RedirectToAction, IUrlHelper
 *   12. Attribute routing on a controller — class-level [Route] prefix
 *   13. HTTP verb attributes — [HttpGet], [HttpPost], [HttpPut], [HttpDelete], [HttpPatch]
 *   14. Route tokens — [controller], [action], [area]
 *   15. Route constraints in attribute routing — {id:int}, {name:alpha:minlength(3)}
 *   16. Route names — Name = "product-detail" and uniqueness rules
 *   17. URL generation — CreatedAtAction, CreatedAtRoute, Url.RouteUrl
 *   18. Mixed conventional and attribute routing — BlogController
 *   19. Areas routing — PREVIEW
 *   20. Route debug view model — exposing RouteData to the view
 *   21. _ViewImports — tag helper registration and @using scope
 *   22. Shared layout — navigation via anchor tag helpers
 *   23. Tag helper URL generation — asp-action, asp-controller, asp-route-*, asp-route, asp-area
 *   24. Attribute route URL generation in views
 *
 * CHAPTER MAP (open files in this order):
 *   SECTION  1 → Program.cs                           Routing Overview
 *   SECTION  2 → Program.cs                           Endpoint Routing Architecture
 *   SECTION  3 → Program.cs                           MVC Service Registration
 *   SECTION  4 → Program.cs                           Routing Middleware Pipeline
 *   SECTION  5 → Program.cs                           Default Conventional Route
 *   SECTION  6 → Program.cs                           Multiple Routes & Ordering
 *   SECTION  7 → Program.cs                           Route Constraints (Conventional)
 *   SECTION  8 → Program.cs                           Areas Route Registration (PREVIEW)
 *   SECTION  9 → Controllers/HomeController.cs        Conventional Routing Target
 *   SECTION 10 → Controllers/HomeController.cs        Accessing Route Data
 *   SECTION 11 → Controllers/HomeController.cs        URL Generation in Controllers
 *   SECTION 12 → Controllers/ProductsController.cs   Attribute Routing on Controller
 *   SECTION 13 → Controllers/ProductsController.cs   HTTP Verb Attributes
 *   SECTION 14 → Controllers/ProductsController.cs   Route Tokens
 *   SECTION 15 → Controllers/ProductsController.cs   Route Constraints (Attribute)
 *   SECTION 16 → Controllers/ProductsController.cs   Route Names
 *   SECTION 17 → Controllers/ProductsController.cs   CreatedAtAction & Url.RouteUrl
 *   SECTION 18 → Controllers/BlogController.cs       Mixed Conventional + Attribute
 *   SECTION 19 → Controllers/BlogController.cs       Areas Routing (PREVIEW)
 *   SECTION 20 → Models/RouteDebugViewModel.cs       Route Debug View Model
 *   SECTION 21 → Views/_ViewImports.cshtml           Tag Helper Registration
 *   SECTION 22 → Views/Shared/_Layout.cshtml         Navigation Tag Helpers
 *   SECTION 23 → Views/Home/Index.cshtml             Tag Helper URL Generation
 *   SECTION 24 → Views/Products/Index.cshtml         Attribute Route URL in Views
 */

/*
 * SECTION 1: ROUTING OVERVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * Routing maps an incoming HTTP URL to an endpoint — a controller action, a
 * Razor Page handler, or a minimal API delegate — and extracts values from URL
 * segments into route data.
 *
 * CONVENTIONAL ROUTING:
 *   Templates registered centrally in Program.cs via MapControllerRoute.
 *   All controllers WITHOUT a class-level [Route] attribute participate.
 *
 *   Pros: all routes in one place; easy to audit and reorder
 *   Cons: URL shape is separated from the controller code it describes
 *
 * ATTRIBUTE ROUTING:
 *   Templates declared directly on controllers/actions via [Route] and [HttpGet] etc.
 *   Controllers WITH a class-level [Route] attribute participate ONLY in attribute
 *   routing — they are completely excluded from conventional routes.
 *
 *   Pros: template is co-located with the action; flexible per-action URLs
 *   Cons: harder to audit all routes at once; verbose in large controllers
 *
 * WHICH TO USE:
 *   HTML web apps (server-rendered):  conventional routing recommended
 *   REST APIs:                        attribute routing preferred (RESTful URLs)
 *   Mixed:                            use separate controllers per strategy
 *
 * IN THIS PROJECT:
 *   HomeController     — conventional only (no [Route] at class level) → SECTION 9
 *   ProductsController — attribute only   ([Route("[controller]")])    → SECTION 12
 *   BlogController     — mixed            (some actions add [Route])   → SECTION 18
 */

/*
 * SECTION 2: ENDPOINT ROUTING ARCHITECTURE
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core uses "endpoint routing" (since 3.0).  Route processing has two
 * distinct phases separated by other middleware:
 *
 *   PHASE 1 — Route matching  (UseRouting middleware):
 *     Inspects the incoming URL against all registered templates (conventional
 *     and attribute).  The first matching template wins.  The result — an
 *     "Endpoint" object — is stored in HttpContext.  RouteData.Values is
 *     populated with the extracted segment values.
 *
 *   PHASE 2 — Endpoint execution  (MapControllerRoute / MapRazorPages / MapGet …):
 *     The terminal middleware reads the matched Endpoint from HttpContext and
 *     invokes the corresponding action, page, or delegate.
 *
 * WHY TWO PHASES:
 *   Authorization and rate-limiting middleware can run BETWEEN the phases.
 *   UseAuthorization runs after Phase 1 so it can read [Authorize] from the
 *   matched endpoint before executing the action.
 *
 * KEY TYPES:
 *   RouteData             — matched segment values, data tokens (set by routing)
 *   IEndpointRouteBuilder — extension point for MapControllerRoute and friends
 *   IUrlHelper            — generates URLs within an HTTP context (controller/view)
 *   LinkGenerator         — generates URLs from ANY context (injected service)
 *   EndpointMetadata      — attributes on the endpoint (e.g. [Authorize], [Route])
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RoutingAttributeRouting;

/*
 * SECTION 3: MVC SERVICE REGISTRATION — AddControllersWithViews
 * ─────────────────────────────────────────────────────────────────────────────
 * AddControllersWithViews() registers the full MVC pipeline into the DI container:
 *
 *   Service registered                     Purpose
 *   ─────────────────────────────────────  ─────────────────────────────────────
 *   IActionInvokerFactory                  Executes controller actions
 *   IControllerActivator                   Creates controller instances (via DI)
 *   IModelBinderFactory                    Binds route/query/form values to params
 *   RazorViewEngine                        Locates & compiles .cshtml view files
 *   ITagHelperActivator                    Instantiates tag helpers from DI
 *   IDataAnnotationsModelValidatorProvider Validates models via [Required] etc.
 *   IUrlHelperFactory                      Creates IUrlHelper for Url.Action etc.
 *   IAntiforgery                           CSRF token services for <form asp-action>
 *
 * Compare registration methods:
 *
 *   Method                        Controllers  Views  Razor Pages
 *   ─────────────────────────     ───────────  ─────  ───────────
 *   AddControllers()                   ✓         ✗        ✗
 *   AddControllersWithViews()          ✓         ✓        ✗
 *   AddRazorPages()                    ✗         ✓        ✓
 *   AddMvc()                           ✓         ✓        ✓
 *
 * NOTE: Attribute route discovery happens here — the framework scans the
 * assembly for classes with [Route] at the class or action level and builds
 * the endpoint route table alongside the conventional routes added below.
 */
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args); // host + config + DI container
        builder.Services.AddControllersWithViews();                         // full MVC + Razor + tag helpers

        WebApplication app = builder.Build();

        /*
         * SECTION 4: ROUTING MIDDLEWARE PIPELINE
         * ─────────────────────────────────────────────────────────────────────────────
         * The middleware pipeline order is critical.  For routing the required order is:
         *
         *   Call                   Stage   Why this position
         *   ─────────────────────  ──────  ────────────────────────────────────────────
         *   UseStaticFiles()       before  Short-circuits requests for /wwwroot files;
         *                                  no route matching overhead for static assets
         *   UseRouting()           phase 1 Selects the matching endpoint; populates
         *                                  HttpContext.GetEndpoint() and RouteData
         *   UseAuthentication()    middle  Reads ClaimsPrincipal; AFTER routing so it
         *                                  can read route values (optional here)
         *   UseAuthorization()     middle  Checks [Authorize] on the matched endpoint;
         *                                  MUST come AFTER UseRouting
         *   MapControllerRoute()   phase 2 Executes the selected controller action
         *
         * WebApplication minimal hosting note:
         *   MapControllerRoute() implicitly calls UseRouting() if not already added.
         *   UseRouting() is explicit here so the pipeline order is visible in code.
         *   Explicit UseRouting() is REQUIRED when middleware between routing phases
         *   (UseAuthorization) needs to run AFTER phase 1 but BEFORE phase 2.
         *
         * IsDevelopment():
         *   Returns true when ASPNETCORE_ENVIRONMENT == "Development".
         *   In production, UseExceptionHandler shows a friendly error page.
         */
        if (!app.Environment.IsDevelopment())
            app.UseExceptionHandler("/Home/Error"); // user-friendly error page in production

        app.UseStaticFiles(); // serve /wwwroot BEFORE routing; short-circuits static file requests
        app.UseRouting();     // phase 1: select the matching endpoint; populate RouteData
        app.UseAuthorization(); // check [Authorize] metadata; MUST be after UseRouting

        /*
         * SECTION 5: DEFAULT CONVENTIONAL ROUTE — MapControllerRoute
         * ─────────────────────────────────────────────────────────────────────────────
         * MapControllerRoute registers ONE named route template in the endpoint table.
         * Multiple calls add multiple routes evaluated in registration order.
         *
         * ANATOMY OF THE DEFAULT PATTERN:
         *   "{controller=Home}/{action=Index}/{id?}"
         *    ─────────────────  ──────────────  ────
         *   {controller=Home}  — route parameter extracted from URL segment;
         *                        =Home is the inline DEFAULT used when the segment is absent
         *   {action=Index}     — route parameter; =Index is the default when absent
         *   {id?}              — route parameter; ? makes the segment OPTIONAL
         *                        (absent = id is null; present = value bound to "id")
         *
         * URL → action mapping for this route:
         *
         *   URL                  controller   action   id
         *   ──────────────────   ──────────   ──────   ──────
         *   /                    Home         Index    (null)
         *   /home                Home         Index    (null)
         *   /home/index          Home         Index    (null)
         *   /home/about          Home         About    (null)
         *   /home/about/7        Home         About    7
         *   /blog/index          Blog         Index    (null)
         *
         * NOTE about ProductsController:
         *   GET /products does NOT reach ProductsController via this route.
         *   ProductsController has class-level [Route("[controller]")] — it participates
         *   ONLY in attribute routing.  The routing middleware selects the attribute route
         *   "products" directly; the conventional route is never evaluated for /products.
         *   See SECTION 12 in Controllers/ProductsController.cs.
         *
         * ROUTE NAME ("default"):
         *   Names are used for URL generation and diagnostics.
         *   Url.RouteUrl("default", new { controller = "Home", action = "About" }) → "/home/about"
         *   Names must be UNIQUE across all registered routes.
         *   Duplicate names throw InvalidOperationException at startup.
         */
        app.MapControllerRoute(
            name:    "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"); // phase 2: execute the matched action

        /*
         * SECTION 6: MULTIPLE CONVENTIONAL ROUTES AND ORDERING
         * ─────────────────────────────────────────────────────────────────────────────
         * Routes are evaluated in REGISTRATION ORDER — the FIRST match wins.
         *
         * RULE: Register MORE SPECIFIC patterns BEFORE less specific ones.
         *
         * WRONG ORDER example (would produce 404 for /blog/archive/2024/11):
         *   app.MapControllerRoute("default",      "{controller=Home}/{action=Index}/{id?}");
         *   app.MapControllerRoute("blog-monthly", "blog/archive/{year}/{month}", ...);
         *   // The default route matches "blog/archive/2024/11" first:
         *   // controller="blog", action="archive", id="2024" — routes to Blog.Archive(id=2024)
         *   // NOT to the blog-monthly action with year=2024, month=11.
         *
         * CORRECT ORDER (specific before general):
         *   app.MapControllerRoute("blog-monthly", "blog/archive/{year}/{month}", ...);
         *   app.MapControllerRoute("default",      "{controller=Home}/{action=Index}/{id?}");
         *
         * NOTE: The default route (SECTION 5) is registered ABOVE this comment in the file,
         * meaning the blog-monthly route registered here CANNOT be reached via the
         * conventional route because the default route matches blog/archive/{year}/{month} first.
         * This is intentional — the tutorial uses BlogController.PostByDate via attribute routing
         * ([HttpGet("blog/{year:int}/{month:int:range(1,12)}/{slug}")]).  The routes here
         * exist to demonstrate SYNTAX, not to serve the running demo.
         *
         * DEFAULTS PARAMETER vs INLINE DEFAULTS:
         *   Equivalent approaches; separate defaults preferred when controller/action are fixed:
         *   Inline:   "blog/archive/{Blog=controller}/{Archive=action}/{year}/{month}"
         *   Separate: pattern: "blog/archive/{year}/{month}",
         *             defaults: new { controller = "Blog", action = "Archive" }
         *
         * NAMED ROUTE:
         *   "blog-monthly" enables:
         *     Url.RouteUrl("blog-monthly", new { year = 2024, month = 11 })  → /blog/archive/2024/11
         *     <a asp-route="blog-monthly" asp-route-year="2024" asp-route-month="11">
         */
        app.MapControllerRoute(
            name:     "blog-monthly",
            pattern:  "blog/archive/{year}/{month}",                     // fixed prefix; variable year & month
            defaults: new { controller = "Blog", action = "Archive" });  // BlogController.Archive (not defined)

        /*
         * SECTION 7: ROUTE CONSTRAINTS IN CONVENTIONAL ROUTING
         * ─────────────────────────────────────────────────────────────────────────────
         * Constraints narrow which URL values match a route template.
         * They are expressed in two equivalent ways:
         *
         * --- 7a. Inline constraints in the pattern ---
         *
         *   Syntax:  {parameter:constraint}  or chained  {parameter:c1:c2}
         *
         *   "blog/archive/{year:int:min(2000)}/{month:int:range(1,12)}"
         *     :int         — value must parse as a 32-bit integer
         *     :min(2000)   — integer must be ≥ 2000
         *     :int:range(1,12) — integer in [1, 12]
         *
         *   /blog/archive/2024/11  → matches   (year=2024, month=11)
         *   /blog/archive/abc/11   → no match  (year must be int) → next route tried
         *   /blog/archive/2024/13  → no match  (month > 12) → next route tried
         *
         * --- 7b. Separate `constraints` parameter ---
         *
         *   app.MapControllerRoute(
         *       name:        "...",
         *       pattern:     "blog/archive/{year}/{month}",
         *       constraints: new { year = @"\d{4}", month = new RangeRouteConstraint(1, 12) });
         *
         *   Accepts:
         *     • Regex string:            @"\d{4}"
         *     • IRouteConstraint type:   new RangeRouteConstraint(min, max)
         *     • Type name string:        "int"
         *
         * --- 7c. Full built-in constraint set (same in both conventional & attribute routing) ---
         *
         *   Constraint         Inline syntax                 Matches
         *   ───────────────    ──────────────────────────    ─────────────────────────────────
         *   int                {id:int}                      −2 147 483 648 to 2 147 483 647
         *   long               {id:long}                     64-bit integer
         *   double             {price:double}                floating-point
         *   bool               {flag:bool}                   true or false
         *   guid               {id:guid}                     Globally Unique Identifier
         *   alpha              {name:alpha}                  letters only (a-z, A-Z)
         *   minlength(n)       {name:minlength(3)}           string with ≥ n chars
         *   maxlength(n)       {name:maxlength(50)}          string with ≤ n chars
         *   length(n)          {name:length(5)}              string of exactly n chars
         *   min(n)             {age:min(18)}                 integer ≥ n
         *   max(n)             {age:max(120)}                integer ≤ n
         *   range(min,max)     {age:range(18,65)}            integer in [min, max]
         *   regex(expr)        {code:regex(^\\d{{5}}$)}      matches regex (double-brace escaping)
         *   required           {name:required}               non-empty string
         *
         * --- 7d. Constraint vs validation distinction ---
         *
         *   Constraints  → affect ROUTING (does this URL match this template?)
         *   DataAnnotations → affect MODEL VALIDATION (is the bound value valid?)
         *   Use constraints for type safety and URL disambiguation;
         *   use validation for business rules.
         *
         *   GOTCHA: no match = 404, not 400.
         *   GET /blog/archive/abc/5 with {year:int} → 404 (route doesn't match).
         *   Users may find this confusing; provide a catch-all route with a helpful error.
         *
         * --- 7e. Constraint effect on URL generation ---
         *
         *   Constraints are NOT enforced during URL generation by default.
         *   Url.RouteUrl("blog-monthly-constrained", new { year = "abc", month = 5 })
         *   succeeds at link generation even though "abc" would fail on an incoming request.
         */
        app.MapControllerRoute(
            name:     "blog-monthly-constrained",
            pattern:  "blog/archive/{year:int:min(2000)}/{month:int:range(1,12)}", // inline constraints
            defaults: new { controller = "Blog", action = "Archive" });            // 404 at runtime — syntax demo

        /*
         * SECTION 8: AREAS ROUTE REGISTRATION — PREVIEW
         * ─────────────────────────────────────────────────────────────────────────────
         * Areas partition a large MVC application into self-contained functional groups.
         * The area route MUST be registered BEFORE the default route; the :exists constraint
         * prevents every 3-segment URL from being tried as an area route.
         *
         * TWO AREA REGISTRATION STYLES:
         *
         * 1. Generic area route (one route covers all areas):
         *      app.MapControllerRoute(
         *          name:    "areas",
         *          pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
         *
         *    :exists — URL segment must match a registered area name (e.g. "Admin").
         *              Without it, any 3-segment URL is tried as an area route first.
         *
         * 2. Per-area route (explicit area name in route):
         *      app.MapAreaControllerRoute(
         *          name:     "admin-area",
         *          areaName: "Admin",
         *          pattern:  "Admin/{controller=Dashboard}/{action=Index}/{id?}");
         *
         * CONTROLLER ATTRIBUTE (required in both styles):
         *   [Area("Admin")]
         *   public class DashboardController : Controller { ... }
         *
         * LINK GENERATION FROM VIEWS:
         *   <a asp-area="Admin" asp-controller="Dashboard" asp-action="Index">Admin</a>
         *   @Url.Action("Index", "Dashboard", new { area = "Admin" })
         *
         *   PITFALL: Inside an area view, the current area is "ambient".
         *   A link without asp-area="" will reuse the ambient area — producing wrong hrefs.
         *   Always set asp-area="" explicitly when linking OUT of an area.
         *
         * COVERED IN DETAIL LATER → 10. Areas
         * This project has no Areas/ directory.  Uncomment the route to experiment.
         */
        // app.MapControllerRoute(
        //     name:    "areas",
        //     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}

/*
 * QUICK REFERENCE — Routing at a Glance
 *
 * ── CONVENTIONAL ROUTING (Program.cs) ──────────────────────────────────────
 * app.MapControllerRoute(name, pattern, defaults?, constraints?, dataTokens?)
 *
 * Common patterns:
 *   "{controller=Home}/{action=Index}/{id?}"                   — default
 *   "blog/archive/{year:int:min(2000)}/{month:int:range(1,12)}" — constrained
 *   "{area:exists}/{controller=Home}/{action=Index}/{id?}"     — areas
 *
 * Inline constraints:  {id:int}  {year:int:min(2000)}  {name:alpha:minlength(3)}
 * Separate constraints:  constraints: new { id = @"\d+" }
 *
 * ── ATTRIBUTE ROUTING (Controllers/) ───────────────────────────────────────
 * [Route("products")]                  controller prefix (literal)
 * [Route("[controller]")]              token → "products"
 * [Route("[controller]/[action]")]     tokens → "products/create"
 * [HttpGet]                            GET  (uses prefix alone)
 * [HttpGet("{id:int}")]                GET  + int constraint
 * [HttpPost]                           POST (uses prefix alone)
 * [HttpPut("{id:int}")]                PUT  + int constraint
 * [HttpDelete("{id:int}")]             DELETE + int constraint
 * [HttpPatch("{id:int}")]              PATCH + int constraint
 * [HttpGet("{id:int}", Name = "...")]  with named route
 *
 * ── URL GENERATION ──────────────────────────────────────────────────────────
 * Url.Action("Details", "Products", new { id = 5 })           → /products/5
 * Url.RouteUrl("product-detail", new { id = 5 })              → /products/5
 * RedirectToAction("Index", "Home")                            → 302 to /
 * CreatedAtAction(nameof(Details), new { id = 99 }, obj)      → 201 + Location
 * <a asp-action="Details" asp-controller="Products" asp-route-id="5">
 * <a asp-route="product-detail" asp-route-id="5">
 *
 * ── REGISTRATION ORDER (most specific first) ────────────────────────────────
 * 1. Area routes          {area:exists}/...
 * 2. Specific patterns    blog/archive/{year:int}/{month:int}
 * 3. Default route        {controller=Home}/{action=Index}/{id?}
 *
 * ── CONVENTIONAL vs ATTRIBUTE ───────────────────────────────────────────────
 * Class-level [Route]   → attribute routing ONLY (excluded from conventional routes)
 * No class-level [Route] → conventional routing; individual actions may add [Route] → mixed
 *
 * ── ROUTE DEBUGGING ─────────────────────────────────────────────────────────
 * RouteData.Values["controller"]          — matched controller name
 * RouteData.Values["action"]              — matched action name
 * HttpContext.GetEndpoint()?.DisplayName  — full endpoint description
 * ControllerContext.ActionDescriptor
 *   .AttributeRouteInfo?.Template         — non-null for attribute routes
 * appsettings.json: "Microsoft.AspNetCore.Routing": "Debug"
 */
