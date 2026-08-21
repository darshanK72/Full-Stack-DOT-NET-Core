# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/06. Multithreading & Async Programming/02. ThreadPool/`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly invoice import queues validation onto the thread pool but reports wrong counts in production (sometimes all zeros). Review this service method. What fails under load, and how do you fix it in priority order?

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

---

#### Q2. (R) A legacy COM-aware host copied the tutorial's `ManualResetEvent` + `WaitHandle.WaitAll` pattern for large batches. Review this batch runner used with `jobCount = 500`:

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

---

#### Q3. (R) After a refactor, an audit pipeline starves under concurrent load — other timers and `Task.Run` work stops progressing. Review the pool callback:

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

---

#### Q4. (P) Every microservice instance calls this at startup in `Program.cs` to "avoid cold-start latency" after deploy:

```csharp
ThreadPool.SetMinThreads(workerMin: 250, ioMin: 250);
ThreadPool.GetMinThreads(out int wMin, out int ioMin);
Console.WriteLine($"Pool min threads: {wMin} workers / {ioMin} I/O");
```

The fleet runs 40 pods on 8-core nodes. What goes wrong in production, and when is `SetMinThreads` actually appropriate?

---

#### Q5. (M) During a traffic spike, dashboards show `GetAvailableThreads` reporting very few free worker threads, but CPU is only ~35%. A teammate concludes "we need more cores." Given this monitoring snippet from a pool callback, what is the more likely root cause?

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

---

#### Q6. (D) A thumbnail service receives bursts of 2,000 independent resize jobs per upload batch (~50 ms CPU each). Two proposals:

**A)** `new Thread(...).Start()` per job, `Join` at the end of the batch  
**B)** `ThreadPool.QueueUserWorkItem` (or `Task.Run`) with `CountdownEvent` to wait for completion  

Compare throughput, memory, and operational risk. Which do you ship, and when would you still choose manual `Thread`?

---

#### Q7. (R) Pool callbacks silently drop failures in production — support sees partial imports with no error logs. Review this aggregation helper:

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
