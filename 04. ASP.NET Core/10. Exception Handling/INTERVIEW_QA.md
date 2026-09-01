# Exception Handling — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. How does ASP.NET Core handle unhandled exceptions by default?](#q1-how-does-aspnet-core-handle-unhandled-exceptions-by-default)
2. [Q2. What is `UseExceptionHandler` middleware?](#q2-what-is-useexceptionhandler-middleware)
3. [Q3. What is `DeveloperExceptionPage`, and when is it enabled?](#q3-what-is-developerexceptionpage-and-when-is-it-enabled)
4. [Q4. What is the difference between Development and Production exception behavior?](#q4-what-is-the-difference-between-development-and-production-exception-behavior)
5. [Q5. What is RFC 7807 ProblemDetails?](#q5-what-is-rfc-7807-problemdetails)
6. [Q6. How do you return ProblemDetails from an API?](#q6-how-do-you-return-problemdetails-from-an-api)
7. [Q7. What is `IExceptionHandler` in .NET 8+?](#q7-what-is-iexceptionhandler-in-net-8)
8. [Q8. What is the difference between `throw;` and `throw ex;`?](#q8-what-is-the-difference-between-throw-and-throw-ex)
9. [Q9. Why should you log the full exception object, not just `ex.Message`?](#q9-why-should-you-log-the-full-exception-object-not-just-exmessage)
10. [Q10. Where should global exception handling middleware be placed in the pipeline?](#q10-where-should-global-exception-handling-middleware-be-placed-in-the-pipeline)
11. [Q11. What is an exception filter, and how does it differ from exception middleware?](#q11-what-is-an-exception-filter-and-how-does-it-differ-from-exception-middleware)
12. [Q12. What information should never be exposed to external API clients in error responses?](#q12-what-information-should-never-be-exposed-to-external-api-clients-in-error-responses)
13. [Q13. How do you map domain exceptions to HTTP status codes centrally?](#q13-how-do-you-map-domain-exceptions-to-http-status-codes-centrally)
14. [Q14. What is `AddProblemDetails()`?](#q14-what-is-addproblemdetails)
15. [Q15. What happens when an exception is thrown in middleware vs in a controller action?](#q15-what-happens-when-an-exception-is-thrown-in-middleware-vs-in-a-controller-action)
16. [Q16. How do you customize error responses per exception type?](#q16-how-do-you-customize-error-responses-per-exception-type)
17. [Q17. What is the difference between client errors (4xx) and server errors (5xx)?](#q17-what-is-the-difference-between-client-errors-4xx-and-server-errors-5xx)
18. [Q18. How does `[ApiController]` affect exception handling for validation failures?](#q18-how-does-apicontroller-affect-exception-handling-for-validation-failures)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. How does ASP.NET Core handle unhandled exceptions by default?

How does ASP.NET Core handle unhandled exceptions by default?

**Answer:** Unhandled exceptions propagate up the middleware pipeline; if nothing catches them, Kestrel returns a generic HTTP 500 response with no useful body for API clients, while Development hosting may show richer diagnostics when configured.

- In **Development**, the default template enables `UseDeveloperExceptionPage()`, which returns an HTML page with exception type, message, and stack trace.
- In **Production**, without custom handling, clients typically receive a blank or minimal 500 response — no stack trace and no structured error shape.
- Exceptions thrown in middleware, filters, or endpoint code all bubble upward until exception-handling middleware, an exception filter, or the host catches them.
- ASP.NET Core 8 encourages `AddProblemDetails()` plus `UseExceptionHandler()` or `IExceptionHandler` for consistent API error responses instead of relying on defaults.

---

## Q2. What is `UseExceptionHandler` middleware?

What is `UseExceptionHandler` middleware?

**Answer:** `UseExceptionHandler` wraps downstream middleware and endpoints in a try/catch; when an unhandled exception occurs, it re-executes the pipeline on a configured error path or invokes a registered `IExceptionHandler` to produce a safe response.

- Call `app.UseExceptionHandler()` early in the pipeline — typically right after `Build()` — so routing, auth, and endpoints are covered.
- You can pass a path (`UseExceptionHandler("/error")`) or rely on .NET 8's `AddExceptionHandler<T>()` registration for programmatic handling.
- The middleware clears the response, sets an appropriate status code, and writes the error payload without rethrowing to the client.
- It does not replace logging — always log the full exception before returning the sanitized response.

---

## Q3. What is `DeveloperExceptionPage`, and when is it enabled?

What is `DeveloperExceptionPage`, and when is it enabled?

**Answer:** `DeveloperExceptionPage` is middleware that renders a detailed HTML diagnostic page for unhandled exceptions, intended only for local development debugging.

- Enable with `app.UseDeveloperExceptionPage()` when `app.Environment.IsDevelopment()` is true.
- The page shows exception type, message, stack trace, query string, cookies, headers, and routing data — information that must never reach external users.
- The default Web API template enables it only in Development; Production uses `UseExceptionHandler` instead.
- It returns HTML, not JSON — unsuitable as the sole error handler for API-only applications even in Development if clients expect `ProblemDetails`.

---

## Q4. What is the difference between Development and Production exception behavior?

What is the difference between Development and Production exception behavior?

**Answer:** Development prioritizes developer visibility (stack traces, detailed pages); Production prioritizes security and stable client contracts (generic messages, structured errors, full server-side logging).

- **Development:** `UseDeveloperExceptionPage()` or enriched `ProblemDetails` with `Extensions` containing exception details for local debugging.
- **Production:** `UseExceptionHandler` / `IExceptionHandler` returns RFC 7807 `ProblemDetails` with safe titles and details; stack traces stay in logs only.
- Environment is determined by `ASPNETCORE_ENVIRONMENT` and checked via `IHostEnvironment.IsDevelopment()`.
- Misconfiguring Production as Development exposes internal implementation details and is a common security incident.

---

## Q5. What is RFC 7807 ProblemDetails?

What is RFC 7807 ProblemDetails?

**Answer:** RFC 7807 defines a standard machine-readable format for HTTP API error responses, using fields such as `type`, `title`, `status`, `detail`, and `instance` to describe problems consistently.

- ASP.NET Core maps `ProblemDetails` to JSON with `Content-Type: application/problem+json`.
- The `status` field mirrors the HTTP status code; `title` gives a short human-readable summary; `detail` explains the specific failure.
- `type` is typically a URI identifying the error category; `instance` identifies the specific request (often the path or trace ID).
- Using ProblemDetails keeps OpenAPI/Swagger, client deserializers, and global handlers aligned on one error contract.

---

## Q6. How do you return ProblemDetails from an API?

How do you return ProblemDetails from an API?

**Answer:** Register ProblemDetails services, then return them from controllers, minimal APIs, or global exception handlers using built-in helpers or explicit `Results.Problem()` / `TypedResults.Problem()` calls.

- Call `builder.Services.AddProblemDetails()` in .NET 8 to enable customization hooks and consistent serialization.
- In controllers: `return Problem(detail: "…", statusCode: 404)` or `return NotFound(new ProblemDetails { … })`.
- In Minimal APIs: `Results.Problem(statusCode: 409, title: "Conflict")` or `TypedResults.Problem(...)` for compile-time typing.
- Global handlers implement `IExceptionHandler.TryHandleAsync` and write `ProblemDetails` via `IProblemDetailsService`.

---

## Q7. What is `IExceptionHandler` in .NET 8+?

What is `IExceptionHandler` in .NET 8+?

**Answer:** `IExceptionHandler` is a DI-registered service invoked by exception-handling middleware to centralize exception-to-response mapping in a testable, single-responsibility class.

- Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and enable via `app.UseExceptionHandler()`.
- Implement `ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)` — return `true` when the exception is handled.
- Multiple handlers can be registered; the pipeline tries them in registration order until one returns `true`.
- Prefer `IExceptionHandler` over inline lambda middleware for mapping domain exceptions to status codes and ProblemDetails shapes.

---

## Q8. What is the difference between `throw;` and `throw ex;`?

What is the difference between `throw;` and `throw ex;`?

**Answer:** `throw;` rethrows the current exception preserving the original stack trace; `throw ex;` rethrows the same exception object but resets the stack trace to the catch block, hiding the true failure site.

- Always use `throw;` when logging and delegating an unhandled failure upward without wrapping.
- Use `throw new DomainException("message", ex)` when intentionally wrapping — the inner exception preserves the original stack in `InnerException`.
- `throw ex;` makes Application Insights, Serilog, and `IExceptionHandler` logs point at the catch block instead of the root cause.
- This applies equally in async code — the stack-trace rule is unchanged after `await`.

---

## Q9. Why should you log the full exception object, not just `ex.Message`?

Why should you log the full exception object, not just `ex.Message`?

**Answer:** Logging only `ex.Message` drops the stack trace, inner exceptions, and structured logging scopes that production diagnostics depend on to find root cause quickly.

- Pass the exception as the first parameter: `_logger.LogError(ex, "Payment failed for {OrderId}", orderId)`.
- Structured logging providers capture exception type, stack, and nested `InnerException` chains automatically.
- Message-only logs cannot distinguish identical messages from different failure locations or underlying causes.
- Global exception handlers should log once at the boundary with full context, then return sanitized ProblemDetails to the client.

---

## Q10. Where should global exception handling middleware be placed in the pipeline?

Where should global exception handling middleware be placed in the pipeline?

**Answer:** Place `UseExceptionHandler()` as early as possible after `Build()`, before routing, authentication, HTTPS redirection, and endpoint middleware, so it wraps the entire downstream pipeline.

- Recommended order start: `UseExceptionHandler()` → `UseForwardedHeaders()` → `UseHttpsRedirection()` → `UseRouting()` → auth → endpoints.
- If placed too late, exceptions thrown in early middleware (e.g., forwarded headers, auth) bypass the handler.
- Exception filters run within MVC's filter pipeline and do not catch middleware exceptions — middleware placement still matters for non-controller code.
- Only one primary exception handler should write the final response; avoid duplicate catch/log/write layers.

---

## Q11. What is an exception filter, and how does it differ from exception middleware?

What is an exception filter, and how does it differ from exception middleware?

**Answer:** An exception filter (`IExceptionFilter` / `IAsyncExceptionFilter`) runs inside the MVC filter pipeline for controller actions; exception middleware wraps the entire ASP.NET Core pipeline including middleware and Minimal APIs.

- Exception filters only apply to MVC controller actions and Razor Pages — not to raw middleware or Minimal API endpoints (unless using endpoint-specific handling).
- Middleware catches exceptions from any downstream component — authentication, custom middleware, minimal routes, and controllers.
- Filters can access `ActionContext`, model state, and action metadata; middleware only sees `HttpContext`.
- Use middleware for API-wide policy; use exception filters only when action-specific context is required and middleware is insufficient.

---

## Q12. What information should never be exposed to external API clients in error responses?

What information should never be exposed to external API clients in error responses?

**Answer:** Never return stack traces, internal file paths, connection strings, SQL queries, server hostnames, dependency versions, or raw exception messages that reveal implementation details.

- Production ProblemDetails should use generic `title`/`detail` text; map sensitive internals to safe, user-facing messages.
- Include a `traceId` (from `HttpContext.TraceIdentifier`) for support correlation without exposing internals.
- Log full exception details server-side with correlation IDs tied to the same trace identifier.
- Validation errors may include field-level messages; unexpected 500 errors should not echo exception types from third-party libraries.

---

## Q13. How do you map domain exceptions to HTTP status codes centrally?

How do you map domain exceptions to HTTP status codes centrally?

**Answer:** Implement a single `IExceptionHandler` (or exception middleware) with a switch or dictionary that maps known domain exception types to HTTP status codes and ProblemDetails payloads.

- Define domain exceptions such as `NotFoundException` → 404, `ValidationException` → 400, `ConflictException` → 409, `ForbiddenException` → 403.
- Unmapped exceptions default to 500 with a generic message; log them as errors with full detail.
- Keep controllers thin — throw domain exceptions and let the handler translate; avoid per-action try/catch for predictable failures.
- Register the handler in DI so mapping logic is unit-testable independent of HTTP plumbing.

---

## Q14. What is `AddProblemDetails()`?

What is `AddProblemDetails()`?

**Answer:** `AddProblemDetails()` registers services that configure and write RFC 7807 ProblemDetails responses, including customization delegates and integration with exception handling in .NET 8.

- Call `builder.Services.AddProblemDetails(options => { … })` to set default `type`, customize `ProblemDetails` per status code, or add `Extensions`.
- Works with `IProblemDetailsService` to customize output in `IExceptionHandler.TryHandleAsync`.
- Replaces older ad-hoc JSON error shapes with a consistent, OpenAPI-friendly format across controllers and Minimal APIs.
- Can add `traceId`, validation errors, and environment-specific detail through the customization callback.

---

## Q15. What happens when an exception is thrown in middleware vs in a controller action?

What happens when an exception is thrown in middleware vs in a controller action?

**Answer:** Middleware exceptions propagate up until caught by `UseExceptionHandler` or the host; controller exceptions are first seen by MVC exception filters (if registered), then bubble to the same global middleware if unhandled.

- Middleware has no MVC filter pipeline — only global exception middleware or the host handles it.
- Controller exceptions trigger `IExceptionFilter` before reaching exception middleware, allowing action-context-aware handling.
- Both paths should converge on the same ProblemDetails contract to avoid inconsistent API error shapes.
- Minimal API exceptions skip MVC filters entirely — only endpoint filters and global middleware apply.

---

## Q16. How do you customize error responses per exception type?

How do you customize error responses per exception type?

**Answer:** In `IExceptionHandler`, pattern-match on exception type (or base types), set `ProblemDetails.Status`, `Title`, `Detail`, and optional `Extensions`, then write the response via `IProblemDetailsService`.

- Use `exception switch` or a dictionary of `Type` → handler delegate for maintainable mapping tables.
- Add custom extension fields (e.g., `errorCode`, `fieldErrors`) through `ProblemDetails.Extensions`.
- Configure status-specific defaults in `AddProblemDetails` for errors not tied to a specific exception type.
- Avoid exposing different response shapes per endpoint — one consistent schema simplifies client error handling.

---

## Q17. What is the difference between client errors (4xx) and server errors (5xx)?

What is the difference between client errors (4xx) and server errors (5xx)?

**Answer:** 4xx indicates the client sent a bad or unauthorized request and can often fix it; 5xx indicates the server failed to fulfill a valid request and the client should retry or contact support.

- **4xx examples:** 400 validation failure, 401 unauthenticated, 403 forbidden, 404 not found, 409 conflict, 422 semantic validation.
- **5xx examples:** 500 unhandled exception, 502 bad gateway, 503 service unavailable — the server or dependency failed unexpectedly.
- Map predictable business-rule violations to 4xx — returning 500 for "not found" breaks monitoring and client retry logic.
- Log 4xx at Warning/Information when expected; log 5xx at Error with full exception detail.

---

## Q18. How does `[ApiController]` affect exception handling for validation failures?

How does `[ApiController]` affect exception handling for validation failures?

**Answer:** `[ApiController]` automatically returns HTTP 400 with a ValidationProblemDetails body when model validation fails — before the action runs — without throwing an exception or requiring manual `ModelState` checks.

- Invalid models short-circuit via the automatic `[ApiController]` filter; no exception is thrown for annotation validation failures.
- The response shape is `ValidationProblemDetails` (a ProblemDetails subtype) with an `errors` dictionary keyed by field name.
- This is distinct from unhandled exceptions — validation failures are expected client errors, not 500 server errors.
- Custom validation can still throw domain exceptions that global handlers map separately from automatic 400 responses.

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

#### Q1. (P) How would you implement centralized, uniform exception handling in an ASP.NET Core Web API — including status code mapping, `ProblemDetails` response shape, and different behavior in Development vs Production?

---

**Answer:**

**Answer:** Register a global `IExceptionHandler` (or exception-handling middleware) that maps domain exceptions to HTTP status codes, returns RFC 7807 `ProblemDetails` in Production, and enables richer diagnostics only when `IHostEnvironment.IsDevelopment()` — never expose stack traces to external clients in Production.

- Register `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and `builder.Services.AddProblemDetails()` (.NET 8+); call `app.UseExceptionHandler()` early so all downstream middleware and endpoints are wrapped.
- Map exceptions deliberately in one place: `ValidationException` → 400, `NotFoundException` → 404, `ConflictException` → 409 — avoid turning predictable client errors into 500.
- In **Development**, use `AddProblemDetails` customization or `UseDeveloperExceptionPage()` for local debugging with stack traces; in **Production**, log full exceptions with `LogError(ex, "...")` and return generic titles/details.
- Include `traceId` (`HttpContext.TraceIdentifier`) in every `ProblemDetails.Extensions` for support correlation.
- Keep controllers thin — let exceptions bubble to the handler; use `Results.Problem()` only for expected, controlled failures.

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
```

**Production takeaway:** Centralized handling keeps status codes, logging, and client contracts consistent — scattered try/catch in controllers diverges under team scale and breaks OpenAPI accuracy.

---

---

#### Q2. (R) Review this exception-handling code from a service layer. What would you change and why?

```csharp
try
{
    await _paymentGateway.ChargeAsync(orderId, amount);
}
catch (Exception ex)
{
    _logger.LogError(ex.Message);
    throw ex;
}
```

---

**Answer:**

```csharp
try
{
    await _paymentGateway.ChargeAsync(orderId, amount);
}
catch (Exception ex)
{
    _logger.LogError(ex.Message);
    throw ex;
}
```

**Answer:** Logging only `ex.Message` drops the exception object from structured logs, and `throw ex` resets the stack trace to this catch block — destroying the original fault location for production diagnostics.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — no exception parameter | Missing stack trace, inner exceptions, and queryable log fields |
| Diagnostics | `throw ex` resets stack trace | APM and logs point at the catch site, not the payment gateway failure |
| Design | Catching broad `Exception` without mapping | Callers and global handlers cannot distinguish transient vs business failures |
| API contract | Rethrowing raw exception | May surface as 500 instead of mapped payment failure response |

**Fix (priority order):**

1. Change to `_logger.LogError(ex, "Payment charge failed for order {OrderId}", orderId)`.
2. Replace `throw ex` with `throw;` or wrap: `throw new PaymentFailedException("Charge declined", ex)` (See C# Gotcha — `throw;` vs `throw ex`).
3. Catch specific gateway exceptions and map to domain types the global `IExceptionHandler` understands.

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Payment charge failed for order {OrderId}", orderId);
    throw;
}
```

**Production takeaway:** This pattern passes code review visually but makes production debugging nearly impossible — Karat uses it to test observability literacy, not exception syntax.

---

---

#### Q3. (M) In a catch block, what is the difference between `throw;` and `throw ex;`, and why does the choice affect production diagnostics in ASP.NET Core?

---

**Answer:**

**Answer:** `throw;` rethrows the caught exception while preserving its original stack trace and inner-exception chain; `throw ex` throws the same object but resets the stack trace to the current catch location, making the failure appear to originate in your handler.

- Use `throw;` when logging or enriching context and delegating failure upward — Application Insights, Serilog, and OpenTelemetry need the true fault site.
- `throw ex` (or `throw new Exception(ex.Message)`) is appropriate only when intentionally wrapping: `throw new DomainException("…", ex)` to retain inner exceptions with a new outer type.
- Global handlers (`IExceptionHandler`, `UseExceptionHandler`) and exception filters rely on intact stacks to classify errors and populate `ProblemDetails`.
- In async code, the same rule applies in `catch` after `await` — stack preservation matters equally for thread-pool continuations.

**Production takeaway:** Teams that habitually write `throw ex` spend hours tracing bugs that telemetry already captured — if the stack trace were preserved.

---

---

#### Q4. (P) How should structured logging use the exception object (`LogError(ex, "...")`) vs logging only `ex.Message`? What do you lose in Application Insights or Serilog when you log message-only?

---

**Answer:**

**Answer:** Always pass the exception instance as the first argument to `LogError` so the logging provider captures type, message, stack trace, and inner exceptions as structured fields — not just a plain string concatenated into the message.

- `_logger.LogError(ex, "Payment failed for order {OrderId}", orderId)` emits `Exception`, `StackTrace`, and template properties as queryable columns in Serilog and Application Insights.
- Message-only logging loses inner exceptions (critical for `AggregateException`, `HttpRequestException` wrappers) and prevents "show me all SqlException deadlocks" queries.
- Include correlation identifiers in the message template (`TraceIdentifier`, `Activity.Current?.Id`), not embedded in `ex.Message`.
- Avoid duplicate logging — if a global handler logs unhandled exceptions, service-layer catches should either handle fully or rethrow without re-logging the same fault.

**Production takeaway:** Message-only logging looks sufficient in development consoles but breaks the first time you need to query failures by exception type in a log platform.

---

---

#### Q5. (P) Register and implement `IExceptionHandler` (.NET 8+) for a global JSON error envelope. Where does it sit relative to `UseExceptionHandler`, and what does `TryHandleAsync` returning `true` vs `false` mean?

---

**Answer:**

**Answer:** `IExceptionHandler` implementations are registered in DI and invoked by the exception-handler middleware when `UseExceptionHandler()` is in the pipeline; `TryHandleAsync` returning `true` means the handler wrote the response and processing stops — `false` delegates to the next registered handler.

- Register: `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` (multiple handlers run in registration order).
- Pipeline: `app.UseExceptionHandler()` must be registered early — typically near the top after `Build()` — to wrap routing, auth, and endpoints.
- Implement `TryHandleAsync(HttpContext, Exception, CancellationToken)` — set status code, write `ProblemDetails` JSON, log with `LogError(ex, ...)`, return `true` when handled.
- Returning `false` allows fallback handlers or default behavior; the last resort may still produce a generic 500.
- Pair with `AddProblemDetails()` for consistent RFC 7807 shape and optional customization via `IProblemDetailsService`.

```csharp
public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
{
    _logger.LogError(ex, "Unhandled exception");
    var status = MapStatusCode(ex);
    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status, Title = GetTitle(ex),
        Extensions = { ["traceId"] = ctx.TraceIdentifier }
    }, ct);
    return true;
}
```

**Production takeaway:** `IExceptionHandler` is the modern replacement for custom exception middleware — register handlers in DI instead of monolithic `UseExceptionHandler` lambda logic.

---

---

#### Q6. (M) Your API must return RFC 7807 `ProblemDetails` for all client-facing errors. Review this controller catch — what's wrong with the response contract?

```csharp
catch (ValidationException ex)
{
    return StatusCode(500, new { error = ex.Message, fields = ex.Errors });
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
```

---

**Answer:**

```csharp
catch (ValidationException ex)
{
    return StatusCode(500, new { error = ex.Message, fields = ex.Errors });
}
catch (NotFoundException ex)
{
    return NotFound(ex.Message);
}
```

**Answer:** Validation failures return 500 with an ad-hoc anonymous object instead of 400 `ProblemDetails`, and `NotFound(ex.Message)` returns plain text or inconsistent JSON — breaking RFC 7807, OpenAPI schema, and client parsers expecting a uniform error envelope.

- `ValidationException` should map to **400 Bad Request**, not 500 — client input errors are not server faults.
- Anonymous `{ error, fields }` is not `ProblemDetails` — missing `type`, `title`, `status`, `traceId`; use `ValidationProblemDetails` for field errors.
- `NotFound(ex.Message)` often produces `text/plain` string body — use `NotFound(new ProblemDetails { Status = 404, Title = "Not found", Detail = … })` or `Results.Problem`.
- Scattered controller catches duplicate logic that belongs in `IExceptionHandler` — each controller will drift to different shapes.

```csharp
// Prefer: let exception bubble to global handler, or locally:
return ValidationProblem(new ValidationProblemDetails(ex.Errors) { Status = 400 });
```

**Production takeaway:** Mixed error shapes (anonymous objects, plain strings, ProblemDetails) is a common API integration failure — centralize mapping once.

---

---

#### Q7. (D) Compare Development vs Production exception behavior: `DeveloperExceptionPage`, detailed `ProblemDetails`, stack traces in JSON, and what must never leak to external clients. How do you configure both without `#if DEBUG` in controllers?

---

**Answer:**

**Answer:** Use `IHostEnvironment` and conditional middleware registration in `Program.cs` — Development exposes detailed faults for developers; Production logs fully server-side and returns sanitized `ProblemDetails` without stack traces, connection strings, or file paths.

- **Development:** `app.UseDeveloperExceptionPage()` or `AddProblemDetails(o => o.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment())` — rich HTML or JSON detail locally.
- **Production:** `UseExceptionHandler` + `IExceptionHandler` returning generic titles ("An error occurred") and `traceId`; never include `ex.StackTrace` or internal exception types in response body.
- Configure via `if (app.Environment.IsDevelopment()) { … } else { … }` in `Program.cs` — not `#if DEBUG` in controllers.
- `launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` locally; Production must be enforced by hosting platform env vars — mis-set Production to Development exposes stack traces publicly.
- Log full exception server-side regardless of environment: `LogError(ex, ...)`.

**Production takeaway:** Environment-driven pipeline configuration keeps controllers environment-agnostic — a single misconfigured env var is a security incident.

---

---

#### Q8. (M) An MVC action throws inside an action filter; another failure occurs in custom middleware before routing. Which handlers run — exception filter, `IExceptionHandler`, `UseExceptionHandler` fallback — and in what order?

---

**Answer:**

**Answer:** Middleware exceptions are caught only by exception middleware (`UseExceptionHandler` / `IExceptionHandler`) if registered to wrap that middleware; MVC exception filters run only for exceptions thrown inside the MVC filter/action pipeline after routing — not for failures in early custom middleware.

- **Early middleware failure** (before endpoint execution): exception filters never run; only exception-handler middleware catches it if the failing middleware is downstream of `UseExceptionHandler`'s outer try/catch wrapper — register exception handler **first** (outermost) to wrap the entire pipeline.
- **Action filter / action failure:** MVC invokes exception filters (`IExceptionFilter`) first; if `ExceptionHandled = true`, the exception may not reach middleware; if unhandled, it propagates to exception middleware.
- **`IExceptionHandler` chain:** handlers run in registration order until one returns `true` from `TryHandleAsync`.
- **DeveloperExceptionPage:** only in Development; replaces or supplements handler output for browser requests.
- Minimal API exceptions skip MVC exception filters entirely — only middleware handlers apply.

**Production takeaway:** Relying on exception filters alone leaves middleware and minimal API failures with default 500 HTML — global exception middleware is mandatory for uniform API behavior.

---

---

#### Q9. (P) Design a status-code mapping table for domain exceptions (`ValidationException` → 400, `NotFoundException` → 404, conflict → 409, unauthorized business rule → 403). Where should mapping live so controllers stay thin and OpenAPI stays accurate?



**Answer:**

**Answer:** Implement mapping in a single `IExceptionHandler` (or dedicated `IExceptionToStatusCodeMapper` service injected into it) — controllers throw domain exceptions; the handler translates to HTTP status and `ProblemDetails` extensions.

- **400** — validation, malformed input (`ValidationException`, `ArgumentException` where client fault).
- **404** — missing aggregate/resource (`NotFoundException`).
- **409** — concurrency conflict, duplicate key (`ConflictException`).
- **403** — authenticated but forbidden business rule (distinct from 401 missing credentials).
- **401** — authentication failure (usually handled by auth middleware, not business exceptions).
- **500** — truly unexpected faults only; log with high severity.
- Document expected error responses in OpenAPI with `ProducesProblem(400)`, `ProducesProblem(404)`, etc., on endpoints or via operation filters.
- Avoid duplicating mapping in controller catch blocks — one switch expression or dictionary in the handler.

```csharp
private static int MapStatusCode(Exception ex) => ex switch
{
    ValidationException => StatusCodes.Status400BadRequest,
    NotFoundException => StatusCodes.Status404NotFound,
    ConflictException => StatusCodes.Status409Conflict,
    ForbiddenOperationException => StatusCodes.Status403Forbidden,
    _ => StatusCodes.Status500InternalServerError
};
```

**Production takeaway:** Domain exceptions express business outcomes; HTTP status mapping is an infrastructure concern — keeping them separate preserves clean architecture and consistent API docs.

---
