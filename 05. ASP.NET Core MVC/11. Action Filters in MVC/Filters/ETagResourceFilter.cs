/*
 * FILE ROLE: Demonstrates IResourceFilter — the filter stage that wraps the entire
 *   remaining pipeline (model binding + action filters + action + result filters +
 *   result execution).  ETagResourceFilter implements an ETag-based HTTP cache
 *   validation strategy: if the client sends a matching If-None-Match header, the
 *   filter short-circuits with a 304 Not Modified response, skipping the action
 *   entirely.  This is the most powerful short-circuit in the filter pipeline.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 8  — IResourceFilter: OnResourceExecuting (cache short-circuit),
 *                OnResourceExecuted (add ETag to response), wrapping model binding,
 *                IAsyncResourceFilter pattern
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 8: IResourceFilter — WRAPPING THE ENTIRE PIPELINE (CACHING USE CASE)
 * ─────────────────────────────────────────────────────────────────────────────
 * IResourceFilter is the second filter stage, running just after IAuthorizationFilter.
 * It wraps EVERYTHING that follows — model binding, action filters, the action method
 * itself, exception filters, and result execution.
 *
 * WHY IT IS UNIQUE AMONG FILTER TYPES:
 *   · It is the only filter type that wraps MODEL BINDING.  This means you can
 *     short-circuit before the framework allocates memory to deserialise the request
 *     body or bind form fields — ideal for caching use cases.
 *   · OnResourceExecuted runs AFTER result execution, giving you the full response
 *     for post-processing (e.g., caching the response, writing ETag headers).
 *
 * COMPARE WITH ACTION FILTERS:
 *   IActionFilter.OnActionExecuting — runs AFTER model binding has completed
 *   IResourceFilter.OnResourceExecuting — runs BEFORE model binding; maximum short-circuit
 *
 * ResourceExecutingContext KEY PROPERTIES:
 *   .HttpContext        — full request context
 *   .ActionDescriptor   — action about to execute
 *   .Result             — set to IActionResult to short-circuit (skip model binding,
 *                         action filters, action method, and result filters)
 *
 * ResourceExecutedContext KEY PROPERTIES:
 *   .Result             — IActionResult that was executed (ViewResult, JsonResult, etc.)
 *   .Exception          — any exception that propagated from the pipeline
 *   .ExceptionHandled   — set to true to suppress the exception
 *   .Canceled           — true if short-circuited by this or another resource filter
 *
 * ETAG CACHE VALIDATION (this demo):
 *   ETags (Entity Tags) are identifiers for a version of a resource.
 *   HTTP cache validation flow:
 *     1. First request:  server returns 200 + ETag: "products-v1"
 *     2. Client caches the response and stores the ETag
 *     3. Next request:   client sends If-None-Match: "products-v1"
 *     4. Server checks: if the current ETag matches the client's → 304 Not Modified
 *                       if different → 200 with new content + new ETag
 *
 *   Benefits: bandwidth saved (no response body on 304), lower server CPU,
 *   faster perceived performance for the user.
 *
 * PRODUCTION ETag GENERATION:
 *   This demo uses a simple route-based ETag.  In production, compute the ETag
 *   from a hash of the actual response content or from a database row version
 *   (e.g., SQL Server ROWVERSION, EF Core .IsRowVersion()).
 *
 * ASYNC VARIANT (IAsyncResourceFilter):
 *   public async Task OnResourceExecutionAsync(
 *       ResourceExecutingContext context, ResourceExecutionDelegate next)
 *   {
 *       // before (OnResourceExecuting equivalent)
 *       if (cachedHit) { context.Result = cachedResult; return; } // short-circuit
 *       ResourceExecutedContext executed = await next();
 *       // after (OnResourceExecuted equivalent)
 *       CacheResponse(executed);
 *   }
 *
 * REGISTRATION:
 *   Registered as Scoped in DI and applied via [ServiceFilter(typeof(ETagResourceFilter))]
 *   on ProductsController (controller level) — see Controllers/ProductsController.cs.
 */
public sealed class ETagResourceFilter : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // Compute a simple, deterministic ETag for this route.
        // Format: "{controller}-{action}-v1"  (quoted string per RFC 7232)
        string etag = ComputeETag(context);

        // Store the computed ETag in HttpContext.Items so OnResourceExecuted can use it
        // without recomputing — avoids any state on the filter instance itself.
        context.HttpContext.Items["ETagResourceFilter_ETag"] = etag;

        // Check the If-None-Match header sent by the client
        string ifNoneMatch = context.HttpContext.Request.Headers.IfNoneMatch.ToString();

        if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch == etag)
        {
            // Client's cached version is current — short-circuit with 304 Not Modified.
            // This skips model binding, all action filters, the action method itself,
            // and all result filters.  The 304 response has no body.
            context.Result = new StatusCodeResult(304);
        }
        // If no match: do nothing — pipeline continues normally to the action
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // After the full pipeline has run (action executed + result rendered).
        // Add the ETag header to the response so the client can cache it for future requests.
        if (!context.Canceled                                             // not short-circuited
            && context.HttpContext.Items["ETagResourceFilter_ETag"] is string etag
            && context.HttpContext.Response.StatusCode == 200)            // only cache 200 responses
        {
            // ETag header: the client will send this back as If-None-Match next time
            context.HttpContext.Response.Headers.ETag = etag;

            // Cache-Control: tell the client to validate with the server after 60 s
            context.HttpContext.Response.Headers.CacheControl = "max-age=60, must-revalidate";
        }
    }

    // Generates a deterministic ETag from the current controller and action route values.
    // In a real app: use a hash of the response content or a DB row-version stamp.
    private static string ComputeETag(ResourceExecutingContext context)
    {
        string controller = context.RouteData.Values["controller"] as string ?? "unknown";
        string action     = context.RouteData.Values["action"]     as string ?? "unknown";
        return $"\"{controller}-{action}-v1\""; // double-quoted per RFC 7232 strong ETag format
    }
}
