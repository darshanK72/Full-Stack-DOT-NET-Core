# 06. Multithreading & Async Programming — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Threads & Thread Lifecycle](#01-threads-thread-lifecycle)
  - [Q1. Explain multithreading in C# and when it is appropriate vs a…](#01-threads-thread-lifecycle-q1)
  - [Q2. What is a `Thread`, and how do you create and start one?](#01-threads-thread-lifecycle-q2)
  - [Q3. What are foreground vs background threads, and how do they a…](#01-threads-thread-lifecycle-q3)
  - [Q4. What are the main thread states in the lifecycle (unstarted,…](#01-threads-thread-lifecycle-q4)
  - [Q5. What is `Thread.Join()`, and what happens if you never join …](#01-threads-thread-lifecycle-q5)
  - [Q6. What is `Thread.Sleep()` vs spinning vs waiting — when is ea…](#01-threads-thread-lifecycle-q6)
  - [Q7. What is thread affinity, and why does it matter for UI appli…](#01-threads-thread-lifecycle-q7)
  - [Q8. What is the difference between creating a raw `Thread` and u…](#01-threads-thread-lifecycle-q8)
  - [Q9. What are `Thread.Name`, `IsBackground`, `Priority` — which a…](#01-threads-thread-lifecycle-q9)
  - [Q10. What is a race condition at the thread level, and how can tw…](#01-threads-thread-lifecycle-q10)
  - [Q11. What is the difference between kernel threads and managed th…](#01-threads-thread-lifecycle-q11)
  - [Q12. Why is manually creating many threads often a scalability an…](#01-threads-thread-lifecycle-q12)
  - [Q13. What is `ThreadStatic`, and how does it differ from `ThreadL…](#01-threads-thread-lifecycle-q13)
  - [Q14. What exceptions can occur when aborting or interrupting thre…](#01-threads-thread-lifecycle-q14)
  - [Q15. How does the main thread exiting affect background work stil…](#01-threads-thread-lifecycle-q15)

- [02. ThreadPool](#02-threadpool)
  - [Q1. What is the thread pool in .NET, and why is it preferred ove…](#02-threadpool-q1)
  - [Q2. How does the thread pool manage worker threads and I/O compl…](#02-threadpool-q2)
  - [Q3. What is hill-climbing in the .NET thread pool (high level)?](#02-threadpool-q3)
  - [Q4. What is `ThreadPool.QueueUserWorkItem`, and how does it rela…](#02-threadpool-q4)
  - [Q5. What is starvation in the thread pool, and what causes it?](#02-threadpool-q5)
  - [Q6. How do synchronous blocking calls inside pool threads affect…](#02-threadpool-q6)
  - [Q7. What is the difference between dedicated threads and pool th…](#02-threadpool-q7)
  - [Q8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and wh…](#02-threadpool-q8)
  - [Q9. How does the thread pool interact with `async`/`await` conti…](#02-threadpool-q9)
  - [Q10. What is the danger of blocking the UI thread vs blocking a p…](#02-threadpool-q10)
  - [Q11. How do thread pool threads relate to `Parallel.For` and PLIN…](#02-threadpool-q11)
  - [Q12. What diagnostics exist for thread pool queue length and thre…](#02-threadpool-q12)

- [03. Tasks & Task Parallel Library](#03-tasks-task-parallel-library)
  - [Q1. What is the Task Parallel Library (TPL)?](#03-tasks-task-parallel-library-q1)
  - [Q2. Explain the difference between `Thread` and `Task` in purpos…](#03-tasks-task-parallel-library-q2)
  - [Q3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use …](#03-tasks-task-parallel-library-q3)
  - [Q4. What is `Task.Run`, and when should it be used vs when it sh…](#03-tasks-task-parallel-library-q4)
  - [Q5. What is `Task.Factory.StartNew`, and why is `Task.Run` usual…](#03-tasks-task-parallel-library-q5)
  - [Q6. Explain task continuations with `ContinueWith` — options, sc…](#03-tasks-task-parallel-library-q6)
  - [Q7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they diff…](#03-tasks-task-parallel-library-q7)
  - [Q8. What is `TaskCompletionSource<T>`, and what scenarios does i…](#03-tasks-task-parallel-library-q8)
  - [Q9. What is the difference between completing a `TaskCompletionS…](#03-tasks-task-parallel-library-q9)
  - [Q10. What is `Task.FromResult`, `Task.CompletedTask`, and when ar…](#03-tasks-task-parallel-library-q10)
  - [Q11. What is the difference between `AggregateException` and a re…](#03-tasks-task-parallel-library-q11)
  - [Q12. How do child tasks relate to parent tasks (`TaskCreationOpti…](#03-tasks-task-parallel-library-q12)
  - [Q13. What is task cancellation via `CancellationToken` registrati…](#03-tasks-task-parallel-library-q13)
  - [Q14. What are unobserved task exceptions, and how does .NET handl…](#03-tasks-task-parallel-library-q14)
  - [Q15. What is `ValueTask` pooling/caching, and why must consumers …](#03-tasks-task-parallel-library-q15)
  - [Q16. How do you implement a timeout around a `Task` using `Cancel…](#03-tasks-task-parallel-library-q16)

- [04. Async and Await](#04-async-and-await)
  - [Q1. Explain asynchronous programming in C# — what problem does i…](#04-async-and-await-q1)
  - [Q2. Explain the `async` and `await` keywords in detail.](#04-async-and-await-q2)
  - [Q3. What is the difference between CPU-bound and I/O-bound async…](#04-async-and-await-q3)
  - [Q4. What is `ConfigureAwait(false)`, and when should library vs …](#04-async-and-await-q4)
  - [Q5. How do you handle exceptions in async/await methods?](#04-async-and-await-q5)
  - [Q6. What is the difference between `async void`, `async Task`, a…](#04-async-and-await-q6)
  - [Q7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, an…](#04-async-and-await-q7)
  - [Q8. How does the async state machine work under the hood (high l…](#04-async-and-await-q8)
  - [Q9. What is synchronization context, and how does it affect cont…](#04-async-and-await-q9)
  - [Q10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()…](#04-async-and-await-q10)
  - [Q11. What is the difference between `await task` and `return task…](#04-async-and-await-q11)
  - [Q12. How do you implement retry with exponential backoff in async…](#04-async-and-await-q12)
  - [Q13. What is jitter in backoff strategies, and why is it used?](#04-async-and-await-q13)
  - [Q14. How do Polly-style resilience policies relate to manual retr…](#04-async-and-await-q14)
  - [Q15. What is `CancellationTokenSource.CreateLinkedTokenSource`, a…](#04-async-and-await-q15)
  - [Q16. How do you propagate cancellation through layered async APIs…](#04-async-and-await-q16)
  - [Q17. What is `Task.Delay` vs `Thread.Sleep` in async methods?](#04-async-and-await-q17)
  - [Q18. What is "async all the way" — why is mixing blocking and asy…](#04-async-and-await-q18)
  - [Q19. How do you unit test async methods and time-dependent retry …](#04-async-and-await-q19)
  - [Q20. What is `IAsyncDisposable`, and how does `await using` work?](#04-async-and-await-q20)

- [05. Parallel Programming](#05-parallel-programming)
  - [Q1. What is `Parallel.For` and `Parallel.ForEach`?](#05-parallel-programming-q1)
  - [Q2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `Cancel…](#05-parallel-programming-q2)
  - [Q3. What is a `Partitioner<TSource>`, and when would you supply …](#05-parallel-programming-q3)
  - [Q4. What is the difference between range partitioning and chunk …](#05-parallel-programming-q4)
  - [Q5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `Wit…](#05-parallel-programming-q5)
  - [Q6. When is parallelization slower than sequential execution?](#05-parallel-programming-q6)
  - [Q7. What types of workloads benefit from PLINQ vs `Parallel.ForE…](#05-parallel-programming-q7)
  - [Q8. What are thread-safe requirements when using parallel loops …](#05-parallel-programming-q8)
  - [Q9. How do you perform parallel aggregation with `lock`, `Interl…](#05-parallel-programming-q9)
  - [Q10. What is `ParallelLoopResult`, and how do you detect partial …](#05-parallel-programming-q10)
  - [Q11. What are ordering guarantees in PLINQ (`AsOrdered`) and thei…](#05-parallel-programming-q11)
  - [Q12. How does parallel LINQ decide default partition sizes?](#05-parallel-programming-q12)
  - [Q13. What exceptions are thrown from parallel loops (`AggregateEx…](#05-parallel-programming-q13)
  - [Q14. How do you combine async I/O with parallel CPU work without …](#05-parallel-programming-q14)
  - [Q15. What are best practices for parallel and async code in serve…](#05-parallel-programming-q15)

- [06. Synchronization and Locks](#06-synchronization-and-locks)
  - [Q1. Explain synchronization primitives: `lock`, `Monitor`, `Mute…](#06-synchronization-and-locks-q1)
  - [Q2. What is `ReaderWriterLockSlim`, and when is it preferable to…](#06-synchronization-and-locks-q2)
  - [Q3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualRes…](#06-synchronization-and-locks-q3)
  - [Q4. What is `CancellationToken`, and how do you implement cooper…](#06-synchronization-and-locks-q4)
  - [Q5. Explain deadlocks in multithreading — necessary conditions a…](#06-synchronization-and-locks-q5)
  - [Q6. What are race conditions, and how can they be prevented?](#06-synchronization-and-locks-q6)
  - [Q7. What is the `volatile` keyword, and when does it provide vis…](#06-synchronization-and-locks-q7)
  - [Q8. What is the difference between `volatile` and `lock` for thr…](#06-synchronization-and-locks-q8)
  - [Q9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`…](#06-synchronization-and-locks-q9)
  - [Q10. What is `SpinLock`, and when might low-latency spinning beat…](#06-synchronization-and-locks-q10)
  - [Q11. What is lock ordering, and how does it prevent deadlock?](#06-synchronization-and-locks-q11)
  - [Q12. What is the `Monitor.TryEnter` pattern, and how do timeouts …](#06-synchronization-and-locks-q12)
  - [Q13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`)…](#06-synchronization-and-locks-q13)
  - [Q14. What is a priority inversion problem (conceptual), and which…](#06-synchronization-and-locks-q14)
  - [Q15. How do you diagnose deadlocks and lock contention in product…](#06-synchronization-and-locks-q15)
  - [Q16. What is thread-safe lazy initialization (`Lazy<T>`, double-c…](#06-synchronization-and-locks-q16)

- [07. Concurrent Collections](#07-concurrent-collections)
  - [Q1. What concurrent collections exist in .NET (`ConcurrentDictio…](#07-concurrent-collections-q1)
  - [Q2. When should you use thread-safe collections instead of stand…](#07-concurrent-collections-q2)
  - [Q3. What is the difference between `Dictionary<TKey, TValue>` an…](#07-concurrent-collections-q3)
  - [Q4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `Conc…](#07-concurrent-collections-q4)
  - [Q5. What is `BlockingCollection<T>`, and how does it implement p…](#07-concurrent-collections-q5)
  - [Q6. What is the difference between bounded and unbounded `Blocki…](#07-concurrent-collections-q6)
  - [Q7. How do you use `BlockingCollection` with multiple producers …](#07-concurrent-collections-q7)
  - [Q8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `Concurren…](#07-concurrent-collections-q8)
  - [Q9. When is `ConcurrentBag` the wrong choice despite being threa…](#07-concurrent-collections-q9)
  - [Q10. What is `IProducerConsumerCollection<T>` and custom underlyi…](#07-concurrent-collections-q10)
  - [Q11. How do concurrent collections compare to locking a `List<T>`…](#07-concurrent-collections-q11)
  - [Q12. What enumeration semantics do concurrent collections provide…](#07-concurrent-collections-q12)
  - [Q13. How do you gracefully complete adding to a `BlockingCollecti…](#07-concurrent-collections-q13)
  - [Q14. What pitfalls arise when mixing concurrent collections with …](#07-concurrent-collections-q14)
  - [Q15. When should you use channels (`System.Threading.Channels`) i…](#07-concurrent-collections-q15)
  - [Q16. **`.Result` / `.Wait()` deadlock** — Blocking async on a cap…](#07-concurrent-collections-q16)
  - [Q17. **`async void` swallows observability** — Exceptions cannot …](#07-concurrent-collections-q17)
  - [Q18. **`Task.Run` for I/O** — Offloading blocking I/O to the pool…](#07-concurrent-collections-q18)
  - [Q19. **Async does not mean threaded** — I/O `await` often complet…](#07-concurrent-collections-q19)
  - [Q20. **Unobserved task exceptions** — Faulted tasks that are neve…](#07-concurrent-collections-q20)
  - [Q21. **Race on `List<T>`/`Dictionary<,>`** — Even `Add` is not th…](#07-concurrent-collections-q21)
  - [Q22. **`ConfigureAwait(false)` in libraries** — Library code shou…](#07-concurrent-collections-q22)
  - [Q23. **`ValueTask` double-await** — Re-awaiting or concurrent awa…](#07-concurrent-collections-q23)
  - [Q24. **`TaskCompletionSource` set twice** — Second `TrySet*` call…](#07-concurrent-collections-q24)
  - [Q25. **`BlockingCollection` after `CompleteAdding`** — Adding thr…](#07-concurrent-collections-q25)
  - [Q26. **`Interlocked` is not composable** — Check-then-act on comp…](#07-concurrent-collections-q26)
  - [Q27. **`volatile` does not make operations atomic** — `i++` still…](#07-concurrent-collections-q27)
  - [Q28. **Parallel loop over small work** — Partitioning overhead ca…](#07-concurrent-collections-q28)
  - [Q29. **Shared `Random` is not thread-safe** — Use `Random.Shared`…](#07-concurrent-collections-q29)
  - [Q30. **Retry without cancellation** — Exponential backoff loops m…](#07-concurrent-collections-q30)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Threads & Thread Lifecycle

#### Q1. Explain multithreading in C# and when it is appropriate vs async I/O or tasks. {#01-threads-thread-lifecycle-q1}

(R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:

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

#### Q2. What is a `Thread`, and how do you create and start one? {#01-threads-thread-lifecycle-q2}

(R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:

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

#### Q3. What are foreground vs background threads, and how do they affect process shutdown? {#01-threads-thread-lifecycle-q3}

(R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:

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

#### Q4. What are the main thread states in the lifecycle (unstarted, running, wait/sleep/join, stopped)? {#01-threads-thread-lifecycle-q4}

(P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?

**Answer:** Use **cooperative cancellation** with `CancellationToken` linked to the service's `IHostApplicationLifetime.ApplicationStopping` (or a `CancellationTokenSource` cancelled in `StopAsync`). The worker checks `IsCancellationRequested` (or `ThrowIfCancellationRequested`) in its loop, finishes the current aisle/unit of work if needed, releases locks and handles, and exits the delegate normally — then the host `Join`s the thread or awaits a `Task` wrapper.

- Never call `Thread.Abort` — removed from .NET Core because it could leave locks held and invariants broken mid-method (**Program.cs** Section 8).
- Pass the token into the worker at construction/start; cancel once from the shutdown path; block shutdown on `Join(timeout)` and log if the worker exceeds the SLA.
- Keep loop body idempotent at cancellation boundaries — persist checkpoint if stopping mid-batch matters for ops.
- For I/O-bound sweeps, prefer `async`/`await` with the same token (later chapter) so threads are not blocked in `Sleep`.

**Production takeaway:** Production shutdown is "signal, wait, log timeout" — not force-terminate. Dedicated `Thread` is acceptable for a long-lived CPU worker when lifecycle and join semantics are explicit.

---

#### Q5. What is `Thread.Join()`, and what happens if you never join a foreground thread? {#01-threads-thread-lifecycle-q5}

(M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?

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

#### Q6. What is `Thread.Sleep()` vs spinning vs waiting — when is each appropriate? {#01-threads-thread-lifecycle-q6}

(D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?

**Answer:** Unbounded `new Thread` per shipment exhausts OS thread limits and memory (default stack reserve per thread), thrashes the scheduler, and makes shutdown join storms impossible. `ThreadPriority` is an OS hint, not a SLA — express lanes are not reliably prioritized across machines. `[ThreadStatic]` metrics break when work moves to thread pool threads or `async` continuations hop threads — counters attach to threads, not logical shipments.

- Cap concurrency: fixed pool of long-lived worker threads **or** `ThreadPool` / `Task` with a bounded `SemaphoreSlim` (e.g., max 8 scanners) — same lifecycle idea (start work, join/complete, cooperative stop) without 1:1 OS threads.
- Express handling belongs in queue priority or business rules, not `ThreadPriority.AboveNormal`.
- Export metrics with labels (`shipment_id`, `worker_id`) via `IMeterFactory`/Prometheus counters — not `[ThreadStatic]` tallies.
- Keep `CancellationToken` on the batch host so service shutdown still cooperates (**Section 8** pattern).
- CPU-bound parallel loops → **05. Parallel Programming**; I/O-bound waits → **04. Async and Await**.

**Production takeaway:** This chapter teaches manual threads for **lifecycle literacy** — production scales with bounded pools, tokens, and synchronized shared state, not unbounded `Thread` construction.

---

#### Q7. What is thread affinity, and why does it matter for UI applications? {#01-threads-thread-lifecycle-q7}

(R) A retry path tries to restart workers after a transient fault. Review:

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

---

#### Q8. What is the difference between creating a raw `Thread` and using thread pool threads? {#01-threads-thread-lifecycle-q8}

_Answer not found._

---

#### Q9. What are `Thread.Name`, `IsBackground`, `Priority` — which actually affect scheduling? {#01-threads-thread-lifecycle-q9}

_Answer not found._

---

#### Q10. What is a race condition at the thread level, and how can two threads interleave unpredictably? {#01-threads-thread-lifecycle-q10}

_Answer not found._

---

#### Q11. What is the difference between kernel threads and managed threads (conceptual model)? {#01-threads-thread-lifecycle-q11}

_Answer not found._

---

#### Q12. Why is manually creating many threads often a scalability anti-pattern? {#01-threads-thread-lifecycle-q12}

_Answer not found._

---

#### Q13. What is `ThreadStatic`, and how does it differ from `ThreadLocal<T>`? {#01-threads-thread-lifecycle-q13}

_Answer not found._

---

#### Q14. What exceptions can occur when aborting or interrupting threads (historical vs modern guidance)? {#01-threads-thread-lifecycle-q14}

_Answer not found._

---

#### Q15. How does the main thread exiting affect background work still running? {#01-threads-thread-lifecycle-q15}

_Answer not found._

---

### 02. ThreadPool

#### Q1. What is the thread pool in .NET, and why is it preferred over creating raw threads? {#02-threadpool-q1}

(R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?

**Answer:** `QueueUserWorkItem` returns immediately — the method reads `results` and publishes a summary before pool callbacks finish, so `valid` is often zero or partial. There is no synchronization, and exceptions inside callbacks would be unobserved.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | No wait after queuing work | Race — reads `results` while callbacks still writing |
| Threading | Fire-and-forget pool callbacks | Import summary wrong under any real batch size |
| Observability | No try/finally around callback body | Validation exceptions disappear on pool threads |
| Design | Treats async queue like synchronous loop | Silent data loss in nightly job metrics |

**Fix (priority order):**

1. Block until all callbacks complete — use `CountdownEvent` initialized to `jobCount`, `Signal()` in a `finally` block per callback, then `done.Wait()` before counting (matches **Program.cs** Section 4).
2. Wrap callback work in try/catch and log or aggregate failures — never let pool exceptions vanish.
3. Prefer `Task.Run` + `Task.WhenAll` (ch.03) or `Parallel.For` (ch.05) when you need structured exception propagation and cancellation.
4. Return the summary only after the wait gate completes; add a timeout/cancellation token for hung batches.

```csharp
using var done = new CountdownEvent(jobCount);
for (int i = 0; i < jobCount; i++)
{
    int jobId = i;
    ThreadPool.QueueUserWorkItem(_ =>
    {
        try { results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, 8_000)); }
        catch (Exception ex) { _logger.LogError(ex, "Line {Id}", jobId); }
        finally { done.Signal(); }
    });
}
done.Wait();
```

**Production takeaway:** The chapter's core trap — queuing is not completion. Karat expects you to name `CountdownEvent` (or equivalent) before trusting shared result arrays.

---

#### Q2. How does the thread pool manage worker threads and I/O completion threads? {#02-threadpool-q2}

(R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?

**Answer:** `WaitHandle.WaitAll` on more than 64 handles throws `NotSupportedException` when the calling thread is STA — common in legacy COM/WPF hosts. Even when it succeeds, allocating 500 `ManualResetEvent` objects per batch is expensive compared to one `CountdownEvent`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `WaitAll` with 500 handles on STA thread | Batch fails at scale — works in small demo, fails in prod |
| Resource | One `ManualResetEvent` per job | Handle churn, allocation pressure, dispose overhead |
| Correctness | `Set()` outside `finally` | Exception in `DoWork` leaves event unsignaled → permanent hang |
| Maintainability | Per-job handles for large `jobCount` | Violates tutorial guidance (Section 5 pitfall) |

**Fix (priority order):**

1. Replace per-job events + `WaitAll` with a single `CountdownEvent(jobCount)` and `Signal()` in `finally` ( **Program.cs** Section 4).
2. Move `signal.Set()` into `finally` so failures cannot deadlock the waiter.
3. If you must use events, use `WaitOne` on one gate or `Task.WhenAll` — not `WaitAll` on hundreds of handles.
4. Dispose synchronization primitives via `using` on the countdown/event wrapper.

**Production takeaway:** The tutorial explicitly warns that `WaitAll` is limited to 64 handles on some STA paths — Karat tests whether you read that footnote and default to `CountdownEvent` for large batches.

---

#### Q3. What is hill-climbing in the .NET thread pool (high level)? {#02-threadpool-q3}

(R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.

**Answer:** `.Result` inside a thread-pool callback blocks a worker thread for the entire HTTP wait — sync-over-async on the same pool that ASP.NET, timers, and `Task.Run` share. Under load, workers pile up blocked on I/O while the queue grows, producing apparent "deadlock" or severe latency.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async / threading | `.Result` on `GetStringAsync` in pool callback | Blocks worker threads during I/O — pool starvation |
| Scalability | Many concurrent `EnqueueAuditFetch` calls | Queue backlog; timers and request handling stall |
| Design | CPU pool used for I/O-bound work synchronously | Low CPU, high latency — misread as "need more cores" |
| HTTP | Long-lived `HttpClient` assumed but pattern ignores async model | Same trap as web `.Result` on `GetAsync` |

**Fix (priority order):**

1. Remove sync-over-async — use `async`/`await` end-to-end: `await _httpClient.GetStringAsync(url)` on an async code path (ch.04), not `.Result` on a pool thread.
2. If you must queue, queue async work via `Task.Run` only for CPU-bound segments; I/O should use async I/O that frees workers during waits (IOCP — **Program.cs** Section 9).
3. Bound concurrency with `SemaphoreSlim` or a dedicated channel/worker so audit fetches cannot exhaust the global pool.
4. Register `IHttpClientFactory` in ASP.NET hosts instead of ad-hoc blocking calls.

**Production takeaway:** Thread-pool starvation from `.Result`/`Wait()` is a top Karat theme — the fix is async I/O, not `SetMinThreads`. See foundation gotcha on sync-over-async in the multithreading module.

---

#### Q4. What is `ThreadPool.QueueUserWorkItem`, and how does it relate to `Task.Run`? {#02-threadpool-q4}

(P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

**Answer:** Inflating minimum threads on every instance reserves idle workers and I/O threads that consume memory (~1 MB stack each on Windows) without guaranteed throughput gain. Forty pods × 250 workers can oversubscribe an 8-core node and increase context switching while hiding the real bottleneck (slow or blocking callbacks).

- **What breaks:** RAM pressure, scheduler thrashing, false sense of capacity; does not fix blocking code — blocked threads stay blocked regardless of min count.
- **When it helps (sparingly):** Measured cold-start after idle where pool ramp-up latency dominates *short* bursts of queued work — e.g., a known spike right after deploy. Raise min modestly, validate with `GetAvailableThreads` and latency metrics, revert if idle waste grows.
- **Prefer instead:** Fix blocking/sync-over-async, use async I/O, bound parallel fan-out, scale horizontally with sensible concurrency limits — **Program.cs** Section 8 warns to use `SetMinThreads` sparingly.
- **`SetMaxThreads`:** Capping the pool can create unbounded queue backlog — rarely the first lever; fix slow callbacks first.

**Production takeaway:** Karat distinguishes tuning the pool from compensating for bad callbacks — `SetMinThreads(250)` on every pod is a red flag, not a standard template.

---

#### Q5. What is starvation in the thread pool, and what causes it? {#02-threadpool-q5}

(M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?

**Answer:** Worker threads blocked on I/O or locks still count as busy (`max − available`), even when they are not executing CPU instructions — so low CPU with a exhausted-looking pool usually means blocking work on the pool, not insufficient cores.

- The callback opens a DB connection and sleeps — both block the worker without consuming CPU; many such items queue up and starve unrelated pool users (timers, ASP.NET, other `QueueUserWorkItem` work).
- `GetAvailableThreads` is a snapshot — useful for trend logging, not a capacity plan by itself; pair with queue wait time, request latency, and thread-pool starvation counters.
- **First change:** Move blocking DB access to async ADO.NET (`await conn.OpenAsync`, async execute) so workers release during I/O waits (IOCP path — Section 9 preview).
- **Second:** Do not perform long synchronous DB work directly on thread-pool threads — use a bounded dedicated worker or channel with explicit concurrency.
- **Not first:** Buying cores or blindly raising `SetMinThreads` — that multiplies blocked threads, not useful parallelism.

**Production takeaway:** Busy pool + low CPU screams "blocked workers," matching the tutorial's worker vs I/O thread distinction and the async chapter forward reference.

---

#### Q6. How do synchronous blocking calls inside pool threads affect throughput? {#02-threadpool-q6}

(D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

**Answer:** Ship **B** (pool or modern `Task`/`Parallel` APIs) for this workload — many short, independent CPU jobs are exactly what the thread pool amortizes (**Program.cs** Section 10 comparison table).

- **Throughput:** Manual threads pay OS creation/teardown per job; the pool reuses workers and scales with processor count — the chapter's Stopwatch demo (Section 11) shows pool wins for large batches.
- **Memory:** 2,000 manual threads ≈ gigabytes of default stack reservation; the pool holds a bounded worker set capped by `GetMaxThreads`.
- **Operational risk:** Thread explosion can OOM or thrash the scheduler; pool caps growth and integrates with existing ASP.NET/`Task` infrastructure.
- **When manual `Thread` still fits:** One or few long-lived workers (custom name/priority, foreground lifetime, special apartment/stack) — not 2,000 ephemeral resize jobs.
- **Modern default:** Prefer `Task.Run` + `Task.WhenAll` or `Parallel.For` with `MaxDegreeOfParallelism` for clearer cancellation/exception handling; `QueueUserWorkItem` remains valid for legacy fire-and-forget patterns.

**Production takeaway:** Karat uses batch thumbnail/validation scenarios to test the decision matrix in Section 10 — not reciting "what is a thread pool."

---

#### Q7. What is the difference between dedicated threads and pool threads for long-running work? {#02-threadpool-q7}

(R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.

**Answer:** The method queues work and returns before validations finish, mutates a non-thread-safe `List<string>` from multiple pool threads without synchronization, and never surfaces callback exceptions — so clients get 202 while data is incomplete and errors are lost.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | Unsynchronized `List<T>.Add` | Corrupted list, lost entries, rare crashes |
| Threading | No completion gate before caller continues | HTTP 202 implies done; work still running |
| Observability | No try/catch in callback | Validation exceptions never logged |
| API contract | Fire-and-forget from request handler | Partial imports, race with downstream steps |
| Design | Shared mutable aggregator on pool threads | Violates Section 6 guidance — prefer per-slot results or concurrent collection |

**Fix (priority order):**

1. Decide contract: if the API must return after validation, wait with `CountdownEvent`/`Task.WhenAll` (and return 200/422 with results); if truly background, enqueue to a durable queue (Azure Service Bus, Hangfire) — not naked `QueueUserWorkItem` from a request.
2. Replace `List<string>` with `ConcurrentBag<string>` or write each failure to `results[i]` then merge after the wait — one writer per index avoids locks (**Program.cs** Section 4 pattern).
3. Wrap callback body in try/catch/finally — log and signal completion in `finally`.
4. Propagate unhandled failures to telemetry (`ILogger`, Application Insights unhandled exception tracking).

**Production takeaway:** Thread-pool callbacks require the same completion, exception, and thread-safety discipline as any parallel code — "queue and forget" from an HTTP handler is a production incident waiting to happen. See QUICK REFERENCE — "Ignoring exceptions in callbacks" and "Exit Main before callbacks finish."

---

#### Q8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and when might you tune them? {#02-threadpool-q8}

_Answer not found._

---

#### Q9. How does the thread pool interact with `async`/`await` continuations? {#02-threadpool-q9}

_Answer not found._

---

#### Q10. What is the danger of blocking the UI thread vs blocking a pool thread? {#02-threadpool-q10}

_Answer not found._

---

#### Q11. How do thread pool threads relate to `Parallel.For` and PLINQ? {#02-threadpool-q11}

_Answer not found._

---

#### Q12. What diagnostics exist for thread pool queue length and thread counts (`ThreadPool.ThreadCount`, ETW)? {#02-threadpool-q12}

_Answer not found._

---

### 03. Tasks & Task Parallel Library

#### Q1. What is the Task Parallel Library (TPL)? {#03-tasks-task-parallel-library-q1}

(R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

**Answer:** The action blocks the request thread with `.Result` on `Task.WhenAll`, which is sync-over-async on ASP.NET Core's thread pool and can cause starvation or deadlocks under concurrency — compounded by wrapping likely I/O-bound validation in `Task.Run`, which wastes pool threads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `Task.WhenAll` | Blocks request thread; sync-over-async under load |
| Scalability | `Task.Run` for validation | Extra thread-pool hop; steals threads from Kestrel |
| API design | Non-async action, no `CancellationToken` | Cannot abort on client disconnect; poor composability |
| Correctness | Unbounded parallel tasks per request | Large batches can exhaust pool or downstream DB |

**Fix (priority order):**

1. Change signature to `async Task<IActionResult> ValidateBatch(..., CancellationToken ct)` and `bool[] results = await Task.WhenAll(validations)`.
2. If validation is I/O-bound, call `_orderValidator.ValidateAsync(id, ct)` directly — no `Task.Run`.
3. Cap concurrency for large batches (`SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or chunked `WhenAll`).
4. Return early or fail fast if `orderIds` exceeds a configured limit.

**Production takeaway:** Passes locally with one user; fails when the pool saturates because every request blocks waiting on `.Result` — the same Karat trap as `.Result` on a single service call, amplified by `WhenAll`. See chapter QUICK REFERENCE — `.Result on UI / ASP.NET request → Deadlock`.

---

#### Q2. Explain the difference between `Thread` and `Task` in purpose and scheduling. {#03-tasks-task-parallel-library-q2}

(R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

**Answer:** The parent task completes as soon as the `StartNew` delegate returns — before nested `Task.Run` children finish — so the caller releases the dock slot while picks are still in flight. `Task.Factory.StartNew` without an explicit scheduler also inherits `TaskScheduler.Current`, which can inline work unexpectedly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Task composition | Nested `Task.Run` not attached to parent | Parent `RanToCompletion` before children; missing picks |
| Scheduler | `StartNew` without `TaskScheduler.Default` | May run on wrong scheduler or inline on caller |
| Lifecycle | Caller `.Wait()` only waits for parent shell | Downstream assumes fulfillment done when it is not |
| Design | Fire-and-forget child tasks | No aggregation, fault propagation, or cancellation |

**Fix (priority order):**

1. Collect child tasks and wait for all: `var picks = order.Lines.Select(line => Task.Run(() => Pick(line))).ToArray(); Task.WhenAll(picks).Wait();` inside the parent — or prefer `async`/`await` with `await Task.WhenAll(picks)` (ch.04).
2. For parent/child lifetime linking, use `TaskCreationOptions.AttachedToParent` with explicit `TaskScheduler.Default` on `StartNew` — as in **Program.cs** Section 12 — only when you truly need attached semantics.
3. Replace outer `Task.Factory.StartNew` with `Task.Run` unless non-default creation options are required.
4. Propagate exceptions: observe all child tasks; a faulted pick must fail the fulfillment operation, not disappear.

**Production takeaway:** Karat tests whether you know a `Task` completing does not mean all nested work finished — unattached children are the TPL equivalent of forgotten `Join` on threads. See **Program.cs** Section 12 and QUICK REFERENCE — "Unattached nested Task.Run in parent."

---

#### Q3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use each. {#03-tasks-task-parallel-library-q3}

(R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

**Answer:** The wrapper leaks event handlers on every call, uses throwing `SetResult`/`SetException` instead of `TrySet*`, and has no path to complete the task if the gateway never fires — so callers hang. A second callback can throw `InvalidOperationException` when the task is already completed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` handlers never removed | Memory leak; duplicate callbacks on reused gateway |
| Correctness | `SetResult` / `SetException` after completion | `InvalidOperationException` on duplicate events |
| Reliability | No timeout or cancel registration | Hung tasks when gateway drops the callback |
| Design | No single-flight guard per charge attempt | Concurrent charges race on one TCS instance |

**Fix (priority order):**

1. Use `TrySetResult`, `TrySetException`, `TrySetCanceled` and unsubscribe handlers in the callback after the first terminal signal.
2. Register `CancellationToken` / `CancelAfter` to call `TrySetCanceled` when the HTTP/client timeout fires.
3. Create one `TaskCompletionSource` per `ChargeAsync` invocation (already implied) — never share across concurrent calls.
4. Optionally wrap with `Task.WhenAny(tcs.Task, timeoutTask)` for defense in depth.

```csharp
void CompleteOnce(Action complete) { if (tcs.TrySetResult(default!)) { /* use TrySet* */ complete(); } }
// Prefer: if (tcs.TrySetResult(result)) { _gateway.PaymentCompleted -= handler; }
```

**Production takeaway:** TCS bridges external callbacks into the task model — production wrappers must be idempotent and self-cleaning. See **Program.cs** Section 13 — `TrySet*` returns false if already completed.

---

#### Q4. What is `Task.Run`, and when should it be used vs when it should be avoided? {#03-tasks-task-parallel-library-q4}

(R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

**Answer:** When the antecedent is faulted, the continuation still runs by default and accessing `antecedent.Result` rethrows — often as `AggregateException` — instead of routing to a fault handler. Unobserved or poorly observed faulted continuations can tear down the process via `TaskScheduler.UnobservedTaskException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Continuation | Missing `TaskContinuationOptions.OnlyOnRanToCompletion` | Fault path executes success delegate |
| Exception | `antecedent.Result` on faulted task | `AggregateException` at continuation time |
| Observability | No `OnlyOnFaulted` logging branch | Silent or unobserved faults in background chains |
| Style | `.Result` at end of chain | Blocks worker thread; wraps exceptions again |

**Fix (priority order):**

1. Add status filter: `ContinueWith(..., TaskContinuationOptions.OnlyOnRanToCompletion)` for the label step.
2. Add fault handler: `pickTask.ContinueWith(t => Log(t.Exception), TaskContinuationOptions.OnlyOnFaulted)`.
3. Prefer `await pickTask` / `await pickTask.ContinueWith(...)` (ch.04) — compiler preserves stack traces better than manual `ContinueWith`.
4. Replace final `.Result` with `await labelTask` or `GetAwaiter().GetResult()` only at a true sync boundary.

**Production takeaway:** `ContinueWith` defaults to running on any terminal state — Karat expects you to filter with `OnlyOnRanToCompletion` / `OnlyOnFaulted`. See **Program.cs** Sections 11 and 16.

---

#### Q5. What is `Task.Factory.StartNew`, and why is `Task.Run` usually preferred? {#03-tasks-task-parallel-library-q5}

(P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

**Answer:** `Release()` is not in a `finally` block — any exception or cancellation after `WaitAsync` consumes a semaphore slot permanently. After enough failures, all four slots are held and every new pick blocks forever until process restart.

- Wrap the guarded work in `try/finally` and call `_pickerGate.Release()` in `finally` — matching **Program.cs** Section 10 (`ThrottledPick`).
- Prefer `await _pickerGate.WaitAsync(ct)` with the same `CancellationToken` passed to downstream calls so aborting a request releases the wait cleanly.
- Consider `SemaphoreSlim` as a singleton with explicit max count documented; dispose only on application shutdown — not per request.
- Monitor `_pickerGate.CurrentCount` in health checks to detect leak regressions early.

**Production takeaway:** Semaphore throttling is correct for capping concurrent warehouse/API work — but without `finally`, one transient fault becomes a permanent outage. See chapter QUICK REFERENCE — "Forget Release on SemaphoreSlim → Permanent throttle / leak."

---

#### Q6. Explain task continuations with `ContinueWith` — options, scheduling, and exception handling. {#03-tasks-task-parallel-library-q6}

(M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

**Answer:** `Task.WhenAny` completes when the first task finishes — it does not cancel or dispose the slower tasks. Losing carrier HTTP calls continue until completion, holding connections, thread-pool slots, and memory under sustained load.

- After `WhenAny`, cancel remaining work with a linked `CancellationTokenSource` passed into each quote call, then `cts.Cancel()` once the winner is chosen.
- If APIs are not cancelable, track in-flight calls and abandon results safely — but still close/dispose `HttpResponseMessage` and respect `IHttpClientFactory` lifetimes.
- Replace blocking `.Result` with `await Task.WhenAny(...)` in an async API so the request thread is not blocked during the race.
- Log slow-loser latency separately — persistent tail latency after "winner found" signals missing cancellation.

**Production takeaway:** `WhenAny` is a coordination primitive, not a resource cleanup primitive — production races must explicitly stop losers. See **Program.cs** Section 9 — first completed task wins; others keep running unless canceled.

---

#### Q7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they differ from manual continuation chaining? {#03-tasks-task-parallel-library-q7}

(D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?

**Answer:** Launching four thousand simultaneous `Task.Run` validations queues thousands of work items at once, spiking thread-pool usage and likely overwhelming the database — `Task.WaitAll` also blocks the orchestrator thread until every task completes, with no backpressure. `Parallel.ForEach` helps CPU-bound validation but is the wrong default if validation is I/O-bound and still needs a concurrency cap for downstream limits.

- Ship chunked or throttled async orchestration: `await Task.WhenAll(batch.Select(o => ValidateAsync(o, ct)))` over batches of 50–200, or use `SemaphoreSlim` / `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` tuned to DB connection limits.
- Use `Task.Run` only for CPU-bound validation logic; I/O-bound checks should be truly async end-to-end (ch.04).
- Prefer `Task.WhenAll` over `Task.WaitAll` in async hosts — composable with cancellation and does not block a precious thread for the entire batch duration.
- Emit metrics: queue depth, validation latency p95, and faulted task count — unobserved faults in nightly jobs can fail silently until morning.

**Production takeaway:** Tasks make it easy to express parallelism; production requires **bounded** parallelism. Karat distinguishes "I can start 4,000 tasks" from "I should start 4,000 tasks." See **Program.cs** Sections 8 (`WhenAll`) and 10 (`SemaphoreSlim` throttle).

---

---

#### Q8. What is `TaskCompletionSource<T>`, and what scenarios does it enable (bridging callbacks, manual completion)? {#03-tasks-task-parallel-library-q8}

_Answer not found._

---

#### Q9. What is the difference between completing a `TaskCompletionSource` with result, exception, or cancellation? {#03-tasks-task-parallel-library-q9}

_Answer not found._

---

#### Q10. What is `Task.FromResult`, `Task.CompletedTask`, and when are they preferable to `Task.Run`? {#03-tasks-task-parallel-library-q10}

_Answer not found._

---

#### Q11. What is the difference between `AggregateException` and a regular exception when tasks fail? {#03-tasks-task-parallel-library-q11}

_Answer not found._

---

#### Q12. How do child tasks relate to parent tasks (`TaskCreationOptions`, attached vs detached)? {#03-tasks-task-parallel-library-q12}

_Answer not found._

---

#### Q13. What is task cancellation via `CancellationToken` registration vs `TrySetCanceled`? {#03-tasks-task-parallel-library-q13}

_Answer not found._

---

#### Q14. What are unobserved task exceptions, and how does .NET handle them? {#03-tasks-task-parallel-library-q14}

_Answer not found._

---

#### Q15. What is `ValueTask` pooling/caching, and why must consumers avoid double-awaiting unless documented safe? {#03-tasks-task-parallel-library-q15}

_Answer not found._

---

#### Q16. How do you implement a timeout around a `Task` using `CancellationTokenSource` or `WhenAny`? {#03-tasks-task-parallel-library-q16}

_Answer not found._

---

### 04. Async and Await

#### Q1. Explain asynchronous programming in C# — what problem does it solve? {#04-async-and-await-q1}

(R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?

**Answer:** The endpoint blocks a thread-pool thread twice via `.Result` and `.Wait()` on unfinished Tasks, defeating ASP.NET Core's async I/O model and risking deadlocks when a captured request context prevents continuations from running under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` / `.Wait()` on `Task` | Sync-over-async; blocks thread per request |
| Scalability | Two blocking waits per HTTP call | Thread-pool starvation; throughput collapse under concurrency |
| API design | Non-async endpoint delegate | Cannot accept `CancellationToken`; poor composability with filters/middleware |
| Runtime | Captured `SynchronizationContext` on legacy ASP.NET / some hosts | Potential deadlock — continuation waits for blocked request thread |

**Fix (priority order):**

1. Change the endpoint to `async Task<IResults>` (or `async Task`) and `await` both service calls end-to-end.
2. Thread `CancellationToken` from `HttpContext.RequestAborted` into `FetchMetadataAsync` and downstream I/O.
3. Ensure the service layer stays async all the way — no `.Result` / `.Wait()` anywhere in the call chain.
4. Load-test after the change — thread-pool queue length should stay flat under I/O-bound load.

```csharp
app.MapGet("/reports/{id}", async (ReportService svc, string id, CancellationToken ct) =>
{
    var metadata = await svc.FetchMetadataAsync(id, ct);
    await svc.ProcessReportAsync(metadata, ct);
    return Results.Ok(metadata);
});
```

**Production takeaway:** Passes locally with one user; fails in production when hundreds of requests each block a pool thread waiting on I/O — the classic Karat async trap. See **Program.cs** Section 14 — deadlock pitfalls with `.Result` / `.Wait()`.

---

#### Q2. Explain the `async` and `await` keywords in detail. {#04-async-and-await-q2}

(R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?

**Answer:** `LogExportStarted` is `async void`, so its exception cannot be caught by the caller and propagates through the synchronization context as an unhandled exception — often terminating the worker. The fire-and-forget pipeline Task is also unobserved, so its failures are silent until an unobserved-task handler fires.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `async void` on non-event method | Exceptions crash process; caller cannot await or catch |
| Observability | `_ = RunExportPipelineAsync(...)` discard | Faulted export Task may go unlogged |
| Design | Sync `StartExport` returns before work completes | Host thinks job started successfully; no completion signal |
| Testing | Neither path returns `Task` | Cannot assert success/failure in unit or integration tests |

**Fix (priority order):**

1. Change `LogExportStarted` to `async Task` and await it (or fold logging into the pipeline).
2. Return `Task` from `StartExport` / `RunExportPipelineAsync` and await at the host boundary (BackgroundService, Hangfire job, Azure Function entry).
3. Wrap the pipeline in try/catch with structured logging; rethrow or surface failure to the scheduler.
4. Reserve `async void` strictly for UI event handlers — see **Program.cs** Section 4.

```csharp
public async Task StartExportAsync(string reportId, CancellationToken ct)
{
    await LogExportStartedAsync(reportId, ct);
    await RunExportPipelineAsync(reportId, ct);
}
```

**Production takeaway:** Karat embeds `async void` in service code because it compiles and "works" until the first fault — then the process dies with no useful audit trail.

---

#### Q3. What is the difference between CPU-bound and I/O-bound async work? {#04-async-and-await-q3}

(R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?

**Answer:** The UI thread blocks on `.Result` while the async continuation tries to marshal back to the same UI thread (default `await` captures `SynchronizationContext`). The blocked UI thread cannot run the continuation — classic sync-over-async deadlock.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on UI thread | Deadlock — UI blocked, continuation needs UI |
| Library | Missing `ConfigureAwait(false)` | Continuation posts back to captured context |
| Design | Sync wrapper around async API on UI | Forces blocking anti-pattern |
| Runtime | Works in console (no context) | Hides bug until WPF/WinForms/legacy ASP.NET |

**Fix (priority order):**

1. **App:** Make `LoadReportOnStartup` async (`async void` acceptable for UI event) and `await ReportFetcher.GetReportAsync("Q1")`.
2. **Library:** Add `.ConfigureAwait(false)` on every await in reusable code that does not touch UI after the await — see **Program.cs** Section 5.
3. Never expose sync `.Result` / `.Wait()` wrappers from library public APIs.
4. If sync API is unavoidable at a legacy boundary, document it as UI-thread-unsafe and offload with `Task.Run` only as a last resort (still inferior to async all the way).

```csharp
// Library
await Task.Delay(100).ConfigureAwait(false);

// UI
private async void LoadReportOnStartup()
{
    string data = await ReportFetcher.GetReportAsync("Q1");
    ReportLabel.Text = data;
}
```

**Production takeaway:** Console tutorials mask this — Karat pairs library + UI caller to test whether you fix both the blocking call site and context capture in shared code.

---

#### Q4. What is `ConfigureAwait(false)`, and when should library vs application code use it? {#04-async-and-await-q4}

(P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?

**Answer:** `Task.WhenAny` only stops awaiting the loser — the legacy download keeps running in the background after timeout or cancellation. Under repeated cancels, orphaned downloads accumulate, wasting sockets, memory, and upstream quota; callers believe work stopped but it did not.

- **Abandoned work:** When `delay` wins, `download` is never awaited — exception on faulted Task may become unobserved; successful completion is silently ignored but still consumed resources.
- **No cooperative cancel:** Legacy API cannot be interrupted — only the wrapper's wait ends.
- **Resource leaks:** Connection pool exhaustion when many users navigate away or hit client-side timeouts.

**Production pattern:**

1. Document that this is **best-effort abandonment**, not true cancellation — matches **Program.cs** Section 8.
2. Prefer upgrading the legacy client or wrapping at a process boundary you can kill (separate worker, linked `CancellationTokenSource` with timeout).
3. If stuck with `WhenAny`, track in-flight downloads in a registry; optionally use a `CancellationTokenSource` linked to caller token + timeout and log abandoned operation IDs for ops visibility.
4. Surface `OperationCanceledException` to callers but monitor background completion rate — alert if abandoned tasks pile up.
5. For HTTP specifically, migrate to `HttpClient` with `CancellationToken` and `IHttpClientFactory` rather than permanent `WhenAny` shims.

**Production takeaway:** Timeout via `WhenAny` improves caller responsiveness but does not cancel underlying I/O — Karat tests whether you explain the orphan-work trade-off, not just paste the pattern.

---

#### Q5. How do you handle exceptions in async/await methods? {#04-async-and-await-q5}

(R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?

**Answer:** The retry loop catches `OperationCanceledException` and retries anyway, and the backoff delay ignores `ct` — so user disconnect or request abort does not stop retries until all attempts and delays finish.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cancellation | `catch (Exception)` swallows `OperationCanceledException` | User cancel ignored; work continues after client left |
| Async | `Task.Delay` without `ct` | Each backoff waits full duration even when token is signaled |
| Correctness | `operation()` may not receive `ct` | Inner fetch keeps running after outer cancel |
| Operability | Up to 5 attempts × multi-second delay | Minutes of wasted upstream calls post-cancel |

**Fix (priority order):**

1. Exclude cancellation from retry filter: `catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)`.
2. Pass token to delay: `await Task.Delay(attempt * 2000, ct)`.
3. Pass `ct` into `FetchReportAsync` inside the lambda so in-flight I/O aborts cooperatively.
4. Optionally distinguish transient faults (`HttpRequestException`, 503) from permanent errors — do not retry 400-class failures.
5. Align with **Program.cs** Section 9 — retry skeleton respects `CancellationToken` and skips `OperationCanceledException`.

```csharp
catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
{
    await Task.Delay(attempt * 2000, ct);
}
```

**Production takeaway:** Retry helpers that catch all exceptions silently extend request lifetime after abort — a common production incident when `RequestAborted` is wired but the retry layer ignores it.

---

#### Q6. What is the difference between `async void`, `async Task`, and `async Task<T>`? {#04-async-and-await-q6}

(D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?

**Answer:** Missing `Release()` in `finally` permanently reduces the semaphore count after the first export — subsequent callers block forever on `WaitAsync`. A static gate also only serializes within one process, not across scaled-out instances.

- **Missing `finally` / `Release`:** One successful path consumes the permit; second export deadlocks all waiters — matches broken mutex usage, not the **Program.cs** Section 10 pattern.
- **Exception before `Release`:** Any fault between `WaitAsync` and manual release leaks a permit — always `try/finally`.
- **Cancellation during `WaitAsync`:** Correctly throws without acquiring — OK; cancellation during write must still run `Release` if wait succeeded.
- **Multi-instance:** Static `SemaphoreSlim` is per-process — two pods write concurrently to shared storage; need distributed lock (blob lease, Redis RedLock, DB advisory lock) for cluster-wide exclusivity.
- **Singleton + static gate:** Redundant — instance field on scoped service or explicit distributed lock is clearer for testability.

**Correct pattern:**

```csharp
await ExportGate.WaitAsync(ct);
try
{
    await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
}
finally
{
    ExportGate.Release();
}
```

For multi-node: replace in-memory gate with storage-level lease; keep `SemaphoreSlim` only for single-process throttling.

**Production takeaway:** Karat stacks async gate bugs — forgotten `Release` causes gradual "exports stopped working" incidents; static gates give false confidence after horizontal scale-out.

---

#### Q7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, and how does `await foreach` work? {#04-async-and-await-q7}

(M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?

**Answer:** A `ValueTask` / `ValueTask<T>` must be consumed exactly once — storing it in a field and awaiting it from two concurrent callers violates that rule, producing `InvalidOperationException` when the second await tries to reuse the same instance.

- **Single consumption:** Unlike `Task`, `ValueTask` may wrap a pooled `IValueTaskSource` — double-await is undefined.
- **Concurrency:** Two threads can both see `_cachedCount is null` and create races even before double-await.
- **Preview scope:** **Program.cs** Section 13 warns — do not await a `ValueTask` twice or store for later; consume immediately.

**Safe caching options:**

1. Cache the **`Task<int>`** (or `int` after completion), not the `ValueTask` — e.g. `private Task<int>? _countTask;` assigned once under lock.
2. Use `Lazy<Task<int>>` or `AsyncLazy<T>` pattern for memoized async initialization.
3. If the method often completes synchronously (cache hit), return `ValueTask.FromResult(cachedInt)` on the hot path and only allocate `Task` on miss — but never share one `ValueTask` instance across callers.
4. For concurrent first access, use `SemaphoreSlim` or `lock` around cache population.

```csharp
private Task<int>? _countTask;

public Task<int> GetRowCountAsync(string reportId) =>
    _countTask ??= ComputeCountAsync(reportId).AsTask();
```

**Production takeaway:** `ValueTask` micro-optimizations backfire when treated like cacheable `Task` instances — Karat tests the "consume immediately" rule from the tutorial preview, not just allocation trivia.

---

#### Q8. How does the async state machine work under the hood (high level: `MoveNext`, `IAsyncStateMachine`)? {#04-async-and-await-q8}

_Answer not found._

---

#### Q9. What is synchronization context, and how does it affect continuation marshaling? {#04-async-and-await-q9}

_Answer not found._

---

#### Q10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` cause deadlocks? {#04-async-and-await-q10}

_Answer not found._

---

#### Q11. What is the difference between `await task` and `return task` from an async method (async method builder behavior)? {#04-async-and-await-q11}

_Answer not found._

---

#### Q12. How do you implement retry with exponential backoff in async code? {#04-async-and-await-q12}

_Answer not found._

---

#### Q13. What is jitter in backoff strategies, and why is it used? {#04-async-and-await-q13}

_Answer not found._

---

#### Q14. How do Polly-style resilience policies relate to manual retry loops? {#04-async-and-await-q14}

_Answer not found._

---

#### Q15. What is `CancellationTokenSource.CreateLinkedTokenSource`, and when is linking tokens needed? {#04-async-and-await-q15}

_Answer not found._

---

#### Q16. How do you propagate cancellation through layered async APIs? {#04-async-and-await-q16}

_Answer not found._

---

#### Q17. What is `Task.Delay` vs `Thread.Sleep` in async methods? {#04-async-and-await-q17}

_Answer not found._

---

#### Q18. What is "async all the way" — why is mixing blocking and async problematic? {#04-async-and-await-q18}

_Answer not found._

---

#### Q19. How do you unit test async methods and time-dependent retry logic? {#04-async-and-await-q19}

_Answer not found._

---

#### Q20. What is `IAsyncDisposable`, and how does `await using` work? {#04-async-and-await-q20}

_Answer not found._

---

### 05. Parallel Programming

#### Q1. What is `Parallel.For` and `Parallel.ForEach`? {#05-parallel-programming-q1}

(R) A nightly warehouse job sums reconciled inventory values in parallel. Finance reports totals that drift from the serial baseline. Review the hot path:

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?

**Answer:** `runningTotal += …` is not atomic — parallel workers read-modify-write the same `decimal` and lose updates, so totals are nondeterministic. It appears to pass when contention is low or the batch is small, then drifts under heavier parallel scheduling.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Threading | Unsynchronized shared `decimal` update | Lost increments → wrong financial totals |
| Correctness | Nondeterministic race | Intermittent failures; hard to reproduce in dev |
| Design | Per-iteration sharing instead of partition/merge | Forces either races or a hot lock |

**Fix (priority order):**

1. Use thread-local aggregation on `Parallel.For` / `Parallel.ForEach` — merge once per worker, not per line (see **Program.cs** Section 2b and `ThreadSafeDecimal.Add`).
2. Prefer `batch.AsParallel().Sum(r => r.ReconciledValue)` for a read-only reduction — PLINQ handles partition/merge internally.
3. If you must share one accumulator, merge under `lock` only in the finalizer delegate — never inside the per-item body.
4. Add a serial golden-sum test in CI for the same batch to catch drift before finance does.

```csharp
decimal total = 0m;
Parallel.ForEach(
    batch,
    () => 0m,
    (record, _, local) => local + record.ReconciledValue,
    local => ThreadSafeDecimal.Add(ref total, local));
```

**Production takeaway:** Parallel speedup requires **no shared writes** or **merge-at-end** patterns — `counter++`-style updates on a shared field are the chapter's core race preview. See **Program.cs** Section 8 and QUICK REFERENCE — "Shared counter++ without sync."

---

#### Q2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `CancellationToken`) used for? {#05-parallel-programming-q2}

(R) A teammate parallelizes audit-log line generation for the same SKU batch:

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?

**Answer:** `List<T>` is not thread-safe — concurrent `Add` calls corrupt internal state (exceptions, lost entries, or rare structural damage). Ordering after the fact does not fix the race, and parallel iteration over a non-indexable `IEnumerable` may buffer or enumerate unsafely depending on the source.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Threading | `List<string>.Add` from multiple workers | Corrupted list, `IndexOutOfRangeException`, missing audit lines |
| Scalability | Shared mutable collection as merge point | Workers serialize on internal list growth or fail unpredictably |
| Design | `IEnumerable` source — unknown thread safety | EF/`DbContext`, lazy sequences, or file streams may not support parallel enumeration |

**Fix (priority order):**

1. Replace with `ConcurrentBag<string>` for lock-free per-thread staging, then sort once at the end — matches the audit pattern in **Program.cs** Section 3 (lock shown there; concurrent collection is cleaner at scale).
2. Materialize to `List<StockRecord>` or an array **before** parallel work if the source is deferred or not thread-safe.
3. If order must be deterministic and cheap, consider parallel map into a pre-sized `string[]` by index when indices exist.
4. Never share a `DbContext` or single connection across parallel bodies — load the batch in one scoped query first.

```csharp
var bag = new ConcurrentBag<string>();
Parallel.ForEach(batchList, record =>
{
    bag.Add($"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}");
});
return bag.OrderBy(l => l).ToList();
```

**Production takeaway:** Parallel output collection → `ConcurrentBag` / pre-sized array / thread-local list merged once — not `List<T>` with hope. See **Program.cs** Section 3 — "Shared List<T> is not thread-safe."

---

#### Q3. What is a `Partitioner<TSource>`, and when would you supply a custom partitioner? {#05-parallel-programming-q3}

(R) Under load, a reporting endpoint times out and thread-pool starvation alerts fire. Review the "optimization" added to fetch order details:

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?

**Answer:** This is I/O-bound work forced through parallel sync-over-async — each iteration blocks a thread-pool thread waiting on HTTP, while `Parallel.ForEach` multiplies concurrent blocked threads. Combined with an unsynchronized `List<T>`, you get starvation, wrong results, and socket exhaustion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetAwaiter().GetResult()` on async HTTP | Blocks thread-pool threads; sync-over-async |
| Threading | `Parallel.ForEach` on I/O waits | Many blocked workers → ASP.NET request starvation |
| Threading | Unsynchronized `List<string>.Add` | Same race as Q2 — corrupted output |
| Scalability | Parallel API for network-bound batch | Wrong tool — threads idle on I/O instead of CPU work |
| HTTP | Unbounded parallel HTTP from one request | Socket/port pressure; downstream rate-limit trips |

**Fix (priority order):**

1. Make the action async end-to-end: `Parallel.ForEachAsync` ( .NET 6+ ) or `Task.WhenAll` with a `SemaphoreSlim` cap — **not** `Parallel.ForEach` + `.Result`.
2. Use `IHttpClientFactory` and pass `CancellationToken` from `HttpContext.RequestAborted`.
3. Collect results in a thread-safe structure or pre-sized array indexed by position.
4. Cap concurrency (`MaxDegreeOfParallelism` or semaphore) to protect the internal API and thread pool.

```csharp
public async Task<IActionResult> ExportOrders(int[] orderIds, CancellationToken ct)
{
    var lines = new string[orderIds.Length];
    await Parallel.ForEachAsync(
        orderIds.Select((id, i) => (id, i)),
        new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = ct },
        async (item, token) =>
        {
            lines[item.i] = await _httpClient.GetStringAsync($"/internal/orders/{item.id}", token);
        });
    return Ok(lines);
}
```

**Production takeaway:** **Parallel / PLINQ = CPU-bound in-memory work**; **async = I/O wait without blocking threads** — the chapter comparison table exists because mixing them causes exactly this production outage. See **Program.cs** Sections 11–12 and sibling chapter 04 Async and Await.

---

#### Q4. What is the difference between range partitioning and chunk partitioning? {#05-parallel-programming-q4}

(P) A CPU-bound pricing engine recalculates thousands of in-memory `StockRecord` rows on a 16-core VM shared with other services. A developer caps workers like this:

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?

**Answer:** `Environment.ProcessorCount` on a 16-core box allows 16 concurrent workers for this loop alone, which can starve Kestrel, GC, and other tenants on the same VM. The default `-1` is also often too aggressive on shared infrastructure — you need an explicit cap from configuration plus cooperative cancellation.

- Read `MaxDegreeOfParallelism` from `IOptions<PricingEngineOptions>` (e.g. 4 on a shared 16-core host) — same intent as **Program.cs** Section 5 throttling.
- Wire `CancellationToken` from `IHostApplicationLifetime.ApplicationStopping` or job timeout so deploys and scale-in cancel long batches via `OperationCanceledException` — see Section 6a.
- Leave at least one core for the web tier and system processes unless this worker runs on a dedicated node pool.
- Measure: if CPU is already saturated, raising parallelism does not help; if workers block on locks, lowering parallelism can **improve** throughput.

**Production takeaway:** Parallelism is a **resource budget**, not "use all cores" — Karat expects you to tie `ParallelOptions` to hosting context, not machine topology alone.

---

#### Q5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `WithMergeOptions`). {#05-parallel-programming-q5}

(D) A reconciliation worker must stop processing once cumulative value crosses a credit limit — not process the entire batch. Two implementations were proposed:

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?

**Answer:** Ship **Option B** for a strict cumulative threshold on ordered data — it is deterministic, needs no lock, and stops exactly when the limit is crossed. Option A adds lock contention on every iteration (often erasing parallel benefit) and still does not give strict "process until limit" semantics.

- `Break()` stops **starting** iterations with index **greater than** the break index; lower-index iterations may still be running or not yet started — see **Program.cs** Section 6b (`LowestBreakIteration`, `IsCompleted` false).
- Iterations with index ≤ break index are **not** cancelled — already-started higher-index work may still complete briefly before the loop winds down.
- Parallel order of accumulation is nondeterministic unless the batch order defines business meaning — cumulative credit limits usually require serial order or partitioned serial phases.
- If the batch is huge and per-item CPU work is heavy **and** order does not matter for the limit, consider parallel partial sums then a serial merge — not locked `Break()` on every line.

**Production takeaway:** `Break()` / `Stop()` are cooperative loop control, not transactional cutoffs — for financial thresholds on ordered inventory, prefer serial early exit or map-reduce with a clear merge rule.

---

#### Q6. When is parallelization slower than sequential execution? {#05-parallel-programming-q6}

(R) A dashboard query was "speed up" with PLINQ. Users see wrong top-SKU ordering under load and elevated CPU:

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?

**Answer:** The first query already applies a global `OrderByDescending` — PLINQ merges partitions correctly for that operator, so "wrong ordering" likely comes from **nondeterministic ties** (equal `ReconciledValue`) or from mutating `batch` during the query, not from missing `AsOrdered()`. Adding `AsOrdered()` before `OrderByDescending` forces ordered merge overhead **twice** and hurts CPU without fixing tie-breaking.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `AsOrdered()` + `OrderByDescending` | Redundant ordered merge — higher CPU, little benefit |
| Correctness | Unstable sort on equal keys | "Wrong" top 5 when values tie — need `ThenBy(r => r.Sku)` |
| Scalability | PLINQ on small in-memory batch | Partition/merge cost may exceed serial LINQ — see Section 11 |
| Threading | Shared `batch` mutated elsewhere during query | Undefined results under concurrent writes |

**Fix (priority order):**

1. For dozens/hundreds of rows, use serial LINQ — often faster than PLINQ setup cost on tiny sequences (**Program.cs** tiny-array demo).
2. If CPU work per row is large and count is thousands+, keep `AsParallel()` but drop redundant `AsOrdered()` unless downstream requires input-order preservation without a sort key.
3. Add deterministic tie-break: `.OrderByDescending(r => r.ReconciledValue).ThenBy(r => r.Sku)`.
4. If the pipeline is filter + top-N only on a hot API path, consider pre-indexing or caching — not parallel LINQ on every request.

**Production takeaway:** PLINQ is not free — `AsOrdered()` is for **input-sequence order** in the output, not a substitute for a proper sort key; profile before parallelizing dashboard queries.

---

#### Q7. What types of workloads benefit from PLINQ vs `Parallel.ForEach`? {#05-parallel-programming-q7}

(M) A partitioner was introduced to reduce scheduling overhead on a uniform-cost batch, but throughput dropped on a 4-core machine:

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?

**Answer:** `rangeSize: 1` creates one partition per index — for 12 trivial iterations that means 12 delegate invocations, partition handoffs, and thread-pool scheduling rounds. On small uniform work, that overhead dominates the multiply, so parallel is slower than a serial loop (**Program.cs** Section 11 — five-item demo).

- **Mechanism:** TPL partition granularity trades load balance against scheduling cost — micro-partitions maximize stealing flexibility but explode fixed overhead per chunk.
- **12 cheap ops:** Do not parallelize — serial `for` or simple LINQ is correct.
- **Large uniform batch:** Use default partitioning or `Partitioner.Create(0, count)` without forcing `rangeSize: 1`; let the runtime pick chunk sizes dynamically (**Section 7**).
- **Uneven per-item cost:** `Partitioner.Create(list, loadBalance: true)` for dynamic chunk stealing when row work varies widely.
- **Fixed moderate chunks:** `Partitioner.Create(0, count, rangeSize: 64)` (or similar) when items are uniform and count is in the thousands.

**Production takeaway:** Custom partitioners tune **when** parallel pays off — `rangeSize: 1` on tiny cheap work is a classic "made it parallel therefore faster" mistake Karat embeds in realistic batch code.

---

#### Q8. What are thread-safe requirements when using parallel loops (shared state, locals, aggregation)? {#05-parallel-programming-q8}

_Answer not found._

---

#### Q9. How do you perform parallel aggregation with `lock`, `Interlocked`, or thread-local accumulators? {#05-parallel-programming-q9}

_Answer not found._

---

#### Q10. What is `ParallelLoopResult`, and how do you detect partial failures? {#05-parallel-programming-q10}

_Answer not found._

---

#### Q11. What are ordering guarantees in PLINQ (`AsOrdered`) and their cost? {#05-parallel-programming-q11}

_Answer not found._

---

#### Q12. How does parallel LINQ decide default partition sizes? {#05-parallel-programming-q12}

_Answer not found._

---

#### Q13. What exceptions are thrown from parallel loops (`AggregateException`, inner exceptions)? {#05-parallel-programming-q13}

_Answer not found._

---

#### Q14. How do you combine async I/O with parallel CPU work without blocking the pool? {#05-parallel-programming-q14}

_Answer not found._

---

#### Q15. What are best practices for parallel and async code in server applications? {#05-parallel-programming-q15}

_Answer not found._

---

### 06. Synchronization and Locks

#### Q1. Explain synchronization primitives: `lock`, `Monitor`, `Mutex`, and `Semaphore`/`SemaphoreSlim`. {#06-synchronization-and-locks-q1}

(R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:

**Answer:** `Credit` and `Debit` lock on different objects (`this` vs `typeof(LedgerService)`), so they do not serialize against each other, and `Balance` reads `_balance` without any lock. The singleton shares one field across all requests — you get lost updates and torn reads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Different sync roots per mutation path | Credit and Debit can interleave on `_balance` |
| Correctness | Unsynchronized read on `Balance` | Callers see stale or torn decimal values |
| Design | `lock (this)` / `lock (typeof(T))` | External code can deadlock on the same object; violates ch.06 guidance |
| DI / lifetime | Singleton + mutable balance field | All HTTP requests share one ledger — drift scales with traffic |

**Fix (priority order):**

1. Use one private readonly sync root for all balance access — same object for Credit, Debit, and Balance getter (see `BankAccount._syncRoot` in **Program.cs** Section 3).
2. Lock (or use `Interlocked` only if you refactor to a single `long` cents field) on every read/write of `_balance`.
3. Re-evaluate singleton lifetime — per-tenant or scoped ledger may be required; at minimum document that this service is a process-wide counter, not per-account isolation.
4. Never lock on `this` or `typeof(LedgerService)`.

```csharp
private readonly object _sync = new();
private decimal _balance;

public void Credit(decimal amount) { lock (_sync) { _balance += amount; } }
public void Debit(decimal amount)  { lock (_sync) { _balance -= amount; } }
public decimal Balance { get { lock (_sync) { return _balance; } } }
```

**Production takeaway:** One resource, one sync root — mixed lock targets are a classic "looks synchronized but isn't" defect on singleton services.

---

#### Q2. What is `ReaderWriterLockSlim`, and when is it preferable to a plain `lock`? {#06-synchronization-and-locks-q2}

(R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:

**Answer:** `Transfer` acquires `from` then `to`, while concurrent `Transfer(beta, alpha, …)` acquires in the opposite order — classic circular wait deadlock. Nested locks on account roots that are also locked inside `Deposit`/`Withdraw` compound contention but the hang is the ordering inversion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Opposite lock order on two accounts | Intermittent deadlock — threads block forever |
| Design | Nested lock on `to` while holding `from` | Circular wait when transfers run in both directions |
| Maintainability | `Sleep` inside lock | Extends hold time, increases deadlock window and throughput collapse |

**Fix (priority order):**

1. Acquire locks in a consistent global order — e.g. by `AccountId` string comparison (`TransferSafely` in **Program.cs** Section 10).
2. Call internal mutators (`WithdrawInternal` / `DepositInternal`) only while both locks are held; do not re-enter public methods that lock again on the same root (reentrancy saves you here, but the pattern is fragile).
3. Remove simulated I/O from inside the lock; validate outside or use a short critical section.
4. Optionally use `Monitor.TryEnter` with timeout and retry/backoff when ordering cannot be guaranteed.

```csharp
var first  = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
var second = ReferenceEquals(first, from) ? to : from;
lock (first.SyncRoot) {
    lock (second.SyncRoot) {
        if (from.Balance >= amount) { from.WithdrawInternal(amount); to.DepositInternal(amount); }
    }
}
```

**Production takeaway:** Any time two resources can be locked together, define a total order — Karat expects you to name deadlock before suggesting `lock` everywhere.

---

#### Q3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualResetEventSlim`. {#06-synchronization-and-locks-q3}

(R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:

**Answer:** `RefreshAsync` is async in name only — it blocks a thread inside `lock` via `.Result` on `GetStringAsync`, which can deadlock on ASP.NET's sync context and always starves the thread pool. Holding `lock` during network I/O serializes all refreshes and blocks other readers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetStringAsync` inside lock | Thread-pool starvation; potential ASP.NET deadlock |
| Scalability | Network I/O under `lock` | One refresh blocks all other cache access |
| DI | Singleton + `new HttpClient()` | Socket exhaustion; no DNS refresh — use `IHttpClientFactory` |
| API design | `async Task` method with no `await` | Misleading signature; analyzers may flag CS1998 |

**Fix (priority order):**

1. Remove `.Result` — `await _http.GetStringAsync(...)` **outside** any lock; only lock for the dictionary write (or use `ConcurrentDictionary` from ch.07).
2. Inject `IHttpClientFactory` and create clients via `CreateClient("rates")`.
3. Use `SemaphoreSlim` (not `lock`) if you need to limit concurrent refreshes — `await gate.WaitAsync(ct)` is async-safe.
4. Consider double-checked locking with versioned snapshot replace instead of locking around HTTP.

```csharp
var json = await _http.GetStringAsync($"/rates/{productCode}", ct).ConfigureAwait(false);
var rate = ParseRate(json);
lock (_sync) { _rates[productCode] = rate; }
```

**Production takeaway:** Never combine `lock` + sync-over-async — ch.04 async rules and ch.06 lock rules collide here; pick async coordination primitives.

---

#### Q4. What is `CancellationToken`, and how do you implement cooperative cancellation? {#06-synchronization-and-locks-q4}

(R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:

**Answer:** The code calls `EnterWriteLock` while already holding `EnterReadLock` on the same `ReaderWriterLockSlim`. That lock type is not upgradeable — the thread blocks forever waiting for itself to release the read lock.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Read lock → write lock upgrade on same thread | Self-deadlock on first cache miss — service hangs |
| Correctness | Lazy insert under read lock | Write never acquired; all readers eventually block |
| Design | Check-then-act outside write lock | Duplicate loads possible even after fix — needs double-check |

**Fix (priority order):**

1. Release read lock before taking write lock — exit read, enter write, re-check, insert, exit write, re-enter read for return (classic double-checked locking).
2. Or enter write lock directly when miss is likely; keep read lock only for the happy path (`GetRate` in **Program.cs** Section 7 shows the read-only path).
3. For ASP.NET Core read-heavy caches, consider immutable snapshot replace or `ConcurrentDictionary.GetOrAdd` (ch.07) to avoid manual RW lock upgrade entirely.

```csharp
_rwLock.EnterReadLock();
try {
    if (_rates.TryGetValue(productCode, out var rate)) return rate;
} finally { _rwLock.ExitReadLock(); }

_rwLock.EnterWriteLock();
try {
    if (!_rates.ContainsKey(productCode))
        _rates[productCode] = LoadDefaultFromConfig(productCode);
    return _rates[productCode];
} finally { _rwLock.ExitWriteLock(); }
```

**Production takeaway:** `ReaderWriterLockSlim` does not support lock upgrade — lazy insert requires release-then-acquire or a concurrent collection.

---

#### Q5. Explain deadlocks in multithreading — necessary conditions and prevention strategies. {#06-synchronization-and-locks-q5}

(P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?

**Answer:** Use `SemaphoreSlim(50, 50)` with `WaitAsync`/`Release` for outbound throttling, `Interlocked.Increment` (or `Interlocked.Read` patterns) for the metrics counter, and `volatile bool` or `CancellationToken` for cooperative shutdown. Using `lock` for all three serializes HTTP concurrency to one call at a time and blocks async waits inside the lock.

- **Concurrency cap (50 calls):** `SemaphoreSlim` — counting semaphore matches "N at a time" (Section 6). `WaitAsync` avoids blocking thread-pool threads during I/O.
- **Global request counter:** `Interlocked.Increment(ref _totalRequests)` — single-field atomic math without lock overhead (Section 8).
- **Cooperative shutdown:** `CancellationTokenSource.Cancel()` linked to host shutdown, or `volatile bool _stopRequested` checked in the poller loop (Section 9).

**What breaks with `lock` everywhere:**

- Throttling under `lock` during `await` — cannot await inside `lock`; you'd block one thread per wait, defeating parallelism and risking deadlocks.
- Counter under `lock` works but adds contention on every metric tick; unnecessary when `Interlocked` suffices.
- Stop flag under `lock` on every loop iteration adds latency; visibility is solved by `volatile` or `CancellationToken` without serializing the loop.

**Production takeaway:** Match primitive to concern — `SemaphoreSlim` for N-way gates, `Interlocked` for counters, `CancellationToken`/`volatile` for flags; `lock` is the default exclusive choice, not the only hammer.

---

#### Q6. What are race conditions, and how can they be prevented? {#06-synchronization-and-locks-q6}

(M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:

**Answer:** `_stopRequested` is a plain `bool` without `volatile` or synchronization. The JIT/CPU may cache the field in a register on the worker core, so writes from the UI thread are not guaranteed visible — the loop never observes `true`. This is the visibility problem **Program.cs** Section 9 demonstrates with `volatile bool _stopRequested`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory model | Non-volatile bool flag | Worker may never see stop write across cores |
| Correctness | Cooperative cancel relies on visibility | Production-only hangs after Stop click |
| Design | Ignores `CancellationToken` already passed in | Host shutdown cannot propagate cleanly |

**Fix (priority order):**

1. Mark `_stopRequested` as `volatile bool` **or** prefer checking `externalToken.IsCancellationRequested` only and call `RequestStop` via `CancellationTokenSource.Cancel()`.
2. Wire host shutdown to cancel the same token passed to `Run`.
3. Do not use `Thread.Abort` — cooperative exit only.
4. For complex state, use `lock` around flag read/write or `Interlocked.Exchange` — overkill for a simple stop bit but valid.

```csharp
private volatile bool _stopRequested;

public void RequestStop() => _stopRequested = true;
// Better: inject CancellationToken and drop the bool entirely.
```

**Production takeaway:** Visibility ≠ atomicity — a stop flag needs `volatile`, `CancellationToken`, or a lock; `bool` alone is a release-build Heisenbug.

---

#### Q7. What is the `volatile` keyword, and when does it provide visibility guarantees? {#06-synchronization-and-locks-q7}

(D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:

**Answer:** Ship **Option B** (`ConcurrentDictionary` with snapshot replace on refresh) for a read-heavy ASP.NET Core pricing API. Readers never block writers except briefly during reference swap; no manual RW lock upgrade risk; scales with concurrent pricing requests.

| | A — RW lock + Dictionary | B — ConcurrentDictionary + snapshot replace |
|---|---|---|
| Read path | `EnterReadLock` per request — readers parallel but lock object overhead | Unsynchronized reads on stable dictionary reference |
| Refresh | Must take write lock — blocks all pricing during update | Build new dictionary off-thread; `Interlocked.Exchange` or volatile swap of reference |
| Complexity | Upgrade/lazy-insert traps; must dispose `ReaderWriterLockSlim` | Refresh logic must publish immutable snapshot atomically |
| ASP.NET fit | OK for moderate read load | Better for high RPS read-heavy APIs |

**Trade-offs:**

- Choose **A** when refresh mutates entries in place and you need fine-grained per-key updates with strong in-process RW semantics and moderate traffic.
- Choose **B** when updates are batch/hourly and reads dominate — copy-on-write avoids long write locks and matches options-pattern snapshot refresh.
- Either way: do not expose mutable `Dictionary` without synchronization; document that pricing reads see eventually consistent fees for one refresh window.

**Production takeaway:** Read-heavy web APIs favor immutable snapshot publish over long-lived RW locks — aligns with ch.07 concurrent collections and ch.06 "short critical sections."

---

#### Q8. What is the difference between `volatile` and `lock` for thread safety? {#06-synchronization-and-locks-q8}

_Answer not found._

---

#### Q9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`), and when is it enough without `lock`? {#06-synchronization-and-locks-q9}

_Answer not found._

---

#### Q10. What is `SpinLock`, and when might low-latency spinning beat `lock`? {#06-synchronization-and-locks-q10}

_Answer not found._

---

#### Q11. What is lock ordering, and how does it prevent deadlock? {#06-synchronization-and-locks-q11}

_Answer not found._

---

#### Q12. What is the `Monitor.TryEnter` pattern, and how do timeouts help avoid indefinite blocking? {#06-synchronization-and-locks-q12}

_Answer not found._

---

#### Q13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`) vs blocking `lock` in async code? {#06-synchronization-and-locks-q13}

_Answer not found._

---

#### Q14. What is a priority inversion problem (conceptual), and which primitives exacerbate it? {#06-synchronization-and-locks-q14}

_Answer not found._

---

#### Q15. How do you diagnose deadlocks and lock contention in production (dump analysis, `dotnet-sync`, counters)? {#06-synchronization-and-locks-q15}

_Answer not found._

---

#### Q16. What is thread-safe lazy initialization (`Lazy<T>`, double-checked locking pitfalls)? {#06-synchronization-and-locks-q16}

_Answer not found._

---

### 07. Concurrent Collections

#### Q1. What concurrent collections exist in .NET (`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection`, etc.)? {#07-concurrent-collections-q1}

(R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:

**Answer:** `ApplyPick` performs read-modify-write with separate `TryGetValue` and indexer assignment — not atomic on `ConcurrentDictionary`. Two threads can read the same `current`, both subtract, and one update is lost. `ConcurrentDictionary` makes single operations thread-safe, not compound sequences.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Non-atomic read-modify-write | Lost decrements — negative or inflated on-hand counts |
| API misuse | Indexer after `TryGetValue` | Classic ch.07 anti-pattern documented in Section 8 |
| Correctness | `TryAdd(sku, -quantity)` on miss | Seeds wrong baseline; races with concurrent first pick |

**Fix (priority order):**

1. Replace the sequence with `AddOrUpdate` — atomic insert or transform in one call (see **Program.cs** Section 2).
2. Or use `TryUpdate` in a retry loop with compare-and-swap semantics until success.
3. Enforce business rule (non-negative stock) inside the update delegate or after atomic update with validation/retry.
4. Reserve `lock` only when multiple collections or fields must change together (ch.06).

```csharp
public void ApplyPick(string sku, int quantity) =>
    _onHand.AddOrUpdate(
        sku,
        _ => -quantity,
        (_, current) => current - quantity);
```

**Production takeaway:** `ConcurrentDictionary` does not fix check-then-act — use `AddOrUpdate`/`TryUpdate` or lock for multi-step invariants.

---

#### Q2. When should you use thread-safe collections instead of standard collections plus locks? {#07-concurrent-collections-q2}

(R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:

**Answer:** Under contention, `GetOrAdd` may invoke the factory delegate multiple times for the same key — only one result is stored, but every invocation runs. Side effects (`LoadProduct`, metric increment) are not deduplicated.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrent collection semantics | Factory may run more than once per key | Duplicate DB load and double-counted cache misses |
| Observability | `_metrics.Increment` inside factory | Metrics lie during stampedes |
| Performance | ~40 ms I/O per duplicate factory | Database overload on hot SKU during spike |

**Fix (priority order):**

1. Move side effects out of the factory — factory returns value only; increment metrics after `GetOrAdd` returns if this thread's value was the one stored (hard to detect) **or** use explicit double-check with `SemaphoreSlim` per key / `Lazy<Task<Product>>` per key.
2. Preferred pattern: `GetOrAdd(key, _ => new Lazy<Task<Product>>(() => LoadAsync(key)))` then `await lazy.Value` — one factory constructs the `Lazy`, one `LoadProduct` per key.
3. Or use `IMemoryCache.GetOrCreateAsync` with built-in stampede protection in ASP.NET Core.
4. Document that factory must be idempotent and cheap if you keep raw `GetOrAdd`.

```csharp
var lazy = _cache.GetOrAdd(sku, k => new Lazy<Product>(() => _repo.LoadProduct(k)));
var product = lazy.Value;
```

**Production takeaway:** `GetOrAdd` factory is not "run once" — never put I/O or metrics inside it without extra coordination.

---

#### Q3. What is the difference between `Dictionary<TKey, TValue>` and `ConcurrentDictionary<TKey, TValue>`? {#07-concurrent-collections-q3}

(R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:

**Answer:** The producer never calls `CompleteAdding()`, so `GetConsumingEnumerable` waits forever for more items even after `FetchPendingOrders` finishes. The consumer never exits; `Task.WhenAll` blocks indefinitely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `CompleteAdding()` | Consumer hangs — batch job never completes |
| Shutdown | Cancellation on `Add` only | Consumer may block on `Take` if producer dies without completing |
| Resource | Bounded buffer without completion signal | Ops must kill process — partial ship state |

**Fix (priority order):**

1. Call `buffer.CompleteAdding()` in a `finally` after the producer loop — matches **Program.cs** Section 6 (`PickBufferDemo`).
2. Use `try/finally` on producer so completion fires even on exception; log and rethrow or surface failure.
3. Pass `CancellationToken` to both `GetConsumingEnumerable(ct)` and producer; on cancel, complete adding if not already done.
4. Await both tasks; consider `Task.WhenAll` with timeout for ops visibility.

```csharp
try {
    foreach (var order in _repo.FetchPendingOrders())
        buffer.Add(order, ct);
} finally {
    buffer.CompleteAdding();
}
```

**Production takeaway:** `BlockingCollection` producer-consumer contracts require `CompleteAdding()` — without it, consumers are intentionally infinite loops.

---

#### Q4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `ConcurrentDictionary`? {#07-concurrent-collections-q4}

(R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:

**Answer:** `ConcurrentBag` provides no global FIFO ordering — it uses thread-local lists and `TryTake` prefers items from the calling thread's partition. Ticket order becomes undefined; SLA and fairness break even though `TryTake` "works."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Wrong collection for ordering requirement | VIP tickets may wait while newer local-thread tickets dispatch |
| API semantics | `ConcurrentBag` is for unordered aggregation | Misapplied from ch.07 Section 5 (parallel scan notes) |
| Observability | `Count` approximate under load | `PendingCount` misleading for ops dashboards |

**Fix (priority order):**

1. Replace with `ConcurrentQueue<SupportTicket>` — `Enqueue` / `TryDequeue` preserves FIFO (Section 3).
2. If multiple consumers need blocking when empty, wrap in `BlockingCollection<SupportTicket>` with bounded capacity for back-pressure.
3. Keep `ConcurrentBag` only when order is irrelevant (error aggregation from `Parallel.ForEach`).
4. For priority tiers, use separate queues or a priority queue with appropriate synchronization — not a bag.

**Production takeaway:** Collection choice is a business-rule decision — bag for unordered parallel results, queue for FIFO work dispatch.

---

#### Q5. What is `BlockingCollection<T>`, and how does it implement producer-consumer patterns? {#07-concurrent-collections-q5}

(P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?

**Answer:** Replace the unbounded queue with `BlockingCollection<LogEntry>` backed by `ConcurrentQueue`, set `boundedCapacity` to match writer throughput and memory budget (e.g. 5,000–20,000 entries), and have producers use `TryAdd` with timeout or `Add` with cancellation when full. Writers drain via `GetConsumingEnumerable`; producers call `CompleteAdding()` on shutdown.

- **Bounded buffer:** `new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), boundedCapacity: 10_000)` — producers block or fail when full instead of allocating without limit (Section 6).
- **Writers:** fixed pool of 4 tasks consuming `GetConsumingEnumerable(ct)` — batch flush to disk/network.
- **Producers:** on burst, `TryAdd` returns false → drop with metric, sample, or spill to disk — explicit policy beats OOM.
- **Shutdown:** `CompleteAdding()` when ingress stops; writers drain and exit.

**What breaks without back-pressure:**

- Unbounded `ConcurrentQueue` grows until gen2 LOH pressure and OOM kill the process.
- Silent latency growth — queue depth rises, log delivery lags minutes behind real time.
- GC pauses spike under sustained producer > consumer mismatch.

**Production takeaway:** Concurrent collections remove lock contention; they do not replace flow control — `BlockingCollection` capacity is your memory fuse.

---

#### Q6. What is the difference between bounded and unbounded `BlockingCollection` behavior? {#07-concurrent-collections-q6}

(D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:

**Answer:** Use **Option B (`ConcurrentBag`)** for parallel error collection when order does not matter — avoids serializing every `Add` on a global lock. Use **Option A (lock + List)** when you need deterministic ordering, deduplication, or a single sorted merge with other state under one invariant.

| | Option A — lock + List | Option B — ConcurrentBag |
|---|---|---|
| Contention | Every error serializes on `gate` | Per-thread local lists — low contention |
| Order | Insertion order preserved | Undefined order |
| Best for | Small error volume, ordered API response | High row count, order irrelevant |

**Before returning to API client:**

1. Copy bag to array (`errors.ToArray()` or `ToList()`) — do not enumerate live bag while workers still add unless complete.
2. Sort or dedupe if client expects stable ordering — `OrderBy` on row number if error carries line index.
3. Cap response size — if errors exceed limit, return summary + truncated list with total count.
4. Do not return the mutable bag directly — snapshot first (Section 5 pattern).

**Production takeaway:** `ConcurrentBag` is the right parallel aggregation sink; API contracts still need a sorted, bounded snapshot for humans.

---

#### Q7. How do you use `BlockingCollection` with multiple producers and consumers? {#07-concurrent-collections-q7}

(M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:

**Answer:** Enumeration over `ConcurrentDictionary` while mutators run yields a weakly consistent snapshot — you may miss concurrent adds, see duplicate keys is impossible, but values can change mid-enumeration. `Count` during heavy mutation is approximate and can disagree with the number of entries you enumerate. Sorting during live enumeration produces a report that was never true at any single instant.

- **Snapshot semantics:** Foreach is safe (no `InvalidOperationException`) but not a point-in-time photograph — documented weak consistency.
- **Misleading `Count`:** Can differ from `rows.Count` after loop — do not use for reconciliation dashboards without copying first.
- **Sort mid-mutation:** Order reflects values observed at different times — ops may see phantom shortages.

**When to copy:**

- Financial or ops reconciliation → `ToArray()` or `Select(...).ToList()` under a defined policy, or pause writers briefly.
- Live dashboard OK with "approximate live" → document lag; refresh on interval; prefer `OrderBy` on copied snapshot.
- High-stakes inventory → version counter (`Interlocked`) incremented on each batch publish; report includes version stamp.

```csharp
var snapshot = _onHand.ToArray();
var rows = snapshot
    .Select(kv => new StockRow(kv.Key, kv.Value))
    .OrderBy(r => r.Sku, StringComparer.Ordinal)
    .ToList();
```

**Production takeaway:** Thread-safe enumeration ≠ immutable snapshot — copy then sort for dashboards that must be internally consistent.

---

#### Q8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `ConcurrentBag` — ordering and stealing semantics? {#07-concurrent-collections-q8}

_Answer not found._

---

#### Q9. When is `ConcurrentBag` the wrong choice despite being thread-safe? {#07-concurrent-collections-q9}

_Answer not found._

---

#### Q10. What is `IProducerConsumerCollection<T>` and custom underlying stores for `BlockingCollection`? {#07-concurrent-collections-q10}

_Answer not found._

---

#### Q11. How do concurrent collections compare to locking a `List<T>` for high-contention scenarios? {#07-concurrent-collections-q11}

_Answer not found._

---

#### Q12. What enumeration semantics do concurrent collections provide (weakly consistent iterators)? {#07-concurrent-collections-q12}

_Answer not found._

---

#### Q13. How do you gracefully complete adding to a `BlockingCollection` (`CompleteAdding`)? {#07-concurrent-collections-q13}

_Answer not found._

---

#### Q14. What pitfalls arise when mixing concurrent collections with LINQ? {#07-concurrent-collections-q14}

_Answer not found._

---

#### Q15. When should you use channels (`System.Threading.Channels`) instead of `BlockingCollection` in modern code? {#07-concurrent-collections-q15}

_Answer not found._

---

#### Q16. **`.Result` / `.Wait()` deadlock** — Blocking async on a captured synchronization context (UI, legacy ASP.NET) deadlocks when the continuation needs that same context. {#07-concurrent-collections-q16}

_Answer not found._

---

#### Q17. **`async void` swallows observability** — Exceptions cannot be awaited by callers; use only for event handlers. {#07-concurrent-collections-q17}

_Answer not found._

---

#### Q18. **`Task.Run` for I/O** — Offloading blocking I/O to the pool wastes threads; prefer truly async APIs. {#07-concurrent-collections-q18}

_Answer not found._

---

#### Q19. **Async does not mean threaded** — I/O `await` often completes without extra threads; continuations may run on any pool thread. {#07-concurrent-collections-q19}

_Answer not found._

---

#### Q20. **Unobserved task exceptions** — Faulted tasks that are never awaited may surface later as unobserved exception events. {#07-concurrent-collections-q20}

_Answer not found._

---

#### Q21. **Race on `List<T>`/`Dictionary<,>`** — Even `Add` is not thread-safe; use locks or concurrent collections. {#07-concurrent-collections-q21}

_Answer not found._

---

#### Q22. **`ConfigureAwait(false)` in libraries** — Library code should not marshal back to UI context; app code often needs the default for UI updates. {#07-concurrent-collections-q22}

_Answer not found._

---

#### Q23. **`ValueTask` double-await** — Re-awaiting or concurrent awaits on a pooled `ValueTask` can corrupt state unless documented safe. {#07-concurrent-collections-q23}

_Answer not found._

---

#### Q24. **`TaskCompletionSource` set twice** — Second `TrySet*` calls fail; race to complete can drop results if not coordinated. {#07-concurrent-collections-q24}

_Answer not found._

---

#### Q25. **`BlockingCollection` after `CompleteAdding`** — Adding throws; consumers must drain remaining items correctly. {#07-concurrent-collections-q25}

_Answer not found._

---

#### Q26. **`Interlocked` is not composable** — Check-then-act on complex invariants still needs `lock` or careful CAS loops. {#07-concurrent-collections-q26}

_Answer not found._

---

#### Q27. **`volatile` does not make operations atomic** — `i++` still races even if `i` is volatile. {#07-concurrent-collections-q27}

_Answer not found._

---

#### Q28. **Parallel loop over small work** — Partitioning overhead can make `Parallel.ForEach` slower than sequential code. {#07-concurrent-collections-q28}

_Answer not found._

---

#### Q29. **Shared `Random` is not thread-safe** — Use `Random.Shared` or thread-local RNG in parallel code. {#07-concurrent-collections-q29}

_Answer not found._

---

#### Q30. **Retry without cancellation** — Exponential backoff loops must honor `CancellationToken` and max attempts to avoid runaway delays. {#07-concurrent-collections-q30}

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

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

---

**Answer:**

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

---

**Answer:**

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

---

**Answer:**

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

---

#### Q4. (P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?

---

**Answer:**

**Answer:** Use **cooperative cancellation** with `CancellationToken` linked to the service's `IHostApplicationLifetime.ApplicationStopping` (or a `CancellationTokenSource` cancelled in `StopAsync`). The worker checks `IsCancellationRequested` (or `ThrowIfCancellationRequested`) in its loop, finishes the current aisle/unit of work if needed, releases locks and handles, and exits the delegate normally — then the host `Join`s the thread or awaits a `Task` wrapper.

- Never call `Thread.Abort` — removed from .NET Core because it could leave locks held and invariants broken mid-method (**Program.cs** Section 8).
- Pass the token into the worker at construction/start; cancel once from the shutdown path; block shutdown on `Join(timeout)` and log if the worker exceeds the SLA.
- Keep loop body idempotent at cancellation boundaries — persist checkpoint if stopping mid-batch matters for ops.
- For I/O-bound sweeps, prefer `async`/`await` with the same token (later chapter) so threads are not blocked in `Sleep`.

**Production takeaway:** Production shutdown is "signal, wait, log timeout" — not force-terminate. Dedicated `Thread` is acceptable for a long-lived CPU worker when lifecycle and join semantics are explicit.

---

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

---

**Answer:**

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

---

#### Q6. (D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?

---

**Answer:**

**Answer:** Unbounded `new Thread` per shipment exhausts OS thread limits and memory (default stack reserve per thread), thrashes the scheduler, and makes shutdown join storms impossible. `ThreadPriority` is an OS hint, not a SLA — express lanes are not reliably prioritized across machines. `[ThreadStatic]` metrics break when work moves to thread pool threads or `async` continuations hop threads — counters attach to threads, not logical shipments.

- Cap concurrency: fixed pool of long-lived worker threads **or** `ThreadPool` / `Task` with a bounded `SemaphoreSlim` (e.g., max 8 scanners) — same lifecycle idea (start work, join/complete, cooperative stop) without 1:1 OS threads.
- Express handling belongs in queue priority or business rules, not `ThreadPriority.AboveNormal`.
- Export metrics with labels (`shipment_id`, `worker_id`) via `IMeterFactory`/Prometheus counters — not `[ThreadStatic]` tallies.
- Keep `CancellationToken` on the batch host so service shutdown still cooperates (**Section 8** pattern).
- CPU-bound parallel loops → **05. Parallel Programming**; I/O-bound waits → **04. Async and Await**.

**Production takeaway:** This chapter teaches manual threads for **lifecycle literacy** — production scales with bounded pools, tokens, and synchronized shared state, not unbounded `Thread` construction.

---

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

---

### 02. ThreadPool

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/02. ThreadPool/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

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

---

### 02. ThreadPool

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/02. ThreadPool/`

---

---

#### Q1. (R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?

```csharp
public sealed class InvoiceImportService
{
    public ImportSummary ProcessBatch(int jobCount)
    {
        var results = new int[jobCount];

        for (int i = 0; i < jobCount; i++)
        {
            int jobId = i;
            ThreadPool.QueueUserWorkItem(_ =>
                results[jobId] = InvoiceValidation.ValidateLine(
                    new InvoiceLineJob(jobId, workUnits: 8_000)));
        }

        int valid = results.Count(r => r == 1);
        return new ImportSummary(valid, jobCount);
    }
}
```

---

**Answer:**

**Answer:** `QueueUserWorkItem` returns immediately — the method reads `results` and publishes a summary before pool callbacks finish, so `valid` is often zero or partial. There is no synchronization, and exceptions inside callbacks would be unobserved.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | No wait after queuing work | Race — reads `results` while callbacks still writing |
| Threading | Fire-and-forget pool callbacks | Import summary wrong under any real batch size |
| Observability | No try/finally around callback body | Validation exceptions disappear on pool threads |
| Design | Treats async queue like synchronous loop | Silent data loss in nightly job metrics |

**Fix (priority order):**

1. Block until all callbacks complete — use `CountdownEvent` initialized to `jobCount`, `Signal()` in a `finally` block per callback, then `done.Wait()` before counting (matches **Program.cs** Section 4).
2. Wrap callback work in try/catch and log or aggregate failures — never let pool exceptions vanish.
3. Prefer `Task.Run` + `Task.WhenAll` (ch.03) or `Parallel.For` (ch.05) when you need structured exception propagation and cancellation.
4. Return the summary only after the wait gate completes; add a timeout/cancellation token for hung batches.

```csharp
using var done = new CountdownEvent(jobCount);
for (int i = 0; i < jobCount; i++)
{
    int jobId = i;
    ThreadPool.QueueUserWorkItem(_ =>
    {
        try { results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, 8_000)); }
        catch (Exception ex) { _logger.LogError(ex, "Line {Id}", jobId); }
        finally { done.Signal(); }
    });
}
done.Wait();
```

**Production takeaway:** The chapter's core trap — queuing is not completion. Karat expects you to name `CountdownEvent` (or equivalent) before trusting shared result arrays.

---

---

#### Q2. (R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?

---

**Answer:**

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?

**Answer:** `WaitHandle.WaitAll` on more than 64 handles throws `NotSupportedException` when the calling thread is STA — common in legacy COM/WPF hosts. Even when it succeeds, allocating 500 `ManualResetEvent` objects per batch is expensive compared to one `CountdownEvent`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `WaitAll` with 500 handles on STA thread | Batch fails at scale — works in small demo, fails in prod |
| Resource | One `ManualResetEvent` per job | Handle churn, allocation pressure, dispose overhead |
| Correctness | `Set()` outside `finally` | Exception in `DoWork` leaves event unsignaled → permanent hang |
| Maintainability | Per-job handles for large `jobCount` | Violates tutorial guidance (Section 5 pitfall) |

**Fix (priority order):**

1. Replace per-job events + `WaitAll` with a single `CountdownEvent(jobCount)` and `Signal()` in `finally` ( **Program.cs** Section 4).
2. Move `signal.Set()` into `finally` so failures cannot deadlock the waiter.
3. If you must use events, use `WaitOne` on one gate or `Task.WhenAll` — not `WaitAll` on hundreds of handles.
4. Dispose synchronization primitives via `using` on the countdown/event wrapper.

**Production takeaway:** The tutorial explicitly warns that `WaitAll` is limited to 64 handles on some STA paths — Karat tests whether you read that footnote and default to `CountdownEvent` for large batches.

---

---

#### Q3. (R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.

---

**Answer:**

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.

**Answer:** `.Result` inside a thread-pool callback blocks a worker thread for the entire HTTP wait — sync-over-async on the same pool that ASP.NET, timers, and `Task.Run` share. Under load, workers pile up blocked on I/O while the queue grows, producing apparent "deadlock" or severe latency.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async / threading | `.Result` on `GetStringAsync` in pool callback | Blocks worker threads during I/O — pool starvation |
| Scalability | Many concurrent `EnqueueAuditFetch` calls | Queue backlog; timers and request handling stall |
| Design | CPU pool used for I/O-bound work synchronously | Low CPU, high latency — misread as "need more cores" |
| HTTP | Long-lived `HttpClient` assumed but pattern ignores async model | Same trap as web `.Result` on `GetAsync` |

**Fix (priority order):**

1. Remove sync-over-async — use `async`/`await` end-to-end: `await _httpClient.GetStringAsync(url)` on an async code path (ch.04), not `.Result` on a pool thread.
2. If you must queue, queue async work via `Task.Run` only for CPU-bound segments; I/O should use async I/O that frees workers during waits (IOCP — **Program.cs** Section 9).
3. Bound concurrency with `SemaphoreSlim` or a dedicated channel/worker so audit fetches cannot exhaust the global pool.
4. Register `IHttpClientFactory` in ASP.NET hosts instead of ad-hoc blocking calls.

**Production takeaway:** Thread-pool starvation from `.Result`/`Wait()` is a top Karat theme — the fix is async I/O, not `SetMinThreads`. See foundation gotcha on sync-over-async in the multithreading module.

---

---

#### Q4. (P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

---

**Answer:**

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

**Answer:** Inflating minimum threads on every instance reserves idle workers and I/O threads that consume memory (~1 MB stack each on Windows) without guaranteed throughput gain. Forty pods × 250 workers can oversubscribe an 8-core node and increase context switching while hiding the real bottleneck (slow or blocking callbacks).

- **What breaks:** RAM pressure, scheduler thrashing, false sense of capacity; does not fix blocking code — blocked threads stay blocked regardless of min count.
- **When it helps (sparingly):** Measured cold-start after idle where pool ramp-up latency dominates *short* bursts of queued work — e.g., a known spike right after deploy. Raise min modestly, validate with `GetAvailableThreads` and latency metrics, revert if idle waste grows.
- **Prefer instead:** Fix blocking/sync-over-async, use async I/O, bound parallel fan-out, scale horizontally with sensible concurrency limits — **Program.cs** Section 8 warns to use `SetMinThreads` sparingly.
- **`SetMaxThreads`:** Capping the pool can create unbounded queue backlog — rarely the first lever; fix slow callbacks first.

**Production takeaway:** Karat distinguishes tuning the pool from compensating for bad callbacks — `SetMinThreads(250)` on every pod is a red flag, not a standard template.

---

---

#### Q5. (M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?

```csharp
ThreadPool.GetMaxThreads(out int maxW, out _);
ThreadPool.GetAvailableThreads(out int freeW, out _);
int busyWorkers = maxW - freeW;
_logger.LogWarning("Pool busy workers: {Busy}/{Max}", busyWorkers, maxW);

// Typical callback queued during the spike:
ThreadPool.QueueUserWorkItem(_ =>
{
    using var conn = _db.OpenConnection();          // blocks until pool slot
    Thread.Sleep(120);                              // simulated slow query
    conn.Execute("UPDATE Inventory SET Qty = Qty - 1 WHERE Sku = @sku", sku);
});
```

Explain how worker threads can be "busy" without saturating CPU, and what you would change first.

---

**Answer:**

**Answer:** Worker threads blocked on I/O or locks still count as busy (`max − available`), even when they are not executing CPU instructions — so low CPU with a exhausted-looking pool usually means blocking work on the pool, not insufficient cores.

- The callback opens a DB connection and sleeps — both block the worker without consuming CPU; many such items queue up and starve unrelated pool users (timers, ASP.NET, other `QueueUserWorkItem` work).
- `GetAvailableThreads` is a snapshot — useful for trend logging, not a capacity plan by itself; pair with queue wait time, request latency, and thread-pool starvation counters.
- **First change:** Move blocking DB access to async ADO.NET (`await conn.OpenAsync`, async execute) so workers release during I/O waits (IOCP path — Section 9 preview).
- **Second:** Do not perform long synchronous DB work directly on thread-pool threads — use a bounded dedicated worker or channel with explicit concurrency.
- **Not first:** Buying cores or blindly raising `SetMinThreads` — that multiplies blocked threads, not useful parallelism.

**Production takeaway:** Busy pool + low CPU screams "blocked workers," matching the tutorial's worker vs I/O thread distinction and the async chapter forward reference.

---

---

#### Q6. (D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

---

**Answer:**

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

**Answer:** Ship **B** (pool or modern `Task`/`Parallel` APIs) for this workload — many short, independent CPU jobs are exactly what the thread pool amortizes (**Program.cs** Section 10 comparison table).

- **Throughput:** Manual threads pay OS creation/teardown per job; the pool reuses workers and scales with processor count — the chapter's Stopwatch demo (Section 11) shows pool wins for large batches.
- **Memory:** 2,000 manual threads ≈ gigabytes of default stack reservation; the pool holds a bounded worker set capped by `GetMaxThreads`.
- **Operational risk:** Thread explosion can OOM or thrash the scheduler; pool caps growth and integrates with existing ASP.NET/`Task` infrastructure.
- **When manual `Thread` still fits:** One or few long-lived workers (custom name/priority, foreground lifetime, special apartment/stack) — not 2,000 ephemeral resize jobs.
- **Modern default:** Prefer `Task.Run` + `Task.WhenAll` or `Parallel.For` with `MaxDegreeOfParallelism` for clearer cancellation/exception handling; `QueueUserWorkItem` remains valid for legacy fire-and-forget patterns.

**Production takeaway:** Karat uses batch thumbnail/validation scenarios to test the decision matrix in Section 10 — not reciting "what is a thread pool."

---

---

#### Q7. (R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.

---

### 03. Tasks & Task Parallel Library

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/03. Tasks & Task Parallel Library/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.

**Answer:** The method queues work and returns before validations finish, mutates a non-thread-safe `List<string>` from multiple pool threads without synchronization, and never surfaces callback exceptions — so clients get 202 while data is incomplete and errors are lost.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | Unsynchronized `List<T>.Add` | Corrupted list, lost entries, rare crashes |
| Threading | No completion gate before caller continues | HTTP 202 implies done; work still running |
| Observability | No try/catch in callback | Validation exceptions never logged |
| API contract | Fire-and-forget from request handler | Partial imports, race with downstream steps |
| Design | Shared mutable aggregator on pool threads | Violates Section 6 guidance — prefer per-slot results or concurrent collection |

**Fix (priority order):**

1. Decide contract: if the API must return after validation, wait with `CountdownEvent`/`Task.WhenAll` (and return 200/422 with results); if truly background, enqueue to a durable queue (Azure Service Bus, Hangfire) — not naked `QueueUserWorkItem` from a request.
2. Replace `List<string>` with `ConcurrentBag<string>` or write each failure to `results[i]` then merge after the wait — one writer per index avoids locks (**Program.cs** Section 4 pattern).
3. Wrap callback body in try/catch/finally — log and signal completion in `finally`.
4. Propagate unhandled failures to telemetry (`ILogger`, Application Insights unhandled exception tracking).

**Production takeaway:** Thread-pool callbacks require the same completion, exception, and thread-safety discipline as any parallel code — "queue and forget" from an HTTP handler is a production incident waiting to happen. See QUICK REFERENCE — "Ignoring exceptions in callbacks" and "Exit Main before callbacks finish."

---

### 03. Tasks & Task Parallel Library

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/03. Tasks & Task Parallel Library/`

---

---

#### Q1. (R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

---

**Answer:**

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

**Answer:** The action blocks the request thread with `.Result` on `Task.WhenAll`, which is sync-over-async on ASP.NET Core's thread pool and can cause starvation or deadlocks under concurrency — compounded by wrapping likely I/O-bound validation in `Task.Run`, which wastes pool threads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `Task.WhenAll` | Blocks request thread; sync-over-async under load |
| Scalability | `Task.Run` for validation | Extra thread-pool hop; steals threads from Kestrel |
| API design | Non-async action, no `CancellationToken` | Cannot abort on client disconnect; poor composability |
| Correctness | Unbounded parallel tasks per request | Large batches can exhaust pool or downstream DB |

**Fix (priority order):**

1. Change signature to `async Task<IActionResult> ValidateBatch(..., CancellationToken ct)` and `bool[] results = await Task.WhenAll(validations)`.
2. If validation is I/O-bound, call `_orderValidator.ValidateAsync(id, ct)` directly — no `Task.Run`.
3. Cap concurrency for large batches (`SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or chunked `WhenAll`).
4. Return early or fail fast if `orderIds` exceeds a configured limit.

**Production takeaway:** Passes locally with one user; fails when the pool saturates because every request blocks waiting on `.Result` — the same Karat trap as `.Result` on a single service call, amplified by `WhenAll`. See chapter QUICK REFERENCE — `.Result on UI / ASP.NET request → Deadlock`.

---

---

#### Q2. (R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

---

**Answer:**

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

**Answer:** The parent task completes as soon as the `StartNew` delegate returns — before nested `Task.Run` children finish — so the caller releases the dock slot while picks are still in flight. `Task.Factory.StartNew` without an explicit scheduler also inherits `TaskScheduler.Current`, which can inline work unexpectedly.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Task composition | Nested `Task.Run` not attached to parent | Parent `RanToCompletion` before children; missing picks |
| Scheduler | `StartNew` without `TaskScheduler.Default` | May run on wrong scheduler or inline on caller |
| Lifecycle | Caller `.Wait()` only waits for parent shell | Downstream assumes fulfillment done when it is not |
| Design | Fire-and-forget child tasks | No aggregation, fault propagation, or cancellation |

**Fix (priority order):**

1. Collect child tasks and wait for all: `var picks = order.Lines.Select(line => Task.Run(() => Pick(line))).ToArray(); Task.WhenAll(picks).Wait();` inside the parent — or prefer `async`/`await` with `await Task.WhenAll(picks)` (ch.04).
2. For parent/child lifetime linking, use `TaskCreationOptions.AttachedToParent` with explicit `TaskScheduler.Default` on `StartNew` — as in **Program.cs** Section 12 — only when you truly need attached semantics.
3. Replace outer `Task.Factory.StartNew` with `Task.Run` unless non-default creation options are required.
4. Propagate exceptions: observe all child tasks; a faulted pick must fail the fulfillment operation, not disappear.

**Production takeaway:** Karat tests whether you know a `Task` completing does not mean all nested work finished — unattached children are the TPL equivalent of forgotten `Join` on threads. See **Program.cs** Section 12 and QUICK REFERENCE — "Unattached nested Task.Run in parent."

---

---

#### Q3. (R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

---

**Answer:**

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

**Answer:** The wrapper leaks event handlers on every call, uses throwing `SetResult`/`SetException` instead of `TrySet*`, and has no path to complete the task if the gateway never fires — so callers hang. A second callback can throw `InvalidOperationException` when the task is already completed.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` handlers never removed | Memory leak; duplicate callbacks on reused gateway |
| Correctness | `SetResult` / `SetException` after completion | `InvalidOperationException` on duplicate events |
| Reliability | No timeout or cancel registration | Hung tasks when gateway drops the callback |
| Design | No single-flight guard per charge attempt | Concurrent charges race on one TCS instance |

**Fix (priority order):**

1. Use `TrySetResult`, `TrySetException`, `TrySetCanceled` and unsubscribe handlers in the callback after the first terminal signal.
2. Register `CancellationToken` / `CancelAfter` to call `TrySetCanceled` when the HTTP/client timeout fires.
3. Create one `TaskCompletionSource` per `ChargeAsync` invocation (already implied) — never share across concurrent calls.
4. Optionally wrap with `Task.WhenAny(tcs.Task, timeoutTask)` for defense in depth.

```csharp
void CompleteOnce(Action complete) { if (tcs.TrySetResult(default!)) { /* use TrySet* */ complete(); } }
// Prefer: if (tcs.TrySetResult(result)) { _gateway.PaymentCompleted -= handler; }
```

**Production takeaway:** TCS bridges external callbacks into the task model — production wrappers must be idempotent and self-cleaning. See **Program.cs** Section 13 — `TrySet*` returns false if already completed.

---

---

#### Q4. (R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

---

**Answer:**

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

**Answer:** When the antecedent is faulted, the continuation still runs by default and accessing `antecedent.Result` rethrows — often as `AggregateException` — instead of routing to a fault handler. Unobserved or poorly observed faulted continuations can tear down the process via `TaskScheduler.UnobservedTaskException`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Continuation | Missing `TaskContinuationOptions.OnlyOnRanToCompletion` | Fault path executes success delegate |
| Exception | `antecedent.Result` on faulted task | `AggregateException` at continuation time |
| Observability | No `OnlyOnFaulted` logging branch | Silent or unobserved faults in background chains |
| Style | `.Result` at end of chain | Blocks worker thread; wraps exceptions again |

**Fix (priority order):**

1. Add status filter: `ContinueWith(..., TaskContinuationOptions.OnlyOnRanToCompletion)` for the label step.
2. Add fault handler: `pickTask.ContinueWith(t => Log(t.Exception), TaskContinuationOptions.OnlyOnFaulted)`.
3. Prefer `await pickTask` / `await pickTask.ContinueWith(...)` (ch.04) — compiler preserves stack traces better than manual `ContinueWith`.
4. Replace final `.Result` with `await labelTask` or `GetAwaiter().GetResult()` only at a true sync boundary.

**Production takeaway:** `ContinueWith` defaults to running on any terminal state — Karat expects you to filter with `OnlyOnRanToCompletion` / `OnlyOnFaulted`. See **Program.cs** Sections 11 and 16.

---

---

#### Q5. (P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

---

**Answer:**

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

**Answer:** `Release()` is not in a `finally` block — any exception or cancellation after `WaitAsync` consumes a semaphore slot permanently. After enough failures, all four slots are held and every new pick blocks forever until process restart.

- Wrap the guarded work in `try/finally` and call `_pickerGate.Release()` in `finally` — matching **Program.cs** Section 10 (`ThrottledPick`).
- Prefer `await _pickerGate.WaitAsync(ct)` with the same `CancellationToken` passed to downstream calls so aborting a request releases the wait cleanly.
- Consider `SemaphoreSlim` as a singleton with explicit max count documented; dispose only on application shutdown — not per request.
- Monitor `_pickerGate.CurrentCount` in health checks to detect leak regressions early.

**Production takeaway:** Semaphore throttling is correct for capping concurrent warehouse/API work — but without `finally`, one transient fault becomes a permanent outage. See chapter QUICK REFERENCE — "Forget Release on SemaphoreSlim → Permanent throttle / leak."

---

---

#### Q6. (M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

---

**Answer:**

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

**Answer:** `Task.WhenAny` completes when the first task finishes — it does not cancel or dispose the slower tasks. Losing carrier HTTP calls continue until completion, holding connections, thread-pool slots, and memory under sustained load.

- After `WhenAny`, cancel remaining work with a linked `CancellationTokenSource` passed into each quote call, then `cts.Cancel()` once the winner is chosen.
- If APIs are not cancelable, track in-flight calls and abandon results safely — but still close/dispose `HttpResponseMessage` and respect `IHttpClientFactory` lifetimes.
- Replace blocking `.Result` with `await Task.WhenAny(...)` in an async API so the request thread is not blocked during the race.
- Log slow-loser latency separately — persistent tail latency after "winner found" signals missing cancellation.

**Production takeaway:** `WhenAny` is a coordination primitive, not a resource cleanup primitive — production races must explicitly stop losers. See **Program.cs** Section 9 — first completed task wins; others keep running unless canceled.

---

---

#### Q7. (D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?

---

---

### 04. Async and Await

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/04. Async and Await/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Launching four thousand simultaneous `Task.Run` validations queues thousands of work items at once, spiking thread-pool usage and likely overwhelming the database — `Task.WaitAll` also blocks the orchestrator thread until every task completes, with no backpressure. `Parallel.ForEach` helps CPU-bound validation but is the wrong default if validation is I/O-bound and still needs a concurrency cap for downstream limits.

- Ship chunked or throttled async orchestration: `await Task.WhenAll(batch.Select(o => ValidateAsync(o, ct)))` over batches of 50–200, or use `SemaphoreSlim` / `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` tuned to DB connection limits.
- Use `Task.Run` only for CPU-bound validation logic; I/O-bound checks should be truly async end-to-end (ch.04).
- Prefer `Task.WhenAll` over `Task.WaitAll` in async hosts — composable with cancellation and does not block a precious thread for the entire batch duration.
- Emit metrics: queue depth, validation latency p95, and faulted task count — unobserved faults in nightly jobs can fail silently until morning.

**Production takeaway:** Tasks make it easy to express parallelism; production requires **bounded** parallelism. Karat distinguishes "I can start 4,000 tasks" from "I should start 4,000 tasks." See **Program.cs** Sections 8 (`WhenAll`) and 10 (`SemaphoreSlim` throttle).

---

---

### 04. Async and Await

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/04. Async and Await/`

---

---

#### Q1. (R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?

---

**Answer:**

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?

**Answer:** The endpoint blocks a thread-pool thread twice via `.Result` and `.Wait()` on unfinished Tasks, defeating ASP.NET Core's async I/O model and risking deadlocks when a captured request context prevents continuations from running under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` / `.Wait()` on `Task` | Sync-over-async; blocks thread per request |
| Scalability | Two blocking waits per HTTP call | Thread-pool starvation; throughput collapse under concurrency |
| API design | Non-async endpoint delegate | Cannot accept `CancellationToken`; poor composability with filters/middleware |
| Runtime | Captured `SynchronizationContext` on legacy ASP.NET / some hosts | Potential deadlock — continuation waits for blocked request thread |

**Fix (priority order):**

1. Change the endpoint to `async Task<IResults>` (or `async Task`) and `await` both service calls end-to-end.
2. Thread `CancellationToken` from `HttpContext.RequestAborted` into `FetchMetadataAsync` and downstream I/O.
3. Ensure the service layer stays async all the way — no `.Result` / `.Wait()` anywhere in the call chain.
4. Load-test after the change — thread-pool queue length should stay flat under I/O-bound load.

```csharp
app.MapGet("/reports/{id}", async (ReportService svc, string id, CancellationToken ct) =>
{
    var metadata = await svc.FetchMetadataAsync(id, ct);
    await svc.ProcessReportAsync(metadata, ct);
    return Results.Ok(metadata);
});
```

**Production takeaway:** Passes locally with one user; fails in production when hundreds of requests each block a pool thread waiting on I/O — the classic Karat async trap. See **Program.cs** Section 14 — deadlock pitfalls with `.Result` / `.Wait()`.

---

---

#### Q2. (R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?

---

**Answer:**

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?

**Answer:** `LogExportStarted` is `async void`, so its exception cannot be caught by the caller and propagates through the synchronization context as an unhandled exception — often terminating the worker. The fire-and-forget pipeline Task is also unobserved, so its failures are silent until an unobserved-task handler fires.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `async void` on non-event method | Exceptions crash process; caller cannot await or catch |
| Observability | `_ = RunExportPipelineAsync(...)` discard | Faulted export Task may go unlogged |
| Design | Sync `StartExport` returns before work completes | Host thinks job started successfully; no completion signal |
| Testing | Neither path returns `Task` | Cannot assert success/failure in unit or integration tests |

**Fix (priority order):**

1. Change `LogExportStarted` to `async Task` and await it (or fold logging into the pipeline).
2. Return `Task` from `StartExport` / `RunExportPipelineAsync` and await at the host boundary (BackgroundService, Hangfire job, Azure Function entry).
3. Wrap the pipeline in try/catch with structured logging; rethrow or surface failure to the scheduler.
4. Reserve `async void` strictly for UI event handlers — see **Program.cs** Section 4.

```csharp
public async Task StartExportAsync(string reportId, CancellationToken ct)
{
    await LogExportStartedAsync(reportId, ct);
    await RunExportPipelineAsync(reportId, ct);
}
```

**Production takeaway:** Karat embeds `async void` in service code because it compiles and "works" until the first fault — then the process dies with no useful audit trail.

---

---

#### Q3. (R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?

---

**Answer:**

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?

**Answer:** The UI thread blocks on `.Result` while the async continuation tries to marshal back to the same UI thread (default `await` captures `SynchronizationContext`). The blocked UI thread cannot run the continuation — classic sync-over-async deadlock.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on UI thread | Deadlock — UI blocked, continuation needs UI |
| Library | Missing `ConfigureAwait(false)` | Continuation posts back to captured context |
| Design | Sync wrapper around async API on UI | Forces blocking anti-pattern |
| Runtime | Works in console (no context) | Hides bug until WPF/WinForms/legacy ASP.NET |

**Fix (priority order):**

1. **App:** Make `LoadReportOnStartup` async (`async void` acceptable for UI event) and `await ReportFetcher.GetReportAsync("Q1")`.
2. **Library:** Add `.ConfigureAwait(false)` on every await in reusable code that does not touch UI after the await — see **Program.cs** Section 5.
3. Never expose sync `.Result` / `.Wait()` wrappers from library public APIs.
4. If sync API is unavoidable at a legacy boundary, document it as UI-thread-unsafe and offload with `Task.Run` only as a last resort (still inferior to async all the way).

```csharp
// Library
await Task.Delay(100).ConfigureAwait(false);

// UI
private async void LoadReportOnStartup()
{
    string data = await ReportFetcher.GetReportAsync("Q1");
    ReportLabel.Text = data;
}
```

**Production takeaway:** Console tutorials mask this — Karat pairs library + UI caller to test whether you fix both the blocking call site and context capture in shared code.

---

---

#### Q4. (P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?

---

**Answer:**

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?

**Answer:** `Task.WhenAny` only stops awaiting the loser — the legacy download keeps running in the background after timeout or cancellation. Under repeated cancels, orphaned downloads accumulate, wasting sockets, memory, and upstream quota; callers believe work stopped but it did not.

- **Abandoned work:** When `delay` wins, `download` is never awaited — exception on faulted Task may become unobserved; successful completion is silently ignored but still consumed resources.
- **No cooperative cancel:** Legacy API cannot be interrupted — only the wrapper's wait ends.
- **Resource leaks:** Connection pool exhaustion when many users navigate away or hit client-side timeouts.

**Production pattern:**

1. Document that this is **best-effort abandonment**, not true cancellation — matches **Program.cs** Section 8.
2. Prefer upgrading the legacy client or wrapping at a process boundary you can kill (separate worker, linked `CancellationTokenSource` with timeout).
3. If stuck with `WhenAny`, track in-flight downloads in a registry; optionally use a `CancellationTokenSource` linked to caller token + timeout and log abandoned operation IDs for ops visibility.
4. Surface `OperationCanceledException` to callers but monitor background completion rate — alert if abandoned tasks pile up.
5. For HTTP specifically, migrate to `HttpClient` with `CancellationToken` and `IHttpClientFactory` rather than permanent `WhenAny` shims.

**Production takeaway:** Timeout via `WhenAny` improves caller responsiveness but does not cancel underlying I/O — Karat tests whether you explain the orphan-work trade-off, not just paste the pattern.

---

---

#### Q5. (R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?

---

**Answer:**

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?

**Answer:** The retry loop catches `OperationCanceledException` and retries anyway, and the backoff delay ignores `ct` — so user disconnect or request abort does not stop retries until all attempts and delays finish.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cancellation | `catch (Exception)` swallows `OperationCanceledException` | User cancel ignored; work continues after client left |
| Async | `Task.Delay` without `ct` | Each backoff waits full duration even when token is signaled |
| Correctness | `operation()` may not receive `ct` | Inner fetch keeps running after outer cancel |
| Operability | Up to 5 attempts × multi-second delay | Minutes of wasted upstream calls post-cancel |

**Fix (priority order):**

1. Exclude cancellation from retry filter: `catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)`.
2. Pass token to delay: `await Task.Delay(attempt * 2000, ct)`.
3. Pass `ct` into `FetchReportAsync` inside the lambda so in-flight I/O aborts cooperatively.
4. Optionally distinguish transient faults (`HttpRequestException`, 503) from permanent errors — do not retry 400-class failures.
5. Align with **Program.cs** Section 9 — retry skeleton respects `CancellationToken` and skips `OperationCanceledException`.

```csharp
catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
{
    await Task.Delay(attempt * 2000, ct);
}
```

**Production takeaway:** Retry helpers that catch all exceptions silently extend request lifetime after abort — a common production incident when `RequestAborted` is wired but the retry layer ignores it.

---

---

#### Q6. (D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?

---

**Answer:**

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?

**Answer:** Missing `Release()` in `finally` permanently reduces the semaphore count after the first export — subsequent callers block forever on `WaitAsync`. A static gate also only serializes within one process, not across scaled-out instances.

- **Missing `finally` / `Release`:** One successful path consumes the permit; second export deadlocks all waiters — matches broken mutex usage, not the **Program.cs** Section 10 pattern.
- **Exception before `Release`:** Any fault between `WaitAsync` and manual release leaks a permit — always `try/finally`.
- **Cancellation during `WaitAsync`:** Correctly throws without acquiring — OK; cancellation during write must still run `Release` if wait succeeded.
- **Multi-instance:** Static `SemaphoreSlim` is per-process — two pods write concurrently to shared storage; need distributed lock (blob lease, Redis RedLock, DB advisory lock) for cluster-wide exclusivity.
- **Singleton + static gate:** Redundant — instance field on scoped service or explicit distributed lock is clearer for testability.

**Correct pattern:**

```csharp
await ExportGate.WaitAsync(ct);
try
{
    await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
}
finally
{
    ExportGate.Release();
}
```

For multi-node: replace in-memory gate with storage-level lease; keep `SemaphoreSlim` only for single-process throttling.

**Production takeaway:** Karat stacks async gate bugs — forgotten `Release` causes gradual "exports stopped working" incidents; static gates give false confidence after horizontal scale-out.

---

---

#### Q7. (M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?

---

### 05. Parallel Programming

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/05. Parallel Programming/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?

**Answer:** A `ValueTask` / `ValueTask<T>` must be consumed exactly once — storing it in a field and awaiting it from two concurrent callers violates that rule, producing `InvalidOperationException` when the second await tries to reuse the same instance.

- **Single consumption:** Unlike `Task`, `ValueTask` may wrap a pooled `IValueTaskSource` — double-await is undefined.
- **Concurrency:** Two threads can both see `_cachedCount is null` and create races even before double-await.
- **Preview scope:** **Program.cs** Section 13 warns — do not await a `ValueTask` twice or store for later; consume immediately.

**Safe caching options:**

1. Cache the **`Task<int>`** (or `int` after completion), not the `ValueTask` — e.g. `private Task<int>? _countTask;` assigned once under lock.
2. Use `Lazy<Task<int>>` or `AsyncLazy<T>` pattern for memoized async initialization.
3. If the method often completes synchronously (cache hit), return `ValueTask.FromResult(cachedInt)` on the hot path and only allocate `Task` on miss — but never share one `ValueTask` instance across callers.
4. For concurrent first access, use `SemaphoreSlim` or `lock` around cache population.

```csharp
private Task<int>? _countTask;

public Task<int> GetRowCountAsync(string reportId) =>
    _countTask ??= ComputeCountAsync(reportId).AsTask();
```

**Production takeaway:** `ValueTask` micro-optimizations backfire when treated like cacheable `Task` instances — Karat tests the "consume immediately" rule from the tutorial preview, not just allocation trivia.

---

### 05. Parallel Programming

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/05. Parallel Programming/`

---

---

#### Q1. (R) A nightly warehouse job sums reconciled inventory values in parallel. Finance reports totals that drift from the serial baseline. Review the hot path:

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?

---

**Answer:**

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?

**Answer:** `runningTotal += …` is not atomic — parallel workers read-modify-write the same `decimal` and lose updates, so totals are nondeterministic. It appears to pass when contention is low or the batch is small, then drifts under heavier parallel scheduling.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Threading | Unsynchronized shared `decimal` update | Lost increments → wrong financial totals |
| Correctness | Nondeterministic race | Intermittent failures; hard to reproduce in dev |
| Design | Per-iteration sharing instead of partition/merge | Forces either races or a hot lock |

**Fix (priority order):**

1. Use thread-local aggregation on `Parallel.For` / `Parallel.ForEach` — merge once per worker, not per line (see **Program.cs** Section 2b and `ThreadSafeDecimal.Add`).
2. Prefer `batch.AsParallel().Sum(r => r.ReconciledValue)` for a read-only reduction — PLINQ handles partition/merge internally.
3. If you must share one accumulator, merge under `lock` only in the finalizer delegate — never inside the per-item body.
4. Add a serial golden-sum test in CI for the same batch to catch drift before finance does.

```csharp
decimal total = 0m;
Parallel.ForEach(
    batch,
    () => 0m,
    (record, _, local) => local + record.ReconciledValue,
    local => ThreadSafeDecimal.Add(ref total, local));
```

**Production takeaway:** Parallel speedup requires **no shared writes** or **merge-at-end** patterns — `counter++`-style updates on a shared field are the chapter's core race preview. See **Program.cs** Section 8 and QUICK REFERENCE — "Shared counter++ without sync."

---

---

#### Q2. (R) A teammate parallelizes audit-log line generation for the same SKU batch:

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?

---

**Answer:**

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?

**Answer:** `List<T>` is not thread-safe — concurrent `Add` calls corrupt internal state (exceptions, lost entries, or rare structural damage). Ordering after the fact does not fix the race, and parallel iteration over a non-indexable `IEnumerable` may buffer or enumerate unsafely depending on the source.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Threading | `List<string>.Add` from multiple workers | Corrupted list, `IndexOutOfRangeException`, missing audit lines |
| Scalability | Shared mutable collection as merge point | Workers serialize on internal list growth or fail unpredictably |
| Design | `IEnumerable` source — unknown thread safety | EF/`DbContext`, lazy sequences, or file streams may not support parallel enumeration |

**Fix (priority order):**

1. Replace with `ConcurrentBag<string>` for lock-free per-thread staging, then sort once at the end — matches the audit pattern in **Program.cs** Section 3 (lock shown there; concurrent collection is cleaner at scale).
2. Materialize to `List<StockRecord>` or an array **before** parallel work if the source is deferred or not thread-safe.
3. If order must be deterministic and cheap, consider parallel map into a pre-sized `string[]` by index when indices exist.
4. Never share a `DbContext` or single connection across parallel bodies — load the batch in one scoped query first.

```csharp
var bag = new ConcurrentBag<string>();
Parallel.ForEach(batchList, record =>
{
    bag.Add($"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}");
});
return bag.OrderBy(l => l).ToList();
```

**Production takeaway:** Parallel output collection → `ConcurrentBag` / pre-sized array / thread-local list merged once — not `List<T>` with hope. See **Program.cs** Section 3 — "Shared List<T> is not thread-safe."

---

---

#### Q3. (R) Under load, a reporting endpoint times out and thread-pool starvation alerts fire. Review the "optimization" added to fetch order details:

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?

---

**Answer:**

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?

**Answer:** This is I/O-bound work forced through parallel sync-over-async — each iteration blocks a thread-pool thread waiting on HTTP, while `Parallel.ForEach` multiplies concurrent blocked threads. Combined with an unsynchronized `List<T>`, you get starvation, wrong results, and socket exhaustion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetAwaiter().GetResult()` on async HTTP | Blocks thread-pool threads; sync-over-async |
| Threading | `Parallel.ForEach` on I/O waits | Many blocked workers → ASP.NET request starvation |
| Threading | Unsynchronized `List<string>.Add` | Same race as Q2 — corrupted output |
| Scalability | Parallel API for network-bound batch | Wrong tool — threads idle on I/O instead of CPU work |
| HTTP | Unbounded parallel HTTP from one request | Socket/port pressure; downstream rate-limit trips |

**Fix (priority order):**

1. Make the action async end-to-end: `Parallel.ForEachAsync` ( .NET 6+ ) or `Task.WhenAll` with a `SemaphoreSlim` cap — **not** `Parallel.ForEach` + `.Result`.
2. Use `IHttpClientFactory` and pass `CancellationToken` from `HttpContext.RequestAborted`.
3. Collect results in a thread-safe structure or pre-sized array indexed by position.
4. Cap concurrency (`MaxDegreeOfParallelism` or semaphore) to protect the internal API and thread pool.

```csharp
public async Task<IActionResult> ExportOrders(int[] orderIds, CancellationToken ct)
{
    var lines = new string[orderIds.Length];
    await Parallel.ForEachAsync(
        orderIds.Select((id, i) => (id, i)),
        new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = ct },
        async (item, token) =>
        {
            lines[item.i] = await _httpClient.GetStringAsync($"/internal/orders/{item.id}", token);
        });
    return Ok(lines);
}
```

**Production takeaway:** **Parallel / PLINQ = CPU-bound in-memory work**; **async = I/O wait without blocking threads** — the chapter comparison table exists because mixing them causes exactly this production outage. See **Program.cs** Sections 11–12 and sibling chapter 04 Async and Await.

---

---

#### Q4. (P) A CPU-bound pricing engine recalculates thousands of in-memory `StockRecord` rows on a 16-core VM shared with other services. A developer caps workers like this:

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?

---

**Answer:**

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?

**Answer:** `Environment.ProcessorCount` on a 16-core box allows 16 concurrent workers for this loop alone, which can starve Kestrel, GC, and other tenants on the same VM. The default `-1` is also often too aggressive on shared infrastructure — you need an explicit cap from configuration plus cooperative cancellation.

- Read `MaxDegreeOfParallelism` from `IOptions<PricingEngineOptions>` (e.g. 4 on a shared 16-core host) — same intent as **Program.cs** Section 5 throttling.
- Wire `CancellationToken` from `IHostApplicationLifetime.ApplicationStopping` or job timeout so deploys and scale-in cancel long batches via `OperationCanceledException` — see Section 6a.
- Leave at least one core for the web tier and system processes unless this worker runs on a dedicated node pool.
- Measure: if CPU is already saturated, raising parallelism does not help; if workers block on locks, lowering parallelism can **improve** throughput.

**Production takeaway:** Parallelism is a **resource budget**, not "use all cores" — Karat expects you to tie `ParallelOptions` to hosting context, not machine topology alone.

---

---

#### Q5. (D) A reconciliation worker must stop processing once cumulative value crosses a credit limit — not process the entire batch. Two implementations were proposed:

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?

---

**Answer:**

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?

**Answer:** Ship **Option B** for a strict cumulative threshold on ordered data — it is deterministic, needs no lock, and stops exactly when the limit is crossed. Option A adds lock contention on every iteration (often erasing parallel benefit) and still does not give strict "process until limit" semantics.

- `Break()` stops **starting** iterations with index **greater than** the break index; lower-index iterations may still be running or not yet started — see **Program.cs** Section 6b (`LowestBreakIteration`, `IsCompleted` false).
- Iterations with index ≤ break index are **not** cancelled — already-started higher-index work may still complete briefly before the loop winds down.
- Parallel order of accumulation is nondeterministic unless the batch order defines business meaning — cumulative credit limits usually require serial order or partitioned serial phases.
- If the batch is huge and per-item CPU work is heavy **and** order does not matter for the limit, consider parallel partial sums then a serial merge — not locked `Break()` on every line.

**Production takeaway:** `Break()` / `Stop()` are cooperative loop control, not transactional cutoffs — for financial thresholds on ordered inventory, prefer serial early exit or map-reduce with a clear merge rule.

---

---

#### Q6. (R) A dashboard query was "speed up" with PLINQ. Users see wrong top-SKU ordering under load and elevated CPU:

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?

---

**Answer:**

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?

**Answer:** The first query already applies a global `OrderByDescending` — PLINQ merges partitions correctly for that operator, so "wrong ordering" likely comes from **nondeterministic ties** (equal `ReconciledValue`) or from mutating `batch` during the query, not from missing `AsOrdered()`. Adding `AsOrdered()` before `OrderByDescending` forces ordered merge overhead **twice** and hurts CPU without fixing tie-breaking.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | `AsOrdered()` + `OrderByDescending` | Redundant ordered merge — higher CPU, little benefit |
| Correctness | Unstable sort on equal keys | "Wrong" top 5 when values tie — need `ThenBy(r => r.Sku)` |
| Scalability | PLINQ on small in-memory batch | Partition/merge cost may exceed serial LINQ — see Section 11 |
| Threading | Shared `batch` mutated elsewhere during query | Undefined results under concurrent writes |

**Fix (priority order):**

1. For dozens/hundreds of rows, use serial LINQ — often faster than PLINQ setup cost on tiny sequences (**Program.cs** tiny-array demo).
2. If CPU work per row is large and count is thousands+, keep `AsParallel()` but drop redundant `AsOrdered()` unless downstream requires input-order preservation without a sort key.
3. Add deterministic tie-break: `.OrderByDescending(r => r.ReconciledValue).ThenBy(r => r.Sku)`.
4. If the pipeline is filter + top-N only on a hot API path, consider pre-indexing or caching — not parallel LINQ on every request.

**Production takeaway:** PLINQ is not free — `AsOrdered()` is for **input-sequence order** in the output, not a substitute for a proper sort key; profile before parallelizing dashboard queries.

---

---

#### Q7. (M) A partitioner was introduced to reduce scheduling overhead on a uniform-cost batch, but throughput dropped on a 4-core machine:

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?

---

### 06. Synchronization and Locks

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/06. Synchronization and Locks/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?

**Answer:** `rangeSize: 1` creates one partition per index — for 12 trivial iterations that means 12 delegate invocations, partition handoffs, and thread-pool scheduling rounds. On small uniform work, that overhead dominates the multiply, so parallel is slower than a serial loop (**Program.cs** Section 11 — five-item demo).

- **Mechanism:** TPL partition granularity trades load balance against scheduling cost — micro-partitions maximize stealing flexibility but explode fixed overhead per chunk.
- **12 cheap ops:** Do not parallelize — serial `for` or simple LINQ is correct.
- **Large uniform batch:** Use default partitioning or `Partitioner.Create(0, count)` without forcing `rangeSize: 1`; let the runtime pick chunk sizes dynamically (**Section 7**).
- **Uneven per-item cost:** `Partitioner.Create(list, loadBalance: true)` for dynamic chunk stealing when row work varies widely.
- **Fixed moderate chunks:** `Partitioner.Create(0, count, rangeSize: 64)` (or similar) when items are uniform and count is in the thousands.

**Production takeaway:** Custom partitioners tune **when** parallel pays off — `rangeSize: 1` on tiny cheap work is a classic "made it parallel therefore faster" mistake Karat embeds in realistic batch code.

---

### 06. Synchronization and Locks

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/06. Synchronization and Locks/`

---

---

#### Q1. (R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:

```csharp
public sealed class LedgerService
{
    private decimal _balance;

    public void Credit(decimal amount)
    {
        lock (this)
        {
            _balance += amount;
        }
    }

    public void Debit(decimal amount)
    {
        lock (typeof(LedgerService))
        {
            _balance -= amount;
        }
    }

    public decimal Balance => _balance; // read without synchronization
}
```

What fails in production, and what is the prioritized fix?

---

**Answer:**

**Answer:** `Credit` and `Debit` lock on different objects (`this` vs `typeof(LedgerService)`), so they do not serialize against each other, and `Balance` reads `_balance` without any lock. The singleton shares one field across all requests — you get lost updates and torn reads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Different sync roots per mutation path | Credit and Debit can interleave on `_balance` |
| Correctness | Unsynchronized read on `Balance` | Callers see stale or torn decimal values |
| Design | `lock (this)` / `lock (typeof(T))` | External code can deadlock on the same object; violates ch.06 guidance |
| DI / lifetime | Singleton + mutable balance field | All HTTP requests share one ledger — drift scales with traffic |

**Fix (priority order):**

1. Use one private readonly sync root for all balance access — same object for Credit, Debit, and Balance getter (see `BankAccount._syncRoot` in **Program.cs** Section 3).
2. Lock (or use `Interlocked` only if you refactor to a single `long` cents field) on every read/write of `_balance`.
3. Re-evaluate singleton lifetime — per-tenant or scoped ledger may be required; at minimum document that this service is a process-wide counter, not per-account isolation.
4. Never lock on `this` or `typeof(LedgerService)`.

```csharp
private readonly object _sync = new();
private decimal _balance;

public void Credit(decimal amount) { lock (_sync) { _balance += amount; } }
public void Debit(decimal amount)  { lock (_sync) { _balance -= amount; } }
public decimal Balance { get { lock (_sync) { return _balance; } } }
```

**Production takeaway:** One resource, one sync root — mixed lock targets are a classic "looks synchronized but isn't" defect on singleton services.

---

---

#### Q2. (R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:

```csharp
public static void Transfer(BankAccount from, BankAccount to, decimal amount)
{
    lock (from.SyncRoot)
    {
        Thread.Sleep(15); // simulate ledger validation
        lock (to.SyncRoot)
        {
            if (from.Balance >= amount)
            {
                from.WithdrawInternal(amount);
                to.DepositInternal(amount);
            }
        }
    }
}

// Worker pool runs Transfer(alpha, beta, 40) and Transfer(beta, alpha, 25) concurrently.
// BankAccount.Deposit/Withdraw each lock the same private SyncRoot internally.
```

What causes the hang, and how do you fix it without removing multi-account transfers?

---

**Answer:**

**Answer:** `Transfer` acquires `from` then `to`, while concurrent `Transfer(beta, alpha, …)` acquires in the opposite order — classic circular wait deadlock. Nested locks on account roots that are also locked inside `Deposit`/`Withdraw` compound contention but the hang is the ordering inversion.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Opposite lock order on two accounts | Intermittent deadlock — threads block forever |
| Design | Nested lock on `to` while holding `from` | Circular wait when transfers run in both directions |
| Maintainability | `Sleep` inside lock | Extends hold time, increases deadlock window and throughput collapse |

**Fix (priority order):**

1. Acquire locks in a consistent global order — e.g. by `AccountId` string comparison (`TransferSafely` in **Program.cs** Section 10).
2. Call internal mutators (`WithdrawInternal` / `DepositInternal`) only while both locks are held; do not re-enter public methods that lock again on the same root (reentrancy saves you here, but the pattern is fragile).
3. Remove simulated I/O from inside the lock; validate outside or use a short critical section.
4. Optionally use `Monitor.TryEnter` with timeout and retry/backoff when ordering cannot be guaranteed.

```csharp
var first  = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
var second = ReferenceEquals(first, from) ? to : from;
lock (first.SyncRoot) {
    lock (second.SyncRoot) {
        if (from.Balance >= amount) { from.WithdrawInternal(amount); to.DepositInternal(amount); }
    }
}
```

**Production takeaway:** Any time two resources can be locked together, define a total order — Karat expects you to name deadlock before suggesting `lock` everywhere.

---

---

#### Q3. (R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:

```csharp
public sealed class RateCacheWarmer
{
    private readonly object _sync = new();
    private readonly Dictionary<string, decimal> _rates = new();
    private readonly HttpClient _http = new();

    public async Task RefreshAsync(string productCode, CancellationToken ct)
    {
        lock (_sync)
        {
            var json = _http.GetStringAsync($"/rates/{productCode}", ct).Result;
            _rates[productCode] = ParseRate(json);
        }
    }
}
```

What are the problems (compile-time where applicable, runtime, and scalability), and how do you fix them in priority order?

---

**Answer:**

**Answer:** `RefreshAsync` is async in name only — it blocks a thread inside `lock` via `.Result` on `GetStringAsync`, which can deadlock on ASP.NET's sync context and always starves the thread pool. Holding `lock` during network I/O serializes all refreshes and blocks other readers.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetStringAsync` inside lock | Thread-pool starvation; potential ASP.NET deadlock |
| Scalability | Network I/O under `lock` | One refresh blocks all other cache access |
| DI | Singleton + `new HttpClient()` | Socket exhaustion; no DNS refresh — use `IHttpClientFactory` |
| API design | `async Task` method with no `await` | Misleading signature; analyzers may flag CS1998 |

**Fix (priority order):**

1. Remove `.Result` — `await _http.GetStringAsync(...)` **outside** any lock; only lock for the dictionary write (or use `ConcurrentDictionary` from ch.07).
2. Inject `IHttpClientFactory` and create clients via `CreateClient("rates")`.
3. Use `SemaphoreSlim` (not `lock`) if you need to limit concurrent refreshes — `await gate.WaitAsync(ct)` is async-safe.
4. Consider double-checked locking with versioned snapshot replace instead of locking around HTTP.

```csharp
var json = await _http.GetStringAsync($"/rates/{productCode}", ct).ConfigureAwait(false);
var rate = ParseRate(json);
lock (_sync) { _rates[productCode] = rate; }
```

**Production takeaway:** Never combine `lock` + sync-over-async — ch.04 async rules and ch.06 lock rules collide here; pick async coordination primitives.

---

---

#### Q4. (R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:

```csharp
public decimal GetOrAddRate(string productCode)
{
    _rwLock.EnterReadLock();
    try
    {
        if (!_rates.ContainsKey(productCode))
        {
            _rwLock.EnterWriteLock();
            try
            {
                _rates[productCode] = LoadDefaultFromConfig(productCode);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        return _rates[productCode];
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}
```

What breaks, and what is the correct locking pattern for lazy insert under concurrent readers?

---

**Answer:**

**Answer:** The code calls `EnterWriteLock` while already holding `EnterReadLock` on the same `ReaderWriterLockSlim`. That lock type is not upgradeable — the thread blocks forever waiting for itself to release the read lock.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Read lock → write lock upgrade on same thread | Self-deadlock on first cache miss — service hangs |
| Correctness | Lazy insert under read lock | Write never acquired; all readers eventually block |
| Design | Check-then-act outside write lock | Duplicate loads possible even after fix — needs double-check |

**Fix (priority order):**

1. Release read lock before taking write lock — exit read, enter write, re-check, insert, exit write, re-enter read for return (classic double-checked locking).
2. Or enter write lock directly when miss is likely; keep read lock only for the happy path (`GetRate` in **Program.cs** Section 7 shows the read-only path).
3. For ASP.NET Core read-heavy caches, consider immutable snapshot replace or `ConcurrentDictionary.GetOrAdd` (ch.07) to avoid manual RW lock upgrade entirely.

```csharp
_rwLock.EnterReadLock();
try {
    if (_rates.TryGetValue(productCode, out var rate)) return rate;
} finally { _rwLock.ExitReadLock(); }

_rwLock.EnterWriteLock();
try {
    if (!_rates.ContainsKey(productCode))
        _rates[productCode] = LoadDefaultFromConfig(productCode);
    return _rates[productCode];
} finally { _rwLock.ExitWriteLock(); }
```

**Production takeaway:** `ReaderWriterLockSlim` does not support lock upgrade — lazy insert requires release-then-acquire or a concurrent collection.

---

---

#### Q5. (P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?

---

**Answer:**

**Answer:** Use `SemaphoreSlim(50, 50)` with `WaitAsync`/`Release` for outbound throttling, `Interlocked.Increment` (or `Interlocked.Read` patterns) for the metrics counter, and `volatile bool` or `CancellationToken` for cooperative shutdown. Using `lock` for all three serializes HTTP concurrency to one call at a time and blocks async waits inside the lock.

- **Concurrency cap (50 calls):** `SemaphoreSlim` — counting semaphore matches "N at a time" (Section 6). `WaitAsync` avoids blocking thread-pool threads during I/O.
- **Global request counter:** `Interlocked.Increment(ref _totalRequests)` — single-field atomic math without lock overhead (Section 8).
- **Cooperative shutdown:** `CancellationTokenSource.Cancel()` linked to host shutdown, or `volatile bool _stopRequested` checked in the poller loop (Section 9).

**What breaks with `lock` everywhere:**

- Throttling under `lock` during `await` — cannot await inside `lock`; you'd block one thread per wait, defeating parallelism and risking deadlocks.
- Counter under `lock` works but adds contention on every metric tick; unnecessary when `Interlocked` suffices.
- Stop flag under `lock` on every loop iteration adds latency; visibility is solved by `volatile` or `CancellationToken` without serializing the loop.

**Production takeaway:** Match primitive to concern — `SemaphoreSlim` for N-way gates, `Interlocked` for counters, `CancellationToken`/`volatile` for flags; `lock` is the default exclusive choice, not the only hammer.

---

---

#### Q6. (M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:

```csharp
public sealed class VaultScanWorker
{
    private bool _stopRequested;

    public void Run(CancellationToken externalToken)
    {
        while (!externalToken.IsCancellationRequested && !_stopRequested)
        {
            ScanNextBatch();
            Thread.Sleep(40);
        }
    }

    public void RequestStop()
    {
        _stopRequested = true;
    }
}
```

Why does `_stopRequested` fail to stop the loop reliably across CPU cores, and what is the production-safe fix?

---

**Answer:**

**Answer:** `_stopRequested` is a plain `bool` without `volatile` or synchronization. The JIT/CPU may cache the field in a register on the worker core, so writes from the UI thread are not guaranteed visible — the loop never observes `true`. This is the visibility problem **Program.cs** Section 9 demonstrates with `volatile bool _stopRequested`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory model | Non-volatile bool flag | Worker may never see stop write across cores |
| Correctness | Cooperative cancel relies on visibility | Production-only hangs after Stop click |
| Design | Ignores `CancellationToken` already passed in | Host shutdown cannot propagate cleanly |

**Fix (priority order):**

1. Mark `_stopRequested` as `volatile bool` **or** prefer checking `externalToken.IsCancellationRequested` only and call `RequestStop` via `CancellationTokenSource.Cancel()`.
2. Wire host shutdown to cancel the same token passed to `Run`.
3. Do not use `Thread.Abort` — cooperative exit only.
4. For complex state, use `lock` around flag read/write or `Interlocked.Exchange` — overkill for a simple stop bit but valid.

```csharp
private volatile bool _stopRequested;

public void RequestStop() => _stopRequested = true;
// Better: inject CancellationToken and drop the bool entirely.
```

**Production takeaway:** Visibility ≠ atomicity — a stop flag needs `volatile`, `CancellationToken`, or a lock; `bool` alone is a release-build Heisenbug.

---

---

#### Q7. (D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:

**A.** `Dictionary<string, decimal>` + `ReaderWriterLockSlim` (manual read/write locks)  
**B.** `ConcurrentDictionary<string, decimal>` with snapshot replace on refresh

Which do you ship for a read-heavy ASP.NET Core pricing API, and what trade-offs drive the choice?

---

### 07. Concurrent Collections

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/07. Concurrent Collections/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Ship **Option B** (`ConcurrentDictionary` with snapshot replace on refresh) for a read-heavy ASP.NET Core pricing API. Readers never block writers except briefly during reference swap; no manual RW lock upgrade risk; scales with concurrent pricing requests.

| | A — RW lock + Dictionary | B — ConcurrentDictionary + snapshot replace |
|---|---|---|
| Read path | `EnterReadLock` per request — readers parallel but lock object overhead | Unsynchronized reads on stable dictionary reference |
| Refresh | Must take write lock — blocks all pricing during update | Build new dictionary off-thread; `Interlocked.Exchange` or volatile swap of reference |
| Complexity | Upgrade/lazy-insert traps; must dispose `ReaderWriterLockSlim` | Refresh logic must publish immutable snapshot atomically |
| ASP.NET fit | OK for moderate read load | Better for high RPS read-heavy APIs |

**Trade-offs:**

- Choose **A** when refresh mutates entries in place and you need fine-grained per-key updates with strong in-process RW semantics and moderate traffic.
- Choose **B** when updates are batch/hourly and reads dominate — copy-on-write avoids long write locks and matches options-pattern snapshot refresh.
- Either way: do not expose mutable `Dictionary` without synchronization; document that pricing reads see eventually consistent fees for one refresh window.

**Production takeaway:** Read-heavy web APIs favor immutable snapshot publish over long-lived RW locks — aligns with ch.07 concurrent collections and ch.06 "short critical sections."

---

### 07. Concurrent Collections

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/07. Concurrent Collections/`

---

---

#### Q1. (R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:

```csharp
public sealed class StockLedger
{
    private readonly ConcurrentDictionary<string, int> _onHand = new();

    public void ApplyPick(string sku, int quantity)
    {
        if (_onHand.TryGetValue(sku, out int current))
            _onHand[sku] = current - quantity;
        else
            _onHand.TryAdd(sku, -quantity);
    }

    public int GetOnHand(string sku) =>
        _onHand.TryGetValue(sku, out int v) ? v : 0;
}
```

What fails under concurrent picks on the same SKU, and how do you fix it without wrapping every call in `lock`?

---

**Answer:**

**Answer:** `ApplyPick` performs read-modify-write with separate `TryGetValue` and indexer assignment — not atomic on `ConcurrentDictionary`. Two threads can read the same `current`, both subtract, and one update is lost. `ConcurrentDictionary` makes single operations thread-safe, not compound sequences.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Non-atomic read-modify-write | Lost decrements — negative or inflated on-hand counts |
| API misuse | Indexer after `TryGetValue` | Classic ch.07 anti-pattern documented in Section 8 |
| Correctness | `TryAdd(sku, -quantity)` on miss | Seeds wrong baseline; races with concurrent first pick |

**Fix (priority order):**

1. Replace the sequence with `AddOrUpdate` — atomic insert or transform in one call (see **Program.cs** Section 2).
2. Or use `TryUpdate` in a retry loop with compare-and-swap semantics until success.
3. Enforce business rule (non-negative stock) inside the update delegate or after atomic update with validation/retry.
4. Reserve `lock` only when multiple collections or fields must change together (ch.06).

```csharp
public void ApplyPick(string sku, int quantity) =>
    _onHand.AddOrUpdate(
        sku,
        _ => -quantity,
        (_, current) => current - quantity);
```

**Production takeaway:** `ConcurrentDictionary` does not fix check-then-act — use `AddOrUpdate`/`TryUpdate` or lock for multi-step invariants.

---

---

#### Q2. (R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:

```csharp
public sealed class ProductCache
{
    private readonly ConcurrentDictionary<string, Product> _cache = new();
    private readonly IProductRepository _repo;
    private readonly IMetrics _metrics;

    public Product Get(string sku) =>
        _cache.GetOrAdd(sku, key =>
        {
            _metrics.Increment("catalog.cache_miss");
            var row = _repo.LoadProduct(key);   // ~40 ms I/O
            return row;
        });
}
```

What concurrent-collection behavior causes duplicate work, and what pattern keeps the factory side-effect safe?

---

**Answer:**

**Answer:** Under contention, `GetOrAdd` may invoke the factory delegate multiple times for the same key — only one result is stored, but every invocation runs. Side effects (`LoadProduct`, metric increment) are not deduplicated.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrent collection semantics | Factory may run more than once per key | Duplicate DB load and double-counted cache misses |
| Observability | `_metrics.Increment` inside factory | Metrics lie during stampedes |
| Performance | ~40 ms I/O per duplicate factory | Database overload on hot SKU during spike |

**Fix (priority order):**

1. Move side effects out of the factory — factory returns value only; increment metrics after `GetOrAdd` returns if this thread's value was the one stored (hard to detect) **or** use explicit double-check with `SemaphoreSlim` per key / `Lazy<Task<Product>>` per key.
2. Preferred pattern: `GetOrAdd(key, _ => new Lazy<Task<Product>>(() => LoadAsync(key)))` then `await lazy.Value` — one factory constructs the `Lazy`, one `LoadProduct` per key.
3. Or use `IMemoryCache.GetOrCreateAsync` with built-in stampede protection in ASP.NET Core.
4. Document that factory must be idempotent and cheap if you keep raw `GetOrAdd`.

```csharp
var lazy = _cache.GetOrAdd(sku, k => new Lazy<Product>(() => _repo.LoadProduct(k)));
var product = lazy.Value;
```

**Production takeaway:** `GetOrAdd` factory is not "run once" — never put I/O or metrics inside it without extra coordination.

---

---

#### Q3. (R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:

```csharp
public async Task RunBatchAsync(CancellationToken ct)
{
    var buffer = new BlockingCollection<Order>(boundedCapacity: 200);

    var ingest = Task.Run(() =>
    {
        foreach (Order order in _repo.FetchPendingOrders())
            buffer.Add(order, ct);
        // producer loop ends here — no CompleteAdding()
    }, ct);

    var ship = Task.Run(() =>
    {
        foreach (Order order in buffer.GetConsumingEnumerable(ct))
            _shipper.Dispatch(order);
    }, ct);

    await Task.WhenAll(ingest, ship);
}
```

What keeps `GetConsumingEnumerable` from terminating, and what else should you verify for graceful shutdown under cancellation?

---

**Answer:**

**Answer:** The producer never calls `CompleteAdding()`, so `GetConsumingEnumerable` waits forever for more items even after `FetchPendingOrders` finishes. The consumer never exits; `Task.WhenAll` blocks indefinitely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `CompleteAdding()` | Consumer hangs — batch job never completes |
| Shutdown | Cancellation on `Add` only | Consumer may block on `Take` if producer dies without completing |
| Resource | Bounded buffer without completion signal | Ops must kill process — partial ship state |

**Fix (priority order):**

1. Call `buffer.CompleteAdding()` in a `finally` after the producer loop — matches **Program.cs** Section 6 (`PickBufferDemo`).
2. Use `try/finally` on producer so completion fires even on exception; log and rethrow or surface failure.
3. Pass `CancellationToken` to both `GetConsumingEnumerable(ct)` and producer; on cancel, complete adding if not already done.
4. Await both tasks; consider `Task.WhenAll` with timeout for ops visibility.

```csharp
try {
    foreach (var order in _repo.FetchPendingOrders())
        buffer.Add(order, ct);
} finally {
    buffer.CompleteAdding();
}
```

**Production takeaway:** `BlockingCollection` producer-consumer contracts require `CompleteAdding()` — without it, consumers are intentionally infinite loops.

---

---

#### Q4. (R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:

```csharp
public sealed class TicketDispatcher
{
    private readonly ConcurrentBag<SupportTicket> _inbox = new();

    public void Enqueue(SupportTicket ticket) => _inbox.Add(ticket);

    public bool TryDispatchNext(out SupportTicket ticket)
    {
        return _inbox.TryTake(out ticket);
    }

    public int PendingCount => _inbox.Count;
}
```

What ordering guarantees does this give in production, and which concurrent type fits FIFO fairness?

---

**Answer:**

**Answer:** `ConcurrentBag` provides no global FIFO ordering — it uses thread-local lists and `TryTake` prefers items from the calling thread's partition. Ticket order becomes undefined; SLA and fairness break even though `TryTake` "works."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Wrong collection for ordering requirement | VIP tickets may wait while newer local-thread tickets dispatch |
| API semantics | `ConcurrentBag` is for unordered aggregation | Misapplied from ch.07 Section 5 (parallel scan notes) |
| Observability | `Count` approximate under load | `PendingCount` misleading for ops dashboards |

**Fix (priority order):**

1. Replace with `ConcurrentQueue<SupportTicket>` — `Enqueue` / `TryDequeue` preserves FIFO (Section 3).
2. If multiple consumers need blocking when empty, wrap in `BlockingCollection<SupportTicket>` with bounded capacity for back-pressure.
3. Keep `ConcurrentBag` only when order is irrelevant (error aggregation from `Parallel.ForEach`).
4. For priority tiers, use separate queues or a priority queue with appropriate synchronization — not a bag.

**Production takeaway:** Collection choice is a business-rule decision — bag for unordered parallel results, queue for FIFO work dispatch.

---

---

#### Q5. (P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?

---

**Answer:**

**Answer:** Replace the unbounded queue with `BlockingCollection<LogEntry>` backed by `ConcurrentQueue`, set `boundedCapacity` to match writer throughput and memory budget (e.g. 5,000–20,000 entries), and have producers use `TryAdd` with timeout or `Add` with cancellation when full. Writers drain via `GetConsumingEnumerable`; producers call `CompleteAdding()` on shutdown.

- **Bounded buffer:** `new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), boundedCapacity: 10_000)` — producers block or fail when full instead of allocating without limit (Section 6).
- **Writers:** fixed pool of 4 tasks consuming `GetConsumingEnumerable(ct)` — batch flush to disk/network.
- **Producers:** on burst, `TryAdd` returns false → drop with metric, sample, or spill to disk — explicit policy beats OOM.
- **Shutdown:** `CompleteAdding()` when ingress stops; writers drain and exit.

**What breaks without back-pressure:**

- Unbounded `ConcurrentQueue` grows until gen2 LOH pressure and OOM kill the process.
- Silent latency growth — queue depth rises, log delivery lags minutes behind real time.
- GC pauses spike under sustained producer > consumer mismatch.

**Production takeaway:** Concurrent collections remove lock contention; they do not replace flow control — `BlockingCollection` capacity is your memory fuse.

---

---

#### Q6. (D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:

**Option A — lock + List**

```csharp
var errors = new List<string>();
var gate = new object();
Parallel.ForEach(rows, row =>
{
    if (!Validate(row, out string msg))
        lock (gate) { errors.Add(msg); }
});
```

**Option B — ConcurrentBag**

```csharp
var errors = new ConcurrentBag<string>();
Parallel.ForEach(rows, row =>
{
    if (!Validate(row, out string msg))
        errors.Add(msg);
});
```

When is each appropriate, and what must you do before returning errors to the API client?

---

**Answer:**

**Answer:** Use **Option B (`ConcurrentBag`)** for parallel error collection when order does not matter — avoids serializing every `Add` on a global lock. Use **Option A (lock + List)** when you need deterministic ordering, deduplication, or a single sorted merge with other state under one invariant.

| | Option A — lock + List | Option B — ConcurrentBag |
|---|---|---|
| Contention | Every error serializes on `gate` | Per-thread local lists — low contention |
| Order | Insertion order preserved | Undefined order |
| Best for | Small error volume, ordered API response | High row count, order irrelevant |

**Before returning to API client:**

1. Copy bag to array (`errors.ToArray()` or `ToList()`) — do not enumerate live bag while workers still add unless complete.
2. Sort or dedupe if client expects stable ordering — `OrderBy` on row number if error carries line index.
3. Cap response size — if errors exceed limit, return summary + truncated list with total count.
4. Do not return the mutable bag directly — snapshot first (Section 5 pattern).

**Production takeaway:** `ConcurrentBag` is the right parallel aggregation sink; API contracts still need a sorted, bounded snapshot for humans.

---

---

#### Q7. (M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:

```csharp
public IReadOnlyList<StockRow> GetLiveReport()
{
    var rows = new List<StockRow>();
    foreach (var kv in _onHand)   // concurrent adds/updates still running
    {
        rows.Add(new StockRow(kv.Key, kv.Value));
    }
    rows.Sort((a, b) => string.Compare(a.Sku, b.Sku, StringComparison.Ordinal));
    return rows;
}
```

What snapshot semantics does enumeration provide under mutation, and when is `Count` or a copied snapshot misleading for ops dashboards?

**Answer:**

**Answer:** Enumeration over `ConcurrentDictionary` while mutators run yields a weakly consistent snapshot — you may miss concurrent adds, see duplicate keys is impossible, but values can change mid-enumeration. `Count` during heavy mutation is approximate and can disagree with the number of entries you enumerate. Sorting during live enumeration produces a report that was never true at any single instant.

- **Snapshot semantics:** Foreach is safe (no `InvalidOperationException`) but not a point-in-time photograph — documented weak consistency.
- **Misleading `Count`:** Can differ from `rows.Count` after loop — do not use for reconciliation dashboards without copying first.
- **Sort mid-mutation:** Order reflects values observed at different times — ops may see phantom shortages.

**When to copy:**

- Financial or ops reconciliation → `ToArray()` or `Select(...).ToList()` under a defined policy, or pause writers briefly.
- Live dashboard OK with "approximate live" → document lag; refresh on interval; prefer `OrderBy` on copied snapshot.
- High-stakes inventory → version counter (`Interlocked`) incremented on each batch publish; report includes version stamp.

```csharp
var snapshot = _onHand.ToArray();
var rows = snapshot
    .Select(kv => new StockRow(kv.Key, kv.Value))
    .OrderBy(r => r.Sku, StringComparer.Ordinal)
    .ToList();
```

**Production takeaway:** Thread-safe enumeration ≠ immutable snapshot — copy then sort for dashboards that must be internally consistent.

---
