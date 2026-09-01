/*
 * TOPIC: Routing & Endpoints in ASP.NET Core
 *
 * WHY IT MATTERS:
 *   Every HTTP request that reaches an ASP.NET Core application must be matched to a
 *   handler before any response can be produced.  Routing is that matching process.
 *   Understanding endpoint routing unlocks:
 *     • Predictable, debuggable URL structures that map directly to business resources
 *     • Type-safe route parameter extraction — the framework rejects malformed inputs
 *     • Centralized link generation — no hardcoded URL strings scattered across the app
 *     • Composable groups, shared metadata, and lightweight per-endpoint filters
 *     • A clean separation between the middleware pipeline and endpoint execution
 *
 * WHAT YOU WILL LEARN:
 *    1. Product DTO                          → Models/Product.cs
 *    2. Attribute routing on a controller    → Controllers/ProductsController.cs
 *    3. Order endpoint group (MapGroup)      → Endpoints/OrderEndpoints.cs
 *    4. User endpoint group (MapGroup)       → Endpoints/UserEndpoints.cs
 *    5. IEndpointRouteBuilder extension      → Extensions/RouteBuilderExtensions.cs
 *    6. Endpoint routing pipeline            (UseRouting / UseEndpoints / ordering rules)
 *    7. MapGet / MapPost / MapPut / MapDelete / MapMethods
 *    8. Route templates and syntax           ({id}, {id?}, {id=default}, {**slug})
 *    9. Route constraints                    ({id:int}, {name:alpha}, {code:regex(...)})
 *   10. Link generation with LinkGenerator
 *   11. Route groups with MapGroup
 *   12. Endpoint metadata                   (WithName, WithTags, WithSummary)
 *   13. Endpoint filters                    (AddEndpointFilter — pipeline & short-circuit)
 *   14. Conventional routing in MVC         (MapControllerRoute)
 *   PREVIEW: Model Binding & Validation     → 08. Model Binding & Validation
 *   PREVIEW: Minimal APIs                   → 13. Minimal APIs
 *
 * CHAPTER MAP (open files in this order):
 *    1. Product DTO                     → Models/Product.cs
 *    2. Attribute routing on controller → Controllers/ProductsController.cs
 *    3. Order endpoint group            → Endpoints/OrderEndpoints.cs
 *    4. User endpoint group             → Endpoints/UserEndpoints.cs
 *    5. IEndpointRouteBuilder extension → Extensions/RouteBuilderExtensions.cs
 *    6. Routing pipeline & inline routes → Program.cs (sections below)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RoutingEndpoints.Endpoints;
using RoutingEndpoints.Extensions;
using System;

// ─── SERVICE REGISTRATION ────────────────────────────────────────────────────

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

/*
 * SECTION 6: ENDPOINT ROUTING PIPELINE
 *
 * ASP.NET Core 3.0+ uses ENDPOINT ROUTING — a two-phase model that separates
 * route matching from endpoint execution:
 *
 *   Phase 1 — UseRouting middleware:
 *     Inspects the request URL, matches it against all registered route templates,
 *     and stores the matched endpoint (and its metadata) on HttpContext.
 *     After this middleware, any subsequent middleware can read which endpoint was
 *     selected — enabling auth policies, CORS rules, and custom logic to act on
 *     the intended handler BEFORE it actually executes.
 *
 *   Phase 2 — UseEndpoints middleware (or implicit via Map* calls):
 *     Invokes the matched endpoint's handler delegate.
 *
 * In WebApplication (net6+), calling app.MapGet / app.MapControllers / etc. inserts
 * UseRouting and UseEndpoints automatically in the correct positions.
 * Explicit calls are only needed when custom middleware must run BETWEEN the two phases.
 *
 * Correct middleware ordering (wrong order causes auth/CORS bypasses):
 *   ┌────────────────────────────────────────────────────────────────────────┐
 *   │  UseRouting              — match; endpoint metadata available after    │
 *   │  UseCors                 — needs matched endpoint to apply CORS policy  │
 *   │  UseAuthentication       — verify identity (reads cookie / bearer token)│
 *   │  UseAuthorization        — enforce endpoint's [Authorize] metadata      │
 *   │  UseEndpoints / Map*     — execute matched handler                     │
 *   └────────────────────────────────────────────────────────────────────────┘
 *
 * Endpoint routing vs the old IRouter / UseMvc approach (pre-3.0):
 *   Old:  app.UseMvc(routes => routes.MapRoute(...))
 *   New:  app.MapGet(...) / app.MapControllers() / app.MapRazorPages()
 *         — composable, metadata-aware, testable, DI-friendly
 */
builder.Services.AddControllers(); // registers MVC + attribute routing + [ApiController] support
builder.Services.AddRouting();     // registers LinkGenerator, IEndpointRouteBuilder, route options
                                   // AddControllers() already calls AddRouting() internally;
                                   // this explicit call documents the dependency clearly.

WebApplication app = builder.Build();

// Explicit pipeline order — WebApplication would insert these automatically,
// but writing them explicitly shows the correct relative positions.
app.UseRouting();       // Phase 1: match request → endpoint (metadata now available to later middleware)
app.UseAuthorization(); // example: reads the matched endpoint's [Authorize] metadata

// ─── INLINE ROUTE REGISTRATIONS ──────────────────────────────────────────────

/*
 * SECTION 7: MapGet / MapPost / MapPut / MapDelete / MapMethods
 *
 * Map* methods are extension methods on IEndpointRouteBuilder (from Microsoft.AspNetCore.Builder).
 * Both WebApplication and RouteGroupBuilder implement IEndpointRouteBuilder, so the same
 * methods work at top level and inside groups.
 *
 * Common signatures:
 *   IEndpointConventionBuilder MapGet(string pattern, Delegate handler)
 *   IEndpointConventionBuilder MapPost(string pattern, Delegate handler)
 *   IEndpointConventionBuilder MapPut(string pattern, Delegate handler)
 *   IEndpointConventionBuilder MapDelete(string pattern, Delegate handler)
 *   IEndpointConventionBuilder MapMethods(string pattern, IEnumerable<string> httpMethods, Delegate handler)
 *
 * Handler delegate parameter binding (brief — FULL DEPTH in 08. Model Binding):
 *   Name matches route token  → bound from route values  (e.g. int id from {id:int})
 *   Known service type        → resolved from DI          (e.g. LinkGenerator)
 *   HttpContext / HttpRequest → special framework parameters
 *   Other complex types       → bound from request body as JSON
 *
 * Handler return values:
 *   IResult    — Results.Ok(), Results.NotFound(), Results.Created(), etc.
 *   string     — plain-text 200 response
 *   T (any)    — JSON-serialized 200 response
 *   void       — 200 with empty body
 *
 * All Map* calls MUST come after app.Build() and BEFORE app.Run().
 */

// Root handler — returns plain text (string return → Content-Type: text/plain, 200)
app.MapGet("/", () => "Routing & Endpoints tutorial is running.");

// MapPost — body-bound string parameter (minimal model binding preview)
app.MapPost("/api/echo", (string message) => Results.Ok(new { Echo = message }));

// MapPut — explicit 204 No Content; id comes from the {id:int} route token
app.MapPut("/api/items/{id:int}", (int id) =>
{
    bool valid = id > 0; // id bound from route; body binding shown in 08. Model Binding
    return valid ? Results.NoContent() : Results.NotFound();
});

// MapDelete — 204 No Content on success
app.MapDelete("/api/items/{id:int}", (int id) =>
{
    bool valid = id > 0;
    return valid ? Results.NoContent() : Results.NotFound();
});

/*
 * MapMethods — registers one handler for an explicit list of HTTP verbs.
 * Common uses: HEAD (same as GET but no body), OPTIONS, PATCH, custom verbs.
 *
 * IEnumerable<string> httpMethods:
 *   Pass a string array — string[] implements IEnumerable<string>.
 *   HTTP method names are matched case-insensitively internally but the
 *   HTTP specification requires UPPERCASE (GET, HEAD, OPTIONS, PATCH, …).
 */
app.MapMethods(
    "/api/ping",
    new string[] { "GET", "HEAD" },                          // both verbs reach this handler
    () => Results.Ok(new { Status = "pong", At = DateTime.UtcNow })); // DateTime → using System

/*
 * SECTION 8: ROUTE TEMPLATES AND SYNTAX
 *
 * Route templates are strings with literal segments and parameter tokens.
 * ASP.NET Core splits the request URL by "/" and matches each segment against
 * the template from left to right.
 *
 * Parameter token forms:
 *   {param}           required — must be present and non-empty (no slash)
 *   {param?}          optional — segment may be absent; parameter is nullable
 *   {param=default}   default value — absent segment uses "default"
 *   {param:constraint} constrained — must satisfy constraint (Section 9)
 *   {**slug}          catch-all — captures the rest of the URL including slashes
 *                     MUST be the last segment; cannot be optional ({**slug?} is invalid)
 *
 * Complex segment — literal text mixed with a parameter in one segment:
 *   /files/report-{year}.pdf   matches   /files/report-2024.pdf   (year = "2024")
 *   Only ONE parameter per complex segment is allowed.
 *
 * Specificity — when multiple templates could match:
 *   Literal segments > constrained parameters > unconstrained parameters > catch-all
 *   Example: /api/products/new wins over /api/products/{id} for the URL /api/products/new.
 */

// {id} — required; matches any single non-slash, non-empty value
app.MapGet("/api/catalog/{id}", (string id) => Results.Ok(new { Id = id }));

// {name?} — optional; GET /api/greet  OR  GET /api/greet/alice both match
app.MapGet("/api/greet/{name?}", (string? name) =>  // nullable string because optional
    Results.Ok(new { Message = $"Hello, {name ?? "world"}!" }));

// {version=1} — default value; GET /api/v/status → version = "1"
//                              GET /api/v/2/status → version = "2"
app.MapGet("/api/v/{version=1}/status", (string version) =>
    Results.Ok(new { ApiVersion = version }));

// {**slug} — catch-all; captures everything after /api/docs/ including slashes
// GET /api/docs/routing/templates/syntax → slug = "routing/templates/syntax"
app.MapGet("/api/docs/{**slug}", (string slug) =>
    Results.Ok(new { Document = slug, Path = slug.Split('/') }));

/*
 * SECTION 9: ROUTE CONSTRAINTS
 *
 * Constraints narrow route matching to values that satisfy a predicate.
 * Template syntax:  {parameter:constraint}
 * Chained:          {parameter:constraint1:constraint2}
 *
 * Built-in constraint types:
 *   ┌──────────────────┬─────────────────────────────────────────────────────────┐
 *   │ Constraint        │ Matches                                                │
 *   ├──────────────────┼─────────────────────────────────────────────────────────┤
 *   │ int               │ 32-bit integer (sign allowed)                          │
 *   │ long              │ 64-bit integer                                          │
 *   │ float / double    │ floating-point number                                  │
 *   │ decimal           │ decimal number                                          │
 *   │ bool              │ true or false                                           │
 *   │ guid              │ standard GUID format (with or without hyphens/braces)  │
 *   │ datetime          │ parseable by DateTime.Parse                             │
 *   │ alpha             │ one or more ASCII alphabetic characters                │
 *   │ length(min,max)   │ string length in [min, max]                            │
 *   │ minlength(n)      │ string at least n characters                           │
 *   │ maxlength(n)      │ string at most n characters                            │
 *   │ min(n)            │ integer >= n                                           │
 *   │ max(n)            │ integer <= n                                           │
 *   │ range(min,max)    │ integer in [min, max]                                  │
 *   │ regex(pattern)    │ matches regular expression                             │
 *   └──────────────────┴─────────────────────────────────────────────────────────┘
 *
 * Constraints affect ROUTE MATCHING, not validation:
 *   Failing constraint → 404 Not Found  (no route matched)
 *   Failing validation → 400 Bad Request (route matched; value rejected by rules)
 *   Do not use :int/:alpha as a substitute for business-rule validation.
 *
 * Custom constraints:
 *   Implement IRouteConstraint and register via:
 *   builder.Services.Configure<RouteOptions>(o =>
 *       o.ConstraintMap["slug"] = typeof(SlugConstraint));
 *   Then use {param:slug} in templates.
 */

// :int — only matches integer values; /api/products/abc → 404
app.MapGet("/api/products/{id:int}", (int id) =>
    Results.Ok(new { ProductId = id, Type = "integer-constrained" }));

// :guid — only matches standard GUID format
app.MapGet("/api/sessions/{sessionId:guid}", (Guid sessionId) =>
    Results.Ok(new { SessionId = sessionId }));

// :alpha — alphabetic characters only; digits fail this constraint
app.MapGet("/api/tags/{tag:alpha}", (string tag) =>
    Results.Ok(new { Tag = tag }));

// Chained constraints — :int AND :min(1): matches positive integers only
app.MapGet("/api/pages/{page:int:min(1)}", (int page) =>
    Results.Ok(new { Page = page }));

// :regex — matches exactly 2 uppercase letters (ISO 3166-1 alpha-2 country code)
// Inside a route template string, curly braces within a regex must be doubled:
//   {{ }} in the raw template string → the route parser converts them to { } in the regex.
app.MapGet("/api/countries/{code:regex(^[A-Z]{{2}}$)}", (string code) =>
    Results.Ok(new { CountryCode = code }));

/*
 * SECTION 10: LINK GENERATION WITH LinkGenerator
 *
 * LinkGenerator is an injectable service (Microsoft.AspNetCore.Routing) that
 * produces URLs from endpoint names and route values — the INVERSE of routing.
 *
 * Why use LinkGenerator instead of hardcoded URL strings?
 *   • Refactoring safety — rename a route template once; all generated links update.
 *   • Correctness — the service validates that all required route values are provided.
 *   • Host-awareness — absolute URL methods include the correct scheme and host.
 *
 * Key members:
 *   GetPathByName(name, values)              → "/api/items/42"  (path only, no host)
 *   GetUriByName(httpContext, name, values)  → "https://host/api/items/42" (absolute)
 *   GetPathByAddress<TAddress>(addr, values) → low-level; rarely needed directly
 *
 * Registration:
 *   LinkGenerator is registered automatically by AddRouting() and AddControllers().
 *   Inject via constructor DI in services or as a handler parameter in minimal APIs.
 *
 * Endpoint names:
 *   Set at registration with .WithName("UniqueName").
 *   Names must be application-wide unique.  Duplicates cause a runtime
 *   InvalidOperationException when GetPathByName is called.
 */

// Named endpoint so LinkGenerator can resolve it
app.MapGet("/api/items/{id:int}", (int id) =>
    Results.Ok(new { ItemId = id })
).WithName("GetItemById"); // name used below by LinkGenerator

// Handler receives LinkGenerator from DI and HttpContext as a framework parameter
app.MapGet("/api/items/link-demo", (LinkGenerator links, HttpContext httpCtx) =>
{
    // GetPathByName — relative path; returns null when name is not found or values are insufficient
    string? relativePath = links.GetPathByName("GetItemById", new { id = 99 });
    // GetUriByName — absolute URL including scheme and authority (requires HttpContext for host info)
    string? absoluteUri  = links.GetUriByName(httpCtx, "GetItemById", new { id = 99 });
    return Results.Ok(new { RelativePath = relativePath, AbsoluteUri = absoluteUri });
});

/*
 * SECTION 11: ROUTE GROUPS WITH MapGroup
 *
 * MapGroup(prefix) returns a RouteGroupBuilder — an IEndpointRouteBuilder scoped
 * to a URL prefix.  Every endpoint registered on the group inherits that prefix.
 *
 * Benefits:
 *   • Single prefix definition — rename /api/v1 to /api/v2 in one place.
 *   • Shared metadata — .WithTags() / .RequireAuthorization() / .WithOpenApi() on
 *     the group propagate to ALL member endpoints automatically.
 *   • Composability — groups can be nested (RouteGroupBuilder also implements IEndpointRouteBuilder).
 *
 * Two usage styles:
 *   Inline (below)    — group declared and populated in Program.cs; fine for small feature sets
 *   Extension method  — group defined in its own file (see Endpoints/OrderEndpoints.cs);
 *                       preferred for larger feature areas
 *
 * External groups registered further below:
 *   app.MapOrderEndpoints()  → see Endpoints/OrderEndpoints.cs (Section 3)
 *   app.MapUserEndpoints()   → see Endpoints/UserEndpoints.cs  (Section 4)
 */

// Inline group — all report endpoints share the /api/v1/reports prefix and "Reports" tag
RouteGroupBuilder reportsGroup = app.MapGroup("/api/v1/reports").WithTags("Reports");
reportsGroup.MapGet("/summary",  () => Results.Ok(new { Report = "summary"  })).WithName("ReportSummary");
reportsGroup.MapGet("/detail",   () => Results.Ok(new { Report = "detail"   })).WithName("ReportDetail");
reportsGroup.MapGet("/archived", () => Results.Ok(new { Report = "archived" })).WithName("ReportArchived");

// Register external groups (defined as IEndpointRouteBuilder extension methods)
app.MapOrderEndpoints(); // → Endpoints/OrderEndpoints.cs  — Section 3
app.MapUserEndpoints();  // → Endpoints/UserEndpoints.cs   — Section 4

/*
 * SECTION 12: ENDPOINT METADATA
 *
 * Metadata is additional information attached to an endpoint at registration time.
 * Middleware, filters, and OpenAPI tooling read this metadata at runtime via:
 *   Endpoint? ep = context.GetEndpoint();
 *   var meta = ep?.Metadata.GetMetadata<SomeAttribute>();
 *
 * Common fluent metadata methods (available on IEndpointConventionBuilder):
 *   .WithName(string)         — unique identifier; required for LinkGenerator
 *   .WithTags(params string[]) — Swagger/OpenAPI grouping tags
 *   .WithSummary(string)      — one-line description in Swagger UI
 *   .WithDescription(string)  — longer description in Swagger UI detail panel
 *   .WithDisplayName(string)  — developer tooling display name
 *   .RequireAuthorization(...)— adds authorization policy metadata (read by UseAuthorization)
 *   .AllowAnonymous()         — marks endpoint as no-auth-required (overrides RequireAuthorization)
 *   .Produces<T>(statusCode)  — OpenAPI response type declaration
 *   .ProducesProblem(code)    — OpenAPI error response declaration
 *
 * Custom metadata:
 *   .WithMetadata(new MyCustomAttribute())  — attach any object
 *   Read it from middleware or filters via Endpoint.Metadata.GetMetadata<T>().
 *
 * Methods return IEndpointConventionBuilder — chain as many as needed.
 */
app.MapGet("/api/metadata-demo", () => Results.Ok(new { Message = "metadata demo" }))
   .WithName("MetadataDemo")
   .WithTags("Demo")
   .WithSummary("Demonstrates endpoint metadata chaining")
   .WithDescription("Longer description visible in the Swagger UI details panel")
   .WithDisplayName("Metadata Demo Endpoint");

/*
 * SECTION 13: ENDPOINT FILTERS
 *
 * Endpoint filters wrap a handler in a pipeline of pre/post processing steps —
 * similar to MVC action filters but lighter-weight and available to all Map* routes.
 *
 * Execution order (outer-to-inner before, inner-to-outer after):
 *   Filter 1 → Filter 2 → Handler → Filter 2 → Filter 1
 *
 * AddEndpointFilter delegate signature:
 *   Func<EndpointFilterInvocationContext, EndpointFilterDelegate, ValueTask<object?>>
 *
 *   EndpointFilterInvocationContext:
 *     .HttpContext    — the current request context
 *     .Arguments      — handler parameter list (can be inspected or replaced)
 *   EndpointFilterDelegate:
 *     next(context)  — calls the next filter or the handler itself
 *
 * Short-circuit pattern:
 *   Return an IResult from the filter WITHOUT calling next(context).
 *   The handler and all inner filters are skipped.
 *
 * IEndpointFilter interface (alternative to the inline delegate):
 *   class LoggingFilter : IEndpointFilter
 *   {
 *       public async ValueTask<object?> InvokeAsync(
 *           EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
 *       {
 *           Console.WriteLine($"Before: {ctx.HttpContext.Request.Path}");
 *           var result = await next(ctx);
 *           Console.WriteLine("After");
 *           return result;
 *       }
 *   }
 *   app.MapGet("/route", handler).AddEndpointFilter<LoggingFilter>();
 *
 * Endpoint filters vs middleware:
 *   Middleware    — runs for ALL requests regardless of whether a route matched.
 *   Endpoint filter — runs ONLY for the specific endpoint it is attached to.
 *   Use filters for per-endpoint concerns; use middleware for cross-cutting concerns.
 *
 * COVERED IN MORE DEPTH → 09. Filters (MVC action/resource/result/exception filters)
 */

// Filter 1: timing — logs how long the handler took to execute
app.MapGet("/api/filtered-demo", () => Results.Ok(new { Message = "Handler executed" }))
   .WithName("FilteredDemo")
   .AddEndpointFilter(async (context, next) =>
   {
       string name   = context.HttpContext.GetEndpoint()?.DisplayName ?? "unknown";
       var    sw     = System.Diagnostics.Stopwatch.StartNew();         // fully-qualified; no extra using

       object? result = await next(context); // invoke the next filter or the actual handler

       sw.Stop();
       Console.WriteLine($"[TimingFilter] {name} completed in {sw.ElapsedMilliseconds} ms");
       return result;
   });

// Filter 2: validation guard — short-circuits with 400 if required query param is absent
app.MapGet("/api/guarded", (string? key) => Results.Ok(new { Key = key }))
   .AddEndpointFilter(async (context, next) =>
   {
       string? key = context.HttpContext.Request.Query["key"]; // read query string value
       if (string.IsNullOrWhiteSpace(key))
           return Results.Problem("Query parameter 'key' is required.", statusCode: 400); // short-circuit
       return await next(context); // all good — proceed to the handler
   });

/*
 * SECTION 14: CONVENTIONAL ROUTING IN MVC (MapControllerRoute)
 *
 * Conventional routing defines a central URL pattern shared by many controllers.
 * The framework parses the URL to determine the controller and action to invoke.
 *
 * MapControllerRoute(name, pattern):
 *   pattern:  "{controller=Home}/{action=Index}/{id?}"
 *     {controller}  → controller class name without "Controller" suffix
 *     {action}      → method name
 *     {id?}         → optional extra parameter
 *     =Default      → value used when the segment is absent from the URL
 *
 * Route matching examples:
 *   /                    → HomeController.Index(id = null)
 *   /Products            → ProductsController.Index(id = null)
 *   /Products/Details/5  → ProductsController.Details(id = "5")
 *
 * Attribute routing vs conventional routing:
 *   Attribute   — [Route] / [HttpGet] on each action; preferred for REST APIs
 *   Conventional — one central MapControllerRoute pattern; preferred for page-based MVC
 *   Both can coexist; attribute routes take precedence when both could match a URL.
 *
 * MapControllers()             — activates attribute routing for all [ApiController] classes.
 * MapControllerRoute(name, p)  — adds one conventional route; call multiple times for multiple patterns.
 * MapDefaultControllerRoute()  — shorthand for "{controller=Home}/{action=Index}/{id?}".
 *
 * For this project only MapControllers() is active because ProductsController uses [Route].
 * MapControllerRoute is shown for reference; it won't match any requests since no
 * conventional controller exists in this project.
 */

// Enable attribute routing for all [ApiController] classes (ProductsController)
app.MapControllers();

// Conventional route — reference only; not reached because ProductsController uses
// [Route] (attribute routing), which takes precedence over conventional routes.
app.MapControllerRoute(
    name:    "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Register health endpoints via IEndpointRouteBuilder extension (Section 5)
app.MapHealthEndpoints(); // → Extensions/RouteBuilderExtensions.cs

/*
 * SECTION 15: PREVIEW — MODEL BINDING & VALIDATION
 *
 * COVERED IN DETAIL LATER → 08. Model Binding & Validation
 *
 * Model binding automatically populates handler parameters from parts of the HTTP request:
 *
 *   Source         Attribute       Example
 *   Route values   [FromRoute]     /products/{id}   → int id
 *   Query string   [FromQuery]     ?page=2&size=10  → int page, int size
 *   Request body   [FromBody]      JSON payload     → Product product
 *   Form data      [FromForm]      multipart form   → IFormFile file
 *   HTTP headers   [FromHeader]    X-Api-Key        → string apiKey
 *   DI services    [FromServices]  injected         → IMyService svc
 *
 * With [ApiController] the binding source is inferred from parameter shape and position:
 *   Complex type not in the route template → [FromBody]
 *   Simple type whose name matches a route token → [FromRoute]
 *   Otherwise → [FromQuery]
 *
 * Validation runs AFTER binding via DataAnnotations ([Required], [Range], [StringLength])
 * or a FluentValidation library.  [ApiController] returns HTTP 400 ProblemDetails
 * automatically when model state is invalid — no manual check needed.
 */

/*
 * SECTION 16: PREVIEW — MINIMAL APIs
 *
 * COVERED IN DETAIL LATER → 13. Minimal APIs
 *
 * This chapter already uses minimal-API syntax (MapGet etc. with lambda handlers).
 * The Minimal APIs chapter covers the topics that go beyond routing:
 *
 *   • TypedResults — TypedResults.Ok<T>(), TypedResults.NotFound() (compile-time type safety)
 *   • OpenAPI integration — .WithOpenApi(), Swashbuckle vs Microsoft.AspNetCore.OpenApi
 *   • Filters at scale — IEndpointFilter implementations and filter factory pattern
 *   • Handler extraction — static or instance methods instead of inline lambdas
 *   • ProblemDetails — Results.ValidationProblem, Results.Problem with custom extensions
 *   • Streaming — Results.Stream, Results.Bytes, Results.File for large responses
 *   • File uploads — IFormFile in minimal API handlers
 *   • Endpoint groups — full real-world feature-module organisation
 */

app.Run(); // start Kestrel web server and block until the application shuts down

/*
 * QUICK REFERENCE — Routing & Endpoints
 * ═══════════════════════════════════════════════════════════════════════════════
 *
 * MAP METHODS (IEndpointRouteBuilder extension methods)
 *   app.MapGet(pattern, handler)          GET request → handler
 *   app.MapPost(pattern, handler)         POST
 *   app.MapPut(pattern, handler)          PUT
 *   app.MapDelete(pattern, handler)       DELETE
 *   app.MapMethods(pat, string[], handler) custom verb list
 *   app.MapControllers()                  enable attribute routing on [ApiController] classes
 *   app.MapControllerRoute(name, pattern) add conventional MVC route
 *   app.MapDefaultControllerRoute()       shorthand for {controller=Home}/{action=Index}/{id?}
 *
 * ROUTE TEMPLATES
 *   {id}              required parameter (any non-slash, non-empty value)
 *   {id?}             optional parameter (type must be nullable)
 *   {id=default}      parameter with default value
 *   {id:int}          constrained parameter (must satisfy :int)
 *   {**slug}          catch-all (captures rest of URL including slashes; last segment only)
 *   report-{year}.pdf complex segment (literal + parameter mixed in one segment)
 *
 * ROUTE CONSTRAINTS  {param:constraint}
 *   int, long, float, double, decimal, bool, guid, datetime
 *   alpha           ASCII alphabetic characters only
 *   length(n,m)     string length in [n, m]
 *   min(n), max(n)  integer >= n or <= n
 *   range(n,m)      integer in [n, m]
 *   regex(pat)      regex; escape template braces as {{ }}
 *
 * LINK GENERATION
 *   // Inject: LinkGenerator links
 *   links.GetPathByName("EndpointName", new { id = 1 })    → "/api/items/1"
 *   links.GetUriByName(ctx, "EndpointName", new { id = 1}) → "https://host/api/items/1"
 *
 * ROUTE GROUPS
 *   RouteGroupBuilder g = app.MapGroup("/api/prefix");
 *   g.MapGet("/sub", handler);    → GET /api/prefix/sub
 *   g.WithTags("Tag")             → metadata shared by all group members
 *
 * ENDPOINT METADATA  (chained after any Map* call)
 *   .WithName("EndpointName")     → unique name for LinkGenerator
 *   .WithTags("Tag1", "Tag2")     → Swagger/OpenAPI grouping
 *   .WithSummary("one-liner")     → OpenAPI summary text
 *   .WithDescription("detail")   → OpenAPI long description
 *   .RequireAuthorization()       → attach authorization policy metadata
 *   .AllowAnonymous()             → exempt endpoint from auth requirement
 *
 * ENDPOINT FILTERS  (short-circuit or wrap handler)
 *   .AddEndpointFilter(async (ctx, next) => {
 *       // before handler
 *       var result = await next(ctx);
 *       // after handler
 *       return result;
 *   });
 *   .AddEndpointFilter<MyFilter>()   // IEndpointFilter implementation
 *
 * ATTRIBUTE ROUTING (on controller)
 *   [Route("api/[controller]")]       prefix; [controller] token substitution
 *   [HttpGet("{id:int}")]             GET + route template + constraint
 *   [HttpPost], [HttpPut], [HttpDelete]
 *   HttpContext.GetRouteData()        → RouteData with .Values dictionary
 *   ControllerBase.RouteData          → same, via inherited property
 *
 * PIPELINE ORDER (incorrect order bypasses auth/CORS)
 *   app.UseRouting()          ← match; endpoint metadata available after this point
 *   app.UseCors()             ← needs matched endpoint to apply correct policy
 *   app.UseAuthentication()
 *   app.UseAuthorization()    ← after Authentication
 *   app.MapGet(...)           ← execute (Map* auto-inserts UseEndpoints terminal)
 * ═══════════════════════════════════════════════════════════════════════════════
 */
