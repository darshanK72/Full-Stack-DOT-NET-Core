/*
 * =============================================================================
 * 04. ASYNC AND AWAIT — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: async/await — write non-blocking I/O code that composes with Task,
 *        handles exceptions cooperatively, supports cancellation, retries,
 *        single-flight guards, and previews async streams and ValueTask.
 *
 * WHY IT MATTERS:
 *   Blocking a thread while waiting for network, disk, or database I/O wastes
 *   capacity. async/await lets a thread serve other work while a Task is
 *   incomplete, then resume when the operation finishes. APIs, services, and
 *   UI code rely on this model. Mixing sync blocking (.Result, .Wait) with
 *   captured synchronization contexts is a common source of deadlocks — this
 *   chapter shows the safe patterns.
 *
 * WHAT YOU WILL LEARN:
 *   1.  async/await — suspend without blocking the thread pool
 *   2.  Task-returning methods — Task vs Task<T>
 *   3.  async void pitfalls — event handlers only
 *   4.  ConfigureAwait(false) — library code and context capture
 *   5.  Exception handling in async — await unwraps; sync blocking differs
 *   6.  CancellationToken — cooperative cancellation
 *   7.  Workarounds when an API ignores cancellation
 *   8.  Retry with exponential backoff
 *   9.  Only-one pattern with SemaphoreSlim
 *  10.  Multithreading vs async — when to use which (full comparison)
 *  11.  IAsyncEnumerable / async streams preview → Advanced C# 8
 *  12.  ValueTask preview → Advanced C# 8
 *  13.  Async Main preview
 *  14.  Deadlock pitfalls — .Result / .Wait on captured contexts
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAndAwait;

/*
 * =========================================================================
 * SECTION 1: SCENARIO — REPORT EXPORT PIPELINE
 * =========================================================================
 *
 * An analytics service exports quarterly reports:
 *   fetch metadata → process rows → retry unreliable API → exclusive write slot
 *
 * All I/O is simulated with Task.Delay (no real network or disk). Every later
 * section reuses this pipeline so patterns connect to one scenario.
 * -------------------------------------------------------------------------
 */
public sealed record ReportMetadata(string ReportId, int RowCount);

/*
 * =========================================================================
 * SECTION 2: async / await KEYWORDS
 * =========================================================================
 *
 * async marks a method that can use await. The compiler rewrites it into a
 * state machine that returns a Task (or Task<T>) immediately to the caller.
 *
 * await:
 *   • If the awaited Task is incomplete, the async method returns control
 *     WITHOUT blocking the current thread for the wait.
 *   • When the Task completes, a continuation runs the rest of the method.
 *
 * Rule of thumb: await every Task you care about; don't fire-and-forget
 * unless you deliberately track background work (see Section 4).
 * -------------------------------------------------------------------------
 */
public static class ReportPipeline
{
    public static async Task<ReportMetadata> FetchReportMetadataAsync(
        string reportId,
        CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken); // simulate HTTP round-trip
        return new ReportMetadata(reportId, RowCount: 4);
    }

    /*
     * =========================================================================
     * SECTION 3: TASK-RETURNING METHODS — Task vs Task<T>
     * =========================================================================
     *
     *   Return type    | Meaning
     *   ---------------|--------------------------------------------------
     *   Task           | Async work with no return value (await for completion)
     *   Task<T>        | Async work that produces T (await gives T)
     *   async void     | Fire-and-forget; ONLY for event handlers — see §4
     *
     * async methods must return Task, Task<T>, ValueTask, ValueTask<T>, or
     * void (void = rare/event only). Name async methods with an Async suffix
     * by convention (FetchReportMetadataAsync, not FetchReportMetadata).
     * -------------------------------------------------------------------------
     */
    public static async Task ProcessReportAsync(ReportMetadata metadata, CancellationToken cancellationToken)
    {
        await Task.Delay(40, cancellationToken);
        Console.WriteLine($"  Processed report {metadata.ReportId}");
    }

    public static async Task<int> CountProcessedRowsAsync(int rowCount, CancellationToken cancellationToken)
    {
        await Task.Delay(30, cancellationToken);
        return rowCount; // Task<int> — caller receives int via await
    }

    /*
     * =========================================================================
     * SECTION 4: async void PITFALLS
     * =========================================================================
     *
     * async void is acceptable ONLY for UI/event handlers where the caller
     * cannot await (Button_Click, event handlers).
     *
     * Problems everywhere else:
     *   • Caller cannot await completion or know when work finished
     *   • Exceptions thrown inside async void crash the process via
     *     TaskScheduler.UnobservedTaskException — caller try/catch is useless
     *   • Testing and composition become unreliable
     *
     * Prefer async Task for everything except true event handlers.
     * -------------------------------------------------------------------------
     */
    public static async void LogExportStarted(string reportId)
    {
        await Task.Delay(20); // fire-and-forget — no Task returned to caller
        Console.WriteLine($"  [Log] Export started for {reportId}");
    }

    /*
     * =========================================================================
     * SECTION 5: ConfigureAwait(false)
     * =========================================================================
     *
     * By default, await captures SynchronizationContext (UI thread, ASP.NET
     * request context) and posts continuations back to it.
     *
     *   await SomeTask();                     // may marshal back to UI thread
     *   await SomeTask().ConfigureAwait(false); // continuation can run on any thread
     *
     * Use ConfigureAwait(false) in library/helper code that does not touch UI
     * or request-specific state after the await. Application code (UI event
     * handlers) usually omits it so UI updates stay on the UI thread.
     *
     * Console apps have no SynchronizationContext — ConfigureAwait has no
     * practical effect here, but the idiom matters for reusable libraries.
     * -------------------------------------------------------------------------
     */
    public static async Task<string> FetchWithLibraryPatternAsync(
        string reportId,
        CancellationToken cancellationToken)
    {
        await Task.Delay(25, cancellationToken).ConfigureAwait(false);
        return $"library-data:{reportId}";
    }

    /*
     * =========================================================================
     * SECTION 6: EXCEPTION HANDLING IN ASYNC
     * =========================================================================
     *
     * With await, exceptions behave like synchronous code:
     *
     *   try { await FailingAsync(); }
     *   catch (InvalidOperationException ex) { ... }   // works — no AggregateException
     *
     * The compiler unwraps the Task's exception when you await. You do NOT
     * need catch (AggregateException) around a single awaited call.
     *
     * --- 6a. Unobserved Task exceptions ---
     * If you start a Task and never await or .Wait() it, the exception sits on
     * the Task until observed. Always await or attach a continuation.
     *
     * --- 6b. Sync blocking (.Wait / .Result) ---
     * Blocking on a faulted Task wraps the inner exception in AggregateException.
     * Prefer await; if you must block, use .GetAwaiter().GetResult() for a
     * single exception (still risky on UI — see Section 14).
     * -------------------------------------------------------------------------
     */
    public static async Task<string> FetchWithTransientFailureAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);
        throw new InvalidOperationException("Simulated API failure");
    }

    public static async Task DemonstrateAsyncExceptionHandlingAsync(CancellationToken cancellationToken)
    {
        // --- 6a. try/catch around await — normal exception flow ---
        try
        {
            await FetchWithTransientFailureAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"  Caught via await: {ex.Message}");
        }

        // --- 6b. faulted Task left unawaited — exception stored on Task ---
        Task<string> unobserved = FetchWithTransientFailureAsync(cancellationToken);
        try
        {
            await unobserved; // observing the same Task — exception re-thrown here
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("  Re-awaiting faulted Task throws the same exception");
        }

        // --- 6c. .Wait() wraps in AggregateException ---
        Task<string> faulted = FetchWithTransientFailureAsync(cancellationToken);
        try
        {
            faulted.Wait(cancellationToken);
        }
        catch (AggregateException agg)
        {
            Console.WriteLine($"  .Wait() on faulted Task → AggregateException ({agg.InnerExceptions.Count} inner)");
        }
    }

    /*
     * =========================================================================
     * SECTION 7: CancellationToken — COOPERATIVE CANCELLATION
     * =========================================================================
     *
     * Cancellation is cooperative — nothing is force-killed. Pass
     * CancellationToken into async methods and:
     *
     *   • Call token.ThrowIfCancellationRequested() in loops
     *   • Pass token to Task.Delay(ms, token) and token-aware APIs
     *
     * CancellationTokenSource creates and owns the token; call .Cancel() or
     * .CancelAfter(TimeSpan) to signal all linked operations.
     * -------------------------------------------------------------------------
     */
    public static async Task SimulateLongRunningJobAsync(CancellationToken cancellationToken)
    {
        for (int step = 1; step <= 10; step++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(50, cancellationToken);
            Console.WriteLine($"  Long job step {step}/10");
        }
    }

    /*
     * =========================================================================
     * SECTION 8: CANCEL NON-CANCELLABLE TASK — WORKAROUNDS
     * =========================================================================
     *
     * Legacy APIs may not accept CancellationToken. Options:
     *
     *   1. Task.WhenAny(work, Task.Delay(timeout, token)) — abandon if timeout
     *      wins (original work may still run in background)
     *   2. Wrap in Task.Run and poll token (heavy-handed; prefer upgrading API)
     *   3. Prefer upgrading the API or wrapping at the service boundary
     *
     * --- 8a. WhenAny + timeout token ---
     * -------------------------------------------------------------------------
     */
    public static async Task<string> FetchLegacyWithoutTokenAsync()
    {
        await Task.Delay(200); // legacy API — no cancellation support
        return "legacy-payload";
    }

    public static async Task<string> FetchLegacyWithTimeoutAsync(CancellationToken cancellationToken)
    {
        Task<string> legacyWork = FetchLegacyWithoutTokenAsync();
        Task timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(150), cancellationToken);

        Task completed = await Task.WhenAny(legacyWork, timeoutTask);
        if (completed == legacyWork)
        {
            return await legacyWork;
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new OperationCanceledException(cancellationToken);
    }

    /*
     * =========================================================================
     * SECTION 9: RETRY PATTERN WITH BACKOFF
     * =========================================================================
     *
     * Transient failures (network blips, 503 responses) often warrant retries.
     *
     * Pattern:
     *   for attempt in 1..maxAttempts:
     *     try return await operation()
     *     catch when retryable && attempt < max: delay(backoff * attempt, token)
     *
     * Always respect CancellationToken during delay between attempts.
     * Do not retry OperationCanceledException — that is intentional cancellation.
     * -------------------------------------------------------------------------
     */
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts,
        int baseDelayMs,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
            {
                int delayMs = baseDelayMs * attempt; // linear backoff
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException("RetryAsync exhausted attempts without returning.");
    }

    public static async Task<string> UnreliableFetchAsync(int attemptNumber, CancellationToken cancellationToken)
    {
        await Task.Delay(20, cancellationToken);
        if (attemptNumber < 3)
        {
            throw new InvalidOperationException($"Simulated transient failure (attempt {attemptNumber})");
        }

        return "payload-ok";
    }

    /*
     * =========================================================================
     * SECTION 10: ONLY-ONE PATTERN (SemaphoreSlim)
     * =========================================================================
     *
     * Some resources allow only one concurrent user (single export slot, file
     * lock, printer). SemaphoreSlim(1, 1) acts as an async-friendly mutex:
     *
     *   await _gate.WaitAsync(token);
     *   try { ... exclusive work ... }
     *   finally { _gate.Release(); }
     *
     * Only one caller holds the semaphore at a time; others await WaitAsync.
     * (SemaphoreSlim intro also appears in 03. Tasks and TPL.)
     * -------------------------------------------------------------------------
     */
    private static readonly SemaphoreSlim ExportGate = new SemaphoreSlim(1, 1);

    public static async Task RunExclusiveExportAsync(string exportName, CancellationToken cancellationToken)
    {
        await ExportGate.WaitAsync(cancellationToken);
        try
        {
            Console.WriteLine($"  [{exportName}] acquired export slot");
            await Task.Delay(80, cancellationToken);
            Console.WriteLine($"  [{exportName}] released export slot");
        }
        finally
        {
            ExportGate.Release();
        }
    }

    /*
     * =========================================================================
     * SECTION 11: MULTITHREADING VS ASYNC — FULL COMPARISON
     * =========================================================================
     *
     * These solve different problems. Pick based on whether work is waiting
     * (I/O-bound) or computing (CPU-bound).
     *
     *  Model              | Best for              | Thread while waiting     | Primary chapter
     *  -------------------|-----------------------|--------------------------|------------------
     *  Thread (OS)        | Legacy, long-lived    | Blocked — wasted         | 01. Threads
     *  ThreadPool + Task  | Background work units | Blocked if sync wait     | 02–03. ThreadPool/Tasks
     *  async/await        | I/O-bound (HTTP, DB)  | Released — reusable      | this chapter
     *  Parallel.* / PLINQ | CPU-bound computation | All cores busy computing | 05. Parallel
     *
     * async does NOT make CPU work faster — it frees threads during waits.
     * For heavy math on many cores, use Parallel (ch.05), not more async.
     *
     * --- 11a. Demo — thread id during blocking vs async wait ---
     * -------------------------------------------------------------------------
     */
    public static void DemonstrateThreadBlockingVsAsync()
    {
        int beforeThread = Environment.CurrentManagedThreadId;
        Console.WriteLine($"  Thread before wait: {beforeThread}");

        // Blocking wait ties up the thread for the full duration
        Thread.Sleep(30);
        Console.WriteLine($"  After Thread.Sleep — same thread blocked: {Environment.CurrentManagedThreadId}");

        // Async wait releases the thread; continuation may run on a different pool thread
        Task asyncDemo = DemonstrateAsyncReleaseAsync(beforeThread);
        asyncDemo.GetAwaiter().GetResult();
    }

    private static async Task DemonstrateAsyncReleaseAsync(int callerThreadId)
    {
        Console.WriteLine($"  Before await — thread {Environment.CurrentManagedThreadId}");
        await Task.Delay(30); // thread released during delay
        Console.WriteLine(
            $"  After await — thread {Environment.CurrentManagedThreadId} " +
            $"(may differ from caller {callerThreadId}; thread was free during wait)");
    }

    /*
     * =========================================================================
     * SECTION 12: IAsyncEnumerable / ASYNC STREAMS (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → Advanced C# 8
     *
     * async iterators yield items over time without materializing the full
     * sequence. Consume with await foreach. Pass [EnumeratorCancellation]
     * and use .WithCancellation(token) to stop enumeration cooperatively.
     *
     * Minimal preview below — full patterns (cancellation, ConfigureAwait
     * in iterators, IAsyncDisposable) deferred to Advanced C# 8.
     * -------------------------------------------------------------------------
     */
    public static async IAsyncEnumerable<string> StreamReportPagesPreviewAsync(
        int pageCount,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int page = 1; page <= pageCount; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(30, cancellationToken);
            yield return $"Page-{page}";
        }
    }

    /*
     * =========================================================================
     * SECTION 13: ValueTask (PREVIEW)
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → Advanced C# 8
     *
     * ValueTask / ValueTask<T> reduce Task allocations when an async method
     * often completes synchronously (cached results, fast paths). Do not
     * await a ValueTask twice or store it for later — consume immediately.
     *
     * Preview: return ValueTask.FromResult for an already-complete value.
     * -------------------------------------------------------------------------
     */
    public static ValueTask<int> GetCachedRowCountPreviewAsync(int rowCount)
    {
        return ValueTask.FromResult(rowCount); // no Task allocation on hot path
    }

    /*
     * =========================================================================
     * SECTION 14: DEADLOCK PITFALLS — .Result AND .Wait()
     * =========================================================================
     *
     * Sync-over-async anti-patterns:
     *
     *   var x = GetDataAsync().Result;   // blocks calling thread
     *   GetDataAsync().Wait();           // same problem
     *
     * When the caller holds a SynchronizationContext (UI, legacy ASP.NET),
     * the async continuation needs that same thread to resume. Blocking that
     * thread with .Result/.Wait prevents the continuation → DEADLOCK.
     *
     * Console apps: often no context → blocking "works" but still wastes a
     * thread and hides async composition benefits.
     *
     * Fixes:
     *   • async all the way (await from async caller)
     *   • ConfigureAwait(false) in library code
     *   • Never .Result/.Wait on UI/request thread
     * -------------------------------------------------------------------------
     */
    public static void DemonstrateSyncOverAsyncSafely()
    {
        // Console has no SynchronizationContext — GetAwaiter().GetResult() completes
        int value = ComputeValueAsync().GetAwaiter().GetResult();
        Console.WriteLine($"  Sync-over-async in console (no context): ComputeValueAsync → {value}");
        Console.WriteLine("  On UI/ASP.NET: .Result/.Wait on captured context → deadlock");
    }

    public static async Task<int> ComputeValueAsync()
    {
        await Task.Delay(30);
        return 42;
    }
}

public class Program
{
    /*
     * Sync entry point. Async Main (C# 7.1+) is PREVIEW in Section 13 below.
     * GetAwaiter().GetResult() unwraps the Task and propagates exceptions.
     * Acceptable for console tutorial runner; production apps prefer async all the way.
     */
    public static void Main(string[] args)
    {
        RunTutorialAsync().GetAwaiter().GetResult();
    }

    /*
     * =========================================================================
     * DEMONSTRATION — Main ORCHESTRATES THE CHAPTER DEMO
     * =========================================================================
     *
     * Each block calls into ReportPipeline helpers defined above with their
     * own section comments. Main wires the scenario; it does not teach concepts.
     * -------------------------------------------------------------------------
     */
    private static async Task RunTutorialAsync()
    {
        Console.WriteLine("=== Report export pipeline (async/await tutorial) ===");
        Console.WriteLine();

        using CancellationTokenSource pipelineCts = new CancellationTokenSource();
        CancellationToken token = pipelineCts.Token;
        string reportId = "Q1-2026-Sales";

        // Section 2 — async/await
        Console.WriteLine("--- SECTION 2: async/await ---");
        ReportMetadata metadata = await ReportPipeline.FetchReportMetadataAsync(reportId, token);
        Console.WriteLine($"  Fetched metadata: {metadata.ReportId} ({metadata.RowCount} rows)");
        Console.WriteLine();

        // Section 3 — Task vs Task<T>
        Console.WriteLine("--- SECTION 3: Task-returning methods ---");
        await ReportPipeline.ProcessReportAsync(metadata, token);
        int processedCount = await ReportPipeline.CountProcessedRowsAsync(metadata.RowCount, token);
        Console.WriteLine($"  CountProcessedRowsAsync (Task<int>): {processedCount} rows");
        Console.WriteLine();

        // Section 4 — async void
        Console.WriteLine("--- SECTION 4: async void (event-handler pattern only) ---");
        ReportPipeline.LogExportStarted(reportId);
        await Task.Delay(30, token); // allow fire-and-forget log to finish
        Console.WriteLine();

        // Section 5 — ConfigureAwait
        Console.WriteLine("--- SECTION 5: ConfigureAwait ---");
        string libraryResult = await ReportPipeline.FetchWithLibraryPatternAsync(reportId, token);
        Console.WriteLine($"  Library helper (ConfigureAwait false): {libraryResult}");
        Console.WriteLine();

        // Section 6 — exception handling
        Console.WriteLine("--- SECTION 6: Exception handling in async ---");
        await ReportPipeline.DemonstrateAsyncExceptionHandlingAsync(token);
        Console.WriteLine();

        // Section 7 — CancellationToken
        Console.WriteLine("--- SECTION 7: CancellationToken ---");
        using CancellationTokenSource longJobCts = new CancellationTokenSource();
        longJobCts.CancelAfter(TimeSpan.FromMilliseconds(120));
        try
        {
            await ReportPipeline.SimulateLongRunningJobAsync(longJobCts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  Long job cancelled via CancelAfter (expected)");
        }

        Console.WriteLine();

        // Section 8 — non-cancellable workaround
        Console.WriteLine("--- SECTION 8: Cancel non-cancellable work ---");
        using CancellationTokenSource legacyCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        try
        {
            string legacy = await ReportPipeline.FetchLegacyWithTimeoutAsync(legacyCts.Token);
            Console.WriteLine($"  Legacy fetch returned: {legacy}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  Legacy fetch abandoned — WhenAny timeout won (expected)");
        }

        Console.WriteLine();

        // Section 9 — retry
        Console.WriteLine("--- SECTION 9: Retry pattern ---");
        int unreliableAttempts = 0;
        string reliableData = await ReportPipeline.RetryAsync(
            async () =>
            {
                unreliableAttempts++;
                return await ReportPipeline.UnreliableFetchAsync(unreliableAttempts, token);
            },
            maxAttempts: 4,
            baseDelayMs: 40,
            token);
        Console.WriteLine($"  Retry succeeded after {unreliableAttempts} attempt(s): {reliableData}");
        Console.WriteLine();

        // Section 10 — only-one
        Console.WriteLine("--- SECTION 10: Only-one pattern ---");
        Task exportA = ReportPipeline.RunExclusiveExportAsync("Export-A", token);
        Task exportB = ReportPipeline.RunExclusiveExportAsync("Export-B", token);
        await Task.WhenAll(exportA, exportB);
        Console.WriteLine("  Both exports finished sequentially (only-one gate)");
        Console.WriteLine();

        // Section 11 — multithreading vs async
        Console.WriteLine("--- SECTION 11: Multithreading vs async ---");
        ReportPipeline.DemonstrateThreadBlockingVsAsync();
        Console.WriteLine();

        // Section 12 — IAsyncEnumerable preview
        Console.WriteLine("--- SECTION 12: IAsyncEnumerable (preview) ---");
        List<string> pages = new List<string>();
        await foreach (string page in ReportPipeline.StreamReportPagesPreviewAsync(3, token))
        {
            pages.Add(page);
        }

        Console.WriteLine($"  Streamed {pages.Count} pages: {string.Join(", ", pages)}");
        Console.WriteLine("  Full async streams → Advanced C# 8");
        Console.WriteLine();

        // Section 13 — ValueTask preview
        Console.WriteLine("--- SECTION 13: ValueTask (preview) ---");
        int cached = await ReportPipeline.GetCachedRowCountPreviewAsync(metadata.RowCount);
        Console.WriteLine($"  ValueTask.FromResult preview: {cached} rows (no Task alloc on sync path)");
        Console.WriteLine("  ValueTask rules and pooling → Advanced C# 8");
        Console.WriteLine();

        // Section 13 — Async Main preview (comment-only; this project uses sync Main)
        Console.WriteLine("--- SECTION 13: Async Main (preview) ---");
        Console.WriteLine("  Entry: sync Main → RunTutorialAsync().GetAwaiter().GetResult()");
        Console.WriteLine("  Alternative: public static async Task Main(...) { await RunTutorialAsync(); }");
        Console.WriteLine();

        // Section 14 — deadlock pitfalls
        Console.WriteLine("--- SECTION 14: Deadlock pitfalls ---");
        ReportPipeline.DemonstrateSyncOverAsyncSafely();
        Console.WriteLine();
        Console.WriteLine("=== Tutorial complete ===");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — ASYNC AND AWAIT
 * =========================================================================
 *
 * --- Method signatures ---
 *
 *   async Task              DoWorkAsync()              // no return value
 *   async Task<T>           GetDataAsync()             // returns T via await
 *   async void              OnClick(...)               // events only
 *   async ValueTask<T>      GetCachedAsync()           // preview — Advanced C# 8
 *   async IAsyncEnumerable<T> StreamAsync()           // preview — Advanced C# 8
 *
 * --- await ---
 *
 *   var result = await task;                          // suspends method, not thread
 *   await task.ConfigureAwait(false);                 // library — skip context capture
 *
 * --- Exceptions ---
 *
 *   try { await task; } catch (Ex ex) { }              // await unwraps — no AggregateException
 *   task.Wait();                                      // throws AggregateException
 *
 * --- Cancellation ---
 *
 *   cts.Cancel();  token.ThrowIfCancellationRequested();
 *   await Task.Delay(ms, token);
 *   await foreach (var x in stream.WithCancellation(token))   // preview
 *
 * --- Non-cancellable API workaround ---
 *
 *   if (await Task.WhenAny(work, Task.Delay(ms, token)) != work)
 *       throw new OperationCanceledException(token);
 *
 * --- Retry skeleton ---
 *
 *   for (int i = 1; i <= max; i++)
 *       try { return await op(); }
 *       catch when (i < max && ex is not OperationCanceledException)
 *           { await Task.Delay(backoff * i, token); }
 *
 * --- Only-one (SemaphoreSlim) ---
 *
 *   await gate.WaitAsync(token);
 *   try { ... }
 *   finally { gate.Release(); }
 *
 * --- Multithreading vs async ---
 *
 *   I/O waiting     → async/await (this chapter)
 *   CPU computation → Parallel.* / PLINQ (ch.05)
 *   Raw threads     → ch.01; Task composition → ch.03
 *
 * --- Deadlock avoidance ---
 *
 *   DO     await all the way from async callers
 *   DO     ConfigureAwait(false) in libraries
 *   DON'T  .Result / .Wait() on UI or request thread
 *
 * --- Related chapters ---
 *
 *   01. Threads and Thread Lifecycle    OS threads, blocking
 *   03. Tasks and TPL                   Task, WhenAll, SemaphoreSlim intro
 *   05. Parallel Programming            CPU parallel vs I/O async
 *   06. Synchronization and Locks       locks, Monitor, broader concurrency
 *
 * =========================================================================
 */
