# Logging & Diagnostics — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is `ILogger<T>` in ASP.NET Core?](#q1-what-is-iloggert-in-aspnet-core)
2. [Q2. How is logging configured in ASP.NET Core?](#q2-how-is-logging-configured-in-aspnet-core)
3. [Q3. What are the standard log levels in .NET logging?](#q3-what-are-the-standard-log-levels-in-net-logging)
4. [Q4. What is structured logging?](#q4-what-is-structured-logging)
5. [Q5. Why should you use message templates instead of string interpolation in log calls?](#q5-why-should-you-use-message-templates-instead-of-string-interpolation-in-log-calls)
6. [Q6. What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?](#q6-what-is-the-difference-between-_loggerlogerrorexmessage-and-_loggerlogerrorex)
7. [Q7. What is the difference between `throw;` and `throw ex;` in a catch block?](#q7-what-is-the-difference-between-throw-and-throw-ex-in-a-catch-block)
8. [Q8. What is `ILogger.BeginScope`, and what is it used for?](#q8-what-is-iloggerbeginscope-and-what-is-it-used-for)
9. [Q9. How does ASP.NET Core assign a `TraceIdentifier` to each request?](#q9-how-does-aspnet-core-assign-a-traceidentifier-to-each-request)
10. [Q10. What is a correlation ID, and where is it typically set?](#q10-what-is-a-correlation-id-and-where-is-it-typically-set)
11. [Q11. How do you configure log levels per namespace in `appsettings.json`?](#q11-how-do-you-configure-log-levels-per-namespace-in-appsettingsjson)
12. [Q12. What logging providers ship with ASP.NET Core by default?](#q12-what-logging-providers-ship-with-aspnet-core-by-default)
13. [Q13. What is OpenTelemetry, and how does it relate to ASP.NET Core?](#q13-what-is-opentelemetry-and-how-does-it-relate-to-aspnet-core)
14. [Q14. What is distributed tracing?](#q14-what-is-distributed-tracing)
15. [Q15. What should you never log in a production application?](#q15-what-should-you-never-log-in-a-production-application)
16. [Q16. What is the difference between logging and diagnostics?](#q16-what-is-the-difference-between-logging-and-diagnostics)
17. [Q17. How does Application Insights integrate with ASP.NET Core logging?](#q17-how-does-application-insights-integrate-with-aspnet-core-logging)
18. [Q18. What is `LoggerMessage` source generators, and why use them?](#q18-what-is-loggermessage-source-generators-and-why-use-them)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is `ILogger<T>` in ASP.NET Core?

What is `ILogger<T>` in ASP.NET Core?

**Answer:** `ILogger<T>` is a generic logging abstraction registered in DI where the type parameter `T` sets the log category (typically the consuming class name). It wraps the underlying `Microsoft.Extensions.Logging` pipeline and is the standard way application and framework code emit structured log entries.

- Category names appear in filters (`Logging:LogLevel:YourApp.Services.OrderService`) for per-namespace level control.
- Methods include `LogInformation`, `LogWarning`, `LogError`, and overloads that accept exceptions and structured parameters.
- `ILogger<T>` is registered as a singleton factory; each `T` gets a category-specific logger without manual registration.
- Prefer injecting `ILogger<MyService>` over non-generic `ILogger` so categories are precise in production log queries.

---

## Q2. How is logging configured in ASP.NET Core?

How is logging configured in ASP.NET Core?

**Answer:** Logging is configured through the `Logging` section of configuration (commonly `appsettings.json`) and optional code-based provider registration in `Program.cs`. The host adds default providers and reads minimum levels and category overrides from configuration at startup.

- Set default and per-namespace levels: `"Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } }`.
- Add providers explicitly with `builder.Logging.AddConsole()`, `AddDebug()`, or third-party sinks (Serilog, Application Insights).
- `ClearProviders()` removes defaults when you want full control — typical in tests or custom Serilog bootstrap.
- Log level filters apply before messages reach providers, reducing overhead for noisy framework categories.

---

## Q3. What are the standard log levels in .NET logging?

What are the standard log levels in .NET logging?

**Answer:** .NET defines six log levels ordered by severity: Trace, Debug, Information, Warning, Error, and Critical, plus None to disable logging. A log call is emitted only when its level is greater than or equal to the effective minimum level for that logger category.

- **Trace** — detailed diagnostic flow, rarely enabled in production.
- **Debug** — developer-oriented diagnostic information useful during local troubleshooting.
- **Information** — general application flow events (request handled, order created).
- **Warning** — unexpected but recoverable situations (retry, deprecated API use).
- **Error** — failures in the current operation that were handled or returned to the caller.
- **Critical** — unrecoverable application or system failures requiring immediate attention.

---

## Q4. What is structured logging?

What is structured logging?

**Answer:** Structured logging records events as typed name-value properties attached to a message template rather than as a single formatted string. Log sinks (Seq, Application Insights, Elasticsearch) can index and query on `OrderId`, `UserId`, or `ElapsedMs` without fragile regular expressions.

- Use templates: `_logger.LogInformation("Order {OrderId} shipped in {ElapsedMs} ms", orderId, elapsed);`
- Properties are stored as fields in the logging backend, enabling filters like `OrderId == 'abc'` across all log levels.
- Structured data supports aggregation, alerting, and correlation across services in centralized logging systems.
- Exception objects passed to logging overloads capture type, message, and stack trace as structured exception details when the provider supports it.

---

## Q5. Why should you use message templates instead of string interpolation in log calls?

Why should you use message templates instead of string interpolation in log calls?

**Answer:** Message templates defer formatting to the logging provider and preserve parameter names as structured properties, while string interpolation (`$"Order {id}"`) always allocates the final string even when the log level is disabled. Templates also enable consistent property indexing in Application Insights and Seq.

- `_logger.LogDebug("Processing {OrderId}", orderId)` skips string building when Debug is disabled for that category.
- `$"Processing {orderId}"` evaluates immediately, wasting CPU and allocations on hot paths at Information default levels.
- Templates keep property names explicit for queries (`OrderId`) instead of parsing a concatenated sentence.
- Some providers use template hashing for high-performance logging (`LoggerMessage` source generators build on this model).

---

## Q6. What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?

What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?

**Answer:** Passing only `ex.Message` logs a plain string with no exception object attached, so sinks lose stack trace, inner exceptions, and structured exception metadata. The overload `_logger.LogError(exception, messageTemplate, ...)` records the full exception for diagnostics and still supports structured message properties.

- `LogError(ex.Message)` uses a string overload — Application Insights and Seq show text without exception details.
- `LogError(ex, "Payment failed for {OrderId}", orderId)` stores the exception type, stack, and custom properties together.
- Support teams searching by exception type or stack frame cannot triage effectively when only `.Message` was logged.
- Always pass the exception instance as the first argument to error/critical logging overloads when an exception exists.

---

## Q7. What is the difference between `throw;` and `throw ex;` in a catch block?

What is the difference between `throw;` and `throw ex;` in a catch block?

**Answer:** `throw;` rethrows the caught exception while preserving the original stack trace, so logs and debuggers point to the true failure site. `throw ex;` throws the same exception object but resets the stack trace to the current catch line, hiding where the error originally occurred.

- Use `throw;` when logging and rethrowing, or when a catch block cannot handle the error and must propagate it.
- `throw ex;` makes production incidents look like the bug is in middleware or a generic handler instead of the root cause.
- Filtering and alerting on stack traces become misleading when `throw ex` is used in shared exception handling code.
- If you do not need to catch, omit the try/catch and let global exception middleware log once with a preserved stack.

---

## Q8. What is `ILogger.BeginScope`, and what is it used for?

What is `ILogger.BeginScope`, and what is it used for?

**Answer:** `BeginScope` adds contextual properties to all log entries written within a `using` block on that logger, until the scope is disposed. It is used to attach correlation IDs, tenant IDs, or operation names so related log lines share queryable fields across layers.

- Example: `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id })) { ... }`
- ASP.NET Core request logging scopes often include `RequestId`, `TraceIdentifier`, or path information automatically in some providers.
- Scopes nest; inner scopes add properties without removing outer scope values unless keys collide.
- Middleware that establishes a correlation scope at request start ties controller, service, and repository logs to one customer action.

---

## Q9. How does ASP.NET Core assign a `TraceIdentifier` to each request?

How does ASP.NET Core assign a `TraceIdentifier` to each request?

**Answer:** When a request is received, the host generates a unique string and stores it on `HttpContext.TraceIdentifier` for the lifetime of that request. Loggers, diagnostics, and developer exception pages include this value so operators can correlate all activity for a single HTTP transaction.

- The identifier is available before middleware runs and remains constant until the request completes.
- Built-in request logging and many templates include `{TraceIdentifier}` or map it to logging scope properties.
- It is local to one server instance — distributed systems propagate a separate correlation or trace ID across outbound calls.
- Clients can supply their own correlation header; middleware may copy or complement it but `TraceIdentifier` always exists server-side.

---

## Q10. What is a correlation ID, and where is it typically set?

What is a correlation ID, and where is it typically set?

**Answer:** A correlation ID is an application-wide identifier carried through a logical operation — often spanning multiple services — so all related logs and traces can be filtered together. It is typically set in edge middleware from an incoming header (for example, `X-Correlation-ID`) or generated when absent, then added to logging scopes and outbound HTTP headers.

- Incoming gateway or API middleware reads or creates the ID at the start of the pipeline.
- `BeginScope` pushes the correlation ID onto all logs for the request and downstream work triggered synchronously within that scope.
- Outgoing `HttpClient` calls should forward the same header via a delegating handler or OpenTelemetry propagation.
- Correlation IDs differ from W3C `traceparent` trace IDs but serve a similar operational purpose; many systems align them in OpenTelemetry setups.

---

## Q11. How do you configure log levels per namespace in `appsettings.json`?

How do you configure log levels per namespace in `appsettings.json`?

**Answer:** Under `Logging:LogLevel`, add entries whose keys are category names (usually namespace prefixes) and whose values are minimum level names. The longest matching prefix wins for a given logger category, with `Default` as the fallback when no specific rule matches.

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
    "YourApp.Payments": "Debug"
  }
}
```

- Framework namespaces (`Microsoft.*`) are commonly elevated to Warning to reduce noise while keeping application code at Information.
- Temporary Debug on a subtree (`YourApp.Payments`) aids targeted investigation without enabling Debug globally in production.
- Changes take effect on restart unless using a dynamic logging level provider or configuration reload integrated with logging.

---

## Q12. What logging providers ship with ASP.NET Core by default?

What logging providers ship with ASP.NET Core by default?

**Answer:** The generic host registers a small set of built-in providers suitable for local development and simple deployments. Additional sinks are added via NuGet packages or `ILoggerProvider` implementations.

- **Console** — writes formatted log output to stdout/stderr (default in ASP.NET Core 8 templates for development visibility).
- **Debug** — writes to the debugger output window when running under Visual Studio or attached debuggers.
- **EventSource** — emits ETW/EventSource events for low-overhead diagnostics on Windows and tooling integration.
- **EventLog** — available on Windows for writing to the Windows Event Log when configured.
- Production APIs commonly add Application Insights, Serilog, or OpenTelemetry exporters rather than relying on console alone.

---

## Q13. What is OpenTelemetry, and how does it relate to ASP.NET Core?

What is OpenTelemetry, and how does it relate to ASP.NET Core?

**Answer:** OpenTelemetry (OTel) is a vendor-neutral standard and SDK for collecting traces, metrics, and logs from applications and exporting them to observability backends. ASP.NET Core 8 integrates OTel through `OpenTelemetry.Extensions.Hosting` and instrumentation packages that automatically record HTTP requests, outbound calls, and custom spans.

- Add with `builder.Services.AddOpenTelemetry()` and configure tracing, metrics, and logging exporters (OTLP, Azure Monitor, Prometheus).
- ASP.NET Core instrumentation creates spans for incoming requests with method, route, status code, and duration.
- OTel unifies correlation with W3C trace context (`traceparent`) propagated across services.
- It complements — and increasingly replaces — ad hoc Application Insights SDK wiring for portable observability across clouds.

---

## Q14. What is distributed tracing?

What is distributed tracing?

**Answer:** Distributed tracing records a tree of spans representing work across multiple services and machines for one logical operation, linked by shared trace and span IDs. Each service creates child spans for database calls, HTTP outbound requests, and queue processing so operators can see end-to-end latency and failure points.

- A trace ID ties together all spans from the initial API call through downstream microservices.
- Parent-child span relationships show which dependency slowed or failed the overall request.
- Propagation headers (`traceparent`, `tracestate`) carry context on HTTP and messaging calls between services.
- ASP.NET Core with OpenTelemetry or Application Insights produces distributed traces automatically for incoming and outgoing HTTP when instrumentation is enabled.

---

## Q15. What should you never log in a production application?

What should you never log in a production application?

**Answer:** Never log secrets, credentials, full payment card data, authentication tokens, passwords, or other regulated personal data at levels that reach persistent sinks. Production logs are replicated, indexed, and retained — treating them as secure storage causes compliance violations and expands breach impact when log systems are compromised.

- Avoid logging raw JWTs, API keys, connection strings, or session cookies even in Error logs during incidents.
- Minimize personally identifiable information (PII) such as full email, government IDs, or health data — prefer opaque internal IDs.
- Request/response body logging at Information or Debug often captures sensitive payloads unintentionally.
- When debugging auth issues, log outcome and correlation ID, not the secret or full credential material.

---

## Q16. What is the difference between logging and diagnostics?

What is the difference between logging and diagnostics?

**Answer:** Logging records discrete timestamped events with messages and severity intended for human investigation and alerting, while diagnostics encompasses broader telemetry — metrics, traces, health checks, counters, and performance counters — that describe system behavior quantitatively over time.

- Logs answer "what happened on this request at 14:32:05?" with narrative detail.
- Metrics answer "what is the error rate and p95 latency this hour?" as aggregated time series.
- Traces answer "which service call in the chain added 800 ms?" with span hierarchies.
- ASP.NET Core uses `ILogger` for logging and OpenTelemetry / `System.Diagnostics.Activity` for tracing; both feed observability platforms but serve different query patterns.

---

## Q17. How does Application Insights integrate with ASP.NET Core logging?

How does Application Insights integrate with ASP.NET Core logging?

**Answer:** The `Microsoft.ApplicationInsights.AspNetCore` package registers an `ILoggerProvider` that forwards log entries to Azure Application Insights alongside automatic request, dependency, and exception telemetry. Log levels and sampling settings control volume and cost while preserving Error and Critical signals.

- Add with `builder.Services.AddApplicationInsightsTelemetry()` and configure `APPLICATIONINSIGHTS_CONNECTION_STRING` or `InstrumentationKey` in the environment.
- `ILogger` messages appear as trace telemetry with severity, custom properties, and linked operation ID when a request is active.
- Exception logging with the exception overload creates exception telemetry with stack traces searchable in the portal.
- Adaptive sampling reduces ingestion for high-volume Information logs while keeping statistically representative error data.

---

## Q18. What is `LoggerMessage` source generators, and why use them?

What is `LoggerMessage` source generators, and why use them?

**Answer:** `LoggerMessage` attributes and source generators (in `Microsoft.Extensions.Logging.Abstractions`) compile high-performance logging methods that cache delegate instances and avoid boxing and string formatting when a log level is disabled. They are used in hot paths and framework code where logging overhead must stay minimal.

- Define partial methods with `[LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} created")]` on a partial static class.
- The generator emits strongly typed `LogOrderCreated(ILogger logger, int orderId)` methods with optimal `IsEnabled` checks.
- Parameter names in the template become structured properties identically to hand-written template calls.
- Use when profiling shows logging allocations matter — ordinary `ILogger` template calls are sufficient for most business code.

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

---

**Answer:**

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

// OrderService.cs — no scope, logs only order id
_logger.LogInformation("Persisting order {OrderId}", order.Id);
```

---

**Answer:**

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

---

#### Q3. (P) Production `appsettings.Production.json` sets `"Default": "Debug"` for logging while Development uses `"Information"`. What risks does this create, and what levels would you configure per namespace for a public API?

---

**Answer:**

**Answer:** Debug in production floods sinks with per-request noise, increases cost, may log sensitive payloads, and hides Critical/Error signals — production should default to Warning or Information with tighter overrides per namespace.

- **Default:** `Information` for `Microsoft.AspHost` / app namespace; `Warning` for `Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore` (or `Warning` for EF SQL unless debugging).
- **Never Debug globally in prod** — use short-lived dynamic log level (App Insights adaptive sampling, `LoggingLevelSwitch`, or Azure App Configuration) for targeted investigations.
- Risks: PII in Debug templates, I/O overhead on hot paths, log storage bill, alert fatigue masking payment failures.
- Use **category overrides**: `"YourApp.Payments": "Information"`, `"YourApp": "Warning"` for verbose subsystems only when needed.
- Pair levels with **sampling** in Application Insights so high-volume Information is retained statistically.

**Production takeaway:** Karat tests operational judgment — Debug in prod is a common post-incident finding, not a best practice.

---

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

    var result = await _auth.SignInAsync(request.Username, request.Password);
    return result.Succeeded ? Ok() : Unauthorized();
}
```

---

**Answer:**

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

---

#### Q5. (P) A team wants distributed traces from ASP.NET Core through an outbound `HttpClient` call to a downstream pricing service. Outline the OpenTelemetry setup in `Program.cs` and what must the outbound call participate in for trace continuity.

---

**Answer:**

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

---

**Answer:**

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

---

#### Q7. (M) Explain how `ILogger.BeginScope` (or `LoggerMessage` scopes) propagates correlation IDs across async calls in a request, and where you would set the initial `TraceIdentifier` or custom `CorrelationId` in the ASP.NET Core pipeline.

---

**Answer:**

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

---

#### Q8. (D) An on-call alert fires on `LogCritical` from a background worker when a queue is full, but the same team ignores `LogError` in controllers. How do you define log level policy, sampling, and alert thresholds so production signal stays actionable?



**Answer:**

**Answer:** Define a level contract: **Critical** = immediate human page (service down, data loss); **Error** = failed operation requiring ticket but not always page; **Warning** = degraded; reserve alerts for SLO-burning rates, not single Error lines in controllers.

- Document team semantics in runbooks — `LogCritical` on queue full is valid if it blocks orders; controller `LogError` on 404-like business cases should be Warning or Information.
- Alert on **rates** (N Critical per 5 min, Error spike 3× baseline) not single events — Application Insights alerts, Prometheus recording rules.
- Use **sampling** for high-volume Information; never sample Error/Critical by default.
- Separate **audit logs** (security) from **diagnostic logs** (engineering) with different retention and alert routes.
- Review weekly: top Error messages — fix root cause or downgrade intentional paths to Warning.

**Production takeaway:** Karat tests whether you treat logging as an operational API — levels drive paging, not personal preference.

---

---
