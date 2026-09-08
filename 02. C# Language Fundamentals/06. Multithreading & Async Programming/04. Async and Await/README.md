# Async and Await — Interview Q&A


## Table of Contents

1. [Q1. What does the async keyword do to a method?](#q1-what-does-the-async-keyword-do-to-a-method)
2. [Q2. What happens at runtime when execution reaches an await expression?](#q2-what-happens-at-runtime-when-execution-reaches-an-await-expression)
3. [Q3. What is SynchronizationContext and how does it interact with async/await?](#q3-what-is-synchronizationcontext-and-how-does-it-interact-with-asyncawait)
4. [Q4. What does ConfigureAwait(false) do and when should you use it?](#q4-what-does-configureawaitfalse-do-and-when-should-you-use-it)
5. [Q5. What is the difference between async void and async Task?](#q5-what-is-the-difference-between-async-void-and-async-task)
6. [Q6. How does async Main work in C#?](#q6-how-does-async-main-work-in-c)
7. [Q7. What is the awaitable pattern and how does it work?](#q7-what-is-the-awaitable-pattern-and-how-does-it-work)
8. [Q8. What is IAsyncEnumerable<T> and how does it differ from IEnumerable<T>?](#q8-what-is-iasyncenumerablet-and-how-does-it-differ-from-ienumerablet)
9. [Q9. What is AsyncLocal<T> and when is it used?](#q9-what-is-asynclocalt-and-when-is-it-used)
10. [Q10. What is IProgress<T> and how is it used with async methods?](#q10-what-is-iprogresst-and-how-is-it-used-with-async-methods)
11. [Q11. What does await Task.Yield() do?](#q11-what-does-await-taskyield-do)
12. [Q12. How does exception handling work across await boundaries?](#q12-how-does-exception-handling-work-across-await-boundaries)
13. [Q13. What is the async state machine and why does it allocate on the heap?](#q13-what-is-the-async-state-machine-and-why-does-it-allocate-on-the-heap)
14. [Q14. What is async over sync and why is it an anti-pattern?](#q14-what-is-async-over-sync-and-why-is-it-an-anti-pattern)
15. [Q15. How do you implement async initialization for a class?](#q15-how-do-you-implement-async-initialization-for-a-class)
16. [Q16. Classic deadlock: how does calling .Result on an async method deadlock in WPF?](#q16-classic-deadlock-how-does-calling-result-on-an-async-method-deadlock-in-wpf)
17. [Q17. What is the async void exception trap?](#q17-what-is-the-async-void-exception-trap)
18. [Q18. What is the "not awaiting a task" (fire-and-forget) bug?](#q18-what-is-the-not-awaiting-a-task-fire-and-forget-bug)
19. [Q19. What is awaiting in a loop vs Task.WhenAll — which is better?](#q19-what-is-awaiting-in-a-loop-vs-taskwhenall-which-is-better)
20. [Q20. What happens when you use ConfigureAwait(false) incorrectly in UI code?](#q20-what-happens-when-you-use-configureawaitfalse-incorrectly-in-ui-code)
21. [Q21. What are the pitfalls of using async with LINQ?](#q21-what-are-the-pitfalls-of-using-async-with-linq)
22. [Q22. How would you convert a synchronous method to async without breaking existing callers?](#q22-how-would-you-convert-a-synchronous-method-to-async-without-breaking-existing-callers)
23. [Q23. How would you build a streaming API endpoint using IAsyncEnumerable<T>?](#q23-how-would-you-build-a-streaming-api-endpoint-using-iasyncenumerablet)
24. [Q24. A developer reports that their async WPF application freezes briefly during data loading. How do you diagnose and fix it?](#q24-a-developer-reports-that-their-async-wpf-application-freezes-briefly-during-data-loading-how-do-you-diagnose-and-fix-it)
25. [Q25. How do you implement rate-limited async processing (e.g., 10 API calls per second)?](#q25-how-do-you-implement-rate-limited-async-processing-eg-10-api-calls-per-second)
26. [Q26. How do you write unit tests for async methods?](#q26-how-do-you-write-unit-tests-for-async-methods)
27. [Q27. How do you handle exceptions in async streams (IAsyncEnumerable<T>)?](#q27-how-do-you-handle-exceptions-in-async-streams-iasyncenumerablet)

---
## Foundation Questions

---

## Q1. What does the async keyword do to a method?

**Concepts**
- Marks method as asynchronous — enables await inside it
- Compiler generates a state machine
- Does NOT make the method run on a new thread
- Return type must be void, Task, Task<T>, ValueTask, ValueTask<T>, or IAsyncEnumerable<T>
- Wrapper over synchronous code until first await

**Answer**

The `async` keyword instructs the C# compiler to transform the method body into a state machine. The actual IL that runs at runtime is not your code verbatim — it is a generated class with fields for local variables, parameters, and a state integer. The method runs synchronously until the first `await` expression. If that awaitable is already completed, execution continues inline without suspending. If it is not yet complete, the state machine captures the current position (the "continuation") and returns an incomplete task to the caller.

Crucially, `async` does not create a new thread. The method runs on the caller's thread up to the first suspension. After suspension, the continuation runs on a ThreadPool thread (or back on the original context if a `SynchronizationContext` was captured). The method body is purely a description of what to do — the CLR and ThreadPool decide when and on which thread to do it.

---

## Q2. What happens at runtime when execution reaches an await expression?

**Concepts**
- Check if awaitable is already completed
- If completed: continue synchronously (no state machine suspend)
- If not completed: capture continuation, return to caller
- SynchronizationContext captured at await point
- Continuation scheduled when awaitable completes

**Answer**

When the runtime evaluates `await someTask`, it calls `someTask.GetAwaiter()` and checks `IsCompleted`. If the awaitable is already done, execution continues synchronously in the same stack frame — no suspension, no context capture, minimal overhead. If it is not yet complete, the state machine serializes all live local variables into heap fields, stores the current state number, and registers the continuation (the rest of the method) with the awaitable. Control returns to the caller with an incomplete `Task`.

When the awaitable eventually completes, it invokes the continuation. If a `SynchronizationContext` was captured, the continuation is posted to it (e.g., posted to the UI thread dispatcher in WPF). Without an ambient context — ASP.NET Core, console apps, background threads — the continuation is scheduled on a ThreadPool thread. This is why `await` in ASP.NET Core does not require the same thread to resume: there is no SynchronizationContext, so any pool thread can pick up the continuation.

---

## Q3. What is SynchronizationContext and how does it interact with async/await?

**Concepts**
- Abstract mechanism for marshaling callbacks to a specific thread/context
- WPF/WinForms: DispatcherSynchronizationContext (UI thread)
- ASP.NET Classic: AspNetSynchronizationContext
- ASP.NET Core: null (no context)
- ConfigureAwait(false) opts out of context capture

**Answer**

`SynchronizationContext` is an abstraction that lets code post work to a specific execution context. WPF uses `DispatcherSynchronizationContext` to marshal callbacks to the UI thread. ASP.NET Classic uses `AspNetSynchronizationContext` to associate continuations with the HTTP request context.

When `await` suspends an async method, it captures `SynchronizationContext.Current`. When the awaitable completes, the continuation is posted to that context via `context.Post(...)`. This is what makes UI updates after an `await` work without manual `Dispatcher.Invoke` calls in WPF — the continuation automatically runs on the UI thread.

The problem arises when the context thread is blocked (e.g., calling `.Result`) and the continuation needs to post back to it — a deadlock. ASP.NET Core deliberately does not install a `SynchronizationContext`, so all continuations run on arbitrary ThreadPool threads, eliminating this deadlock class entirely and enabling higher throughput.

---

## Q4. What does ConfigureAwait(false) do and when should you use it?

**Concepts**
- Opts out of SynchronizationContext capture
- Continuation runs on any ThreadPool thread
- Use in library code to avoid context-dependency
- Do NOT use if continuation needs to access UI elements
- No effect in ASP.NET Core (no context anyway)

**Answer**

`ConfigureAwait(false)` tells the awaiter not to capture the current `SynchronizationContext`. The continuation will be scheduled on any available ThreadPool thread rather than being posted back to the original context. This has two effects: it avoids a context marshal (a small performance gain), and it eliminates the deadlock risk when the context thread is blocked.

The guideline is: library code should use `ConfigureAwait(false)` on every `await`. This makes libraries safe to call from any context (WPF, ASP.NET Classic, console) without risking deadlock. Application code (the code that actually touches UI or accesses request-specific data) should not use `ConfigureAwait(false)` on `await`s that need the original context afterward.

```csharp
// Library code: always ConfigureAwait(false)
public async Task<string> FetchDataAsync(string url)
{
    using var client = new HttpClient();
    var response = await client.GetAsync(url).ConfigureAwait(false);
    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
}
```

In ASP.NET Core, there is no `SynchronizationContext`, so `ConfigureAwait(false)` has no behavioral effect — but it is still good practice in library code to signal intent and remain portable.

---

## Q5. What is the difference between async void and async Task?

**Concepts**
- async Task: exceptions captured in Task, propagated on await
- async void: exceptions fired at SynchronizationContext, crash process
- async void: caller cannot await completion
- async void: appropriate only for event handlers
- Compiler does not prevent async void in non-event contexts

**Answer**

`async void` and `async Task` are syntactically similar but behaviorally very different in exception handling and composability. With `async Task`, exceptions are stored in the returned task and rethrown when the task is awaited. With `async void`, exceptions are posted to the `SynchronizationContext` as unhandled exceptions — in most environments, this terminates the process with no opportunity for recovery.

Furthermore, a caller of an `async void` method gets no `Task` back, so it cannot `await` the method, cannot cancel it, and cannot know when it has finished. This makes testing and composition impossible.

The only legitimate use of `async void` is for event handlers, which have a fixed `void` return type signature that cannot be changed:

```csharp
// Acceptable: event handler (cannot return Task)
private async void Button_Click(object sender, EventArgs e)
{
    try { await LoadDataAsync(); }
    catch (Exception ex) { ShowError(ex); }
}

// Everywhere else: async Task
public async Task LoadDataAsync() { ... }
```

Always wrap the body of `async void` event handlers in `try/catch` to prevent unhandled exceptions from reaching the `SynchronizationContext`.

---

## Q6. How does async Main work in C#?

**Concepts**
- Available since C# 7.1
- Signature: static async Task Main(string[] args)
- Compiler generates a synchronous entry point that calls .GetAwaiter().GetResult()
- Supports await in top-level startup code
- Return code: Task<int> for exit code support

**Answer**

Since C# 7.1, the `Main` method can be declared `async Task` or `async Task<int>`. The compiler generates a synchronous entry-point wrapper that calls `Main(...).GetAwaiter().GetResult()`, blocking the main thread until the async work completes. This is safe because it is the entry-point thread with no `SynchronizationContext` to deadlock on.

```csharp
static async Task<int> Main(string[] args)
{
    await InitializeServicesAsync();
    await RunApplicationAsync();
    return 0;
}
```

Before C# 7.1, async startup required `Task.Run(...).GetAwaiter().GetResult()` or similar workarounds. The `async Task<int>` overload allows returning a process exit code. Top-level statements (C# 9+) also support async naturally — the implicit `Main` is async.

---

## Q7. What is the awaitable pattern and how does it work?

**Concepts**
- Any type with GetAwaiter() returning an awaiter is awaitable
- Awaiter must implement: IsCompleted, GetResult(), OnCompleted(Action)
- ICriticalNotifyCompletion for unsafe continuations
- Custom awaitables: YieldAwaitable, Task.Yield(), custom schedulers
- Duck typing — no interface required

**Answer**

The `await` keyword uses duck typing: any expression is awaitable if it has a `GetAwaiter()` method (instance or extension) returning an object with `bool IsCompleted { get; }`, `void OnCompleted(Action continuation)`, and `TResult GetResult()`. No interface is strictly required.

`Task` and `ValueTask` implement this pattern. So does `Task.Yield()`, which returns a `YieldAwaitable` that is never complete synchronously, forcing a context switch. `ConfigureAwait(false)` works by returning a `ConfiguredTaskAwaitable` — a different awaiter that skips context capture.

Custom awaitables are useful in advanced scenarios: a game engine might have an `AwaitNextFrame()` awaitable; a test harness might have a deterministic scheduler awaitable. The pattern enables first-class async support for any completion mechanism without requiring inheritance from `Task`.

```csharp
// Task.Yield: forces async continuation
public async Task<T> EnsureAsyncAsync(Func<T> syncWork)
{
    await Task.Yield(); // yield to allow other tasks to run first
    return syncWork();
}
```

---

## Q8. What is IAsyncEnumerable<T> and how does it differ from IEnumerable<T>?

**Concepts**
- IAsyncEnumerable<T>: each MoveNextAsync() is awaitable
- Lazy pull-based async streaming
- await foreach for consumption
- Implemented with async iterator methods (yield return)
- CancellationToken via [EnumeratorCancellation] or WithCancellation()

**Answer**

`IAsyncEnumerable<T>` is the async counterpart of `IEnumerable<T>`. Each call to `MoveNextAsync()` returns a `ValueTask<bool>`, allowing the producer to perform async operations (I/O, database reads) between items. The consumer uses `await foreach`:

```csharp
await foreach (var item in GetItemsAsync(cancellationToken))
{
    await ProcessAsync(item);
}
```

Unlike returning `Task<List<T>>` (which buffers all items before returning), `IAsyncEnumerable<T>` streams items one at a time — the consumer begins processing before all items are produced. This is essential for large result sets, real-time feeds, and memory-efficient pipelines.

Implement with an async iterator:

```csharp
public async IAsyncEnumerable<Order> GetOrdersAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var batch in _db.GetOrderBatchesAsync(ct))
        foreach (var order in batch)
            yield return order;
}
```

`[EnumeratorCancellation]` is required when passing a `CancellationToken` through `WithCancellation()` — it signals to the compiler to inject the token into the `ct` parameter.

---

## Q9. What is AsyncLocal<T> and when is it used?

**Concepts**
- Per-execution-context value (flows with async continuations)
- Unlike ThreadLocal<T>, works across thread switches
- Child execution contexts inherit parent's value (copy-on-write)
- Changes in child do not propagate back to parent
- Basis for HttpContext.Current, Activity, ILogger scope

**Answer**

`AsyncLocal<T>` stores a value that flows with the execution context across `await` boundaries. Unlike `ThreadLocal<T>`, which ties a value to a specific OS thread, `AsyncLocal<T>` is associated with the logical async call chain. When an `await` causes execution to resume on a different thread, the `AsyncLocal` values are correctly restored.

```csharp
private static readonly AsyncLocal<string> _correlationId = new AsyncLocal<string>();

async Task HandleRequestAsync()
{
    _correlationId.Value = Guid.NewGuid().ToString();
    await Task.WhenAll(
        ProcessPartAAsync(), // inherits correlationId
        ProcessPartBAsync()  // inherits correlationId independently
    );
}
```

Child execution contexts (spawned by `Task.Run` or `Task.WhenAll`) inherit the parent's `AsyncLocal` values, but modifications in the child do not propagate back to the parent — this is copy-on-write semantics. ASP.NET Core's `IHttpContextAccessor`, `Activity.Current` (distributed tracing), and `ILogger`'s scope mechanism all use `AsyncLocal` internally. Avoid overusing `AsyncLocal` for data that should be passed explicitly — it creates implicit coupling that is hard to test and debug.

---

## Q10. What is IProgress<T> and how is it used with async methods?

**Concepts**
- IProgress<T> interface with Report(T) method
- Progress<T> implementation (marshals to SynchronizationContext)
- Decouples async work from UI update mechanism
- Thread-safe reporting from any thread
- Null-safe pattern: progress?.Report(value)

**Answer**

`IProgress<T>` is an interface with a single method `Report(T value)`. It decouples an async operation from the mechanism used to display progress, allowing library code to report progress without knowing about UI frameworks.

`Progress<T>` is the standard implementation. It captures the `SynchronizationContext` at construction time and posts `Report` calls to it, automatically marshaling progress updates to the UI thread:

```csharp
// UI code:
var progress = new Progress<int>(percent =>
    progressBar.Value = percent); // always runs on UI thread

await ProcessFilesAsync(files, progress, cancellationToken);

// Library/service code:
async Task ProcessFilesAsync(
    IEnumerable<string> files,
    IProgress<int>? progress,
    CancellationToken ct)
{
    int i = 0, total = files.Count();
    foreach (var file in files)
    {
        await ProcessFileAsync(file, ct);
        progress?.Report((int)(++i * 100.0 / total));
    }
}
```

The null-conditional `progress?.Report(...)` handles the case where the caller does not care about progress. `IProgress<T>` is intentionally an interface so it is easy to mock in unit tests. The `Report` method is fire-and-forget from the worker's perspective — it does not return a `Task` and the worker does not wait for the UI to update.

---

## Q11. What does await Task.Yield() do?

**Concepts**
- Forces an await that never completes synchronously
- Always schedules continuation asynchronously
- Useful for releasing the current thread to process other work
- Different from Thread.Yield() (which is synchronous)
- Performance cost: always causes a context switch

**Answer**

`Task.Yield()` returns a `YieldAwaitable` whose `IsCompleted` is always `false`. This forces the state machine to suspend and schedule the continuation rather than continuing synchronously. The continuation is posted to the current `SynchronizationContext` or ThreadPool.

Common use cases: (1) in a long synchronous loop, periodically `await Task.Yield()` to give other tasks a chance to run, preventing a single task from monopolizing a thread; (2) in test helpers, force async behavior to test async code paths; (3) in UI code, break up work to keep the UI responsive.

```csharp
async Task ProcessLargeCollectionAsync(IEnumerable<Item> items)
{
    int i = 0;
    foreach (var item in items)
    {
        Process(item);
        if (++i % 100 == 0)
            await Task.Yield(); // yield every 100 items
    }
}
```

`Task.Yield()` has overhead — it always causes a context switch even if the calling thread has nothing else to do. For high-performance tight loops, consider `Thread.Sleep(0)` or `SpinWait` for yielding that avoids full context switch overhead. But for async methods, `Task.Yield()` is the appropriate mechanism.

---

## Q12. How does exception handling work across await boundaries?

**Concepts**
- try/catch works naturally across await boundaries
- Exception captured in Task state, rethrown on await
- finally blocks execute after both normal and exceptional paths
- AggregateException unwrapped for single-task await
- Synchronous exceptions before first await are not wrapped

**Answer**

The C# compiler transforms `try/catch/finally` blocks around `await` expressions into state machine transitions that correctly propagate exceptions. From the developer's perspective, exception handling in async code looks identical to synchronous code:

```csharp
async Task DoWorkAsync()
{
    try
    {
        var data = await FetchDataAsync();    // exception here is caught below
        await SaveDataAsync(data);
        Console.WriteLine("Success");
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Fetch failed");
    }
    finally
    {
        _logger.LogInformation("Cleanup");    // always runs
    }
}
```

Exceptions thrown before the first `await` propagate synchronously when the async method is called (not when the task is awaited), which is a subtle difference for callers doing argument validation. When you `await` a task, the CLR unwraps the `AggregateException` and throws the original exception type directly — unlike `.Result` which leaves it wrapped. `finally` blocks run whether the async method completes successfully, throws, or is cancelled.

---

## Q13. What is the async state machine and why does it allocate on the heap?

**Concepts**
- Compiler-generated struct implementing IAsyncStateMachine
- Boxed to heap when first suspension occurs
- Fields: local variables, parameters, awaiter, state integer
- Zero allocation if method completes synchronously (no boxing)
- MoveNext() called to advance state machine

**Answer**

The C# compiler transforms every `async` method into a struct implementing `IAsyncStateMachine`. The struct has fields for every local variable, method parameter, and the current awaiter. It also has a `state` integer tracking which `await` point the method is currently at.

When an `async` method is called, the state machine starts as a stack-allocated struct. If the very first awaitable is already complete (`IsCompleted = true`), the method runs entirely inline without heap allocation. When the method suspends at an `await`, the struct is boxed to the heap (the `IAsyncStateMachine` reference stored in the awaiter's callback list must outlive the stack frame). The continuation — `MoveNext()` — is called by the awaiter when it completes, advancing the state integer and resuming execution.

This is why hot-path methods that frequently complete synchronously benefit from `ValueTask<T>` — it avoids `Task<T>` allocation AND, if written carefully, the state machine struct can avoid boxing too. The `[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]` attribute (available in .NET 6+) further reduces allocations by pooling state machine objects.

---

## Q14. What is async over sync and why is it an anti-pattern?

**Concepts**
- Wrapping synchronous code in Task.Run in a library method
- Caller deceived: thinks I/O is async but is actually CPU on pool thread
- Wastes a ThreadPool thread for synchronous work
- Correct pattern: let caller decide to use Task.Run
- Library APIs should be either truly async or purely synchronous

**Answer**

"Async over sync" means using `Task.Run` inside a library method to make a synchronous operation appear async. The problem: the library takes a ThreadPool thread for itself, denying the caller the choice of threading strategy. The call is not truly non-blocking — it simply moves the blocking to a different thread.

```csharp
// LIBRARY ANTI-PATTERN: async over sync
public Task<int> ComputeAsync(int n) => Task.Run(() => Compute(n));

// CORRECT: synchronous library, caller decides threading
public int Compute(int n) { /* pure CPU */ }

// CALLER (application code): decides to offload
var result = await Task.Run(() => _service.Compute(input));
```

Additionally, the library does not know the caller's preferred scheduler, affinity, or cancellation story — all of which are lost when `Task.Run` is used internally. The definitive reference is Stephen Toub's "Should I expose asynchronous wrappers for synchronous methods?" (answer: no for libraries).

The exception: if the library method performs genuine I/O (network, disk, database), it should expose a true `async Task` method, not a `Task.Run` wrapper.

---

## Q15. How do you implement async initialization for a class?

**Concepts**
- Constructors cannot be async
- Factory method pattern with private constructor
- Lazy<Task<T>> for single initialization
- IAsyncDisposable for async cleanup
- AsyncLazy<T> pattern

**Answer**

Constructors cannot be `async`, so async initialization requires a factory method:

```csharp
public class DataService
{
    private readonly DbConnection _connection;

    private DataService(DbConnection connection)
    {
        _connection = connection;
    }

    // Factory: async initialization enforced by design
    public static async Task<DataService> CreateAsync(
        string connectionString,
        CancellationToken ct = default)
    {
        var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct);
        return new DataService(conn);
    }
}

// Usage:
var service = await DataService.CreateAsync(connStr);
```

For classes where initialization should happen once and be reused, use the "async lazy" pattern:

```csharp
private readonly Lazy<Task<Config>> _config =
    new Lazy<Task<Config>>(() => LoadConfigAsync());

public async Task<Config> GetConfigAsync()
    => await _config.Value;
```

This ensures `LoadConfigAsync` is called once. `Lazy<Task<T>>` is thread-safe for creation but the task may start multiple times under race conditions if the first call hasn't completed — for true single-execution, use `AsyncLazy<T>` from third-party libraries or a `SemaphoreSlim`-guarded initialization flag.

---

## Gotchas — Async and Await (Interview Traps)

---

#### Gotcha 1. async void Exceptions Crash the Process

**Concepts**
- async void has no Task to hold the exception
- Exception is posted to the SynchronizationContext and becomes unhandled
- Process terminates in most .NET environments
- Only valid use of async void is event handlers
- Fix: change to async Task; wrap event handler body in try/catch

**Answer**

`async void` methods cannot be awaited, so any exception they throw has nowhere to go — it is posted to `SynchronizationContext.Current` and becomes an unhandled exception, terminating the process. Unlike `async Task`, there is no `Task.Exception` property to observe later. The only legitimate use of `async void` is for event handlers, because event delegates are `void`-returning and cannot be changed. Even there, the handler body should be wrapped in `try/catch` to prevent silent crashes. In all other contexts, change the return type to `async Task`.

---

#### Gotcha 2. Sync-Over-Async Deadlocks in Single-Threaded Contexts

**Concepts**
- SynchronizationContext posts continuations back to a single thread
- .Result or .Wait() blocks that thread
- Continuation cannot run — deadlock
- No deadlock in ASP.NET Core or console apps (no single-threaded context)
- Fix: await all the way through the call chain

**Answer**

Calling `.Result` or `.Wait()` on an async method in a WPF, WinForms, or ASP.NET Classic environment blocks the single context thread. The awaited method captures that context at its `await`, and its continuation is scheduled to run on the same thread — which is blocked waiting for it. The result is a permanent deadlock. This pattern works fine in ASP.NET Core and console apps because they have no single-threaded `SynchronizationContext`, but it reliably deadlocks in UI apps. The only safe fix is to propagate `await` through the entire call chain.

---

#### Gotcha 3. ConfigureAwait(false) Is Required in Library Code

**Concepts**
- Library code runs in arbitrary caller contexts
- Without ConfigureAwait(false), continuations marshal back to the caller's context
- If caller uses .Result in a UI thread, deadlock risk increases
- ConfigureAwait(false) is defensive coding in libraries — not optional
- Application-layer code that uses UI or request context should NOT use ConfigureAwait(false)

**Answer**

Library methods that do not need to resume on the caller's context should always add `.ConfigureAwait(false)` to every internal `await`. Without it, continuations post back to whatever `SynchronizationContext` the caller has — typically the UI thread or request thread — increasing the chance of deadlocks when callers use the library from synchronous code. This is defensive practice: the library does not know how it will be called. Application code that accesses UI controls, `HttpContext`, or `DbContext` after an `await` must not use `ConfigureAwait(false)` on that specific `await`, because it needs to resume on the original context.

---

#### Gotcha 4. await in catch and finally Requires C# 6 or Later

**Concepts**
- C# 5 and earlier: await not allowed in catch or finally blocks
- C# 6+: await is permitted in both catch and finally
- Pre-C# 6 workaround: flag variable + second try block
- Incorrect workaround can miss awaiting the cleanup task
- Most code targets C# 9+ today — but legacy codebases still exist

**Answer**

In C# 5, `await` was not permitted inside `catch` or `finally` blocks. Developers who needed to perform async cleanup on exception had to set a flag, exit the catch block, and conditionally execute the async cleanup afterward — an awkward and error-prone pattern. C# 6 removed this restriction, and today `await` works correctly inside both `catch` and `finally`. This is a non-issue for code targeting modern C#, but when maintaining pre-C# 6 codebases or understanding older patterns, awareness of this limitation explains why some cleanup code is written in an unusual flag-based style.

---

#### Gotcha 5. Returning Task Without async Avoids Unnecessary State Machine Overhead

**Concepts**
- Adding async to a method that only delegates creates a state machine for no benefit
- State machine adds allocation and overhead per call
- If the method simply returns the result of another async call, drop async/await
- Exception semantics differ slightly: without async, exceptions propagate synchronously
- Benchmark before optimizing — the overhead is small for most paths

**Answer**

When an async method body is simply `return await SomeOtherAsync()`, the `async` keyword creates an unnecessary state machine — an allocation and a few nanoseconds of overhead per call. The same behavior can be achieved by removing `async` and returning the `Task` directly: `return SomeOtherAsync()`. However, there is a subtle difference: without `async`, exceptions thrown synchronously before the task is returned propagate directly to the caller rather than being wrapped in the returned task. For methods with no logic before the `return`, either style works; for methods with guard clauses before the return, keep `async` to ensure all exceptions are captured in the task.

---

#### Gotcha 6. Exceptions Wrap Into AggregateException Only via .Result, Not via await

**Concepts**
- await unwraps AggregateException and rethrows only the first inner exception
- .Result/.Wait() throw AggregateException wrapping the task's exceptions
- Code that catches AggregateException with await never fires
- Code that catches specific exception types works with await
- Mismatch causes swallowed exceptions when switching between await and .Result

**Answer**

`await` unwraps a faulted task's exception and rethrows the first `InnerException` directly — the `catch (AggregateException)` block never fires. `.Result` and `.Wait()` throw an `AggregateException` wrapping all task exceptions. Code written to catch `AggregateException` breaks silently when migrated from `.Result` to `await`, because the `catch` clause is never entered. The fix is to catch the specific exception type in async code (e.g., `catch (InvalidOperationException)`) and reserve `AggregateException` handling for code that explicitly uses `.Result` or `Task.WhenAll` results without awaiting.

---

#### Gotcha 7. IAsyncDisposable Requires "await using", Not Just "using"

**Concepts**
- IAsyncDisposable.DisposeAsync returns ValueTask
- A plain "using" statement calls Dispose(), not DisposeAsync()
- If the type only implements IAsyncDisposable (not IDisposable), "using" fails to compile
- "await using" is the correct syntax for async disposal
- Common with EF Core DbContext, HttpClient handlers, and stream wrappers

**Answer**

`IAsyncDisposable` exposes `DisposeAsync()` which returns a `ValueTask`. Using such a type inside a plain `using` statement calls `Dispose()` — if the type implements only `IAsyncDisposable` (not the synchronous `IDisposable`), this fails to compile. Even when both interfaces are implemented, a plain `using` block calls the synchronous `Dispose`, bypassing the async cleanup path entirely. The correct syntax is `await using var resource = new AsyncResource()`, which calls `DisposeAsync()` and awaits the result. This is particularly important for EF Core's `DbContext` and custom stream wrappers that need async flush on disposal.

---

#### Gotcha 8. CancellationToken Must Flow Through the Entire Call Chain

**Concepts**
- Passing CancellationToken only to the top-level method does not cancel inner calls
- Inner awaits that don't receive the token cannot be cancelled
- Results in slow cancellation: outer method is cancelled but inner work continues
- Pass the token to every async method, every HttpClient call, every EF Core query
- CancellationToken.None is an anti-pattern in library code

**Answer**

Cancellation in .NET is cooperative — every method that can be cancelled must receive the `CancellationToken` and pass it to its own inner calls. Accepting a token at the entry point but not threading it through to `HttpClient.GetAsync`, `DbContext.SaveChangesAsync`, or `Task.Delay` calls means those operations continue running after cancellation is requested, causing slow or incomplete cancellation. The pattern to follow: every async method signature that does any I/O should accept a `CancellationToken` parameter with a default of `default`, and pass it to every inner awaitable call.

---

#### Gotcha 9. CPU-Bound Work in async Methods Must Use Task.Run

**Concepts**
- async/await is designed for I/O-bound work — it does not parallelize CPU work
- CPU-bound code inside an async method still runs synchronously on the calling thread
- Task.Run moves CPU work to a ThreadPool thread
- Blocking the calling thread in an async method defeats its purpose
- Use Task.Run for CPU-intensive operations in async UI or server code

**Answer**

`async/await` does not make CPU-bound work faster or non-blocking — it only releases the calling thread during genuine waits (I/O, timers, network). A CPU-intensive loop inside an `async` method runs synchronously on whatever thread called it, blocking that thread just as a non-async method would. For CPU-bound work that must not block the calling thread (e.g., in a UI handler), use `await Task.Run(() => CpuIntensiveWork())` to offload the computation to a ThreadPool thread. For ASP.NET Core server code, `Task.Run` for CPU work is generally unnecessary because there is no UI thread to protect — but it still helps for extremely long-running computations that would otherwise hold a request thread.

---

#### Gotcha 10. Async Constructors Are Not Allowed — Use a Static Factory Method

**Concepts**
- Constructors cannot be async and cannot return Task
- Calling an async method from a constructor leaves it fire-and-forget
- The object is returned before async initialization completes
- Pattern: private constructor + public static async factory method
- Alternatively: a separate InitializeAsync() method called after construction

**Answer**

C# constructors are synchronous and cannot be `async`. Calling an async initialization method inside a constructor and not awaiting it creates a fire-and-forget task — the constructor returns the object before initialization completes, leaving the object in a partially initialized state. The standard pattern is a private constructor combined with a `public static async Task<T> CreateAsync(...)` factory method that awaits all async initialization before returning the fully ready object. Callers use `var obj = await MyClass.CreateAsync()` instead of `new MyClass()`. An alternative is a public `InitializeAsync()` method, but this is less safe because callers can forget to call it.

---

## Real-World Scenarios

---

## Q22. How would you convert a synchronous method to async without breaking existing callers?

**Concepts**
- Add async Task overload alongside synchronous method
- Deprecate synchronous version with [Obsolete]
- ConfigureAwait(false) in library async methods
- Avoid sync-over-async wrapper in the synchronous version
- Communicate migration path in API documentation

**Answer**

The safe migration path is to add a new async method, keep the synchronous version temporarily, and give callers time to migrate:

```csharp
// Step 1: Add async overload with Async suffix
public async Task<Order> GetOrderAsync(int id, CancellationToken ct = default)
{
    return await _db.Orders
        .Where(o => o.Id == id)
        .FirstOrDefaultAsync(ct)
        .ConfigureAwait(false);
}

// Step 2: Mark synchronous version obsolete (do NOT wrap async in .Result)
[Obsolete("Use GetOrderAsync for better performance. Will be removed in v3.")]
public Order GetOrder(int id)
{
    return _db.Orders.Where(o => o.Id == id).FirstOrDefault();
}
```

Never implement the synchronous version as `GetOrderAsync(id).GetAwaiter().GetResult()` — this creates a sync-over-async wrapper that can deadlock. If the synchronous implementation must use the same data access logic, the synchronous version should use synchronous data access methods (e.g., EF Core's synchronous `FirstOrDefault`).

Communicate the migration: update documentation, add XML doc comments to the async method, and set a removal timeline for the synchronous version in the changelog.

---

## Q23. How would you build a streaming API endpoint using IAsyncEnumerable<T>?

**Concepts**
- ASP.NET Core 6+ supports IAsyncEnumerable return from controllers
- Streams items as they become available (JSON array streamed)
- CancellationToken from HttpContext.RequestAborted
- Memory-efficient for large result sets
- Client-side: consume with streaming HttpClient

**Answer**

ASP.NET Core 6+ natively supports `IAsyncEnumerable<T>` as a controller action return type, streaming JSON items as they are produced:

```csharp
[HttpGet("orders/stream")]
public async IAsyncEnumerable<OrderDto> StreamOrders(
    [FromQuery] int customerId,
    [EnumeratorCancellation] CancellationToken ct)
{
    await foreach (var order in _repository.GetOrdersByCustomerAsync(customerId, ct))
    {
        ct.ThrowIfCancellationRequested();
        yield return new OrderDto(order);
    }
}

// Repository:
public async IAsyncEnumerable<Order> GetOrdersByCustomerAsync(
    int customerId,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var order in _db.Orders
        .Where(o => o.CustomerId == customerId)
        .AsAsyncEnumerable()
        .WithCancellation(ct))
    {
        yield return order;
    }
}
```

The framework serializes items incrementally — the client begins receiving data before the server has produced all items. `ct.ThrowIfCancellationRequested()` ensures the stream stops cleanly if the client disconnects. The repository uses EF Core's `AsAsyncEnumerable()` to stream rows from the database without buffering the entire result set, which is critical for large tables.

---

## Q24. A developer reports that their async WPF application freezes briefly during data loading. How do you diagnose and fix it?

**Concepts**
- Synchronous code on UI thread before first await
- CPU-heavy work running on UI thread after await without ConfigureAwait
- Missing await (synchronous method called on UI thread)
- Task.Run for CPU-bound work in async context
- Profiler: CPU samples on UI thread during freeze

**Answer**

UI freezes in async WPF code almost always have one of three causes: CPU-bound work happening on the UI thread, a missing `await` (the method is synchronous and blocks the UI), or a blocked `await` (synchronous wait on UI thread).

Diagnosis: use Visual Studio's Performance Profiler with CPU sampling. If the UI thread shows CPU activity during the freeze, the work is running on it. If the thread is blocked (waiting), look for `.Result`, `.Wait()`, or blocking I/O.

Common fix patterns:

```csharp
// PROBLEM 1: CPU work on UI thread
private async void LoadButton_Click(object s, RoutedEventArgs e)
{
    var result = ParseLargeFile(filePath); // sync, heavy CPU, blocks UI
    label.Content = result;
}

// FIX: offload CPU work to ThreadPool
private async void LoadButton_Click(object s, RoutedEventArgs e)
{
    var result = await Task.Run(() => ParseLargeFile(filePath)); // off UI thread
    label.Content = result; // back on UI thread — safe
}

// PROBLEM 2: missing await on async method
var data = GetDataAsync(); // forgot await — returns Task, doesn't wait
label.Content = data.ToString(); // prints Task object, not result

// FIX: await the method
var data = await GetDataAsync();
```

After finding the root cause in the profiler, add `Task.Run` to offload CPU work. Use `IProgress<T>` to report loading progress back to the UI thread without freezing it.

---

## Q25. How do you implement rate-limited async processing (e.g., 10 API calls per second)?

**Concepts**
- SemaphoreSlim for concurrency limiting
- RateLimiter (System.Threading.RateLimiting, .NET 7+)
- Fixed window vs sliding window vs token bucket
- await Task.Delay for backoff between requests
- Per-request cancellation token

**Answer**

.NET 7+ includes `System.Threading.RateLimiting` with `TokenBucketRateLimiter`, `FixedWindowRateLimiter`, and `SlidingWindowRateLimiter`. For simple per-second rate limiting:

```csharp
// .NET 7+ preferred approach
using var rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
{
    TokenLimit = 10,
    ReplenishmentPeriod = TimeSpan.FromSeconds(1),
    TokensPerPeriod = 10,
    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
    QueueLimit = 100
});

async Task ProcessItemsAsync(IEnumerable<Item> items, CancellationToken ct)
{
    foreach (var item in items)
    {
        using var lease = await rateLimiter.AcquireAsync(1, ct);
        if (!lease.IsAcquired) throw new OperationCanceledException(ct);
        await ProcessItemAsync(item, ct);
    }
}
```

For an older .NET version, a `SemaphoreSlim` plus `Task.Delay` timer approach works:

```csharp
private static readonly SemaphoreSlim _rateSemaphore = new SemaphoreSlim(10, 10);

async Task ProcessWithRateLimit(Item item, CancellationToken ct)
{
    await _rateSemaphore.WaitAsync(ct);
    try
    {
        await ProcessItemAsync(item, ct);
        await Task.Delay(1000 / 10, ct); // ~100ms between each of 10/s
    }
    finally { _rateSemaphore.Release(); }
}
```

The `RateLimiter` approach is preferable because it correctly handles burst behavior, queue overflow, and multiple concurrent callers without manual timer arithmetic.

---

## Q26. How do you write unit tests for async methods?

**Concepts**
- Test methods must be async Task (not async void)
- await the method under test
- xUnit/NUnit/MSTest all support async Task test methods
- Assert.ThrowsAsync for exception testing
- Mocking async dependencies with Returns(Task.FromResult(...))

**Answer**

Testing async methods is straightforward — test methods simply `return Task` and `await` the code under test:

```csharp
// xUnit example
public class OrderServiceTests
{
    [Fact]
    public async Task GetOrderAsync_ReturnsOrder_WhenExists()
    {
        // Arrange
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Order { Id = 1, Name = "Test" });

        var service = new OrderService(mockRepo.Object);

        // Act
        var order = await service.GetOrderAsync(1);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(1, order.Id);
    }

    [Fact]
    public async Task GetOrderAsync_ThrowsNotFoundException_WhenMissing()
    {
        var mockRepo = new Mock<IOrderRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

        var service = new OrderService(mockRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetOrderAsync(99));
    }
}
```

Key rules: always return `Task` from test methods (never `async void`), use `ReturnsAsync` when mocking (not `Returns(Task.FromResult(...))`), and use `Assert.ThrowsAsync` for exception assertions. For cancellation testing, pass a `CancellationToken.None` in normal tests and a pre-cancelled token or `CancellationTokenSource.CancelAfter(0)` for cancellation tests.

---

## Q27. How do you handle exceptions in async streams (IAsyncEnumerable<T>)?

**Concepts**
- try/catch around await foreach
- Exception in producer propagates to consumer via MoveNextAsync
- IAsyncDisposable for cleanup on exception
- Partial enumeration: only consumed items' errors surface
- OperationCanceledException on cancellation

**Answer**

Exceptions in an async stream propagate from the producer to the consumer through the `MoveNextAsync` call. The consumer catches them with a standard `try/catch` around the `await foreach`:

```csharp
// Consumer: catches producer exceptions
async Task ConsumeOrdersAsync(CancellationToken ct)
{
    try
    {
        await foreach (var order in _service.GetOrdersAsync(ct))
        {
            await ProcessAsync(order);
        }
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Stream interrupted by network error");
        // Partial results may have been processed — decide on retry strategy
    }
    catch (OperationCanceledException) when (ct.IsCancellationRequested)
    {
        _logger.LogInformation("Stream cancelled by user");
    }
}

// Producer: proper cleanup with IAsyncDisposable
public async IAsyncEnumerable<Order> GetOrdersAsync(
    [EnumeratorCancellation] CancellationToken ct)
{
    await using var connection = await OpenConnectionAsync(ct);
    await foreach (var batch in connection.FetchBatchesAsync(ct))
        foreach (var order in batch)
            yield return order;
    // IAsyncDisposable on connection ensures cleanup even on exception
}
```

Resources in the producer are cleaned up through `IAsyncDisposable` and `await using` — this fires even when the consumer stops iterating early (e.g., `break`) or when an exception occurs. The `await foreach` statement calls `IAsyncDisposable.DisposeAsync()` on the enumerator in a `finally` block.
