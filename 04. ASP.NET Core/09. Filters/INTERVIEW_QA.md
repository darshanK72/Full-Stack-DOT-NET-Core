# Filters — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 09. Filters](#chapter-09-filters)
  - [Q1. What are filters in ASP.NET Core MVC?](#chapter-09-filters-q1)
  - [Q2. What is the MVC filter pipeline execution order?](#chapter-09-filters-q2)
  - [Q3. What is an authorization filter?](#chapter-09-filters-q3)
  - [Q4. What is an action filter?](#chapter-09-filters-q4)
  - [Q5. What is a resource filter?](#chapter-09-filters-q5)
  - [Q6. What is a result filter?](#chapter-09-filters-q6)
  - [Q7. What is an exception filter?](#chapter-09-filters-q7)
  - [Q8. What is the difference between middleware and filters?](#chapter-09-filters-q8)
  - [Q9. When would you use a filter instead of middleware?](#chapter-09-filters-q9)
  - [Q10. How do you register a global filter?](#chapter-09-filters-q10)
  - [Q11. How do you apply a filter to a single action or controller?](#chapter-09-filters-q11)
  - [Q12. What is `IAsyncActionFilter`, and how does it differ from `I…](#chapter-09-filters-q12)
  - [Q13. How does `[Authorize]` relate to authorization filters?](#chapter-09-filters-q13)
  - [Q14. What is the difference between authentication middleware and…](#chapter-09-filters-q14)
  - [Q15. Do Minimal APIs use MVC filters?](#chapter-09-filters-q15)
  - [Q16. What are endpoint filters in Minimal APIs?](#chapter-09-filters-q16)
  - [Q17. How does DI work with filters?](#chapter-09-filters-q17)
  - [Q18. Can a filter short-circuit a request? How?](#chapter-09-filters-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 09. Filters

### Q1. What are filters in ASP.NET Core MVC? {#chapter-09-filters-q1}

What are filters in ASP.NET Core MVC?

**Answer:** Filters are attributes or DI-registered components that run before and after MVC action execution, providing cross-cutting logic at the controller/action level. ASP.NET Core 8 supports authorization, resource, action, exception, and result filters — distinct from middleware, which runs for the entire pipeline.

- Filters operate only after routing selects an MVC controller action (or Razor Page handler).
- They access MVC-specific context — `ActionDescriptor`, route values, bound arguments, and `IActionResult`.
- Register globally in `AddControllers(options => ...)`, via `[ServiceFilter]`, or `[TypeFilter]`.
- Minimal APIs use endpoint filters instead of MVC filters for the same cross-cutting concerns.

---

### Q2. What is the MVC filter pipeline execution order? {#chapter-09-filters-q2}

What is the MVC filter pipeline execution order?

**Answer:** MVC filters run in a fixed order: authorization filters, resource filters, action filters, exception filters (on failure), then result filters. Within each type, global filters run before controller filters, which run before action filters — `IOrderedFilter.Order` can fine-tune ordering.

- Authorization filters run first — fail-fast 401/403 before expensive work.
- Resource filters wrap the remainder of the pipeline including model binding and action execution.
- Action filters wrap the action method itself — `OnActionExecuting` before, `OnActionExecuted` after.
- Result filters wrap `IActionResult` execution (view rendering, JSON serialization).

---

### Q3. What is an authorization filter? {#chapter-09-filters-q3}

What is an authorization filter?

**Answer:** An authorization filter implements `IAuthorizationFilter` or `IAsyncAuthorizationFilter` and runs before the action to verify the caller is allowed to execute it. The `[Authorize]` attribute is implemented as an authorization filter that evaluates policies via `IAuthorizationService`.

- Set `context.Result` to `UnauthorizedResult` or `ForbidResult` to short-circuit before the action runs.
- Runs after authentication middleware populates `HttpContext.User`.
- Custom filters can enforce API keys or custom claims, but prefer policy-based authorization for maintainability.
- Use `IAsyncAuthorizationFilter` when authorization logic performs async I/O — never block with `.Result`.

---

### Q4. What is an action filter? {#chapter-09-filters-q4}

What is an action filter?

**Answer:** An action filter implements `IActionFilter` or `IAsyncActionFilter` and runs immediately before and after the action method executes. It is ideal for action-scoped concerns — audit logging, timing, mutating bound arguments, or validating models after binding.

- `OnActionExecuting` / before `await next()`: can set `context.Result` to skip the action.
- `OnActionExecuted` / after `await next()`: inspect outcome, exceptions, or elapsed time.
- Prefer `IAsyncActionFilter` when the filter performs I/O or async work.
- Action filters see bound parameters — middleware does not have this MVC context.

---

### Q5. What is a resource filter? {#chapter-09-filters-q5}

What is a resource filter?

**Answer:** A resource filter implements `IResourceFilter` or `IAsyncResourceFilter` and wraps execution of the entire remainder of the filter pipeline plus the action. It runs after authorization but before model binding and action filters on the way in, and after result execution on the way out.

- Use for request-scoped caching in `HttpContext.Items` or short-circuiting with a cached `IActionResult`.
- `OnResourceExecuting` can set `context.Result` to bypass the action entirely.
- Not intended for cross-request caching — use `IMemoryCache` with TTL for that.
- Runs earlier than action filters — suitable for decisions that skip model binding cost.

---

### Q6. What is a result filter? {#chapter-09-filters-q6}

What is a result filter?

**Answer:** A result filter implements `IResultFilter` or `IAsyncResultFilter` and runs before and after the `IActionResult` is executed — rendering a view, writing JSON, or returning a file. Use it to modify headers, wrap responses, or log final output status.

- `OnResultExecuting` runs before the result executes — can cancel or replace `context.Result`.
- `OnResultExecuted` runs after — inspect the response status code and log completion.
- Exception filters handle action exceptions; result filters handle result execution wrapping.
- For uniform response shaping across APIs, middleware or endpoint filters may be simpler.

---

### Q7. What is an exception filter? {#chapter-09-filters-q7}

What is an exception filter?

**Answer:** An exception filter implements `IExceptionFilter` or `IAsyncExceptionFilter` and runs when an unhandled exception occurs in the action or earlier MVC filter stages, before the result is committed. It can mark the exception handled and set `context.Result` to an error response.

- Only catches exceptions from the MVC pipeline — not middleware failures or minimal API exceptions.
- Runs before result execution — useful for converting domain exceptions to specific MVC views.
- ASP.NET Core 8 APIs prefer centralized `IExceptionHandler` middleware for uniform ProblemDetails.
- Exception filters do not replace logging — always log the full exception object server-side.

---

### Q8. What is the difference between middleware and filters? {#chapter-09-filters-q8}

What is the difference between middleware and filters?

**Answer:** Middleware runs for every request matching its branch in the pipeline, before or after routing, and applies to all endpoint types. Filters run only for MVC/Razor Page requests after an action is selected, with access to MVC-specific context like `ActionDescriptor` and bound arguments.

- Middleware order is global and explicit in `Program.cs` — filters follow MVC's fixed pipeline order.
- Middleware is the right layer for authentication, forwarded headers, rate limiting, and exception handling across all endpoints.
- Filters suit per-controller or per-action logic — audit trails, action timing, MVC-specific authorization checks.
- Minimal APIs and middleware-only pipelines never execute MVC filters.

---

### Q9. When would you use a filter instead of middleware? {#chapter-09-filters-q9}

When would you use a filter instead of middleware?

**Answer:** Use a filter when the logic needs MVC context — action name, bound parameters, or controller-level policies — or when the behavior should apply only to specific controllers/actions rather than every request. Use middleware for transport-wide concerns that must run before routing or across minimal APIs and MVC alike.

- Action-specific audit logging with access to bound DTOs → action filter.
- Per-controller authorization beyond standard `[Authorize]` policies → authorization filter.
- Rejecting requests before expensive DB middleware on all paths including health checks → middleware.
- API key validation on all routes including minimal APIs → middleware, not controller filters.

---

### Q10. How do you register a global filter? {#chapter-09-filters-q10}

How do you register a global filter?

**Answer:** Add the filter type in `AddControllers` options or register it as a scoped service and use `AddService`. Global filters apply to every controller action unless excluded.

```csharp
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddControllers(options =>
    options.Filters.AddService<AuditActionFilter>());
```

- `options.Filters.Add<AuditActionFilter>()` resolves the filter from DI when needed.
- Global filters with scoped dependencies must use `AddService<T>()` with `AddScoped<T>()`.
- `[AllowAnonymous]` bypasses authorization filters but not other filter types.
- Order global filters with `IOrderedFilter.Order` when sequence matters.

---

### Q11. How do you apply a filter to a single action or controller? {#chapter-09-filters-q11}

How do you apply a filter to a single action or controller?

**Answer:** Apply filter attributes directly on the controller class or action method — `[ServiceFilter(typeof(AuditActionFilter))]`, `[TypeFilter(typeof(AuditActionFilter))]`, or built-in attributes like `[Authorize]`. Controller-level filters inherit to all actions unless overridden.

- `[ServiceFilter]` resolves the filter from DI — supports constructor injection with proper lifetimes.
- `[TypeFilter]` instantiates the filter with specified constructor arguments.
- Multiple filters on one action run in declaration order within the same filter stage.
- Action-level `[Authorize(Roles = "Admin")]` overrides or narrows controller-level authorization.

---

### Q12. What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`? {#chapter-09-filters-q12}

What is `IAsyncActionFilter`, and how does it differ from `IActionFilter`?

**Answer:** `IAsyncActionFilter` defines a single `OnActionExecutionAsync` method with an `ActionExecutionDelegate next`, enabling async/await around action execution. `IActionFilter` uses synchronous `OnActionExecuting` and `OnActionExecuted` callbacks — blocking async I/O in sync filters causes thread-pool starvation.

- Async filter: `var executed = await next();` then inspect `executed.Result` or `executed.Exception`.
- Sync filter: implement both `OnActionExecuting` and `OnActionExecuted` — no async support.
- Always prefer `IAsyncActionFilter` when the filter awaits databases, HTTP calls, or file I/O.
- Never use `.Result` or `.Wait()` on tasks inside synchronous filter methods.

---

### Q13. How does `[Authorize]` relate to authorization filters? {#chapter-09-filters-q13}

How does `[Authorize]` relate to authorization filters?

**Answer:** `[Authorize]` is implemented as an authorization filter (`AuthorizeFilter`) that evaluates the configured authorization policy against `HttpContext.User` via `IAuthorizationService`. It sets `context.Result` to 401 or 403 when the policy fails, before the action or model binding proceeds.

- Policy names, roles, and schemes come from the attribute properties — `[Authorize(Policy = "CanEdit")]`.
- `[AllowAnonymous]` on an action skips authorization filter enforcement for that endpoint.
- The same policy system backs both `[Authorize]` and minimal API `.RequireAuthorization()`.
- Authentication must run first via `UseAuthentication()` — filters authorize an already populated principal.

---

### Q14. What is the difference between authentication middleware and authorization filters? {#chapter-09-filters-q14}

What is the difference between authentication middleware and authorization filters?

**Answer:** Authentication middleware (`UseAuthentication()`) validates credentials and constructs `HttpContext.User` — it runs for all requests before authorization. Authorization filters run later in the MVC pipeline and decide whether the authenticated (or anonymous) user may execute a specific action.

- Authentication answers "who is this caller?" — JWT, cookies, API keys via authentication handlers.
- Authorization answers "may this caller perform this action?" — roles, policies, claims.
- `UseAuthorization()` middleware enforces endpoint metadata including minimal API routes.
- Authorization filters are the MVC integration point for `[Authorize]` on controllers — both use the same policy infrastructure.

---

### Q15. Do Minimal APIs use MVC filters? {#chapter-09-filters-q15}

Do Minimal APIs use MVC filters?

**Answer:** No — minimal API endpoints do not execute the MVC filter pipeline (`IActionFilter`, `IAuthorizationFilter`, etc.). Cross-cutting logic for minimal APIs uses endpoint filters (`IEndpointFilter`), authorization middleware, or custom middleware instead.

- `[Authorize]` on controllers does not protect minimal API routes registered separately.
- Apply `.RequireAuthorization()` on minimal API endpoints or groups for policy enforcement.
- Use `.AddEndpointFilter<T>()` for validation, logging, and timing around minimal handlers.
- Mixing controllers and minimal APIs requires configuring both filter/middleware and endpoint metadata consistently.

---

### Q16. What are endpoint filters in Minimal APIs? {#chapter-09-filters-q16}

What are endpoint filters in Minimal APIs?

**Answer:** Endpoint filters implement `IEndpointFilter` and wrap minimal API route handlers — the closest equivalent to MVC action filters. Register with `.AddEndpointFilter<T>()` on a route or group to run logic before and after the handler delegate.

- Signature: `InvokeAsync(HttpContext, EndpointFilterInvocationContext next)`.
- Can validate arguments in `EndpointFilterInvocationContext.Arguments` before the handler runs.
- Return `Results.Problem()` or other `IResult` to short-circuit without calling the handler.
- Group-level filters apply to every endpoint in a `MapGroup` — useful for shared validation or timing.

---

### Q17. How does DI work with filters? {#chapter-09-filters-q17}

How does DI work with filters?

**Answer:** Filters are resolved from the DI container when registered via `[ServiceFilter]`, `AddService<T>()`, or `TypeFilter`. Constructor injection works, but filter lifetimes must align with their dependencies — scoped filters for scoped services like `DbContext`.

- Register: `builder.Services.AddScoped<MyActionFilter>()` then `[ServiceFilter(typeof(MyActionFilter))]`.
- Global `options.Filters.Add<MyActionFilter>()` with scoped dependencies requires `AddService<MyActionFilter>()`.
- Injecting scoped `DbContext` into a singleton-cached filter causes captive dependency errors in Production with `ValidateScopes`.
- Use `IServiceScopeFactory` or `IDbContextFactory<T>` inside singleton filters when a scope per invocation is needed.

---

### Q18. Can a filter short-circuit a request? How? {#chapter-09-filters-q18}

Can a filter short-circuit a request? How?

**Answer:** Yes — filters short-circuit by assigning `context.Result` to an `IActionResult` (authorization, resource, action filters) or returning an `IResult` without calling `next()` (endpoint filters). Subsequent filters and the action are skipped once a result is set.

- Authorization filter: `context.Result = new UnauthorizedResult();` before the action runs.
- Action filter: set `context.Result` in `OnActionExecuting` or before `await next()` in async filters.
- Resource filter: set `context.Result` in `OnResourceExecuting` to skip model binding and the action entirely.
- Endpoint filter: return `Results.BadRequest(...)` without invoking `await next()` to skip the handler.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A teammate says "we'll put auth in an action filter instead of middleware." Walk through the MVC filter execution order — authorization filter, resource filter, action filter, exception filter, result filter — and explain which filter type owns authentication vs authorization vs action-specific validation.

---

**Answer:**

**Answer:** MVC runs filters in a fixed pipeline around the action: authorization filters first (fail-fast on access), then resource filters (before/after resource execution), action filters (around the action method), exception filters (on unhandled exceptions from earlier stages), and result filters (around `IActionResult` execution). Authentication belongs in middleware (`UseAuthentication`); authorization belongs in authorization filters or `[Authorize]`; action-specific validation belongs in action filters.

- **Authorization filters** (`IAuthorizationFilter`, `[Authorize]`) run before model binding completes expensive work in some setups and set `context.Result` to short-circuit (401/403) before the action executes.
- **Resource filters** (`IResourceFilter`) wrap the entire remainder of the filter pipeline plus action execution — useful for request-scoped caching of lookup data or short-circuiting if a cached `IActionResult` already exists.
- **Action filters** (`IActionFilter` / `IAsyncActionFilter`) run immediately before and after the action — ideal for validating bound models, timing, or mutating `ActionExecutingContext`.
- **Exception filters** run only when an exception escapes the action/resource stages but before the result is committed — they do not catch middleware failures or exceptions in result execution unless configured separately.
- **Result filters** wrap execution of `IActionResult` (view rendering, JSON serialization).

**Production takeaway:** Putting authentication in an action filter runs too late for non-MVC endpoints and duplicates what `UseAuthentication`/`UseAuthorization` middleware already standardizes — filters are for MVC/action-scoped concerns after routing.

---

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

*(Assume the filter is registered globally via `AddControllers(options => options.Filters.Add<ApiKeyAuthFilter>())`.)*

---

**Answer:**

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

---

#### Q3. (P) Implement cross-cutting request timing and audit logging around controller actions using `IAsyncActionFilter`. What runs in `OnActionExecutionAsync` before vs after `await next()`, and what can you still change at each stage?

---

**Answer:**

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

---

#### Q4. (D) Your team needs to reject requests without a valid API key before routing reaches expensive database middleware. Another developer wants an `IActionFilter` on every controller. Compare filter short-circuit (`context.Result = …`) vs middleware short-circuit (no `_next`). When is each the right seam?

---

**Answer:**

**Answer:** Middleware short-circuit rejects requests before routing and endpoint selection — saving work for non-MVC paths and global gates; filter short-circuit only runs after MVC has selected a controller action, so expensive upstream middleware already ran.

- **Middleware** (`RequestDelegate` chain): runs for all requests matching its branch; skip `_next` to return 401/429 immediately; required for concerns before `UseRouting` (forwarded headers, global API key on all paths including minimal APIs and health endpoints if desired).
- **Filter short-circuit:** sets `context.Result` (e.g., `UnauthorizedResult`) so the action and later filters skip execution, but routing, model binding, and earlier middleware already executed.
- **Filter advantage:** access to `ActionDescriptor`, `[Authorize]` integration, per-controller policies, and `IAsyncActionFilter` with action arguments.
- **Middleware advantage:** uniform behavior for MVC + minimal APIs; runs before DI scope for unrelated endpoints; pairs with endpoint routing `MapWhen` for path-specific branches.

**Production takeaway:** "Reject before DB middleware" is a pipeline-order problem — action filters are the wrong layer; place API-key middleware before the expensive middleware, not on every controller.

---

---

#### Q5. (M) `[Authorize(Roles = "Admin")]` is implemented as an authorization filter. How does that differ from calling `app.UseAuthentication()` / `UseAuthorization()` middleware, and what happens if authorization middleware runs but no authorization filter is reached (e.g., minimal API endpoint)?

---

**Answer:**

**Answer:** Middleware authentication populates `HttpContext.User` from cookies, JWT, or schemes; middleware authorization enforces policies via endpoint metadata (`RequireAuthorization()`). MVC `[Authorize]` adds an authorization filter that checks the same policy system but only on controller actions — minimal APIs rely on endpoint metadata, not MVC filters.

- `UseAuthentication()` runs authentication handlers and sets `ClaimsPrincipal` on `HttpContext.User`.
- `UseAuthorization()` evaluates `[Authorize]`, `[AllowAnonymous]`, and `RequireAuthorization()` metadata attached to endpoints — including minimal APIs and Razor Pages.
- MVC **authorization filters** are one integration point for `[Authorize]` on controllers; they call into `IAuthorizationService` with the same policies as middleware.
- A minimal API with `.RequireAuthorization("Admin")` never runs MVC authorization filters — only authorization middleware after routing.
- If authorization middleware is missing, `[Authorize]` on controllers may not enforce correctly; filters alone cannot authenticate — they only authorize an already-populated `User`.

**Production takeaway:** Teams mixing minimal APIs and controllers must configure authorization at the endpoint/middleware layer — sprinkling authorization filters on controllers leaves minimal API routes unprotected.

---

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

---

**Answer:**

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

---

#### Q7. (P) A global action filter is registered in `Program.cs` and constructor-injects `AppDbContext`. The app starts in Development but throws `Cannot consume scoped service from singleton` in Production with scope validation enabled. Explain filter DI lifetime, how global filters are resolved, and the correct fix.

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddDbContext<AppDbContext>();
```

---

**Answer:**

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

---

#### Q8. (D) Unhandled exceptions in a controller can be caught by an exception filter, `IExceptionHandler` middleware, or `UseExceptionHandler`. Compare scope (MVC-only vs entire pipeline), ordering, and when you would keep an exception filter vs centralizing everything in middleware.



**Answer:**

**Answer:** Exception filters only see exceptions thrown from actions/filters in the MVC pipeline after routing; `IExceptionHandler` and exception middleware catch unhandled exceptions from the entire app including middleware, minimal APIs, and infrastructure — prefer centralized middleware/`IExceptionHandler` for uniform `ProblemDetails`.

- **Exception filter (`IExceptionFilter`):** MVC/Razor Pages only; runs before result execution; can mark exception handled and set `context.Result`; ignored by minimal APIs and middleware failures.
- **`IExceptionHandler` (.NET 8+):** registered in DI; invoked by exception-handler middleware; returns `ProblemDetails`; chain multiple handlers; `TryHandleAsync` return true stops propagation.
- **`UseExceptionHandler` / developer page:** outer safety net; re-executes pipeline to error endpoint or renders developer page in Development.
- **Ordering:** exception middleware should be early (often first after `Build`) to wrap downstream pipeline; exception filters run only if the exception reaches MVC and is not already handled.
- **Keep exception filters when:** converting specific MVC action failures to particular views (HTML) or when third-party MVC extensions require filter-level handling — otherwise migrate to global handler for APIs.

**Production takeaway:** API teams standardizing on `IExceptionHandler` avoid split-brain error shapes between controllers, minimal APIs, and middleware — exception filters become legacy MVC-only paths.

---
