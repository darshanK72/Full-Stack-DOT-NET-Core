# Synchronization and Locks — Interview Q&A


## Table of Contents

1. [Q1. What does the lock statement do and how is it implemented?](#q1-what-does-the-lock-statement-do-and-how-is-it-implemented)
2. [Q2. What should you use as a lock object and what should you avoid?](#q2-what-should-you-use-as-a-lock-object-and-what-should-you-avoid)
3. [Q3. What is Monitor and how does it differ from the lock statement?](#q3-what-is-monitor-and-how-does-it-differ-from-the-lock-statement)
4. [Q4. What is the difference between Mutex and lock?](#q4-what-is-the-difference-between-mutex-and-lock)
5. [Q5. What is SemaphoreSlim and when would you use it over Semaphore?](#q5-what-is-semaphoreslim-and-when-would-you-use-it-over-semaphore)
6. [Q6. What is ReaderWriterLockSlim and when should you use it?](#q6-what-is-readerwriterlockslim-and-when-should-you-use-it)
7. [Q7. What are ManualResetEvent and AutoResetEvent?](#q7-what-are-manualresetevent-and-autoresetevent)
8. [Q8. What is CountdownEvent and when is it useful?](#q8-what-is-countdownevent-and-when-is-it-useful)
9. [Q9. What is Barrier and how does it implement phased parallel algorithms?](#q9-what-is-barrier-and-how-does-it-implement-phased-parallel-algorithms)
10. [Q10. What are Interlocked operations and when should you use them?](#q10-what-are-interlocked-operations-and-when-should-you-use-them)
11. [Q11. What is the volatile keyword and how does it differ from Interlocked?](#q11-what-is-the-volatile-keyword-and-how-does-it-differ-from-interlocked)
12. [Q12. What is SpinLock and when is it appropriate?](#q12-what-is-spinlock-and-when-is-it-appropriate)
13. [Q13. How does lock-free programming with ConcurrentStack compare to lock-based approaches?](#q13-how-does-lock-free-programming-with-concurrentstack-compare-to-lock-based-approaches)
14. [Q14. What is the difference between lock-based and lockless approaches for a shared counter?](#q14-what-is-the-difference-between-lock-based-and-lockless-approaches-for-a-shared-counter)
15. [Q15. What is the SynchronizationContext and how does it relate to locking?](#q15-what-is-the-synchronizationcontext-and-how-does-it-relate-to-locking)
16. [Q16. How does inconsistent lock ordering cause a deadlock?](#q16-how-does-inconsistent-lock-ordering-cause-a-deadlock)
17. [Q17. What are the dangers of locking on `this` or `typeof(T)`?](#q17-what-are-the-dangers-of-locking-on-this-or-typeoft)
18. [Q18. What happens if you throw an exception inside a lock block?](#q18-what-happens-if-you-throw-an-exception-inside-a-lock-block)
19. [Q19. What is the missing Release in SemaphoreSlim?](#q19-what-is-the-missing-release-in-semaphoreslim)
20. [Q20. What is lock convoy and how does it affect high-throughput systems?](#q20-what-is-lock-convoy-and-how-does-it-affect-high-throughput-systems)
21. [Q21. What is the volatile not-enough-for-compound-operations trap?](#q21-what-is-the-volatile-not-enough-for-compound-operations-trap)
22. [Q22. Implement a thread-safe lazy singleton with double-checked locking.](#q22-implement-a-thread-safe-lazy-singleton-with-double-checked-locking)
23. [Q23. Build a read-write cache with ReaderWriterLockSlim.](#q23-build-a-read-write-cache-with-readerwriterlockslim)
24. [Q24. Diagnose a production deadlock from a thread dump.](#q24-diagnose-a-production-deadlock-from-a-thread-dump)
25. [Q25. Replace a lock-based counter with Interlocked for maximum throughput.](#q25-replace-a-lock-based-counter-with-interlocked-for-maximum-throughput)
26. [Q26. How do you build a rate limiter with SemaphoreSlim?](#q26-how-do-you-build-a-rate-limiter-with-semaphoreslim)
27. [Q27. Implement a phased parallel pipeline using Barrier.](#q27-implement-a-phased-parallel-pipeline-using-barrier)
28. [Q28. How do you choose between lock, SemaphoreSlim, and ReaderWriterLockSlim for a given scenario?](#q28-how-do-you-choose-between-lock-semaphoreslim-and-readerwriterlockslim-for-a-given-scenario)

---
## Foundation Questions

---

## Q1. What does the lock statement do and how is it implemented?

**Concepts**
- Exclusive mutual exclusion over a code section
- Syntactic sugar for Monitor.Enter/Exit
- try/finally ensures release on exception
- Lock object must be a reference type
- Only one thread at a time in the guarded section

**Answer**

The `lock` statement ensures that only one thread executes the protected code block at a time. Any other thread attempting to enter the block will block (wait) until the first thread exits. It is syntactic sugar for `Monitor.Enter` and `Monitor.Exit` wrapped in a `try/finally`:

```csharp
lock (_syncObject)
{
    // Only one thread here at a time
    _counter++;
}

// Compiled equivalent:
bool lockTaken = false;
try
{
    Monitor.Enter(_syncObject, ref lockTaken);
    _counter++;
}
finally
{
    if (lockTaken) Monitor.Exit(_syncObject);
}
```

The `ref lockTaken` overload introduced in .NET 4 handles the edge case where a `ThreadAbortException` (legacy) could occur between `Enter` and the `try` — if `Enter` succeeded but the exception fired before `lockTaken` was set, the `finally` would incorrectly try to exit a lock not held. The object used as the lock must be a reference type; value types cause a compile error because they would box to different objects each time.

---

## Q2. What should you use as a lock object and what should you avoid?

**Concepts**
- Use: private readonly object _lock = new object()
- Avoid: `this` (accessible externally — deadlock risk)
- Avoid: `typeof(T)` (shared across all instances and assemblies)
- Avoid: string literals (interned — unexpected sharing)
- Avoid: value types (boxing creates different objects each time)

**Answer**

The lock object should be a `private readonly object` field dedicated to synchronization:

```csharp
private readonly object _lock = new object();

public void Update(int value)
{
    lock (_lock) { _data = value; }
}
```

**Do not use `this`**: any code with a reference to your object can lock on it externally, potentially creating deadlocks with code you do not control. **Do not use `typeof(MyClass)`**: type objects are shared across all instances of the class and even across AppDomains in some scenarios. **Do not use string literals**: the CLR interns string literals, meaning `"lock"` in one assembly and `"lock"` in another may be the same object, creating unintended cross-assembly locking. **Do not use value types**: locking on an `int` would box it, and each `lock` statement would box to a different object — no mutual exclusion would occur.

The dedicated private object pattern is clear about intent, has no unintended sharing, and is easy to reason about.

---

## Q3. What is Monitor and how does it differ from the lock statement?

**Concepts**
- Monitor.Enter(obj) / Monitor.Exit(obj)
- Monitor.TryEnter(obj, timeout) — non-blocking attempt
- Monitor.Wait(obj) — releases lock and waits for Pulse
- Monitor.Pulse(obj) / PulseAll(obj) — signal waiting threads
- lock statement is Monitor without Wait/Pulse/TryEnter

**Answer**

`Monitor` is the class underlying the `lock` statement. The `lock` statement is equivalent to `Monitor.Enter` + `try/finally` + `Monitor.Exit` but omits `TryEnter`, `Wait`, and `Pulse` functionality.

`Monitor.TryEnter(obj, timeout)` attempts to acquire the lock without blocking indefinitely, returning `false` if the timeout elapses. This is valuable for avoiding deadlocks or implementing non-blocking try-lock patterns.

`Monitor.Wait(obj)` atomically releases the lock and suspends the thread, waiting for a `Pulse`. When woken, it re-acquires the lock before returning. `Monitor.Pulse(obj)` wakes one waiting thread; `Monitor.PulseAll(obj)` wakes all. This implements condition variables — a powerful pattern for producer-consumer synchronization:

```csharp
// Producer:
lock (_queue)
{
    _queue.Enqueue(item);
    Monitor.Pulse(_queue); // wake waiting consumer
}

// Consumer:
lock (_queue)
{
    while (_queue.Count == 0)
        Monitor.Wait(_queue); // release lock, wait for Pulse, re-acquire
    var item = _queue.Dequeue();
}
```

The `while` (not `if`) around `Monitor.Wait` is essential — spurious wakeups can occur and the condition must be re-checked.

---

## Q4. What is the difference between Mutex and lock?

**Concepts**
- lock: in-process only, fast (kernel-mode not required for uncontested lock)
- Mutex: cross-process synchronization via named kernel object
- Mutex: slower (always involves kernel)
- Mutex: thread affinity — must be released by same thread
- Named Mutex for single-instance application enforcement

**Answer**

`lock` uses the CLR's `Monitor` class which, on modern Windows, uses an "optimistic spin" approach before falling back to a kernel event — uncontested locks avoid the kernel entirely and are extremely fast. However, `Monitor`/`lock` is in-process only — it cannot coordinate between separate processes.

`Mutex` is a kernel synchronization object. Creating a named `Mutex` with the same name across multiple processes creates a shared lock:

```csharp
// Single-instance application pattern
using var mutex = new Mutex(true, "MyApp-SingleInstance", out bool createdNew);
if (!createdNew)
{
    Console.WriteLine("Another instance is already running.");
    return;
}
// Run application...
```

`Mutex` has thread affinity: the thread that calls `WaitOne()` must call `ReleaseMutex()`. Calling `ReleaseMutex()` from a different thread throws `ApplicationException`. This is a significant footgun compared to `lock`, where any thread in the same process can use the object. For in-process synchronization, always prefer `lock` over `Mutex` due to performance and simplicity.

---

## Q5. What is SemaphoreSlim and when would you use it over Semaphore?

**Concepts**
- SemaphoreSlim: user-mode, in-process, async-capable (WaitAsync)
- Semaphore: kernel-mode, cross-process, no async wait
- Initial count and max count
- WaitAsync(CancellationToken) for non-blocking async wait
- Release(int) to release multiple slots at once

**Answer**

`SemaphoreSlim` is a lightweight semaphore that works entirely in user mode (no kernel transitions for uncontested waits), supports `async/await` via `WaitAsync()`, and is scoped to a single process. It allows up to `N` threads (or tasks) to enter a section simultaneously, where `N` is the initial count.

```csharp
// Allow at most 5 concurrent database connections
private readonly SemaphoreSlim _dbSemaphore = new SemaphoreSlim(5, 5);

public async Task<Data> QueryAsync(CancellationToken ct)
{
    await _dbSemaphore.WaitAsync(ct);
    try
    {
        return await _db.ExecuteQueryAsync(ct);
    }
    finally
    {
        _dbSemaphore.Release();
    }
}
```

`Semaphore` (the full kernel object) supports cross-process coordination and can be named, but it does not have `WaitAsync` and is slower for in-process use. Choose `SemaphoreSlim` for all in-process concurrency limiting, especially in async code. The `finally { Release(); }` is mandatory — forgetting it permanently exhausts the semaphore.

---

## Q6. What is ReaderWriterLockSlim and when should you use it?

**Concepts**
- Multiple concurrent readers OR one exclusive writer
- EnterReadLock / ExitReadLock
- EnterWriteLock / ExitWriteLock
- EnterUpgradeableReadLock / ExitUpgradeableReadLock
- RecursionPolicy for nested lock acquisitions
- Replace with reader-writer lock only when reads >> writes

**Answer**

`ReaderWriterLockSlim` allows many threads to read simultaneously (shared access) but requires exclusive access for writing. This is appropriate when reads are frequent and fast, writes are rare, and holding a write lock for reads is unnecessarily restrictive.

```csharp
private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();
private Dictionary<string, int> _cache = new Dictionary<string, int>();

public int Read(string key)
{
    _rwLock.EnterReadLock();
    try { return _cache.TryGetValue(key, out int v) ? v : -1; }
    finally { _rwLock.ExitReadLock(); }
}

public void Write(string key, int value)
{
    _rwLock.EnterWriteLock();
    try { _cache[key] = value; }
    finally { _rwLock.ExitWriteLock(); }
}
```

`EnterUpgradeableReadLock` is used when you need to read, then potentially upgrade to a write lock — only one thread can hold an upgradeable read lock at a time, preventing the check-then-act race condition.

Use `ReaderWriterLockSlim` over a plain `lock` when: read operations vastly outnumber writes and reads are somewhat expensive (otherwise, the overhead of the reader-writer lock itself may exceed the gain). For simple cases or when reads are fast, a plain `lock` with `Dictionary` is often sufficient and simpler.

---

## Q7. What are ManualResetEvent and AutoResetEvent?

**Concepts**
- ManualResetEvent: stays signaled until Reset() — releases all waiting threads
- AutoResetEvent: auto-resets after releasing one thread
- WaitOne(), Set(), Reset()
- ManualResetEventSlim: user-mode version of ManualResetEvent
- Initial state: signaled (true) or unsignaled (false)

**Answer**

Both are signaling primitives that allow one thread to signal one or more waiting threads. They differ in what happens after a `Set()`.

`AutoResetEvent` releases exactly one waiting thread and then immediately resets to unsignaled. It is like a turnstile — each `Set()` lets exactly one thread through. Useful for passing a single token or work item from producer to consumer.

`ManualResetEvent` stays signaled after `Set()` until `Reset()` is called. All threads waiting on `WaitOne()` are released simultaneously, and subsequent `WaitOne()` calls return immediately until `Reset()` is called. Useful for "start all workers" broadcast signals.

```csharp
// AutoResetEvent: one producer, one consumer per signal
var gate = new AutoResetEvent(false);
// Producer: gate.Set(); // releases one consumer
// Consumer: gate.WaitOne(); // blocks until Set

// ManualResetEvent: broadcast "ready" to all workers
var readySignal = new ManualResetEventSlim(false);
// All workers: readySignal.Wait();
// Coordinator: readySignal.Set(); // releases all workers simultaneously
```

`ManualResetEventSlim` (user-mode) is preferred over `ManualResetEvent` (kernel) for in-process use. It uses spin-wait before falling back to a kernel wait, making it faster for short waits.

---

## Q8. What is CountdownEvent and when is it useful?

**Concepts**
- Initial count; WaitOne blocks until count reaches zero
- Signal() decrements the count
- AddCount() increases the count (before it reaches zero)
- TryAddCount() for safe increment when count might already be zero
- Fork-join parallel pattern

**Answer**

`CountdownEvent` allows a thread to wait for multiple other threads or operations to complete — like a latch. Initialize with a count equal to the number of workers; each worker calls `Signal()` when done; the waiting thread calls `Wait()` and is released when the count reaches zero.

```csharp
var countdown = new CountdownEvent(workers.Length);

foreach (var worker in workers)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        try { DoWork(); }
        finally { countdown.Signal(); } // decrement count
    });
}

countdown.Wait(); // blocks until all workers have signaled
Console.WriteLine("All workers complete");
```

`CountdownEvent` is reusable via `Reset()` and supports dynamic addition of work with `AddCount()` before the count reaches zero. This is useful for hierarchical parallel work where top-level work spawns sub-work dynamically. For Task-based code, `Task.WhenAll` is usually simpler and should be preferred. `CountdownEvent` is most valuable when working with raw threads or `ThreadPool.QueueUserWorkItem` where returning a `Task` is not natural.

---

## Q9. What is Barrier and how does it implement phased parallel algorithms?

**Concepts**
- Synchronizes multiple threads at phase boundaries
- SignalAndWait() — signals completion, waits for all participants
- Phase number increments when all participants signal
- Post-phase action executed once between phases
- Participant count can be changed dynamically

**Answer**

`Barrier` is used when multiple threads must all complete a phase before any of them can proceed to the next phase — like a checkpoint in a parallel algorithm. All participants call `SignalAndWait()` at the phase boundary; the call blocks until all participants have signaled, then all are released simultaneously for the next phase.

```csharp
int phaseCount = 3;
int workerCount = 4;
var barrier = new Barrier(workerCount, b =>
    Console.WriteLine($"Phase {b.CurrentPhaseNumber} complete"));

Parallel.For(0, workerCount, workerId =>
{
    for (int phase = 0; phase < phaseCount; phase++)
    {
        // Each worker does its phase work
        DoPhaseWork(workerId, phase);

        // Wait for all workers to complete this phase
        barrier.SignalAndWait();
        // Now all workers are synchronized and begin next phase
    }
});
```

The post-phase action (lambda in the constructor) runs once between phases on the last thread to signal — useful for intermediate aggregation or validation. `Barrier` is ideal for iterative algorithms like parallel sorting (where each pass must complete before the next), simulation steps, or iterative numerical methods.

---

## Q10. What are Interlocked operations and when should you use them?

**Concepts**
- Atomic read-modify-write operations without a lock
- Increment, Decrement, Add, Exchange, CompareExchange, Read (64-bit)
- No lock overhead for simple counter/flag operations
- Memory barrier included (full fence semantics)
- CompareExchange for lock-free algorithms

**Answer**

`Interlocked` provides atomic operations on shared variables without acquiring a lock. These operations are implemented with CPU atomic instructions (LOCK prefix on x86), making them much faster than `Monitor.Enter`/`Exit` for simple operations.

```csharp
private int _counter = 0;

// Thread-safe increment:
Interlocked.Increment(ref _counter);

// Thread-safe compare-and-swap:
int original = Interlocked.CompareExchange(ref _state, newValue, expectedValue);
bool success = original == expectedValue; // true if swap occurred

// Thread-safe 64-bit read on 32-bit systems:
long value = Interlocked.Read(ref _longField);
```

`CompareExchange` is the foundation of lock-free algorithms: it atomically checks that a variable equals `expected` and, if so, replaces it with `desired`, returning the original value. This is the building block for optimistic concurrency patterns, lock-free stacks, and CAS (compare-and-swap) loops.

Use `Interlocked` for: counters, flags, and single-variable state transitions. For multi-variable state that must be consistent, use a `lock` instead — `Interlocked` cannot atomically update two fields simultaneously.

---

## Q11. What is the volatile keyword and how does it differ from Interlocked?

**Concepts**
- volatile: prevents compiler/JIT/CPU reordering of reads and writes
- Ensures a field is read from memory, not from a CPU register cache
- Does NOT make compound operations atomic
- Interlocked: atomic operations with memory barrier
- volatile is sufficient for flag variables; Interlocked for read-modify-write

**Answer**

The `volatile` keyword tells the compiler and JIT not to cache the field in a CPU register and to emit memory barriers on every read and write. Without `volatile`, the compiler may optimize a polling loop by reading the field once and reusing the register value:

```csharp
private volatile bool _shutdown = false;

// Thread 1:
void WorkerLoop()
{
    while (!_shutdown) // reads fresh value each iteration due to volatile
        DoWork();
}

// Thread 2:
void Shutdown() => _shutdown = true; // write is immediately visible
```

Without `volatile`, the JIT might hoist `_shutdown` out of the loop and `Thread 1` would never see the update. `volatile` ensures visibility.

However, `volatile` does NOT make operations atomic. `_counter++` on a `volatile int` is still three non-atomic steps. If you need atomic increment, use `Interlocked.Increment`. The rule: use `volatile` only for simple flag variables where one thread writes and others read, and the only required guarantee is visibility (not atomicity). For anything involving compound operations, use `Interlocked` or `lock`.

---

## Q12. What is SpinLock and when is it appropriate?

**Concepts**
- Busy-wait loop instead of kernel sleep
- No kernel transition — very low overhead for short contention windows
- CPU wasteful if contention is prolonged
- struct (not class) — do not copy
- SpinWait for adaptive spinning

**Answer**

`SpinLock` is a mutual-exclusion primitive that busy-waits (spins) rather than blocking. Instead of releasing the CPU to run other threads, the waiting thread executes a tight loop checking if the lock is free. This avoids the overhead of a kernel context switch (~10 microseconds), making it faster than `Monitor` for critical sections that hold the lock for a very short time (microseconds).

```csharp
private SpinLock _spinLock = new SpinLock(enableThreadOwnerTracking: false);

public void Update(int value)
{
    bool taken = false;
    try
    {
        _spinLock.Enter(ref taken);
        _data = value;
    }
    finally
    {
        if (taken) _spinLock.Exit();
    }
}
```

`SpinLock` is a `struct` — never copy it (assign to a variable, pass by value) or you copy the state and get two independent spinlocks. Always pass by `ref` or keep it in a fixed location. `SpinWait` is a lower-level utility that adaptively transitions from CPU spinning to `Thread.Yield` to kernel sleep as time passes, which is appropriate for unknown contention durations. For most application code, `lock` (Monitor) is correct and more forgiving; `SpinLock` is for high-performance library code where lock hold times are provably microseconds.

---

## Q13. How does lock-free programming with ConcurrentStack compare to lock-based approaches?

**Concepts**
- ConcurrentStack<T> uses CompareExchange for lock-free push/pop
- ABA problem in lock-free structures
- No kernel involvement — purely CPU-atomic operations
- Lock-free does not mean contention-free
- Use concurrent collections over hand-rolled lock-free code

**Answer**

Lock-free data structures use `Interlocked.CompareExchange` loops (CAS loops) to update shared state atomically without locks. `ConcurrentStack<T>` implements `Push` as:

```
head = _head;
newNode.Next = head;
// Atomically replace _head with newNode IF _head is still head:
while (Interlocked.CompareExchange(ref _head, newNode, head) != head)
{
    head = _head; // another thread modified; retry
    newNode.Next = head;
}
```

This is lock-free (no thread can hold a lock indefinitely, blocking others) but not contention-free — under high contention, threads retry many times. The ABA problem arises when another thread replaces the node at `head` with a different node that happens to have the same address. `ConcurrentStack` avoids this via node-per-operation allocation.

In practice: use `ConcurrentStack<T>`, `ConcurrentQueue<T>`, and `ConcurrentDictionary<TK,TV>` from the BCL rather than writing your own lock-free code. These are tested, correct, and handle edge cases. Hand-rolled lock-free code is notoriously difficult to get right and should only be written by experts with formal correctness analysis.

---

## Q14. What is the difference between lock-based and lockless approaches for a shared counter?

**Concepts**
- lock: guaranteed correct, overhead per operation
- Interlocked.Increment: atomic, no lock, faster
- volatile: visibility only — not atomic for increment
- Concurrent access without synchronization: undefined behavior
- Benchmark: measure actual difference before optimizing

**Answer**

For a simple counter shared across threads, there are three approaches with different performance characteristics:

```csharp
private int _counter = 0;
private readonly object _lock = new object();

// Option 1: lock — correct, ~20-50ns per operation under contention
void IncrementLocked() { lock (_lock) _counter++; }

// Option 2: Interlocked — correct, ~5-15ns, no kernel transition
void IncrementAtomic() => Interlocked.Increment(ref _counter);

// Option 3: volatile — WRONG for increment (not atomic)
private volatile int _bad; // do not use volatile for read-modify-write

// Option 4: no synchronization — WRONG, data race, undefined behavior
void IncrementUnsafe() => _counter++; // lost updates, cache incoherence
```

`Interlocked.Increment` is correct and significantly faster than `lock` for single-variable increments because it maps directly to the `LOCK XADD` instruction. However, for multi-step operations (increment A and B atomically), `lock` is required.

Measure before optimizing: the difference between `lock` and `Interlocked` is often irrelevant in applications where the bottleneck is I/O or business logic, and the simpler `lock` is easier to review and understand.

---

## Q15. What is the SynchronizationContext and how does it relate to locking?

**Concepts**
- Post/Send to marshal work to a specific thread
- UI thread (STA) as an implicit lock on all UI operations
- SynchronizationContext.Current captured by await
- DispatcherSynchronizationContext in WPF
- Not a locking mechanism per se, but provides serialization

**Answer**

`SynchronizationContext` is an abstraction that allows posting work to a specific execution context, most commonly a single UI thread. In WPF, `DispatcherSynchronizationContext` ensures all posted callbacks run on the UI thread — effectively serializing all UI access, which is why you don't need `lock` to access WPF controls (as long as you always access them on the UI thread).

While not a lock in the traditional sense, `SynchronizationContext` provides a form of cooperative serialization: all UI work is funneled through a single queue, executed by a single thread, one item at a time. This eliminates data races on UI state without explicit locks.

For ASP.NET Core, there is no `SynchronizationContext`. Each request is handled on a ThreadPool thread; if multiple threads access the same request's data, you must use explicit locks. The `async/await` machinery interacts with `SynchronizationContext` by capturing it before suspension and posting continuations back to it — which is why UI code can access controls after `await` without `Dispatcher.Invoke`.

---

## Gotchas — Synchronization and Locks (Interview Traps)

---

#### Gotcha 1. Locking on `this` or a Public Object Invites External Deadlocks

**Concepts**
- `this` is accessible to all callers — any external code can lock on the same object
- External lock held while calling a method that also locks `this` = deadlock
- `typeof(T)` is shared across all instances and AppDomains — same risk at class scope
- Fix: lock on a private readonly object field only visible inside the class
- Static methods: private static readonly object _staticLock = new object()

**Answer**

`lock(this)` exposes the lock object to all callers, because `this` is the instance reference they already hold. Any external code can write `lock(myInstance)` and contend with the class's internal lock, or cause a deadlock by holding an external lock on the instance while calling a method that also locks `this`. `lock(typeof(T))` is even broader — it is shared across all instances and across AppDomains. Always lock on a `private readonly object` field that is completely invisible outside the class, eliminating any possibility of external code interfering with the internal lock protocol.

---

#### Gotcha 2. Inconsistent Lock Ordering Causes Deadlock

**Concepts**
- Thread A acquires lockX then lockY
- Thread B acquires lockY then lockX
- Circular wait: each holds what the other needs
- .NET has no automatic deadlock detection
- Fix: establish a consistent global ordering for all lock acquisitions

**Answer**

When two threads acquire the same two locks in opposite orders, each holds a lock the other needs, creating a circular wait that neither can break — a deadlock. .NET provides no automatic deadlock detection; the symptom is the application freezing with 100% thread-wait time and no CPU activity. The fix is a global lock ordering rule: always acquire `lockA` before `lockB` in every code path. When ordering is impossible to impose (e.g., locks are acquired dynamically), use `Monitor.TryEnter` with a timeout to detect and recover from potential deadlocks.

---

#### Gotcha 3. Monitor.Enter Without try/finally Leaks the Lock on Exception

**Concepts**
- If an exception is thrown between Monitor.Enter and Monitor.Exit, the lock is never released
- All other threads waiting on that lock block forever
- The `lock` keyword compiles to Monitor.Enter inside try/finally — the lock is always released
- Explicitly calling Monitor.Enter/Exit (without lock) requires manual try/finally
- Never call Monitor.Enter/Exit manually; use the lock keyword

**Answer**

The `lock` statement compiles to `Monitor.Enter(obj)` inside a `try` block with `Monitor.Exit(obj)` in the `finally`. This guarantees the lock is released even if an exception is thrown. If you call `Monitor.Enter` manually (e.g., to use `Monitor.TryEnter`) and forget to wrap it in `try/finally`, any exception between entry and exit leaks the lock permanently — all threads waiting to acquire it will block forever. Always use the `lock` keyword for simple mutual exclusion; use `Monitor.TryEnter` with `try/finally` only when the timeout feature is specifically needed.

---

#### Gotcha 4. volatile Guarantees Visibility — Not Atomicity

**Concepts**
- volatile prevents compiler/CPU reordering and ensures fresh reads
- count++ is three operations: read, add, write — not atomic even with volatile
- Two threads can read the same value, compute the same result, both write it — lost update
- Interlocked.Increment is the correct atomic increment
- Use volatile only for single-read/single-write flag patterns (bool, reference fields)

**Answer**

`volatile` guarantees that reads and writes are not cached in registers and are not reordered by the compiler or CPU. It does not make compound operations atomic. `_count++` on a `volatile int` still consists of three separate steps — read, add, write — and two threads can interleave these steps to lose an update. Replace `_count++` with `Interlocked.Increment(ref _count)`, which is a single atomic CPU instruction. Remove the `volatile` keyword when using `Interlocked` — `Interlocked` methods include the necessary memory barriers.

---

#### Gotcha 5. Interlocked.CompareExchange Is the Foundation of Lock-Free CAS

**Concepts**
- CompareExchange(ref location, newValue, comparand): atomically sets location to newValue only if it equals comparand
- Returns the original value before the exchange
- Used to build optimistic lock-free updates (spin until CAS succeeds)
- Non-trivial to compose correctly — ABA problem is a known pitfall
- Prefer Interlocked.Increment/Add for simple scalars; CAS for custom update logic

**Answer**

`Interlocked.CompareExchange` performs an atomic compare-and-swap: it reads the current value, compares it to an expected value, and writes the new value only if the comparison succeeds — all in one uninterruptible CPU instruction. The return value is the original value before the swap. This is the primitive underlying all lock-free data structures and spin-update loops: read the current value, compute the new value, attempt CAS, retry the loop if CAS failed (another thread changed the value). The ABA problem — where a value is changed from A to B and back to A between read and CAS — can cause CAS to succeed incorrectly; mitigate with version stamps or `Interlocked.CompareExchange` on a `long` combining value + version.

---

#### Gotcha 6. ReaderWriterLockSlim Upgradeable Read Lock Recursion Causes Deadlock

**Concepts**
- EnterUpgradeableReadLock allows upgrade to write lock from within a read
- Only one thread can hold the upgradeable read lock at a time
- If that thread also tries to enter a normal read lock (recursion), it deadlocks
- LockRecursionPolicy.SupportsRecursion enables recursion but adds overhead
- Prefer simple lock for most scenarios; use RWLS only for high read-to-write ratios

**Answer**

`ReaderWriterLockSlim` allows concurrent reads and exclusive writes. The upgradeable read lock allows a single thread to hold a read lock and later upgrade it to a write lock atomically. However, if the thread holding an upgradeable read lock recursively tries to enter a normal read lock (default recursion policy is `NoRecursion`), it deadlocks because the normal read lock cannot be acquired while the upgradeable lock is held. Additionally, only one thread can hold the upgradeable read lock at a time, so multiple "readers that may write" still serialize on it. `ReaderWriterLockSlim` is beneficial only when reads dominate (>95%) and write operations are rare and brief.

---

#### Gotcha 7. SemaphoreSlim.Wait() Blocks a Thread — WaitAsync() Does Not

**Concepts**
- SemaphoreSlim.Wait() occupies a ThreadPool thread for the entire wait
- SemaphoreSlim.WaitAsync() releases the thread during the wait, resumes via continuation
- Mixing sync and async waits on the same semaphore is safe but unusual
- Always use WaitAsync() in async code to avoid pool thread starvation
- Forgetting Release() after WaitAsync() permanently decrements the count

**Answer**

`SemaphoreSlim.Wait()` blocks the calling thread synchronously, holding a pool slot for the entire wait duration. `WaitAsync()` returns a `Task` that completes when the semaphore is acquired — the thread is released during the wait and a pool thread picks up the continuation afterward. In async code that uses `SemaphoreSlim` for rate-limiting or bounded concurrency, always use `await semaphore.WaitAsync(cancellationToken)` inside `try/finally` with `Release()` in the finally block. Using `Wait()` in async code wastes pool threads and can cause pool exhaustion under load.

---

#### Gotcha 8. SpinLock Is Only Beneficial for Microsecond-Duration Critical Sections

**Concepts**
- SpinLock avoids kernel context switch by busy-waiting in a CPU loop
- Beneficial only when the wait is shorter than a context switch (~1–10 μs)
- For longer critical sections, spinning wastes CPU while blocking achieves the same result
- SpinLock is a struct — must be passed by ref; copying it breaks it
- In most .NET code, lock keyword is correct; SpinLock only for ultra-low-latency paths

**Answer**

`SpinLock` repeatedly checks whether the lock is free in a tight CPU loop, avoiding the overhead of a kernel context switch. This is faster than `lock` only when the critical section completes in a few microseconds — shorter than the ~1–10 μs cost of a context switch. For critical sections longer than that, spinning wastes CPU cycles that could be used by other threads. An important structural trap: `SpinLock` is a `struct`. Copying it (passing by value, assigning to a new variable) creates an independent copy that does not share lock state — always store and pass `SpinLock` by `ref`.

---

#### Gotcha 9. Mutex Is Cross-Process but Significantly Slower Than lock

**Concepts**
- Mutex is a kernel object — acquire/release crosses to kernel mode
- lock (Monitor) is user-mode — no kernel call in the uncontended case
- Mutex is ~30× slower than lock for uncontended acquisition
- Use Mutex only when cross-process synchronization is genuinely required
- Named mutexes allow cross-process exclusive access (single-instance pattern)

**Answer**

`Mutex` and `lock` both provide mutual exclusion, but `Mutex` is a kernel-mode object requiring a user-to-kernel context switch on every acquire and release. `lock` (backed by `Monitor`) operates entirely in user space when uncontended, making it orders of magnitude faster. The only legitimate use of `Mutex` in .NET code is cross-process synchronization — for example, ensuring only one instance of an application runs, or coordinating access to a named shared resource across processes. For single-process mutual exclusion, always use `lock` or `SemaphoreSlim`.

---

#### Gotcha 10. Lock Cannot Be Awaited — Using lock with async Code Requires SemaphoreSlim

**Concepts**
- lock keyword cannot contain await — compiler error
- Awaiting inside a lock would hold the lock across thread switches
- Lock is not owned by a thread in async contexts — Monitor.Enter/Exit must be on the same thread
- SemaphoreSlim(1, 1) is the async-compatible mutual exclusion primitive
- Always use WaitAsync() + try/finally + Release() pattern

**Answer**

The `lock` keyword cannot contain `await` — the compiler rejects it. The reason is fundamental: `Monitor` (which backs `lock`) is thread-affine — the thread that calls `Monitor.Enter` must be the same thread that calls `Monitor.Exit`. After an `await`, the continuation may resume on a different ThreadPool thread, making it impossible to call `Monitor.Exit` on the correct thread. The async-compatible replacement is `SemaphoreSlim(1, 1)` — a semaphore with a maximum count of 1 behaves as a mutex. Use `await semaphore.WaitAsync()` inside `try/finally` with `semaphore.Release()` in the finally block to ensure the semaphore is always released.

---

## Real-World Scenarios

---

## Q22. Implement a thread-safe lazy singleton with double-checked locking.

**Concepts**
- Double-checked locking pattern
- volatile for visibility of instance field
- Lazy<T> as the modern preferred alternative
- Lazy<T> LazyThreadSafetyMode.ExecutionAndPublication
- Race condition in naïve singleton implementation

**Answer**

The classic double-checked locking pattern requires `volatile` to prevent the CPU from reordering object construction and the assignment to `_instance`:

```csharp
public sealed class Singleton
{
    private static volatile Singleton? _instance;
    private static readonly object _lock = new object();

    private Singleton() { }

    public static Singleton Instance
    {
        get
        {
            if (_instance == null) // first check — avoids lock on every call
            {
                lock (_lock)
                {
                    if (_instance == null) // second check — prevents double init
                        _instance = new Singleton();
                }
            }
            return _instance;
        }
    }
}
```

Without `volatile`, the CPU could reorder: allocate memory → assign `_instance` → run constructor. Another thread reading `_instance` after the assignment but before the constructor runs would get a partially initialized object.

The modern idiomatic alternative using `Lazy<T>`:

```csharp
public sealed class Singleton
{
    private static readonly Lazy<Singleton> _lazy =
        new Lazy<Singleton>(() => new Singleton(),
            LazyThreadSafetyMode.ExecutionAndPublication);

    private Singleton() { }

    public static Singleton Instance => _lazy.Value;
}
```

`Lazy<T>` with `ExecutionAndPublication` guarantees exactly-once initialization and is clearer than double-checked locking. Use `Lazy<T>` for new code; understand double-checked locking for interviews and legacy codebase maintenance.

---

## Q23. Build a read-write cache with ReaderWriterLockSlim.

**Concepts**
- EnterReadLock for cache reads (many concurrent)
- EnterWriteLock for cache updates (exclusive)
- EnterUpgradeableReadLock for read-then-conditional-write
- try/finally for every lock/unlock pair
- Dispose the lock when cache is disposed

**Answer**

```csharp
public sealed class ReadWriteCache<TKey, TValue> : IDisposable
{
    private readonly Dictionary<TKey, TValue> _store = new();
    private readonly ReaderWriterLockSlim _lock =
        new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    public bool TryGet(TKey key, out TValue? value)
    {
        _lock.EnterReadLock();
        try { return _store.TryGetValue(key, out value); }
        finally { _lock.ExitReadLock(); }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_store.TryGetValue(key, out var existing))
                return existing; // read path — no write lock needed

            var value = factory(key);
            _lock.EnterWriteLock();
            try { _store[key] = value; }
            finally { _lock.ExitWriteLock(); }
            return value;
        }
        finally { _lock.ExitUpgradeableReadLock(); }
    }

    public void Set(TKey key, TValue value)
    {
        _lock.EnterWriteLock();
        try { _store[key] = value; }
        finally { _lock.ExitWriteLock(); }
    }

    public void Dispose() => _lock.Dispose();
}
```

`EnterUpgradeableReadLock` is used in `GetOrAdd` to prevent the check-then-write race condition: between checking the cache miss and acquiring the write lock, no other thread can enter an upgradeable read lock (only regular read locks can coexist). The factory is called outside the write lock to minimize contention — the factory may be slow (e.g., a database call). `ReaderWriterLockSlim` must be disposed when done to release its internal kernel event.

---

## Q24. Diagnose a production deadlock from a thread dump.

**Concepts**
- dotnet-dump collect + analyze
- clrthreads command to list managed threads
- clrstack to inspect each thread's call stack
- syncblk to find Monitor lock owners and waiters
- Circular dependency pattern in stacks

**Answer**

A production deadlock where the application hangs is diagnosed with a memory dump and managed debugging tools:

**Step 1: Capture dump**
```
dotnet-dump collect -p <pid> -o deadlock.dmp
dotnet-dump analyze deadlock.dmp
```

**Step 2: List threads and their states**
```
> clrthreads
ThreadCount: 12
...
ThreadID  State  ManagedId  ...
0x1234    2      5          Dead
0x5678    8200   6          Wait  (8200 = WaitSleepJoin)
0x9abc    8200   7          Wait
```

**Step 3: Inspect stacks of waiting threads**
```
> setthread 6
> clrstack
...
System.Threading.Monitor.Enter(Object, Boolean&)
MyService.ProcessOrder(Int32)
```

```
> setthread 7
> clrstack
...
System.Threading.Monitor.Enter(Object, Boolean&)
MyService.UpdateInventory(Int32)
```

**Step 4: Identify lock owners**
```
> syncblk
Index SyncBlock MonitorHeld Recursion Owning Thread
  42  ...       1           1         0x5678  (Thread 6 holds _orderLock)
  43  ...       1           1         0x9abc  (Thread 7 holds _inventoryLock)
```

Thread 6 holds `_orderLock` and waits for `_inventoryLock`. Thread 7 holds `_inventoryLock` and waits for `_orderLock`. Classic circular deadlock. Fix: always acquire locks in the same order in both methods.

---

## Q25. Replace a lock-based counter with Interlocked for maximum throughput.

**Concepts**
- Interlocked.Increment/Decrement for counters
- Interlocked.Add for delta increments
- Interlocked.CompareExchange for conditional update
- CQRS: measure before optimizing
- Per-thread counters aggregated at read time for highest throughput

**Answer**

```csharp
// BEFORE: lock-based counter — correct but adds contention
public class LockBasedStats
{
    private long _requestCount = 0;
    private long _errorCount = 0;
    private readonly object _lock = new();

    public void RecordRequest() { lock (_lock) _requestCount++; }
    public void RecordError() { lock (_lock) _errorCount++; }
    public (long Requests, long Errors) GetStats()
    {
        lock (_lock) return (_requestCount, _errorCount);
    }
}

// AFTER: lock-free with Interlocked — same correctness, less contention
public class LockFreeStats
{
    private long _requestCount = 0;
    private long _errorCount = 0;

    public void RecordRequest() => Interlocked.Increment(ref _requestCount);
    public void RecordError() => Interlocked.Increment(ref _errorCount);

    // Note: two reads are not atomically consistent with each other
    public (long Requests, long Errors) GetStats()
        => (Interlocked.Read(ref _requestCount),
            Interlocked.Read(ref _errorCount));
}
```

For even higher throughput under extreme contention (millions of increments/second), use `System.Threading.Channels` or thread-local counters aggregated at read time — eliminating shared-state writes entirely. The `LockFreeStats.GetStats()` method is not fully consistent (two separate `Interlocked.Read` calls are not atomic with each other), which is acceptable for monitoring but not for transactional correctness.

---

## Q26. How do you build a rate limiter with SemaphoreSlim?

**Concepts**
- SemaphoreSlim as token bucket
- Timed slot release with Task.Delay
- WaitAsync for non-blocking rate limit enforcement
- CancellationToken for shutdown
- Window-based vs sliding window design

**Answer**

```csharp
public sealed class SemaphoreRateLimiter : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _window;
    private readonly CancellationTokenSource _cts = new();

    public SemaphoreRateLimiter(int maxPerWindow, TimeSpan window)
    {
        _semaphore = new SemaphoreSlim(maxPerWindow, maxPerWindow);
        _window = window;
    }

    public async Task<bool> TryAcquireAsync(CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);
        if (!await _semaphore.WaitAsync(0, linked.Token)) // non-blocking check
            return false;

        // Auto-release after window expires
        _ = Task.Delay(_window, _cts.Token)
            .ContinueWith(t =>
            {
                if (!t.IsCanceled) _semaphore.Release();
            }, TaskContinuationOptions.NotOnCanceled);

        return true;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
        _semaphore.Dispose();
        await Task.CompletedTask;
    }
}
```

Each successful `TryAcquireAsync` acquires a semaphore slot. After `_window` elapses, the slot is released automatically via a background `Task.Delay` continuation. The semaphore count represents "remaining capacity in the current window." `WaitAsync(0, ...)` with timeout 0 is a non-blocking try-acquire. For production-grade rate limiting, prefer `System.Threading.RateLimiting` from .NET 7+ (`TokenBucketRateLimiter`) which handles edge cases (queue overflow, burst semantics) more robustly.

---

## Q27. Implement a phased parallel pipeline using Barrier.

**Concepts**
- Barrier with fixed participant count
- SignalAndWait at each phase boundary
- Post-phase action for inter-phase aggregation
- Multiple phases in a pipeline
- Exception propagation from post-phase action

**Answer**

```csharp
public void RunPhasedPipeline(int[][] data, int threads)
{
    int[] partialSums = new int[threads];
    int[] roundResults = new int[3]; // one per phase

    var barrier = new Barrier(threads, b =>
    {
        // Post-phase action: aggregate results after each phase
        roundResults[b.CurrentPhaseNumber] = partialSums.Sum();
        Array.Clear(partialSums, 0, partialSums.Length);
        Console.WriteLine($"Phase {b.CurrentPhaseNumber} total: {roundResults[b.CurrentPhaseNumber]}");
    });

    Parallel.For(0, threads, threadId =>
    {
        int[] myPartition = data[threadId];

        // Phase 0: filter
        var filtered = myPartition.Where(x => x > 0).ToArray();
        partialSums[threadId] = filtered.Sum();
        barrier.SignalAndWait(); // wait for all threads to finish filtering

        // Phase 1: transform
        var transformed = filtered.Select(x => x * 2).ToArray();
        partialSums[threadId] = transformed.Sum();
        barrier.SignalAndWait(); // wait for all threads to finish transforming

        // Phase 2: save
        SavePartition(threadId, transformed);
        partialSums[threadId] = transformed.Length;
        barrier.SignalAndWait(); // final synchronization
    });

    Console.WriteLine($"Pipeline complete. Saved {roundResults[2]} items.");
}
```

Each call to `SignalAndWait()` blocks the thread until all `threads` participants have called it for the current phase. The post-phase action (runs once per phase, on the last thread to arrive) aggregates partial sums. Writing to `partialSums[threadId]` is safe because each thread writes to its own index — no synchronization needed. Phases progress atomically: all threads complete phase N before any begin phase N+1.

---

## Q28. How do you choose between lock, SemaphoreSlim, and ReaderWriterLockSlim for a given scenario?

**Concepts**
- lock: simple exclusive access, one thread at a time
- SemaphoreSlim: N concurrent threads, async-friendly
- ReaderWriterLockSlim: many readers OR one writer
- Contention level and access pattern determine choice
- Measure before choosing complex synchronization

**Answer**

The right primitive depends on the access pattern:

**Use `lock`** when: you need simple mutual exclusion for a short critical section, you are protecting a multi-step operation, or you need `Monitor.Wait/Pulse` for condition signaling. It is the default choice for any exclusive state modification.

**Use `SemaphoreSlim`** when: multiple threads should access a resource concurrently up to a limit N (connection pools, API rate limiting), or when you need `async/await`-compatible waiting (`WaitAsync`). `lock` cannot be used with async code — `SemaphoreSlim` is the async-safe alternative.

**Use `ReaderWriterLockSlim`** when: reads are significantly more frequent than writes, reads are somewhat expensive (not just a field read), and holding a write lock for reads is a measurable bottleneck. Profile first — for most scenarios, a plain `lock` is simpler and fast enough.

```
Access Pattern          | Recommended Primitive
------------------------|----------------------
Exclusive modification  | lock
Async critical section  | SemaphoreSlim(1, 1)
N-concurrent access     | SemaphoreSlim(N, N)
Read-heavy, rare writes | ReaderWriterLockSlim
Single atomic op        | Interlocked
Flag/signal             | ManualResetEventSlim
Fan-join completion     | CountdownEvent / Task.WhenAll
Phase synchronization   | Barrier
```

As a rule: start with `lock`, measure, and upgrade to a more specialized primitive only when profiling shows contention is a bottleneck.
