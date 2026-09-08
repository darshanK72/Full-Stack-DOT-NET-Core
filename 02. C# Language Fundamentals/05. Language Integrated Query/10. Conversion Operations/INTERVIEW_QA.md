# LINQ: Conversion Operations — Interview Q&A

---


## Table of Contents

1. [Q1. What does `ToList` do and when should you use it?](#q1-what-does-tolist-do-and-when-should-you-use-it)
2. [Q2. What does `ToArray` do and how does it compare to `ToList`?](#q2-what-does-toarray-do-and-how-does-it-compare-to-tolist)
3. [Q3. What does `ToDictionary` do and what happens with duplicate keys?](#q3-what-does-todictionary-do-and-what-happens-with-duplicate-keys)
4. [Q4. What does `ToHashSet` do and when is it useful?](#q4-what-does-tohashset-do-and-when-is-it-useful)
5. [Q5. What does `ToLookup` do and how does it differ from `ToDictionary`?](#q5-what-does-tolookup-do-and-how-does-it-differ-from-todictionary)
6. [Q6. What does `Cast<T>` do and when does it throw?](#q6-what-does-castt-do-and-when-does-it-throw)
7. [Q7. What does `OfType<T>` do?](#q7-what-does-oftypet-do)
8. [Q8. What does `AsEnumerable` do and why is it important in EF Core?](#q8-what-does-asenumerable-do-and-why-is-it-important-in-ef-core)
9. [Q9. What does `AsQueryable` do?](#q9-what-does-asqueryable-do)
10. [Q10. What is the difference between `ToList()`, `ToArray()`, and `AsEnumerable()` in terms of execution? (Gotcha)](#q10-what-is-the-difference-between-tolist-toarray-and-asenumerable-in-terms-of-execution-gotcha)
11. [Q11. Why does `ToDictionary` throw when there are duplicate keys? (Gotcha)](#q11-why-does-todictionary-throw-when-there-are-duplicate-keys-gotcha)
12. [Q12. Scenario: A service method calls `ToList()` early in the pipeline before filtering. What is wrong and how do you fix it? (Scenario)](#q12-scenario-a-service-method-calls-tolist-early-in-the-pipeline-before-filtering-what-is-wrong-and-how-do-you-fix-it-scenario)
13. [Q13. Scenario: You need to build an in-memory lookup table from a database query for use across multiple requests. What conversion operator and caching strategy do you use? (Scenario)](#q13-scenario-you-need-to-build-an-in-memory-lookup-table-from-a-database-query-for-use-across-multiple-requests-what-conversion-operator-and-caching-strategy-do-you-use-scenario)

---
## Q1. What does `ToList` do and when should you use it?

**Concepts**
- Immediate execution / materialization
- Creates new List<T> in memory
- Breaks deferred execution chain
- Enables re-enumeration without re-query
- Appropriate boundary between IQueryable and application logic

**Answer**

`ToList()` enumerates the entire source sequence immediately and stores all elements in a new `List<T>`, ending deferred execution at that point. It is the most common materialization operator and should be used when: (1) you need to enumerate the results multiple times without re-executing the query; (2) you are crossing a service or repository boundary and want to return a concrete collection; (3) the source is a database query and you want a single, bounded database round-trip at a known point. The resulting `List<T>` is a fully independent in-memory collection — subsequent modifications to the original source do not affect it. Avoid calling `ToList()` prematurely before further LINQ operators that could be pushed to the database (e.g., calling `ToList()` before `Where` on an EF Core query). Call it as late as possible in the chain, after all filters, sorts, and projections have been composed.

---

## Q2. What does `ToArray` do and how does it compare to `ToList`?

**Concepts**
- Fixed-size array vs. resizable list
- Immediate execution
- Slightly more memory-efficient for fixed-size output
- No Add/Remove after creation
- ToArray preferred for immutable-result semantics

**Answer**

`ToArray()` enumerates the source and stores elements in a new `T[]` array. Like `ToList`, it is an immediate operator that ends deferred execution. The practical differences from `ToList` are minor: arrays have a fixed size after creation (no `Add` or `Remove`), while `List<T>` can grow dynamically. Arrays may be marginally more memory-efficient because `List<T>` typically over-allocates capacity. Arrays offer slightly faster element access due to their simpler memory layout, but this difference is negligible for most code. In terms of API surface, `IReadOnlyList<T>` is implemented by both, so either works for read-only consumers. Prefer `ToArray` when you want to communicate that the result should not be modified, or when you need to pass the result to an API that requires `T[]`. Prefer `ToList` when the caller may need to add or remove elements from the result.

---

## Q3. What does `ToDictionary` do and what happens with duplicate keys?

**Concepts**
- Immediate execution
- Key selector and optional value selector
- Throws ArgumentException on duplicate keys
- O(n) time and space
- IDictionary<TKey, TValue> result

**Answer**

`ToDictionary(keySelector)` iterates the source and builds a `Dictionary<TKey, TSource>` where each element's key is determined by the key selector. The overloaded form `ToDictionary(keySelector, valueSelector)` also transforms the value stored. `ToDictionary` throws `ArgumentException` ("An item with the same key has already been added") if the key selector produces duplicate keys — it enforces a one-to-one key-to-value relationship. To handle multiple values per key, use `ToLookup` instead, which groups duplicates. `ToDictionary` is O(n) in time and O(n) in memory. The resulting `Dictionary` is fully independent of the source. Common usage: building an id-to-entity lookup for O(1) access: `var entityMap = entities.ToDictionary(e => e.Id)`. An `IEqualityComparer<TKey>` overload is available for custom key comparison (e.g., case-insensitive string keys).

---

## Q4. What does `ToHashSet` do and when is it useful?

**Concepts**
- Immediate execution
- Unique elements only (deduplicates)
- O(1) Contains checks
- IEqualityComparer<T> overload
- Useful for membership tests

**Answer**

`ToHashSet()` (available since .NET Core 2.0 as an extension method, available on `HashSet<T>` itself since .NET 4.7.2) enumerates the source and creates a `HashSet<T>`, automatically deduplicating elements. The primary use case is converting a sequence to a set for fast `O(1)` membership testing: `var lookup = allowedIds.ToHashSet(); items.Where(x => lookup.Contains(x.Id))`. Without the `HashSet`, `Contains` on a `List<T>` or `IEnumerable<T>` is O(n), making the entire filter O(n²). An `IEqualityComparer<T>` overload supports custom equality: `strings.ToHashSet(StringComparer.OrdinalIgnoreCase)` creates a case-insensitive set. `ToHashSet` is also useful for deduplication without needing the preserved-order behavior of `Distinct` — though the order of elements in a `HashSet` is not guaranteed.

---

## Q5. What does `ToLookup` do and how does it differ from `ToDictionary`?

**Concepts**
- Groups multiple values per key
- ILookup<TKey, TElement>
- Immediate execution
- Missing key returns empty sequence (no exception)
- Contrast with ToDictionary (unique keys required)

**Answer**

`ToLookup(keySelector)` groups elements by key, allowing multiple elements per key — it is like a `Dictionary<TKey, List<TElement>>` but with a cleaner interface. It immediately materializes into an `ILookup<TKey, TElement>` where accessing a missing key returns an empty `IEnumerable<TElement>` rather than throwing `KeyNotFoundException`. `ToDictionary`, by contrast, requires unique keys and throws on duplicates. Use `ToLookup` when the key-to-value relationship is one-to-many (multiple orders per customer, multiple items per category). The `ILookup` result is read-only and cannot be modified after creation. It is always more efficient than calling `GroupBy` multiple times, because it builds the index once and provides O(1) group access thereafter. `ToLookup` also accepts an element selector overload to transform values before storing.

---

## Q6. What does `Cast<T>` do and when does it throw?

**Concepts**
- Explicit type cast of each element
- Throws InvalidCastException on incompatible type
- Useful for non-generic IEnumerable sources
- No type filtering (use OfType for that)
- Deferred execution

**Answer**

`Cast<T>()` attempts to cast every element of a non-generic `IEnumerable` (or an `IEnumerable<object>`) to type `T`. It is deferred — casts happen as elements are enumerated. If any element cannot be cast to `T`, `InvalidCastException` is thrown at that point. Use `Cast<T>` when you are certain all elements are of the target type (e.g., an `ArrayList` filled with `int` values). When the sequence may contain mixed types and you want to silently skip non-matching elements, use `OfType<T>` instead. A common use is converting legacy non-generic collections to `IEnumerable<T>`:

```csharp
ArrayList legacyList = GetLegacyData();
var typed = legacyList.Cast<Customer>(); // throws if any element is not Customer
var safe = legacyList.OfType<Customer>(); // skips non-Customer elements
```

`Cast<T>` should not be confused with `Select(x => (T)x)` — they are semantically identical for explicit casts, but `Cast<T>` is more readable when the intent is type coercion of a non-generic source.

---

## Q7. What does `OfType<T>` do?

**Concepts**
- Type-based filter and cast
- Uses `is T` check internally
- Skips nulls and incompatible types
- Returns IEnumerable<T>
- Deferred execution

**Answer**

`OfType<T>()` filters a sequence, yielding only elements that are instances of type `T` (using `is T`), and casts them to `T`. Nulls and elements of incompatible types are silently skipped — no exception is thrown. It is equivalent to `Where(x => x is T).Cast<T>()` but expressed as a single, self-documenting operator. Use `OfType<T>` when: (1) working with non-generic `IEnumerable` sources that may contain mixed types; (2) filtering a `List<BaseType>` to only `DerivedType` instances; (3) safely extracting one type from a heterogeneous collection. For example, extracting all text nodes from a mixed XML node list: `xmlNodes.OfType<XText>()`. `OfType<T>` also works correctly with inheritance — if `T` is a base class, elements of any subclass are included.

---

## Q8. What does `AsEnumerable` do and why is it important in EF Core?

**Concepts**
- Returns IEnumerable<T> typed reference
- Forces subsequent operators to LINQ to Objects
- Does not materialize (deferred)
- Breaks IQueryable provider chain
- Provider boundary control

**Answer**

`AsEnumerable()` returns the same sequence but typed as `IEnumerable<T>` rather than `IQueryable<T>`. This is significant because subsequent LINQ operators are resolved by the `Enumerable` extension methods (in-memory) rather than the `Queryable` extension methods (provider-translated). For EF Core, calling `AsEnumerable()` before a filter forces that filter to run in C# after fetching data from the database. This breaks server-side query translation at that point. The use case is when you need to apply a LINQ operation that the EF Core provider cannot translate — for example, calling a custom C# method in a `Where` predicate. The correct pattern is to push as much filtering as possible to the provider before calling `AsEnumerable`:

```csharp
var result = _context.Orders
    .Where(o => o.Status == "Active")    // translated to SQL
    .AsEnumerable()                       // switches to LINQ to Objects
    .Where(o => MyUntranslatableMethod(o)) // runs in C#
    .ToList();
```

---

## Q9. What does `AsQueryable` do?

**Concepts**
- Returns IQueryable<T> typed reference
- Enables IQueryable-style operators on IEnumerable<T>
- Does not add provider translation
- Useful for composing generic query methods
- In-memory IQueryable wraps Enumerable

**Answer**

`AsQueryable()` wraps an `IEnumerable<T>` in a `Queryable` adapter that implements `IQueryable<T>`. When operators like `Where` or `Select` are applied to the result, they compile lambdas as `Expression<Func<T, ...>>` expression trees rather than compiled delegates. However, the default in-memory provider simply compiles these expression trees to delegates and executes them in C# — so there is no magical SQL translation from calling `AsQueryable` on a list. The primary use case is writing generic methods that accept `IQueryable<T>` for composability with both database sources and in-memory sources:

```csharp
public IQueryable<T> ApplyFilter<T>(IQueryable<T> source, ...)
// Works with both: dbContext.Orders and orders.AsQueryable()
```

This makes unit testing such methods easier — pass `list.AsQueryable()` in tests instead of a real database. Do not use `AsQueryable` on a database query result to add more SQL-translated filters — the translation happened at the `DbSet<T>` / `IQueryable<T>` level; `AsQueryable` on a list is always in-memory.

---

## Q10. What is the difference between `ToList()`, `ToArray()`, and `AsEnumerable()` in terms of execution? (Gotcha)

**Concepts**
- ToList/ToArray: immediate, materializes to collection
- AsEnumerable: deferred, only changes type view
- Common misconception that AsEnumerable materializes
- Memory implications
- When to use each

**Answer**

`ToList()` and `ToArray()` immediately enumerate the entire sequence and allocate a concrete in-memory collection — all elements are fetched and stored. `AsEnumerable()` does nothing except change the compile-time type of the reference from `IQueryable<T>` or a specific collection type to `IEnumerable<T>`. It is a deferred no-op from a data access perspective — no elements are fetched and no memory is allocated for element storage. The common mistake is using `AsEnumerable()` expecting it to "snapshot" the data like `ToList()`. Since `AsEnumerable()` is lazy, the source is still queried each time the result is enumerated. To materialize a snapshot, always use `ToList()` or `ToArray()`. Use `AsEnumerable()` solely to control which set of extension methods (provider vs. in-memory) is applied to subsequent operators.

---

## Q11. Why does `ToDictionary` throw when there are duplicate keys? (Gotcha)

**Concepts**
- Dictionary requires unique keys
- ArgumentException on second occurrence
- Use ToLookup for duplicate keys
- Conditional GroupBy+First pattern
- Debugging with key inspection

**Answer**

`ToDictionary` enforces unique key invariants — a `Dictionary<TKey, TValue>` can hold only one value per key. When a duplicate key is encountered, it throws `ArgumentException: "An item with the same key has already been added. Key: [value]"` (the key value is included in the message in modern .NET). This is intentional — it surfaces data quality issues (unexpected duplicates) rather than silently discarding them. The fix depends on intent: if duplicates are expected and you want all values per key, use `ToLookup`. If duplicates are unexpected, investigate the data source. If you want only the first occurrence, use `GroupBy(keySelector).ToDictionary(g => g.Key, g => g.First())`. If you want only the last occurrence, use `.ToDictionary(keySelector, valueSelector)` after deduplicating with `.Last()`:

```csharp
// Keep last occurrence per key
var dict = items
    .GroupBy(x => x.Code)
    .ToDictionary(g => g.Key, g => g.Last());
```

---

## Q12. Scenario: A service method calls `ToList()` early in the pipeline before filtering. What is wrong and how do you fix it? (Scenario)

**Concepts**
- Premature materialization
- Full table load into memory
- Subsequent Where runs in C# not SQL
- Performance impact
- Move ToList after all IQueryable operators

**Answer**

Calling `ToList()` before `Where` on an EF Core query loads the entire table into memory before filtering:

```csharp
// Problematic: full table loaded, then filtered in memory
public async Task<List<Product>> GetActiveExpensiveProducts()
{
    var all = await _context.Products.ToListAsync(); // loads ALL products
    return all.Where(p => p.IsActive && p.Price > 100).ToList();
}

// Fixed: filter pushed to SQL, only matching rows transferred
public async Task<List<Product>> GetActiveExpensiveProducts()
{
    return await _context.Products
        .Where(p => p.IsActive && p.Price > 100)
        .ToListAsync();
}
```

The fixed version generates `SELECT * FROM Products WHERE IsActive = 1 AND Price > 100`, transferring only matching rows. The problematic version generates `SELECT * FROM Products` (potentially millions of rows) and then discards non-matching ones in C#. In a code review, I flag any `ToList()` or `ToListAsync()` that is followed by further LINQ filtering operators, and check whether the ToList position can be moved to after all operators that are translatable to SQL.

---

## Q13. Scenario: You need to build an in-memory lookup table from a database query for use across multiple requests. What conversion operator and caching strategy do you use? (Scenario)

**Concepts**
- ToLookup or ToDictionary for indexed access
- IMemoryCache for cross-request caching
- Immediate execution at cache load time
- Cache invalidation strategy
- Thread safety of read-only collections

**Answer**

The correct pattern combines `ToDictionary` or `ToLookup` for building the indexed structure and `IMemoryCache` for cross-request caching:

```csharp
public class ProductCatalogService
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;

    public async Task<ILookup<string, Product>> GetByCategoryAsync()
    {
        return await _cache.GetOrCreateAsync("products_by_category", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();
            return products.ToLookup(p => p.Category);
        });
    }
}
```

The `ToListAsync()` materializes all active products once. `ToLookup` then builds the indexed structure in memory. `IMemoryCache` stores the `ILookup` for 15 minutes, so subsequent requests reuse the in-memory structure without hitting the database. `ILookup<TKey, TElement>` is read-only and thread-safe for concurrent reads after construction. Cache invalidation should be triggered when product data changes — either by expiry time, by a cache invalidation event, or by using a versioned cache key.

## Gotchas — Conversion Operations (Interview Traps)

---

#### Gotcha 1. `ToList()` vs `ToArray()` — Both Materialize, but `ToList` Stays Mutable

**Concepts**
- Both `ToList()` and `ToArray()` immediately enumerate the source and allocate a new collection
- `ToList()` returns a `List<T>` that supports `Add`, `Remove`, and `Insert` after creation
- `ToArray()` returns a fixed-size array; no elements can be added or removed (though existing elements can be replaced)
- `ToArray()` may be slightly more memory-efficient for a known-size result; `ToList()` over-allocates when the count is unknown

**Answer**

`ToList()` and `ToArray()` both force immediate evaluation of a deferred LINQ query, but they produce different collection types. The result of `ToList()` can be mutated after creation — elements can be added, removed, or reordered — making it the right choice when further collection manipulation is planned. `ToArray()` is the right choice when the result is read-only after creation and you want the fixed-size guarantee, or when an array is required by an API. Performance differences are negligible for most use cases.

---

#### Gotcha 2. `ToDictionary()` Throws on Duplicate Keys

**Concepts**
- `ToDictionary(keySelector)` throws `ArgumentException` if the key selector produces duplicate keys
- The exception message mentions "An item with the same key has already been added"
- Use `GroupBy(keySelector).ToDictionary(g => g.Key, g => g.ToList())` to aggregate duplicates
- Alternatively, use `ToLookup()` which handles duplicate keys natively

**Answer**

`source.ToDictionary(x => x.Category)` throws `ArgumentException` the moment two elements produce the same key, because a `Dictionary<K,V>` requires unique keys. The fix depends on intent: if only one value per key is expected, investigate the data for unexpected duplicates; if multiple values per key are valid, use `GroupBy(x => x.Category).ToDictionary(g => g.Key, g => g.ToList())` or switch to `ToLookup(x => x.Category)`, which tolerates duplicate keys by design.

---

#### Gotcha 3. `ToHashSet()` Deduplicates While Materializing — Requires Correct Equality

**Concepts**
- `ToHashSet()` creates a `HashSet<T>` and silently drops duplicate elements
- Deduplication uses `GetHashCode` and `Equals`; custom types without overrides deduplicate by reference
- `ToHashSet(IEqualityComparer<T>)` overload allows custom comparison
- The resulting `HashSet<T>` is unordered; original sequence order is not preserved

**Answer**

`source.ToHashSet()` is a concise way to deduplicate a sequence while materializing it, but it silently discards duplicates — if you expect a unique sequence and it is not, you will not get an exception, only a smaller result. For custom reference types, `GetHashCode` and `Equals` must be correctly overridden (or a `record` type used) so that structurally equal objects are treated as duplicates. Because `HashSet<T>` is unordered, the output element order is nondeterministic.

---

#### Gotcha 4. `AsEnumerable()` Does NOT Materialize — It Only Changes the Static Type

**Concepts**
- `AsEnumerable()` returns the same underlying object cast to `IEnumerable<T>`; no data is buffered
- It is used to force client-side LINQ evaluation by hiding `IQueryable<T>` from subsequent operators
- Confused with `ToList()` / `ToArray()` which actually enumerate and buffer the source
- Calling `AsEnumerable()` on a `DbSet` query causes all subsequent operators to run in memory after loading rows

**Answer**

`dbSet.Where(serverFilter).AsEnumerable().Where(clientFilter)` applies `serverFilter` in SQL and then loads all matching rows into memory before applying `clientFilter` in C#. The `AsEnumerable()` call itself does no work — it just prevents EF Core from translating the second `Where` to SQL. Developers who confuse `AsEnumerable()` with `ToList()` may expect data to be buffered at that point, but enumeration is still deferred until the final consumer iterates the result.

---

#### Gotcha 5. `AsQueryable()` on an In-Memory Collection Does NOT Push Queries to a Database

**Concepts**
- `AsQueryable()` wraps an in-memory `IEnumerable<T>` as `IQueryable<T>` using `EnumerableQuery<T>`
- It does not connect the collection to any database or ORM provider
- Subsequent LINQ operators run in-memory using the LINQ-to-Objects provider
- `AsQueryable()` is useful for writing provider-agnostic query code in tests but provides no database access

**Answer**

`list.AsQueryable().Where(x => x.Active)` runs a standard in-memory LINQ-to-Objects query — no SQL is generated because `list` is an `IEnumerable<T>`, not a real `IQueryable<T>` backed by a database provider. The common misconception is that calling `AsQueryable()` on a collection somehow enables ORM features; it does not. Its real value is enabling expression-tree-based query composition in test doubles or generic repository abstractions where the calling code accepts `IQueryable<T>`.

---

#### Gotcha 6. `Cast<T>()` Throws on Incompatible Types — Use `OfType<T>()` to Filter Safely

**Concepts**
- `Cast<T>()` attempts to cast every element; throws `InvalidCastException` on the first incompatible element
- `OfType<T>()` silently skips elements that cannot be cast to `T`, returning only compatible elements
- `Cast<T>()` is appropriate when all elements are guaranteed to be of type `T`
- `OfType<T>()` is the safe alternative when the source may contain mixed types

**Answer**

`objects.Cast<string>()` throws `InvalidCastException` at the element that is not a `string`; iteration succeeds up to that point and then fails. `objects.OfType<string>()` returns only the elements that are already `string` (or a subtype), silently skipping everything else. Use `Cast` when you have a contract that all elements are of the target type and want an immediate error if violated; use `OfType` when heterogeneous input is expected and filtering is correct.

---

#### Gotcha 7. `ToLookup()` Materializes Immediately — Missing Keys Return Empty Sequences

**Concepts**
- `ToLookup()` enumerates the source immediately and builds the in-memory `ILookup<K,V>` structure
- Accessing a key that does not exist returns an empty `IEnumerable<V>`, not `null` and not an exception
- Unlike `Dictionary`, `ILookup` natively handles multiple values per key
- `ILookup` is read-only; elements cannot be added or removed after creation

**Answer**

`lookup["missing-key"]` returns an empty sequence rather than throwing `KeyNotFoundException` — this is intentional and makes `ToLookup` convenient for grouping without defensive null checks. Because `ToLookup` materializes immediately, it is suitable for in-memory caches that are built once and queried many times. For scenarios requiring deferred or incremental grouping, `GroupBy` is the alternative, but `GroupBy` does not give the non-throwing key-access behaviour of `ILookup`.

---

#### Gotcha 8. `ToList().AsReadOnly()` Returns a Live View, Not a Copy

**Concepts**
- `list.AsReadOnly()` returns a `ReadOnlyCollection<T>` wrapping the same underlying `List<T>`
- Mutations to the original `List<T>` are immediately visible through the `ReadOnlyCollection<T>`
- `ImmutableList<T>` (from `System.Collections.Immutable`) creates a true independent copy
- Use `AsReadOnly()` for lightweight read-only exposure; use `ImmutableList` when true immutability is required

**Answer**

`list.AsReadOnly()` is a thin wrapper — it blocks direct mutations through the `ReadOnlyCollection<T>` reference but the underlying `List<T>` can still be mutated by whoever holds a reference to it, and those mutations are immediately visible through the wrapper. This is a common source of bugs in code that passes `AsReadOnly()` with the intent to prevent all future changes. `ImmutableList<T>` creates a structurally independent copy that cannot be changed by any reference; it is the correct choice when true immutability is required.

---

#### Gotcha 9. `Enumerable.Empty<T>()` Returns a Cached Singleton — Prefer Over `new T[0]`

**Concepts**
- `Enumerable.Empty<T>()` returns the same cached empty `IEnumerable<T>` instance on every call
- Avoids heap allocation of a new empty array or list each time a "no results" value is needed
- Callers receive a zero-element sequence that can be enumerated safely with `foreach`
- `new T[0]` and `Array.Empty<T>()` also avoid allocation (both are cached), but `Enumerable.Empty<T>()` signals intent

**Answer**

`return Enumerable.Empty<Order>();` is the idiomatic way to return a zero-element sequence from a method with an `IEnumerable<T>` return type. It allocates nothing (the runtime caches one instance per type parameter) and communicates intent clearly: the method found no results. Returning `null` instead forces every caller to null-check before iterating and is an anti-pattern; returning `new List<Order>()` allocates a list object unnecessarily.

---

#### Gotcha 10. `ToDictionary` Triggers One Full Enumeration — Double-Enumeration Trap

**Concepts**
- `ToDictionary()` enumerates the source once during construction — if the source is a deferred query, it executes once
- The constructed `Dictionary` is fully in-memory; subsequent reads are O(1) lookups with no re-enumeration
- If the source is a deferred `IEnumerable` wrapped in a variable, calling `ToDictionary` on it elsewhere re-runs the query
- Always materialize deferred queries before passing them to `ToDictionary` when reuse is needed

**Answer**

`var dict = source.ToDictionary(x => x.Id)` executes the underlying query exactly once to populate the dictionary; after that, all lookups are O(1) in memory with no further enumeration. The trap arises when the same deferred `source` variable is used elsewhere after the `ToDictionary` call — each independent enumeration re-executes the query from the start, potentially reading different data if the underlying source is non-deterministic or side-effectful. Materialize with `ToList()` first if the source needs to be enumerated more than once.

---
