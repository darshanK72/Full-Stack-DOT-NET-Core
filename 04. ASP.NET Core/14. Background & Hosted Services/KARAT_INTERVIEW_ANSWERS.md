# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_ANSWERS.md).

> **Folder:** `05. ASP.NET Core/14. Background & Hosted Services`

---

#### Q1. (M) A team registers both a custom `IHostedService` that only implements `StartAsync`/`StopAsync` and a `BackgroundService` subclass. When would you choose each, and what does `BackgroundService` provide on top of raw `IHostedService`?

**Answer:** Use raw `IHostedService` for short startup/shutdown hooks (warm caches, register timers) and `BackgroundService` for long-running loops — it supplies a default `StartAsync` that schedules `ExecuteAsync` on the host's background thread and links cancellation to host shutdown.

- `IHostedService.StartAsync` should return quickly; blocking here delays Kestrel accepting traffic and health checks passing.
- `BackgroundService` overrides `ExecuteAsync(CancellationToken stoppingToken)` — the host calls it without blocking `StartAsync`, passing a token cancelled on shutdown.
- Override `StopAsync` in either type to flush buffers; `BackgroundService.StopAsync` cancels the token and optionally waits for `ExecuteAsync` to observe it.
- Choose raw `IHostedService` when integrating third-party hosted components with their own lifecycle API.
- Register with `builder.Services.AddHostedService<T>()` — singleton lifetime; never inject scoped services into the constructor (see Q2).

**Production takeaway:** The split is lifecycle placement — startup work belongs in quick `StartAsync`; unbounded loops belong in `ExecuteAsync` with cancellation, not in startup.

---

#### Q2. (R) Review this email-dispatch hosted service. The app starts cleanly in Development but throws `ObjectDisposedException` on the second queued email in Production. What is wrong?

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

#### Q3. (P) Kubernetes sends SIGTERM and gives the pod 30 seconds before SIGKILL. How should a `BackgroundService` honor graceful shutdown using `CancellationToken`, and what must you configure so in-flight HTTP calls and queue items can finish?

**Answer:** Observe the `stoppingToken` passed to `ExecuteAsync`, pass it through to I/O and delays, avoid swallowing `OperationCanceledException`, and align `HostOptions.ShutdownTimeout` with the platform grace period so the host waits for workers before force-kill.

- `ExecuteAsync` receives a token linked to application shutdown — exit loops when `IsCancellationRequested` or catch `OperationCanceledException` when shutting down.
- Pass the same token to `ReadAllAsync`, `HttpClient` calls, and `Task.Delay` so blocked operations wake promptly on SIGTERM.
- Configure `builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(25))` — slightly under K8s `terminationGracePeriodSeconds` to allow flush logging.
- For in-flight queue items: stop accepting new work in `StopAsync`, drain with a timeout, or mark items back to pending in DB if shutdown exceeds grace.
- `IHostApplicationLifetime.ApplicationStopping` registers last-chance callbacks for metrics and leader election release.

**Production takeaway:** Ignoring `stoppingToken` causes mid-transaction kills — data corruption and duplicate charges on restart are common postmortem themes.

---

#### Q4. (P) You need to offload heavy report generation from API requests without losing work on restart. Describe the in-process queue pattern with `Channel<T>` and a hosted consumer — what belongs in the API vs the worker, and when do you outgrow in-process queues?

**Answer:** The API enqueues a durable job id into `Channel<T>` (or writes to an outbox table first), returns `202 Accepted` immediately, and a singleton `BackgroundService` reads the channel and processes with a per-job DI scope — outgrow this when you need cross-process durability, horizontal workers, or poison-message handling.

- API endpoint: validate request, persist job row (`Status = Queued`), optionally write to channel, return location URL for status polling.
- Consumer: `BackgroundService` with `Channel.CreateBounded<T>` (backpressure) or unbounded (risk OOM); `ReadAllAsync(stoppingToken)` loop.
- Per job: `CreateAsyncScope()`, resolve services, update job status, store artifact path, handle failures with retry count.
- Register channel writer as singleton service shared between API and hosted service, or use `System.Threading.Channels` wrapped in `IReportQueue`.
- Outgrow in-process when: multi-instance deployments (each pod has its own channel), restart loses unbounded in-memory items, or CPU-heavy jobs starve HTTP threads — move to RabbitMQ, Azure Service Bus, or Hangfire with shared storage.

**Production takeaway:** Channels decouple latency from the request thread but are not durable — pair with DB outbox or external broker for anything financial or compliance-bound.

---

#### Q5. (D) Two designs poll for overdue invoices: (A) `PeriodicTimer` in `ExecuteAsync` with a DB query every minute, (B) `Channel<T>` fed by API events plus a nightly reconciliation sweep. Compare throughput, duplicate processing risk, and shutdown behavior.

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

#### Q6. (R) Review this `BackgroundService` that processes files in parallel. Under load, SQL connection pool exhaustion and duplicate charges appear. What are the defects?

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

#### Q7. (M) `IHostedService.StartAsync` runs during host startup. What happens if `StartAsync` blocks on long synchronous work or awaits a never-completing task, and how does that differ from work placed in `BackgroundService.ExecuteAsync`?

**Answer:** Blocking `StartAsync` delays the entire host startup — health probes fail, orchestrator restarts the pod, and dependent services never bind — whereas `BackgroundService` schedules long work off the startup path via `ExecuteAsync`, allowing Kestrel to start while the loop runs.

- Host awaits all `IHostedService.StartAsync` before marking startup complete — synchronous CPU work or `.Wait()` on long tasks blocks here.
- Awaiting a never-completing task in `StartAsync` deadlocks startup entirely — the process hangs before listening on ports.
- `BackgroundService.StartAsync` returns after queuing `ExecuteAsync` — appropriate for polling loops and queue consumers.
- Heavy initialization (load large cache) should either complete quickly in `StartAsync` with timeout or run in `ExecuteAsync` with readiness gate if partial startup is acceptable.
- Use `IHostApplicationLifetime.ApplicationStarted` to defer work until after the server is listening.

**Production takeaway:** Readiness probe failures during deploy often trace to hosted services doing minutes of work in `StartAsync` instead of `ExecuteAsync`.

---

#### Q8. (R) This outbox publisher runs as a hosted service. Messages are marked published in the database but never reach the broker after deploys. Review the loop.

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
