---
module: 06. Multithreading & Async Programming
difficulty: Hard
chapters: 07 Concurrent Collections
domain: WarehousePicking
---

# Pick Ticket Buffer

Build a **.NET 8 console application from scratch** using concurrent collections for a warehouse pick pipeline: SKU price cache, producer/consumer pick tickets, and parallel result aggregation.

## Business context

Pick requests arrive from multiple aisles. A price cache avoids repeated lookups. Pickers consume tickets from a bounded buffer; completed picks land in a concurrent bag for reporting.

## Definitions

**Record `PickTicket`**

- `TicketId` (int), `Sku` (string), `Quantity` (int > 0)

**Record `PickResult`**

- `TicketId`, `Sku`, `PickedQuantity`, `WorkerName` (string)

**Class `SkuPriceCache`**

- Wraps `ConcurrentDictionary<string, decimal>` (ordinal ignore case)
- `decimal GetOrAddPrice(string sku, Func<string, decimal> factory)` — delegate to `GetOrAdd`
- `bool TryApplySurcharge(string sku, decimal multiplier)` — `AddOrUpdate` multiply existing or skip if missing (return false if key absent)

**Class `PickTicketBuffer`**

- `BlockingCollection<PickTicket>` with bounded capacity constructor
- `void Enqueue(PickTicket ticket, CancellationToken ct)` — `Add` with cancellation
- `bool TryDequeue(out PickTicket? ticket, TimeSpan timeout)` — `TryTake`
- `void CompleteAdding()` — signal no more tickets
- `int RemainingCount` — approximate queued count

**Class `PickFloorCoordinator`**

- Constructor `(SkuPriceCache cache, PickTicketBuffer buffer, int pickerCount)`
- `Task RunAsync(CancellationToken ct)` — start `pickerCount` `Task.Run` consumers; each dequeues until complete; simulate pick delay 25ms; add `PickResult` to `ConcurrentBag`; lookup price via cache
- `IReadOnlyList<PickResult> GetResults()` — bag snapshot via `ToArray()`
- Producer helper `static void FeedTickets(PickTicketBuffer buffer, IEnumerable<PickTicket> tickets, CancellationToken ct)`

## Demo Main

1. Seed cache with factory simulating DB lookup (delay 10ms on miss).
2. Feed 20 tickets into buffer (capacity 5).
3. Run 3 pickers; print result count and distinct SKUs priced.
4. Apply surcharge on one SKU; show updated cache value.

## Constraints

- net8, explicit usings
- Use `ConcurrentDictionary`, `BlockingCollection`, `ConcurrentBag` — not lock+List for hot paths
- Compound read-modify on cache must use `AddOrUpdate` / `TryUpdate`

## Non-goals

Real WMS integration, async streams

## Evaluation

[EVALUATION.md](EVALUATION.md)
