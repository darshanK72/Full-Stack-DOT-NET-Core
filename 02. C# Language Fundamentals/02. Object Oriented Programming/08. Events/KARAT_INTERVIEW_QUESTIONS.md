# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/08. Events`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A WPF-style desktop app keeps growing in memory after users open and close account detail panels. Review this wiring. What keeps `AccountDetailPanel` instances alive, and how do you fix it?

```csharp
public sealed class AccountDetailPanel : IDisposable
{
    private readonly BankAccount _account;

    public AccountDetailPanel(BankAccount account)
    {
        _account = account;
        _account.BalanceChanged += (_, e) =>
            RefreshBalanceLabel(e.NewBalance);
    }

    public void Dispose() { /* panel removed from UI */ }

    private void RefreshBalanceLabel(decimal balance) { /* update UI */ }
}

// Caller creates panels on navigation:
var panel = new AccountDetailPanel(sharedAccount);
// ... user navigates away; panel.Dispose() called but memory does not drop
```

---

#### Q2. (R) After a refactor, balance notifications crash when no UI is subscribed. Review the publisher change:

```csharp
public class BankAccount
{
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        if (BalanceChanged != null)
        {
            BalanceChanged(this, e);  // was: BalanceChanged?.Invoke(this, e);
        }
    }
}
```

What breaks at runtime, and what is the idiomatic raise pattern in modern C#?

---

#### Q3. (R) A teammate exposes a notification hook as a public delegate field "for flexibility." Review usage from another assembly:

```csharp
public class PaymentGateway
{
    public Action<string>? PaymentCompleted;  // public field, not event
}

// Consumer startup:
gateway.PaymentCompleted += msg => _audit.Log(msg);

// Later, a test helper "resets" listeners before each test:
gateway.PaymentCompleted = null;

// Malicious or buggy caller in another module:
gateway.PaymentCompleted?.Invoke("Fake payment — ship order");
```

What production risks does this design create compared to `public event Action<string>? PaymentCompleted`?

---

#### Q4. (P) A background `BankAccount` service raises `BalanceChanged` from worker threads while the UI thread subscribes handlers. A developer uses only null-conditional invoke inside `OnBalanceChanged`:

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    BalanceChanged?.Invoke(this, e);
}
```

Under concurrent subscribe/unsubscribe, handlers are occasionally skipped or you see rare `NullReferenceException` in older .NET code paths. Explain the race and show the thread-safe raise pattern from this chapter.

---

#### Q5. (P) An ASP.NET Core API registers a **Singleton** `OrderStateTracker` that exposes `event EventHandler<OrderPlacedEventArgs>? OrderPlaced`. Scoped services subscribe in their constructors to push SignalR updates. After a few thousand requests, memory climbs and old connections still receive events. What is wrong with this wiring, and what pattern replaces in-process events for web apps?

```csharp
builder.Services.AddSingleton<OrderStateTracker>();
builder.Services.AddScoped<OrderNotificationService>();

public sealed class OrderNotificationService
{
    public OrderNotificationService(OrderStateTracker tracker, IHubContext<OrderHub> hub)
    {
        tracker.OrderPlaced += async (_, e) =>
            await hub.Clients.All.SendAsync("orderPlaced", e.OrderId);
    }
}
```

---

#### Q6. (D) Your team debates three ways to notify downstream code when `BankAccount` balance changes: (A) `public event EventHandler<T>`, (B) `public Action<T>?` callback field, (C) `INotificationService` injected and called directly from `Deposit`/`TryWithdraw`. When would you choose each in a production ASP.NET Core domain layer, and what is the unsubscribe/lifetime rule of thumb?
