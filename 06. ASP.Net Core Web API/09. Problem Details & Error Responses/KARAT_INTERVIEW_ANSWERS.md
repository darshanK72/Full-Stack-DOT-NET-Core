# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `07. ASP.Net Core Web API/09. Problem Details & Error Responses`

---

#### Q1. (R) Review this payment service called from a Web API controller. QA sees 500 responses with no correlation id in logs, and Application Insights shows the stack trace starting at the rethrow site — not the gateway call.

**Answer:** The catch block destroys observability by logging only `ex.Message` (no stack trace or inner exception) and rethrowing with `throw ex`, which resets the stack trace to this catch site. The global handler still runs, but support cannot tie logs to the original gateway failure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Observability | `LogError(ex.Message)` — exception object not passed | Serilog/App Insights lose stack, inner exceptions, structured `Exception` property |
| Diagnostics | `throw ex` resets stack trace | Root cause appears to be this service method, not `_gateway.ChargeAsync` |
| Design | Catch-log-rethrow in service layer | Duplicates global handler; message-only logs cannot be correlated with ProblemDetails `traceId` if handler logs properly but service does not |

**Fix (priority order):**

1. Remove the try/catch entirely — let domain/gateway exceptions bubble to `IExceptionHandler` or `UseExceptionHandler` (preferred for Web APIs).
2. If local handling is required: `_logger.LogError(ex, "Payment charge failed for {OrderId}", orderId);` then **`throw;`** (preserves stack).
3. Map gateway-specific faults to typed exceptions (`PaymentDeclinedException`) in one place — the global handler — not in every service.
4. Ensure middleware adds/propagates `Activity`/`TraceIdentifier` so client-facing `ProblemDetails.Extensions["traceId"]` matches server logs.

**Production takeaway:** See C# Module 01 — **`throw;` vs `throw ex`**. Web API services should throw typed domain errors; centralized handlers own HTTP shape. Debrief pattern: `LogError(ex.Message)` + `throw ex`.

---

#### Q2. (P) You inherit a Web API with per-controller try/catch blocks returning `{ error = ex.Message }`, plain strings for 404, and occasional 500 for validation failures. How do you move to centralized exception handling with RFC 7807 `ProblemDetails`, consistent status codes, and different Development vs Production response bodies?

**Answer:** Register one global path (`AddProblemDetails`, `AddExceptionHandler<T>()`, `UseExceptionHandler()`), delete controller catch blocks for unexpected faults, map domain exceptions to status codes in the handler, and use environment-based `ProblemDetails` customization so Development can include extensions without leaking stacks in Production.

- **`builder.Services.AddProblemDetails()`** — configures RFC 7807 defaults and optional customizers.
- **`builder.Services.AddExceptionHandler<GlobalExceptionHandler>()`** + **`app.UseExceptionHandler()`** early in the pipeline (after routing setup, wrapping endpoints) — handler writes JSON `ProblemDetails` for all unhandled exceptions.
- **Status mapping in handler:** `NotFoundException` → 404, `ValidationException` → 400, conflicts → 409; never return 500 for expected domain validation.
- **Development vs Production:** In `ProblemDetails` options or handler, when `IHostEnvironment.IsDevelopment()`, add safe diagnostic extensions (exception type, stack in extension field); Production returns `title`, `detail` (sanitized), `status`, `type`, `instance`, and `traceId` only.
- **Controllers:** `[ApiController]` + let validation produce automatic `ValidationProblemDetails`; remove `{ error = ex.Message }` catches.
- **OpenAPI:** Document `ProducesResponseType(typeof(ProblemDetails), 400/404/409/500)` on base controller or filter.

**Production takeaway:** Centralized handling is the Web API contract — ad hoc JSON shapes break mobile clients and API gateways. Cross-ref: **05. ASP.NET Core/10. Exception Handling** Q1.

---

#### Q3. (M) A mobile client parses error JSON using RFC 7807. Review this `[ApiController]` action and the actual response body when model validation fails.

**Answer:** The action method never runs. `[ApiController]` triggers automatic model validation; a missing required `OrderLines` yields **HTTP 400 Bad Request** with **`ValidationProblemDetails`** JSON — not 200 and not a custom shape.

- **Producer:** `ModelStateInvalidFilter` (part of `[ApiController]` conventions) short-circuits before `Create` executes.
- **Body shape:** RFC 7807 fields (`type`, `title`, `status`, `traceId`) plus **`errors`** dictionary — keys are property names, values are string arrays of messages.
- **Client parsing:** Read `status` (400), iterate `errors["OrderLines"]`, use `title`/`detail` for display; do not assume `{ error: "..." }` unless you customize `InvalidModelStateResponseFactory`.
- **422 vs 400:** Default ASP.NET Core uses **400** for model binding/validation failures; 422 is optional via `ConfigureApiBehaviorOptions` if the team standardizes on Unprocessable Entity.

**Production takeaway:** `[ApiController]` validation is a **separate error path** from exception middleware — both must emit ProblemDetails for a uniform client experience.

---

#### Q4. (R) Review this controller catch blocks. Swagger documents 400/404/422, but production clients receive inconsistent shapes and wrong status codes.

**Answer:** Each catch returns a different contract and **`ValidationException` incorrectly maps to 500**. Clients cannot write one deserializer; monitoring alerts treat validation bugs as server faults.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| HTTP semantics | `ValidationException` → 500 | Client retries; SLO violations for user input errors |
| Contract | `NotFound(ex.Message)` — string body, not ProblemDetails | Breaks RFC 7807 clients expecting JSON object |
| Consistency | Conflict returns `ProblemDetails`; others do not | Partner integrations need branch per endpoint |
| Architecture | Controller-level catch duplicates global handler | New endpoints will copy wrong patterns |

**Fix (priority order):**

1. Remove catches for domain exceptions — throw `NotFoundException`, `ValidationException`, `DuplicateOrderException` and handle in `IExceptionHandler`.
2. If staying in controller: all branches return `ProblemDetails` or `ValidationProblemDetails` with correct status (400/404/409).
3. Register `services.AddProblemDetails()` and use `TypedResults.Problem()` / `Results.Problem()` in minimal APIs for the same shape.
4. Align OpenAPI response types with actual `ProblemDetails` schema for every documented status.

**Production takeaway:** Status code consistency matters as much as JSON shape — 500 on validation trains clients to retry forever.

---

#### Q5. (D) Compare Development vs Production error responses for the same unhandled `NullReferenceException` in a minimal API endpoint.

**Answer:** Both environments should return **500** with a generic, safe message to external clients; Development may attach richer diagnostics through `ProblemDetails` customization or `DeveloperExceptionPage` for browser traffic, but Production must never expose stack traces, file paths, or connection strings in JSON.

| Aspect | Development | Production |
|---|---|---|
| Status | 500 Internal Server Error | 500 |
| `title` / `detail` | Can include exception type name in extension | Generic "An error occurred" — no null reference hints that reveal code paths |
| Stack trace | Allowed in `ProblemDetails.Extensions["stackTrace"]` or dev page | **Never** in response body |
| Logging | `LogError(ex, "...")` with full stack + `TraceIdentifier` | Same — full detail server-side only |
| Configuration | `AddProblemDetails` customize delegate checks `IsDevelopment()` | Same handler, stricter detail sanitization |

- Use **`app.UseExceptionHandler()`** + **`AddExceptionHandler<T>`** — not `#if DEBUG` in endpoints.
- Optional: `DeveloperExceptionPage` only for local browser debugging; APIs should still use ProblemDetails JSON.
- Include **`traceId`** / **`requestId`** in Production responses so support maps client reports to logs without exposing internals.

**Production takeaway:** Dev-friendly diagnostics belong in **logs and secure extensions**, not in Production response bodies — a common Karat distinction for Web APIs.

---

#### Q6. (P) Implement global handling with `IExceptionHandler` (.NET 8+) for a JSON Web API.

**Answer:** Register the handler in DI, enable ProblemDetails services, and place exception middleware so it wraps endpoint execution. `TryHandleAsync` returning **`true`** means this handler wrote the response and pipeline stops; **`false`** delegates to the next registered handler or default behavior.

```csharp
// Program.cs
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.UseExceptionHandler(); // early — after UseRouting if you need route data in handler
app.MapControllers();

// GlobalExceptionHandler.cs
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception ex, CancellationToken ct)
    {
        logger.LogError(ex, "Unhandled exception {TraceId}", context.TraceIdentifier);

        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex is NotFoundException ? ex.Message : "An unexpected error occurred.",
            Instance = context.Request.Path
        }, ct);

        return true; // handled — do not rethrow
    }
}
```

- Multiple handlers: register with `AddExceptionHandler<T>()` in order — first handler returning `true` wins.
- **`UseExceptionHandler()`** without path uses registered `IExceptionHandler` implementations (.NET 8+).
- Controllers stay thin — they throw; handler owns HTTP mapping.

**Production takeaway:** `IExceptionHandler` replaces custom middleware for exception-to-ProblemDetails mapping in modern Web APIs.

---

#### Q7. (R) Two teams merged their APIs. Support tickets report "sometimes 404 is JSON ProblemDetails, sometimes plain text."

**Answer:** `[ApiController]` on Team A enables automatic ProblemDetails for `NotFound()`; Team B's controller lacks it, so `NotFound(string)` returns **text/plain** (or negotiated string) with the same 404 status — identical status code, incompatible bodies.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API contract | Missing `[ApiController]` on LegacyReportsController | 404 body is raw string, not RFC 7807 |
| Consistency | Mixed controller conventions on one host | Client parsers fail intermittently |
| OpenAPI | Documented ProblemDetails schema does not match Legacy routes | Generated clients break at runtime |

**Fix (priority order):**

1. Add `[ApiController]` to all API controllers **or** standardize on `return NotFound(new ProblemDetails { ... })` / `Results.Problem(statusCode: 404)`.
2. Configure global **`InvalidModelStateResponseFactory`** and a filter ensuring all `ObjectResult` error paths use ProblemDetails.
3. Add integration tests asserting 404/400 response `Content-Type: application/problem+json` (or `application/json` with ProblemDetails shape).
4. Document unified error schema in OpenAPI under `components/schemas/ProblemDetails`.

**Production takeaway:** `[ApiController]` is not cosmetic — it changes **error response content negotiation** for the same helper methods.

---

#### Q8. (M) Partner sends invalid email on `POST /api/customers`. Walk through validation pipeline and `ValidationProblemDetails`.

**Answer:** Model binding deserializes the body, validation runs (`[EmailAddress]` / `[Required]`), `ModelState` is invalid, and `[ApiController]`'s filter returns **400** with **`ValidationProblemDetails`** before the action executes.

- **Pipeline order:** Routing → model binding → validation attributes → **`ModelStateInvalidFilter`** (automatic 400).
- **Status:** Default **400 Bad Request** (not 422 unless configured).
- **`ValidationProblemDetails` vs custom object:** Subclass of `ProblemDetails` with **`Errors`** property (`IDictionary<string, string[]>`) — field-scoped messages. Custom `{ errors: [...] }` lacks standard `title`, `status`, `type`, and breaks ProblemDetails clients.
- **Customization:** `services.Configure<ApiBehaviorOptions>(o => o.InvalidModelStateResponseFactory = ctx => new BadRequestObjectResult(...))` — still prefer ValidationProblemDetails subclass for RFC alignment.

**Production takeaway:** Automatic validation is the first line of ProblemDetails consistency — global exception handler does not replace it.

---

#### Q9. (D) Design a status-code mapping table for domain exceptions. Where should mapping live?

**Answer:** Map exceptions to HTTP status in a single **`IExceptionHandler`** (or dedicated middleware), keep controllers throwing domain types only, log full exceptions in the handler with `LogError(ex, ...)`, and reference the same status list in OpenAPI via attributes or document filter.

| Domain exception | HTTP status | ProblemDetails `title` | Client `detail` |
|---|---|---|---|
| `ValidationException` / invalid argument | 400 | Validation failed | Safe field-level via extensions or ValidationProblemDetails |
| `NotFoundException` | 404 | Resource not found | Entity id/type message OK |
| Duplicate / concurrency | 409 | Conflict | Explain conflict without internal ids |
| Forbidden business rule | 403 | Forbidden | No sensitive policy internals |
| Unexpected | 500 | Server error | Generic; full ex in logs only |

- **Location:** `GlobalExceptionHandler.TryHandleAsync` switch expression — not duplicated per controller.
- **Controllers:** `throw new NotFoundException("Customer", id);` — no HTTP knowledge.
- **OpenAPI:** Base `[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]` on controller or Swashbuckle operation filter reading same mapping.
- **Logging:** Always log unexpected 500 with exception object; map 400/404 at Information/Debug if desired.

**Production takeaway:** Status code consistency is enforced by **one mapping layer** — scattering `StatusCode(...)` in controllers guarantees drift.
