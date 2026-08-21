# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/06. Closures`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A batch job queues three background tasks to process order IDs 0, 1, and 2. In production every task logs `Processing order 3`. Review the scheduling code:

```csharp
public void ScheduleOrderProcessors(IOrderService orders)
{
    for (int i = 0; i < 3; i++)
    {
        Task.Run(() => orders.ProcessOrder(i));
    }
}
```

What is wrong, why does it pass a quick local smoke test sometimes, and how do you fix it?

---

#### Q2. (R) A price-filter service builds deferred LINQ queries inside a loop and stores them for later execution. Review this helper:

```csharp
public List<Func<decimal, bool>> BuildTierFilters(decimal[] thresholds)
{
    var filters = new List<Func<decimal, bool>>();
    for (int tier = 0; tier < thresholds.Length; tier++)
    {
        filters.Add(price => price >= thresholds[tier]);
    }
    return filters;
}

// Caller runs all filters later against the same quote:
foreach (var filter in BuildTierFilters(new[] { 10m, 50m, 100m }))
    Console.WriteLine(filter(75m));
```

Every filter uses the same threshold at runtime. Diagnose the capture bug and show two safe fixes from this chapter.

---

#### Q3. (R) After users navigate away from detail views, memory stays high. Review this WinForms-style panel:

```csharp
public sealed class TradeDetailPanel : IDisposable
{
    private readonly byte[] _quoteBuffer = new byte[512 * 1024];
    private readonly MarketDataFeed _feed;

    public TradeDetailPanel(MarketDataFeed feed)
    {
        _feed = feed;
        _feed.TickReceived += (_, tick) =>
            UpdateChart(tick, _quoteBuffer);
    }

    public void Dispose() { /* removed from parent */ }

    private void UpdateChart(Tick tick, byte[] scratch) { /* UI update */ }
}
```

What keeps `TradeDetailPanel` and the 512 KB buffer alive, and how do you refactor to break the capture?

---

#### Q4. (P) A singleton `RetryScheduler` registers one-shot timers that retry failed HTTP calls. Review the registration:

```csharp
public sealed class RetryScheduler
{
    private readonly List<Timer> _timers = new();

    public void ScheduleRetry(HttpCallContext context, TimeSpan delay)
    {
        var timer = new Timer(_ =>
        {
            context.RetryCount++;
            _httpClient.PostAsync(context.Url, context.Body);
        }, null, delay, Timeout.InfiniteTimeSpan);

        _timers.Add(timer);
    }
}
```

What memory and correctness issues come from this closure, and what production pattern replaces capturing the whole `context` graph?

---

#### Q5. (R) A team parallelizes CSV row validation with `Parallel.ForEach`. Under load, totals and error lists are wrong. Review:

```csharp
public ValidationSummary ValidateRows(IEnumerable<CsvRow> rows)
{
    int invalidCount = 0;
    var errors = new List<string>();

    Parallel.ForEach(rows, row =>
    {
        if (!row.IsValid)
        {
            invalidCount++;
            errors.Add($"Row {row.LineNumber}: {row.Error}");
        }
    });

    return new ValidationSummary(invalidCount, errors);
}
```

What closure-related defects are present, and how do you fix them without abandoning parallelism?

---

#### Q6. (M) An API endpoint filters products on every request using a closure factory. A junior dev argues "it's just a lambda — no allocation concern." Review the hot path:

```csharp
app.MapGet("/products", (decimal minPrice, ProductRepository repo) =>
{
    var filters = new List<Func<Product, bool>>();
    for (int i = 0; i < 50; i++)
    {
        decimal floor = minPrice + i;
        filters.Add(p => p.UnitPrice >= floor);
    }
    return repo.GetAll().Where(p => filters.Any(f => f(p))).ToList();
});
```

Explain what the compiler generates per request (display classes, delegate allocations) and how you would refactor for readability **and** capture control.

---

#### Q7. (D) You inherit a service that mixes lambdas and local functions for deferred work:

```csharp
public Func<int, bool> CreateRule(int threshold)
{
    int hits = 0;
    return value =>
    {
        bool pass = value >= threshold;
        if (pass) hits++;
        return pass && hits <= 3;
    };
}

// New requirement: same logic but no hidden mutable capture — test must assert
// each invocation independently without shared `hits` state leaking between rules.
```

Compare refactoring with (A) a closure over `hits`, (B) a local function with explicit state object, and (C) a small named class. When do you prefer local functions over lambdas for capture control in production code?
