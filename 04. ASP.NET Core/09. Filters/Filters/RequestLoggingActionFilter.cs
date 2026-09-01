/*
 * FILE ROLE: Teaches IAsyncActionFilter (the Action stage) and IAsyncResultFilter
 *            (the Result stage) of the ASP.NET Core filter pipeline, including
 *            ActionExecutingContext, ActionExecutedContext, short-circuiting,
 *            and the IOrderedFilter.Order property.
 * SECTIONS IN THIS FILE:
 *   2. IAsyncActionFilter — request/response logging with short-circuit note
 *   3. IResultFilter / IAsyncResultFilter — result-stage timing and headers
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Filters.Filters;

/*
 * SECTION 2: IAsyncActionFilter — ASYNC ACTION FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * IAsyncActionFilter has ONE method with a next() delegate pattern:
 *
 *   Task OnActionExecutionAsync(ActionExecutingContext context,
 *                               ActionExecutionDelegate next)
 *
 * Execution pattern:
 *   Code before await next()  →  runs like OnActionExecuting (before the action)
 *   ActionExecutedContext      ← returned by await next()
 *   Code after  await next()  →  runs like OnActionExecuted (after  the action)
 *
 * ActionExecutingContext (BEFORE — inspect / short-circuit):
 *   .ActionDescriptor     — DisplayName, RouteValues, action attributes
 *   .HttpContext           — raw HttpContext: headers, user, connection info
 *   .ActionArguments      — Dictionary<string, object?> of model-bound parameters
 *   .Result               — SET THIS to short-circuit (action will NOT execute)
 *   .Controller           — the controller instance (cast to access custom members)
 *
 * ActionExecutedContext (AFTER — returned by await next()):
 *   .Result               — the IActionResult produced by the action (may replace)
 *   .Exception            — non-null if the action threw an unhandled exception
 *   .ExceptionHandled     — set true on .Exception to suppress rethrow
 *   .Cancelled            — true if a previous filter set context.Result (short-circuit)
 *
 * SHORT-CIRCUITING:
 *   Do NOT call await next().
 *   Set context.Result to any IActionResult before returning.
 *   Filters that already entered "Before" will still receive "After" callbacks.
 *   Filters at equal or inner scope that have NOT entered "Before" are skipped.
 *
 *     // Short-circuit example inside OnActionExecutionAsync:
 *     if (!IsValid(context))
 *     {
 *         context.Result = new UnauthorizedResult(); // skip action + inner filters
 *         return;                                    // do NOT call await next()
 *     }
 *
 * SYNC VARIANT — IActionFilter:
 *   void OnActionExecuting(ActionExecutingContext context)
 *   void OnActionExecuted(ActionExecutedContext context)
 *   Use the sync variant only when no async I/O is needed inside the filter.
 *
 * IOrderedFilter.Order (filter ordering within the same scope):
 *   Implement IOrderedFilter to expose int Order { get; }.
 *   Lower Order value runs first in the Before phase (and last in the After phase).
 *   Default is 0.  Use negative values to ensure this filter wraps others.
 */
public sealed class RequestLoggingActionFilter : IAsyncActionFilter, IOrderedFilter
{
    private readonly ILogger<RequestLoggingActionFilter> _logger;

    public int Order => -1000; // run outermost among action filters at the same scope

    public RequestLoggingActionFilter(ILogger<RequestLoggingActionFilter> logger)
    {
        _logger = logger; // ILogger<T> resolved from DI by TypeFilter / AddControllers
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        string actionName = context.ActionDescriptor.DisplayName ?? "unknown"; // e.g. "OrdersController.GetById"
        Stopwatch sw = Stopwatch.StartNew();

        _logger.LogInformation("[Action:Before] {Action} | args: {Args}",
            actionName,
            string.Join(", ", context.ActionArguments.Keys)); // log bound param names

        // ── To short-circuit here: set context.Result and return ───────────
        // context.Result = new UnauthorizedResult();
        // return;  // do NOT call next() — the action will not run

        ActionExecutedContext executed = await next(); // ← invoke action + inner filters

        sw.Stop();

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            // Exception is present but not yet handled — log it; exception filter handles response
            _logger.LogError(executed.Exception,
                "[Action:After]  {Action} threw after {Ms}ms", actionName, sw.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogInformation("[Action:After]  {Action} completed in {Ms}ms | result: {Result}",
                actionName,
                sw.ElapsedMilliseconds,
                executed.Result?.GetType().Name ?? "null"); // e.g. "OkObjectResult"
        }
    }
}

/*
 * SECTION 3: IResultFilter / IAsyncResultFilter — RESULT FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * The result filter wraps IActionResult.ExecuteResultAsync — the step that
 * serializes the response body (JSON, HTML, files) and writes HTTP headers.
 *
 * IAsyncResultFilter — ONE method with a next() delegate:
 *
 *   Task OnResultExecutionAsync(ResultExecutingContext context,
 *                               ResultExecutionDelegate next)
 *
 * Code before await next()  → runs like OnResultExecuting (before serialization)
 * Code after  await next()  → runs like OnResultExecuted  (after  serialization)
 *
 * ResultExecutingContext (BEFORE serialization):
 *   .Result        — the IActionResult about to execute (may read or replace)
 *   .Cancel        — set true to skip result execution without setting .Result
 *   .HttpContext   — access Response.Headers here to add custom headers
 *
 * ResultExecutedContext (AFTER serialization — from await next()):
 *   .Result        — the IActionResult that ran
 *   .Exception     — non-null if result execution threw
 *   .Cancelled     — true if a previous filter set .Cancel
 *
 * SYNC VARIANT — IResultFilter:
 *   void OnResultExecuting(ResultExecutingContext context)
 *   void OnResultExecuted(ResultExecutedContext context)
 *   Prefer the async variant when you need async I/O in the callbacks.
 *
 * IMPORTANT: Result filters run only when the action produced an IActionResult.
 *   They also run when an exception filter sets context.Result to handle an
 *   exception — the pipeline resumes at the Result stage with that result.
 *
 * Apply via [TypeFilter(typeof(ResponseTimingResultFilter))] at action or
 * controller scope.  TypeFilter creates the instance via ActivatorUtilities;
 * ILogger<T> is resolved from DI automatically.
 */
public sealed class ResponseTimingResultFilter : IAsyncResultFilter
{
    private readonly ILogger<ResponseTimingResultFilter> _logger;

    public ResponseTimingResultFilter(ILogger<ResponseTimingResultFilter> logger)
    {
        _logger = logger; // injected by TypeFilter via ActivatorUtilities
    }

    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        // Add a custom response header before the result serializes
        context.HttpContext.Response.Headers["X-Filter-Timestamp"] =
            DateTimeOffset.UtcNow.ToString("O"); // ISO-8601 UTC timestamp

        Stopwatch sw = Stopwatch.StartNew();

        ResultExecutedContext executed = await next(); // ← serialize + write response body

        sw.Stop();
        _logger.LogInformation("[Result:After] {Result} serialized in {Ms}ms",
            executed.Result?.GetType().Name,  // e.g. "OkObjectResult"
            sw.ElapsedMilliseconds);
    }
}
