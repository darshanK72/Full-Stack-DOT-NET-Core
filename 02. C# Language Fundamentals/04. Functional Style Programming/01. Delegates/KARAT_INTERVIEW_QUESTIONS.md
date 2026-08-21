# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/01. Delegates`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

---

#### Q6. (D) Your team is extending `OrderFulfillmentService` to support pluggable shipping and price adjustment. Two proposals:

- **Option A:** `ShippingRule` and `PriceAdjuster` delegates (as in this chapter's pipeline demo)
- **Option B:** `IShippingStrategy` and `IPriceAdjuster` interfaces injected via DI

When would you choose delegates vs interfaces for each hook in a production ASP.NET Core app, and what unsubscribe or lifetime rules apply if you keep multicast delegate chains in-process?

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
