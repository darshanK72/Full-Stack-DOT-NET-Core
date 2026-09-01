# Async and Await — Interview Q&A

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

## Gotchas & Traps

---

## Q16. Classic deadlock: how does calling .Result on an async method deadlock in WPF?

**Concepts**
- DispatcherSynchronizationContext on UI thread
- .Result blocks UI thread waiting for task completion
- Task continuation needs UI thread to resume
- Deadlock: UI thread waiting for continuation; continuation waiting for UI thread
- Fix: async all the way, or ConfigureAwait(false) throughout

**Answer**

```csharp
// DEADLOCK: WPF button click handler
private void Button_Click(object sender, RoutedEventArgs e)
{
    // Blocks the UI thread (DispatcherSynchronizationContext)
    var result = GetDataAsync().Result;
    label.Content = result;
}

private async Task<string> GetDataAsync()
{
    // After await, continuation wants to resume on UI thread (captured context)
    var data = await httpClient.GetStringAsync("https://api.example.com/data");
    return data;
}
```

| Category | Problem | Impact |
|---|---|---|
| Deadlock | UI thread blocked by `.Result`; continuation posted to UI thread | Application hangs indefinitely — hard freeze |
| Root cause | SynchronizationContext captured at await; post requires blocked thread | Classic circular wait |
| Hidden risk | Works on console/ASP.NET Core (no SynchronizationContext) | Only fails in UI/ASP.NET Classic environments |

**Fix priority:**
1. Make the click handler `async void`: `private async void Button_Click(...)` and `await GetDataAsync()`.
2. Add `ConfigureAwait(false)` to every `await` in `GetDataAsync()` as a library-safe fallback — continuations won't need the UI thread.
3. Never use `.Result` or `.Wait()` on the UI thread.

---

## Q17. What is the async void exception trap?

**Concepts**
- async void exceptions posted to SynchronizationContext
- No Task to observe the exception
- Process termination in many environments
- Compiler gives no warning for async void in non-event contexts
- Test frameworks may also fail silently

**Answer**

```csharp
// DANGER: async void swallows exception context
async void FireAndForgetAsync()
{
    await Task.Delay(100);
    throw new InvalidOperationException("This terminates the process");
}

void CallerMethod()
{
    FireAndForgetAsync(); // no Task returned, no await possible
    // Exception posted to SynchronizationContext → process crash
}
```

| Category | Problem | Impact |
|---|---|---|
| Exception Propagation | Exception cannot be caught by caller | Silent application crash |
| Composability | No Task returned — cannot await, cancel, or chain | Untestable code |
| Debugging | Stack trace lost when posted to SynchronizationContext | Hard to diagnose crash location |

**Fix priority:**
1. Change return type to `async Task`: allows caller to `await` and catch exceptions.
2. If fire-and-forget is required, wrap in a `Task.Run` with explicit exception handling.
3. Register `TaskScheduler.UnobservedTaskException` as a safety net for logging.

---

## Q18. What is the "not awaiting a task" (fire-and-forget) bug?

**Concepts**
- Assigning Task to _ (discard) without error handling
- Returning before task completes (race condition)
- Exception silently lost
- Application state may be inconsistent
- Proper fire-and-forget pattern with logging

**Answer**

```csharp
// BUG: not awaiting — task result/exception silently lost
public async Task UpdateCacheAsync(string key, object value)
{
    await _db.SaveAsync(key, value);
    _ = _cache.RefreshAsync(key); // NOT awaited — fire and forget
    // Method returns before cache refresh completes
    // If RefreshAsync throws, exception is lost
}
```

| Category | Problem | Impact |
|---|---|---|
| Exception Safety | RefreshAsync exception never observed | Cache inconsistency with no diagnostic |
| Race Condition | Caller may read stale cache before refresh completes | Data integrity issue |
| Lifecycle | No cancellation on shutdown — refresh may run after service stops | Resource leak |

**Fix priority:**
1. If the cache refresh should complete before returning: `await _cache.RefreshAsync(key)`.
2. If truly fire-and-forget: add explicit fault handling: `_ = _cache.RefreshAsync(key).ContinueWith(t => _logger.LogError(t.Exception, "Cache refresh failed"), TaskContinuationOptions.OnlyOnFaulted)`.
3. For background work with lifecycle management, enqueue to a hosted `IBackgroundTaskQueue`.

---

## Q19. What is awaiting in a loop vs Task.WhenAll — which is better?

**Concepts**
- Sequential await: tasks run one at a time (serial)
- Task.WhenAll: all tasks run in parallel
- Awaiting in loop misses parallelism opportunity
- Task.WhenAll: all tasks created before any is awaited
- Consider bounded parallelism for large collections

**Answer**

```csharp
// SEQUENTIAL — slow: each fetch waits for previous to complete
async Task<List<string>> FetchAllSequential(IEnumerable<string> urls)
{
    var results = new List<string>();
    foreach (var url in urls)
        results.Add(await httpClient.GetStringAsync(url)); // serial
    return results;
}

// PARALLEL — fast: all fetches in-flight simultaneously
async Task<string[]> FetchAllParallel(IEnumerable<string> urls)
{
    var tasks = urls.Select(url => httpClient.GetStringAsync(url));
    return await Task.WhenAll(tasks); // parallel
}
```

The sequential version runs in `N × latency` time; the parallel version runs in approximately `max(latency)` time. For I/O-bound operations like HTTP requests, `Task.WhenAll` is almost always better. However, for large collections (thousands of URLs), creating all tasks at once can overwhelm the target service or exhaust resources — use `SemaphoreSlim` to bound concurrency. The rule: if the operations are independent and I/O-bound, prefer `Task.WhenAll` with bounded concurrency over sequential `await` in a loop.

---

## Q20. What happens when you use ConfigureAwait(false) incorrectly in UI code?

**Concepts**
- ConfigureAwait(false) removes context capture
- Continuation runs on ThreadPool, not UI thread
- Accessing UI elements from non-UI thread throws InvalidOperationException
- Correct use: library methods that don't touch UI
- Wrong use: UI code that accesses controls after await

**Answer**

`ConfigureAwait(false)` is correct in library code that does not need the original context. In UI code, it causes exceptions when the code after the `await` tries to access UI elements:

```csharp
// BUG in WPF code-behind: ConfigureAwait(false) in UI handler
private async void LoadButton_Click(object sender, RoutedEventArgs e)
{
    var data = await LoadDataAsync().ConfigureAwait(false); // opts out of UI context
    label.Content = data; // CRASH: InvalidOperationException — wrong thread
}

// CORRECT in WPF code-behind: no ConfigureAwait(false)
private async void LoadButton_Click(object sender, RoutedEventArgs e)
{
    var data = await LoadDataAsync(); // captures UI context, resumes on UI thread
    label.Content = data; // safe
}
```

The guideline: application-layer code that accesses context-sensitive resources (UI controls, `HttpContext`, `DbContext`) should not use `ConfigureAwait(false)`. Library code that has no such dependencies should always use it. The simplest rule: if you need to touch a UI element after the `await`, do not use `ConfigureAwait(false)` on that `await`.

---

## Q21. What are the pitfalls of using async with LINQ?

**Concepts**
- LINQ Select does not await — returns IEnumerable<Task<T>>
- ToList() needed to materialize tasks before WhenAll
- Async lambdas in Where/OrderBy not awaited
- Parallel execution vs sequential — LINQ doesn't control it
- System.Linq.Async for true async LINQ

**Answer**

Standard LINQ operators do not support async natively. Passing an async lambda to `Select` creates an `IEnumerable<Task<T>>`, not `IEnumerable<T>`. Not materializing the tasks leads to deferred execution issues:

```csharp
// BUG: LINQ deferred — tasks not started yet
var results = items.Select(async item => await ProcessAsync(item));
// results is IEnumerable<Task<string>> — nothing has run

// CORRECT: materialize into array of started tasks, then WhenAll
var tasks = items.Select(item => ProcessAsync(item)).ToArray();
var results = await Task.WhenAll(tasks);
```

Async lambdas in `Where` or `OrderBy` are even more dangerous: `Select` returns `Task<bool>` from the predicate, not `bool`, so filtering does not work as intended. The `System.Linq.Async` NuGet package from the Reactive Extensions team provides `SelectAwait`, `WhereAwait`, and other operators that properly handle async lambdas with sequential execution. For parallel LINQ, prefer `Task.WhenAll` over `AsParallel().Select(async ...)`, which has poor async support.

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
