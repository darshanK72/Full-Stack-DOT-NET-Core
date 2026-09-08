# Var, Dynamic & Special Keywords — Interview Q&A


## Table of Contents

1. [Q1. What does the var keyword do and what does it mean for type safety?](#q1-what-does-the-var-keyword-do-and-what-does-it-mean-for-type-safety)
2. [Q2. What is the dynamic keyword and how does it differ from var and object?](#q2-what-is-the-dynamic-keyword-and-how-does-it-differ-from-var-and-object)
3. [Q3. What is ExpandoObject and when would you use it?](#q3-what-is-expandoobject-and-when-would-you-use-it)
4. [Q4. What are nameof and typeof, and why should you prefer nameof over string literals?](#q4-what-are-nameof-and-typeof-and-why-should-you-prefer-nameof-over-string-literals)
5. [Q5. What does the default keyword do in C# 10+?](#q5-what-does-the-default-keyword-do-in-c-10)
6. [Q6. What is the checked keyword and when would you use it?](#q6-what-is-the-checked-keyword-and-when-would-you-use-it)
7. [Q7. What is the volatile keyword and what threading problem does it solve?](#q7-what-is-the-volatile-keyword-and-what-threading-problem-does-it-solve)
8. [Q8. What does the unsafe keyword enable and what risks does it introduce?](#q8-what-does-the-unsafe-keyword-enable-and-what-risks-does-it-introduce)
9. [Q9. What is stackalloc and when is it preferred over heap allocation?](#q9-what-is-stackalloc-and-when-is-it-preferred-over-heap-allocation)
10. [Q10. What is the difference between ref, in, and out parameters?](#q10-what-is-the-difference-between-ref-in-and-out-parameters)
11. [Q11. What does the global:: qualifier do and when is it needed?](#q11-what-does-the-global-qualifier-do-and-when-is-it-needed)
12. [Q12. Why does dynamic typed code miss typos and broken contracts until runtime?](#q12-why-does-dynamic-typed-code-miss-typos-and-broken-contracts-until-runtime)
13. [Q13. Why does var capture a deferred LINQ query, and what is the deferred-execution trap?](#q13-why-does-var-capture-a-deferred-linq-query-and-what-is-the-deferred-execution-trap)
14. [Q14. Why is volatile insufficient for a counter incremented by multiple threads?](#q14-why-is-volatile-insufficient-for-a-counter-incremented-by-multiple-threads)
15. [Q15. (Code Review) A warehouse integration mapper uses dynamic for CSV rows and throws in production. Review and harden it.](#q15-code-review-a-warehouse-integration-mapper-uses-dynamic-for-csv-rows-and-throws-in-production-review-and-harden-it)
16. [Q16. A price-refresh worker ignores a stop request in production. Review the stop flag mechanism.](#q16-a-price-refresh-worker-ignores-a-stop-request-in-production-review-the-stop-flag-mechanism)

---
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

## Gotchas — var, dynamic & Special Keywords (Interview Traps)

---

#### Gotcha 1. `var` is statically typed — the type is inferred at compile time, not at runtime

**Concepts**
- `var` resolves to a concrete type at compile time via type inference
- no runtime type resolution; IL is identical to using the explicit type
- `var` cannot change the declared type after initialization
- `object` or `dynamic` required for true runtime type flexibility

**Answer**

`var x = 42;` is syntactic sugar: the compiler infers that `x` is `int` and emits identical IL to `int x = 42;`. There is no runtime overhead and no dynamic lookup. The declared type is fixed at the point of assignment — `x` cannot later hold a `string`. This makes `var` purely a readability tool, not a runtime feature. A common misconception is that `var` makes code more dynamic or flexible at runtime; it does not. If you need a variable whose type changes at runtime, you need `object` (with manual casting) or `dynamic` (with DLR dispatch). Overusing `var` where the inferred type is non-obvious (e.g., `var result = GetSomething();`) reduces readability without any performance benefit.

---

#### Gotcha 2. `dynamic` bypasses compile-time type checking — errors appear at runtime

**Concepts**
- compiler emits DLR call sites instead of typed IL for `dynamic` accesses
- no IntelliSense, no compile-time member resolution
- `RuntimeBinderException` on missing members at runtime
- testing `dynamic` code requires runtime execution to find member-access bugs

**Answer**

Declaring a variable as `dynamic` tells the compiler to skip all type checking for member accesses on that variable. `dynamic d = GetSomething(); d.Proccess();` (with a typo) compiles without error and throws `RuntimeBinderException` at runtime when the misspelled method is not found. This shifts an entire class of bugs — wrong method names, wrong argument types, wrong return type assumptions — from the compile step to the runtime step, making them harder to find. Unit tests must exercise every `dynamic` code path explicitly; a refactor that renames a method does not cause a compile error in calling `dynamic` code, only a runtime exception in production if the test coverage is incomplete.

---

#### Gotcha 3. `dynamic` and `ExpandoObject` — adding properties dynamically, no IntelliSense

**Concepts**
- `ExpandoObject` implements `IDynamicMetaObjectProvider` and `IDictionary<string, object>`
- properties added at runtime via `dynamic` syntax or dictionary
- no compile-time member checks; no IntelliSense on dynamic variable
- serialization: `ExpandoObject` serializes as a JSON object when accessed via `IDictionary`

**Answer**

`dynamic person = new ExpandoObject(); person.Name = "Alice"; person.Age = 30;` adds properties to the expandable bag at runtime. This is useful for building ad hoc DTOs in scripting or interop scenarios, but the dynamic variable loses all IDE support — no autocomplete, no rename refactoring, no static analysis. Code that reads `person.Name` will compile even after the property was renamed or removed because the compiler emits a DLR lookup. `ExpandoObject` also implements `IDictionary<string, object>`, so it can be iterated as key-value pairs and is serialized by `System.Text.Json` as a standard JSON object. For production code, prefer explicitly typed records or DTOs; use `ExpandoObject` only when the property set is genuinely unknown at compile time.

---

#### Gotcha 4. `checked`/`unchecked` blocks — overflow behavior for arithmetic

**Concepts**
- default integer arithmetic is unchecked — overflow wraps silently
- `checked` block throws `OverflowException` on integer overflow
- `unchecked` keyword suppresses overflow checking inside a `checked` context
- only applies to integer arithmetic; floating-point overflow produces `±Infinity`

**Answer**

By default, integer arithmetic in C# is unchecked: `int.MaxValue + 1` silently wraps to `int.MinValue` without any exception or warning. Wrapping overflow is a well-known source of security vulnerabilities in buffer size calculations. Wrapping the computation in a `checked` block — `checked { int result = a + b; }` — causes an `OverflowException` to be thrown if the result exceeds the type's range. The `checked` and `unchecked` keywords can also be applied as expressions: `checked(a * b)`. The compiler flag `/checked` enables overflow checking globally for a project. `unchecked` is used inside a globally checked context to explicitly opt out for a known-safe or intentional wrap (e.g., computing a hash code). Floating-point overflow is always unchecked and produces `double.PositiveInfinity` or `double.NaN`, not an exception.

---

#### Gotcha 5. `unsafe` and `fixed` — pointer arithmetic in C#; requires `/unsafe` compiler flag

**Concepts**
- `unsafe` block or method enables unmanaged pointer arithmetic
- `fixed` statement pins a managed object to prevent GC movement during pointer access
- `/unsafe` compiler flag required; not allowed in AOT-restricted environments
- `Span<T>` and `Memory<T>` as the modern safe alternative to raw pointers

**Answer**

`unsafe` code in C# allows direct pointer manipulation — `int* p = &value; *p = 42;` — bypassing the type system and GC safety guarantees. To use `unsafe`, the project must set `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the project file. Accessing a managed array through a pointer requires a `fixed` statement to pin the array in memory so the garbage collector does not move it during the pointer operation: `fixed (int* p = array) { p[0] = 1; }`. Forgetting `fixed` causes a compiler error. In modern .NET, `Span<T>` and `Memory<T>` provide pointer-speed access to contiguous memory safely without `unsafe` — they are the preferred tool for high-performance I/O, parsing, and buffer manipulation.

---

#### Gotcha 6. `ref` return and `ref` local — returning a reference to a variable, not a copy

**Concepts**
- `ref return` returns a managed reference to the variable, not its value
- `ref` local aliases a variable; mutations via the alias affect the original
- cannot return `ref` to a local variable that goes out of scope
- enables in-place mutation of array elements without index re-lookup

**Answer**

`ref` returns allow a method to return a direct reference to a field, array element, or parameter rather than a copy. `public ref int GetElement(int[] arr, int index) => ref arr[index];` lets the caller write `ref int elem = ref GetElement(arr, 2); elem = 99;` to mutate `arr[2]` in place without a second indexer call. The key restriction is that you cannot return a `ref` to a local variable — the variable goes out of scope when the method returns, leaving a dangling reference — the compiler enforces this. `ref` returns are most valuable for large struct scenarios (avoiding copies) and for high-performance collection types. Confusion arises when developers expect `ref` return to behave like a pointer without understanding the lifetime constraints.

---

#### Gotcha 7. `in` parameter modifier — pass by reference, read-only inside the method

**Concepts**
- `in` passes a value type by reference (no copy) to avoid copying large structs
- `in` parameter is read-only inside the callee — compiler enforces immutability
- implicit defensive copy when calling a method on a non-readonly struct via `in`
- most useful for large value types (`readonly struct` recommended with `in`)

**Answer**

The `in` modifier passes a value type to a method by reference so the runtime does not copy the struct, improving performance for large structs (e.g., `Matrix4x4`). Inside the method the parameter is read-only — any attempt to assign to it is a compile error. The subtle gotcha is defensive copying: if the struct is not `readonly` (it has mutable methods), calling an instance method on an `in` parameter may cause the compiler to emit a hidden copy to prevent the method from modifying the original. This negates the performance benefit of `in` and makes `in` + mutable struct an anti-pattern. To benefit from `in` without defensive copies, mark the struct `readonly struct` — this guarantees all instance members are non-mutating and the compiler can safely pass the reference without copying.

---

#### Gotcha 8. `nameof()` — compile-time string of a member name; survives renaming refactors

**Concepts**
- `nameof(MyClass.Property)` returns the member name as a string at compile time
- refactoring tools rename the member and update `nameof` references automatically
- evaluates to the simple name, not the fully qualified name
- valid in attributes and other compile-time contexts where string literals work

**Answer**

`nameof(Customer.FirstName)` produces the string `"FirstName"` at compile time — identical to the string literal but refactor-safe. When you rename `FirstName` to `GivenName`, the IDE's rename refactoring updates the `nameof` expression automatically, whereas a hardcoded `"FirstName"` string would silently become stale. This is particularly important in `ArgumentNullException(nameof(firstName))`, `PropertyChanged(nameof(FirstName))`, and `[Required(ErrorMessage = "...")]` scenarios where the string must match the actual member name. `nameof` evaluates to the simple unqualified identifier: `nameof(System.Text.Json.JsonSerializer)` returns `"JsonSerializer"`, not the full namespace path.

---

#### Gotcha 9. `default` literal in C# 7.1 — `default` without explicit type

**Concepts**
- `default(T)` requires explicit type before C# 7.1
- `default` literal infers the type from context (assignment, return, method argument)
- `default` on a reference type is `null`; on a value type is the zero-initialized struct
- `default` in switch expressions and pattern matching

**Answer**

Before C# 7.1, producing the default value of a type required `default(MyStruct)`. C# 7.1 introduced the contextually typed `default` literal: `MyStruct s = default;` infers the type from the left-hand side. This works in method arguments (`Process(default)`), return statements (`return default;`), and conditional expressions (`condition ? value : default`). The literal produces `null` for reference types and the zero-initialized value for value types — a struct where every field is zero or null. A potential confusion is using `default` in a switch expression as the discard arm (`_ =>` or `default =>`), where it is a pattern, not the default-value literal. The context determines which `default` is meant.

---

#### Gotcha 10. `is` pattern matching — `is int n` assigns and type-checks in one expression; `n` is in scope after

**Concepts**
- `obj is int n` tests type and assigns to `n` in a single expression
- `n` is in scope for the remainder of the enclosing block, not just the `if` body
- `is null` never invokes `==` operator; always performs reference/null check
- combined patterns: `obj is int n and > 0` for type + value check

**Answer**

`if (obj is int n)` performs a type check and, if successful, assigns the cast value to `n` — equivalent to `if (obj is int) { int n = (int)obj; ... }` but more concise. The scope of `n` extends to the end of the enclosing block, not just the `if` body. This means `n` is technically in scope in the `else` branch (though the type check failed, `n` has an undefined value there). `obj is null` is distinct from `obj == null`: `is null` always performs a reference equality check and never calls a user-defined `==` operator, making it the reliable null check for code that must not invoke overloaded equality. Combined patterns `obj is int n and > 0` allow type checking and value constraint in a single expression.

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
