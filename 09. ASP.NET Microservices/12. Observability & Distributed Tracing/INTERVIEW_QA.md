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

**Concepts**
- Observability — inferring internal state from external outputs without deploying new code
- Monitoring — checking predefined thresholds, telling you that something is wrong
- Exploratory investigation as the key capability observability adds
- Control theory origin of the term

**Answer**

Observability is the ability to understand what is happening inside a system purely from its external outputs — logs, metrics, and traces — without needing to deploy new code to ask new questions. Monitoring, by contrast, means checking whether predefined thresholds are breached: it tells you that something is wrong, not why. An observable system lets you explore arbitrary questions about its internal state after the fact, which is essential in microservices where failures often span many independent services. Monitoring is reactive and alert-driven: you decide in advance what to measure and receive alerts when a value crosses a boundary. Observability is exploratory: when an incident happens, you can query any dimension of the data — even ones you never anticipated — to find the root cause. Observability becomes critical in microservices because a request failure in Service A may be caused by latency in Service C two hops away, and no single threshold alert covers that chain.

---

## Q2. What are the three pillars of observability, and what does each one measure?

**Concepts**
- Logs — discrete timestamped events with rich context
- Metrics — numeric measurements aggregated over time for alerting and trends
- Traces — end-to-end call chains across service boundaries
- OpenTelemetry 1.0 unifying all three under a single SDK and wire format

**Answer**

The three pillars are logs, metrics, and traces. Each captures a different dimension of system behavior, and together they give a complete picture: metrics show you what is degraded, traces show you where in the call chain it broke, and logs show you why a specific operation failed. Logs answer point-in-time questions — "what happened during order 42's checkout?" — by recording discrete timestamped events with full context. Metrics answer trend questions — "has p99 latency on the payment service been rising for the last hour?" — by aggregating numeric measurements over time at low storage cost. Traces answer path questions — "which service in the chain is responsible for this 3-second response?" — by stitching together a causally linked record of every operation across service boundaries. OpenTelemetry 1.0 unifies all three under a single SDK and wire format, so correlation between pillars is first-class rather than bolted on.

---

## Q3. Why is observability harder in a microservices architecture than in a monolith?

**Concepts**
- Distributed log streams requiring correlation IDs and aggregation for cross-service tracing
- Network-introduced failure modes invisible without distributed tracing
- Polyglot teams and languages making consistent instrumentation harder to enforce
- Cascading failures spanning services that no individual service log can reveal

**Answer**

In a monolith, a single process produces a single stream of logs and a stack trace fully describes a failure. In microservices, a single user request may touch a dozen services across different hosts, networks, and runtimes, so no individual service can see the full picture. Each service has its own log stream; without correlation IDs and a log aggregation system, tracing a failure means manually searching across multiple tools. Network calls between services introduce new failure modes — timeouts, partial failures, retries — that do not exist in-process, and each hop adds latency that is invisible without distributed tracing. Services may be owned by different teams and written in different languages, making a consistent instrumentation strategy harder to enforce. Cascading failures are common: Service A is slow not because of its own code but because Service B is slow, which is waiting on a degraded database — only a distributed trace reveals this chain, since each service in isolation looks like it is just waiting.

---

## Q4. What is the difference between a log, a metric, and a trace?

**Concepts**
- Log — high-cardinality discrete event record, rich in context
- Metric — low-cardinality aggregated number, cheap at scale
- Trace — sampled causally linked execution path across service boundaries
- Optimal workflow: metric alert fires, trace identifies where, log explains why

**Answer**

A log is a discrete timestamped record of a specific event, rich in context but produced per occurrence so it is expensive to store and query at scale. A metric is a numeric measurement aggregated over time — designed for cheap, high-frequency collection where individual event detail is discarded in favor of aggregate trends; you know the 95th-percentile latency but not which specific request was slow. A trace is a causally linked set of operations across service boundaries showing the full execution path of a single request, typically sampled in production to manage cost while preserving the ability to investigate individual request paths. The three signals complement each other in investigation: a metric alert fires because error rate exceeded a threshold, you open the tracing backend and filter for traces with errors in that time window, then open the relevant log entries from the trace ID to see the exact exception message and stack trace.

---

## Q5. What is the difference between white-box monitoring and black-box monitoring?

**Concepts**
- White-box monitoring — instrumenting system internals for diagnostic detail
- Black-box monitoring — probing from outside to measure user-facing behavior
- White-box catching root causes early vs. black-box catching user-visible symptoms
- Both needed for complete coverage

**Answer**

White-box monitoring instruments the internals of a system — reading process metrics, custom counters, and application-level events that only the system itself can expose. Black-box monitoring probes the system from the outside — sending real or synthetic requests and observing the response — without access to internal state. White-box examples include CPU and memory gauges, database query duration histograms, queue depth counters, and `Activity`-based spans; black-box examples include synthetic health check pings and external uptime monitors checking an API endpoint every 30 seconds from outside the cluster. White-box gives rich diagnostic detail but can miss problems that only manifest at the network boundary or load balancer. Black-box is the ground truth for "is the service working from a user's perspective?" but gives little diagnostic information when it fails — you know the service returned an error but not why. Both are needed: white-box catches root causes early with fine-grained data, black-box catches the user-facing symptoms that actually matter to the business.

---

## Q6. What is a service map (or dependency graph), and how is it produced?

**Concepts**
- Service map — live visual graph of which services call which, with health and latency per edge
- Automatic production from span parent-child relationships in distributed trace data
- Near-real-time updates as traces arrive from instrumented services
- Azure Application Insights Application Map as a concrete example

**Answer**

A service map is a visual graph showing which services call which other services, along with live health and latency information on each edge. It is produced automatically by a distributed tracing backend — tools like Jaeger, Azure Application Insights, or AWS X-Ray parse the span data coming from instrumented services and reconstruct the call graph from parent-child span relationships. Each span in a distributed trace records the service it belongs to, its parent span ID, and the remote endpoint it called — enough information to build the graph edge by edge. A service map updates in near real-time as traces arrive, so it reflects the live topology of the system including dynamically discovered dependencies, which means new services appear on the map as soon as they start emitting traces. In Azure Application Insights, the Application Map view is generated this way, showing request rates, failure rates, and average latency per service edge without any manual configuration. Service maps are invaluable during incident response because they immediately show which service is the source of elevated error rates or latency in the dependency chain.

---

## Chapter 2. Distributed Tracing & OpenTelemetry

---

## Q7. What is a distributed trace, and what is a span?

**Concepts**
- Distributed trace — complete record of a single request stitched across multiple services
- Span — one node in the trace tree representing a single unit of work
- `traceId` (128-bit) as the globally unique trace identifier
- Span attributes carrying queryable context: HTTP method, URL, status, DB statement

**Answer**

A distributed trace is the complete record of a single request as it travels through multiple services — stitched together into a tree of operations that shows every hop, its duration, and any errors. A span is one node in that tree: it represents a single unit of work within one service, such as handling an HTTP request, executing a database query, or calling a downstream service, with a start time, duration, attributes, and a reference to its parent span. Every trace has a globally unique `traceId` of 128 bits; every span has its own 64-bit `spanId` and records its parent's `spanId`, which is how the tree is assembled by the backend. The root span is created when the first service receives the request, and every downstream call starts a child span that inherits the same `traceId`, so all spans from all services participating in one request share that identifier. Spans can be nested — an HTTP handler span may have a child database span, which may have a child connection-pool span — and attributes on a span carry context such as HTTP method, URL, status code, and database statement that are individually queryable in your tracing backend.

---

## Q8. What is OpenTelemetry (OTel), and what problem does it solve?

**Concepts**
- OpenTelemetry — vendor-neutral SDK for collecting logs, metrics, and traces from any app
- Separation of instrumentation API from exporter enabling backend-agnostic instrumentation
- CNCF graduated project merging OpenCensus and OpenTracing
- OTLP wire format and semantic conventions as the interoperability mechanism

**Answer**

OpenTelemetry is a vendor-neutral, open-source observability framework that provides a single API and SDK for collecting logs, metrics, and traces from any application, exporting them to any compatible backend. Before OTel, every observability vendor shipped its own SDK — switching from Jaeger to Datadog, for example, meant rewriting all instrumentation. OTel solves this by separating the instrumentation API — which your code calls — from the exporter — which decides where data goes — so you instrument once and change backends by reconfiguring an exporter. The .NET OTel SDK integrates directly with `System.Diagnostics.Activity` and `System.Diagnostics.Metrics`, both of which are built into the .NET runtime, so no third-party primitives are needed in library code. The OTel specification defines the OTLP wire format and semantic conventions — standard attribute names like `http.method` and `db.statement` — enabling consistent queries across services and languages. Popular backends including Jaeger, Zipkin, Azure Monitor, Datadog, and Honeycomb all accept OTLP, so the same instrumented app works with any of them.

---

## Q9. What is the W3C Trace Context standard, and what are the `traceparent` and `tracestate` headers?

**Concepts**
- W3C Trace Context — HTTP header standard for propagating distributed tracing context
- `traceparent` carrying version, traceId, parentId, and sample flags
- `tracestate` as a vendor-extension field for proprietary context alongside standard fields
- OTel `ActivityPropagator` reading and writing `traceparent` automatically

**Answer**

W3C Trace Context is an HTTP header standard that defines how distributed tracing context is propagated between services via HTTP headers, allowing services from different vendors, languages, and frameworks to participate in the same distributed trace without prior coordination. The `traceparent` header carries four required fields: version (`00`), `traceId` (32 hex characters, 128-bit), `parentId` (16 hex characters, the current span's 64-bit ID), and flags (a bitmask where bit 0 indicates the request is sampled). An example looks like `00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01`. The `tracestate` header is a vendor-extension header — an ordered list of `key=value` pairs that each tracing system uses to carry its own proprietary context alongside the standard fields. ASP.NET Core with OpenTelemetry reads and writes `traceparent` automatically through the `ActivityPropagator`, so no manual header parsing is needed in application code. If an incoming request has no `traceparent`, the OTel SDK creates a new root span and a fresh `traceId`, starting a new trace.

---

## Q10. What is the `Activity` class in .NET, and how does it relate to OpenTelemetry spans?

**Concepts**
- `System.Diagnostics.Activity` as the .NET runtime's built-in span representation
- OTel SDK mapping `Activity` to OTel span model — start creates span, dispose exports it
- `ActivitySource` as the factory corresponding to an OTel Tracer
- Zero overhead when no OTel listener is attached

**Answer**

`System.Diagnostics.Activity` is the .NET runtime's built-in representation of a unit of work in a distributed trace — conceptually identical to an OpenTelemetry span. The OTel .NET SDK maps `Activity` to the OTel span model: when you start an `Activity`, the OTel SDK creates a corresponding span and exports it to the configured backend. This means .NET libraries such as HttpClient, SqlClient, and EF Core that already instrument themselves with `Activity` automatically produce OTel spans without any extra code. `ActivitySource` is the factory for creating `Activity` objects — it corresponds to an OTel Tracer — and libraries expose a named `ActivitySource` that you subscribe to in the OTel SDK with `AddSource(...)`. `Activity.Current` is the ambient current span, flowing across `await` boundaries via `AsyncLocal<T>`, so child spans created inside an async method automatically link to the correct parent. Because `Activity` is in the .NET runtime rather than a library, it has zero overhead when no listener is attached — the OTel SDK acts as the listener and takes the overhead only when enabled.

---

## Q11. How do you add OpenTelemetry tracing to an ASP.NET Core microservice with outbound `HttpClient` calls?

**Concepts**
- `AddOpenTelemetry().WithTracing()` as the setup entry point
- `AddAspNetCoreInstrumentation()` creating a root span per incoming request
- `AddHttpClientInstrumentation()` creating child spans and writing `traceparent` on outbound calls
- Resource builder setting service name on every span for backend filtering

**Answer**

You configure OpenTelemetry in `Program.cs` by calling `AddOpenTelemetry()` on the service collection, then chaining `WithTracing(...)` to register instrumentation sources and an exporter. The ASP.NET Core and HttpClient instrumentation libraries hook into the existing `Activity`-based diagnostics that .NET already emits, so no changes to controller or service code are required.

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

`AddAspNetCoreInstrumentation` creates a root span for each incoming request, reading `traceparent` from the request header to link it to an existing trace if one is present. `AddHttpClientInstrumentation` adds a `DelegatingHandler` to every `HttpClient` managed by `IHttpClientFactory`; it creates a child span for each outbound call and writes `traceparent` into the request headers automatically, so context propagates to the next service without any manual code. The resource builder sets identifying attributes — service name and version — that appear on every span, which is essential for filtering by service in your tracing backend. `AddOtlpExporter` sends spans in the OTLP format defaulting to gRPC on `localhost:4317`; configure the endpoint via the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable for container environments.

---

## Q12. What is the OpenTelemetry Collector, and why would you use one instead of exporting directly from the app?

**Concepts**
- OTel Collector as a standalone agent receiving, processing, and forwarding telemetry
- Decoupling application code from backend configuration
- Fan-out to multiple backends simultaneously via pipelines configuration
- Processing — PII scrubbing, tail-based sampling, infrastructure metadata enrichment

**Answer**

The OpenTelemetry Collector is a standalone agent or gateway process that receives telemetry from applications, processes it, and forwards it to one or more backends. Rather than each microservice maintaining direct connections to Jaeger, Prometheus, and a logging backend simultaneously, services send OTLP data to the Collector, which fans it out. This decouples your application code from backend configuration: if you switch from Jaeger to Honeycomb, you update the Collector config only — no application redeployment is needed. The Collector can forward the same trace data to multiple backends simultaneously using its pipelines configuration, which is useful when developers need Jaeger for local investigation and operations teams need Azure Monitor for alerting. The processing layer allows redacting sensitive span attributes for PII scrubbing, performing tail-based sampling, adding infrastructure metadata, and batching data to reduce network overhead — concerns that are inappropriate to put in application code. In Kubernetes the Collector can be deployed as a sidecar container per pod for low latency, or as a shared cluster-level gateway for centralized control.

---

## Q13. What is sampling in distributed tracing, and what is the difference between head-based and tail-based sampling?

**Concepts**
- Sampling — recording only a fraction of traces to control storage cost
- Head-based sampling — keep/drop decision at trace start, propagated in `traceparent` flags
- Tail-based sampling — decision after trace completion, requiring stateful buffer
- Combined strategy: low-percentage head sampling plus 100% for errors and slow traces

**Answer**

Sampling is the practice of recording only a fraction of traces rather than every request, to control storage costs and processing overhead. At high traffic volumes, recording 100% of traces is impractical — a service handling ten thousand requests per second would generate millions of spans per minute. Head-based sampling makes the keep/drop decision at the start of a trace before any spans have been collected; the decision propagates in `traceparent` flags so every service in the chain samples or drops together. It is simple and low-overhead but cannot keep "interesting" traces since an error that occurs late in the chain may be dropped because the head decision was already made. Tail-based sampling collects all spans for a trace and makes the keep/drop decision after the trace is complete — once you know whether it was slow or contained an error. This requires a stateful component such as the OTel Collector's tail sampling processor that buffers spans in memory until the trace is done. A common production strategy combines both: one to five percent head-based sampling for routine traffic, plus 100% sampling for all traces containing errors or exceeding a latency threshold using tail-based sampling.

---

## Q14. What are the common distributed tracing backends compatible with OpenTelemetry?

**Concepts**
- OTLP as the universal wire format accepted by all major backends
- Jaeger and Zipkin as self-hosted open-source options
- Azure Monitor as the managed choice for Azure-hosted services
- Datadog and Honeycomb as full-stack managed SaaS options

**Answer**

OpenTelemetry's OTLP wire format is accepted by all major tracing backends, so the choice of backend is a deployment and cost decision rather than an instrumentation one. The most common options in .NET microservices deployments are Jaeger — self-hosted, open source, Kubernetes-friendly, native OTel support; Zipkin — self-hosted, open source, simple setup with low resource use; Azure Monitor and Application Insights — managed on Azure with native .NET SDK integration and the Application Map view; Datadog — managed SaaS with full-stack APM; and Honeycomb — managed SaaS specializing in tail-based sampling and rich querying. All accept OTLP via the `AddOtlpExporter()` call; some such as Zipkin and Datadog have their own exporter NuGet packages for non-OTLP protocols. In Azure environments, Application Insights with `Azure.Monitor.OpenTelemetry.AspNetCore` is the default choice and requires no separate infrastructure.

---

## Q15. What is the difference between a trace exporter and a trace processor in the OTel SDK?

**Concepts**
- Trace processor — in-process pipeline stage between span creation and export
- Trace exporter — out-of-process boundary serializing and sending spans to a backend
- `BatchSpanProcessor` for production vs. `SimpleSpanProcessor` for development
- Custom `BaseProcessor<Activity>` for PII redaction before spans leave the process

**Answer**

A trace processor sits in the SDK pipeline between span creation and export — it receives every finished span and can batch, filter, enrich, or transform it before passing it along. A trace exporter is the final sink that serializes finished spans and sends them over the wire to a backend; processors run in-process while exporters are the out-of-process boundary. The SDK ships with `BatchSpanProcessor` — the default, recommended for production because it buffers spans and exports them in batches to reduce network round trips — and `SimpleSpanProcessor`, which exports each span immediately and is useful only in development since it introduces a synchronous network call on every span. The `CompositeExporter` lets you chain multiple exporters to send to both Jaeger and a console exporter for local debugging without needing multiple processors. A custom `BaseProcessor<Activity>` is the correct place to redact PII from span attributes before they leave the process — it runs before the exporter, so sensitive data never reaches the backend. The full pipeline is: `ActivitySource.StartActivity()` → SDK records span → `Processor.OnEnd(span)` → `Exporter.Export(batch)` → backend.

---

## Q16. How do you create a custom span inside an existing trace using `ActivitySource`?

**Concepts**
- Static `ActivitySource` declared per library or component
- `AddSource()` registration required in OTel setup for the SDK to listen
- Null-conditional `span?.SetTag()` since `StartActivity` returns null when unregistered
- `using` block disposal triggering `OnEnd` and triggering export

**Answer**

You declare a static `ActivitySource` in your class — one per logical library or component — then call `StartActivity` at the point where you want to begin instrumenting a unit of work. The SDK automatically links the new span to `Activity.Current` as its parent, so it joins the existing trace with no manual ID wiring.

```csharp
private static readonly ActivitySource _source = new("OrderService.Payments");

public async Task ChargeAsync(Guid orderId)
{
    using var span = _source.StartActivity("ChargeCard");
    span?.SetTag("order.id", orderId.ToString());
    // ... work ...
}
```

The `ActivitySource` name must be registered in `AddSource("OrderService.Payments")` during OTel setup; without registration the SDK ignores the source and `StartActivity` returns `null` — which is why the null-conditional `span?.SetTag(...)` is important. `StartActivity` sets `Activity.Current` to the new span for the duration of the `using` block, so child spans started inside will automatically link to it. Dispose the `Activity` via `using` when the work is done — the SDK calls `OnEnd` at that point and the span is processed and exported. You can add events via `span.AddEvent("RetryAttempted")`, set status via `span.SetStatus(ActivityStatusCode.Error, "Card declined")`, and record exceptions via `span.RecordException(ex)`.

---

## Chapter 3. Structured Logging in Microservices

---

## Q17. What is structured logging, and why is it preferred over plain-text logging in microservices?

**Concepts**
- Structured logging — log entries as named-field data records rather than prose strings
- Individual field indexing enabling O(1) queries vs. full-text scan
- MEL message template syntax preserving structure at the call site
- Sink deciding whether to render as JSON for aggregators or text for development

**Answer**

Structured logging treats each log entry as a structured data record — typically JSON — where values are stored as named fields rather than embedded in a prose string. In a microservices system with dozens of services and millions of events per hour flowing into a central log store, structured logs are queryable by any field without full-text scanning, making incident investigation orders of magnitude faster. A plain-text log like `"Order 42 failed for customer jane@example.com"` cannot be efficiently queried by `OrderId` or `CustomerId` — the values are invisible to the indexer. The same information as a structured record `{ "OrderId": 42, "CustomerId": "jane@example.com", "Event": "OrderFailed" }` has each field individually indexed in Seq, Elastic, or Azure Monitor. In .NET, structured logging is built into `Microsoft.Extensions.Logging` via message templates: `_logger.LogError("Order {OrderId} failed", orderId)` — the provider decides whether to render the message as text or JSON, so the call site is identical regardless of destination. Serilog, NLog, and the OTel log bridge all support structured output; the sink determines the format.

---

## Q18. What is Serilog, and how does it differ from the built-in `Microsoft.Extensions.Logging` framework?

**Concepts**
- Serilog implementing the MEL `ILogger<T>` abstraction with a richer sink ecosystem
- MEL as the abstraction layer — business code never changes when switching providers
- Serilog `LogContext.PushProperty()` attaching ambient properties to every event in scope
- Serilog console sink emitting compact JSON for container log aggregators

**Answer**

Serilog is a third-party structured logging library for .NET that implements the `Microsoft.Extensions.Logging` abstraction but extends it with a rich sink ecosystem, log context enrichment, and fine-grained output formatting. The built-in MEL framework defines the `ILogger<T>` abstraction and a handful of default providers — Console, Debug, EventSource — but Serilog replaces those providers with a pipeline that routes log events to any number of sinks with full structured data preserved. MEL is the abstraction layer — your code always calls `ILogger<T>`, so switching from Serilog to any other provider never requires touching business code. Serilog's sink library covers Seq, Elastic, Application Insights, Splunk, file rolling, Slack, and dozens more, all configured declaratively in `appsettings.json` without code changes. Serilog's `LogContext.PushProperty(...)` and enrichers attach ambient properties — machine name, environment, request path — to every log event in a scope, which is valuable for correlating events across microservices. The built-in MEL Console provider emits plain text, while Serilog's `WriteTo.Console(new RenderedCompactJsonFormatter())` emits compact JSON, which is important when log aggregators parse stdout from containers.

---

## Q19. What is a log enricher in Serilog, and why is it useful in microservices?

**Concepts**
- Log enricher — attaches additional properties to every event without explicit call-site code
- Consistent metadata guaranteed across all events regardless of developer diligence
- `Enrich.WithCorrelationId()` reading the HTTP correlation header automatically
- Enrichers running before sinks, available for both filtering and output

**Answer**

A log enricher is a component that attaches additional properties to every log event that flows through the Serilog pipeline, without requiring application code to explicitly include those properties in each log call. Enrichers are useful in microservices because they guarantee that every log event from a given service carries consistent metadata — service name, version, environment, host name, correlation ID — making cross-service filtering reliable even when developers forget to add them manually. `Enrich.WithProperty("ServiceName", "OrderService")` adds a static property to every event, so log aggregation dashboards can filter by service without relying on log message content. `Enrich.WithCorrelationId()` from `Serilog.Enrichers.CorrelationId` reads the HTTP `X-Correlation-ID` header and attaches it as a property automatically. `Enrich.WithMachineName()`, `Enrich.WithEnvironmentName()`, and `Enrich.FromLogContext()` for dynamic context pushed via `LogContext.PushProperty` are common in container environments. Enrichers run before the event reaches any sink, so the additional properties are available for both minimum-level filtering and output.

---

## Q20. How do you correlate log entries across multiple microservices that collaborate on a single request?

**Concepts**
- W3C `traceId` as the shared identifier stamped on every log entry via OTel log bridge
- OTel HttpClient instrumentation writing `traceparent` into outbound request headers
- Querying by `TraceId` in Seq or Elastic revealing all events for one request
- Manual correlation ID with `LogContext` and explicit header forwarding as the non-OTel fallback

**Answer**

Correlation works by attaching a shared identifier — typically the W3C `traceId` from the `traceparent` header — to every log entry produced during a request, across every service that handles it. When OpenTelemetry tracing is active, the OTel log bridge automatically attaches the current `TraceId` and `SpanId` to every MEL log entry, so correlation comes for free as long as all services are instrumented. The receiving service reads `traceparent` from the incoming HTTP request; the OTel instrumentation populates `Activity.Current.TraceId` from it, and Serilog's `Enrich.WithSpan()` or the OTel log bridge stamps that `TraceId` on every log event within the request scope. Outbound calls via `HttpClient` with OTel instrumentation write the same `traceparent` header to the downstream request, so the `TraceId` propagates automatically without manual work. In Seq or Elastic, filtering by `TraceId` shows every log event across all services for a single request — the full narrative of what happened, in chronological order. If OTel is not in use, the alternative is to set a correlation ID manually in early middleware, push it to `LogContext`, and forward it in every outbound HTTP header.

---

## Q21. What is a log aggregation system, and which ones are commonly used with .NET microservices?

**Concepts**
- Log aggregation — centralizing, indexing, and querying log events from many service instances
- Seq as the .NET-native choice with first-party Serilog integration
- ELK stack as the industry standard for large volumes
- Container log agents (Fluent Bit, Fluentd) shipping stdout to aggregators without app changes

**Answer**

A log aggregation system is a platform that collects, indexes, and stores log events from many service instances and makes them queryable through a single interface. Without aggregation, diagnosing a distributed failure means SSH-ing into individual pods and searching through files, which is impractical at scale. The most common options in .NET microservices deployments are: Seq — self-hosted or cloud-hosted, with native .NET and Serilog integration and a rich query UI that understands Serilog message templates natively; the ELK stack (Elasticsearch, Logstash, Kibana) — the industry standard that scales to very large volumes; Azure Monitor Logs — managed on Azure with unified Application Insights integration and the KQL query language; Datadog Logs — full APM and logs in one platform; and Grafana Loki — label-based indexing that integrates naturally with the Prometheus ecosystem. In container environments, services typically emit logs to stdout and stderr, and a log agent such as Fluent Bit or Fluentd running as a DaemonSet in Kubernetes ships those to the aggregation system without modifying the application.

---

## Q22. What is the difference between a log's trace ID and a correlation ID?

**Concepts**
- Trace ID — 128-bit OTel-assigned identifier propagated via `traceparent`, linking to spans
- Correlation ID — business-level or custom identifier propagated manually in headers
- Best practice: using the trace ID as the correlation ID when OTel is active
- Legacy systems using a custom correlation ID with explicit header forwarding

**Answer**

A trace ID is the globally unique identifier assigned by the distributed tracing system to an end-to-end request trace; it is defined by the W3C Trace Context standard, generated automatically by the OTel SDK, and propagated via the `traceparent` HTTP header. A correlation ID is a business-level identifier that you assign yourself — often the same value as the trace ID, but sometimes a domain concept like an `OrderId` or a user-assigned `RequestId` header — and you must propagate it manually if you want it to appear in downstream service logs. The trace ID is 128 bits, hexadecimal, opaque, and generated once per trace; it links traces to spans in your tracing backend. A correlation ID you set in middleware may be the same value as the trace ID, a user-supplied value, or a separate ID — it lives in the log record and in HTTP headers, not in the tracing backend. When OTel is active, the best practice is to use the trace ID as the correlation ID — write `Activity.Current?.TraceId` into the log context and the `X-Correlation-ID` response header, so logs and traces are linked by the same identifier. Legacy systems without OTel use a custom correlation ID that they set manually, requiring explicit header forwarding at every outbound call.

---

## Q23. What is `LoggerMessage.Define` / source-generated logging, and when should you prefer it over `_logger.LogInformation(...)`?

**Concepts**
- `[LoggerMessage]` source generator pre-compiling allocation-free logging delegates
- Boxing value types into `object[]` on every standard `LogInformation` call
- Zero allocation cost when log level is below the configured minimum
- High-frequency hot paths as the target — rare events do not justify the change

**Answer**

`LoggerMessage.Define` and the newer `[LoggerMessage]` source generator in .NET 6+ pre-compile logging delegates that avoid boxing value types and allocating `object[]` arrays on every call, making them significantly faster and allocation-free compared to the standard `_logger.LogInformation(...)` overloads. Standard `_logger.LogInformation("...", arg1, arg2)` boxes value types such as `int` and `Guid` into `object` and allocates an `object[]` on every call, even when the log is filtered out by level — that allocation happens before the level check. The source generator emits a cached delegate where the log level check is inlined, so there is zero allocation cost when the level is below the configured minimum. I would prefer source-generated logging on high-frequency code paths — tight loops, per-request hot paths, or any logger call that executes thousands of times per second. For rare events such as startup, exception paths, or one-per-request logs, the standard overloads are fine — the savings only matter at high frequency.

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

---

## Chapter 4. Metrics

---

## Q24. What are metrics, and how do they differ from traces and logs in an observability stack?

**Concepts**
- Metrics — pre-aggregated numeric measurements, cheap to store, fast to query
- Traces — sampled individual request paths, medium cost
- Logs — per-event records, expensive at scale
- Optimal investigation workflow: metric alert → trace → log

**Answer**

Metrics are numeric measurements collected at regular intervals and aggregated over time — things like request count, error rate, latency percentiles, and memory usage. Unlike traces, which capture one request's full path, or logs, which record individual events, metrics are pre-aggregated: you lose individual detail in exchange for cheap, scalable storage and fast queries at any time range. A metric like `http_requests_total{method="POST",status="500"}` tells you how many 500s occurred per minute, but not which requests they were or why they failed. Traces tell you which specific request failed and exactly where in the call graph, but storing a span for every request at high traffic is expensive. Logs capture the exact exception message and stack trace but are the most expensive to store and query at scale since they grow linearly with event count. The ideal investigation workflow combines all three: a metric alert fires when error rate exceeds a threshold, you open the tracing backend and filter for traces with errors in that time window, then open the relevant log entries from the trace ID to see why.

---

## Q25. What is the .NET `Meter` API (`System.Diagnostics.Metrics`), and how does it relate to OpenTelemetry?

**Concepts**
- `System.Diagnostics.Metrics` as the .NET BCL metrics API introduced in .NET 6
- `Meter` as the factory for instruments, analogous to `ActivitySource` for tracing
- `AddMeter()` in OTel setup registering the listener with zero overhead when absent
- OTel translating .NET `Meter` instruments to OTLP metric protocol

**Answer**

`System.Diagnostics.Metrics` is the .NET runtime's built-in metrics API introduced in .NET 6. You create a `Meter` — analogous to `ActivitySource` for tracing — then create typed instruments from it: `Counter<T>`, `Histogram<T>`, `ObservableGauge<T>`, and so on. OpenTelemetry's `AddMeter(...)` hooks into these instruments exactly as `AddSource(...)` hooks into `Activity` — the OTel SDK listens to the `Meter`'s instruments and exports measurements to your backend. `Meter` is in the BCL, so .NET libraries including ASP.NET Core, HttpClient, and EF Core already emit metrics via `Meter` that OTel can pick up without any additional instrumentation. `AddMeter("OrderService.Payments")` in the OTel setup registers the listener; without this registration the `Meter` has zero overhead. The OTel SDK translates .NET `Meter` instruments to the OTLP metric protocol and exports them to Prometheus, Azure Monitor, or any other compatible backend.

```csharp
private static readonly Meter _meter = new("OrderService.Payments");
private static readonly Counter<long> _chargeCount =
    _meter.CreateCounter<long>("payment.charges.total");

// In handler:
_chargeCount.Add(1, new TagList { { "currency", "USD" } });
```

---

## Q26. What are the four metric instrument types in OpenTelemetry/Prometheus (Counter, Gauge, Histogram, Summary)?

**Concepts**
- `Counter<T>` — monotonically increasing total count
- `UpDownCounter<T>` (Gauge) — current level that can go up and down
- `Histogram<T>` — distribution of values for percentile calculation at query time
- Prometheus Summary computing percentiles client-side — avoid in multi-instance deployments

**Answer**

OpenTelemetry defines Counter, UpDownCounter, Histogram, and ObservableGauge as the core instrument types, and choosing the wrong type leads to incorrect aggregation. A `Counter<T>` is monotonically increasing and appropriate for total counts such as `http.requests.total` — it should never decrease, so if you need to track something that can go down such as active connections, you need an `UpDownCounter` instead. An `UpDownCounter<T>` — called a Gauge in Prometheus — tracks current levels that can go both up and down, such as queue depth or active database connections. A `Histogram<T>` records individual observations into configurable buckets and allows accurate percentile calculation at query time via `histogram_quantile(0.99, ...)` in PromQL, making it the right choice for latency and payload size distributions. An `ObservableGauge<T>` is polled rather than pushed — you provide a callback that returns the current value — which suits snapshot measurements like memory usage or CPU percentage. Prometheus Summary computes percentiles client-side and cannot be reaggregated across replicas, so it should be avoided in multi-instance deployments in favor of Histogram.

---

## Q27. What is Prometheus, and how does it scrape metrics from an ASP.NET Core service?

**Concepts**
- Prometheus as a pull-based time-series monitoring system
- `/metrics` endpoint exposing measurements in Prometheus text format
- High-cardinality label values as a major anti-pattern to avoid
- Kubernetes Prometheus operator and `ServiceMonitor` CRDs for automated target discovery

**Answer**

Prometheus is an open-source, pull-based time-series monitoring system that periodically sends HTTP GET requests to `/metrics` endpoints on your services, parses the response in Prometheus text format or OTLP, and stores the measurements in its time-series database. In ASP.NET Core, you expose Prometheus metrics by adding `prometheus-net.AspNetCore` or the OTel Prometheus exporter — with OTel, `AddPrometheusExporter()` adds a `/metrics` endpoint automatically. Prometheus identifies each time series by its metric name plus a set of label key-value pairs such as `http_requests_total{method="GET",status="200"}`. High-cardinality labels such as `user_id` or `request_id` must be avoided — they create millions of distinct time series and can cause Prometheus to run out of memory and crash. In Kubernetes, the Prometheus operator and `ServiceMonitor` custom resource definitions automate target discovery — you annotate your `Service` and Prometheus finds the scrape endpoint without manual config file editing. Alerting rules in Prometheus using PromQL expressions fire when thresholds are breached and integrate with Alertmanager for routing to PagerDuty, Slack, and email.

---

## Q28. How do you expose built-in .NET runtime and ASP.NET Core metrics via OpenTelemetry?

**Concepts**
- .NET 8+ runtime emitting meters for GC, threadpool, JIT, and HTTP server
- `AddAspNetCoreInstrumentation()` subscribing to ASP.NET Core hosting meters
- `AddRuntimeInstrumentation()` exposing GC heap sizes, pause durations, and threadpool queue
- Custom meters included via `AddMeter()` alongside framework meters

**Answer**

In .NET 8+ the runtime emits meters for GC, threadpool, JIT, and HTTP server statistics; ASP.NET Core emits meters for request rate, duration, and active connections — all via `System.Diagnostics.Metrics`. You opt into them in the OTel setup by calling the appropriate `AddX()` methods.

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()   // "Microsoft.AspNetCore.Hosting" meter
        .AddHttpClientInstrumentation()   // "System.Net.Http" meter
        .AddRuntimeInstrumentation()      // GC, threadpool (OpenTelemetry.Instrumentation.Runtime)
        .AddPrometheusExporter());        // expose /metrics endpoint
```

`AddAspNetCoreInstrumentation()` subscribes to `Microsoft.AspNetCore.Hosting` and related meters, giving you request duration histograms and active request counts for free. `AddRuntimeInstrumentation()` from the `OpenTelemetry.Instrumentation.Runtime` NuGet package exposes GC heap sizes, GC pause durations, threadpool queue length, and exception counts — the signals most useful for detecting memory pressure and thread starvation. Custom meters in your own code are included by calling `AddMeter("OrderService.Payments")` alongside the framework meters, so all metrics flow through the same OTel pipeline to the same backend.

---

## Q29. What is Grafana, and how does it relate to Prometheus in a typical metrics stack?

**Concepts**
- Grafana as a visualization and dashboarding platform querying Prometheus via PromQL
- Prometheus handling data collection and storage, Grafana handling presentation
- Grafana native alerting as an alternative to Prometheus Alertmanager
- Grafana Cloud providing managed Prometheus + Loki + Tempo + Grafana in one stack

**Answer**

Grafana is a visualization and dashboarding platform that connects to time-series databases — including Prometheus — and renders charts, gauges, heatmaps, and alerts. Prometheus handles data collection and storage; Grafana handles presentation. They are separate tools almost always deployed together because Prometheus's built-in expression browser is minimal and not suitable for operational dashboards shared with a team. Grafana connects to Prometheus as a data source and issues PromQL queries against it to power dashboard panels, so any metric Prometheus stores can be visualized in Grafana without any additional configuration on the Prometheus side. Grafana also supports native alerting that can fire on the same PromQL expressions and route to notification channels such as PagerDuty, Slack, or email — an alternative to Prometheus Alertmanager for simpler setups. Grafana Cloud offers a managed stack covering Prometheus, Loki for logs, Tempo for traces, and Grafana itself, which covers all three observability pillars without self-hosting infrastructure. Pre-built community dashboards for .NET and ASP.NET Core can be imported and used immediately, giving request rate, latency, and GC panels without writing PromQL manually.

---

## Q30. What is the difference between pull-based and push-based metrics collection, and which model does Prometheus use?

**Concepts**
- Pull model — collector initiates and fetches from the service's `/metrics` endpoint
- Push model — service sends measurements to a collector or gateway
- Prometheus using the pull model with configured scrape targets
- Push model fitting short-lived jobs that exit before a pull interval completes

**Answer**

In a pull-based model, the metrics collector initiates the connection and fetches data from the service's `/metrics` endpoint on its configured interval. In a push-based model, the service sends metrics to a central collector or gateway whenever measurements are ready. Prometheus uses the pull model: it maintains a list of scrape targets and periodically calls each one, which makes Prometheus itself the single source of truth for scrape health and timing. Pull is simpler for long-running services in Kubernetes where targets are stable and the Prometheus operator handles discovery automatically. Push works better for short-lived jobs such as batch processing or function-style workloads that may exit before Prometheus scrapes them — they push to a Pushgateway or an OTLP endpoint instead, because Prometheus cannot scrape a process that has already exited. The OTel SDK supports both models: `AddPrometheusExporter()` creates a pull endpoint for Prometheus to scrape, while `AddOtlpExporter()` pushes spans and metrics to a Collector or backend on a configurable interval.

---

## Chapter 5. Health Checks

---

## Q31. What are health checks in ASP.NET Core, and why are they important in a microservices deployment?

**Concepts**
- ASP.NET Core health checks as first-class framework feature since 2.2
- Three-status model: Healthy, Degraded, Unhealthy
- Orchestrator removing unhealthy instances from rotation before users experience failures
- Pre-built community packages for SQL Server, Redis, RabbitMQ, MongoDB

**Answer**

Health checks are lightweight HTTP endpoints that report whether a service instance is capable of handling requests. In a microservices deployment, health checks are the mechanism by which orchestrators such as Kubernetes, load balancers, and service meshes decide whether to route traffic to an instance — without health checks, traffic continues to reach an instance that has lost its database connection or run out of memory, silently failing users. ASP.NET Core's `Microsoft.Extensions.Diagnostics.HealthChecks` package provides `IHealthCheck`, `AddHealthChecks()`, and `MapHealthChecks()` as a first-class framework feature. Health checks return one of three statuses: `Healthy`, `Degraded`, or `Unhealthy`; the HTTP response is 200 for `Healthy` or `Degraded` and 503 for `Unhealthy` by default, which is the signal Kubernetes interprets as a failing probe. The `AspNetCore.HealthChecks.*` community NuGet packages provide pre-built checks for SQL Server, Redis, RabbitMQ, MongoDB, and other common dependencies, so teams do not have to write probe logic from scratch.

---

## Q32. What is the difference between a liveness probe and a readiness probe in Kubernetes?

**Concepts**
- Liveness probe failure — Kubernetes kills and restarts the container
- Readiness probe failure — removes pod from endpoints, container keeps running
- Same check for both as a common mistake causing unnecessary restart loops
- ASP.NET Core tag-based filtering for separate liveness and readiness endpoints

**Answer**

A liveness probe tells Kubernetes whether the container process is still alive and should keep running — if it fails, Kubernetes restarts the container. A readiness probe tells Kubernetes whether the container is ready to receive traffic — if it fails, Kubernetes removes the pod from the load balancer endpoints but does not restart it. The distinction matters when a service is healthy but temporarily unavailable, such as during a warm-up phase or while a dependency is recovering. A liveness failure means the process is in an unrecoverable state — a deadlock, an out-of-memory error, or an infinite loop — so a restart is the right response. A readiness failure means the service is alive but cannot currently serve requests correctly, so Kubernetes should drain traffic away and wait for it to recover. Using the same health check for both is a common mistake: a pod that cannot reach its database should fail readiness — stop receiving traffic — but should not fail liveness, because Kubernetes would restart it in a loop even though the database is the problem, not the pod. ASP.NET Core supports separate endpoints via `app.MapHealthChecks("/health/live", ...)` and `app.MapHealthChecks("/health/ready", ...)` using tag-based filtering.

---

## Q33. How do you add and expose a health check endpoint in ASP.NET Core?

**Concepts**
- `AddHealthChecks()` with chained dependency checks in `Program.cs`
- `MapHealthChecks()` with `HealthCheckOptions.Predicate` for tag-based filtering
- `/health/live` for liveness — no dependency checks
- Excluding health check endpoints from authentication middleware

**Answer**

You register health checks in `Program.cs` with `builder.Services.AddHealthChecks()`, optionally chaining dependency checks, and expose them via `app.MapHealthChecks(path)` in the middleware pipeline.

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

The `/health/live` endpoint runs all checks that have no `ready` tag — typically just a check that the process is responsive, returning `Healthy` immediately. The `/health/ready` endpoint filters to checks tagged `"ready"` and returns 503 Unhealthy if any dependency is unreachable. `HealthCheckOptions.ResponseWriter` accepts a delegate to customize the JSON output; `UIResponseWriter.WriteHealthCheckUIResponse` from `AspNetCore.HealthChecks.UI.Client` is a common choice that includes check names and durations. Health check endpoints should be excluded from authentication middleware — using `AllowAnonymous` or exposing them on a separate management port — so that Kubernetes can call them without credentials.

---

## Q34. How do you write a custom `IHealthCheck` for an external dependency like a database?

**Concepts**
- `IHealthCheck.CheckHealthAsync` performing the probe and returning `HealthCheckResult`
- Cheap non-destructive probe — `SELECT 1` rather than a full business query
- `CancellationToken` passed to async calls to respect probe timeout
- `HealthCheckContext.Registration` available for check name, failure status, and tags

**Answer**

You implement `IHealthCheck` and inject your dependencies normally. The `CheckHealthAsync` method performs the actual probe and returns a `HealthCheckResult` with a status and optional description. Keep checks fast — under a few hundred milliseconds — and non-destructive, since a health check should never modify data.

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

Register it with `builder.Services.AddHealthChecks().AddCheck<InventoryApiHealthCheck>("inventory", tags: new[] { "ready" })`. For database checks, prefer a cheap query like `SELECT 1` rather than a full business query — you want to verify connectivity, not functional correctness. Always pass the `CancellationToken` to async calls inside the check; Kubernetes probes have a configured timeout and the framework cancels the token when that timeout is reached.

---

## Q35. What is a startup probe, and how does it differ from liveness and readiness probes?

**Concepts**
- Startup probe — gates liveness and readiness evaluation until the container finishes initializing
- `failureThreshold × periodSeconds` as the startup grace window
- Preventing liveness probe from restarting a legitimately slow-starting pod
- `IHostApplicationLifetime.ApplicationStarted` as a custom startup signal

**Answer**

A startup probe tells Kubernetes that the container is still initializing and that liveness and readiness probes should not yet be evaluated. If the startup probe fails within the configured `failureThreshold × periodSeconds` window, Kubernetes kills and restarts the container. Once the startup probe succeeds, Kubernetes hands off to the normal liveness and readiness probes. Startup probes exist to give slow-starting applications — loading large ML models, running EF Core migrations, or establishing a large connection pool at startup — a grace period without requiring an aggressive `initialDelaySeconds` on the liveness probe that would slow failure detection after startup completes. Without a startup probe, you must set `initialDelaySeconds` on liveness large enough to cover the worst-case startup time — often so large that a genuine post-startup crash takes minutes to detect. With a startup probe, `initialDelaySeconds` on liveness can be set to zero because the startup probe takes over for the early phase and liveness only fires once the app signals it is ready. In ASP.NET Core, `IHostApplicationLifetime.ApplicationStarted` fires once the .NET host has finished startup — you can wire a custom health check to return `Unhealthy` until that event fires, then switch to `Healthy`.

---

## Q36. What HTTP status codes do ASP.NET Core health check endpoints return, and what do they mean?

**Concepts**
- `Healthy` and `Degraded` mapping to HTTP 200 by default
- `Unhealthy` mapping to HTTP 503 — the signal Kubernetes interprets as a failing probe
- `HealthCheckOptions.ResultStatusCodes` for custom status code mapping
- `Degraded` returning 200 enabling graceful degradation without removal from rotation

**Answer**

By default, ASP.NET Core maps `Healthy` and `Degraded` to HTTP 200 OK, and `Unhealthy` to HTTP 503 Service Unavailable. Kubernetes interprets any 2xx response as a passing probe and any non-2xx as a failing probe, so the 503 for `Unhealthy` correctly triggers the orchestrator's failure handling. `Degraded` returning 200 means the service stays in the load balancer rotation even when a non-critical dependency is slow — useful for graceful degradation scenarios where partial functionality is better than no traffic. The status-to-code mapping is controlled by `HealthCheckOptions.ResultStatusCodes`, a dictionary you can override if the default 200/503 mapping does not fit your infrastructure. Some teams map `Degraded` to 200 for the readiness probe — stay in rotation, serve degraded — and to 503 for the liveness probe — restart the pod if degraded for too long — which requires two separate `MapHealthChecks` calls with different `ResultStatusCodes` configurations. The response body is `text/plain` with the status name (`Healthy`, `Unhealthy`, etc.) by default; you can replace this with structured JSON using a custom `ResponseWriter`.

---
