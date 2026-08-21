# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/07. Concurrent Collections/`

---

#### Q1. (R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:

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

#### Q2. (R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:

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

#### Q3. (R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:

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

#### Q4. (R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:

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

#### Q5. (P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?

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

#### Q6. (D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:

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

#### Q7. (M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:

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
