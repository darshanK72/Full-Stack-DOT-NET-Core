# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/08. Advanced C# Features/05. C# 7 Features/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) Express VIP orders are routed to the standard express lane in production. Review this C# 7 switch with `when` guards (mirrors the warehouse routing demo):

```csharp
public string RouteOrder(Order order)
{
    switch (order.Priority)
    {
        case OrderPriority.Express:
            return "EXPRESS-STANDARD";
        case OrderPriority.Express when order.IsHighValue:
            return "EXPRESS-VIP";
        case OrderPriority.Critical:
            return "CRITICAL-LANE";
        case OrderPriority.Standard when order.Quantity > 100:
            return "BULK-STANDARD";
        default:
            return "STANDARD";
    }
}
```

What is wrong, why do unit tests on isolated high-value express orders pass while mixed batches fail SLA, and how do you fix it?

---

#### Q2. (R) A product lookup was optimized with `ValueTask<string>` for cache hits. Under retry logic, intermittent `InvalidOperationException` appears. Review:

```csharp
private readonly Dictionary<int, string> _cache = new();

public async ValueTask<string> GetProductNameAsync(int productId)
{
    if (_cache.TryGetValue(productId, out string? cached))
        return cached;

    string fetched = await _catalogClient.FetchNameAsync(productId);
    _cache[productId] = fetched;
    return fetched;
}

// Caller in order enrichment:
public async Task EnrichOrderAsync(Order order)
{
    var nameTask = _lookup.GetProductNameAsync(order.ProductId);

    order.ProductName = await nameTask;

    if (string.IsNullOrEmpty(order.ProductName))
        order.ProductName = await nameTask;  // retry same ValueTask
}
```

Identify stacked issues (async semantics, caching, API contract). What production pattern replaces the double await?

---

#### Q3. (R) A developer refactors inventory reservation to use C# 7 ref returns for in-place updates. The build fails; after a workaround it crashes in QA. Review:

```csharp
public ref int FindSlot(int[] counts, int index)
{
    if (index < 0 || index >= counts.Length)
        throw new ArgumentOutOfRangeException(nameof(index));

    ref int slot = ref counts[index];
    return ref slot;  // "clearer" than returning ref counts[index]
}

public async Task ReserveAsync(Order order, int[] inventory, int skuIndex)
{
    ref int slot = ref FindSlot(inventory, skuIndex);
    await Task.Delay(10);  // simulate DB commit
    slot -= order.Quantity;
}

public ref int GetOrCreateSlot(List<int> counts, int index)
{
    while (counts.Count <= index)
        counts.Add(0);

    return ref counts[index];
}
```

What compile-time and runtime problems exist, and how should reservation mutate inventory safely in async code?

---

#### Q4. (R) A CSV import pipeline uses C# 7 out variables. Finance sees rows with quantity `0` marked as successfully imported. Review:

```csharp
public bool TryImportRow(string[] columns, out Order row)
{
    row = new Order();

    if (!int.TryParse(columns[0], out var orderId))
        return false;

    row.OrderId = orderId;

    int.TryParse(columns[2], out var qty);
    row.Quantity = qty;

    if (dict.TryGetValue(columns[1], out var skuMeta))
        row.Sku = skuMeta;
    else
        row.Sku = columns[1];

    return true;
}
```

What logic bugs hide behind valid C# 7 syntax, and how do you harden parsing without reverting to pre-C# 7 style?

---

#### Q5. (P) A warehouse fulfillment microservice returns `(bool CanFulfill, string Note)` tuples from `CheckFulfillment` — the same shape as the tutorial's tuple demo. The team debates replacing tuples with a `FulfillmentResult` record before exposing the method on a public NuGet contract. When is the tuple idiomatic, and when does it break production maintainability?

---

#### Q6. (M) A batch job uses a local function with captured outer state to retry flaky lane assignments. Ops reports duplicate reservations on the same SKU after parallel batch splits. Review:

```csharp
public void ProcessBatch(IEnumerable<Order> orders, int[] inventory)
{
    int reservationFailures = 0;

    void TryReserve(Order order, int skuIndex)
    {
        ref int slot = ref inventory[skuIndex];
        if (slot < order.Quantity)
        {
            reservationFailures++;
            return;
        }
        slot -= order.Quantity;
    }

    Parallel.ForEach(orders, order =>
    {
        int idx = _skuIndexMap[order.Sku];
        TryReserve(order, idx);
    });

    _metrics.RecordFailures(reservationFailures);
}
```

Explain mechanism-level behavior: what C# 7 features interact here, and what breaks under concurrency even though local functions and ref locals compile?

---

#### Q7. (D) Two teammates implement guard clauses for order validation. Which approach do you standardize on for a shared domain library, and why?

**Option A — throw expressions (C# 7):**

```csharp
public Order Accept(Order? order) =>
    order ?? throw new ArgumentNullException(nameof(order));

public int NormalizeQty(int qty) =>
    qty > 0 ? qty : throw new ArgumentOutOfRangeException(nameof(qty));
```

**Option B — classic blocks:**

```csharp
public Order Accept(Order? order)
{
    if (order is null)
        throw new ArgumentNullException(nameof(order));
    return order;
}

public int NormalizeQty(int qty)
{
    if (qty <= 0)
        throw new ArgumentOutOfRangeException(nameof(qty), qty, "Quantity must be positive.");
}
```

Consider expression-bodied members, exception detail for operators, and refactor safety in code review.
