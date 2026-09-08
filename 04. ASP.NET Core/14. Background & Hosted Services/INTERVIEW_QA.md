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

## Gotchas — Background & Hosted Services (Interview Traps)

---

#### Gotcha 1. `BackgroundService` is a singleton — injecting scoped services directly causes captive dependency

**Concepts**
- Hosted services registered as singletons for application lifetime
- Scoped `DbContext` injected into singleton living past its scope
- `IServiceScopeFactory` for creating per-operation scoped resolution
- `ValidateScopes` catching illegal scope injection at startup in Development

**Answer**

`BackgroundService` is registered as a singleton because hosted services live for the application lifetime. Injecting a scoped service like `DbContext` directly into its constructor creates a captive dependency — the scoped instance is created once at startup and held indefinitely instead of being created and disposed per unit of work. If `ValidateScopes` is enabled in Development, the host throws at startup. If not, the EF Core change tracker accumulates stale entities across all background iterations and eventually throws `ObjectDisposedException`. The correct pattern is injecting `IServiceScopeFactory`, then inside `ExecuteAsync` using `await using var scope = factory.CreateAsyncScope()` and resolving services from `scope.ServiceProvider`.

---

#### Gotcha 2. Unhandled exceptions in `ExecuteAsync` stop the hosted service — .NET 5 and earlier crash the host

**Concepts**
- Unhandled exception in `ExecuteAsync` completing the internal `Task`
- `BackgroundService` monitoring the task internally and stopping the service
- .NET 5 and earlier: unhandled exception crashes the entire host process
- .NET 6+: `BackgroundServiceExceptionBehavior` controlling crash vs stop behavior

**Answer**

If `ExecuteAsync` throws an unhandled exception, the `BackgroundService` infrastructure catches it, logs it, and marks the service as stopped. In .NET 5 and earlier, this behavior is configurable but defaults to crashing the entire host process (consistent with Worker Service behavior). In .NET 6 and later, the default changed to `Ignore` — the exception stops the service but the host continues running, which can leave the application in a degraded state without the background processing silently. Configure `HostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost` if the background service is critical and its failure should take down the app so infrastructure restart policies trigger.

---

#### Gotcha 3. `StopAsync` timeout — graceful shutdown may be truncated by the default 5-second timeout

**Concepts**
- `IHostedService.StopAsync(CancellationToken)` called during host shutdown
- Default host shutdown timeout of 5 seconds
- Long-running cleanup work truncated when timeout expires
- `HostOptions.ShutdownTimeout` for extending the shutdown window

**Answer**

When the application shuts down, the host calls `StopAsync` on all hosted services with a cancellation token that fires after the configured shutdown timeout (default 5 seconds). Background services that perform cleanup work — flushing queues, completing in-flight database writes, sending a final message — may be aborted mid-cleanup when the timeout expires. Extend the timeout with `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30))` for services with meaningful shutdown work. Inside `StopAsync`, respect the cancellation token and exit promptly when cancelled rather than ignoring it, so clean shutdown is possible within the extended window.

---

#### Gotcha 4. `CancellationToken` not checked in tight loops — background service ignores shutdown signal

**Concepts**
- `ExecuteAsync(CancellationToken stoppingToken)` receiving shutdown signal
- Tight processing loop without checking `stoppingToken.IsCancellationRequested`
- `await Task.Delay(interval, stoppingToken)` respecting cancellation for delays
- Graceful shutdown requiring loop exit on cancellation

**Answer**

A `BackgroundService` loop that processes work without checking the `stoppingToken` continues running after the host initiates shutdown, preventing graceful termination and extending shutdown time unnecessarily. Replace `while (true)` with `while (!stoppingToken.IsCancellationRequested)` or use `await Task.Delay(pollingInterval, stoppingToken)` inside the loop — when `stoppingToken` fires, `Task.Delay` throws `OperationCanceledException`, which `ExecuteAsync` should let propagate (do not catch `OperationCanceledException` unless to perform cleanup). Pass `stoppingToken` to all async calls inside the loop so database queries, HTTP calls, and queue reads also cancel promptly when shutdown is requested.

---

#### Gotcha 5. Multiple hosted services start in registration order but stop in reverse — dependencies on execution order are fragile

**Concepts**
- `AddHostedService<T>()` registering multiple services
- Startup order: services start in registration order
- Shutdown order: services stop in reverse registration order
- No explicit coordination mechanism between hosted services

**Answer**

Multiple services registered with `AddHostedService<T>()` start in the order they were registered and stop in reverse order. While this is deterministic, there is no built-in coordination mechanism to ensure one service is fully operational before another begins processing. Background services that consume a queue filled by another background service may start and process before the producer is ready. Use `IHostApplicationLifetime.ApplicationStarted` to delay dependent service work until all services have completed their `StartAsync` phase, or use a shared coordination primitive like a `SemaphoreSlim` or a `Channel<T>` that naturally buffers until a producer is running.

---

#### Gotcha 6. `IHostedService.StartAsync` must not block — long initialization work blocks the entire host startup

**Concepts**
- `StartAsync` intended for lightweight, non-blocking initialization
- `ExecuteAsync` for long-running background work
- Host startup timeout affected by blocking `StartAsync`
- Fire-and-forget task from `StartAsync` for deferred startup work

**Answer**

`IHostedService.StartAsync` must return quickly — it is called sequentially for all hosted services during host startup, and a service that blocks in `StartAsync` (waiting for a database, warming a cache, or running migrations) delays the startup of all subsequent services and can trigger host startup timeouts. Long-running work belongs in `ExecuteAsync` for `BackgroundService` subclasses, or started as a background task in `StartAsync` and stored for later cancellation. If a service must wait for a dependency to be healthy before starting work, defer that check to the first iteration of `ExecuteAsync` rather than blocking `StartAsync`.

---

#### Gotcha 7. Fire-and-forget `Task.Run` from a controller — not tracked, exceptions lost, not cancelled on shutdown

**Concepts**
- `_ = Task.Run(...)` from controller action bypassing hosted service lifecycle
- Unobserved `TaskException` event for fire-and-forget exceptions
- Graceful shutdown not waiting for fire-and-forget tasks
- `IHostedService` as the correct mechanism for background work

**Answer**

Starting background work from a controller action with `_ = Task.Run(...)` or `_ = Task.Factory.StartNew(...)` creates an untracked task that is not monitored by the host, not cancelled during shutdown, and whose exceptions are silently swallowed (as unobserved task exceptions). If the application shuts down while the task is running, it is aborted without cleanup. For any background work that must complete reliably, use `IHostedService` or `BackgroundService` registered via `AddHostedService<T>()`. If work must be triggered by a request, enqueue it into a `Channel<T>` or a queue that a registered background service consumes — this decouples the request from the background execution and keeps work lifecycle management in the host.

---

#### Gotcha 8. `IHostApplicationLifetime` vs `CancellationToken` in `ExecuteAsync` — different cancellation signals

**Concepts**
- `stoppingToken` in `ExecuteAsync` fired when service's `StopAsync` is called
- `IHostApplicationLifetime.ApplicationStopping` fired when host begins shutdown
- `ApplicationStarted` for post-startup work that should not run before app is ready
- Using both for different coordination needs

**Answer**

The `stoppingToken` passed to `ExecuteAsync` fires when the individual service's `StopAsync` is invoked, which normally coincides with host shutdown but can also be invoked independently. `IHostApplicationLifetime.ApplicationStopping` fires when the host begins its shutdown sequence, which may precede `StopAsync` calls. `ApplicationStarted` fires after all hosted services have completed `StartAsync` and the host is fully running. For a background service that should wait until the application is fully started before beginning work, register a callback on `ApplicationStarted` inside `ExecuteAsync` rather than immediately processing — this prevents the service from attempting to use databases or services that have not finished their own startup.

---

#### Gotcha 9. `PeriodicTimer` vs `Task.Delay` — `PeriodicTimer` does not drift, `Task.Delay` accumulates timing error

**Concepts**
- `Task.Delay(interval, token)` sleeping for interval after each iteration
- Wall-clock drift accumulation when iteration work takes time
- `PeriodicTimer` (introduced .NET 6) firing at fixed intervals regardless of iteration duration
- `await timer.WaitForNextTickAsync(token)` pattern for non-drifting periodic work

**Answer**

A background service loop using `await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken)` sleeps 30 seconds after each iteration completes, so if each iteration takes 5 seconds, work happens every 35 seconds — the interval drifts. `PeriodicTimer` (introduced in .NET 6) fires at fixed wall-clock intervals: `await using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30)); while (await timer.WaitForNextTickAsync(stoppingToken))` fires every 30 seconds regardless of how long each iteration takes, as long as the work completes before the next tick. Use `PeriodicTimer` for scheduling work that must happen at consistent calendar-aligned intervals; use `Task.Delay` when the interval between the end of one execution and the start of the next is what matters.

---

#### Gotcha 10. Hosted service exceptions during startup are swallowed unless re-thrown from `StartAsync`

**Concepts**
- Exception in `StartAsync` stopping that service but not necessarily others
- `ValidateOnStart` for configuration validation at startup vs in `StartAsync`
- Host failing to start if `StartAsync` throws an exception
- Dependency check logic in `StartAsync` vs `ExecuteAsync`

**Answer**

If `StartAsync` throws an exception, the host propagates it and fails to start — the process exits with an error. This is the desired behavior for true startup failures like missing critical configuration or failed database connection checks that must block app startup. However, wrapping all `StartAsync` logic in a `try/catch` that logs and swallows exceptions makes startup silently succeed with a misconfigured or non-functional background service. Distinguish between startup failures that must block the app (re-throw from `StartAsync`) and transient initial failures that the service can retry in `ExecuteAsync`. Configuration validation should use `ValidateOnStart()` in the DI options system rather than manual checks in `StartAsync`.

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
