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
