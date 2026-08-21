# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/04. Functional Style Programming/03. Anonymous Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

#### Q4. (P) Your team inherits a .NET Framework 4.x WinForms/WPF codebase full of `delegate { … }` event handlers, `List<T>.FindAll(delegate …)`, and `ThreadPool.QueueUserWorkItem(delegate …)`. Product wants incremental modernization — no big-bang rewrite. Describe a safe migration strategy from anonymous methods to lambdas (or local functions), including what you verify before merging each touched file.

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
