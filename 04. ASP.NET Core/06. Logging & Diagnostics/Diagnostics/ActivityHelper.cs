/*
 * FILE ROLE: Demonstrates ActivitySource and Activity for distributed tracing spans,
 *            and DiagnosticListener for observing framework diagnostic events.
 * SECTIONS IN THIS FILE:
 *   6. ActivitySource and Activity — creating, enriching, and completing trace spans
 *   7. DiagnosticListener — subscribing to framework-level diagnostic event streams
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LoggingDiagnostics.Diagnostics;

/*
 * SECTION 6: ActivitySource AND Activity — DISTRIBUTED TRACING PRIMITIVES
 *
 * CONCEPT — distributed tracing (W3C Trace Context):
 *   A trace represents a request flowing across multiple services.
 *   Each unit of work within a trace is a SPAN (.NET: Activity). Spans form a tree:
 *     TraceId  — unique id for the entire end-to-end request (propagated via HTTP headers)
 *     SpanId   — unique id for this single span
 *     ParentId — the span that created this one (null at the root)
 *
 * .NET TYPES:
 *   ActivitySource  — a named factory for Activity instances; one per library/component.
 *                     Name identifies the instrumentation library (e.g., "MyApp.Orders").
 *   Activity        — represents one span; carries TraceId, SpanId, ParentId, Tags, Baggage, Events.
 *   ActivityKind    — OpenTelemetry semantic roles: Server, Client, Producer, Consumer, Internal.
 *
 * LIFECYCLE:
 *   1. Create one static ActivitySource per component (shared, never disposed during the app lifetime).
 *   2. Call source.StartActivity("operation.name") — returns null if no listener is sampling.
 *   3. Enrich: add Tags (this-span metadata), Baggage (propagated downstream), Events (milestones).
 *   4. Dispose the activity when the operation ends — this records the end time and fires ActivityStopped.
 *
 * ALWAYS NULL-CHECK:
 *   StartActivity returns Activity? — null when no ActivityListener is attached and sampling.
 *   Use the null-conditional operator: activity?.SetTag("key", "value")
 *
 * SAMPLING — ActivityListener controls which activities are created:
 *   ActivitySamplingResult.None     → Activity object is not created (returns null)
 *   ActivitySamplingResult.AllData  → Activity is created; all Tags/Events are recorded
 *   ActivitySamplingResult.AllDataAndRecorded → all data + marks the activity as "recorded" for exporters
 *   Without any listener, StartActivity always returns null (no overhead in production without a collector).
 *
 * NAMING CONVENTIONS (OpenTelemetry semantic conventions):
 *   Source name : "CompanyName.ComponentName" (e.g., "Contoso.Orders")
 *   Activity name: "verb.noun" — "order.process", "db.query", "http.send"
 *
 * Tags vs Baggage:
 *   Tags    — key/value attached to THIS span only; NOT propagated downstream.
 *   Baggage — key/value propagated to all downstream services via HTTP headers.
 *             Keep baggage minimal — every key/value adds to every outbound header.
 */
public static class ActivityHelper
{
    // Static ActivitySource — one per component; name identifies this instrumentation library
    public static readonly ActivitySource Source =
        new ActivitySource("LoggingDiagnostics.Orders", "1.0.0"); // name, version

    /*
     * StartOrderActivity — creates and enriches a trace span for an order operation.
     * Returns null when no listener is sampling (typical in production without OTEL).
     * The caller MUST dispose the returned Activity via `using`.
     */
    public static Activity? StartOrderActivity(string operationName, int orderId, string customerId)
    {
        Activity? activity = Source.StartActivity(operationName, ActivityKind.Internal);

        if (activity is not null) // only enrich when the span is actually being sampled
        {
            activity.SetTag("order.id", orderId);             // indexed metadata on this span only
            activity.SetTag("order.customer_id", customerId);
            activity.SetTag("component", "OrderProcessing");

            // Baggage propagates to downstream services — use sparingly; adds to every request header
            activity.SetBaggage("correlation.id", Guid.NewGuid().ToString("N"));
        }

        return activity;
    }

    /*
     * RecordException — marks the span as failed and attaches exception details.
     * Follows OpenTelemetry semantic conventions for exception events.
     */
    public static void RecordException(Activity? activity, Exception ex)
    {
        if (activity is null) return; // no span — nothing to record on

        activity.SetStatus(ActivityStatusCode.Error, ex.Message); // marks the span as errored

        // AddEvent records a named point-in-time event with structured tags on the span
        ActivityTagsCollection tags = new ActivityTagsCollection
        {
            { "exception.type", ex.GetType().FullName ?? ex.GetType().Name }, // FullName is string? — fallback to Name
            { "exception.message", ex.Message },
            { "exception.stacktrace", ex.StackTrace }
        };
        activity.AddEvent(new ActivityEvent("exception", tags: tags)); // OTel convention: event named "exception"
    }

    /*
     * DemonstrateUsageAsync — full Activity lifecycle in one method.
     * In real code this pattern appears in services, repositories, and middleware.
     */
    public static async Task DemonstrateUsageAsync(int orderId)
    {
        // `using` ends the span automatically (records end time, fires ActivityStopped callback)
        using Activity? activity = StartOrderActivity("order.process", orderId, "CUST-42");

        activity?.AddEvent(new ActivityEvent("order.validation.started")); // milestone event

        await Task.Delay(10); // simulate async I/O — the span duration is measured automatically

        activity?.SetTag("order.items_count", 3); // tags can be added at any point during the span

        activity?.AddEvent(new ActivityEvent("order.validation.complete"));

        // Success path: ActivityStatusCode remains Unset (= OK in OTEL convention)
        // Failure path: call RecordException(activity, ex) before re-throwing
    }
}

/*
 * SECTION 7: DiagnosticListener — OBSERVING FRAMEWORK DIAGNOSTIC EVENTS
 *
 * DiagnosticListener is an internal pub/sub event bus used by ASP.NET Core,
 * HttpClient, EF Core, and other framework components to emit diagnostic events
 * (not log messages) as strongly-typed or anonymous payloads.
 *
 * RELATIONSHIP TO ActivitySource:
 *   DiagnosticListener predates ActivitySource and is used by older framework components.
 *   ActivitySource is the modern replacement for new custom instrumentation.
 *   OpenTelemetry bridges both automatically — you do not normally subscribe to
 *   DiagnosticListener directly unless building a custom instrumentation library.
 *
 * SUBSCRIPTION MODEL (two-layer observer pattern):
 *   Layer 1 — DiagnosticListener.AllListeners : IObservable<DiagnosticListener>
 *             Fires once for each new DiagnosticListener created anywhere in the process.
 *   Layer 2 — the specific DiagnosticListener : IObservable<KeyValuePair<string, object?>>
 *             Fires for each event emitted by that listener.
 *
 * COMMON LISTENER NAMES:
 *   "Microsoft.AspNetCore"              — ASP.NET Core request pipeline events
 *   "HttpHandlerDiagnosticListener"     — HttpClient send/receive events
 *   "Microsoft.EntityFrameworkCore"     — EF Core query command events
 *
 * PITFALL — synchronous; never block in OnNext:
 *   DiagnosticListener events fire synchronously on the emitting thread.
 *   Observers must be lightweight. Queue heavy work to a background channel.
 */
public sealed class DiagnosticObserver
    : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object?>>, IDisposable
{
    private readonly string _listenerName;            // which DiagnosticListener to subscribe to
    private IDisposable? _allListenersSubscription;   // subscription to AllListeners (outer)
    private IDisposable? _listenerSubscription;       // subscription to the specific listener (inner)

    private DiagnosticObserver(string listenerName) // private — use Subscribe() factory method
    {
        _listenerName = listenerName;
    }

    /*
     * Subscribe — factory method; subscribes to AllListeners and returns the observer.
     * Caller should store the returned DiagnosticObserver and Dispose() it on shutdown.
     */
    public static DiagnosticObserver Subscribe(string listenerName)
    {
        DiagnosticObserver observer = new DiagnosticObserver(listenerName);
        // Subscribe to the global stream of all DiagnosticListeners in the process
        observer._allListenersSubscription = DiagnosticListener.AllListeners.Subscribe(observer);
        return observer;
    }

    // Called for every DiagnosticListener created in the process
    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener listener)
    {
        if (listener.Name == _listenerName) // only subscribe to the one listener we care about
        {
            // `this` implements IObserver<KeyValuePair<string, object?>> — subscribe to its events
            _listenerSubscription = listener.Subscribe(this);
        }
    }

    // Called for each diagnostic event emitted by the subscribed listener
    void IObserver<KeyValuePair<string, object?>>.OnNext(KeyValuePair<string, object?> value)
    {
        // value.Key   = event name (e.g. "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")
        // value.Value = payload object (framework-specific type; use pattern matching or reflection)
        Console.WriteLine($"[DiagnosticEvent] {value.Key}"); // real observer would cast and process value.Value
    }

    void IObserver<DiagnosticListener>.OnCompleted() { }
    void IObserver<DiagnosticListener>.OnError(Exception error) { }
    void IObserver<KeyValuePair<string, object?>>.OnCompleted() { }
    void IObserver<KeyValuePair<string, object?>>.OnError(Exception error) { }

    // Dispose unsubscribes from both the specific listener and AllListeners
    public void Dispose()
    {
        _listenerSubscription?.Dispose();
        _allListenersSubscription?.Dispose();
    }
}
