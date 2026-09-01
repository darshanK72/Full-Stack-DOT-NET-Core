# Parallel Programming — Interview Q&A

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

## Gotchas & Traps

---

## Q15. What is the race condition in Parallel.ForEach with shared mutable state?

**Concepts**
- Multiple threads writing to the same variable without synchronization
- Interleaved read-modify-write operations
- Lost updates
- Fix: Interlocked, lock, or thread-local state
- No compiler/runtime warning for this class of bug

**Answer**

```csharp
// RACE CONDITION: multiple threads increment count simultaneously
int count = 0;
Parallel.ForEach(items, item =>
{
    if (Matches(item))
        count++; // NOT thread-safe: read-modify-write is not atomic
});
Console.WriteLine($"Count: {count}"); // result is non-deterministic, usually wrong
```

| Category | Problem | Impact |
|---|---|---|
| Race Condition | `count++` is three operations (read, increment, write) — not atomic | Count is consistently lower than actual; magnitude depends on core count |
| Non-Determinism | Result varies between runs | Tests may pass sometimes, fail other times |
| No Warning | Compiler does not detect this | Silent data corruption |

**Fix priority:**
1. Use `Interlocked.Increment(ref count)` for atomic increment.
2. Use `lock` if the operation is more complex than a single increment.
3. Use thread-local accumulation (`localInit`/`localFinally` pattern) for maximum throughput — eliminates contention entirely.

---

## Q16. Why doesn't Stop() immediately stop a Parallel.For loop?

**Concepts**
- Stop() signals future dispatch to cease
- Already-started iterations run to completion
- "No new iterations beyond current" — not abort
- IsStopped property on ParallelLoopState
- Use IsStopped to short-circuit long-running iterations

**Answer**

A common misconception is that `Stop()` immediately terminates all iterations. It does not — it is a signal to the scheduler to stop dispatching new iterations. Iterations already in progress continue to their natural end. Calling `Stop()` from one thread while 7 other threads are mid-iteration means all 7 will finish their current item before the loop winds down.

```csharp
Parallel.For(0, 1_000_000, (i, state) =>
{
    if (FoundResult(i))
    {
        state.Stop();
        return; // exit this iteration quickly
    }

    // For long-running iterations, also check IsStopped periodically:
    for (int step = 0; step < 1000; step++)
    {
        if (state.IsStopped) return; // exit early if another thread stopped
        DoStep(i, step);
    }
});
```

If each iteration is long (contains inner loops or multiple operations), check `state.IsStopped` periodically inside the iteration to exit as quickly as possible after `Stop()` is signaled. Without this check, a 10-second iteration will run its full duration even after `Stop()` was called.

---

## Q17. What is the danger of using async lambdas inside Parallel.ForEach?

**Concepts**
- Parallel.ForEach action is Action<T> — synchronous
- async lambda becomes async void — exceptions lost
- Parallel.ForEach returns before async work completes
- TPL does not await async bodies
- Fix: collect tasks and await with Task.WhenAll

**Answer**

```csharp
// BUG: async lambda in Parallel.ForEach becomes async void
Parallel.ForEach(items, async item =>
{
    await ProcessAsync(item); // fire-and-forget! Parallel.ForEach doesn't await this
});
// Returns here before any async work completes — all tasks are fire-and-forget
// Exceptions are lost (async void behavior)
```

| Category | Problem | Impact |
|---|---|---|
| Premature Return | Parallel.ForEach completes before async operations finish | Downstream code uses incomplete results |
| Exception Loss | async void swallows exceptions from each item | Silent failures for all async operations |
| Resource Leak | Async operations continue after method returns | Concurrent access to disposed resources |

**Fix priority:**
1. Collect tasks instead: `var tasks = items.Select(item => ProcessAsync(item)); await Task.WhenAll(tasks);`
2. For bounded concurrency: use `SemaphoreSlim` + `Task.WhenAll`.
3. Never use `async` lambdas with `Parallel.ForEach`, `Parallel.For`, or `Parallel.Invoke` — these APIs are synchronous only.

---

## Q18. When does PLINQ perform worse than sequential LINQ?

**Concepts**
- Partitioning overhead for small collections
- Fast operations where overhead > work
- Ordered queries with merge overhead
- Side effects in queries (breaks parallelism assumptions)
- Memory pressure from parallel execution

**Answer**

PLINQ has overhead: source partitioning, cross-thread coordination, and result merging. For small collections (fewer than ~1000 elements) or operations that complete in microseconds, this overhead exceeds the parallelism benefit and PLINQ runs slower than sequential LINQ.

```csharp
// FAST sequentially — parallelism overhead dominates:
var sum = numbers.AsParallel().Sum(); // slower than numbers.Sum() for small arrays

// SLOW sequentially — parallelism pays off:
var results = largeImagePaths.AsParallel()
    .Select(path => LoadAndTransformImage(path)) // 50ms per item
    .ToList();
```

Other scenarios where PLINQ hurts: `AsOrdered()` on a large collection (merge overhead), queries that access shared state (serial contention), and non-deterministic queries where ordering matters to the logic. Benchmark before and after adding `AsParallel()` — it is not always a win. PLINQ works best for independent, CPU-bound operations on large collections (>1000 items) where each item takes >1ms to process.

---

## Q19. What is the thundering herd problem in parallel batch processing?

**Concepts**
- All workers complete simultaneously and hammer a downstream resource
- Database, API, or network becomes bottleneck
- Staggered starts as mitigation
- Bounded concurrency limits peak load
- Exponential backoff on rate limit responses

**Answer**

When `Parallel.ForEach` or `Task.WhenAll` fans out to many workers that all finish roughly simultaneously, they can all hit a downstream resource (database, external API, file system) at once, creating a spike that exceeds the resource's capacity. This "thundering herd" effect causes the downstream resource to reject or slow down requests.

```csharp
// THUNDERING HERD: all 10,000 DB calls start simultaneously
await Task.WhenAll(items.Select(item => _db.SaveAsync(item)));

// BOUNDED: at most 20 concurrent DB calls at any time
var sem = new SemaphoreSlim(20);
await Task.WhenAll(items.Select(async item =>
{
    await sem.WaitAsync();
    try { await _db.SaveAsync(item); }
    finally { sem.Release(); }
}));
```

Bounded concurrency with `SemaphoreSlim` (or `Polly`'s `BulkheadPolicy`) caps peak load. For APIs with per-second rate limits, add `await Task.Delay(...)` between batches. Staggered starts (e.g., `await Task.Delay(i * 10)`) can smooth the load curve. The goal is to keep downstream resources within their capacity while still achieving meaningful parallelism.

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
