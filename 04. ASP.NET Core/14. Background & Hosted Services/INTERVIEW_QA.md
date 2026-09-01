# Background & Hosted Services — Interview Q&A
> 18 questions · Back to [README](../README.md)

## Table of Contents
1. [Q1. What is a hosted service in ASP.NET Core?](#q1-what-is-a-hosted-service-in-aspnet-core)
2. [Q2. What is `IHostedService`?](#q2-what-is-ihostedservice)
3. [Q3. What is `BackgroundService`, and how does it differ from `IHostedService`?](#q3-what-is-backgroundservice-and-how-does-it-differ-from-ihostedservice)
4. [Q4. What is the difference between `StartAsync` and `ExecuteAsync`?](#q4-what-is-the-difference-between-startasync-and-executeasync)
5. [Q5. How do you register a hosted service in DI?](#q5-how-do-you-register-a-hosted-service-in-di)
6. [Q6. Why can't you inject a Scoped service directly into a Singleton hosted service?](#q6-why-cant-you-inject-a-scoped-service-directly-into-a-singleton-hosted-service)
7. [Q7. What is `IServiceScopeFactory`, and how is it used in background work?](#q7-what-is-iservicescopefactory-and-how-is-it-used-in-background-work)
8. [Q8. What is `Channel<T>`, and how is it used for in-process queuing?](#q8-what-is-channelt-and-how-is-it-used-for-in-process-queuing)
9. [Q9. How does graceful shutdown work for hosted services?](#q9-how-does-graceful-shutdown-work-for-hosted-services)
10. [Q10. What is the role of `CancellationToken` in `BackgroundService`?](#q10-what-is-the-role-of-cancellationtoken-in-backgroundservice)
11. [Q11. What happens when Kubernetes sends SIGTERM to a pod?](#q11-what-happens-when-kubernetes-sends-sigterm-to-a-pod)
12. [Q12. What is `PeriodicTimer`, and when would you use it in a hosted service?](#q12-what-is-periodictimer-and-when-would-you-use-it-in-a-hosted-service)
13. [Q13. What is the difference between polling and event-driven background processing?](#q13-what-is-the-difference-between-polling-and-event-driven-background-processing)
14. [Q14. When should background work stay in-process vs move to an external queue/broker?](#q14-when-should-background-work-stay-in-process-vs-move-to-an-external-queuebroker)
15. [Q15. What is the outbox pattern?](#q15-what-is-the-outbox-pattern)
16. [Q16. What problems arise from unbounded parallelism in a background worker?](#q16-what-problems-arise-from-unbounded-parallelism-in-a-background-worker)
17. [Q17. How does a hosted service relate to the ASP.NET Core application lifetime?](#q17-how-does-a-hosted-service-relate-to-the-aspnet-core-application-lifetime)
18. [Q18. What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call?](#q18-what-is-the-difference-between-ihostedservice-and-a-taskrun-fire-and-forget-call)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Q1. What is a hosted service in ASP.NET Core?

**Concepts**
- Hosted service — background work tied to IHost application lifetime
- `AddHostedService<T>` singleton registration
- Coordinated startup and shutdown with Kestrel
- Worker Service vs web app hosting model

**Answer**

A hosted service is a class registered with the generic host that runs background work starting when the host starts and stopping gracefully when the host shuts down. The key is that these services integrate with `IHost` so background loops, queue consumers, and startup initialization participate in coordinated startup and shutdown rather than running as untracked threads. They are registered as singletons via `AddHostedService<T>()` and implement `IHostedService` or inherit `BackgroundService`. Examples include cache warmers, periodic sync jobs, and in-process queue consumers. The same hosting model applies to worker services and web apps, so background work can live alongside Kestrel in one process.

---

## Q2. What is `IHostedService`?

**Concepts**
- `StartAsync` — must complete quickly before host marks startup finished
- `StopAsync` — graceful resource release on shutdown
- Host-managed ordering of start and stop across all services

**Answer**

`IHostedService` is the interface defining `StartAsync(CancellationToken)` and `StopAsync(CancellationToken)` hooks that the host calls to begin and end background components during application startup and shutdown. The reason `StartAsync` must complete quickly is that the host awaits all hosted services' `StartAsync` before marking startup finished and accepting traffic, so blocking there delays port binding and causes readiness probe failures. `StopAsync` runs during shutdown and should release resources, flush buffers, and signal long-running work to exit. Any class can implement `IHostedService` directly for short-lived startup and shutdown tasks without a long-running loop, because the host manages ordering: all services start, then the web server listens; on shutdown, `StopAsync` is called with a linked cancellation token.

---

## Q3. What is `BackgroundService`, and how does it differ from `IHostedService`?

**Concepts**
- `BackgroundService` — abstract base implementing `IHostedService`
- `ExecuteAsync` — long-running loop template method
- Non-blocking `StartAsync` — queues `ExecuteAsync` and returns immediately
- Raw `IHostedService` — manual thread or timer management

**Answer**

`BackgroundService` is an abstract base class implementing `IHostedService` that schedules a long-running `ExecuteAsync(CancellationToken)` loop on a background thread while returning promptly from `StartAsync`, which is the pattern most queue consumers and polling workers need. Raw `IHostedService` requires you to manage your own background thread or timer inside `StartAsync` and `StopAsync`, while `BackgroundService.StartAsync` queues `ExecuteAsync` and returns immediately so Kestrel can start listening while the loop runs. I would override `StopAsync` to cancel the token and optionally wait for `ExecuteAsync` to observe shutdown before the process exits. The choice is simple: use raw `IHostedService` for quick initialization hooks and `BackgroundService` for continuous processing until cancellation.

---

## Q4. What is the difference between `StartAsync` and `ExecuteAsync`?

**Concepts**
- `StartAsync` — host startup hook, must return quickly
- `ExecuteAsync` — long-running loop owned by `BackgroundService`
- `stoppingToken` — linked to host shutdown cancellation
- Blocking `StartAsync` delays health and readiness probes

**Answer**

`StartAsync` is the host's startup hook that must finish quickly so the application can become ready, while `ExecuteAsync` on `BackgroundService` is where long-running loops, queue consumption, and periodic work belong. Blocking `StartAsync` with synchronous I/O or `.Wait()` on long tasks delays port binding and causes health and readiness probe failures in orchestrators, since the host awaits all `StartAsync` calls before the application is considered started. `ExecuteAsync` receives a `stoppingToken` linked to host shutdown so loops should check `stoppingToken.IsCancellationRequested` and pass the token to I/O calls. `IHostedService` has no `ExecuteAsync` — only `BackgroundService` provides that template method.

---

## Q5. How do you register a hosted service in DI?

**Concepts**
- `AddHostedService<T>` — registers as singleton implementing `IHostedService`
- Factory overload for parameterized construction
- Multiple hosted services coexisting independently

**Answer**

I call `builder.Services.AddHostedService<T>()` during service configuration, which registers the implementation as a singleton implementing both `T` and `IHostedService`.

```csharp
builder.Services.AddHostedService<EmailDispatchWorker>();
// or with factory:
builder.Services.AddHostedService(sp => new OutboxPublisher(sp.GetRequiredService<IServiceScopeFactory>()));
```

The host resolves all `IHostedService` registrations and invokes their lifecycle methods automatically, so no manual `new Thread()` is required. Because hosted services are singletons, scoped services must not be injected directly into the constructor. Multiple hosted services can coexist — outbox publisher, metrics reporter, cache refresher — each running independently with shared shutdown semantics. Supporting singletons such as channels should be registered separately and injected into the hosted service constructor.

---

## Q6. Why can't you inject a Scoped service directly into a Singleton hosted service?

**Concepts**
- Captive dependency — scoped lifetime captured by singleton
- `DbContext` scoped per unit of work, not per application lifetime
- `ValidateScopes` and `ValidateOnBuild` — fail-fast scope validation
- `ObjectDisposedException` from disposed scoped instance

**Answer**

Hosted services register as singletons and live for the entire application lifetime, while scoped services such as `DbContext` are created and disposed per request or per scope. Injecting scoped into singleton creates a captive dependency that outlives its scope, which causes `ObjectDisposedException` or stale state because the context is disposed after the first scope ends but the singleton keeps using the same reference. The DI container may allow the registration at build time unless `ValidateScopes` is enabled, but the bug surfaces at runtime. EF Core `DbContext` is specifically scoped because it tracks changes for one unit of work — reusing one instance across background jobs corrupts the change tracker and connection pooling. The fix is never to change `DbContext` to singleton — always open a new scope per work item using `IServiceScopeFactory`.

---

## Q7. What is `IServiceScopeFactory`, and how is it used in background work?

**Concepts**
- `IServiceScopeFactory` — singleton-safe scope creation
- `CreateAsyncScope` — per-job async-disposable scope
- Scope per message or batch unit, not per application lifetime
- `IDbContextFactory<TContext>` — alternative for short-lived DbContext

**Answer**

`IServiceScopeFactory` creates new DI scopes on demand, allowing singleton hosted services to resolve scoped services safely by opening a scope per background job and disposing it when the job completes.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.SaveChangesAsync(stoppingToken);
```

The key is to inject `IServiceScopeFactory` (which is singleton-safe) into the hosted service constructor rather than the scoped service itself. I create one scope per queue message, file, or batch unit — not one scope for the entire application lifetime — so each unit of work gets a fresh context that is disposed cleanly when the scope exits. `CreateAsyncScope()` supports async disposal and is preferred over `CreateScope()` in async workers. `IDbContextFactory<TContext>` is an alternative when the primary need is creating short-lived DbContext instances without a full scope.

---

## Q8. What is `Channel<T>`, and how is it used for in-process queuing?

**Concepts**
- `Channel<T>` — thread-safe producer-consumer queue
- `Channel.CreateBounded<T>` — backpressure via capacity cap
- `ChannelWriter` / `ChannelReader` — decoupled API and consumer
- In-process durability limitation — restart loses in-memory items

**Answer**

`System.Threading.Channels.Channel<T>` is a thread-safe producer-consumer queue built into .NET that decouples API request threads from background processing by letting endpoints write work items and hosted services read them asynchronously. API handlers write to `ChannelWriter<T>` and return `202 Accepted` immediately, while a `BackgroundService` reads from `ChannelReader<T>` with `ReadAllAsync(stoppingToken)`. The reason to use `Channel.CreateBounded<T>(capacity)` is that it applies backpressure when the queue fills — producers await space rather than growing memory without limit. The channel or a wrapper interface should be registered as a singleton shared between the endpoint and the consumer hosted service. Channels are in-process and not durable, so a restart loses unbounded in-memory items unless job state is persisted in a database first.

---

## Q9. How does graceful shutdown work for hosted services?

**Concepts**
- `HostOptions.ShutdownTimeout` — maximum wait before force exit
- `stoppingToken` cancellation linked to host shutdown signal
- `terminationGracePeriodSeconds` alignment for Kubernetes
- Draining in-flight work vs persisting back to durable store

**Answer**

When the host receives a shutdown signal, it cancels a linked `CancellationToken` passed to hosted services, calls `StopAsync` on each `IHostedService`, and waits up to `HostOptions.ShutdownTimeout` for background work to finish before the process exits. Loops in `ExecuteAsync` should check `stoppingToken.IsCancellationRequested` and pass the token to I/O calls so blocked operations wake promptly. I configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` to align with platform grace periods such as Kubernetes `terminationGracePeriodSeconds`. `StopAsync` should stop accepting new work, drain in-flight items within the timeout, or persist incomplete jobs back to a durable store. Swallowing `OperationCanceledException` during shutdown is correct when the service is exiting cleanly — it should not be treated as an error.

---

## Q10. What is the role of `CancellationToken` in `BackgroundService`?

**Concepts**
- `stoppingToken` — application-lifetime cancellation signal
- Propagating the token to all I/O, delays, and async calls
- SIGKILL after grace period — consequence of ignoring cancellation

**Answer**

The `stoppingToken` passed to `ExecuteAsync` is cancelled when the host begins shutdown, giving the background loop a signal to exit gracefully instead of being killed mid-operation. The key is to pass the same token to `Task.Delay`, `PeriodicTimer.WaitForNextTickAsync`, database calls, and HTTP requests so blocked operations wake promptly on shutdown rather than waiting out their full timeout. When the token fires, I finish the current work unit if possible and then exit the loop without starting new long operations after cancellation. `HttpContext.RequestAborted` is the per-request equivalent; `stoppingToken` is the application-lifetime equivalent for background work. Ignoring the token causes Kubernetes SIGKILL after the grace period, often mid-transaction, leading to duplicate charges or corrupted state on restart.

---

## Q11. What happens when Kubernetes sends SIGTERM to a pod?

**Concepts**
- SIGTERM to SIGKILL grace period — default 30 seconds
- ASP.NET Core host shutdown translation from OS signal
- Readiness probe failure on termination — stops new traffic
- Idempotent checkpointing for SIGKILL mid-job resume

**Answer**

Kubernetes marks the pod for termination, removes it from service endpoints, sends SIGTERM to the container process, waits for the configured grace period (default 30 seconds), then sends SIGKILL if the process is still running. ASP.NET Core's host translates the shutdown signal into cancellation of hosted service tokens and `StopAsync` calls, so workers must observe this to drain gracefully. Readiness probes fail immediately so the load balancer stops sending new traffic, but in-flight HTTP requests and background jobs may still be running at that point. I set `HostOptions.ShutdownTimeout` slightly below `terminationGracePeriodSeconds` so the app exits cleanly before SIGKILL. Long-running jobs should checkpoint progress in durable storage so a SIGKILL mid-job can resume idempotently on restart.

---

## Q12. What is `PeriodicTimer`, and when would you use it in a hosted service?

**Concepts**
- `PeriodicTimer.WaitForNextTickAsync` — async polling with cancellation
- Timer drift handling vs `Task.Delay` loop
- Idempotent DB updates for overlapping tick scenarios

**Answer**

`PeriodicTimer` (.NET 6+) is an async-friendly timer that exposes `WaitForNextTickAsync(CancellationToken)`, which is ideal for polling intervals in `BackgroundService` because it integrates cleanly with shutdown cancellation unlike `Task.Delay` loops alone.

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    await ProcessOverdueInvoicesAsync(stoppingToken);
}
```

I prefer it over `while(true) { await Task.Delay(...); }` because `WaitForNextTickAsync` respects timer drift and cancellation in one call. I use it for reconciliation sweeps, cache refreshes, and health checks that poll on a fixed interval, and I combine it with idempotent database updates so overlapping ticks or restarts do not double-process records. For event-driven workloads with low latency requirements, `Channel<T>` or external message brokers reduce unnecessary polling load.

---

## Q13. What is the difference between polling and event-driven background processing?

**Concepts**
- Polling — fixed-interval database or resource query
- Event-driven — channel or broker reaction to work arrival
- Hybrid: hot-path events plus nightly reconciliation timer

**Answer**

Polling repeatedly queries a data store or timer on a fixed interval to find work, while event-driven processing reacts to messages or API events as they occur, typically through channels or external brokers. Polling is simpler to implement and self-healing since missed events are picked up on the next tick, but it generates steady database load even when idle. Event-driven processing has lower latency and less idle load but requires reliable enqueue paths and reconciliation for missed events. Polling suits small fleets and low-frequency tasks while event-driven suits high-throughput notifications and billing triggers. Many production systems use a hybrid: channels or webhooks for hot paths plus a nightly reconciliation timer for drift detection.

---

## Q14. When should background work stay in-process vs move to an external queue/broker?

**Concepts**
- In-process `Channel<T>` — single-node, low-durability background work
- External broker — cross-instance distribution and durable retries
- Multi-instance deployment — in-memory channels invisible to other pods
- Poison-message handling and dead-letter queues

**Answer**

I keep work in-process when volume is low, durability requirements are modest, and a single instance handles the load. I move to an external queue such as RabbitMQ, Azure Service Bus, or Hangfire with shared storage when I need cross-instance distribution, durable retries, or isolation from the web process. In-process `Channel<T>` plus a hosted service is fine for fire-and-forget emails or report generation on a single node with persisted job rows. Multi-instance deployments need a shared queue or outbox table so any instance can pick up work, since in-memory channels are invisible to other pods. CPU-heavy or long-running jobs should not share the web app's thread pool, so offloading to worker services or dedicated consumers makes sense. External brokers add operational complexity but provide poison-message handling, dead-letter queues, and at-least-once delivery guarantees.

---

## Q15. What is the outbox pattern?

**Concepts**
- Outbox — atomic write of message and business state in one transaction
- Hosted service publisher — polls and delivers outbox rows
- Row locking for multi-instance publishers — `SKIP LOCKED`
- Idempotent consumers — at-least-once delivery after crash

**Answer**

The outbox pattern writes outbound messages or integration events to a database table in the same transaction as the business state change, then a separate publisher process reads unpublished rows and sends them to a broker, guaranteeing consistency between the database and downstream systems. The reason this works is that business code updates entities and inserts an outbox row atomically, so there is no "database committed but message lost" race condition. A hosted service polls or leases outbox rows, publishes to the message broker, and marks rows published only after confirmed delivery, or uses a state machine such as Pending → Processing → Published. Multi-instance publishers require row locking with `FOR UPDATE SKIP LOCKED` or `UPDLOCK` so two workers do not publish the same message. Consumers must be idempotent because at-least-once delivery can produce duplicates after crashes between publish and mark-published steps.

---

## Q16. What problems arise from unbounded parallelism in a background worker?

**Concepts**
- `Task.WhenAll` without concurrency cap — thread pool exhaustion
- Database connection pool exhaustion under parallel scopes
- `SemaphoreSlim` or `Parallel.ForEachAsync` — bounded concurrency
- Idempotency keys for partial-failure retries

**Answer**

Unbounded parallelism — for example `Task.WhenAll` over thousands of files or messages without a concurrency cap — exhausts thread pool threads, database connection pools, and memory, and increases duplicate processing and partial-failure corruption under load. Each parallel task may open a scoped `DbContext` or HTTP connection simultaneously, hitting pool limits and causing timeouts across the entire application. Without per-item error isolation, one failure's semantics become unclear when mixed with concurrent successes. I limit concurrency with `SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or bounded channel readers. Combining bounded parallelism with idempotency keys means retries after partial failure do not double-charge or duplicate side effects.

---

## Q17. How does a hosted service relate to the ASP.NET Core application lifetime?

**Concepts**
- `IHost` lifetime — Kestrel and hosted services share one process
- `IHostApplicationLifetime` — ApplicationStarted/Stopping/Stopped callbacks
- Root service provider disposal on shutdown
- Worker Service template — hosted services without Kestrel

**Answer**

Hosted services start after the host builds the service provider and complete their `StartAsync` before the application is considered started, and they receive shutdown signals through the same `IHost` lifetime that stops Kestrel and disposes the root service provider. In a web app, Kestrel and hosted services share one process so background workers run concurrently with HTTP request handling. `IHostApplicationLifetime` exposes `ApplicationStarted`, `ApplicationStopping`, and `ApplicationStopped` for registering callbacks around hosted service work. When the root provider disposes at shutdown, singleton dependencies are disposed as well, which is another reason scoped services must not be captured for the application's entire lifetime. Worker Service templates use the same `IHost` without Kestrel, demonstrating that hosted services are not web-specific.

---

## Q18. What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call?

**Concepts**
- `Task.Run` fire-and-forget — untracked, unobserved exceptions
- `IHostedService` — coordinated startup, shutdown, and exception logging
- `StopAsync` — cleanup hook absent in fire-and-forget
- `AddHostedService<T>` vs `_ = Task.Run(...)` from request handlers

**Answer**

`IHostedService` is registered with the host, participates in coordinated startup and shutdown, and receives cancellation when the application stops, whereas `Task.Run` fire-and-forget work is untracked, may outlive intended scope, and is aborted abruptly on process exit without cleanup. Fire-and-forget tasks started from a controller or middleware are not awaited by the host, so exceptions may go unobserved and shutdown will not wait for them. Hosted services run under the host's exception logging and lifecycle management, and `StopAsync` provides a hook to flush state. `Task.Run` inside a hosted service for parallel work is acceptable when bounded and awaited within `ExecuteAsync`, but it should not be used as a substitute for registering the worker itself. For background work in ASP.NET Core, always prefer `AddHostedService<T>()` over `_ = Task.Run(...)` from request handlers.

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Concepts**
- `UseRouting` before `UseAuthentication` and `UseAuthorization`
- Endpoint metadata — only available after routing selects the endpoint
- `[Authorize]` policy resolution depends on endpoint selection

**Answer**

In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata. When auth runs before routing, the endpoint has not been selected yet, which means `[Authorize]` metadata on minimal routes or controllers may not apply correctly and policy resolution breaks silently. The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints. Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.

---

#### Gotcha 2. Scoped service in a Singleton

**Concepts**
- Captive dependency — singleton outliving the scoped instance
- EF change tracker corruption across requests
- `ValidateScopes` — startup detection of scope violations
- `IServiceScopeFactory` or `IDbContextFactory<T>` as fix

**Answer**

Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`. The singleton holds one scoped instance forever rather than one per request, so EF change trackers accumulate unrelated entities. Enabling `ValidateScopes` in Development and staging catches illegal scope combinations at startup. The fix is injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation. This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Concepts**
- `HttpClient` socket exhaustion from per-use instantiation
- `IHttpClientFactory` — handler lifetime and connection pooling
- Named or typed client registration

**Answer**

Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected. `HttpClient` is disposable but not meant for per-use disposal, so `using var client = new HttpClient()` in a singleton is an anti-pattern. `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly. I register named or typed clients with `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>()`. Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Concepts**
- `IOptions<T>` — snapshot at first resolution, never updates
- `IOptionsSnapshot<T>` — recalculates per request scope
- `IOptionsMonitor<T>` — change notifications via `OnChange`
- Singleton services require `IOptionsMonitor<T>` for live config

**Answer**

`IOptions<T>` captures a configuration snapshot at first resolution, so reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled. `IOptionsSnapshot<T>` recalculates per request scope while `IOptionsMonitor<T>` supports change notifications via `OnChange`. Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates. Misconfiguration persists silently until process restart when `.Value` was cached at construction.

---

#### Gotcha 5. GET with `[FromBody]`

**Concepts**
- HTTP GET body — stripped by proxies, caches, and browsers
- `[FromQuery]` with `[AsParameters]` for complex GET filters
- Silent binding failure in production vs Swagger

**Answer**

Using `[FromBody]` on GET action parameters is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies so binding fails silently in production. Query strings and route values are the correct binding sources for GET requests. Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys. Failures often appear only in specific browsers or CDN layers, not in Swagger during development. REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Concepts**
- `JsonNamingPolicy.CamelCase` — ASP.NET Core 8 default
- `PropertyNameCaseInsensitive` — opt-in case-insensitive binding
- Silent default-value binding when keys do not match

**Answer**

ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys such as `"CustomerName"` may not bind to `CustomerName` unless case-insensitive matching is enabled. Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values such as empty string or zero. I prefer standardizing clients on camelCase and documenting the contract in OpenAPI. The optional mitigation is `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)`, but explicit camelCase contracts are cleaner. Adding validation attributes turns silent binding failures into 400 responses rather than corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Concepts**
- `throw ex` — resets stack trace to catch block
- Bare `throw` — preserves original exception origin
- `throw new WrapperException("...", ex)` — preserving InnerException

**Answer**

Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown. Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis, so I always use `throw;` when rethrowing after logging or cleanup. Wrapping in a new exception is appropriate only when adding context: `throw new OrderProcessingException("...", ex)` preserves the `InnerException`. This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Concepts**
- Kestrel — application server, not an edge gateway
- Reverse proxy — TLS termination, WAF, rate limiting
- `UseForwardedHeaders` — client IP and scheme restoration

**Answer**

Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require. Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front. TLS certificates are easier to manage at the proxy layer with automatic renewal. Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy. Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Concepts**
- `launchSettings.json` — development ergonomics only, not deployed
- `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT` — runtime configuration
- `appsettings.Production.json` — correct production values location

**Answer**

Settings in `Properties/launchSettings.json` including `applicationUrl`, environment variables, and launch profiles apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile — they are not deployed to production hosts. Production URLs and environment come from environment variables such as `ASPNETCORE_URLS` and `ASPNETCORE_ENVIRONMENT`, container configuration, or IIS and nginx site settings. Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments. The file is development ergonomics, not runtime configuration, so `appsettings.Production.json` and host-level env vars are the correct locations for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Concepts**
- Non-nullable `bool` — cannot distinguish omitted from explicit `false`
- `bool?` for tri-state PATCH intent
- System.Text.Json deserializes missing properties to `default`

**Answer**

A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics. PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent. Marketing consent and feature flags are common domains where this bug causes compliance or logic errors. Create DTOs may use non-nullable bool when explicit values are always required on insert. Nullable fields should be documented in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Concepts**
- `X-Forwarded-For`, `X-Forwarded-Proto`, `X-Forwarded-Host` headers
- `ForwardedHeadersOptions` — trust only known proxy networks
- `Request.Scheme` and client IP wrong without forwarded headers

**Answer**

Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs. I call `UseForwardedHeaders()` early, before middleware that reads scheme or host such as HTTPS redirection, link generation, and rate limiting by IP. I configure `ForwardedHeadersOptions` to trust only my reverse proxy network because trusting all proxies enables header spoofing. Local development without a proxy does not need this; production behind nginx, IIS, or an ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Concepts**
- `UseStaticFiles` — serves `wwwroot` to unauthenticated clients
- Sensitive config outside web root — `IConfiguration`, secret managers
- Accidental `appsettings.Production.json` in `wwwroot`

**Answer**

Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default, so placing secrets, `.env` files, backup configs, or private keys there exposes them over HTTP. Only public assets such as CSS, JS, images, and public PDFs belong in `wwwroot`. Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers. Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident, so build pipelines should verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Concepts**
- SPA fallback — must be registered after API endpoint mapping
- `/api/*` 404 responses returned as `index.html`
- Middleware registration order in `Program.cs`

**Answer**

SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers. I map API routes with `MapControllers` or minimal API groups before `MapFallbackToFile("index.html")`, and scope the fallback to non-API paths or use conditional fallback that excludes `/api` prefixes. Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting. The order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Concepts**
- Singleton hosted service — cannot constructor-inject scoped services
- `IServiceScopeFactory.CreateAsyncScope` — per-job scope
- `ValidateScopes` — startup detection of injected scoped services

**Answer**

A singleton `BackgroundService` that injects scoped services such as `DbContext` or repositories directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration. Hosted services live for the application lifetime so scoped dependencies must not be constructor-injected. The fix is to inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes. The same rule applies to timers and `Task.Run` loops started from singletons. Enabling `ValidateScopes` catches this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Concepts**
- SignalR backplane — Redis or Azure Service Bus for cross-instance fan-out
- Sticky sessions — per-client affinity, not cross-instance event routing
- `AddStackExchangeRedis` — backplane registration

**Answer**

SignalR broadcasts from one server instance reach only clients connected to that instance. Without a Redis or Azure Service Bus backplane, users on different nodes never receive each other's real-time events, since each instance only knows about its local connections. Sticky sessions keep one client on one node but do not route events raised on other nodes to that client. I register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application. Raw WebSocket apps need equivalent custom pub/sub while SignalR's backplane is the built-in solution. Multi-instance scale-out should be tested with at least two instances before launch.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A team registers both a custom `IHostedService` that only implements `StartAsync`/`StopAsync` and a `BackgroundService` subclass. When would you choose each, and what does `BackgroundService` provide on top of raw `IHostedService`?

**Concepts**
- Raw `IHostedService` — startup/shutdown hooks only
- `BackgroundService.ExecuteAsync` — off-startup-path long-running loop
- `stoppingToken` — linked to host shutdown in `ExecuteAsync`
- `StartAsync` blocking delays Kestrel readiness

**Answer**

I use raw `IHostedService` for short startup and shutdown hooks such as warming caches or registering timers, and `BackgroundService` for long-running loops. The key thing `BackgroundService` provides is a default `StartAsync` that schedules `ExecuteAsync` on a background thread and returns immediately, so Kestrel can start listening and health checks can pass while the loop runs. With raw `IHostedService`, anything awaited in `StartAsync` blocks the entire host startup — if it never completes, the process hangs before listening on ports. `BackgroundService.ExecuteAsync(CancellationToken stoppingToken)` receives a token cancelled on shutdown, so the loop exits cleanly when the host stops. I override `StopAsync` in either type to flush buffers or drain in-flight work. I choose raw `IHostedService` when integrating third-party components with their own lifecycle API that does not fit the `ExecuteAsync` template.

---

#### Q2. (R) Review this email-dispatch hosted service. The app starts cleanly in Development but throws `ObjectDisposedException` on the second queued email in Production. What is wrong?

```csharp
public sealed class EmailQueueService : BackgroundService
{
    private readonly Channel<EmailJob> _queue = Channel.CreateUnbounded<EmailJob>();
    private readonly AppDbContext _db; // scoped registration

    public EmailQueueService(AppDbContext db) => _db = db;

    public ValueTask EnqueueAsync(EmailJob job) =>
        _queue.Writer.WriteAsync(job);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            var user = await _db.Users.FindAsync(job.UserId);
            await SendAsync(user!.Email, job.Template, stoppingToken);
            job.MarkSent();
            await _db.SaveChangesAsync(stoppingToken);
        }
    }
}
```

**Concepts**
- Captive dependency — scoped `DbContext` in singleton hosted service
- `ObjectDisposedException` after first scope ends
- EF change tracker corruption across jobs
- `IServiceScopeFactory.CreateAsyncScope` per email job

**Answer**

A scoped `AppDbContext` is injected into a singleton `BackgroundService`, creating a captive dependency — the context is disposed when the root scope ends, so the second queue item touches a disposed `DbContext`. In Development this is hidden because `ValidateScopes` is often not enabled, but in Production the second call fails with `ObjectDisposedException`. Beyond the disposal error, reusing one `DbContext` across jobs corrupts the change tracker with unrelated entities and causes connection pooling issues.

The fix is to remove `AppDbContext` from the constructor, inject `IServiceScopeFactory`, and create a new scope per email job:

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var user = await db.Users.FindAsync(new object[] { job.UserId }, stoppingToken);
```

Enabling `ValidateScopes` and `ValidateOnBuild` in staging would have caught this at startup before it reached Production.

---

#### Q3. (P) Kubernetes sends SIGTERM and gives the pod 30 seconds before SIGKILL. How should a `BackgroundService` honor graceful shutdown using `CancellationToken`, and what must you configure so in-flight HTTP calls and queue items can finish?

**Concepts**
- `stoppingToken` — observe and propagate through all I/O
- `HostOptions.ShutdownTimeout` — slightly below `terminationGracePeriodSeconds`
- `IHostApplicationLifetime.ApplicationStopping` — last-chance callbacks
- Drain or requeue in-flight items within the grace window

**Answer**

I observe the `stoppingToken` passed to `ExecuteAsync`, pass it through to every I/O call, delay, and channel read, and avoid swallowing `OperationCanceledException` during shutdown so the loop exits cleanly when the signal arrives. `ExecuteAsync` should exit loops when `IsCancellationRequested` is true, and all blocking operations such as `ReadAllAsync`, `HttpClient` calls, and `Task.Delay` should receive the same token so they wake promptly on SIGTERM rather than waiting out their full timeout. I configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` — slightly under the Kubernetes `terminationGracePeriodSeconds` — to allow the host to flush logs before SIGKILL. For in-flight queue items, `StopAsync` should stop accepting new work and drain with a timeout, or mark items back to pending in the database if shutdown exceeds the grace window. `IHostApplicationLifetime.ApplicationStopping` registers last-chance callbacks for metrics flushing and leader election release.

---

#### Q4. (P) You need to offload heavy report generation from API requests without losing work on restart. Describe the in-process queue pattern with `Channel<T>` and a hosted consumer — what belongs in the API vs the worker, and when do you outgrow in-process queues?

**Concepts**
- API enqueues durable job row then writes to `Channel<T>`
- `BackgroundService` consumer with `CreateAsyncScope` per job
- `Channel.CreateBounded<T>` — backpressure against OOM
- In-process limit — multi-instance deployments need external broker

**Answer**

The API endpoint validates the request, persists a job row with `Status = Queued`, optionally writes the job ID to `Channel<T>`, and returns `202 Accepted` with a status polling URL immediately. The `BackgroundService` consumer reads from `Channel.CreateBounded<T>` (providing backpressure so OOM is bounded), and for each job creates an async scope, resolves services, updates job status, stores the artifact path, and handles failures with a retry count. I register the channel writer as a singleton shared between the API and the hosted service, wrapped in an interface such as `IReportQueue`. I outgrow this pattern when deployments have multiple instances (each pod has its own channel), when restart loses unbounded in-memory items that have not yet been committed to the database, or when CPU-heavy jobs starve HTTP threads — at that point I move to RabbitMQ, Azure Service Bus, or Hangfire with shared storage.

---

#### Q5. (D) Two designs poll for overdue invoices: (A) `PeriodicTimer` in `ExecuteAsync` with a DB query every minute, (B) `Channel<T>` fed by API events plus a nightly reconciliation sweep. Compare throughput, duplicate processing risk, and shutdown behavior.

**Concepts**
- `PeriodicTimer` polling — simple and self-healing, constant DB load
- Event-driven channel — low latency, requires reconciliation for missed events
- Idempotent SQL — mitigates duplicate processing in both designs
- Lease timestamps — prevent double billing across instances

**Answer**

Event-driven `Channel` processing reduces idle DB load and latency for new overdue items but requires reconciliation for missed events, while `PeriodicTimer` polling is simpler and self-healing but generates steady database load even when idle and extends shutdown until the in-flight tick completes.

For throughput, the channel design processes items near real-time as API events arrive while the polling design has latency up to one poll interval. For duplicates, the polling design needs idempotent update SQL since the same overdue invoice may appear on consecutive ticks, while the channel design must handle duplicate enqueues from retried API calls and at-least-once delivery. For shutdown, `PeriodicTimer.WaitForNextTickAsync(stoppingToken)` cancels cleanly and the current tick can finish; the channel design stops the reader and drains or requeues in-flight items.

I use `PeriodicTimer` with `WaitForNextTickAsync(stoppingToken)` in option A rather than `Task.Delay` loops for cleaner cancellation. The channel design should mark invoices `Processing` with lease timestamps to avoid double billing across instances. The hybrid approach — channel for hot path, timer reconciliation for drift detection — is the most robust pattern for billing systems.

---

#### Q6. (R) Review this `BackgroundService` that processes files in parallel. Under load, SQL connection pool exhaustion and duplicate charges appear. What are the defects?

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var files = Directory.GetFiles(_inbox, "*.json");
        var tasks = files.Select(async path =>
        {
            var json = await File.ReadAllTextAsync(path, stoppingToken);
            var order = JsonSerializer.Deserialize<OrderPayload>(json)!;
            await _billing.ChargeAsync(order); // uses DbContext inside
            File.Delete(path);
        });
        await Task.WhenAll(tasks); // unbounded parallelism
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}
```

*(Assume `_billing` is a scoped service injected into the singleton hosted service via constructor.)*

**Concepts**
- Captive scoped `_billing` in singleton — pool exhaustion
- `Task.WhenAll` unbounded — connection and thread starvation
- `File.Delete` before durable idempotency proof — data loss on crash
- `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` — bounded fix

**Answer**

The service has four problems compounding each other. First, a scoped `_billing` service is captured by a singleton hosted service, so all parallel tasks share one `DbContext` which races and exhausts connections. Second, `Task.WhenAll` on all files starts unlimited concurrent tasks, each of which may open additional connections via the captive scoped service, pushing the pool to its limit. Third, `File.Delete` runs before any durable idempotency proof — if the process crashes after delete but before commit, the file is gone and the charge never recorded; if it runs again from a backup it double-charges. Fourth, there is no per-file try/catch so one failure's exception semantics are unclear when mixed with concurrent successes.

The fixes in priority order: inject `IServiceScopeFactory` and create one scope per file; cap parallelism with `Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = stoppingToken }, ...)` or a `SemaphoreSlim`; move the file to a `processing/` folder before charging and to `completed/` only after successful commit, using an idempotency key in `ChargeAsync`; wrap per-file logic in try/catch and quarantine poison files rather than deleting on first failure.

---

#### Q7. (M) `IHostedService.StartAsync` runs during host startup. What happens if `StartAsync` blocks on long synchronous work or awaits a never-completing task, and how does that differ from work placed in `BackgroundService.ExecuteAsync`?

**Concepts**
- `StartAsync` blocking — delays Kestrel port binding and health checks
- Never-completing task in `StartAsync` — full startup deadlock
- `BackgroundService.StartAsync` — queues `ExecuteAsync` and returns immediately
- `IHostApplicationLifetime.ApplicationStarted` — post-listen deferral

**Answer**

Blocking `StartAsync` delays the entire host startup because the host awaits all `IHostedService.StartAsync` calls before marking startup complete and binding ports. Awaiting a never-completing task in `StartAsync` deadlocks startup entirely — the process hangs before listening on any port, health probes time out, and orchestrators restart the pod in a loop. Synchronous CPU work or `.Wait()` on long tasks has the same effect since the thread is blocked and the host cannot proceed.

`BackgroundService` solves this by overriding `StartAsync` to queue `ExecuteAsync` on the host's background thread and return immediately, so the appropriate place for polling loops and queue consumers is `ExecuteAsync`, not `StartAsync`. Heavy initialization such as loading a large cache should either complete within a short timeout in `StartAsync`, or defer into `ExecuteAsync` behind a readiness gate if partial startup is acceptable. `IHostApplicationLifetime.ApplicationStarted` can defer work until after the server is listening when initialization must happen post-bind.

---

#### Q8. (R) This outbox publisher runs as a hosted service. Messages are marked published in the database but never reach the broker after deploys. Review the loop.

```csharp
public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IMessageBus _bus; // singleton wrapper around broker client

    public OutboxPublisher(IServiceScopeFactory scopes, IMessageBus bus)
    {
        _scopes = scopes;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var batch = await db.Outbox.Where(x => !x.Published).Take(50).ToListAsync(stoppingToken);

            foreach (var msg in batch)
            {
                await _bus.PublishAsync(msg.Payload, stoppingToken);
                msg.Published = true;
            }
            await db.SaveChangesAsync(stoppingToken);
            // no delay — tight loop
        }
    }
}
```

*(Consider exceptions, shutdown, and duplicate delivery.)*

**Concepts**
- Set `Published = true` before confirmed broker ack — data loss on throw
- Tight loop without backoff — hammers DB and broker on outage
- `SKIP LOCKED` row locking for multi-instance safety
- Message idempotency key — deduplication after crash and re-publish

**Answer**

The publisher has five problems. It sets `msg.Published = true` and then calls `SaveChangesAsync` after publishing — if `PublishAsync` throws, the exception is unhandled and the batch is retried but the broker may have already received some messages, producing duplicates without dedup keys. Conversely, if `SaveChangesAsync` throws after a successful publish, the row stays `Published = false` and is published again on the next iteration. There is no `try/catch` per message and no backoff, so a broker outage causes a tight loop hammering the database at full speed. Without row locking (`UPDLOCK`/`SKIP LOCKED`), two instances running the same query pick up and publish the same 50 rows simultaneously.

The fixes in priority order: use a two-phase approach with a `Processing` lease column — mark row as `Processing` in one transaction, publish, then mark `Published` in a separate transaction; wrap each message in try/catch and increment an `AttemptCount` with dead-lettering after a threshold; add `await Task.Delay(pollInterval, stoppingToken)` or use `PeriodicTimer` between batches; use `FOR UPDATE SKIP LOCKED` or `UPDLOCK ROWLOCK READPAST` for multi-instance safety; pass the message ID as an idempotency key to broker consumers.
