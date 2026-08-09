---
module: 06. Multithreading & Async Programming
difficulty: Medium
chapters: 01 Threads and Thread Lifecycle
domain: LogisticsScanning
---

# Shipment Scan Station

Build a **.NET 8 console application from scratch** that simulates multiple warehouse scan stations using dedicated `Thread` workers, cooperative shutdown, and thread-local logging.

## Business context

A fulfillment center runs several independent scan stations. Each station processes a batch of boxes for one shipment. The orchestrator starts one thread per shipment, waits for completion, collects results from shared storage, and can signal all workers to stop early during a drill.

## Definitions

**Record `ScanJob`**

- `ShipmentId` (string, non-empty)
- `BoxCount` (int > 0)
- `MillisecondsPerBox` (int ≥ 1) — simulated work delay

**Record `ScanResult`**

- `ShipmentId`, `Destination` (string), `BoxesProcessed` (int), `ElapsedMs` (int), `WorkerThreadId` (int), `ScanLog` (string)

**Class `ThreadLocalScanLogger`**

- Wraps `ThreadLocal<StringBuilder>` — each thread appends `"[{ShipmentId}] box {n}\n"` lines
- `void Append(string shipmentId, int boxNumber)`
- `string DrainForCurrentThread()` — returns accumulated log and clears current thread's buffer

**Class `ScanStationOrchestrator`**

- Constructor `(IReadOnlyList<ScanJob> jobs, IReadOnlyDictionary<string, string> shipmentDestinations)` — maps shipment id → destination; null checks
- `void StartAll(CancellationToken cancellationToken)` — one `Thread` per job with `ParameterizedThreadStart`; thread name `"Scan-{ShipmentId}"`; **background** threads; pass `(ScanJob, CancellationToken)` as state tuple/object
- `bool WaitAll(int timeoutMs)` — `Join(timeoutMs)` each thread; return `true` only if **all** threads finished within timeout
- `IReadOnlyList<ScanResult> GetResults()` — snapshot of completed results (thread-safe read after join)
- Worker body: loop boxes 1..BoxCount; check `cancellationToken` each iteration; `Thread.Sleep(MillisecondsPerBox)`; on cancel break; append to thread-local logger; add `ScanResult` to internal list under **lock** (preview ch06)

**Static factory on `ScanStationOrchestrator`**

- `static ScanStationOrchestrator Create(IReadOnlyList<ScanJob> jobs, IReadOnlyDictionary<string, string> destinations)` — reject null/empty jobs or missing destination for any shipment id

## Demo Main

1. Create 3 scan jobs and destination map.
2. Start all workers; wait with 30s timeout.
3. Print each result (thread id, boxes processed, log snippet).
4. Run again with `CancellationTokenSource`, cancel after ~100ms, show partial results.

## Constraints

- net8, explicit usings, no `Task`/`async` in this project (Thread only)
- Cooperative cancellation only — no `Thread.Abort`
- Results list mutations protected by one dedicated lock object

## Non-goals

ThreadPool, Task, real hardware scanners

## Evaluation

[EVALUATION.md](EVALUATION.md)
