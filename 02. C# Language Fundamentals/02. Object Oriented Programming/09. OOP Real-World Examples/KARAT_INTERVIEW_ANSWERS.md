# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/09. OOP Real-World Examples`

---

#### Q1. (R) A team ports the chapter's order-fulfillment payment flow into a service class. Support sees duplicate debits and failed rollbacks after card declines. Review:

```csharp
public sealed class OrderPaymentService
{
    public string Run(BankAccount wallet, decimal total, string orderRef)
    {
        var card = new CardPaymentProcessor();
        var walletGw = new WalletPaymentProcessor();

        if (wallet.Balance < total)
            return "Insufficient funds";

        wallet.TryWithdraw(total, out _);

        string result = card.ProcessOrderPayment(total, orderRef);
        if (result.Contains("declined", StringComparison.OrdinalIgnoreCase))
        {
            wallet.Deposit(total);
            result = walletGw.ProcessOrderPayment(total, "WLT-" + orderRef);
        }

        return result;
    }
}
```

What is wrong across encapsulation, abstraction, and correctness — and how would you fix it in priority order?

**Answer:** The service debits the wallet before any gateway succeeds, then infers payment outcome from a formatted string and silently retries a second gateway — so a declined card can still leave the customer charged twice or in an inconsistent ledger state. It also hard-codes concrete processors instead of depending on the chapter's `PaymentProcessor` abstraction.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Withdraw **before** confirmed charge; fallback charges a **second** gateway after partial success | Duplicate debits, reconciliation nightmares, support tickets |
| Encapsulation | Ignores `TryWithdraw` result (`out _` discarded); balance check + withdraw not atomic with payment | Race conditions; withdraw can fail while flow continues |
| Abstraction / DIP | `new CardPaymentProcessor()` / `new WalletPaymentProcessor()` inside method | Cannot swap gateways, mock in tests, or extend without editing this class (OCP) |
| Design | Parses `"declined"` from human-readable `ProcessOrderPayment` string | Fragile coupling to message text; breaks localization or logging changes |
| Domain | No idempotency on `orderRef` | Retries double-charge the same order |

**Fix (priority order):**

1. **Stop debiting before payment succeeds** — call `PaymentProcessor.TryCharge` (or gateway API) first; only `TryWithdraw` / ledger debit after confirmed charge, inside one transactional boundary (DB transaction or saga with compensating action).
2. **Inject `PaymentProcessor` (or strategy per payment method)** — caller or factory selects one processor per order; do not sequentially hammer two gateways on one decline string match.
3. **Use structured results** — return `bool` / result type from charge APIs, not `Contains("declined")` on formatted strings.
4. **Respect `TryWithdraw` outcome** and frozen-account rules from chapter `BankAccount` — propagate `errorMessage`; never ignore `out` parameters.
5. Add **idempotency key** on `orderRef` so retries are safe.

```csharp
public sealed class OrderPaymentService
{
    private readonly PaymentProcessor _processor;

    public OrderPaymentService(PaymentProcessor processor) => _processor = processor;

    public bool Run(BankAccount wallet, decimal total, string orderRef, out string message)
    {
        if (!_processor.TryCharge(total, orderRef))
        {
            message = _processor.ProcessOrderPayment(total, orderRef);
            return false;
        }

        if (!wallet.TryWithdraw(total, out message))
        {
            // Compensating refund/charge reversal on gateway
            return false;
        }

        message = _processor.ProcessOrderPayment(total, orderRef);
        return true;
    }
}
```

**Production takeaway:** The chapter separates **encapsulated ledger rules** (`BankAccount`) from **hidden gateway logic** (`PaymentProcessor`) — Karat stacks them to see if you preserve invariants when wiring a "real" service. See **Program.cs** Sections 2–3 — `TryWithdraw` + `ProcessOrderPayment`.

---

#### Q2. (R) A logistics API quotes delivery cost from the chapter's `Vehicle` fleet. After adding `Motorcycle` to the fleet, quotes are wrong and every new vehicle type requires editing this method. Review:

```csharp
public static decimal QuoteDelivery(Vehicle vehicle, decimal distanceKm, decimal ratePerKm)
{
    if (vehicle is Car)
        return distanceKm * ratePerKm;

    if (vehicle is Truck truck)
        return distanceKm * ratePerKm * (1.0m + truck.PayloadTons * 0.05m);

    // Fallback for anything else (Motorcycle, future types)
    return distanceKm * ratePerKm * 2.0m;
}
```

What design problems do you see, and how does the chapter's polymorphism model replace this?

**Answer:** The method re-implements pricing with type tests and a punitive default multiplier, so `Motorcycle` quotes are wrong and every new `Vehicle` subtype forces another branch — exactly what polymorphic `EstimateDeliveryCostKm` on the chapter's hierarchy avoids.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Polymorphism | Ignores `Vehicle.EstimateDeliveryCostKm` override on `Truck` | Duplicated / divergent pricing logic; truck payload formula may drift from domain |
| OCP | Central `is` / `if` chain | New vehicle types require editing shared utility — merge conflicts, missed cases |
| LSP / correctness | `2.0m` fallback for unknown types | Motorcycles overcharged; silent wrong quotes in production |
| Maintainability | `Car` branch duplicates base `ratePerKm * 1.0m` | Two places to change base rate logic |

**Fix (priority order):**

1. Replace the method body with **`vehicle.EstimateDeliveryCostKm(ratePerKm) * distanceKm`** — one line using runtime dispatch.
2. Override `EstimateDeliveryCostKm` on subtypes that differ (`Truck` already does); leave `Car` / `Motorcycle` on base behavior or add precise overrides.
3. Delete the fallback multiplier — if a new type needs special pricing, add a derived class override instead of editing a god-method.
4. Accept `Vehicle` (or `IReadOnlyList<Vehicle>`) in fleet APIs so callers never downcast for pricing.

**Production takeaway:** Chapter Section 4–5 shows **virtual override + base reference** so fleet loops stay branch-free — Karat uses logistics quoting to test whether you reach for `is` checks after learning polymorphism. See **Program.cs** — `deliveryVehicle.EstimateDeliveryCostKm(ratePerKm)`.

---

#### Q3. (R) A PR consolidates payment, delivery, labels, notifications, and invoicing into one coordinator for "simplicity." Review:

```csharp
public sealed class OrderFulfillmentHub
{
    public BankAccount CustomerWallet { get; set; } = new("ACC-DEFAULT", 0m);

    public string Fulfill(string customer, string orderRef, decimal total)
    {
        CustomerWallet.TryWithdraw(total, out _);

        var card = new CardPaymentProcessor();
        card.ProcessOrderPayment(total, orderRef);

        var truck = new Truck("Tata", "LPT", 2021, 3.5m);
        decimal cost = truck.EstimateDeliveryCostKm(2.4m) * 12.5m;

        var circle = new Circle(3.5);
        string label = $"Label area={Math.PI * circle.Radius * circle.Radius:0.##}";

        var email = new EmailNotificationSender();
        email.Send(customer, $"Order {orderRef} for {total:C}");

        var invoice = new InvoiceDocument("INV-1", DateTime.UtcNow, customer, total);
        return invoice.Render() + $" | delivery={cost:C} | {label}";
    }
}
```

Identify stacked OOP and SOLID issues. What would you split, inject, or abstract first?

**Answer:** One class owns mutable shared wallet state, hard-coded collaborators, duplicated shape math, and a string-concatenated API response — violating SRP and DIP while bypassing the chapter's interface and polymorphism seams.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| SRP | Payment, delivery, labeling, notify, invoice in one method | Untestable blob; any change risks regressions everywhere |
| Encapsulation | Public `CustomerWallet` setter + default account | Any caller can swap or corrupt shared wallet; multi-tenant bleed |
| DIP / abstraction | `new CardPaymentProcessor`, `new EmailNotificationSender`, inline `Circle` math | No injection; cannot add SMS/Push or swap truck without editing hub |
| Polymorphism | Recomputes circle area instead of `circle.Area` / `Shape.Draw()` | Duplicated domain logic; breaks when label rules change |
| Correctness | Withdraw + charge ordering (same as Q1) | Financial inconsistency |
| API design | Returns opaque concatenated string | Callers cannot compose invoice PDF, audit log, or HTTP 201 body cleanly |

**Fix (priority order):**

1. **Extract orchestrator** that accepts dependencies — `PaymentProcessor`, `Vehicle` (or fleet service), `IReadOnlyList<Shape>`, `IEnumerable<INotificationSender>`, `Document` factory — constructor injection.
2. **Remove mutable shared `BankAccount` property** — pass per-order wallet/account id into `Fulfill`; load scoped instance per request.
3. **Use chapter contracts** — `NotifyCustomer(senders, …)` pattern from **Program.cs**; `RenderLabels` / `SumAreas` for shapes; `invoice.Render()` as sole document output, map to DTO separately.
4. Split **domain services** — `OrderPaymentService`, `DeliveryQuoteService`, `NotificationService` — orchestrator coordinates; each unit-tested.
5. Return a **structured result** (payment status, delivery cost, notification receipts, invoice text) — not one mega-string.

**Production takeaway:** The chapter's `Main` intentionally orchestrates for learning — production code inverts that into injected abstractions. Karat capstone tests whether you recognize demo-style composition vs shippable boundaries.

---

#### Q4. (D) Product wants **push notifications** and a shared **retry-with-backoff** helper for all channels. Two proposals land in code review:

**Option A — extend abstract base:**

```csharp
public abstract class NotificationSenderBase
{
    protected void Retry(Action sendAttempt) { /* shared retry */ }
    public abstract string Send(string recipient, string message);
}

public class PushNotificationSender : NotificationSenderBase { /* ... */ }
```

**Option B — keep chapter interface + optional helper:**

```csharp
public interface INotificationSender
{
    string ChannelName { get; }
    string Send(string recipient, string message);
}

public static class NotificationRetry
{
    public static string SendWithRetry(INotificationSender sender, string recipient, string message) { /* ... */ }
}
```

Email and SMS already implement `INotificationSender` with no common base. Which direction fits this chapter's fulfillment model, and when would you combine both?

**Answer:** Prefer **Option B** — keep `INotificationSender` and add `PushNotificationSender : INotificationSender`, with retry as a cross-cutting helper or decorator — because email and SMS are unrelated types united only by a contract, matching Section 6. Introduce an abstract base only when several channels share substantial state or template steps, not for one shared utility method.

- **Why not Option A alone:** Forcing `EmailNotificationSender` and `SmsNotificationSender` onto a new base class reshapes existing types, introduces fragile inheritance where a interface sufficed, and violates **ISP** if the base accumulates channel-specific hooks (push tokens, SMS truncation).
- **Option B alignment:** Chapter `NotifyCustomer` already loops `INotificationSender[]` — push slots in without changing orchestration; retry wraps any sender.
- **When to combine both:** If push and SMS later share **significant** infrastructure (shared rate limiter state, correlation id field, template rendering), extract a small `NotificationSenderBase` **in addition to** the interface for those two — or use a **decorator** `RetryingNotificationSender : INotificationSender` that wraps any implementer.
- **Events vs direct Send:** Audit/logging can stay on `OrderFulfillmentCoordinator.OrderCompleted` (Section 9 preview) — do not push audit into the notification hierarchy.
- **Testing:** Interface + decorator/helper lets you mock `INotificationSender` and assert retry policy independently.

**Production takeaway:** Chapter rule — **interface when unrelated types share a capability; abstract class when subtypes share fields + template logic** (`Document` vs `INotificationSender`). Karat asks you to apply that rule under feature pressure, not pick inheritance by default.

---

#### Q5. (P) An ASP.NET Core team registers the chapter's fulfillment types in `Program.cs` for a checkout API:

```csharp
builder.Services.AddSingleton<BankAccount>();
builder.Services.AddSingleton<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<PaymentProcessor>(sp => sp.GetRequiredService<CardPaymentProcessor>());
builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
```

Under concurrent requests, balances mix between customers and notification behavior looks "sticky." Explain what breaks at the DI lifetime layer and how you would register these abstractions for production.

**Answer:** `BankAccount` and a single `INotificationSender` registered as **singletons** share one instance for all HTTP requests, so every customer's checkout mutates the same balance and notification channel — a functional bug that only appears under concurrent load.

- **`BankAccount` singleton:** Domain objects with mutable balance must be **scoped per request** (or loaded per customer from persistence), never singleton — same rule as cart state in web apps. Opening an account belongs in a repository + scoped unit of work, not a shared DI instance.
- **`INotificationSender` singleton:** If the implementer holds per-send state, connection, or throttling counters, those leak across users. Prefer **transient** senders or **stateless singleton** that only wraps an `HttpClient` from `IHttpClientFactory`.
- **`OrderFulfillmentCoordinator` singleton:** Acceptable only if it is **stateless** and raises events without storing subscriber lists incorrectly — but event handlers that capture scoped services from singleton are a captive dependency smell; usually register coordinator **scoped**.
- **`PaymentProcessor` transient mapping:** Fine for stateless gateways; register **multiple implementations** via factory or keyed services (`IPaymentProcessorFactory`) when checkout picks card vs wallet per order — not a single `PaymentProcessor` → card binding.
- **Production pattern:** Scoped `OrderFulfillmentService` orchestrator; transient/scoped processors; `IEnumerable<INotificationSender>` or separate sends via factory; **never** singleton mutable domain entities.

```csharp
builder.Services.AddScoped<OrderFulfillmentCoordinator>();
builder.Services.AddTransient<CardPaymentProcessor>();
builder.Services.AddTransient<WalletPaymentProcessor>();
builder.Services.AddTransient<INotificationSender, EmailNotificationSender>();
builder.Services.AddTransient<INotificationSender, SmsNotificationSender>();
// BankAccount: resolve from scoped service using customer id — not AddSingleton<BankAccount>()
```

**Production takeaway:** Chapter types teach OOP shape; ASP.NET DI teaches **which instance lives how long**. Karat capstone connects `BankAccount` encapsulation to **scoped vs singleton** — see foundation DI lifetime gotchas when moving console demo to API.

---

#### Q6. (R) A developer splits `BankAccount` into partial files (as in this chapter) but adds a "fast path" for internal ops. Frozen accounts still accept money in staging. Review both fragments:

```csharp
// BankAccount.Core.cs
public partial class BankAccount
{
    private decimal _balance;

    public decimal Balance => _balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }
}

// BankAccount.Ops.cs
public partial class BankAccount
{
    public bool IsActive { get; set; } = true;

    public void CreditOpsAdjustment(decimal amount)
    {
        // Skips ValidateForTransaction — ops-only
        _balance += amount;
    }

    private void ValidateForTransaction()
    {
        if (!IsActive) throw new InvalidOperationException("Account is frozen.");
    }
}
```

`TryWithdraw` still calls `ValidateForTransaction`, but `Deposit` no longer does. What failed across encapsulation and invariants, and how do you fix it?

**Answer:** Partial classes merge into one type, but splitting files does not split invariants — `Deposit` and `CreditOpsAdjustment` now mutate `_balance` without the freeze check, while `TryWithdraw` still enforces it, so callers can credit frozen accounts and `IsActive` is publicly settable, breaking encapsulation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | `IsActive` public setter | Any consumer can unfreeze/freeze accounts; bypasses `Freeze()` intent from chapter |
| Invariant | `Deposit` dropped `ValidateForTransaction()` | Frozen accounts accept deposits — staging bug matches production fraud/ops risk |
| Design | `CreditOpsAdjustment` writes `_balance` directly | Second mutation path; ops and customer deposits diverge in rules |
| partial class misuse | Team assumed file boundary = security boundary | Partial only splits compilation units, not access control |

**Fix (priority order):**

1. Restore **`ValidateForTransaction()` at the start of every public mutator** — `Deposit`, and any ops path that should respect freeze (or explicitly document and gate ops behind internal/admin API).
2. Change **`IsActive` to `{ get; private set; }`** — only `Freeze()` (and controlled `Reactivate()` if needed) mutate lifecycle.
3. Route **all balance changes** through private helpers, e.g. `ApplyCredit(decimal amount, bool bypassFreeze = false)` used only from trusted internal assembly with `InternalsVisibleTo` — not a public `CreditOpsAdjustment`.
4. Add tests: deposit/withdraw on frozen account must fail consistently across partial files.

```csharp
public void Deposit(decimal amount)
{
    ValidateForTransaction();
    if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount));
    _balance += amount;
}

public bool IsActive { get; private set; } = true;
```

**Production takeaway:** Chapter Section 2c–2d uses **partial** for team file layout — Karat checks you know both fragments share one invariant surface. See **Program.cs** — `TryDepositOnFrozenAccount` after `Freeze()`.

---

#### Q7. (D) You inherit a monolithic fulfillment codebase that mirrors this chapter's demo `Main` — one method creates every object, mutates wallet state, picks a truck by array index, renders shapes, sends notifications, and prints the invoice. The team has one sprint to improve production readiness without a full rewrite.

What refactor order would you choose (encapsulation fixes, introduce interfaces, extract services, events/DI), and what would you **defer**? Tie your answer to the chapter's types (`BankAccount`, `PaymentProcessor`, `Vehicle`, `Shape`, `INotificationSender`, `Document`, `OrderFulfillmentCoordinator`).

**Answer:** First stop financial and state corruption (wallet + payment ordering + singleton/scoped mistakes), then introduce constructor-injected abstractions for payment and notifications, then extract read-only polymorphic helpers for fleet/shapes/documents — defer full event-driven architecture and extension-method polish until core seams are testable.

**Sprint 1 priority (do now):**

1. **Encapsulation / correctness (`BankAccount`, payment flow):** Ensure all debits go through `TryWithdraw`; fix withdraw-before-charge ordering; no public wallet mutation; per-customer account resolution — highest business risk.
2. **DIP entry points (`PaymentProcessor`, `INotificationSender`):** Extract an `OrderFulfillmentService` that accepts `PaymentProcessor` + `IEnumerable<INotificationSender>` — mirrors chapter `NotifyCustomer` and `ProcessOrderPayment` without rewriting domain types.
3. **Polymorphism cleanup (`Vehicle`, `Shape`):** Replace index/`is` checks with `EstimateDeliveryCostKm` and `Shape.Area`/`Draw()` helpers already in **Program.cs** — low risk, high clarity win.
4. **Document output (`Document`):** Keep `Render()` template method; return invoice string from service, not `Console.WriteLine` in orchestrator — enables API responses.
5. **DI lifetimes (when moving to ASP.NET):** Scoped orchestrator; never singleton `BankAccount`.

**Defer (explicitly):**

- **Full event-driven redesign** (`OrderFulfillmentCoordinator` audit via events) until core flow is unit-tested — events are valuable but add indirection early.
- **New subtypes** (extra shapes, vehicle types) — OCP is already satisfied once polymorphic calls exist.
- **Extension methods** (`ToDisplayLabel`) — cosmetic; no production risk.
- **Partial class splits** — organizational only; no runtime benefit until team scale demands it.
- **Sealed/further inheritance tuning** on `Motorcycle` — design hygiene, not sprint-critical.

**Production takeaway:** Capstone chapter integrates pillars in one narrative — Karat asks for **prioritized** hardening: protect invariants first, inject swappable collaborators second, unify polymorphic dispatch third, polish decoupling (events) last. That mirrors how you would evolve the chapter demo `Main` into a shippable checkout pipeline without a big-bang rewrite.

---
