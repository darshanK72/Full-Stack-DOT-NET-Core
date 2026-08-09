/*
 * =============================================================================
 * 06. SYNCHRONIZATION AND LOCKS — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: Protecting shared mutable state when multiple threads run at once —
 *        lock/Monitor, Mutex, counting semaphores, reader/writer locks,
 *        Interlocked atomics, volatile visibility, deadlocks, and best practices.
 *
 * WHY IT MATTERS:
 *   A community bank processes deposits and withdrawals on several teller
 *   windows at once. Without synchronization, two threads can read the same
 *   balance, both apply a change, and one update is lost — the ledger no
 *   longer matches reality. The primitives here enforce exclusive, bounded,
 *   or atomic access so counters, balances, and rate tables stay consistent.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Why shared mutable state needs synchronization
 *   2.  Race conditions — broken counter without protection
 *   3.  lock statement — exclusive access and sync-root rules
 *   4.  Monitor.Enter/Exit and Monitor.TryEnter
 *   5.  Mutex — exclusive ownership (cross-process capable)
 *   6.  SemaphoreSlim and Semaphore — limit concurrent workers
 *   7.  ReaderWriterLockSlim — many readers, one writer
 *   8.  Interlocked — hardware-backed atomic operations
 *   9.  volatile — publish stop flags and simple visibility
 *   10. Deadlock — causes, hang pattern, prevention
 *   11. Synchronization best practices — when to use which primitive
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynchronizationAndLocks;

/*
 * =========================================================================
 * SECTION 1: WHY THREAD SYNCHRONIZATION IS NEEDED
 * =========================================================================
 *
 * Threads share the same process memory. When two threads update the same
 * field without coordination, their instructions interleave:
 *
 *   Thread A reads balance 100
 *   Thread B reads balance 100
 *   Thread A writes 100 + 50 = 150
 *   Thread B writes 100 - 30 = 70        ← lost A's deposit
 *
 * Even counter++ is NOT one atomic CPU instruction. The compiler emits
 * roughly: load → add → store. Another thread can slip between those steps.
 *
 * Synchronization ensures only one thread (or a bounded number) executes
 * a critical section that touches shared state at a time.
 * -------------------------------------------------------------------------
 */
public static class SyncIntro
{
    public static void PrintOverview()
    {
        Console.WriteLine("--- SECTION 1: Shared state and interleaving ---");
        Console.WriteLine("  Multiple tellers update the same ledger fields concurrently.");
        Console.WriteLine("  Without coordination, updates can be lost or reordered.");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 2: RACE CONDITION — COUNTER WITHOUT PROTECTION
 * =========================================================================
 *
 * Many workers each increment a shared transaction counter. Expected result:
 * workerCount × incrementsPerWorker. Without a lock the final value is often
 * lower because increments overwrite each other.
 *
 * Run several times — the wrong total varies; that non-determinism is the
 * hallmark of a data race.
 * -------------------------------------------------------------------------
 */
public static class RaceConditionDemo
{
    public static void RunUnsafe(int workerCount, int incrementsPerWorker, int expectedTotal)
    {
        int unsafeCounter = 0;
        Thread[] workers = StartWorkers(workerCount, () =>
        {
            for (int i = 0; i < incrementsPerWorker; i++)
            {
                unsafeCounter++; // read-modify-write — not atomic
                if (i % 50 == 0)
                {
                    Thread.Yield(); // encourage interleaving for the demo
                }
            }
        });

        JoinAll(workers);

        Console.WriteLine("--- SECTION 2: Race — no lock ---");
        Console.WriteLine($"  Expected transaction count: {expectedTotal}");
        Console.WriteLine($"  Actual (unsafe):            {unsafeCounter}");
        Console.WriteLine("  Lost updates occur when threads interleave read-modify-write.");
        Console.WriteLine();
    }

    private static Thread[] StartWorkers(int count, ThreadStart body)
    {
        Thread[] workers = new Thread[count];
        for (int i = 0; i < count; i++)
        {
            int workerIndex = i;
            workers[i] = new Thread(body) { Name = $"CounterWorker-{workerIndex + 1}" };
            workers[i].Start();
        }

        return workers;
    }

    private static void JoinAll(Thread[] workers)
    {
        foreach (Thread worker in workers)
        {
            worker.Join();
        }
    }
}

/*
 * =========================================================================
 * SECTION 3: lock STATEMENT — EXCLUSIVE ACCESS
 * =========================================================================
 *
 * lock (syncRoot) { … } is C# sugar for:
 *
 *   Monitor.Enter(syncRoot);
 *   try { … }
 *   finally { Monitor.Exit(syncRoot); }
 *
 * Rules:
 *   • Use ONE dedicated readonly object as the sync root per resource.
 *   • Do not lock on `this`, public types, or strings (interning surprises).
 *   • Keep critical sections short — no I/O or long work inside lock.
 *
 * BankAccount wraps balance mutations in lock so Deposit and Withdraw are
 * atomic relative to each other. Balance getter also locks for a consistent read.
 * -------------------------------------------------------------------------
 */
public sealed class BankAccount
{
    private readonly object _syncRoot = new object(); // private sync root — never lock(this)
    private decimal _balance;

    public BankAccount(string accountId, decimal initialBalance)
    {
        AccountId = accountId;
        _balance = initialBalance;
    }

    public string AccountId { get; }

    public object SyncRoot => _syncRoot; // expose for Monitor/Mutex demos that need the same root

    public decimal Balance
    {
        get
        {
            lock (_syncRoot)
            {
                return _balance;
            }
        }
    }

    public void Deposit(decimal amount)
    {
        lock (_syncRoot)
        {
            DepositInternal(amount);
        }
    }

    public void Withdraw(decimal amount)
    {
        lock (_syncRoot)
        {
            WithdrawInternal(amount);
        }
    }

    internal void DepositInternal(decimal amount) => _balance += amount;

    internal void WithdrawInternal(decimal amount) => _balance -= amount;
}

public static class LockDemo
{
    private static readonly object CounterLock = new object();

    public static void RunSafeCounter(int workerCount, int incrementsPerWorker, int expectedTotal)
    {
        int safeCounter = 0;
        Thread[] workers = new Thread[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            int workerIndex = i;
            workers[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerWorker; j++)
                {
                    lock (CounterLock)
                    {
                        safeCounter++;
                    }
                }
            })
            { Name = $"SafeWorker-{workerIndex + 1}" };

            workers[i].Start();
        }

        foreach (Thread worker in workers)
        {
            worker.Join();
        }

        Console.WriteLine("--- SECTION 3: lock fixes the counter ---");
        Console.WriteLine($"  Expected transaction count: {expectedTotal}");
        Console.WriteLine($"  Actual (locked):            {safeCounter}");
        Console.WriteLine();
    }

    public static void RunAccountMutations(BankAccount checking, BankAccount savings)
    {
        Thread depositThread = new Thread(() =>
        {
            for (int i = 0; i < 5; i++)
            {
                checking.Deposit(10m);
                Thread.Sleep(5);
            }
        })
        { Name = "DepositWorker" };

        Thread withdrawThread = new Thread(() =>
        {
            for (int i = 0; i < 3; i++)
            {
                checking.Withdraw(15m);
                Thread.Sleep(5);
            }
        })
        { Name = "WithdrawWorker" };

        depositThread.Start();
        withdrawThread.Start();
        depositThread.Join();
        withdrawThread.Join();

        Console.WriteLine("--- SECTION 3b: BankAccount with lock ---");
        Console.WriteLine($"  {checking.AccountId} balance: {checking.Balance:C} (500 + 5×10 − 3×15 = 505)");
        Console.WriteLine($"  {savings.AccountId} balance:  {savings.Balance:C}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 4: Monitor.Enter / Exit — EXPLICIT lock EQUIVALENT
 * =========================================================================
 *
 * Use explicit Monitor when you need logic before Enter or guaranteed Exit
 * in complex control flow. Prefer lock when the body is a simple block.
 *
 * --- 4a. Monitor.Enter / Exit ---
 *
 * --- 4b. Monitor.TryEnter — timeout instead of blocking forever ---
 *
 * TryEnter returns false if the lock is not acquired within the timeout.
 * Useful when waiting might deadlock — fail fast and retry or abort.
 * -------------------------------------------------------------------------
 */
public static class MonitorDemo
{
    private static readonly object CounterLock = new object();

    public static void RunEnterExit(BankAccount savings)
    {
        object savingsSync = savings.SyncRoot;
        Monitor.Enter(savingsSync);
        try
        {
            savings.Deposit(50m);
        }
        finally
        {
            Monitor.Exit(savingsSync); // always Exit in finally — same thread that Enter'd
        }

        Console.WriteLine("--- SECTION 4a: Monitor.Enter / Exit ---");
        Console.WriteLine($"  After monitored deposit: {savings.AccountId} balance {savings.Balance:C}");
        Console.WriteLine();
    }

    public static void RunTryEnter()
    {
        Thread lockHolder = new Thread(() =>
        {
            lock (CounterLock)
            {
                Thread.Sleep(300);
            }
        })
        { Name = "LockHolder" };

        lockHolder.Start();
        Thread.Sleep(50);

        bool tryEnterSucceeded = Monitor.TryEnter(CounterLock, millisecondsTimeout: 100);
        if (tryEnterSucceeded)
        {
            Monitor.Exit(CounterLock);
        }

        lockHolder.Join();

        Console.WriteLine("--- SECTION 4b: Monitor.TryEnter ---");
        Console.WriteLine($"  TryEnter(100 ms) while lock held: {(tryEnterSucceeded ? "acquired" : "timed out")}");
        Console.WriteLine("  Timed-out wait avoids blocking forever when a lock may never free.");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 5: Mutex — EXCLUSIVE OWNERSHIP
 * =========================================================================
 *
 * Mutex is like a lock but can be named for cross-process synchronization
 * (single-instance app). Rules:
 *
 *   • Only the thread that called WaitOne may call ReleaseMutex.
 *   • Heavier than Monitor — use for cross-process or when you need Mutex API.
 *
 * Here we guard audit log writes with a local Mutex.
 * -------------------------------------------------------------------------
 */
public static class MutexDemo
{
    private static readonly object AuditLogLock = new object();
    private static readonly StringBuilder AuditLog = new StringBuilder();

    public static void RunAuditLog()
    {
        using Mutex auditMutex = new Mutex(initiallyOwned: false, name: null);

        Thread[] auditWorkers =
        [
            new Thread(() => WriteAuditEntry(auditMutex, "Teller-1 opened session")) { Name = "Audit-1" },
            new Thread(() => WriteAuditEntry(auditMutex, "Teller-2 opened session")) { Name = "Audit-2" },
        ];

        foreach (Thread worker in auditWorkers)
        {
            worker.Start();
        }

        foreach (Thread worker in auditWorkers)
        {
            worker.Join();
        }

        Console.WriteLine("--- SECTION 5: Mutex — audit log ---");
        Console.WriteLine(AuditLog.ToString());
        Console.WriteLine();
    }

    private static void WriteAuditEntry(Mutex auditMutex, string message)
    {
        auditMutex.WaitOne();
        try
        {
            lock (AuditLogLock)
            {
                AuditLog.AppendLine($"  [{Thread.CurrentThread.Name}] {message} @ {DateTime.Now:HH:mm:ss.fff}");
            }
        }
        finally
        {
            auditMutex.ReleaseMutex(); // must be same thread that called WaitOne
        }
    }
}

/*
 * =========================================================================
 * SECTION 6: SemaphoreSlim AND Semaphore — LIMIT CONCURRENCY
 * =========================================================================
 *
 * A counting semaphore allows N threads into a region at once (not just 1).
 *
 *   SemaphoreSlim   lighter; prefer for in-process throttling (.NET 4+)
 *   Semaphore       OS kernel object; can be named cross-process
 *
 * --- 6a. SemaphoreSlim — three teller windows, five customers ---
 *
 * --- 6b. Semaphore (legacy/kernel) — same counting idea ---
 *
 * Semaphore(initialCount, maximumCount). WaitOne decrements; Release increments.
 * -------------------------------------------------------------------------
 */
public static class SemaphoreDemo
{
    public static void RunTellerWindows()
    {
        const int tellerWindows = 3;
        using SemaphoreSlim tellerSlots = new SemaphoreSlim(tellerWindows, tellerWindows);
        int customersServed = 0;
        object servedLock = new object();

        Thread[] customers =
        [
            new Thread(() => ServeCustomer(tellerSlots, ref customersServed, servedLock, "Customer-A")) { Name = "Cust-A" },
            new Thread(() => ServeCustomer(tellerSlots, ref customersServed, servedLock, "Customer-B")) { Name = "Cust-B" },
            new Thread(() => ServeCustomer(tellerSlots, ref customersServed, servedLock, "Customer-C")) { Name = "Cust-C" },
            new Thread(() => ServeCustomer(tellerSlots, ref customersServed, servedLock, "Customer-D")) { Name = "Cust-D" },
            new Thread(() => ServeCustomer(tellerSlots, ref customersServed, servedLock, "Customer-E")) { Name = "Cust-E" },
        ];

        foreach (Thread customer in customers)
        {
            customer.Start();
        }

        foreach (Thread customer in customers)
        {
            customer.Join();
        }

        Console.WriteLine("--- SECTION 6a: SemaphoreSlim (3 teller windows) ---");
        Console.WriteLine($"  Customers served: {customersServed} (max 3 concurrent)");
        Console.WriteLine();
    }

    public static void RunLegacyGate()
    {
        using Semaphore legacyGate = new Semaphore(initialCount: 2, maximumCount: 2);
        int gatePasses = 0;

        Thread[] gateWorkers =
        [
            new Thread(() => PassGate(legacyGate, ref gatePasses, "Gate-A")) { Name = "Gate-A" },
            new Thread(() => PassGate(legacyGate, ref gatePasses, "Gate-B")) { Name = "Gate-B" },
            new Thread(() => PassGate(legacyGate, ref gatePasses, "Gate-C")) { Name = "Gate-C" },
        ];

        foreach (Thread worker in gateWorkers)
        {
            worker.Start();
        }

        foreach (Thread worker in gateWorkers)
        {
            worker.Join();
        }

        Console.WriteLine("--- SECTION 6b: Semaphore (max 2 concurrent) ---");
        Console.WriteLine($"  Gate passes recorded: {gatePasses}");
        Console.WriteLine();
    }

    private static void ServeCustomer(SemaphoreSlim tellerSlots, ref int customersServed, object servedLock, string customerName)
    {
        tellerSlots.Wait(); // decrements available slots; blocks when zero
        try
        {
            Console.WriteLine($"  [{Thread.CurrentThread.Name}] {customerName} at teller window.");
            Thread.Sleep(120);
            lock (servedLock)
            {
                customersServed++;
            }
        }
        finally
        {
            tellerSlots.Release(); // returns a slot to the pool
        }
    }

    private static void PassGate(Semaphore gate, ref int gatePasses, string label)
    {
        gate.WaitOne();
        try
        {
            Console.WriteLine($"  [{Thread.CurrentThread.Name}] {label} inside gated region.");
            Thread.Sleep(80);
            Interlocked.Increment(ref gatePasses); // atomic increment — safe without lock
        }
        finally
        {
            gate.Release();
        }
    }
}

/*
 * =========================================================================
 * SECTION 7: ReaderWriterLockSlim — MANY READERS, ONE WRITER
 * =========================================================================
 *
 * Exclusive lock (lock/Monitor) blocks everyone — even read-only lookups.
 * ReaderWriterLockSlim allows:
 *
 *   EnterReadLock   many threads concurrently when no writer holds the lock
 *   EnterWriteLock  exclusive — blocks all readers and other writers
 *
 * Prefer for read-heavy shared caches (interest rates, fee schedules).
 * Always ExitReadLock / ExitWriteLock in finally. Dispose when done.
 *
 * COVERED IN DETAIL LATER → 07. Concurrent Collections (ConcurrentDictionary
 * avoids manual RW locks for many dictionary scenarios).
 * -------------------------------------------------------------------------
 */
public sealed class InterestRateTable
{
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
    private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>
    {
        ["CHK"] = 0.01m,
        ["SAV"] = 0.025m,
        ["CD"] = 0.045m,
    };

    public decimal GetRate(string productCode)
    {
        _rwLock.EnterReadLock();
        try
        {
            return _rates[productCode];
        }
        finally
        {
            _rwLock.ExitReadLock();
        }
    }

    public void UpdateRate(string productCode, decimal newRate)
    {
        _rwLock.EnterWriteLock();
        try
        {
            _rates[productCode] = newRate;
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }

    public int ReadRateConcurrently(string productCode, int readCount)
    {
        int completedReads = 0;
        Thread[] readers = new Thread[readCount];
        for (int i = 0; i < readCount; i++)
        {
            readers[i] = new Thread(() =>
            {
                decimal rate = GetRate(productCode); // shared read lock — parallel readers OK
                if (rate > 0m)
                {
                    Interlocked.Increment(ref completedReads);
                }
            })
            { Name = $"RateReader-{i + 1}" };

            readers[i].Start();
        }

        foreach (Thread reader in readers)
        {
            reader.Join();
        }

        return completedReads;
    }
}

public static class ReaderWriterLockDemo
{
    public static void RunRateTable()
    {
        InterestRateTable rates = new InterestRateTable();

        Thread writer = new Thread(() =>
        {
            Thread.Sleep(50);
            rates.UpdateRate("SAV", 0.0275m);
            Console.WriteLine($"  [{Thread.CurrentThread.Name}] updated SAV rate to 2.75%");
        })
        { Name = "RateWriter" };

        writer.Start();
        int readsCompleted = rates.ReadRateConcurrently("SAV", readCount: 6);
        writer.Join();

        Console.WriteLine("--- SECTION 7: ReaderWriterLockSlim ---");
        Console.WriteLine($"  Concurrent reads completed: {readsCompleted}");
        Console.WriteLine($"  Final SAV rate: {rates.GetRate("SAV"):P2}");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 8: Interlocked — HARDWARE-BACKED ATOMIC OPERATIONS
 * =========================================================================
 *
 * Interlocked performs atomic read-modify-write on a single location:
 *
 *   Increment, Decrement, Add
 *   Exchange           swap old for new, return old
 *   CompareExchange    if current == expected, swap to new; return actual
 *
 * When to use:
 *   One int/long counter or flag → Interlocked (no lock object overhead)
 *   Multiple fields or invariants  → lock
 *
 * Interlocked works on int, long, float, double, IntPtr, and object refs
 * (reference Exchange). It does NOT atomically update decimal — use lock.
 *
 * 05. Parallel Programming reuses these APIs inside Parallel.For loops.
 * -------------------------------------------------------------------------
 */
public static class InterlockedDemo
{
    public static void RunCounter(int workerCount, int incrementsPerWorker, int expectedTotal)
    {
        int atomicCounter = 0;
        Thread[] workers = new Thread[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            int workerIndex = i;
            workers[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerWorker; j++)
                {
                    Interlocked.Increment(ref atomicCounter);
                }
            })
            { Name = $"AtomicWorker-{workerIndex + 1}" };

            workers[i].Start();
        }

        foreach (Thread worker in workers)
        {
            worker.Join();
        }

        Console.WriteLine("--- SECTION 8a: Interlocked.Increment ---");
        Console.WriteLine($"  Expected: {expectedTotal}, Actual: {atomicCounter}");
        Console.WriteLine();

        int exchanged = Interlocked.Exchange(ref atomicCounter, 0); // swap to 0, return prior value
        Console.WriteLine("--- SECTION 8b: Interlocked.Exchange ---");
        Console.WriteLine($"  Prior counter value exchanged away: {exchanged}");
        Console.WriteLine($"  Counter after Exchange(..., 0): {atomicCounter}");
        Console.WriteLine();

        int compareSlot = 100;
        int observed = Interlocked.CompareExchange(ref compareSlot, 999, comparand: 100); // CAS succeeds
        Console.WriteLine("--- SECTION 8c: Interlocked.CompareExchange ---");
        Console.WriteLine($"  CompareExchange succeeded: slot={compareSlot}, returned={observed}");
        Console.WriteLine("  Pattern: lock-free update when only one field changes.");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 9: volatile — MEMORY VISIBILITY FOR SIMPLE FLAGS
 * =========================================================================
 *
 * Without volatile or a lock, one thread may cache a bool in a CPU register
 * and never see another thread's write — a worker loop may never stop.
 *
 * volatile tells the compiler/runtime: reads/writes go to shared memory;
 * do not cache the field in a register across reads.
 *
 * Use for:
 *   Simple stop/cancel flags set by one thread, read by another
 *
 * Do NOT use volatile for:
 *   Compound operations (counter++) — use Interlocked or lock
 *   Publishing complex object graphs — use lock or immutable snapshots
 *
 * 01. Threads and Thread Lifecycle previewed this pattern; full rules here.
 * -------------------------------------------------------------------------
 */
public sealed class VaultScanWorker
{
    private volatile bool _stopRequested; // visibility guarantee for the loop read

    public void Run(int scanIterations)
    {
        for (int i = 0; i < scanIterations && !_stopRequested; i++)
        {
            Thread.Sleep(40);
        }
    }

    public void RequestStop()
    {
        _stopRequested = true; // write visible to Run loop on another core
    }
}

public static class VolatileDemo
{
    public static void RunStopFlag()
    {
        VaultScanWorker worker = new VaultScanWorker();
        Thread scanThread = new Thread(() => worker.Run(scanIterations: 200)) { Name = "VaultScan" };

        scanThread.Start();
        Thread.Sleep(120);
        worker.RequestStop(); // cooperative stop — no Thread.Abort
        scanThread.Join();

        Console.WriteLine("--- SECTION 9: volatile stop flag ---");
        Console.WriteLine("  Scan thread exited after RequestStop() — loop saw _stopRequested.");
        Console.WriteLine("  For counter math use Interlocked; for flags use volatile or lock.");
        Console.WriteLine();
    }
}

/*
 * =========================================================================
 * SECTION 10: DEADLOCK — CAUSES AND PREVENTION
 * =========================================================================
 *
 * Deadlock: threads block each other in a circular wait for locks.
 *
 * Classic pattern (DO NOT RUN — hangs forever):
 *
 *   object lockA = new object(), lockB = new object();
 *
 *   Thread t1 = new Thread(() => {
 *       lock (lockA) {
 *           Thread.Sleep(50);
 *           lock (lockB) { Console.WriteLine("t1"); }
 *       }
 *   });
 *
 *   Thread t2 = new Thread(() => {
 *       lock (lockB) {
 *           Thread.Sleep(50);
 *           lock (lockA) { Console.WriteLine("t2"); }
 *       }
 *   });
 *
 *   t1.Start(); t2.Start(); t1.Join(); t2.Join();
 *
 * t1 holds A waits for B; t2 holds B waits for A — neither proceeds.
 *
 * Prevention strategies:
 *   • Acquire locks in a consistent global order (e.g. by account id)
 *   • Use Monitor.TryEnter with timeout and back off
 *   • Reduce lock scope; avoid nested locks when possible
 *
 * --- 10a. Safe transfer — lock accounts in id order ---
 * -------------------------------------------------------------------------
 */
public static class DeadlockDemo
{
    public static void RunOrderedTransfer(BankAccount accountAlpha, BankAccount accountBeta)
    {
        Thread transferAToB = new Thread(() => TransferSafely(accountAlpha, accountBeta, 40m)) { Name = "Xfer-A→B" };
        Thread transferBToA = new Thread(() => TransferSafely(accountBeta, accountAlpha, 25m)) { Name = "Xfer-B→A" };

        transferAToB.Start();
        transferBToA.Start();
        transferAToB.Join();
        transferBToA.Join();

        Console.WriteLine("--- SECTION 10: Deadlock prevention (ordered locks) ---");
        Console.WriteLine($"  {accountAlpha.AccountId} balance: {accountAlpha.Balance:C}");
        Console.WriteLine($"  {accountBeta.AccountId} balance:  {accountBeta.Balance:C}");
        Console.WriteLine("  TransferSafely locks lower AccountId first — no circular wait.");
        Console.WriteLine();
    }

    private static void TransferSafely(BankAccount from, BankAccount to, decimal amount)
    {
        BankAccount first = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
        BankAccount second = ReferenceEquals(first, from) ? to : from;

        lock (first.SyncRoot)
        {
            lock (second.SyncRoot)
            {
                if (from.Balance >= amount)
                {
                    from.WithdrawInternal(amount);
                    to.DepositInternal(amount);
                    Console.WriteLine(
                        $"  [{Thread.CurrentThread.Name}] transferred {amount:C} from {from.AccountId} to {to.AccountId}");
                }
            }
        }
    }
}

/*
 * =========================================================================
 * SECTION 11: SYNCHRONIZATION BEST PRACTICES
 * =========================================================================
 *
 *  Practice                              | Why
 *  --------------------------------------|----------------------------------
 *  Dedicated private readonly sync root  | Avoid lock(this) / lock(typeof(T))
 *  Short critical sections               | Less contention, fewer deadlocks
 *  One lock ordering for nested resources| Breaks circular wait
 *  Interlocked for single-field math     | Faster than lock for one counter
 *  ReaderWriterLockSlim for read-heavy   | Readers don't block each other
 *  SemaphoreSlim to cap concurrency      | Protect downstream resources
 *  TryEnter + timeout when order unclear | Fail fast instead of hang
 *  Prefer Concurrent collections (ch.07)  | Less hand-rolled locking
 *
 * This demo wires the practices above through BankAccount, Interlocked,
 * ReaderWriterLockSlim, ordered TransferSafely, and Monitor.TryEnter.
 * -------------------------------------------------------------------------
 */
public static class BestPracticesDemo
{
    public static void PrintSummary()
    {
        Console.WriteLine("--- SECTION 11: Best practices (see section comment above) ---");
        Console.WriteLine("  ✓ Private sync roots (BankAccount._syncRoot)");
        Console.WriteLine("  ✓ Interlocked for single counters; lock for multi-field invariants");
        Console.WriteLine("  ✓ ReaderWriterLockSlim for read-heavy rate table");
        Console.WriteLine("  ✓ Global lock order in TransferSafely");
        Console.WriteLine("  ✓ Monitor.TryEnter timeout instead of infinite wait");
        Console.WriteLine("  → Thread-safe collections: 07. Concurrent Collections");
        Console.WriteLine();
        Console.WriteLine("=== Demo complete ===");
    }
}

public class Program
{
    /*
     * SECTION 12: DEMONSTRATION — Main orchestrates the chapter demo
     *
     * Constants keep worker counts deterministic. Each Run* method maps to
     * the section comment blocks defined above in this file.
     * -------------------------------------------------------------------------
     */
    public static void Main(string[] args)
    {
        const int workerCount = 12;
        const int incrementsPerWorker = 2_000;
        const int expectedTotal = workerCount * incrementsPerWorker;

        Console.WriteLine("=== Riverview Community Bank — synchronization demo ===");
        Console.WriteLine($"Main thread id: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine();

        SyncIntro.PrintOverview();
        RaceConditionDemo.RunUnsafe(workerCount, incrementsPerWorker, expectedTotal);
        LockDemo.RunSafeCounter(workerCount, incrementsPerWorker, expectedTotal);

        BankAccount checking = new BankAccount("CHK-1001", initialBalance: 500m);
        BankAccount savings = new BankAccount("SAV-2002", initialBalance: 200m);
        LockDemo.RunAccountMutations(checking, savings);

        MonitorDemo.RunEnterExit(savings);
        MonitorDemo.RunTryEnter();
        MutexDemo.RunAuditLog();
        SemaphoreDemo.RunTellerWindows();
        SemaphoreDemo.RunLegacyGate();
        ReaderWriterLockDemo.RunRateTable();
        InterlockedDemo.RunCounter(workerCount, incrementsPerWorker, expectedTotal);
        VolatileDemo.RunStopFlag();

        BankAccount accountAlpha = new BankAccount("ACC-001", 300m);
        BankAccount accountBeta = new BankAccount("ACC-002", 300m);
        DeadlockDemo.RunOrderedTransfer(accountAlpha, accountBeta);

        BestPracticesDemo.PrintSummary();
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — SYNCHRONIZATION AND LOCKS
 * =========================================================================
 *
 * --- When to synchronize ---
 *
 *   Shared mutable fields updated by 2+ threads → race without protection
 *   counter++, balance +/-, dictionary writes from parallel threads
 *
 * --- lock (recommended default for in-process exclusive access) ---
 *
 *   private readonly object _sync = new object();
 *   lock (_sync) { sharedValue++; }
 *
 *   Equivalent to Monitor.Enter/Exit with try/finally
 *
 * --- Monitor ---
 *
 *   Monitor.Enter(obj); try { … } finally { Monitor.Exit(obj); }
 *   Monitor.TryEnter(obj, ms)  → false on timeout
 *
 * --- Mutex ---
 *
 *   using Mutex m = new Mutex(false, "Global\\MyApp");  // named = cross-process
 *   m.WaitOne(); try { … } finally { m.ReleaseMutex(); }
 *
 * --- SemaphoreSlim (in-process) ---
 *
 *   using SemaphoreSlim gate = new SemaphoreSlim(3, 3);
 *   gate.Wait();       // WaitAsync in 04. Async and Await
 *   try { … } finally { gate.Release(); }
 *
 * --- Semaphore (kernel / cross-process) ---
 *
 *   using Semaphore sem = new Semaphore(2, 2);
 *   sem.WaitOne(); … sem.Release();
 *
 * --- ReaderWriterLockSlim ---
 *
 *   _rw.EnterReadLock();  try { read } finally { _rw.ExitReadLock(); }
 *   _rw.EnterWriteLock(); try { write } finally { _rw.ExitWriteLock(); }
 *
 * --- Interlocked ---
 *
 *   Interlocked.Increment(ref n)     single-location atomic ++
 *   Interlocked.Add, Exchange, CompareExchange
 *   Not for decimal or multi-field invariants — use lock
 *
 * --- volatile ---
 *
 *   volatile bool _stop;   simple flag visibility across threads
 *   Not for counter++ — use Interlocked or lock
 *
 * --- Deadlock prevention ---
 *
 *   Lock ordering       always acquire A before B (e.g. sorted id)
 *   TryEnter + timeout  back off and retry
 *   Avoid holding lock while waiting on another lock
 *
 * --- Primitive choice ---
 *
 *  Need                          | Type
 *  ------------------------------|----------------------------------
 *  Exclusive access (1 at a time)| lock / Monitor / Mutex
 *  Bounded concurrency (N)       | SemaphoreSlim / Semaphore
 *  Many reads, rare writes       | ReaderWriterLockSlim
 *  Single int/long counter       | Interlocked
 *  Simple stop flag              | volatile (+ cooperative check)
 *  Cross-process                 | Named Mutex / Named Semaphore
 *
 * --- Common mistakes ---
 *
 *  Mistake                     | Result
 *  ----------------------------|------------------------------------------
 *  lock(this) or lock(typeof)  | External code can deadlock on same object
 *  Forgot Release/Exit         | Other threads block forever
 *  Nested locks opposite order | Deadlock (circular wait)
 *  Long work inside lock       | Throughput collapse; contention
 *  volatile on counter++       | Still racy — not atomic
 *
 * --- Related chapters ---
 *
 *   01. Threads and Thread Lifecycle    Thread creation, Join, lock preview
 *   04. Async and Await                 SemaphoreSlim.WaitAsync, async coordination
 *   05. Parallel Programming            Interlocked in Parallel.For, PLINQ
 *   07. Concurrent Collections            thread-safe collections without manual lock
 *
 * =========================================================================
 */
