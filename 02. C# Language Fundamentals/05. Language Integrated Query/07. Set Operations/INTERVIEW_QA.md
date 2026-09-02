# LINQ: Set Operations — Interview Q&A

---


## Table of Contents

1. [Q1. What does `Distinct` do and how does it determine equality?](#q1-what-does-distinct-do-and-how-does-it-determine-equality)
2. [Q2. What is `DistinctBy` and when was it introduced?](#q2-what-is-distinctby-and-when-was-it-introduced)
3. [Q3. What does `Union` do and how does it differ from `Concat`?](#q3-what-does-union-do-and-how-does-it-differ-from-concat)
4. [Q4. What does `Intersect` do?](#q4-what-does-intersect-do)
5. [Q5. What does `Except` do?](#q5-what-does-except-do)
6. [Q6. What are `UnionBy`, `IntersectBy`, and `ExceptBy` (.NET 6+)?](#q6-what-are-unionby-intersectby-and-exceptby-net-6)
7. [Q7. How does `Distinct` behave with custom reference types that don't override `Equals`?](#q7-how-does-distinct-behave-with-custom-reference-types-that-dont-override-equals)
8. [Q8. What is the memory and time complexity of `Distinct`, `Union`, `Intersect`, and `Except`?](#q8-what-is-the-memory-and-time-complexity-of-distinct-union-intersect-and-except)
9. [Q9. Does `Union` guarantee element order?](#q9-does-union-guarantee-element-order)
10. [Q10. How does `SequenceEqual` differ from `Intersect`?](#q10-how-does-sequenceequal-differ-from-intersect)
11. [Q11. Scenario: A sync service compares two lists of IDs to find items added, removed, and unchanged. How do you implement this with set operations? (Scenario)](#q11-scenario-a-sync-service-compares-two-lists-of-ids-to-find-items-added-removed-and-unchanged-how-do-you-implement-this-with-set-operations-scenario)
12. [Q12. Scenario: A developer uses `Distinct()` on a list of DTOs and complains it doesn't work. What do you tell them? (Scenario)](#q12-scenario-a-developer-uses-distinct-on-a-list-of-dtos-and-complains-it-doesnt-work-what-do-you-tell-them-scenario)
13. [Q13. When would you use `Except` instead of `Where` with `Contains`? (Scenario)](#q13-when-would-you-use-except-instead-of-where-with-contains-scenario)

---
## Q1. What does `Distinct` do and how does it determine equality?

**Concepts**
- Removes duplicate elements
- Default equality comparer
- IEqualityComparer<T> overload
- Deferred execution
- Reference equality for classes without override

**Answer**

`Distinct()` filters a sequence to return only unique elements, discarding duplicates. It uses the default equality comparer for the element type (`EqualityComparer<T>.Default`), which calls `Equals` and `GetHashCode` on each element. For value types and strings, this compares by value. For custom reference types that do not override `Equals`, it falls back to reference equality — two different objects with the same data are considered distinct. An overloaded form `Distinct(IEqualityComparer<T> comparer)` accepts a custom comparer for custom equality semantics. Execution is deferred but partially buffering: `Distinct` maintains an internal `HashSet<T>` as it yields elements, adding each element to the set; if an element is already in the set, it is skipped. This means memory usage grows proportionally to the number of unique elements. `Distinct` preserves the first occurrence of each unique element in the original sequence order.

---

## Q2. What is `DistinctBy` and when was it introduced?

**Concepts**
- .NET 6+ operator
- Key selector for uniqueness
- Preserves first element per key
- Avoids Select+Distinct+re-Select roundtrip
- Returns distinct elements, not distinct keys

**Answer**

`DistinctBy(keySelector)` was introduced in .NET 6 and returns the distinct elements of a sequence based on a key projection rather than the element itself. It retains the first element for each unique key value and discards subsequent duplicates. For example, `products.DistinctBy(p => p.Category)` returns one product per category — the first product encountered for each category. Before .NET 6, the equivalent required a more verbose workaround: `products.GroupBy(p => p.Category).Select(g => g.First())`. `DistinctBy` also accepts an `IEqualityComparer<TKey>` overload for custom key comparison. The critical difference from `Distinct()` is that `DistinctBy` returns the original elements (the full `Product` object), not the projected keys. This makes it ideal for deduplicating a sequence while retaining the full element data.

---

## Q3. What does `Union` do and how does it differ from `Concat`?

**Concepts**
- Set union removes duplicates
- Concat appends without deduplication
- Two sequences merged
- Default equality comparer
- Deferred execution

**Answer**

`Union(second)` returns the set union of two sequences: all distinct elements from both sequences combined, with duplicates removed across both inputs. `Concat(second)` appends all elements of the second sequence to the first without removing any duplicates. If both sequences contain the element `5`, `Union` produces it once while `Concat` produces it twice. `Union` uses the default equality comparer (or a custom one if provided) to determine duplicates. The result order is: first, elements from the first sequence in their original order (first occurrence of each); then elements from the second sequence that were not already in the first, in their original order. `Concat` preserves order strictly: all first-sequence elements, then all second-sequence elements, in order. Use `Union` for a mathematical set union (unique values from both); use `Concat` when you want all elements including duplicates.

---

## Q4. What does `Intersect` do?

**Concepts**
- Set intersection
- Elements present in both sequences
- Default equality comparer
- Order from first sequence
- Duplicates within each sequence are handled

**Answer**

`Intersect(second)` returns the set intersection: only elements that appear in both sequences. The result contains each such element once, even if it appears multiple times in either sequence. The relative order of elements in the result matches their order in the first sequence. For example, `new[]{1,2,3,4}.Intersect(new[]{3,4,5,6})` returns `{3,4}`. Equality is determined by the default equality comparer, or a custom `IEqualityComparer<T>` if provided. Internally, `Intersect` builds a `HashSet<T>` from the second sequence and then streams the first sequence, yielding elements that are found in the hash set. The memory cost is O(|second|) — the second sequence is fully buffered. Common use cases include finding common tags, shared category memberships, or shared permissions between two sets.

---

## Q5. What does `Except` do?

**Concepts**
- Set difference
- Elements in first but not second
- Asymmetric operation
- Default equality comparer
- Preserves first sequence order

**Answer**

`Except(second)` returns the set difference: elements that appear in the first sequence but not in the second. The result is asymmetric — `a.Except(b)` and `b.Except(a)` produce different results. For example, `new[]{1,2,3,4}.Except(new[]{3,4,5})` returns `{1,2}`. Each element from the first sequence is included at most once in the result even if it appears multiple times in the first sequence. Internally, `Except` builds a `HashSet<T>` from the second sequence and yields elements from the first that are not found in the hash set. Common use cases include finding items to process that have not been seen before, items in a catalog that are not yet in a shopping cart, or permissions a user has that are not in a revoked list. `ExceptBy(second, keySelector)` (available since .NET 6) compares by a projected key rather than the full element.

---

## Q6. What are `UnionBy`, `IntersectBy`, and `ExceptBy` (.NET 6+)?

**Concepts**
- Key-selector variants of set operators
- Compare by projected key, not full element
- Preserves first full element per key
- Available since .NET 6
- IEqualityComparer<TKey> overload

**Answer**

`UnionBy(second, keySelector)`, `IntersectBy(second, keySelector)`, and `ExceptBy(second, keySelector)` were introduced in .NET 6 alongside `DistinctBy`. They perform set operations based on a projected key rather than the full element, allowing you to deduplicate or compare complex objects by a specific property without requiring custom `IEqualityComparer<T>` implementations. For example, `products1.UnionBy(products2, p => p.ProductCode)` produces all products whose `ProductCode` is unique across both lists, keeping the first instance for each code. `ExceptBy(second, keySelector)` is particularly useful: `allProducts.ExceptBy(discontinuedCodes, p => p.Code)` efficiently returns products whose codes are not in the `discontinuedCodes` collection. Before .NET 6, the equivalent required implementing `IEqualityComparer<Product>` or using `Where` with `Contains`. These operators preserve the full element, not just the key.

---

## Q7. How does `Distinct` behave with custom reference types that don't override `Equals`?

**Concepts**
- Reference equality fallback
- Objects with same data not considered equal
- Requires Equals/GetHashCode override
- IEqualityComparer<T> alternative
- record types provide value equality

**Answer**

For a custom class that does not override `Equals` and `GetHashCode`, `Distinct()` uses reference equality — two separate object instances with identical property values are treated as distinct elements. This is a common gotcha: if you call `persons.Distinct()` on a list of `Person` objects loaded from a database (or constructed by `new Person { Name = "Alice" }` twice), you will get all objects because each is a different reference. The fix is to either: (1) override `Equals` and `GetHashCode` in the class based on value fields; (2) use `record` types (which auto-generate value-based equality); or (3) pass a custom `IEqualityComparer<T>` to `Distinct`. For EF Core entities, overriding `Equals` should be done carefully — EF's identity map already tracks entities by key, so overriding equality can interfere with change tracking. The recommended approach for EF entities is to pass a custom `IEqualityComparer<T>` or project to value types/records before deduplicating.

---

## Q8. What is the memory and time complexity of `Distinct`, `Union`, `Intersect`, and `Except`?

**Concepts**
- HashSet<T> internal buffering
- Time: O(n) per operator
- Memory: O(n) for buffered elements
- Streaming first sequence, hashed second
- Incremental HashSet for Distinct

**Answer**

All four set operators use `HashSet<T>` internally, giving O(1) average-case lookups and therefore O(n) overall time complexity, where n is the total number of elements processed. `Distinct` maintains a `HashSet<T>` as it streams, adding each yielded element — memory usage is O(|unique elements|), growing as unique elements are seen. `Union` similarly streams both sequences, maintaining a set of seen elements — O(|unique elements across both|) memory. `Intersect` and `Except` build a `HashSet<T>` from the second sequence first (O(|second|) memory), then stream the first sequence checking against it. The second sequence is fully loaded into memory before the first element of the first sequence is examined. For very large second sequences, this can be a memory concern — if the second sequence is an `IQueryable<T>` database query, calling `Intersect` or `Except` in LINQ to Objects will load the entire second result set into memory. On `IQueryable<T>` providers, these operators may translate to `INTERSECT`, `EXCEPT`, or `NOT IN` SQL clauses, avoiding in-memory loading.

---

## Q9. Does `Union` guarantee element order?

**Concepts**
- First sequence elements in order
- Second sequence elements not in first, in order
- Not alphabetical or numerical order
- OrderBy required for sorted result
- Stable with respect to input order

**Answer**

`Union` does not guarantee alphabetical or numerical order. It preserves encounter order: elements from the first sequence appear first (in their original sequence order, first occurrence only), followed by elements from the second sequence that were not already in the first (in their original order, first occurrence only). This is consistent but not sorted. For a sorted union, chain `OrderBy`: `seq1.Union(seq2).OrderBy(x => x)`. Similarly, `Distinct` returns elements in the order they are first encountered in the source — the first occurrence of each value is preserved, subsequent duplicates are suppressed. This stable encounter-order behavior makes `Union` and `Distinct` predictable even though they are not sorted. On SQL providers, `UNION` may or may not preserve any order depending on the database engine — always use `ORDER BY` in SQL or `OrderBy` in LINQ when order matters.

---

## Q10. How does `SequenceEqual` differ from `Intersect`?

**Concepts**
- SequenceEqual checks order and exact content
- Intersect finds common elements
- SequenceEqual is a boolean test
- Length mismatch → false
- Use cases: equality test vs. set operation

**Answer**

`SequenceEqual(second)` returns `true` only if both sequences have the same elements in the same order — it is a complete equality check, not a set operation. It returns `false` if the sequences differ in length or if any element at position i is different. `Intersect(second)`, by contrast, returns a new sequence of elements common to both regardless of order or length. `SequenceEqual` is for testing whether two sequences are identical; `Intersect` is for finding shared elements. For example, `{1,2,3}.SequenceEqual({1,2,3})` is `true`, but `{1,2,3}.SequenceEqual({3,2,1})` is `false`. `{1,2,3}.Intersect({3,2,1})` returns `{1,2,3}` (all are common). For set equality (same elements regardless of order), there is no built-in operator — the common pattern is `!a.Except(b).Any() && !b.Except(a).Any()` or comparing sorted sequences.

---

## Q11. Scenario: A sync service compares two lists of IDs to find items added, removed, and unchanged. How do you implement this with set operations? (Scenario)

**Concepts**
- Except for added/removed items
- Intersect for unchanged items
- Remote vs. local set comparison
- HashSet for O(1) membership tests
- Single pass with Aggregate alternative

**Answer**

The canonical pattern uses `Except` and `Intersect` on two collections of IDs:

```csharp
var localIds = localItems.Select(x => x.Id).ToHashSet();
var remoteIds = remoteItems.Select(x => x.Id).ToHashSet();

var addedIds   = remoteIds.Except(localIds).ToList();    // in remote, not local
var removedIds = localIds.Except(remoteIds).ToList();    // in local, not remote
var unchangedIds = localIds.Intersect(remoteIds).ToList(); // in both
```

Converting to `HashSet<T>` before calling `Except` and `Intersect` uses the `HashSet<T>` methods directly, which are O(1) per lookup rather than building a second internal hash set. The full synchronization logic then becomes:

```csharp
await AddItemsAsync(addedIds);
await RemoveItemsAsync(removedIds);
// unchanged items may still need content comparison
var contentChanged = unchangedIds
    .Where(id => localMap[id].Hash != remoteMap[id].Hash)
    .ToList();
await UpdateItemsAsync(contentChanged);
```

This pattern is clean, readable, and efficient. For very large ID sets (millions of items), consider streaming from the database sorted and using a merge-join comparison rather than loading all IDs into memory.

---

## Q12. Scenario: A developer uses `Distinct()` on a list of DTOs and complains it doesn't work. What do you tell them? (Scenario)

**Concepts**
- Reference equality on classes
- Missing Equals/GetHashCode override
- record type solution
- IEqualityComparer<T> solution
- Debugging with breakpoints in GetHashCode

**Answer**

The problem is almost certainly that `Distinct()` is using reference equality because the DTO class does not override `Equals` and `GetHashCode`. Two `new ProductDto { Id = 1, Name = "Widget" }` instances are different objects in memory and are treated as distinct by `Distinct()` even though their data is identical. There are three solutions:

1. **Override `Equals` and `GetHashCode`** in the DTO class — boilerplate-heavy but works everywhere.

2. **Convert to a `record`** — records auto-generate value-based `Equals` and `GetHashCode`:
   ```csharp
   public record ProductDto(int Id, string Name);
   // productDtos.Distinct() now works by value
   ```

3. **Pass a custom `IEqualityComparer<T>`**:
   ```csharp
   public class ProductDtoComparer : IEqualityComparer<ProductDto>
   {
       public bool Equals(ProductDto? x, ProductDto? y) => x?.Id == y?.Id;
       public int GetHashCode(ProductDto obj) => obj.Id.GetHashCode();
   }
   productDtos.Distinct(new ProductDtoComparer())
   ```

For .NET 6+, `DistinctBy(p => p.Id)` is the most concise solution when deduplication is purely by a single key.

---

## Q13. When would you use `Except` instead of `Where` with `Contains`? (Scenario)

**Concepts**
- Except uses hash set internally
- Where+Contains can be O(n*m) if Contains is on IEnumerable
- HashSet.Contains is O(1)
- For IQueryable, Contains translates to NOT IN
- Performance and readability trade-off

**Answer**

`Except` is often more readable than `Where(x => !excluded.Contains(x))` and is equally efficient when the excluded set is already materialized as a collection with fast `Contains` (like `HashSet<T>`). The danger is when `excluded` is a plain `IEnumerable<T>` — `Contains` on `IEnumerable<T>` is O(n), turning the overall operation O(n×m). `Except` builds a `HashSet<T>` from the excluded sequence internally, guaranteeing O(n+m) regardless.

```csharp
// Potentially O(n*m) if excludedList is IEnumerable<T>
var safe = allItems.Where(x => !excludedList.Contains(x));

// Always O(n+m) — Except hashes the second sequence
var safe = allItems.Except(excludedList);
```

On `IQueryable<T>`, `Where(x => !excludedList.Contains(x.Id))` typically translates to `WHERE Id NOT IN (...)` in SQL, which is fine for small excluded sets. For large excluded sets (hundreds of values), the SQL `NOT IN` clause can be slow without an index — consider a LEFT JOIN pattern with a null check instead. `Except` on `IQueryable<T>` translates to `EXCEPT` set operator which may perform differently depending on the database.
