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

## Gotchas — Exception Handling (Interview Traps)

---

#### Gotcha 1. Exception handling middleware must be the first registration — exceptions before it propagate unhandled

**Concepts**
- Middleware pipeline as nested delegates — first registered is outermost wrapper
- Exceptions from routing, auth, and action pipeline only caught if handler wraps them
- `UseExceptionHandler` or `UseDeveloperExceptionPage` placed before all other middleware
- Host-level unhandled exception logging as last resort

**Answer**

The middleware pipeline runs as a nested chain of delegates — the first registered middleware is the outermost layer. Exception handling middleware catches exceptions thrown by all delegates that run inside it. Any middleware registered before `UseExceptionHandler` is outside the exception handler's catch scope — exceptions from those early middlewares propagate directly to the host. The canonical `Program.cs` ordering places `UseExceptionHandler` (or `UseDeveloperExceptionPage` in development) as the very first `app.Use*()` call so that errors from routing, authentication, authorization, and action execution are all intercepted by a single centralized handler.

---

#### Gotcha 2. `UseDeveloperExceptionPage` must be gated to Development — leaks stack traces in production

**Concepts**
- `UseDeveloperExceptionPage` rendering full exception details as HTML
- Environment check `app.Environment.IsDevelopment()` required
- Stack trace and inner exception details visible to any user in production
- Security impact of exposing framework internals and connection strings in error pages

**Answer**

`UseDeveloperExceptionPage` renders a detailed HTML page with the full stack trace, exception message, query string, headers, and cookies for any unhandled exception. Without an environment check, this page is served to every user in production — exposing stack traces, connection string fragments, internal paths, and framework version information that attackers use to craft targeted exploits. The page must be conditionally registered: `if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage(); else app.UseExceptionHandler("/error")`. Never invert this condition or remove the environment check to "make debugging easier" in staging or production.

---

#### Gotcha 3. `UseExceptionHandler` vs `IExceptionHandler` — .NET 8 prefers the interface-based approach

**Concepts**
- `UseExceptionHandler("/error")` routing to a controller action for error responses
- `IExceptionHandler` interface registered in DI for structured exception mapping
- Multiple `IExceptionHandler` implementations tried in registration order
- `AddProblemDetails()` combined with `IExceptionHandler` for consistent response shape

**Answer**

The traditional `UseExceptionHandler("/error")` re-executes the request to a controller action, which can be confusing — the action runs with the original request's authorization context and must detect that it is an error response via `IExceptionHandlerFeature`. In .NET 8, the preferred approach is implementing `IExceptionHandler`, registering it with `services.AddExceptionHandler<MyHandler>()`, and calling `app.UseExceptionHandler()` without a path. Multiple handlers are tried in registration order; the first that handles the exception by returning `true` wins. This approach is cleaner, testable, and integrates naturally with `AddProblemDetails()` for RFC 7807-compliant error responses.

---

#### Gotcha 4. Exception handlers must check `context.Response.HasStarted` before writing a response

**Concepts**
- Response body bytes committed after first write — headers already sent
- `HttpResponse.HasStarted` flag for detecting committed responses
- Exception during streaming response — cannot replace response
- Logging as the only action available when response has started

**Answer**

If an exception occurs after the response body has started being written — during streaming, server-sent events, or a chunked response — the response headers have already been sent and cannot be changed. An exception handler that tries to write a 500 `ProblemDetails` response throws `InvalidOperationException: Headers are read-only, response has already started`. Every exception handler must check `context.Response.HasStarted` and, if true, log the error and either abort the connection or do nothing rather than attempting to write a new response. The `IExceptionHandler` interface pattern handles this correctly when combined with the built-in middleware.

---

#### Gotcha 5. Swallowing exceptions in catch blocks — logging without re-throwing silently hides failures

**Concepts**
- `catch` block logging and returning success masking actual failure
- Silent data corruption when partial operations succeed before the exception
- Callers receiving success responses for failed operations
- Deliberate swallowing requiring explicit documentation and design justification

**Answer**

A common antipattern is catching an exception, logging it, and then returning a success response or continuing execution — the exception is silently swallowed. For database operations, this means a partial write might have occurred, and the caller receives a 200 OK for an operation that failed midway. If an exception should not propagate to the caller, the handler must return an explicit error response using the exception's context, not a generic success. The only valid case for swallowing is when the exception represents a truly optional operation where failure is acceptable — and that decision must be documented. Always re-throw after logging unless there is a deliberate architectural reason not to.

---

#### Gotcha 6. `throw ex` resets the stack trace — always use bare `throw` to preserve it

**Concepts**
- `throw ex` resetting stack trace to the catch block line
- `throw` (bare) preserving original exception origin in stack trace
- `InnerException` for wrapping with additional context
- APM tools relying on accurate stack traces for root-cause analysis

**Answer**

`throw ex` inside a catch block replaces the exception's stack trace with the current catch block location, hiding the line where the original failure occurred. Exception filters, middleware, Application Insights, and Serilog then show the re-throw site as the error origin rather than the actual problem location — making production root-cause analysis significantly harder. Always use bare `throw;` to rethrow without modifying the stack trace. The only valid reason to create a new exception is to add domain context: `throw new PaymentProcessingException("Card declined", ex)` preserves the original as `InnerException` while exposing a meaningful domain type to callers and observers.

---

#### Gotcha 7. Business rule violations should return 4xx, not 5xx — `HttpStatusCode` mapping matters

**Concepts**
- 500 status code indicating unexpected server error vs predictable business failure
- 422 Unprocessable Entity for semantically valid but business-rule-failing requests
- 400 Bad Request for input validation failures
- Domain exception hierarchy mapping to HTTP status codes

**Answer**

An exception like `InsufficientInventoryException` or `OrderAlreadyShippedException` represents a predictable business rule violation that the client should handle — it is not an unexpected server error. Returning 500 for these exceptions misleads clients (and monitoring systems) that 500 means an unexpected infrastructure failure. The convention is: 400 for input that fails validation, 404 for resources not found, 409 for state conflicts, 422 for semantically valid requests that violate business rules. Map domain exception types to appropriate 4xx status codes in `IExceptionHandler`, and reserve 500 for genuinely unexpected exceptions. A well-designed exception hierarchy makes this mapping clean and maintainable.

---

#### Gotcha 8. Exception filters do not catch exceptions from middleware — middleware exceptions require middleware-level handling

**Concepts**
- Exception filter scope limited to MVC action pipeline
- Exceptions from `UseRouting`, `UseAuthentication`, or custom middleware not caught by exception filters
- `UseExceptionHandler` as the global safety net for all pipeline exceptions
- Complementary roles of exception filters and exception handling middleware

**Answer**

Exception filters only catch exceptions that escape the MVC action execution pipeline — they have no visibility into exceptions thrown in upstream middleware or in the routing process. An exception thrown in `UseAuthentication`, `UseRateLimiter`, or a custom middleware runs outside the MVC filter pipeline entirely and propagates past any registered exception filters without them being invoked. `UseExceptionHandler` middleware wraps the entire pipeline and is the correct place for global exception handling. Exception filters are for per-controller or per-action exception-to-response mapping (e.g., catching `DbUpdateConcurrencyException` on specific actions) as a complement to global exception handling, not a replacement.

---

#### Gotcha 9. `ProblemDetails` `type` field should be a URI — random strings break RFC 7807 compliance

**Concepts**
- RFC 7807 `type` as a URI referencing documentation or error catalog
- `about:blank` as the default `type` when no specific URI applies
- `status` field required — other fields optional but recommended
- Client error `type` URIs enabling machine-readable error handling

**Answer**

RFC 7807 specifies that the `type` field in `ProblemDetails` should be a URI — ideally a URL linking to documentation for that error type, or `about:blank` when no documentation URI is available. Developers sometimes set `type` to a random string like `"VALIDATION_ERROR"` or leave it null, which breaks clients that use `type` as a machine-readable discriminator for error handling logic. A consistent URL scheme like `https://api.example.com/errors/validation` or `https://tools.ietf.org/html/rfc7231#section-6.5.1` (for standard HTTP errors) makes the contract explicit and allows API consumers to build type-safe error handling. Ensure `status` is always populated — it is required by the RFC and many clients rely on it.

---

#### Gotcha 10. Async exception handlers — not awaiting async work inside `catch` blocks causes silent swallowing

**Concepts**
- `async void` exception handlers not propagating exceptions to callers
- `await` required inside async catch blocks for correct exception propagation
- Fire-and-forget logging in exception handlers losing exceptions
- `Task.Run` in exception handlers detaching from the request context

**Answer**

Writing `catch (Exception ex) { _ = LogExceptionAsync(ex); return BadRequest(); }` inside a controller or middleware silently detaches the logging operation — if `LogExceptionAsync` throws, that exception is unobserved and silently lost. Additionally, `async void` exception handler methods that throw inside an `await` cause unhandled exceptions to crash the process on .NET because there is no task to propagate the exception to. Inside `catch` blocks, always `await` async operations, including logging calls. If the exception handler itself must be fire-and-forget (e.g., sending to an external system), wrap it in a try-catch internally and ensure the error-handling path itself cannot throw unobserved exceptions.

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
