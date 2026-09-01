# C# Func, Action & Predicate — Interview Q&A

## Foundation Questions

---

## Q1. What are `Func<T, TResult>`, `Action<T>`, and `Predicate<T>` and how do they differ?

**Concepts**
- generic delegate types in BCL
- return type distinction
- `Func` with up to 16 input parameters
- `Action` as void delegate
- `Predicate<T>` as specialized boolean filter

**Answer**

`Func<T, TResult>`, `Action<T>`, and `Predicate<T>` are pre-defined generic delegate types in the BCL that cover the most common delegate shapes. `Func` delegates always have a return value — the last type parameter is the return type, and there can be 0 to 16 input parameters before it. `Action` delegates have a `void` return and accept 0 to 16 input parameters. `Predicate<T>` is a specialized delegate equivalent to `Func<T, bool>` — it takes one input and returns a boolean. Because `Func` and `Action` cover nearly every use case, custom delegate type declarations are now rare in modern C#. `Predicate<T>` exists primarily for historical reasons, as older BCL APIs like `List<T>.FindAll` and `Array.FindAll` were designed before `Func` existed.

---

## Q2. When should you define a custom delegate type instead of using `Func` or `Action`?

**Concepts**
- named intent vs anonymous signature
- `ref`/`out`/`params` parameters
- readability and self-documenting APIs
- IDE experience improvement
- interop with legacy code

**Answer**

Custom delegate types are preferable when the signature needs `ref`, `out`, or `params` parameters — `Func` and `Action` cannot express those. They are also valuable when the delegate's purpose is the primary semantic contract of an API: a type named `DataTransformer<TInput, TOutput>` communicates intent far better than `Func<TInput, TOutput>` in a public library. Custom types also participate in inheritance hierarchies and attributes, and they produce clearer compiler error messages when a caller passes an incompatible method. For internal utility code, event wiring, and LINQ-style operations, `Func` and `Action` are preferred because they eliminate boilerplate and compose seamlessly with the rest of the BCL. The general heuristic: use `Func`/`Action` for implementation convenience and private APIs, consider custom types for stable public contracts.

---

## Q3. How does `Predicate<T>` relate to `Func<T, bool>`, and can you use them interchangeably?

**Concepts**
- same underlying signature
- different delegate type identity
- no implicit conversion between delegate types
- explicit wrapping required
- `List<T>.FindAll` vs LINQ `Where`

**Answer**

`Predicate<T>` and `Func<T, bool>` have exactly the same parameter and return type signature, but they are declared as separate delegate types, and the C# type system treats them as incompatible. You cannot pass a `Predicate<T>` where `Func<T, bool>` is expected, nor vice versa, without an explicit conversion. The standard workaround is to wrap one in the other: `Func<int, bool> f = x => x > 0; Predicate<int> p = x => f(x);`. LINQ's `Where` operator accepts `Func<T, bool>`, while `List<T>.FindAll` accepts `Predicate<T>` — this mismatch is the most common point where developers encounter the incompatibility. In new code, prefer `Func<T, bool>` to stay consistent with LINQ and avoid the wrapper ceremony. Use `Predicate<T>` only when calling older BCL methods that require it.

---

## Q4. What is a higher-order function, and how do `Func` and `Action` enable them in C#?

**Concepts**
- function accepting function as parameter
- function returning function
- abstraction over behavior
- strategy and template method alternatives
- composability principle

**Answer**

A higher-order function either accepts a function as an argument or returns a function as its result. `Func` and `Action` make higher-order functions first-class in C# by giving functions a concrete type that can be stored, passed, and returned. A method signature like `IEnumerable<T> Filter<T>(IEnumerable<T> source, Func<T, bool> predicate)` accepts behavior as data — callers supply the filtering logic at the call site rather than through subclassing or interface implementation. Returning a `Func` is equally powerful: `Func<int, int> Multiplier(int factor) => x => x * factor;` creates a closure-backed function factory. Higher-order functions are the mechanism behind LINQ's composable operators, middleware pipelines, retry policies, and many testing utilities. They allow behavior parameterization without the verbosity of the strategy pattern's traditional interface-per-operation design.

---

## Q5. How do you store multiple operations as a dispatch table using `Func` or `Action`?

**Concepts**
- dictionary keyed by discriminator
- runtime behavior selection
- replacing switch/if-else chains
- open/closed principle support
- command pattern relationship

**Answer**

A dispatch table is a `Dictionary<TKey, Func<...>>` or `Dictionary<TKey, Action<...>>` that maps a discriminator value to executable behavior. Instead of a `switch` that must be updated whenever a new case is added, you populate the dictionary at startup and look up and invoke the right function at runtime: `var handlers = new Dictionary<string, Action<Order>> { ["place"] = PlaceOrder, ["cancel"] = CancelOrder }; handlers[command](order);`. This pattern separates registration from execution, supports open extension (add new entries without modifying existing code), and is easy to test — each function can be tested in isolation and the dispatch itself is a trivial lookup. The pattern underpins command buses, plugin architectures, and any system where new operations are added frequently. The tradeoff is that missing keys require explicit handling (either a default entry or a `TryGetValue` check followed by a fallback).

---

## Q6. What is the difference between `Func<Task>` and `async void` when used as callbacks?

**Concepts**
- `async void` fire-and-forget danger
- `Func<Task>` awaitable callback
- exception propagation difference
- caller's ability to await
- event handler exception model

**Answer**

`Func<Task>` returns a `Task` that the caller can await, observe for exceptions, and chain further work onto. When you invoke a `Func<Task>` callback, you typically `await` the returned task, which means any exception thrown inside the async body propagates through the awaiter and surfaces at the `await` site. `async void` methods start a fire-and-forget operation: the caller receives no `Task`, cannot await completion, and cannot observe exceptions — they propagate to the `SynchronizationContext` and can crash the process if unhandled. Use `async void` only for event handlers, where the pattern is mandated by the delegate type. For all other async callback scenarios — retry wrappers, middleware, timer callbacks, custom event aggregators — define the callback as `Func<Task>` or `Func<T, Task>` so callers retain control over the async execution. This is one of the most consequential distinctions in async C# programming.

---

## Q7. How can you compose multiple `Func` delegates into a pipeline?

**Concepts**
- function composition pattern
- `Func<T, T>` chaining
- `Aggregate` for pipeline assembly
- extension method for compose
- order of application

**Answer**

Function composition combines two functions `f` and `g` into a single function where the output of `f` feeds into `g`. In C#, you can compose `Func<T, T>` delegates using an extension method or LINQ's `Aggregate`: `Func<string, string> pipeline = transforms.Aggregate((f, g) => x => g(f(x)));`. Each element wraps the previous in a new lambda, creating a chain where input passes left-to-right through all transforms. This is the functional equivalent of the decorator or pipeline pattern without inheritance. ASP.NET Core's middleware pipeline uses a similar approach with `Func<RequestDelegate, RequestDelegate>` where each middleware wraps the next. The key design consideration is ensuring the intermediate types match — composing `Func<T, T>` is simpler than composing functions with different input and output types, which requires careful ordering and type alignment.

---

## Q8. How does `Func<T>` enable lazy evaluation?

**Concepts**
- deferred computation
- value factory pattern
- `Lazy<T>` comparison
- conditional materialization
- expensive object avoidance

**Answer**

`Func<T>` wraps a computation and defers its execution until the function is explicitly called. Instead of computing a value eagerly, you pass `Func<T>` as a value factory and invoke it only when — and if — the value is needed. A common pattern is `GetOrAdd(key, Func<TValue> factory)` where the factory runs only on a cache miss. `Lazy<T>` is built on the same principle but adds thread-safe initialization and single-execution guarantees via double-checked locking internally. For non-thread-safe lazy evaluation or single-use value factories, a raw `Func<T>` is leaner. Lazy evaluation also appears in default parameter patterns: `T GetValueOrDefault(T value, Func<T> defaultFactory)` avoids computing the default when `value` is already present, which matters when the default is expensive (e.g., a database call or object construction).

---

## Q9. What does it mean to partially apply a function using `Func` in C#?

**Concepts**
- partial application definition
- currying relationship
- closure-based parameter binding
- fixed argument capture
- function factory pattern

**Answer**

Partial application means creating a new function by pre-supplying some arguments to an existing function, leaving the remaining arguments to be supplied later. C# does not have built-in syntax for this, but you can simulate it with closures over `Func`. For example, `Func<int, int, int> add = (a, b) => a + b; Func<int, int> addFive = b => add(5, b);` creates a new function that always adds five. The outer value `5` is captured in the closure. A more general approach is a helper method: `public static Func<T2, TResult> Partial<T1, T2, TResult>(Func<T1, T2, TResult> f, T1 arg1) => arg2 => f(arg1, arg2);`. Partial application is useful for creating specialized functions from general ones — building a specific log formatter from a general template, or creating a currency converter for a fixed base currency. It reduces repetition at call sites without losing the general function's reusability.

---

## Q10. How does `Action<T>` differ from `EventHandler<TEventArgs>` in the event pattern?

**Concepts**
- `Action<T>` vs `EventHandler<T>` signature
- `sender` parameter convention
- `EventArgs` derivation requirement
- type safety comparison
- modern event pattern trends

**Answer**

`EventHandler<TEventArgs>` follows the classic .NET event pattern: it always includes a `sender` parameter typed as `object` and requires `TEventArgs` to derive from `EventArgs`. This convention was established for interoperability and extensibility across the .NET Framework. `Action<T>` is a simpler, more direct delegate — it accepts one typed argument with no sender and no `EventArgs` constraint. Modern C# code increasingly uses `Action<T>` or custom `Func` delegates for events when the sender and EventArgs hierarchy are unnecessary overhead. The tradeoff is that `EventHandler<T>` is the established convention that WinForms, WPF, and other framework designers expect, making it important for public library APIs that integrate with those frameworks. For internal events, MVVM commands, and reactive pipelines, `Action<T>` or even `IObservable<T>` are often cleaner choices.

---

## Q11. Can `Func` or `Action` be used with `ref` or `out` parameters?

**Concepts**
- `Func`/`Action` signature limitations
- no `ref`/`out` parameter support
- custom delegate as workaround
- `ref` return on `Func` alternative
- C# `delegate*` function pointers

**Answer**

No — the generic `Func` and `Action` delegate definitions in the BCL do not include overloads with `ref` or `out` parameters. This is a known limitation. If you need a callback that uses `ref` or `out`, you must declare a custom delegate type: `delegate bool TryParse<T>(string input, out T result);`. In .NET 10, C# also offers managed function pointers (`delegate*`) as a lower-level, unsafe alternative that supports `ref`/`out` and has lower overhead, but these are niche and require the `unsafe` context. The practical impact is that methods like `int.TryParse`, `Dictionary<K,V>.TryGetValue`, and similar try-pattern APIs cannot be stored as `Func` without wrapping. A common workaround is a lambda wrapper that hides the `out` parameter: `Func<string, (bool ok, int val)> tryParseInt = s => (int.TryParse(s, out var v), v);`.

---

## Q12. How do you use `Func` to implement a retry policy?

**Concepts**
- `Func<Task<T>>` as retriable operation
- exception handling in retry loop
- exponential backoff
- cancellation token integration
- policy pattern abstraction

**Answer**

A retry policy wraps any `Func<Task<T>>` and re-invokes it on transient failures, providing resilience without changing the operation's code. The factory must be a `Func<Task<T>>` rather than a `Task<T>` because a `Task` is already running — you need the ability to call it multiple times, which requires a factory. A minimal retry helper looks like: calling `await operation()` in a loop, catching `Exception`, waiting with exponential backoff (e.g., `await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)))`), and re-throwing on the final attempt. Integrating a `CancellationToken` prevents indefinite retries when the calling context is shutting down. Libraries like Polly abstract this pattern further, accepting `Func<CancellationToken, Task<T>>` and providing rich retry, circuit-breaker, and bulkhead policies. The key design insight is that the `Func` indirection makes the policy completely decoupled from the specific operation being retried.

---

## Gotchas

---

## Q13. Why does `Func<T, bool>` and `Predicate<T>` have the same signature but are not assignable to each other?

**Concepts**
- delegate type structural vs nominal typing
- CLR nominal type identity
- implicit conversion absence
- wrapping workaround
- compiler overload resolution impact

**Answer**

C# uses nominal typing for delegates: two delegate types are identical only if they refer to the same type declaration, even if their parameter and return types are identical. `Func<T, bool>` is declared in `System`, while `Predicate<T>` is a separate declaration also in `System`. Despite having the same signature, they are different types and there is no implicit conversion between them. The compiler does not treat them as interchangeable. This matters most when calling older APIs that take `Predicate<T>` with a lambda you have already stored as `Func<T, bool>`: you must wrap it — `list.FindAll(x => myFunc(x))` or `list.FindAll(new Predicate<T>(myFunc))`. This is a historical artifact of the BCL's evolution rather than a deliberate design choice, and it is a frequent interview gotcha because developers intuitively expect structurally identical delegates to be assignable.

---

## Q14. What happens when you invoke a multicast `Func<T>` with multiple registered delegates?

**Concepts**
- multicast delegate invocation list
- last return value rule
- intermediate values discarded
- `GetInvocationList()` for all results
- design anti-pattern

**Answer**

When a `Func<TResult>` has multiple delegates in its invocation list (added via `+=`), invoking it runs every delegate in order and returns only the value from the last one. All intermediate return values are silently discarded. This behavior is rarely what developers intend with `Func` — multicast makes sense for `Action` (fire all subscribers) but not for `Func` (which value do you want?). A common mistake is building a plugin system where each plugin returns a transformation result, expecting to collect all results, only to discover only the last one is captured. The correct approach for collecting multiple results is to use `GetInvocationList()`, iterate each `Delegate`, cast to the specific `Func` type, invoke each one individually, and accumulate results in a list. For most scenarios involving multiple results, a `List<Func<T, TResult>>` with an explicit `foreach` loop is clearer and less error-prone than multicast.

---

## Q15. Can a null `Func` or `Action` be invoked, and how do you guard against it?

**Concepts**
- null delegate exception
- `?.Invoke()` null-conditional
- default parameter value
- defensive API design
- no-op `Action` pattern

**Answer**

Invoking a null `Func` or `Action` throws a `NullReferenceException`. The null-conditional invocation syntax `callback?.Invoke(args)` is the idiomatic guard — it checks for null before calling and short-circuits with the default value (null for reference types, default for value types) when the delegate is null. For `Action` fields that are optional callbacks, a common defensive pattern is to initialize with a no-op: `Action<string> onComplete = _ => {};`. This eliminates null checks at every call site since the delegate is always non-null and invoking it is safe. For `Func` fields where a meaningful default result is possible, use a similar initialization: `Func<int> getCount = () => 0;`. This approach is especially valuable in classes with many optional callbacks, where null-checking every call site produces noisy, repetitive code.

---

## Q16. Why are `async void` lambdas assigned to `Action` parameters dangerous?

**Concepts**
- `Action` return type is `void`
- async void exception propagation
- unobserved exceptions
- caller cannot await
- fire-and-forget semantics

**Answer**

When you write `Action callback = async () => { await SomethingAsync(); };`, the lambda compiles as an `async void` method. The `Action` type's signature is `void`, so the compiler generates an async state machine whose `Task` is not exposed. The caller invokes `callback()` and gets back immediately — there is no way to await the async work or observe exceptions. If the async body throws after the first `await`, the exception propagates to the `SynchronizationContext` (which can crash a UI app) or becomes an unobserved task exception (which may be swallowed). The fix is to change the parameter type to `Func<Task>` wherever async callbacks are expected, making the async nature explicit in the API contract. Using `Action` for async callbacks is a latent bug that often does not surface during development but causes crashes or silent failures in production.

---

## Real-World Scenarios

---

## Q17. You are building a data processing pipeline where each step transforms a record. Design a composable pipeline using `Func<T, T>`.

**Concepts**
- pipeline composition pattern
- `IEnumerable<Func<T,T>>` step collection
- `Aggregate` for assembly
- step registration and ordering
- pipeline immutability

**Answer**

Define each processing step as a `Func<Record, Record>`, store them in a list, and compose the pipeline with `Aggregate`: `Func<Record, Record> pipeline = steps.Aggregate((current, next) => r => next(current(r)));`. Callers register steps at startup: `steps.Add(NormalizeNames); steps.Add(EnrichWithMetadata); steps.Add(FilterSensitiveFields);`. The assembled `pipeline` is a single function — invoking it runs all steps left-to-right on the input record. To handle errors, wrap each step in a try-catch inside the composition or use a `Result<T>` monad to propagate failures without exceptions. The pipeline can be rebuilt when configuration changes, since each `Aggregate` call is cheap. This design is easily testable: each `Func<Record, Record>` step can be unit tested in isolation, and the full pipeline can be integration tested with a known input and expected output. The pattern scales to parallel steps when steps are independent — partition the list and execute branches concurrently.

---

## Q18. Review the following code and identify problems:

```csharp
public class OrderProcessor
{
    private Func<Order, bool> _validator;
    private Action<Order> _onComplete;

    public OrderProcessor(Func<Order, bool> validator)
    {
        _validator = validator;
    }

    public void Process(Order order)
    {
        if (_validator(order))
        {
            SaveToDatabase(order);
            _onComplete(order);
        }
    }

    public void SetCompletionCallback(Action<Order> callback)
    {
        _onComplete = callback;
    }
}
```

**Issues**

| Category | Problem | Impact |
|----------|---------|--------|
| Null reference | `_onComplete` is never initialized; `_onComplete(order)` throws `NullReferenceException` if `SetCompletionCallback` not called | Runtime crash on every successful order if callback not set |
| Design | Mutable callback set via separate method is error-prone; constructor injection preferred | Race condition possible if callback set concurrently |
| Null propagation | `_validator` could be null if null is passed to constructor | NullReferenceException in `Process` |
| Async gap | `SaveToDatabase` is likely async; synchronous wrapper risks deadlock or missed errors | Unobserved exceptions, performance degradation |

**Fix priority**
1. Initialize `_onComplete` to a no-op in the field declaration: `private Action<Order> _onComplete = _ => {};` or use `?.Invoke`.
2. Accept both validator and callback in the constructor and validate for null with `ArgumentNullException.ThrowIfNull`.
3. Make `Process` return `Task` and await `SaveToDatabaseAsync`.
4. Remove `SetCompletionCallback` or replace it with an immutable constructor parameter.

---

## Q19. A team is replacing a large `switch` statement with a `Func`-based dispatch table. What are the design considerations and risks?

**Concepts**
- dispatch table registration
- default handler for unknown keys
- registration order independence
- reflection-based auto-registration option
- testing each handler in isolation

**Answer**

The primary benefit of the dispatch table is extensibility — new commands are added by registering entries rather than modifying the switch. Define the table as `Dictionary<CommandType, Func<Command, Task<Result>>>` and populate it at startup (constructor, DI registration, or a configuration phase). Always handle missing keys explicitly: use `TryGetValue` and return a structured error result rather than letting a `KeyNotFoundException` propagate. The registration order is irrelevant for correctness (dictionary lookup is O(1) by key) but matters for team conventions — document a single registration location to avoid scattered registrations that are hard to audit. For large command sets, consider reflection-based auto-registration: scan for classes implementing a marker interface and register them automatically, reducing per-command boilerplate. The main risks are that the dispatch table is harder to step through in a debugger (no IDE navigation from call site to handler), and that it may hide all available commands from static analysis tools that check switch exhaustiveness. Mitigate by keeping the registration code co-located and writing integration tests that verify every expected key is registered.
