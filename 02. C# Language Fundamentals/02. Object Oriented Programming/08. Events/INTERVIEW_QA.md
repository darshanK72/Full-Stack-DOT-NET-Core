# C# Events — Interview Q&A

## Foundation Questions

---

## Q1. What is an event in C#, and how does it differ from a plain delegate field?

**Concepts**
- `event` keyword wraps a delegate field
- External code: subscribe (`+=`) and unsubscribe (`-=`) only
- Cannot invoke or assign (`=`) from outside the declaring class
- Encapsulates the invocation list
- Multicast delegate as the underlying type

**Answer**

An event in C# is a member that wraps a delegate field and restricts what external code can do with it. External code can only subscribe (`+=`) and unsubscribe (`-=`); it cannot directly invoke the event or replace the entire subscription list with `=`. In contrast, a plain `public Action<string>? PaymentCompleted` field allows any code to invoke it directly, clear all subscribers with `= null`, or replace the list entirely. This means a test helper, a buggy module, or a malicious caller can fire fake notifications or silence real ones without the declaring class's knowledge. The `event` modifier enforces that only the declaring class can raise the event, protecting the publisher's control over when notifications are sent. The underlying type is always a multicast delegate — adding multiple subscribers creates a chain that is invoked in registration order.

---

## Q2. What is the `EventHandler<TEventArgs>` pattern, and why is it the standard for .NET events?

**Concepts**
- `EventHandler<TEventArgs>` delegate signature
- `object sender` and `TEventArgs e` parameters
- `EventArgs` subclass carries event-specific data
- Consistent calling convention across the BCL
- Enables generic event wiring and reflection

**Answer**

The standard .NET event pattern declares events as `EventHandler<TEventArgs>` where `TEventArgs` inherits from `EventArgs`. The delegate signature is `void(object sender, TEventArgs e)`. The `sender` is a reference to the object that raised the event — useful when the same handler is wired to multiple sources and needs to know which one fired. The `TEventArgs` subclass carries all event-specific data (old value, new value, context). This convention enables generic frameworks — WPF, WinForms, ASP.NET, unit-test assertion libraries — to subscribe to arbitrary events through reflection without knowing the specific delegate type. Defining custom delegates like `Action<decimal, decimal>` for events works but breaks this convention, preventing such framework integration. In new code, `EventArgs` subclasses should be immutable and sealed to avoid consumers mutating the event data after the raise.

---

## Q3. How do you safely raise an event in C#, and what is the null-conditional pattern?

**Concepts**
- Check for null before invoking
- Thread-safe local copy to avoid TOCTOU race
- `?.Invoke(sender, args)` as idiomatic pattern
- `EventHandler<T>?` null annotation
- Avoid double-checked locking for events

**Answer**

An event with no subscribers is `null`, so invoking it without a null check throws `NullReferenceException`. The old pattern was `if (BalanceChanged != null) BalanceChanged(this, e)`, but this has a race condition: between the null check and the invocation, another thread might unsubscribe all handlers, setting the delegate to null. The correct pattern is to capture a local copy first and invoke the copy: `var handler = BalanceChanged; handler?.Invoke(this, e)`. The local copy captures the delegate's current invocation list atomically; even if subscriptions change after the capture, the local reference remains valid and non-null. The null-conditional `?.Invoke` further handles the case where the local copy is null (no subscribers at the time of capture) without throwing. In modern C#, declaring the event as `EventHandler<T>?` with the nullable annotation communicates that the event may have no subscribers and makes the null check explicit at the call site.

---

## Q4. What is a multicast delegate, and what happens when one subscriber throws?

**Concepts**
- Invocation list of delegate targets
- All subscribers invoked in registration order
- Exception in one subscriber aborts remaining invocations
- `GetInvocationList()` for per-subscriber error handling
- No return value aggregation for events

**Answer**

A multicast delegate holds an ordered list of method references, all matching the same signature. When invoked, it calls each entry in the invocation list in the order they were registered. If one subscriber throws an unhandled exception, the exception propagates to the publisher's `Invoke` call, and the remaining subscribers in the list are not called. For events where all subscribers must be notified regardless of errors, the publisher must iterate `GetInvocationList()` and call each individually inside a try/catch: `foreach (var h in BalanceChanged.GetInvocationList()) { try { ((EventHandler<BalanceChangedEventArgs>)h)(this, e); } catch (Exception ex) { /* log */ } }`. This is more defensive but adds complexity. For typical UI or domain events, the simpler `?.Invoke` is sufficient because exceptions in subscribers represent programming errors that should surface immediately.

---

## Q5. What is an event memory leak, and how does it occur with lambda subscriptions?

**Concepts**
- Publisher holds a reference to subscriber via the delegate
- Subscriber cannot be GC'd while subscribed
- Lambda subscription — no reference to unsubscribe
- Closed-over variables kept alive by the lambda
- `Dispose()` pattern must unsubscribe

**Answer**

When an object subscribes to an event, the publisher's delegate invocation list holds a reference to the subscriber (or its handler method). As long as the publisher is alive and the subscription exists, the subscriber cannot be garbage collected — the publisher acts as a root keeping the subscriber alive. This becomes a memory leak when subscribers are short-lived but the publisher is long-lived. The problem is especially acute with lambda subscriptions: `_account.BalanceChanged += (_, e) => RefreshLabel(e.NewBalance)`. The lambda closes over `this` (the subscribing object), but there is no way to unsubscribe this specific lambda later because no reference to it was saved. The fix requires storing the handler in a field — `_balanceChangedHandler = (_, e) => RefreshLabel(e.NewBalance); _account.BalanceChanged += _balanceChangedHandler` — and unsubscribing in `Dispose()`: `_account.BalanceChanged -= _balanceChangedHandler`.

---

## Q6. What is the weak event pattern, and when should you use it?

**Concepts**
- WeakReference to subscriber prevents GC-rooting
- Subscriber collected even if still subscribed
- `WeakEventManager` in WPF BCL
- Custom implementation complexity
- Trade-off: correctness risk vs memory safety

**Answer**

The weak event pattern replaces strong delegate references with `WeakReference<T>` to subscriber handlers, so the publisher does not prevent the subscriber from being garbage collected. When the weak reference's target is GC'd, the next event raise finds the reference null and removes it from the list. WPF provides `WeakEventManager` for common scenarios. The use case is when the subscriber's lifetime should be independent of the publisher — for example, a UI element subscribed to a long-lived service. The trade-off is complexity and a correctness risk: if the subscriber is GC'd while the UI still logically needs the subscription, the handler silently stops being called with no error. In ASP.NET Core, the weak event pattern is rarely appropriate; the correct solution is using `IDisposable` scoping to explicitly unsubscribe when the consumer's scope ends. Weak events are primarily a WPF/MAUI pattern for UI components.

---

## Q7. What is `INotifyPropertyChanged`, and how does it leverage events?

**Concepts**
- `PropertyChanged` event on the interface
- Raised with property name when a property changes
- Used by data binding frameworks (WPF, MAUI, Blazor)
- `[CallerMemberName]` attribute for property name
- `SetField` helper pattern to reduce boilerplate

**Answer**

`INotifyPropertyChanged` declares a single event `event PropertyChangedEventHandler? PropertyChanged` that types raise whenever a property's value changes. Data binding frameworks subscribe to this event to refresh UI elements when domain model state changes. The event carries a `PropertyChangedEventArgs` with the property name string. Implementing it requires boilerplate in every setter: compare the new value to the backing field, update the field if different, and raise `PropertyChanged` with the property name. The `[CallerMemberName]` attribute on the notification method's parameter fills in the property name at compile time, eliminating hard-coded strings that become stale during refactors. A common `SetField<T>(ref T field, T value, [CallerMemberName] string name = "")` helper encapsulates the compare-update-raise logic, reducing each property setter to a single `SetField(ref _name, value)` call. In .NET 10, source generators can implement `INotifyPropertyChanged` automatically via `[ObservableProperty]` attributes.

---

## Q8. What are custom event accessors (`add`/`remove`), and when do you need them?

**Concepts**
- `add` and `remove` accessors replace `+=`/`-=` default
- Thread-safe subscription management
- Lazy event wire-up (only subscribe to underlying source when needed)
- Event forwarding / aggregation patterns
- No backing field when using custom accessors

**Answer**

By default, `event` uses a compiler-generated backing delegate field and simple `+=`/`-=` operations. Custom `add`/`remove` accessors replace this with explicit subscription logic: `public event EventHandler<T> MyEvent { add { /* ... */ } remove { /* ... */ } }`. When using custom accessors, there is no auto-generated backing field — you must manage the delegate list yourself (often using `Delegate.Combine` and `Delegate.Remove`). This pattern is used in several scenarios: thread-safe subscription management (locking around the delegate field); lazy initialization of an underlying event source (only subscribe to a slow external resource when someone is listening); event aggregation (forwarding subscriptions from multiple inner sources); and exposing an event on an interface while the implementation delegates to a different underlying event. In WinForms, control events use a custom `EventHandlerList` to store many events compactly without allocating a field per event.

---

## Q9. What is `IObservable<T>` / `IObserver<T>`, and how does it compare to events?

**Concepts**
- Push-based notification model
- `OnNext`, `OnError`, `OnCompleted` callbacks
- `IDisposable` subscription token
- Reactive Extensions (Rx.NET) for composition
- Stronger contract than events (termination, error propagation)

**Answer**

`IObservable<T>` and `IObserver<T>` define a push-based notification model more powerful than events. An `IObservable<T>` is a source that pushes a sequence of `T` values; an `IObserver<T>` receives them via `OnNext(T)`, and also receives `OnError(Exception)` for faults and `OnCompleted()` for stream end — a complete protocol for asynchronous sequences. Subscription returns an `IDisposable` whose `Dispose()` unsubscribes cleanly, solving the event unsubscription problem elegantly. Reactive Extensions (Rx.NET) builds a rich composition library over these interfaces: `Where`, `Select`, `Throttle`, `Merge`, `Combine` operators allow declarative transformation of event streams. Events are simpler for single notifications without sequencing semantics; observables are better when you need to combine, filter, buffer, or sequence notifications. In modern .NET, `System.Reactive` and `IAsyncEnumerable<T>` (for pull-based async) often replace events in complex eventing scenarios.

---

## Q10. What is thread safety with events, and why does `?.Invoke` not eliminate all races?

**Concepts**
- `+=`/`-=` not atomic — two threads can race on the backing field
- Local copy pattern (`var h = Event; h?.Invoke(...)`) avoids NRE race
- `lock` needed for subscribe/unsubscribe race on custom accessors
- `Interlocked` compound operations for lock-free event fields
- Handler invoked after unsubscription is still possible

**Answer**

The null-conditional local-copy pattern `var h = BalanceChanged; h?.Invoke(this, e)` prevents `NullReferenceException` by copying the delegate reference before the null check and invocation, so an unsubscription between check and invoke does not cause a crash. However, it does not prevent a handler from being invoked after the subscriber intends to unsubscribe: the copy was taken before the unsubscription, so the handler still runs once. For event publishers using custom accessors, the backing delegate field itself must be protected: two threads calling `+=` simultaneously can both read the current value, compute the combined delegate, and write back, with one overwriting the other's subscription. Using `Interlocked.CompareExchange` in the `add` accessor provides lock-free thread safety. Alternatively, a `lock` around the backing field assignment is simpler and sufficient for low-contention scenarios. In practice, subscribers must be designed to tolerate being called once after unsubscription, as eliminating this race requires additional signaling.

---

## Q11. When would you choose an event over an injected callback or an `INotificationService` dependency?

**Concepts**
- Event: open-ended, unknown consumers, zero-to-many subscribers
- Callback (`Action<T>`): exactly one consumer, known at construction
- `INotificationService` injection: DI-managed, single consumer, testable
- Event appropriate for extensibility/plugin scenarios
- Injected service for required behavior, event for optional behavior

**Answer**

Events are appropriate when a class needs to notify an unknown number of consumers about something that happened, without any dependency on who those consumers are. The domain object (`BankAccount`) should not know about audit loggers, UI labels, or analytics services — it raises `BalanceChanged` and whoever cares subscribes. This is the Observer pattern: zero, one, or many subscribers, decoupled from the publisher. An injected callback (`Action<BalanceChangedEventArgs>`) is appropriate when there is always exactly one consumer and the publisher should be constructed with that consumer. An injected service (`INotificationService`) is appropriate when the notification is a required part of the operation's behavior — if the notification must succeed for the operation to be considered complete — and when the service needs to be mocked in tests. The lifetime rule of thumb: if the subscriber has a shorter lifetime than the publisher (a UI panel subscribing to a domain object), use events and dispose-to-unsubscribe. If lifetimes are equal (both are DI singletons), an injected service or `IMediator`/event bus is more predictable.

---

## Q12. What happens when an event subscriber throws an exception during the raise?

**Concepts**
- Exception propagates back to the publisher's raise code
- Remaining subscribers in the invocation list are not called
- Publisher must handle or the exception unwinds the call stack
- `GetInvocationList()` for defensive raise
- Async event handlers and fire-and-forget risks

**Answer**

When the publisher calls `BalanceChanged?.Invoke(this, e)` and one subscriber's handler throws, the exception propagates from the delegate invocation back through the publisher's raise method. If the publisher does not catch it, the exception continues up the call stack — potentially crashing a background thread or being swallowed in an async fire-and-forget. All subscribers registered after the throwing subscriber never receive the notification for that raise. For events where all subscribers must be called regardless of individual failures — for example, audit logging that must not block balance changes — the publisher should iterate `GetInvocationList()` and catch per-subscriber. For async handlers, `async void` event handlers are especially dangerous: exceptions thrown inside them go to the thread pool's unhandled exception handler and can crash the process. The pattern for async event handling is to use `Task`-returning delegates and explicitly `await` them, or use a channel/queue to decouple the raise from the async processing.

---

## Gotcha Questions

---

## Q13. A lambda subscribed to an event holds a reference to `this`. After `Dispose()`, memory usage does not drop. Why?

**Concepts**
- Lambda captures `this` via closure
- Delegate chain in publisher holds reference to closed-over `this`
- `Dispose()` does not automatically unsubscribe
- Publisher as GC root keeping subscriber alive
- Fix: save handler reference, unsubscribe in `Dispose()`

**Answer**

The lambda `(_, e) => RefreshBalanceLabel(e.NewBalance)` captures `this` (the `AccountDetailPanel`) via a compiler-generated closure. This closure is stored in the delegate that the `BankAccount.BalanceChanged` event holds. As long as `BankAccount` is alive and the subscription exists, the delegate — and therefore the closure — and therefore `AccountDetailPanel` — are reachable from a GC root. Calling `Dispose()` on the panel does not automatically unsubscribe from `BalanceChanged`, so the reference chain persists. The fix is to save the handler in a field at subscription time and unsubscribe in `Dispose()`. `_handler = (_, e) => RefreshBalanceLabel(e.NewBalance); _account.BalanceChanged += _handler;` then in `Dispose()`: `_account.BalanceChanged -= _handler; _handler = null;`. After this, `BankAccount` no longer holds a reference to the panel, and the GC can collect it. Always implement `IDisposable` on objects that subscribe to events published by longer-lived objects.

---

## Q14. A delegate field `public Action<string>? PaymentCompleted` is used instead of `event`. A test resets it with `= null`. What production risks exist?

**Concepts**
- Public field allows assignment (`=`) from anywhere
- Any caller can replace all subscriptions
- Any caller can invoke the delegate directly with fake data
- `event` restricts to `+=`/`-=` from outside the class
- Security and correctness implications

**Answer**

A public delegate field has no protection. Any code with a reference to `PaymentGateway` can: (1) invoke `gateway.PaymentCompleted("Fake payment — ship order")`, triggering fulfillment logic with fabricated data; (2) set `gateway.PaymentCompleted = null`, silently clearing all audit subscriptions and payment handlers; (3) set `gateway.PaymentCompleted = myHandler`, replacing the entire invocation list rather than adding to it. In a test, using `= null` to reset the field before each test is convenient but in production the same mechanism allows any module to sabotage notifications. Declaring the field as `public event Action<string>? PaymentCompleted` restricts external code to `+=` and `-=`. The declaring class alone can invoke it and clear it. The test that previously used `= null` must instead unsubscribe known handlers individually or redesign the test to use separate instances. This restriction is the core value of the `event` keyword over a plain delegate field.

---

## Q15. A scoped `OrderNotificationService` subscribes to a singleton `OrderStateTracker` event in its constructor. After thousands of requests, memory climbs. Why?

**Concepts**
- Scoped service created per request
- Singleton event publisher holds reference to scoped handler
- Scoped service never GC'd — kept alive by singleton's event
- Captive dependency (scoped in singleton) with event variant
- Fix: `IHostedService` or `MediatR` for cross-lifetime notifications

**Answer**

`OrderStateTracker` is a singleton — it lives for the application's lifetime. `OrderNotificationService` is scoped — a new instance is created per HTTP request. In its constructor, `OrderNotificationService` subscribes a handler lambda (capturing `this`) to `OrderStateTracker.OrderPlaced`. Because the singleton holds a reference to every scoped handler via the event's invocation list, every scoped `OrderNotificationService` ever created is permanently rooted to the singleton and can never be GC'd. After thousands of requests, thousands of `OrderNotificationService` instances accumulate in memory, each holding its `IHubContext<OrderHub>`. The `IHubContext` holds connections, amplifying the leak. The fix is architectural: do not subscribe long-lived publisher events in short-lived scoped constructors. Instead, use a `IHostedService` with a singleton `IOrderEventChannel` (a `Channel<T>`) where the singleton pushes events and the scoped handler reads from the channel for its request lifetime, or use `MediatR` with `INotification`/`INotificationHandler` which the framework dispatches per-event without persistent subscriptions.

---

## Real-World Scenarios

---

## Q16. A balance notification system crashes when no UI is subscribed. Diagnose the null-check race and show the correct idiomatic raise pattern.

**Concepts**
- Non-thread-safe null check before invocation
- Race: null check passes, subscriber unsubscribes, invocation throws NRE
- Local copy pattern
- `?.Invoke` as idiomatic fix
- `protected virtual OnXxx(EventArgs)` raise method pattern

```csharp
public class BankAccount
{
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)
    {
        if (BalanceChanged != null)
        {
            BalanceChanged(this, e);   // race between null check and invocation
        }
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Race condition | Null check and invocation are separate steps | Unsubscription between steps causes NRE |
| Invocation style | Direct `BalanceChanged(this, e)` instead of `Invoke` | Less readable, same functional risk |
| Missing local copy | Delegate not captured before check | Concurrent unsubscription window exists |

**Fix priority list**
1. Replace with `BalanceChanged?.Invoke(this, e)` — the compiler captures a local copy automatically.
2. Alternatively, explicitly: `var h = BalanceChanged; h?.Invoke(this, e)`.
3. Use `protected virtual void OnBalanceChanged(BalanceChangedEventArgs e)` as the canonical raise method so derived classes can suppress or augment the raise safely.

**Answer**

The check `if (BalanceChanged != null)` and the subsequent invocation `BalanceChanged(this, e)` are two separate operations. Between the check (which passes) and the invocation, another thread can call `-=` until the invocation list is empty, setting the delegate to `null`. The invocation then dereferences `null` and throws `NullReferenceException`. The local-copy pattern is the correct fix: `var handler = BalanceChanged; handler?.Invoke(this, e)`. Capturing the delegate reference into a local variable is an atomic read (delegate assignment is reference-size and atomically readable on 32-bit and 64-bit platforms). Even if a thread unsubscribes between the local-copy assignment and the `?.Invoke`, the local copy is still a valid non-null delegate targeting the handlers that were registered at capture time, and the invocation succeeds. The `?.Invoke` then handles the case where the capture produced `null` (no subscribers at that moment). Wrapping the raise in a `protected virtual OnBalanceChanged(args)` method is the BCL convention for events, allowing derived classes to override raise behavior or suppress the event under specific conditions.

---

## Q17. Design a `BankAccount` that raises `BalanceChanged` safely from background threads. Show the thread-safe subscription pattern using custom event accessors.

**Concepts**
- `lock` on backing field for `add`/`remove` synchronization
- Local copy before invocation
- Custom `add`/`remove` accessors
- `Delegate.Combine`/`Remove` for explicit invocation list management
- Balance between lock granularity and performance

**Answer**

When `BalanceChanged` can be raised from worker threads while UI threads subscribe and unsubscribe, the default auto-generated backing field is not thread-safe for concurrent subscriptions. Custom accessors with a dedicated lock object provide the correct pattern. The backing field is a private delegate field; `add` acquires the lock, uses `Delegate.Combine` to add the new handler, and releases; `remove` similarly uses `Delegate.Remove`. The `OnBalanceChanged` raise method captures a local copy outside the lock (taking the lock only to read the field atomically) and invokes the local copy. This prevents holding the lock during invocation, which would deadlock if a handler tries to subscribe or unsubscribe on the same thread. The lock must be a dedicated private static `readonly object _eventLock = new()` rather than `this`, to prevent external code from deadlocking against the same monitor. In high-throughput scenarios, `Interlocked.CompareExchange` provides lock-free subscription management at the cost of a retry loop. For most domain objects, the lock-based approach is simpler and entirely adequate.

---

## Q18. An ASP.NET Core API exposes `event` for order notifications in a singleton service. Compare three notification approaches and choose one for production.

**Concepts**
- Option A: `event EventHandler<T>` — simple but lifetime risks
- Option B: `Action<T>` field — weaker encapsulation
- Option C: `INotificationService` injection — DI, testable, explicit
- Lifetime mismatch between singleton and scoped handlers
- Channel<T> / MediatR for web API scenarios

**Answer**

For a production ASP.NET Core domain layer, `INotificationService` injection (Option C) is the correct choice for behavior that is a required part of the operation — notifications the operation must send for correctness. The interface is registered in DI, is easily mocked in tests, and makes the dependency explicit in the constructor. Events (Option A) are appropriate for optional, extensibility-oriented notifications where the service does not know or care who is listening and notifications are best-effort — for example, metrics collection or diagnostics. The lifetime mismatch trap: a singleton raising `event EventHandler<T>` and scoped services subscribing in constructors (Option A as described in the scenario) causes the memory leak analyzed in Q15. For web APIs, the production-safe pattern for event-driven cross-service notification is a `Channel<OrderPlacedEvent>` (a producer/consumer queue) where the singleton writes events and a hosted background service reads and dispatches them. This decouples producer and consumer lifetimes entirely. A plain delegate field (Option B) should be banned from production code — its lack of encapsulation creates the risks described in Q14.
