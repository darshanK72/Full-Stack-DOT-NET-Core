/*
 * =============================================================================
 * 07. CONCURRENT COLLECTIONS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Thread-safe collections in System.Collections.Concurrent — share
 *        mutable state across threads without wrapping List<T> or Dictionary
 *        in lock on every operation.
 *
 * WHY IT MATTERS:
 *   Warehouse systems, web caches, and log pipelines all have multiple threads
 *   reading and writing shared data. A plain List<T> or Dictionary<K,V> is not
 *   safe for concurrent mutation — you get torn state or InvalidOperationException.
 *   Wrapping every Add/Remove in lock works but serializes all access. Concurrent
 *   collections use finer-grained locking or lock-free algorithms so producers and
 *   consumers wait less. This chapter complements ch.06 (lock, Monitor, Semaphore)
 *   and ch.05 (Parallel.For, Task-based pipelines).
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why plain collections fail under concurrent mutation
 *   2.  ConcurrentDictionary — thread-safe cache (GetOrAdd, AddOrUpdate, TryUpdate)
 *   3.  ConcurrentQueue and ConcurrentStack — FIFO and LIFO without external lock
 *   4.  ConcurrentBag — unordered results from parallel workers
 *   5.  BlockingCollection — bounded producer/consumer buffer
 *   6.  IProducerConsumerCollection — plug a different backing store
 *   7.  When concurrent collections beat lock + List, and when they do not
 *
 * =============================================================================
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentCollections;

/*
 * =========================================================================
 * SECTION 1: WHY PLAIN COLLECTIONS ARE NOT THREAD-SAFE
 * =========================================================================
 *
 * List<T>.Add, Dictionary<K,V> indexer assignment, and even Count are not
 * atomic as a whole operation. Two threads can interleave:
 *
 *   Thread A: read Count=5, write at index 5
 *   Thread B: read Count=5, write at index 5   ← duplicate slot or exception
 *
 * Options when sharing mutable collections:
 *
 *   lock (gate) { list.Add(x); }     serializes every operation — simple, slow
 *   Concurrent* collection             thread-safe per-operation API
 *   Immutable snapshot + replace       readers see stable copy; writers rebuild
 *
 * Concurrent collections make individual Add/Remove/Lookup safe. They do NOT
 * make multi-step sequences safe (read then write based on value). For compound
 * invariants use AddOrUpdate, TryUpdate, or lock — covered in ch.06.
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: ConcurrentDictionary<TKey, TValue> — THREAD-SAFE CACHE
 * =========================================================================
 *
 * Thread-safe counterpart to Dictionary<TKey, TValue>. Different keys can update
 * in parallel (fine-grained locking per bucket), so throughput stays higher
 * than one global lock around Dictionary.
 *
 * Key members:
 *
 *   GetOrAdd(key, factory)              add if missing; return existing or new
 *   GetOrAdd(key, value)                add if missing with fixed value
 *   TryAdd(key, value)                  add only when key absent (bool success)
 *   TryGetValue(key, out val)           read without throwing
 *   TryRemove(key, out val)             remove one entry
 *   TryUpdate(key, new, comparison)     CAS-style update when value matches
 *   AddOrUpdate(key, add, update)       insert or transform existing atomically
 *   ContainsKey / Count / Keys / Values snapshot helpers
 *
 * Pitfalls:
 *   • GetOrAdd factory may run MORE THAN ONCE for the same key under contention
 *     — factory must be idempotent or side-effect free.
 *   • Indexer dict[key] = value is thread-safe for a single write but NOT for
 *     read-modify-write: if (TryGetValue) { dict[k] = v + 1; } races.
 *   • foreach while others mutate — enumerator sees a point-in-time snapshot but
 *     may not include concurrent adds; copy ToArray() if you need a stable report.
 * -------------------------------------------------------------------------
 */
internal static class SkuPriceCacheDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 2: ConcurrentDictionary — SKU price cache ---");

        ConcurrentDictionary<string, decimal> cache = new ConcurrentDictionary<string, decimal>(
            concurrencyLevel: Environment.ProcessorCount,
            capacity: 16,
            comparer: StringComparer.OrdinalIgnoreCase);

        string[] skuBatch = { "SKU-100", "SKU-200", "SKU-100", "SKU-300", "SKU-200" };

        Parallel.ForEach(skuBatch, sku =>
        {
            decimal price = cache.GetOrAdd(sku, LookupPriceFromDatabase); // factory on miss only
            Console.WriteLine($"  [cache] {sku} => {price:F2} (thread {Environment.CurrentManagedThreadId})");
        });

        Console.WriteLine($"  Cache entries: {cache.Count}");
        PrintSnapshot(cache);

        // TryAdd — fails when key already present
        bool addedNew = cache.TryAdd("SKU-400", 4.99m);
        bool duplicate = cache.TryAdd("SKU-100", 0m);
        Console.WriteLine($"  TryAdd SKU-400: {addedNew}, TryAdd duplicate SKU-100: {duplicate}");

        // AddOrUpdate — atomic insert or transform (safe surcharge)
        cache.AddOrUpdate(
            "SKU-100",
            _ => LookupPriceFromDatabase("SKU-100") + 1.50m,
            (_, current) => current + 1.50m);

        if (cache.TryGetValue("SKU-100", out decimal updated))
        {
            Console.WriteLine($"  After surcharge, SKU-100 = {updated:F2}");
        }

        // TryUpdate — succeeds only when current value matches comparisonValue
        if (cache.TryGetValue("SKU-200", out decimal sku200))
        {
            bool updated200 = cache.TryUpdate("SKU-200", sku200 + 0.25m, sku200);
            Console.WriteLine($"  TryUpdate SKU-200 (+0.25): {updated200}");
        }

        if (cache.TryRemove("SKU-300", out decimal removed))
        {
            Console.WriteLine($"  TryRemove SKU-300 => {removed:F2}");
        }

        Console.WriteLine();
    }

    private static decimal LookupPriceFromDatabase(string sku)
    {
        Thread.Sleep(10); // simulates I/O on cache miss
        return sku switch
        {
            "SKU-100" => 12.99m,
            "SKU-200" => 8.45m,
            "SKU-300" => 19.50m,
            _ => 0.00m,
        };
    }

    private static void PrintSnapshot(ConcurrentDictionary<string, decimal> cache)
    {
        Console.WriteLine("  Cache snapshot (sorted for display):");
        foreach (KeyValuePair<string, decimal> entry in cache.OrderBy(e => e.Key))
        {
            Console.WriteLine($"    {entry.Key} => {entry.Value:F2}");
        }
    }
}

/*
 * =========================================================================
 * SECTION 3: ConcurrentQueue<T> — FIFO WORK QUEUE
 * =========================================================================
 *
 * First-in, first-out — ideal for inbound orders, log lines, or task hand-off.
 *
 *   Enqueue(item)           add at tail
 *   TryDequeue(out item)    remove from head (false when empty)
 *   TryPeek(out item)       read head without removing
 *   IsEmpty / Count         approximate under heavy contention
 *   ToArray()               snapshot copy for safe iteration
 *
 * Multiple producers and consumers can Enqueue/TryDequeue concurrently without
 * an external lock. Order is preserved per producer/consumer pair, not globally
 * across all threads when you foreach the live collection during mutation.
 * -------------------------------------------------------------------------
 */
internal static class InboundQueueDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 3: ConcurrentQueue — inbound orders (FIFO) ---");

        ConcurrentQueue<string> inbound = new ConcurrentQueue<string>();
        inbound.Enqueue("PO-9001: 40x SKU-100");
        inbound.Enqueue("PO-9002: 12x SKU-300");
        inbound.Enqueue("PO-9003: 8x SKU-200");

        if (inbound.TryPeek(out string? peeked))
        {
            Console.WriteLine($"  Peek head (still queued): {peeked}");
        }

        while (inbound.TryDequeue(out string? order))
        {
            Console.WriteLine($"  Dequeued: {order}");
        }

        Console.WriteLine($"  Queue empty: {inbound.IsEmpty}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 4: ConcurrentStack<T> — LIFO UNDO / RECENT HISTORY
 * =========================================================================
 *
 * Last-in, first-out — undo stacks, depth-first traversal buffers, recent alerts.
 *
 *   Push(item)              add at top
 *   TryPop(out item)        remove top (false when empty)
 *   TryPeek(out item)       read top without removing
 *   PushRange / TryPopRange bulk operations for batches
 * -------------------------------------------------------------------------
 */
internal static class UndoStackDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 4: ConcurrentStack — undo history (LIFO) ---");

        ConcurrentStack<string> undo = new ConcurrentStack<string>();
        undo.Push("Assigned aisle A-12 to SKU-100");
        undo.Push("Marked PO-9002 received");
        undo.Push("Adjusted SKU-300 count -2");

        string[] batchPop = new string[2];
        int popped = undo.TryPopRange(batchPop); // bulk pop into buffer
        Console.WriteLine($"  TryPopRange took {popped} items:");
        for (int i = 0; i < popped; i++)
        {
            Console.WriteLine($"    {batchPop[i]}");
        }

        while (undo.TryPop(out string? action))
        {
            Console.WriteLine($"  Undo: {action}");
        }

        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 5: ConcurrentBag<T> — UNORDERED PARALLEL AGGREGATION
 * =========================================================================
 *
 * Stores items with no guaranteed global order. Internally keeps thread-local
 * lists per producer thread — Add from thread T and TryTake on thread T is very
 * fast (typical Parallel.For aggregation pattern).
 *
 *   Add(item)               thread-local insert
 *   TryTake(out item)       remove any item (prefers local thread's list)
 *   TryPeek(out item)       inspect without removing
 *
 * Good for: collecting per-partition scan notes, error messages, metrics.
 * Poor for: FIFO fairness, indexed access, ordered reporting.
 *
 * To display: copy to array and sort — bag order is undefined.
 * -------------------------------------------------------------------------
 */
internal static class ScanBagDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 5: ConcurrentBag — parallel shelf scans ---");

        ConcurrentBag<string> scanNotes = new ConcurrentBag<string>();

        Parallel.For(0, 8, aisleIndex =>
        {
            string note = $"Aisle {aisleIndex + 1}: {Random.Shared.Next(1, 6)} picks";
            scanNotes.Add(note);
            Console.WriteLine($"  Added '{note}' on thread {Environment.CurrentManagedThreadId}");
        });

        string[] sorted = scanNotes.OrderBy(n => n).ToArray(); // sort for readable output
        Console.WriteLine($"  Total notes: {scanNotes.Count} (sorted display: {sorted.Length})");

        int drained = 0;
        while (scanNotes.TryTake(out string? taken))
        {
            drained++;
        }

        Console.WriteLine($"  Drained via TryTake: {drained}, bag empty: {scanNotes.IsEmpty}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 6: BlockingCollection<T> — BOUNDED PRODUCER / CONSUMER
 * =========================================================================
 *
 * Wraps any IProducerConsumerCollection<T> (default: ConcurrentQueue) and adds:
 *
 *   bounded capacity        Add blocks when full; Take blocks when empty
 *   CompleteAdding()        signal producers finished — consumers eventually end
 *   GetConsumingEnumerable  foreach-friendly drain (used below)
 *   TryAdd / TryTake        non-blocking or timed variants
 *   IsAddingCompleted       true after CompleteAdding (may still hold items)
 *   IsCompleted             true when adding done AND collection empty
 *
 * Classic pattern: producers outrun consumers → buffer absorbs bursts; when full,
 * producers block (back-pressure) instead of allocating unbounded memory.
 *
 * CancellationToken overloads exist on Add/Take — preview only; full async
 * producer/consumer patterns are in ch.04 Async and Await.
 * -------------------------------------------------------------------------
 */
internal static class PickBufferDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 6: BlockingCollection — bounded pick buffer ---");

        const int bufferCapacity = 3;
        using BlockingCollection<string> pickBuffer = new BlockingCollection<string>(bufferCapacity);

        Task producer = Task.Run(() =>
        {
            for (int box = 1; box <= 6; box++)
            {
                string label = $"Box-{box:D3}";
                pickBuffer.Add(label); // blocks when buffer at capacity
                Console.WriteLine($"  [producer] Added {label} (count={pickBuffer.Count})");
                Thread.Sleep(30);
            }

            pickBuffer.CompleteAdding();
            Console.WriteLine("  [producer] CompleteAdding — no more boxes");
        });

        Task consumer = Task.Run(() =>
        {
            foreach (string box in pickBuffer.GetConsumingEnumerable())
            {
                Console.WriteLine($"  [consumer] Picked {box} on thread {Environment.CurrentManagedThreadId}");
                Thread.Sleep(50);
            }

            Console.WriteLine("  [consumer] Buffer drained");
        });

        Task.WaitAll(producer, consumer);
        Console.WriteLine($"  IsCompleted: {pickBuffer.IsCompleted}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 7: IProducerConsumerCollection — CUSTOM BACKING STORE
 * =========================================================================
 *
 * BlockingCollection constructor accepts any IProducerConsumerCollection<T>:
 *
 *   ConcurrentQueue<T>   FIFO (default when you pass only a capacity int)
 *   ConcurrentStack<T>   LIFO buffer
 *   ConcurrentBag<T>     unordered (unusual for BlockingCollection but valid)
 *
 * Same Add/Take blocking semantics; only storage order changes.
 * -------------------------------------------------------------------------
 */
internal static class StackBackedBufferDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 7: BlockingCollection backed by ConcurrentStack ---");

        IProducerConsumerCollection<int> backing = new ConcurrentStack<int>();
        using BlockingCollection<int> stackBuffer = new BlockingCollection<int>(backing, boundedCapacity: 2);

        stackBuffer.Add(10);
        stackBuffer.Add(20);
        stackBuffer.CompleteAdding();

        int first = stackBuffer.Take(); // LIFO — 20 removed first
        int second = stackBuffer.Take();
        Console.WriteLine($"  Stack-backed Take order: {first}, then {second} (LIFO)");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 8: CONCURRENT COLLECTIONS VS lock + PLAIN COLLECTIONS
 * =========================================================================
 *
 * | Scenario                              | Prefer                    |
 * |---------------------------------------|---------------------------|
 * | Many threads add/remove independently | Concurrent* collection    |
 * | Shared key/value cache                | ConcurrentDictionary      |
 * | Producer/consumer with back-pressure  | BlockingCollection        |
 * | Unordered parallel aggregation        | ConcurrentBag             |
 * | Compound multi-step invariant         | lock + plain collection   |
 * | Need consistent snapshot of whole list| Copy under lock, then read|
 * | Single-threaded code                  | List / Dictionary         |
 *
 * ch.06 Synchronization and Locks owns lock, Monitor, Semaphore, and deadlock
 * prevention in depth. Use lock when one critical section spans multiple
 * collections or non-collection state that must change together.
 *
 * Anti-pattern (still races despite ConcurrentDictionary):
 *
 *   if (dict.TryGetValue(k, out var v)) { dict[k] = v + 1; }
 *
 * Fix: AddOrUpdate, TryUpdate, or lock around the read-modify-write sequence.
 * -------------------------------------------------------------------------
 */
internal static class LockVsConcurrentDemo
{
    public static void Run()
    {
        Console.WriteLine("--- SECTION 8: lock + List vs ConcurrentBag ---");

        List<string> lockedList = new List<string>();
        object gate = new object();
        ConcurrentBag<string> bag = new ConcurrentBag<string>();

        Parallel.For(0, 20, i =>
        {
            string ticket = $"T-{i:D2}";
            lock (gate)
            {
                lockedList.Add(ticket); // every Add serializes on gate
            }

            bag.Add(ticket); // no external lock — per-thread local fast path
        });

        Console.WriteLine($"  lock + List count:   {lockedList.Count}");
        Console.WriteLine($"  ConcurrentBag count: {bag.Count}");
        Console.WriteLine("  Both reach 20; ConcurrentBag avoids serializing every Add.");
        Console.WriteLine();
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 9: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Each demo class above owns one collection family. Main prints a header
     * and calls them in reading order — scroll up to the section comment above
     * each type for the full API tables and pitfalls.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Warehouse concurrent collections demo ===");
        Console.WriteLine();

        SkuPriceCacheDemo.Run();
        InboundQueueDemo.Run();
        UndoStackDemo.Run();
        ScanBagDemo.Run();
        PickBufferDemo.Run();
        StackBackedBufferDemo.Run();
        LockVsConcurrentDemo.Run();

        Console.WriteLine("=== Demo complete ===");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — CONCURRENT COLLECTIONS
 * =========================================================================
 *
 * --- Namespace ---
 *
 *   using System.Collections.Concurrent;
 *
 * --- ConcurrentDictionary<TKey, TValue> ---
 *
 *   var cache = new ConcurrentDictionary<string, decimal>();
 *   cache.GetOrAdd(key, k => Load(k));           // factory may run more than once
 *   cache.TryAdd(key, value);                    // false if key exists
 *   cache.TryGetValue(key, out var v);
 *   cache.AddOrUpdate(key, addVal, (k, old) => old + 1);
 *   cache.TryUpdate(key, newVal, comparisonVal);
 *   cache.TryRemove(key, out var removed);
 *
 * --- ConcurrentQueue<T> (FIFO) ---
 *
 *   queue.Enqueue(item);
 *   queue.TryDequeue(out var item);
 *   queue.TryPeek(out var item);
 *
 * --- ConcurrentStack<T> (LIFO) ---
 *
 *   stack.Push(item);
 *   stack.TryPop(out var item);
 *   stack.TryPopRange(array, 0, array.Length);
 *
 * --- ConcurrentBag<T> (unordered) ---
 *
 *   bag.Add(item);
 *   bag.TryTake(out var item);   // fast when taker == original adder thread
 *
 * --- BlockingCollection<T> ---
 *
 *   using var buffer = new BlockingCollection<T>(boundedCapacity: 10);
 *   buffer.Add(item);                    // blocks when full
 *   buffer.Take();                       // blocks when empty
 *   buffer.CompleteAdding();             // producers finished
 *   foreach (var x in buffer.GetConsumingEnumerable()) { … }
 *
 *   // custom backing:
 *   var bc = new BlockingCollection<T>(new ConcurrentStack<T>(), capacity: 5);
 *
 * --- vs lock + List<T> ---
 *
 *   Prefer Concurrent*     many independent add/remove/lookup operations
 *   Prefer lock + List     multi-step invariants, ordered snapshot, single thread
 *   Either + copy          report/display needs stable point-in-time view
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Read-modify-write without AddOrUpdate| Lost updates under contention
 *  foreach while others mutate          | Undefined / incomplete enumeration
 *  ConcurrentBag for fair FIFO          | Wrong tool — use ConcurrentQueue
 *  Assuming Count is exact under load     | May be approximate during heavy ops
 *  Side effects inside GetOrAdd factory   | Factory may run multiple times
 *
 * --- Related chapters ---
 *
 *   05. Parallel Programming          Parallel.For, partition-local work
 *   06. Synchronization and Locks     lock, Monitor, Semaphore when collections
 *                                     are not enough for compound operations
 *   03. Tasks and Task Parallel Library  Task.Run producer/consumer pairing
 *   04. Async and Await               async producer/consumer with await
 *
 * =========================================================================
 */
