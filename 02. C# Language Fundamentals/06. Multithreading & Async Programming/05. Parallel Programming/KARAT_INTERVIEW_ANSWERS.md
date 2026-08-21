# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/05. Parallel Programming/`

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

**Answer:** `Environment.ProcessorCount` on a 16-core box allows 16 concurrent workers for this loop alone, which can starve Kestrel, GC, and other tenants on the same VM. The default `-1` is also often too aggressive on shared infrastructure — you need an explicit cap from configuration plus cooperative cancellation.

- Read `MaxDegreeOfParallelism` from `IOptions<PricingEngineOptions>` (e.g. 4 on a shared 16-core host) — same intent as **Program.cs** Section 5 throttling.
- Wire `CancellationToken` from `IHostApplicationLifetime.ApplicationStopping` or job timeout so deploys and scale-in cancel long batches via `OperationCanceledException` — see Section 6a.
- Leave at least one core for the web tier and system processes unless this worker runs on a dedicated node pool.
- Measure: if CPU is already saturated, raising parallelism does not help; if workers block on locks, lowering parallelism can **improve** throughput.

**Production takeaway:** Parallelism is a **resource budget**, not "use all cores" — Karat expects you to tie `ParallelOptions` to hosting context, not machine topology alone.

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

**Answer:** Ship **Option B** for a strict cumulative threshold on ordered data — it is deterministic, needs no lock, and stops exactly when the limit is crossed. Option A adds lock contention on every iteration (often erasing parallel benefit) and still does not give strict "process until limit" semantics.

- `Break()` stops **starting** iterations with index **greater than** the break index; lower-index iterations may still be running or not yet started — see **Program.cs** Section 6b (`LowestBreakIteration`, `IsCompleted` false).
- Iterations with index ≤ break index are **not** cancelled — already-started higher-index work may still complete briefly before the loop winds down.
- Parallel order of accumulation is nondeterministic unless the batch order defines business meaning — cumulative credit limits usually require serial order or partitioned serial phases.
- If the batch is huge and per-item CPU work is heavy **and** order does not matter for the limit, consider parallel partial sums then a serial merge — not locked `Break()` on every line.

**Production takeaway:** `Break()` / `Stop()` are cooperative loop control, not transactional cutoffs — for financial thresholds on ordered inventory, prefer serial early exit or map-reduce with a clear merge rule.

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

**Answer:** `rangeSize: 1` creates one partition per index — for 12 trivial iterations that means 12 delegate invocations, partition handoffs, and thread-pool scheduling rounds. On small uniform work, that overhead dominates the multiply, so parallel is slower than a serial loop (**Program.cs** Section 11 — five-item demo).

- **Mechanism:** TPL partition granularity trades load balance against scheduling cost — micro-partitions maximize stealing flexibility but explode fixed overhead per chunk.
- **12 cheap ops:** Do not parallelize — serial `for` or simple LINQ is correct.
- **Large uniform batch:** Use default partitioning or `Partitioner.Create(0, count)` without forcing `rangeSize: 1`; let the runtime pick chunk sizes dynamically (**Section 7**).
- **Uneven per-item cost:** `Partitioner.Create(list, loadBalance: true)` for dynamic chunk stealing when row work varies widely.
- **Fixed moderate chunks:** `Partitioner.Create(0, count, rangeSize: 64)` (or similar) when items are uniform and count is in the thousands.

**Production takeaway:** Custom partitioners tune **when** parallel pays off — `rangeSize: 1` on tiny cheap work is a classic "made it parallel therefore faster" mistake Karat embeds in realistic batch code.
