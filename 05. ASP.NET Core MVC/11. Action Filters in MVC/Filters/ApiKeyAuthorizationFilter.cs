/*
 * FILE ROLE: Demonstrates IAuthorizationFilter and IFilterFactory.
 *   ApiKeyAuthorizationFilter runs in the Authorization stage — the FIRST and
 *   outermost filter stage — and short-circuits the entire pipeline with a 401
 *   if no valid API key is present.  RequireApiKeyAttribute implements IFilterFactory
 *   to bridge the gap between attributes (which cannot use DI directly) and filters
 *   that need DI services — a common real-world pattern.
 *
 * SECTIONS IN THIS FILE:
 *   SECTION 7  — IAuthorizationFilter: AuthorizationFilterContext, short-circuit,
 *                position in the pipeline, no access to action result
 *   SECTION 7a — IFilterFactory: per-request filter construction, IsReusable,
 *                [RequireApiKey] attribute as IFilterFactory, DI-friendly attributes
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ActionFiltersMvc.Filters;

/*
 * SECTION 7: IAuthorizationFilter — THE FIRST FILTER STAGE
 * ─────────────────────────────────────────────────────────────────────────────
 * IAuthorizationFilter (and its async counterpart IAsyncAuthorizationFilter) runs
 * BEFORE all other filter stages — before resource filters, before model binding,
 * and certainly before action filters.
 *
 * WHY IT RUNS FIRST:
 *   The authorization stage is designed to gate the entire rest of the pipeline.
 *   If a request lacks credentials, we want to reject it immediately without
 *   consuming any resources (no model binding, no action execution, no views).
 *
 * AuthorizationFilterContext KEY PROPERTIES:
 *   .HttpContext        — access request headers, User (ClaimsPrincipal), path, etc.
 *   .ActionDescriptor   — the action being invoked
 *   .Filters            — the full ordered list of filters for this invocation
 *   .Result             — set to IActionResult to SHORT-CIRCUIT the entire pipeline;
 *                         all remaining filters (resource, action, result) are skipped
 *
 * SHORT-CIRCUIT RESULTS FOR AUTHORIZATION:
 *   context.Result = new UnauthorizedResult()      // 401 — no valid credentials
 *   context.Result = new ForbidResult()            // 403 — authenticated but not authorised
 *   context.Result = new ChallengeResult()         // 401 + redirect to login (for cookies)
 *
 * IMPORTANT LIMITATIONS:
 *   · Authorization filters do NOT have access to an IActionResult returned by the action.
 *     They can ONLY block or allow — they cannot modify the final response.
 *   · They run BEFORE model binding, so ActionArguments are not available.
 *   · They run BEFORE [Authorize] attribute handling by the built-in authorization
 *     middleware if this filter is placed before UseAuthorization() in the pipeline.
 *     In practice, combine with app.UseAuthorization() and [Authorize] for policy-based
 *     auth; use IAuthorizationFilter for custom header/token checks.
 *
 * SYNC VS ASYNC:
 *   IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
 *   IAsyncAuthorizationFilter.OnAuthorizationAsync(AuthorizationFilterContext context)
 *   Implement ONE of these, not both.  Use async when you need to call an async
 *   service (e.g., validate an API key against a database).
 *
 * THIS DEMO USES A HARDCODED KEY for simplicity.
 * In production, read the valid key from IConfiguration (appsettings.json / secrets).
 */
public sealed class ApiKeyAuthorizationFilter : IAuthorizationFilter
{
    private const string ApiKeyHeader = "X-Api-Key";     // header the client must send
    private const string ValidApiKey  = "demo-key-12345"; // in production: IConfiguration["ApiKey"]

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Check if the API key header is present and matches the expected value
        string? providedKey = context.HttpContext.Request.Headers[ApiKeyHeader]; // StringValues → string?

        if (string.IsNullOrWhiteSpace(providedKey) || providedKey != ValidApiKey)
        {
            // Short-circuit: no further filter stages run; action method never executes
            context.Result = new UnauthorizedResult(); // HTTP 401
            return;
        }
        // Key is valid — do nothing; pipeline continues to the next filter stage
    }
}

/*
 * SECTION 7a: IFilterFactory — DI-FRIENDLY ATTRIBUTE PATTERN
 * ─────────────────────────────────────────────────────────────────────────────
 * PROBLEM: Attribute instances are created by the runtime at class/method discovery
 * time, not at request time, so they CANNOT receive constructor-injected DI services.
 * A plain [MyAttribute] on a controller cannot get IConfiguration, ILogger, etc.
 *
 * SOLUTION: IFilterFactory
 *   Make the ATTRIBUTE implement IFilterFactory.  The attribute itself holds no
 *   filter logic — it is just a marker and a factory.  At each request, MVC calls
 *   CreateInstance(IServiceProvider) to construct the REAL filter from DI.
 *
 * IFilterFactory INTERFACE:
 *   IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
 *     → Called once per request when the filter is invoked.
 *     → serviceProvider is the request-scoped IServiceProvider.
 *     → Return the resolved filter instance (must implement IFilterMetadata).
 *
 *   bool IsReusable
 *     → false: CreateInstance() is called for EVERY request (use for Scoped/Transient filters)
 *     → true:  the instance returned by CreateInstance() may be cached and reused
 *              (safe ONLY for Singleton filters with no per-request state)
 *   Always set IsReusable = false when the filter has Scoped dependencies (e.g., DbContext).
 *
 * USAGE:
 *   [RequireApiKey]                    // attribute syntax — triggers CreateInstance() at runtime
 *   public class SecureController : Controller { ... }
 *
 * COMPARE:
 *   [RequireApiKey]                    IFilterFactory — attribute, DI-resolved each request
 *   [ServiceFilter(typeof(T))]         shorthand IFilterFactory built in to MVC; T must be in DI
 *   [TypeFilter(typeof(T))]            shorthand without DI registration; ctor args via Arguments
 *
 *   [RequireApiKey] is the preferred pattern when you want clean attribute syntax AND DI.
 *   ServiceFilterAttribute is equally valid and requires less boilerplate.
 */
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireApiKeyAttribute : Attribute, IFilterFactory
{
    // false: create a new ApiKeyAuthorizationFilter from DI on every request
    // This is mandatory because ApiKeyAuthorizationFilter could be Scoped in DI.
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        // Resolve the actual filter from the request-scoped DI container.
        // ApiKeyAuthorizationFilter must be registered in DI (see Program.cs).
        return serviceProvider.GetRequiredService<ApiKeyAuthorizationFilter>();
    }
}
