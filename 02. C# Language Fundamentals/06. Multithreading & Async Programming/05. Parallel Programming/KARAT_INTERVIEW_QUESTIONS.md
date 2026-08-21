# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/05. Parallel Programming/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
