# C# Delegates — Interview Q&A

---

## Foundation Questions

---

## Q1. What is a delegate in C# and how does it differ from a direct method call?

**Concepts**
- reference type wrapping a method signature
- compile-time type safety on signature matching
- runtime target selection
- System.MulticastDelegate base class
- behavior-as-data pattern

**Answer**

A delegate is a reference type that holds a type-safe pointer to one or more methods. When you declare a delegate type — for example `public delegate decimal ShippingRule(decimal weightKg, string zone)` — the compiler ensures that only methods whose signature matches exactly (same parameter types and return type) can be assigned to a variable of that type. This is fundamentally different from a direct method call, where the target is fixed at compile time and woven permanently into the call site's IL.

Because a delegate variable is just data, you can pass it as a method argument, store it in a collection, return it from a factory, or swap it at runtime without recompiling the caller. An order-processing pipeline can store different `ShippingRule` implementations for domestic, international, and express zones, and pick one dynamically based on the order at runtime. Under the hood every delegate type inherits from `System.MulticastDelegate`, which itself inherits from `System.Delegate`, giving every delegate built-in support for chaining multiple targets into a single invocation.

---

## Q2. How do you declare and instantiate a custom delegate type?

**Concepts**
- `delegate` keyword declaration
- namespace-scope vs nested type placement
- method group conversion
- explicit constructor form
- lambda assignment

**Answer**

You declare a delegate type with the `delegate` keyword, specifying the return type and parameter list that every compatible method must share. The declaration creates a new type — not a variable — and should normally appear at namespace scope when it is shared across multiple classes, or nested inside a class when it is an implementation detail.

```csharp
// Declaration (net10.0)
public delegate decimal PriceAdjuster(decimal price);

// Instantiation — three equivalent forms
PriceAdjuster adj1 = ApplyDiscount;                   // method group conversion
PriceAdjuster adj2 = new PriceAdjuster(ApplyDiscount); // explicit constructor
PriceAdjuster adj3 = price => price * 0.9m;           // lambda

static decimal ApplyDiscount(decimal price) => price * 0.9m;
```

The method group conversion form is idiomatic in modern C#. The compiler generates the explicit constructor call behind the scenes, so all three forms produce identical IL. Using a lambda is the most concise and lets you close over local variables without defining a named method. Since C# 10 and .NET 6, natural type inference for lambdas means you can often omit the delegate type entirely when the context makes it clear.

---

## Q3. What is method group conversion and when does the compiler perform it?

**Concepts**
- implicit conversion from method name to delegate instance
- overload resolution during conversion
- method group vs lambda
- compiler-generated delegate allocation
- Func/Action compatibility

**Answer**

Method group conversion is an implicit conversion that the C# compiler applies whenever a method name (without parentheses) appears in a context that expects a compatible delegate type. The compiler resolves overloads at conversion time, picks the method whose signature matches the target delegate, and emits a `newobj` IL instruction to allocate a delegate instance wrapping that method and (for instance methods) the target object.

This conversion fires for custom delegate types, built-in `Action`/`Func`/`Predicate` generics, and event subscriptions alike. For example, writing `button.Click += HandleClick` relies entirely on method group conversion — you never write `new EventHandler(HandleClick)` in modern code. One subtle point is that each conversion typically produces a new delegate object on the heap; if the same method group is converted repeatedly in a hot path (such as inside a loop), it can cause repeated allocations. Caching the delegate in a field or static variable eliminates that cost. Starting in .NET 5, the runtime introduced a delegate cache for static method groups, so many static conversions are allocation-free, but instance method groups still allocate.

---

## Q4. How do you invoke a delegate safely, and why is null checking necessary?

**Concepts**
- `Invoke` method vs direct invocation syntax
- null delegate variable
- null-conditional operator `?.Invoke`
- NullReferenceException risk
- thread-safe snapshot pattern

**Answer**

Calling a delegate variable as if it were a method — `myDelegate(arg)` — is syntactic sugar for `myDelegate.Invoke(arg)`. If the variable is `null` (no target has been assigned), both forms throw a `NullReferenceException`. The idiomatic null-safe pattern in modern C# is the null-conditional invocation: `myDelegate?.Invoke(arg)`. The compiler lowers this to a null check followed by a call, and it evaluates the variable only once, which is important because in multithreaded code the variable could theoretically be set to null between your explicit check and the call.

```csharp
// Unsafe
onOrderShipped(order);          // throws if no subscriber

// Safe — idiomatic modern C#
onOrderShipped?.Invoke(order);  // silently does nothing if null
```

In event patterns, a common thread-safe technique is to copy the field into a local variable before the null-conditional call: `var handler = OnOrderShipped; handler?.Invoke(order);`. Because delegate instances are immutable, capturing the reference in a local snapshot means that even if another thread removes the last subscriber between the copy and the call, `handler` still points to the old non-null instance and invokes safely.

---

## Q5. What are multicast delegates, and how does the invocation chain work?

**Concepts**
- multiple targets in one delegate variable
- `+=` and `-=` operators
- `Delegate.Combine` and `Delegate.Remove`
- invocation order (order of addition)
- immutable delegate instances

**Answer**

A multicast delegate is a delegate instance whose internal invocation list holds more than one method target. Every delegate type in C# is technically a multicast delegate because every delegate type inherits from `System.MulticastDelegate`. When you use the `+=` operator on a delegate variable, the runtime calls `Delegate.Combine`, which creates a brand-new delegate instance whose invocation list is the concatenation of the two operands' lists — it does not mutate the existing instance. The `-=` operator calls `Delegate.Remove` to create another new instance with the first occurrence of the right-hand target removed from the list.

```csharp
OrderAuditHandler audit = null;
audit += LogToConsole;
audit += LogToDatabase;
audit += SendSlackAlert;
audit?.Invoke("Order 42 shipped");   // all three fire in addition order
audit -= LogToDatabase;
audit?.Invoke("Order 43 shipped");   // only Console + Slack fire
```

The runtime calls each target in the order they were added. If the delegate has a non-void return type, each target's return value is discarded except the last one — only the final invocation's result is visible to the caller. Because delegate instances are immutable value-like objects (the reference can be replaced but the instance itself does not change), multicast combination is inherently thread-safe at the immutability level, though the variable holding the reference still needs synchronization in concurrent code.

---

## Q6. What are the built-in generic delegate types Func, Action, and Predicate?

**Concepts**
- `Action<T...>` — void-returning delegates
- `Func<T..., TResult>` — value-returning delegates
- `Predicate<T>` — boolean filter delegate
- generic type parameters (up to 16 for Func/Action)
- custom delegate type vs built-in trade-off

**Answer**

The .NET base class library ships three families of generic delegate types that cover the vast majority of callback shapes, eliminating the need to declare custom delegate types for most scenarios. `Action` and its generic variants (`Action<T>`, `Action<T1, T2>`, up to `Action<T1…T16>`) represent methods that return `void`. `Func<TResult>` and its variants (`Func<T, TResult>`, up to `Func<T1…T16, TResult>`) represent methods that return a value; the last type argument is always the return type. `Predicate<T>` is equivalent to `Func<T, bool>` and was introduced earlier specifically for collection-filtering patterns like `List<T>.FindAll`.

Because these generic types are structural matches — any method with the right shape is compatible — they make APIs far more flexible. A sorting method that accepts `Comparison<T>` (another built-in) can receive any lambda or named method matching `int (T, T)`. You should still declare a named delegate type when the semantic meaning matters for documentation and readability: `public delegate decimal ShippingRule(decimal weightKg, string zone)` is clearer in an API than `Func<decimal, string, decimal>`. In library APIs where the caller writes the delegate, named types also produce better IntelliSense and error messages.

---

## Q7. How does delegate covariance and contravariance work?

**Concepts**
- covariant return types on delegate assignment
- contravariant parameter types on delegate assignment
- `out` and `in` variance on generic delegate type parameters
- compile-time variance vs runtime behavior
- Func covariance / Action contravariance

**Answer**

Delegate variance allows you to assign a method to a delegate variable even when the method's signature is not an exact match, as long as the mismatch is "safe" with respect to the Liskov substitution principle. Covariance applies to return types: if a delegate type declares `Animal` as the return type, you can assign a method that returns `Dog` (a derived class) because any code expecting an `Animal` can safely use a `Dog`. Contravariance applies to parameter types: if a delegate type declares `Dog` as a parameter, you can assign a method that accepts `Animal` because the method can handle anything an `Animal` can represent, including a `Dog`.

```csharp
// Covariance — method returns Dog, delegate return type is Animal
Func<Animal> getAnimal = GetDog;    // Dog is-a Animal, so this is safe

// Contravariance — method accepts Animal, delegate parameter is Dog
Action<Dog> processDog = ProcessAnimal; // method can handle Animal → handles Dog too

static Dog GetDog() => new Dog();
static void ProcessAnimal(Animal a) => Console.WriteLine(a.Name);
```

Generic delegate types `Func` and `Action` encode this in their type parameter declarations: `Func<out TResult>` is covariant in the return, `Action<in T>` is contravariant in the input. This means `Func<Dog>` is assignable to `Func<Animal>`, and `Action<Animal>` is assignable to `Action<Dog>`. Custom delegate types can also participate in variance but only when used through interfaces; direct delegate variable assignments rely on method-level variance rather than generic variance annotations.

---

## Q8. How does the event keyword build on top of delegates?

**Concepts**
- `event` as an access modifier on a delegate field
- publisher-subscriber encapsulation
- restricted `=` assignment from outside the class
- EventHandler and EventHandler<TEventArgs> conventions
- `add`/`remove` accessors

**Answer**

The `event` keyword is an access modifier that sits on top of a delegate field and enforces publisher-subscriber encapsulation. Without it, any external code can overwrite the entire invocation list with a direct assignment (`publisher.OnShipped = null`), invoke the event directly, or inspect its contents. Marking the field with `event` restricts external code to only `+=` (subscribe) and `-=` (unsubscribe); only code inside the declaring class can invoke the delegate or assign to it directly. This is the standard pattern across all .NET UI frameworks and the BCL.

```csharp
public class OrderService
{
    public event EventHandler<OrderEventArgs>? OrderShipped;

    public void Ship(Order order)
    {
        ProcessShipment(order);
        OrderShipped?.Invoke(this, new OrderEventArgs(order));
    }
}

// Subscriber
service.OrderShipped += (_, e) => Console.WriteLine($"Shipped {e.Order.Id}");
```

The `event` keyword generates field-backed `add` and `remove` accessors automatically, similar to how a property generates `get`/`set`. You can define custom accessors when you need thread safety (using a `lock`), when you want to store subscriptions in a dictionary rather than a field (useful in classes with many events), or when you delegate the subscription to an inner event aggregator. The .NET convention is to use `EventHandler<TEventArgs>` as the delegate type, derive `TEventArgs` from `EventArgs`, and name the event in the present or past tense.

---

## Q9. What is GetInvocationList and when do you need it?

**Concepts**
- `Delegate.GetInvocationList()` method
- per-subscriber invocation control
- collecting individual return values
- exception isolation per target
- empty invocation list vs null

**Answer**

`Delegate.GetInvocationList()` returns an array of `Delegate` objects, one per subscriber in the multicast chain, in invocation order. By default, invoking a multicast delegate calls every subscriber in a single atomic operation — you cannot collect individual return values, and an exception thrown by one subscriber aborts the rest of the chain. When either of those behaviors is unacceptable, you iterate the invocation list manually.

```csharp
public decimal AggregateShippingQuotes(ShippingRule rule, decimal weight, string zone)
{
    if (rule is null) return 0m;
    decimal min = decimal.MaxValue;
    foreach (ShippingRule target in rule.GetInvocationList().Cast<ShippingRule>())
    {
        try
        {
            decimal quote = target(weight, zone);
            if (quote < min) min = quote;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Carrier quote failed, skipping");
        }
    }
    return min == decimal.MaxValue ? 0m : min;
}
```

This pattern is common in plugin or event aggregator systems where you want the cheapest quote from multiple shipping carriers, or where you want to log individual handler failures without letting one bad subscriber silence all the others. Note that `GetInvocationList()` on a non-null delegate with a single target returns a one-element array, not null — it is always safe to iterate.

---

## Q10. What are the trade-offs between delegates and interfaces for callbacks?

**Concepts**
- delegate — single-method, lightweight, composable
- interface — multi-method contract, named semantic intent
- dependency injection compatibility
- testability and mocking
- event aggregator pattern preference

**Answer**

Delegates and single-method interfaces overlap significantly in their capabilities: both let you pass behavior to a method and decouple the caller from the implementation. The practical choice comes down to several dimensions. Delegates are more concise when only one method is needed — `Func<Order, bool>` is far lighter than declaring an `IOrderFilter` interface with a single `bool Filter(Order o)` method and then implementing a class for every variation. Lambdas and method group conversions make wiring up delegates trivial. Delegates also compose naturally through multicast and LINQ chains.

Interfaces win when the callback involves multiple related methods (e.g., `IProgressReporter` with `OnStarted`, `OnProgress`, and `OnCompleted`), when you need named, documentable contract semantics, when you want the callback to carry state across calls through instance fields, or when the implementation will be registered in a DI container. Interfaces are also easier to mock in unit tests using frameworks like NSubstitute or Moq, though modern versions of those frameworks can mock delegates too. A useful heuristic: if you find yourself grouping several `Action`/`Func` parameters that always travel together, that cluster is telling you it wants to be an interface. Conversely, if you find yourself declaring a one-method interface only to enable lambdas, switch to a delegate.

---

## Q11. How does delegate chaining work with += and -= operators?

**Concepts**
- `+=` calls `Delegate.Combine`
- `-=` calls `Delegate.Remove`
- immutability — new instance on each operation
- removing a non-present target is a no-op
- null left-hand side with `+=`

**Answer**

Every `+=` on a delegate variable is syntactic sugar for `variable = Delegate.Combine(variable, newTarget)`, which allocates a brand-new delegate instance whose invocation list is the concatenation of the two operands. Every `-=` is `variable = Delegate.Remove(variable, targetToRemove)`, which scans the invocation list for the first occurrence of the right-hand delegate (matched by target object and method pointer equality) and returns a new instance with that entry removed. If the target is not found, `Delegate.Remove` returns the original delegate unchanged — it does not throw. If removing the last entry, it returns `null`, so the variable becomes null after the last subscriber unsubscribes.

One important consequence of immutability is that `+=` on a null variable is perfectly safe and simply assigns the right-hand delegate directly: `null += handler` evaluates to `handler`. This is why event fields are often declared without an initializer — the first `+=` brings them to life. Because each operation allocates a new instance, code that does `+=` thousands of times in a tight loop should prefer building the chain once and caching it, or consider a `List<Action>` if the invocation list is highly dynamic.

---

## Q12. How do async and await interact with delegates?

**Concepts**
- `async` lambda producing `Func<Task>` or `Action` (fire-and-forget risk)
- `async void` event handler convention
- awaiting delegate invocations
- exception propagation differences
- `Func<Task>` vs `Action` for async callbacks

**Answer**

Delegates can represent asynchronous operations, but the type you choose determines whether exceptions propagate and whether the caller can await completion. An `async` lambda assigned to `Action` or a void-returning delegate type produces a fire-and-forget operation: the lambda starts an async state machine, but the caller has no `Task` to await, and any exception thrown inside the lambda after the first `await` becomes an unobserved task exception rather than surfacing at the call site. This is the only valid use case for `async void` — event handlers — because event invocation syntax does not return a `Task`.

```csharp
// Fire-and-forget — exception is lost if unhandled
Action notify = async () => await SendEmailAsync(order);

// Awaitable — caller can observe completion and exceptions
Func<Task> notifyAsync = async () => await SendEmailAsync(order);
await notifyAsync();

// Event handler — async void is acceptable only here
button.Click += async (_, _) => await LoadDataAsync();
```

When designing a callback API that is expected to be async, always use `Func<Task>` or `Func<Task<TResult>>` rather than `Action`. This allows callers to write `async` lambdas that are properly awaited, so exceptions surface correctly and the pipeline can respect completion before moving on. For multicast async delegates, iterate `GetInvocationList()` and `await` each `Func<Task>` individually, or use `Task.WhenAll` to run them concurrently.

---

## Q13. What are closed-over variables in delegates and how do they behave?

**Concepts**
- closure — capturing outer variable by reference
- heap promotion of captured variables
- variable sharing across multiple delegates
- loop variable capture gotcha
- `readonly` and value-type capture semantics

**Answer**

When a lambda or anonymous method references a variable from the enclosing scope, the compiler creates a closure: a compiler-generated class that holds the captured variables as fields, and the lambda becomes a method on that class. The captured variable is promoted to the heap so it outlives the stack frame where it was declared. Crucially, the closure captures the variable itself — not a copy of its value at the moment of capture — so if the variable changes after the delegate is created but before it is invoked, the delegate sees the updated value.

This has an important consequence for loop variable capture. In older C# code (before C# 5 for `foreach`, and still today for `for` loops), capturing a loop counter in a lambda captures the single loop variable, so all lambdas end up seeing the same final value:

```csharp
var actions = new List<Action>();
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.WriteLine(i));  // all print "3"

actions.ForEach(a => a());
```

The fix is to copy the loop variable into a local inside the loop body: `int captured = i; actions.Add(() => Console.WriteLine(captured));`. Each iteration then closes over a distinct variable. In `foreach` loops over C# 5+ this problem is already fixed by the language specification, which mandates a fresh variable per iteration.

---

## Q14. How does delegate equality work?

**Concepts**
- structural equality by target object + method pointer
- reference equality for multicast chains
- `-=` matching semantics
- anonymous methods / lambdas are never equal across separate expressions
- cached delegate fields for reliable unsubscription

**Answer**

Two delegate instances are considered equal if they point to the same method on the same target object — the runtime compares the method pointer (the function pointer part of the delegate) and the target object reference. For static methods, only the method pointer is compared. This equality is used by `-=` when scanning the invocation list for the target to remove.

The consequence that surprises developers is that two separately created lambda expressions — even if their source text is identical — are never equal, because each lambda expression in source code produces a distinct delegate instance (and potentially a distinct closure class). This means subscribing with an inline lambda and then trying to unsubscribe with another inline lambda does nothing:

```csharp
service.OrderShipped += (_, e) => Console.WriteLine(e.Order.Id); // subscribe
service.OrderShipped -= (_, e) => Console.WriteLine(e.Order.Id); // does NOT unsubscribe
```

The correct approach is to cache the lambda in a field or local variable before subscribing, then pass the same variable to `-=`. This is another argument for using named methods when reliable unsubscription is important: method group conversions that refer to the same method on the same instance will compare equal, making `-=` work as expected.

---

## Q15. What is the difference between Delegate.Combine returning null and an empty invocation list?

**Concepts**
- `Delegate.Remove` returning null when last target removed
- null delegate variable vs delegate with empty list
- invocation of null throws NullReferenceException
- no "empty delegate" value exists in .NET
- pattern for always-safe invocation

**Answer**

In .NET, a delegate variable that holds `null` is fundamentally different from a delegate instance with an empty invocation list — the latter simply does not exist. The runtime does not model a "zero-subscriber" delegate; instead, it models "no delegate at all" as null. This design means that when `Delegate.Remove` strips the last entry from an invocation list, it returns `null` rather than an empty delegate instance. Assigning the result back to the field makes the field null, which is why the null-conditional invocation pattern `handler?.Invoke(...)` is necessary on every call site.

This is easy to forget when exposing events: if all subscribers unsubscribe, the backing field becomes null, and the publisher must guard every invocation. It also means you cannot do `someDelegate.GetInvocationList()` on a null variable — that throws immediately. If you need to represent "always safe to invoke with no effect," one common pattern is to initialize the delegate field with a no-op lambda: `private Action<Order> _onShipped = _ => {};`. This ensures the field is never null and avoids per-call null checks at the cost of one extra no-op invocation when there are no real subscribers.

---

## Q16. How does delegate variance interact with generic constraints?

**Concepts**
- variance applies only to reference types
- `in`/`out` annotations on generic delegate type parameters
- variance does not apply to value types
- covariant return requires reference type
- interaction with nullable reference types

**Answer**

Delegate variance — whether through method-level covariance/contravariance or through generic `in`/`out` annotations — applies only to reference types. The runtime cannot apply variance to value types like `int` or `struct` because value types are not substitutable through inheritance; a method returning `int` cannot be assigned to a delegate returning `object` even though boxing would produce an object, because the delegate contract includes the unboxed type and the IL calling convention. This restriction is enforced at the type-system level and produces a compile-time error if violated.

For generic delegate types like `Func<out TResult>`, the `out` annotation means the type is covariant in `TResult`, so `Func<Dog>` is a subtype of `Func<Animal>`. However, this variance only activates for reference type arguments: `Func<int>` is not a subtype of `Func<object>` even though `int` boxes to `object`. With nullable reference types enabled, `Func<Dog>` is assignable to `Func<Dog?>` (covariance works with nullability annotations) but not vice versa, because returning a possibly-null `Dog?` from a context expecting a non-null `Dog` would break the contract. These rules are consistent with the covariance of arrays and generic interfaces in .NET.

---

## Q17. How do you use delegates to implement a strategy pattern at runtime?

**Concepts**
- delegate as a strategy encapsulation
- dictionary of delegates for dispatch
- avoiding large if-else chains
- runtime strategy selection
- composability with LINQ

**Answer**

The strategy pattern traditionally requires an interface and a family of implementing classes, but delegates offer a lighter alternative when the strategy is a single operation. By storing delegates in a dictionary keyed by some discriminator value (an enum, a string, an order type), you replace a long `if-else` or `switch` chain with a single dictionary lookup. This approach is particularly clean when the strategies themselves are short enough to express as lambdas, or when they vary by deployment (loaded from configuration or a plugin assembly at startup).

```csharp
// net10.0
var shippingRules = new Dictionary<string, ShippingRule>
{
    ["domestic"]      = (kg, _)    => kg * 2.50m,
    ["express"]       = (kg, _)    => kg * 5.00m + 10m,
    ["international"] = (kg, zone) => zone == "EU" ? kg * 8m : kg * 12m,
};

decimal Quote(string ruleKey, decimal kg, string zone)
    => shippingRules.TryGetValue(ruleKey, out var rule)
        ? rule(kg, zone)
        : throw new InvalidOperationException($"Unknown rule: {ruleKey}");
```

Because the dictionary values are first-class objects, you can add, remove, or replace strategies at runtime without modifying existing code — satisfying the Open/Closed Principle. You can also compose strategies by storing multicast delegates in the dictionary, or by chaining LINQ `Select` and `Aggregate` over a list of `Func<Order, Order>` transforms to build a processing pipeline dynamically.

---

## Q18. How does GetInvocationList enable collecting per-subscriber results from a multicast delegate?

**Concepts**
- default multicast returns only last result
- per-target invocation via GetInvocationList
- casting each Delegate to the concrete delegate type
- aggregating results into a collection
- exception isolation per subscriber

**Answer**

When you invoke a multicast delegate that has a non-void return type, the runtime discards every return value except the one from the last subscriber in the invocation list. This is rarely what you want in plugin or aggregation scenarios. By calling `GetInvocationList()`, you retrieve an array of individual `Delegate` objects and can cast and invoke each one separately, collecting every return value.

```csharp
public IReadOnlyList<decimal> CollectAllQuotes(ShippingRule rule, decimal kg, string zone)
{
    var results = new List<decimal>();
    foreach (ShippingRule target in rule.GetInvocationList().Cast<ShippingRule>())
    {
        decimal quote = target(kg, zone);
        results.Add(quote);
    }
    return results;
}
```

This pattern also gives you exception isolation: wrap the individual call in a `try/catch` and log the failure without aborting the rest of the chain. In the default multicast invocation, an exception from subscriber N stops subscribers N+1 through M from running at all. By iterating `GetInvocationList()` manually you can decide independently for each subscriber whether to rethrow, log-and-continue, or accumulate errors. This is the standard approach in plugin systems, event aggregators, and any scenario where "best effort from all subscribers" is more valuable than "all or nothing."

---

## Gotchas

---

## Q19. Why does invoking a multicast delegate with a return type only give you the last result?

**Concepts**
- multicast invocation discards intermediate return values
- only the last invocation's return value is captured
- hidden data loss in subscriber chains
- GetInvocationList as the workaround
- API design implication

**Answer**

This is one of the most surprising behaviors for developers new to multicast delegates. When you invoke a delegate that holds multiple subscribers and has a non-void return type, the runtime calls each subscriber in order and silently discards every return value except the one from the final subscriber. The caller only ever sees the last result. There is no exception, no warning, and no indication that earlier results existed.

```csharp
Func<int, int> pipeline = x => x + 1;
pipeline += x => x * 10;
pipeline += x => x - 3;

int result = pipeline(5);
// result == (5 - 3) == 2  ← only the last subscriber's output
// (5 + 1) and (5 * 10) are computed and thrown away
```

The practical implication is that multicast delegates with return types are almost always a design mistake unless you are explicitly using `GetInvocationList()` to collect all results. APIs that chain transformations should compose functions explicitly (e.g., `x => transform1(transform2(x))` or a pipeline list), not rely on multicast. Event-style patterns avoid this entirely by using `void` return types (`Action`, `EventHandler`), which is part of why the .NET event convention mandates void return.

---

## Q20. What happens when one subscriber in a multicast chain throws an exception?

**Concepts**
- default invocation aborts chain on first exception
- subsequent subscribers never run
- GetInvocationList for exception isolation
- partial side-effect risk
- aggregate exception pattern

**Answer**

When the runtime invokes a multicast delegate and one subscriber throws, the exception propagates immediately to the call site and every subscriber after the failing one is silently skipped. This means a bug in subscriber number two can prevent subscribers three through N from ever running — a problem that is especially serious when those subscribers have critical responsibilities like releasing locks, committing transactions, or sending alerts.

Consider an order-shipped event with three subscribers: an email notifier, a warehouse update, and an analytics tracker. If the email notifier throws, the warehouse is never updated and the analytics tracker never fires. From the perspective of the publisher, the call appeared to partially succeed — one handler ran — but the system is now in an inconsistent state.

The only way to isolate subscribers is to iterate `GetInvocationList()` manually and wrap each call in a `try/catch`. You can accumulate all exceptions into an `AggregateException` and rethrow at the end to inform the publisher that something went wrong without starving later subscribers:

```csharp
List<Exception>? errors = null;
foreach (EventHandler<OrderEventArgs> h in OnShipped!.GetInvocationList())
{
    try { h(this, args); }
    catch (Exception ex) { (errors ??= []).Add(ex); }
}
if (errors is not null) throw new AggregateException(errors);
```

---

## Q21. Why does adding a lambda with += and removing it with -= (using a different expression) fail silently?

**Concepts**
- delegate equality by target object + method pointer
- each lambda expression is a distinct instance
- closure class identity
- cached delegate field pattern
- anonymous method unsubscription pitfall

**Answer**

Delegate removal via `-=` relies on equality: the runtime searches the invocation list for a subscriber that equals the right-hand operand (same target object and same method pointer). Lambda expressions compile to methods on compiler-generated closure classes, and two separate lambda expressions in source code — even with identical bodies — compile to different methods or different class instances. As a result, the delegate produced by the second lambda expression never equals the one produced by the first, and `-=` scans the list, finds no match, and returns the original delegate unchanged without any warning or error.

This silent no-op is a common source of memory leaks in long-lived objects: you subscribe a lambda in a constructor, try to unsubscribe it later with what looks like the same code, and the subscription persists forever, keeping the subscriber alive through the publisher's strong reference.

The fix is to store the lambda in a field, local, or static variable before subscribing, and pass the same variable to both `+=` and `-=`:

```csharp
private readonly EventHandler<OrderEventArgs> _onShipped;

public OrderTracker(OrderService svc)
{
    _onShipped = (_, e) => Track(e.Order);
    svc.OrderShipped += _onShipped;
}

public void Detach(OrderService svc) => svc.OrderShipped -= _onShipped;
```

Using a named method instead of a lambda also works, because method group conversions to the same method on the same instance produce equal delegates.

---

## Q22. What is the += null delegate pitfall and why does it work?

**Concepts**
- null left-hand side with `+=`
- Delegate.Combine(null, d) returns d
- uninitialized event field pattern
- implicit null initialization
- safe first subscription

**Answer**

When you write `myDelegate += handler` and `myDelegate` is currently `null`, the compiler expands this to `myDelegate = Delegate.Combine(myDelegate, handler)`, which becomes `myDelegate = Delegate.Combine(null, handler)`. The `Delegate.Combine` method treats a null first argument as an empty left side and simply returns the second argument. So the result is that `myDelegate` is now pointing to `handler` — the operation succeeds silently without a `NullReferenceException`.

This is intentional and relied upon throughout the BCL. Event fields in classes are declared without initialization — `public event EventHandler? OrderShipped;` — and the first subscriber's `+=` brings them from null to a real delegate instance. Developers do not need to write `OrderShipped = new EventHandler(...)` before the first subscription.

The important asymmetry is with invocation: `myDelegate?.Invoke()` handles null safely, but `myDelegate(args)` without the null-conditional throws. The pattern is therefore: always initialize event fields as null (safe), use `+=` freely to subscribe (safe), but always use `?.Invoke` to fire (critical). Mixing `+=` carefreeness with direct invocation carefreeness is the source of `NullReferenceException` bugs in event-heavy code.

---

## Q23. How can closed-over loop variables cause subtle bugs in delegate-heavy code?

**Concepts**
- single loop variable shared by all iterations
- closure captures reference not snapshot
- all delegates see final loop value
- fix via local copy inside loop body
- `foreach` vs `for` loop distinction in C# 5+

**Answer**

When you create delegates inside a `for` loop and capture the loop variable, all delegates share a reference to the same variable — the one that changes on each iteration and ends at its final value when the loop completes. By the time any of those delegates is invoked (say, after the loop finishes), the loop variable holds its post-loop value, so every delegate computes using that final value rather than the per-iteration value you intended.

```csharp
var actions = new List<Action>();
for (int i = 0; i < 5; i++)
    actions.Add(() => Console.WriteLine(i));   // captures i, not a copy

actions.ForEach(a => a());  // prints 5, 5, 5, 5, 5 — not 0,1,2,3,4
```

The fix is to copy `i` into a new local variable declared inside the loop body. Each iteration's local is a distinct variable, so each closure captures its own independent copy:

```csharp
for (int i = 0; i < 5; i++)
{
    int copy = i;
    actions.Add(() => Console.WriteLine(copy));  // each closure captures its own copy
}
```

In C# 5 and later, `foreach` loops were fixed at the language level: the iteration variable is effectively re-declared on each iteration, so closures over `foreach` variables do not exhibit this problem. `for` loop counters, however, are still single shared variables and still require the copy pattern.

---

## Real-World Scenarios

---

## Q24. Design a callback-based progress reporting system using delegates for a long-running file import.

**Concepts**
- `IProgress<T>` pattern vs raw delegate
- progress callback delegate type design
- thread marshaling in UI callbacks
- null-safe invocation during progress
- decoupling importer from UI framework

**Answer**

A long-running file import should report progress without coupling the import logic to any specific UI framework. The cleanest approach is to define a callback delegate (or use `IProgress<T>`, which is the BCL standard for exactly this pattern) and let the caller supply the implementation — whether it writes to the console, updates a WPF progress bar, or logs to a file.

```csharp
// net10.0
public record ImportProgress(int ProcessedRows, int TotalRows, string CurrentFile);
public delegate void ImportProgressCallback(ImportProgress progress);

public class CsvImporter
{
    private readonly ImportProgressCallback? _onProgress;

    public CsvImporter(ImportProgressCallback? onProgress = null)
        => _onProgress = onProgress;

    public async Task ImportAsync(IEnumerable<string> filePaths, CancellationToken ct)
    {
        var files = filePaths.ToList();
        int total = files.Count;
        for (int i = 0; i < total; i++)
        {
            ct.ThrowIfCancellationRequested();
            await ProcessFileAsync(files[i]);
            _onProgress?.Invoke(new ImportProgress(i + 1, total, files[i]));
        }
    }
}

// Console caller
var importer = new CsvImporter(p =>
    Console.WriteLine($"[{p.ProcessedRows}/{p.TotalRows}] {p.CurrentFile}"));
await importer.ImportAsync(files, CancellationToken.None);
```

In a UI context, the progress callback should marshal back to the UI thread. The BCL's `Progress<T>` class does this automatically by capturing the synchronization context at construction time — so `new Progress<ImportProgress>(p => progressBar.Value = p.ProcessedRows)` created on the UI thread safely updates the control from a background thread. Using delegates (or `IProgress<T>`) rather than an interface keeps the importer testable: in unit tests you supply a lambda that records calls, and no mock framework is needed.

---

## Q25. How would you implement a simple event aggregator using delegates to decouple publishers and subscribers in a multi-layer application?

**Concepts**
- event aggregator pattern
- central hub holding delegate per event type
- type-safe subscription by message type
- weak reference subscriber management
- decoupling ViewModels from Services

**Answer**

An event aggregator is a central object that manages subscriptions and publications, allowing publishers and subscribers to communicate without knowing about each other. It replaces direct event subscriptions that would create strong coupling between layers. The simplest implementation uses a dictionary keyed by message type, with each entry holding an `Action<object>` delegate chain.

```csharp
// net10.0
public sealed class EventAggregator
{
    private readonly Dictionary<Type, Action<object>> _subscriptions = new();

    public void Subscribe<TMessage>(Action<TMessage> handler)
    {
        var key = typeof(TMessage);
        _subscriptions[key] = _subscriptions.TryGetValue(key, out var existing)
            ? existing + (obj => handler((TMessage)obj))
            : obj => handler((TMessage)obj);
    }

    public void Publish<TMessage>(TMessage message)
    {
        if (_subscriptions.TryGetValue(typeof(TMessage), out var handler))
            handler?.Invoke(message!);
    }
}

// Usage
var bus = new EventAggregator();
bus.Subscribe<OrderShippedMessage>(msg => Console.WriteLine($"Order {msg.OrderId} shipped"));
bus.Publish(new OrderShippedMessage(42));
```

In production, the aggregator is usually registered as a singleton in the DI container. ViewModels subscribe in their constructors and unsubscribe in `Dispose`. The main weakness of the simple implementation above is that it holds strong references to subscribers — if you forget to unsubscribe a ViewModel that gets discarded, the aggregator's delegate chain prevents GC. Production implementations either use weak references (with the trade-off of occasional silent failures when the subscriber has been collected) or enforce a strict subscribe-then-unsubscribe lifecycle.

---

## Q26. A colleague writes the following delegate-based plugin loader. Review it for defects.

**Concepts**
- multicast exception propagation
- missing null guard before invocation
- closed-over loop variable
- fire-and-forget async void in non-event context
- delegate removal failure with inline lambdas

**Answer**

```csharp
// net10.0 — code under review
public class PluginHost
{
    private Action<string>? _onMessage;

    public void Register(string[] pluginNames)
    {
        for (int i = 0; i < pluginNames.Length; i++)
        {
            string name = pluginNames[i];
            _onMessage += msg => Console.WriteLine($"[{name}] {msg}: iteration {i}");
        }
    }

    public void Broadcast(string message)
    {
        _onMessage(message);
    }

    public void Unregister()
    {
        _onMessage -= msg => Console.WriteLine("old handler");
    }

    public async void LoadAsync(string source)
    {
        await Task.Delay(100);
        _onMessage += msg => Console.WriteLine($"[{source}] {msg}");
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Null safety | `_onMessage(message)` throws NullReferenceException when no plugins are registered | Crash on first Broadcast before any Register call |
| Closed-over variable | `i` is captured by reference; all lambdas see `pluginNames.Length` after loop ends | All handlers print the post-loop index value, not per-iteration index |
| Delegate removal | Lambda in `Unregister` is a new instance; `-=` silently does nothing | Subscriptions never removed; memory leak |
| Async void | `LoadAsync` is `async void`, so exceptions are unobserved and callers cannot await completion | Unhandled exceptions crash the process; no way to coordinate loading |

**Fix priority:**
1. Change `_onMessage(message)` to `_onMessage?.Invoke(message)` to guard the null case.
2. The `name` local is already correctly copied before the lambda (good); remove the `i` reference from the lambda entirely — it serves no purpose and demonstrates the closed-over loop variable hazard.
3. Cache each lambda in a list alongside the plugin name so `Unregister` can remove the correct delegate instance.
4. Change `async void LoadAsync` to `async Task LoadAsync` and have callers `await` it; propagate exceptions through the returned `Task`.

---

## Q27. How would you use a delegate dictionary to replace a sprawling if-else chain that routes commands in a CQRS command dispatcher?

**Concepts**
- delegate dictionary as dispatch table
- `Type` as dictionary key
- avoiding reflection-heavy handler resolution
- compile-time registration vs runtime discovery
- open-generic handler resolution

**Answer**

A command dispatcher in a CQRS system often starts as a large `if-else` or `switch` chain that checks the command type and routes to the appropriate handler. As the number of commands grows, this file becomes a maintenance burden and a merge-conflict hotspot. Replacing it with a dictionary keyed by `Type`, where each value is an `Action<object>` or a typed handler delegate, gives O(1) dispatch and keeps registration decentralized.

```csharp
// net10.0
public sealed class CommandDispatcher
{
    private readonly Dictionary<Type, Func<object, Task>> _handlers = new();

    public void Register<TCommand>(Func<TCommand, Task> handler)
        => _handlers[typeof(TCommand)] = cmd => handler((TCommand)cmd);

    public Task DispatchAsync(object command)
    {
        var type = command.GetType();
        return _handlers.TryGetValue(type, out var handler)
            ? handler(command)
            : throw new InvalidOperationException($"No handler for {type.Name}");
    }
}

// Registration (startup)
var dispatcher = new CommandDispatcher();
dispatcher.Register<PlaceOrderCommand>(cmd => placeOrderHandler.HandleAsync(cmd));
dispatcher.Register<CancelOrderCommand>(cmd => cancelOrderHandler.HandleAsync(cmd));

// Dispatch (runtime)
await dispatcher.DispatchAsync(new PlaceOrderCommand(customerId, items));
```

Registration happens once at startup (in `Program.cs` or a DI extension method), so every new command type requires adding one line — not modifying a central `if-else`. The `Func<TCommand, Task>` delegate stored per type allows async handlers, and the cast inside the wrapper lambda is safe because the key was registered by the same `TCommand`. In a DI-based system, you can auto-register all `ICommandHandler<T>` implementations at startup and build this dictionary via reflection during the composition root phase, keeping individual handler classes free of dispatcher knowledge.

---

## Q28. How do delegates enable a middleware pipeline pattern similar to ASP.NET Core's RequestDelegate chain?

**Concepts**
- `RequestDelegate` as `Func<HttpContext, Task>`
- middleware as `Func<RequestDelegate, RequestDelegate>`
- pipeline construction via delegate composition
- short-circuiting without modifying other components
- functional pipeline vs OOP chain-of-responsibility

**Answer**

ASP.NET Core's middleware pipeline is built entirely on delegates. `RequestDelegate` is defined as `delegate Task RequestDelegate(HttpContext context)`. Each middleware is a function that takes the next `RequestDelegate` in the chain and returns a new `RequestDelegate` that wraps it — logically a `Func<RequestDelegate, RequestDelegate>`. The pipeline is assembled by folding the middleware stack in reverse order, so the first registered middleware is the outermost wrapper.

You can build the same pattern from scratch to understand it:

```csharp
// net10.0
using RequestDelegate = Func<HttpContext, Task>;
using MiddlewareFactory = Func<Func<HttpContext, Task>, Func<HttpContext, Task>>;

public class PipelineBuilder
{
    private readonly List<MiddlewareFactory> _middlewares = new();

    public PipelineBuilder Use(MiddlewareFactory mw) { _middlewares.Add(mw); return this; }

    public RequestDelegate Build()
    {
        RequestDelegate pipeline = _ => Task.CompletedTask; // terminal — 404 by default
        for (int i = _middlewares.Count - 1; i >= 0; i--)
            pipeline = _middlewares[i](pipeline);
        return pipeline;
    }
}

// Composition
var builder = new PipelineBuilder()
    .Use(next => async ctx => { Log(ctx); await next(ctx); })
    .Use(next => async ctx => { if (!IsAuth(ctx)) { ctx.Response.StatusCode = 401; return; } await next(ctx); })
    .Use(next => async ctx => await RouteAsync(ctx));

RequestDelegate app = builder.Build();
```

Each middleware closes over the `next` delegate it received, forming an immutable linked chain. Short-circuiting is simply returning early without calling `next`. This design is purely functional — there is no base class, no interface to implement, no visitor pattern — just composed delegates. The resulting pipeline is a single `Func<HttpContext, Task>` that carries the entire chain in its closure tree, making it trivially testable: pass in a fake `HttpContext` and inspect the state after the call.

---

## Q29. How do you implement a reliable async callback pattern where the caller can await the result and handle exceptions from subscriber code?

**Concepts**
- `Func<T, Task>` as awaitable callback type
- sequential vs parallel subscriber invocation
- exception propagation through Task
- AggregateException for multi-subscriber failure
- cancellation token threading through delegates

**Answer**

An awaitable callback pattern replaces void-returning event delegates with `Func<TArg, CancellationToken, Task>` delegates, allowing the publisher to await every subscriber and propagate exceptions back to the call site. This is the correct approach whenever the publisher needs to know that all subscribers have completed before proceeding — for example, ensuring all persistence operations finish before committing a transaction.

```csharp
// net10.0
public class AsyncEventBus<TMessage>
{
    private readonly List<Func<TMessage, CancellationToken, Task>> _handlers = new();

    public void Subscribe(Func<TMessage, CancellationToken, Task> handler)
        => _handlers.Add(handler);

    public void Unsubscribe(Func<TMessage, CancellationToken, Task> handler)
        => _handlers.Remove(handler);

    public async Task PublishAsync(TMessage message, CancellationToken ct = default)
    {
        var exceptions = new List<Exception>();
        foreach (var handler in _handlers)
        {
            try { await handler(message, ct); }
            catch (OperationCanceledException) { throw; }   // let cancellation bubble
            catch (Exception ex)               { exceptions.Add(ex); }
        }
        if (exceptions.Count > 0) throw new AggregateException(exceptions);
    }
}

// Usage
var bus = new AsyncEventBus<OrderShippedMessage>();
bus.Subscribe(async (msg, ct) => await emailService.SendAsync(msg.Order, ct));
bus.Subscribe(async (msg, ct) => await warehouseService.UpdateAsync(msg.Order, ct));

await bus.PublishAsync(new OrderShippedMessage(order));
```

By storing handlers in a `List<Func<...>>` rather than a multicast delegate, `Unsubscribe` works correctly with the exact same instance (because `List<T>.Remove` uses reference equality on the delegate objects, which are the same references that were added). Sequential invocation with individual `try/catch` ensures that a failing email sender does not prevent the warehouse update from running. Cancellation is treated specially — an `OperationCanceledException` propagates immediately, aborting the remaining handlers, because cancellation is a cooperative protocol, not an isolated subscriber error.
