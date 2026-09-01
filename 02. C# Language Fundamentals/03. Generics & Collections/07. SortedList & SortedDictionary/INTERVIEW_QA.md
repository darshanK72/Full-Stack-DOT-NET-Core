# SortedList&lt;TKey,TValue&gt; & SortedDictionary&lt;TKey,TValue&gt; — Interview Q&A

---

## Foundation Questions

---

## Q1. What is `SortedList<TKey,TValue>` and how does it maintain sorted order internally?

**Concepts**
- Array-backed sorted map (two parallel arrays: keys, values)
- Binary search locates the insertion position on every `Add`
- Array shift moves trailing elements to keep sorted order
- Capacity doubles when the arrays are full, like `List<T>` resizing
- O(log n) lookup, O(n) insert and remove due to the shift cost

**Answer**

`SortedList<TKey,TValue>` stores keys and their associated values in two side-by-side arrays. The key array is kept in ascending sorted order at all times. When you call `Add` or assign through the indexer for a new key, the runtime performs a binary search to find where the new key belongs, then shifts every element at or above that position one slot to the right before writing the new key and value. This shift is what makes insertion and removal O(n) in the worst case. Lookups — `TryGetValue`, `ContainsKey`, `IndexOfKey` — use binary search alone and are O(log n). Because the backing store is a contiguous array, sequential reads of `Keys` or `Values` benefit from CPU cache lines in a way that a tree-backed collection cannot, which makes small `SortedList` maps surprisingly fast in read-heavy workloads.

```csharp
// .NET 10
var prices = new SortedList<string, decimal>
{
    ["WH-9920"] = 14.50m,
    ["WH-1100"] = 8.25m,   // inserted at index 0; "WH-9920" shifts right
    ["WH-5500"] = 22.00m
};

Console.WriteLine(prices.Keys[0]);  // WH-1100 — O(1) rank access
```

---

## Q2. What is `SortedDictionary<TKey,TValue>` and what data structure underpins it?

**Concepts**
- Red-black tree (self-balancing BST) as the internal store
- O(log n) insert, remove, and lookup — no array shifting
- Keys and Values exposed as read-only `ICollection<T>` (no indexer)
- Higher per-node memory overhead than the array approach
- Preferred when entries churn frequently or n is large

**Answer**

`SortedDictionary<TKey,TValue>` is backed by a red-black tree, a self-balancing binary search tree that maintains O(log n) height regardless of insertion order. Every `Add` or `Remove` may trigger one or more tree rotations to restore the red-black invariant, but this rebalancing is bounded at O(log n) — no array shifting occurs. The trade-off is per-node heap allocation: each entry lives in an individual tree node with left, right, and parent pointers, which costs more memory and produces more GC pressure than `SortedList`'s contiguous arrays. `SortedDictionary.Keys` and `.Values` are `ICollection<TKey>` and `ICollection<TValue>` respectively — they support `foreach` and `Count` but do not expose a numeric indexer. If you need to access the first or last key you must iterate or use LINQ's `First()` / `Last()`.

```csharp
var regionSales = new SortedDictionary<string, int>
{
    ["West"]    = 4200,
    ["East"]    = 5100,
    ["Central"] = 3800
};

// Keys are ICollection<string> — no [0] indexer
foreach (string region in regionSales.Keys)
    Console.WriteLine(region);  // Central, East, West
```

---

## Q3. What are the time-complexity guarantees for `Add`, lookup by key, and `Remove` for both sorted collection types?

**Concepts**
- SortedList lookup: O(log n) binary search; insert/remove: O(n) array shift
- SortedDictionary: O(log n) for all three operations (tree traversal + rebalance)
- Dictionary average O(1) for comparison
- Amortized cost matters for bulk-load scenarios
- Array shift dominates SortedList at large n with frequent writes

**Answer**

For `SortedList<TKey,TValue>`, finding a key is always O(log n) because the key array is sorted and binary search applies. However, inserting a new key requires shifting every element after the insertion point one position to the right, making worst-case insert O(n). Remove shifts elements left by the same logic — also O(n). For `SortedDictionary<TKey,TValue>`, the red-black tree keeps all three operations at O(log n) because rebalancing never touches more than O(log n) nodes. In practice this means that a `SortedList` with 100,000 entries and frequent inserts can be dramatically slower than a `SortedDictionary` of the same size, even though both are described as "sorted" collections. `Dictionary<TKey,TValue>` still wins at O(1) average for pure lookup, but it provides no ordering guarantee.

| Operation | Dictionary | SortedList | SortedDictionary |
|---|---|---|---|
| Lookup by key | O(1) avg | O(log n) | O(log n) |
| Insert | O(1) avg | O(n) | O(log n) |
| Remove by key | O(1) avg | O(n) | O(log n) |
| Access by sorted rank | N/A | O(1) | N/A |

---

## Q4. How does `Keys[i]` on `SortedList` differ from what is available on `SortedDictionary`?

**Concepts**
- `SortedList.Keys` is `IList<TKey>` — supports numeric indexer
- `SortedDictionary.Keys` is `ICollection<TKey>` — no numeric indexer
- O(1) rank access by sorted position on `SortedList`
- `SortedDictionary` requires foreach or LINQ to reach first/last key
- `RemoveAt(index)` is also unique to `SortedList`

**Answer**

`SortedList<TKey,TValue>.Keys` returns an `IList<TKey>` view over the underlying key array. Because the array is already sorted, `Keys[0]` is the minimum key, `Keys[Count - 1]` is the maximum, and any intermediate index yields the key at that sorted rank — all in O(1). The `Values` property provides the same capability for the paired values. `SortedDictionary<TKey,TValue>.Keys` returns an `ICollection<TKey>` that only supports `foreach`, `CopyTo`, and `Count`. There is no `[i]` indexer. To find the minimum key in a `SortedDictionary` you must either iterate with `foreach` and break on the first element, or use LINQ's `First()`, which internally iterates the tree's leftmost node. This is the most common interview confusion between the two types: `SortedList` is the only one that lets you address entries by their sorted position.

```csharp
var sl = new SortedList<string, int> { ["B"] = 2, ["A"] = 1, ["C"] = 3 };
Console.WriteLine(sl.Keys[0]);     // A  — O(1)
Console.WriteLine(sl.Values[2]);   // 3  — O(1)
Console.WriteLine(sl.IndexOfKey("B")); // 1

var sd = new SortedDictionary<string, int> { ["B"] = 2, ["A"] = 1, ["C"] = 3 };
// sd.Keys[0] — COMPILE ERROR: ICollection<string> has no indexer
string first = sd.Keys.First();    // "A" via LINQ, O(log n) tree traversal
```

---

## Q5. What is `IComparer<T>` and how does it differ from `IEqualityComparer<T>`?

**Concepts**
- `IComparer<T>` defines total order — `Compare(x, y)` returns negative/zero/positive
- `IEqualityComparer<T>` defines equality and hash code — `Equals` + `GetHashCode`
- Sorted collections use `IComparer<T>`; `Dictionary` and `HashSet` use `IEqualityComparer<T>`
- `Comparer<T>.Default` uses `IComparable<T>` if available
- Passing the wrong interface to a collection constructor is a compile error

**Answer**

`IComparer<T>` imposes a total ordering on objects of type `T` through a single `Compare(T x, T y)` method that returns a negative integer when `x` precedes `y`, zero when they are considered equal in sort order, and a positive integer when `x` follows `y`. Sorted collections — `SortedList`, `SortedDictionary`, `SortedSet` — accept an `IComparer<T>` in their constructors because they need to determine *where* each key sits relative to others. `IEqualityComparer<T>`, by contrast, answers only two questions: are two values equal, and what is the hash code of a value? Hash-based collections (`Dictionary`, `HashSet`) rely entirely on this interface. A common mistake is implementing `IEqualityComparer<string>` for case-insensitive key comparison and then passing it to a `SortedDictionary` constructor — the compiler rejects it immediately because the constructor signatures are different interfaces. When you need case-insensitive sorted keys, use `StringComparer.OrdinalIgnoreCase`, which implements *both* interfaces and works correctly in sorted and hash-based contexts.

---

## Q6. How do you supply a custom sort order to `SortedList` or `SortedDictionary`?

**Concepts**
- Pass `IComparer<TKey>` as a constructor argument
- Built-in options: `StringComparer.OrdinalIgnoreCase`, `StringComparer.Ordinal`, `Comparer<T>.Create`
- Custom class implements `IComparer<TKey>` for complex domain rules
- Comparer controls duplicate detection: keys that compare equal are the same key
- Cannot change the comparer after construction

**Answer**

Both sorted collection types accept an optional `IComparer<TKey>` in one of their constructor overloads. When omitted, `Comparer<TKey>.Default` is used, which delegates to `IComparable<TKey>` on the key type. To override this, pass any object implementing `IComparer<TKey>`. For strings, `StringComparer.OrdinalIgnoreCase` is the most predictable choice for identifiers and tags because it is culture-invariant. `Comparer<T>.Create` accepts a `Comparison<T>` lambda, which is useful for one-off reverse sorts without writing a full class. For domain-specific ordering — such as a leaderboard where entries must sort by score descending, then by player name ascending as a tiebreaker — implement a dedicated class. The comparer also governs key identity: if `Compare(a, b)` returns 0, the collection treats `a` and `b` as the same key, so the choice of comparer directly affects whether a second insert throws or updates the existing entry.

```csharp
// .NET 10 — reverse numeric order via lambda
var descendingIds = new SortedList<int, string>(
    Comparer<int>.Create((a, b) => b.CompareTo(a)));

descendingIds.Add(10, "ten");
descendingIds.Add(30, "thirty");
descendingIds.Add(20, "twenty");

Console.WriteLine(descendingIds.Keys[0]);  // 30 — highest first
```

---

## Q7. What does `IndexOfKey` do on `SortedList` and what algorithm does it use?

**Concepts**
- Binary search on the sorted key array
- O(log n) complexity
- Returns -1 when the key is absent (unlike indexer, which throws)
- Paired with `RemoveAt(index)` for positional deletion
- `IndexOfValue` also exists but uses O(n) linear scan

**Answer**

`SortedList<TKey,TValue>.IndexOfKey(key)` performs a binary search over the internal key array and returns the zero-based index of the matching key in sorted order, or -1 if the key is not present. This is O(log n) and is the efficient way to answer "what rank does this key occupy?" without iterating. It is commonly paired with `RemoveAt(index)`, which takes the rank directly and avoids a second search. `IndexOfValue(value)` is a different operation: it scans the value array linearly — O(n) — because values are not sorted. Use `ContainsValue` or `IndexOfValue` sparingly; if you regularly look up by value, a reverse-index `Dictionary<TValue, TKey>` is the right tool. `SortedDictionary` has neither `IndexOfKey` nor `IndexOfValue` because there is no positional array to search.

```csharp
var skus = new SortedList<string, decimal>
{
    ["WH-1100"] = 8.25m,
    ["WH-3300"] = 11.75m,
    ["WH-9920"] = 14.50m
};

int rank = skus.IndexOfKey("WH-3300");  // 1
skus.RemoveAt(rank);                    // removes WH-3300 in O(n)
Console.WriteLine(skus.Count);          // 2
```

---

## Q8. What exception does `Add` throw when a duplicate key is inserted, and how does the indexer behave differently?

**Concepts**
- `Add(key, value)` throws `ArgumentException` when key already exists
- Indexer `this[key] = value` adds when absent, silently updates when present
- Same semantics as `Dictionary<TKey,TValue>`
- Duplicate detection is comparer-relative, not reference-relative
- `TryAdd` is not available on sorted collections (use ContainsKey + Add or indexer)

**Answer**

Calling `Add(key, value)` on either `SortedList` or `SortedDictionary` with a key that already exists (as determined by the collection's `IComparer<TKey>`) throws `ArgumentException` with the message "An item with the same key has already been added." This mirrors `Dictionary<TKey,TValue>.Add`. The indexer `collection[key] = value` is the upsert path: it inserts a new entry when the key is absent and overwrites the existing value when it is present, without throwing. Choosing between them is a design contract: use `Add` when a duplicate signals a programming error or data integrity problem, use the indexer when insert-or-update semantics are intentional. One subtle point: "duplicate" is defined by the comparer, not by reference equality. If you constructed the collection with `StringComparer.OrdinalIgnoreCase`, then `"CSharp"` and `"csharp"` are the same key — the second indexer assignment updates the entry, while a second `Add` would throw.

---

## Q9. What is the `Capacity` property on `SortedList` and when would you call `TrimExcess`?

**Concepts**
- `Capacity` is the current pre-allocated length of the internal arrays
- Doubles when Count reaches Capacity, like `List<T>`
- Constructor overload accepts an initial capacity to avoid reallocations
- `TrimExcess()` shrinks Capacity to Count when Capacity > Count * 1.1
- `SortedDictionary` has no Capacity — each node is heap-allocated independently

**Answer**

`SortedList<TKey,TValue>` allocates two arrays (keys and values) whose length is the `Capacity`. When an `Add` call would exceed the current capacity, the runtime allocates new arrays at double the current size and copies all elements into them — identical to `List<T>` resizing. This means that loading many entries one at a time with unsorted keys triggers both repeated copies and per-insert array shifts, making bulk construction expensive. If you know the final size in advance, pass it to the constructor (`new SortedList<string, int>(500)`) to avoid all intermediate reallocations. After removing a large number of entries, the internal arrays retain their allocated length. Call `TrimExcess()` to reduce `Capacity` down to `Count` and reclaim memory; the method only acts when `Capacity > Count * 1.1` to avoid repeated trimming near the threshold. `SortedDictionary` has no analogous property because each entry is an individually allocated tree node.

---

## Q10. What is `SortedSet<T>` and how does it compare to `SortedList` and `SortedDictionary`?

**Concepts**
- `SortedSet<T>` stores unique values only — no key/value pairs
- Backed by a red-black tree (same as `SortedDictionary`)
- O(log n) Add, Remove, Contains
- `GetViewBetween(min, max)` provides range query without LINQ
- `Min` and `Max` properties for O(log n) boundary access

**Answer**

`SortedSet<T>` is a sorted collection of unique values with no associated mapped value — it is the sorted counterpart of `HashSet<T>`. Like `SortedDictionary`, it uses a red-black tree, giving O(log n) for `Add`, `Remove`, and `Contains`. Its distinguishing feature is `GetViewBetween(lower, upper)`, which returns a live view — backed by the same tree — of all elements whose sort order falls between the two bounds, enabling efficient range scans without allocating a new collection. `Min` and `Max` properties return the boundary values in O(log n). Compare this to `SortedList` and `SortedDictionary`, which are always key-to-value maps. Use `SortedSet<T>` when you need sorted membership testing or range enumeration on a set of values, not when you need to associate each key with a payload.

```csharp
var timestamps = new SortedSet<DateOnly>
{
    new(2025, 6, 1), new(2025, 3, 15), new(2025, 9, 10)
};

var summer = timestamps.GetViewBetween(new(2025, 6, 1), new(2025, 8, 31));
foreach (var d in summer)
    Console.WriteLine(d);  // 2025-06-01
```

---

## Q11. When should you prefer `SortedList` over `SortedDictionary` in practice?

**Concepts**
- Prefer `SortedList` for small, mostly static maps
- O(1) rank access (`Keys[i]`, `Values[i]`) unique to `SortedList`
- Cache-friendly contiguous memory layout benefits sequential reads
- Prefer `SortedDictionary` when entries churn frequently or n is large
- Memory per entry is lower in `SortedList` (no node pointers)

**Answer**

Choose `SortedList<TKey,TValue>` when two conditions hold: the collection is relatively small or loaded once and then read many times without many inserts and removes during its lifetime, and you need access to entries by their sorted rank (`Keys[0]`, `Values[i]`, `RemoveAt(index)`). The contiguous memory layout of `SortedList` gives better CPU cache utilization when iterating all keys or values sequentially, which matters for small reports and configuration tables. It also uses less memory per entry than `SortedDictionary` because there are no tree node pointers. Choose `SortedDictionary<TKey,TValue>` when the collection is large, when inserts and removes happen throughout the collection's lifetime (not just at initial load), or when the keys arrive roughly in random order in bulk. The O(n) per-insert shift cost of `SortedList` compounds badly: at 100,000 randomly ordered inserts, array shifting can be orders of magnitude slower than the O(log n) tree rebalance of `SortedDictionary`.

---

## Q12. How do `ContainsKey` and `ContainsValue` differ in time complexity on `SortedList`?

**Concepts**
- `ContainsKey`: O(log n) binary search on sorted key array
- `ContainsValue`: O(n) linear scan of value array
- Values are not indexed or sorted — no binary search applies
- Frequent value lookups warrant a reverse-index `Dictionary<TValue, TKey>`
- `SortedDictionary` has no `ContainsValue` — use `Values.Contains` (also O(n))

**Answer**

`SortedList<TKey,TValue>.ContainsKey(key)` exploits the sorted key array with a binary search and completes in O(log n). `ContainsValue(value)` has no such shortcut because the value array is not sorted — the runtime walks every element until it finds a match or exhausts the array, making it O(n). This is the same asymmetry as `Dictionary<TKey,TValue>`, which also has O(1) key lookup and O(n) value lookup. If your code frequently answers "does this value appear anywhere in the map?", consider maintaining a secondary `HashSet<TValue>` or a bidirectional index. `SortedDictionary` omits `ContainsValue` entirely; the equivalent is `sortedDict.Values.Contains(value)`, which iterates the tree's values in O(n).

---

## Q13. What is the non-generic `SortedList` and why should it be avoided in modern code?

**Concepts**
- `System.Collections.SortedList` — pre-generics, stores `object` keys and values
- Value types box on insert and unbox on read
- No compile-time type safety — runtime `InvalidCastException` possible
- Predates .NET 2.0 generics; exists only for legacy compatibility
- Prefer `SortedList<TKey,TValue>` for all new code

**Answer**

`System.Collections.SortedList` is the non-generic sorted map introduced before generics arrived in .NET 2.0. It stores all keys and values as `object`, which means that value types such as `int` or `decimal` are boxed to heap objects on every insert and unboxed on every read — adding GC pressure and reducing throughput. There is no compile-time guarantee about the types of keys or values, so a cast on retrieval can throw `InvalidCastException` at runtime. The API (`GetKey(index)`, `GetByIndex(index)`) is less intuitive than the generic version. The only reason to encounter this type is when maintaining code written before .NET 2.0 or when working with a legacy API that returns `System.Collections.SortedList`. In all new code, use `SortedList<TKey,TValue>`.

---

## Q14. What interfaces does `SortedList<TKey,TValue>` implement that `SortedDictionary<TKey,TValue>` does not?

**Concepts**
- `SortedList` implements `IDictionary<TKey,TValue>` and also exposes `Keys` as `IList<TKey>`
- `IList<TKey>` adds a numeric indexer, `IndexOf`, and positional `RemoveAt` to the Keys view
- `SortedDictionary.Keys` is `ICollection<TKey>` — only `foreach`, `Count`, `CopyTo`
- `SortedList` additionally implements non-generic `IDictionary` and `IList` via its Keys/Values views
- This difference is the root cause of all `Keys[i]` compile errors on `SortedDictionary`

**Answer**

Both types implement `IDictionary<TKey,TValue>`, `ICollection<KeyValuePair<TKey,TValue>>`, `IEnumerable<KeyValuePair<TKey,TValue>>`, and their non-generic equivalents. The meaningful difference is in their `Keys` and `Values` properties. `SortedList<TKey,TValue>.Keys` returns `IList<TKey>`, which inherits from `ICollection<TKey>` and adds an integer indexer (`this[int index]`), `IndexOf`, and — through the non-generic `IList` — `RemoveAt`. `SortedDictionary<TKey,TValue>.Keys` returns only `ICollection<TKey>`, which has no positional indexer. This is a deliberate design choice: the tree structure of `SortedDictionary` cannot provide O(1) rank access, so the interface does not promise it. When a method parameter is typed as `IDictionary<string, int>`, both types satisfy it, but the rank-access methods on `Keys` are only reachable through the concrete `SortedList` type.

---

## Q15. What happens at runtime when `TKey` has no `IComparable<TKey>` implementation and no `IComparer<TKey>` is supplied?

**Concepts**
- `Comparer<T>.Default` resolution happens at runtime, not compile time
- `InvalidOperationException` is thrown at the first comparison
- Not a compile error — the compiler cannot verify `IComparable<T>` on a type parameter
- Resolution order: `IComparable<T>` → `IComparable` → exception
- Solution: supply an explicit `IComparer<T>` or constrain `TKey : IComparable<TKey>`

**Answer**

When neither the constructor argument nor the key type provides ordering information, `Comparer<TKey>.Default` is the fallback. At the moment the collection first needs to compare two keys — typically the second `Add` call — `Comparer<TKey>.Default` reflects on `TKey` to find `IComparable<TKey>` or the legacy `IComparable`. If neither interface is implemented, it throws `InvalidOperationException` with a message such as "Failed to compare two elements in the array." This is a runtime failure, not a compile-time error, because generics in C# do not allow you to constrain `TKey : IComparable<TKey>` at the call site of a framework collection constructor. The fix is either to implement `IComparable<TKey>` on the key type or to pass an explicit `IComparer<TKey>` to the collection constructor. The collection holds onto the comparer it was constructed with; it cannot be replaced later without recreating the collection.

---

## Q16. How does `Comparer<T>.Default` determine which ordering to use for a type?

**Concepts**
- Checks `T implements IComparable<T>` first — generic, type-safe path
- Falls back to `T implements IComparable` (non-generic, boxing path)
- Throws `InvalidOperationException` at runtime if neither is found
- `string` uses ordinal byte-order comparison through `IComparable<string>`
- Explicit `IComparer<T>` always overrides `Comparer<T>.Default`

**Answer**

`Comparer<T>.Default` is a cached singleton that wraps whatever comparison logic is available for `T`. It first checks whether `T` implements `IComparable<T>`; if so, it delegates to `CompareTo(T other)` directly — no boxing occurs for value types. If `T` does not implement the generic interface, it checks for the non-generic `IComparable`, which accepts `object` parameters and therefore boxes value-type arguments on every call. If neither interface is found, the singleton is still created but defers the error to the first comparison call — it throws `InvalidOperationException` at that point rather than at the site where `Default` was accessed. This means a missing `IComparable` implementation on a key type passes compilation silently and only explodes at runtime when the collection first needs to order two keys. Providing an explicit `IComparer<T>` in the constructor bypasses `Comparer<T>.Default` entirely.

---

## Q17. How do sorted collections handle thread safety, and what options exist for concurrent sorted scenarios?

**Concepts**
- Neither `SortedList` nor `SortedDictionary` is thread-safe
- Concurrent reads are safe; any concurrent write is not
- `lock` statement provides the simplest mutual exclusion
- `ImmutableSortedDictionary<TKey,TValue>` for read-heavy, infrequent-write scenarios
- No `ConcurrentSortedDictionary` in the BCL — sorted + concurrent requires external locking or immutable snapshots

**Answer**

`SortedList<TKey,TValue>` and `SortedDictionary<TKey,TValue>` are not thread-safe. Concurrent reads from multiple threads without any write are safe because no state is mutated. Any concurrent write — or a write concurrent with any read — can corrupt the internal structure: for `SortedList` the array shift can leave keys and values momentarily misaligned; for `SortedDictionary` the tree rotation can expose a partially rebalanced state. The BCL does not include a `ConcurrentSortedDictionary`. The common options are: wrap the collection in a `lock` statement for both reads and writes when contention is low; use `ReaderWriterLockSlim` when reads vastly outnumber writes; or use `ImmutableSortedDictionary<TKey,TValue>` from `System.Collections.Immutable`, which never mutates in place — updates produce a new tree that shares unchanged subtrees (structural sharing), making the reference swap atomic. `ImmutableSortedDictionary` is ideal when the sorted map is built once and then referenced by many concurrent readers.

---

## Gotchas

---

## Q18. Why does `sortedDict.Keys[0]` fail to compile even though `sortedList.Keys[0]` works fine?

**Concepts**
- `SortedDictionary.Keys` returns `ICollection<TKey>` — no integer indexer
- `SortedList.Keys` returns `IList<TKey>` — has integer indexer
- Both collections look identical at the `IDictionary<K,V>` level, masking the difference
- Common mistake when refactoring from `SortedList` to `SortedDictionary`
- Compiler error CS0021: cannot apply indexing with [] to an expression of type `ICollection<T>`

**Answer**

`SortedDictionary<TKey,TValue>.Keys` is typed as `SortedDictionary<TKey,TValue>.KeyCollection`, which implements `ICollection<TKey>` — an interface that provides `foreach`, `Count`, and `CopyTo` but has no numeric indexer. `SortedList<TKey,TValue>.Keys` implements `IList<TKey>`, which inherits from `ICollection<TKey>` and adds the `this[int index]` indexer. The compiler enforces this: `sortedDict.Keys[0]` causes CS0021 because `ICollection<TKey>` does not define `this[int]`. The gotcha appears when a developer switches a field from `SortedList` to `SortedDictionary` to reduce insert cost, then discovers the rank-access code no longer compiles. If rank access is required, keep `SortedList`. If it is not required and you only need min or max key, iterate `Keys` with `foreach` or call `Keys.First()` via LINQ. Never cast to `IList<T>` at runtime hoping the underlying type provides it — `SortedDictionary.KeyCollection` does not implement `IList<TKey>`.

---

## Q19. What happens when you pass `IEqualityComparer<TKey>` instead of `IComparer<TKey>` to a sorted collection constructor?

**Concepts**
- Sorted collection constructors require `IComparer<TKey>` — a different interface
- `IEqualityComparer<TKey>` defines hash-based equality, not total order
- The compiler rejects the wrong interface with CS1503 (argument type mismatch)
- `StringComparer` implements both interfaces — it satisfies either constructor
- Custom `IEqualityComparer` classes have no `Compare` method and cannot be adapted

**Answer**

The constructor overloads of `SortedList<TKey,TValue>` and `SortedDictionary<TKey,TValue>` that accept a comparer are typed as `IComparer<TKey>`, not `IEqualityComparer<TKey>`. If you pass an object that implements `IEqualityComparer<TKey>` but not `IComparer<TKey>`, the compiler emits CS1503. This happens most often when a developer has an existing `IEqualityComparer<string>` for a `Dictionary` or `HashSet` and tries to reuse it for a sorted collection: the two interfaces are unrelated in the type hierarchy. The cleanest fix for string keys is to use one of the `StringComparer` singletons (`StringComparer.OrdinalIgnoreCase`, `StringComparer.Ordinal`, etc.) — the `StringComparer` abstract class implements both `IComparer<string>` and `IEqualityComparer<string>`, so its instances satisfy either constructor. For custom key types, you must implement `IComparer<TKey>` specifically — there is no adapter in the BCL to convert an `IEqualityComparer` into an `IComparer`.

---

## Q20. How can a locale-sensitive `StringComparer` cause duplicate logical keys in a sorted map on a different server?

**Concepts**
- `StringComparer.CurrentCulture` delegates to the host OS culture at runtime
- Turkish locale: dotted-I (`İ`) and dotless-I (`ı`) rules change case comparisons
- Keys that compare equal on a developer's machine may differ on a production server
- `OrdinalIgnoreCase` is culture-invariant — behaviour is identical on every host
- Use culture-sensitive comparers only for UI display sort, not for key identity

**Answer**

`StringComparer.CurrentCulture` resolves to the culture of the thread or process at runtime. In the Turkish locale, the uppercase form of `"i"` is `"İ"` (dotted capital I), not `"I"` — so a case-insensitive comparison under `CurrentCulture` on a Turkish server treats `"image"` and `"IMAGE"` as different keys, while the same code on an English developer workstation treats them as the same key. The result is that a `SortedDictionary<string, T>` constructed with `StringComparer.CurrentCultureIgnoreCase` may silently accumulate duplicate logical keys on a server with a different culture, or throw `ArgumentException` from `Add` where none was expected. The fix is `StringComparer.OrdinalIgnoreCase`, which compares bytes directly without any culture-specific folding rules and produces identical results on every host. Reserve `CurrentCulture` for user-visible display sort orders (alphabetical lists presented to a Turkish-speaking user), never for canonical key identity in services.

---

## Q21. Why does `SortedList` perform poorly when entries are inserted in random order at large scale?

**Concepts**
- Each insert: binary search O(log n) to find position, then O(n) array shift
- Random order means the insertion point is on average in the middle — maximum shift
- Total work for n random inserts: O(n²) in the worst case
- `SortedDictionary` costs O(n log n) total for n random inserts (tree rebalance only)
- Bulk-loading sorted data into `SortedList` avoids the worst shifts

**Answer**

When you insert entries into `SortedList` in random key order, each insertion lands somewhere in the middle of the array on average. After the binary search locates that position, every element to the right of it must shift one slot — and for a random distribution the average shift length is n/2. For n insertions the expected total shift work is n × (n/2) = O(n²). In practice this is observable: loading 100,000 randomly keyed entries into `SortedList` can take seconds, while the same entries in `SortedDictionary` take milliseconds because tree rebalance is bounded at O(log n) per insert regardless of key distribution. The only scenario where `SortedList` matches `SortedDictionary` for bulk construction is when entries arrive in already-sorted order, because then every insert appends to the end and the shift cost is zero. If you must use `SortedList` for its rank-access feature and your data arrives unsorted, collect entries in a `List<KeyValuePair<TKey,TValue>>`, sort the list once with `List.Sort`, then populate the `SortedList` from the constructor that accepts `IDictionary<TKey,TValue>`.

---

## Q22. How can comparing two keys that are equal by reference still result in a sorted collection treating them as different keys?

**Concepts**
- Sorted collections use `IComparer<TKey>.Compare`, not reference equality
- Custom comparers define key identity — `Compare(a, b) == 0` means "same key"
- An inconsistent or poorly implemented `Compare` can declare two distinct objects as different keys
- Value-type keys with identical values are always the same key (no reference ambiguity)
- Mutable keys whose sort-relevant fields change after insertion corrupt the collection

**Answer**

In sorted collections, two keys are considered equal — and therefore the same key — when `comparer.Compare(a, b)` returns 0. Reference equality is never consulted. This creates two related gotchas. First, a custom `IComparer<TKey>` that is inconsistent (violates the antisymmetry or transitivity requirements of a total order) can cause the binary search to miss an existing key, resulting in duplicate logical entries or `ArgumentException` on seemingly valid operations. Second, if you use a mutable class as a key and then change the fields that the comparer reads, the collection's internal invariant is broken: a subsequent lookup using the modified key runs binary search based on the new value but the node is filed at the old position — the key becomes unreachable. `SortedList` and `SortedDictionary` both assume keys are immutable with respect to their ordering once inserted. Use immutable types (`string`, `int`, `record` structs) or types you can guarantee will not change after insertion.

---

## Real-World Scenarios

---

## Q23. A price-catalog API returns the cheapest and most expensive SKUs on every request. A developer writes the following. Review it.

```csharp
public class CatalogService
{
    private readonly SortedDictionary<string, decimal> _prices = new();

    public void Load(IEnumerable<(string Sku, decimal Price)> catalog)
    {
        foreach (var (sku, price) in catalog)
            _prices[sku] = price;
    }

    public (string MinSku, string MaxSku) GetBoundarySkus()
    {
        string min = _prices.Keys[0];
        string max = _prices.Keys[_prices.Count - 1];
        return (min, max);
    }
}
```

**Concepts**
- `SortedDictionary.Keys` is `ICollection<TKey>` — no integer indexer
- CS0021 compile error on `Keys[0]` and `Keys[Count - 1]`
- `SortedList.Keys` is `IList<TKey>` — supports rank access
- Iterating Keys to capture first/last is the SortedDictionary-compatible workaround
- LINQ `First()` / `Last()` on SortedDictionary.Keys are O(n) tree walks

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Keys[0]` on `SortedDictionary<string,decimal>` | CS0021 — `ICollection<string>` has no indexer; code does not build |
| API mismatch | Assumed both sorted types expose rank indexing | Wrong collection type chosen for a feature that requires rank access |
| Performance (if LINQ workaround applied) | `Keys.First()` / `Keys.Last()` each iterate the tree | Two O(n) traversals per request instead of O(1) on a SortedList |

**Fix priority**

1. Switch the backing field to `SortedList<string, decimal>` — it exposes `Keys` as `IList<string>`, making `Keys[0]` and `Keys[Count - 1]` legal O(1) calls.
2. If insert frequency is high and the `SortedList` shift cost is a concern, keep `SortedDictionary` and track min/max explicitly during `Load` instead of computing them on each request.
3. Do not use `Keys.First()` / `Keys.Last()` on every request — each is an O(n) enumeration of the tree.

```csharp
// .NET 10 — fix using SortedList for O(1) rank access
public class CatalogService
{
    private readonly SortedList<string, decimal> _prices = new();

    public void Load(IEnumerable<(string Sku, decimal Price)> catalog)
    {
        foreach (var (sku, price) in catalog)
            _prices[sku] = price;
    }

    public (string MinSku, string MaxSku) GetBoundarySkus() =>
        (_prices.Keys[0], _prices.Keys[_prices.Count - 1]);
}
```

---

## Q24. A telemetry service ingests zone-level pallet counts every few seconds. A developer writes the following hot-path upsert. Review it.

```csharp
public class WarehouseTelemetry
{
    private readonly SortedList<int, int> _countsByZone = new();

    public void UpsertZone(int zoneId, int palletCount)
    {
        if (_countsByZone.ContainsKey(zoneId))
            _countsByZone[zoneId] = palletCount;
        else
            _countsByZone.Add(zoneId, palletCount);
    }

    public IEnumerable<int> ZonesInOrder() => _countsByZone.Keys;
}
```

**Concepts**
- `SortedList` insert shifts the key/value arrays — O(n) per new key
- High-frequency upsert on a growing SortedList leads to O(n²) total insert cost
- `SortedDictionary` provides O(log n) insert (tree rebalance, no shift)
- The `ContainsKey` + `Add` idiom is an unnecessary double lookup — the indexer handles both
- Sorted foreach is preserved by both types

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Performance | `SortedList.Add` on a new zone id shifts all higher zone ids | Insert latency grows linearly with the number of zones; at 500 zones, each new insert shifts ~250 elements on average |
| Idiom | `ContainsKey` check followed by conditional `Add` or indexer | Two O(log n) lookups; minor cost but unnecessary — indexer alone handles the upsert |
| Collection choice | `SortedList` for a write-heavy sync service | Wrong tool per Section 8 guidance; `SortedDictionary` is correct when entries churn |

**Fix priority**

1. Replace `SortedList<int, int>` with `SortedDictionary<int, int>` — all insert and remove operations become O(log n) with no array shifting.
2. Collapse the upsert to a single indexer assignment — it adds when absent and updates when present in one step.
3. Keep `SortedList` only for small, mostly static maps such as nightly config tables or reorder reports where rank-based access (`Keys[i]`) is needed.

```csharp
// .NET 10 — fix
public class WarehouseTelemetry
{
    private readonly SortedDictionary<int, int> _countsByZone = new();

    public void UpsertZone(int zoneId, int palletCount) =>
        _countsByZone[zoneId] = palletCount;

    public IEnumerable<int> ZonesInOrder() => _countsByZone.Keys;
}
```

---

## Q25. A catalog normalization service stores product tags in a sorted map with case-insensitive key matching. After deploying to a Turkish-locale server, QA reports duplicate entries for tags such as `"image"` and `"IMAGE"`. A developer writes the following. Review it.

```csharp
public class TagIndex
{
    private readonly SortedDictionary<string, int> _tagCounts =
        new(StringComparer.CurrentCultureIgnoreCase);

    public void IncrementTag(string tag)
    {
        if (_tagCounts.TryGetValue(tag, out int count))
            _tagCounts[tag] = count + 1;
        else
            _tagCounts[tag] = 1;
    }

    public IReadOnlyDictionary<string, int> Snapshot() => _tagCounts;
}
```

**Concepts**
- `StringComparer.CurrentCultureIgnoreCase` uses runtime OS culture
- Turkish locale: dotted-I rule makes `"i"` and `"I"` non-equivalent under case fold
- `OrdinalIgnoreCase` is culture-invariant — identical on every platform
- Culture-sensitive comparers are appropriate for UI display, not for key identity
- Locale-driven bugs surface only on servers with non-English cultures

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `CurrentCultureIgnoreCase` delegates to OS culture | Tags identical to developers on en-US appear as distinct keys on a Turkish server |
| Environment disparity | Dev environment passes; production fails | Difficult to reproduce and diagnose without culture replication |
| Key identity | Comparer-relative equality determines "same tag" | Two entries for `"image"` / `"IMAGE"` inflate counts and corrupt reports |

**Fix priority**

1. Replace `StringComparer.CurrentCultureIgnoreCase` with `StringComparer.OrdinalIgnoreCase` — byte-level comparison, identical on every host.
2. Collapse the TryGetValue + update pattern to a single additive indexer expression to remove the redundant lookup.
3. Add a unit test that constructs the index with `CultureInfo.GetCultureInfo("tr-TR")` as the current culture and verifies that `"image"` and `"IMAGE"` map to the same entry.

```csharp
// .NET 10 — fix
public class TagIndex
{
    private readonly SortedDictionary<string, int> _tagCounts =
        new(StringComparer.OrdinalIgnoreCase);

    public void IncrementTag(string tag)
    {
        _tagCounts.TryGetValue(tag, out int count);
        _tagCounts[tag] = count + 1;
    }

    public IReadOnlyDictionary<string, int> Snapshot() => _tagCounts;
}
```

---

## Q26. You are building a game leaderboard API. The board holds up to 10,000 active players and must return the top-10 entries sorted by score descending, then by player name ascending as a tiebreaker. Describe your design using sorted collections.

**Concepts**
- `IComparer<T>` on a custom key type to enforce score-desc, name-asc order
- `SortedList<LeaderboardEntry, string>` gives O(1) access to top-N by rank
- `SortedDictionary<LeaderboardEntry, string>` is better under frequent score updates
- Comparer must be consistent: transitivity and antisymmetry required
- Null guards in `Compare` prevent `NullReferenceException` at tree rotation time

**Answer**

Model each player's standing as a key object `LeaderboardEntry { string Player; int Score }` and implement `IComparer<LeaderboardEntry>` to sort by `Score` descending first, then by `Player` ascending as a tiebreaker. Passing this comparer to a `SortedList<LeaderboardEntry, string>` means `Keys[0]` is always the leading player and `Keys[1..9]` are ranks two through ten — the top-N report becomes a simple loop with no LINQ sorting. If players update their scores frequently, switch to `SortedDictionary<LeaderboardEntry, string>` to eliminate O(n) array shifts on each score change: remove the old entry and add the updated key in O(log n) each. Ensure the comparer handles nulls explicitly to avoid `NullReferenceException` during tree rebalance. Critically, when a player's score changes you must remove the old `LeaderboardEntry` key and insert a new one — mutating the key's `Score` field after insertion breaks the tree's sorted invariant and makes the entry unreachable.

```csharp
// .NET 10
public sealed class LeaderboardComparer : IComparer<LeaderboardEntry>
{
    public static readonly LeaderboardComparer Instance = new();

    public int Compare(LeaderboardEntry? x, LeaderboardEntry? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return  1;

        int byScore = y.Score.CompareTo(x.Score);   // descending
        return byScore != 0
            ? byScore
            : string.Compare(x.Player, y.Player, StringComparison.Ordinal);
    }
}

var board = new SortedList<LeaderboardEntry, string>(LeaderboardComparer.Instance);
board[new LeaderboardEntry("Jordan", 9200)] = "Gold";
board[new LeaderboardEntry("Alex",   8800)] = "Gold";
board[new LeaderboardEntry("Sam",    7500)] = "Silver";

Console.WriteLine(board.Keys[0].Player);  // Jordan — highest score
```

---

## Q27. A scheduling service stores calendar events keyed by `DateOnly`. On each request it must return all events that fall within a user-supplied date window efficiently. How would you implement this with sorted collections, and what are the trade-offs?

**Concepts**
- `SortedList` or `SortedDictionary` for sorted-key range scan via linear traversal
- `SortedSet<T>.GetViewBetween` for range queries on value-only sets
- No built-in O(log n) range-start seek on `SortedList` / `SortedDictionary`
- `IndexOfKey` on `SortedList` locates the start position in O(log n), then iterate forward
- `SortedDictionary` has no start-position seek — full key scan from the first entry

**Answer**

For a range query `[from, to]` over a `SortedList<DateOnly, Event>`, call `IndexOfKey(from)` to binary-search the start boundary in O(log n). If the exact date is absent, `IndexOfKey` returns -1, so use a LINQ-style approach or an `Array.BinarySearch`-style fallback to find the first key that is greater than or equal to `from`. Once you have the start index, walk forward through `Keys` until `Keys[i] > to`, yielding `Values[i]` at each step. The total cost is O(log n + k) where k is the number of entries in the window. With `SortedDictionary<DateOnly, Event>`, there is no start-position index, so the range scan must begin from the first entry and skip keys below `from` — O(n) in the worst case rather than O(log n + k). For value-only (keyless) scenarios, `SortedSet<DateOnly>.GetViewBetween(from, to)` returns a live bounded view of the tree with O(log n) seek. If multiple events share a date, key the map by `(DateOnly, Guid)` or store a `List<Event>` as the value to handle multi-event days.

```csharp
// .NET 10 — O(log n + k) range scan on SortedList
static IEnumerable<Event> EventsInWindow(
    SortedList<DateOnly, Event> calendar,
    DateOnly from, DateOnly to)
{
    int start = calendar.IndexOfKey(from);
    if (start < 0) start = ~start;   // bitwise complement trick not available on SortedList;
                                     // fallback: linear seek or maintain a sorted list of distinct dates

    for (int i = start; i < calendar.Count && calendar.Keys[i] <= to; i++)
        yield return calendar.Values[i];
}
```

---

## Q28. A developer migrates a `Dictionary<string, int>` helper to a sorted collection and copies the existing custom comparer. Review the following.

```csharp
public class SkuEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y) =>
        string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode(string obj) =>
        obj.ToUpperInvariant().GetHashCode();
}

public class ReorderService
{
    private readonly SortedList<string, int> _qtys =
        new(new SkuEqualityComparer());  // intended: case-insensitive key match

    public void Set(string sku, int qty) => _qtys[sku] = qty;
}
```

**Concepts**
- `SortedList` constructor requires `IComparer<TKey>`, not `IEqualityComparer<TKey>`
- CS1503 compile error — argument type mismatch
- `SkuEqualityComparer` has no `Compare` method — cannot be adapted
- `StringComparer.OrdinalIgnoreCase` implements both interfaces — correct replacement
- Keeping `SkuEqualityComparer` for `Dictionary` / `HashSet` is still valid

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Compile | `IEqualityComparer<string>` passed to `SortedList` constructor | CS1503 — no overload accepts `IEqualityComparer<TKey>`; build fails |
| Conceptual | Equality + hash ≠ total order | `IEqualityComparer` answers "are these equal?" — sorted collections also need "which comes first?" |
| Reuse | `SkuEqualityComparer` cannot be cast or adapted to `IComparer<string>` | The interfaces are unrelated; no built-in adapter exists |

**Fix priority**

1. Replace `new SkuEqualityComparer()` with `StringComparer.OrdinalIgnoreCase` — this singleton implements both `IComparer<string>` and `IEqualityComparer<string>`.
2. Keep `SkuEqualityComparer` for `Dictionary<string, int>` and `HashSet<string>` where it is correct and sufficient.
3. If a fully custom sort rule is needed beyond case folding, implement `IComparer<string>` directly — it requires only a `Compare(string? x, string? y)` method.

```csharp
// .NET 10 — fix
public class ReorderService
{
    private readonly SortedList<string, int> _qtys =
        new(StringComparer.OrdinalIgnoreCase);

    public void Set(string sku, int qty) => _qtys[sku] = qty;
}
```
