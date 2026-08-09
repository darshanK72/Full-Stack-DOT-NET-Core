---
module: 06. Multithreading & Async Programming
difficulty: Hard
chapters: 03 Tasks and Task Parallel Library
domain: WarehouseFulfillment
---

# Warehouse Order Coordinator

Build a **.NET 8 console application from scratch** coordinating order validation, picking, and payment callback bridging with `Task`, `Task.WhenAll`, continuations, and `TaskCompletionSource<T>`.

## Business context

E-commerce orders flow validate → pick lines → await payment gateway callback. The coordinator runs stages with TPL primitives, caps concurrent picks with `SemaphoreSlim`, and surfaces faults from child tasks.

## Definitions

**Record `Order`**

- `OrderId` (int), `Customer` (string), `Total` (decimal > 0), `Lines` (`IReadOnlyList<string>`, count ≥ 1)

**Record `PaymentResult`**

- `OrderId`, `Status` (string), `TransactionId` (string)

**Static class `OrderStages`**

- `static Task ValidateAsync(Order order, CancellationToken ct)` — delay 30ms; throw if `Total <= 0`
- `static Task<IReadOnlyList<string>> PickLinesAsync(Order order, SemaphoreSlim gate, CancellationToken ct)` — `await gate.WaitAsync(ct)`; delay 20ms per line; release gate in `finally`
- `static void SimulateGatewayCallback(TaskCompletionSource<PaymentResult> tcs, PaymentResult result, int delayMs)` — `Task.Run` delays then `TrySetResult`

**Class `OrderCoordinator`**

- Constructor `(SemaphoreSlim pickGate, int maxConcurrentPicks)` — stores gate (may ignore max if gate injected)
- `Task RunPipelineAsync(Order order, CancellationToken ct)` — validate then pick; return picked lines count summary via side effect or tuple stored
- `Task<IReadOnlyList<Order>> ValidateAllAsync(IEnumerable<Order> orders, CancellationToken ct)` — `Task.WhenAll`; return only orders that validated (catch/log faults per order optional: failed orders excluded)
- `Task<PaymentResult> AwaitPaymentAsync(int orderId, TaskCompletionSource<PaymentResult> tcs, CancellationToken ct)` — `await tcs.Task` with ct registered to cancel wait
- `Task<int> RunWithContinuationAsync(Order order, CancellationToken ct)` — `Task.Run` validate+pick; `ContinueWith` on `OnlyOnRanToCompletion` returning line count; faulted continuation returns `-1`

## Demo Main

1. Three sample orders; `SemaphoreSlim(2)`.
2. `ValidateAllAsync`; print surviving count.
3. `RunWithContinuationAsync` on one order; print line count.
4. Payment: create `TaskCompletionSource<PaymentResult>`, start `AwaitPaymentAsync`, fire callback after 100ms; print transaction id.

## Constraints

- net8, explicit usings; prefer `Task.Run` over `Task.Factory.StartNew`
- Register cancellation on TCS wait
- `ConfigureAwait` not required in console app

## Non-goals

async/await throughout (mix Task and async where specified)

## Evaluation

[EVALUATION.md](EVALUATION.md)
