/*
 * FILE ROLE: Demonstrates ILogger<T> dependency injection, all six log levels,
 *            structured logging with message templates, and log scopes (BeginScope).
 * SECTIONS IN THIS FILE:
 *   1. ILogger<T> and ILoggerFactory — DI, categories, and log levels
 *   2. Structured Logging — message templates and named placeholders
 *   3. Log Scopes — BeginScope for ambient correlation context
 */

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LoggingDiagnostics.Services;

/*
 * SECTION 1: ILogger<T> — DEPENDENCY INJECTION AND LOG LEVELS
 *
 * ILogger<T> is the primary logging abstraction in ASP.NET Core.
 * The generic parameter T sets the log "category" — the full class name used
 * to route messages to providers and control levels in appsettings.json.
 *
 * SIX LOG LEVELS (lowest → highest severity):
 * ┌───────────────────┬───────┬─────────────────────────────────────────────────┐
 * │ Level             │ Value │ When to use                                     │
 * ├───────────────────┼───────┼─────────────────────────────────────────────────┤
 * │ LogLevel.Trace    │   0   │ Step-by-step execution; always off in prod.     │
 * │ LogLevel.Debug    │   1   │ Developer diagnostics; useful during debugging. │
 * │ LogLevel.Information │ 2  │ Normal operations: startup, requests served.   │
 * │ LogLevel.Warning  │   3   │ Recoverable anomaly; may need attention.        │
 * │ LogLevel.Error    │   4   │ Failed operation or unhandled exception.        │
 * │ LogLevel.Critical │   5   │ System failure requiring immediate action.      │
 * │ LogLevel.None     │   6   │ Disables the category entirely.                 │
 * └───────────────────┴───────┴─────────────────────────────────────────────────┘
 *
 * CATEGORY MATCHING — longest-prefix wins in appsettings.json:
 *   "LoggingDiagnostics.Services.OrderProcessingService": "Trace" ← most specific
 *   "LoggingDiagnostics.Services": "Debug"
 *   "LoggingDiagnostics": "Information"
 *   "Default": "Warning"                                          ← least specific
 *
 * ILoggerFactory — when DI is not available or you need a logger by string category:
 *   ILoggerFactory factory = LoggerFactory.Create(b => b.AddConsole());
 *   ILogger<MyService> logger = factory.CreateLogger<MyService>();
 *   ILogger        named   = factory.CreateLogger("MyApp.Startup");
 *   factory.Dispose(); // disposes all providers created with it
 *   Prefer injecting ILogger<T> directly in DI-enabled types. ILoggerFactory
 *   is useful in Program.cs startup or non-DI contexts (static helpers, tests).
 *
 * IsEnabled GUARD — skip expensive formatting when the level is filtered out:
 *   if (_logger.IsEnabled(LogLevel.Debug))
 *       _logger.LogDebug("Payload: {Data}", SerializeExpensiveObject());
 */
public sealed class OrderProcessingService
{
    private readonly ILogger<OrderProcessingService> _logger; // category = full class name

    public OrderProcessingService(ILogger<OrderProcessingService> logger) // injected by DI container
    {
        _logger = logger;
    }

    /*
     * SECTION 2: STRUCTURED LOGGING — MESSAGE TEMPLATES AND NAMED PLACEHOLDERS
     *
     * Structured logging stores data as key-value properties, not flat strings.
     * This enables filtering, searching, and aggregation in log sinks (Seq, ELK, App Insights).
     *
     * TEMPLATE SYNTAX: "Processing order {OrderId} for customer {CustomerId}"
     *   • {OrderId}  — placeholder name; becomes a queryable property on the log entry
     *   • Arguments bind LEFT-TO-RIGHT by position — orderId fills {OrderId}, customerId fills {CustomerId}
     *   • DO NOT use string interpolation: $"Order {orderId}" — it loses structure entirely
     *
     * DESTRUCTURING OPERATOR @:
     *   {Order}  → calls ToString() on the argument (default; stores a flat string)
     *   {@Order} → serializes the object's properties (like JSON) for structured sinks
     *              e.g. _logger.LogInformation("Created {@Order}", orderDto)
     *
     * EVENT IDs — numeric identifiers for each named event type:
     *   _logger.LogInformation(new EventId(1001, "OrderReceived"), "Order {Id} received", id);
     *   Useful for monitoring rules, alerts, and filtering by event in log aggregators.
     *
     * ALL SIX LEVEL SHORTHAND METHODS:
     *   _logger.LogTrace(...)        → LogLevel.Trace (0)
     *   _logger.LogDebug(...)        → LogLevel.Debug (1)
     *   _logger.LogInformation(...)  → LogLevel.Information (2)
     *   _logger.LogWarning(...)      → LogLevel.Warning (3)
     *   _logger.LogError(ex, ...)    → LogLevel.Error (4)  — exception as first arg
     *   _logger.LogCritical(ex, ...) → LogLevel.Critical (5)
     *
     * GENERIC OVERLOAD — pass level as a variable:
     *   _logger.Log(level, "Message {Val}", val);
     */
    public async Task ProcessOrderAsync(int orderId, string customerId, CancellationToken ct = default)
    {
        _logger.LogTrace("Entering ProcessOrderAsync — order {OrderId}", orderId); // step-level; usually filtered out

        _logger.LogDebug("Fetching details for order {OrderId} (customer {CustomerId})", orderId, customerId);

        await SimulateWorkAsync(ct); // represents async DB / API call

        bool isHighValue = orderId > 1000; // simplified business rule
        if (isHighValue)
        {
            _logger.LogWarning(
                "High-value order {OrderId} detected; manual review recommended", orderId); // recoverable anomaly
        }

        // Both orderId and customerId become queryable log properties — not just a string
        _logger.LogInformation(
            "Order {OrderId} processed for customer {CustomerId}", orderId, customerId);

        try
        {
            await ValidateInventoryAsync(orderId, ct);
        }
        catch (InvalidOperationException ex)
        {
            // Exception is always the FIRST argument to LogError/LogCritical
            _logger.LogError(ex, "Inventory validation failed for order {OrderId}", orderId);
        }
    }

    /*
     * SECTION 3: LOG SCOPES — BeginScope FOR AMBIENT CORRELATION CONTEXT
     *
     * A scope attaches key-value pairs to every log message emitted while
     * the scope is active. All nested calls inherit the scope automatically.
     *
     * WHY IT MATTERS:
     *   Without scopes, log lines for the same order scatter through the log stream.
     *   With a scope, every line for order 42 carries "Order 42" — easy to filter.
     *
     * SETUP — providers must be configured to surface scope data:
     *   "Console": { "FormatterOptions": { "IncludeScopes": true } }  — in appsettings.json
     *
     * TWO OVERLOADS:
     *   string template:  _logger.BeginScope("Order {OrderId}", orderId)
     *   dictionary:       _logger.BeginScope(new Dictionary<string, object?> { ["OrderId"] = orderId })
     *
     *   The dictionary form passes structured properties to rich sinks (Serilog, SEQ).
     *   The string form is simpler and sufficient for Console output.
     *
     * LIFECYCLE — BeginScope returns IDisposable?:
     *   The scope is active until Dispose() is called (end of `using` block).
     *   Null-safe with IDisposable? — if the provider does not support scopes,
     *   BeginScope returns null; the `using IDisposable?` pattern handles that safely.
     *
     * NESTING — scopes accumulate; all active scopes appear on each message:
     *   outer scope: "Order {OrderId}" → "Order 42"
     *   inner scope: { "Step": "Shipment" }
     *   result: message carries both "Order 42" and Step=Shipment
     */
    public async Task FulfillOrderAsync(int orderId, CancellationToken ct = default)
    {
        // Outer scope — all log messages in this method carry "Order {orderId}"
        using IDisposable? orderScope = _logger.BeginScope("Order {OrderId}", orderId);

        _logger.LogInformation("Starting fulfillment pipeline");

        // Inner (nested) scope — structured properties form; adds Step=Shipment on top of the outer scope
        using IDisposable? shipmentScope = _logger.BeginScope(
            new Dictionary<string, object?> { ["Step"] = "Shipment" });

        _logger.LogInformation("Dispatching shipment notification"); // carries both outer and inner scope

        await SimulateWorkAsync(ct);

        _logger.LogInformation("Fulfillment complete");
        // shipmentScope disposed here; orderScope disposed at end of method
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private static Task SimulateWorkAsync(CancellationToken ct) =>
        Task.Delay(10, ct); // stand-in for real async I/O

    private static Task ValidateInventoryAsync(int orderId, CancellationToken ct)
    {
        if (orderId < 0) // triggers the error path for demonstration purposes
            throw new InvalidOperationException($"Inventory check failed for order {orderId}");
        return Task.CompletedTask;
    }
}
