# 06. Multithreading & Async Programming — Interview Q&A
> Back to [README](../README.md)

## Module Index — Subfolder Q&A Files

Each subfolder has its own focused INTERVIEW_QA.md with 12–18 Foundation questions, 4–6 Gotchas, and 5–8 Real-World Scenarios.

| # | Topic | File |
|---|-------|------|
| 01 | Threads & Thread Lifecycle | [INTERVIEW_QA.md](01.%20Threads%20%26%20Thread%20Lifecycle/INTERVIEW_QA.md) |
| 02 | ThreadPool | [INTERVIEW_QA.md](02.%20ThreadPool/INTERVIEW_QA.md) |
| 03 | Tasks & Task Parallel Library | [INTERVIEW_QA.md](03.%20Tasks%20%26%20Task%20Parallel%20Library/INTERVIEW_QA.md) |
| 04 | Async and Await | [INTERVIEW_QA.md](04.%20Async%20and%20Await/INTERVIEW_QA.md) |
| 05 | Parallel Programming | [INTERVIEW_QA.md](05.%20Parallel%20Programming/INTERVIEW_QA.md) |
| 06 | Synchronization and Locks | [INTERVIEW_QA.md](06.%20Synchronization%20and%20Locks/INTERVIEW_QA.md) |
| 07 | Concurrent Collections | [INTERVIEW_QA.md](07.%20Concurrent%20Collections/INTERVIEW_QA.md) |

---

## Cross-Cutting Questions — Spanning the Full Module

---

### CQ1. How do Thread, ThreadPool, Task, async/await, and Parallel relate to each other — when do you reach for each?

**Concepts**
- Thread: OS thread, manual lifecycle, STA/priority control
- ThreadPool: reusable threads, foundation for Task and async I/O
- Task: logical work unit, composable, cancellable, awaitable
- async/await: non-blocking I/O via state machine, suspends without holding threads
- Parallel: data parallelism, CPU-bound partitioning over ThreadPool
- Vertical stack: async/await → Task → ThreadPool → Thread (most→least abstraction)

**Answer**

These abstractions form a vertical stack, each building on the one below. `Thread` is the OS-level resource. `ThreadPool` manages a pool of threads to amortize creation cost. `Task` is a logical unit of work that is scheduled on the ThreadPool by default. `async/await` is a compiler transformation that suspends a method at I/O boundaries and schedules the continuation on the ThreadPool (or back on a `SynchronizationContext`), without holding any thread during the wait. `Parallel` is a data-parallelism abstraction that partitions collections across ThreadPool threads for CPU-bound work.

The guidance for choosing: for I/O-bound operations (HTTP, database, disk), use `async/await` — no threads are held during the wait. For CPU-bound parallel work over collections, use `Parallel.For`/`ForEach` or PLINQ. For one-off CPU-bound work, use `Task.Run`. For long-running dedicated work (minutes to hours), use `Task.Factory.StartNew` with `LongRunning` or `new Thread` with `IsBackground = true`. For cross-process synchronization or STA COM interop, use `new Thread` directly. In modern C# (.NET 10), raw `Thread` usage is increasingly rare — `Task`, `async/await`, and `Channel<T>` cover the vast majority of concurrency needs.

---

### CQ2. How do CancellationToken, SynchronizationContext, and AsyncLocal<T> work together in a request-scoped async pipeline?

**Concepts**
- CancellationToken: cooperative stop signal flowing down the call chain
- SynchronizationContext: thread-affinity for continuation scheduling
- AsyncLocal<T>: per-execution-context ambient values flowing with async continuations
- Request lifecycle: token from HttpContext; context from DI; correlation id via AsyncLocal
- Together: structured async with proper cancellation, marshaling, and context flow

**Answer**

In an ASP.NET Core request pipeline, these three mechanisms cooperate to give each request a self-contained execution context. `CancellationToken` (from `HttpContext.RequestAborted`) propagates the request's lifecycle downward through every service call — when the client disconnects, every layer can observe the cancellation cooperatively and stop work. `SynchronizationContext` in ASP.NET Core is `null` by default, meaning `await` continuations run on any ThreadPool thread, enabling high throughput without UI-style thread affinity. `AsyncLocal<T>` (used internally by `IHttpContextAccessor`, `Activity.Current`, and `ILogger`'s scopes) flows ambient context — like a correlation ID — across thread hops in the async call chain without passing it explicitly through every method signature.

The interplay: when a continuation runs on a different ThreadPool thread after `await`, the `CancellationToken` remains valid (it is a struct carried in parameters), `AsyncLocal` values are restored from the captured execution context, and `SynchronizationContext` determines where the continuation is posted. Misusing any of these — forgetting to pass the token, calling `.Result` while a `SynchronizationContext` is captured, or using `ThreadLocal<T>` instead of `AsyncLocal<T>` for request context — breaks the isolation and correctness of the pipeline.

---

### CQ3. How do you choose between lock, SemaphoreSlim, Channel<T>, and concurrent collections for shared state in an async service?

**Concepts**
- lock: exclusive synchronous access (not usable inside async methods)
- SemaphoreSlim.WaitAsync(): async-compatible exclusive or N-concurrent access
- Channel<T>: async producer-consumer with built-in backpressure and lifecycle
- ConcurrentDictionary/Queue: individual-operation thread safety, no async wait
- Design principle: prefer message-passing (Channel) over shared mutable state

**Answer**

The choice depends on the access pattern and whether the code is async. `lock` cannot be held across an `await` — attempting it will either fail to compile or (in rare cases) release the lock prematurely. For any section that must remain exclusive while doing async work, use `SemaphoreSlim(1,1)` with `WaitAsync()`: it provides the same mutual exclusion as `lock` but releases the thread during the wait.

For N-concurrent access (e.g., at most 5 DB connections), use `SemaphoreSlim(N, N)`. For read-heavy state, `ReaderWriterLockSlim` with `EnterReadLock`/`EnterWriteLock` reduces contention — though it lacks an async API and should be paired with `Task.Run` if reads are I/O-bound. For individual dictionary or queue operations in a multithreaded context, `ConcurrentDictionary<TK,TV>` or `ConcurrentQueue<T>` are the right choice. For producer-consumer pipelines where producers and consumers run at different rates, `Channel<T>` is the modern best practice — it provides async backpressure, bounded capacity, and clean completion semantics without manual synchronization.

The deeper principle: prefer message-passing and data ownership (via `Channel<T>` or `ImmutableDictionary`) over shared mutable state protected by locks. When state must be shared and mutated, the lock hierarchy should be clear and lock scope should be minimal.

---

### CQ4. Walk through all the ways an async/await operation can go wrong — from ThreadPool starvation to deadlock to silent exception loss.

**Concepts**
- ThreadPool starvation: blocking pool threads with .Result/.Wait()
- Deadlock: .Result on SynchronizationContext-captured thread
- Silent exception loss: async void, unobserved Task, fire-and-forget
- Context issues: ConfigureAwait(false) missing in library, or present in UI code
- Variable capture: closure captures loop variable by reference

**Answer**

Async/await failures cluster into five categories. (1) **ThreadPool starvation**: calling `.Result` or `.Wait()` on a `Task` inside an async pipeline blocks a pool thread for the duration of the awaited I/O. Under load, all pool threads block simultaneously; the hill-climbing algorithm injects replacements at ~1/500ms — too slowly, causing cascading timeouts. Fix: `await` all the way through. (2) **Deadlock**: in WPF or ASP.NET Classic, `SynchronizationContext` is captured at `await`; calling `.Result` on the captured context thread means the continuation can never post back — circular wait. Fix: `async` all the way, or `ConfigureAwait(false)` in library code. (3) **Silent exception loss**: `async void` posts exceptions to the `SynchronizationContext` and crashes the process; unobserved faulted tasks silently vanish. Fix: return `async Task`, never `async void` outside event handlers; observe all tasks. (4) **Context misuse**: `ConfigureAwait(false)` in UI code means the continuation runs on a ThreadPool thread and crashes on UI element access; missing it in a library causes an avoidable context switch and potential deadlock. Fix: library code always uses `ConfigureAwait(false)`; UI event handlers never do. (5) **Variable capture**: loop-started tasks capture the loop variable by reference — all tasks may see the final loop value. Fix: copy to a local before capturing in the lambda.

---

### CQ5. How do you design a graceful shutdown for a .NET service that has background threads, async hosted services, and Channel-based pipelines?

**Concepts**
- IHostedService.StopAsync receives CancellationToken (shutdown deadline)
- CancellationTokenSource linked to shutdown token
- Channel.Writer.Complete() to stop pipeline consumers
- Thread.Join with timeout for raw background threads
- Flush and drain before returning from StopAsync

**Answer**

A .NET service shutdown requires all components to observe the cancellation signal, finish in-flight work, and clean up before the host process exits. The `IHostedService.StopAsync(CancellationToken stoppingToken)` method provides a cancellation token that fires when shutdown is requested (typically SIGTERM, Ctrl+C, or `Environment.Exit`). The token has a deadline — ASP.NET Core's default shutdown timeout is 30 seconds.

The correct sequence: (1) Signal all production loops to stop: call `_cancellationTokenSource.Cancel()`, which flows into any `Channel<T>` writer's `WriteAsync(item, ct)` and all `await Task.Delay(..., ct)` retry loops. (2) Complete any `Channel<T>` writers: `channel.Writer.Complete()` so consumer `ReadAllAsync()` loops drain and exit. (3) Await in-flight tasks: `await Task.WhenAll(allBackgroundTasks)` with a timeout via `Task.WhenAny(..., Task.Delay(timeout, CancellationToken.None))`. (4) Join any raw background threads: `thread.Join(TimeSpan.FromSeconds(5))` and log a warning if the thread did not stop. (5) Dispose resources: `await channel.Reader.Completion`, then `_cancellationTokenSource.Dispose()`.

The key anti-patterns to avoid: not passing the shutdown token to all blocking/delaying operations (leaves them running after stop is signaled), not draining `Channel<T>` before returning (loses buffered items), and calling `GC.Collect()` or throwing exceptions from `StopAsync` (delays or breaks the shutdown sequence for other hosted services).

---

## Table of Contents

- [01. Threads & Thread Lifecycle](#01-threads-thread-lifecycle)
  - [Q1. Explain multithreading in C# and when it is appropriate vs async I/O or tasks.](#q1-explain-multithreading-in-c-and-when-it-is-appropriate-vs-async-io-or-tasks)
  - [Q2. What is a `Thread`, and how do you create and start one?](#q2-what-is-a-thread-and-how-do-you-create-and-start-one)
  - [Q3. What are foreground vs background threads, and how do they affect process shutdown?](#q3-what-are-foreground-vs-background-threads-and-how-do-they-affect-process-shutdown)
  - [Q4. What are the main thread states in the lifecycle (unstarted, running, wait/sleep/join, stopped)?](#q4-what-are-the-main-thread-states-in-the-lifecycle-unstarted-running-waitsleepjoin-stopped)
  - [Q5. What is `Thread.Join()`, and what happens if you never join a foreground thread?](#q5-what-is-threadjoin-and-what-happens-if-you-never-join-a-foreground-thread)
  - [Q6. What is `Thread.Sleep()` vs spinning vs waiting — when is each appropriate?](#q6-what-is-threadsleep-vs-spinning-vs-waiting-when-is-each-appropriate)
  - [Q7. What is thread affinity, and why does it matter for UI applications?](#q7-what-is-thread-affinity-and-why-does-it-matter-for-ui-applications)
  - [Q8. What is the difference between creating a raw `Thread` and using thread pool threads?](#q8-what-is-the-difference-between-creating-a-raw-thread-and-using-thread-pool-threads)
  - [Q9. What are `Thread.Name`, `IsBackground`, `Priority` — which actually affect scheduling?](#q9-what-are-threadname-isbackground-priority-which-actually-affect-scheduling)
  - [Q10. What is a race condition at the thread level, and how can two threads interleave unpredictably?](#q10-what-is-a-race-condition-at-the-thread-level-and-how-can-two-threads-interleave-unpredictably)
  - [Q11. What is the difference between kernel threads and managed threads (conceptual model)?](#q11-what-is-the-difference-between-kernel-threads-and-managed-threads-conceptual-model)
  - [Q12. Why is manually creating many threads often a scalability anti-pattern?](#q12-why-is-manually-creating-many-threads-often-a-scalability-anti-pattern)
  - [Q13. What is `ThreadStatic`, and how does it differ from `ThreadLocal<T>`?](#q13-what-is-threadstatic-and-how-does-it-differ-from-threadlocalt)
  - [Q14. What exceptions can occur when aborting or interrupting threads (historical vs modern guidance)?](#q14-what-exceptions-can-occur-when-aborting-or-interrupting-threads-historical-vs-modern-guidance)
  - [Q15. How does the main thread exiting affect background work still running?](#q15-how-does-the-main-thread-exiting-affect-background-work-still-running)

- [02. ThreadPool](#02-threadpool)
  - [Q1. What is the thread pool in .NET, and why is it preferred over creating raw threads?](#q1-what-is-the-thread-pool-in-net-and-why-is-it-preferred-over-creating-raw-threads)
  - [Q2. How does the thread pool manage worker threads and I/O completion threads?](#q2-how-does-the-thread-pool-manage-worker-threads-and-io-completion-threads)
  - [Q3. What is hill-climbing in the .NET thread pool (high level)?](#q3-what-is-hill-climbing-in-the-net-thread-pool-high-level)
  - [Q4. What is `ThreadPool.QueueUserWorkItem`, and how does it relate to `Task.Run`?](#q4-what-is-threadpoolqueueuserworkitem-and-how-does-it-relate-to-taskrun)
  - [Q5. What is starvation in the thread pool, and what causes it?](#q5-what-is-starvation-in-the-thread-pool-and-what-causes-it)
  - [Q6. How do synchronous blocking calls inside pool threads affect throughput?](#q6-how-do-synchronous-blocking-calls-inside-pool-threads-affect-throughput)
  - [Q7. What is the difference between dedicated threads and pool threads for long-running work?](#q7-what-is-the-difference-between-dedicated-threads-and-pool-threads-for-long-running-work)
  - [Q8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and when might you tune them?](#q8-what-is-threadpoolsetminthreads-setmaxthreads-and-when-might-you-tune-them)
  - [Q9. How does the thread pool interact with `async`/`await` continuations?](#q9-how-does-the-thread-pool-interact-with-asyncawait-continuations)
  - [Q10. What is the danger of blocking the UI thread vs blocking a pool thread?](#q10-what-is-the-danger-of-blocking-the-ui-thread-vs-blocking-a-pool-thread)
  - [Q11. How do thread pool threads relate to `Parallel.For` and PLINQ?](#q11-how-do-thread-pool-threads-relate-to-parallelfor-and-plinq)
  - [Q12. What diagnostics exist for thread pool queue length and thread counts (`ThreadPool.ThreadCount`, ETW)?](#q12-what-diagnostics-exist-for-thread-pool-queue-length-and-thread-counts-threadpoolthreadcount-etw)

- [03. Tasks & Task Parallel Library](#03-tasks-task-parallel-library)
  - [Q1. What is the Task Parallel Library (TPL)?](#q1-what-is-the-task-parallel-library-tpl)
  - [Q2. Explain the difference between `Thread` and `Task` in purpose and scheduling.](#q2-explain-the-difference-between-thread-and-task-in-purpose-and-scheduling)
  - [Q3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use each.](#q3-explain-task-taskt-and-valuetaskt-when-to-use-each)
  - [Q4. What is `Task.Run`, and when should it be used vs when it should be avoided?](#q4-what-is-taskrun-and-when-should-it-be-used-vs-when-it-should-be-avoided)
  - [Q5. What is `Task.Factory.StartNew`, and why is `Task.Run` usually preferred?](#q5-what-is-taskfactorystartnew-and-why-is-taskrun-usually-preferred)
  - [Q6. Explain task continuations with `ContinueWith` — options, scheduling, and exception handling.](#q6-explain-task-continuations-with-continuewith-options-scheduling-and-exception-handling)
  - [Q7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they differ from manual continuation chaining?](#q7-what-is-taskwhenall-taskwhenany-and-how-do-they-differ-from-manual-continuation-chaining)
  - [Q8. What is `TaskCompletionSource<T>`, and what scenarios does it enable (bridging callbacks, manual completion)?](#q8-what-is-taskcompletionsourcet-and-what-scenarios-does-it-enable-bridging-callbacks-manual-completion)
  - [Q9. What is the difference between completing a `TaskCompletionSource` with result, exception, or cancellation?](#q9-what-is-the-difference-between-completing-a-taskcompletionsource-with-result-exception-or-cancellation)
  - [Q10. What is `Task.FromResult`, `Task.CompletedTask`, and when are they preferable to `Task.Run`?](#q10-what-is-taskfromresult-taskcompletedtask-and-when-are-they-preferable-to-taskrun)
  - [Q11. What is the difference between `AggregateException` and a regular exception when tasks fail?](#q11-what-is-the-difference-between-aggregateexception-and-a-regular-exception-when-tasks-fail)
  - [Q12. How do child tasks relate to parent tasks (`TaskCreationOptions`, attached vs detached)?](#q12-how-do-child-tasks-relate-to-parent-tasks-taskcreationoptions-attached-vs-detached)
  - [Q13. What is task cancellation via `CancellationToken` registration vs `TrySetCanceled`?](#q13-what-is-task-cancellation-via-cancellationtoken-registration-vs-trysetcanceled)
  - [Q14. What are unobserved task exceptions, and how does .NET handle them?](#q14-what-are-unobserved-task-exceptions-and-how-does-net-handle-them)
  - [Q15. What is `ValueTask` pooling/caching, and why must consumers avoid double-awaiting unless documented safe?](#q15-what-is-valuetask-poolingcaching-and-why-must-consumers-avoid-double-awaiting-unless-documented-safe)
  - [Q16. How do you implement a timeout around a `Task` using `CancellationTokenSource` or `WhenAny`?](#q16-how-do-you-implement-a-timeout-around-a-task-using-cancellationtokensource-or-whenany)

- [04. Async and Await](#04-async-and-await)
  - [Q1. Explain asynchronous programming in C# — what problem does it solve?](#q1-explain-asynchronous-programming-in-c-what-problem-does-it-solve)
  - [Q2. Explain the `async` and `await` keywords in detail.](#q2-explain-the-async-and-await-keywords-in-detail)
  - [Q3. What is the difference between CPU-bound and I/O-bound async work?](#q3-what-is-the-difference-between-cpu-bound-and-io-bound-async-work)
  - [Q4. What is `ConfigureAwait(false)`, and when should library vs application code use it?](#q4-what-is-configureawaitfalse-and-when-should-library-vs-application-code-use-it)
  - [Q5. How do you handle exceptions in async/await methods?](#q5-how-do-you-handle-exceptions-in-asyncawait-methods)
  - [Q6. What is the difference between `async void`, `async Task`, and `async Task<T>`?](#q6-what-is-the-difference-between-async-void-async-task-and-async-taskt)
  - [Q7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, and how does `await foreach` work?](#q7-what-is-an-async-stream-iasyncenumerablet-in-c-8-and-how-does-await-foreach-work)
  - [Q8. How does the async state machine work under the hood (high level: `MoveNext`, `IAsyncStateMachine`)?](#q8-how-does-the-async-state-machine-work-under-the-hood-high-level-movenext-iasyncstatemachine)
  - [Q9. What is synchronization context, and how does it affect continuation marshaling?](#q9-what-is-synchronization-context-and-how-does-it-affect-continuation-marshaling)
  - [Q10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` cause deadlocks?](#q10-why-can-result-wait-and-getawaitergetresult-cause-deadlocks)
  - [Q11. What is the difference between `await task` and `return task` from an async method (async method builder behavior)?](#q11-what-is-the-difference-between-await-task-and-return-task-from-an-async-method-async-method-builder-behavior)
  - [Q12. How do you implement retry with exponential backoff in async code?](#q12-how-do-you-implement-retry-with-exponential-backoff-in-async-code)
  - [Q13. What is jitter in backoff strategies, and why is it used?](#q13-what-is-jitter-in-backoff-strategies-and-why-is-it-used)
  - [Q14. How do Polly-style resilience policies relate to manual retry loops?](#q14-how-do-polly-style-resilience-policies-relate-to-manual-retry-loops)
  - [Q15. What is `CancellationTokenSource.CreateLinkedTokenSource`, and when is linking tokens needed?](#q15-what-is-cancellationtokensourcecreatelinkedtokensource-and-when-is-linking-tokens-needed)
  - [Q16. How do you propagate cancellation through layered async APIs?](#q16-how-do-you-propagate-cancellation-through-layered-async-apis)
  - [Q17. What is `Task.Delay` vs `Thread.Sleep` in async methods?](#q17-what-is-taskdelay-vs-threadsleep-in-async-methods)
  - [Q18. What is "async all the way" — why is mixing blocking and async problematic?](#q18-what-is-async-all-the-way-why-is-mixing-blocking-and-async-problematic)
  - [Q19. How do you unit test async methods and time-dependent retry logic?](#q19-how-do-you-unit-test-async-methods-and-time-dependent-retry-logic)
  - [Q20. What is `IAsyncDisposable`, and how does `await using` work?](#q20-what-is-iasyncdisposable-and-how-does-await-using-work)

- [05. Parallel Programming](#05-parallel-programming)
  - [Q1. What is `Parallel.For` and `Parallel.ForEach`?](#q1-what-is-parallelfor-and-parallelforeach)
  - [Q2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `CancellationToken`) used for?](#q2-what-is-paralleloptions-maxdegreeofparallelism-cancellationtoken-used-for)
  - [Q3. What is a `Partitioner<TSource>`, and when would you supply a custom partitioner?](#q3-what-is-a-partitionertsource-and-when-would-you-supply-a-custom-partitioner)
  - [Q4. What is the difference between range partitioning and chunk partitioning?](#q4-what-is-the-difference-between-range-partitioning-and-chunk-partitioning)
  - [Q5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `WithMergeOptions`).](#q5-explain-plinq-asparallel-withdegreeofparallelism-withmergeoptions)
  - [Q6. When is parallelization slower than sequential execution?](#q6-when-is-parallelization-slower-than-sequential-execution)
  - [Q7. What types of workloads benefit from PLINQ vs `Parallel.ForEach`?](#q7-what-types-of-workloads-benefit-from-plinq-vs-parallelforeach)
  - [Q8. What are thread-safe requirements when using parallel loops (shared state, locals, aggregation)?](#q8-what-are-thread-safe-requirements-when-using-parallel-loops-shared-state-locals-aggregation)
  - [Q9. How do you perform parallel aggregation with `lock`, `Interlocked`, or thread-local accumulators?](#q9-how-do-you-perform-parallel-aggregation-with-lock-interlocked-or-thread-local-accumulators)
  - [Q10. What is `ParallelLoopResult`, and how do you detect partial failures?](#q10-what-is-parallelloopresult-and-how-do-you-detect-partial-failures)
  - [Q11. What are ordering guarantees in PLINQ (`AsOrdered`) and their cost?](#q11-what-are-ordering-guarantees-in-plinq-asordered-and-their-cost)
  - [Q12. How does parallel LINQ decide default partition sizes?](#q12-how-does-parallel-linq-decide-default-partition-sizes)
  - [Q13. What exceptions are thrown from parallel loops (`AggregateException`, inner exceptions)?](#q13-what-exceptions-are-thrown-from-parallel-loops-aggregateexception-inner-exceptions)
  - [Q14. How do you combine async I/O with parallel CPU work without blocking the pool?](#q14-how-do-you-combine-async-io-with-parallel-cpu-work-without-blocking-the-pool)
  - [Q15. What are best practices for parallel and async code in server applications?](#q15-what-are-best-practices-for-parallel-and-async-code-in-server-applications)

- [06. Synchronization and Locks](#06-synchronization-and-locks)
  - [Q1. Explain synchronization primitives: `lock`, `Monitor`, `Mutex`, and `Semaphore`/`SemaphoreSlim`.](#q1-explain-synchronization-primitives-lock-monitor-mutex-and-semaphoresemaphoreslim)
  - [Q2. What is `ReaderWriterLockSlim`, and when is it preferable to a plain `lock`?](#q2-what-is-readerwriterlockslim-and-when-is-it-preferable-to-a-plain-lock)
  - [Q3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualResetEventSlim`.](#q3-explain-autoresetevent-manualresetevent-and-manualreseteventslim)
  - [Q4. What is `CancellationToken`, and how do you implement cooperative cancellation?](#q4-what-is-cancellationtoken-and-how-do-you-implement-cooperative-cancellation)
  - [Q5. Explain deadlocks in multithreading — necessary conditions and prevention strategies.](#q5-explain-deadlocks-in-multithreading-necessary-conditions-and-prevention-strategies)
  - [Q6. What are race conditions, and how can they be prevented?](#q6-what-are-race-conditions-and-how-can-they-be-prevented)
  - [Q7. What is the `volatile` keyword, and when does it provide visibility guarantees?](#q7-what-is-the-volatile-keyword-and-when-does-it-provide-visibility-guarantees)
  - [Q8. What is the difference between `volatile` and `lock` for thread safety?](#q8-what-is-the-difference-between-volatile-and-lock-for-thread-safety)
  - [Q9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`), and when is it enough without `lock`?](#q9-what-is-interlocked-increment-compareexchange-add-and-when-is-it-enough-without-lock)
  - [Q10. What is `SpinLock`, and when might low-latency spinning beat `lock`?](#q10-what-is-spinlock-and-when-might-low-latency-spinning-beat-lock)
  - [Q11. What is lock ordering, and how does it prevent deadlock?](#q11-what-is-lock-ordering-and-how-does-it-prevent-deadlock)
  - [Q12. What is the `Monitor.TryEnter` pattern, and how do timeouts help avoid indefinite blocking?](#q12-what-is-the-monitortryenter-pattern-and-how-do-timeouts-help-avoid-indefinite-blocking)
  - [Q13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`) vs blocking `lock` in async code?](#q13-what-is-async-compatible-locking-semaphoreslimwaitasync-vs-blocking-lock-in-async-code)
  - [Q14. What is a priority inversion problem (conceptual), and which primitives exacerbate it?](#q14-what-is-a-priority-inversion-problem-conceptual-and-which-primitives-exacerbate-it)
  - [Q15. How do you diagnose deadlocks and lock contention in production (dump analysis, `dotnet-sync`, counters)?](#q15-how-do-you-diagnose-deadlocks-and-lock-contention-in-production-dump-analysis-dotnet-sync-counters)
  - [Q16. What is thread-safe lazy initialization (`Lazy<T>`, double-checked locking pitfalls)?](#q16-what-is-thread-safe-lazy-initialization-lazyt-double-checked-locking-pitfalls)

- [07. Concurrent Collections](#07-concurrent-collections)
  - [Q1. What concurrent collections exist in .NET (`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection`, etc.)?](#q1-what-concurrent-collections-exist-in-net-concurrentdictionary-concurrentqueue-concurrentbag-blockingcollection-etc)
  - [Q2. When should you use thread-safe collections instead of standard collections plus locks?](#q2-when-should-you-use-thread-safe-collections-instead-of-standard-collections-plus-locks)
  - [Q3. What is the difference between `Dictionary<TKey, TValue>` and `ConcurrentDictionary<TKey, TValue>`?](#q3-what-is-the-difference-between-dictionarytkey-tvalue-and-concurrentdictionarytkey-tvalue)
  - [Q4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `ConcurrentDictionary`?](#q4-what-are-addorupdate-getoradd-and-tryupdate-on-concurrentdictionary)
  - [Q5. What is `BlockingCollection<T>`, and how does it implement producer-consumer patterns?](#q5-what-is-blockingcollectiont-and-how-does-it-implement-producer-consumer-patterns)
  - [Q6. What is the difference between bounded and unbounded `BlockingCollection` behavior?](#q6-what-is-the-difference-between-bounded-and-unbounded-blockingcollection-behavior)
  - [Q7. How do you use `BlockingCollection` with multiple producers and consumers?](#q7-how-do-you-use-blockingcollection-with-multiple-producers-and-consumers)
  - [Q8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `ConcurrentBag` — ordering and stealing semantics?](#q8-what-is-concurrentqueue-vs-concurrentstack-vs-concurrentbag-ordering-and-stealing-semantics)
  - [Q9. When is `ConcurrentBag` the wrong choice despite being thread-safe?](#q9-when-is-concurrentbag-the-wrong-choice-despite-being-thread-safe)
  - [Q10. What is `IProducerConsumerCollection<T>` and custom underlying stores for `BlockingCollection`?](#q10-what-is-iproducerconsumercollectiont-and-custom-underlying-stores-for-blockingcollection)
  - [Q11. How do concurrent collections compare to locking a `List<T>` for high-contention scenarios?](#q11-how-do-concurrent-collections-compare-to-locking-a-listt-for-high-contention-scenarios)
  - [Q12. What enumeration semantics do concurrent collections provide (weakly consistent iterators)?](#q12-what-enumeration-semantics-do-concurrent-collections-provide-weakly-consistent-iterators)
  - [Q13. How do you gracefully complete adding to a `BlockingCollection` (`CompleteAdding`)?](#q13-how-do-you-gracefully-complete-adding-to-a-blockingcollection-completeadding)
  - [Q14. What pitfalls arise when mixing concurrent collections with LINQ?](#q14-what-pitfalls-arise-when-mixing-concurrent-collections-with-linq)
  - [Q15. When should you use channels (`System.Threading.Channels`) instead of `BlockingCollection` in modern code?](#q15-when-should-you-use-channels-systemthreadingchannels-instead-of-blockingcollection-in-modern-code)
  - [Q16. **`.Result` / `.Wait()` deadlock** — Blocking async on a captured synchronization context (UI, legacy ASP.NET) deadlocks when the continuation needs that same context.](#q16-result-wait-deadlock-blocking-async-on-a-captured-synchronization-context-ui-legacy-aspnet-deadlocks-when-the-continuation-needs-that-same-context)
  - [Q17. **`async void` swallows observability** — Exceptions cannot be awaited by callers; use only for event handlers.](#q17-async-void-swallows-observability-exceptions-cannot-be-awaited-by-callers-use-only-for-event-handlers)
  - [Q18. **`Task.Run` for I/O** — Offloading blocking I/O to the pool wastes threads; prefer truly async APIs.](#q18-taskrun-for-io-offloading-blocking-io-to-the-pool-wastes-threads-prefer-truly-async-apis)
  - [Q19. **Async does not mean threaded** — I/O `await` often completes without extra threads; continuations may run on any pool thread.](#q19-async-does-not-mean-threaded-io-await-often-completes-without-extra-threads-continuations-may-run-on-any-pool-thread)
  - [Q20. **Unobserved task exceptions** — Faulted tasks that are never awaited may surface later as unobserved exception events.](#q20-unobserved-task-exceptions-faulted-tasks-that-are-never-awaited-may-surface-later-as-unobserved-exception-events)
  - [Q21. **Race on `List<T>`/`Dictionary<,>`** — Even `Add` is not thread-safe; use locks or concurrent collections.](#q21-race-on-listtdictionary-even-add-is-not-thread-safe-use-locks-or-concurrent-collections)
  - [Q22. **`ConfigureAwait(false)` in libraries** — Library code should not marshal back to UI context; app code often needs the default for UI updates.](#q22-configureawaitfalse-in-libraries-library-code-should-not-marshal-back-to-ui-context-app-code-often-needs-the-default-for-ui-updates)
  - [Q23. **`ValueTask` double-await** — Re-awaiting or concurrent awaits on a pooled `ValueTask` can corrupt state unless documented safe.](#q23-valuetask-double-await-re-awaiting-or-concurrent-awaits-on-a-pooled-valuetask-can-corrupt-state-unless-documented-safe)
  - [Q24. **`TaskCompletionSource` set twice** — Second `TrySet*` calls fail; race to complete can drop results if not coordinated.](#q24-taskcompletionsource-set-twice-second-tryset-calls-fail-race-to-complete-can-drop-results-if-not-coordinated)
  - [Q25. **`BlockingCollection` after `CompleteAdding`** — Adding throws; consumers must drain remaining items correctly.](#q25-blockingcollection-after-completeadding-adding-throws-consumers-must-drain-remaining-items-correctly)
  - [Q26. **`Interlocked` is not composable** — Check-then-act on complex invariants still needs `lock` or careful CAS loops.](#q26-interlocked-is-not-composable-check-then-act-on-complex-invariants-still-needs-lock-or-careful-cas-loops)
  - [Q27. **`volatile` does not make operations atomic** — `i++` still races even if `i` is volatile.](#q27-volatile-does-not-make-operations-atomic-i-still-races-even-if-i-is-volatile)
  - [Q28. **Parallel loop over small work** — Partitioning overhead can make `Parallel.ForEach` slower than sequential code.](#q28-parallel-loop-over-small-work-partitioning-overhead-can-make-parallelforeach-slower-than-sequential-code)
  - [Q29. **Shared `Random` is not thread-safe** — Use `Random.Shared` or thread-local RNG in parallel code.](#q29-shared-random-is-not-thread-safe-use-randomshared-or-thread-local-rng-in-parallel-code)
  - [Q30. **Retry without cancellation** — Exponential backoff loops must honor `CancellationToken` and max attempts to avoid runaway delays.](#q30-retry-without-cancellation-exponential-backoff-loops-must-honor-cancellationtoken-and-max-attempts-to-avoid-runaway-delays)
  - [Q1. (R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:](#q1-r-a-warehouse-console-tool-spawns-label-printers-on-dedicated-threads-operators-report-the-process-hangs-after-pressing-enter-to-quit-even-though-cancellation-was-requested-review-the-shutdown-wiring)
  - [Q2. (R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:](#q2-r-a-teammate-copied-the-shipment-worker-from-the-chapter-tutorial-but-dropped-synchronization-for-speed-under-load-totals-and-result-lists-disagree-review)
  - [Q3. (R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:](#q3-r-after-parallelizing-shipment-processing-every-worker-log-shows-the-same-shipment-id-sh-1003-even-though-three-different-ids-were-queued-review-the-spawn-loop)
  - [Q4. (P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?](#q4-p-a-long-running-inventory-sweep-runs-on-a-dedicated-thread-like-runinventorysweep-in-the-chapter-demo-ops-wants-the-windows-service-to-stop-within-30-seconds-on-shutdown-no-threadabort-what-production-pattern-replaces-force-kill-and-what-must-the-worker-loop-guarantee)
  - [Q5. (M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?](#q5-m-main-waits-for-workers-using-isalive-and-join100-in-a-loop-matching-the-chapter-demo-under-heavy-load-logs-show-hundreds-of-waiting-on-worker--lines-per-second-while-workers-are-still-running-is-this-a-bug-and-what-waiting-pattern-is-preferable-in-production)
  - [Q6. (D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?](#q6-d-apex-warehouse-will-scan-400-inbound-shipments-per-hour-a-developer-proposes-new-threadprocessshipment-per-shipment-forever-threadpriorityabovenormal-on-express-lanes-and-threadstatic-counters-for-per-worker-metrics-exported-to-prometheus-what-breaks-at-scale-and-what-would-you-use-instead-while-still-honoring-lifecycle-concepts-from-this-chapter)
  - [Q7. (R) A retry path tries to restart workers after a transient fault. Review:](#q7-r-a-retry-path-tries-to-restart-workers-after-a-transient-fault-review)

- [02. ThreadPool](#02-threadpool-1)

- [02. ThreadPool](#02-threadpool-2)
  - [Q1. (R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?](#q1-r-a-nightly-invoice-import-queues-validation-onto-the-thread-pool-but-reports-wrong-counts-in-production-sometimes-all-zeros-review-this-service-method-what-fails-under-load-and-how-do-you-fix-it-in-priority-order)
  - [Q2. (R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:](#q2-r-a-legacy-com-aware-host-copied-the-tutorials-manualresetevent-waithandlewaitall-pattern-for-large-batches-review-this-batch-runner-used-with-jobcount-500)
  - [Q3. (R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:](#q3-r-after-a-refactor-an-audit-pipeline-starves-under-concurrent-load-other-timers-and-taskrun-work-stops-progressing-review-the-pool-callback)
  - [Q4. (P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:](#q4-p-every-microservice-instance-calls-this-at-startup-in-programcs-to-avoid-cold-start-latency-after-deploy)
  - [Q5. (M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?](#q5-m-during-a-traffic-spike-dashboards-show-getavailablethreads-reporting-very-few-free-worker-threads-but-cpu-is-only-35-a-teammate-concludes-we-need-more-cores-given-this-monitoring-snippet-from-a-pool-callback-what-is-the-more-likely-root-cause)
  - [Q6. (D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:](#q6-d-a-thumbnail-service-receives-bursts-of-2000-independent-resize-jobs-per-upload-batch-50-ms-cpu-each-two-proposals)
  - [Q7. (R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:](#q7-r-pool-callbacks-silently-drop-failures-in-production-support-sees-partial-imports-with-no-error-logs-review-this-aggregation-helper)

- [03. Tasks & Task Parallel Library](#03-tasks-task-parallel-library-1)

- [03. Tasks & Task Parallel Library](#03-tasks-task-parallel-library-2)
  - [Q1. (R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:](#q1-r-an-aspnet-core-batch-validation-endpoint-works-in-dev-but-stalls-under-load-review-the-action)
  - [Q2. (R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:](#q2-r-a-fulfillment-service-refactored-raw-threads-to-tasks-but-ops-reports-missing-line-picks-and-intermittent-duplicate-shipments-review)
  - [Q3. (R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:](#q3-r-a-payment-integration-wraps-a-legacy-callback-gateway-with-taskcompletionsource-declined-payments-sometimes-hang-until-timeout-approved-payments-occasionally-throw-invalidoperationexception-review)
  - [Q4. (R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:](#q4-r-a-shipping-pipeline-chains-pick-label-with-continuations-after-removing-asyncawait-for-clarity-fault-injection-tests-crash-the-worker-process-review)
  - [Q5. (P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:](#q5-p-a-warehouse-api-throttles-concurrent-picks-with-semaphoreslim-matching-the-chapter-pattern-after-a-downstream-timeout-spike-throughput-collapses-to-zero-until-restart-review)
  - [Q6. (M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:](#q6-m-a-carrier-selection-service-uses-taskwhenany-to-take-the-fastest-quote-as-in-the-chapter-demo-load-tests-show-open-http-connection-counts-climbing-review)
  - [Q7. (D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?](#q7-d-a-team-must-batch-validate-four-thousand-orders-every-night-one-developer-proposes-taskwaitall-on-thousands-of-taskrun-validateorder-calls-another-wants-parallelforeach-immediately-a-third-wants-asyncawait-with-taskwhenall-and-a-concurrency-limit-what-breaks-at-scale-with-the-first-approach-and-what-pattern-would-you-ship)

- [04. Async and Await](#04-async-and-await-1)

- [04. Async and Await](#04-async-and-await-2)
  - [Q1. (R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:](#q1-r-under-load-report-export-api-requests-time-out-and-thread-pool-starvation-alerts-fire-review-this-aspnet-core-minimal-endpoint-and-service)
  - [Q2. (R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:](#q2-r-a-nightly-export-job-sometimes-crashes-the-worker-process-with-no-log-line-review-this-orchestrator)
  - [Q3. (R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:](#q3-r-a-wpf-desktop-app-deadlocks-on-startup-when-loading-reports-through-a-shared-nuget-library-review-the-library-and-caller)
  - [Q4. (P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:](#q4-p-a-team-wraps-a-legacy-http-client-that-ignores-cancellationtoken-they-ship-this-timeout-helper-for-report-downloads)
  - [Q5. (R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:](#q5-r-transient-upstream-failures-are-handled-with-a-shared-retry-helper-but-operators-report-exports-running-for-minutes-after-a-user-cancels-review)
  - [Q6. (D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:](#q6-d-only-one-report-may-write-to-a-shared-export-folder-at-a-time-a-developer-adds-this-gate-to-a-singleton-registered-service)
  - [Q7. (M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:](#q7-m-a-hot-path-metadata-lookup-was-optimized-to-return-valuetaskint-after-a-refactor-intermittent-invalidoperationexception-appears-in-logs-review)

- [05. Parallel Programming](#05-parallel-programming-1)

- [05. Parallel Programming](#05-parallel-programming-2)
  - [Q1. (R) A nightly warehouse job sums reconciled inventory values in parallel. Finance reports totals that drift from the serial baseline. Review the hot path:](#q1-r-a-nightly-warehouse-job-sums-reconciled-inventory-values-in-parallel-finance-reports-totals-that-drift-from-the-serial-baseline-review-the-hot-path)
  - [Q2. (R) A teammate parallelizes audit-log line generation for the same SKU batch:](#q2-r-a-teammate-parallelizes-audit-log-line-generation-for-the-same-sku-batch)
  - [Q3. (R) Under load, a reporting endpoint times out and thread-pool starvation alerts fire. Review the "optimization" added to fetch order details:](#q3-r-under-load-a-reporting-endpoint-times-out-and-thread-pool-starvation-alerts-fire-review-the-optimization-added-to-fetch-order-details)
  - [Q4. (P) A CPU-bound pricing engine recalculates thousands of in-memory `StockRecord` rows on a 16-core VM shared with other services. A developer caps workers like this:](#q4-p-a-cpu-bound-pricing-engine-recalculates-thousands-of-in-memory-stockrecord-rows-on-a-16-core-vm-shared-with-other-services-a-developer-caps-workers-like-this)
  - [Q5. (D) A reconciliation worker must stop processing once cumulative value crosses a credit limit — not process the entire batch. Two implementations were proposed:](#q5-d-a-reconciliation-worker-must-stop-processing-once-cumulative-value-crosses-a-credit-limit-not-process-the-entire-batch-two-implementations-were-proposed)
  - [Q6. (R) A dashboard query was "speed up" with PLINQ. Users see wrong top-SKU ordering under load and elevated CPU:](#q6-r-a-dashboard-query-was-speed-up-with-plinq-users-see-wrong-top-sku-ordering-under-load-and-elevated-cpu)
  - [Q7. (M) A partitioner was introduced to reduce scheduling overhead on a uniform-cost batch, but throughput dropped on a 4-core machine:](#q7-m-a-partitioner-was-introduced-to-reduce-scheduling-overhead-on-a-uniform-cost-batch-but-throughput-dropped-on-a-4-core-machine)

- [06. Synchronization and Locks](#06-synchronization-and-locks-1)

- [06. Synchronization and Locks](#06-synchronization-and-locks-2)
  - [Q1. (R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:](#q1-r-a-payment-microservice-registers-ledgerservice-as-a-singleton-under-concurrent-deposits-and-withdrawals-balances-drift-and-qa-sees-different-totals-on-every-run-review)
  - [Q2. (R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:](#q2-r-a-batch-job-transfers-funds-between-two-bankaccount-instances-on-background-threads-the-job-hangs-intermittently-under-load-no-exception-threads-stuck-in-monitorwait-review)
  - [Q3. (R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:](#q3-r-a-developer-async-ified-a-cache-warmer-registered-as-a-singleton-in-aspnet-core-the-app-compiles-in-some-branches-but-stalls-request-threads-under-traffic-review)
  - [Q4. (R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:](#q4-r-a-read-heavy-interest-rate-api-uses-readerwriterlockslim-like-the-chapter-tutorial-the-first-request-for-a-missing-product-code-freezes-the-entire-rate-service-review)
  - [Q5. (P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?](#q5-p-an-outbound-api-integration-must-allow-at-most-50-concurrent-http-calls-cluster-wide-per-process-record-a-global-request-counter-for-metrics-and-support-cooperative-shutdown-of-a-background-poller-which-synchronization-primitives-do-you-use-for-each-concern-and-what-breaks-if-you-use-lock-for-all-three)
  - [Q6. (M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:](#q6-m-a-nightly-vault-scan-worker-runs-on-a-dedicated-thread-operators-click-stop-in-a-winforms-style-host-locally-it-often-exits-but-on-release-builds-in-production-the-thread-keeps-running-until-the-process-is-killed-review)
  - [Q7. (D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:](#q7-d-two-designs-protect-a-singleton-in-memory-fee-schedule-updated-once-per-hour-and-read-on-every-pricing-request)

- [07. Concurrent Collections](#07-concurrent-collections-1)

- [07. Concurrent Collections](#07-concurrent-collections-2)
  - [Q1. (R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:](#q1-r-a-warehouse-api-records-parallel-pick-confirmations-into-shared-stock-counts-under-load-inventory-drifts-negative-even-though-each-sale-is-valid-review-this-service-method)
  - [Q2. (R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:](#q2-r-a-catalog-microservice-caches-product-rows-in-concurrentdictionary-to-cut-database-round-trips-after-a-traffic-spike-ops-sees-duplicate-loadproduct-calls-and-inflated-cache-miss-metrics-for-the-same-sku-review)
  - [Q3. (R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:](#q3-r-a-nightly-batch-job-ships-orders-through-a-bounded-in-memory-buffer-locally-it-finishes-in-production-the-job-hangs-until-the-host-kills-the-process-review-the-pipeline)
  - [Q4. (R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:](#q4-r-support-tickets-must-be-processed-first-in-first-out-a-developer-chose-concurrentbag-because-its-built-for-parallel-workers-review-the-dispatcher)
  - [Q5. (P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?](#q5-p-a-log-ingestion-service-has-50-http-producers-and-4-background-writers-an-unbounded-concurrentqueuelogentry-caused-an-oom-during-a-burst-how-would-you-redesign-the-buffer-using-types-from-this-chapter-and-what-breaks-if-you-skip-back-pressure)
  - [Q6. (D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:](#q6-d-two-approaches-for-collecting-validation-errors-from-parallelforeach-over-10000-csv-rows)
  - [Q7. (M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:](#q7-m-during-peak-picking-a-dashboard-polls-concurrentdictionary-for-a-live-inventory-report)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

### 01. Threads & Thread Lifecycle

## Q1. Explain multithreading in C# and when it is appropriate vs async I/O or tasks.

(R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:

```csharp
public static void Main()
{
    using var cts = new CancellationTokenSource();
    var labelThread = new Thread(() => PrintLabelsLoop(cts.Token));
    labelThread.Name = "LabelPrinter";
    labelThread.Start();

    Console.WriteLine("Press Enter to stop…");
    Console.ReadLine();
    cts.Cancel();
    // expects immediate exit after Enter
}

static void PrintLabelsLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        Thread.Sleep(500);
        Console.WriteLine("[LabelPrinter] stamp…");
    }
}
```

What keeps the process alive, and how do you fix shutdown so cancellation is honored cleanly?


**Concepts**
- foreground vs background thread
- CancellationToken cooperative flag
- Thread.Join for graceful shutdown
- process lifetime and foreground threads
- dedicated thread use cases

**Answer:** `CancellationToken` only sets a flag — it does not terminate the thread. The label thread defaults to **foreground** (`IsBackground == false`), so the CLR keeps the process alive until that thread's delegate finishes. Main exits after `Cancel()` without `Join`, but the foreground worker may still be inside `Thread.Sleep(500)` before it observes cancellation.


**Fix (priority order):**

1. After `cts.Cancel()`, call `labelThread.Join()` (optionally `Join(TimeSpan.FromSeconds(30))` and log if timeout).
2. Keep cooperative cancellation in the loop — check `token.IsCancellationRequested` and exit cleanly (matches **Program.cs** Section 8 — `RunInventorySweep`).
3. For daemon-style helpers that must not block process exit, set `labelThread.IsBackground = true` **only** when you accept abrupt termination without guaranteed cleanup — still prefer `Join` for graceful shutdown.
4. Shorten blocking intervals or use `token.WaitHandle.WaitOne(100)` so cancellation is observed faster between iterations.

```csharp
cts.Cancel();
if (!labelThread.Join(TimeSpan.FromSeconds(30)))
    Console.Error.WriteLine("LabelPrinter did not stop in time.");
```


---

## Q2. What is a `Thread`, and how do you create and start one?

(R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:

```csharp
private static int _packagesProcessed;
private static readonly List<ShipmentResult> _completed = new();

public static void ProcessShipment(object? state)
{
    var work = (ShipmentWork)state!;
    int boxesDone = 0;

    for (int box = 1; box <= work.BoxCount; box++)
    {
        Thread.Sleep(work.MillisecondsPerBox);
        _packagesProcessed++;          // no lock
        boxesDone++;
    }

    _completed.Add(new ShipmentResult(
        work.ShipmentId, work.Destination, boxesDone,
        0, Thread.CurrentThread.ManagedThreadId, ""));

    Console.WriteLine($"[{Thread.CurrentThread.Name}] done {work.ShipmentId}");
}

// Main starts three ParameterizedThreadStart workers concurrently on shared static fields.
```

What fails in production, and what is the prioritized fix?


**Concepts**
- shared mutable state
- lost-update race
- List<T> non-thread-safe
- Interlocked.Increment
- lock synchronization

**Answer:** Multiple workers perform unsynchronized read-modify-write on `_packagesProcessed` and concurrent `List<T>.Add` calls. The tally loses increments (classic lost update), and the list can corrupt internal state or throw — intermittent failures that pass single-threaded demos.


**Fix (priority order):**

1. Protect shared mutations with one lock object (same pattern as **Program.cs** `TallyLock` around increment and `CompletedShipments.Add`).
2. Prefer `Interlocked.Increment(ref _packagesProcessed)` for the counter if it is the only numeric shared state — still lock the list or use a thread-safe collection.
3. For production aggregation, consider `ConcurrentBag<ShipmentResult>` or per-worker local results merged after `Join` — eliminates lock contention on hot paths.
4. Re-run under parallel workers with stress timing (reduce `Sleep`) to reproduce before/after fix.

```csharp
lock (TallyLock)
{
    _packagesProcessed++;
}
// …
lock (TallyLock)
{
    _completed.Add(result);
}
```


---

## Q3. What are foreground vs background threads, and how do they affect process shutdown?

(R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:

```csharp
ShipmentWork[] pending =
[
    new("SH-1001", "North", 3, 50),
    new("SH-1002", "South", 2, 50),
    new("SH-1003", "East", 4, 50),
];

var threads = new Thread[pending.Length];
for (int i = 0; i < pending.Length; i++)
{
    threads[i] = new Thread(() => ProcessShipment(pending[i]));
    threads[i].Name = $"Worker-{pending[i].ShipmentId}";
    threads[i].Start();
}

foreach (var t in threads) t.Join();
```

Why does every thread process the last shipment, and how do you fix it without changing the worker signature?


**Concepts**
- closure capture over loop variable
- lambda variable capture bug
- ParameterizedThreadStart
- per-iteration local capture
- thread naming

**Answer:** The lambda closes over the loop variable `i`, not the value at iteration time. All threads may start after the loop finishes, so `pending[i]` resolves to the last index for every delegate — a closure capture bug unrelated to `ParameterizedThreadStart` itself.


**Fix (priority order):**

1. Capture a per-iteration local: `var work = pending[i];` then `new Thread(() => ProcessShipment(work))`.
2. Or use `ParameterizedThreadStart` directly: `new Thread(ProcessShipment)` and `worker.Start(pending[i])` — passes state at start, no closure over `i` (**Program.cs** Section 6–7 pattern).
3. Add a unit/integration test that asserts three distinct `ShipmentId` values in results after parallel start.

```csharp
for (int i = 0; i < pending.Length; i++)
{
    ShipmentWork work = pending[i];
    threads[i] = new Thread(() => ProcessShipment(work));
    threads[i].Start();
}
```


---

## Q4. What are the main thread states in the lifecycle (unstarted, running, wait/sleep/join, stopped)?

(P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?


**Concepts**
- cooperative cancellation
- CancellationToken propagation
- Thread.Abort removal in .NET Core
- Windows Service graceful stop
- Join with timeout

**Answer:** Use **cooperative cancellation** with `CancellationToken` linked to the service's `IHostApplicationLifetime.ApplicationStopping` (or a `CancellationTokenSource` cancelled in `StopAsync`). The worker checks `IsCancellationRequested` (or `ThrowIfCancellationRequested`) in its loop, finishes the current aisle/unit of work if needed, releases locks and handles, and exits the delegate normally — then the host `Join`s the thread or awaits a `Task` wrapper.

- Never call `Thread.Abort` — removed from .NET Core because it could leave locks held and invariants broken mid-method (**Program.cs** Section 8).
- Pass the token into the worker at construction/start; cancel once from the shutdown path; block shutdown on `Join(timeout)` and log if the worker exceeds the SLA.
- Keep loop body idempotent at cancellation boundaries — persist checkpoint if stopping mid-batch matters for ops.
- For I/O-bound sweeps, prefer `async`/`await` with the same token (later chapter) so threads are not blocked in `Sleep`.


---

## Q5. What is `Thread.Join()`, and what happens if you never join a foreground thread?

(M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?

```csharp
foreach (Thread worker in workers)
{
    while (worker.IsAlive)
    {
        Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
        if (!worker.Join(millisecondsTimeout: 100))
            continue;
    }
    Console.WriteLine($"  {worker.Name} finished.");
}
```


**Concepts**
- Thread.Join blocking vs polling
- IsAlive busy-poll anti-pattern
- timed Join(TimeSpan)
- CountdownEvent alternative
- production join semantics

**Answer:** This is not a correctness bug — it is a **busy-polling** wait pattern. `Join(100)` returns `false` every 100 ms while the worker runs, so the loop spins and floods logs under load. Functionally, the thread eventually completes; operationally, you waste CPU and drown observability.

- Prefer a single blocking `worker.Join()` per worker when you simply need to wait until done (**Program.cs** Section 7).
- Use `Join(TimeSpan)` once when you need a timeout — handle `false` as SLA breach, do not spin in a tight loop unless you must interleave other work.
- If Main must pump progress UI or heartbeats while waiting, use `Join(100)` **without** logging every iteration — log on interval or on state change.
- For many workers, `Task.Run` + `Task.WhenAll` or `Parallel.Invoke` gives clearer composition than manual `IsAlive` polling (later chapters).


---

## Q6. What is `Thread.Sleep()` vs spinning vs waiting — when is each appropriate?

(D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?


**Concepts**
- new Thread per task anti-pattern
- ThreadPriority OS hint
- ThreadStatic metric pitfall
- bounded concurrency
- thread pool preference

**Answer:** Unbounded `new Thread` per shipment exhausts OS thread limits and memory (default stack reserve per thread), thrashes the scheduler, and makes shutdown join storms impossible. `ThreadPriority` is an OS hint, not a SLA — express lanes are not reliably prioritized across machines. `[ThreadStatic]` metrics break when work moves to thread pool threads or `async` continuations hop threads — counters attach to threads, not logical shipments.

- Cap concurrency: fixed pool of long-lived worker threads **or** `ThreadPool` / `Task` with a bounded `SemaphoreSlim` (e.g., max 8 scanners) — same lifecycle idea (start work, join/complete, cooperative stop) without 1:1 OS threads.
- Express handling belongs in queue priority or business rules, not `ThreadPriority.AboveNormal`.
- Export metrics with labels (`shipment_id`, `worker_id`) via `IMeterFactory`/Prometheus counters — not `[ThreadStatic]` tallies.
- Keep `CancellationToken` on the batch host so service shutdown still cooperates (**Section 8** pattern).
- CPU-bound parallel loops → **05. Parallel Programming**; I/O-bound waits → **04. Async and Await**.


---

## Q7. What is thread affinity, and why does it matter for UI applications?

(R) A retry path tries to restart workers after a transient fault. Review:

```csharp
Thread worker = new Thread(ProcessShipment);
worker.Name = "Worker-Retry";
worker.Start(shipment);

worker.Join();
if (shipment.NeedsRetry)
{
    worker.Start(shipment);   // "restart same thread"
    worker.Join();
}
```

What fails at runtime, and what is the correct lifecycle approach?


**Concepts**
- Thread one-shot lifecycle
- ThreadStateException on restart
- Stopped state
- new Thread per retry
- ParameterizedThreadStart retry pattern

**Answer:** A `Thread` instance is **one-shot**. After the delegate completes, `ThreadState` is `Stopped` and calling `Start()` again throws `ThreadStateException` ("Thread is dead; it cannot be started"). You cannot restart the same `Thread` object.


**Fix (priority order):**

1. Create a **new** `Thread` (or new `Task`) for each retry attempt: `worker = new Thread(ProcessShipment); worker.Start(shipment);`
2. Better: one worker loop that reads from a `BlockingCollection<ShipmentWork>` or channel and retries internally — single thread lifecycle, many shipments.
3. Best at scale: queue work to `ThreadPool` / `Task` with retry policy (`Polly`) — no manual thread reuse semantics to get wrong.
4. Guard retry with max attempts and log `ManagedThreadId` per attempt for correlation.

```csharp
for (int attempt = 1; attempt <= maxAttempts && shipment.NeedsRetry; attempt++)
{
    var worker = new Thread(ProcessShipment) { Name = $"Worker-Retry-{attempt}" };
    worker.Start(shipment);
    worker.Join();
}
```


---

## Q8. What is the difference between creating a raw `Thread` and using thread pool threads?


**Concepts**
- raw Thread creation cost
- thread pool reuse and amortization
- OS thread handle and stack
- ThreadPool.QueueUserWorkItem
- when dedicated thread is justified

**Answer:** The key distinction is ownership and cost. A raw `Thread` is a dedicated OS resource I create, run once, and discard — the CLR never recycles it. Each raw thread reserves approximately 1 MB of stack space on Windows, an OS thread handle, and scheduler-tracking overhead, which means 400 simultaneous threads can exhaust several hundred megabytes before any actual work begins. A thread pool thread, by contrast, is borrowed from a managed pool the CLR maintains and tunes automatically through its hill-climbing algorithm. The pool reuses idle workers for new work items, so creation overhead is amortized across many tasks. I reach for raw threads only when I need a persistent long-lived CPU worker with a specific name for diagnostics, an explicit foreground lifetime to keep the process alive, or a non-default apartment state for COM interop. For everything else — short tasks, I/O callbacks, batch processing — pooled threads via `ThreadPool.QueueUserWorkItem` or `Task.Run` are the right default because they keep memory and scheduling overhead bounded regardless of workload size.

---

## Q9. What are `Thread.Name`, `IsBackground`, `Priority` — which actually affect scheduling?


**Concepts**
- Thread.Name diagnostic label
- IsBackground process exit behavior
- ThreadPriority OS scheduling hint
- no guaranteed priority enforcement
- foreground vs background effect

**Answer:** `Thread.Name` is a pure diagnostic label: the debugger and thread-dump tools display it, but the scheduler ignores it entirely. `IsBackground` actually affects behavior — a background thread (`IsBackground = true`) is terminated automatically when all foreground threads exit, while a foreground thread keeps the process alive until its delegate returns. This means forgetting to set `IsBackground = true` on a worker that should be daemon-style can silently prevent process shutdown. `ThreadPriority` is an OS hint that the scheduler may honor or ignore depending on the platform, load, and other threads; relying on `AboveNormal` or `Highest` for correctness is a design smell since it provides no guarantees and can starve lower-priority system threads. The practical guidance is to use `IsBackground` intentionally, use `Thread.Name` for every worker thread, and avoid `ThreadPriority` except in unusual real-time scenarios with profiling evidence.

---

## Q10. What is a race condition at the thread level, and how can two threads interleave unpredictably?


**Concepts**
- read-modify-write non-atomicity
- thread interleaving
- lost update
- i++ as three operations
- memory visibility across cores

**Answer:** A race condition occurs when correctness depends on the timing or order of operations across two or more threads accessing shared memory, because the CPU and compiler are free to interleave instructions in any order between threads. The classic example is `i++`, which compiles to three operations: read `i`, add 1, write back. If Thread A reads the value (say, 5) and is descheduled before writing, Thread B reads the same 5, increments it to 6, and writes back; when Thread A resumes it also writes 6 — so two increments produce a count of 6 instead of 7, a "lost update." Because this interleaving is non-deterministic, the bug appears only under certain timing conditions, making it reproducible in production but invisible in single-threaded tests. The fix requires either making the operation atomic (`Interlocked.Increment`) or surrounding the read-modify-write with a `lock` so only one thread executes the full sequence at a time.

---

## Q11. What is the difference between kernel threads and managed threads (conceptual model)?


**Concepts**
- kernel thread as OS unit
- managed thread as CLR abstraction
- 1:1 mapping in .NET
- thread ID vs OS thread
- CLR scheduler abstraction

**Answer:** A kernel thread is an OS-level execution unit tracked by the operating system scheduler; it has a kernel stack, a set of CPU registers, and an OS thread handle, and the OS is responsible for switching between them. A managed thread is the CLR's abstraction over a kernel thread: it adds a managed stack, a CLR thread ID (`ManagedThreadId`), exception handling state, and garbage-collector bookkeeping. In .NET, the mapping is 1:1 — every managed thread corresponds to exactly one kernel thread, so creating many managed threads creates equally many kernel threads with their associated OS costs. The distinction matters conceptually because some environments (like coroutines or green-thread systems) decouple the two, but in .NET you should assume that each `Thread` object you create or each pool worker the CLR assigns maps to a real OS kernel thread with real scheduling and memory overhead.

---

## Q12. Why is manually creating many threads often a scalability anti-pattern?


**Concepts**
- OS thread creation overhead
- stack memory reservation
- scheduler thrashing at scale
- thread pool as scalable alternative
- context switch cost

**Answer:** The problem is that each manual thread carries fixed overhead — roughly 1 MB of reserved stack on Windows, an OS kernel handle, and the scheduler's per-thread bookkeeping — regardless of how much actual work it does. Creating hundreds of threads for independent tasks means hundreds of megabytes of reserved virtual address space and a scheduler that must time-slice among all of them, which degrades throughput because the CPU spends more cycles on context switching than on useful work. Beyond ~a few hundred threads on a typical machine, adding more threads often makes things slower rather than faster. The correct mental model is that threads model concurrent execution units, not work items; the thread pool already maintains the right number of workers for the hardware and adjusts dynamically, so the scalable pattern is to express parallelism through work items (`Task.Run`, `Parallel.ForEach`) and let the pool decide how many threads to use.

---

## Q13. What is `ThreadStatic`, and how does it differ from `ThreadLocal<T>`?


**Concepts**
- ThreadStatic field-level initialization
- ThreadLocal<T> per-thread factory
- Dispose support on ThreadLocal
- ThreadStatic null for new threads
- thread affinity of values

**Answer:** `[ThreadStatic]` is an attribute on a static field that makes the field's storage per-thread — each thread sees its own independent copy. The subtlety is that the initializer in the field declaration (`static int _counter = 0`) only runs for the thread that initializes the type (typically the first thread to access it); all other threads see the default value (0 for int, null for reference types), which is a common source of bugs when developers expect the initializer to run per-thread. `ThreadLocal<T>`, introduced in .NET 4, solves this by accepting a factory delegate that runs on each thread the first time the thread accesses the value — so every thread genuinely starts with the correctly initialized value. `ThreadLocal<T>` also supports `Dispose` to release per-thread state and provides `Values` to enumerate all thread-local values for aggregation, which `[ThreadStatic]` lacks. I use `ThreadLocal<T>` when I need per-thread initialization, cleanup, or aggregation across threads, and reserve `[ThreadStatic]` for scenarios where I understand the null-initial-value behavior.

---

## Q14. What exceptions can occur when aborting or interrupting threads (historical vs modern guidance)?


**Concepts**
- Thread.Abort removed in .NET Core
- ThreadAbortException history
- Thread.Interrupt throws ThreadInterruptedException
- cooperative cancellation modern approach
- OperationCanceledException

**Answer:** `Thread.Abort` and `ThreadAbortException` existed in .NET Framework to forcibly inject an exception into a running thread, but this was dangerous because the abort could fire mid-instruction inside a `finally` block or lock release, leaving objects in inconsistent states. .NET Core and .NET 5+ removed this entirely — calling `Thread.Abort` throws `PlatformNotSupportedException`. `Thread.Interrupt` is a gentler operation that causes the thread to throw `ThreadInterruptedException` the next time it enters a blocking wait state (`WaitOne`, `Join`, `Sleep`), but it does not interrupt non-blocking code and is rarely used in practice. The modern guidance is cooperative cancellation via `CancellationToken`: the worker periodically checks `token.IsCancellationRequested` or calls `token.ThrowIfCancellationRequested`, which throws `OperationCanceledException` cleanly at a controlled checkpoint and allows proper resource cleanup.

---

## Q15. How does the main thread exiting affect background work still running?


**Concepts**
- main thread exit behavior
- foreground thread keeps process alive
- background thread abrupt termination
- process shutdown sequence
- Thread.Join before exit

**Answer:** When the main thread exits, the CLR checks whether any foreground threads are still running. If there are active foreground threads, the process waits — the main method returns but the process does not exit until all foreground threads complete their delegates. Background threads have the opposite behavior: when all foreground threads are done (or the main thread exits and no foreground threads remain), the runtime terminates background threads abruptly without running their `finally` blocks or completing their current operation. This means background threads are suitable for daemon-style work where it is acceptable to be killed mid-operation on shutdown, but any background worker that does I/O, holds locks, or manages resources should be converted to use cooperative cancellation and joined before the main thread exits to ensure clean shutdown.

---

### 02. ThreadPool

## Q1. What is the thread pool in .NET, and why is it preferred over creating raw threads?

(R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?


**Concepts**
- QueueUserWorkItem fire-and-forget
- CountdownEvent completion gate
- shared result array race
- callback exception swallowed
- async completion vs queued items

**Answer:** `QueueUserWorkItem` returns immediately — the method reads `results` and publishes a summary before pool callbacks finish, so `valid` is often zero or partial. There is no synchronization, and exceptions inside callbacks would be unobserved.


**Fix (priority order):**

1. Block until all callbacks complete — use `CountdownEvent` initialized to `jobCount`, `Signal()` in a `finally` block per callback, then `done.Wait()` before counting (matches **Program.cs** Section 4).
2. Wrap callback work in try/catch and log or aggregate failures — never let pool exceptions vanish.
3. Prefer `Task.Run` + `Task.WhenAll` (ch.03) or `Parallel.For` (ch.05) when you need structured exception propagation and cancellation.
4. Return the summary only after the wait gate completes; add a timeout/cancellation token for hung batches.

```csharp
using var done = new CountdownEvent(jobCount);
for (int i = 0; i < jobCount; i++)
{
    int jobId = i;
    ThreadPool.QueueUserWorkItem(_ =>
    {
        try { results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, 8_000)); }
        catch (Exception ex) { _logger.LogError(ex, "Line {Id}", jobId); }
        finally { done.Signal(); }
    });
}
done.Wait();
```


---

## Q2. How does the thread pool manage worker threads and I/O completion threads?

(R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?


**Concepts**
- WaitHandle.WaitAll 64-handle limit
- ManualResetEvent per-job overhead
- STA thread restriction
- CountdownEvent replacement
- signal in finally block

**Answer:** `WaitHandle.WaitAll` on more than 64 handles throws `NotSupportedException` when the calling thread is STA — common in legacy COM/WPF hosts. Even when it succeeds, allocating 500 `ManualResetEvent` objects per batch is expensive compared to one `CountdownEvent`.


**Fix (priority order):**

1. Replace per-job events + `WaitAll` with a single `CountdownEvent(jobCount)` and `Signal()` in `finally` ( **Program.cs** Section 4).
2. Move `signal.Set()` into `finally` so failures cannot deadlock the waiter.
3. If you must use events, use `WaitOne` on one gate or `Task.WhenAll` — not `WaitAll` on hundreds of handles.
4. Dispose synchronization primitives via `using` on the countdown/event wrapper.


---

## Q3. What is hill-climbing in the .NET thread pool (high level)?

(R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.


**Concepts**
- sync-over-async .Result in pool callback
- IOCP vs CPU pool
- thread pool starvation
- HttpClient async API
- blocking I/O blocks workers

**Answer:** `.Result` inside a thread-pool callback blocks a worker thread for the entire HTTP wait — sync-over-async on the same pool that ASP.NET, timers, and `Task.Run` share. Under load, workers pile up blocked on I/O while the queue grows, producing apparent "deadlock" or severe latency.


**Fix (priority order):**

1. Remove sync-over-async — use `async`/`await` end-to-end: `await _httpClient.GetStringAsync(url)` on an async code path (ch.04), not `.Result` on a pool thread.
2. If you must queue, queue async work via `Task.Run` only for CPU-bound segments; I/O should use async I/O that frees workers during waits (IOCP — **Program.cs** Section 9).
3. Bound concurrency with `SemaphoreSlim` or a dedicated channel/worker so audit fetches cannot exhaust the global pool.
4. Register `IHttpClientFactory` in ASP.NET hosts instead of ad-hoc blocking calls.


---

## Q4. What is `ThreadPool.QueueUserWorkItem`, and how does it relate to `Task.Run`?

(P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?


**Concepts**
- SetMinThreads cold-start tuning
- stack reservation per thread
- fleet oversubscription
- when to tune pool minimums
- SetMaxThreads backlog risk

**Answer:** Inflating minimum threads on every instance reserves idle workers and I/O threads that consume memory (~1 MB stack each on Windows) without guaranteed throughput gain. Forty pods × 250 workers can oversubscribe an 8-core node and increase context switching while hiding the real bottleneck (slow or blocking callbacks).

- **What breaks:** RAM pressure, scheduler thrashing, false sense of capacity; does not fix blocking code — blocked threads stay blocked regardless of min count.
- **When it helps (sparingly):** Measured cold-start after idle where pool ramp-up latency dominates *short* bursts of queued work — e.g., a known spike right after deploy. Raise min modestly, validate with `GetAvailableThreads` and latency metrics, revert if idle waste grows.
- **Prefer instead:** Fix blocking/sync-over-async, use async I/O, bound parallel fan-out, scale horizontally with sensible concurrency limits — **Program.cs** Section 8 warns to use `SetMinThreads` sparingly.
- **`SetMaxThreads`:** Capping the pool can create unbounded queue backlog — rarely the first lever; fix slow callbacks first.


---

## Q5. What is starvation in the thread pool, and what causes it?

(M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?


**Concepts**
- GetAvailableThreads snapshot
- low CPU high busy-pool
- blocking worker threads
- async ADO.NET fix
- starvation vs insufficient cores

**Answer:** Worker threads blocked on I/O or locks still count as busy (`max − available`), even when they are not executing CPU instructions — so low CPU with a exhausted-looking pool usually means blocking work on the pool, not insufficient cores.

- The callback opens a DB connection and sleeps — both block the worker without consuming CPU; many such items queue up and starve unrelated pool users (timers, ASP.NET, other `QueueUserWorkItem` work).
- `GetAvailableThreads` is a snapshot — useful for trend logging, not a capacity plan by itself; pair with queue wait time, request latency, and thread-pool starvation counters.
- **First change:** Move blocking DB access to async ADO.NET (`await conn.OpenAsync`, async execute) so workers release during I/O waits (IOCP path — Section 9 preview).
- **Second:** Do not perform long synchronous DB work directly on thread-pool threads — use a bounded dedicated worker or channel with explicit concurrency.
- **Not first:** Buying cores or blindly raising `SetMinThreads` — that multiplies blocked threads, not useful parallelism.


---

## Q6. How do synchronous blocking calls inside pool threads affect throughput?

(D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?


**Concepts**
- manual Thread vs ThreadPool for batch
- 2000 threads memory cost
- pool amortization for short jobs
- Task.WhenAll batch pattern
- when manual Thread fits

**Answer:** Ship **B** (pool or modern `Task`/`Parallel` APIs) for this workload — many short, independent CPU jobs are exactly what the thread pool amortizes (**Program.cs** Section 10 comparison table).

- **Throughput:** Manual threads pay OS creation/teardown per job; the pool reuses workers and scales with processor count — the chapter's Stopwatch demo (Section 11) shows pool wins for large batches.
- **Memory:** 2,000 manual threads ≈ gigabytes of default stack reservation; the pool holds a bounded worker set capped by `GetMaxThreads`.
- **Operational risk:** Thread explosion can OOM or thrash the scheduler; pool caps growth and integrates with existing ASP.NET/`Task` infrastructure.
- **When manual `Thread` still fits:** One or few long-lived workers (custom name/priority, foreground lifetime, special apartment/stack) — not 2,000 ephemeral resize jobs.
- **Modern default:** Prefer `Task.Run` + `Task.WhenAll` or `Parallel.For` with `MaxDegreeOfParallelism` for clearer cancellation/exception handling; `QueueUserWorkItem` remains valid for legacy fire-and-forget patterns.


---

## Q7. What is the difference between dedicated threads and pool threads for long-running work?

(R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.


**Concepts**
- unsynchronized List<T>.Add
- fire-and-forget from HTTP handler
- no completion gate
- ConcurrentBag alternative
- callback try/catch/finally

**Answer:** The method queues work and returns before validations finish, mutates a non-thread-safe `List<string>` from multiple pool threads without synchronization, and never surfaces callback exceptions — so clients get 202 while data is incomplete and errors are lost.


**Fix (priority order):**

1. Decide contract: if the API must return after validation, wait with `CountdownEvent`/`Task.WhenAll` (and return 200/422 with results); if truly background, enqueue to a durable queue (Azure Service Bus, Hangfire) — not naked `QueueUserWorkItem` from a request.
2. Replace `List<string>` with `ConcurrentBag<string>` or write each failure to `results[i]` then merge after the wait — one writer per index avoids locks (**Program.cs** Section 4 pattern).
3. Wrap callback body in try/catch/finally — log and signal completion in `finally`.
4. Propagate unhandled failures to telemetry (`ILogger`, Application Insights unhandled exception tracking).


---

## Q8. What is `ThreadPool.SetMinThreads` / `SetMaxThreads`, and when might you tune them?


**Concepts**
- SetMinThreads cold-start benefit
- SetMaxThreads cap risk
- stack memory per thread
- pool ramp-up hill-climbing
- tuning with profiling evidence

**Answer:** `ThreadPool.SetMinThreads(workerMin, ioMin)` raises the floor of pre-created pool threads so the pool can immediately dispatch work without the hill-climbing ramp-up delay — useful when a service receives a predictable burst right after startup and the default ramp-up latency would cause request timeouts. `SetMaxThreads(workerMax, ioMax)` caps the total worker and I/O completion threads the pool can create; setting it too low causes a growing queue backlog and latency spikes under load, so it is rarely the right lever. The danger with both is that they are global process-wide settings: inflating minimum threads means each pre-created thread reserves stack memory even when idle, and on containerized or shared-host deployments this can oversubscribe the node. The correct approach is to measure first — use `GetAvailableThreads` counters and request latency under realistic traffic, tune modestly, and verify that the bottleneck is actually pool ramp-up rather than blocking callbacks or under-provisioned infrastructure.

---

## Q9. How does the thread pool interact with `async`/`await` continuations?


**Concepts**
- SynchronizationContext capture at await
- continuation posting back to context
- ConfigureAwait(false) skip post-back
- ASP.NET Core null context
- ThreadPool.QueueUserWorkItem for continuations

**Answer:** When a method reaches an `await` expression, the compiler-generated state machine captures the current `SynchronizationContext` (or `TaskScheduler`, if no context is present). After the awaited task completes, the continuation is scheduled back onto the captured context — which for WinForms/WPF means posting to the UI message loop, and for ASP.NET Core means null context (continuation runs on any pool thread). If `ConfigureAwait(false)` is used, the state machine does not capture the context and the continuation runs directly on whatever thread completed the task, which is usually a thread pool worker. This interaction explains the main async deadlock pattern: blocking a context-bound thread with `.Result` prevents the continuation from running on that thread, which the continuation is waiting for, creating a cycle. Library code should always use `ConfigureAwait(false)` to avoid tying continuations to the caller's context.

---

## Q10. What is the danger of blocking the UI thread vs blocking a pool thread?


**Concepts**
- UI thread frozen on blocking call
- pool thread starvation on blocking
- SynchronizationContext and deadlock
- async I/O releases thread
- blocking vs non-blocking resource use

**Answer:** Blocking the UI thread prevents the application's message pump from processing events, which freezes all user interaction — no repaints, no input, no animations — until the blocking call returns. Users experience this as the application becoming unresponsive or appearing to crash. Blocking a pool thread is less catastrophic but still harmful: the blocked thread sits idle consuming a pool slot while not making progress, and under load many blocked pool threads cause starvation — other work items queue up, latency climbs, and in ASP.NET Core this manifests as request timeouts. The fundamental fix in both cases is the same: use `await` with truly async I/O APIs so the thread is released during the wait and returned to the pool (or the UI loop) to handle other work. Never call `.Result`, `.Wait()`, or `Thread.Sleep` from a UI thread or inside an async pipeline.

---

## Q11. How do thread pool threads relate to `Parallel.For` and PLINQ?


**Concepts**
- Parallel.For queues work items on pool
- PLINQ partition workers from pool
- pool worker reuse across parallel operations
- MaxDegreeOfParallelism pool cap
- pool shared by all parallel APIs

**Answer:** `Parallel.For` and `Parallel.ForEach` work by partitioning the iteration space and submitting each partition as a work item to the `ThreadPool`'s worker thread queue — they do not create dedicated threads. PLINQ similarly partitions the source sequence and uses pool workers to process partitions in parallel before merging results. Because all these APIs share the same global thread pool, they can interfere with other pool-based work in the same process: a `Parallel.ForEach` that spawns 16 partitions on a 16-core machine consumes all pool workers and can starve `Task.Run` continuations, ASP.NET requests, or `Timer` callbacks queued at the same time. Controlling `MaxDegreeOfParallelism` in `ParallelOptions` limits how many pool threads the parallel operation claims, which is important in server applications that share the pool with request handling.

---

## Q12. What diagnostics exist for thread pool queue length and thread counts (`ThreadPool.ThreadCount`, ETW)?


**Concepts**
- ThreadPool.ThreadCount total workers
- GetAvailableThreads snapshot
- ETW thread pool events
- dotnet-counters threadpool-queue-length
- EventSource for pool diagnostics

**Answer:** The main in-process diagnostic APIs are `ThreadPool.ThreadCount` (total current thread count), `ThreadPool.GetAvailableThreads(out worker, out io)` (free worker and I/O completion threads), and `ThreadPool.GetMinThreads`/`GetMaxThreads` for configuration. For production monitoring, `dotnet-counters monitor --counters System.Runtime` reports `threadpool-thread-count`, `threadpool-queue-length`, and `threadpool-completed-items-count` in real time without attaching a debugger. ETW events via PerfView or EventPipe expose fine-grained pool events including thread injection/retirement decisions from the hill-climbing algorithm, which helps distinguish ramp-up delays from blocking-induced starvation. Pairing `threadpool-queue-length` with application latency metrics is the most actionable combination: a growing queue under sustained load while CPU is low strongly indicates blocked workers, not insufficient hardware.

---

### 03. Tasks & Task Parallel Library

## Q1. What is the Task Parallel Library (TPL)?

(R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?


**Concepts**
- .Result sync-over-async on request thread
- Task.Run for I/O-bound work
- async action signature
- CancellationToken on endpoint
- SemaphoreSlim concurrency cap

**Answer:** The action blocks the request thread with `.Result` on `Task.WhenAll`, which is sync-over-async on ASP.NET Core's thread pool and can cause starvation or deadlocks under concurrency — compounded by wrapping likely I/O-bound validation in `Task.Run`, which wastes pool threads.


**Fix (priority order):**

1. Change signature to `async Task<IActionResult> ValidateBatch(..., CancellationToken ct)` and `bool[] results = await Task.WhenAll(validations)`.
2. If validation is I/O-bound, call `_orderValidator.ValidateAsync(id, ct)` directly — no `Task.Run`.
3. Cap concurrency for large batches (`SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or chunked `WhenAll`).
4. Return early or fail fast if `orderIds` exceeds a configured limit.


---

## Q2. Explain the difference between `Thread` and `Task` in purpose and scheduling.

(R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?


**Concepts**
- unattached nested Task.Run
- parent completes before children
- TaskCreationOptions.AttachedToParent
- Task.Factory.StartNew scheduler
- Task.WhenAll for child collection

**Answer:** The parent task completes as soon as the `StartNew` delegate returns — before nested `Task.Run` children finish — so the caller releases the dock slot while picks are still in flight. `Task.Factory.StartNew` without an explicit scheduler also inherits `TaskScheduler.Current`, which can inline work unexpectedly.


**Fix (priority order):**

1. Collect child tasks and wait for all: `var picks = order.Lines.Select(line => Task.Run(() => Pick(line))).ToArray(); Task.WhenAll(picks).Wait();` inside the parent — or prefer `async`/`await` with `await Task.WhenAll(picks)` (ch.04).
2. For parent/child lifetime linking, use `TaskCreationOptions.AttachedToParent` with explicit `TaskScheduler.Default` on `StartNew` — as in **Program.cs** Section 12 — only when you truly need attached semantics.
3. Replace outer `Task.Factory.StartNew` with `Task.Run` unless non-default creation options are required.
4. Propagate exceptions: observe all child tasks; a faulted pick must fail the fulfillment operation, not disappear.


---

## Q3. Explain `Task`, `Task<T>`, and `ValueTask<T>` — when to use each.

(R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?


**Concepts**
- TaskCompletionSource event handler leak
- TrySetResult vs SetResult
- timeout registration
- no completion path hang
- idempotent TCS completion

**Answer:** The wrapper leaks event handlers on every call, uses throwing `SetResult`/`SetException` instead of `TrySet*`, and has no path to complete the task if the gateway never fires — so callers hang. A second callback can throw `InvalidOperationException` when the task is already completed.


**Fix (priority order):**

1. Use `TrySetResult`, `TrySetException`, `TrySetCanceled` and unsubscribe handlers in the callback after the first terminal signal.
2. Register `CancellationToken` / `CancelAfter` to call `TrySetCanceled` when the HTTP/client timeout fires.
3. Create one `TaskCompletionSource` per `ChargeAsync` invocation (already implied) — never share across concurrent calls.
4. Optionally wrap with `Task.WhenAny(tcs.Task, timeoutTask)` for defense in depth.

```csharp
void CompleteOnce(Action complete) { if (tcs.TrySetResult(default!)) { /* use TrySet* */ complete(); } }
// Prefer: if (tcs.TrySetResult(result)) { _gateway.PaymentCompleted -= handler; }
```


---

## Q4. What is `Task.Run`, and when should it be used vs when it should be avoided?

(R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?


**Concepts**
- ContinueWith default runs on any state
- TaskContinuationOptions.OnlyOnRanToCompletion
- AggregateException from antecedent.Result
- fault handler branch
- await vs ContinueWith stack traces

**Answer:** When the antecedent is faulted, the continuation still runs by default and accessing `antecedent.Result` rethrows — often as `AggregateException` — instead of routing to a fault handler. Unobserved or poorly observed faulted continuations can tear down the process via `TaskScheduler.UnobservedTaskException`.


**Fix (priority order):**

1. Add status filter: `ContinueWith(..., TaskContinuationOptions.OnlyOnRanToCompletion)` for the label step.
2. Add fault handler: `pickTask.ContinueWith(t => Log(t.Exception), TaskContinuationOptions.OnlyOnFaulted)`.
3. Prefer `await pickTask` / `await pickTask.ContinueWith(...)` (ch.04) — compiler preserves stack traces better than manual `ContinueWith`.
4. Replace final `.Result` with `await labelTask` or `GetAwaiter().GetResult()` only at a true sync boundary.


---

## Q5. What is `Task.Factory.StartNew`, and why is `Task.Run` usually preferred?

(P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?


**Concepts**
- SemaphoreSlim release in finally
- cancellation mid-flight leaks slot
- throttle pattern with try/finally
- CurrentCount health check
- permanent throughput collapse on exception

**Answer:** `Release()` is not in a `finally` block — any exception or cancellation after `WaitAsync` consumes a semaphore slot permanently. After enough failures, all four slots are held and every new pick blocks forever until process restart.

- Wrap the guarded work in `try/finally` and call `_pickerGate.Release()` in `finally` — matching **Program.cs** Section 10 (`ThrottledPick`).
- Prefer `await _pickerGate.WaitAsync(ct)` with the same `CancellationToken` passed to downstream calls so aborting a request releases the wait cleanly.
- Consider `SemaphoreSlim` as a singleton with explicit max count documented; dispose only on application shutdown — not per request.
- Monitor `_pickerGate.CurrentCount` in health checks to detect leak regressions early.


---

## Q6. Explain task continuations with `ContinueWith` — options, scheduling, and exception handling.

(M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?


**Concepts**
- Task.WhenAny winner found
- loser tasks continue holding resources
- linked CancellationTokenSource to cancel losers
- blocking .Result on sync caller
- IHttpClientFactory lifetime

**Answer:** `Task.WhenAny` completes when the first task finishes — it does not cancel or dispose the slower tasks. Losing carrier HTTP calls continue until completion, holding connections, thread-pool slots, and memory under sustained load.

- After `WhenAny`, cancel remaining work with a linked `CancellationTokenSource` passed into each quote call, then `cts.Cancel()` once the winner is chosen.
- If APIs are not cancelable, track in-flight calls and abandon results safely — but still close/dispose `HttpResponseMessage` and respect `IHttpClientFactory` lifetimes.
- Replace blocking `.Result` with `await Task.WhenAny(...)` in an async API so the request thread is not blocked during the race.
- Log slow-loser latency separately — persistent tail latency after "winner found" signals missing cancellation.


---

## Q7. What is `Task.WhenAll`, `Task.WhenAny`, and how do they differ from manual continuation chaining?

(D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?


**Concepts**
- bounded parallel validation
- Task.WaitAll unbounded pool spike
- Parallel.ForEachAsync with max degree
- chunked WhenAll pattern
- async vs CPU-bound task distinction

**Answer:** Launching four thousand simultaneous `Task.Run` validations queues thousands of work items at once, spiking thread-pool usage and likely overwhelming the database — `Task.WaitAll` also blocks the orchestrator thread until every task completes, with no backpressure. `Parallel.ForEach` helps CPU-bound validation but is the wrong default if validation is I/O-bound and still needs a concurrency cap for downstream limits.

- Ship chunked or throttled async orchestration: `await Task.WhenAll(batch.Select(o => ValidateAsync(o, ct)))` over batches of 50–200, or use `SemaphoreSlim` / `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` tuned to DB connection limits.
- Use `Task.Run` only for CPU-bound validation logic; I/O-bound checks should be truly async end-to-end (ch.04).
- Prefer `Task.WhenAll` over `Task.WaitAll` in async hosts — composable with cancellation and does not block a precious thread for the entire batch duration.
- Emit metrics: queue depth, validation latency p95, and faulted task count — unobserved faults in nightly jobs can fail silently until morning.


---
## Q8. What is `TaskCompletionSource<T>`, and what scenarios does it enable (bridging callbacks, manual completion)?


**Concepts**
- TaskCompletionSource bridges callbacks to Task
- manual completion timing
- SetResult/SetException/SetCanceled
- TCS for event-based APIs
- non-awaitable to awaitable conversion

**Answer:** `TaskCompletionSource<T>` is a lightweight wrapper that exposes a `Task<T>` whose completion I control manually — I can call `SetResult`, `SetException`, or `SetCanceled` at any time from any thread, and anyone awaiting `tcs.Task` unblocks accordingly. The primary scenario is bridging event-driven or callback-based APIs into the task model: I create a TCS, subscribe to the completion event, have the callback call `TrySetResult` when it fires, and return `tcs.Task` to callers who then `await` it naturally. This lets older async patterns (APM's `IAsyncResult`, `event`-based APIs, OS handle wait callbacks) compose cleanly with `async`/`await` without spawning threads. Another common scenario is manual promise logic — for example, a request-gating mechanism that suspends callers until some external condition is met, then signals all waiting tasks by completing the TCS at once.

---

## Q9. What is the difference between completing a `TaskCompletionSource` with result, exception, or cancellation?


**Concepts**
- TCS.SetResult completes task normally
- TCS.SetException faults task
- TCS.SetCanceled cancels task
- TrySet* returns false on double-complete
- idempotent completion guard

**Answer:** `SetResult(value)` transitions the task to `RanToCompletion` and stores the result; `SetException(ex)` transitions it to `Faulted` and any awaiter rethrows the exception; `SetCanceled()` transitions it to `Canceled` and awaiters receive `OperationCanceledException`. All three throw `InvalidOperationException` if called after the task is already in a terminal state — which means concurrent callbacks racing to complete the same TCS will crash on the second call. The `TrySet*` variants (`TrySetResult`, `TrySetException`, `TrySetCanceled`) are idempotent: they return `false` if the task is already complete instead of throwing, making them safe for multi-callback scenarios where only the first one should win. In production wrappers I always use the `TrySet*` variants and unsubscribe from the source event inside the callback so subsequent fires are no-ops.

---

## Q10. What is `Task.FromResult`, `Task.CompletedTask`, and when are they preferable to `Task.Run`?


**Concepts**
- Task.FromResult avoids allocation
- Task.CompletedTask for void return
- no thread pool usage
- cache of completed tasks
- synchronous fast-path optimization

**Answer:** `Task.FromResult<T>(value)` returns a `Task<T>` that is already in the `RanToCompletion` state with the specified value — it never touches the thread pool and involves no async state machine overhead, making it the right choice for synchronous fast paths in async interfaces (for example, a cache hit that can return immediately). `Task.CompletedTask` is the singleton `Task` equivalent for void-returning async methods where there is no result — it avoids allocating a new Task object on every synchronous return. Contrast this with `Task.Run(() => value)`, which unnecessarily schedules work onto the pool, creating a pool thread hop for something that needs no concurrency. I use these pre-completed tasks to satisfy async interface contracts without introducing artificial concurrency, which keeps hot paths allocation-free and avoids scheduling overhead.

---

## Q11. What is the difference between `AggregateException` and a regular exception when tasks fail?


**Concepts**
- AggregateException wraps multiple inner exceptions
- await unwraps to single inner exception
- .Result rethrows as AggregateException
- WhenAll collects all faults
- InnerExceptions collection

**Answer:** When a `Task` faults, the exception is stored inside the task and rethrown when the task is observed. How it surfaces depends on how you observe it. `await task` unwraps the first inner exception from the `AggregateException` and rethrows it directly — so `catch (IOException ex)` works naturally in async code. `.Result` and `.Wait()` wrap the original exception in an `AggregateException`, so callers must catch `AggregateException` and inspect `InnerExceptions`. With `Task.WhenAll`, all faulted tasks' exceptions are collected into a single `AggregateException` with multiple inner exceptions; `await Task.WhenAll(...)` still unwraps to only the first exception, but the others are accessible via the `AggregateException` in `Task.Exception`. The practical rule is: use `await` and let the compiler unwrap for you; fall back to `AggregateException.Handle` or `InnerExceptions` enumeration when you need all faults from a multi-task batch.

---

## Q12. How do child tasks relate to parent tasks (`TaskCreationOptions`, attached vs detached)?


**Concepts**
- AttachedToParent child lifecycle coupling
- detached child independent completion
- TaskCreationOptions.AttachedToParent
- parent waits for attached children
- fire-and-forget detached default

**Answer:** When a child task is created with `TaskCreationOptions.AttachedToParent` and executed inside a parent task, the parent task does not transition to `RanToCompletion` until all attached children complete — the parent's lifecycle is extended to encompass its children. Exceptions from attached children propagate up to the parent's `AggregateException`. Detached children (the default) are fully independent: the parent completes as soon as its own delegate returns, regardless of any nested tasks, which means fire-and-forget nested `Task.Run` calls inside a parent task give no lifetime guarantees to the parent. `AttachedToParent` is mainly used with `Task.Factory.StartNew` when you need explicit parent-child relationships, but it is rarely needed in modern async code where `await Task.WhenAll(children)` provides more explicit and composable lifetime control.

---

## Q13. What is task cancellation via `CancellationToken` registration vs `TrySetCanceled`?


**Concepts**
- CancellationToken.Register callback
- TrySetCanceled on token fire
- linked token source for timeout
- unregister token callback on completion
- cooperative cancellation of TCS

**Answer:** `CancellationToken.Register(callback)` subscribes a delegate that runs synchronously on the thread that calls `CancellationTokenSource.Cancel()` (or on the pool if canceled from a finalizer). To cancel a `TaskCompletionSource`-backed task cooperatively, I register a callback that calls `tcs.TrySetCanceled(token)` — this connects external cancellation to the TCS's lifetime cleanly. I store the `CancellationTokenRegistration` returned by `Register` and dispose it when the task completes (inside the TCS completion callback) to avoid a registration leak. `TrySetCanceled` is the direct approach when I already control the cancellation trigger — for example, in a timeout wrapper where I call `cts.CancelAfter(timeout)` and the registered callback immediately propagates the cancel signal to the task.

---

## Q14. What are unobserved task exceptions, and how does .NET handle them?


**Concepts**
- unobserved task exception event
- GC finalization fires event
- TaskScheduler.UnobservedTaskException
- .NET 4.5+ no crash by default
- always observe or handle faulted tasks

**Answer:** If a `Task` faults and no code ever observes its exception — by awaiting it, checking `.Exception`, attaching a continuation with fault handling, or calling `.Wait()` — the .NET runtime considers it an "unobserved task exception." In .NET 4.0, the finalizer thread would rethrow this and crash the process. Starting with .NET 4.5, the behavior was softened: unobserved exceptions no longer crash the process by default; instead, when the garbage collector finalizes the dead faulted task, the runtime raises `TaskScheduler.UnobservedTaskException`. I subscribe to this event for logging in production, since otherwise these silent failures produce no error trace. The correct fix is to always observe task results — every fire-and-forget task should at minimum attach `ContinueWith(t => log(t.Exception), TaskContinuationOptions.OnlyOnFaulted)`.

---

## Q15. What is `ValueTask` pooling/caching, and why must consumers avoid double-awaiting unless documented safe?


**Concepts**
- ValueTask wraps pooled IValueTaskSource
- double-await corrupts pool state
- consume ValueTask exactly once immediately
- IValueTaskSource recycling
- when ValueTask is safe to re-await

**Answer:** `ValueTask<T>` is a discriminated union that can hold either a result synchronously (no heap allocation) or wrap an `IValueTaskSource<T>` that the underlying implementation may pool and recycle. The pooling is the danger: once I `await` a `ValueTask`, the source may be returned to its pool and reused for a different logical operation. If I then `await` it a second time or call `GetAwaiter().GetResult()` concurrently from another thread, I am reading state that now belongs to a different operation, which causes `InvalidOperationException` or silent data corruption. The rule is to consume a `ValueTask` exactly once, immediately after creation, by either directly awaiting it or calling `.AsTask()` (which copies the task's state into a stable `Task<T>` object that can be awaited multiple times). Only `ValueTask` implementations that explicitly document multi-await safety (which is rare) can be awaited more than once.

---

## Q16. How do you implement a timeout around a `Task` using `CancellationTokenSource` or `WhenAny`?


**Concepts**
- CancellationTokenSource.CancelAfter timeout
- Task.WhenAny with Task.Delay race
- TrySetCanceled on timeout fire
- timeout token linked to request token
- WhenAny loser cleanup after timeout

**Answer:** The simplest pattern is `CancellationTokenSource.CancelAfter(timeoutMs)` linked to the operation's token — the operation sees cancellation and throws `OperationCanceledException` when the timeout fires. For more control I use `Task.WhenAny`: `var winner = await Task.WhenAny(workTask, Task.Delay(timeout, ct))`, then check if `winner == workTask` to distinguish completion from timeout. If a timeout is detected I cancel the work via a linked `CancellationTokenSource` and `await` the original task briefly to let it clean up. The key pitfall with `WhenAny` is that the losing task continues running unless explicitly canceled — always cancel and observe the loser to prevent resource leaks. For library code I prefer the `CancelAfter` approach because it integrates with the existing token model and does not require managing a second `Task.Delay` task.

---

### 04. Async and Await

## Q1. Explain asynchronous programming in C# — what problem does it solve?

(R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?


**Concepts**
- .Result blocking on async endpoint
- sync-over-async thread pool starvation
- async Task<IActionResult> signature
- await Task.WhenAll usage
- ConfigureAwait(false) in library call

**Answer:** The endpoint blocks a thread-pool thread twice via `.Result` and `.Wait()` on unfinished Tasks, defeating ASP.NET Core's async I/O model and risking deadlocks when a captured request context prevents continuations from running under load.


**Fix (priority order):**

1. Change the endpoint to `async Task<IResults>` (or `async Task`) and `await` both service calls end-to-end.
2. Thread `CancellationToken` from `HttpContext.RequestAborted` into `FetchMetadataAsync` and downstream I/O.
3. Ensure the service layer stays async all the way — no `.Result` / `.Wait()` anywhere in the call chain.
4. Load-test after the change — thread-pool queue length should stay flat under I/O-bound load.

```csharp
app.MapGet("/reports/{id}", async (ReportService svc, string id, CancellationToken ct) =>
{
    var metadata = await svc.FetchMetadataAsync(id, ct);
    await svc.ProcessReportAsync(metadata, ct);
    return Results.Ok(metadata);
});
```


---

## Q2. Explain the `async` and `await` keywords in detail.

(R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?


**Concepts**
- async void exception uncatchable
- process crash on unhandled async void
- fire-and-forget orchestration
- async Task vs async void
- event handler only use case for async void

**Answer:** `LogExportStarted` is `async void`, so its exception cannot be caught by the caller and propagates through the synchronization context as an unhandled exception — often terminating the worker. The fire-and-forget pipeline Task is also unobserved, so its failures are silent until an unobserved-task handler fires.


**Fix (priority order):**

1. Change `LogExportStarted` to `async Task` and await it (or fold logging into the pipeline).
2. Return `Task` from `StartExport` / `RunExportPipelineAsync` and await at the host boundary (BackgroundService, Hangfire job, Azure Function entry).
3. Wrap the pipeline in try/catch with structured logging; rethrow or surface failure to the scheduler.
4. Reserve `async void` strictly for UI event handlers — see **Program.cs** Section 4.

```csharp
public async Task StartExportAsync(string reportId, CancellationToken ct)
{
    await LogExportStartedAsync(reportId, ct);
    await RunExportPipelineAsync(reportId, ct);
}
```


---

## Q3. What is the difference between CPU-bound and I/O-bound async work?

(R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?


**Concepts**
- WPF SynchronizationContext deadlock
- ConfigureAwait(false) in library
- .GetResult() blocking UI thread
- await in library code
- UI thread captured context loop

**Answer:** The UI thread blocks on `.Result` while the async continuation tries to marshal back to the same UI thread (default `await` captures `SynchronizationContext`). The blocked UI thread cannot run the continuation — classic sync-over-async deadlock.


**Fix (priority order):**

1. **App:** Make `LoadReportOnStartup` async (`async void` acceptable for UI event) and `await ReportFetcher.GetReportAsync("Q1")`.
2. **Library:** Add `.ConfigureAwait(false)` on every await in reusable code that does not touch UI after the await — see **Program.cs** Section 5.
3. Never expose sync `.Result` / `.Wait()` wrappers from library public APIs.
4. If sync API is unavoidable at a legacy boundary, document it as UI-thread-unsafe and offload with `Task.Run` only as a last resort (still inferior to async all the way).

```csharp
// Library
await Task.Delay(100).ConfigureAwait(false);

// UI
private async void LoadReportOnStartup()
{
    string data = await ReportFetcher.GetReportAsync("Q1");
    ReportLabel.Text = data;
}
```


---

## Q4. What is `ConfigureAwait(false)`, and when should library vs application code use it?

(P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?


**Concepts**
- CancellationToken ignored by legacy client
- WhenAny with Task.Delay timeout
- CancelAfter for download timeout
- abandon vs cancel distinction
- timeout wrapper pattern

**Answer:** `Task.WhenAny` only stops awaiting the loser — the legacy download keeps running in the background after timeout or cancellation. Under repeated cancels, orphaned downloads accumulate, wasting sockets, memory, and upstream quota; callers believe work stopped but it did not.

- **Abandoned work:** When `delay` wins, `download` is never awaited — exception on faulted Task may become unobserved; successful completion is silently ignored but still consumed resources.
- **No cooperative cancel:** Legacy API cannot be interrupted — only the wrapper's wait ends.
- **Resource leaks:** Connection pool exhaustion when many users navigate away or hit client-side timeouts.

**Production pattern:**

1. Document that this is **best-effort abandonment**, not true cancellation — matches **Program.cs** Section 8.
2. Prefer upgrading the legacy client or wrapping at a process boundary you can kill (separate worker, linked `CancellationTokenSource` with timeout).
3. If stuck with `WhenAny`, track in-flight downloads in a registry; optionally use a `CancellationTokenSource` linked to caller token + timeout and log abandoned operation IDs for ops visibility.
4. Surface `OperationCanceledException` to callers but monitor background completion rate — alert if abandoned tasks pile up.
5. For HTTP specifically, migrate to `HttpClient` with `CancellationToken` and `IHttpClientFactory` rather than permanent `WhenAny` shims.


---

## Q5. How do you handle exceptions in async/await methods?

(R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?


**Concepts**
- CancellationToken not passed to retry loop
- OperationCanceledException not re-thrown
- catch swallows cancellation
- retry ignores user cancel
- pass token to Task.Delay in backoff

**Answer:** The retry loop catches `OperationCanceledException` and retries anyway, and the backoff delay ignores `ct` — so user disconnect or request abort does not stop retries until all attempts and delays finish.


**Fix (priority order):**

1. Exclude cancellation from retry filter: `catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)`.
2. Pass token to delay: `await Task.Delay(attempt * 2000, ct)`.
3. Pass `ct` into `FetchReportAsync` inside the lambda so in-flight I/O aborts cooperatively.
4. Optionally distinguish transient faults (`HttpRequestException`, 503) from permanent errors — do not retry 400-class failures.
5. Align with **Program.cs** Section 9 — retry skeleton respects `CancellationToken` and skips `OperationCanceledException`.

```csharp
catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
{
    await Task.Delay(attempt * 2000, ct);
}
```


---

## Q6. What is the difference between `async void`, `async Task`, and `async Task<T>`?

(D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?


**Concepts**
- lock statement with await compile error
- SemaphoreSlim.WaitAsync async gate
- singleton async locking
- async-compatible mutual exclusion
- lock vs SemaphoreSlim(1,1)

**Answer:** Missing `Release()` in `finally` permanently reduces the semaphore count after the first export — subsequent callers block forever on `WaitAsync`. A static gate also only serializes within one process, not across scaled-out instances.

- **Missing `finally` / `Release`:** One successful path consumes the permit; second export deadlocks all waiters — matches broken mutex usage, not the **Program.cs** Section 10 pattern.
- **Exception before `Release`:** Any fault between `WaitAsync` and manual release leaks a permit — always `try/finally`.
- **Cancellation during `WaitAsync`:** Correctly throws without acquiring — OK; cancellation during write must still run `Release` if wait succeeded.
- **Multi-instance:** Static `SemaphoreSlim` is per-process — two pods write concurrently to shared storage; need distributed lock (blob lease, Redis RedLock, DB advisory lock) for cluster-wide exclusivity.
- **Singleton + static gate:** Redundant — instance field on scoped service or explicit distributed lock is clearer for testability.

**Correct pattern:**

```csharp
await ExportGate.WaitAsync(ct);
try
{
    await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
}
finally
{
    ExportGate.Release();
}
```

For multi-node: replace in-memory gate with storage-level lease; keep `SemaphoreSlim` only for single-process throttling.


---

## Q7. What is an async stream (`IAsyncEnumerable<T>`) in C# 8+, and how does `await foreach` work?

(M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?


**Concepts**
- ValueTask double-await corruption
- IValueTaskSource pool reuse
- InvalidOperationException on reuse
- consume ValueTask once
- pooled ValueTask semantics

**Answer:** A `ValueTask` / `ValueTask<T>` must be consumed exactly once — storing it in a field and awaiting it from two concurrent callers violates that rule, producing `InvalidOperationException` when the second await tries to reuse the same instance.

- **Single consumption:** Unlike `Task`, `ValueTask` may wrap a pooled `IValueTaskSource` — double-await is undefined.
- **Concurrency:** Two threads can both see `_cachedCount is null` and create races even before double-await.
- **Preview scope:** **Program.cs** Section 13 warns — do not await a `ValueTask` twice or store for later; consume immediately.

**Safe caching options:**

1. Cache the **`Task<int>`** (or `int` after completion), not the `ValueTask` — e.g. `private Task<int>? _countTask;` assigned once under lock.
2. Use `Lazy<Task<int>>` or `AsyncLazy<T>` pattern for memoized async initialization.
3. If the method often completes synchronously (cache hit), return `ValueTask.FromResult(cachedInt)` on the hot path and only allocate `Task` on miss — but never share one `ValueTask` instance across callers.
4. For concurrent first access, use `SemaphoreSlim` or `lock` around cache population.

```csharp
private Task<int>? _countTask;

public Task<int> GetRowCountAsync(string reportId) =>
    _countTask ??= ComputeCountAsync(reportId).AsTask();
```


---

## Q8. How does the async state machine work under the hood (high level: `MoveNext`, `IAsyncStateMachine`)?


**Concepts**
- async state machine struct
- IAsyncStateMachine.MoveNext
- await suspension point
- locals captured as state machine fields
- compiler-generated MoveNext dispatch

**Answer:** When the compiler processes an `async` method, it generates a private struct (or class in debug mode) that implements `IAsyncStateMachine`. This struct has one field per local variable and per `await` expression's awaiter, plus an integer state field. The first call to the method runs synchronously until the first incomplete `await`, captures all locals into the state machine fields, and returns an incomplete `Task`. Later, when the awaited operation completes, the continuation callback calls `MoveNext()` on the state machine, which resumes execution at the correct `await` point by switching on the state integer. The `MoveNext` method contains all the logic of the original async method, rewritten as a state machine: before the first await, between awaits, and after the last one. Understanding this explains why `async` methods have allocation overhead (the state machine heap object if it escapes) and why locals survive across awaits even though the thread may have changed.

---

## Q9. What is synchronization context, and how does it affect continuation marshaling?


**Concepts**
- SynchronizationContext captured at await
- continuation posted back to captured context
- WPF/WinForms UI thread context
- ASP.NET Core null context
- ConfigureAwait(false) skips post-back

**Answer:** At each `await`, the compiler-generated state machine captures `SynchronizationContext.Current` (falling back to `TaskScheduler.Current`). After the awaited task completes, the continuation is posted back to the captured context via `context.Post(MoveNext, null)`. For WinForms/WPF UI threads the context is the message-loop dispatcher, so continuations automatically return to the UI thread — this is what allows `await` to flow naturally without explicit `Invoke` calls. ASP.NET Core installs no context (`null`), so continuations run on any available thread pool worker. `ConfigureAwait(false)` instructs the state machine to skip the post-back: the continuation runs directly on the thread that completed the task, which is typically a pool worker. Library code should always `ConfigureAwait(false)` to avoid capturing the caller's context, which prevents deadlocks and reduces overhead in non-UI hosts.

---

## Q10. Why can `.Result`, `.Wait()`, and `.GetAwaiter().GetResult()` cause deadlocks?


**Concepts**
- .Result/.Wait() blocks calling thread
- UI context needed for continuation
- continuation waits for blocked thread
- deadlock in SynchronizationContext apps
- GetAwaiter().GetResult() same risk

**Answer:** The deadlock happens because of how context-based scheduling works. Consider a WPF application: the UI thread has a `SynchronizationContext`. When code calls `.Result` or `.Wait()` on an incomplete task, it blocks the UI thread waiting for the task to complete. The async method inside that task reaches an `await` and schedules its continuation to post back to the captured UI `SynchronizationContext` — but the UI thread is blocked waiting for the task to finish. Neither can proceed: the UI thread waits for the task, the task's continuation waits for the UI thread. This does not occur in ASP.NET Core (null context) or pure thread-pool code, but it is a reliable deadlock in WinForms, WPF, and classic ASP.NET. The fix is to always `await` async calls rather than blocking, or to `ConfigureAwait(false)` in library code so the continuation does not need the captured context.

---

## Q11. What is the difference between `await task` and `return task` from an async method (async method builder behavior)?


**Concepts**
- await task creates state machine
- return task skips async overhead
- exception context differs between forms
- elided async for passthrough
- stack trace preservation with await

**Answer:** When I write `await task` inside an `async` method, the compiler generates a state machine that suspends at that point, captures the continuation, and resumes when the task completes — which means any exception thrown by the task is re-thrown at the `await` site with a preserved stack trace. When I write `return task` (eliding `async`/`await`), the method returns the task directly without creating a state machine, which is slightly more efficient (no allocation, no extra MoveNext hop). However, exceptions thrown by the task surface at the caller's `await` rather than at this method's location, which loses the intermediate method from the exception stack trace. This matters for debugging complex call chains. The practical rule: use `return task` (elide async) for simple passthrough methods with no logic after the await; use `await task` whenever there is any logic after the await, try/finally, or when stack-trace fidelity is important.

---

## Q12. How do you implement retry with exponential backoff in async code?


**Concepts**
- try/catch per attempt in loop
- catch transient exception types only
- exponential delay with Math.Pow
- pass CancellationToken to Task.Delay
- max-attempt guard

**Answer:** I structure retry with a loop that catches only transient exception types, applies an exponential delay between attempts, and always forwards the `CancellationToken` to both the operation and the delay. A basic skeleton looks like: `for (int attempt = 0; attempt < maxAttempts; attempt++) { try { return await operation(ct); } catch (TransientException) when (attempt < maxAttempts - 1) { await Task.Delay(baseMs * (int)Math.Pow(2, attempt), ct); } }` — the `when` guard prevents catching on the final attempt, letting the exception propagate naturally. The `CancellationToken` is passed to `Task.Delay` so that a user cancellation immediately exits the backoff rather than waiting the full delay interval. I also add jitter to the delay to prevent synchronized retries from multiple clients hitting the server at the same moment.

---

## Q13. What is jitter in backoff strategies, and why is it used?


**Concepts**
- jitter adds random delay offset
- thundering herd prevention
- synchronized client retry storms
- random delay distribution
- Polly jitter strategy

**Answer:** Jitter adds a random offset to the exponential backoff delay so that many clients do not all retry at the same instant after a shared failure. Without jitter, if 1,000 clients all hit an error at time T, they all wait exactly 2 seconds and retry simultaneously, creating a synchronized load spike that likely fails again — this is the "thundering herd" problem. With jitter, each client waits T × (2^attempt) + random(0, jitter_range), spreading retries across a window and allowing the server to recover gradually. The random distribution can be uniform, decorrelated (Polly's preferred approach), or full jitter depending on the retry density required. In production I use Polly's `RetryOptions.UseJitter = true` (Polly v8) or the classic `Random.Shared.Next(0, jitterMs)` addition to the computed delay.

---

## Q14. How do Polly-style resilience policies relate to manual retry loops?


**Concepts**
- Polly declarative resilience policies
- retry/circuit breaker/timeout/bulkhead
- composable policy pipeline
- manual try/catch verbose and error-prone
- Polly.Core ResiliencePipeline in .NET 8

**Answer:** Polly (now part of Microsoft.Extensions.Resilience in .NET 8+) provides declarative, composable resilience policies that I configure once and apply through a pipeline — retry, circuit breaker, timeout, bulkhead isolation, and hedging can be layered without nesting try/catch blocks. A manual retry loop is 15-30 lines of code per call site and usually gets details wrong: swallowing cancellation, not re-throwing on the final attempt, missing jitter, no circuit state tracking. Polly handles all these correctly by design, offers telemetry hooks, and lets me change strategy (retry vs. circuit break) without touching business logic. The main scenario where I write manual retry is when I need to embed domain-specific logic between attempts (like refreshing an OAuth token only on 401) that does not fit Polly's generic callback model — but even then I build on Polly's backoff helpers rather than rolling raw `Task.Delay` arithmetic.

---

## Q15. What is `CancellationTokenSource.CreateLinkedTokenSource`, and when is linking tokens needed?


**Concepts**
- CreateLinkedTokenSource combines multiple tokens
- fires when any source cancels
- request token plus timeout token
- Dispose linked source after use
- composed cancellation scope

**Answer:** `CancellationTokenSource.CreateLinkedTokenSource(token1, token2, ...)` returns a new `CancellationTokenSource` whose token fires when any of the input tokens is canceled. The common scenario is combining a caller's request cancellation token with an internally managed timeout token: `using var linked = CancellationTokenSource.CreateLinkedTokenSource(requestCt, timeoutCts.Token)` — I pass `linked.Token` to the downstream operation, which will be canceled if either the user cancels the request or the internal timeout fires first. I always `Dispose` the linked source after the operation completes, since it holds internal subscriptions to the source tokens that will otherwise leak. Linked tokens are also useful in orchestrators that coordinate multiple parallel operations: a single root cancellation automatically propagates through all linked child tokens without each child needing to check multiple separate conditions.

---

## Q16. How do you propagate cancellation through layered async APIs?


**Concepts**
- thread CancellationToken through every async method
- pass to all I/O calls
- never swallow OperationCanceledException
- propagate cancellation to callers
- graceful async cancellation chain

**Answer:** The pattern is to thread a `CancellationToken` parameter through every async method from the outermost entry point down to every I/O or delay call. This means every method signature includes `CancellationToken ct = default`, every database query passes `ct`, every `HttpClient` call passes `ct`, and every `Task.Delay` inside retry loops passes `ct`. Intermediary methods must never swallow `OperationCanceledException` — they should let it propagate to the caller who owns the token source and can decide how to react. In ASP.NET Core, the framework passes `HttpContext.RequestAborted` automatically to action parameters typed as `CancellationToken`, so the propagation starts for free at the controller level. The anti-pattern is to accept `CancellationToken` at the API boundary but then pass `CancellationToken.None` to downstream calls, which makes cancellation a no-op for actual I/O.

---

## Q17. What is `Task.Delay` vs `Thread.Sleep` in async methods?


**Concepts**
- Thread.Sleep blocks OS thread
- Task.Delay releases thread
- cooperative timer on thread pool
- always use Task.Delay in async methods
- Thread.Sleep in async causes starvation

**Answer:** `Thread.Sleep(ms)` blocks the current OS thread for the specified duration — it stays allocated, consuming stack memory and a pool slot, doing nothing. `Task.Delay(ms)` sets a timer and returns a `Task` that completes after the delay; the current `await`-ing method suspends, the thread is released back to the pool or message loop, and a different thread picks up the continuation after the timer fires. In an async method, `Thread.Sleep` defeats the entire purpose of async: it ties up a thread pool worker during the wait, contributing to pool starvation under load. `Task.Delay` is the correct async-friendly pause — it uses one OS timer object regardless of how many concurrent delays are active, compared to `Thread.Sleep` which requires one blocked thread per concurrent sleep. The only exception is benchmark or test code that intentionally needs to simulate blocking work rather than async I/O.

---

## Q18. What is "async all the way" — why is mixing blocking and async problematic?


**Concepts**
- sync over async blocks thread pool
- .Wait() negates async benefits
- async must propagate to I/O boundary
- async all the way up the call stack
- mixing blocking and async risks deadlock

**Answer:** "Async all the way" means that once I introduce `async`/`await` at the bottom of the call stack (at the actual I/O), every method above it must also be `async Task` and `await` the call below — there should be no synchronous blocking (`.Wait()`, `.Result`) anywhere in the chain. Mixing blocking into an async chain negates the threading benefits: the blocked thread cannot be released, pool slots are consumed by waiting rather than working, and in context-bound environments (UI, classic ASP.NET) it creates deadlock risk. The main motivation for mixing is "I can't change this synchronous interface" — the correct answer there is to expose both sync and async overloads at the boundary, not to block inside an async chain. The anti-pattern is particularly destructive in high-concurrency ASP.NET Core services where each blocked thread represents one fewer request the server can handle concurrently.

---

## Q19. How do you unit test async methods and time-dependent retry logic?


**Concepts**
- async Task test methods in xUnit
- ISystemClock abstraction for time
- CancellationTokenSource for retry test
- await test assertions
- avoid Thread.Sleep in tests

**Answer:** xUnit, NUnit, and MSTest all support `async Task` test methods natively — I write `public async Task MyTest()` and the framework awaits the returned task, so `await`-based assertions work naturally. For testing retry or time-dependent logic I abstract time through an `ISystemClock` or `TimeProvider` interface (introduced in .NET 8) and inject a fake implementation that advances time programmatically, avoiding real sleeps in tests. To verify cancellation behavior I create a `CancellationTokenSource` with a short timeout and check that `OperationCanceledException` is thrown: `cts.CancelAfter(100); await Assert.ThrowsAsync<OperationCanceledException>(() => sut.DoWorkAsync(cts.Token))`. I never use `Thread.Sleep` or real delays in unit tests since they slow CI and introduce flakiness; instead I use `Task.Yield()` in fake I/O to force genuine async suspension so state machines are exercised properly.

---

## Q20. What is `IAsyncDisposable`, and how does `await using` work?


**Concepts**
- IAsyncDisposable.DisposeAsync returns ValueTask
- await using calls DisposeAsync
- async cleanup for DB connections and streams
- DisposeAsync graceful resource release
- await using scope boundary

**Answer:** `IAsyncDisposable` exposes a single method `ValueTask DisposeAsync()` that performs asynchronous cleanup — flushing buffers, closing network connections, or draining channels — and returns a `ValueTask` the caller awaits. `await using` is the syntactic sugar that calls `DisposeAsync()` when the scope exits, equivalent to a `try/finally` that `await`s the disposal. This is important for resources that need genuinely async cleanup: synchronously disposing a database connection or a gRPC channel may block waiting for pending operations to drain, while `DisposeAsync` can do that asynchronously. The practical rules are: implement `IAsyncDisposable` on types that own async resources; always use `await using` rather than plain `using` for such types; and if a type must support both sync and async disposal, implement `IDisposable` (which does a sync best-effort cleanup) alongside `IAsyncDisposable`, but only call one per instance.

---

### 05. Parallel Programming

## Q1. What is `Parallel.For` and `Parallel.ForEach`?

(R) A nightly warehouse job sums reconciled inventory values in parallel. Finance reports totals that drift from the serial baseline. Review the hot path:

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?


**Concepts**
- parallel inventory aggregation race
- shared accumulator without synchronization
- Parallel.ForEach thread-local overload
- Interlocked.Add for atomic merge
- per-partition accumulation pattern

**Answer:** `runningTotal += …` is not atomic — parallel workers read-modify-write the same `decimal` and lose updates, so totals are nondeterministic. It appears to pass when contention is low or the batch is small, then drifts under heavier parallel scheduling.


**Fix (priority order):**

1. Use thread-local aggregation on `Parallel.For` / `Parallel.ForEach` — merge once per worker, not per line (see **Program.cs** Section 2b and `ThreadSafeDecimal.Add`).
2. Prefer `batch.AsParallel().Sum(r => r.ReconciledValue)` for a read-only reduction — PLINQ handles partition/merge internally.
3. If you must share one accumulator, merge under `lock` only in the finalizer delegate — never inside the per-item body.
4. Add a serial golden-sum test in CI for the same batch to catch drift before finance does.

```csharp
decimal total = 0m;
Parallel.ForEach(
    batch,
    () => 0m,
    (record, _, local) => local + record.ReconciledValue,
    local => ThreadSafeDecimal.Add(ref total, local));
```


---

## Q2. What is `ParallelOptions` (`MaxDegreeOfParallelism`, `CancellationToken`) used for?

(R) A teammate parallelizes audit-log line generation for the same SKU batch:

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?


**Concepts**
- parallel log generation shared List
- ConcurrentBag for parallel collection
- lock vs concurrent collection
- thread-safe aggregation
- Parallel.ForEach body collects to local

**Answer:** `List<T>` is not thread-safe — concurrent `Add` calls corrupt internal state (exceptions, lost entries, or rare structural damage). Ordering after the fact does not fix the race, and parallel iteration over a non-indexable `IEnumerable` may buffer or enumerate unsafely depending on the source.


**Fix (priority order):**

1. Replace with `ConcurrentBag<string>` for lock-free per-thread staging, then sort once at the end — matches the audit pattern in **Program.cs** Section 3 (lock shown there; concurrent collection is cleaner at scale).
2. Materialize to `List<StockRecord>` or an array **before** parallel work if the source is deferred or not thread-safe.
3. If order must be deterministic and cheap, consider parallel map into a pre-sized `string[]` by index when indices exist.
4. Never share a `DbContext` or single connection across parallel bodies — load the batch in one scoped query first.

```csharp
var bag = new ConcurrentBag<string>();
Parallel.ForEach(batchList, record =>
{
    bag.Add($"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}");
});
return bag.OrderBy(l => l).ToList();
```


---

## Q3. What is a `Partitioner<TSource>`, and when would you supply a custom partitioner?

(R) Under load, a reporting endpoint times out and thread-pool starvation alerts fire. Review the "optimization" added to fetch order details:

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?


**Concepts**
- Parallel.ForEach with async delegate
- GetAwaiter().GetResult() in parallel body
- pool starvation from sync-over-async
- Parallel.ForEachAsync .NET 6
- SemaphoreSlim + Task.WhenAll alternative

**Answer:** This is I/O-bound work forced through parallel sync-over-async — each iteration blocks a thread-pool thread waiting on HTTP, while `Parallel.ForEach` multiplies concurrent blocked threads. Combined with an unsynchronized `List<T>`, you get starvation, wrong results, and socket exhaustion.


**Fix (priority order):**

1. Make the action async end-to-end: `Parallel.ForEachAsync` ( .NET 6+ ) or `Task.WhenAll` with a `SemaphoreSlim` cap — **not** `Parallel.ForEach` + `.Result`.
2. Use `IHttpClientFactory` and pass `CancellationToken` from `HttpContext.RequestAborted`.
3. Collect results in a thread-safe structure or pre-sized array indexed by position.
4. Cap concurrency (`MaxDegreeOfParallelism` or semaphore) to protect the internal API and thread pool.

```csharp
public async Task<IActionResult> ExportOrders(int[] orderIds, CancellationToken ct)
{
    var lines = new string[orderIds.Length];
    await Parallel.ForEachAsync(
        orderIds.Select((id, i) => (id, i)),
        new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = ct },
        async (item, token) =>
        {
            lines[item.i] = await _httpClient.GetStringAsync($"/internal/orders/{item.id}", token);
        });
    return Ok(lines);
}
```


---

## Q4. What is the difference between range partitioning and chunk partitioning?

(P) A CPU-bound pricing engine recalculates thousands of in-memory `StockRecord` rows on a 16-core VM shared with other services. A developer caps workers like this:

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?


**Concepts**
- MaxDegreeOfParallelism with Environment.ProcessorCount
- shared-core VM oversubscription
- leave cores for other services
- ParallelOptions tuning
- CPU-bound vs I/O-bound degree

**Answer:** `Environment.ProcessorCount` on a 16-core box allows 16 concurrent workers for this loop alone, which can starve Kestrel, GC, and other tenants on the same VM. The default `-1` is also often too aggressive on shared infrastructure — you need an explicit cap from configuration plus cooperative cancellation.

- Read `MaxDegreeOfParallelism` from `IOptions<PricingEngineOptions>` (e.g. 4 on a shared 16-core host) — same intent as **Program.cs** Section 5 throttling.
- Wire `CancellationToken` from `IHostApplicationLifetime.ApplicationStopping` or job timeout so deploys and scale-in cancel long batches via `OperationCanceledException` — see Section 6a.
- Leave at least one core for the web tier and system processes unless this worker runs on a dedicated node pool.
- Measure: if CPU is already saturated, raising parallelism does not help; if workers block on locks, lowering parallelism can **improve** throughput.


---

## Q5. Explain PLINQ (`AsParallel`, `WithDegreeOfParallelism`, `WithMergeOptions`).

(D) A reconciliation worker must stop processing once cumulative value crosses a credit limit — not process the entire batch. Two implementations were proposed:

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?


**Concepts**
- ParallelLoopState.Break vs Stop
- LowestBreakIteration semantics
- AggregateException from parallel loop
- Stop terminates ASAP
- Break processes lower indices first

**Answer:** Ship **Option B** for a strict cumulative threshold on ordered data — it is deterministic, needs no lock, and stops exactly when the limit is crossed. Option A adds lock contention on every iteration (often erasing parallel benefit) and still does not give strict "process until limit" semantics.

- `Break()` stops **starting** iterations with index **greater than** the break index; lower-index iterations may still be running or not yet started — see **Program.cs** Section 6b (`LowestBreakIteration`, `IsCompleted` false).
- Iterations with index ≤ break index are **not** cancelled — already-started higher-index work may still complete briefly before the loop winds down.
- Parallel order of accumulation is nondeterministic unless the batch order defines business meaning — cumulative credit limits usually require serial order or partitioned serial phases.
- If the batch is huge and per-item CPU work is heavy **and** order does not matter for the limit, consider parallel partial sums then a serial merge — not locked `Break()` on every line.


---

## Q6. When is parallelization slower than sequential execution?

(R) A dashboard query was "speed up" with PLINQ. Users see wrong top-SKU ordering under load and elevated CPU:

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?


**Concepts**
- PLINQ AsOrdered merge overhead
- wrong ordering without AsOrdered
- AsUnordered performance benefit
- order-preserving vs throughput
- PLINQ ordering cost

**Answer:** The first query already applies a global `OrderByDescending` — PLINQ merges partitions correctly for that operator, so "wrong ordering" likely comes from **nondeterministic ties** (equal `ReconciledValue`) or from mutating `batch` during the query, not from missing `AsOrdered()`. Adding `AsOrdered()` before `OrderByDescending` forces ordered merge overhead **twice** and hurts CPU without fixing tie-breaking.


**Fix (priority order):**

1. For dozens/hundreds of rows, use serial LINQ — often faster than PLINQ setup cost on tiny sequences (**Program.cs** tiny-array demo).
2. If CPU work per row is large and count is thousands+, keep `AsParallel()` but drop redundant `AsOrdered()` unless downstream requires input-order preservation without a sort key.
3. Add deterministic tie-break: `.OrderByDescending(r => r.ReconciledValue).ThenBy(r => r.Sku)`.
4. If the pipeline is filter + top-N only on a hot API path, consider pre-indexing or caching — not parallel LINQ on every request.


---

## Q7. What types of workloads benefit from PLINQ vs `Parallel.ForEach`?

(M) A partitioner was introduced to reduce scheduling overhead on a uniform-cost batch, but throughput dropped on a 4-core machine:

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?


**Concepts**
- static range partitioner fixed chunks
- chunk partitioning adaptive
- range partition misses work-stealing
- 4-core range partition imbalance
- custom Partitioner use case

**Answer:** `rangeSize: 1` creates one partition per index — for 12 trivial iterations that means 12 delegate invocations, partition handoffs, and thread-pool scheduling rounds. On small uniform work, that overhead dominates the multiply, so parallel is slower than a serial loop (**Program.cs** Section 11 — five-item demo).

- **Mechanism:** TPL partition granularity trades load balance against scheduling cost — micro-partitions maximize stealing flexibility but explode fixed overhead per chunk.
- **12 cheap ops:** Do not parallelize — serial `for` or simple LINQ is correct.
- **Large uniform batch:** Use default partitioning or `Partitioner.Create(0, count)` without forcing `rangeSize: 1`; let the runtime pick chunk sizes dynamically (**Section 7**).
- **Uneven per-item cost:** `Partitioner.Create(list, loadBalance: true)` for dynamic chunk stealing when row work varies widely.
- **Fixed moderate chunks:** `Partitioner.Create(0, count, rangeSize: 64)` (or similar) when items are uniform and count is in the thousands.


---

## Q8. What are thread-safe requirements when using parallel loops (shared state, locals, aggregation)?


**Concepts**
- no shared mutable state in parallel body
- thread-local variables for accumulation
- Parallel.ForEach localInit/body/localFinally
- lock for final merge only
- immutable inputs per worker

**Answer:** The core rule is never write to shared mutable state from a parallel loop body without synchronization, since multiple partitions execute concurrently. The recommended pattern for aggregation is to use the three-argument overload of `Parallel.ForEach` with `localInit`, `body`, and `localFinally` delegates: `localInit` returns a thread-local accumulator, `body` updates it without any locking, and `localFinally` merges the local result into the global accumulator using `Interlocked` or a `lock`. This keeps the hot path (body) lock-free while still producing a correct global result. If the shared state is a collection, I use `ConcurrentBag<T>` or `ConcurrentQueue<T>` instead of a `List<T>` with a lock, since concurrent collections use fine-grained or lock-free algorithms that scale better under high contention.

---

## Q9. How do you perform parallel aggregation with `lock`, `Interlocked`, or thread-local accumulators?


**Concepts**
- Parallel.ForEach thread-local accumulator
- localInit returns initial value
- localFinally merges with Interlocked
- per-partition sum pattern
- no lock in hot body

**Answer:** The three-argument `Parallel.ForEach` overload is the idiomatic way: I provide `() => 0m` as `localInit` to create a per-partition decimal accumulator, `(item, state, local) => local + item.Value` as the body to accumulate without any lock, and `local => Interlocked.Add(ref total, (long)(local * 100)) / 100m` (or use a lock on a decimal field) as `localFinally` to atomically merge. Alternatively I can collect per-partition sums in a `ConcurrentBag<decimal>` and sum them after the loop, or use PLINQ's `.AsParallel().Sum(x => x.Value)` which handles partitioning and aggregation internally. The anti-pattern is `lock(_lock) { total += item.Value; }` inside the body — this serializes every iteration and eliminates the parallelism benefit for compute-light items.

---

## Q10. What is `ParallelLoopResult`, and how do you detect partial failures?


**Concepts**
- ParallelLoopResult.IsCompleted
- LowestBreakIteration on Break
- partial completion detection
- Stop vs Break semantics
- check IsCompleted after loop

**Answer:** `Parallel.For` and `Parallel.ForEach` return a `ParallelLoopResult` struct. Its `IsCompleted` property is `true` only when all iterations ran to completion without a `Break` or `Stop` call. If `ParallelLoopState.Break` was called, `IsCompleted` is `false` and `LowestBreakIteration` holds the lowest iteration index at which Break was requested — iterations at or above that index may not have run, but all iterations below it are guaranteed to have completed. `Stop` causes `IsCompleted = false` with `LowestBreakIteration = null`, indicating an early abort with no iteration guarantee. I check `IsCompleted` after the loop to decide whether to treat the result as partial and potentially re-process missing ranges, log a warning, or fail the batch.

---

## Q11. What are ordering guarantees in PLINQ (`AsOrdered`) and their cost?


**Concepts**
- AsOrdered preserves input sequence order
- ordering requires merge buffer
- throughput penalty vs sequential merge
- AsUnordered default
- when order matters in PLINQ

**Answer:** `AsOrdered()` instructs PLINQ to preserve the input-sequence order in the output stream. Internally this requires each partition to buffer completed items and delay outputting them until all items in preceding positions are ready, which introduces extra memory overhead and a merge synchronization step. The throughput cost is measurable — for sequences where order does not matter, omitting `AsOrdered` allows PLINQ to stream results as partitions finish, which is faster. I use `AsOrdered` only when the downstream operation genuinely requires the original sequence order — for example, writing output to a positional file format — and accept the overhead consciously. For aggregation operations like `Sum`, `Count`, or `Any`, order is irrelevant and `AsOrdered` adds cost with no benefit.

---

## Q12. How does parallel LINQ decide default partition sizes?


**Concepts**
- PLINQ chunk partitioning default
- range partitioning for indexed sources
- dynamic chunk size adjustment
- work-stealing with chunks
- partition size tuning

**Answer:** For arrays and `List<T>`, PLINQ uses range partitioning by default: it divides the index range into N equal-sized contiguous chunks (one per logical core) and assigns each chunk to a worker, which eliminates synchronization overhead during enumeration since each worker accesses a disjoint index range. For non-indexed sequences (anything implementing `IEnumerable<T>` but not `IList<T>`) PLINQ uses chunk partitioning, where workers grab small batches of elements from a shared enumerator under a lock; the chunk size starts small and grows to reduce lock contention as iteration proceeds. A custom `Partitioner<T>` lets me override this — for example, using a load-balanced partitioner when work items have highly variable cost so faster workers pick up more items rather than sitting idle after finishing their fixed range.

---

## Q13. What exceptions are thrown from parallel loops (`AggregateException`, inner exceptions)?


**Concepts**
- AggregateException wraps all faulted iterations
- catch around parallel call
- InnerExceptions iteration
- parallel fault propagation
- Handle method for filtering exceptions

**Answer:** `Parallel.For` and `Parallel.ForEach` do not rethrow exceptions immediately; instead they allow all partitions to run to a natural stopping point (unless `Stop` is called), then aggregate all thrown exceptions into a single `AggregateException` that is thrown after the loop returns. I catch this with `try { Parallel.ForEach(items, body); } catch (AggregateException ae) { foreach (var ex in ae.InnerExceptions) HandleError(ex); }`. The `AggregateException.Handle(Func<Exception, bool>)` method is useful for filtering — it re-throws any exceptions where the predicate returns `false`. The key point is that one iteration throwing does not abort other concurrently running iterations; PLINQ follows the same pattern. This differs from `await Task.WhenAll` where exceptions are collected similarly into `AggregateException` but `await` unwraps to only the first one.

---

## Q14. How do you combine async I/O with parallel CPU work without blocking the pool?


**Concepts**
- Parallel.ForEachAsync for async delegates
- SemaphoreSlim plus Task.WhenAll pattern
- no async in Parallel.ForEach body
- avoid GetAwaiter().GetResult() in parallel
- MaxDegreeOfParallelism for async fan-out

**Answer:** `Parallel.ForEach` accepts only synchronous delegates, so using async lambdas results in `async void` (for non-generic overloads) which swallows exceptions, or creates a `Func<Task>` where the task is immediately discarded. Neither is safe. The correct approach for async I/O inside parallel fan-out is `Parallel.ForEachAsync` (available in .NET 6+), which accepts `Func<T, CancellationToken, ValueTask>` and integrates properly with the async model. For older .NET, I use `SemaphoreSlim` to bound concurrency and `Task.WhenAll` to await all: `await Task.WhenAll(items.Select(async item => { await _gate.WaitAsync(ct); try { await ProcessAsync(item, ct); } finally { _gate.Release(); } }))`. Never call `.GetAwaiter().GetResult()` or `.Result` inside a `Parallel.ForEach` body — that is sync-over-async and causes pool starvation.

---

## Q15. What are best practices for parallel and async code in server applications?


**Concepts**
- prefer async I/O over parallel blocking
- cap MaxDegreeOfParallelism
- CancellationToken on parallel operations
- Parallel.ForEachAsync in .NET 6+
- avoid sync-over-async in parallel bodies

**Answer:** For server applications the core principles are: prefer native async I/O over parallel blocking I/O because async releases threads during waits while parallel just adds threads for each waiter; always set `MaxDegreeOfParallelism` in `ParallelOptions` to avoid consuming the entire pool (a good starting point is `Environment.ProcessorCount / 2` for mixed workloads); always pass `CancellationToken` so operations respond to request cancellation or service shutdown; and use `Parallel.ForEachAsync` (not `Parallel.ForEach` with `async void` bodies) when loop iterations involve I/O. PLINQ is most valuable for in-memory CPU-bound transformations — applying it to database queries or HTTP calls will block pool workers and hurt throughput. The overarching guidance is to measure first: if the bottleneck is CPU saturation, parallelism helps; if it is I/O latency or thread starvation, async is the fix.

---

### 06. Synchronization and Locks

## Q1. Explain synchronization primitives: `lock`, `Monitor`, `Mutex`, and `Semaphore`/`SemaphoreSlim`.

(R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:


**Concepts**
- LedgerService singleton unsynchronized
- balance drift under concurrency
- lock object per instance
- Interlocked for atomic counter
- thread-safe balance operations

**Answer:** `Credit` and `Debit` lock on different objects (`this` vs `typeof(LedgerService)`), so they do not serialize against each other, and `Balance` reads `_balance` without any lock. The singleton shares one field across all requests — you get lost updates and torn reads.


**Fix (priority order):**

1. Use one private readonly sync root for all balance access — same object for Credit, Debit, and Balance getter (see `BankAccount._syncRoot` in **Program.cs** Section 3).
2. Lock (or use `Interlocked` only if you refactor to a single `long` cents field) on every read/write of `_balance`.
3. Re-evaluate singleton lifetime — per-tenant or scoped ledger may be required; at minimum document that this service is a process-wide counter, not per-account isolation.
4. Never lock on `this` or `typeof(LedgerService)`.

```csharp
private readonly object _sync = new();
private decimal _balance;

public void Credit(decimal amount) { lock (_sync) { _balance += amount; } }
public void Debit(decimal amount)  { lock (_sync) { _balance -= amount; } }
public decimal Balance { get { lock (_sync) { return _balance; } } }
```


---

## Q2. What is `ReaderWriterLockSlim`, and when is it preferable to a plain `lock`?

(R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:


**Concepts**
- deadlock circular lock order
- lock(a) then lock(b) vs lock(b) then lock(a)
- Monitor.Wait vs consistent ordering
- lock hierarchy to prevent deadlock
- TransferFunds lock order fix

**Answer:** `Transfer` acquires `from` then `to`, while concurrent `Transfer(beta, alpha, …)` acquires in the opposite order — classic circular wait deadlock. Nested locks on account roots that are also locked inside `Deposit`/`Withdraw` compound contention but the hang is the ordering inversion.


**Fix (priority order):**

1. Acquire locks in a consistent global order — e.g. by `AccountId` string comparison (`TransferSafely` in **Program.cs** Section 10).
2. Call internal mutators (`WithdrawInternal` / `DepositInternal`) only while both locks are held; do not re-enter public methods that lock again on the same root (reentrancy saves you here, but the pattern is fragile).
3. Remove simulated I/O from inside the lock; validate outside or use a short critical section.
4. Optionally use `Monitor.TryEnter` with timeout and retry/backoff when ordering cannot be guaranteed.

```csharp
var first  = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
var second = ReferenceEquals(first, from) ? to : from;
lock (first.SyncRoot) {
    lock (second.SyncRoot) {
        if (from.Balance >= amount) { from.WithdrawInternal(amount); to.DepositInternal(amount); }
    }
}
```


---

## Q3. Explain `AutoResetEvent`, `ManualResetEvent`, and `ManualResetEventSlim`.

(R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:


**Concepts**
- lock statement with await
- SemaphoreSlim.WaitAsync async lock
- cannot await inside lock
- singleton async gate
- lock vs SemaphoreSlim(1,1) in async

**Answer:** `RefreshAsync` is async in name only — it blocks a thread inside `lock` via `.Result` on `GetStringAsync`, which can deadlock on ASP.NET's sync context and always starves the thread pool. Holding `lock` during network I/O serializes all refreshes and blocks other readers.


**Fix (priority order):**

1. Remove `.Result` — `await _http.GetStringAsync(...)` **outside** any lock; only lock for the dictionary write (or use `ConcurrentDictionary` from ch.07).
2. Inject `IHttpClientFactory` and create clients via `CreateClient("rates")`.
3. Use `SemaphoreSlim` (not `lock`) if you need to limit concurrent refreshes — `await gate.WaitAsync(ct)` is async-safe.
4. Consider double-checked locking with versioned snapshot replace instead of locking around HTTP.

```csharp
var json = await _http.GetStringAsync($"/rates/{productCode}", ct).ConfigureAwait(false);
var rate = ParseRate(json);
lock (_sync) { _rates[productCode] = rate; }
```


---

## Q4. What is `CancellationToken`, and how do you implement cooperative cancellation?

(R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:


**Concepts**
- ReaderWriterLockSlim upgrade deadlock
- EnterUpgradeableReadLock
- nested lock upgrade hang
- UpgradeableRead must be acquired first
- read/upgrade/write lock ordering

**Answer:** The code calls `EnterWriteLock` while already holding `EnterReadLock` on the same `ReaderWriterLockSlim`. That lock type is not upgradeable — the thread blocks forever waiting for itself to release the read lock.


**Fix (priority order):**

1. Release read lock before taking write lock — exit read, enter write, re-check, insert, exit write, re-enter read for return (classic double-checked locking).
2. Or enter write lock directly when miss is likely; keep read lock only for the happy path (`GetRate` in **Program.cs** Section 7 shows the read-only path).
3. For ASP.NET Core read-heavy caches, consider immutable snapshot replace or `ConcurrentDictionary.GetOrAdd` (ch.07) to avoid manual RW lock upgrade entirely.

```csharp
_rwLock.EnterReadLock();
try {
    if (_rates.TryGetValue(productCode, out var rate)) return rate;
} finally { _rwLock.ExitReadLock(); }

_rwLock.EnterWriteLock();
try {
    if (!_rates.ContainsKey(productCode))
        _rates[productCode] = LoadDefaultFromConfig(productCode);
    return _rates[productCode];
} finally { _rwLock.ExitWriteLock(); }
```


---

## Q5. Explain deadlocks in multithreading — necessary conditions and prevention strategies.

(P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?


**Concepts**
- SemaphoreSlim for 50 concurrent calls
- Interlocked for global counter
- CancellationToken for poller shutdown
- lock for all three breaks async
- right primitive for each concern

**Answer:** Use `SemaphoreSlim(50, 50)` with `WaitAsync`/`Release` for outbound throttling, `Interlocked.Increment` (or `Interlocked.Read` patterns) for the metrics counter, and `volatile bool` or `CancellationToken` for cooperative shutdown. Using `lock` for all three serializes HTTP concurrency to one call at a time and blocks async waits inside the lock.

- **Concurrency cap (50 calls):** `SemaphoreSlim` — counting semaphore matches "N at a time" (Section 6). `WaitAsync` avoids blocking thread-pool threads during I/O.
- **Global request counter:** `Interlocked.Increment(ref _totalRequests)` — single-field atomic math without lock overhead (Section 8).
- **Cooperative shutdown:** `CancellationTokenSource.Cancel()` linked to host shutdown, or `volatile bool _stopRequested` checked in the poller loop (Section 9).

**What breaks with `lock` everywhere:**

- Throttling under `lock` during `await` — cannot await inside `lock`; you'd block one thread per wait, defeating parallelism and risking deadlocks.
- Counter under `lock` works but adds contention on every metric tick; unnecessary when `Interlocked` suffices.
- Stop flag under `lock` on every loop iteration adds latency; visibility is solved by `volatile` or `CancellationToken` without serializing the loop.


---

## Q6. What are race conditions, and how can they be prevented?

(M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:


**Concepts**
- volatile without memory barrier sufficient
- release-build optimizer reorders
- volatile keyword for flag fields
- JIT compiler and CPU reorder
- Memory.Fence or Interlocked needed

**Answer:** `_stopRequested` is a plain `bool` without `volatile` or synchronization. The JIT/CPU may cache the field in a register on the worker core, so writes from the UI thread are not guaranteed visible — the loop never observes `true`. This is the visibility problem **Program.cs** Section 9 demonstrates with `volatile bool _stopRequested`.


**Fix (priority order):**

1. Mark `_stopRequested` as `volatile bool` **or** prefer checking `externalToken.IsCancellationRequested` only and call `RequestStop` via `CancellationTokenSource.Cancel()`.
2. Wire host shutdown to cancel the same token passed to `Run`.
3. Do not use `Thread.Abort` — cooperative exit only.
4. For complex state, use `lock` around flag read/write or `Interlocked.Exchange` — overkill for a simple stop bit but valid.

```csharp
private volatile bool _stopRequested;

public void RequestStop() => _stopRequested = true;
// Better: inject CancellationToken and drop the bool entirely.
```


---

## Q7. What is the `volatile` keyword, and when does it provide visibility guarantees?

(D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:


**Concepts**
- ReaderWriterLockSlim for read-heavy fee schedule
- immutable snapshot swap with Interlocked
- read/write lock vs plain lock
- write-rarely read-often pattern
- volatile reference swap

**Answer:** Ship **Option B** (`ConcurrentDictionary` with snapshot replace on refresh) for a read-heavy ASP.NET Core pricing API. Readers never block writers except briefly during reference swap; no manual RW lock upgrade risk; scales with concurrent pricing requests.


**Trade-offs:**

- Choose **A** when refresh mutates entries in place and you need fine-grained per-key updates with strong in-process RW semantics and moderate traffic.
- Choose **B** when updates are batch/hourly and reads dominate — copy-on-write avoids long write locks and matches options-pattern snapshot refresh.
- Either way: do not expose mutable `Dictionary` without synchronization; document that pricing reads see eventually consistent fees for one refresh window.


---

## Q8. What is the difference between `volatile` and `lock` for thread safety?


**Concepts**
- volatile ensures read/write visibility
- volatile does not prevent compound races
- lock provides mutual exclusion plus visibility
- volatile sufficient for single-field flag
- lock required for multi-field invariant

**Answer:** `volatile` ensures that reads and writes to the marked field are not cached in CPU registers or reordered by the JIT across that field access — every read sees the latest write from any thread, and every write is immediately visible to all threads. But `volatile` only guarantees visibility for individual read and write operations; it does not make compound operations atomic. An increment (`i++`) still compiles to three steps (load, increment, store), and `volatile` does not prevent two threads from interleaving those steps and producing a lost update. `lock` provides both visibility (all threads see writes from within the lock before the next lock acquisition) and mutual exclusion (only one thread executes the protected region at a time), making it correct for any compound operation involving multiple reads and writes. I use `volatile` only for a single flag field (`bool _shutdown`) that one thread writes and others read — everything more complex needs `lock` or `Interlocked`.

---

## Q9. What is `Interlocked` (`Increment`, `CompareExchange`, `Add`), and when is it enough without `lock`?


**Concepts**
- Interlocked atomic hardware guarantee
- sufficient for single-field counter and CAS
- Interlocked.CompareExchange CAS pattern
- not composable for multi-field invariants
- lock needed for compound check-then-act

**Answer:** `Interlocked` provides a set of hardware-backed atomic operations — `Increment`, `Decrement`, `Add`, `Exchange`, and `CompareExchange` — that execute as a single indivisible instruction on the CPU, so no other thread can observe an intermediate state. These are sufficient when the correctness of an operation depends only on one field: `Interlocked.Increment(ref _counter)` is a correct, lock-free counter. Where `Interlocked` is not sufficient is multi-field invariants — for example, updating both a balance and a transaction log entry atomically, or performing a "check-then-act" across two fields. In those cases I need a `lock` to hold mutual exclusion across the compound operation. `Interlocked.CompareExchange` enables lock-free CAS loops for single-field swap-if-equal patterns, but those are easy to get wrong and rarely necessary given how cheap uncontended `lock` is.

---

## Q10. What is `SpinLock`, and when might low-latency spinning beat `lock`?


**Concepts**
- SpinLock tight-loop spinning
- avoids OS context switch
- microsecond critical section only
- wrong when body may block
- SpinLock vs Monitor cost tradeoff

**Answer:** `SpinLock` is a lock that busy-waits — instead of yielding the thread to the OS scheduler when the lock is unavailable, it loops checking the lock flag in a tight loop, consuming 100% CPU on that core. This avoids the OS context switch overhead (which takes several microseconds), making SpinLock faster than `Monitor` for critical sections that are held for a very short time — shorter than the context switch cost. The scenario where it wins is protecting an operation that takes nanoseconds to microseconds (updating a counter, swapping a pointer) under high contention. SpinLock is the wrong choice when the critical section might block (any I/O, lock acquisition, or anything that could be descheduled), since spinning during a long wait wastes a core completely. It is also wrong in async code since spinning blocks a pool thread. In practice I profile before reaching for SpinLock — uncontended `lock` is already very fast, and genuine contention usually points to a design problem rather than a need for spinning.

---

## Q11. What is lock ordering, and how does it prevent deadlock?


**Concepts**
- global consistent lock ordering
- acquire by object ID or pointer
- document ordering convention
- prevents circular wait condition
- lock hierarchy design

**Answer:** Lock ordering means establishing a global, consistent order in which multiple locks must always be acquired, and every thread that needs more than one lock acquires them in that order. Deadlock requires circular waiting: Thread A holds Lock 1 and waits for Lock 2 while Thread B holds Lock 2 and waits for Lock 1. If both threads always acquire Lock 1 before Lock 2, the circular dependency cannot form — one thread will always get Lock 1 first and proceed without waiting for the other. In practice I document the ordering (often by object identity: lock with lower `RuntimeHelpers.GetHashCode` first) and enforce it in code review. For `BankAccount.Transfer`, the fix is `if (from.Id < to.Id) { lock(from) lock(to) ... } else { lock(to) lock(from) ... }` — consistent ordering eliminates the deadlock regardless of which thread initiates the transfer.

---

## Q12. What is the `Monitor.TryEnter` pattern, and how do timeouts help avoid indefinite blocking?


**Concepts**
- Monitor.TryEnter returns false on timeout
- non-blocking lock attempt
- contention detection
- graceful degradation on timeout
- logging TryEnter failures

**Answer:** `Monitor.TryEnter(obj, timeout)` attempts to acquire the monitor lock and returns `true` on success or `false` if the lock was not available within the specified timeout. Unlike `lock` (which blocks indefinitely), `TryEnter` lets the thread do something useful when lock acquisition fails — log a contention warning, increment a metric, enqueue the work for later retry, or fail fast with an appropriate error. This is valuable in server applications where a long wait on a contested lock degrades overall throughput: rather than silently queuing behind a slow holder, I can detect the problem and respond. The pattern is: `if (!Monitor.TryEnter(obj, TimeSpan.FromMilliseconds(50))) { // contention, handle gracefully } else { try { /* critical section */ } finally { Monitor.Exit(obj); } }`. I always use `try/finally` around the `Monitor.Exit` call since `TryEnter` with a `bool` out parameter bypasses the `lock` statement's automatic cleanup.

---

## Q13. What is async-compatible locking (`SemaphoreSlim.WaitAsync`) vs blocking `lock` in async code?


**Concepts**
- cannot await inside lock statement
- SemaphoreSlim.WaitAsync async-compatible gate
- SemaphoreSlim(1,1) as async mutex
- never mix lock and await
- async-safe mutual exclusion pattern

**Answer:** The `lock` statement compiles to `Monitor.Enter`/`Monitor.Exit`, which are synchronous blocking operations — the thread cannot yield to the async scheduler while holding a `lock`, and the C# compiler actually prevents `await` inside a `lock` block. The async-compatible alternative is `SemaphoreSlim` with an initial count of 1: `await _gate.WaitAsync(ct)` acquires the semaphore asynchronously (yielding the thread if not immediately available), and `_gate.Release()` in a `finally` block releases it. Since `SemaphoreSlim.WaitAsync` is a true async wait, it does not block a thread while waiting, making it safe inside async code and compatible with `ConfigureAwait(false)`. The trade-off is that `SemaphoreSlim` is slightly slower than an uncontended `lock` due to task allocation, so I use it only where `async` code actually needs mutual exclusion, not as a drop-in replacement for `lock` everywhere.

---

## Q14. What is a priority inversion problem (conceptual), and which primitives exacerbate it?


**Concepts**
- priority inversion low holds lock high needs
- high-priority thread starves
- Mutex OS priority inheritance
- SemaphoreSlim no priority inheritance
- design to avoid priority coupling

**Answer:** Priority inversion occurs when a low-priority thread holds a lock that a high-priority thread needs — the high-priority thread cannot run because it is waiting for the lock, but the low-priority thread keeps getting preempted by medium-priority threads, so the lock is never released. The high-priority thread effectively runs at the priority of the low-priority lock holder, which is the "inversion." Some RTOS and OS-level `Mutex` implementations have priority inheritance (the holder temporarily receives the higher priority of its waiter) to mitigate this, but .NET's `SemaphoreSlim`, `Monitor` (used by `lock`), `ManualResetEvent`, and `AutoResetEvent` have no such mechanism. In practice, the fix is design-level: keep critical sections short, avoid long-running work inside locks, and avoid priority settings on threads that interact through shared locks.

---

## Q15. How do you diagnose deadlocks and lock contention in production (dump analysis, `dotnet-sync`, counters)?


**Concepts**
- dotnet-dump and SOS analysis
- dotnet-stack thread state inspection
- PerfView thread analysis
- dotnet-counters lock contention metrics
- blocking chain examination

**Answer:** For a live system I start with `dotnet-dump collect` to capture a process snapshot, then analyze it with `dotnet-dump analyze` using SOS commands like `!threads`, `!syncblk` (shows monitor owners and waiters), and `!dumpstack` per thread to identify blocked chains. `dotnet-stack` (in .NET 7+) prints all managed thread stacks without a full dump. For contention metrics without stopping the process, `dotnet-counters monitor --counters System.Runtime` surfaces `monitor-lock-contention-count` in real time; rising contention count without a rising lock-hold time usually means many threads competing for a hot lock that can be sharded or redesigned. PerfView's CPU and thread-time views show where threads spend time waiting on lock acquisitions over a profiling window, which is useful for identifying the specific method and lock object. In async codebases I also check for `SemaphoreSlim` CurrentCount dropping to zero under load and never recovering, which indicates a missing `Release` in a fault path.

---

## Q16. What is thread-safe lazy initialization (`Lazy<T>`, double-checked locking pitfalls)?


**Concepts**
- Lazy<T> LazyThreadSafetyMode.ExecutionAndPublication
- double-checked locking volatile pitfall
- Lazy<T> correct memory barriers
- volatile for double-check field
- thread-safe singleton initialization

**Answer:** `Lazy<T>` with `LazyThreadSafetyMode.ExecutionAndPublication` (the default parameterless constructor) is thread-safe: the first thread to access `.Value` acquires an internal lock, runs the factory, and stores the result; subsequent threads receive the cached value without running the factory again. This is the correct singleton initialization pattern in most cases. The classic double-checked locking anti-pattern in C# (check-lock-check-assign without `volatile`) is broken because without a memory barrier, the JIT or CPU can reorder the write to the reference and the write of the object's fields, so another thread can see a non-null but not-yet-fully-constructed instance. Making the backing field `volatile` fixes the visibility issue, but `Lazy<T>` handles all of this correctly internally and is the idiomatic choice. I use `Lazy<T>` for any expensive singleton that should be initialized on first access, and I avoid manual double-checked locking unless I need `LazyThreadSafetyMode.PublicationOnly` semantics (where multiple factories can run but only one result wins).

---

### 07. Concurrent Collections

## Q1. What concurrent collections exist in .NET (`ConcurrentDictionary`, `ConcurrentQueue`, `ConcurrentBag`, `BlockingCollection`, etc.)?

(R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:


**Concepts**
- ConcurrentDictionary parallel stock drift
- AddOrUpdate non-atomic check-then-act
- GetOrAdd factory race
- Interlocked for numeric delta
- atomic update with TryUpdate

**Answer:** `ApplyPick` performs read-modify-write with separate `TryGetValue` and indexer assignment — not atomic on `ConcurrentDictionary`. Two threads can read the same `current`, both subtract, and one update is lost. `ConcurrentDictionary` makes single operations thread-safe, not compound sequences.


**Fix (priority order):**

1. Replace the sequence with `AddOrUpdate` — atomic insert or transform in one call (see **Program.cs** Section 2).
2. Or use `TryUpdate` in a retry loop with compare-and-swap semantics until success.
3. Enforce business rule (non-negative stock) inside the update delegate or after atomic update with validation/retry.
4. Reserve `lock` only when multiple collections or fields must change together (ch.06).

```csharp
public void ApplyPick(string sku, int quantity) =>
    _onHand.AddOrUpdate(
        sku,
        _ => -quantity,
        (_, current) => current - quantity);
```


---

## Q2. When should you use thread-safe collections instead of standard collections plus locks?

(R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:


**Concepts**
- GetOrAdd factory race condition
- Lazy<T> wrapped in GetOrAdd
- double initialization risk
- TryAdd then TryGetValue pattern
- cache stampede for missing key

**Answer:** Under contention, `GetOrAdd` may invoke the factory delegate multiple times for the same key — only one result is stored, but every invocation runs. Side effects (`LoadProduct`, metric increment) are not deduplicated.


**Fix (priority order):**

1. Move side effects out of the factory — factory returns value only; increment metrics after `GetOrAdd` returns if this thread's value was the one stored (hard to detect) **or** use explicit double-check with `SemaphoreSlim` per key / `Lazy<Task<Product>>` per key.
2. Preferred pattern: `GetOrAdd(key, _ => new Lazy<Task<Product>>(() => LoadAsync(key)))` then `await lazy.Value` — one factory constructs the `Lazy`, one `LoadProduct` per key.
3. Or use `IMemoryCache.GetOrCreateAsync` with built-in stampede protection in ASP.NET Core.
4. Document that factory must be idempotent and cheap if you keep raw `GetOrAdd`.

```csharp
var lazy = _cache.GetOrAdd(sku, k => new Lazy<Product>(() => _repo.LoadProduct(k)));
var product = lazy.Value;
```


---

## Q3. What is the difference between `Dictionary<TKey, TValue>` and `ConcurrentDictionary<TKey, TValue>`?

(R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:


**Concepts**
- BlockingCollection CompleteAdding never called
- GetConsumingEnumerable hangs
- producer exits without completing
- CompleteAdding in finally
- bounded buffer deadlock prevention

**Answer:** The producer never calls `CompleteAdding()`, so `GetConsumingEnumerable` waits forever for more items even after `FetchPendingOrders` finishes. The consumer never exits; `Task.WhenAll` blocks indefinitely.


**Fix (priority order):**

1. Call `buffer.CompleteAdding()` in a `finally` after the producer loop — matches **Program.cs** Section 6 (`PickBufferDemo`).
2. Use `try/finally` on producer so completion fires even on exception; log and rethrow or surface failure.
3. Pass `CancellationToken` to both `GetConsumingEnumerable(ct)` and producer; on cancel, complete adding if not already done.
4. Await both tasks; consider `Task.WhenAll` with timeout for ops visibility.

```csharp
try {
    foreach (var order in _repo.FetchPendingOrders())
        buffer.Add(order, ct);
} finally {
    buffer.CompleteAdding();
}
```


---

## Q4. What are `AddOrUpdate`, `GetOrAdd`, and `TryUpdate` on `ConcurrentDictionary`?

(R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:


**Concepts**
- ConcurrentBag ordering not FIFO
- ConcurrentQueue for FIFO order
- thread-local steal semantics
- wrong collection for ordered dispatch
- FIFO vs LIFO vs unordered choice

**Answer:** `ConcurrentBag` provides no global FIFO ordering — it uses thread-local lists and `TryTake` prefers items from the calling thread's partition. Ticket order becomes undefined; SLA and fairness break even though `TryTake` "works."


**Fix (priority order):**

1. Replace with `ConcurrentQueue<SupportTicket>` — `Enqueue` / `TryDequeue` preserves FIFO (Section 3).
2. If multiple consumers need blocking when empty, wrap in `BlockingCollection<SupportTicket>` with bounded capacity for back-pressure.
3. Keep `ConcurrentBag` only when order is irrelevant (error aggregation from `Parallel.ForEach`).
4. For priority tiers, use separate queues or a priority queue with appropriate synchronization — not a bag.


---

## Q5. What is `BlockingCollection<T>`, and how does it implement producer-consumer patterns?

(P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?


**Concepts**
- bounded BlockingCollection back-pressure
- BoundedCapacity OOM prevention
- Channel<T> as modern alternative
- producer blocks when full
- consumer drain keeps memory bounded

**Answer:** Replace the unbounded queue with `BlockingCollection<LogEntry>` backed by `ConcurrentQueue`, set `boundedCapacity` to match writer throughput and memory budget (e.g. 5,000–20,000 entries), and have producers use `TryAdd` with timeout or `Add` with cancellation when full. Writers drain via `GetConsumingEnumerable`; producers call `CompleteAdding()` on shutdown.

- **Bounded buffer:** `new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), boundedCapacity: 10_000)` — producers block or fail when full instead of allocating without limit (Section 6).
- **Writers:** fixed pool of 4 tasks consuming `GetConsumingEnumerable(ct)` — batch flush to disk/network.
- **Producers:** on burst, `TryAdd` returns false → drop with metric, sample, or spill to disk — explicit policy beats OOM.
- **Shutdown:** `CompleteAdding()` when ingress stops; writers drain and exit.

**What breaks without back-pressure:**

- Unbounded `ConcurrentQueue` grows until gen2 LOH pressure and OOM kill the process.
- Silent latency growth — queue depth rises, log delivery lags minutes behind real time.
- GC pauses spike under sustained producer > consumer mismatch.


---

## Q6. What is the difference between bounded and unbounded `BlockingCollection` behavior?

(D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:


**Concepts**
- ConcurrentBag for parallel collection
- lock plus List pitfall
- ConcurrentQueue for ordered errors
- per-thread bag merge after loop
- concurrent collection choice for parallel aggregation

**Answer:** Use **Option B (`ConcurrentBag`)** for parallel error collection when order does not matter — avoids serializing every `Add` on a global lock. Use **Option A (lock + List)** when you need deterministic ordering, deduplication, or a single sorted merge with other state under one invariant.


**Before returning to API client:**

1. Copy bag to array (`errors.ToArray()` or `ToList()`) — do not enumerate live bag while workers still add unless complete.
2. Sort or dedupe if client expects stable ordering — `OrderBy` on row number if error carries line index.
3. Cap response size — if errors exceed limit, return summary + truncated list with total count.
4. Do not return the mutable bag directly — snapshot first (Section 5 pattern).


---

## Q7. How do you use `BlockingCollection` with multiple producers and consumers?

(M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:


**Concepts**
- ConcurrentDictionary LINQ snapshot inconsistency
- ToArray before LINQ query
- Count vs enumerated items race
- weakly consistent enumeration
- snapshot for consistent reporting

**Answer:** Enumeration over `ConcurrentDictionary` while mutators run yields a weakly consistent snapshot — you may miss concurrent adds, see duplicate keys is impossible, but values can change mid-enumeration. `Count` during heavy mutation is approximate and can disagree with the number of entries you enumerate. Sorting during live enumeration produces a report that was never true at any single instant.

- **Snapshot semantics:** Foreach is safe (no `InvalidOperationException`) but not a point-in-time photograph — documented weak consistency.
- **Misleading `Count`:** Can differ from `rows.Count` after loop — do not use for reconciliation dashboards without copying first.
- **Sort mid-mutation:** Order reflects values observed at different times — ops may see phantom shortages.

**When to copy:**

- Financial or ops reconciliation → `ToArray()` or `Select(...).ToList()` under a defined policy, or pause writers briefly.
- Live dashboard OK with "approximate live" → document lag; refresh on interval; prefer `OrderBy` on copied snapshot.
- High-stakes inventory → version counter (`Interlocked`) incremented on each batch publish; report includes version stamp.

```csharp
var snapshot = _onHand.ToArray();
var rows = snapshot
    .Select(kv => new StockRow(kv.Key, kv.Value))
    .OrderBy(r => r.Sku, StringComparer.Ordinal)
    .ToList();
```


---

## Q8. What is `ConcurrentQueue` vs `ConcurrentStack` vs `ConcurrentBag` — ordering and stealing semantics?


**Concepts**
- ConcurrentQueue FIFO MPMC-safe
- ConcurrentStack LIFO TryPop
- ConcurrentBag unordered work-stealing
- thread-local lists in ConcurrentBag
- cross-thread steal cost

**Answer:** `ConcurrentQueue<T>` is FIFO and multiple-producer, multiple-consumer safe — `Enqueue` adds to the tail and `TryDequeue` removes from the head. `ConcurrentStack<T>` is LIFO — `Push` adds to the top and `TryPop` removes from the top; it is useful for depth-first work-stealing algorithms. `ConcurrentBag<T>` is unordered and uses a thread-local list per thread: a thread that adds items owns them locally and can dequeue them without contention; when a thread's local list is empty it steals from another thread's list. `ConcurrentBag` performs best when the same thread both produces and consumes its items — like worker threads that generate and process their own sub-tasks. When producers and consumers are different threads, work-stealing from foreign thread-local lists adds synchronization overhead that can make `ConcurrentBag` slower than `ConcurrentQueue` for the same workload.

---

## Q9. When is `ConcurrentBag` the wrong choice despite being thread-safe?


**Concepts**
- ConcurrentBag wrong for FIFO/LIFO order
- cross-thread steal expensive
- non-deterministic ordering
- better for same-thread produce-consume
- use ConcurrentQueue or ConcurrentStack instead

**Answer:** `ConcurrentBag<T>` is the wrong choice when order matters (it provides no ordering guarantees), when producers and consumers are different threads consistently (cross-thread stealing is expensive and negates its performance advantage), or when deterministic ordering is required for correctness. Its performance profile is optimized for the "each thread consumes what it produces" pattern, not general producer-consumer queues. I use `ConcurrentQueue<T>` when FIFO order matters or when there are dedicated producer and consumer threads, `ConcurrentStack<T>` when LIFO is semantically correct, and `ConcurrentBag<T>` only in work-stealing scenarios like parallel tree traversal where each worker generates sub-tasks it will process itself.

---

## Q10. What is `IProducerConsumerCollection<T>` and custom underlying stores for `BlockingCollection`?


**Concepts**
- IProducerConsumerCollection<T> interface
- BlockingCollection wraps any IProducerConsumerCollection
- pass ConcurrentStack for LIFO
- pass ConcurrentBag for unordered
- custom underlying store

**Answer:** `IProducerConsumerCollection<T>` is the interface that all .NET concurrent collections implement — it defines `TryAdd`, `TryTake`, `CopyTo`, and `ToArray`. `BlockingCollection<T>` wraps any `IProducerConsumerCollection<T>` to add blocking semantics: its constructor accepts the underlying store, defaulting to `ConcurrentQueue<T>`. By passing `new ConcurrentStack<T>()` I get a `BlockingCollection` with LIFO semantics, and by passing `new ConcurrentBag<T>()` I get unordered semantics with blocking behavior. This lets me swap the ordering behavior of `BlockingCollection`-based producer-consumer pipelines without changing the producer or consumer code, since both still interact with `BlockingCollection`'s `Add`/`Take` or `GetConsumingEnumerable` API.

---

## Q11. How do concurrent collections compare to locking a `List<T>` for high-contention scenarios?


**Concepts**
- fine-grained stripe locking in ConcurrentDictionary
- lock-free algorithms for ConcurrentQueue
- single lock serializes all List<T> access
- high-contention benefit of concurrent collections
- concurrent collections reduce lock contention

**Answer:** A `lock + List<T>` uses a single lock for every operation, so all producers and consumers serialize through one contention point regardless of which list slot they would use — under high concurrency, throughput is limited by how fast threads can acquire the single lock. `ConcurrentDictionary<K,V>` uses stripe-based locking (conceptually N independent locks over N buckets) so operations on different keys proceed in parallel; most concurrent add/update operations never contend at all. `ConcurrentQueue<T>` uses a lock-free linked-list algorithm where enqueue and dequeue operate on opposite ends simultaneously. The practical result is that for high-throughput scenarios with many concurrent writers, concurrent collections scale much better than a single-lock approach. For low concurrency (a handful of threads) the difference is negligible and `lock + List<T>` is simpler.

---

## Q12. What enumeration semantics do concurrent collections provide (weakly consistent iterators)?


**Concepts**
- weakly consistent iterator snapshot
- no InvalidOperationException on concurrent change
- elements may appear or be missed
- concurrent add/remove during enumeration
- documented weak consistency semantics

**Answer:** Concurrent collections in .NET provide "weakly consistent" iterators — the enumerator takes a logical snapshot based on the state at each step rather than a single atomic snapshot of the whole collection. This means that elements added after enumeration begins may or may not appear in the iteration, and elements removed during iteration may or may not be included, but the enumerator never throws `InvalidOperationException` for concurrent modifications as non-concurrent collections do. The practical implication is that LINQ queries over a `ConcurrentDictionary` or `ConcurrentQueue` can observe the collection in a partially-updated state. For a consistent view I call `.ToArray()` or `.ToList()` on the collection before the query — this materializes a snapshot at a point in time that LINQ then processes without further concurrent modifications affecting the result.

---

## Q13. How do you gracefully complete adding to a `BlockingCollection` (`CompleteAdding`)?


**Concepts**
- CompleteAdding signals no more items
- GetConsumingEnumerable exits on CompleteAdding
- call CompleteAdding in finally block
- multiple producers coordinate on single collection
- IsCompleted after CompleteAdding

**Answer:** `CompleteAdding()` signals to `BlockingCollection<T>` that no more items will be added — it sets the `IsAddingCompleted` flag. Any consumer iterating with `GetConsumingEnumerable` will drain all remaining items and then exit the `foreach` loop naturally when the collection is both empty and marked completed. The critical pattern is to call `CompleteAdding()` inside a `finally` block so it always executes even if a producer throws an exception — without it, consumers block forever on `Take` or `GetConsumingEnumerable` waiting for items that will never arrive. With multiple producers, only the last active producer should call `CompleteAdding`, typically coordinated through a `CountdownEvent` or by having an outer orchestrator call it after all producers' tasks complete.

---

## Q14. What pitfalls arise when mixing concurrent collections with LINQ?


**Concepts**
- LINQ snapshot inconsistency on ConcurrentDictionary
- ToArray before LINQ
- Count disagrees with enumeration
- LINQ not atomic on concurrent data
- materialize before querying

**Answer:** LINQ queries like `.Where`, `.OrderBy`, or `.Select` on a `ConcurrentDictionary` enumerate the dictionary while other threads may concurrently add or remove entries. The enumeration is weakly consistent, meaning LINQ may see a mix of old and new state depending on timing — a `.Count` check followed by a LINQ query can report a count inconsistent with the elements actually enumerated. The fix is to materialize a snapshot first: `var snapshot = dict.ToArray()` or `dict.ToList()` copies the key-value pairs at one moment, and the subsequent LINQ chain runs against the immutable snapshot rather than the live collection. This adds a small allocation and copying cost but ensures that the LINQ result is consistent with what was in the collection at the moment of the snapshot.

---

## Q15. When should you use channels (`System.Threading.Channels`) instead of `BlockingCollection` in modern code?


**Concepts**
- Channel<T> native async ReadAsync and WriteAsync
- natural back-pressure with BoundedChannel
- better than BlockingCollection for async pipelines
- ChannelOptions.Capacity for bounding
- System.Threading.Channels modern choice

**Answer:** `System.Threading.Channels` (available since .NET Core 3.0) provides `Channel<T>` with `WriteAsync`/`TryWrite` on the writer side and `ReadAsync`/`ReadAllAsync` on the reader side — all natively async, so producers and consumers do not block threads while waiting. `BoundedChannel<T>` applies back-pressure automatically: `WriteAsync` waits (without blocking a thread) when the channel is full, preventing unbounded memory growth. `BlockingCollection<T>` predates async and uses blocking `Add`/`Take` that hold OS threads during waits, making it poorly suited for async pipelines. In modern code I use `Channel<T>` as the default for async producer-consumer pipelines, reserving `BlockingCollection<T>` only for legacy sync contexts or interop with non-async consumers.

---

## Q16. **`.Result` / `.Wait()` deadlock** — Blocking async on a captured synchronization context (UI, legacy ASP.NET) deadlocks when the continuation needs that same context.


**Concepts**
- .Result/.Wait() on captured SynchronizationContext
- UI or legacy ASP.NET context deadlock
- continuation waits for blocked thread
- ConfigureAwait(false) library fix
- never block async on context-holding threads

**Answer:** The classic deadlock: a UI method calls `.Result` on an async task, blocking the UI thread. The async method's continuation was scheduled to post back to the UI's `SynchronizationContext`, but the UI thread is blocked waiting for `.Result`. Neither can proceed. The fix is to `await` throughout, or to use `ConfigureAwait(false)` in the library so the continuation does not need the UI context.

---

## Q17. **`async void` swallows observability** — Exceptions cannot be awaited by callers; use only for event handlers.


**Concepts**
- async void exception not observable by caller
- crashes process via AppDomain.UnhandledException
- use only for event handlers
- async Task for all other cases
- async void swallows task faults

**Answer:** `async void` returns no observable `Task`. Any exception thrown inside an `async void` method bypasses normal `try/catch` at the call site and is raised directly on the thread pool via `AppDomain.UnhandledException`, which by default terminates the process. The valid use case is event handlers where the event delegate signature is `void`. For all other cases — including fire-and-forget background work — use `async Task` and either await it or attach a fault-handling continuation.

---

## Q18. **`Task.Run` for I/O** — Offloading blocking I/O to the pool wastes threads; prefer truly async APIs.


**Concepts**
- Task.Run for I/O offloads to pool thread
- truly async I/O releases thread entirely
- pool thread blocked on I/O wastes resource
- prefer Stream.ReadAsync, HttpClient.GetAsync
- Task.Run correct only for CPU-bound work

**Answer:** `Task.Run(() => _file.ReadAllBytes(path))` queues a pool thread that blocks synchronously on I/O. The thread is occupied the entire time the disk read runs. A truly async API like `File.ReadAllBytesAsync` uses I/O completion ports: the thread is released during the wait and reclaimed by another request; only a callback fires when the OS signals completion. For CPU-bound work (compression, encryption) `Task.Run` is correct. For I/O-bound work it wastes pool threads and does not improve throughput.

---

## Q19. **Async does not mean threaded** — I/O `await` often completes without extra threads; continuations may run on any pool thread.


**Concepts**
- async continuation may run on any pool thread
- I/O await does not always use extra thread
- IOCP completion ports
- not every await spawns a thread
- async is concurrency not parallelism

**Answer:** When an async method `await`s an incomplete task, execution returns to the caller — there is no second thread involved. The continuation is queued after the awaited task completes, and it may run on any pool thread (or the original context thread if `SynchronizationContext` is active). Pure I/O awaits like database queries and HTTP calls often complete without ever spawning an additional thread beyond the I/O completion callback. Async provides concurrency (multiple operations in flight simultaneously) but not necessarily parallelism (multiple threads executing simultaneously).

---

## Q20. **Unobserved task exceptions** — Faulted tasks that are never awaited may surface later as unobserved exception events.


**Concepts**
- GC finalizes faulted unobserved task
- TaskScheduler.UnobservedTaskException fires
- no crash in .NET 4.5+ by default
- always await or ContinueWith(OnlyOnFaulted)
- unobserved exception event for logging

**Answer:** When a `Task` faults and no code ever observes its exception, the GC eventually finalizes the dead `Task` object and the runtime raises `TaskScheduler.UnobservedTaskException`. In .NET 4.5+ this no longer crashes the process by default, but the exception is still silently lost unless someone subscribes to that event for logging. The fix is to always observe tasks — either `await` them, attach `ContinueWith(OnlyOnFaulted)` for logging, or store them and check `.Exception`. Every fire-and-forget task should have a fault handler.

---

## Q21. **Race on `List<T>`/`Dictionary<,>`** — Even `Add` is not thread-safe; use locks or concurrent collections.


**Concepts**
- List<T>.Add not thread-safe
- internal array resize race
- ConcurrentBag or lock required
- Dictionary<,> concurrent read/write corrupts state
- concurrent collection or lock+collection pattern

**Answer:** `List<T>` and `Dictionary<K,V>` are not thread-safe for concurrent writes. `List<T>.Add` may resize its internal array; if two threads both trigger a resize, one will corrupt the other's copy. `Dictionary<K,V>` can enter an infinite loop under concurrent writes due to hash-table bucket pointer corruption. Both can cause arbitrary exceptions or silent data loss. The fix is either `lock` around every access or, preferably, `ConcurrentBag<T>` / `ConcurrentDictionary<K,V>` which are designed for concurrent use.

---

## Q22. **`ConfigureAwait(false)` in libraries** — Library code should not marshal back to UI context; app code often needs the default for UI updates.


**Concepts**
- ConfigureAwait(false) skips UI context marshal
- library code avoids deadlock with false
- application code needs true for UI updates
- default behavior captures context
- NuGet library best practice

**Answer:** Library code should always use `ConfigureAwait(false)` because it does not know what `SynchronizationContext` the caller has. Without `false`, a library continuation captures the caller's UI or ASP.NET context and posts back to it — if that context is blocked waiting for the library (deadlock) or simply does not exist in a different host, failures follow. Application/UI code that updates UI elements after an `await` needs the default `true` behavior so the continuation returns to the UI thread. The practical rule: all NuGet library and infrastructure code uses `ConfigureAwait(false)` everywhere; application presentation code omits it.

---

## Q23. **`ValueTask` double-await** — Re-awaiting or concurrent awaits on a pooled `ValueTask` can corrupt state unless documented safe.


**Concepts**
- ValueTask pooled IValueTaskSource reuse
- double-await corrupts pool slot
- concurrent await undefined behavior
- consume ValueTask exactly once
- AsTask() for multi-await scenarios

**Answer:** `ValueTask` may wrap a pooled `IValueTaskSource<T>` that gets recycled after being consumed. After the first `await` the source is returned to its pool. A second `await` reads recycled state that may belong to a completely different logical operation, resulting in `InvalidOperationException` or silent wrong data. If I need to await the same result multiple times, I call `.AsTask()` immediately, which copies the value into a stable `Task<T>` object that is safe to await repeatedly. I only use `ValueTask` when the hot path completes synchronously most of the time; otherwise `Task<T>` is simpler and safer.

---

## Q24. **`TaskCompletionSource` set twice** — Second `TrySet*` calls fail; race to complete can drop results if not coordinated.


**Concepts**
- TaskCompletionSource set-twice throws InvalidOperationException
- TrySetResult returns false on second call
- race to complete may drop result
- coordinate single-writer or use TrySet
- idempotent TCS completion guard

**Answer:** `TaskCompletionSource<T>.SetResult` throws `InvalidOperationException` if the task is already in a terminal state. This means if two callbacks both call `SetResult`, the second one crashes. `TrySetResult` returns `false` on a repeated call instead of throwing, making it safe for races. The pattern is always to use `TrySet*` variants in any code where more than one caller might attempt to complete the TCS, and to coordinate which caller "owns" completion using the `false` return to detect and discard the race loser.

---

## Q25. **`BlockingCollection` after `CompleteAdding`** — Adding throws; consumers must drain remaining items correctly.


**Concepts**
- adding after CompleteAdding throws InvalidOperationException
- consumers drain remaining items
- IsAddingCompleted check
- GetConsumingEnumerable reads until empty and completed
- producer/consumer shutdown order

**Answer:** After `CompleteAdding()` is called, calling `Add` or `TryAdd` on the `BlockingCollection` throws `InvalidOperationException`. Consumers iterating with `GetConsumingEnumerable` will drain all existing items and then exit the loop naturally — the enumerator returns `false` when the collection is both empty and completed. The correct teardown sequence is: producers signal completion (call `CompleteAdding` in a `finally`), consumers run `GetConsumingEnumerable` or check `IsCompleted` + drain remaining items, then both sides complete. Never add after `CompleteAdding`.

---

## Q26. **`Interlocked` is not composable** — Check-then-act on complex invariants still needs `lock` or careful CAS loops.


**Concepts**
- Interlocked single-operation atomic
- check-then-act still races
- CAS loop for complex invariants
- lock for multi-step compound operations
- Interlocked.CompareExchange for CAS

**Answer:** `Interlocked` operations are atomic on a single field at a time. A "check, then act" pattern across two fields — for example, checking a count before inserting into a list — is not atomic even if each step uses `Interlocked`. Between the check and the act, another thread can change the count, invalidating the check. For any invariant that spans multiple fields or requires a multi-step sequence, a `lock` is necessary. `Interlocked.CompareExchange` can implement CAS loops for single-field optimistic updates, but these are complex and easy to get wrong.

---

## Q27. **`volatile` does not make operations atomic** — `i++` still races even if `i` is volatile.


**Concepts**
- volatile ensures visibility not atomicity
- i++ is read-modify-write three steps
- volatile i++ still races
- Interlocked.Increment for atomic increment
- volatile does not prevent lost update

**Answer:** `volatile` inserts memory barriers to ensure read/write visibility across cores, but it does not make compound operations atomic. `i++` is still a separate load, increment, and store — two threads can both load the same value, both increment it, and both store the same incremented value, resulting in a lost update, even if `i` is `volatile`. For atomic increment, `Interlocked.Increment(ref i)` is the correct fix; for larger operations, `lock` is required.

---

## Q28. **Parallel loop over small work** — Partitioning overhead can make `Parallel.ForEach` slower than sequential code.


**Concepts**
- Parallel.ForEach partition overhead
- small work items sequential is faster
- threshold for parallelism benefit
- overhead exceeds speedup for trivial work
- benchmark before parallelizing

**Answer:** `Parallel.ForEach` has overhead: partitioning the input, coordinating worker threads, and merging results all take time regardless of the work per item. For very small work items (sub-millisecond body) this coordination overhead can exceed the savings from parallelism, making the parallel version slower than a sequential `foreach`. The threshold depends on the machine, but a common heuristic is that parallelism benefits CPU-bound work only when each item takes at least several milliseconds. I always benchmark with realistic data sizes before parallelizing, and when items are cheap I consider batching them into larger chunks before parallelizing.

---

## Q29. **Shared `Random` is not thread-safe** — Use `Random.Shared` or thread-local RNG in parallel code.


**Concepts**
- Random not thread-safe internal state race
- Random.Shared thread-safe in .NET 6+
- ThreadLocal<Random> per-thread RNG
- [ThreadStatic] Random initialization
- concurrent Random corrupts internal state

**Answer:** `System.Random` maintains internal state (a seed array and indices) that is not thread-safe. Concurrent calls to `Next()` from multiple threads cause data races on that internal state, producing incorrect values or throwing exceptions. In .NET 6+ `Random.Shared` is a thread-safe static instance backed by a per-thread cache. Alternatively, `ThreadLocal<Random>` (seeded with a unique value per thread to avoid all threads starting from the same seed) ensures each thread has its own independent `Random` without any contention.

---

## Q30. **Retry without cancellation** — Exponential backoff loops must honor `CancellationToken` and max attempts to avoid runaway delays.


**Concepts**
- retry loop must check CancellationToken
- max attempt guard prevents infinite loop
- Task.Delay with CancellationToken
- exponential backoff with jitter
- OperationCanceledException on cancel

**Answer:** A retry loop without a `CancellationToken` will keep retrying indefinitely if the operation keeps failing, even after a user cancels or the service shuts down. The loop also needs a maximum attempt count to avoid running forever when a transient error is actually permanent. The correct pattern threads `ct` into both the operation call and `Task.Delay`, and has a finite retry count with the last attempt rethrowing the exception rather than catching it. `Task.Delay(delay, ct)` ensures that a cancellation during a backoff pause immediately exits rather than waiting the full delay interval.

---

## Scenario-Based Questions (Karat Format)

## Q1. (R) A warehouse console tool spawns label printers on dedicated threads. Operators report the process "hangs" after pressing Enter to quit, even though cancellation was requested. Review the shutdown wiring:

```csharp
public static void Main()
{
    using var cts = new CancellationTokenSource();
    var labelThread = new Thread(() => PrintLabelsLoop(cts.Token));
    labelThread.Name = "LabelPrinter";
    labelThread.Start();

    Console.WriteLine("Press Enter to stop…");
    Console.ReadLine();
    cts.Cancel();
    // expects immediate exit after Enter
}

static void PrintLabelsLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        Thread.Sleep(500);
        Console.WriteLine("[LabelPrinter] stamp…");
    }
}
```

What keeps the process alive, and how do you fix shutdown so cancellation is honored cleanly?

**Concepts**
- foreground vs background thread
- CancellationToken cooperative flag
- Thread.Join for graceful shutdown
- process lifetime and foreground threads
- dedicated thread use cases

**Answer:**

```csharp
public static void Main()
{
    using var cts = new CancellationTokenSource();
    var labelThread = new Thread(() => PrintLabelsLoop(cts.Token));
    labelThread.Name = "LabelPrinter";
    labelThread.Start();

    Console.WriteLine("Press Enter to stop…");
    Console.ReadLine();
    cts.Cancel();
    // expects immediate exit after Enter
}

static void PrintLabelsLoop(CancellationToken token)
{
    while (!token.IsCancellationRequested)
    {
        Thread.Sleep(500);
        Console.WriteLine("[LabelPrinter] stamp…");
    }
}
```

What keeps the process alive, and how do you fix shutdown so cancellation is honored cleanly?

**Answer:** `CancellationToken` only sets a flag — it does not terminate the thread. The label thread defaults to **foreground** (`IsBackground == false`), so the CLR keeps the process alive until that thread's delegate finishes. Main exits after `Cancel()` without `Join`, but the foreground worker may still be inside `Thread.Sleep(500)` before it observes cancellation.


**Fix (priority order):**

1. After `cts.Cancel()`, call `labelThread.Join()` (optionally `Join(TimeSpan.FromSeconds(30))` and log if timeout).
2. Keep cooperative cancellation in the loop — check `token.IsCancellationRequested` and exit cleanly (matches **Program.cs** Section 8 — `RunInventorySweep`).
3. For daemon-style helpers that must not block process exit, set `labelThread.IsBackground = true` **only** when you accept abrupt termination without guaranteed cleanup — still prefer `Join` for graceful shutdown.
4. Shorten blocking intervals or use `token.WaitHandle.WaitOne(100)` so cancellation is observed faster between iterations.

```csharp
cts.Cancel();
if (!labelThread.Join(TimeSpan.FromSeconds(30)))
    Console.Error.WriteLine("LabelPrinter did not stop in time.");
```


---
## Q2. (R) A teammate copied the shipment worker from the chapter tutorial but dropped synchronization "for speed." Under load, totals and result lists disagree. Review:

```csharp
private static int _packagesProcessed;
private static readonly List<ShipmentResult> _completed = new();

public static void ProcessShipment(object? state)
{
    var work = (ShipmentWork)state!;
    int boxesDone = 0;

    for (int box = 1; box <= work.BoxCount; box++)
    {
        Thread.Sleep(work.MillisecondsPerBox);
        _packagesProcessed++;          // no lock
        boxesDone++;
    }

    _completed.Add(new ShipmentResult(
        work.ShipmentId, work.Destination, boxesDone,
        0, Thread.CurrentThread.ManagedThreadId, ""));

    Console.WriteLine($"[{Thread.CurrentThread.Name}] done {work.ShipmentId}");
}

// Main starts three ParameterizedThreadStart workers concurrently on shared static fields.
```

What fails in production, and what is the prioritized fix?

**Concepts**
- shared mutable state
- lost-update race
- List<T> non-thread-safe
- Interlocked.Increment
- lock synchronization

**Answer:**

```csharp
private static int _packagesProcessed;
private static readonly List<ShipmentResult> _completed = new();

public static void ProcessShipment(object? state)
{
    var work = (ShipmentWork)state!;
    int boxesDone = 0;

    for (int box = 1; box <= work.BoxCount; box++)
    {
        Thread.Sleep(work.MillisecondsPerBox);
        _packagesProcessed++;          // no lock
        boxesDone++;
    }

    _completed.Add(new ShipmentResult(
        work.ShipmentId, work.Destination, boxesDone,
        0, Thread.CurrentThread.ManagedThreadId, ""));

    Console.WriteLine($"[{Thread.CurrentThread.Name}] done {work.ShipmentId}");
}

// Main starts three ParameterizedThreadStart workers concurrently on shared static fields.
```

What fails in production, and what is the prioritized fix?

**Answer:** Multiple workers perform unsynchronized read-modify-write on `_packagesProcessed` and concurrent `List<T>.Add` calls. The tally loses increments (classic lost update), and the list can corrupt internal state or throw — intermittent failures that pass single-threaded demos.


**Fix (priority order):**

1. Protect shared mutations with one lock object (same pattern as **Program.cs** `TallyLock` around increment and `CompletedShipments.Add`).
2. Prefer `Interlocked.Increment(ref _packagesProcessed)` for the counter if it is the only numeric shared state — still lock the list or use a thread-safe collection.
3. For production aggregation, consider `ConcurrentBag<ShipmentResult>` or per-worker local results merged after `Join` — eliminates lock contention on hot paths.
4. Re-run under parallel workers with stress timing (reduce `Sleep`) to reproduce before/after fix.

```csharp
lock (TallyLock)
{
    _packagesProcessed++;
}
// …
lock (TallyLock)
{
    _completed.Add(result);
}
```


---
## Q3. (R) After parallelizing shipment processing, every worker log shows the same shipment id (`SH-1003`) even though three different ids were queued. Review the spawn loop:

```csharp
ShipmentWork[] pending =
[
    new("SH-1001", "North", 3, 50),
    new("SH-1002", "South", 2, 50),
    new("SH-1003", "East", 4, 50),
];

var threads = new Thread[pending.Length];
for (int i = 0; i < pending.Length; i++)
{
    threads[i] = new Thread(() => ProcessShipment(pending[i]));
    threads[i].Name = $"Worker-{pending[i].ShipmentId}";
    threads[i].Start();
}

foreach (var t in threads) t.Join();
```

Why does every thread process the last shipment, and how do you fix it without changing the worker signature?

**Concepts**
- closure capture over loop variable
- lambda variable capture bug
- ParameterizedThreadStart
- per-iteration local capture
- thread naming

**Answer:**

```csharp
ShipmentWork[] pending =
[
    new("SH-1001", "North", 3, 50),
    new("SH-1002", "South", 2, 50),
    new("SH-1003", "East", 4, 50),
];

var threads = new Thread[pending.Length];
for (int i = 0; i < pending.Length; i++)
{
    threads[i] = new Thread(() => ProcessShipment(pending[i]));
    threads[i].Name = $"Worker-{pending[i].ShipmentId}";
    threads[i].Start();
}

foreach (var t in threads) t.Join();
```

Why does every thread process the last shipment, and how do you fix it without changing the worker signature?

**Answer:** The lambda closes over the loop variable `i`, not the value at iteration time. All threads may start after the loop finishes, so `pending[i]` resolves to the last index for every delegate — a closure capture bug unrelated to `ParameterizedThreadStart` itself.


**Fix (priority order):**

1. Capture a per-iteration local: `var work = pending[i];` then `new Thread(() => ProcessShipment(work))`.
2. Or use `ParameterizedThreadStart` directly: `new Thread(ProcessShipment)` and `worker.Start(pending[i])` — passes state at start, no closure over `i` (**Program.cs** Section 6–7 pattern).
3. Add a unit/integration test that asserts three distinct `ShipmentId` values in results after parallel start.

```csharp
for (int i = 0; i < pending.Length; i++)
{
    ShipmentWork work = pending[i];
    threads[i] = new Thread(() => ProcessShipment(work));
    threads[i].Start();
}
```


---
## Q4. (P) A long-running inventory sweep runs on a dedicated `Thread` (like `RunInventorySweep` in the chapter demo). Ops wants the Windows Service to stop within 30 seconds on shutdown — no `Thread.Abort`. What production pattern replaces force-kill, and what must the worker loop guarantee?

**Concepts**
- cooperative cancellation
- CancellationToken propagation
- Thread.Abort removal in .NET Core
- Windows Service graceful stop
- Join with timeout

**Answer:** Use **cooperative cancellation** with `CancellationToken` linked to the service's `IHostApplicationLifetime.ApplicationStopping` (or a `CancellationTokenSource` cancelled in `StopAsync`). The worker checks `IsCancellationRequested` (or `ThrowIfCancellationRequested`) in its loop, finishes the current aisle/unit of work if needed, releases locks and handles, and exits the delegate normally — then the host `Join`s the thread or awaits a `Task` wrapper.

- Never call `Thread.Abort` — removed from .NET Core because it could leave locks held and invariants broken mid-method (**Program.cs** Section 8).
- Pass the token into the worker at construction/start; cancel once from the shutdown path; block shutdown on `Join(timeout)` and log if the worker exceeds the SLA.
- Keep loop body idempotent at cancellation boundaries — persist checkpoint if stopping mid-batch matters for ops.
- For I/O-bound sweeps, prefer `async`/`await` with the same token (later chapter) so threads are not blocked in `Sleep`.


---
## Q5. (M) Main waits for workers using `IsAlive` and `Join(100)` in a loop (matching the chapter demo). Under heavy load, logs show hundreds of `"Waiting on Worker-…"` lines per second while workers are still running. Is this a bug, and what waiting pattern is preferable in production?

```csharp
foreach (Thread worker in workers)
{
    while (worker.IsAlive)
    {
        Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
        if (!worker.Join(millisecondsTimeout: 100))
            continue;
    }
    Console.WriteLine($"  {worker.Name} finished.");
}
```

**Concepts**
- Thread.Join blocking vs polling
- IsAlive busy-poll anti-pattern
- timed Join(TimeSpan)
- CountdownEvent alternative
- production join semantics

**Answer:**

```csharp
foreach (Thread worker in workers)
{
    while (worker.IsAlive)
    {
        Console.WriteLine($"  Waiting on {worker.Name} … IsAlive={worker.IsAlive}");
        if (!worker.Join(millisecondsTimeout: 100))
            continue;
    }
    Console.WriteLine($"  {worker.Name} finished.");
}
```

**Answer:** This is not a correctness bug — it is a **busy-polling** wait pattern. `Join(100)` returns `false` every 100 ms while the worker runs, so the loop spins and floods logs under load. Functionally, the thread eventually completes; operationally, you waste CPU and drown observability.

- Prefer a single blocking `worker.Join()` per worker when you simply need to wait until done (**Program.cs** Section 7).
- Use `Join(TimeSpan)` once when you need a timeout — handle `false` as SLA breach, do not spin in a tight loop unless you must interleave other work.
- If Main must pump progress UI or heartbeats while waiting, use `Join(100)` **without** logging every iteration — log on interval or on state change.
- For many workers, `Task.Run` + `Task.WhenAll` or `Parallel.Invoke` gives clearer composition than manual `IsAlive` polling (later chapters).


---
## Q6. (D) Apex Warehouse will scan 400 inbound shipments per hour. A developer proposes `new Thread(ProcessShipment)` per shipment forever, `ThreadPriority.AboveNormal` on express lanes, and `[ThreadStatic]` counters for per-worker metrics exported to Prometheus. What breaks at scale, and what would you use instead while still honoring lifecycle concepts from this chapter?

**Concepts**
- new Thread per task anti-pattern
- ThreadPriority OS hint
- ThreadStatic metric pitfall
- bounded concurrency
- thread pool preference

**Answer:** Unbounded `new Thread` per shipment exhausts OS thread limits and memory (default stack reserve per thread), thrashes the scheduler, and makes shutdown join storms impossible. `ThreadPriority` is an OS hint, not a SLA — express lanes are not reliably prioritized across machines. `[ThreadStatic]` metrics break when work moves to thread pool threads or `async` continuations hop threads — counters attach to threads, not logical shipments.

- Cap concurrency: fixed pool of long-lived worker threads **or** `ThreadPool` / `Task` with a bounded `SemaphoreSlim` (e.g., max 8 scanners) — same lifecycle idea (start work, join/complete, cooperative stop) without 1:1 OS threads.
- Express handling belongs in queue priority or business rules, not `ThreadPriority.AboveNormal`.
- Export metrics with labels (`shipment_id`, `worker_id`) via `IMeterFactory`/Prometheus counters — not `[ThreadStatic]` tallies.
- Keep `CancellationToken` on the batch host so service shutdown still cooperates (**Section 8** pattern).
- CPU-bound parallel loops → **05. Parallel Programming**; I/O-bound waits → **04. Async and Await**.


---
## Q7. (R) A retry path tries to restart workers after a transient fault. Review:

```csharp
Thread worker = new Thread(ProcessShipment);
worker.Name = "Worker-Retry";
worker.Start(shipment);

worker.Join();
if (shipment.NeedsRetry)
{
    worker.Start(shipment);   // "restart same thread"
    worker.Join();
}
```

What fails at runtime, and what is the correct lifecycle approach?

---

### 02. ThreadPool


**Answer:**

```csharp
Thread worker = new Thread(ProcessShipment);
worker.Name = "Worker-Retry";
worker.Start(shipment);

worker.Join();
if (shipment.NeedsRetry)
{
    worker.Start(shipment);   // "restart same thread"
    worker.Join();
}
```

What fails at runtime, and what is the correct lifecycle approach?

**Answer:** A `Thread` instance is **one-shot**. After the delegate completes, `ThreadState` is `Stopped` and calling `Start()` again throws `ThreadStateException` ("Thread is dead; it cannot be started"). You cannot restart the same `Thread` object.


**Fix (priority order):**

1. Create a **new** `Thread` (or new `Task`) for each retry attempt: `worker = new Thread(ProcessShipment); worker.Start(shipment);`
2. Better: one worker loop that reads from a `BlockingCollection<ShipmentWork>` or channel and retries internally — single thread lifecycle, many shipments.
3. Best at scale: queue work to `ThreadPool` / `Task` with retry policy (`Polly`) — no manual thread reuse semantics to get wrong.
4. Guard retry with max attempts and log `ManagedThreadId` per attempt for correlation.

```csharp
for (int attempt = 1; attempt <= maxAttempts && shipment.NeedsRetry; attempt++)
{
    var worker = new Thread(ProcessShipment) { Name = $"Worker-Retry-{attempt}" };
    worker.Start(shipment);
    worker.Join();
}
```


---

### 02. ThreadPool




---
## Q1. (R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?

```csharp
public sealed class InvoiceImportService
{
    public ImportSummary ProcessBatch(int jobCount)
    {
        var results = new int[jobCount];

        for (int i = 0; i < jobCount; i++)
        {
            int jobId = i;
            ThreadPool.QueueUserWorkItem(_ =>
                results[jobId] = InvoiceValidation.ValidateLine(
                    new InvoiceLineJob(jobId, workUnits: 8_000)));
        }

        int valid = results.Count(r => r == 1);
        return new ImportSummary(valid, jobCount);
    }
}
```

**Concepts**
- QueueUserWorkItem fire-and-forget
- CountdownEvent completion gate
- shared result array race
- callback exception swallowed
- async completion vs queued items

**Answer:** `QueueUserWorkItem` returns immediately — the method reads `results` and publishes a summary before pool callbacks finish, so `valid` is often zero or partial. There is no synchronization, and exceptions inside callbacks would be unobserved.


**Fix (priority order):**

1. Block until all callbacks complete — use `CountdownEvent` initialized to `jobCount`, `Signal()` in a `finally` block per callback, then `done.Wait()` before counting (matches **Program.cs** Section 4).
2. Wrap callback work in try/catch and log or aggregate failures — never let pool exceptions vanish.
3. Prefer `Task.Run` + `Task.WhenAll` (ch.03) or `Parallel.For` (ch.05) when you need structured exception propagation and cancellation.
4. Return the summary only after the wait gate completes; add a timeout/cancellation token for hung batches.

```csharp
using var done = new CountdownEvent(jobCount);
for (int i = 0; i < jobCount; i++)
{
    int jobId = i;
    ThreadPool.QueueUserWorkItem(_ =>
    {
        try { results[jobId] = InvoiceValidation.ValidateLine(new InvoiceLineJob(jobId, 8_000)); }
        catch (Exception ex) { _logger.LogError(ex, "Line {Id}", jobId); }
        finally { done.Signal(); }
    });
}
done.Wait();
```


---
## Q2. (R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?

**Concepts**
- WaitHandle.WaitAll 64-handle limit
- ManualResetEvent per-job overhead
- STA thread restriction
- CountdownEvent replacement
- signal in finally block

**Answer:**

```csharp
public static int RunBatch(int jobCount)
{
    var results = new int[jobCount];
    var doneEvents = new ManualResetEvent[jobCount];

    for (int i = 0; i < jobCount; i++)
    {
        doneEvents[i] = new ManualResetEvent(false);
        int jobId = i;
        ManualResetEvent signal = doneEvents[i];

        ThreadPool.QueueUserWorkItem(_ =>
        {
            results[jobId] = DoWork(jobId);
            signal.Set();
        });
    }

    WaitHandle.WaitAll(doneEvents);
    return results.Sum();
}
```

What breaks at runtime, and what synchronization pattern from this chapter replaces it?

**Answer:** `WaitHandle.WaitAll` on more than 64 handles throws `NotSupportedException` when the calling thread is STA — common in legacy COM/WPF hosts. Even when it succeeds, allocating 500 `ManualResetEvent` objects per batch is expensive compared to one `CountdownEvent`.


**Fix (priority order):**

1. Replace per-job events + `WaitAll` with a single `CountdownEvent(jobCount)` and `Signal()` in `finally` ( **Program.cs** Section 4).
2. Move `signal.Set()` into `finally` so failures cannot deadlock the waiter.
3. If you must use events, use `WaitOne` on one gate or `Task.WhenAll` — not `WaitAll` on hundreds of handles.
4. Dispose synchronization primitives via `using` on the countdown/event wrapper.


---
## Q3. (R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.

**Concepts**
- sync-over-async .Result in pool callback
- IOCP vs CPU pool
- thread pool starvation
- HttpClient async API
- blocking I/O blocks workers

**Answer:**

```csharp
public void EnqueueAuditFetch(string url)
{
    ThreadPool.QueueUserWorkItem(_ =>
    {
        // "Simple — just block until the HTTP call returns"
        string payload = _httpClient.GetStringAsync(url).Result;
        _repository.InsertAudit(payload);
    });
}
```

Diagnose the threading failure mode and propose a production-safe replacement.

**Answer:** `.Result` inside a thread-pool callback blocks a worker thread for the entire HTTP wait — sync-over-async on the same pool that ASP.NET, timers, and `Task.Run` share. Under load, workers pile up blocked on I/O while the queue grows, producing apparent "deadlock" or severe latency.


**Fix (priority order):**

1. Remove sync-over-async — use `async`/`await` end-to-end: `await _httpClient.GetStringAsync(url)` on an async code path (ch.04), not `.Result` on a pool thread.
2. If you must queue, queue async work via `Task.Run` only for CPU-bound segments; I/O should use async I/O that frees workers during waits (IOCP — **Program.cs** Section 9).
3. Bound concurrency with `SemaphoreSlim` or a dedicated channel/worker so audit fetches cannot exhaust the global pool.
4. Register `IHttpClientFactory` in ASP.NET hosts instead of ad-hoc blocking calls.


---
## Q4. (P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

**Concepts**
- SetMinThreads cold-start tuning
- stack reservation per thread
- fleet oversubscription
- when to tune pool minimums
- SetMaxThreads backlog risk

**Answer:**

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

**Answer:** Inflating minimum threads on every instance reserves idle workers and I/O threads that consume memory (~1 MB stack each on Windows) without guaranteed throughput gain. Forty pods × 250 workers can oversubscribe an 8-core node and increase context switching while hiding the real bottleneck (slow or blocking callbacks).

- **What breaks:** RAM pressure, scheduler thrashing, false sense of capacity; does not fix blocking code — blocked threads stay blocked regardless of min count.
- **When it helps (sparingly):** Measured cold-start after idle where pool ramp-up latency dominates *short* bursts of queued work — e.g., a known spike right after deploy. Raise min modestly, validate with `GetAvailableThreads` and latency metrics, revert if idle waste grows.
- **Prefer instead:** Fix blocking/sync-over-async, use async I/O, bound parallel fan-out, scale horizontally with sensible concurrency limits — **Program.cs** Section 8 warns to use `SetMinThreads` sparingly.
- **`SetMaxThreads`:** Capping the pool can create unbounded queue backlog — rarely the first lever; fix slow callbacks first.


---
## Q5. (M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?

```csharp
ThreadPool.GetMaxThreads(out int maxW, out _);
ThreadPool.GetAvailableThreads(out int freeW, out _);
int busyWorkers = maxW - freeW;
_logger.LogWarning("Pool busy workers: {Busy}/{Max}", busyWorkers, maxW);

// Typical callback queued during the spike:
ThreadPool.QueueUserWorkItem(_ =>
{
    using var conn = _db.OpenConnection();          // blocks until pool slot
    Thread.Sleep(120);                              // simulated slow query
    conn.Execute("UPDATE Inventory SET Qty = Qty - 1 WHERE Sku = @sku", sku);
});
```

Explain how worker threads can be "busy" without saturating CPU, and what you would change first.

**Concepts**
- GetAvailableThreads snapshot
- low CPU high busy-pool
- blocking worker threads
- async ADO.NET fix
- starvation vs insufficient cores

**Answer:** Worker threads blocked on I/O or locks still count as busy (`max − available`), even when they are not executing CPU instructions — so low CPU with a exhausted-looking pool usually means blocking work on the pool, not insufficient cores.

- The callback opens a DB connection and sleeps — both block the worker without consuming CPU; many such items queue up and starve unrelated pool users (timers, ASP.NET, other `QueueUserWorkItem` work).
- `GetAvailableThreads` is a snapshot — useful for trend logging, not a capacity plan by itself; pair with queue wait time, request latency, and thread-pool starvation counters.
- **First change:** Move blocking DB access to async ADO.NET (`await conn.OpenAsync`, async execute) so workers release during I/O waits (IOCP path — Section 9 preview).
- **Second:** Do not perform long synchronous DB work directly on thread-pool threads — use a bounded dedicated worker or channel with explicit concurrency.
- **Not first:** Buying cores or blindly raising `SetMinThreads` — that multiplies blocked threads, not useful parallelism.


---
## Q6. (D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

**Concepts**
- manual Thread vs ThreadPool for batch
- 2000 threads memory cost
- pool amortization for short jobs
- Task.WhenAll batch pattern
- when manual Thread fits

**Answer:**

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

**Answer:** Ship **B** (pool or modern `Task`/`Parallel` APIs) for this workload — many short, independent CPU jobs are exactly what the thread pool amortizes (**Program.cs** Section 10 comparison table).

- **Throughput:** Manual threads pay OS creation/teardown per job; the pool reuses workers and scales with processor count — the chapter's Stopwatch demo (Section 11) shows pool wins for large batches.
- **Memory:** 2,000 manual threads ≈ gigabytes of default stack reservation; the pool holds a bounded worker set capped by `GetMaxThreads`.
- **Operational risk:** Thread explosion can OOM or thrash the scheduler; pool caps growth and integrates with existing ASP.NET/`Task` infrastructure.
- **When manual `Thread` still fits:** One or few long-lived workers (custom name/priority, foreground lifetime, special apartment/stack) — not 2,000 ephemeral resize jobs.
- **Modern default:** Prefer `Task.Run` + `Task.WhenAll` or `Parallel.For` with `MaxDegreeOfParallelism` for clearer cancellation/exception handling; `QueueUserWorkItem` remains valid for legacy fire-and-forget patterns.


---
## Q7. (R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.

---

### 03. Tasks & Task Parallel Library


**Answer:**

```csharp
public void QueueLineValidations(IReadOnlyList<InvoiceLineJob> lines)
{
    var failures = new List<string>(); // shared across callbacks

    foreach (var line in lines)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (!TryValidate(line, out var error))
                failures.Add(error); // no lock
        });
    }

    // caller returns HTTP 202 immediately — no wait for pool work
}
```

List the defects (correctness, observability, and API contract) and how you would harden this for production.

**Answer:** The method queues work and returns before validations finish, mutates a non-thread-safe `List<string>` from multiple pool threads without synchronization, and never surfaces callback exceptions — so clients get 202 while data is incomplete and errors are lost.


**Fix (priority order):**

1. Decide contract: if the API must return after validation, wait with `CountdownEvent`/`Task.WhenAll` (and return 200/422 with results); if truly background, enqueue to a durable queue (Azure Service Bus, Hangfire) — not naked `QueueUserWorkItem` from a request.
2. Replace `List<string>` with `ConcurrentBag<string>` or write each failure to `results[i]` then merge after the wait — one writer per index avoids locks (**Program.cs** Section 4 pattern).
3. Wrap callback body in try/catch/finally — log and signal completion in `finally`.
4. Propagate unhandled failures to telemetry (`ILogger`, Application Insights unhandled exception tracking).


---

### 03. Tasks & Task Parallel Library




---
## Q1. (R) An ASP.NET Core batch-validation endpoint works in dev but stalls under load. Review the action:

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

**Concepts**
- .Result sync-over-async on request thread
- Task.Run for I/O-bound work
- async action signature
- CancellationToken on endpoint
- SemaphoreSlim concurrency cap

**Answer:**

```csharp
[HttpPost("orders/validate-batch")]
public IActionResult ValidateBatch([FromBody] int[] orderIds)
{
    Task<bool>[] validations = orderIds
        .Select(id => Task.Run(() => _orderValidator.Validate(id)))
        .ToArray();

    bool[] results = Task.WhenAll(validations).Result;
    return Ok(new { ValidCount = results.Count(r => r) });
}
```

What are the problems (threading, scalability, and API shape), and how do you fix them in priority order?

**Answer:** The action blocks the request thread with `.Result` on `Task.WhenAll`, which is sync-over-async on ASP.NET Core's thread pool and can cause starvation or deadlocks under concurrency — compounded by wrapping likely I/O-bound validation in `Task.Run`, which wastes pool threads.


**Fix (priority order):**

1. Change signature to `async Task<IActionResult> ValidateBatch(..., CancellationToken ct)` and `bool[] results = await Task.WhenAll(validations)`.
2. If validation is I/O-bound, call `_orderValidator.ValidateAsync(id, ct)` directly — no `Task.Run`.
3. Cap concurrency for large batches (`SemaphoreSlim`, `Parallel.ForEachAsync` with `MaxDegreeOfParallelism`, or chunked `WhenAll`).
4. Return early or fail fast if `orderIds` exceeds a configured limit.


---
## Q2. (R) A fulfillment service refactored raw threads to tasks, but ops reports missing line picks and intermittent duplicate shipments. Review:

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

**Concepts**
- unattached nested Task.Run
- parent completes before children
- TaskCreationOptions.AttachedToParent
- Task.Factory.StartNew scheduler
- Task.WhenAll for child collection

**Answer:**

```csharp
public Task FulfillMultiLineOrder(Order order)
{
    return Task.Factory.StartNew(() =>
    {
        foreach (string line in order.Lines)
        {
            Task.Run(() =>
            {
                Thread.Sleep(40);
                _pickLog.Record(order.OrderId, line);
            });
        }
    });
}

// Caller in a background worker:
_fulfillment.FulfillMultiLineOrder(order).Wait();
Console.WriteLine("Fulfillment complete — releasing dock slot");
```

What fails at runtime, and what is the corrected task composition?

**Answer:** The parent task completes as soon as the `StartNew` delegate returns — before nested `Task.Run` children finish — so the caller releases the dock slot while picks are still in flight. `Task.Factory.StartNew` without an explicit scheduler also inherits `TaskScheduler.Current`, which can inline work unexpectedly.


**Fix (priority order):**

1. Collect child tasks and wait for all: `var picks = order.Lines.Select(line => Task.Run(() => Pick(line))).ToArray(); Task.WhenAll(picks).Wait();` inside the parent — or prefer `async`/`await` with `await Task.WhenAll(picks)` (ch.04).
2. For parent/child lifetime linking, use `TaskCreationOptions.AttachedToParent` with explicit `TaskScheduler.Default` on `StartNew` — as in **Program.cs** Section 12 — only when you truly need attached semantics.
3. Replace outer `Task.Factory.StartNew` with `Task.Run` unless non-default creation options are required.
4. Propagate exceptions: observe all child tasks; a faulted pick must fail the fulfillment operation, not disappear.


---
## Q3. (R) A payment integration wraps a legacy callback gateway with `TaskCompletionSource`. Declined payments sometimes hang until timeout; approved payments occasionally throw `InvalidOperationException`. Review:

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

**Concepts**
- TaskCompletionSource event handler leak
- TrySetResult vs SetResult
- timeout registration
- no completion path hang
- idempotent TCS completion

**Answer:**

```csharp
public Task<PaymentResult> ChargeAsync(int orderId, decimal amount)
{
    var tcs = new TaskCompletionSource<PaymentResult>();

    _gateway.PaymentCompleted += result =>
    {
        tcs.SetResult(result);
    };

    _gateway.PaymentFailed += ex =>
    {
        tcs.SetException(ex);
    };

    _gateway.Charge(orderId, amount);
    return tcs.Task;
}
```

What production defects are embedded here, and how do you harden the wrapper?

**Answer:** The wrapper leaks event handlers on every call, uses throwing `SetResult`/`SetException` instead of `TrySet*`, and has no path to complete the task if the gateway never fires — so callers hang. A second callback can throw `InvalidOperationException` when the task is already completed.


**Fix (priority order):**

1. Use `TrySetResult`, `TrySetException`, `TrySetCanceled` and unsubscribe handlers in the callback after the first terminal signal.
2. Register `CancellationToken` / `CancelAfter` to call `TrySetCanceled` when the HTTP/client timeout fires.
3. Create one `TaskCompletionSource` per `ChargeAsync` invocation (already implied) — never share across concurrent calls.
4. Optionally wrap with `Task.WhenAny(tcs.Task, timeoutTask)` for defense in depth.

```csharp
void CompleteOnce(Action complete) { if (tcs.TrySetResult(default!)) { /* use TrySet* */ complete(); } }
// Prefer: if (tcs.TrySetResult(result)) { _gateway.PaymentCompleted -= handler; }
```


---
## Q4. (R) A shipping pipeline chains pick → label with continuations after removing `async/await` "for clarity." Fault injection tests crash the worker process. Review:

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

**Concepts**
- ContinueWith default runs on any state
- TaskContinuationOptions.OnlyOnRanToCompletion
- AggregateException from antecedent.Result
- fault handler branch
- await vs ContinueWith stack traces

**Answer:**

```csharp
Task<string> pickTask = Task.Run(() =>
{
    if (_inventory.IsEmpty(slotId))
        throw new InvalidOperationException("Inventory slot empty");
    return $"Picked order #{orderId}";
});

Task<string> labelTask = pickTask.ContinueWith(
    antecedent => _labelService.Create(antecedent.Result));

string label = labelTask.Result;
_audit.Log($"Label created: {label}");
```

What breaks when the pick task faults, and how should the continuation chain be written?

**Answer:** When the antecedent is faulted, the continuation still runs by default and accessing `antecedent.Result` rethrows — often as `AggregateException` — instead of routing to a fault handler. Unobserved or poorly observed faulted continuations can tear down the process via `TaskScheduler.UnobservedTaskException`.


**Fix (priority order):**

1. Add status filter: `ContinueWith(..., TaskContinuationOptions.OnlyOnRanToCompletion)` for the label step.
2. Add fault handler: `pickTask.ContinueWith(t => Log(t.Exception), TaskContinuationOptions.OnlyOnFaulted)`.
3. Prefer `await pickTask` / `await pickTask.ContinueWith(...)` (ch.04) — compiler preserves stack traces better than manual `ContinueWith`.
4. Replace final `.Result` with `await labelTask` or `GetAwaiter().GetResult()` only at a true sync boundary.


---
## Q5. (P) A warehouse API throttles concurrent picks with `SemaphoreSlim` (matching the chapter pattern). After a downstream timeout spike, throughput collapses to zero until restart. Review:

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

**Concepts**
- SemaphoreSlim release in finally
- cancellation mid-flight leaks slot
- throttle pattern with try/finally
- CurrentCount health check
- permanent throughput collapse on exception

**Answer:**

```csharp
private readonly SemaphoreSlim _pickerGate = new(4, 4);

public async Task<string> PickOrderAsync(Order order, CancellationToken ct)
{
    await _pickerGate.WaitAsync(ct);
    string pickResult = await _warehouseClient.PickAsync(order, ct);
    _pickerGate.Release();
    return pickResult;
}
```

What fails when `PickAsync` throws or the request is canceled mid-flight, and what is the production-safe throttle pattern?

**Answer:** `Release()` is not in a `finally` block — any exception or cancellation after `WaitAsync` consumes a semaphore slot permanently. After enough failures, all four slots are held and every new pick blocks forever until process restart.

- Wrap the guarded work in `try/finally` and call `_pickerGate.Release()` in `finally` — matching **Program.cs** Section 10 (`ThrottledPick`).
- Prefer `await _pickerGate.WaitAsync(ct)` with the same `CancellationToken` passed to downstream calls so aborting a request releases the wait cleanly.
- Consider `SemaphoreSlim` as a singleton with explicit max count documented; dispose only on application shutdown — not per request.
- Monitor `_pickerGate.CurrentCount` in health checks to detect leak regressions early.


---
## Q6. (M) A carrier-selection service uses `Task.WhenAny` to take the fastest quote (as in the chapter demo). Load tests show open HTTP connection counts climbing. Review:

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

**Concepts**
- Task.WhenAny winner found
- loser tasks continue holding resources
- linked CancellationTokenSource to cancel losers
- blocking .Result on sync caller
- IHttpClientFactory lifetime

**Answer:**

```csharp
public decimal GetBestShippingRate(Order order)
{
    Task<CarrierQuote>[] carrierTasks =
    [
        Task.Run(() => _fastFreight.Quote(order)),
        Task.Run(() => _economyPost.Quote(order)),
        Task.Run(() => _premiumAir.Quote(order)),
    ];

    Task<CarrierQuote> winner = Task.WhenAny(carrierTasks).Result;
    return winner.Result.Price;
}
```

Why do losing carrier calls keep consuming resources, and what changes after you pick a winner?

**Answer:** `Task.WhenAny` completes when the first task finishes — it does not cancel or dispose the slower tasks. Losing carrier HTTP calls continue until completion, holding connections, thread-pool slots, and memory under sustained load.

- After `WhenAny`, cancel remaining work with a linked `CancellationTokenSource` passed into each quote call, then `cts.Cancel()` once the winner is chosen.
- If APIs are not cancelable, track in-flight calls and abandon results safely — but still close/dispose `HttpResponseMessage` and respect `IHttpClientFactory` lifetimes.
- Replace blocking `.Result` with `await Task.WhenAny(...)` in an async API so the request thread is not blocked during the race.
- Log slow-loser latency separately — persistent tail latency after "winner found" signals missing cancellation.


---
## Q7. (D) A team must batch-validate four thousand orders every night. One developer proposes `Task.WaitAll` on thousands of `Task.Run(() => Validate(order))` calls; another wants `Parallel.ForEach` immediately; a third wants `async`/`await` with `Task.WhenAll` and a concurrency limit. What breaks at scale with the first approach, and what pattern would you ship?

---
### 04. Async and Await


**Answer:** Launching four thousand simultaneous `Task.Run` validations queues thousands of work items at once, spiking thread-pool usage and likely overwhelming the database — `Task.WaitAll` also blocks the orchestrator thread until every task completes, with no backpressure. `Parallel.ForEach` helps CPU-bound validation but is the wrong default if validation is I/O-bound and still needs a concurrency cap for downstream limits.

- Ship chunked or throttled async orchestration: `await Task.WhenAll(batch.Select(o => ValidateAsync(o, ct)))` over batches of 50–200, or use `SemaphoreSlim` / `Parallel.ForEachAsync` with `MaxDegreeOfParallelism` tuned to DB connection limits.
- Use `Task.Run` only for CPU-bound validation logic; I/O-bound checks should be truly async end-to-end (ch.04).
- Prefer `Task.WhenAll` over `Task.WaitAll` in async hosts — composable with cancellation and does not block a precious thread for the entire batch duration.
- Emit metrics: queue depth, validation latency p95, and faulted task count — unobserved faults in nightly jobs can fail silently until morning.


---
### 04. Async and Await




---
## Q1. (R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?

**Concepts**
- .Result blocking on async endpoint
- sync-over-async thread pool starvation
- async Task<IActionResult> signature
- await Task.WhenAll usage
- ConfigureAwait(false) in library call

**Answer:**

```csharp
app.MapGet("/reports/{id}", (ReportService svc, string id) =>
{
    var metadata = svc.FetchMetadataAsync(id).Result;
    svc.ProcessReportAsync(metadata).Wait();
    return Results.Ok(metadata);
});

public class ReportService
{
    public async Task<ReportMetadata> FetchMetadataAsync(string id)
    {
        await Task.Delay(100); // HTTP to upstream
        return new ReportMetadata(id, 500);
    }

    public async Task ProcessReportAsync(ReportMetadata m) =>
        await Task.Delay(80);
}
```

What are the problems (runtime, scalability, API design), and how do you fix them in priority order?

**Answer:** The endpoint blocks a thread-pool thread twice via `.Result` and `.Wait()` on unfinished Tasks, defeating ASP.NET Core's async I/O model and risking deadlocks when a captured request context prevents continuations from running under load.


**Fix (priority order):**

1. Change the endpoint to `async Task<IResults>` (or `async Task`) and `await` both service calls end-to-end.
2. Thread `CancellationToken` from `HttpContext.RequestAborted` into `FetchMetadataAsync` and downstream I/O.
3. Ensure the service layer stays async all the way — no `.Result` / `.Wait()` anywhere in the call chain.
4. Load-test after the change — thread-pool queue length should stay flat under I/O-bound load.

```csharp
app.MapGet("/reports/{id}", async (ReportService svc, string id, CancellationToken ct) =>
{
    var metadata = await svc.FetchMetadataAsync(id, ct);
    await svc.ProcessReportAsync(metadata, ct);
    return Results.Ok(metadata);
});
```


---
## Q2. (R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?

**Concepts**
- async void exception uncatchable
- process crash on unhandled async void
- fire-and-forget orchestration
- async Task vs async void
- event handler only use case for async void

**Answer:**

```csharp
public class ExportOrchestrator
{
    public void StartExport(string reportId)
    {
        LogExportStarted(reportId); // kicks off async work
        _ = RunExportPipelineAsync(reportId); // fire-and-forget
    }

    private async void LogExportStarted(string reportId)
    {
        await Task.Delay(50);
        throw new InvalidOperationException("Audit sink unreachable");
    }

    private async Task RunExportPipelineAsync(string reportId)
    {
        await Task.Delay(200);
        Console.WriteLine($"Export complete: {reportId}");
    }
}
```

What fails at runtime, and what pattern replaces this wiring?

**Answer:** `LogExportStarted` is `async void`, so its exception cannot be caught by the caller and propagates through the synchronization context as an unhandled exception — often terminating the worker. The fire-and-forget pipeline Task is also unobserved, so its failures are silent until an unobserved-task handler fires.


**Fix (priority order):**

1. Change `LogExportStarted` to `async Task` and await it (or fold logging into the pipeline).
2. Return `Task` from `StartExport` / `RunExportPipelineAsync` and await at the host boundary (BackgroundService, Hangfire job, Azure Function entry).
3. Wrap the pipeline in try/catch with structured logging; rethrow or surface failure to the scheduler.
4. Reserve `async void` strictly for UI event handlers — see **Program.cs** Section 4.

```csharp
public async Task StartExportAsync(string reportId, CancellationToken ct)
{
    await LogExportStartedAsync(reportId, ct);
    await RunExportPipelineAsync(reportId, ct);
}
```


---
## Q3. (R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?

**Concepts**
- WPF SynchronizationContext deadlock
- ConfigureAwait(false) in library
- .GetResult() blocking UI thread
- await in library code
- UI thread captured context loop

**Answer:**

```csharp
// ReportLib.dll — reusable helper
public static class ReportFetcher
{
    public static async Task<string> GetReportAsync(string id)
    {
        await Task.Delay(100); // simulates I/O
        return $"report:{id}";
    }
}

// App startup on UI thread
public void LoadReportOnStartup()
{
    string data = ReportFetcher.GetReportAsync("Q1").Result;
    ReportLabel.Text = data;
}
```

What causes the deadlock, and what changes fix it on both sides?

**Answer:** The UI thread blocks on `.Result` while the async continuation tries to marshal back to the same UI thread (default `await` captures `SynchronizationContext`). The blocked UI thread cannot run the continuation — classic sync-over-async deadlock.


**Fix (priority order):**

1. **App:** Make `LoadReportOnStartup` async (`async void` acceptable for UI event) and `await ReportFetcher.GetReportAsync("Q1")`.
2. **Library:** Add `.ConfigureAwait(false)` on every await in reusable code that does not touch UI after the await — see **Program.cs** Section 5.
3. Never expose sync `.Result` / `.Wait()` wrappers from library public APIs.
4. If sync API is unavoidable at a legacy boundary, document it as UI-thread-unsafe and offload with `Task.Run` only as a last resort (still inferior to async all the way).

```csharp
// Library
await Task.Delay(100).ConfigureAwait(false);

// UI
private async void LoadReportOnStartup()
{
    string data = await ReportFetcher.GetReportAsync("Q1");
    ReportLabel.Text = data;
}
```


---
## Q4. (P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?

**Concepts**
- CancellationToken ignored by legacy client
- WhenAny with Task.Delay timeout
- CancelAfter for download timeout
- abandon vs cancel distinction
- timeout wrapper pattern

**Answer:**

```csharp
public async Task<byte[]> DownloadReportAsync(CancellationToken ct)
{
    Task<byte[]> download = _legacyClient.DownloadAsync(url); // no token overload
    Task delay = Task.Delay(TimeSpan.FromSeconds(30), ct);

    Task finished = await Task.WhenAny(download, delay);
    if (finished == download)
        return await download;

    throw new OperationCanceledException(ct);
}
```

What breaks in production when callers cancel or time out, and how should the service boundary handle abandoned work?

**Answer:** `Task.WhenAny` only stops awaiting the loser — the legacy download keeps running in the background after timeout or cancellation. Under repeated cancels, orphaned downloads accumulate, wasting sockets, memory, and upstream quota; callers believe work stopped but it did not.

- **Abandoned work:** When `delay` wins, `download` is never awaited — exception on faulted Task may become unobserved; successful completion is silently ignored but still consumed resources.
- **No cooperative cancel:** Legacy API cannot be interrupted — only the wrapper's wait ends.
- **Resource leaks:** Connection pool exhaustion when many users navigate away or hit client-side timeouts.

**Production pattern:**

1. Document that this is **best-effort abandonment**, not true cancellation — matches **Program.cs** Section 8.
2. Prefer upgrading the legacy client or wrapping at a process boundary you can kill (separate worker, linked `CancellationTokenSource` with timeout).
3. If stuck with `WhenAny`, track in-flight downloads in a registry; optionally use a `CancellationTokenSource` linked to caller token + timeout and log abandoned operation IDs for ops visibility.
4. Surface `OperationCanceledException` to callers but monitor background completion rate — alert if abandoned tasks pile up.
5. For HTTP specifically, migrate to `HttpClient` with `CancellationToken` and `IHttpClientFactory` rather than permanent `WhenAny` shims.


---
## Q5. (R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?

**Concepts**
- CancellationToken not passed to retry loop
- OperationCanceledException not re-thrown
- catch swallows cancellation
- retry ignores user cancel
- pass token to Task.Delay in backoff

**Answer:**

```csharp
public async Task<T> RetryAsync<T>(
    Func<Task<T>> operation,
    int maxAttempts,
    CancellationToken ct)
{
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            await Task.Delay(attempt * 2000); // fixed backoff, no token
        }
    }
    throw new InvalidOperationException("Retries exhausted.");
}

// Caller passes ct from HttpContext.RequestAborted
var data = await RetryAsync(() => FetchReportAsync(id), maxAttempts: 5, ct);
```

What are the defects, and how do you fix the retry contract for production?

**Answer:** The retry loop catches `OperationCanceledException` and retries anyway, and the backoff delay ignores `ct` — so user disconnect or request abort does not stop retries until all attempts and delays finish.


**Fix (priority order):**

1. Exclude cancellation from retry filter: `catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)`.
2. Pass token to delay: `await Task.Delay(attempt * 2000, ct)`.
3. Pass `ct` into `FetchReportAsync` inside the lambda so in-flight I/O aborts cooperatively.
4. Optionally distinguish transient faults (`HttpRequestException`, 503) from permanent errors — do not retry 400-class failures.
5. Align with **Program.cs** Section 9 — retry skeleton respects `CancellationToken` and skips `OperationCanceledException`.

```csharp
catch (Exception ex) when (attempt < maxAttempts && ex is not OperationCanceledException)
{
    await Task.Delay(attempt * 2000, ct);
}
```


---
## Q6. (D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?

**Concepts**
- lock statement with await compile error
- SemaphoreSlim.WaitAsync async gate
- singleton async locking
- async-compatible mutual exclusion
- lock vs SemaphoreSlim(1,1)

**Answer:**

```csharp
public sealed class ReportExportService
{
    private static readonly SemaphoreSlim ExportGate = new(1, 1);

    public async Task ExportAsync(string name, CancellationToken ct)
    {
        await ExportGate.WaitAsync(ct);
        await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
        // forgot Release — relying on GC
    }
}
```

What production failures appear under concurrency, cancellation, and multi-instance deployment — and what is the correct pattern?

**Answer:** Missing `Release()` in `finally` permanently reduces the semaphore count after the first export — subsequent callers block forever on `WaitAsync`. A static gate also only serializes within one process, not across scaled-out instances.

- **Missing `finally` / `Release`:** One successful path consumes the permit; second export deadlocks all waiters — matches broken mutex usage, not the **Program.cs** Section 10 pattern.
- **Exception before `Release`:** Any fault between `WaitAsync` and manual release leaks a permit — always `try/finally`.
- **Cancellation during `WaitAsync`:** Correctly throws without acquiring — OK; cancellation during write must still run `Release` if wait succeeded.
- **Multi-instance:** Static `SemaphoreSlim` is per-process — two pods write concurrently to shared storage; need distributed lock (blob lease, Redis RedLock, DB advisory lock) for cluster-wide exclusivity.
- **Singleton + static gate:** Redundant — instance field on scoped service or explicit distributed lock is clearer for testability.

**Correct pattern:**

```csharp
await ExportGate.WaitAsync(ct);
try
{
    await File.WriteAllTextAsync($"exports/{name}.json", "{}", ct);
}
finally
{
    ExportGate.Release();
}
```

For multi-node: replace in-memory gate with storage-level lease; keep `SemaphoreSlim` only for single-process throttling.


---
## Q7. (M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?

---

### 05. Parallel Programming


**Answer:**

```csharp
public class ReportCache
{
    private ValueTask<int>? _cachedCount;

    public async ValueTask<int> GetRowCountAsync(string reportId)
    {
        if (_cachedCount is null)
            _cachedCount = ComputeCountAsync(reportId);
        return await _cachedCount.Value;
    }

    private async ValueTask<int> ComputeCountAsync(string reportId)
    {
        await Task.Delay(10);
        return 42;
    }
}

// Two concurrent callers:
var t1 = cache.GetRowCountAsync("Q1");
var t2 = cache.GetRowCountAsync("Q1");
await Task.WhenAll(t1.AsTask(), t2.AsTask());
```

What rule of `ValueTask` was violated, and how should caching expose async results safely?

**Answer:** A `ValueTask` / `ValueTask<T>` must be consumed exactly once — storing it in a field and awaiting it from two concurrent callers violates that rule, producing `InvalidOperationException` when the second await tries to reuse the same instance.

- **Single consumption:** Unlike `Task`, `ValueTask` may wrap a pooled `IValueTaskSource` — double-await is undefined.
- **Concurrency:** Two threads can both see `_cachedCount is null` and create races even before double-await.
- **Preview scope:** **Program.cs** Section 13 warns — do not await a `ValueTask` twice or store for later; consume immediately.

**Safe caching options:**

1. Cache the **`Task<int>`** (or `int` after completion), not the `ValueTask` — e.g. `private Task<int>? _countTask;` assigned once under lock.
2. Use `Lazy<Task<int>>` or `AsyncLazy<T>` pattern for memoized async initialization.
3. If the method often completes synchronously (cache hit), return `ValueTask.FromResult(cachedInt)` on the hot path and only allocate `Task` on miss — but never share one `ValueTask` instance across callers.
4. For concurrent first access, use `SemaphoreSlim` or `lock` around cache population.

```csharp
private Task<int>? _countTask;

public Task<int> GetRowCountAsync(string reportId) =>
    _countTask ??= ComputeCountAsync(reportId).AsTask();
```


---

### 05. Parallel Programming




---
## Q1. (R) A nightly warehouse job sums reconciled inventory values in parallel. Finance reports totals that drift from the serial baseline. Review the hot path:

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?

**Concepts**
- parallel inventory aggregation race
- shared accumulator without synchronization
- Parallel.ForEach thread-local overload
- Interlocked.Add for atomic merge
- per-partition accumulation pattern

**Answer:**

```csharp
public decimal ReconcileBatchTotal(IReadOnlyList<StockRecord> batch)
{
    decimal runningTotal = 0m;

    Parallel.ForEach(batch, record =>
    {
        runningTotal += record.ReconciledValue;
    });

    return runningTotal;
}
```

What is wrong, why does it pass some nights and fail others, and how do you fix it without locking on every line?

**Answer:** `runningTotal += …` is not atomic — parallel workers read-modify-write the same `decimal` and lose updates, so totals are nondeterministic. It appears to pass when contention is low or the batch is small, then drifts under heavier parallel scheduling.


**Fix (priority order):**

1. Use thread-local aggregation on `Parallel.For` / `Parallel.ForEach` — merge once per worker, not per line (see **Program.cs** Section 2b and `ThreadSafeDecimal.Add`).
2. Prefer `batch.AsParallel().Sum(r => r.ReconciledValue)` for a read-only reduction — PLINQ handles partition/merge internally.
3. If you must share one accumulator, merge under `lock` only in the finalizer delegate — never inside the per-item body.
4. Add a serial golden-sum test in CI for the same batch to catch drift before finance does.

```csharp
decimal total = 0m;
Parallel.ForEach(
    batch,
    () => 0m,
    (record, _, local) => local + record.ReconciledValue,
    local => ThreadSafeDecimal.Add(ref total, local));
```


---
## Q2. (R) A teammate parallelizes audit-log line generation for the same SKU batch:

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?

**Concepts**
- parallel log generation shared List
- ConcurrentBag for parallel collection
- lock vs concurrent collection
- thread-safe aggregation
- Parallel.ForEach body collects to local

**Answer:**

```csharp
public IReadOnlyList<string> BuildAuditTrail(IEnumerable<StockRecord> batch)
{
    var auditLines = new List<string>();

    Parallel.ForEach(batch, record =>
    {
        string line = $"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}";
        auditLines.Add(line);
    });

    return auditLines.OrderBy(l => l).ToList();
}
```

Identify compile-time, runtime, and scalability problems. What production pattern replaces the shared `List<T>`?

**Answer:** `List<T>` is not thread-safe — concurrent `Add` calls corrupt internal state (exceptions, lost entries, or rare structural damage). Ordering after the fact does not fix the race, and parallel iteration over a non-indexable `IEnumerable` may buffer or enumerate unsafely depending on the source.


**Fix (priority order):**

1. Replace with `ConcurrentBag<string>` for lock-free per-thread staging, then sort once at the end — matches the audit pattern in **Program.cs** Section 3 (lock shown there; concurrent collection is cleaner at scale).
2. Materialize to `List<StockRecord>` or an array **before** parallel work if the source is deferred or not thread-safe.
3. If order must be deterministic and cheap, consider parallel map into a pre-sized `string[]` by index when indices exist.
4. Never share a `DbContext` or single connection across parallel bodies — load the batch in one scoped query first.

```csharp
var bag = new ConcurrentBag<string>();
Parallel.ForEach(batchList, record =>
{
    bag.Add($"{record.Sku}: {record.OnHand} × {record.AdjustmentFactor}");
});
return bag.OrderBy(l => l).ToList();
```


---
## Q3. (R) Under load, a reporting endpoint times out and thread-pool starvation alerts fire. Review the "optimization" added to fetch order details:

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?

**Concepts**
- Parallel.ForEach with async delegate
- GetAwaiter().GetResult() in parallel body
- pool starvation from sync-over-async
- Parallel.ForEachAsync .NET 6
- SemaphoreSlim + Task.WhenAll alternative

**Answer:**

```csharp
public IActionResult ExportOrders([FromBody] int[] orderIds)
{
    var lines = new List<string>();

    Parallel.ForEach(orderIds, id =>
    {
        string json = _httpClient
            .GetStringAsync($"/internal/orders/{id}")
            .GetAwaiter()
            .GetResult();

        lines.Add(json);
    });

    return Ok(lines);
}
```

What stacked issues make this worse than a serial loop in production?

**Answer:** This is I/O-bound work forced through parallel sync-over-async — each iteration blocks a thread-pool thread waiting on HTTP, while `Parallel.ForEach` multiplies concurrent blocked threads. Combined with an unsynchronized `List<T>`, you get starvation, wrong results, and socket exhaustion.


**Fix (priority order):**

1. Make the action async end-to-end: `Parallel.ForEachAsync` ( .NET 6+ ) or `Task.WhenAll` with a `SemaphoreSlim` cap — **not** `Parallel.ForEach` + `.Result`.
2. Use `IHttpClientFactory` and pass `CancellationToken` from `HttpContext.RequestAborted`.
3. Collect results in a thread-safe structure or pre-sized array indexed by position.
4. Cap concurrency (`MaxDegreeOfParallelism` or semaphore) to protect the internal API and thread pool.

```csharp
public async Task<IActionResult> ExportOrders(int[] orderIds, CancellationToken ct)
{
    var lines = new string[orderIds.Length];
    await Parallel.ForEachAsync(
        orderIds.Select((id, i) => (id, i)),
        new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = ct },
        async (item, token) =>
        {
            lines[item.i] = await _httpClient.GetStringAsync($"/internal/orders/{item.id}", token);
        });
    return Ok(lines);
}
```


---
## Q4. (P) A CPU-bound pricing engine recalculates thousands of in-memory `StockRecord` rows on a 16-core VM shared with other services. A developer caps workers like this:

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?

**Concepts**
- MaxDegreeOfParallelism with Environment.ProcessorCount
- shared-core VM oversubscription
- leave cores for other services
- ParallelOptions tuning
- CPU-bound vs I/O-bound degree

**Answer:**

```csharp
var options = new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount,
};

Parallel.For(0, batch.Count, options, i =>
{
    batch[i] = ApplyMarginRules(batch[i]);
});
```

When is this cap wrong on a shared host, and how do you choose `MaxDegreeOfParallelism` and cancellation for a batch job that must leave headroom for the web tier?

**Answer:** `Environment.ProcessorCount` on a 16-core box allows 16 concurrent workers for this loop alone, which can starve Kestrel, GC, and other tenants on the same VM. The default `-1` is also often too aggressive on shared infrastructure — you need an explicit cap from configuration plus cooperative cancellation.

- Read `MaxDegreeOfParallelism` from `IOptions<PricingEngineOptions>` (e.g. 4 on a shared 16-core host) — same intent as **Program.cs** Section 5 throttling.
- Wire `CancellationToken` from `IHostApplicationLifetime.ApplicationStopping` or job timeout so deploys and scale-in cancel long batches via `OperationCanceledException` — see Section 6a.
- Leave at least one core for the web tier and system processes unless this worker runs on a dedicated node pool.
- Measure: if CPU is already saturated, raising parallelism does not help; if workers block on locks, lowering parallelism can **improve** throughput.


---
## Q5. (D) A reconciliation worker must stop processing once cumulative value crosses a credit limit — not process the entire batch. Two implementations were proposed:

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?

**Concepts**
- ParallelLoopState.Break vs Stop
- LowestBreakIteration semantics
- AggregateException from parallel loop
- Stop terminates ASAP
- Break processes lower indices first

**Answer:**

**Option A — `ParallelLoopState.Break()` after a locked running total:**

```csharp
decimal running = 0m;
object gate = new();

Parallel.For(0, batch.Count, (i, state) =>
{
    lock (gate)
    {
        running += batch[i].ReconciledValue;
        if (running >= creditLimit)
            state.Break();
    }
});
```

**Option B — serial scan with early `break`:**

```csharp
decimal running = 0m;
for (int i = 0; i < batch.Count; i++)
{
    running += batch[i].ReconciledValue;
    if (running >= creditLimit)
        break;
}
```

Which do you ship for correctness and throughput, and what does `Break()` guarantee (and not guarantee) about iterations that already started?

**Answer:** Ship **Option B** for a strict cumulative threshold on ordered data — it is deterministic, needs no lock, and stops exactly when the limit is crossed. Option A adds lock contention on every iteration (often erasing parallel benefit) and still does not give strict "process until limit" semantics.

- `Break()` stops **starting** iterations with index **greater than** the break index; lower-index iterations may still be running or not yet started — see **Program.cs** Section 6b (`LowestBreakIteration`, `IsCompleted` false).
- Iterations with index ≤ break index are **not** cancelled — already-started higher-index work may still complete briefly before the loop winds down.
- Parallel order of accumulation is nondeterministic unless the batch order defines business meaning — cumulative credit limits usually require serial order or partitioned serial phases.
- If the batch is huge and per-item CPU work is heavy **and** order does not matter for the limit, consider parallel partial sums then a serial merge — not locked `Break()` on every line.


---
## Q6. (R) A dashboard query was "speed up" with PLINQ. Users see wrong top-SKU ordering under load and elevated CPU:

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?

**Concepts**
- PLINQ AsOrdered merge overhead
- wrong ordering without AsOrdered
- AsUnordered performance benefit
- order-preserving vs throughput
- PLINQ ordering cost

**Answer:**

```csharp
var topSkus = batch
    .AsParallel()
    .WithDegreeOfParallelism(8)
    .Where(r => r.ReconciledValue >= threshold)
    .OrderByDescending(r => r.ReconciledValue)
    .Take(5)
    .Select(r => r.Sku)
    .ToList();
```

Later, a second developer adds `AsOrdered()` before `OrderByDescending` "to fix ordering." Review both versions — what is redundant, what still breaks, and when is PLINQ the wrong tool here?

**Answer:** The first query already applies a global `OrderByDescending` — PLINQ merges partitions correctly for that operator, so "wrong ordering" likely comes from **nondeterministic ties** (equal `ReconciledValue`) or from mutating `batch` during the query, not from missing `AsOrdered()`. Adding `AsOrdered()` before `OrderByDescending` forces ordered merge overhead **twice** and hurts CPU without fixing tie-breaking.


**Fix (priority order):**

1. For dozens/hundreds of rows, use serial LINQ — often faster than PLINQ setup cost on tiny sequences (**Program.cs** tiny-array demo).
2. If CPU work per row is large and count is thousands+, keep `AsParallel()` but drop redundant `AsOrdered()` unless downstream requires input-order preservation without a sort key.
3. Add deterministic tie-break: `.OrderByDescending(r => r.ReconciledValue).ThenBy(r => r.Sku)`.
4. If the pipeline is filter + top-N only on a hot API path, consider pre-indexing or caching — not parallel LINQ on every request.


---
## Q7. (M) A partitioner was introduced to reduce scheduling overhead on a uniform-cost batch, but throughput dropped on a 4-core machine:

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?

---

### 06. Synchronization and Locks


**Answer:**

```csharp
Parallel.ForEach(
    Partitioner.Create(0, batch.Count, rangeSize: 1),
    range =>
    {
        for (int i = range.Item1; i < range.Item2; i++)
            totals[i] = batch[i].ReconciledValue;
    });
```

The batch has 12 items; each `ReconciledValue` is a cheap multiply. What mechanism explains the slowdown, and how would you partition this workload instead?

**Answer:** `rangeSize: 1` creates one partition per index — for 12 trivial iterations that means 12 delegate invocations, partition handoffs, and thread-pool scheduling rounds. On small uniform work, that overhead dominates the multiply, so parallel is slower than a serial loop (**Program.cs** Section 11 — five-item demo).

- **Mechanism:** TPL partition granularity trades load balance against scheduling cost — micro-partitions maximize stealing flexibility but explode fixed overhead per chunk.
- **12 cheap ops:** Do not parallelize — serial `for` or simple LINQ is correct.
- **Large uniform batch:** Use default partitioning or `Partitioner.Create(0, count)` without forcing `rangeSize: 1`; let the runtime pick chunk sizes dynamically (**Section 7**).
- **Uneven per-item cost:** `Partitioner.Create(list, loadBalance: true)` for dynamic chunk stealing when row work varies widely.
- **Fixed moderate chunks:** `Partitioner.Create(0, count, rangeSize: 64)` (or similar) when items are uniform and count is in the thousands.


---

### 06. Synchronization and Locks




---
## Q1. (R) A payment microservice registers `LedgerService` as a **Singleton**. Under concurrent deposits and withdrawals, balances drift and QA sees different totals on every run. Review:

```csharp
public sealed class LedgerService
{
    private decimal _balance;

    public void Credit(decimal amount)
    {
        lock (this)
        {
            _balance += amount;
        }
    }

    public void Debit(decimal amount)
    {
        lock (typeof(LedgerService))
        {
            _balance -= amount;
        }
    }

    public decimal Balance => _balance; // read without synchronization
}
```

What fails in production, and what is the prioritized fix?

**Concepts**
- LedgerService singleton unsynchronized
- balance drift under concurrency
- lock object per instance
- Interlocked for atomic counter
- thread-safe balance operations

**Answer:** `Credit` and `Debit` lock on different objects (`this` vs `typeof(LedgerService)`), so they do not serialize against each other, and `Balance` reads `_balance` without any lock. The singleton shares one field across all requests — you get lost updates and torn reads.


**Fix (priority order):**

1. Use one private readonly sync root for all balance access — same object for Credit, Debit, and Balance getter (see `BankAccount._syncRoot` in **Program.cs** Section 3).
2. Lock (or use `Interlocked` only if you refactor to a single `long` cents field) on every read/write of `_balance`.
3. Re-evaluate singleton lifetime — per-tenant or scoped ledger may be required; at minimum document that this service is a process-wide counter, not per-account isolation.
4. Never lock on `this` or `typeof(LedgerService)`.

```csharp
private readonly object _sync = new();
private decimal _balance;

public void Credit(decimal amount) { lock (_sync) { _balance += amount; } }
public void Debit(decimal amount)  { lock (_sync) { _balance -= amount; } }
public decimal Balance { get { lock (_sync) { return _balance; } } }
```


---
## Q2. (R) A batch job transfers funds between two `BankAccount` instances on background threads. The job hangs intermittently under load — no exception, threads stuck in `Monitor.Wait`. Review:

```csharp
public static void Transfer(BankAccount from, BankAccount to, decimal amount)
{
    lock (from.SyncRoot)
    {
        Thread.Sleep(15); // simulate ledger validation
        lock (to.SyncRoot)
        {
            if (from.Balance >= amount)
            {
                from.WithdrawInternal(amount);
                to.DepositInternal(amount);
            }
        }
    }
}

// Worker pool runs Transfer(alpha, beta, 40) and Transfer(beta, alpha, 25) concurrently.
// BankAccount.Deposit/Withdraw each lock the same private SyncRoot internally.
```

What causes the hang, and how do you fix it without removing multi-account transfers?

**Concepts**
- deadlock circular lock order
- lock(a) then lock(b) vs lock(b) then lock(a)
- Monitor.Wait vs consistent ordering
- lock hierarchy to prevent deadlock
- TransferFunds lock order fix

**Answer:** `Transfer` acquires `from` then `to`, while concurrent `Transfer(beta, alpha, …)` acquires in the opposite order — classic circular wait deadlock. Nested locks on account roots that are also locked inside `Deposit`/`Withdraw` compound contention but the hang is the ordering inversion.


**Fix (priority order):**

1. Acquire locks in a consistent global order — e.g. by `AccountId` string comparison (`TransferSafely` in **Program.cs** Section 10).
2. Call internal mutators (`WithdrawInternal` / `DepositInternal`) only while both locks are held; do not re-enter public methods that lock again on the same root (reentrancy saves you here, but the pattern is fragile).
3. Remove simulated I/O from inside the lock; validate outside or use a short critical section.
4. Optionally use `Monitor.TryEnter` with timeout and retry/backoff when ordering cannot be guaranteed.

```csharp
var first  = string.CompareOrdinal(from.AccountId, to.AccountId) <= 0 ? from : to;
var second = ReferenceEquals(first, from) ? to : from;
lock (first.SyncRoot) {
    lock (second.SyncRoot) {
        if (from.Balance >= amount) { from.WithdrawInternal(amount); to.DepositInternal(amount); }
    }
}
```


---
## Q3. (R) A developer "async-ified" a cache warmer registered as a **Singleton** in ASP.NET Core. The app compiles in some branches but stalls request threads under traffic. Review:

```csharp
public sealed class RateCacheWarmer
{
    private readonly object _sync = new();
    private readonly Dictionary<string, decimal> _rates = new();
    private readonly HttpClient _http = new();

    public async Task RefreshAsync(string productCode, CancellationToken ct)
    {
        lock (_sync)
        {
            var json = _http.GetStringAsync($"/rates/{productCode}", ct).Result;
            _rates[productCode] = ParseRate(json);
        }
    }
}
```

What are the problems (compile-time where applicable, runtime, and scalability), and how do you fix them in priority order?

**Concepts**
- lock statement with await
- SemaphoreSlim.WaitAsync async lock
- cannot await inside lock
- singleton async gate
- lock vs SemaphoreSlim(1,1) in async

**Answer:** `RefreshAsync` is async in name only — it blocks a thread inside `lock` via `.Result` on `GetStringAsync`, which can deadlock on ASP.NET's sync context and always starves the thread pool. Holding `lock` during network I/O serializes all refreshes and blocks other readers.


**Fix (priority order):**

1. Remove `.Result` — `await _http.GetStringAsync(...)` **outside** any lock; only lock for the dictionary write (or use `ConcurrentDictionary` from ch.07).
2. Inject `IHttpClientFactory` and create clients via `CreateClient("rates")`.
3. Use `SemaphoreSlim` (not `lock`) if you need to limit concurrent refreshes — `await gate.WaitAsync(ct)` is async-safe.
4. Consider double-checked locking with versioned snapshot replace instead of locking around HTTP.

```csharp
var json = await _http.GetStringAsync($"/rates/{productCode}", ct).ConfigureAwait(false);
var rate = ParseRate(json);
lock (_sync) { _rates[productCode] = rate; }
```


---
## Q4. (R) A read-heavy interest-rate API uses `ReaderWriterLockSlim` like the chapter tutorial. The first request for a missing product code freezes the entire rate service. Review:

```csharp
public decimal GetOrAddRate(string productCode)
{
    _rwLock.EnterReadLock();
    try
    {
        if (!_rates.ContainsKey(productCode))
        {
            _rwLock.EnterWriteLock();
            try
            {
                _rates[productCode] = LoadDefaultFromConfig(productCode);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        return _rates[productCode];
    }
    finally
    {
        _rwLock.ExitReadLock();
    }
}
```

What breaks, and what is the correct locking pattern for lazy insert under concurrent readers?

**Concepts**
- ReaderWriterLockSlim upgrade deadlock
- EnterUpgradeableReadLock
- nested lock upgrade hang
- UpgradeableRead must be acquired first
- read/upgrade/write lock ordering

**Answer:** The code calls `EnterWriteLock` while already holding `EnterReadLock` on the same `ReaderWriterLockSlim`. That lock type is not upgradeable — the thread blocks forever waiting for itself to release the read lock.


**Fix (priority order):**

1. Release read lock before taking write lock — exit read, enter write, re-check, insert, exit write, re-enter read for return (classic double-checked locking).
2. Or enter write lock directly when miss is likely; keep read lock only for the happy path (`GetRate` in **Program.cs** Section 7 shows the read-only path).
3. For ASP.NET Core read-heavy caches, consider immutable snapshot replace or `ConcurrentDictionary.GetOrAdd` (ch.07) to avoid manual RW lock upgrade entirely.

```csharp
_rwLock.EnterReadLock();
try {
    if (_rates.TryGetValue(productCode, out var rate)) return rate;
} finally { _rwLock.ExitReadLock(); }

_rwLock.EnterWriteLock();
try {
    if (!_rates.ContainsKey(productCode))
        _rates[productCode] = LoadDefaultFromConfig(productCode);
    return _rates[productCode];
} finally { _rwLock.ExitWriteLock(); }
```


---
## Q5. (P) An outbound API integration must allow at most **50 concurrent HTTP calls** cluster-wide per process, record a global request counter for metrics, and support cooperative shutdown of a background poller. Which synchronization primitives do you use for each concern, and what breaks if you use `lock` for all three?

**Concepts**
- SemaphoreSlim for 50 concurrent calls
- Interlocked for global counter
- CancellationToken for poller shutdown
- lock for all three breaks async
- right primitive for each concern

**Answer:** Use `SemaphoreSlim(50, 50)` with `WaitAsync`/`Release` for outbound throttling, `Interlocked.Increment` (or `Interlocked.Read` patterns) for the metrics counter, and `volatile bool` or `CancellationToken` for cooperative shutdown. Using `lock` for all three serializes HTTP concurrency to one call at a time and blocks async waits inside the lock.

- **Concurrency cap (50 calls):** `SemaphoreSlim` — counting semaphore matches "N at a time" (Section 6). `WaitAsync` avoids blocking thread-pool threads during I/O.
- **Global request counter:** `Interlocked.Increment(ref _totalRequests)` — single-field atomic math without lock overhead (Section 8).
- **Cooperative shutdown:** `CancellationTokenSource.Cancel()` linked to host shutdown, or `volatile bool _stopRequested` checked in the poller loop (Section 9).

**What breaks with `lock` everywhere:**

- Throttling under `lock` during `await` — cannot await inside `lock`; you'd block one thread per wait, defeating parallelism and risking deadlocks.
- Counter under `lock` works but adds contention on every metric tick; unnecessary when `Interlocked` suffices.
- Stop flag under `lock` on every loop iteration adds latency; visibility is solved by `volatile` or `CancellationToken` without serializing the loop.


---
## Q6. (M) A nightly vault-scan worker runs on a dedicated thread. Operators click "Stop" in a WinForms-style host; locally it often exits, but on release builds in production the thread keeps running until the process is killed. Review:

```csharp
public sealed class VaultScanWorker
{
    private bool _stopRequested;

    public void Run(CancellationToken externalToken)
    {
        while (!externalToken.IsCancellationRequested && !_stopRequested)
        {
            ScanNextBatch();
            Thread.Sleep(40);
        }
    }

    public void RequestStop()
    {
        _stopRequested = true;
    }
}
```

Why does `_stopRequested` fail to stop the loop reliably across CPU cores, and what is the production-safe fix?

**Concepts**
- volatile without memory barrier sufficient
- release-build optimizer reorders
- volatile keyword for flag fields
- JIT compiler and CPU reorder
- Memory.Fence or Interlocked needed

**Answer:** `_stopRequested` is a plain `bool` without `volatile` or synchronization. The JIT/CPU may cache the field in a register on the worker core, so writes from the UI thread are not guaranteed visible — the loop never observes `true`. This is the visibility problem **Program.cs** Section 9 demonstrates with `volatile bool _stopRequested`.


**Fix (priority order):**

1. Mark `_stopRequested` as `volatile bool` **or** prefer checking `externalToken.IsCancellationRequested` only and call `RequestStop` via `CancellationTokenSource.Cancel()`.
2. Wire host shutdown to cancel the same token passed to `Run`.
3. Do not use `Thread.Abort` — cooperative exit only.
4. For complex state, use `lock` around flag read/write or `Interlocked.Exchange` — overkill for a simple stop bit but valid.

```csharp
private volatile bool _stopRequested;

public void RequestStop() => _stopRequested = true;
// Better: inject CancellationToken and drop the bool entirely.
```


---
## Q7. (D) Two designs protect a singleton in-memory fee schedule updated once per hour and read on every pricing request:

**A.** `Dictionary<string, decimal>` + `ReaderWriterLockSlim` (manual read/write locks)  
**B.** `ConcurrentDictionary<string, decimal>` with snapshot replace on refresh

Which do you ship for a read-heavy ASP.NET Core pricing API, and what trade-offs drive the choice?

---

### 07. Concurrent Collections


**Answer:** Ship **Option B** (`ConcurrentDictionary` with snapshot replace on refresh) for a read-heavy ASP.NET Core pricing API. Readers never block writers except briefly during reference swap; no manual RW lock upgrade risk; scales with concurrent pricing requests.


**Trade-offs:**

- Choose **A** when refresh mutates entries in place and you need fine-grained per-key updates with strong in-process RW semantics and moderate traffic.
- Choose **B** when updates are batch/hourly and reads dominate — copy-on-write avoids long write locks and matches options-pattern snapshot refresh.
- Either way: do not expose mutable `Dictionary` without synchronization; document that pricing reads see eventually consistent fees for one refresh window.


---

### 07. Concurrent Collections




---
## Q1. (R) A warehouse API records parallel pick confirmations into shared stock counts. Under load, inventory drifts negative even though each sale is valid. Review this service method:

```csharp
public sealed class StockLedger
{
    private readonly ConcurrentDictionary<string, int> _onHand = new();

    public void ApplyPick(string sku, int quantity)
    {
        if (_onHand.TryGetValue(sku, out int current))
            _onHand[sku] = current - quantity;
        else
            _onHand.TryAdd(sku, -quantity);
    }

    public int GetOnHand(string sku) =>
        _onHand.TryGetValue(sku, out int v) ? v : 0;
}
```

What fails under concurrent picks on the same SKU, and how do you fix it without wrapping every call in `lock`?

**Concepts**
- ConcurrentDictionary parallel stock drift
- AddOrUpdate non-atomic check-then-act
- GetOrAdd factory race
- Interlocked for numeric delta
- atomic update with TryUpdate

**Answer:** `ApplyPick` performs read-modify-write with separate `TryGetValue` and indexer assignment — not atomic on `ConcurrentDictionary`. Two threads can read the same `current`, both subtract, and one update is lost. `ConcurrentDictionary` makes single operations thread-safe, not compound sequences.


**Fix (priority order):**

1. Replace the sequence with `AddOrUpdate` — atomic insert or transform in one call (see **Program.cs** Section 2).
2. Or use `TryUpdate` in a retry loop with compare-and-swap semantics until success.
3. Enforce business rule (non-negative stock) inside the update delegate or after atomic update with validation/retry.
4. Reserve `lock` only when multiple collections or fields must change together (ch.06).

```csharp
public void ApplyPick(string sku, int quantity) =>
    _onHand.AddOrUpdate(
        sku,
        _ => -quantity,
        (_, current) => current - quantity);
```


---
## Q2. (R) A catalog microservice caches product rows in `ConcurrentDictionary` to cut database round-trips. After a traffic spike, ops sees duplicate `LoadProduct` calls and inflated cache-miss metrics for the same SKU. Review:

```csharp
public sealed class ProductCache
{
    private readonly ConcurrentDictionary<string, Product> _cache = new();
    private readonly IProductRepository _repo;
    private readonly IMetrics _metrics;

    public Product Get(string sku) =>
        _cache.GetOrAdd(sku, key =>
        {
            _metrics.Increment("catalog.cache_miss");
            var row = _repo.LoadProduct(key);   // ~40 ms I/O
            return row;
        });
}
```

What concurrent-collection behavior causes duplicate work, and what pattern keeps the factory side-effect safe?

**Concepts**
- GetOrAdd factory race condition
- Lazy<T> wrapped in GetOrAdd
- double initialization risk
- TryAdd then TryGetValue pattern
- cache stampede for missing key

**Answer:** Under contention, `GetOrAdd` may invoke the factory delegate multiple times for the same key — only one result is stored, but every invocation runs. Side effects (`LoadProduct`, metric increment) are not deduplicated.


**Fix (priority order):**

1. Move side effects out of the factory — factory returns value only; increment metrics after `GetOrAdd` returns if this thread's value was the one stored (hard to detect) **or** use explicit double-check with `SemaphoreSlim` per key / `Lazy<Task<Product>>` per key.
2. Preferred pattern: `GetOrAdd(key, _ => new Lazy<Task<Product>>(() => LoadAsync(key)))` then `await lazy.Value` — one factory constructs the `Lazy`, one `LoadProduct` per key.
3. Or use `IMemoryCache.GetOrCreateAsync` with built-in stampede protection in ASP.NET Core.
4. Document that factory must be idempotent and cheap if you keep raw `GetOrAdd`.

```csharp
var lazy = _cache.GetOrAdd(sku, k => new Lazy<Product>(() => _repo.LoadProduct(k)));
var product = lazy.Value;
```


---
## Q3. (R) A nightly batch job ships orders through a bounded in-memory buffer. Locally it finishes; in production the job hangs until the host kills the process. Review the pipeline:

```csharp
public async Task RunBatchAsync(CancellationToken ct)
{
    var buffer = new BlockingCollection<Order>(boundedCapacity: 200);

    var ingest = Task.Run(() =>
    {
        foreach (Order order in _repo.FetchPendingOrders())
            buffer.Add(order, ct);
        // producer loop ends here — no CompleteAdding()
    }, ct);

    var ship = Task.Run(() =>
    {
        foreach (Order order in buffer.GetConsumingEnumerable(ct))
            _shipper.Dispatch(order);
    }, ct);

    await Task.WhenAll(ingest, ship);
}
```

What keeps `GetConsumingEnumerable` from terminating, and what else should you verify for graceful shutdown under cancellation?

**Concepts**
- BlockingCollection CompleteAdding never called
- GetConsumingEnumerable hangs
- producer exits without completing
- CompleteAdding in finally
- bounded buffer deadlock prevention

**Answer:** The producer never calls `CompleteAdding()`, so `GetConsumingEnumerable` waits forever for more items even after `FetchPendingOrders` finishes. The consumer never exits; `Task.WhenAll` blocks indefinitely.


**Fix (priority order):**

1. Call `buffer.CompleteAdding()` in a `finally` after the producer loop — matches **Program.cs** Section 6 (`PickBufferDemo`).
2. Use `try/finally` on producer so completion fires even on exception; log and rethrow or surface failure.
3. Pass `CancellationToken` to both `GetConsumingEnumerable(ct)` and producer; on cancel, complete adding if not already done.
4. Await both tasks; consider `Task.WhenAll` with timeout for ops visibility.

```csharp
try {
    foreach (var order in _repo.FetchPendingOrders())
        buffer.Add(order, ct);
} finally {
    buffer.CompleteAdding();
}
```


---
## Q4. (R) Support tickets must be processed first-in, first-out. A developer chose `ConcurrentBag` because "it's built for parallel workers." Review the dispatcher:

```csharp
public sealed class TicketDispatcher
{
    private readonly ConcurrentBag<SupportTicket> _inbox = new();

    public void Enqueue(SupportTicket ticket) => _inbox.Add(ticket);

    public bool TryDispatchNext(out SupportTicket ticket)
    {
        return _inbox.TryTake(out ticket);
    }

    public int PendingCount => _inbox.Count;
}
```

What ordering guarantees does this give in production, and which concurrent type fits FIFO fairness?

**Concepts**
- ConcurrentBag ordering not FIFO
- ConcurrentQueue for FIFO order
- thread-local steal semantics
- wrong collection for ordered dispatch
- FIFO vs LIFO vs unordered choice

**Answer:** `ConcurrentBag` provides no global FIFO ordering — it uses thread-local lists and `TryTake` prefers items from the calling thread's partition. Ticket order becomes undefined; SLA and fairness break even though `TryTake` "works."


**Fix (priority order):**

1. Replace with `ConcurrentQueue<SupportTicket>` — `Enqueue` / `TryDequeue` preserves FIFO (Section 3).
2. If multiple consumers need blocking when empty, wrap in `BlockingCollection<SupportTicket>` with bounded capacity for back-pressure.
3. Keep `ConcurrentBag` only when order is irrelevant (error aggregation from `Parallel.ForEach`).
4. For priority tiers, use separate queues or a priority queue with appropriate synchronization — not a bag.


---
## Q5. (P) A log-ingestion service has 50 HTTP producers and 4 background writers. An unbounded `ConcurrentQueue<LogEntry>` caused an OOM during a burst. How would you redesign the buffer using types from this chapter, and what breaks if you skip back-pressure?

**Concepts**
- bounded BlockingCollection back-pressure
- BoundedCapacity OOM prevention
- Channel<T> as modern alternative
- producer blocks when full
- consumer drain keeps memory bounded

**Answer:** Replace the unbounded queue with `BlockingCollection<LogEntry>` backed by `ConcurrentQueue`, set `boundedCapacity` to match writer throughput and memory budget (e.g. 5,000–20,000 entries), and have producers use `TryAdd` with timeout or `Add` with cancellation when full. Writers drain via `GetConsumingEnumerable`; producers call `CompleteAdding()` on shutdown.

- **Bounded buffer:** `new BlockingCollection<LogEntry>(new ConcurrentQueue<LogEntry>(), boundedCapacity: 10_000)` — producers block or fail when full instead of allocating without limit (Section 6).
- **Writers:** fixed pool of 4 tasks consuming `GetConsumingEnumerable(ct)` — batch flush to disk/network.
- **Producers:** on burst, `TryAdd` returns false → drop with metric, sample, or spill to disk — explicit policy beats OOM.
- **Shutdown:** `CompleteAdding()` when ingress stops; writers drain and exit.

**What breaks without back-pressure:**

- Unbounded `ConcurrentQueue` grows until gen2 LOH pressure and OOM kill the process.
- Silent latency growth — queue depth rises, log delivery lags minutes behind real time.
- GC pauses spike under sustained producer > consumer mismatch.


---
## Q6. (D) Two approaches for collecting validation errors from `Parallel.ForEach` over 10,000 CSV rows:

**Option A — lock + List**

```csharp
var errors = new List<string>();
var gate = new object();
Parallel.ForEach(rows, row =>
{
    if (!Validate(row, out string msg))
        lock (gate) { errors.Add(msg); }
});
```

**Option B — ConcurrentBag**

```csharp
var errors = new ConcurrentBag<string>();
Parallel.ForEach(rows, row =>
{
    if (!Validate(row, out string msg))
        errors.Add(msg);
});
```

When is each appropriate, and what must you do before returning errors to the API client?

**Concepts**
- ConcurrentBag for parallel collection
- lock plus List pitfall
- ConcurrentQueue for ordered errors
- per-thread bag merge after loop
- concurrent collection choice for parallel aggregation

**Answer:** Use **Option B (`ConcurrentBag`)** for parallel error collection when order does not matter — avoids serializing every `Add` on a global lock. Use **Option A (lock + List)** when you need deterministic ordering, deduplication, or a single sorted merge with other state under one invariant.


**Before returning to API client:**

1. Copy bag to array (`errors.ToArray()` or `ToList()`) — do not enumerate live bag while workers still add unless complete.
2. Sort or dedupe if client expects stable ordering — `OrderBy` on row number if error carries line index.
3. Cap response size — if errors exceed limit, return summary + truncated list with total count.
4. Do not return the mutable bag directly — snapshot first (Section 5 pattern).


---
## Q7. (M) During peak picking, a dashboard polls `ConcurrentDictionary` for a live inventory report:

```csharp
public IReadOnlyList<StockRow> GetLiveReport()
{
    var rows = new List<StockRow>();
    foreach (var kv in _onHand)   // concurrent adds/updates still running
    {
        rows.Add(new StockRow(kv.Key, kv.Value));
    }
    rows.Sort((a, b) => string.Compare(a.Sku, b.Sku, StringComparison.Ordinal));
    return rows;
}
```

What snapshot semantics does enumeration provide under mutation, and when is `Count` or a copied snapshot misleading for ops dashboards?


**Concepts**
- ConcurrentDictionary LINQ snapshot inconsistency
- ToArray before LINQ query
- Count vs enumerated items race
- weakly consistent enumeration
- snapshot for consistent reporting

**Answer:** Enumeration over `ConcurrentDictionary` while mutators run yields a weakly consistent snapshot — you may miss concurrent adds, see duplicate keys is impossible, but values can change mid-enumeration. `Count` during heavy mutation is approximate and can disagree with the number of entries you enumerate. Sorting during live enumeration produces a report that was never true at any single instant.

- **Snapshot semantics:** Foreach is safe (no `InvalidOperationException`) but not a point-in-time photograph — documented weak consistency.
- **Misleading `Count`:** Can differ from `rows.Count` after loop — do not use for reconciliation dashboards without copying first.
- **Sort mid-mutation:** Order reflects values observed at different times — ops may see phantom shortages.

**When to copy:**

- Financial or ops reconciliation → `ToArray()` or `Select(...).ToList()` under a defined policy, or pause writers briefly.
- Live dashboard OK with "approximate live" → document lag; refresh on interval; prefer `OrderBy` on copied snapshot.
- High-stakes inventory → version counter (`Interlocked`) incremented on each batch publish; report includes version stamp.

```csharp
var snapshot = _onHand.ToArray();
var rows = snapshot
    .Select(kv => new StockRow(kv.Key, kv.Value))
    .OrderBy(r => r.Sku, StringComparer.Ordinal)
    .ToList();
```


---
