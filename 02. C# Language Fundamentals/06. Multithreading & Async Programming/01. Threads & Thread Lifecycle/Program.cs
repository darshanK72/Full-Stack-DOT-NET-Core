/*
 * =============================================================================
 * 01. THREADS AND THREAD LIFECYCLE — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Introduction to concurrency with System.Threading.Thread — creating
 *        OS threads, lifecycle (Start / Join / Sleep), foreground vs background,
 *        thread-local storage, passing parameters, cooperative shutdown, and
 *        debugging multithreaded console apps.
 *
 * WHY IT MATTERS:
 *   A warehouse may scan, weigh, and label packages on separate stations at
 *   the same time. Each station can map to a dedicated thread when you need
 *   explicit control over thread creation (before ThreadPool or Task in later
 *   chapters). Understanding lifecycle, foreground vs background, thread-local
 *   data, and safe termination prevents hung processes and corrupted tallies
 *   when multiple workers touch shared counters.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Concurrency basics — process, thread, parallel vs sequential
 *   2.  Thread constructor, ThreadStart, Start, and Sleep
 *   3.  ThreadState lifecycle (Unstarted → Running → Stopped)
 *   4.  ThreadPriority as an OS scheduling hint
 *   5.  Foreground vs background threads and process lifetime
 *   6.  ParameterizedThreadStart and returning data via shared state
 *   7.  Join, Join(timeout), and IsAlive — waiting for workers
 *   8.  Cooperative termination with CancellationToken (no Thread.Abort)
 *   9.  Thread-local storage — ThreadLocal<T> and [ThreadStatic]
 *  10.  Shared memory and lock preview for inter-thread communication
 *  11.  Multithreading vs async — preview only
 *  12.  Debug tips for multithreaded apps
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadsAndThreadLifecycle;

/*
 * =========================================================================
 * SECTION 1: INTRODUCTION TO CONCURRENCY
 * =========================================================================
 *
 * A process is a running program with its own memory. A thread is the unit
 * of execution inside that process — the scheduler runs threads, not processes.
 *
 *   Term              | Meaning
 *   ------------------|----------------------------------------------------
 *   Sequential        | One step after another on a single thread
 *   Concurrent        | Multiple threads make progress; may time-slice on one CPU
 *   Parallel          | Multiple threads run truly at the same time (multi-core)
 *
 * System.Threading.Thread gives you a dedicated OS thread you create and start
 * manually. Most modern server code prefers Task / async (later chapters) for
 * I/O-bound work and Parallel / ThreadPool for short CPU bursts — but Thread
 * remains the foundation for lifecycle concepts and long-lived dedicated workers.
 *
 * COVERED IN DETAIL LATER → 02. ThreadPool, 03. Tasks and Task Parallel Library,
 *                             04. Async and Await, 05. Parallel Programming
 * -------------------------------------------------------------------------
 */

/*
 * =========================================================================
 * SECTION 2: SCENARIO DATA — SHIPMENT WORK ITEMS
 * =========================================================================
 *
 * ShipmentWork travels into a ParameterizedThreadStart delegate as object? state.
 * ShipmentResult is written to a shared list after each worker finishes — threads
 * cannot return values like ordinary methods.
 * -------------------------------------------------------------------------
 */
public sealed class ShipmentWork
{
    public ShipmentWork(string shipmentId, string destination, int boxCount, int millisecondsPerBox)
    {
        ShipmentId = shipmentId;
        Destination = destination;
        BoxCount = boxCount;
        MillisecondsPerBox = millisecondsPerBox;
    }

    public string ShipmentId { get; }
    public string Destination { get; }
    public int BoxCount { get; }
    public int MillisecondsPerBox { get; }
}

public sealed class ShipmentResult
{
    public ShipmentResult(
        string shipmentId,
        string destination,
        int boxesProcessed,
        int elapsedMs,
        int workerThreadId,
        string scanLog)
    {
        ShipmentId = shipmentId;
        Destination = destination;
        BoxesProcessed = boxesProcessed;
        ElapsedMs = elapsedMs;
        WorkerThreadId = workerThreadId;
        ScanLog = scanLog;
    }

    public string ShipmentId { get; }
    public string Destination { get; }
    public int BoxesProcessed { get; }
    public int ElapsedMs { get; }
    public int WorkerThreadId { get; }
    public string ScanLog { get; }
}

public class Program
{
    /*
     * Shared tally protected by lock in ProcessShipment (SECTION 10 preview).
     * CompletedShipments is appended once per worker after all boxes are scanned.
     */
    private static readonly object TallyLock = new object();
    private static int _packagesProcessed;
    private static readonly List<ShipmentResult> CompletedShipments = new List<ShipmentResult>();

    /*
     * =========================================================================
     * SECTION 9: THREAD-LOCAL STORAGE — ThreadLocal<T>
     * =========================================================================
     *
     * Each thread gets its own copy of thread-local data — no lock needed for
     * per-thread scratch state. ThreadLocal<T> lazily constructs a value the
     * first time a thread reads .Value.
     *
     *   ThreadLocal<StringBuilder> buffer = new(() => new StringBuilder());
     *   buffer.Value.Append("…");   // unique builder per thread
     *
     * Dispose the ThreadLocal when done if you allocated unmanaged resources.
     * trackAllValues: true keeps references for inspection/debugging (uses memory).
     *
     * --- 9a. [ThreadStatic] attribute ---
     *
     *   [ThreadStatic] private static int _serial;
     *
     * Each thread sees its own _serial field. Caveats:
     *   • Only works on static fields
     *   • No per-thread initializer — defaults to 0/null until you assign
     *   • Async/await can hop threads — [ThreadStatic] may surprise you (ch.04)
     * -------------------------------------------------------------------------
     */
    private static readonly ThreadLocal<StringBuilder> ScanLogBuffer =
        new ThreadLocal<StringBuilder>(() => new StringBuilder(capacity: 128), trackAllValues: true);

    [ThreadStatic]
    private static int _boxSerialOnThread;

    /*
     * =========================================================================
     * SECTION 3: Thread CLASS — ThreadStart, Start, AND Sleep
     * =========================================================================
     *
     * Thread wraps an OS thread. Constructor overloads accept:
     *
     *   ThreadStart              → void Worker()
     *   ParameterizedThreadStart → void Worker(object? state)
     *
     * Start() schedules the delegate. Until then ThreadState is Unstarted.
     *
     * Thread.Sleep(milliseconds) blocks the CURRENT thread only and yields its
     * time slice. It is not a precise timer — the OS may resume slightly later.
     * -------------------------------------------------------------------------
     */
    public static void PrintDockReady()
    {
        Console.WriteLine($"[{Thread.CurrentThread.Name}] Dock systems online (ThreadStart demo).");
        Thread.Sleep(100); // simulate brief startup self-test on this thread
    }

    /*
     * =========================================================================
     * SECTION 6: ParameterizedThreadStart — ProcessShipment WORKER
     * =========================================================================
     *
     * Signature required by ParameterizedThreadStart:
     *
     *   void ProcessShipment(object? state)
     *
     * Cast state to your work type, simulate per-box work with Sleep, increment
     * a shared counter under lock, and append a ShipmentResult when finished.
     *
     * ThreadLocal<StringBuilder> accumulates per-thread scan lines without
     * locking — each worker thread owns its buffer instance.
     * -------------------------------------------------------------------------
     */
    public static void ProcessShipment(object? state)
    {
        ShipmentWork work = (ShipmentWork)state!;
        DateTime started = DateTime.UtcNow;
        int boxesDone = 0;
        StringBuilder log = ScanLogBuffer.Value!; // thread-local — no lock for append
        log.Clear();

        for (int box = 1; box <= work.BoxCount; box++)
        {
            Thread.Sleep(work.MillisecondsPerBox);

            _boxSerialOnThread++; // [ThreadStatic] — independent counter per thread

            lock (TallyLock)
            {
                _packagesProcessed++; // shared — must synchronize
            }

            boxesDone++;
            log.Append($"box{box} ");
            Console.WriteLine(
                $"[{Thread.CurrentThread.Name}] {work.ShipmentId} box {box}/{work.BoxCount} " +
                $"(thread serial {_boxSerialOnThread}).");
        }

        ShipmentResult result = new ShipmentResult(
            work.ShipmentId,
            work.Destination,
            boxesDone,
            (int)(DateTime.UtcNow - started).TotalMilliseconds,
            Thread.CurrentThread.ManagedThreadId,
            log.ToString().TrimEnd());

        lock (TallyLock)
        {
            CompletedShipments.Add(result);
        }
    }

    /*
     * =========================================================================
     * SECTION 8: COOPERATIVE TERMINATION — CancellationToken
     * =========================================================================
     *
     * NEVER use Thread.Abort — removed from .NET Core because it could leave
     * locks and invariants broken mid-method.
     *
     * Safe patterns:
     *   1. CancellationToken — pass token; check IsCancellationRequested in loop
     *   2. volatile bool _stop — worker checks flag and exits cleanly
     *
     * The delegate returns normally; the thread becomes Stopped without force.
     * -------------------------------------------------------------------------
     */
    public static void RunInventorySweep(CancellationToken token)
    {
        int aisle = 0;
        while (!token.IsCancellationRequested)
        {
            aisle++;
            Thread.Sleep(80);
            Console.WriteLine($"[{Thread.CurrentThread.Name}] scanning aisle {aisle} …");

            if (aisle >= 20)
            {
                break; // natural completion without cancellation
            }
        }

        if (token.IsCancellationRequested)
        {
            Console.WriteLine($"[{Thread.CurrentThread.Name}] cancellation acknowledged at aisle {aisle}.");
        }
    }

    /*
     * =========================================================================
     * SECTION 13: DEMONSTRATION — Main ORCHESTRATES THE CHAPTER
     * =========================================================================
     *
     * Main creates threads, prints lifecycle observations, Joins workers, and
     * prints shared results. Section comments live above the types and methods
     * they teach — Main wires the runnable demo only.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        ShipmentWork[] pendingShipments =
        [
            new ShipmentWork("SH-1001", "North Hub", boxCount: 3, millisecondsPerBox: 120),
            new ShipmentWork("SH-1002", "South Hub", boxCount: 2, millisecondsPerBox: 150),
            new ShipmentWork("SH-1003", "East Hub", boxCount: 4, millisecondsPerBox: 90),
        ];

        Console.WriteLine("=== Apex Warehouse — threads and lifecycle demo ===");
        Console.WriteLine($"Main thread id: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"Pending shipments: {pendingShipments.Length}");
        Console.WriteLine();

        // --- SECTION 3: ThreadStart probe ---
        Thread startupProbe = new Thread(PrintDockReady);
        startupProbe.Name = "DockProbe";

        // --- SECTION 4: ThreadState lifecycle ---
        Console.WriteLine("--- Thread lifecycle (DockProbe) ---");
        Console.WriteLine($"  Before Start:          {startupProbe.ThreadState}");
        startupProbe.Start();
        Console.WriteLine($"  After Start:           {startupProbe.ThreadState}");
        startupProbe.Join();
        Console.WriteLine($"  After Join (finished): {startupProbe.ThreadState}");
        Console.WriteLine();

        /*
         * SECTION 4: THREAD LIFECYCLE STATES (reference)
         *
         *   State            | Meaning
         *   -----------------|--------------------------------------------------
         *   Unstarted        | Constructed; Start() not called yet
         *   Running          | Actively executing managed code
         *   WaitSleepJoin    | Blocked: Sleep, Wait, or Join on another thread
         *   Stopped          | Delegate finished; cannot restart — create new Thread
         *
         * Background is OR-ed into Running/WaitSleepJoin when IsBackground is true.
         */

        // --- SECTION 5: ThreadPriority hint ---
        Thread expressScanner = new Thread(() =>
        {
            Thread.Sleep(80);
            Console.WriteLine("[Express scanner] Priority lane label applied.");
        });
        expressScanner.Priority = ThreadPriority.AboveNormal; // set before Start when possible
        expressScanner.Name = "ExpressScanner";

        Thread standardScanner = new Thread(() =>
        {
            Thread.Sleep(80);
            Console.WriteLine("[Standard scanner] Regular lane label applied.");
        });
        standardScanner.Priority = ThreadPriority.Normal;
        standardScanner.Name = "StandardScanner";

        Console.WriteLine("--- Thread priorities (output order may vary) ---");
        expressScanner.Start();
        standardScanner.Start();
        expressScanner.Join();
        standardScanner.Join();
        Console.WriteLine();

        // --- SECTION 6: Foreground vs background ---
        Thread heartbeat = new Thread(() =>
        {
            for (int tick = 1; tick <= 3; tick++)
            {
                Thread.Sleep(50);
                Console.WriteLine($"[Heartbeat] background tick {tick}");
            }
        });
        heartbeat.IsBackground = true; // process will not wait for this at exit
        heartbeat.Name = "DockHeartbeat";
        heartbeat.Start();
        heartbeat.Join(); // Main still waits here so output is visible before workers spawn

        // --- SECTION 6–7: Parameterized workers ---
        Thread[] workers = new Thread[pendingShipments.Length];

        for (int i = 0; i < pendingShipments.Length; i++)
        {
            ShipmentWork work = pendingShipments[i];
            Thread worker = new Thread(ProcessShipment);
            worker.Name = $"Worker-{work.ShipmentId}";
            worker.Priority = i == 0 ? ThreadPriority.AboveNormal : ThreadPriority.Normal;
            workers[i] = worker;
            worker.Start(work); // ParameterizedThreadStart — pass ShipmentWork as state
        }

        Console.WriteLine("--- Parameterized workers started ---");
        foreach (Thread worker in workers)
        {
            Console.WriteLine(
                $"  {worker.Name}  IsBackground={worker.IsBackground}  Priority={worker.Priority}");
        }

        // --- SECTION 7: Join and IsAlive ---
        Console.WriteLine();
        Console.WriteLine("--- Waiting for workers (IsAlive → Join) ---");

        foreach (Thread worker in workers)
        {
            while (worker.IsAlive)
            {
                Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
                if (!worker.Join(millisecondsTimeout: 100))
                {
                    continue; // Join(ms) returns false when timeout expires
                }
            }

            Console.WriteLine($"  {worker.Name} finished. ThreadState={worker.ThreadState}");
        }

        // --- SECTION 8: Cooperative cancellation demo ---
        using CancellationTokenSource inventoryCts = new CancellationTokenSource();
        CancellationToken inventoryToken = inventoryCts.Token;

        Thread inventorySweep = new Thread(() => RunInventorySweep(inventoryToken));
        inventorySweep.Name = "InventorySweep";
        inventorySweep.Start();

        Thread.Sleep(250);
        inventoryCts.Cancel();
        inventorySweep.Join();

        Console.WriteLine();
        Console.WriteLine("--- Cooperative cancellation complete ---");

        // --- SECTION 9: ThreadLocal values collected after workers finished ---
        Console.WriteLine();
        Console.WriteLine("--- ThreadLocal scan buffers (one per worker thread) ---");
        foreach (StringBuilder buffer in ScanLogBuffer.Values)
        {
            Console.WriteLine($"  buffer hash={buffer.GetHashCode()}  content=\"{buffer}\"");
        }

        // --- SECTION 10: Shared results and lock preview ---
        Console.WriteLine();
        Console.WriteLine("--- Shipment results (shared state) ---");
        foreach (ShipmentResult result in CompletedShipments)
        {
            Console.WriteLine(
                $"  {result.ShipmentId} → {result.Destination}: " +
                $"{result.BoxesProcessed} boxes in {result.ElapsedMs} ms " +
                $"(worker thread {result.WorkerThreadId}, log: {result.ScanLog})");
        }

        Console.WriteLine();
        Console.WriteLine($"Total packages processed (locked tally): {_packagesProcessed}");

        /*
         * SECTION 10: INTER-THREAD COMMUNICATION — lock PREVIEW
         *
         * Threads communicate through shared memory (fields, collections). Without
         * coordination, read-modify-write on the same counter loses updates.
         *
         *   lock (syncRoot) { … }   — only one thread enters at a time
         *
         * COVERED IN DETAIL LATER → 06. Synchronization and Locks
         */

        /*
         * SECTION 11: MULTITHREADING VS ASYNC — PREVIEW
         *
         *   Model           | Best for                         | Blocking?
         *   ----------------|----------------------------------|----------
         *   Thread          | Dedicated long-lived CPU worker  | Yes — Sleep/Join block
         *   ThreadPool/Task | Many short CPU callbacks         | Yes on worker thread
         *   async/await     | I/O-bound (network, files, DB)   | No — thread released during I/O
         *
         * async does not create one thread per operation; it frees threads while
         * waiting on I/O. CPU-bound parallel work → ch.05 Parallel Programming.
         *
         * COVERED IN DETAIL LATER → 04. Async and Await
         */

        /*
         * SECTION 12: DEBUG MULTITHREADED APP TIPS
         *
         *   Tip                         | Practice
         *   ----------------------------|------------------------------------------
         *   Name threads                | thread.Name = "Worker-SH-1001" in debugger
         *   Console order varies        | Interleaved WriteLine is normal; use tags
         *   Breakpoints hit any thread  | Pause All Threads; inspect ManagedThreadId
         *   Reproduce races             | Run multiple times; reduce Sleep to expose bugs
         *   Avoid Sleep for sync        | Sleep hides timing bugs; prefer Join/events
         *
         * Visual Studio Threads / Parallel Stacks windows show Name labels assigned above.
         */

        ScanLogBuffer.Dispose(); // release thread-local storage tracking

        Console.WriteLine();
        Console.WriteLine("=== All foreground workers joined — Main exiting ===");
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — THREADS AND THREAD LIFECYCLE
 * =========================================================================
 *
 * --- Concurrency ---
 *
 *   Process           running program; owns memory
 *   Thread            unit of execution; System.Threading.Thread = manual OS thread
 *   Concurrent        progress on multiple threads (may time-slice)
 *   Parallel          simultaneous execution (multi-core)
 *
 * --- Create and start ---
 *
 *   Thread t = new Thread(() => { … });           // ThreadStart
 *   Thread t = new Thread(Worker);                // method group
 *   Thread t = new Thread(WorkerWithState);
 *   t.Start();                                    // no parameter
 *   t.Start(stateObject);                         // ParameterizedThreadStart
 *
 * --- Sleep ---
 *
 *   Thread.Sleep(500);                            // current thread only; ms
 *
 * --- Lifecycle ---
 *
 *   t.ThreadState     Unstarted | Running | WaitSleepJoin | Stopped (+ Background)
 *   t.IsAlive         true until delegate completes
 *   t.Join()          block until finished
 *   t.Join(2000)      wait up to 2 s; false if timeout
 *
 * --- Priority and background ---
 *
 *   t.Priority = ThreadPriority.AboveNormal;      // OS hint only — not a guarantee
 *   t.IsBackground = true;                        // won't keep process alive alone
 *
 * --- Pass / return data ---
 *
 *   void Worker(object? state) { var w = (WorkItem)state!; … }
 *   Return via shared field/list — use lock if concurrent writes
 *
 * --- Thread-local storage ---
 *
 *   ThreadLocal<T> tl = new(() => new T());
 *   tl.Value …                                    // per-thread instance
 *   tl.Dispose();                                 // when done
 *   [ThreadStatic] static int _x;                 // static only; no ctor init
 *
 * --- Safe termination ---
 *
 *   CancellationTokenSource cts = new();
 *   token.IsCancellationRequested  → exit loop cleanly
 *   volatile bool _stop;            → simple flag pattern
 *   NEVER Thread.Abort               removed / unsafe
 *
 * --- lock preview ---
 *
 *   lock (syncRoot) { sharedCounter++; }
 *   Full treatment → 06. Synchronization and Locks
 *
 * --- Debug ---
 *
 *   t.Name = "Worker-1";              debugger Threads window
 *   Thread.CurrentThread.ManagedThreadId
 *   Console output order is non-deterministic across runs
 *
 * --- Related chapters ---
 *
 *   02. ThreadPool                    pooled threads, QueueUserWorkItem
 *   03. Tasks and Task Parallel Library typed results, continuations
 *   04. Async and Await               I/O-bound concurrency model
 *   05. Parallel Programming          CPU-bound parallel loops
 *   06. Synchronization and Locks     lock, Monitor, Mutex, Semaphore, deadlock
 *   07. Concurrent Collections        thread-safe collection types
 *
 * =========================================================================
 */
