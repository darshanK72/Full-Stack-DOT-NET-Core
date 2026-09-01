/*
 * TOPIC: Action Filters in ASP.NET Core MVC
 * ─────────────────────────────────────────────────────────────────────────────
 * WHY IT MATTERS:
 *   Real-world MVC controllers grow cluttered fast when every action manually
 *   checks authentication headers, validates ModelState, logs execution times,
 *   and catches unexpected exceptions.  Action Filters let you extract those
 *   cross-cutting concerns into reusable, composable classes that plug into the
 *   MVC pipeline automatically — keeping controllers thin and focused.
 *
 *   The MVC filter pipeline runs INSIDE the ASP.NET Core middleware pipeline,
 *   giving you lifecycle hooks that are tightly coupled to controller/action
 *   execution — something middleware alone cannot provide.
 *
 * WHAT YOU WILL LEARN:
 *    1. The MVC filter pipeline: Authorization → Resource → Action → Exception → Result
 *    2. Filter scope and execution order (global → controller → action)
 *    3. IAsyncActionFilter / IActionFilter (OnActionExecuting / OnActionExecuted,
 *       ActionExecutingContext, ActionExecutedContext, short-circuit via Result)
 *    4. ActionFilterAttribute (base class that combines IActionFilter + IResultFilter)
 *    5. IResultFilter / IAsyncResultFilter (OnResultExecuting / OnResultExecuted,
 *       intercepting ViewResult before and after rendering)
 *    6. IAsyncExceptionFilter (ExceptionContext, ExceptionHandled, ViewResult for
 *       HTML errors vs ObjectResult/ProblemDetails for API clients — MVC-specific)
 *    7. IAuthorizationFilter (AuthorizationFilterContext, short-circuit with 401/403)
 *    8. IResourceFilter (ResourceExecutingContext, cache short-circuit, ETag pattern)
 *    9. ServiceFilterAttribute and TypeFilterAttribute (DI-friendly filter application)
 *   10. IFilterFactory (per-request filter construction with DI from an attribute)
 *   11. [SkipStatusCodePages] (prevent StatusCodePages middleware from intercepting
 *       error responses already handled by a filter)
 *   12. Global filter registration via AddControllersWithViews options
 *
 * CHAPTER MAP (multi-file project — open files in this order):
 *   SECTION  1 → Program.cs                               Filter Pipeline Overview
 *   SECTION  2 → Program.cs                               Global Registration + Scope & Order
 *   SECTION  3 → Filters/AuditActionFilter.cs             IAsyncActionFilter
 *   SECTION  4 → Filters/ValidateModelActionFilter.cs     IActionFilter + Short-Circuit
 *   SECTION  5 → Filters/PerformanceActionFilter.cs       ActionFilterAttribute + IResultFilter
 *   SECTION  6 → Filters/MvcExceptionFilter.cs            IAsyncExceptionFilter + ViewResult
 *   SECTION  7 → Filters/ApiKeyAuthorizationFilter.cs     IAuthorizationFilter + IFilterFactory
 *   SECTION  8 → Filters/ETagResourceFilter.cs            IResourceFilter (cache short-circuit)
 *   SECTION  9 → Controllers/ProductsController.cs        Applying Filters at All Scopes
 *   SECTION 10 → Models/ErrorViewModel.cs                 Error View Model
 *   SECTION 11 → Models/ProductViewModel.cs               Product View Model
 */

/*
 * SECTION 1: THE MVC FILTER PIPELINE — OVERVIEW
 * ─────────────────────────────────────────────────────────────────────────────
 * ASP.NET Core MVC adds its own filter pipeline INSIDE the middleware pipeline.
 * When a request reaches the MVC router and a controller action is selected, it
 * runs through five filter stages in a strict order:
 *
 *   ┌─────────────────────────────────────────────────────────────────────────┐
 *   │ 1. IAuthorizationFilter   — runs first; sets ctx.Result to short-circuit│
 *   │                             with 401/403 before anything else executes  │
 *   ├─────────────────────────────────────────────────────────────────────────┤
 *   │ 2. IResourceFilter        — wraps the ENTIRE remaining pipeline;        │
 *   │    OnResourceExecuting    — useful to serve cached responses early,     │
 *   │    OnResourceExecuted     — runs AFTER result execution too             │
 *   ├─────────────────────────────────────────────────────────────────────────┤
 *   │ ← Model Binding happens here (not a filter, but part of the pipeline) → │
 *   ├─────────────────────────────────────────────────────────────────────────┤
 *   │ 3. IActionFilter          — most commonly used stage                    │
 *   │    OnActionExecuting      — before the controller action method runs    │
 *   │    [ Controller action executes and returns IActionResult ]             │
 *   │    OnActionExecuted       — after the action method; can inspect Result │
 *   ├─────────────────────────────────────────────────────────────────────────┤
 *   │ 4. IExceptionFilter       — catches exceptions from action + result     │
 *   │                             execution; set Result to suppress exception │
 *   ├─────────────────────────────────────────────────────────────────────────┤
 *   │ 5. IResultFilter          — wraps only IActionResult.ExecuteResultAsync │
 *   │    OnResultExecuting      — before ViewResult/JsonResult renders        │
 *   │    [ IActionResult.ExecuteResultAsync — response body written ]         │
 *   │    OnResultExecuted       — after response body is written              │
 *   └─────────────────────────────────────────────────────────────────────────┘
 *
 * KEY DISTINCTIONS BETWEEN FILTER TYPES:
 *
 *   Authorization   → No access to IActionResult; only set Result to block
 *   Resource        → Wraps EVERYTHING including model binding; best for caching
 *   Action          → Access to both ActionArguments (before) and Result (after)
 *   Exception       → Only triggered by unhandled exceptions; NOT for 404/401
 *   Result          → Wraps result execution only; runs even after Action short-circuit
 *
 * SYNC VS ASYNC:
 *   Each stage has both IXxxFilter (sync) and IAsyncXxxFilter (async) versions.
 *   Implement one or the other — NOT both.  The framework prefers async when both
 *   are implemented on the same class.  For async, the "before" and "after" code
 *   lives in a single OnXxxAsync method separated by `await next()`.
 *
 * FILTER INTERFACES SUMMARY:
 *   Interface                 Sync                           Async
 *   ─────────────────────     ─────────────────────────────  ─────────────────────────────────
 *   IAuthorizationFilter      OnAuthorization(ctx)           OnAuthorizationAsync(ctx)
 *   IResourceFilter           OnResourceExecuting/Executed   OnResourceExecutionAsync(ctx, next)
 *   IActionFilter             OnActionExecuting/Executed     OnActionExecutionAsync(ctx, next)
 *   IResultFilter             OnResultExecuting/Executed     OnResultExecutionAsync(ctx, next)
 *   IExceptionFilter          OnException(ctx)               OnExceptionAsync(ctx)
 *   ActionFilterAttribute     override any of the above      (same type, abstract base class)
 */

using ActionFiltersMvc.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

/*
 * SECTION 2: GLOBAL FILTER REGISTRATION, SCOPE, AND ORDER
 * ─────────────────────────────────────────────────────────────────────────────
 * Filters can be applied at three scopes — from outermost to innermost:
 *
 *   SCOPE           HOW TO APPLY                         RUNS ON
 *   ──────────────  ───────────────────────────────────  ────────────────────────
 *   Global          options.Filters.Add<T>()             Every controller action
 *   Controller      [ServiceFilter] / [TypeFilter] attr  All actions on the class
 *   Action          [ServiceFilter] / [TypeFilter] attr  That specific action only
 *
 * EXECUTION ORDER for the same filter TYPE across scopes:
 *   · "Before" callbacks  → Global → Controller → Action  (outer → inner)
 *   · "After" callbacks   → Action → Controller → Global  (inner → outer)
 *   This mirrors how middleware wrapping works.
 *
 * EXECUTION ORDER within the same scope — use the Order property (default = 0):
 *   Lower Order value = runs earlier in the "before" phase (and later in "after").
 *   options.Filters.Add<MyFilter>(order: -1000) // runs before Order=0 filters
 *   [TypeFilter(typeof(MyFilter), Order = 10)]  // runs after Order=0 within scope
 *
 * REGISTRATION OPTIONS for global filters:
 *   a) options.Filters.Add<TFilter>()               — generic; TFilter resolved from DI
 *   b) options.Filters.Add(new MyFilter())           — instance; no DI injection possible
 *   c) options.Filters.Add(typeof(MyFilter))         — type; resolved from DI
 *   d) options.Filters.Add<TFilter>(order: -1000)    — with explicit Order value
 *
 * ServiceFilterAttribute vs TypeFilterAttribute (controller/action level):
 *   [ServiceFilter(typeof(T))]
 *     → T MUST be registered in DI; lifetime is controlled by that DI registration
 *     → Use when the filter needs a specific DI lifetime (Scoped, Singleton, etc.)
 *
 *   [TypeFilter(typeof(T))]
 *     → T does NOT need to be registered in DI; framework creates it each request
 *     → Can inject constructor args via the Arguments property
 *     → [TypeFilter(typeof(T), Arguments = new object[] { "value" })]
 *
 * IMPORTANT — Attribute filter instance sharing:
 *   Attribute instances ([PerformanceActionFilter]) are shared across requests when
 *   used on a controller/action — their instance fields are NOT per-request safe.
 *   Use HttpContext.Items["key"] to store per-request state inside filter attributes.
 *   ServiceFilter and TypeFilter always create a new filter instance per request.
 */
builder.Services.AddControllersWithViews(options =>
{
    // Global exception filter — catches unhandled exceptions from every action in the app
    // Runs innermost-last; other global filters run around it
    options.Filters.Add<MvcExceptionFilter>();

    // Global audit filter — logs every action entry and exit for the whole app
    // Adding here means controllers do NOT need [ServiceFilter(typeof(AuditActionFilter))]
    options.Filters.Add<AuditActionFilter>();
});

// Register filters that are resolved by the DI system.
// Required for: options.Filters.Add<T>() and [ServiceFilter(typeof(T))]
// NOT required for: [TypeFilter(typeof(T))] — it bypasses DI registration
builder.Services.AddScoped<AuditActionFilter>();       // one per HTTP request (has ILogger dep)
builder.Services.AddScoped<MvcExceptionFilter>();      // one per HTTP request (has IModelMetadataProvider dep)
builder.Services.AddScoped<ETagResourceFilter>();      // one per HTTP request
builder.Services.AddScoped<ApiKeyAuthorizationFilter>(); // one per HTTP request

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // add HSTS header in production
}

app.UseHttpsRedirection();  // redirect HTTP → HTTPS
app.UseStaticFiles();       // serve wwwroot/ (CSS, JS, images)
app.UseRouting();           // match URL to MVC route
app.UseAuthorization();     // run [Authorize] policies

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}"); // default to Products/Index

app.Run(); // start Kestrel; blocks until shutdown

/*
 * QUICK REFERENCE — Action Filters in ASP.NET Core MVC
 * ─────────────────────────────────────────────────────────────────────────────
 *
 * FILTER PIPELINE ORDER:
 *   Authorization → Resource → (Model Binding) → Action → Exception → Result
 *
 * GLOBAL REGISTRATION (AddControllersWithViews):
 *   options.Filters.Add<TFilter>()
 *   options.Filters.Add<TFilter>(order: -1000)      // explicit order; lower = earlier
 *
 * CONTROLLER / ACTION LEVEL (attributes):
 *   [ServiceFilter(typeof(T))]                       // T must be in DI; scoped by DI
 *   [TypeFilter(typeof(T))]                          // no DI reg needed; new per request
 *   [TypeFilter(typeof(T), Arguments = new[] { x })] // pass ctor args to TypeFilter
 *   [MyFilterAttribute]                              // if filter inherits ActionFilterAttribute
 *
 * FILTER SCOPE EXECUTION (for same type, e.g. IActionFilter):
 *   Executing: Global → Controller → Action   (outer → inner)
 *   Executed:  Action → Controller → Global   (inner → outer)
 *
 * IAsyncActionFilter PATTERN:
 *   public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
 *   {
 *       // before logic — ctx.ActionArguments, ctx.HttpContext, ctx.ActionDescriptor
 *       ctx.Result = new RedirectResult("/"); // short-circuit: skip action + subsequent filters
 *       ActionExecutedContext executed = await next();
 *       // after logic — executed.Result, executed.Exception, executed.ExceptionHandled
 *   }
 *
 * IActionFilter SHORT-CIRCUIT (sync):
 *   public void OnActionExecuting(ActionExecutingContext ctx)
 *   {
 *       if (invalid) ctx.Result = new BadRequestResult(); // stops pipeline
 *   }
 *
 * IExceptionFilter PATTERN:
 *   public void OnException(ExceptionContext ctx)
 *   {
 *       ctx.Result = new ViewResult { ViewName = "Error" }; // HTML error page
 *       ctx.ExceptionHandled = true;                        // suppress exception
 *   }
 *
 * [SkipStatusCodePages] — suppress StatusCodePages middleware for this action:
 *   [SkipStatusCodePages]  // attribute on controller or action
 *   // or in a filter: context.HttpContext.SetSkipStatusCodePages()
 *
 * IFilterFactory — DI-friendly attribute:
 *   public class MyAttribute : Attribute, IFilterFactory
 *   {
 *       public bool IsReusable => false;
 *       public IFilterMetadata CreateInstance(IServiceProvider sp)
 *           => sp.GetRequiredService<MyActualFilter>();
 *   }
 *
 * SCOPE CHEAT SHEET:
 *   Apply globally     → options.Filters.Add<T>()     in AddControllersWithViews
 *   Apply per class    → [ServiceFilter(typeof(T))]   on the controller class
 *   Apply per action   → [TypeFilter(typeof(T))]      on the action method
 *   Apply as attribute → [MyFilterAttribute]           when T : ActionFilterAttribute
 *
 * NEXT CHAPTERS:
 *   12. TempData, ViewData & ViewBag   — passing data between redirects and views
 *   13. AJAX & Partial Page Updates    — partial view rendering and JSON endpoints
 * ─────────────────────────────────────────────────────────────────────────────
 */
