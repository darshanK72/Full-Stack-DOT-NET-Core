---
module: 06. Multithreading & Async Programming
difficulty: Medium
chapters: 02 ThreadPool
domain: FinanceAudit
---

# Audit Line Processor

Build a **.NET 8 console application from scratch** that validates thousands of invoice lines using `ThreadPool.QueueUserWorkItem`, `WaitCallback`, and `CountdownEvent` for batch completion.

## Business context

An overnight import produces invoice lines that must be checksum-validated before posting. Each line is independent CPU work. The processor queues all lines on the thread pool, tracks pass/fail in a pre-sized array indexed by line id, and reports throughput.

## Definitions

**Record `AuditLineJob`**

- `LineId` (int, 1-based index into results array)
- `WorkUnits` (int ≥ 1) — iterations of deterministic hash work

**Static class `LineValidation`**

- `static int Validate(AuditLineJob job)` — deterministic pseudo-hash loop over `WorkUnits`; return `0` if `LineId % 10 == 0` else `1` (fail every 10th line)
- `static int CountPassed(int[] results)` — count entries equal to `1`

**Class `AuditBatchProcessor`**

- Constructor `(int batchSize, int workUnitsPerLine)` — pre-allocates `int[batchSize + 1]` results (index 0 unused)
- `void QueueAll()` — for each line 1..batchSize, `ThreadPool.QueueUserWorkItem` with state carrying job + shared results array; callback must cast state safely
- `void WaitForCompletion()` — uses `CountdownEvent` initialized to batch size; each callback signals once; main thread waits
- `int PassedCount { get; }` — after wait, computed via `LineValidation.CountPassed`
- `bool AllCallbacksUsedPoolThread { get; }` — set true only if every callback observed `Thread.CurrentThread.IsThreadPoolThread`

**Class `ThroughputReport`**

- `static void PrintComparison(int batchSize, int workUnits, TimeSpan poolElapsed, TimeSpan? manualThreadElapsed)` — prints batch size, passed count, pool ms; optional manual comparison line

## Demo Main

1. Print `ThreadPool.GetMinThreads` / `GetMaxThreads` / `GetAvailableThreads` for worker and I/O.
2. Run batch of 120 lines via `AuditBatchProcessor`; measure with `Stopwatch`.
3. Print passed count and confirm all callbacks ran on pool threads.

## Constraints

- net8, explicit usings
- Use `WaitCallback` + `QueueUserWorkItem` — not `Task.Run`
- `CountdownEvent` for completion (not `ManualResetEvent` per item)

## Non-goals

Task API, changing pool min/max threads at runtime

## Evaluation

[EVALUATION.md](EVALUATION.md)
