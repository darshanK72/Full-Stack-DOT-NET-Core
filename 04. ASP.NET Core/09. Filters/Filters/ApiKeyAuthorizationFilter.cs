/*
 * FILE ROLE: Teaches IAsyncAuthorizationFilter (the Authorization stage — first
 *            filter to run) and IAsyncResourceFilter (the Resource stage — second,
 *            before model binding), covering AuthorizationFilterContext, short-
 *            circuiting at the earliest pipeline stages, and when to use each stage.
 * SECTIONS IN THIS FILE:
 *   6. IAuthorizationFilter / IAsyncAuthorizationFilter — API key check
 *   7. IResourceFilter / IAsyncResourceFilter — pre-binding cache pattern
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Filters.Filters;

/*
 * SECTION 6: IAsyncAuthorizationFilter — AUTHORIZATION FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * The authorization filter is the FIRST stage in the MVC filter pipeline.
 * It runs before model binding, resource filters, and action filters.
 * Setting context.Result here short-circuits the entire inner pipeline:
 * no other filter stage runs.
 *
 * IAuthorizationFilter — sync:
 *   void OnAuthorization(AuthorizationFilterContext context)
 *
 * IAsyncAuthorizationFilter — async (preferred when doing async I/O):
 *   Task OnAuthorizationAsync(AuthorizationFilterContext context)
 *
 * AuthorizationFilterContext properties:
 *   .HttpContext       — raw HttpContext: request headers, User (ClaimsPrincipal)
 *   .Result            — set to any IActionResult to short-circuit immediately
 *   .ActionDescriptor  — action metadata (name, route values, attributes)
 *   .Filters           — all IFilterMetadata instances applied to this action
 *
 * SHORT-CIRCUITING:
 *   Set context.Result = new UnauthorizedResult() (or ForbidResult(), etc.)
 *   and return.  Every filter stage after Authorization is skipped.
 *
 * IMPORTANT — Do NOT re-implement authentication here:
 *   Use ASP.NET Core's built-in middleware for standard schemes:
 *     app.UseAuthentication()  — validates JWT, Cookie, etc.
 *     app.UseAuthorization()   — enforces [Authorize] policies
 *   Authorization filters are for CUSTOM, non-scheme checks:
 *     API keys in a header, HMAC request signatures, IP allow-lists, etc.
 *
 * Apply via [ServiceFilter(typeof(ApiKeyAuthorizationFilter))] on a controller
 * or action.  DI registration is required:
 *   builder.Services.AddScoped<ApiKeyAuthorizationFilter>();
 *   (registered in Program.cs)
 */
public sealed class ApiKeyAuthorizationFilter : IAsyncAuthorizationFilter
{
    private const string ApiKeyHeader = "X-API-Key";          // expected header name
    private const string ValidKey     = "demo-secret-key-123"; // hard-coded for tutorial demo

    private readonly ILogger<ApiKeyAuthorizationFilter> _logger;

    public ApiKeyAuthorizationFilter(ILogger<ApiKeyAuthorizationFilter> logger)
    {
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // TryGetValue returns false if the header is absent entirely
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out StringValues values)
            || values.Count == 0)
        {
            _logger.LogWarning("Request rejected: missing {Header} header", ApiKeyHeader);
            context.Result = new UnauthorizedResult(); // 401 — header absent
            return Task.CompletedTask;
        }

        string? providedKey = values[0]; // first value of the header

        if (providedKey != ValidKey)
        {
            _logger.LogWarning("Request rejected: invalid API key provided");
            context.Result = new ForbidResult(); // 403 — header present but key wrong
            return Task.CompletedTask;
        }

        _logger.LogInformation("API key authorized — continuing pipeline");
        return Task.CompletedTask; // no context.Result set → pipeline continues
    }
}

/*
 * SECTION 7: IAsyncResourceFilter — RESOURCE FILTER
 * ─────────────────────────────────────────────────────────────────────────────
 * The resource filter runs after Authorization but BEFORE model binding.
 * It is the second stage of the MVC filter pipeline and wraps the rest:
 * model binding, action execution, exception handling, and result execution.
 *
 * IResourceFilter — sync:
 *   void OnResourceExecuting(ResourceExecutingContext context)
 *   void OnResourceExecuted(ResourceExecutedContext context)
 *
 * IAsyncResourceFilter — async:
 *   Task OnResourceExecutionAsync(ResourceExecutingContext context,
 *                                  ResourceExecutionDelegate next)
 *
 * Code before await next()  → runs like OnResourceExecuting (before model binding)
 * Code after  await next()  → runs like OnResourceExecuted  (after full pipeline)
 *
 * ResourceExecutingContext (BEFORE model binding):
 *   .Result    — set to short-circuit model binding + action + result stages
 *   .HttpContext — full request context
 *
 * ResourceExecutedContext (AFTER the inner pipeline completes):
 *   .Result    — the IActionResult the action produced
 *
 * PRIMARY USE — Whole-response caching:
 *   In OnResourceExecuting:  check a cache keyed to the request path/query.
 *                            If hit, set context.Result = cached result and return.
 *                            Model binding and the action are skipped entirely.
 *   In OnResourceExecuted:   store the produced result in the cache.
 *
 * Why not [ResponseCache] or Output Caching middleware?
 *   IResourceFilter gives programmatic control over cache keys, bypass conditions,
 *   and invalidation that attribute-level or middleware caching cannot express
 *   when the key depends on runtime state (e.g. user ID, request headers).
 *
 * Apply via [TypeFilter(typeof(ResponseCacheResourceFilter))] at controller or
 * action scope, or via options.Filters.Add<ResponseCacheResourceFilter>() globally.
 */
public sealed class ResponseCacheResourceFilter : IAsyncResourceFilter
{
    // In a real app inject IMemoryCache or IDistributedCache here
    private readonly ILogger<ResponseCacheResourceFilter> _logger;

    public ResponseCacheResourceFilter(ILogger<ResponseCacheResourceFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnResourceExecutionAsync(
        ResourceExecutingContext context,
        ResourceExecutionDelegate next)
    {
        string cacheKey = context.HttpContext.Request.Path.ToString(); // simplistic key for demo

        // ── Cache lookup (Before model binding) ───────────────────────────
        // Production pattern:
        //   if (_cache.TryGetValue(cacheKey, out IActionResult? cached))
        //   { context.Result = cached; return; }  // short-circuit — skip action
        _logger.LogInformation("[Resource:Before] cache miss for {Key}", cacheKey);

        ResourceExecutedContext executed = await next(); // model binding + action + result

        // ── Cache store (After inner pipeline) ────────────────────────────
        // Production pattern:
        //   if (executed.Result is not null && executed.Exception is null)
        //       _cache.Set(cacheKey, executed.Result, TimeSpan.FromMinutes(5));
        _logger.LogInformation("[Resource:After] storing result for {Key}", cacheKey);

        _ = executed; // suppress CS8600 — result used in production pattern above
    }
}
