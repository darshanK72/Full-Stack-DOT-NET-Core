/*
 * FILE ROLE: Demonstrates IAsyncExceptionFilter — the filter-pipeline's exception
 *   handling stage.  MvcExceptionFilter catches unhandled exceptions thrown by any
 *   action or result execution and returns either a Razor ViewResult (HTML error page
 *   for browser requests) or an ObjectResult with ProblemDetails (JSON for API clients).
 *   This is an MVC-specific pattern: only MVC filters can return a ViewResult; plain
 *   ASP.NET Core middleware cannot render Razor views.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 6  — IAsyncExceptionFilter: ExceptionContext, ExceptionHandled,
 *                ViewResult for HTML errors, ObjectResult/ProblemDetails for JSON,
 *                SkipStatusCodePages, Accept-header sniffing
 */

using System.Threading.Tasks;
using ActionFiltersMvc.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 6: IAsyncExceptionFilter — EXCEPTION HANDLING IN THE FILTER PIPELINE
 * ─────────────────────────────────────────────────────────────────────────────
 * IAsyncExceptionFilter (and its sync counterpart IExceptionFilter) is triggered
 * when an UNHANDLED exception propagates out of:
 *   · The controller action method
 *   · An action filter's OnActionExecuting / OnActionExecuted
 *   · A result filter's OnResultExecuting
 *
 * It does NOT handle exceptions from middleware outside of MVC, nor 404s generated
 * by the router (no route matched).  For those, use app.UseExceptionHandler() or
 * app.UseStatusCodePagesWithReExecute() in Program.cs middleware.
 *
 * ExceptionContext KEY PROPERTIES:
 *   .Exception          — the unhandled exception (always non-null when filter fires)
 *   .ExceptionHandled   — set to TRUE to suppress the exception (prevent rethrow);
 *                         if left false, the exception continues to propagate
 *   .Result             — set to IActionResult to define the response; combined with
 *                         ExceptionHandled = true, this fully handles the exception
 *   .HttpContext        — full HttpContext for request details, headers, TraceIdentifier
 *   .ActionDescriptor   — the action that threw the exception
 *   .ModelState         — binding errors if any existed before the exception
 *
 * RETURNING ViewResult FROM AN EXCEPTION FILTER (MVC-specific):
 *   Exception filters have access to the MVC infrastructure, unlike middleware.
 *   They can return a ViewResult that the Razor view engine will render.
 *   This requires:
 *     1. A ViewDataDictionary<TModel> populated with the error model
 *     2. A ViewResult with ViewName pointing to the error Razor view
 *   IModelMetadataProvider (injected via DI) is needed to construct ViewDataDictionary.
 *
 * RETURNING JSON ProblemDetails FOR API REQUESTS:
 *   When the Accept header contains "application/json", return an ObjectResult
 *   wrapping a ProblemDetails object (RFC 7807 — standard JSON error format).
 *   StatusCode = 500 must be set on the ObjectResult, not just ProblemDetails.Status.
 *
 * --- 6a. SkipStatusCodePages ---
 * [SkipStatusCodePages] and SetSkipStatusCodePages():
 *   The StatusCodePages middleware (app.UseStatusCodePages()) intercepts responses
 *   with status codes like 404 or 500 and rewrites them with its own error page.
 *   When a filter already produces a custom error response (e.g., our ViewResult),
 *   we want StatusCodePages to leave it alone.
 *
 *   Two ways to suppress StatusCodePages for a given response:
 *     a) [SkipStatusCodePages] attribute on the controller or action
 *        → tells the middleware to skip for that route unconditionally
 *     b) Disable the IStatusCodePagesFeature in a filter or middleware:
 *          var feat = ctx.HttpContext.Features.Get<IStatusCodePagesFeature>();
 *          if (feat is not null) feat.Enabled = false;
 *        This is equivalent to [SkipStatusCodePages] but applied at runtime.
 *
 *   We disable the feature below in the JSON error path because ObjectResult
 *   sets StatusCode = 500, and without it the StatusCodePages middleware might
 *   rewrite the response before the client receives our JSON.
 *
 * REGISTRATION:
 *   Registered globally in Program.cs via options.Filters.Add<MvcExceptionFilter>().
 *   Also registered as Scoped in DI so AddControllersWithViews can resolve it.
 *   IModelMetadataProvider and IWebHostEnvironment are injected automatically.
 */
public sealed class MvcExceptionFilter : IAsyncExceptionFilter
{
    private readonly IModelMetadataProvider _metadataProvider; // needed for ViewDataDictionary ctor
    private readonly IWebHostEnvironment _env;                 // IsDevelopment() → show stack trace
    private readonly ILogger<MvcExceptionFilter> _logger;

    public MvcExceptionFilter(
        IModelMetadataProvider metadataProvider,
        IWebHostEnvironment env,
        ILogger<MvcExceptionFilter> logger)
    {
        _metadataProvider = metadataProvider;
        _env = env;
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        _logger.LogError(
            context.Exception,
            "[MvcExceptionFilter] Unhandled exception in {Action}",
            context.ActionDescriptor.DisplayName);

        // Detect whether the client expects JSON or HTML
        string accept = context.HttpContext.Request.Headers.Accept.ToString();
        bool isApiRequest = accept.Contains("application/json", System.StringComparison.OrdinalIgnoreCase);

        if (isApiRequest)
        {
            // ── JSON response for API clients ────────────────────────────────
            // ProblemDetails is the RFC 7807 standard for HTTP error payloads.
            // StatusCode on ProblemDetails is informational; the actual HTTP status
            // code comes from ObjectResult.StatusCode below.
            var problem = new ProblemDetails
            {
                Status  = 500,
                Title   = "An unexpected error occurred.",
                Detail  = _env.IsDevelopment()
                    ? $"{context.Exception.GetType().Name}: {context.Exception.Message}"
                    : "Please contact support if the problem persists.",
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(problem) { StatusCode = 500 }; // HTTP 500

            // Tell StatusCodePages middleware to leave this 500 response alone —
            // we have already formatted it exactly as we want it.
            // Programmatic equivalent of [SkipStatusCodePages] attribute:
            IStatusCodePagesFeature? statusCodeFeature =
                context.HttpContext.Features.Get<IStatusCodePagesFeature>();
            if (statusCodeFeature is not null)
                statusCodeFeature.Enabled = false;
        }
        else
        {
            // ── HTML ViewResult response for browser clients ──────────────────
            // Build a ViewDataDictionary<ErrorViewModel> so the Razor view is
            // strongly typed and can use @model ErrorViewModel.
            string message = _env.IsDevelopment()
                ? $"{context.Exception.GetType().Name}: {context.Exception.Message}"
                : "An unexpected error occurred. Please try again later.";

            var viewData = new ViewDataDictionary<ErrorViewModel>(_metadataProvider, context.ModelState)
            {
                Model = new ErrorViewModel
                {
                    RequestId = context.HttpContext.TraceIdentifier, // W3C trace ID
                    Message   = message
                }
            };

            // ViewName: absolute path avoids discovery ambiguity from any controller
            context.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = viewData
            };
            // Note: HTTP status is 200 from ViewResult by default.
            // To return 500 with the HTML view:
            //   context.HttpContext.Response.StatusCode = 500;
            // However, this can trigger StatusCodePages middleware to rewrite the
            // response. With the HTML view approach we typically leave it as 200
            // or call SetSkipStatusCodePages() if we need a non-200 status.
        }

        context.ExceptionHandled = true; // suppress rethrow — filter has fully handled it
        return Task.CompletedTask;
    }
}
