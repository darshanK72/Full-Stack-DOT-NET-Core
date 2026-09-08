# Dictionary&lt;TKey, TValue&gt; — Interview Q&A


## Table of Contents

1. [Q1. What is `Dictionary<TKey, TValue>` and how does it work internally as a hash table?](#q1-what-is-dictionarytkey-tvalue-and-how-does-it-work-internally-as-a-hash-table)
2. [Q2. What are the time complexities of the core `Dictionary<TKey, TValue>` operations?](#q2-what-are-the-time-complexities-of-the-core-dictionarytkey-tvalue-operations)
3. [Q3. What is the difference between the indexer getter and `TryGetValue` for reading a value?](#q3-what-is-the-difference-between-the-indexer-getter-and-trygetvalue-for-reading-a-value)
4. [Q4. What is `KeyNotFoundException`, when is it thrown, and how do you avoid it?](#q4-what-is-keynotfoundexception-when-is-it-thrown-and-how-do-you-avoid-it)
5. [Q5. What is the difference between `Add` and the indexer setter, and when should you use each?](#q5-what-is-the-difference-between-add-and-the-indexer-setter-and-when-should-you-use-each)
6. [Q6. What is `TryAdd` and how does it differ from `Add` and the indexer setter?](#q6-what-is-tryadd-and-how-does-it-differ-from-add-and-the-indexer-setter)
7. [Q7. When should you use `ContainsKey` versus `TryGetValue`?](#q7-when-should-you-use-containskey-versus-trygetvalue)
8. [Q8. What rules must a custom reference-type key satisfy to work correctly in `Dictionary`?](#q8-what-rules-must-a-custom-reference-type-key-satisfy-to-work-correctly-in-dictionary)
9. [Q9. How does the initial capacity hint affect `Dictionary` performance?](#q9-how-does-the-initial-capacity-hint-affect-dictionary-performance)
10. [Q10. What equality comparer does `Dictionary` use by default, and how do you supply a custom one?](#q10-what-equality-comparer-does-dictionary-use-by-default-and-how-do-you-supply-a-custom-one)
11. [Q11. What is `StringComparer.OrdinalIgnoreCase` and when should you prefer it over `StringComparer.CurrentCultureIgnoreCase`?](#q11-what-is-stringcomparerordinalignorecase-and-when-should-you-prefer-it-over-stringcomparercurrentcultureignorecase)
12. [Q12. What is the iteration order of `Dictionary<TKey, TValue>`, and can you rely on it?](#q12-what-is-the-iteration-order-of-dictionarytkey-tvalue-and-can-you-rely-on-it)
13. [Q13. What does `foreach` yield when iterating a `Dictionary<TKey, TValue>`, and what is the deconstruction syntax?](#q13-what-does-foreach-yield-when-iterating-a-dictionarytkey-tvalue-and-what-is-the-deconstruction-syntax)
14. [Q14. What happens when you pass `null` as a key to `Dictionary<TKey, TValue>`?](#q14-what-happens-when-you-pass-null-as-a-key-to-dictionarytkey-tvalue)
15. [Q15. What is `ConcurrentDictionary<TKey, TValue>` and what thread-safety guarantees does it provide?](#q15-what-is-concurrentdictionarytkey-tvalue-and-what-thread-safety-guarantees-does-it-provide)
16. [Q16. What is `ImmutableDictionary<TKey, TValue>` and when should you choose it?](#q16-what-is-immutabledictionarytkey-tvalue-and-when-should-you-choose-it)
17. [Q17. How does `Dictionary` handle hash collisions, and what is a hash-flooding attack?](#q17-how-does-dictionary-handle-hash-collisions-and-what-is-a-hash-flooding-attack)
18. [Q18. What is `CollectionsMarshal.GetValueRefOrNullRef` and when is it relevant?](#q18-what-is-collectionsmarshalgetvaluerefornullref-and-when-is-it-relevant)
19. [Q19. What is the `InvalidOperationException: Collection was modified` error and how do you avoid it?](#q19-what-is-the-invalidoperationexception-collection-was-modified-error-and-how-do-you-avoid-it)
20. [Q20. Why does mutating a reference-type key object after insertion silently break lookups?](#q20-why-does-mutating-a-reference-type-key-object-after-insertion-silently-break-lookups)
21. [Q21. Why does `ConcurrentDictionary.GetOrAdd` with a factory delegate sometimes call the factory more than once?](#q21-why-does-concurrentdictionarygetoradd-with-a-factory-delegate-sometimes-call-the-factory-more-than-once)
22. [Q22. What is the difference between `Dictionary` iteration order and insertion order, and why does relying on apparent insertion order fail?](#q22-what-is-the-difference-between-dictionary-iteration-order-and-insertion-order-and-why-does-relying-on-apparent-insertion-order-fail)
23. [Q23. What happens if you use `ImmutableDictionary` but forget that `Add` returns a new instance?](#q23-what-happens-if-you-use-immutabledictionary-but-forget-that-add-returns-a-new-instance)
24. [Q24. Code Review: Caching product lookups in a singleton service using a plain `Dictionary`](#q24-code-review-caching-product-lookups-in-a-singleton-service-using-a-plain-dictionary)
25. [Q25. Code Review: Building a frequency map while modifying the dictionary inside `foreach`](#q25-code-review-building-a-frequency-map-while-modifying-the-dictionary-inside-foreach)
26. [Q26. Code Review: Using a case-sensitive dictionary for HTTP header lookups](#q26-code-review-using-a-case-sensitive-dictionary-for-http-header-lookups)
27. [Q27. Code Review: Enumerating a dictionary by key collection while a background task modifies it](#q27-code-review-enumerating-a-dictionary-by-key-collection-while-a-background-task-modifies-it)
28. [Q28. Scenario: Implementing a multi-level configuration lookup with fallback using `ImmutableDictionary`](#q28-scenario-implementing-a-multi-level-configuration-lookup-with-fallback-using-immutabledictionary)
29. [Q29. Scenario: Building a reverse-lookup index from a product catalog dictionary](#q29-scenario-building-a-reverse-lookup-index-from-a-product-catalog-dictionary)

---
> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary`
> **Source file:** `Program.cs`

---

## Foundation Questions

---

## Q1. What is `Dictionary<TKey, TValue>` and how does it work internally as a hash table?

**Concepts**
- Hash table data structure
- Key hashing and bucket assignment
- Chaining or open-addressing for collision resolution
- O(1) average-case lookup
- Internal arrays: `_buckets` and `_entries`
- Load factor and rehash threshold

**Answer**

`Dictionary<TKey, TValue>` is a generic hash map that stores key-value pairs in an array-backed structure divided into buckets. When you call `Add` or the indexer setter, the runtime calls `key.GetHashCode()`, reduces the result modulo the bucket count to select a slot, and stores the entry there. Because multiple keys can hash to the same bucket, the .NET implementation uses chaining: each bucket holds the index of the first entry in a linked chain stored in a parallel `_entries` array.

Lookup follows the same path — hash the key, find the bucket, walk the chain calling `Equals` until the key matches. When the dictionary's load factor (entries divided by capacity) exceeds a threshold (roughly 0.72 by default), it resizes to the next prime capacity and rehashes all entries into the larger array. Resizing is O(n) but amortized across many insertions so the average cost per insert stays O(1). In .NET 10 the bucket storage uses `int[]` indices into the `_entries` span, making cache-line behaviour more predictable than earlier pointer-chained designs.

---

## Q2. What are the time complexities of the core `Dictionary<TKey, TValue>` operations?

**Concepts**
- O(1) amortized insert and lookup
- O(n) worst-case for degenerate hash functions
- O(n) rehash on capacity growth
- Count property is O(1)
- Keys and Values views are O(1) to obtain, O(n) to enumerate

**Answer**

Under a well-distributed hash function, `Add`, the indexer getter and setter, `Remove`, `ContainsKey`, and `TryGetValue` all run in O(1) amortized time — the hash computation plus one or a small constant number of `Equals` comparisons. In the degenerate case where every key produces the same hash code, all entries fall in one bucket and each operation degenerates to O(n) linear chain walk, which is why custom key types must implement a well-distributed `GetHashCode`. `Count` is a stored integer, so it is always O(1). Obtaining `Keys` or `Values` is O(1) because they are live views backed by the same internal array; iterating either collection is O(n). A capacity resize is O(n) but happens at most O(log n) times across n inserts, so the amortized per-insert cost remains O(1).

---

## Q3. What is the difference between the indexer getter and `TryGetValue` for reading a value?

**Concepts**
- Indexer GET throws `KeyNotFoundException` on missing key
- `TryGetValue` returns a `bool` and sets an `out` parameter
- Single hash lookup in both paths
- Null `out` value when key is absent (nullable annotation)
- Preference for `TryGetValue` in safe access patterns

**Answer**

Both the indexer getter (`dict[key]`) and `TryGetValue` perform exactly one hash lookup, but they differ in how they handle a missing key. The indexer throws `KeyNotFoundException` when the key is not present, which means you must either be certain the key exists or wrap the call in a try-catch — both options add overhead or ceremony. `TryGetValue` instead returns `false` and sets the `out` parameter to `default(TValue)` (which is `null` for reference types under C# nullable analysis), letting you branch on the result without exception handling. The idiomatic pattern is `if (dict.TryGetValue(key, out var value)) { /* use value */ }`. Reserve the indexer for read access only when the key is guaranteed present — for example, iterating over `Keys` and reading paired values, or in unit test assertions where a missing key is a hard failure.

---

## Q4. What is `KeyNotFoundException`, when is it thrown, and how do you avoid it?

**Concepts**
- Thrown by indexer GET on missing key
- Inherits from `SystemException`
- Message includes the dictionary type name but not the missing key value
- Preventive patterns: `TryGetValue`, `ContainsKey` + indexer
- `GetValueOrDefault` extension (System.Collections.Generic)

**Answer**

`KeyNotFoundException` is thrown when you read a value through the indexer — `dict[key]` — and the key is not in the dictionary. It inherits from `SystemException`, meaning it is an unexpected-condition exception rather than a signal for control flow. The exception message identifies the dictionary type but does not echo the missing key value, which makes it harder to diagnose in production logs than a null return would be.

The standard avoidance pattern is `TryGetValue`, which returns `false` instead of throwing when the key is absent. A lighter alternative for value types or when you have a sensible default is `CollectionExtensions.GetValueOrDefault(dict, key, defaultValue)`, which calls `TryGetValue` internally and returns the default when the key is missing. For cases where a missing key genuinely represents a programming error — a configuration entry that should always exist — catching `KeyNotFoundException` and re-throwing with the key value in the message makes debugging faster:

```csharp
if (!config.TryGetValue("ConnectionStrings:Catalog", out string? cs))
    throw new InvalidOperationException(
        "Required config key 'ConnectionStrings:Catalog' is missing.");
```

---

## Q5. What is the difference between `Add` and the indexer setter, and when should you use each?

**Concepts**
- `Add` throws `ArgumentException` on duplicate key
- Indexer SET silently replaces an existing value
- Idempotent writes prefer indexer SET
- Guard-before-add pattern with `ContainsKey`
- `TryAdd` as a non-throwing alternative to `Add`

**Answer**

`Add(key, value)` inserts a new entry and throws `ArgumentException` with message "An item with the same key has already been added" if the key already exists. It is appropriate when duplicate keys are a programming error — for example, building a lookup from data that is contractually unique and a duplicate signals corrupt input. The indexer setter `dict[key] = value` either inserts a new entry or silently replaces the value for an existing key. It is appropriate for upsert semantics — build-a-map-from-a-list loops, cache population, or configuration overrides where the last writer wins. Choose `Add` when a collision should be surfaced as a defect; choose the indexer when you want set semantics. A third option, `TryAdd(key, value)`, returns `false` on a duplicate without throwing — useful when you want to skip duplicates silently without an exception or an explicit `ContainsKey` guard.

---

## Q6. What is `TryAdd` and how does it differ from `Add` and the indexer setter?

**Concepts**
- `TryAdd` returns `false` on duplicate instead of throwing
- Introduced in .NET Core 2.0 alongside `Dictionary`
- Mirrors `ConcurrentDictionary.TryAdd` for a consistent API surface
- One atomic (for single-threaded use) check-and-insert
- Not thread-safe on `Dictionary`; use `ConcurrentDictionary` for concurrent access

**Answer**

`TryAdd(key, value)` was added to `Dictionary<TKey, TValue>` in .NET Core 2.0 to provide a non-throwing "insert if absent" operation. It returns `true` when the entry was inserted and `false` when the key already existed, without throwing. This eliminates the try-catch boilerplate around `Add` in scenarios where a duplicate is expected and not an error — for example, processing an event stream where the same ID can appear more than once but you only want to record the first occurrence.

Compared to `ContainsKey` + `Add`, `TryAdd` is cleaner but performs the same two-step check internally on a non-concurrent `Dictionary`. It is not thread-safe: two threads calling `TryAdd` on a shared `Dictionary` concurrently can both receive `true` for the same key, which corrupts internal state. For concurrent "insert if absent" semantics, use `ConcurrentDictionary.TryAdd` or `GetOrAdd` instead.

```csharp
var seen = new Dictionary<string, int>(StringComparer.Ordinal);
foreach (string sku in skuStream)
{
    seen.TryAdd(sku, 0);   // first occurrence wins; duplicates silently skipped
    seen[sku]++;
}
```

---

## Q7. When should you use `ContainsKey` versus `TryGetValue`?

**Concepts**
- `ContainsKey` performs one hash lookup and returns `bool`
- `TryGetValue` performs one hash lookup and returns `bool` plus the value
- Double-lookup antipattern: `ContainsKey` then indexer
- Use `ContainsKey` for existence checks only
- Use `TryGetValue` when you need the value on success

**Answer**

Both `ContainsKey` and `TryGetValue` perform a single hash lookup internally. The distinction is in what you do with the result. Use `ContainsKey` alone when you only need to know whether a key exists — for example, guarding before calling `Add` (though `TryAdd` is cleaner), or validating input. Use `TryGetValue` when you intend to read the associated value on a hit: calling `ContainsKey` then the indexer performs two hash lookups, which is wasteful and occasionally inconsistent in concurrent scenarios. The pattern `if (dict.ContainsKey(k)) { var v = dict[k]; }` is a common antipattern that signals unfamiliarity with the standard library. The idiomatic replacement is `if (dict.TryGetValue(k, out var v)) { /* use v */ }`. Code reviewers and automated analysers (Roslyn analyser CA1854) flag the double-lookup pattern specifically.

---

## Q8. What rules must a custom reference-type key satisfy to work correctly in `Dictionary`?

**Concepts**
- `GetHashCode` contract: equal objects must return equal hash codes
- `Equals` symmetry, reflexivity, and transitivity
- Hash codes must be stable while the key is in the dictionary
- `record` and `readonly record struct` implement the contract automatically
- `IEqualityComparer<T>` as an alternative to overriding on the type

**Answer**

A type used as a `Dictionary` key must satisfy three rules. First, if `a.Equals(b)` returns `true`, then `a.GetHashCode()` must equal `b.GetHashCode()` — the hash code gates which bucket is probed, so unequal hash codes make equal keys invisible to each other. Second, `Equals` must be an equivalence relation: reflexive (`a.Equals(a)` is true), symmetric, and transitive. Third and most critically, the hash code must not change while the object is a key in the dictionary — if `GetHashCode` depends on a mutable field and that field changes after insertion, the runtime will hash the mutated key to a different bucket and fail to find the entry even though it is still present. In .NET 10 the simplest safe approach is to use `record` or `readonly record struct` — both generate `GetHashCode` and `Equals` over all positional members and, for structs, the immutability is structural. For classes with custom equality semantics, seal the class, mark the key fields `readonly`, and override both methods. Alternatively, supply a custom `IEqualityComparer<T>` to the dictionary constructor to keep the hash contract external to the key type.

---

## Q9. How does the initial capacity hint affect `Dictionary` performance?

**Concepts**
- Default capacity is 0 or a small prime; grows on first insert
- Each resize doubles to the next prime and rehashes all entries
- Capacity hint pre-allocates internal arrays to avoid early resizes
- `EnsureCapacity(n)` method added in .NET 6
- No semantic effect, only a performance hint

**Answer**

When you construct a `Dictionary` without a capacity, the internal arrays start empty and grow on the first insert. Each time the entry count reaches the next resize threshold (roughly 72% of current capacity by default), .NET doubles the internal arrays to the next prime number of buckets and rehashes every existing entry. If you will insert a known number of items, passing that count as the capacity argument — `new Dictionary<string, Product>(1000)` — pre-allocates arrays large enough to hold those entries without any intermediate resizes, eliminating O(n) rehash work. In .NET 6 and later the `EnsureCapacity(int)` method does the same thing post-construction. The capacity is a performance hint only: it never restricts how many entries you can add, and its effect on throughput only shows when inserting thousands of entries in a tight loop. For small dictionaries the default behaviour is fine. For bulk-load scenarios — deserializing a large JSON object, building a lookup from a database result set — setting the capacity upfront is a low-cost, high-value micro-optimisation.

---

## Q10. What equality comparer does `Dictionary` use by default, and how do you supply a custom one?

**Concepts**
- Default: `EqualityComparer<TKey>.Default`
- For `string`: ordinal case-sensitive comparison by default
- `IEqualityComparer<TKey>` injected at construction
- `StringComparer` static members: `Ordinal`, `OrdinalIgnoreCase`, `CurrentCultureIgnoreCase`
- Custom comparer to group keys by domain rule

**Answer**

By default `Dictionary<TKey, TValue>` uses `EqualityComparer<TKey>.Default`, which for `string` keys performs an ordinal case-sensitive comparison using the CLR's default `string.Equals` and `string.GetHashCode`. That means `"SKU-001"` and `"sku-001"` are different keys. To override equality semantics, pass an `IEqualityComparer<TKey>` to the constructor. For `string` keys, `StringComparer` provides ready-made instances: `StringComparer.OrdinalIgnoreCase` for case-insensitive ASCII-safe comparison (highest performance and most predictable), `StringComparer.CurrentCultureIgnoreCase` for locale-sensitive comparison (useful for user-facing display names), and `StringComparer.Ordinal` when you want explicit ordinal semantics stated in code rather than implied by the default. For custom key types you implement `IEqualityComparer<T>` in a separate class, which keeps the key type clean and allows the same type to be used with different comparison strategies in different dictionaries.

```csharp
var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
headers["Content-Type"] = "application/json";
bool found = headers.ContainsKey("content-type"); // true
```

---

## Q11. What is `StringComparer.OrdinalIgnoreCase` and when should you prefer it over `StringComparer.CurrentCultureIgnoreCase`?

**Concepts**
- Ordinal comparison: byte-by-byte Unicode code point comparison
- Culture-insensitive: unaffected by regional locale settings
- `CurrentCultureIgnoreCase`: uses OS locale rules (Turkish `i`/`I` problem)
- Performance: ordinal is faster due to no ICU lookup
- Use `OrdinalIgnoreCase` for identifiers, keys, file names, HTTP headers

**Answer**

`StringComparer.OrdinalIgnoreCase` compares strings byte-by-byte on Unicode code points after uppercasing using invariant rules — it ignores locale and produces identical results regardless of the machine's regional settings. `StringComparer.CurrentCultureIgnoreCase` delegates case folding to the OS ICU library, which applies locale-specific rules: in the Turkish locale, `"i".ToUpper()` produces `"İ"` (capital I with dot), not `"I"`, so a case-insensitive lookup for `"item"` would fail to match `"Item"` on a Turkish server. This is the classic "Turkish I" problem. For dictionary keys that represent technical identifiers — HTTP header names, URL path segments, configuration keys, file names, environment variable names, SKU codes — always use `OrdinalIgnoreCase`. Reserve `CurrentCultureIgnoreCase` (or `InvariantCultureIgnoreCase`) for dictionaries keyed by user-visible display names where cultural sorting rules matter. In .NET 10, `OrdinalIgnoreCase` is also faster because the CLR has intrinsics for ordinal comparison that bypass the ICU layer entirely.

---

## Q12. What is the iteration order of `Dictionary<TKey, TValue>`, and can you rely on it?

**Concepts**
- Insertion order is not guaranteed by the `Dictionary` contract
- Internal bucket layout determines enumeration sequence
- Resizing can change the apparent order
- `SortedDictionary` for ascending key order
- Preserving insertion order requires a wrapper or custom data structure

**Answer**

The `Dictionary<TKey, TValue>` specification explicitly does not guarantee any iteration order. In practice, current .NET implementations tend to enumerate entries in something close to insertion order for small dictionaries that have never been resized, but this is an implementation detail that has changed across runtime versions and can differ between Debug and Release builds, between 32-bit and 64-bit processes, and across runtimes. You must never write code that depends on dictionary enumeration order. If you need keys iterated in sorted order, use `SortedDictionary<TKey, TValue>`, which maintains a balanced binary tree and enumerates keys in ascending comparer order at O(log n) insert/lookup cost. If you need insertion order preserved, maintain a parallel `List<TKey>` and iterate that list to drive dictionary lookups, or use a third-party `OrderedDictionary<TKey, TValue>` implementation. For read-only scenarios where order is fixed at construction, an array of `KeyValuePair<TKey, TValue>` sorted at build time is often the simplest solution.

---

## Q13. What does `foreach` yield when iterating a `Dictionary<TKey, TValue>`, and what is the deconstruction syntax?

**Concepts**
- `IEnumerable<KeyValuePair<TKey, TValue>>` implemented by `Dictionary`
- `KeyValuePair<TKey, TValue>` struct with `.Key` and `.Value` properties
- C# 7 deconstruction via `(key, value)` tuple destructure on `KeyValuePair`
- `foreach (var (k, v) in dict)` syntax
- Keys and Values views as separate IEnumerable sources

**Answer**

`Dictionary<TKey, TValue>` implements `IEnumerable<KeyValuePair<TKey, TValue>>`, so each iteration variable is a `KeyValuePair<TKey, TValue>` struct exposing `.Key` and `.Value`. The classic form is `foreach (KeyValuePair<string, Product> entry in catalog)`. Since C# 7, `KeyValuePair<TKey, TValue>` has a `Deconstruct` extension method that allows the tuple deconstruction shorthand `foreach (var (sku, product) in catalog)`, which is more concise when both sides are needed. If you only need keys or values, iterate `dict.Keys` or `dict.Values` directly — both are `ICollection<T>` views backed by the same internal arrays, so no allocation occurs. Do not add or remove entries from the dictionary while any enumerator over the dictionary, `Keys`, or `Values` is active; doing so invalidates the enumerator and raises `InvalidOperationException` with message "Collection was modified" on the next `MoveNext` call.

---

## Q14. What happens when you pass `null` as a key to `Dictionary<TKey, TValue>`?

**Concepts**
- Reference-type `TKey`: `ArgumentNullException` on all key-accepting members
- Value-type `TKey`: null not possible unless `TKey` is `Nullable<T>`
- Applies to: `Add`, indexer GET, indexer SET, `ContainsKey`, `TryGetValue`, `Remove`
- `ArgumentNullException.ParamName` is `"key"`
- `Hashtable` (non-generic) allows null keys; `Dictionary` does not

**Answer**

When `TKey` is a reference type such as `string`, passing `null` as the key argument to any key-accepting method — `Add`, the indexer getter, the indexer setter, `ContainsKey`, `TryGetValue`, or `Remove` — throws `ArgumentNullException` with `ParamName` set to `"key"`. This is by design: a null key cannot be hashed deterministically, and the dictionary would have no reliable way to store or retrieve such entries. The older non-generic `Hashtable` does allow a null key because it stores it in a special slot, but `Dictionary<TKey, TValue>` does not replicate that behaviour. When `TKey` is a value type such as `int` or `Guid`, null is structurally impossible and the compiler enforces it. When `TKey` is `string?` (nullable reference type annotation), the comparer still throws at runtime on null even though the annotation suggests null is in range — the runtime behaviour is unchanged by nullable reference annotations. Always validate incoming key parameters before dictionary access, especially in API handlers where query strings and route values can legitimately be null.

---

## Q15. What is `ConcurrentDictionary<TKey, TValue>` and what thread-safety guarantees does it provide?

**Concepts**
- Fine-grained lock striping (16 lock objects by default)
- Thread-safe read, write, and update operations
- `TryGetValue`, `TryAdd`, `TryUpdate`, `TryRemove`, `GetOrAdd`, `AddOrUpdate`
- `GetOrAdd` factory may be called more than once under high contention
- Not suitable for iterating-then-mutating without external synchronisation

**Answer**

`ConcurrentDictionary<TKey, TValue>` (in `System.Collections.Concurrent`) partitions the internal key space into 16 stripes (by default), each protected by its own `Monitor` lock. This means reads from different stripes can proceed simultaneously and writes to different stripes do not block each other, giving much higher throughput than a single-lock wrapper around `Dictionary`. Individual operations — `TryGetValue`, `TryAdd`, `TryUpdate`, `TryRemove` — are atomic with respect to the dictionary's internal state. `GetOrAdd(key, factory)` is atomic in the sense that only one value per key will ever be stored, but the factory delegate may be invoked by multiple threads if they race on the same key simultaneously; the dictionary picks one result and discards the rest. If the factory has side effects (database insert, file creation), wrap it in `Lazy<T>` per key to ensure the factory runs exactly once. Enumeration of `ConcurrentDictionary` is a point-in-time snapshot — changes made during iteration are not guaranteed to be visible, and the enumerator does not throw `InvalidOperationException` if the dictionary is modified concurrently.

---

## Q16. What is `ImmutableDictionary<TKey, TValue>` and when should you choose it?

**Concepts**
- Immutable collection from `System.Collections.Immutable`
- `Add`, `Remove`, `SetItem` return a new `ImmutableDictionary` instance
- Structural sharing via HAMT (hash array mapped trie)
- Thread-safe by nature: no mutation, no locks needed
- `ImmutableDictionary.Builder` for bulk construction then freeze
- Higher per-operation cost than mutable `Dictionary`

**Answer**

`ImmutableDictionary<TKey, TValue>` (NuGet `System.Collections.Immutable`, inbox in .NET 5+) is a persistent data structure that never changes after creation. Any method that appears to modify it — `Add`, `Remove`, `SetItem` — returns a new `ImmutableDictionary` instance that structurally shares as many internal nodes as possible with the original using a hash-array mapped trie (HAMT). Because no instance ever mutates, it is inherently thread-safe without locks and can be freely passed across threads, stored in shared fields, or published as a snapshot. The trade-off is higher per-operation cost — O(log₃₂ n) instead of O(1) — and more allocations per write. The idiomatic way to construct a large `ImmutableDictionary` is through `ImmutableDictionary.CreateBuilder<TKey, TValue>()`, which builds into a mutable scratch structure and calls `.ToImmutable()` at the end, paying the allocation cost once. Use `ImmutableDictionary` for configuration snapshots passed to subsystems, domain event state, and read-heavy dictionaries where the snapshot semantics eliminate the need for defensive copies.

```csharp
var builder = ImmutableDictionary.CreateBuilder<string, string>(
    StringComparer.OrdinalIgnoreCase);
builder.Add("host", "localhost");
builder.Add("port", "5432");
ImmutableDictionary<string, string> config = builder.ToImmutable();
```

---

## Q17. How does `Dictionary` handle hash collisions, and what is a hash-flooding attack?

**Concepts**
- Collision chaining via linked entry indices
- All colliding entries stay accessible but lookup degrades to O(n) per bucket
- Hash flooding: adversary sends keys that all hash to the same bucket
- Randomised hash seed (`DOTNET_SYSTEM_RANDOMIZEDSTRINGHASHING`)
- Denial of service via pathological insert workload

**Answer**

When two different keys produce the same bucket index — a collision — `Dictionary` stores both in a linked chain rooted at that bucket. Normal usage with well-distributed keys produces chains of length one or two, keeping lookup close to O(1). If many keys collide into the same bucket, the chain grows and lookup degrades toward O(n) for that bucket. Hash flooding is a deliberate attack where an adversary submits a carefully crafted set of string keys that all hash to the same value under the predictable default seed, forcing the dictionary into O(n²) behaviour for n inserts. .NET mitigates this by randomising the string hash seed per process using `DOTNET_SYSTEM_RANDOMIZEDSTRINGHASHING` (enabled by default since .NET Core 2.1 and enabled in ASP.NET Core HTTP header and form parsing). An attacker who cannot predict the per-process seed cannot engineer a collision-heavy key set. For non-string key types where hash flooding is a concern, supply a custom `IEqualityComparer<TKey>` that introduces additional randomisation, or design key types whose `GetHashCode` incorporates a per-process secret.

---

## Q18. What is `CollectionsMarshal.GetValueRefOrNullRef` and when is it relevant?

**Concepts**
- `System.Runtime.InteropServices.CollectionsMarshal`
- Returns a `ref TValue` directly into the dictionary's internal storage
- Avoids a second lookup when updating a value in place
- Unsafe to use across resizes; must check `Unsafe.IsNullRef` for missing key
- Relevant only in measured hot paths; `TryGetValue` is the default

**Answer**

`CollectionsMarshal.GetValueRefOrNullRef<TKey, TValue>(dict, key)` is a low-level API introduced in .NET 6 that returns a managed reference (`ref TValue`) directly into the dictionary's internal entry array. For value types, this means you can increment a counter or update a struct field without copying the value out, modifying it, and writing it back — eliminating an extra lookup that `TryGetValue` + indexer setter would incur. You check for a missing key with `Unsafe.IsNullRef(ref value)`. The reference is only valid until the next operation that could cause a resize; storing it across `Add` calls is unsafe. This API exists for performance-critical scenarios such as frequency tables built over millions of events or tight inner loops in game engines. In everyday application code, `TryGetValue` and the indexer are correct and readable; reach for `GetValueRefOrNullRef` only after profiling confirms it is a bottleneck.

```csharp
ref int count = ref CollectionsMarshal.GetValueRefOrNullRef(freq, word);
if (Unsafe.IsNullRef(ref count))
    freq[word] = 1;
else
    count++;
```

---

## Gotchas — Dictionary`<TKey,TValue>` (Interview Traps)

---

#### Gotcha 1. `GetHashCode` and `Equals` contract — silent key collision

**Concepts**
- Dictionary uses GetHashCode to bucket, Equals to confirm match
- Overriding Equals without overriding GetHashCode breaks lookups
- Two "equal" objects in different buckets — ContainsKey returns false
- Records auto-generate consistent Equals + GetHashCode

**Answer**

`Dictionary<TKey,TValue>` calls `GetHashCode` to find the bucket and `Equals` to confirm the match within that bucket. If a custom key type overrides `Equals` for value equality but leaves `GetHashCode` as the default object identity hash, two logically equal keys land in different buckets and `ContainsKey` returns `false` — the entry is silently unreachable. The rule: whenever you override `Equals`, you must override `GetHashCode` to return the same value for equal objects. `record` types in C# auto-generate consistent implementations based on their declared properties.

---

#### Gotcha 2. Mutating a key after insertion corrupts the dictionary

**Concepts**
- Keys must be immutable for the lifetime of dictionary membership
- Hash changes after mutation make entry unreachable
- No runtime error thrown — silent data loss
- Use immutable types (string, record, struct) as keys

**Answer**

`Dictionary` computes and stores the hash of a key at insertion time and uses it to place the entry in a bucket. If the key object is mutable and its hash changes after insertion (because a field that participates in `GetHashCode` is modified), the entry now lives in the wrong bucket — lookups compute the new hash, find an empty bucket, and return `false`. No exception is thrown; the entry is silently lost. The fix is to use immutable types as keys — `string`, value types, or C# `record`s with read-only properties.

---

#### Gotcha 3. `TryGetValue` vs. double-lookup with `ContainsKey`

**Concepts**
- `ContainsKey` + indexer = two hash+bucket traversals
- `TryGetValue` does one traversal and returns found + value atomically
- Double-lookup is a TOCTOU race in multi-threaded code
- Always prefer TryGetValue for conditional access

**Answer**

Using `if (dict.ContainsKey(k)) { var v = dict[k]; }` performs two separate hash lookups — one to check existence, one to retrieve the value. `TryGetValue(k, out var v)` does a single lookup and returns both the success flag and the value atomically (within the method call). In single-threaded code the double-lookup is just wasteful; in multithreaded code (without external locking) another thread can remove the key between the two calls, causing `KeyNotFoundException` on the indexer call even though `ContainsKey` returned `true`. Always use `TryGetValue`.

---

#### Gotcha 4. `KeyNotFoundException` from indexer access

**Concepts**
- `dict[key]` throws KeyNotFoundException if key is absent
- `dict.GetValueOrDefault(key)` returns default(TValue) safely
- `TryGetValue` for conditional access patterns
- `dict.TryAdd(key, value)` to avoid duplicate-key ArgumentException

**Answer**

The `Dictionary` indexer `dict[key]` throws `KeyNotFoundException` (not null or default) when the key does not exist — unlike `Array` which returns `null` for reference types. `GetValueOrDefault(key)` returns `default(TValue)` (null for reference types, 0 for int) without throwing. The symmetric trap is `dict[key] = value` for insertion: it silently overwrites an existing entry, which is fine for update-or-insert semantics but wrong when you want to detect duplicate keys — use `dict.TryAdd(key, value)` (returns false on duplicate) or `dict.Add(key, value)` (throws `ArgumentException` on duplicate).

---

#### Gotcha 5. Iteration order is not guaranteed

**Concepts**
- Dictionary iteration order is implementation-defined, not insertion order
- Order may appear consistent in practice but is not contractual
- `SortedDictionary` for sorted key iteration
- `OrderedDictionary` or `List<KeyValuePair>` for insertion-order iteration

**Answer**

`Dictionary<K,V>` does not guarantee any particular enumeration order — the order of `foreach (var kvp in dict)` depends on hash values, bucket layout, and insertion/deletion history. In practice, .NET's implementation often appears to iterate in insertion order for small dictionaries, but this is an implementation artifact, not a contract, and can change across versions or with different key types. Use `SortedDictionary<K,V>` to iterate in key order, or maintain a separate `List<TKey>` to track insertion order explicitly.

---

#### Gotcha 6. Thread safety — `ConcurrentDictionary` vs. locking

**Concepts**
- Dictionary is not thread-safe for concurrent writes
- Concurrent reads are safe only if no write is in progress
- ConcurrentDictionary is thread-safe with fine-grained locks
- GetOrAdd on ConcurrentDictionary is not atomic for factory invocation

**Answer**

`Dictionary<K,V>` is not thread-safe — concurrent writes or a write interleaved with a read can corrupt the internal state and throw `InvalidOperationException` or produce incorrect results. `ConcurrentDictionary<K,V>` provides thread-safe operations, but note that `GetOrAdd(key, factory)` is not fully atomic: the factory delegate may be called multiple times if two threads race on the same key, and only one result is stored. If the factory is expensive or has side effects, protect it with an outer `Lazy<T>` or `GetOrAdd` followed by a `TryUpdate` pattern.

---

#### Gotcha 7. `Values` and `Keys` collections are live views

**Concepts**
- `dict.Keys` and `dict.Values` return live views, not snapshots
- Modifying the dictionary while iterating Keys/Values throws InvalidOperationException
- ToList() or ToArray() to take a snapshot
- Count on the view reflects current dictionary size

**Answer**

`dict.Keys` and `dict.Values` return live view collections that reflect the current state of the dictionary. Iterating them while modifying the dictionary throws `InvalidOperationException` — the same version check as `foreach` on the dictionary itself. To safely iterate while modifying, take a snapshot: `foreach (var key in dict.Keys.ToList())`. Also, `dict.Keys.Count` is always equal to `dict.Count` — there is no separate key count.

---

#### Gotcha 8. `EqualityComparer` must be provided at construction time

**Concepts**
- Default comparer for string keys is ordinal case-sensitive
- Case-insensitive lookup requires StringComparer at construction
- Cannot change the comparer after construction
- Wrong comparer causes lookup misses without exceptions

**Answer**

`Dictionary<string, V>` uses `StringComparer.Ordinal` by default — a case-sensitive, culture-independent byte comparison. If your key population mixes cases ("User" and "user") and you need them to match, you must pass `StringComparer.OrdinalIgnoreCase` at construction: `new Dictionary<string, V>(StringComparer.OrdinalIgnoreCase)`. There is no way to change the comparer after the dictionary is built; the only fix is to rebuild it with the correct comparer. The trap is that both dictionary instances compile and run silently, but one produces lookup misses.

---

#### Gotcha 9. Nested dictionary initialization vs. `AddOrUpdate` pattern

**Concepts**
- Indexer assignment creates or updates the outer key
- Inner `Add` throws on duplicate if inner dictionary already exists
- Common pattern: GetOrAdd the inner dict, then modify it
- CollectionsMarshal.GetValueRefOrAddDefault for zero-copy nested update

**Answer**

When building a `Dictionary<K, Dictionary<K2, V>>`, a common mistake is `outer["a"]["x"] = 1` when the outer key "a" doesn't exist yet — this throws `KeyNotFoundException` on `outer["a"]`. The safe pattern is `outer.TryGetValue("a", out var inner) ? inner : (outer["a"] = new Dictionary<K2, V>())` then mutate `inner`. In C# 8+ with `CollectionsMarshal.GetValueRefOrAddDefault`, you can get a reference to the inner slot and set it in one lookup, avoiding a second hash traversal.

---

#### Gotcha 10. Capacity pre-sizing and default load factor behavior

**Concepts**
- Default initial capacity is small (prime near 0), grows by ~2x factor
- Each resize rehashes all entries — O(n) per resize
- `new Dictionary<K,V>(capacity)` pre-sizes to reduce resizes
- Load factor is fixed at ~72% in current .NET implementation

**Answer**

`Dictionary<K,V>` starts with a small prime-number capacity and rehashes all entries (redistributes across a new, larger bucket array) each time the load factor threshold (approximately 72% full) is reached. For a dictionary expected to hold a million entries, this means roughly 20 resize operations without pre-sizing. Passing `new Dictionary<K, V>(expectedCapacity)` at construction allocates a bucket array large enough to hold that many entries without rehashing, significantly improving insertion throughput for bulk-load scenarios. Over-allocating is rarely harmful since each entry slot is a fixed-size struct.

---

## Real-World Scenarios

---

## Q24. Code Review: Caching product lookups in a singleton service using a plain `Dictionary`

```csharp
// ProductCatalogService.cs — registered as singleton in DI
public class ProductCatalogService
{
    private readonly Dictionary<string, Product> _cache = new();
    private readonly IProductRepository _repo;

    public ProductCatalogService(IProductRepository repo) => _repo = repo;

    public async Task<Product?> GetProductAsync(string sku)
    {
        if (_cache.ContainsKey(sku))
            return _cache[sku];

        Product? product = await _repo.GetBySkuAsync(sku);
        if (product != null)
            _cache[sku] = product;

        return product;
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Thread Safety | Plain `Dictionary` is not thread-safe for concurrent reads and writes | Corrupted internal buckets, `InvalidOperationException`, or torn reads under load |
| Double Lookup | `ContainsKey` + indexer performs two hash lookups | Unnecessary CPU cost on every cache hit |
| Race Condition | Check-then-add is not atomic | Two threads can both miss the cache and both call `_repo.GetBySkuAsync` for the same SKU |
| Memory | No eviction policy; cache grows without bound | OOM kills on pods with high SKU diversity |

**Fix Priority**

1. Replace `Dictionary` with `ConcurrentDictionary<string, Product?>` to eliminate the thread-safety bug.
2. Use `GetOrAdd` with an async-compatible factory or `TryGetValue` + `TryAdd` pattern to make the check-then-insert atomic from the caller's perspective.
3. Consider wrapping with `Lazy<Task<Product?>>` per key to deduplicate concurrent repository calls for the same SKU during cold-cache startup.
4. Replace the raw dictionary with `IMemoryCache` registered as a singleton, configured with `SizeLimit` and `AbsoluteExpirationRelativeToNow` to cap memory and prevent stale data.

```csharp
// .NET 10 — thread-safe, deduplicated cache
public class ProductCatalogService
{
    private readonly ConcurrentDictionary<string, Lazy<Task<Product?>>> _cache = new(
        StringComparer.OrdinalIgnoreCase);
    private readonly IProductRepository _repo;

    public ProductCatalogService(IProductRepository repo) => _repo = repo;

    public Task<Product?> GetProductAsync(string sku) =>
        _cache.GetOrAdd(sku,
            s => new Lazy<Task<Product?>>(() => _repo.GetBySkuAsync(s))).Value;
}
```

---

## Q25. Code Review: Building a frequency map while modifying the dictionary inside `foreach`

```csharp
public Dictionary<string, int> CountWords(IEnumerable<string> words)
{
    var freq = new Dictionary<string, int>();
    foreach (string word in words)
    {
        if (freq.ContainsKey(word))
            freq[word]++;
        else
            freq.Add(word, 1);
    }

    // Remove low-frequency words during a second pass
    foreach (string key in freq.Keys)
    {
        if (freq[key] < 3)
            freq.Remove(key);  // BUG: modifies dict while iterating .Keys
    }

    return freq;
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Collection Modified | `freq.Remove(key)` inside `foreach (freq.Keys)` mutates the dictionary | `InvalidOperationException: Collection was modified` at runtime |
| Double Lookup | `ContainsKey` + indexer in accumulation loop | Two lookups per word; `TryGetValue` or `CollectionsMarshal` is more efficient |
| Readability | Explicit `if/else Add` vs idiomatic upsert pattern | More code than needed; obscures intent |

**Fix Priority**

1. Materialise the keys to remove into a `List<string>` during the second foreach, then iterate the list to call `Remove` after the enumeration ends — never mutate the dictionary inside a `foreach` over itself, `.Keys`, or `.Values`.
2. Replace `ContainsKey` + `freq[word]++` with the idiomatic `freq.TryGetValue` + increment or the `CollectionExtensions.GetValueOrDefault` shorthand for cleaner accumulation.
3. In .NET 10 a concise one-liner using `LINQ GroupBy` or `TryGetValue` with `ref` via `CollectionsMarshal` is available for hot paths.

```csharp
public Dictionary<string, int> CountWords(IEnumerable<string> words)
{
    var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (string word in words)
    {
        freq.TryGetValue(word, out int current);
        freq[word] = current + 1;
    }

    var toRemove = freq.Keys.Where(k => freq[k] < 3).ToList();
    foreach (string key in toRemove)
        freq.Remove(key);

    return freq;
}
```

---

## Q26. Code Review: Using a case-sensitive dictionary for HTTP header lookups

```csharp
public class HttpResponseParser
{
    private readonly Dictionary<string, string> _headers = new();

    public void ParseHeaders(string rawHeaders)
    {
        foreach (string line in rawHeaders.Split('\n'))
        {
            int colon = line.IndexOf(':');
            if (colon < 0) continue;
            string name = line[..colon].Trim();
            string value = line[(colon + 1)..].Trim();
            _headers[name] = value;
        }
    }

    public string? GetContentType() => _headers["Content-Type"];   // BUG 1
    public bool IsJson() =>
        _headers.ContainsKey("content-type") &&                    // BUG 2
        _headers["content-type"].Contains("json");
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Case Sensitivity | Default `Dictionary` uses ordinal case-sensitive comparison | `"Content-Type"` and `"content-type"` treated as different keys; lookup misses |
| KeyNotFoundException | `_headers["Content-Type"]` throws if header absent or stored under different casing | Unhandled exception in response parsing |
| Correctness | RFC 7230 declares HTTP header names case-insensitive | Parser silently fails on servers that lowercase headers (nginx) or capitalise them (IIS) |
| Double Lookup | `ContainsKey` + indexer in `IsJson` | Two hash lookups where one suffices |

**Fix Priority**

1. Construct the dictionary with `StringComparer.OrdinalIgnoreCase` so `"Content-Type"`, `"content-type"`, and `"CONTENT-TYPE"` all map to the same slot — this is the single most important fix.
2. Replace the indexer GET in `GetContentType` with `TryGetValue` and return `null` on miss instead of throwing.
3. Rewrite `IsJson` using `TryGetValue` in one lookup; inline the null-safe `Contains` check.
4. Consider using `HttpRequestHeaders` / `HttpResponseHeaders` from `System.Net.Http` for production HTTP parsing, which already handles case-insensitivity and header semantics.

```csharp
// .NET 10 fix
private readonly Dictionary<string, string> _headers =
    new(StringComparer.OrdinalIgnoreCase);

public string? GetContentType() =>
    _headers.TryGetValue("Content-Type", out string? v) ? v : null;

public bool IsJson() =>
    _headers.TryGetValue("content-type", out string? ct) &&
    ct.Contains("json", StringComparison.OrdinalIgnoreCase);
```

---

## Q27. Code Review: Enumerating a dictionary by key collection while a background task modifies it

```csharp
public class InventoryAdjuster
{
    private readonly Dictionary<string, int> _stock = new();

    public void ApplyAdjustments(IEnumerable<(string Sku, int Delta)> adjustments)
    {
        foreach (var (sku, delta) in adjustments)
            _stock[sku] = _stock.GetValueOrDefault(sku) + delta;
    }

    public IEnumerable<string> GetLowStockSkus(int threshold)
    {
        // Called from a background thread while ApplyAdjustments runs on another
        foreach (string sku in _stock.Keys)        // BUG: no synchronisation
        {
            if (_stock[sku] < threshold)
                yield return sku;
        }
    }
}
```

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Thread Safety | `_stock.Keys` enumerated on one thread while another calls `ApplyAdjustments` | Torn reads, `InvalidOperationException`, or `NullReferenceException` from corrupted state |
| Unsynchronised Concurrent Access | `Dictionary` is not safe for concurrent reads and writes | Data corruption; non-deterministic crash |
| Lazy Enumeration + Mutation | `yield return` keeps the enumerator alive; `ApplyAdjustments` may mutate during iteration | Version check fires at next `MoveNext` |
| Design | Business logic mixed with threading concerns | Hard to test, hard to reason about |

**Fix Priority**

1. Replace `_stock` with `ConcurrentDictionary<string, int>` so concurrent reads and writes are safe without external locking.
2. Materialise the snapshot in `GetLowStockSkus` before yielding: `return _stock.Where(kv => kv.Value < threshold).Select(kv => kv.Key).ToList()` — this resolves the lazy-enumeration-while-mutating hazard.
3. Alternatively, wrap both methods with a `ReaderWriterLockSlim` — upgrade to write lock in `ApplyAdjustments`, read lock in `GetLowStockSkus` — if read throughput matters and you must keep `Dictionary`.
4. Use `Interlocked.Add` or `ConcurrentDictionary.AddOrUpdate` for the delta application to make each SKU update atomic.

```csharp
// .NET 10 fix
private readonly ConcurrentDictionary<string, int> _stock = new(
    StringComparer.OrdinalIgnoreCase);

public void ApplyAdjustments(IEnumerable<(string Sku, int Delta)> adjustments)
{
    foreach (var (sku, delta) in adjustments)
        _stock.AddOrUpdate(sku, delta, (_, current) => current + delta);
}

public IReadOnlyList<string> GetLowStockSkus(int threshold) =>
    _stock
        .Where(kv => kv.Value < threshold)
        .Select(kv => kv.Key)
        .ToList();
```

---

## Q28. Scenario: Implementing a multi-level configuration lookup with fallback using `ImmutableDictionary`

**Concepts**
- Layered configuration: environment-specific overrides general
- `ImmutableDictionary` as an immutable snapshot per layer
- Merge strategy: environment layer shadows base layer
- `StringComparer.OrdinalIgnoreCase` for configuration key matching
- `ImmutableDictionary.Builder` for efficient bulk construction

**Answer**

A common production requirement is a configuration system with multiple layers — a base `appsettings.json`, an environment-specific `appsettings.Production.json`, and environment variable overrides — where later layers shadow earlier ones. `ImmutableDictionary` is a natural fit here because each resolved snapshot is fixed for the lifetime of the application startup; no background thread can accidentally overwrite a configuration key, and you can pass the snapshot safely across DI boundaries without defensive copying.

```csharp
// .NET 10 — Immutable layered config
public static ImmutableDictionary<string, string> MergeConfigs(
    ImmutableDictionary<string, string> baseConfig,
    ImmutableDictionary<string, string> overrides)
{
    // Start from base, apply each override (SetItem replaces or adds)
    var builder = baseConfig.ToBuilder();
    foreach (var (key, value) in overrides)
        builder[key] = value;
    return builder.ToImmutable();
}

// Usage during startup
ImmutableDictionary<string, string> baseConf =
    LoadJson("appsettings.json", StringComparer.OrdinalIgnoreCase);
ImmutableDictionary<string, string> envConf =
    LoadJson($"appsettings.{env}.json", StringComparer.OrdinalIgnoreCase);
ImmutableDictionary<string, string> envVars =
    Environment.GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .ToImmutableDictionary(
            e => (string)e.Key,
            e => (string?)e.Value ?? string.Empty,
            StringComparer.OrdinalIgnoreCase);

ImmutableDictionary<string, string> resolved =
    MergeConfigs(MergeConfigs(baseConf, envConf), envVars);
```

The key design insight is that `ImmutableDictionary.ToBuilder()` creates a mutable scratch copy backed by structural sharing with the original, so `MergeConfigs` only allocates new nodes for the keys that differ. The final `ToImmutable()` freezes the result in O(1). The resolved snapshot is then registered as a singleton and injected as `IReadOnlyDictionary<string, string>` — callers see only the read interface, making accidental mutation impossible even if the underlying type were replaced by a mutable collection.

---

## Q29. Scenario: Building a reverse-lookup index from a product catalog dictionary

**Concepts**
- Inverting a dictionary: value becomes key, key becomes value
- Handling non-unique values (one name → multiple SKUs)
- `Dictionary<string, List<string>>` for one-to-many reverse index
- `GetOrAdd`-style upsert pattern for appending to list values
- `TryGetValue` to check before creating a new list entry

**Answer**

A product catalog maps SKUs to `Product` objects. A common query is "give me all SKUs for products named X" — the reverse direction. Because product names are not guaranteed unique (two products can share a display name), the reverse index needs a `Dictionary<string, List<string>>` mapping name to a list of SKUs.

```csharp
// .NET 10 — build a name-to-SKU reverse index
public static Dictionary<string, List<string>> BuildNameIndex(
    Dictionary<string, Product> catalog,
    IEqualityComparer<string>? nameComparer = null)
{
    nameComparer ??= StringComparer.OrdinalIgnoreCase;
    var index = new Dictionary<string, List<string>>(
        catalog.Count, nameComparer);

    foreach (var (sku, product) in catalog)
    {
        if (!index.TryGetValue(product.Name, out List<string>? skus))
        {
            skus = new List<string>();
            index[product.Name] = skus;
        }
        skus.Add(sku);
    }

    return index;
}

// Query usage
if (index.TryGetValue("Steel bracket", out List<string>? matchingSkus))
    Console.WriteLine($"SKUs: {string.Join(", ", matchingSkus)}");
```

Several implementation details matter. The capacity hint `catalog.Count` gives the index a generous starting allocation, avoiding resizes when names are mostly unique. The `TryGetValue` pattern creates a new list only on the first occurrence of a name and reuses the existing list on subsequent ones — the common "get or create" idiom without `ConcurrentDictionary`. The `nameComparer` parameter allows the caller to control whether `"Steel Bracket"` and `"steel bracket"` share a bucket. In .NET 10 the equivalent using `LINQ.GroupBy` is more concise but creates intermediate groupings and does not reuse an existing comparer-aware dictionary:

```csharp
var index = catalog
    .GroupBy(kv => kv.Value.Name, nameComparer)
    .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList(), nameComparer);
```
