# 03. Generics & Collections — Cross-Topic Interview Q&A
> Back to [C# Language Fundamentals](../README.md)

## Subfolders

| # | Topic | Questions |
|---|-------|-----------|
| 01 | [Generics](01.%20Generics/INTERVIEW_QA.md) | Generic constraints, covariance/contravariance, CLR reification |
| 02 | [ArrayList](02.%20ArrayList/INTERVIEW_QA.md) | Legacy non-generic collections, boxing, migration |
| 03 | [List](03.%20List/INTERVIEW_QA.md) | List\<T\> internals, growth, sort stability, AsReadOnly |
| 04 | [Dictionary](04.%20Dictionary/INTERVIEW_QA.md) | Hashing, collisions, TryGetValue, key immutability |
| 05 | [HashSet](05.%20HashSet/INTERVIEW_QA.md) | Set algebra, deduplication, custom equality |
| 06 | [Queue and Stack](06.%20Queue%20and%20Stack/INTERVIEW_QA.md) | FIFO/LIFO, BFS, workflow patterns |
| 07 | [SortedList & SortedDictionary](07.%20SortedList%20%26%20SortedDictionary/INTERVIEW_QA.md) | Array-backed vs tree-backed sorted maps |
| 08 | [IEnumerable & IEnumerator](08.%20IEnumerable%20%26%20IEnumerator/INTERVIEW_QA.md) | Deferred execution, yield, async streams |

---

## Table of Contents

- [CQ1. How do generics, IEnumerable\<T\>, and the collection types relate at the type-system level?](#cq1-how-do-generics-ienumerablet-and-the-collection-types-relate-at-the-type-system-level)
- [CQ2. Choosing the right collection: decision framework spanning all eight types.](#cq2-choosing-the-right-collection-decision-framework-spanning-all-eight-types)
- [CQ3. How does the GetHashCode / Equals contract affect Dictionary, HashSet, and SortedDictionary differently?](#cq3-how-does-the-gethashcode--equals-contract-affect-dictionary-hashset-and-sorteddictionary-differently)
- [CQ4. Covariance across IEnumerable\<out T\>, IReadOnlyList\<out T\>, and generic class invariance.](#cq4-covariance-across-ienumerableout-t-ireadonlylistout-t-and-generic-class-invariance)
- [CQ5. Performance profile of List\<T\>, Dictionary\<K,V\>, HashSet\<T\>, and SortedDictionary\<K,V\> for insert, lookup, and iteration.](#cq5-performance-profile-of-listt-dictionarykv-hashsett-and-sorteddictionarykv-for-insert-lookup-and-iteration)

---

## CQ1. How do generics, `IEnumerable<T>`, and the collection types relate at the type-system level?

**Concepts**
- generics as the foundation layer
- IEnumerable\<out T\> as the universal read contract
- ICollection\<T\> adding Count/Add/Remove
- IList\<T\> adding positional indexer
- concrete types closing the open generic interfaces

**Answer**

Generics are the language and CLR mechanism that makes the entire collections hierarchy type-safe without boxing. Every concrete collection (`List<T>`, `Dictionary<TKey,TValue>`, `HashSet<T>`) is a closed construction of a generic type definition, which means the CLR JIT-compiles optimized code per value-type substitution and the compiler enforces element-type correctness at the call site. `IEnumerable<out T>` sits at the root of the read-side hierarchy as the covariant "I can give you Ts" contract — `foreach` only needs this interface, and the `out` annotation lets `IEnumerable<string>` flow where `IEnumerable<object>` is expected. `ICollection<T>` extends it with `Count`, `Add`, and `Remove`, and `IList<T>` further adds the positional indexer. Concrete types like `List<T>` implement `IList<T>`, `HashSet<T>` implements `ISet<T>` (which extends `ICollection<T>`), and `Dictionary<K,V>` implements `IDictionary<K,V>`. Accepting an interface rather than a concrete type in method signatures is what allows these implementations to be substituted — a method that accepts `IEnumerable<T>` works with any collection including lazy `IAsyncEnumerable`-backed sequences.

---

## CQ2. Choosing the right collection: decision framework spanning all eight types

**Concepts**
- access pattern as the primary driver
- membership test → HashSet
- key-value lookup → Dictionary
- ordered key iteration → SortedDictionary / SortedList
- positional or sequential → List
- FIFO / LIFO → Queue / Stack
- avoid ArrayList in new code

**Answer**

Start with the dominant operation. If you need to test whether an element exists or deduplicate a stream, `HashSet<T>` gives O(1) average. If you need to look up a value by an arbitrary key, `Dictionary<TKey,TValue>` gives O(1) average and `SortedDictionary` gives O(log n) with guaranteed key ordering for iteration or range queries; prefer `SortedList` over `SortedDictionary` when the collection is built once and read many times because its array-backed storage provides better cache locality and lower memory overhead. If you need elements in insertion order with positional access, `List<T>` is the default — its contiguous array gives O(1) indexed reads and amortized O(1) appends, at the cost of O(n) arbitrary insertions. If ordering constrains access direction, `Queue<T>` enforces FIFO and `Stack<T>` enforces LIFO with O(1) enqueue/dequeue or push/pop. `LinkedList<T>` is only preferable when you frequently insert or remove at known interior positions. Never use `ArrayList` or other non-generic collections in new code — `List<T>` replaces them entirely with no trade-off. Expose the narrowest interface (`IEnumerable<T>`, `IReadOnlyList<T>`, `IReadOnlyDictionary<K,V>`) in return types to keep callers decoupled from the concrete implementation.

---

## CQ3. How does the `GetHashCode` / `Equals` contract affect `Dictionary`, `HashSet`, and `SortedDictionary` differently?

**Concepts**
- hash-based types depend on GetHashCode + Equals
- SortedDictionary depends on IComparer\<T\> not GetHashCode
- contract violation: silent lookup failure in hash types
- contract violation in sorted type: wrong ordering / missing elements
- records as safest custom key type

**Answer**

`Dictionary<K,V>` and `HashSet<T>` are hash-based: they use `GetHashCode` to select a bucket and `Equals` to confirm a match within the bucket. Violating the contract — overriding `Equals` by field values but leaving `GetHashCode` as the reference-based default — causes two logically equal objects to land in different buckets, so `ContainsKey` returns false and entries become unreachable without throwing. `SortedDictionary<K,V>` and `SortedSet<T>` are tree-based and use `IComparer<T>` for both ordering and equality (`CompareTo` returning 0 means equal); they never call `GetHashCode`, so the hash/equals contract is irrelevant to their correctness. However, if `IComparer<T>` is inconsistent with `Equals` — two objects that `Equals` considers the same but `CompareTo` considers different — `SortedDictionary` treats them as distinct keys, leading to phantom duplicates. The safest approach is `readonly record struct` or `record` for custom key types: records auto-generate consistent `Equals`, `GetHashCode`, and `IComparable<T>` implementations based on their primary constructor parameters.

---

## CQ4. Covariance across `IEnumerable<out T>`, `IReadOnlyList<out T>`, and generic class invariance

**Concepts**
- out T covariance on read-only interfaces
- List\<T\> invariance (both in and out positions)
- widening assignment through covariant interface
- IReadOnlyList\<out T\> for indexed covariant access
- practical API design implication

**Answer**

Covariance is declared with `out T` on an interface or delegate and restricts the type parameter to output (read) positions only. `IEnumerable<out T>` is covariant, so `IEnumerable<string>` is assignable to `IEnumerable<object>` — every string is safely read as object. `IReadOnlyList<out T>` is also covariant, extending the same guarantee to indexed read access: `IReadOnlyList<string>` is assignable to `IReadOnlyList<object>`. `List<T>` is invariant because `T` appears in both `Add(T item)` (input) and `T this[int i]` (output); allowing covariant assignment would let a `List<object>` reference call `Add(42)` on a `List<string>` backing store, corrupting it. The practical design rule: return `IReadOnlyList<T>` instead of `List<T>` from service methods — this gives callers indexed access and covariant assignability, while preventing mutation through the returned reference and hiding the concrete implementation.

---

## CQ5. Performance profile of `List<T>`, `Dictionary<K,V>`, `HashSet<T>`, and `SortedDictionary<K,V>` for insert, lookup, and iteration

**Concepts**
- List\<T\>: O(1) amortized append, O(n) lookup by value, O(1) indexed access
- Dictionary: O(1) average insert/lookup, O(n) iteration unordered
- HashSet: O(1) average insert/membership, O(n) iteration unordered
- SortedDictionary: O(log n) insert/lookup, O(n) iteration in key order
- cache locality: List > SortedDictionary > Dictionary/HashSet

**Answer**

`List<T>` gives O(1) amortized appends via doubling growth, O(1) indexed reads with excellent cache locality because elements are contiguous, and O(n) linear `Contains` or `Remove(value)` by scanning. `Dictionary<K,V>` and `HashSet<T>` give O(1) average insert, lookup, and remove, but iteration order is unspecified and the scattered bucket layout means cache misses per element during enumeration. `SortedDictionary<K,V>` is red-black tree backed with O(log n) insert and lookup; iteration is in key order but node-per-element heap objects hurt cache performance more than an unsorted dictionary. `SortedList<K,V>` is array-backed and has better iteration cache locality than `SortedDictionary` but O(n) worst-case insert for out-of-order keys because elements must shift. For throughput-critical numerical iteration `List<T>` or arrays win decisively over all map types because contiguous memory enables SIMD auto-vectorization and hardware prefetch. For lookups by identifier key across more than a few hundred items, `Dictionary` is the right default; only reach for `SortedDictionary` or `SortedList` when sorted-order enumeration or range queries are required.
