/*
 * =============================================================================
 * 02. THREAD POOL — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: The CLR thread pool — queue short work items on shared worker threads
 *        via ThreadPool.QueueUserWorkItem and WaitCallback instead of creating
 *        a new System.Threading.Thread for every job.
 *
 * WHY IT MATTERS:
 *   Server and desktop apps often receive bursts of small tasks: validate rows,
 *   resize thumbnails, write audit log entries. Spawning hundreds of manual
 *   threads per second wastes memory and CPU on thread startup/teardown. The
 *   pool reuses a bounded set of worker threads and scales them when the queue
 *   grows. Most BCL async APIs (Timers, ASP.NET, Task.Run) sit on this pool.
 *
 * WHAT YOU WILL LEARN:
 *   1.  WaitCallback and ThreadPool.QueueUserWorkItem (with state parameter)
 *   2.  Pool worker threads vs manually created Thread for the same batch
 *   3.  IsThreadPoolThread — identifying which thread runs your callback
 *   4.  Waiting for queued work — CountdownEvent, ManualResetEvent, WaitAll
 *   5.  GetMinThreads / GetMaxThreads / GetAvailableThreads and SetMinThreads tuning
 *   6.  I/O completion ports preview — why the pool tracks I/O threads separately
 *   7.  When to use ThreadPool vs manual Thread vs Task (forward refs)
 *   8.  Measuring batch throughput with Stopwatch
 *
 * =============================================================================
 */

using System;
using System.Diagnostics;
using System.Threading;

namespace ThreadPool;

/*
 * =========================================================================
 * SECTION 1: BATCH JOB SCENARIO — INDEPENDENT INVOICE LINE VALIDATION
 * =========================================================================
 *
 * An overnight import produced many invoice lines. Each line must be validated
 * (checksum-style CPU work simulated below). Jobs are independent and roughly
 * equal length — ideal for parallel workers on the thread pool.
 *
 * Two strategies compared in this chapter:
 *
 *   A) Manual Thread — new Thread(...).Start() per line, Join each one
 *   B) Thread pool  — ThreadPool.QueueUserWorkItem per line
 *
 * ch.01 Threads and Thread Lifecycle covers Thread lifecycle, Join, and
 * foreground/background in depth. Here we focus on throughput for batches.
 * -------------------------------------------------------------------------
 */
public readonly record struct InvoiceLineJob(int LineId, int WorkUnits);

/*
 * =========================================================================
 * SECTION 2: WaitCallback — THE DELEGATE ThreadPool.QueueUserWorkItem USES
 * =========================================================================
 *
 * WaitCallback is a built-in delegate in System.Threading:
 *
 *   public delegate void WaitCallback(object? state);
 *
 * ThreadPool.QueueUserWorkItem overloads:
 *
 *   bool QueueUserWorkItem(WaitCallback callBack)
 *   bool QueueUserWorkItem(WaitCallback callBack, object? state)
 *
 * Returns true if the item was queued (false only when the pool is shutting
 * down). A pool worker picks up the callback when available — you do NOT get
 * a dedicated thread per call.
 *
 * Optional state: pass one object; cast inside the callback. Common patterns:
 *   • Boxed value types (int) — prefer a small context record/class instead
 *   • InvoiceLineJob or a tuple carrying job id + shared result arrays
 *   • A synchronization primitive (ManualResetEvent) passed as state
 *
 * ch.03 Tasks and Task Parallel Library wraps the same pool with Task.Run;
 * QueueUserWorkItem is the lower-level API you still see in legacy code.
 * -------------------------------------------------------------------------
 */
public static class InvoiceValidation
{
    /*
     * Deterministic pseudo-checksum work. ~90% of lines pass (LineId % 10 != 0)
     * so output is not all zeros. Each iteration is cheap but repeats enough
     * times that thread creation cost matters when BatchSize is large.
     */
    public static int ValidateLine(InvoiceLineJob job)
    {
        uint hash = (uint)job.LineId;
        for (int n = 0; n < job.WorkUnits; n++)
        {
            hash = hash * 1_103_515_245u + 12_345u;
        }

        return job.LineId % 10 == 0 ? 0 : 1;
    }

    public static int CountValidResults(int[] results)
    {
        int count = 0;
        foreach (int flag in results)
        {
            if (flag == 1)
            {
                count++;
            }
        }

        return count;
    }
}

public class Program
{
    private const int BatchSize = 120;
    private const int WorkUnitsPerJob = 8_000;

    /*
     * =========================================================================
     * SECTION 3: MANUAL Thread BATCH — ONE OS THREAD PER JOB
     * =========================================================================
     *
     * Pattern from ch.01:
     *
     *   var t = new Thread(() => DoWork(id));
     *   t.Start();
     *   t.Join();
     *
     * Creating a Thread allocates a ~1 MB default stack (Windows), registers
     * with the OS scheduler, and tears all of that down when the thread ends.
     * Fine for a handful of long-lived workers; expensive when job count is
     * large and each job is milliseconds of work.
     * -------------------------------------------------------------------------
     */
    private static int RunManualThreadBatch(int jobCount, int workUnits)
    {
        var results = new int[jobCount];
        var threads = new Thread[jobCount];

        for (int i = 0; i < jobCount; i++)
        {
            int jobId = i;
            threads[i] = new Thread(() =>
            {
                results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, workUnits));
            });
            threads[i].Start();
        }

        for (int i = 0; i < jobCount; i++)
        {
            threads[i].Join();
        }

        return InvoiceValidation.CountValidResults(results);
    }

    /*
     * =========================================================================
     * SECTION 4: ThreadPool.QueueUserWorkItem — COUNTDOWNEVENT WAIT
     * =========================================================================
     *
     * QueueUserWorkItem returns immediately; the callback runs later on a pool
     * worker. The caller must synchronize before reading results or exiting Main.
     *
     * CountdownEvent (also a WaitHandle) is the clean pattern for large batches:
     *   • Initialize with the job count
     *   • Each callback calls Signal() in a finally block
     *   • Main thread calls Wait() once
     *
     * ch.06 Synchronization and Locks covers lock/Monitor when multiple threads
     * mutate the same object; here each job writes its own results[i] slot.
     * -------------------------------------------------------------------------
     */
    private static int RunThreadPoolBatch(int jobCount, int workUnits)
    {
        var results = new int[jobCount];
        using var done = new CountdownEvent(jobCount);

        for (int i = 0; i < jobCount; i++)
        {
            int jobId = i;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, workUnits));
                }
                finally
                {
                    done.Signal();
                }
            });
        }

        done.Wait();
        return InvoiceValidation.CountValidResults(results);
    }

    /*
     * =========================================================================
     * SECTION 5: WAITING WITH ManualResetEvent + WaitHandle.WaitAll
     * =========================================================================
     *
     * ManualResetEvent starts unsignaled (false). The callback calls Set() when
     * finished. WaitAll blocks until every event is signaled.
     *
     * Pitfall: WaitHandle.WaitAll is limited to 64 handles on some runtimes when
     * the calling thread is STA. For larger batches prefer CountdownEvent (SECTION 4).
     *
     * This helper demonstrates the per-job event pattern on a small batch only.
     * -------------------------------------------------------------------------
     */
    private static int RunThreadPoolWithManualResetEvents(int jobCount, int workUnits)
    {
        var results = new int[jobCount];
        var doneEvents = new ManualResetEvent[jobCount];

        for (int i = 0; i < jobCount; i++)
        {
            doneEvents[i] = new ManualResetEvent(initialState: false);
            int jobId = i;
            ManualResetEvent signal = doneEvents[i];

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, workUnits));
                }
                finally
                {
                    signal.Set();
                }
            });
        }

        WaitHandle.WaitAll(doneEvents);

        foreach (ManualResetEvent handle in doneEvents)
        {
            handle.Dispose();
        }

        return InvoiceValidation.CountValidResults(results);
    }

    /*
     * =========================================================================
     * SECTION 6: QUEUE WITH STATE PARAMETER — WaitCallback(object? state)
     * =========================================================================
     *
     * The second QueueUserWorkItem overload passes state into the callback.
     * Cast once at the top of the callback; avoid capturing many closure variables
     * when a single context object is clearer.
     * -------------------------------------------------------------------------
     */
    private sealed class LineValidationContext
    {
        public required int JobId { get; init; }
        public required int WorkUnits { get; init; }
        public required int[] Results { get; init; }
        public required CountdownEvent Done { get; init; }
    }

    private static int RunThreadPoolWithStateParameter(int jobCount, int workUnits)
    {
        var results = new int[jobCount];
        using var done = new CountdownEvent(jobCount);

        for (int i = 0; i < jobCount; i++)
        {
            var context = new LineValidationContext
            {
                JobId = i,
                WorkUnits = workUnits,
                Results = results,
                Done = done,
            };

            System.Threading.ThreadPool.QueueUserWorkItem(ValidationCallbackWithState, context);
        }

        done.Wait();
        return InvoiceValidation.CountValidResults(results);
    }

    private static void ValidationCallbackWithState(object? state)
    {
        var ctx = (LineValidationContext)state!; // state is always our context object here
        try
        {
            ctx.Results[ctx.JobId] = InvoiceValidation.ValidateLine(
                new InvoiceLineJob(ctx.JobId, ctx.WorkUnits));
        }
        finally
        {
            ctx.Done.Signal();
        }
    }

    /*
     * =========================================================================
     * SECTION 7: POOL THREAD IDENTITY — IsThreadPoolThread
     * =========================================================================
     *
     * Thread.CurrentThread.IsThreadPoolThread is true on callbacks queued through
     * the pool (and on threads started by Task.Run — ch.03). Manual Thread
     * instances return false unless you explicitly queue work onto the pool.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateIsThreadPoolThread()
    {
        Console.WriteLine("--- IsThreadPoolThread ---");
        Console.WriteLine($"  Main thread:              {Thread.CurrentThread.IsThreadPoolThread}");

        var manualDone = new ManualResetEvent(false);
        var manualThread = new Thread(() =>
        {
            Console.WriteLine($"  Manual Thread worker:     {Thread.CurrentThread.IsThreadPoolThread}");
            manualDone.Set();
        });
        manualThread.Start();
        manualDone.WaitOne();
        manualDone.Dispose();

        var poolDone = new ManualResetEvent(false);
        System.Threading.ThreadPool.QueueUserWorkItem(_ =>
        {
            Console.WriteLine($"  QueueUserWorkItem worker: {Thread.CurrentThread.IsThreadPoolThread}");
            poolDone.Set();
        });
        poolDone.WaitOne();
        poolDone.Dispose();
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 8: MIN / MAX / AVAILABLE THREADS — INSPECT AND TUNE THE POOL
     * =========================================================================
     *
     * The CLR maintains two logical pools inside one facility:
     *
     *   Worker threads  — CPU-bound callbacks (QueueUserWorkItem, Task.Run)
     *   I/O threads     — completion ports for async I/O (see SECTION 9 preview)
     *
     * Query APIs (always pass out parameters for both counts):
     *
     *   GetMinThreads(out int workerMin, out int ioMin)
     *   GetMaxThreads(out int workerMax, out int ioMax)
     *   GetAvailableThreads(out int workerFree, out int ioFree)
     *
     * SetMinThreads(workerMin, ioMin) — ask the runtime to keep at least this
     * many threads ready (reduces ramp-up latency after idle periods). Returns
     * false if the request exceeds machine limits. Use sparingly — each thread
     * consumes memory even when idle.
     *
     * SetMaxThreads(workerMax, ioMax) — cap pool growth (rare; can cause queue
     * backlog under heavy load). Prefer fixing slow callbacks over starving the pool.
     * -------------------------------------------------------------------------
     */
    private static void PrintThreadPoolLimits(string label)
    {
        System.Threading.ThreadPool.GetMinThreads(out int minWorkers, out int minIo);
        System.Threading.ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIo);
        System.Threading.ThreadPool.GetAvailableThreads(out int availWorkers, out int availIo);

        int busyWorkers = maxWorkers - availWorkers;
        int busyIo = maxIo - availIo;

        Console.WriteLine($"--- Thread pool limits ({label}) ---");
        Console.WriteLine($"  Min worker / I/O:         {minWorkers,4} / {minIo}");
        Console.WriteLine($"  Max worker / I/O:         {maxWorkers,4} / {maxIo}");
        Console.WriteLine($"  Available worker / I/O:   {availWorkers,4} / {availIo}");
        Console.WriteLine($"  Busy (max − available):   {busyWorkers,4} / {busyIo}");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 9: I/O COMPLETION PORTS — PREVIEW (I/O THREAD COUNT)
     * =========================================================================
     *
     * Windows I/O completion ports (IOCP) let the OS notify the process when
     * disk/network operations finish without blocking a worker thread for the
     * entire wait. The CLR thread pool is built on IOCP; the "I/O thread" count
     * from GetMaxThreads reflects threads reserved for dispatching those completions.
     *
     * ch.04 Async and Await builds on this infrastructure — async File/Network
     * methods complete on pool threads via IOCP under the hood. You rarely call
     * IOCP APIs directly in application code; know that I/O threads and worker
     * threads share one coordinated pool manager.
     * -------------------------------------------------------------------------
     */
    private static void PreviewIoCompletionPorts()
    {
        System.Threading.ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIo);
        Console.WriteLine("--- I/O completion ports preview ---");
        Console.WriteLine($"  Pool reports {maxIo} max I/O threads alongside {maxWorkers} workers.");
        Console.WriteLine("  Async I/O (ch.04) completes via IOCP — not a separate manual Thread per read.");
        Console.WriteLine("  COVERED IN DETAIL LATER → 04. Async and Await");
        Console.WriteLine();
    }

    /*
     * =========================================================================
     * SECTION 10: WHEN TO USE ThreadPool VS MANUAL Thread VS Task
     * =========================================================================
     *
     *  Concern              | Manual Thread              | Thread pool
     *  ---------------------|----------------------------|---------------------------
     *  Creation cost        | High per thread            | Amortized — workers reused
     *  Typical use          | Long-lived dedicated work  | Many short callbacks
     *  Thread count         | You create N threads       | CLR caps/grows pool size
     *  Stack memory         | ~1 MB each (default)       | Shared worker set
     *  API surface          | Thread, ParameterizedStart | QueueUserWorkItem (+ Tasks)
     *
     * When manual Thread still makes sense:
     *   • One or few threads with a custom name/priority running for minutes
     *   • Foreground thread that must keep the process alive (ch.01)
     *   • Isolated stack size or apartment state requirements
     *
     * Modern code usually prefers Task.Run (ch.03) which also uses the pool —
     * QueueUserWorkItem remains useful for fire-and-forget callbacks and when
     * reading legacy BCL code. ch.05 Parallel Programming adds Parallel.For for
     * data-parallel loops; ch.07 Concurrent Collections covers thread-safe bags.
     * -------------------------------------------------------------------------
     */
    private static void PrintWhenToUseGuidance()
    {
        Console.WriteLine("--- When to use which API ---");
        Console.WriteLine("  Manual Thread     → few/long-lived, custom priority, foreground lifetime");
        Console.WriteLine("  ThreadPool        → many short callbacks, timers, legacy queue patterns");
        Console.WriteLine("  Task.Run (ch.03)  → modern wrapper; still uses pool threads by default");
        Console.WriteLine("  async/await (ch.04) → I/O-bound work; frees workers during waits");
        Console.WriteLine();
    }

    private static long TimeBatchRun(Func<int> batchRunner)
    {
        var stopwatch = Stopwatch.StartNew();
        _ = batchRunner();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    /*
     * =========================================================================
     * SECTION 11: DEMONSTRATION — Main ORCHESTRATES THE CHAPTER DEMO
     * =========================================================================
     */
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Invoice line batch validation (ThreadPool chapter) ===");
        Console.WriteLine($"Jobs: {BatchSize}  |  Work units per job: {WorkUnitsPerJob:N0}");
        Console.WriteLine();

        PrintThreadPoolLimits("before batch work");
        DemonstrateIsThreadPoolThread();
        PreviewIoCompletionPorts();

        int manualValid = RunManualThreadBatch(BatchSize, WorkUnitsPerJob);
        Console.WriteLine($"Manual Thread batch — valid lines: {manualValid}/{BatchSize}");

        int poolValid = RunThreadPoolBatch(BatchSize, WorkUnitsPerJob);
        Console.WriteLine($"ThreadPool batch     — valid lines: {poolValid}/{BatchSize}");

        const int stateBatch = 16;
        int stateValid = RunThreadPoolWithStateParameter(stateBatch, WorkUnitsPerJob);
        Console.WriteLine($"State-parameter demo ({stateBatch} jobs) — valid: {stateValid}/{stateBatch}");

        const int waitAllBatch = 12;
        int waitAllValid = RunThreadPoolWithManualResetEvents(waitAllBatch, WorkUnitsPerJob);
        Console.WriteLine(
            $"WaitAll demo ({waitAllBatch} jobs, ManualResetEvent[]) — valid: {waitAllValid}/{waitAllBatch}");
        Console.WriteLine();

        PrintWhenToUseGuidance();

        /*
         * Stopwatch uses a high-resolution timer — good for comparing approaches
         * on the same machine. Prefer Release builds; warm up once (JIT, pool
         * growth); repeat runs and compare trends, not a single sample.
         */
        const int perfBatchSize = 200;
        RunThreadPoolBatch(10, WorkUnitsPerJob); // warm-up: JIT helpers and grow pool

        long manualMs = TimeBatchRun(() => RunManualThreadBatch(perfBatchSize, WorkUnitsPerJob));
        long poolMs = TimeBatchRun(() => RunThreadPoolBatch(perfBatchSize, WorkUnitsPerJob));

        Console.WriteLine("=== Stopwatch comparison (Release build recommended) ===");
        Console.WriteLine($"Batch size: {perfBatchSize} jobs");
        Console.WriteLine($"Manual Thread total: {manualMs,6} ms");
        Console.WriteLine($"Thread pool total:   {poolMs,6} ms");
        if (poolMs > 0)
        {
            double ratio = (double)manualMs / poolMs;
            Console.WriteLine($"Pool vs manual ratio: {ratio:F2}x (values > 1 ⇒ pool faster here)");
        }

        PrintThreadPoolLimits("after batch work");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — THREAD POOL
 * =========================================================================
 *
 * --- Queue work ---
 *
 *   System.Threading.ThreadPool.QueueUserWorkItem(callback);
 *   System.Threading.ThreadPool.QueueUserWorkItem(callback, state);
 *
 *   delegate void WaitCallback(object? state);
 *
 * --- Identify pool threads ---
 *
 *   Thread.CurrentThread.IsThreadPoolThread   // true inside pool callbacks
 *
 * --- Wait for queued items to finish ---
 *
 *   // Many jobs — prefer CountdownEvent
 *   using var gate = new CountdownEvent(jobCount);
 *   ThreadPool.QueueUserWorkItem(_ => { try { … } finally { gate.Signal(); } });
 *   gate.Wait();
 *
 *   // One event per job (small batches; WaitAll ≤ 64 handles on some STA paths)
 *   var ev = new ManualResetEvent(false);
 *   ThreadPool.QueueUserWorkItem(_ => { …; ev.Set(); });
 *   ev.WaitOne();
 *
 * --- Inspect / tune pool ---
 *
 *   ThreadPool.GetMinThreads(out int wMin, out int ioMin);
 *   ThreadPool.GetMaxThreads(out int wMax, out int ioMax);
 *   ThreadPool.GetAvailableThreads(out int wFree, out int ioFree);
 *   ThreadPool.SetMinThreads(workerMin, ioMin);   // reduce cold-start latency — use sparingly
 *   ThreadPool.SetMaxThreads(workerMax, ioMax);   // cap growth — rarely needed
 *
 * --- Manual Thread vs pool ---
 *
 *   Manual Thread     few/long-lived, custom priority, foreground lifetime
 *   Thread pool       many short callbacks, server request handling, timers
 *   Task.Run (ch.03)  modern wrapper; still uses pool threads by default
 *   async/await (ch.04) I/O-bound; IOCP dispatches completions to I/O threads
 *
 * --- Performance testing ---
 *
 *   var sw = Stopwatch.StartNew();
 *   RunWorkload();
 *   sw.Stop();
 *   Console.WriteLine(sw.ElapsedMilliseconds);
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  Exit Main before callbacks finish    | Process ends; work cut off
 *  Unbounded manual Thread per request  | Thread explosion, OOM, thrashing
 *  Assuming callback order              | Pool order is undefined
 *  Ignoring exceptions in callbacks     | Unobserved on pool thread — log/handle
 *  SetMinThreads too high everywhere    | Idle threads waste RAM on every app
 *
 * --- Related chapters ---
 *
 *   01. Threads and Thread Lifecycle   Thread, Start, Join, background
 *   03. Tasks and Task Parallel Library Task.Run, Task.WaitAll
 *   04. Async and Await                async I/O, IOCP under the hood
 *   05. Parallel Programming           Parallel.For, partitioners
 *   06. Synchronization and Locks      lock, Monitor for shared state
 *   07. Concurrent Collections         thread-safe bags and dictionaries
 *
 * =========================================================================
 */
