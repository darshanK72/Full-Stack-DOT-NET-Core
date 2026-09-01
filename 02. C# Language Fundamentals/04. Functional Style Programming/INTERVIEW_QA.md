# Functional Style Programming in C# — Interview Q&A Index

This module covers the functional programming features built into C#. Each subfolder has its own dedicated Q&A file. Use this index to navigate to a specific topic or to study the cross-cutting themes that span the whole module.

---

## Subfolders

| Topic | File |
|-------|------|
| Delegates | [01. Delegates/INTERVIEW_QA.md](01.%20Delegates/INTERVIEW_QA.md) |
| Lambda Expressions | [02. Lambda Expressions/INTERVIEW_QA.md](02.%20Lambda%20Expressions/INTERVIEW_QA.md) |
| Anonymous Methods | [03. Anonymous Methods/INTERVIEW_QA.md](03.%20Anonymous%20Methods/INTERVIEW_QA.md) |
| Extension Methods | [04. Extension Methods/INTERVIEW_QA.md](04.%20Extension%20Methods/INTERVIEW_QA.md) |
| Func, Action & Predicate | [05. Func Action & Predicate/INTERVIEW_QA.md](05.%20Func%20Action%20%26%20Predicate/INTERVIEW_QA.md) |
| Closures | [06. Closures/INTERVIEW_QA.md](06.%20Closures/INTERVIEW_QA.md) |

---

## Cross-Cutting Questions

---

## Q1. How do delegates, lambdas, anonymous methods, and `Func`/`Action` relate to each other conceptually, and how did the language evolve to arrive at the current model?

**Concepts**
- C# 1 named delegate types
- C# 2 anonymous methods
- C# 3 lambda expressions and type inference
- BCL generic delegate family (`Func`, `Action`, `Predicate`)
- unified closure model across all forms

**Answer**

C# functional programming features arrived in layered releases. C# 1 introduced delegate types as typed function pointers that could be stored and invoked, but required named methods for instantiation. C# 2 added anonymous methods (`delegate { }` syntax), allowing inline code to be assigned to delegates without a separate named method — a significant ergonomic improvement. C# 3 replaced anonymous method syntax with the more concise lambda expression (`=>` operator), added type inference for parameters, and introduced expression trees so lambdas could be translated to data structures for remote query execution (LINQ to SQL, Entity Framework). C# 3 also added the `Func<>` and `Action<>` generic delegate families in the BCL, eliminating the need for custom delegate declarations in most cases. Closures — the mechanism by which any function captures outer variables via a compiler-generated display class — underpin all three forms equally. `Func`/`Action` are simply type aliases for the most common delegate shapes; lambdas are the primary syntax for creating delegate values; and anonymous methods remain valid but are largely superseded. Understanding this evolution helps you reason about why certain limitations exist (anonymous methods cannot be expression trees) and why certain patterns emerged (LINQ requires lambdas, not anonymous methods, for IQueryable).

---

## Q2. What is the display class mechanism and how does it connect delegates, lambdas, anonymous methods, and closures into a single underlying model?

**Concepts**
- compiler-generated display class
- variable capture common to all forms
- field promotion from stack to heap
- delegate as method reference on display class
- shared model across syntactic forms

**Answer**

Regardless of whether you write a delegate using a lambda, an anonymous method, or a method group, the compiler uses the same underlying mechanism when outer variables need to be captured. It synthesizes a private nested class — the display class — that holds captured variables as fields. The enclosing method is rewritten to instantiate this class, assign initial values, and replace local variable references with field accesses. The inline function (lambda or anonymous method) becomes an instance method on the display class, and the delegate is created pointing to that method with the display class instance as the target. This unified model means all three syntactic forms have identical runtime behavior and identical performance characteristics for closures. The differences are purely syntactic and expressibility differences: lambdas support expression trees, anonymous methods support parameter-list omission, and method groups have no closure overhead at all. Recognizing this shared foundation makes it possible to reason about capture semantics, lifetime, and threading implications uniformly, regardless of which syntactic form was used.

---

## Q3. When building an abstraction over behavior — passing logic as a parameter — how do you choose between a delegate (`Func`/`Action`), an interface, and a local function?

**Concepts**
- delegate vs single-method interface tradeoff
- composability of `Func`/`Action`
- interface for named contract and multiple methods
- local function for non-delegated private logic
- testing and discoverability considerations

**Answer**

The choice between `Func`/`Action`, an interface, and a local function depends on whether the behavior crosses API boundaries, whether multiple related methods are needed, and whether testability requires mocking. `Func`/`Action` are best for single-method callbacks, short-lived behavior parameters, and internal utilities where composability and LINQ integration matter — they require no extra type declarations and work directly with lambda syntax. Interfaces are better when the abstraction has a name that carries semantic meaning (`IOrderValidator`, `IPaymentGateway`), when multiple related operations belong together, or when the implementation needs to be mocked or swapped in a DI container. Local functions are best for helper logic that is only needed within one method, is not stored or passed around, but is complex enough to warrant extraction from inline lambdas for readability — they share the enclosing scope's variables without display class overhead when they do not close over anything mutable. The pragmatic heuristic: use `Func`/`Action` for behavior arguments in private APIs, local functions for named helpers within a method, and interfaces for public contracts and injectable dependencies.

---

## Q4. How do extension methods and higher-order functions (via `Func`) work together to enable the fluent, composable style seen in LINQ?

**Concepts**
- extension method on `IEnumerable<T>`
- `Func<T, TResult>` as behavior parameter
- deferred execution chain
- method chaining fluency
- composability without inheritance

**Answer**

LINQ achieves its composable fluency by combining extension methods with `Func`-parameterized higher-order functions. Each LINQ operator is an extension method on `IEnumerable<T>` that accepts a `Func` delegate for its behavior (`Where` takes `Func<T, bool>`, `Select` takes `Func<T, TResult>`, `OrderBy` takes `Func<T, TKey>`). Because each operator returns an `IEnumerable<T>`, the result can immediately be chained with another extension method. The lambdas supplied at the call site create closures that capture the query parameters, and deferred execution means those closures are not called until the sequence is enumerated. This design separates what the query does (behavior, supplied as `Func` lambdas) from how it iterates (the extension method implementation). You can apply exactly this pattern to your own domain: define a minimal interface or concrete type, add extension methods that accept `Func` delegates for customizable steps, and return the same type for chaining. The result is an API that reads like a sentence and defers computation until the caller explicitly enumerates or materializes.

---

## Q5. What are the performance implications of closures — display class allocation, delegate instantiation, and static lambdas — and when do they matter in a .NET 10 application?

**Concepts**
- display class heap allocation
- delegate instance allocation
- static lambda caching optimization
- GC pressure in hot paths
- `Span<T>` / `stackalloc` alternative for value scenarios

**Answer**

Every closure that captures at least one outer variable causes a display class instance to be allocated on the heap. Every delegate instantiation also allocates. In most application code these allocations are negligible — the GC handles them efficiently and the overhead is dwarfed by I/O, network calls, and business logic. However, in tight loops, hot paths processing millions of items per second, or latency-sensitive code (game loops, real-time financial processing, high-frequency trading systems), repeated display class and delegate allocations create GC pressure that shows up as increased Gen 0 collection frequency and latency spikes. The mitigations in .NET 10 include: using the `static` lambda modifier to prevent capture and allow the compiler to cache the delegate instance; using method groups instead of lambdas where the method does not need to close over anything (method group conversion can be cached in many cases); passing state through an explicit parameter rather than capturing it, converting a closure to a static lambda; and using `stackalloc` or `Span<T>`-based APIs that avoid heap allocation entirely for short-lived data. The rule of thumb: profile first, optimize second. Premature elimination of closures in ordinary CRUD application code trades readability for no measurable gain.
