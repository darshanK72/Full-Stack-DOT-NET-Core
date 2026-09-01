# C# 8 Features — Interview Q&A

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

## Gotchas

---

## Q12. Why does using ! to silence NRT warnings on nullable properties cause NullReferenceException at runtime?

**Concepts**
- ! operator is compile-time only
- suppressing warning does not add null check
- Notes.ToUpperInvariant() throws when Notes is null
- correct fix: null check or null-conditional operator
- NRT migration discipline

**Answer**

`metadata.Notes!.ToUpperInvariant()` tells the compiler "I guarantee Notes is not null here" — the `!` operator emits no IL instruction and performs no runtime check. If `Notes` is actually null (a document with no footnotes, for example), the call throws `NullReferenceException` at the `.ToUpperInvariant()` call site. The developer who added `!` silenced the compiler warning without addressing the underlying nullability. The correct fix depends on intent: if `Notes` is optional and a missing note should produce an empty string, use `metadata.Notes?.ToUpperInvariant() ?? string.Empty`; if a null note is an invariant violation that should never happen, add an actual null guard that throws a descriptive exception rather than masking with `!`. The `!` operator should be reserved for cases where you can prove non-nullability through logic the compiler cannot analyze — not as a way to merge PRs faster.

---

## Q13. What is the C# 8 async stream anti-pattern of calling ToListAsync followed by synchronous foreach?

**Concepts**
- ToListAsync materializes entire stream into memory
- eliminates streaming benefit
- blocking GetAwaiter().GetResult() prevents cancellation
- correct pattern: await foreach with cancellation token
- OOM risk for large streams

**Answer**

Calling `await source.ToListAsync(ct)` (or `.GetAwaiter().GetResult()`) and then iterating the resulting list synchronously defeats the memory and cancellation benefits of `IAsyncEnumerable<T>`. The entire result set is held in memory before any processing begins — for a millions-of-rows document archive this causes an OutOfMemoryException. The `.GetAwaiter().GetResult()` form is synchronous blocking and ignores the cancellation token's cooperative cancellation semantics, meaning a deploy cancellation event does not stop the stream from being fully materialized. The correct pattern is `await foreach (var page in producer.ReadPagesAsync().WithCancellation(stoppingToken))` followed by processing of each page individually inside the loop. This way, only one page is in memory at a time and cancellation interrupts the enumeration at the next yield boundary. The producer must also accept and use the cancellation token internally.

---

## Q14. What are the C# 8 rules that prevent ref struct and Span<T> from crossing async await boundaries?

**Concepts**
- ref struct cannot be stored in heap field
- async state machine lifts locals to heap fields
- Span<T> is a ref struct
- ReadOnlySpan<char> cannot be used across await
- fix: complete span work before await or use string/Memory<T>

**Answer**

`ref struct` types (including `Span<T>` and `ReadOnlySpan<char>`) are constrained to live only on the stack. The C# compiler transforms an `async` method into a state machine class (heap-allocated) that stores all locals in its fields so they survive suspension at `await` points. Because a `ref struct` cannot be stored in a heap field, it cannot be a local variable in an `async` method that is live across an `await` expression — the compiler rejects this with an error. In practice, this means you cannot declare a `ReadOnlySpan<char>` before an `await` and use it after. The resolution is to complete all span-based computation before the first `await` in the method, extract the needed result into a `string`, `Memory<T>`, or other heap-safe type, and then pass that type across the `await`. Alternatively, decompose the method into a synchronous span-processing helper and an async I/O method that calls it, keeping the ref struct entirely inside the synchronous scope.

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
