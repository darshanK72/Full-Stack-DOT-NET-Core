/*
 * =============================================================================
 * 05. PARALLEL PROGRAMMING — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Data-parallel CPU work with the Task Parallel Library — Parallel.For,
 *        Parallel.ForEach, Parallel.Invoke, custom partitioners, ParallelOptions,
 *        and PLINQ (parallel LINQ). Focus is splitting in-memory computation
 *        across cores, not waiting on I/O without blocking (async/await, ch.04).
 *
 * WHY IT MATTERS:
 *   Modern CPUs have many cores, but a single thread uses one at a time. Parallel
 *   loops partition index ranges or collections and schedule chunks on the thread
 *   pool (ch.02). Used correctly on CPU-heavy batch work, throughput scales with
 *   cores. Used incorrectly — tiny data, heavy locks, or I/O waits — parallel code
 *   is slower and harder to debug than a simple loop.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Parallel.For — index-range loops and thread-local aggregation
 *   2.  Parallel.ForEach — parallel iteration over collections
 *   3.  Parallel.Invoke — a fixed set of concurrent delegates
 *   4.  ParallelOptions — MaxDegreeOfParallelism and CancellationToken
 *   5.  ParallelLoopState — Break() and Stop() inside a loop body
 *   6.  Partitioners — control how work is split across workers
 *   7.  Race conditions in parallel loops — preview of unsafe shared updates
 *   8.  Interlocked vs lock — preview → ch.06 Synchronization and Locks
 *   9.  PLINQ — AsParallel, degree, ordering, cancellation
 *  10.  When parallel helps vs hurts — decision guide with examples
 *  11.  Multithreading vs async vs parallel — full comparison (this chapter)
 *
 * =============================================================================
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelProgramming;

/*
 * =========================================================================
 * SECTION 1: SAMPLE DATA — WAREHOUSE SKU RECONCILIATION BATCH
 * =========================================================================
 *
 * Each StockRecord holds on-hand quantity and a per-unit adjustment factor.
 * Reconciliation multiplies them — CPU-bound math we can parallelize across
 * the batch. Later sections reuse this list so every demo shares one scenario.
 * -------------------------------------------------------------------------
 */
public sealed class StockRecord
{
    public StockRecord(string sku, int onHand, decimal adjustmentFactor)
    {
        Sku = sku;
        OnHand = onHand;
        AdjustmentFactor = adjustmentFactor;
    }

    public string Sku { get; }
    public int OnHand { get; }
    public decimal AdjustmentFactor { get; }

    public decimal ReconciledValue => OnHand * AdjustmentFactor; // per-line CPU work
}

/*
 * =========================================================================
 * SECTION 8: INTERLOCKED VS lock — DECIMAL MERGE HELPER
 * =========================================================================
 *
 * Interlocked.Increment/Add work on int and long only. Merging decimal totals
 * from thread-local Parallel.For accumulators needs either lock or a concurrent
 * structure. This helper uses lock for a single shared decimal — see ch.06 for
 * Monitor, Mutex, and Semaphore depth.
 * -------------------------------------------------------------------------
 */
file static class ThreadSafeDecimal
{
    private static readonly object Gate = new object();

    public static void Add(ref decimal target, decimal value)
    {
        lock (Gate)
        {
            target += value;
        }
    }
}

public class Program
{
    /*
     * =========================================================================
     * SECTION 2: Parallel.For — INDEX-BASED PARALLEL LOOP
     * =========================================================================
     *
     * Parallel.For(fromInclusive, toExclusive, body) splits the index range
     * [from, to) across thread-pool workers. Each iteration receives:
     *
     *   int index                 current index
     *   ParallelLoopState state   Break(), Stop(), IsExceptional
     *
     * Returns ParallelLoopResult:
     *   .IsCompleted             all iterations finished normally
     *   .LowestBreakIteration    index where Break() was called (if any)
     *
     * --- 2a. Simple body — write into pre-sized array (no sharing) ---
     *
     * --- 2b. Thread-local aggregation overload ---
     *
     * Avoid locking on every iteration. Four-parameter overload:
     *
     *   Parallel.For(from, to,
     *       () => localInit,                    // once per worker thread
     *       (i, state, local) => updateLocal,   // per iteration
     *       local => mergeLocalIntoShared);     // once per worker thread
     * -------------------------------------------------------------------------
     */
    private static decimal DemoParallelFor(List<StockRecord> batch)
    {
        decimal[] reconciled = new decimal[batch.Count];

        Parallel.For(0, batch.Count, index =>
        {
            reconciled[index] = batch[index].ReconciledValue; // each index writes its own slot
        });

        decimal serialTotal = 0m;
        for (int i = 0; i < batch.Count; i++)
        {
            serialTotal += reconciled[i];
        }

        decimal parallelLocalTotal = 0m;
        Parallel.For(
            0,
            batch.Count,
            () => 0m,
            (index, loopState, localSum) => localSum + batch[index].ReconciledValue,
            localSum => ThreadSafeDecimal.Add(ref parallelLocalTotal, localSum));

        Console.WriteLine("--- Parallel.For ---");
        Console.WriteLine($"  Reconciled line count: {reconciled.Length}");
        Console.WriteLine($"  Sum via thread-local Parallel.For: {parallelLocalTotal:N2}");
        Console.WriteLine($"  Matches serial sum: {parallelLocalTotal == serialTotal}");
        Console.WriteLine();

        return serialTotal;
    }

    /*
     * =========================================================================
     * SECTION 3: Parallel.ForEach — COLLECTION PARALLEL LOOP
     * =========================================================================
     *
     * Parallel.ForEach(source, body) is the collection equivalent of For.
     * Prefer it when you already have IEnumerable<T> and do not need the index.
     *
     * Body: Action<TSource> or Action<TSource, ParallelLoopState>
     *
     * Shared List<T> is not thread-safe — use lock, ConcurrentBag (ch.07), or
     * collect per-thread then merge (like thread-local For).
     * -------------------------------------------------------------------------
     */
    private static void DemoParallelForEach(List<StockRecord> batch)
    {
        List<string> auditLines = new List<string>();
        object auditGate = new object();

        Parallel.ForEach(batch, record =>
        {
            string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor} = {record.ReconciledValue:N2}";

            lock (auditGate) // List<T>.Add is not atomic — one writer at a time
            {
                auditLines.Add(line);
            }
        });

        Console.WriteLine("--- Parallel.ForEach (audit trail, lock for List<T>) ---");
        foreach (string line in auditLines.OrderBy(l => l, StringComparer.Ordinal))
        {
            Console.WriteLine($"  {line}");
        }
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 4: Parallel.Invoke — FIXED CONCURRENT DELEGATES
     * =========================================================================
     *
     * Parallel.Invoke(Action, Action, …) runs each delegate on the pool and
     * blocks until ALL complete. Use when you have a small, fixed number of
     * independent CPU tasks — not a loop over thousands of items.
     *
     * Overloads accept Action[] or params Action[].
     * -------------------------------------------------------------------------
     */
    private static void DemoParallelInvoke(List<StockRecord> batch, decimal batchTotal)
    {
        decimal zoneNorth = 0m;
        decimal zoneSouth = 0m;
        decimal zoneEast = 0m;

        Parallel.Invoke(
            () => zoneNorth = SumZone(batch, sku => sku[6] <= '4'),
            () => zoneSouth = SumZone(batch, sku => sku[6] >= '5' && sku[6] <= '7'),
            () => zoneEast = SumZone(batch, sku => sku[6] >= '8'));

        Console.WriteLine("--- Parallel.Invoke (three zone totals concurrently) ---");
        Console.WriteLine($"  North (001-004): {zoneNorth:N2}");
        Console.WriteLine($"  South (005-007): {zoneSouth:N2}");
        Console.WriteLine($"  East  (008-012): {zoneEast:N2}");
        Console.WriteLine($"  Batch total:     {batchTotal:N2}");
        Console.WriteLine();
    }

    private static decimal SumZone(List<StockRecord> records, Func<string, bool> skuFilter)
    {
        decimal total = 0m;
        foreach (StockRecord record in records)
        {
            if (skuFilter(record.Sku))
            {
                total += record.ReconciledValue;
            }
        }

        return total;
    }

    /*
     * =========================================================================
     * SECTION 5: ParallelOptions — MaxDegreeOfParallelism
     * =========================================================================
     *
     * ParallelOptions.MaxDegreeOfParallelism limits concurrent workers.
     *
     *   -1 (default)  no explicit cap — scheduler decides (often ~ core count)
     *    1            effectively serial execution through the parallel API
     *    N > 1        cap at N concurrent workers
     *
     * Throttle when leaving a core free for other processes, respecting license
     * limits, or reducing memory from per-thread buffers.
     * -------------------------------------------------------------------------
     */
    private static void DemoMaxDegreeOfParallelism()
    {
        int concurrentWorkers = 0;
        object workerGate = new object();

        ParallelOptions cappedOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2,
        };

        Parallel.For(0, 24, cappedOptions, i =>
        {
            lock (workerGate)
            {
                concurrentWorkers = Math.Max(concurrentWorkers, Thread.CurrentThread.ManagedThreadId);
            }

            Thread.SpinWait(50_000); // simulate short CPU work per iteration
        });

        Console.WriteLine("--- MaxDegreeOfParallelism = 2 ---");
        Console.WriteLine("  TPL enforces at most 2 concurrent workers for this loop.");
        Console.WriteLine($"  Sample worker thread id observed: {concurrentWorkers}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 6: CANCEL PARALLEL OPERATIONS
     * =========================================================================
     *
     * Two mechanisms:
     *
     *   1. ParallelOptions.CancellationToken
     *      External cancel (timeout, user action). TPL throws
     *      OperationCanceledException when the token is signaled.
     *
     *   2. ParallelLoopState.Break() / Stop()
     *      In-loop control. Break() stops iterations with index > break index
     *      (lower indexes still run). Stop() tries to stop all not-yet-started
     *      iterations as soon as possible.
     *
     * --- 6a. CancellationToken ---
     * --- 6b. ParallelLoopState.Break() ---
     * --- 6c. ParallelLoopState.Stop() ---
     * -------------------------------------------------------------------------
     */
    private static void DemoCancellation(List<StockRecord> batch)
    {
        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(5));

        ParallelOptions cancelOptions = new ParallelOptions
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = 4,
        };

        int tokenCancelIterations = 0;
        try
        {
            Parallel.For(0, 100_000, cancelOptions, i =>
            {
                Interlocked.Increment(ref tokenCancelIterations);
                Thread.SpinWait(500);
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("--- CancellationToken on Parallel.For ---");
            Console.WriteLine($"  Loop canceled after ~{tokenCancelIterations} iterations.");
        }

        const decimal breakThreshold = 600m;
        decimal runningBreakTotal = 0m;
        object breakGate = new object();

        ParallelLoopResult breakResult = Parallel.For(0, batch.Count, (index, loopState) =>
        {
            decimal lineValue = batch[index].ReconciledValue;

            lock (breakGate)
            {
                runningBreakTotal += lineValue;
                if (runningBreakTotal >= breakThreshold)
                {
                    loopState.Break(); // stop iterations with index > current
                }
            }
        });

        Console.WriteLine();
        Console.WriteLine("--- ParallelLoopState.Break() ---");
        Console.WriteLine($"  Threshold: {breakThreshold:N2}");
        Console.WriteLine($"  Break at index: {breakResult.LowestBreakIteration}");
        Console.WriteLine($"  IsCompleted (all iterations): {breakResult.IsCompleted}");
        Console.WriteLine($"  Running total at break: {runningBreakTotal:N2}");

        int stopIterationsStarted = 0;
        ParallelLoopResult stopResult = Parallel.For(0, batch.Count, (index, loopState) =>
        {
            Interlocked.Increment(ref stopIterationsStarted);
            if (index == 2)
            {
                loopState.Stop(); // try to cancel all not-yet-started iterations
            }
        });

        Console.WriteLine();
        Console.WriteLine("--- ParallelLoopState.Stop() ---");
        Console.WriteLine($"  Stop() at index 2 — iterations started: {stopIterationsStarted} of {batch.Count}");
        Console.WriteLine($"  IsCompleted: {stopResult.IsCompleted}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 7: PARTITIONERS — HOW WORK IS SPLIT
     * =========================================================================
     *
     * By default Parallel.For/ForEach choose chunk sizes dynamically. Custom
     * partitioners control splitting — useful when per-item cost varies or
     * you want fewer, larger chunks to reduce scheduling overhead.
     *
     * Key APIs in System.Collections.Concurrent:
     *
     *   Partitioner.Create(from, to)                    // range partitioner
     *   Partitioner.Create(from, to, rangeSize)       // fixed range chunk size
     *   Partitioner.Create(list, loadBalance)         // list partitioner
     *
     * Pass the partitioner to Parallel.ForEach instead of the raw collection:
     *
     *   Parallel.ForEach(Partitioner.Create(0, count), range => { … });
     *   Parallel.ForEach(Partitioner.Create(list, loadBalance: true), item => { … });
     *
     * loadBalance: true  — dynamic chunk stealing (better for uneven work)
     * loadBalance: false — static chunks (less overhead when work is uniform)
     * -------------------------------------------------------------------------
     */
    private static void DemoPartitioners(List<StockRecord> batch)
    {
        int fixedChunkHits = 0;
        Parallel.ForEach(
            Partitioner.Create(0, batch.Count, rangeSize: 3),
            range =>
            {
                for (int index = range.Item1; index < range.Item2; index++)
                {
                    Interlocked.Increment(ref fixedChunkHits);
                    _ = batch[index].ReconciledValue;
                }
            });

        Console.WriteLine("--- Partitioner.Create(0, count, rangeSize: 3) ---");
        Console.WriteLine($"  Index visits (should equal {batch.Count}): {fixedChunkHits}");

        decimal partitionedSum = 0m;
        Parallel.ForEach(
            Partitioner.Create(batch, loadBalance: true),
            record => ThreadSafeDecimal.Add(ref partitionedSum, record.ReconciledValue));

        Console.WriteLine();
        Console.WriteLine("--- Partitioner.Create(list, loadBalance: true) ---");
        Console.WriteLine($"  Sum via load-balanced list partitioner: {partitionedSum:N2}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: RACE CONDITIONS — UNSAFE SHARED COUNTER (PREVIEW)
     * =========================================================================
     *
     * counter++ is NOT atomic (read → add → write). Parallel iterations that
     * share one int without synchronization lose updates.
     *
     * Expect unsafeCount < expected when many parallel increments collide.
     * Full lock/Monitor treatment → 06. Synchronization and Locks.
     * -------------------------------------------------------------------------
     */
    private static void DemoRaceCondition()
    {
        const int incrementIterations = 100_000;
        int unsafeCount = 0;

        Parallel.For(0, incrementIterations, _ => { unsafeCount++; });

        Console.WriteLine("--- Race condition (unsafe counter++) ---");
        Console.WriteLine($"  Expected: {incrementIterations}");
        Console.WriteLine($"  Actual:   {unsafeCount}  (often lower — lost updates)");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 9: INTERLOCKED VS lock (PREVIEW)
     * =========================================================================
     *
     * Interlocked — hardware-backed atomic ops on a single variable:
     *   Increment, Add, Exchange, CompareExchange
     *   Fast; only simple single-location updates.
     *
     * lock — mutual exclusion for ANY critical section:
     *   lock (gate) { … multi-step invariant … List.Add … }
     *
     * Rule of thumb:
     *   One int/long counter → Interlocked
     *   List<T>.Add, multi-field invariant → lock (or concurrent collection ch.07)
     *
     * COVERED IN DETAIL LATER → 06. Synchronization and Locks
     * -------------------------------------------------------------------------
     */
    private static void DemoInterlockedVsLock()
    {
        const int incrementIterations = 100_000;
        int interlockedCount = 0;

        Parallel.For(0, incrementIterations, _ => Interlocked.Increment(ref interlockedCount));

        Console.WriteLine("--- Interlocked.Increment ---");
        Console.WriteLine($"  Count: {interlockedCount} (matches expected: {interlockedCount == incrementIterations})");

        int lockCount = 0;
        List<int> lockLog = new List<int>();
        object lockGate = new object();

        Parallel.For(0, 100, i =>
        {
            lock (lockGate)
            {
                lockCount++;
                lockLog.Add(i);
            }
        });

        Console.WriteLine();
        Console.WriteLine("--- lock (counter + List.Add in one critical section) ---");
        Console.WriteLine($"  lockCount: {lockCount}, lockLog entries: {lockLog.Count}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 10: PARALLEL LINQ (PLINQ)
     * =========================================================================
     *
     * PLINQ parallelizes LINQ-to-objects queries on in-memory sequences.
     *
     *   sequence.AsParallel()
     *           .WithDegreeOfParallelism(n)   // optional cap (like ParallelOptions)
     *           .AsOrdered()                  // preserve input order in output (slower)
     *           .WithCancellation(token)      // cooperative cancel
     *           .Where(...).Select(...).Sum()
     *
     * PREVIEW ORIGIN → 05. Language Integrated Query / 03. Ordering showed:
     *   openLines.AsParallel().OrderByDescending(line => line.LineTotal)
     *
     * PLINQ uses the same thread pool as Parallel.*. Merge options (AsOrdered)
     * add overhead — use only when order matters in the result.
     * -------------------------------------------------------------------------
     */
    private static void DemoPlinq(List<StockRecord> batch)
    {
        decimal plinqHighValueThreshold = 150m;

        decimal plinqFilteredSum = batch
            .AsParallel()
            .WithDegreeOfParallelism(4)
            .Where(r => r.ReconciledValue >= plinqHighValueThreshold)
            .Sum(r => r.ReconciledValue);

        IEnumerable<string> plinqTopSkus = batch
            .AsParallel()
            .AsOrdered()
            .OrderByDescending(r => r.ReconciledValue)
            .Take(3)
            .Select(r => r.Sku);

        using CancellationTokenSource plinqCts = new CancellationTokenSource();
        plinqCts.CancelAfter(TimeSpan.FromSeconds(30));

        int plinqCount = batch
            .AsParallel()
            .WithCancellation(plinqCts.Token)
            .Count(r => r.OnHand > 50);

        Console.WriteLine("--- PLINQ ---");
        Console.WriteLine($"  Sum where reconciled >= {plinqHighValueThreshold:N2}: {plinqFilteredSum:N2}");
        Console.WriteLine($"  Top 3 SKUs (AsOrdered merge): {string.Join(", ", plinqTopSkus)}");
        Console.WriteLine($"  Count where OnHand > 50 (WithCancellation wired): {plinqCount}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 11: WHEN PARALLEL HELPS VS HURTS
     * =========================================================================
     *
     * Parallel wins when ALL of these are roughly true:
     *   • Work is CPU-bound (not waiting on network/disk)
     *   • Enough items or heavy enough per-item work to amortize setup cost
     *   • Little contention on shared mutable state
     *
     * Parallel loses when:
     *   • Tiny collections — scheduling overhead exceeds savings
     *   • lock on every iteration — workers serialize on the gate
     *   • I/O-bound work — threads block waiting; use async/await (ch.04)
     *   • Strict ordering required without AsOrdered — merge cost dominates
     *
     * The demos below are qualitative (console hints), not micro-benchmarks.
     * -------------------------------------------------------------------------
     */
    private static void DemoWhenParallelHelpsOrHurts()
    {
        int[] tiny = { 1, 2, 3, 4, 5 };
        long tinySerial = 0;
        foreach (int n in tiny)
        {
            tinySerial += ExpensiveSquare(n);
        }

        long tinyParallel = 0;
        Parallel.ForEach(
            Partitioner.Create(tiny, loadBalance: false),
            n => Interlocked.Add(ref tinyParallel, ExpensiveSquare(n)));

        Console.WriteLine("--- When parallel helps vs hurts ---");
        Console.WriteLine($"  Tiny array (5 items): serial sum={tinySerial}, parallel sum={tinyParallel}");
        Console.WriteLine("  → Five items rarely benefit; partition + delegate cost often wins for serial.");

        int lockBoundSum = 0;
        object hotGate = new object();
        Parallel.For(0, 500, i =>
        {
            lock (hotGate)
            {
                lockBoundSum += ExpensiveSquare(i % 20);
            }
        });

        Console.WriteLine();
        Console.WriteLine($"  Lock on every iteration: sum={lockBoundSum} — workers queue at hotGate;");
        Console.WriteLine("  → Contention removes speedup; prefer thread-local merge or no sharing.");

        Console.WriteLine();
        Console.WriteLine("  I/O-bound batch (fetch, save): use async/await — see 04. Async and Await.");
        Console.WriteLine("  CPU-bound in-memory batch: Parallel.* / PLINQ — this chapter.");
        Console.WriteLine();
    }

    private static int ExpensiveSquare(int n)
    {
        int acc = 0;
        for (int i = 0; i < 200; i++)
        {
            acc += n * n;
        }

        return acc;
    }

    /*
     * =========================================================================
     * SECTION 12: MULTITHREADING VS ASYNC VS PARALLEL
     * =========================================================================
     *
     * These terms overlap in conversation but solve different problems.
     * This module places the full comparison here (see module README).
     *
     *   Mechanism          | Primary API              | Problem shape
     *   -------------------|--------------------------|----------------------------
     *   Raw multithreading | Thread, Thread.Start     | Explicit thread control (ch.01)
     *   Thread pool          | ThreadPool, Task.Run     | Many short work items (ch.02–03)
     *   Parallel / PLINQ     | Parallel.*, AsParallel   | CPU-bound data in memory (here)
     *   async / await        | async Task methods       | I/O wait without blocking (ch.04)
     *
     * PREVIEW ONLY for Thread/Task/async depth → sibling chapters listed below.
     * -------------------------------------------------------------------------
     */
    private static void PrintComparisonTable()
    {
        Console.WriteLine("=== Multithreading vs async vs parallel ===");
        Console.WriteLine();
        Console.WriteLine("  Concept      | Mechanism              | Best for              | Blocks caller?");
        Console.WriteLine("  -------------|------------------------|-----------------------|---------------");
        Console.WriteLine("  Thread       | new Thread / Start     | Low-level control     | Yes (Join)");
        Console.WriteLine("  Thread pool  | QueueUserWorkItem        | Many short tasks      | Depends");
        Console.WriteLine("  Task/TPL     | Task.Run, WhenAll        | Composable background | Often yes");
        Console.WriteLine("  Parallel/TPL | Parallel.* / PLINQ       | CPU-bound batch work  | Yes (blocks caller)");
        Console.WriteLine("  async/await  | Task + state machine     | I/O-bound waiting     | No (await yields)");
        Console.WriteLine();
        Console.WriteLine("  Use parallel when: CPU-heavy, data in memory, minimal shared writes.");
        Console.WriteLine("  Use async when: waiting on network, disk, DB — do not block threads.");
        Console.WriteLine("  See: 01 Threads, 02 ThreadPool, 03 Tasks, 04 Async and Await.");
        Console.WriteLine();
    }

    private static void PrintBatch(IEnumerable<StockRecord> batch)
    {
        foreach (StockRecord record in batch)
        {
            Console.WriteLine(
                $"  {record.Sku}  OnHand={record.OnHand,4}  Factor={record.AdjustmentFactor,5:N2}");
        }
    }

    /*
     * =========================================================================
     * SECTION 13: DEMONSTRATION — Main orchestrates the chapter demo
     * =========================================================================
     *
     * Main creates sample data, prints the batch, then runs each section in
     * reading order. Section comments live above the methods and types they
     * teach — not duplicated here.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        List<StockRecord> batch =
        [
            new StockRecord("SKU-001", 120, 1.02m),
            new StockRecord("SKU-002", 85, 0.98m),
            new StockRecord("SKU-003", 200, 1.00m),
            new StockRecord("SKU-004", 45, 1.05m),
            new StockRecord("SKU-005", 310, 0.95m),
            new StockRecord("SKU-006", 67, 1.01m),
            new StockRecord("SKU-007", 150, 1.03m),
            new StockRecord("SKU-008", 92, 0.99m),
            new StockRecord("SKU-009", 178, 1.04m),
            new StockRecord("SKU-010", 64, 0.97m),
            new StockRecord("SKU-011", 240, 1.00m),
            new StockRecord("SKU-012", 55, 1.06m),
        ];

        Console.WriteLine("=== Warehouse SKU reconciliation batch ===");
        PrintBatch(batch);
        Console.WriteLine();

        decimal batchTotal = DemoParallelFor(batch);
        DemoParallelForEach(batch);
        DemoParallelInvoke(batch, batchTotal);
        DemoMaxDegreeOfParallelism();
        DemoCancellation(batch);
        DemoPartitioners(batch);
        DemoRaceCondition();
        DemoInterlockedVsLock();
        DemoPlinq(batch);
        DemoWhenParallelHelpsOrHurts();
        PrintComparisonTable();
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — PARALLEL PROGRAMMING
 * =========================================================================
 *
 * --- Parallel.For / ForEach ---
 *
 *   Parallel.For(0, count, i => { … });
 *   Parallel.ForEach(items, item => { … });
 *
 *   ParallelLoopResult r = Parallel.For(…);
 *   r.IsCompleted, r.LowestBreakIteration
 *
 * --- Thread-local For (avoid per-iteration lock) ---
 *
 *   Parallel.For(0, n, () => 0L,
 *       (i, state, local) => local + work(i),
 *       local => Interlocked.Add(ref shared, local));
 *
 * --- Parallel.Invoke ---
 *
 *   Parallel.Invoke(() => TaskA(), () => TaskB());
 *
 * --- ParallelOptions ---
 *
 *   new ParallelOptions {
 *       MaxDegreeOfParallelism = 4,      // -1 = default (no explicit cap)
 *       CancellationToken = cts.Token
 *   }
 *
 * --- Cancel / stop ---
 *
 *   cts.Cancel()           → OperationCanceledException on cooperative check
 *   loopState.Break()      → stop iterations with index > break index
 *   loopState.Stop()       → stop all not-yet-started iterations ASAP
 *
 * --- Partitioners ---
 *
 *   Partitioner.Create(from, to)
 *   Partitioner.Create(from, to, rangeSize)
 *   Partitioner.Create(list, loadBalance: true)
 *   Parallel.ForEach(partitioner, body)
 *
 * --- Interlocked vs lock (preview) ---
 *
 *   Interlocked.Increment(ref n)     single-location atomic math
 *   lock (obj) { … }                 multi-step critical section → ch.06
 *
 * --- PLINQ ---
 *
 *   seq.AsParallel()
 *      .WithDegreeOfParallelism(4)
 *      .AsOrdered()
 *      .WithCancellation(token)
 *      .Where(…).Sum()
 *
 * --- When to use what ---
 *
 *   Parallel / PLINQ   CPU cores, in-memory batch compute
 *   async/await        I/O wait without blocking threads
 *   Raw threads        Fine control; prefer pool/Task for app code
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Shared counter++ without sync          | Race — wrong totals
 *  lock inside tight Parallel.For body   | Serialization — no speedup
 *  PLINQ on tiny sequences              | Overhead exceeds benefit
 *  async for CPU-bound loop             | Extra state machines, no core use
 *  Ignore CancellationToken             | Cancel request ignored until end
 *
 * --- Related chapters ---
 *
 *   01. Threads and Thread Lifecycle     Thread class, Join, lifecycle
 *   02. ThreadPool                       pool queue, work items
 *   03. Tasks and TPL                    Task.Run, WhenAll, continuations
 *   04. Async and Await                  I/O-bound non-blocking patterns
 *   06. Synchronization and Locks        Monitor, Mutex, Semaphore depth
 *   07. Concurrent Collections           Thread-safe bags/queues/dictionaries
 *   05. LINQ / 03. Ordering              PLINQ preview (AsParallel OrderBy)
 *
 * =========================================================================
 */
