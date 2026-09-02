# Concurrent Collections — Interview Q&A


## Table of Contents

1. [Q1. What are concurrent collections and why do they exist?](#q1-what-are-concurrent-collections-and-why-do-they-exist)
2. [Q2. What is ConcurrentDictionary<TKey, TValue> and how does it differ from Dictionary?](#q2-what-is-concurrentdictionarytkey-tvalue-and-how-does-it-differ-from-dictionary)
3. [Q3. What is the danger with ConcurrentDictionary.GetOrAdd and factory side effects?](#q3-what-is-the-danger-with-concurrentdictionarygetoradd-and-factory-side-effects)
4. [Q4. What is ConcurrentQueue<T> and how does it differ from Queue<T>?](#q4-what-is-concurrentqueuet-and-how-does-it-differ-from-queuet)
5. [Q5. What is ConcurrentStack<T> and when would you use it over ConcurrentQueue<T>?](#q5-what-is-concurrentstackt-and-when-would-you-use-it-over-concurrentqueuet)
6. [Q6. What is ConcurrentBag<T> and when is it appropriate?](#q6-what-is-concurrentbagt-and-when-is-it-appropriate)
7. [Q7. What is BlockingCollection<T> and how does it implement producer-consumer?](#q7-what-is-blockingcollectiont-and-how-does-it-implement-producer-consumer)
8. [Q8. What happens if CompleteAdding() is never called on a BlockingCollection?](#q8-what-happens-if-completeadding-is-never-called-on-a-blockingcollection)
9. [Q9. How does Channel<T> differ from BlockingCollection<T>?](#q9-how-does-channelt-differ-from-blockingcollectiont)
10. [Q10. What is IProducerConsumerCollection<T> and how is it used?](#q10-what-is-iproducerconsumercollectiont-and-how-is-it-used)
11. [Q11. How does enumeration work on concurrent collections?](#q11-how-does-enumeration-work-on-concurrent-collections)
12. [Q12. When should you choose ConcurrentDictionary over a lock-protected Dictionary?](#q12-when-should-you-choose-concurrentdictionary-over-a-lock-protected-dictionary)
13. [Q13. How do you implement a simple cache with ConcurrentDictionary with expiry?](#q13-how-do-you-implement-a-simple-cache-with-concurrentdictionary-with-expiry)
14. [Q14. How does Channel<T> support multiple producers and multiple consumers?](#q14-how-does-channelt-support-multiple-producers-and-multiple-consumers)
15. [Q15. What is the check-then-act race condition with ConcurrentDictionary?](#q15-what-is-the-check-then-act-race-condition-with-concurrentdictionary)
16. [Q16. What is the ConcurrentBag contention problem when used as a cross-thread queue?](#q16-what-is-the-concurrentbag-contention-problem-when-used-as-a-cross-thread-queue)
17. [Q17. What is the BlockingCollection dispose/cancel race?](#q17-what-is-the-blockingcollection-disposecancel-race)
18. [Q18. Why does ConcurrentDictionary.Count perform a full scan?](#q18-why-does-concurrentdictionarycount-perform-a-full-scan)
19. [Q19. What happens when you enumerate a ConcurrentQueue while it is being modified?](#q19-what-happens-when-you-enumerate-a-concurrentqueue-while-it-is-being-modified)
20. [Q20. Implement a thread-safe cache with ConcurrentDictionary and Lazy<T> for expensive initialization.](#q20-implement-a-thread-safe-cache-with-concurrentdictionary-and-lazyt-for-expensive-initialization)
21. [Q21. Build a multi-producer single-consumer pipeline with Channel<T>.](#q21-build-a-multi-producer-single-consumer-pipeline-with-channelt)
22. [Q22. Replace a lock+Queue with BlockingCollection for bounded producer-consumer.](#q22-replace-a-lockqueue-with-blockingcollection-for-bounded-producer-consumer)
23. [Q23. Diagnose a hang in a BlockingCollection-based pipeline.](#q23-diagnose-a-hang-in-a-blockingcollection-based-pipeline)
24. [Q24. Implement a work-stealing job queue using ConcurrentStack.](#q24-implement-a-work-stealing-job-queue-using-concurrentstack)
25. [Q25. How do you choose the right concurrent collection for a log aggregation service?](#q25-how-do-you-choose-the-right-concurrent-collection-for-a-log-aggregation-service)

---
## Foundation Questions

---

## Q1. What are concurrent collections and why do they exist?

**Concepts**
- Thread-safe alternatives to standard collections
- Avoid lock-based wrapping of non-thread-safe collections
- System.Collections.Concurrent namespace
- Fine-grained locking or lock-free algorithms internally
- Do not guarantee atomicity of multi-step operations

**Answer**

Standard .NET collections (`List<T>`, `Dictionary<TK,TV>`, `Queue<T>`) are not thread-safe — concurrent access without external synchronization causes data corruption, lost updates, or exceptions. The naive fix of wrapping every access with a `lock` works but creates a single contention point that scales poorly.

The `System.Collections.Concurrent` namespace provides collections designed for concurrent access. They use techniques like fine-grained locking (locking only a portion of the structure), lock-free algorithms (CAS loops with `Interlocked.CompareExchange`), and thread-local work queues (work stealing in `ConcurrentBag<T>`) to allow multiple threads to operate simultaneously with minimal contention.

Crucially, individual operations on concurrent collections are thread-safe, but multi-step operations across multiple calls are not automatically atomic. For example, checking if a key exists in `ConcurrentDictionary` and then adding it are two separate operations — you must use the atomic `GetOrAdd`/`AddOrUpdate` methods instead.

---

## Q2. What is ConcurrentDictionary<TKey, TValue> and how does it differ from Dictionary?

**Concepts**
- Thread-safe dictionary with fine-grained striped locking
- Atomic operations: TryAdd, TryRemove, TryUpdate, TryGetValue
- AddOrUpdate: atomic read-modify-write
- GetOrAdd: atomic get-or-create
- Snapshot enumeration (safe but may not reflect all current entries)

**Answer**

`ConcurrentDictionary<TK,TV>` uses internal lock striping — the key space is divided into segments (typically 2× processor count by default), and only the segment containing the key is locked for most operations. This allows multiple threads to operate on different keys simultaneously.

Key atomic operations:
- `TryAdd(key, value)` — adds only if key doesn't exist; returns `false` if already present.
- `TryRemove(key, out value)` — removes atomically and returns the removed value.
- `TryUpdate(key, newValue, comparisonValue)` — updates only if current value equals `comparisonValue` (optimistic concurrency).
- `AddOrUpdate(key, addValue, updateFactory)` — atomically adds or updates.
- `GetOrAdd(key, valueFactory)` — returns existing value or adds a new one if not present.

```csharp
var dict = new ConcurrentDictionary<string, int>();

// Thread-safe increment of a counter:
dict.AddOrUpdate("hits", 1, (key, existing) => existing + 1);

// Get-or-create:
var config = dict.GetOrAdd("timeout", key => LoadConfig(key));
```

Enumeration (`foreach`) over a `ConcurrentDictionary` is snapshot-safe (no `InvalidOperationException` during modification) but may not reflect all changes happening concurrently.

---

## Q3. What is the danger with ConcurrentDictionary.GetOrAdd and factory side effects?

**Concepts**
- GetOrAdd's factory may execute multiple times under contention
- Factory is not called under the dictionary's lock
- Only one result is stored; others are discarded
- Side effects in factory (file creation, DB insert) execute multiple times
- Workaround: use Lazy<T> as value or external lock

**Answer**

`GetOrAdd(key, valueFactory)` is NOT atomic at the factory level. If multiple threads call `GetOrAdd` with the same key simultaneously when the key does not exist, all threads may execute the `valueFactory` concurrently. The dictionary stores only one result — the first to be inserted — and discards the others. But all factories have run:

```csharp
// DANGER: factory may run multiple times
var cache = new ConcurrentDictionary<string, DbConnection>();
var conn = cache.GetOrAdd("main", _ =>
{
    // May execute 3 times if 3 threads race here simultaneously
    return new SqlConnection(connectionString); // 3 connections opened, 2 leaked
});
```

The fix for expensive or side-effectful factories is to use `Lazy<T>` as the value:

```csharp
var cache = new ConcurrentDictionary<string, Lazy<DbConnection>>();
var lazy = cache.GetOrAdd("main",
    _ => new Lazy<DbConnection>(() => new SqlConnection(connStr)));
var conn = lazy.Value; // factory runs exactly once, even under concurrency
```

Multiple `Lazy<T>` objects may be created (one per racing thread), but only one is stored, and `Lazy.Value` ensures the factory inside runs exactly once on the winning `Lazy` instance.

---

## Q4. What is ConcurrentQueue<T> and how does it differ from Queue<T>?

**Concepts**
- Thread-safe FIFO queue
- Lock-free enqueue/dequeue using CAS
- TryDequeue(out T item) — returns false if empty
- TryPeek(out T item) — look without removing
- IsEmpty and Count (Count requires full scan — approximate)

**Answer**

`ConcurrentQueue<T>` is a thread-safe FIFO queue using a lock-free linked-list structure internally. Multiple threads can enqueue and dequeue simultaneously without blocking each other.

```csharp
var queue = new ConcurrentQueue<WorkItem>();

// Producer (any thread):
queue.Enqueue(new WorkItem { Id = 1 });

// Consumer (any thread):
if (queue.TryDequeue(out WorkItem? item))
    Process(item);
```

Unlike `Queue<T>.Dequeue()` which throws if empty, `ConcurrentQueue.TryDequeue` returns `false` for empty queues — avoiding exceptions in concurrent scenarios where empty is a valid state.

Important: `Count` requires scanning the entire queue and is not atomically consistent — use `IsEmpty` for fast empty checks. Enumeration is snapshot-safe (creates an internal snapshot) but does not modify the queue — use it for diagnostics, not for drain-and-process patterns (use `TryDequeue` in a loop instead).

---

## Q5. What is ConcurrentStack<T> and when would you use it over ConcurrentQueue<T>?

**Concepts**
- Thread-safe LIFO stack
- Lock-free push/pop using CAS on head node
- TryPop, TryPeek, PushRange, TryPopRange for batch operations
- Work-stealing alternative when LIFO order is needed
- Depth-first processing patterns

**Answer**

`ConcurrentStack<T>` is a thread-safe LIFO stack. `Push` and `TryPop` use lock-free compare-and-swap on the head pointer. Batch operations `PushRange` (push an array atomically) and `TryPopRange` (pop up to N items) are key differentiators from `ConcurrentQueue`:

```csharp
var stack = new ConcurrentStack<int>();

// Batch push — atomically pushes array of items
stack.PushRange(new[] { 1, 2, 3, 4, 5 });

// Batch pop — gets up to 3 items efficiently
int[] buffer = new int[3];
int count = stack.TryPopRange(buffer, 0, 3);
```

Use `ConcurrentStack` when you need LIFO ordering (most recently added item processed first) — DFS graph traversal, undo history, most-recently-used caches. `PushRange`/`TryPopRange` enable efficient batch processing without the overhead of N individual CAS operations. For work-stealing algorithms, `ConcurrentStack` is often used as the per-thread deque (processed LIFO locally, stolen FIFO from tail).

---

## Q6. What is ConcurrentBag<T> and when is it appropriate?

**Concepts**
- Unordered thread-safe collection optimized for same-thread produce-consume
- Per-thread work-stealing queues internally
- TryTake removes an arbitrary item (no ordering guarantee)
- Best for: object pooling, parallel aggregation
- Poor for: producer-consumer across different threads

**Answer**

`ConcurrentBag<T>` is an unordered collection where each thread has its own local list. When a thread adds and takes items, it operates on its own local list without contention. When a thread's list is empty and it tries `TryTake`, it "steals" from another thread's list.

```csharp
var bag = new ConcurrentBag<Result>();

// Parallel processing — each thread adds to its own local list
Parallel.ForEach(items, item =>
{
    var result = Process(item);
    bag.Add(result); // fast: adds to current thread's local list
});

// Collecting results — order is non-deterministic
var allResults = bag.ToList();
```

`ConcurrentBag` shines for parallel aggregation (each thread produces results and adds them) and object pooling (threads frequently get and return the same objects — the work-stealing design ensures locality). It performs poorly as a producer-consumer queue across different threads because cross-thread access triggers work-stealing, which is slower than same-thread access and involves more contention. For cross-thread communication, use `ConcurrentQueue` or `Channel<T>`.

---

## Q7. What is BlockingCollection<T> and how does it implement producer-consumer?

**Concepts**
- Wraps any IProducerConsumerCollection<T> (default: ConcurrentQueue)
- Add() blocks when at capacity (bounded)
- Take() blocks when empty (bounded or unbounded)
- CompleteAdding() signals no more items
- GetConsumingEnumerable() for foreach-style consumption

**Answer**

`BlockingCollection<T>` wraps a concurrent collection and adds blocking and bounding semantics. It is the standard .NET implementation of a bounded producer-consumer queue:

```csharp
// Bounded: Add blocks when queue reaches capacity 10
var queue = new BlockingCollection<Work>(boundedCapacity: 10);

// Producer:
var producer = Task.Run(() =>
{
    try
    {
        foreach (var item in GenerateWork())
            queue.Add(item); // blocks if queue is full (backpressure)
    }
    finally
    {
        queue.CompleteAdding(); // CRITICAL: must always be called
    }
});

// Consumer:
var consumer = Task.Run(() =>
{
    foreach (var item in queue.GetConsumingEnumerable())
        Process(item); // loop exits when queue is marked complete AND empty
});

await Task.WhenAll(producer, consumer);
```

`CompleteAdding()` must be called in a `finally` block — without it, the consumer's `GetConsumingEnumerable()` loop never exits. The `boundedCapacity` creates natural backpressure: if the producer is faster than the consumer, `Add` blocks rather than growing memory without bound.

---

## Q8. What happens if CompleteAdding() is never called on a BlockingCollection?

**Concepts**
- GetConsumingEnumerable() blocks indefinitely waiting for more items
- Consumer thread hangs forever after all work is processed
- Application cannot shutdown gracefully
- CompleteAdding must be in a finally block
- TryAdd with timeout as alternative to prevent producer blocking

**Answer**

`BlockingCollection<T>.GetConsumingEnumerable()` implements `IEnumerable<T>` by calling `Take()` repeatedly. `Take()` blocks until either an item is available or `CompleteAdding()` is called. If `CompleteAdding()` is never called, the consumer blocks forever after processing all current items — the loop never exits.

```csharp
// BUG: CompleteAdding() omitted on exception path
var queue = new BlockingCollection<string>(10);

var producer = Task.Run(() =>
{
    foreach (var item in source)
        queue.Add(item);
    // Exception here means CompleteAdding never called
    queue.CompleteAdding(); // only reached on happy path
});

var consumer = Task.Run(() =>
{
    foreach (var item in queue.GetConsumingEnumerable())
        Process(item);
    // Consumer hangs here if CompleteAdding never called
});
```

Fix: wrap the producer's entire body in `try/finally` with `CompleteAdding()` in the `finally`. This ensures the consumer is always signaled, even on exception or cancellation. If the consumer should stop on cancellation before the producer completes, use the `CancellationToken` overload of `GetConsumingEnumerable(token)`.

---

## Q9. How does Channel<T> differ from BlockingCollection<T>?

**Concepts**
- Channel<T>: async-first design (ReadAsync, WriteAsync)
- BlockingCollection<T>: synchronous blocking (Take/Add)
- BoundedChannel vs UnboundedChannel
- ChannelReader/ChannelWriter separation of concerns
- Channel<T> preferred for async/await pipelines

**Answer**

`Channel<T>` (System.Threading.Channels, .NET Core 2.1+) is the modern async-native producer-consumer mechanism. Unlike `BlockingCollection` which blocks threads on `Take`/`Add`, `Channel` provides fully async operations that release the thread during waits:

```csharp
// Bounded channel: writer blocks (async) when full
var channel = Channel.CreateBounded<Work>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
});

// Producer:
var producer = Task.Run(async () =>
{
    await foreach (var item in GenerateWorkAsync())
        await channel.Writer.WriteAsync(item); // async backpressure
    channel.Writer.Complete(); // analog of CompleteAdding()
});

// Consumer:
var consumer = Task.Run(async () =>
{
    await foreach (var item in channel.Reader.ReadAllAsync())
        await ProcessAsync(item); // async processing
});

await Task.WhenAll(producer, consumer);
```

Choose `Channel<T>` for new async code — it does not hold threads during waits, supports multiple readers/writers, and integrates with `IAsyncEnumerable`. Use `BlockingCollection<T>` in synchronous code or when integrating with legacy thread-based pipelines. `ChannelWriter` and `ChannelReader` can be separated and passed to different components as focused interfaces.

---

## Q10. What is IProducerConsumerCollection<T> and how is it used?

**Concepts**
- Interface implemented by ConcurrentQueue, ConcurrentStack, ConcurrentBag
- TryAdd(T item) and TryTake(out T item) abstract operations
- BlockingCollection uses it as backing store
- Allows swapping backing collection type
- Custom implementations possible

**Answer**

`IProducerConsumerCollection<T>` is an interface with two core methods: `TryAdd(T item)` and `TryTake(out T item)`. `ConcurrentQueue<T>`, `ConcurrentStack<T>`, and `ConcurrentBag<T>` all implement it. `BlockingCollection<T>` wraps any `IProducerConsumerCollection<T>` to add blocking and bounding semantics.

This design lets you swap the ordering behavior of a `BlockingCollection` by changing the backing collection:

```csharp
// Default: FIFO (ConcurrentQueue)
var fifoQueue = new BlockingCollection<Work>();

// LIFO: most recently added item processed first
var lifoQueue = new BlockingCollection<Work>(new ConcurrentStack<Work>(), capacity: 50);

// Unordered (work-stealing):
var bagQueue = new BlockingCollection<Work>(new ConcurrentBag<Work>(), capacity: 50);
```

The `IProducerConsumerCollection<T>` abstraction also allows custom implementations — for example, a priority queue that orders items by priority while maintaining thread safety. Custom implementations must guarantee that `TryAdd` and `TryTake` are thread-safe. In practice, this interface is rarely implemented by application code; it is most useful for the `BlockingCollection` parameterization pattern.

---

## Q11. How does enumeration work on concurrent collections?

**Concepts**
- Snapshot iteration: no InvalidOperationException during modification
- Snapshot may not reflect all in-flight changes
- ConcurrentDictionary enumeration is weakly consistent
- ConcurrentQueue: enumerator sees items added before enumeration started
- Do not use enumeration for drain — use TryDequeue/TryTake

**Answer**

Standard collection enumeration throws `InvalidOperationException` if the collection is modified during iteration. Concurrent collections instead use snapshot-based enumeration: they take a consistent view at the start of enumeration and iterate that view, never throwing on concurrent modification.

This comes with a trade-off: the snapshot may not reflect modifications made after enumeration began. Items added after the snapshot was taken are not included; items removed may still appear in the snapshot. For `ConcurrentDictionary`, the documentation calls this "weakly consistent" — the enumeration reflects some valid state of the dictionary that existed during the enumeration, but not necessarily a precise snapshot at a single point in time.

```csharp
var dict = new ConcurrentDictionary<int, string>();
// Safe: no exception if dict is modified during foreach
foreach (var kvp in dict)
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
```

For processing all items exactly once (drain pattern), do not use `foreach` on concurrent collections. Instead, loop with `TryDequeue` or `TryTake`:

```csharp
// Correct drain pattern:
while (queue.TryDequeue(out var item))
    Process(item);
```

---

## Q12. When should you choose ConcurrentDictionary over a lock-protected Dictionary?

**Concepts**
- ConcurrentDictionary: better under concurrent read+write load
- lock + Dictionary: simpler, lower overhead for low-concurrency
- ConcurrentDictionary has higher per-operation overhead (CAS retry)
- ConcurrentDictionary: multi-reader without contention
- Measure: for low write rates, ReaderWriterLockSlim + Dictionary may win

**Answer**

`ConcurrentDictionary` is the right choice when multiple threads frequently read and write the dictionary simultaneously. Its striped locking (one lock per segment) allows concurrent writes to different keys and concurrent reads from any key — far better than a single `lock` that serializes all access.

For very low write rates (reads dominate by 100:1 or more), `ReaderWriterLockSlim` + `Dictionary` can be faster because readers do not contend with each other at all. For very low total access rates (e.g., a config dictionary read a few times per second), a plain `lock` + `Dictionary` is simpler and fast enough.

```
Scenario                          | Recommendation
----------------------------------|-------------------
High read+write concurrency       | ConcurrentDictionary
Read-heavy, rare writes           | ReaderWriterLockSlim + Dictionary
Low concurrency, simplicity first | lock + Dictionary
Async-await access pattern        | ConcurrentDictionary + SemaphoreSlim for complex ops
```

Always benchmark with realistic concurrency levels before choosing. The "just use ConcurrentDictionary for everything thread-safe" approach is common and usually acceptable, but it adds overhead that matters at extreme throughput.

---

## Q13. How do you implement a simple cache with ConcurrentDictionary with expiry?

**Concepts**
- ConcurrentDictionary as the cache store
- Cache entry with value + expiry time
- GetOrAdd with Lazy<T> for thread-safe initialization
- Background cleanup of expired entries
- CancellationToken for cleanup task shutdown

**Answer**

```csharp
public class ExpiringCache<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private record CacheEntry(TValue Value, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<TKey, CacheEntry> _store = new();
    private readonly TimeSpan _ttl;
    private readonly Timer _cleanupTimer;

    public ExpiringCache(TimeSpan ttl)
    {
        _ttl = ttl;
        // Periodic cleanup of expired entries
        _cleanupTimer = new Timer(Cleanup, null, ttl, ttl);
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        var entry = _store.GetOrAdd(key, k =>
            new CacheEntry(factory(k), DateTimeOffset.UtcNow.Add(_ttl)));

        // Refresh on access if expired:
        if (entry.ExpiresAt < DateTimeOffset.UtcNow)
        {
            var fresh = new CacheEntry(factory(key), DateTimeOffset.UtcNow.Add(_ttl));
            _store[key] = fresh; // overwrite expired entry
            return fresh.Value;
        }
        return entry.Value;
    }

    private void Cleanup(object? _)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var key in _store.Keys)
            if (_store.TryGetValue(key, out var entry) && entry.ExpiresAt < now)
                _store.TryRemove(key, out _);
    }

    public void Dispose() => _cleanupTimer.Dispose();
}
```

The `GetOrAdd` factory can run multiple times (the `ConcurrentDictionary` race), which is acceptable for idempotent factories (database reads, HTTP fetches). For expensive factories, wrap in `Lazy<T>` as described earlier. The cleanup timer runs periodically to prevent unbounded memory growth from expired entries.

---

## Q14. How does Channel<T> support multiple producers and multiple consumers?

**Concepts**
- BoundedChannelOptions.AllowSynchronousContinuations
- SingleReader/SingleWriter optimization hints
- Multiple ReadAllAsync consumers with async foreach
- Task.WhenAll to await multiple consumers
- Channel.Writer.Complete() signals all consumers to stop

**Answer**

`Channel<T>` natively supports multiple producers and consumers. `BoundedChannelOptions` has `SingleReader` and `SingleWriter` flags that enable internal optimizations when you know only one reader or writer exists — these are hints for performance, not enforcement.

```csharp
var channel = Channel.CreateBounded<Work>(new BoundedChannelOptions(200)
{
    SingleReader = false,  // multiple consumers
    SingleWriter = false   // multiple producers
});

// Multiple producers:
var producers = Enumerable.Range(0, 3).Select(id => Task.Run(async () =>
{
    foreach (var item in GetWorkForProducer(id))
        await channel.Writer.WriteAsync(item);
}));

// Signal completion after all producers finish:
var producersDone = Task.WhenAll(producers).ContinueWith(_ =>
    channel.Writer.Complete());

// Multiple consumers:
var consumers = Enumerable.Range(0, 4).Select(_ => Task.Run(async () =>
{
    await foreach (var item in channel.Reader.ReadAllAsync())
        await ProcessAsync(item);
}));

await Task.WhenAll(consumers.Concat(new[] { producersDone }));
```

`channel.Writer.Complete()` should be called only once (by the last or sole coordinator), after all producers are done. All consumer `ReadAllAsync()` loops terminate naturally once the channel is marked complete and empty. The channel internally handles the thread-safe handoff between any producer and any consumer.

---

## Gotchas & Traps

---

## Q15. What is the check-then-act race condition with ConcurrentDictionary?

**Concepts**
- ContainsKey + Add = two separate non-atomic operations
- Another thread can insert between check and add
- TryAdd() as atomic alternative
- GetOrAdd() for get-or-create pattern
- AddOrUpdate() for atomic conditional update

**Answer**

```csharp
// RACE CONDITION: check-then-act
var dict = new ConcurrentDictionary<string, int>();

// Thread A and Thread B both execute simultaneously:
if (!dict.ContainsKey("counter")) // Both see false
{
    dict["counter"] = 0; // Both set to 0 — Thread B overwrites Thread A's init
}
dict["counter"]++; // Both increment independently — lost update

// CORRECT: use atomic TryAdd
dict.TryAdd("counter", 0); // Only one thread succeeds; other gets false

// Or use AddOrUpdate for the combined add-or-increment:
dict.AddOrUpdate("counter", 1, (key, existing) => existing + 1);
```

| Category | Problem | Impact |
|---|---|---|
| Race Condition | ContainsKey and Add are two separate operations | Another thread modifies between them |
| Lost Update | Both threads initialize to 0; subsequent increments lose one update | Counter is permanently off by one (or more) |
| Subtle Failure | Works correctly in single-threaded tests | Only fails under actual concurrent load |

**Fix priority:**
1. Replace `ContainsKey` + `[key] = value` with `TryAdd(key, value)`.
2. Replace `ContainsKey` + `[key] = value` + increment with `AddOrUpdate`.
3. Replace `TryGetValue` + `[key] = value` (when not found) with `GetOrAdd`.

---

## Q16. What is the ConcurrentBag contention problem when used as a cross-thread queue?

**Concepts**
- ConcurrentBag optimized for same-thread add-and-take
- Cross-thread take = work-stealing (slower, higher contention)
- Producer-consumer across threads defeats ConcurrentBag's design
- ConcurrentQueue for cross-thread producer-consumer
- Channel<T> for async producer-consumer

**Answer**

`ConcurrentBag<T>` uses per-thread local lists. When a thread adds items and later takes them, it operates on its own list — extremely fast. When a thread takes items but they were added by a different thread, it must "steal" from that thread's list, which requires acquiring a lock on the other thread's list.

```csharp
// WRONG: using ConcurrentBag as a cross-thread producer-consumer queue
var bag = new ConcurrentBag<Work>();

// Producer thread adds work
var producer = Task.Run(() => {
    foreach (var item in GenerateWork())
        bag.Add(item); // adds to producer's local list
});

// Different consumer thread tries to take — must steal every time
var consumer = Task.Run(() => {
    while (!bag.IsEmpty)
        if (bag.TryTake(out var item)) // work-stealing on every take — high contention
            Process(item);
});
```

For the producer-consumer pattern across different threads, use `ConcurrentQueue<T>` (no work-stealing, purpose-built FIFO) or `Channel<T>` (async-first, backpressure). `ConcurrentBag` is correct for the parallel aggregation pattern (many threads producing, one thread draining) or object pooling.

---

## Q17. What is the BlockingCollection dispose/cancel race?

**Concepts**
- Disposing while threads are blocked in Take/Add throws ObjectDisposedException
- Cancellation should precede Dispose
- TryAdd/TryTake with timeout as safer alternative to blocking indefinitely
- CancellationToken overloads on all blocking methods
- Graceful shutdown sequence: signal, wait, dispose

**Answer**

```csharp
// BUG: disposing BlockingCollection while consumer thread is blocked in Take
var queue = new BlockingCollection<Work>(10);
var cts = new CancellationTokenSource();

var consumer = Task.Run(() =>
{
    try
    {
        foreach (var item in queue.GetConsumingEnumerable(cts.Token))
            Process(item);
    }
    catch (OperationCanceledException) { /* expected on shutdown */ }
});

// DANGEROUS: dispose without cancellation first
queue.Dispose(); // throws ObjectDisposedException in consumer thread
```

| Category | Problem | Impact |
|---|---|---|
| ObjectDisposedException | Consumer blocked in Take() receives ObjectDisposedException on Dispose | Unexpected exception propagation |
| Race Condition | Dispose races with consumer — outcome depends on timing | Non-deterministic crash |
| Resource Leak | If exception is not caught, consumer task faults silently | No guarantee of graceful shutdown |

**Fix priority:**
1. Cancel first: `cts.Cancel(); await consumer; queue.Dispose();`
2. Use `GetConsumingEnumerable(cancellationToken)` to enable clean cancellation.
3. Complete adding rather than disposing for controlled shutdown: `queue.CompleteAdding()` is safer than `Dispose()` for stopping consumers.

---

## Q18. Why does ConcurrentDictionary.Count perform a full scan?

**Concepts**
- Count requires summing counts across all lock stripes
- Not O(1) — O(segments) which is proportional to processor count
- IsEmpty is O(1) and preferred for empty check
- Count is approximate under concurrent modification
- ContainsKey is O(1) and thread-safe

**Answer**

`ConcurrentDictionary.Count` requires taking all internal stripe locks to get a consistent snapshot of the total count, then summing counts across all segments. On a machine with 16 cores and 32 stripes, this acquires 32 locks sequentially. It is not an O(1) operation and is not a point-in-time snapshot — by the time all stripes are counted, earlier stripes may have changed.

```csharp
var dict = new ConcurrentDictionary<int, string>();

// AVOID in hot paths: takes all locks
int count = dict.Count; // O(segments)

// PREFER for empty check: O(1)
bool empty = dict.IsEmpty;

// PREFER for existence check: O(1)
bool exists = dict.ContainsKey(key);

// PREFER for value check: O(1)
bool found = dict.TryGetValue(key, out var value);
```

If you need to frequently check the count for rate limiting or capacity decisions, maintain a separate `Interlocked`-based counter that you increment/decrement alongside dictionary operations. This gives O(1) count at the cost of keeping the counter in sync. Never call `Count` in a hot loop — it serializes all dictionary segments.

---

## Q19. What happens when you enumerate a ConcurrentQueue while it is being modified?

**Concepts**
- Snapshot enumeration: safe but reflects state at snapshot time
- Items enqueued after snapshot started are not included
- Items dequeued during enumeration may still appear in snapshot
- Not a live view — use for diagnostics only
- TryDequeue in loop for actual processing

**Answer**

`ConcurrentQueue<T>` enumerator creates an internal snapshot when enumeration begins. Items added after the snapshot was taken are not included. Items dequeued during enumeration remain in the snapshot — they are logically removed from the queue but the enumeration still yields them.

```csharp
var queue = new ConcurrentQueue<int>();
for (int i = 1; i <= 5; i++) queue.Enqueue(i);

// Snapshot taken here (contains 1,2,3,4,5):
foreach (var item in queue)
{
    // Another thread dequeues item 3 here — snapshot still yields 3
    // Another thread enqueues 6 here — snapshot does NOT yield 6
    Console.Write(item + " ");
}
// Prints: 1 2 3 4 5 — snapshot, not live view
```

The practical rule: use enumeration only for diagnostics (logging queue contents, monitoring queue depth). For actual item processing — especially drain-all patterns — always use `TryDequeue` in a loop, which reflects the true current state of the queue with each call.

---

## Real-World Scenarios

---

## Q20. Implement a thread-safe cache with ConcurrentDictionary and Lazy<T> for expensive initialization.

**Concepts**
- GetOrAdd with Lazy<T> value
- Lazy<T> guarantees single factory execution per key
- Multiple Lazy instances created but only one stored
- Lazy<T> initialized on first .Value access
- Handles concurrent cache miss scenario correctly

**Answer**

```csharp
public class LazyCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _store = new();

    public TValue GetOrCreate(TKey key, Func<TKey, TValue> factory)
    {
        // Multiple Lazy<T> objects may be created under concurrency,
        // but only one is stored; the winner's factory runs exactly once.
        var lazy = _store.GetOrAdd(key,
            k => new Lazy<TValue>(() => factory(k),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value; // factory runs exactly once on the winning Lazy<T>
    }

    public bool TryRemove(TKey key) => _store.TryRemove(key, out _);

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (_store.TryGetValue(key, out var lazy))
        {
            value = lazy.Value;
            return true;
        }
        value = default;
        return false;
    }
}
```

`LazyThreadSafetyMode.ExecutionAndPublication` ensures that if two threads race to call `.Value` on the same `Lazy<T>`, one blocks until the other completes the factory, and both get the same result. The small cost: if the `GetOrAdd` factory races (creates two `Lazy<T>` objects), the losing `Lazy<T>` is GC'd without ever running its factory — this is safe and correct.

---

## Q21. Build a multi-producer single-consumer pipeline with Channel<T>.

**Concepts**
- Channel.CreateBounded for backpressure
- Multiple producers using Writer.WriteAsync concurrently
- Single consumer using ReadAllAsync
- Writer.Complete() when all producers finish
- Task.WhenAll for coordinating producer completion

**Answer**

```csharp
public async Task RunPipelineAsync(
    IEnumerable<IDataSource> sources,
    IDataProcessor processor,
    CancellationToken ct)
{
    var channel = Channel.CreateBounded<DataItem>(new BoundedChannelOptions(500)
    {
        SingleReader = true,   // optimization: known single consumer
        SingleWriter = false,  // multiple producers
        FullMode = BoundedChannelFullMode.Wait
    });

    // Multiple producers — each writes to the shared channel
    var producerTasks = sources.Select(source => Task.Run(async () =>
    {
        await foreach (var item in source.ReadItemsAsync(ct))
            await channel.Writer.WriteAsync(item, ct);
    }, ct)).ToList();

    // Signal completion when all producers finish (or on cancellation)
    var allProducersDone = Task.WhenAll(producerTasks).ContinueWith(
        t => channel.Writer.Complete(t.Exception),
        TaskContinuationOptions.ExecuteSynchronously);

    // Single consumer — processes in order items are written
    await foreach (var item in channel.Reader.ReadAllAsync(ct))
        await processor.ProcessAsync(item, ct);

    // Ensure producers' exceptions are observed
    await allProducersDone;
}
```

`FullMode = Wait` creates natural backpressure — producers wait asynchronously (releasing their threads) when the channel is full. `channel.Writer.Complete(exception)` propagates producer exceptions to the consumer's `ReadAllAsync` (it terminates the reader with the exception). `SingleReader = true` enables a faster internal implementation without the multi-reader synchronization overhead.

---

## Q22. Replace a lock+Queue with BlockingCollection for bounded producer-consumer.

**Concepts**
- Old pattern: lock + Queue + ManualResetEvent
- BlockingCollection replaces all three
- Bounded capacity for memory control
- GetConsumingEnumerable for clean consumer loop
- Exception safety with finally for CompleteAdding

**Answer**

```csharp
// BEFORE: manual lock + Queue + event (verbose and error-prone)
public class OldQueue<T>
{
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly object _lock = new object();
    private readonly ManualResetEvent _hasItems = new ManualResetEvent(false);
    private bool _completed = false;

    public void Enqueue(T item)
    {
        lock (_lock) { _queue.Enqueue(item); _hasItems.Set(); }
    }

    public void Complete()
    {
        lock (_lock) { _completed = true; _hasItems.Set(); }
    }

    public IEnumerable<T> Consume()
    {
        while (true)
        {
            _hasItems.WaitOne();
            lock (_lock)
            {
                while (_queue.Count > 0) yield return _queue.Dequeue();
                if (_completed) yield break;
                _hasItems.Reset();
            }
        }
    }
}

// AFTER: BlockingCollection (simpler, bounded, correct)
public class NewQueue<T>
{
    private readonly BlockingCollection<T> _collection;

    public NewQueue(int capacity) =>
        _collection = new BlockingCollection<T>(capacity);

    public void Enqueue(T item) => _collection.Add(item);

    public void Complete() => _collection.CompleteAdding();

    public IEnumerable<T> Consume() =>
        _collection.GetConsumingEnumerable();

    public void Dispose() => _collection.Dispose();
}
```

The `BlockingCollection`-based version is ~15 lines vs ~35 for the manual approach, handles exception safety in `CompleteAdding` naturally, supports bounded capacity, and does not have the `_hasItems` reset race condition that the manual version is susceptible to.

---

## Q23. Diagnose a hang in a BlockingCollection-based pipeline.

**Concepts**
- Consumer blocked in GetConsumingEnumerable — CompleteAdding not called
- Producer blocked in Add — consumer died or queue is full
- Thread dump: identify blocked threads and what they wait on
- dotnet-dump + clrstack to see blocking location
- Fix: ensure CompleteAdding in finally; handle consumer exceptions

**Answer**

A `BlockingCollection`-based pipeline hangs silently. The application appears to stop but does not exit.

**Step 1: Identify blocked threads using dotnet-dump:**
```
dotnet-dump collect -p <pid>
dotnet-dump analyze <dump>
> clrthreads        # find threads in WaitSleepJoin state
> setthread <id>
> clrstack
```

**Scenario A: Consumer thread blocked in `GetConsumingEnumerable`:**
Stack shows `BlockingCollection.TryTakeWithNoTimeValidation` → means `CompleteAdding()` was never called. Check producer code for exception paths that skip `CompleteAdding()`.

Fix:
```csharp
try { foreach (var item in source) queue.Add(item); }
finally { queue.CompleteAdding(); } // always called
```

**Scenario B: Producer thread blocked in `Add`:**
Stack shows `BlockingCollection.TryAddWithNoTimeValidation` → queue is full and consumer has stopped (or is too slow). Check consumer for exceptions that exit the loop early without draining.

Fix: use `TryAdd(item, timeout, ct)` with a cancellation token so the producer can detect shutdown:
```csharp
if (!queue.TryAdd(item, millisecondsTimeout: 5000, ct))
    throw new TimeoutException("Queue backpressure timeout");
```

**Scenario C: Both blocked on each other:** rarely seen with `BlockingCollection`, but possible if producer and consumer share another lock.

---

## Q24. Implement a work-stealing job queue using ConcurrentStack.

**Concepts**
- Per-worker thread local ConcurrentStack
- Steal from other workers when local stack empty
- LIFO local access for cache locality
- FIFO-like stealing from others' bottoms (TryPopRange)
- Load balancing without centralized coordination

**Answer**

```csharp
public class WorkStealingQueue<T>
{
    private readonly ConcurrentStack<T>[] _queues;
    private readonly int _workerCount;

    public WorkStealingQueue(int workerCount)
    {
        _workerCount = workerCount;
        _queues = Enumerable.Range(0, workerCount)
            .Select(_ => new ConcurrentStack<T>())
            .ToArray();
    }

    public void Enqueue(int workerId, T item) =>
        _queues[workerId].Push(item);

    public bool TryDequeue(int workerId, out T? item)
    {
        // Try own queue first (LIFO — cache-friendly)
        if (_queues[workerId].TryPop(out item))
            return true;

        // Steal from another worker's queue
        for (int i = 1; i < _workerCount; i++)
        {
            int victimId = (workerId + i) % _workerCount;
            if (_queues[victimId].TryPop(out item))
                return true;
        }

        return false;
    }

    public void Run(int workerId, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (TryDequeue(workerId, out var item))
                Process(workerId, item!);
            else
                Thread.Yield(); // no work — yield briefly before retry
        }
    }

    private static void Process(int workerId, T item) =>
        Console.WriteLine($"Worker {workerId} processed {item}");
}
```

Each worker processes its own stack (LIFO for cache locality — recently enqueued work is likely related to what was just processed). When idle, it steals from another worker's stack. The stealing pattern distributes load without a centralized coordinator or lock. This is the same principle used by the .NET ThreadPool internally. `Thread.Yield()` when idle avoids busy-waiting without the latency of `Thread.Sleep`.

---

## Q25. How do you choose the right concurrent collection for a log aggregation service?

**Concepts**
- Multiple producers (log writers) — thread safety required
- Single or multiple consumers (flush/batch)
- Order requirements (FIFO for log sequence)
- Bounded capacity to prevent memory exhaustion
- Async consumption preferred in modern services

**Answer**

A log aggregation service has multiple producer threads writing log entries and one or more consumer threads batching entries for writing to a sink (file, database, monitoring service). The requirements: thread-safe multi-producer writes, FIFO ordering for correct log sequence, bounded capacity to limit memory under high load, and async-friendly consumption.

**Recommended: `Channel<T>` with BoundedChannel**

```csharp
public class LogAggregator : IAsyncDisposable
{
    private readonly Channel<LogEntry> _channel;
    private readonly Task _consumerTask;
    private readonly CancellationTokenSource _cts = new();

    public LogAggregator(int capacity = 10_000)
    {
        _channel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest, // or Wait for backpressure
            SingleReader = true
        });
        _consumerTask = ConsumeAsync(_cts.Token);
    }

    public bool TryLog(LogEntry entry) =>
        _channel.Writer.TryWrite(entry); // non-blocking; returns false if full

    public async ValueTask LogAsync(LogEntry entry, CancellationToken ct = default) =>
        await _channel.Writer.WriteAsync(entry, ct); // backpressure if full

    private async Task ConsumeAsync(CancellationToken ct)
    {
        var batch = new List<LogEntry>(100);
        await foreach (var entry in _channel.Reader.ReadAllAsync(ct))
        {
            batch.Add(entry);
            if (batch.Count >= 100 || _channel.Reader.Count == 0)
            {
                await FlushBatchAsync(batch, ct);
                batch.Clear();
            }
        }
        if (batch.Count > 0) await FlushBatchAsync(batch, default);
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        await _consumerTask;
        _cts.Dispose();
    }
}
```

`Channel<T>` is chosen over `BlockingCollection<T>` because the consumer uses `await foreach` (non-blocking, no thread held during waits), `FullMode.DropOldest` provides graceful degradation under load (drop old logs rather than stalling producers), and `SingleReader = true` optimizes the internal implementation. The `DisposeAsync` pattern ensures all buffered entries are flushed before the service shuts down.
