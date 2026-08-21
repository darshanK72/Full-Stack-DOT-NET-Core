# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `06. ASP.NET Core MVC/11. Action Filters in MVC`

---

#### Q1. (R) Review this custom action filter registered globally on MVC controllers. Under load, thread-pool starvation appears and Kestrel queue depth grows. What is wrong, and when should this be `IAsyncActionFilter` instead of `IActionFilter`?

**Answer:** The filter blocks a request thread with sync-over-async (`GetAwaiter().GetResult()`) inside `OnActionExecuting`, which defeats ASP.NET Core's async model on a globally registered filter that runs on every MVC action — convert to `IAsyncActionFilter` and await the repository call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetAwaiter().GetResult()` on `GetBySlugAsync` | Thread-pool starvation under concurrent I/O |
| Design | Sync `IActionFilter` for database I/O | Wrong interface — blocks entire action pipeline stage |
| Correctness | `slug!` null-forgiving when route value missing | Possible `ArgumentNullException` or bad query |
| Scalability | Global filter on all MVC actions | Multiplies blocking calls across every controller hit |

**Fix (priority order):**

1. Implement `IAsyncActionFilter` and `await _tenants.GetBySlugAsync(slug, context.HttpContext.RequestAborted)` before `await next()`.
2. Guard missing `tenant` route value — set `context.Result = new NotFoundResult()` and return without calling the action.
3. Keep `AddScoped` + `AddService` registration (that part is correct).

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
{
    var slug = ctx.RouteData.Values["tenant"]?.ToString();
    if (string.IsNullOrEmpty(slug))
    {
        ctx.Result = new NotFoundResult();
        return;
    }
    ctx.HttpContext.Items["Tenant"] = await _tenants.GetBySlugAsync(slug, ctx.HttpContext.RequestAborted);
    await next();
}
```

**Production takeaway:** `IActionFilter` is fine for CPU-only checks; any filter that touches I/O on the hot path should be `IAsyncActionFilter` — especially global MVC filters.

---

#### Q2. (M) A controller has three filters on one action: a global `AuditFilter` (`Order = 100`), controller-level `[ValidateAntiForgeryToken]`, and action-level `[RequireRole("Admin")]`. Describe MVC filter **scope** (global → controller → action) and **order** within the same filter stage — which runs first within authorization filters, and can a lower-ordered authorization filter still short-circuit after a higher-ordered one passes?

**Answer:** MVC runs filters by **stage** (authorization → resource → action → exception → result), then by **scope** (global → controller → action) within each stage, then by `IOrderedFilter.Order` ascending — authorization filters run before action filters entirely, so `[RequireRole]` executes before `AuditFilter` or antiforgery action filters; multiple authorization filters all run in order and any one can set `context.Result` to short-circuit later stages.

- **Authorization stage first:** `[RequireRole("Admin")]` (authorization filter) runs before resource/action stages — antiforgery validation is an **authorization filter** (`ValidateAntiForgeryTokenAuthorizationFilter`), so it also runs in the authorization stage, not in action filters.
- **Scope within a stage:** global authorization filters → controller authorization filters → action authorization filters; same pattern for action filters (`AuditFilter` is an action filter — runs later).
- **`Order` property:** lower numbers run first within the same scope and filter type; default `Order = 0` when unspecified.
- **Short-circuit:** setting `context.Result` in an authorization filter skips the action and remaining action filters, but authorization filters with higher `Order` in the same stage may still run depending on pipeline construction — safest pattern is one authoritative auth filter with early `Order`.
- **`AuditFilter` at Order 100:** runs after default-ordered action filters unless they specify higher order.

**Production takeaway:** Teams often assume `[ValidateAntiForgeryToken]` is an action filter — it is authorization-stage, which is why invalid tokens reject before model binding and action execution.

---

#### Q3. (R) Review global filter registration and DI. Production enables scope validation; the first POST to `/Orders/Create` throws `InvalidOperationException: Cannot consume scoped service 'AppDbContext' from singleton`. Diagnose.

**Answer:** `options.Filters.Add<AuditActionFilter>()` resolves the filter through `ObjectFactory` as an effectively singleton filter instance while `AppDbContext` is scoped — register the filter as scoped and use `AddService`, or inject `IDbContextFactory<AppDbContext>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | `Filters.Add<T>()` without service registration | Filter resolved with captive scoped dependency |
| Runtime | Scope validation enabled in Production | First request throws instead of silent bug |
| Design | Synchronous audit write before action | Failed action still logged if not moved to `finally`/post-`next()` |
| Maintainability | Global filter hidden in `Program.cs` | Easy to miss in code review vs `[ServiceFilter]` on controllers |

**Fix (priority order):**

1. `builder.Services.AddScoped<AuditActionFilter>()` and `options.Filters.AddService<AuditActionFilter>()`.
2. Prefer auditing **after** `await next()` so only successful actions log, or use a try/finally pattern.
3. Alternative: inject `IDbContextFactory<AppDbContext>` if the filter must remain singleton-compatible.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
```

**Production takeaway:** Global MVC filters participate in DI lifetimes exactly like `[ServiceFilter]` — `Add<T>()` without scoped registration is a classic Production-only failure.

---

#### Q4. (D) Product wants to block unauthenticated traffic before a custom middleware that opens an expensive read replica connection on every request. A developer proposes `[Authorize]` on every MVC controller instead of moving auth earlier in the pipeline. Compare authorization-filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). Which seam fixes replica churn for MVC-only apps vs mixed MVC + minimal API hosts?

**Answer:** `[Authorize]` short-circuits at the **authorization filter** stage — after routing, endpoint selection, and all middleware registered before the MVC endpoint, including the replica middleware — so it does **not** stop replica connection churn; place authentication/authorization middleware **before** the expensive middleware, or split branches with `MapWhen`.

- **Filter short-circuit:** sets `context.Result` (401/403) — skips action and later MVC filters, but **middleware already executed** on the way in, including replica setup.
- **Middleware short-circuit:** omit `_next` before expensive work — request never reaches replica middleware; works for all endpoints on that branch.
- **MVC-only app:** still wrong if replica middleware is global — `[Authorize]` on every controller duplicates policy and still pays middleware cost; fix pipeline order: `UseAuthentication` → `UseAuthorization` → replica middleware (only if authenticated) or conditional middleware checking `context.User`.
- **Mixed MVC + minimal API:** controller `[Authorize]` leaves minimal API routes unprotected unless they also call `RequireAuthorization()`; middleware + endpoint metadata is the unified seam.
- See [Middleware Pipeline Q8](../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_ANSWERS.md): "Before routing / before binding" concerns belong in middleware, not filters.

**Production takeaway:** Authorization filters enforce **who may invoke an action**; they do not rewind middleware that already ran — pipeline order is the fix for replica churn.

---

#### Q5. (R) Review this exception filter deployed to Production alongside `app.UseExceptionHandler()`. Clients sometimes receive HTML error pages and sometimes JSON `ProblemDetails` for the same exception type; logs show duplicate stack traces. What is wrong?

**Answer:** The filter marks the exception handled **and then rethrows**, so MVC may emit JSON while exception middleware also processes the same fault — remove the rethrow, pick one strategy (filter for MVC-specific views or centralized `IExceptionHandler` for APIs), and never log sensitive `Exception.Message` in Production responses.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ExceptionHandled = true` then `throw` | Double handling — race between filter result and middleware |
| HTTP / API | Inconsistent error shape (HTML vs JSON) | Clients cannot parse errors reliably |
| Security | `Detail = context.Exception.Message` | Information disclosure in Production |
| Observability | Duplicate logging of same exception | Alert noise and inflated error rates |

**Fix (priority order):**

1. Remove `throw context.Exception` — if handled, return cleanly from `OnException`.
2. For JSON APIs, prefer `IExceptionHandler` / exception middleware only; remove the exception filter or limit it to HTML MVC controllers returning views.
3. Map known exceptions to safe `ProblemDetails` without inner exception messages; log full detail server-side only.

```csharp
public void OnException(ExceptionContext context)
{
    _logger.LogError(context.Exception, "Action {Action} failed", context.ActionDescriptor.DisplayName);
    context.ExceptionHandled = true;
    context.Result = new ObjectResult(new ProblemDetails { Status = 500, Title = "An error occurred." })
        { StatusCode = 500 };
}
```

**Production takeaway:** Exception filters are MVC/Razor-only hooks — pairing them with global exception middleware without a single owner produces split-brain error responses.

---

#### Q6. (R) Review this result filter meant to skip view rendering when a cached HTML fragment exists. QA reports the view engine still runs and `_Layout.cshtml` executes twice on cache hits. Diagnose.

**Answer:** Writing to the response in `OnResultExecuting` without assigning `context.Result` does not short-circuit result execution — the original `ViewResult` still renders; set `context.Result` to a short-circuiting result (e.g., `ContentResult`) or assign `context.Cancel = true` and replace the result.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| MVC pipeline | No `context.Result` assignment | View engine still executes — double output |
| HTTP | `WriteAsync` without clearing pipeline | Layout + cached HTML concatenated or corrupted |
| Async | Fire-and-forget `WriteAsync` not awaited | Incomplete writes under load |
| Caching | Path-only cache key | Personalized or varied responses collide |

**Fix (priority order):**

1. On cache hit: `context.Result = new ContentResult { Content = html, ContentType = "text/html" };` — optionally `context.Cancel = true` (when supported) to skip remaining result filters' execution of the original result.
2. Await response writes if using manual stream writing; prefer replacing `IActionResult`.
3. Include tenant/user/version in cache key; set cache headers explicitly.

```csharp
public void OnResultExecuting(ResultExecutingContext context)
{
    var key = BuildCacheKey(context.HttpContext);
    if (_cache.TryGetValue(key, out string? html))
        context.Result = new ContentResult { Content = html, ContentType = "text/html; charset=utf-8" };
}
```

**Production takeaway:** Result-filter short-circuit means **replace `context.Result`**, not write alongside it — the view engine only skips when the pipeline is told the result changed.

---

#### Q7. (P) You need an audit action filter on every MVC controller but not on `/api/*` minimal routes in the same app. Show the correct global registration pattern with `AddControllersWithViews`, filter `Order`, and how `[AllowAnonymous]` / `[IgnoreAntiforgeryToken]` interact with globally registered filters.

**Answer:** Register the audit filter globally via `AddControllersWithViews(options => options.Filters.AddService<AuditActionFilter>())`, map minimal APIs separately without MVC filter discovery, and use `[AllowAnonymous]` only for authorization — not to skip unrelated global action filters unless the audit filter explicitly checks `[AllowAnonymous]` or endpoint metadata.

- **MVC global registration:**

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllersWithViews(o => o.Filters.AddService<AuditActionFilter>());
builder.Services.AddEndpointsApiExplorer();
// Minimal APIs: app.MapGroup("/api/orders").MapGet(...).RequireAuthorization();
// — no MVC action filters run on minimal endpoints
```

- **`Order`:** set `AuditActionFilter : IAsyncActionFilter, IOrderedFilter` with `Order => 1000` to run after validation filters (`Order = 0` or negative for idempotency).
- **`[AllowAnonymous]`:** bypasses **authorization** requirements only; a global **action** filter still runs unless it checks `context.ActionDescriptor.EndpointMetadata` for `IAllowAnonymous` and no-ops.
- **`[IgnoreAntiforgeryToken]`:** adds `IgnoreAntiforgeryTokenAttribute` metadata so `ValidateAntiForgeryTokenAuthorizationFilter` skips validation — unrelated to audit action filters.
- **Selective MVC exclusion:** prefer `[ServiceFilter]` on a base `Controller` class instead of global registration, or tag controllers with `[Audit]` and register via convention.

**Production takeaway:** Global MVC filters never attach to minimal API endpoints — that separation is automatic; use global filters for cross-controller MVC concerns, middleware for cross-host concerns.

---

#### Q8. (R) A teammate registers a filter with `[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]` but the app fails at first request with `InvalidOperationException` about `IRateLimitStore`. Another controller uses `[ServiceFilter(typeof(RateLimitFilter))]` successfully. Explain filter factory resolution (`TypeFilterAttribute` vs `ServiceFilterAttribute`) and fix the broken registration.

**Answer:** `TypeFilterAttribute` constructs the filter via DI **plus** constructor arguments you supply — dependencies like `IRateLimitStore` must still be registered in DI; `ServiceFilterAttribute` resolves the **entire** filter from DI using the parameterless or DI-filled constructor registered in `Program.cs`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI | `IRateLimitStore` not registered | `TypeFilter` cannot resolve store at activation |
| Design | `Arguments = { 60 }` only supplies `perMinute` | First ctor param still needs DI services |
| Confusion | Copying `[ServiceFilter]` pattern onto `[TypeFilter]` | Works only when filter fully registered with all ctor params |
| API contract | Same filter, two registration styles | Inconsistent per-controller rate limits |

**Fix (priority order):**

1. Register `builder.Services.AddScoped<IRateLimitStore, RateLimitStore>()`.
2. Keep `[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]` — DI fills `store`, argument fills `perMinute`.
3. Or register `builder.Services.AddScoped<RateLimitFilter>()` and use `[ServiceFilter(typeof(RateLimitFilter))]` with options via `IOptions<RateLimitOptions>` instead of ctor int.

```csharp
builder.Services.AddScoped<IRateLimitStore, RateLimitStore>();
// TypeFilter: DI resolves IRateLimitStore, Arguments supplies perMinute=60
[TypeFilter(typeof(RateLimitFilter), Arguments = new object[] { 60 })]
public class ReportsController : Controller { }
```

**Production takeaway:** `TypeFilter` = **factory with extra ctor args**; `ServiceFilter` = **fully DI-managed instance** — both require dependencies in the service collection.

---

#### Q9. (R) An AJAX partial-update endpoint returns `400 Bad Request` with empty body after deploy. The action has `[ValidateAntiForgeryToken]`; the jQuery client POSTs JSON to `/Cart/AddItem` without a token header. Review the controller setup and client contract — what changed with `[AutoValidateAntiforgeryToken]`, and how do you fix AJAX without disabling CSRF?

**Answer:** `[AutoValidateAntiforgeryToken]` applies antiforgery validation to all unsafe HTTP methods on the controller — the JSON AJAX POST lacks `RequestVerificationToken` header/form field, so `ValidateAntiForgeryTokenAuthorizationFilter` short-circuits with 400 before the action runs; fix by sending the token from the page and validating header/form field, not by `[IgnoreAntiforgeryToken]` on the main endpoint.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | AJAX POST without antiforgery token | Correct 400 — CSRF protection working |
| API design | JSON body endpoint on MVC controller with auto antiforgery | API clients need token contract, not just JSON |
| Workaround smell | `[IgnoreAntiforgeryToken]` on production action | Opens CSRF unless replaced with other auth |
| Client | No token in `$.ajax` headers | Empty 400 body — client cannot distinguish validation vs CSRF |

**Fix (priority order):**

1. Render token in the page: `@Html.AntiForgeryToken()` or `<input name="__RequestVerificationToken" ...>` from `IAntiforgery.GetAndStoreTokens`.
2. Send header on AJAX POST:

```javascript
headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }
```

3. For pure JSON APIs, move to `ControllerBase` + minimal API with bearer auth instead of antiforgery; keep antiforgery for cookie-based MVC forms.
4. Reserve `[IgnoreAntiforgeryToken]` for webhooks with alternate signatures — not cart mutations.

**Production takeaway:** Antiforgery is an **authorization filter** — AJAX MVC endpoints must participate in the same token contract as full-page forms; `[AutoValidateAntiforgeryToken]` just removes per-action attribute boilerplate.

---

#### Q10. (R) Review this resource filter intended to memoize an expensive catalog lookup **once per HTTP request** across two action filters and the action. Instead, users intermittently see another tenant's catalog after scale-out to three pods. Diagnose lifetime and caching scope.

**Answer:** A `static` dictionary is process-wide, not per-request — under load it leaks catalog data across tenants and requests; use `HttpContext.Items` only (no static field) for per-request memoization, or `IMemoryCache`/distributed cache with tenant-scoped keys and TTL for cross-request caching.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design / state | `static` `_requestCache` | Cross-request, cross-tenant data leak |
| Concurrency | Unsynchronized `Dictionary<>` | Corrupted entries under parallel requests |
| Scale-out | In-memory static per pod | Inconsistent catalog between instances |
| Naming | Comment says "per request" but static persists | Misleading abstraction — security bug |

**Fix (priority order):**

1. Remove static field; store catalog only in `HttpContext.Items["Catalog"]` within `OnResourceExecuting`/`OnResourceExecuted` for the same request.
2. Inject scoped `ICatalogService` that memoizes on first call per request scope — resource filter optional.
3. For cross-request cache: `IMemoryCache` with key `tenantId:categoryId`, sliding expiration, and never share mutable DTOs without copy.

```csharp
public void OnResourceExecuting(ResourceExecutingContext context)
{
    if (context.HttpContext.Items.ContainsKey("Catalog")) return;
    // load once into Items — visible to action filters and action in same request
}
```

**Production takeaway:** Resource filters wrap the action pipeline — **`HttpContext.Items` is the per-request cache**; static fields are cross-request singletons regardless of filter stage.

---

#### Q11. (D) Your MVC app needs (a) per-action audit with route values and bound model metadata, (b) uniform 413 rejection for oversized uploads before model binding, and (c) a correlation ID on every response including static files. For each concern, choose middleware, authorization filter, action filter, resource filter, or result filter — and cite one reason the wrong layer fails.

**Answer:** (a) action filter, (b) middleware/Kestrel limits, (c) middleware — filters lack visibility into static-file middleware paths and cannot run before model binding for upload size.

| Concern | Layer | Why |
|---|---|---|
| (a) Per-action audit with route + model metadata | **Action filter** (`IAsyncActionFilter`) | Runs with `ActionExecutingContext.ActionArguments` and `ActionDescriptor` — middleware lacks bound model |
| (b) 413 before model binding | **Middleware** + `KestrelServerLimits.MaxRequestBodySize` | Action/resource filters run after routing and binding start — body may already be buffered |
| (c) Correlation ID on all responses incl. static files | **Middleware** (early) | MVC filters never run for `UseStaticFiles` short-circuit paths |

- Wrong layer for (a): middleware logs URL only — no action name or bound DTO type without reflection hacks.
- Wrong layer for (b): action filter checking size — too late; large upload already consumed memory.
- Wrong layer for (c): result filter — static files bypass MVC result pipeline entirely.

**Production takeaway:** Match the seam to the earliest point the needed context exists — see [Middleware Pipeline Q8](../../05.%20ASP.NET%20Core/03.%20Middleware%20Pipeline/KARAT_INTERVIEW_ANSWERS.md) for the same judgment framework.

---

#### Q12. (M) Two action filters both implement `IOrderedFilter`: `IdempotencyFilter` (`Order = -1000`, short-circuits duplicate POSTs) and `ModelStateValidationFilter` (`Order = 0`, returns `BadRequestObjectResult` when invalid). Walk through `OnActionExecutionAsync` / `OnActionExecuting` invocation order for `[ServiceFilter(typeof(IdempotencyFilter)), ServiceFilter(typeof(ModelStateValidationFilter))]` on a POST action — which runs first, and why does order matter for idempotency?

**Answer:** Lower `Order` runs first on the **executing** leg — `IdempotencyFilter` (`-1000`) runs before `ModelStateValidationFilter` (`0`); on the **executed** leg, order reverses (like middleware onion), so validation's `OnActionExecuted` runs before idempotency's.

- **Executing (before action):** sort by ascending `Order` → idempotency checks duplicate `Idempotency-Key` first; if duplicate, sets `context.Result` and skips validation and action entirely.
- **Why order matters:** if validation ran first (`Order 0` before `-1000`), invalid requests would consume idempotency keys or store partial state before rejection — wrong for POST retry semantics.
- **Executed (after action):** reverse order — validation post-action hooks run before idempotency commit/persist side effects.
- **Short-circuit:** first filter to set `context.Result` on executing leg prevents action invocation; later executing filters may still run depending on filter implementation — design idempotency filter to run earliest (`Order` large negative).
- **`IAsyncActionFilter` vs `IActionFilter`:** both participate in the same ordering; async filters interleave by order value, not by interface type.

```csharp
// Executing:  Idempotency (-1000) → ModelStateValidation (0) → action
// Executed:   ModelStateValidation (0) → Idempotency (-1000)
```

**Production takeaway:** Action filter `Order` controls **which guard runs first** — idempotency and auth-style guards belong at the lowest order values on the executing side.
