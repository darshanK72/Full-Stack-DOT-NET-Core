# C# ArrayList & Non-Generic Collections — Interview Q&A

> Topics: ArrayList, Hashtable, SortedList (non-generic), Stack (non-generic), Queue (non-generic), boxing/unboxing costs, type safety issues, migration to generic equivalents, IList, ICollection, IEnumerable (non-generic)

---

## Foundation Questions

---

## Q1. What is ArrayList and how does it differ fundamentally from List\<T\>?

**Concepts**
- `System.Collections` namespace (pre-generics)
- Object array as backing store
- Compile-time type erasure
- Runtime cast requirement
- Generic type parameter T in `List<T>`

**Answer**

`ArrayList` lives in `System.Collections`, the namespace shipped before C# 2 introduced generics in 2005. Internally it wraps a resizable `object[]` array, which means every element — regardless of its actual type — is stored as a reference to `System.Object`. Because of this, any value can be added to an `ArrayList` without restriction, but every read requires an explicit cast such as `(Product)list[0]`. The compiler accepts any object on `Add`, so type errors are invisible until the wrong cast throws an `InvalidCastException` at runtime.

`List<T>`, by contrast, parameterises the element type at the declaration site. The compiler binds `T` to a concrete type such as `Product` or `int`, enforces type safety on every `Add` and `Insert` call, and returns `T` directly from the indexer — no cast required. For value types such as `int`, `List<T>` also avoids boxing because the JIT generates a version of the class that stores the values directly in an `int[]` rather than wrapping each one in a heap object. The practical upshot is that `List<T>` is faster, safer, and more readable for all homogeneous collections, while `ArrayList` belongs exclusively to legacy or interop scenarios.

---

## Q2. What are boxing and unboxing, and why do they matter specifically for ArrayList?

**Concepts**
- Value type storage on the stack
- Boxing: allocation of a heap wrapper around a value
- Unboxing: extracting a value from its heap wrapper
- Garbage collector pressure
- ArrayList's `object[]` forcing boxing of every value type

**Answer**

Boxing is the implicit conversion that occurs when a value type — such as `int`, `double`, `bool`, or any `struct` — is assigned to a variable of type `object`. The runtime allocates a small heap object, copies the value into it, and returns a reference. Unboxing is the reverse: an explicit cast extracts the original value from the heap wrapper and copies it back to a local variable. Both operations carry measurable overhead: boxing causes a heap allocation (typically 16–24 bytes on a 64-bit runtime) and eventual garbage collection, while unboxing requires a runtime type check followed by a copy.

Every value type stored in an `ArrayList` is boxed on entry and unboxed on retrieval. In a hot path — for example, accumulating thousands of integer sensor readings into an `ArrayList` — each `Add(42)` allocates a new boxed `int` on the managed heap, and each `(int)list[i]` unboxes it. With large collections this causes significant GC pressure and degrades throughput noticeably compared with `List<int>`, which stores integers directly in an `int[]` without any boxing. Reference types such as `string` or class instances are not boxed when stored in `ArrayList` because the list holds their existing heap reference; however, every read still returns `object`, so a cast is needed even though no boxing occurred. The asymmetry is a frequent interview trap: boxing affects only value types, but the cast-on-read requirement affects all types.

---

## Q3. What is the difference between Count and Capacity in ArrayList, and how does the internal array grow?

**Concepts**
- `Count`: logical element count
- `Capacity`: physical backing-array length
- Doubling growth strategy
- `TrimToSize()` for memory reclamation
- Pre-allocation via constructor overload

**Answer**

`Count` reports how many elements are logically present in the `ArrayList` — the number the caller has added and not yet removed. `Capacity` reports the current length of the internal `object[]` array, which is always greater than or equal to `Count`. When the two values are equal and a new element is added, the runtime cannot fit it without reallocating: it creates a new array roughly double the current capacity, copies all existing elements into it, and discards the old array. The doubling strategy keeps the amortised cost of `Add` at O(1) across a sequence of appends, identical to `List<T>`'s behaviour, because even though individual reallocations are O(n), they occur exponentially less frequently as the list grows.

Developers can avoid reallocation entirely by passing an initial capacity hint to the constructor — `new ArrayList(1000)` — or by setting the `Capacity` property explicitly before a bulk `Add` loop. After a batch operation that leaves many unused slots, `TrimToSize()` shrinks `Capacity` down to match `Count`, releasing the surplus memory back to the GC. One subtlety visible in `CapacityDemo` in the project's `Program.cs` is that setting `Capacity` directly to a value lower than `Count` throws an `ArgumentOutOfRangeException`, so `TrimToSize` is the safe way to compact the backing array after the list stabilises.

---

## Q4. What interfaces does ArrayList implement, and what does each one provide?

**Concepts**
- `IList` interface: positional access and mutation
- `ICollection` interface: count, copy, sync primitives
- `IEnumerable` interface: forward iteration via enumerator
- `ICloneable` for shallow copy
- Programming to interfaces for legacy API compatibility

**Answer**

`ArrayList` implements four non-generic interfaces from `System.Collections`. `IList` is the richest: it adds `Add`, `Insert`, `Remove`, `RemoveAt`, `Contains`, `IndexOf`, `Clear`, and the positional indexer `this[int]`. `ICollection` is a subset that exposes `Count`, `CopyTo(Array, int)`, `IsSynchronized`, and `SyncRoot` — the synchronisation primitives are relevant for the thread-safety story covered separately. `IEnumerable` provides `GetEnumerator()`, which powers `foreach`; the returned `IEnumerator` exposes `MoveNext()` and `Current` (type `object`). `ICloneable` offers `Clone()`, which returns a shallow copy of the list with the same element references.

Programming against `IList` rather than the concrete `ArrayList` type is the correct approach when consuming pre-generics APIs: your own methods can accept `IList` and work with `ArrayList`, `SortedList`, or any other `IList` implementation without a hard dependency on the concrete class. In practice this pattern appears in COM interop wrappers and older framework callbacks that still return `IList` or `ICollection`. When you receive such an interface and know the actual runtime type is `ArrayList`, you can safely downcast. When you know only that it is some `IList`, iterate with `foreach` treating each element as `object` and narrow carefully.

---

## Q5. How does the non-generic IEnumerable and IEnumerator work during a foreach loop over ArrayList?

**Concepts**
- `IEnumerable.GetEnumerator()` returning `IEnumerator`
- `IEnumerator.MoveNext()` and `IEnumerator.Current`
- `Current` typed as `object`
- Compiler expansion of `foreach`
- Modification-during-iteration exception

**Answer**

When the C# compiler encounters `foreach (object item in arrayList)`, it expands the loop into a `GetEnumerator / MoveNext / Current` pattern using the non-generic `IEnumerable` interface. Concretely, it calls `IEnumerator enumerator = list.GetEnumerator()`, then enters a `while (enumerator.MoveNext())` loop where `enumerator.Current` provides the current element typed as `object`. Because `Current` returns `object`, every element retrieved in the loop body is an `object` reference — even if all the stored values happen to be `int`, the loop variable receives a boxed copy each iteration.

This behaviour has two important consequences. First, iterating a large `ArrayList` of value types in a `foreach` unboxes each element on access, incurring GC activity proportional to collection size. Second, modifying the `ArrayList` — adding, removing, or replacing elements — while an enumerator is active causes the enumerator's next `MoveNext()` call to throw `InvalidOperationException: Collection was modified`. This is the same invalidation logic as `List<T>` and is intentional: the internal version counter is incremented on any structural change, and the enumerator checks it on every step. If in-place modification is required during iteration, iterate a copy or collect indices to process after the loop.

---

## Q6. What type-safety problems arise from ArrayList's indexer returning object?

**Concepts**
- Indexer return type `object`
- Compile-time silencing of type errors
- Runtime `InvalidCastException`
- CS0266 implicit conversion error
- `as` operator returning null vs hard cast throwing

**Answer**

The `ArrayList` indexer is declared `object this[int index]`, which means the compiler has no information about what type of object is stored at any given position. When you write `int n = list[0]`, the compiler rejects it immediately with CS0266 because it cannot implicitly convert `object` to `int`. So far, so good. But when you write `(int)list[0]`, the compiler accepts the explicit cast unconditionally, even if `list[0]` actually holds a `string` — that error surfaces only as an `InvalidCastException` at runtime, potentially in production and potentially far from the `Add` call that stored the wrong type.

The problem is amplified when the list is populated across different code paths. If one call path adds integers and another mistakenly adds strings, the compilation succeeds entirely; the cast-failure manifests only on the code path that reads back the string slot while expecting an integer. Contrast this with `List<int>`, where `list.Add("hello")` is rejected at compile time with CS1503. The `as` operator is safer than a hard cast — `list[0] as string` returns `null` rather than throwing when the object is not a `string` — but it only works with reference types; you cannot write `list[0] as int`. The correct defensive pattern is to check with `is` before casting, or to migrate to the strongly typed generic equivalent.

---

## Q7. When, if ever, is ArrayList still appropriate in modern .NET 10?

**Concepts**
- Legacy codebase maintenance
- COM interop returning non-generic collections
- Reflection APIs returning `object[]`
- Intentionally heterogeneous collections
- Obsolescence without removal

**Answer**

`ArrayList` has not been removed from .NET 10, but there are essentially no scenarios where it is the preferred choice for new code. The .NET team considers it a legacy type retained purely for backward compatibility, and the official guidance documented in `dotnet/runtime` is to use `List<T>` for any homogeneous collection and a typed model class or tuple for any heterogeneous one.

The realistic situations where `ArrayList` still appears are: first, maintaining or extending existing .NET Framework code that predates generics and where a full migration is out of scope for the current task; second, consuming COM interop interfaces or P/Invoke layers that return `System.Collections.IList` or concrete `ArrayList` references because the underlying COM object predates generics; and third, certain reflection and late-bound scenarios where the element type is not known at compile time and the caller must treat everything as `object` anyway. In all three situations the correct long-term response is wrapping the non-generic API behind a typed adapter so that consumers deal only with strongly typed collections. For new .NET 10 code — including greenfield services, domain models, and algorithm implementations — `List<T>`, `Dictionary<TKey,TValue>`, and the other generic collections in `System.Collections.Generic` are unconditionally the right choice.

---

## Q8. How does non-generic Hashtable differ from Dictionary\<TKey,TValue\>, and what are the migration considerations?

**Concepts**
- `Hashtable` storing `object` keys and values
- `Dictionary<TKey,TValue>` type-parameterised lookup
- Boxing of value-type keys in `Hashtable`
- Key collision and `GetHashCode` / `Equals` contract
- `ContainsKey` vs dual lookup pattern

**Answer**

`Hashtable`, like `ArrayList`, lives in `System.Collections` and stores both keys and values as `object`. Every lookup boxes any value-type key before hashing it, and every retrieved value returns as `object` requiring a cast. The type contract is entirely unenforced: `skuToBin.Add("WH-100", "Aisle 3-B")` compiles equally well as `skuToBin.Add(42, new DateTime())` — the compiler has no opinion on either.

`Dictionary<TKey,TValue>` parameterises both the key and the value type. It uses a generic `IEqualityComparer<TKey>` internally rather than the virtual `object.GetHashCode()` / `object.Equals()` dispatch that `Hashtable` relies on, which makes equality comparison faster for value types because it avoids boxing and virtual dispatch. The API is also safer: `dict["WH-100"]` returns `string` directly without a cast, and `dict.TryGetValue(key, out string? bin)` eliminates the double-lookup anti-pattern (calling `ContainsKey` then the indexer) that is common in legacy `Hashtable` code.

When migrating `Hashtable` to `Dictionary<TKey,TValue>`, the main considerations are: the indexer on `Hashtable` returns `null` for a missing key, while `Dictionary` throws `KeyNotFoundException`; `Hashtable`'s `ContainsKey` should be replaced with `TryGetValue` for safe lookup; and if the original code relied on the `Hashtable` accepting `null` keys (it does), note that `Dictionary<TKey,TValue>` forbids `null` keys for non-nullable reference-type keys by default.

---

## Q9. How do non-generic Stack and Queue differ from Stack\<T\> and Queue\<T\>?

**Concepts**
- LIFO (Stack) and FIFO (Queue) semantics
- `Push/Pop/Peek` vs `Enqueue/Dequeue/Peek`
- `object` element type in non-generic versions
- Boxing of value types on Push/Enqueue
- Generic versions eliminating casts

**Answer**

Non-generic `Stack` and `Queue` in `System.Collections` implement LIFO and FIFO disciplines respectively, but store every element as `object`, so they carry the same boxing and cast-on-read burden as `ArrayList`. `Stack.Pop()` and `Stack.Peek()` both return `object`, meaning code that holds a pick-order stack of strings must write `(string)pickStack.Pop()` on every retrieval, and the compiler will not prevent someone from pushing an integer onto what was intended as a string stack. `Queue.Dequeue()` and `Queue.Peek()` have the same signature. Both types also throw `InvalidOperationException` when you call `Pop` on an empty stack or `Dequeue` on an empty queue, which is identical behaviour to their generic counterparts.

`Stack<T>` and `Queue<T>` in `System.Collections.Generic` are parameterised, so `Stack<string>` can only receive strings and its `Pop()` returns `string` directly. For value types such as `int`, the generic versions avoid boxing entirely. The only behavioural difference worth noting in migration is that iteration order differs: iterating `Stack<T>` with `foreach` visits elements from top to bottom (most recent first), which is the same as non-generic `Stack`. When migrating, a direct type substitution plus the removal of all casts is usually sufficient, with the addition of `TryPop` or `TryDequeue` if the calling code currently checks `Count > 0` before popping.

---

## Q10. What does ICollection provide that IEnumerable does not, and how is it used with ArrayList?

**Concepts**
- `IEnumerable`: forward-only iteration
- `ICollection`: count, copy, and synchronisation
- `CopyTo(Array, int)` for bulk transfer
- `IsSynchronized` and `SyncRoot` for legacy thread safety
- Interface segregation in non-generic hierarchy

**Answer**

`IEnumerable` is the minimum contract for iteration: it exposes only `GetEnumerator()`, which allows `foreach` but nothing else. A caller holding only an `IEnumerable` reference cannot determine how many elements are present without iterating the whole sequence, and cannot copy elements to an array without writing the loop by hand.

`ICollection` extends `IEnumerable` with three additional capabilities. `Count` provides the element count as an O(1) property, avoiding a full iteration just to determine size. `CopyTo(Array destination, int index)` bulk-copies all elements into an existing array starting at the given offset — useful when the caller needs a snapshot in a plain `object[]` without allocating a new list. `IsSynchronized` and `SyncRoot` are legacy synchronisation hooks: `IsSynchronized` is `false` for plain `ArrayList`, and `SyncRoot` exposes the internal object that legacy code can lock on for coarse-grained thread safety. These were the pre-`System.Collections.Concurrent` mechanism and are considered obsolete today.

In `InterfaceDemo` in the project code, the caller obtains an `ICollection collectionView = internalList`, calls `collectionView.CopyTo(buffer, 0)`, and checks `IsSynchronized`. This pattern appears in legacy adapters that accept `ICollection` rather than a concrete type, allowing the same method to work with `ArrayList`, `SortedList`, or any other pre-generics collection that implements the interface.

---

## Q11. How does null handling work in ArrayList?

**Concepts**
- `object` accepting null as a valid value
- `null` stored as element vs missing element
- `Contains(null)` returning true
- `IndexOf(null)` locating the first null slot
- `Remove(null)` removing first null occurrence

**Answer**

Because `ArrayList` stores elements as `object` references, `null` is a perfectly legal element value — it simply stores a null reference in the corresponding slot of the backing `object[]`. This is distinct from the element not existing at all. Calling `list.Add(null)` increments `Count` by one, so a list with one null has `Count == 1`. Calling `list.Contains(null)` returns `true` if any element is null. `list.IndexOf(null)` returns the zero-based index of the first null slot, and `list.Remove(null)` removes the first null occurrence, shifting later elements left.

The practical implication is that null checking in iteration is mandatory: a `foreach` loop that casts every element without a null guard will throw `NullReferenceException` on a null element, not `InvalidCastException`. The safe pattern is `if (item is Product p) { ... }` rather than `(Product)item`. Similarly, when migrating to `List<T>` with a non-nullable reference type such as `List<Product>`, any nulls stored in the original `ArrayList` become compile-time errors if the new type is `List<Product>` (non-nullable) — you must decide between `List<Product?>` (allowing null) or pre-filtering nulls during migration. This null-permissiveness is one of the silent migration traps in legacy ArrayList code that contains null sentinels for "item not yet assigned."

---

## Q12. What is the thread safety model of ArrayList, and how should concurrent access be handled?

**Concepts**
- ArrayList not thread-safe by default
- `IsSynchronized` flag always false on plain instance
- `ArrayList.Synchronized(list)` wrapper
- Single-writer / multiple-reader hazard
- Preferred alternatives: `ConcurrentBag<T>`, lock statements

**Answer**

Plain `ArrayList` provides no thread safety. `IsSynchronized` returns `false` and multiple threads reading and writing concurrently — even if only one writes — can corrupt the internal state. The backing array can be resized mid-operation, the element count can be temporarily inconsistent during a grow-copy cycle, and two concurrent `Add` calls can write to the same slot, losing one element entirely.

The `System.Collections.ArrayList.Synchronized(ArrayList)` static factory method returns a wrapper that acquires an exclusive lock (on `SyncRoot`) for every public operation. This makes individual method calls thread-safe but does not compose: a check-then-act sequence such as `if (list.Count > 0) { item = list[0]; }` is not atomic even on the synchronized wrapper because another thread can clear the list between the count check and the index access. The wrapper also serialises all operations through one lock, so it is a bottleneck under concurrent load.

Modern .NET 10 code should use `System.Collections.Concurrent` types — `ConcurrentBag<T>`, `ConcurrentQueue<T>`, `ConcurrentStack<T>`, or `ConcurrentDictionary<TKey,TValue>` — which are designed for concurrent access with fine-grained locking or lock-free algorithms. When an existing `ArrayList` must be protected in a migration scenario, the safest pattern is to wrap the entire multi-step operation in a `lock` block on a dedicated object rather than relying on the synchronized wrapper.

---

## Q13. How do you migrate legacy ArrayList code to List\<T\>?

**Concepts**
- Namespace change from `System.Collections` to `System.Collections.Generic`
- Removing explicit casts on read
- Surfacing latent type errors at compile time
- `list.Cast<T>()` for LINQ bridge
- `AddRange` accepting `IEnumerable<T>`

**Answer**

The mechanical steps to migrate an `ArrayList` to `List<T>` are straightforward. Replace `System.Collections.ArrayList` with `List<T>` where `T` is the concrete type of the elements actually stored. Remove every explicit cast on read — `(Product)list[i]` becomes `list[i]`, `(string)list[j]` becomes `list[j]`. The compiler will now flag any `Add` call that passes the wrong type, surfacing latent bugs that were previously invisible at compile time.

The migration also has a diagnostic benefit: if the codebase compiles cleanly after removing all casts, the original code was consistently storing the expected type. If it does not compile, the type errors are genuine bugs that the `ArrayList` was silently concealing. Where the list is populated from a legacy API that still returns `ArrayList`, use LINQ's `Cast<T>()` extension to bridge the gap: `legacyList.Cast<Product>().ToList()` creates a typed `List<Product>` from the non-generic source. `Cast<T>()` throws `InvalidCastException` if any element does not match — which is exactly the error you want to discover at the boundary rather than deep in business logic.

For `AddRange`, `ArrayList.AddRange(ICollection)` maps to `List<T>.AddRange(IEnumerable<T>)`. The main API surface — `Add`, `Insert`, `Remove`, `RemoveAt`, `Count`, `Capacity`, `Contains`, `IndexOf`, `Clear`, `TrimExcess` (the `List<T>` equivalent of `TrimToSize`) — is essentially identical in naming and semantics, making mechanical migration straightforward.

---

## Q14. What is SortedList (non-generic) and how does it differ from SortedList\<TKey,TValue\>?

**Concepts**
- `SortedList` maintaining sorted key order
- `object` keys and values
- `IComparer` for custom comparison
- `SortedList<TKey,TValue>` generic replacement
- Index-based access on SortedList

**Answer**

Non-generic `System.Collections.SortedList` maintains a sorted array of key/value pairs where both keys and values are `object`. Keys are kept in ascending order determined by the default `Comparer` (which delegates to `IComparable` on the key type) or a custom `IComparer` supplied at construction. The sorted order makes range-based operations fast but insertion is O(n) because elements must be shifted to maintain order. Like other non-generic types, reading a value requires a cast: `int capacity = (int)zoneCapacity["Zone-A"]!`.

`SortedList<TKey,TValue>` in `System.Collections.Generic` parameterises both key and value, eliminating boxing for value-type keys and removing casts from all value retrievals. It exposes `Keys` and `Values` as `IList<TKey>` and `IList<TValue>` respectively, enabling direct indexed access to keys and values by position — for example, `sortedList.Keys[0]` without casting. Semantically the insert complexity remains O(n) in both versions because the sorted invariant requires element shifting. When the number of reads greatly exceeds writes and range queries are common, `SortedList<TKey,TValue>` is preferred; when the read/write ratio is more balanced, `SortedDictionary<TKey,TValue>` offers O(log n) insert at the cost of slightly higher memory.

---

## Q15. How does the non-generic IList interface relate to ArrayList, and why would you program against it?

**Concepts**
- `IList` as positional read/write abstraction
- Polymorphic method accepting any `IList` implementor
- `IsReadOnly` and `IsFixedSize` flags
- `Array` also implementing `IList`
- Decoupling from concrete ArrayList type

**Answer**

`IList` extends `ICollection` with positional access: the `this[int]` indexer, `Add`, `Insert`, `Remove`, `RemoveAt`, `Contains`, and `IndexOf`. Both `ArrayList` and `Array` implement `IList`, though `Array`'s implementation is fixed-size so calling `Add` on an `Array` reference through `IList` throws `NotSupportedException`. The `IsFixedSize` property distinguishes the two: it is `false` for `ArrayList` and `true` for `Array`.

Programming against `IList` rather than the concrete `ArrayList` type is the correct approach when writing code that must consume legacy collections coming from framework or COM APIs. A method signature `void ProcessLines(IList lines)` can receive an `ArrayList`, a synchronized wrapper around one, or even a custom `IList` implementation introduced later — the method is decoupled from the concrete type. Inside the method you interact through `Count`, the indexer, and `Contains`, which are guaranteed by the interface contract regardless of the concrete backing type. This pattern is important in legacy migration: the interface reference remains unchanged in call sites while the concrete type is swapped from `ArrayList` to a wrapped or generic type behind the scenes.

---

## Q16. What happens internally when ArrayList.Add is called and the backing array is full?

**Concepts**
- Capacity doubling on overflow
- `Array.Copy` for element migration
- Old array eligibility for GC
- Amortised O(1) complexity
- Pre-allocation with constructor or Capacity setter

**Answer**

When `Add` is called and `Count` equals `Capacity`, `ArrayList` cannot fit the new element in the existing `object[]`. The runtime determines the new capacity — typically `Capacity * 2` for a non-empty list, or a minimum of four for a list starting empty — allocates a new `object[]` of that size, copies all existing element references into it using `Array.Copy`, and then stores the new element in the first vacant slot. The old array is no longer referenced by the `ArrayList` and becomes eligible for garbage collection on the next collection cycle.

Because the capacity doubles on each reallocation, the total work performed by all copy operations across n successive `Add` calls is bounded by 2n — geometric series — making the amortised cost of `Add` O(1) even though individual reallocation operations are O(n). This is the same strategy used by `List<T>`, `System.Text.StringBuilder`, and most dynamic array implementations. The cost is paid entirely in allocations and copies; there is no CPU-expensive hashing or tree rebalancing. Pre-allocating by passing the expected capacity to the constructor `new ArrayList(n)` eliminates reallocation entirely when the final size is known in advance, at the cost of potentially reserving more memory than needed — which `TrimToSize()` reclaims afterwards.

---

## Q17. How does ArrayList.Sort work, and what are the limitations compared to List\<T\>.Sort?

**Concepts**
- `ArrayList.Sort()` using `IComparable` on elements
- Custom `IComparer` overload
- Homogeneous type requirement for sort to succeed
- `List<T>.Sort(Comparison<T>)` lambda overload
- Mixed-type list throwing `InvalidOperationException`

**Answer**

`ArrayList.Sort()` uses an internal introsort algorithm (the same family as `List<T>.Sort()`) but relies on `IComparable` (non-generic) dispatched via `object` for element comparisons. An overload accepts an `IComparer` (non-generic) for custom ordering. The limitation is that every element in the list must be comparable to every other: if the list is truly heterogeneous — containing both integers and strings — the sort will throw `InvalidOperationException` because `int` does not implement `IComparable` relative to `string`. This is a runtime failure with no compile-time warning.

`List<T>.Sort()` has the same algorithmic complexity but with the significant advantage that comparisons are type-safe. The `List<T>.Sort(Comparison<T>)` overload accepts a lambda, enabling concise inline sort definitions like `products.Sort((a, b) => a.Price.CompareTo(b.Price))` without creating a separate comparer class. The `List<T>.Sort(IComparer<T>)` overload uses `IComparer<T>` (generic), which avoids boxing for value types during comparison. For the rare case where a legacy `ArrayList` must be sorted, the safest approach is to ensure homogeneity first — either by construction or by filtering — before calling `Sort()`, or to migrate to `List<T>` so the type system enforces homogeneity.

---

## Gotchas

---

## Q18. Why does `(long)list[0]` throw InvalidCastException even when list[0] holds an int value?

**Concepts**
- Unboxing type must match exact boxed type
- No implicit widening during unbox
- Runtime type identity check before copy
- `Convert.ToInt64()` as safe alternative
- Boxing as a two-step operation: box then cast

**Answer**

Boxing wraps a value inside a heap object that carries its exact runtime type. When you store `list.Add(42)`, the runtime creates a boxed `int` object whose type tag is `System.Int32`. When you unbox with `(int)list[0]`, the runtime checks that the stored type tag is `System.Int32`, finds it matches, and copies the value — success. When you write `(long)list[0]`, the runtime checks for `System.Int64`, finds `System.Int32` instead, and throws `InvalidCastException` — even though `int` is implicitly widening-convertible to `long` in normal typed code.

The rule is: the unboxing cast must name the exact type that was boxed. Implicit numeric widening conversions (`int` to `long`, `float` to `double`, `byte` to `int`) apply only in typed contexts; they do not apply through `object`. To safely convert a boxed integer to `long`, you must first unbox to `int` and then widen: `(long)(int)list[0]`. Alternatively, `Convert.ToInt64(list[0])` uses reflection-based conversion that handles the widening correctly, but it is slower. This exact-type requirement is one of the most common runtime surprises in legacy ArrayList code: the original developer adds `int` values, a later developer reads them as `long` expecting numeric promotion, and the exception appears seemingly at random when the path is exercised.

---

## Q19. What subtle data loss can occur when iterating an ArrayList with a for loop and calling RemoveAt inside the loop?

**Concepts**
- Index shifting on `RemoveAt`
- Loop counter not compensating for removed element
- Skipping the element after the removed one
- Backward iteration as the standard fix
- `foreach` throwing `InvalidOperationException` instead of silently skipping

**Answer**

When `RemoveAt(i)` is called inside a forward `for` loop, every element at index `i+1` and beyond shifts left by one position. The loop counter then increments to `i+1`, but the element that was previously at `i+1` is now at `i` — so the loop skips it entirely without throwing any exception. This is a silent data loss bug: items are never processed, never removed when they should be, or removed incorrectly, depending on the logic.

The classic fix is to iterate backwards: `for (int i = list.Count - 1; i >= 0; i--)`. Removing `list[i]` shifts elements to its right, but those have already been visited, so no future iteration is affected. The alternative — collecting indices to remove in a first pass and then iterating them in reverse to call `RemoveAt` — is more readable in complex cases. Note that `foreach` over an `ArrayList` avoids the silent skipping problem entirely but replaces it with a loud one: any structural modification (`Add`, `Remove`, `RemoveAt`, `Clear`, `Insert`) invalidates the enumerator and the next `MoveNext()` call throws `InvalidOperationException`. The forward-for silently skips; `foreach` loudly fails. Both are undesirable — backward-for or separate pass is the correct pattern.

---

## Q20. Why does LINQ not work directly on ArrayList without an adapter, and what is the correct bridge?

**Concepts**
- LINQ extension methods targeting `IEnumerable<T>`
- `ArrayList` implementing only non-generic `IEnumerable`
- `Cast<T>()` extension method as bridge
- `OfType<T>()` for safe filtering without throw
- Performance cost of the cast enumeration step

**Answer**

All standard LINQ query operators — `Where`, `Select`, `OrderBy`, `Sum`, `Any`, `First`, and so on — are extension methods defined on `IEnumerable<T>` in `System.Linq`. `ArrayList` implements only the non-generic `IEnumerable`, which the LINQ extension methods do not target. Writing `arrayList.Where(x => ...)` is a compile error because the compiler cannot resolve the `Where` extension method on a non-generic `IEnumerable`.

The correct bridge is `Cast<T>()`, which is an extension method on non-generic `IEnumerable` that wraps it in a typed enumerator performing an explicit cast on each element: `arrayList.Cast<Product>().Where(p => p.Price > 50)`. This works but carries the same runtime risk as manual casting: if any element is not a `Product`, `Cast<T>()` throws `InvalidCastException` at the point the offending element is enumerated. A safer alternative is `OfType<T>()`, which silently skips elements that are not of type `T` rather than throwing — appropriate when the list is genuinely heterogeneous and you want only the elements of a particular type. `OfType<T>()` performs an `is` check internally and is the idiomatic way to query a mixed `ArrayList` with LINQ. For homogeneous lists, `Cast<T>().ToList()` materialises the list immediately and is the correct first step in migration.

---

## Q21. What is the wrong-null-sentinel trap in ArrayList and how does it differ from List\<T\> behaviour?

**Concepts**
- `null` as valid element in `ArrayList`
- Silent null propagation during iteration
- Null-reference exception at the cast site
- Non-nullable generic type rejecting null at compile time
- Nullable reference type annotations in .NET 10

**Answer**

Because `ArrayList` stores `object` references, `null` is an entirely legal element value. Code that uses null as a sentinel — for example, a null entry meaning "order slot is reserved but not yet filled" — compiles and runs without complaint. The trap springs during iteration: a `foreach (object item in list)` loop that immediately casts `(Product)item` will throw `NullReferenceException` rather than `InvalidCastException` when it encounters the null sentinel, because the null reference cannot be dereferenced to check its runtime type. The exception message points to the cast line rather than the `Add(null)` call that introduced the sentinel, making the root cause hard to find.

`List<Product>` with non-nullable reference type annotation rejects `list.Add(null)` at compile time in a project with nullable reference types enabled (`<Nullable>enable</Nullable>`), surfacing the issue immediately. If null entries are genuinely required, `List<Product?>` makes the intent explicit at the type level and forces the caller to handle the null case at every usage site. When migrating legacy `ArrayList` code that contains null sentinels, the first step is to locate all `Add(null)` call sites and decide whether to replace nulls with a `NullObject` pattern, a sentinel struct, or `List<T?>`. Leaving them as `List<T?>` and scattering null checks through consuming code is correct but verbose; the `NullObject` pattern eliminates the null entirely.

---

## Q22. Why is ArrayList.Synchronized not sufficient for compound operations, and what is the correct synchronisation pattern?

**Concepts**
- Synchronized wrapper locking individual operations
- Check-then-act race condition
- Non-atomic composite operations
- `lock` statement for critical sections
- `System.Collections.Concurrent` as preferred solution

**Answer**

`ArrayList.Synchronized(list)` returns a wrapper that acquires `SyncRoot` before every individual method call and releases it immediately after. Each individual call — `Count`, `Add`, `RemoveAt`, the indexer — is individually atomic. However, a sequence of two calls is not atomic: after the first call returns its lock, another thread can modify the list before the second call acquires the lock.

The canonical race condition is: thread A checks `if (list.Count > 0)`, finds it true, then thread B calls `list.Clear()`, then thread A calls `list[0]` — which now throws `ArgumentOutOfRangeException` because the list is empty. Both thread A and thread B were using the synchronized wrapper correctly for their individual operations, yet the compound action is unsound. The correct pattern is to lock the `SyncRoot` manually around the entire compound operation: `lock (list.SyncRoot) { if (list.Count > 0) { var item = list[0]; ... } }`. This is verbose and error-prone because forgetting one `lock` block anywhere in the codebase breaks the invariant. The modern alternative is `System.Collections.Concurrent.ConcurrentBag<T>`, `ConcurrentQueue<T>`, or `ConcurrentStack<T>`, which are designed from the ground up for concurrent access and expose atomic composite operations like `TryDequeue`.

---

## Real-World Scenarios

---

## Q23. You are reviewing a PR that introduces a new service layer. The following code uses ArrayList to maintain a list of order line items. Identify all issues and propose fixes.

**Concepts**
- Type-safety violations detectable at compile time with generics
- Boxing overhead on decimal and int values
- Missing null guard before cast
- Lack of LINQ compatibility
- Thread-unsafety under concurrent request handling

```csharp
// OrderService.cs  (legacy pattern introduced into a new .NET 10 service)
using System.Collections;

public class OrderService
{
    private ArrayList _lineItems = new ArrayList();

    public void AddLine(int productId, decimal unitPrice, int qty)
    {
        _lineItems.Add(productId);   // int — boxed
        _lineItems.Add(unitPrice);   // decimal — boxed
        _lineItems.Add(qty);         // int — boxed
    }

    public decimal GetTotal()
    {
        decimal total = 0m;
        for (int i = 1; i < _lineItems.Count; i += 3)
        {
            total += (decimal)_lineItems[i];
        }
        return total;
    }

    public void RemoveFirst()
    {
        _lineItems.RemoveAt(0);
        _lineItems.RemoveAt(0);
        _lineItems.RemoveAt(0);
    }
}
```

| Category | Problem | Impact |
|---|---|---|
| Type Safety | `productId`, `unitPrice`, and `qty` stored as separate `object` slots rather than a typed record | A caller adding items out of order or with wrong offsets silently corrupts data; no compile-time protection |
| Boxing | `int` and `decimal` value types boxed on every `Add` | Heap allocation per field per line; GC pressure proportional to order size |
| Fragile Offset Math | `GetTotal` accesses index `i+1` (unit price) by hardcoded arithmetic | Adding or reordering fields breaks the method silently; off-by-one errors corrupt results |
| Thread Safety | `_lineItems` is a shared mutable field with no synchronisation | Concurrent HTTP requests calling `AddLine` and `GetTotal` simultaneously cause data races |
| No LINQ | `GetTotal` iterates manually; no `Sum` or `Select` possible without `Cast<T>()` | More code, more opportunity for index bugs, no expressiveness |
| Null Risk | `RemoveFirst` calls `RemoveAt(0)` three times without checking `Count` | Throws `ArgumentOutOfRangeException` if fewer than three slots exist |

**Fix Priority**

1. Replace `ArrayList _lineItems` with `List<OrderLineItem>` where `OrderLineItem` is a record with `ProductId`, `UnitPrice`, and `Qty` properties — eliminating boxing, type errors, and the fragile offset arithmetic in one change.
2. Add a `lock` object or migrate to `ConcurrentBag<OrderLineItem>` / immutable snapshot pattern to eliminate the thread-safety race condition.
3. Replace the manual `GetTotal` loop with `_lineItems.Sum(l => l.UnitPrice * l.Qty)` once the generic list is in place.
4. Guard `RemoveFirst` with a `Count >= 1` check (or, after migration, simply remove by element reference rather than by index).

**Answer**

The central problem is that this code uses a flat `ArrayList` as a poor substitute for a typed record, storing three conceptually related values — product ID, unit price, quantity — as three consecutive anonymous `object` slots. This design makes every operation fragile: `GetTotal` works only if every `AddLine` call appended exactly three items in exactly the right order, with no intervening operations. There is no compile-time enforcement of that invariant. Each of the three value types is boxed on `Add` and unboxed on retrieval, producing six heap allocations per order line. The correct fix is to introduce a domain type — `record OrderLineItem(int ProductId, decimal UnitPrice, int Qty)` — and a `List<OrderLineItem>`. The resulting `GetTotal` is a single LINQ expression, `RemoveFirst` becomes `_lineItems.RemoveAt(0)` on a typed list guarded by a count check, and `AddLine` is a single `_lineItems.Add(new OrderLineItem(productId, unitPrice, qty))`. Thread safety is a separate concern that requires either a lock or a concurrent collection and cannot be solved by the type migration alone.

---

## Q24. A legacy .NET Framework COM interop wrapper returns an ArrayList of invoice records. You need to consume this data in a new .NET 10 service that uses strongly typed models. How do you bridge the gap safely?

**Concepts**
- COM interop returning non-generic `ArrayList`
- `Cast<T>()` bridge to LINQ
- `OfType<T>()` for heterogeneous or dirty data
- Adapter / anti-corruption layer pattern
- Boundary validation before materialising typed list

**Answer**

The correct architectural response is to confine the non-generic `ArrayList` to an adapter layer — an anti-corruption layer in domain-driven design terms — that converts the legacy output into a strongly typed `IReadOnlyList<InvoiceRecord>` before any domain logic sees it. Nothing outside the adapter should hold an `ArrayList` reference.

Inside the adapter, the conversion depends on data quality. If you trust that every element in the returned `ArrayList` is a non-null COM-interop `InvoiceDto` object, use `legacyList.Cast<InvoiceDto>().Select(dto => InvoiceRecord.FromDto(dto)).ToList()`. `Cast<InvoiceDto>()` will throw `InvalidCastException` on the first element that does not match — which is the correct behaviour at a system boundary because it surfaces data contract violations loudly rather than silently returning partial results. If the COM layer is unreliable and may return nulls, unexpected types, or mixed entries, use `OfType<InvoiceDto>()` instead and log or alert on the discarded elements so the integration failure is visible.

The adapter should also validate business rules — non-null invoice number, positive amount, valid date — before building the `InvoiceRecord` domain object, throwing a typed exception with diagnostic context rather than propagating a raw `InvalidCastException` that gives the caller no actionable information. This pattern isolates the non-generic ugliness entirely: all code beyond the adapter boundary deals exclusively with `List<InvoiceRecord>`, uses LINQ freely, and has full compile-time safety. The only maintenance burden is the adapter itself, which is the correct place to absorb the impedance mismatch with the legacy system.

---

## Q25. A performance profiling run on a warehouse reporting service shows that 40% of GC allocations come from a method that builds a summary of integer sensor readings. The method uses ArrayList. How do you diagnose and fix this?

**Concepts**
- Boxing allocations in managed heap
- GC allocation profiler output
- `List<int>` eliminating boxing for `int`
- `Span<int>` or array for zero-allocation summaries
- Batch processing and buffer reuse strategies

**Answer**

The profiler is pointing at boxing. Every time the method calls `readings.Add(sensorValue)` where `sensorValue` is an `int`, the runtime allocates a fresh 16-byte (on 64-bit) boxed integer on the managed heap. If the method processes thousands of readings per second — a realistic sensor ingestion rate — this generates millions of short-lived objects that the GC must collect, causing frequent Gen 0 collections and, if the readings are retained briefly across async boundaries, Gen 1 promotions.

The immediate fix is to replace `ArrayList` with `List<int>`. The JIT generates a specialised version of `List<int>` backed by an `int[]`, so `Add(sensorValue)` stores the integer directly in the array with no heap allocation beyond the array itself. The 40% GC load should largely disappear once this change is deployed. If further reduction is needed — for example, the method is a hot path called hundreds of thousands of times per second — consider pre-allocating `new List<int>(expectedCapacity)` to eliminate reallocation, or using a pooled `int[]` from `ArrayPool<int>.Shared` combined with `Span<int>` for the summary computation, which eliminates even the list allocation. The diagnostic approach for confirming the fix is to re-run the profiler under the same load after the change and verify that the allocation site drops from the top of the hot-path list; for production, an `EventCounters` or OpenTelemetry GC metric can confirm reduced allocation rate without a full profiler run.

---

## Q26. You are migrating a .NET Framework service to .NET 10. The service stores a list of warehouse bin locations in an ArrayList field and exposes it through a public IList property. Describe the migration steps and risks.

**Concepts**
- Public API surface using `IList` (non-generic)
- Binary compatibility considerations
- Changing return type to `IList<T>` as a breaking change
- Internal vs public migration scope
- Versioning and adapter shim for external consumers

**Answer**

The migration has two distinct scopes — the internal implementation and the public API surface — and conflating them is the primary risk. Internally, replacing `ArrayList _bins` with `List<BinLocation> _bins` is safe, removes all casts, eliminates boxing for any value-type fields on `BinLocation`, and enables LINQ. This internal change is invisible to callers and has no binary compatibility implications.

The public `IList` property is more sensitive. If callers are in the same solution, changing the return type from `IList` to `IList<BinLocation>` is source-compatible — any code that iterated the non-generic `IList` with `foreach (object item in bins)` should be updated to `foreach (BinLocation bin in bins)` — but it is a binary breaking change if the property is part of an assembly published as a library and consumed by code compiled against the old signature. In that case, provide both: retain the `IList` property with a `[Obsolete]` attribute (returning `new ArrayList(_bins)` or `_bins.Cast<object>().ToList()`) and add a new `IList<BinLocation> BinLocations` property. Schedule removal of the obsolete property after consumer migration is verified.

The hidden risk is null: if the existing `ArrayList` contains null sentinels for "bin not yet assigned," the migration to `List<BinLocation>` (non-nullable) will surface compile errors on those `Add(null)` call sites — which is exactly the right outcome. Address each one explicitly rather than weakening to `List<BinLocation?>` unless nulls carry genuine domain meaning.

---

## Q27. A colleague argues that using ArrayList for a truly heterogeneous log-entry collection — mixing strings, ints, DateTimes, and custom structs — is justified because List\<T\> requires a single type parameter. Evaluate this argument.

**Concepts**
- Heterogeneous collections as a design smell
- `object` as the universal base type
- Discriminated union / sealed hierarchy pattern
- `List<ILogEntry>` with polymorphism
- `object[]` as explicit heterogeneous container

**Answer**

The argument has a narrow technical validity but fails as a design position. It is true that `List<T>` requires a single type parameter, so `List<object>` is the closest generic equivalent to `ArrayList` for truly mixed content — both store `object` and require casts. Using `List<object>` over `ArrayList` is still preferable because it is LINQ-compatible, does not carry the legacy stigma, and signals intentional heterogeneity rather than accidental use of an old API.

However, the deeper question is whether a genuinely heterogeneous flat list is the right model at all. Log entries that contain strings, integers, `DateTimes`, and custom structs are almost always logically related through a common abstraction — they are all log entries; they differ in their payload type. The idiomatic .NET solution is a sealed hierarchy or interface: `interface ILogEntry { DateTime Timestamp { get; } }` with concrete types `StringLogEntry`, `MetricLogEntry`, and `StructuredLogEntry` all implementing it. The collection becomes `List<ILogEntry>`, which is fully typed, LINQ-compatible, and exploits polymorphic dispatch to eliminate `is`-checks in consuming code.

If the log format is genuinely unstructured — for example, raw tokens from a deserialiser that may be any JSON primitive — then `List<object>` or `object[]` is acceptable at the serialisation boundary, but the payload should be converted to a typed model before entering the domain layer. Using `ArrayList` in new code for this purpose is unjustified: the type carries legacy overhead, lacks LINQ extension methods directly, and signals to future readers that the code predates generics rather than making a deliberate design choice.

---

## Q28. During a code review you encounter a method that accepts IList and calls list[0] directly without checking Count. Explain the full set of exceptions this method could throw and how to make it robust.

**Concepts**
- `ArgumentOutOfRangeException` on empty list access
- `NullReferenceException` when list parameter is null
- `InvalidCastException` when element type is unexpected
- Defensive contracts and guard clauses
- `ArgumentNullException` vs `ArgumentException` selection

**Answer**

A method `void Process(IList list)` that accesses `list[0]` without guards is vulnerable to at least three distinct exceptions. First, if `list` itself is `null`, accessing `list[0]` dereferences a null reference and throws `NullReferenceException` — this should be prevented by a null guard at the method entry: `ArgumentNullException.ThrowIfNull(list)` in .NET 10. Second, if `list` is non-null but empty (`list.Count == 0`), accessing `list[0]` throws `ArgumentOutOfRangeException` from inside the `IList` implementation. Third, if the method proceeds to cast `(Product)list[0]` and the first element is not a `Product`, it throws `InvalidCastException`.

Making the method robust requires three corresponding guards. Replace the null reference risk with `ArgumentNullException.ThrowIfNull(list, nameof(list))` as the first statement. Guard the empty case with `if (list.Count == 0) throw new ArgumentException("List must contain at least one element.", nameof(list))` or, if an empty list is a normal case rather than a programming error, return early or return a sentinel. Replace the hard cast with pattern matching: `if (list[0] is not Product product) throw new InvalidOperationException($"Expected Product at index 0, got {list[0]?.GetType().Name ?? "null"}")`. This gives the caller a diagnostic message with the actual type rather than a bare `InvalidCastException`. In general, method boundaries that accept non-generic `IList` are high-risk surfaces that deserve explicit contracts — both as guard clauses and as XML doc comments stating what element types are expected — because the type system cannot enforce them.
