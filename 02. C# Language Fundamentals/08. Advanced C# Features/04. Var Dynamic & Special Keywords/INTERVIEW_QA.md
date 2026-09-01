# Var, Dynamic & Special Keywords — Interview Q&A

> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 04. Var Dynamic & Special Keywords  
> **Stack:** .NET 10 · C# 13 · DLR · ExpandoObject

---

## Foundation Questions

---

## Q1. What does the var keyword do and what does it mean for type safety?

**Concepts**
- compile-time implicit type inference
- var is not dynamic — type is fixed at compile time
- mandatory initializer on var declaration
- IDE readability trade-off
- LINQ query type as primary use case

**Answer**

`var` instructs the compiler to infer the static type of a local variable from the initializer expression. The type is resolved at compile time, not runtime — `var x = 42` is exactly identical to `int x = 42` in the emitted IL; the variable is strongly typed and `x.SomeStringMethod()` is still a compile error. `var` provides no dynamic behavior whatsoever. It is primarily useful when the type name is long, redundant, or anonymous — particularly with LINQ queries that return `IEnumerable<IGrouping<string, Order>>` or anonymous types where the type has no name to write. `var` is a declaration shorthand, not a runtime feature. It requires an initializer at the point of declaration; `var x;` without an initializer is a compile error. The guideline is to use `var` when the type is obvious from the right-hand side (`var list = new List<Order>()`) and to use the explicit type when the right-hand side is a method call whose return type is not self-evident (`var result = service.Compute()`).

---

## Q2. What is the dynamic keyword and how does it differ from var and object?

**Concepts**
- DLR (Dynamic Language Runtime)
- runtime binding dispatch
- no compile-time type checking
- dynamic vs object runtime type
- ExpandoObject and DynamicObject

**Answer**

`dynamic` defers all member binding, operator resolution, and method overload selection to runtime via the Dynamic Language Runtime (DLR). A `dynamic` variable compiles without errors regardless of what members you access on it; errors appear at runtime as `RuntimeBinderException` if the member does not exist. This contrasts with `var`, which resolves the type at compile time and gives full IntelliSense and compile-time checking. It also contrasts with `object`, which stores any reference type at runtime but requires an explicit cast before any non-`object` members can be accessed — the cast fails at runtime if wrong, but the intent is explicit. `dynamic` is appropriate for interoperating with COM objects (where the dispatch model is inherently dynamic), consuming IronPython or IronRuby objects, and consuming JSON or configuration data with schemas unknown at compile time when `ExpandoObject` is the backing type. In all other cases, strongly typed code is faster, safer, and more maintainable.

---

## Q3. What is ExpandoObject and when would you use it?

**Concepts**
- IDictionary<string, object> backing store
- runtime property addition
- INotifyPropertyChanged support
- JSON deserialization target
- performance versus typed class

**Answer**

`ExpandoObject` from `System.Dynamic` is a class that implements `IDynamicMetaObjectProvider` and `IDictionary<string, object>`. Assigning `((dynamic)expando).NewProp = "value"` adds a new key to the underlying dictionary; reading `((dynamic)expando).NewProp` retrieves it. This enables objects whose shape is not known until runtime — useful for building configuration bags from JSON where the key names come from external input, for scripting hosts, and for rapid prototyping. Because `ExpandoObject` implements `IDictionary<string, object>`, you can also manipulate it without `dynamic` via the dictionary interface, which is faster. The primary downside is that every member access goes through DLR dispatch and dictionary lookup — far slower than a compiled property accessor — and there is no IntelliSense or compile-time shape checking. For most real production scenarios, `System.Text.Json`'s `JsonExtensionData` or strongly typed records with nullable optional properties are the better choice over an `ExpandoObject` pipeline.

---

## Q4. What are nameof and typeof, and why should you prefer nameof over string literals?

**Concepts**
- typeof(T) produces System.Type at compile time
- nameof(member) produces string at compile time
- refactor-safe string literals
- ArgumentNullException and ArgumentException usage
- nameof vs string in reflection lookup

**Answer**

`typeof(T)` is a compile-time operator that returns the `System.Type` object for the named type — it is equivalent to `T.GetType()` but works on types, not instances, and does not require an object. It is the correct way to pass a type to reflection APIs, serializers, and attribute constructors. `nameof(expr)` evaluates to a string containing the simple name of a type, member, parameter, or local variable at compile time — `nameof(order.OrderId)` produces `"OrderId"`. The critical benefit over a string literal is refactor safety: if `OrderId` is renamed to `Id` by a rename refactor, the `nameof` expression updates automatically to `"Id"`, while a string literal `"OrderId"` silently stays wrong. `nameof` is the correct argument to `ArgumentNullException`, `ArgumentException`, `InvalidOperationException`, and reflection `GetMethod`/`GetProperty` calls. The only limitation is that `nameof` produces the identifier as written, without namespace qualification — `nameof(List<string>)` produces `"List"`, not the full generic name.

---

## Q5. What does the default keyword do in C# 10+?

**Concepts**
- default(T) target-typed default expression
- default literal (type inferred from context)
- value type zeros, reference type null
- use in generic constraints
- optional parameter defaults

**Answer**

`default` produces the zero value for any type: `null` for reference types and nullable value types, `0` for numeric types, `false` for `bool`, and a zeroed struct for value types. The original syntax `default(T)` requires naming the type explicitly; C# 7.1 introduced the target-typed `default` literal which infers the type from context — `int x = default;` is identical to `int x = default(int);`. In generic code, `default` is the only way to express the zero value of an unconstrained type parameter, since you cannot write `0` or `null` without knowing whether `T` is a value type or reference type. Common uses include initializing out parameters in methods that return early, resetting a variable to its baseline state, and providing optional parameter defaults in method signatures. In C# 10+ the pattern `x is default(T)` is valid in switch expressions for default-value matching.

---

## Q6. What is the checked keyword and when would you use it?

**Concepts**
- arithmetic overflow behavior in .NET
- unchecked (default): silent wrap-around
- checked: throws OverflowException
- checked block vs checked expression
- financial calculation discipline

**Answer**

By default, integer arithmetic in C# uses unchecked evaluation — overflow wraps silently around (an `int` at `int.MaxValue` incremented by 1 becomes `int.MinValue`). The `checked` keyword enables overflow checking: `checked { int x = int.MaxValue + 1; }` throws `System.OverflowException` instead of silently wrapping. The `unchecked` keyword explicitly opts back out of checking inside a `checked` context. `checked` is appropriate in financial, scientific, or safety-critical calculations where silent overflow would produce subtly wrong results that propagate through subsequent operations. Conversely, `unchecked` is appropriate in intentional bit manipulation, hash functions, and checksum algorithms where modular wraparound is the desired behavior. The compiler flag `/checked+` enables checked arithmetic globally; most production codebases use the default unchecked mode and apply `checked` selectively to high-risk calculations.

---

## Q7. What is the volatile keyword and what threading problem does it solve?

**Concepts**
- CPU register caching of variables
- memory visibility across threads
- volatile forces read/write from main memory
- not a full lock (no atomicity for composite operations)
- Interlocked for atomic increment

**Answer**

`volatile` is a modifier on a field that instructs the C# compiler and JIT not to cache the field's value in a CPU register across reads. Without it, a compiler or CPU may legally hoist a field read out of a loop if it determines the field is not written in that thread — causing a background loop that checks `_stopRequested` to never see the update from another thread, effectively running forever. Marking a `bool _stopRequested` field as `volatile` ensures that every read fetches the current value from the memory bus rather than a stale register copy. The important caveat is that `volatile` does not provide atomicity — for a `bool` toggle it is sufficient, but for a counter that must be incremented atomically across threads, use `Interlocked.Increment` instead. For complex state transitions, use `lock` or a `Monitor`. `CancellationToken` is the idiomatic .NET replacement for `volatile bool` stop flags in production code.

---

## Q8. What does the unsafe keyword enable and what risks does it introduce?

**Concepts**
- pointer types in C#
- fixed statement to pin managed memory
- stackalloc for stack-allocated buffers
- /unsafe compiler flag required
- memory safety bypassed

**Answer**

`unsafe` marks a method, block, class, or struct as containing unmanaged pointer code, unlocking C-style pointer arithmetic, the `fixed` statement to pin a managed object in memory during a pointer operation, and `stackalloc` for stack-allocated arrays. It requires the `/unsafe` compiler flag (or `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the project file). The primary use cases are interoperability with native libraries via P/Invoke, direct manipulation of memory buffers in high-performance scenarios (image processing, cryptographic primitives), and the implementation of `Span<T>` and `Memory<T>` in the BCL itself. The risks are significant: pointer arithmetic bypasses bounds checking, a bug can corrupt adjacent memory or access freed memory, and the garbage collector cannot move pinned objects which increases fragmentation. In .NET 10 most of the performance scenarios that historically required `unsafe` can be addressed with `Span<T>`, `Memory<T>`, and `MemoryMarshal` — reaching for `unsafe` should be justified by a benchmark.

---

## Foundation Questions (continued)

---

## Q9. What is stackalloc and when is it preferred over heap allocation?

**Concepts**
- stack-allocated buffer (no GC pressure)
- Span<T> as safe wrapper for stackalloc
- size limit (typically ~1 MB stack per thread)
- not for async methods holding references across await
- performance-critical small-buffer scenarios

**Answer**

`stackalloc T[n]` allocates a contiguous block of `n` elements of type `T` on the current thread's stack rather than on the GC heap. Because the allocation lives on the stack, it is automatically reclaimed when the method returns with no GC involvement. In C# 7.2+ the result can be wrapped in a `Span<T>` without requiring `unsafe` code, which gives bounds-checked access: `Span<byte> buf = stackalloc byte[128]`. This is appropriate for small, fixed-size buffers used within a single method — for example, assembling a binary protocol header, performing a fast hash, or parsing a known-length token. The constraints are: the size should be bounded and small (typically under a few kilobytes to avoid stack overflow); `stackalloc` inside an `async` method that crosses an `await` is illegal because the stack frame may move; and `stackalloc` with `Span<T>` cannot escape the method that allocated it. For unknown-size or large buffers, `ArrayPool<T>.Shared.Rent(size)` is the GC-friendly alternative.

---

## Q10. What is the difference between ref, in, and out parameters?

**Concepts**
- ref: read/write alias to caller's variable
- out: write-only, must be assigned before return
- in: read-only alias (avoids copy for large structs)
- caller must pre-initialize ref and in, not out
- use cases: performance structs, TryParse pattern

**Answer**

`ref`, `in`, and `out` are all parameter-passing modifiers that pass a variable by reference rather than by value, avoiding a copy. `out` requires the called method to assign the parameter before returning and the caller does not need to initialize it — the canonical pattern is `TryParse(string s, out int result)`. `ref` requires the caller to initialize the variable before passing it and the method may both read and write it — used for update-in-place patterns. `in` is a read-only ref introduced in C# 7.2: the called method cannot assign to the parameter, making it an efficient way to pass large value types (like a 64-byte `Matrix4x4`) without copying while preventing accidental mutation. The caller must pass a variable (not a literal) for `ref` and `in`, and must use the keyword at the call site — `SomeMethod(in matrix)` — making the pass-by-ref intent visible at the call site. For all three, the aliased variable must not outlive the stack frame that contains it; returning a `ref` to a local variable is a compile error.

---

## Q11. What does the global:: qualifier do and when is it needed?

**Concepts**
- fully qualified name from global namespace root
- collision between user type and BCL type
- namespace ambiguity in large solutions
- global using directive (.NET 6+)
- file-scoped namespace conflict

**Answer**

`global::` is a namespace alias that roots a qualified name at the global namespace, bypassing any local namespace or `using` directive that might shadow a type name. It is needed when a user-defined type in the current namespace has the same name as a BCL type — for example, if a team adds `class Math` to `WarehouseAnalytics.Pricing`, unqualified calls to `Math.Round` inside that namespace resolve to the user-defined `Math`, not `System.Math`. Writing `global::System.Math.Round(value, 2)` explicitly roots the lookup at the global namespace and reaches `System.Math` unambiguously. This situation is a symptom of a naming collision that should be resolved by renaming the conflicting type. In .NET 6+ `global using System;` adds a global implicit using for an entire project; `global::` is still needed for the disambiguation case and is unaffected by global usings.

---

## Gotchas

---

## Q12. Why does dynamic typed code miss typos and broken contracts until runtime?

**Concepts**
- no IntelliSense on dynamic
- RuntimeBinderException at runtime not compile time
- test coverage required for dynamic paths
- refactor blindness
- production incident rate higher than typed code

**Answer**

`dynamic` suppresses all compile-time member resolution. A typo like `row.Qantity` (misspelled "Quantity") compiles successfully — the compiler emits a DLR call-site that will attempt to resolve `Qantity` at runtime. If the backing object (an `ExpandoObject`, a `JsonElement`, a COM object) does not have a property named `Qantity`, the call throws `RuntimeBinderException`. In a batch job this means the first malformed row crashes the process rather than producing a compile error during development. Automated tests that only run the happy path with valid data provide no protection because the typo is syntactically correct. The production risk of `dynamic` code scales with the number of members accessed and the number of callers — each access point is a potential runtime failure. The mitigation is to use `dynamic` only at the boundary where truly dynamic data enters the system, map it immediately to a strongly typed DTO using a schema-validating step, and never pass `dynamic` values deeper into business logic.

---

## Q13. Why does var capture a deferred LINQ query, and what is the deferred-execution trap?

**Concepts**
- IEnumerable<T> vs materialized collection
- LINQ deferred execution
- var type is IEnumerable<T> not List<T>
- collection mutated between query and enumeration
- ToList() / ToArray() to force materialization

**Answer**

`var lowStock = stock.Where(i => i.Quantity < 15)` assigns an `IEnumerable<InventoryItem>` — a deferred query, not a snapshot. The LINQ chain is not executed at the point of assignment; it executes when the `foreach` or `Count()` is called. If the source collection is mutated between the query definition and its enumeration — for example, a restock operation bumps quantities — the query evaluates against the mutated data, not the data as it was when the query was defined. Callers expecting a snapshot of the low-stock state at query time receive results reflecting the post-mutation state, which silently misses some alerts or generates false ones. The fix is `var lowStock = stock.Where(…).OrderBy(…).ToList()` to force immediate materialization into a `List<InventoryItem>`. The lesson is that `var` accurately reflects the inferred type — in this case, a lazy enumerable — and the programmer must be aware of deferred execution semantics when the source may be modified.

---

## Q14. Why is volatile insufficient for a counter incremented by multiple threads?

**Concepts**
- volatile provides visibility not atomicity
- read-modify-write is not atomic
- Interlocked.Increment for atomic counter
- race condition window
- lock vs Interlocked trade-off

**Answer**

`volatile` ensures that a read or write to a field observes the most recent value written by any thread — it prevents stale register caching. However, the increment operation `x++` is not a single machine instruction; it compiles to a read, an add, and a write. With multiple threads, two threads can both read the same value before either writes back, then both write `original + 1` — producing a count of one instead of two. `volatile` on `x` only guarantees that each individual read and write is not cached; it does not prevent the race between the three sub-steps. The correct solution for an integer counter shared across threads is `Interlocked.Increment(ref _counter)`, which is a single atomic compare-and-swap operation guaranteed to increment exactly once per call regardless of concurrency. `volatile bool` is sufficient for a simple stop flag (one writer, many readers, no compound operation) but not for counters, flags with multiple states, or any operation involving more than one field.

---

## Real-World Scenarios

---

## Q15. (Code Review) A warehouse integration mapper uses dynamic for CSV rows and throws in production. Review and harden it.

```csharp
public sealed class DynamicImportMapper
{
    public decimal ComputeLineTotal(dynamic row)
    {
        var sku = row.Sku;
        var qty = row.Qantity;      // typo: Quantity
        var price = row.UnitPrice;
        return qty * price;
    }

    public void ImportBatch(IEnumerable<dynamic> rows)
    {
        foreach (dynamic row in rows)
        {
            var total = ComputeLineTotal(row);
            _ledger.Post(row.Sku, total);
        }
    }
}
```

**Concepts**
- RuntimeBinderException from member typo
- no compile-time checking on dynamic
- missing null/error handling for malformed rows
- batch abort on first bad row
- immediate typed mapping at boundary

| Category | Problem | Impact |
|---|---|---|
| Correctness | `row.Qantity` typo — RuntimeBinderException on first row | Entire batch fails; no partial progress |
| Correctness | No null check on `qty` or `price` before multiply | DynamicException or NullReferenceException for missing columns |
| Design | `dynamic` used for entire pipeline — no compile-time shape | Any future vendor column rename silently breaks compute |
| Reliability | Exception in `ComputeLineTotal` propagates without row context | Impossible to identify which row failed |

**Fix priority:**
1. Define a typed `CsvImportRow` record with all expected columns and deserialize the CSV into it at the entry point.
2. Replace the `dynamic` map with a schema-validation step that checks required columns and reports all errors before posting.
3. If `dynamic` is required for backward compatibility, add a `try/catch` per row that skips and logs the row with its index rather than aborting the batch.
4. Correct the typo `Qantity` → `Quantity` in the interim.

---

## Q16. A price-refresh worker ignores a stop request in production. Review the stop flag mechanism.

```csharp
public sealed class PriceRefreshWorker
{
    private bool _stopRequested;

    public void RequestStop() => _stopRequested = true;

    public void RunLoop()
    {
        while (!_stopRequested)
        {
            RefreshNextSku();
            Thread.Sleep(10);
        }
    }
}
```

**Concepts**
- bool field not volatile — stale CPU register read
- passes locally on single-core / low-load
- volatile bool as minimal fix
- CancellationToken as idiomatic replacement
- Thread.Sleep(10) preventing cooperative cancellation

**Answer**

The non-volatile `bool _stopRequested` field allows the JIT to cache its value in a CPU register inside the `while` loop, because the field is not written in the loop body. On a single-core dev machine or under low load the JIT may not apply this optimization aggressively, so the bug passes locally. On a multi-core production machine under load, the JIT may hoist the read, and the thread never sees the `true` written by `RequestStop`. The minimal fix is `private volatile bool _stopRequested`, which prevents register caching. The idiomatic .NET replacement is `CancellationToken`: pass a `CancellationTokenSource` to the worker, check `token.IsCancellationRequested` in the loop condition, and pass the token to any async operations. This integrates with `IHostedService` cancellation, supports cooperative cancellation inside the `RefreshNextSku` call, and provides proper exception semantics (`OperationCanceledException`) when the token fires. `Thread.Sleep(10)` should also be replaced with `Task.Delay(10, cancellationToken)` in an async worker to allow the sleep itself to respond to cancellation.
