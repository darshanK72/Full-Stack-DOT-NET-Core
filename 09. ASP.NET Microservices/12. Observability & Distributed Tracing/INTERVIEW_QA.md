# Observability & Distributed Tracing — Interview Q&A
> 36 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What does "observability" mean in the context of microservices, and how does it differ from monitoring?](#q1-what-does-observability-mean-in-the-context-of-microservices-and-how-does-it-differ-from-monitoring)
2. [Q2. What are the three pillars of observability, and what does each one measure?](#q2-what-are-the-three-pillars-of-observability-and-what-does-each-one-measure)
3. [Q3. Why is observability harder in a microservices architecture than in a monolith?](#q3-why-is-observability-harder-in-a-microservices-architecture-than-in-a-monolith)
4. [Q4. What is the difference between a log, a metric, and a trace?](#q4-what-is-the-difference-between-a-log-a-metric-and-a-trace)
5. [Q5. What is the difference between white-box monitoring and black-box monitoring?](#q5-what-is-the-difference-between-white-box-monitoring-and-black-box-monitoring)
6. [Q6. What is a service map (or dependency graph), and how is it produced?](#q6-what-is-a-service-map-or-dependency-graph-and-how-is-it-produced)
7. [Q7. What is a distributed trace, and what is a span?](#q7-what-is-a-distributed-trace-and-what-is-a-span)
8. [Q8. What is OpenTelemetry (OTel), and what problem does it solve?](#q8-what-is-opentelemetry-otel-and-what-problem-does-it-solve)
9. [Q9. What is the W3C Trace Context standard, and what are the `traceparent` and `tracestate` headers?](#q9-what-is-the-w3c-trace-context-standard-and-what-are-the-traceparent-and-tracestate-headers)
10. [Q10. What is the `Activity` class in .NET, and how does it relate to OpenTelemetry spans?](#q10-what-is-the-activity-class-in-net-and-how-does-it-relate-to-opentelemetry-spans)
11. [Q11. How do you add OpenTelemetry tracing to an ASP.NET Core microservice with outbound `HttpClient` calls?](#q11-how-do-you-add-opentelemetry-tracing-to-an-aspnet-core-microservice-with-outbound-httpclient-calls)
12. [Q12. What is the OpenTelemetry Collector, and why would you use one instead of exporting directly from the app?](#q12-what-is-the-opentelemetry-collector-and-why-would-you-use-one-instead-of-exporting-directly-from-the-app)
13. [Q13. What is sampling in distributed tracing, and what is the difference between head-based and tail-based sampling?](#q13-what-is-sampling-in-distributed-tracing-and-what-is-the-difference-between-head-based-and-tail-based-sampling)
14. [Q14. What are the common distributed tracing backends compatible with OpenTelemetry?](#q14-what-are-the-common-distributed-tracing-backends-compatible-with-opentelemetry)
15. [Q15. What is the difference between a trace exporter and a trace processor in the OTel SDK?](#q15-what-is-the-difference-between-a-trace-exporter-and-a-trace-processor-in-the-otel-sdk)
16. [Q16. How do you create a custom span inside an existing trace using `ActivitySource`?](#q16-how-do-you-create-a-custom-span-inside-an-existing-trace-using-activitysource)
17. [Q17. What is structured logging, and why is it preferred over plain-text logging in microservices?](#q17-what-is-structured-logging-and-why-is-it-preferred-over-plain-text-logging-in-microservices)
18. [Q18. What is Serilog, and how does it differ from the built-in `Microsoft.Extensions.Logging` framework?](#q18-what-is-serilog-and-how-does-it-differ-from-the-built-in-microsoftextensionslogging-framework)
19. [Q19. What is a log enricher in Serilog, and why is it useful in microservices?](#q19-what-is-a-log-enricher-in-serilog-and-why-is-it-useful-in-microservices)
20. [Q20. How do you correlate log entries across multiple microservices that collaborate on a single request?](#q20-how-do-you-correlate-log-entries-across-multiple-microservices-that-collaborate-on-a-single-request)
21. [Q21. What is a log aggregation system, and which ones are commonly used with .NET microservices?](#q21-what-is-a-log-aggregation-system-and-which-ones-are-commonly-used-with-net-microservices)
22. [Q22. What is the difference between a log's trace ID and a correlation ID?](#q22-what-is-the-difference-between-a-logs-trace-id-and-a-correlation-id)
23. [Q23. What is `LoggerMessage.Define` / source-generated logging, and when should you prefer it over `_logger.LogInformation(...)`?](#q23-what-is-loggermessagedefine-source-generated-logging-and-when-should-you-prefer-it-over-_loggerloginformation)
24. [Q24. What are metrics, and how do they differ from traces and logs in an observability stack?](#q24-what-are-metrics-and-how-do-they-differ-from-traces-and-logs-in-an-observability-stack)
25. [Q25. What is the .NET `Meter` API (`System.Diagnostics.Metrics`), and how does it relate to OpenTelemetry?](#q25-what-is-the-net-meter-api-systemdiagnosticsmetrics-and-how-does-it-relate-to-opentelemetry)
26. [Q26. What are the four metric instrument types in OpenTelemetry/Prometheus (Counter, Gauge, Histogram, Summary)?](#q26-what-are-the-four-metric-instrument-types-in-opentelemetryprometheus-counter-gauge-histogram-summary)
27. [Q27. What is Prometheus, and how does it scrape metrics from an ASP.NET Core service?](#q27-what-is-prometheus-and-how-does-it-scrape-metrics-from-an-aspnet-core-service)
28. [Q28. How do you expose built-in .NET runtime and ASP.NET Core metrics via OpenTelemetry?](#q28-how-do-you-expose-built-in-net-runtime-and-aspnet-core-metrics-via-opentelemetry)
29. [Q29. What is Grafana, and how does it relate to Prometheus in a typical metrics stack?](#q29-what-is-grafana-and-how-does-it-relate-to-prometheus-in-a-typical-metrics-stack)
30. [Q30. What is the difference between pull-based and push-based metrics collection, and which model does Prometheus use?](#q30-what-is-the-difference-between-pull-based-and-push-based-metrics-collection-and-which-model-does-prometheus-use)
31. [Q31. What are health checks in ASP.NET Core, and why are they important in a microservices deployment?](#q31-what-are-health-checks-in-aspnet-core-and-why-are-they-important-in-a-microservices-deployment)
32. [Q32. What is the difference between a liveness probe and a readiness probe in Kubernetes?](#q32-what-is-the-difference-between-a-liveness-probe-and-a-readiness-probe-in-kubernetes)
33. [Q33. How do you add and expose a health check endpoint in ASP.NET Core?](#q33-how-do-you-add-and-expose-a-health-check-endpoint-in-aspnet-core)
34. [Q34. How do you write a custom `IHealthCheck` for an external dependency like a database?](#q34-how-do-you-write-a-custom-ihealthcheck-for-an-external-dependency-like-a-database)
35. [Q35. What is a startup probe, and how does it differ from liveness and readiness probes?](#q35-what-is-a-startup-probe-and-how-does-it-differ-from-liveness-and-readiness-probes)
36. [Q36. What HTTP status codes do ASP.NET Core health check endpoints return, and what do they mean?](#q36-what-http-status-codes-do-aspnet-core-health-check-endpoints-return-and-what-do-they-mean)

---

## Q1. What does "observability" mean in the context of microservices, and how does it differ from monitoring?

What does "observability" mean in the context of microservices, and how does it differ from monitoring?

**Answer:** Observability is the ability to understand what is happening inside a system purely from its external outputs — logs, metrics, and traces — without needing to deploy new code to ask new questions. Monitoring, by contrast, means checking whether predefined thresholds are breached; it tells you *that* something is wrong, not *why*. An observable system lets you explore arbitrary questions about its internal state after the fact, which is essential in microservices where failures often span many independent services.

- Monitoring is reactive and alert-driven: you decide in advance what to measure and receive alerts when a value crosses a boundary.
- Observability is exploratory: when an incident happens, you can query any dimension of the data — even ones you never anticipated — to find the root cause.
- Observability becomes critical in microservices because a request failure in Service A may be caused by latency in Service C two hops away; no single threshold alert covers that chain.
- The term comes from control theory: a system is observable if its internal state can be inferred from its outputs over time.

---

## Q2. What are the three pillars of observability, and what does each one measure?

What are the three pillars of observability, and what does each one measure?

**Answer:** The three pillars are logs, metrics, and traces. Each captures a different dimension of system behavior, and together they give a complete picture: metrics show you *what* is degraded, traces show you *where* in the call chain it broke, and logs show you *why* a specific operation failed.

| Pillar | What it captures | Example tool |
|---|---|---|
| **Logs** | Discrete timestamped events with context | Serilog → Seq / Elastic |
| **Metrics** | Numeric measurements aggregated over time | Prometheus + Grafana |
| **Traces** | End-to-end call chains across services | OpenTelemetry → Jaeger / Zipkin |

- Logs answer point-in-time questions: "what happened during order 42's checkout?"
- Metrics answer trend questions: "has p99 latency on the payment service been rising for the last hour?"
- Traces answer path questions: "which service in the chain is responsible for this 3-second response?"
- OpenTelemetry 1.0 unifies all three under a single SDK and wire format, so correlation between pillars is first-class rather than bolted on.

---

## Q3. Why is observability harder in a microservices architecture than in a monolith?

Why is observability harder in a microservices architecture than in a monolith?

**Answer:** In a monolith, a single process produces a single stream of logs, and a stack trace fully describes a failure. In microservices, a single user request may touch a dozen services across different hosts, networks, and runtimes, so no individual service can see the full picture. Assembling that picture requires deliberate instrumentation and context propagation at every boundary.

- Each service has its own log stream; without correlation IDs and a log aggregation system, tracing a failure means manually searching across multiple tools.
- Network calls between services introduce new failure modes (timeouts, partial failures, retries) that don't exist in-process, and each hop adds latency that is invisible without distributed tracing.
- Services may be owned by different teams and written in different languages, making a consistent instrumentation strategy harder to enforce.
- Cascading failures are common: Service A is slow not because of its own code but because Service B is slow, which is waiting on a degraded database — only a distributed trace reveals this chain.

---

## Q4. What is the difference between a log, a metric, and a trace?

What is the difference between a log, a metric, and a trace?

**Answer:** A log is a discrete timestamped record of a specific event, rich in context but unstructured by nature. A metric is a numeric measurement aggregated over time — designed for cheap, high-frequency collection. A trace is a causally linked set of operations across service boundaries, showing the full execution path of a single request.

| Signal | Cardinality | Cost | Best for |
|---|---|---|---|
| Log | High (one record per event) | Medium | Diagnosing specific failures |
| Metric | Low (aggregated numbers) | Low | Alerting on trends and thresholds |
| Trace | Medium (sampled) | Medium | Understanding request flow and latency |

- Logs retain full event detail but are expensive to store and query at scale.
- Metrics are cheap to collect and query, but they discard individual-event detail — you know the 95th-percentile latency but not which specific request was slow.
- Traces are typically sampled in production to manage cost while preserving the ability to investigate individual request paths.

---

## Q5. What is the difference between white-box monitoring and black-box monitoring?

What is the difference between white-box monitoring and black-box monitoring?

**Answer:** White-box monitoring instruments the internals of a system — reading process metrics, custom counters, and application-level events that only the system itself can expose. Black-box monitoring probes the system from the outside — sending real or synthetic requests and observing the response — without access to internal state. Both are needed: white-box catches root causes early, black-box catches the user-facing symptoms that matter most.

- White-box examples: CPU/memory gauges, database query duration histograms, queue depth counters, `Activity`-based spans.
- Black-box examples: synthetic health check pings, external uptime monitors (e.g., checking an API endpoint every 30 seconds from outside the cluster).
- White-box gives rich diagnostic detail but can miss problems that only manifest at the network boundary or load balancer.
- Black-box is the ground truth for "is the service working from a user's perspective?" but gives little diagnostic information when it fails.

---

## Q6. What is a service map (or dependency graph), and how is it produced?

What is a service map (or dependency graph), and how is it produced?

**Answer:** A service map is a visual graph showing which services call which other services, along with live health and latency information on each edge. It is produced automatically by a distributed tracing backend — tools like Jaeger, Azure Application Insights, or AWS X-Ray parse the span data coming from instrumented services and reconstruct the call graph from parent-child span relationships.

- Each span in a distributed trace records the service it belongs to, its parent span ID, and the remote endpoint it called — enough information to build the graph edge by edge.
- A service map updates in near real-time as traces arrive, so it reflects the live topology of the system including dynamically discovered dependencies.
- In Azure Application Insights, the Application Map view is generated this way — it shows request rates, failure rates, and average latency per service edge without any manual configuration.
- Service maps are invaluable during incident response because they immediately show which service is the source of elevated error rates or latency in the dependency chain.

---

## Chapter 2. Distributed Tracing & OpenTelemetry

---

## Q7. What is a distributed trace, and what is a span?

What is a distributed trace, and what is a span?

**Answer:** A distributed trace is the complete record of a single request as it travels through multiple services — stitched together into a tree of operations that shows every hop, its duration, and any errors. A span is one node in that tree: it represents a single unit of work (handling an HTTP request, executing a database query, calling a downstream service) within one service, with a start time, duration, attributes, and a reference to its parent span.

- Every trace has a globally unique `traceId` (128-bit identifier); every span has its own `spanId` (64-bit) and records its parent's `spanId`, which is how the tree is assembled.
- The root span is created when the first service receives the request; every downstream call starts a child span that inherits the same `traceId`.
- Spans can be nested: an HTTP handler span may have a child database span, which has a child connection-pool span.
- Attributes on a span (tags/baggage) carry context: HTTP method, URL, status code, database statement — queryable fields in your tracing backend.

---

## Q8. What is OpenTelemetry (OTel), and what problem does it solve?

What is OpenTelemetry (OTel), and what problem does it solve?

**Answer:** OpenTelemetry is a vendor-neutral, open-source observability framework that provides a single API and SDK for collecting logs, metrics, and traces from any application, exporting them to any compatible backend. Before OTel, every observability vendor shipped its own SDK — switching from Jaeger to Datadog, for example, meant rewriting all instrumentation. OTel solves this by separating the instrumentation API (which your code calls) from the exporter (which decides where data goes), so you instrument once and change backends by reconfiguring an exporter.

- OTel is a merger of the OpenCensus and OpenTracing projects and is now a Cloud Native Computing Foundation (CNCF) graduated project — the industry standard for instrumentation.
- The .NET OTel SDK integrates directly with `System.Diagnostics.Activity` and `System.Diagnostics.Metrics`, both of which are built into the .NET runtime — no third-party primitives are needed in library code.
- The OTel specification defines the wire format (OTLP — OpenTelemetry Line Protocol) and semantic conventions (standard attribute names like `http.method`, `db.statement`), enabling consistent queries across services and languages.
- Popular backends — Jaeger, Zipkin, Azure Monitor, Datadog, Honeycomb — all accept OTLP, so the same instrumented app works with any of them.

---

## Q9. What is the W3C Trace Context standard, and what are the `traceparent` and `tracestate` headers?

What is the W3C Trace Context standard, and what are the `traceparent` and `tracestate` headers?

**Answer:** W3C Trace Context is an HTTP header standard that defines how distributed tracing context is propagated between services via HTTP headers. It allows services from different vendors, languages, and frameworks to participate in the same distributed trace without prior coordination, as long as they both read and forward the standard headers.

- `traceparent` carries the four required fields: version (`00`), `traceId` (32 hex chars / 128-bit), `parentId` (16 hex chars / 64-bit current span ID), and `flags` (a bitmask — bit 0 indicates the request is sampled). Example: `00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01`.
- `tracestate` is a vendor-extension header — an ordered list of `key=value` pairs that each tracing system uses to carry its own proprietary context alongside the standard fields.
- ASP.NET Core with OpenTelemetry reads and writes `traceparent` automatically through the `ActivityPropagator`; no manual header parsing is needed in application code.
- If an incoming request has no `traceparent`, the OTel SDK creates a new root span and a fresh `traceId`, starting a new trace.

---

## Q10. What is the `Activity` class in .NET, and how does it relate to OpenTelemetry spans?

What is the `Activity` class in .NET, and how does it relate to OpenTelemetry spans?

**Answer:** `System.Diagnostics.Activity` is the .NET runtime's built-in representation of a unit of work in a distributed trace — conceptually identical to an OpenTelemetry span. The OTel .NET SDK maps `Activity` to the OTel span model: when you start an `Activity`, the OTel SDK creates a corresponding span and exports it to the configured backend. This means .NET libraries (HttpClient, SqlClient, EF Core) that already instrument themselves with `Activity` automatically produce OTel spans without any extra code.

- `ActivitySource` is the factory for creating `Activity` objects; it corresponds to an OTel Tracer. Libraries expose a named `ActivitySource` (e.g., `"Microsoft.AspNetCore"`) that you subscribe to in the OTel SDK with `AddSource(...)`.
- `Activity.Current` is the ambient current span, flowing across `await` boundaries via `AsyncLocal<T>` — child spans created inside an async method automatically link to the correct parent.
- Tags on an `Activity` (`SetTag`) become span attributes in OTel; `Activity.Events` map to OTel span events.
- Because `Activity` is in the .NET runtime (not a library), it has zero overhead when no listener is attached — the OTel SDK acts as the listener and takes the overhead only when enabled.

---

## Q11. How do you add OpenTelemetry tracing to an ASP.NET Core microservice with outbound `HttpClient` calls?

How do you add OpenTelemetry tracing to an ASP.NET Core microservice with outbound `HttpClient` calls?

**Answer:** You configure OpenTelemetry in `Program.cs` by calling `AddOpenTelemetry()` on the service collection, then chaining `WithTracing(...)` to register instrumentation sources and an exporter. The ASP.NET Core and HttpClient instrumentation libraries hook into the existing `Activity`-based diagnostics that .NET already emits — no changes to controller or service code are required.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("OrderService"))
        .AddAspNetCoreInstrumentation()   // incoming HTTP requests
        .AddHttpClientInstrumentation()   // outbound HttpClient calls
        .AddSqlClientInstrumentation()    // ADO.NET / SQL Server queries
        .AddOtlpExporter());             // send to Jaeger / OTel Collector
```

- `AddAspNetCoreInstrumentation` creates a root span for each incoming request, reading `traceparent` from the request header to link it to an existing trace if one is present.
- `AddHttpClientInstrumentation` adds a `DelegatingHandler` to every `HttpClient` managed by `IHttpClientFactory`; it creates a child span for each outbound call and writes `traceparent` into the request headers automatically.
- The resource builder sets identifying attributes (service name, version) that appear on every span — essential for filtering by service in your tracing backend.
- `AddOtlpExporter` sends spans in the OTLP format (default: gRPC to `localhost:4317`); configure the endpoint via `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable for container environments.

---

## Q12. What is the OpenTelemetry Collector, and why would you use one instead of exporting directly from the app?

What is the OpenTelemetry Collector, and why would you use one instead of exporting directly from the app?

**Answer:** The OpenTelemetry Collector is a standalone agent or gateway process that receives telemetry from applications, processes it (filtering, batching, enriching), and forwards it to one or more backends. Rather than each microservice maintaining direct connections to Jaeger, Prometheus, and a logging backend simultaneously, services send OTLP data to the Collector, which fans it out. This decouples your application code from backend configuration and centralizes concerns like retry logic, buffering, and pipeline changes.

- **Decoupling:** If you switch from Jaeger to Honeycomb, you update the Collector config only — no application redeployment is needed.
- **Fan-out:** The Collector can forward the same trace data to multiple backends simultaneously (Jaeger for developers, Azure Monitor for ops) using its `pipelines` configuration.
- **Processing:** Collectors can redact sensitive span attributes (PII scrubbing), perform tail-based sampling, add infrastructure metadata, and batch data to reduce network overhead.
- **Deployment modes:** Run as a sidecar container per pod (agent mode) for low latency, or as a shared cluster-level gateway for centralized control — both are common in Kubernetes.

---

## Q13. What is sampling in distributed tracing, and what is the difference between head-based and tail-based sampling?

What is sampling in distributed tracing, and what is the difference between head-based and tail-based sampling?

**Answer:** Sampling is the practice of recording only a fraction of traces rather than every request, to control storage costs and processing overhead. At high traffic volumes, recording 100 % of traces is impractical — a service handling 10,000 requests per second would generate millions of spans per minute. Sampling lets you retain full fidelity for errors and slow requests while discarding routine successful traces.

- **Head-based sampling** makes the keep/drop decision at the start of a trace, before any spans have been collected. The decision propagates in `traceparent` flags so every service in the chain samples or drops together. It is simple and low-overhead but cannot keep "interesting" traces — an error that occurs late in the chain may be dropped because the head decision was already made.
- **Tail-based sampling** collects all spans for a trace and makes the keep/drop decision after the trace is complete — once you know whether it was slow or errored. This requires a stateful component (the OTel Collector's tail sampling processor) that buffers spans in memory until the trace is done.
- A common production strategy: 1–5 % head-based sampling for routine traffic, plus 100 % sampling for all traces containing errors or exceeding a latency threshold (tail-based).
- The `traceparent` flags field bit 0 (`sampled`) signals downstream services whether to record spans for the current trace, avoiding wasted work in unsampled chains.

---

## Q14. What are the common distributed tracing backends compatible with OpenTelemetry?

What are the common distributed tracing backends compatible with OpenTelemetry?

**Answer:** OpenTelemetry's OTLP wire format is accepted by all major tracing backends, so the choice of backend is a deployment and cost decision rather than an instrumentation one. The most common options in .NET microservices deployments are Jaeger (self-hosted, open source), Zipkin (self-hosted, open source), Azure Monitor / Application Insights (managed cloud), Datadog (managed SaaS), and Honeycomb (managed SaaS).

| Backend | Hosting | Key strength |
|---|---|---|
| Jaeger | Self-hosted (open source) | Native OTel, Kubernetes-friendly |
| Zipkin | Self-hosted (open source) | Simple setup, low resource use |
| Azure Monitor | Managed (Azure) | Native .NET SDK integration, App Map |
| Datadog | Managed SaaS | Full-stack observability, APM |
| Honeycomb | Managed SaaS | Tail-based sampling, rich querying |

- All accept OTLP via the `AddOtlpExporter()` call; some (Zipkin, Datadog) have their own exporter NuGet packages for non-OTLP protocols.
- In Azure environments, Application Insights with `Azure.Monitor.OpenTelemetry.AspNetCore` is the default choice and requires no separate infrastructure.

---

## Q15. What is the difference between a trace exporter and a trace processor in the OTel SDK?

What is the difference between a trace exporter and a trace processor in the OTel SDK?

**Answer:** A trace processor sits in the SDK pipeline between span creation and export — it receives every finished span and can batch, filter, enrich, or transform it before passing it along. A trace exporter is the final sink that serializes finished spans and sends them over the wire to a backend. Processors run in-process; exporters are the out-of-process boundary.

- The SDK ships with `BatchSpanProcessor` (default, recommended for production — buffers spans and exports in batches to reduce network round trips) and `SimpleSpanProcessor` (exports each span immediately, useful only in development).
- The `CompositeExporter` lets you chain multiple exporters (e.g., send to both Jaeger and a console exporter for local debugging) without needing multiple processors.
- You can write a custom `BaseProcessor<Activity>` to redact PII from span attributes before they leave the process — this runs before the exporter and is the correct place for data governance logic.
- The pipeline is: `ActivitySource.StartActivity()` → SDK records the span → `Processor.OnEnd(span)` → `Exporter.Export(batch)` → backend.

---

## Q16. How do you create a custom span inside an existing trace using `ActivitySource`?

How do you create a custom span inside an existing trace using `ActivitySource`?

**Answer:** You declare a static `ActivitySource` in your class (one per logical library or component), then call `StartActivity` at the point where you want to begin instrumenting a unit of work. The SDK automatically links the new span to `Activity.Current` as its parent, so it joins the existing trace with no manual ID wiring.

```csharp
private static readonly ActivitySource _source = new("OrderService.Payments");

public async Task ChargeAsync(Guid orderId)
{
    using var span = _source.StartActivity("ChargeCard");
    span?.SetTag("order.id", orderId.ToString());
    // ... work ...
}
```

- The `ActivitySource` name must be registered in `AddSource("OrderService.Payments")` during OTel setup; without registration the SDK ignores the source and `StartActivity` returns `null` — which is why the null-conditional `span?.SetTag(...)` is important.
- `StartActivity` sets `Activity.Current` to the new span for the duration of the `using` block; child spans started inside will automatically link to it.
- Dispose the `Activity` (via `using`) when the work is done; the SDK calls `OnEnd` at that point and the span is processed and exported.
- You can add events (`span.AddEvent("RetryAttempted")`), set status (`span.SetStatus(ActivityStatusCode.Error, "Card declined")`), and record exceptions (`span.RecordException(ex)`).

---

## Chapter 3. Structured Logging in Microservices

---

## Q17. What is structured logging, and why is it preferred over plain-text logging in microservices?

What is structured logging, and why is it preferred over plain-text logging in microservices?

**Answer:** Structured logging treats each log entry as a structured data record — typically JSON — where values are stored as named fields rather than embedded in a prose string. In a microservices system with dozens of services and millions of events per hour flowing into a central log store, structured logs are queryable by any field without full-text scanning, making incident investigation orders of magnitude faster.

- Plain-text logs like `"Order 42 failed for customer jane@example.com"` cannot be efficiently queried by `OrderId` or `CustomerId` — the values are invisible to the indexer.
- Structured logs record the same information as `{ "OrderId": 42, "CustomerId": "jane@example.com", "Event": "OrderFailed" }` — each field is individually indexed in Seq, Elastic, or Azure Monitor.
- In .NET, structured logging is built into `Microsoft.Extensions.Logging` via message templates: `_logger.LogError("Order {OrderId} failed", orderId)` — the provider decides whether to render the message as text or JSON.
- Serilog, NLog, and the OTel log bridge all support structured output; the sink (destination) determines the format — e.g., `WriteTo.Seq()` sends JSON, `WriteTo.Console()` can render human-readable text during development.

---

## Q18. What is Serilog, and how does it differ from the built-in `Microsoft.Extensions.Logging` framework?

What is Serilog, and how does it differ from the built-in `Microsoft.Extensions.Logging` framework?

**Answer:** Serilog is a third-party structured logging library for .NET that implements the `Microsoft.Extensions.Logging` abstraction but extends it with a rich sink ecosystem, log context enrichment, and fine-grained output formatting. The built-in MEL (Microsoft.Extensions.Logging) framework defines the `ILogger<T>` abstraction and a handful of default providers (Console, Debug, EventSource); Serilog replaces those providers with a pipeline that routes log events to any number of sinks with full structured data preserved.

- MEL is the abstraction layer — your code always calls `ILogger<T>`, so switching from Serilog to any other provider never requires touching business code.
- Serilog's sink library covers Seq, Elastic, Application Insights, Splunk, file rolling, Slack, and dozens more — configured declaratively in `appsettings.json` without code changes.
- Serilog's `LogContext.PushProperty(...)` and enrichers attach ambient properties (machine name, environment, request path) to every log event in a scope, which is valuable for correlating events across microservices.
- MEL's built-in Console provider emits plain text; Serilog's `WriteTo.Console(new RenderedCompactJsonFormatter())` emits compact JSON — important when log aggregators parse stdout from containers.

---

## Q19. What is a log enricher in Serilog, and why is it useful in microservices?

What is a log enricher in Serilog, and why is it useful in microservices?

**Answer:** A log enricher is a component that attaches additional properties to every log event that flows through the Serilog pipeline, without requiring the application code to explicitly include those properties in each log call. Enrichers are useful in microservices because they guarantee that every log event from a given service carries consistent metadata — service name, version, environment, host name, correlation ID — making cross-service filtering reliable even when developers forget to add them manually.

- `Enrich.WithProperty("ServiceName", "OrderService")` adds a static property to every event; log aggregation dashboards can then filter by service without relying on log message content.
- `Enrich.WithCorrelationId()` (from `Serilog.Enrichers.CorrelationId`) reads the HTTP `X-Correlation-ID` header and attaches it as a property automatically.
- `Enrich.WithMachineName()`, `Enrich.WithEnvironmentName()`, and `Enrich.FromLogContext()` (dynamic context pushed via `LogContext.PushProperty`) are common in container environments.
- Enrichers run before the event reaches any sink, so the additional properties are available for both filtering (`MinimumLevel.Override`) and output without any caller involvement.

---

## Q20. How do you correlate log entries across multiple microservices that collaborate on a single request?

How do you correlate log entries across multiple microservices that collaborate on a single request?

**Answer:** Correlation works by attaching a shared identifier — typically the W3C `traceId` from the `traceparent` header — to every log entry produced during a request, across every service that handles it. When OpenTelemetry tracing is active, the OTel log bridge automatically attaches the current `TraceId` and `SpanId` to every MEL log entry, so correlation comes for free as long as all services are instrumented.

- The receiving service reads `traceparent` from the incoming HTTP request; the OTel instrumentation populates `Activity.Current.TraceId` from it. Serilog's `Enrich.WithSpan()` or OTel's log bridge then stamps that `TraceId` on every log event within the request scope.
- Outbound calls via `HttpClient` (with OTel HttpClient instrumentation) write the same `traceparent` header to the downstream request, so the `TraceId` propagates automatically without manual work.
- In Seq or Elastic, you can filter by `TraceId` to see every log event across all services for a single request — the full narrative of what happened, in chronological order.
- If you are not using OTel, you must set a correlation ID manually in early middleware, push it to `LogContext`, and forward it in every outbound HTTP header — more work but the same result.

---

## Q21. What is a log aggregation system, and which ones are commonly used with .NET microservices?

What is a log aggregation system, and which ones are commonly used with .NET microservices?

**Answer:** A log aggregation system is a platform that collects, indexes, and stores log events from many service instances and makes them queryable through a single interface. Without aggregation, diagnosing a distributed failure means SSH-ing into individual pods and searching through files — impractical at scale. Aggregation centralizes all events, correlates them, and provides a query language for root-cause analysis.

| System | Hosting | Strengths |
|---|---|---|
| **Seq** | Self-hosted / cloud | Native .NET / Serilog integration, rich query UI |
| **ELK Stack** (Elastic + Logstash + Kibana) | Self-hosted | Industry standard, scales to very large volumes |
| **Azure Monitor Logs** | Managed (Azure) | Unified with Application Insights, KQL query language |
| **Datadog Logs** | Managed SaaS | Full APM + logs in one platform |
| **Grafana Loki** | Self-hosted | Label-based indexing, Prometheus-compatible ecosystem |

- Seq is particularly popular in .NET shops because Serilog ships a first-party `WriteTo.Seq()` sink and the query language understands Serilog message templates natively.
- Container environments typically emit logs to stdout/stderr; a log agent (Fluent Bit, Fluentd, Filebeat) running as a DaemonSet in Kubernetes ships those to the aggregation system without modifying the application.

---

## Q22. What is the difference between a log's trace ID and a correlation ID?

What is the difference between a log's trace ID and a correlation ID?

**Answer:** A trace ID is the globally unique identifier assigned by the distributed tracing system to an end-to-end request trace; it is defined by the W3C Trace Context standard, generated automatically by the OTel SDK, and propagated via the `traceparent` HTTP header. A correlation ID is a business-level identifier that you assign yourself — often the same value as the trace ID, but sometimes a domain concept like an `OrderId` or a user-assigned `RequestId` header — and you must propagate it manually if you want it to appear in downstream service logs.

- The trace ID is 128 bits, hexadecimal, opaque, and generated once per trace; it links traces to spans in your tracing backend.
- A correlation ID you set in middleware (`X-Correlation-ID` header) may be the same value, a user-supplied value, or a separate ID — it lives in the log record and in HTTP headers, not in the tracing backend.
- When OTel is active, the best practice is to *use the trace ID as the correlation ID* — write `Activity.Current?.TraceId` into the log context and the `X-Correlation-ID` response header, so logs and traces are linked by the same identifier.
- Legacy systems without OTel still use a custom correlation ID that they set manually; the approach works but requires manual header forwarding at every outbound call.

---

## Q23. What is `LoggerMessage.Define` / source-generated logging, and when should you prefer it over `_logger.LogInformation(...)`?

What is `LoggerMessage.Define` / source-generated logging, and when should you prefer it over `_logger.LogInformation(...)`?

**Answer:** `LoggerMessage.Define` (and the newer `[LoggerMessage]` source generator in .NET 6+) pre-compiles logging delegates that avoid boxing value types and allocating `object[]` arrays on every call, making them significantly faster and allocation-free compared to the standard `_logger.LogInformation(...)` overloads. You should prefer them on high-frequency code paths — tight loops, per-request hot paths, or any logger call that executes thousands of times per second.

```csharp
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information,
                   Message = "Order {OrderId} created for {CustomerId}")]
    public static partial void OrderCreated(
        this ILogger logger, Guid orderId, string customerId);
}

// Call site:
Log.OrderCreated(_logger, order.Id, order.CustomerId);
```

- The source generator emits a cached `LogDefineOptions`-backed delegate; the log level check is inlined, so there is zero allocation cost when the level is below the configured minimum.
- Standard `_logger.LogInformation("...", arg1, arg2)` boxes value types (`int`, `Guid`) into `object` and allocates an `object[]` on every call, even when the log is filtered out by level.
- For rare events (startup, exception paths, one-per-request logs), the standard overloads are fine — the savings only matter at high frequency.
- `[LoggerMessage]` is the modern form (introduced in .NET 6); `LoggerMessage.Define<T1, T2>` is the .NET 5 / manual equivalent.

---

## Chapter 4. Metrics

---

## Q24. What are metrics, and how do they differ from traces and logs in an observability stack?

What are metrics, and how do they differ from traces and logs in an observability stack?

**Answer:** Metrics are numeric measurements collected at regular intervals and aggregated over time — things like request count, error rate, latency percentiles, and memory usage. Unlike traces (which capture one request's full path) or logs (which record individual events), metrics are pre-aggregated: you lose individual detail in exchange for cheap, scalable storage and fast queries at any time range. Metrics are the right signal for alerting and capacity planning; traces and logs are the right tools for diagnosing the specific requests that broke.

- A metric like `http_requests_total{method="POST",status="500"}` tells you how many 500s occurred per minute, but not which requests they were or why they failed.
- Traces tell you which request failed and exactly where in the call graph, but storing a span for every request at high traffic is expensive.
- The ideal workflow: a metric alert fires (error rate > 1 %), you open your tracing backend and filter for traces with errors in that time window, then open the relevant log entries from the trace ID.
- Metrics are O(cardinality of label values) to store; traces are O(sampled request count) × O(depth of call graph); logs are O(event count). Metrics are always cheapest at scale.

---

## Q25. What is the .NET `Meter` API (`System.Diagnostics.Metrics`), and how does it relate to OpenTelemetry?

What is the .NET `Meter` API (`System.Diagnostics.Metrics`), and how does it relate to OpenTelemetry?

**Answer:** `System.Diagnostics.Metrics` is the .NET runtime's built-in metrics API, introduced in .NET 6. You create a `Meter` (analogous to `ActivitySource` for tracing), then create typed instruments from it — `Counter<T>`, `Histogram<T>`, `ObservableGauge<T>`, etc. OpenTelemetry's `AddMeter(...)` hooks into these instruments exactly as `AddSource(...)` hooks into `Activity` — the OTel SDK listens to the `Meter`'s instruments and exports measurements to your backend.

```csharp
private static readonly Meter _meter = new("OrderService.Payments");
private static readonly Counter<long> _chargeCount =
    _meter.CreateCounter<long>("payment.charges.total");

// In handler:
_chargeCount.Add(1, new TagList { { "currency", "USD" } });
```

- `Meter` is in the BCL (`System.Diagnostics.Metrics`), so .NET libraries (ASP.NET Core, HttpClient, EF Core) already emit metrics via `Meter` that OTel can pick up without any additional instrumentation.
- `AddMeter("OrderService.Payments")` in the OTel setup registers the listener; without this registration the Meter has zero overhead.
- The OTel SDK translates .NET `Meter` instruments to OTLP metric protocol and exports them to Prometheus, Azure Monitor, or any other compatible backend.

---

## Q26. What are the four metric instrument types in OpenTelemetry/Prometheus (Counter, Gauge, Histogram, Summary)?

What are the four metric instrument types in OpenTelemetry/Prometheus (Counter, Gauge, Histogram, Summary)?

**Answer:** OpenTelemetry defines Counter, UpDownCounter (Gauge in Prometheus), Histogram, and ObservableGauge as the core instrument types. Each fits a different measurement pattern, and choosing the wrong type leads to incorrect aggregation.

| Instrument | Direction | Use case | Example |
|---|---|---|---|
| `Counter<T>` | Monotonically increasing | Total counts | `http.requests.total` |
| `UpDownCounter<T>` (Gauge) | Up and down | Current levels | Active connections, queue depth |
| `Histogram<T>` | Distribution | Latency, payload size | `http.request.duration` (ms) |
| `ObservableGauge<T>` | Polled | Snapshot values | Memory usage, CPU % |

- Counters should never decrease — if you need to track something that can go down (active connections), use `UpDownCounter` (Prometheus Gauge).
- Histograms record individual observations into configurable buckets and allow accurate percentile calculation at query time (`histogram_quantile(0.99, ...)` in PromQL).
- Prometheus Summary computes percentiles client-side and cannot be reaggregated across replicas — avoid it in multi-instance deployments and prefer Histogram instead.

---

## Q27. What is Prometheus, and how does it scrape metrics from an ASP.NET Core service?

What is Prometheus, and how does it scrape metrics from an ASP.NET Core service?

**Answer:** Prometheus is an open-source, pull-based time-series monitoring system that periodically sends HTTP GET requests to `/metrics` endpoints on your services, parses the response (in Prometheus text format or OTLP), and stores the measurements in its time-series database. You configure scrape targets in Prometheus's `prometheus.yml` and it handles polling, storage, and alerting rules; Grafana visualizes the data.

- In ASP.NET Core, expose Prometheus metrics by adding `prometheus-net.AspNetCore` or the OTel Prometheus exporter. With OTel: `AddPrometheusExporter()` adds a `/metrics` endpoint automatically.
- Prometheus identifies each time series by its metric name plus a set of label key-value pairs (e.g., `http_requests_total{method="GET",status="200"}`). High-cardinality labels (e.g., `user_id`) must be avoided — they create millions of distinct time series.
- In Kubernetes, the Prometheus operator and `ServiceMonitor` CRDs automate target discovery — you annotate your `Service` and Prometheus finds the scrape endpoint without manual config file editing.
- Alerting rules in Prometheus (`PromQL` expressions) fire when thresholds are breached and integrate with Alertmanager for routing to PagerDuty, Slack, etc.

---

## Q28. How do you expose built-in .NET runtime and ASP.NET Core metrics via OpenTelemetry?

How do you expose built-in .NET runtime and ASP.NET Core metrics via OpenTelemetry?

**Answer:** In .NET 8+ the runtime emits meters for GC, threadpool, JIT, and HTTP server statistics; ASP.NET Core emits meters for request rate, duration, and active connections — all via `System.Diagnostics.Metrics`. You opt into them in the OTel setup by calling `AddMeter` with the meter names the runtime uses.

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()   // "Microsoft.AspNetCore.Hosting" meter
        .AddHttpClientInstrumentation()   // "System.Net.Http" meter
        .AddRuntimeInstrumentation()      // GC, threadpool (OpenTelemetry.Instrumentation.Runtime)
        .AddPrometheusExporter());        // expose /metrics endpoint
```

- `AddAspNetCoreInstrumentation()` subscribes to `Microsoft.AspNetCore.Hosting`, `Microsoft.AspNetCore.Http.Connections`, and related meters — you get request duration histograms and active request counts for free.
- `AddRuntimeInstrumentation()` (NuGet: `OpenTelemetry.Instrumentation.Runtime`) exposes GC heap sizes, GC pause durations, threadpool queue length, and exception counts.
- Custom meters in your own code are included by calling `AddMeter("OrderService.Payments")` alongside the framework meters.

---

## Q29. What is Grafana, and how does it relate to Prometheus in a typical metrics stack?

What is Grafana, and how does it relate to Prometheus in a typical metrics stack?

**Answer:** Grafana is a visualization and dashboarding platform that connects to time-series databases — including Prometheus — and renders charts, gauges, heatmaps, and alerts. Prometheus handles data collection and storage; Grafana handles presentation. They are separate tools that are almost always deployed together because Prometheus's built-in expression browser is minimal and not suitable for operational dashboards.

- Grafana connects to Prometheus as a data source and issues PromQL queries against it to power dashboard panels.
- Grafana also supports native alerting, which can fire on the same PromQL expressions and route to notification channels (PagerDuty, Slack, email) — an alternative to Prometheus Alertmanager for simpler setups.
- Grafana Cloud offers a managed stack (Prometheus + Loki + Tempo + Grafana) that covers all three observability pillars without self-hosting infrastructure.
- Pre-built dashboards for .NET / ASP.NET Core (community-contributed `grafana.com/grafana/dashboards`) can be imported and used with your Prometheus data source immediately, giving request rate, latency, and GC panels without writing PromQL manually.

---

## Q30. What is the difference between pull-based and push-based metrics collection, and which model does Prometheus use?

What is the difference between pull-based and push-based metrics collection, and which model does Prometheus use?

**Answer:** In a pull-based model, the metrics collector initiates the connection and fetches data from the service's `/metrics` endpoint on its configured interval. In a push-based model, the service sends metrics to a central collector or gateway whenever measurements are ready. Prometheus uses the pull model: it maintains a list of scrape targets and periodically calls each one, which makes Prometheus itself the single source of truth for scrape health and timing.

| Aspect | Pull (Prometheus) | Push (StatsD, InfluxDB line protocol, OTLP push) |
|---|---|---|
| Who initiates | Collector scrapes the service | Service sends to collector |
| Firewall / NAT | Collector needs network access to service | Service needs access to collector |
| Scale | Collector must manage all targets | Each service instance pushes independently |
| Short-lived jobs | Requires Pushgateway for batch jobs | Natural fit |

- Pull is simpler for long-running services in Kubernetes where targets are stable and the Prometheus operator handles discovery.
- Push works better for short-lived jobs (batch processing, Lambda-style functions) that may exit before Prometheus scrapes them — they push to a Pushgateway or OTLP endpoint instead.
- The OTel SDK supports both: `AddPrometheusExporter()` creates a pull endpoint; `AddOtlpExporter()` pushes to a Collector or backend.

---

## Chapter 5. Health Checks

---

## Q31. What are health checks in ASP.NET Core, and why are they important in a microservices deployment?

What are health checks in ASP.NET Core, and why are they important in a microservices deployment?

**Answer:** Health checks are lightweight HTTP endpoints that report whether a service instance is capable of handling requests. In a microservices deployment, health checks are the mechanism by which orchestrators (Kubernetes), load balancers, and service meshes decide whether to route traffic to an instance — without health checks, traffic continues to reach an instance that has lost its database connection or run out of memory, silently failing users.

- ASP.NET Core's `Microsoft.Extensions.Diagnostics.HealthChecks` package provides `IHealthCheck`, `AddHealthChecks()`, and `MapHealthChecks()` as a first-class framework feature since ASP.NET Core 2.2.
- Health checks return one of three statuses: `Healthy`, `Degraded`, or `Unhealthy`. The HTTP response is 200 for `Healthy`/`Degraded` and 503 for `Unhealthy` by default (configurable).
- Beyond the built-in framework, `AspNetCore.HealthChecks.*` NuGet packages provide pre-built checks for SQL Server, Redis, RabbitMQ, MongoDB, and other common dependencies.
- In Kubernetes, health check endpoints serve as the `livenessProbe` and `readinessProbe` targets in the pod spec.

---

## Q32. What is the difference between a liveness probe and a readiness probe in Kubernetes?

What is the difference between a liveness probe and a readiness probe in Kubernetes?

**Answer:** A liveness probe tells Kubernetes whether the container process is still alive and should keep running — if it fails, Kubernetes restarts the container. A readiness probe tells Kubernetes whether the container is ready to receive traffic — if it fails, Kubernetes removes the pod from the load balancer endpoints but does not restart it. The distinction matters when a service is healthy but temporarily unavailable, such as during a warm-up phase or while a dependency is recovering.

- Liveness failure → Kubernetes kills and restarts the container, potentially losing in-flight requests.
- Readiness failure → Kubernetes drains traffic away from the pod; the container keeps running and Kubernetes tries again on the next probe interval.
- A common pattern: liveness checks only that the process is responsive (can it return 200 at all?); readiness checks actual dependencies (can it reach the database?). This prevents the liveness probe from restarting a pod that is merely waiting for a database to recover.
- ASP.NET Core supports this with `MapHealthChecks("/health/live", ...)` and `MapHealthChecks("/health/ready", ...)` targeting different tag-filtered subsets of registered checks.

---

## Q33. How do you add and expose a health check endpoint in ASP.NET Core?

How do you add and expose a health check endpoint in ASP.NET Core?

**Answer:** You register health checks in `Program.cs` with `builder.Services.AddHealthChecks()`, optionally chaining dependency checks, and expose them via `app.MapHealthChecks(path)` in the middleware pipeline.

```csharp
// Registration
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, tags: new[] { "ready" })
    .AddRedis(redisConnectionString, tags: new[] { "ready" });

// Exposure
app.MapHealthChecks("/health/live");   // liveness — no dependency checks
app.MapHealthChecks("/health/ready",  // readiness — database + Redis
    new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
```

- The `/health/live` endpoint runs all checks that have no `ready` tag — typically just a check that the process is responsive, returning `Healthy` immediately.
- The `/health/ready` endpoint filters to checks tagged `"ready"` and returns `503 Unhealthy` if any dependency is unreachable.
- `HealthCheckOptions.ResponseWriter` accepts a delegate to customize the JSON output (e.g., including check names and durations); `UIResponseWriter.WriteHealthCheckUIResponse` from `AspNetCore.HealthChecks.UI.Client` is a common choice.
- Health check endpoints should be excluded from authentication middleware (`AllowAnonymous` or `RequireHost` on a different port) so that Kubernetes can call them without credentials.

---

## Q34. How do you write a custom `IHealthCheck` for an external dependency like a database?

How do you write a custom `IHealthCheck` for an external dependency like a database?

**Answer:** You implement `IHealthCheck` and inject your dependencies normally. The `CheckHealthAsync` method performs the actual probe and returns a `HealthCheckResult` with a status and optional description. Keep checks fast (under a few hundred milliseconds) and non-destructive — a health check should not modify data.

```csharp
public class InventoryApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _client;

    public InventoryApiHealthCheck(IHttpClientFactory factory)
        => _client = factory.CreateClient("InventoryApi");

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetAsync("/health/live", ct);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Non-200 from InventoryApi");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("InventoryApi unreachable", ex);
        }
    }
}
```

- Register it with `builder.Services.AddHealthChecks().AddCheck<InventoryApiHealthCheck>("inventory", tags: new[] { "ready" })`.
- The `HealthCheckContext` contains the `Registration` (name, failure status, tags) if you need to inspect them inside the check.
- For database checks, prefer a cheap query like `SELECT 1` rather than a full business query — you want to verify connectivity, not functional correctness.
- Always pass a `CancellationToken` to async calls inside the check; Kubernetes probes have a configured timeout and the framework cancels the token when that timeout is reached.

---

## Q35. What is a startup probe, and how does it differ from liveness and readiness probes?

What is a startup probe, and how does it differ from liveness and readiness probes?

**Answer:** A startup probe tells Kubernetes that the container is still initializing and that liveness and readiness probes should not yet be evaluated. If the startup probe fails within the configured `failureThreshold × periodSeconds` window, Kubernetes kills and restarts the container. Once the startup probe succeeds, Kubernetes hands off to the normal liveness and readiness probes. Startup probes exist to give slow-starting applications (loading large ML models, running EF Core migrations on startup) a grace period without setting aggressive liveness thresholds that would restart healthy pods.

- Without a startup probe, you must set `initialDelaySeconds` on the liveness probe large enough to cover the worst-case startup time — often too long to detect a genuine crash quickly after startup completes.
- With a startup probe, `initialDelaySeconds` on liveness can be set to 0; the startup probe takes over for the early phase and the liveness probe only fires once the app has signaled it is ready.
- In ASP.NET Core you would typically use the same `/health/live` endpoint for the startup probe, but with a larger `failureThreshold` in the Kubernetes pod spec.
- `IHostApplicationLifetime.ApplicationStarted` fires once the .NET host has finished startup — you could wire a custom health check to return `Unhealthy` until that event fires, then switch to `Healthy`.

---

## Q36. What HTTP status codes do ASP.NET Core health check endpoints return, and what do they mean?

What HTTP status codes do ASP.NET Core health check endpoints return, and what do they mean?

**Answer:** By default, ASP.NET Core maps `Healthy` and `Degraded` to HTTP 200 OK, and `Unhealthy` to HTTP 503 Service Unavailable. Kubernetes interprets any 2xx response as a passing probe and any non-2xx as a failing probe, so the 503 for `Unhealthy` correctly triggers the orchestrator's failure handling. `Degraded` returning 200 means the service stays in rotation even when a non-critical dependency is slow — useful for graceful degradation scenarios.

- The status-to-code mapping is controlled by `HealthCheckOptions.ResultStatusCodes`, a dictionary you can override if 200/503 does not fit your infrastructure.
- Some teams map `Degraded` to 200 for the readiness probe (stay in rotation, serve degraded) and 503 for the liveness probe (restart the pod if degraded for too long). This requires two separate `MapHealthChecks` calls with different `ResultStatusCodes` configs.
- The response body is `text/plain` with the status name (`Healthy`, `Unhealthy`, etc.) by default; you can replace this with JSON using a custom `ResponseWriter`.
- `Degraded` is the appropriate status when the service is functional but operating below normal capacity — e.g., the cache is down so responses are slower but still correct.

---
