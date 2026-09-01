# Azure Application Insights — Interview Q&A
> 28 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is Azure Application Insights, and what problems does it solve for ASP.NET Core applications deployed on Azure?](#q1-what-is-azure-application-insights-and-what-problems-does-it-solve-for-aspnet-core-applications-deployed-on-azure)
2. [Q2. What is the relationship between Application Insights and Azure Monitor?](#q2-what-is-the-relationship-between-application-insights-and-azure-monitor)
3. [Q3. What are the primary telemetry types Application Insights ingests, and what does each one represent?](#q3-what-are-the-primary-telemetry-types-application-insights-ingests-and-what-does-each-one-represent)
4. [Q4. What is an Application Insights connection string, and how does it differ from the legacy instrumentation key?](#q4-what-is-an-application-insights-connection-string-and-how-does-it-differ-from-the-legacy-instrumentation-key)
5. [Q5. What is adaptive sampling in Application Insights, and how does it affect what you see in the portal?](#q5-what-is-adaptive-sampling-in-application-insights-and-how-does-it-affect-what-you-see-in-the-portal)
6. [Q6. How do you enable Application Insights telemetry collection in an ASP.NET Core 8 web API?](#q6-how-do-you-enable-application-insights-telemetry-collection-in-an-aspnet-core-8-web-api)
7. [Q7. What request, dependency, and exception data is collected automatically after calling `AddApplicationInsightsTelemetry()`?](#q7-what-request-dependency-and-exception-data-is-collected-automatically-after-calling-addapplicationinsightstelemetry)
8. [Q8. How can you configure Application Insights in ASP.NET Core using `appsettings.json`, environment variables, and Azure App Service settings?](#q8-how-can-you-configure-application-insights-in-aspnet-core-using-appsettingsjson-environment-variables-and-azure-app-service-settings)
9. [Q9. How does Application Insights integrate with the built-in `ILogger` pipeline in ASP.NET Core?](#q9-how-does-application-insights-integrate-with-the-built-in-ilogger-pipeline-in-aspnet-core)
10. [Q10. How do you exclude or filter specific telemetry—such as health probe requests—from being sent to Application Insights?](#q10-how-do-you-exclude-or-filter-specific-telemetrysuch-as-health-probe-requestsfrom-being-sent-to-application-insights)
11. [Q11. What is `TelemetryClient`, and when should you emit custom telemetry with it?](#q11-what-is-telemetryclient-and-when-should-you-emit-custom-telemetry-with-it)
12. [Q12. What is dependency tracking in Application Insights, and which outbound calls are instrumented by default in ASP.NET Core?](#q12-what-is-dependency-tracking-in-application-insights-and-which-outbound-calls-are-instrumented-by-default-in-aspnet-core)
13. [Q13. How do you track a custom dependency that the SDK does not auto-detect (for example, a gRPC call or message queue publish)?](#q13-how-do-you-track-a-custom-dependency-that-the-sdk-does-not-auto-detect-for-example-a-grpc-call-or-message-queue-publish)
14. [Q14. What is the difference between custom events, custom metrics, and standard request/dependency telemetry?](#q14-what-is-the-difference-between-custom-events-custom-metrics-and-standard-requestdependency-telemetry)
15. [Q15. How do you enrich telemetry with custom properties so operations teams can slice data in Log Analytics queries?](#q15-how-do-you-enrich-telemetry-with-custom-properties-so-operations-teams-can-slice-data-in-log-analytics-queries)
16. [Q16. How does Application Insights correlate telemetry across multiple services in a distributed system?](#q16-how-does-application-insights-correlate-telemetry-across-multiple-services-in-a-distributed-system)
17. [Q17. What are `operation_Id` and `operation_ParentId` in Application Insights traces, and how do they map to OpenTelemetry span relationships?](#q17-what-are-operation_id-and-operation_parentid-in-application-insights-traces-and-how-do-they-map-to-opentelemetry-span-relationships)
18. [Q18. How does W3C Trace Context (`traceparent` header) propagation work with Application Insights and `HttpClient`?](#q18-how-does-w3c-trace-context-traceparent-header-propagation-work-with-application-insights-and-httpclient)
19. [Q19. What is the end-to-end transaction view in the Application Insights portal, and when would you use it during incident response?](#q19-what-is-the-end-to-end-transaction-view-in-the-application-insights-portal-and-when-would-you-use-it-during-incident-response)
20. [Q20. How do you configure ASP.NET Core to send traces, metrics, and logs to Azure Monitor using OpenTelemetry instead of the classic Application Insights SDK?](#q20-how-do-you-configure-aspnet-core-to-send-traces-metrics-and-logs-to-azure-monitor-using-opentelemetry-instead-of-the-classic-application-insights-sdk)
21. [Q21. What is Live Metrics Stream, and how is it different from standard request logs in Log Analytics?](#q21-what-is-live-metrics-stream-and-how-is-it-different-from-standard-request-logs-in-log-analytics)
22. [Q22. How do you create an alert from Application Insights data—for example, when the failure rate exceeds a threshold?](#q22-how-do-you-create-an-alert-from-application-insights-datafor-example-when-the-failure-rate-exceeds-a-threshold)
23. [Q23. What are Application Insights availability tests (standard and multi-step web tests), and how do they complement in-app health checks?](#q23-what-are-application-insights-availability-tests-standard-and-multi-step-web-tests-and-how-do-they-complement-in-app-health-checks)
24. [Q24. What is the Application Map feature, and what telemetry does it use to draw service dependencies?](#q24-what-is-the-application-map-feature-and-what-telemetry-does-it-use-to-draw-service-dependencies)
25. [Q25. What are Smart Detection and Workbooks in Application Insights, and how do teams use them day to day?](#q25-what-are-smart-detection-and-workbooks-in-application-insights-and-how-do-teams-use-them-day-to-day)
26. [Q26. Describe a Kusto Query Language (KQL) approach to find the slowest API endpoints in the last hour.](#q26-describe-a-kusto-query-language-kql-approach-to-find-the-slowest-api-endpoints-in-the-last-hour)
27. [Q27. What is workspace-based Application Insights versus the classic standalone Application Insights resource, and why does Azure recommend the workspace model?](#q27-what-is-workspace-based-application-insights-versus-the-classic-standalone-application-insights-resource-and-why-does-azure-recommend-the-workspace-model)
28. [Q28. What are common pitfalls when adopting Application Insights in production ASP.NET Core services (sampling, personally identifiable information, cost, missing propagation)?](#q28-what-are-common-pitfalls-when-adopting-application-insights-in-production-aspnet-core-services-sampling-personally-identifiable-information-cost-missing-propagation)

---

## Q1. What is Azure Application Insights, and what problems does it solve for ASP.NET Core applications deployed on Azure?

**Concepts**
- Application performance management (APM) feature of Azure Monitor
- Automatic telemetry collection — requests, dependencies, exceptions, traces
- Log Analytics tables — KQL-queryable telemetry for dashboards and alerts
- Release comparison — latency and error rates across deployments
- Operational questions that console logging cannot answer

**Answer**

Azure Application Insights is an application performance management feature of Azure Monitor that automatically collects telemetry from running ASP.NET Core apps — requests, dependencies, exceptions, traces, and custom events — and stores it for querying, dashboards, and alerting. It answers operational questions that console logging alone cannot: which endpoints are slow, which downstream calls fail, and how errors correlate across a single user request. After you add the SDK or OpenTelemetry exporter, the host instruments incoming HTTP traffic, outbound `HttpClient` calls, dependency failures, and unhandled exceptions without rewriting business logic. Telemetry lands in Log Analytics tables such as `requests`, `dependencies`, and `exceptions` where KQL queries power blade views including Failures, Performance, and Live Metrics. For Azure App Service, Functions, or container-hosted APIs, Application Insights gives a single pane to compare releases, regions, and instances when latency or error rates spike after a deployment.

---

## Q2. What is the relationship between Application Insights and Azure Monitor?

**Concepts**
- Azure Monitor — umbrella observability platform for all Azure resources
- Application Insights — application-centric APM collector within Monitor
- Log Analytics workspace — shared storage and query engine for both
- Cross-resource correlation — app telemetry and infrastructure metrics in one KQL query
- Diagnostic settings from other Azure resources flow into the same ecosystem

**Answer**

Azure Monitor is the umbrella observability platform for Azure; Application Insights is the application-centric layer that ingests APM telemetry from your code and maps it into Monitor's data model, alerts, and workbooks. Think of Monitor as the storage, query, and alerting backbone, and Application Insights as the specialized collector and portal experience for web and API workloads. Metrics, logs, and traces from Application Insights are stored in a Log Analytics workspace (workspace-based resources) or in older classic resources that still surface through Monitor blades. Alert rules, action groups, Azure dashboards, and Grafana connectors typically target the underlying workspace tables, so infrastructure metrics — CPU, memory — and application telemetry can be correlated in one KQL query. Diagnostic settings on Azure resources such as App Service, SQL, and Service Bus also flow into the same Monitor ecosystem, which lets you relate a database DTU spike to a surge in API dependency duration.

---

## Q3. What are the primary telemetry types Application Insights ingests, and what does each one represent?

**Concepts**
- Requests — incoming HTTP calls with URL, duration, response code
- Dependencies — outbound calls with target, duration, success flag
- Exceptions — .NET exceptions with stack traces linked to requests via `operation_Id`
- Traces — `ILogger` and `TrackTrace` messages
- Custom events and metrics — opt-in business signals and numeric measurements

**Answer**

Application Insights organizes incoming data into typed telemetry items, each mapped to a Log Analytics table, so you can query requests, dependencies, exceptions, traces, custom events, and metrics separately or join them on shared correlation identifiers. Request and dependency records include duration and success flags, which drive the Performance and Application Map experiences. Exception telemetry is distinct from trace logs: exceptions carry structured stack data and often link back to the request that triggered them via `operation_Id`. Custom events and metrics are opt-in via `TelemetryClient` or OpenTelemetry instruments when automatic collection does not express domain-specific signals.

| Telemetry type | Typical table | What it captures |
|---|---|---|
| Request | `requests` | Incoming HTTP calls (URL, duration, response code) |
| Dependency | `dependencies` | Outbound calls your app makes (SQL, HTTP APIs, Azure services) |
| Exception | `exceptions` | .NET exceptions with stack traces |
| Trace | `traces` | `ILogger` and `TelemetryClient.TrackTrace` messages |
| Custom event | `customEvents` | Business events such as "CheckoutCompleted" |
| Metric | `customMetrics` | Numeric measurements (queue depth, cache hit rate) |
| Availability | `availabilityResults` | Synthetic probe results |

---

## Q4. What is an Application Insights connection string, and how does it differ from the legacy instrumentation key?

**Concepts**
- Connection string — resource identity plus regional ingestion endpoint
- Instrumentation key — resource identity only, deprecated
- `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable
- Sovereign cloud and workspace-linked endpoint override via connection string
- New applications should use connection strings exclusively

**Answer**

The connection string is the modern configuration value that tells the SDK or OpenTelemetry exporter both which Application Insights resource to send data to and which ingestion endpoint to use — for regional endpoints, sovereign clouds, and linked workspaces. The legacy instrumentation key identified only the resource and is deprecated in favor of the connection string for new applications. A connection string looks like `InstrumentationKey=...;IngestionEndpoint=https://...;LiveEndpoint=https://...` and can be set in `APPLICATIONINSIGHTS_CONNECTION_STRING`, `appsettings.json`, or Azure App Service application settings. Instrumentation keys alone cannot express endpoint overrides, which matters for Azure Government, isolated regions, or workspace-linked resources where ingestion URLs differ from the public cloud default. Microsoft documentation recommends connection strings for all new deployments because key-only configuration will eventually lose feature parity.

---

## Q5. What is adaptive sampling in Application Insights, and how does it affect what you see in the portal?

**Concepts**
- Adaptive sampling — dynamic volume reduction based on incoming traffic
- `itemCount` field — multiplier indicating how many events one row represents
- Errors and slow items retained — successful routine requests more likely dropped
- Cost and ingestion limit management
- Fixed sampling, OpenTelemetry tail-based sampling as alternatives

**Answer**

Adaptive sampling reduces the volume of telemetry the SDK sends by keeping a representative subset of successful requests while retaining virtually all failed or slow items, so high-traffic production apps stay within cost and ingestion limits without losing error signals. It adjusts the sampling percentage dynamically based on incoming traffic rather than using a fixed rate for every telemetry type. When sampling is active, each telemetry item carries an `itemCount` field indicating how many similar events the single exported row represents, and KQL aggregations must account for that multiplier to estimate true volume — so use `sum(itemCount)` rather than `count()` for accurate totals. Failed requests, exceptions, and dependencies above latency thresholds are far less likely to be dropped than routine 200 responses, which preserves diagnostic value during incidents. You can configure fixed sampling, disable sampling in lower environments, or rely on OpenTelemetry tail-based sampling upstream; turning sampling off in production without planning often produces sharp cost increases.

---

## Q6. How do you enable Application Insights telemetry collection in an ASP.NET Core 8 web API?

**Concepts**
- `Microsoft.ApplicationInsights.AspNetCore` NuGet package
- `AddApplicationInsightsTelemetry()` — registers SDK middleware and collectors
- `ApplicationInsights:ConnectionString` in configuration
- `Azure.Monitor.OpenTelemetry.AspNetCore` as the modern alternative
- Local development — separate resource or disabled export to avoid polluting production

**Answer**

Add the `Microsoft.ApplicationInsights.AspNetCore` NuGet package, register telemetry in the host builder with `AddApplicationInsightsTelemetry()`, and supply a connection string through configuration or environment variables so the SDK knows where to export data. Set `ApplicationInsights:ConnectionString` in `appsettings.json` or `APPLICATIONINSIGHTS_CONNECTION_STRING` in Azure App Service configuration; the SDK reads either location automatically. For greenfield projects, many teams prefer `Azure.Monitor.OpenTelemetry.AspNetCore`, which wires traces, metrics, and logs through OpenTelemetry and exports to the same Application Insights backend. Local development can use `AddApplicationInsightsTelemetry()` with a dev resource or disable export via an empty connection string override to avoid polluting production dashboards.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();
```

---

## Q7. What request, dependency, and exception data is collected automatically after calling `AddApplicationInsightsTelemetry()`?

**Concepts**
- Request telemetry — method, URL, response status, duration per incoming HTTP call
- Dependency telemetry — SQL, `HttpClient`, Azure SDK calls instrumented automatically
- Exception telemetry — unhandled exceptions paired with active request's `operation_Id`
- Performance counters and heartbeat on Windows App Service
- Linux/container hosts rely on OpenTelemetry runtime instrumentation for process metrics

**Answer**

The ASP.NET Core Application Insights module registers middleware and diagnostic listeners that emit a request telemetry item for every incoming HTTP call, dependency items for common outbound operations, and exception records when unhandled errors bubble out of the pipeline — without manual `Track` calls in controllers. Requests include method, URL path, response status, duration, and user/session identifiers when available; MVC controller and action names appear as custom dimensions on newer SDK versions. Dependencies are captured for SQL commands via `Microsoft.Data.SqlClient` or Entity Framework Core, HTTP calls through `HttpClient`, Azure SDK calls, and some cache/Redis providers when the matching instrumentation package is present. Exceptions are captured from middleware, filters, and first-chance handlers where configured, and are paired with the active request's operation identifier so the Failures blade groups stack traces by endpoint. Performance counters and heartbeat initialize on Windows App Service; Linux and container hosts rely more on OpenTelemetry runtime instrumentation for process metrics.

---

## Q8. How can you configure Application Insights in ASP.NET Core using `appsettings.json`, environment variables, and Azure App Service settings?

**Concepts**
- `ApplicationInsights:ConnectionString` section in `IConfiguration`
- `appsettings.Production.json` override — environment-specific values
- `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable
- Azure App Service Application Insights toggle — injects connection string automatically
- Advanced options — `ConfigureTelemetryModule`, `AddApplicationInsightsTelemetryProcessor`

**Answer**

All configuration sources feed the same `ApplicationInsights` section of `IConfiguration`, so you can commit non-secret defaults in `appsettings.json`, override per environment with `appsettings.Production.json`, and let Azure App Service application settings or Key Vault references win at runtime because they load later in the configuration chain. The environment variable `APPLICATIONINSIGHTS_CONNECTION_STRING` maps directly and is the standard pattern in containers and GitHub Actions deployments. On Azure App Service, enabling the Application Insights toggle injects the connection string automatically and can attach the agent without redeploying code. Advanced SDK options — sampling percentage, enabling or disabling dependency tracking modules, cloud role name — are set via `ConfigureTelemetryModule`, `AddApplicationInsightsTelemetryProcessor`, or OpenTelemetry exporter options rather than hard-coding in `Program.cs`.

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
  },
  "Logging": {
    "ApplicationInsights": {
      "LogLevel": { "Default": "Information" }
    }
  }
}
```

---

## Q9. How does Application Insights integrate with the built-in `ILogger` pipeline in ASP.NET Core?

**Concepts**
- `ILoggerProvider` forwarding `ILogger<T>` entries to Application Insights traces
- Structured log properties as queryable custom dimensions in `traces` table
- `Logging:ApplicationInsights:LogLevel` — filter verbose `Microsoft.*` categories
- `operation_Id` inheritance — logs correlated to active request timeline
- Message templates — structured logging pairs well with KQL dashboard slicing

**Answer**

When Application Insights logging is enabled, an `ILoggerProvider` forwards log entries written through `ILogger<T>` to Application Insights trace telemetry, preserving log level, category name, message templates, and structured properties so they appear in the `traces` table and correlated request timelines. Add the provider implicitly via `AddApplicationInsightsTelemetry()` or explicitly tune levels under `Logging:ApplicationInsights:LogLevel` to avoid shipping verbose `Microsoft.*` categories in production. Message templates such as `"Order {OrderId} processed"` become queryable custom dimensions in Log Analytics, which is why structured logging pairs well with Application Insights dashboards. Logs emitted during a request automatically inherit the active `operation_Id`, so opening a failed request in the portal shows exceptions, dependencies, and log lines on one timeline.

---

## Q10. How do you exclude or filter specific telemetry—such as health probe requests—from being sent to Application Insights?

**Concepts**
- `ITelemetryProcessor` — SDK pipeline stage that drops matching items before export
- `AddApplicationInsightsTelemetryProcessor<T>()` registration
- Filter at source — preferred over ignoring noise only in KQL dashboards
- OpenTelemetry `AspNetCoreInstrumentationFilter` for OTel-based setups
- Health endpoints — exclude from analytics, monitor probe failures separately

**Answer**

Implement an `ITelemetryProcessor` in the Application Insights SDK pipeline that drops telemetry items matching rules — such as URL paths starting with `/health` or user agents from Kubernetes kube-probes — before they are exported, which cuts noise and ingestion cost without disabling health endpoints themselves. Filtering at the source is preferable to ignoring noise only in KQL dashboards because unsampled health traffic still consumes ingestion quota. ASP.NET Core with OpenTelemetry uses `AspNetCoreInstrumentationFilter` for the same purpose.

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationInsightsTelemetryProcessor<HealthCheckFilter>();

public class HealthCheckFilter : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    public HealthCheckFilter(ITelemetryProcessor next) => _next = next;

    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry req &&
            req.Url.AbsolutePath.StartsWith("/health"))
            return;
        _next.Process(item);
    }
}
```

---

## Q11. What is `TelemetryClient`, and when should you emit custom telemetry with it?

**Concepts**
- `TelemetryClient` — SDK entry point for explicit custom telemetry
- `TrackEvent` — business milestone signals, not diagnostic text
- `TrackMetric` / `TrackTrace` / `TrackException` — numeric, text, and handled errors
- `ILogger` for diagnostic text vs `TrackEvent` for analytics-friendly business signals
- OpenTelemetry equivalent — `ActivitySource`, `Meter`, log exporters

**Answer**

`TelemetryClient` is the Application Insights SDK entry point for explicitly tracking events, metrics, traces, and exceptions that automatic instrumentation does not cover, such as business milestones, batch job progress, or handled errors you still want in the Failures view. Inject `TelemetryClient` via dependency injection (registered as singleton) and call `TrackEvent`, `TrackMetric`, `TrackTrace`, or `TrackException` with optional property bags for dimensions. Prefer `ILogger` for diagnostic text and reserve `TrackEvent` for analytics-friendly business signals product owners might query — such as `FeatureFlagEvaluated` or `PaymentCaptured`. In OpenTelemetry-first apps, analogous data is recorded with `ActivitySource`, `Meter`, and log exporters; `TelemetryClient` belongs to the classic SDK path but remains supported during migration.

---

## Q12. What is dependency tracking in Application Insights, and which outbound calls are instrumented by default in ASP.NET Core?

**Concepts**
- Dependency tracking — outbound call duration, target, result code, success flag
- Auto-instrumented — `HttpClient`, SQL via EF Core and ADO.NET, Azure Storage, Service Bus, Cosmos DB
- Dependency-to-request correlation via shared `operation_Id`
- Application Map edges — built from dependency telemetry matching downstream requests
- Uninstrumented channels — raw gRPC, proprietary TCP require manual telemetry

**Answer**

Dependency tracking records each outbound call your service makes — duration, target, result code, and success — so the portal can show which SQL database, REST API, or Azure service contributes to slow pages or cascading failures. Automatically instrumented dependencies include HTTP/HTTPS via `HttpClient` (including named and typed clients), SQL Server through EF Core and ADO.NET providers, Azure Storage, Service Bus, and Cosmos DB when the relevant Application Insights or OpenTelemetry instrumentation packages are referenced. Each dependency item links to the parent request through shared correlation fields, which powers the Application Map edges between services. Calls the SDK cannot hook — raw gRPC over custom channels, proprietary TCP protocols, or in-process-only work — require manual dependency telemetry.

---

## Q13. How do you track a custom dependency that the SDK does not auto-detect (for example, a gRPC call or message queue publish)?

**Concepts**
- `StartOperation<DependencyTelemetry>` — classic SDK manual dependency span
- `ActivitySource.StartActivity(ActivityKind.Client)` — OpenTelemetry equivalent
- `Target` and `Data` fields — meaningful for Application Map and KQL queries
- Success/failure flag — set explicitly in try/catch
- Trace context propagation into message headers for async dependencies

**Answer**

Start a dependency telemetry operation manually with `StartOperation<DependencyTelemetry>` (classic SDK) or create a child `Activity` span (OpenTelemetry) around the outbound work, set the dependency type and target name, mark success or failure, and stop the operation so duration and correlation attach to the active request. Accurate `Target` and `Data` fields make Application Map and KQL `dependencies | summarize avg(duration) by target` meaningful for operations teams. For OpenTelemetry, use `ActivitySource.StartActivity` with `ActivityKind.Client` and standard tags such as `server.address` and `messaging.system` so the Azure Monitor exporter maps spans into dependency table schema. Always propagate trace context into message headers when the dependency is asynchronous; otherwise downstream consumers start orphaned traces.

```csharp
using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("Queue publish");
operation.Telemetry.Type = "Azure Service Bus";
operation.Telemetry.Target = "orders-topic";
try
{
    await _sender.SendMessageAsync(message);
    operation.Telemetry.Success = true;
}
catch (Exception ex)
{
    operation.Telemetry.Success = false;
    _telemetryClient.TrackException(ex);
    throw;
}
```

---

## Q14. What is the difference between custom events, custom metrics, and standard request/dependency telemetry?

**Concepts**
- Request/dependency — technical HTTP and outbound-call operations, auto-collected
- Custom events — discrete business occurrences, opt-in via `TrackEvent`
- Custom metrics — numeric measurements aggregated over time via `TrackMetric` or OTel `Meter`
- Events for funnels and feature adoption vs metrics for SLA gauges and alerts
- High-cardinality numeric data as events — inflates ingestion cost

**Answer**

Request and dependency telemetry describe technical HTTP and outbound-call operations the framework observes automatically, while custom events name business occurrences you choose explicitly and custom metrics record numeric measurements you aggregate over time. Custom events appear in the Events blade and `customEvents` table; they support user/session dimensions but are not a substitute for request telemetry on API latency. Custom metrics via `TrackMetric` or OpenTelemetry `Meter` feed metric charts and alert rules on aggregations such as average, min, or max over five-minute bins. Choose events for discrete occurrences and metrics for continuous measurements; overusing events for high-cardinality numeric data inflates ingestion costs.

| Kind | Example | Primary use |
|---|---|---|
| Request | `GET /api/orders/5` | Latency and HTTP status per endpoint |
| Dependency | `HTTP api.payments.com` | Downstream reliability |
| Custom event | `SubscriptionUpgraded` | Funnels, feature adoption counts |
| Custom metric | `QueueDepth = 842` | Gauges, rates, alerting on business KPIs |

---

## Q15. How do you enrich telemetry with custom properties so operations teams can slice data in Log Analytics queries?

**Concepts**
- `ITelemetryInitializer` — runs for all telemetry items, automatic and manual
- Global properties — `DeploymentSlot`, `TenantId`, `FeatureVariant` on every record
- OpenTelemetry `ActivityProcessor` or `Enrich` callback for OTel-based apps
- Low cardinality constraint — bounded values over per-customer identifiers
- `Context.Cloud.RoleName` — differentiates services in Application Map

**Answer**

Attach key-value properties to telemetry at creation time — via `ITelemetryInitializer`, `Activity` tags, or log scopes — so every request and dependency in a deployment carries dimensions like `DeploymentSlot`, `TenantId`, or `FeatureVariant` without repeating them in each log call. Register initializers with `builder.Services.AddSingleton<ITelemetryInitializer, CloudRoleInitializer>()` so they run for automatic and manual telemetry alike. In OpenTelemetry, use `AspNetCoreInstrumentation.Enrich` or a custom `ActivityProcessor` to add tags that export to Application Insights custom dimensions. Keep cardinality low: a `CustomerId` property on millions of requests explodes storage cost; prefer bounded values such as `PlanTier=Premium` or hash identifiers when needed for support lookups.

```csharp
public class CloudRoleInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "OrderApi";
        telemetry.Context.GlobalProperties["Environment"] = "Production";
    }
}
```

---

## Q16. How does Application Insights correlate telemetry across multiple services in a distributed system?

**Concepts**
- `operation_Id` — shared trace identifier across all telemetry in one transaction
- W3C `traceparent` header propagation on `HttpClient` calls
- `cloud_RoleName` — distinguishes services in Application Map nodes and edges
- Log Analytics join on `operation_Id` — rebuilds cross-service call trees
- Parent/child linking — `operation_ParentId` maps to OTel parent span ID

**Answer**

Application Insights assigns an operation identifier to the root incoming request and propagates it — along with parent/child links — to outbound dependencies and downstream services so all telemetry items from one logical transaction share the same correlation key and appear as a single end-to-end trace. The SDK reads and writes W3C `traceparent` (and optionally `tracestate`) headers on `HttpClient` calls, allowing Service A's request telemetry to become the parent of Service B's request telemetry. Log Analytics joins `requests`, `dependencies`, and `exceptions` on `operation_Id` and related `id`/`operation_ParentId` fields to rebuild call trees spanning microservices. Cloud role names — `cloud_RoleName` — distinguish which service emitted each span, which Application Map uses to draw nodes and edges between APIs, functions, and databases.

---

## Q17. What are `operation_Id` and `operation_ParentId` in Application Insights traces, and how do they map to OpenTelemetry span relationships?

**Concepts**
- `operation_Id` — trace identifier shared by all telemetry in one distributed transaction
- `operation_ParentId` — immediate parent item identifier for span nesting
- W3C Trace Context alignment — OpenTelemetry `TraceId` maps to `operation_Id`
- Cross-process span continuation — remote service reuses trace ID via `traceparent`
- Legacy proprietary headers vs modern W3C alignment in current SDKs

**Answer**

`operation_Id` is the trace identifier shared by all telemetry items participating in one distributed transaction, while `operation_ParentId` points to the immediate parent item's identifier so the portal can nest spans in a hierarchy analogous to OpenTelemetry's trace ID and parent span ID. A root ASP.NET Core request generates a new `operation_Id`; an outbound HTTP dependency logged in the same process shares that ID and receives its own `id` as a child node. When the remote service accepts propagated `traceparent`, its root request reuses the same trace ID — OpenTelemetry `TraceId` — but sets its parent span to the caller's span ID, continuing the chain across process boundaries. Legacy Application Insights used proprietary correlation headers; modern SDKs align with W3C Trace Context, so the same trace renders correctly whether exported via classic SDK or OpenTelemetry.

---

## Q18. How does W3C Trace Context (`traceparent` header) propagation work with Application Insights and `HttpClient`?

**Concepts**
- `traceparent` header format — `00-{trace-id}-{parent-span-id}-{flags}`
- Automatic injection on instrumented `HttpClient` calls
- Missing propagation — orphaned downstream nodes in Application Map
- `tracestate` — optional vendor hints, usually optional in .NET defaults
- Custom HTTP and gRPC channels require manual header propagation

**Answer**

When ASP.NET Core handles a request, the instrumentation creates an `Activity` with a W3C-compliant trace ID and span ID, and outgoing `HttpClient` calls automatically inject a `traceparent` header so the next hop can attach its work as a child span in the same trace. The header format `00-{trace-id}-{parent-span-id}-{flags}` lets any W3C-aware service participate regardless of language; Application Insights and Azure Monitor store the parsed values on dependency and request records. Custom HTTP clients or gRPC metadata channels that bypass instrumented handlers must copy propagation headers manually — missing propagation is a common reason microservice maps show disconnected nodes. `tracestate` carries vendor-specific hints but is optional; most .NET defaults rely on `traceparent` plus baggage for cross-cutting context.

---

## Q19. What is the end-to-end transaction view in the Application Insights portal, and when would you use it during incident response?

**Concepts**
- End-to-end transaction details — Gantt timeline of all items sharing one `operation_Id`
- Per-node links — raw telemetry JSON, KQL drill-through, related exceptions
- Incident use — identify which downstream call caused a user-facing timeout
- Complements aggregate metrics — aggregate failure rate hides per-request root cause
- Navigating by request ID or exception to the transaction view

**Answer**

The end-to-end transaction details blade shows a Gantt-style timeline of every request, dependency, exception, and trace log sharing one operation identifier, which lets you see exactly which downstream call or retry caused a user-facing timeout without manually joining tables in Log Analytics. During an incident, search by failing request ID or exception, open the transaction, and walk the tree to find the longest SQL query or HTTP 503 from a partner API. Each node links to raw telemetry JSON, KQL drill-through, and related items such as all exceptions thrown in the same operation. Use it when metrics show elevated failure rate but aggregate charts hide whether problems originate in your code, a dependency, or a specific deployment slot.

---

## Q20. How do you configure ASP.NET Core to send traces, metrics, and logs to Azure Monitor using OpenTelemetry instead of the classic Application Insights SDK?

**Concepts**
- `Azure.Monitor.OpenTelemetry.AspNetCore` NuGet package
- `AddOpenTelemetry().UseAzureMonitor()` — single call wires traces, metrics, logs
- `APPLICATIONINSIGHTS_CONNECTION_STRING` — same resource as classic SDK
- Instrumentation packages — `OpenTelemetry.Instrumentation.Http`, `.SqlClient`, `.EntityFrameworkCore`
- OpenTelemetry recommended for new projects — avoids vendor lock-in, supports OTel Collector

**Answer**

Reference `Azure.Monitor.OpenTelemetry.AspNetCore`, call `AddOpenTelemetry().UseAzureMonitor()` in `Program.cs`, and set `APPLICATIONINSIGHTS_CONNECTION_STRING` so traces, metrics, and logs export through the OpenTelemetry pipeline to the same Application Insights workspace. Add instrumentation packages such as `OpenTelemetry.Instrumentation.Http`, `.SqlClient`, and `.EntityFrameworkCore` for parity with automatic dependency collection from the classic SDK. Logging integration uses `OpenTelemetryLoggerProvider`; configure log levels in standard `Logging` sections to control which categories export. OpenTelemetry is Microsoft's recommended long-term approach because it avoids vendor lock-in, supports the OpenTelemetry Collector for routing to multiple backends, and unifies tracing with `System.Diagnostics.Activity`.

```csharp
builder.Services.AddOpenTelemetry()
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });
```

---

## Q21. What is Live Metrics Stream, and how is it different from standard request logs in Log Analytics?

**Concepts**
- Live Metrics — near-real-time sampled counters via separate `LiveEndpoint`
- Standard telemetry — batched, sampled, 1–3 minute ingestion delay
- Deployment confirmation — see error rates drop seconds after a fix
- No multi-hour trend analysis — complements Log Analytics, does not replace it
- `LiveEndpoint` in connection string — distinct ingestion channel

**Answer**

Live Metrics Stream is a near-real-time channel that pushes sampled performance counters, incoming request rates, failure counts, and server health to the Azure portal within seconds, whereas standard telemetry is batched, may be sampled, and typically appears in Log Analytics with a one- to three-minute ingestion delay. Live Metrics uses a separate ingestion endpoint optimized for low latency rather than long-term retention. Operators use it during deployments or active incidents to confirm error rates dropping immediately after a fix, before KQL queries reflect the change. It complements — but does not replace — historical analysis in Log Analytics; no multi-hour trend analysis or complex KQL runs inside Live Metrics alone.

---

## Q22. How do you create an alert from Application Insights data—for example, when the failure rate exceeds a threshold?

**Concepts**
- Log alert rule — scheduled KQL query with threshold-based firing
- Action group — email, SMS, webhook, Logic App notification
- Metric alert — simpler threshold without KQL for standard metrics
- `cloud_RoleName` filter — per-environment alert rules to avoid staging noise
- Smart detection — automatic anomaly alerts as a complement

**Answer**

Define a log alert rule in Azure Monitor that runs a scheduled KQL query against the Application Insights workspace tables and fires when the result count or aggregation crosses a threshold, then attach an action group — email, SMS, webhook, Logic App — to notify on-call engineers. Metric alerts can target Application Insights standard metrics such as server response time or availability for simpler thresholds without KQL. Separate alert rules per environment using `cloud_RoleName` or custom dimensions to avoid staging noise paging production on-call. Combine with smart detection for automatic anomaly alerts on failure spikes, but tune sensitivity so seasonal traffic does not cause alert fatigue.

```kusto
requests
| where timestamp > ago(5m)
| summarize total = count(), failed = countif(success == false) by bin(timestamp, 1m)
| extend failureRate = 100.0 * failed / total
| where failureRate > 5
```

---

## Q23. What are Application Insights availability tests (standard and multi-step web tests), and how do they complement in-app health checks?

**Concepts**
- Standard (URL ping) test — single GET from Azure data centers on a schedule
- Multi-step web test — recorded request sequence with validation rules
- `availabilityResults` table — synthetic probe results independent of live traffic
- Kubernetes readiness probes — tell orchestrator about pod health, not global DNS
- External coverage — DNS, CDN, Azure Front Door paths not covered by in-cluster probes

**Answer**

Availability tests are synthetic monitors that ping your application's public URL from Azure data centers around the world on a schedule, recording response time, status code, and TLS validity, while in-app health checks prove only that the process running inside your cluster can reach its dependencies. Standard URL ping tests issue a single GET request and are good for homepage or API swagger endpoint reachability. Multi-step web tests record a sequence of requests with validation rules, simulating login or checkout flows. Synthetic failures appear in the `availabilityResults` table and Availability blade even when no real user traffic hits the bug — for example DNS or certificate misconfiguration. Kubernetes readiness probes tell the orchestrator whether to send traffic to a pod; they do not measure global DNS, CDN, or Azure Front Door paths that availability tests cover.

---

## Q24. What is the Application Map feature, and what telemetry does it use to draw service dependencies?

**Concepts**
- Application Map — live diagram of cloud roles and dependency edges
- Nodes — `cloud_RoleName` per service; edges — dependency and request correlation
- Edge health — failure rate and response time color coding
- Drill-through — clicking a node filters Failures or Performance blades
- Accuracy depends on consistent role names and trace context propagation

**Answer**

Application Map is a live diagram in the Application Insights portal that renders each monitored cloud role as a node and draws edges between services based on dependency telemetry and cross-component request links, giving a visual topology of who calls whom and where failures concentrate. Nodes represent components identified by `cloud_RoleName` — your API, a Function App, a SQL Database; edge thickness reflects call volume and color indicates health status. Edges are built from outbound dependency telemetry in the `dependencies` table matched to incoming requests on downstream components sharing correlation identifiers. Selecting a node filters Failures or Performance blades to that component, and the map is most accurate when every service uses Application Insights with consistent role names and propagates trace context.

---

## Q25. What are Smart Detection and Workbooks in Application Insights, and how do teams use them day to day?

**Concepts**
- Smart Detection — automatic anomaly engine on failure rates and latency baselines
- Workbooks — interactive KQL report templates with charts and parameters
- Smart Detection rules — slow page loads, rising exception rates, memory leaks
- Workbook templates from gallery — pre-built reports committed to source control
- Cross-data-source workbooks — Application Insights + Azure resource metrics on one canvas

**Answer**

Smart Detection is a built-in anomaly engine that automatically notifies you when failure rates, dependency durations, or trace severity patterns deviate from learned baselines, while Workbooks are interactive Azure Monitor report templates that combine KQL queries, charts, and parameters for runbooks, post-incident reviews, and executive dashboards. Smart Detection rules such as slow page loads, rising exception rates, and memory leaks run in the background without authoring KQL first, though teams tune or disable noisy detectors per application. Workbooks can merge Application Insights data with Azure resource metrics such as App Service CPU and SQL DTU on one canvas, which supports single-pane operational reviews. Teams clone Microsoft gallery workbooks or commit JSON templates to source control so on-call engineers open a pre-built API health report instead of writing ad hoc queries during outages.

---

## Q26. Describe a Kusto Query Language (KQL) approach to find the slowest API endpoints in the last hour.

**Concepts**
- `requests` table — primary source for endpoint latency data
- `percentile(duration, 95)` — tail latency rather than average for actionable results
- Filtering health check and static asset paths to reduce noise
- Join to `dependencies` on `operation_Id` — identify slow downstream calls
- Parameterized workbooks — `_TimeRange` and environment filtering for reuse

**Answer**

Query the `requests` table, filter to the recent time window, group by request name, and order by 95th percentile duration so the slowest endpoints surface at the top of the result set. Use `percentile` for tail latency because averages hide sporadic multi-second outliers. Exclude health check paths to reduce noise, and join to `dependencies` on `operation_Id` when a slow endpoint is caused by a specific downstream SQL or HTTP dependency rather than controller logic alone. Parameterize workbooks with `_TimeRange` and `Environment` so the same query works across staging and production workspaces.

```kusto
requests
| where timestamp > ago(1h)
| where name !has "/health"
| summarize
    count(),
    avgDuration = avg(duration),
    p95 = percentile(duration, 95)
  by name
| order by p95 desc
| take 20
```

---

## Q27. What is workspace-based Application Insights versus the classic standalone Application Insights resource, and why does Azure recommend the workspace model?

**Concepts**
- Workspace-based — telemetry stored in shared Log Analytics workspace
- Classic component — legacy isolated Application Insights backend storage
- Unified RBAC and retention policy across app and infrastructure logs
- Cross-resource KQL — join app telemetry with Azure resource diagnostics
- Default for new resources — classic migration encouraged

**Answer**

Workspace-based Application Insights links each Application Insights resource to a Log Analytics workspace so all application telemetry lands in the same storage and query engine as platform logs and metrics, while classic resources used dedicated siloed storage with limited cross-resource querying. New Azure portals create workspace-based resources by default; you choose or create a workspace during creation. Unified workspaces simplify retention policies, role-based access control, and export to Sentinel or external SIEM tools, since you can join application telemetry with Azure resource diagnostics in one KQL query rather than running separate queries with different app IDs. Migration tooling moves historical data and updates connection strings; plan the cutover to avoid dual-export during transition.

| | Workspace-based | Classic component |
|---|---|---|
| Storage | Shared Log Analytics workspace | Legacy Application Insights backend |
| Cross-query | Same KQL across app + infra logs | Requires separate queries / app IDs |
| Azure feature path | Default for new resources | Legacy; migration encouraged |

---

## Q28. What are common pitfalls when adopting Application Insights in production ASP.NET Core services (sampling, personally identifiable information, cost, missing propagation)?

**Concepts**
- Sampling — `itemCount` multiplier must be used in aggregations
- PII and secrets — scrub URLs, JWTs, emails in telemetry initializers
- Cost — filter health checks, tune log levels, set daily cap alerts
- Missing propagation — one uninstrumented hop splits the Application Map
- `sum(itemCount)` vs `count()` — critical distinction for accurate volume estimates

**Answer**

Teams most often get surprised by adaptive sampling hiding request volume unless KQL uses `itemCount`, by shipping personally identifiable information (PII) in URLs or custom properties, by ingestion bills growing after disabling sampling at high traffic, and by broken distributed traces when one microservice omits `traceparent` propagation. For sampling, treat sampled rows as estimates and use `sum(itemCount)` in aggregations; rely on exceptions being retained for debugging since they are far less likely to be dropped. For PII and secrets, scrub query strings, JWTs, and email addresses in telemetry initializers; never put credentials in custom dimensions since they are retained for the workspace retention period. For cost, filter health checks and static assets, tune log levels, use a daily cap alert on the workspace, and consider an OpenTelemetry Collector for pre-sampling before Azure ingestion. For propagation, ensure every service and background worker forwarding HTTP or queue messages copies W3C headers; one uninstrumented hop splits the Application Map and hides root cause during cross-service incidents.

---
