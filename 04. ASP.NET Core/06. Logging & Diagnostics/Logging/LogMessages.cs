/*
 * FILE ROLE: Demonstrates the [LoggerMessage] source generator for zero-allocation
 *            high-performance logging, and the older LoggerMessage.Define delegate API.
 * SECTIONS IN THIS FILE:
 *   4. [LoggerMessage] source generator — compile-time–safe, zero-allocation logging
 *   5. LoggerMessage.Define — manual delegate-based high-performance logging (legacy)
 */

using Microsoft.Extensions.Logging;
using System;

namespace LoggingDiagnostics.Logging;

/*
 * SECTION 4: [LoggerMessage] SOURCE GENERATOR — HIGH-PERFORMANCE LOGGING
 *
 * PROBLEM with _logger.LogInformation("...", arg1, arg2):
 *   Each call allocates a params object[] for arguments, boxes any value types,
 *   and formats the message string — even when the log level is filtered out.
 *   At high throughput (thousands of requests/sec) this adds measurable GC pressure.
 *
 * SOLUTION — [LoggerMessage] source generator (introduced .NET 6 / C# 10):
 *   The Roslyn source generator produces an optimized partial method implementation that:
 *   ✓ Caches a compiled log action at class-init time (zero per-call allocation for the action)
 *   ✓ Inlines an IsEnabled check — skips ALL work when the level is filtered out
 *   ✓ Uses a typed delegate (no params object[], no boxing of value types)
 *   ✓ Makes placeholder names and event IDs part of the public API signature (compile-time safety)
 *
 * REQUIREMENTS:
 *   • The class must be partial (and optionally static)
 *   • The method must be partial, return void, take ILogger as the first parameter
 *   • [LoggerMessage] attribute supplies EventId, Level, and the message template
 *   • Exception parameters (if any) are typed System.Exception or a derived type
 *     — position them after ILogger and before the template parameters
 *
 * NAMING CONVENTION — name after the event, not after the log level:
 *   Good: OrderReceived, PaymentFailed, InventoryShortfall
 *   Bad:  LogInformationOrder, LogErrorPayment
 *
 * TEMPLATE RULES — identical to ILogger message templates:
 *   • {OrderId} placeholder name must match the corresponding method parameter name exactly
 *   • Use {@Param} to request object destructuring in rich sinks (Seq, Serilog)
 */
public static partial class LogMessages
{
    // EventId = numeric identifier; a name makes log aggregator rules human-readable
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Order {OrderId} received from customer {CustomerId}")]
    public static partial void OrderReceived(ILogger logger, int orderId, string customerId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Order {OrderId} — inventory shortfall: {Missing} units of SKU {Sku}")]
    public static partial void InventoryShortfall(ILogger logger, int orderId, int missing, string sku);

    // Exception parameter comes after ILogger and before the message template parameters
    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Payment failed for order {OrderId}")]
    public static partial void PaymentFailed(ILogger logger, Exception exception, int orderId);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Critical,
        Message = "Order subsystem unrecoverable error — instance {InstanceId}")]
    public static partial void OrderSubsystemCritical(ILogger logger, Exception exception, string instanceId);

    /*
     * DYNAMIC LEVEL VARIANT — omit Level from the attribute, pass it as a LogLevel parameter.
     * The generated code uses the runtime value as the log level.
     * Useful when severity depends on a runtime condition (e.g., retry count → Warning vs Error).
     */
    [LoggerMessage(
        EventId = 1005,
        Message = "Order {OrderId} state changed to {State}")]
    public static partial void OrderStateChanged(ILogger logger, LogLevel level, int orderId, string state);
}

/*
 * SECTION 5: LoggerMessage.Define — MANUAL DELEGATE-BASED HIGH-PERFORMANCE LOGGING
 *
 * Before the source generator existed (.NET 5 and earlier), the recommended
 * high-performance pattern used LoggerMessage.Define to create a cached delegate.
 *
 * HOW IT WORKS:
 *   LoggerMessage.Define<T1, T2>(level, eventId, messageTemplate)
 *   returns an Action<ILogger, T1, T2, Exception?> compiled once at class-init
 *   that skips formatting when the level is filtered out.
 *
 * COMPARISON: [LoggerMessage] source generator vs LoggerMessage.Define
 * ┌─────────────────────────────┬──────────────────────────┬──────────────────────────────┐
 * │ Aspect                      │ [LoggerMessage] (gen)    │ LoggerMessage.Define         │
 * ├─────────────────────────────┼──────────────────────────┼──────────────────────────────┤
 * │ Syntax                      │ Attribute + partial meth │ Static delegate field        │
 * │ Compile-time placeholder    │ Yes — names checked      │ No — positional only         │
 * │ .NET version requirement    │ .NET 6+                  │ All .NET Core versions       │
 * │ Boilerplate                 │ Minimal                  │ Verbose (field + method)     │
 * │ Exception parameter         │ Typed (Exception)        │ Always last (Exception?)     │
 * │ Recommended for new code    │ Yes                      │ Legacy / .NET 5 and below    │
 * └─────────────────────────────┴──────────────────────────┴──────────────────────────────┘
 *
 * RULE: Prefer the source generator for all new .NET 6+ code.
 *       Use LoggerMessage.Define only when targeting .NET 5 or maintaining legacy code.
 *
 * RETURN TYPE — Action<ILogger, T1, ..., Tn, Exception?>:
 *   The last parameter of the returned Action is always Exception?
 *   Pass null when no exception is associated with the event.
 */
public static class LegacyLogMessages
{
    // Compiled once at class-init; Action<ILogger, orderId, customerId, Exception?>
    private static readonly Action<ILogger, int, string, Exception?> s_orderReceived =
        LoggerMessage.Define<int, string>(
            logLevel: LogLevel.Information,
            eventId: new EventId(2001, "LegacyOrderReceived"),
            formatString: "Order {OrderId} received from {CustomerId}");

    // Action<ILogger, orderId, Exception?> — only one template arg so Define<int>
    private static readonly Action<ILogger, int, Exception?> s_paymentFailed =
        LoggerMessage.Define<int>(
            logLevel: LogLevel.Error,
            eventId: new EventId(2002, "LegacyPaymentFailed"),
            formatString: "Payment failed for order {OrderId}");

    // Wrapper methods keep call-sites clean — callers do not deal with the delegate directly
    public static void OrderReceived(ILogger logger, int orderId, string customerId) =>
        s_orderReceived(logger, orderId, customerId, null); // null = no associated exception

    public static void PaymentFailed(ILogger logger, int orderId, Exception ex) =>
        s_paymentFailed(logger, orderId, ex); // exception is the last argument to the delegate
}
