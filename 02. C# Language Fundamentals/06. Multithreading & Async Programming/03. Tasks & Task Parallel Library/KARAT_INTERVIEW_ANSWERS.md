# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/03. Tasks & Task Parallel Library/`

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

**Answer:** `Release()` is not in a `finally` block — any exception or cancellation after `WaitAsync` consumes a semaphore slot permanently. After enough failures, all four slots are held and every new pick blocks forever until process restart.

- Wrap the guarded work in `try/finally` and call `_pickerGate.Release()` in `finally` — matching **Program.cs** Section 10 (`ThrottledPick`).
- Prefer `await _pickerGate.WaitAsync(ct)` with the same `CancellationToken` passed to downstream calls so aborting a request releases the wait cleanly.
- Consider `SemaphoreSlim` as a singleton with explicit max count documented; dispose only on application shutdown — not per request.
- Monitor `_pickerGate.CurrentCount` in health checks to detect leak regressions early.

**Production takeaway:** Semaphore throttling is correct for capping concurrent warehouse/API work — but without `finally`, one transient fault becomes a permanent outage. See chapter QUICK REFERENCE — "Forget Release on SemaphoreSlim → Permanent throttle / leak."

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

**Answer:** `Task.WhenAny` completes when the first task finishes — it does not cancel or dispose the slower tasks. Losing carrier HTTP calls continue until completion, holding connections, thread-pool slots, and memory under sustained load.

- After `WhenAny`, cancel remaining work with a linked `CancellationTokenSource` passed into each quote call, then `cts.Cancel()` once the winner is chosen.
- If APIs are not cancelable, track in-flight calls and abandon results safely — but still close/dispose `HttpResponseMessage` and respect `IHttpClientFactory` lifetimes.
- Replace blocking `.Result` with `await Task.WhenAny(...)` in an async API so the request thread is not blocked during the race.
- Log slow-loser latency separately — persistent tail latency after "winner found" signals missing cancellation.

**Production takeaway:** `WhenAny` is a coordination primitive, not a resource cleanup primitive — production races must explicitly stop losers. See **Program.cs** Section 9 — first completed task wins; others keep running unless canceled.

---

#### Q7. (D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?

**Answer:** Launching four thousand simultaneous `Task.Run` validations queues thousands of work items at once, spiking thread-pool usage and likely overwhelming the database — `Task.WaitAll` also blocks the orchestrator thread until every task completes, with no backpressure. `Parallel.ForEach` helps CPU-bound validation but is the wrong default if validation is I/O-bound and still needs a concurrency cap for downstream limits.

- Ship chunked or throttled async orchestration: `await Task.WhenAll(batch.Select(o => ValidateAsync(o, ct)))` over batches of 50–200, or use `SemaphoreSlim` / `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` tuned to DB connection limits.
- Use `Task.Run` only for CPU-bound validation logic; I/O-bound checks should be truly async end-to-end (ch.04).
- Prefer `Task.WhenAll` over `Task.WaitAll` in async hosts — composable with cancellation and does not block a precious thread for the entire batch duration.
- Emit metrics: queue depth, validation latency p95, and faulted task count — unobserved faults in nightly jobs can fail silently until morning.

**Production takeaway:** Tasks make it easy to express parallelism; production requires **bounded** parallelism. Karat distinguishes "I can start 4,000 tasks" from "I should start 4,000 tasks." See **Program.cs** Sections 8 (`WhenAll`) and 10 (`SemaphoreSlim` throttle).

---
