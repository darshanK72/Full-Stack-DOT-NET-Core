# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/03. Anonymous Methods`

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

#### Q4. (P) Your team inherits a .NET Framework 4.x WinForms/WPF codebase full of `delegate { … }` event handlers, `List<T>.FindAll(delegate …)`, and `ThreadPool.QueueUserWorkItem(delegate …)`. Product wants incremental modernization — no big-bang rewrite. Describe a safe migration strategy from anonymous methods to lambdas (or local functions), including what you verify before merging each touched file.

**Answer:** Touch only files you are already changing for a feature or bugfix; replace anonymous methods with lambdas or local functions in place, run existing tests, and add targeted tests wherever capture or event subscription is involved — never mass-convert unrelated legacy code without coverage.

- **Inventory per change:** Identify delegate type (`EventHandler`, custom delegate, `Action`/`Func`), explicit vs omitted parameter lists, and captured outer variables — omitted lists on non-void delegates must become explicit lambda parameters in modern Roslyn (see **Program.cs** Section 5).
- **Mechanical rewrite rules:** `delegate (T x) { return expr; }` → `x => expr`; multi-statement bodies → `(x) => { … }`; void handlers → `(s, e) => { … }` or statement lambda; `ThreadPool`/`Task` callbacks → prefer `Task.Run` / `async` with named local functions when stack traces matter.
- **Preserve behavior:** Capture semantics are equivalent — verify rules that depend on outer locals, especially config snapshots vs live reads (Q2). Event handlers: keep field-stored handler for `-=` if the type is disposable.
- **When to prefer local functions over lambdas:** Recursive helpers, `async` bodies needing clear names in logs, or rules that deserve unit tests (`static local function` or private named method).
- **Verify before merge:** Full build, UI smoke on affected forms, memory profile on open/close dialogs with event handlers, and any golden-file or integration tests for filtered lists previously using `FindAll(delegate …)`.

**Production takeaway:** Karat rewards incremental, test-backed migration — "prefer lambdas in new code; read `delegate { }` when maintaining older projects" (**Program.cs** Section 8 and Section 11), not blanket regex replacement.

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

**Answer:** When the anonymous method references `matchCount`, the compiler lifts it into a heap-allocated display class field — value type or not, captured locals are not stack-only after closure creation. An equivalent lambda mutates the same display-class field; capture semantics are identical.

- **While `CountMatchingOrders` runs:** `matchCount` may live on the stack frame, but the act of capturing copies it into a display class instance referenced by the delegate.
- **After return if delegate survives:** The display class (holding `matchCount`, and here also `rule`) lives on the heap until the delegate is unreachable — not on the stack.
- **Mutation:** `matchCount++` inside the anonymous method mutates the captured field, which is why the count updates correctly across invocations within the method — same pattern as **Program.cs** Section 7.
- **Lambda equivalent:** `o => { … matchCount++; … }` generates the same display class pattern; no behavioral difference.
- **Contrast with non-captured locals:** A local never referenced from the anonymous body truly stays stack-only and dies with the frame.

**Production takeaway:** "Value types live on the stack" is wrong for closures — Karat uses this to test closure mechanics before the dedicated Closures chapter. See **06. Closures** for loop-variable pitfalls; see **Program.cs** Section 7 — "compiler generates a display class."

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

**Answer:** Extract to named `static` methods (or testable factory methods) when the rule encodes business policy QA must assert independently; keep inline anonymous methods or lambdas only for one-off glue, UI wiring, or rules already covered by integration tests — and accept that `rule.Method?.Name` will read `"anonymous"` for diagnostics.

- **Refactor to named methods when:** The rule is stable business logic (corporate minimum, line-item cap), needs table-driven tests with edge orders, appears in multiple pipelines, or must show up in logs/metrics by name — mirror **Program.cs** `HasPositiveTotal` + `BuildMaxLineItemsRule` split.
- **Use factory + lambda/anonymous when:** The rule is parameterized (`maxAllowedItems`) and you will test the factory once with varied inputs rather than each call site — `BuildCorporateMinimumRule` pattern.
- **Keep inline closure when:** The behavior is truly local to one screen, throwaway, or purely orchestration; cost of extraction exceeds value — but document that `RunAllRules` failure messages will say `"anonymous"` for those entries.
- **Local function middle ground:** `OrderRule MaxItemsRule() => o => o.LineItemCount <= _config.MaxItems;` inside a testable static helper gives a name in stack traces without polluting class surface — good for ASP.NET Core private validation builders.
- **Do not refactor solely for syntax:** Replacing `delegate { }` with `=>` without extraction does not improve testability — extraction to named/testable members does.

**Production takeaway:** Anonymous methods are a maintainability signal, not a performance choice — Karat tests judgment on test seams vs brevity. Prefer lambdas or named methods in new code (**Program.cs** Section 8); use anonymous syntax only to match surrounding legacy style.
