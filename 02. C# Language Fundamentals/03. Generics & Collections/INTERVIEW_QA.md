# 03. Generics & Collections — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

**01. Generics** (Q1–Q20)
- [Q1. What are generics in C#? Why were they introduced?](#q1-what-are-generics-in-c-why-were-they-introduced)
- [Q2. What is the difference between generic and non-generic collections?](#q2-what-is-the-difference-between-generic-and-non-generic-collections)
- [Q3. Explain generic constraints in C# (`where` clause) with examples.](#q3-explain-generic-constraints-in-c-where-clause-with-examples)
- [Q4. Explain covariance and contravariance in generics (`in` and `out` keywords).](#q4-explain-covariance-and-contravariance-in-generics-in-and-out-keywords)
- [Q5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?](#q5-can-you-use-where-t--enum-or-where-t--unmanaged-what-problems-do-these-solve)
- [Q6. What happens when you use `default(T)` on an unconstrained type parameter?](#q6-what-happens-when-you-use-defaultt-on-an-unconstrained-type-parameter)
- [Q7. Why can't you write `T value = null;` unless `T` is constrained to `class`?](#q7-why-cant-you-write-t-value--null-unless-t-is-constrained-to-class)
- [Q8. What is the difference between a generic class and a generic method?](#q8-what-is-the-difference-between-a-generic-class-and-a-generic-method)
- [Q9. Reflection over an open generic type (`List<>`) vs a closed generic type (`List<int>`).](#q9-reflection-over-an-open-generic-type-list-vs-a-closed-generic-type-listint)
- [Q10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`?](#q10-why-does-typeoflistint--typeofliststring-return-false)
- [Q11. Type erasure vs reification — does C# retain generic type information at runtime?](#q11-type-erasure-vs-reification--does-c-retain-generic-type-information-at-runtime)
- [Q12. What does `where T : new()` enable?](#q12-what-does-where-t--new-enable)
- [Q13. Difference between `where T : class` and `where T : struct`.](#q13-difference-between-where-t--class-and-where-t--struct)
- [Q14. What does `where T : notnull` mean for nullable reference type analysis?](#q14-what-does-where-t--notnull-mean-for-nullable-reference-type-analysis)
- [Q15. Why are generic value types separate closed types at runtime for static fields?](#q15-why-are-generic-value-types-separate-closed-types-at-runtime-for-static-fields)
- [Q16. Covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?](#q16-covariance-on-ienumerableout-t--why-can-you-assign-ienumerablestring-to-ienumerableobject)
- [Q17. Contravariance on `Action<in T>` / `IComparer<in T>`.](#q17-contravariance-on-actionin-t--icomparerin-t)
- [Q18. Why is `List<T>` neither covariant nor contravariant on `T`?](#q18-why-is-listt-neither-covariant-nor-contravariant-on-t)
- [Q19. Generic specialization performance: value types vs reference types.](#q19-generic-specialization-performance-value-types-vs-reference-types)
- [Q20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?](#q20-can-you-cast-from-liststring-to-listobject--what-error-or-exception-occurs)

**02. ArrayList** (Q21–Q30)
- [Q21. Difference between `Array` and `ArrayList`.](#q21-difference-between-array-and-arraylist)
- [Q22. Difference between `List<T>` and `ArrayList`.](#q22-difference-between-listt-and-arraylist)
- [Q23. Why is `ArrayList` considered a legacy collection in modern C#?](#q23-why-is-arraylist-considered-a-legacy-collection-in-modern-c)
- [Q24. What boxing occurs when storing `int` values in an `ArrayList`?](#q24-what-boxing-occurs-when-storing-int-values-in-an-arraylist)
- [Q25. Performance cost of repeated boxing/unboxing in hot loops using `ArrayList`.](#q25-performance-cost-of-repeated-boxingunboxing-in-hot-loops-using-arraylist)
- [Q26. Can you store mixed types in an `ArrayList`, and what typing risks does that create?](#q26-can-you-store-mixed-types-in-an-arraylist-and-what-typing-risks-does-that-create)
- [Q27. Difference between `ArrayList.Capacity` and `Count`.](#q27-difference-between-arraylistcapacity-and-count)
- [Q28. When might you still encounter `ArrayList` in maintained legacy codebases?](#q28-when-might-you-still-encounter-arraylist-in-maintained-legacy-codebases)
- [Q29. Difference between `ArrayList` and `object[]`.](#q29-difference-between-arraylist-and-object)
- [Q30. Legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) for maintenance scenarios.](#q30-legacy-non-generic-collections-hashtable-queue-stack-for-maintenance-scenarios)

**03. List** (Q31–Q45)
- [Q31. Internal working and performance of `List<T>` vs `LinkedList<T>`.](#q31-internal-working-and-performance-of-listt-vs-linkedlistt)
- [Q32. What is `LinkedList<T>` and when should it be used?](#q32-what-is-linkedlistt-and-when-should-it-be-used)
- [Q33. Difference between `List<T>.Sort()` stability and `OrderBy()` stability.](#q33-difference-between-listtsort-stability-and-orderby-stability)
- [Q34. How does `List<T>` grow its internal buffer when capacity is exceeded?](#q34-how-does-listt-grow-its-internal-buffer-when-capacity-is-exceeded)
- [Q35. Amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle.](#q35-amortized-cost-of-add-on-listt-vs-insert-at-the-beginning-or-middle)
- [Q36. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?](#q36-what-does-listtasreadonly-return-and-can-callers-still-mutate-the-underlying-list)
- [Q37. Difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list.](#q37-difference-between-convertall-foreach-and-linq-select-on-a-list)
- [Q38. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?](#q38-what-do-toarray-copyto-and-getrange-do--which-allocate-new-arrays)
- [Q39. When to expose `List<T>` vs `IReadOnlyList<T>` or `IEnumerable<T>` as a return type.](#q39-when-to-expose-listt-vs-ireadonlylistt-or-ienumerablet-as-a-return-type)
- [Q40. Difference between `List<T>.Capacity` and `Count`.](#q40-difference-between-listtcapacity-and-count)
- [Q41. What happens if you mutate a list while iterating with `foreach`?](#q41-what-happens-if-you-mutate-a-list-while-iterating-with-foreach)
- [Q42. What is `TrimExcess`, and when is it useful?](#q42-what-is-trimexcess-and-when-is-it-useful)
- [Q43. Binary search on a list (`BinarySearch`) — what precondition must the list satisfy?](#q43-binary-search-on-a-list-binarysearch--what-precondition-must-the-list-satisfy)
- [Q44. `List<T>` indexer access compared to `LinkedList<T>` (no indexer).](#q44-listt-indexer-access-compared-to-linkedlistt-no-indexer)
- [Q45. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?](#q45-what-is-comparisont-delegate-and-how-does-it-relate-to-listtsort)

**04. Dictionary** (Q46–Q58)
- [Q46. Difference between `Dictionary<TKey, TValue>` and `Hashtable`.](#q46-difference-between-dictionarytkey-tvalue-and-hashtable)
- [Q47. `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`.](#q47-idictionarytkey-tvalue-and-ireadonlydictionarytkey-tvalue)
- [Q48. How does `Dictionary<TKey, TValue>` handle hashing and collisions?](#q48-how-does-dictionarytkey-tvalue-handle-hashing-and-collisions)
- [Q49. Difference between `Dictionary.Add` and the indexer when the key already exists.](#q49-difference-between-dictionaryadd-and-the-indexer-when-the-key-already-exists)
- [Q50. Difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup.](#q50-difference-between-containskey-trygetvalue-and-the-indexer-for-lookup)
- [Q51. Why must keys be immutable (or stable) after insertion?](#q51-why-must-keys-be-immutable-or-stable-after-insertion)
- [Q52. What exception is thrown when accessing a missing key via the indexer?](#q52-what-exception-is-thrown-when-accessing-a-missing-key-via-the-indexer)
- [Q53. Can `null` be used as a key when `TKey` is a reference type?](#q53-can-null-be-used-as-a-key-when-tkey-is-a-reference-type)
- [Q54. Average vs worst-case time complexity for lookup, insert, and remove.](#q54-average-vs-worst-case-time-complexity-for-lookup-insert-and-remove)
- [Q55. Hash code contract between `GetHashCode` and `Equals` for custom key types.](#q55-hash-code-contract-between-gethashcode-and-equals-for-custom-key-types)
- [Q56. `IEqualityComparer<TKey>` — when do you pass a custom comparer to the constructor?](#q56-iequalitycomparertkey--when-do-you-pass-a-custom-comparer-to-the-constructor)
- [Q57. When to choose `Dictionary` over `List` for lookups by id or SKU.](#q57-when-to-choose-dictionary-over-list-for-lookups-by-id-or-sku)
- [Q58. What happens internally when two keys hash to the same bucket?](#q58-what-happens-internally-when-two-keys-hash-to-the-same-bucket)

**05. HashSet** (Q59–Q68)
- [Q59. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?](#q59-explain-hashsett-and-its-use-cases-how-is-it-different-from-listt)
- [Q60. Difference between `SortedSet<T>` and `HashSet<T>`.](#q60-difference-between-sortedsett-and-hashsett)
- [Q61. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?](#q61-why-does-hashsett-require-correct-gethashcodeequals-for-custom-types)
- [Q62. Set operations: `UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`.](#q62-set-operations-unionwith-intersectwith-exceptwith-symmetricexceptwith)
- [Q63. `Add` returning `false` on duplicate vs `List.Add` behavior.](#q63-add-returning-false-on-duplicate-vs-listadd-behavior)
- [Q64. Constructing `HashSet<T>` with custom equality (`IEqualityComparer<T>`).](#q64-constructing-hashsett-with-custom-equality-iequalitycomparert)
- [Q65. `HashSet<T>` for deduplication vs `Distinct()` in LINQ.](#q65-hashsett-for-deduplication-vs-distinct-in-linq)
- [Q66. Set membership test in `HashSet` vs scanning a `List`.](#q66-set-membership-test-in-hashset-vs-scanning-a-list)
- [Q67. `IsSubsetOf`, `IsSupersetOf`, and `Overlaps`.](#q67-issubsetof-issupersetof-and-overlaps)
- [Q68. Modifying an element in `HashSet` in place if it affects equality.](#q68-modifying-an-element-in-hashset-in-place-if-it-affects-equality)

**06. Queue and Stack** (Q69–Q78)
- [Q69. `Queue<T>` and `Stack<T>` vs their non-generic counterparts.](#q69-queuet-and-stackt-vs-their-non-generic-counterparts)
- [Q70. FIFO vs LIFO, and which collection maps to each.](#q70-fifo-vs-lifo-and-which-collection-maps-to-each)
- [Q71. Operations `Queue<T>` exposes.](#q71-operations-queuet-exposes)
- [Q72. Operations `Stack<T>` exposes.](#q72-operations-stackt-exposes)
- [Q73. Why `Queue` and `Stack` do not support random access by index.](#q73-why-queue-and-stack-do-not-support-random-access-by-index)
- [Q74. `Queue<T>` in breadth-first search (BFS).](#q74-queuet-in-breadth-first-search-bfs)
- [Q75. Why BFS finds shortest paths in unweighted graphs.](#q75-why-bfs-finds-shortest-paths-in-unweighted-graphs)
- [Q76. Real-world workflows that map to a stack.](#q76-real-world-workflows-that-map-to-a-stack)
- [Q77. Non-generic `Queue`/`Stack` vs generic versions regarding boxing.](#q77-non-generic-queuestack-vs-generic-versions-regarding-boxing)
- [Q78. `Queue<T>` over `List<T>` with remove-from-front patterns.](#q78-queuet-over-listt-with-remove-from-front-patterns)

**07. SortedList & SortedDictionary** (Q79–Q86)
- [Q79. `SortedList<TKey, TValue>` and `SortedDictionary<TKey, TValue>` — when to use each.](#q79-sortedlisttkey-tvalue-and-sorteddictionarytkey-tvalue--when-to-use-each)
- [Q80. Interface defining ordering: `IComparer<TKey>` vs `IEqualityComparer<TKey>`.](#q80-interface-defining-ordering-icomparertkey-vs-iequalitycomparertkey)
- [Q81. `SortedList` (array-backed) vs `SortedDictionary` (tree-backed) performance.](#q81-sortedlist-array-backed-vs-sorteddictionary-tree-backed-performance)
- [Q82. When `SortedList` is preferred over `SortedDictionary`.](#q82-when-sortedlist-is-preferred-over-sorteddictionary)
- [Q83. Cost of inserting out-of-order keys into a sorted collection.](#q83-cost-of-inserting-out-of-order-keys-into-a-sorted-collection)
- [Q84. Lookup by index in `SortedList` — what `Keys[index]` provides.](#q84-lookup-by-index-in-sortedlist--what-keysindex-provides)
- [Q85. Difference between `SortedSet<T>` and `SortedDictionary<TKey, TValue>`.](#q85-difference-between-sortedsett-and-sorteddictionarytkey-tvalue)
- [Q86. `SortedDictionary` vs sorting keys from a `Dictionary` at read time.](#q86-sorteddictionary-vs-sorting-keys-from-a-dictionary-at-read-time)

**08. IEnumerable & IEnumerator** (Q87–Q115)
- [Q87. Difference between `IEnumerable<T>` and `ICollection<T>`.](#q87-difference-between-ienumerablet-and-icollectiont)
- [Q88. Difference between `ICollection<T>` and `IList<T>`.](#q88-difference-between-icollectiont-and-ilistt)
- [Q89. `IReadOnlyList<T>` and `IReadOnlyCollection<T>`.](#q89-ireadonlylistt-and-ireadonlycollectiont)
- [Q90. Difference between `IEnumerator` and `IEnumerator<T>`.](#q90-difference-between-ienumerator-and-ienumeratort)
- [Q91. The `yield` keyword and iterator methods.](#q91-the-yield-keyword-and-iterator-methods)
- [Q92. Deferred execution vs immediate execution for IEnumerable sequences.](#q92-deferred-execution-vs-immediate-execution-for-ienumerable-sequences)
- [Q93. Iterator pattern — `MoveNext`, `Current`, and `Reset`.](#q93-iterator-pattern--movenext-current-and-reset)
- [Q94. `yield break` vs `return` in an iterator method.](#q94-yield-break-vs-return-in-an-iterator-method)
- [Q95. Why multiple enumeration of the same `IEnumerable` re-runs the pipeline.](#q95-why-multiple-enumeration-of-the-same-ienumerable-re-runs-the-pipeline)
- [Q96. Returning `IEnumerable<T>` vs `List<T>` from a method.](#q96-returning-ienumerablet-vs-listt-from-a-method)
- [Q97. Modifying a collection during `foreach` — how the enumerator detects it.](#q97-modifying-a-collection-during-foreach--how-the-enumerator-detects-it)
- [Q98. Covariance on `IEnumerable<out T>` — practical assignment examples.](#q98-covariance-on-ienumerableout-t--practical-assignment-examples)
- [Q99. Difference between `foreach` and manual `while (enumerator.MoveNext())`.](#q99-difference-between-foreach-and-manual-while-enumeratormovenext)
- [Q100. `ToList()` materialization — when to materialize before multiple passes.](#q100-tolist-materialization--when-to-materialize-before-multiple-passes)
- [Q101. `IAsyncEnumerable<T>` and async iterators.](#q101-iasyncenumerablet-and-async-iterators)
- [Q102. Gotcha: Modify while iterating.](#q102-gotcha-modify-while-iterating)
- [Q103. Gotcha: Mutable keys.](#q103-gotcha-mutable-keys)
- [Q104. Gotcha: `IEnumerable<T>` covariant, `List<T>` not.](#q104-gotcha-ienumerablet-covariant-listt-not)
- [Q105. Gotcha: Wrong collection for the job.](#q105-gotcha-wrong-collection-for-the-job)
- [Q106. Gotcha: Static fields on generic types.](#q106-gotcha-static-fields-on-generic-types)
- [Q107. Gotcha: Boxing in non-generic collections.](#q107-gotcha-boxing-in-non-generic-collections)
- [Q108. Gotcha: Passing `List<T>` by value.](#q108-gotcha-passing-listt-by-value)
- [Q109. Gotcha: `Dictionary.Add` vs indexer on duplicate key.](#q109-gotcha-dictionaryadd-vs-indexer-on-duplicate-key)
- [Q110. Gotcha: `AsReadOnly()` is a view.](#q110-gotcha-asreadonly-is-a-view)
- [Q111. Gotcha: Assuming dictionary enumeration order.](#q111-gotcha-assuming-dictionary-enumeration-order)
- [Q112. Gotcha: Multiple enumeration cost.](#q112-gotcha-multiple-enumeration-cost)
- [Q113. Gotcha: Wrong comparer on sorted types.](#q113-gotcha-wrong-comparer-on-sorted-types)
- [Q114. Gotcha: Poor `GetHashCode` distribution.](#q114-gotcha-poor-gethashcode-distribution)
- [Q115. Gotcha: `Queue.Contains` is O(n).](#q115-gotcha-queuecontains-is-on)

---

### 01. Generics

---

## Q1. What are generics in C#? Why were they introduced?

**Concepts**
- type parameterization
- compile-time type safety
- boxing elimination for value types
- CLR reification (per-construction JIT code)
- code reuse without runtime casts

**Answer**

Generics let you define classes, methods, interfaces, and delegates with type parameters that are filled in at the call site rather than at definition time, so `List<int>` and `List<string>` are distinct types derived from the same `List<T>` definition. They were introduced in C# 2.0 to eliminate two problems with the `object`-based collections that preceded them: runtime `InvalidCastException` from unchecked casts, and GC pressure from boxing every value type into a heap object on insertion. Since the CLR reifies generics — JIT-compiling separate native code for each distinct value-type substitution — `List<int>` stores unboxed integers directly in a typed array while the compiler rejects any `Add` call with the wrong type, which means both cast failures and unnecessary allocations disappear at no extra code cost.

---

## Q2. What is the difference between generic and non-generic collections?

**Concepts**
- object-based non-generic storage
- boxing and unboxing overhead
- compile-time vs runtime type enforcement
- ArrayList legacy
- List<T> type safety

**Answer**

Non-generic collections like `ArrayList`, `Hashtable`, `Queue`, and `Stack` store every element as `object`, so value types are boxed on insert and require an explicit cast on read with no compiler protection against wrong-type insertions. Generic collections like `List<T>`, `Dictionary<TKey,TValue>`, and `HashSet<T>` parameterize the element type, which means mismatched `Add` calls fail at compile time and value types live unboxed in a typed backing array. This trade of heterogeneous flexibility for type safety and unboxed storage is almost always the right one because the added safety costs nothing at runtime and the unboxed storage actively improves GC throughput in value-type-heavy workloads.

---

## Q3. Explain generic constraints in C# (`where` clause) with examples.

**Concepts**
- constraint kinds (struct, class, new(), base type, interface)
- mutual exclusivity of struct and class/base-type constraints
- compiler-enforced member availability
- multiple constraints on one type parameter
- newer constraints (notnull, Enum, unmanaged)

**Answer**

The `where T : constraint` clause tells the compiler which members are available on `T` and which substitutions are legal. The main forms are `where T : struct` (value types only, enables unboxed storage and rules out null), `where T : class` (reference types, allows `T value = null`), `where T : new()` (requires a public parameterless constructor so `new T()` is legal inside the body), `where T : SomeBase` (requires inheritance from a class or interface, enabling member access), and newer additions like `where T : notnull`, `where T : Enum`, and `where T : unmanaged`. Multiple constraints on one parameter are comma-separated — for example `where T : class, IComparable<T>, new()` — but `struct` and a class base are mutually exclusive since a struct cannot inherit a class, so combining them is a compile error.

---

## Q4. Explain covariance and contravariance in generics (`in` and `out` keywords).

**Concepts**
- covariance (out) — output positions only
- contravariance (in) — input positions only
- interface-only and delegate variance
- IEnumerable<out T> covariance
- Action<in T> contravariance
- type safety rationale

**Answer**

Covariance, marked with `out T` on an interface or delegate, means the type parameter may only appear in return/output positions, so `IEnumerable<string>` is assignable to `IEnumerable<object>` because every element you read out is a `string` which is safely treated as an `object`. Contravariance, marked with `in T`, restricts the type parameter to input positions, so `Action<object>` is assignable to `Action<string>` because a handler that accepts any object can certainly handle a string. Mutable generic classes like `List<T>` are invariant because the same `T` appears in both positions — allowing covariant assignment would let you call `Add(42)` through a `List<object>` reference that is really a `List<string>`, which would corrupt the backing array. Variance annotations are only valid on interfaces and delegates because the compiler can statically verify the in/out position rule there.

---

## Q5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?

**Concepts**
- Enum constraint (C# 7.3)
- unmanaged constraint (C# 7.3)
- boxing elimination for enum helpers
- unsafe pointer operations
- Span<T> with unmanaged types

**Answer**

`where T : Enum` lets you write generic helpers that work across all enum types without boxing — before this constraint, calling `HasFlag` or formatting an enum generically required accepting `object` or using reflection, both of which box the value on every call. `where T : unmanaged` restricts `T` to types with no managed references (primitives, pointers, and structs composed only of unmanaged fields), since that guarantee is required to take `&item`, use `sizeof(T)`, or construct `Span<T>` from a pointer without unsafe caveats. Both constraints were added in C# 7.3 and solve the problem of duplicating utility code for each concrete type or accepting unnecessary runtime cost when working with enum flags, bitwise masks, or low-level memory representations.

---

## Q6. What happens when you use `default(T)` on an unconstrained type parameter?

**Concepts**
- default value expression
- null for reference types
- zero-value for value types
- unconstrained T ambiguity
- default literal (C# 7.1)

**Answer**

`default(T)` produces `null` when `T` is a reference type and the zero-value of the struct when `T` is a value type — `0` for `int`, `false` for `bool`, all fields zeroed for a struct. Since an unconstrained `T` could be either kind, the compiler allows `default(T)` as the universal way to produce a "nothing" value without knowing the concrete substitution, because both outcomes are valid initializations for their respective kinds. The default literal (C# 7.1) makes this more concise as just `default`, and `T? value = default` is the idiomatic way to represent an absent value for both constrained and unconstrained type parameters since `default` evaluates to `null` for reference types and zero-init for value types.

---

## Q7. Why can't you write `T value = null;` unless `T` is constrained to `class`?

**Concepts**
- null non-assignability to value types
- class constraint enabling null
- compiler static verification
- T? syntax for unconstrained nullable
- default as the universal zero

**Answer**

`null` is only a valid value for reference types because value types are stored inline and have no null state — an `int`, `bool`, or any struct simply cannot hold null. An unconstrained `T` might resolve to `int` or another struct at the call site, so the compiler rejects `T value = null` to prevent an assignment that would be meaningless for value-type substitutions. Adding `where T : class` tells the compiler every substitution will be a reference type, making `null` legal. When you need an "absent" value for any `T` without constraining to class, write `T? value = default` — for reference types `default` is `null`, for value types it is the zero-initialized struct, which is safe for both.

---

## Q8. What is the difference between a generic class and a generic method?

**Concepts**
- class-level type parameter
- method-level type parameter
- type argument inference at call site
- scope and lifetime of T
- additional type parameters on methods

**Answer**

A generic class declares its type parameter at the class level, so every method and field in that class shares the same `T` and each instance is tied to one specific substitution like `List<int>` or `Repository<Product>`. A generic method declares its own type parameter independently of the class, enabling type-specific behavior on a non-generic class or introducing extra type parameters beyond what the class already has. The compiler infers a generic method's type argument from the call-site arguments when possible, so `Swap(a, b)` resolves without `Swap<int>(a, b)` when both arguments are `int`. The essential difference is scope: the class parameter is fixed for an instance's lifetime, while the method parameter resolves fresh per call and disappears after the call returns.

---

## Q9. Reflection over an open generic type (`List<>`) vs a closed generic type (`List<int>`)

**Concepts**
- open generic type definition
- closed constructed type
- MakeGenericType
- IsGenericTypeDefinition
- GetGenericArguments

**Answer**

`typeof(List<>)` is an open generic type definition — `IsGenericTypeDefinition` returns `true`, it has no concrete type argument, and you cannot create instances from it directly. Calling `MakeGenericType(typeof(int))` closes it, returning a `Type` object representing `List<int>`, from which you can call `Activator.CreateInstance`. `GetGenericArguments()` on the open type returns the unbound parameter `T`; the same call on `List<int>` returns `typeof(int)`. Open types are useful in DI containers, code generators, and serializers that need to register or reflect on generic patterns without knowing the concrete argument at registration time — you store `typeof(List<>)` and close it later with whatever element type the caller specifies.

---

## Q10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`?

**Concepts**
- closed type identity per construction
- Type object equality
- GetGenericTypeDefinition
- shared open type definition
- per-value-type CLR specialization

**Answer**

`typeof(List<int>)` and `typeof(List<string>)` are different `Type` objects because the CLR creates a distinct type representation for each unique closed construction — they share the same generic definition but are separate types. For value-type substitutions the CLR also JIT-compiles separate machine code, reinforcing that they are genuinely different. Calling `typeof(List<int>).GetGenericTypeDefinition()` returns `typeof(List<>)`, and comparing the definitions of both: `typeof(List<int>).GetGenericTypeDefinition() == typeof(List<string>).GetGenericTypeDefinition()` returns `true` since both share the same open definition. This distinction matters when writing plugin loaders or serializers that need to recognize "any `List<>` of anything" versus "exactly `List<int>`".

---

## Q11. Type erasure vs reification — does C# retain generic type information at runtime?

**Concepts**
- Java type erasure
- CLR reification
- per-value-type JIT specialization
- shared reference-type code
- typeof(T) at runtime

**Answer**

Java erases generic type arguments at runtime — a `List<String>` and `List<Integer>` are both plain `List` at the bytecode level, so value types cannot be stored without boxing and you cannot ask a `List<String>` what type it holds. C# reifies generics: the CLR preserves all type arguments in metadata and JIT-compiles separate native code for each distinct value-type substitution, so `List<int>` and `List<double>` are genuinely different types with optimized int-array and double-array backing. Reference-type substitutions share a single compiled code body because all references are pointer-sized, but the type argument is still available at runtime through `typeof(T)`, `GetType()`, and reflection. This means writing `typeof(T) == typeof(int)` inside a generic method returns the correct answer, which is impossible in Java without extra metadata annotations.

---

## Q12. What does `where T : new()` enable?

**Concepts**
- new() constraint
- parameterless constructor requirement
- Activator.CreateInstance alternative
- struct implicit default constructor
- factory and ORM use cases

**Answer**

`where T : new()` requires that `T` has a public parameterless constructor, which allows `new T()` inside the generic method or class body to create instances without knowing the concrete type. Every struct satisfies `new()` by definition since structs always have an implicit parameterless constructor; classes only satisfy it if they explicitly declare or inherit a public no-arg constructor. Records with a primary constructor still get a synthesized parameterless constructor unless they suppress it. The constraint is used in factory helpers, ORM row mappers, and default-construction utilities, though it is limited to parameterless creation — when construction needs arguments or DI, `Activator.CreateInstance<T>()` or a factory delegate parameter is more flexible.

---

## Q13. Difference between `where T : class` and `where T : struct`

**Concepts**
- class constraint — reference types, nullable, heap allocation
- struct constraint — value types, non-nullable, inline storage
- mutual exclusivity
- assignment semantics
- nullable class constraint (T : class?)

**Answer**

`where T : class` restricts `T` to reference types, meaning the substitution is heap-allocated, assignment copies the reference, and `T value = null` is legal. `where T : struct` restricts `T` to non-nullable value types, meaning the substitution is stored inline or on the stack, assignment copies the entire value, and null is not assignable. The two are mutually exclusive — a single type parameter cannot carry both — and you choose based on whether you need null-checking and identity semantics or boxing-free inline storage. With nullable reference types enabled, `where T : class?` extends the class constraint to include nullable reference types, letting `T` be `string?` as well as `string`.

---

## Q14. What does `where T : notnull` mean for nullable reference type analysis?

**Concepts**
- notnull constraint
- nullable reference types (NRT)
- nullable annotation context
- compiler warning suppression
- T? in unconstrained contexts

**Answer**

`where T : notnull` tells the nullable analyzer that the type argument must not be a nullable type — neither a nullable reference type (`string?`) nor a nullable value type (`int?`). Inside the method body the compiler allows using `T` without null-dereference warnings because no nullable substitution is valid. It is purely an annotation contract enforced by the analyzer, not a runtime restriction, so it does not affect boxing or execution. When nullable reference types are enabled, an unconstrained `T` can be substituted with `T?` in some contexts, so `notnull` is the way to express "this must be a non-null type parameter" without forcing `T` to be specifically a class or a struct.

---

## Q15. Why are generic value types separate closed types at runtime for static fields?

**Concepts**
- per-construction static field slots
- CLR closed-type identity
- value-type JIT specialization
- reference-type code sharing
- static field multiplication

**Answer**

For each unique value-type substitution of a generic type the CLR JIT-compiles separate native code and allocates a separate set of static fields, because `List<int>` and `List<double>` are genuinely distinct types with distinct memory layouts. This means `Cache<int>.Slot` and `Cache<double>.Slot` are different memory locations — incrementing one does not affect the other. Reference-type substitutions share a single compiled representation, so `Cache<string>.Slot` and `Cache<object>.Slot` happen to share the same static storage. The practical consequence is that a static counter or singleton inside a generic class multiplies by the number of distinct value-type arguments used, which can surprise developers who expect one shared global but get per-type isolation instead.

---

## Q16. Covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?

**Concepts**
- out keyword on interface type parameter
- read-only output position
- type hierarchy widening
- IEnumerable<out T> declaration
- safety rationale

**Answer**

`IEnumerable<T>` is declared `IEnumerable<out T>` — the `out` keyword marks `T` as covariant, meaning a more derived type can stand in where a less derived type is expected. Assigning `IEnumerable<string>` to `IEnumerable<object>` is safe because you only ever read elements out of the sequence and every `string` is-a `object`, so no type invariant is violated. The `out` keyword enforces this safety by prohibiting `T` from appearing in any input position on the interface — if you could write through `IEnumerable<T>`, you could attempt to insert a non-string via the `IEnumerable<object>` reference, but since the interface has no write operations that constraint is met and covariance is sound.

---

## Q17. Contravariance on `Action<in T>` / `IComparer<in T>`

**Concepts**
- in keyword on delegate/interface
- input-only position restriction
- consumer widening
- Action<in T> assignability
- IComparer<in T> use

**Answer**

Contravariance, declared with `in T`, lets you substitute a more general type where a more specific one is expected, as long as the type parameter only appears in input (consumer) positions. An `Action<object>` — a callback that handles any object — can safely be assigned to an `Action<string>` variable because when the caller passes a `string`, the handler receives it as `object`, which is valid. Similarly, `IComparer<object>` can serve as `IComparer<string>` because a comparison method that orders objects certainly works on strings. The `in` keyword makes the compiler verify that `T` never appears in an output position on the interface or delegate, which is what makes the substitution sound — you can always use a more general consumer where a specific one is expected.

---

## Q18. Why is `List<T>` neither covariant nor contravariant on `T`?

**Concepts**
- invariance
- T in both input and output positions
- covariance unsoundness for mutable types
- class type parameter restriction
- IEnumerable<out T> workaround

**Answer**

`List<T>` is invariant because `T` appears in both output positions (the indexer getter, `ToArray`) and input positions (the indexer setter, `Add`, `Insert`). Variance requires `T` to appear exclusively in one kind of position, so `List<T>` cannot be declared covariant or contravariant. If it were covariant and `List<string>` were assignable to `List<object>`, you could call `Add(42)` through the `List<object>` reference, which would corrupt the `string[]` backing array. Generic class types are invariant by default in C#; variance declarations are only supported on interfaces and delegates where the compiler can verify the in/out position rule. To get covariant behavior from a list, cast to the covariant interface `IEnumerable<out T>` or `IReadOnlyList<out T>`.

---

## Q19. Generic specialization performance: value types vs reference types

**Concepts**
- per-value-type JIT machine code
- shared reference-type code
- unboxed value-type storage
- pointer-width uniformity for references
- cache locality benefit

**Answer**

The JIT compiles a distinct native code body for each unique value-type substitution of a generic method or type, so `List<int>.Add` operates directly on an `int[]` with integer-sized reads and no per-element heap allocation, while `List<double>.Add` gets its own code path for double-array manipulation. All reference-type substitutions share a single compiled method because every reference is pointer-sized, so `List<string>` and `List<Product>` reuse the same native code with pointer manipulation. The practical outcome is that generic value-type code achieves optimal unboxed performance with cache-friendly contiguous memory layout, while reference-type generics benefit from code sharing at the cost of not being able to make value-type-specific layout optimizations.

---

## Q20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?

**Concepts**
- List<T> invariance
- InvalidCastException at runtime
- explicit cast behavior
- IEnumerable<object> as covariant workaround
- compiler warning vs runtime failure

**Answer**

A direct cast `(List<object>)stringList` fails at runtime with `InvalidCastException` because `List<T>` is invariant — the runtime verifies that the object's actual type is compatible with `List<object>`, and a `List<string>` is not. Depending on how the cast is written, the compiler may or may not warn at compile time; an explicit cast always compiles but always throws at runtime for this particular pair. If you need to consume strings as objects you should cast to the covariant interface `IEnumerable<object>` instead, which succeeds because `IEnumerable<out T>` is covariant and `List<string>` implements `IEnumerable<string>`, which is assignable to `IEnumerable<object>`.

---

### 02. ArrayList

---

## Q21. Difference between `Array` and `ArrayList`

**Concepts**
- fixed-length array
- dynamically resizable ArrayList
- typed vs object[] storage
- boxing for value types in ArrayList
- List<T> as modern replacement

**Answer**

An `Array` has a fixed length set at creation and stores elements of a single declared type directly — `int[]` stores unboxed integers contiguously and the length cannot grow after allocation. `ArrayList` is a dynamically resizable collection that stores every element as `object`, automatically doubling its internal array when capacity is exceeded, so value types are boxed on insert and the collection can grow indefinitely. Both support indexed access, but `Array` gives compile-time type guarantees and avoids boxing for value types, while `ArrayList` trades those guarantees for dynamic resizing and heterogeneous storage. In modern C#, `List<T>` replaces both for most use cases because it combines `ArrayList`'s dynamic resizing with `Array`'s type safety and avoids boxing.

---

## Q22. Difference between `List<T>` and `ArrayList`

**Concepts**
- generic type safety
- boxing elimination in List<T>
- ArrayList object[] backing
- compile-time rejection of wrong types
- performance difference for value types

**Answer**

`List<T>` stores elements in a typed `T[]` backing array so value types are never boxed and the compiler rejects `Add` calls with the wrong element type at compile time. `ArrayList` stores elements in an `object[]`, boxing every value type on insert and requiring an explicit cast on read — both are runtime operations with no compiler check. Adding 50,000 integers to an `ArrayList` creates 50,000 heap box objects that the GC must collect, while `List<int>` stores them directly in contiguous memory with zero extra allocations. Since `ArrayList` provides no safety or performance advantage over `List<T>` in any scenario on modern .NET, it is considered legacy and should not appear in new code.

---

## Q23. Why is `ArrayList` considered a legacy collection in modern C#?

**Concepts**
- pre-generics history (.NET 1.x)
- List<T> as full replacement
- no remaining advantage
- migration target
- .NET Framework maintenance context

**Answer**

`ArrayList` predates generics, which were introduced in .NET 2.0, and was the only dynamic list available in .NET 1.x. Once `List<T>` shipped it became obsolete because `List<T>` is faster for value types (no boxing), safer for all types (compile-time checking), and equally convenient for everything else. There is no use case today where `ArrayList` outperforms or outclasses `List<T>`. The only reason to encounter `ArrayList` is maintaining .NET Framework code written before 2005, reading COM interop output, or working with legacy reflection APIs — all of which are migration targets rather than designs to emulate.

---

## Q24. What boxing occurs when storing `int` values in an `ArrayList`?

**Concepts**
- boxing mechanism
- heap allocation per value type
- object[] backing array
- GC pressure from box objects
- unboxing on read

**Answer**

Each call to `ArrayList.Add(someInt)` causes the runtime to allocate a small heap object, copy the integer value into it, and store a reference to that wrapper in the internal `object[]`. This is boxing — wrapping the value type in a reference-type shell so it can be stored as `object`. Reading the integer back requires unboxing: dereferencing the heap object and copying the integer out, combined with an explicit cast to confirm the stored type. For a collection of N integers, this boxing produces N heap allocations that the garbage collector must reclaim, generating Gen0 pressure proportional to the number of insertions. `List<int>` avoids this entirely by storing integers directly in an `int[]` with no wrapper objects.

---

## Q25. Performance cost of repeated boxing/unboxing in hot loops using `ArrayList`

**Concepts**
- allocation rate in hot loops
- Gen0 GC collection frequency
- cache locality loss
- CPU overhead of indirect access
- benchmark magnitude

**Answer**

Each box allocation in a hot loop adds a heap object of the value type size plus object overhead (typically 16–24 bytes), and the GC must scan and collect all of them. For a loop processing a million integers per second, boxing produces a million heap objects per second, filling Gen0 quickly and triggering frequent collections that pause throughput. The CPU cost compounds because boxed integers are scattered across the heap rather than stored contiguously, destroying the cache-line efficiency that sequential `int[]` access provides in `List<int>`. In benchmarks on .NET 8, `List<int>` for numeric hot loops typically runs several times faster than `ArrayList` with significantly lower memory allocation, which is why the boxing argument alone is sufficient to disqualify `ArrayList` for any value-type-heavy workload.

---

## Q26. Can you store mixed types in an `ArrayList`, and what typing risks does that create?

**Concepts**
- heterogeneous object[] storage
- implicit type contract violation
- InvalidCastException at runtime
- no compile-time rejection
- is/as pattern to mitigate crashes

**Answer**

Yes, `ArrayList.Add` accepts any `object` so you can store strings, integers, custom objects, and nulls in the same collection with no restriction at any point. The typing risk is that any code reading elements must know or guess the type at each index, relying on documentation or an implicit convention rather than a compiler guarantee. A cast like `(Product)list[i]` compiles regardless of what was actually stored, so a teammate adding a string causes an `InvalidCastException` at runtime that only surfaces during the execution path that casts that index. Using `is` and `as` prevents crashes but adds branching overhead and hides the fact that the collection's type contract has already been violated at the insertion site.

---

## Q27. Difference between `ArrayList.Capacity` and `Count`

**Concepts**
- Count as logical element count
- Capacity as backing array size
- doubling growth strategy
- pre-sizing for bulk inserts
- TrimExcess after shrinking

**Answer**

`Count` is the number of elements actually stored and iterable, while `Capacity` is the size of the internal `object[]` backing array — the maximum elements it can hold before needing to resize. When `Count` reaches `Capacity` on an `Add`, `ArrayList` allocates a new array twice the current capacity, copies all existing elements, and discards the old array. Setting `Capacity` explicitly before bulk inserts avoids repeated resize-and-copy cycles; calling `TrimExcess()` after removing many elements reduces `Capacity` back down to `Count`, releasing the oversized backing array. The same `Capacity` / `Count` / `TrimExcess()` pattern applies identically to `List<T>`.

---

## Q28. When might you still encounter `ArrayList` in maintained legacy codebases?

**Concepts**
- .NET Framework 1.x code
- COM interop wrappers
- legacy serializers
- ASP.NET Web Forms / System.Web
- migration debt

**Answer**

`ArrayList` appears in pre-.NET 2.0 code that has never been modernized, particularly in enterprise systems from the mid-2000s that have received patches but not a full rewrite. COM interop wrappers generated from older type libraries sometimes surface `IList` or `ArrayList` at the boundary. Legacy XML and binary serializers used `ArrayList` as the default deserialized collection type, and configuration APIs like `System.Web.Configuration` and older ASP.NET Web Forms code used non-generic collections throughout. In all cases the right maintenance response is to understand and test the existing behavior, then migrate to `List<T>` when the blast radius of the change is acceptable.

---

## Q29. Difference between `ArrayList` and `object[]`

**Concepts**
- fixed-length array vs dynamic resizing
- IList interface on ArrayList
- direct element access parity
- boxing cost equivalence
- allocation overhead comparison

**Answer**

`object[]` is a fixed-length array whose size must be declared upfront; indexed access is O(1) and there is no wrapper overhead. `ArrayList` is a dynamically resizable wrapper around an `object[]` that doubles capacity automatically when full, implements `IList`, and provides `Add`, `Remove`, `Insert`, and `Sort` operations. Both store elements as `object` with the same boxing cost for value types. The practical choice comes down to whether you know the size upfront: if you do, `object[]` is simpler and slightly faster without the wrapper; if the size is unknown and grows at runtime, `ArrayList` handles resizing automatically. In modern code both are replaced by `List<T>`, which combines dynamic resizing with type safety.

---

## Q30. Legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) for maintenance scenarios

**Concepts**
- Hashtable as Dictionary predecessor
- non-generic Queue and Stack
- System.Collections namespace
- boxing for value type keys/values
- generic replacements

**Answer**

`Hashtable` maps `object` keys to `object` values with no type parameters, boxing value-type keys and values and requiring explicit casts on read; it is replaced by `Dictionary<TKey,TValue>`. Non-generic `Queue` and `Stack` store `object` elements with FIFO and LIFO semantics respectively and are replaced by `Queue<T>` and `Stack<T>`. `SortedList` (non-generic) stores key-value pairs sorted by key in parallel `object[]` arrays, replaced by `SortedList<TKey,TValue>` and `SortedDictionary<TKey,TValue>`. For maintenance work the three most important facts are: `Hashtable` throws `ArgumentNullException` for null keys just as `Dictionary` does; cast errors from all of these manifest as `InvalidCastException` at runtime with no compile-time warning; and `Hashtable` is documented as thread-safe for one writer and multiple concurrent readers while `Dictionary` is not thread-safe for any concurrent access.

---

### 03. List

---

## Q31. Internal working and performance of `List<T>` vs `LinkedList<T>`

**Concepts**
- contiguous T[] backing in List<T>
- doubly-linked node heap objects in LinkedList<T>
- O(1) indexed access vs O(n) traversal
- O(n) insertion shift vs O(1) node insertion
- cache locality advantage of List<T>

**Answer**

`List<T>` stores elements in a contiguous `T[]` backing array, giving O(1) indexed access and cache-friendly sequential iteration, but O(n) insertions or removals at arbitrary positions because all subsequent elements must shift to fill or make the gap. `LinkedList<T>` stores each element in a `LinkedListNode<T>` that holds the value and pointers to the previous and next nodes, so inserting or removing at a known node is O(1) with no shifting, but reaching position N requires traversing N nodes from the head. The memory overhead of `LinkedList<T>` is substantial — each node is a separate heap object with two reference pointers — which increases GC pressure and destroys cache locality compared to the contiguous array of `List<T>`. For most workloads where random access and iteration dominate, `List<T>` is faster in practice despite its O(n) insertion cost; `LinkedList<T>` is only preferable when the dominant operation is inserting or removing at known interior positions with many such operations per second.

---

## Q32. What is `LinkedList<T>` and when should it be used?

**Concepts**
- doubly-linked list structure
- LinkedListNode<T> heap object
- O(1) node insertion and removal
- O(n) indexed access
- appropriate use cases

**Answer**

`LinkedList<T>` is a doubly-linked list where each element is wrapped in a `LinkedListNode<T>` referencing the previous and next nodes, so inserting or removing a node you already have a reference to is O(1) — no shifting required. Finding a node by value or position still requires linear traversal since there is no backing array. Use `LinkedList<T>` when the primary operations are inserting or removing elements at arbitrary known positions frequently, such as an LRU cache eviction list, a scheduler that promotes or demotes items between priority slots, or a token stream where tokens are inserted and removed during transformation. For collections where reads, appends, and iteration dominate, `List<T>` is the better choice because its cache-friendly memory layout makes sequential access and indexing significantly faster in practice.

---

## Q33. Difference between `List<T>.Sort()` stability and `OrderBy()` stability

**Concepts**
- unstable sort in List<T>.Sort (introspective sort)
- stable sort in LINQ OrderBy (merge sort)
- equal-element relative ordering
- in-place vs new sequence
- multi-key sort implications

**Answer**

`List<T>.Sort()` uses an introspective sort (a combination of quicksort, heapsort, and insertion sort) which is unstable, meaning equal elements may appear in any order relative to their original positions after sorting. `Enumerable.OrderBy()` uses a stable merge sort that preserves the relative order of elements that compare as equal, so if two `Product` records have the same price the one that appeared first in the original sequence still appears first after `OrderBy(p => p.Price)`. The practical choice is: use `Sort()` when the comparison produces a total ordering with no ties, since it sorts in place with slightly lower overhead; use `OrderBy().ThenBy()` for multi-key sorts or user-visible lists where equal elements need to maintain input order, since stability is observable there.

---

## Q34. How does `List<T>` grow its internal buffer when capacity is exceeded?

**Concepts**
- doubling growth strategy
- amortized O(1) Add
- backing array copy on resize
- initial capacity hint
- LOH pressure from large arrays

**Answer**

When `Count` equals `Capacity` and another element is added, `List<T>` allocates a new backing array of twice the current capacity (or 4 if capacity was 0), copies all existing elements into it, and releases the old array for GC. The doubling strategy ensures the amortized cost of N appends is O(N) — each element is copied on average once across all resizes. Without a capacity hint, a list that grows to 500,000 elements goes through roughly 17 resize-and-copy cycles, allocating and discarding arrays of size 4, 8, 16, up to the final size. Providing `new List<T>(expectedCount)` eliminates these intermediate allocations, which matters both for throughput and for avoiding Large Object Heap pressure when the intermediate arrays exceed the ~85KB LOH threshold.

---

## Q35. Amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle

**Concepts**
- amortized O(1) tail append
- O(n) element shift on Insert
- O(n²) total cost for N front-insertions
- queue-as-list anti-pattern
- LinkedList or Queue as alternatives

**Answer**

`List<T>.Add` appends at the end — it writes to the next available slot and increments `Count` — with an amortized O(1) cost because resizes are infrequent on average. `Insert(0, item)` at the beginning is O(n) because every existing element must shift right by one position to make room; `Insert(middle, item)` shifts all elements from that index to the end. Using a list as a queue by repeatedly calling `Insert(0, ...)` for 50,000 elements is O(n²) total — a common performance pitfall. If frequent insertions at arbitrary positions are required, prefer `LinkedList<T>` for known-node O(1) insertions, or restructure the algorithm to append and then sort, or use a `Queue<T>` for the prepend-and-drain pattern.

---

## Q36. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?

**Concepts**
- ReadOnlyCollection<T> wrapper
- live view semantics
- NotSupportedException on mutation attempt
- underlying list still mutable
- ToArray/ToList for true snapshot

**Answer**

`AsReadOnly()` returns a `ReadOnlyCollection<T>` that wraps the original list and throws `NotSupportedException` if a caller tries to call `Add`, `Remove`, or `Clear` through the wrapper. However it is a live view — changes to the underlying `List<T>` are immediately visible through the read-only wrapper because it holds a reference to the same backing array, not a copy. Exposing `_list.AsReadOnly()` as a property prevents external callers from mutating through the returned reference, but any code that still holds the original `List<T>` reference can still modify it and the wrapper reflects those changes. For a true snapshot that external mutation cannot affect, return `_list.ToArray()` or `_list.ToList()` — the copy is more expensive but independent.

---

## Q37. Difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list

**Concepts**
- ConvertAll eager projection to new List
- ForEach side-effect iteration (void)
- LINQ Select deferred execution
- eager vs lazy evaluation
- allocation behavior

**Answer**

`List<T>.ConvertAll(converter)` eagerly produces a new `List<TOutput>` by applying the converter to every element, materializing immediately as a fresh list allocation. `List<T>.ForEach(action)` iterates the list and invokes the action for its side effects, returning nothing — it is a void loop, not a transformation. LINQ `Select(selector)` returns a lazy `IEnumerable<TResult>` that applies the selector only when enumerated, deferring execution until a terminal operation like `ToList()`, `Count()`, or `foreach` forces it. The practical choice: use `ConvertAll` when you want a `List<T>` result and the source is already a list; use `ForEach` for side effects where the loop body is brief; use `Select` when composing with other LINQ operators or when the result should stay lazy for a single-pass consumer.

---

## Q38. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?

**Concepts**
- ToArray always allocates new T[]
- CopyTo writes to caller-provided array
- GetRange returns new List<T> slice
- allocation trade-offs
- CollectionsMarshal.AsSpan zero-copy alternative

**Answer**

`ToArray()` allocates and returns a new `T[]` containing all elements — always a fresh allocation sized to `Count`. `CopyTo(array, arrayIndex)` writes elements into a caller-provided array that must already be large enough; it does not allocate a new array but writes into the buffer you supply, making it useful when you already own the destination memory. `GetRange(index, count)` returns a new `List<T>` containing a shallow copy of the specified slice — another allocation. So `ToArray` and `GetRange` always allocate; `CopyTo` does not. In performance-sensitive paths that repeatedly need a span over the list's contents, `CollectionsMarshal.AsSpan(list)` provides a zero-copy `Span<T>` over the backing array, avoiding all three.

---

## Q39. When to expose `List<T>` vs `IReadOnlyList<T>` or `IEnumerable<T>` as a return type

**Concepts**
- return type API surface design
- IReadOnlyList<T> for indexed read-only result
- IEnumerable<T> for lazy or single-pass results
- List<T> when caller legitimately needs mutation
- encapsulation of internal state

**Answer**

Returning `List<T>` exposes the full mutable API — callers can `Add`, `Remove`, `Sort`, and replace the contents — which breaks encapsulation if the list is also owned internally. Return `IReadOnlyList<T>` when the result is already materialized and callers need indexed access (`Count`, `[i]`) but should not mutate it; this is the correct default for service methods that return computed results. Return `IEnumerable<T>` when the result may be lazy, the caller only needs a single forward pass, or you want to retain freedom to change the underlying storage type without breaking callers. Return `List<T>` only when callers legitimately need to add to or sort the returned collection, such as a builder method that hands back a working list the caller is expected to populate further.

---

## Q40. Difference between `List<T>.Capacity` and `Count`

**Concepts**
- Count as logical element count
- Capacity as physical backing array size
- doubling on overflow
- pre-sizing avoids resize storms
- TrimExcess after shrinking

**Answer**

`Count` is the number of elements currently in the list — what you iterate and what indices 0 through `Count-1` address. `Capacity` is the size of the underlying backing array — the maximum elements it can hold before triggering a resize. When you add the element that would make `Count` exceed `Capacity`, the list doubles its capacity, copies all `Count` elements into the new array, and the old array becomes garbage. Setting `Capacity` before bulk inserts avoids these intermediate copies; after removing many elements, `TrimExcess()` sets `Capacity` to match `Count` (if they differ by more than 10%) to release the oversized array back to the heap.

---

## Q41. What happens if you mutate a list while iterating with `foreach`?

**Concepts**
- version counter in List<T>
- InvalidOperationException on structural change
- enumerator validation on MoveNext
- reverse for-loop workaround
- RemoveAll as safe alternative

**Answer**

`List<T>` maintains an internal version counter that increments on any structural modification — `Add`, `Remove`, `Insert`, `Clear`, `Sort`. The enumerator captures the version at the start of `foreach` and checks it on every `MoveNext` call; if the version has changed, it throws `InvalidOperationException: "Collection was modified; enumeration operation may not execute."` This detection is intentional because allowing mutation mid-enumeration would cause skipped elements or infinite loops as indices shift. Safe alternatives are iterating a copy (`foreach (var x in list.ToList())`), using a reverse `for` loop with `RemoveAt(i)` for in-place removal, or using `list.RemoveAll(predicate)` for bulk removal in a single pass without any enumerator.

---

## Q42. What is `TrimExcess`, and when is it useful?

**Concepts**
- capacity shrinkage after element removal
- memory reclamation
- build-then-shrink pattern
- reallocation cost trade-off
- long-lived list optimization

**Answer**

`TrimExcess()` sets the list's `Capacity` to equal its `Count`, reallocating a smaller backing array and releasing the excess memory. It is useful after a build-and-shrink pattern — loading a large collection and then removing many elements — because without it a list that grew to 100,000 elements and had 90,000 removed still holds a 100,000-slot array. The trade-off is that `TrimExcess()` triggers an allocation and element copy, so it is only worthwhile when the shrunken list will live long enough that the memory savings justify the one-time copy cost. For short-lived local variables or collections that will be rebuilt shortly, calling `TrimExcess()` adds work without lasting benefit.

---

## Q43. Binary search on a list (`BinarySearch`) — what precondition must the list satisfy?

**Concepts**
- O(log n) search algorithm
- sorted precondition
- IComparable<T> default comparison
- custom IComparer<T> overload
- negative return on miss (bitwise complement)

**Answer**

`List<T>.BinarySearch(item)` performs an O(log n) search by repeatedly halving the search range and comparing the target to the midpoint element. The precondition is that the list must be sorted in the same order used by the comparison — if the list is unsorted, `BinarySearch` produces unreliable results and may miss elements that exist. It returns the zero-based index of the found element, or if not found the bitwise complement of the index where the element would be inserted (`~result` is a positive number usable for sorted insertions). You can pass a custom `IComparer<T>` to both `Sort` and `BinarySearch` to ensure both operations use the same ordering rule, which is essential for correctness.

---

## Q44. `List<T>` indexer access compared to `LinkedList<T>` (no indexer)

**Concepts**
- O(1) array indexer in List<T>
- O(n) traversal in LinkedList<T>
- cache locality of contiguous array
- IList<T> interface provision
- linked node pointer indirection

**Answer**

`List<T>[i]` is O(1) — a direct array element access at a known memory offset, and adjacent elements are contiguous in memory so a single cache miss often loads several subsequent elements into the cache line. `LinkedList<T>` has no indexer because finding the element at position i requires traversing from the head node through i `Next` pointer dereferences, which is O(n) and cache-hostile since each node is a separate heap object potentially scattered in memory. This is the decisive reason `List<T>` dominates for random-access patterns: the contiguous array layout makes both indexed and sequential access fast, while LinkedList causes a cache miss on every node hop. If you need index-by-position, `List<T>` is always the correct choice over `LinkedList<T>`.

---

## Q45. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?

**Concepts**
- Comparison<T> delegate signature
- Sort(Comparison<T>) overload
- lambda sort expression
- IComparer<T> alternative
- when to prefer each

**Answer**

`Comparison<T>` is the delegate type `delegate int Comparison<T>(T x, T y)` — it takes two values and returns negative if x should come before y, zero if equal, and positive if x should come after y. `List<T>.Sort(Comparison<T>)` accepts this delegate directly, letting you write inline sort logic as a lambda without implementing a comparer class: `list.Sort((a, b) => a.Price.CompareTo(b.Price))`. The `IComparer<T>` overload is preferable when the comparison logic is reused across multiple call sites or needs to be injected and tested independently; the `Comparison<T>` lambda is preferable for one-off sorts at a specific call site where the intent is clear from the lambda body.

---

### 04. Dictionary

---

## Q46. Difference between `Dictionary<TKey, TValue>` and `Hashtable`

**Concepts**
- generic type safety vs object storage
- boxing elimination for value-type keys/values
- thread-safety difference
- null key behavior difference
- API evolution

**Answer**

`Hashtable` stores both keys and values as `object`, boxing value types and requiring explicit casts on read with no compile-time type checking. `Dictionary<TKey,TValue>` parameterizes both types, preventing wrong-type inserts at compile time and avoiding boxing for value-type keys and values like `Dictionary<int, decimal>`. Both use hash table internals with average O(1) lookup, insert, and delete. `Hashtable` is documented as thread-safe for one writer and multiple concurrent readers simultaneously, while `Dictionary<TKey,TValue>` is not thread-safe for any concurrent access. `Hashtable` allows null keys while `Dictionary` throws `ArgumentNullException` for null reference-type keys. For all new code, use `Dictionary<TKey,TValue>` or `ConcurrentDictionary<TKey,TValue>` for concurrent scenarios.

---

## Q47. `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`

**Concepts**
- IDictionary<K,V> mutable interface
- IReadOnlyDictionary<K,V> read-only interface
- encapsulation via return type
- implementation substitution
- testing with dictionary literals

**Answer**

`IDictionary<TKey,TValue>` is the mutable dictionary interface exposing `Add`, `Remove`, the read-write indexer, `ContainsKey`, `TryGetValue`, `Keys`, and `Values`. Accepting `IDictionary<TKey,TValue>` in a method parameter allows callers to pass `Dictionary`, `SortedDictionary`, or any custom implementation without coupling to a concrete type. `IReadOnlyDictionary<TKey,TValue>` exposes only read operations — read-only indexer, no `Add` or `Remove` — and is the correct return type for a service method that exposes a lookup map without permitting external mutation. Both interfaces enable swapping implementations in tests by passing a hand-filled dictionary in place of one backed by a real data source.

---

## Q48. How does `Dictionary<TKey, TValue>` handle hashing and collisions?

**Concepts**
- GetHashCode bucket mapping
- collision chaining in entry arrays
- Equals comparison in bucket walk
- load factor and rehashing
- O(n) worst case with poor hash

**Answer**

`Dictionary<TKey,TValue>` computes `GetHashCode()` on the key, maps the result to a bucket index via modulo arithmetic, and stores the entry in that bucket. When two keys hash to the same bucket (a collision), .NET's implementation uses a chained structure where entries in the same bucket are linked via a `next` field in the internal entries array — the bucket effectively heads a short linked list. On lookup the dictionary walks all entries in the matching bucket comparing keys with `Equals` until it finds a match or exhausts the chain. When the ratio of entries to buckets exceeds a load threshold, the dictionary rehashes by allocating a larger bucket array and redistributing all entries. A hash function that always returns the same value degrades all operations to O(n) because every entry lands in one chain.

---

## Q49. Difference between `Dictionary.Add` and the indexer when the key already exists

**Concepts**
- Add throws ArgumentException on duplicate
- indexer silently overwrites on duplicate
- upsert semantics
- explicit duplicate detection
- initialization vs update patterns

**Answer**

`Dictionary.Add(key, value)` throws `ArgumentException` if the key already exists, enforcing the invariant that each key appears at most once and failing loudly when that is violated. The indexer assignment `dict[key] = value` silently replaces the existing value if the key is present, or adds a new entry if it is absent — an "upsert" that never throws for duplicate keys. Use `Add` when a duplicate key is a programming error you want detected immediately; use the indexer when your intent is "set this key to this value regardless of whether it was set before". Mixing them inadvertently is a common source of bugs: `Add` in an initialization loop that can run twice causes exceptions, while the indexer in a uniqueness-critical log causes silent overwrites.

---

## Q50. Difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup

**Concepts**
- double hash lookup waste in ContainsKey + indexer
- TryGetValue single atomic lookup
- KeyNotFoundException from bare indexer
- idiomatic C# lookup pattern
- performance on hot paths

**Answer**

`ContainsKey(key)` performs one hash lookup returning a bool; using the indexer `dict[key]` immediately after performs a second hash lookup to retrieve the value — two hash computations for what should be one operation. `TryGetValue(key, out value)` performs a single hash lookup and returns both the bool result and the value atomically, which is the idiomatic pattern for "get if present" in C#. The bare indexer `dict[key]` throws `KeyNotFoundException` when the key is absent, which is appropriate only when a missing key is a genuine invariant violation that should propagate as an unhandled exception. In any code path where key absence is an expected possibility, prefer `TryGetValue` over the double-lookup pattern.

---

## Q51. Why must keys be immutable (or stable) after insertion?

**Concepts**
- hash code computed at insertion time
- wrong-bucket orphaned entry
- silent lookup failure after mutation
- remove-reinsert pattern for key changes
- readonly record struct as safe key

**Answer**

When you insert a key, the dictionary computes `GetHashCode()` and places the entry in the bucket corresponding to that hash. If you later mutate a property that participates in `GetHashCode`, the key's hash value changes but the entry remains in the bucket keyed to the old hash, so any subsequent lookup using the new hash value goes to a different bucket and finds nothing — the entry is technically in the dictionary but permanently unreachable through normal lookup. This causes silent failures where `ContainsKey` returns false and `TryGetValue` returns nothing for a key that is present. The fix is to use immutable keys — `string`, `int`, `readonly record struct`, or sealed classes with constructor-only identity fields — and if a key must change, remove the entry before mutating, then re-insert with the new key value.

---

## Q52. What exception is thrown when accessing a missing key via the indexer?

**Concepts**
- KeyNotFoundException
- indexer exception vs ArgumentException
- TryGetValue as non-throwing alternative
- exception-driven control flow cost
- defensive coding pattern

**Answer**

Accessing `dict[key]` when the key is not present throws `KeyNotFoundException` with a message indicating the key was not found. Unlike `ArgumentException` thrown by `Add` on a duplicate key, `KeyNotFoundException` signals that expected data was absent at runtime rather than a programming error — it often indicates a data synchronization issue, a stale reference, or a missing configuration entry. Because catching `KeyNotFoundException` for control flow is expensive and obscures intent, prefer `TryGetValue` when key absence is an expected possibility, and use the indexer without a catch only when a missing key represents a genuine invariant violation that should propagate as an unhandled exception.

---

## Q53. Can `null` be used as a key when `TKey` is a reference type?

**Concepts**
- ArgumentNullException for null keys in Dictionary
- Hashtable null key allowance
- default equality comparer limitation
- sentinel value workaround
- custom IEqualityComparer for null handling

**Answer**

`Dictionary<TKey,TValue>` throws `ArgumentNullException` when you pass null as a key to `Add`, the indexer, `ContainsKey`, `TryGetValue`, or `Remove` when `TKey` is a reference type, because null cannot have a meaningful hash code computed by the default equality comparer. `Hashtable` is the only built-in .NET map that accepts null as a key by special-casing it to a dedicated slot. If you genuinely need null-as-key semantics in a generic dictionary, one approach is to use a non-null sentinel value as a proxy for null (an empty string, a special static singleton). A custom `IEqualityComparer<TKey>` that explicitly handles null can also be passed to the dictionary constructor to enable null key support.

---

## Q54. Average vs worst-case time complexity for lookup, insert, and remove

**Concepts**
- average O(1) with good hash distribution
- worst-case O(n) with constant hash
- amortized rehash cost
- load factor threshold
- practical performance guarantee

**Answer**

With a good hash function that distributes keys evenly across buckets, lookup, insert, and remove are all O(1) average — the hash maps directly to a bucket with zero or very few collisions so the entry is found in a constant number of comparisons. The worst case for all three operations is O(n) when a poor hash function maps every key to the same bucket, degrading the dictionary to a linear scan of one long chain — a constant `GetHashCode` that always returns 0 produces this worst case. Rehashing when the load factor threshold is exceeded costs O(n) as a one-time operation but amortizes to O(1) per insertion across the sequence of insertions, similar to `List<T>.Add` amortized growth. The practical guarantee is effectively O(1) for `string` and primitive keys because their hash functions distribute well.

---

## Q55. Hash code contract between `GetHashCode` and `Equals` for custom key types

**Concepts**
- equal objects must produce equal hash codes
- unequal objects may share hash codes (collision)
- contract violation causes silent lookup failures
- overriding both Equals and GetHashCode
- record automatic consistent implementation

**Answer**

The contract requires that if `a.Equals(b)` is true then `a.GetHashCode() == b.GetHashCode()` must also be true — equal objects must hash to the same bucket so lookups can find them. The reverse is not required: two unequal objects can share the same hash code as a collision. Violating the forward direction causes the dictionary to look in the wrong bucket and never find a key even when it is present. The most common violation is overriding `Equals` to compare by field values but forgetting to override `GetHashCode`, which inherits reference-based hashing from `object` and produces different hash codes for two objects that `Equals` considers identical. Records automatically implement both methods consistently based on all primary constructor parameters, which is why they are the safest choice for value-equality key types.

---

## Q56. `IEqualityComparer<TKey>` — when do you pass a custom comparer to the constructor?

**Concepts**
- external equality definition
- IEqualityComparer<T> interface
- case-insensitive string keys
- domain-specific identity
- comparer injected for testing

**Answer**

`IEqualityComparer<TKey>` defines `Equals(x, y)` and `GetHashCode(x)` externally to the key type, letting you specify how equality and hashing work for dictionary lookup without modifying the key class. Pass one to the dictionary constructor when the default equality is wrong for your use case — case-insensitive string keys with `StringComparer.OrdinalIgnoreCase`, domain identity different from reference equality such as matching subscribers by email field regardless of object instance, or when the key type is from a library you cannot modify. The passed comparer is used for every `Add`, lookup, and remove operation throughout the dictionary's lifetime, so it must produce consistent results for the same key values.

---

## Q57. When to choose `Dictionary` over `List` for lookups by id or SKU

**Concepts**
- O(1) dictionary key lookup
- O(n) list linear scan
- memory overhead trade-off
- key-based vs position-based access
- dual data structure pattern

**Answer**

Use `Dictionary<TKey,TValue>` when the primary operation is looking up items by a key such as id, SKU, or username and you perform many lookups over the collection's lifetime. A `List<Product>` with 10,000 items requires scanning up to 10,000 elements to find one by SKU, while `Dictionary<string,Product>` finds it in O(1) average. The trade-off is that `Dictionary` uses more memory for hash buckets and entry metadata and has no natural ordering. Use `List<T>` when you primarily iterate all elements in sequence, need access by position index, or have a small enough collection that the overhead of building and maintaining a dictionary outweighs the scan cost. A common pattern is maintaining both: a `List<T>` for ordered iteration and a parallel `Dictionary<string,T>` for key-based lookup.

---

## Q58. What happens internally when two keys hash to the same bucket?

**Concepts**
- bucket chaining in entries array
- next field linking colliding entries
- Equals comparison in chain walk
- rehashing on load factor threshold
- O(n) worst case with all collisions

**Answer**

In .NET's `Dictionary<TKey,TValue>` implementation each bucket holds an index into an internal `entries` array. When two keys hash to the same bucket index, the second entry is linked to the first via a `next` field in the entries array — the bucket effectively heads a singly-linked chain stored inline within the entries array. On lookup the dictionary computes the hash, finds the bucket index, then walks the chain comparing each entry's key with `Equals` until a match is found or the chain is exhausted. With a good hash function chains stay length 0 or 1 almost always, preserving O(1) average lookup. When the number of entries exceeds the load factor threshold the dictionary rehashes to a larger bucket array, redistributing all entries and reducing average chain length back to near 1.

---

### 05. HashSet

---

## Q59. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?

**Concepts**
- O(1) average membership test
- automatic duplicate rejection
- unordered storage
- ISet<T> interface
- hash table backing

**Answer**

`HashSet<T>` is an unordered collection that stores unique elements using a hash table, giving O(1) average-case `Contains`, `Add`, and `Remove`. It differs from `List<T>` in three ways: duplicate elements are silently rejected (`Add` returns `false` rather than inserting), there is no guaranteed enumeration order, and there is no indexer for position-based access. Use `HashSet<T>` when the primary operations are membership testing ("is this SKU already processed?"), deduplicating a stream, or performing set algebra — all three rely on the O(1) hash lookup that `List<T>` cannot provide because `List.Contains` is O(n).

---

## Q60. Difference between `SortedSet<T>` and `HashSet<T>`

**Concepts**
- SortedSet red-black tree vs HashSet hash table
- O(log n) vs O(1) operations
- IComparer<T> vs IEqualityComparer<T>
- sorted enumeration and range queries
- Min/Max members

**Answer**

`HashSet<T>` stores elements in a hash table with no ordering guarantee, giving O(1) average add/remove/contains. `SortedSet<T>` stores elements in a red-black tree sorted by `IComparer<T>`, giving O(log n) for the same operations but with elements always in sorted order on enumeration. `SortedSet<T>` also exposes `Min`, `Max`, `GetViewBetween(low, high)`, and `Reverse()` which are impossible on an unordered set. Use `SortedSet<T>` when you need both uniqueness and sorted iteration or range queries; use `HashSet<T>` when only fast membership and deduplication matter and order is irrelevant.

---

## Q61. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?

**Concepts**
- hash bucket placement by GetHashCode
- Equals for collision resolution in bucket walk
- silent failure on contract violation
- equal objects must produce equal hash codes
- overriding both methods together

**Answer**

`HashSet<T>` places each element in a bucket determined by `GetHashCode()`, then uses `Equals` to distinguish elements within the same bucket. If two logically equal objects return different hash codes, the second one lands in a different bucket and `Contains` returns false even though a matching element exists — deduplication silently fails and both objects end up stored. The most common violation is overriding `Equals` to compare by field values without overriding `GetHashCode`, which inherits reference-based hashing from `object` and produces different hash codes for two instances that `Equals` considers identical. Records automatically implement both methods consistently, which is why they are the safest key/element types for hash-based collections.

---

## Q62. Set operations: `UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`

**Concepts**
- mutating in-place semantics
- UnionWith adds from other
- IntersectWith keeps only common elements
- ExceptWith removes what is in other
- SymmetricExceptWith keeps only non-overlapping elements

**Answer**

All four methods mutate the receiver in place. `UnionWith(other)` adds every element from `other` not already present. `IntersectWith(other)` removes elements not present in `other`, keeping only the intersection. `ExceptWith(other)` removes elements that appear in `other`, leaving the set difference. `SymmetricExceptWith(other)` removes elements present in both and adds elements present only in `other`, keeping non-overlapping elements from either side. Since all four modify the receiver, always operate on a copy when the original set must remain unchanged — use LINQ `Union`, `Intersect`, and `Except` for non-mutating equivalents, passing the result to a new `HashSet<T>` constructor with the correct comparer.

---

## Q63. `Add` returning `false` on duplicate vs `List.Add` behavior

**Concepts**
- HashSet.Add return value as duplicate signal
- List.Add unconditional append (void)
- combined add-if-absent semantics
- deduplication without pre-check
- TryAdd pattern

**Answer**

`HashSet<T>.Add(item)` returns `true` if the element was inserted and `false` if it already existed, making the return value a meaningful duplicate signal — the method serves as a combined "add if not present" in one call. `List<T>.Add(item)` always appends unconditionally and returns `void`; it has no concept of duplicates since a list allows the same value at multiple positions. This means using `HashSet<T>` for deduplication requires no separate `Contains` check before `Add`, which eliminates a double-lookup and makes the intent clear. For a `List<T>`, you must call `Contains` first, adding O(n) cost per insertion.

---

## Q64. Constructing `HashSet<T>` with custom equality (`IEqualityComparer<T>`)

**Concepts**
- IEqualityComparer<T> constructor parameter
- StringComparer built-in comparers
- comparer governs all operations for lifetime
- LINQ set ops require separate comparer pass-through
- external equality without modifying key type

**Answer**

Pass an `IEqualityComparer<T>` to the `HashSet<T>` constructor: `new HashSet<string>(StringComparer.OrdinalIgnoreCase)` creates a case-insensitive set. The comparer governs all `Add`, `Contains`, and `Remove` operations for the lifetime of the set. When using LINQ set operators like `Union`, `Intersect`, or `Except` on the results of a `HashSet<T>` with a custom comparer, pass the same comparer to the final `HashSet<T>` constructor wrapping the result — LINQ accepts `IEqualityComparer<T>` as an optional parameter but the existing set's comparer does not flow through automatically.

---

## Q65. `HashSet<T>` for deduplication vs `Distinct()` in LINQ

**Concepts**
- HashSet for explicit set ownership
- Distinct for pipeline deduplication
- materialized set for multi-pass use
- comparer specification in both
- incremental membership tracking

**Answer**

Use `HashSet<T>` when you need explicit ownership of a deduplicated collection that will be queried or mutated repeatedly, when elements arrive incrementally and each must be checked as it arrives, or when you need set algebra operations. Use LINQ `Distinct()` when deduplication is one step in a larger pipeline consumed once, since `Distinct` is deferred and composes naturally into a query chain. Both accept a custom comparer — `Distinct(comparer)` and `new HashSet<T>(sequence, comparer)` — but materializing into a `HashSet<T>` is the right choice when the deduplicated collection will be used as a lookup set later in the same request.

---

## Q66. Set membership test in `HashSet` vs scanning a `List`

**Concepts**
- O(1) HashSet.Contains
- O(n) List.Contains linear scan
- nested loop O(n²) hazard
- warm-up cost for HashSet construction
- break-even point

**Answer**

`HashSet<T>.Contains(item)` is O(1) average — it computes the hash, finds the bucket, and walks a typically zero or one-element chain. `List<T>.Contains(item)` is O(n) — it scans every element from index 0 until it finds a match or exhausts the list. For a single lookup on a short list the difference is negligible, but for repeated membership tests in an outer loop checking each incoming item against a 50,000-element seen-set, the total cost is O(n²) for a list versus O(n) for a hash set. The break-even point is roughly 10–20 elements — below that a list's simple sequential scan is competitive because it avoids hash computation overhead.

---

## Q67. `IsSubsetOf`, `IsSupersetOf`, and `Overlaps`

**Concepts**
- IsSubsetOf checks every receiver element exists in other
- IsSupersetOf checks every other element exists in receiver
- Overlaps checks for any common element
- ISet<T> interface methods
- cheaper than full intersection for boolean result

**Answer**

`IsSubsetOf(other)` returns true when every element in the receiver also appears in `other` — useful for checking whether a required permission set is fully satisfied by a granted set. `IsSupersetOf(other)` returns true when the receiver contains every element of `other` — the inverse, useful for asserting that a full catalog covers a required subset. `Overlaps(other)` returns true when the two sets share at least one common element without computing the full intersection — cheaper than `IntersectWith` when you only need to know whether any overlap exists. All three are O(n) in the smaller set since they must verify each element, but avoid allocating a new set the way a full intersection would.

---

## Q68. Modifying an element in `HashSet` in place if it affects equality

**Concepts**
- in-place mutation of hashed element
- wrong bucket after hash-affecting mutation
- orphaned element (Count includes it, Contains misses it)
- remove-modify-reinsert pattern
- immutable identity fields

**Answer**

Mutating a property that participates in `GetHashCode` or `Equals` while the element is in a `HashSet<T>` leaves the element in the bucket corresponding to the old hash, so subsequent `Contains`, `Remove`, and set operations stop finding it. The element is still counted — `Count` includes it — but every hash-based operation looks in the new-hash bucket and finds nothing while the element sits unreachable in the old bucket. The correct pattern is to remove before mutating, apply the change, then re-add: `set.Remove(item); item.Field = newValue; set.Add(item)`. Better long-term is making identity fields immutable (`init` or readonly) so mutation that would break the hash contract is prevented at the type level.

---

### 06. Queue and Stack

---

## Q69. `Queue<T>` and `Stack<T>` vs their non-generic counterparts

**Concepts**
- generic type safety eliminates boxing
- TryDequeue/TryPop safe non-throwing variants
- non-generic Queue/Stack store object elements
- compile-time type checking on Enqueue/Push
- explicit cast required on non-generic read

**Answer**

Generic `Queue<T>` and `Stack<T>` store elements in typed arrays, avoiding boxing for value types and providing compile-time type checking on `Enqueue`/`Push` and `Dequeue`/`Pop`. Non-generic `Queue` and `Stack` (in `System.Collections`) store `object` elements, boxing value types on every operation and returning `object` that requires an explicit cast on read. The generic versions also introduce the try-patterns absent from the non-generic API: `TryDequeue(out T)`, `TryPeek(out T)`, and `TryPop(out T)` return `bool` without throwing when the collection is empty, making consumer loops cleaner than the `Count > 0` + `Dequeue` pattern required with the non-generic types.

---

## Q70. FIFO vs LIFO, and which collection maps to each

**Concepts**
- FIFO — first-in, first-out ordering
- LIFO — last-in, first-out ordering
- Queue<T> implements FIFO
- Stack<T> implements LIFO
- ordering invariant determines use case

**Answer**

FIFO (first-in, first-out) means elements are processed in the order they were added — the first item enqueued is the first dequeued. `Queue<T>` implements FIFO with `Enqueue` adding to the tail and `Dequeue` removing from the head, naturally modeling task queues, message buffers, and any "serve in arrival order" scenario. LIFO (last-in, first-out) means the most recently added element is the first removed. `Stack<T>` implements LIFO with `Push` adding to the top and `Pop` removing from the top, naturally modeling undo history, call stacks, expression evaluation, and depth-first search frontiers.

---

## Q71. Operations `Queue<T>` exposes

**Concepts**
- Enqueue adds to tail
- Dequeue removes from head (throws if empty)
- Peek reads head without removing (throws if empty)
- TryDequeue and TryPeek safe non-throwing variants
- circular array amortized O(1) operations

**Answer**

`Enqueue(item)` adds an element to the tail. `Dequeue()` removes and returns the head element, throwing `InvalidOperationException` if the queue is empty. `Peek()` returns the head without removing it, also throwing if empty. `TryDequeue(out T result)` and `TryPeek(out T result)` are the non-throwing equivalents returning `false` when empty — these are correct for consumer loops where an empty queue is a normal operational state rather than an error. The underlying implementation uses a circular array that doubles when full, giving amortized O(1) for `Enqueue` and O(1) for `Dequeue` without element shifting.

---

## Q72. Operations `Stack<T>` exposes

**Concepts**
- Push adds to top
- Pop removes from top (throws if empty)
- Peek reads top without removing (throws if empty)
- TryPop safe non-throwing variant
- array-backed with amortized O(1) Push/Pop

**Answer**

`Push(item)` adds an element to the top of the stack. `Pop()` removes and returns the top element, throwing `InvalidOperationException` when empty. `Peek()` returns the top element without removing it, also throwing when empty. `TryPop(out T result)` is the non-throwing variant that returns `false` when empty, making it the correct choice for iterative algorithms where the stack naturally empties as the algorithm completes. The underlying implementation uses an array that doubles when full, giving amortized O(1) for both `Push` and `Pop`.

---

## Q73. Why `Queue` and `Stack` do not support random access by index

**Concepts**
- restricted end-access by design
- semantic contract enforcement
- indexer would expose middle elements
- ordering invariant preserved
- List<T> as alternative when random access needed

**Answer**

`Queue<T>` and `Stack<T>` deliberately expose only end-access operations because their semantic contracts — FIFO and LIFO — are their defining value. Exposing an indexer would encourage using them as general-purpose lists, undermining the ordering guarantees that make them useful for their intended patterns. The underlying circular array and array implementations could technically support indexing, but doing so would allow callers to access or remove elements from the middle, which would break the queue or stack invariant. If you need both end-based semantics and random access, use `List<T>` with explicit discipline about which end is operated on.

---

## Q74. `Queue<T>` in breadth-first search (BFS)

**Concepts**
- frontier queue for layer-by-layer expansion
- enqueue start, dequeue-process-enqueue-neighbors loop
- visited HashSet<T> to prevent re-processing
- shortest path guarantee in unweighted graphs
- FIFO ordering drives layer-by-layer traversal

**Answer**

BFS uses a `Queue<T>` as a frontier — enqueue the start node, then repeatedly dequeue the head node, process it, and enqueue all of its unvisited neighbors. Because `Queue<T>` is FIFO, all nodes at distance 1 from the start are processed before any node at distance 2, ensuring that the first time you reach a destination node you have found the shortest path in terms of edge count. The typical pattern pairs the queue with a `HashSet<T>` of visited nodes: before enqueuing a neighbor check `visited.Contains(neighbor)` and add to visited at the same time as enqueue to prevent processing the same node twice and entering cycles.

---

## Q75. Why BFS finds shortest paths in unweighted graphs

**Concepts**
- layer-by-layer FIFO expansion
- non-decreasing distance ordering
- minimum edge count on first reach
- unweighted edge assumption (all edges cost 1)
- DFS comparison — no such guarantee

**Answer**

BFS explores nodes in non-decreasing order of their distance from the source because the queue's FIFO ordering means all nodes enqueued at step k (distance k edges) are dequeued before any node enqueued at step k+1. Since every edge has the same weight of 1 in an unweighted graph, processing nodes layer by layer means the first time you dequeue the destination node you have reached it via the minimum number of edges — any later path to that node would have been enqueued at a greater step. DFS has no such guarantee because it pursues one branch as deeply as possible before backtracking, potentially discovering a long path before a short one.

---

## Q76. Real-world workflows that map to a stack

**Concepts**
- undo/redo history (push on edit, pop to undo)
- CLR call stack (push frame, pop on return)
- depth-first search frontier
- expression evaluation and bracket matching
- backtracking algorithms

**Answer**

Undo/redo systems push state snapshots or command objects onto a `Stack<T>` on each edit; `Pop` retrieves the most recent state to restore, which is the LIFO property applied to editing history. The CLR call stack is itself a stack — each method invocation pushes a frame with local variables and the return address, and returning pops it, which is why deeply recursive algorithms overflow with `StackOverflowException`. Depth-first search uses an explicit `Stack<T>` as its frontier: push the start node, then repeatedly pop, process, and push unvisited neighbors — LIFO ensures you pursue one path deeply before exploring siblings. Other uses include expression parsing with operator-precedence (shunting-yard), bracket matching in syntax highlighters, and Sudoku solvers that backtrack to the last decision point.

---

## Q77. Non-generic `Queue`/`Stack` vs generic versions regarding boxing

**Concepts**
- non-generic object[] backing causes boxing
- generic T[] backing avoids boxing for value types
- cast required on non-generic read
- allocation rate difference in hot loops
- reference types unaffected

**Answer**

Non-generic `Queue` and `Stack` (in `System.Collections`) store elements in `object[]` backing arrays, so every `int`, `bool`, or other value type is boxed into a heap object on `Enqueue`/`Push` and unboxed (with an explicit cast) on `Dequeue`/`Pop`. Generic `Queue<T>` and `Stack<T>` store elements in `T[]` backing arrays so value-type elements are unboxed with no per-element heap allocation. For reference types the behavior is identical since references are stored directly in both cases. The boxing cost matters in high-frequency scenarios — a `Queue` processing a million integer events per second creates a million box objects per second, while `Queue<int>` creates none.

---

## Q78. `Queue<T>` over `List<T>` with remove-from-front patterns

**Concepts**
- O(1) Dequeue via circular array head advance
- O(n) List.RemoveAt(0) due to element shifting
- semantic clarity of Queue contract
- producer-consumer pattern
- performance scaling difference

**Answer**

`Queue<T>` uses a circular array so `Dequeue()` is O(1) — it just advances the head index without shifting elements. `List<T>.RemoveAt(0)` is O(n) because every remaining element must shift one position left to fill the gap at index 0. For any pattern that appends to the back and removes from the front, `Queue<T>` is dramatically faster at scale and communicates the FIFO intent clearly. Use `List<T>` only when you need random access by index, insertion in the middle, or other operations that `Queue<T>` does not expose — if the only operations are append-to-end and remove-from-front, `Queue<T>` is the correct tool for both performance and code clarity.

---

### 07. SortedList & SortedDictionary

---

## Q79. `SortedList<TKey,TValue>` vs `SortedDictionary<TKey,TValue>`

**Concepts**
- SortedList array-backed with O(1) rank access
- SortedDictionary red-black tree O(log n) all ops
- O(n) insert/delete for SortedList due to array shifting
- no rank indexer on SortedDictionary
- use case: static small map vs write-heavy sorted map

**Answer**

`SortedList<TKey,TValue>` uses two parallel arrays (keys[] and values[]) kept in sorted order, which gives O(1) access by sorted rank via `Keys[i]` and `Values[i]` and O(log n) lookup via binary search, but O(n) insert and delete because shifting elements is required. `SortedDictionary<TKey,TValue>` uses a red-black tree so insert, delete, and lookup are all O(log n) with no shifting, but the `Keys` property returns `ICollection<TKey>` with no indexer, meaning random access by rank is not available. Use `SortedList` for small mostly-static collections where indexed rank access matters; use `SortedDictionary` for write-heavy workloads where O(log n) insert/delete is preferred over O(n) shifting.

---

## Q80. `IComparer<TKey>` vs `IEqualityComparer<TKey>`

**Concepts**
- IComparer<T> defines ordering (Compare returns int)
- IEqualityComparer<T> defines hash-bucket equality
- sorted collections require IComparer<T>
- hash collections require IEqualityComparer<T>
- StringComparer implements both interfaces

**Answer**

`IComparer<T>` defines a total order by a `Compare(T x, T y)` method returning negative, zero, or positive, and is required by sorted collections (`SortedList`, `SortedDictionary`, `SortedSet`) to determine where to place and find elements. `IEqualityComparer<T>` defines hash bucket equality via `Equals(T x, T y)` and `GetHashCode(T obj)`, and is required by hash-based collections (`Dictionary`, `HashSet`). The two interfaces are not interchangeable — passing an `IEqualityComparer` to a sorted collection constructor causes a compile error because it provides no comparison for ordering. `StringComparer.OrdinalIgnoreCase` and similar built-in comparers happen to implement both interfaces, which is why they work with both collection types, but a custom comparer class must implement the correct interface for the collection being used.

---

## Q81. SortedList array-backed vs SortedDictionary tree-backed performance

**Concepts**
- SortedList O(log n) lookup, O(n) insert/delete
- SortedDictionary O(log n) lookup, insert, delete
- array shifting cost on SortedList mutations
- cache-friendly array iteration vs pointer-chased tree
- per-node heap allocations in SortedDictionary

**Answer**

`SortedList` binary-searches the key array for O(log n) lookup and rank access, but insert and delete must shift remaining elements, making both O(n). This means N random inserts into a `SortedList` of N elements total O(n²) work. `SortedDictionary` tree operations are O(log n) for lookup, insert, and delete with no shifting, so N insertions cost O(n log n) total. For sequential iteration, `SortedList`'s contiguous arrays benefit from CPU cache prefetch, while `SortedDictionary` traverses tree nodes scattered on the heap with pointer indirection. For small collections that are built once and read often, `SortedList` wins on memory layout and rank access. For frequently mutating collections, `SortedDictionary` wins on mutation cost.

---

## Q82. When to prefer `SortedList` over `SortedDictionary`

**Concepts**
- Keys[i] and Values[i] rank access
- small mostly-static datasets
- contiguous memory for cache efficiency
- lower overhead than tree nodes
- sorted export without extra allocation

**Answer**

`SortedList` is preferable when you need `Keys[i]` or `Values[i]` rank access — for example, finding the first or last key directly, or walking keys by position — since `SortedDictionary` exposes no such indexer. It is also preferable for small mostly-static collections (configuration tables, lookup maps built once at startup) where insertions are rare or sequential, because the contiguous parallel arrays are more memory-efficient than tree nodes and iteration is cache-friendly. When the dataset is large and frequently mutated with random inserts, `SortedDictionary` is the better choice to avoid O(n) shifting per insert.

---

## Q83. Cost of inserting out-of-order keys into a sorted collection

**Concepts**
- binary search locates insert position O(log n)
- array shift after found position O(n)
- N random inserts total O(n²) for SortedList
- SortedDictionary N inserts total O(n log n)
- pre-sorted input reduces shift cost

**Answer**

Every insert into `SortedList` first binary-searches the key array in O(log n) to find the insertion position, then shifts all elements after that position one slot right, which is O(n). For N random inserts, the total is O(n²) which becomes visible once the collection has more than a few thousand entries. `SortedDictionary` tree rebalancing is O(log n) per insert, so N inserts cost O(n log n) total. If keys happen to arrive in sorted order, `SortedList` shifts zero or very few elements per insert, approaching O(n log n) in that special case, but random-order insertion eliminates that advantage. The practical consequence is that bulk loading a `SortedList` from an unsorted source is significantly slower than bulk loading a `SortedDictionary`.

---

## Q84. `SortedList.Keys` vs `SortedDictionary.Keys` — indexer availability

**Concepts**
- SortedList.Keys returns IList<TKey> with O(1) indexer
- SortedDictionary.Keys returns ICollection<TKey> without indexer
- rank access only on SortedList
- SortedDictionary requires iteration to find nth key
- interface differences encode capability difference

**Answer**

`SortedList<TKey,TValue>.Keys` returns an `IList<TKey>` view of the key array, which provides an O(1) indexer so you can write `list.Keys[0]` for the minimum or `list.Keys[list.Count - 1]` for the maximum in constant time. `SortedDictionary<TKey,TValue>.Keys` returns an `ICollection<TKey>` view which has no indexer — accessing the nth key requires iterating with `foreach` or materializing with `Keys.First()` (O(1) for min via tree traversal) but index-by-position is not directly supported. The type difference in the return is intentional and signals this structural difference: if your code requires `Keys[i]`, you must use `SortedList`.

---

## Q85. `SortedSet<T>` vs `SortedDictionary<TKey,TValue>`

**Concepts**
- SortedSet<T> keys only (implements ISet<T>)
- SortedDictionary stores key-value pairs
- SortedSet has set operations (Union, Intersect, GetViewBetween)
- SortedDictionary has no set operations
- both tree-backed O(log n)

**Answer**

`SortedSet<T>` stores only distinct keys in sorted order (red-black tree) and implements `ISet<T>`, providing set operations like `UnionWith`, `IntersectWith`, `ExceptWith`, and `GetViewBetween(min, max)` for range slices. It has no associated values. `SortedDictionary<TKey,TValue>` stores key-value pairs in sorted key order (also red-black tree) and does not implement `ISet<T>`, providing no set operations. Use `SortedSet<T>` when you need a sorted collection of distinct items with set algebra; use `SortedDictionary<TKey,TValue>` when each sorted key maps to a value.

---

## Q86. `SortedDictionary` vs `Dictionary` + sort at read time

**Concepts**
- SortedDictionary O(log n) insert keeps order maintained incrementally
- Dictionary + OrderBy O(n log n) on every sorted read
- read-to-write ratio determines trade-off
- caching sorted projection amortizes sort cost
- SortedDictionary better for interleaved writes and sorted reads

**Answer**

`SortedDictionary` maintains sorted order incrementally as keys are inserted at O(log n) each, so any subsequent sorted iteration costs just O(n). `Dictionary` + `OrderBy` or `.Keys.Order()` pays O(n log n) on every sorted read, which is acceptable when sorted reads are infrequent relative to the total number of reads and writes. If the pattern is "write many, read sorted rarely," sorting once when exporting is fine and `Dictionary` has faster O(1) average lookup. If the pattern is "write incrementally and read sorted frequently," `SortedDictionary` avoids repeating O(n log n) sort cost on each read. A cached sorted projection from a `Dictionary` is a middle ground — sort once, cache the projection, invalidate on mutation — but adds cache-coherence complexity that `SortedDictionary` handles automatically.

---

### 08. IEnumerable & IEnumerator

---

## Q87. `IEnumerable<T>` vs `ICollection<T>`

**Concepts**
- IEnumerable<T> — GetEnumerator only, forward-only, lazy
- ICollection<T> extends IEnumerable<T> with Count, Add, Remove, Contains
- IEnumerable<T> does not expose Count
- lazy vs in-memory distinction
- interface hierarchy

**Answer**

`IEnumerable<T>` has one method — `GetEnumerator()` — enabling forward-only sequential enumeration with no count and no mutation. It makes no promise about how many elements exist or whether another call to `GetEnumerator()` repeats work. `ICollection<T>` extends `IEnumerable<T>` and adds `Count`, `Add(T)`, `Remove(T)`, `Contains(T)`, and `CopyTo(T[],int)`, which means it is always in-memory and finite. The practical consequence is that a method returning `IEnumerable<T>` may be lazy (yield, LINQ), while one returning `ICollection<T>` guarantees `Count` is available in O(1) and mutation is possible.

---

## Q88. `ICollection<T>` vs `IList<T>`

**Concepts**
- ICollection<T> — no indexer, no positional operations
- IList<T> adds indexer, Insert(int,T), RemoveAt(int), IndexOf(T)
- positional access is the distinguishing capability
- array implements IList<T>
- choosing return type by capability

**Answer**

`ICollection<T>` provides count and unordered mutation (add/remove by value) but no indexer or positional operations. `IList<T>` extends `ICollection<T>` by adding an `int`-indexed getter/setter (`this[int]`), `Insert(int,T)`, `RemoveAt(int)`, and `IndexOf(T)`, enabling positional access and O(1) random read when the backing type is an array or `List<T>`. Use `ICollection<T>` as a return type when callers need mutation but not random access; use `IList<T>` when callers need to read or write by index.

---

## Q89. `IReadOnlyList<T>` and `IReadOnlyCollection<T>`

**Concepts**
- IReadOnlyList<T> — read-only indexer plus Count, covariant
- IReadOnlyCollection<T> — Count only, covariant
- covariance enables IReadOnlyList<string> to IReadOnlyList<object>
- signals materialized read-only result to callers
- no Add/Remove mutation through interface

**Answer**

`IReadOnlyCollection<T>` adds `Count` to `IEnumerable<T>` and is covariant (`out T`), meaning `IReadOnlyCollection<string>` is assignable to `IReadOnlyCollection<object>`. `IReadOnlyList<T>` extends it with a read-only indexer (`this[int]`) and is also covariant, so `IReadOnlyList<string>` is assignable to `IReadOnlyList<object>`. These are the correct return types for service methods that return a fully materialized snapshot the caller should not mutate — they communicate that the result is in-memory and indexed without exposing mutation methods. `List<T>.AsReadOnly()` returns `IReadOnlyList<T>` as a view over the list.

---

## Q90. `IEnumerator` vs `IEnumerator<T>`

**Concepts**
- IEnumerator.Current returns object (boxing for value types)
- IEnumerator<T>.Current returns T (typed, no boxing)
- IEnumerator<T> extends IDisposable
- foreach emits try/finally Dispose call
- non-generic interface legacy from .NET 1.0

**Answer**

Non-generic `IEnumerator` has `Current` as `object`, requiring a cast and boxing any value-type elements. Generic `IEnumerator<T>` has a typed `Current` property returning `T` directly, eliminating the cast and boxing overhead. More importantly, `IEnumerator<T>` extends `IDisposable`, so `foreach` always emits a `try/finally` that calls `Dispose()` on the enumerator when iteration completes or exits early via `break` or exception. Non-generic `IEnumerator` does not extend `IDisposable`, so compiler-generated code checks whether the enumerator implements `IDisposable` at runtime and conditionally disposes it, but the clean pattern relies on `IEnumerator<T>`.

---

## Q91. The `yield` keyword and how iterator methods relate to `IEnumerable<T>`

**Concepts**
- yield return turns method into state machine
- compiler generates IEnumerable<T>/IEnumerator<T> state machine class
- lazy — body runs only when MoveNext is called
- each foreach call gets a fresh state machine instance
- yield break terminates the sequence

**Answer**

A method that returns `IEnumerable<T>` or `IEnumerator<T>` and contains a `yield return` statement is an iterator method. The compiler transforms it into a state machine class that implements both `IEnumerable<T>` and `IEnumerator<T>`. When a caller calls `GetEnumerator()` (directly or via `foreach`), they receive an instance of that state machine with position reset to before the first element. Each call to `MoveNext()` runs the method body from where it last yielded until it hits the next `yield return`, which sets `Current` and suspends execution. This means the method body runs lazily — only as fast as the consumer calls `MoveNext()` — and calling the method itself does not execute any of the body.

---

## Q92. Deferred execution vs immediate execution

**Concepts**
- Where/Select/Skip/Take — lazy IEnumerable<T> operators
- ToList/ToArray/Count/Sum/First — immediate operators
- deferred stores recipe not results
- immediate forces full enumeration
- side effects in deferred pipelines run at consumption time

**Answer**

LINQ operators like `Where`, `Select`, `Skip`, `Take`, `OrderBy`, and `GroupBy` return a new lazy `IEnumerable<T>` that stores the operation as a description of what to do, not the results. Nothing runs until a terminal operator or `foreach` forces enumeration. Operators like `ToList()`, `ToArray()`, `Count()`, `Sum()`, `First()`, and `Any()` are immediate — they trigger full enumeration and return a concrete value or collection. The practical consequence is that a `Where(predicate)` chain applied to a database-backed source runs the query on every enumeration; materializing with `ToList()` runs it once and stores the snapshot for safe multiple reads.

---

## Q93. The iterator pattern — `MoveNext`, `Current`, and `Reset`

**Concepts**
- MoveNext advances and returns bool
- Current exposes element at current position
- Reset optional — compiler iterators throw NotSupportedException
- before-first and after-last sentinel positions
- enumerator is stateful and not thread-safe

**Answer**

`MoveNext()` advances the enumerator to the next element and returns `true` if a next element exists, `false` if the sequence is exhausted. Calling `Current` before the first `MoveNext()` or after `MoveNext()` returns `false` is undefined behavior (typically throws or returns a default). `Reset()` is defined on the interface but compiler-generated iterators (state machines from `yield return`) throw `NotSupportedException` on `Reset()` — the method was kept for COM interop compatibility but is effectively obsolete. `foreach` never calls `Reset()`; it calls `GetEnumerator()` to obtain a fresh enumerator for each iteration.

---

## Q94. `yield break` vs `return` in an iterator method

**Concepts**
- yield break causes MoveNext to return false
- return is not valid in iterator body
- yield break used for conditional early termination
- caller sees end of sequence
- state machine transitions to finished state

**Answer**

In an iterator method, `return` without a value is a compile error — the only valid exit keyword is `yield break`. When the state machine executes `yield break`, the next call to `MoveNext()` returns `false`, signaling the consumer that the sequence is exhausted. `yield break` is used for conditional early termination — for example, if a guard condition fails, the method exits with `yield break` rather than yielding any element and the consumer's `foreach` ends naturally without receiving anything. This is equivalent to a method that produces zero elements from that point onward.

---

## Q95. Why multiple enumeration of the same LINQ `IEnumerable` re-runs the pipeline

**Concepts**
- IEnumerable<T> stores recipe not results
- each GetEnumerator creates new state machine instance
- re-enumeration re-executes all deferred operators
- I/O-backed source repeats query on each pass
- ToList materializes for safe multiple reads

**Answer**

A LINQ chain like `source.Where(x => x.Active).Select(x => x.Name)` stores a chain of operator objects — each holding a reference to the previous operator and its lambda. There is no backing collection. Every time `foreach` (or any terminal operator) calls `GetEnumerator()`, it creates a fresh state machine instance at position zero and walks the source from the beginning, re-executing every predicate and projection for every element. If the source is a database query, a file read, or any non-idempotent `yield return` method, that work repeats. Calling `ToList()` forces enumeration once and returns a `List<T>` whose `GetEnumerator()` always reads the same in-memory data.

---

## Q96. Returning `IEnumerable<T>` vs `List<T>` from a method

**Concepts**
- IEnumerable<T> hides whether result is lazy or materialized
- List<T> signals materialized mutable result
- IReadOnlyList<T> signals materialized read-only indexed result
- return type communicates capability to caller
- multiple enumeration risk with IEnumerable<T> return

**Answer**

Returning `IEnumerable<T>` allows the implementation to be lazy or materialized but hides that distinction from callers, who may accidentally enumerate multiple times and pay O(n) work per pass or hit I/O twice. Returning `List<T>` signals that the result is already materialized and mutable, but exposes mutation methods the caller should not use. Returning `IReadOnlyList<T>` is the best default for a materialized result: it signals that the data is in-memory (callers can safely count and index), prevents mutation through the interface, and is covariant for assignment compatibility. Reserve `IEnumerable<T>` return types for genuinely lazy streaming pipelines where materialization would waste memory.

---

## Q97. Modifying a collection during `foreach` — how the enumerator detects it

**Concepts**
- List<T> internal _version counter
- enumerator captures _version at GetEnumerator time
- MoveNext compares current _version to captured version
- throws InvalidOperationException on mismatch
- structural change (add/remove) increments _version

**Answer**

`List<T>` maintains a private `_version` counter that increments on every structural change — `Add`, `Remove`, `Insert`, `RemoveAt`, `Clear`, and the indexer setter. When `GetEnumerator()` is called, the returned enumerator captures the current `_version` value. Each call to `MoveNext()` compares the current list `_version` to the captured value; if they differ, it throws `InvalidOperationException` with the message "Collection was modified; enumeration operation may not execute." This detection is best-effort — it catches any structural modification but cannot guarantee detection of all concurrent modifications in multi-threaded scenarios without locking.

---

## Q98. Covariance on `IEnumerable<out T>`

**Concepts**
- out T variance annotation enables covariance
- IEnumerable<string> assignable to IEnumerable<object>
- works because IEnumerable<T> is read-only (out direction only)
- List<T> is not covariant (IList<T> is invariant)
- practical use: return base type or interface

**Answer**

`IEnumerable<T>` is declared as `IEnumerable<out T>` — the `out` variance annotation means `T` only flows out of the interface (through `Current`), never in. This makes it covariant: `IEnumerable<string>` is assignable to `IEnumerable<object>` because reading a string where an object is expected is safe. Practical examples include passing `IEnumerable<SqlCommand>` where `IEnumerable<IDbCommand>` is expected, or a `List<string>` (which implements `IEnumerable<string>`) to a method taking `IEnumerable<object>`. `IList<T>` is invariant because it both reads and writes `T` — allowing `IList<string>` as `IList<object>` would let callers insert an `int` into a string list, breaking type safety.

---

## Q99. `foreach` vs manual `while (enumerator.MoveNext())`

**Concepts**
- foreach generates try/finally with Dispose
- manual while loop misses Dispose on early exit
- foreach emits checked IDisposable cast
- resource leak on break/exception without using
- foreach preferred for correctness

**Answer**

`foreach` compiles to a `try/finally` block that calls `Dispose()` on the enumerator in the `finally` clause, ensuring cleanup even when the loop exits via `break`, `return`, or an exception. A manual `while (enumerator.MoveNext())` loop without a `using` or explicit `try/finally` skips `Dispose()` on early exit, leaking any resources held by the enumerator — file handles, database cursors, pooled connections. The correct manual equivalent is `using var e = source.GetEnumerator(); while (e.MoveNext()) { ... }`. Use `foreach` by default; only use the manual pattern when you need to access the enumerator across multiple scopes or interleave two enumerators.

---

## Q100. `ToList()` materialization — when to materialize before multiple passes

**Concepts**
- single terminal operator needs no materialization
- multiple terminal operators on deferred source require materialization
- I/O-backed or non-idempotent source must be materialized
- snapshot before mutation or parallel work
- already-materialized sources need no extra ToList

**Answer**

Materialize with `ToList()` or `ToArray()` when you will enumerate the sequence more than once (`Count()` followed by `foreach`, or two separate `foreach` loops), when the source is I/O-backed (database query, file reader) and re-enumeration would repeat the I/O, when you need a stable snapshot before the underlying collection can be mutated by concurrent code, or before handing the sequence to parallel work that runs multiple threads simultaneously. Skip materialization when you have a single `foreach` over an in-memory source (it copies without benefit), when the sequence is infinite or very large and materialization would exhaust memory, or when the source is already a `List<T>` or array (materialized).

---

## Q101. `IAsyncEnumerable<T>` and async iterators

**Concepts**
- IAsyncEnumerable<T> for async streaming
- await foreach consumes it
- async iterator method: async + IAsyncEnumerable<T> return + yield return
- MoveNextAsync returns ValueTask<bool>
- ConfigureAwait on await foreach

**Answer**

`IAsyncEnumerable<T>` is the async counterpart of `IEnumerable<T>`, enabling a producer to `yield return` elements asynchronously — for example, streaming rows from a database one page at a time without buffering all results. An async iterator method is marked `async`, returns `IAsyncEnumerable<T>`, and uses `yield return` to produce elements, which may each be preceded by `await` calls. Consumers use `await foreach (var item in source)` which internally calls `MoveNextAsync()` returning `ValueTask<bool>` and awaits each advancement. To configure the synchronization context, use `await foreach (var item in source.ConfigureAwait(false))`. This pattern is ideal for paginated API responses, streaming database reads with `IAsyncEnumerable<T>` from EF Core's `AsAsyncEnumerable()`, and any scenario where pulling each element has I/O cost.

---

## Q102. Modifying a collection during `foreach`

**Concepts**
- _version counter invalidation
- InvalidOperationException on structural change
- snapshot via ToList to allow safe mutation
- reverse-index for loop as alternative
- RemoveAll for filtering in place

**Answer**

Changing a `List<T>` (or most BCL collections) structurally during a `foreach` throws `InvalidOperationException` because the enumerator detects the version counter mismatch. The clean fix is to iterate a snapshot: `foreach (var item in list.ToList())` lets you mutate `list` freely inside the loop because the enumerator walks the copy. For removal-only patterns, `list.RemoveAll(predicate)` is the most efficient single-pass alternative. For general mutation using an index, a reverse `for (int i = list.Count - 1; i >= 0; i--)` loop is safe because removing at or above the current index does not shift items that have not been visited yet.

---

## Q103. Mutable keys in hash collections

**Concepts**
- Dictionary lookup by GetHashCode then Equals
- mutable key changes hash after insertion
- lookup finds wrong bucket — key appears lost
- value types as keys are safe (copied)
- strings and records as keys are naturally immutable

**Answer**

`Dictionary<TKey,TValue>` computes `GetHashCode` when a key is inserted to determine which hash bucket to place the entry in. If the key is a mutable reference type and its equality-relevant state changes after insertion, the next lookup recomputes a different hash code, lands in a different bucket, finds nothing, and reports the key as missing — the entry is stranded in the old bucket. The dictionary is not corrupted but the entry is effectively inaccessible until a full bucket scan (which `ContainsKey` does not do). The fix is to use immutable or naturally-immutable types as keys: strings, value-type structs copied by value at insert, or records with value equality based on constructor arguments.

---

## Q104. `IEnumerable<T>` is covariant but `List<T>` is not

**Concepts**
- IEnumerable<out T> covariance — read-only justified
- IList<T> invariant — write operations break covariance
- assigning List<string> to IEnumerable<object> is valid
- assigning List<string> to List<object> is compile error
- why mutable covariance is unsound

**Answer**

`IEnumerable<out T>` is covariant because the interface only produces `T` values (via `Current`) and never consumes them, so a `string` produced where `object` is expected is always safe. `IList<T>` and `List<T>` are invariant because they also accept `T` via `Add(T)` and the indexer setter — if `List<string>` were assignable to `List<object>`, a caller could insert an `int` into what is actually a `string` list, breaking type safety at runtime. The runtime enforces this: `(List<object>)(object)new List<string>()` compiles but throws `InvalidCastException` at runtime. The correct approach is to expose `IEnumerable<object>` or `IReadOnlyList<object>` when covariant read-only access is needed.

---

## Q105. Wrong collection for the job

**Concepts**
- List<T> O(n) for middle insert/remove
- LinkedList<T> O(1) for middle insert when node known
- Dictionary<TKey,TValue> O(1) lookup vs List O(n) search
- HashSet<T> for membership testing
- choosing collection by access pattern

**Answer**

Choosing the wrong collection for the dominant operation is a common performance trap. `List<T>.Insert(0, item)` and `List<T>.RemoveAt(0)` are O(n) because all elements shift; use `LinkedList<T>` or `Queue<T>` for front-insertion or front-removal patterns. `List<T>.Contains(x)` is O(n) linear scan; use `HashSet<T>` for O(1) membership testing. Looking up values by key in a `List<T>` with `First(x => x.Id == id)` is O(n); use `Dictionary<int,T>` for O(1) lookup. The principle is: identify the dominant operation (lookup, insertion, deletion, iteration, membership), then pick the collection whose asymptotic cost for that operation matches the performance requirement.

---

## Q106. Static fields on generic types

**Concepts**
- separate static slot per closed generic type
- Cache<int>.Instance != Cache<string>.Instance
- CLR creates separate type per type argument
- intended isolation is correct but surprising
- runtime cost of many closed generic types

**Answer**

Every closed generic type — `Cache<int>`, `Cache<string>`, `Cache<DateTime>` — is a distinct type in the CLR, each with its own set of static fields. A static field on `class Cache<T>` is not shared across all `T`; `Cache<int>.Count` and `Cache<string>.Count` are entirely separate storage locations. This is usually the intended behavior (per-type caches) but is surprising when developers expect one shared counter or singleton across all instantiations. The flip side is that code with a large number of distinct type arguments can create many closed generic type objects in the process. If genuinely shared state is needed, move it to a non-generic wrapper class that the generic type references.

---

## Q107. Boxing in non-generic collections

**Concepts**
- ArrayList stores object[]
- every value type Enqueue/Add causes boxing
- every read from non-generic collection requires cast
- List<T>/Queue<T>/Stack<T> store T[] — no boxing for value types
- GC pressure from box objects in hot paths

**Answer**

`ArrayList`, non-generic `Queue`, and non-generic `Stack` store `object[]` backing arrays, so every `int`, `bool`, `struct`, or other value type inserted is boxed into a heap-allocated wrapper object, and every read requires an explicit cast plus unboxing. In a hot path that processes millions of values, this creates millions of short-lived box objects per second, increasing garbage collection pressure. Generic `List<int>`, `Queue<int>`, and `Stack<int>` store `int[]` directly — no boxing, no cast, no extra heap objects. The non-generic collections exist only for backward compatibility with .NET 1.x code and should not be used in new code.

---

## Q108. Passing `List<T>` by value

**Concepts**
- reference type — variable holds reference, not copy
- passing by value copies the reference not the list
- both caller and callee see the same List<T> contents
- Add/Remove inside method mutates caller's list
- ToList() or AsReadOnly() to snapshot/protect

**Answer**

`List<T>` is a reference type, so passing it to a method by value copies the reference — both the caller and the callee hold a reference to the same heap object. Any structural mutation inside the method (`Add`, `Remove`, `Clear`) is visible to the caller through their reference. This is surprising to developers who think "by value" means a copy. To pass a snapshot that the method cannot affect, call `new List<T>(original)` or `original.ToList()` before passing. To pass a read-only view, pass `original.AsReadOnly()` or use `IReadOnlyList<T>` as the parameter type to signal read-only intent.

---

## Q109. `Dictionary.Add` vs indexer on duplicate key

**Concepts**
- Add throws ArgumentException on duplicate key
- indexer overwrites silently on duplicate key
- TryAdd returns bool, no throw
- choosing Add for uniqueness assertion
- choosing indexer for upsert semantics

**Answer**

`Dictionary<TKey,TValue>.Add(key, value)` throws `ArgumentException` if the key already exists, making it suitable when inserting a duplicate is a bug and you want an exception rather than silent data loss. The indexer `dict[key] = value` inserts if the key is absent or overwrites the existing value silently if the key exists, making it correct for upsert semantics. `TryAdd(key, value)` returns `false` without throwing when the key exists, which is useful for concurrent-friendly patterns or when a missed insert is a normal non-error state. Choosing between them depends on whether a duplicate is an error (use `Add`), expected (use indexer), or a conditional no-op (use `TryAdd`).

---

## Q110. `AsReadOnly()` is a view, not a copy

**Concepts**
- AsReadOnly returns ReadOnlyCollection<T> wrapper
- mutations to original list visible through wrapper
- no snapshot — same underlying array
- ReadOnlyCollection<T> does not prevent caller mutation of original
- ToList() or ImmutableList<T> for true isolation

**Answer**

`List<T>.AsReadOnly()` returns a `ReadOnlyCollection<T>` that wraps the original list by reference, not by copy. If the original list is mutated after `AsReadOnly()` is called, the read-only wrapper immediately reflects those changes — `Count` grows, elements shift. The wrapper only prevents mutation through its own interface (no `Add`, `Remove`, or indexer setter), but the original reference is still available and mutable. To give a consumer a truly isolated snapshot, use `new List<T>(original)` or `original.ToList()` before wrapping, or use `ImmutableList<T>.ToImmutableList()` which creates a fully immutable persistent structure.

---

## Q111. Assuming dictionary enumeration order

**Concepts**
- Dictionary<TKey,TValue> enumeration order undefined
- insertion order not guaranteed
- order depends on hash codes and bucket layout
- .NET runtime may change order across versions
- SortedDictionary or OrderBy for deterministic order

**Answer**

`Dictionary<TKey,TValue>` does not guarantee any enumeration order — iterating `Keys`, `Values`, or entries yields items in an internal hash-bucket-dependent order that may differ between runs, CLR versions, or after resize operations. Code that relies on the first enumerated key being the most-recently inserted key, or on keys appearing in alphabetical order, will produce inconsistent results. For sorted enumeration use `SortedDictionary<TKey,TValue>` or explicitly sort: `dict.Keys.Order()` (LINQ). For insertion-order preservation, `LinkedList<T>` or maintaining a separate `List<TKey>` alongside the dictionary is the correct approach; `OrderedDictionary` from `System.Collections.Specialized` preserves insertion order but is non-generic.

---

## Q112. Multiple enumeration cost

**Concepts**
- deferred IEnumerable<T> re-executes on each enumeration
- I/O-backed source repeats query per enumeration
- count then foreach on same lazy sequence = two passes
- ToList materializes once
- IReadOnlyList<T> return signals safe multiple reads

**Answer**

Every terminal operator and `foreach` on a deferred `IEnumerable<T>` calls `GetEnumerator()` and walks the source from scratch. Calling `Count()` followed by `foreach` on the same lazy sequence performs two full enumerations — doubling I/O, doubling compute, and potentially returning inconsistent results if the source changes between the two passes. The fix is to materialize once: `var list = source.ToList()` then use `list.Count` and iterate `list` as many times as needed. Returning `IReadOnlyList<T>` from service methods signals that the result is already materialized and safe for multiple reads without hidden cost.

---

## Q113. Wrong comparer on sorted types

**Concepts**
- SortedDictionary/SortedList require IComparer<TKey>
- IEqualityComparer<TKey> is for hash collections only
- passing wrong interface causes compile error
- StringComparer implements both
- custom sort rules need IComparer<T> implementation

**Answer**

`SortedDictionary<TKey,TValue>` and `SortedList<TKey,TValue>` constructors accept an `IComparer<TKey>` to define ordering. Passing an `IEqualityComparer<TKey>` (which `Dictionary` and `HashSet` use) causes a compile error because the interfaces are unrelated. A common mistake is extracting a custom equality comparer from a `Dictionary` and passing it to a `SortedDictionary` constructor — the two interfaces serve different purposes and are not interchangeable. `StringComparer.OrdinalIgnoreCase` and similar built-in comparers implement both interfaces and work with both collection types. A custom ordering rule requires implementing `IComparer<T>` with a `Compare(T x, T y)` method, not `IEqualityComparer<T>`.

---

## Q114. Poor `GetHashCode` distribution

**Concepts**
- all keys map to same bucket when hash is constant
- O(n) bucket scan on lookup instead of O(1)
- GetHashCode must distribute across int range
- must be consistent with Equals
- XOR of field hash codes as common pattern

**Answer**

`Dictionary<TKey,TValue>` and `HashSet<T>` distribute entries across buckets by `GetHashCode() % bucketCount`. If a custom `GetHashCode` returns the same constant for all instances — for example, `return 42;` — every key lands in the same bucket, turning the hash table into a linear-search list with O(n) lookup. A well-distributed `GetHashCode` uses all distinguishing fields: typically XOR or combine their individual hash codes. The contract requires that two equal objects (as defined by `Equals`) must return the same hash code, but unequal objects should ideally return different hash codes. In modern .NET, `HashCode.Combine(field1, field2, ...)` provides a correct and well-distributed implementation.

---

## Q115. `Queue.Contains` is O(n)

**Concepts**
- Queue<T> has no hash index
- Contains performs linear scan of all elements
- O(n) per membership check
- maintain parallel HashSet<T> for O(1) membership
- composite structure: queue for ordering, set for deduplication

**Answer**

`Queue<T>.Contains(item)` performs a linear scan of the circular backing array — it is O(n) because the queue has no hash index. For workloads that need both FIFO ordering and fast membership testing — for example, a BFS visited-check or a deduplicating message buffer — maintain a `HashSet<T>` alongside the queue. When enqueuing, check `set.Contains(item)` first (O(1)); if absent, `queue.Enqueue(item)` and `set.Add(item)`. On dequeue, `queue.Dequeue()` and optionally `set.Remove(item)` if membership should expire when the item is processed. This composite keeps both operations at O(1) amortized at the cost of double memory for the keys.

---
