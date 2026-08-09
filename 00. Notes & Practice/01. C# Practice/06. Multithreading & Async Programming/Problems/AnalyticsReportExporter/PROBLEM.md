---
module: 06. Multithreading & Async Programming
difficulty: Hard
chapters: 04 Async and Await
domain: AnalyticsReporting
---

# Analytics Report Exporter

Build a **.NET 8 console application from scratch** for a simulated report export pipeline using `async`/`await`, cooperative cancellation, retry with backoff, and an exclusive write slot (`SemaphoreSlim` only-one pattern).

## Business context

A analytics service fetches report metadata, processes rows, retries flaky API calls, and writes exports through a single-writer slot so two exports never corrupt the same file handle.

## Definitions

**Record `ReportMetadata`**

- `ReportId` (string), `RowCount` (int ≥ 0)

**Static class `ReportPipeline`**

- `static async Task<ReportMetadata> FetchMetadataAsync(string reportId, CancellationToken ct)` — `Task.Delay(50, ct)`; return metadata with 4 rows
- `static async Task ProcessReportAsync(ReportMetadata metadata, CancellationToken ct)` — delay 40ms; no return
- `static async Task<int> CountRowsAsync(int rowCount, CancellationToken ct)` — delay 30ms; return rowCount
- `static async Task<string> UnreliableFetchAsync(string reportId, CancellationToken ct)` — fails first `failCount` attempts with `InvalidOperationException`; succeeds afterward (accept `failCount` parameter)
- `static async Task<string> FetchWithRetryAsync(string reportId, int maxAttempts, CancellationToken ct)` — exponential backoff starting 50ms; rethrow after max attempts

**Class `ExclusiveWriteGate`**

- Wraps `SemaphoreSlim(1,1)`
- `async Task<T> RunExclusiveAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)` — `WaitAsync`; try/finally `Release`
- `static ExclusiveWriteGate Shared { get; }` — singleton for demo

**Class `ReportExportService`**

- `async Task<int> ExportAsync(string reportId, CancellationToken ct)` — fetch metadata → process → count rows → `FetchWithRetryAsync` → exclusive write (return row count from write delegate)
- `async Task RunParallelExportsAsync(string[] reportIds, CancellationToken ct)` — start two exports concurrently via `Task.WhenAll`; both must use exclusive gate (serialized writes)

## Demo Main

1. Single export; print row count.
2. Retry demo: `FetchWithRetryAsync` with unreliable API (fail twice).
3. Parallel two exports; print total elapsed showing serialization at write gate.
4. Cancel mid-export with `CancellationTokenSource.CancelAfter(60ms)`.

## Constraints

- net8, explicit usings, `Async` suffix on async methods
- No `.Result` or `.Wait()` on tasks
- `ConfigureAwait(false)` optional in console

## Non-goals

Real HTTP, IAsyncEnumerable (preview only in scenarios)

## Evaluation

[EVALUATION.md](EVALUATION.md)
