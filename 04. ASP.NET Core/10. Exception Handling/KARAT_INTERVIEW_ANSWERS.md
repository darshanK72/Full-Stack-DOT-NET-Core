# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/10. Exception Handling`

---

#### Q1. (P) How would you implement centralized, uniform exception handling in an ASP.NET Core Web API — including status code mapping, `ProblemDetails` response shape, and different behavior in Development vs Production?

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

#### Q3. (M) In a catch block, what is the difference between `throw;` and `throw ex;`, and why does the choice affect production diagnostics in ASP.NET Core?

**Answer:** `throw;` rethrows the caught exception while preserving its original stack trace and inner-exception chain; `throw ex` throws the same object but resets the stack trace to the current catch location, making the failure appear to originate in your handler.

- Use `throw;` when logging or enriching context and delegating failure upward — Application Insights, Serilog, and OpenTelemetry need the true fault site.
- `throw ex` (or `throw new Exception(ex.Message)`) is appropriate only when intentionally wrapping: `throw new DomainException("…", ex)` to retain inner exceptions with a new outer type.
- Global handlers (`IExceptionHandler`, `UseExceptionHandler`) and exception filters rely on intact stacks to classify errors and populate `ProblemDetails`.
- In async code, the same rule applies in `catch` after `await` — stack preservation matters equally for thread-pool continuations.

**Production takeaway:** Teams that habitually write `throw ex` spend hours tracing bugs that telemetry already captured — if the stack trace were preserved.

---

#### Q4. (P) How should structured logging use the exception object (`LogError(ex, "...")`) vs logging only `ex.Message`? What do you lose in Application Insights or Serilog when you log message-only?

**Answer:** Always pass the exception instance as the first argument to `LogError` so the logging provider captures type, message, stack trace, and inner exceptions as structured fields — not just a plain string concatenated into the message.

- `_logger.LogError(ex, "Payment failed for order {OrderId}", orderId)` emits `Exception`, `StackTrace`, and template properties as queryable columns in Serilog and Application Insights.
- Message-only logging loses inner exceptions (critical for `AggregateException`, `HttpRequestException` wrappers) and prevents "show me all SqlException deadlocks" queries.
- Include correlation identifiers in the message template (`TraceIdentifier`, `Activity.Current?.Id`), not embedded in `ex.Message`.
- Avoid duplicate logging — if a global handler logs unhandled exceptions, service-layer catches should either handle fully or rethrow without re-logging the same fault.

**Production takeaway:** Message-only logging looks sufficient in development consoles but breaks the first time you need to query failures by exception type in a log platform.

---

#### Q5. (P) Register and implement `IExceptionHandler` (.NET 8+) for a global JSON error envelope. Where does it sit relative to `UseExceptionHandler`, and what does `TryHandleAsync` returning `true` vs `false` mean?

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

#### Q7. (D) Compare Development vs Production exception behavior: `DeveloperExceptionPage`, detailed `ProblemDetails`, stack traces in JSON, and what must never leak to external clients. How do you configure both without `#if DEBUG` in controllers?

**Answer:** Use `IHostEnvironment` and conditional middleware registration in `Program.cs` — Development exposes detailed faults for developers; Production logs fully server-side and returns sanitized `ProblemDetails` without stack traces, connection strings, or file paths.

- **Development:** `app.UseDeveloperExceptionPage()` or `AddProblemDetails(o => o.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment())` — rich HTML or JSON detail locally.
- **Production:** `UseExceptionHandler` + `IExceptionHandler` returning generic titles ("An error occurred") and `traceId`; never include `ex.StackTrace` or internal exception types in response body.
- Configure via `if (app.Environment.IsDevelopment()) { … } else { … }` in `Program.cs` — not `#if DEBUG` in controllers.
- `launchSettings.json` sets `ASPNETCORE_ENVIRONMENT=Development` locally; Production must be enforced by hosting platform env vars — mis-set Production to Development exposes stack traces publicly.
- Log full exception server-side regardless of environment: `LogError(ex, ...)`.

**Production takeaway:** Environment-driven pipeline configuration keeps controllers environment-agnostic — a single misconfigured env var is a security incident.

---

#### Q8. (M) An MVC action throws inside an action filter; another failure occurs in custom middleware before routing. Which handlers run — exception filter, `IExceptionHandler`, `UseExceptionHandler` fallback — and in what order?

**Answer:** Middleware exceptions are caught only by exception middleware (`UseExceptionHandler` / `IExceptionHandler`) if registered to wrap that middleware; MVC exception filters run only for exceptions thrown inside the MVC filter/action pipeline after routing — not for failures in early custom middleware.

- **Early middleware failure** (before endpoint execution): exception filters never run; only exception-handler middleware catches it if the failing middleware is downstream of `UseExceptionHandler`'s outer try/catch wrapper — register exception handler **first** (outermost) to wrap the entire pipeline.
- **Action filter / action failure:** MVC invokes exception filters (`IExceptionFilter`) first; if `ExceptionHandled = true`, the exception may not reach middleware; if unhandled, it propagates to exception middleware.
- **`IExceptionHandler` chain:** handlers run in registration order until one returns `true` from `TryHandleAsync`.
- **DeveloperExceptionPage:** only in Development; replaces or supplements handler output for browser requests.
- Minimal API exceptions skip MVC exception filters entirely — only middleware handlers apply.

**Production takeaway:** Relying on exception filters alone leaves middleware and minimal API failures with default 500 HTML — global exception middleware is mandatory for uniform API behavior.

---

#### Q9. (P) Design a status-code mapping table for domain exceptions (`ValidationException` → 400, `NotFoundException` → 404, conflict → 409, unauthorized business rule → 403). Where should mapping live so controllers stay thin and OpenAPI stays accurate?

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
