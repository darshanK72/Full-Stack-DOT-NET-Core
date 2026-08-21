# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `05. ASP.NET Core/06. Logging & Diagnostics`

---

#### Q1. (R) Review this exception handling in a payment service. Support cannot find stack traces in Application Insights after incidents.

```csharp
public async Task ChargeAsync(Guid orderId, CancellationToken ct)
{
    try
    {
        await _gateway.ChargeAsync(orderId, ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex.Message);
        throw ex;
    }
}
```

**Answer:** Logging only `ex.Message` drops the exception object (and stack trace) from structured logs, and `throw ex` resets the stack trace to this catch site — both destroy observability for production triage.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Logging | `LogError(ex.Message)` string overload | No exception recorded in App Insights / Seq — stack trace missing |
| Observability | `throw ex` instead of `throw` | Stack trace shows catch block as root, hiding original failure site |
| Alerting | Message-only logs | Duplicate messages from different failures look identical |

**Fix (priority order):**

1. Log the exception: `_logger.LogError(ex, "Charge failed for order {OrderId}", orderId);`
2. Rethrow without resetting stack: `throw;` (or omit catch if only logging upstream).
3. Add correlation scope with `orderId` before the try block.

**Production takeaway:** See C# Module — `throw` vs `throw ex`; Karat pairs this with logging every time.

---

#### Q2. (R) Review this controller action. Logs in Seq show duplicate lines with no way to tie gateway, repository, and controller entries to one customer checkout.

```csharp
[HttpPost("checkout")]
public async Task<IActionResult> Checkout(CheckoutRequest request, CancellationToken ct)
{
    _logger.LogInformation("Checkout started for {Email}", request.Email);
    var order = await _orderService.CreateAsync(request, ct);
    _logger.LogInformation("Checkout completed order {OrderId}", order.Id);
    return Ok(order);
}
```

**Answer:** Without a shared logging scope (correlation ID / trace ID), each layer logs isolated events — you cannot filter one checkout flow across services in Seq or Application Insights.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correlation | No `BeginScope` with trace/correlation ID | Cannot stitch controller + service + DB logs for one request |
| PII | `{Email}` in Information logs | GDPR/PCI concern; also noisy identifier for correlation |
| Cross-service | Downstream HTTP calls lack propagated headers | Distributed traces break at first outbound call |

**Fix (priority order):**

1. Add middleware that sets scope: `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier }))` for the request pipeline.
2. Propagate `traceparent` / custom header on `HttpClient` via `IHttpClientFactory` and OpenTelemetry.
3. Log `OrderId` or hashed user id — not raw email — at Information level.

**Production takeaway:** Scopes turn a pile of log lines into one request story — required for any multi-layer API.

---

#### Q3. (P) Production `appsettings.Production.json` sets `"Default": "Debug"` for logging while Development uses `"Information"`. What risks does this create, and what levels would you configure per namespace for a public API?

**Answer:** Debug in production floods sinks with per-request noise, increases cost, may log sensitive payloads, and hides Critical/Error signals — production should default to Warning or Information with tighter overrides per namespace.

- **Default:** `Information` for `Microsoft.AspHost` / app namespace; `Warning` for `Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore` (or `Warning` for EF SQL unless debugging).
- **Never Debug globally in prod** — use short-lived dynamic log level (App Insights adaptive sampling, `LoggingLevelSwitch`, or Azure App Configuration) for targeted investigations.
- Risks: PII in Debug templates, I/O overhead on hot paths, log storage bill, alert fatigue masking payment failures.
- Use **category overrides**: `"YourApp.Payments": "Information"`, `"YourApp": "Warning"` for verbose subsystems only when needed.
- Pair levels with **sampling** in Application Insights so high-volume Information is retained statistically.

**Production takeaway:** Karat tests operational judgment — Debug in prod is a common post-incident finding, not a best practice.

---

#### Q4. (R) Review this audit logging added for a login endpoint. Security flags the change in PR review.

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    _logger.LogInformation(
        "Login attempt user={User} password={Password} ip={Ip}",
        request.Username,
        request.Password,
        HttpContext.Connection.RemoteIpAddress);
    // ...
}
```

**Answer:** Logging passwords — even failed attempts — writes credentials to persistent log stores where retention, access control, and compliance (PCI, SOC2) are violated; audit logs should record outcome and non-sensitive identifiers only.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| PII / secrets | `password={Password}` in log template | Credentials stored in Seq/Splunk/App Insights indefinitely |
| Security | Logs often have broader read access than DB | Credential leak via support dashboards |
| Compliance | Authentication secrets in log aggregation | Audit failure; mandatory rotation and incident response |

**Fix (priority order):**

1. Remove password from all log statements — never log secrets, tokens, or full PAN.
2. Log `Username` (or hashed subject id), result (`Succeeded`/`Failed`), and `Ip` at Information; use Warning for lockout patterns.
3. Add redaction middleware or Serilog `DestructuringPolicy` to strip known sensitive property names globally.

**Production takeaway:** Structured logging makes exfiltration easier — templates must be reviewed like database schemas.

---

#### Q5. (P) A team wants distributed traces from ASP.NET Core through an outbound `HttpClient` call to a downstream pricing service. Outline the OpenTelemetry setup in `Program.cs` and what must the outbound call participate in for trace continuity.

**Answer:** Add OpenTelemetry tracing with ASP.NET Core and `HttpClient` instrumentation so `Activity` context flows from inbound request to outbound `traceparent` header automatically when using `IHttpClientFactory`.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter()); // or Azure Monitor exporter

builder.Services.AddHttpClient<IPricingClient, PricingClient>();
```

- `AddAspNetCoreInstrumentation()` creates a server span per request; `AddHttpClientInstrumentation()` injects W3C trace context on outbound calls.
- Use **`IHttpClientFactory`** — manually `new HttpClient()` bypasses handler chain and often breaks propagation.
- Export to OTLP collector, Jaeger, or Application Insights; align resource attributes (`service.name`) for service map.
- Logs link to traces via `Activity.TraceId` in scopes when using `OpenTelemetryLoggerProvider` or App Insights integration.

**Production takeaway:** Tracing is not "install NuGet" — outbound calls must use instrumented `HttpClient` handlers.

---

#### Q6. (R) Review this high-traffic endpoint logging. Log volume spikes cost and hides real errors in noise.

```csharp
[HttpGet("products")]
public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(CancellationToken ct)
{
    _logger.LogInformation($"GetProducts called at {DateTime.UtcNow}");
    var products = await _repo.GetAllAsync(ct);
    foreach (var p in products)
        _logger.LogDebug("Product: " + p.Name + " price=" + p.Price);
    _logger.LogInformation("Returning " + products.Count + " products");
    return Ok(products);
}
```

**Answer:** String interpolation (`$"..."` and `+`) defeats structured logging — messages are not template-based (hurting aggregation), Debug lines in loops multiply volume, and Information on every catalog read is unnecessary noise in production.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Structured logging | `$"GetProducts called at {DateTime.UtcNow}"` | Parameters not captured as fields; poor queryability in Seq/Kusto |
| Volume | Information on every GET | High-cost hot path; masks real errors |
| Loop logging | Debug per product in loop | Thousands of lines per request if Debug enabled |

**Fix (priority order):**

1. Remove routine success logs or gate at Debug with message templates: `_logger.LogDebug("Returning {Count} products", products.Count);`
2. Use constant templates only — `_logger.LogInformation("GetProducts completed {Count}", products.Count);` — no `$` interpolation.
3. Consider `LoggerMessage` source generators for high-frequency paths.

**Production takeaway:** If everything is Information, nothing is — Karat tests structured logging discipline, not syntax.

---

#### Q7. (M) Explain how `ILogger.BeginScope` (or `LoggerMessage` scopes) propagates correlation IDs across async calls in a request, and where you would set the initial `TraceIdentifier` or custom `CorrelationId` in the ASP.NET Core pipeline.

**Answer:** `BeginScope` attaches key-value pairs to the logical logging context for the current async flow; ASP.NET Core already assigns `HttpContext.TraceIdentifier` per request — middleware should open a scope early so all downstream `ILogger` calls inherit the same correlation id without passing it manually.

```csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? context.TraceIdentifier;
    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId
    }))
    {
        await next();
    }
});
```

- Scopes flow across `await` in the same request when using the default `AsyncLocal` logger scope provider.
- Place scope middleware **after** routing (so you have endpoint metadata) but **before** controllers and exception handlers.
- OpenTelemetry `Activity.Current` complements scopes — align `CorrelationId` with `Activity.TraceId` for log-trace linking.
- Background work started with `Task.Run` without `IHttpContextAccessor` does **not** inherit HTTP scope — pass correlation id explicitly.

**Production takeaway:** One middleware scope beats adding `{CorrelationId}` to every log line manually.

---

#### Q8. (D) An on-call alert fires on `LogCritical` from a background worker when a queue is full, but the same team ignores `LogError` in controllers. How do you define log level policy, sampling, and alert thresholds so production signal stays actionable?

**Answer:** Define a level contract: **Critical** = immediate human page (service down, data loss); **Error** = failed operation requiring ticket but not always page; **Warning** = degraded; reserve alerts for SLO-burning rates, not single Error lines in controllers.

- Document team semantics in runbooks — `LogCritical` on queue full is valid if it blocks orders; controller `LogError` on 404-like business cases should be Warning or Information.
- Alert on **rates** (N Critical per 5 min, Error spike 3× baseline) not single events — Application Insights alerts, Prometheus recording rules.
- Use **sampling** for high-volume Information; never sample Error/Critical by default.
- Separate **audit logs** (security) from **diagnostic logs** (engineering) with different retention and alert routes.
- Review weekly: top Error messages — fix root cause or downgrade intentional paths to Warning.

**Production takeaway:** Karat tests whether you treat logging as an operational API — levels drive paging, not personal preference.

---
