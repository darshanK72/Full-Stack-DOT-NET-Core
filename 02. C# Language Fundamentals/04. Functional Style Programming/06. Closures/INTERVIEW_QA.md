# C# Closures — Interview Q&A

## Foundation Questions

---

## Q1. What is a closure in C# and how does it work?

**Concepts**
- function + captured environment
- outer variable capture by reference
- compiler-generated display class
- heap-allocated variable storage
- lambda/anonymous method context

**Answer**

A closure is a function that carries a reference to the variables from the scope where it was defined, even after that scope would normally have ended. In C#, closures are created when a lambda expression or anonymous method references a variable from an enclosing scope. The compiler implements this by generating a hidden display class — a nested class whose fields store the captured variables — and the function body becomes a method on that class. When the enclosing method executes, the captured variables are promoted from the stack to fields on a display class instance allocated on the heap. Both the enclosing method and the closure's method operate on those same fields, so mutations are visible to both sides. The display class instance lives as long as any delegate referencing it is reachable, potentially extending the captured variable's lifetime well beyond the original method's stack frame.

---

## Q2. What does "captured by reference" mean for closure variables?

**Concepts**
- shared storage location
- mutation visibility
- snapshot misconception
- display class field aliasing
- side effects across closures

**Answer**

"Captured by reference" means the closure and the enclosing code share the same storage slot for the variable, rather than the closure receiving a copy of the variable's value at the moment of capture. Concretely, both the outer method and the closure read and write the same field on the display class instance. If the outer method mutates the variable after creating the closure, the closure will see the updated value when it runs. Conversely, if the closure mutates the variable, the outer method sees the change. This is a frequent source of bugs because developers often expect closure capture to work like a snapshot: `int x = 5; Action a = () => Console.WriteLine(x); x = 99; a();` prints `99`, not `5`. The fix is to create a local copy before capturing: `int snapshot = x; Action a = () => Console.WriteLine(snapshot);`. The snapshot variable gets its own display class field, completely independent of further changes to `x`.

---

## Q3. What is a compiler-generated display class and how does it enable closures?

**Concepts**
- synthesized nested class
- captured variable promotion
- display class per scope
- field-per-variable mapping
- decompiler visibility

**Answer**

When the compiler detects a lambda or anonymous method that captures outer variables, it synthesizes a private nested class — conventionally named something like `<>c__DisplayClass1_0` — and promotes each captured local variable to a field on that class. The enclosing method is rewritten to create an instance of this class early in its execution, store initial values into the fields, and replace all references to the captured locals with reads and writes to the fields. The lambda body becomes an instance method on the display class, giving it access to the fields through `this`. The delegate is created as an instance delegate pointing to this generated method. You can inspect this structure directly using SharpLab (sharplab.io) or ILSpy, which shows the generated class, its fields, and the rewritten method body. Understanding display classes is essential for reasoning about closure behavior, memory implications, and debugging unexpected variable sharing.

---

## Q4. How are closures affected by variable scope boundaries?

**Concepts**
- block scope vs method scope
- separate display classes per scope
- variable sharing across closures in same scope
- loop body scope isolation
- inner lambda nesting

**Answer**

The compiler creates display classes based on the scope in which variables are declared. Variables declared in the same block share the same display class, which means two lambdas defined in the same block that capture the same variable both operate on the same field. Variables declared in a nested block get a separate display class, which may hold a reference to the outer display class. In a `for` loop, the loop variable `i` is declared once at the outer scope — there is a single `i` field shared across all iterations — while a variable declared inside the loop body is in the loop's inner scope and gets a fresh allocation per iteration (though the compiler may optimize this away). This distinction is why the classic loop-capture bug is specific to the loop counter but not to variables declared inside the loop body, and why the fix of creating a local copy inside the body works: the copy is in the inner scope.

---

## Q5. What is the classic loop-capture bug and how do you fix it?

**Concepts**
- loop counter captured by reference
- shared display class field
- all delegates see final value
- local copy pattern
- C# `foreach` vs `for` behavior

**Answer**

The classic bug is creating closures inside a `for` loop that capture the loop variable: `var actions = new List<Action>(); for (int i = 0; i < 5; i++) { actions.Add(() => Console.WriteLine(i)); }`. When any action in the list is invoked, they all print `5` — the final value of `i` — not 0, 1, 2, 3, 4 as expected. This happens because all five lambdas share the same `i` field on the display class, and by the time they execute, the loop has completed and `i` is 5. The fix is to create a local copy inside the loop body: `int copy = i; actions.Add(() => Console.WriteLine(copy));`. Now each iteration has its own `copy` variable in the loop body's scope, giving each closure an independent field. It is worth noting that the C# compiler fixed a similar issue for `foreach` in C# 5: the iteration variable in `foreach` is now scoped per-iteration, so closures in `foreach` bodies capture independent copies automatically.

---

## Q6. How does capturing a variable in a closure extend its lifetime?

**Concepts**
- stack-to-heap promotion
- GC root via delegate
- extended variable lifetime
- memory implication
- `WeakReference` workaround

**Answer**

Normally, a local variable lives on the stack for the duration of the method call and is released when the method returns. When a closure captures that variable, the compiler promotes it to a field on the display class, which is a heap-allocated object. The display class instance lives as long as any delegate that references it is reachable. If a delegate is stored in a long-lived collection, a static field, or an event subscription, the display class — and therefore the captured variable — stays alive for the lifetime of that reference. This is a form of memory retention that is both the feature (data outlives the method) and a potential memory leak (large objects captured unintentionally). The diagnostic is to profile with a heap snapshotter and trace GC roots through delegate chains. The fix is to ensure delegate lifetimes are bounded — unsubscribe from events, clear collections, and avoid capturing large objects (like `HttpContext` or database connections) in closures stored beyond the request scope.

---

## Q7. Can closures cause memory leaks in event-driven applications?

**Concepts**
- event subscription lifetime
- publisher–subscriber coupling
- delegate holding display class reference
- long-lived publisher root
- Dispose pattern for unsubscription

**Answer**

Yes — closures are a common source of memory leaks in event-driven code. When you subscribe to an event with a lambda that closes over instance fields or `this`, the delegate holds a reference to the display class, which in turn holds a reference to the subscriber object. If the event publisher is longer-lived than the subscriber (e.g., a singleton service publishing to a short-lived view), the subscriber cannot be collected because the publisher's event invocation list still references it indirectly through the delegate. The subscriber is effectively "leaked" for the publisher's lifetime. The fix is to unsubscribe in a `Dispose` method: store the lambda in a field and call `-=` on disposal. Alternatively, use a weak event pattern (via `WeakReference<T>` wrappers or WPF's `WeakEventManager`) so the event subscription does not prevent collection. Design guidance: always consider the relative lifetimes of publisher and subscriber when wiring events with closures.

---

## Q8. What happens when multiple closures in the same scope share a captured variable?

**Concepts**
- shared display class field
- mutual visibility of mutations
- unintended coupling
- independent copy pattern
- multi-delegate interaction

**Answer**

When two or more lambdas in the same scope capture the same variable, they all share the same display class field representing that variable. A write made through one closure is immediately visible to all others. This aliasing is often unintentional: a developer writes two independent callbacks, each needing a counter, and is surprised to find they share state. For example, `int count = 0; Action inc = () => count++; Action dec = () => count--;` — both `inc` and `dec` operate on the same `count` field. Invoking `inc(); inc(); dec();` leaves `count` at 1, which may be intentional for shared state or accidental for independent counters. When closures must be independent, declare separate variables for each: `int countA = 0; int countB = 0; Action incA = () => countA++; Action decB = () => countB--;`. The compiler allocates separate fields for each.

---

## Q9. How do closures behave inside async methods?

**Concepts**
- async state machine generation
- captured variable in state machine struct/class
- `await` suspension and resumption
- captured `HttpContext` / `DbContext` pitfalls
- `ConfigureAwait` and captured synchronization context

**Answer**

Async methods are compiled into state machines by the compiler, and any local variables that are in use across an `await` point are promoted to fields on the generated state machine type — a mechanism similar to closure capture but applied to the entire async method. When you create a lambda inside an async method and the lambda captures a local variable, the closure's display class holds a reference to the variable (now a field on the state machine), effectively extending its lifetime until the delegate and the async operation are both done. A significant pitfall is capturing context objects like `HttpContext` or `DbContext` in closures scheduled for execution after an `await` — these objects may have been disposed or recycled by the time the closure runs (for example, after a request completes). The safe pattern is to extract the values you need from those objects before the `await` and capture only the primitive values.

---

## Q10. What are static lambdas and how do they relate to closures?

**Concepts**
- `static` lambda modifier (C# 9)
- no captured variables
- compile-time enforcement
- performance benefit (no display class)
- CS8820 compiler error

**Answer**

Introduced in C# 9, the `static` modifier on a lambda (`static x => x * 2`) tells the compiler that the lambda must not capture any variables, `this`, or instance members. If the body attempts to close over any outer variable, the compiler issues error CS8820. Static lambdas are compiled without a display class allocation — the compiler can cache the delegate instance statically, eliminating heap allocations for closures used in tight loops or frequently called hot paths. They are the lambda equivalent of a static method: fully self-contained with no external dependencies. In .NET 10, many BCL performance-sensitive paths use static lambdas in `OrderBy`, `Select`, and similar LINQ operations to avoid allocations. The practical rule is to use `static` on lambdas wherever no capturing is needed — it both documents intent and enforces it, preventing a future code change from accidentally introducing a capture.

---

## Q11. How do closures in LINQ queries interact with deferred execution?

**Concepts**
- deferred LINQ evaluation
- variable captured at query definition
- value at iteration time matters
- query re-execution behavior
- capture mutation between define and enumerate

**Answer**

LINQ methods like `Where` and `Select` use deferred execution — the lambda is not called when the query is defined, but each time the result sequence is enumerated. If the lambda closes over a variable that changes between query definition and enumeration, the query uses the value at enumeration time, not at definition time. For example, `int threshold = 5; var q = items.Where(x => x > threshold); threshold = 10;` — when you iterate `q`, it filters with `threshold = 10`. This is sometimes intentional (the query should always use the current threshold) and sometimes a bug (you expected the threshold at the time you wrote the query). Always be explicit: if you need a snapshot, copy the value before the query: `int t = threshold; var q = items.Where(x => x > t);`. Re-enumerating a LINQ query re-captures the current variable values, which means a query defined once can produce different results across multiple enumerations if captured variables change between them.

---

## Q12. How can closures lead to unexpected shared state in multi-threaded code?

**Concepts**
- shared display class field across threads
- race condition on closure variable
- no implicit synchronization
- `Interlocked` for atomic operations
- `ThreadLocal<T>` alternative

**Answer**

When multiple threads invoke delegates that close over the same variable, they all read and write the same display class field without any synchronization. If the operations are not atomic, this is a data race that can corrupt the value or produce inconsistent results. For example, `int count = 0; Action inc = () => count++;` — `count++` is not atomic (read-modify-write), and concurrent invocations from a thread pool can cause increments to be lost. The fix depends on the requirement: for simple counters, `Interlocked.Increment(ref count)` provides atomic increments. For more complex operations, use `lock(syncObject) { count++; }`. When each thread needs an independent copy, use `ThreadLocal<int> count = new ThreadLocal<int>(() => 0);` — each thread gets its own value rather than sharing one. Understanding that closures capture fields, not thread-local copies, is essential when transitioning closure-heavy code to concurrent execution.

---

## Gotchas

---

## Q13. Why do all lambdas in a `for` loop print the same final value?

**Concepts**
- loop counter in outer scope
- single display class field
- all delegates share same `i`
- iteration completion before invocation
- local copy fix

**Answer**

The `for` loop counter `i` is declared once before the loop body. The compiler creates a single display class instance with one `i` field for the entire loop, and every lambda created in the loop body captures that same field. When the loop finishes, `i` holds its final value (e.g., `5` for `i < 5`). When any lambda is subsequently invoked, it reads the current value of the shared field — which is `5` — regardless of which iteration created it. The fix is to declare a new variable inside the loop body and assign the current `i` to it: `int copy = i;`, then capture `copy` instead. Because `copy` is declared inside the loop body, the compiler gives it a fresh field per iteration (or per display class scope), making each lambda independent. After C# 5, `foreach` loops automatically create a per-iteration copy of the iteration variable, so this bug is specific to `for` and `while` loops where the variable is declared outside the body.

---

## Q14. What is the "async lambda captures large object" memory leak pattern?

**Concepts**
- async continuation capturing context
- `HttpContext` / `IServiceScope` retention
- request lifetime vs Task lifetime
- captured reference prevents collection
- scope disposal mismatch

**Answer**

In ASP.NET Core, the `HttpContext` (and objects derived from it such as the `IServiceScope` or database connections) is scoped to the request lifetime and disposed when the request completes. If you create an async lambda inside a controller or middleware that captures the `HttpContext` by closing over it, and that lambda is scheduled to run after the request ends (e.g., via `Task.Run`, a background queue, or a timer), the captured reference keeps `HttpContext` alive past its intended disposal. Using the disposed `HttpContext` causes runtime errors, and even just holding the reference prevents the GC from collecting the request's allocated objects. The fix is to extract the data you need before the async operation starts and capture only value types or simple strings: `var userId = context.User.Identity.Name; _ = Task.Run(() => LogAsync(userId));`. The captured `userId` string has no lifetime coupling to the request.

---

## Q15. Why can sharing a display class between two closures cause subtle bugs that are hard to reproduce?

**Concepts**
- shared mutable field aliasing
- non-obvious coupling between functions
- ordering-dependent behavior
- concurrency amplification
- debugging difficulty

**Answer**

When two lambdas in the same scope capture the same variable, the compiler places them both in the same display class. The bug manifests when a developer writes the closures as if they are independent but fails to realize they share state. Because the sharing is invisible at the lambda call sites and requires knowledge of the compiler's display class generation rules to diagnose, it rarely appears during code review. The bug is further obscured by the fact that it reproduces only when the closures execute in certain orderings — in a sequential single-threaded scenario, the ordering may be consistent enough that the bug only appears under load or with different call patterns. Under concurrent execution, the shared field becomes a race condition, making the bug non-deterministic. The most reliable diagnostic is to decompile the code with a tool like ILSpy or SharpLab and inspect the display class structure, which makes the shared fields immediately visible.

---

## Q16. Does `foreach` have the same loop-capture bug as `for`?

**Concepts**
- C# 5 breaking change for `foreach`
- per-iteration variable scope
- pre-C# 5 behavior
- `for` loop still affected
- explicit enumeration with index

**Answer**

Not since C# 5. Before C# 5, `foreach`'s iteration variable was treated as declared once outside the loop body, so all closures created inside a `foreach` captured the same variable and saw the final value after iteration — the same bug as `for`. C# 5 changed the specification so that the `foreach` iteration variable is conceptually re-declared fresh for each iteration, meaning each closure captures an independent copy. This was a deliberate breaking change because the old behavior was almost never intentional and was a common source of bugs. After C# 5, you still have the bug in `for` loops because the counter is explicitly declared once before the loop, and the language cannot change that semantics without breaking valid code that relies on the counter's mutation across iterations. The `foreach` fix does not extend to `for`, so the local-copy workaround is still required for `for` loops.

---

## Real-World Scenarios

---

## Q17. A production service has a memory leak. Heap profiling shows event delegates retaining subscriber objects. Describe the diagnosis and fix.

**Concepts**
- profiler heap snapshot
- GC root tracing through delegates
- event invocation list as root
- Dispose-based unsubscription
- service lifetime alignment

**Answer**

The profiling workflow starts by taking two heap snapshots with a forced GC collection between them in dotMemory, PerfView, or Visual Studio Diagnostics. Compare retained objects between snapshots: objects of the subscriber type that are increasing or never decreasing point to a retention problem. Select a retained instance and inspect its GC root chain — you will typically see a delegate holding a reference to the display class, which holds a reference to the subscriber. The delegate is held by the publisher's event invocation list. Confirm the publisher is longer-lived (e.g., a singleton) while subscribers are shorter-lived (e.g., per-request or per-scope services). The fix is to implement `IDisposable` on the subscriber, store the event handler delegate in a field, and unsubscribe in `Dispose`: `_publisher.DataArrived -= _handler;`. Ensure `Dispose` is actually called by registering the subscriber with the DI container using a scoped or transient lifetime and verifying the DI container disposes it. For cases where explicit unsubscription is impractical, introduce a weak event pattern using `WeakReference<T>` or restructure the code so the subscriber's lifetime matches the publisher's.

---

## Q18. You need a memoize function that caches the results of an expensive computation. Implement it using closures and explain the design.

**Concepts**
- closure over dictionary cache
- factory function pattern
- thread-safety considerations
- cache invalidation omission
- generic implementation

**Answer**

A memoize function wraps any `Func<TArg, TResult>` and returns a new `Func<TArg, TResult>` that caches results:

```csharp
public static Func<TArg, TResult> Memoize<TArg, TResult>(
    Func<TArg, TResult> func)
    where TArg : notnull
{
    var cache = new Dictionary<TArg, TResult>();
    return arg =>
    {
        if (!cache.TryGetValue(arg, out var result))
        {
            result = func(arg);
            cache[arg] = result;
        }
        return result;
    };
};
```

The returned lambda closes over both `cache` and `func` — the cache is the private state of this memoized function instance, and `func` is the original computation. Each call first checks the dictionary; on a miss it invokes the original function, stores the result, and returns it. Subsequent calls with the same argument return the cached result immediately. This implementation is single-threaded safe only. For concurrent use, replace `Dictionary` with `ConcurrentDictionary<TArg, TResult>` and use `GetOrAdd` to avoid double computation: `return cache.GetOrAdd(arg, func);`. Note that `GetOrAdd` does not guarantee the factory runs exactly once under high concurrency — if strict single execution is needed, use `Lazy<TResult>` per key. The closure's `cache` lives as long as the returned delegate is referenced, which is usually the lifetime of the object or method that called `Memoize`.

---

## Q19. Review the following code and identify closure-related problems:

```csharp
public List<Func<int>> CreateCounters(int count)
{
    var counters = new List<Func<int>>();
    for (int i = 0; i < count; i++)
    {
        counters.Add(() => i * i);
    }
    return counters;
}
```

**Issues**

| Category | Problem | Impact |
|----------|---------|--------|
| Loop capture | All lambdas capture the same `i` field; all return `count * count` (e.g., 25 for count=5) | Incorrect results — all counters return the same value |
| Intent mismatch | Each counter should return its own index squared, but all share the loop variable | Silent logical error, no compiler warning |
| API clarity | Return type `List<Func<int>>` does not communicate index semantics | Callers cannot tell which position each function represents |

**Fix priority**
1. Add `int copy = i;` inside the loop body and capture `copy` instead of `i`.
2. Consider using LINQ: `return Enumerable.Range(0, count).Select(i => (Func<int>)(() => i * i)).ToList();` — the `Select` lambda's `i` parameter is scoped per iteration.
3. Add an XML doc comment clarifying that each returned function returns its 0-based index squared.
