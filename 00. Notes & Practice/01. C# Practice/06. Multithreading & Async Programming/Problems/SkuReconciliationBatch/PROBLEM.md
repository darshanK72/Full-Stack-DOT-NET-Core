---
module: 06. Multithreading & Async Programming
difficulty: Medium
chapters: 05 Parallel Programming
domain: InventoryReconciliation
---

# SKU Reconciliation Batch

Build a **.NET 8 console application from scratch** that reconciles warehouse stock records in parallel using `Parallel.For`, thread-local aggregation, `ParallelOptions`, and PLINQ.

## Business context

End-of-day reconciliation multiplies on-hand quantity by an adjustment factor for thousands of SKUs. CPU-bound math should scale across cores without corrupting the running total.

## Definitions

**Class `StockRecord`**

- `Sku` (string), `OnHand` (int ≥ 0), `AdjustmentFactor` (decimal > 0)
- Read-only `ReconciledValue => OnHand * AdjustmentFactor`

**Static class `ReconciliationEngine`**

- `static decimal SequentialTotal(IReadOnlyList<StockRecord> records)` — sum ReconciledValue
- `static decimal ParallelForTotal(IReadOnlyList<StockRecord> records, int maxDegree)` — `Parallel.For` with thread-local decimal sum merged under `lock` (or provided `ThreadSafeDecimal.Add` helper)
- `static decimal ParallelForEachWithOptions(IReadOnlyList<StockRecord> records, int maxDegree, CancellationToken ct)` — `Parallel.ForEach` + `ParallelOptions { MaxDegreeOfParallelism = maxDegree, CancellationToken = ct }`
- `static IOrderedEnumerable<StockRecord> TopSkusByValue(IReadOnlyList<StockRecord> records, int topN)` — PLINQ `AsParallel().OrderByDescending(r => r.ReconciledValue).Take(topN).AsOrdered()`

**Class `ReconciliationReport`**

- `void CompareMethods(IReadOnlyList<StockRecord> records)` — print sequential vs parallel totals (must match within 0.01m) and top 3 SKUs

## Demo Main

1. Build list of ≥ 50 `StockRecord` entries.
2. `CompareMethods` with `maxDegree = 2` and with default parallelism.
3. Run parallel foreach with token cancelled after 1ms on large list — handle `OperationCanceledException`.

## Constraints

- net8, explicit usings, `decimal` for money
- No `async`/`await` in this project
- Thread-local aggregation preferred over locking every iteration

## Non-goals

Real database I/O, Parallel.Invoke (optional mention in scenarios)

## Evaluation

[EVALUATION.md](EVALUATION.md)
