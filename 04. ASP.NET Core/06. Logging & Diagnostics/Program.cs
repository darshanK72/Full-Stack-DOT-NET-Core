/*
 * TOPIC: Logging & Diagnostics in ASP.NET Core
 *
 * WHY IT MATTERS:
 *   Production bugs are diagnosed via logs. Without structured, leveled logging,
 *   debugging a live system means scanning walls of text. ASP.NET Core's logging
 *   abstraction decouples your code from any specific sink — switch from Console
 *   to Seq to Application Insights by changing config, not code. Health checks
 *   integrate with Kubernetes, load balancers, and dashboards to ensure traffic
 *   only reaches healthy instances. Distributed tracing (Activity/ActivitySource)
 *   connects log lines across microservices into a single end-to-end request view.
 *
 * WHAT YOU WILL LEARN:
 *   1. ILogger<T> and ILoggerFactory — DI, categories, and log levels
 *   2. Structured logging — message templates, named placeholders, event IDs
 *   3. Log scopes — BeginScope for ambient correlation context
 *   4. [LoggerMessage] source generator — zero-allocation compile-time–safe logging
 *   5. LoggerMessage.Define — manual delegate API (legacy / .NET 5)
 *   6. ActivitySource and Activity — distributed tracing spans (W3C Trace Context)
 *   7. DiagnosticListener — observing framework diagnostic events
 *   8. IHealthCheck — implementing a custom health check
 *   9. HealthCheckResult — Healthy, Degraded, and Unhealthy response types
 *  10. Startup wiring — built-in providers, appsettings.json config, health endpoints
 *
 * CHAPTER MAP (read files in this order):
 *   1. ILogger<T>, log levels, structured logging, BeginScope
 *                            → Services/OrderProcessingService.cs  (Sections 1–3)
 *   2. [LoggerMessage] source generator and LoggerMessage.Define
 *                            → Logging/LogMessages.cs              (Sections 4–5)
 *   3. ActivitySource, Activity, and DiagnosticListener
 *                            → Diagnostics/ActivityHelper.cs       (Sections 6–7)
 *   4. IHealthCheck and HealthCheckResult
 *                            → Services/HealthCheckService.cs      (Sections 8–9)
 *   5. appsettings.json log-level configuration
 *                            → appsettings.json                    (inline comments)
 *   6. Startup wiring — built-in providers, ActivityListener, health endpoints
 *                            → Program.cs (below)                  (Section 10)
 */

using LoggingDiagnostics.Diagnostics;
using LoggingDiagnostics.Logging;
using LoggingDiagnostics.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

/*
 * SECTION 10: STARTUP WIRING — LOGGING PROVIDERS, HEALTH CHECKS, ACTIVITY LISTENER
 *
 * BUILT-IN LOGGING PROVIDERS:
 *   Console        — writes to stdout; default formatter is "simple", "json", or "systemd".
 *                    Enable IncludeScopes to surface BeginScope context in output.
 *   Debug          — writes to the debug output window (IDE); useful during development.
 *   EventSource    — writes to ETW / EventPipe; consumed by `dotnet-trace` and PerfView.
 *   EventLog       — Windows Event Log (Windows only; add builder.Logging.AddEventLog()).
 *
 * PROVIDER CONFIGURATION IN appsettings.json (see appsettings.json):
 *   "Logging": {
 *     "LogLevel": { "Default": "Information", "MyApp": "Debug" }      ← global floor
 *     "Console": {
 *       "LogLevel": { "Default": "Information" },                      ← Console-specific floor
 *       "FormatterOptions": { "IncludeScopes": true }
 *     }
 *   }
 *   appsettings.Development.json overrides these values in Development environment.
 *
 * FILTERING BY CATEGORY (programmatic — supplements appsettings.json):
 *   builder.Logging.AddFilter("LoggingDiagnostics.Services", LogLevel.Debug);
 *   builder.Logging.AddFilter<ConsoleLoggerProvider>("Microsoft", LogLevel.Warning);
 *
 * ACTIVITY LISTENER — required for ActivitySource spans to be created:
 *   Without a listener, ActivitySource.StartActivity always returns null (zero overhead).
 *   In production, OpenTelemetry SDK attaches its own listener (via AddOpenTelemetry()).
 *   Here we wire a manual listener for demo purposes only.
 *
 * HEALTH CHECK ENDPOINTS:
 *   /health       — runs all registered checks; 200 if all Healthy or Degraded, 503 if any Unhealthy
 *   /health/ready — runs only checks tagged "critical" (Kubernetes readiness probe pattern)
 *
 * PREVIEW — THIRD-PARTY PROVIDERS (out of scope for this chapter):
 *   Serilog:  builder.Host.UseSerilog(); — rich structured sinks (file, Seq, Elasticsearch)
 *   NLog:     builder.Logging.AddNLog(); — flexible routing via NLog.config XML
 *   Both integrate via ILoggingBuilder and are drop-in replacements for the built-in providers.
 *   COVERED IN DETAIL: a dedicated "Serilog & NLog" chapter when added to this module.
 */

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Logging providers ────────────────────────────────────────────────────────

// ClearProviders removes the default Console+Debug providers added by the SDK.
// Explicit AddXxx calls give full control over which providers are active.
builder.Logging.ClearProviders();

builder.Logging.AddConsole();                // stdout — reads config from "Logging:Console" section
builder.Logging.AddDebug();                  // IDE debug output window — reads "Logging:Debug"
builder.Logging.AddEventSourceLogger();      // ETW / EventPipe — for dotnet-trace and PerfView

// Global minimum level — categories in appsettings.json can raise this floor per-category
builder.Logging.SetMinimumLevel(LogLevel.Trace);

// Programmatic category filter (supplements appsettings.json; last registration wins)
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning); // suppress ASP.NET Core noise

// ── Services ────────────────────────────────────────────────────────────────

builder.Services.AddScoped<OrderProcessingService>(); // scoped — one instance per HTTP request

// AddHealthChecks registers the IHealthCheckService and returns IHealthChecksBuilder
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(
        name: "database",
        tags: new[] { "db", "critical" })          // "critical" used for readiness-probe filtering
    .AddCheck<OrderQueueHealthCheck>(
        name: "order-queue",
        tags: new[] { "queue" })                   // "queue" — excluded from readiness-probe endpoint
    .AddCheck(                                     // inline lambda check — no separate class needed
        name: "ping",
        check: () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("pong"));

WebApplication app = builder.Build();

// ── ActivityListener (demo wiring) ──────────────────────────────────────────

// Wire a listener so ActivitySource.StartActivity returns non-null spans in this demo.
// In production replace this with: builder.Services.AddOpenTelemetry().WithTracing(...)
ActivityListener activityListener = new ActivityListener
{
    ShouldListenTo = source =>
        source.Name.StartsWith("LoggingDiagnostics", StringComparison.Ordinal), // listen to our own sources only
    Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
        ActivitySamplingResult.AllData,            // sample every activity (demo only — use probabilistic in prod)
    ActivityStarted  = a => Console.WriteLine($"[Trace] Start  {a.DisplayName} — TraceId={a.TraceId}"),
    ActivityStopped  = a => Console.WriteLine($"[Trace] Finish {a.DisplayName} — {a.Duration.TotalMilliseconds:F1}ms"),
};
ActivitySource.AddActivityListener(activityListener); // register the listener with the global source registry

// ── DiagnosticListener observer (demo wiring) ───────────────────────────────

// Subscribe to ASP.NET Core's DiagnosticListener for Section 7 demo.
// Dispose on shutdown to release the subscription.
DiagnosticObserver diagnosticObserver =
    DiagnosticObserver.Subscribe("Microsoft.AspNetCore");

// ── Health check endpoints ───────────────────────────────────────────────────

app.MapHealthChecks("/health");                // all checks; 200 or 503

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration =>
        registration.Tags.Contains("critical") // readiness: only "critical"-tagged checks
});

// ── Demo endpoint ────────────────────────────────────────────────────────────

app.MapGet("/orders/{id:int}", async (
    int id,
    OrderProcessingService svc,
    ILogger<Program> logger,
    CancellationToken ct) =>
{
    // Demonstrate structured logging from the source generator (Section 4)
    LogMessages.OrderReceived(logger, id, "CUST-001");

    // Demonstrate ActivitySource span (Section 6)
    using Activity? span = ActivityHelper.StartOrderActivity("order.http.process", id, "CUST-001");

    await svc.ProcessOrderAsync(id, "CUST-001", ct);    // Section 2 — structured logging + levels
    await svc.FulfillOrderAsync(id, ct);                // Section 3 — BeginScope demo

    return Results.Ok(new { OrderId = id, Status = "Processed" }); // JSON 200 response
});

// Cleanup on shutdown
IHostApplicationLifetime lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    diagnosticObserver.Dispose();  // unsubscribe from DiagnosticListener
    activityListener.Dispose();    // detach from ActivitySource registry
});

app.Run();

/*
 * QUICK REFERENCE — Logging & Diagnostics
 *
 * ILogger<T>
 *   Inject via DI. T sets the log category (= full class name).
 *   _logger.LogInformation("Order {Id}", id) — structured; Id becomes a queryable property.
 *   _logger.LogError(ex, "Failed {Op}", op) — exception is always the FIRST argument.
 *   _logger.BeginScope("Order {Id}", id)     — returns IDisposable?; wraps messages in scope.
 *
 * Log levels (lowest → highest):
 *   Trace(0) Debug(1) Information(2) Warning(3) Error(4) Critical(5) None(6)
 *
 * appsettings.json filter (longest-prefix wins):
 *   "LoggingDiagnostics.Services.Foo": "Trace" beats "LoggingDiagnostics": "Warning"
 *
 * [LoggerMessage] source generator — zero-allocation (net6+):
 *   [LoggerMessage(EventId=1, Level=LogLevel.Info, Message="Order {Id}")]
 *   public static partial void OrderReceived(ILogger logger, int id);
 *
 * LoggerMessage.Define — legacy cached delegate (net5 and below):
 *   static readonly Action<ILogger, int, Exception?> _log =
 *       LoggerMessage.Define<int>(LogLevel.Info, new EventId(1), "Order {Id}");
 *
 * ActivitySource / Activity:
 *   static readonly ActivitySource Src = new ActivitySource("MyApp", "1.0");
 *   using Activity? span = Src.StartActivity("op.name", ActivityKind.Internal);
 *   span?.SetTag("key", value);    // metadata on this span
 *   span?.AddEvent(new ActivityEvent("milestone"));
 *   span?.SetStatus(ActivityStatusCode.Error, "msg");
 *
 * DiagnosticListener:
 *   DiagnosticListener.AllListeners.Subscribe(observer) — subscribe to all listeners
 *   observer implements IObserver<DiagnosticListener> and IObserver<KVP<string,object?>>
 *
 * Health checks:
 *   AddHealthChecks().AddCheck<MyCheck>("name", tags: new[]{"tag"})
 *   MapHealthChecks("/health")
 *   MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = r => r.Tags.Contains("critical") })
 *   HealthCheckResult.Healthy("msg", data)
 *   HealthCheckResult.Degraded("msg", ex?, data)  — HTTP 200; warn only
 *   HealthCheckResult.Unhealthy("msg", ex?, data) — HTTP 503; shed traffic
 */
