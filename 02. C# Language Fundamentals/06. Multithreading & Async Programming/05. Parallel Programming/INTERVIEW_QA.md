# Parallel Programming — Interview Q&A


## Table of Contents

1. [Q1. What is Parallel.For and how does it differ from a regular for loop?](#q1-what-is-parallelfor-and-how-does-it-differ-from-a-regular-for-loop)
2. [Q2. How does Parallel.ForEach work and how does it partition collections?](#q2-how-does-parallelforeach-work-and-how-does-it-partition-collections)
3. [Q3. What is ParallelOptions and how do you use MaxDegreeOfParallelism?](#q3-what-is-paralleloptions-and-how-do-you-use-maxdegreeofparallelism)
4. [Q4. What is the difference between ParallelLoopState.Break() and Stop()?](#q4-what-is-the-difference-between-parallelloopstatebreak-and-stop)
5. [Q5. How does Parallel.Invoke work?](#q5-how-does-parallelinvoke-work)
6. [Q6. How do you use thread-local state in Parallel.For to avoid synchronization?](#q6-how-do-you-use-thread-local-state-in-parallelfor-to-avoid-synchronization)
7. [Q7. How does Parallel.For handle exceptions?](#q7-how-does-parallelfor-handle-exceptions)
8. [Q8. What is PLINQ and how do you use AsParallel()?](#q8-what-is-plinq-and-how-do-you-use-asparallel)
9. [Q9. How does AsOrdered() affect PLINQ performance?](#q9-how-does-asordered-affect-plinq-performance)
10. [Q10. What is WithDegreeOfParallelism in PLINQ?](#q10-what-is-withdegreeofparallelism-in-plinq)
11. [Q11. What is ForAll() in PLINQ and when should you use it?](#q11-what-is-forall-in-plinq-and-when-should-you-use-it)
12. [Q12. How does PLINQ cancellation work with WithCancellation?](#q12-how-does-plinq-cancellation-work-with-withcancellation)
13. [Q13. What are the merge options in PLINQ (WithMergeOptions)?](#q13-what-are-the-merge-options-in-plinq-withmergeoptions)
14. [Q14. How does PLINQ handle exceptions?](#q14-how-does-plinq-handle-exceptions)
15. [Q15. What is the race condition in Parallel.ForEach with shared mutable state?](#q15-what-is-the-race-condition-in-parallelforeach-with-shared-mutable-state)
16. [Q16. Why doesn't Stop() immediately stop a Parallel.For loop?](#q16-why-doesnt-stop-immediately-stop-a-parallelfor-loop)
17. [Q17. What is the danger of using async lambdas inside Parallel.ForEach?](#q17-what-is-the-danger-of-using-async-lambdas-inside-parallelforeach)
18. [Q18. When does PLINQ perform worse than sequential LINQ?](#q18-when-does-plinq-perform-worse-than-sequential-linq)
19. [Q19. What is the thundering herd problem in parallel batch processing?](#q19-what-is-the-thundering-herd-problem-in-parallel-batch-processing)
20. [Q20. How would you parallelize image resizing for 10,000 images?](#q20-how-would-you-parallelize-image-resizing-for-10000-images)
21. [Q21. How would you use PLINQ for a data transformation pipeline with filtering?](#q21-how-would-you-use-plinq-for-a-data-transformation-pipeline-with-filtering)
22. [Q22. How do you implement a parallel aggregation that sums values without lock contention?](#q22-how-do-you-implement-a-parallel-aggregation-that-sums-values-without-lock-contention)
23. [Q23. How do you determine whether parallelism will help for a given workload?](#q23-how-do-you-determine-whether-parallelism-will-help-for-a-given-workload)
24. [Q24. How do you implement parallel batch processing with a bounded degree of parallelism?](#q24-how-do-you-implement-parallel-batch-processing-with-a-bounded-degree-of-parallelism)
25. [Q25. How do you benchmark parallel vs sequential to find the performance crossover point?](#q25-how-do-you-benchmark-parallel-vs-sequential-to-find-the-performance-crossover-point)

---
## Foundation Questions

---

## Q1. What is Parallel.For and how does it differ from a regular for loop?

**Concepts**
- Parallel.For: partitions iterations across ThreadPool threads
- Regular for: sequential, single-threaded
- No guaranteed iteration order
- Shared mutable state requires synchronization
- Returns ParallelLoopResult with IsCompleted and LowestBreakIteration

**Answer**

`Parallel.For(fromInclusive, toExclusive, body)` divides the iteration range across available ThreadPool threads and executes the body delegate concurrently. Each thread processes a chunk of iterations. A regular `for` loop executes iterations one at a time on the calling thread in strict order.

```csharp
// Sequential: processes 0..999 one at a time
for (int i = 0; i < 1000; i++)
    ProcessItem(i);

// Parallel: multiple threads process chunks concurrently
Parallel.For(0, 1000, i => ProcessItem(i));
```

Key differences: (1) iteration order is not guaranteed — `Parallel.For` may process index 500 before index 1; (2) shared mutable state (e.g., incrementing a counter) requires synchronization because multiple threads execute concurrently; (3) exceptions from any iteration are collected and thrown as `AggregateException` after all iterations complete; (4) `Parallel.For` returns `ParallelLoopResult` indicating whether all iterations completed or a `Break`/`Stop` was issued.

Parallelism helps for CPU-bound work. For I/O-bound work or very fast operations (tight loops), the partitioning overhead often makes `Parallel.For` slower than a simple `for` loop.

---

## Q2. How does Parallel.ForEach work and how does it partition collections?

**Concepts**
- Chunks source IEnumerable into partitions per thread
- Partition strategies: range (arrays/lists) vs chunk (enumerables)
- Dynamic load balancing when partitions finish at different speeds
- Partitioner<T> for custom partitioning
- OrderablePartitioner for index-preserving partitioning

**Answer**

`Parallel.ForEach(source, body)` partitions the source collection across ThreadPool threads. For indexed sources (`IList<T>`, arrays), it uses range partitioning — each thread gets a contiguous block of indices, maximizing cache locality. For non-indexed sources (`IEnumerable<T>`), it uses chunk partitioning — threads periodically request a batch of elements, with batch sizes growing as the operation progresses to amortize locking overhead.

Dynamic load balancing is built-in: if one partition finishes early, it steals work from other partitions. This handles heterogeneous collections where some items take much longer to process than others.

```csharp
// Range partitioned (efficient for List/Array):
Parallel.ForEach(new List<int>(Enumerable.Range(0, 10000)),
    item => Process(item));

// Custom partitioner for optimal chunk size:
var partitioner = Partitioner.Create(items, loadBalance: true);
Parallel.ForEach(partitioner, item => Process(item));
```

Custom `Partitioner<T>` is useful when you know the optimal chunk size (e.g., SIMD batch size) or need ordered processing. `OrderablePartitioner<T>` preserves index information for use in the body.

---

## Q3. What is ParallelOptions and how do you use MaxDegreeOfParallelism?

**Concepts**
- MaxDegreeOfParallelism: cap on concurrent threads used
- Default (-1): up to ThreadPool max available threads
- CancellationToken: cooperative cancellation
- TaskScheduler: custom execution scheduler
- Setting to 1: sequential execution (useful for debugging)

**Answer**

`ParallelOptions` configures how `Parallel.For`, `Parallel.ForEach`, and `Parallel.Invoke` use threads:

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
    CancellationToken = cancellationToken
};

Parallel.ForEach(items, options, item => ProcessItem(item));
```

`MaxDegreeOfParallelism = -1` (default) allows the TPL to use as many threads as it deems appropriate. Setting it to a specific value caps concurrent thread usage. Common guidance: for CPU-bound work, set to `Environment.ProcessorCount` to saturate CPUs without over-subscription. For mixed I/O+CPU, higher values may help. Setting to `1` makes the loop sequential — invaluable for debugging parallel code by eliminating concurrency.

`CancellationToken` cancels the loop cooperatively: when the token is signalled, the next iteration dispatch stops, running iterations complete, and `OperationCanceledException` is thrown. It does not abort mid-iteration.

---

## Q4. What is the difference between ParallelLoopState.Break() and Stop()?

**Concepts**
- Break(): stop accepting new iterations above current index; running iterations complete
- Stop(): stop accepting any new iterations; running iterations complete
- Break: LowestBreakIteration set on result
- Stop: result IsCompleted = false
- Neither aborts currently executing iterations

**Answer**

Both `Break()` and `Stop()` signal the parallel loop to stop dispatching new iterations, but they differ in scope. `Stop()` tells the loop to stop dispatching any new iterations — threads that have already started their current iteration will finish, but no new iterations will be started. `Break()` is index-aware: it tells the loop to stop dispatching iterations with an index higher than the current one, but iterations with a lower index may still be dispatched to ensure all elements before the break point are processed.

```csharp
ParallelLoopResult result = Parallel.For(0, 1000, (i, state) =>
{
    if (ShouldStop(i))
        state.Break(); // all iterations 0..i will complete; >i may not
    Process(i);
});

if (!result.IsCompleted)
    Console.WriteLine($"Stopped early; lowest break: {result.LowestBreakIteration}");
```

`Break` is appropriate when you need to process all elements up to the stopping point (like a sorted search that must verify preceding elements). `Stop` is appropriate for search operations where you just want the first match and do not care about order. Neither guarantees stopping all concurrent iterations immediately — currently executing iterations always run to completion.

---

## Q5. How does Parallel.Invoke work?

**Concepts**
- Executes delegates in parallel (not necessarily all concurrent)
- Blocks until all delegates complete
- AggregateException if any delegate throws
- ParallelOptions for cancellation and max parallelism
- Best for fixed set of independent operations

**Answer**

`Parallel.Invoke(params Action[] actions)` executes a set of delegates concurrently and blocks until all complete. It is simpler than `Task.WhenAll` for a fixed, compile-time-known set of independent operations:

```csharp
Parallel.Invoke(
    () => LoadUserData(),
    () => LoadProductData(),
    () => LoadInventoryData()
);
// All three run concurrently; execution continues here when all are done
```

If any delegate throws, exceptions are collected and thrown as a single `AggregateException` after all delegates complete — the same behavior as `Parallel.For`. `Parallel.Invoke` does not guarantee that all delegates run truly concurrently: with two delegates and one available thread, one may run after the other sequentially. With `MaxDegreeOfParallelism = 1`, all run sequentially.

For a dynamic list of operations, prefer `Task.WhenAll(items.Select(item => Task.Run(() => Process(item))))`. `Parallel.Invoke` is ergonomic for a fixed set of unrelated startup tasks that should overlap in time.

---

## Q6. How do you use thread-local state in Parallel.For to avoid synchronization?

**Concepts**
- localInit: creates per-thread initial state
- body: action receiving item, state, and thread-local value; returns updated local value
- localFinally: aggregates per-thread results into shared state (once per thread)
- Minimizes contention compared to Interlocked or lock per-iteration
- Pattern for accumulators: sum, count, max, collect

**Answer**

When each parallel iteration contributes to an aggregate result (sum, count, collection), using a lock or `Interlocked` per iteration creates contention. Thread-local state accumulates a partial result per thread, and `localFinally` merges into the shared result once per thread:

```csharp
long total = 0;

Parallel.For(
    0,                           // fromInclusive
    data.Length,                 // toExclusive
    () => 0L,                    // localInit: each thread starts with 0
    (i, state, localSum) =>      // body: update local accumulator
    {
        return localSum + data[i];
    },
    localSum =>                  // localFinally: merge per-thread total
    {
        Interlocked.Add(ref total, localSum);
    });

Console.WriteLine($"Total: {total}");
```

This pattern reduces contention from N lock acquisitions (one per iteration) to P lock acquisitions (one per thread, where P is the degree of parallelism). `localFinally` is called once per thread even if `Break()` or `Stop()` was called — always runs, making it safe for cleanup. The `Interlocked.Add` in `localFinally` is sufficient because multiple threads rarely call it simultaneously.

---

## Q7. How does Parallel.For handle exceptions?

**Concepts**
- Exceptions from all iterations collected
- Thrown as AggregateException after loop completes (or stops)
- Loop does not stop on first exception — other iterations continue
- InnerExceptions contains all failures
- Handle() for selective exception processing

**Answer**

When one or more iterations of `Parallel.For` or `Parallel.ForEach` throw, the exceptions are collected. The loop does not stop immediately when an exception occurs — other iterations that have already started or are queued continue to completion. After the loop finishes (or all dispatch stops), an `AggregateException` containing all thrown exceptions is thrown to the calling thread.

```csharp
try
{
    Parallel.For(0, 100, i =>
    {
        if (i % 10 == 0)
            throw new InvalidOperationException($"Failed at {i}");
        Process(i);
    });
}
catch (AggregateException ae)
{
    ae.Flatten().Handle(ex =>
    {
        _logger.LogError(ex.Message);
        return ex is InvalidOperationException; // handle these, re-throw others
    });
}
```

If you want to stop on the first exception, use `ParallelLoopState.Stop()` inside a `catch` within the body. However, even `Stop()` does not abort already-running iterations. The collected exception pattern is intentional: it gives visibility into all failures, not just the first one.

---

## Q8. What is PLINQ and how do you use AsParallel()?

**Concepts**
- PLINQ: Parallel LINQ — executes LINQ queries in parallel
- AsParallel() converts IEnumerable to ParallelQuery
- AsSequential() opts back to sequential
- Terminal operators: ToList(), ToArray(), ForAll(), Sum(), etc.
- Query runs on ThreadPool threads

**Answer**

PLINQ extends LINQ to run queries across multiple threads. `AsParallel()` marks the query as parallelizable; PLINQ partitions the source and processes chunks concurrently:

```csharp
// Sequential LINQ:
var results = data
    .Where(x => IsExpensive(x))
    .Select(x => Transform(x))
    .ToList();

// Parallel LINQ:
var results = data
    .AsParallel()
    .Where(x => IsExpensive(x))
    .Select(x => Transform(x))
    .ToList();
```

PLINQ measures whether parallelism is beneficial and may fall back to sequential execution if the source is small or the query overhead exceeds the gain. This makes it self-tuning for mixed workloads, unlike `Parallel.ForEach` which always parallelizes.

Terminal operators like `ToList()`, `ToArray()`, `Sum()`, and `Count()` execute the query. `ForAll(action)` is PLINQ's version of parallel `foreach` — it processes results in parallel without ordering them, which is faster than `foreach` over the result.

---

## Q9. How does AsOrdered() affect PLINQ performance?

**Concepts**
- AsOrdered(): preserves original source order in output
- Default PLINQ: output order is non-deterministic
- AsOrdered() adds merge step — significant overhead
- AsUnordered() explicitly opts out after AsOrdered
- Only use when order is semantically required

**Answer**

By default, PLINQ does not preserve the original source order in the output — results appear in whatever order threads finish processing them. This is faster because threads do not need to coordinate ordering. `AsOrdered()` instructs PLINQ to maintain the original sequence order, but this requires a merge phase where results are buffered and reordered before the terminal operator consumes them.

```csharp
// Fast: unordered results
var results = data.AsParallel()
    .Where(x => Filter(x))
    .Select(x => Transform(x))
    .ToList(); // order is non-deterministic

// Correct order but slower:
var results = data.AsParallel()
    .AsOrdered()
    .Where(x => Filter(x))
    .Select(x => Transform(x))
    .ToList(); // order matches source

// Reset ordering after an ordered section:
var results = data.AsParallel().AsOrdered()
    .Select(x => Transform(x)) // ordered
    .AsUnordered()              // subsequent operations don't need order
    .Where(x => Expensive(x))
    .ToList();
```

Use `AsOrdered()` only when the semantic correctness of the output depends on order (e.g., taking the first N results from a ranked dataset). For aggregation or independent transformations, skip ordering for maximum throughput.

---

## Q10. What is WithDegreeOfParallelism in PLINQ?

**Concepts**
- Controls maximum threads for PLINQ query
- Default: up to 64 threads (capped by PLINQ)
- Useful for throttling against rate-limited external resources
- Over-provisioning degrades throughput through contention
- Set to processor count for pure CPU work

**Answer**

`WithDegreeOfParallelism(n)` is PLINQ's equivalent of `ParallelOptions.MaxDegreeOfParallelism`. It caps the number of threads the query uses:

```csharp
var results = data.AsParallel()
    .WithDegreeOfParallelism(4)
    .Select(item => ProcessItem(item))
    .ToList();
```

For CPU-bound queries, set it to `Environment.ProcessorCount` to keep all cores busy without over-subscription. PLINQ's default is up to 64 threads, which can be excessive for CPU work on an 8-core machine, causing context-switching overhead.

For queries that call external services with rate limits (e.g., an API that allows 10 concurrent calls), `WithDegreeOfParallelism(10)` ensures you do not exceed the limit. This is a common pattern for batch API enrichment scenarios. Note that PLINQ is not ideal for I/O-bound work — `Task.WhenAll` with `SemaphoreSlim` is better because PLINQ blocks threads during I/O.

---

## Q11. What is ForAll() in PLINQ and when should you use it?

**Concepts**
- ForAll(Action<T>): terminal operator that processes results in parallel
- Does not merge results back into a single collection
- Faster than .ToList() followed by foreach
- No output collection — side-effect only
- Thread-safe actions required

**Answer**

`ForAll(action)` is a PLINQ terminal operator that processes each result element in parallel without merging them into an ordered collection. Unlike calling `foreach` on the results of a PLINQ query (which first collects all results), `ForAll` pipes results directly to the action as threads complete, avoiding the collection and ordering overhead.

```csharp
// Collects all into ordered list first, then iterates sequentially
data.AsParallel().Select(Transform).ToList().ForEach(Save);

// Processes results directly in parallel as they are produced:
data.AsParallel().Select(Transform).ForAll(item => Save(item));
```

`ForAll` is appropriate when: you don't need the output collection (fire-and-forget style), the action is thread-safe, and order does not matter. The `Save` action in the example must be thread-safe since multiple threads call it simultaneously. For writing to a shared collection, use a concurrent collection or thread-local aggregation.

---

## Q12. How does PLINQ cancellation work with WithCancellation?

**Concepts**
- WithCancellation(CancellationToken) for cooperative cancellation
- OperationCanceledException thrown to caller when cancelled
- Partitions stop; running iterations complete
- Token also cancels merge/ordering phases
- Combine with try/catch OperationCanceledException

**Answer**

`WithCancellation(token)` attaches a `CancellationToken` to a PLINQ query. When the token is cancelled, the query stops dispatching new partitions, and once running partitions complete, `OperationCanceledException` is thrown:

```csharp
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));

try
{
    var results = data.AsParallel()
        .WithCancellation(cts.Token)
        .WithDegreeOfParallelism(4)
        .Select(item => ExpensiveTransform(item))
        .ToList();
}
catch (OperationCanceledException)
{
    Console.WriteLine("Query cancelled after timeout");
}
```

Cancellation in PLINQ is cooperative — the query checks the token at partition dispatch points, not inside individual `Select`/`Where` lambda executions. If the lambda itself is long-running, pass the token to it manually and check `ct.ThrowIfCancellationRequested()` periodically. PLINQ's built-in cancellation handles the partitioning infrastructure; per-item cancellation is the developer's responsibility.

---

## Q13. What are the merge options in PLINQ (WithMergeOptions)?

**Concepts**
- WithMergeOptions controls how results from threads are collected
- AutoBuffered (default): buffer results, good general performance
- NotBuffered: stream results immediately as produced
- FullyBuffered: collect all results before yielding any (like ToList)
- Affects iteration latency vs throughput trade-off

**Answer**

`WithMergeOptions(ParallelMergeOptions)` controls when and how results from parallel partitions are made available to the consumer:

- `AutoBuffered` (default): threads buffer a chunk of results and hand them off, balancing latency and throughput.
- `NotBuffered`: results are yielded immediately as each partition produces them, minimizing first-result latency. Best for streaming scenarios where the consumer can start processing early.
- `FullyBuffered`: all results are collected before any are yielded. Like `.ToList().AsEnumerable()`. Best when you need all results before acting (e.g., sorting the full output).

```csharp
// Stream results immediately — good for real-time display
foreach (var result in data.AsParallel()
    .WithMergeOptions(ParallelMergeOptions.NotBuffered)
    .Select(Transform))
{
    Display(result); // gets first result as soon as any thread finishes
}
```

`NotBuffered` is valuable when the total computation takes seconds but each result can be processed immediately — the UI or downstream consumer sees progress without waiting for the full query to complete.

---

## Q14. How does PLINQ handle exceptions?

**Concepts**
- Same as Parallel.For: AggregateException after all partitions complete
- Query does not stop on first exception
- Handle() for selective exception processing
- Prefer per-element try/catch in Select for partial results
- AggregateException.Flatten() for nested exceptions

**Answer**

PLINQ collects exceptions from all parallel executions and throws them as `AggregateException` when the query terminates. The query does not stop at the first exception — all already-started work completes. This means you may get partial results alongside exceptions, which PLINQ discards — only the exceptions are observable.

To allow partial results (collect successes, log failures), wrap the transform in a per-element try/catch:

```csharp
var results = data.AsParallel()
    .Select(item =>
    {
        try { return (Success: true, Value: Transform(item), Error: (Exception?)null); }
        catch (Exception ex) { return (Success: false, Value: default, Error: ex); }
    })
    .ToList();

var successes = results.Where(r => r.Success).Select(r => r.Value).ToList();
var failures = results.Where(r => !r.Success).Select(r => r.Error).ToList();
```

This pattern is essential for bulk operations (image processing, ETL) where you want to process as many items as possible and report failures separately rather than failing the entire batch.

---

## Gotchas — Parallel Programming (Interview Traps)

---

#### Gotcha 1. Exceptions Are Collected Into AggregateException, Not Rethrown Immediately

**Concepts**
- Parallel.For/ForEach/Invoke collect all exceptions from all threads
- After all iterations complete (or Stop is called), AggregateException is thrown
- AggregateException.InnerExceptions contains every thrown exception
- Catching Exception instead of AggregateException misses multiple faults
- Flatten() simplifies nested AggregateException hierarchies

**Answer**

When multiple iterations of `Parallel.ForEach` or `Parallel.For` throw exceptions, the parallel loop does not stop immediately — it lets running iterations complete and collects every thrown exception. The `AggregateException` thrown after the loop contains them all in `InnerExceptions`. Catching `Exception` with a plain `catch` block will catch the `AggregateException`, but without iterating `InnerExceptions`, all but the first fault are silently dropped. Always catch `AggregateException` explicitly and use `.Flatten().Handle(...)` or iterate `InnerExceptions` to process every failure.

---

#### Gotcha 2. Shared Mutable State Causes Silent Data Corruption

**Concepts**
- Multiple threads reading and writing the same variable without synchronization
- count++ is not atomic: read-modify-write = three separate operations
- Lost updates: both threads read the same value, both increment, one write is lost
- Non-deterministic: fails more under high core count
- Fix: Interlocked for simple scalars, lock for compound operations, thread-local accumulation for throughput

**Answer**

`Parallel.ForEach` runs loop bodies concurrently on multiple threads. Incrementing a shared `int` counter with `count++` is a three-step operation (read, add, write) — two threads can read the same value, both compute the same result, and one write overwrites the other, losing an increment. This happens silently with no exception and is non-deterministic: tests on a single-core CI machine may pass while production on 16 cores consistently produces wrong counts. Use `Interlocked.Increment(ref count)` for a single atomic increment, or a thread-local accumulator pattern to eliminate contention entirely.

---

#### Gotcha 3. LINQ Source Is Re-Enumerated Under Partitioning

**Concepts**
- PLINQ partitions the source IEnumerable into chunks for parallel processing
- If the source is a non-replayable IEnumerable (e.g., database cursor, network stream), re-enumeration fails
- Materialize with .ToList() or .ToArray() before calling .AsParallel()
- Lazy IEnumerable sources with side effects may execute side effects multiple times
- IList and arrays partition by index and do not re-enumerate

**Answer**

PLINQ partitions the source collection to distribute work across threads. For `IEnumerable` sources that are not `IList` or arrays, PLINQ may use a chunk partitioner that re-reads the source. If the source is a one-time-forward-only cursor (a database reader, a network stream, a generator with side effects), re-enumeration throws or produces incorrect results. Always materialize the source into a `List<T>` or array with `.ToList()` or `.ToArray()` before calling `.AsParallel()`, unless the source is explicitly an array or `IList<T>`.

---

#### Gotcha 4. MaxDegreeOfParallelism = -1 Means Unlimited — Not One Per Core

**Concepts**
- Default MaxDegreeOfParallelism is -1 (unlimited)
- Parallel.ForEach without setting it will use as many threads as the scheduler allows
- On a 64-core machine this can overwhelm downstream resources
- Setting it to Environment.ProcessorCount limits CPU saturation
- For I/O-bound parallel work, a higher value (e.g., 4 × cores) may be appropriate

**Answer**

The default `MaxDegreeOfParallelism = -1` means the Parallel library may use any number of ThreadPool threads, not one per core. On a machine with many cores or under a burst load, this can create hundreds of concurrent iterations, overwhelming databases, external APIs, or file systems. Explicitly set `MaxDegreeOfParallelism = Environment.ProcessorCount` for CPU-bound work to avoid over-subscribing the CPU. For I/O-bound parallel work, a higher value is often beneficial, but it should be tuned and bounded rather than left unlimited.

---

#### Gotcha 5. Break() vs Stop() Have Different Index Semantics

**Concepts**
- Break(): process all iterations with index less than or equal to current — no new iterations beyond this index
- Stop(): stop dispatching all future iterations regardless of index
- Break() with index semantics: lower-index iterations may still run after Break() is called
- Stop() is the correct choice for early exit (e.g., search for first match)
- Break() is for "process at least through this index" guarantees

**Answer**

`ParallelLoopState.Break()` and `Stop()` are both early-exit mechanisms but with different guarantees. `Break()` tells the loop "do not start any new iterations with an index higher than the current iteration's index" — lower-indexed iterations that haven't run yet may still execute to ensure partial ordering. `Stop()` tells the loop "do not start any new iterations at all, regardless of index." For a parallel search where you want to stop as soon as any match is found, `Stop()` is the right choice. `Break()` is for algorithms that need to process a prefix of the range up to some discovered boundary.

---

#### Gotcha 6. Parallel.Invoke May Execute Actions Sequentially

**Concepts**
- Parallel.Invoke may inline some actions on the calling thread
- If only one hardware thread is available, all actions run sequentially
- Not a guarantee of concurrent execution — it is a hint
- For guaranteed parallelism, use Task.WhenAll with Task.Run
- Thread count in ParallelOptions still limits concurrency

**Answer**

`Parallel.Invoke` is a scheduling hint, not a guarantee of simultaneous execution. If there is only one available processor or the ThreadPool is busy, the runtime may execute all provided actions sequentially on the calling thread. Tests that assume parallel execution (e.g., checking that two operations truly overlapped in time) may fail on a single-core CI machine. `Parallel.Invoke` is appropriate for expressing "these actions can be parallelized" — not for guaranteeing they will be. For guaranteed parallel execution, use `Task.WhenAll(Task.Run(a1), Task.Run(a2))`.

---

#### Gotcha 7. PLINQ Overhead Makes It Slower Than Sequential for Small Collections

**Concepts**
- PLINQ adds partitioning, thread coordination, and result merging overhead
- For small collections (<1000 elements) or microsecond-duration operations, overhead exceeds benefit
- AsOrdered() adds merge cost — defeats most parallelism benefit
- Benchmark both paths before choosing PLINQ
- Diminishing returns for operations faster than ~1ms per element

**Answer**

PLINQ's parallelism overhead — source partitioning, cross-thread scheduling, and result merging — is measurable. For collections with fewer than roughly 1 000 elements, or per-element operations that complete in microseconds, the overhead exceeds the parallelism savings and PLINQ is slower than sequential LINQ. Adding `.AsOrdered()` adds a merge phase that serializes output ordering and further reduces the benefit. Always benchmark both the sequential and parallel versions with production-representative data before choosing PLINQ. PLINQ is most effective for large, independent, CPU-bound operations where each item takes at least a millisecond.

---

#### Gotcha 8. Random Is Not Thread-Safe — Parallel Code Produces Incorrect Random Numbers

**Concepts**
- System.Random is not thread-safe — concurrent calls corrupt internal state
- Corrupted state produces sequences of all zeros or other degenerate output
- Thread-static Random instances are the traditional workaround
- Random.Shared (static, .NET 6+) is thread-safe and should be preferred
- Seeding multiple Random instances with the same seed produces identical sequences

**Answer**

`System.Random` maintains internal state that is not protected against concurrent access. When multiple threads call methods on the same `Random` instance in a `Parallel.ForEach` loop, they corrupt each other's internal state, causing the random number generator to return degenerate output (often a long sequence of zeros). The traditional workaround is a `[ThreadStatic] Random` field, each initialized with a different seed. The modern solution, available since .NET 6, is `Random.Shared` — a thread-safe static instance that uses lock-free operations and is safe to call from any number of threads simultaneously.

---

#### Gotcha 9. Side Effects in PLINQ Queries Are Dangerous

**Concepts**
- PLINQ assumes queries are pure functions
- Side effects (incrementing a counter, writing to a list) execute on multiple threads concurrently
- Ordering of side effects is non-deterministic
- Correct use: transform-and-collect pattern (no shared mutation)
- For side-effecting work, use Parallel.ForEach, not PLINQ

**Answer**

PLINQ is built around the functional model where query operations are pure transformations without side effects. When a PLINQ `Select`, `Where`, or other operator accesses shared mutable state — incrementing a counter, writing to a `List<T>`, or updating a dictionary — multiple threads execute those operations concurrently without synchronization, causing data races and incorrect output. PLINQ is appropriate for transforming a source collection into a result collection where each element's transformation is independent. For operations with side effects, use `Parallel.ForEach` with explicit thread-safe access patterns.

---

#### Gotcha 10. Parallel.ForEach With async Body Does Not Await — Use Parallel.ForEachAsync

**Concepts**
- Parallel.ForEach accepts Action<T> — synchronous only
- async lambda becomes async void in that context
- Parallel.ForEach returns before any async work finishes
- Exceptions from async void are lost
- .NET 6+ Parallel.ForEachAsync is the correct API for async parallel work

**Answer**

Passing an `async` lambda to `Parallel.ForEach` makes it `async void` because `Action<T>` is a void-returning delegate. `Parallel.ForEach` has no way to await the returned task — it fires every iteration as a fire-and-forget and returns immediately, before any async work completes. Exceptions are silently swallowed. For async parallel work in .NET 6 and later, use `Parallel.ForEachAsync(source, options, async (item, ct) => { ... })`, which correctly awaits each async body and propagates exceptions. For earlier runtimes, collect tasks with `items.Select(item => ProcessAsync(item))` and await with `Task.WhenAll`, using `SemaphoreSlim` to bound concurrency.

---

## Real-World Scenarios

---

## Q20. How would you parallelize image resizing for 10,000 images?

**Concepts**
- CPU-bound work — ideal for Parallel.ForEach
- ThreadPool saturation to processor count
- Exception handling per image (partial success)
- Progress reporting
- Result collection thread-safety

**Answer**

Image resizing is CPU-bound — ideal for `Parallel.ForEach` with `MaxDegreeOfParallelism` equal to the processor count:

```csharp
public async Task<BatchResult> ResizeImagesAsync(
    IEnumerable<string> imagePaths,
    string outputDir,
    CancellationToken ct)
{
    var successes = new ConcurrentBag<string>();
    var failures = new ConcurrentBag<(string Path, Exception Error)>();

    var options = new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        CancellationToken = ct
    };

    Parallel.ForEach(imagePaths, options, path =>
    {
        try
        {
            using var image = Image.Load(path);
            image.Mutate(x => x.Resize(800, 600));
            var outputPath = Path.Combine(outputDir, Path.GetFileName(path));
            image.Save(outputPath);
            successes.Add(outputPath);
        }
        catch (Exception ex)
        {
            failures.Add((path, ex));
        }
    });

    return new BatchResult(successes.ToList(), failures.ToList());
}
```

`ConcurrentBag` allows thread-safe collection of results. Per-item `try/catch` enables partial success — failed images are recorded but do not stop the batch. `MaxDegreeOfParallelism = ProcessorCount` prevents over-subscription. For very large collections or when memory is constrained, process in chunks and report progress via `IProgress<int>` after each chunk.

---

## Q21. How would you use PLINQ for a data transformation pipeline with filtering?

**Concepts**
- PLINQ Where + Select + ToList
- WithDegreeOfParallelism for CPU control
- AsOrdered if output order matters
- Exception handling with per-element try/catch
- Benchmark vs sequential for the specific workload

**Answer**

PLINQ is natural for independent data transformation pipelines where each element is processed in isolation:

```csharp
public List<ProductDto> TransformProducts(IEnumerable<RawProduct> rawProducts)
{
    return rawProducts
        .AsParallel()
        .WithDegreeOfParallelism(Environment.ProcessorCount)
        .Where(p => p.IsActive && p.Price > 0)
        .Select(p =>
        {
            // CPU-intensive enrichment
            var category = _categoryResolver.Resolve(p.CategoryCode);
            var priceWithTax = _taxCalculator.Calculate(p.Price, p.Region);
            return new ProductDto(p.Id, p.Name, category, priceWithTax);
        })
        .ToList();
}
```

If the order of products in the output must match the input order (e.g., for pagination), add `.AsOrdered()` after `AsParallel()`. If order does not matter and the list is large, omitting `AsOrdered()` is significantly faster.

Benchmark this against sequential LINQ with BenchmarkDotNet. If `_categoryResolver.Resolve` and `_taxCalculator.Calculate` are fast (in-memory lookups), sequential may be faster due to PLINQ overhead. If they involve computation taking milliseconds per item, PLINQ will win.

---

## Q22. How do you implement a parallel aggregation that sums values without lock contention?

**Concepts**
- Thread-local accumulator pattern
- Parallel.For with localInit/localFinally
- Interlocked.Add in localFinally (once per thread, not per iteration)
- PLINQ .Sum() as simpler alternative
- Performance: O(n/P) computation + O(P) merge

**Answer**

```csharp
long[] data = Enumerable.Range(1, 10_000_000).Select(i => (long)i).ToArray();

// Option 1: Parallel.For with thread-local accumulator
long total = 0;
Parallel.For(
    0,
    data.Length,
    () => 0L,                               // localInit: per-thread accumulator
    (i, state, localSum) => localSum + data[i], // body: accumulate locally
    localSum => Interlocked.Add(ref total, localSum) // localFinally: merge once per thread
);

// Option 2: PLINQ (simpler but similar performance)
long total2 = data.AsParallel().Sum();

// Option 3: For custom aggregation (not Sum), use Aggregate:
string combined = data.AsParallel()
    .WithDegreeOfParallelism(4)
    .Aggregate(
        () => new StringBuilder(),            // seed factory per partition
        (sb, item) => sb.Append(item).Append(','), // accumulate
        (sb1, sb2) => sb1.Append(sb2),        // merge partition results
        sb => sb.ToString()                   // final result
    );
```

The thread-local approach with `Parallel.For` reduces `Interlocked.Add` calls from 10 million to P (thread count), typically 8–32. PLINQ's built-in `Sum()` uses the same pattern internally and is equivalent in performance. For custom aggregation that cannot use built-in operators, PLINQ's 4-parameter `Aggregate` overload mirrors the same pattern.

---

## Q23. How do you determine whether parallelism will help for a given workload?

**Concepts**
- Amdahl's Law: maximum speedup limited by sequential fraction
- Gustafson's Law: parallel efficiency for scaling workloads
- Benchmark with BenchmarkDotNet
- CPU-bound vs I/O-bound distinction
- Crossover point: parallel > sequential only above N items

**Answer**

Not all code benefits from parallelism. Amdahl's Law states that if a fraction `f` of the work is sequential (cannot be parallelized), the maximum speedup with P processors is `1 / (f + (1-f)/P)`. A workload where 50% must be sequential can never be more than 2x faster, regardless of how many processors are added.

Practical guidance for .NET parallel code:

1. **CPU-bound and independent**: Excellent candidate. `Parallel.ForEach` or PLINQ. Benchmark to confirm speedup.
2. **I/O-bound**: Use `async/await` + `Task.WhenAll`, not `Parallel.ForEach`. Parallelism here wastes threads on blocking.
3. **Mixed**: Profile to find which phase dominates. Parallelize only the CPU phase.
4. **Small collections**: Parallel overhead often exceeds benefit. Benchmark the crossover point.

```csharp
// BenchmarkDotNet comparison:
[Benchmark] public List<R> Sequential() => items.Select(Transform).ToList();
[Benchmark] public List<R> Parallel() => items.AsParallel().Select(Transform).ToList();
```

Run with `BenchmarkDotNet` at realistic collection sizes. Add `[Params(100, 1000, 10000)]` to find the crossover. Generally, items below 100–1000 are not worth parallelizing unless each item takes >1ms to process.

---

## Q24. How do you implement parallel batch processing with a bounded degree of parallelism?

**Concepts**
- Process items in bounded batches
- SemaphoreSlim for concurrency control
- Task.WhenAll for fan-out
- Progress reporting per batch
- Graceful cancellation between batches

**Answer**

```csharp
public async Task ProcessInBatchesAsync(
    IReadOnlyList<Item> items,
    int batchSize,
    int maxConcurrent,
    IProgress<int>? progress,
    CancellationToken ct)
{
    using var semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
    int processed = 0;

    var batches = items
        .Select((item, index) => new { item, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.item).ToList());

    foreach (var batch in batches)
    {
        ct.ThrowIfCancellationRequested();

        var batchTasks = batch.Select(async item =>
        {
            await semaphore.WaitAsync(ct);
            try { await ProcessItemAsync(item, ct); }
            finally { semaphore.Release(); }
        });

        await Task.WhenAll(batchTasks);
        processed += batch.Count;
        progress?.Report(processed * 100 / items.Count);
    }
}
```

Batching (grouping items before dispatching) controls memory pressure — you don't create tasks for all items upfront. `SemaphoreSlim` bounds concurrent executions within each batch. `progress?.Report(...)` provides user feedback. `ct.ThrowIfCancellationRequested()` between batches gives a clean cancellation point without stopping mid-batch processing.

---

## Q25. How do you benchmark parallel vs sequential to find the performance crossover point?

**Concepts**
- BenchmarkDotNet for accurate micro-benchmarks
- [Params] for testing multiple collection sizes
- Warmup runs to stabilize JIT
- GC pressure consideration (allocations)
- Baseline comparison methodology

**Answer**

BenchmarkDotNet is the standard tool for .NET performance measurement. Use `[Params]` to test multiple collection sizes and find where parallelism becomes beneficial:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net100)]
public class ParallelVsSequentialBenchmarks
{
    [Params(100, 1_000, 10_000, 100_000)]
    public int N;

    private double[] _data = null!;

    [GlobalSetup]
    public void Setup() => _data = Enumerable.Range(1, N)
        .Select(i => (double)i).ToArray();

    [Benchmark(Baseline = true)]
    public double Sequential()
        => _data.Select(Math.Sqrt).Sum();

    [Benchmark]
    public double PLinq()
        => _data.AsParallel().Select(Math.Sqrt).Sum();

    [Benchmark]
    public double ParallelFor()
    {
        double total = 0;
        Parallel.For(0, _data.Length,
            () => 0.0,
            (i, _, local) => local + Math.Sqrt(_data[i]),
            local => Interlocked.Exchange(ref total, total + local));
        return total;
    }
}
```

Run with `BenchmarkDotNet.Artifacts` and examine the `Ratio` column (relative to Baseline). Typical findings: sequential wins below ~1,000 items for simple operations; PLINQ wins above ~10,000 items for operations taking >10µs per item. Memory allocations (`[MemoryDiagnoser]`) reveal PLINQ's per-partition allocation overhead. Use these findings to set a minimum collection size threshold before enabling parallelism in production code.
