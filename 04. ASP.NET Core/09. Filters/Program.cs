/*
 * TOPIC: ASP.NET Core Filters
 *
 * WHY IT MATTERS:
 *   Filters let you run code at specific, named points in the request-processing
 *   pipeline — before/after action execution, before/after result serialization,
 *   on unhandled exceptions, or even before model binding.  They are the right
 *   pattern for cross-cutting concerns: request logging, API-key validation,
 *   centralized model-state checks, and structured error responses.
 *
 *   Without filters every action must repeat guard clauses, try/catch blocks,
 *   and logging calls.  With filters that boilerplate lives in one class and
 *   applies globally, per-controller, or per-action.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Filter pipeline order — the six stages and how they nest
 *   2.  Filter types overview — Authorization, Resource, Action, Exception, Result
 *   3.  IAsyncActionFilter + short-circuiting      → Filters/RequestLoggingActionFilter.cs
 *   4.  IResultFilter (sync + async)               → Filters/RequestLoggingActionFilter.cs
 *   5.  IActionFilter — ModelState short-circuit   → Filters/ValidateModelFilter.cs
 *   6.  IAsyncExceptionFilter — structured errors  → Filters/GlobalExceptionFilter.cs
 *   7.  IAuthorizationFilter + IResourceFilter     → Filters/ApiKeyAuthorizationFilter.cs
 *   8.  Filter attributes on controller/action     → Controllers/OrdersController.cs
 *   9.  Global registration, DI, scope, ordering   → Program.cs (Sections 3-6)
 *   10. PREVIEW: exception middleware vs filters   → Program.cs (Section 7)
 *
 * CHAPTER MAP:
 *   1.  Filter pipeline order + filter types    → Program.cs (Sections 1-2)
 *   2.  IAsyncActionFilter + IResultFilter      → Filters/RequestLoggingActionFilter.cs
 *   3.  ModelState validation filter            → Filters/ValidateModelFilter.cs
 *   4.  Global exception filter                 → Filters/GlobalExceptionFilter.cs
 *   5.  Authorization + Resource filters        → Filters/ApiKeyAuthorizationFilter.cs
 *   6.  Controller / action attribute usage     → Controllers/OrdersController.cs
 *   7.  Global registration + DI + ordering     → Program.cs (Sections 3-6)
 *   8.  PREVIEW: exception middleware           → Program.cs (Section 7)
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Filters.Filters;

/*
 * SECTION 1: FILTER PIPELINE ORDER
 *
 *   A request first travels through the outer Middleware pipeline (routing,
 *   authentication, CORS, etc.), then enters the MVC/API filter pipeline.
 *   The filter pipeline has six stages that nest like brackets:
 *
 *   ┌── 1. Authorization ──────────────────────────────────────────────────┐
 *   │  ┌── 2. Resource ────────────────────────────────────────────────── ┐│
 *   │  │  ┌── [ model binding happens here ] ────────────────────────── ┐││
 *   │  │  │  ┌── 3. Action ─────────────────────────────────────────── ┐│││
 *   │  │  │  │           [ action method executes ]                    ││││
 *   │  │  │  └── Action After (OnActionExecuted) ─────────────────────┘│││
 *   │  │  │  ┌── 4. Exception (wraps Result stage) ──────────────────┐ │││
 *   │  │  │  │  ┌── 5. Result ────────────────────────────────────── ┐│ │││
 *   │  │  │  │  │       [ IActionResult.ExecuteResultAsync ]         ││ │││
 *   │  │  │  │  └── Result After (OnResultExecuted) ────────────────┘│ │││
 *   │  │  │  └───────────────────────────────────────────────────────┘ │││
 *   │  │  └──────────────────────────────────────────────────────────────┘││
 *   │  └─────────────────────────────────────────────────────────────────┘│
 *   └──────────────────────────────────────────────────────────────────────┘
 *
 *   Stage          Interface(s)                           Typical use
 *   ─────────────────────────────────────────────────────────────────────
 *   Authorization  IAuthorizationFilter                   API key / JWT check
 *                  IAsyncAuthorizationFilter
 *   Resource       IResourceFilter                        Pre-binding cache read
 *                  IAsyncResourceFilter
 *   Action         IActionFilter                          Logging, validation
 *                  IAsyncActionFilter
 *   Exception      IExceptionFilter                       Structured error response
 *                  IAsyncExceptionFilter
 *   Result         IResultFilter                          Response headers, timing
 *                  IAsyncResultFilter
 *   Page           IPageFilter                            Razor Pages only
 */

/*
 * SECTION 2: FILTER TYPES — QUICK TAXONOMY
 *
 *   AUTHORIZATION FILTER  — First to run.  Setting context.Result short-circuits
 *                           the entire inner pipeline.  Do not re-implement
 *                           authentication here; use built-in middleware schemes
 *                           (JWT, Cookie) for that.  Filters handle custom,
 *                           non-scheme checks such as API keys or HMAC signatures.
 *
 *   RESOURCE FILTER       — Runs after auth, before model binding.
 *                           OnResourceExecuting: chance to return a cached response.
 *                           OnResourceExecuted:  chance to cache the action result.
 *
 *   ACTION FILTER         — Surrounds the action method invocation.
 *                           OnActionExecuting: validate, log, short-circuit.
 *                           OnActionExecuted: inspect or replace the result.
 *                           Short-circuit: set context.Result before calling next().
 *
 *   EXCEPTION FILTER      — Catches unhandled exceptions from action methods and
 *                           action/result filters.  Set context.ExceptionHandled
 *                           to suppress rethrow; set context.Result for a response.
 *                           Does NOT catch middleware exceptions (see Section 7).
 *
 *   RESULT FILTER         — Surrounds IActionResult.ExecuteResultAsync.
 *                           OnResultExecuting: modify response headers.
 *                           OnResultExecuted: record response timing.
 *
 *   PAGE FILTER           — Razor Pages only; similar to Action filter but for page
 *                           model handlers.  Not covered here.
 *
 *   Async variants (IAsyncXxxFilter) use a single method with a next() delegate.
 *   Prefer async variants when you need to await I/O inside the filter.
 */

namespace Filters;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // ── Filters that are injected via [ServiceFilter] must be in DI ──
        builder.Services.AddScoped<ApiKeyAuthorizationFilter>();  // resolved by ServiceFilter
        builder.Services.AddScoped<GlobalExceptionFilter>();      // resolved by ServiceFilter + global

        /*
         * SECTION 3: GLOBAL FILTER REGISTRATION
         *
         *   Filters registered in AddControllers.Options.Filters run for EVERY
         *   controller action in the application — this is the global scope.
         *
         *   Registration methods on FilterCollection:
         *
         *     options.Filters.Add<TFilter>()
         *       Uses TypeFilter semantics: creates TFilter via ActivatorUtilities,
         *       which resolves constructor args from DI.  TFilter itself does NOT
         *       need to be registered as a DI service.
         *
         *     options.Filters.Add(instance)
         *       Passes a pre-built instance.  Instance filters are singletons for
         *       the lifetime of the application; do not use if the filter has
         *       per-request state.
         *
         *   FILTER SCOPE (outermost → innermost, Before phase):
         *     Global → Controller → Action
         *
         *   The "After" callbacks (OnActionExecuted, OnResultExecuted) run in
         *   reverse order:
         *     Action → Controller → Global
         *
         *   ORDER OF STAGES (always, regardless of scope):
         *     Authorization → Resource → Action → Exception → Result
         */
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();       // global — runs for every action
            options.Filters.Add<RequestLoggingActionFilter>();  // DI resolves ILogger<T> for ctor
        });

        /*
         * SECTION 4: ServiceFilter vs TypeFilter vs IFilterFactory
         *
         *   [ServiceFilter(typeof(T))]
         *     • Resolves T from the DI container (T must be registered).
         *     • Respects the DI lifetime (Singleton, Scoped, Transient).
         *     • Constructor parameters come from DI only.
         *     • Use when the filter has stable dependencies (ILogger, DbContext, etc.)
         *       and you want DI to own the lifetime.
         *
         *   [TypeFilter(typeof(T))]
         *     • Creates T via ActivatorUtilities (constructor args resolved from DI).
         *     • T does NOT need to be registered as a DI service.
         *     • Supports passing additional non-DI args via Arguments property:
         *         [TypeFilter(typeof(ValidateModelFilter), Arguments = new[] { "v1" })]
         *     • Use when the filter is one-off and a DI registration would clutter
         *       the service container.
         *
         *   IFilterFactory (on an Attribute subclass)
         *     • The attribute class itself implements CreateInstance(IServiceProvider).
         *     • Lets you read attribute constructor parameters and forward them to the
         *       filter — [RequireRole("Admin")] can pass "Admin" to RoleCheckFilter.
         *     • IsReusable = false → new filter per request (safe for stateful filters).
         *     • Defined and demonstrated in Controllers/OrdersController.cs.
         *
         *   Decision guide:
         *     Has DI-registered dependencies + want lifetime control  → ServiceFilter
         *     No DI registration, possibly extra ctor params           → TypeFilter
         *     Attribute that must forward params to filter ctor         → IFilterFactory
         */

        builder.Services.AddEndpointsApiExplorer();

        WebApplication app = builder.Build();

        /*
         * SECTION 5: FILTER ORDERING — THE Order PROPERTY
         *
         *   Every filter can implement IOrderedFilter (int Order { get; }).
         *   When multiple filters share a scope, Order determines sequence
         *   within that scope's Before phase.
         *
         *   Lower Order value  → runs earlier in the Before phase.
         *   Default Order       = 0 (when not explicitly set).
         *   int.MinValue        → run before all framework-provided filters.
         *   int.MaxValue        → run after  all framework-provided filters.
         *
         *   Example registered in Section 3:
         *     RequestLoggingActionFilter.Order = -1000
         *     → wraps all other global action filters; its "Before" runs first,
         *       its "After" runs last.
         *
         *   Global filters also implement IOrderedFilter via their interface
         *   inheritance.  Scope (Global > Controller > Action) takes precedence
         *   over Order ONLY when Order values are equal (Order 0 Global beats
         *   Order 0 Action).  Explicit Order can override scope ordering:
         *     A controller-level filter with Order = -9999 runs before a global
         *     filter with Order = 0.
         *
         *   SHORT-CIRCUITING RECAP:
         *     Set context.Result in a "Before" callback to skip the action/result.
         *     Already-entered filters still receive their "After" callback.
         *     No subsequent "Before" callbacks at equal or inner scope run.
         */

        app.UseRouting();
        app.MapControllers();

        /*
         * SECTION 6: FILTER SCOPE — EXECUTION TRACE EXAMPLE
         *
         *   Given: global RequestLoggingActionFilter (Order -1000),
         *          controller-level ValidateModelFilter (Order 0),
         *          action-level  ResponseTimingResultFilter
         *
         *   Full execution trace for GET /api/orders/1:
         *
         *   [Stage: Authorization]
         *     GlobalScope  ApiKeyAuthorizationFilter.OnAuthorizationAsync
         *     ControllerScope  ApiKeyAuthorizationFilter (controller attr, same filter)
         *
         *   [Stage: Action — Before phase]
         *     GlobalScope  RequestLoggingActionFilter.OnActionExecutionAsync (before next)
         *     ControllerScope  ValidateModelFilter.OnActionExecuting
         *     → action method: OrdersController.GetById executes
         *
         *   [Stage: Action — After phase]
         *     ControllerScope  ValidateModelFilter.OnActionExecuted
         *     GlobalScope  RequestLoggingActionFilter.OnActionExecutionAsync (after next)
         *
         *   [Stage: Result — Before phase]
         *     ActionScope  ResponseTimingResultFilter.OnResultExecutionAsync (before next)
         *     → OkObjectResult.ExecuteResultAsync serializes JSON
         *
         *   [Stage: Result — After phase]
         *     ActionScope  ResponseTimingResultFilter.OnResultExecutionAsync (after next)
         */

        /*
         * SECTION 7: PREVIEW — Exception Middleware vs Exception Filters
         *
         *   IExceptionFilter / IAsyncExceptionFilter catches exceptions thrown by:
         *     ✓ Action methods
         *     ✓ Action filters (IActionFilter, IAsyncActionFilter)
         *     ✓ Result filters (IResultFilter, IAsyncResultFilter)
         *
         *   It does NOT catch exceptions from:
         *     ✗ Middleware that runs before MVC routing
         *     ✗ Authorization or Resource filters (those wrap the exception filter stage)
         *     ✗ Minimal API endpoints
         *
         *   Exception-handling middleware (app.UseExceptionHandler / custom middleware)
         *   catches ALL unhandled exceptions from anywhere in the pipeline, including
         *   middleware, MVC, and minimal APIs.
         *
         *   Rule of thumb:
         *     Use IExceptionFilter when you need MVC-specific behavior: ProblemDetails
         *     responses, inspecting action metadata, per-controller error shapes.
         *     Use middleware when you need a single catch-all for the whole application.
         *
         *   COVERED IN DETAIL → 10. Exception Handling
         */

        app.Run();
    }
}

/*
 * QUICK REFERENCE — ASP.NET Core Filters
 * ──────────────────────────────────────────────────────────────────────────
 *
 *  Filter type     Interface(s)                    Sync / Async
 *  ─────────────── ─────────────────────────────── ─────────────────────────
 *  Authorization   IAuthorizationFilter             OnAuthorization(ctx)
 *                  IAsyncAuthorizationFilter        OnAuthorizationAsync(ctx)
 *  Resource        IResourceFilter                  OnResourceExecuting/ed
 *                  IAsyncResourceFilter             OnResourceExecutionAsync
 *  Action          IActionFilter                    OnActionExecuting/ed
 *                  IAsyncActionFilter               OnActionExecutionAsync
 *  Exception       IExceptionFilter                 OnException(ctx)
 *                  IAsyncExceptionFilter            OnExceptionAsync(ctx)
 *  Result          IResultFilter                    OnResultExecuting/ed
 *                  IAsyncResultFilter               OnResultExecutionAsync
 *  Page            IPageFilter                      Razor Pages only
 *
 *  Scope               Apply via
 *  ─────────────────── ──────────────────────────────────────────────────────
 *  Global              options.Filters.Add<T>() or .Add(instance)
 *  Controller          [ServiceFilter(typeof(T))] or [TypeFilter(typeof(T))]
 *  Action              same as Controller
 *
 *  Ordering            IOrderedFilter.Order — lower = earlier in Before phase
 *
 *  Short-circuit       Set context.Result in any Before callback
 *
 *  ServiceFilter       Filter resolved from DI (must be registered as service)
 *  TypeFilter          Filter created via ActivatorUtilities (no registration)
 *  IFilterFactory      Attribute creates its own filter instance (forward params)
 * ──────────────────────────────────────────────────────────────────────────
 */
