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

**Concepts**
- Exceptions propagating up the middleware pipeline to Kestrel
- Development default — DeveloperExceptionPage
- Production default — generic 500 with no structured body
- AddProblemDetails() + UseExceptionHandler() as the encouraged pattern

**Answer**

Unhandled exceptions propagate up the middleware pipeline; if nothing catches them, Kestrel returns a generic HTTP 500 response with no useful body for API clients. In Development the default template enables `UseDeveloperExceptionPage()`, which returns an HTML page with exception type, message, and stack trace. In Production, without custom handling, clients receive a blank or minimal 500 response — no stack trace and no structured error shape. ASP.NET Core 8 encourages `AddProblemDetails()` plus `UseExceptionHandler()` or `IExceptionHandler` for consistent API error responses rather than relying on these defaults.

---

## Q2. What is `UseExceptionHandler` middleware?

**Concepts**
- Try/catch wrapping downstream middleware and endpoints
- Pipeline position — must be early to cover all downstream code
- Re-executing on the configured error path
- Complement to logging — does not replace it

**Answer**

`UseExceptionHandler` wraps downstream middleware and endpoints in a try/catch; when an unhandled exception occurs, it clears the response, re-executes the pipeline on a configured error path or invokes registered `IExceptionHandler` implementations to produce a safe response. Call `app.UseExceptionHandler()` early in the pipeline — typically right after `Build()` — so routing, auth, and endpoints are all covered. Passing a path (`UseExceptionHandler("/error")`) delegates to a controller action, while .NET 8's `AddExceptionHandler<T>()` registration handles exceptions programmatically. This middleware does not replace logging — always log the full exception before returning the sanitized response.

---

## Q3. What is `DeveloperExceptionPage`, and when is it enabled?

**Concepts**
- HTML diagnostic page for local debugging
- IsDevelopment() gate for enabling it
- Information exposure risk — never for production
- HTML output incompatible with JSON API clients

**Answer**

`DeveloperExceptionPage` is middleware that renders a detailed HTML diagnostic page for unhandled exceptions, intended only for local development debugging. Enable it with `app.UseDeveloperExceptionPage()` when `app.Environment.IsDevelopment()` is true — the page shows exception type, message, stack trace, query string, cookies, headers, and routing data, which must never reach external users. The default Web API template enables it only in Development and uses `UseExceptionHandler` instead in all other environments. Since it returns HTML rather than JSON, it is unsuitable as the sole error handler for API-only applications even in Development when clients expect `ProblemDetails`.

---

## Q4. What is the difference between Development and Production exception behavior?

**Concepts**
- Development — stack traces and detailed diagnostics for visibility
- Production — generic messages, structured errors, full server-side logging
- ASPNETCORE_ENVIRONMENT controlling the conditional branch
- Security incident risk of misconfiguring Production as Development

**Answer**

Development prioritizes developer visibility with stack traces, detailed HTML pages, and enriched `ProblemDetails.Extensions` containing exception details for local debugging. Production prioritizes security and stable client contracts — `UseExceptionHandler` or `IExceptionHandler` returns RFC 7807 `ProblemDetails` with safe titles and details while stack traces stay in server logs only. The environment is determined by `ASPNETCORE_ENVIRONMENT` and checked via `IHostEnvironment.IsDevelopment()`. Misconfiguring Production as Development exposes internal implementation details and is a common security incident.

---

## Q5. What is RFC 7807 ProblemDetails?

**Concepts**
- Standard machine-readable HTTP error response format
- Fields — type, title, status, detail, instance
- Content-Type: application/problem+json
- Alignment between OpenAPI, client deserializers, and global handlers

**Answer**

RFC 7807 defines a standard machine-readable format for HTTP API error responses, using fields such as `type`, `title`, `status`, `detail`, and `instance` to describe problems consistently. ASP.NET Core maps `ProblemDetails` to JSON with `Content-Type: application/problem+json`. The `status` field mirrors the HTTP status code; `title` gives a short human-readable summary; `detail` explains the specific failure; `type` is typically a URI identifying the error category; and `instance` identifies the specific request, often the path or trace ID. Using `ProblemDetails` keeps OpenAPI/Swagger, client deserializers, and global handlers aligned on one error contract.

---

## Q6. How do you return ProblemDetails from an API?

**Concepts**
- AddProblemDetails() registration for customization hooks
- Results.Problem() and TypedResults.Problem() in Minimal APIs
- Controller Problem() helper method
- IExceptionHandler writing via IProblemDetailsService

**Answer**

Register `builder.Services.AddProblemDetails()` in .NET 8 to enable customization hooks and consistent serialization, then return `ProblemDetails` from controllers, minimal APIs, or global exception handlers using built-in helpers. In controllers use `return Problem(detail: "…", statusCode: 404)` or `return NotFound(new ProblemDetails { … })`; in Minimal APIs use `Results.Problem(statusCode: 409, title: "Conflict")` or `TypedResults.Problem(...)` for compile-time typing. Global handlers implement `IExceptionHandler.TryHandleAsync` and write `ProblemDetails` via `IProblemDetailsService`.

---

## Q7. What is `IExceptionHandler` in .NET 8+?

**Concepts**
- DI-registered exception-to-response mapping service
- TryHandleAsync returning bool — chain of responsibility pattern
- Multiple handlers in registration order
- Preferred over inline lambda middleware

**Answer**

`IExceptionHandler` is a DI-registered service invoked by exception-handling middleware to centralize exception-to-response mapping in a testable, single-responsibility class. Register with `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and enable via `app.UseExceptionHandler()`. Implement `ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)` — return `true` when the exception is handled, which stops propagation. Multiple handlers can be registered and the pipeline tries them in registration order until one returns `true`. Prefer `IExceptionHandler` over inline lambda middleware for mapping domain exceptions to status codes and `ProblemDetails` shapes, since each handler is independently testable.

---

## Q8. What is the difference between `throw;` and `throw ex;`?

**Concepts**
- throw; preserving the original stack trace
- throw ex; resetting the stack trace to the catch site
- InnerException preservation when intentionally wrapping
- APM and structured logging dependency on accurate stacks

**Answer**

`throw;` rethrows the current exception while preserving its original stack trace and inner-exception chain; `throw ex` rethrows the same exception object but resets the stack trace to the catch block, hiding the true failure site. Always use `throw;` when logging and delegating a failure upward — Application Insights, Serilog, and `IExceptionHandler` all need the true fault site. Use `throw new DomainException("message", ex)` only when intentionally wrapping to add context, since this preserves the original stack in `InnerException`. The rule applies equally in async code after `await` — stack-trace preservation is unchanged across thread-pool continuations.

---

## Q9. Why should you log the full exception object, not just `ex.Message`?

**Concepts**
- Exception parameter as first LogError argument
- Structured logging capturing type, stack trace, InnerException chain
- Message-only losing queryable log fields
- Single logging point at the boundary to avoid duplication

**Answer**

Logging only `ex.Message` drops the stack trace, inner exceptions, and structured logging scopes that production diagnostics depend on to find root cause quickly. Pass the exception as the first parameter — `_logger.LogError(ex, "Payment failed for {OrderId}", orderId)` — so the logging provider captures exception type, stack, and nested `InnerException` chains as queryable structured fields. Message-only logs cannot distinguish identical messages from different failure locations or underlying causes. Global exception handlers should log once at the boundary with full context, then return sanitized `ProblemDetails` to the client.

---

## Q10. Where should global exception handling middleware be placed in the pipeline?

**Concepts**
- Position — first middleware after Build() to wrap entire pipeline
- Middleware exceptions bypassing exception filters
- Single primary handler to avoid duplicate response writes
- Recommended pipeline order

**Answer**

Place `UseExceptionHandler()` as early as possible after `Build()`, before routing, authentication, HTTPS redirection, and endpoint middleware, so it wraps the entire downstream pipeline. The recommended order is: `UseExceptionHandler()` → `UseForwardedHeaders()` → `UseHttpsRedirection()` → `UseRouting()` → auth → endpoints. If placed too late, exceptions thrown in early middleware — forwarded headers, auth — bypass the handler entirely. Exception filters run within MVC's filter pipeline and do not catch middleware exceptions, so middleware placement still matters for non-controller code. Only one primary exception handler should write the final response; avoid duplicate catch-log-write layers.

---

## Q11. What is an exception filter, and how does it differ from exception middleware?

**Concepts**
- Exception filter scope — MVC controller actions and Razor Pages only
- Exception middleware scope — entire pipeline including minimal APIs
- ActionContext and model state access in filters vs HttpContext only in middleware
- API-wide policy belonging in middleware

**Answer**

An exception filter (`IExceptionFilter` / `IAsyncExceptionFilter`) runs inside the MVC filter pipeline for controller actions — it never applies to raw middleware, minimal API endpoints, or requests that fail before routing selects an MVC action. Exception middleware wraps the entire ASP.NET Core pipeline, since covering every downstream component including authentication, custom middleware, minimal routes, and controllers. Filters can access `ActionContext`, model state, and action metadata; middleware only sees `HttpContext`. Use middleware for API-wide exception policy; use exception filters only when action-specific MVC context is required and middleware is insufficient.

---

## Q12. What information should never be exposed to external API clients in error responses?

**Concepts**
- Stack traces, file paths, connection strings as information disclosure
- traceId for support correlation without exposing internals
- Generic safe titles for unexpected 500 errors
- Server-side logging with full context vs sanitized client response

**Answer**

Never return stack traces, internal file paths, connection strings, SQL queries, server hostnames, dependency versions, or raw exception messages that reveal implementation details. Production `ProblemDetails` should use generic `title` and `detail` text, mapping sensitive internals to safe user-facing messages. Include a `traceId` from `HttpContext.TraceIdentifier` in every error response for support correlation without exposing internals — this lets operators find the full server log for that request. Validation errors may include field-level messages since those describe client input; unexpected 500 errors should not echo exception type names from third-party libraries.

---

## Q13. How do you map domain exceptions to HTTP status codes centrally?

**Concepts**
- Single IExceptionHandler with switch or dictionary mapping
- Domain exception hierarchy — NotFoundException, ValidationException, ConflictException
- Thin controllers that let exceptions bubble to the handler
- Unit-testable mapping logic independent of HTTP plumbing

**Answer**

Implement a single `IExceptionHandler` (or exception middleware) with a switch or dictionary that maps known domain exception types to HTTP status codes and `ProblemDetails` payloads — `NotFoundException` → 404, `ValidationException` → 400, `ConflictException` → 409, `ForbiddenException` → 403. Unmapped exceptions default to 500 with a generic message and are logged as errors with full detail. Keep controllers thin — throw domain exceptions and let the handler translate, rather than scattering per-action try/catch blocks for predictable failures. Placing the mapping logic in a DI-registered handler makes it independently unit-testable without HTTP plumbing.

---

## Q14. What is `AddProblemDetails()`?

**Concepts**
- Registering IProblemDetailsService and customization delegates
- Per-status-code default configuration via options callback
- traceId and environment-specific extensions
- OpenAPI-friendly uniform error format

**Answer**

`AddProblemDetails()` registers services that configure and write RFC 7807 `ProblemDetails` responses, including customization delegates and integration with exception handling in .NET 8. Call `builder.Services.AddProblemDetails(options => { … })` to set default `type` URIs, customize `ProblemDetails` per status code, or add `Extensions`. It works with `IProblemDetailsService` inside `IExceptionHandler.TryHandleAsync` to write consistent output. This replaces older ad-hoc JSON error shapes with a consistent, OpenAPI-friendly format across controllers and Minimal APIs, and can add `traceId`, validation errors, and environment-specific detail through the customization callback.

---

## Q15. What happens when an exception is thrown in middleware vs in a controller action?

**Concepts**
- Middleware exceptions reaching only UseExceptionHandler
- Controller exceptions triggering IExceptionFilter before middleware
- Both paths converging on the same ProblemDetails contract
- Minimal API exceptions skipping MVC filters entirely

**Answer**

Middleware exceptions propagate up until caught by `UseExceptionHandler` or the host — there is no MVC filter pipeline for them. Controller exceptions are first seen by MVC exception filters if registered, giving action-context-aware handling, and only then bubble to global exception middleware if unhandled. Both paths should converge on the same `ProblemDetails` contract to avoid inconsistent API error shapes depending on where the failure originated. Minimal API exceptions skip MVC filters entirely — only endpoint filters and global middleware apply, which is why global exception middleware is mandatory for uniform API behavior.

---

## Q16. How do you customize error responses per exception type?

**Concepts**
- Pattern matching on exception type in TryHandleAsync
- ProblemDetails.Extensions for custom fields like errorCode
- Status-specific defaults via AddProblemDetails for non-exception errors
- Consistent single schema across all endpoints

**Answer**

In `IExceptionHandler`, pattern-match on exception type using a switch expression or a dictionary of `Type` to handler delegate, then set `ProblemDetails.Status`, `Title`, `Detail`, and optional `Extensions`. Add custom extension fields such as `errorCode` or `fieldErrors` through `ProblemDetails.Extensions`. Configure status-specific defaults in `AddProblemDetails` for errors not tied to a specific exception type. Avoid different response shapes per endpoint — a single consistent schema simplifies client error handling and keeps OpenAPI accurate.

---

## Q17. What is the difference between client errors (4xx) and server errors (5xx)?

**Concepts**
- 4xx — client-fixable request errors
- 5xx — server or dependency failures
- Mapping predictable business violations to 4xx not 500
- Log levels — Warning for expected 4xx, Error for 5xx

**Answer**

4xx indicates the client sent a bad or unauthorized request that the client can often fix — 400 validation failure, 401 unauthenticated, 403 forbidden, 404 not found, 409 conflict, 422 semantic validation. 5xx indicates the server failed to fulfill a valid request — 500 unhandled exception, 502 bad gateway, 503 service unavailable. The key reason this distinction matters is that returning 500 for "not found" breaks monitoring dashboards, client retry logic, and alerting thresholds calibrated for server failures. Map predictable business-rule violations to 4xx; log 4xx at Warning or Information when expected, and log 5xx at Error with full exception detail.

---

## Q18. How does `[ApiController]` affect exception handling for validation failures?

**Concepts**
- Automatic 400 ValidationProblemDetails before the action runs
- No exception thrown for annotation validation failures
- ValidationProblemDetails with errors dictionary keyed by field
- Distinction from unhandled exceptions — validation is expected, not 500

**Answer**

`[ApiController]` automatically returns HTTP 400 with a `ValidationProblemDetails` body when model validation fails — before the action runs — without throwing an exception or requiring manual `ModelState` checks. The response shape is `ValidationProblemDetails` (a `ProblemDetails` subtype) with an `errors` dictionary keyed by field name. This is distinct from unhandled exceptions — validation failures are expected client errors that the `[ApiController]` filter handles, not 500 server errors that reach `IExceptionHandler`. Custom validation that throws domain exceptions still flows to the global handler and maps separately from these automatic 400 responses.

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- UseRouting must precede UseAuthentication and UseAuthorization
- Endpoint metadata not selected before routing runs
- Recommended pipeline order for ASP.NET Core 8

**Answer**

In ASP.NET Core endpoint routing, `UseAuthentication` and `UseAuthorization` must run after `UseRouting` so the auth middleware can read endpoint metadata — if auth runs before routing, the endpoint has not been selected yet and policy resolution for `[Authorize]` and `RequireAuthorization()` cannot inspect the correct attributes. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints. Symptoms of wrong order include anonymous access to protected endpoints and 401 challenges that fire without correctly applying per-endpoint allow-anonymous overrides.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive dependency lifetime violation
- EF DbContext stale change tracker accumulation
- ValidateScopes detecting the problem at startup
- IServiceScopeFactory as the correct fix

**Answer**

A scoped service injected into a singleton is held for the entire application lifetime, long after the scope that created it was disposed. The most common case is `DbContext`: the change tracker accumulates entities from unrelated requests, and after the scope is torn down any access throws `ObjectDisposedException`. Enable `ValidateScopes = true` in Development and staging to catch these combinations at startup rather than under production load. The fix is to inject `IServiceScopeFactory` and create a scope per unit of work, or use `IDbContextFactory<T>` to get a short-lived context per operation.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- HttpMessageHandler lifetime and socket exhaustion
- IHttpClientFactory managed handler recycling
- Named and typed client registration pattern

**Answer**

Instantiating `HttpClient` with `new` in a long-lived singleton prevents socket reuse because each instance holds its own `HttpMessageHandler` and the underlying TCP connections are not returned to a pool until garbage collection. Under load this causes socket exhaustion — `SocketException` and timeout errors that do not appear in local testing with low concurrency. `IHttpClientFactory` manages handler lifetimes and recycles connections correctly, so the fix is to register named or typed clients via `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()` and inject them rather than constructing `HttpClient` directly.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- IOptions<T> frozen snapshot at first resolution
- IOptionsSnapshot<T> recalculates per request scope
- IOptionsMonitor<T> live change notifications for singletons
- Silent staleness until process restart

**Answer**

`IOptions<T>` resolves once and caches the configuration snapshot for the service's lifetime, so a singleton that reads `.Value` in its constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope but is only usable in scoped services. `IOptionsMonitor<T>` supports change notifications via `OnChange` and works correctly in singletons. The failure mode is silent — misconfiguration persists until process restart because `.Value` was captured at construction.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET semantics and safe/idempotent URL parameters
- Proxies and caches stripping GET request bodies
- [FromQuery] with [AsParameters] for complex filter criteria
- Silent failures in CDN and proxy layers

**Answer**

`[FromBody]` on a GET endpoint is an anti-pattern because HTTP GET is defined as safe and idempotent with parameters in the URL — many clients, CDNs, and caching proxies strip or ignore request bodies on GET requests, so binding fails silently in production while "Try it out" in Swagger may appear to work. Use `[FromQuery]` with separate parameter names or `[AsParameters]` on a record type to aggregate complex filter criteria into a single clean parameter object.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- JsonNamingPolicy.CamelCase as ASP.NET Core default
- Silent binding producing default values instead of errors
- PropertyNameCaseInsensitive as a mitigation
- Validation attributes turning silent failure into 400 responses

**Answer**

ASP.NET Core Web API serializes JSON with `JsonNamingPolicy.CamelCase` by default, which means incoming JSON with PascalCase keys like `"CustomerName"` does not match the property — the model binds successfully but properties silently hold default values (null, zero, false). The preferred fix is standardizing all clients on camelCase and enforcing it through OpenAPI contracts. As a mitigation, `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` relaxes matching. Add required validation attributes so silent binding failures produce 400 responses rather than corrupt data silently stored to the database.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- throw; preserving original stack trace
- throw ex; resetting stack trace to the catch site
- InnerException preservation when intentionally wrapping
- APM and structured logging dependency on accurate stack traces

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, which means Application Insights, Serilog, and `IExceptionHandler` all point at the handler rather than the code that actually failed. Bare `throw;` preserves the full original stack trace. Use `throw;` when logging and delegating upward; wrap with a new exception type only when adding context — `throw new OrderProcessingException("...", ex)` — so the original failure is preserved in `InnerException`. This rule applies identically in async code after `await`.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel as application server vs edge gateway
- TLS termination and certificate management at the reverse proxy
- WAF, rate limiting, and static file caching at the edge
- UseForwardedHeaders required for client IP logging

**Answer**

Kestrel is a production-grade application server optimized for running .NET efficiently, but directly exposing it to the internet skips TLS certificate centralization, WAF filtering, centralized rate limiting, and efficient static-file caching that reverse proxies handle. nginx, IIS, Azure Front Door, or AWS ALB typically sit in front so certificates are managed at the proxy layer with automatic renewal. If Kestrel is exposed directly, client IP logging requires `UseForwardedHeaders` configuration, and containers typically bind Kestrel to an internal port while the ingress controller handles external HTTPS.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- launchSettings.json applies only to dotnet run and IDE launch
- ASPNETCORE_URLS and ASPNETCORE_ENVIRONMENT as production env vars
- appsettings.Production.json for non-secret production tuning

**Answer**

`Properties/launchSettings.json` contains URLs, environment variables, and launch profiles that are read only by `dotnet run`, Visual Studio, and VS Code — the file is not deployed to production hosts and has no effect on them. Relying on it for environment name or URL configuration leads to wrong `ASPNETCORE_ENVIRONMENT` or binding address in deployed environments. Production URLs and environment come from host-level environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- default(false) for missing JSON field
- Nullable bool? for tri-state intent
- PATCH semantics requiring omitted-vs-false distinction
- Update DTO design for partial updates

**Answer**

A non-nullable `bool` property in a PATCH DTO cannot distinguish "field omitted from JSON" from "explicitly set to false" because `System.Text.Json` deserializes missing properties to `default(false)`, which corrupts partial-update semantics — a client updating only an email address accidentally resets a consent flag to false. PATCH endpoints need `bool?`, separate update DTOs that only include fields being modified, or tri-state enums like `Unspecified | OptIn | OptOut` to represent intent explicitly. Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host headers
- ForwardedHeadersOptions.KnownProxies for trusted network restriction
- Pipeline position — must run before HTTPS redirection and auth
- Header spoofing risk when trusting all proxies

**Answer**

Without `UseForwardedHeaders()` configured with known proxy IPs, `HttpContext.Request.Scheme` stays `http` even when clients used HTTPS, `Request.Host` reflects the internal address, and the client IP is the proxy — breaking HTTPS redirects, secure cookie flags, and audit logs. Call `UseForwardedHeaders()` as early as possible, before HTTPS redirection, authentication, link generation, and rate limiting by IP. Configure `ForwardedHeadersOptions` to trust only your specific reverse proxy network rather than all proxies, since trusting all enables header spoofing by any client.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- UseStaticFiles() serving without authentication
- wwwroot as a public CDN root
- Secrets management via environment variables and Key Vault
- Build pipeline verification of publish output

**Answer**

Every file in `wwwroot` is served to unauthenticated anonymous clients by `UseStaticFiles()` — there is no authentication gate by default. Placing `.env` files, `appsettings.Production.json`, private keys, or backup configs there makes them directly downloadable via their URL path. Only public assets such as CSS, JavaScript, images, and public PDFs belong in `wwwroot`. Sensitive configuration must live in environment variables, Azure Key Vault, or similar secret managers, and build pipelines should verify that publish output does not include secrets in the web root.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback order relative to API endpoint mapping
- /api/* returning index.html with HTTP 200 as a silent failure
- Endpoint-first ordering in Program.cs

**Answer**

Registering `MapFallbackToFile("index.html")` before API endpoint mapping causes any unmatched API route — including valid 404s — to return `index.html` with HTTP 200, which breaks JSON parsers on clients and masks the real failure. The correct order is to map API routes with `MapControllers()` or `MapGroup("/api")` first, then static files, then the SPA fallback last. Symptoms include CORS errors appearing as HTML responses and Swagger fetch failures in production SPA hosting.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- BackgroundService singleton lifetime
- Scoped service constructor injection causing disposal errors
- IServiceScopeFactory.CreateAsyncScope() per background job
- ValidateScopes detecting this at startup

**Answer**

A singleton `BackgroundService` cannot constructor-inject scoped services like `DbContext` because hosted services live for the application lifetime while scoped instances are disposed after their first scope ends, causing `ObjectDisposedException` or scope validation errors at startup. The fix is to inject `IServiceScopeFactory`, then inside each background job call `await using var scope = factory.CreateAsyncScope()`, resolve the scoped service from `scope.ServiceProvider`, and dispose the scope when the job finishes. Enable `ValidateScopes` in Development to catch this before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR broadcast scope — single server instance only
- Redis or Azure Service Bus backplane for multi-instance routing
- Sticky sessions vs backplane trade-offs
- Azure SignalR Service as a managed alternative

**Answer**

SignalR tracks connected clients per server instance, so a broadcast from one instance reaches only the clients connected to that instance. With multiple instances behind a load balancer, users on different nodes never receive events raised on other nodes — a critical failure for real-time chat or notifications. Sticky sessions keep one client on one node but do not route server-side events across nodes. The solution is a Redis or Azure Service Bus backplane registered with `AddSignalR().AddStackExchangeRedis(...)`, or the managed Azure SignalR Service. Test scale-out with at least two instances before launch.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (P) How would you implement centralized, uniform exception handling in an ASP.NET Core Web API — including status code mapping, `ProblemDetails` response shape, and different behavior in Development vs Production?

**Concepts**
- IExceptionHandler registered in DI for centralized mapping
- AddProblemDetails() for consistent RFC 7807 serialization
- Domain exception to HTTP status code mapping in one place
- Development diagnostic detail vs Production sanitized responses
- traceId in every ProblemDetails.Extensions for correlation

**Answer**

Register a global `IExceptionHandler` that maps domain exceptions to HTTP status codes, returns RFC 7807 `ProblemDetails` in Production, and enables richer diagnostics only when `IHostEnvironment.IsDevelopment()`. The setup is `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`, `builder.Services.AddProblemDetails()`, and `app.UseExceptionHandler()` early so all downstream middleware and endpoints are wrapped.

Map exceptions deliberately in one place: `ValidationException` → 400, `NotFoundException` → 404, `ConflictException` → 409 — since returning 500 for predictable client errors breaks monitoring and client retry logic. In Development, use `AddProblemDetails` customization or `UseDeveloperExceptionPage()` for local debugging with stack traces; in Production, log full exceptions with `LogError(ex, "...")` and return generic titles and details only. Include `traceId` from `HttpContext.TraceIdentifier` in every `ProblemDetails.Extensions` so operators can correlate the sanitized response to the full server log. Keep controllers thin — let exceptions bubble to the handler rather than scattering per-action try/catch blocks.

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
```

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

**Concepts**
- LogError(ex, ...) vs LogError(ex.Message) — structured fields lost
- throw ex; resetting stack trace to catch site
- Catching broad Exception without type-specific mapping
- Single-point logging at the global handler boundary

**Answer**

Logging only `ex.Message` drops the exception object from structured logs, so Application Insights and Serilog receive a plain string rather than a queryable `Exception` type, stack trace, and inner exception chain. `throw ex` then resets the stack trace to this catch block, making telemetry point at the service layer catch rather than the actual payment gateway failure.

The first fix is `_logger.LogError(ex, "Payment charge failed for order {OrderId}", orderId)` so the exception instance is captured as a structured field. The second fix is replacing `throw ex` with `throw;` to preserve the original stack, or wrapping with `throw new PaymentFailedException("Charge declined", ex)` when a domain type is needed for the global handler's status-code mapping. Catching broad `Exception` without type-specific handling also means transient network errors and business validation failures look identical to callers — mapping to specific domain types here lets the global `IExceptionHandler` return appropriate 4xx versus 5xx responses.

```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Payment charge failed for order {OrderId}", orderId);
    throw;
}
```

---

#### Q3. (M) In a catch block, what is the difference between `throw;` and `throw ex;`, and why does the choice affect production diagnostics in ASP.NET Core?

**Concepts**
- throw; preserving original stack trace through the call chain
- throw ex; resetting stack trace to catch block
- Wrapping pattern — throw new DomainException("...", ex)
- APM, Serilog, and IExceptionHandler needing intact stacks

**Answer**

`throw;` rethrows the caught exception while preserving its original stack trace and inner-exception chain; `throw ex` throws the same object but resets the stack trace to the current catch location, making the failure appear to originate in the handler rather than in the payment gateway, database driver, or service that actually failed. Application Insights, Serilog, and OpenTelemetry need the true fault site to classify errors, populate structured log fields, and surface root-cause alerts.

Use `throw;` when logging or enriching context and delegating failure upward. `throw ex` (or `throw new Exception(ex.Message)`) is appropriate only when intentionally wrapping to add a new exception type — `throw new DomainException("…", ex)` — which retains the original in `InnerException` so the full chain is still visible in structured logs. In async code after `await`, the same rule applies — stack preservation is unchanged for thread-pool continuations.

---

#### Q4. (P) How should structured logging use the exception object (`LogError(ex, "...")`) vs logging only `ex.Message`? What do you lose in Application Insights or Serilog when you log message-only?

**Concepts**
- Exception instance as first LogError argument — structured fields
- Stack trace, InnerException chain, and queryable exception type
- Correlation identifiers in message template not in ex.Message
- Avoiding duplicate logging with a centralized global handler

**Answer**

Always pass the exception instance as the first argument to `LogError` — `_logger.LogError(ex, "Payment failed for order {OrderId}", orderId)` — so the logging provider captures exception type, stack trace, and inner exceptions as queryable structured columns in Serilog and Application Insights. Message-only logging loses inner exceptions (critical for `AggregateException` and `HttpRequestException` wrappers), prevents "show me all SqlException deadlocks" queries, and collapses distinct failure paths into identical strings.

Include correlation identifiers in the message template — `TraceIdentifier`, `Activity.Current?.Id` — rather than embedding them in `ex.Message`. Avoid duplicate logging: if a global handler logs unhandled exceptions at the boundary, service-layer catches should either handle the failure fully or rethrow without re-logging the same fault, since duplicate log entries make correlation harder rather than easier.

---

#### Q5. (P) Register and implement `IExceptionHandler` (.NET 8+) for a global JSON error envelope. Where does it sit relative to `UseExceptionHandler`, and what does `TryHandleAsync` returning `true` vs `false` mean?

**Concepts**
- IExceptionHandler registered in DI, invoked by UseExceptionHandler
- TryHandleAsync returning true stops chain, false passes to next handler
- Registration order determining priority
- AddProblemDetails() for consistent RFC 7807 shape

**Answer**

`IExceptionHandler` implementations are registered in DI and invoked by `UseExceptionHandler` middleware — the middleware must be registered early in the pipeline to wrap all downstream code. `TryHandleAsync` returning `true` means the handler wrote the response and processing stops; returning `false` passes the exception to the next registered handler in registration order, allowing a chain of responsibility where a specific handler for `ValidationException` runs before a catch-all for everything else.

Register via `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` — multiple handlers run in the order registered. Implement `TryHandleAsync(HttpContext, Exception, CancellationToken)`, set status code, write `ProblemDetails` JSON, log with `LogError(ex, ...)`, and return `true` when handled. Pair with `AddProblemDetails()` for consistent RFC 7807 shape and optional customization via `IProblemDetailsService`.

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

**Concepts**
- ValidationException mapped to 500 instead of 400
- Anonymous object not ProblemDetails — missing type, title, traceId
- NotFound(string) producing text/plain not ProblemDetails
- Scattered controller catches producing inconsistent schemas

**Answer**

Validation failures are client errors, not server faults, so returning `StatusCode(500, ...)` is wrong — they should map to 400. The anonymous `{ error, fields }` object is also not `ProblemDetails` — it lacks `type`, `title`, `status`, and `traceId`, which breaks RFC 7807 compliance, OpenAPI schema accuracy, and client parsers expecting a uniform error envelope. `NotFound(ex.Message)` often produces a `text/plain` string body rather than JSON, since the string overload does not produce `ProblemDetails` automatically.

Scattering these catches across controllers means each will drift to different shapes over time. The fix is to let these exceptions bubble to the global `IExceptionHandler` where mapping is centralized, or locally use `ValidationProblem` and `NotFound` with explicit `ProblemDetails` objects.

```csharp
// Prefer: let exception bubble to global handler, or locally:
return ValidationProblem(new ValidationProblemDetails(ex.Errors) { Status = 400 });
```

---

#### Q7. (D) Compare Development vs Production exception behavior: `DeveloperExceptionPage`, detailed `ProblemDetails`, stack traces in JSON, and what must never leak to external clients. How do you configure both without `#if DEBUG` in controllers?

**Concepts**
- IHostEnvironment conditional middleware in Program.cs
- DeveloperExceptionPage — HTML diagnostics for local use only
- Stack traces and internal paths never in production response body
- launchSettings.json setting Development locally vs host env vars for Production

**Answer**

Configure environment-specific behavior in `Program.cs` using `if (app.Environment.IsDevelopment()) { … } else { … }` — not `#if DEBUG` in controllers, since `#if DEBUG` is a compile-time switch while environment configuration is a deployment-time switch. In Development, `app.UseDeveloperExceptionPage()` or `AddProblemDetails(o => o.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment())` exposes rich detail locally. In Production, `UseExceptionHandler` plus `IExceptionHandler` returns generic titles and no stack traces; stack traces, file paths, connection strings, and exception type names from third-party libraries must never appear in the response body.

`launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` on developer machines; Production must be enforced by hosting platform environment variables — an Azure App Setting, Kubernetes manifest entry, or Docker `-e` flag. Misconfiguring Production as Development is a single environment variable away from a security incident that exposes stack traces publicly. Log full exceptions server-side regardless of environment — `LogError(ex, ...)` — so the detailed information is always available in your logging platform even when the client receives only a `traceId`.

---

#### Q8. (M) An MVC action throws inside an action filter; another failure occurs in custom middleware before routing. Which handlers run — exception filter, `IExceptionHandler`, `UseExceptionHandler` fallback — and in what order?

**Concepts**
- Action filter exception reaching IExceptionFilter before middleware
- Early middleware exception reaching only UseExceptionHandler
- IExceptionHandler chain TryHandleAsync registration order
- Minimal API exceptions bypassing MVC exception filters

**Answer**

For a failure in custom middleware before routing, exception filters never run — only exception-handler middleware catches it, provided the failing middleware is downstream of `UseExceptionHandler`'s outer try/catch. This is why `UseExceptionHandler` must be registered first: if it wraps all downstream middleware, failures anywhere in the pipeline reach it.

For a failure inside an action filter or action method, MVC invokes exception filters (`IExceptionFilter`) first — they run inside the MVC pipeline and can mark the exception handled before it propagates. If the exception filter sets `ExceptionHandled = true`, the exception may not reach middleware. If left unhandled, the exception bubbles to `UseExceptionHandler`, which invokes registered `IExceptionHandler` implementations in registration order until one returns `true` from `TryHandleAsync`. The DeveloperExceptionPage sits at the outermost layer and only runs in Development. Minimal API exceptions skip MVC exception filters entirely — only middleware handlers apply, so relying on exception filters alone leaves minimal API failures with unhandled 500 HTML responses.

---

#### Q9. (P) Design a status-code mapping table for domain exceptions (`ValidationException` → 400, `NotFoundException` → 404, conflict → 409, unauthorized business rule → 403). Where should mapping live so controllers stay thin and OpenAPI stays accurate?

**Concepts**
- Single IExceptionHandler as the mapping owner
- Domain exception hierarchy aligned to HTTP semantics
- ProducesProblem OpenAPI annotation for accurate docs
- 500 reserved for genuinely unexpected faults only

**Answer**

Implement mapping in a single `IExceptionHandler` using a switch expression — controllers throw domain exceptions and let the handler translate to HTTP status and `ProblemDetails` extensions rather than catching per-action. The mapping is: `ValidationException` → 400 for client input errors; `NotFoundException` → 404 for missing aggregates; `ConflictException` → 409 for concurrency or duplicate key failures; `ForbiddenOperationException` → 403 for authenticated-but-forbidden business rules (distinct from 401 missing credentials, which authentication middleware handles); 500 only for genuinely unexpected faults. Log 5xx at Error with high severity and every detail; log expected 4xx at Warning.

Document expected error responses in OpenAPI with `ProducesProblem(400)`, `ProducesProblem(404)`, and so on on endpoints or via global operation filters so generated clients handle each response shape correctly. Placing the mapping in a DI-registered handler keeps it unit-testable independent of HTTP plumbing.

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
