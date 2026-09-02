# Tasks & Task Parallel Library — Interview Q&A


## Table of Contents

1. [Q1. What is a Task in C# and how does it differ from a Thread?](#q1-what-is-a-task-in-c-and-how-does-it-differ-from-a-thread)
2. [Q2. What are the different ways to create and start a Task?](#q2-what-are-the-different-ways-to-create-and-start-a-task)
3. [Q3. What are the TaskStatus values and when does each occur?](#q3-what-are-the-taskstatus-values-and-when-does-each-occur)
4. [Q4. How does Task.ContinueWith work and what are its options?](#q4-how-does-taskcontinuewith-work-and-what-are-its-options)
5. [Q5. What is the difference between Task.WhenAll and Task.WaitAll?](#q5-what-is-the-difference-between-taskwhenall-and-taskwaitall)
6. [Q6. How does Task.WhenAny work and when is it useful?](#q6-how-does-taskwhenany-work-and-when-is-it-useful)
7. [Q7. What is TaskCompletionSource<T> and when would you use it?](#q7-what-is-taskcompletionsourcet-and-when-would-you-use-it)
8. [Q8. What is ValueTask<T> and when should you prefer it over Task<T>?](#q8-what-is-valuetaskt-and-when-should-you-prefer-it-over-taskt)
9. [Q9. How does AggregateException work and how do you unwrap it?](#q9-how-does-aggregateexception-work-and-how-do-you-unwrap-it)
10. [Q10. What are parent-child task relationships and when are they used?](#q10-what-are-parent-child-task-relationships-and-when-are-they-used)
11. [Q11. What is the difference between Task.Run and Task.Factory.StartNew?](#q11-what-is-the-difference-between-taskrun-and-taskfactorystartnew)
12. [Q12. How does task cancellation with CancellationToken work?](#q12-how-does-task-cancellation-with-cancellationtoken-work)
13. [Q13. What is the difference between accessing Task.Result vs awaiting a Task?](#q13-what-is-the-difference-between-accessing-taskresult-vs-awaiting-a-task)
14. [Q14. How do you implement a task timeout pattern?](#q14-how-do-you-implement-a-task-timeout-pattern)
15. [Q15. What is the significance of TaskScheduler and when would you use a custom one?](#q15-what-is-the-significance-of-taskscheduler-and-when-would-you-use-a-custom-one)
16. [Q16. What is the async void anti-pattern and why is it dangerous?](#q16-what-is-the-async-void-anti-pattern-and-why-is-it-dangerous)
17. [Q17. What is the unobserved task exception problem?](#q17-what-is-the-unobserved-task-exception-problem)
18. [Q18. What causes a deadlock when using .Result on an async method in WPF or ASP.NET?](#q18-what-causes-a-deadlock-when-using-result-on-an-async-method-in-wpf-or-aspnet)
19. [Q19. What is the ValueTask double-await bug?](#q19-what-is-the-valuetask-double-await-bug)
20. [Q20. What is the fire-and-forget task leak pattern?](#q20-what-is-the-fire-and-forget-task-leak-pattern)
21. [Q21. How do you wrap an old EAP (Event-based Asynchronous Pattern) API with TaskCompletionSource?](#q21-how-do-you-wrap-an-old-eap-event-based-asynchronous-pattern-api-with-taskcompletionsource)
22. [Q22. Implement a fan-out/fan-in pattern to fetch data from 5 microservices in parallel with a timeout.](#q22-implement-a-fan-outfan-in-pattern-to-fetch-data-from-5-microservices-in-parallel-with-a-timeout)
23. [Q23. How do you implement an async retry policy with exponential backoff?](#q23-how-do-you-implement-an-async-retry-policy-with-exponential-backoff)
24. [Q24. You have a service that fires 100 tasks and some fail. How do you collect all results and errors?](#q24-you-have-a-service-that-fires-100-tasks-and-some-fail-how-do-you-collect-all-results-and-errors)
25. [Q25. How would you build a simple async circuit breaker using Task?](#q25-how-would-you-build-a-simple-async-circuit-breaker-using-task)
26. [Q26. How do you diagnose a deadlock caused by .Result in an async chain?](#q26-how-do-you-diagnose-a-deadlock-caused-by-result-in-an-async-chain)
27. [Q27. How do you choose between Task and ValueTask for a high-performance cache read method?](#q27-how-do-you-choose-between-task-and-valuetask-for-a-high-performance-cache-read-method)

---
## Foundation Questions

---

## Q1. What is a Task in C# and how does it differ from a Thread?

**Concepts**
- Task: unit of work (logical abstraction)
- Thread: OS execution resource (physical)
- Task runs on ThreadPool by default
- Task composable: awaitable, continuations, cancellation
- Thread is heavyweight; Task is lightweight

**Answer**

A `Thread` is an OS resource — it has its own stack, scheduling state, and kernel object. Creating one costs milliseconds and megabytes. A `Task` is a logical unit of work that describes something to be done and how to observe its completion. It does not inherently own a thread; by default, `Task.Run` queues the work on the ThreadPool, reusing an existing thread.

The practical differences: tasks are composable — you can await them, chain continuations, combine multiple tasks with `Task.WhenAll`/`Task.WhenAny`, and cancel them with `CancellationToken`. Threads lack all of these directly. Tasks capture exceptions and expose them through the `Task.Exception` property or re-throw them on `await`. Threads crash the process if an exception goes unhandled. Tasks integrate with the `async/await` machinery of the C# compiler; threads do not.

Use `Task.Run` for almost all concurrent work in modern C#. Reserve `new Thread()` for the rare cases where you need STA apartment state, a custom stack size, or a truly long-running dedicated thread.

---

## Q2. What are the different ways to create and start a Task?

**Concepts**
- Task.Run (most common — pool thread, deny child attach)
- Task.Factory.StartNew (more options, legacy)
- new Task(...).Start() (rarely used)
- TaskCompletionSource<T> for manual completion
- Task.FromResult / Task.CompletedTask for pre-completed tasks

**Answer**

`Task.Run(action)` is the modern idiomatic way to run a delegate on a ThreadPool thread. It is equivalent to `Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default)` — the `DenyChildAttach` is important because it prevents accidental parent-child task relationships.

`Task.Factory.StartNew` provides more options: `TaskCreationOptions.LongRunning` for a dedicated thread, `AttachedToParent` for hierarchical tasks, and custom `TaskScheduler`. It is generally only needed when those options are required.

`new Task(action).Start()` is the two-step approach, useful when you need to configure the task before starting it. `TaskCompletionSource<T>` creates a task whose completion you control explicitly — essential for wrapping callback-based APIs. `Task.FromResult(value)` and `Task.CompletedTask` return already-completed tasks synchronously, avoiding allocations in hot paths. `Task.FromException(ex)` and `Task.FromCanceled(token)` similarly create pre-faulted or pre-cancelled tasks.

---

## Q3. What are the TaskStatus values and when does each occur?

**Concepts**
- Created, WaitingForActivation, WaitingToRun, Running
- WaitingForChildrenToComplete
- RanToCompletion, Canceled, Faulted
- IsCompleted, IsCompletedSuccessfully, IsFaulted, IsCanceled shorthand properties
- Terminal states: RanToCompletion, Canceled, Faulted

**Answer**

`TaskStatus` is an enum tracking the task's lifecycle. `Created` means the task was constructed with `new Task()` but `Start()` has not been called. `WaitingForActivation` is the state for promise-style tasks (from `TaskCompletionSource` or async method machinery) that are waiting for a signal. `WaitingToRun` means the task is queued on a scheduler. `Running` means a thread is executing the delegate. `WaitingForChildrenToComplete` occurs when a parent task has finished its own body but attached child tasks are still running.

Terminal states: `RanToCompletion` (success), `Canceled` (cancelled before or during execution), `Faulted` (exception thrown). Once a task reaches a terminal state, it never transitions again. Accessing `Task.Result` on a faulted task throws `AggregateException`; awaiting it unwraps to the inner exception.

In modern C#, the convenience properties `IsCompleted`, `IsCompletedSuccessfully`, `IsFaulted`, and `IsCanceled` are clearer than comparing `TaskStatus` directly. `IsCompleted` is `true` for all three terminal states.

---

## Q4. How does Task.ContinueWith work and what are its options?

**Concepts**
- Schedules a delegate to run when antecedent completes
- TaskContinuationOptions flags (OnlyOnRanToCompletion, OnlyOnFaulted, etc.)
- Accesses antecedent result via Task<T> parameter
- ExecuteSynchronously option for same-thread execution
- Superceded by async/await for most use cases

**Answer**

`ContinueWith` schedules a callback to execute when the preceding task completes. The callback receives the antecedent task as a parameter, allowing inspection of its result or exception:

```csharp
var task = Task.Run(() => ComputeValue());
var continuation = task.ContinueWith(t =>
{
    if (t.IsFaulted) HandleError(t.Exception!.InnerException);
    else UseResult(t.Result);
});
```

`TaskContinuationOptions` controls when the continuation runs: `OnlyOnRanToCompletion`, `OnlyOnFaulted`, `OnlyOnCanceled`, and their negations. `NotOnCanceled | NotOnFaulted` means the continuation only runs on success. `ExecuteSynchronously` hints that the continuation should run on the same thread as the antecedent — useful for short callbacks that avoid a thread switch, though it is a hint, not a guarantee.

In modern C#, `async/await` replaces most `ContinueWith` usage because it is clearer, handles exceptions naturally, and does not require understanding the nuances of continuation options. `ContinueWith` remains useful for dynamic pipeline construction where the number of stages is not known at compile time.

---

## Q5. What is the difference between Task.WhenAll and Task.WaitAll?

**Concepts**
- WhenAll: async, returns Task — does not block calling thread
- WaitAll: synchronous, blocks calling thread
- WhenAll aggregates all exceptions; WaitAll throws AggregateException
- WhenAll(Task<T>[]) returns Task<T[]> with all results
- WaitAll with timeout overload

**Answer**

`Task.WhenAll(tasks)` returns a new `Task` that completes when all the input tasks complete. It is non-blocking — the calling thread is freed when you `await` it. `Task.WaitAll(tasks)` blocks the calling thread synchronously until all tasks complete. In async code, `WaitAll` should never be used because it holds a ThreadPool thread during the entire wait, causing potential starvation.

`Task.WhenAll` runs all tasks to completion even if some fail — it collects all exceptions. When awaited, if any task faulted, it throws the first exception; to see all exceptions, inspect `WhenAll(...).Exception.InnerExceptions`. The typed overload `Task.WhenAll(IEnumerable<Task<T>>)` returns `Task<T[]>`, giving all results in order.

```csharp
var tasks = urls.Select(url => httpClient.GetStringAsync(url));
string[] results = await Task.WhenAll(tasks); // all run in parallel
```

For the rare case where you need synchronous all-completion waits in non-async code (console app main thread, test setup), `Task.WaitAll` is acceptable but should be accompanied by a comment explaining why `await` is not used.

---

## Q6. How does Task.WhenAny work and when is it useful?

**Concepts**
- Returns Task<Task> — the first task to complete
- Does not cancel other tasks
- Timeout pattern with Task.Delay
- Polling pattern without busy-wait
- Must inspect the winning task for exceptions

**Answer**

`Task.WhenAny(tasks)` returns a `Task<Task>` that completes as soon as the first of the input tasks completes. The inner task is the one that completed first. The remaining tasks continue running — `WhenAny` does not cancel them.

The canonical use case is implementing a timeout:

```csharp
var workTask = DoWorkAsync(cancellationToken);
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

var winner = await Task.WhenAny(workTask, timeoutTask);
if (winner == timeoutTask)
    throw new TimeoutException("Work did not complete within 10 seconds");

// Await the actual task to propagate exceptions:
await workTask;
```

You must await the winning task separately to propagate exceptions — `WhenAny` itself will not throw if the winning task is faulted. Another use: "first response wins" — fire multiple requests to redundant services and use whichever responds first. Be careful to cancel the losing tasks to avoid resource waste.

---

## Q7. What is TaskCompletionSource<T> and when would you use it?

**Concepts**
- Manual task completion control
- Wrapping callback-based (EAP/APM) APIs
- SetResult, SetException, SetCanceled
- TrySetResult for thread-safe multiple-setter scenarios
- RunContinuationsAsynchronously option

**Answer**

`TaskCompletionSource<T>` creates a `Task<T>` whose completion is controlled by code rather than by executing a delegate. It bridges the gap between callback-based patterns and task-based async:

```csharp
public Task<string> ReadLineAsync(Stream stream)
{
    var tcs = new TaskCompletionSource<string>(
        TaskCreationOptions.RunContinuationsAsynchronously);

    stream.BeginRead(buffer, 0, buffer.Length, ar =>
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            tcs.SetResult(Encoding.UTF8.GetString(buffer, 0, bytesRead));
        }
        catch (Exception ex) { tcs.SetException(ex); }
    }, null);

    return tcs.Task;
}
```

`RunContinuationsAsynchronously` is important: without it, calling `SetResult` synchronously on a `TaskCompletionSource` runs continuations inline on the thread that called `SetResult`, which can cause unexpected behavior or stack overflows in deep chains. Always use this option unless you explicitly need synchronous continuation execution.

Use `TrySetResult`/`TrySetException` (which return `bool`) instead of the `Set*` variants when multiple code paths might try to complete the same TCS — only one can succeed, and the `Try` variants avoid `InvalidOperationException`.

---

## Q8. What is ValueTask<T> and when should you prefer it over Task<T>?

**Concepts**
- Struct wrapper — no heap allocation when result is synchronous
- Task<T>: always allocates on heap
- ValueTask<T>: zero-allocation for hot synchronous path
- Cannot be awaited more than once
- Cannot be safely blocked on with .Result

**Answer**

`Task<T>` is a class — every `Task.Run` or async method that returns `Task<T>` allocates a heap object. For hot-path methods that frequently complete synchronously (e.g., a cache read that hits 99% of the time), this allocation is measurable overhead. `ValueTask<T>` is a struct that can hold either a synchronous result or a `Task<T>`, paying the allocation cost only when the result is genuinely asynchronous.

```csharp
// Zero allocation when item is in cache
public async ValueTask<string> GetAsync(string key)
{
    if (_cache.TryGetValue(key, out string? value))
        return value; // no allocation — stored in struct directly

    value = await FetchFromDatabaseAsync(key); // allocates Task here
    _cache[key] = value;
    return value;
}
```

Critical restrictions: a `ValueTask<T>` must be awaited exactly once. Awaiting it multiple times, storing it in a variable and awaiting it later, or calling `.Result` are all bugs — the underlying `IValueTaskSource<T>` may have been returned to a pool. The rule: `await` a `ValueTask` exactly once, immediately. If you need to inspect the result multiple times, call `.AsTask()` to convert it to a regular `Task<T>` first.

---

## Q9. How does AggregateException work and how do you unwrap it?

**Concepts**
- Container for multiple exceptions from parallel operations
- InnerExceptions collection
- Flatten() for nested AggregateExceptions
- Handle(Func<Exception, bool>) for selective handling
- await unwraps first InnerException automatically

**Answer**

`AggregateException` is the exception type used to wrap multiple exceptions from parallel or concurrent operations. It contains an `InnerExceptions` read-only collection. It arises from `Task.WaitAll`, accessing `Task.Exception`, and `Parallel.For`/`ForEach`.

`Flatten()` is essential when `AggregateException` instances are nested (e.g., a task that contains tasks that failed): it produces a single-level `AggregateException` with all leaf exceptions. `Handle(predicate)` allows selective handling — the predicate returns `true` for exceptions you handled and `false` for those you want re-thrown; any unhandled exceptions are rethrown in a new `AggregateException`.

```csharp
try { Task.WaitAll(tasks); }
catch (AggregateException ae)
{
    ae.Flatten().Handle(ex =>
    {
        if (ex is TimeoutException) { LogTimeout(ex); return true; }
        return false; // re-throw everything else
    });
}
```

When you `await` a task, the C# compiler automatically unwraps `AggregateException` and throws the first `InnerException` directly. This is why `await` feels like normal exception handling with `try/catch`. To catch all exceptions from a `WhenAll`, use `Task.WhenAll(...).Exception.InnerExceptions` after checking `IsFaulted`, or use `WaitAll` with an explicit `AggregateException` catch.

---

## Q10. What are parent-child task relationships and when are they used?

**Concepts**
- TaskCreationOptions.AttachedToParent
- Parent task waits for attached children before completing
- Exception propagation from child to parent
- DenyChildAttach prevents accidental attachment
- Rarely used in modern async code

**Answer**

When a task is created with `TaskCreationOptions.AttachedToParent` from within another task's body, the parent task will not transition to a terminal state until all attached children complete. Exceptions from children propagate to the parent as nested `AggregateException` instances.

```csharp
var parent = Task.Factory.StartNew(() =>
{
    Task.Factory.StartNew(() => /* child 1 */,
        TaskCreationOptions.AttachedToParent);
    Task.Factory.StartNew(() => /* child 2 */,
        TaskCreationOptions.AttachedToParent);
});
await parent; // waits for both children automatically
```

`Task.Run` passes `TaskCreationOptions.DenyChildAttach` to prevent tasks created inside it from accidentally attaching — a safety measure because task nesting can create unexpected waiting and exception propagation behavior. You must use `Task.Factory.StartNew` explicitly to create attached children.

Parent-child relationships are rarely the right tool in modern code. `Task.WhenAll` for explicit waiting or structured concurrency patterns (like async streams) are clearer. The main use case is hierarchical work where propagating cancellation and exceptions through the hierarchy naturally is valuable.

---

## Q11. What is the difference between Task.Run and Task.Factory.StartNew?

**Concepts**
- Task.Run: convenience wrapper, safe defaults
- Task.Factory.StartNew: full control, more footguns
- DenyChildAttach in Task.Run
- Async lambda handling difference
- LongRunning option only available on StartNew

**Answer**

`Task.Run` is a safe, opinionated wrapper introduced in .NET 4.5. It always uses `TaskScheduler.Default`, always sets `DenyChildAttach`, and properly handles async lambdas — when you pass `async () => ...`, it returns `Task<Task>` and `Task.Run` correctly unwraps it to a single `Task`.

`Task.Factory.StartNew` is the low-level API with full control. The critical footgun: passing an async lambda creates a `Task<Task>` but the outer task completes when the async method reaches its first `await`, not when it finishes. Many developers accidentally ignore the inner task:

```csharp
// BUG: task completes at first await, not at method end
Task.Factory.StartNew(async () =>
{
    await Task.Delay(1000); // outer task completes here
    DoMoreWork(); // this might not run before caller proceeds
});

// CORRECT:
Task.Run(async () =>
{
    await Task.Delay(1000); // Task.Run unwraps the inner task
    DoMoreWork(); // this runs before the task completes
});
```

Use `Task.Factory.StartNew` only when you need `LongRunning`, a custom scheduler, or `AttachedToParent`. For all other cases, `Task.Run` is correct and safer.

---

## Q12. How does task cancellation with CancellationToken work?

**Concepts**
- CancellationTokenSource.Cancel() signals cancellation
- CancellationToken.ThrowIfCancellationRequested() for cooperative cancel
- Task enters Canceled state (not Faulted)
- OperationCanceledException distinguished from other exceptions
- Linking tokens with CreateLinkedTokenSource

**Answer**

Task cancellation in .NET is cooperative. The producer creates a `CancellationTokenSource` and passes `token` to the async method. The method periodically checks `token.ThrowIfCancellationRequested()` at safe stopping points. When `Cancel()` is called, the next check throws `OperationCanceledException`, which the runtime interprets as cancellation (not a fault), transitioning the task to the `Canceled` state rather than `Faulted`.

```csharp
var cts = new CancellationTokenSource();

var task = Task.Run(async () =>
{
    for (int i = 0; i < 100; i++)
    {
        cts.Token.ThrowIfCancellationRequested();
        await ProcessItemAsync(i, cts.Token);
    }
}, cts.Token);

cts.Cancel(); // signal cancellation
try { await task; }
catch (OperationCanceledException) { Console.WriteLine("Cancelled"); }
```

Pass the token to every async operation in the chain — most built-in APIs accept it. `CancellationTokenSource.CreateLinkedTokenSource(t1, t2)` creates a token that is cancelled when either parent is cancelled, useful for combining a per-request timeout with a process-wide shutdown token.

---

## Q13. What is the difference between accessing Task.Result vs awaiting a Task?

**Concepts**
- .Result blocks the calling thread synchronously
- await suspends without blocking
- .Result wraps exception in AggregateException; await unwraps it
- Deadlock risk: .Result in SynchronizationContext
- GetAwaiter().GetResult() as slightly better alternative to .Result

**Answer**

`task.Result` is a synchronous blocking property. It blocks the calling thread until the task completes and then returns the value. If the task faulted, it throws `AggregateException` wrapping the original exception. If the task was cancelled, it throws `AggregateException` wrapping `TaskCanceledException`.

`await task` suspends the current async method without blocking any thread, resumes on the appropriate context when the task completes, and re-throws the original exception type directly (unwrapping `AggregateException`).

The classic deadlock: in ASP.NET Classic or WPF/WinForms, the `SynchronizationContext` captures the UI/request thread. If an async method does `await` without `ConfigureAwait(false)`, the continuation is posted back to that context. If the context thread is blocked in `.Result`, the continuation can never execute — deadlock.

```csharp
// DEADLOCK in WPF/ASP.NET Classic:
var result = SomeAsync().Result; // blocks UI thread
// SomeAsync's continuation tries to post back to UI thread — deadlocked
```

`GetAwaiter().GetResult()` is slightly better than `.Result` because it throws the original exception directly (not wrapped), but it still blocks and can still deadlock. Truly fix the problem: use `await` throughout.

---

## Q14. How do you implement a task timeout pattern?

**Concepts**
- Task.WhenAny with Task.Delay
- CancellationTokenSource with CancelAfter
- Cancelling the work task on timeout
- Cleaning up the non-winning task
- TimeoutException vs OperationCanceledException

**Answer**

Two approaches: `Task.WhenAny` + `Task.Delay`, or `CancellationTokenSource.CancelAfter`. The `CancelAfter` approach is cleaner because it propagates cancellation to the work task rather than just timing out the wait:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
try
{
    var result = await DoWorkAsync(cts.Token);
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    throw new TimeoutException("Operation timed out after 10 seconds");
}
```

The `Task.WhenAny` approach is useful when you cannot pass a `CancellationToken` to the work:

```csharp
var workTask = DoWorkAsync();
var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));

if (await Task.WhenAny(workTask, timeoutTask) == timeoutTask)
    throw new TimeoutException();

await workTask; // re-await to propagate exceptions
```

Note: `workTask` continues running in the background after the timeout with `WhenAny` — it is not cancelled. This can be a resource leak. The `CancelAfter` approach is preferred because it cancels the work cooperatively.

---

## Q15. What is the significance of TaskScheduler and when would you use a custom one?

**Concepts**
- TaskScheduler: decides where tasks run
- TaskScheduler.Default: ThreadPool
- TaskScheduler.Current: ambient scheduler inside a task
- SynchronizationContextTaskScheduler for UI thread marshaling
- Custom scheduler for throttling, dedicated threads, etc.

**Answer**

`TaskScheduler` is the abstraction that decides how and where tasks execute. `TaskScheduler.Default` queues tasks on the ThreadPool. `TaskScheduler.FromCurrentSynchronizationContext()` creates a scheduler that posts tasks to the current `SynchronizationContext` — used in WPF/WinForms to marshal work back to the UI thread.

`TaskScheduler.Current` returns the scheduler of the currently running task; if called from outside a task, it returns `Default`. This is a subtle gotcha: `Task.Factory.StartNew` uses `TaskScheduler.Current`, not `Default`, by default. If called from within a UI-thread continuation, it inadvertently creates tasks that run on the UI thread.

Custom schedulers are rarely needed in modern code but are useful for: throttling concurrency (a limiting scheduler that queues excess tasks), running tasks on a dedicated thread (single-thread scheduler for STA COM objects), or priority queuing. Most concurrency limiting is done instead with `SemaphoreSlim` or `Channel<T>`, which do not require custom schedulers.

---

## Gotchas & Traps

---

## Q16. What is the async void anti-pattern and why is it dangerous?

**Concepts**
- async void: exceptions crash the process (not observable via Task)
- async Task: exceptions stored in Task, propagated on await
- async void only appropriate for event handlers
- Fire-and-forget with Task.Run instead
- TopLevelExceptionHandler for async void

**Answer**

```csharp
// DANGEROUS: async void
async void LoadDataAsync()
{
    await Task.Delay(100);
    throw new InvalidOperationException("silent crash");
}

button.Click += (s, e) => LoadDataAsync(); // exception terminates process

// SAFE: async Task
async Task LoadDataAsync()
{
    await Task.Delay(100);
    throw new InvalidOperationException("properly propagated");
}
```

| Category | Problem | Impact |
|---|---|---|
| Exception Safety | Exception escapes to SynchronizationContext, then crashes process | Undiagnosable process termination |
| Observability | Caller cannot await or catch exceptions | Silent failure |
| Lifecycle | Caller cannot know when work completes | Race conditions in tests |

**Fix priority:**
1. Change `async void` to `async Task` everywhere except event handlers.
2. In event handlers that must use `async void`, wrap the body in `try/catch` and handle all exceptions explicitly.
3. For fire-and-forget work, use `Task.Run(async () => { try { await Work(); } catch { Log(ex); } })`.

---

## Q17. What is the unobserved task exception problem?

**Concepts**
- Task that faults without being awaited
- UnobservedTaskException event
- CLR behavior changed between .NET 4.0 and 4.5
- Fire-and-forget tasks with no error observation
- GC-triggered event (non-deterministic)

**Answer**

If a `Task` faults (throws an exception) and nothing ever reads its `Exception` property or awaits it, the exception becomes "unobserved." In .NET 4.5+, unobserved exceptions do not crash the process by default (changed from 4.0 where they did). Instead, when the task is garbage-collected, `TaskScheduler.UnobservedTaskException` fires — but this is non-deterministic because GC timing is not predictable.

This means you can silently lose errors. A fire-and-forget task that throws an exception will never surface unless you subscribe to `UnobservedTaskException`.

```csharp
// Silently swallows exception
Task.Run(() => throw new InvalidOperationException("lost forever"));

// Safe fire-and-forget
_ = Task.Run(async () =>
{
    try { await DoWorkAsync(); }
    catch (Exception ex) { _logger.LogError(ex, "Background task failed"); }
});
```

Always observe task exceptions, either through `await`, `.ContinueWith(t => ..., OnlyOnFaulted)`, or a try/catch inside the task body. Subscribe to `TaskScheduler.UnobservedTaskException` as a safety net for logging any that slip through in tests.

---

## Q18. What causes a deadlock when using .Result on an async method in WPF or ASP.NET?

**Concepts**
- SynchronizationContext captures UI/request context
- async method continuation scheduled back to captured context
- .Result blocks the captured context's thread
- Continuation can never run — deadlock
- Fix: ConfigureAwait(false) or end-to-end async

**Answer**

```csharp
// DEADLOCK in WPF or ASP.NET Classic
public string GetData()
{
    return GetDataAsync().Result; // blocks UI thread
}

public async Task<string> GetDataAsync()
{
    // After this await, continuation is posted back to UI thread
    var data = await httpClient.GetStringAsync(url);
    return data;
}
```

| Category | Problem | Impact |
|---|---|---|
| Deadlock | `.Result` blocks UI thread; continuation tries to post back to blocked UI thread | Application hangs forever |
| Root Cause | SynchronizationContext captured at `await`; `.Result` holds that context thread | Classic deadlock |
| Hidden Risk | Works in console / ASP.NET Core (no SynchronizationContext) but fails in WPF / ASP.NET Classic | Environment-specific bug |

**Fix priority:**
1. Make the caller async: `public async Task<string> GetData()` and `await GetDataAsync()`.
2. If synchronous callers truly cannot be avoided, add `ConfigureAwait(false)` to every `await` inside `GetDataAsync` — this prevents the continuation from requiring the original context.
3. Never use `.Result` in code that runs on a `SynchronizationContext`-bound thread. Use `GetAwaiter().GetResult()` only in context-free code (console app main, background threads).

---

## Q19. What is the ValueTask double-await bug?

**Concepts**
- ValueTask may not be awaited more than once
- Underlying IValueTaskSource may recycle state
- Second await reads garbage or throws
- Convert to Task with .AsTask() for multiple consumption
- Compiler does not catch this bug

**Answer**

`ValueTask<T>` must be awaited exactly once. Some implementations use pooled `IValueTaskSource<T>` objects for efficiency — after the first `await` completes, the source is returned to a pool and may be reused for a completely different operation. A second `await` on the same `ValueTask<T>` then reads from a recycled, unrelated source, producing incorrect results or corrupting state.

```csharp
// BUG: ValueTask awaited twice
ValueTask<int> valueTask = GetValueAsync();
int first = await valueTask;  // ok
int second = await valueTask; // BUG: undefined behavior

// FIX: convert to Task first if you need multiple awaits
Task<int> task = GetValueAsync().AsTask();
int first = await task;
int second = await task; // safe - Task can be awaited multiple times
```

Similarly, storing a `ValueTask` and awaiting it later (after the method has had time to advance) is dangerous. The rule: capture the return value and await it immediately, in the same expression or the very next statement. If you need to await it multiple times, share results, or observe it from multiple places, call `.AsTask()` once and distribute the `Task<T>`.

---

## Q20. What is the fire-and-forget task leak pattern?

**Concepts**
- Task created but never awaited or observed
- Exceptions are silently lost
- Task lifecycle not tracked
- Background work without cancellation on shutdown
- Proper fire-and-forget alternatives

**Answer**

Fire-and-forget tasks — created with `Task.Run` or `_ = SomeAsync()` — are a common source of silent failures. If the task faults, the exception disappears. If the application shuts down, the task may be abruptly terminated mid-execution. If many fire-and-forget tasks accumulate, there is no way to wait for them during graceful shutdown.

```csharp
// LEAKY fire-and-forget
void OnEvent(Event e)
{
    Task.Run(() => HandleEventAsync(e)); // no awaiting, no error handling
}

// BETTER: tracked fire-and-forget with logging
void OnEvent(Event e)
{
    _ = HandleEventAsync(e).ContinueWith(
        t => _logger.LogError(t.Exception, "Event handling failed"),
        TaskContinuationOptions.OnlyOnFaulted);
}

// BEST: use a background task queue with lifecycle management
void OnEvent(Event e)
{
    _backgroundQueue.Enqueue(e); // IHostedService consumes the queue with CancellationToken
}
```

For production code, use a hosted background service (`IHostedService` + `BackgroundService`) or `Channel<T>` to manage fire-and-forget work. This provides lifecycle tracking, graceful shutdown, bounded concurrency, and error handling in one place.

---

## Real-World Scenarios

---

## Q21. How do you wrap an old EAP (Event-based Asynchronous Pattern) API with TaskCompletionSource?

**Concepts**
- EAP pattern: Begin/End or event + RunWorkerCompleted
- TaskCompletionSource as bridge
- SetResult on completion event
- SetException on error
- RunContinuationsAsynchronously to avoid inline execution

**Answer**

The Event-based Asynchronous Pattern uses completion events and callbacks rather than returning tasks. `TaskCompletionSource<T>` bridges these to the modern task model:

```csharp
public Task<int> ComputeAsync(int input)
{
    var tcs = new TaskCompletionSource<int>(
        TaskCreationOptions.RunContinuationsAsynchronously);

    var worker = new BackgroundWorker();
    worker.DoWork += (s, e) => e.Result = DoHeavyComputation((int)e.Argument!);
    worker.RunWorkerCompleted += (s, e) =>
    {
        if (e.Error != null) tcs.SetException(e.Error);
        else if (e.Cancelled) tcs.SetCanceled();
        else tcs.SetResult((int)e.Result!);
    };
    worker.RunWorkerAsync(input);

    return tcs.Task;
}
```

`RunContinuationsAsynchronously` is critical: without it, calling `SetResult` from the `RunWorkerCompleted` event handler would execute the task's continuations synchronously on that thread, potentially causing reentrancy or unexpected context issues. With it, continuations are always scheduled asynchronously. Use `TrySetResult`/`TrySetException` if there is any chance the completion could be triggered multiple times.

---

## Q22. Implement a fan-out/fan-in pattern to fetch data from 5 microservices in parallel with a timeout.

**Concepts**
- Task.WhenAll for fan-out collection
- CancellationTokenSource.CancelAfter for overall timeout
- Individual task exception handling
- AggregateException flattening
- Result aggregation after WhenAll

**Answer**

```csharp
public async Task<DashboardData> GetDashboardAsync(CancellationToken userCt)
{
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(userCt);
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    var tasks = new[]
    {
        _userService.GetUserAsync(cts.Token),
        _orderService.GetOrdersAsync(cts.Token),
        _productService.GetProductsAsync(cts.Token),
        _notificationService.GetNotificationsAsync(cts.Token),
        _analyticsService.GetMetricsAsync(cts.Token)
    };

    try
    {
        await Task.WhenAll(tasks);
    }
    catch (Exception)
    {
        // WhenAll throws on first fault, but ALL tasks complete (or fail)
        // Inspect individual tasks for partial success:
    }

    return new DashboardData
    {
        User = tasks[0].IsCompletedSuccessfully ? tasks[0].Result : null,
        Orders = tasks[1].IsCompletedSuccessfully ? tasks[1].Result : null,
        // ... allow partial results for degraded dashboard
    };
}
```

The linked token combines user cancellation with the 5-second timeout. `Task.WhenAll` runs all 5 calls in parallel. Even after `WhenAll` throws, all tasks have reached a terminal state — you can safely inspect `IsCompletedSuccessfully` on each to build a degraded response rather than failing the entire dashboard.

---

## Q23. How do you implement an async retry policy with exponential backoff?

**Concepts**
- Retry loop with await Task.Delay
- CancellationToken integration
- Exponential backoff with jitter
- Maximum retry count
- Specific exception filtering

**Answer**

```csharp
public async Task<T> RetryAsync<T>(
    Func<CancellationToken, Task<T>> operation,
    int maxRetries,
    CancellationToken ct)
{
    int delay = 100; // ms
    for (int attempt = 0; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation(ct);
        }
        catch (HttpRequestException) when (attempt < maxRetries)
        {
            // Add jitter to avoid thundering herd
            int jitter = Random.Shared.Next(0, delay / 2);
            await Task.Delay(delay + jitter, ct);
            delay = Math.Min(delay * 2, 30_000); // cap at 30s
        }
    }
    throw new InvalidOperationException("Should not reach here");
}
```

Key design decisions: `await Task.Delay` rather than `Thread.Sleep` so no thread is held during backoff. Jitter prevents the thundering herd problem where all retrying clients hit the server simultaneously. The `when (attempt < maxRetries)` exception filter avoids catching on the final attempt, letting the exception propagate naturally. The `CancellationToken` is passed to both the operation and the delay — if shutdown is requested mid-backoff, `Task.Delay` throws `OperationCanceledException` immediately.

---

## Q24. You have a service that fires 100 tasks and some fail. How do you collect all results and errors?

**Concepts**
- Task.WhenAll does not stop on first failure
- Access .Result only after checking IsCompletedSuccessfully
- Partition results into successes and failures
- AggregateException.InnerExceptions for all errors
- Structured result type

**Answer**

```csharp
public async Task<(List<T> Successes, List<Exception> Failures)>
    RunAllAsync<T>(IEnumerable<Func<Task<T>>> operations)
{
    var tasks = operations.Select(op => op()).ToList();

    // WhenAll will throw on first failure, but all tasks still run to completion
    await Task.WhenAll(tasks).ContinueWith(_ => { }); // swallow to inspect all

    var successes = new List<T>();
    var failures = new List<Exception>();

    foreach (var t in tasks)
    {
        if (t.IsCompletedSuccessfully)
            successes.Add(t.Result);
        else if (t.IsFaulted)
            failures.Add(t.Exception!.InnerException!);
    }

    return (successes, failures);
}
```

The key insight is that `Task.WhenAll` runs every task to completion even when some fail — it only throws after all tasks finish. By attaching a continuation that swallows the exception, we can inspect every task individually. Accessing `.Result` only on `IsCompletedSuccessfully` tasks avoids `AggregateException`. This pattern is useful for bulk import/export operations where partial success is acceptable and you want to report all failures at once.

---

## Q25. How would you build a simple async circuit breaker using Task?

**Concepts**
- Circuit states: Closed, Open, Half-Open
- Failure threshold and reset timeout
- Task.Delay for half-open probe
- Interlocked / lock for state management
- Cancellation integration

**Answer**

```csharp
public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _openedAt = DateTime.MinValue;
    private CircuitState _state = CircuitState.Closed;

    private readonly int _threshold;
    private readonly TimeSpan _resetTimeout;

    public CircuitBreaker(int threshold = 5, int resetSeconds = 30)
    {
        _threshold = threshold;
        _resetTimeout = TimeSpan.FromSeconds(resetSeconds);
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _openedAt < _resetTimeout)
                throw new CircuitBreakerOpenException();
            _state = CircuitState.HalfOpen;
        }

        try
        {
            var result = await action(ct);
            // success — reset
            _failureCount = 0;
            _state = CircuitState.Closed;
            return result;
        }
        catch when (!ct.IsCancellationRequested)
        {
            if (Interlocked.Increment(ref _failureCount) >= _threshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
            }
            throw;
        }
    }

    private enum CircuitState { Closed, Open, HalfOpen }
}
```

`Interlocked.Increment` makes the failure count update thread-safe without a lock. The circuit opens when the threshold is crossed and remains open until the reset timeout elapses. In `HalfOpen`, the next call is a probe: success closes the circuit, failure re-opens it. For production use, consider Polly's `CircuitBreakerPolicy` which handles edge cases (half-open concurrency, per-exception filtering, events) more robustly.

---

## Q26. How do you diagnose a deadlock caused by .Result in an async chain?

**Concepts**
- Deadlock: thread blocked waiting for task; task waiting for blocked thread's context
- Debugger: pause execution, inspect Threads window
- Thread stacks showing WaitOne or SleepSpecialWait
- SynchronizationContext.Current != null indicates risk
- Fix: async all the way or ConfigureAwait(false) throughout

**Answer**

A `.Result` deadlock presents as: application hangs, 100% CPU is not occurring, and the UI or request is permanently unresponsive. In Visual Studio, Debug > Break All, then open the Threads window and select the blocked thread. Its call stack will show the thread waiting in `Task.Wait` or accessing `.Result`. Navigate to the awaiting code.

Simultaneously, look for threads stuck in `SynchronizationContextTaskScheduler.TryExecuteTask` — these are continuations waiting to execute on the context thread that is blocked. This is the classical "both waiting for each other" scenario.

Fix strategy: (1) Make the blocking call async: replace `GetDataAsync().Result` with `await GetDataAsync()` and propagate `async` up the call stack. (2) If you absolutely cannot make the outer call async (legacy interface, synchronous interface method), use `ConfigureAwait(false)` on every `await` inside the called async methods so continuations do not need the original context. (3) Use `Task.Run(() => GetDataAsync().GetAwaiter().GetResult())` as a last resort — this runs the blocking call on a ThreadPool thread with no ambient `SynchronizationContext`, avoiding the deadlock but wasting a thread.

---

## Q27. How do you choose between Task and ValueTask for a high-performance cache read method?

**Concepts**
- Cache hit: synchronous result — ValueTask zero-allocation
- Cache miss: async DB fetch — ValueTask<T> wraps Task<T>
- await-once rule for ValueTask
- Benchmark to measure real impact
- API design: expose ValueTask for hot-path callers

**Answer**

For a method that hits the cache (synchronous) most of the time but must go to the database (async) on a miss, `ValueTask<T>` is the correct return type. The synchronous path avoids any heap allocation; the async path pays the cost of a `Task<T>` only when needed.

```csharp
public async ValueTask<Product?> GetProductAsync(int id, CancellationToken ct)
{
    // Hot path: cache hit — no allocation
    if (_cache.TryGetValue(id, out Product? product))
        return product;

    // Cold path: DB miss — allocates a Task internally
    product = await _repository.GetByIdAsync(id, ct);
    if (product != null)
        _cache[id] = product;

    return product;
}
```

Callers must obey the single-await rule. If a caller needs to store the result and reference it later, they should `await` it immediately:

```csharp
var product = await _cache.GetProductAsync(id, ct);
// Do NOT store the ValueTask for later awaiting
```

The allocation savings are meaningful in high-throughput scenarios (100k+ calls/second) but negligible in typical business applications. Use `BenchmarkDotNet` to measure before committing to `ValueTask` — its usage restrictions are a correctness risk, and the benefit must justify that risk. In ASP.NET Core, many framework methods now return `ValueTask` precisely because they are on critical hot paths.
