# IEnumerable & IEnumerator — Interview Q&A

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/08. IEnumerable & IEnumerator`
> **Covers:** `IEnumerable<T>`, `IEnumerator<T>`, `ICollection<T>`, `IList<T>`, iterator pattern, `yield return`/`yield break`, deferred execution, `MoveNext`/`Current`/`Reset`, custom iterator implementation, `IAsyncEnumerable<T>` (.NET 10), LINQ compatibility.

---

## Foundation Questions

---

## Q1. What is `IEnumerable<T>` and what single member does it expose?

**Concepts**
- Sequence abstraction over an ordered set of elements of type `T`
- Single method `GetEnumerator()` returning `IEnumerator<T>`
- Implemented by all BCL collections: `List<T>`, arrays, `Dictionary<TKey,TValue>`, `HashSet<T>`
- Also implemented by lazy compiler-generated iterator state machines
- Extends non-generic `IEnumerable` for backward compatibility with pre-2.0 code

**Answer**

`IEnumerable<T>` is the lowest-common-denominator contract for any sequence of `T`. It declares exactly one method: `IEnumerator<T> GetEnumerator()`. Every time you call that method you get a fresh, independent forward-only cursor positioned before the first element. The interface itself carries no state — it is merely a factory for cursors. `List<T>`, `T[]`, `Dictionary<TKey,TValue>`, `HashSet<T>`, and any type you annotate with a `yield return` iterator method all implement it. Because it is so minimal, passing `IEnumerable<T>` as a parameter or return type communicates "I only need to walk this once, forward" without committing to a concrete type. The generic form extends non-generic `IEnumerable`, so legacy code that only knows `IEnumerable` can still call `GetEnumerator()` and receive an `IEnumerator` whose `Current` property returns `object`.

```csharp
// net10.0 — any type that implements IEnumerable<T> works here
static decimal SumWeights(IEnumerable<PickLine> lines)
{
    decimal total = 0m;
    foreach (PickLine line in lines)   // only needs GetEnumerator()
        total += line.TotalWeightKg;
    return total;
}
```

---

## Q2. What is `IEnumerator<T>` and what are its members?

**Concepts**
- Forward-only read cursor over a sequence
- `MoveNext()` advances the cursor and returns `bool`
- `Current` property holds the element at the current position
- `Reset()` rewinds to before the first element (legacy; rarely used in modern code)
- Inherits `IDisposable` — `Dispose()` must release unmanaged resources
- Explicit non-generic `IEnumerator.Current` returns `object` for backward compatibility

**Answer**

`IEnumerator<T>` is the cursor half of the iterator pattern. A freshly obtained enumerator sits before position zero — `Current` is undefined and calling it before the first `MoveNext` may throw or return a default value depending on the implementation. Calling `MoveNext()` advances one step and returns `true` if a new element is available; once the sequence is exhausted it returns `false` and `Current` is again undefined. `Reset()` is a remnant from COM-era interop; most modern implementations either throw `NotSupportedException` or do nothing useful, so prefer obtaining a new enumerator via `GetEnumerator()` when you need to restart. Because `IEnumerator<T>` extends `IDisposable`, every concrete enumerator must implement `Dispose()` — for in-memory collections this is a no-op, but for enumerators wrapping file handles, database readers, or network streams it is where cleanup happens. The compiler's `foreach` desugaring always calls `Dispose()` in a `finally` block, which is why `foreach` is safer than a hand-rolled `while` loop.

```csharp
// net10.0 — manual walk with explicit Dispose via using
using IEnumerator<PickLine> cursor = pickList.GetEnumerator();
while (cursor.MoveNext())
{
    Console.WriteLine(cursor.Current.Sku);
}
// Dispose() called here even on early break or exception
```

---

## Q3. How does `foreach` desugar to `GetEnumerator` / `MoveNext` / `Current` / `Dispose`?

**Concepts**
- Compiler syntactic transformation, not a runtime feature
- `GetEnumerator()` called once at loop start to obtain the cursor
- `MoveNext()` called before each iteration body; loop exits when it returns `false`
- `Current` read inside each iteration to retrieve the element
- `try/finally` wraps the loop; `Dispose()` called in `finally` unconditionally

**Answer**

When the C# compiler sees `foreach (PickLine line in source)`, it emits roughly the following IL-level equivalent:

```csharp
// net10.0 — compiler-generated desugaring of foreach
IEnumerator<PickLine> _e = source.GetEnumerator();
try
{
    while (_e.MoveNext())
    {
        PickLine line = _e.Current;
        // loop body here
    }
}
finally
{
    _e?.Dispose();
}
```

The `try/finally` is the critical safety net. Whether the loop runs to completion, exits via `break`, or throws an exception, `Dispose()` is always called. This ensures file handles and database connections held by the enumerator are released promptly. The iteration variable (`line`) is read-only in the loop body — assigning to it is a compile-time error. Duck typing applies: the compiler actually looks for a public `GetEnumerator()` method returning any type with `MoveNext()` and `Current`, not strictly `IEnumerator<T>`. This is why `foreach` works on `Span<T>`, `ImmutableArray<T>`, and custom enumerables that avoid boxing.

---

## Q4. What is the difference between `IEnumerable<T>`, `ICollection<T>`, and `IList<T>`?

**Concepts**
- `IEnumerable<T>` — read-only forward traversal only
- `ICollection<T>` — adds `Count`, `Add`, `Remove`, `Contains`, `Clear`, `CopyTo`, `IsReadOnly`
- `IList<T>` — adds index-based access via `this[int]`, `IndexOf`, `Insert`, `RemoveAt`
- Each interface is a richer superset of the previous
- Choosing the narrowest interface as a parameter type reduces coupling

**Answer**

The three interfaces form a capability ladder. `IEnumerable<T>` is the floor: you can walk elements forward once and that is all. `ICollection<T>` extends it with mutation (`Add`, `Remove`, `Clear`) and count-without-walking (`Count`), making it suitable for parameters that need to add items or check size efficiently. `IList<T>` adds positional access — `lines[2]`, `IndexOf`, `Insert(i, item)`, `RemoveAt(i)` — which implies an ordering by index. `List<T>` implements all three; `HashSet<T>` implements only `ICollection<T>` because hashing gives no inherent index; arrays implement `IList<T>` but `IsReadOnly` is `true` and `Add`/`Remove` throw. When writing method parameters, prefer the narrowest interface that satisfies your needs: take `IEnumerable<T>` when you only iterate, `ICollection<T>` when you also need `Count` or mutation, `IList<T>` when you need random access — doing so makes the method easier to test with mocks and easier to call with any compatible type.

```csharp
// net10.0 — parameter narrowness demo
static void PrintAll(IEnumerable<PickLine> lines)    { /* forward walk only */ }
static void AddLine(ICollection<PickLine> lines)     { lines.Add(new PickLine("NEW", 1, 0.1m)); }
static PickLine ByIndex(IList<PickLine> lines, int i) { return lines[i]; }
```

---

## Q5. What does `yield return` do and what does the compiler generate?

**Concepts**
- `yield return` pauses an iterator method and delivers one element to the consumer
- Compiler transforms the method body into a private state machine class
- State machine implements `IEnumerator<T>` and `IEnumerable<T>`
- Execution resumes after the `yield return` on the next `MoveNext()` call
- Local variables and parameters are preserved across yield points as state machine fields

**Answer**

When a method returns `IEnumerable<T>` (or `IEnumerator<T>`) and contains at least one `yield return`, the C# compiler entirely rewrites it. The method body disappears; instead, the compiler synthesizes a nested private class — the state machine — that captures all local variables and parameters as fields and implements a `MoveNext()` that advances through the original code using a numeric state variable. Calling the original method does not execute any of the body at all; it merely instantiates the state machine and returns it. The body runs only when a consumer drives the iterator by calling `MoveNext()`. Execution pauses at each `yield return value` statement: `MoveNext()` returns `true`, `Current` is set to `value`, and the state integer records where to resume next. This mechanism enables lazy, composable pipelines with very little allocated memory — one state machine object plus its captured fields, regardless of how large the source sequence is.

```csharp
// net10.0 — yield return iterator
static IEnumerable<PickLine> HeavyLines(IEnumerable<PickLine> source, decimal minKg)
{
    foreach (PickLine line in source)
    {
        if (line.TotalWeightKg >= minKg)
            yield return line;   // pause; resume here on next MoveNext()
    }
}
// Calling HeavyLines(...) alone does nothing — no filter runs yet
```

---

## Q6. What is `yield break` and when should you use it?

**Concepts**
- `yield break` terminates an iterator method early
- Equivalent to `return` in a normal method but inside an iterator context
- Causes the next `MoveNext()` call to return `false` and iteration to end
- Required to conditionally stop a sequence mid-stream
- Also triggers `Dispose()` for `foreach` consumers via the `finally` path

**Answer**

`yield break` is the "stop now" statement for iterator methods. Without it, you would have no clean way to end iteration before the end of the method body — a plain `return` is a compile error inside an iterator. When the state machine encounters `yield break`, it transitions to the finished state and any subsequent `MoveNext()` call returns `false`. Consumer code using `foreach` then falls through to the `finally` block and calls `Dispose()`. Common uses include guard clauses at the start of an iterator (if the source is null or empty, `yield break` immediately), pagination boundaries (stop after delivering `pageSize` items), and sentinel-value termination (stop when a special end-of-stream marker is encountered). Because `yield break` causes the state machine to enter the done state, any `try/finally` blocks in the iterator body run their `finally` clauses just as they would for a normal method exit, which is important for cleanup of resources opened partway through the iterator.

```csharp
// net10.0 — yield break as guard and page boundary
static IEnumerable<PickLine> TakeUpToWeight(IEnumerable<PickLine> source, decimal maxKg)
{
    if (source is null) yield break;      // guard clause

    decimal running = 0m;
    foreach (PickLine line in source)
    {
        running += line.TotalWeightKg;
        if (running > maxKg) yield break; // stop when weight cap exceeded
        yield return line;
    }
}
```

---

## Q7. What is deferred execution and why does it matter for `IEnumerable<T>` and LINQ?

**Concepts**
- Deferred (lazy) execution: query logic does not run at definition time
- Work runs only when a terminal operation or `foreach` forces enumeration
- Calling a LINQ method or iterator method returns a recipe, not a result
- Multiple enumerations re-execute the recipe each time
- Materializing with `ToList()` / `ToArray()` converts a recipe into a stable snapshot

**Answer**

Deferred execution means that writing `pickList.Where(l => l.TotalWeightKg > 1m)` does not filter anything immediately — it allocates a tiny iterator object that remembers the predicate and the source reference. Filtering happens only when something drives enumeration: a `foreach`, a call to `Count()`, `Sum()`, `First()`, `ToList()`, and so on. Every time you enumerate the same `IEnumerable<T>` reference, the full pipeline re-executes from the source. For in-memory lists this is just a loop overhead, but for database queries or file readers it means a round-trip or file open on every pass. This property is a feature — it allows building composable pipelines without intermediate allocations — but it becomes a bug when you assume the reference holds a fixed snapshot. If the backing `List<T>` changes between two enumerations, the second pass sees different data. If the source queries a database, each enumeration is a separate query. The fix at service boundaries is explicit materialization: `.ToList()` forces one full enumeration and returns a `List<T>` that you own, independent of the original source.

---

## Q8. How do you implement a custom enumerable type with a hand-written `IEnumerator<T>`?

**Concepts**
- Type implements `IEnumerable<T>` and returns a new cursor from `GetEnumerator()`
- Cursor class implements `IEnumerator<T>`: `MoveNext()`, `Current`, `Reset()`, `Dispose()`
- Initial `_index = -1` convention: before the first element
- `MoveNext()` increments index and returns `true` while within bounds
- Explicit non-generic `IEnumerator.Current` implementation for legacy compatibility

**Answer**

To give a custom type `foreach` support with full control over traversal, implement `IEnumerable<T>` on the collection and `IEnumerator<T>` on a private nested cursor class. The collection's `GetEnumerator()` constructs a fresh cursor on each call — two concurrent `foreach` loops on the same collection each get their own independent cursor. The cursor holds a copy of (or reference to) the underlying storage and an `int _index` initialized to `-1`, representing the "before first" state. `MoveNext()` increments `_index` and returns `true` while less than the length. `Current` returns `_storage[_index]` — calling it when `_index` is out of range is undefined behavior; well-written code throws `InvalidOperationException`. `Reset()` resets `_index` to `-1` for legacy consumers. `Dispose()` releases any resources the cursor holds. Implementing the explicit non-generic `IEnumerator.Current` property (returning `object`) satisfies callers that only know the pre-generics `IEnumerator` interface. This pattern matches `PickBatch` + `PickBatchEnumerator` in `Program.cs`.

---

## Q9. What is `Reset()` on `IEnumerator<T>` and why is it rarely used in modern code?

**Concepts**
- `Reset()` is meant to rewind the cursor to the before-first-element state
- Part of the non-generic `IEnumerator` contract, inherited by `IEnumerator<T>`
- Compiler-generated iterators (`yield return`) throw `NotSupportedException` from `Reset()`
- LINQ does not call `Reset()` — it calls `GetEnumerator()` for each enumeration
- Modern best practice: call `GetEnumerator()` again rather than `Reset()`

**Answer**

`Reset()` dates from the COM `IEnumVARIANT` days when obtaining a new enumerator was expensive. In .NET, calling `GetEnumerator()` again is cheap — it allocates a small state machine or enumerator object — so there is almost no reason to use `Reset()`. More importantly, any iterator method that uses `yield return` will throw `NotSupportedException` from `Reset()` because the compiler-generated state machine has no rewind capability. LINQ operators never rely on `Reset()`; they create a fresh enumerator via `GetEnumerator()` for every enumeration. The practical rule: if you write a custom `IEnumerator<T>` class, implementing `Reset()` correctly is straightforward — set the index back to `-1`. If you are consuming an `IEnumerator<T>` from an unknown source, never call `Reset()` because it may throw. Always prefer disposing the old enumerator and calling `GetEnumerator()` again. `PickBatchEnumerator` in `Program.cs` implements `Reset()` to illustrate the pattern but the Section 4c comment explicitly labels it legacy.

---

## Q10. Why does `IEnumerator<T>` implement `IDisposable`, and what happens if you skip `Dispose()`?

**Concepts**
- `IEnumerator<T>` inherits from `IDisposable` unlike non-generic `IEnumerator`
- Iterator state machines use `Dispose()` to run `finally` blocks inside the iterator body
- Enumerators wrapping `StreamReader`, `DbDataReader`, or network streams leak handles without `Dispose()`
- `foreach` always calls `Dispose()` in a compiler-emitted `finally` block
- Manual loops must use `using` or explicit `try/finally` to guarantee cleanup

**Answer**

The `IDisposable` inheritance on `IEnumerator<T>` serves two purposes. First, it handles real resources: an enumerator wrapping a `StreamReader` or a `SqlDataReader` needs to close that handle when iteration ends or is abandoned. Without `Dispose()`, handles stay open until the garbage collector runs a finalizer — if one exists — which under load can exhaust file descriptors or database connections before GC runs. Second, the C# specification mandates that the compiler-generated state machine's `Dispose()` method runs any `finally` blocks inside the iterator body that have not yet executed. If you write an iterator that opens a file inside `try` and yield-returns rows, breaking out of the consumer `foreach` calls `Dispose()` on the state machine, which triggers the `finally`, which closes the file. This guarantee holds because `foreach` wraps the loop in a `try/finally` that calls `_enumerator?.Dispose()`. A hand-rolled `while (e.MoveNext())` loop without `using` loses this guarantee entirely — the `finally` inside the iterator only runs if `Dispose()` is called.

---

## Q11. What is `IAsyncEnumerable<T>` and how does `await foreach` work?

**Concepts**
- `IAsyncEnumerable<T>` — async sequence: `GetAsyncEnumerator()` returns `IAsyncEnumerator<T>`
- `IAsyncEnumerator<T>` members: `MoveNextAsync()` returning `ValueTask<bool>`, `Current`, `DisposeAsync()`
- `await foreach (T x in asyncSeq)` desugars to `await MoveNextAsync()` per element
- `yield return` inside `async` methods produces `IAsyncEnumerable<T>` state machines
- `ConfigureAwait(false)` applied via `.ConfigureAwait(false)` on the enumerable

**Answer**

`IAsyncEnumerable<T>`, introduced in C# 8 / .NET Core 3 and fully supported in .NET 10, is the async counterpart to `IEnumerable<T>`. Its single member, `GetAsyncEnumerator(CancellationToken)`, returns an `IAsyncEnumerator<T>` with `ValueTask<bool> MoveNextAsync()` and `T Current`. The `await foreach` statement desugars to `await _e.MoveNextAsync()` in a loop and calls `await _e.DisposeAsync()` in a `finally`, mirroring the synchronous pattern. You produce an async sequence the same way you produce a synchronous one — use `yield return` inside an `async` method whose return type is `IAsyncEnumerable<T>`:

```csharp
// net10.0 — async iterator streaming pick lines from a database
static async IAsyncEnumerable<PickLine> StreamPickLinesAsync(
    string ticketId,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await using SqlConnection conn = new SqlConnection(_connectionString);
    await conn.OpenAsync(ct);
    await using SqlCommand cmd = new SqlCommand(
        "SELECT Sku, Qty, WeightKg FROM PickLines WHERE TicketId = @id", conn);
    cmd.Parameters.AddWithValue("@id", ticketId);
    await using SqlDataReader reader = await cmd.ExecuteReaderAsync(ct);
    while (await reader.ReadAsync(ct))
        yield return new PickLine(
            reader.GetString(0), reader.GetInt32(1), reader.GetDecimal(2));
}

// Consumer
await foreach (PickLine line in StreamPickLinesAsync("PB-2201").ConfigureAwait(false))
    Console.WriteLine(line);
```

This allows processing database rows one at a time without loading all of them into memory, and the `CancellationToken` flows through `[EnumeratorCancellation]` so callers can cancel mid-stream.

---

## Q12. How does `IEnumerable<T>` integrate with LINQ?

**Concepts**
- LINQ extension methods defined on `IEnumerable<T>` in `System.Linq`
- Methods like `Where`, `Select`, `OrderBy`, `GroupBy` return new `IEnumerable<T>` (deferred)
- Terminal operators (`Count`, `Sum`, `First`, `ToList`, `ToArray`) force enumeration
- Pipeline chains: each operator wraps the previous in a new iterator object
- `AsEnumerable()` forces LINQ-to-Objects evaluation on otherwise-queryable types

**Answer**

`System.Linq.Enumerable` contains around 50 extension methods whose first parameter is `IEnumerable<T>`. Because every collection, array, and iterator method satisfies `IEnumerable<T>`, LINQ operators compose over all of them uniformly. Intermediate operators such as `Where`, `Select`, `Take`, `Skip`, and `OrderBy` are lazy — they return a new `IEnumerable<T>` wrapper that captures the source and the lambda, executing nothing until enumerated. Terminal operators such as `Count()`, `Sum()`, `First()`, `ToList()`, and `ToArray()` drive enumeration all the way through the chain, collecting or aggregating results. The pipeline pattern means you can write `pickList.Where(l => l.TotalWeightKg > 1m).OrderBy(l => l.Sku).Select(l => l.Sku)` and the actual filtering, sorting, and projection happen in a single pass when a terminal operator fires. LINQ-to-Objects works entirely over `IEnumerable<T>`, while LINQ providers (EF Core, for example) swap `IQueryable<T>`, which extends `IEnumerable<T>` but translates the expression tree to SQL before any .NET enumeration occurs.

---

## Q13. What does calling `GetEnumerator()` multiple times on the same `IEnumerable<T>` mean, and how does it differ for a list vs a lazy sequence?

**Concepts**
- Each `GetEnumerator()` call produces an independent cursor
- For `List<T>` or arrays the cursor walks already-computed in-memory data
- For `yield return` iterators the cursor re-executes the method body from scratch
- For `IQueryable<T>` a new cursor issues a new database query
- The `IEnumerable<T>` contract does not guarantee idempotent or cheap re-enumeration

**Answer**

The `IEnumerable<T>` interface makes no promise about what `GetEnumerator()` costs. For a `List<T>` or array, each call returns a lightweight struct cursor pointing at index `-1`; re-enumeration just walks the same in-memory array again — cheap. For a `yield return` iterator, each call re-instantiates the state machine, re-executes the method body from the start, and re-runs any I/O, filtering, or transformation inside it — expensive and possibly non-idempotent. For an `IQueryable<T>` backed by EF Core, each `GetEnumerator()` call (or each `foreach`) issues a new SQL query to the database. This means writing `source.Count()` followed by `foreach (var x in source)` enumerates twice: two database round-trips, two filter passes, two sets of side effects. The discipline in production code is to treat `IEnumerable<T>` returned from any I/O boundary as a one-shot recipe and materialize it once — `var list = source.ToList()` — before using it more than once. For purely in-memory sources, multiple enumerations are benign but still worth making explicit for readability.

---

## Q14. What is the duck-typing rule for `foreach` and when does it matter?

**Concepts**
- Compiler looks for a public `GetEnumerator()` method — not necessarily `IEnumerable<T>`
- Returned type needs public `bool MoveNext()` and `T Current` property
- Allows `foreach` on value types without boxing (e.g., `List<T>.Enumerator`, `Span<T>.Enumerator`)
- No interface constraint required — compile-time duck typing
- Enables high-performance iteration on structs that would box if cast to `IEnumerator<T>`

**Answer**

The C# specification defines `foreach` by pattern, not by interface. The compiler searches for a public instance method named `GetEnumerator()` on the collection type (or an applicable extension method in C# 9+). The returned type must have a public `bool MoveNext()` method and a public `Current` property. The compiler uses those members directly without an interface cast. This matters for performance because `List<T>` returns a mutable struct `List<T>.Enumerator` instead of an interface reference. When `foreach` is used on a `List<T>` variable declared as `List<T>` (not as `IEnumerable<T>`), the compiler calls the concrete struct methods — zero virtual dispatch, zero boxing. If you upcast to `IEnumerable<T>` first, the compiler calls the interface method `GetEnumerator()`, which boxes the struct into an `IEnumerator<T>` heap object. `Span<T>` takes this further: it implements `GetEnumerator()` returning a ref struct `Span<T>.Enumerator` that cannot implement any interface and yet supports `foreach` entirely through the pattern. The practical rule: declare collection variables with their concrete type when in hot paths; use interface types for APIs where flexibility matters more than micro-allocation.

---

## Q15. What is the difference between returning `IEnumerable<T>` and `IReadOnlyList<T>` from a method?

**Concepts**
- `IEnumerable<T>` — forward-only, possibly lazy, re-enumeration may be expensive
- `IReadOnlyList<T>` — random access by index, `Count` property, guaranteed materialized snapshot
- `IReadOnlyList<T>` extends `IReadOnlyCollection<T>` which extends `IEnumerable<T>`
- Returning `IReadOnlyList<T>` signals to callers that re-enumeration is safe and cheap
- Callers cannot mutate the list through `IReadOnlyList<T>` — read-only contract

**Answer**

Choosing between `IEnumerable<T>` and `IReadOnlyList<T>` as a return type is a contract decision, not just an API style choice. Returning `IEnumerable<T>` tells the caller: you may iterate this once, forward; re-enumeration may be costly or have side effects; do not assume `Count` is available. Returning `IReadOnlyList<T>` tells the caller: this sequence is already materialized, `Count` is O(1), you can index by position, and iterating multiple times is cheap. It also signals that the collection is frozen from the caller's perspective — they cannot cast to `List<T>` and mutate it without unsafe patterns. In service layer design, methods that perform database queries should return `IReadOnlyList<T>` or `IReadOnlyCollection<T>` rather than `IEnumerable<T>` to prevent accidental double enumeration by callers. Methods that build transformation pipelines without materializing are better expressed returning `IEnumerable<T>` to keep the lazy composition benefit. The rule of thumb: if you have already called `ToList()` inside the method, return `IReadOnlyList<T>` to signal that to callers; if you are returning a raw lazy sequence, return `IEnumerable<T>` and document that it is lazy.

---

## Q16. How do you cancel an `IAsyncEnumerable<T>` mid-stream?

**Concepts**
- `CancellationToken` passed to `GetAsyncEnumerator(CancellationToken)` at the start
- `[EnumeratorCancellation]` attribute wires the token from `WithCancellation()` into the iterator
- `await foreach` combined with `.WithCancellation(token)` passes the token through
- Iterator body should pass `ct` to every `await` inside it for prompt cancellation
- `OperationCanceledException` surfaces in the consumer when the token is signaled

**Answer**

`IAsyncEnumerable<T>` accepts a `CancellationToken` through `GetAsyncEnumerator(CancellationToken ct)`. When writing an async iterator, decorate the `CancellationToken` parameter with `[EnumeratorCancellation]` — this makes the token injected via `.WithCancellation()` on the consumer side flow into the iterator body automatically. Inside the iterator, pass `ct` to every `await` call so each async operation (database reads, HTTP calls) observes the token and throws `OperationCanceledException` promptly when signaled. Without the `[EnumeratorCancellation]` attribute the token reaches only `GetAsyncEnumerator`, not the individual `await` expressions, so cancellation may be delayed until the next `MoveNextAsync()` boundary.

```csharp
// net10.0 — proper cancellation wiring
static async IAsyncEnumerable<PickLine> StreamAsync(
    string ticketId,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var row in _repo.QueryAsync(ticketId, ct).WithCancellation(ct))
        yield return row;
}

// Consumer with timeout
using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
await foreach (PickLine line in StreamAsync("PB-2201").WithCancellation(cts.Token))
    Process(line);
```

---

## Gotchas

---

## Q17. What happens when you access `Current` before the first `MoveNext()` or after `MoveNext()` returns `false`?

**Concepts**
- `Current` is undefined outside the valid iteration range
- Compiler-generated state machines return `default(T)` in the before-start and after-end states
- Custom implementations may throw `InvalidOperationException`
- `foreach` and correct manual loops never access `Current` in an invalid state
- The non-generic `IEnumerator.Current` with its `object` return can silently return `null`

**Answer**

The `IEnumerator<T>` contract deliberately leaves `Current` undefined before the first `MoveNext()` and after `MoveNext()` has returned `false`. Compiler-generated iterators (from `yield return`) return `default(T)` in both boundary states — for reference types that is `null`, for value types it is the zero-initialized struct. Custom enumerators may instead throw `InvalidOperationException` with "Enumeration has not started" or "Enumeration already finished." The important gotcha is that `default(T)` access does not crash — it silently returns null or zero, which can cause subtle bugs if your manual loop logic is slightly wrong. For example, calling `Current` on a freshly obtained enumerator (before any `MoveNext()`) returns `null` for a `IEnumerator<string>` rather than throwing. The safe pattern is strict: only access `Current` inside the `while (e.MoveNext())` body or inside the `foreach` loop body — never before the loop starts or in the `finally` block.

```csharp
// net10.0 — silent default(T) gotcha
IEnumerator<string> e = new List<string> { "A", "B" }.GetEnumerator();
string first = e.Current;  // default(string) == null — no exception, just null
Console.WriteLine(first ?? "(null)");  // prints "(null)" — easily missed
e.MoveNext();
Console.WriteLine(e.Current);  // "A" — now correct
```

---

## Q18. Why does modifying a `List<T>` during a `foreach` over it throw `InvalidOperationException`?

**Concepts**
- `List<T>` maintains an internal `_version` counter
- `List<T>.Enumerator` captures the version at construction time
- `MoveNext()` checks that the current version matches the captured version
- Any structural modification (`Add`, `Remove`, `Clear`, `Insert`) increments `_version`
- Version mismatch causes `InvalidOperationException`: "Collection was modified"

**Answer**

`List<T>` guards against concurrent mutation by storing a version counter (`_version`) that increments on every structural change — `Add`, `Remove`, `Clear`, `Insert`, `RemoveAt`, `Sort`, and `Reverse` all bump it. When `foreach` calls `GetEnumerator()`, the returned `List<T>.Enumerator` struct copies the current version. Every subsequent `MoveNext()` call checks whether `_list._version == _version` and throws if they differ. This is a fail-fast safety mechanism: it prevents you from accidentally processing wrong data when the list shifts under you — removing an item can cause the enumerator to skip the next element or revisit a previous one depending on direction. The fix depends on intent: if you need to filter and remove, use `list.RemoveAll(predicate)` in one step; if you need to iterate a snapshot, materialize first with `foreach (var x in list.ToList())`; if you need a reverse walk with removal, use a downward `for (int i = list.Count - 1; i >= 0; i--)`. Dictionary and most other BCL mutable collections use the same version-counter pattern for the same reason.

---

## Q19. What is the multiple-enumeration gotcha with `IEnumerable<T>` from an I/O source?

**Concepts**
- `IEnumerable<T>` from `yield return` over a database query re-executes the query each time
- LINQ chains composed over such a source re-execute the full chain per terminal operation
- `Count()` then `foreach` on the same lazy reference: two full database passes
- No compile-time warning for multiple enumeration — common code-review bug
- Roslyn analyzer `CA1851` / third-party tools detect likely double enumeration

**Answer**

This is the single most common production bug involving `IEnumerable<T>`. A service method returns `IEnumerable<PickLine>` built from `yield return` over a `DbDataReader`. The caller writes `int n = lines.Count(); foreach (PickLine l in lines) ...`. Each terminal operation — `Count()` and the `foreach` — independently calls `GetEnumerator()`, which re-runs the iterator body, re-executes the SQL, and reads rows from the database again. The `Count()` pass correctly returns 42; by the time `foreach` runs, the ticket might have been updated, returning 43 rows — or the connection context is gone and it throws. Even for in-memory `yield return` filters with side effects, the side effects run twice. The compiler gives no warning because the language specification says `IEnumerable<T>` may be enumerated multiple times. Roslyn analyzer `CA1851` ("Possible multiple enumerations of IEnumerable") and JetBrains Rider's analysis highlight these patterns. The fix is materialization: `var lines = service.GetLines().ToList();` before any LINQ or loop, or return `IReadOnlyList<T>` from the service to make the contract explicit.

---

## Q20. What is the `yield return` inside `try/catch` restriction and what is the `try/finally` rule?

**Concepts**
- `yield return` is not allowed inside a `catch` block
- `yield return` is not allowed inside a `finally` block
- `yield return` IS allowed inside a `try` block that has a `finally` (but not a `catch`)
- This restriction exists because the state machine cannot model resumption from within a catch handler
- The workaround is to yield before entering the try, or restructure to capture the value first

**Answer**

The C# compiler enforces a restriction: `yield return` cannot appear inside a `catch` block or a `finally` block, because resuming execution from within an exception handler or cleanup block is not expressible in the state machine model. However, `yield return` can appear inside a `try` block as long as that block does not have a `catch` clause — only a `finally` is permitted alongside a `try` that contains `yield return`. The `finally` clause runs when the consumer disposes the state machine, ensuring cleanup happens even if the consumer breaks out early. The practical impact is that you cannot yield elements from inside an exception handler. The workaround is to capture the value before the `try` and yield it after, or restructure so the potentially-throwing work happens outside the yield statement. Attempting to write `yield return` in a `catch` block is a compile-time error: "Cannot yield in the body of a catch clause."

```csharp
// net10.0 — LEGAL: yield inside try with finally, no catch
static IEnumerable<string> ReadLines(string path)
{
    using StreamReader reader = new StreamReader(path);
    string? line;
    while ((line = reader.ReadLine()) is not null)
        yield return line;   // legal: try block is implicit in using, no catch
}

// ILLEGAL — compile error:
// try { yield return "x"; } catch (Exception) { yield return "err"; }
```

---

## Q21. What happens to captured variables and closures defined inside an iterator method?

**Concepts**
- Local variables used after a `yield return` become fields on the compiler-generated state machine
- Lambdas captured inside the iterator close over state machine fields, not stack variables
- Shared mutable state between lambda captures and iterator variables causes unexpected aliasing
- Loop variable capture in pre-C# 5 style can produce the "modified closure" bug inside iterators
- All locals in an iterator are heap-allocated as part of the state machine object

**Answer**

Every local variable and parameter referenced across a `yield return` boundary is promoted from a stack slot to a field on the heap-allocated state machine class. This means that closures (lambdas) created inside the iterator and capturing local variables actually close over the state machine fields. The classic "modified closure" issue — where multiple lambdas created in a loop all reference the same loop variable — applies here too. A `for` loop index captured in a lambda inside an iterator produces one field for the index; all lambdas created during different iterations close over the same field. By the time the lambdas run (after iteration ends), the field holds the final loop value. The .NET 10 compiler handles `foreach` loop variables correctly — a fresh captured slot per iteration — but classic `for (int i = 0; ...)` with a captured `i` still aliases. Beyond the aliasing issue, the fact that all iterator locals live on the heap means memory is held for the lifetime of the state machine object (until the last consumer disposes it), not just the iteration scope. For large buffers or heavy objects allocated in an iterator, this can cause unexpected memory retention.

---

## Q22. Why does `Reset()` throw `NotSupportedException` on compiler-generated iterators?

**Concepts**
- Compiler-generated state machine has no "rewind" transition
- States model a forward-only progress from initial through each yield to done
- Adding reverse transitions would require replaying arbitrary method logic
- `IEnumerator` contract inherits `Reset()` but the C# spec permits `NotSupportedException`
- LINQ and `foreach` never call `Reset()` — only legacy COM-era code relies on it

**Answer**

The compiler translates an iterator method into a state machine where each `yield return` is a numbered state and `MoveNext()` advances through them in sequence. There is no reverse gear — the states only flow forward, and reaching the done state transitions the machine permanently. Implementing `Reset()` would require the state machine to return to state zero and replay the entire method body from the beginning, which is impossible for arbitrary logic (imagine a method that opens a file then yields lines — `Reset()` would need to reopen the file). The C# specification explicitly allows iterator-generated enumerators to throw `NotSupportedException` from `Reset()`. This is why every compiler-generated iterator does exactly that. The correct pattern when a second pass is needed is always `GetEnumerator()` again — for `IEnumerable<T>` iterators that re-execute the body, the second call produces a fresh state machine. Legacy code calling `Reset()` on an iterator will crash at runtime. When reviewing code that calls `Reset()`, flag it as using an unreliable API; replace with a second `foreach` or a second `GetEnumerator()` call.

---

## Real-World Scenarios

---

## Q23. Code review: a warehouse report service enumerates a lazy `IEnumerable<PickLine>` three times. Review the following method and identify issues.

```csharp
public ReportSummary BuildReport(string ticketId)
{
    IEnumerable<PickLine> lines = _repo.GetLines(ticketId); // yield return over DbDataReader

    int count   = lines.Count();
    decimal kg  = lines.Sum(l => l.TotalWeightKg);
    string skus = string.Join(", ", lines.Select(l => l.Sku));

    return new ReportSummary(count, kg, skus);
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Multiple enumeration | `Count()`, `Sum()`, and `Select()` each call `GetEnumerator()` independently | Database query executes three times; three round-trips per report request |
| Data consistency | Each pass may see different data if the ticket changes between calls | `count`, `kg`, and `skus` can reflect different versions of the same ticket |
| Resource leaks | Each enumeration opens a `DbDataReader`; an exception mid-pass may leave one open | Connection pool exhaustion under concurrent load |
| API contract | Returning `IEnumerable<T>` from `_repo.GetLines()` does not signal "expensive re-enumeration" | Any future caller of the method also risks triple enumeration |

**Fix priority**

1. Materialize once at the top of the method: `var lines = _repo.GetLines(ticketId).ToList();` — one query, one consistent snapshot.
2. Replace three LINQ terminal operators with a single pass using `Aggregate` or a manual loop to compute `count`, `kg`, and `skus` simultaneously, avoiding even the `ToList` allocation if memory is a concern.
3. Change `_repo.GetLines()` to return `Task<IReadOnlyList<PickLine>>` so the signature itself communicates that I/O has been performed and the result is stable.
4. Add unit test that asserts the mock repository's `GetLines` is called exactly once per `BuildReport` invocation.

```csharp
// net10.0 — fixed: single materialization
public ReportSummary BuildReport(string ticketId)
{
    IReadOnlyList<PickLine> lines = _repo.GetLines(ticketId).ToList();

    int     count = lines.Count;
    decimal kg    = lines.Sum(l => l.TotalWeightKg);
    string  skus  = string.Join(", ", lines.Select(l => l.Sku));

    return new ReportSummary(count, kg, skus);
}
```

---

## Q24. Code review: a bulk-import service opens a file enumerator but forgets to dispose it when breaking early. Review the method.

```csharp
public int ImportUntilError(string filePath)
{
    IEnumerator<PickLine> walker = ParseLines(filePath).GetEnumerator();
    int imported = 0;

    while (walker.MoveNext())
    {
        PickLine line = walker.Current;
        if (!_validator.IsValid(line))
            return imported;           // returns without Dispose

        _store.Save(line);
        imported++;
    }

    return imported;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | `walker.Dispose()` never called when `return` exits early | `StreamReader` inside `ParseLines` stays open; file locked until GC finalizer |
| Exception safety | An exception from `_validator` or `_store.Save` also bypasses `Dispose` | Same file lock leak on any unhandled exception path |
| Pattern violation | Manual enumerator loop missing `using` | Mirrors the exact anti-pattern flagged in Program.cs Section 4b tutorial |
| Scalability | Under load, temp directory fills with open file handles | OS-level `Too many open files` errors at moderate request concurrency |

**Fix priority**

1. Wrap the enumerator in `using`: `using IEnumerator<PickLine> walker = ParseLines(filePath).GetEnumerator();` — disposes on every exit path including exceptions.
2. Prefer `foreach` over `ParseLines(filePath)` when you do not need the raw enumerator — the compiler emits the `try/finally` automatically.
3. If early return must be preserved and `foreach` is used, use `break` instead of `return` inside `foreach` to trigger the compiler's implicit `Dispose`.
4. Add integration test that opens the file immediately after `ImportUntilError` returns on a bad line to confirm the handle is released.

```csharp
// net10.0 — fixed: using guarantees Dispose on every exit
public int ImportUntilError(string filePath)
{
    int imported = 0;
    foreach (PickLine line in ParseLines(filePath))
    {
        if (!_validator.IsValid(line))
            break;          // foreach finally block calls Dispose
        _store.Save(line);
        imported++;
    }
    return imported;
}
```

---

## Q25. Code review: a UI service tries to clean short lines while iterating the same `List<T>`. Review the method.

```csharp
public void PruneAndAssign(List<PickLine> lines)
{
    foreach (PickLine line in lines)
    {
        if (line.Quantity < 5)
            lines.Remove(line);     // mutates the list being iterated
        else
            _picker.Assign(line);
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Runtime crash | `List<T>._version` incremented by `Remove`; next `MoveNext()` throws `InvalidOperationException` | Method crashes on the first removal — always reproducible |
| Logic error | Even without the exception, removing at current index shifts later elements left, causing the element immediately after the removed one to be skipped | Silent data loss: some valid lines never reach `_picker.Assign` |
| Semantic confusion | Treating `foreach` as an index-based loop with in-place removal | Common mistake when migrating from index-for loops to `foreach` |

**Fix priority**

1. Preferred: use `lines.RemoveAll(l => l.Quantity < 5)` to filter in one pass, then `foreach` the cleaned list for `_picker.Assign` — clearest intent, zero exception risk.
2. Reverse `for` with index if `RemoveAll` is not available: `for (int i = lines.Count - 1; i >= 0; i--)` — removing at `i` does not affect indices below `i`.
3. Snapshot approach: `foreach (PickLine line in lines.ToList())` iterates a copy so the original can be mutated safely — readable but allocates a second list.
4. Never mix structural mutation and `foreach` on the same `List<T>` instance — enforce via code-review checklist or Roslyn analyzer.

```csharp
// net10.0 — fixed: separate filter and assign passes
public void PruneAndAssign(List<PickLine> lines)
{
    lines.RemoveAll(l => l.Quantity < 5);
    foreach (PickLine line in lines)
        _picker.Assign(line);
}
```

---

## Q26. A real-time feed streams 100,000 pick events per minute from Kafka. Design an `IAsyncEnumerable<T>` pipeline that processes them in batches of 500 with backpressure and cancellation.

**Concepts**
- `IAsyncEnumerable<T>` as the consumer-side abstraction over a Kafka reader
- `Channel<T>` or `IAsyncEnumerable<T>` from `Confluent.Kafka` as the producer
- Batching via a buffer loop with `MoveNextAsync()` and a count threshold
- `CancellationToken` with `[EnumeratorCancellation]` threading through every `await`
- Backpressure: bounded `Channel<T>` blocks the Kafka reader when the batch processor is slow

**Answer**

The pipeline has three layers: a Kafka consumer that pushes messages into a bounded `Channel<PickEvent>`, an async iterator that reads from the channel and batches into lists of 500, and a batch-processing step that calls the downstream service. The bounded channel provides backpressure — when the batch processor is slow, `ChannelWriter.WriteAsync` blocks the Kafka reader thread, preventing runaway memory growth. The async iterator uses `[EnumeratorCancellation]` so the consumer can cancel mid-stream and the iterator propagates the token to every `await` inside it.

```csharp
// net10.0 — streaming Kafka events in batches of 500
static async IAsyncEnumerable<List<PickEvent>> BatchedEventsAsync(
    ChannelReader<PickEvent> source,
    int batchSize,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    List<PickEvent> batch = new List<PickEvent>(batchSize);
    await foreach (PickEvent ev in source.ReadAllAsync(ct))
    {
        batch.Add(ev);
        if (batch.Count >= batchSize)
        {
            yield return batch;
            batch = new List<PickEvent>(batchSize);
        }
    }
    if (batch.Count > 0)
        yield return batch;   // flush partial final batch
}

// Consumer
await foreach (List<PickEvent> batch in BatchedEventsAsync(channel.Reader, 500, cts.Token))
    await _processor.HandleBatchAsync(batch, cts.Token);
```

The key design decisions: bounded channel size limits in-flight messages; `ReadAllAsync` on `ChannelReader<T>` returns `IAsyncEnumerable<T>` so the inner `await foreach` is naturally cancellable; yielding a fresh `List<PickEvent>` per batch (not reusing the same list reference) prevents the processor from racing against the next fill cycle.

---

## Q27. A reporting service exposes an `IEnumerable<PickLine>` property that re-queries the database on each access. A new developer adds LINQ ordering and pagination on top. Describe the performance risk and the correct API design.

**Concepts**
- Lazy `IEnumerable<T>` property re-executes query on every access
- LINQ `OrderBy` + `Skip` + `Take` on top of re-querying source adds query overhead per page
- Each pagination call triggers a full table scan then client-side ordering
- Returning `IReadOnlyList<T>` or exposing `Task<IReadOnlyList<T>>` corrects the contract
- EF Core `IQueryable<T>` is the correct primitive for server-side pagination; `IEnumerable<T>` forces client-side

**Answer**

The underlying issue is conflating two meanings of `IEnumerable<T>`: an in-memory sequence and a lazy database query. The property's getter calls the repository and returns an iterator; every LINQ operator applied to it — `OrderBy`, `Skip`, `Take` — operates in LINQ-to-Objects (client side) because the type is `IEnumerable<T>`, not `IQueryable<T>`. This means each page request loads all rows from the database, orders them in memory, then discards all but the requested page. For a table with one million rows, page 500 loads and sorts a million rows to return 20. The fix has two layers. First, change the service to expose `Task<IReadOnlyList<PickLine>> GetLinesAsync(string ticketId)` — a single materialized query, no lazy re-evaluation. Second, for genuine server-side pagination, expose `IQueryable<PickLine>` (only within the data layer, never to external API consumers) or a dedicated method `Task<Page<PickLine>> GetLinesPageAsync(string ticketId, int page, int pageSize)` that builds the SQL with `ORDER BY`, `OFFSET`, and `FETCH NEXT` before executing. The contract the return type communicates is more important than the implementation detail hidden behind it.

---

## Q28. Design a custom `IEnumerable<T>` that yields pick lines in aisle-then-bay order without sorting the underlying array. Walk through the cursor design choices.

**Concepts**
- Custom sort key extracted from each `PickLine` without mutating source array
- Enumerator holds sorted index array rather than copying the data
- `GetEnumerator()` returns a fresh cursor with its own sorted index each time
- `Dispose()` is a no-op for in-memory index arrays
- Explicit non-generic `IEnumerable.GetEnumerator()` satisfies legacy callers

**Answer**

The collection type holds the source `PickLine[]` unchanged. When `GetEnumerator()` is called, it builds a sorted index array by extracting aisle and bay from each SKU (e.g., "A3-B07" → aisle "A3", bay 7) and sorting those indices. The cursor (`AisleOrderEnumerator`) stores this sorted index array and a position cursor. `MoveNext()` advances the position and returns `true` while within bounds; `Current` returns `_lines[_sortedIndices[_pos]]`. This design avoids copying the elements, keeps the source array intact, and produces a new sort order on every `GetEnumerator()` call — two independent `foreach` loops get independent cursors that both sort independently. For a production system with millions of lines, the sort-on-demand cost would prompt caching the sorted index, but for warehouse batches of a few hundred lines it is clean and correct.

```csharp
// net10.0 — aisle-order enumerable with sorted index cursor
public sealed class AisleOrderedBatch : IEnumerable<PickLine>
{
    private readonly PickLine[] _lines;
    public AisleOrderedBatch(PickLine[] lines) => _lines = lines;

    public IEnumerator<PickLine> GetEnumerator()
    {
        int[] sorted = Enumerable.Range(0, _lines.Length)
            .OrderBy(i => ExtractAisle(_lines[i].Sku))
            .ThenBy(i => ExtractBay(_lines[i].Sku))
            .ToArray();
        return new AisleEnumerator(_lines, sorted);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static string ExtractAisle(string sku) => sku.Split('-')[0];
    private static int    ExtractBay(string sku)
        => int.TryParse(sku.Split('-').ElementAtOrDefault(1), out int b) ? b : 0;

    private sealed class AisleEnumerator : IEnumerator<PickLine>
    {
        private readonly PickLine[] _lines;
        private readonly int[]      _sorted;
        private int _pos = -1;

        public AisleEnumerator(PickLine[] lines, int[] sorted)
            { _lines = lines; _sorted = sorted; }

        public PickLine Current => _lines[_sorted[_pos]];
        object IEnumerator.Current => Current;
        public bool MoveNext() { _pos++; return _pos < _sorted.Length; }
        public void Reset()    { _pos = -1; }
        public void Dispose()  { }
    }
}
```
