# LINQ: Filtering & Aggregation — Interview Q&A

---

## Q1. What does the `Where` operator do and what is its signature?

**Concepts**
- Filter operator
- Predicate delegate `Func<T, bool>`
- Deferred execution
- Returns IEnumerable<T> of same element type
- Index-aware overload

**Answer**

`Where` filters a sequence by retaining only elements for which a predicate returns `true`. Its standard signature is `Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate)`, returned as a new deferred `IEnumerable<TSource>`. The operator evaluates the predicate lazily — no element is tested until enumeration begins — and the result sequence contains only those elements that passed the test, in their original order. An overloaded form accepts `Func<TSource, int, bool>`, where the integer parameter carries the zero-based index of the current element, allowing position-aware filtering such as `list.Where((x, i) => i % 2 == 0)` to keep every other element. Because `Where` returns the same element type, multiple `Where` calls can be chained, and they are logically equivalent to a single `Where` with a compound predicate joined by `&&` — though chaining can improve readability when each condition is conceptually distinct.

---

## Q2. What is the difference between `Where` and `OfType<T>`?

**Concepts**
- Type-based filtering
- Null-safe type check
- OfType uses `is` operator
- Cast vs. OfType exception behavior
- Non-generic IEnumerable source

**Answer**

`Where` filters elements based on any boolean predicate you provide, while `OfType<T>` filters a sequence by retaining only elements that are instances of type `T`, silently discarding nulls and elements of incompatible types. `OfType<T>` is equivalent to `Where(x => x is T).Cast<T>()` but written as a single operator. It is particularly useful when working with non-generic `IEnumerable` sources (such as `ArrayList` or `Control.Controls`) that may contain mixed types, or when a collection holds elements of a base type and you want only a specific derived type. By contrast, `Cast<T>` throws `InvalidCastException` for any element that cannot be cast to `T`, making it appropriate only when you are certain all elements are of the target type. Using `Where(x => x is T).Cast<T>()` is equivalent to `OfType<T>()` but more verbose; prefer `OfType<T>()` for clarity.

---

## Q3. How does `Count()` differ from `Count(predicate)`?

**Concepts**
- No-argument Count
- Predicate overload as filtered count
- Short-circuit not available (full enumeration)
- ICollection<T> optimization
- Equivalent to Where().Count()

**Answer**

`Count()` with no arguments returns the total number of elements in the sequence. When the source implements `ICollection<T>` (such as `List<T>` or arrays), the implementation uses the `Count` property directly — an O(1) operation. For plain `IEnumerable<T>`, it iterates the entire sequence. `Count(predicate)` returns the count of elements satisfying the predicate; it is exactly equivalent to `Where(predicate).Count()` but expressed as one call. Neither form short-circuits — the predicate version must examine every element to produce the correct count, unlike `Any(predicate)` which stops at the first match. A common performance mistake is writing `if (list.Count() == 0)` instead of `if (!list.Any())` — the former iterates the entire sequence (unless the source is a concrete collection), while `Any()` stops as soon as it confirms the sequence is non-empty.

---

## Q4. What does `Sum` do and what types does it support?

**Concepts**
- Aggregate summation
- Overloads for int, long, float, double, decimal, nullable variants
- Selector lambda overload
- Empty sequence returns zero
- Overflow risk with integer types

**Answer**

`Sum` computes the arithmetic sum of the numeric values in a sequence. The standard LINQ library provides overloads for `int`, `long`, `float`, `double`, `decimal`, and their nullable counterparts (`int?`, `decimal?`, etc.). Nullable overloads skip `null` elements and return `null` only if every element is `null`; if any non-null value exists, they return the sum of non-null values. The selector overload `Sum(x => x.Price)` projects each element to a numeric value and sums the results, combining a `Select` and a `Sum` into one call. Importantly, `Sum` returns `0` (or `0m`, etc.) for an empty sequence rather than throwing. When using the `int` overload on large datasets, overflow is a real risk — consider `Sum(x => (long)x.Count)` or use `decimal` for financial calculations. On `IQueryable<T>` providers, `Sum` is translated to `SUM(...)` in SQL.

---

## Q5. What does `Average` return for an empty sequence?

**Concepts**
- InvalidOperationException on empty non-nullable sequence
- Nullable overload returns null for empty
- Arithmetic mean calculation
- Return type promotion (int Average → double)
- Use DefaultIfEmpty to guard

**Answer**

`Average` on a non-nullable numeric sequence (e.g., `IEnumerable<int>`) throws `InvalidOperationException` with the message "Sequence contains no elements" when the source is empty. The nullable overloads (`Average` on `IEnumerable<int?>`) return `null` instead of throwing. The return type of `Average` promotes integer inputs to `double` — `Enumerable.Average(IEnumerable<int>)` returns `double`, not `int` — which surprises developers expecting truncating integer division. To safely compute an average over a potentially empty sequence of non-nullable values, use `DefaultIfEmpty(0)` first: `numbers.DefaultIfEmpty(0).Average()`, though this returns `0` for an empty sequence which may or may not be semantically correct for your use case. Alternatively, check `numbers.Any()` before calling `Average`. On `IQueryable<T>`, the provider translates `Average` to `AVG(...)` in SQL, which already returns `NULL` for an empty set.

---

## Q6. What does `Min` and `Max` return for an empty sequence?

**Concepts**
- InvalidOperationException on empty non-nullable sequence
- Nullable overloads return null for empty
- MinBy / MaxBy (.NET 6+)
- Custom IComparer<T> overload
- Use with value types vs. reference types

**Answer**

`Min()` and `Max()` on non-nullable numeric sequences throw `InvalidOperationException` when the source is empty. Their nullable variants return `null` for empty sequences. For reference types (e.g., `IEnumerable<string>`), `Min()` and `Max()` use the default comparer and return `null` rather than throwing when the sequence is empty — this is a subtle inconsistency compared to numeric types. In .NET 6+, `MinBy(keySelector)` and `MaxBy(keySelector)` return the element (not the key) whose key is minimum or maximum, returning `null` for an empty sequence of reference types (or the default for value types). These replace the common pattern of `OrderBy(x => x.Date).First()` with the more efficient `MinBy(x => x.Date)`, which avoids a full sort when only one element is needed.

---

## Q7. What is the `Aggregate` operator and when would you use it?

**Concepts**
- General-purpose fold/reduce
- Accumulator function
- Seed value overload
- Result selector overload
- Equivalent to manual foreach accumulation

**Answer**

`Aggregate` is LINQ's general-purpose fold (reduce) operator, equivalent to iterating a sequence and accumulating a result in a variable. Its simplest form `Aggregate(func)` uses the first element as the initial accumulator and then applies `func(accumulator, element)` for each subsequent element — it throws on an empty sequence. The seeded overload `Aggregate(seed, func)` starts with an explicit seed value and is safe for empty sequences (it simply returns the seed). The three-argument overload adds a result selector applied to the final accumulator: `Aggregate(seed, func, resultSelector)`. For example, concatenating strings with a separator: `words.Aggregate((a, b) => a + ", " + b)`. `Aggregate` is the building block for operators like `Sum`, `Max`, and `Count`, but it should be reserved for cases where no more specific operator exists, since operators like `Sum` have optimized implementations. Use `Aggregate` for custom accumulations such as computing a running product, building a custom string format, or folding into a complex data structure.

---

## Q8. How does chaining multiple `Where` conditions compare to a single `Where` with a compound predicate?

**Concepts**
- Multiple Where calls vs. && in one predicate
- Compiled output identity
- Readability trade-off
- Iterator overhead (minor)
- Short-circuit evaluation preserved

**Answer**

Chaining `Where(x => x.Active).Where(x => x.Age > 18)` is logically equivalent to `Where(x => x.Active && x.Age > 18)` — both produce the same filtered result. The compiler does not automatically merge chained `Where` calls, so at runtime there are two iterator objects wrapping the source instead of one. This adds a tiny allocation and indirection overhead per element, which is negligible for most workloads but measurable in high-frequency, tight-loop code. Both forms preserve short-circuit evaluation: in the chained form, if the first `Where` predicate is false the element never reaches the second iterator; in the compound predicate, `&&` also short-circuits. The practical choice is readability — use a single `Where` with `&&` for closely related conditions, and chain when the conditions are conceptually independent or when you are composing query fragments from separate methods.

---

## Q9. What is `LongCount` and when should you use it instead of `Count`?

**Concepts**
- Returns long vs. int
- Sequences exceeding int.MaxValue
- IQueryable<T> COUNT(*) translation
- Rare practical need
- Count overflow risk

**Answer**

`LongCount` returns a `long` (64-bit integer) instead of the `int` that `Count` returns. It exists for sequences whose element count could exceed `int.MaxValue` (~2.1 billion). For in-memory collections this is rarely a concern, but for database queries against tables with billions of rows, `Count()` would overflow and return a negative number — `LongCount()` handles these cases correctly. On `IQueryable<T>`, both translate to `COUNT(*)` in SQL, but the return type differs: `Count` maps the result back to `int` and can throw if the result exceeds `int.MaxValue`, while `LongCount` returns a `long`. In practice, most applications never reach this scale on a single query, so `Count` is fine for the vast majority of use cases. When working with very large datasets where counts are reported to users or used in calculations, prefer `LongCount` defensively.

---

## Q10. How do you compute a conditional aggregate, like the count of active users?

**Concepts**
- Count with predicate overload
- Where().Count() equivalence
- Sum with ternary for conditional summing
- Multiple aggregates in one pass
- Aggregate operator for custom results

**Answer**

The most direct approach is `users.Count(u => u.IsActive)`, using the predicate overload of `Count`. This is exactly equivalent to `users.Where(u => u.IsActive).Count()` but avoids creating the intermediate `Where` iterator. For conditional sums — "total revenue from completed orders" — use `orders.Where(o => o.Status == "Completed").Sum(o => o.Total)`, again filtering before aggregating to clarify intent. For computing multiple aggregates in a single pass through the data without multiple enumerations, use `Aggregate` with a tuple accumulator:

```csharp
var (activeCount, total) = users.Aggregate(
    (Count: 0, Total: 0m),
    (acc, u) => (
        acc.Count + (u.IsActive ? 1 : 0),
        acc.Total + u.Balance));
```

This iterates the sequence once to compute both values. On `IQueryable<T>`, multiple aggregates in a single query can sometimes be expressed with a `GroupBy` followed by a projection, but for in-memory collections the `Aggregate` single-pass approach is the most efficient.

---

## Q11. What does `Any` do compared to `Count() > 0`?

**Concepts**
- Short-circuit evaluation
- Any returns bool on first match
- Count iterates full sequence
- Performance difference on large sequences
- Any with predicate vs. no predicate

**Answer**

`Any()` returns `true` as soon as it finds a single element in the sequence, stopping iteration immediately — it short-circuits. `Count() > 0` iterates the entire sequence, counting every element before returning. For a large sequence where the first element satisfies the condition, `Any` performs O(1) work while `Count() > 0` performs O(n) work. The difference is stark for `IEnumerable<T>` sources; for `ICollection<T>` sources like `List<T>`, `Count` reads the `.Count` property in O(1), but `Any()` is still the idiomatic choice because it communicates intent (does any element exist?) rather than implementation (what is the cardinality?). `Any(predicate)` similarly short-circuits after the first element satisfying the predicate, making it far more efficient than `Count(predicate) > 0` for large sequences. The analyzer CA1827 in .NET code analyzers specifically flags `Count() > 0` patterns and suggests replacing them with `Any()`.

---

## Q12. What does the `All` operator do and what does it return for an empty sequence?

**Concepts**
- Universal quantifier
- Returns true for empty sequences (vacuous truth)
- Short-circuits on first false
- Complement with Any and negation
- Use in validation scenarios

**Answer**

`All(predicate)` returns `true` if every element in the sequence satisfies the predicate. It short-circuits as soon as a predicate returns `false`, so it can be O(1) in the best case and O(n) in the worst case. The important edge case is that `All` returns `true` for an empty sequence — this is mathematically correct (vacuous truth) but often surprises developers who expect it to return `false`. A common validation bug is `if (users.All(u => u.IsValid))` when `users` could be empty — the check passes vacuously, allowing downstream code to run on an empty set. The fix is to check `users.Any() && users.All(u => u.IsValid)`. The logical complement of `All` is `!All(!predicate)` which equals `Any(predicate)`, and vice versa. In business logic, `All` maps naturally to constraints like "all line items must have a positive price" or "all approvers have reviewed".

---

## Q13. Explain `Min` and `Max` with a selector lambda.

**Concepts**
- Selector overload projects to comparable value
- Combined Select+Min into one call
- Returns projected value, not original element
- MinBy/MaxBy for returning the element (.NET 6+)
- Type must implement IComparable<T>

**Answer**

`Min(selector)` and `Max(selector)` accept a `Func<TSource, TResult>` that projects each element to a comparable value, then returns the minimum or maximum of those projected values. For example, `products.Min(p => p.Price)` returns a `decimal` (the smallest price), not the `Product` with the smallest price. This is equivalent to `products.Select(p => p.Price).Min()` but expressed as one call. A critical distinction: if you want the entire element with the minimum key (e.g., the cheapest product), you need `MinBy(p => p.Price)` (available since .NET 6) which returns the `Product`, not the price. Before .NET 6, the workaround was `products.OrderBy(p => p.Price).First()` — correct but O(n log n) instead of O(n). The selector lambda must project to a type that implements `IComparable<T>`, and the result type is inferred from the lambda's return type.

---

## Q14. How does the `Aggregate` operator with a seed handle empty sequences?

**Concepts**
- Seed initializes accumulator
- No-seed version throws on empty
- Seed version returns seed for empty
- Result selector applied to final accumulator
- Safe default for empty collections

**Answer**

When `Aggregate` is called without a seed — `source.Aggregate(func)` — it uses the first element as the initial accumulator value and throws `InvalidOperationException` if the source is empty. The seeded overload — `source.Aggregate(seed, func)` — initializes the accumulator to `seed` and, when the source is empty, immediately returns `seed` without executing the accumulator function at all. This makes the seeded form safe for empty sequences. For example, `numbers.Aggregate(0, (sum, x) => sum + x)` returns `0` for an empty list, while `numbers.Aggregate((a, b) => a + b)` throws. The three-argument form `Aggregate(seed, func, resultSelector)` applies a final transformation to the accumulated result; this is useful when the accumulator type differs from the desired output type (e.g., accumulating into a `StringBuilder` and then calling `.ToString()` as the result selector).

---

## Q15. What is the difference between `Sum` on `IEnumerable<int>` and `IEnumerable<int?>`?

**Concepts**
- Nullable value type handling
- Null elements skipped in nullable overload
- Non-nullable Sum includes every element
- Returns null only if all elements are null
- Practical use with optional numeric columns

**Answer**

`Sum` on `IEnumerable<int>` (non-nullable) processes every element and adds them together; null values cannot appear in this sequence. `Sum` on `IEnumerable<int?>` (nullable) skips any `null` elements — they are treated as if they were not in the sequence — and returns `int?`. If the sequence is empty or contains only `null` values, the nullable `Sum` returns `null`. If at least one non-null value exists, it returns the sum of non-null values as a non-null `int?`. This mirrors the behavior of SQL `SUM(nullable_column)`: it ignores `NULL` rows. The distinction matters when working with domain models that have optional numeric properties — for example, a `decimal? DiscountAmount` column. Calling `orders.Sum(o => o.DiscountAmount)` on the nullable version gives you the total of all discounts where a discount was specified, ignoring orders without a discount, rather than forcing you to coerce nulls to zero first.

---

## Q16. Can you use multiple aggregate operators in a single LINQ query?

**Concepts**
- Multiple aggregates require multiple enumerations normally
- Aggregate with tuple accumulator for single pass
- GroupBy(constant) trick
- ToList materialization before multiple aggregates
- Performance implications

**Answer**

Standard LINQ does not have a built-in operator that computes multiple aggregates in a single pass. The idiomatic approach is to materialize the sequence with `ToList` or `ToArray` and then call separate aggregate operators on the in-memory list — each call iterates the list, but list iteration is cheap and the data is already loaded. For expensive sources (database calls, file reads), use the `Aggregate` operator with a tuple accumulator to fold everything in one pass: `source.Aggregate((Count: 0, Sum: 0m, Max: decimal.MinValue), (acc, x) => (acc.Count + 1, acc.Sum + x.Amount, Math.Max(acc.Max, x.Amount)))`. On `IQueryable<T>`, EF Core can sometimes batch multiple aggregates when using `GroupBy` with a constant key followed by a `Select` projecting multiple aggregate calls — this generates a single SQL `SELECT COUNT(*), SUM(...), MAX(...)` query.

---

## Q17. What happens when `Where` is called on a null sequence?

**Concepts**
- ArgumentNullException from source being null
- Defensive null checks before LINQ
- Null coalescing with Enumerable.Empty<T>()
- Extension method null source behavior
- Null-safety patterns

**Answer**

If the source sequence is `null`, calling `Where` (or any LINQ extension method) throws `ArgumentNullException` with the parameter name `source`. This is because the LINQ extension methods in `Enumerable` validate their arguments and throw immediately when `source` is null. The exception is thrown during the `Where` call itself for some operators, though for deferred operators it may be thrown during the first call to `MoveNext()` depending on the implementation. The defensive pattern is to substitute `null` sources with an empty sequence using the null-coalescing operator: `(collection ?? Enumerable.Empty<T>()).Where(...)`. A more robust approach is to ensure that collection-typed properties and return values never return `null` — return `Array.Empty<T>()`, `Enumerable.Empty<T>()`, or an empty `List<T>` instead. This eliminates the defensive pattern throughout the codebase.

---

## Q18. How do `Min`/`Max` work with reference types that don't implement IComparable?

**Concepts**
- Requires IComparable<T> or IComparable
- InvalidOperationException for non-comparable types
- Custom IComparer<T> not available for Min/Max directly
- MinBy/MaxBy with key selector as workaround
- OrderBy().First() alternative

**Answer**

`Min()` and `Max()` without a selector on a sequence of custom reference types require that `T` implements `IComparable<T>` or `IComparable`. If `T` does not implement either, a `NullReferenceException` or `InvalidOperationException` is thrown at runtime when comparison is attempted. The `Min`/`Max` operators do not accept a custom `IComparer<T>` — unlike `OrderBy` which does. The workaround before .NET 6 was `sequence.OrderBy(x => x.SomeKey).First()` or `sequence.Select(x => x.SomeKey).Min()` (projecting to a comparable type). Since .NET 6, `MinBy(keySelector)` and `MaxBy(keySelector)` provide a clean solution: they take a key selector, use the default comparer for the key type, and return the element with the minimum/maximum key — no `IComparable` required on the element type itself. This is both cleaner and more efficient (O(n) instead of O(n log n) for sort-based approaches).

---

## Q19. Why does `Count()` on a LINQ query over a `List<T>` return 0 after modifying the source list? (Gotcha)

**Concepts**
- Deferred execution re-evaluates source
- Snapshot vs. live reference
- When to materialize
- Mutation during enumeration
- IEnumerable<T> is a live view

**Answer**

This is a deferred-execution gotcha. A LINQ query variable holds a live reference to the source, not a snapshot of it. If you define a filtered query over a `List<T>` and then modify the list (remove all elements, for example), the query will reflect the modification the next time it is enumerated because it pulls from the original list on each evaluation. Developers expect the query to capture the state at definition time, but LINQ's lazy design means each enumeration re-executes against the current state of the source. The fix for scenarios requiring a snapshot is to materialize immediately: `var results = list.Where(x => x.Active).ToList()`. After this, `results` is an independent list that is unaffected by subsequent changes to `list`. Conversely, when you *want* a live view that reflects mutations (for example, a filtered view of a binding source in UI code), a deferred query is exactly the right tool — just be intentional about which behavior you need.

---

## Q20. Why does `Count(predicate)` not short-circuit? (Gotcha)

**Concepts**
- Count must examine all elements
- Contrast with Any (short-circuits)
- Performance implication for large sequences
- Common mistake: checking for any match
- Correct alternative: Any(predicate)

**Answer**

`Count(predicate)` cannot short-circuit because it needs to count *all* elements satisfying the predicate, so it must examine every element regardless of how many matches it finds. This is correct behavior — the return value is a total count. The gotcha arises when a developer uses `Count(predicate) > 0` to check whether *any* match exists, intending only to verify existence. This forces a full traversal when `Any(predicate)` would stop at the very first match. For sequences of millions of elements, the difference between O(1) (early match with `Any`) and O(n) (`Count`) is significant. The fix is straightforward: replace `items.Count(x => x.IsError) > 0` with `items.Any(x => x.IsError)`. Static analysis tools (Roslyn analyzers, SonarQube) flag this pattern automatically. Similarly, `Count(predicate) == 0` should become `!Any(predicate)`, and `Count(predicate) == 1` in many cases should become `Single(predicate)` or a more intentional check.

---

## Q21. What happens when you call `Sum` on a sequence with very large values? (Gotcha)

**Concepts**
- Integer overflow wraps around
- No checked arithmetic in LINQ Sum
- Use long or decimal for large sums
- Sum on empty returns zero (no exception)
- Floating-point precision loss

**Answer**

`Sum` on `IEnumerable<int>` uses unchecked arithmetic, meaning that if the sum overflows `int.MaxValue` (2,147,483,647), the result wraps around to a negative number without throwing an exception. This silent integer overflow is a serious bug in financial or counting code dealing with large datasets. The fix is to use `Sum(x => (long)x.Value)` to cast each element to `long` before summing, keeping the result in the `long` range. For monetary values, always use `decimal` — both to avoid overflow and to prevent the floating-point rounding errors that affect `float` and `double` sums. Consider `100_000 * 100_000 = 10_000_000_000`, which overflows `int` but fits comfortably in `long`. As a defensive practice, prefer `LongCount` and `long`-typed `Sum` when dealing with potentially large datasets, even if current data is well within `int` range, to guard against future data growth.

---

## Q22. Scenario: A report endpoint is counting records with `Count()` but the page sometimes loads slowly. What might be wrong? (Scenario)

**Concepts**
- IQueryable<T> being materialized before Count
- N+1 query pattern
- AsEnumerable forcing in-memory evaluation
- Proper SQL COUNT(*) via IQueryable.Count()
- Database index on filtered column

**Answer**

The most likely cause is that the query is being materialized into memory before `Count` is called, turning a single `COUNT(*)` database query into a full table scan plus data transfer. This happens when the query chain crosses the `IQueryable`/`IEnumerable` boundary before the aggregation:

```csharp
// Slow: loads all rows into memory, then counts in C#
var count = _context.Orders
    .Where(o => o.Status == "Pending")
    .AsEnumerable()   // <-- breaks the IQueryable chain
    .Count();

// Fast: single SQL COUNT(*) WHERE Status='Pending'
var count = _context.Orders
    .Where(o => o.Status == "Pending")
    .Count();
```

A second possibility is that `Count()` is being called inside a loop, issuing one database query per loop iteration (N+1). A third possibility is that the `Status` column lacks a database index, causing a full table scan even with a correctly formed SQL `COUNT(*)`. In a code review I would check: (1) that no `AsEnumerable`, `ToList`, or `AsQueryable` call precedes the aggregate; (2) that `Count` is not inside a loop; and (3) that an index exists on the `Status` column in the database schema.

---

## Q23. Scenario: You need to compute average order value per customer in a single database round-trip. How do you write this with LINQ? (Scenario)

**Concepts**
- GroupBy with aggregation projection
- Single SQL query with GROUP BY and AVG
- IQueryable<T> provider translation
- Anonymous type for projection
- ToList for materialization

**Answer**

Using `GroupBy` with a nested aggregation in the `Select` projection allows EF Core to generate a single `GROUP BY` SQL query:

```csharp
var avgByCustomer = await _context.Orders
    .Where(o => o.Date >= DateTime.UtcNow.AddMonths(-12))
    .GroupBy(o => o.CustomerId)
    .Select(g => new CustomerAvgDto
    {
        CustomerId = g.Key,
        AverageOrderValue = g.Average(o => o.Total),
        OrderCount = g.Count()
    })
    .OrderByDescending(x => x.AverageOrderValue)
    .ToListAsync();
```

EF Core translates this to `SELECT CustomerId, AVG(Total), COUNT(*) FROM Orders WHERE Date >= @cutoff GROUP BY CustomerId ORDER BY AVG(Total) DESC`. All computation happens in the database — no data is transferred to the application until the final result set is received. The `Where` before `GroupBy` pushes the date filter into the query, reducing the rows the database must group. If `CustomerAvgDto` has columns matching the projection, EF Core maps them directly. Avoid calling `GroupBy` after `ToList` — that would group in memory after loading all rows.

---

## Q24. Scenario: A developer writes `items.Where(x => x.Score > 0).Sum(x => x.Score)`. Is this optimal? (Scenario)

**Concepts**
- Filter before Sum vs. Sum with conditional
- Iterator overhead of Where+Sum chain
- Sum(selector with condition) alternative
- Semantic correctness with negative scores
- Performance for large in-memory sequences

**Answer**

The code is correct and idiomatic. Whether it is optimal depends on context. The chain creates two iterator objects — `Where` and `Sum` — and passes each positive-score element through both. An alternative is `items.Sum(x => x.Score > 0 ? x.Score : 0)`, which uses a single iterator with a conditional in the selector, eliminating the intermediate `Where` iterator. For large in-memory sequences in hot-path code, the single-pass version has marginally lower overhead due to fewer state machine transitions and one fewer allocation. However, the difference is typically in nanoseconds per element and rarely justifies sacrificing readability. On `IQueryable<T>` sources, both forms translate to equivalent SQL: `SUM(CASE WHEN Score > 0 THEN Score ELSE 0 END)`, so there is no difference. Choose the form that makes the intent clearest in context — the `Where` chain clearly separates the filter concern from the aggregation concern and is preferred for readability in most team settings.

---

## Q25. Scenario: A nightly job aggregates daily sales data using LINQ but throws `InvalidOperationException: Sequence contains no elements` in production. How do you fix it? (Scenario)

**Concepts**
- Empty sequence behavior of Min/Max/Average/First
- OrDefault and DefaultIfEmpty patterns
- Guard with Any before aggregation
- Business default vs. technical default
- Logging and alerting before fixing

**Answer**

The exception indicates one of the non-safe aggregate operators (`Min`, `Max`, `Average`, `First`, or `Single` without `OrDefault`) was called on an empty sequence. Operators like `Sum` and `Count` return `0` for empty sequences, but `Min`, `Max`, `Average`, and `First` throw. The fix depends on the business semantics:

```csharp
// Before fix — throws if no sales today
var maxSale = dailySales.Max(s => s.Amount);

// Option 1 — return null and handle null downstream
decimal? maxSale = dailySales.Any()
    ? dailySales.Max(s => s.Amount)
    : null;

// Option 2 — default to zero
decimal maxSale = dailySales.Select(s => s.Amount)
    .DefaultIfEmpty(0m)
    .Max();

// Option 3 — nullable sequence always safe
decimal? maxSale = dailySales
    .Select(s => (decimal?)s.Amount)
    .Max();
```

Option 3 is the most concise — casting to `decimal?` and calling `Max()` on the nullable overload returns `null` for an empty sequence rather than throwing. Choose the option whose default value makes sense in the report: returning zero where no sales occurred may produce misleading reports, while `null` allows downstream logic to show "N/A" or skip the day.
