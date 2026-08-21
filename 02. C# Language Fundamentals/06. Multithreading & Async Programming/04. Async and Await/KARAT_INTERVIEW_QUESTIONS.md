# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/04. Async and Await/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
