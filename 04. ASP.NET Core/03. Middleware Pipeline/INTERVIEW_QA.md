# Middleware Pipeline — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is middleware in ASP.NET Core?](#q1-what-is-middleware-in-aspnet-core)
2. [Q2. How does the middleware pipeline process an HTTP request?](#q2-how-does-the-middleware-pipeline-process-an-http-request)
3. [Q3. What is a `RequestDelegate`?](#q3-what-is-a-requestdelegate)
4. [Q4. What does calling `_next(context)` do in custom middleware?](#q4-what-does-calling-_nextcontext-do-in-custom-middleware)
5. [Q5. What happens when middleware returns without calling `_next`?](#q5-what-happens-when-middleware-returns-without-calling-_next)
6. [Q6. Why does middleware order matter?](#q6-why-does-middleware-order-matter)
7. [Q7. What is the recommended order for routing, authentication, and authorization middleware?](#q7-what-is-the-recommended-order-for-routing-authentication-and-authorization-middleware)
8. [Q8. What is the difference between `Use`, `Run`, and `Map`?](#q8-what-is-the-difference-between-use-run-and-map)
9. [Q9. What does `Map("/path", ...)` do to the pipeline?](#q9-what-does-mappath-do-to-the-pipeline)
10. [Q10. What is `MapWhen`, and when would you use it?](#q10-what-is-mapwhen-and-when-would-you-use-it)
11. [Q11. How do you register custom middleware in `Program.cs`?](#q11-how-do-you-register-custom-middleware-in-programcs)
12. [Q12. What is the difference between middleware and MVC filters?](#q12-what-is-the-difference-between-middleware-and-mvc-filters)
13. [Q13. Where should exception-handling middleware be placed in the pipeline?](#q13-where-should-exception-handling-middleware-be-placed-in-the-pipeline)
14. [Q14. What is `UseForwardedHeaders`, and why must it run early?](#q14-what-is-useforwardedheaders-and-why-must-it-run-early)
15. [Q15. What is built-in rate limiting middleware, and where does it belong in the pipeline?](#q15-what-is-built-in-rate-limiting-middleware-and-where-does-it-belong-in-the-pipeline)
16. [Q16. What does "short-circuiting the pipeline" mean?](#q16-what-does-short-circuiting-the-pipeline-mean)
17. [Q17. How does middleware differ from an endpoint filter?](#q17-how-does-middleware-differ-from-an-endpoint-filter)
18. [Q18. Can middleware access DI-registered services? How?](#q18-can-middleware-access-di-registered-services-how)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is middleware in ASP.NET Core?

What is middleware in ASP.NET Core?

**Answer:** Middleware is a component in the HTTP pipeline that receives an `HttpContext` and either passes control to the next component via `_next(context)` or terminates the request. Each middleware can execute logic before and after the next component runs.

- Built-in middleware includes exception handling, HTTPS redirection, static files, routing, authentication, and authorization.
- Custom middleware is a class with `RequestDelegate _next` in its constructor and an `Invoke` or `InvokeAsync` method.
- Middleware is registered in order in `Program.cs`; the first registered runs first on the inbound request.
- ASP.NET Core 8 ships additional middleware such as rate limiting (`UseRateLimiter`).

---

## Q2. How does the middleware pipeline process an HTTP request?

How does the middleware pipeline process an HTTP request?

**Answer:** Kestrel hands the request to the first middleware's delegate; each component may inspect or modify the request, call the next delegate, then inspect or modify the response on the way back. The chain continues until an endpoint executes or middleware short-circuits without calling next.

- The pipeline is a linked list of `RequestDelegate` functions built at startup.
- Endpoint middleware runs the matched route handler as the innermost logic.
- Response status and headers can be set by any middleware until the response starts streaming.
- Async middleware should return `Task` from `InvokeAsync` to avoid thread pool blocking under load.

---

## Q3. What is a `RequestDelegate`?

What is a `RequestDelegate`?

**Answer:** A `RequestDelegate` is a delegate type — `Task RequestDelegate(HttpContext context)` — representing the signature of middleware and the pipeline entry point. Each middleware holds a reference to the next `RequestDelegate` in the chain.

- The terminal delegate after all `Use` calls either runs the endpoint or returns 404.
- `app.Run(async context => { ... })` replaces the remaining pipeline with a single terminal delegate.
- Custom middleware invokes `_next(context)` to call the next delegate in the chain.
- Understanding `RequestDelegate` clarifies how `Use`, `Run`, and `Map` compose the pipeline.

---

## Q4. What does calling `_next(context)` do in custom middleware?

What does calling `_next(context)` do in custom middleware?

**Answer:** Calling `_next(context)` passes the current `HttpContext` to the next middleware in the pipeline, allowing downstream components and ultimately the endpoint to run. Code after `await _next(context)` executes during the response phase after inner middleware returns.

- Omitting `await _next(context)` prevents later middleware and endpoints from running — a short-circuit.
- Exceptions thrown after `_next` can still be caught by outer exception-handling middleware if the response has not started.
- Middleware often branches: call `_next` only when a condition passes (e.g., API key valid).
- Always pass the same `HttpContext` instance unless intentionally replacing it (rare).

---

## Q5. What happens when middleware returns without calling `_next`?

What happens when middleware returns without calling `_next`?

**Answer:** The pipeline short-circuits — no subsequent middleware or endpoint runs for that request. The current middleware is responsible for setting status code, headers, and body before returning.

- Common for authentication failures returning 401, rate limiting returning 429, or custom guards.
- If no response is written, clients may see empty or error responses depending on server defaults.
- Short-circuiting skips authorization and endpoint logic — useful for health checks handled early.
- Over-short-circuiting (buggy middleware) causes mysterious 404/401 for routes that should exist downstream.

---

## Q6. Why does middleware order matter?

Why does middleware order matter?

**Answer:** Middleware runs in registration order inbound and reverse order outbound; each component only sees requests that prior middleware forwarded with `_next`. Wrong order breaks routing, auth, exception handling, and static file serving.

- Exception handling should wrap outermost (registered early) to catch exceptions from inner middleware.
- `UseRouting` before `UseAuthentication` and `UseAuthorization` ensures endpoint metadata is available for auth.
- `UseStaticFiles` before routing can serve files without hitting controllers; after routing allows endpoint precedence rules.
- `UseForwardedHeaders` must run before middleware that reads scheme or client IP.

---

## Q7. What is the recommended order for routing, authentication, and authorization middleware?

What is the recommended order for routing, authentication, and authorization middleware?

**Answer:** Register `UseRouting` (or rely on implicit routing in minimal templates), then `UseAuthentication`, then `UseAuthorization`, then endpoint mapping (`MapControllers`, etc.). In ASP.NET Core 6+ minimal hosting, the template order is `UseAuthentication`, `UseAuthorization`, then `Map*` after routing middleware.

- Authentication populates `HttpContext.User` from cookies, JWT, or other handlers.
- Authorization evaluates policies against the matched endpoint's `[Authorize]` metadata or Minimal API `.RequireAuthorization()`.
- Placing auth before routing (older mistake) breaks endpoint-aware authorization in modern apps.
- CORS middleware (`UseCors`) has its own placement rules — typically after routing, before auth.

---

## Q8. What is the difference between `Use`, `Run`, and `Map`?

What is the difference between `Use`, `Run`, and `Map`?

**Answer:** `Use` adds middleware that can call the next component; `Run` adds terminal middleware that never calls next and replaces the remainder of the branch; `Map` branches the pipeline by path prefix and builds a sub-pipeline for matching requests.

- `app.Use(async (context, next) => { ... await next(); })` is inline middleware with next.
- `app.Run(async context => { await context.Response.WriteAsync("Done"); })` terminates — nothing after it runs for that branch.
- `app.Map("/admin", adminApp => { adminApp.Use... })` only runs the sub-pipeline for paths starting with `/admin`.
- `MapWhen` branches on arbitrary predicates instead of path prefix alone.

---

## Q9. What does `Map("/path", ...)` do to the pipeline?

What does `Map("/path", ...)` do to the pipeline?

**Answer:** `Map` creates a pipeline branch — requests with paths matching the prefix execute only the middleware registered inside the branch; non-matching requests skip the branch entirely and continue on the main pipeline.

- Matching is prefix-based by default: `/api` matches `/api/users` and `/api`.
- The branch can have its own terminal middleware or call `MapControllers` for isolated admin APIs.
- Each branch builds a separate middleware chain linked at the map point.
- Useful for serving different auth or rate limits on `/api` vs `/internal` paths.

---

## Q10. What is `MapWhen`, and when would you use it?

What is `MapWhen`, and when would you use it?

**Answer:** `MapWhen` branches the pipeline based on a custom predicate on `HttpContext`, not just path prefix. Use it when branching logic depends on headers, query strings, or content type rather than URL path alone.

- Example: run special middleware when `context.Request.Headers.ContainsKey("X-Debug")`.
- Non-matching requests bypass the branch without executing its middleware.
- Combines with `UseWhen` for conditional middleware on the main pipeline without full branching.
- Prefer `Map` for path-based splits — `MapWhen` for conditional cross-cutting behavior.

---

## Q11. How do you register custom middleware in `Program.cs`?

How do you register custom middleware in `Program.cs`?

**Answer:** Use `app.UseMiddleware<MyMiddleware>()` where the class has a constructor accepting `RequestDelegate` and optional DI services, plus `InvokeAsync(HttpContext)`. Alternatively use inline `app.Use(async (context, next) => { ... })`.

- `UseMiddleware<T>` resolves `T` from DI per application lifetime with `RequestDelegate` injected.
- Convention-based class name `MyMiddleware` with method `InvokeAsync` or `Invoke` is required.
- Register order relative to other middleware determines execution order.
- Extension method `app.UseCustomMiddleware()` wrapping `UseMiddleware` improves readability in larger apps.

---

## Q12. What is the difference between middleware and MVC filters?

What is the difference between middleware and MVC filters?

**Answer:** Middleware runs for every request that reaches it in the pipeline, before MVC selects an action. MVC filters run only within the MVC pipeline around controller actions, Razor pages, or view execution — after routing has chosen an MVC endpoint.

- Middleware is global unless branched with `Map`; filters can be scoped to controllers, actions, or globally registered in MVC.
- Authentication middleware establishes identity; authorization filters can enforce roles on specific actions.
- Exception middleware catches all unhandled exceptions; exception filters only see MVC action exceptions.
- Minimal APIs use endpoint filters instead of MVC filters — middleware still applies to Minimal API requests.

---

## Q13. Where should exception-handling middleware be placed in the pipeline?

Where should exception-handling middleware be placed in the pipeline?

**Answer:** Register exception-handling middleware early — among the first `Use` calls — so it wraps subsequent middleware and endpoints and can catch their exceptions. `UseExceptionHandler` or `UseDeveloperExceptionPage` belong near the top of the pipeline configuration.

- Outer placement ensures exceptions from auth, routing, and controllers are handled consistently.
- Developer exception page should only run in Development; production uses `UseExceptionHandler` with ProblemDetails.
- Middleware registered after exception handler will not have its exceptions caught by that handler.
- Response must not have started before exception middleware writes an alternate response body.

---

## Q14. What is `UseForwardedHeaders`, and why must it run early?

What is `UseForwardedHeaders`, and why must it run early?

**Answer:** `UseForwardedHeaders` reads `X-Forwarded-For`, `X-Forwarded-Proto`, and related headers from a reverse proxy and updates `HttpContext.Connection.RemoteIpAddress` and `Request.Scheme` accordingly. It must run before middleware that redirects to HTTPS, generates absolute URLs, or logs client IPs.

- Without it, apps behind nginx or IIS ARR see the proxy IP and `http` scheme.
- Configure `ForwardedHeadersOptions` to trust known proxy IP ranges — trusting all proxies is a security risk.
- Known networks and proxies must be explicitly configured in production.
- ASP.NET Core 8 still requires explicit `app.UseForwardedHeaders()` — it is not enabled by default.

---

## Q15. What is built-in rate limiting middleware, and where does it belong in the pipeline?

What is built-in rate limiting middleware, and where does it belong in the pipeline?

**Answer:** ASP.NET Core 7+ includes rate limiting via `AddRateLimiter` services and `UseRateLimiter` middleware, enforcing fixed-window, sliding-window, token-bucket, or concurrency limits per partition key. Place `UseRateLimiter` after routing (so policies can use endpoint metadata) and before endpoints execute — commonly after `UseAuthentication` if limits vary by user.

- Policies are registered with `builder.Services.AddRateLimiter(options => ...)` in ASP.NET Core 8.
- Rejected requests receive 429 status by default with optional `Retry-After` headers.
- Partition keys can be IP, user identity, or API key header values.
- Rate limiting at the app layer complements edge rate limiting on nginx or API gateways.

---

## Q16. What does "short-circuiting the pipeline" mean?

What does "short-circuiting the pipeline" mean?

**Answer:** Short-circuiting means a middleware completes the response without invoking `_next`, so later middleware and the endpoint never run for that request. It is intentional for auth failures, cached responses, or early returns.

- Differs from an endpoint completing normally after all prior middleware called `_next`.
- Static file middleware short-circuits when a file is found and served.
- Accidental short-circuit bugs occur when middleware forgets to call `_next` for valid requests.
- Short-circuit middleware must not call `_next` after writing the response — double execution causes errors.

---

## Q17. How does middleware differ from an endpoint filter?

How does middleware differ from an endpoint filter?

**Answer:** Middleware operates globally on the HTTP pipeline before endpoint invocation is finalized; endpoint filters (Minimal APIs) run immediately around a specific route handler after the endpoint is matched. Middleware cannot easily access route-handler-specific metadata without routing; endpoint filters are per-endpoint and support typed arguments.

- Endpoint filters implement `IEndpointFilter` and chain like middleware but only for one Minimal API route or group.
- MVC action filters are the controller equivalent — scoped to MVC actions, not raw middleware.
- Use middleware for cross-cutting concerns affecting many unrelated endpoints (logging, correlation IDs).
- Use endpoint filters for validation, auditing, or transformation tied to specific Minimal API handlers.

---

## Q18. Can middleware access DI-registered services? How?

Can middleware access DI-registered services? How?

**Answer:** Yes — middleware activated via `UseMiddleware<T>()` can inject any service registered in DI through its constructor, except scoped services should not be injected into singleton middleware constructors. Per-request scoped services are obtained from `HttpContext.RequestServices` inside `InvokeAsync`.

- `RequestDelegate` is always injected as the first constructor parameter for convention-based middleware.
- Injecting `DbContext` into middleware constructor causes captive dependency if middleware is singleton.
- Resolve scoped services with `context.RequestServices.GetRequiredService<T>()` within `InvokeAsync`.
- Inline lambda middleware can use constructor injection only via `UseMiddleware` or factory patterns.

---

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

#### Q1. (P) How do you limit incoming client requests within a time window using ASP.NET Core's built-in rate limiting middleware? What must you register in services, how do you attach policies to endpoints, and where should `UseRateLimiter()` sit relative to routing and authentication?

---

**Answer:**

**Answer:** Register policies with `builder.Services.AddRateLimiter(...)`, call `app.UseRateLimiter()` after routing (and typically after authentication when limits are per-user), and attach policies globally, via `[EnableRateLimiting("policy")]`, or `RequireRateLimiting` on minimal routes.

- Define policies — e.g., `AddFixedWindowLimiter("api", o => { o.Window = TimeSpan.FromMinutes(1); o.PermitLimit = 100; })`, or sliding/token-bucket for burst control.
- Rejected requests return **429 Too Many Requests**; configure `OnRejected` to log partition key and emit `Retry-After`.
- **Order:** `UseRouting()` → `UseAuthentication()` (if limits use claims) → `UseRateLimiter()` → `MapControllers()` / minimal endpoints.
- Anonymous abuse protection can use IP partition keys earlier; authenticated per-user limits need `User` populated first.
- Complement edge proxy rate limits — in-app limits enable per-route and per-identity granularity.

```csharp
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("api", w => { w.Window = TimeSpan.FromMinutes(1); w.PermitLimit = 60; });
});
app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("api");
```

**Production takeaway:** Rate limiting is middleware + service registration — wrong order means wrong partition keys or limits applied before the endpoint is known.

---

---

#### Q2. (R) Review this API key middleware. What is wrong with behavior, headers, and security — and what happens to downstream middleware when the key is missing?

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("X-Api-Key"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await _next(context);
    }
}
```

*(Registration order is correct; focus on response completion, validation, and client contract.)*

---

**Answer:**

**Answer:** The middleware correctly skips `_next` on failure (downstream never runs) but produces an incomplete 401 — no body, weak header validation — so clients and gateways cannot parse errors and empty keys bypass auth.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP / API | 401 with no body or `Content-Type` | API clients receive empty responses |
| Runtime / logic | `ContainsKey` only — empty header passes | Auth bypass with `X-Api-Key: ` |
| Security | No comparison to configured secret; not constant-time | Key validation is cosmetic |
| Observability | No logging or `ProblemDetails` | Cannot audit rejected calls |

**Fix (priority order):**

1. Use `TryGetValue` and validate against `IOptions<ApiKeyOptions>` or secret store.
2. Write `ProblemDetails` JSON and set status before return.
3. Log correlation id; avoid distinguishing missing vs invalid if policy requires.

```csharp
if (!context.Request.Headers.TryGetValue("X-Api-Key", out var key)
    || !_validator.IsValid(key))
{
    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = 401, Title = "Unauthorized", Detail = "Invalid or missing API key."
    });
    return;
}
await _next(context);
```

**Production takeaway:** Short-circuit without `_next` is correct for 401; **incomplete response** is the production bug.

---

---

#### Q3. (M) An authentication middleware sets `Response.StatusCode = 401` and returns without calling `_next`. Trace one request through the pipeline — which components still run, which are skipped, and why is omitting `_next` intentional rather than a bug?

---

**Answer:**

**Answer:** Middleware registered **before** auth still ran on the way in; once auth sets 401 and returns without `_next`, all subsequent middleware, routing, and endpoints are skipped — only earlier middleware's "after `_next`" code (if any) on the inbound leg does not run for skipped components; the response travels back through the server.

- **Already executed:** Middleware earlier in `Program.cs` (e.g., forwarded headers, logging start) ran before auth invoked `_next`.
- **Skipped:** Everything registered after auth — authorization, endpoints, MVC filters, business logic.
- **Intentional:** Fail-closed security — unauthenticated traffic must not hit handlers that assume identity.
- **Response path:** Kestrel sends status/headers/body to client; no further pipeline stages add headers unless terminal middleware mishandles started response.
- Auth should set complete response (status, optional `WWW-Authenticate`, JSON body) before return.

**Production takeaway:** 401 short-circuit is a **deliberate pipeline termination** — Karat tests whether you confuse it with an exception or missing `await _next`.

---

---

#### Q4. (R) After deploy behind nginx, HTTPS redirects loop and `[Authorize]` sees anonymous users. Review middleware order — what is wrong?

```csharp
var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseForwardedHeaders();
app.MapControllers();
app.Run();
```

---

**Answer:**

**Answer:** `UseForwardedHeaders()` runs **after** HTTPS redirection and authentication, so `Request.Scheme` stays `http` and client IP stays wrong — redirects loop and auth cookies/schemes mismatch production TLS termination.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline order | Forwarded headers after `UseHttpsRedirection` | Infinite redirect loops behind TLS-offloading proxy |
| Security | Auth middleware sees wrong scheme | Cookie `Secure` policy and redirect URI validation fail |
| Observability | Wrong `RemoteIpAddress` | Rate limits and audit logs show proxy IP only |
| Configuration | Missing `KnownProxies`/`KnownNetworks` | Spoofed `X-Forwarded-*` headers possible |

**Fix (priority order):**

1. Move `UseForwardedHeaders()` **first** (after exception handler if any).
2. Configure trusted proxy networks in `ForwardedHeadersOptions`.
3. Then `UseHttpsRedirection`, routing, authentication, authorization.

```csharp
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

**Production takeaway:** Middleware order is not cosmetic behind reverse proxies — forwarded headers are a **prerequisite** for correct scheme-aware middleware.

---

---

#### Q5. (R) Review `Run` vs `Map` branching. `/admin` returns 404 and static files leak on `/admin/config.json`. Explain pipeline behavior and fix order.

```csharp
app.UseStaticFiles();
app.Map("/admin", adminApp =>
{
    adminApp.UseRouting();
    adminApp.MapControllers();
});
app.Run(async context =>
{
    await context.Response.WriteAsync("Fallback for unmatched routes");
});
app.MapControllers();
```

---

**Answer:**

**Answer:** `app.Run` terminates the pipeline for all requests that reach it — because it is registered before `MapControllers()`, main app controllers never run; the `/admin` branch lacks static file handling control, and global static files serve sensitive paths.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | `Run` before `MapControllers()` | Terminal delegate catches requests — controllers unreachable |
| Branching | `/admin` branch missing auth/static rules | 404 on admin controllers; unintended file exposure |
| Static files | Global `UseStaticFiles()` serves `/admin/config.json` | Sensitive config downloadable |
| Design | Fallback `Run` too early | Entire API appears broken except branch routes |

**Fix (priority order):**

1. Register `MapControllers()` **before** any terminal `Run`.
2. Use `Map`/`MapWhen` for branches; avoid global terminal `Run` except true fallback at end.
3. Restrict static files with `RequestPath` or exclude admin paths; protect admin branch with auth middleware.

```csharp
app.UseStaticFiles(); // or scoped StaticFileOptions
app.Map("/admin", admin =>
{
    admin.UseAuthentication();
    admin.UseAuthorization();
    admin.MapControllers();
});
app.MapControllers();
app.Run(async ctx => { ctx.Response.StatusCode = 404; }); // last only
```

**Production takeaway:** `Run` is **terminal** — placement ends the pipeline for matching traffic; `Map` creates sub-pipelines without stopping the main app registration order.

---

---

#### Q6. (P) Where should global exception-handling middleware sit relative to routing, authentication, and `UseDeveloperExceptionPage`? What breaks if exception middleware is registered too early or too late?

---

**Answer:**

**Answer:** Register developer-facing exception pages early in Development; production exception handler (`UseExceptionHandler` or `IExceptionHandler` middleware) should wrap routing and endpoints so action and endpoint exceptions convert to uniform responses — but still inside the outermost logging/correlation middleware.

- **Development:** `UseDeveloperExceptionPage()` near top (after forwarded headers) — shows stack traces for all downstream exceptions.
- **Production:** `UseExceptionHandler("/error")` or `AddExceptionHandler<GlobalHandler>()` + `UseExceptionHandler()` early enough to catch endpoint exceptions, **before** response starts.
- **Too early:** Handler may not have endpoint metadata for ProblemDetails context — still catches exceptions.
- **Too late (after endpoints only):** Some middleware exceptions missed; worse if placed after terminal middleware.
- **Auth exceptions:** Authentication failures are often not exceptions — they short-circuit with 401/403; do not rely on exception middleware for normal auth denial.
- Never expose developer page in Production.

```csharp
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseRouting();
app.UseAuthentication();
app.MapControllers();
```

**Production takeaway:** Exception middleware is the **outer envelope** for unhandled faults — auth short-circuits and validation ProblemDetails are separate paths.

---

---

#### Q7. (R) This middleware tries to short-circuit banned clients but clients still receive full response bodies and logs show "Headers already sent." What went wrong?

```csharp
public async Task InvokeAsync(HttpContext context)
{
    await _next(context);

    if (IsBanned(context.Connection.RemoteIpAddress))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    }
}
```

---

**Answer:**

**Answer:** The middleware calls `_next` **before** checking the ban — the endpoint executes and starts the response; afterward status and body cannot be rewritten, causing "headers already sent" and defeated blocking intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline logic | Post-check after `await _next(context)` | Ban runs too late — full response already generated |
| HTTP | `StatusCode`/`WriteAsJsonAsync` after response started | Runtime error or ignored status change |
| Security | Banned IPs receive full payloads | Data leak to blocked clients |
| Performance | Expensive work runs for banned clients | Defeats purpose of early rejection |

**Fix (priority order):**

1. Check ban **before** `await _next(context)`.
2. Short-circuit with status + body and `return` without calling `_next`.
3. Place IP ban middleware early — before routing/endpoints.

```csharp
if (IsBanned(context.Connection.RemoteIpAddress))
{
    context.Response.StatusCode = StatusCodes.Status403Forbidden;
    await context.Response.WriteAsJsonAsync(new { error = "Forbidden" });
    return;
}
await _next(context);
```

**Production takeaway:** Middleware that rejects requests must run **before** the work it prevents — post-processing cannot undo a started response.

---

---

#### Q8. (M) Product needs request timing in logs for every endpoint including minimal APIs, plus early rejection of oversized uploads before MVC model binding. Would you use middleware, an endpoint filter, or an action filter for each concern — and why?

---

**Answer:**

**Answer:** Request timing belongs in middleware wrapping the whole pipeline (or `IHttpMetrics` / built-in logging); upload size rejection belongs in early middleware (Kestrel limits + custom middleware checking `Content-Length`) before routing/model binding — not action filters.

- **Request timing:** Middleware with `Stopwatch` around `await _next` captures minimal APIs and controllers uniformly; action filters miss middleware-only paths and other endpoints.
- **Upload size cap:** Configure `KestrelServerLimits.MaxRequestBodySize` and/or middleware inspecting `Content-Length` **before** `UseRouting` — reject 413 before buffering body into memory.
- **Action filters** suit per-action authorization, validation tweaks, or transforming `IActionResult` — they need routed MVC context.
- **Endpoint filters** (.NET 7+) apply to minimal APIs and route handlers — good for per-route validation, not global timing.
- Correlation IDs and security headers — middleware; business validation — filters or endpoint filters.

**Production takeaway:** "Before routing / before binding" almost always means **middleware or server limits**, not filters.

---

---

#### Q9. (R) Review custom correlation-ID middleware and registration. Some responses have two `X-Correlation-Id` headers and downstream services receive empty IDs. Diagnose.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = id;
    context.Response.Headers.Append("X-Correlation-Id", id);
    await _next(context);
}

// Program.cs
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>(); // registered twice by mistake
app.MapControllers();
```

---

**Answer:**

**Answer:** Duplicate middleware registration appends the header twice, and downstream calls likely read from `Request.Headers` after the response path instead of from `HttpContext.Items` or propagated outbound handler — empty IDs when incoming header was absent on internal calls.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pipeline | `UseMiddleware<CorrelationIdMiddleware>()` registered twice | Duplicate `X-Correlation-Id` headers confuse clients and APM |
| Design | Downstream HTTP calls not using `Items["CorrelationId"]` | Broken distributed tracing |
| HTTP | `Headers.Append` without checking existing | Violates single-value expectation |
| Observability | Logs may use different id than response header | Cannot correlate user report to logs |

**Fix (priority order):**

1. Register middleware once; use `Use` extension with guard.
2. Store id in `Items` and `HttpContext.TraceIdentifier`; set response header only if not present.
3. Use `IHttpClientFactory` delegating handler to forward header on outbound calls.

```csharp
if (!context.Response.Headers.ContainsKey("X-Correlation-Id"))
    context.Response.Headers["X-Correlation-Id"] = id;
```

**Production takeaway:** Middleware duplication is silent in compile — **double registration** produces subtle HTTP spec violations in production.

---

---

#### Q10. (D) You inherit a pipeline with 14 custom middleware components, some duplicated between `MapWhen` branches. How do you refactor without changing outward behavior — what belongs in middleware vs endpoint filters vs hosting reverse proxy?



**Answer:**

**Answer:** Consolidate duplicate cross-cutting middleware into a single ordered pipeline segment; move edge concerns (TLS, WAF, coarse rate limits) to the reverse proxy; move action-specific logic to endpoint filters — preserve behavior with integration tests on status codes, headers, and body for representative routes.

| Concern | Preferred layer |
|---|---|
| TLS, L7 DDoS, geo block | Reverse proxy / CDN |
| Forwarded headers, global exception envelope, correlation ID | Middleware (once) |
| Per-route rate limits, API key on subset | Middleware `MapWhen` or endpoint metadata + policy |
| Model validation, `IActionResult` shaping | Endpoint/action filters |
| Business rules | Application services |

- Draw current pipeline order; mark duplicates; extract `UseStandardApiPipeline()` called once.
- Avoid `MapWhen` copies that re-register auth — use endpoint metadata and policies instead.
- Characterization tests through `WebApplicationFactory` before/after refactor.

**Production takeaway:** Middleware sprawl is a maintenance hazard — **one ordered pipeline** plus edge proxy beats fourteen slightly different branches.

---

---
