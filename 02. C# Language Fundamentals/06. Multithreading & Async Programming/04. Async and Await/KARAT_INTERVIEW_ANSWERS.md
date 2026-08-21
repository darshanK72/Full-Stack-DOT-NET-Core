# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/04. Async and Await/`

---

#### Q1. (R) Under load, report-export API requests time out and thread-pool starvation alerts fire. Review this ASP.NET Core minimal endpoint and service:

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

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` / `.Wait()` on `Task` | Sync-over-async; blocks thread per request |
| Scalability | Two blocking waits per HTTP call | Thread-pool starvation; throughput collapse under concurrency |
| API design | Non-async endpoint delegate | Cannot accept `CancellationToken`; poor composability with filters/middleware |
| Runtime | Captured `SynchronizationContext` on legacy ASP.NET / some hosts | Potential deadlock — continuation waits for blocked request thread |

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

**Production takeaway:** Passes locally with one user; fails in production when hundreds of requests each block a pool thread waiting on I/O — the classic Karat async trap. See **Program.cs** Section 14 — deadlock pitfalls with `.Result` / `.Wait()`.

---

#### Q2. (R) A nightly export job sometimes crashes the worker process with no log line. Review this orchestrator:

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

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `async void` on non-event method | Exceptions crash process; caller cannot await or catch |
| Observability | `_ = RunExportPipelineAsync(...)` discard | Faulted export Task may go unlogged |
| Design | Sync `StartExport` returns before work completes | Host thinks job started successfully; no completion signal |
| Testing | Neither path returns `Task` | Cannot assert success/failure in unit or integration tests |

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

**Production takeaway:** Karat embeds `async void` in service code because it compiles and "works" until the first fault — then the process dies with no useful audit trail.

---

#### Q3. (R) A WPF desktop app deadlocks on startup when loading reports through a shared NuGet library. Review the library and caller:

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

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on UI thread | Deadlock — UI blocked, continuation needs UI |
| Library | Missing `ConfigureAwait(false)` | Continuation posts back to captured context |
| Design | Sync wrapper around async API on UI | Forces blocking anti-pattern |
| Runtime | Works in console (no context) | Hides bug until WPF/WinForms/legacy ASP.NET |

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

**Production takeaway:** Console tutorials mask this — Karat pairs library + UI caller to test whether you fix both the blocking call site and context capture in shared code.

---

#### Q4. (P) A team wraps a legacy HTTP client that ignores `CancellationToken`. They ship this timeout helper for report downloads:

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

**Production takeaway:** Timeout via `WhenAny` improves caller responsiveness but does not cancel underlying I/O — Karat tests whether you explain the orphan-work trade-off, not just paste the pattern.

---

#### Q5. (R) Transient upstream failures are handled with a shared retry helper, but operators report exports running for minutes after a user cancels. Review:

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

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Cancellation | `catch (Exception)` swallows `OperationCanceledException` | User cancel ignored; work continues after client left |
| Async | `Task.Delay` without `ct` | Each backoff waits full duration even when token is signaled |
| Correctness | `operation()` may not receive `ct` | Inner fetch keeps running after outer cancel |
| Operability | Up to 5 attempts × multi-second delay | Minutes of wasted upstream calls post-cancel |

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

**Production takeaway:** Retry helpers that catch all exceptions silently extend request lifetime after abort — a common production incident when `RequestAborted` is wired but the retry layer ignores it.

---

#### Q6. (D) Only one report may write to a shared export folder at a time. A developer adds this gate to a singleton-registered service:

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

**Production takeaway:** Karat stacks async gate bugs — forgotten `Release` causes gradual "exports stopped working" incidents; static gates give false confidence after horizontal scale-out.

---

#### Q7. (M) A hot-path metadata lookup was optimized to return `ValueTask<int>`. After a refactor, intermittent `InvalidOperationException` appears in logs. Review:

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

**Production takeaway:** `ValueTask` micro-optimizations backfire when treated like cacheable `Task` instances — Karat tests the "consume immediately" rule from the tutorial preview, not just allocation trivia.
