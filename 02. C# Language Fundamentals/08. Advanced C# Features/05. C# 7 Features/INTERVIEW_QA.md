# C# 7 Features — Interview Q&A


## Table of Contents

1. [Q1. What are C# 7 tuples and how do named tuples improve readability?](#q1-what-are-c-7-tuples-and-how-do-named-tuples-improve-readability)
2. [Q2. What is C# 7 pattern matching with is and when guards in switch?](#q2-what-is-c-7-pattern-matching-with-is-and-when-guards-in-switch)
3. [Q3. What are local functions and what advantages do they have over lambdas?](#q3-what-are-local-functions-and-what-advantages-do-they-have-over-lambdas)
4. [Q4. What are out variables in C# 7 and how did they simplify TryParse patterns?](#q4-what-are-out-variables-in-c-7-and-how-did-they-simplify-tryparse-patterns)
5. [Q5. What are ref returns and ref locals, and what are their constraints?](#q5-what-are-ref-returns-and-ref-locals-and-what-are-their-constraints)
6. [Q6. What are throw expressions in C# 7 and how do they enable cleaner guard clauses?](#q6-what-are-throw-expressions-in-c-7-and-how-do-they-enable-cleaner-guard-clauses)
7. [Q7. What are binary literals and digit separators, and when are they useful?](#q7-what-are-binary-literals-and-digit-separators-and-when-are-they-useful)
8. [Q8. What is ValueTask and how does it differ from Task for performance-sensitive methods?](#q8-what-is-valuetask-and-how-does-it-differ-from-task-for-performance-sensitive-methods)
9. [Q9. What is tuple deconstruction and how does it work with out parameters?](#q9-what-is-tuple-deconstruction-and-how-does-it-work-with-out-parameters)
10. [Q10. What is the async Main method and why does it matter?](#q10-what-is-the-async-main-method-and-why-does-it-matter)
11. [Q11. Why does case ordering in C# 7 switch-when produce unreachable arms silently?](#q11-why-does-case-ordering-in-c-7-switch-when-produce-unreachable-arms-silently)
12. [Q12. Why can a ValueTask not be awaited twice and what breaks when it is?](#q12-why-can-a-valuetask-not-be-awaited-twice-and-what-breaks-when-it-is)
13. [Q13. Why do local functions that capture outer variables cause unexpected concurrency behavior when parallelized?](#q13-why-do-local-functions-that-capture-outer-variables-cause-unexpected-concurrency-behavior-when-parallelized)
14. [Q14. What are the ref return constraints and why do ref locals break inside async methods?](#q14-what-are-the-ref-return-constraints-and-why-do-ref-locals-break-inside-async-methods)
15. [Q15. (Code Review) VIP orders are routed to the standard express lane. Review the switch.](#q15-code-review-vip-orders-are-routed-to-the-standard-express-lane-review-the-switch)
16. [Q16. A batch job uses a local function with captured state and concurrency. Review the race conditions.](#q16-a-batch-job-uses-a-local-function-with-captured-state-and-concurrency-review-the-race-conditions)
17. [Q17. (Design) When should a method return a named tuple versus a dedicated result class?](#q17-design-when-should-a-method-return-a-named-tuple-versus-a-dedicated-result-class)

---
> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 05. C# 7 Features  
> **Stack:** .NET 10 · C# 7.0–7.3

---

## Foundation Questions

---

## Q1. What are C# 7 tuples and how do named tuples improve readability?

**Concepts**
- ValueTuple vs System.Tuple (heap-allocated)
- named element syntax (FieldName: value)
- tuple deconstruction
- tuple equality and comparison
- public API vs internal use trade-off

**Answer**

C# 7 introduced lightweight value tuple literals using `ValueTuple<T1, T2, …>` as the backing struct. A tuple is expressed as `(bool CanFulfill, string Note)` where the names are compiler-generated field names stored in attributes — they carry no runtime overhead but provide IntelliSense and readable syntax. Named tuples significantly improve readability over numbered access (`item1`, `item2`): `result.CanFulfill` is self-documenting while `result.Item1` requires the caller to know the meaning of each position. Tuples support deconstruction: `var (canFulfill, note) = CheckFulfillment(order)`. They are value types, so assigning a tuple variable copies the fields — there is no aliasing. The appropriate use case for tuples is returning two or three closely related values from a private or internal method where defining a named class or record would be disproportionate overhead. For public API surfaces, named result classes or records are preferable because they have stable member names, support documentation comments, and are resilient to position changes that would silently shift tuple values.

---

## Q2. What is C# 7 pattern matching with is and when guards in switch?

**Concepts**
- is type pattern (Type t)
- switch case Type t when condition
- case ordering matters (most specific first)
- pattern matching eliminating explicit casts
- switch expression (C# 8) as evolution

**Answer**

C# 7 extended `is` from a pure type check to a type pattern that simultaneously checks and binds: `if (obj is Order o)` tests whether `obj` is an `Order` and, if so, binds it to `o` within the `if` block — no separate cast needed. Switch statements gained `case Type t:` patterns and `when` guards: `case OrderPriority.Express when order.IsHighValue: return "EXPRESS-VIP"`. The critical rule is that cases are evaluated top to bottom, and a more general case earlier in the switch arms shadows any more specific case that follows. The `case OrderPriority.Express:` arm before `case OrderPriority.Express when order.IsHighValue:` always matches first for Express orders, so the VIP guard is unreachable. Compilers emit a warning for some unreachable cases, but not all orderings trigger the warning. The fix is always to put the most specific (most constrained) `when` cases before the general case of the same type. In C# 8, switch expressions replaced this syntax with a more concise, exhaustiveness-checked form.

---

## Q3. What are local functions and what advantages do they have over lambdas?

**Concepts**
- nested method inside another method
- access to outer scope variables (closure)
- static local function (C# 8) no capture
- named vs anonymous
- iterator and async local functions

**Answer**

A local function is a named method declared inside another method, constructor, or property accessor. Like a lambda it can capture outer variables by closure, but unlike a lambda it is not a delegate — it compiles to a private method on the enclosing type (or a hidden struct for non-static captures). The advantages over lambdas are: local functions can be iterators (`yield return`) or `async` — lambdas assigned to `Func<T>` cannot be iterator methods; local functions can have XML documentation; the name appears in stack traces rather than an anonymous lambda signature, which improves debuggability; and for non-capturing local functions the compiler avoids a delegate allocation. The primary use case is for a helper method that belongs semantically inside a single caller method but would clutter the class's member list if declared at type scope — for example, a retry loop body, a recursive helper, or a validation sub-step.

---

## Q4. What are out variables in C# 7 and how did they simplify TryParse patterns?

**Concepts**
- inline out variable declaration
- scope of out variable (containing block)
- discard _ for unused out
- bool success with out together
- pre-C# 7 ceremony comparison

**Answer**

Before C# 7, using a `TryParse` method required pre-declaring the out variable: `int result; if (int.TryParse(s, out result))`. C# 7 allows the variable to be declared inline at the call site: `if (int.TryParse(s, out int result))`. The variable `result` is in scope for the entire enclosing block after the if statement — not just the then branch — which is important when you need the parsed value in a subsequent else branch. The discard `_` can replace the out variable when the caller does not need the value: `if (int.TryParse(s, out _))` checks parseability without allocating a variable. The key gotcha is that a failed `TryParse` leaves the out variable at `default(int)` — zero — and code that calls `TryParse` and then uses the out variable without checking the bool return value silently uses zero for unparseable strings. Always check the return bool before using the out variable.

---

## Q5. What are ref returns and ref locals, and what are their constraints?

**Concepts**
- ref return provides alias into an array or field
- ref local variable holds an alias
- async method cannot store ref locals across await
- cannot return ref to a local variable
- use case: avoiding struct copies in performance code

**Answer**

`ref` returns allow a method to return a reference to a storage location (array element, field, or property) rather than a copy of its value. The caller receives a `ref` alias: `ref int slot = ref FindSlot(inventory, index); slot -= quantity;` modifies the array element in place without copying. The constraints are strict: a method cannot return a `ref` to a local variable because the local's lifetime ends when the method returns; it can only return a `ref` to a field, array element, or another `ref` parameter. `async` methods cannot use `ref` locals that are live across an `await` because the compiler transforms async methods into state machines where local variables must be storable in fields — and `ref` fields to managed memory are not allowed in this context. The practical use case is performance-sensitive code that mutates elements of large arrays or structs in place, avoiding the allocation and copy that a return-by-value approach would incur.

---

## Q6. What are throw expressions in C# 7 and how do they enable cleaner guard clauses?

**Concepts**
- throw in expression context (ternary, null-coalescing, expression body)
- ArgumentNullException and ArgumentOutOfRangeException patterns
- readability vs exception detail trade-off
- nameof for parameter names
- expression-bodied members

**Answer**

Before C# 7, `throw` could only appear as a statement. C# 7 made `throw` an expression, allowing it in the `??` operator: `order ?? throw new ArgumentNullException(nameof(order))`, in ternary: `qty > 0 ? qty : throw new ArgumentOutOfRangeException(nameof(qty))`, and in expression-bodied members. This enables concise one-line guard clauses in methods and properties. The main trade-off versus classic block-style guards is that throw expressions in ternary chains cannot include extra context in the exception message — `throw new ArgumentOutOfRangeException(nameof(qty), qty, "Quantity must be positive")` requires the message overload, which is available in block style but makes the expression-body form longer. For shared domain libraries where exception messages aid operator debugging, the block style with explicit message overloads provides better diagnostic value. For internal helpers where brevity matters, throw expressions are idiomatic. In .NET 10, `ArgumentNullException.ThrowIfNull(order)` and `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(qty)` are the preferred patterns for common guard cases — they are shorter than either form and produce standardized messages.

---

## Q7. What are binary literals and digit separators, and when are they useful?

**Concepts**
- 0b prefix for binary literals
- _ separator for any numeric literal
- no runtime cost (compile-time constant)
- readability in bit manipulation and constants
- long and uint type suffixes

**Answer**

C# 7 added binary integer literals with the `0b` prefix — `0b1010_1100` — and the digit separator `_` which can appear anywhere inside a numeric literal without affecting its value: `1_000_000`, `0xFF_EA_D0`, `3.141_592_653`. These are purely compile-time syntactic features; the emitted IL is identical to the unadorned literal. Binary literals are useful when defining bitmask flags, GPIO pin configurations, protocol byte values, or compression bit patterns where the individual bit positions are meaningful. Digit separators improve readability of large numbers, currency values, and IPv4 address component literals. The separator can appear between any digits including hex and binary: `0xDEAD_BEEF`, `0b1111_0000_1111_0000`. Underscores cannot appear at the start or end of a literal or adjacent to the `0b` or `0x` prefix.

---

## Q8. What is ValueTask and how does it differ from Task for performance-sensitive methods?

**Concepts**
- ValueTask avoids Task heap allocation for synchronous-fast paths
- completed ValueTask wraps a value directly (no heap)
- ValueTask awaited only once rule
- IValueTaskSource for advanced pooling
- when to prefer Task over ValueTask

**Answer**

`ValueTask<T>` is a value type that wraps either a completed result or a `Task<T>` for the async case. When a method commonly returns synchronously — such as a cache hit that returns immediately without ever going async — returning `ValueTask<T>` avoids allocating a `Task<T>` heap object for the common case: `return new ValueTask<T>(cachedValue)` stores the value directly in the struct with no heap allocation. When the method must go async (cache miss), it returns `new ValueTask<T>(actualTask)` which wraps the underlying `Task`. The critical constraint is that a `ValueTask<T>` must be awaited at most once. Awaiting the same `ValueTask<T>` twice — for example, storing it in a variable and awaiting it in a retry path — is undefined behavior and can throw `InvalidOperationException`. For retry patterns, convert to `Task<T>` with `.AsTask()` first, which creates a proper heap-allocated `Task` that can be awaited multiple times. `ValueTask` is appropriate for cache-first methods and high-throughput API implementations; for most application-level async methods, `Task<T>` is simpler and safer.

---

## Foundation Questions (continued)

---

## Q9. What is tuple deconstruction and how does it work with out parameters?

**Concepts**
- Deconstruct instance method pattern
- var (a, b) = tuple syntax
- user-defined Deconstruct extension method
- position-based binding
- discards in deconstruction

**Answer**

Tuple deconstruction uses `var (a, b) = expr` syntax to assign each element of a tuple to a separate variable in one statement. The compiler looks for a `Deconstruct` method — either an instance method on the type or an extension method — with `out` parameters for each component. For `ValueTuple` types the compiler handles deconstruction natively without a `Deconstruct` method. User-defined types can participate in deconstruction by defining `public void Deconstruct(out T1 a, out T2 b)`. Deconstruction can use discards for elements the caller does not need: `var (_, note) = CheckFulfillment(order)` captures only the second element. Deconstruction also works in `foreach` over collections of tuples: `foreach (var (key, value) in dictionary)`. In C# 10, deconstruction is extended to work with property patterns in positional pattern matching.

---

## Q10. What is the async Main method and why does it matter?

**Concepts**
- Task-returning Main entry point (.NET 7+ apps)
- await in top-level code
- synchronous wait anti-pattern (.Result / .GetAwaiter().GetResult())
- console app host builder pattern
- Windows Forms / WPF still need SynchronizationContext awareness

**Answer**

C# 7.1 introduced `async Task Main(string[] args)` as a valid program entry point, allowing `await` expressions directly in `Main`. Before this, console applications that needed async APIs had to use `GetAwaiter().GetResult()` or `.Result`, which blocks the calling thread and can cause deadlocks on platforms with a synchronization context (ASP.NET, Windows Forms) where the awaited continuation needs to resume on the same thread. With `async Task Main`, the entry point is properly awaitable and the runtime handles the async-to-synchronous bridge at the framework level. In .NET 6+ top-level statements (`await MyMethodAsync()` at the top of `Program.cs` without a `Main` method) are even more concise and are compiled to an `async Task Main` under the hood. The practical benefit is that database migrations, HttpClient calls, file operations, and service bootstrapping in console utilities can all use natural async/await without the synchronization risks of `.Result`.

---

## Gotchas

---

## Q11. Why does case ordering in C# 7 switch-when produce unreachable arms silently?

**Concepts**
- first-matching case wins
- general case before specific when guard
- compiler warning not always emitted
- unit tests on isolated values not covering ordering
- fix: most specific case first

**Answer**

In a C# 7 switch statement, cases are evaluated sequentially from top to bottom. When `case OrderPriority.Express:` appears before `case OrderPriority.Express when order.IsHighValue:`, every Express order matches the first arm and control never reaches the second — the VIP arm is dead code. The C# compiler emits `CS8120` ("The switch case is unreachable") for some unreachable patterns, but the warning is not always generated for `when` guards on the same constant value depending on the compiler version. Unit tests that test `IsHighValue` orders in isolation pass because the test directly invokes the routing method with that one order — but in production, routing processes batches where the first matching arm handles all Express orders before the guarded arm is considered. The fix is to order cases from most constrained to least constrained: `when` guarded cases must precede the bare case for the same value.

---

## Q12. Why can a ValueTask not be awaited twice and what breaks when it is?

**Concepts**
- ValueTask state machine internal reference counting
- second await on completed ValueTask: undefined behavior
- InvalidOperationException in debug builds
- .AsTask() for multi-await scenarios
- cache retry pattern pitfall

**Answer**

`ValueTask<T>` is designed for the single-await, single-continuation use pattern. When a `ValueTask` wraps an `IValueTaskSource` (used in pooled async state machine scenarios), the source tracks whether it has been consumed. A second `await` on the same `ValueTask` may observe a stale completed state, throw `InvalidOperationException`, or silently return incorrect data — the behavior is undefined by specification. In a retry pattern like `var nameTask = GetProductNameAsync(id); order.Name = await nameTask; if (string.IsNullOrEmpty(order.Name)) order.Name = await nameTask;`, the second `await nameTask` hits this undefined behavior. The correct fix is `var nameTask = GetProductNameAsync(id).AsTask();` which materializes a proper `Task<T>` heap object that can be safely awaited multiple times. Alternatively, restructure the code to call the method again for the retry rather than re-awaiting the same task handle.

---

## Q13. Why do local functions that capture outer variables cause unexpected concurrency behavior when parallelized?

**Concepts**
- closure captures variable by reference not value
- shared captured variable across Parallel.ForEach threads
- ref local to array element not thread-safe
- race condition on captured counter
- lock or Interlocked required for shared mutable state

**Answer**

A local function that captures an outer variable shares that variable by reference with every execution of the function, including parallel executions. In a `Parallel.ForEach` that calls a local function `TryReserve`, all parallel invocations share the same captured `reservationFailures` field and the same `ref int slot` aliases into the inventory array. Concurrent increments to `reservationFailures` are a data race producing an incorrect count. Concurrent `slot -= order.Quantity` operations on the same array element are also a race — two threads can both read `slot = 5`, both check `5 >= order.Quantity`, and both subtract, reducing the slot to a negative value. The fixes are: use `Interlocked.Increment(ref reservationFailures)` for the counter; use `Interlocked.Add` or a lock around the slot read-modify-write for inventory updates; or, better, replace `Parallel.ForEach` with sequential processing for operations that require consistent multi-step reads and writes to shared mutable state.

---

## Q14. What are the ref return constraints and why do ref locals break inside async methods?

**Concepts**
- async state machine heap lifts locals into fields
- ref fields to managed memory not allowed in async state machine struct
- ref local lifetime limited to declaring scope
- ref return cannot return ref to local variable
- span-based patterns as alternative

**Answer**

The C# compiler transforms `async` methods into state machine structs that lift all local variables into heap-allocated fields so they survive across `await` suspension points. A `ref local` is fundamentally incompatible with this transformation because a `ref` is a managed pointer to a specific memory location — it cannot be stored in a field (heap) since the referenced location may not outlive the heap object. Therefore, storing a `ref local` in a state machine field would create a dangling reference. The compiler rejects `ref` locals in async methods that cross an `await` with a compile error. The workaround for async inventory reservation is to read the index, perform the reservation using the index (not a ref alias), and re-read the value after the async operation, or to use a non-async helper method for the synchronous mutation step and only await the persistence call afterward. For high-performance patterns, `Span<T>` slices from `MemoryMarshal` can be used in synchronous helper methods.

---

## Real-World Scenarios

---

## Q15. (Code Review) VIP orders are routed to the standard express lane. Review the switch.

```csharp
public string RouteOrder(Order order)
{
    switch (order.Priority)
    {
        case OrderPriority.Express:
            return "EXPRESS-STANDARD";
        case OrderPriority.Express when order.IsHighValue:
            return "EXPRESS-VIP";
        case OrderPriority.Critical:
            return "CRITICAL-LANE";
        case OrderPriority.Standard when order.Quantity > 100:
            return "BULK-STANDARD";
        default:
            return "STANDARD";
    }
}
```

**Concepts**
- case ordering: general before specific makes specific unreachable
- when guard on same-value case must precede bare case
- compiler warning may not fire
- unit tests on isolated orders miss the ordering bug

| Category | Problem | Impact |
|---|---|---|
| Correctness | `case Express:` precedes `case Express when IsHighValue:` — the when-guard arm is unreachable | All high-value express orders go to EXPRESS-STANDARD, missing SLA |
| Testing | Unit tests on isolated IsHighValue orders pass (call method directly) but miss production routing of mixed batches | Bug survives code review |

**Fix priority:**
1. Move `case OrderPriority.Express when order.IsHighValue:` before the bare `case OrderPriority.Express:` arm.
2. Add an integration test that routes a batch containing both standard and high-value express orders and asserts the VIP lane count.
3. Consider migrating to C# 8 switch expression with explicit exhaustiveness checking and a cleaner arm order.

---

## Q16. A batch job uses a local function with captured state and concurrency. Review the race conditions.

```csharp
public void ProcessBatch(IEnumerable<Order> orders, int[] inventory)
{
    int reservationFailures = 0;

    void TryReserve(Order order, int skuIndex)
    {
        ref int slot = ref inventory[skuIndex];
        if (slot < order.Quantity)
        {
            reservationFailures++;
            return;
        }
        slot -= order.Quantity;
    }

    Parallel.ForEach(orders, order =>
    {
        int idx = _skuIndexMap[order.Sku];
        TryReserve(order, idx);
    });

    _metrics.RecordFailures(reservationFailures);
}
```

**Concepts**
- shared captured variable across parallel threads
- non-atomic increment on reservationFailures
- non-atomic read-modify-write on slot
- data race producing negative inventory
- Interlocked and lock as fixes

**Answer**

Two independent data races exist. First, `reservationFailures++` is a non-atomic read-modify-write on a shared variable; multiple threads executing concurrently see the same value, increment it, and write back the same incremented value — the final count is lower than the actual number of failures. The fix is `Interlocked.Increment(ref reservationFailures)`. Second, `slot -= order.Quantity` is a read-modify-write on an array element; two threads reserving against the same SKU can both observe `slot = 10`, both check that 10 is sufficient, and both subtract, reducing the slot to `10 - qty1 - qty2` which may be negative — inventory goes below zero silently. The fix is either a `lock` around the entire check-and-subtract for that SKU index, or using `Interlocked.Add` with a compare-and-exchange loop. The deeper design issue is that `Parallel.ForEach` is inappropriate for operations that require consistent multi-step reads and writes to shared mutable state; a channel-based or sequential reservation pipeline eliminates the concurrency complexity entirely.

---

## Q17. (Design) When should a method return a named tuple versus a dedicated result class?

**Concepts**
- tuple for private/internal methods returning 2–3 values
- record/class for public API surface
- versioning: adding a fourth value breaks positional deconstruction
- documentation and discoverability
- NuGet contract stability

**Answer**

Named tuples are idiomatic for private or internal methods where the return type is used by one or two callers within the same assembly. They reduce ceremony for small groupings: `(bool CanFulfill, string Note) CheckFulfillment(Order o)` is readable and needs no separate file. The problems emerge at scale. Adding a fourth field to a tuple returned by a public method changes the method signature and breaks any caller that uses positional deconstruction `var (ok, note) = Check(order)` — the new element silently shifts if it is inserted before position 2. Documentation comments cannot be placed on tuple element names. Public NuGet packages or inter-service contracts that expose tuple return types are particularly fragile because consuming assemblies bind positionally. The correct rule is: use tuples for private helpers and LINQ projections; use a dedicated `record FulfillmentResult(bool CanFulfill, string Note)` for any method on a public interface, any async method that may evolve, and any method whose return shape is referenced by more than one caller. Records are zero-overhead compared to classes for read-only results and add `ToString`, `Equals`, and deconstruction automatically.
