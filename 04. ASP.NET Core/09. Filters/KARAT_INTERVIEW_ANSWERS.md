# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/09. Filters`

---

#### Q1. (M) A teammate says "we'll put auth in an action filter instead of middleware." Walk through the MVC filter execution order — authorization filter, resource filter, action filter, exception filter, result filter — and explain which filter type owns authentication vs authorization vs action-specific validation.

**Answer:** MVC runs filters in a fixed pipeline around the action: authorization filters first (fail-fast on access), then resource filters (before/after resource execution), action filters (around the action method), exception filters (on unhandled exceptions from earlier stages), and result filters (around `IActionResult` execution). Authentication belongs in middleware (`UseAuthentication`); authorization belongs in authorization filters or `[Authorize]`; action-specific validation belongs in action filters.

- **Authorization filters** (`IAuthorizationFilter`, `[Authorize]`) run before model binding completes expensive work in some setups and set `context.Result` to short-circuit (401/403) before the action executes.
- **Resource filters** (`IResourceFilter`) wrap the entire remainder of the filter pipeline plus action execution — useful for request-scoped caching of lookup data or short-circuiting if a cached `IActionResult` already exists.
- **Action filters** (`IActionFilter` / `IAsyncActionFilter`) run immediately before and after the action — ideal for validating bound models, timing, or mutating `ActionExecutingContext`.
- **Exception filters** run only when an exception escapes the action/resource stages but before the result is committed — they do not catch middleware failures or exceptions in result execution unless configured separately.
- **Result filters** wrap execution of `IActionResult` (view rendering, JSON serialization).

**Production takeaway:** Putting authentication in an action filter runs too late for non-MVC endpoints and duplicates what `UseAuthentication`/`UseAuthorization` middleware already standardizes — filters are for MVC/action-scoped concerns after routing.

---

#### Q2. (R) Review this custom authorization filter. What breaks at runtime or under load, and what would you change?

```csharp
public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IUserRepository _users;

    public ApiKeyAuthFilter(IUserRepository users) => _users = users;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var key = context.HttpContext.Request.Headers["X-Api-Key"];
        var user = _users.FindByApiKey(key).Result;
        if (user is null)
            context.Result = new UnauthorizedResult();
    }
}
```

**Answer:** The filter blocks a thread with `.Result` on async database I/O inside the authorization path, causing sync-over-async under load, and it reads the header without validation while returning a bare `UnauthorizedResult` without `ProblemDetails` or logging.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `FindByApiKey` | Thread-pool starvation; blocked requests during I/O |
| API design | `StringValues` header assigned without `ToString()` / empty check | May query with invalid key shape; empty header passes to DB |
| HTTP / API | `UnauthorizedResult` with no body | Clients get empty 401; inconsistent with API error contract |
| Design | Sync authorization filter for async data access | Wrong filter interface — should be async auth handler or policy |

**Fix (priority order):**

1. Replace with `IAsyncAuthorizationFilter` and `await _users.FindByApiKeyAsync(key, context.HttpContext.RequestAborted)`.
2. Validate header with `TryGetValue`; compare API keys with fixed-time equality.
3. Set `context.Result` to `UnauthorizedObjectResult` with `ProblemDetails`, or use ASP.NET Core authentication handlers + `[Authorize]` policies instead of custom sync filter logic.

```csharp
public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
{
    if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var key)
        || !await _users.IsValidKeyAsync(key.ToString(), context.HttpContext.RequestAborted))
    {
        context.Result = new UnauthorizedObjectResult(new ProblemDetails
        {
            Status = 401, Title = "Unauthorized", Detail = "Invalid or missing API key."
        });
    }
}
```

**Production takeaway:** Authorization filters run on every matched MVC action — blocking I/O here multiplies across the entire controller surface.

---

#### Q3. (P) Implement cross-cutting request timing and audit logging around controller actions using `IAsyncActionFilter`. What runs in `OnActionExecutionAsync` before vs after `await next()`, and what can you still change at each stage?

**Answer:** Register an `IAsyncActionFilter`, run audit setup and timing before `await next()`, then inspect or log the outcome after `next()` returns — you can still short-circuit before `next()` by setting `context.Result`, but after `next()` the action has executed and you can read `context.Result` or handled exceptions from `ActionExecutedContext`.

- **Before `await next()`:** `ActionExecutingContext` — set `context.Result` to skip the action entirely (validation failure → 400); read route values and `HttpContext.User`.
- **`await next()`:** invokes downstream filters and the action method; on exception, `ActionExecutedContext.Exception` is populated unless another filter handled it.
- **After `await next()`:** log elapsed time, status code, user id; do not change the action's return value easily — mutate `context.Result` only if the action did not run or you are compensating.
- Register via `[ServiceFilter(typeof(AuditActionFilter))]` for DI or globally with `options.Filters.Add<AuditActionFilter>()`.
- Prefer `IAsyncActionFilter` over sync `IActionFilter` when logging or audit touches I/O.

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
{
    var sw = Stopwatch.StartNew();
    var executed = await next();
    sw.Stop();
    _logger.LogInformation("Action {Action} took {Ms}ms, status {Result}",
        ctx.ActionDescriptor.DisplayName, sw.ElapsedMilliseconds,
        executed.Result?.GetType().Name ?? "exception");
}
```

**Production takeaway:** Action filters see MVC context (action name, bound arguments) that middleware lacks — use them for per-action audit, not transport-level rejection.

---

#### Q4. (D) Your team needs to reject requests without a valid API key before routing reaches expensive database middleware. Another developer wants an `IActionFilter` on every controller. Compare filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). When is each the right seam?

**Answer:** Middleware short-circuit rejects requests before routing and endpoint selection — saving work for non-MVC paths and global gates; filter short-circuit only runs after MVC has selected a controller action, so expensive upstream middleware already ran.

- **Middleware** (`RequestDelegate` chain): runs for all requests matching its branch; skip `_next` to return 401/429 immediately; required for concerns before `UseRouting` (forwarded headers, global API key on all paths including minimal APIs and health endpoints if desired).
- **Filter short-circuit:** sets `context.Result` (e.g., `UnauthorizedResult`) so the action and later filters skip execution, but routing, model binding, and earlier middleware already executed.
- **Filter advantage:** access to `ActionDescriptor`, `[Authorize]` integration, per-controller policies, and `IAsyncActionFilter` with action arguments.
- **Middleware advantage:** uniform behavior for MVC + minimal APIs; runs before DI scope for unrelated endpoints; pairs with endpoint routing `MapWhen` for path-specific branches.

**Production takeaway:** "Reject before DB middleware" is a pipeline-order problem — action filters are the wrong layer; place API-key middleware before the expensive middleware, not on every controller.

---

#### Q5. (M) `[Authorize(Roles = "Admin")]` is implemented as an authorization filter. How does that differ from calling `app.UseAuthentication()` / `UseAuthorization()` middleware, and what happens if authorization middleware runs but no authorization filter is reached (e.g., minimal API endpoint)?

**Answer:** Middleware authentication populates `HttpContext.User` from cookies, JWT, or schemes; middleware authorization enforces policies via endpoint metadata (`RequireAuthorization()`). MVC `[Authorize]` adds an authorization filter that checks the same policy system but only on controller actions — minimal APIs rely on endpoint metadata, not MVC filters.

- `UseAuthentication()` runs authentication handlers and sets `ClaimsPrincipal` on `HttpContext.User`.
- `UseAuthorization()` evaluates `[Authorize]`, `[AllowAnonymous]`, and `RequireAuthorization()` metadata attached to endpoints — including minimal APIs and Razor Pages.
- MVC **authorization filters** are one integration point for `[Authorize]` on controllers; they call into `IAuthorizationService` with the same policies as middleware.
- A minimal API with `.RequireAuthorization("Admin")` never runs MVC authorization filters — only authorization middleware after routing.
- If authorization middleware is missing, `[Authorize]` on controllers may not enforce correctly; filters alone cannot authenticate — they only authorize an already-populated `User`.

**Production takeaway:** Teams mixing minimal APIs and controllers must configure authorization at the endpoint/middleware layer — sprinkling authorization filters on controllers leaves minimal API routes unprotected.

---

#### Q6. (R) Review this resource filter intended to cache expensive lookup data for the duration of one request. What is wrong?

```csharp
public class CatalogCacheResourceFilter : IResourceFilter
{
    private static readonly Dictionary<string, object> _cache = new();

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && _cache.TryGetValue(key, out var cached))
            context.HttpContext.Items["Catalog"] = cached;
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        var key = context.RouteData.Values["categoryId"]?.ToString();
        if (key != null && !context.HttpContext.Items.ContainsKey("Catalog"))
            _cache[key] = context.HttpContext.Items["Catalog"]!;
    }
}
```

**Answer:** A `static` dictionary shares catalog data across all requests and users indefinitely — not per-request caching — creating stale data, unbounded memory growth, and thread-safety bugs under concurrent writes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design / state | `static` cache shared process-wide | User A sees User B's catalog; stale data never expires |
| Concurrency | `Dictionary<>` without synchronization | Race conditions and corrupted state under load |
| Correctness | `OnResourceExecuted` stores null if lookup failed | NullReference or poisoned cache entries |
| Architecture | Resource filter misused for cross-request cache | Wrong lifetime — should be `IMemoryCache` with TTL or scoped service |

**Fix (priority order):**

1. For **per-request** reuse, store in `HttpContext.Items` only within the same request — remove static field; inject a scoped `ICatalogService` that memoizes once per scope.
2. For **cross-request** caching, use `IMemoryCache` or distributed cache with expiration and cache keys including tenant/user.
3. If short-circuiting with cached `IActionResult` is the goal, set `context.Result` in `OnResourceExecuting` when a valid cached result exists (resource filter pattern for short-circuit).

**Production takeaway:** Resource filters excel at per-request short-circuit and `HttpContext.Items` — static fields turn them into hidden singleton state bugs.

---

#### Q7. (P) A global action filter is registered in `Program.cs` and constructor-injects `AppDbContext`. The app starts in Development but throws `Cannot consume scoped service from singleton` in Production with scope validation enabled. Explain filter DI lifetime, how global filters are resolved, and the correct fix.

**Answer:** Global filters added via `options.Filters.Add<T>()` are resolved from DI when the filter runs; if the filter is registered as singleton (default for `TypeFilter` without scope) or cached at startup, injecting scoped `AppDbContext` violates lifetime rules — fix with `[ServiceFilter]` + scoped filter registration or `IServiceFilter` pattern.

- `AddControllers(options => options.Filters.Add<AuditActionFilter>())` resolves `AuditActionFilter` through DI per filter contract — scoped dependencies require the filter itself to be scoped per request.
- `ValidateScopes` in Production catches captive dependencies at runtime; Development may lazy-resolve until first request.
- **Fix 1:** Register `builder.Services.AddScoped<AuditActionFilter>()` and use `options.Filters.AddService<AuditActionFilter>()`.
- **Fix 2:** Inject `IServiceProvider` or `IDbContextFactory<AppDbContext>` and create a scope inside `OnActionExecutionAsync` — factory pattern for filters that must stay singleton.
- **Fix 3:** Use `TypeFilterAttribute` or `ServiceFilterAttribute` on controllers needing the filter instead of global registration if only some actions need DB access.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(o => o.Filters.AddService<AuditActionFilter>());
```

**Production takeaway:** Global filters look like middleware but participate in DI lifetimes — treating them as singletons with DbContext is one of the most common Production startup/request failures.

---

#### Q8. (D) Unhandled exceptions in a controller can be caught by an exception filter, `IExceptionHandler` middleware, or `UseExceptionHandler`. Compare scope (MVC-only vs entire pipeline), ordering, and when you would keep an exception filter vs centralizing everything in middleware.

**Answer:** Exception filters only see exceptions thrown from actions/filters in the MVC pipeline after routing; `IExceptionHandler` and exception middleware catch unhandled exceptions from the entire app including middleware, minimal APIs, and infrastructure — prefer centralized middleware/`IExceptionHandler` for uniform `ProblemDetails`.

- **Exception filter (`IExceptionFilter`):** MVC/Razor Pages only; runs before result execution; can mark exception handled and set `context.Result`; ignored by minimal APIs and middleware failures.
- **`IExceptionHandler` (.NET 8+):** registered in DI; invoked by exception-handler middleware; returns `ProblemDetails`; chain multiple handlers; `TryHandleAsync` return true stops propagation.
- **`UseExceptionHandler` / developer page:** outer safety net; re-executes pipeline to error endpoint or renders developer page in Development.
- **Ordering:** exception middleware should be early (often first after `Build`) to wrap downstream pipeline; exception filters run only if the exception reaches MVC and is not already handled.
- **Keep exception filters when:** converting specific MVC action failures to particular views (HTML) or when third-party MVC extensions require filter-level handling — otherwise migrate to global handler for APIs.

**Production takeaway:** API teams standardizing on `IExceptionHandler` avoid split-brain error shapes between controllers, minimal APIs, and middleware — exception filters become legacy MVC-only paths.
