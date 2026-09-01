/*
 * FILE ROLE: Demonstrates ActionFilterAttribute — the abstract base class that
 *   implements both IActionFilter AND IResultFilter in a single type.  Inherit from
 *   it to create attribute-based filters that can be applied with [PerformanceActionFilter]
 *   syntax directly on controllers and actions without ServiceFilter or TypeFilter wrappers.
 *   Also shows IResultFilter / IAsyncResultFilter (OnResultExecuting / OnResultExecuted)
 *   and how to safely store per-request state in HttpContext.Items.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 5  — ActionFilterAttribute: inherits IActionFilter + IResultFilter,
 *                override On*Executing / On*Executed, use as [Attribute] directly
 *   SECTION 5a — IResultFilter: OnResultExecuting / OnResultExecuted,
 *                intercepting ViewResult before rendering, IAsyncResultFilter pattern
 */

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 5: ActionFilterAttribute — BASE CLASS FOR ATTRIBUTE-BASED FILTERS
 * ─────────────────────────────────────────────────────────────────────────────
 * ActionFilterAttribute is an abstract class in Microsoft.AspNetCore.Mvc.Filters
 * that implements four interfaces at once:
 *   · IActionFilter       (OnActionExecuting / OnActionExecuted)
 *   · IAsyncActionFilter  (OnActionExecutionAsync)
 *   · IResultFilter       (OnResultExecuting / OnResultExecuted)
 *   · IAsyncResultFilter  (OnResultExecutionAsync)
 *
 * Override ONLY the methods you need — the base class provides empty defaults.
 * Do NOT override both sync and async variants of the same stage; pick one.
 *
 * ATTRIBUTE USAGE:
 *   [PerformanceActionFilter]                         // default order
 *   [PerformanceActionFilter(Order = 10)]             // explicit order (higher = later)
 *   public IActionResult Create(ProductViewModel m) { ... }
 *
 * IMPORTANT — instance field sharing:
 *   When used as a controller/action attribute, ONE instance of the attribute class
 *   is created at startup and reused across ALL requests.  Instance fields are
 *   therefore NOT per-request safe:
 *
 *   WRONG: private Stopwatch _sw = new();  // shared across all requests → race condition
 *   RIGHT: store state in HttpContext.Items[key] — a per-request dictionary
 *
 * This filter does NOT require DI registration because it is instantiated directly
 * as an attribute.  If you need DI-injected services, use ServiceFilterAttribute
 * or implement IFilterFactory (see Filters/ApiKeyAuthorizationFilter.cs).
 *
 * Order property (inherited from IOrderedFilter):
 *   The Order property on any filter controls execution order within the same scope.
 *   Lower Order = runs first in "before" phase, last in "after" phase.
 *   Default Order = 0.  Set it on the attribute: [PerformanceActionFilter(Order = -10)]
 */

/*
 * SECTION 5a: IResultFilter — OnResultExecuting / OnResultExecuted
 * ─────────────────────────────────────────────────────────────────────────────
 * ActionFilterAttribute also covers IResultFilter, which wraps the execution of
 * IActionResult (rendering a ViewResult, serialising JSON, writing a redirect, etc.):
 *
 *   OnResultExecuting(ResultExecutingContext context)
 *     → Called just BEFORE IActionResult.ExecuteResultAsync() writes the response.
 *     → context.Result holds the IActionResult — you can inspect or REPLACE it here.
 *     → context.Cancel = true skips result execution (writes no response body).
 *     → Particularly useful for MVC: check `context.Result is ViewResult` to inject
 *       ViewData entries or swap the view name before the Razor engine renders.
 *
 *   OnResultExecuted(ResultExecutedContext context)
 *     → Called just AFTER the response body has been written.
 *     → context.Exception holds any exception from result execution.
 *     → The response is already committed; you CANNOT change headers or body here.
 *     → Useful for: cleanup, final metrics, post-render logging.
 *
 * IAsyncResultFilter (single-method async variant):
 *   public async Task OnResultExecutionAsync(
 *       ResultExecutingContext context, ResultExecutionDelegate next)
 *   {
 *       // before rendering
 *       ResultExecutedContext executed = await next();  // render the result
 *       // after rendering
 *   }
 *
 * RESULT FILTERS RUN EVEN AFTER ACTION SHORT-CIRCUITS:
 *   If an action filter sets context.Result (short-circuit), the action method is
 *   skipped but result filters STILL execute for that short-circuit result.
 *   This is intentional — it allows result filters to apply final transformations
 *   (e.g., compression, ETag headers) to ALL responses regardless of origin.
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class PerformanceActionFilter : ActionFilterAttribute
{
    // WRONG APPROACH (shown here for contrast — do NOT do this):
    // private Stopwatch? _sw;  ← shared across requests; race condition under load

    // Unique key for HttpContext.Items — namespaced to avoid clashes with other filters
    private const string TimingKey = "PerformanceFilter_StartTicks";

    // ── IActionFilter overrides ──────────────────────────────────────────────

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Store the start timestamp in the per-request HttpContext.Items dictionary.
        // GetTimestamp() is higher resolution than DateTime.UtcNow and avoids clock jumps.
        context.HttpContext.Items[TimingKey] = Stopwatch.GetTimestamp(); // long, not null
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Compute elapsed time and add it as a response header for debugging.
        if (context.HttpContext.Items[TimingKey] is long startTicks)
        {
            long elapsedMs = (Stopwatch.GetTimestamp() - startTicks) * 1000
                             / Stopwatch.Frequency;                         // ms since OnActionExecuting
            context.HttpContext.Response.Headers["X-Action-Ms"] =
                elapsedMs.ToString();                                        // e.g., "X-Action-Ms: 42"
        }
    }

    // ── IResultFilter overrides ──────────────────────────────────────────────

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        // Runs just BEFORE the result (ViewResult, JsonResult, etc.) writes the response.
        // Here we demonstrate ViewResult interception — an MVC-specific capability.
        if (context.Result is ViewResult viewResult)
        {
            // Inject a value into ViewData that the Razor view can display.
            // This is how cross-cutting concerns (e.g., breadcrumbs, feature flags)
            // can be injected into every view that goes through this filter.
            viewResult.ViewData["FilterActive"] = nameof(PerformanceActionFilter);
        }
    }

    public override void OnResultExecuted(ResultExecutedContext context)
    {
        // Runs just AFTER the response body has been committed.
        // The response is already sent — cannot modify headers or body here.
        // Useful for cleanup or logging the final status code.
        int statusCode = context.HttpContext.Response.StatusCode; // e.g., 200, 302
        if (context.Exception != null)
        {
            // An exception occurred during result execution (e.g., view engine error).
            // Log it; the exception STILL propagates unless ExceptionHandled = true.
            _ = statusCode; // suppress CA1804 in case of no-op — statusCode is for demo
        }
    }
}
