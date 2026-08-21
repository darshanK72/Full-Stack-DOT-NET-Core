# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/07. Concurrent Collections/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

#### Q5. (P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?

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
