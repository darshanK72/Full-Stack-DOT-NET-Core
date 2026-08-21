# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/02. Object Oriented Programming/08. Events`

---

#### Q1. (R) A WPF-style desktop app keeps growing in memory after users open and close account detail panels. Review this wiring. What keeps `AccountDetailPanel` instances alive, and how do you fix it?

**Answer:** The panel subscribes to `_account.BalanceChanged` with a lambda but never unsubscribes in `Dispose`, so the long-lived `BankAccount` publisher holds a delegate that captures `this` — the closed panel cannot be collected even after it is removed from the UI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | `+=` in constructor, no `-=` in `Dispose` | Publisher retains subscriber → memory leak |
| Handler target | Lambda captures `this` (the panel instance) | GC cannot reclaim disposed UI objects |
| Design | Shared singleton/static `BankAccount` outlives every panel | Leak accumulates on each navigation open/close |

**Fix (priority order):**

1. Unsubscribe in `Dispose` (or `IAsyncDisposable`) — store the handler in a field if you used a lambda so `-=` matches the same delegate instance.
2. Prefer a named instance method handler when possible: `_account.BalanceChanged += OnBalanceChanged;` and `-= OnBalanceChanged` in `Dispose`.
3. If the publisher outlives all subscribers, consider weak-event patterns or a mediator (`IMediator`, `Channel<T>`) for UI refresh instead of direct domain events.
4. Profile with a memory dump — look for `AccountDetailPanel` instances retained via `BankAccount` → multicast delegate chain.

```csharp
private readonly EventHandler<BalanceChangedEventArgs> _balanceHandler;

public AccountDetailPanel(BankAccount account)
{
    _account = account;
    _balanceHandler = (_, e) => RefreshBalanceLabel(e.NewBalance);
    _account.BalanceChanged += _balanceHandler;
}

public void Dispose()
{
    _account.BalanceChanged -= _balanceHandler;
}
```

**Production takeaway:** Events create implicit references from publisher to subscriber — Karat tests whether you treat `-=` as mandatory cleanup, not optional. See **Program.cs** Section 6 — subscribe/unsubscribe and **Section 4c** — publisher outlives handlers.

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

**Answer:** The null check and invoke are not atomic — another thread can unsubscribe between the `!= null` test and the call, leaving `BalanceChanged` null and throwing `NullReferenceException`. The idiomatic fix is null-conditional invoke: `BalanceChanged?.Invoke(this, e)`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Split null-check + direct invoke | Rare NRE when last handler unsubscribes during raise |
| Style | Verbose `if (BalanceChanged != null)` | Easy to regress during refactor away from `?.` |
| Threading | Non-atomic check-then-invoke | Same race as Q4; worse under concurrent UI/service threads |

**Fix (priority order):**

1. Restore null-conditional invoke inside `OnBalanceChanged`: `BalanceChanged?.Invoke(this, e);`
2. For multi-threaded publishers, copy to a local before invoke (see Q4): `var handler = BalanceChanged; handler?.Invoke(this, e);`
3. Keep raise logic centralized in `OnBalanceChanged` so derived classes override one hook — matches **Program.cs** Section 4c.
4. Add a unit test that unsubscribes a handler from inside another handler — reproduces the race without UI.

**Production takeaway:** Forgetting `?.` is a classic production footgun — zero subscribers is normal, not exceptional. See **Program.cs** QUICK REFERENCE — "Forgetting ?. before Invoke → NullReferenceException."

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

**Answer:** A public delegate field lets any caller invoke the callback chain or assign `null`, wiping every subscriber without their knowledge — breaking audit trails, tests, and domain integrity. The `event` keyword restricts outsiders to `+=` / `-=` only; only `PaymentGateway` may raise from inside the type (CS0070 blocks external `Invoke`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security / integrity | External `Invoke` fakes domain events | Downstream systems act on spoofed "payment completed" |
| Encapsulation | `= null` clears entire multicast chain | Silent loss of audit/logging handlers after test reset or bug |
| API contract | Callers cannot distinguish publisher vs subscriber responsibilities | Violates publisher/subscriber roles from **Program.cs** Section 1 |
| Compile-time safety | No CS0070 guard on external raise | Fake notifications ship to production undetected |

**Fix (priority order):**

1. Change to `public event Action<string>? PaymentCompleted;` and raise only from an internal `Publish(string message)` method.
2. Replace test `= null` with explicit `-=` per registered handler, or create a fresh gateway instance per test.
3. For cross-assembly extensibility, prefer interfaces + DI (`INotificationPublisher`) over exposed delegate fields.
4. Code-review rule: flag `public Action`/`Func` fields on domain types — require `event` or method-based hooks.

```csharp
public class PaymentGateway
{
    public event Action<string>? PaymentCompleted;

    public void CompletePayment(string receiptId)
    {
        // real gateway work...
        PaymentCompleted?.Invoke(receiptId);
    }
}
```

**Production takeaway:** **Program.cs** Section 5 — `UnsafeNotifier` vs `SafeNotifier` — same lesson at enterprise scale: events protect who may raise and who may clear subscribers.

---

#### Q4. (P) A background `BankAccount` service raises `BalanceChanged` from worker threads while the UI thread subscribes handlers. A developer uses only null-conditional invoke inside `OnBalanceChanged`:

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    BalanceChanged?.Invoke(this, e);
}
```

Under concurrent subscribe/unsubscribe, handlers are occasionally skipped or you see rare `NullReferenceException` in older .NET code paths. Explain the race and show the thread-safe raise pattern from this chapter.

**Answer:** `BalanceChanged?.Invoke` still reads the event field twice conceptually — between load and invoke another thread can `-=` the last handler and set the backing delegate to null, so some handlers never run or an older pattern throws. Copy the delegate reference to a local variable, then null-conditional invoke the copy so the invocation list is fixed for that raise.

- **Race:** Thread A loads non-null delegate → Thread B unsubscribes last handler (field becomes null) → Thread A invokes — skipped notification or NRE with explicit null-check code.
- **Thread-safe pattern (from this chapter):**

```csharp
protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
{
    EventHandler<BalanceChangedEventArgs>? handler = BalanceChanged;
    handler?.Invoke(this, e);
}
```

- **Why it works:** The local `handler` captures the multicast delegate snapshot at raise time; subsequent `+=`/`-=` on the event do not affect that snapshot.
- **Stronger option:** Custom `add`/`remove` accessors with a lock if subscribe/unsubscribe must be synchronized with raise — **Program.cs** Section 4d; default compiler accessors are usually enough once you copy locally.
- **UI note:** Even with a safe raise, handlers that touch UI controls must marshal to the UI thread (`Dispatcher`, `SynchronizationContext`) — thread-safe raise does not make handler bodies thread-safe.

**Production takeaway:** Null-conditional invoke fixes "no subscribers"; local copy fixes "subscribers changed mid-raise" — Karat stacks both. See **BankAccount.OnBalanceChanged** in **Program.cs** lines 215–219.

---

#### Q5. (P) An ASP.NET Core API registers a **Singleton** `OrderStateTracker` that exposes `event EventHandler<OrderPlacedEventArgs>? OrderPlaced`. Scoped services subscribe in their constructors to push SignalR updates. After a few thousand requests, memory climbs and old connections still receive events. What is wrong with this wiring, and what pattern replaces in-process events for web apps?

**Answer:** A singleton publisher lives for the app lifetime, but each scoped `OrderNotificationService` subscribes in its constructor and never unsubscribes — every request adds another handler to the same event, retaining disposed scopes, `IHubContext` captures, and stale SignalR targets until the process recycles.

- **DI lifetime mismatch:** Singleton event source + scoped subscriber constructor subscription = unbounded handler list growth per HTTP request.
- **Memory:** Each handler closes over `hub` and possibly request state — GC cannot collect completed requests still referenced by the delegate chain.
- **Correctness:** Old handlers fire on new orders — clients see duplicate or ghost notifications from recycled connection ids.

**Fix (priority order):**

1. **Do not** subscribe in scoped service constructors to singleton events without matching `-=` in `Dispose`/`IAsyncDisposable` — hard to get right in ASP.NET.
2. Prefer **`IOptions` + `IHostedService`**, a **singleton** broadcaster with explicit connection mapping, or **`IHubContext` injected into a singleton** that tracks groups — not per-request event handlers.
3. For domain decoupling in ASP.NET Core, use **`IMediator` (MediatR)**, **`Channel<T>`**, or **message bus** (Azure Service Bus, RabbitMQ) scoped to the unit of work — not classic C# events across DI lifetimes.
4. If events are required (e.g., `DbContext.SaveChanges` interceptors), keep subscriber lifetime **≤ publisher lifetime** and unsubscribe when scope ends.

```csharp
// Better: scoped handler invoked explicitly from application service, no singleton event
public sealed class OrderApplicationService
{
    private readonly IHubContext<OrderHub> _hub;
    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        // persist order...
        await _hub.Clients.Group(order.CustomerId).SendAsync("orderPlaced", order.Id, ct);
    }
}
```

**Production takeaway:** C# events assume you manage lifetimes manually — ASP.NET DI scopes do not auto-unsubscribe. Karat links **Events** to **DI lifetimes**: singleton + scoped event wiring is a production leak. Preview: **Program.cs** Section 6 — multi-handler wiring moves to ch.09 with service registration.

---

#### Q6. (D) Your team debates three ways to notify downstream code when `BankAccount` balance changes: (A) `public event EventHandler<T>`, (B) `public Action<T>?` callback field, (C) `INotificationService` injected and called directly from `Deposit`/`TryWithdraw`. When would you choose each in a production ASP.NET Core domain layer, and what is the unsubscribe/lifetime rule of thumb?

**Answer:** In ASP.NET Core domain services, prefer **(C) injected abstractions** for application boundaries; use **(A) events** for in-process, same-lifetime object graphs (UI controls, short-lived aggregates with explicit cleanup); avoid **(B) public delegate fields** in production domain code except internal test doubles.

| Option | When to use | Lifetime rule |
|---|---|---|
| **(A) `event`** | Same-assembly domain objects, UI binding, aggregates where subscribers share publisher lifetime | Every `+=` needs matching `-=` when subscriber dies first; publisher must outlive or use weak patterns |
| **(B) `Action` field** | Rare — single callback slot, prototype code, serializer-friendly delegates you control entirely | Same as (A), plus anyone can `= null` or invoke — not for public APIs |
| **(C) `INotificationService` / MediatR** | ASP.NET Core services, cross-layer notifications, testability, multiple implementations | DI scope owns lifetime — no manual unsubscribe; singleton must not capture scoped services |

**Production guidance:**

- **Domain layer in API:** `BankAccount` should not expose public events to the web stack — call `INotificationService.PublishBalanceChanged(...)` from application services after persistence so lifetimes follow the request scope.
- **Console/UI tools:** Events match **Program.cs** tutorial — `BankAccount` + handlers in `Main` with clear subscribe/unsubscribe demo.
- **Testing:** (C) is easiest to mock; (A) requires raising events or attaching test handlers with cleanup; (B) invites test code that clears production handlers with `= null`.
- **Rule of thumb:** If the subscriber has a **shorter lifetime than the publisher**, you must unsubscribe — or do not use events. If lifetimes are managed by DI, use interfaces instead of events.

**Production takeaway:** Events excel at decoupling within one process and one lifetime story; ASP.NET Core's scoped/singleton graph breaks that assumption — Karat expects you to pick the mechanism by **who raises, who listens, and who outlives whom**, not syntax preference alone.
