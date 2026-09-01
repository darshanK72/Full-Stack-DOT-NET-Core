/*
 * FILE ROLE: Demonstrates IAsyncActionFilter — the async version of the action filter
 *   stage.  AuditActionFilter logs every controller action entry and exit, including
 *   action arguments, elapsed time, and any exception that propagated out.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 3  — IAsyncActionFilter: OnActionExecutionAsync, ActionExecutingContext,
 *                ActionExecutedContext, await next(), short-circuit pattern
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 3: IAsyncActionFilter — ASYNC ACTION FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * IAsyncActionFilter is the async counterpart to IActionFilter.  Implement it
 * when your "before" or "after" logic involves I/O (DB, HTTP, file) — or when
 * you simply prefer a single method over two.
 *
 * SINGLE-METHOD PATTERN:
 *   The async interface collapses OnActionExecuting + OnActionExecuted into ONE
 *   method.  Code before `await next()` is the "executing" phase; code after is
 *   the "executed" phase:
 *
 *   public async Task OnActionExecutionAsync(
 *       ActionExecutingContext context,   // before: arguments, route data, HttpContext
 *       ActionExecutionDelegate next)     // call to run the action + remaining filters
 *   {
 *       // BEFORE — action has NOT run yet
 *       ActionExecutedContext executed = await next();   // run action + downstream filters
 *       // AFTER  — action HAS run; executed.Result holds the IActionResult
 *   }
 *
 * SHORT-CIRCUIT (skip the action entirely):
 *   Set context.Result BEFORE calling next() to short-circuit — the action method
 *   and all inner action filters are skipped.  Result filters STILL run.
 *   if (blocked) { context.Result = new ForbidResult(); return; }  // skip next()
 *
 * ActionExecutingContext KEY PROPERTIES:
 *   .ActionDescriptor.DisplayName    — "ProductsController.Index (ActionFiltersMvc)"
 *   .ActionArguments                 — IDictionary<string, object?> of bound parameters
 *   .HttpContext                     — full HttpContext (Request, Response, User, Items)
 *   .RouteData                       — route values (controller, action, id)
 *   .ModelState                      — binding and validation errors before action runs
 *   .Result                          — set to IActionResult to short-circuit pipeline
 *
 * ActionExecutedContext KEY PROPERTIES (returned by await next()):
 *   .Result                          — IActionResult the action returned (or null if exception)
 *   .Exception                       — any unhandled exception thrown by the action
 *   .ExceptionHandled                — set to true to suppress the exception
 *   .Canceled                        — true if a previous filter short-circuited
 *
 * RULE: Implement IAsyncActionFilter OR IActionFilter — not both.
 *   If both are implemented on the same class, the framework calls only the async version.
 *
 * DI LIFETIME FOR THIS FILTER:
 *   Registered as Scoped (one instance per HTTP request) in Program.cs.
 *   This is appropriate because ILogger<T> is itself registered with a scoped lifetime
 *   by ASP.NET Core's default logging infrastructure.
 */
public sealed class AuditActionFilter : IAsyncActionFilter
{
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(ILogger<AuditActionFilter> logger) // injected by DI
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // ── BEFORE phase ────────────────────────────────────────────────────
        string actionName = context.ActionDescriptor.DisplayName ?? "Unknown"; // e.g. "ProductsController.Create"
        string httpMethod = context.HttpContext.Request.Method;                // GET, POST, etc.

        // Log the action arguments for audit trail (argument names + types, not values
        // in production to avoid logging sensitive data)
        IEnumerable<string> argNames = context.ActionArguments.Keys;          // IDs / parameter names
        _logger.LogInformation(
            "[AUDIT] → {Method} {Action} | Args: [{Args}]",
            httpMethod,
            actionName,
            string.Join(", ", argNames));

        // Record start time using HttpContext.Items so it is per-request safe.
        // Storing state in filter instance fields is NOT safe for attribute-based
        // filters because attribute instances are shared across requests.
        long startTicks = System.Diagnostics.Stopwatch.GetTimestamp(); // high-res timer
        context.HttpContext.Items["AuditFilter_Start"] = startTicks;   // per-request storage

        // ── CALL NEXT (runs the action and all remaining action filters) ────
        ActionExecutedContext executed = await next();

        // ── AFTER phase ─────────────────────────────────────────────────────
        long endTicks = System.Diagnostics.Stopwatch.GetTimestamp();
        double elapsedMs = (endTicks - startTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;

        if (executed.Exception != null && !executed.ExceptionHandled)
        {
            // Exception propagated out of the action — log it here (exception filters
            // will handle the response, but we still want the audit trail)
            _logger.LogError(
                executed.Exception,
                "[AUDIT] ✗ {Action} faulted after {Ms:F1} ms",
                actionName,
                elapsedMs);
        }
        else
        {
            string resultType = executed.Result?.GetType().Name ?? "null"; // ViewResult, RedirectResult, etc.
            _logger.LogInformation(
                "[AUDIT] ← {Action} completed in {Ms:F1} ms | Result: {Result}",
                actionName,
                elapsedMs,
                resultType);
        }
    }
}
