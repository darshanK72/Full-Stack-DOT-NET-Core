# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/01. Delegates`

---

#### Q1. (R) A pricing microservice chains discount calculators on a returning delegate and logs the "final adjusted price." Review the pipeline:

```csharp
public delegate decimal PriceAdjuster(decimal price);

PriceAdjuster pipeline = ApplyTenPercentOff;
pipeline += ApplyLoyaltyTierDiscount;
pipeline += ApplyPromoCode;

decimal listPrice = 200.00m;
decimal finalPrice = pipeline(listPrice);
_logger.LogInformation("Final price after {Count} adjustments: {Price}",
    pipeline.GetInvocationList().Length, finalPrice);
```

`ApplyTenPercentOff` returns 180, `ApplyLoyaltyTierDiscount` returns 153, and `ApplyPromoCode` returns 137.70 — but production logs show `Final price: 137.70` while finance expects a step-by-step audit of each stage. What is wrong with this multicast design, and how would you fix it?

**Answer:** Multicast on a returning delegate runs every handler but keeps only the last handler's return value — earlier adjustments are silently discarded, so a chained `PriceAdjuster` cannot produce an audited step-by-step pipeline without a different design.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Delegate semantics | Returning multicast keeps last return only | Intermediate prices lost; finance audit trail wrong |
| Design | `+=` used for sequential price transforms | Reads as "pipeline" but CLR does not fold returns |
| Observability | `GetInvocationList().Length` implies all stages contributed to `finalPrice` | Misleading logs — count ≠ cumulative calculation |

**Fix (priority order):**

1. Replace multicast with an explicit loop or LINQ fold that threads the decimal through each adjuster and logs after each step.
2. If you only need side effects (audit logging), use a `void` multicast delegate (`Action<decimal>` per stage) separate from the single `PriceAdjuster` that computes the final value.
3. For production pricing, prefer a list of `IPriceAdjuster` or `Func<decimal, decimal>` in a collection invoked sequentially — testable and deterministic.

```csharp
decimal price = listPrice;
foreach (PriceAdjuster step in pipeline.GetInvocationList().Cast<PriceAdjuster>())
{
    price = step(price);
    _logger.LogInformation("After {Step}: {Price}", step.Method.Name, price);
}
decimal finalPrice = price;
```

**Production takeaway:** Multicast delegates are for void notification chains (audit, UI) — not accumulating return values. See **Program.cs** Sections 5 and Quick Reference — "only LAST handler's return value kept."

---

#### Q2. (R) An order service exposes an optional audit hook as a nullable delegate. After a handler throws, downstream code never runs and later calls crash:

```csharp
public sealed class OrderProcessor
{
    public OrderAuditHandler? OnOrderProcessed { get; set; }

    public void CompleteOrder(string orderId, decimal total)
    {
        _repository.Save(orderId, total);

        OnOrderProcessed.Invoke($"Completed {orderId}: {total:C}");

        _metrics.Increment("orders.completed");
    }
}
```

One audit handler throws `IOException` on a full disk; the next order raises `NullReferenceException` because `OnOrderProcessed` was set to null by a test teardown. Identify the problems and prioritize fixes.

**Answer:** The method invokes a nullable delegate without null-conditional syntax and lets a throwing audit handler abort the rest of `CompleteOrder` — stacked null-safety and exception-isolation bugs that pass happy-path tests.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `OnOrderProcessed.Invoke(...)` without `?.` | `NullReferenceException` when hook is unset |
| Correctness | Unhandled exception in audit handler | `_metrics.Increment` never runs; order saved but marked incomplete downstream |
| Design | Public setter allows `= null` from tests/other modules | Entire multicast chain wiped — same risk as public delegate fields (Q3) |
| Multicast | One failing handler stops the chain | Remaining audit targets never run |

**Fix (priority order):**

1. Use null-conditional invoke: `OnOrderProcessed?.Invoke(...)`.
2. Wrap multicast invocation in per-handler try/catch (or invoke via `GetInvocationList()` individually) so audit failures cannot break order completion.
3. Replace the public setter with `event` or controlled `+=`/`-=` API so external code cannot assign `= null`.
4. Ensure `_metrics.Increment` runs in `finally` or before audit if audit is best-effort.

```csharp
_repository.Save(orderId, total);

if (OnOrderProcessed is not null)
{
    foreach (OrderAuditHandler handler in OnOrderProcessed.GetInvocationList())
    {
        try { handler.Invoke($"Completed {orderId}: {total:C}"); }
        catch (Exception ex) { _logger.LogWarning(ex, "Audit handler failed"); }
    }
}

_metrics.Increment("orders.completed");
```

**Production takeaway:** Optional callbacks need `?.Invoke` and fault isolation — Karat stacks null delegate + exception propagation in one snippet. See **Program.cs** Section 4 — null-safe invoke.

---

#### Q3. (R) A teammate exposes notification wiring as a public delegate field "so integrators can subscribe without boilerplate." Review cross-team usage:

```csharp
public class InventorySyncService
{
    public OrderAuditHandler SyncCompleted;  // public field, not event
}

// Module A — startup wiring:
sync.SyncCompleted += msg => _audit.Log(msg);

// Module B — test reset before each case:
sync.SyncCompleted = null;

// Module C — "helpful" shortcut when no listeners yet:
if (sync.SyncCompleted == null)
    sync.SyncCompleted = DefaultNoOpHandler;

// Module D — integration test simulates a sync without the service:
sync.SyncCompleted?.Invoke("SKU-991 restocked — trigger reorder");
```

What production risks does this create compared to wrapping the multicast chain in an `event`, and what would you change?

**Answer:** A public delegate field lets any caller invoke the chain, replace it with `=`, or spoof notifications — `event` restricts outsiders to `+=`/`-=` only and keeps `Invoke` on the publisher.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | Public field exposes invocation list | Module D fakes sync completion → spurious downstream reorder jobs |
| Lifetime | `SyncCompleted = null` (Module B) | Silently drops all subscribers — Module A's audit never fires again |
| API contract | External `Invoke` allowed | Violates publisher/subscriber boundary; hard to reason about order of handlers |
| Testability | Tests mutate production wiring globally | Flaky cross-test interference |

**Fix (priority order):**

1. Change to `public event OrderAuditHandler? SyncCompleted` — external code can only subscribe/unsubscribe.
2. Add a `protected` or `private` `RaiseSyncCompleted(string message)` that performs `SyncCompleted?.Invoke(message)` inside the service.
3. For test reset, expose `ClearSyncHandlersForTesting()` internally or use fresh service instances — never public `= null` on shared singletons.
4. Document that multicast order follows registration order (**Program.cs** Section 5).

**Production takeaway:** Delegate *mechanics* belong in this chapter; `event` adds access control on top (**Program.cs** Section 12). Karat tests whether you know why "flexible public delegate" is worse than `event` in shared services.

---

#### Q4. (P) A warehouse API raises audit notifications from background worker threads while HTTP middleware subscribes and unsubscribes handlers per request. The publisher uses direct multicast invoke:

```csharp
private OrderAuditHandler? _auditChain;

public void RaiseAudit(string message)
{
    _auditChain?.Invoke(message);
}
```

Under load, handlers are occasionally skipped or you see rare `NullReferenceException` when the last subscriber unsubscribes during the raise. Explain the race on the multicast invocation list and show the thread-safe raise pattern for delegate chains.

**Answer:** Multicast delegate fields can change between the null check and `Invoke`, and `?.Invoke` still invokes a snapshot that may differ from the live field — copying the delegate reference to a local before invoking makes the raise atomic for that notification.

- **Race:** Thread A reads `_auditChain` (non-null). Thread B unsubscribes the last handler, setting `_auditChain` to null. Thread A calls `Invoke` on a delegate whose invocation list was mutated — on older paths or with torn reads, this surfaces as skipped handlers or `NullReferenceException`.
- **Pattern:** `var chain = _auditChain; chain?.Invoke(message);` — the local holds the invocation list as it existed at copy time; unsubscribes during the raise do not affect this invocation.
- For long-running handlers, iterate `chain.GetInvocationList()` and invoke each target separately with try/catch so one failing subscriber does not abort the rest.
- Prefer `event` with the same local-copy raise inside the owning class; do not expose the multicast field publicly.

```csharp
public void RaiseAudit(string message)
{
    var chain = _auditChain;
    if (chain is null) return;

    foreach (OrderAuditHandler handler in chain.GetInvocationList())
    {
        try { handler(message); }
        catch (Exception ex) { _logger.LogError(ex, "Audit handler failed"); }
    }
}
```

**Production takeaway:** Thread-safe delegate raise = copy to local, then invoke — same mechanism underlying event raises. Events chapter covers subscriber lifetime; this chapter owns multicast invocation semantics.

---

#### Q5. (P) An ASP.NET Core app registers a **Singleton** `ShippingCalculator` that takes a `Func<decimal, decimal>` built at startup from a **Scoped** `TaxRateProvider`:

```csharp
builder.Services.AddScoped<TaxRateProvider>();
builder.Services.AddSingleton<ShippingCalculator>(sp =>
{
    var taxProvider = sp.GetRequiredService<TaxRateProvider>();
    Func<decimal, decimal> applyTax = amount => amount * (1 + taxProvider.CurrentRate);
    return new ShippingCalculator(applyTax);
});

public sealed class ShippingCalculator
{
    private readonly Func<decimal, decimal> _applyTax;
    public ShippingCalculator(Func<decimal, decimal> applyTax) => _applyTax = applyTax;
    public decimal Calculate(decimal baseShipping) => _applyTax(baseShipping);
}
```

Requests intermittently use stale tax rates or throw `ObjectDisposedException`. What is wrong with this delegate wiring in DI, and how do you fix it?

**Answer:** The singleton captures a delegate that closes over a scoped `TaxRateProvider` from the root provider at startup — a captive dependency that outlives the scope and reads wrong or disposed state on later requests.

- `GetRequiredService<TaxRateProvider>()` inside the singleton factory resolves one scope's instance (or throws at validation) and embeds it in the lambda's closure for the app lifetime.
- Delegates make the capture invisible — the signature `Func<decimal, decimal>` looks stateless but holds the scoped service reference.
- **Fix options:** (1) Make `ShippingCalculator` scoped and inject `TaxRateProvider` directly. (2) Keep singleton but pass `IServiceScopeFactory` and resolve `TaxRateProvider` per `Calculate` call inside the method — not inside a cached delegate. (3) Inject `IOptionsMonitor<TaxSettings>` or a singleton rate cache updated by a background refresh — no scoped capture.
- Enable `ValidateOnBuild` and `ValidateScopes` in development to catch this at startup.

```csharp
builder.Services.AddScoped<ShippingCalculator>();
builder.Services.AddScoped<TaxRateProvider>();

public sealed class ShippingCalculator
{
    private readonly TaxRateProvider _taxProvider;
    public ShippingCalculator(TaxRateProvider taxProvider) => _taxProvider = taxProvider;
    public decimal Calculate(decimal baseShipping) =>
        baseShipping * (1 + _taxProvider.CurrentRate);
}
```

**Production takeaway:** `Func<T>`/`Action<T>` in DI hide captured lifetimes — Karat pairs delegate syntax with ASP.NET Core scope rules. Prefer injecting the service or factory interface explicitly over a pre-built closure on a singleton.

---

#### Q6. (D) Your team is extending `OrderFulfillmentService` to support pluggable shipping and price adjustment. Two proposals:

- **Option A:** `ShippingRule` and `PriceAdjuster` delegates (as in this chapter's pipeline demo)
- **Option B:** `IShippingStrategy` and `IPriceAdjuster` interfaces injected via DI

When would you choose delegates vs interfaces for each hook in a production ASP.NET Core app, and what unsubscribe or lifetime rules apply if you keep multicast delegate chains in-process?

**Answer:** Use delegates for single, swappable callbacks with optional multicast (in-process audit/logging); use interfaces for multi-operation contracts, DI registration, and test doubles in ASP.NET Core services.

- **Delegates (`ShippingRule`, `PriceAdjuster`):** Fit when you need one method slot swapped at runtime — e.g., choosing `StandardShipping` vs `ExpressShipping` via method group assignment (**Program.cs** Section 9). Good for strategy passed into a single method call, local plugin hooks, or short-lived multicast audit chains. Downside: no discoverable contract, harder to mock without wrapping in an interface, and multicast lifetime must be managed manually (`-=` on dispose).
- **Interfaces (`IShippingStrategy`, `IPriceAdjuster`):** Fit when the consumer needs a named capability registered in DI, multiple related members, or unit tests with fakes (**Program.cs** Section 8). ASP.NET Core already resolves `IPriceAdjuster` per request or as keyed services — preferred for domain services shared across controllers.
- **Lifetime rule for multicast chains:** Publisher must outlive subscribers or subscribers must `-=` in `Dispose`/`IAsyncDisposable`. Never store per-request lambdas on a singleton delegate field. For web apps, avoid in-process multicast for cross-request notification — use `IHostedService`, message bus, or `Channel<T>` instead.
- **Practical split:** `ShippingRule` as a delegate parameter to `CalculateShipping(baseRate, rule)` is fine; registering global `OrderAuditHandler` multicast on a singleton without `event` is not.

**Production takeaway:** Chapter demo delegates excel at algorithm slots and audit chains; production ASP.NET Core domain code usually registers interfaces in DI and keeps multicast delegate chains local and short-lived.

---

#### Q7. (M) A reporting job wires a covariant factory delegate and then fails when accessing derived-only data:

```csharp
public delegate ReportSummary SummaryFactory();

SummaryFactory factory = ReportBuilders.BuildDetailedReport;
ReportSummary summary = factory();

// Later — production code expects page count for PDF pagination:
int pages = ((DetailedReport)summary).PageCount;  // InvalidCastException in some builds
```

The assignment compiles and `summary.Title` works. Why does the cast fail at runtime, and what pattern safely preserves `DetailedReport` through the callback chain?

**Answer:** Covariant delegate assignment only widens the compile-time return type — `factory()` is typed as `ReportSummary`, and the compiler treats the return as the base type unless you downcast from the known runtime type or change the delegate signature.

- `BuildDetailedReport` returns `DetailedReport`, which satisfies `SummaryFactory` because return types are covariant on delegates (**Program.cs** Section 7).
- The variable `summary` is statically typed as `ReportSummary`; the actual object may still be `DetailedReport` at runtime — but if the factory is swapped for `BuildSummaryStub()` returning plain `ReportSummary`, the cast throws `InvalidCastException`.
- **Safe patterns:** (1) Declare `SummaryFactory` as returning `DetailedReport` when all consumers need derived data. (2) Use pattern matching: `if (summary is DetailedReport detailed) { ... }`. (3) Prefer `Func<DetailedReport>` or a generic `Func<TReport>` with constraint when wiring DI. (4) As in **Program.cs** demo: `DetailedReport? detailed = summary as DetailedReport;` and handle null.
- Do not assume covariant assignment preserves derived type through subsequent indirection — only the method's declared return at the call site matters for the static type of `factory()`.

```csharp
SummaryFactory factory = ReportBuilders.BuildDetailedReport;
ReportSummary summary = factory();

if (summary is DetailedReport detailed)
    _pdfPaginator.Configure(detailed.PageCount);
else
    _pdfPaginator.Configure(defaultPageCount: 1);
```

**Production takeaway:** Covariance lets you *assign* a derived-return method to a base-return delegate — it does not guarantee every future target returns the derived type; production code must match or pattern-match on the runtime type.
