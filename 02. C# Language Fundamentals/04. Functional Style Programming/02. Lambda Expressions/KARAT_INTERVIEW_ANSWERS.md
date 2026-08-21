# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/02. Lambda Expressions`

---

#### Q1. (R) A pricing microservice builds per-SKU discount rules at startup and applies them later during checkout. QA reports every SKU gets the same discount as the last item in the catalog. Review this registration code:

```csharp
public sealed class DiscountRuleRegistry
{
    private readonly List<Func<decimal, decimal>> _rules = new();

    public void RegisterRules(IEnumerable<(string Sku, decimal Rate)> catalog)
    {
        foreach (var item in catalog)
        {
            _rules.Add(price => price * (1m - item.Rate));
        }
    }

    public decimal ApplyAll(decimal price) =>
        _rules.Aggregate(price, (current, rule) => rule(current));
}
```

What is wrong with the lambdas, and how do you fix it without changing the public API shape?

**Answer:** Each stored lambda captures the **same** loop variable `item` by reference, not a snapshot of each iteration's rate. When rules run later, every delegate reads `item.Rate` from the final loop value — the classic foreach closure bug previewed in **Program.cs** Section 11 and detailed in **06. Closures**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Closure | `item` captured by reference across iterations | All rules apply the last SKU's discount rate |
| Correctness | Deferred invocation of loop-created lambdas | Passes small manual tests; fails full catalog |
| Design | No per-iteration copy of `Rate` | Silent revenue/pricing bug in production |

**Fix (priority order):**

1. Copy loop values into locals before creating the lambda so each delegate closes over its own snapshot:

```csharp
foreach (var item in catalog)
{
    decimal rate = item.Rate;
    _rules.Add(price => price * (1m - rate));
}
```

2. Alternatively use a `for` loop with an indexed copy, or build rules with a factory: `_rules.Add(MakeRule(item.Rate))` where `MakeRule(decimal rate)` returns `price => price * (1m - rate)`.
3. Add a unit test that registers ≥3 distinct rates and asserts each rule returns a different multiplier.

**Production takeaway:** Karat embeds this in realistic service code — the lambda syntax looks per-item, but closure semantics share one slot until you copy. See **06. Closures** for foreach/`for` pitfalls.

---

#### Q2. (R) A teammate refactors price validation from a statement lambda to an "expression" lambda for readability. The project fails to compile. Review the change:

```csharp
PriceFilter isValidUnitPrice = price =>
{
    if (price <= 0m) return false;
    if (price > 999_999m) return false;
    return true;
};

// Refactor attempt:
PriceFilter isValidUnitPrice = price => { price > 0m && price <= 999_999m };
```

What compile errors or design mistakes appear, and when should you keep a statement (block) lambda instead of forcing an expression form?

**Answer:** The refactor uses `{ }` in what must be a single expression — that is a **statement** lambda body, not an expression lambda, and the block does not return a value. The compiler reports **CS0834** (statements not allowed in expression lambda) or **CS0161** (not all code paths return) depending on how braces are parsed. Multi-step validation with `if` chains belongs in a **statement lambda** with explicit `return`, as shown in **Program.cs** Section 4 and Section 9.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `{ price > 0m && ... }` is a block, not one expression | Build fails — CS0834 / CS0161 |
| Design | Forcing expression form for branching logic | Wrong tool; harder to read than statement body |
| Correctness | Even `price => price > 0m && price <= 999_999m` omits `<= 0` guard clarity | Logic drift vs original three-path check |

**Fix (priority order):**

1. Keep the statement lambda when you need multiple checks or locals:

```csharp
PriceFilter isValidUnitPrice = price =>
{
    if (price <= 0m) return false;
    if (price > 999_999m) return false;
    return true;
};
```

2. If truly one expression suffices, drop braces: `price => price > 0m && price <= 999_999m`.
3. For reused validation, prefer a named method or local function and assign via method group — **Program.cs** Section 7.

**Production takeaway:** Expression lambdas are for one-liners; block bodies need explicit `return` for non-void delegates. Karat tests whether you recognize CS0834/CS0161 from real refactors, not from memorizing error numbers alone.

---

#### Q3. (R) An order API caches a `PriceTransform` delegate per tenant so repeated requests skip rebuilding markup logic. Review the scoped service:

```csharp
public sealed class TenantPricingService
{
    private Func<decimal, decimal>? _cachedTransform;

    public decimal GetMarkedUpPrice(decimal basePrice, decimal tenantMarkupPercent)
    {
        _cachedTransform ??= price => price * (1m + tenantMarkupPercent);
        return _cachedTransform(basePrice);
    }
}
```

The service is registered **Scoped**, but finance reports wrong markups when tenants change rates at runtime. What closure/capture bug is embedded here, and what is the correct fix?

**Answer:** The cached lambda captures `tenantMarkupPercent` from the **first** call that populated `_cachedTransform`. Later calls with a different markup still invoke the old closure — caching the delegate freezes the captured rate, not the calculation pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Closure | `tenantMarkupPercent` captured on first `??=` | Subsequent calls use stale markup |
| Caching | Delegate cache ignores parameter changes | Wrong prices after tenant config updates |
| Correctness | Looks like a performance win | Silent financial discrepancy |

**Fix (priority order):**

1. Do not cache a lambda that closes over a per-call parameter — compute inline or cache keyed by rate:

```csharp
public decimal GetMarkedUpPrice(decimal basePrice, decimal tenantMarkupPercent) =>
    basePrice * (1m + tenantMarkupPercent);
```

2. If caching is required, key by `(tenantId, tenantMarkupPercent)` or store the rate in a field updated from configuration, and rebuild the delegate when the rate changes.
3. Use `static` lambda only when no outer state is needed: `static price => price * 1.08m` for a fixed global tax — see **Program.cs** Section 9 (`static lambda` cannot capture locals).

**Production takeaway:** Closures capture **variables**, not parameter **values at each call** once the delegate is created. Caching + capture is a common Karat stack: scoped lifetime does not fix stale captured locals.

---

#### Q4. (P) An EF Core repository exposes two overloads for filtering products. In production, one path translates to SQL; the other loads the entire table into memory. Review:

```csharp
public async Task<List<Product>> GetExpensiveAsync(
    decimal minPrice,
    CancellationToken ct)
{
    Expression<Func<Product, bool>> predicate =
        p => p.UnitPrice >= minPrice;

    return await _db.Products
        .Where(predicate)
        .ToListAsync(ct);
}

public async Task<List<Product>> GetExpensiveInMemoryAsync(
    decimal minPrice,
    CancellationToken ct)
{
    Func<Product, bool> predicate =
        p => p.UnitPrice >= minPrice;

    return await _db.Products
        .Where(predicate)
        .ToListAsync(ct);
}
```

Explain why `Expression<Func<T, bool>>` vs `Func<T, bool>` matters for EF Core, and what breaks if you standardize on `Func` everywhere "because lambdas look the same."

**Answer:** EF Core's `IQueryable.Where` accepts `Expression<Func<T, bool>>` so the provider can **inspect the lambda tree** and translate `p.UnitPrice >= minPrice` to SQL. `Func<Product, bool>` is a compiled delegate — `Where` on `IQueryable` cannot translate it and falls back to **client evaluation** (often materializing the whole table first), which destroys performance and can pull logic out of the database incorrectly.

- Use `Expression<Func<T, bool>>` (or inline lambda) for **IQueryable** / EF filters, includes, projections that must run on the server.
- Use `Func<T, bool>` for **in-memory** `IEnumerable` / LINQ-to-Objects after materialization (`AsEnumerable()`, lists, arrays) — matches **Program.cs** `PricePipeline.FilterPrices` eager loops.
- API design: expose expression-based filters in repositories; compile to `Func` only after `.AsEnumerable()` when necessary.
- `minPrice` is captured as a constant in the expression tree parameter — EF parameterizes it correctly when the expression is built per call.

**Production takeaway:** Same `=>` syntax, different delegate type — Karat tests whether you know **Expression trees vs delegates**, not lambda syntax. Standardizing on `Func` in EF repositories is a common production foot-gun.

---

#### Q5. (R) A background price-sync job fires work with `Task.Run` and an async lambda. Failures never reach Application Insights. Review:

```csharp
public void ScheduleCatalogRefresh(IEnumerable<string> skus)
{
    foreach (var sku in skus)
    {
        Task.Run(async () =>
        {
            var price = await _gateway.FetchPriceAsync(sku);
            _cache.Set(sku, price);
        });
    }
}
```

What async/lambda issues stack here (including the classic loop capture), and how do you fix observability and correctness?

**Answer:** This code combines **unobserved async void-like fire-and-forget** (exceptions inside `Task.Run(async () => …)` are stored on the returned `Task` but never awaited), the **foreach `sku` capture bug** (every task may fetch the last SKU), and **unbounded parallel fan-out** with no throttling or cancellation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | Returned `Task` discarded | Exceptions never observed → silent sync failures |
| Closure | `sku` captured by reference in loop | Wrong SKU updated or duplicate work |
| Scalability | Unbounded `Task.Run` per item | Thread-pool stampede; gateway rate limits hit |
| Hosting | No `CancellationToken` propagation | Shutdown mid-run leaves partial cache |

**Fix (priority order):**

1. Capture the loop variable: `var currentSku = sku;` before the lambda, or use `foreach` with a local copy inside the loop body passed to a named async method.
2. Await and handle errors — e.g. `await Task.WhenAll(tasks)` with try/catch logging, or use a channel/worker with explicit exception logging to Application Insights.
3. Prefer `async Task ScheduleCatalogRefreshAsync(...)` end-to-end instead of `void` + fire-and-forget; pass `CancellationToken`.
4. Throttle concurrency (`SemaphoreSlim`, `Parallel.ForEachAsync`, or TPL Dataflow) for thousands of SKUs.

```csharp
public async Task ScheduleCatalogRefreshAsync(IEnumerable<string> skus, CancellationToken ct)
{
    var tasks = skus.Select(async sku =>
    {
        try
        {
            var price = await _gateway.FetchPriceAsync(sku, ct);
            _cache.Set(sku, price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Price sync failed for {Sku}", sku);
            throw;
        }
    });
    await Task.WhenAll(tasks);
}
```

**Production takeaway:** `async` lambdas return `Task`; discarding that task hides failures. Karat stacks async traps with closure capture — same pattern as Q1 in a hosting context.

---

#### Q6. (M) A developer chains LINQ over an in-memory price list and assumes the filter runs once at definition time. Review:

```csharp
decimal minPromoPrice = 25m;
var promoSkus = catalog
    .Where(p => p >= minPromoPrice)
    .Select(p => p * 0.90m);

Console.WriteLine($"Eligible count: {promoSkus.Count()}");

minPromoPrice = 50m;

foreach (var price in promoSkus)
{
    Console.WriteLine(price);
}
```

What does deferred execution plus closure capture imply for the printed count vs the foreach output? How would you make the pipeline deterministic for a report snapshot?

**Answer:** `Where`/`Select` on `IEnumerable` build a **deferred pipeline** — nothing runs until enumeration. `Count()` executes the filter with `minPromoPrice == 25m`, so the count reflects the ≥ $25 threshold. The lambda **captures** `minPromoPrice` by reference, so the later `foreach` re-runs the pipeline with `minPromoPrice == 50m` — fewer items and different discounted values than the count implied. This mirrors **Program.cs** Section 11 capture preview plus LINQ preview (Section 11b): same lambda, updated outer variable at execution time.

- **Count vs foreach mismatch:** Count is computed at 25m; iteration uses 50m — report looks inconsistent.
- **Snapshot fix:** Materialize once: `var promoSkus = catalog.Where(...).Select(...).ToList();` before mutating `minPromoPrice`.
- **Capture fix for reports:** Copy to a local before the query: `decimal threshold = minPromoPrice;` then `p => p >= threshold`.
- **Eager alternative:** Use array helpers like `PricePipeline.FilterPrices` from **Program.cs** Section 10 when you want immediate execution and no surprise re-evaluation.

**Production takeaway:** Deferred LINQ + captured locals means "definition time" and "execution time" differ — Karat tests whether you materialize when building financial snapshots.

---

#### Q7. (D) A hot-path checkout endpoint transforms thousands of line items per second. The team debates three filter styles:

```csharp
// A — expression lambda inline
var result = PricePipeline.ApplyToAll(prices, p => p * 1.08m);

// B — method group
var result = PricePipeline.ApplyToAll(prices, ApplyTax);

// C — static lambda (C# 9+)
var result = PricePipeline.ApplyToAll(prices, static p => p * 1.08m);
```

When would you choose A vs B vs C for production throughput and maintainability, and what allocation/closure trade-offs should you mention in a design review?

**Answer:** All three compile to delegate calls, but closure and reuse semantics differ. For a fixed 8% tax with no captured state, **C (`static` lambda)** or **B (method group to a static method)** avoids allocating a closure object; **A** may still avoid capture here because `1.08m` is a constant in the expression, but any outer local (e.g. `taxRate` from config) forces a display class allocation per creation site.

- **Choose B (method group)** when logic is reused, unit-tested, or complex enough to name — aligns with **Program.cs** Section 7 (`RoundToNearestDollar` vs equivalent lambda).
- **Choose C (`static` lambda)** for short, call-site-specific logic that must **not** capture instance or locals — enforces no accidental capture at compile time (**Program.cs** Section 9).
- **Choose A (inline lambda)** for one-off, readable transforms at a single call when capture is intentional (e.g. `p => p * (1m + tenantRate)`) or the delegate is not stored long-term.
- **Performance note:** Creating a new delegate instance inside a tight loop on every request allocates; hoist to `static readonly` field or cache when the transform is fixed. Method groups to static methods and `static` lambdas are equivalent for "no closure" scenarios.
- **Maintainability:** Prefer named methods for tax rules that change with regulation; lambdas excel at local filters as in **Program.cs** Section 10 pipeline demos.

**Production takeaway:** Karat uses design choice, not syntax trivia — `static` lambda vs method group signals "no capture, safe to reuse"; inline lambdas that close over request state belong in scoped code, not cached singleton fields (see Q3).
