# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/01. Threads & Thread Lifecycle/`

---

#### Q1. (R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:

```csharp
public static void Main()
{
    using var cts = new CancellationTokenSource();
    var labelThread = new Thread(() => PrintLabelsLoop(cts.Token));
    labelThread.Name = "LabelPrinter";
    labelThread.Start();

    Console.WriteLine("Press Enter to stop…");
    Console.ReadLine();
    cts.Cancel();
    // expects immediate exit after Enter
}

static void PrintLabelsLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        Thread.Sleep(500);
        Console.WriteLine("[LabelPrinter] stamp…");
    }
}
```

What keeps the process alive, and how do you fix shutdown so cancellation is honored cleanly?

**Answer:** `CancellationToken` only sets a flag — it does not terminate the thread. The label thread defaults to **foreground** (`IsBackground == false`), so the CLR keeps the process alive until that thread's delegate finishes. Main exits after `Cancel()` without `Join`, but the foreground worker may still be inside `Thread.Sleep(500)` before it observes cancellation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifecycle | Foreground thread not joined on shutdown | Process appears hung until worker loop exits on its own |
| Threading | `Cancel()` without waiting for cooperative exit | Operator thinks shutdown failed; orphaned work may continue briefly |
| Design | Long `Sleep` between cancellation checks | Up to 500 ms (or full sleep window) delay before loop observes token |

**Fix (priority order):**

1. After `cts.Cancel()`, call `labelThread.Join()` (optionally `Join(TimeSpan.FromSeconds(30))` and log if timeout).
2. Keep cooperative cancellation in the loop — check `token.IsCancellationRequested` and exit cleanly (matches **Program.cs** Section 8 — `RunInventorySweep`).
3. For daemon-style helpers that must not block process exit, set `labelThread.IsBackground = true` **only** when you accept abrupt termination without guaranteed cleanup — still prefer `Join` for graceful shutdown.
4. Shorten blocking intervals or use `token.WaitHandle.WaitOne(100)` so cancellation is observed faster between iterations.

```csharp
cts.Cancel();
if (!labelThread.Join(TimeSpan.FromSeconds(30)))
    Console.Error.WriteLine("LabelPrinter did not stop in time.");
```

**Production takeaway:** Foreground vs background and `Join` control process lifetime — cancellation alone is not shutdown. See **Program.cs** Sections 6–7 (foreground/background, Join) and Section 8 (cooperative termination).

---

#### Q2. (R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:

```csharp
private static int _packagesProcessed;
private static readonly List<ShipmentResult> _completed = new();

public static void ProcessShipment(object? state)
{
    var work = (ShipmentWork)state!;
    int boxesDone = 0;

    for (int box = 1; box <= work.BoxCount; box++)
    {
        Thread.Sleep(work.MillisecondsPerBox);
        _packagesProcessed++;          // no lock
        boxesDone++;
    }

    _completed.Add(new ShipmentResult(
        work.ShipmentId, work.Destination, boxesDone,
        0, Thread.CurrentThread.ManagedThreadId, ""));

    Console.WriteLine($"[{Thread.CurrentThread.Name}] done {work.ShipmentId}");
}

// Main starts three ParameterizedThreadStart workers concurrently on shared static fields.
```

What fails in production, and what is the prioritized fix?

**Answer:** Multiple workers perform unsynchronized read-modify-write on `_packagesProcessed` and concurrent `List<T>.Add` calls. The tally loses increments (classic lost update), and the list can corrupt internal state or throw — intermittent failures that pass single-threaded demos.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | `_packagesProcessed++` without synchronization | Lost updates; reported total < actual boxes scanned |
| Runtime / correctness | `_completed.Add` from multiple threads | `ArgumentException`, torn internal array, or dropped entries |
| Design | Shared mutable statics across ParameterizedThreadStart workers | Race surface on every concurrent worker |

**Fix (priority order):**

1. Protect shared mutations with one lock object (same pattern as **Program.cs** `TallyLock` around increment and `CompletedShipments.Add`).
2. Prefer `Interlocked.Increment(ref _packagesProcessed)` for the counter if it is the only numeric shared state — still lock the list or use a thread-safe collection.
3. For production aggregation, consider `ConcurrentBag<ShipmentResult>` or per-worker local results merged after `Join` — eliminates lock contention on hot paths.
4. Re-run under parallel workers with stress timing (reduce `Sleep`) to reproduce before/after fix.

```csharp
lock (TallyLock)
{
    _packagesProcessed++;
}
// …
lock (TallyLock)
{
    _completed.Add(result);
}
```

**Production takeaway:** Threads communicate through shared memory — the chapter's lock preview exists because this bug ships silently until concurrency rises. Full treatment → **06. Synchronization and Locks**.

---

#### Q3. (R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:

```csharp
ShipmentWork[] pending =
[
    new("SH-1001", "North", 3, 50),
    new("SH-1002", "South", 2, 50),
    new("SH-1003", "East", 4, 50),
];

var threads = new Thread[pending.Length];
for (int i = 0; i < pending.Length; i++)
{
    threads[i] = new Thread(() => ProcessShipment(pending[i]));
    threads[i].Name = $"Worker-{pending[i].ShipmentId}";
    threads[i].Start();
}

foreach (var t in threads) t.Join();
```

Why does every thread process the last shipment, and how do you fix it without changing the worker signature?

**Answer:** The lambda closes over the loop variable `i`, not the value at iteration time. All threads may start after the loop finishes, so `pending[i]` resolves to the last index for every delegate — a closure capture bug unrelated to `ParameterizedThreadStart` itself.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | Closure captures mutable loop variable `i` | Every worker receives `pending[2]` / `SH-1003` |
| Design | Lambda + loop index instead of per-iteration capture | Wrong work dispatched; duplicate processing and skipped shipments |
| Maintainability | `Name` uses `pending[i]` at Start time — may also show wrong id if timing differs | Misleading diagnostics in Threads window |

**Fix (priority order):**

1. Capture a per-iteration local: `var work = pending[i];` then `new Thread(() => ProcessShipment(work))`.
2. Or use `ParameterizedThreadStart` directly: `new Thread(ProcessShipment)` and `worker.Start(pending[i])` — passes state at start, no closure over `i` (**Program.cs** Section 6–7 pattern).
3. Add a unit/integration test that asserts three distinct `ShipmentId` values in results after parallel start.

```csharp
for (int i = 0; i < pending.Length; i++)
{
    ShipmentWork work = pending[i];
    threads[i] = new Thread(() => ProcessShipment(work));
    threads[i].Start();
}
```

**Production takeaway:** Karat stacks threading with C# closure semantics — `ParameterizedThreadStart` + `Start(state)` is the idiomatic way to pass work and avoids loop capture entirely.

---

#### Q4. (P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?

**Answer:** Use **cooperative cancellation** with `CancellationToken` linked to the service's `IHostApplicationLifetime.ApplicationStopping` (or a `CancellationTokenSource` cancelled in `StopAsync`). The worker checks `IsCancellationRequested` (or `ThrowIfCancellationRequested`) in its loop, finishes the current aisle/unit of work if needed, releases locks and handles, and exits the delegate normally — then the host `Join`s the thread or awaits a `Task` wrapper.

- Never call `Thread.Abort` — removed from .NET Core because it could leave locks held and invariants broken mid-method (**Program.cs** Section 8).
- Pass the token into the worker at construction/start; cancel once from the shutdown path; block shutdown on `Join(timeout)` and log if the worker exceeds the SLA.
- Keep loop body idempotent at cancellation boundaries — persist checkpoint if stopping mid-batch matters for ops.
- For I/O-bound sweeps, prefer `async`/`await` with the same token (later chapter) so threads are not blocked in `Sleep`.

**Production takeaway:** Production shutdown is "signal, wait, log timeout" — not force-terminate. Dedicated `Thread` is acceptable for a long-lived CPU worker when lifecycle and join semantics are explicit.

---

#### Q5. (M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?

```csharp
foreach (Thread worker in workers)
{
    while (worker.IsAlive)
    {
        Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
        if (!worker.Join(millisecondsTimeout: 100))
            continue;
    }
    Console.WriteLine($"  {worker.Name} finished.");
}
```

**Answer:** This is not a correctness bug — it is a **busy-polling** wait pattern. `Join(100)` returns `false` every 100 ms while the worker runs, so the loop spins and floods logs under load. Functionally, the thread eventually completes; operationally, you waste CPU and drown observability.

- Prefer a single blocking `worker.Join()` per worker when you simply need to wait until done (**Program.cs** Section 7).
- Use `Join(TimeSpan)` once when you need a timeout — handle `false` as SLA breach, do not spin in a tight loop unless you must interleave other work.
- If Main must pump progress UI or heartbeats while waiting, use `Join(100)` **without** logging every iteration — log on interval or on state change.
- For many workers, `Task.Run` + `Task.WhenAll` or `Parallel.Invoke` gives clearer composition than manual `IsAlive` polling (later chapters).

**Production takeaway:** The chapter demo uses polling to **teach** `IsAlive` and timed `Join` — production code should block once or wait on a `CountdownEvent`/`Task`, not hot-loop status checks.

---

#### Q6. (D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?

**Answer:** Unbounded `new Thread` per shipment exhausts OS thread limits and memory (default stack reserve per thread), thrashes the scheduler, and makes shutdown join storms impossible. `ThreadPriority` is an OS hint, not a SLA — express lanes are not reliably prioritized across machines. `[ThreadStatic]` metrics break when work moves to thread pool threads or `async` continuations hop threads — counters attach to threads, not logical shipments.

- Cap concurrency: fixed pool of long-lived worker threads **or** `ThreadPool` / `Task` with a bounded `SemaphoreSlim` (e.g., max 8 scanners) — same lifecycle idea (start work, join/complete, cooperative stop) without 1:1 OS threads.
- Express handling belongs in queue priority or business rules, not `ThreadPriority.AboveNormal`.
- Export metrics with labels (`shipment_id`, `worker_id`) via `IMeterFactory`/Prometheus counters — not `[ThreadStatic]` tallies.
- Keep `CancellationToken` on the batch host so service shutdown still cooperates (**Section 8** pattern).
- CPU-bound parallel loops → **05. Parallel Programming**; I/O-bound waits → **04. Async and Await**.

**Production takeaway:** This chapter teaches manual threads for **lifecycle literacy** — production scales with bounded pools, tokens, and synchronized shared state, not unbounded `Thread` construction.

---

#### Q7. (R) A retry path tries to restart workers after a transient fault. Review:

```csharp
Thread worker = new Thread(ProcessShipment);
worker.Name = "Worker-Retry";
worker.Start(shipment);

worker.Join();
if (shipment.NeedsRetry)
{
    worker.Start(shipment);   // "restart same thread"
    worker.Join();
}
```

What fails at runtime, and what is the correct lifecycle approach?

**Answer:** A `Thread` instance is **one-shot**. After the delegate completes, `ThreadState` is `Stopped` and calling `Start()` again throws `ThreadStateException` ("Thread is dead; it cannot be started"). You cannot restart the same `Thread` object.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Second `Start()` on completed thread | `ThreadStateException`; retry path never runs |
| Lifecycle | Assumes thread is reusable like a process pool worker | Violates **Program.cs** lifecycle table — Stopped threads cannot restart |
| Design | Retry logic coupled to dead thread instance | Transient faults appear as hard failures |

**Fix (priority order):**

1. Create a **new** `Thread` (or new `Task`) for each retry attempt: `worker = new Thread(ProcessShipment); worker.Start(shipment);`
2. Better: one worker loop that reads from a `BlockingCollection<ShipmentWork>` or channel and retries internally — single thread lifecycle, many shipments.
3. Best at scale: queue work to `ThreadPool` / `Task` with retry policy (`Polly`) — no manual thread reuse semantics to get wrong.
4. Guard retry with max attempts and log `ManagedThreadId` per attempt for correlation.

```csharp
for (int attempt = 1; attempt <= maxAttempts && shipment.NeedsRetry; attempt++)
{
    var worker = new Thread(ProcessShipment) { Name = $"Worker-Retry-{attempt}" };
    worker.Start(shipment);
    worker.Join();
}
```

**Production takeaway:** Thread lifecycle is construct → start once → join → discard. Retries mean new execution contexts, not `Start()` on a stopped instance — a common Karat trap after reading demo code.
