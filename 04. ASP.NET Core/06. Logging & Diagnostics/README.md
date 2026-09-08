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

**Concepts**
- Generic logging abstraction with type parameter setting the log category
- `Microsoft.Extensions.Logging` pipeline as the underlying system
- Per-namespace level control via category names
- Singleton factory registration — no manual registration needed per `T`

**Answer**

`ILogger<T>` is a generic logging abstraction registered in DI where the type parameter `T` sets the log category, typically the consuming class name. It wraps the underlying `Microsoft.Extensions.Logging` pipeline and is the standard way application and framework code emit structured log entries. Category names appear in filters (`Logging:LogLevel:YourApp.Services.OrderService`) for per-namespace level control, and methods include `LogInformation`, `LogWarning`, `LogError`, and overloads that accept exceptions and structured parameters. `ILogger<T>` is registered as a singleton factory so each `T` gets a category-specific logger without manual registration, and injecting `ILogger<MyService>` over non-generic `ILogger` keeps categories precise in production log queries.

---

## Q2. How is logging configured in ASP.NET Core?

**Concepts**
- `Logging` configuration section controlling minimum levels and category overrides
- `builder.Logging.AddConsole()` and other provider registrations
- `ClearProviders()` for full control in tests or custom Serilog bootstrap
- Level filters applying before messages reach providers

**Answer**

Logging is configured through the `Logging` section of configuration (commonly `appsettings.json`) and optional code-based provider registration in `Program.cs`. The host adds default providers and reads minimum levels and category overrides from configuration at startup. Set default and per-namespace levels with `"Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } }`. Add providers explicitly with `builder.Logging.AddConsole()`, `AddDebug()`, or third-party sinks (Serilog, Application Insights). `ClearProviders()` removes defaults when you want full control, which is typical in tests or custom Serilog bootstrap. Log level filters apply before messages reach providers, reducing overhead for noisy framework categories.

---

## Q3. What are the standard log levels in .NET logging?

**Concepts**
- Six ordered levels: Trace, Debug, Information, Warning, Error, Critical
- `None` to disable logging for a category
- Minimum level filter controlling which calls are emitted

**Answer**

.NET defines six log levels ordered by severity — Trace, Debug, Information, Warning, Error, and Critical — plus None to disable logging, and a log call is emitted only when its level is greater than or equal to the effective minimum level for that logger category. Trace is detailed diagnostic flow, rarely enabled in production. Debug is developer-oriented diagnostic information useful during local troubleshooting. Information records general application flow events such as request handled or order created. Warning covers unexpected but recoverable situations such as retries or deprecated API use. Error records failures in the current operation that were handled or returned to the caller. Critical covers unrecoverable application or system failures requiring immediate attention.

---

## Q4. What is structured logging?

**Concepts**
- Named template parameters captured as typed properties, not concatenated strings
- Log sinks indexing on `OrderId`, `UserId`, `ElapsedMs` without regex
- Structured exception details when the exception object is passed
- Aggregation and alerting enabled by consistent property names

**Answer**

Structured logging records events as typed name-value properties attached to a message template rather than as a single formatted string, so log sinks (Seq, Application Insights, Elasticsearch) can index and query on `OrderId`, `UserId`, or `ElapsedMs` without fragile regular expressions. Use templates such as `_logger.LogInformation("Order {OrderId} shipped in {ElapsedMs} ms", orderId, elapsed)` — properties are stored as fields in the logging backend, enabling filters like `OrderId == 'abc'` across all log levels. Structured data supports aggregation, alerting, and correlation across services in centralized logging systems. Exception objects passed to logging overloads capture type, message, and stack trace as structured exception details when the provider supports it.

---

## Q5. Why should you use message templates instead of string interpolation in log calls?

**Concepts**
- Deferred formatting — templates skip string building when level is disabled
- Template parameter names preserved as structured properties
- `$"..."` interpolation allocating on every call regardless of level
- `LoggerMessage` source generators building on the same template model

**Answer**

Message templates defer formatting to the logging provider and preserve parameter names as structured properties, while string interpolation (`$"Order {id}"`) always allocates the final string even when the log level is disabled. `_logger.LogDebug("Processing {OrderId}", orderId)` skips string building entirely when Debug is disabled for that category, which matters on hot paths at the default Information level. `$"Processing {orderId}"` evaluates immediately, wasting CPU and allocations. Templates also keep property names explicit for queries — `OrderId` is searchable in Application Insights or Seq — instead of parsing a concatenated sentence. Some providers use template hashing for high-performance logging, and `LoggerMessage` source generators build on this model to eliminate boxing and allocation entirely.

---

## Q6. What is the difference between `_logger.LogError(ex.Message)` and `_logger.LogError(ex, "...")`?

**Concepts**
- String overload vs exception overload — stack trace loss
- Application Insights and Seq requiring the exception object for structured details
- `LogError(ex, template, params)` storing exception type, stack, and custom properties together

**Answer**

Passing only `ex.Message` logs a plain string with no exception object attached, so sinks lose stack trace, inner exceptions, and structured exception metadata. The overload `_logger.LogError(exception, messageTemplate, ...)` records the full exception for diagnostics and still supports structured message properties. `LogError(ex.Message)` uses a string overload — Application Insights and Seq show text without exception details, which means support teams searching by exception type or stack frame cannot triage effectively when only `.Message` was logged. Always pass the exception instance as the first argument to error/critical logging overloads when an exception exists.

---

## Q7. What is the difference between `throw;` and `throw ex;` in a catch block?

**Concepts**
- `throw;` preserving the original stack trace
- `throw ex;` resetting the stack trace to the catch block line
- Root-cause obscuring in Application Insights and Seq with `throw ex`
- `InnerException` preservation when wrapping in a new exception type

**Answer**

`throw;` rethrows the caught exception while preserving the original stack trace, so logs and debuggers point to the true failure site. `throw ex;` throws the same exception object but resets the stack trace to the current catch line, hiding where the error originally occurred, which makes production incidents look like the bug is in middleware or a generic handler instead of the root cause. Filtering and alerting on stack traces become misleading when `throw ex` is used in shared exception handling code. Use `throw;` when logging and rethrowing, or when a catch block cannot handle the error. If you need to add context, wrap with a new exception and preserve the original: `throw new OrderProcessingException("...", ex)`.

---

## Q8. What is `ILogger.BeginScope`, and what is it used for?

**Concepts**
- Scope attaching contextual properties to all logs within a `using` block
- Nesting scopes — inner scopes add without removing outer properties
- Middleware establishing correlation scope at request start
- `AsyncLocal` propagation across `await` boundaries

**Answer**

`BeginScope` adds contextual properties to all log entries written within a `using` block on that logger, until the scope is disposed. It is used to attach correlation IDs, tenant IDs, or operation names so related log lines share queryable fields across layers. Use it as `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id })) { ... }`. ASP.NET Core request logging scopes often include `RequestId`, `TraceIdentifier`, or path information automatically in some providers. Scopes nest — inner scopes add properties without removing outer scope values unless keys collide — and middleware that establishes a correlation scope at request start ties controller, service, and repository logs to one customer action. Scopes propagate across `await` in the same request because the default logger scope provider uses `AsyncLocal`.

---

## Q9. How does ASP.NET Core assign a `TraceIdentifier` to each request?

**Concepts**
- `HttpContext.TraceIdentifier` generated by the host per request
- Constant for the request lifetime, available before middleware runs
- Local to one server instance — not propagated across services
- Complements, does not replace, distributed correlation IDs

**Answer**

When a request is received, the host generates a unique string and stores it on `HttpContext.TraceIdentifier` for the lifetime of that request. Loggers, diagnostics, and developer exception pages include this value so operators can correlate all activity for a single HTTP transaction. The identifier is available before middleware runs and remains constant until the request completes; built-in request logging and many templates include `{TraceIdentifier}` or map it to logging scope properties. It is local to one server instance — distributed systems propagate a separate correlation or trace ID across outbound calls. Clients can supply their own correlation header, and middleware may copy or complement it, but `TraceIdentifier` always exists server-side as a fallback.

---

## Q10. What is a correlation ID, and where is it typically set?

**Concepts**
- Correlation ID spanning multiple services for a single logical operation
- Edge middleware reading or generating the ID from `X-Correlation-ID`
- `BeginScope` pushing the ID onto all logs for the request
- Forwarding on outbound `HttpClient` calls via a delegating handler

**Answer**

A correlation ID is an application-wide identifier carried through a logical operation — often spanning multiple services — so all related logs and traces can be filtered together. It is typically set in edge middleware from an incoming header such as `X-Correlation-ID` or generated when absent, then added to logging scopes and outbound HTTP headers. Incoming gateway or API middleware reads or creates the ID at the start of the pipeline, `BeginScope` pushes it onto all logs for the request and downstream work triggered synchronously within that scope, and outgoing `HttpClient` calls should forward the same header via a delegating handler or OpenTelemetry propagation. Correlation IDs differ from W3C `traceparent` trace IDs but serve a similar operational purpose; many systems align them in OpenTelemetry setups.

---

## Q11. How do you configure log levels per namespace in `appsettings.json`?

**Concepts**
- Longest-matching-prefix wins for category-level resolution
- `Default` as the fallback when no specific rule matches
- Elevated framework namespace levels reducing noise
- Temporary Debug on a subtree for targeted investigation

**Answer**

Under `Logging:LogLevel`, add entries whose keys are category name prefixes and whose values are minimum level names. The longest matching prefix wins for a given logger category, with `Default` as the fallback when no specific rule matches.

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

Framework namespaces (`Microsoft.*`) are commonly elevated to Warning to reduce noise while keeping application code at Information. Temporary Debug on a subtree (`YourApp.Payments`) aids targeted investigation without enabling Debug globally in production. Changes take effect on restart unless using a dynamic logging level provider or configuration reload integrated with logging.

---

## Q12. What logging providers ship with ASP.NET Core by default?

**Concepts**
- Console provider for stdout/stderr in development
- Debug provider for Visual Studio output window
- EventSource for ETW/EventSource low-overhead diagnostics
- EventLog available on Windows
- Production APIs adding Application Insights, Serilog, or OTel exporters

**Answer**

The generic host registers a small set of built-in providers suitable for local development and simple deployments. Console writes formatted log output to stdout/stderr, which is the default in ASP.NET Core 8 templates for development visibility. Debug writes to the debugger output window when running under Visual Studio or attached debuggers. EventSource emits ETW/EventSource events for low-overhead diagnostics on Windows and tooling integration. EventLog is available on Windows for writing to the Windows Event Log when configured. Production APIs commonly add Application Insights, Serilog, or OpenTelemetry exporters rather than relying on console alone, and `ClearProviders()` removes all defaults when a custom setup is preferred.

---

## Q13. What is OpenTelemetry, and how does it relate to ASP.NET Core?

**Concepts**
- Vendor-neutral standard for traces, metrics, and logs
- `AddOpenTelemetry()` with ASP.NET Core and `HttpClient` instrumentation
- W3C `traceparent` propagation across services
- Complement or replacement for ad hoc Application Insights SDK wiring

**Answer**

OpenTelemetry (OTel) is a vendor-neutral standard and SDK for collecting traces, metrics, and logs from applications and exporting them to observability backends. ASP.NET Core 8 integrates OTel through `OpenTelemetry.Extensions.Hosting` and instrumentation packages that automatically record HTTP requests, outbound calls, and custom spans. Add with `builder.Services.AddOpenTelemetry()` and configure tracing, metrics, and logging exporters (OTLP, Azure Monitor, Prometheus). ASP.NET Core instrumentation creates spans for incoming requests with method, route, status code, and duration, and OTel unifies correlation with W3C trace context (`traceparent`) propagated across services. It complements — and increasingly replaces — ad hoc Application Insights SDK wiring for portable observability across clouds.

---

## Q14. What is distributed tracing?

**Concepts**
- Tree of spans representing work across multiple services for one logical operation
- Trace ID linking all spans for a single request chain
- Parent-child span relationships showing dependency latency
- W3C `traceparent` and `tracestate` propagation headers

**Answer**

Distributed tracing records a tree of spans representing work across multiple services and machines for one logical operation, linked by shared trace and span IDs. Each service creates child spans for database calls, HTTP outbound requests, and queue processing so operators can see end-to-end latency and failure points. A trace ID ties together all spans from the initial API call through downstream microservices, and parent-child span relationships show which dependency slowed or failed the overall request. Propagation headers (`traceparent`, `tracestate`) carry context on HTTP and messaging calls between services. ASP.NET Core with OpenTelemetry or Application Insights produces distributed traces automatically for incoming and outgoing HTTP when instrumentation is enabled.

---

## Q15. What should you never log in a production application?

**Concepts**
- Secrets, credentials, tokens, and passwords never in log entries
- PII minimization — opaque internal IDs rather than full personal data
- Request/response body logging capturing sensitive payloads unintentionally
- Compliance violations from credentials in log aggregation stores

**Answer**

Never log secrets, credentials, full payment card data, authentication tokens, passwords, or other regulated personal data at levels that reach persistent sinks. Production logs are replicated, indexed, and retained — treating them as secure storage causes compliance violations and expands breach impact when log systems are compromised. Avoid logging raw JWTs, API keys, connection strings, or session cookies even in Error logs during incidents. Minimize personally identifiable information (PII) such as full email, government IDs, or health data — prefer opaque internal IDs. Request/response body logging at Information or Debug often captures sensitive payloads unintentionally. When debugging auth issues, log outcome and correlation ID, not the secret or full credential material.

---

## Q16. What is the difference between logging and diagnostics?

**Concepts**
- Logging — discrete timestamped events for human investigation
- Metrics — aggregated time series for quantitative system behavior
- Traces — span hierarchies showing cross-service latency
- `ILogger` for logging vs OpenTelemetry / `System.Diagnostics.Activity` for tracing

**Answer**

Logging records discrete timestamped events with messages and severity intended for human investigation and alerting, while diagnostics encompasses broader telemetry — metrics, traces, health checks, counters, and performance counters — that describe system behavior quantitatively over time. Logs answer "what happened on this request at 14:32:05?" with narrative detail. Metrics answer "what is the error rate and p95 latency this hour?" as aggregated time series. Traces answer "which service call in the chain added 800 ms?" with span hierarchies. ASP.NET Core uses `ILogger` for logging and OpenTelemetry / `System.Diagnostics.Activity` for tracing; both feed observability platforms but serve different query patterns and alert strategies.

---

## Q17. How does Application Insights integrate with ASP.NET Core logging?

**Concepts**
- `Microsoft.ApplicationInsights.AspNetCore` registering an `ILoggerProvider`
- Log levels and sampling controlling volume and cost
- `ILogger` messages appearing as trace telemetry with linked operation ID
- Adaptive sampling reducing ingestion for high-volume Information logs

**Answer**

The `Microsoft.ApplicationInsights.AspNetCore` package registers an `ILoggerProvider` that forwards log entries to Azure Application Insights alongside automatic request, dependency, and exception telemetry. Add with `builder.Services.AddApplicationInsightsTelemetry()` and configure `APPLICATIONINSIGHTS_CONNECTION_STRING` in the environment. `ILogger` messages appear as trace telemetry with severity, custom properties, and linked operation ID when a request is active, and exception logging with the exception overload creates exception telemetry with stack traces searchable in the portal. Adaptive sampling reduces ingestion for high-volume Information logs while keeping statistically representative error data, so configuring appropriate minimum levels per namespace (`Microsoft.*` at Warning) is important to avoid high ingestion costs.

---

## Q18. What is `LoggerMessage` source generators, and why use them?

**Concepts**
- Source-generated methods avoiding boxing and string formatting at disabled levels
- `[LoggerMessage]` attribute on partial static class methods
- Strongly typed parameters becoming structured properties
- Use on hot paths where logging allocations are profiler-visible

**Answer**

`LoggerMessage` attributes and source generators (in `Microsoft.Extensions.Logging.Abstractions`) compile high-performance logging methods that cache delegate instances and avoid boxing and string formatting when a log level is disabled. They are used in hot paths and framework code where logging overhead must stay minimal. Define partial methods with `[LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} created")]` on a partial static class — the generator emits strongly typed `LogOrderCreated(ILogger logger, int orderId)` methods with optimal `IsEnabled` checks so no allocation occurs when the level is filtered. Parameter names in the template become structured properties identically to hand-written template calls. Use when profiling shows logging allocations matter — ordinary `ILogger` template calls are sufficient for most business code.

---

## Gotchas — Logging & Diagnostics (Interview Traps)

---

#### Gotcha 1. Log level filtering is per-provider and per-category — minimum level alone does not control output

**Concepts**
- `Logging:LogLevel:Default` as the global minimum level
- Per-provider filter configuration overriding the global minimum
- Per-category rules applied before per-provider rules
- Most specific rule wins — category path matching is prefix-based

**Answer**

Setting `Logging:LogLevel:Default` to `Information` globally does not mean every provider emits all Information logs. Each provider (Console, Application Insights, Serilog) applies its own filter on top of the minimum level, and categories can be filtered more finely than the global level. The most specific matching rule wins: `Logging:LogLevel:Microsoft.EntityFrameworkCore` overrides `Logging:LogLevel:Microsoft`, which overrides `Default`. A common mistake is setting the global minimum to `Debug` expecting to see all debug output, then being surprised that the Console provider filters it to `Warning` via its own section. Always check provider-specific and category-specific rules in configuration when log output doesn't match expectations.

---

#### Gotcha 2. Structured log template parameter order must match argument order

**Concepts**
- Message template positional parameters — `{0}` vs named `{OrderId}`
- `ILogger.LogInformation("{UserId} placed order {OrderId}", userId, orderId)` — order matters
- Argument names in templates for structured log property names
- Mismatched parameter count causing `FormatException` or wrong property names

**Answer**

`ILogger.LogInformation("User {UserId} placed order {OrderId}", userId, orderId)` maps arguments by position to template placeholders. Swapping arguments — `LogInformation("User {UserId} placed order {OrderId}", orderId, userId)` — silently assigns `orderId` to the `UserId` property and `userId` to `OrderId` in structured log sinks, producing misleading queries. Unlike `string.Format`, no `FormatException` is thrown for mismatched types — Serilog and other structured logging providers serialize whatever argument is supplied. Use named placeholders that match local variable names for readability, and always verify the argument order matches the template when adding or refactoring log statements.

---

#### Gotcha 3. `ILogger<T>` vs `ILoggerFactory` — `T` sets the category name, not the log provider

**Concepts**
- `ILogger<T>` category derived from `typeof(T).FullName`
- `ILoggerFactory.CreateLogger("CategoryName")` for custom category strings
- Category name used for log filtering — must match configuration keys
- Injecting `ILogger` without generic type loses category context

**Answer**

`ILogger<OrderService>` creates a logger with category `Namespace.OrderService`, which is used for per-category filtering in `appsettings.json`. If you inject `ILogger` (non-generic) into a class, the category defaults to an empty string or the framework's generic type, losing the per-class filtering granularity. Custom category names via `ILoggerFactory.CreateLogger("Payment.Processor")` are valid for shared utilities that aren't tied to a single class. The category name must match the prefix used in `Logging:LogLevel` configuration — a mismatch means category-specific log level rules never apply. Always prefer `ILogger<T>` for class-level loggers.

---

#### Gotcha 4. Logging sensitive data — connection strings and secrets captured in log messages

**Concepts**
- Structured logging capturing all argument values verbatim
- Database exception messages containing connection string fragments
- `DbContext.EnableSensitiveDataLogging()` exposing parameter values
- Log scrubbing and redaction at the provider level

**Answer**

Structured logging captures all argument values verbatim — passing a connection string, user email, API key, or credit card number as a log argument writes it to every configured log sink. EF Core's `EnableSensitiveDataLogging()` is useful during development but must never be enabled in production, as it logs SQL parameter values including passwords and PII. When logging exceptions from database operations, the exception message sometimes includes connection string fragments. Use `[LogSensitiveDataRedacted]` attributes, a custom destructuring policy in Serilog, or dedicated log scrubber middleware to prevent sensitive values from reaching log aggregators or sinks that store logs persistently.

---

#### Gotcha 5. `LogError` records the error — it does not throw, and execution continues

**Concepts**
- `_logger.LogError(ex, "message")` writing to log sinks, not throwing
- Execution continuing after `LogError` unless explicitly thrown or returned
- `LogCritical` vs `LogError` — same execution behavior, different severity level
- Combining logging with re-throwing to both record and propagate

**Answer**

`_logger.LogError(exception, "Order processing failed")` writes the error to all configured log sinks and then returns — execution continues on the next line. A common mistake is writing `_logger.LogError(ex, "Unexpected error")` and returning a success response, effectively swallowing the exception after recording it. If the caller or global error handler should also see the exception, log it and then re-throw with bare `throw`. Similarly, `_logger.LogCritical` has higher severity semantics but identical execution behavior — it does not halt the application. Explicitly check whether logging is intended to supplement exception propagation or replace it.

---

#### Gotcha 6. Log scopes do not flow across `async void` or fire-and-forget tasks

**Concepts**
- `ILogger.BeginScope()` using `AsyncLocal<T>` for scope propagation
- `AsyncLocal` flowing into `await` continuations but not across `Task.Run` thread boundaries correctly
- Scope correlation IDs missing in background task logs
- Capturing scope data explicitly before starting background work

**Answer**

`ILogger.BeginScope()` stores scope properties in an `AsyncLocal<T>` which flows through awaited continuations in the same logical async context. However, work started with `Task.Run()` from within a scope may or may not carry that scope depending on the synchronization context and how the work is detached. Fire-and-forget tasks using `_ = SomeAsync()` definitely lose scope context because the async logical call chain is detached. When background work must carry correlation context — such as a request ID or user ID — capture the values explicitly from the current scope before starting the background task and create a new scope with those values inside the background work's own logging context.

---

#### Gotcha 7. Default log level in production is `Warning` — `Information` logs are not emitted unless configured

**Concepts**
- `appsettings.Production.json` default `LogLevel:Default` of `Warning`
- `appsettings.Development.json` default of `Information`
- Application logs emitted at `Information` silently suppressed in production
- Intentional production default to reduce log volume and cost

**Answer**

`WebApplication.CreateBuilder` sets the default log level to `Information` in development and `Warning` in production via the environment-specific `appsettings` files. Application code that logs business events at `Information` will appear in development but be silently suppressed in production unless the configuration is overridden. This is the intended behavior — production logging at `Information` for all categories generates significant volume — but teams that expect production-level tracing of business events must configure the specific categories they need at `Information` in `appsettings.Production.json`. Discovering that no application logs appear in production after deployment is a common first-week surprise.

---

#### Gotcha 8. Console provider outputs plain text by default — structured logging requires a separate sink

**Concepts**
- Console log provider formatting as human-readable text, not JSON
- Serilog, NLog, or Application Insights for structured JSON output
- `AddJsonConsole()` for structured JSON output to console
- Log aggregators requiring structured output for queryable properties

**Answer**

The built-in `Console` log provider writes human-readable text suitable for local development but is not structured JSON. Log aggregators such as Datadog, Elasticsearch, and Azure Monitor are most effective when ingesting structured JSON where fields like `orderId`, `userId`, and `requestId` are queryable properties rather than substrings of a text line. Use `AddJsonConsole()` for structured console output in containers, or replace the console provider with Serilog or NLog configured with a structured sink. Serilog's `WriteTo.Console(new JsonFormatter())` or Application Insights `AddApplicationInsightsTelemetry()` produce the structured output that log aggregators expect.

---

#### Gotcha 9. `appsettings.json` log level changes require restart unless `reloadOnChange` and a live config monitor are in use

**Concepts**
- `AddJsonFile("appsettings.json", reloadOnChange: true)` enabling live reload
- `IOptionsMonitor<LoggingOptions>` not used by the logging framework directly
- Log provider filters re-evaluated on `IConfiguration` reload
- Production log level changes without restart using environment variables

**Answer**

By default, `AddJsonFile` loads `appsettings.json` once at startup. Setting `reloadOnChange: true` enables file-watcher-based live reload of the configuration, but whether the logging infrastructure actually picks up changed log levels depends on the provider. The built-in logging framework does re-apply filters when `IConfiguration` is reloaded because it reads from the live `IConfiguration` instance rather than a snapshot. However, third-party providers like Serilog require their own reload wiring via `ReadFrom.Configuration(configuration, reloadOnChanges: true)`. Changing log levels via environment variables is the most reliable approach for production because environment variable changes require a process restart, making the change deliberate and auditable.

---

#### Gotcha 10. `ActivitySource` and OpenTelemetry tracing require explicit listener registration — no output without a listener

**Concepts**
- `ActivitySource.StartActivity()` returning `null` when no listener is subscribed
- OpenTelemetry tracer provider requiring `AddSource("SourceName")` registration
- `Activity.Current` null without an active span
- `AddOpenTelemetry().WithTracing()` as the listener registration point

**Answer**

`ActivitySource.StartActivity("operation")` returns `null` when no `ActivityListener` is subscribed to the source — the activity is simply not created. Code that assumes `Activity.Current` is non-null after `StartActivity()` throws `NullReferenceException` when no tracer is registered. In OpenTelemetry, the tracer provider must register the source by name: `tracerProviderBuilder.AddSource("MyApp.Orders")`. Without this registration, the `ActivitySource` creates no spans and emits nothing to the exporter. This is a common issue in development environments where OpenTelemetry is configured for production but not locally — all tracing calls silently no-op, which is correct behavior but confuses developers who expect spans to appear.

---

## Scenario-Based Questions (Karat Format)

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

**Concepts**
- `LogError(ex.Message)` string overload losing exception object and stack trace
- `throw ex` resetting stack trace to catch block
- `_logger.LogError(ex, template, params)` as the correct pattern
- Correlation scope with `orderId` for request stitching

**Answer**

Logging only `ex.Message` drops the exception object from structured logs, so Application Insights and Seq receive a plain string with no exception details, stack trace, or inner exception. `throw ex` then resets the stack trace to this catch site, which means even if the exception reached a global handler, the trace would point to the catch block rather than the `_gateway.ChargeAsync` call that actually failed. The fix is `_logger.LogError(ex, "Charge failed for order {OrderId}", orderId)` to store the full exception with structured `OrderId` context, and `throw;` to rethrow without resetting the stack. If the intent is only to log and rethrow, the try/catch can be omitted entirely and the exception handled once by global middleware.

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

**Concepts**
- Missing shared logging scope preventing cross-layer log correlation
- `BeginScope` with `CorrelationId` tying controller, service, and DB logs
- PII risk from `{Email}` in Information logs
- OpenTelemetry or `traceparent` header propagation for distributed tracing

**Answer**

Without a shared logging scope (correlation ID / trace ID), each layer logs isolated events so you cannot filter one checkout flow across services in Seq or Application Insights. The fix is middleware that opens a scope for every request:

```csharp
using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier }))
{
    await next();
}
```

This ties all controller, service, and repository logs to one request without passing the correlation ID explicitly through every method. Log `OrderId` or a hashed user identifier rather than raw email at Information level to reduce PII exposure, and propagate `traceparent` or a custom header on outbound `HttpClient` calls via `IHttpClientFactory` so distributed traces connect across service boundaries.

---

#### Q3. (P) Production `appsettings.Production.json` sets `"Default": "Debug"` for logging while Development uses `"Information"`. What risks does this create, and what levels would you configure per namespace for a public API?

**Concepts**
- Debug globally in production flooding sinks and hiding Critical/Error signals
- Namespace-level overrides keeping app code at Information, framework at Warning
- Adaptive sampling in Application Insights for high-volume Information logs
- Short-lived dynamic log level for targeted investigations

**Answer**

Debug in production floods sinks with per-request noise, increases ingestion cost, may log sensitive payloads captured by Debug-level templates, and drowns Critical/Error signals in volume, making alerting unreliable. The correct production defaults are `Default: Information` for application namespaces, `Warning` for `Microsoft.AspNetCore`, `Microsoft.EntityFrameworkCore`, and other framework namespaces. Never enable Debug globally — use short-lived dynamic log level switches (App Insights adaptive sampling, `LoggingLevelSwitch`, or Azure App Configuration) for targeted investigations in production. Pair levels with sampling in Application Insights so high-volume Information is retained statistically without full ingestion cost. Temporary `"YourApp.Payments": "Debug"` for a specific subsystem is acceptable during an incident window, not as a permanent setting.

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

**Concepts**
- Password in log template written to persistent log stores (PCI/SOC2 violation)
- Structured logging making credential exfiltration easy via log aggregation
- Audit logs recording outcome and non-sensitive identifiers only
- Global redaction middleware or destructuring policy for sensitive properties

**Answer**

Logging `password={Password}` writes credentials — even failed-attempt passwords — to Seq, Splunk, Application Insights, or whatever persistent sink is configured, where retention, access control, and compliance requirements (PCI DSS, SOC 2) are violated. Log stores often have broader read access than the application database, meaning support staff or attackers with log access could extract credentials. The fix is to remove `{Password}` from all log statements — never log secrets, tokens, or full credentials — and log only `Username` (or a hashed subject ID), the result (`Succeeded`/`Failed`), and `Ip` at Information. Use Warning for lockout patterns. Add a Serilog `DestructuringPolicy` or a custom log scope enricher to strip known sensitive property names globally so future templates cannot accidentally include them.

---

#### Q5. (P) A team wants distributed traces from ASP.NET Core through an outbound `HttpClient` call to a downstream pricing service. Outline the OpenTelemetry setup in `Program.cs` and what must the outbound call participate in for trace continuity.

**Concepts**
- `AddAspNetCoreInstrumentation()` creating server spans per request
- `AddHttpClientInstrumentation()` injecting W3C trace context on outbound calls
- `IHttpClientFactory` required for handler-based propagation
- OTLP or Azure Monitor exporter for trace storage

**Answer**

Add OpenTelemetry tracing with ASP.NET Core and `HttpClient` instrumentation so `Activity` context flows from the inbound request to an outbound `traceparent` header automatically when using `IHttpClientFactory`.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter()); // or Azure Monitor exporter

builder.Services.AddHttpClient<IPricingClient, PricingClient>();
```

`AddAspNetCoreInstrumentation()` creates a server span per request and `AddHttpClientInstrumentation()` injects W3C trace context on outbound calls — but the outbound call must use `IHttpClientFactory` since manually created `new HttpClient()` bypasses the handler chain and breaks propagation. Export to OTLP collector, Jaeger, or Application Insights with aligned `service.name` resource attributes for the service map. Logs link to traces via `Activity.TraceId` in scopes when using `OpenTelemetryLoggerProvider` or Application Insights integration.

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

**Concepts**
- String interpolation (`$"..."` and `+`) defeating structured logging and forcing immediate allocation
- Information on every catalog read generating unnecessary high-volume noise
- Debug per-product in a loop multiplying lines by catalog size
- `LoggerMessage` source generators for genuinely hot paths

**Answer**

String interpolation (`$"..."` and `+`) defeats structured logging — parameters are not captured as queryable fields, and the string is always built regardless of whether the level is enabled, so every catalog page view allocates strings even when Information logs are later filtered. The per-product Debug loop in a foreach multiplies to thousands of lines per request if Debug is ever enabled for the category. The routine success Information on every GET adds noise without signal, since a catalog endpoint returning 200 is expected behavior and should not be logged at Information. The fix is to remove the two Information calls (or gate them at Debug with message templates) and never use string interpolation in log calls:

```csharp
_logger.LogDebug("GetProducts returning {Count} products", products.Count);
```

For genuinely hot paths where even template-based logging shows up in profiler allocations, use `LoggerMessage` source generators.

---

#### Q7. (M) Explain how `ILogger.BeginScope` (or `LoggerMessage` scopes) propagates correlation IDs across async calls in a request, and where you would set the initial `TraceIdentifier` or custom `CorrelationId` in the ASP.NET Core pipeline.

**Concepts**
- `BeginScope` using `AsyncLocal` to propagate scope across `await` boundaries
- Middleware placed after routing but before controllers and exception handlers
- `Activity.TraceId` alignment for log-trace linking
- Background work requiring explicit correlation ID passing, not implicit scope

**Answer**

`BeginScope` attaches key-value pairs to the logical logging context for the current async flow using `AsyncLocal` under the hood, so all `ILogger` calls downstream of the scope — including across `await` — inherit the same correlation ID without passing it manually through method signatures. Set the scope in middleware early in the pipeline, after routing (so endpoint metadata is available) but before controllers and exception handlers:

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

Align `CorrelationId` with `Activity.Current.TraceId` when OpenTelemetry is enabled so logs and traces link in Application Insights or Grafana. Background work started with `Task.Run` without `IHttpContextAccessor` does not inherit the HTTP scope — pass correlation ID explicitly through message headers or job context.

---

#### Q8. (D) An on-call alert fires on `LogCritical` from a background worker when a queue is full, but the same team ignores `LogError` in controllers. How do you define log level policy, sampling, and alert thresholds so production signal stays actionable?

**Concepts**
- Level semantics contract — Critical vs Error vs Warning
- Alerting on rates rather than single events
- Sampling high-volume Information without sampling Error/Critical
- Separate audit log stream with different retention and routing

**Answer**

The key is a documented level contract that the team agrees to and enforces in code review. Critical means immediate human page — service down, data loss, SLO breach — so a queue-full Critical is valid if it blocks order processing. Controller `LogError` on expected business cases (duplicate order, validation bypass) should be Warning or Information, since alerting on every 400 generates noise that trains the team to ignore the alert channel. Alert on rates rather than individual events: N Criticals per five minutes or Error spike 3x baseline, not single lines. Never sample Error or Critical by default; use Application Insights adaptive sampling or Prometheus recording rules only for high-volume Information. Maintain a separate audit log stream (security/compliance) with its own retention, alerting, and access control distinct from diagnostic engineering logs. Review the top Error messages weekly — fix root causes or downgrade intentional paths to Warning to keep the error channel meaningful.
