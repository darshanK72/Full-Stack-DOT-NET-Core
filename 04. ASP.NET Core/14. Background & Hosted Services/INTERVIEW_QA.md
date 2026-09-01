# Background & Hosted Services — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 14. Background & Hosted Services](#chapter-14-background-hosted-services)
  - [Q1. What is a hosted service in ASP.NET Core?](#chapter-14-background-hosted-services-q1)
  - [Q2. What is `IHostedService`?](#chapter-14-background-hosted-services-q2)
  - [Q3. What is `BackgroundService`, and how does it differ from `IH…](#chapter-14-background-hosted-services-q3)
  - [Q4. What is the difference between `StartAsync` and `ExecuteAsyn…](#chapter-14-background-hosted-services-q4)
  - [Q5. How do you register a hosted service in DI?](#chapter-14-background-hosted-services-q5)
  - [Q6. Why can't you inject a Scoped service directly into a Single…](#chapter-14-background-hosted-services-q6)
  - [Q7. What is `IServiceScopeFactory`, and how is it used in backgr…](#chapter-14-background-hosted-services-q7)
  - [Q8. What is `Channel<T>`, and how is it used for in-process queu…](#chapter-14-background-hosted-services-q8)
  - [Q9. How does graceful shutdown work for hosted services?](#chapter-14-background-hosted-services-q9)
  - [Q10. What is the role of `CancellationToken` in `BackgroundServic…](#chapter-14-background-hosted-services-q10)
  - [Q11. What happens when Kubernetes sends SIGTERM to a pod?](#chapter-14-background-hosted-services-q11)
  - [Q12. What is `PeriodicTimer`, and when would you use it in a host…](#chapter-14-background-hosted-services-q12)
  - [Q13. What is the difference between polling and event-driven back…](#chapter-14-background-hosted-services-q13)
  - [Q14. When should background work stay in-process vs move to an ex…](#chapter-14-background-hosted-services-q14)
  - [Q15. What is the outbox pattern?](#chapter-14-background-hosted-services-q15)
  - [Q16. What problems arise from unbounded parallelism in a backgrou…](#chapter-14-background-hosted-services-q16)
  - [Q17. How does a hosted service relate to the ASP.NET Core applica…](#chapter-14-background-hosted-services-q17)
  - [Q18. What is the difference between `IHostedService` and a `Task.…](#chapter-14-background-hosted-services-q18)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 14. Background & Hosted Services

### Q1. What is a hosted service in ASP.NET Core? {#chapter-14-background-hosted-services-q1}

What is a hosted service in ASP.NET Core?

**Answer:** A hosted service is a class registered with the generic host that runs background work tied to the application lifetime, starting when the host starts and stopping gracefully when the host shuts down.

- Hosted services integrate with ASP.NET Core's `IHost` so background loops, queue consumers, and startup initialization participate in coordinated startup and shutdown rather than running as untracked threads.
- They are registered as singletons via `AddHostedService<T>()` and implement `IHostedService` or inherit `BackgroundService`.
- Examples include cache warmers, periodic sync jobs, message publishers, and in-process queue consumers.
- The same hosting model applies to worker services (`Worker` template) and web apps — background work can live alongside Kestrel in one process.

---

### Q2. What is `IHostedService`? {#chapter-14-background-hosted-services-q2}

What is `IHostedService`?

**Answer:** `IHostedService` is the interface defining `StartAsync(CancellationToken)` and `StopAsync(CancellationToken)` hooks that the host calls to begin and end background components during application startup and shutdown.

- `StartAsync` should complete quickly — the host awaits all hosted services' `StartAsync` before marking startup finished and accepting traffic.
- `StopAsync` runs during shutdown and should release resources, flush buffers, and signal long-running work to exit.
- Any class can implement `IHostedService` directly for short-lived startup/shutdown tasks without a long-running loop.
- The host manages ordering: all services start, then the web server listens; on shutdown, `StopAsync` is called with a linked cancellation token.

---

### Q3. What is `BackgroundService`, and how does it differ from `IHostedService`? {#chapter-14-background-hosted-services-q3}

What is `BackgroundService`, and how does it differ from `IHostedService`?

**Answer:** `BackgroundService` is an abstract base class implementing `IHostedService` that schedules a long-running `ExecuteAsync(CancellationToken)` loop on a background thread while returning promptly from `StartAsync`, which is the pattern most queue consumers and polling workers need.

- Raw `IHostedService` requires you to manage your own background thread or timer inside `StartAsync`/`StopAsync`.
- `BackgroundService.StartAsync` queues `ExecuteAsync` and returns immediately, so Kestrel can start listening while the loop runs.
- Override `StopAsync` to cancel the token and optionally wait for `ExecuteAsync` to observe shutdown before the process exits.
- Choose raw `IHostedService` for quick initialization hooks; choose `BackgroundService` for continuous processing until cancellation.

---

### Q4. What is the difference between `StartAsync` and `ExecuteAsync`? {#chapter-14-background-hosted-services-q4}

What is the difference between `StartAsync` and `ExecuteAsync`?

**Answer:** `StartAsync` is the host's startup hook that must finish quickly so the application can become ready, while `ExecuteAsync` (on `BackgroundService`) is where long-running loops, queue consumption, and periodic work belong.

- Blocking `StartAsync` with synchronous I/O or `.Wait()` on long tasks delays port binding and causes health/readiness probe failures in orchestrators.
- `ExecuteAsync` receives a `stoppingToken` linked to host shutdown — exit loops when cancellation is requested or when catching `OperationCanceledException` during shutdown.
- One-time initialization that takes seconds may run in `StartAsync` if bounded with a timeout, or defer to `ExecuteAsync` with a readiness gate if partial startup is acceptable.
- `IHostedService` has no `ExecuteAsync` — only `BackgroundService` provides that template method.

---

### Q5. How do you register a hosted service in DI? {#chapter-14-background-hosted-services-q5}

How do you register a hosted service in DI?

**Answer:** Call `builder.Services.AddHostedService<T>()` (or the overload accepting a factory) during service configuration, which registers the implementation as a singleton implementing both `T` and `IHostedService`.

```csharp
builder.Services.AddHostedService<EmailDispatchWorker>();
// or with factory:
builder.Services.AddHostedService(sp => new OutboxPublisher(sp.GetRequiredService<IServiceScopeFactory>()));
```

- The host resolves all `IHostedService` registrations and invokes their lifecycle methods automatically — no manual `new Thread()` is required.
- Hosted services are singletons; do not inject scoped services directly into the constructor (see Q6).
- Multiple hosted services can coexist (outbox publisher, metrics reporter, cache refresher) — each runs independently with shared shutdown semantics.
- Register supporting singletons (channels, queues) separately and inject them into the hosted service constructor.

---

### Q6. Why can't you inject a Scoped service directly into a Singleton hosted service? {#chapter-14-background-hosted-services-q6}

Why can't you inject a Scoped service directly into a Singleton hosted service?

**Answer:** Hosted services register as singletons and live for the entire application lifetime, while scoped services such as `DbContext` are created and disposed per request or per scope — injecting scoped into singleton creates a captive dependency that outlives its scope and causes `ObjectDisposedException` or stale state.

- The DI container may allow the registration at build time unless `ValidateScopes` is enabled, but the bug surfaces at runtime when the scoped instance is disposed after the first scope ends.
- EF Core `DbContext` is scoped because it tracks changes for one unit of work — reusing one instance across background jobs corrupts the change tracker and connection pooling.
- ASP.NET Core 8 can validate scope violations at startup when `ValidateScopes` and `ValidateOnBuild` are enabled in Development or staging.
- The fix is never to change `DbContext` to singleton — always open a new scope per work item (see Q7).

---

### Q7. What is `IServiceScopeFactory`, and how is it used in background work? {#chapter-14-background-hosted-services-q7}

What is `IServiceScopeFactory`, and how is it used in background work?

**Answer:** `IServiceScopeFactory` creates new dependency injection scopes on demand, allowing singleton hosted services to resolve scoped services safely by opening a scope per background job and disposing it when the job completes.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await db.SaveChangesAsync(stoppingToken);
```

- Inject `IServiceScopeFactory` (singleton-safe) into the hosted service constructor, not the scoped service itself.
- Create one scope per queue message, file, or batch unit — not one scope for the entire application lifetime.
- `CreateAsyncScope()` supports async disposal patterns and is preferred over `CreateScope()` in async workers.
- `IDbContextFactory<TContext>` is an alternative when the primary need is creating short-lived DbContext instances without a full scope.

---

### Q8. What is `Channel<T>`, and how is it used for in-process queuing? {#chapter-14-background-hosted-services-q8}

What is `Channel<T>`, and how is it used for in-process queuing?

**Answer:** `System.Threading.Channels.Channel<T>` is a thread-safe producer-consumer queue built into .NET that decouples API request threads from background processing by letting endpoints write work items and hosted services read them asynchronously.

- API handlers write to `ChannelWriter<T>` and return `202 Accepted` immediately; a `BackgroundService` reads from `ChannelReader<T>` with `ReadAllAsync(stoppingToken)`.
- `Channel.CreateBounded<T>(capacity)` applies backpressure when the queue fills — producers await space instead of growing memory without limit.
- Register the channel or a wrapper interface as a singleton shared between the endpoint and the consumer hosted service.
- Channels are in-process and not durable — restart loses unbounded in-memory items unless you persist job state in a database first.

---

### Q9. How does graceful shutdown work for hosted services? {#chapter-14-background-hosted-services-q9}

How does graceful shutdown work for hosted services?

**Answer:** When the host receives a shutdown signal, it cancels a linked `CancellationToken` passed to hosted services, calls `StopAsync` on each `IHostedService`, and waits up to `HostOptions.ShutdownTimeout` for background work to finish before the process exits.

- `BackgroundService` links cancellation to `ExecuteAsync` — loops should check `stoppingToken.IsCancellationRequested` and pass the token to I/O calls.
- Configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` to align with platform grace periods (for example Kubernetes `terminationGracePeriodSeconds`).
- `StopAsync` should stop accepting new work, drain in-flight items within the timeout, or persist incomplete jobs back to a durable store.
- Swallowing `OperationCanceledException` during shutdown is correct when the service is exiting cleanly; do not treat shutdown cancellation as an error.

---

### Q10. What is the role of `CancellationToken` in `BackgroundService`? {#chapter-14-background-hosted-services-q10}

What is the role of `CancellationToken` in `BackgroundService`?

**Answer:** The `stoppingToken` passed to `ExecuteAsync` is cancelled when the host begins shutdown, giving the background loop a signal to exit gracefully instead of being killed mid-operation.

- Pass the same token to `Task.Delay`, `PeriodicTimer.WaitForNextTickAsync`, database calls, and HTTP requests so blocked operations wake promptly on shutdown.
- When the token fires, finish the current work unit if possible, then exit the loop — do not start new long operations after cancellation.
- `HttpContext.RequestAborted` is the per-request equivalent; `stoppingToken` is the application-lifetime equivalent for background work.
- Ignoring the token causes Kubernetes SIGKILL after the grace period, often mid-transaction, leading to duplicate charges or corrupted state on restart.

---

### Q11. What happens when Kubernetes sends SIGTERM to a pod? {#chapter-14-background-hosted-services-q11}

What happens when Kubernetes sends SIGTERM to a pod?

**Answer:** Kubernetes marks the pod for termination, removes it from service endpoints, sends SIGTERM to the container process, waits for the configured grace period (default 30 seconds), then sends SIGKILL if the process is still running.

- ASP.NET Core's host translates shutdown into cancellation of hosted service tokens and `StopAsync` calls — workers must observe this to drain gracefully.
- Readiness probes fail immediately so the load balancer stops sending new traffic, but in-flight HTTP requests and background jobs may still be running.
- Set `HostOptions.ShutdownTimeout` slightly below `terminationGracePeriodSeconds` so the app exits cleanly before SIGKILL.
- Long-running jobs should checkpoint progress in durable storage so SIGKILL mid-job can resume idempotently on restart.

---

### Q12. What is `PeriodicTimer`, and when would you use it in a hosted service? {#chapter-14-background-hosted-services-q12}

What is `PeriodicTimer`, and when would you use it in a hosted service?

**Answer:** `PeriodicTimer` (.NET 6+) is an async-friendly timer that exposes `WaitForNextTickAsync(CancellationToken)`, which is ideal for polling intervals in `BackgroundService` because it integrates cleanly with shutdown cancellation unlike `Task.Delay` loops alone.

```csharp
using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    await ProcessOverdueInvoicesAsync(stoppingToken);
}
```

- Prefer it over `while(true) { await Task.Delay(...); }` because `WaitForNextTickAsync` respects timer drift and cancellation in one call.
- Use for reconciliation sweeps, cache refreshes, and health checks that poll on a fixed interval.
- Combine with idempotent database updates so overlapping ticks or restarts do not double-process records.
- For event-driven workloads with low latency requirements, `Channel<T>` or external message brokers reduce unnecessary polling load.

---

### Q13. What is the difference between polling and event-driven background processing? {#chapter-14-background-hosted-services-q13}

What is the difference between polling and event-driven background processing?

**Answer:** Polling repeatedly queries a data store or timer on a fixed interval to find work, while event-driven processing reacts to messages or API events as they occur, typically through channels or external brokers.

- Polling is simpler to implement and self-healing (missed events are picked up on the next tick) but generates steady database load even when idle.
- Event-driven processing has lower latency and less idle load but requires reliable enqueue paths and reconciliation for missed events.
- Polling suits small fleets and low-frequency tasks; event-driven suits high-throughput notifications and billing triggers.
- Many production systems use a hybrid: channels or webhooks for hot paths plus a nightly reconciliation timer for drift detection.

---

### Q14. When should background work stay in-process vs move to an external queue/broker? {#chapter-14-background-hosted-services-q14}

When should background work stay in-process vs move to an external queue/broker?

**Answer:** Keep work in-process when volume is low, durability requirements are modest, and a single instance handles the load; move to an external queue (RabbitMQ, Azure Service Bus, Hangfire with shared storage) when you need cross-instance distribution, durable retries, or isolation from the web process.

- In-process `Channel<T>` plus a hosted service is fine for fire-and-forget emails or report generation on a single node with persisted job rows.
- Multi-instance deployments need a shared queue or outbox table so any instance can pick up work — in-memory channels are invisible to other pods.
- CPU-heavy or long-running jobs should not share the web app's thread pool — offload to worker services or dedicated consumers.
- External brokers add operational complexity but provide poison-message handling, dead-letter queues, and at-least-once delivery guarantees.

---

### Q15. What is the outbox pattern? {#chapter-14-background-hosted-services-q15}

What is the outbox pattern?

**Answer:** The outbox pattern writes outbound messages or integration events to a database table in the same transaction as the business state change, then a separate publisher process reads unpublished rows and sends them to a broker, guaranteeing consistency between the database and downstream systems.

- Business code updates entities and inserts an outbox row atomically — no "database committed but message lost" race.
- A hosted service polls or leases outbox rows, publishes to the message broker, and marks rows published only after confirmed delivery (or uses a state machine: Pending → Processing → Published).
- Multi-instance publishers require row locking (`FOR UPDATE SKIP LOCKED`, `UPDLOCK`) so two workers do not publish the same message.
- Consumers must be idempotent because at-least-once delivery can produce duplicates after crashes between publish and mark-published steps.

---

### Q16. What problems arise from unbounded parallelism in a background worker? {#chapter-14-background-hosted-services-q16}

What problems arise from unbounded parallelism in a background worker?

**Answer:** Unbounded parallelism — for example `Task.WhenAll` over thousands of files or messages without a concurrency cap — exhausts thread pool threads, database connection pools, and memory, and increases duplicate processing and partial-failure corruption under load.

- Each parallel task may open a scoped `DbContext` or HTTP connection simultaneously, hitting pool limits and causing timeouts across the entire application.
- Without per-item error isolation, one failure's semantics become unclear when mixed with concurrent successes.
- Limit concurrency with `SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or bounded channel readers.
- Combine bounded parallelism with idempotency keys so retries after partial failure do not double-charge or duplicate side effects.

---

### Q17. How does a hosted service relate to the ASP.NET Core application lifetime? {#chapter-14-background-hosted-services-q17}

How does a hosted service relate to the ASP.NET Core application lifetime?

**Answer:** Hosted services start after the host builds the service provider and complete their `StartAsync` before the application is considered started, and they receive shutdown signals through the same `IHost` lifetime that stops Kestrel and disposes the root service provider.

- In a web app, Kestrel and hosted services share one process — background workers run concurrently with HTTP request handling.
- `IHostApplicationLifetime` exposes `ApplicationStarted`, `ApplicationStopping`, and `ApplicationStopped` for registering callbacks around hosted service work.
- When the root provider disposes at shutdown, singleton dependencies are disposed — another reason scoped services must not be captured for the app's entire lifetime.
- Worker Service templates use the same `IHost` without Kestrel, demonstrating that hosted services are not web-specific.

---

### Q18. What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call? {#chapter-14-background-hosted-services-q18}

What is the difference between `IHostedService` and a `Task.Run` fire-and-forget call?

**Answer:** `IHostedService` is registered with the host, participates in coordinated startup and shutdown, and receives cancellation when the application stops, whereas `Task.Run` fire-and-forget work is untracked, may outlive intended scope, and is aborted abruptly on process exit without cleanup.

- Fire-and-forget tasks started from a controller or middleware are not awaited by the host — exceptions may go unobserved and shutdown will not wait for them.
- Hosted services run under the host's exception logging and lifecycle management — `StopAsync` provides a hook to flush state.
- `Task.Run` inside a hosted service for parallel work is acceptable when bounded and awaited within `ExecuteAsync`, but not as a substitute for registering the worker itself.
- For background work in ASP.NET Core 8, always prefer `AddHostedService<T>()` over `_ = Task.Run(...)` from request handlers.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

#### Gotcha 1. Middleware order — routing before auth

**Answer:** In ASP.NET Core 8 endpoint routing, `UseRouting` must run before `UseAuthentication` and `UseAuthorization` so the auth middleware can inspect endpoint metadata — registering auth before routing breaks endpoint-aware authorization and policy resolution.

- The recommended order is exception handling → forwarded headers → routing → authentication → authorization → endpoints (`MapControllers` / `MapGet`).
- When auth runs before routing, the endpoint has not been selected yet and `[Authorize]` metadata on minimal routes or controllers may not apply correctly.
- Symptoms include anonymous access to protected endpoints or 401 responses without proper challenge behavior.
- Always verify middleware order in `Program.cs` during code review for new services.

---

#### Gotcha 2. Scoped service in a Singleton

**Answer:** Registering a scoped service such as `DbContext` into a singleton creates a captive dependency that lives for the application lifetime while the scoped instance is disposed after its first scope ends, causing stale data, thread-safety bugs, or `ObjectDisposedException`.

- The singleton holds one scoped instance forever instead of one per request — EF change trackers accumulate unrelated entities.
- Enable `ValidateScopes` in Development/staging to catch illegal scope combinations at startup.
- Fix by injecting `IServiceScopeFactory` or `IDbContextFactory<T>` and creating a scope per operation.
- This applies equally to singleton services, hosted services, and cached delegates in Minimal APIs.

---

#### Gotcha 3. `new HttpClient()` in a singleton

**Answer:** Instantiating `HttpClient` with `new` inside a long-lived singleton prevents socket reuse and causes socket exhaustion under load because each instance holds its own connection pool until garbage-collected.

- `HttpClient` is disposable but not meant for per-use disposal — `using var client = new HttpClient()` in a singleton is an anti-pattern.
- `IHttpClientFactory` manages `HttpMessageHandler` lifetimes and recycles connections correctly.
- Register named or typed clients: `builder.Services.AddHttpClient<IExternalApi, ExternalApiClient>();`
- Symptoms include `SocketException` and timeout errors only under production traffic, not in local testing.

---

#### Gotcha 4. `IOptions<T>` vs reload

**Answer:** `IOptions<T>` captures configuration snapshot at first resolution — reading `.Value` once in a singleton constructor freezes settings even when `appsettings.json` reloads with `ReloadOnChange` enabled.

- `IOptionsSnapshot<T>` recalculates per request scope; `IOptionsMonitor<T>` supports change notifications via `OnChange`.
- Singleton services must use `IOptionsMonitor<T>` or read options inside scoped operations if they need live updates.
- Misconfiguration persists silently until process restart when `.Value` was cached at construction.
- See Chapter 05 for the full options lifetime comparison.

---

#### Gotcha 5. GET with `[FromBody]`

**Answer:** Using `[FromBody]` on GET action parameters or minimal API handlers is an anti-pattern because HTTP GET semantics discourage bodies, and many clients, proxies, and caches strip or ignore GET request bodies, so binding fails silently in production.

- Query strings and route values are the correct binding sources for GET requests.
- Complex filters should use `[FromQuery]` with `[AsParameters]` or flattened query keys.
- Failures often appear only in specific browsers or CDN layers, not in Swagger "Try it out" during development.
- REST conventions expect GET to be safe and idempotent with parameters in the URL.

---

#### Gotcha 6. PascalCase JSON keys with default camelCase policy

**Answer:** ASP.NET Core 8 Web API serializes JSON with camelCase property names by default via `JsonNamingPolicy.CamelCase`, so incoming JSON with PascalCase keys (for example `"CustomerName"`) may not bind to `CustomerName` unless case-insensitive matching is enabled.

- Mobile or legacy clients sending PascalCase appear to succeed but properties remain default values (empty string, zero).
- Prefer standardizing clients on camelCase and documenting the contract in OpenAPI.
- Optional mitigation: `AddJsonOptions(o => o.JsonSerializerOptions.PropertyNameCaseInsensitive = true)` — but explicit camelCase contracts are cleaner.
- Add validation attributes so silent binding failures become 400 responses instead of corrupt data.

---

#### Gotcha 7. `throw ex` vs `throw`

**Answer:** Rethrowing with `throw ex` resets the stack trace to the catch block line, hiding the original failure location in logs and diagnostics, while bare `throw` preserves the full stack trace from where the exception was first thrown.

- Exception filters, middleware, and Application Insights rely on accurate stack traces for root-cause analysis.
- Always use `throw;` when rethrowing after logging or cleanup in a catch block.
- Wrap in a new exception only when adding context: `throw new OrderProcessingException("...", ex)` to preserve `InnerException`.
- This trap appears in both application code and background worker error handlers.

---

#### Gotcha 8. Kestrel as the only production layer

**Answer:** Running Kestrel exposed directly to the internet without a reverse proxy skips TLS termination at the edge, centralized rate limiting, WAF protection, and efficient static-file caching that production deployments typically require.

- Kestrel is production-grade as an application server but is not a full edge gateway — nginx, IIS, Azure Front Door, or AWS ALB commonly sit in front.
- TLS certificates are easier to manage at the proxy layer with automatic renewal.
- Direct exposure also complicates client IP logging unless `UseForwardedHeaders` is configured with a trusted proxy.
- Containers often bind Kestrel to port 8080 internally while the ingress controller handles HTTPS externally.

---

#### Gotcha 9. `launchSettings.json` in production

**Answer:** Settings in `Properties/launchSettings.json` — including `applicationUrl`, environment variables, and launch profiles — apply only when starting from Visual Studio, VS Code, or `dotnet run` with a profile; they are not deployed to production hosts.

- Production URLs and environment come from environment variables (`ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT`), container configuration, or IIS/nginx site settings.
- Assuming `launchSettings.json` sets Production behavior leads to wrong environment or binding in deployed environments.
- The file is development ergonomics, not runtime configuration.
- Use `appsettings.Production.json` and host-level env vars for production values.

---

#### Gotcha 10. Non-nullable `bool` for PATCH semantics

**Answer:** A non-nullable `bool` property cannot distinguish "field omitted from JSON" from "explicitly set to false" because System.Text.Json deserializes missing properties to `default(false)`, corrupting partial-update semantics.

- PATCH endpoints need `bool?`, separate update DTOs, or enums such as `Unspecified | OptIn | OptOut` for tri-state intent.
- Marketing consent and feature flags are common domains where this bug causes compliance or logic errors.
- Create DTOs may use non-nullable bool when explicit values are always required on insert.
- Document nullable fields in OpenAPI so generated clients represent optional updates correctly.

---

#### Gotcha 11. Forgetting `UseForwardedHeaders` behind a proxy

**Answer:** Without forwarded headers middleware configured with known proxy IPs, `HttpContext.Request.Scheme` remains `http`, `Request.Host` reflects the internal address, and client IP is the proxy — breaking HTTPS redirects, cookie secure flags, and audit logs.

- Call `UseForwardedHeaders()` early, before middleware that reads scheme or host (HTTPS redirection, link generation, rate limiting by IP).
- Configure `ForwardedHeadersOptions` to trust only your reverse proxy network — trusting all proxies enables header spoofing.
- Headers include `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.
- Local development without a proxy does not need this; production behind nginx/IIS/ALB does.

---

#### Gotcha 12. Static files in `wwwroot` are public

**Answer:** Any file under `wwwroot` is served by `UseStaticFiles()` to unauthenticated clients by default — placing secrets, `.env`, backup configs, or private keys there exposes them over HTTP.

- Only public assets (CSS, JS, images, public PDFs) belong in `wwwroot`.
- Sensitive configuration stays outside the web root and is loaded through `IConfiguration`, environment variables, or secret managers.
- Accidental copy of `appsettings.Production.json` into `wwwroot` is a critical security incident.
- Use build pipelines to verify web root contents before deploy.

---

#### Gotcha 13. `MapFallbackToFile` intercepting API routes

**Answer:** SPA fallback middleware registered before API endpoint mapping returns `index.html` for `/api/*` 404 responses, making API failures look like successful HTML responses to clients and breaking JSON parsers.

- Map API routes (`MapControllers`, minimal API groups) before `MapFallbackToFile("index.html")`.
- Scope fallback to non-API paths or use conditional fallback that excludes `/api` prefixes.
- Symptoms include CORS errors masked as HTML responses and Swagger fetch failures in production SPA hosting.
- Order in `Program.cs` is: API endpoints first, static files, fallback last.

---

#### Gotcha 14. Background service without scope factory

**Answer:** A singleton `BackgroundService` that injects scoped services (`DbContext`, repositories) directly into its constructor fails at startup with scope validation errors or uses disposed instances after the first background iteration.

- Hosted services live for the application lifetime — scoped dependencies must not be constructor-injected.
- Inject `IServiceScopeFactory`, create `await using var scope = factory.CreateAsyncScope()` per job, resolve scoped services inside the scope, and dispose when the job completes.
- Same rule applies to timers and `Task.Run` loops started from singletons.
- Enable `ValidateScopes` to catch this defect before production deployment.

---

#### Gotcha 15. SignalR without a backplane on multiple instances

**Answer:** SignalR broadcasts from one server instance reach only clients connected to that instance — without a Redis or Azure Service Bus backplane (or Azure SignalR Service), users on different nodes never receive each other's real-time events.

- Sticky sessions keep one client on one node but do not route events raised on other nodes to that client.
- Register `AddSignalR().AddStackExchangeRedis(...)` with a consistent channel prefix per application.
- Raw WebSocket apps need equivalent custom pub/sub — SignalR's backplane is the built-in solution.
- Test scale-out with at least two instances before launch, not single-node staging alone.

---

---

## Gotchas — ASP.NET Core (Interview Traps)

## Gotchas — ASP.NET Core (Interview Traps)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (M) A team registers both a custom `IHostedService` that only implements `StartAsync`/`StopAsync` and a `BackgroundService` subclass. When would you choose each, and what does `BackgroundService` provide on top of raw `IHostedService`?

---

**Answer:**

**Answer:** Use raw `IHostedService` for short startup/shutdown hooks (warm caches, register timers) and `BackgroundService` for long-running loops — it supplies a default `StartAsync` that schedules `ExecuteAsync` on the host's background thread and links cancellation to host shutdown.

- `IHostedService.StartAsync` should return quickly; blocking here delays Kestrel accepting traffic and health checks passing.
- `BackgroundService` overrides `ExecuteAsync(CancellationToken stoppingToken)` — the host calls it without blocking `StartAsync`, passing a token cancelled on shutdown.
- Override `StopAsync` in either type to flush buffers; `BackgroundService.StopAsync` cancels the token and optionally waits for `ExecuteAsync` to observe it.
- Choose raw `IHostedService` when integrating third-party hosted components with their own lifecycle API.
- Register with `builder.Services.AddHostedService<T>()` — singleton lifetime; never inject scoped services into the constructor (see Q2).

**Production takeaway:** The split is lifecycle placement — startup work belongs in quick `StartAsync`; unbounded loops belong in `ExecuteAsync` with cancellation, not in startup.

---

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

---

**Answer:**

**Answer:** A scoped `AppDbContext` is injected into a singleton `BackgroundService`, creating a captive dependency — the context is disposed when the root scope ends, so the second queue item touches a disposed `DbContext`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Scoped `DbContext` in singleton hosted service | Captive dependency; disposed context |
| Runtime | `ObjectDisposedException` after first SaveChanges | Intermittent email loss in prod |
| Design | Long-lived service reuses one EF context | Stale change tracker; cross-job contamination |
| Validation | Passes dev without `ValidateScopes` | Bug ships until load triggers timing |

**Fix (priority order):**

1. Remove `AppDbContext` from constructor; inject `IServiceScopeFactory` or `IDbContextFactory<AppDbContext>`.
2. Create `await using var scope = _scopeFactory.CreateAsyncScope()` **per email job** (or per batch), resolve fresh `AppDbContext`, dispose scope after send.
3. Enable `ValidateScopes` and `ValidateOnBuild` in staging to fail fast at startup.

```csharp
await using var scope = _scopeFactory.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var user = await db.Users.FindAsync(new object[] { job.UserId }, stoppingToken);
```

**Production takeaway:** See DI Module Q4 — background workers must open their own scope per unit of work; request-scoped services never belong in hosted service constructors.

---

---

#### Q3. (P) Kubernetes sends SIGTERM and gives the pod 30 seconds before SIGKILL. How should a `BackgroundService` honor graceful shutdown using `CancellationToken`, and what must you configure so in-flight HTTP calls and queue items can finish?

---

**Answer:**

**Answer:** Observe the `stoppingToken` passed to `ExecuteAsync`, pass it through to I/O and delays, avoid swallowing `OperationCanceledException`, and align `HostOptions.ShutdownTimeout` with the platform grace period so the host waits for workers before force-kill.

- `ExecuteAsync` receives a token linked to application shutdown — exit loops when `IsCancellationRequested` or catch `OperationCanceledException` when shutting down.
- Pass the same token to `ReadAllAsync`, `HttpClient` calls, and `Task.Delay` so blocked operations wake promptly on SIGTERM.
- Configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` — slightly under K8s `terminationGracePeriodSeconds` to allow flush logging.
- For in-flight queue items: stop accepting new work in `StopAsync`, drain with a timeout, or mark items back to pending in DB if shutdown exceeds grace.
- `IHostApplicationLifetime.ApplicationStopping` registers last-chance callbacks for metrics and leader election release.

**Production takeaway:** Ignoring `stoppingToken` causes mid-transaction kills — data corruption and duplicate charges on restart are common postmortem themes.

---

---

#### Q4. (P) You need to offload heavy report generation from API requests without losing work on restart. Describe the in-process queue pattern with `Channel<T>` and a hosted consumer — what belongs in the API vs the worker, and when do you outgrow in-process queues?

---

**Answer:**

**Answer:** The API enqueues a durable job id into `Channel<T>` (or writes to an outbox table first), returns `202 Accepted` immediately, and a singleton `BackgroundService` reads the channel and processes with a per-job DI scope — outgrow this when you need cross-process durability, horizontal workers, or poison-message handling.

- API endpoint: validate request, persist job row (`Status = Queued`), optionally write to channel, return location URL for status polling.
- Consumer: `BackgroundService` with `Channel.CreateBounded<T>` (backpressure) or unbounded (risk OOM); `ReadAllAsync(stoppingToken)` loop.
- Per job: `CreateAsyncScope()`, resolve services, update job status, store artifact path, handle failures with retry count.
- Register channel writer as singleton service shared between API and hosted service, or use `System.Threading.Channels` wrapped in `IReportQueue`.
- Outgrow in-process when: multi-instance deployments (each pod has its own channel), restart loses unbounded in-memory items, or CPU-heavy jobs starve HTTP threads — move to RabbitMQ, Azure Service Bus, or Hangfire with shared storage.

**Production takeaway:** Channels decouple latency from the request thread but are not durable — pair with DB outbox or external broker for anything financial or compliance-bound.

---

---

#### Q5. (D) Two designs poll for overdue invoices: (A) `PeriodicTimer` in `ExecuteAsync` with a DB query every minute, (B) `Channel<T>` fed by API events plus a nightly reconciliation sweep. Compare throughput, duplicate processing risk, and shutdown behavior.

---

**Answer:**

**Answer:** Event-driven `Channel` processing reduces idle DB load and latency for new overdue items but requires reconciliation for missed events; `PeriodicTimer` polling is simpler and self-healing but hammers the database and extends shutdown until the next tick completes.

| Dimension | (A) PeriodicTimer poll | (B) Channel + reconciliation |
|---|---|---|
| Latency | Up to one poll interval | Near real-time on event |
| DB load | Fixed query cost every tick | Queries only on events + nightly sweep |
| Duplicates | Idempotent update SQL mitigates | Must handle duplicate enqueue + at-least-once delivery |
| Shutdown | Wait for in-flight tick; cancel timer promptly | Stop reader; drain or requeue channel items |
| Complexity | Low; good for small fleets | Higher; needs outbox/reconciliation |

- Use `PeriodicTimer` with `WaitForNextTickAsync(stoppingToken)` instead of `Task.Delay` loops — cleaner cancellation.
- Channel design should mark invoices `Processing` with lease timestamps to avoid double billing across instances.
- Hybrid: channel for hot path, timer reconciliation for drift detection — common in billing systems.

**Production takeaway:** Karat expects trade-off articulation — polling is not "wrong," but it fails cost and latency reviews at scale without idempotent SQL.

---

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

---

**Answer:**

**Answer:** The service runs unbounded parallel file processing with a captive scoped billing service in a singleton host, deleting files before durable commit and ignoring partial failures — exhausting the connection pool and allowing double charges on retry.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| DI lifetime | Scoped `_billing` captured by singleton | Captive `DbContext`; pool exhaustion |
| Concurrency | `Task.WhenAll` on all files — unbounded | Thread and SQL connection starvation |
| Correctness | `File.Delete` before durable idempotency proof | Lost files on crash; re-drop causes double charge |
| Resilience | No per-file try/catch or move to `/processed` | One failure aborts entire batch semantics unclear |
| Shutdown | No `stoppingToken` passed to `ChargeAsync` | Mid-charge kill → ambiguous state |

**Fix (priority order):**

1. Inject `IServiceScopeFactory`; create one scope **per file** (or bounded `Parallel.ForEachAsync` with max degree).
2. Limit parallelism: `SemaphoreSlim` or `Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = stoppingToken }, ...)`.
3. Move file to `processing/` then `completed/` only after successful commit; use idempotency keys in `ChargeAsync`.
4. Log and quarantine poison files instead of deleting on first attempt.

**Production takeaway:** Background parallelism without bounds is a classic production incident pattern — always cap concurrency and scope database work per item.

---

---

#### Q7. (M) `IHostedService.StartAsync` runs during host startup. What happens if `StartAsync` blocks on long synchronous work or awaits a never-completing task, and how does that differ from work placed in `BackgroundService.ExecuteAsync`?

---

**Answer:**

**Answer:** Blocking `StartAsync` delays the entire host startup — health probes fail, orchestrator restarts the pod, and dependent services never bind — whereas `BackgroundService` schedules long work off the startup path via `ExecuteAsync`, allowing Kestrel to start while the loop runs.

- Host awaits all `IHostedService.StartAsync` before marking startup complete — synchronous CPU work or `.Wait()` on long tasks blocks here.
- Awaiting a never-completing task in `StartAsync` deadlocks startup entirely — the process hangs before listening on ports.
- `BackgroundService.StartAsync` returns after queuing `ExecuteAsync` — appropriate for polling loops and queue consumers.
- Heavy initialization (load large cache) should either complete quickly in `StartAsync` with timeout or run in `ExecuteAsync` with readiness gate if partial startup is acceptable.
- Use `IHostApplicationLifetime.ApplicationStarted` to defer work until after the server is listening.

**Production takeaway:** Readiness probe failures during deploy often trace to hosted services doing minutes of work in `StartAsync` instead of `ExecuteAsync`.

---

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

**Answer:**

**Answer:** The publisher marks rows published in the same transaction as a fire-and-forget publish without handling broker failures, runs a tight loop without backoff, and lacks idempotent consumer semantics — so exceptions or shutdown after `SaveChanges` orphan messages while duplicates are possible on retry.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Set `Published = true` before confirmed broker ack | Data loss if publish throws after save |
| Resilience | No try/catch per message; no retry/backoff | Tight loop hammers DB and broker on outage |
| Shutdown | No delay; ignores batch failure isolation | Partial batch saved; inconsistent broker state |
| Idempotency | Re-publish after crash duplicates downstream | Double side effects without dedup keys |
| Ordering | `Take(50)` without row locking | Two instances publish same rows |

**Fix (priority order):**

1. Publish first or use two-phase: `Processing` lease column, publish, then `Published` in same scope with transaction.
2. Wrap each message in try/catch; increment `AttemptCount`; dead-letter after threshold.
3. Add `await Task.Delay(pollInterval, stoppingToken)` or use `PeriodicTimer` between batches.
4. Use `UPDLOCK`/`SKIP LOCKED` (SQL Server) or `FOR UPDATE SKIP LOCKED` (PostgreSQL) for multi-instance safety.
5. Pass message id as idempotency key to broker consumers.

**Production takeaway:** Outbox pattern fails on "update DB then hope publish works" — atomic handoff or explicit state machine beats a boolean `Published` flag.

---
