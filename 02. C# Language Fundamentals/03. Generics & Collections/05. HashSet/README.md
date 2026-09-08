# HashSet&lt;T&gt; — Interview Q&A


## Table of Contents

1. [Q1. What is `HashSet<T>` and what problem does it solve that `List<T>` does not?](#q1-what-is-hashsett-and-what-problem-does-it-solve-that-listt-does-not)
2. [Q2. What are the time complexities for `Add`, `Remove`, `Contains`, and `Count` on `HashSet<T>`?](#q2-what-are-the-time-complexities-for-add-remove-contains-and-count-on-hashsett)
3. [Q3. `HashSet<T>.Add` returns `bool`. What does the return value mean and why does it matter?](#q3-hashsettadd-returns-bool-what-does-the-return-value-mean-and-why-does-it-matter)
4. [Q4. Explain the hash table bucket model inside `HashSet<T>`. How does the runtime find an element?](#q4-explain-the-hash-table-bucket-model-inside-hashsett-how-does-the-runtime-find-an-element)
5. [Q5. What is `IEqualityComparer<T>` and how do you pass one to `HashSet<T>`?](#q5-what-is-iequalitycomparert-and-how-do-you-pass-one-to-hashsett)
6. [Q6. What is the difference between the mutating set operations and the LINQ equivalents?](#q6-what-is-the-difference-between-the-mutating-set-operations-and-the-linq-equivalents)
7. [Q7. Explain `IsSubsetOf`, `IsProperSubsetOf`, `IsSupersetOf`, `Overlaps`, and `SetEquals`.](#q7-explain-issubsetof-ispropersubsetof-issupersetof-overlaps-and-setequals)
8. [Q8. How does `HashSet<T>` handle `null` elements?](#q8-how-does-hashsett-handle-null-elements)
9. [Q9. What is `SortedSet<T>` and how does it differ from `HashSet<T>`?](#q9-what-is-sortedsett-and-how-does-it-differ-from-hashsett)
10. [Q10. What is `ImmutableHashSet<T>` and when should it be preferred over `HashSet<T>`?](#q10-what-is-immutablehashsett-and-when-should-it-be-preferred-over-hashsett)
11. [Q11. Is `HashSet<T>` thread-safe? What are the thread-safe alternatives?](#q11-is-hashsett-thread-safe-what-are-the-thread-safe-alternatives)
12. [Q12. Explain how `StringComparer.OrdinalIgnoreCase` integrates with `HashSet<string>`.](#q12-explain-how-stringcomparerordinalignorecase-integrates-with-hashsetstring)
13. [Q13. What is the contract between `GetHashCode` and `Equals`, and what breaks when it is violated?](#q13-what-is-the-contract-between-gethashcode-and-equals-and-what-breaks-when-it-is-violated)
14. [Q14. How do you implement `IEquatable<T>` on an element type and why is it preferred over just overriding `Object.Equals`?](#q14-how-do-you-implement-iequatablet-on-an-element-type-and-why-is-it-preferred-over-just-overriding-objectequals)
15. [Q15. What happens to iteration order when you `foreach` over a `HashSet<T>`?](#q15-what-happens-to-iteration-order-when-you-foreach-over-a-hashsett)
16. [Q16. What is `EnsureCapacity` on `HashSet<T>` and when should you call it?](#q16-what-is-ensurecapacity-on-hashsett-and-when-should-you-call-it)
17. [Q17. How does `HashSet<T>.TryGetValue` work and why does it exist?](#q17-how-does-hashsetttrygetvalue-work-and-why-does-it-exist)
18. [Q18. When should you choose `HashSet<T>` over `Dictionary<TKey, TValue>` with a dummy value?](#q18-when-should-you-choose-hashsett-over-dictionarytkey-tvalue-with-a-dummy-value)
19. [Q19. What happens if you mutate a field used by `GetHashCode` on an element that is already in a `HashSet<T>`?](#q19-what-happens-if-you-mutate-a-field-used-by-gethashcode-on-an-element-that-is-already-in-a-hashsett)
20. [Q20. Why is relying on `HashSet<T>` iteration order a bug waiting to happen?](#q20-why-is-relying-on-hashsett-iteration-order-a-bug-waiting-to-happen)
21. [Q21. What goes wrong when you wrap a LINQ set result in a new `HashSet<T>` without passing the original comparer?](#q21-what-goes-wrong-when-you-wrap-a-linq-set-result-in-a-new-hashsett-without-passing-the-original-comparer)
22. [Q22. Why does adding two distinct objects with identical field values to a default `HashSet<T>` keep both?](#q22-why-does-adding-two-distinct-objects-with-identical-field-values-to-a-default-hashsett-keep-both)
23. [Q23. What goes wrong when `Equals` is not symmetric or not transitive inside `IEqualityComparer<T>`?](#q23-what-goes-wrong-when-equals-is-not-symmetric-or-not-transitive-inside-iequalitycomparert)
24. [Q24. (Code Review) A developer wrote this deduplication helper for user IDs arriving from multiple API pages. Review the code and identify the issues.](#q24-code-review-a-developer-wrote-this-deduplication-helper-for-user-ids-arriving-from-multiple-api-pages-review-the-code-and-identify-the-issues)
25. [Q25. A content platform needs to enforce that every published article has at least one tag from the required taxonomy and none from a banned list. How would you model this with `HashSet<T>` operations?](#q25-a-content-platform-needs-to-enforce-that-every-published-article-has-at-least-one-tag-from-the-required-taxonomy-and-none-from-a-banned-list-how-would-you-model-this-with-hashsett-operations)
26. [Q26. (Code Review) A developer implemented a subscriber deduplication service. Review the code.](#q26-code-review-a-developer-implemented-a-subscriber-deduplication-service-review-the-code)
27. [Q27. How would you use `HashSet<T>` to build an efficient graph-traversal visited-node tracker for a BFS over a large social-network graph?](#q27-how-would-you-use-hashsett-to-build-an-efficient-graph-traversal-visited-node-tracker-for-a-bfs-over-a-large-social-network-graph)
28. [Q28. (Code Review) A permissions system builds a user's effective permission set from role assignments. Review the code.](#q28-code-review-a-permissions-system-builds-a-users-effective-permission-set-from-role-assignments-review-the-code)
29. [Q29. You need a thread-safe "seen items" cache for a high-throughput event processor where events arrive from 16 concurrent partitions. What data structure do you choose and why?](#q29-you-need-a-thread-safe-seen-items-cache-for-a-high-throughput-event-processor-where-events-arrive-from-16-concurrent-partitions-what-data-structure-do-you-choose-and-why)
30. [Q30. How would you design a tag-normalisation pipeline that merges tags from multiple sources, removes blocked tags, and produces a canonical sorted list, using `HashSet<T>` operations throughout?](#q30-how-would-you-design-a-tag-normalisation-pipeline-that-merges-tags-from-multiple-sources-removes-blocked-tags-and-produces-a-canonical-sorted-list-using-hashsett-operations-throughout)

---
## Foundation Questions

---

## Q1. What is `HashSet<T>` and what problem does it solve that `List<T>` does not?

**Concepts**
- Unique-element guarantee enforced on every Add
- O(1) average Contains via hash table vs O(n) linear scan in List
- Set algebra methods: UnionWith, IntersectWith, ExceptWith, SymmetricExceptWith
- No index-based access; membership test is the primary query
- Backed by the same hash-table algorithm as Dictionary (without separate values)

**Answer**

`HashSet<T>` is a generic collection that stores only distinct elements. When you call `Add`, it returns `false` and discards the element if an equal item is already present, so the set never contains duplicates without any external guard code. The critical performance advantage over `List<T>` is membership testing: `Contains` runs in O(1) average time by computing a hash and probing a small bucket chain, whereas `List.Contains` always walks every element. Beyond simple membership, `HashSet<T>` exposes the full vocabulary of set algebra — union, intersection, difference, and symmetric difference — making it the natural fit for problems like merging permission sets, finding shared product tags, or deduplicating import batches. The trade-off is that elements are not ordered and there is no indexer, so any code that needs to read element `i` or iterate in insertion order should still use `List<T>`.

---

## Q2. What are the time complexities for `Add`, `Remove`, `Contains`, and `Count` on `HashSet<T>`?

**Concepts**
- O(1) average for Add, Remove, and Contains under good hash distribution
- O(n) worst case when all elements collide into one bucket
- O(1) for Count (stored field, not computed)
- O(n) for set operations UnionWith/IntersectWith (must visit every element)
- Resize (rehash) is O(n) but amortized across many adds

**Answer**

Under a well-distributed hash function, `Add`, `Remove`, and `Contains` each run in O(1) average time. The runtime computes `GetHashCode`, maps it to a bucket index, and walks a short collision chain before calling `Equals` — with a good hash function that chain is nearly always length one. In the degenerate case where every element hashes to the same bucket the chain becomes O(n), but this is rare in practice and a security concern only when accepting untrusted keys. `Count` is a stored integer updated on each mutation, so reading it is always O(1). Set-algebra methods like `UnionWith` and `IntersectWith` must visit every element in at least one set, so they run in O(n + m) where n and m are the two sizes. The internal array grows via doubling when load exceeds a threshold; each resize is O(n), but the amortized cost per add remains O(1).

---

## Q3. `HashSet<T>.Add` returns `bool`. What does the return value mean and why does it matter?

**Concepts**
- `true` — element was new and was inserted
- `false` — an equal element already existed; set is unchanged
- Enables conditional logic without a prior Contains call
- Avoids double-lookup pattern (Contains then Add)
- Contrast with `List<T>.Add` which is always void (never rejects)

**Answer**

`HashSet<T>.Add(item)` returns `true` when the item is new and was inserted, and `false` when an equal element was already present. This is meaningful because it lets callers detect a first-seen event without paying for a separate `Contains` call first. The double-lookup pattern — `if (!set.Contains(x)) set.Add(x)` — performs two hash lookups; the single `if (set.Add(x))` pattern performs one. A common use-case is deduplication while streaming a sequence: each returned `false` identifies a repeat that should be logged, skipped, or counted without disrupting the main flow. This design also surfaces intent clearly in code review: a void `Add` implies the caller does not care whether the item was already there, while a checked `bool` result signals that the duplicate case is handled deliberately.

---

## Q4. Explain the hash table bucket model inside `HashSet<T>`. How does the runtime find an element?

**Concepts**
- Internal array of bucket slots, sized as a prime number
- Hash code mapped to bucket index via modulo (or bitwise AND post-.NET 6)
- Collision chain: linked entries share a bucket but are not equal
- Equals called only on candidates in the same bucket
- Hash must be stable while element is in the set

**Answer**

Internally `HashSet<T>` maintains two arrays: a `_buckets` array of head-of-chain indices and an `_entries` array of slot records each holding the hash code, a next-entry pointer, and the element value. When `Add` or `Contains` is called, the runtime computes `comparer.GetHashCode(item)`, masks or mods it to a valid index into `_buckets`, then follows the chain of entries in that bucket, calling `comparer.Equals` on each candidate until it finds a match or exhausts the chain. Two unequal items can share a bucket — that is a collision, handled by chaining — but two items that `Equals` considers equal must hash to the same bucket, which is enforced by the contract that `Equals(a, b) == true` implies `GetHashCode(a) == GetHashCode(b)`. Violating this contract means `Contains` may follow the wrong bucket chain and return `false` for an element that is actually present, or `Remove` may fail silently. The element's hash must remain stable (the same value) for as long as the element is stored in the set; mutating a field that feeds `GetHashCode` while the element is held by the set is the most common source of corrupted HashSet state.

---

## Q5. What is `IEqualityComparer<T>` and how do you pass one to `HashSet<T>`?

**Concepts**
- Interface with `Equals(T x, T y)` and `GetHashCode(T obj)` members
- Injected via HashSet constructor overload
- Overrides default equality for all set operations
- Required when T does not override Object.GetHashCode/Equals
- BCL provides `StringComparer.OrdinalIgnoreCase` and similar ready-made comparers

**Answer**

`IEqualityComparer<T>` is an interface with two members: `Equals(T x, T y)` and `GetHashCode(T obj)`. When you construct `HashSet<T>` with an `IEqualityComparer<T>` instance, every hash computation and equality check in the set — including Add, Remove, Contains, and all set-algebra methods — goes through that comparer rather than calling methods on T directly. This is essential for reference types that do not override `Equals` and `GetHashCode`, where the default behaviour is reference identity (two objects with the same business key are still treated as distinct). It is equally essential for case-insensitive string sets: `new HashSet<string>(StringComparer.OrdinalIgnoreCase)` ensures "Linux", "linux", and "LINUX" all map to the same slot. The comparer must satisfy the same contract as the type-level methods: if `Equals` returns `true` for two objects, `GetHashCode` must return the same value for both. The comparer is stored inside the `HashSet` and participates in every operation for the lifetime of the set.

---

## Q6. What is the difference between the mutating set operations and the LINQ equivalents?

**Concepts**
- Mutating: `UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith` — void, modifies the receiver
- LINQ: `Union`, `Intersect`, `Except` on `IEnumerable<T>` — returns a new lazy sequence, no mutation
- LINQ has no `SymmetricExcept`; requires wrapping a copy + `SymmetricExceptWith`
- Mutating methods reuse the existing set's comparer; LINQ uses default equality unless overridden
- Allocation: LINQ returns an iterator that materialises on enumeration; mutating modifies in place

**Answer**

The instance methods `UnionWith`, `IntersectWith`, `ExceptWith`, and `SymmetricExceptWith` modify the `HashSet<T>` they are called on and return `void`. They are intended for scenarios where you are building a working set incrementally and want to avoid allocating a new collection at each step. The LINQ extension methods `Union`, `Intersect`, and `Except` operate on any `IEnumerable<T>`, leave the source sequences untouched, and return a deferred lazy sequence materialised on first enumeration. If you want a new `HashSet<T>` from a LINQ result you must wrap it: `new HashSet<T>(setA.Union(setB), comparer)`. The comparer issue is subtle: a LINQ `Union` uses the element type's default equality unless you pass a comparer explicitly, while the mutating `UnionWith` uses the comparer that was given to the set at construction time. LINQ also has no `SymmetricExcept` method; the only option is to copy the set and call `SymmetricExceptWith` on the copy, or compute it as `(A.Except(B)).Union(B.Except(A))`.

---

## Q7. Explain `IsSubsetOf`, `IsProperSubsetOf`, `IsSupersetOf`, `Overlaps`, and `SetEquals`.

**Concepts**
- `IsSubsetOf(other)` — every element of this set is in other (A ⊆ B)
- `IsProperSubsetOf(other)` — subset AND this set is strictly smaller (A ⊂ B)
- `IsSupersetOf(other)` — every element of other is in this set (A ⊇ B)
- `Overlaps(other)` — at least one element is shared; does not require subset
- `SetEquals(other)` — same elements regardless of order

**Answer**

These five methods answer relational questions about two sets without materialising a new collection. `IsSubsetOf(other)` returns `true` when every element of the receiver appears in `other`; a set is always a subset of itself. `IsProperSubsetOf(other)` adds the strict requirement that the two sets are not equal — the receiver must have fewer elements than `other`. `IsSupersetOf(other)` is the mirror: every element of `other` must be present in the receiver. `Overlaps(other)` returns `true` as soon as one shared element is found, short-circuiting further iteration; it is useful for quick overlap detection such as checking whether any required permission is present in a user's granted set. `SetEquals(other)` returns `true` when both sides contain exactly the same elements regardless of order, which is not the same as `Equals` on the `HashSet` objects themselves (which checks reference identity by default). All five methods accept any `IEnumerable<T>`, not just another `HashSet<T>`, so you can compare against a plain array or list.

---

## Q8. How does `HashSet<T>` handle `null` elements?

**Concepts**
- `null` is a valid element when T is a reference type or nullable value type
- Stored in a dedicated slot (bucket 0 in some implementations)
- `Add(null)` returns `false` on a second attempt; set treats null as equal to null
- Passing a null comparer to the constructor throws `ArgumentNullException`
- Nullable reference type annotations (`#nullable enable`) do not prevent adding null at runtime

**Answer**

For reference types and nullable value types (`T?`), `HashSet<T>` accepts `null` as a valid element. The runtime handles `null` specially because calling `GetHashCode(null)` would throw a `NullReferenceException`; the set instead stores `null` in a reserved slot and considers it equal to any other `null`. This means `Add(null)` returns `true` the first time and `false` on subsequent calls, and `Contains(null)` returns `true` once null has been added. The comparer passed to the constructor must not itself be `null` — that throws `ArgumentNullException` immediately. When T is a non-nullable reference type under `#nullable enable`, the compiler warns on `Add(null)`, but at runtime nothing enforces the constraint, so a suppressed warning can still place null in the set. Code that later calls `Contains(someValue)` will work correctly because the null entry sits in its own bucket and does not interfere with other lookups.

---

## Q9. What is `SortedSet<T>` and how does it differ from `HashSet<T>`?

**Concepts**
- Red-black tree backing instead of hash table
- O(log n) Add, Remove, Contains versus O(1) average in HashSet
- Elements iterated in ascending sort order (comparer-defined)
- `Min`, `Max`, and `GetViewBetween(low, high)` range-view API
- Implements `ISet<T>` — same set-algebra interface as HashSet

**Answer**

`SortedSet<T>` stores unique elements like `HashSet<T>` but uses a self-balancing red-black tree rather than a hash table. Every Add, Remove, and Contains is O(log n) because the tree must be traversed top-to-bottom to find the correct position. In exchange, elements are always iterated in sorted ascending order as defined by the `IComparer<T>` supplied at construction (or `Comparer<T>.Default`). `SortedSet<T>` also offers `Min` and `Max` properties in O(1), and `GetViewBetween(lowerValue, upperValue)` which returns a live range view of all elements between two bounds without copying. Because both types implement `ISet<T>` you can pass a `SortedSet<T>` to the mutating set operations on a `HashSet<T>` and vice versa. The practical rule is: use `HashSet<T>` when the only concern is uniqueness and fast membership, use `SortedSet<T>` when you also need sorted iteration, range queries, or minimum/maximum access.

---

## Q10. What is `ImmutableHashSet<T>` and when should it be preferred over `HashSet<T>`?

**Concepts**
- Lives in `System.Collections.Immutable` (NuGet or built-in since .NET 5)
- Every mutating operation returns a new immutable set; original unchanged
- Structural sharing: unchanged nodes are reused across versions
- Naturally thread-safe for reads; no locks needed for concurrent readers
- `Builder` pattern for batch mutations without intermediate allocations

**Answer**

`ImmutableHashSet<T>`, from the `System.Collections.Immutable` namespace, is a hash set where `Add`, `Remove`, and set-algebra operations return a new `ImmutableHashSet<T>` rather than modifying the receiver. The original instance is unchanged and can be safely shared across threads because its state never mutates after construction. Internally, the implementation uses a hash-array-mapped trie with structural sharing, so creating a modified version by adding one element does not copy the entire collection — only the path from root to the changed leaf is reallocated. The cost is that operations are slower than mutable `HashSet<T>` because of the tree traversal and allocation. When you need many batch mutations, the `ToBuilder()` method returns a mutable `ImmutableHashSet<T>.Builder` that accumulates changes efficiently; calling `ToImmutable()` on the builder produces the final immutable instance. `ImmutableHashSet<T>` is the right choice when a set must be shared across threads or passed to components that must not be able to change it, and when snapshot semantics (each version remains valid independently) are required.

---

## Q11. Is `HashSet<T>` thread-safe? What are the thread-safe alternatives?

**Concepts**
- `HashSet<T>` is not thread-safe; concurrent writes corrupt internal state
- Multiple concurrent reads are safe only if no write is occurring simultaneously
- `ConcurrentDictionary<T, byte>` as a set substitute for concurrent write access
- `ImmutableHashSet<T>` — safe to read concurrently; mutations produce new instances
- `lock` or `ReaderWriterLockSlim` wrapping a regular `HashSet<T>` for fine-grained control

**Answer**

`HashSet<T>` is explicitly not thread-safe. Concurrent calls to `Add` or `Remove` from multiple threads can corrupt the internal bucket and entry arrays, leading to infinite loops during enumeration, missed elements, or exceptions. Multiple threads reading simultaneously are safe only when no thread is writing; even one concurrent writer invalidates any ongoing reads. The standard alternatives depend on the access pattern. If you need concurrent adds and lookups from many threads, `ConcurrentDictionary<TKey, byte>` with a dummy value is the most common workaround: `dict.TryAdd(key, 0)` is the add and `dict.ContainsKey(key)` is the membership test, both fully thread-safe. `ImmutableHashSet<T>` is safe for concurrent reads; write paths use `Interlocked.CompareExchange` to atomically replace the shared reference with the new immutable version, giving lock-free snapshot semantics at the cost of contention under heavy writes. Wrapping a `HashSet<T>` with a `lock` is the simplest solution for low-contention scenarios, and `ReaderWriterLockSlim` is appropriate when reads vastly outnumber writes.

---

## Q12. Explain how `StringComparer.OrdinalIgnoreCase` integrates with `HashSet<string>`.

**Concepts**
- StringComparer implements both `IEqualityComparer<string>` and `IComparer<string>`
- OrdinalIgnoreCase: byte-level comparison after ASCII case fold, no culture influence
- Affects both hash code computation and equality checks inside the set
- `Equals("LINQ", "linq")` returns true; both map to the same bucket
- Must pass the same comparer when wrapping results in a new HashSet to preserve semantics

**Answer**

`StringComparer.OrdinalIgnoreCase` implements `IEqualityComparer<string>` with a hash function that folds ASCII letters to lowercase before hashing, ensuring that "Linux", "linux", and "LINUX" all yield the same hash code and are therefore placed in the same bucket. When `HashSet<string>` is constructed with this comparer, every `Add`, `Contains`, `Remove`, and set-algebra call routes through the comparer's `GetHashCode` and `Equals` methods rather than the default `String.GetHashCode` (which is case-sensitive). The practical consequence is that adding "LINQ" when "linq" is already present returns `false` and leaves the set unchanged. The comparer choice is significant when combining sets: if you construct one `HashSet<string>` with `OrdinalIgnoreCase` and produce a LINQ `Union` with a second `HashSet<string>` that has no comparer, the LINQ result uses default (case-sensitive) equality. To preserve semantics, always wrap the LINQ result explicitly: `new HashSet<string>(setA.Union(setB), StringComparer.OrdinalIgnoreCase)`. The BCL also provides `StringComparer.Ordinal` (case-sensitive, fastest), `InvariantCultureIgnoreCase` (culture-neutral but not byte-level), and culture-specific comparers for locale-aware scenarios.

---

## Q13. What is the contract between `GetHashCode` and `Equals`, and what breaks when it is violated?

**Concepts**
- Rule: if `Equals(a, b)` is `true` then `GetHashCode(a) == GetHashCode(b)` must hold
- Reverse is not required: equal hashes do not imply equal objects (collision is fine)
- Violation causes Contains/Remove to look in the wrong bucket
- Mutable fields fed into GetHashCode invalidate the element's position after mutation
- Symmetric and transitive Equals required; inconsistent Equals corrupts relational checks

**Answer**

The fundamental contract is: if two objects compare equal under `Equals`, they must produce the same value from `GetHashCode`. The reverse — equal hash codes imply equal objects — is not required; two unequal objects sharing a hash code is a normal collision handled by chaining. When this contract is violated, `HashSet<T>` breaks silently. Consider an element whose `GetHashCode` returns 42 when added; the runtime places it in bucket 42 mod n. If `Equals` later considers this element equal to a new item whose `GetHashCode` returns 99, the lookup walks bucket 99 mod n, never finds the stored element, and returns `false` — so `Contains` lies and `Remove` does nothing. The most common real-world violation involves mutable fields: if a property used by `GetHashCode` changes value after the object is added, the object now hashes to a different bucket but physically sits in the old one, making it invisible to all operations until a rehash by accident moves it. Correct implementations use only immutable fields for the hash computation, or apply the `sealed` keyword and initialise the relevant fields only in the constructor.

---

## Q14. How do you implement `IEquatable<T>` on an element type and why is it preferred over just overriding `Object.Equals`?

**Concepts**
- `IEquatable<T>` exposes a strongly typed `Equals(T other)` overload
- Avoids boxing for value types when called from generic contexts
- `HashSet<T>` calls the typed overload through `EqualityComparer<T>.Default`
- Still requires overriding `object.Equals(object)` and `GetHashCode` for completeness
- `sealed` on the class prevents subclass breaks of the Equals/GetHashCode contract

**Answer**

`IEquatable<T>` adds a strongly typed `bool Equals(T? other)` method to a type. When `HashSet<T>` uses `EqualityComparer<T>.Default` to pick an equality strategy, it checks at construction time whether T implements `IEquatable<T>` and, if so, calls the typed overload rather than the `object.Equals(object)` virtual method. For value types this eliminates boxing — calling `object.Equals` on a struct packages it in a heap-allocated `object` reference on every comparison. For reference types the performance benefit is smaller, but the typed signature makes intent clear and avoids an unnecessary cast. The implementation still requires overriding `object.Equals(object? obj)` (usually delegating to the typed overload) and `GetHashCode()` so that the type behaves correctly in non-generic contexts such as `Hashtable` or boxing equality checks. Marking the class `sealed` prevents a subclass from introducing new fields while inheriting an `Equals` that ignores them, which would silently break the contract.

---

## Q15. What happens to iteration order when you `foreach` over a `HashSet<T>`?

**Concepts**
- No ordering guarantee; order is an implementation detail of the bucket layout
- Insertion order is not preserved (unlike `LinkedHashSet` in Java or `Dictionary` in .NET 7+)
- Order can change after a resize (rehash reorders entries)
- For deterministic order use `SortedSet<T>` or `.OrderBy(...)` on enumeration
- Enumerator throws `InvalidOperationException` if the set is modified during iteration

**Answer**

`HashSet<T>` provides no ordering guarantee. When you iterate with `foreach`, the enumerator walks the internal entries array in physical slot order, which reflects bucket assignment and insertion history after any resizes — not the order in which items were added. In practice the order often appears consistent within a single run of the program, which misleads developers into relying on it; a small resize triggered by one more `Add` call silently reorders the entries. If you need sorted output, wrap the enumeration with LINQ `.OrderBy(x => x)` or switch to `SortedSet<T>` which always iterates in comparer-defined ascending order. If you need insertion order, `HashSet<T>` is the wrong tool entirely; a `List<T>` with a separate `HashSet<T>` lookup, or a `LinkedList<T>` with an index dictionary, preserves sequence while still giving O(1) membership tests. The enumerator is also single-use and will throw `InvalidOperationException` if any structural modification — Add, Remove, Clear, or any mutating set operation — occurs while iteration is in progress.

---

## Q16. What is `EnsureCapacity` on `HashSet<T>` and when should you call it?

**Concepts**
- Added in .NET 5; pre-allocates the internal arrays to avoid intermediate resizes
- Resize (rehash) is O(n) and allocates a new array; multiple resizes add GC pressure
- Call before a bulk-load when the final count is approximately known
- Does not shrink the set; use `TrimExcess` to return unused slots to the GC
- Returns the actual capacity after the call (may be rounded up to a prime)

**Answer**

`EnsureCapacity(int capacity)` instructs the `HashSet<T>` to grow its internal arrays now so that at least `capacity` elements can be added without triggering a rehash. Each organic resize doubles the bucket array and rehashes all entries, which is O(n) and creates a burst of GC pressure. When loading a large batch — for example, populating a set from a database result or a file import — and the approximate count is known in advance, calling `EnsureCapacity` before the loop avoids all intermediate resizes. The method returns the actual new capacity, which may be larger than requested because the implementation rounds up to a suitable size. The complement is `TrimExcess()`, which shrinks the internal arrays to the minimum needed for the current `Count`, releasing unused memory when the set has been reduced significantly and will not grow again. On .NET 10 both methods are present on `HashSet<T>`, `Dictionary<K,V>`, and `List<T>`, giving a consistent pattern across the generic collection types.

---

## Q17. How does `HashSet<T>.TryGetValue` work and why does it exist?

**Concepts**
- Added in .NET Core 2.0; signature: `bool TryGetValue(T equalValue, out T actualValue)`
- Returns the stored element that equals the search key, not just a bool
- Enables retrieval of the canonical stored instance when two objects compare equal
- Useful when the comparer ignores some fields; caller may want the first-stored version
- Mirrors the "get or miss" pattern of `Dictionary.TryGetValue`

**Answer**

`TryGetValue(T equalValue, out T actualValue)` searches for an element equal to `equalValue` under the set's comparer and, if found, writes the stored element reference to `actualValue`. This matters when the comparer is partial — it considers two objects equal even if some of their fields differ. For example, a `SubscriberByEmailComparer` equates two `Subscriber` objects that share an email regardless of their `Name` field. After a bulk import you may want to retrieve the original `Subscriber` instance (with its `Name` as first registered) rather than the duplicate that was rejected. Without `TryGetValue` you would have to call `Contains` and then maintain a parallel dictionary to retrieve the stored item. `TryGetValue` closes that gap in a single O(1) hash lookup. It returns `false` and sets `actualValue` to `default(T)` when no equal element exists, making the pattern safe in the same way as `Dictionary<K,V>.TryGetValue`.

---

## Q18. When should you choose `HashSet<T>` over `Dictionary<TKey, TValue>` with a dummy value?

**Concepts**
- `HashSet<T>` expresses intent: the element IS the key, no associated value
- `Dictionary<T, byte>` used as a set pays an extra byte per entry for the dummy value
- `HashSet<T>` provides set-algebra API (UnionWith, IntersectWith, etc.) that Dictionary lacks
- `Dictionary<TKey, TValue>` needed when each unique key maps to a value
- `ConcurrentDictionary<T, byte>` is the correct thread-safe set substitute (no ConcurrentHashSet in BCL)

**Answer**

`HashSet<T>` is the right choice whenever the collection holds unique elements and the element itself is the identity — there is no associated value to look up. Using `Dictionary<T, byte>` with a dummy zero byte as a stand-in set works but wastes memory (one extra byte per slot plus boxing for value-type keys in some scenarios), obscures intent in code review, and forfeits the set-algebra API. A `HashSet<string>` with `UnionWith`, `IntersectWith`, and `SetEquals` is far more readable than the equivalent `Dictionary` manipulations. The exception is thread safety: the BCL provides no `ConcurrentHashSet<T>`, so `ConcurrentDictionary<T, byte>` with `TryAdd` and `ContainsKey` is the accepted pattern for lock-free concurrent set operations. Another exception is when you need to associate metadata with each unique key — "which user last added this tag?" — where the value field earns its place. In summary, start with `HashSet<T>` and migrate to `Dictionary` only when an associated value or concurrent access is genuinely needed.

---

## Gotchas — HashSet`<T>` (Interview Traps)

---

#### Gotcha 1. `Add` returns bool — silently ignores duplicates

**Concepts**
- `HashSet<T>.Add(item)` returns false if the item already exists
- Does not throw on duplicate — silent no-op
- `List<T>.Add` always adds, even duplicates
- Check return value when deduplication tracking matters

**Answer**

Unlike `List<T>.Add` which always appends, `HashSet<T>.Add(item)` is a no-op if the element is already present and returns `false` — it does not throw. Code that adds to a HashSet expecting to detect duplicates must check the return value: `if (!set.Add(item)) { // duplicate }`. This silent behavior is a feature for deduplication pipelines but a trap when you need to know whether insertion actually happened.

---

#### Gotcha 2. Set operations mutate the receiver — use copies when needed

**Concepts**
- `IntersectWith`, `UnionWith`, `ExceptWith` modify the calling set in place
- No "returns-new-set" variant in the BCL
- Copy before calling to preserve the original
- `ImmutableHashSet<T>` returns new sets on set operations

**Answer**

`hashSetA.IntersectWith(hashSetB)` modifies `hashSetA` in place to contain only elements present in both sets — it does not return a new set, and `hashSetA` is permanently changed. To compute the intersection without altering the original, copy first: `var result = new HashSet<T>(hashSetA); result.IntersectWith(hashSetB);`. `ImmutableHashSet<T>` has set-operation methods that return new instances without modifying the original.

---

#### Gotcha 3. Custom `Equals`/`GetHashCode` contract same as Dictionary

**Concepts**
- HashSet uses GetHashCode + Equals for membership tests
- Mutable objects violate the contract if hashed fields change
- Use immutable types or override both methods consistently
- Records provide correct implementations automatically

**Answer**

`HashSet<T>` uses `GetHashCode` to select a bucket and `Equals` to confirm a match — the same contract as `Dictionary` keys. If a reference-type element implements `Equals` by field values but leaves `GetHashCode` as the default identity hash, two logically equal objects land in different buckets and the set allows "duplicates". Mutating an element that is already in the set (changing a field that participates in `GetHashCode`) makes it unreachable via `Contains` but still present in its original bucket — a memory leak and correctness bug.

---

#### Gotcha 4. `Contains` is O(1) average — not O(n) like List

**Concepts**
- HashSet Contains is O(1) average, O(n) worst case with hash collisions
- List.Contains is always O(n) linear scan
- HashSet is the correct type for high-frequency membership checks
- Proper GetHashCode distribution prevents worst-case

**Answer**

`HashSet<T>.Contains(item)` is O(1) average because it computes the hash, jumps to the bucket, and checks a handful of items in that bucket. `List<T>.Contains(item)` is O(n) — it must scan every element. For code that performs many membership tests (e.g., checking whether an ID appears in a previously computed set), storing IDs in a `HashSet<long>` instead of a `List<long>` changes a hot path from O(n) to O(1). Degenerate hash codes that all return the same value degrade HashSet to O(n), so a good `GetHashCode` distribution matters.

---

#### Gotcha 5. `HashSet<T>` does not preserve insertion order

**Concepts**
- Enumeration order is bucket order, not insertion order
- Order appears stable in practice but is not contractual
- `LinkedHashSet` pattern not in BCL — use `List<T>` + `HashSet<T>` combination
- `SortedSet<T>` iterates in sorted order

**Answer**

`HashSet<T>` does not guarantee any enumeration order — elements are iterated in bucket traversal order, which depends on hash values and deletion history. While the order often appears to match insertion order for small sets, this is an implementation artifact. If you need deduplication with preserved insertion order, maintain both a `HashSet<T>` (for O(1) contains checks) and a `List<T>` (for ordered enumeration) and add to both. `SortedSet<T>` iterates in `IComparer<T>` order if sorted enumeration is needed.

---

#### Gotcha 6. `ExceptWith` removes elements present in the argument set

**Concepts**
- A.ExceptWith(B) removes from A every element that is also in B
- The receiver (A) is mutated, not the argument (B)
- `IsSubsetOf`, `IsSupersetOf`, `SetEquals` do not mutate
- Confusing ExceptWith with filtering the argument is a common error

**Answer**

`setA.ExceptWith(setB)` removes from `setA` all elements that are also in `setB` — it computes `setA = setA \ setB` and modifies `setA` in place. A common mistake is expecting it to filter `setB` (remove from B what's in A) — that would be `setB.ExceptWith(setA)`. The query methods (`IsSubsetOf`, `IsSupersetOf`, `Overlaps`, `SetEquals`) are non-mutating read-only checks and return bool, making them safe to call without worrying about side effects.

---

#### Gotcha 7. Null handling depends on the element type

**Concepts**
- Reference types: `null` is a valid HashSet element (one null allowed)
- Value types: null is not a valid element (struct cannot be null)
- `Nullable<T>` where T is value type: null is allowed as a distinct element
- null does not cause exceptions but occupies a bucket

**Answer**

`HashSet<string>` can contain `null` as a valid element — `set.Add(null)` succeeds on first call and returns `false` on subsequent calls (just like any other element). Calling `set.Contains(null)` returns `true` if null was added. `HashSet<int>` cannot contain null because `int` is a non-nullable value type — the compiler rejects `null` as an argument. `HashSet<int?>` (nullable int) can contain null as a distinct element. Unexpected null membership can cause subtle bugs in set operations if nulls were not intended.

---

#### Gotcha 8. EqualityComparer controls both hashing and comparison

**Concepts**
- Pass custom IEqualityComparer<T> to constructor for custom equality
- Default comparer for strings is ordinal case-sensitive
- Case-insensitive string sets need StringComparer.OrdinalIgnoreCase
- Comparer cannot be changed after construction

**Answer**

`HashSet<string>` uses `StringComparer.Ordinal` by default, so "Hello" and "hello" are treated as different elements. For case-insensitive deduplication, construct with `new HashSet<string>(StringComparer.OrdinalIgnoreCase)`. The comparer is baked in at construction and cannot be replaced — to switch comparers you must rebuild the set from scratch. The same rule applies to set operations: `setA.IntersectWith(setB)` uses `setA`'s comparer for all equality checks, so if the two sets were built with different comparers the result may not match intuition.

---

#### Gotcha 9. `TryGetValue` is not on HashSet — use Contains + iteration

**Concepts**
- HashSet has no TryGetValue — it only tracks membership, not key-value pairs
- To retrieve an equal element from the set, enumerate and filter
- .NET 4.7.2+ added TryGetValue(T equalValue, out T actualValue)
- This pattern is used to canonicalize equal objects to a shared reference

**Answer**

`HashSet<T>` does not have a value retrieval method (unlike `Dictionary`) because it stores elements, not key-value pairs. A common need is "I have an object that compares equal to one in the set — give me the one already in the set (the canonical instance)." Starting with .NET 4.7.2, `TryGetValue(T equalValue, out T actualValue)` was added for exactly this use case — it returns the stored element that equals the argument. Before .NET 4.7.2, the workaround was a Dictionary where keys and values were the same object.

---

#### Gotcha 10. `HashSet` vs. `SortedSet` performance trade-off

**Concepts**
- HashSet: O(1) average add/remove/contains, unordered iteration
- SortedSet: O(log n) all operations, always iterates in sorted order
- SortedSet is red-black tree backed — more memory per element
- Choose SortedSet only when sorted enumeration or range queries are needed

**Answer**

`HashSet<T>` is backed by a hash table with O(1) average add, remove, and contains, while `SortedSet<T>` is backed by a red-black tree with O(log n) for all mutation and lookup operations. The performance gap is significant at scale — a `SortedSet` of 1 million elements has lookup about 20 comparisons deep, while `HashSet` is typically 1-2 comparisons. `SortedSet` should only be chosen when you actually need sorted enumeration (`Min`, `Max`, `GetViewBetween`) — for pure deduplication and membership testing, `HashSet` is always faster.

---

## Real-World Scenarios

---

## Q24. (Code Review) A developer wrote this deduplication helper for user IDs arriving from multiple API pages. Review the code and identify the issues.

```csharp
public static List<string> DeduplicateUserIds(IEnumerable<IEnumerable<string>> pages)
{
    List<string> result = new List<string>();
    foreach (IEnumerable<string> page in pages)
    {
        foreach (string id in page)
        {
            if (!result.Contains(id))
                result.Add(id);
        }
    }
    return result;
}
```

**Concepts**
- `List<T>.Contains` is O(n) linear scan; outer loop makes whole method O(n^2)
- Case sensitivity of IDs not addressed; "User123" and "user123" treated as distinct
- Return type `List<string>` unnecessarily exposes mutation; `IReadOnlyList<string>` preferred
- `HashSet<string>` as an accumulator reduces Contains to O(1)
- Missing null-guard on input enumerable

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Performance | `result.Contains(id)` is O(n); combined with two loops the method is O(total\_ids^2) | Unacceptably slow for more than a few thousand IDs across pages |
| Correctness | No case normalisation for IDs; "USER1" and "user1" produce two entries | Duplicate logical users downstream |
| API design | Returns mutable `List<string>`; callers can accidentally modify the deduplicated result | Subtle state-sharing bugs in concurrent consumers |
| Robustness | No null check on the outer or inner enumerables | `NullReferenceException` on a null page from a failed API call |

**Fix priority**

1. Replace `List<string> result` with `HashSet<string> seen` using `StringComparer.OrdinalIgnoreCase`; build a separate `List<string>` only for the ordered output if order matters, or return `IReadOnlyCollection<string>` directly from the set.
2. Add a null guard: `if (pages is null) return Array.Empty<string>();` and `if (page is null) continue;` inside the outer loop.
3. Change the return type to `IReadOnlyList<string>` or `IReadOnlyCollection<string>` to signal that callers should not mutate the result.
4. Add an XML doc comment that states the case-normalisation behaviour so future maintainers do not silently change the comparer.

---

## Q25. A content platform needs to enforce that every published article has at least one tag from the required taxonomy and none from a banned list. How would you model this with `HashSet<T>` operations?

**Concepts**
- `Overlaps(requiredTags)` for fast "at least one required tag present" check
- `ExceptWith(bannedTags)` or `Overlaps(bannedTags)` to detect banned tags
- `IsSubsetOf(approvedTags)` to ensure all tags are from an approved vocabulary
- Separation of validation logic from mutation prevents accidental state change
- `ImmutableHashSet<T>` for taxonomy definitions shared across requests

**Answer**

Define the taxonomy and ban list as `ImmutableHashSet<string>` instances loaded once at startup with `StringComparer.OrdinalIgnoreCase`, ensuring case differences in article tags do not cause false negatives. For validation, take a copy of the article's tag set so validation never mutates the domain object. Use `articleTags.Overlaps(requiredTaxonomy)` to check that at least one approved category tag is present — `Overlaps` short-circuits on the first hit so it is efficient. Use `articleTags.Overlaps(bannedTagSet)` to detect any disallowed tag; if true, collect `articleTags.Intersect(bannedTagSet)` to build the rejection message listing the specific offending tags. Use `articleTags.IsSubsetOf(approvedVocabulary)` if the platform enforces a closed vocabulary — every tag must be pre-approved. Combine these three checks into a `ValidationResult Validate(HashSet<string> articleTags)` method that returns a typed result object listing which rules failed. Keeping the taxonomy sets immutable means validation is thread-safe without locks, and the same `ImmutableHashSet<string>` instances can be used across all in-flight requests.

---

## Q26. (Code Review) A developer implemented a subscriber deduplication service. Review the code.

```csharp
public class SubscriberService
{
    private HashSet<Subscriber> _active = new HashSet<Subscriber>();

    public bool Register(Subscriber s)
    {
        return _active.Add(s);
    }

    public bool IsRegistered(string email)
    {
        return _active.Any(sub => sub.Email.Equals(email,
            StringComparison.OrdinalIgnoreCase));
    }
}
```

**Concepts**
- Default reference equality means two `Subscriber` objects with same email are both kept
- `IsRegistered` uses O(n) LINQ scan instead of O(1) set lookup
- Missing `IEqualityComparer<Subscriber>` on the HashSet construction
- Thread safety not considered for a service-level field
- `Any` with lambda defeats the entire purpose of using a HashSet

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `HashSet<Subscriber>` uses reference equality by default; two `new Subscriber(email)` objects are both accepted | Duplicate logical subscribers silently accumulate |
| Performance | `IsRegistered` calls `_active.Any(...)` — O(n) scan — instead of using the set's O(1) Contains | Defeats the performance reason for choosing HashSet |
| Thread safety | `_active` is a shared mutable field with no synchronisation; `Register` and `IsRegistered` called from multiple threads corrupt the set | Race conditions, possible infinite loop during concurrent enumeration |
| Design | `IsRegistered` takes a `string` but the set stores `Subscriber`; the lookup key type mismatch forces a linear scan | Architectural impedance; consider `HashSet<string>` for emails or `Dictionary<string, Subscriber>` |

**Fix priority**

1. Inject `SubscriberByEmailComparer` (or an equivalent) into the `HashSet<Subscriber>` constructor so `Add` enforces email-based uniqueness.
2. Replace the `IsRegistered(string email)` implementation: either hold a parallel `HashSet<string>` of normalised emails for O(1) lookup, or switch the primary store to `Dictionary<string, Subscriber>` keyed by normalised email.
3. Protect `_active` with a `ReaderWriterLockSlim` (`EnterReadLock` for `IsRegistered`, `EnterWriteLock` for `Register`) or replace the field with `ConcurrentDictionary<string, Subscriber>` for lock-free access.
4. Add `ArgumentNullException.ThrowIfNull(s)` at the top of `Register` to fail fast on a null subscriber rather than allowing a null entry.

---

## Q27. How would you use `HashSet<T>` to build an efficient graph-traversal visited-node tracker for a BFS over a large social-network graph?

**Concepts**
- `HashSet<long>` (or `HashSet<Guid>`) stores visited node IDs in O(1) lookup time
- `Add` returns bool — false means "already visited"; skip enqueue
- Prevents re-enqueuing neighbours in multi-edge graphs
- Pre-sizing with `EnsureCapacity` avoids resize when expected node count is known
- Per-traversal instance (not shared) ensures no cross-request contamination

**Answer**

In BFS over a social graph each node may be reachable via many paths, so a visited check is executed for every edge. With a `List<long>` the visited check is O(n), making the traversal O(E * V) in the worst case — unacceptable for graphs with millions of nodes. Using `HashSet<long>` reduces each check to O(1) average, bringing the overall complexity down to O(V + E). The traversal loop dequeues a node ID, calls `visited.Add(nodeId)` — which returns `false` if already visited so the body is skipped — then enqueues all unvisited neighbours. Because `Add` and `Contains` are both O(1), the visited check adds a constant overhead per edge rather than a linear one. When the graph's approximate size is known from metadata (for example, the database reports 500 000 users in a network component), calling `visited.EnsureCapacity(500_000)` before the loop prevents all intermediate resizes. Each traversal creates its own `HashSet<long>` instance; sharing a static visited set across concurrent traversals would require locks and introduce cross-request contamination. For extremely large graphs that exceed available RAM, a `BloomFilter` or a bitset over a compact node-index space can replace the HashSet, but for in-process traversals up to tens of millions of nodes `HashSet<long>` is the standard tool.

---

## Q28. (Code Review) A permissions system builds a user's effective permission set from role assignments. Review the code.

```csharp
public static bool HasAllPermissions(
    List<string> userRoles,
    Dictionary<string, List<string>> rolePermissions,
    List<string> requiredPermissions)
{
    List<string> effective = new List<string>();
    foreach (string role in userRoles)
    {
        if (rolePermissions.TryGetValue(role, out List<string>? perms))
        {
            foreach (string p in perms)
                if (!effective.Contains(p))
                    effective.Add(p);
        }
    }
    foreach (string req in requiredPermissions)
        if (!effective.Contains(req))
            return false;
    return true;
}
```

**Concepts**
- `List.Contains` is O(n); two nested loops make the effective-set build O(roles * perms)
- Second loop also uses `List.Contains`, adding O(effective * required) cost
- `HashSet<string>.IsSupersetOf(requiredPermissions)` replaces both loops cleanly
- Case sensitivity of permission strings not addressed
- No null guards on inputs

**Issues**

| Category | Problem | Impact |
|---|---|---|
| Performance | `effective.Contains(p)` inside the build loop is O(n) on a growing list; repeated for every permission in every role | Quadratic cost when a user has many roles with overlapping permissions |
| Performance | Second loop also uses `List.Contains` — O(effective * required) — when `IsSupersetOf` would be O(required) | Unnecessary linear scan on each required permission |
| Correctness | Permission string comparison is case-sensitive; "Read" and "read" are treated as different permissions | Authorisation bypass if permission names are inconsistently cased |
| Robustness | No null guards; passing `null` for any argument throws `NullReferenceException` | Unhandled exception at runtime |

**Fix priority**

1. Replace `List<string> effective` with `HashSet<string> effective = new HashSet<string>(StringComparer.OrdinalIgnoreCase)` and replace the inner duplicate-check with a bare `effective.Add(p)` (HashSet rejects duplicates automatically).
2. Replace the second loop with `return effective.IsSupersetOf(requiredPermissions)` — one O(required) pass that leverages the set's O(1) lookups.
3. Add `ArgumentNullException.ThrowIfNull` guards for all three parameters.
4. Consider changing the `rolePermissions` dictionary value type to `IReadOnlyList<string>` and the parameter types to `IEnumerable<string>` to reduce coupling to concrete `List<T>`.

---

## Q29. You need a thread-safe "seen items" cache for a high-throughput event processor where events arrive from 16 concurrent partitions. What data structure do you choose and why?

**Concepts**
- `ConcurrentDictionary<TKey, byte>` as the thread-safe set equivalent
- Lock-free striped-lock internals handle concurrent Add/Contains without a global lock
- `ImmutableHashSet<T>` + `Interlocked.CompareExchange` for snapshot-consistent reads
- Regular `HashSet<T>` + `ReaderWriterLockSlim` for read-heavy, write-light workloads
- Partition-local `HashSet<T>` instances (no sharing) for partitioned event streams

**Answer**

For 16 concurrent writers with frequent reads the best approach depends on the access pattern. If events are already partitioned by key and each partition is processed by exactly one thread, the cleanest solution is 16 separate `HashSet<string>` instances with no sharing — each partition owns its own set, eliminating all contention and synchronisation overhead. When partitions can produce overlapping IDs that must be globally deduplicated, `ConcurrentDictionary<string, byte>` is the standard BCL solution: `dict.TryAdd(eventId, 0)` returns `false` for a duplicate and `true` for a first occurrence, both O(1) under the striped lock. The dummy `byte` value occupies negligible memory. `ImmutableHashSet<T>` with `Interlocked.CompareExchange` works for scenarios where the entire snapshot is frequently read by many consumers while writes are less frequent: the writer builds a new immutable version and atomically swaps the reference, giving readers a consistent view without blocking. `ReaderWriterLockSlim` wrapping a plain `HashSet<string>` is appropriate when read throughput far exceeds write throughput — multiple concurrent reads share the read lock while writes take an exclusive write lock. On .NET 10, profile under load before choosing; the partition-local design often wins because it removes all lock overhead at the cost of a fan-in merge step at the end.

---

## Q30. How would you design a tag-normalisation pipeline that merges tags from multiple sources, removes blocked tags, and produces a canonical sorted list, using `HashSet<T>` operations throughout?

**Concepts**
- `HashSet<string>` with `OrdinalIgnoreCase` comparer as accumulation set
- `UnionWith` to merge each source batch into the accumulator
- `ExceptWith(blockedTags)` to strip blocked tags in one pass
- `SortedSet<string>` for the final sorted output without a separate sort step
- Immutable blocklist (`ImmutableHashSet<string>`) loaded once from config

**Answer**

Load the blocked-tags vocabulary at startup as `ImmutableHashSet<string> blocked = config.BlockedTags.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase)`. For each pipeline run, initialise an accumulator `HashSet<string> merged = new HashSet<string>(StringComparer.OrdinalIgnoreCase)` and call `merged.UnionWith(source)` for each incoming tag batch — the comparer ensures cross-source case variants like "DotNet" and "dotnet" collapse to one canonical entry. After all sources have been merged, call `merged.ExceptWith(blocked)` to remove every blocked tag in a single O(merged + blocked) pass. Finally, wrap the result in a `SortedSet<string>` with the same comparer to produce a sorted canonical list without a separate `.OrderBy` allocation: `new SortedSet<string>(merged, StringComparer.OrdinalIgnoreCase)`. Expose the output as `IReadOnlyCollection<string>` so downstream stages cannot mutate the result. This design keeps the hot path — iterating sources and merging — at O(total tags) with no per-tag nested loops, and the sort is implicit in the `SortedSet<T>` constructor's O(n log n) tree-build rather than a post-processing step.
