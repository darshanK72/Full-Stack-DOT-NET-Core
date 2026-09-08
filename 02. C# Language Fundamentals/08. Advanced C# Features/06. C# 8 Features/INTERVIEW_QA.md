# C# 8 Features — Interview Q&A


## Table of Contents

1. [Q1. What are nullable reference types (NRT) and what problem do they solve?](#q1-what-are-nullable-reference-types-nrt-and-what-problem-do-they-solve)
2. [Q2. What is the null-forgiving operator ! and when is it safe to use?](#q2-what-is-the-null-forgiving-operator-and-when-is-it-safe-to-use)
3. [Q3. What is IAsyncEnumerable<T> and how does it differ from IEnumerable<T>?](#q3-what-is-iasyncenumerablet-and-how-does-it-differ-from-ienumerablet)
4. [Q4. What are switch expressions in C# 8 and how do they differ from switch statements?](#q4-what-are-switch-expressions-in-c-8-and-how-do-they-differ-from-switch-statements)
5. [Q5. What are default interface methods (DIM) and what problem do they solve?](#q5-what-are-default-interface-methods-dim-and-what-problem-do-they-solve)
6. [Q6. What are indices and ranges in C# 8 and how do they work?](#q6-what-are-indices-and-ranges-in-c-8-and-how-do-they-work)
7. [Q7. What does using var (using declaration) do in C# 8?](#q7-what-does-using-var-using-declaration-do-in-c-8)
8. [Q8. What is the null-coalescing assignment operator ??= in C# 8?](#q8-what-is-the-null-coalescing-assignment-operator-in-c-8)
9. [Q9. What are readonly struct members in C# 8?](#q9-what-are-readonly-struct-members-in-c-8)
10. [Q10. What is the property pattern in C# 8 and how is it used in switch expressions?](#q10-what-is-the-property-pattern-in-c-8-and-how-is-it-used-in-switch-expressions)
11. [Q11. What is async stream consumption with await foreach and WithCancellation?](#q11-what-is-async-stream-consumption-with-await-foreach-and-withcancellation)
12. [Q12. Why does using ! to silence NRT warnings on nullable properties cause NullReferenceException at runtime?](#q12-why-does-using-to-silence-nrt-warnings-on-nullable-properties-cause-nullreferenceexception-at-runtime)
13. [Q13. What is the C# 8 async stream anti-pattern of calling ToListAsync followed by synchronous foreach?](#q13-what-is-the-c-8-async-stream-anti-pattern-of-calling-tolistasync-followed-by-synchronous-foreach)
14. [Q14. What are the C# 8 rules that prevent ref struct and Span<T> from crossing async await boundaries?](#q14-what-are-the-c-8-rules-that-prevent-ref-struct-and-spant-from-crossing-async-await-boundaries)
15. [Q15. (Code Review) After enabling NRT, a document service still throws NullReferenceException on Notes. Review the service.](#q15-code-review-after-enabling-nrt-a-document-service-still-throws-nullreferenceexception-on-notes-review-the-service)
16. [Q16. (Code Review) An async stream worker OOMs and ignores cancellation on deploy. Review the consumer and producer.](#q16-code-review-an-async-stream-worker-ooms-and-ignores-cancellation-on-deploy-review-the-consumer-and-producer)
17. [Q17. (Design) A 400-project solution enables NRT repo-wide. What migration strategy avoids the 8,000-warning chokepoint?](#q17-design-a-400-project-solution-enables-nrt-repo-wide-what-migration-strategy-avoids-the-8000-warning-chokepoint)

---
> **Module:** 02. C# Language Fundamentals › 08. Advanced C# Features › 06. C# 8 Features  
> **Stack:** .NET 10 · C# 8.0 · Nullable Reference Types · IAsyncEnumerable

---

## Foundation Questions

---

## Q1. What are nullable reference types (NRT) and what problem do they solve?

**Concepts**
- #nullable enable / project-wide Nullable element
- string vs string? semantic difference
- compiler flow analysis for null safety
- nullable warning codes (CS8600, CS8602, CS8603)
- NRT as annotation layer (no runtime behavior change)

**Answer**

Nullable reference types, introduced in C# 8, extend the type system to express nullable intent for reference types. Before C# 8, `string` and `string?` were identical to the CLR — both could be null at runtime. With NRT enabled (`<Nullable>enable</Nullable>` in the project file, or `#nullable enable` per file), `string` means "not null by design" and `string?` means "may be null." The compiler performs flow analysis and emits warnings when code may dereference a nullable reference without a null check, or when a potentially null value is assigned to a non-nullable variable. This catches a category of `NullReferenceException` at compile time that would otherwise surface at runtime. Critically, NRT is a purely compile-time annotation feature — the CLR still allows any reference type to be null at runtime, so the warnings are suppressible with the null-forgiving operator `!`. Treating suppressions as a migration shortcut rather than a genuine fix defeats the purpose and re-introduces the original null-safety gap.

---

## Q2. What is the null-forgiving operator ! and when is it safe to use?

**Concepts**
- suppresses CS8601 / CS8602 warnings
- no runtime effect
- valid when logic guarantees non-null
- misuse hiding real null paths
- [MemberNotNull] and [NotNullWhen] as alternatives

**Answer**

The null-forgiving operator `!` placed after an expression (`doc!.Trim()`) suppresses nullable warnings at that point without any runtime check — it is a pure compiler directive. It is appropriate when the developer has information the compiler's flow analysis cannot prove: for example, after a factory method that always returns non-null but whose return type is `string?` for backward-compatibility reasons, or in a test fixture where the value is set in a `[SetUp]` method and the analyzer cannot track that initialization. It is misused when applied to suppress warnings on a code path that can actually be null — `metadata.Notes!.ToUpperInvariant()` throws `NullReferenceException` when `Notes` is null, and the `!` merely silences the compile-time warning that would have pointed to the bug. The preferred alternatives are `[MemberNotNull]` on methods that guarantee initialization, `[NotNullWhen(true)]` on TryGet-style methods, and actual null checks. Every `!` in a codebase should be treated as technical debt to be reviewed.

---

## Q3. What is IAsyncEnumerable<T> and how does it differ from IEnumerable<T>?

**Concepts**
- async iteration protocol
- await foreach consumer syntax
- [EnumeratorCancellation] for cooperative cancellation
- IAsyncEnumerable<T> producer via async iterator (yield return in async method)
- streaming vs buffering entire result set

**Answer**

`IAsyncEnumerable<T>` is the async counterpart to `IEnumerable<T>`. A producer method marked `async IAsyncEnumerable<T>` uses `yield return` to emit values one at a time, with `await` expressions allowed between yields — each iteration step can be an asynchronous operation (database fetch, HTTP call, file read). The consumer uses `await foreach (var item in source)` which awaits each `MoveNextAsync()` call individually. The key advantage over materializing all results into a `List<T>` is memory efficiency for large data sets: a paginated database cursor that returns millions of rows can stream them to the consumer without ever holding more than one page in memory. The cancellation contract requires the producer to accept `[EnumeratorCancellation] CancellationToken ct` and pass it to inner async operations; the consumer passes the token via `WithCancellation(token)` on the enumerable or as the second argument to `await foreach`. Calling `.ToListAsync(token)` and then iterating the list synchronously defeats the streaming benefit entirely.

---

## Q4. What are switch expressions in C# 8 and how do they differ from switch statements?

**Concepts**
- expression (not statement) — returns a value
- arms: pattern => result
- discard arm _ for default
- exhaustiveness checking (no unmatched patterns)
- property patterns and positional patterns in arms

**Answer**

Switch expressions use `value switch { pattern => result, … }` syntax and evaluate to a value rather than executing statements. They are exhaustive by default — the compiler emits a warning (and at runtime a `SwitchExpressionException`) if the input can reach a case that no arm handles. Each arm is a pattern followed by `=>` and an expression. The arms support all C# 8 patterns: constant (`"active" =>`), type (`Order o =>`), property (`{ IsHighValue: true } =>`), tuple, and positional. Compared to switch statements they are more concise for expression-valued dispatch, they enforce exhaustiveness, and they work inside expression-bodied members and LINQ. The `_` discard arm is the "default" for switch expressions: `_ => throw new ArgumentException("Unknown")` or `_ => defaultValue`. Switch expressions cannot execute side-effect-only statements (no `Console.WriteLine`) in arms — each arm must be an expression that produces a value.

---

## Q5. What are default interface methods (DIM) and what problem do they solve?

**Concepts**
- interface method with body (default implementation)
- backwards-compatible interface evolution
- diamond ambiguity with multiple interfaces
- explicit implementation required to resolve ambiguity
- not inherited by implementing class — must cast to interface

**Answer**

Default interface methods allow an interface to provide a method body. When an interface adds a new method with a default implementation, existing implementors do not need to recompile — they silently inherit the default. This solves the previously sharp pain of evolving a shared interface: adding a method broke all implementations. The typical use case is a NuGet-published interface where the provider wants to add a helper method without a breaking change. The subtlety is that the default method is not inherited by the implementing class in the normal OOP sense — calling `obj.NewMethod()` where `obj` is the concrete type does not compile if the class does not implement the method; it is only callable through the interface reference `((IMyInterface)obj).NewMethod()`. The diamond ambiguity problem occurs when a class implements two interfaces that both define a default for the same method — the compiler requires an explicit implementation on the class to resolve the ambiguity; otherwise it is a compile error.

---

## Q6. What are indices and ranges in C# 8 and how do they work?

**Concepts**
- Index type and ^ from-end operator
- Range type with .. operator
- implicit conversion from int and Range in indexers
- string, array, Span<T> support
- defensive length validation

**Answer**

C# 8 introduced `Index` and `Range` as first-class types. An `Index` value is a position in a sequence — `3` means position 3 from the start, `^3` means position 3 from the end (hat operator). A `Range` is a pair of indices — `1..4` means elements 1 through 3 (exclusive end), `^3..` means the last 3 elements. Arrays, strings, and `Span<T>` all support these via compiler-generated calls to `Slice` or subarray constructors. The expressions `arr[^1]` (last element), `arr[2..5]` (slice), and `arr[^tailCount..]` are syntactic sugar that compiles to `arr[arr.Length - 1]`, `arr[2..5]`, etc. The gotcha is that no bounds validation is performed at compile time: `documentId[4..8]` on a string shorter than 8 characters throws `ArgumentOutOfRangeException`. Any code that applies a range to user-supplied or untrusted strings must validate the length first. `^tailCount` with `tailCount = 0` produces an empty slice, which is valid but may be an unintended edge case.

---

## Q7. What does using var (using declaration) do in C# 8?

**Concepts**
- using declaration vs using statement
- variable scoped to enclosing block
- disposal at end of enclosing block not end of declaration scope
- resource leak risk in long methods
- preferred use in short-scoped methods

**Answer**

The `using var stream = File.OpenRead(path)` declaration (without braces) disposes the resource when control leaves the enclosing block — typically the end of the containing method, `if` body, or `foreach` iteration, whichever is innermost. Before C# 8, `using (var stream = …) { }` created an explicit scope; disposal happened at the closing brace. The C# 8 form is more concise but has a broader disposal scope. In a tight `foreach` loop that opens a new resource per iteration, `using var stream = File.OpenRead(path)` inside the loop body disposes at the end of each loop iteration, which is correct — the enclosing block is the loop body. However, in a long method that opens a resource at the top with `using var`, the resource is held open until the method returns, which may be much longer than necessary. The gotcha is that developers migrating from `using (…) { }` blocks may assume `using var` disposes immediately at the nearest logical boundary, but it actually holds resources until the containing block exits.

---

## Q8. What is the null-coalescing assignment operator ??= in C# 8?

**Concepts**
- ??= assigns right side only when left is null
- lazy initialization pattern
- equivalent to: if (x == null) x = value
- thread safety not implied
- use with nullable reference types

**Answer**

`x ??= defaultValue` is shorthand for `if (x == null) { x = defaultValue; }`. It assigns the right-hand expression to the variable only if the variable is currently null, and short-circuits the right-hand side evaluation if the variable is not null. This is useful for lazy initialization of optional fields, setting defaults in property setters, and initializing collection fields inline: `_cache ??= new Dictionary<string, object>()`. It works on any nullable type — reference types and `Nullable<T>` value types. The important caveat is that `??=` is not thread-safe — if two threads concurrently evaluate the operator on the same field, both may observe null and both may assign, introducing a race. For thread-safe lazy initialization use `Lazy<T>` or `Interlocked.CompareExchange`. Combined with nullable reference types, `??=` is a clean way to express "if this optional field has not been set yet, compute and set it now."

---

## Foundation Questions (continued)

---

## Q9. What are readonly struct members in C# 8?

**Concepts**
- readonly modifier on individual methods and properties
- guarantees the method does not mutate the struct
- prevents defensive copies by the JIT
- applied to getters and methods
- struct immutability discipline

**Answer**

C# 8 allows individual methods and property getters on a struct to be marked `readonly`, meaning they do not mutate any fields. Before this, if you passed a struct value via `in` (read-only reference) and called a method on it, the JIT emitted a defensive copy because it could not prove the method would not mutate the struct — the copy is returned the moment the `in` parameter is read. Marking the method `readonly` tells the compiler that no mutation occurs, so no defensive copy is needed. This is a performance optimization for high-frequency struct operations — think `System.Numerics.Vector3` or custom geometric types. The `readonly` keyword on a struct method is a correctness guarantee: if you add mutation inside a `readonly` method later, the compiler emits an error. Combined with `readonly struct` (which makes every field `readonly`), `readonly` member annotations allow per-method opt-in granularity on non-readonly structs.

---

## Q10. What is the property pattern in C# 8 and how is it used in switch expressions?

**Concepts**
- { PropertyName: pattern } syntax
- nested property pattern access
- combined with when clause
- discard pattern { }
- null check implied

**Answer**

The C# 8 property pattern matches an object by testing the values of its properties: `case { IsHighValue: true, Priority: OrderPriority.Express }:` in a switch or `order is { IsHighValue: true }` in an `is` expression. In switch expressions: `order switch { { IsHighValue: true, Priority: OrderPriority.Express } => "EXPRESS-VIP", { Priority: OrderPriority.Express } => "EXPRESS-STANDARD", _ => "STANDARD" }`. Property patterns compose with nested patterns — `{ Address: { Country: "US" } }` — and with positional patterns on deconstruct-able types. An empty property pattern `{ }` matches any non-null value and is equivalent to `is not null`. The property pattern is null-safe: the match implicitly checks that the input is not null before testing properties. This makes switch expressions with property patterns a concise, null-safe way to express multi-criteria routing logic.

---

## Q11. What is async stream consumption with await foreach and WithCancellation?

**Concepts**
- await foreach as consumer syntax
- IAsyncEnumerable<T>.GetAsyncEnumerator
- CancellationToken.WithCancellation extension
- [EnumeratorCancellation] on producer parameter
- streaming without materializing entire set

**Answer**

`await foreach (var item in asyncEnumerable)` iterates an `IAsyncEnumerable<T>` producer, awaiting each element. For cancellation, the consumer calls `await foreach (var item in source.WithCancellation(ct))` which passes the token to the enumerator via `GetAsyncEnumerator(ct)`. The producer must declare its `CancellationToken` parameter with `[EnumeratorCancellation]` — this attribute ensures that when the consumer passes the token via `WithCancellation`, it flows correctly into the parameter even if the consumer does not call `GetAsyncEnumerator` directly. If the producer does not forward the token to its inner async operations (database readers, HTTP calls), cancellation has no effect until the producer naturally completes or throws on the next blocking call. The correct producer pattern is `async IAsyncEnumerable<T> ProduceAsync([EnumeratorCancellation] CancellationToken ct = default)` with `ct` passed to every `await` inside. Without this, a deploy cancellation event results in the job running to completion — which was the original bug scenario where the worker OOMed because it materialized the entire stream before streaming to blob storage.

---

## Gotchas — C# 8 Features (Interview Traps)

---

#### Gotcha 1. Nullable reference types (NRT) — warnings only, not runtime enforcement; legacy code needs `#nullable enable`

**Concepts**
- NRT is a compile-time static analysis feature, not a runtime guard
- `string?` vs `string` — nullability annotation only; both compile to the same IL `string`
- `#nullable enable` opt-in directive for files or the whole project
- existing code full of `NullReferenceException` risks remains unchanged without annotation

**Answer**

Nullable reference types in C# 8 add compile-time warnings when you dereference a potentially null reference without a null check — they do NOT add any runtime null checks or throw at runtime. A `string?` parameter annotated as nullable compiles to the same IL as `string`; the difference is that the compiler warns if you use it without checking. Enabling `<Nullable>enable</Nullable>` in the project file turns on NRT project-wide, but legacy code written before the feature existed will suddenly show hundreds of warnings without any bugs being fixed. The pragmatic migration strategy is to enable NRT in new files with `#nullable enable` and address existing files gradually, adding `?` annotations and null checks as each file is touched.

---

#### Gotcha 2. `??=` null-coalescing assignment — `x ??= defaultValue` assigns only if x is null

**Concepts**
- `x ??= expr` is shorthand for `x = x ?? expr`
- the right-hand side is only evaluated if `x` is null
- works with nullable value types (`int?`) and reference types
- does not cover default value checks (`x == 0` or empty string) — only null

**Answer**

`x ??= GetDefault()` assigns the result of `GetDefault()` to `x` only when `x` is currently `null`. If `x` already has a non-null value, `GetDefault()` is never called — the right-hand side is lazily evaluated. This is equivalent to `if (x is null) x = GetDefault();` in one expression. A common mistake is expecting `??=` to handle all "falsy" values: `count ??= 0` does nothing when `count` is `0` (already non-null), whereas `count ??= 0` on a nullable `int?` correctly assigns `0` when `count` is `null`. The operator is strictly a null check; use `||=` patterns (via ternary or explicit `if`) for zero, empty string, or other sentinel-value replacements.

---

#### Gotcha 3. `switch` expression — exhaustiveness checked at compile time; non-exhaustive throws `SwitchExpressionException` at runtime

**Concepts**
- compiler warns on non-exhaustive switch expressions (not all inputs covered)
- `_` discard arm to handle the remaining cases
- missing arm causes `SwitchExpressionException: The switch expression does not have a matching arm` at runtime
- different from switch statement where fall-through to default is optional

**Answer**

A `switch` expression must be exhaustive — every possible input value must match one arm. If the compiler can determine that not all values are covered (e.g., an enum switch that is missing a member), it emits a warning. If coverage analysis is incomplete (e.g., switching on `string` without a `_` discard), the compiler may not warn but a non-matching input at runtime throws `SwitchExpressionException`. Adding a discard arm `_ => throw new ArgumentOutOfRangeException(...)` is both safer and more descriptive than letting the runtime throw the generic `SwitchExpressionException`. For enums, the discard arm also guards against values added to the enum in the future without updating every switch expression.

---

#### Gotcha 4. `IAsyncEnumerable<T>` and `await foreach` — requires `System.Linq.Async` for LINQ operators

**Concepts**
- `IAsyncEnumerable<T>` supports `await foreach` natively
- standard LINQ operators (`Where`, `Select`, etc.) are not defined on `IAsyncEnumerable<T>`
- `System.Linq.Async` NuGet package adds async LINQ
- `WithCancellation(ct)` to propagate `CancellationToken` through `await foreach`

**Answer**

`await foreach (var item in GetItemsAsync())` works out of the box for any type implementing `IAsyncEnumerable<T>`. However, the standard `System.Linq` extension methods like `.Where()`, `.Select()`, and `.FirstOrDefault()` are defined only on `IEnumerable<T>` — they are not available on `IAsyncEnumerable<T>`. Calling `GetItemsAsync().Where(x => x.Active)` is a compile error unless you add the `System.Linq.Async` NuGet package (from the Reactive Extensions team), which provides `AsyncEnumerable.Where`, `Select`, `ToListAsync`, and related operators. Alternatively, calling `.ToListAsync()` materializes the async sequence into a `List<T>` on which regular LINQ then works, at the cost of buffering all results in memory.

---

#### Gotcha 5. Default interface methods — implementing class does not inherit the default; call through the interface reference

**Concepts**
- default interface method (DIM) provides a fallback implementation
- a class that implements the interface but not the method does NOT have the method as a class member
- must cast to the interface to invoke the default implementation
- breaks the "interfaces have no state" contract if misused

**Answer**

When `interface ILogger` defines `void Log(string msg) => Console.WriteLine(msg);`, a class `MyLogger : ILogger` that does not override `Log` does NOT get `Log` as a public method — `new MyLogger().Log(...)` is a compile error. The default implementation is only accessible through the interface reference: `((ILogger)new MyLogger()).Log(...)`. This differs from abstract class inheritance where the base method is directly callable on the derived instance. Default interface methods were introduced primarily to allow interfaces to evolve (add new methods with defaults) without breaking existing implementors, not to serve as a general inheritance mechanism. Using them as a substitute for abstract classes leads to confusing code where methods are invisible on the concrete type.

---

#### Gotcha 6. `using` declaration — `using var stream = ...` — disposed at end of enclosing scope, not the block

**Concepts**
- `using` declaration disposes at the end of the enclosing code block (method, `if` body, etc.)
- `using` statement `using (var x = ...) { }` disposes at the closing brace
- extended disposal lifetime: resource held longer than intended
- nested using declarations: disposed in reverse order at the end of scope

**Answer**

`using var stream = File.OpenRead(path);` disposes `stream` at the end of the enclosing block (typically the method), not at the next closing brace. If the enclosing method is long and the resource (a database connection, file handle, or network socket) should be released as soon as it is no longer needed, the `using` declaration holds it open for the entire remaining method body. The traditional `using (var conn = GetConnection()) { /* just this block */ }` releases the resource as soon as the block closes. The `using` declaration is appropriate for short methods where the resource is needed throughout; the `using` statement is better when you want precise control over the disposal point.

---

#### Gotcha 7. Indices and ranges — `^1` is last element; `[1..^1]` excludes first and last; ranges are not available on `IEnumerable<T>` without ToArray

**Concepts**
- `^n` is "from the end" index: `^1` is last, `^0` is one past end (invalid as element index)
- `[start..end]` is exclusive on the end: `[0..3]` includes indices 0, 1, 2
- `Range` and `Index` types require `Length` property; not supported on `IEnumerable<T>`
- `string` and `Span<T>` support slicing natively

**Answer**

`array[^1]` returns the last element (`array[array.Length - 1]`). `array[1..^1]` slices from index 1 up to but not including the last element. `^0` is the length itself — using it as an element index throws `IndexOutOfRangeException`. The range syntax requires the collection to expose an `int Length` (or `Count`) property and a `Slice` method or direct indexer; it is not available on `IEnumerable<T>` without first calling `.ToArray()` or `.ToList()`. A common mistake is writing `collection[1..^1]` on a LINQ sequence and getting a compile error; the fix is to materialize the sequence first. `string` and `Span<T>` support ranges natively without materialization.

---

#### Gotcha 8. `static` local functions — must capture nothing; use `static` to prevent accidental captures

**Concepts**
- `static` local function cannot reference variables from the enclosing method (no closure)
- compiler error if a `static` local function captures an outer variable
- use `static` to document intent and prevent accidental performance-degrading captures
- captures allocate a closure object on the heap on first invocation

**Answer**

A non-static local function creates a closure that captures variables from the enclosing scope, which typically allocates a heap object on first invocation. In performance-sensitive code, this can be an unintended allocation. Marking the local function `static` prevents it from capturing anything — the compiler enforces this with an error if you accidentally reference an outer variable. This is useful both as documentation ("this helper has no side effects on the enclosing scope") and as a correctness guard. If the `static` local function needs data from the caller, it must receive that data as explicit parameters. The `static` modifier also enables the JIT to inline the function more aggressively since there is no closure to dereference.

---

#### Gotcha 9. `ReadOnlySpan<char>` from string — `str.AsSpan(start, length)` is zero-copy; cannot be stored in a class field

**Concepts**
- `ReadOnlySpan<char>` is a `ref struct` — stack-only
- `AsSpan()` returns a span over the string's internal buffer — zero allocation
- cannot store `Span<T>` or `ReadOnlySpan<T>` as a class field or in an async method across `await`
- `Memory<T>` or `ReadOnlyMemory<T>` for heap-storable slice references

**Answer**

`str.AsSpan(5, 10)` returns a `ReadOnlySpan<char>` that directly references the string's internal character buffer with no allocation. This is 2-5x faster than `str.Substring(5, 10)` for parsing scenarios because `Substring` allocates a new string. However, `ReadOnlySpan<char>` is a `ref struct` and cannot be stored in a class field, a lambda capture, or across an `await` boundary. Attempting any of these produces a compile error. When you need to store a slice for later use (e.g., in a class that caches parsed segments), use `ReadOnlyMemory<char>` instead — it is a regular struct that holds a heap reference and can be stored anywhere `Memory<T>` is a storable, non-stack-constrained type.

---

#### Gotcha 10. Pattern matching enhancements — positional patterns and property patterns; `{ Length: > 0 }` on string

**Concepts**
- property pattern `{ Prop: value }` matches if the property equals the value
- relational pattern `{ Count: > 0 }` uses comparison operators in a pattern
- positional pattern `(x, y)` deconstructs via `Deconstruct` method
- `and`, `or`, `not` combinators for compound patterns

**Answer**

C# 8 property patterns allow inline inspection of object properties in `switch` expressions and `is` checks: `order is { Status: OrderStatus.Paid, Items.Count: > 0 }` checks both the `Status` property and the count of `Items` in a single expression. Relational patterns (`{ Price: > 100 }`) combine type checking with value comparison without a separate `when` guard. Positional patterns (`(_, var y)`) deconstruct a tuple or any type with `Deconstruct` directly in the pattern. A common mistake is writing `obj is { Length: 0 }` on a `string` and being surprised that it matches an empty string — the pattern checks the `Length` property, not reference equality, which is the correct and intended behavior. Combining patterns with `and`/`or`/`not` enables concise null and range guards: `x is not null and { Count: >= 1 }`.

---

## Real-World Scenarios

---

## Q15. (Code Review) After enabling NRT, a document service still throws NullReferenceException on Notes. Review the service.

```csharp
public sealed class DocumentIngestService
{
    public string BuildSummary(DocumentMetadata metadata)
    {
        string header = metadata.Id!.Trim();
        string note = metadata.Notes.ToUpperInvariant();
        return $"{header}: {note}";
    }

    public DocumentMetadata LoadFromJson(string json)
    {
        var doc = JsonSerializer.Deserialize<DocumentMetadata>(json);
        doc!.Id = doc.Id ?? "UNKNOWN";
        return doc;
    }
}

public sealed class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
```

**Concepts**
- Notes is string? — may be null
- ! on Id suppresses warning without null check
- Deserialize returns null for null JSON token
- doc! after Deserialize hides null-document case
- correct fix: null-conditional and ?? operator

| Category | Problem | Impact |
|---|---|---|
| Correctness | `metadata.Notes.ToUpperInvariant()` — Notes is `string?` and can be null | NullReferenceException on any document without footnotes |
| Correctness | `metadata.Id!.Trim()` — suppresses warning but Id can be null if Deserialize produces one | Latent NRE if Id is not set |
| Correctness | `doc!` after Deserialize — Deserialize returns null for a null JSON token; `!` masks the null | NRE on the next line for a null document payload |

**Fix priority:**
1. Fix `Notes` handling: `string note = metadata.Notes?.ToUpperInvariant() ?? string.Empty;`
2. Fix `LoadFromJson`: `var doc = JsonSerializer.Deserialize<DocumentMetadata>(json) ?? throw new ArgumentException("Invalid JSON");`
3. Remove `!` from `metadata.Id!` and use `metadata.Id?.Trim() ?? string.Empty` or require Id to be non-null via a constructor parameter.
4. Enable `<Nullable>enable</Nullable>` project-wide and treat zero `!` usages as a quality gate.

---

## Q16. (Code Review) An async stream worker OOMs and ignores cancellation on deploy. Review the consumer and producer.

```csharp
public async Task ArchivePagesAsync(CancellationToken stoppingToken)
{
    var allPages = _documentStream.ReadPagesAsync()
        .ToListAsync(stoppingToken)
        .GetAwaiter()
        .GetResult();

    foreach (string page in allPages)
    {
        await _blobWriter.UploadPageAsync(page);
    }
}

public async IAsyncEnumerable<string> ReadPagesAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    foreach (string page in _pages)
    {
        await Task.Delay(200, cancellationToken);
        yield return page;
    }
}
```

**Concepts**
- ToListAsync + GetAwaiter().GetResult() materializes entire stream synchronously
- GetResult() blocks thread and ignores cooperative cancellation
- all pages loaded into memory before upload starts
- correct pattern: await foreach with WithCancellation
- upload cancellation token not threaded through UploadPageAsync

| Category | Problem | Impact |
|---|---|---|
| Memory | `ToListAsync` materializes all pages into memory before processing | OOM on large archives |
| Cancellation | `GetAwaiter().GetResult()` blocks and does not respond to stoppingToken | Worker cannot be stopped by deploy cancellation |
| Performance | No overlap between reading and uploading | Slow: all reads complete before any uploads start |
| Correctness | stoppingToken not passed to `UploadPageAsync` | Upload continues after cancellation is requested |

**Fix priority:**
1. Replace `ToListAsync` + `GetResult` with `await foreach (var page in _documentStream.ReadPagesAsync().WithCancellation(stoppingToken))`.
2. Pass `stoppingToken` to `_blobWriter.UploadPageAsync(page, stoppingToken)`.
3. Consider pipelining: use a `Channel<string>` to overlap reading pages with uploading, improving throughput.

---

## Q17. (Design) A 400-project solution enables NRT repo-wide. What migration strategy avoids the 8,000-warning chokepoint?

**Concepts**
- incremental enablement per project
- #nullable enable at file level before project level
- [NotNullWhen] and [MemberNotNull] for contract annotations
- ! suppressions as temporary markers with TODO comments
- nullable migration tracking via warning count metric

**Answer**

A "flip repo-wide in one PR" strategy is unworkable: 8,000 warnings generate enormous PR noise and incentivize mass `!` suppression or `#nullable disable` which permanently defeats the feature. The recommended strategy is incremental from the inside out. Start with leaf projects — domain models, DTOs, value objects — that have the fewest dependencies and the most stable schemas. Enable `<Nullable>enable</Nullable>` on one project at a time, fix the warnings properly (not with `!`), and merge before moving to the next. For the heavy `string` metadata types (IDs, paths, optional notes), audit which properties are genuinely optional (`string?`) versus invariantly non-null (`string`). Add `[NotNullWhen(true)]` to TryGet-style methods so callers get nullability flow after the boolean check. Add `[MemberNotNull(nameof(Id))]` to initializer methods so the compiler knows which fields they set. Reserve `!` for rare cases where proof exists but the compiler cannot track it, and annotate each with a `// NRT: guaranteed by factory contract` comment to distinguish intentional from lazy suppressions. Track progress as a CI metric: the warning count should decrease monotonically; any PR that increases it must justify the increase in code review.
