# 02. C# Language Fundamentals

C# basics, OOP, generics, collections, functional programming, LINQ, async, file I/O, advanced features, and unit testing.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | 01. C# Basics - Done | 256 | [README.md](./01.%20C#%20Basics%20-%20Done/README.md) |
| 02 | 02. Object Oriented Programming | 216 | [README.md](./02.%20Object%20Oriented%20Programming/README.md) |
| 03 | 03. Generics & Collections | 164 | [README.md](./03.%20Generics%20%26%20Collections/README.md) |
| 04 | 04. Functional Style Programming | 113 | [README.md](./04.%20Functional%20Style%20Programming/README.md) |
| 05 | 05. Language Integrated Query | 250 | [README.md](./05.%20Language%20Integrated%20Query/README.md) |
| 06 | 06. Multithreading & Async Programming | 173 | [README.md](./06.%20Multithreading%20%26%20Async%20Programming/README.md) |
| 07 | 07. File Input & Outpout and Streams | 128 | [README.md](./07.%20File%20Input%20%26%20Outpout%20and%20Streams/README.md) |
| 08 | 08. Advanced C# Features | 187 | [README.md](./08.%20Advanced%20C#%20Features/README.md) |
| 09 | 09. Unit Testing | 100 | [README.md](./09.%20Unit%20Testing/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

# 02. C# Language Fundamentals — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [C# Basics](01.%20C%23%20Basics%20-%20Done/README.md) | Variables, types, operators, control flow, and exception fundamentals |
| 02 | [Object Oriented Programming](02.%20Object%20Oriented%20Programming/README.md) | Classes, inheritance, interfaces, polymorphism, and encapsulation |
| 03 | [Generics & Collections](03.%20Generics%20%26%20Collections/README.md) | Generic types, variance, List, Dictionary, HashSet, and typed collections |
| 04 | [Functional Style Programming](04.%20Functional%20Style%20Programming/README.md) | Delegates, lambdas, Func/Action, closures, and expression trees |
| 05 | [Language Integrated Query](05.%20Language%20Integrated%20Query/README.md) | LINQ syntax, deferred execution, query operators, and EF Core integration |
| 06 | [Multithreading & Async Programming](06.%20Multithreading%20%26%20Async%20Programming/README.md) | Task, async/await, parallelism, synchronization primitives, and cancellation |
| 07 | [File Input & Output and Streams](07.%20File%20Input%20%26%20Outpout%20and%20Streams/README.md) | FileStream, StreamReader/Writer, async file I/O, and compression |
| 08 | [Advanced C# Features](08.%20Advanced%20C%23%20Features/README.md) | Records, pattern matching, nullable references, spans, and source generators |
| 09 | [Unit Testing](09.%20Unit%20Testing/README.md) | xUnit, NUnit, Moq, test organization, and async test patterns |

---

## Table of Contents

- [CQ1. Why can `IEnumerable<Derived>` substitute `IEnumerable<Base>` but `List<Derived>` cannot substitute `List<Base>`?](#cq1-why-can-ienumerablederived-substitute-ienumerablebase-but-listderived-cannot-substitute-listbase)
- [CQ2. What breaks when you mix LINQ's deferred execution with async/await, and how do you fix it?](#cq2-what-breaks-when-you-mix-linqs-deferred-execution-with-asyncawait-and-how-do-you-fix-it)
- [CQ3. Why does try/catch around Task.WhenAll miss exceptions, and how does async void make it worse?](#cq3-why-does-trycatch-around-taskwhenall-miss-exceptions-and-how-does-async-void-make-it-worse)
- [CQ4. What happens when you use a record with a mutable property as a Dictionary key or HashSet element?](#cq4-what-happens-when-you-use-a-record-with-a-mutable-property-as-a-dictionary-key-or-hashset-element)
- [CQ5. How do generic constraints combine with Func<T> and LINQ operators to enforce compile-time type safety?](#cq5-how-do-generic-constraints-combine-with-funct-and-linq-operators-to-enforce-compile-time-type-safety)
- [CQ6. How do you correctly write and test an async method and an IAsyncEnumerable<T> producer in xUnit or NUnit?](#cq6-how-do-you-correctly-write-and-test-an-async-method-and-iasyncenumerablet-producer-in-xunit-or-nunit)
- [CQ7. Why should value types that serve as collection keys or sorted elements implement `IEquatable<T>` and `IComparable<T>` explicitly?](#cq7-why-should-value-types-that-serve-as-collection-keys-or-sorted-elements-implement-iequatablet-and-icomparablet-explicitly)
- [CQ8. How do generic delegates like `Action<T>` and `Func<T, TResult>` avoid boxing compared to non-generic delegate types?](#cq8-how-do-generic-delegates-like-actiont-and-funct-tresult-avoid-boxing-compared-to-non-generic-delegate-types)
- [CQ9. What pitfalls arise when performing file I/O inside async methods, and how does sync-over-async cause deadlocks?](#cq9-what-pitfalls-arise-when-performing-file-io-inside-async-methods-and-how-does-sync-over-async-cause-deadlocks)
- [CQ10. Why can `Span<T>` not survive an `await` boundary, and how does `Memory<T>` address that for async file I/O?](#cq10-why-can-spant-not-survive-an-await-boundary-and-how-does-memoryt-address-that-for-async-file-io)
- [CQ11. When should you prefer a `switch` expression with type patterns over virtual dispatch, and when is polymorphism still better?](#cq11-when-should-you-prefer-a-switch-expression-with-type-patterns-over-virtual-dispatch-and-when-is-polymorphism-still-better)
- [CQ12. What is the trap of calling `.Select()` on an `IAsyncEnumerable<T>`, and which LINQ operators genuinely work with async sequences?](#cq12-what-is-the-trap-of-calling-select-on-an-iasyncenumerablet-and-which-linq-operators-genuinely-work-with-async-sequences)
- [CQ13. How do records interact with inheritance in C# 13, and when should you use a `readonly struct` instead?](#cq13-how-do-records-interact-with-inheritance-in-c-13-and-when-should-you-use-a-readonly-struct-instead)
- [CQ14. How does reflection enable calling a generic method when the type argument is only known at runtime?](#cq14-how-does-reflection-enable-calling-a-generic-method-when-the-type-argument-is-only-known-at-runtime)
- [CQ15. How do nullable reference type annotations behave inside generic type parameters, and why does `T?` mean different things depending on the constraint?](#cq15-how-do-nullable-reference-type-annotations-behave-inside-generic-type-parameters-and-why-does-t-mean-different-things-depending-on-the-constraint)
- [CQ16. How does designing to interfaces enable unit testing with Moq, and why is mocking concrete classes a pitfall?](#cq16-how-does-designing-to-interfaces-enable-unit-testing-with-moq-and-why-is-mocking-concrete-classes-a-pitfall)
- [CQ17. What is the loop-variable capture gotcha with closures, and how does it interact with async lambdas and `CancellationToken`?](#cq17-what-is-the-loop-variable-capture-gotcha-with-closures-and-how-does-it-interact-with-async-lambdas-and-cancellationtoken)
- [CQ18. How does LINQ's deferred execution interact with a `List<T>` that is modified between query creation and enumeration?](#cq18-how-does-linqs-deferred-execution-interact-with-a-listt-that-is-modified-between-query-creation-and-enumeration)

---

## CQ1. Why can `IEnumerable<Derived>` substitute `IEnumerable<Base>` but `List<Derived>` cannot substitute `List<Base>`?

**Concepts**
- Covariance (`out T`) declared on `IEnumerable<T>` (Generics & Collections, OOP interfaces)
- Invariance of `List<T>` due to both read and write members (Generics & Collections)
- Liskov Substitution Principle and static type safety (OOP)
- How covariance enables polymorphic LINQ pipelines (Generics, LINQ)

**Answer**

`IEnumerable<T>` is declared with the `out` covariance annotation (`IEnumerable<out T>`) in .NET, which tells the compiler that `T` only ever flows out of the interface — you can enumerate items but never write them back. Because no mutation is possible through the interface, assigning an `IEnumerable<Dog>` to an `IEnumerable<Animal>` variable is provably safe: every `Dog` is an `Animal`, so reading an element as `Animal` can never yield an invalid object.

`List<T>` is invariant because it exposes both reads (`T this[int index] { get; }`) and writes (`void Add(T item)`). If the compiler allowed `List<Dog>` to be treated as `List<Animal>`, you could call `list.Add(new Cat())` through the `List<Animal>` reference, silently inserting a `Cat` into what is physically a `List<Dog>`. That corrupts the collection's type contract at runtime, so the compiler forbids the assignment entirely.

The practical payoff surfaces in LINQ: because every concrete collection — `List<Dog>`, `Dog[]`, `HashSet<Dog>` — implicitly converts to `IEnumerable<Animal>` when `Dog : Animal`, a single LINQ method accepting `IEnumerable<Animal>` handles any homogeneous collection of subtypes without an explicit cast or copy. You can also cross from invariant to covariant form explicitly with `.Cast<Animal>()` or `.OfType<Animal>()`, but with covariant interfaces the conversion is implicit and allocation-free. Understanding why `List<T>` must stay invariant explains one of the most common C# compile errors beginners encounter and prevents subtle collection-corruption bugs.

---

## CQ2. What breaks when you mix LINQ's deferred execution with `async`/`await`, and how do you fix it?

**Concepts**
- LINQ deferred execution — query is declared, not run, at assignment (LINQ)
- `async` lambdas inside `Select`/`Where` produce `Task<T>`, not `T` (LINQ, Async)
- `Task.WhenAll` as a bounded-batch workaround (Async)
- `IAsyncEnumerable<T>` and `await foreach` for true async streaming (Async, Generics & Collections)

**Answer**

LINQ operators like `Select` and `Where` are synchronous higher-order functions that expect a `Func<T, TResult>` returning a plain value. When you pass an `async` lambda, the compiler infers `Func<T, Task<TResult>>`, so `Select` materialises a sequence of hot `Task<TResult>` objects rather than a sequence of unwrapped results. The tasks are started lazily — only when the outer `foreach` or `.ToList()` pulls them — and nothing inside the LINQ pipeline awaits them. If you forget to await each element individually, the resulting `IEnumerable<Task<TResult>>` silently drops exceptions and produces default values, yielding incorrect output with no compile-time warning.

The conventional fix for a bounded, in-memory batch is `await Task.WhenAll(source.Select(async x => await FetchAsync(x)))`, which starts all tasks concurrently and awaits the entire group at once. However, this buffers every result in memory before any can be consumed, and a single faulted task causes `WhenAll` to throw while the others finish unobserved.

For true streaming with back-pressure, .NET provides `IAsyncEnumerable<T>`. A producer marks itself `async IAsyncEnumerable<T>` and uses `yield return` inside async code; the consumer uses `await foreach` which pulls one item at a time. The `System.Linq.Async` NuGet package adds `WhereAwait`, `SelectAwait`, and `ToListAsync` — async-aware LINQ operators over `IAsyncEnumerable<T>` — restoring the familiar pipeline style without the deferred-execution trap.

---

## CQ3. Why does `try`/`catch` around `Task.WhenAll` miss exceptions, and how does `async void` make it worse?

**Concepts**
- `AggregateException` wrapping multiple faulted tasks (Async)
- `await` unwrapping only the first inner exception from `AggregateException` (Async)
- `async void` fire-and-forget with no observable `Task` (Async, C# Basics exception model)
- `TaskScheduler.UnobservedTaskException` as a process-level trap (Async)

**Answer**

`Task.WhenAll` returns a single `Task` that faults when any constituent task faults, wrapping all failures in an `AggregateException`. When you `await` that combined task inside a `try`/`catch`, the C# awaiter machinery unwraps the `AggregateException` and re-throws only the first `InnerException`. Every subsequent failure from other faulted tasks is silently discarded by the `await` unwrapping step. The `catch` block therefore receives exactly one exception regardless of how many tasks failed, and the others leave no trace unless you capture the `Task` reference before awaiting it and inspect `.Exception.InnerExceptions` directly in the `catch` block.

`async void` compounds the problem at a more fundamental level. An `async void` method cannot be awaited; the returned value is simply discarded. When an exception escapes an `async void` body, the runtime has no `Task` to attach it to, so it re-raises the exception directly on the captured `SynchronizationContext` — the thread-pool in console and ASP.NET Core apps. In .NET 10, an unhandled thread-pool exception triggers `TaskScheduler.UnobservedTaskException` and can terminate the process. There is no `try`/`catch` at the call site that can intercept it, because the caller completed before the async work threw. For this reason `async void` is reserved exclusively for event-handler signatures mandated by framework contracts; every other async method must return `Task` or `Task<T>` so exceptions are observable and awaitable.

---

## CQ4. What happens when you use a `record` with a mutable property as a `Dictionary` key or `HashSet` element?

**Concepts**
- Compiler-generated `Equals`/`GetHashCode` on records uses all properties (Advanced C# Features)
- Hash-based collections store items in buckets chosen by the hash code at insertion time (Generics & Collections)
- Hash code stability requirement for collection correctness (Generics & Collections)
- `init`-only setters vs. mutable `set` on record properties (Advanced C# Features)

**Answer**

C# positional records automatically synthesise `Equals` and `GetHashCode` from all declared properties. This makes records attractive as dictionary keys or set elements because value equality is provided without any boilerplate — two records with identical property values are considered equal, unlike classes which default to reference identity. The feature works reliably as long as the record is immutable.

The trap is mutability. Hash-based collections — `Dictionary<TKey, TValue>` and `HashSet<T>` — compute the hash code at insertion time and store the object in the corresponding bucket. The fundamental contract for any key type is that `GetHashCode()` must return the same value for the lifetime of the object while it lives in the collection. If you define a record with a regular mutable `set` accessor and then mutate that property after insertion, the record's hash code changes, but the object still occupies the old bucket. A subsequent lookup by the new value searches the new bucket and finds nothing; a removal by the new key silently fails; `ContainsKey` returns `false` for an item that is physically present. No exception is thrown — the collection is silently corrupted.

Records declared with `init`-only property setters (`public string Name { get; init; }`) are safe because properties cannot be changed after object construction, keeping the hash code stable for the object's lifetime. The compiler does not prevent you from adding a mutable `set` to a record property, so the defence is a deliberate design rule: only use records with all-`init` or read-only properties as keys in hash-based collections, or implement `IEquatable<T>` on a class backed by deliberately immutable key fields.

---

## CQ5. How do generic constraints combine with `Func<T>` and LINQ operators to enforce compile-time type safety?

**Concepts**
- Generic type constraints (`where T : IComparable<T>`, `where T : notnull`) (Generics & Collections)
- Contravariance of `Func<in T, out TResult>` delegate input parameter (Functional Style, Generics)
- LINQ operators as generic extension methods on `IEnumerable<T>` (LINQ, Generics)
- Compile-time type inference across chained pipeline stages (Generics, LINQ)

**Answer**

Generic constraints let you declare at the method signature level which capabilities `T` must provide, so the compiler can verify correctness without boxing to `object` or deferring errors to runtime. For example, `static T Max<T>(IEnumerable<T> source) where T : IComparable<T>` guarantees that `CompareTo` is available on every element; the compiler rejects any call where `T` does not satisfy the constraint rather than failing with a `MissingMethodException` at runtime. Adding `where T : notnull` in .NET 10 similarly suppresses nullable-reference warnings for generic containers that must not store null.

LINQ is built entirely on this principle. `OrderBy<TSource, TKey>` relies internally on `Comparer<TKey>.Default`, which works only because `TKey` must support comparison; `GroupBy` and `ToDictionary` rely on `EqualityComparer<TKey>.Default`. Because all operators are generic extension methods on `IEnumerable<T>`, the C# type-inference engine propagates the inferred type through the entire chain: `numbers.Where(n => n > 0).Select(n => n * 2.0)` infers `IEnumerable<double>` for the final result without a single explicit annotation.

`Func<T, TResult>` introduces variance into the picture. `Func` is contravariant in `T` (the input) and covariant in `TResult` (the output). A `Func<Animal, string>` can be stored in a `Func<Dog, string>` variable — a predicate that handles all animals handles dogs too, so the assignment is safe. Combining constraints with `Func` parameters lets library authors write methods that accept arbitrary predicates or projections while retaining full static type information at every stage, which is why a twenty-step LINQ pipeline never loses its element type even when mixing `Select`, `Where`, `GroupBy`, and custom extension methods.

---

## CQ6. How do you correctly write and test an `async` method and an `IAsyncEnumerable<T>` producer in xUnit or NUnit?

**Concepts**
- `async Task` test methods in xUnit/NUnit vs. `async void` tests (Unit Testing, Async)
- `async void` tests silently passing even when the body throws (Unit Testing, Async)
- `ConfigureAwait(false)` behaviour in test runners vs. UI contexts (Async, Unit Testing)
- Consuming `IAsyncEnumerable<T>` inside a test with `await foreach` or `ToListAsync` (Async, Generics, Unit Testing)

**Answer**

Both xUnit and NUnit support `async Task` test methods natively. The test runner calls the method, receives the returned `Task`, and awaits it on its own scheduler before recording the outcome. This means an exception thrown inside the async body propagates correctly through the `Task` and is recorded as a test failure. The critical rule is that the test method must return `Task` — never `async void`. An `async void` test appears to pass even when it throws, because the test framework receives no `Task` to await; the synchronous portion of the method completes, the runner records a green result, and the exception surfaces later on the thread-pool, completely outside the test's observation window.

`ConfigureAwait(false)` has a subtler role in test contexts. In library code it prevents deadlocks by not capturing the caller's `SynchronizationContext` for continuations. Most xUnit and NUnit runner contexts have a null or custom context, so omitting `ConfigureAwait(false)` is generally harmless in tests. Tests that assert against UI-thread state in WinForms or WPF must preserve the context, but for the vast majority of service and domain logic tests, either form is safe. Consistency with the production code under test is a reasonable tiebreaker.

Testing an `IAsyncEnumerable<T>` producer requires explicitly consuming the stream. The idiomatic pattern is to accumulate results with `await foreach` into a `List<T>` and then assert on the list, or to use `System.Linq.Async`'s `ToListAsync()` extension for a one-liner. Cancellation paths are tested by passing a pre-cancelled `CancellationToken` to the producer and asserting that an `OperationCanceledException` is thrown; xUnit's `Assert.ThrowsAsync<OperationCanceledException>` or NUnit's `Assert.ThrowsAsync` await the consuming wrapper correctly, as long as the test method itself is `async Task`.

---

## CQ7. Why should value types that serve as collection keys or sorted elements implement `IEquatable<T>` and `IComparable<T>` explicitly?

**Concepts**
- Default `object.Equals` and `GetHashCode` on structs using reflection-based field comparison (OOP)
- `IEquatable<T>` bypassing virtual dispatch and boxing on value types (Generics & Collections, OOP)
- `EqualityComparer<T>.Default` resolving to `IEquatable<T>` when available, avoiding the boxing fallback (Generics & Collections)
- `IComparable<T>` required by `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, and `Comparer<T>.Default` (Generics & Collections)

**Answer**

The default `object.Equals` and `GetHashCode` implementations on user-defined `struct` types use reflection internally to compare or hash every field. `EqualityComparer<T>.Default` checks at construction time whether `T` implements `IEquatable<T>`, and if it does, it calls `T.Equals(T other)` directly — a statically dispatched, non-boxing call. If `T` does not implement `IEquatable<T>`, the fallback path boxes both operands to `object` and invokes the reflective equality check. For a struct stored in a `Dictionary<T, V>` or `HashSet<T>`, every lookup, insertion, and removal triggers equality comparison, so the boxing cost accumulates in tight loops or high-throughput code.

Implementing `IComparable<T>` matters whenever the type participates in ordered collections or LINQ sort operators. `SortedSet<T>` and `SortedDictionary<TKey, TValue>` require their key type to implement `IComparable<T>` (or receive an explicit `IComparer<T>`); without it the constructor throws `InvalidOperationException` at runtime. `Array.Sort<T>` and `OrderBy` call `Comparer<T>.Default`, which resolves to `IComparable<T>` when available, falling back to the non-generic `IComparable` otherwise — a path that boxes value types on every comparison and can measurably slow a sort over even a modest array.

The practical rule: any `struct` that will be a dictionary key, a set element, or a sort key should implement both `IEquatable<T>` and `IComparable<T>`, and should also override `object.Equals(object?)` and `GetHashCode` for completeness. Annotating the struct `readonly` in C# 13 guarantees no mutation during comparison and prevents defensive copies that the compiler sometimes inserts when a non-readonly struct is passed to an interface method.

---

## CQ8. How do generic delegates like `Action<T>` and `Func<T, TResult>` avoid boxing compared to non-generic delegate types?

**Concepts**
- Non-generic `delegate void Process(object item)` requiring boxing for every value-type argument (Functional Style)
- Generic delegates instantiated per concrete `T` by the JIT; value-type `T` gets its own compiled body (Generics & Collections, Functional Style)
- JIT code sharing for reference-type instantiations; dedicated native code for each value-type `T` (Generics & Collections)
- `Comparison<T>` as a delegate-based alternative to `IComparer<T>` in `List<T>.Sort`, enabling JIT inlining (Generics & Collections, Functional Style)

**Answer**

Before generics, delegates had to accept `object` parameters to be type-agnostic, which forced value-type arguments to be boxed on every invocation. Passing an `int` to a `delegate void Process(object item)` allocates a heap object, performs the call, and the result must be unboxed before use — three operations for what should be a trivial function application. For high-frequency callbacks over value-type sequences, that allocation pressure is significant.

Generic delegates resolve this at the JIT level. When the runtime compiles `Action<int>`, it creates a specialised code path where the `int` argument flows as a 32-bit integer through the calling convention with no heap allocation. For reference-type instantiations — `Action<string>`, `Action<List<int>>` — the JIT shares a single compiled body because all references are the same pointer size; only value-type instantiations each get their own compiled body. This is why generic collections and LINQ operators can process `int[]` or `Span<int>` at near-native speed.

`Comparison<T>` (`delegate int Comparison<T>(T x, T y)`) is the canonical example of applying this principle to sorting. `List<T>.Sort(Comparison<T>)` avoids allocating an `IComparer<T>` heap object and avoids virtual dispatch through the interface; the JIT can inline the delegate body into the sort algorithm when the delegate wraps a static method or a simple lambda. The same advantage applies to `Func<T, TResult>` in LINQ — `Select<TSource, TResult>(Func<TSource, TResult> selector)` is generic in both source and result, so projecting from `int` to `double` never boxes either operand. The non-generic `ArrayList` and its `object`-based delegate callbacks predate this design and are avoided in all modern .NET 10 code for exactly this reason.

---

## CQ9. What pitfalls arise when performing file I/O inside async methods, and how does sync-over-async cause deadlocks?

**Concepts**
- `StreamReader.ReadToEndAsync(CancellationToken)` vs. synchronous `ReadToEnd` inside an async call chain (File I/O, Async)
- Sync-over-async antipattern: `.Result` or `.Wait()` on a file-reading `Task` from a single-threaded `SynchronizationContext` (Async, File I/O)
- `SynchronizationContext` capture and the deadlock mechanism when a continuation needs the blocked thread (Async)
- `CancellationToken` propagation from an HTTP request to the file operation to avoid wasted I/O bandwidth (Async, File I/O)

**Answer**

Async file I/O in .NET uses overlapped I/O on Windows or epoll on Linux, completing the operation on the OS thread-pool without blocking a managed thread. When you call `File.ReadAllTextAsync(path, token)` or `await reader.ReadToEndAsync(token)`, the managed thread is returned to the pool while the kernel services the request. The critical rule is that every step in the async call chain must also be asynchronous; inserting one synchronous file call defeats the purpose and, in certain hosting contexts, causes a deadlock.

The classic deadlock arises in any framework that installs a single-threaded `SynchronizationContext` — classic ASP.NET, WinForms, WPF. Calling `reader.ReadToEndAsync().Result` or `.Wait()` on that thread blocks the thread waiting for the `Task` to complete. The task's continuation is scheduled back to the captured `SynchronizationContext`, which requires the same thread that is now blocked. Neither side can proceed: the thread waits for the task, the task waits for the thread. ASP.NET Core and .NET 10 console apps use a thread-pool context without this constraint, so the deadlock does not reproduce there — which is exactly why it often surfaces only in production environments whose host differs from the developer's test runner.

`CancellationToken` should flow all the way from the caller to the file operation. In ASP.NET Core, `HttpContext.RequestAborted` is cancelled when the client disconnects; plumbing it to `ReadToEndAsync(cancellationToken)` aborts the disk read and releases the file handle rather than continuing to read a large file for a client that has gone away. Forgetting to propagate the token means the server wastes I/O bandwidth, holds file handles longer than necessary, and cannot shed load during peak traffic.

---

## CQ10. Why can `Span<T>` not survive an `await` boundary, and how does `Memory<T>` address that for async file I/O?

**Concepts**
- `Span<T>` as a `ref struct` restricted to the stack; cannot be captured in heap closures (Advanced C# Features)
- Async state-machine capturing local variables in a heap-allocated object across `await` suspension points (Async)
- `Memory<T>` as a heap-safe counterpart: ordinary managed struct, not a `ref struct` (Advanced C# Features, File I/O)
- `RandomAccess.ReadAsync(SafeFileHandle, Memory<byte>, long, CancellationToken)` for zero-copy async reads (File I/O, Async)

**Answer**

`Span<T>` is declared as a `ref struct`, which means the compiler enforces that every instance lives exclusively on the stack. This restriction exists so the runtime never needs to GC-trace a `Span<T>` — it cannot outlive the stack frame that created it, and the GC never moves it unexpectedly. The restriction becomes a hard compiler error the moment `Span<T>` meets `await`: when a method suspends at an `await` point, the current stack frame is dismantled and all local state is captured in a state-machine heap object so the continuation can resume later. Because `Span<T>` cannot be placed on the heap, the compiler refuses to let a `Span<T>` local variable cross an `await` boundary — you get a compile error rather than a subtle memory-safety bug.

`Memory<T>` solves this by being an ordinary managed struct (not a `ref struct`), backed by an array, a `MemoryManager<T>`, or a string segment. It can be captured in a state-machine closure, stored in fields, and passed across async method calls freely. The idiomatic pattern for async file I/O in .NET 10 is to allocate a `byte[]` buffer once, wrap it as `Memory<byte>`, and pass it to `RandomAccess.ReadAsync(handle, memory, fileOffset, token)`. The underlying implementation pins the memory and hands it to the OS for a single overlapped I/O operation with no intermediate copies.

When you need to process the buffer contents synchronously within a single stack frame — parsing, slicing, searching — you call `memory.Span` to obtain a `Span<T>` for that scope, do the work, and drop the span before the next `await`. This pattern gives you the ergonomics and zero-copy performance of span-based processing while keeping the buffer lifetime managed by the heap through `Memory<T>` across async suspension boundaries.

---

## CQ11. When should you prefer a `switch` expression with type patterns over virtual dispatch, and when is polymorphism still better?

**Concepts**
- `switch` expression with type pattern `is TypeName { Property: value }` for exhaustive type discrimination (Advanced C# Features)
- Virtual dispatch via `override`/`abstract` methods centralising behaviour per type (OOP)
- Open/closed principle: new-type extension favours virtual dispatch; new-operation extension favours switch (OOP, Advanced C# Features)
- Exhaustiveness checking and `SwitchExpressionException` on unmatched arms with sealed hierarchies (Advanced C# Features)

**Answer**

Type-pattern `switch` expressions and virtual dispatch solve the same problem from opposite directions, and the choice depends on which axis of change is more likely.

Virtual dispatch shines when you expect new subtypes to be added over time. Defining an `abstract Shape.Area()` method means a new `Pentagon` class only needs to provide its own `Area()` implementation; every existing call site that invokes `shape.Area()` works without modification. Adding a new operation across all subtypes is harder — you must add an abstract method to the base class and implement it everywhere.

Pattern-matching `switch` expressions flip this trade-off. A switch over all known `Shape` subtypes makes it easy to add new operations — a `Perimeter` calculation needs only one new switch expression — but adding a new `Pentagon` means updating every existing switch. The compiler helps when the hierarchy is `sealed`: a missing arm produces a warning, and forgetting a case causes a `SwitchExpressionException` at runtime rather than silently returning a wrong result. With C# 13 list patterns and property patterns, arms can be remarkably concise: `case Circle { Radius: var r } => Math.PI * r * r`.

The practical guidance: prefer virtual dispatch for domain entities in a growing model where new types arrive frequently (classic DDD aggregates and value objects), and prefer pattern-matching for discriminated-union-like data where the type set is fixed and operations are varied — for example, a result type that is `Success`, `ValidationError`, or `NotFound`. Records pair especially well with pattern-matching in this role. Mixing both is valid and common: virtual dispatch for the shared hot path, a switch expression for an outlier operation that has no business being on every type.

---

## CQ12. What is the trap of calling `.Select()` on an `IAsyncEnumerable<T>`, and which LINQ operators genuinely work with async sequences?

**Concepts**
- `IAsyncEnumerable<T>` not implementing `IEnumerable<T>`; `GetAsyncEnumerator` vs. `GetEnumerator` (Async, LINQ)
- Compiler falling back to `Enumerable.Select` when no async-aware overload is in scope, producing `IEnumerable<Task<T>>` (LINQ, Async)
- `System.Linq.Async` NuGet package providing `SelectAwait`, `WhereAwait`, `OrderByAwait`, and `ToListAsync` (LINQ, Async)
- `await foreach` as the only standard-library consumption mechanism for `IAsyncEnumerable<T>` (Async)

**Answer**

`IAsyncEnumerable<T>` does not inherit from `IEnumerable<T>`. It has its own `GetAsyncEnumerator` method and requires `await foreach` to consume it. When you write `asyncStream.Select(x => Transform(x))` without importing `System.Linq.Async`, the C# compiler looks for an extension method named `Select` on `IAsyncEnumerable<T>` and finds none in the standard library. It then considers `Enumerable.Select`, which accepts any type for its first argument — technically not a match for `IAsyncEnumerable<T>` as a sequence, but if the compiler can satisfy the overload by treating the async-enumerable as a single-element `IEnumerable<IAsyncEnumerable<T>>`, the result is a meaningless `IEnumerable<TResult>` that projects the stream object as one item rather than its elements. Even in scenarios where the call resolves with an `async` lambda, `Enumerable.Select` returns `IEnumerable<Task<TResult>>` — a synchronous sequence of hot tasks, none of which are awaited by the pipeline. The resulting code may compile without warnings and produce silently wrong output.

`System.Linq.Async` (part of the Rx.NET ecosystem, fully supported on .NET 10) adds async-aware operators: `SelectAwait(async x => ...)`, `WhereAwait(async x => ...)`, `OrderByAwait`, and `ToListAsync()`. These return `IAsyncEnumerable<TResult>` and honour `await foreach` semantics throughout the chain. For simple synchronous projections, the package also provides `Select` returning `IAsyncEnumerable<TResult>`.

The rule: never apply `System.Linq.Enumerable` operators directly to `IAsyncEnumerable<T>`. Either add `System.Linq.Async` and use the `*Await` variants, or accumulate with `await foreach` into a `List<T>` and apply synchronous LINQ on the materialised list.

---

## CQ13. How do records interact with inheritance in C# 13, and when should you use a `readonly struct` instead?

**Concepts**
- Compiler-synthesised `EqualityContract` property ensuring polymorphic equality correctness in record hierarchies (Advanced C# Features, OOP)
- Virtual `Clone()` method making `with` expressions preserve the derived runtime type (Advanced C# Features, OOP)
- Primary constructors (C# 12) on classes vs. positional record constructors: property synthesis vs. parameter capture only (Advanced C# Features)
- `readonly struct` for guaranteed stack allocation; `record struct` for value-semantic copy-with (Advanced C# Features)

**Answer**

Records support single-inheritance chains: a `record` can extend another `record`, but not a plain `class`, and vice versa. The compiler synthesises a protected `EqualityContract` property on every record type — derived records override it to return their own `Type`. Equality comparisons between a base-record variable and a derived-record instance return `false` even when all shared properties match, because the `EqualityContract` check ensures a `Point` and a `Point3D` with the same `X` and `Y` are never considered equal. This is one of the subtler differences from class equality and is important when records are stored in sets or used as dictionary keys.

The `with` expression creates a copy via the virtual `Clone()` method, which derived records override. `with` on a derived record stored in a base-record variable still produces the correct derived type at runtime, though the compile-time type of the expression is the declared variable type. Primary constructors in C# 12 brought compact constructor syntax to plain classes and structs, but they differ from positional record constructors: a primary constructor on a `class` captures parameters as private fields; it does not synthesise public `init` properties. A positional record synthesises both the constructor and the corresponding public `init` properties in one declaration, along with `Equals`, `GetHashCode`, `Deconstruct`, and `ToString`.

`readonly struct` is the right choice when you need guaranteed stack allocation — for use with `Span<T>` APIs, `stackalloc` patterns, or zero-GC-pressure value objects like `Vector2` or `Color`. A `record struct` adds value-semantic equality and the `with` expression to a struct while remaining a value type that copies on assignment. Choose `record struct` when the struct is conceptually an immutable data bundle that needs non-destructive mutation syntax; choose `readonly struct` when the priority is performance, AOT friendliness, and ensuring every access path sees consistent state.

---

## CQ14. How does reflection enable calling a generic method when the type argument is only known at runtime?

**Concepts**
- `MethodInfo.MakeGenericMethod(Type[])` constructing a closed generic method from an open `MethodInfo` (Advanced C# Features, Generics)
- `typeof(List<>)` as an open generic type; `Type.MakeGenericType(Type[])` producing a closed type (Generics, Advanced C# Features)
- Performance cost of `MethodInfo.Invoke` vs. direct calls; boxing of value-type parameters (Advanced C# Features)
- Compiled-expression delegates and source generators as AOT-safe alternatives to runtime reflection (Functional Style, Advanced C# Features)

**Answer**

Generic methods are compiled with type parameters as placeholders resolved at compile time. When the concrete type argument is known only at runtime — for example, when deserialising a heterogeneous list of types read from a database schema or a configuration file — the normal `Method<T>()` call site cannot be used because the C# compiler requires `T` to be a compile-time symbol. Reflection bridges this gap.

The pattern is: obtain the open generic `MethodInfo` via `typeof(Service).GetMethod("Process")`, verify it is generic with `IsGenericMethodDefinition`, then call `openMethod.MakeGenericMethod(runtimeType)` to produce a `MethodInfo` whose type parameters are bound to the runtime `Type`. Finally, call `closedMethod.Invoke(target, parameters)`. For types, `typeof(List<>).MakeGenericType(runtimeType)` creates a `Type` representing `List<RuntimeType>`, which you can then pass to `Activator.CreateInstance`.

The cost is significant: `MethodInfo.Invoke` is fifty to a hundred times slower than a direct call due to boxing of value-type arguments, security demands, and the lack of inlining. For code paths called infrequently — plugin loading, startup configuration, one-time serialiser registration — this is acceptable. For hot paths, the conventional fix is to compile the invocation once using `Expression.Lambda<Func<object, object>>(...).Compile()`, cache the resulting delegate in a `ConcurrentDictionary<Type, Delegate>`, and invoke the delegate directly on subsequent calls.

In .NET 10, source generators provide a compile-time alternative for many patterns that historically required runtime reflection: `System.Text.Json` with `[JsonSerializable]`, DI registration via `[GeneratedCode]`, and mapper generation. Where a source generator can enumerate types at build time, runtime `MakeGenericType` is no longer needed, and the resulting code is trim-safe, AOT-compatible, and an order of magnitude faster.

---

## CQ15. How do nullable reference type annotations behave inside generic type parameters, and why does `T?` mean different things depending on the constraint?

**Concepts**
- NRT as a compile-time annotation layer with no runtime representation; `null` is still physically possible (Advanced C# Features)
- `T?` meaning `Nullable<T>` when `T : struct`; meaning an annotated nullable reference when `T : class` (Generics & Collections, Advanced C# Features)
- Unconstrained `T?` being a compile error; `[MaybeNull]`/`[NotNull]` attributes as the workaround (Generics & Collections, Advanced C# Features)
- `where T : notnull` prohibiting nullable type arguments; NRT contract flow through virtual method overrides (OOP, Advanced C# Features)

**Answer**

Nullable reference types are a purely compile-time feature: the runtime never changed — a reference is still either null or not at the IL level. The compiler tracks nullability as a flow annotation, warning when a potentially-null value is dereferenced without a null check. This annotation system interacts subtly with generics because `T?` has two distinct semantics depending on how `T` is constrained.

When `T : struct`, `T?` desugars to `Nullable<T>` — a real runtime type with a `HasValue` boolean and a `Value` field, occupying slightly more stack space than `T` alone. When `T : class`, `T?` is a nullable reference annotation with no runtime representation; the value can still physically be null but the compiler tracks and warns on unsafe dereferences. For an unconstrained generic `T` (no `class` or `struct` constraint), writing `T?` is a compile error because the compiler cannot determine which semantics to apply.

To express "this method may return a null `T` regardless of whether `T` is a reference or value type", the idiomatic approach in .NET 10 is `[return: MaybeNull] T MyMethod<T>()` from `System.Diagnostics.CodeAnalysis`. Conversely, `[NotNull]` documents that a `T?` parameter is guaranteed non-null on method exit. Adding `where T : notnull` prohibits callers from passing nullable types as `T`, which is the correct constraint for containers such as `Dictionary<TKey, TValue>` that must not store null keys.

Inheritance adds another layer: if a base class declares `virtual string? GetName()`, an override narrowing to `string` (non-nullable) is safe and the compiler permits it. An override widening from `string` to `string?` violates the base contract and is flagged as a warning, because callers trusting the base declaration may not null-check the result.

---

## CQ16. How does designing to interfaces enable unit testing with Moq, and why is mocking concrete classes a pitfall?

**Concepts**
- Interface segregation as the prerequisite for substitutable test doubles in DI constructors (OOP, Unit Testing)
- Moq using Castle.DynamicProxy to generate a runtime class implementing the interface (Unit Testing)
- Mocking a concrete class requiring `virtual` methods; non-virtual members calling real production code silently (Unit Testing, OOP)
- Strict vs. loose mocks; over-specifying interactions with `Verify` producing brittle tests (Unit Testing)

**Answer**

Test isolation requires that the code under test can receive a fake collaborator instead of the real one. Dependency injection achieves this by accepting collaborator types through constructor parameters typed to interfaces, and interfaces make the compiler accept any conforming substitute. When a service declares `IOrderRepository` in its constructor rather than `OrderRepository`, a unit test can pass a Moq-generated `Mock<IOrderRepository>` without touching the database.

Moq generates test doubles at runtime using Castle.DynamicProxy, which creates a class that either implements the mocked interface or inherits from the mocked concrete class. When you mock an interface, every member is interceptable because all interface members are virtual by definition in IL. When you mock a concrete class, DynamicProxy generates a subclass and can only intercept members declared `virtual` or `abstract`. A non-virtual method is bound at compile time through the concrete type's vtable slot; DynamicProxy cannot override it, so Moq silently calls the real implementation. Your "mock" is then running production code during a unit test — the test provides false confidence, and the bug it hides may only surface in integration tests or production.

The recommended pattern is to program to the smallest interface the test needs (interface segregation): an `IOrderReader` with only `GetByIdAsync` separate from an `IOrderWriter` with `SaveAsync`. Tests of read paths receive an `IOrderReader` mock and never need to arrange save operations.

Moq's `Verify` checks that specific methods were called, while assertions on output check that the system under test produced correct results. Over-relying on `Verify` — asserting exact method calls with exact arguments — produces brittle tests that fail when internal implementation details change even when observable behaviour is preserved. The preferred approach is to verify outcomes (return values, state, exceptions) and reserve `Verify` for cases where a side-effect call is itself the contract being tested, such as confirming an audit log was written.

---

## CQ17. What is the loop-variable capture gotcha with closures, and how does it interact with async lambdas and `CancellationToken`?

**Concepts**
- Closure capturing a variable by reference to its storage location, not by value at capture time (Functional Style)
- `for` loop sharing a single `i` variable across all lambda instances; `foreach` giving each iteration its own copy since C# 5 (Functional Style)
- Async lambda capturing outer `CancellationToken` that may be cancelled before the lambda executes on a thread-pool thread (Functional Style, Async)
- `Task.Run(action, state)` overload avoiding closure allocation for high-frequency dispatch (Async, Functional Style)

**Answer**

A closure captures a reference to a variable's storage location, not a copy of the value at the moment of capture. In a classic `for` loop, a single `i` variable is shared by all lambda instances created in the loop body:

```csharp
var actions = new List<Action>();
for (int i = 0; i < 5; i++)
    actions.Add(() => Console.WriteLine(i)); // all share the same i
actions.ForEach(a => a()); // prints 5 5 5 5 5
```

By the time the lambdas execute, the loop has finished and `i` equals 5. The fix is to introduce a local copy inside the loop body (`int copy = i;`) so each lambda captures its own distinct variable. C# 5 fixed this specifically for `foreach` — each iteration gets its own captured copy of the loop variable — but `for` loops still share a single `i`.

Async lambdas add a temporal risk. When you write `Task.Run(async () => await ProcessAsync(cts.Token))` in a loop that enqueues many tasks, each lambda captures the `CancellationToken` value at the time the lambda object is created. If the source is cancelled between the point a lambda is created and the point it actually runs on a thread-pool thread, `ProcessAsync` receives a pre-cancelled token and throws `OperationCanceledException` immediately. For a large queue of eagerly created tasks, every task created after cancellation signals fails instantly — which may be correct behaviour, but it must be an explicit design decision rather than an accidental consequence of when the token is read. The defence is to check `token.IsCancellationRequested` at the start of the lambda body and decide how to respond before reaching the first `await`.

For high-frequency `Task.Run` dispatch, passing state via the `Task.Run(Func<object?, Task>, object?)` overload avoids allocating a closure heap object entirely, which the .NET performance guidelines recommend for hot paths.

---

## CQ18. How does LINQ's deferred execution interact with a `List<T>` that is modified between query creation and enumeration?

**Concepts**
- LINQ query as a lazy `IEnumerable<T>` pipeline evaluated only when iterated, not when declared (LINQ)
- `List<T>` internal version counter throwing `InvalidOperationException` on structural modification during enumeration (Generics & Collections)
- `ToList()` and `ToArray()` materialising the sequence into an independent snapshot (LINQ, Generics & Collections)
- Double-enumeration bug: materialise once when the source is expensive to re-evaluate or structurally unstable (LINQ)

**Answer**

LINQ queries are lazy: calling `Where`, `Select`, or any non-materialising operator returns an `IEnumerable<T>` that encodes the pipeline as a chain of iterator objects. No actual work occurs until something iterates the result — a `foreach`, a `ToList()`, or an aggregate such as `Sum()`. The same query object can be iterated multiple times, re-evaluating the entire pipeline on each pass.

`List<T>` tracks an internal version counter that increments on every structural modification (add, remove, insert, clear). Its enumerator checks this counter on every `MoveNext()`. If the list is modified between query creation and enumeration — or mid-enumeration — the enumerator throws `InvalidOperationException: Collection was modified; enumeration operation may not execute`. This is .NET's defence against undefined iteration behaviour rather than silent corruption.

The common mistake is forgetting that a LINQ query does not snapshot the list at the time it is written:

```csharp
var query = list.Where(x => x.IsActive); // no iteration yet
list.Add(new Item());                     // modifies list before query runs
foreach (var item in query)              // throws if the add raced with enumeration
    Process(item);
```

The fix is to call `.ToList()` or `.ToArray()` immediately after the LINQ expression, materialising the result into an independent collection before the source can change:

```csharp
var snapshot = list.Where(x => x.IsActive).ToList();
list.Add(new Item()); // safe: snapshot is a separate list
```

A secondary subtlety is the double-enumeration bug: calling a materialising operation twice on the same lazy query re-executes the pipeline twice. For an `IEnumerable<T>` backed by a database query, a file read, or a network call, double enumeration means two round trips with no warning. The rule is to materialise exactly once and reuse the resulting array or list.

---

# 02. C# Language Fundamentals — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [C# Basics](01.%20C%23%20Basics%20-%20Done/INTERVIEW_QA.md) | Variables, types, operators, control flow, and exception fundamentals |
| 02 | [Object Oriented Programming](02.%20Object%20Oriented%20Programming/INTERVIEW_QA.md) | Classes, inheritance, interfaces, polymorphism, and encapsulation |
| 03 | [Generics & Collections](03.%20Generics%20%26%20Collections/INTERVIEW_QA.md) | Generic types, variance, List, Dictionary, HashSet, and typed collections |
| 04 | [Functional Style Programming](04.%20Functional%20Style%20Programming/INTERVIEW_QA.md) | Delegates, lambdas, Func/Action, closures, and expression trees |
| 05 | [Language Integrated Query](05.%20Language%20Integrated%20Query/INTERVIEW_QA.md) | LINQ syntax, deferred execution, query operators, and EF Core integration |
| 06 | [Multithreading & Async Programming](06.%20Multithreading%20%26%20Async%20Programming/INTERVIEW_QA.md) | Task, async/await, parallelism, synchronization primitives, and cancellation |
| 07 | [File Input & Output and Streams](07.%20File%20Input%20%26%20Outpout%20and%20Streams/INTERVIEW_QA.md) | FileStream, StreamReader/Writer, async file I/O, and compression |
| 08 | [Advanced C# Features](08.%20Advanced%20C%23%20Features/INTERVIEW_QA.md) | Records, pattern matching, nullable references, spans, and source generators |
| 09 | [Unit Testing](09.%20Unit%20Testing/INTERVIEW_QA.md) | xUnit, NUnit, Moq, test organization, and async test patterns |

---

## Table of Contents

- [CQ1. Why can `IEnumerable<Derived>` substitute `IEnumerable<Base>` but `List<Derived>` cannot substitute `List<Base>`?](#cq1-why-can-ienumerablederived-substitute-ienumerablebase-but-listderived-cannot-substitute-listbase)
- [CQ2. What breaks when you mix LINQ's deferred execution with async/await, and how do you fix it?](#cq2-what-breaks-when-you-mix-linqs-deferred-execution-with-asyncawait-and-how-do-you-fix-it)
- [CQ3. Why does try/catch around Task.WhenAll miss exceptions, and how does async void make it worse?](#cq3-why-does-trycatch-around-taskwhenall-miss-exceptions-and-how-does-async-void-make-it-worse)
- [CQ4. What happens when you use a record with a mutable property as a Dictionary key or HashSet element?](#cq4-what-happens-when-you-use-a-record-with-a-mutable-property-as-a-dictionary-key-or-hashset-element)
- [CQ5. How do generic constraints combine with Func<T> and LINQ operators to enforce compile-time type safety?](#cq5-how-do-generic-constraints-combine-with-funct-and-linq-operators-to-enforce-compile-time-type-safety)
- [CQ6. How do you correctly write and test an async method and an IAsyncEnumerable<T> producer in xUnit or NUnit?](#cq6-how-do-you-correctly-write-and-test-an-async-method-and-iasyncenumerablet-producer-in-xunit-or-nunit)
- [CQ7. Why should value types that serve as collection keys or sorted elements implement `IEquatable<T>` and `IComparable<T>` explicitly?](#cq7-why-should-value-types-that-serve-as-collection-keys-or-sorted-elements-implement-iequatablet-and-icomparablet-explicitly)
- [CQ8. How do generic delegates like `Action<T>` and `Func<T, TResult>` avoid boxing compared to non-generic delegate types?](#cq8-how-do-generic-delegates-like-actiont-and-funct-tresult-avoid-boxing-compared-to-non-generic-delegate-types)
- [CQ9. What pitfalls arise when performing file I/O inside async methods, and how does sync-over-async cause deadlocks?](#cq9-what-pitfalls-arise-when-performing-file-io-inside-async-methods-and-how-does-sync-over-async-cause-deadlocks)
- [CQ10. Why can `Span<T>` not survive an `await` boundary, and how does `Memory<T>` address that for async file I/O?](#cq10-why-can-spant-not-survive-an-await-boundary-and-how-does-memoryt-address-that-for-async-file-io)
- [CQ11. When should you prefer a `switch` expression with type patterns over virtual dispatch, and when is polymorphism still better?](#cq11-when-should-you-prefer-a-switch-expression-with-type-patterns-over-virtual-dispatch-and-when-is-polymorphism-still-better)
- [CQ12. What is the trap of calling `.Select()` on an `IAsyncEnumerable<T>`, and which LINQ operators genuinely work with async sequences?](#cq12-what-is-the-trap-of-calling-select-on-an-iasyncenumerablet-and-which-linq-operators-genuinely-work-with-async-sequences)
- [CQ13. How do records interact with inheritance in C# 13, and when should you use a `readonly struct` instead?](#cq13-how-do-records-interact-with-inheritance-in-c-13-and-when-should-you-use-a-readonly-struct-instead)
- [CQ14. How does reflection enable calling a generic method when the type argument is only known at runtime?](#cq14-how-does-reflection-enable-calling-a-generic-method-when-the-type-argument-is-only-known-at-runtime)
- [CQ15. How do nullable reference type annotations behave inside generic type parameters, and why does `T?` mean different things depending on the constraint?](#cq15-how-do-nullable-reference-type-annotations-behave-inside-generic-type-parameters-and-why-does-t-mean-different-things-depending-on-the-constraint)
- [CQ16. How does designing to interfaces enable unit testing with Moq, and why is mocking concrete classes a pitfall?](#cq16-how-does-designing-to-interfaces-enable-unit-testing-with-moq-and-why-is-mocking-concrete-classes-a-pitfall)
- [CQ17. What is the loop-variable capture gotcha with closures, and how does it interact with async lambdas and `CancellationToken`?](#cq17-what-is-the-loop-variable-capture-gotcha-with-closures-and-how-does-it-interact-with-async-lambdas-and-cancellationtoken)
- [CQ18. How does LINQ's deferred execution interact with a `List<T>` that is modified between query creation and enumeration?](#cq18-how-does-linqs-deferred-execution-interact-with-a-listt-that-is-modified-between-query-creation-and-enumeration)

---

## CQ1. Why can `IEnumerable<Derived>` substitute `IEnumerable<Base>` but `List<Derived>` cannot substitute `List<Base>`?

**Concepts**
- Covariance (`out T`) declared on `IEnumerable<T>` (Generics & Collections, OOP interfaces)
- Invariance of `List<T>` due to both read and write members (Generics & Collections)
- Liskov Substitution Principle and static type safety (OOP)
- How covariance enables polymorphic LINQ pipelines (Generics, LINQ)

**Answer**

`IEnumerable<T>` is declared with the `out` covariance annotation (`IEnumerable<out T>`) in .NET, which tells the compiler that `T` only ever flows out of the interface — you can enumerate items but never write them back. Because no mutation is possible through the interface, assigning an `IEnumerable<Dog>` to an `IEnumerable<Animal>` variable is provably safe: every `Dog` is an `Animal`, so reading an element as `Animal` can never yield an invalid object.

`List<T>` is invariant because it exposes both reads (`T this[int index] { get; }`) and writes (`void Add(T item)`). If the compiler allowed `List<Dog>` to be treated as `List<Animal>`, you could call `list.Add(new Cat())` through the `List<Animal>` reference, silently inserting a `Cat` into what is physically a `List<Dog>`. That corrupts the collection's type contract at runtime, so the compiler forbids the assignment entirely.

The practical payoff surfaces in LINQ: because every concrete collection — `List<Dog>`, `Dog[]`, `HashSet<Dog>` — implicitly converts to `IEnumerable<Animal>` when `Dog : Animal`, a single LINQ method accepting `IEnumerable<Animal>` handles any homogeneous collection of subtypes without an explicit cast or copy. You can also cross from invariant to covariant form explicitly with `.Cast<Animal>()` or `.OfType<Animal>()`, but with covariant interfaces the conversion is implicit and allocation-free. Understanding why `List<T>` must stay invariant explains one of the most common C# compile errors beginners encounter and prevents subtle collection-corruption bugs.

---

## CQ2. What breaks when you mix LINQ's deferred execution with `async`/`await`, and how do you fix it?

**Concepts**
- LINQ deferred execution — query is declared, not run, at assignment (LINQ)
- `async` lambdas inside `Select`/`Where` produce `Task<T>`, not `T` (LINQ, Async)
- `Task.WhenAll` as a bounded-batch workaround (Async)
- `IAsyncEnumerable<T>` and `await foreach` for true async streaming (Async, Generics & Collections)

**Answer**

LINQ operators like `Select` and `Where` are synchronous higher-order functions that expect a `Func<T, TResult>` returning a plain value. When you pass an `async` lambda, the compiler infers `Func<T, Task<TResult>>`, so `Select` materialises a sequence of hot `Task<TResult>` objects rather than a sequence of unwrapped results. The tasks are started lazily — only when the outer `foreach` or `.ToList()` pulls them — and nothing inside the LINQ pipeline awaits them. If you forget to await each element individually, the resulting `IEnumerable<Task<TResult>>` silently drops exceptions and produces default values, yielding incorrect output with no compile-time warning.

The conventional fix for a bounded, in-memory batch is `await Task.WhenAll(source.Select(async x => await FetchAsync(x)))`, which starts all tasks concurrently and awaits the entire group at once. However, this buffers every result in memory before any can be consumed, and a single faulted task causes `WhenAll` to throw while the others finish unobserved.

For true streaming with back-pressure, .NET provides `IAsyncEnumerable<T>`. A producer marks itself `async IAsyncEnumerable<T>` and uses `yield return` inside async code; the consumer uses `await foreach` which pulls one item at a time. The `System.Linq.Async` NuGet package adds `WhereAwait`, `SelectAwait`, and `ToListAsync` — async-aware LINQ operators over `IAsyncEnumerable<T>` — restoring the familiar pipeline style without the deferred-execution trap.

---

## CQ3. Why does `try`/`catch` around `Task.WhenAll` miss exceptions, and how does `async void` make it worse?

**Concepts**
- `AggregateException` wrapping multiple faulted tasks (Async)
- `await` unwrapping only the first inner exception from `AggregateException` (Async)
- `async void` fire-and-forget with no observable `Task` (Async, C# Basics exception model)
- `TaskScheduler.UnobservedTaskException` as a process-level trap (Async)

**Answer**

`Task.WhenAll` returns a single `Task` that faults when any constituent task faults, wrapping all failures in an `AggregateException`. When you `await` that combined task inside a `try`/`catch`, the C# awaiter machinery unwraps the `AggregateException` and re-throws only the first `InnerException`. Every subsequent failure from other faulted tasks is silently discarded by the `await` unwrapping step. The `catch` block therefore receives exactly one exception regardless of how many tasks failed, and the others leave no trace unless you capture the `Task` reference before awaiting it and inspect `.Exception.InnerExceptions` directly in the `catch` block.

`async void` compounds the problem at a more fundamental level. An `async void` method cannot be awaited; the returned value is simply discarded. When an exception escapes an `async void` body, the runtime has no `Task` to attach it to, so it re-raises the exception directly on the captured `SynchronizationContext` — the thread-pool in console and ASP.NET Core apps. In .NET 10, an unhandled thread-pool exception triggers `TaskScheduler.UnobservedTaskException` and can terminate the process. There is no `try`/`catch` at the call site that can intercept it, because the caller completed before the async work threw. For this reason `async void` is reserved exclusively for event-handler signatures mandated by framework contracts; every other async method must return `Task` or `Task<T>` so exceptions are observable and awaitable.

---

## CQ4. What happens when you use a `record` with a mutable property as a `Dictionary` key or `HashSet` element?

**Concepts**
- Compiler-generated `Equals`/`GetHashCode` on records uses all properties (Advanced C# Features)
- Hash-based collections store items in buckets chosen by the hash code at insertion time (Generics & Collections)
- Hash code stability requirement for collection correctness (Generics & Collections)
- `init`-only setters vs. mutable `set` on record properties (Advanced C# Features)

**Answer**

C# positional records automatically synthesise `Equals` and `GetHashCode` from all declared properties. This makes records attractive as dictionary keys or set elements because value equality is provided without any boilerplate — two records with identical property values are considered equal, unlike classes which default to reference identity. The feature works reliably as long as the record is immutable.

The trap is mutability. Hash-based collections — `Dictionary<TKey, TValue>` and `HashSet<T>` — compute the hash code at insertion time and store the object in the corresponding bucket. The fundamental contract for any key type is that `GetHashCode()` must return the same value for the lifetime of the object while it lives in the collection. If you define a record with a regular mutable `set` accessor and then mutate that property after insertion, the record's hash code changes, but the object still occupies the old bucket. A subsequent lookup by the new value searches the new bucket and finds nothing; a removal by the new key silently fails; `ContainsKey` returns `false` for an item that is physically present. No exception is thrown — the collection is silently corrupted.

Records declared with `init`-only property setters (`public string Name { get; init; }`) are safe because properties cannot be changed after object construction, keeping the hash code stable for the object's lifetime. The compiler does not prevent you from adding a mutable `set` to a record property, so the defence is a deliberate design rule: only use records with all-`init` or read-only properties as keys in hash-based collections, or implement `IEquatable<T>` on a class backed by deliberately immutable key fields.

---

## CQ5. How do generic constraints combine with `Func<T>` and LINQ operators to enforce compile-time type safety?

**Concepts**
- Generic type constraints (`where T : IComparable<T>`, `where T : notnull`) (Generics & Collections)
- Contravariance of `Func<in T, out TResult>` delegate input parameter (Functional Style, Generics)
- LINQ operators as generic extension methods on `IEnumerable<T>` (LINQ, Generics)
- Compile-time type inference across chained pipeline stages (Generics, LINQ)

**Answer**

Generic constraints let you declare at the method signature level which capabilities `T` must provide, so the compiler can verify correctness without boxing to `object` or deferring errors to runtime. For example, `static T Max<T>(IEnumerable<T> source) where T : IComparable<T>` guarantees that `CompareTo` is available on every element; the compiler rejects any call where `T` does not satisfy the constraint rather than failing with a `MissingMethodException` at runtime. Adding `where T : notnull` in .NET 10 similarly suppresses nullable-reference warnings for generic containers that must not store null.

LINQ is built entirely on this principle. `OrderBy<TSource, TKey>` relies internally on `Comparer<TKey>.Default`, which works only because `TKey` must support comparison; `GroupBy` and `ToDictionary` rely on `EqualityComparer<TKey>.Default`. Because all operators are generic extension methods on `IEnumerable<T>`, the C# type-inference engine propagates the inferred type through the entire chain: `numbers.Where(n => n > 0).Select(n => n * 2.0)` infers `IEnumerable<double>` for the final result without a single explicit annotation.

`Func<T, TResult>` introduces variance into the picture. `Func` is contravariant in `T` (the input) and covariant in `TResult` (the output). A `Func<Animal, string>` can be stored in a `Func<Dog, string>` variable — a predicate that handles all animals handles dogs too, so the assignment is safe. Combining constraints with `Func` parameters lets library authors write methods that accept arbitrary predicates or projections while retaining full static type information at every stage, which is why a twenty-step LINQ pipeline never loses its element type even when mixing `Select`, `Where`, `GroupBy`, and custom extension methods.

---

## CQ6. How do you correctly write and test an `async` method and an `IAsyncEnumerable<T>` producer in xUnit or NUnit?

**Concepts**
- `async Task` test methods in xUnit/NUnit vs. `async void` tests (Unit Testing, Async)
- `async void` tests silently passing even when the body throws (Unit Testing, Async)
- `ConfigureAwait(false)` behaviour in test runners vs. UI contexts (Async, Unit Testing)
- Consuming `IAsyncEnumerable<T>` inside a test with `await foreach` or `ToListAsync` (Async, Generics, Unit Testing)

**Answer**

Both xUnit and NUnit support `async Task` test methods natively. The test runner calls the method, receives the returned `Task`, and awaits it on its own scheduler before recording the outcome. This means an exception thrown inside the async body propagates correctly through the `Task` and is recorded as a test failure. The critical rule is that the test method must return `Task` — never `async void`. An `async void` test appears to pass even when it throws, because the test framework receives no `Task` to await; the synchronous portion of the method completes, the runner records a green result, and the exception surfaces later on the thread-pool, completely outside the test's observation window.

`ConfigureAwait(false)` has a subtler role in test contexts. In library code it prevents deadlocks by not capturing the caller's `SynchronizationContext` for continuations. Most xUnit and NUnit runner contexts have a null or custom context, so omitting `ConfigureAwait(false)` is generally harmless in tests. Tests that assert against UI-thread state in WinForms or WPF must preserve the context, but for the vast majority of service and domain logic tests, either form is safe. Consistency with the production code under test is a reasonable tiebreaker.

Testing an `IAsyncEnumerable<T>` producer requires explicitly consuming the stream. The idiomatic pattern is to accumulate results with `await foreach` into a `List<T>` and then assert on the list, or to use `System.Linq.Async`'s `ToListAsync()` extension for a one-liner. Cancellation paths are tested by passing a pre-cancelled `CancellationToken` to the producer and asserting that an `OperationCanceledException` is thrown; xUnit's `Assert.ThrowsAsync<OperationCanceledException>` or NUnit's `Assert.ThrowsAsync` await the consuming wrapper correctly, as long as the test method itself is `async Task`.

---

## CQ7. Why should value types that serve as collection keys or sorted elements implement `IEquatable<T>` and `IComparable<T>` explicitly?

**Concepts**
- Default `object.Equals` and `GetHashCode` on structs using reflection-based field comparison (OOP)
- `IEquatable<T>` bypassing virtual dispatch and boxing on value types (Generics & Collections, OOP)
- `EqualityComparer<T>.Default` resolving to `IEquatable<T>` when available, avoiding the boxing fallback (Generics & Collections)
- `IComparable<T>` required by `SortedSet<T>`, `SortedDictionary<TKey,TValue>`, and `Comparer<T>.Default` (Generics & Collections)

**Answer**

The default `object.Equals` and `GetHashCode` implementations on user-defined `struct` types use reflection internally to compare or hash every field. `EqualityComparer<T>.Default` checks at construction time whether `T` implements `IEquatable<T>`, and if it does, it calls `T.Equals(T other)` directly — a statically dispatched, non-boxing call. If `T` does not implement `IEquatable<T>`, the fallback path boxes both operands to `object` and invokes the reflective equality check. For a struct stored in a `Dictionary<T, V>` or `HashSet<T>`, every lookup, insertion, and removal triggers equality comparison, so the boxing cost accumulates in tight loops or high-throughput code.

Implementing `IComparable<T>` matters whenever the type participates in ordered collections or LINQ sort operators. `SortedSet<T>` and `SortedDictionary<TKey, TValue>` require their key type to implement `IComparable<T>` (or receive an explicit `IComparer<T>`); without it the constructor throws `InvalidOperationException` at runtime. `Array.Sort<T>` and `OrderBy` call `Comparer<T>.Default`, which resolves to `IComparable<T>` when available, falling back to the non-generic `IComparable` otherwise — a path that boxes value types on every comparison and can measurably slow a sort over even a modest array.

The practical rule: any `struct` that will be a dictionary key, a set element, or a sort key should implement both `IEquatable<T>` and `IComparable<T>`, and should also override `object.Equals(object?)` and `GetHashCode` for completeness. Annotating the struct `readonly` in C# 13 guarantees no mutation during comparison and prevents defensive copies that the compiler sometimes inserts when a non-readonly struct is passed to an interface method.

---

## CQ8. How do generic delegates like `Action<T>` and `Func<T, TResult>` avoid boxing compared to non-generic delegate types?

**Concepts**
- Non-generic `delegate void Process(object item)` requiring boxing for every value-type argument (Functional Style)
- Generic delegates instantiated per concrete `T` by the JIT; value-type `T` gets its own compiled body (Generics & Collections, Functional Style)
- JIT code sharing for reference-type instantiations; dedicated native code for each value-type `T` (Generics & Collections)
- `Comparison<T>` as a delegate-based alternative to `IComparer<T>` in `List<T>.Sort`, enabling JIT inlining (Generics & Collections, Functional Style)

**Answer**

Before generics, delegates had to accept `object` parameters to be type-agnostic, which forced value-type arguments to be boxed on every invocation. Passing an `int` to a `delegate void Process(object item)` allocates a heap object, performs the call, and the result must be unboxed before use — three operations for what should be a trivial function application. For high-frequency callbacks over value-type sequences, that allocation pressure is significant.

Generic delegates resolve this at the JIT level. When the runtime compiles `Action<int>`, it creates a specialised code path where the `int` argument flows as a 32-bit integer through the calling convention with no heap allocation. For reference-type instantiations — `Action<string>`, `Action<List<int>>` — the JIT shares a single compiled body because all references are the same pointer size; only value-type instantiations each get their own compiled body. This is why generic collections and LINQ operators can process `int[]` or `Span<int>` at near-native speed.

`Comparison<T>` (`delegate int Comparison<T>(T x, T y)`) is the canonical example of applying this principle to sorting. `List<T>.Sort(Comparison<T>)` avoids allocating an `IComparer<T>` heap object and avoids virtual dispatch through the interface; the JIT can inline the delegate body into the sort algorithm when the delegate wraps a static method or a simple lambda. The same advantage applies to `Func<T, TResult>` in LINQ — `Select<TSource, TResult>(Func<TSource, TResult> selector)` is generic in both source and result, so projecting from `int` to `double` never boxes either operand. The non-generic `ArrayList` and its `object`-based delegate callbacks predate this design and are avoided in all modern .NET 10 code for exactly this reason.

---

## CQ9. What pitfalls arise when performing file I/O inside async methods, and how does sync-over-async cause deadlocks?

**Concepts**
- `StreamReader.ReadToEndAsync(CancellationToken)` vs. synchronous `ReadToEnd` inside an async call chain (File I/O, Async)
- Sync-over-async antipattern: `.Result` or `.Wait()` on a file-reading `Task` from a single-threaded `SynchronizationContext` (Async, File I/O)
- `SynchronizationContext` capture and the deadlock mechanism when a continuation needs the blocked thread (Async)
- `CancellationToken` propagation from an HTTP request to the file operation to avoid wasted I/O bandwidth (Async, File I/O)

**Answer**

Async file I/O in .NET uses overlapped I/O on Windows or epoll on Linux, completing the operation on the OS thread-pool without blocking a managed thread. When you call `File.ReadAllTextAsync(path, token)` or `await reader.ReadToEndAsync(token)`, the managed thread is returned to the pool while the kernel services the request. The critical rule is that every step in the async call chain must also be asynchronous; inserting one synchronous file call defeats the purpose and, in certain hosting contexts, causes a deadlock.

The classic deadlock arises in any framework that installs a single-threaded `SynchronizationContext` — classic ASP.NET, WinForms, WPF. Calling `reader.ReadToEndAsync().Result` or `.Wait()` on that thread blocks the thread waiting for the `Task` to complete. The task's continuation is scheduled back to the captured `SynchronizationContext`, which requires the same thread that is now blocked. Neither side can proceed: the thread waits for the task, the task waits for the thread. ASP.NET Core and .NET 10 console apps use a thread-pool context without this constraint, so the deadlock does not reproduce there — which is exactly why it often surfaces only in production environments whose host differs from the developer's test runner.

`CancellationToken` should flow all the way from the caller to the file operation. In ASP.NET Core, `HttpContext.RequestAborted` is cancelled when the client disconnects; plumbing it to `ReadToEndAsync(cancellationToken)` aborts the disk read and releases the file handle rather than continuing to read a large file for a client that has gone away. Forgetting to propagate the token means the server wastes I/O bandwidth, holds file handles longer than necessary, and cannot shed load during peak traffic.

---

## CQ10. Why can `Span<T>` not survive an `await` boundary, and how does `Memory<T>` address that for async file I/O?

**Concepts**
- `Span<T>` as a `ref struct` restricted to the stack; cannot be captured in heap closures (Advanced C# Features)
- Async state-machine capturing local variables in a heap-allocated object across `await` suspension points (Async)
- `Memory<T>` as a heap-safe counterpart: ordinary managed struct, not a `ref struct` (Advanced C# Features, File I/O)
- `RandomAccess.ReadAsync(SafeFileHandle, Memory<byte>, long, CancellationToken)` for zero-copy async reads (File I/O, Async)

**Answer**

`Span<T>` is declared as a `ref struct`, which means the compiler enforces that every instance lives exclusively on the stack. This restriction exists so the runtime never needs to GC-trace a `Span<T>` — it cannot outlive the stack frame that created it, and the GC never moves it unexpectedly. The restriction becomes a hard compiler error the moment `Span<T>` meets `await`: when a method suspends at an `await` point, the current stack frame is dismantled and all local state is captured in a state-machine heap object so the continuation can resume later. Because `Span<T>` cannot be placed on the heap, the compiler refuses to let a `Span<T>` local variable cross an `await` boundary — you get a compile error rather than a subtle memory-safety bug.

`Memory<T>` solves this by being an ordinary managed struct (not a `ref struct`), backed by an array, a `MemoryManager<T>`, or a string segment. It can be captured in a state-machine closure, stored in fields, and passed across async method calls freely. The idiomatic pattern for async file I/O in .NET 10 is to allocate a `byte[]` buffer once, wrap it as `Memory<byte>`, and pass it to `RandomAccess.ReadAsync(handle, memory, fileOffset, token)`. The underlying implementation pins the memory and hands it to the OS for a single overlapped I/O operation with no intermediate copies.

When you need to process the buffer contents synchronously within a single stack frame — parsing, slicing, searching — you call `memory.Span` to obtain a `Span<T>` for that scope, do the work, and drop the span before the next `await`. This pattern gives you the ergonomics and zero-copy performance of span-based processing while keeping the buffer lifetime managed by the heap through `Memory<T>` across async suspension boundaries.

---

## CQ11. When should you prefer a `switch` expression with type patterns over virtual dispatch, and when is polymorphism still better?

**Concepts**
- `switch` expression with type pattern `is TypeName { Property: value }` for exhaustive type discrimination (Advanced C# Features)
- Virtual dispatch via `override`/`abstract` methods centralising behaviour per type (OOP)
- Open/closed principle: new-type extension favours virtual dispatch; new-operation extension favours switch (OOP, Advanced C# Features)
- Exhaustiveness checking and `SwitchExpressionException` on unmatched arms with sealed hierarchies (Advanced C# Features)

**Answer**

Type-pattern `switch` expressions and virtual dispatch solve the same problem from opposite directions, and the choice depends on which axis of change is more likely.

Virtual dispatch shines when you expect new subtypes to be added over time. Defining an `abstract Shape.Area()` method means a new `Pentagon` class only needs to provide its own `Area()` implementation; every existing call site that invokes `shape.Area()` works without modification. Adding a new operation across all subtypes is harder — you must add an abstract method to the base class and implement it everywhere.

Pattern-matching `switch` expressions flip this trade-off. A switch over all known `Shape` subtypes makes it easy to add new operations — a `Perimeter` calculation needs only one new switch expression — but adding a new `Pentagon` means updating every existing switch. The compiler helps when the hierarchy is `sealed`: a missing arm produces a warning, and forgetting a case causes a `SwitchExpressionException` at runtime rather than silently returning a wrong result. With C# 13 list patterns and property patterns, arms can be remarkably concise: `case Circle { Radius: var r } => Math.PI * r * r`.

The practical guidance: prefer virtual dispatch for domain entities in a growing model where new types arrive frequently (classic DDD aggregates and value objects), and prefer pattern-matching for discriminated-union-like data where the type set is fixed and operations are varied — for example, a result type that is `Success`, `ValidationError`, or `NotFound`. Records pair especially well with pattern-matching in this role. Mixing both is valid and common: virtual dispatch for the shared hot path, a switch expression for an outlier operation that has no business being on every type.

---

## CQ12. What is the trap of calling `.Select()` on an `IAsyncEnumerable<T>`, and which LINQ operators genuinely work with async sequences?

**Concepts**
- `IAsyncEnumerable<T>` not implementing `IEnumerable<T>`; `GetAsyncEnumerator` vs. `GetEnumerator` (Async, LINQ)
- Compiler falling back to `Enumerable.Select` when no async-aware overload is in scope, producing `IEnumerable<Task<T>>` (LINQ, Async)
- `System.Linq.Async` NuGet package providing `SelectAwait`, `WhereAwait`, `OrderByAwait`, and `ToListAsync` (LINQ, Async)
- `await foreach` as the only standard-library consumption mechanism for `IAsyncEnumerable<T>` (Async)

**Answer**

`IAsyncEnumerable<T>` does not inherit from `IEnumerable<T>`. It has its own `GetAsyncEnumerator` method and requires `await foreach` to consume it. When you write `asyncStream.Select(x => Transform(x))` without importing `System.Linq.Async`, the C# compiler looks for an extension method named `Select` on `IAsyncEnumerable<T>` and finds none in the standard library. It then considers `Enumerable.Select`, which accepts any type for its first argument — technically not a match for `IAsyncEnumerable<T>` as a sequence, but if the compiler can satisfy the overload by treating the async-enumerable as a single-element `IEnumerable<IAsyncEnumerable<T>>`, the result is a meaningless `IEnumerable<TResult>` that projects the stream object as one item rather than its elements. Even in scenarios where the call resolves with an `async` lambda, `Enumerable.Select` returns `IEnumerable<Task<TResult>>` — a synchronous sequence of hot tasks, none of which are awaited by the pipeline. The resulting code may compile without warnings and produce silently wrong output.

`System.Linq.Async` (part of the Rx.NET ecosystem, fully supported on .NET 10) adds async-aware operators: `SelectAwait(async x => ...)`, `WhereAwait(async x => ...)`, `OrderByAwait`, and `ToListAsync()`. These return `IAsyncEnumerable<TResult>` and honour `await foreach` semantics throughout the chain. For simple synchronous projections, the package also provides `Select` returning `IAsyncEnumerable<TResult>`.

The rule: never apply `System.Linq.Enumerable` operators directly to `IAsyncEnumerable<T>`. Either add `System.Linq.Async` and use the `*Await` variants, or accumulate with `await foreach` into a `List<T>` and apply synchronous LINQ on the materialised list.

---

## CQ13. How do records interact with inheritance in C# 13, and when should you use a `readonly struct` instead?

**Concepts**
- Compiler-synthesised `EqualityContract` property ensuring polymorphic equality correctness in record hierarchies (Advanced C# Features, OOP)
- Virtual `Clone()` method making `with` expressions preserve the derived runtime type (Advanced C# Features, OOP)
- Primary constructors (C# 12) on classes vs. positional record constructors: property synthesis vs. parameter capture only (Advanced C# Features)
- `readonly struct` for guaranteed stack allocation; `record struct` for value-semantic copy-with (Advanced C# Features)

**Answer**

Records support single-inheritance chains: a `record` can extend another `record`, but not a plain `class`, and vice versa. The compiler synthesises a protected `EqualityContract` property on every record type — derived records override it to return their own `Type`. Equality comparisons between a base-record variable and a derived-record instance return `false` even when all shared properties match, because the `EqualityContract` check ensures a `Point` and a `Point3D` with the same `X` and `Y` are never considered equal. This is one of the subtler differences from class equality and is important when records are stored in sets or used as dictionary keys.

The `with` expression creates a copy via the virtual `Clone()` method, which derived records override. `with` on a derived record stored in a base-record variable still produces the correct derived type at runtime, though the compile-time type of the expression is the declared variable type. Primary constructors in C# 12 brought compact constructor syntax to plain classes and structs, but they differ from positional record constructors: a primary constructor on a `class` captures parameters as private fields; it does not synthesise public `init` properties. A positional record synthesises both the constructor and the corresponding public `init` properties in one declaration, along with `Equals`, `GetHashCode`, `Deconstruct`, and `ToString`.

`readonly struct` is the right choice when you need guaranteed stack allocation — for use with `Span<T>` APIs, `stackalloc` patterns, or zero-GC-pressure value objects like `Vector2` or `Color`. A `record struct` adds value-semantic equality and the `with` expression to a struct while remaining a value type that copies on assignment. Choose `record struct` when the struct is conceptually an immutable data bundle that needs non-destructive mutation syntax; choose `readonly struct` when the priority is performance, AOT friendliness, and ensuring every access path sees consistent state.

---

## CQ14. How does reflection enable calling a generic method when the type argument is only known at runtime?

**Concepts**
- `MethodInfo.MakeGenericMethod(Type[])` constructing a closed generic method from an open `MethodInfo` (Advanced C# Features, Generics)
- `typeof(List<>)` as an open generic type; `Type.MakeGenericType(Type[])` producing a closed type (Generics, Advanced C# Features)
- Performance cost of `MethodInfo.Invoke` vs. direct calls; boxing of value-type parameters (Advanced C# Features)
- Compiled-expression delegates and source generators as AOT-safe alternatives to runtime reflection (Functional Style, Advanced C# Features)

**Answer**

Generic methods are compiled with type parameters as placeholders resolved at compile time. When the concrete type argument is known only at runtime — for example, when deserialising a heterogeneous list of types read from a database schema or a configuration file — the normal `Method<T>()` call site cannot be used because the C# compiler requires `T` to be a compile-time symbol. Reflection bridges this gap.

The pattern is: obtain the open generic `MethodInfo` via `typeof(Service).GetMethod("Process")`, verify it is generic with `IsGenericMethodDefinition`, then call `openMethod.MakeGenericMethod(runtimeType)` to produce a `MethodInfo` whose type parameters are bound to the runtime `Type`. Finally, call `closedMethod.Invoke(target, parameters)`. For types, `typeof(List<>).MakeGenericType(runtimeType)` creates a `Type` representing `List<RuntimeType>`, which you can then pass to `Activator.CreateInstance`.

The cost is significant: `MethodInfo.Invoke` is fifty to a hundred times slower than a direct call due to boxing of value-type arguments, security demands, and the lack of inlining. For code paths called infrequently — plugin loading, startup configuration, one-time serialiser registration — this is acceptable. For hot paths, the conventional fix is to compile the invocation once using `Expression.Lambda<Func<object, object>>(...).Compile()`, cache the resulting delegate in a `ConcurrentDictionary<Type, Delegate>`, and invoke the delegate directly on subsequent calls.

In .NET 10, source generators provide a compile-time alternative for many patterns that historically required runtime reflection: `System.Text.Json` with `[JsonSerializable]`, DI registration via `[GeneratedCode]`, and mapper generation. Where a source generator can enumerate types at build time, runtime `MakeGenericType` is no longer needed, and the resulting code is trim-safe, AOT-compatible, and an order of magnitude faster.

---

## CQ15. How do nullable reference type annotations behave inside generic type parameters, and why does `T?` mean different things depending on the constraint?

**Concepts**
- NRT as a compile-time annotation layer with no runtime representation; `null` is still physically possible (Advanced C# Features)
- `T?` meaning `Nullable<T>` when `T : struct`; meaning an annotated nullable reference when `T : class` (Generics & Collections, Advanced C# Features)
- Unconstrained `T?` being a compile error; `[MaybeNull]`/`[NotNull]` attributes as the workaround (Generics & Collections, Advanced C# Features)
- `where T : notnull` prohibiting nullable type arguments; NRT contract flow through virtual method overrides (OOP, Advanced C# Features)

**Answer**

Nullable reference types are a purely compile-time feature: the runtime never changed — a reference is still either null or not at the IL level. The compiler tracks nullability as a flow annotation, warning when a potentially-null value is dereferenced without a null check. This annotation system interacts subtly with generics because `T?` has two distinct semantics depending on how `T` is constrained.

When `T : struct`, `T?` desugars to `Nullable<T>` — a real runtime type with a `HasValue` boolean and a `Value` field, occupying slightly more stack space than `T` alone. When `T : class`, `T?` is a nullable reference annotation with no runtime representation; the value can still physically be null but the compiler tracks and warns on unsafe dereferences. For an unconstrained generic `T` (no `class` or `struct` constraint), writing `T?` is a compile error because the compiler cannot determine which semantics to apply.

To express "this method may return a null `T` regardless of whether `T` is a reference or value type", the idiomatic approach in .NET 10 is `[return: MaybeNull] T MyMethod<T>()` from `System.Diagnostics.CodeAnalysis`. Conversely, `[NotNull]` documents that a `T?` parameter is guaranteed non-null on method exit. Adding `where T : notnull` prohibits callers from passing nullable types as `T`, which is the correct constraint for containers such as `Dictionary<TKey, TValue>` that must not store null keys.

Inheritance adds another layer: if a base class declares `virtual string? GetName()`, an override narrowing to `string` (non-nullable) is safe and the compiler permits it. An override widening from `string` to `string?` violates the base contract and is flagged as a warning, because callers trusting the base declaration may not null-check the result.

---

## CQ16. How does designing to interfaces enable unit testing with Moq, and why is mocking concrete classes a pitfall?

**Concepts**
- Interface segregation as the prerequisite for substitutable test doubles in DI constructors (OOP, Unit Testing)
- Moq using Castle.DynamicProxy to generate a runtime class implementing the interface (Unit Testing)
- Mocking a concrete class requiring `virtual` methods; non-virtual members calling real production code silently (Unit Testing, OOP)
- Strict vs. loose mocks; over-specifying interactions with `Verify` producing brittle tests (Unit Testing)

**Answer**

Test isolation requires that the code under test can receive a fake collaborator instead of the real one. Dependency injection achieves this by accepting collaborator types through constructor parameters typed to interfaces, and interfaces make the compiler accept any conforming substitute. When a service declares `IOrderRepository` in its constructor rather than `OrderRepository`, a unit test can pass a Moq-generated `Mock<IOrderRepository>` without touching the database.

Moq generates test doubles at runtime using Castle.DynamicProxy, which creates a class that either implements the mocked interface or inherits from the mocked concrete class. When you mock an interface, every member is interceptable because all interface members are virtual by definition in IL. When you mock a concrete class, DynamicProxy generates a subclass and can only intercept members declared `virtual` or `abstract`. A non-virtual method is bound at compile time through the concrete type's vtable slot; DynamicProxy cannot override it, so Moq silently calls the real implementation. Your "mock" is then running production code during a unit test — the test provides false confidence, and the bug it hides may only surface in integration tests or production.

The recommended pattern is to program to the smallest interface the test needs (interface segregation): an `IOrderReader` with only `GetByIdAsync` separate from an `IOrderWriter` with `SaveAsync`. Tests of read paths receive an `IOrderReader` mock and never need to arrange save operations.

Moq's `Verify` checks that specific methods were called, while assertions on output check that the system under test produced correct results. Over-relying on `Verify` — asserting exact method calls with exact arguments — produces brittle tests that fail when internal implementation details change even when observable behaviour is preserved. The preferred approach is to verify outcomes (return values, state, exceptions) and reserve `Verify` for cases where a side-effect call is itself the contract being tested, such as confirming an audit log was written.

---

## CQ17. What is the loop-variable capture gotcha with closures, and how does it interact with async lambdas and `CancellationToken`?

**Concepts**
- Closure capturing a variable by reference to its storage location, not by value at capture time (Functional Style)
- `for` loop sharing a single `i` variable across all lambda instances; `foreach` giving each iteration its own copy since C# 5 (Functional Style)
- Async lambda capturing outer `CancellationToken` that may be cancelled before the lambda executes on a thread-pool thread (Functional Style, Async)
- `Task.Run(action, state)` overload avoiding closure allocation for high-frequency dispatch (Async, Functional Style)

**Answer**

A closure captures a reference to a variable's storage location, not a copy of the value at the moment of capture. In a classic `for` loop, a single `i` variable is shared by all lambda instances created in the loop body:

```csharp
var actions = new List<Action>();
for (int i = 0; i < 5; i++)
    actions.Add(() => Console.WriteLine(i)); // all share the same i
actions.ForEach(a => a()); // prints 5 5 5 5 5
```

By the time the lambdas execute, the loop has finished and `i` equals 5. The fix is to introduce a local copy inside the loop body (`int copy = i;`) so each lambda captures its own distinct variable. C# 5 fixed this specifically for `foreach` — each iteration gets its own captured copy of the loop variable — but `for` loops still share a single `i`.

Async lambdas add a temporal risk. When you write `Task.Run(async () => await ProcessAsync(cts.Token))` in a loop that enqueues many tasks, each lambda captures the `CancellationToken` value at the time the lambda object is created. If the source is cancelled between the point a lambda is created and the point it actually runs on a thread-pool thread, `ProcessAsync` receives a pre-cancelled token and throws `OperationCanceledException` immediately. For a large queue of eagerly created tasks, every task created after cancellation signals fails instantly — which may be correct behaviour, but it must be an explicit design decision rather than an accidental consequence of when the token is read. The defence is to check `token.IsCancellationRequested` at the start of the lambda body and decide how to respond before reaching the first `await`.

For high-frequency `Task.Run` dispatch, passing state via the `Task.Run(Func<object?, Task>, object?)` overload avoids allocating a closure heap object entirely, which the .NET performance guidelines recommend for hot paths.

---

## CQ18. How does LINQ's deferred execution interact with a `List<T>` that is modified between query creation and enumeration?

**Concepts**
- LINQ query as a lazy `IEnumerable<T>` pipeline evaluated only when iterated, not when declared (LINQ)
- `List<T>` internal version counter throwing `InvalidOperationException` on structural modification during enumeration (Generics & Collections)
- `ToList()` and `ToArray()` materialising the sequence into an independent snapshot (LINQ, Generics & Collections)
- Double-enumeration bug: materialise once when the source is expensive to re-evaluate or structurally unstable (LINQ)

**Answer**

LINQ queries are lazy: calling `Where`, `Select`, or any non-materialising operator returns an `IEnumerable<T>` that encodes the pipeline as a chain of iterator objects. No actual work occurs until something iterates the result — a `foreach`, a `ToList()`, or an aggregate such as `Sum()`. The same query object can be iterated multiple times, re-evaluating the entire pipeline on each pass.

`List<T>` tracks an internal version counter that increments on every structural modification (add, remove, insert, clear). Its enumerator checks this counter on every `MoveNext()`. If the list is modified between query creation and enumeration — or mid-enumeration — the enumerator throws `InvalidOperationException: Collection was modified; enumeration operation may not execute`. This is .NET's defence against undefined iteration behaviour rather than silent corruption.

The common mistake is forgetting that a LINQ query does not snapshot the list at the time it is written:

```csharp
var query = list.Where(x => x.IsActive); // no iteration yet
list.Add(new Item());                     // modifies list before query runs
foreach (var item in query)              // throws if the add raced with enumeration
    Process(item);
```

The fix is to call `.ToList()` or `.ToArray()` immediately after the LINQ expression, materialising the result into an independent collection before the source can change:

```csharp
var snapshot = list.Where(x => x.IsActive).ToList();
list.Add(new Item()); // safe: snapshot is a separate list
```

A secondary subtlety is the double-enumeration bug: calling a materialising operation twice on the same lazy query re-executes the pipeline twice. For an `IEnumerable<T>` backed by a database query, a file read, or a network call, double enumeration means two round trips with no warning. The rule is to materialise exactly once and reuse the resulting array or list.
