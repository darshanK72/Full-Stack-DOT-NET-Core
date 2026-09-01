# 04. Functional Style Programming — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Delegates](#01-delegates)
  - [Q1. What is Functional Programming, and how does C# support it w…](#01-delegates-q1)
  - [Q2. What are the key principles of Functional Programming (immut…](#01-delegates-q2)
  - [Q3. What is the difference between imperative and declarative pr…](#01-delegates-q3)
  - [Q4. What does it mean for functions to be first-class citizens i…](#01-delegates-q4)
  - [Q5. What is a delegate in C#? How does it differ from a method g…](#01-delegates-q5)
  - [Q6. How do you declare, instantiate, and invoke a custom delegat…](#01-delegates-q6)
  - [Q7. What is a multicast delegate? How does `+=` and `-=` work on…](#01-delegates-q7)
  - [Q8. What is the difference between single-cast and multicast del…](#01-delegates-q8)
  - [Q9. What happens when you invoke a multicast delegate and one su…](#01-delegates-q9)
  - [Q10. What is delegate covariance and contravariance in C#?](#01-delegates-q10)
  - [Q11. When would you prefer a named delegate type over `Func`/`Act…](#01-delegates-q11)
  - [Q12. What are the advantages and limitations of adopting a functi…](#01-delegates-q12)

- [02. Lambda Expressions](#02-lambda-expressions)
  - [Q1. What is a lambda expression in C#? What problem does it solv…](#02-lambda-expressions-q1)
  - [Q2. What is the difference between an expression lambda and a st…](#02-lambda-expressions-q2)
  - [Q3. When can parameter types be omitted in a lambda, and when mu…](#02-lambda-expressions-q3)
  - [Q4. What are target-typed lambdas (C# 10+)? In what contexts doe…](#02-lambda-expressions-q4)
  - [Q5. What is the natural type of a lambda — when does the compile…](#02-lambda-expressions-q5)
  - [Q6. How do lambda expressions differ from anonymous methods in s…](#02-lambda-expressions-q6)
  - [Q7. Can a lambda expression access `ref`, `out`, or `in` paramet…](#02-lambda-expressions-q7)
  - [Q8. Can a lambda be converted to an expression tree? What syntax…](#02-lambda-expressions-q8)
  - [Q9. What is the difference between a lambda that captures no loc…](#02-lambda-expressions-q9)
  - [Q10. How do async lambdas work (`async x => ...`)? What delegate …](#02-lambda-expressions-q10)
  - [Q11. What happens if you use a lambda where a `Expression<TDelega…](#02-lambda-expressions-q11)

- [03. Anonymous Methods](#03-anonymous-methods)
  - [Q1. What are anonymous methods in C#? Why were they introduced, …](#03-anonymous-methods-q1)
  - [Q2. What is the syntax for an anonymous method, and how does it …](#03-anonymous-methods-q2)
  - [Q3. Can anonymous methods omit parameter lists? When is that use…](#03-anonymous-methods-q3)
  - [Q4. What outer scope variables can anonymous methods access, and…](#03-anonymous-methods-q4)
  - [Q5. In modern C# code, when (if ever) would you still choose an …](#03-anonymous-methods-q5)

- [04. Extension Methods](#04-extension-methods)
  - [Q1. What are extension methods in C#? How do they appear to the …](#04-extension-methods-q1)
  - [Q2. What are the language rules for declaring an extension metho…](#04-extension-methods-q2)
  - [Q3. How does the compiler resolve an extension method call at co…](#04-extension-methods-q3)
  - [Q4. What is the difference in resolution order between an instan…](#04-extension-methods-q4)
  - [Q5. Can extension methods access `private` members of the extend…](#04-extension-methods-q5)
  - [Q6. What are the limitations of extension methods?](#04-extension-methods-q6)
  - [Q7. How do extension methods work on interfaces? What are design…](#04-extension-methods-q7)
  - [Q8. What happens when two namespaces define extensions with the …](#04-extension-methods-q8)
  - [Q9. Can you define generic extension methods? How does type infe…](#04-extension-methods-q9)
  - [Q10. What are anti-patterns with extension methods (god extension…](#04-extension-methods-q10)

- [05. Func, Action & Predicate](#05-func-action-predicate)
  - [Q1. What are `Func<T>`, `Func<T, TResult>`, and the general `Fun…](#05-func-action-predicate-q1)
  - [Q2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?](#05-func-action-predicate-q2)
  - [Q3. What is `Predicate<T>`, and how does it relate to `Func<T, b…](#05-func-action-predicate-q3)
  - [Q4. When should you use `Func` vs `Action` vs `Predicate` vs a c…](#05-func-action-predicate-q4)
  - [Q5. What are higher-order functions? Give C# examples using `Fun…](#05-func-action-predicate-q5)
  - [Q6. What is function composition, and how can it be achieved in …](#05-func-action-predicate-q6)
  - [Q7. How many generic parameters do `Func` and `Action` support, …](#05-func-action-predicate-q7)
  - [Q8. How are `Func` and `Action` used in LINQ method parameters (…](#05-func-action-predicate-q8)
  - [Q9. When does using `Func<T, bool>` instead of `Predicate<T>` im…](#05-func-action-predicate-q9)

- [06. Closures](#06-closures)
  - [Q1. What is a closure in C#?](#06-closures-q1)
  - [Q2. How does the compiler implement variable capture for lambdas…](#06-closures-q2)
  - [Q3. What is the difference between capturing a variable vs captu…](#06-closures-q3)
  - [Q4. What is the classic `for` loop closure bug, and how did C# 5…](#06-closures-q4)
  - [Q5. How does the same capture bug appear in `foreach`, LINQ, and…](#06-closures-q5)
  - [Q6. What problems arise when multiple closures share the same ca…](#06-closures-q6)
  - [Q7. What is a display class (compiler-generated closure type), a…](#06-closures-q7)
  - [Q8. What is a pure function? Give an example in C# and explain w…](#06-closures-q8)
  - [Q9. What is immutability, and why is it important in functional …](#06-closures-q9)
  - [Q10. How can immutability be achieved in C# (`readonly`, `record`…](#06-closures-q10)
  - [Q11. How do you avoid side effects when passing lambdas to APIs t…](#06-closures-q11)
  - [Q12. When should you copy loop values to a local inside the loop …](#06-closures-q12)
  - [Q13. How do local functions compare to lambdas regarding capture …](#06-closures-q13)
  - [Q14. **Closure captures the variable, not the value** — Loop lamb…](#06-closures-q14)
  - [Q15. **Same trap in LINQ and tasks** — Capturing loop variables i…](#06-closures-q15)
  - [Q16. **Multicast delegate short-circuit on exception** — Later su…](#06-closures-q16)
  - [Q17. **Extension method not in scope** — Missing `using` for the …](#06-closures-q17)
  - [Q18. **Instance method wins over extension** — An instance method…](#06-closures-q18)
  - [Q19. **Shared captured storage** — Multiple lambdas share one slo…](#06-closures-q19)
  - [Q20. **Target-typed lambda ambiguity** — Without a clear target t…](#06-closures-q20)
  - [Q21. **Expression tree vs delegate** — Expression-tree lambdas ca…](#06-closures-q21)
  - [Q22. **Capturing `this` implicitly** — Instance lambdas capture `…](#06-closures-q22)
  - [Q23. **Extension on null reference** — Extension methods can be c…](#06-closures-q23)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Delegates

#### Q1. What is Functional Programming, and how does C# support it without being a purely functional language? {#01-delegates-q1}

(R) A pricing microservice chains discount calculators on a returning delegate and logs the "final adjusted price." Review the pipeline:

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

#### Q2. What are the key principles of Functional Programming (immutability, pure functions, first-class functions, higher-order functions, referential transparency)? {#01-delegates-q2}

(R) An order service exposes an optional audit hook as a nullable delegate. After a handler throws, downstream code never runs and later calls crash:

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

#### Q3. What is the difference between imperative and declarative programming styles? Give a C# example of each. {#01-delegates-q3}

(R) A teammate exposes notification wiring as a public delegate field "so integrators can subscribe without boilerplate." Review cross-team usage:

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

#### Q4. What does it mean for functions to be first-class citizens in C#? {#01-delegates-q4}

(P) A warehouse API raises audit notifications from background worker threads while HTTP middleware subscribes and unsubscribes handlers per request. The publisher uses direct multicast invoke:

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

#### Q5. What is a delegate in C#? How does it differ from a method group and from an interface with a single method? {#01-delegates-q5}

(P) An ASP.NET Core app registers a **Singleton** `ShippingCalculator` that takes a `Func<decimal, decimal>` built at startup from a **Scoped** `TaxRateProvider`:

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

#### Q6. How do you declare, instantiate, and invoke a custom delegate type? {#01-delegates-q6}

(D) Your team is extending `OrderFulfillmentService` to support pluggable shipping and price adjustment. Two proposals:

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

#### Q7. What is a multicast delegate? How does `+=` and `-=` work on delegate instances? {#01-delegates-q7}

(M) A reporting job wires a covariant factory delegate and then fails when accessing derived-only data:

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

---

#### Q8. What is the difference between single-cast and multicast delegates at invocation time? {#01-delegates-q8}

_Answer not found._

---

#### Q9. What happens when you invoke a multicast delegate and one subscriber throws an exception? {#01-delegates-q9}

_Answer not found._

---

#### Q10. What is delegate covariance and contravariance in C#? {#01-delegates-q10}

_Answer not found._

---

#### Q11. When would you prefer a named delegate type over `Func`/`Action` in a public API? {#01-delegates-q11}

_Answer not found._

---

#### Q12. What are the advantages and limitations of adopting a functional style in typical enterprise C# codebases? {#01-delegates-q12}

_Answer not found._

---

### 02. Lambda Expressions

#### Q1. What is a lambda expression in C#? What problem does it solve compared to named methods? {#02-lambda-expressions-q1}

(R) A pricing microservice builds per-SKU discount rules at startup and applies them later during checkout. QA reports every SKU gets the same discount as the last item in the catalog. Review this registration code:

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

#### Q2. What is the difference between an expression lambda and a statement lambda? {#02-lambda-expressions-q2}

(R) A teammate refactors price validation from a statement lambda to an "expression" lambda for readability. The project fails to compile. Review the change:

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

#### Q3. When can parameter types be omitted in a lambda, and when must they be explicit? {#02-lambda-expressions-q3}

(R) An order API caches a `PriceTransform` delegate per tenant so repeated requests skip rebuilding markup logic. Review the scoped service:

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

#### Q4. What are target-typed lambdas (C# 10+)? In what contexts does the compiler infer the delegate type? {#02-lambda-expressions-q4}

(P) An EF Core repository exposes two overloads for filtering products. In production, one path translates to SQL; the other loads the entire table into memory. Review:

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

#### Q5. What is the natural type of a lambda — when does the compiler infer `Func`/`Action` vs require an explicit target type? {#02-lambda-expressions-q5}

(R) A background price-sync job fires work with `Task.Run` and an async lambda. Failures never reach Application Insights. Review:

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

#### Q6. How do lambda expressions differ from anonymous methods in syntax, capabilities, and compiler output? {#02-lambda-expressions-q6}

(M) A developer chains LINQ over an in-memory price list and assumes the filter runs once at definition time. Review:

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

#### Q7. Can a lambda expression access `ref`, `out`, or `in` parameters from the enclosing method? {#02-lambda-expressions-q7}

(D) A hot-path checkout endpoint transforms thousands of line items per second. The team debates three filter styles:

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

---

#### Q8. Can a lambda be converted to an expression tree? What syntax or API constraints apply? {#02-lambda-expressions-q8}

_Answer not found._

---

#### Q9. What is the difference between a lambda that captures no locals vs one that captures outer variables? {#02-lambda-expressions-q9}

_Answer not found._

---

#### Q10. How do async lambdas work (`async x => ...`)? What delegate types can they target? {#02-lambda-expressions-q10}

_Answer not found._

---

#### Q11. What happens if you use a lambda where a `Expression<TDelegate>` is expected vs where a `TDelegate` is expected? {#02-lambda-expressions-q11}

_Answer not found._

---

### 03. Anonymous Methods

#### Q1. What are anonymous methods in C#? Why were they introduced, and what largely replaced them? {#03-anonymous-methods-q1}

(R) A legacy WinForms order screen leaks memory after users open and close detail dialogs dozens of times. Review this maintenance patch that still uses anonymous methods:

```csharp
public sealed class OrderDetailDialog : Form
{
    private readonly OrderService _service;

    public OrderDetailDialog(OrderService service, int orderId)
    {
        _service = service;
        int retryCount = 0;

        _service.OrderUpdated += delegate (object sender, OrderUpdatedEventArgs e)
        {
            if (e.OrderId == orderId)
            {
                retryCount++;
                RefreshGrid(e.Order);
            }
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        // dialog removed from screen
    }
}
```

What keeps each closed dialog alive, and how do you fix it without changing the event contract?

**Answer:** The long-lived `OrderService` holds the multicast delegate chain; the anonymous method captures `this` (via `RefreshGrid`) and `orderId`, so every closed dialog remains reachable from the publisher until the handler is removed. Fix by storing the delegate instance and unsubscribing in `OnFormClosed`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` in constructor, no `-=` on close | Publisher retains every dialog instance → memory leak |
| Capture | Anonymous body calls instance method `RefreshGrid` | Implicit capture of `this` pins the entire `Form` |
| Capture | `orderId` and `retryCount` captured in display class | Extra heap state per dialog; harmless alone but part of leak graph |
| Legacy pattern | WinForms-style `delegate { }` event wiring | Common in pre-lambda codebases — easy to miss during UI refactors |

**Fix (priority order):**

1. Store the handler in a field so `-=` matches the same delegate instance (required for anonymous methods and lambdas alike).
2. Unsubscribe in `OnFormClosed`: `_service.OrderUpdated -= _orderUpdatedHandler;`.
3. Prefer a named instance method handler when the body is more than one line — easier to match on unsubscribe and to debug in crash dumps.
4. If migrating to lambdas, same rule applies: field + unsubscribe; syntax change alone does not fix the leak.

```csharp
private EventHandler<OrderUpdatedEventArgs>? _orderUpdatedHandler;

public OrderDetailDialog(OrderService service, int orderId)
{
    _service = service;
    _orderUpdatedHandler = delegate (object sender, OrderUpdatedEventArgs e)
    {
        if (e.OrderId == orderId)
            RefreshGrid(e.Order);
    };
    _service.OrderUpdated += _orderUpdatedHandler;
}

protected override void OnFormClosed(FormClosedEventArgs e)
{
    if (_orderUpdatedHandler != null)
        _service.OrderUpdated -= _orderUpdatedHandler;
    base.OnFormClosed(e);
}
```

**Production takeaway:** Anonymous event handlers are a top legacy leak vector — Karat tests whether you treat capture + missing `-=` as one problem, not "lambda vs delegate syntax." See **Program.cs** Section 11 — WinForms/WPF legacy hotspots and Section 7 — outer variable capture.

---

#### Q2. What is the syntax for an anonymous method, and how does it compare to lambda syntax? {#03-anonymous-methods-q2}

(R) A developer modernizes a validation pipeline by replacing anonymous methods with lambdas but leaves one factory unchanged. Review both versions — what breaks at runtime in the combined pipeline?

```csharp
public delegate bool OrderRule(Order o);

public static OrderRule BuildMaxLineItemsRule(int maxAllowedItems)
{
    return delegate (Order o)
    {
        return o.LineItemCount <= maxAllowedItems;
    };
}

// "Modernized" sibling — same intent, different capture:
public static OrderRule BuildMaxLineItemsLambda(int maxAllowedItems)
{
    int limit = maxAllowedItems;
    return o => o.LineItemCount <= limit;
}

// Caller caches rules once at startup, then changes config at runtime:
OrderRule[] pipeline = { BuildMaxLineItemsRule(_config.MaxItems) };
// ... later, admin updates _config.MaxItems from 10 → 25 ...
// pipeline still rejects orders with 15 line items
```

Is this an anonymous-method vs lambda difference, or something shared? What is the correct fix?

**Answer:** This is not an anonymous-method vs lambda difference — both forms capture `maxAllowedItems` (or `limit`) by closure at factory invocation time. The bug is caching a delegate built from a snapshot of config while expecting live reads from `_config` later.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Rule array built once from `_config.MaxItems` at startup | Runtime config changes do not affect cached delegate |
| Misdiagnosis | Blaming anonymous methods vs lambdas | Wasted refactor; same closure semantics either way |
| Design | Long-lived delegate + mutable config without refresh | Stale validation in production after admin updates |

**Fix (priority order):**

1. Rebuild the `OrderRule[]` (or the whole pipeline) when `_config` changes — subscribe to `IOptionsMonitor` / config reload events in ASP.NET Core, or invalidate cache on save in desktop apps.
2. If the rule must always read live config, capture `_config` (or an `IOptions` accessor) instead of the int snapshot: `o => o.LineItemCount <= _config.MaxItems`.
3. When migrating anonymous → lambda, preserve capture intent line-for-line; run tests that change outer variables after delegate creation.
4. Document whether each factory returns a snapshot rule or a live-config rule — both are valid, but callers must know which.

**Production takeaway:** Anonymous methods and lambdas share identical capture semantics — migration is stylistic unless you accidentally change what gets captured. See **Program.cs** Section 8 — migration steps and Section 7 — capture preview.

---

#### Q3. Can anonymous methods omit parameter lists? When is that useful? {#03-anonymous-methods-q3}

(R) A code review flags this void delegate wiring in a long-lived `StringBuilder` audit helper. Identify compile-time and lifetime issues:

```csharp
public delegate void OrderNotifier(string message);

public OrderNotifier BuildAuditLogger(StringBuilder auditLog)
{
    OrderNotifier logFailure = delegate (string message)
    {
        auditLog.AppendLine(message);
        return message.Length > 0;  // highlight failed orders in red downstream
    };

    return logFailure;
}

// Consumer:
var notifier = BuildAuditLogger(sharedAuditLog);
notifier("REJECT order 1002");
// sharedAuditLog passed to background export task that outlives the factory call
```

What is wrong, and what happens to `auditLog` after `BuildAuditLogger` returns?

**Answer:** The void `OrderNotifier` body cannot return a value — that is a compile error (CS0126). If the erroneous return were removed, the anonymous method would still capture `auditLog` on the heap inside a compiler-generated display class, so the returned delegate remains valid after `BuildAuditLogger` returns and mutates the same `StringBuilder` instance the caller passed in.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `return message.Length > 0` in void delegate body | CS0126 — blocks build |
| Lifetime | Captured `auditLog` reference | Delegate works after factory returns — intended here, but surprises devs who expect stack locals to die |
| Concurrency | Shared `StringBuilder` + background export | `StringBuilder` is not thread-safe — parallel `notifier` calls can corrupt audit text |
| Design | Side-effect-only void delegate | Correct pattern for logging (see **Program.cs** Section 6) once return is removed |

**Fix (priority order):**

1. Remove the return statement — void anonymous methods run statements only (bare `return;` allowed for early exit).
2. Keep capture of `auditLog` if the delegate must append to the caller's buffer; document shared-mutation contract.
3. If background export runs concurrently, synchronize access or queue messages to a thread-safe channel instead of mutating one `StringBuilder`.
4. Lambda equivalent: `msg => auditLog.AppendLine(msg)` — same void semantics and same capture.

```csharp
OrderNotifier logFailure = delegate (string message)
{
    auditLog.AppendLine(message);
};
```

**Production takeaway:** Void anonymous methods are the legacy twin of `Action<T>` — the trap is mixing return values with void delegates, not the `delegate` keyword itself. See **Program.cs** Section 6 and QUICK REFERENCE — "return value in void delegate body → CS0126."

---

#### Q4. What outer scope variables can anonymous methods access, and how does capture work? {#03-anonymous-methods-q4}

(P) Your team inherits a .NET Framework 4.x WinForms/WPF codebase full of `delegate { … }` event handlers, `List<T>.FindAll(delegate …)`, and `ThreadPool.QueueUserWorkItem(delegate …)`. Product wants incremental modernization — no big-bang rewrite. Describe a safe migration strategy from anonymous methods to lambdas (or local functions), including what you verify before merging each touched file.

**Answer:** Touch only files you are already changing for a feature or bugfix; replace anonymous methods with lambdas or local functions in place, run existing tests, and add targeted tests wherever capture or event subscription is involved — never mass-convert unrelated legacy code without coverage.

- **Inventory per change:** Identify delegate type (`EventHandler`, custom delegate, `Action`/`Func`), explicit vs omitted parameter lists, and captured outer variables — omitted lists on non-void delegates must become explicit lambda parameters in modern Roslyn (see **Program.cs** Section 5).
- **Mechanical rewrite rules:** `delegate (T x) { return expr; }` → `x => expr`; multi-statement bodies → `(x) => { … }`; void handlers → `(s, e) => { … }` or statement lambda; `ThreadPool`/`Task` callbacks → prefer `Task.Run` / `async` with named local functions when stack traces matter.
- **Preserve behavior:** Capture semantics are equivalent — verify rules that depend on outer locals, especially config snapshots vs live reads (Q2). Event handlers: keep field-stored handler for `-=` if the type is disposable.
- **When to prefer local functions over lambdas:** Recursive helpers, `async` bodies needing clear names in logs, or rules that deserve unit tests (`static local function` or private named method).
- **Verify before merge:** Full build, UI smoke on affected forms, memory profile on open/close dialogs with event handlers, and any golden-file or integration tests for filtered lists previously using `FindAll(delegate …)`.

**Production takeaway:** Karat rewards incremental, test-backed migration — "prefer lambdas in new code; read `delegate { }` when maintaining older projects" (**Program.cs** Section 8 and Section 11), not blanket regex replacement.

---

#### Q5. In modern C# code, when (if ever) would you still choose an anonymous method over a lambda? {#03-anonymous-methods-q5}

(M) Explain where captured locals from an anonymous method live after the enclosing method returns. A junior developer claims `int matchCount = 0` stays on the stack because it is a value type. Review this snippet from an order-filter utility:

```csharp
public static int CountMatchingOrders(List<Order> orders, OrderRule rule)
{
    int matchCount = 0;

    OrderRule countIfMatch = delegate (Order o)
    {
        bool passes = rule(o);
        if (passes)
            matchCount++;
        return passes;
    };

    foreach (Order o in orders)
        countIfMatch(o);

    return matchCount;
}
```

Where does `matchCount` actually live once `CountMatchingOrders` returns but callers still hold `countIfMatch`? Does an equivalent lambda change capture semantics?

**Answer:** When the anonymous method references `matchCount`, the compiler lifts it into a heap-allocated display class field — value type or not, captured locals are not stack-only after closure creation. An equivalent lambda mutates the same display-class field; capture semantics are identical.

- **While `CountMatchingOrders` runs:** `matchCount` may live on the stack frame, but the act of capturing copies it into a display class instance referenced by the delegate.
- **After return if delegate survives:** The display class (holding `matchCount`, and here also `rule`) lives on the heap until the delegate is unreachable — not on the stack.
- **Mutation:** `matchCount++` inside the anonymous method mutates the captured field, which is why the count updates correctly across invocations within the method — same pattern as **Program.cs** Section 7.
- **Lambda equivalent:** `o => { … matchCount++; … }` generates the same display class pattern; no behavioral difference.
- **Contrast with non-captured locals:** A local never referenced from the anonymous body truly stays stack-only and dies with the frame.

**Production takeaway:** "Value types live on the stack" is wrong for closures — Karat uses this to test closure mechanics before the dedicated Closures chapter. See **06. Closures** for loop-variable pitfalls; see **Program.cs** Section 7 — "compiler generates a display class."

---

### 04. Extension Methods

#### Q1. What are extension methods in C#? How do they appear to the caller vs how they are implemented? {#04-extension-methods-q1}

(R) A teammate nests extension helpers inside an existing service class to "keep related code together." Review this addition:

```csharp
public sealed class OrderPricingService
{
    public decimal CalculateTotal(IEnumerable<OrderLine> lines) =>
        lines.Sum(l => l.LineTotal);

    public static class LineExtensions
    {
        public string ToReceiptLine(this OrderLine line) =>
            $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
    }
}

// Caller in another file (using OrderServices;):
var text = line.ToReceiptLine(); // CS1061 — 'OrderLine' does not contain a definition for 'ToReceiptLine'
```

What compile-time rules block this pattern, and how should the extension be relocated?

**Answer:** Extension methods must live in a **non-nested** static class at namespace scope — a nested `static class` inside `OrderPricingService` cannot host extensions (CS1110 / CS1106), so the compiler never registers `ToReceiptLine` as an extension even if the nested class compiles.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile rules | Nested static class hosts extension | CS1110 — extension not in scope for instance-style calls |
| Discovery | Extension tied to service type, not `OrderLine` | Callers cannot find method via normal `using` on extension namespace |
| Design | Mixes domain service with syntactic sugar API | Violates separation — extensions belong in dedicated `*.Extensions` types |

**Fix (priority order):**

1. Move `ToReceiptLine` to a top-level `public static class OrderLineExtensions` in its own file (or at namespace root), matching **Program.cs** Section 2 and Section 5.
2. Place it in a namespace imported by callers — e.g. `Acme.Ordering.Extensions` — and add `using Acme.Ordering.Extensions;`.
3. Keep `OrderPricingService` as a normal instance/service class with no nested extension containers.

```csharp
namespace Acme.Ordering.Extensions;

public static class OrderLineExtensions
{
    public static string ToReceiptLine(this OrderLine line) =>
        $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
}
```

**Production takeaway:** Karat uses nested-class extensions to test whether you know the static-class rule set — not just `this` syntax. Relocate to a top-level static class every time.

---

#### Q2. What are the language rules for declaring an extension method (static class, `this` parameter, accessibility)? {#04-extension-methods-q2}

(R) After splitting helpers into a shared library, API controllers fail to build. Review the controller and library layout:

```csharp
// File: Acme.WebApi/Controllers/OrdersController.cs
using Acme.Domain;

public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        string label = id.ToDisplayLabel(); // CS1061
        return Ok(label);
    }
}

// File: Acme.Common/StringExtensions.cs
namespace Acme.Common.Extensions;

public static class StringExtensions
{
    public static string ToDisplayLabel(this string value) => $"[{value}]";
}
```

The domain models compile fine; only the controller breaks. What is missing, and why does `using static Acme.Common.Extensions.StringExtensions;` not fix it?

**Answer:** Extension methods are discovered by the namespace of the **static extension class**, not the extended type — the controller needs `using Acme.Common.Extensions;`. `using static` imports static members for direct calls (`ToDisplayLabel(id)`) but does **not** import extension methods for instance-style syntax.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `using Acme.Common.Extensions;` | CS1061 — method not found on `string` |
| Misconception | `using static StringExtensions` expected to enable `id.ToDisplayLabel()` | Instance-style extension syntax still fails |
| API surface | Extensions hidden from IntelliSense in WebApi layer | Team thinks library reference alone is enough |

**Fix (priority order):**

1. Add `using Acme.Common.Extensions;` to the controller (or a global `GlobalUsings.cs` in the WebApi project).
2. Alternatively call explicitly: `StringExtensions.ToDisplayLabel(id)` — no extension `using` required.
3. Do **not** rely on `using static` for extension discovery — it only lifts static members, not extension method binding.

**Production takeaway:** Shared helper libraries fail at the call site, not the definition — Karat tests namespace import rules from **Program.cs** Section 8c. Convention: `*.Extensions` namespaces + document required `using` in README or analyzer.

---

#### Q3. How does the compiler resolve an extension method call at compile time? {#04-extension-methods-q3}

(R) A null-safe helper was added for optional promo codes on checkout. Review the extension and its first production call:

```csharp
public static class StringExtensions
{
    public static string RequirePromoCode(this string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Promo code required.");
        return code.Trim().ToUpperInvariant();
    }
}

// CheckoutService:
string? promo = request.PromoCode; // may be null when omitted
string normalized = promo.RequirePromoCode(); // no compiler warning
```

The developer assumed "extension methods behave like instance methods on null." What actually happens at runtime, and how should the API be shaped for optional promo codes?

**Answer:** Unlike a true instance method, an extension **can** be invoked when the receiver is `null` — the compiler emits a static call and passes `null` as the first argument. `RequirePromoCode` then throws inside the method body (from `IsNullOrWhiteSpace`), but the developer lost nullable flow analysis because `this string` (non-nullable) does not warn on a `string?` receiver.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null semantics | Extension call allowed on null receiver | Differs from instance-method NRE at call site |
| Nullable | `this string` on nullable receiver | No CS8602/CS8604 — silent null reaches helper |
| API design | `Require*` throws on missing optional field | 500s for valid "no promo" checkout paths |

**Fix (priority order):**

1. For optional promos, use a null-tolerant extension with `this string?` — mirror **Program.cs** `IsNullOrBlank` (Section 8d).
2. Split APIs: `NormalizePromoCode(this string code)` (non-null precondition) vs `TryNormalizePromo(this string? code, out string normalized)`.
3. At the call site, guard before require: `if (!promo.IsNullOrBlank()) { … }` or pattern-match nullable promo in the service layer.
4. Enable nullable reference types project-wide so `this string` vs `this string?` documents intent.

```csharp
public static string? NormalizePromoOrNull(this string? code) =>
    string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
```

**Production takeaway:** Extensions on reference types are a common null trap — Karat checks whether you know null is passed **into** the static method, not blocked at the call site like instance dispatch.

---

#### Q4. What is the difference in resolution order between an instance method and an extension method with the same signature? {#04-extension-methods-q4}

(R) Two NuGet packages ship extensions on `string` with the same signature. After adding both, CI builds but behavior flipped in staging:

```csharp
// Package A — Acme.Text.JsonHelpers
namespace Acme.Text.JsonHelpers;
public static class StringJsonExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().Replace("\"", "'");
}

// Package B — Contoso.Security
namespace Contoso.Security;
public static class StringSecurityExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().ToLowerInvariant();
}

// Startup (both usings present):
using Acme.Text.JsonHelpers;
using Contoso.Security;

var safe = userInput.Sanitize(); // now calls Contoso's version
```

What binding rule caused the silent behavior change, and what are your options to make the call explicit and stable?

**Answer:** When multiple extension methods match, the compiler picks the **most specific** `this` type match; if still tied, **namespace/usings order** and internal tie-break rules apply — one extension wins at compile time with no runtime error. Adding a second package with the same signature can silently rebind the call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding | Duplicate extension signatures in scope | Behavior change without compile failure |
| Maintainability | `Sanitize` name collision across packages | Staging/prod diverge when usings reorder |
| Security | Wrong sanitizer (JSON vs security) | Data corruption or missed normalization |

**Fix (priority order):**

1. Call explicitly via static syntax: `StringJsonExtensions.Sanitize(userInput)` — unambiguous, refactor-safe.
2. Remove one `using` and fully-qualify the chosen extension class.
3. Rename internal extensions to domain-specific names (`SanitizeForJson`, `SanitizeForLog`) — avoid BCL-style generic names on `string`.
4. Remember: **instance methods always beat extensions** with the same signature — extensions never override existing instance API (**Program.cs** Section 8e).

**Production takeaway:** Extension conflicts do not throw — they compile and swap implementations. Prefer explicit static calls at integration boundaries (security, serialization).

---

#### Q5. Can extension methods access `private` members of the extended type? Why or why not? {#04-extension-methods-q5}

(P) A logging extension on `IEnumerable<T>` looks convenient but skews metrics under load. Review:

```csharp
public static class EnumerableDiagnosticsExtensions
{
    public static IEnumerable<T> Tap<T>(
        this IEnumerable<T> source,
        Action<T> onEach)
    {
        foreach (var item in source)
        {
            onEach(item);           // logs every element
            yield return item;
        }
    }
}

// OrderReportService:
var premiumLines = _cache.GetLines(orderId)
    .Where(l => l.UnitPrice >= 50m)
    .Tap(l => _logger.LogDebug("Premium line {Sku}", l.Sku))
    .ToList();

decimal total = premiumLines.Sum(l => l.LineTotal);
int count = premiumLines.Count(); // second pass — but source was already materialized
```

Assume `_cache.GetLines` returns a deferred `IEnumerable` backed by a live database query. A developer later removes `.ToList()` to "avoid an extra allocation." What breaks in production, and when should this extension materialize vs stay deferred?

**Answer:** `Tap` uses `yield return`, so it is **deferred** — the database query and logging run only when the pipeline is enumerated. Removing `.ToList()` while still calling `Sum` and `Count` (or any two consumers) re-executes the entire chain twice: double DB round-trips, double side effects in `Tap`, and inconsistent snapshots if data changes between enumerations.

- With `.ToList()`, enumeration happens once; `Sum`/`Count` operate on an in-memory list — correct for reporting totals.
- Without materialization, each terminal operator (`Sum`, `Count`, `foreach`) re-walks the deferred chain from `_cache.GetLines`.
- `Tap` side effects (logging) fire once per enumeration — log volume and DB load multiply with chained consumers.

**When to materialize vs defer:**

- **Materialize** (`ToList`, `ToArray`) when you need a stable snapshot, multiple passes, or bounded side effects — typical for report aggregation after filtering.
- **Stay deferred** when a single downstream consumer streams once (export pipeline, single `foreach`) and the source is cheap/idempotent.

**Production takeaway:** IEnumerable extensions compose like LINQ — deferred by default (**Program.cs** Sections 7 and 8h). Karat pairs extensions with enumeration cost, not just syntax.

---

#### Q6. What are the limitations of extension methods? {#04-extension-methods-q6}

(D) Your team debates where pricing rules belong for `OrderLine`. Option A adds extensions; Option B keeps methods on the type:

```csharp
// Option A — OrderLineExtensions.cs
public static decimal ApplyBulkDiscount(this OrderLine line, int tier) { /* 40 lines */ }
public static decimal ApplyRegionalTax(this OrderLine line, string region) { /* … */ }

// Option B — OrderLine.cs (sealed domain type)
public decimal ApplyBulkDiscount(int tier) { /* same logic */ }
```

The type is **sealed**, owned by your team, and referenced from API, tests, and a reporting job. When do extensions earn their place vs polluting discoverability, and what is your rule of thumb for third-party `HttpRequest`/`string` helpers vs domain types?

**Answer:** For **owned domain types** with core business rules (`ApplyBulkDiscount`, tax), prefer **instance methods on the type** (Option B) — discoverability, single place for behavior, and clearer unit tests. Reserve extensions for cross-cutting syntactic helpers that should not bloat the domain model, or when you **cannot** modify the type.

- **Use extensions on owned types sparingly:** formatting (`ToReceiptLine`), small adapters, or keeping `OrderLine` a pure data record while rules live in a policy service injected via DI.
- **Use extensions on BCL/third-party types:** `string`, `DateTime`, `HttpRequest`, `IEnumerable<T>` — you cannot add instance methods to sealed framework types (**Program.cs** Sections 3–4).
- **Avoid** putting 40-line pricing rules in extensions — they hide domain logic, bypass constructor/DI seams, and appear everywhere IntelliSense lists `OrderLine` methods.
- **Middle ground:** `OrderLine` stays immutable data; `IPricingPolicy` or domain service applies discounts — testable and mockable without static extension soup.

**Production takeaway:** Extensions extend surface area without extending responsibility — Karat tests judgment: `ToReceiptLine` on `OrderLine` fits; `ApplyRegionalTax` belongs on the type or a service, not a static helper class.

---

#### Q7. How do extension methods work on interfaces? What are design implications (e.g., LINQ)? {#04-extension-methods-q7}

(P) An ASP.NET Core teammate models custom middleware as extension methods on `IApplicationBuilder`, mirroring `UseRouting` / `UseAuthentication`. Review this registration block:

```csharp
public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Correlation-Id"] = id;
            await next(); // forgot to push id into HttpContext.Items / ILogger scope
        });
        return app;
    }
}

// Program.cs:
app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Logs still cannot be correlated across services. What is wrong with the middleware body, and why is the extension-method shape (`this IApplicationBuilder`) the standard pattern here even though it is "just syntactic sugar"?

**Answer:** The middleware echoes a correlation ID on the **response** but never stores it in `HttpContext.Items`, `Activity`/OpenTelemetry baggage, or an `ILogger` scope — downstream middleware, controllers, and `ILogger` output never see the ID. The extension-method shape is standard because it attaches fluent, discoverable pipeline entry points to `IApplicationBuilder` without modifying the framework type — same mechanism as `StringExtensions.ToDisplayLabel(this string)`.

- **Fix the body:** after resolving `id`, set `context.Items["CorrelationId"] = id` and wrap `await next()` in `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))` or `Activity.Current?.SetTag(...)`.
- **Order:** correlation middleware should run **early** (before auth/logging-heavy middleware) so all subsequent components share the same ID — often immediately after `UseForwardedHeaders` / before `UseAuthentication`.
- **Why extension on `IApplicationBuilder`:** reads as `app.UseCorrelationId()` in `Program.cs`; groups middleware registration API in one static class; returns `IApplicationBuilder` for chaining — identical compiler rewrite to `CorrelationIdExtensions.UseCorrelationId(app)`.

**Production takeaway:** ASP.NET `Use*` methods are extension methods — Karat connects the C# feature to production pipeline ergonomics. Fixing the sugar without fixing `HttpContext`/logging scope leaves observability broken.

---

#### Q8. What happens when two namespaces define extensions with the same name and signature for the same type? {#04-extension-methods-q8}

(M) Unit tests for a service that uses string extensions pass locally but fail in CI with `NullReferenceException`. Review the test setup:

```csharp
// Production code — Acme.Common.Extensions
public static class StringExtensions
{
    public static bool IsNullOrBlank(this string? value) =>
        string.IsNullOrWhiteSpace(value);
}

// Test project — no reference usings to Acme.Common.Extensions
public class CheckoutValidatorTests
{
    [Fact]
    public void Missing_promo_is_treated_as_blank()
    {
        string? promo = null;
        Assert.True(promo.IsNullOrBlank()); // fails in CI — CS1061 or runtime?
    }
}
```

The test project references the production assembly. Explain why extension methods are harder to mock than injected services, and what you would change if the team needs to swap validation rules per environment without `#if DEBUG` forks.

**Answer:** With the production assembly referenced but no `using Acme.Common.Extensions;`, the test fails at **compile time** with CS1061 — extension methods are not instance members of `string`. If a duplicate local extension exists in the test project, CI may bind differently. Extensions are **static dispatch** — you cannot mock `promo.IsNullOrBlank()` with Moq/NSubstitute the way you mock `ICheckoutValidator.IsBlank(promo)`.

- **Why hard to mock:** extensions compile to static calls on a fixed class; no interface, no virtual slot, no DI seam.
- **Fix immediate CI failure:** add `using Acme.Common.Extensions;` or call `StringExtensions.IsNullOrBlank(promo)` explicitly.
- **Swappable rules per environment:** extract behavior behind an interface — `IStringNormalizer` / `ICheckoutValidator` injected into the service; keep thin extensions as one-liner wrappers over injected services only at the edges (API binding), not core validation.
- **Testing extensions directly:** unit-test the static extension class with plain xUnit/NUnit tests — no mocking needed for pure functions like `IsNullOrBlank`.

**Production takeaway:** Extensions are ideal for pure, stateless helpers on types you do not own; inject interfaces when behavior must vary, be mocked, or carry policy — Karat stacks syntax discovery (Q2) with testability judgment here.

---

#### Q9. Can you define generic extension methods? How does type inference work at the call site? {#04-extension-methods-q9}

_Answer not found._

---

#### Q10. What are anti-patterns with extension methods (god extensions, violating encapsulation)? {#04-extension-methods-q10}

_Answer not found._

---

### 05. Func, Action & Predicate

#### Q1. What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family? {#05-func-action-predicate-q1}

What are `Func<T>`, `Func<T, TResult>`, and the general `Func<...>` family?

**Answer:** `Func` delegates are generic templates in `System` for functions that return a value. `Func<TResult>` takes no parameters and returns `TResult`; `Func<T, TResult>` takes one `T` and returns `TResult`; the family extends up to 16 input type parameters with the last type parameter always being the return type.

- `Func<DateTime> now = () => DateTime.UtcNow;` — zero inputs.
- `Func<string, int> len = s => s.Length;` — one input, `int` result.
- Method group assignment works when signatures align: `Func<int, int> abs = Math.Abs;`
- `Func` replaces many custom `delegate` declarations for computational callbacks.

---

#### Q2. What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`? {#05-func-action-predicate-q2}

What is `Action` vs `Action<T>` vs `Action<T1, T2, ...>`?

**Answer:** `Action` delegates represent void-returning callbacks. Plain `Action` takes no parameters; `Action<T>` takes one parameter; multi-parameter forms mirror `Func` arity up to 16 parameters, all returning void.

- `Action log = () => Console.WriteLine("done");`
- `Action<string> print = msg => Console.WriteLine(msg);`
- Use `Action` when side effects matter and no return value is needed — logging, notifications, UI updates.
- `Func<T>` with return type `void` does not exist — `Action` fills that role.

---

#### Q3. What is `Predicate<T>`, and how does it relate to `Func<T, bool>`? {#05-func-action-predicate-q3}

What is `Predicate<T>`, and how does it relate to `Func<T, bool>`?

**Answer:** `Predicate<T>` is a legacy built-in delegate that takes one argument and returns `bool`, semantically identical to `Func<T, bool>`. Modern APIs and LINQ prefer `Func<T, bool>`, but older BCL methods like `List<T>.FindAll` still accept `Predicate<T>`.

- `Predicate<int> isEven = n => n % 2 == 0;` — same shape as `Func<int, bool>`.
- Lambdas convert to either type when the target parameter expects it.
- Choose `Func<T, bool>` in new public APIs for consistency with LINQ.
- `Array.TrueForAll` and similar methods may still name `Predicate<T>` in signatures.

---

#### Q4. When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate? {#05-func-action-predicate-q4}

When should you use `Func` vs `Action` vs `Predicate` vs a custom delegate?

**Answer:** Use `Func` when the callback returns a value, `Action` for void side effects, and `Func<T, bool>` (or `Predicate<T>` for legacy BCL) for filters. Use a custom named delegate when the signature carries domain meaning important to public API consumers.

- Private helpers and LINQ chains: generic `Func`/`Action` reduce boilerplate.
- Public plugin points like `ShippingRule`: custom delegate or dedicated type documents intent.
- `Predicate<T>` only when calling APIs that require it — otherwise prefer `Func<T, bool>`.
- Identical signatures for different concepts should not share one `Func` typedef — use distinct delegate names.

---

#### Q5. What are higher-order functions? Give C# examples using `Func` and `Action`. {#05-func-action-predicate-q5}

What are higher-order functions? Give C# examples using `Func` and `Action`.

**Answer:** Higher-order functions take other functions as parameters or return functions as results. C# expresses them through delegate-typed parameters and return types.

- `ApplyToAll(decimal[] prices, Func<decimal, decimal> transform)` — passes a transform function.
- `Factory` methods returning `Func<int, int>` create configured behaviors at runtime.
- LINQ `Select` and `Where` are higher-order — they accept `Func` delegates.
- Higher-order style enables strategy injection without subclass explosion.

---

#### Q6. What is function composition, and how can it be achieved in C#? {#05-func-action-predicate-q6}

What is function composition, and how can it be achieved in C#?

**Answer:** Function composition chains functions so the output of one becomes the input of the next: `(f ∘ g)(x) = f(g(x))`. C# has no built-in compose operator, but you implement it with a small helper or nested calls.

```csharp
Func<int, int> h = x => f(g(x));
// or: Func<Func<T,R>, Func<T,T>, Func<T,R>> Compose = (f, g) => x => f(g(x));
```

- LINQ pipelines compose declaratively: `source.Select(g).Select(f)` equivalent to mapping `f(g(x))`.
- Method chaining on fluent APIs achieves similar sequencing for object transformations.
- Pure functional libraries may supply `Compose`/`Pipe` extension methods.
- Composition preserves deferred execution when built on `IEnumerable` operators.

---

#### Q7. How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`? {#05-func-action-predicate-q7}

How many generic parameters do `Func` and `Action` support, and which parameter is always the return type for `Func`?

**Answer:** Both families support up to 16 type parameters for inputs in their longest overloads. For `Func`, the last type parameter is always `TResult` — the return type; all preceding parameters are input types. `Action` overloads have no return type parameter — all type parameters are inputs.

- `Func<T1, T2, T3, T4, TResult>` — three inputs, `TResult` return.
- `Action<T1, T2, T3, T4>` — four inputs, void return.
- Arity counting includes only type parameters, not the implicit void of `Action`.
- Very long arities exist for completeness; most code uses zero to three parameters.

---

#### Q8. How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)? {#05-func-action-predicate-q8}

How are `Func` and `Action` used in LINQ method parameters (`Select`, `Where`, etc.)?

**Answer:** LINQ extension methods on `IEnumerable<T>` take `Func` delegates: `Where` accepts `Func<T, bool>`, `Select` accepts `Func<T, TResult>`, `Aggregate` accepts funcs with varying arity. The caller supplies lambdas or method groups matching those shapes.

- `orders.Where(o => o.Total > 100)` — `Func<Order, bool>` predicate.
- `orders.Select(o => o.Customer)` — `Func<Order, string>` projector.
- `IQueryable` overloads use `Expression<Func<...>>` for translation — same logical roles, different compile target.
- Deferred execution means funcs run during enumeration, not when `Where`/`Select` are called.

---

#### Q9. When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity? {#05-func-action-predicate-q9}

When does using `Func<T, bool>` instead of `Predicate<T>` improve or hurt API clarity?

**Answer:** `Func<T, bool>` improves clarity and consistency in new code because it aligns with LINQ and modern BCL additions; `Predicate<T>` hurts cross-API uniformity when mixed arbitrarily but remains required when calling legacy methods that name `Predicate<T>` explicitly.

- Public new APIs should standardize on `Func<T, bool>` unless wrapping `List.FindAll` signatures.
- Converting between them is trivial at call sites — lambdas bind to both.
- XML docs read clearer when filter parameters use the same `Func` pattern as `Enumerable.Where`.
- Keeping `Predicate<T>` in a wrapper method isolates legacy types from new code.

---

### 06. Closures

#### Q1. What is a closure in C#? {#06-closures-q1}

(R) A batch job queues three background tasks to process order IDs 0, 1, and 2. In production every task logs `Processing order 3`. Review the scheduling code:

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

#### Q2. How does the compiler implement variable capture for lambdas and anonymous methods? {#06-closures-q2}

(R) A price-filter service builds deferred LINQ queries inside a loop and stores them for later execution. Review this helper:

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

#### Q3. What is the difference between capturing a variable vs capturing a value at closure creation time? {#06-closures-q3}

(R) After users navigate away from detail views, memory stays high. Review this WinForms-style panel:

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

#### Q4. What is the classic `for` loop closure bug, and how did C# 5 change loop variable capture semantics? {#06-closures-q4}

(P) A singleton `RetryScheduler` registers one-shot timers that retry failed HTTP calls. Review the registration:

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

#### Q5. How does the same capture bug appear in `foreach`, LINQ, and `Task.Run` callbacks? {#06-closures-q5}

(R) A team parallelizes CSV row validation with `Parallel.ForEach`. Under load, totals and error lists are wrong. Review:

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

#### Q6. What problems arise when multiple closures share the same captured variable? {#06-closures-q6}

(M) An API endpoint filters products on every request using a closure factory. A junior dev argues "it's just a lambda — no allocation concern." Review the hot path:

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

#### Q7. What is a display class (compiler-generated closure type), and what performance cost does capture introduce? {#06-closures-q7}

(D) You inherit a service that mixes lambdas and local functions for deferred work:

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

---

#### Q8. What is a pure function? Give an example in C# and explain what makes it pure. {#06-closures-q8}

_Answer not found._

---

#### Q9. What is immutability, and why is it important in functional and concurrent programming? {#06-closures-q9}

_Answer not found._

---

#### Q10. How can immutability be achieved in C# (`readonly`, `record`, avoiding mutable captures)? {#06-closures-q10}

_Answer not found._

---

#### Q11. How do you avoid side effects when passing lambdas to APIs that store or invoke them later? {#06-closures-q11}

_Answer not found._

---

#### Q12. When should you copy loop values to a local inside the loop before capturing (`var copy = item`)? {#06-closures-q12}

_Answer not found._

---

#### Q13. How do local functions compare to lambdas regarding capture and allocation behavior? {#06-closures-q13}

_Answer not found._

---

#### Q14. **Closure captures the variable, not the value** — Loop lambda prints `3, 3, 3`, not `0, 1, 2`. {#06-closures-q14}

_Answer not found._

---

#### Q15. **Same trap in LINQ and tasks** — Capturing loop variables inside `.Where()` / `Task.Run` produces identical bugs. {#06-closures-q15}

_Answer not found._

---

#### Q16. **Multicast delegate short-circuit on exception** — Later subscribers may not run if an early one throws. {#06-closures-q16}

_Answer not found._

---

#### Q17. **Extension method not in scope** — Missing `using` for the static class namespace. {#06-closures-q17}

_Answer not found._

---

#### Q18. **Instance method wins over extension** — An instance method hides the extension; you cannot "override" with an extension. {#06-closures-q18}

_Answer not found._

---

#### Q19. **Shared captured storage** — Multiple lambdas share one slot for the same outer variable. {#06-closures-q19}

_Answer not found._

---

#### Q20. **Target-typed lambda ambiguity** — Without a clear target type, lambda expressions may fail to compile. {#06-closures-q20}

_Answer not found._

---

#### Q21. **Expression tree vs delegate** — Expression-tree lambdas cannot contain many C# constructs that delegate lambdas allow. {#06-closures-q21}

_Answer not found._

---

#### Q22. **Capturing `this` implicitly** — Instance lambdas capture `this`, extending object lifetime. {#06-closures-q22}

_Answer not found._

---

#### Q23. **Extension on null reference** — Extension methods can be called on null receivers; may throw inside the method. {#06-closures-q23}

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

---

#### Q6. (D) Your team is extending `OrderFulfillmentService` to support pluggable shipping and price adjustment. Two proposals:

- **Option A:** `ShippingRule` and `PriceAdjuster` delegates (as in this chapter's pipeline demo)
- **Option B:** `IShippingStrategy` and `IPriceAdjuster` interfaces injected via DI

When would you choose delegates vs interfaces for each hook in a production ASP.NET Core app, and what unsubscribe or lifetime rules apply if you keep multicast delegate chains in-process?

---

**Answer:**

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

---

### 02. Lambda Expressions

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/02. Lambda Expressions`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

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

---

### 02. Lambda Expressions

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/02. Lambda Expressions`

---

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

---

**Answer:**

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

---

**Answer:**

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

---

**Answer:**

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

---

**Answer:**

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

---

**Answer:**

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

---

**Answer:**

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

---

### 03. Anonymous Methods

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/03. Anonymous Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

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

---

### 03. Anonymous Methods

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/03. Anonymous Methods`

---

---

#### Q1. (R) A legacy WinForms order screen leaks memory after users open and close detail dialogs dozens of times. Review this maintenance patch that still uses anonymous methods:

```csharp
public sealed class OrderDetailDialog : Form
{
    private readonly OrderService _service;

    public OrderDetailDialog(OrderService service, int orderId)
    {
        _service = service;
        int retryCount = 0;

        _service.OrderUpdated += delegate (object sender, OrderUpdatedEventArgs e)
        {
            if (e.OrderId == orderId)
            {
                retryCount++;
                RefreshGrid(e.Order);
            }
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        // dialog removed from screen
    }
}
```

What keeps each closed dialog alive, and how do you fix it without changing the event contract?

---

**Answer:**

```csharp
public sealed class OrderDetailDialog : Form
{
    private readonly OrderService _service;

    public OrderDetailDialog(OrderService service, int orderId)
    {
        _service = service;
        int retryCount = 0;

        _service.OrderUpdated += delegate (object sender, OrderUpdatedEventArgs e)
        {
            if (e.OrderId == orderId)
            {
                retryCount++;
                RefreshGrid(e.Order);
            }
        };
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        // dialog removed from screen
    }
}
```

What keeps each closed dialog alive, and how do you fix it without changing the event contract?

**Answer:** The long-lived `OrderService` holds the multicast delegate chain; the anonymous method captures `this` (via `RefreshGrid`) and `orderId`, so every closed dialog remains reachable from the publisher until the handler is removed. Fix by storing the delegate instance and unsubscribing in `OnFormClosed`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` in constructor, no `-=` on close | Publisher retains every dialog instance → memory leak |
| Capture | Anonymous body calls instance method `RefreshGrid` | Implicit capture of `this` pins the entire `Form` |
| Capture | `orderId` and `retryCount` captured in display class | Extra heap state per dialog; harmless alone but part of leak graph |
| Legacy pattern | WinForms-style `delegate { }` event wiring | Common in pre-lambda codebases — easy to miss during UI refactors |

**Fix (priority order):**

1. Store the handler in a field so `-=` matches the same delegate instance (required for anonymous methods and lambdas alike).
2. Unsubscribe in `OnFormClosed`: `_service.OrderUpdated -= _orderUpdatedHandler;`.
3. Prefer a named instance method handler when the body is more than one line — easier to match on unsubscribe and to debug in crash dumps.
4. If migrating to lambdas, same rule applies: field + unsubscribe; syntax change alone does not fix the leak.

```csharp
private EventHandler<OrderUpdatedEventArgs>? _orderUpdatedHandler;

public OrderDetailDialog(OrderService service, int orderId)
{
    _service = service;
    _orderUpdatedHandler = delegate (object sender, OrderUpdatedEventArgs e)
    {
        if (e.OrderId == orderId)
            RefreshGrid(e.Order);
    };
    _service.OrderUpdated += _orderUpdatedHandler;
}

protected override void OnFormClosed(FormClosedEventArgs e)
{
    if (_orderUpdatedHandler != null)
        _service.OrderUpdated -= _orderUpdatedHandler;
    base.OnFormClosed(e);
}
```

**Production takeaway:** Anonymous event handlers are a top legacy leak vector — Karat tests whether you treat capture + missing `-=` as one problem, not "lambda vs delegate syntax." See **Program.cs** Section 11 — WinForms/WPF legacy hotspots and Section 7 — outer variable capture.

---

---

#### Q2. (R) A developer modernizes a validation pipeline by replacing anonymous methods with lambdas but leaves one factory unchanged. Review both versions — what breaks at runtime in the combined pipeline?

```csharp
public delegate bool OrderRule(Order o);

public static OrderRule BuildMaxLineItemsRule(int maxAllowedItems)
{
    return delegate (Order o)
    {
        return o.LineItemCount <= maxAllowedItems;
    };
}

// "Modernized" sibling — same intent, different capture:
public static OrderRule BuildMaxLineItemsLambda(int maxAllowedItems)
{
    int limit = maxAllowedItems;
    return o => o.LineItemCount <= limit;
}

// Caller caches rules once at startup, then changes config at runtime:
OrderRule[] pipeline = { BuildMaxLineItemsRule(_config.MaxItems) };
// ... later, admin updates _config.MaxItems from 10 → 25 ...
// pipeline still rejects orders with 15 line items
```

Is this an anonymous-method vs lambda difference, or something shared? What is the correct fix?

---

**Answer:**

```csharp
public delegate bool OrderRule(Order o);

public static OrderRule BuildMaxLineItemsRule(int maxAllowedItems)
{
    return delegate (Order o)
    {
        return o.LineItemCount <= maxAllowedItems;
    };
}

// "Modernized" sibling — same intent, different capture:
public static OrderRule BuildMaxLineItemsLambda(int maxAllowedItems)
{
    int limit = maxAllowedItems;
    return o => o.LineItemCount <= limit;
}

// Caller caches rules once at startup, then changes config at runtime:
OrderRule[] pipeline = { BuildMaxLineItemsRule(_config.MaxItems) };
// ... later, admin updates _config.MaxItems from 10 → 25 ...
// pipeline still rejects orders with 15 line items
```

Is this an anonymous-method vs lambda difference, or something shared? What is the correct fix?

**Answer:** This is not an anonymous-method vs lambda difference — both forms capture `maxAllowedItems` (or `limit`) by closure at factory invocation time. The bug is caching a delegate built from a snapshot of config while expecting live reads from `_config` later.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Rule array built once from `_config.MaxItems` at startup | Runtime config changes do not affect cached delegate |
| Misdiagnosis | Blaming anonymous methods vs lambdas | Wasted refactor; same closure semantics either way |
| Design | Long-lived delegate + mutable config without refresh | Stale validation in production after admin updates |

**Fix (priority order):**

1. Rebuild the `OrderRule[]` (or the whole pipeline) when `_config` changes — subscribe to `IOptionsMonitor` / config reload events in ASP.NET Core, or invalidate cache on save in desktop apps.
2. If the rule must always read live config, capture `_config` (or an `IOptions` accessor) instead of the int snapshot: `o => o.LineItemCount <= _config.MaxItems`.
3. When migrating anonymous → lambda, preserve capture intent line-for-line; run tests that change outer variables after delegate creation.
4. Document whether each factory returns a snapshot rule or a live-config rule — both are valid, but callers must know which.

**Production takeaway:** Anonymous methods and lambdas share identical capture semantics — migration is stylistic unless you accidentally change what gets captured. See **Program.cs** Section 8 — migration steps and Section 7 — capture preview.

---

---

#### Q3. (R) A code review flags this void delegate wiring in a long-lived `StringBuilder` audit helper. Identify compile-time and lifetime issues:

```csharp
public delegate void OrderNotifier(string message);

public OrderNotifier BuildAuditLogger(StringBuilder auditLog)
{
    OrderNotifier logFailure = delegate (string message)
    {
        auditLog.AppendLine(message);
        return message.Length > 0;  // highlight failed orders in red downstream
    };

    return logFailure;
}

// Consumer:
var notifier = BuildAuditLogger(sharedAuditLog);
notifier("REJECT order 1002");
// sharedAuditLog passed to background export task that outlives the factory call
```

What is wrong, and what happens to `auditLog` after `BuildAuditLogger` returns?

---

**Answer:**

```csharp
public delegate void OrderNotifier(string message);

public OrderNotifier BuildAuditLogger(StringBuilder auditLog)
{
    OrderNotifier logFailure = delegate (string message)
    {
        auditLog.AppendLine(message);
        return message.Length > 0;  // highlight failed orders in red downstream
    };

    return logFailure;
}

// Consumer:
var notifier = BuildAuditLogger(sharedAuditLog);
notifier("REJECT order 1002");
// sharedAuditLog passed to background export task that outlives the factory call
```

What is wrong, and what happens to `auditLog` after `BuildAuditLogger` returns?

**Answer:** The void `OrderNotifier` body cannot return a value — that is a compile error (CS0126). If the erroneous return were removed, the anonymous method would still capture `auditLog` on the heap inside a compiler-generated display class, so the returned delegate remains valid after `BuildAuditLogger` returns and mutates the same `StringBuilder` instance the caller passed in.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `return message.Length > 0` in void delegate body | CS0126 — blocks build |
| Lifetime | Captured `auditLog` reference | Delegate works after factory returns — intended here, but surprises devs who expect stack locals to die |
| Concurrency | Shared `StringBuilder` + background export | `StringBuilder` is not thread-safe — parallel `notifier` calls can corrupt audit text |
| Design | Side-effect-only void delegate | Correct pattern for logging (see **Program.cs** Section 6) once return is removed |

**Fix (priority order):**

1. Remove the return statement — void anonymous methods run statements only (bare `return;` allowed for early exit).
2. Keep capture of `auditLog` if the delegate must append to the caller's buffer; document shared-mutation contract.
3. If background export runs concurrently, synchronize access or queue messages to a thread-safe channel instead of mutating one `StringBuilder`.
4. Lambda equivalent: `msg => auditLog.AppendLine(msg)` — same void semantics and same capture.

```csharp
OrderNotifier logFailure = delegate (string message)
{
    auditLog.AppendLine(message);
};
```

**Production takeaway:** Void anonymous methods are the legacy twin of `Action<T>` — the trap is mixing return values with void delegates, not the `delegate` keyword itself. See **Program.cs** Section 6 and QUICK REFERENCE — "return value in void delegate body → CS0126."

---

---

#### Q4. (P) Your team inherits a .NET Framework 4.x WinForms/WPF codebase full of `delegate { … }` event handlers, `List<T>.FindAll(delegate …)`, and `ThreadPool.QueueUserWorkItem(delegate …)`. Product wants incremental modernization — no big-bang rewrite. Describe a safe migration strategy from anonymous methods to lambdas (or local functions), including what you verify before merging each touched file.

---

**Answer:**

**Answer:** Touch only files you are already changing for a feature or bugfix; replace anonymous methods with lambdas or local functions in place, run existing tests, and add targeted tests wherever capture or event subscription is involved — never mass-convert unrelated legacy code without coverage.

- **Inventory per change:** Identify delegate type (`EventHandler`, custom delegate, `Action`/`Func`), explicit vs omitted parameter lists, and captured outer variables — omitted lists on non-void delegates must become explicit lambda parameters in modern Roslyn (see **Program.cs** Section 5).
- **Mechanical rewrite rules:** `delegate (T x) { return expr; }` → `x => expr`; multi-statement bodies → `(x) => { … }`; void handlers → `(s, e) => { … }` or statement lambda; `ThreadPool`/`Task` callbacks → prefer `Task.Run` / `async` with named local functions when stack traces matter.
- **Preserve behavior:** Capture semantics are equivalent — verify rules that depend on outer locals, especially config snapshots vs live reads (Q2). Event handlers: keep field-stored handler for `-=` if the type is disposable.
- **When to prefer local functions over lambdas:** Recursive helpers, `async` bodies needing clear names in logs, or rules that deserve unit tests (`static local function` or private named method).
- **Verify before merge:** Full build, UI smoke on affected forms, memory profile on open/close dialogs with event handlers, and any golden-file or integration tests for filtered lists previously using `FindAll(delegate …)`.

**Production takeaway:** Karat rewards incremental, test-backed migration — "prefer lambdas in new code; read `delegate { }` when maintaining older projects" (**Program.cs** Section 8 and Section 11), not blanket regex replacement.

---

---

#### Q5. (M) Explain where captured locals from an anonymous method live after the enclosing method returns. A junior developer claims `int matchCount = 0` stays on the stack because it is a value type. Review this snippet from an order-filter utility:

```csharp
public static int CountMatchingOrders(List<Order> orders, OrderRule rule)
{
    int matchCount = 0;

    OrderRule countIfMatch = delegate (Order o)
    {
        bool passes = rule(o);
        if (passes)
            matchCount++;
        return passes;
    };

    foreach (Order o in orders)
        countIfMatch(o);

    return matchCount;
}
```

Where does `matchCount` actually live once `CountMatchingOrders` returns but callers still hold `countIfMatch`? Does an equivalent lambda change capture semantics?

---

**Answer:**

```csharp
public static int CountMatchingOrders(List<Order> orders, OrderRule rule)
{
    int matchCount = 0;

    OrderRule countIfMatch = delegate (Order o)
    {
        bool passes = rule(o);
        if (passes)
            matchCount++;
        return passes;
    };

    foreach (Order o in orders)
        countIfMatch(o);

    return matchCount;
}
```

Where does `matchCount` actually live once `CountMatchingOrders` returns but callers still hold `countIfMatch`? Does an equivalent lambda change capture semantics?

**Answer:** When the anonymous method references `matchCount`, the compiler lifts it into a heap-allocated display class field — value type or not, captured locals are not stack-only after closure creation. An equivalent lambda mutates the same display-class field; capture semantics are identical.

- **While `CountMatchingOrders` runs:** `matchCount` may live on the stack frame, but the act of capturing copies it into a display class instance referenced by the delegate.
- **After return if delegate survives:** The display class (holding `matchCount`, and here also `rule`) lives on the heap until the delegate is unreachable — not on the stack.
- **Mutation:** `matchCount++` inside the anonymous method mutates the captured field, which is why the count updates correctly across invocations within the method — same pattern as **Program.cs** Section 7.
- **Lambda equivalent:** `o => { … matchCount++; … }` generates the same display class pattern; no behavioral difference.
- **Contrast with non-captured locals:** A local never referenced from the anonymous body truly stays stack-only and dies with the frame.

**Production takeaway:** "Value types live on the stack" is wrong for closures — Karat uses this to test closure mechanics before the dedicated Closures chapter. See **06. Closures** for loop-variable pitfalls; see **Program.cs** Section 7 — "compiler generates a display class."

---

---

#### Q6. (D) A teammate argues that anonymous methods in `RunAllRules` should stay inline because "they are only five lines," but QA cannot unit-test individual rules without running the whole pipeline. The pipeline today:

```csharp
public static List<string> RunAllRules(
    List<Order> orders,
    OrderRule[] rules,
    OrderNotifier notify)
{
    foreach (Order order in orders)
        foreach (OrderRule rule in rules)
            if (!rule(order))
                notify($"Order {order.Id} failed rule {rule.Method?.Name ?? "anonymous"}");
    // ...
}

// Inline rules at call site:
rules = new OrderRule[]
{
    HasPositiveTotal,
    delegate (Order o) { return o.LineItemCount <= _config.MaxItems; },
    delegate (Order o) { return o.Total >= _config.MinCorporateTotal; }
};
```

When do you refactor anonymous (or lambda) inline rules to named methods or local functions for testability, and when is inline closure capture the right trade-off?

---

### 04. Extension Methods

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/04. Extension Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public static List<string> RunAllRules(
    List<Order> orders,
    OrderRule[] rules,
    OrderNotifier notify)
{
    foreach (Order order in orders)
        foreach (OrderRule rule in rules)
            if (!rule(order))
                notify($"Order {order.Id} failed rule {rule.Method?.Name ?? "anonymous"}");
    // ...
}

// Inline rules at call site:
rules = new OrderRule[]
{
    HasPositiveTotal,
    delegate (Order o) { return o.LineItemCount <= _config.MaxItems; },
    delegate (Order o) { return o.Total >= _config.MinCorporateTotal; }
};
```

When do you refactor anonymous (or lambda) inline rules to named methods or local functions for testability, and when is inline closure capture the right trade-off?

**Answer:** Extract to named `static` methods (or testable factory methods) when the rule encodes business policy QA must assert independently; keep inline anonymous methods or lambdas only for one-off glue, UI wiring, or rules already covered by integration tests — and accept that `rule.Method?.Name` will read `"anonymous"` for diagnostics.

- **Refactor to named methods when:** The rule is stable business logic (corporate minimum, line-item cap), needs table-driven tests with edge orders, appears in multiple pipelines, or must show up in logs/metrics by name — mirror **Program.cs** `HasPositiveTotal` + `BuildMaxLineItemsRule` split.
- **Use factory + lambda/anonymous when:** The rule is parameterized (`maxAllowedItems`) and you will test the factory once with varied inputs rather than each call site — `BuildCorporateMinimumRule` pattern.
- **Keep inline closure when:** The behavior is truly local to one screen, throwaway, or purely orchestration; cost of extraction exceeds value — but document that `RunAllRules` failure messages will say `"anonymous"` for those entries.
- **Local function middle ground:** `OrderRule MaxItemsRule() => o => o.LineItemCount <= _config.MaxItems;` inside a testable static helper gives a name in stack traces without polluting class surface — good for ASP.NET Core private validation builders.
- **Do not refactor solely for syntax:** Replacing `delegate { }` with `=>` without extraction does not improve testability — extraction to named/testable members does.

**Production takeaway:** Anonymous methods are a maintainability signal, not a performance choice — Karat tests judgment on test seams vs brevity. Prefer lambdas or named methods in new code (**Program.cs** Section 8); use anonymous syntax only to match surrounding legacy style.

---

### 04. Extension Methods

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/04. Extension Methods`

---

---

#### Q1. (R) A teammate nests extension helpers inside an existing service class to "keep related code together." Review this addition:

```csharp
public sealed class OrderPricingService
{
    public decimal CalculateTotal(IEnumerable<OrderLine> lines) =>
        lines.Sum(l => l.LineTotal);

    public static class LineExtensions
    {
        public string ToReceiptLine(this OrderLine line) =>
            $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
    }
}

// Caller in another file (using OrderServices;):
var text = line.ToReceiptLine(); // CS1061 — 'OrderLine' does not contain a definition for 'ToReceiptLine'
```

What compile-time rules block this pattern, and how should the extension be relocated?

---

**Answer:**

```csharp
public sealed class OrderPricingService
{
    public decimal CalculateTotal(IEnumerable<OrderLine> lines) =>
        lines.Sum(l => l.LineTotal);

    public static class LineExtensions
    {
        public string ToReceiptLine(this OrderLine line) =>
            $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
    }
}

// Caller in another file (using OrderServices;):
var text = line.ToReceiptLine(); // CS1061 — 'OrderLine' does not contain a definition for 'ToReceiptLine'
```

What compile-time rules block this pattern, and how should the extension be relocated?

**Answer:** Extension methods must live in a **non-nested** static class at namespace scope — a nested `static class` inside `OrderPricingService` cannot host extensions (CS1110 / CS1106), so the compiler never registers `ToReceiptLine` as an extension even if the nested class compiles.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile rules | Nested static class hosts extension | CS1110 — extension not in scope for instance-style calls |
| Discovery | Extension tied to service type, not `OrderLine` | Callers cannot find method via normal `using` on extension namespace |
| Design | Mixes domain service with syntactic sugar API | Violates separation — extensions belong in dedicated `*.Extensions` types |

**Fix (priority order):**

1. Move `ToReceiptLine` to a top-level `public static class OrderLineExtensions` in its own file (or at namespace root), matching **Program.cs** Section 2 and Section 5.
2. Place it in a namespace imported by callers — e.g. `Acme.Ordering.Extensions` — and add `using Acme.Ordering.Extensions;`.
3. Keep `OrderPricingService` as a normal instance/service class with no nested extension containers.

```csharp
namespace Acme.Ordering.Extensions;

public static class OrderLineExtensions
{
    public static string ToReceiptLine(this OrderLine line) =>
        $"{line.Sku} x{line.Quantity} = {line.LineTotal:C}";
}
```

**Production takeaway:** Karat uses nested-class extensions to test whether you know the static-class rule set — not just `this` syntax. Relocate to a top-level static class every time.

---

---

#### Q2. (R) After splitting helpers into a shared library, API controllers fail to build. Review the controller and library layout:

```csharp
// File: Acme.WebApi/Controllers/OrdersController.cs
using Acme.Domain;

public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        string label = id.ToDisplayLabel(); // CS1061
        return Ok(label);
    }
}

// File: Acme.Common/StringExtensions.cs
namespace Acme.Common.Extensions;

public static class StringExtensions
{
    public static string ToDisplayLabel(this string value) => $"[{value}]";
}
```

The domain models compile fine; only the controller breaks. What is missing, and why does `using static Acme.Common.Extensions.StringExtensions;` not fix it?

---

**Answer:**

```csharp
// File: Acme.WebApi/Controllers/OrdersController.cs
using Acme.Domain;

public class OrdersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        string label = id.ToDisplayLabel(); // CS1061
        return Ok(label);
    }
}

// File: Acme.Common/StringExtensions.cs
namespace Acme.Common.Extensions;

public static class StringExtensions
{
    public static string ToDisplayLabel(this string value) => $"[{value}]";
}
```

The domain models compile fine; only the controller breaks. What is missing, and why does `using static Acme.Common.Extensions.StringExtensions;` not fix it?

**Answer:** Extension methods are discovered by the namespace of the **static extension class**, not the extended type — the controller needs `using Acme.Common.Extensions;`. `using static` imports static members for direct calls (`ToDisplayLabel(id)`) but does **not** import extension methods for instance-style syntax.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Missing `using Acme.Common.Extensions;` | CS1061 — method not found on `string` |
| Misconception | `using static StringExtensions` expected to enable `id.ToDisplayLabel()` | Instance-style extension syntax still fails |
| API surface | Extensions hidden from IntelliSense in WebApi layer | Team thinks library reference alone is enough |

**Fix (priority order):**

1. Add `using Acme.Common.Extensions;` to the controller (or a global `GlobalUsings.cs` in the WebApi project).
2. Alternatively call explicitly: `StringExtensions.ToDisplayLabel(id)` — no extension `using` required.
3. Do **not** rely on `using static` for extension discovery — it only lifts static members, not extension method binding.

**Production takeaway:** Shared helper libraries fail at the call site, not the definition — Karat tests namespace import rules from **Program.cs** Section 8c. Convention: `*.Extensions` namespaces + document required `using` in README or analyzer.

---

---

#### Q3. (R) A null-safe helper was added for optional promo codes on checkout. Review the extension and its first production call:

```csharp
public static class StringExtensions
{
    public static string RequirePromoCode(this string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Promo code required.");
        return code.Trim().ToUpperInvariant();
    }
}

// CheckoutService:
string? promo = request.PromoCode; // may be null when omitted
string normalized = promo.RequirePromoCode(); // no compiler warning
```

The developer assumed "extension methods behave like instance methods on null." What actually happens at runtime, and how should the API be shaped for optional promo codes?

---

**Answer:**

```csharp
public static class StringExtensions
{
    public static string RequirePromoCode(this string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Promo code required.");
        return code.Trim().ToUpperInvariant();
    }
}

// CheckoutService:
string? promo = request.PromoCode; // may be null when omitted
string normalized = promo.RequirePromoCode(); // no compiler warning
```

The developer assumed "extension methods behave like instance methods on null." What actually happens at runtime, and how should the API be shaped for optional promo codes?

**Answer:** Unlike a true instance method, an extension **can** be invoked when the receiver is `null` — the compiler emits a static call and passes `null` as the first argument. `RequirePromoCode` then throws inside the method body (from `IsNullOrWhiteSpace`), but the developer lost nullable flow analysis because `this string` (non-nullable) does not warn on a `string?` receiver.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Null semantics | Extension call allowed on null receiver | Differs from instance-method NRE at call site |
| Nullable | `this string` on nullable receiver | No CS8602/CS8604 — silent null reaches helper |
| API design | `Require*` throws on missing optional field | 500s for valid "no promo" checkout paths |

**Fix (priority order):**

1. For optional promos, use a null-tolerant extension with `this string?` — mirror **Program.cs** `IsNullOrBlank` (Section 8d).
2. Split APIs: `NormalizePromoCode(this string code)` (non-null precondition) vs `TryNormalizePromo(this string? code, out string normalized)`.
3. At the call site, guard before require: `if (!promo.IsNullOrBlank()) { … }` or pattern-match nullable promo in the service layer.
4. Enable nullable reference types project-wide so `this string` vs `this string?` documents intent.

```csharp
public static string? NormalizePromoOrNull(this string? code) =>
    string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
```

**Production takeaway:** Extensions on reference types are a common null trap — Karat checks whether you know null is passed **into** the static method, not blocked at the call site like instance dispatch.

---

---

#### Q4. (R) Two NuGet packages ship extensions on `string` with the same signature. After adding both, CI builds but behavior flipped in staging:

```csharp
// Package A — Acme.Text.JsonHelpers
namespace Acme.Text.JsonHelpers;
public static class StringJsonExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().Replace("\"", "'");
}

// Package B — Contoso.Security
namespace Contoso.Security;
public static class StringSecurityExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().ToLowerInvariant();
}

// Startup (both usings present):
using Acme.Text.JsonHelpers;
using Contoso.Security;

var safe = userInput.Sanitize(); // now calls Contoso's version
```

What binding rule caused the silent behavior change, and what are your options to make the call explicit and stable?

---

**Answer:**

```csharp
// Package A — Acme.Text.JsonHelpers
namespace Acme.Text.JsonHelpers;
public static class StringJsonExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().Replace("\"", "'");
}

// Package B — Contoso.Security
namespace Contoso.Security;
public static class StringSecurityExtensions
{
    public static string Sanitize(this string input) =>
        input.Trim().ToLowerInvariant();
}

// Startup (both usings present):
using Acme.Text.JsonHelpers;
using Contoso.Security;

var safe = userInput.Sanitize(); // now calls Contoso's version
```

What binding rule caused the silent behavior change, and what are your options to make the call explicit and stable?

**Answer:** When multiple extension methods match, the compiler picks the **most specific** `this` type match; if still tied, **namespace/usings order** and internal tie-break rules apply — one extension wins at compile time with no runtime error. Adding a second package with the same signature can silently rebind the call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Binding | Duplicate extension signatures in scope | Behavior change without compile failure |
| Maintainability | `Sanitize` name collision across packages | Staging/prod diverge when usings reorder |
| Security | Wrong sanitizer (JSON vs security) | Data corruption or missed normalization |

**Fix (priority order):**

1. Call explicitly via static syntax: `StringJsonExtensions.Sanitize(userInput)` — unambiguous, refactor-safe.
2. Remove one `using` and fully-qualify the chosen extension class.
3. Rename internal extensions to domain-specific names (`SanitizeForJson`, `SanitizeForLog`) — avoid BCL-style generic names on `string`.
4. Remember: **instance methods always beat extensions** with the same signature — extensions never override existing instance API (**Program.cs** Section 8e).

**Production takeaway:** Extension conflicts do not throw — they compile and swap implementations. Prefer explicit static calls at integration boundaries (security, serialization).

---

---

#### Q5. (P) A logging extension on `IEnumerable<T>` looks convenient but skews metrics under load. Review:

```csharp
public static class EnumerableDiagnosticsExtensions
{
    public static IEnumerable<T> Tap<T>(
        this IEnumerable<T> source,
        Action<T> onEach)
    {
        foreach (var item in source)
        {
            onEach(item);           // logs every element
            yield return item;
        }
    }
}

// OrderReportService:
var premiumLines = _cache.GetLines(orderId)
    .Where(l => l.UnitPrice >= 50m)
    .Tap(l => _logger.LogDebug("Premium line {Sku}", l.Sku))
    .ToList();

decimal total = premiumLines.Sum(l => l.LineTotal);
int count = premiumLines.Count(); // second pass — but source was already materialized
```

Assume `_cache.GetLines` returns a deferred `IEnumerable` backed by a live database query. A developer later removes `.ToList()` to "avoid an extra allocation." What breaks in production, and when should this extension materialize vs stay deferred?

---

**Answer:**

```csharp
public static class EnumerableDiagnosticsExtensions
{
    public static IEnumerable<T> Tap<T>(
        this IEnumerable<T> source,
        Action<T> onEach)
    {
        foreach (var item in source)
        {
            onEach(item);           // logs every element
            yield return item;
        }
    }
}

// OrderReportService:
var premiumLines = _cache.GetLines(orderId)
    .Where(l => l.UnitPrice >= 50m)
    .Tap(l => _logger.LogDebug("Premium line {Sku}", l.Sku))
    .ToList();

decimal total = premiumLines.Sum(l => l.LineTotal);
int count = premiumLines.Count(); // second pass — but source was already materialized
```

Assume `_cache.GetLines` returns a deferred `IEnumerable` backed by a live database query. A developer later removes `.ToList()` to "avoid an extra allocation." What breaks in production, and when should this extension materialize vs stay deferred?

**Answer:** `Tap` uses `yield return`, so it is **deferred** — the database query and logging run only when the pipeline is enumerated. Removing `.ToList()` while still calling `Sum` and `Count` (or any two consumers) re-executes the entire chain twice: double DB round-trips, double side effects in `Tap`, and inconsistent snapshots if data changes between enumerations.

- With `.ToList()`, enumeration happens once; `Sum`/`Count` operate on an in-memory list — correct for reporting totals.
- Without materialization, each terminal operator (`Sum`, `Count`, `foreach`) re-walks the deferred chain from `_cache.GetLines`.
- `Tap` side effects (logging) fire once per enumeration — log volume and DB load multiply with chained consumers.

**When to materialize vs defer:**

- **Materialize** (`ToList`, `ToArray`) when you need a stable snapshot, multiple passes, or bounded side effects — typical for report aggregation after filtering.
- **Stay deferred** when a single downstream consumer streams once (export pipeline, single `foreach`) and the source is cheap/idempotent.

**Production takeaway:** IEnumerable extensions compose like LINQ — deferred by default (**Program.cs** Sections 7 and 8h). Karat pairs extensions with enumeration cost, not just syntax.

---

---

#### Q6. (D) Your team debates where pricing rules belong for `OrderLine`. Option A adds extensions; Option B keeps methods on the type:

```csharp
// Option A — OrderLineExtensions.cs
public static decimal ApplyBulkDiscount(this OrderLine line, int tier) { /* 40 lines */ }
public static decimal ApplyRegionalTax(this OrderLine line, string region) { /* … */ }

// Option B — OrderLine.cs (sealed domain type)
public decimal ApplyBulkDiscount(int tier) { /* same logic */ }
```

The type is **sealed**, owned by your team, and referenced from API, tests, and a reporting job. When do extensions earn their place vs polluting discoverability, and what is your rule of thumb for third-party `HttpRequest`/`string` helpers vs domain types?

---

**Answer:**

```csharp
// Option A — OrderLineExtensions.cs
public static decimal ApplyBulkDiscount(this OrderLine line, int tier) { /* 40 lines */ }
public static decimal ApplyRegionalTax(this OrderLine line, string region) { /* … */ }

// Option B — OrderLine.cs (sealed domain type)
public decimal ApplyBulkDiscount(int tier) { /* same logic */ }
```

The type is **sealed**, owned by your team, and referenced from API, tests, and a reporting job. When do extensions earn their place vs polluting discoverability, and what is your rule of thumb for third-party `HttpRequest`/`string` helpers vs domain types?

**Answer:** For **owned domain types** with core business rules (`ApplyBulkDiscount`, tax), prefer **instance methods on the type** (Option B) — discoverability, single place for behavior, and clearer unit tests. Reserve extensions for cross-cutting syntactic helpers that should not bloat the domain model, or when you **cannot** modify the type.

- **Use extensions on owned types sparingly:** formatting (`ToReceiptLine`), small adapters, or keeping `OrderLine` a pure data record while rules live in a policy service injected via DI.
- **Use extensions on BCL/third-party types:** `string`, `DateTime`, `HttpRequest`, `IEnumerable<T>` — you cannot add instance methods to sealed framework types (**Program.cs** Sections 3–4).
- **Avoid** putting 40-line pricing rules in extensions — they hide domain logic, bypass constructor/DI seams, and appear everywhere IntelliSense lists `OrderLine` methods.
- **Middle ground:** `OrderLine` stays immutable data; `IPricingPolicy` or domain service applies discounts — testable and mockable without static extension soup.

**Production takeaway:** Extensions extend surface area without extending responsibility — Karat tests judgment: `ToReceiptLine` on `OrderLine` fits; `ApplyRegionalTax` belongs on the type or a service, not a static helper class.

---

---

#### Q7. (P) An ASP.NET Core teammate models custom middleware as extension methods on `IApplicationBuilder`, mirroring `UseRouting` / `UseAuthentication`. Review this registration block:

```csharp
public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Correlation-Id"] = id;
            await next(); // forgot to push id into HttpContext.Items / ILogger scope
        });
        return app;
    }
}

// Program.cs:
app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Logs still cannot be correlated across services. What is wrong with the middleware body, and why is the extension-method shape (`this IApplicationBuilder`) the standard pattern here even though it is "just syntactic sugar"?

---

**Answer:**

```csharp
public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var id = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Correlation-Id"] = id;
            await next(); // forgot to push id into HttpContext.Items / ILogger scope
        });
        return app;
    }
}

// Program.cs:
app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

Logs still cannot be correlated across services. What is wrong with the middleware body, and why is the extension-method shape (`this IApplicationBuilder`) the standard pattern here even though it is "just syntactic sugar"?

**Answer:** The middleware echoes a correlation ID on the **response** but never stores it in `HttpContext.Items`, `Activity`/OpenTelemetry baggage, or an `ILogger` scope — downstream middleware, controllers, and `ILogger` output never see the ID. The extension-method shape is standard because it attaches fluent, discoverable pipeline entry points to `IApplicationBuilder` without modifying the framework type — same mechanism as `StringExtensions.ToDisplayLabel(this string)`.

- **Fix the body:** after resolving `id`, set `context.Items["CorrelationId"] = id` and wrap `await next()` in `using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))` or `Activity.Current?.SetTag(...)`.
- **Order:** correlation middleware should run **early** (before auth/logging-heavy middleware) so all subsequent components share the same ID — often immediately after `UseForwardedHeaders` / before `UseAuthentication`.
- **Why extension on `IApplicationBuilder`:** reads as `app.UseCorrelationId()` in `Program.cs`; groups middleware registration API in one static class; returns `IApplicationBuilder` for chaining — identical compiler rewrite to `CorrelationIdExtensions.UseCorrelationId(app)`.

**Production takeaway:** ASP.NET `Use*` methods are extension methods — Karat connects the C# feature to production pipeline ergonomics. Fixing the sugar without fixing `HttpContext`/logging scope leaves observability broken.

---

---

#### Q8. (M) Unit tests for a service that uses string extensions pass locally but fail in CI with `NullReferenceException`. Review the test setup:

```csharp
// Production code — Acme.Common.Extensions
public static class StringExtensions
{
    public static bool IsNullOrBlank(this string? value) =>
        string.IsNullOrWhiteSpace(value);
}

// Test project — no reference usings to Acme.Common.Extensions
public class CheckoutValidatorTests
{
    [Fact]
    public void Missing_promo_is_treated_as_blank()
    {
        string? promo = null;
        Assert.True(promo.IsNullOrBlank()); // fails in CI — CS1061 or runtime?
    }
}
```

The test project references the production assembly. Explain why extension methods are harder to mock than injected services, and what you would change if the team needs to swap validation rules per environment without `#if DEBUG` forks.

---

### 05. Func Action & Predicate

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/05. Func Action & Predicate`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
// Production code — Acme.Common.Extensions
public static class StringExtensions
{
    public static bool IsNullOrBlank(this string? value) =>
        string.IsNullOrWhiteSpace(value);
}

// Test project — no reference usings to Acme.Common.Extensions
public class CheckoutValidatorTests
{
    [Fact]
    public void Missing_promo_is_treated_as_blank()
    {
        string? promo = null;
        Assert.True(promo.IsNullOrBlank()); // fails in CI — CS1061 or runtime?
    }
}
```

The test project references the production assembly. Explain why extension methods are harder to mock than injected services, and what you would change if the team needs to swap validation rules per environment without `#if DEBUG` forks.

**Answer:** With the production assembly referenced but no `using Acme.Common.Extensions;`, the test fails at **compile time** with CS1061 — extension methods are not instance members of `string`. If a duplicate local extension exists in the test project, CI may bind differently. Extensions are **static dispatch** — you cannot mock `promo.IsNullOrBlank()` with Moq/NSubstitute the way you mock `ICheckoutValidator.IsBlank(promo)`.

- **Why hard to mock:** extensions compile to static calls on a fixed class; no interface, no virtual slot, no DI seam.
- **Fix immediate CI failure:** add `using Acme.Common.Extensions;` or call `StringExtensions.IsNullOrBlank(promo)` explicitly.
- **Swappable rules per environment:** extract behavior behind an interface — `IStringNormalizer` / `ICheckoutValidator` injected into the service; keep thin extensions as one-liner wrappers over injected services only at the edges (API binding), not core validation.
- **Testing extensions directly:** unit-test the static extension class with plain xUnit/NUnit tests — no mocking needed for pure functions like `IsNullOrBlank`.

**Production takeaway:** Extensions are ideal for pure, stateless helpers on types you do not own; inject interfaces when behavior must vary, be mocked, or carry policy — Karat stacks syntax discovery (Q2) with testability judgment here.

---

### 05. Func Action & Predicate

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/05. Func Action & Predicate`

---

---

#### Q1. (R) A warehouse API reuses a shared filter delegate across `List<T>` and LINQ. The build fails after a refactor. What is wrong, and how do you fix it without duplicating filter logic?

```csharp
public class InventoryService
{
    private readonly Func<Product, bool> _inStockFilter = p => p.IsActive && p.StockQty > 0;

    public List<Product> GetPickList(List<Product> items) =>
        items.FindAll(_inStockFilter); // CS1503

    public IEnumerable<Product> GetPickListLinq(IEnumerable<Product> items) =>
        items.Where(_inStockFilter);
}
```

---

**Answer:**

**Answer:** `List<T>.FindAll` requires `Predicate<T>`, not `Func<T, bool>` — they have identical invoke shapes but are different delegate types with no implicit conversion. Store one shape and wrap at the boundary, or use lambdas at call sites so the compiler infers the expected type.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `FindAll(_inStockFilter)` — `Func<Product, bool>` → `Predicate<Product>` | CS1503; build fails |
| Design | Assumed semantic equivalence implies type equivalence | Refactors break when crossing BCL APIs |
| Maintainability | Duplicating filter logic in two delegate variables | Drift between List and LINQ paths |

**Fix (priority order):**

1. Pick a canonical stored type — usually `Func<Product, bool>` for LINQ-heavy code — and wrap for List APIs: `items.FindAll(p => _inStockFilter(p))` or `new Predicate<Product>(_inStockFilter.Invoke)`.
2. Alternatively store `Predicate<Product>` and wrap for LINQ: `items.Where(p => _predicate(p))`.
3. Extract the condition once: `private static bool IsInStock(Product p) => p.IsActive && p.StockQty > 0;` then method-group into either delegate type at each call site.
4. Prefer a single domain method or small `IProductFilter` when the rule is shared across many APIs — avoids delegate-type friction entirely.

**Production takeaway:** Karat embeds the **Program.cs** lesson — lambdas infer the parameter type at the call site, but **stored** delegates do not convert between `Predicate<T>` and `Func<T, bool>`. See Section 7 and QUICK REFERENCE.

---

---

#### Q2. (R) A teammate wires logging callbacks into a pick-list pipeline. Review the registration and invocation:

```csharp
public static void ProcessPickList(
    List<Product> items,
    Func<Product, decimal> lineTotal,
    Func<string> logHeader)   // intended: print banner once, return nothing
{
    logHeader(); // CS0029 — cannot convert void to decimal
    items.ForEach(p => Console.WriteLine($"{p.Sku}: {lineTotal(p):C}"));
}

// Startup:
ProcessPickList(
    catalog,
    CalculateLineTotal,
    () => Console.WriteLine("=== Pick list ==="));
```

What are the compile-time mistakes, and which built-in delegate types belong here?

---

**Answer:**

```csharp
public static void ProcessPickList(
    List<Product> items,
    Func<Product, decimal> lineTotal,
    Func<string> logHeader)   // intended: print banner once, return nothing
{
    logHeader(); // CS0029 — cannot convert void to decimal
    items.ForEach(p => Console.WriteLine($"{p.Sku}: {lineTotal(p):C}"));
}
```

What are the compile-time mistakes, and which built-in delegate types belong here?

**Answer:** `logHeader` is declared as `Func<string>` (returns `string`) but the lambda returns `void`, and the call site treats it like a side-effect callback. Void-returning work belongs on `Action` or `Action<string>`, not `Func`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Func<string>` assigned `() => Console.WriteLine(...)` | CS0123 / CS0029 — void is not a valid `Func` return |
| API design | `Func<string>` implies a computed header string | Misleading contract; callers expect a return value |
| Invoke | `logHeader()` used for effect only | Wrong type family — `Action` expresses intent |

**Fix (priority order):**

1. Change the parameter to `Action printHeader` (zero parameters, void return): `printHeader();`
2. If the banner string is needed elsewhere, use `Func<string> headerFactory = () => "=== Pick list ===";` and pass that separately from `Action<string> emit`.
3. Keep `Func<Product, decimal> lineTotal` — it returns a value; that is the correct choice.
4. Match **Program.cs** Section 4: side effects → `Action`; computations → `Func`.

**Production takeaway:** Using `Func` for void methods is one of the most common compile errors in callback-heavy code — Karat tests whether you reach for `Action` immediately. See QUICK REFERENCE — "Using Func for void method → CS0123."

---

---

#### Q3. (R) After making a filter optional, production throws intermittently when a branch has no active rule. Review:

```csharp
public IEnumerable<Product> FilterCatalog(
    IEnumerable<Product> items,
    Func<Product, bool>? rule)
{
    return items.Where(rule); // sometimes NullReferenceException at runtime
}

// Caller when no custom rule configured:
var visible = FilterCatalog(catalog, null);
```

What breaks at runtime, why does it pass some code paths, and what is the production-safe fix?

---

**Answer:**

```csharp
public IEnumerable<Product> FilterCatalog(
    IEnumerable<Product> items,
    Func<Product, bool>? rule)
{
    return items.Where(rule);
}
```

What breaks at runtime, why does it pass some code paths, and what is the production-safe fix?

**Answer:** LINQ's `Where` invokes the predicate for every element — passing `null` throws `NullReferenceException` on the first item, not at the call to `Where`. Branches that always supply a rule appear fine until configuration omits one.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Where(null)` — predicate invoked per element | NRE on first enumeration; looks like "random" prod failure |
| Nullability | Nullable parameter without default | Optional filter contract is unsafe |
| API | Deferred execution hides failure until `foreach`/materialization | Fails in reporting job, not at startup |

**Fix (priority order):**

1. Apply a default before LINQ: `rule ??= _ => true;` or `return items.Where(rule ?? (_ => true));`
2. Short-circuit when absent: `if (rule is null) return items;`
3. For optional callbacks in custom APIs, use `?.Invoke` pattern from **Program.cs** Section 6 — but LINQ operators require a non-null delegate; guard at the API boundary.
4. Add a unit test with `rule: null` that forces enumeration (`ToList()`) — catches deferred-execution traps.

**Production takeaway:** Null-safe invoke (`?.Invoke`) works for your own optional `Action`/`Func` fields; BCL LINQ methods never accept null predicates. See **Program.cs** Section 6 and Section 11 preview.

---

---

#### Q4. (P) An ASP.NET Core app registers a `Func<IServiceProvider, decimal>` factory in DI to read tax rate per request. Review startup:

```csharp
builder.Services.AddSingleton<Func<IServiceProvider, decimal>>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TaxOptions>>();
    return () => options.Value.Rate; // Func<decimal> closed over IOptions snapshot
});

builder.Services.AddScoped<ProductPricingService>();

public class ProductPricingService
{
    private readonly Func<decimal> _taxRate;

    public ProductPricingService(Func<decimal> taxRate) => _taxRate = taxRate;

    public decimal PriceWithTax(Product p) => p.UnitPrice * (1m + _taxRate());
}
```

What lifetime and resolution problems appear under load or with `IOptionsMonitor`, and how should factories be registered instead?

---

**Answer:**

```csharp
builder.Services.AddSingleton<Func<IServiceProvider, decimal>>(sp => { /* … */ });
builder.Services.AddScoped<ProductPricingService>();
// ProductPricingService ctor: Func<decimal> taxRate
```

What lifetime and resolution problems appear under load or with `IOptionsMonitor`, and how should factories be registered instead?

**Answer:** Registering the outer factory as singleton while resolving scoped or monitor-backed options inside it captures stale configuration and blurs request scope. Inject `IOptionsMonitor<TaxOptions>` or `Func<decimal>` via a scoped factory registration so each request gets current values.

- **Singleton factory + scoped dependencies:** `GetRequiredService` inside a singleton delegate can resolve scoped services from the root provider — invalid outside a scope (may throw or return wrong instance depending on version/options).
- **Closed-over `IOptions` vs `IOptionsMonitor`:** snapshot `IOptions<T>` inside a singleton `Func<decimal>` never sees updated `appsettings` or key-vault reloads.
- **`Func<decimal>` in scoped service:** acceptable when the func is registered scoped or when it reads from `IOptionsMonitor` per invocation, not once at singleton creation.

**Fix (priority order):**

1. Register per-request factory: `builder.Services.AddScoped<Func<decimal>>(sp => () => sp.GetRequiredService<IOptionsMonitor<TaxOptions>>().CurrentValue.Rate);`
2. Better: inject `IOptionsMonitor<TaxOptions>` directly into `ProductPricingService` — clearer than func indirection for a single value.
3. Reserve `Func<IServiceProvider, T>` singleton factories for truly stateless object creation (e.g., `Func<IServiceProvider, ILogger>` patterns) — not for request-scoped configuration reads.
4. Enable scope validation in development: `builder.Host.UseDefaultServiceProvider(o => o.ValidateScopes = true);` — surfaces captive dependencies early.

**Production takeaway:** Func factories in DI are convenient but do not bypass lifetime rules — Karat stacks delegate typing with DI scope traps. Aligns with **Program.cs** Section 2 — `Func<decimal>` as testable configuration reader, but lifetime must match how often the value may change.

---

---

#### Q5. (R) A pricing service accepts `Func<Product, decimal>` so callers can plug in "async catalog lookups." Review usage from a minimal API endpoint:

```csharp
public decimal GetExtendedPrice(Product p, Func<Product, decimal> unitPriceLookup)
{
    decimal unit = unitPriceLookup(p); // blocks
    return unit * 1.0825m;
}

app.MapGet("/price/{sku}", async (string sku, CatalogClient catalog) =>
{
    Func<Product, decimal> lookup = product =>
        catalog.GetUnitPriceAsync(product.Sku).Result;

    var product = new Product(sku, "Item", 0m, 1, true);
    return Results.Ok(pricing.GetExtendedPrice(product, lookup));
});
```

What are the async, scalability, and delegate-signature problems, and what signature should replace `Func<Product, decimal>`?

---

**Answer:**

```csharp
Func<Product, decimal> lookup = product =>
    catalog.GetUnitPriceAsync(product.Sku).Result;
```

What are the async, scalability, and delegate-signature problems, and what signature should replace `Func<Product, decimal>`?

**Answer:** `Func<Product, decimal>` cannot represent asynchronous work — forcing `.Result` blocks a thread and causes sync-over-async under load. The API should accept `Func<Product, CancellationToken, Task<decimal>>` (or a dedicated service interface) and `await` end-to-end.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `GetUnitPriceAsync` | Thread-pool starvation; potential deadlocks |
| Signature | Sync `Func` for I/O-bound lookup | Misleading contract hides async requirement |
| Scalability | Blocking minimal API delegate | Reduced throughput on catalog-bound endpoints |
| Design | Func used to smuggle async into sync shape | Callers repeat `.Result` at every site |

**Fix (priority order):**

1. Change to async API: `public async Task<decimal> GetExtendedPriceAsync(Product p, Func<Product, CancellationToken, Task<decimal>> unitPriceLookup, CancellationToken ct)` and `await unitPriceLookup(p, ct)`.
2. Prefer injecting `ICatalogClient` into the service instead of passing func per call — func parameter is for pluggable algorithms, not HTTP clients.
3. Endpoint: `return Results.Ok(await pricing.GetExtendedPriceAsync(product, catalog.GetUnitPriceAsync, ct));` — method group to compatible func or direct service call.
4. Never register `Func<Product, decimal>` in DI when the implementation performs I/O — use typed client + async methods.

**Production takeaway:** Func/Action/Predicate are synchronous delegate shapes — async work needs `Func<..., Task<T>>` and `await`, or an interface. See C# Module 06 — sync-over-async; this question stacks it with delegate choice.

---

---

#### Q6. (D) A team replaces every inventory rule interface with `Func<Product, bool>` parameters "to reduce boilerplate." Tests now require copying lambdas from production code. Compare:

```csharp
// Before
public interface IProductFilter { bool Include(Product p); }

// After
public class InventoryReport
{
    public decimal TotalValue(IEnumerable<Product> items, Func<Product, bool> include) { /* … */ }
}
```

What testability and design seams do you lose, and when is `Func<Product, bool>` still the right API surface?

---

**Answer:**

```csharp
public interface IProductFilter { bool Include(Product p); }
// After: Func<Product, bool> include everywhere
```

What testability and design seams do you lose, and when is `Func<Product, bool>` still the right API surface?

**Answer:** A named `IProductFilter` (or small record rule type) is a stable, mockable contract with discoverable implementations; bare `Func<Product, bool>` hides intent, prevents polymorphic composition, and pushes test doubles toward duplicating lambda logic instead of substituting a fake rule.

- **Lost seams:** no `Mock<IProductFilter>` / `FakeActiveOnlyFilter`; tests pass inline lambdas that mirror production conditions — refactors break tests silently.
- **Lost composition:** interfaces support chaining decorators (`AndFilter`, `OrFilter`); func parameters encourage copy-paste boolean expressions.
- **Lost discoverability:** "implements `IProductFilter`" is grep-friendly; anonymous funcs are not.
- **When `Func<Product, bool>` is right:** local helpers (`ProcessInventory` in **Program.cs** Section 9), LINQ-shaped APIs (`Where`), one-off pipeline parameters where the caller owns the logic and tests assert on outputs not filter identity.

**Fix (priority order):**

1. Keep domain rules as `IProductFilter` or static named methods (`IsActiveInStock`) method-grouped into funcs at boundaries.
2. Use `Func<Product, bool>` at the **pipeline edge** only — convert `filter.Include` to func internally if needed.
3. For Karat-style judgment: prefer named types on public service contracts; func for internal utility parameters.

**Production takeaway:** Func reduces ceremony but is not a free replacement for interfaces on bounded contexts — Karat tests whether you preserve test seams. **Program.cs** Section 8 — named `LineTotalCalculator` vs `Func` when the name is part of the contract.

---

---

#### Q7. (P) An API adds a custom endpoint filter using a predicate delegate. Review registration and behavior:

```csharp
builder.Services.AddSingleton<Func<HttpContext, bool>>(_ =>
    ctx => ctx.Request.Headers.ContainsKey("X-Warehouse-Id"));

app.MapGet("/pick-list", (IInventoryService svc) => svc.GetPickList())
   .AddEndpointFilter(async (ctx, next) =>
   {
       var gate = ctx.HttpContext.RequestServices
           .GetRequiredService<Func<HttpContext, bool>>();

       if (!gate(ctx.HttpContext))
           return Results.Unauthorized();

       return await next(ctx);
   });
```

What breaks for multi-tenant routing, testing, and filter ordering compared to `IEndpointFilter` or a typed authorization requirement?

---

**Answer:**

```csharp
builder.Services.AddSingleton<Func<HttpContext, bool>>(_ =>
    ctx => ctx.Request.Headers.ContainsKey("X-Warehouse-Id"));
```

What breaks for multi-tenant routing, testing, and filter ordering compared to `IEndpointFilter` or a typed authorization requirement?

**Answer:** A singleton `Func<HttpContext, bool>` encodes authorization as an untyped header check with no access to route data, user claims, or scoped tenant services — it is hard to test in isolation, runs late if registered inside an ad hoc filter, and cannot participate in policy-based auth.

- **Multi-tenant:** header presence ≠ valid warehouse; no correlation to route `{warehouseId}`, claim, or scoped `ITenantContext` — wrong tenant data can still be served if the header is spoofed without validation.
- **Testing:** must spin `HttpContext` and service provider to test a func pulled from DI; `IAuthorizationService` / policy tests are standard.
- **Ordering:** custom inline filter runs after routing but competes with auth middleware — warehouse checks belong in authorization policy or early middleware, not a one-off func gate duplicated per endpoint.
- **Singleton:** cannot inject scoped tenant/store services into the predicate without captive dependency.

**Fix (priority order):**

1. Replace with policy: `[Authorize(Policy = "WarehouseAccess")]` and `AddAuthorization` handler reading claim + route.
2. If endpoint-specific, implement `IEndpointFilter` as a typed class injecting `ITenantValidator` (scoped) — unit-test the filter class directly.
3. Use built-in `RequireAuthorization()` / `AddEndpointFilter<WarehouseFilter>()` — consistent ordering via `MapGroup` filters.
4. Drop singleton `Func<HttpContext, bool>` from DI — it hides security rules and blocks scoped dependencies.

**Production takeaway:** Func fits local predicates (`Predicate<Product>` on in-memory lists); HTTP gates need typed filters, policies, and scoped services — not a global bool func. Connects to **Program.cs** pipeline pattern (Section 9) at the wrong abstraction layer for ASP.NET.

---

---

#### Q8. (M) A generic helper tries to widen a discontinued-SKU predicate for use on the full catalog. Review:

```csharp
public sealed class DiscontinuedProduct : Product
{
    public DateTime? EndOfLifeDate { get; init; }
}

Predicate<DiscontinuedProduct> discontinuedOnly =
    p => p.EndOfLifeDate.HasValue;

Predicate<Product> catalogFilter = discontinuedOnly; // CS0029

List<Product> items = GetFullCatalog();
items.RemoveAll(catalogFilter);
```

Why does assignment fail despite `DiscontinuedProduct : Product`, and how does delegate variance differ from `IEnumerable<T>` assignment?

---

### 06. Closures

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/06. Closures`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
Predicate<DiscontinuedProduct> discontinuedOnly =
    p => p.EndOfLifeDate.HasValue;

Predicate<Product> catalogFilter = discontinuedOnly; // CS0029
```

Why does assignment fail despite `DiscontinuedProduct : Product`, and how does delegate variance differ from `IEnumerable<T>` assignment?

**Answer:** `Predicate<T>` is declared contravariant (`in T`) — you may assign a **wider** input predicate (`Predicate<Product>`) to a **narrower** slot (`Predicate<DiscontinuedProduct>`), but not the reverse. Widening `Predicate<DiscontinuedProduct>` to `Predicate<Product>` would let `RemoveAll` invoke the rule with plain `Product` instances that are not `DiscontinuedProduct`, so accessing `EndOfLifeDate` would be unsound.

- **`Predicate<in T>` (contravariant):** valid direction — `Predicate<Product> wide = p => p.IsActive; Predicate<DiscontinuedProduct> narrow = wide;` (callers pass `DiscontinuedProduct`, handler accepts any `Product`).
- **Invalid direction (this snippet):** `Predicate<DiscontinuedProduct>` → `Predicate<Product>` — catalog list can contain non-discontinued SKUs; the delegate body assumes derived-only members.
- **vs `IEnumerable<out T>` (covariant):** `IEnumerable<DiscontinuedProduct>` → `IEnumerable<Product>` works because you only **read** items out; delegate parameters are **inputs**, so variance flips.

**Fix (priority order):**

1. Keep the narrow predicate on `List<DiscontinuedProduct>` only; do not widen the delegate type.
2. For mixed catalogs, filter with a lambda that pattern-matches: `items.RemoveAll(p => p is DiscontinuedProduct d && d.EndOfLifeDate.HasValue);`
3. Or extract a safe `Product`-level rule that uses only `Product` members: `Predicate<Product> catalogFilter = p => !p.IsActive;`
4. Reuse `Predicate<Product>` on derived lists via contravariance: `List<DiscontinuedProduct> disc; disc.RemoveAll(wideProductPredicate);` — valid when the stored delegate is `Predicate<Product>`.

**Production takeaway:** Inheritance intuition from collections does not transfer to input delegates — Karat tests contravariance direction, not just "same shape." See **Program.cs** Section 5 — `Predicate<in T>`; variance governs which stored predicate can be reused across base/derived lists.

---

### 06. Closures

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/06. Closures`

---

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

**Answer:**

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

---
