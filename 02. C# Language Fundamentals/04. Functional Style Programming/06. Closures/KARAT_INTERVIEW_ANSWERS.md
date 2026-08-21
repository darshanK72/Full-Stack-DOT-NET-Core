# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/06. Closures`

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

**Answer:** Each `Task.Run` lambda captures the **same** loop variable `i`, not the value at scheduling time. When the thread pool runs the tasks, the loop has usually finished and `i` is `3`, so every callback sees `3`. A fast local run can accidentally process the "right" IDs if tasks start before the loop increments — masking the bug until production load defers execution.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Capture semantics | `for` declares one shared `i` | All deferred lambdas read final loop value |
| Correctness | Wrong order IDs processed | Duplicate work, skipped orders, bad audit trail |
| Testability | Race with loop completion | Flaky passes locally, fails under thread-pool delay |

**Fix (priority order):**

1. Copy to an inner local each iteration — the pattern from **Program.cs** Section 8a:

```csharp
for (int i = 0; i < 3; i++)
{
    int orderId = i;
    Task.Run(() => orders.ProcessOrder(orderId));
}
```

2. Pass `i` as a parameter to a helper so each lambda captures a distinct parameter slot (**Section 8b** — `AddPrinter` pattern):

```csharp
for (int i = 0; i < 3; i++)
    ScheduleOne(orders, i);

static void ScheduleOne(IOrderService orders, int orderId) =>
    Task.Run(() => orders.ProcessOrder(orderId));
```

3. Prefer `foreach` only when iterating a collection — `foreach` gets a per-iteration variable on C# 5+, but **`for` still needs the copy fix** (**Section 9**).

**Production takeaway:** This is the classic closure loop bug Karat embeds in `Task.Run`, timers, and event handlers — capture is by reference to shared storage, not a snapshot. See **Program.cs** QUICK REFERENCE — `for (int i ...)` → deferred λ sees final `i`.

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

**Answer:** `tier` is a single loop variable reused across iterations. Every stored lambda closes over the same display-class field, so when filters run later they all read the final `tier` index (`3` if length is 3) — out of range or comparing against the wrong threshold. Deferred execution does not snapshot the index at `Add` time.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Capture | Shared `tier` across all lambdas | All filters behave identically at runtime |
| Correctness | Wrong tier boundaries | Pricing rules, eligibility, or alerts misfire |
| Deferred LINQ | Same rules as stored delegates (**Section 10**) | Bug survives refactoring to `IQueryable` deferred queries |

**Fix (priority order):**

1. Inner copy per iteration:

```csharp
for (int tier = 0; tier < thresholds.Length; tier++)
{
    int capturedTier = tier;
    filters.Add(price => price >= thresholds[capturedTier]);
}
```

2. Parameter snapshot via helper (**Section 8b**):

```csharp
for (int tier = 0; tier < thresholds.Length; tier++)
    AddTierFilter(filters, thresholds, tier);

static void AddTierFilter(List<Func<decimal, bool>> sink, decimal[] thresholds, int tier) =>
    sink.Add(price => price >= thresholds[tier]);
```

3. If tiers are known at compile time or small, consider building predicates eagerly without deferred capture — evaluate threshold into a local `decimal floor = thresholds[tier]` inside the copy block when the array slot is the real shared concern.

**Production takeaway:** Factory methods that return lambdas are closure-heavy — always ask "which variable slot does each delegate share?" before shipping deferred filter lists.

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

**Answer:** The event handler is a long-lived delegate on the singleton/static `MarketDataFeed`. The lambda captures `this` (implicitly, to call `UpdateChart`) and `_quoteBuffer`, so the GC root chain is: feed → multicast delegate → display class → panel + buffer. `Dispose` removes the UI but never `-=` the handler, so every closed panel stays reachable (**Program.cs** Sections 6–7).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | Subscribe in ctor, no unsubscribe in `Dispose` | Panels accumulate across navigation |
| Capture | Lambda captures `this` + `_quoteBuffer` | 512 KB buffer pinned per panel instance |
| GC roots | Long-lived publisher holds delegate | Memory climb; Gen2 pressure in desktop and server UI |

**Fix (priority order):**

1. Store the handler and unsubscribe in `Dispose`:

```csharp
private readonly EventHandler<TickEventArgs> _tickHandler;

public TradeDetailPanel(MarketDataFeed feed)
{
    _feed = feed;
    _tickHandler = (_, tick) => UpdateChart(tick, _quoteBuffer);
    _feed.TickReceived += _tickHandler;
}

public void Dispose()
{
    _feed.TickReceived -= _tickHandler;
}
```

2. Break the large capture — pass only what the handler needs (tick id, small struct), not the whole scratch buffer; allocate scratch inside `UpdateChart` or a pool if needed (**Section 7** — capture small identifiers).

3. Prefer a named instance method handler when it avoids extra display-class fields: `_feed.TickReceived += OnTickReceived;`

**Production takeaway:** Capturing `this` in a long-lived event handler is a silent leak — Karat pairs events chapter unsubscribe rules with closure capture of instance state and large graphs.

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

**Answer:** The timer callback closes over the entire `HttpCallContext` (URL, body, mutable retry state) and likely `this` on the scheduler. Timers and their delegates stay reachable in `_timers` until explicitly disposed, pinning large request payloads and preventing GC. Fire-and-forget `PostAsync` inside the callback adds async correctness issues on top of the capture leak.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | Timer + list holds closure over `context` | Request bodies and headers linger for app lifetime |
| Lifetime | No `timer.Dispose()` after fire | `_timers` grows without bound on busy systems |
| Correctness | Mutating shared `context` from timer thread | Race if same context retried or logged elsewhere |
| Async | Unobserved `PostAsync` Task | Swallowed exceptions; no cancellation |

**Fix (priority order):**

1. Capture **identifiers only** — copy `contextId`, `url`, and retry count primitives into locals before creating the timer; load fresh state from a store when the timer fires:

```csharp
var callId = context.Id;
var url = context.Url;
var timer = new Timer(async _ =>
{
    await _retryService.RetryAsync(callId, url, ct);
    // dispose timer after success/final failure
}, null, delay, Timeout.InfiniteTimeSpan);
```

2. Remove completed timers from `_timers` and call `timer.Dispose()` — break the GC root (**Section 7** — release delegate when work completes).

3. Use `IHostedService` + `Channel<T>` or a proper job scheduler (Hangfire, Quartz, Azure Service Bus delayed messages) instead of ad-hoc `Timer` lists for production retry.

**Production takeaway:** Timers are long-lived closure hosts — treat them like event subscriptions: minimal capture, explicit disposal, and no unbounded collector lists.

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

**Answer:** The parallel lambda **captures** `invalidCount` and `errors` from the outer scope and mutates them from multiple threads concurrently. Closure gives every iteration the same shared fields — `++` on `invalidCount` and `List<T>.Add` are not thread-safe, producing lost updates and corrupted list internal state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Shared captured mutable state | Wrong invalid counts; `List<T>` corruption / exceptions |
| Closure semantics | One display-class field for `invalidCount` | Parallel bodies fight over same storage |
| Design | Closure used as implicit shared accumulator | Non-deterministic results under load |

**Fix (priority order):**

1. Use thread-local or concurrent accumulators — do not mutate captured locals from parallel bodies:

```csharp
var errors = new ConcurrentBag<string>();
var invalidCount = 0;

Parallel.ForEach(rows, row =>
{
    if (!row.IsValid)
    {
        Interlocked.Increment(ref invalidCount);
        errors.Add($"Row {row.LineNumber}: {row.Error}");
    }
});
```

2. Prefer `Parallel.ForEach` with **local init / local finally** to aggregate without locking hot paths:

```csharp
Parallel.ForEach(rows,
    () => (Invalid: 0, Errors: new List<string>()),
    (row, _, local) =>
    {
        if (!row.IsValid)
            return (local.Invalid + 1,
                local.Errors.Append($"Row {row.LineNumber}: {row.Error}").ToList());
        return local;
    },
    local => { /* merge local into global summary under lock or concurrent structure */ });
```

3. If order must be preserved, use `AsParallel().AsOrdered()` with immutable aggregation or sequential validation — parallelism is not free when closure sharing is involved.

**Production takeaway:** Closures over mutable outer locals are fine on a single thread (**Program.cs** Section 4) but become data races the moment the delegate runs on multiple threads — Karat stacks closure capture with `Parallel.ForEach` and async callbacks.

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

**Answer:** Each `p => p.UnitPrice >= floor` that captures `floor` becomes a compiler-generated display class instance plus a delegate allocation — roughly 50 display classes and 50 delegates **per HTTP request**, plus the `List<Func<...>>` and the outer lambda's own captures (`minPrice`, `repo`). This is correct but wasteful on a hot endpoint; the junior dev conflated "small syntax" with "zero cost."

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Allocation | 50 closures × per request | Gen0/Gen1 churn; GC pressure at scale |
| Capture | Each iteration correctly captures distinct `floor` | Correctness is fine; cost is the issue |
| Readability | Nested lambdas + `Any` over delegate list | Hard to test and profile |

**Fix (priority order):**

1. Replace 50 closures with one predicate or a numeric range check — no per-tier delegate:

```csharp
app.MapGet("/products", (decimal minPrice, ProductRepository repo) =>
{
    decimal maxFloor = minPrice + 49;
    return repo.GetAll()
        .Where(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxFloor)
        .ToList();
});
```

2. If tier logic is required, use a **local function** with explicit parameters (no hidden display class per tier) or a static method:

```csharp
bool InAnyTier(Product p, decimal minPrice, int tierCount)
{
    for (int i = 0; i < tierCount; i++)
        if (p.UnitPrice >= minPrice + i) return true;
    return false;
}
```

3. Cache immutable filter delegates at startup if thresholds are fixed — closures belong in factory/setup code, not per-request loops.

**Production takeaway:** Closures allocate heap display classes; capture control means choosing local functions, static methods, or inlined logic when lambdas would multiply allocations on hot paths. See **Program.cs** — display class promotion to heap on capture.

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

**Answer:** Option (A) is intentional shared mutable capture — correct for a single stateful rule instance but opaque to tests and callers because `hits` is hidden inside the display class. Options (B) and (C) make state explicit and are easier to unit test, serialize, and reason about in code review.

- **(A) Closure over `hits`:** Minimal code; state is shared across invocations of **one** returned delegate (**Program.cs** Sections 4–5). Poor fit when tests need isolated counters or when multiple rules must not share accidental state — each `CreateRule` call still gets its **own** display class, but the mutable `hits` is invisible on the API surface.

- **(B) Local function + explicit state object:** Return a lambda that closes over a `RuleState` instance you define in the factory — same semantics, visible type:

```csharp
public Func<int, bool> CreateRule(int threshold)
{
    var state = new RuleState();
    return value => Evaluate(value, threshold, state);

    static bool Evaluate(int value, int threshold, RuleState state)
    {
        bool pass = value >= threshold;
        if (pass) state.Hits++;
        return pass && state.Hits <= 3;
    }
}
```

  Local functions can be `static` to avoid capturing `this`; capture is deliberate and named.

- **(C) Named class:** Best when rules are long-lived, configured from DI, or need interfaces — `IRule.TryApply(int value)` with instance field `Hits`. Clearest lifetime and test seams for production services.

**When to prefer local functions over lambdas:**

- Hot paths where you want **`static` local functions** to guarantee no accidental `this` or outer local capture.
- Readability when a lambda nests multiple levels — extract to a local function with parameters instead of deepening closure chains.
- When the same factory needs both a closure (returned delegate) and helper logic that should **not** share capture — locals/functions separate "what is returned" from "how it works."

Keep lambdas for short LINQ/`Task.Run`/event one-liners; switch to local functions or small types when mutable capture, test isolation, or allocation visibility matters.

**Production takeaway:** Closures vs local functions is a **capture-control and readability** choice, not syntax sugar — Karat expects you to name what is shared, who owns it, and how long it lives. See **Program.cs** Sections 3, 5, and 6 — modified outer locals, shared capture, and heap promotion.
