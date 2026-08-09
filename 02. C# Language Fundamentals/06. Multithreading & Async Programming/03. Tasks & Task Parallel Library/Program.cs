/*
 * =============================================================================
 * 03. TASKS AND TASK PARALLEL LIBRARY — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Task-based programming with the Task Parallel Library (TPL) —
 *        represent work as Task / Task<T>, schedule on the thread pool,
 *        coordinate batches, chain continuations, handle faults, cancel
 *        cooperatively, and bridge legacy callback APIs into a task model.
 *
 * WHY IT MATTERS:
 *   Raw Thread objects (ch.01) and ThreadPool.QueueUserWorkItem (ch.02) get
 *   work onto threads but offer no standard way to know when work finished,
 *   collect a return value, or combine many operations. Task fills that gap
 *   and is the foundation for async/await (ch.04) and Parallel.* (ch.05).
 *
 * WHAT YOU WILL LEARN:
 *   1.  TPL overview — Task lifecycle and status properties
 *   2.  Task vs Thread — abstraction vs OS thread resource
 *   3.  Task.Run vs Task.Factory.StartNew — prefer Run
 *   4.  Task<T>, Task.FromResult — return values and pre-completed tasks
 *   5.  Task.WhenAll and Task.WhenAny — batch and first-wins coordination
 *   6.  SemaphoreSlim — cap concurrent warehouse operations
 *   7.  Continuation tasks — ContinueWith and status filters
 *   8.  Attached child tasks — parent waits for nested work
 *   9.  TaskCompletionSource<T> — control result from outside the delegate
 *  10.  Sync wrapper — expose blocking API over task-based callback code
 *  11.  TaskScheduler basics — Default, Current, and scheduling choices
 *  12.  Exception handling — Faulted, AggregateException, OnlyOnFaulted
 *  13.  CancellationToken preview → ch.04
 *  14.  async vs Task distinction preview → ch.04
 *  15.  ValueTask preview → ch.04 / Advanced C# 8
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TasksAndTaskParallelLibrary;

/*
 * =========================================================================
 * SECTION 1: ORDER — DOMAIN TYPE FOR WAREHOUSE PIPELINE DEMOS
 * =========================================================================
 *
 * A small e-commerce order flows through validate → pick → ship in every
 * section below. Keeping one scenario makes Task composition concrete.
 *
 *   OrderId    unique identifier
 *   Customer   display name
 *   Total      order value (validation checks Total > 0)
 *   Lines      line items picked individually in attached-child demo
 *
 * init-only properties — set once at construction; immutable after create.
 * -------------------------------------------------------------------------
 */
public sealed class Order
{
    public required int OrderId { get; init; }
    public required string Customer { get; init; }
    public required decimal Total { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }
}

/*
 * =========================================================================
 * SECTION 2: PAYMENT RESULT — RESULT TYPE FOR TaskCompletionSource
 * =========================================================================
 *
 * Payment gateway callbacks (SECTION 9) complete a Task<PaymentResult>
 * from outside the original delegate. A small record keeps the demo readable.
 * -------------------------------------------------------------------------
 */
public sealed record PaymentResult(int OrderId, string Status, string TransactionId);

public class Program
{
    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 3: DEMONSTRATION — MAIN ORCHESTRATES THE CHAPTER DEMO
         * =========================================================================
         *
         * Main creates sample orders and calls each static demo method in reading
         * order. Concept explanation lives above the methods and types — not here.
         * -------------------------------------------------------------------------
         */

        Order[] orders =
        [
            new Order
            {
                OrderId = 1001,
                Customer = "Ada",
                Total = 129.50m,
                Lines = new[] { "Keyboard", "Mouse pad" },
            },
            new Order
            {
                OrderId = 1002,
                Customer = "Lin",
                Total = 45.00m,
                Lines = new[] { "USB cable" },
            },
            new Order
            {
                OrderId = 1003,
                Customer = "Sam",
                Total = 899.99m,
                Lines = new[] { "Monitor", "HDMI cable", "Desk mount" },
            },
            new Order
            {
                OrderId = 1004,
                Customer = "Ravi",
                Total = 18.25m,
                Lines = new[] { "Sticker pack" },
            },
        ];

        Console.WriteLine("=== Order batch — Tasks and TPL tutorial ===");
        foreach (Order order in orders)
        {
            Console.WriteLine($"  #{order.OrderId} {order.Customer} — ${order.Total:F2} ({order.Lines.Count} lines)");
        }

        Console.WriteLine();
        DemonstrateTaskLifecycle();
        DemonstrateTaskVsThread(orders[0]);
        DemonstrateTaskRunVsStartNew(orders);
        DemonstrateTaskOfT(orders);
        DemonstrateWhenAll(orders);
        DemonstrateWhenAny();
        DemonstrateThrottledPicks(orders);
        DemonstrateContinuations(orders[0]);
        DemonstrateAttachedChildren(orders[2]);
        DemonstrateTaskCompletionSource(orders[0]);
        DemonstrateSyncWrapper(orders[1]);
        DemonstrateTaskScheduler();
        DemonstrateTaskExceptions();
        DemonstrateCancellationPreview();
        DemonstrateAsyncVsTaskPreview();
        DemonstrateValueTaskPreview();

        Console.WriteLine();
        Console.WriteLine("=== Order batch processing demo complete ===");
    }

    /*
     * =========================================================================
     * SECTION 4: TPL OVERVIEW — TASK LIFECYCLE AND STATUS
     * =========================================================================
     *
     * The Task Parallel Library (System.Threading.Tasks) models work as a Task
     * object with a lifecycle:
     *
     *   Status     Created → WaitingToRun → Running → RanToCompletion
     *                                              ↘ Faulted (exception)
     *                                              ↘ Canceled
     *
     * Key members while observing a running task:
     *
     *   Status           current lifecycle state (TaskStatus enum)
     *   IsCompleted      true when RanToCompletion, Faulted, or Canceled
     *   IsFaulted        true when an unhandled exception ended the task
     *   IsCanceled       true when canceled via CancellationToken
     *   Id               unique id (except default Task / some special tasks)
     *
     * Roadmap in this module:
     *
     *   ch.03 (here)   Task API, coordination, TCS wrappers
     *   ch.04          async/await — syntactic sugar over tasks + CancellationToken
     *   ch.05          Parallel.For / ForEach — data-parallel loops
     *
     * You CAN block on tasks with .Wait() or .Result, but in UI/ASP.NET that
     * often deadlocks — ch.04 covers safe patterns.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskLifecycle()
    {
        Console.WriteLine("--- TPL overview: Task lifecycle ---");

        Task lifecycleTask = Task.Run(() =>
        {
            Thread.Sleep(40);
            Console.WriteLine("    lifecycle task body running");
        });

        Console.WriteLine($"  after Start (queued): Status={lifecycleTask.Status}, IsCompleted={lifecycleTask.IsCompleted}");
        lifecycleTask.Wait();
        Console.WriteLine($"  after Wait():         Status={lifecycleTask.Status}, IsCompleted={lifecycleTask.IsCompleted}");
    }

    /*
     * =========================================================================
     * SECTION 5: TASK VS THREAD
     * =========================================================================
     *
     * Comparison:
     *
     *   Thread                         Task
     *   ------------------------------ ---------------------------------
     *   OS-level execution resource    Abstraction for a unit of work
     *   You manage Start/Join          Status, result, continuations built in
     *   Expensive to create many       Cheap to queue many (pool-backed)
     *   Low-level (ch.01)              High-level coordination (this chapter)
     *
     * A Task usually runs on a thread-pool thread, but the Task object is
     * NOT the thread — it is the promise that work will complete (or fail).
     *
     * --- 5a. Thread — manual thread for contrast ---
     * --- 5b. Task — same idea with built-in completion tracking ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskVsThread(Order sampleOrder)
    {
        int orderIdFromThread = 0;
        using (ManualResetEvent threadDone = new ManualResetEvent(false))
        {
            Thread validationThread = new Thread(() =>
            {
                Thread.Sleep(30);
                orderIdFromThread = sampleOrder.OrderId;
                threadDone.Set();
            });
            validationThread.IsBackground = true;
            validationThread.Start();
            threadDone.WaitOne();
        }

        Console.WriteLine();
        Console.WriteLine($"--- Task vs Thread: Thread validated order #{orderIdFromThread} (manual signal) ---");

        Task<int> validateTask = Task.Run(() =>
        {
            Thread.Sleep(30);
            return sampleOrder.OrderId;
        });

        int orderIdFromTask = validateTask.Result; // blocks until RanToCompletion
        Console.WriteLine($"--- Task vs Thread: Task validated order #{orderIdFromTask} (Task.Result) ---");
    }

    /*
     * =========================================================================
     * SECTION 6: Task.Run VS Task.Factory.StartNew
     * =========================================================================
     *
     * Both schedule delegates, but defaults differ:
     *
     *   Task.Run(action)
     *     • Uses TaskScheduler.Default (thread pool)
     *     • TaskCreationOptions.None
     *     • Safe default for CPU / short pool work
     *
     *   Task.Factory.StartNew(action, options)
     *     • Inherits current TaskScheduler unless you pass one
     *     • Easy to accidentally use wrong scheduler or LongRunning
     *     • Still valid when you NEED specific StartNew options (rare)
     *
     * Rule of thumb: prefer Task.Run. Reach for StartNew only when you
     * explicitly require non-default TaskCreationOptions AND understand
     * the scheduler implications.
     *
     * --- 6a. Preferred: Task.Run ---
     * --- 6b. StartNew — shown for recognition, not daily use ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskRunVsStartNew(Order[] orders)
    {
        Task pickRun = Task.Run(() => SimulatePick(orders[1].OrderId, delayMs: 40));
        pickRun.Wait();

        Console.WriteLine();
        Console.WriteLine($"--- Task.Run: picked order #{orders[1].OrderId} on pool thread ---");

        Task pickStartNew = Task.Factory.StartNew(
            () => SimulatePick(orders[2].OrderId, delayMs: 40),
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default);
        pickStartNew.Wait();

        Console.WriteLine($"--- StartNew (Default scheduler): picked order #{orders[2].OrderId} — prefer Task.Run ---");
    }

    /*
     * =========================================================================
     * SECTION 7: RETURN VALUE FROM Task — Task<T> AND Task.FromResult
     * =========================================================================
     *
     * Task<T> adds a .Result property (and await in ch.04) after the task
     * reaches RanToCompletion.
     *
     *   Task<decimal> taxTask = Task.Run(() => CalculateTax(order));
     *   decimal tax = taxTask.Result;   // blocks until done
     *
     * Task.FromResult(value) returns an already-completed Task<T> — useful
     * when a code path has a synchronous result but the API must return Task<T>.
     *
     * .Result and .GetAwaiter().GetResult() block the calling thread.
     * On a thread-pool thread or with synchronization context (UI), blocking
     * can deadlock — see ch.04 "Deadlock pitfalls".
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskOfT(Order[] orders)
    {
        Task<decimal>[] quoteTasks = orders
            .Select(order => Task.Run(() => CalculateShippingQuote(order)))
            .ToArray();

        Task.WaitAll(quoteTasks);

        Console.WriteLine();
        Console.WriteLine("--- Task<T> shipping quotes ---");
        for (int i = 0; i < orders.Length; i++)
        {
            Console.WriteLine($"  Order #{orders[i].OrderId}: quote ${quoteTasks[i].Result:F2}");
        }

        // Fast path: order already has a cached quote — no thread pool work needed
        Task<decimal> cachedQuote = Task.FromResult(9.99m);
        Console.WriteLine($"--- Task.FromResult: cached quote ${cachedQuote.Result:F2} (already RanToCompletion) ---");
    }

    /*
     * =========================================================================
     * SECTION 8: Task.WhenAll — EXECUTE MULTIPLE TASKS, WAIT FOR ALL
     * =========================================================================
     *
     * WhenAll returns a Task that completes when every input task completes.
     * Overloads accept Task[] or IEnumerable<Task> / Task<T>.
     *
     *   Task<PickResult[]> batch = Task.WhenAll(pickTasks);
     *   PickResult[] results = batch.Result;
     *
     * Task.WaitAll(tasks) blocks until all finish but does not return a
     * combined Task<T[]> — prefer WhenAll for composition and await (ch.04).
     *
     * Use for batch fulfillment: validate every order before releasing picks.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateWhenAll(Order[] orders)
    {
        Task<bool>[] validationTasks = orders
            .Select(order => Task.Run(() => ValidateOrder(order)))
            .ToArray();

        Task<bool[]> allValidations = Task.WhenAll(validationTasks);
        bool[] validationResults = allValidations.Result;

        int validCount = validationResults.Count(valid => valid);

        Console.WriteLine();
        Console.WriteLine($"--- WhenAll: validated {validCount}/{orders.Length} orders ---");
    }

    /*
     * =========================================================================
     * SECTION 9: Task.WhenAny — FIRST COMPLETED TASK WINS
     * =========================================================================
     *
     * WhenAny completes when ANY constituent task completes. Useful when
     * multiple carriers quote shipping and you take the fastest response.
     *
     *   Task<CarrierQuote> fastest = await Task.WhenAny(carrierTasks);
     *
     * (Blocking .Result shown here; ch.04 uses await.)
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateWhenAny()
    {
        Task<(string Carrier, decimal Price, int Ms)>[] carrierTasks =
        [
            Task.Run(() => SimulateCarrierQuote("FastFreight", basePrice: 12.00m, delayMs: 120)),
            Task.Run(() => SimulateCarrierQuote("EconomyPost", basePrice: 8.50m, delayMs: 40)),
            Task.Run(() => SimulateCarrierQuote("PremiumAir", basePrice: 24.00m, delayMs: 80)),
        ];

        Task<(string Carrier, decimal Price, int Ms)> winner = Task.WhenAny(carrierTasks).Result;
        (string carrier, decimal price, int ms) = winner.Result;

        Console.WriteLine();
        Console.WriteLine($"--- WhenAny: first carrier response — {carrier} ${price:F2} ({ms} ms) ---");
    }

    /*
     * =========================================================================
     * SECTION 10: LIMIT CONCURRENT TASKS — SemaphoreSlim
     * =========================================================================
     *
     * Firing one Task per order for warehouse picking can overload workers or
     * downstream APIs. SemaphoreSlim maintains an internal count:
     *
     *   Wait()     decrement count; block if zero
     *   Release()  increment count; unblock a waiter
     *
     * Pattern: acquire in try, Release in finally.
     *
     * Full lock/mutex treatment → 06. Synchronization and Locks
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateThrottledPicks(Order[] orders)
    {
        const int maxConcurrentPicks = 2;
        using SemaphoreSlim pickerGate = new SemaphoreSlim(maxConcurrentPicks, maxConcurrentPicks);

        Task<string>[] throttledPickTasks = orders
            .Select(order => Task.Run(() => ThrottledPick(order, pickerGate)))
            .ToArray();

        Task.WaitAll(throttledPickTasks);

        Console.WriteLine();
        Console.WriteLine("--- SemaphoreSlim (max 2 concurrent picks) ---");
        foreach (Task<string> pickTask in throttledPickTasks)
        {
            Console.WriteLine($"  {pickTask.Result}");
        }
    }

    /*
     * =========================================================================
     * SECTION 11: CONTINUATION TASKS — ContinueWith
     * =========================================================================
     *
     * ContinueWith schedules follow-up work when the antecedent task finishes.
     *
     *   pickTask.ContinueWith(
     *       completed => Ship(completed.Result),
     *       TaskContinuationOptions.OnlyOnRanToCompletion);
     *
     * Options filter on status: OnlyOnRanToCompletion, OnlyOnFaulted, etc.
     *
     * In modern code, await (ch.04) replaces most ContinueWith chains because
     * it preserves exception stack traces and reads linearly.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateContinuations(Order sampleOrder)
    {
        Task<string> pickForContinuation = Task.Run(() => SimulatePick(sampleOrder.OrderId, delayMs: 35));

        Task<string> shipContinuation = pickForContinuation.ContinueWith(
            antecedent =>
            {
                string pickMessage = antecedent.Result;
                return $"Ship label created after: {pickMessage}";
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);

        string shipmentMessage = shipContinuation.Result;

        Console.WriteLine();
        Console.WriteLine($"--- Continuation: {shipmentMessage} ---");
    }

    /*
     * =========================================================================
     * SECTION 12: CHILD TASKS ATTACHED TO PARENT
     * =========================================================================
     *
     * By default, nested Task.Run inside a parent Task is independent — the
     * parent can finish before children. AttachedToParent links the child
     * lifetime to the parent:
     *
     *   Task.Factory.StartNew(() => {
     *       Task.Factory.StartNew(PickLine, AttachedToParent);
     *       Task.Factory.StartNew(PickLine, AttachedToParent);
     *   });
     *
     * Parent task status stays Running until all attached children complete.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateAttachedChildren(Order multiLineOrder)
    {
        Task fulfillmentParent = Task.Factory.StartNew(() =>
        {
            Console.WriteLine();
            Console.WriteLine($"--- Attached children: parent fulfillment started for #{multiLineOrder.OrderId} ---");

            foreach (string line in multiLineOrder.Lines)
            {
                Task.Factory.StartNew(
                    () =>
                    {
                        Thread.Sleep(25);
                        Console.WriteLine($"    child pick: {line}");
                    },
                    CancellationToken.None,
                    TaskCreationOptions.AttachedToParent,
                    TaskScheduler.Default);
            }
        });

        fulfillmentParent.Wait();
        Console.WriteLine($"--- Attached children: parent fulfillment finished for #{multiLineOrder.OrderId} ---");
    }

    /*
     * =========================================================================
     * SECTION 13: CONTROL TASK RESULT — TaskCompletionSource<T>
     * =========================================================================
     *
     * Sometimes YOU do not run the delegate — an external callback completes
     * the work (payment gateway, hardware event, legacy API). TaskCompletionSource
     * creates a Task you control:
     *
     *   TaskCompletionSource<PaymentResult> tcs = new();
     *   gateway.OnApproved += r => tcs.TrySetResult(r);
     *   gateway.OnDeclined += ex => tcs.TrySetException(ex);
     *   return tcs.Task;
     *
     * TrySet* returns false if the task already completed (avoid double-set).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskCompletionSource(Order order)
    {
        Task<PaymentResult> paymentTask = SimulatePaymentGatewayCallback(order.OrderId, approved: true);
        PaymentResult payment = paymentTask.Result;

        Console.WriteLine();
        Console.WriteLine($"--- TaskCompletionSource: payment {payment.Status} for order #{payment.OrderId} (txn {payment.TransactionId}) ---");
    }

    /*
     * =========================================================================
     * SECTION 14: CREATE SYNC WRAPPER USING Task
     * =========================================================================
     *
     * Legacy callers expect synchronous methods. Bridge task-based callback
     * code by blocking at the boundary:
     *
     *   public PaymentResult ChargeSync(int orderId)
     *   {
     *       return ChargeAsync(orderId).GetAwaiter().GetResult();
     *   }
     *
     * GetAwaiter().GetResult() unwraps AggregateException to the inner
     * exception (slightly cleaner than .Result for faulted tasks).
     *
     * WARNING: Only block at application edges (console Main, legacy sync API).
     * Blocking on ASP.NET or UI threads while waiting for pool work that needs
     * the same context causes deadlocks — ch.04 covers this in depth.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateSyncWrapper(Order order)
    {
        PaymentResult syncPayment = ChargeOrderSynchronously(order.OrderId);
        Console.WriteLine($"--- Sync wrapper: ChargeOrderSynchronously → #{syncPayment.OrderId} {syncPayment.Status} ---");
    }

    /*
     * =========================================================================
     * SECTION 15: TaskScheduler BASICS
     * =========================================================================
     *
     * A TaskScheduler decides which thread runs a queued Task.
     *
     *   TaskScheduler.Default    thread-pool scheduler — Task.Run uses this
     *   TaskScheduler.Current    scheduler of the calling context (may inline)
     *   TaskScheduler.FromCurrentSynchronizationContext()
     *                            posts to UI/ASP.NET sync context (ch.04)
     *
     * Task.Factory.StartNew without an explicit scheduler uses Current, which
     * is why Task.Run (always Default) is the safer default.
     *
     * --- 15a. Observe Default vs explicit scheduler on StartNew ---
     * --- 15b. Schedule continuation on Default explicitly ---
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskScheduler()
    {
        Console.WriteLine();
        Console.WriteLine("--- TaskScheduler: Default vs Current ---");
        Console.WriteLine($"  TaskScheduler.Default:  {TaskScheduler.Default.Id} (thread pool)");
        Console.WriteLine($"  TaskScheduler.Current:  {TaskScheduler.Current.Id} (context-dependent)");

        Task<int> scheduledOnDefault = Task.Factory.StartNew(
            () => Environment.CurrentManagedThreadId,
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default);

        int poolThreadId = scheduledOnDefault.Result;
        Console.WriteLine($"  StartNew with Default scheduler ran on thread {poolThreadId}");

        Task<string> antecedent = Task.Run(() => "pick complete");
        Task<string> continuation = antecedent.ContinueWith(
            t => $"continued: {t.Result}",
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        Console.WriteLine($"  ContinueWith on Default: {continuation.Result}");
    }

    /*
     * =========================================================================
     * SECTION 16: EXCEPTION HANDLING ON TASKS
     * =========================================================================
     *
     * Unhandled exceptions inside a Task delegate mark the task Faulted.
     * Observing the fault:
     *
     *   task.Wait()              throws AggregateException wrapping inner ex
     *   task.Result              same when task is Task<T>
     *   task.Exception           AggregateException or null if not faulted
     *   GetAwaiter().GetResult() unwraps to inner exception (no Aggregate wrapper)
     *
     * ContinueWith with OnlyOnFaulted runs cleanup/logging without throwing.
     *
     * Always observe task exceptions — an unobserved faulted task can tear
     * down the process via TaskScheduler.UnobservedTaskException (rare in demos).
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateTaskExceptions()
    {
        Console.WriteLine();
        Console.WriteLine("--- Exception handling on tasks ---");

        Task faultyTask = Task.Run(() =>
        {
            Thread.Sleep(20);
            throw new InvalidOperationException("Inventory slot empty");
        });

        try
        {
            faultyTask.Wait();
        }
        catch (AggregateException aggregate)
        {
            Console.WriteLine($"  Wait() caught AggregateException: {aggregate.InnerException?.Message}");
        }

        Console.WriteLine($"  faultyTask.IsFaulted={faultyTask.IsFaulted}, Exception type={faultyTask.Exception?.InnerException?.GetType().Name}");

        Task faultWithContinuation = Task.Run(() =>
        {
            throw new InvalidOperationException("Label printer offline");
        });

        Task logFault = faultWithContinuation.ContinueWith(
            antecedent =>
            {
                Exception? inner = antecedent.Exception?.Flatten().InnerException;
                Console.WriteLine($"  OnlyOnFaulted continuation logged: {inner?.Message}");
            },
            TaskContinuationOptions.OnlyOnFaulted);

        logFault.Wait(); // observe continuation; antecedent exception already handled by continuation path
    }

    /*
     * =========================================================================
     * SECTION 17: CancellationToken — PREVIEW
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → 04. Async and Await
     *   (CancellationTokenSource, ThrowIfCancellationRequested, cancel async
     *    streams, linking tokens, cancel non-cancellable workarounds)
     *
     * Tasks accept CancellationToken on Task.Run and cooperative checks inside
     * the delegate:
     *
     *   token.ThrowIfCancellationRequested();
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateCancellationPreview()
    {
        using CancellationTokenSource previewCts = new CancellationTokenSource();
        previewCts.CancelAfter(50);

        Task previewCancelTask = Task.Run(() =>
        {
            for (int step = 0; step < 20; step++)
            {
                previewCts.Token.ThrowIfCancellationRequested();
                Thread.Sleep(20);
            }
        }, previewCts.Token);

        try
        {
            previewCancelTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
            Console.WriteLine();
            Console.WriteLine("--- CancellationToken preview: long pick canceled cooperatively (detail in ch.04) ---");
        }
    }

    /*
     * =========================================================================
     * SECTION 18: async VS Task — PREVIEW
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → 04. Async and Await
     *
     *   async Task<int> FetchCountAsync() { … await Task.Delay(100); … }
     *
     * An async method returns Task / Task<T> immediately; the compiler rewrites
     * the body into a state machine that resumes after await points. Task is
     * the runtime contract; async/await is syntactic sugar that composes tasks
     * without blocking threads.
     *
     * This chapter uses Task.Run and blocking .Wait()/.Result in console Main
     * so you see the underlying primitives before ch.04 adds await.
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateAsyncVsTaskPreview()
    {
        Task<int> taskShapedWork = Task.Run(() =>
        {
            Thread.Sleep(30);
            return 42;
        });

        int value = taskShapedWork.Result;
        Console.WriteLine($"--- async vs Task preview: manual Task<int> returned {value}; async/await wraps this in ch.04 ---");
    }

    /*
     * =========================================================================
     * SECTION 19: ValueTask — PREVIEW
     * =========================================================================
     *
     * COVERED IN DETAIL LATER → 04. Async and Await + 08. Advanced C# Features
     *   (ValueTask vs Task allocation, when to return ValueTask, IAsyncEnumerable)
     *
     * ValueTask / ValueTask<T> reduce allocations when async methods often
     * complete synchronously (cached result, fast path). Full async methods
     * in ch.04 typically return Task<T>; libraries may expose ValueTask<T>
     * for hot paths.
     *
     * Signature preview only — no async/await syntax in this chapter:
     *
     *   ValueTask<OrderStatus> GetStatusCachedAsync(int id);
     * -------------------------------------------------------------------------
     */
    private static void DemonstrateValueTaskPreview()
    {
        Console.WriteLine("--- ValueTask preview: allocation-friendly async return type (ch.04 / Advanced C# 8) ---");
    }

    // --- Pipeline simulation helpers (used by demo methods above) ---

    private static string SimulatePick(int orderId, int delayMs)
    {
        Thread.Sleep(delayMs);
        return $"Picked order #{orderId}";
    }

    private static decimal CalculateShippingQuote(Order order)
    {
        Thread.Sleep(25);
        return Math.Round(6.00m + order.Lines.Count * 1.50m, 2);
    }

    private static bool ValidateOrder(Order order)
    {
        Thread.Sleep(20);
        return order.Total > 0 && order.Lines.Count > 0;
    }

    private static (string Carrier, decimal Price, int Ms) SimulateCarrierQuote(
        string carrier,
        decimal basePrice,
        int delayMs)
    {
        Thread.Sleep(delayMs);
        return (carrier, basePrice, delayMs);
    }

    private static string ThrottledPick(Order order, SemaphoreSlim gate)
    {
        gate.Wait();
        try
        {
            int threadId = Environment.CurrentManagedThreadId;
            Thread.Sleep(60);
            return $"Order #{order.OrderId} picked on thread {threadId}";
        }
        finally
        {
            gate.Release();
        }
    }

    private static Task<PaymentResult> SimulatePaymentGatewayCallback(int orderId, bool approved)
    {
        TaskCompletionSource<PaymentResult> tcs = new TaskCompletionSource<PaymentResult>();

        Task.Run(() =>
        {
            Thread.Sleep(80);
            if (approved)
            {
                PaymentResult result = new PaymentResult(orderId, "Approved", Guid.NewGuid().ToString("N")[..8]);
                tcs.TrySetResult(result);
            }
            else
            {
                tcs.TrySetException(new InvalidOperationException("Payment declined"));
            }
        });

        return tcs.Task;
    }

    private static PaymentResult ChargeOrderSynchronously(int orderId)
    {
        Task<PaymentResult> paymentTask = SimulatePaymentGatewayCallback(orderId, approved: true);
        return paymentTask.GetAwaiter().GetResult();
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — TASKS AND TPL
 * =========================================================================
 *
 * --- Start work ---
 *
 *   Task.Run(() => Work());              // preferred — queues to pool
 *   Task.Run(() => 42);                  // Task<int>
 *   Task.FromResult(42);                 // already completed Task<int>
 *   Task.Factory.StartNew(...);         // only when you need specific options
 *
 * --- Wait / results (blocking — edge use only) ---
 *
 *   task.Wait();                         // void Task
 *   task.WaitAll(tasks);
 *   int x = taskOfInt.Result;            // blocks; deadlock risk on UI/ASP.NET
 *   int x = taskOfInt.GetAwaiter().GetResult();
 *
 * --- Combine tasks ---
 *
 *   Task.WhenAll(t1, t2, t3);             // all must finish
 *   Task.WhenAny(t1, t2);                  // first finish wins
 *
 * --- Throttle concurrency ---
 *
 *   using var gate = new SemaphoreSlim(max, max);
 *   gate.Wait(); try { work; } finally { gate.Release(); }
 *
 * --- Continuations ---
 *
 *   task.ContinueWith(t => FollowUp(t.Result),
 *       TaskContinuationOptions.OnlyOnRanToCompletion);
 *
 * --- Attached child ---
 *
 *   Task.Factory.StartNew(child, token,
 *       TaskCreationOptions.AttachedToParent, TaskScheduler.Default);
 *
 * --- External completion ---
 *
 *   var tcs = new TaskCompletionSource<T>();
 *   tcs.TrySetResult(value);
 *   tcs.TrySetException(ex);
 *   tcs.TrySetCanceled();
 *   Task<T> task = tcs.Task;
 *
 * --- TaskScheduler ---
 *
 *   TaskScheduler.Default                         // thread pool
 *   TaskScheduler.Current                         // context scheduler
 *   TaskScheduler.FromCurrentSynchronizationContext()  // UI / ASP.NET
 *
 * --- Exceptions ---
 *
 *   task.IsFaulted / task.Exception               // inspect without rethrow
 *   task.Wait()                                   // AggregateException
 *   GetAwaiter().GetResult()                      // unwraps inner exception
 *   ContinueWith(..., OnlyOnFaulted)              // fault handler chain
 *
 * --- Sync wrapper (legacy boundary only) ---
 *
 *   return DoAsync().GetAwaiter().GetResult();
 *
 * --- Task vs Thread ---
 *
 *   Thread     OS resource, manual lifecycle (ch.01)
 *   Task       work + status + result + composition
 *
 * --- Common mistakes ---
 *
 *  Mistake                              | Result
 *  -------------------------------------|----------------------------------
 *  .Result on UI / ASP.NET request      | Deadlock (ch.04)
 *  StartNew without Default scheduler   | Unexpected inline or wrong queue
 *  Forget Release on SemaphoreSlim      | Permanent throttle / leak
 *  SetResult twice on TCS               | TrySet* returns false; ignore or log
 *  Unattached nested Task.Run in parent | Parent may finish before children
 *  Ignore Faulted task                  | UnobservedTaskException / process exit
 *
 * --- Related chapters ---
 *
 *   01. Threads and Thread Lifecycle    Thread, Join, foreground/background
 *   02. ThreadPool                      QueueUserWorkItem, pool role
 *   04. Async and Await                 await, CancellationToken, deadlocks
 *   05. Parallel Programming            Parallel.For, PLINQ
 *   06. Synchronization and Locks       lock, Monitor, beyond SemaphoreSlim
 *
 * =========================================================================
 */
