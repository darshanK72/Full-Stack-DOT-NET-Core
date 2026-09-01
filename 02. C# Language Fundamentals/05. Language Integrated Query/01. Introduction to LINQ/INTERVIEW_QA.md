# LINQ: Introduction to LINQ — Interview Q&A

---

## Q1. What is LINQ and why was it introduced in C#?

**Concepts**
- Language Integrated Query
- Uniform data access syntax
- Compile-time type checking
- Extension methods on IEnumerable<T>
- Query providers

**Answer**

LINQ (Language Integrated Query) is a set of language features and library APIs introduced in C# 3.0 that allows developers to write structured, type-safe queries directly in C# code against any data source that implements IEnumerable<T> or IQueryable<T>. Before LINQ, querying different data sources — in-memory collections, databases, XML — required learning separate APIs and query languages, with no compiler support to catch errors. LINQ unifies these by expressing data transformations as composable method chains or declarative query expressions that the compiler checks at build time. The runtime evaluates these queries through provider implementations: LINQ to Objects works over in-memory collections using delegates, while LINQ to Entities translates the query to SQL through Entity Framework's expression-tree machinery. This uniform surface reduces cognitive overhead, enables IDE tooling like IntelliSense to work across all data sources, and produces code that is often more readable than equivalent imperative loops.

---

## Q2. What is the difference between query syntax and method syntax in LINQ?

**Concepts**
- Query expression syntax (SQL-like keywords)
- Method syntax (fluent extension method chains)
- Compiler translation to method calls
- Readability trade-offs
- Supported operators

**Answer**

Query syntax uses SQL-like keywords (`from`, `where`, `select`, `orderby`, `group`, `join`) embedded directly in C# code as a declarative expression. Method syntax chains extension methods such as `Where`, `Select`, `OrderBy`, and `GroupBy` using lambda expressions. Both styles produce identical compiled output because the C# compiler translates every query expression into its equivalent method call during compilation — query syntax is purely syntactic sugar. The practical difference is readability: query syntax reads naturally for multi-source joins and grouping (`from o in orders join c in customers...`), while method syntax is more concise for simple filters and is the only way to call operators that have no query-expression counterpart, such as `Count`, `Sum`, `Distinct`, `Skip`, and `Take`. In practice most teams use method syntax for short chains and reserve query syntax for complex joins. The two styles can also be mixed — wrapping a query expression in parentheses makes it a value that further method calls can be chained onto.

---

## Q3. What is IEnumerable<T> and what role does it play in LINQ?

**Concepts**
- IEnumerable<T> interface
- GetEnumerator pattern
- Pull-based iteration
- Extension method target type
- LINQ to Objects foundation

**Answer**

`IEnumerable<T>` is a generic interface in `System.Collections.Generic` that exposes a single method, `GetEnumerator()`, returning an `IEnumerator<T>`. Any type implementing this interface can be iterated with a `foreach` loop and, crucially, is the target of the LINQ extension methods defined in `System.Linq.Enumerable`. This makes LINQ universally applicable: arrays, lists, queues, custom collections, and even lazily computed sequences all expose the same surface. When a LINQ operator like `Where` is called, it returns a new `IEnumerable<T>` that wraps the source with a filter predicate; no elements are actually inspected until an enumerator is requested and `MoveNext` is called. This pull-based, lazy design is what enables deferred execution and allows chains of operators to process large sequences without allocating intermediate in-memory collections for every step.

---

## Q4. Explain deferred execution in LINQ.

**Concepts**
- Lazy evaluation
- Iterator state machine
- Query definition vs. query execution
- Side effects and re-enumeration
- Immediate vs. deferred operators

**Answer**

Deferred execution means that a LINQ query expression builds a recipe — an object graph representing the intended operations — but performs no actual work until the result is consumed by iterating or by calling an immediate operator. When you write `var q = numbers.Where(x => x > 5)`, the call returns an iterator object immediately without examining a single element. The filter runs only when you `foreach` over `q`, call `q.ToList()`, or pass `q` to `Count()`. This design has two important consequences. First, if the underlying data source changes between query definition and execution, the query sees the modified data. Second, calling the query multiple times re-executes it each time, which can be expensive if the source is an I/O-backed stream but is perfectly correct for in-memory collections. Operators that terminate deferred execution — `ToList`, `ToArray`, `ToDictionary`, `Count`, `Sum`, `First`, `Single`, `Any`, `All`, and `ForEach` on `List<T>` — are called immediate operators because they drive the enumeration to completion and return a concrete value or collection.

---

## Q5. How do extension methods enable LINQ?

**Concepts**
- Extension method declaration (`this` parameter)
- Static class requirement
- Compile-time method resolution
- Namespace import with `using`
- Method chaining pattern

**Answer**

Extension methods let you add new methods to existing types without modifying those types or creating subclasses. They are declared as static methods in a static class, with the first parameter prefixed by `this` indicating the type being extended. The C# compiler transforms a call like `numbers.Where(x => x > 5)` into `Enumerable.Where(numbers, x => x > 5)` when `System.Linq` is in scope. This mechanism is what allows LINQ operators to appear on every `IEnumerable<T>` — the `Enumerable` static class declares all the standard query operators as extension methods targeting `IEnumerable<T>`. Method chaining becomes natural because each operator returns another `IEnumerable<T>`, so the output of `Where` can feed directly into `Select`, then into `OrderBy`, forming a readable pipeline. The chain reads left to right in the direction of data flow, matching the mental model of a pipeline of transformations.

---

## Q6. What are lambda expressions and how are they used in LINQ?

**Concepts**
- Lambda expression syntax
- Delegate compatibility
- Func<T,TResult> and Predicate<T>
- Captured variables (closures)
- Expression trees vs. compiled delegates

**Answer**

Lambda expressions are anonymous functions written as `parameters => body`, used throughout LINQ to pass behavior as data. In LINQ method syntax, predicates like `Where(x => x.Age > 18)` and projections like `Select(x => x.Name)` are lambdas compiled to `Func<T, bool>` or `Func<T, TResult>` delegates respectively. The C# compiler generates an anonymous method (or a display class if the lambda captures local variables) and stores a delegate pointing to it. When used with `IQueryable<T>` providers such as Entity Framework, the compiler compiles lambdas as `Expression<Func<T, TResult>>` — an abstract syntax tree that the provider can inspect and translate to SQL — rather than as an executable delegate. Lambdas can capture variables from their enclosing scope (closures), which is useful for parameterized queries like `var min = 10; list.Where(x => x > min)`, but this capture should be understood: the variable is captured by reference, so a mutation of `min` after the lambda is defined but before it runs will affect the query result.

---

## Q7. What is the difference between IEnumerable<T> and IQueryable<T> in LINQ?

**Concepts**
- In-memory vs. remote execution
- Expression tree vs. delegate
- Provider translation to SQL
- AsQueryable / AsEnumerable conversion
- Performance implications

**Answer**

`IEnumerable<T>` operates over sequences in memory using compiled delegate callbacks — each operator is a C# method that calls your lambda directly. `IQueryable<T>` extends `IEnumerable<T>` with an `Expression` property and a `Provider` property; it stores the query as an expression tree rather than executing it immediately. When you enumerate an `IQueryable<T>`, the provider (e.g., Entity Framework's `DbSet<T>`) inspects that expression tree and translates it to the target query language — typically SQL — sending only the requested data across the wire. The practical consequence is that `dbContext.Users.Where(u => u.Active)` sends `WHERE Active = 1` to the database, while `dbContext.Users.AsEnumerable().Where(u => u.Active)` loads all users into memory first and then filters in C#. Calling `AsEnumerable()` forces a switch to in-memory LINQ, which is sometimes desirable when a filter involves a method the provider cannot translate, but it can destroy query performance when applied too early.

---

## Q8. What are anonymous types and how do they relate to LINQ projections?

**Concepts**
- Anonymous type inference
- Read-only properties
- Compiler-generated equality
- `var` keyword requirement
- Use in select clauses

**Answer**

Anonymous types are compiler-generated immutable reference types created using the syntax `new { Property1 = value1, Property2 = value2 }`. The compiler infers property names and types, generates a `GetHashCode` and `Equals` implementation based on all properties, and creates a class with a hidden name. They are primarily useful in LINQ `Select` projections when you need to shape data for immediate use without defining a named DTO. For example, `orders.Select(o => new { o.Id, o.Total, CustomerName = o.Customer.Name })` creates a sequence of anonymous-type instances. Because the type name is unspeakable, you must use `var` or pass the result to another generic method. Anonymous types work well for intermediate transformations within a method but cannot be returned from methods or stored in fields without boxing — for those scenarios, named tuples or `record` types defined with `record` syntax in modern C# are preferred, offering the same brevity with reusable, named types.

---

## Q9. How do you write a LINQ query in query syntax?

**Concepts**
- `from` clause (range variable)
- `where` clause (filter predicate)
- `select` clause (projection)
- `orderby` clause
- Mandatory `select` or `group` terminator

**Answer**

A LINQ query expression always begins with a `from` clause that declares a range variable ranging over the source sequence, followed by optional `where`, `orderby`, `join`, and `group` clauses, and is terminated by a mandatory `select` or `group by` clause. For example:

```csharp
var seniors = from p in people
              where p.Age >= 65
              orderby p.LastName
              select new { p.FirstName, p.LastName, p.Age };
```

The range variable `p` is scoped to the entire expression and represents one element at a time as the query is evaluated. The compiler translates this to `people.Where(p => p.Age >= 65).OrderBy(p => p.LastName).Select(p => new { p.FirstName, p.LastName, p.Age })`. Every query expression is purely syntactic sugar — there is no runtime distinction. The `from` clause must come first, but multiple `from` clauses can be chained to produce cross joins, and `let` clauses introduce sub-expressions reused later. The resulting expression is an `IEnumerable<T>` (or `IQueryable<T>` for EF sources) that evaluates lazily.

---

## Q10. What is the role of the `select` clause in LINQ?

**Concepts**
- Projection operator
- Shape transformation
- Anonymous type creation
- Identity projection
- `Select` overload with index

**Answer**

The `select` clause (or the `Select` method) defines the shape of each output element by transforming the input element into a new form. It is the projection step in a LINQ pipeline. The simplest form is an identity projection — `select x` — which passes each element through unchanged, but the most common usage transforms an element into a different type, an anonymous type, a DTO, or a computed value. For example, `select new ProductDto { Name = p.Name, Price = p.Price * 1.1m }` creates a new `ProductDto` for each product with a 10% price markup. The method-syntax overload `Select(Func<T, int, TResult>)` also accepts a second parameter that carries the zero-based index of the current element, enabling constructions like `items.Select((item, i) => $"{i + 1}. {item}")`. When `Select` is the last operator before materialization and the projected type is a struct or value type, care should be taken with boxing implications, though this is rarely a problem in practice.

---

## Q11. What is the `var` keyword's role in LINQ queries?

**Concepts**
- Local type inference
- Compiler-inferred type
- Anonymous type requirement
- Readability trade-off
- No runtime impact

**Answer**

The `var` keyword instructs the compiler to infer the type of a local variable from the right-hand side of the assignment. In LINQ, `var` is essential when the result type is an anonymous type — since anonymous type names are unspeakable in source code, there is no other way to declare a variable of that type. For named types, `var` is optional but reduces verbosity: `var results = names.Where(n => n.StartsWith("A"))` is equivalent to `IEnumerable<string> results = ...` with the advantage that if the source type changes, the declaration does not need updating. The inferred type is determined entirely at compile time; `var` has no runtime overhead or boxing implications. It does not produce `dynamic` or `object` variables — the type is fully static and IntelliSense works exactly as it would with an explicit type declaration. Style guides vary: some teams use explicit types for improved readability at the cost of verbosity, while others use `var` universally to reduce noise.

---

## Q12. What happens when you chain multiple LINQ operators together?

**Concepts**
- Operator composition
- Iterator pipeline
- Lazy pull-through evaluation
- Intermediate IEnumerable wrappers
- Stack of iterator state machines

**Answer**

Chaining LINQ operators creates a lazy pipeline of iterator state machines, each wrapping the previous one. When you write `source.Where(x => x > 0).Select(x => x * 2).Take(5)`, no elements are processed yet. Once enumeration begins — for example, a `foreach` loop calls `GetEnumerator().MoveNext()` on the `Take` iterator — `Take` asks its inner `Select` iterator for the next element, which in turn asks its inner `Where` iterator, which pulls from the source. Each element travels the full pipeline from source to consumer before the next element is requested. This pull-through design means only as many elements as necessary are evaluated: if `Take(5)` has been satisfied, `Where` and `Select` stop processing even if the source has millions of remaining elements. There are no intermediate lists allocated between operators. The only exception is operators that must inspect all input before producing output — `OrderBy`, `GroupBy`, and `Reverse` — which buffer internally and break the lazy chain at that point.

---

## Q13. How does LINQ handle null values in a sequence?

**Concepts**
- Null reference elements
- NullReferenceException in predicates
- Null-safe navigation operator `?.`
- FirstOrDefault returning null
- DefaultIfEmpty behavior

**Answer**

LINQ operators do not guard against null elements in a sequence; they pass elements through to your lambda expressions as-is. If your source collection contains `null` references and your predicate or projection dereferences them without null checks, a `NullReferenceException` will be thrown during enumeration. The fix is to add null guards in the lambda: `list.Where(x => x?.Name != null)` uses the null-conditional operator to short-circuit on null items. Separately, certain element-retrieval operators return null (for reference types) or the default value (for value types) when no match is found — `FirstOrDefault`, `LastOrDefault`, `SingleOrDefault`, and `ElementAtOrDefault`. These are safe alternatives to `First` and friends when an empty result is expected. `DefaultIfEmpty()` modifies a sequence so that it yields a single default element if the source is empty, which is useful in left-join patterns with `GroupJoin`.

---

## Q14. What is the Enumerable class and what does it provide?

**Concepts**
- Static class in System.Linq
- Standard Query Operators
- Extension methods on IEnumerable<T>
- Generation operators (Range, Repeat, Empty)
- Aggregation operators

**Answer**

`System.Linq.Enumerable` is the static class that defines all the standard LINQ query operators as extension methods targeting `IEnumerable<T>`. It ships as part of the `System.Linq` namespace in the .NET runtime. The operators it provides cover filtering (`Where`), projection (`Select`, `SelectMany`), ordering (`OrderBy`, `OrderByDescending`, `ThenBy`), grouping (`GroupBy`), aggregation (`Sum`, `Count`, `Average`, `Min`, `Max`, `Aggregate`), set operations (`Distinct`, `Union`, `Intersect`, `Except`), element retrieval (`First`, `Last`, `Single`, `ElementAt`), quantification (`Any`, `All`, `Contains`), partitioning (`Skip`, `Take`, `SkipWhile`, `TakeWhile`), conversion (`ToList`, `ToArray`, `ToDictionary`, `ToHashSet`), and sequence generation (`Range`, `Repeat`, `Empty`). Adding a `using System.Linq;` directive brings all these methods into scope for any `IEnumerable<T>`. Understanding this class is fundamental because every LINQ query — whether written in query or method syntax — ultimately maps to these methods.

---

## Q15. What is LINQ to Objects?

**Concepts**
- In-memory collection queries
- Enumerable extension methods
- No provider translation
- Delegate-based predicates
- Direct C# execution

**Answer**

LINQ to Objects refers specifically to using LINQ operators against in-memory .NET collections that implement `IEnumerable<T>` — arrays, `List<T>`, `Dictionary<TKey,TValue>`, custom collections, and any sequence produced by `yield return`. Unlike LINQ to Entities or LINQ to XML, there is no translation step; the lambdas are compiled to ordinary C# delegates and executed directly by the `System.Linq.Enumerable` methods. This means that any valid C# expression can appear in a predicate or projection, including calls to methods, complex multi-statement lambdas (as block-body delegates), and operations on non-serializable types. The trade-off is that all filtering happens in memory — if the source data comes from a database and you accidentally materialize it before filtering (for example by calling `ToList` on a `DbSet<T>` before `Where`), you lose the ability to push the filter to the server. LINQ to Objects is ideal for post-fetch shaping, unit-testable business logic, and working with in-process data structures.

---

## Q16. What are the benefits of using LINQ over traditional `foreach` loops?

**Concepts**
- Declarative vs. imperative style
- Reduced mutable state
- Composability and reuse
- Compiler type checking
- Readability and intent clarity

**Answer**

LINQ promotes a declarative programming style where you state *what* data you want rather than *how* to retrieve it, in contrast to imperative `foreach` loops that explicitly manage accumulators, conditionals, and intermediate lists. This reduces mutable state, making code easier to reason about and test. A four-line `foreach` with an `if` condition, an accumulator, and a result list collapses to a single-line method chain — `orders.Where(o => o.IsActive).Select(o => o.Total).Sum()` — that is self-documenting in its intent. LINQ chains are also composable: you can build query fragments in parts, pass `IEnumerable<T>` values between methods, and extend them without modifying existing code. The compiler type-checks every step, so a mismatch between a `Where` predicate type and the source element type is caught at build time rather than at runtime. Performance is generally equivalent to well-written imperative code for simple operations, though very hot paths may warrant manual iteration to avoid the overhead of state-machine allocators.

---

## Q17. How do you use `let` in a query expression?

**Concepts**
- `let` clause for sub-expression naming
- Range variable extension
- Avoiding repeated computation
- Scope within query expression
- Compiler expansion to anonymous type

**Answer**

The `let` clause in a LINQ query expression introduces a new range variable computed from existing ones, allowing you to name an intermediate result and reuse it later in the same query without duplicating the computation. For example:

```csharp
var results = from s in strings
              let lower = s.ToLower()
              where lower.StartsWith("a")
              select lower;
```

Without `let`, you would have to call `s.ToLower()` twice — once in the `where` clause and again in the `select`. Internally, the compiler expands a `let` clause by introducing an anonymous type that carries both the original range variable and the new one, equivalent to `strings.Select(s => new { s, lower = s.ToLower() }).Where(x => x.lower.StartsWith("a")).Select(x => x.lower)`. The `let` clause improves readability for complex transformations and is especially helpful when a computed value is used in multiple clauses. In method syntax, the equivalent pattern is to use `Select` to introduce the computed property before filtering.

---

## Q18. What is the `into` continuation clause in LINQ?

**Concepts**
- Query continuation with `into`
- `group...by...into` pattern
- `join...into` for group joins
- Range variable reset
- Re-querying grouped results

**Answer**

The `into` keyword in a LINQ query expression introduces a continuation that lets you query the result of a `group by` or `join` clause as a new source. After a `group by ... into g` clause, `g` becomes a new range variable of type `IGrouping<TKey, TElement>`, and the query can continue with additional `where`, `orderby`, or `select` clauses operating on those groups. For example:

```csharp
var grouped = from p in products
              group p by p.Category into g
              where g.Count() > 5
              select new { Category = g.Key, Count = g.Count() };
```

Without `into`, the query would have to terminate at the `group` clause and a second query would be needed to filter groups. In the context of `join`, `join ... into j` produces a group join, where `j` is an `IEnumerable<T>` of matching elements from the inner sequence for each outer element — the LINQ equivalent of a LEFT OUTER JOIN. The `into` keyword effectively resets the range variable scope, hiding the previous range variables after the continuation point.

---

## Q19. Why does LINQ query syntax not support all operators? (Gotcha)

**Concepts**
- Query expression keyword coverage
- Unsupported operators (Distinct, Skip, Take, Count)
- Method syntax as fallback
- Compiler-supported keywords vs. extension methods
- Mixed syntax pattern

**Answer**

Query expression syntax in C# only maps to a fixed set of operators defined by the language specification: `where`, `select`, `orderby`, `group by`, `join`, and `let`. Many useful LINQ operators — `Distinct`, `Skip`, `Take`, `Count`, `Sum`, `Average`, `Min`, `Max`, `Union`, `Intersect`, `First`, `Single`, and `Any` — have no corresponding query expression keyword and can only be invoked using method syntax. This is a common gotcha for developers who try to write pure query syntax. The idiomatic workaround is to wrap the query expression in parentheses and append method calls: `(from p in products where p.Active select p).Take(10).ToList()`. There is nothing wrong with this mixed style; the compiler translates both forms identically. The real takeaway is that method syntax is always the full-featured option, and query syntax is a readability convenience for a specific subset of operations.

---

## Q20. What is the difference between `First` and `Single`, and when does each throw? (Gotcha)

**Concepts**
- `First` — returns first matching element
- `Single` — asserts exactly one match exists
- `InvalidOperationException` on empty sequence
- `OrDefault` variants for null-safe access
- Performance difference (early termination vs. full scan)

**Answer**

`First()` returns the first element of a sequence (or the first matching a predicate) and throws `InvalidOperationException` if the sequence is empty. `Single()` returns the one element in the sequence but throws `InvalidOperationException` if there are zero *or more than one* matching elements. The distinction is semantic intent: use `Single` when your domain invariant guarantees exactly one match and you want the query to act as an assertion — if a bug introduces duplicates, `Single` surfaces the problem immediately rather than silently returning the first of many. Use `First` when you expect multiple matches and only want one. Regarding performance, `First` can stop iterating after finding one element, while `Single` must continue to the end of the sequence to verify no second match exists — this means `Single` is O(n) even after finding a candidate. Their `OrDefault` variants return `null` (or the value-type default) instead of throwing when the sequence is empty, but `SingleOrDefault` still throws for more-than-one matches. In EF Core, both are translated to SQL `TOP 1` or `TOP 2` as appropriate.

---

## Q21. Why is re-enumerating a LINQ query potentially dangerous? (Gotcha)

**Concepts**
- Deferred execution on re-enumeration
- Side effects from data source changes
- Multiple database round-trips
- Expensive operation duplication
- Materializing with ToList/ToArray

**Answer**

Because LINQ queries are lazy, every time you enumerate a query variable — whether in a `foreach`, `Count()`, `Any()`, or any other consuming call — the entire pipeline re-executes from scratch. If the underlying data source has side effects (a database query, a file read, a web API call, or even in-memory data that mutates between calls), each enumeration may return different results. More subtly, if the query variable is passed to multiple methods and each method iterates it, you perform the full pipeline work multiple times — this is particularly costly if the source is an EF Core query, because each iteration issues a new SQL query to the database. The gotcha surfaces in code like:

```csharp
var results = context.Orders.Where(o => o.IsActive);
var count = results.Count();        // SQL query 1
foreach (var o in results) { ... } // SQL query 2
```

The fix is to call `.ToList()` or `.ToArray()` once to materialize the results into a concrete in-memory collection, then use that collection everywhere. This makes the cost explicit and eliminates repeated round-trips.

---

## Q22. Does a LINQ query run immediately when it is written? (Gotcha)

**Concepts**
- Deferred execution misconception
- Query variable holds recipe, not result
- When execution actually starts
- Immediate operators
- Debugging implications

**Answer**

No — this is one of the most common LINQ misconceptions. Writing a LINQ query and assigning it to a variable does absolutely no work on the data; it only constructs an iterator object that remembers the source and the operations to perform. Execution is deferred until the first request for an element. This surprises developers who set breakpoints inside predicates expecting them to hit during the `var q = ...` line, only to find they hit during the `foreach` or `ToList()` call that follows. A related pitfall is that exceptions thrown inside predicates — such as `NullReferenceException` or `DivisionByZeroException` — surface not at the point the query is defined but at the point of enumeration. When debugging, materializing a query early with `.ToList()` can be helpful to isolate where the exception originates and to confirm that the query produces expected results before passing it to another method.

---

## Q23. Scenario: A junior developer complains that a LINQ query is slow. How would you investigate? (Scenario)

**Concepts**
- Profiling deferred execution cost
- Multiple enumerations of the same query
- LINQ to Objects vs. provider queries
- N+1 query problem
- Materialization timing

**Answer**

The investigation starts by identifying whether the query is LINQ to Objects or a provider query (e.g., EF Core). For a provider query, enable logging or use a profiler (like EF Core's `LogTo` or SQL Server Profiler) to inspect the generated SQL — often the problem is a missing `Include` causing N+1 selects, a filter applied in C# after materializing too much data (because `AsEnumerable()` was called prematurely), or a complex expression the provider cannot translate efficiently. For a LINQ to Objects query, check whether the same query variable is enumerated multiple times: two separate calls to `Count()` and `foreach` over an `IEnumerable<T>` backed by a database will issue two round-trips. The fix is to call `.ToList()` once and reuse the list. Also check for `OrderBy` or `GroupBy` deep in a loop — buffering operators allocate and sort the full input on every iteration. Use BenchmarkDotNet to measure, and consider whether replacing a chain of operators with a single pass using `Aggregate` or a custom iterator would reduce allocations in a hot path.

---

## Q24. Scenario: You need to expose a filtered, sorted, paged list from a service method. How do you design the LINQ pipeline? (Scenario)

**Concepts**
- IQueryable<T> composition
- Filter-then-sort-then-page ordering
- Deferred execution across method boundaries
- ToList timing
- Separation of query construction from execution

**Answer**

The recommended pattern is to accept an `IQueryable<T>` (from the repository or DbContext), apply filter, sort, and paging operators in that order — always filtering first to minimize the dataset before sorting and then paging — and return either an `IQueryable<T>` to allow further composition or materialize with `ToList` at the service boundary. A typical implementation:

```csharp
public async Task<List<OrderDto>> GetPagedOrders(
    OrderFilter filter, int page, int pageSize)
{
    IQueryable<Order> query = _context.Orders
        .Where(o => o.CustomerId == filter.CustomerId)
        .Where(o => !filter.FromDate.HasValue || o.Date >= filter.FromDate)
        .OrderByDescending(o => o.Date)
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

    return await query
        .Select(o => new OrderDto { Id = o.Id, Total = o.Total })
        .ToListAsync();
}
```

The entire pipeline remains as an `IQueryable<T>` until `ToListAsync`, meaning a single SQL query with `WHERE`, `ORDER BY`, `OFFSET`, and `FETCH NEXT` is sent to the database. Never call `ToList` before `Skip`/`Take` — that would load all matching records into memory before paging. The `Select` projection ensures only the columns needed for `OrderDto` are fetched, reducing payload size.

---

## Q25. Scenario: A LINQ query in a background service is raising unexpected NullReferenceExceptions in production but not in tests. How do you diagnose it? (Scenario)

**Concepts**
- Deferred execution in multi-threaded context
- Data source mutation during enumeration
- Null elements in live data not present in test fixtures
- Exception point mismatch (definition vs. enumeration)
- Safe navigation and null guards

**Answer**

The first clue is that exceptions occur during enumeration, not at query definition — the stack trace will point inside an iterator `MoveNext` rather than the line where the query was written. This gap between definition and execution often means the source data in production contains null references or unexpected shapes that test fixtures do not replicate. To diagnose, add structured logging inside the predicate temporarily, or wrap the enumeration in a try/catch that logs the offending element. A common cause in background services is a collection that is mutated by another thread while the LINQ pipeline is pulling from it — iterating `List<T>` while another thread adds or removes items is not thread-safe and can produce `InvalidOperationException` or data corruption. The fix is to snapshot the collection with `.ToList()` before the query runs, then query the snapshot. For the null issue itself, add explicit null guards using the null-conditional operator (`?.`) in predicates and projections, and consider using nullable reference type annotations (`#nullable enable`) to surface these paths at compile time in future code.

---

## Q26. Scenario: You are reviewing code where a coworker uses LINQ to Objects on a DbSet. What problems do you raise? (Scenario)

**Concepts**
- AsEnumerable premature materialization
- Missing server-side filtering
- Full table scan in memory
- N+1 query risk
- IQueryable vs. IEnumerable provider boundary

**Answer**

When a developer calls `.ToList()` or `.AsEnumerable()` on a `DbSet<T>` before applying `Where`, `Select`, or `OrderBy`, they force EF Core to load every row from the table into memory before filtering. Consider this code:

```csharp
// Problematic
var result = context.Products
    .AsEnumerable()          // materializes ALL products
    .Where(p => p.Price > 100)
    .ToList();
```

The `WHERE Price > 100` filter runs in C# on thousands of in-memory objects instead of in the database. The fix:

```csharp
// Correct
var result = context.Products
    .Where(p => p.Price > 100)
    .ToList();
```

In a code review I would flag: (1) performance — a full table scan replaces a selective index seek; (2) memory — all rows are allocated as .NET objects unnecessarily; (3) network — all data travels from the database to the application server before being discarded. The only legitimate reason to call `AsEnumerable` before filtering is when the filter expression uses a C# method the EF Core provider cannot translate to SQL. In that case, filter as much as possible in the provider query first, then call `AsEnumerable`, then apply the untranslatable predicate.
