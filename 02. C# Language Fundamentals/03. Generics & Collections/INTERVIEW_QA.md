# 03. Generics & Collections — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [01. Generics](#01-generics)
  - [Q1. What are generics in C#? Why were they introduced?](#q1-what-are-generics-in-c-why-were-they-introduced)
  - [Q2. What is the difference between generic and non-generic collections?](#q2-what-is-the-difference-between-generic-and-non-generic-collections)
  - [Q3. Explain generic constraints in C# (`where` clause) with examples.](#q3-explain-generic-constraints-in-c-where-clause-with-examples)
  - [Q4. Explain covariance and contravariance in generics (`in` and `out` keywords).](#q4-explain-covariance-and-contravariance-in-generics-in-and-out-keywords)
  - [Q5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?](#q5-can-you-use-where-t-enum-or-where-t-unmanaged-what-problems-do-these-solve)
  - [Q6. What happens when you use `default(T)` on an unconstrained type parameter?](#q6-what-happens-when-you-use-defaultt-on-an-unconstrained-type-parameter)
  - [Q7. Why can't you write `T value = null;` unless `T` is constrained to `class`?](#q7-why-cant-you-write-t-value-null-unless-t-is-constrained-to-class)
  - [Q8. What is the difference between a generic class and a generic method?](#q8-what-is-the-difference-between-a-generic-class-and-a-generic-method)
  - [Q9. What's the difference between reflection over an open generic type (`List<>`) and a closed generic type (`List<int>`)?](#q9-whats-the-difference-between-reflection-over-an-open-generic-type-list-and-a-closed-generic-type-listint)
  - [Q10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`, and how do you get the shared generic type definition?](#q10-why-does-typeoflistint-typeofliststring-return-false-and-how-do-you-get-the-shared-generic-type-definition)
  - [Q11. What is type erasure vs reification — does C# retain generic type information at runtime?](#q11-what-is-type-erasure-vs-reification-does-c-retain-generic-type-information-at-runtime)
  - [Q12. What constraints allow calling `new T()` — what does `where T : new()` enable?](#q12-what-constraints-allow-calling-new-t-what-does-where-t-new-enable)
  - [Q13. What is the difference between `where T : class` and `where T : struct` constraints?](#q13-what-is-the-difference-between-where-t-class-and-where-t-struct-constraints)
  - [Q14. What does `where T : notnull` mean for nullable reference type analysis?](#q14-what-does-where-t-notnull-mean-for-nullable-reference-type-analysis)
  - [Q15. Why are generic value types separate closed types at runtime for static fields?](#q15-why-are-generic-value-types-separate-closed-types-at-runtime-for-static-fields)
  - [Q16. What is covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?](#q16-what-is-covariance-on-ienumerableout-t-why-can-you-assign-ienumerablestring-to-ienumerableobject)
  - [Q17. What is contravariance on `Action<in T>` / `IComparer<in T>`?](#q17-what-is-contravariance-on-actionin-t-icomparerin-t)
  - [Q18. Why is `List<T>` neither covariant nor contravariant on `T`?](#q18-why-is-listt-neither-covariant-nor-contravariant-on-t)
  - [Q19. What is the difference between generic specialization performance for value types vs reference types?](#q19-what-is-the-difference-between-generic-specialization-performance-for-value-types-vs-reference-types)
  - [Q20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?](#q20-can-you-cast-from-liststring-to-listobject-what-error-or-exception-occurs)

- [02. ArrayList](#02-arraylist)
  - [Q1. What is the difference between `Array` and `ArrayList`?](#q1-what-is-the-difference-between-array-and-arraylist)
  - [Q2. What is the difference between `List<T>` and `ArrayList`?](#q2-what-is-the-difference-between-listt-and-arraylist)
  - [Q3. Why is `ArrayList` considered a legacy collection in modern C#?](#q3-why-is-arraylist-considered-a-legacy-collection-in-modern-c)
  - [Q4. What boxing occurs when storing `int` values in an `ArrayList`?](#q4-what-boxing-occurs-when-storing-int-values-in-an-arraylist)
  - [Q5. What is the performance cost of repeated boxing/unboxing in hot loops using `ArrayList`?](#q5-what-is-the-performance-cost-of-repeated-boxingunboxing-in-hot-loops-using-arraylist)
  - [Q6. Can you store mixed types in an `ArrayList`, and what typing risks does that create?](#q6-can-you-store-mixed-types-in-an-arraylist-and-what-typing-risks-does-that-create)
  - [Q7. What is the difference between `ArrayList.Capacity` and `Count`?](#q7-what-is-the-difference-between-arraylistcapacity-and-count)
  - [Q8. When might you still encounter `ArrayList` in maintained legacy codebases?](#q8-when-might-you-still-encounter-arraylist-in-maintained-legacy-codebases)
  - [Q9. What is the difference between `ArrayList` and `object[]`?](#q9-what-is-the-difference-between-arraylist-and-object)
  - [Q10. What legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) should you know for maintenance scenarios?](#q10-what-legacy-non-generic-collections-hashtable-queue-stack-should-you-know-for-maintenance-scenarios)

- [03. List](#03-list)
  - [Q1. Explain the internal working and performance of `List<T>` vs `LinkedList<T>`.](#q1-explain-the-internal-working-and-performance-of-listt-vs-linkedlistt)
  - [Q2. What is `LinkedList<T>` and when should it be used?](#q2-what-is-linkedlistt-and-when-should-it-be-used)
  - [Q3. What is the difference between `List<T>.Sort()` stability and `OrderBy()` stability?](#q3-what-is-the-difference-between-listtsort-stability-and-orderby-stability)
  - [Q4. How does `List<T>` grow its internal buffer when capacity is exceeded?](#q4-how-does-listt-grow-its-internal-buffer-when-capacity-is-exceeded)
  - [Q5. What is the amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle?](#q5-what-is-the-amortized-cost-of-add-on-listt-vs-insert-at-the-beginning-or-middle)
  - [Q6. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?](#q6-what-does-listtasreadonly-return-and-can-callers-still-mutate-the-underlying-list)
  - [Q7. What is the difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list?](#q7-what-is-the-difference-between-convertall-foreach-and-linq-select-on-a-list)
  - [Q8. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?](#q8-what-do-toarray-copyto-and-getrange-do-which-allocate-new-arrays)
  - [Q9. When would you expose `List<T>` as a return type vs `IReadOnlyList<T>` or `IEnumerable<T>`?](#q9-when-would-you-expose-listt-as-a-return-type-vs-ireadonlylistt-or-ienumerablet)
  - [Q10. What is the difference between `List<T>.Capacity` and `Count`?](#q10-what-is-the-difference-between-listtcapacity-and-count)
  - [Q11. What happens if you mutate a list while iterating with `foreach`?](#q11-what-happens-if-you-mutate-a-list-while-iterating-with-foreach)
  - [Q12. What is `TrimExcess`, and when is it useful?](#q12-what-is-trimexcess-and-when-is-it-useful)
  - [Q13. What is binary search on a list (`BinarySearch`) — what precondition must the list satisfy?](#q13-what-is-binary-search-on-a-list-binarysearch-what-precondition-must-the-list-satisfy)
  - [Q14. How does `List<T>` indexer access compare to `LinkedList<T>` (no indexer)?](#q14-how-does-listt-indexer-access-compare-to-linkedlistt-no-indexer)
  - [Q15. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?](#q15-what-is-comparisont-delegate-and-how-does-it-relate-to-listtsort)

- [04. Dictionary](#04-dictionary)
  - [Q1. What is the difference between `Dictionary<TKey, TValue>` and `Hashtable`?](#q1-what-is-the-difference-between-dictionarytkey-tvalue-and-hashtable)
  - [Q2. Explain `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`.](#q2-explain-idictionarytkey-tvalue-and-ireadonlydictionarytkey-tvalue)
  - [Q3. How does `Dictionary<TKey, TValue>` handle hashing and collisions?](#q3-how-does-dictionarytkey-tvalue-handle-hashing-and-collisions)
  - [Q4. What is the difference between `Dictionary.Add` and the indexer when the key already exists?](#q4-what-is-the-difference-between-dictionaryadd-and-the-indexer-when-the-key-already-exists)
  - [Q5. What is the difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup?](#q5-what-is-the-difference-between-containskey-trygetvalue-and-the-indexer-for-lookup)
  - [Q6. Why must keys be immutable (or stable) after insertion for correct hash table behavior?](#q6-why-must-keys-be-immutable-or-stable-after-insertion-for-correct-hash-table-behavior)
  - [Q7. What exception is thrown when accessing a missing key via the indexer?](#q7-what-exception-is-thrown-when-accessing-a-missing-key-via-the-indexer)
  - [Q8. Can `null` be used as a key when `TKey` is a reference type?](#q8-can-null-be-used-as-a-key-when-tkey-is-a-reference-type)
  - [Q9. What is the average vs worst-case time complexity for lookup, insert, and remove?](#q9-what-is-the-average-vs-worst-case-time-complexity-for-lookup-insert-and-remove)
  - [Q10. What is the hash code contract between `GetHashCode` and `Equals` for custom key types?](#q10-what-is-the-hash-code-contract-between-gethashcode-and-equals-for-custom-key-types)
  - [Q11. What is `IEqualityComparer<TKey>`, and when do you pass a custom comparer to the constructor?](#q11-what-is-iequalitycomparertkey-and-when-do-you-pass-a-custom-comparer-to-the-constructor)
  - [Q12. When would you choose `Dictionary` over `List` for lookups by id or SKU?](#q12-when-would-you-choose-dictionary-over-list-for-lookups-by-id-or-sku)
  - [Q13. What happens internally when two keys hash to the same bucket?](#q13-what-happens-internally-when-two-keys-hash-to-the-same-bucket)

- [05. HashSet](#05-hashset)
  - [Q1. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?](#q1-explain-hashsett-and-its-use-cases-how-is-it-different-from-listt)
  - [Q2. What is the difference between `SortedSet<T>` and `HashSet<T>`?](#q2-what-is-the-difference-between-sortedsett-and-hashsett)
  - [Q3. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?](#q3-why-does-hashsett-require-correct-gethashcodeequals-for-custom-types)
  - [Q4. What set operations does `HashSet<T>` provide (`UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`)?](#q4-what-set-operations-does-hashsett-provide-unionwith-intersectwith-exceptwith-symmetricexceptwith)
  - [Q5. What is the difference between `Add` returning `false` on duplicate vs `List.Add` behavior?](#q5-what-is-the-difference-between-add-returning-false-on-duplicate-vs-listadd-behavior)
  - [Q6. How do you construct a `HashSet<T>` with custom equality (`IEqualityComparer<T>`)?](#q6-how-do-you-construct-a-hashsett-with-custom-equality-iequalitycomparert)
  - [Q7. When would you use `HashSet<T>` for deduplication vs `Distinct()` in LINQ?](#q7-when-would-you-use-hashsett-for-deduplication-vs-distinct-in-linq)
  - [Q8. What is the difference between set membership test in `HashSet` vs scanning a `List`?](#q8-what-is-the-difference-between-set-membership-test-in-hashset-vs-scanning-a-list)
  - [Q9. What is `IsSubsetOf`, `IsSupersetOf`, and `Overlaps` used for?](#q9-what-is-issubsetof-issupersetof-and-overlaps-used-for)
  - [Q10. Can you modify an element in a `HashSet` in place if it affects equality — what goes wrong?](#q10-can-you-modify-an-element-in-a-hashset-in-place-if-it-affects-equality-what-goes-wrong)

- [06. Queue and Stack](#06-queue-and-stack)
  - [Q1. Explain `Queue<T>` and `Stack<T>` vs their non-generic counterparts.](#q1-explain-queuet-and-stackt-vs-their-non-generic-counterparts)
  - [Q2. What is FIFO vs LIFO, and which collection maps to each?](#q2-what-is-fifo-vs-lifo-and-which-collection-maps-to-each)
  - [Q3. What operations does `Queue<T>` expose (`Enqueue`, `Dequeue`, `Peek`, `TryDequeue`, `TryPeek`)?](#q3-what-operations-does-queuet-expose-enqueue-dequeue-peek-trydequeue-trypeek)
  - [Q4. What operations does `Stack<T>` expose (`Push`, `Pop`, `Peek`, `TryPop`)?](#q4-what-operations-does-stackt-expose-push-pop-peek-trypop)
  - [Q5. Why do `Queue` and `Stack` not support random access by index?](#q5-why-do-queue-and-stack-not-support-random-access-by-index)
  - [Q6. How is `Queue<T>` used in breadth-first search (BFS) on a graph or grid?](#q6-how-is-queuet-used-in-breadth-first-search-bfs-on-a-graph-or-grid)
  - [Q7. Why does BFS find shortest paths in unweighted graphs?](#q7-why-does-bfs-find-shortest-paths-in-unweighted-graphs)
  - [Q8. What real-world workflows map naturally to a stack (undo/redo, call stack, DFS)?](#q8-what-real-world-workflows-map-naturally-to-a-stack-undoredo-call-stack-dfs)
  - [Q9. What is the difference between non-generic `Queue`/`Stack` and generic versions regarding boxing?](#q9-what-is-the-difference-between-non-generic-queuestack-and-generic-versions-regarding-boxing)
  - [Q10. When would you use `Queue<T>` over `List<T>` with remove-from-front patterns?](#q10-when-would-you-use-queuet-over-listt-with-remove-from-front-patterns)

- [07. SortedList & SortedDictionary](#07-sortedlist-sorteddictionary)
  - [Q1. What is `SortedList<TKey, TValue>` and `SortedDictionary<TKey, TValue>`? When would you use each?](#q1-what-is-sortedlisttkey-tvalue-and-sorteddictionarytkey-tvalue-when-would-you-use-each)
  - [Q2. What interface defines ordering for sorted collections (`IComparer<TKey>` vs `IEqualityComparer<TKey>`)?](#q2-what-interface-defines-ordering-for-sorted-collections-icomparertkey-vs-iequalitycomparertkey)
  - [Q3. What is the difference between `SortedList` (array-backed) and `SortedDictionary` (tree-backed) performance?](#q3-what-is-the-difference-between-sortedlist-array-backed-and-sorteddictionary-tree-backed-performance)
  - [Q4. When is `SortedList` preferred over `SortedDictionary` for memory or indexed access?](#q4-when-is-sortedlist-preferred-over-sorteddictionary-for-memory-or-indexed-access)
  - [Q5. What is the cost of inserting out-of-order keys into a sorted collection?](#q5-what-is-the-cost-of-inserting-out-of-order-keys-into-a-sorted-collection)
  - [Q6. Can you look up by index in `SortedList` — what does `Keys[index]` provide?](#q6-can-you-look-up-by-index-in-sortedlist-what-does-keysindex-provide)
  - [Q7. What is the difference between `SortedSet<T>` and `SortedDictionary<TKey, TValue>`?](#q7-what-is-the-difference-between-sortedsett-and-sorteddictionarytkey-tvalue)
  - [Q8. When would you choose `SortedDictionary` over sorting keys from a `Dictionary` at read time?](#q8-when-would-you-choose-sorteddictionary-over-sorting-keys-from-a-dictionary-at-read-time)

- [08. IEnumerable & IEnumerator](#08-ienumerable-ienumerator)
  - [Q1. What is the difference between `IEnumerable<T>` and `ICollection<T>`?](#q1-what-is-the-difference-between-ienumerablet-and-icollectiont)
  - [Q2. What is the difference between `ICollection<T>` and `IList<T>`?](#q2-what-is-the-difference-between-icollectiont-and-ilistt)
  - [Q3. What are `IReadOnlyList<T>` and `IReadOnlyCollection<T>`?](#q3-what-are-ireadonlylistt-and-ireadonlycollectiont)
  - [Q4. What is the difference between `IEnumerator` and `IEnumerator<T>`?](#q4-what-is-the-difference-between-ienumerator-and-ienumeratort)
  - [Q5. What is the `yield` keyword, and how do iterator methods relate to `IEnumerable<T>`?](#q5-what-is-the-yield-keyword-and-how-do-iterator-methods-relate-to-ienumerablet)
  - [Q6. What is the difference between deferred execution and immediate execution for IEnumerable sequences?](#q6-what-is-the-difference-between-deferred-execution-and-immediate-execution-for-ienumerable-sequences)
  - [Q7. What is the iterator pattern — what do `MoveNext`, `Current`, and `Reset` do?](#q7-what-is-the-iterator-pattern-what-do-movenext-current-and-reset-do)
  - [Q8. What is `yield break` vs `return` in an iterator method?](#q8-what-is-yield-break-vs-return-in-an-iterator-method)
  - [Q9. Why can multiple enumeration of the same `IEnumerable` from a LINQ query re-run the pipeline?](#q9-why-can-multiple-enumeration-of-the-same-ienumerable-from-a-linq-query-re-run-the-pipeline)
  - [Q10. What is the difference between returning `IEnumerable<T>` from a method vs `List<T>`?](#q10-what-is-the-difference-between-returning-ienumerablet-from-a-method-vs-listt)
  - [Q11. What happens if you modify a collection during `foreach` — how does the enumerator detect it?](#q11-what-happens-if-you-modify-a-collection-during-foreach-how-does-the-enumerator-detect-it)
  - [Q12. What is covariance on `IEnumerable<out T>` — practical assignment examples?](#q12-what-is-covariance-on-ienumerableout-t-practical-assignment-examples)
  - [Q13. What is the difference between `foreach` and manual `while (enumerator.MoveNext())`?](#q13-what-is-the-difference-between-foreach-and-manual-while-enumeratormovenext)
  - [Q14. What is `ToList()` materialization, and when must you materialize before multiple passes?](#q14-what-is-tolist-materialization-and-when-must-you-materialize-before-multiple-passes)
  - [Q15. What is the relationship between `IAsyncEnumerable<T>` and iterators (preview)?](#q15-what-is-the-relationship-between-iasyncenumerablet-and-iterators-preview)
  - [Q16. **Modify while iterating** — Changing a collection during `foreach` throws `InvalidOperationException`.](#q16-modify-while-iterating-changing-a-collection-during-foreach-throws-invalidoperationexception)
  - [Q17. **Mutable keys** — Changing equality-relevant state on a key after insertion causes silent lookup failures.](#q17-mutable-keys-changing-equality-relevant-state-on-a-key-after-insertion-causes-silent-lookup-failures)
  - [Q18. **`IEnumerable<T>` covariant, `List<T>` not** — Covariance on mutable lists would break type safety.](#q18-ienumerablet-covariant-listt-not-covariance-on-mutable-lists-would-break-type-safety)
  - [Q19. **Wrong collection for the job** — Frequent middle inserts on `List<T>` are O(n).](#q19-wrong-collection-for-the-job-frequent-middle-inserts-on-listt-are-on)
  - [Q20. **Static fields on generic types** — Separate static slots per closed generic type.](#q20-static-fields-on-generic-types-separate-static-slots-per-closed-generic-type)
  - [Q21. **Boxing in non-generic collections** — `ArrayList` boxes value types; `List<T>` avoids this.](#q21-boxing-in-non-generic-collections-arraylist-boxes-value-types-listt-avoids-this)
  - [Q22. **Passing `List<T>` by value** — Reference is copied; contents still shared.](#q22-passing-listt-by-value-reference-is-copied-contents-still-shared)
  - [Q23. **`Dictionary.Add` vs indexer on duplicate key** — `Add` throws; indexer overwrites silently.](#q23-dictionaryadd-vs-indexer-on-duplicate-key-add-throws-indexer-overwrites-silently)
  - [Q24. **`AsReadOnly()` is a view** — Original list mutations remain visible through the wrapper.](#q24-asreadonly-is-a-view-original-list-mutations-remain-visible-through-the-wrapper)
  - [Q25. **Assuming dictionary enumeration order** — Undefined; sort keys explicitly if order matters.](#q25-assuming-dictionary-enumeration-order-undefined-sort-keys-explicitly-if-order-matters)
  - [Q26. **Multiple enumeration cost** — `yield`/LINQ re-executes work each pass; materialize when needed.](#q26-multiple-enumeration-cost-yieldlinq-re-executes-work-each-pass-materialize-when-needed)
  - [Q27. **Wrong comparer on sorted types** — `SortedDictionary` uses `IComparer<TKey>`, not `IEqualityComparer<TKey>`.](#q27-wrong-comparer-on-sorted-types-sorteddictionary-uses-icomparertkey-not-iequalitycomparertkey)
  - [Q28. **Poor `GetHashCode` distribution** — Constant hash codes degrade to O(n) buckets.](#q28-poor-gethashcode-distribution-constant-hash-codes-degrade-to-on-buckets)
  - [Q29. **Queue `Contains` is O(n)** — Use a `HashSet` alongside if you need fast membership checks.](#q29-queue-contains-is-on-use-a-hashset-alongside-if-you-need-fast-membership-checks)
  - [Q1. (R) A teammate adds a generic repository helper for warehouse stock rows. `dotnet build` fails. Review the constraint stack — what is wrong, and how do you fix it?](#q1-r-a-teammate-adds-a-generic-repository-helper-for-warehouse-stock-rows-dotnet-build-fails-review-the-constraint-stack-what-is-wrong-and-how-do-you-fix-it)
  - [Q2. (R) A developer "fixes" a method that accepts any payload list by widening to `List<object>`. Review the assignment and call site:](#q2-r-a-developer-fixes-a-method-that-accepts-any-payload-list-by-widening-to-listobject-review-the-assignment-and-call-site)
  - [Q3. (R) An API endpoint helper should return the larger of two comparable stock metrics without boxing value types. Review the call chain:](#q3-r-an-api-endpoint-helper-should-return-the-larger-of-two-comparable-stock-metrics-without-boxing-value-types-review-the-call-chain)
  - [Q4. (M) A hot inventory path stores millions of pallet counts per hour. One service uses `List<object>` "for flexibility"; another uses `List<int>`. Review the read loop:](#q4-m-a-hot-inventory-path-stores-millions-of-pallet-counts-per-hour-one-service-uses-listobject-for-flexibility-another-uses-listint-review-the-read-loop)
  - [Q5. (R) A factory method should default-construct inventory DTOs for an import pipeline. Review:](#q5-r-a-factory-method-should-default-construct-inventory-dtos-for-an-import-pipeline-review)
  - [Q6. (R) A library author exposes typed domain exceptions via generics "so callers can catch exactly what they need." Review:](#q6-r-a-library-author-exposes-typed-domain-exceptions-via-generics-so-callers-can-catch-exactly-what-they-need-review)

- [02. ArrayList](#02-arraylist-1)

- [02. ArrayList](#02-arraylist-2)
  - [Q1. (R) A legacy warehouse service stores pick lines in an `ArrayList`. After a refactor, production throws `InvalidCastException` during the nightly export. Review the code — what failed, and why did it compile?](#q1-r-a-legacy-warehouse-service-stores-pick-lines-in-an-arraylist-after-a-refactor-production-throws-invalidcastexception-during-the-nightly-export-review-the-code-what-failed-and-why-did-it-compile)
  - [Q2. (R) A sensor-ingestion job stores telemetry in an `ArrayList` and unboxes on read. Under load, GC pressure spikes and one pod crashes intermittently. Review the hot path — what is wrong at the storage layer and on read?](#q2-r-a-sensor-ingestion-job-stores-telemetry-in-an-arraylist-and-unboxes-on-read-under-load-gc-pressure-spikes-and-one-pod-crashes-intermittently-review-the-hot-path-what-is-wrong-at-the-storage-layer-and-on-read)
  - [Q3. (R) A catalog API still exposes `IList` for backward compatibility. New code assumes every element is a `Product`. Review this controller helper — what breaks at runtime, and what compile-time safety is missing?](#q3-r-a-catalog-api-still-exposes-ilist-for-backward-compatibility-new-code-assumes-every-element-is-a-product-review-this-controller-helper-what-breaks-at-runtime-and-what-compile-time-safety-is-missing)
  - [Q4. (P) Your team is migrating a .NET Framework inventory module that uses `ArrayList` for product catalogs, `Hashtable` for SKU→bin lookup, and manual `(Product)` casts in every loop. What is your migration plan to modern generic collections, and what do you change first to stop runtime cast failures?](#q4-p-your-team-is-migrating-a-net-framework-inventory-module-that-uses-arraylist-for-product-catalogs-hashtable-for-skubin-lookup-and-manual-product-casts-in-every-loop-what-is-your-migration-plan-to-modern-generic-collections-and-what-do-you-change-first-to-stop-runtime-cast-failures)
  - [Q5. (M) Two implementations compute the same warehouse capacity check. One uses `ArrayList`, one uses `List<int>`. A performance test shows the `ArrayList` version allocates more and runs slower on .NET 8. Explain the mechanism — what happens on each `Add` for value types, and why does `List<int>` avoid it?](#q5-m-two-implementations-compute-the-same-warehouse-capacity-check-one-uses-arraylist-one-uses-listint-a-performance-test-shows-the-arraylist-version-allocates-more-and-runs-slower-on-net-8-explain-the-mechanism-what-happens-on-each-add-for-value-types-and-why-does-listint-avoid-it)
  - [Q6. (D) A monolith has 40 call sites passing `ArrayList` into methods typed as `IList`. Full rewrite to `List<T>` is blocked for two sprints. What incremental strategy reduces `InvalidCastException` risk without a big-bang change, and where do you draw the line on leaving `ArrayList` in place?](#q6-d-a-monolith-has-40-call-sites-passing-arraylist-into-methods-typed-as-ilist-full-rewrite-to-listt-is-blocked-for-two-sprints-what-incremental-strategy-reduces-invalidcastexception-risk-without-a-big-bang-change-and-where-do-you-draw-the-line-on-leaving-arraylist-in-place)

- [03. List](#03-list-1)

- [03. List](#03-list-2)
  - [Q1. (R) A nightly import job loads 500,000 shipment SKUs into a `List<string>` by calling `Add` one at a time in a loop. Memory profiling shows repeated large allocations and GC pressure. Review the pattern below. What is happening internally, and how would you fix it?](#q1-r-a-nightly-import-job-loads-500000-shipment-skus-into-a-liststring-by-calling-add-one-at-a-time-in-a-loop-memory-profiling-shows-repeated-large-allocations-and-gc-pressure-review-the-pattern-below-what-is-happening-internally-and-how-would-you-fix-it)
  - [Q2. (R) A warehouse service removes cancelled dock labels during iteration. In staging it throws intermittently. Review this method — what breaks, and what is the correct fix?](#q2-r-a-warehouse-service-removes-cancelled-dock-labels-during-iteration-in-staging-it-throws-intermittently-review-this-method-what-breaks-and-what-is-the-correct-fix)
  - [Q3. (M) A shipment validator checks whether each incoming pallet's SKU already exists in a queue of 50,000 items by calling `IndexOf` inside a loop. What is the performance problem, and what structure would you use instead?](#q3-m-a-shipment-validator-checks-whether-each-incoming-pallets-sku-already-exists-in-a-queue-of-50000-items-by-calling-indexof-inside-a-loop-what-is-the-performance-problem-and-what-structure-would-you-use-instead)
  - [Q4. (D) Two developers search a pallet-count list for the first value over 20. One uses `List.Find`; the other uses LINQ `FirstOrDefault`. When would you prefer each, and what subtle difference matters for value types?](#q4-d-two-developers-search-a-pallet-count-list-for-the-first-value-over-20-one-uses-listfind-the-other-uses-linq-firstordefault-when-would-you-prefer-each-and-what-subtle-difference-matters-for-value-types)
  - [Q5. (R) A `ShipmentQueueService` exposes its internal lane list directly to API callers. Review the property and usage — what can go wrong in production, and how would you expose the data safely?](#q5-r-a-shipmentqueueservice-exposes-its-internal-lane-list-directly-to-api-callers-review-the-property-and-usage-what-can-go-wrong-in-production-and-how-would-you-expose-the-data-safely)
  - [Q6. (P) A singleton background worker and several API threads share one static `List<ShipmentItem>` for the live shipment queue. Under load, counts become wrong and the process occasionally throws. Explain why `List<T>` is unsafe here and what pattern you would use instead.](#q6-p-a-singleton-background-worker-and-several-api-threads-share-one-static-listshipmentitem-for-the-live-shipment-queue-under-load-counts-become-wrong-and-the-process-occasionally-throws-explain-why-listt-is-unsafe-here-and-what-pattern-you-would-use-instead)

- [04. Dictionary](#04-dictionary-1)

- [04. Dictionary](#04-dictionary-2)
  - [Q1. (R) A hot-path SKU lookup uses `ContainsKey` followed by the indexer. Review this warehouse catalog access. What is inefficient, and how would you improve it?](#q1-r-a-hot-path-sku-lookup-uses-containskey-followed-by-the-indexer-review-this-warehouse-catalog-access-what-is-inefficient-and-how-would-you-improve-it)
  - [Q2. (R) A team uses a custom class as the dictionary key and mutates it after insert. Lookups start failing intermittently in production. Review this catalog code:](#q2-r-a-team-uses-a-custom-class-as-the-dictionary-key-and-mutates-it-after-insert-lookups-start-failing-intermittently-in-production-review-this-catalog-code)
  - [Q3. (P) An ASP.NET Core API caches product details in a shared `Dictionary<string, Product>` field on a singleton service. Under load tests, responses are wrong and the process occasionally throws `InvalidOperationException`. Review the cache:](#q3-p-an-aspnet-core-api-caches-product-details-in-a-shared-dictionarystring-product-field-on-a-singleton-service-under-load-tests-responses-are-wrong-and-the-process-occasionally-throws-invalidoperationexception-review-the-cache)
  - [Q4. (R) A REST endpoint maps query parameters directly into dictionary lookups without null checks. Review the handler:](#q4-r-a-rest-endpoint-maps-query-parameters-directly-into-dictionary-lookups-without-null-checks-review-the-handler)
  - [Q5. (D) A microservice adds a static in-memory cache so repeated HTTP fetches are fast. After two weeks in production, pods hit OOM kills even though traffic is steady. Review the cache:](#q5-d-a-microservice-adds-a-static-in-memory-cache-so-repeated-http-fetches-are-fast-after-two-weeks-in-production-pods-hit-oom-kills-even-though-traffic-is-steady-review-the-cache)
  - [Q6. (P) A developer avoids `ConcurrentDictionary` and hand-rolls lazy initialization with `TryGetValue`. Under load, the expensive factory runs twice for the same key. Review:](#q6-p-a-developer-avoids-concurrentdictionary-and-hand-rolls-lazy-initialization-with-trygetvalue-under-load-the-expensive-factory-runs-twice-for-the-same-key-review)

- [05. HashSet](#05-hashset-1)

- [05. HashSet](#05-hashset-2)
  - [Q1. (R) A nightly tag-import job deduplicates article tags with `List<string>.Contains` before insert. Review the hot path:](#q1-r-a-nightly-tag-import-job-deduplicates-article-tags-with-liststringcontains-before-insert-review-the-hot-path)
  - [Q2. (R) A newsletter service deduplicates subscribers by email but keeps seeing duplicate sends in logs. Review:](#q2-r-a-newsletter-service-deduplicates-subscribers-by-email-but-keeps-seeing-duplicate-sends-in-logs-review)
  - [Q3. (R) After a profile-update feature ships, support reports "user already subscribed" errors even when lookup fails. Review:](#q3-r-after-a-profile-update-feature-ships-support-reports-user-already-subscribed-errors-even-when-lookup-fails-review)
  - [Q4. (R) An editorial dashboard merges article tag sets for a "shared topics" widget. Case variants appear twice after deploy. Review:](#q4-r-an-editorial-dashboard-merges-article-tag-sets-for-a-shared-topics-widget-case-variants-appear-twice-after-deploy-review)
  - [Q5. (R) A publish pipeline accidentally wipes an editor's working tag pool. Review the merge step:](#q5-r-a-publish-pipeline-accidentally-wipes-an-editors-working-tag-pool-review-the-merge-step)
  - [Q6. (R) A custom comparer passes code review but `Remove` and `Contains` behave inconsistently. Review:](#q6-r-a-custom-comparer-passes-code-review-but-remove-and-contains-behave-inconsistently-review)

- [06. Queue and Stack](#06-queue-and-stack-1)

- [06. Queue and Stack](#06-queue-and-stack-2)
  - [Q1. (R) A help-desk service was refactored from `Queue<SupportTicket>` to `Stack<SupportTicket>` "because stacks are faster." Review the handler loop. What ordering bug appears in production, and how do you fix it?](#q1-r-a-help-desk-service-was-refactored-from-queuesupportticket-to-stacksupportticket-because-stacks-are-faster-review-the-handler-loop-what-ordering-bug-appears-in-production-and-how-do-you-fix-it)
  - [Q2. (R) A background worker drains a print queue when the upstream publisher is idle. Under load, the service logs unhandled `InvalidOperationException` and the host restarts. Review the consumer:](#q2-r-a-background-worker-drains-a-print-queue-when-the-upstream-publisher-is-idle-under-load-the-service-logs-unhandled-invalidoperationexception-and-the-host-restarts-review-the-consumer)
  - [Q3. (P) Three ASP.NET Core request threads enqueue audit events; one background `IHostedService` dequeues them for batch upload. The team shares one `Queue<AuditEvent>` instance registered as a **Singleton**. Occasionally events disappear or `InvalidOperationException` appears under concurrent `Enqueue`/`Dequeue`. Explain why `Queue<T>` is unsafe here and what you would register instead.](#q3-p-three-aspnet-core-request-threads-enqueue-audit-events-one-background-ihostedservice-dequeues-them-for-batch-upload-the-team-shares-one-queueauditevent-instance-registered-as-a-singleton-occasionally-events-disappear-or-invalidoperationexception-appears-under-concurrent-enqueuedequeue-explain-why-queuet-is-unsafe-here-and-what-you-would-register-instead)
  - [Q4. (M) A developer rewrites maze pathfinding from the chapter's BFS to recursive DFS. On large grids the process terminates with `StackOverflowException`. They propose "just use `Stack<T>` instead of recursion." Review both approaches:](#q4-m-a-developer-rewrites-maze-pathfinding-from-the-chapters-bfs-to-recursive-dfs-on-large-grids-the-process-terminates-with-stackoverflowexception-they-propose-just-use-stackt-instead-of-recursion-review-both-approaches)
  - [Q5. (D) Your team must pick a frontier collection for two graph tasks on an unweighted social network: (A) find **shortest path** in friend hops from user A to user B, and (B) detect whether a **cycle** exists in a follow graph (direction matters). One engineer says "both are graph search — use `Stack<T>` for both." What would you choose for each task and why?](#q5-d-your-team-must-pick-a-frontier-collection-for-two-graph-tasks-on-an-unweighted-social-network-a-find-shortest-path-in-friend-hops-from-user-a-to-user-b-and-b-detect-whether-a-cycle-exists-in-a-follow-graph-direction-matters-one-engineer-says-both-are-graph-search-use-stackt-for-both-what-would-you-choose-for-each-task-and-why)
  - [Q6. (R) A response editor copied from the chapter's `HelpDeskSession` mixes undo (`Stack<string>`) with ticket draining. Review this merge:](#q6-r-a-response-editor-copied-from-the-chapters-helpdesksession-mixes-undo-stackstring-with-ticket-draining-review-this-merge)

- [07. SortedList & SortedDictionary](#07-sortedlist-sorteddictionary-1)

- [07. SortedList & SortedDictionary](#07-sortedlist-sorteddictionary-2)
  - [Q1. (R) A warehouse dashboard prints the lowest and highest SKU from a live price map. Review:](#q1-r-a-warehouse-dashboard-prints-the-lowest-and-highest-sku-from-a-live-price-map-review)
  - [Q2. (R) An inventory sync service upserts pallet counts every few seconds. Review the hot path:](#q2-r-an-inventory-sync-service-upserts-pallet-counts-every-few-seconds-review-the-hot-path)
  - [Q3. (D) You expose a `/regions/sales` JSON endpoint. Product wants keys returned alphabetically by region code. Two proposals:](#q3-d-you-expose-a-regionssales-json-endpoint-product-wants-keys-returned-alphabetically-by-region-code-two-proposals)
  - [Q4. (R) A catalog search feature stores product tags in a case-insensitive sorted map. QA reports duplicate logical tags after a Turkish-locale server deploy. Review:](#q4-r-a-catalog-search-feature-stores-product-tags-in-a-case-insensitive-sorted-map-qa-reports-duplicate-logical-tags-after-a-turkish-locale-server-deploy-review)
  - [Q5. (M) A pricing microservice benchmarks three shapes for a nightly job that inserts 50_000 random SKUs once, then performs 500_000 lookups:](#q5-m-a-pricing-microservice-benchmarks-three-shapes-for-a-nightly-job-that-inserts-50_000-random-skus-once-then-performs-500_000-lookups)
  - [Q6. (R) A developer ports a `Dictionary` helper to sorted collections but copies the wrong comparer interface. Review:](#q6-r-a-developer-ports-a-dictionary-helper-to-sorted-collections-but-copies-the-wrong-comparer-interface-review)

- [08. IEnumerable & IEnumerator](#08-ienumerable-ienumerator-1)

- [08. IEnumerable & IEnumerator](#08-ienumerable-ienumerator-2)
  - [Q1. (R) A warehouse API returns `IEnumerable<PickLine>` from a `yield return` filter. A report job calls `Count()` then `Sum()` on the same reference without materializing. Totals disagree with the pick ticket and logs show the database query ran twice. Review the service method and caller. What went wrong, and how do you fix it?](#q1-r-a-warehouse-api-returns-ienumerablepickline-from-a-yield-return-filter-a-report-job-calls-count-then-sum-on-the-same-reference-without-materializing-totals-disagree-with-the-pick-ticket-and-logs-show-the-database-query-ran-twice-review-the-service-method-and-caller-what-went-wrong-and-how-do-you-fix-it)
  - [Q2. (R) A custom `IEnumerator<PickLine>` wraps a file reader. A developer copies the manual loop from a tutorial but drops the `using` block. Under load, temp files pile up on disk. Review the loop. What is missing, and what does `foreach` do differently?](#q2-r-a-custom-ienumeratorpickline-wraps-a-file-reader-a-developer-copies-the-manual-loop-from-a-tutorial-but-drops-the-using-block-under-load-temp-files-pile-up-on-disk-review-the-loop-what-is-missing-and-what-does-foreach-do-differently)
  - [Q3. (R) A batch-picking screen tries to skip short lines by removing them while iterating. It crashes on the second line every time. Review the loop (same pattern as **Program.cs** Section 4g). What throws, why is it allowed, and what is the safe fix?](#q3-r-a-batch-picking-screen-tries-to-skip-short-lines-by-removing-them-while-iterating-it-crashes-on-the-second-line-every-time-review-the-loop-same-pattern-as-programcs-section-4g-what-throws-why-is-it-allowed-and-what-is-the-safe-fix)
  - [Q4. (M) A developer builds a lazy LINQ pipeline over live pick lines, logs the count, then mutates the underlying list before a second `foreach`. Results differ between the two passes. Walk through what runs when and why the second pass can change.](#q4-m-a-developer-builds-a-lazy-linq-pipeline-over-live-pick-lines-logs-the-count-then-mutates-the-underlying-list-before-a-second-foreach-results-differ-between-the-two-passes-walk-through-what-runs-when-and-why-the-second-pass-can-change)
  - [Q5. (M) An iterator method logs each SKU as it yields. A caller breaks out of `foreach` after the first match. Later code assumes every line was scanned. Review the iterator and caller. What does `yield return` guarantee about execution state, and when does work *not* run?](#q5-m-an-iterator-method-logs-each-sku-as-it-yields-a-caller-breaks-out-of-foreach-after-the-first-match-later-code-assumes-every-line-was-scanned-review-the-iterator-and-caller-what-does-yield-return-guarantee-about-execution-state-and-when-does-work-not-run)
  - [Q6. (P) A code review flags `var lines = GetHeavyLines(...).ToList()` as "unnecessary allocation." The author argues it prevents double DB hits and stabilizes results if the ticket changes mid-request. When is `ToList()` (or `ToArray()`) the right production fix for `IEnumerable<T>`, and when is it waste?](#q6-p-a-code-review-flags-var-lines-getheavylinestolist-as-unnecessary-allocation-the-author-argues-it-prevents-double-db-hits-and-stabilizes-results-if-the-ticket-changes-mid-request-when-is-tolist-or-toarray-the-right-production-fix-for-ienumerablet-and-when-is-it-waste)
  - [Q7. (R) Two developers iterate the same `PickBatch` concurrently — one with `foreach`, one with a stored `IEnumerator<PickLine>` from an earlier `GetEnumerator()` call. Intermittent duplicates and skipped SKUs appear. Review `PickBatch` (fresh enumerator per `GetEnumerator()`). What contract did the second developer violate, and how should multiple consumers walk the same batch?](#q7-r-two-developers-iterate-the-same-pickbatch-concurrently-one-with-foreach-one-with-a-stored-ienumeratorpickline-from-an-earlier-getenumerator-call-intermittent-duplicates-and-skipped-skus-appear-review-pickbatch-fresh-enumerator-per-getenumerator-what-contract-did-the-second-developer-violate-and-how-should-multiple-consumers-walk-the-same-batch)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Generics

#### Q1. What are generics in C#? Why were they introduced?

(R) A teammate adds a generic repository helper for warehouse stock rows. `dotnet build` fails. Review the constraint stack — what is wrong, and how do you fix it?

**Answer:** `where T : struct, StockEntry, new()` is illegal — a type parameter cannot be both a non-nullable value type (`struct`) and a reference-type base class (`StockEntry`). The compiler rejects the constraint combination before any call site is evaluated.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `struct` + base class `StockEntry` on same `T` | CS0454 — mutually exclusive constraints; build blocked |
| Design | `_cache` stores `StockEntry` but method returns `T` with value-type constraint | Even if it compiled, boxing/unified cache semantics would be wrong |
| API misuse | `new T { Sku = sku }` assumes `T` is a reference type with mutable `Sku` | Value-type `T` could not inherit `StockEntry` anyway |

**Fix (priority order):**

1. Drop `struct` — use `where T : StockEntry, new()` if you truly need default-constructible inventory rows (`InventoryItem`, etc.).
2. If value-type rows are required, do **not** inherit `StockEntry`; use a separate generic struct path (e.g., `Quantity<TUnit> where TUnit : struct`) or a shared interface instead of a class base.
3. Type the cache as `Dictionary<string, T>` inside a generic class `StockRepository<T> where T : StockEntry, new()`, not a mixed `Dictionary<string, StockEntry>` with an inconsistent method signature.
4. Align with this chapter's `DescribeStockEntry<T> where T : StockEntry` — base-class constraints apply to reference types in the inheritance hierarchy.

**Production takeaway:** Constraint misuse is a compile-time gate — Karat tests whether you recognize that `class`/base-type and `struct` constraints exclude each other. See **Program.cs** Sections 7–9 — constraint combinations.

---

#### Q2. What is the difference between generic and non-generic collections?

(R) A developer "fixes" a method that accepts any payload list by widening to `List<object>`. Review the assignment and call site:

**Answer:** `List<T>` is **invariant** — `List<string>` is not assignable to `List<object>` because that would allow adding non-strings through the wider reference. Covariance applies only on interfaces like `IEnumerable<out T>` for **read-only** projection, not on mutable lists.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `AuditSkus(warehouseSkus)` with `List<object>` parameter | CS1503 — cannot convert `List<string>` to `List<object>` |
| Runtime | Cast `IEnumerable<object>` to `List<object>` and `Add(42)` | `InvalidCastException` — sequence is backed by `List<string>`, not `List<object>` |
| Design | Treating covariance as "free widening" for mutable collections | Silent data corruption if the language allowed it — type safety violated |

**Fix (priority order):**

1. For read-only aggregation, accept `IEnumerable<string>` or `IReadOnlyList<string>` — precise, no widening needed.
2. For heterogeneous payloads, use `List<object>` at the **source** (accept the boxing cost consciously) or a discriminated model (`List<StockPayload>` / union type).
3. Use `IEnumerable<object> widened = warehouseSkus` only when consuming items — never cast back to a mutable `List<object>` to add elements.
4. Remember: `IEnumerable<out T>` covariance lets you pass `IEnumerable<string>` where `IEnumerable<object>` is expected, but you still cannot mutate element types.

**Production takeaway:** Confusing `List<T>` invariance with `IEnumerable<out T>` covariance is a common review failure — matches **Program.cs** Section 13 and Quick Reference variance rows.

---

#### Q3. Explain generic constraints in C# (`where` clause) with examples.

(R) An API endpoint helper should return the larger of two comparable stock metrics without boxing value types. Review the call chain:

**Answer:** Generic method inference requires a **single** type argument `T` that fits both parameters — `decimal` and `int` disagree, so the compiler cannot infer `T` (CS0411). Forcing `MaxOf<decimal>` then fails because `int` is not implicitly convertible to `decimal` at the call site (CS1503).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `MaxOf(priceA, unitsB)` — mismatched argument types | CS0411 — type arguments cannot be inferred from the arguments |
| Compile | `MaxOf<decimal>(priceA, unitsB)` | CS1503 — `int` cannot be passed where `decimal` expected |
| Design | One generic `MaxOf<T>` used across unrelated metrics | API encourages comparing apples to units — domain error masked as generic error |

**Fix (priority order):**

1. Call with **homogeneous** types: `MaxOf(priceA, otherPrice)` or `MaxOf(unitsA, unitsB)`.
2. If conversion is intentional, convert explicitly **before** the call: `MaxOf(priceA, (decimal)unitsB)` — documents that the comparison is cross-domain and may be wrong business-wise.
3. Prefer domain methods (`MaxPrice`, `MaxUnits`) or `INumber<T>` (.NET 7+) helpers where numeric widening is well-defined.
4. Do not rely on inference when types differ — specify intent at the call site or split overloads.

**Production takeaway:** Inference failures often signal a design smell — Karat checks that you read CS0411/CS1503 as "one T for all parameters," not as a compiler bug. See **Program.cs** Section 8 — `Swap<T>` inference requires matching types.

---

#### Q4. Explain covariance and contravariance in generics (`in` and `out` keywords).

(M) A hot inventory path stores millions of pallet counts per hour. One service uses `List<object>` "for flexibility"; another uses `List<int>`. Review the read loop:

**Answer:** For each closed constructed type, the JIT specializes `List<T>.Add` and indexer access — `List<int>` stores unboxed ints in a `T[]` with no per-element heap boxing, while `List<object>` boxes every `int` on `Add` and unboxes on read, doubling heap traffic and cache pressure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / GC | Boxing 1M ints into `List<object>` | 1M heap allocations + GC pressure; slower hot loop |
| Runtime | Unbox + cast in `legacySum` loop | Extra CPU per iteration vs direct `int` access |
| JIT | Shared vs specialized code paths | `List<int>` gets efficient `int[]` storage; `List<object>` always handles references |
| Design | "Flexibility" on a numeric hot path | Latency spikes under load; harder to reason about in profiling |

**Fix (priority order):**

1. Use `List<int>` (or `Span<int>`, `int[]`, `ImmutableArray<int>`) for homogeneous numeric streams — matches **LegacyCollectionProbe** vs `List<int>` in **Program.cs** Section 1.
2. If mixed types are required, isolate boxing to boundaries (parse → strongly typed model) rather than the inner loop.
3. Accept `List<object>` only at integration seams (legacy APIs, `ArrayList` interop) with explicit conversion at the edge.
4. Profile with dotMemory / PerfView — boxed collections show as `System.Int32` allocations in GC heaps.

**Production takeaway:** Generics exist partly to eliminate boxing on value-type collections — Layer 2 tests whether you connect language feature to production GC behavior, not just "compile-time safety."

---

#### Q5. Can you use `where T : Enum` or `where T : unmanaged`? What problems do these solve?

(R) A factory method should default-construct inventory DTOs for an import pipeline. Review:

**Answer:** `CreateRow<ImportedLine>()` succeeds — records with a primary constructor still get a synthesized parameterless constructor for `new()` when not explicitly removed. `CreateRow<PalletTag>()` fails — `PalletTag` only declares `PalletTag(int zoneId)`, so it does not satisfy `where T : new()` (CS0310).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `CreateRow<PalletTag>()` | CS0310 — `PalletTag` must have public parameterless constructor |
| Design | `new()` constraint on a factory used for both records and custom structs | Call sites look uniform but only some types qualify |
| Runtime / API | Mutating `row.Sku` on a record instance | Works here, but immutable record designs may prefer `with` instead of post-`new()` mutation |

**Fix (priority order):**

1. For `PalletTag`, add an explicit parameterless ctor **only if** default construction is valid: `public PalletTag() : this(0) { }` — or stop using `new()` for that type.
2. Split factories: `CreateRecord<T>() where T : new()` for DTOs; dedicated `PalletTag CreateTag(int zoneId)` for parameterized structs.
3. Prefer `Activator.CreateInstance<T>()` or DI-backed factories when construction needs parameters or injection — `new()` is for simple default graphs only.
4. Validate at compile time with tests that call `CreateRow<T>()` for every supported import row type.

**Production takeaway:** The `new()` constraint means "public parameterless constructor exists" — not "any struct" or "any record." See **Program.cs** `CreateDefault<T>() where T : new()` and Section 9a.

---

#### Q6. What happens when you use `default(T)` on an unconstrained type parameter?

(R) A library author exposes typed domain exceptions via generics "so callers can catch exactly what they need." Review:

**Answer:** `throw new TException()` where `TException : Exception, new()` produces **parameterless** exceptions with no message, no inner exception, and no structured context — callers catch the right type but lose SKU, quantity, and stack context. Reusing the helper for `InvalidOperationException` by passing `0` to `EnsurePositive` is a semantic hack that obscures intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Generic exception factory anti-pattern | Empty exceptions — useless logs and support tickets |
| Observability | `new TException()` only — no message/data | `_logger.LogWarning(ex, …)` has nothing actionable; APM groups by type only |
| API contract | `EnsurePositive<InvalidOperationException>(0)` for unknown SKU | Wrong exception type and wrong guard — conflates validation with business rules |
| Maintainability | Callers depend on **type** not **error shape** | Adding fields/codes requires new exception types instead of stable error codes |

**Fix (priority order):**

1. Throw **specific, constructed** exceptions: `throw new ArgumentOutOfRangeException(nameof(units), units, "Units must be positive.");`
2. Replace generic throw helpers with domain exceptions (`UnknownSkuException`) or `Result`/validation types for expected failures.
3. Use `ExceptionDispatchInfo` or `throw;` to preserve stack when rethrowing — never `throw new TException()` as a stand-in for wrapping.
4. For libraries, document thrown types in XML docs; avoid letting consumers catch generic `TException` via your helper.

```csharp
if (units <= 0)
{
    throw new ArgumentOutOfRangeException(nameof(units), units, "Reserve quantity must be positive.");
}

if (!IsKnownSku(sku))
{
    throw new InvalidOperationException($"SKU '{sku}' is not in the catalog.");
}
```

**Production takeaway:** Generics + `new()` on exceptions looks clever but fights .NET exception design — production code favors explicit throws with messages and structured error models. See foundation **Exception Handling** — `throw` vs `throw ex` for stack preservation when rethrowing.

---

---

#### Q7. Why can't you write `T value = null;` unless `T` is constrained to `class`?

_Answer not found._

---

#### Q8. What is the difference between a generic class and a generic method?

_Answer not found._

---

#### Q9. What's the difference between reflection over an open generic type (`List<>`) and a closed generic type (`List<int>`)?

_Answer not found._

---

#### Q10. Why does `typeof(List<int>) == typeof(List<string>)` return `false`, and how do you get the shared generic type definition?

_Answer not found._

---

#### Q11. What is type erasure vs reification — does C# retain generic type information at runtime?

_Answer not found._

---

#### Q12. What constraints allow calling `new T()` — what does `where T : new()` enable?

_Answer not found._

---

#### Q13. What is the difference between `where T : class` and `where T : struct` constraints?

_Answer not found._

---

#### Q14. What does `where T : notnull` mean for nullable reference type analysis?

_Answer not found._

---

#### Q15. Why are generic value types separate closed types at runtime for static fields?

_Answer not found._

---

#### Q16. What is covariance on `IEnumerable<out T>` — why can you assign `IEnumerable<string>` to `IEnumerable<object>`?

_Answer not found._

---

#### Q17. What is contravariance on `Action<in T>` / `IComparer<in T>`?

_Answer not found._

---

#### Q18. Why is `List<T>` neither covariant nor contravariant on `T`?

_Answer not found._

---

#### Q19. What is the difference between generic specialization performance for value types vs reference types?

_Answer not found._

---

#### Q20. Can you cast from `List<string>` to `List<object>` — what error or exception occurs?

_Answer not found._

---

### 02. ArrayList

#### Q1. What is the difference between `Array` and `ArrayList`?

(R) A legacy warehouse service stores pick lines in an `ArrayList`. After a refactor, production throws `InvalidCastException` during the nightly export. Review the code — what failed, and why did it compile?

```csharp
ArrayList warehouseLines = LoadLinesFromDatabase(); // returns mixed legacy rows

decimal totalValue = 0m;
foreach (object entry in warehouseLines)
{
    Product product = (Product)entry;
    totalValue += product.ProductPrice;
}
```

A teammate added this line to support rush SKUs before the export job runs:

```csharp
warehouseLines.Add("RUSH-PICK");
```

**Answer:** The export loop assumes every `ArrayList` element is a `Product`, but `Add("RUSH-PICK")` stores a `string` — the cast `(Product)entry` throws `InvalidCastException` at runtime because `ArrayList.Add` accepts any `object` with no compile-time type check.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Type safety | `ArrayList` allows heterogeneous `Add` | Wrong runtime type slips in; compile succeeds |
| Runtime | `(Product)entry` on a `string` | `InvalidCastException` — nightly job fails |
| Design | Mixed domain types in one bag | Same fragility as **Program.cs** Section 5 CRUD demo (`Add(250)` beside `Product`) |
| Maintainability | Implicit contract "all items are Product" | No compiler enforcement; code review must catch bad `Add` |

**Fix (priority order):**

1. Remove the string from the product list — store rush flags on `Product` or use a separate collection.
2. Migrate `warehouseLines` to `List<Product>` so `Add("RUSH-PICK")` fails at compile time.
3. Short-term guard: use pattern matching (`entry is Product p`) and log/skip invalid rows instead of blind cast — stops the crash but hides data quality issues.
4. Add an integration test that runs the export against a fixture mirroring legacy mixed data.

**Production takeaway:** `ArrayList` defers type errors to production — Karat uses this to test whether you connect "it compiled" with "nothing checked the element type." See **Program.cs** Sections 2 and 7 — indexer and `foreach` return `object`.

---

#### Q2. What is the difference between `List<T>` and `ArrayList`?

(R) A sensor-ingestion job stores telemetry in an `ArrayList` and unboxes on read. Under load, GC pressure spikes and one pod crashes intermittently. Review the hot path — what is wrong at the storage layer and on read?

```csharp
ArrayList readings = new ArrayList(capacity: 10_000);

for (int i = 0; i < 10_000; i++)
{
    readings.Add(i); // sensor count snapshot
}

int peak = (long)readings[0]; // "fix" after a code review comment
```

**Answer:** Each `Add(i)` boxes the `int` onto the heap, creating 10,000 extra allocations and GC pressure; the read then uses `(long)` on a boxed `int`, which throws `InvalidCastException` because unboxing requires the exact original type — you cannot unbox a boxed `int` directly to `long`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Boxing every `int` on `Add` | Heap allocations, GC churn under load |
| Runtime | `(long)readings[0]` unboxes boxed `int` as `long` | `InvalidCastException` — intermittent pod crash |
| API misuse | `ArrayList` for homogeneous numeric telemetry | Wrong tool when all elements are `int` |
| "Fix" regression | Widening cast on unbox | Confuses numeric widening with unboxing rules |

**Fix (priority order):**

1. Replace with `List<int>` — no boxing for value-type elements; indexer returns `int` directly.
2. Correct read: `int peak = (int)readings[0]!` only if staying on `ArrayList`; prefer `List<int>` so no cast is needed.
3. If values can exceed `int`, use `List<long>` from the start — store the wider type without boxing.
4. Profile Gen0/Gen1 collections after migration to confirm allocation drop.

**Production takeaway:** Boxing is invisible in small demos but measurable in hot loops — Karat pairs GC symptoms with the `Add(object)` signature. See **Program.cs** Section 9 — boxing on `Add(42)` and correct `(int)` unbox.

---

#### Q3. Why is `ArrayList` considered a legacy collection in modern C#?

(R) A catalog API still exposes `IList` for backward compatibility. New code assumes every element is a `Product`. Review this controller helper — what breaks at runtime, and what compile-time safety is missing?

```csharp
public decimal GetCatalogTotal(IList catalog)
{
    decimal total = 0m;
    for (int i = 0; i < catalog.Count; i++)
    {
        total += ((Product)catalog[i]!).ProductPrice;
    }
    return total;
}

// Caller from legacy batch job:
IList legacyCatalog = new ArrayList
{
    new Product { ProductNo = 10, ProductName = "Scanner", ProductPrice = 89.50m },
    250 // legacy quantity field stored inline before Product migration
};
GetCatalogTotal(legacyCatalog);
```

**Answer:** Index 1 holds a boxed `int` (250), not a `Product` — `((Product)catalog[i]!)` throws `InvalidCastException` on the second iteration. The method compiles because `IList` indexer returns `object?` and the cast is explicit; no compile-time guarantee exists that callers populated the list homogeneously.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Contract | `IList` accepts any element type | Callers can pass legacy mixed `ArrayList` |
| Runtime | Cast `(Product)` on boxed `int` | API call fails mid-loop |
| API design | Non-generic `IList` parameter | Hides intended element type from callers and reviewers |
| Migration debt | Legacy row shape (`250` inline) coexists with `Product` | Data migration incomplete but new code assumes completion |

**Fix (priority order):**

1. Change signature to `IReadOnlyList<Product>` or `List<Product>` — mixed `Add` fails at compile time on the caller side when they migrate.
2. Add a dedicated DTO mapper at the legacy boundary that converts raw rows to `Product` before calling business logic — never pass raw `ArrayList` into domain code.
3. Interim: validate with `catalog[i] is Product` and throw a descriptive error listing index and runtime type.
4. Deprecate `GetCatalogTotal(IList)` once batch jobs are updated; track call sites.

**Production takeaway:** Programming against non-generic `IList`/`ICollection` was necessary pre-generics — modern code should not treat it as a typed list. See **Program.cs** Section 8 — `IList` polymorphism and Section 10 — catalog with manual casts.

---

#### Q4. What boxing occurs when storing `int` values in an `ArrayList`?

(P) Your team is migrating a .NET Framework inventory module that uses `ArrayList` for product catalogs, `Hashtable` for SKU→bin lookup, and manual `(Product)` casts in every loop. What is your migration plan to modern generic collections, and what do you change first to stop runtime cast failures?

**Answer:** Migrate at the boundaries first — replace internal storage with `List<Product>` and `Dictionary<string, string>` (or appropriate typed keys/values), then narrow public APIs from `IList`/`Hashtable` to generic interfaces so new code cannot inject wrong types; leave thin adapter shims for external callers until call sites are updated.

- **Phase 1 — stop the bleeding:** Identify hot paths throwing `InvalidCastException` (catalog totals, export loops). Convert those `ArrayList` instances to `List<Product>` at the point of creation; map legacy rows in one factory method rather than scattering casts.
- **Phase 2 — keyed lookup:** Replace `Hashtable skuToBin` with `Dictionary<string, string>` — eliminates boxing on value types and `as`/cast on values. See **Program.cs** Section 12.
- **Phase 3 — API surface:** Change method parameters from `IList` to `IReadOnlyList<Product>` or `IEnumerable<Product>`; keep obsolete overloads that copy into `List<Product>` with validation for remaining legacy callers.
- **Phase 4 — satellite types:** Migrate `Stack`/`Queue`/`SortedList` usages to `Stack<T>`, `Queue<T>`, `SortedList<TKey,TValue>` as touched (Section 13 preview).
- **Testing:** Characterization tests with production-like mixed `ArrayList` fixtures; assert migrated code either rejects bad rows or maps them explicitly — never silent cast.
- **Do not big-bang** every file — migrate by vertical slice (catalog service end-to-end) so each PR is deployable.

**Production takeaway:** Migration priority is runtime cast failures and public boundaries, not alphabetical file renames — Karat tests whether you know *where* generics buy safety first.

---

#### Q5. What is the performance cost of repeated boxing/unboxing in hot loops using `ArrayList`?

(M) Two implementations compute the same warehouse capacity check. One uses `ArrayList`, one uses `List<int>`. A performance test shows the `ArrayList` version allocates more and runs slower on .NET 8. Explain the mechanism — what happens on each `Add` for value types, and why does `List<int>` avoid it?

**Answer:** `ArrayList.Add` takes `object`, so each `int` is boxed into a separate heap object stored in the internal `object[]`; `List<int>` stores ints directly in its `T[]` backing array with no boxing because the generic type parameter is known at compile time.

- **`ArrayList.Add(i)`:** `int` → boxed `object` (heap allocation + copy) → reference stored in `object[]`. 50,000 iterations ⇒ 50,000 box allocations plus array resizing copies.
- **`List<int>.Add(i)`:** `int` written inline into `int[]` — same amortized growth strategy as `ArrayList`, but no per-element heap wrapper.
- **Read path:** `ArrayList` indexer returns `object` → unbox cast; `List<int>` indexer returns `int` — fewer instructions, no unbox.
- **GC:** Boxed objects are short-lived Gen0 garbage; high-frequency adds inflate collection frequency and cache pressure — matches the pod/GC story in Q2.
- **Capacity hint:** Both honor initial capacity (`new ArrayList(50_000)` / `new List<int>(50_000)`) to reduce resize copies — boxing cost remains unique to `ArrayList` for value types.

**Production takeaway:** Same Big-O for `Add`, different constant factors and allocation profile — Karat expects you to name boxing/unboxing, not just "generics are faster." See **Program.cs** Sections 4 and 11 — capacity behavior and `ArrayList` vs `List<T>` comparison table.

---

#### Q6. Can you store mixed types in an `ArrayList`, and what typing risks does that create?

(D) A monolith has 40 call sites passing `ArrayList` into methods typed as `IList`. Full rewrite to `List<T>` is blocked for two sprints. What incremental strategy reduces `InvalidCastException` risk without a big-bang change, and where do you draw the line on leaving `ArrayList` in place?

**Answer:** Introduce typed wrappers and validated adapters at the edges — new code accepts `IReadOnlyList<T>`; legacy `ArrayList` flows through a single conversion layer that validates or maps elements — and freeze new `ArrayList` usage via analyzer or review rule while migrating call sites by module.

- **Immediate guardrails:** Ban new `ArrayList`/`new ArrayList()` in product code (Roslyn analyzer or `.editorconfig` convention); allow only in the compatibility adapter project.
- **Adapter pattern:** `static List<Product> ToProductList(IList legacy)` — foreach with `is Product` check; throw `InvalidOperationException` with index and type on first bad element (fail fast at boundary, not deep in business logic).
- **Strangler order:** Migrate leaf utilities with no downstream `IList` exports first; then services; public API last — each sprint removes a cluster of call sites, not random files.
- **Interface bridge:** Obsolete `void Process(IList items)` → add `Process(IReadOnlyList<Product> items)`; old overload converts via adapter and logs `[Obsolete]` warning to track remaining callers.
- **Draw the line — keep `ArrayList` temporarily only:** inside isolated interop with external legacy binaries you cannot change, or serialized blobs you have not migrated yet — never in new domain logic.
- **Do not** half-migrate by sprinkling `(Product)` casts — that preserves runtime risk; centralize casts once.

**Production takeaway:** Incremental migration is about *typed boundaries* and *fail-fast validation*, not leaving 40 unchecked cast sites — Karat tests pragmatic legacy strategy, not "rewrite everything day one."

---

---

#### Q7. What is the difference between `ArrayList.Capacity` and `Count`?

_Answer not found._

---

#### Q8. When might you still encounter `ArrayList` in maintained legacy codebases?

_Answer not found._

---

#### Q9. What is the difference between `ArrayList` and `object[]`?

_Answer not found._

---

#### Q10. What legacy non-generic collections (`Hashtable`, `Queue`, `Stack`) should you know for maintenance scenarios?

_Answer not found._

---

### 03. List

#### Q1. Explain the internal working and performance of `List<T>` vs `LinkedList<T>`.

(R) A nightly import job loads 500,000 shipment SKUs into a `List<string>` by calling `Add` one at a time in a loop. Memory profiling shows repeated large allocations and GC pressure. Review the pattern below. What is happening internally, and how would you fix it?

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

**Answer:** Each time `Count` exceeds `Capacity`, `List<T>` allocates a new backing array (typically double the previous size), copies every existing element, and discards the old array — so repeated growth on a half-million-item load causes many intermediate large allocations and full copies before the final size is reached.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | No initial capacity hint | Repeated resize + copy: O(n) per growth step → O(n²) total copy work for n adds |
| Memory | Discarded backing arrays until GC | GC pressure spikes during bulk import; LOH pressure for large string lists |
| Operability | `Clear()` later keeps high `Capacity` | Memory retained after import if list is reused without `TrimExcess()` |

**Fix (priority order):**

1. Pre-size when count is known or estimable: `new List<string>(capacity: 500_000)` or `new List<string>(feedLines as ICollection<string> ?? feedLines.ToList())` when the source exposes count.
2. Prefer `AddRange` over per-item `Add` when inserting a batch — one resize check for the whole range.
3. After bulk deletes, call `TrimExcess()` if the list will stay small long-term to release unused backing array memory.
4. For truly massive feeds, consider streaming processing instead of materializing everything into one list.

```csharp
public static List<string> LoadSkusFromFeed(IReadOnlyCollection<string> feedLines)
{
    var skus = new List<string>(feedLines.Count);
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

**Production takeaway:** `List<T>` growth is amortized O(1) per `Add`, but only if you avoid pathological resize storms — Karat tests whether you know `Capacity` doubles (0 → 4 → 8 → 16 …) and that pre-sizing is a one-line production win. See **Program.cs** Section 3 — Count / Capacity.

---

#### Q2. What is `LinkedList<T>` and when should it be used?

(R) A warehouse service removes cancelled dock labels during iteration. In staging it throws intermittently. Review this method — what breaks, and what is the correct fix?

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

**Answer:** Modifying a `List<T>` while iterating it with `foreach` invalidates the enumerator — the runtime throws `InvalidOperationException` ("Collection was modified") as soon as `Remove` shifts elements and bumps the list's version.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` | `InvalidOperationException` — job fails mid-purge |
| Correctness | Only first match removed per value anyway | Even if it didn't throw, partial removal + skipped items after shift |
| API choice | `Remove(object)` scans from start each call | O(n²) for many cancellations on a large list |

**Fix (priority order):**

1. **Iterate backwards by index** when removing in-place: `for (int i = dockLabels.Count - 1; i >= 0; i--)` then `RemoveAt(i)` — backward removal avoids index skips.
2. **Prefer `RemoveAll`** for predicate-based bulk delete: `dockLabels.RemoveAll(label => cancelled.Contains(label))` — single pass, no enumerator invalidation.
3. **Rebuild** if most items are removed: `dockLabels.RemoveAll(...)` or filter to a new list and replace reference.
4. Never call `Add`, `Insert`, `Remove`, `Clear`, or `Sort` on a collection during `foreach` on that same collection.

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    dockLabels.RemoveAll(label => cancelled.Contains(label));
}
```

**Production takeaway:** This is one of the most common collection bugs in production services — Karat embeds it in realistic warehouse code to see if you diagnose enumerator invalidation, not just "don't modify while looping." See **Program.cs** Section 2 — CRUD / RemoveAll.

---

#### Q3. What is the difference between `List<T>.Sort()` stability and `OrderBy()` stability?

(M) A shipment validator checks whether each incoming pallet's SKU already exists in a queue of 50,000 items by calling `IndexOf` inside a loop. What is the performance problem, and what structure would you use instead?

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0)
        {
            return false;
        }
    }
    return true;
}
```

**Answer:** `IndexOf` performs a linear scan O(n) over the entire list for every incoming item, giving O(n × m) behavior — and because `ShipmentItem` uses reference equality by default, the check may not even match logically equal SKUs unless `Equals` is overridden.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `IndexOf` in outer loop | 50k × incoming count comparisons — timeouts under peak load |
| Correctness | Default reference equality on `ShipmentItem` | Two objects with same SKU may not compare equal — false negatives |
| Design | List is ordered sequence, not lookup index | Wrong tool for membership-by-key checks |

**Fix (priority order):**

1. Build a **`HashSet<string>`** (or `Dictionary<string, ShipmentItem>`) of queued SKUs once — O(1) average lookup per incoming item.
2. If order must be preserved **and** you need key lookup, maintain **both**: `List<ShipmentItem>` for order + `HashSet<string>` for membership (common production pattern).
3. Override **`Equals`/`GetHashCode`** on `ShipmentItem` by SKU if set semantics should match domain identity — required for `IndexOf`/`Contains` to work by value.
4. Use **`Exists(predicate)`** only for single checks — still O(n) per call; does not fix the nested-loop cost.

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    var queuedSkus = new HashSet<string>(queue.Select(q => q.Sku));
    return incoming.All(item => queuedSkus.Contains(item.Sku));
}
```

**Production takeaway:** `List<T>` search helpers (`IndexOf`, `Contains`, `Find`) are fine for small lists or rare checks — Karat uses scale (50k items) to force the jump to hash-based lookup. See **Program.cs** Section 12 — Dictionary preview vs List scan.

---

#### Q4. How does `List<T>` grow its internal buffer when capacity is exceeded?

(D) Two developers search a pallet-count list for the first value over 20. One uses `List.Find`; the other uses LINQ `FirstOrDefault`. When would you prefer each, and what subtle difference matters for value types?

```csharp
List<int> palletCounts = GetPalletCounts();

int a = palletCounts.Find(n => n > 20);
int b = palletCounts.FirstOrDefault(n => n > 20);
```

**Answer:** For `List<int>`, both scan from index 0 and stop at the first match — behavior is equivalent here — but `Find` avoids LINQ's iterator allocation and is the idiomatic in-place search on `List<T>`; the important trap is that both return **`default(T)`** when nothing matches (`0` for `int`, not "no result").

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `default(int)` is `0` when no match | Cannot distinguish "found zero" from "not found" without `Exists` or nullable |
| Performance | LINQ adds delegate + enumerator overhead | Negligible on small lists; matters in hot loops on large lists |
| Consistency | Mixing styles across codebase | Team readability — pick one pattern per layer |

**When to prefer each:**

- **`List.Find` / `Exists` / `FindAll`:** Hot paths on materialized `List<T>` already in memory; mutating-list APIs (`FindAll` returns new list); no extra `using System.Linq`.
- **LINQ (`FirstOrDefault`, `Where`, `Any`):** Composing over `IEnumerable<T>`, deferred pipelines, or when the source may not be a list — keeps query chains uniform.
- **Neither alone for "maybe absent" value types:** Use `int? result = palletCounts.Cast<int?>().FirstOrDefault(n => n > 20)` or check `Exists` first, or return a tuple/bool+value.

**Production takeaway:** Karat tests API semantics, not LINQ religion — `Find` vs `FirstOrDefault` on a `List<int>` is a wash for performance, but **`default(T)` ambiguity** on value types breaks business rules silently. See **Program.cs** Section 4 — Find / Exists.

---

#### Q5. What is the amortized cost of `Add` on `List<T>` vs `Insert` at the beginning or middle?

(R) A `ShipmentQueueService` exposes its internal lane list directly to API callers. Review the property and usage — what can go wrong in production, and how would you expose the data safely?

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void Reassign(string sku, string lane)
    {
        _lanes.Add(lane);
    }
}

// Controller
var lanes = _queueService.Lanes;
lanes.Clear();
lanes.Add("Hijacked-Lane");
```

**Answer:** Returning the live `List<string>` breaks encapsulation — any caller can mutate, clear, or replace elements in the service's internal state without going through `Reassign`, causing invariant violations and race conditions if the service is shared.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public mutable collection escape | Callers bypass validation; `Clear()` wipes production lanes |
| Encapsulation | `List<T>` exposes `Add`/`Remove`/`Sort` | Cannot audit or log changes; hard to evolve to new rules |
| Concurrency | Shared list reference across requests | One request mutates while another reads — corrupt state under load |
| API contract | Return type promises mutability | Consumers depend on side-effecting the service internals |

**Fix (priority order):**

1. Expose **`IReadOnlyList<string>`** backed by **`AsReadOnly()`** or return **`_lanes.ToArray()`** / **`[.. _lanes]`** snapshot when callers must not see live mutations.
2. Prefer **`IReadOnlyList<string> Lanes => _lanes.AsReadOnly()`** for a live read-only view — note underlying list changes still appear (Section 8 behavior).
3. For strict immutability from outside, return a **copy**: `return _lanes.ToList()` or `return (IReadOnlyList<string>)_lanes.ToArray()` — higher allocation, safest for public APIs.
4. Route all mutations through **methods** on the service (`AddLane`, `RemoveLane`) that enforce rules and logging.

```csharp
public IReadOnlyList<string> Lanes => _lanes.AsReadOnly();

public void AddLane(string lane)
{
    if (string.IsNullOrWhiteSpace(lane)) throw new ArgumentException(nameof(lane));
    _lanes.Add(lane);
}
```

**Production takeaway:** `AsReadOnly()` prevents mutation through the wrapper but not through leaked `List<T>` references — Karat stacks encapsulation + API surface design. See **Program.cs** Section 8 — AsReadOnly.

---

#### Q6. What does `List<T>.AsReadOnly()` return, and can callers still mutate the underlying list?

(P) A singleton background worker and several API threads share one static `List<ShipmentItem>` for the live shipment queue. Under load, counts become wrong and the process occasionally throws. Explain why `List<T>` is unsafe here and what pattern you would use instead.

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

**Answer:** `List<T>` is not thread-safe — concurrent `Add`, `RemoveAt`, and reads can corrupt internal array state, lose elements, throw `ArgumentOutOfRangeException`, or throw during enumeration because another thread resized or removed items mid-operation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `Add` + `RemoveAt` | Lost updates, torn reads, occasional exceptions |
| Correctness | `Count > 0` then `[0]` is not atomic | Another thread can dequeue between check and index — race |
| Architecture | Static mutable shared state | Cannot scale out across processes; hidden global coupling |
| Observability | Intermittent failures under load | Passes locally; fails in production peak traffic |

**Fix (priority order):**

1. **`lock` around all queue operations** on a private list if you must share in-process state — simplest fix, limits throughput.
2. Prefer **`ConcurrentQueue<ShipmentItem>`** or **`Channel<ShipmentItem>`** for producer/consumer patterns — designed for concurrent enqueue/dequeue.
3. Remove **static mutable** queues from business logic — inject a **scoped or singleton service** with explicit thread-safe storage; use database/message broker for multi-instance deployments.
4. Never expose the raw list publicly (see Q5) — wrap in a thread-safe API.

```csharp
private static readonly object Gate = new();
private static readonly List<ShipmentItem> LiveQueue = new();

public static void Enqueue(ShipmentItem item)
{
    lock (Gate) { LiveQueue.Add(item); }
}

public static bool TryDequeue(out ShipmentItem? item)
{
    lock (Gate)
    {
        if (LiveQueue.Count == 0) { item = null; return false; }
        item = LiveQueue[0];
        LiveQueue.RemoveAt(0);
        return true;
    }
}
```

**Production takeaway:** `List<T>` documentation explicitly states it is not thread-safe — Karat pairs this with singleton/static patterns to test whether you reach for synchronization or the right concurrent collection. For new code, `Channel<T>` or `ConcurrentQueue<T>` beats hand-rolled locks on `List<T>`.

---

#### Q7. What is the difference between `ConvertAll`, `ForEach`, and LINQ `Select` on a list?

_Answer not found._

---

#### Q8. What do `ToArray`, `CopyTo`, and `GetRange` do — which allocate new arrays?

_Answer not found._

---

#### Q9. When would you expose `List<T>` as a return type vs `IReadOnlyList<T>` or `IEnumerable<T>`?

_Answer not found._

---

#### Q10. What is the difference between `List<T>.Capacity` and `Count`?

_Answer not found._

---

#### Q11. What happens if you mutate a list while iterating with `foreach`?

_Answer not found._

---

#### Q12. What is `TrimExcess`, and when is it useful?

_Answer not found._

---

#### Q13. What is binary search on a list (`BinarySearch`) — what precondition must the list satisfy?

_Answer not found._

---

#### Q14. How does `List<T>` indexer access compare to `LinkedList<T>` (no indexer)?

_Answer not found._

---

#### Q15. What is `Comparison<T>` delegate, and how does it relate to `List<T>.Sort`?

_Answer not found._

---

### 04. Dictionary

#### Q1. What is the difference between `Dictionary<TKey, TValue>` and `Hashtable`?

(R) A hot-path SKU lookup uses `ContainsKey` followed by the indexer. Review this warehouse catalog access. What is inefficient, and how would you improve it?

**Answer:** The code performs two hash lookups — one in `ContainsKey` and one in the indexer — when a single `TryGetValue` call can retrieve the value in one pass. Under hot paths or large catalogs, the extra lookup adds avoidable cost and is harder to read than the idiomatic pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Double hash lookup per hit | Unnecessary CPU on high-frequency SKU resolution |
| Idiomatic C# | `ContainsKey` + indexer is a legacy pattern | Easy to miss in code review; signals unfamiliarity with BCL APIs |
| Concurrency | Two separate reads on a shared dictionary (minor) | Theoretically inconsistent if another thread mutates between calls |

**Fix (priority order):**

1. Replace the pair with `TryGetValue` and branch on its `bool` result.
2. Return the `out` variable directly when found; avoid a second access.
3. Prefer `CollectionsMarshal.GetValueRefOrNullRef` only in measured hot paths — `TryGetValue` is the default fix.

```csharp
public Product? FindProduct(Dictionary<string, Product> catalog, string sku)
{
    return catalog.TryGetValue(sku, out Product? product) ? product : null;
}
```

**Production takeaway:** Karat flags this as a micro-optimization with readability upside — it signals you know standard library APIs, not just that dictionaries exist. See **Program.cs** Section 5 — `TryGetValue` preferred over `ContainsKey` + indexer.

---

#### Q2. Explain `IDictionary<TKey, TValue>` and `IReadOnlyDictionary<TKey, TValue>`.

(R) A team uses a custom class as the dictionary key and mutates it after insert. Lookups start failing intermittently in production. Review this catalog code:

**Answer:** `Dictionary` stores entries by the key's hash code at insert time. Mutating `key.Code` after insert leaves the entry in the wrong bucket — lookups with a new `SkuKey { Code = "WH-1001" }` hash to a different slot, so the product appears missing even though it is still in the table under a stale hash.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable key changed after `catalog[key] = …` | `TryGetValue` / indexer miss; "orphaned" entries |
| Hash contract | `GetHashCode`/`Equals` must be stable while key is in the map | Violated when `Code` property mutates |
| Design | Reference-type key with public setter | Silent data loss in catalog lookups |

**Fix (priority order):**

1. Make key types **immutable** after construction — `init` or `readonly` properties, or use `record`/`readonly record struct` for value semantics.
2. Never mutate a key object that is already in the dictionary; remove, create a new key, and re-insert if the identifier changes.
3. For string SKUs, prefer `Dictionary<string, Product>` — `string` is immutable and already implements the hash contract correctly.
4. If you must wrap identifiers, use `readonly record struct SkuKey(string Code)` or a sealed class with no setters.

```csharp
public readonly record struct SkuKey(string Code);

var catalog = new Dictionary<SkuKey, Product>();
catalog[new SkuKey("WH-1001")] = product;
// To change SKU: remove old entry, insert with new SkuKey — do not mutate in place
```

**Production takeaway:** Hash-table collections assume keys do not change while inserted — Karat uses this to test the hash contract beyond "override GetHashCode." See **Program.cs** Section 1 — custom keys must override both methods and stay immutable after insert.

---

#### Q3. How does `Dictionary<TKey, TValue>` handle hashing and collisions?

(P) An ASP.NET Core API caches product details in a shared `Dictionary<string, Product>` field on a singleton service. Under load tests, responses are wrong and the process occasionally throws `InvalidOperationException`. Review the cache:

**Answer:** `Dictionary<TKey, TValue>` is not thread-safe. Concurrent reads and writes from multiple HTTP requests corrupt internal buckets, throw during enumeration, and allow two threads to both miss the cache and write different `Product` instances for the same SKU — undefined behavior under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `_cache` mutations | `InvalidOperationException`, torn internal state |
| Correctness | Check-then-add without locking | Duplicate DB loads; possible inconsistent cached values |
| Lifetime | Singleton holds one shared dictionary for all requests | Every request shares the same unsynchronized structure |
| Performance | `ContainsKey` + indexer (two lookups) | Extra cost on every cache access |

**Fix (priority order):**

1. Replace with `ConcurrentDictionary<string, Product>` and use `GetOrAdd` or `TryGetValue` for reads.
2. If you must keep `Dictionary`, guard all access with a single lock (`lock (_cache) { … }`) — simpler but lower throughput than `ConcurrentDictionary`.
3. Register the cache as **scoped** only when it is per-request scratch data — not for a cross-request product catalog; singleton + concurrent collection is the usual pattern for shared read-mostly caches.
4. Consider `IMemoryCache` with size limits and expiration instead of a raw unbounded map (see Q5).

```csharp
private readonly ConcurrentDictionary<string, Product> _cache = new();

public Product GetBySku(string sku) =>
    _cache.GetOrAdd(sku, s => _repo.GetBySku(s));
```

**Production takeaway:** Passing local load tests with one thread hides dictionary thread-safety gaps — Karat expects you to name `ConcurrentDictionary` or explicit locking for shared mutable maps. See foundation **Dictionary** gotcha — not thread-safe for concurrent read/write.

---

#### Q4. What is the difference between `Dictionary.Add` and the indexer when the key already exists?

(R) A REST endpoint maps query parameters directly into dictionary lookups without null checks. Review the handler:

**Answer:** When `sku` is omitted, it is `null`. `Dictionary<string, T>.ContainsKey(null)` throws `ArgumentNullException` before the `NotFound()` branch runs — the API returns 500 instead of 400/404. The same rule applies to `Add`, the indexer, and `TryGetValue` with a null reference-type key.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Null key passed to `ContainsKey` | `ArgumentNullException` — 500 to client |
| API contract | Missing query param not validated | Wrong status code; noisy error logs |
| Input hygiene | Nullable `string? sku` used as dictionary key without guard | Any null path hits the same exception |

**Fix (priority order):**

1. Validate input first: `if (string.IsNullOrWhiteSpace(sku)) return BadRequest("sku is required");`
2. Use `TryGetValue` for the lookup after validation — one lookup, no exception on missing key.
3. Do not inject `Dictionary<string, Product>` directly into controllers — use a scoped service that owns catalog access and validation.
4. Return `NotFound()` only after a validated, non-null key misses the catalog.

```csharp
[HttpGet("product")]
public IActionResult GetProduct([FromQuery] string? sku, [FromServices] IProductCatalog catalog)
{
    if (string.IsNullOrWhiteSpace(sku))
        return BadRequest("sku is required");

    return catalog.TryGetProduct(sku, out Product? product)
        ? Ok(product)
        : NotFound();
}
```

**Production takeaway:** Null keys are rejected at the API boundary of `Dictionary<string, …>` — Karat tests whether you validate before touching the collection. See **Program.cs** Section 7c — null key on `Add` throws `ArgumentNullException`.

---

#### Q5. What is the difference between `ContainsKey`, `TryGetValue`, and the indexer for lookup?

(D) A microservice adds a static in-memory cache so repeated HTTP fetches are fast. After two weeks in production, pods hit OOM kills even though traffic is steady. Review the cache:

**Answer:** A plain `Dictionary` with no eviction policy grows without bound — every distinct URL adds a full byte array that is never removed. Static lifetime means the cache survives for the process lifetime and is shared across all requests, so memory only increases as URL diversity grows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | Unbounded key space (`url` strings) + large values (`byte[]`) | OOM kills; GC pressure |
| Lifecycle | `static` cache never cleared | Memory not reclaimed until process restart |
| Operations | No TTL, size cap, or LRU | Cannot reason about worst-case footprint |
| Scale-out | Per-pod static cache | Duplicate memory across replicas; no shared invalidation |

**Fix (priority order):**

1. Replace with `IMemoryCache` (or `MemoryCache`) configured with `SizeLimit`, `CompactionPercentage`, and per-entry `Size` + `AbsoluteExpiration` / `SlidingExpiration`.
2. For distributed deployments, use `IDistributedCache` (Redis) with explicit TTL instead of unbounded in-process storage.
3. If a raw dictionary is unavoidable, implement LRU with a max entry count and max total bytes — evict oldest when limits are hit.
4. Remove `static` — inject a singleton `IMemoryCache` via DI so tests can substitute and options can be configured per environment.

```csharp
// Program.cs — services
builder.Services.AddMemoryCache(o =>
{
    o.SizeLimit = 10_000; // abstract "size units", set per entry
});

// Usage
_cache.GetOrCreate(url, entry =>
{
    entry.Size = 1;
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return Download(url);
});
```

**Production takeaway:** `Dictionary` is a map, not a cache policy — Karat distinguishes "fast lookup" from "safe caching." Unbounded in-memory maps are a common postmortem root cause. See **Program.cs** WHY IT MATTERS — catalogs and caches appear everywhere; size and eviction are production requirements.

---

#### Q6. Why must keys be immutable (or stable) after insertion for correct hash table behavior?

(P) A developer avoids `ConcurrentDictionary` and hand-rolls lazy initialization with `TryGetValue`. Under load, the expensive factory runs twice for the same key. Review:

**Answer:** Between `TryGetValue` returning false and `_catalog[sku] = product`, another thread can pass the same check and also call `_repo.LoadProduct(sku)` — classic check-then-act race. Both threads may insert; the last write wins, but you paid for duplicate DB work and may briefly expose inconsistent state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-atomic check-then-add on `Dictionary` | Duplicate expensive loads under parallel requests |
| Correctness | Two writers without synchronization | Undefined behavior on `Dictionary` itself (see Q3) |
| Cost | Idempotent DB read assumed | Thundering herd on cold keys at startup or cache flush |

**Fix (priority order):**

1. Use `ConcurrentDictionary.GetOrAdd` so factory execution for a given key is coordinated by the collection.
2. If the factory is very expensive, wrap with `Lazy<Product>` per key or use `GetOrAdd` with a factory that returns `Lazy<Product>` and then `.Value` once — avoids duplicate work when factory cost dominates.
3. For single-threaded or scoped usage, plain `TryGetValue` + assign is fine — the bug is specifically shared mutable state under concurrency.
4. Add metrics on cache misses and factory duration to detect duplicate load spikes in production.

```csharp
private readonly ConcurrentDictionary<string, Product> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => _repo.LoadProduct(s));

// Optional: defer heavy work until first read
private readonly ConcurrentDictionary<string, Lazy<Product>> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => new Lazy<Product>(() => _repo.LoadProduct(s))).Value;
```

**Production takeaway:** `GetOrAdd` is the production pattern for "compute once per key" in concurrent caches — Karat tests whether you recognize check-then-add as a race, not whether you memorized the method name. Pair with Q3: thread-safe type **and** atomic get-or-create semantics.

---

---

#### Q7. What exception is thrown when accessing a missing key via the indexer?

_Answer not found._

---

#### Q8. Can `null` be used as a key when `TKey` is a reference type?

_Answer not found._

---

#### Q9. What is the average vs worst-case time complexity for lookup, insert, and remove?

_Answer not found._

---

#### Q10. What is the hash code contract between `GetHashCode` and `Equals` for custom key types?

_Answer not found._

---

#### Q11. What is `IEqualityComparer<TKey>`, and when do you pass a custom comparer to the constructor?

_Answer not found._

---

#### Q12. When would you choose `Dictionary` over `List` for lookups by id or SKU?

_Answer not found._

---

#### Q13. What happens internally when two keys hash to the same bucket?

_Answer not found._

---

### 05. HashSet

#### Q1. Explain `HashSet<T>` and its use cases. How is it different from `List<T>`?

(R) A nightly tag-import job deduplicates article tags with `List<string>.Contains` before insert. Review the hot path:

**Answer:** `List<T>.Contains` is **O(n)** per call, so importing *m* tags against *n* existing tags approaches **O(n × m)** — fine for unit tests with ten tags, catastrophic at 80k × 200k. Replace the backing store with `HashSet<string>` and the same `StringComparer` so `Add` and `Contains` are **O(1)** average.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Linear scan on every `Contains` | Import SLA missed; CPU spikes on large catalogs |
| Scalability | List grows; each check walks all elements | Cost compounds as `_knownTags` grows through the run |
| Collection choice | List chosen for uniqueness | Wrong tool — HashSet exists for exactly this pattern |

**Fix (priority order):**

1. Use `HashSet<string>` with `StringComparer.OrdinalIgnoreCase` as the backing store.
2. Collapse register to a single `Add` — it returns `false` when the tag is already present.

```csharp
private readonly HashSet<string> _knownTags =
    new(StringComparer.OrdinalIgnoreCase);

public bool TryRegisterTag(string tag) => _knownTags.Add(tag);
```

**Production takeaway:** Karat pairs "works in tests" with hidden **O(n²)** membership — see **Program.cs** Section 9 (HashSet vs List). Always ask lookup frequency and collection size, not just correctness on small data.

---

#### Q2. What is the difference between `SortedSet<T>` and `HashSet<T>`?

(R) A newsletter service deduplicates subscribers by email but keeps seeing duplicate sends in logs. Review:

**Answer:** `HashSet<Subscriber>` without a custom comparer uses **reference equality** for class types — two distinct `Subscriber` objects with the same email are different elements. Pass `SubscriberByEmailComparer` (or override `Equals`/`GetHashCode` on the type) so business identity drives uniqueness.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default reference equality on reference type | Duplicate emails stored; duplicate emails sent |
| API misuse | `HashSet` assumed to compare by field values | Silent data-quality bug — `Add` returns `true` twice |
| Design | Equality rule not wired into collection | `Contains`/`Remove` also fail to find "same" subscriber |

**Fix (priority order):**

1. Construct with the chapter comparer: `new HashSet<Subscriber>(new SubscriberByEmailComparer())`.
2. Alternatively, use `SubscriberIdentity`-style immutable type with `IEquatable<T>` + consistent `GetHashCode` on `Email`.

```csharp
private readonly HashSet<Subscriber> _subscribers =
    new(new SubscriberByEmailComparer());
```

**Production takeaway:** Custom types in HashSet/Dictionary **never** dedupe by field values unless you supply equality — see **Program.cs** Section 6 vs Section 7.

---

#### Q3. Why does `HashSet<T>` require correct `GetHashCode()`/`Equals()` for custom types?

(R) After a profile-update feature ships, support reports "user already subscribed" errors even when lookup fails. Review:

**Answer:** `GetHashCode` was computed from `Email` at `Add` time and placed the object in a bucket keyed to the old hash. Mutating `Email` afterward leaves the object in the **wrong bucket**, so `Contains` returns `false` even though the instance is still in the set — classic broken hash contract with mutable keys.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable field participates in `GetHashCode` | Lookup/remove fail after in-place edit |
| Hash contract | Hash at insert ≠ hash at lookup | Element orphaned inside set — `Count` includes it but `Contains` misses |
| Design | Writable `Email` on set member type | Same rule as Dictionary keys — must be immutable for hashed collections |

**Fix (priority order):**

1. Make identity fields immutable (`init` or constructor-only), matching `SubscriberIdentity` in **Program.cs** Section 7.
2. If email must change, **remove** old identity from the set and **add** a new object (or rebuild the set).
3. Never mutate fields that feed `Equals`/`GetHashCode` while the instance lives inside a `HashSet` or `Dictionary`.

```csharp
public sealed class SubscriberProfile
{
    public string Email { get; }
    public string Name { get; set; }

    public SubscriberProfile(string email, string name)
    {
        Email = email;
        Name = name;
    }
    // Equals/GetHashCode on Email only
}
```

**Production takeaway:** Karat tests whether you treat HashSet elements like **Dictionary keys** — mutable hash inputs cause silent lookup failures, not exceptions.

---

#### Q4. What set operations does `HashSet<T>` provide (`UnionWith`, `IntersectWith`, `ExceptWith`, `SymmetricExceptWith`)?

(R) An editorial dashboard merges article tag sets for a "shared topics" widget. Case variants appear twice after deploy. Review:

**Answer:** `HashSet<T>.Union` as a LINQ extension on `IEnumerable<T>` uses **default sequence equality** (`EqualityComparer<string>.Default` → **Ordinal**, case-sensitive), **not** the HashSet's internal `StringComparer.OrdinalIgnoreCase`. Re-wrapping in `new HashSet<string>(allTopics)` without a comparer keeps Ordinal semantics, so `"csharp"` and `"CSharp"` coexist as distinct entries.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | LINQ set ops ignore HashSet's comparer | Logical duplicates in UI and analytics |
| API confusion | `Union` on HashSet still calls `Enumerable.Union` | Developer's comparer choice on construction does not flow to LINQ |
| Data quality | Default `HashSet` ctor uses Ordinal | Case variants inflate counts and break deduped filters |

**Fix (priority order):**

1. Pass the same comparer when materializing: `new HashSet<string>(dotnetTags.Union(linqTags), StringComparer.OrdinalIgnoreCase)`.
2. Or use mutating `UnionWith` on a **copy** if you need set instance semantics with the existing comparer.
3. For intersection-only widgets, same rule: `new HashSet<string>(a.Intersect(b), comparer)`.

```csharp
var widgetTags = new HashSet<string>(
    dotnetTags.Union(linqTags),
    StringComparer.OrdinalIgnoreCase);
```

**Production takeaway:** LINQ `Union`/`Intersect`/`Except` are **comparer-agnostic** — see **Program.cs** Section 3. Always thread `IEqualityComparer<T>` through the final `HashSet` constructor.

---

#### Q5. What is the difference between `Add` returning `false` on duplicate vs `List.Add` behavior?

(R) A publish pipeline accidentally wipes an editor's working tag pool. Review the merge step:

**Answer:** `IntersectWith` **mutates the caller** (`editorPool`) in place, keeping only elements also in `draftTags`. The developer needed a **non-mutating** preview — the chapter's LINQ `Intersect` (Section 3) or a copy-then-`IntersectWith` pattern (Section 4).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `IntersectWith` vs intended read-only preview | `"csharp"` and `"security"` permanently removed from working pool |
| API misuse | Confused mutating (`*With`) vs LINQ extension methods | Downstream `UnionWith` cannot restore deleted tags |
| Operational | Shared `editorPool` referenced elsewhere | Other features see truncated set — data loss in session state |

**Fix (priority order):**

1. Non-mutating LINQ: `var preview = new HashSet<string>(editorPool.Intersect(draftTags), StringComparer.OrdinalIgnoreCase);`
2. Or copy first: `var preview = new HashSet<string>(editorPool, comparer); preview.IntersectWith(draftTags);`
3. Reserve `IntersectWith` for intentional in-place filtering when building a working set incrementally.

```csharp
var preview = new HashSet<string>(
    editorPool.Intersect(draftTags),
    StringComparer.OrdinalIgnoreCase);
// editorPool unchanged: csharp, dotnet, security
```

**Production takeaway:** `*With` methods return `void` and modify **this** — Karat loves swapping them with LINQ equivalents. Read method names literally before calling on shared state.

---

#### Q6. How do you construct a `HashSet<T>` with custom equality (`IEqualityComparer<T>`)?

(R) A custom comparer passes code review but `Remove` and `Contains` behave inconsistently. Review:

**Answer:** `Equals` compares **Email** but `GetHashCode` hashes **Name** — violating the rule that equal objects must share the same hash code. The second subscriber lands in a different bucket, so `Contains`/`Remove` miss while duplicate `Add` behavior looks arbitrary.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `GetHashCode`/`Equals` inconsistency | Silent failures — worst kind of collection bug |
| Hash contract | Equal-by-email objects can differ by hash | `Remove` returns false for objects that "should" match |
| Code review | Comparer named `ByName` but equals on Email | Copy-paste defect easy to miss without contract tests |

**Fix (priority order):**

1. Derive hash from the **same fields** used in `Equals` — here, email case-insensitively.
2. Add unit tests: if `Equals(a,b)` then `GetHashCode(a) == GetHashCode(b)`; round-trip `Add`/`Contains`/`Remove`.

```csharp
public int GetHashCode(Subscriber obj) =>
    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Email);
```

**Production takeaway:** HashSet and Dictionary failures from bad comparers **do not throw** — they return wrong `bool` results. Same contract as **Program.cs** Section 6 quick reference: *Equal objects → same hash code*.

---

#### Q7. When would you use `HashSet<T>` for deduplication vs `Distinct()` in LINQ?

_Answer not found._

---

#### Q8. What is the difference between set membership test in `HashSet` vs scanning a `List`?

_Answer not found._

---

#### Q9. What is `IsSubsetOf`, `IsSupersetOf`, and `Overlaps` used for?

_Answer not found._

---

#### Q10. Can you modify an element in a `HashSet` in place if it affects equality — what goes wrong?

_Answer not found._

---

### 06. Queue and Stack

#### Q1. Explain `Queue<T>` and `Stack<T>` vs their non-generic counterparts.

(R) A help-desk service was refactored from `Queue<SupportTicket>` to `Stack<SupportTicket>` "because stacks are faster." Review the handler loop. What ordering bug appears in production, and how do you fix it?

**Answer:** `Stack<T>` is LIFO — the last ticket pushed is the first popped — so SLA fairness is inverted: newest tickets are resolved before older ones waiting longer. Ticket queues require FIFO semantics, which `Queue<T>` enforces at the type level.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Stack` + `Push`/`Pop` for arrival-order work | Newest-first processing — SLA breaches on oldest tickets |
| Naming/API | Method still named `EnqueueTicket` but calls `Push` | Misleading API; code review misses semantic mismatch |
| Design | Chose collection for perceived speed, not ordering rule | Wrong abstraction — `List<T>` with `Insert(0,…)` would be equally wrong |

**Fix (priority order):**

1. Restore `Queue<SupportTicket>` with `Enqueue` / `TryDequeue` (or `Dequeue` when empty is impossible by contract).
2. Rename methods to match semantics: `EnqueueTicket` + `TryResolveNextTicket` as in **Program.cs** Section 4.
3. If priority tiers are needed later, use `PriorityQueue<TElement, TPriority>` — not `Stack<T>`.
4. Document ordering invariant in tests: enqueue A, B, C → resolve A, B, C.

```csharp
private readonly Queue<SupportTicket> _pending = new();

public void EnqueueTicket(SupportTicket ticket) => _pending.Enqueue(ticket);

public bool TryResolveNext(out SupportTicket ticket) => _pending.TryDequeue(out ticket);
```

**Production takeaway:** Karat tests whether you match collection type to business ordering — FIFO for fair queues, LIFO for undo/call-stack models. See **Program.cs** Section 1 — FIFO vs LIFO table.

---

#### Q2. What is FIFO vs LIFO, and which collection maps to each?

(R) A background worker drains a print queue when the upstream publisher is idle. Under load, the service logs unhandled `InvalidOperationException` and the host restarts. Review the consumer:

```csharp
public sealed class PrintWorker
{
    private readonly Queue<PrintJob> _jobs = new Queue<PrintJob>();

    public void Submit(PrintJob job) => _jobs.Enqueue(job);

    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            PrintJob job = _jobs.Dequeue();  // throws when queue empty
            Print(job);
        }
    }
}
```

What breaks, and how would you harden this for production idle periods?

**Answer:** `Dequeue()` throws `InvalidOperationException` when the queue is empty — the tight loop calls it continuously during idle periods, crashing the worker instead of waiting for the next job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Dequeue()` on empty queue | Unhandled exception → host restart / lost in-flight work |
| Control flow | Busy loop with no back-off when empty | 100% CPU spin if switched to `Count` check without delay |
| Concurrency | Plain `Queue<T>` if multiple producers (minor here) | Not thread-safe — separate from empty-queue bug but common in same services |

**Fix (priority order):**

1. Replace `Dequeue()` with `TryDequeue(out PrintJob? job)` — process only when `true`.
2. When empty, await a signal (`Channel<PrintJob>`, `BlockingCollection<T>`, or `ManualResetEventSlim` + lock) instead of spinning.
3. Optionally combine with `await Task.Delay(pollInterval, ct)` only if a simple poll model is acceptable — prefer event-driven dequeue.
4. For multi-producer scenarios, use `ConcurrentQueue<T>` or a `Channel<T>` writer/reader pair.

```csharp
public async Task RunAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        if (_jobs.TryDequeue(out PrintJob? job))
        {
            Print(job);
            continue;
        }

        await Task.Delay(100, ct); // or await _signal.WaitAsync(ct);
    }
}
```

**Production takeaway:** Empty is an expected state for workers — `TryDequeue`/`TryPop` exist precisely to avoid exception-driven control flow. See **Program.cs** Section 2a — empty queue behavior.

---

#### Q3. What operations does `Queue<T>` expose (`Enqueue`, `Dequeue`, `Peek`, `TryDequeue`, `TryPeek`)?

(P) Three ASP.NET Core request threads enqueue audit events; one background `IHostedService` dequeues them for batch upload. The team shares one `Queue<AuditEvent>` instance registered as a **Singleton**. Occasionally events disappear or `InvalidOperationException` appears under concurrent `Enqueue`/`Dequeue`. Explain why `Queue<T>` is unsafe here and what you would register instead.

**Answer:** `Queue<T>` is not thread-safe — concurrent `Enqueue` and `Dequeue` from multiple threads corrupt internal state without external locking, causing lost items or exceptions. A singleton shared across request threads requires a concurrent collection or a `Channel<T>`.

- **`ConcurrentQueue<T>`:** Lock-free FIFO safe for multiple producers and consumers; `TryDequeue` for the background drainer. Good when you only need in-memory fan-in.
- **`Channel<T>` (System.Threading.Channels):** Preferred in modern ASP.NET Core — bounded capacity for back-pressure, async `Reader.ReadAllAsync`, clean producer/consumer split in DI.
- **`BlockingCollection<T>`:** Legacy pattern wrapping a concurrent queue with blocking take — workable but heavier than channels for new code.
- **Do not** wrap `Queue<T>` in a singleton and synchronize ad hoc on every call without reviewing lock ordering — easy to deadlock with `Dequeue` inside `lock` while producers hold the same lock incorrectly.

```csharp
// Registration sketch
builder.Services.AddSingleton(Channel.CreateBounded<AuditEvent>(
    new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<AuditBatchUploader>();
```

**Production takeaway:** FIFO ordering does not imply thread safety — choose `ConcurrentQueue<T>` or `Channel<T>` when a queue crosses thread boundaries. See **Program.cs** Section 2 — `Queue<T>` API assumes single-threaded mutation unless externally synchronized.

---

#### Q4. What operations does `Stack<T>` expose (`Push`, `Pop`, `Peek`, `TryPop`)?

(M) A developer rewrites maze pathfinding from the chapter's BFS to recursive DFS. On large grids the process terminates with `StackOverflowException`. They propose "just use `Stack<T>` instead of recursion." Review both approaches:

```csharp
// Original (chapter-style BFS) — works on large maze
Queue<(int Row, int Col)> frontier = new();
frontier.Enqueue(start);
while (frontier.TryDequeue(out var current)) { /* expand neighbors */ }

// Rewrite — deep recursion on 2000×2000 grid
void Dfs(int row, int col)
{
    if (visited[row, col]) return;
    visited[row, col] = true;
    foreach (var neighbor in GetNeighbors(row, col))
        Dfs(neighbor.Row, neighbor.Col);  // one frame per depth level
}
```

What actually causes the overflow, and when does an explicit `Stack<T>` fix it vs when recursion is acceptable?

**Answer:** `StackOverflowException` comes from the **CLR call stack** — each recursive `Dfs` call consumes a stack frame (~1 MB default thread stack limit), not from `Stack<T>` heap storage. Replacing recursion with an explicit `Stack<(int,int)>` loop uses the heap for frontier cells, avoiding deep call stacks on large grids.

- **Cause:** Depth-first recursion on a path thousands of cells long nests that many frames; the OS/thread stack overflows before the algorithm finishes.
- **Explicit `Stack<T>` fix:** Push start cell; `while (stack.TryPop(out current))` expand neighbors and push unvisited — same LIFO DFS order, bounded by heap memory instead of call-stack depth.
- **When recursion is fine:** Shallow trees (expression AST depth &lt; ~100), divide-and-conquer with logarithmic depth, or problems with guaranteed small branching depth.
- **BFS vs DFS choice (related):** Chapter BFS with `Queue<T>` finds shortest paths in unweighted grids; DFS (recursive or `Stack<T>`) does not guarantee shortest path but uses less memory for some sparse graphs.
- **Not a fix:** Switching BFS to `Stack<T>` without changing algorithm — that yields DFS traversal order and breaks shortest-path guarantees from **Program.cs** Section 5.

**Production takeaway:** Karat distinguishes the **call stack** (recursion limit) from **`Stack<T>`** (heap collection) — iterative DFS with `Stack<T>` is the standard production pattern for deep graph search.

---

#### Q5. Why do `Queue` and `Stack` not support random access by index?

(D) Your team must pick a frontier collection for two graph tasks on an unweighted social network: (A) find **shortest path** in friend hops from user A to user B, and (B) detect whether a **cycle** exists in a follow graph (direction matters). One engineer says "both are graph search — use `Stack<T>` for both." What would you choose for each task and why?

**Answer:** Shortest hop count in an unweighted graph requires BFS with `Queue<T>` so nodes are discovered in non-decreasing distance from the start; cycle detection in a directed graph is typically DFS with `Stack<T>` (or recursion) and a recursion/recursion-stack coloring strategy — not the same frontier choice.

**Task A — shortest friend hops (unweighted):**

- Use **`Queue<UserId>`** BFS — first time you dequeue B, you have minimum hop count.
- `Stack<T>` DFS may find *a* path quickly but not the shortest — wrong for "degrees of separation" product features.

**Task B — cycle in directed follow graph:**

- Use **DFS** with **`Stack<UserId>`** (explicit or recursion) plus `visited` / `onStack` (three-color) state to detect back edges.
- BFS with `Queue<T>` finds cycles in undirected graphs with parent tracking but directed cycle detection is awkward with BFS alone.

| Task | Collection | Why |
|---|---|---|
| Shortest hops (unweighted) | `Queue<T>` — BFS | Layer-by-layer discovery = minimum edges |
| Directed cycle detection | `Stack<T>` — DFS | Back edge to active stack frame signals cycle |

- **Production note:** At web scale, graph logic moves to a graph DB or precomputed index — but the collection choice still signals correct algorithmic reasoning in code reviews and Karat screens.

**Production takeaway:** Match FIFO vs LIFO to the **invariant** you need (shortest layer vs deep path/back-edge detection), not to "both are graphs." See **Program.cs** Quick Reference — BFS → `Queue<T>`, DFS → `Stack<T>` or recursion.

---

#### Q6. How is `Queue<T>` used in breadth-first search (BFS) on a graph or grid?

(R) A response editor copied from the chapter's `HelpDeskSession` mixes undo (`Stack<string>`) with ticket draining. Review this merge:

```csharp
public sealed class AgentSession
{
    private readonly Queue<SupportTicket> _tickets = new();
    private readonly Stack<string> _undo = new();
    private readonly StringBuilder _draft = new();

    public void BeginResponse(SupportTicket ticket)
    {
        _draft.Clear();
        _undo.Clear();                    // clears undo history
        _tickets.Enqueue(ticket);         // re-queues active ticket to tail
    }

    public void ApplyEdit(Action<StringBuilder> edit)
    {
        _undo.Push(_draft.ToString());
        edit(_draft);
    }

    public SupportTicket? TakeNextTicket()
    {
        return _tickets.Count > 0 ? _tickets.Dequeue() : null;
    }
}
```

The agent reports tickets jumping to the back of the line and undo lost mid-edit. What went wrong with collection choice and API usage?

**Answer:** `BeginResponse` misuses both collections — it re-`Enqueue`s the ticket already being worked (sending it to the tail instead of keeping it as the active item) and clears the undo stack even when only the draft should reset. Tickets and undo stacks serve different lifecycles and must not be conflated in one "begin" method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Queue misuse | `Enqueue(ticket)` on ticket already removed for editing | Ticket moves to back — others processed first |
| Stack misuse | `_undo.Clear()` on every begin | Undo history wiped — agent cannot revert prior edits |
| API design | `BeginResponse` accepts ticket param implying re-queue | Confuses "start draft text" with "return ticket to queue" |
| Empty handling | `TakeNextTicket` uses ternary + `Dequeue` | Acceptable here, but inconsistent with chapter's `TryDequeue` pattern |

**Fix (priority order):**

1. Split responsibilities: `TryResolveNextTicket` dequeues once; `BeginResponse(string openingLine)` only clears draft + undo — **no** queue mutation (mirror **Program.cs** Section 4 `HelpDeskSession`).
2. Do not pass the active ticket back into the queue until the response is sent or explicitly re-queued.
3. Clear `_undo` only when starting a **new** response for a **new** ticket, not on every keystroke batch.
4. Prefer `TryDequeue` over `Count` + `Dequeue` to avoid races if the session becomes multi-threaded.

```csharp
public void BeginResponse(string openingLine)
{
    _undo.Clear();
    _draft.Clear();
    _draft.Append(openingLine);
}

public bool TryResolveNextTicket(out SupportTicket ticket)
    => _tickets.TryDequeue(out ticket);
```

**Production takeaway:** Queue and Stack often appear together in one workflow (tickets FIFO + undo LIFO) — Karat tests that you keep each collection's contract isolated. See **Program.cs** Section 4 — help-desk scenario wiring.

---

#### Q7. Why does BFS find shortest paths in unweighted graphs?

_Answer not found._

---

#### Q8. What real-world workflows map naturally to a stack (undo/redo, call stack, DFS)?

_Answer not found._

---

#### Q9. What is the difference between non-generic `Queue`/`Stack` and generic versions regarding boxing?

_Answer not found._

---

#### Q10. When would you use `Queue<T>` over `List<T>` with remove-from-front patterns?

_Answer not found._

---

### 07. SortedList & SortedDictionary

#### Q1. What is `SortedList<TKey, TValue>` and `SortedDictionary<TKey, TValue>`? When would you use each?

(R) A warehouse dashboard prints the lowest and highest SKU from a live price map. Review:

**Answer:** `SortedDictionary<TKey,TValue>.Keys` is a read-only `ICollection<TKey>` with **no indexer** — only `SortedList` exposes `Keys[i]` and `Values[i]` for O(1) access by sorted rank. Keep `SortedDictionary` and walk `Keys` once (or track min/max while loading) rather than dropping to `Dictionary` and resorting on every request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Keys[0]` on `SortedDictionary` | CS0021 — indexer not defined on Keys view |
| API confusion | Assumed both sorted types support rank indexing | Wrong type chosen for dashboard endpoint |
| Over-correction | Sort-on-read with `Dictionary` + LINQ | O(n log n) per page load when O(n) scan or `SortedList` index suffices |

**Fix (priority order):**

1. If random access by sorted rank is required (`Keys[0]`, `Values[i]`), use `SortedList<string, decimal>` — see **Program.cs** Section 3.
2. If the map stays a `SortedDictionary`, iterate `Keys` once to capture first and last (Section 6 pattern) — O(n) but simple for 12k keys on a dashboard.
3. Do **not** switch to `Dictionary` solely to fix the compile error when sorted iteration is a product requirement.

```csharp
string? lowest = null, highest = null;
foreach (string sku in skuPrices.Keys)
{
    lowest ??= sku;
    highest = sku;
}
```

**Production takeaway:** Karat tests whether you know **index by rank** is `SortedList`-only — `SortedDictionary` gives sorted foreach and O(log n) key lookup, not `Keys[i]`.

---

#### Q2. What interface defines ordering for sorted collections (`IComparer<TKey>` vs `IEqualityComparer<TKey>`)?

(R) An inventory sync service upserts pallet counts every few seconds. Review the hot path:

**Answer:** `SortedList` insert and remove shift parallel key/value arrays — **O(n)** per upsert when the collection is full-sized. Frequent mixed inserts/updates on ~500 zones make array shifting dominate even though `TryGetValue` remains O(log n). Swap to `SortedDictionary<int, int>` for tree-backed O(log n) insert/remove while preserving sorted foreach.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `SortedList.Add` / shift on new keys | Latency grows with zone count under steady churn |
| Idiom | `ContainsKey` + indexer instead of one path | Extra O(log n) lookup; minor vs shifting cost |
| Collection choice | `SortedList` for write-heavy sync | Wrong tool when entries churn — Section 8 guidance |

**Fix (priority order):**

1. Replace backing field with `SortedDictionary<int, int>`.
2. Collapse upsert to indexer assignment — adds or updates in one call: `_countsByZone[zoneId] = palletCount`.
3. Reserve `SortedList` for small, mostly static maps (config tables, reorder reports in **Program.cs** Section 11).

```csharp
private readonly SortedDictionary<int, int> _countsByZone = new();

public void UpsertZoneCount(int zoneId, int palletCount) =>
    _countsByZone[zoneId] = palletCount;
```

**Production takeaway:** "Lookups are fast" is a Karat trap — **insert/remove cost** separates `SortedList` from `SortedDictionary`. See **Program.cs** Section 9 — insert O(n) vs O(log n).

---

#### Q3. What is the difference between `SortedList` (array-backed) and `SortedDictionary` (tree-backed) performance?

(D) You expose a `/regions/sales` JSON endpoint. Product wants keys returned alphabetically by region code. Two proposals:

**Answer:** For ~30 regions, hourly refresh, and read-heavy traffic, **`SortedDictionary<string, int>` (B)** is the better default: sorted order is built into the structure, foreach needs no extra sort, and n is tiny so O(log n) lookup cost is irrelevant. **`Dictionary` + `OrderBy` (A)** adds allocation and O(n log n) work on every response unless you cache the sorted projection — acceptable only if you already hold a `Dictionary` for O(1) hot lookups elsewhere and sort rarely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance (A) | Sort 30 keys per request × 2k rpm | Avoidable CPU and GC from repeated `OrderBy` |
| Correctness (B wrong use) | `SortedDictionary` for millions of writes | Tree rebalancing still O(log n) — fine here, bad at huge churn |
| Design | Picking `Dictionary` "because it's faster" | Ignores that 30-key sort-on-read duplicates work the BCL already provides |

**Decision:**

| Factor | Prefer |
|---|---|
| Small n, sorted output every read | `SortedDictionary` |
| Huge n, order irrelevant, rare sorted export | `Dictionary` + sort once when exporting |
| Need `Keys[i]` by rank | `SortedList` (even smaller static maps) |
| Fastest lookup, no order | Plain `Dictionary` — Section 1 |

**Production takeaway:** Karat expects **size and access pattern** reasoning — 30 hourly regions is the sweet spot for sorted maps; see **Program.cs** Section 8 pick-list.

---

#### Q4. When is `SortedList` preferred over `SortedDictionary` for memory or indexed access?

(R) A catalog search feature stores product tags in a case-insensitive sorted map. QA reports duplicate logical tags after a Turkish-locale server deploy. Review:

**Answer:** `StringComparer.CurrentCulture` uses locale-sensitive rules — casing and ordering can differ by server culture (Turkish **I/i** is the classic trap). For stable product-tag identity, use **`StringComparer.OrdinalIgnoreCase`** (chapter Section 7) so `"CSharp"` and `"csharp"` are the same key and indexer assignment updates rather than duplicates.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-sensitive comparer on logical keys | Duplicate or missing keys when culture changes across environments |
| Environment | Turkish locale vs en-US dev box | QA-only failures after regional deploy |
| API | Indexer update assumes comparer treats keys as equal | Two entries when comparer says keys differ |

**Fix (priority order):**

1. Construct with `StringComparer.OrdinalIgnoreCase` — matches **Program.cs** `DemoCustomComparer`.
2. Use `Add` only when you want duplicate detection to throw; use indexer when upserting counts.
3. Reserve `CurrentCulture` for **display sort** (UI lists), not for canonical tag keys in services.

```csharp
var tagsByCount = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["dotnet"] = 12,
    ["CSharp"] = 8,
    ["LINQ"] = 5
};

tagsByCount["csharp"] = 99; // updates single "CSharp"/"csharp" entry
```

**Production takeaway:** Sorted collections sort by **`IComparer<TKey>`**, not culture by default — explicit `OrdinalIgnoreCase` avoids locale drift between dev and production. See foundation **Strings** — culture vs ordinal.

---

#### Q5. What is the cost of inserting out-of-order keys into a sorted collection?

(M) A pricing microservice benchmarks three shapes for a nightly job that inserts 50_000 random SKUs once, then performs 500_000 lookups:

**Answer:** `SortedList` backs keys and values with **parallel arrays**. Each insert finds the slot with binary search then **shifts** remaining elements — **O(n)** per insert, so 50_000 random inserts approach **O(n²)** total work. `Dictionary` averages **O(1)** insert; `SortedDictionary` uses a red-black tree at **O(log n)** per insert with no shifting — which is why `SortedList` dominates the load phase despite similar O(log n) lookup afterward.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Array shift on every `SortedList.Add` | Load phase orders of magnitude slower at 50k entries |
| Benchmark misread | Blaming "sorted" generically | Wrong fix — might avoid all sorted types instead of swapping list vs tree |
| Pattern | `ContainsKey` guard before every add | Extra lookup; still dwarfed by shift cost on `SortedList` |

**Rule of thumb (Section 9):**

| Need | Pick |
|---|---|
| Fastest lookup, order irrelevant | `Dictionary` |
| Sorted keys + access by rank (`Keys[i]`) | `SortedList` — small / mostly static |
| Sorted keys + frequent inserts/removes | `SortedDictionary` |
| Membership only, no values | `HashSet` |

**Production takeaway:** Karat pairs benchmark numbers with **mechanism** — array shift vs tree rebalance — not just "sorted is slower." Nightly bulk load + heavy lookup → `Dictionary` or `SortedDictionary`; `SortedList` only if you need index-by-rank on a small final map.

---

#### Q6. Can you look up by index in `SortedList` — what does `Keys[index]` provide?

(R) A developer ports a `Dictionary` helper to sorted collections but copies the wrong comparer interface. Review:

**Answer:** `SortedDictionary` and `SortedList` constructors take **`IComparer<TKey>`**, not **`IEqualityComparer<TKey>`**. The snippet fails at compile time (`SkuIgnoreCaseEquality` does not implement `IComparer<string>`). If coerced, the collection would still sort/compare by the wrong contract — hash-based equality does not define sort order. Pass `StringComparer.OrdinalIgnoreCase` (implements `IComparer<string>`) or a custom `IComparer<string>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `IEqualityComparer` passed to sorted ctor | CS1503 — type mismatch |
| Conceptual | Confused hash equality with sort order | Dictionary/HashSet vs sorted maps — Section 7 comment |
| Runtime (if bypassed) | Wrong or default ordering | Duplicate logical keys or unexpected sort sequence |

**Fix (priority order):**

1. Use built-in comparer: `new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase)`.
2. For custom rules, implement **`IComparer<TKey>`** (like `ScoreDescendingComparer` in **Program.cs**), not `IEqualityComparer`.
3. Keep `SkuIgnoreCaseEquality` for `Dictionary<string, T>` / `HashSet<string>` only.

```csharp
var reorderQty = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["ZEBRA-CLIP"] = 40,
    ["ALPHA-PAD"] = 120
};

reorderQty["alpha-pad"] = 200; // updates existing key
```

**Production takeaway:** Karat stacks **interface confusion** with collection choice — `IEqualityComparer` for hash tables, `IComparer` for sorted types. See **Program.cs** QUICK REFERENCE — "Not IEqualityComparer."

---

#### Q7. What is the difference between `SortedSet<T>` and `SortedDictionary<TKey, TValue>`?

_Answer not found._

---

#### Q8. When would you choose `SortedDictionary` over sorting keys from a `Dictionary` at read time?

_Answer not found._

---

### 08. IEnumerable & IEnumerator

#### Q1. What is the difference between `IEnumerable<T>` and `ICollection<T>`?

(R) A warehouse API returns `IEnumerable<PickLine>` from a `yield return` filter. A report job calls `Count()` then `Sum()` on the same reference without materializing. Totals disagree with the pick ticket and logs show the database query ran twice. Review the service method and caller. What went wrong, and how do you fix it?

**Answer:** `IEnumerable<PickLine>` from a `yield return` method is lazy — each consumer (`Count`, then `Sum`) walks the sequence from scratch, re-running `_repository.LoadLines` and the filter. The two passes are independent enumerations, so side effects, timing, and even data can differ if the ticket changed between calls.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Multiple enumeration | `Count()` then `Sum()` on same lazy sequence | DB/repository work runs twice; metrics and billing double-charge I/O |
| Correctness | No snapshot between passes | If lines change mid-report, count and sum can reflect different underlying data |
| API contract | Returning bare `IEnumerable<T>` from I/O | Callers cannot tell whether re-enumeration is cheap or expensive |

**Fix (priority order):**

1. Materialize once at the boundary that owns I/O: `var heavy = _service.GetHeavyLines(...).ToList();` then `Count` / `Sum` on the list.
2. Better API shape: return `IReadOnlyList<PickLine>` or `Task<IReadOnlyList<PickLine>>` from the service so multiple reads are explicit and cheap.
3. If only one pass is needed, use a single loop or one LINQ aggregate (`Aggregate`, custom scan) instead of two terminal operators.
4. Log/measure enumeration in reviews — treat `IEnumerable` from repositories as "run once unless documented otherwise."

```csharp
var heavy = _service.GetHeavyLines("PB-2201", 1.0m).ToList();
int lineCount = heavy.Count;
decimal totalKg = heavy.Sum(l => l.TotalWeightKg);
```

**Production takeaway:** Karat uses double enumeration to test whether you know `IEnumerable<T>` is a recipe, not a cached collection — matches **Program.cs** Section 5a (lazy until consumed) and Section 4h (each LINQ terminal op walks the sequence).

---

#### Q2. What is the difference between `ICollection<T>` and `IList<T>`?

(R) A custom `IEnumerator<PickLine>` wraps a file reader. A developer copies the manual loop from a tutorial but drops the `using` block. Under load, temp files pile up on disk. Review the loop. What is missing, and what does `foreach` do differently?

**Answer:** `IEnumerator<T>` implements `IDisposable` when the concrete enumerator holds unmanaged or file handles. Without `using` or a `finally` that calls `Dispose`, early `break` leaves the `StreamReader` open — file locks and temp directory growth follow. `foreach` always emits a `try/finally` that disposes the enumerator.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | No `Dispose()` on manual loop | Handles stay open until GC finalizer (if any) — unreliable |
| Early exit | `break` skips implicit cleanup | Worse under exceptions or conditional exit — matches tutorial pitfall in Section 4b |
| Pattern drift | Tutorial showed `using (IEnumerator<T> ...)` | Copy-paste without `using` loses the main safety net |

**Fix (priority order):**

1. Wrap manual iteration in `using`: `using IEnumerator<PickLine> walk = batch.GetEnumerator();` — same as **Program.cs** Section 4b.
2. Prefer `foreach` when you do not need the raw enumerator — compiler-generated dispose in `finally`.
3. If you must hold an enumerator across methods, implement `try/finally` with explicit `Dispose()` or use `await foreach` with `IAsyncEnumerable<T>` and `ConfigureAwait` patterns for async sources.
4. Add analyzer/code-review rule: any `GetEnumerator()` manual loop requires `using` or documented wrapper.

```csharp
using (IEnumerator<PickLine> walk = batch.GetEnumerator())
{
    while (walk.MoveNext())
    {
        Process(walk.Current);
        if (walk.Current.Sku.StartsWith("STOP"))
            break;
    }
} // Dispose even on break
```

**Production takeaway:** `foreach` is not syntactic sugar only — it is the correct dispose pattern for `IEnumerator<T>`. See **Program.cs** Section 2a (compiler `finally` → `Dispose`) and Section 3a (`Dispose()` on enumerators wrapping files/DB readers).

---

#### Q3. What are `IReadOnlyList<T>` and `IReadOnlyCollection<T>`?

(R) A batch-picking screen tries to skip short lines by removing them while iterating. It crashes on the second line every time. Review the loop (same pattern as **Program.cs** Section 4g). What throws, why is it allowed, and what is the safe fix?

**Answer:** `List<T>` tracks a version stamp for its enumerator. Adding or removing during `foreach` invalidates that enumerator and throws `InvalidOperationException` ("Collection was modified; enumeration operation may not execute.") — the runtime detects structural change, not logical intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` on same `List<T>` | Guaranteed `InvalidOperationException` after first mutation |
| Logic | In-place filter while walking forward | Even if it did not throw, skipping indices would drop unchecked elements |
| API misuse | Treating `foreach` like index-based `for` with `RemoveAt` | Common UI/service bug when "cleaning" collections live |

**Fix (priority order):**

1. Iterate a snapshot: `foreach (PickLine line in lines.ToList())` and mutate the original list — or build a new list of lines to remove.
2. Reverse `for` loop with index if you must remove in place: `for (int i = lines.Count - 1; i >= 0; i--)`.
3. Prefer `lines.RemoveAll(l => l.Quantity < 5)` then a second pass for `_picker.Assign`.
4. Never add to a list you are actively `foreach`-ing on the same instance — same exception as remove.

```csharp
lines.RemoveAll(l => l.Quantity < 5);
foreach (PickLine line in lines)
    _picker.Assign(line);
```

**Production takeaway:** The pick-ticket demo in **Program.cs** Section 4g exists because this fails in production UI code daily — Karat expects you to name `InvalidOperationException` and choose snapshot or `RemoveAll`, not "it worked once in a small list."

---

#### Q4. What is the difference between `IEnumerator` and `IEnumerator<T>`?

(M) A developer builds a lazy LINQ pipeline over live pick lines, logs the count, then mutates the underlying list before a second `foreach`. Results differ between the two passes. Walk through what runs when and why the second pass can change.

**Answer:** `Where` returns a deferred sequence — no filter runs until a terminal operation or `foreach` forces enumeration. The first `Count()` walks `pickList` at that moment; adding `RUSH-ADD` before the second `foreach` changes the source, so the second walk can include the new heavy line that was not counted in `previewCount`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Deferred execution | `heavy` stores query, not results | Developers think they captured "heavy lines at T0" — they captured a filter *recipe* |
| Live backing collection | `pickList` mutated between enumerations | Count and foreach disagree; audit logs show inconsistent totals |
| Mental model | LINQ chain looks like a new collection | No allocation until enumeration — easy to miss in code review |

**Fix (priority order):**

1. Materialize when you need a stable snapshot: `var heavy = pickList.Where(...).ToList();` before count and display.
2. Mutate a copy if the pipeline must stay tied to original ticket: iterate `pickList.ToList()` for reporting.
3. Document whether service methods return live views vs snapshots — return `IReadOnlyList<T>` when stable.
4. Single enumeration when possible: one loop that counts and prints, or `ToList()` once at API boundary.

**Production takeaway:** Deferred execution is a feature for composable LINQ, not a cache — production bugs appear when request handlers mutate shared lists between logging and processing. See **Program.cs** Section 5a and Section 4h preview.

---

#### Q5. What is the `yield` keyword, and how do iterator methods relate to `IEnumerable<T>`?

(M) An iterator method logs each SKU as it yields. A caller breaks out of `foreach` after the first match. Later code assumes every line was scanned. Review the iterator and caller. What does `yield return` guarantee about execution state, and when does work *not* run?

**Answer:** A `yield return` method compiles to a state machine that runs only until the consumer asks for the next element via `MoveNext`. Breaking out of `foreach` stops calling `MoveNext`, so the iterator body after the last yielded item never runs — remaining source lines are not scanned and `_metrics.RecordScan` is not called for them.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Partial enumeration | `break` after first `yield` consumed | Iterator paused mid-method; trailing source elements skipped |
| Side effects in iterator | `_metrics.RecordScan` inside yield path | Metrics under-count; ops dashboards lie when callers short-circuit |
| Assumption | "Calling the method processed the ticket" | Method invocation alone does nothing — only enumeration drives work |

**Fix (priority order):**

1. Move metrics to the caller after full materialization if full scans are required: `var reps = FirstMatchPerAisle(pickList).ToList();` then record — or record in caller loop without `break` if policy needs all aisles.
2. Split "dedupe yield" from "audit full ticket": one pass for metrics (`foreach` entire source), one lazy pass for shipping selection.
3. Use `yield break` only to end iteration early by design — document that early consumer exit is supported.
4. Avoid heavy side effects inside iterator bodies; prefer pure filters and explicit logging at materialization boundaries.

**Production takeaway:** `yield return` pauses the method, not completes it — Karat tests whether you explain compiler-generated `IEnumerator` state vs eager methods. See **Program.cs** Section 5 (`yield return` state machine) and Section 5a (work on demand only).

---

#### Q6. What is the difference between deferred execution and immediate execution for IEnumerable sequences?

(P) A code review flags `var lines = GetHeavyLines(...).ToList()` as "unnecessary allocation." The author argues it prevents double DB hits and stabilizes results if the ticket changes mid-request. When is `ToList()` (or `ToArray()`) the right production fix for `IEnumerable<T>`, and when is it waste?

**Answer:** Materialize when the sequence is expensive, non-idempotent, or tied to a live collection that may change before you finish multiple passes — or when you need count/index/random access. Skip `ToList()` when you have a single forward-only `foreach` over an in-memory collection and no shared mutation for the request lifetime.

**When `ToList()` / `ToArray()` is right:**

- Multiple terminal LINQ operations (`Count`, `Sum`, `Any`, then `foreach`) on the same deferred chain — Q1/Q4 pattern.
- `IEnumerable<T>` from `yield return`, database, or network where re-enumeration repeats I/O.
- Snapshot before parallel work, caching in a request scope, or passing to another thread — lists are safe snapshots; raw lazy sequences are not.
- Stabilizing results when the underlying `List<T>` may be edited during the same ASP.NET request.

**When it is waste:**

- One `foreach` over `List<T>` or an array — already materialized; `.ToList()` copies for no benefit.
- Known cheap sequences (small in-memory constants) where a second pass is still cheaper than allocation — measure, but default to clarity.
- Infinite or very large streams where materialization blows memory — use single-pass streaming instead.

**Production takeaway:** `ToList()` is not a micro-optimization debate — it documents "this is the snapshot boundary." Prefer returning `IReadOnlyList<T>` from services that already materialize so callers do not double-enumerate by accident. Aligns with **Program.cs** `HeavyLines` lazy filter vs **Section 4g** snapshot `new List<PickLine>(pickList)` before risky work.

---

#### Q7. What is the iterator pattern — what do `MoveNext`, `Current`, and `Reset` do?

(R) Two developers iterate the same `PickBatch` concurrently — one with `foreach`, one with a stored `IEnumerator<PickLine>` from an earlier `GetEnumerator()` call. Intermittent duplicates and skipped SKUs appear. Review `PickBatch` (fresh enumerator per `GetEnumerator()`). What contract did the second developer violate, and how should multiple consumers walk the same batch?

**Answer:** Each call to `GetEnumerator()` returns an independent cursor (`PickBatchEnumerator` with its own `_index`). That is correct. The bug is sharing one `IEnumerator` instance across logical passes or threads while also starting another enumeration — two cursors on the same batch are fine in sequence, but concurrent or overlapping manual + `foreach` walks without coordination produce duplicate/skipped processing. `IEnumerator<T>` is not thread-safe and not meant to be shared as shared state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cursor sharing | Reusing one `IEnumerator` mid-stream while another walk runs | Double-processing or skipped elements depending on interleaving |
| Threading | Concurrent `MoveNext` on same enumerator | Undefined behavior; not supported by BCL collections |
| Design | Treating enumerator as batch-wide singleton | Violates forward-only, one-consumer-per-cursor model |

**Fix (priority order):**

1. One enumerator per pass — never share a single `IEnumerator<T>` between components; call `GetEnumerator()` again (or `foreach`) for each full walk — prefer fresh enumerator over `Reset()` per **Program.cs** Section 4c note.
2. Do not advance a stored enumerator partially then also `foreach` the batch unless you explicitly want two independent views — document which cursor owns which lines.
3. For parallel processing, materialize `batch.ToList()` or index into an array and partition by range — not shared `MoveNext`.
4. Remove `Reset()` from new code paths; `PickBatchEnumerator.Reset()` exists for legacy only.

```csharp
// Two independent full passes — OK:
foreach (PickLine line in batch) ProcessA(line);
foreach (PickLine line in batch) ProcessB(line);

// Not OK: one half-consumed manual cursor + another walk without clear ownership
```

**Production takeaway:** `IEnumerable<T>` is multi-enumerable; `IEnumerator<T>` is single forward cursor — Karat collapses the distinction. See **Program.cs** Section 2 (`GetEnumerator()` fresh per call) and Section 3 (`PickBatchEnumerator` instance state).

---

#### Q8. What is `yield break` vs `return` in an iterator method?

_Answer not found._

---

#### Q9. Why can multiple enumeration of the same `IEnumerable` from a LINQ query re-run the pipeline?

_Answer not found._

---

#### Q10. What is the difference between returning `IEnumerable<T>` from a method vs `List<T>`?

_Answer not found._

---

#### Q11. What happens if you modify a collection during `foreach` — how does the enumerator detect it?

_Answer not found._

---

#### Q12. What is covariance on `IEnumerable<out T>` — practical assignment examples?

_Answer not found._

---

#### Q13. What is the difference between `foreach` and manual `while (enumerator.MoveNext())`?

_Answer not found._

---

#### Q14. What is `ToList()` materialization, and when must you materialize before multiple passes?

_Answer not found._

---

#### Q15. What is the relationship between `IAsyncEnumerable<T>` and iterators (preview)?

_Answer not found._

---

#### Q16. **Modify while iterating** — Changing a collection during `foreach` throws `InvalidOperationException`.

_Answer not found._

---

#### Q17. **Mutable keys** — Changing equality-relevant state on a key after insertion causes silent lookup failures.

_Answer not found._

---

#### Q18. **`IEnumerable<T>` covariant, `List<T>` not** — Covariance on mutable lists would break type safety.

_Answer not found._

---

#### Q19. **Wrong collection for the job** — Frequent middle inserts on `List<T>` are O(n).

_Answer not found._

---

#### Q20. **Static fields on generic types** — Separate static slots per closed generic type.

_Answer not found._

---

#### Q21. **Boxing in non-generic collections** — `ArrayList` boxes value types; `List<T>` avoids this.

_Answer not found._

---

#### Q22. **Passing `List<T>` by value** — Reference is copied; contents still shared.

_Answer not found._

---

#### Q23. **`Dictionary.Add` vs indexer on duplicate key** — `Add` throws; indexer overwrites silently.

_Answer not found._

---

#### Q24. **`AsReadOnly()` is a view** — Original list mutations remain visible through the wrapper.

_Answer not found._

---

#### Q25. **Assuming dictionary enumeration order** — Undefined; sort keys explicitly if order matters.

_Answer not found._

---

#### Q26. **Multiple enumeration cost** — `yield`/LINQ re-executes work each pass; materialize when needed.

_Answer not found._

---

#### Q27. **Wrong comparer on sorted types** — `SortedDictionary` uses `IComparer<TKey>`, not `IEqualityComparer<TKey>`.

_Answer not found._

---

#### Q28. **Poor `GetHashCode` distribution** — Constant hash codes degrade to O(n) buckets.

_Answer not found._

---

#### Q29. **Queue `Contains` is O(n)** — Use a `HashSet` alongside if you need fast membership checks.

_Answer not found._

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A teammate adds a generic repository helper for warehouse stock rows. `dotnet build` fails. Review the constraint stack — what is wrong, and how do you fix it?

```csharp
public static class StockRepository
{
    public static T LoadOrCreate<T>(string sku) where T : struct, StockEntry, new()
    {
        if (_cache.TryGetValue(sku, out T existing))
        {
            return existing;
        }

        T created = new T { Sku = sku };
        _cache[sku] = created;
        return created;
    }

    private static readonly Dictionary<string, StockEntry> _cache = new();
}
```

---

**Answer:**

**Answer:** `where T : struct, StockEntry, new()` is illegal — a type parameter cannot be both a non-nullable value type (`struct`) and a reference-type base class (`StockEntry`). The compiler rejects the constraint combination before any call site is evaluated.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `struct` + base class `StockEntry` on same `T` | CS0454 — mutually exclusive constraints; build blocked |
| Design | `_cache` stores `StockEntry` but method returns `T` with value-type constraint | Even if it compiled, boxing/unified cache semantics would be wrong |
| API misuse | `new T { Sku = sku }` assumes `T` is a reference type with mutable `Sku` | Value-type `T` could not inherit `StockEntry` anyway |

**Fix (priority order):**

1. Drop `struct` — use `where T : StockEntry, new()` if you truly need default-constructible inventory rows (`InventoryItem`, etc.).
2. If value-type rows are required, do **not** inherit `StockEntry`; use a separate generic struct path (e.g., `Quantity<TUnit> where TUnit : struct`) or a shared interface instead of a class base.
3. Type the cache as `Dictionary<string, T>` inside a generic class `StockRepository<T> where T : StockEntry, new()`, not a mixed `Dictionary<string, StockEntry>` with an inconsistent method signature.
4. Align with this chapter's `DescribeStockEntry<T> where T : StockEntry` — base-class constraints apply to reference types in the inheritance hierarchy.

**Production takeaway:** Constraint misuse is a compile-time gate — Karat tests whether you recognize that `class`/base-type and `struct` constraints exclude each other. See **Program.cs** Sections 7–9 — constraint combinations.

---

---

#### Q2. (R) A developer "fixes" a method that accepts any payload list by widening to `List<object>`. Review the assignment and call site:

```csharp
public static void AuditSkus(List<object> allSkus)
{
    foreach (object sku in allSkus)
    {
        Console.WriteLine(sku);
    }
}

List<string> warehouseSkus = new() { "WH-4412", "WH-9901" };
AuditSkus(warehouseSkus); // CS1503 — cannot convert List<string> to List<object>

// Developer tries IEnumerable instead:
IEnumerable<object> widened = warehouseSkus;
foreach (object item in widened)
{
    var mutable = (List<object>)widened; // attempted cast at runtime
    mutable.Add(42);
}
```

What fails at compile time vs runtime, and what is the safe pattern for read-only aggregation?

---

**Answer:**

**Answer:** `List<T>` is **invariant** — `List<string>` is not assignable to `List<object>` because that would allow adding non-strings through the wider reference. Covariance applies only on interfaces like `IEnumerable<out T>` for **read-only** projection, not on mutable lists.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `AuditSkus(warehouseSkus)` with `List<object>` parameter | CS1503 — cannot convert `List<string>` to `List<object>` |
| Runtime | Cast `IEnumerable<object>` to `List<object>` and `Add(42)` | `InvalidCastException` — sequence is backed by `List<string>`, not `List<object>` |
| Design | Treating covariance as "free widening" for mutable collections | Silent data corruption if the language allowed it — type safety violated |

**Fix (priority order):**

1. For read-only aggregation, accept `IEnumerable<string>` or `IReadOnlyList<string>` — precise, no widening needed.
2. For heterogeneous payloads, use `List<object>` at the **source** (accept the boxing cost consciously) or a discriminated model (`List<StockPayload>` / union type).
3. Use `IEnumerable<object> widened = warehouseSkus` only when consuming items — never cast back to a mutable `List<object>` to add elements.
4. Remember: `IEnumerable<out T>` covariance lets you pass `IEnumerable<string>` where `IEnumerable<object>` is expected, but you still cannot mutate element types.

**Production takeaway:** Confusing `List<T>` invariance with `IEnumerable<out T>` covariance is a common review failure — matches **Program.cs** Section 13 and Quick Reference variance rows.

---

---

#### Q3. (R) An API endpoint helper should return the larger of two comparable stock metrics without boxing value types. Review the call chain:

```csharp
public static T MaxOf<T>(T left, T right) where T : IComparable<T>
{
    return left.CompareTo(right) >= 0 ? left : right;
}

decimal priceA = 19.99m;
int unitsB = 120;
var winner = MaxOf(priceA, unitsB); // CS0411 — type arguments cannot be inferred

// After "fix" — explicit type args:
var forced = MaxOf<decimal>(priceA, unitsB); // CS1503 — int not convertible to decimal
```

What broke inference, why does the explicit fix still fail, and how would you design this helper for production?

---

**Answer:**

**Answer:** Generic method inference requires a **single** type argument `T` that fits both parameters — `decimal` and `int` disagree, so the compiler cannot infer `T` (CS0411). Forcing `MaxOf<decimal>` then fails because `int` is not implicitly convertible to `decimal` at the call site (CS1503).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `MaxOf(priceA, unitsB)` — mismatched argument types | CS0411 — type arguments cannot be inferred from the arguments |
| Compile | `MaxOf<decimal>(priceA, unitsB)` | CS1503 — `int` cannot be passed where `decimal` expected |
| Design | One generic `MaxOf<T>` used across unrelated metrics | API encourages comparing apples to units — domain error masked as generic error |

**Fix (priority order):**

1. Call with **homogeneous** types: `MaxOf(priceA, otherPrice)` or `MaxOf(unitsA, unitsB)`.
2. If conversion is intentional, convert explicitly **before** the call: `MaxOf(priceA, (decimal)unitsB)` — documents that the comparison is cross-domain and may be wrong business-wise.
3. Prefer domain methods (`MaxPrice`, `MaxUnits`) or `INumber<T>` (.NET 7+) helpers where numeric widening is well-defined.
4. Do not rely on inference when types differ — specify intent at the call site or split overloads.

**Production takeaway:** Inference failures often signal a design smell — Karat checks that you read CS0411/CS1503 as "one T for all parameters," not as a compiler bug. See **Program.cs** Section 8 — `Swap<T>` inference requires matching types.

---

---

#### Q4. (M) A hot inventory path stores millions of pallet counts per hour. One service uses `List<object>` "for flexibility"; another uses `List<int>`. Review the read loop:

```csharp
List<object> legacyCounts = new();
for (int i = 0; i < 1_000_000; i++)
{
    legacyCounts.Add(i); // boxed int on every Add
}

int legacySum = 0;
foreach (object boxed in legacyCounts)
{
    legacySum += (int)boxed; // unbox per iteration
}

List<int> genericCounts = new(capacity: 1_000_000);
for (int i = 0; i < 1_000_000; i++)
{
    genericCounts.Add(i); // no boxing
}

int genericSum = genericCounts.Sum();
```

Under JIT/AOT, what does the runtime do differently for `List<int>` vs `List<object>`, and when would you still accept the legacy shape?

---

**Answer:**

**Answer:** For each closed constructed type, the JIT specializes `List<T>.Add` and indexer access — `List<int>` stores unboxed ints in a `T[]` with no per-element heap boxing, while `List<object>` boxes every `int` on `Add` and unboxes on read, doubling heap traffic and cache pressure.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / GC | Boxing 1M ints into `List<object>` | 1M heap allocations + GC pressure; slower hot loop |
| Runtime | Unbox + cast in `legacySum` loop | Extra CPU per iteration vs direct `int` access |
| JIT | Shared vs specialized code paths | `List<int>` gets efficient `int[]` storage; `List<object>` always handles references |
| Design | "Flexibility" on a numeric hot path | Latency spikes under load; harder to reason about in profiling |

**Fix (priority order):**

1. Use `List<int>` (or `Span<int>`, `int[]`, `ImmutableArray<int>`) for homogeneous numeric streams — matches **LegacyCollectionProbe** vs `List<int>` in **Program.cs** Section 1.
2. If mixed types are required, isolate boxing to boundaries (parse → strongly typed model) rather than the inner loop.
3. Accept `List<object>` only at integration seams (legacy APIs, `ArrayList` interop) with explicit conversion at the edge.
4. Profile with dotMemory / PerfView — boxed collections show as `System.Int32` allocations in GC heaps.

**Production takeaway:** Generics exist partly to eliminate boxing on value-type collections — Layer 2 tests whether you connect language feature to production GC behavior, not just "compile-time safety."

---

---

#### Q5. (R) A factory method should default-construct inventory DTOs for an import pipeline. Review:

```csharp
public sealed record ImportedLine(string Sku, int Units);

public static class ImportFactory
{
    public static T CreateRow<T>() where T : new()
    {
        return new T();
    }
}

// Startup:
var row = ImportFactory.CreateRow<ImportedLine>();
row.Sku = "WH-4412";

// Alternate path — value-type wrapper:
public readonly struct PalletTag
{
    public PalletTag(int zoneId) => ZoneId = zoneId;
    public int ZoneId { get; }
}

var tag = ImportFactory.CreateRow<PalletTag>();
```

What compiles, what fails, and how do you constrain factories correctly for records vs structs?

---

**Answer:**

**Answer:** `CreateRow<ImportedLine>()` succeeds — records with a primary constructor still get a synthesized parameterless constructor for `new()` when not explicitly removed. `CreateRow<PalletTag>()` fails — `PalletTag` only declares `PalletTag(int zoneId)`, so it does not satisfy `where T : new()` (CS0310).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `CreateRow<PalletTag>()` | CS0310 — `PalletTag` must have public parameterless constructor |
| Design | `new()` constraint on a factory used for both records and custom structs | Call sites look uniform but only some types qualify |
| Runtime / API | Mutating `row.Sku` on a record instance | Works here, but immutable record designs may prefer `with` instead of post-`new()` mutation |

**Fix (priority order):**

1. For `PalletTag`, add an explicit parameterless ctor **only if** default construction is valid: `public PalletTag() : this(0) { }` — or stop using `new()` for that type.
2. Split factories: `CreateRecord<T>() where T : new()` for DTOs; dedicated `PalletTag CreateTag(int zoneId)` for parameterized structs.
3. Prefer `Activator.CreateInstance<T>()` or DI-backed factories when construction needs parameters or injection — `new()` is for simple default graphs only.
4. Validate at compile time with tests that call `CreateRow<T>()` for every supported import row type.

**Production takeaway:** The `new()` constraint means "public parameterless constructor exists" — not "any struct" or "any record." See **Program.cs** `CreateDefault<T>() where T : new()` and Section 9a.

---

---

#### Q6. (R) A library author exposes typed domain exceptions via generics "so callers can catch exactly what they need." Review:

```csharp
public static class StockGuard
{
    public static void EnsurePositive<TException>(int units)
        where TException : Exception, new()
    {
        if (units <= 0)
        {
            throw new TException();
        }
    }

    public static void Reserve(string sku, int units)
    {
        EnsurePositive<ArgumentOutOfRangeException>(units);

        if (!IsKnownSku(sku))
        {
            EnsurePositive<InvalidOperationException>(0); // reuses generic throw
        }
    }
}

// Consumer:
try
{
    StockGuard.Reserve("WH-0000", -5);
}
catch (ArgumentOutOfRangeException ex)
{
    _logger.LogWarning(ex, "Bad quantity for {Sku}", sku);
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Unknown SKU workflow failure");
}
```

What is wrong with generic exception throwing, what breaks observability and API contracts, and what pattern replaces it?

---

---

### 02. ArrayList

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/02. ArrayList`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `throw new TException()` where `TException : Exception, new()` produces **parameterless** exceptions with no message, no inner exception, and no structured context — callers catch the right type but lose SKU, quantity, and stack context. Reusing the helper for `InvalidOperationException` by passing `0` to `EnsurePositive` is a semantic hack that obscures intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Generic exception factory anti-pattern | Empty exceptions — useless logs and support tickets |
| Observability | `new TException()` only — no message/data | `_logger.LogWarning(ex, …)` has nothing actionable; APM groups by type only |
| API contract | `EnsurePositive<InvalidOperationException>(0)` for unknown SKU | Wrong exception type and wrong guard — conflates validation with business rules |
| Maintainability | Callers depend on **type** not **error shape** | Adding fields/codes requires new exception types instead of stable error codes |

**Fix (priority order):**

1. Throw **specific, constructed** exceptions: `throw new ArgumentOutOfRangeException(nameof(units), units, "Units must be positive.");`
2. Replace generic throw helpers with domain exceptions (`UnknownSkuException`) or `Result`/validation types for expected failures.
3. Use `ExceptionDispatchInfo` or `throw;` to preserve stack when rethrowing — never `throw new TException()` as a stand-in for wrapping.
4. For libraries, document thrown types in XML docs; avoid letting consumers catch generic `TException` via your helper.

```csharp
if (units <= 0)
{
    throw new ArgumentOutOfRangeException(nameof(units), units, "Reserve quantity must be positive.");
}

if (!IsKnownSku(sku))
{
    throw new InvalidOperationException($"SKU '{sku}' is not in the catalog.");
}
```

**Production takeaway:** Generics + `new()` on exceptions looks clever but fights .NET exception design — production code favors explicit throws with messages and structured error models. See foundation **Exception Handling** — `throw` vs `throw ex` for stack preservation when rethrowing.

---

---

### 02. ArrayList

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/02. ArrayList`

---

---

#### Q1. (R) A legacy warehouse service stores pick lines in an `ArrayList`. After a refactor, production throws `InvalidCastException` during the nightly export. Review the code — what failed, and why did it compile?

```csharp
ArrayList warehouseLines = LoadLinesFromDatabase(); // returns mixed legacy rows

decimal totalValue = 0m;
foreach (object entry in warehouseLines)
{
    Product product = (Product)entry;
    totalValue += product.ProductPrice;
}
```

A teammate added this line to support rush SKUs before the export job runs:

```csharp
warehouseLines.Add("RUSH-PICK");
```

---

**Answer:**

```csharp
ArrayList warehouseLines = LoadLinesFromDatabase(); // returns mixed legacy rows

decimal totalValue = 0m;
foreach (object entry in warehouseLines)
{
    Product product = (Product)entry;
    totalValue += product.ProductPrice;
}
```

A teammate added this line to support rush SKUs before the export job runs:

```csharp
warehouseLines.Add("RUSH-PICK");
```

**Answer:** The export loop assumes every `ArrayList` element is a `Product`, but `Add("RUSH-PICK")` stores a `string` — the cast `(Product)entry` throws `InvalidCastException` at runtime because `ArrayList.Add` accepts any `object` with no compile-time type check.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Type safety | `ArrayList` allows heterogeneous `Add` | Wrong runtime type slips in; compile succeeds |
| Runtime | `(Product)entry` on a `string` | `InvalidCastException` — nightly job fails |
| Design | Mixed domain types in one bag | Same fragility as **Program.cs** Section 5 CRUD demo (`Add(250)` beside `Product`) |
| Maintainability | Implicit contract "all items are Product" | No compiler enforcement; code review must catch bad `Add` |

**Fix (priority order):**

1. Remove the string from the product list — store rush flags on `Product` or use a separate collection.
2. Migrate `warehouseLines` to `List<Product>` so `Add("RUSH-PICK")` fails at compile time.
3. Short-term guard: use pattern matching (`entry is Product p`) and log/skip invalid rows instead of blind cast — stops the crash but hides data quality issues.
4. Add an integration test that runs the export against a fixture mirroring legacy mixed data.

**Production takeaway:** `ArrayList` defers type errors to production — Karat uses this to test whether you connect "it compiled" with "nothing checked the element type." See **Program.cs** Sections 2 and 7 — indexer and `foreach` return `object`.

---

---

#### Q2. (R) A sensor-ingestion job stores telemetry in an `ArrayList` and unboxes on read. Under load, GC pressure spikes and one pod crashes intermittently. Review the hot path — what is wrong at the storage layer and on read?

```csharp
ArrayList readings = new ArrayList(capacity: 10_000);

for (int i = 0; i < 10_000; i++)
{
    readings.Add(i); // sensor count snapshot
}

int peak = (long)readings[0]; // "fix" after a code review comment
```

---

**Answer:**

```csharp
ArrayList readings = new ArrayList(capacity: 10_000);

for (int i = 0; i < 10_000; i++)
{
    readings.Add(i); // sensor count snapshot
}

int peak = (long)readings[0]; // "fix" after a code review comment
```

**Answer:** Each `Add(i)` boxes the `int` onto the heap, creating 10,000 extra allocations and GC pressure; the read then uses `(long)` on a boxed `int`, which throws `InvalidCastException` because unboxing requires the exact original type — you cannot unbox a boxed `int` directly to `long`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Boxing every `int` on `Add` | Heap allocations, GC churn under load |
| Runtime | `(long)readings[0]` unboxes boxed `int` as `long` | `InvalidCastException` — intermittent pod crash |
| API misuse | `ArrayList` for homogeneous numeric telemetry | Wrong tool when all elements are `int` |
| "Fix" regression | Widening cast on unbox | Confuses numeric widening with unboxing rules |

**Fix (priority order):**

1. Replace with `List<int>` — no boxing for value-type elements; indexer returns `int` directly.
2. Correct read: `int peak = (int)readings[0]!` only if staying on `ArrayList`; prefer `List<int>` so no cast is needed.
3. If values can exceed `int`, use `List<long>` from the start — store the wider type without boxing.
4. Profile Gen0/Gen1 collections after migration to confirm allocation drop.

**Production takeaway:** Boxing is invisible in small demos but measurable in hot loops — Karat pairs GC symptoms with the `Add(object)` signature. See **Program.cs** Section 9 — boxing on `Add(42)` and correct `(int)` unbox.

---

---

#### Q3. (R) A catalog API still exposes `IList` for backward compatibility. New code assumes every element is a `Product`. Review this controller helper — what breaks at runtime, and what compile-time safety is missing?

```csharp
public decimal GetCatalogTotal(IList catalog)
{
    decimal total = 0m;
    for (int i = 0; i < catalog.Count; i++)
    {
        total += ((Product)catalog[i]!).ProductPrice;
    }
    return total;
}

// Caller from legacy batch job:
IList legacyCatalog = new ArrayList
{
    new Product { ProductNo = 10, ProductName = "Scanner", ProductPrice = 89.50m },
    250 // legacy quantity field stored inline before Product migration
};
GetCatalogTotal(legacyCatalog);
```

---

**Answer:**

```csharp
public decimal GetCatalogTotal(IList catalog)
{
    decimal total = 0m;
    for (int i = 0; i < catalog.Count; i++)
    {
        total += ((Product)catalog[i]!).ProductPrice;
    }
    return total;
}

// Caller from legacy batch job:
IList legacyCatalog = new ArrayList
{
    new Product { ProductNo = 10, ProductName = "Scanner", ProductPrice = 89.50m },
    250 // legacy quantity field stored inline before Product migration
};
GetCatalogTotal(legacyCatalog);
```

**Answer:** Index 1 holds a boxed `int` (250), not a `Product` — `((Product)catalog[i]!)` throws `InvalidCastException` on the second iteration. The method compiles because `IList` indexer returns `object?` and the cast is explicit; no compile-time guarantee exists that callers populated the list homogeneously.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Contract | `IList` accepts any element type | Callers can pass legacy mixed `ArrayList` |
| Runtime | Cast `(Product)` on boxed `int` | API call fails mid-loop |
| API design | Non-generic `IList` parameter | Hides intended element type from callers and reviewers |
| Migration debt | Legacy row shape (`250` inline) coexists with `Product` | Data migration incomplete but new code assumes completion |

**Fix (priority order):**

1. Change signature to `IReadOnlyList<Product>` or `List<Product>` — mixed `Add` fails at compile time on the caller side when they migrate.
2. Add a dedicated DTO mapper at the legacy boundary that converts raw rows to `Product` before calling business logic — never pass raw `ArrayList` into domain code.
3. Interim: validate with `catalog[i] is Product` and throw a descriptive error listing index and runtime type.
4. Deprecate `GetCatalogTotal(IList)` once batch jobs are updated; track call sites.

**Production takeaway:** Programming against non-generic `IList`/`ICollection` was necessary pre-generics — modern code should not treat it as a typed list. See **Program.cs** Section 8 — `IList` polymorphism and Section 10 — catalog with manual casts.

---

---

#### Q4. (P) Your team is migrating a .NET Framework inventory module that uses `ArrayList` for product catalogs, `Hashtable` for SKU→bin lookup, and manual `(Product)` casts in every loop. What is your migration plan to modern generic collections, and what do you change first to stop runtime cast failures?

---

**Answer:**

**Answer:** Migrate at the boundaries first — replace internal storage with `List<Product>` and `Dictionary<string, string>` (or appropriate typed keys/values), then narrow public APIs from `IList`/`Hashtable` to generic interfaces so new code cannot inject wrong types; leave thin adapter shims for external callers until call sites are updated.

- **Phase 1 — stop the bleeding:** Identify hot paths throwing `InvalidCastException` (catalog totals, export loops). Convert those `ArrayList` instances to `List<Product>` at the point of creation; map legacy rows in one factory method rather than scattering casts.
- **Phase 2 — keyed lookup:** Replace `Hashtable skuToBin` with `Dictionary<string, string>` — eliminates boxing on value types and `as`/cast on values. See **Program.cs** Section 12.
- **Phase 3 — API surface:** Change method parameters from `IList` to `IReadOnlyList<Product>` or `IEnumerable<Product>`; keep obsolete overloads that copy into `List<Product>` with validation for remaining legacy callers.
- **Phase 4 — satellite types:** Migrate `Stack`/`Queue`/`SortedList` usages to `Stack<T>`, `Queue<T>`, `SortedList<TKey,TValue>` as touched (Section 13 preview).
- **Testing:** Characterization tests with production-like mixed `ArrayList` fixtures; assert migrated code either rejects bad rows or maps them explicitly — never silent cast.
- **Do not big-bang** every file — migrate by vertical slice (catalog service end-to-end) so each PR is deployable.

**Production takeaway:** Migration priority is runtime cast failures and public boundaries, not alphabetical file renames — Karat tests whether you know *where* generics buy safety first.

---

---

#### Q5. (M) Two implementations compute the same warehouse capacity check. One uses `ArrayList`, one uses `List<int>`. A performance test shows the `ArrayList` version allocates more and runs slower on .NET 8. Explain the mechanism — what happens on each `Add` for value types, and why does `List<int>` avoid it?

```csharp
// Version A — legacy
ArrayList slots = new ArrayList(50_000);
for (int i = 0; i < 50_000; i++)
    slots.Add(i);

// Version B — migrated
List<int> slots = new List<int>(50_000);
for (int i = 0; i < 50_000; i++)
    slots.Add(i);
```

---

**Answer:**

**Answer:** `ArrayList.Add` takes `object`, so each `int` is boxed into a separate heap object stored in the internal `object[]`; `List<int>` stores ints directly in its `T[]` backing array with no boxing because the generic type parameter is known at compile time.

- **`ArrayList.Add(i)`:** `int` → boxed `object` (heap allocation + copy) → reference stored in `object[]`. 50,000 iterations ⇒ 50,000 box allocations plus array resizing copies.
- **`List<int>.Add(i)`:** `int` written inline into `int[]` — same amortized growth strategy as `ArrayList`, but no per-element heap wrapper.
- **Read path:** `ArrayList` indexer returns `object` → unbox cast; `List<int>` indexer returns `int` — fewer instructions, no unbox.
- **GC:** Boxed objects are short-lived Gen0 garbage; high-frequency adds inflate collection frequency and cache pressure — matches the pod/GC story in Q2.
- **Capacity hint:** Both honor initial capacity (`new ArrayList(50_000)` / `new List<int>(50_000)`) to reduce resize copies — boxing cost remains unique to `ArrayList` for value types.

**Production takeaway:** Same Big-O for `Add`, different constant factors and allocation profile — Karat expects you to name boxing/unboxing, not just "generics are faster." See **Program.cs** Sections 4 and 11 — capacity behavior and `ArrayList` vs `List<T>` comparison table.

---

---

#### Q6. (D) A monolith has 40 call sites passing `ArrayList` into methods typed as `IList`. Full rewrite to `List<T>` is blocked for two sprints. What incremental strategy reduces `InvalidCastException` risk without a big-bang change, and where do you draw the line on leaving `ArrayList` in place?

---

---

### 03. List

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/03. List`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Introduce typed wrappers and validated adapters at the edges — new code accepts `IReadOnlyList<T>`; legacy `ArrayList` flows through a single conversion layer that validates or maps elements — and freeze new `ArrayList` usage via analyzer or review rule while migrating call sites by module.

- **Immediate guardrails:** Ban new `ArrayList`/`new ArrayList()` in product code (Roslyn analyzer or `.editorconfig` convention); allow only in the compatibility adapter project.
- **Adapter pattern:** `static List<Product> ToProductList(IList legacy)` — foreach with `is Product` check; throw `InvalidOperationException` with index and type on first bad element (fail fast at boundary, not deep in business logic).
- **Strangler order:** Migrate leaf utilities with no downstream `IList` exports first; then services; public API last — each sprint removes a cluster of call sites, not random files.
- **Interface bridge:** Obsolete `void Process(IList items)` → add `Process(IReadOnlyList<Product> items)`; old overload converts via adapter and logs `[Obsolete]` warning to track remaining callers.
- **Draw the line — keep `ArrayList` temporarily only:** inside isolated interop with external legacy binaries you cannot change, or serialized blobs you have not migrated yet — never in new domain logic.
- **Do not** half-migrate by sprinkling `(Product)` casts — that preserves runtime risk; centralize casts once.

**Production takeaway:** Incremental migration is about *typed boundaries* and *fail-fast validation*, not leaving 40 unchecked cast sites — Karat tests pragmatic legacy strategy, not "rewrite everything day one."

---

---

### 03. List

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/03. List`

---

---

#### Q1. (R) A nightly import job loads 500,000 shipment SKUs into a `List<string>` by calling `Add` one at a time in a loop. Memory profiling shows repeated large allocations and GC pressure. Review the pattern below. What is happening internally, and how would you fix it?

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

---

**Answer:**

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

**Answer:** Each time `Count` exceeds `Capacity`, `List<T>` allocates a new backing array (typically double the previous size), copies every existing element, and discards the old array — so repeated growth on a half-million-item load causes many intermediate large allocations and full copies before the final size is reached.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | No initial capacity hint | Repeated resize + copy: O(n) per growth step → O(n²) total copy work for n adds |
| Memory | Discarded backing arrays until GC | GC pressure spikes during bulk import; LOH pressure for large string lists |
| Operability | `Clear()` later keeps high `Capacity` | Memory retained after import if list is reused without `TrimExcess()` |

**Fix (priority order):**

1. Pre-size when count is known or estimable: `new List<string>(capacity: 500_000)` or `new List<string>(feedLines as ICollection<string> ?? feedLines.ToList())` when the source exposes count.
2. Prefer `AddRange` over per-item `Add` when inserting a batch — one resize check for the whole range.
3. After bulk deletes, call `TrimExcess()` if the list will stay small long-term to release unused backing array memory.
4. For truly massive feeds, consider streaming processing instead of materializing everything into one list.

```csharp
public static List<string> LoadSkusFromFeed(IReadOnlyCollection<string> feedLines)
{
    var skus = new List<string>(feedLines.Count);
    foreach (var line in feedLines)
    {
        skus.Add(line.Trim());
    }
    return skus;
}
```

**Production takeaway:** `List<T>` growth is amortized O(1) per `Add`, but only if you avoid pathological resize storms — Karat tests whether you know `Capacity` doubles (0 → 4 → 8 → 16 …) and that pre-sizing is a one-line production win. See **Program.cs** Section 3 — Count / Capacity.

---

---

#### Q2. (R) A warehouse service removes cancelled dock labels during iteration. In staging it throws intermittently. Review this method — what breaks, and what is the correct fix?

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

---

**Answer:**

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

**Answer:** Modifying a `List<T>` while iterating it with `foreach` invalidates the enumerator — the runtime throws `InvalidOperationException` ("Collection was modified") as soon as `Remove` shifts elements and bumps the list's version.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` | `InvalidOperationException` — job fails mid-purge |
| Correctness | Only first match removed per value anyway | Even if it didn't throw, partial removal + skipped items after shift |
| API choice | `Remove(object)` scans from start each call | O(n²) for many cancellations on a large list |

**Fix (priority order):**

1. **Iterate backwards by index** when removing in-place: `for (int i = dockLabels.Count - 1; i >= 0; i--)` then `RemoveAt(i)` — backward removal avoids index skips.
2. **Prefer `RemoveAll`** for predicate-based bulk delete: `dockLabels.RemoveAll(label => cancelled.Contains(label))` — single pass, no enumerator invalidation.
3. **Rebuild** if most items are removed: `dockLabels.RemoveAll(...)` or filter to a new list and replace reference.
4. Never call `Add`, `Insert`, `Remove`, `Clear`, or `Sort` on a collection during `foreach` on that same collection.

```csharp
public void PurgeCancelledLabels(List<string> dockLabels, HashSet<string> cancelled)
{
    dockLabels.RemoveAll(label => cancelled.Contains(label));
}
```

**Production takeaway:** This is one of the most common collection bugs in production services — Karat embeds it in realistic warehouse code to see if you diagnose enumerator invalidation, not just "don't modify while looping." See **Program.cs** Section 2 — CRUD / RemoveAll.

---

---

#### Q3. (M) A shipment validator checks whether each incoming pallet's SKU already exists in a queue of 50,000 items by calling `IndexOf` inside a loop. What is the performance problem, and what structure would you use instead?

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0)
        {
            return false;
        }
    }
    return true;
}
```

*(Assume `ShipmentItem` does not override `Equals` / `GetHashCode`.)*

---

**Answer:**

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    foreach (var item in incoming)
    {
        if (queue.IndexOf(item) < 0)
        {
            return false;
        }
    }
    return true;
}
```

**Answer:** `IndexOf` performs a linear scan O(n) over the entire list for every incoming item, giving O(n × m) behavior — and because `ShipmentItem` uses reference equality by default, the check may not even match logically equal SKUs unless `Equals` is overridden.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `IndexOf` in outer loop | 50k × incoming count comparisons — timeouts under peak load |
| Correctness | Default reference equality on `ShipmentItem` | Two objects with same SKU may not compare equal — false negatives |
| Design | List is ordered sequence, not lookup index | Wrong tool for membership-by-key checks |

**Fix (priority order):**

1. Build a **`HashSet<string>`** (or `Dictionary<string, ShipmentItem>`) of queued SKUs once — O(1) average lookup per incoming item.
2. If order must be preserved **and** you need key lookup, maintain **both**: `List<ShipmentItem>` for order + `HashSet<string>` for membership (common production pattern).
3. Override **`Equals`/`GetHashCode`** on `ShipmentItem` by SKU if set semantics should match domain identity — required for `IndexOf`/`Contains` to work by value.
4. Use **`Exists(predicate)`** only for single checks — still O(n) per call; does not fix the nested-loop cost.

```csharp
public bool AllSkusAlreadyQueued(List<ShipmentItem> queue, IEnumerable<ShipmentItem> incoming)
{
    var queuedSkus = new HashSet<string>(queue.Select(q => q.Sku));
    return incoming.All(item => queuedSkus.Contains(item.Sku));
}
```

**Production takeaway:** `List<T>` search helpers (`IndexOf`, `Contains`, `Find`) are fine for small lists or rare checks — Karat uses scale (50k items) to force the jump to hash-based lookup. See **Program.cs** Section 12 — Dictionary preview vs List scan.

---

---

#### Q4. (D) Two developers search a pallet-count list for the first value over 20. One uses `List.Find`; the other uses LINQ `FirstOrDefault`. When would you prefer each, and what subtle difference matters for value types?

```csharp
List<int> palletCounts = GetPalletCounts();

int a = palletCounts.Find(n => n > 20);
int b = palletCounts.FirstOrDefault(n => n > 20);
```

---

**Answer:**

```csharp
List<int> palletCounts = GetPalletCounts();

int a = palletCounts.Find(n => n > 20);
int b = palletCounts.FirstOrDefault(n => n > 20);
```

**Answer:** For `List<int>`, both scan from index 0 and stop at the first match — behavior is equivalent here — but `Find` avoids LINQ's iterator allocation and is the idiomatic in-place search on `List<T>`; the important trap is that both return **`default(T)`** when nothing matches (`0` for `int`, not "no result").

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `default(int)` is `0` when no match | Cannot distinguish "found zero" from "not found" without `Exists` or nullable |
| Performance | LINQ adds delegate + enumerator overhead | Negligible on small lists; matters in hot loops on large lists |
| Consistency | Mixing styles across codebase | Team readability — pick one pattern per layer |

**When to prefer each:**

- **`List.Find` / `Exists` / `FindAll`:** Hot paths on materialized `List<T>` already in memory; mutating-list APIs (`FindAll` returns new list); no extra `using System.Linq`.
- **LINQ (`FirstOrDefault`, `Where`, `Any`):** Composing over `IEnumerable<T>`, deferred pipelines, or when the source may not be a list — keeps query chains uniform.
- **Neither alone for "maybe absent" value types:** Use `int? result = palletCounts.Cast<int?>().FirstOrDefault(n => n > 20)` or check `Exists` first, or return a tuple/bool+value.

**Production takeaway:** Karat tests API semantics, not LINQ religion — `Find` vs `FirstOrDefault` on a `List<int>` is a wash for performance, but **`default(T)` ambiguity** on value types breaks business rules silently. See **Program.cs** Section 4 — Find / Exists.

---

---

#### Q5. (R) A `ShipmentQueueService` exposes its internal lane list directly to API callers. Review the property and usage — what can go wrong in production, and how would you expose the data safely?

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void Reassign(string sku, string lane)
    {
        _lanes.Add(lane);
    }
}

// Controller
var lanes = _queueService.Lanes;
lanes.Clear();
lanes.Add("Hijacked-Lane");
```

---

**Answer:**

```csharp
public class ShipmentQueueService
{
    private readonly List<string> _lanes = new() { "Lane-1", "Lane-2" };

    public List<string> Lanes => _lanes;

    public void Reassign(string sku, string lane)
    {
        _lanes.Add(lane);
    }
}

// Controller
var lanes = _queueService.Lanes;
lanes.Clear();
lanes.Add("Hijacked-Lane");
```

**Answer:** Returning the live `List<string>` breaks encapsulation — any caller can mutate, clear, or replace elements in the service's internal state without going through `Reassign`, causing invariant violations and race conditions if the service is shared.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Design | Public mutable collection escape | Callers bypass validation; `Clear()` wipes production lanes |
| Encapsulation | `List<T>` exposes `Add`/`Remove`/`Sort` | Cannot audit or log changes; hard to evolve to new rules |
| Concurrency | Shared list reference across requests | One request mutates while another reads — corrupt state under load |
| API contract | Return type promises mutability | Consumers depend on side-effecting the service internals |

**Fix (priority order):**

1. Expose **`IReadOnlyList<string>`** backed by **`AsReadOnly()`** or return **`_lanes.ToArray()`** / **`[.. _lanes]`** snapshot when callers must not see live mutations.
2. Prefer **`IReadOnlyList<string> Lanes => _lanes.AsReadOnly()`** for a live read-only view — note underlying list changes still appear (Section 8 behavior).
3. For strict immutability from outside, return a **copy**: `return _lanes.ToList()` or `return (IReadOnlyList<string>)_lanes.ToArray()` — higher allocation, safest for public APIs.
4. Route all mutations through **methods** on the service (`AddLane`, `RemoveLane`) that enforce rules and logging.

```csharp
public IReadOnlyList<string> Lanes => _lanes.AsReadOnly();

public void AddLane(string lane)
{
    if (string.IsNullOrWhiteSpace(lane)) throw new ArgumentException(nameof(lane));
    _lanes.Add(lane);
}
```

**Production takeaway:** `AsReadOnly()` prevents mutation through the wrapper but not through leaked `List<T>` references — Karat stacks encapsulation + API surface design. See **Program.cs** Section 8 — AsReadOnly.

---

---

#### Q6. (P) A singleton background worker and several API threads share one static `List<ShipmentItem>` for the live shipment queue. Under load, counts become wrong and the process occasionally throws. Explain why `List<T>` is unsafe here and what pattern you would use instead.

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

---

### 04. Dictionary

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

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

**Answer:** `List<T>` is not thread-safe — concurrent `Add`, `RemoveAt`, and reads can corrupt internal array state, lose elements, throw `ArgumentOutOfRangeException`, or throw during enumeration because another thread resized or removed items mid-operation.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `Add` + `RemoveAt` | Lost updates, torn reads, occasional exceptions |
| Correctness | `Count > 0` then `[0]` is not atomic | Another thread can dequeue between check and index — race |
| Architecture | Static mutable shared state | Cannot scale out across processes; hidden global coupling |
| Observability | Intermittent failures under load | Passes locally; fails in production peak traffic |

**Fix (priority order):**

1. **`lock` around all queue operations** on a private list if you must share in-process state — simplest fix, limits throughput.
2. Prefer **`ConcurrentQueue<ShipmentItem>`** or **`Channel<ShipmentItem>`** for producer/consumer patterns — designed for concurrent enqueue/dequeue.
3. Remove **static mutable** queues from business logic — inject a **scoped or singleton service** with explicit thread-safe storage; use database/message broker for multi-instance deployments.
4. Never expose the raw list publicly (see Q5) — wrap in a thread-safe API.

```csharp
private static readonly object Gate = new();
private static readonly List<ShipmentItem> LiveQueue = new();

public static void Enqueue(ShipmentItem item)
{
    lock (Gate) { LiveQueue.Add(item); }
}

public static bool TryDequeue(out ShipmentItem? item)
{
    lock (Gate)
    {
        if (LiveQueue.Count == 0) { item = null; return false; }
        item = LiveQueue[0];
        LiveQueue.RemoveAt(0);
        return true;
    }
}
```

**Production takeaway:** `List<T>` documentation explicitly states it is not thread-safe — Karat pairs this with singleton/static patterns to test whether you reach for synchronization or the right concurrent collection. For new code, `Channel<T>` or `ConcurrentQueue<T>` beats hand-rolled locks on `List<T>`.

---

### 04. Dictionary

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/04. Dictionary`

---

---

#### Q1. (R) A hot-path SKU lookup uses `ContainsKey` followed by the indexer. Review this warehouse catalog access. What is inefficient, and how would you improve it?

```csharp
public Product? FindProduct(Dictionary<string, Product> catalog, string sku)
{
    if (catalog.ContainsKey(sku))
    {
        return catalog[sku];
    }
    return null;
}
```

---

**Answer:**

**Answer:** The code performs two hash lookups — one in `ContainsKey` and one in the indexer — when a single `TryGetValue` call can retrieve the value in one pass. Under hot paths or large catalogs, the extra lookup adds avoidable cost and is harder to read than the idiomatic pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Double hash lookup per hit | Unnecessary CPU on high-frequency SKU resolution |
| Idiomatic C# | `ContainsKey` + indexer is a legacy pattern | Easy to miss in code review; signals unfamiliarity with BCL APIs |
| Concurrency | Two separate reads on a shared dictionary (minor) | Theoretically inconsistent if another thread mutates between calls |

**Fix (priority order):**

1. Replace the pair with `TryGetValue` and branch on its `bool` result.
2. Return the `out` variable directly when found; avoid a second access.
3. Prefer `CollectionsMarshal.GetValueRefOrNullRef` only in measured hot paths — `TryGetValue` is the default fix.

```csharp
public Product? FindProduct(Dictionary<string, Product> catalog, string sku)
{
    return catalog.TryGetValue(sku, out Product? product) ? product : null;
}
```

**Production takeaway:** Karat flags this as a micro-optimization with readability upside — it signals you know standard library APIs, not just that dictionaries exist. See **Program.cs** Section 5 — `TryGetValue` preferred over `ContainsKey` + indexer.

---

---

#### Q2. (R) A team uses a custom class as the dictionary key and mutates it after insert. Lookups start failing intermittently in production. Review this catalog code:

```csharp
public sealed class SkuKey
{
    public string Code { get; set; } = string.Empty;

    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public override bool Equals(object? obj) =>
        obj is SkuKey other && Code == other.Code;
}

var catalog = new Dictionary<SkuKey, Product>();
var key = new SkuKey { Code = "WH-1001" };
catalog[key] = new Product { Sku = "WH-1001", Name = "Steel bracket", UnitPrice = 12.50m };

// Later, a pricing job "normalizes" the key object in place:
key.Code = "WH-1001-NORM";

// Another request:
var lookupKey = new SkuKey { Code = "WH-1001" };
catalog.TryGetValue(lookupKey, out Product? found); // found is null — product "vanished"
```

What broke, and how should keys be designed for `Dictionary<TKey, TValue>`?

---

**Answer:**

**Answer:** `Dictionary` stores entries by the key's hash code at insert time. Mutating `key.Code` after insert leaves the entry in the wrong bucket — lookups with a new `SkuKey { Code = "WH-1001" }` hash to a different slot, so the product appears missing even though it is still in the table under a stale hash.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable key changed after `catalog[key] = …` | `TryGetValue` / indexer miss; "orphaned" entries |
| Hash contract | `GetHashCode`/`Equals` must be stable while key is in the map | Violated when `Code` property mutates |
| Design | Reference-type key with public setter | Silent data loss in catalog lookups |

**Fix (priority order):**

1. Make key types **immutable** after construction — `init` or `readonly` properties, or use `record`/`readonly record struct` for value semantics.
2. Never mutate a key object that is already in the dictionary; remove, create a new key, and re-insert if the identifier changes.
3. For string SKUs, prefer `Dictionary<string, Product>` — `string` is immutable and already implements the hash contract correctly.
4. If you must wrap identifiers, use `readonly record struct SkuKey(string Code)` or a sealed class with no setters.

```csharp
public readonly record struct SkuKey(string Code);

var catalog = new Dictionary<SkuKey, Product>();
catalog[new SkuKey("WH-1001")] = product;
// To change SKU: remove old entry, insert with new SkuKey — do not mutate in place
```

**Production takeaway:** Hash-table collections assume keys do not change while inserted — Karat uses this to test the hash contract beyond "override GetHashCode." See **Program.cs** Section 1 — custom keys must override both methods and stay immutable after insert.

---

---

#### Q3. (P) An ASP.NET Core API caches product details in a shared `Dictionary<string, Product>` field on a singleton service. Under load tests, responses are wrong and the process occasionally throws `InvalidOperationException`. Review the cache:

```csharp
public sealed class ProductCatalogService
{
    private readonly Dictionary<string, Product> _cache = new();
    private readonly IProductRepository _repo;

    public Product GetBySku(string sku)
    {
        if (!_cache.ContainsKey(sku))
        {
            Product loaded = _repo.GetBySku(sku); // DB call
            _cache[sku] = loaded;
        }
        return _cache[sku];
    }
}
```

What fails in production under concurrent requests, and what type/pattern replaces this?

---

**Answer:**

**Answer:** `Dictionary<TKey, TValue>` is not thread-safe. Concurrent reads and writes from multiple HTTP requests corrupt internal buckets, throw during enumeration, and allow two threads to both miss the cache and write different `Product` instances for the same SKU — undefined behavior under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Unsynchronized `_cache` mutations | `InvalidOperationException`, torn internal state |
| Correctness | Check-then-add without locking | Duplicate DB loads; possible inconsistent cached values |
| Lifetime | Singleton holds one shared dictionary for all requests | Every request shares the same unsynchronized structure |
| Performance | `ContainsKey` + indexer (two lookups) | Extra cost on every cache access |

**Fix (priority order):**

1. Replace with `ConcurrentDictionary<string, Product>` and use `GetOrAdd` or `TryGetValue` for reads.
2. If you must keep `Dictionary`, guard all access with a single lock (`lock (_cache) { … }`) — simpler but lower throughput than `ConcurrentDictionary`.
3. Register the cache as **scoped** only when it is per-request scratch data — not for a cross-request product catalog; singleton + concurrent collection is the usual pattern for shared read-mostly caches.
4. Consider `IMemoryCache` with size limits and expiration instead of a raw unbounded map (see Q5).

```csharp
private readonly ConcurrentDictionary<string, Product> _cache = new();

public Product GetBySku(string sku) =>
    _cache.GetOrAdd(sku, s => _repo.GetBySku(s));
```

**Production takeaway:** Passing local load tests with one thread hides dictionary thread-safety gaps — Karat expects you to name `ConcurrentDictionary` or explicit locking for shared mutable maps. See foundation **Dictionary** gotcha — not thread-safe for concurrent read/write.

---

---

#### Q4. (R) A REST endpoint maps query parameters directly into dictionary lookups without null checks. Review the handler:

```csharp
[HttpGet("product")]
public IActionResult GetProduct([FromQuery] string? sku, [FromServices] Dictionary<string, Product> catalog)
{
    if (catalog.ContainsKey(sku))
    {
        return Ok(catalog[sku]);
    }
    return NotFound();
}
```

The client calls `/product` with no `sku` parameter. What exception is thrown and where, and how do you harden this lookup?

---

**Answer:**

**Answer:** When `sku` is omitted, it is `null`. `Dictionary<string, T>.ContainsKey(null)` throws `ArgumentNullException` before the `NotFound()` branch runs — the API returns 500 instead of 400/404. The same rule applies to `Add`, the indexer, and `TryGetValue` with a null reference-type key.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Null key passed to `ContainsKey` | `ArgumentNullException` — 500 to client |
| API contract | Missing query param not validated | Wrong status code; noisy error logs |
| Input hygiene | Nullable `string? sku` used as dictionary key without guard | Any null path hits the same exception |

**Fix (priority order):**

1. Validate input first: `if (string.IsNullOrWhiteSpace(sku)) return BadRequest("sku is required");`
2. Use `TryGetValue` for the lookup after validation — one lookup, no exception on missing key.
3. Do not inject `Dictionary<string, Product>` directly into controllers — use a scoped service that owns catalog access and validation.
4. Return `NotFound()` only after a validated, non-null key misses the catalog.

```csharp
[HttpGet("product")]
public IActionResult GetProduct([FromQuery] string? sku, [FromServices] IProductCatalog catalog)
{
    if (string.IsNullOrWhiteSpace(sku))
        return BadRequest("sku is required");

    return catalog.TryGetProduct(sku, out Product? product)
        ? Ok(product)
        : NotFound();
}
```

**Production takeaway:** Null keys are rejected at the API boundary of `Dictionary<string, …>` — Karat tests whether you validate before touching the collection. See **Program.cs** Section 7c — null key on `Add` throws `ArgumentNullException`.

---

---

#### Q5. (D) A microservice adds a static in-memory cache so repeated HTTP fetches are fast. After two weeks in production, pods hit OOM kills even though traffic is steady. Review the cache:

```csharp
public static class RemoteAssetCache
{
    private static readonly Dictionary<string, byte[]> _cache = new();

    public static byte[] GetAsset(string url)
    {
        if (!_cache.TryGetValue(url, out byte[]? bytes))
        {
            bytes = Download(url); // can be megabytes per entry
            _cache[url] = bytes;
        }
        return bytes;
    }

    private static byte[] Download(string url) { /* HttpClient GET */ return Array.Empty<byte>(); }
}
```

What design problem does this introduce at scale, and what would you use instead of an unbounded `Dictionary`?

---

**Answer:**

**Answer:** A plain `Dictionary` with no eviction policy grows without bound — every distinct URL adds a full byte array that is never removed. Static lifetime means the cache survives for the process lifetime and is shared across all requests, so memory only increases as URL diversity grows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Memory | Unbounded key space (`url` strings) + large values (`byte[]`) | OOM kills; GC pressure |
| Lifecycle | `static` cache never cleared | Memory not reclaimed until process restart |
| Operations | No TTL, size cap, or LRU | Cannot reason about worst-case footprint |
| Scale-out | Per-pod static cache | Duplicate memory across replicas; no shared invalidation |

**Fix (priority order):**

1. Replace with `IMemoryCache` (or `MemoryCache`) configured with `SizeLimit`, `CompactionPercentage`, and per-entry `Size` + `AbsoluteExpiration` / `SlidingExpiration`.
2. For distributed deployments, use `IDistributedCache` (Redis) with explicit TTL instead of unbounded in-process storage.
3. If a raw dictionary is unavoidable, implement LRU with a max entry count and max total bytes — evict oldest when limits are hit.
4. Remove `static` — inject a singleton `IMemoryCache` via DI so tests can substitute and options can be configured per environment.

```csharp
// Program.cs — services
builder.Services.AddMemoryCache(o =>
{
    o.SizeLimit = 10_000; // abstract "size units", set per entry
});

// Usage
_cache.GetOrCreate(url, entry =>
{
    entry.Size = 1;
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return Download(url);
});
```

**Production takeaway:** `Dictionary` is a map, not a cache policy — Karat distinguishes "fast lookup" from "safe caching." Unbounded in-memory maps are a common postmortem root cause. See **Program.cs** WHY IT MATTERS — catalogs and caches appear everywhere; size and eviction are production requirements.

---

---

#### Q6. (P) A developer avoids `ConcurrentDictionary` and hand-rolls lazy initialization with `TryGetValue`. Under load, the expensive factory runs twice for the same key. Review:

```csharp
private readonly Dictionary<string, Product> _catalog = new();

public Product GetOrLoad(string sku)
{
    if (!_catalog.TryGetValue(sku, out Product? product))
    {
        product = _repo.LoadProduct(sku); // slow DB + mapping
        _catalog[sku] = product;
    }
    return product;
}
```

What race exists when multiple threads call `GetOrLoad` for the same missing SKU, and how does `ConcurrentDictionary.GetOrAdd` (or alternatives) fix it?

---

### 05. HashSet

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/05. HashSet`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Between `TryGetValue` returning false and `_catalog[sku] = product`, another thread can pass the same check and also call `_repo.LoadProduct(sku)` — classic check-then-act race. Both threads may insert; the last write wins, but you paid for duplicate DB work and may briefly expose inconsistent state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Non-atomic check-then-add on `Dictionary` | Duplicate expensive loads under parallel requests |
| Correctness | Two writers without synchronization | Undefined behavior on `Dictionary` itself (see Q3) |
| Cost | Idempotent DB read assumed | Thundering herd on cold keys at startup or cache flush |

**Fix (priority order):**

1. Use `ConcurrentDictionary.GetOrAdd` so factory execution for a given key is coordinated by the collection.
2. If the factory is very expensive, wrap with `Lazy<Product>` per key or use `GetOrAdd` with a factory that returns `Lazy<Product>` and then `.Value` once — avoids duplicate work when factory cost dominates.
3. For single-threaded or scoped usage, plain `TryGetValue` + assign is fine — the bug is specifically shared mutable state under concurrency.
4. Add metrics on cache misses and factory duration to detect duplicate load spikes in production.

```csharp
private readonly ConcurrentDictionary<string, Product> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => _repo.LoadProduct(s));

// Optional: defer heavy work until first read
private readonly ConcurrentDictionary<string, Lazy<Product>> _catalog = new();

public Product GetOrLoad(string sku) =>
    _catalog.GetOrAdd(sku, s => new Lazy<Product>(() => _repo.LoadProduct(s))).Value;
```

**Production takeaway:** `GetOrAdd` is the production pattern for "compute once per key" in concurrent caches — Karat tests whether you recognize check-then-add as a race, not whether you memorized the method name. Pair with Q3: thread-safe type **and** atomic get-or-create semantics.

---

---

### 05. HashSet

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/05. HashSet`

---

---

#### Q1. (R) A nightly tag-import job deduplicates article tags with `List<string>.Contains` before insert. Review the hot path:

```csharp
public sealed class TagImportService
{
    private readonly List<string> _knownTags = new();

    public bool TryRegisterTag(string tag)
    {
        if (_knownTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            return false;

        _knownTags.Add(tag);
        return true;
    }
}

// Startup loads 80_000 existing tags, then imports 200_000 candidate tags one-by-one.
```

The job passes unit tests (10 tags) but misses its SLA in staging. What is wrong with this design, and what collection change fixes average lookup cost?

---

**Answer:**

**Answer:** `List<T>.Contains` is **O(n)** per call, so importing *m* tags against *n* existing tags approaches **O(n × m)** — fine for unit tests with ten tags, catastrophic at 80k × 200k. Replace the backing store with `HashSet<string>` and the same `StringComparer` so `Add` and `Contains` are **O(1)** average.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Linear scan on every `Contains` | Import SLA missed; CPU spikes on large catalogs |
| Scalability | List grows; each check walks all elements | Cost compounds as `_knownTags` grows through the run |
| Collection choice | List chosen for uniqueness | Wrong tool — HashSet exists for exactly this pattern |

**Fix (priority order):**

1. Use `HashSet<string>` with `StringComparer.OrdinalIgnoreCase` as the backing store.
2. Collapse register to a single `Add` — it returns `false` when the tag is already present.

```csharp
private readonly HashSet<string> _knownTags =
    new(StringComparer.OrdinalIgnoreCase);

public bool TryRegisterTag(string tag) => _knownTags.Add(tag);
```

**Production takeaway:** Karat pairs "works in tests" with hidden **O(n²)** membership — see **Program.cs** Section 9 (HashSet vs List). Always ask lookup frequency and collection size, not just correctness on small data.

---

---

#### Q2. (R) A newsletter service deduplicates subscribers by email but keeps seeing duplicate sends in logs. Review:

```csharp
public sealed class NewsletterService
{
    private readonly HashSet<Subscriber> _subscribers = new();

    public bool AddSubscriber(Subscriber sub) => _subscribers.Add(sub);

    public bool IsSubscribed(Subscriber sub) => _subscribers.Contains(sub);
}

// Two calls with different Subscriber instances, same email:
AddSubscriber(new Subscriber("Alex", "alex@example.com"));   // true
AddSubscriber(new Subscriber("Alex K.", "alex@example.com")); // true — unexpected
```

What breaks uniqueness here, and how do you align with the `SubscriberByEmailComparer` pattern from this chapter?

---

**Answer:**

**Answer:** `HashSet<Subscriber>` without a custom comparer uses **reference equality** for class types — two distinct `Subscriber` objects with the same email are different elements. Pass `SubscriberByEmailComparer` (or override `Equals`/`GetHashCode` on the type) so business identity drives uniqueness.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Default reference equality on reference type | Duplicate emails stored; duplicate emails sent |
| API misuse | `HashSet` assumed to compare by field values | Silent data-quality bug — `Add` returns `true` twice |
| Design | Equality rule not wired into collection | `Contains`/`Remove` also fail to find "same" subscriber |

**Fix (priority order):**

1. Construct with the chapter comparer: `new HashSet<Subscriber>(new SubscriberByEmailComparer())`.
2. Alternatively, use `SubscriberIdentity`-style immutable type with `IEquatable<T>` + consistent `GetHashCode` on `Email`.

```csharp
private readonly HashSet<Subscriber> _subscribers =
    new(new SubscriberByEmailComparer());
```

**Production takeaway:** Custom types in HashSet/Dictionary **never** dedupe by field values unless you supply equality — see **Program.cs** Section 6 vs Section 7.

---

---

#### Q3. (R) After a profile-update feature ships, support reports "user already subscribed" errors even when lookup fails. Review:

```csharp
public class SubscriberProfile
{
    public string Email { get; set; }  // mutable — used in GetHashCode/Equals
    public string Name { get; set; }

    public override bool Equals(object? obj) =>
        obj is SubscriberProfile other &&
        string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Email);
}

var set = new HashSet<SubscriberProfile>();
var user = new SubscriberProfile { Email = "alex@example.com", Name = "Alex" };
set.Add(user);

user.Email = "alex.k@example.com";  // user corrected typo after Add

bool found = set.Contains(user);  // false — user still "in" set but unreachable
```

What went wrong with mutability and the hash contract, and how do you fix the type for set membership?

---

**Answer:**

**Answer:** `GetHashCode` was computed from `Email` at `Add` time and placed the object in a bucket keyed to the old hash. Mutating `Email` afterward leaves the object in the **wrong bucket**, so `Contains` returns `false` even though the instance is still in the set — classic broken hash contract with mutable keys.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Mutable field participates in `GetHashCode` | Lookup/remove fail after in-place edit |
| Hash contract | Hash at insert ≠ hash at lookup | Element orphaned inside set — `Count` includes it but `Contains` misses |
| Design | Writable `Email` on set member type | Same rule as Dictionary keys — must be immutable for hashed collections |

**Fix (priority order):**

1. Make identity fields immutable (`init` or constructor-only), matching `SubscriberIdentity` in **Program.cs** Section 7.
2. If email must change, **remove** old identity from the set and **add** a new object (or rebuild the set).
3. Never mutate fields that feed `Equals`/`GetHashCode` while the instance lives inside a `HashSet` or `Dictionary`.

```csharp
public sealed class SubscriberProfile
{
    public string Email { get; }
    public string Name { get; set; }

    public SubscriberProfile(string email, string name)
    {
        Email = email;
        Name = name;
    }
    // Equals/GetHashCode on Email only
}
```

**Production takeaway:** Karat tests whether you treat HashSet elements like **Dictionary keys** — mutable hash inputs cause silent lookup failures, not exceptions.

---

---

#### Q4. (R) An editorial dashboard merges article tag sets for a "shared topics" widget. Case variants appear twice after deploy. Review:

```csharp
var dotnetTags = new HashSet<string>(_dotnetArticle.Tags, StringComparer.OrdinalIgnoreCase);
var linqTags = new HashSet<string>(_linqArticle.Tags, StringComparer.OrdinalIgnoreCase);

// Developer assumes LINQ Union inherits the HashSet comparer:
IEnumerable<string> allTopics = dotnetTags.Union(linqTags);

var widgetTags = new HashSet<string>(allTopics);  // default Ordinal comparer
Console.WriteLine(widgetTags.Count);              // "csharp" and "CSharp" both present
```

What comparer mismatch caused duplicate logical tags, and how do you build the union with consistent equality end-to-end?

---

**Answer:**

**Answer:** `HashSet<T>.Union` as a LINQ extension on `IEnumerable<T>` uses **default sequence equality** (`EqualityComparer<string>.Default` → **Ordinal**, case-sensitive), **not** the HashSet's internal `StringComparer.OrdinalIgnoreCase`. Re-wrapping in `new HashSet<string>(allTopics)` without a comparer keeps Ordinal semantics, so `"csharp"` and `"CSharp"` coexist as distinct entries.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | LINQ set ops ignore HashSet's comparer | Logical duplicates in UI and analytics |
| API confusion | `Union` on HashSet still calls `Enumerable.Union` | Developer's comparer choice on construction does not flow to LINQ |
| Data quality | Default `HashSet` ctor uses Ordinal | Case variants inflate counts and break deduped filters |

**Fix (priority order):**

1. Pass the same comparer when materializing: `new HashSet<string>(dotnetTags.Union(linqTags), StringComparer.OrdinalIgnoreCase)`.
2. Or use mutating `UnionWith` on a **copy** if you need set instance semantics with the existing comparer.
3. For intersection-only widgets, same rule: `new HashSet<string>(a.Intersect(b), comparer)`.

```csharp
var widgetTags = new HashSet<string>(
    dotnetTags.Union(linqTags),
    StringComparer.OrdinalIgnoreCase);
```

**Production takeaway:** LINQ `Union`/`Intersect`/`Except` are **comparer-agnostic** — see **Program.cs** Section 3. Always thread `IEqualityComparer<T>` through the final `HashSet` constructor.

---

---

#### Q5. (R) A publish pipeline accidentally wipes an editor's working tag pool. Review the merge step:

```csharp
HashSet<string> editorPool = new(StringComparer.OrdinalIgnoreCase)
{
    "csharp", "dotnet", "security"
};

HashSet<string> draftTags = new(StringComparer.OrdinalIgnoreCase)
{
    "dotnet", "api", "draft"
};

// Intent: preview tags common to BOTH pools without changing editorPool
editorPool.IntersectWith(draftTags);

Console.WriteLine(string.Join(", ", editorPool)); // only "dotnet" — pool mutated
// Later: editorPool.UnionWith(blockedList) no longer restores "csharp", "security"
```

The developer meant a non-mutating preview. What API mistake was made, and show the safe pattern that leaves `editorPool` unchanged?

---

**Answer:**

**Answer:** `IntersectWith` **mutates the caller** (`editorPool`) in place, keeping only elements also in `draftTags`. The developer needed a **non-mutating** preview — the chapter's LINQ `Intersect` (Section 3) or a copy-then-`IntersectWith` pattern (Section 4).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `IntersectWith` vs intended read-only preview | `"csharp"` and `"security"` permanently removed from working pool |
| API misuse | Confused mutating (`*With`) vs LINQ extension methods | Downstream `UnionWith` cannot restore deleted tags |
| Operational | Shared `editorPool` referenced elsewhere | Other features see truncated set — data loss in session state |

**Fix (priority order):**

1. Non-mutating LINQ: `var preview = new HashSet<string>(editorPool.Intersect(draftTags), StringComparer.OrdinalIgnoreCase);`
2. Or copy first: `var preview = new HashSet<string>(editorPool, comparer); preview.IntersectWith(draftTags);`
3. Reserve `IntersectWith` for intentional in-place filtering when building a working set incrementally.

```csharp
var preview = new HashSet<string>(
    editorPool.Intersect(draftTags),
    StringComparer.OrdinalIgnoreCase);
// editorPool unchanged: csharp, dotnet, security
```

**Production takeaway:** `*With` methods return `void` and modify **this** — Karat loves swapping them with LINQ equivalents. Read method names literally before calling on shared state.

---

---

#### Q6. (R) A custom comparer passes code review but `Remove` and `Contains` behave inconsistently. Review:

```csharp
public sealed class SubscriberByNameComparer : IEqualityComparer<Subscriber>
{
    public bool Equals(Subscriber? x, Subscriber? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(x.Email, y.Email, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Subscriber obj) =>
        obj.Name.GetHashCode(StringComparison.Ordinal);  // hashes Name, not Email
}

var set = new HashSet<Subscriber>(new SubscriberByNameComparer());
var a = new Subscriber("Alex", "alex@example.com");
set.Add(a);
set.Contains(new Subscriber("Alex K.", "alex@example.com")); // sometimes false
set.Remove(new Subscriber("Alex K.", "alex@example.com"));    // sometimes false while Add returned false on duplicate
```

What contract violation breaks `HashSet<T>`, and what is the corrected comparer implementation?

---

### 06. Queue and Stack

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/06. Queue and Stack`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `Equals` compares **Email** but `GetHashCode` hashes **Name** — violating the rule that equal objects must share the same hash code. The second subscriber lands in a different bucket, so `Contains`/`Remove` miss while duplicate `Add` behavior looks arbitrary.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `GetHashCode`/`Equals` inconsistency | Silent failures — worst kind of collection bug |
| Hash contract | Equal-by-email objects can differ by hash | `Remove` returns false for objects that "should" match |
| Code review | Comparer named `ByName` but equals on Email | Copy-paste defect easy to miss without contract tests |

**Fix (priority order):**

1. Derive hash from the **same fields** used in `Equals` — here, email case-insensitively.
2. Add unit tests: if `Equals(a,b)` then `GetHashCode(a) == GetHashCode(b)`; round-trip `Add`/`Contains`/`Remove`.

```csharp
public int GetHashCode(Subscriber obj) =>
    StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Email);
```

**Production takeaway:** HashSet and Dictionary failures from bad comparers **do not throw** — they return wrong `bool` results. Same contract as **Program.cs** Section 6 quick reference: *Equal objects → same hash code*.

---

### 06. Queue and Stack

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/06. Queue and Stack`

---

---

#### Q1. (R) A help-desk service was refactored from `Queue<SupportTicket>` to `Stack<SupportTicket>` "because stacks are faster." Review the handler loop. What ordering bug appears in production, and how do you fix it?

```csharp
public sealed class TicketProcessor
{
    private readonly Stack<SupportTicket> _pending = new Stack<SupportTicket>();

    public void EnqueueTicket(SupportTicket ticket) => _pending.Push(ticket);

    public void ProcessAll()
    {
        while (_pending.Count > 0)
        {
            SupportTicket next = _pending.Pop();
            Resolve(next);
        }
    }

    private void Resolve(SupportTicket ticket) { /* SLA tracking */ }
}

// Arrival order: #1001 (9:00), #1002 (9:05), #1003 (9:10)
// ProcessAll resolves: #1003, #1002, #1001
```

---

**Answer:**

**Answer:** `Stack<T>` is LIFO — the last ticket pushed is the first popped — so SLA fairness is inverted: newest tickets are resolved before older ones waiting longer. Ticket queues require FIFO semantics, which `Queue<T>` enforces at the type level.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `Stack` + `Push`/`Pop` for arrival-order work | Newest-first processing — SLA breaches on oldest tickets |
| Naming/API | Method still named `EnqueueTicket` but calls `Push` | Misleading API; code review misses semantic mismatch |
| Design | Chose collection for perceived speed, not ordering rule | Wrong abstraction — `List<T>` with `Insert(0,…)` would be equally wrong |

**Fix (priority order):**

1. Restore `Queue<SupportTicket>` with `Enqueue` / `TryDequeue` (or `Dequeue` when empty is impossible by contract).
2. Rename methods to match semantics: `EnqueueTicket` + `TryResolveNextTicket` as in **Program.cs** Section 4.
3. If priority tiers are needed later, use `PriorityQueue<TElement, TPriority>` — not `Stack<T>`.
4. Document ordering invariant in tests: enqueue A, B, C → resolve A, B, C.

```csharp
private readonly Queue<SupportTicket> _pending = new();

public void EnqueueTicket(SupportTicket ticket) => _pending.Enqueue(ticket);

public bool TryResolveNext(out SupportTicket ticket) => _pending.TryDequeue(out ticket);
```

**Production takeaway:** Karat tests whether you match collection type to business ordering — FIFO for fair queues, LIFO for undo/call-stack models. See **Program.cs** Section 1 — FIFO vs LIFO table.

---

---

#### Q2. (R) A background worker drains a print queue when the upstream publisher is idle. Under load, the service logs unhandled `InvalidOperationException` and the host restarts. Review the consumer:

```csharp
public sealed class PrintWorker
{
    private readonly Queue<PrintJob> _jobs = new Queue<PrintJob>();

    public void Submit(PrintJob job) => _jobs.Enqueue(job);

    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            PrintJob job = _jobs.Dequeue();  // throws when queue empty
            Print(job);
        }
    }
}
```

What breaks, and how would you harden this for production idle periods?

---

**Answer:**

```csharp
public sealed class PrintWorker
{
    private readonly Queue<PrintJob> _jobs = new Queue<PrintJob>();

    public void Submit(PrintJob job) => _jobs.Enqueue(job);

    public void Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            PrintJob job = _jobs.Dequeue();  // throws when queue empty
            Print(job);
        }
    }
}
```

What breaks, and how would you harden this for production idle periods?

**Answer:** `Dequeue()` throws `InvalidOperationException` when the queue is empty — the tight loop calls it continuously during idle periods, crashing the worker instead of waiting for the next job.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Dequeue()` on empty queue | Unhandled exception → host restart / lost in-flight work |
| Control flow | Busy loop with no back-off when empty | 100% CPU spin if switched to `Count` check without delay |
| Concurrency | Plain `Queue<T>` if multiple producers (minor here) | Not thread-safe — separate from empty-queue bug but common in same services |

**Fix (priority order):**

1. Replace `Dequeue()` with `TryDequeue(out PrintJob? job)` — process only when `true`.
2. When empty, await a signal (`Channel<PrintJob>`, `BlockingCollection<T>`, or `ManualResetEventSlim` + lock) instead of spinning.
3. Optionally combine with `await Task.Delay(pollInterval, ct)` only if a simple poll model is acceptable — prefer event-driven dequeue.
4. For multi-producer scenarios, use `ConcurrentQueue<T>` or a `Channel<T>` writer/reader pair.

```csharp
public async Task RunAsync(CancellationToken ct)
{
    while (!ct.IsCancellationRequested)
    {
        if (_jobs.TryDequeue(out PrintJob? job))
        {
            Print(job);
            continue;
        }

        await Task.Delay(100, ct); // or await _signal.WaitAsync(ct);
    }
}
```

**Production takeaway:** Empty is an expected state for workers — `TryDequeue`/`TryPop` exist precisely to avoid exception-driven control flow. See **Program.cs** Section 2a — empty queue behavior.

---

---

#### Q3. (P) Three ASP.NET Core request threads enqueue audit events; one background `IHostedService` dequeues them for batch upload. The team shares one `Queue<AuditEvent>` instance registered as a **Singleton**. Occasionally events disappear or `InvalidOperationException` appears under concurrent `Enqueue`/`Dequeue`. Explain why `Queue<T>` is unsafe here and what you would register instead.

---

**Answer:**

**Answer:** `Queue<T>` is not thread-safe — concurrent `Enqueue` and `Dequeue` from multiple threads corrupt internal state without external locking, causing lost items or exceptions. A singleton shared across request threads requires a concurrent collection or a `Channel<T>`.

- **`ConcurrentQueue<T>`:** Lock-free FIFO safe for multiple producers and consumers; `TryDequeue` for the background drainer. Good when you only need in-memory fan-in.
- **`Channel<T>` (System.Threading.Channels):** Preferred in modern ASP.NET Core — bounded capacity for back-pressure, async `Reader.ReadAllAsync`, clean producer/consumer split in DI.
- **`BlockingCollection<T>`:** Legacy pattern wrapping a concurrent queue with blocking take — workable but heavier than channels for new code.
- **Do not** wrap `Queue<T>` in a singleton and synchronize ad hoc on every call without reviewing lock ordering — easy to deadlock with `Dequeue` inside `lock` while producers hold the same lock incorrectly.

```csharp
// Registration sketch
builder.Services.AddSingleton(Channel.CreateBounded<AuditEvent>(
    new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.Wait }));
builder.Services.AddHostedService<AuditBatchUploader>();
```

**Production takeaway:** FIFO ordering does not imply thread safety — choose `ConcurrentQueue<T>` or `Channel<T>` when a queue crosses thread boundaries. See **Program.cs** Section 2 — `Queue<T>` API assumes single-threaded mutation unless externally synchronized.

---

---

#### Q4. (M) A developer rewrites maze pathfinding from the chapter's BFS to recursive DFS. On large grids the process terminates with `StackOverflowException`. They propose "just use `Stack<T>` instead of recursion." Review both approaches:

```csharp
// Original (chapter-style BFS) — works on large maze
Queue<(int Row, int Col)> frontier = new();
frontier.Enqueue(start);
while (frontier.TryDequeue(out var current)) { /* expand neighbors */ }

// Rewrite — deep recursion on 2000×2000 grid
void Dfs(int row, int col)
{
    if (visited[row, col]) return;
    visited[row, col] = true;
    foreach (var neighbor in GetNeighbors(row, col))
        Dfs(neighbor.Row, neighbor.Col);  // one frame per depth level
}
```

What actually causes the overflow, and when does an explicit `Stack<T>` fix it vs when recursion is acceptable?

---

**Answer:**

```csharp
// Original (chapter-style BFS) — works on large maze
Queue<(int Row, int Col)> frontier = new();
frontier.Enqueue(start);
while (frontier.TryDequeue(out var current)) { /* expand neighbors */ }

// Rewrite — deep recursion on 2000×2000 grid
void Dfs(int row, int col)
{
    if (visited[row, col]) return;
    visited[row, col] = true;
    foreach (var neighbor in GetNeighbors(row, col))
        Dfs(neighbor.Row, neighbor.Col);  // one frame per depth level
}
```

What actually causes the overflow, and when does an explicit `Stack<T>` fix it vs when recursion is acceptable?

**Answer:** `StackOverflowException` comes from the **CLR call stack** — each recursive `Dfs` call consumes a stack frame (~1 MB default thread stack limit), not from `Stack<T>` heap storage. Replacing recursion with an explicit `Stack<(int,int)>` loop uses the heap for frontier cells, avoiding deep call stacks on large grids.

- **Cause:** Depth-first recursion on a path thousands of cells long nests that many frames; the OS/thread stack overflows before the algorithm finishes.
- **Explicit `Stack<T>` fix:** Push start cell; `while (stack.TryPop(out current))` expand neighbors and push unvisited — same LIFO DFS order, bounded by heap memory instead of call-stack depth.
- **When recursion is fine:** Shallow trees (expression AST depth &lt; ~100), divide-and-conquer with logarithmic depth, or problems with guaranteed small branching depth.
- **BFS vs DFS choice (related):** Chapter BFS with `Queue<T>` finds shortest paths in unweighted grids; DFS (recursive or `Stack<T>`) does not guarantee shortest path but uses less memory for some sparse graphs.
- **Not a fix:** Switching BFS to `Stack<T>` without changing algorithm — that yields DFS traversal order and breaks shortest-path guarantees from **Program.cs** Section 5.

**Production takeaway:** Karat distinguishes the **call stack** (recursion limit) from **`Stack<T>`** (heap collection) — iterative DFS with `Stack<T>` is the standard production pattern for deep graph search.

---

---

#### Q5. (D) Your team must pick a frontier collection for two graph tasks on an unweighted social network: (A) find **shortest path** in friend hops from user A to user B, and (B) detect whether a **cycle** exists in a follow graph (direction matters). One engineer says "both are graph search — use `Stack<T>` for both." What would you choose for each task and why?

---

**Answer:**

**Answer:** Shortest hop count in an unweighted graph requires BFS with `Queue<T>` so nodes are discovered in non-decreasing distance from the start; cycle detection in a directed graph is typically DFS with `Stack<T>` (or recursion) and a recursion/recursion-stack coloring strategy — not the same frontier choice.

**Task A — shortest friend hops (unweighted):**

- Use **`Queue<UserId>`** BFS — first time you dequeue B, you have minimum hop count.
- `Stack<T>` DFS may find *a* path quickly but not the shortest — wrong for "degrees of separation" product features.

**Task B — cycle in directed follow graph:**

- Use **DFS** with **`Stack<UserId>`** (explicit or recursion) plus `visited` / `onStack` (three-color) state to detect back edges.
- BFS with `Queue<T>` finds cycles in undirected graphs with parent tracking but directed cycle detection is awkward with BFS alone.

| Task | Collection | Why |
|---|---|---|
| Shortest hops (unweighted) | `Queue<T>` — BFS | Layer-by-layer discovery = minimum edges |
| Directed cycle detection | `Stack<T>` — DFS | Back edge to active stack frame signals cycle |

- **Production note:** At web scale, graph logic moves to a graph DB or precomputed index — but the collection choice still signals correct algorithmic reasoning in code reviews and Karat screens.

**Production takeaway:** Match FIFO vs LIFO to the **invariant** you need (shortest layer vs deep path/back-edge detection), not to "both are graphs." See **Program.cs** Quick Reference — BFS → `Queue<T>`, DFS → `Stack<T>` or recursion.

---

---

#### Q6. (R) A response editor copied from the chapter's `HelpDeskSession` mixes undo (`Stack<string>`) with ticket draining. Review this merge:

```csharp
public sealed class AgentSession
{
    private readonly Queue<SupportTicket> _tickets = new();
    private readonly Stack<string> _undo = new();
    private readonly StringBuilder _draft = new();

    public void BeginResponse(SupportTicket ticket)
    {
        _draft.Clear();
        _undo.Clear();                    // clears undo history
        _tickets.Enqueue(ticket);         // re-queues active ticket to tail
    }

    public void ApplyEdit(Action<StringBuilder> edit)
    {
        _undo.Push(_draft.ToString());
        edit(_draft);
    }

    public SupportTicket? TakeNextTicket()
    {
        return _tickets.Count > 0 ? _tickets.Dequeue() : null;
    }
}
```

The agent reports tickets jumping to the back of the line and undo lost mid-edit. What went wrong with collection choice and API usage?

---

### 07. SortedList & SortedDictionary

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/07. SortedList & SortedDictionary`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public sealed class AgentSession
{
    private readonly Queue<SupportTicket> _tickets = new();
    private readonly Stack<string> _undo = new();
    private readonly StringBuilder _draft = new();

    public void BeginResponse(SupportTicket ticket)
    {
        _draft.Clear();
        _undo.Clear();                    // clears undo history
        _tickets.Enqueue(ticket);         // re-queues active ticket to tail
    }

    public void ApplyEdit(Action<StringBuilder> edit)
    {
        _undo.Push(_draft.ToString());
        edit(_draft);
    }

    public SupportTicket? TakeNextTicket()
    {
        return _tickets.Count > 0 ? _tickets.Dequeue() : null;
    }
}
```

The agent reports tickets jumping to the back of the line and undo lost mid-edit. What went wrong with collection choice and API usage?

**Answer:** `BeginResponse` misuses both collections — it re-`Enqueue`s the ticket already being worked (sending it to the tail instead of keeping it as the active item) and clears the undo stack even when only the draft should reset. Tickets and undo stacks serve different lifecycles and must not be conflated in one "begin" method.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Queue misuse | `Enqueue(ticket)` on ticket already removed for editing | Ticket moves to back — others processed first |
| Stack misuse | `_undo.Clear()` on every begin | Undo history wiped — agent cannot revert prior edits |
| API design | `BeginResponse` accepts ticket param implying re-queue | Confuses "start draft text" with "return ticket to queue" |
| Empty handling | `TakeNextTicket` uses ternary + `Dequeue` | Acceptable here, but inconsistent with chapter's `TryDequeue` pattern |

**Fix (priority order):**

1. Split responsibilities: `TryResolveNextTicket` dequeues once; `BeginResponse(string openingLine)` only clears draft + undo — **no** queue mutation (mirror **Program.cs** Section 4 `HelpDeskSession`).
2. Do not pass the active ticket back into the queue until the response is sent or explicitly re-queued.
3. Clear `_undo` only when starting a **new** response for a **new** ticket, not on every keystroke batch.
4. Prefer `TryDequeue` over `Count` + `Dequeue` to avoid races if the session becomes multi-threaded.

```csharp
public void BeginResponse(string openingLine)
{
    _undo.Clear();
    _draft.Clear();
    _draft.Append(openingLine);
}

public bool TryResolveNextTicket(out SupportTicket ticket)
    => _tickets.TryDequeue(out ticket);
```

**Production takeaway:** Queue and Stack often appear together in one workflow (tickets FIFO + undo LIFO) — Karat tests that you keep each collection's contract isolated. See **Program.cs** Section 4 — help-desk scenario wiring.

---

### 07. SortedList & SortedDictionary

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/07. SortedList & SortedDictionary`

---

---

#### Q1. (R) A warehouse dashboard prints the lowest and highest SKU from a live price map. Review:

```csharp
SortedDictionary<string, decimal> skuPrices = LoadAllSkuPrices(); // ~12_000 entries

string lowestSku = skuPrices.Keys[0];
string highestSku = skuPrices.Keys[skuPrices.Count - 1];

Console.WriteLine($"Range: {lowestSku} … {highestSku}");
```

Build fails. A teammate suggests switching to `Dictionary<string, decimal>` and sorting keys with LINQ on every page load. What is wrong with the original code, and what is the better fix that keeps sorted key order?

---

**Answer:**

**Answer:** `SortedDictionary<TKey,TValue>.Keys` is a read-only `ICollection<TKey>` with **no indexer** — only `SortedList` exposes `Keys[i]` and `Values[i]` for O(1) access by sorted rank. Keep `SortedDictionary` and walk `Keys` once (or track min/max while loading) rather than dropping to `Dictionary` and resorting on every request.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `Keys[0]` on `SortedDictionary` | CS0021 — indexer not defined on Keys view |
| API confusion | Assumed both sorted types support rank indexing | Wrong type chosen for dashboard endpoint |
| Over-correction | Sort-on-read with `Dictionary` + LINQ | O(n log n) per page load when O(n) scan or `SortedList` index suffices |

**Fix (priority order):**

1. If random access by sorted rank is required (`Keys[0]`, `Values[i]`), use `SortedList<string, decimal>` — see **Program.cs** Section 3.
2. If the map stays a `SortedDictionary`, iterate `Keys` once to capture first and last (Section 6 pattern) — O(n) but simple for 12k keys on a dashboard.
3. Do **not** switch to `Dictionary` solely to fix the compile error when sorted iteration is a product requirement.

```csharp
string? lowest = null, highest = null;
foreach (string sku in skuPrices.Keys)
{
    lowest ??= sku;
    highest = sku;
}
```

**Production takeaway:** Karat tests whether you know **index by rank** is `SortedList`-only — `SortedDictionary` gives sorted foreach and O(log n) key lookup, not `Keys[i]`.

---

---

#### Q2. (R) An inventory sync service upserts pallet counts every few seconds. Review the hot path:

```csharp
public sealed class PalletSyncService
{
    private readonly SortedList<int, int> _countsByZone = new(capacity: 500);

    public void UpsertZoneCount(int zoneId, int palletCount)
    {
        if (_countsByZone.ContainsKey(zoneId))
            _countsByZone[zoneId] = palletCount;
        else
            _countsByZone.Add(zoneId, palletCount);
    }
}

// Startup loads 500 zones; sync runs 40 upserts/sec with mixed new and existing zone IDs.
```

Latency spikes after deploy though lookups are fast. What collection cost dominates here, and what type swap fixes churn without losing sorted iteration?

---

**Answer:**

**Answer:** `SortedList` insert and remove shift parallel key/value arrays — **O(n)** per upsert when the collection is full-sized. Frequent mixed inserts/updates on ~500 zones make array shifting dominate even though `TryGetValue` remains O(log n). Swap to `SortedDictionary<int, int>` for tree-backed O(log n) insert/remove while preserving sorted foreach.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | `SortedList.Add` / shift on new keys | Latency grows with zone count under steady churn |
| Idiom | `ContainsKey` + indexer instead of one path | Extra O(log n) lookup; minor vs shifting cost |
| Collection choice | `SortedList` for write-heavy sync | Wrong tool when entries churn — Section 8 guidance |

**Fix (priority order):**

1. Replace backing field with `SortedDictionary<int, int>`.
2. Collapse upsert to indexer assignment — adds or updates in one call: `_countsByZone[zoneId] = palletCount`.
3. Reserve `SortedList` for small, mostly static maps (config tables, reorder reports in **Program.cs** Section 11).

```csharp
private readonly SortedDictionary<int, int> _countsByZone = new();

public void UpsertZoneCount(int zoneId, int palletCount) =>
    _countsByZone[zoneId] = palletCount;
```

**Production takeaway:** "Lookups are fast" is a Karat trap — **insert/remove cost** separates `SortedList` from `SortedDictionary`. See **Program.cs** Section 9 — insert O(n) vs O(log n).

---

---

#### Q3. (D) You expose a `/regions/sales` JSON endpoint. Product wants keys returned alphabetically by region code. Two proposals:

**A.** `Dictionary<string, int>` — load from SQL, then `OrderBy(k => k.Key).ToDictionary()` before serialize.

**B.** `SortedDictionary<string, int>` — insert as rows stream in; foreach already ascending.

The map holds ~30 regions, refreshes once per hour, and serves ~2k requests/min with read-heavy traffic. Which backing store do you pick, and what breaks if you choose wrong?

---

**Answer:**

**Answer:** For ~30 regions, hourly refresh, and read-heavy traffic, **`SortedDictionary<string, int>` (B)** is the better default: sorted order is built into the structure, foreach needs no extra sort, and n is tiny so O(log n) lookup cost is irrelevant. **`Dictionary` + `OrderBy` (A)** adds allocation and O(n log n) work on every response unless you cache the sorted projection — acceptable only if you already hold a `Dictionary` for O(1) hot lookups elsewhere and sort rarely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance (A) | Sort 30 keys per request × 2k rpm | Avoidable CPU and GC from repeated `OrderBy` |
| Correctness (B wrong use) | `SortedDictionary` for millions of writes | Tree rebalancing still O(log n) — fine here, bad at huge churn |
| Design | Picking `Dictionary` "because it's faster" | Ignores that 30-key sort-on-read duplicates work the BCL already provides |

**Decision:**

| Factor | Prefer |
|---|---|
| Small n, sorted output every read | `SortedDictionary` |
| Huge n, order irrelevant, rare sorted export | `Dictionary` + sort once when exporting |
| Need `Keys[i]` by rank | `SortedList` (even smaller static maps) |
| Fastest lookup, no order | Plain `Dictionary` — Section 1 |

**Production takeaway:** Karat expects **size and access pattern** reasoning — 30 hourly regions is the sweet spot for sorted maps; see **Program.cs** Section 8 pick-list.

---

---

#### Q4. (R) A catalog search feature stores product tags in a case-insensitive sorted map. QA reports duplicate logical tags after a Turkish-locale server deploy. Review:

```csharp
var tagsByCount = new SortedDictionary<string, int>(
    StringComparer.CurrentCulture)
{
    ["dotnet"] = 12,
    ["CSharp"] = 8,
    ["LINQ"] = 5
};

tagsByCount["csharp"] = 99; // developer expects this to update "CSharp"

foreach (var tag in tagsByCount)
    Console.WriteLine($"{tag.Key} → {tag.Value}");
```

What comparer behavior caused the surprise, and what comparer + API pattern matches the chapter's `StringComparer.OrdinalIgnoreCase` demo?

---

**Answer:**

**Answer:** `StringComparer.CurrentCulture` uses locale-sensitive rules — casing and ordering can differ by server culture (Turkish **I/i** is the classic trap). For stable product-tag identity, use **`StringComparer.OrdinalIgnoreCase`** (chapter Section 7) so `"CSharp"` and `"csharp"` are the same key and indexer assignment updates rather than duplicates.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Culture-sensitive comparer on logical keys | Duplicate or missing keys when culture changes across environments |
| Environment | Turkish locale vs en-US dev box | QA-only failures after regional deploy |
| API | Indexer update assumes comparer treats keys as equal | Two entries when comparer says keys differ |

**Fix (priority order):**

1. Construct with `StringComparer.OrdinalIgnoreCase` — matches **Program.cs** `DemoCustomComparer`.
2. Use `Add` only when you want duplicate detection to throw; use indexer when upserting counts.
3. Reserve `CurrentCulture` for **display sort** (UI lists), not for canonical tag keys in services.

```csharp
var tagsByCount = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["dotnet"] = 12,
    ["CSharp"] = 8,
    ["LINQ"] = 5
};

tagsByCount["csharp"] = 99; // updates single "CSharp"/"csharp" entry
```

**Production takeaway:** Sorted collections sort by **`IComparer<TKey>`**, not culture by default — explicit `OrdinalIgnoreCase` avoids locale drift between dev and production. See foundation **Strings** — culture vs ordinal.

---

---

#### Q5. (M) A pricing microservice benchmarks three shapes for a nightly job that inserts 50_000 random SKUs once, then performs 500_000 lookups:

```csharp
var hash = new Dictionary<int, decimal>(50_000);
var sortedList = new SortedList<int, decimal>(50_000);
var sortedDict = new SortedDictionary<int, decimal>();

for (int i = 0; i < 50_000; i++)
{
    int key = Random.Shared.Next(100_000);
    hash.TryAdd(key, 1.99m);
    if (!sortedList.ContainsKey(key)) sortedList.Add(key, 1.99m);
    if (!sortedDict.ContainsKey(key)) sortedDict.Add(key, 1.99m);
}

// Then 500_000 TryGetValue / Contains loops on each collection...
```

`SortedList` dominates wall-clock time during the load phase. Explain **why** insert is asymptotically worse than the other two, and state the rule of thumb from this chapter for pick-list vs tree-backed sorted maps.

---

**Answer:**

**Answer:** `SortedList` backs keys and values with **parallel arrays**. Each insert finds the slot with binary search then **shifts** remaining elements — **O(n)** per insert, so 50_000 random inserts approach **O(n²)** total work. `Dictionary` averages **O(1)** insert; `SortedDictionary` uses a red-black tree at **O(log n)** per insert with no shifting — which is why `SortedList` dominates the load phase despite similar O(log n) lookup afterward.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Array shift on every `SortedList.Add` | Load phase orders of magnitude slower at 50k entries |
| Benchmark misread | Blaming "sorted" generically | Wrong fix — might avoid all sorted types instead of swapping list vs tree |
| Pattern | `ContainsKey` guard before every add | Extra lookup; still dwarfed by shift cost on `SortedList` |

**Rule of thumb (Section 9):**

| Need | Pick |
|---|---|
| Fastest lookup, order irrelevant | `Dictionary` |
| Sorted keys + access by rank (`Keys[i]`) | `SortedList` — small / mostly static |
| Sorted keys + frequent inserts/removes | `SortedDictionary` |
| Membership only, no values | `HashSet` |

**Production takeaway:** Karat pairs benchmark numbers with **mechanism** — array shift vs tree rebalance — not just "sorted is slower." Nightly bulk load + heavy lookup → `Dictionary` or `SortedDictionary`; `SortedList` only if you need index-by-rank on a small final map.

---

---

#### Q6. (R) A developer ports a `Dictionary` helper to sorted collections but copies the wrong comparer interface. Review:

```csharp
public sealed class SkuIgnoreCaseEquality : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y) =>
        string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode(string obj) =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
}

var reorderQty = new SortedDictionary<string, int>(new SkuIgnoreCaseEquality())
{
    ["ZEBRA-CLIP"] = 40,
    ["ALPHA-PAD"] = 120
};

reorderQty["alpha-pad"] = 200;
```

What fails at compile time, what would fail at runtime if they forced it to compile, and how do you wire case-insensitive **sort order** correctly for `SortedList` / `SortedDictionary`?

---

### 08. IEnumerable & IEnumerator

# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/08. IEnumerable & IEnumerator`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** `SortedDictionary` and `SortedList` constructors take **`IComparer<TKey>`**, not **`IEqualityComparer<TKey>`**. The snippet fails at compile time (`SkuIgnoreCaseEquality` does not implement `IComparer<string>`). If coerced, the collection would still sort/compare by the wrong contract — hash-based equality does not define sort order. Pass `StringComparer.OrdinalIgnoreCase` (implements `IComparer<string>`) or a custom `IComparer<string>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `IEqualityComparer` passed to sorted ctor | CS1503 — type mismatch |
| Conceptual | Confused hash equality with sort order | Dictionary/HashSet vs sorted maps — Section 7 comment |
| Runtime (if bypassed) | Wrong or default ordering | Duplicate logical keys or unexpected sort sequence |

**Fix (priority order):**

1. Use built-in comparer: `new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase)`.
2. For custom rules, implement **`IComparer<TKey>`** (like `ScoreDescendingComparer` in **Program.cs**), not `IEqualityComparer`.
3. Keep `SkuIgnoreCaseEquality` for `Dictionary<string, T>` / `HashSet<string>` only.

```csharp
var reorderQty = new SortedDictionary<string, int>(
    StringComparer.OrdinalIgnoreCase)
{
    ["ZEBRA-CLIP"] = 40,
    ["ALPHA-PAD"] = 120
};

reorderQty["alpha-pad"] = 200; // updates existing key
```

**Production takeaway:** Karat stacks **interface confusion** with collection choice — `IEqualityComparer` for hash tables, `IComparer` for sorted types. See **Program.cs** QUICK REFERENCE — "Not IEqualityComparer."

---

### 08. IEnumerable & IEnumerator

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/03. Generics & Collections/08. IEnumerable & IEnumerator`

---

---

#### Q1. (R) A warehouse API returns `IEnumerable<PickLine>` from a `yield return` filter. A report job calls `Count()` then `Sum()` on the same reference without materializing. Totals disagree with the pick ticket and logs show the database query ran twice. Review the service method and caller. What went wrong, and how do you fix it?

```csharp
public IEnumerable<PickLine> GetHeavyLines(string ticketId, decimal minKg)
{
    foreach (PickLine line in _repository.LoadLines(ticketId)) // hits DB per enumeration
    {
        if (line.TotalWeightKg >= minKg)
            yield return line;
    }
}

// ReportJob:
var heavy = _service.GetHeavyLines("PB-2201", 1.0m);
int lineCount = heavy.Count();
decimal totalKg = heavy.Sum(l => l.TotalWeightKg);
_logger.LogInformation("Heavy lines: {Count}, total kg: {Total}", lineCount, totalKg);
```

---

**Answer:**

**Answer:** `IEnumerable<PickLine>` from a `yield return` method is lazy — each consumer (`Count`, then `Sum`) walks the sequence from scratch, re-running `_repository.LoadLines` and the filter. The two passes are independent enumerations, so side effects, timing, and even data can differ if the ticket changed between calls.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Multiple enumeration | `Count()` then `Sum()` on same lazy sequence | DB/repository work runs twice; metrics and billing double-charge I/O |
| Correctness | No snapshot between passes | If lines change mid-report, count and sum can reflect different underlying data |
| API contract | Returning bare `IEnumerable<T>` from I/O | Callers cannot tell whether re-enumeration is cheap or expensive |

**Fix (priority order):**

1. Materialize once at the boundary that owns I/O: `var heavy = _service.GetHeavyLines(...).ToList();` then `Count` / `Sum` on the list.
2. Better API shape: return `IReadOnlyList<PickLine>` or `Task<IReadOnlyList<PickLine>>` from the service so multiple reads are explicit and cheap.
3. If only one pass is needed, use a single loop or one LINQ aggregate (`Aggregate`, custom scan) instead of two terminal operators.
4. Log/measure enumeration in reviews — treat `IEnumerable` from repositories as "run once unless documented otherwise."

```csharp
var heavy = _service.GetHeavyLines("PB-2201", 1.0m).ToList();
int lineCount = heavy.Count;
decimal totalKg = heavy.Sum(l => l.TotalWeightKg);
```

**Production takeaway:** Karat uses double enumeration to test whether you know `IEnumerable<T>` is a recipe, not a cached collection — matches **Program.cs** Section 5a (lazy until consumed) and Section 4h (each LINQ terminal op walks the sequence).

---

---

#### Q2. (R) A custom `IEnumerator<PickLine>` wraps a file reader. A developer copies the manual loop from a tutorial but drops the `using` block. Under load, temp files pile up on disk. Review the loop. What is missing, and what does `foreach` do differently?

```csharp
public sealed class PickLineFileEnumerator : IEnumerator<PickLine>
{
    private readonly StreamReader _reader;
    private PickLine? _current;

    public PickLineFileEnumerator(string path)
    {
        _reader = new StreamReader(path);
    }

    public PickLine Current => _current!;
    public bool MoveNext() { /* read next line into _current */ return _current != null; }
    public void Dispose() => _reader.Dispose();
}

// Caller:
IEnumerator<PickLine> walk = batch.GetEnumerator();
while (walk.MoveNext())
{
    Process(walk.Current);
    if (walk.Current.Sku.StartsWith("STOP"))
        break; // exit early on first bad aisle
}
// walk goes out of scope here — no Dispose call
```

---

**Answer:**

**Answer:** `IEnumerator<T>` implements `IDisposable` when the concrete enumerator holds unmanaged or file handles. Without `using` or a `finally` that calls `Dispose`, early `break` leaves the `StreamReader` open — file locks and temp directory growth follow. `foreach` always emits a `try/finally` that disposes the enumerator.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Resource leak | No `Dispose()` on manual loop | Handles stay open until GC finalizer (if any) — unreliable |
| Early exit | `break` skips implicit cleanup | Worse under exceptions or conditional exit — matches tutorial pitfall in Section 4b |
| Pattern drift | Tutorial showed `using (IEnumerator<T> ...)` | Copy-paste without `using` loses the main safety net |

**Fix (priority order):**

1. Wrap manual iteration in `using`: `using IEnumerator<PickLine> walk = batch.GetEnumerator();` — same as **Program.cs** Section 4b.
2. Prefer `foreach` when you do not need the raw enumerator — compiler-generated dispose in `finally`.
3. If you must hold an enumerator across methods, implement `try/finally` with explicit `Dispose()` or use `await foreach` with `IAsyncEnumerable<T>` and `ConfigureAwait` patterns for async sources.
4. Add analyzer/code-review rule: any `GetEnumerator()` manual loop requires `using` or documented wrapper.

```csharp
using (IEnumerator<PickLine> walk = batch.GetEnumerator())
{
    while (walk.MoveNext())
    {
        Process(walk.Current);
        if (walk.Current.Sku.StartsWith("STOP"))
            break;
    }
} // Dispose even on break
```

**Production takeaway:** `foreach` is not syntactic sugar only — it is the correct dispose pattern for `IEnumerator<T>`. See **Program.cs** Section 2a (compiler `finally` → `Dispose`) and Section 3a (`Dispose()` on enumerators wrapping files/DB readers).

---

---

#### Q3. (R) A batch-picking screen tries to skip short lines by removing them while iterating. It crashes on the second line every time. Review the loop (same pattern as **Program.cs** Section 4g). What throws, why is it allowed, and what is the safe fix?

```csharp
List<PickLine> lines = _ticket.Lines.ToList();

foreach (PickLine line in lines)
{
    if (line.Quantity < 5)
        lines.Remove(line); // shrink list during foreach
    else
        _picker.Assign(line);
}
```

---

**Answer:**

**Answer:** `List<T>` tracks a version stamp for its enumerator. Adding or removing during `foreach` invalidates that enumerator and throws `InvalidOperationException` ("Collection was modified; enumeration operation may not execute.") — the runtime detects structural change, not logical intent.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Remove` inside `foreach` on same `List<T>` | Guaranteed `InvalidOperationException` after first mutation |
| Logic | In-place filter while walking forward | Even if it did not throw, skipping indices would drop unchecked elements |
| API misuse | Treating `foreach` like index-based `for` with `RemoveAt` | Common UI/service bug when "cleaning" collections live |

**Fix (priority order):**

1. Iterate a snapshot: `foreach (PickLine line in lines.ToList())` and mutate the original list — or build a new list of lines to remove.
2. Reverse `for` loop with index if you must remove in place: `for (int i = lines.Count - 1; i >= 0; i--)`.
3. Prefer `lines.RemoveAll(l => l.Quantity < 5)` then a second pass for `_picker.Assign`.
4. Never add to a list you are actively `foreach`-ing on the same instance — same exception as remove.

```csharp
lines.RemoveAll(l => l.Quantity < 5);
foreach (PickLine line in lines)
    _picker.Assign(line);
```

**Production takeaway:** The pick-ticket demo in **Program.cs** Section 4g exists because this fails in production UI code daily — Karat expects you to name `InvalidOperationException` and choose snapshot or `RemoveAll`, not "it worked once in a small list."

---

---

#### Q4. (M) A developer builds a lazy LINQ pipeline over live pick lines, logs the count, then mutates the underlying list before a second `foreach`. Results differ between the two passes. Walk through what runs when and why the second pass can change.

```csharp
List<PickLine> pickList = LoadOpenTicket("PB-2201");

IEnumerable<PickLine> heavy = pickList
    .Where(l => l.TotalWeightKg >= 1.0m); // deferred — no filter yet

int previewCount = heavy.Count(); // first full enumeration

pickList.Add(new PickLine("RUSH-ADD", 1, 2.5m)); // mutates source between passes

foreach (PickLine line in heavy) // second enumeration — different sequence
{
    Console.WriteLine(line.Sku);
}
```

What surprises a developer who assumes `heavy` is a snapshot?

---

**Answer:**

**Answer:** `Where` returns a deferred sequence — no filter runs until a terminal operation or `foreach` forces enumeration. The first `Count()` walks `pickList` at that moment; adding `RUSH-ADD` before the second `foreach` changes the source, so the second walk can include the new heavy line that was not counted in `previewCount`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Deferred execution | `heavy` stores query, not results | Developers think they captured "heavy lines at T0" — they captured a filter *recipe* |
| Live backing collection | `pickList` mutated between enumerations | Count and foreach disagree; audit logs show inconsistent totals |
| Mental model | LINQ chain looks like a new collection | No allocation until enumeration — easy to miss in code review |

**Fix (priority order):**

1. Materialize when you need a stable snapshot: `var heavy = pickList.Where(...).ToList();` before count and display.
2. Mutate a copy if the pipeline must stay tied to original ticket: iterate `pickList.ToList()` for reporting.
3. Document whether service methods return live views vs snapshots — return `IReadOnlyList<T>` when stable.
4. Single enumeration when possible: one loop that counts and prints, or `ToList()` once at API boundary.

**Production takeaway:** Deferred execution is a feature for composable LINQ, not a cache — production bugs appear when request handlers mutate shared lists between logging and processing. See **Program.cs** Section 5a and Section 4h preview.

---

---

#### Q5. (M) An iterator method logs each SKU as it yields. A caller breaks out of `foreach` after the first match. Later code assumes every line was scanned. Review the iterator and caller. What does `yield return` guarantee about execution state, and when does work *not* run?

```csharp
private static IEnumerable<PickLine> FirstMatchPerAisle(IEnumerable<PickLine> source)
{
    var seenAisles = new HashSet<string>();

    foreach (PickLine line in source)
    {
        string aisle = line.Sku[..1];
        if (seenAisles.Add(aisle))
        {
            _metrics.RecordScan(line.Sku); // side effect on each yield
            yield return line;
        }
    }
}

// Caller:
foreach (PickLine line in FirstMatchPerAisle(pickList))
{
    Ship(line);
    break; // stop after first aisle representative
}
// Ops dashboard shows 1 scan; warehouse expected full ticket walk
```

---

**Answer:**

**Answer:** A `yield return` method compiles to a state machine that runs only until the consumer asks for the next element via `MoveNext`. Breaking out of `foreach` stops calling `MoveNext`, so the iterator body after the last yielded item never runs — remaining source lines are not scanned and `_metrics.RecordScan` is not called for them.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Partial enumeration | `break` after first `yield` consumed | Iterator paused mid-method; trailing source elements skipped |
| Side effects in iterator | `_metrics.RecordScan` inside yield path | Metrics under-count; ops dashboards lie when callers short-circuit |
| Assumption | "Calling the method processed the ticket" | Method invocation alone does nothing — only enumeration drives work |

**Fix (priority order):**

1. Move metrics to the caller after full materialization if full scans are required: `var reps = FirstMatchPerAisle(pickList).ToList();` then record — or record in caller loop without `break` if policy needs all aisles.
2. Split "dedupe yield" from "audit full ticket": one pass for metrics (`foreach` entire source), one lazy pass for shipping selection.
3. Use `yield break` only to end iteration early by design — document that early consumer exit is supported.
4. Avoid heavy side effects inside iterator bodies; prefer pure filters and explicit logging at materialization boundaries.

**Production takeaway:** `yield return` pauses the method, not completes it — Karat tests whether you explain compiler-generated `IEnumerator` state vs eager methods. See **Program.cs** Section 5 (`yield return` state machine) and Section 5a (work on demand only).

---

---

#### Q6. (P) A code review flags `var lines = GetHeavyLines(...).ToList()` as "unnecessary allocation." The author argues it prevents double DB hits and stabilizes results if the ticket changes mid-request. When is `ToList()` (or `ToArray()`) the right production fix for `IEnumerable<T>`, and when is it waste?

Consider: single `foreach`, multiple LINQ passes, `IEnumerable<T>` returned from repositories, and ASP.NET request-scoped mutation of shared lists.

---

**Answer:**

**Answer:** Materialize when the sequence is expensive, non-idempotent, or tied to a live collection that may change before you finish multiple passes — or when you need count/index/random access. Skip `ToList()` when you have a single forward-only `foreach` over an in-memory collection and no shared mutation for the request lifetime.

**When `ToList()` / `ToArray()` is right:**

- Multiple terminal LINQ operations (`Count`, `Sum`, `Any`, then `foreach`) on the same deferred chain — Q1/Q4 pattern.
- `IEnumerable<T>` from `yield return`, database, or network where re-enumeration repeats I/O.
- Snapshot before parallel work, caching in a request scope, or passing to another thread — lists are safe snapshots; raw lazy sequences are not.
- Stabilizing results when the underlying `List<T>` may be edited during the same ASP.NET request.

**When it is waste:**

- One `foreach` over `List<T>` or an array — already materialized; `.ToList()` copies for no benefit.
- Known cheap sequences (small in-memory constants) where a second pass is still cheaper than allocation — measure, but default to clarity.
- Infinite or very large streams where materialization blows memory — use single-pass streaming instead.

**Production takeaway:** `ToList()` is not a micro-optimization debate — it documents "this is the snapshot boundary." Prefer returning `IReadOnlyList<T>` from services that already materialize so callers do not double-enumerate by accident. Aligns with **Program.cs** `HeavyLines` lazy filter vs **Section 4g** snapshot `new List<PickLine>(pickList)` before risky work.

---

---

#### Q7. (R) Two developers iterate the same `PickBatch` concurrently — one with `foreach`, one with a stored `IEnumerator<PickLine>` from an earlier `GetEnumerator()` call. Intermittent duplicates and skipped SKUs appear. Review `PickBatch` (fresh enumerator per `GetEnumerator()`). What contract did the second developer violate, and how should multiple consumers walk the same batch?

```csharp
PickBatch batch = new PickBatch(/* lines */);

IEnumerator<PickLine> manual = batch.GetEnumerator();
manual.MoveNext(); // advanced once manually

foreach (PickLine line in batch) // second cursor — OK alone
{
    Process(line);
}

while (manual.MoveNext()) // first cursor still mid-stream
{
    Process(manual.Current); // overlaps with foreach timing in other threads
}
```

**Answer:**

**Answer:** Each call to `GetEnumerator()` returns an independent cursor (`PickBatchEnumerator` with its own `_index`). That is correct. The bug is sharing one `IEnumerator` instance across logical passes or threads while also starting another enumeration — two cursors on the same batch are fine in sequence, but concurrent or overlapping manual + `foreach` walks without coordination produce duplicate/skipped processing. `IEnumerator<T>` is not thread-safe and not meant to be shared as shared state.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cursor sharing | Reusing one `IEnumerator` mid-stream while another walk runs | Double-processing or skipped elements depending on interleaving |
| Threading | Concurrent `MoveNext` on same enumerator | Undefined behavior; not supported by BCL collections |
| Design | Treating enumerator as batch-wide singleton | Violates forward-only, one-consumer-per-cursor model |

**Fix (priority order):**

1. One enumerator per pass — never share a single `IEnumerator<T>` between components; call `GetEnumerator()` again (or `foreach`) for each full walk — prefer fresh enumerator over `Reset()` per **Program.cs** Section 4c note.
2. Do not advance a stored enumerator partially then also `foreach` the batch unless you explicitly want two independent views — document which cursor owns which lines.
3. For parallel processing, materialize `batch.ToList()` or index into an array and partition by range — not shared `MoveNext`.
4. Remove `Reset()` from new code paths; `PickBatchEnumerator.Reset()` exists for legacy only.

```csharp
// Two independent full passes — OK:
foreach (PickLine line in batch) ProcessA(line);
foreach (PickLine line in batch) ProcessB(line);

// Not OK: one half-consumed manual cursor + another walk without clear ownership
```

**Production takeaway:** `IEnumerable<T>` is multi-enumerable; `IEnumerator<T>` is single forward cursor — Karat collapses the distinction. See **Program.cs** Section 2 (`GetEnumerator()` fresh per call) and Section 3 (`PickBatchEnumerator` instance state).

---
