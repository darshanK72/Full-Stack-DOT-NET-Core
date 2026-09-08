# C# `List<T>` — Interview Q&A

---


## Table of Contents

1. [Q1. What is `List<T>` and what problem does it solve over plain arrays?](#q1-what-is-listt-and-what-problem-does-it-solve-over-plain-arrays)
2. [Q2. What is the difference between `Count` and `Capacity`?](#q2-what-is-the-difference-between-count-and-capacity)
3. [Q3. How does the capacity growth sequence work and why does pre-sizing matter?](#q3-how-does-the-capacity-growth-sequence-work-and-why-does-pre-sizing-matter)
4. [Q4. What is the difference between `Add`, `AddRange`, and `Insert`?](#q4-what-is-the-difference-between-add-addrange-and-insert)
5. [Q5. Explain `Remove`, `RemoveAt`, and `RemoveAll` — how does each work and when do you use each?](#q5-explain-remove-removeat-and-removeall-how-does-each-work-and-when-do-you-use-each)
6. [Q6. How does `Find` work, and what is the return value when nothing matches?](#q6-how-does-find-work-and-what-is-the-return-value-when-nothing-matches)
7. [Q7. How does `Sort()` work on `List<int>` and `List<string>`, and what happens when T is your own class?](#q7-how-does-sort-work-on-listint-and-liststring-and-what-happens-when-t-is-your-own-class)
8. [Q8. What is `IComparable<T>` and how do you implement it for a custom sort order?](#q8-what-is-icomparablet-and-how-do-you-implement-it-for-a-custom-sort-order)
9. [Q9. What is `IComparer<T>` and when should you use it instead of `IComparable<T>`?](#q9-what-is-icomparert-and-when-should-you-use-it-instead-of-icomparablet)
10. [Q10. What is `Comparison<T>` and when would you choose it over `IComparer<T>`?](#q10-what-is-comparisont-and-when-would-you-choose-it-over-icomparert)
11. [Q11. How does `BinarySearch` work and what are its prerequisites and return-value semantics?](#q11-how-does-binarysearch-work-and-what-are-its-prerequisites-and-return-value-semantics)
12. [Q12. What does `AsReadOnly()` return, and what are its limits?](#q12-what-does-asreadonly-return-and-what-are-its-limits)
13. [Q13. What does `ConvertAll` do, and how does it compare to LINQ `Select`?](#q13-what-does-convertall-do-and-how-does-it-compare-to-linq-select)
14. [Q14. What is `TrimExcess()` and when should you call it?](#q14-what-is-trimexcess-and-when-should-you-call-it)
15. [Q15. When should you choose `List<T>` over a plain `T[]` array?](#q15-when-should-you-choose-listt-over-a-plain-t-array)
16. [Q16. How does `List<T>.ForEach` differ from a `foreach` loop?](#q16-how-does-listtforeach-differ-from-a-foreach-loop)
17. [Q17. How do `Contains` and `IndexOf` determine equality, and what must you do for custom types?](#q17-how-do-contains-and-indexof-determine-equality-and-what-must-you-do-for-custom-types)
18. [Q18. What does `List<T>` do during `Sort` internally, and is it a stable sort?](#q18-what-does-listt-do-during-sort-internally-and-is-it-a-stable-sort)
19. [Q19. What happens if you call `Remove` inside a `foreach` loop over the same list?](#q19-what-happens-if-you-call-remove-inside-a-foreach-loop-over-the-same-list)
20. [Q20. What is the gotcha with `default(T)` when using `Find` on a `List<int>` or `List<bool>`?](#q20-what-is-the-gotcha-with-defaultt-when-using-find-on-a-listint-or-listbool)
21. [Q21. What happens when you call `Sort()` on a `List<T>` where T does not implement `IComparable<T>`?](#q21-what-happens-when-you-call-sort-on-a-listt-where-t-does-not-implement-icomparablet)
22. [Q22. Why does `BinarySearch` return unexpected results on an unsorted list?](#q22-why-does-binarysearch-return-unexpected-results-on-an-unsorted-list)
23. [Q23. What is the `List<T>` thread-safety guarantee (or lack thereof)?](#q23-what-is-the-listt-thread-safety-guarantee-or-lack-thereof)
24. [Q24. What is the gotcha with `Clear()` and memory, and when do you actually need `TrimExcess()`?](#q24-what-is-the-gotcha-with-clear-and-memory-and-when-do-you-actually-need-trimexcess)
25. [Q25. Code Review — Bulk import with repeated capacity growth](#q25-code-review-bulk-import-with-repeated-capacity-growth)
26. [Q26. Code Review — Modifying a list during `foreach`](#q26-code-review-modifying-a-list-during-foreach)
27. [Q27. Code Review — Leaking a mutable internal list through a public property](#q27-code-review-leaking-a-mutable-internal-list-through-a-public-property)
28. [Q28. Code Review — Thread-unsafe shared static list](#q28-code-review-thread-unsafe-shared-static-list)
29. [Q29. Scenario — Choosing between three sort approaches for a reporting service](#q29-scenario-choosing-between-three-sort-approaches-for-a-reporting-service)
30. [Q30. Scenario — Performance regression from nested `IndexOf` / `Contains` calls at scale](#q30-scenario-performance-regression-from-nested-indexof-contains-calls-at-scale)
31. [Q31. Scenario — Building a pipeline that reads, filters, sorts, and exposes a read-only result](#q31-scenario-building-a-pipeline-that-reads-filters-sorts-and-exposes-a-read-only-result)
32. [Q32. Scenario — Deciding between `List<T>`, array, and `IReadOnlyList<T>` for three API boundaries](#q32-scenario-deciding-between-listt-array-and-ireadonlylistt-for-three-api-boundaries)

---
## Foundation Questions

---

## Q1. What is `List<T>` and what problem does it solve over plain arrays?

**Concepts**
- Resizable generic collection in `System.Collections.Generic`
- Internally wraps a `T[]` backing array
- Dynamic size via `Add`, `Remove`, `Insert`
- Type-safe; no boxing for value types unlike `ArrayList`
- Implements `IList<T>`, `ICollection<T>`, `IEnumerable<T>`

**Answer**

`List<T>` is a generic, dynamically-sized ordered collection. Plain arrays have a fixed `Length` set at creation — adding an element forces you to allocate a larger array and copy every element manually. `List<T>` handles that internally: it maintains a `T[]` backing array and grows it automatically when needed, while the public API exposes methods like `Add`, `Remove`, and `Insert` that keep your code clean. Because it is generic, the compiler enforces the element type at compile time with no boxing overhead for value types and no unsafe casts on reads — both pain points of the legacy `ArrayList`.

---

## Q2. What is the difference between `Count` and `Capacity`?

**Concepts**
- `Count` — logical size (elements actually stored)
- `Capacity` — physical size of backing array
- Capacity grows when Count would exceed it
- Growth typically doubles (0 → 4 → 8 → 16 → 32 …)
- `Clear()` resets Count but leaves Capacity unchanged

**Answer**

`Count` is how many elements the list currently holds. `Capacity` is the length of the internal `T[]` backing array. When `Add` or `AddRange` would push `Count` past `Capacity`, `List<T>` allocates a new backing array (typically double the current `Capacity`), copies all existing elements, and discards the old array — an O(n) operation, though amortized O(1) per `Add` over many calls. After a `Clear()`, `Count` drops to zero but `Capacity` stays at its last high-water mark, meaning the backing array is still allocated. If the list will remain small after a bulk delete, call `TrimExcess()` to release that memory.

```csharp
var growth = new List<int>();
for (int i = 0; i < 20; i++)
    growth.Add(i);

Console.WriteLine($"Count={growth.Count}, Capacity={growth.Capacity}");
// Count=20, Capacity=32

growth.Clear();
Console.WriteLine($"After Clear: Count={growth.Count}, Capacity={growth.Capacity}");
// After Clear: Count=0, Capacity=32 — backing array still allocated

growth.TrimExcess();
Console.WriteLine($"After TrimExcess: Capacity={growth.Capacity}");
// After TrimExcess: Capacity=0
```

---

## Q3. How does the capacity growth sequence work and why does pre-sizing matter?

**Concepts**
- Default capacity starts at 0; first `Add` jumps to 4
- Each resize doubles: 0 → 4 → 8 → 16 → 32 …
- Each resize copies all existing elements
- Pre-sizing with constructor overload eliminates resizes
- `AddRange` does a single capacity check for the whole batch

**Answer**

When you default-construct `new List<T>()`, the backing array length is 0. The first `Add` allocates 4 slots. Each subsequent overflow doubles the array — so after 20 `Add` calls you have seen capacity steps of 4, 8, 16, 32. Every step allocates a new `T[]` and copies all elements, which for a 500 000-item bulk load means many large intermediate allocations and GC pressure. The fix is to hint the expected size upfront: `new List<T>(capacity)` sets the backing array to exactly that size, skipping every resize. When you know the source is a collection, pass it directly to the constructor — `new List<T>(collection)` — so `List<T>` can read `ICollection<T>.Count` and pre-size in one step.

```csharp
// Without hint — multiple resizes
var skus = new List<string>();
foreach (var line in feedLines)
    skus.Add(line.Trim());

// With hint — zero resizes for a known-count source
var skus = new List<string>(feedLines.Count);
foreach (var line in feedLines)
    skus.Add(line.Trim());
```

---

## Q4. What is the difference between `Add`, `AddRange`, and `Insert`?

**Concepts**
- `Add(item)` — appends one element; amortized O(1)
- `AddRange(collection)` — appends batch; one capacity check for the whole range
- `Insert(index, item)` — inserts at position; shifts all elements right O(n)
- `Insert` at `Count` is equivalent to `Add` but slower (still calls resize path)
- Index out of range on `Insert` throws `ArgumentOutOfRangeException`

**Answer**

`Add` appends a single element to the end — the common fast path with amortized O(1) cost. `AddRange` appends every element in a collection in one call; internally it checks whether the total would exceed `Capacity` once and resizes at most once for the whole batch, making it faster than calling `Add` in a loop. `Insert(index, item)` places an element at an arbitrary position, but it must shift all elements from `index` to `Count - 1` one step to the right, giving O(n) worst-case time. Inserting repeatedly in the middle of a large list is an antipattern; if order of insertion matters but you need many middle inserts, prefer `LinkedList<T>` or build the list and sort it once.

```csharp
var dockLabels = new List<string> { "Dock-A", "Dock-B", "Dock-C" };

dockLabels.Add("Dock-D");                             // append one
dockLabels.AddRange(new[] { "Dock-E", "Dock-F" });   // append batch, one resize check
dockLabels.Insert(1, "Dock-Alpha");                   // shifts Dock-B, Dock-C, … right
```

---

## Q5. Explain `Remove`, `RemoveAt`, and `RemoveAll` — how does each work and when do you use each?

**Concepts**
- `Remove(item)` — linear scan, removes first matching element, returns bool
- `RemoveAt(index)` — removes element at position, shifts elements left O(n)
- `RemoveAll(predicate)` — single-pass removal of all matches, returns removed count
- Equality for `Remove` uses `EqualityComparer<T>.Default`
- All three throw or return false if target not present; `RemoveAt` throws `ArgumentOutOfRangeException`

**Answer**

`Remove(item)` scans from index 0 and removes the first element equal to `item`, returning `true` if found or `false` if absent. For reference types it uses `Equals` — if you have not overridden it, only the same object reference will match. `RemoveAt(index)` removes the element at a specific zero-based index and shifts all subsequent elements one position left, at O(n) cost. `RemoveAll(predicate)` does a single forward pass over the entire backing array, keeping elements that fail the predicate and compacting in-place; it returns the count removed. When you need to delete many elements matching a condition, `RemoveAll` is both faster (one pass vs. n calls to `Remove`) and safe to call once — unlike calling `Remove` inside `foreach`, which throws `InvalidOperationException` because it modifies the list while the enumerator is active.

```csharp
var palletCounts = new List<int> { 42, 8, 15, 8, 30, 8 };

palletCounts.Remove(8);                        // removes first 8 only → [42, 15, 8, 30, 8]
palletCounts.RemoveAt(0);                      // removes 42 by index → [15, 8, 30, 8]
int removed = palletCounts.RemoveAll(n => n == 8); // removes all 8s → [15, 30]; removed=2
```

---

## Q6. How does `Find` work, and what is the return value when nothing matches?

**Concepts**
- `Find(Predicate<T>)` — returns first match or `default(T)`
- `FindAll(Predicate<T>)` — returns new `List<T>` of all matches
- `Exists(Predicate<T>)` — returns bool; stops at first match
- `default(T)` is `null` for reference types, `0`/`false`/empty for value types
- Cannot distinguish "found zero" from "not found" on `List<int>` without `Exists`

**Answer**

`Find` scans the list from index 0 and returns the first element for which the predicate returns `true`. If no element matches, it returns `default(T)` — which is `null` for reference types and the zero value for value types such as `0` for `int` or `false` for `bool`. This ambiguity is a common source of bugs: `palletCounts.Find(n => n > 20)` returns `0` whether the search fails or the list happens to contain `0`. The safe pattern is to call `Exists` first to confirm a match, then call `Find`, or to switch to a nullable return: `palletCounts.Cast<int?>().FirstOrDefault(n => n > 20)`. `FindAll` always returns a new `List<T>` — never `null` — which may be empty if nothing matches.

```csharp
var palletCounts = new List<int> { 42, 8, 15, 8, 30 };

int first8 = palletCounts.Find(n => n == 8);           // 8
int notFound = palletCounts.Find(n => n > 100);        // 0 — same as default(int)!
bool anyOver100 = palletCounts.Exists(n => n > 100);   // false — use this to distinguish

List<int> allEights = palletCounts.FindAll(n => n == 8); // [8, 8] — never null
```

---

## Q7. How does `Sort()` work on `List<int>` and `List<string>`, and what happens when T is your own class?

**Concepts**
- Parameterless `Sort()` requires `IComparable<T>` or `IComparable`
- Uses `Array.Sort` internally with `Comparison<T>` adapter
- Mutates the list in place; returns `void`
- Throws `InvalidOperationException` at runtime if T lacks natural order
- `string.Sort` is ordinal (Unicode code-point order) by default

**Answer**

Parameterless `Sort()` delegates to `Array.Sort` on the backing array, using `Comparer<T>.Default` which looks for `IComparable<T>` on the element type. Built-in types like `int`, `double`, `DateTime`, and `string` all implement it, so `Sort()` works immediately. Strings sort by Unicode code-point value, which puts uppercase letters before lowercase — "Dock-B" before "dock-a". For your own class, you must either implement `IComparable<T>` on the class itself or use one of the overloads: `Sort(IComparer<T>)` or `Sort(Comparison<T>)`. If you call parameterless `Sort()` on a `List<ShipmentItem>` that does not implement `IComparable<T>`, you get `InvalidOperationException` at runtime, not a compile error — a trap that passes unit tests if the list is always empty or single-element.

```csharp
var priorities = new List<int> { 3, 1, 2 };
priorities.Sort();                            // [1, 2, 3]

var labels = new List<string> { "dock-Z", "Dock-A", "dock-b" };
labels.Sort();                                // ["Dock-A", "dock-Z", "dock-b"] — uppercase first

// Custom type without IComparable<T> — throws at runtime:
// new List<ShipmentItem> { … }.Sort();       // InvalidOperationException
```

---

## Q8. What is `IComparable<T>` and how do you implement it for a custom sort order?

**Concepts**
- `IComparable<T>` defines natural (default) sort order for a type
- `CompareTo(T other)` contract: negative/zero/positive
- Negative means `this` sorts before `other`
- Called by parameterless `Sort()` via `Comparer<T>.Default`
- Also used by `BinarySearch` and `Array.Sort`

**Answer**

`IComparable<T>` lives in `System` and requires a single method: `int CompareTo(T? other)`. The return value follows the universal comparison contract — return a negative integer when `this` should appear before `other`, zero when they are equivalent for ordering purposes, and a positive integer when `this` should appear after `other`. The simplest implementation delegates to an existing comparable field: `return Priority.CompareTo(other.Priority)`. Once a class implements `IComparable<T>`, parameterless `Sort()`, `BinarySearch`, and framework facilities like `SortedSet<T>` all use it without extra configuration. Reserve `IComparable<T>` for the type's most natural, single canonical order; for secondary or alternative orderings, use `IComparer<T>` or `Comparison<T>` to avoid changing the primary contract.

```csharp
public class ShipmentItem : IComparable<ShipmentItem>
{
    public string Sku { get; }
    public int Priority { get; set; }

    public int CompareTo(ShipmentItem? other)
    {
        if (other is null) return 1;           // non-null sorts after null
        return Priority.CompareTo(other.Priority);
    }
}

var queue = new List<ShipmentItem> { /* … */ };
queue.Sort();    // uses ShipmentItem.CompareTo — lower Priority value ships first
```

---

## Q9. What is `IComparer<T>` and when should you use it instead of `IComparable<T>`?

**Concepts**
- `IComparer<T>` — external comparator object, decoupled from T
- `Compare(T? x, T? y)` — same negative/zero/positive contract
- Enables multiple sort orders without modifying T
- Passed to `Sort(IComparer<T>)`, `SortedSet<T>`, `SortedDictionary<TKey,TValue>`
- `Comparer<T>.Create(Comparison<T>)` wraps a lambda into an `IComparer<T>`

**Answer**

`IComparer<T>` separates the comparison algorithm from the type being compared. You implement it in a standalone class and pass an instance to `Sort(IComparer<T>)`. This is the right choice when the type should not own a natural order (e.g., a DTO shared across systems), when you need multiple simultaneous sort orders (by SKU, by quantity, by priority), or when the class is sealed and you cannot add `IComparable<T>`. Each sort order becomes its own class, making them independently testable and composable. For ad-hoc needs, `Comparer<T>.Create(comparison)` wraps a lambda without writing a full class.

```csharp
public class ShipmentItemSkuComparer : IComparer<ShipmentItem>
{
    public int Compare(ShipmentItem? x, ShipmentItem? y)
    {
        if (x is null) return y is null ? 0 : -1;
        if (y is null) return 1;
        return string.Compare(x.Sku, y.Sku, StringComparison.Ordinal);
    }
}

var queue = new List<ShipmentItem> { /* … */ };
queue.Sort(new ShipmentItemSkuComparer());    // by SKU, natural order unchanged

// Or inline with Comparer<T>.Create:
queue.Sort(Comparer<ShipmentItem>.Create((a, b) => a.Sku.CompareTo(b.Sku)));
```

---

## Q10. What is `Comparison<T>` and when would you choose it over `IComparer<T>`?

**Concepts**
- `Comparison<T>` is a `delegate int Comparison<in T>(T x, T y)`
- Passed directly to `Sort(Comparison<T>)` without a class
- Supports lambdas and method groups inline
- Ephemeral — not reusable across multiple sorts without assignment
- `Comparer<T>.Create(Comparison<T>)` converts it to `IComparer<T>` when needed

**Answer**

`Comparison<T>` is a delegate type that carries the same `int(T, T)` contract as `IComparer<T>.Compare` but without requiring a class. You can pass a lambda directly: `queue.Sort((a, b) => b.Quantity.CompareTo(a.Quantity))` for descending quantity, or pass a static method as a method group. It is the most concise choice for a one-time sort that does not need reuse. When the same comparison needs to be passed to `SortedSet<T>`, `SortedDictionary`, or any API that requires an `IComparer<T>`, convert it with `Comparer<T>.Create(comparison)`. The distinction is pragmatic: use a `Comparison<T>` lambda for quick one-off sorts; promote it to a named `IComparer<T>` class when you need it more than once or in DI containers.

```csharp
var queue = new List<ShipmentItem> { /* … */ };

// Lambda — descending quantity
queue.Sort((a, b) => b.Quantity.CompareTo(a.Quantity));

// Method group — same result as IComparer-based sort in Section 6
queue.Sort(CompareShipmentBySku);

static int CompareShipmentBySku(ShipmentItem a, ShipmentItem b) =>
    string.Compare(a.Sku, b.Sku, StringComparison.Ordinal);
```

---

## Q11. How does `BinarySearch` work and what are its prerequisites and return-value semantics?

**Concepts**
- Requires list to be sorted in ascending order first
- Returns index of found element (zero-based), or bitwise complement of insertion point
- Bitwise complement `~result` gives insertion index when not found
- Overloads accept `IComparer<T>` to match the sort order used
- Undefined behavior if list is unsorted at time of call

**Answer**

`BinarySearch(item)` performs an O(log n) search by repeatedly halving the search range. It returns the zero-based index of the matched element when found. When the element is absent it returns a negative number — specifically the bitwise complement (`~`) of the index where the item would be inserted to keep the list sorted; applying `~` again recovers that insertion index. A critical prerequisite is that the list must already be sorted in the same order used for the search — calling `BinarySearch` on an unsorted list returns an undefined result without throwing. If you sort with a custom `IComparer<T>`, pass the same comparer to `BinarySearch(item, comparer)` to ensure consistent results.

```csharp
var priorities = new List<int> { 1, 3, 5, 7, 9 };
// List must be sorted first

int idx = priorities.BinarySearch(5);          // returns 2 (found at index 2)
int missing = priorities.BinarySearch(4);      // returns negative: ~missing = 2 (insert before 5)
int insertAt = ~missing;                       // insertAt = 2

Console.WriteLine($"Found 5 at index {idx}");
Console.WriteLine($"4 not found; insert at index {insertAt}");
```

---

## Q12. What does `AsReadOnly()` return, and what are its limits?

**Concepts**
- Returns `ReadOnlyCollection<T>` wrapping the original list
- Mutations through the wrapper throw `NotSupportedException`
- Wrapper is a live view — changes to the source list are visible
- Does not copy elements; O(1) cost
- Exposes `IReadOnlyList<T>` for public API surface

**Answer**

`AsReadOnly()` wraps the `List<T>` in a `ReadOnlyCollection<T>` that forwards reads but blocks writes — calling `Add`, `Remove`, `Clear`, or the indexer setter through the wrapper throws `NotSupportedException`. The wrapper is a live view: it holds a reference to the original list, so if the internal list grows or changes, the `ReadOnlyCollection<T>` reflects those changes immediately. This is useful when a service owns a list internally but wants to expose it to consumers who should not modify it. Two limits to know: the caller could still cast the wrapper back to `List<T>` via reflection (use `IReadOnlyList<T>` in the API signature to reduce that surface), and mutations to the source still leak through — if true isolation is needed, return a copy with `_list.ToList()` or `[.. _list]`.

```csharp
var internalLanes = new List<string> { "Lane-1", "Lane-2" };
ReadOnlyCollection<string> published = internalLanes.AsReadOnly();

internalLanes.Add("Lane-3");                 // mutation on source
Console.WriteLine(published.Count);          // 3 — live view reflects the change

// published.Add("Lane-4");                  // NotSupportedException
```

---

## Q13. What does `ConvertAll` do, and how does it compare to LINQ `Select`?

**Concepts**
- `ConvertAll<TOutput>(Converter<T, TOutput>)` — maps each element to a new type
- Returns a new `List<TOutput>` with the same `Count`
- Eager evaluation; fully materialized immediately
- No LINQ dependency; available on `List<T>` directly
- LINQ `Select` is lazy (deferred); requires `ToList()` to materialize

**Answer**

`ConvertAll<TOutput>` applies a `Converter<T, TOutput>` delegate to each element and returns a new `List<TOutput>` containing the transformed results. It is fully eager — the entire output list is built before the method returns — and it requires no LINQ import. LINQ `Select` is deferred: it returns an `IEnumerable<TOutput>` that projects elements only as you iterate; you call `.ToList()` to force materialization into a new list. For a straightforward type-to-type projection on a `List<T>` already in memory, `ConvertAll` is concise and self-documenting. Prefer LINQ `Select` when composing a pipeline with filtering, grouping, or other operators, since `ConvertAll` cannot chain.

```csharp
var queue = new List<ShipmentItem> { /* … */ };

// ConvertAll — direct, no LINQ, returns List<string>
List<string> skuLabels = queue.ConvertAll(item => item.Sku);

// LINQ equivalent — lazy until ToList()
List<string> skuLabelsLinq = queue.Select(item => item.Sku).ToList();
```

---

## Q14. What is `TrimExcess()` and when should you call it?

**Concepts**
- Shrinks backing array to exactly `Count`
- No-op if `Count / Capacity >= 0.9` (within 10% of full)
- Useful after bulk deletes to release LOH memory
- `Clear()` does not shrink; `TrimExcess()` after `Clear()` resets to zero
- Does not affect `Count`; only `Capacity` changes

**Answer**

After `Clear()` or many `RemoveAll` calls, the list's `Capacity` stays at its peak allocation — the backing array is still in memory, preventing GC from reclaiming it. `TrimExcess()` reallocates the backing array to match the current `Count`, freeing the excess. The runtime skips the reallocation if `Count` is already within 90% of `Capacity` (the threshold avoids a resize that saves only a few slots). The primary use case is long-lived lists that spike large during batch processing then drop back to a small steady-state size — without `TrimExcess()` the large backing array stays allocated for the lifetime of the object. In short-lived or small lists there is no point calling it; the GC handles the object as a whole.

```csharp
var skus = new List<string>(500_000);
foreach (var s in feedLines) skus.Add(s);   // Capacity ≈ 500 000

skus.RemoveAll(s => !s.StartsWith("WH-"));  // 80% removed
Console.WriteLine($"Count={skus.Count}, Capacity={skus.Capacity}");
// Count=100 000, Capacity=500 000 — backing array still large

skus.TrimExcess();
Console.WriteLine($"After TrimExcess: Capacity={skus.Capacity}");
// Capacity≈100 000 — LOH pressure reduced
```

---

## Q15. When should you choose `List<T>` over a plain `T[]` array?

**Concepts**
- Array: fixed `Length`, minimal overhead, best for known-size buffers
- `List<T>`: dynamic `Count`, built-in `Add`/`Remove`/`Insert`
- Both implement `IList<T>` and `IEnumerable<T>`
- Array spans (`Span<T>`, `ReadOnlySpan<T>`) offer zero-copy slicing
- API return types: prefer `IReadOnlyList<T>` to decouple callers

**Answer**

Use an array when the size is fixed and known at creation — days of the week, RGB triplets, lookup tables, buffer pools. Arrays have lower memory overhead (no `List<T>` object wrapping the array), support `Span<T>` slicing without copying, and signal to readers that the count never changes. Use `List<T>` when the count is unknown at creation, when items are added or removed during the lifetime of the collection, or when you need `Add`, `Remove`, `Insert`, or predicate helpers. In method signatures, returning `IReadOnlyList<T>` decouples callers from the concrete type and prevents them from casting to `List<T>` to mutate it. A common .NET 10 pattern is `List<T>` internally, `IReadOnlyList<T>` on the public API surface.

```csharp
// Array — fixed days, never grows
string[] daysOfWeek = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

// List<T> — grows as user adds lanes
var activeLanes = new List<string> { "Lane-1", "Lane-2" };
activeLanes.Add("Lane-3");   // grows cleanly; no manual copy

// Public API: decouple via interface
public IReadOnlyList<string> GetActiveLanes() => activeLanes.AsReadOnly();
```

---

## Q16. How does `List<T>.ForEach` differ from a `foreach` loop?

**Concepts**
- `ForEach(Action<T>)` — built-in method, void return, no break/continue/return
- `foreach` — language keyword, supports `break`, `continue`, early `return`
- Both iterate the full list unless `ForEach` body throws
- `ForEach` throws `InvalidOperationException` if list is modified during iteration
- No performance advantage; `foreach` is more flexible

**Answer**

`List<T>.ForEach(Action<T>)` is a convenience method that calls an `Action<T>` for each element in order. It cannot be short-circuited — there is no way to break out early, skip an element, or return a value. A `foreach` loop supports `break`, `continue`, and `return`, making it strictly more powerful. Both throw `InvalidOperationException` if the list is modified during iteration. `ForEach` is occasionally useful for compact side-effect expressions — building a log, printing items — but the LINQ community largely prefers `foreach` loops for clarity and debuggability. Avoid `ForEach` when you need conditional exit logic.

```csharp
var palletCounts = new List<int> { 42, 8, 15, 30 };
var scanLog = new List<string>();

// ForEach — compact but no break/continue
palletCounts.ForEach(n => scanLog.Add($"scanned:{n}"));

// foreach — supports early exit
foreach (int n in palletCounts)
{
    if (n > 30) break;       // not possible with ForEach
    scanLog.Add($"processed:{n}");
}
```

---

## Q17. How do `Contains` and `IndexOf` determine equality, and what must you do for custom types?

**Concepts**
- Both use `EqualityComparer<T>.Default`
- For value types: structural equality by default
- For reference types: reference equality unless `Equals` is overridden
- `IEquatable<T>` provides typed equality without boxing
- Overriding `Equals` must also override `GetHashCode` (consistency rule)

**Answer**

`Contains(item)` and `IndexOf(item)` both delegate to `EqualityComparer<T>.Default`, which for value types performs structural comparison (two `int` 5s are equal) and for reference types calls `Equals`, which defaults to reference equality if not overridden. If you have two separate `ShipmentItem` instances with the same `Sku` but have not overridden `Equals`, `Contains` returns `false` because the instances are different objects. To make `Contains` work by business identity, override `Equals(object?)` and `GetHashCode()`, or implement `IEquatable<T>`. The overriding rule: if `a.Equals(b)` is true, `a.GetHashCode()` must equal `b.GetHashCode()` — violating this breaks `Dictionary<T,V>` and `HashSet<T>` lookups.

```csharp
public class ShipmentItem : IEquatable<ShipmentItem>
{
    public string Sku { get; init; }
    public bool Equals(ShipmentItem? other) => other is not null && Sku == other.Sku;
    public override bool Equals(object? obj) => Equals(obj as ShipmentItem);
    public override int GetHashCode() => HashCode.Combine(Sku);
}

var queue = new List<ShipmentItem> { new() { Sku = "WH-4412" } };
bool found = queue.Contains(new ShipmentItem { Sku = "WH-4412" }); // true — IEquatable
```

---

## Q18. What does `List<T>` do during `Sort` internally, and is it a stable sort?

**Concepts**
- Delegates to `Array.Sort` with an `ArraySortHelper<T>`
- Uses introsort (quicksort + heapsort + insertion sort hybrid)
- Not a stable sort — equal elements may change relative order
- `OrderBy` (LINQ) uses a stable merge sort
- Stability matters when sorting by a secondary key after a primary key

**Answer**

`List<T>.Sort` delegates to `Array.Sort`, which uses introsort — a hybrid of quicksort, heapsort, and insertion sort. Quicksort handles the general case, heapsort kicks in if recursion depth exceeds a threshold (preventing worst-case O(n²)), and insertion sort handles small sub-arrays efficiently. The result is O(n log n) average and worst case. Importantly, the sort is not stable: elements that compare as equal may appear in a different relative order after sorting. If you need a stable sort — for example, sorting by priority after having already sorted by SKU — use LINQ's `OrderBy` / `ThenBy`, which implements a stable merge sort. In .NET 10, `List<T>` still does not expose a built-in stable sort method, so LINQ is the idiomatic choice there.

```csharp
var items = new List<ShipmentItem> { /* … already sorted by SKU */ };

// Unstable — relative SKU order within same priority may change
items.Sort((a, b) => a.Priority.CompareTo(b.Priority));

// Stable — preserves existing SKU order among equal-priority items
var stable = items.OrderBy(i => i.Priority).ToList();
```

---

## Gotchas — List`<T>` (Interview Traps)

---

#### Gotcha 1. Sort stability is not guaranteed

**Concepts**
- `List<T>.Sort()` uses an unstable sort (introsort)
- Equal elements may change relative order after Sort
- `OrderBy()` in LINQ is a stable sort
- Stable sort preserves original order for equal elements

**Answer**

`List<T>.Sort()` is an unstable sort (introsort — combination of quicksort, heapsort, and insertion sort) — when two elements compare as equal, their relative order in the sorted result is undefined. LINQ's `OrderBy()` is a stable sort, preserving the original relative order of equal elements. This matters for multi-key sorts and for sorting already-partially-ordered data where order among ties is meaningful — always use LINQ `OrderBy().ThenBy()` chains when stability is required.

---

#### Gotcha 2. `Remove(value)` and `RemoveAll(predicate)` behavioral difference

**Concepts**
- `Remove(T item)` removes only the first matching element
- `RemoveAll(Predicate<T>)` removes ALL matching elements in one pass
- `Remove` returns bool (found/not found); `RemoveAll` returns count removed
- Calling `Remove` in a loop is O(n²); `RemoveAll` is O(n)

**Answer**

`List<T>.Remove(item)` removes only the first occurrence of the item and returns `true` if found. If you want to remove all occurrences, calling `Remove` in a loop until it returns `false` is O(n²) because each call scans from the beginning. `RemoveAll(x => x == value)` performs a single O(n) pass, shifting only once — use it whenever removing multiple elements by predicate.

---

#### Gotcha 3. `ForEach(action)` cannot modify the list during execution

**Concepts**
- `List<T>.ForEach()` throws `InvalidOperationException` on modification
- Same version-check mechanism as `foreach` with enumerator
- Cannot `Remove()` or `Add()` inside the action delegate
- Use `RemoveAll()` or a separate result list for filter-and-transform

**Answer**

`List<T>.ForEach(action)` internally uses the list's enumerator — if the action modifies the list (calls `Add`, `Remove`, `Clear`, etc.), it throws `InvalidOperationException: Collection was modified`. This surprises developers who expect `ForEach` to be lower-level than `foreach`. The solution is to use `RemoveAll` for removal, collect results in a separate `List<T>`, or iterate over a copy (`list.ToList().ForEach(...)`).

---

#### Gotcha 4. `AsReadOnly()` does not copy — mutations to the original are visible

**Concepts**
- `AsReadOnly()` returns a `ReadOnlyCollection<T>` wrapping the same backing list
- Mutations to the original list are visible through the read-only view
- Does not protect against concurrent modification by the owner
- True immutability requires `ImmutableList<T>` or a copy

**Answer**

`list.AsReadOnly()` returns a `ReadOnlyCollection<T>` that wraps the original `List<T>` without copying. If the original list is later modified (items added or removed), the read-only view reflects those changes immediately — it is a live window, not a snapshot. This is often useful (shared read access to a live dataset) but dangerous when you intend to hand out an immutable snapshot. For a true immutable snapshot, call `ImmutableList.CreateRange(list)` or simply copy into a new `List<T>` and return that (without exposing the reference).

---

#### Gotcha 5. `List<T>` is not thread-safe for concurrent writes

**Concepts**
- Concurrent `Add` calls can corrupt internal array state
- No lock is provided — responsibility is on the caller
- `ConcurrentBag<T>` or `ConcurrentQueue<T>` for producer-consumer
- Even concurrent reads are not safe if a write is happening

**Answer**

`List<T>` provides no synchronization — concurrent writes from multiple threads can corrupt the internal array (race on `_size` field and array element writes), causing data loss or `IndexOutOfRangeException`. Even concurrent reads interleaved with a write are undefined behavior in .NET. `System.Collections.Concurrent.ConcurrentBag<T>` or `ConcurrentQueue<T>` are the thread-safe alternatives; for a list with indexed access under concurrency, wrap `List<T>` with a `ReaderWriterLockSlim`.

---

#### Gotcha 6. Capacity doubles on overflow — TrimExcess to reclaim memory

**Concepts**
- Backing array doubles in size when Count reaches Capacity
- Capacity can be 2× the actual Count after many removes
- `TrimExcess()` reallocates to match Count exactly
- High-water-mark Capacity stays even after Clear()

**Answer**

When `List<T>` grows beyond its `Capacity`, it allocates a new array of twice the size and copies all elements — this amortizes to O(1) per append but leaves Capacity potentially double the actual Count. After calling `Clear()`, `Count` becomes zero but `Capacity` stays at its high-water mark, wasting memory. Call `TrimExcess()` after building a list that will be read-only from that point on, or set `Capacity` explicitly after `Clear()` if you know the new expected size.

---

#### Gotcha 7. `CopyTo` copies by value for value types, by reference for objects

**Concepts**
- `CopyTo(T[] array, int index)` copies element references/values into target
- Modifying objects in the copy modifies the originals (shallow copy)
- Use `.Select(x => x.Clone()).ToList()` for deep copy of reference types
- Value types are copied by value — independent copies

**Answer**

`List<T>.CopyTo(array, 0)` copies each element into the destination array. For value types, this produces independent copies. For reference types, the same object references are copied — mutating an object through the new array mutates it in the original list too. This shallow-copy behavior is consistent across all .NET collections and needs to be explicitly handled (via `ICloneable`, copy constructors, or record `with` expressions) whenever deep copy semantics are needed.

---

#### Gotcha 8. `BinarySearch` returns a negative number (bitwise complement) on miss

**Concepts**
- Returns index of the found item (≥ 0) on success
- Returns `~insertionPoint` (negative) when not found
- `~result` gives the insertion point for maintaining sort order
- List must be sorted before calling BinarySearch

**Answer**

`List<T>.BinarySearch(item)` returns the index of the matching element when found, or a negative number equal to the bitwise complement (`~`) of the insertion point when not found. Checking `result < 0` detects a miss; applying `~result` to the negative return value gives the index at which the item would be inserted to keep the list sorted. Forgetting this convention and comparing `result == -1` is wrong — the return can be any negative number, not just -1.

---

#### Gotcha 9. `Insert` and `RemoveAt` are O(n) — not O(1)

**Concepts**
- Insert/RemoveAt shift all subsequent elements
- O(n) for insertion in the middle, not O(1) like linked list head insert
- Use `LinkedList<T>` when frequent interior insertions are needed
- Appending at the end with `Add` is amortized O(1)

**Answer**

`List<T>.Insert(i, item)` and `RemoveAt(i)` must shift all elements from index `i` onward by one position — this is O(n - i), which is O(n) in the worst case (inserting at index 0). Developers coming from languages with dynamic arrays that also support constant-time splices are often surprised. `Add()` at the end and `RemoveAt(Count - 1)` at the end are amortized O(1). For frequent interior insertions by design, `LinkedList<T>` offers O(1) insertions at a known node but sacrifices random access.

---

#### Gotcha 10. `List<T>` implements `IList<T>`, not `IReadOnlyList<out T>` — covariance lost

**Concepts**
- `IList<T>` is invariant; `IReadOnlyList<out T>` is covariant
- `List<string>` is NOT assignable to `IList<object>`
- `List<string>` IS assignable to `IReadOnlyList<object>` (via covariance)
- Return `IReadOnlyList<T>` for better API design and covariance support

**Answer**

`List<T>` implements both `IList<T>` (invariant) and `IReadOnlyList<T>` (covariant). Because `IList<T>` is invariant, `List<string>` cannot be assigned to `IList<object>`. But because `IReadOnlyList<out T>` is covariant, `List<string>` (via its `IReadOnlyList<string>` implementation) is assignable to `IReadOnlyList<object>`. This means method signatures returning `IReadOnlyList<T>` give callers both indexed access and covariant assignment — always prefer `IReadOnlyList<T>` over `List<T>` or `IList<T>` in public API return types.

---

## Real-World Scenarios

---

## Q25. Code Review — Bulk import with repeated capacity growth

A nightly import job loads 500 000 shipment SKUs into a `List<string>`. Memory profiling shows repeated large allocations and GC pressure. Review this code.

```csharp
public static List<string> LoadSkusFromFeed(IEnumerable<string> feedLines)
{
    var skus = new List<string>();
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Performance | No initial capacity hint | Backing array doubles ~18 times for 500 000 items; each resize copies all elements |
| Memory | Discarded intermediate arrays | Multiple large arrays in LOH simultaneously; GC pause spikes |
| Operability | `Clear()` later keeps 500 000-slot array | Memory not released if list is reused after a small subset survives |

**Fix priority**

1. Pre-size when count is known: `new List<string>(feedLines.Count)` when `feedLines` is an `ICollection<string>`, or pattern-match: `new List<string>(feedLines is ICollection<string> c ? c.Count : 0)`.
2. If `feedLines` is truly an unbounded stream, accept the resizes but call `TrimExcess()` after any `RemoveAll` step that discards most entries.
3. For truly massive feeds (millions), consider streaming processing rather than materializing everything into one list.

```csharp
public static List<string> LoadSkusFromFeed(IReadOnlyCollection<string> feedLines)
{
    var skus = new List<string>(feedLines.Count);   // single allocation
    foreach (var line in feedLines)
        skus.Add(line.Trim());
    return skus;
}
```

---

## Q26. Code Review — Modifying a list during `foreach`

A warehouse service removes cancelled dock labels at runtime. In staging it throws intermittently on large batches.

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    foreach (string label in dockLabels)
    {
        if (cancelled.Contains(label))
        {
            dockLabels.Remove(label);
        }
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` invalidates enumerator | `InvalidOperationException` mid-purge; job fails |
| Correctness | `Remove` only removes first match per value | Duplicate labels survive even without the exception |
| Performance | `Remove(object)` is O(n) per call inside an O(n) loop | O(n²) cost for large cancel sets |

**Fix priority**

1. Replace the entire method body with `dockLabels.RemoveAll(label => cancelled.Contains(label))` — single forward pass, O(n × average HashSet lookup), no enumerator invalidation.
2. If in-place mutation is not possible (e.g., caller keeps a reference), iterate backwards: `for (int i = dockLabels.Count - 1; i >= 0; i--)` then `RemoveAt(i)`.
3. Document the contract: never pass the same collection as both the iterable and the mutation target.

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    dockLabels.RemoveAll(label => cancelled.Contains(label));
}
```

---

## Q27. Code Review — Leaking a mutable internal list through a public property

A `ShipmentQueueService` exposes its internal lane list directly. A controller clears it accidentally.

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void AddLane(string lane) => _lanes.Add(lane);
}

// Caller (controller)
var lanes = _queueService.Lanes;
lanes.Clear();           // wipes the service's internal state!
lanes.Add("Hijacked");
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Encapsulation | Public `List<T>` return bypasses service API | Callers mutate internal state without going through validation |
| Concurrency | Multiple requests share one mutable reference | One request clears while another reads — corrupt state under load |
| API contract | Return type promises mutability | Consumers couple to `List<T>` methods; hard to swap to another collection |
| Correctness | Service invariants (non-empty, validated names) silently broken | Production data corruption; hard to trace |

**Fix priority**

1. Change the return type to `IReadOnlyList<string>` and return `_lanes.AsReadOnly()` — callers get a live read-only view; no copy cost.
2. For strict immutability (callers should not see live mutations), return a snapshot: `return [.. _lanes]` (collection expression, .NET 10).
3. Route all mutations through named service methods (`AddLane`, `RemoveLane`) that enforce invariants and emit audit events.
4. Mark the property return type `IReadOnlyList<string>` in the interface definition so DI consumers cannot cast to `List<T>`.

```csharp
public IReadOnlyList<string> Lanes => _lanes.AsReadOnly();

public void AddLane(string lane)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(lane);
    _lanes.Add(lane);
}
```

---

## Q28. Code Review — Thread-unsafe shared static list

A singleton background worker and API threads share one static `List<ShipmentItem>`. Under load, counts become wrong and the process occasionally throws.

```csharp
public static class ShipmentHub
{
    public static readonly List<ShipmentItem> LiveQueue = new();

    public static void Enqueue(ShipmentItem item) => LiveQueue.Add(item);

    public static void ProcessNext()
    {
        if (LiveQueue.Count > 0)
        {
            var next = LiveQueue[0];
            LiveQueue.RemoveAt(0);
            Ship(next);
        }
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `Add` and `RemoveAt` on shared list | Lost updates, torn reads, `ArgumentOutOfRangeException` under concurrent load |
| Race condition | `Count > 0` check and `[0]` access are not atomic | Another thread dequeues between the check and the index — index out of range |
| Architecture | Static public mutable field | Hidden global coupling; impossible to scale across processes |
| Observability | Fails intermittently under load | Passes single-threaded tests; fails only in production peak traffic |

**Fix priority**

1. Replace `List<T>` with `ConcurrentQueue<ShipmentItem>` and call `TryDequeue` — designed for concurrent producer/consumer; no external lock needed.
2. If `List<T>` must be kept, guard every access with a private `lock` object — ensures atomicity of check + dequeue.
3. For distributed scenarios, replace in-process queue with a message broker (Azure Service Bus, RabbitMQ) — removes the static global entirely.
4. Never expose the raw queue publicly (`public static readonly`) — wrap in a service class with a typed API.

```csharp
private static readonly ConcurrentQueue<ShipmentItem> LiveQueue = new();

public static void Enqueue(ShipmentItem item) => LiveQueue.Enqueue(item);

public static void ProcessNext()
{
    if (LiveQueue.TryDequeue(out var next))
        Ship(next);
}
```

---

## Q29. Scenario — Choosing between three sort approaches for a reporting service

A reporting service sorts a `List<ShipmentItem>` in three different ways depending on the caller: by SKU for the warehouse manifest, by quantity descending for the pick list, and by priority then SKU for the dispatch board. How do you implement this without repeating logic or touching the domain class?

**Concepts**
- Multiple sort orders without modifying `ShipmentItem`
- `IComparer<T>` for reusable named comparers
- `Comparison<T>` for lightweight inline sorts
- `Comparer<T>.Create` to build `IComparer<T>` from a lambda
- Stable LINQ `OrderBy` / `ThenBy` for multi-key sort

**Answer**

When a class should not own a natural order, or when multiple orders are needed, implement each as a separate named `IComparer<T>` class and register them in a dictionary or factory. This keeps `ShipmentItem` free of comparison concerns, makes each comparer independently testable, and signals intent through the name. For the multi-key dispatch-board sort, LINQ `OrderBy(...).ThenBy(...)` is the cleanest approach because it produces a stable sort — `Sort` with a composite comparison would work but is a single lambda with harder-to-read chained `CompareTo` calls.

```csharp
// Named comparers — reusable across the codebase
public sealed class SkuComparer : IComparer<ShipmentItem>
{
    public static readonly SkuComparer Instance = new();
    public int Compare(ShipmentItem? x, ShipmentItem? y) =>
        string.Compare(x?.Sku, y?.Sku, StringComparison.Ordinal);
}

public sealed class QuantityDescendingComparer : IComparer<ShipmentItem>
{
    public static readonly QuantityDescendingComparer Instance = new();
    public int Compare(ShipmentItem? x, ShipmentItem? y) =>
        (y?.Quantity ?? 0).CompareTo(x?.Quantity ?? 0);
}

// Usage
manifest.Sort(SkuComparer.Instance);
pickList.Sort(QuantityDescendingComparer.Instance);

// Multi-key: stable LINQ sort for dispatch board
var dispatchBoard = queue
    .OrderBy(i => i.Priority)
    .ThenBy(i => i.Sku, StringComparer.Ordinal)
    .ToList();
```

---

## Q30. Scenario — Performance regression from nested `IndexOf` / `Contains` calls at scale

A validator checks whether each of 5 000 incoming pallet SKUs is already present in a queue of 50 000 `ShipmentItem` objects by calling `IndexOf` in a loop. Load tests time out. Diagnose and fix.

**Concepts**
- `IndexOf` and `Contains` are O(n) linear scans
- Nested O(n) loop inside O(n) outer loop = O(n²)
- `HashSet<T>` lookup is O(1) average
- Pre-building a lookup set amortizes the cost to O(n + m)
- Reference equality gotcha: must project to key (string), not object

**Answer**

`IndexOf` on a `List<T>` does a linear scan from the beginning. Calling it 5 000 times over a list of 50 000 items produces 250 000 000 comparisons in the worst case — O(n × m) behavior that scales badly. The fix is to project the queue to a `HashSet<string>` of SKUs once (O(n)), then do O(1) average lookups for each incoming item (O(m) total), giving O(n + m) overall. An additional gotcha: `IndexOf` on `List<ShipmentItem>` uses reference equality unless `Equals` is overridden on the class, so even the slow O(n²) version may return false negatives for structurally equal objects — projecting to a `HashSet<string>` on the key field avoids that entirely.

```csharp
// BEFORE — O(n²), potentially false negatives
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0) return false;
    }
    return true;
}

// AFTER — O(n + m), correct by-SKU semantics
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    var queuedSkus = new HashSet<string>(
        queue.Select(q => q.Sku), StringComparer.OrdinalIgnoreCase);
    return incoming.All(item => queuedSkus.Contains(item.Sku));
}
```

---

## Q31. Scenario — Building a pipeline that reads, filters, sorts, and exposes a read-only result

A `WarehouseReportService` must: load shipment items from a repository (returns `IEnumerable<ShipmentItem>`), filter to priority 1 and 2, sort by quantity descending, and expose the result as a read-only list to the report controller. The list must not be re-fetchable on every property access.

**Concepts**
- Eager materialization of `IEnumerable<T>` pipeline with `ToList()`
- `Where` + `OrderByDescending` for filter + stable sort
- Caching result in a field to avoid repeated evaluation
- Exposing as `IReadOnlyList<T>` via `AsReadOnly()` or collection expression snapshot
- `[.. collection]` spread syntax for snapshot in .NET 10

**Answer**

The pipeline should be materialized once: call `.Where(...).OrderByDescending(...).ToList()` to execute the query and store the result in a private field. Returning `IReadOnlyList<ShipmentItem>` from the public property prevents callers from mutating the cached result. Use `AsReadOnly()` for a live wrapper (no copy) or `[.. _cached]` (collection expression) for a defensive copy if you want the controller's reference to be fully isolated from future internal updates. The LINQ `OrderByDescending` produces a stable sort, so items with equal quantities preserve their original relative order — relevant if the repository returns them in creation order.

```csharp
public sealed class WarehouseReportService
{
    private readonly IShipmentRepository _repo;
    private IReadOnlyList<ShipmentItem>? _urgentItems;

    public WarehouseReportService(IShipmentRepository repo) => _repo = repo;

    public IReadOnlyList<ShipmentItem> GetUrgentItems()
    {
        if (_urgentItems is not null) return _urgentItems;

        List<ShipmentItem> result = _repo.GetAll()
            .Where(i => i.Priority is 1 or 2)
            .OrderByDescending(i => i.Quantity)
            .ToList();

        _urgentItems = result.AsReadOnly();   // live read-only wrapper; no copy
        return _urgentItems;
    }
}
```

---

## Q32. Scenario — Deciding between `List<T>`, array, and `IReadOnlyList<T>` for three API boundaries

You are designing a .NET 10 service with three methods: (a) `GetDaysOfWeek()` returns a fixed sequence of labels used for a schedule grid; (b) `GetActiveLanes()` returns the current lane list that other services may query but not modify; (c) `BuildShipmentQueue(IEnumerable<string> skus)` accumulates parsed items and returns them to the caller for further processing. What concrete type do you use for each return and why?

**Concepts**
- Array for fixed-size, known-at-compile-time data
- `IReadOnlyList<T>` return type for encapsulated mutable internals
- `List<T>` return when caller needs to mutate the result
- `AsReadOnly()` vs snapshot copy tradeoffs
- API surface design: concrete vs interface return types

**Answer**

For `GetDaysOfWeek()`, return a `string[]` (or better, a `static readonly string[]` or `IReadOnlyList<string>` backed by a frozen array): the set of days is known at compile time and never grows. For `GetActiveLanes()`, return `IReadOnlyList<string>` backed by `_lanes.AsReadOnly()` — callers can iterate and index but cannot add or remove; the internal `List<string>` remains mutable for the service itself. For `BuildShipmentQueue()`, return `List<ShipmentItem>` — the method is building a result list for the caller to process further, and handing back a `List<T>` signals "this is yours to use as you see fit." Returning `IReadOnlyList<T>` from a factory-style builder is premature restriction; returning `List<T>` from an accessor that owns the data is an encapsulation leak. The distinction is ownership: service owns → expose read-only; caller owns → return the concrete mutable type.

```csharp
// (a) Fixed data — array constant, never grows
private static readonly string[] DaysOfWeek = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"];
public IReadOnlyList<string> GetDaysOfWeek() => DaysOfWeek;

// (b) Owned internal data — live read-only view
private readonly List<string> _lanes = new();
public IReadOnlyList<string> GetActiveLanes() => _lanes.AsReadOnly();

// (c) Caller owns the result — return List<T>
public List<ShipmentItem> BuildShipmentQueue(IEnumerable<string> skus)
{
    var result = new List<ShipmentItem>();
    foreach (var sku in skus)
        result.Add(new ShipmentItem { Sku = sku });
    return result;
}
```
