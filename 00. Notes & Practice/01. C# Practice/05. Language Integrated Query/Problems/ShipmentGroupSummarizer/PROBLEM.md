---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 04 Grouping, 10 Conversion Operations
domain: Logistics
---

# Shipment Group Summarizer

Build a **.NET 8 console application from scratch** that groups shipment lines by carrier and priority, builds summary rows, and materializes a lookup for assignee queues.

## Business context

Logistics dashboards show per-carrier totals and priority breakdowns. Dispatchers need instant lookup of all shipments for an assignee without re-scanning the full list each time.

## Definitions

**Record `ShipmentLine`**

- `ShipmentId` (int)
- `Carrier` (string)
- `Priority` (int) — 1=Critical, 2=Standard, 3=Economy
- `WeightKg` (decimal)
- `Assignee` (string)

**Record `CarrierSummary`**

- `Carrier` (string)
- `LineCount` (int)
- `TotalWeightKg` (decimal)

**Class `ShipmentGroupSummarizer`**

- Constructor accepts `IEnumerable<ShipmentLine> lines`
- `IEnumerable<IGrouping<string, ShipmentLine>> ByCarrier()` — deferred **`GroupBy` Carrier**
- `IEnumerable<CarrierSummary> CarrierSummaries()` — GroupBy with **result selector**: `(carrier, items) => new CarrierSummary(...)` with Count and Sum WeightKg
- `IEnumerable<CarrierSummary> QuerySyntaxSummaries()` — query syntax `group … by Carrier into g` projecting summary
- `ILookup<string, ShipmentLine> AssigneeLookup()` — immediate **`ToLookup`** on Assignee (case-sensitive default)
- `Dictionary<string, decimal> CarrierWeightDictionary()` — **`ToDictionary`** Carrier → TotalWeightKg; **throws if duplicate carrier keys would occur** (should not with GroupBy first — group then ToDictionary)
- `IEnumerable<ShipmentLine> CriticalInCarrier(string carrier)` — nested filter: group by carrier, pick group, Where Priority==1

## Demo Main

1. Print each carrier group key and member count from `ByCarrier()`.
2. Print `CarrierSummaries` formatted table.
3. Index `AssigneeLookup()["Alex"]` — print shipment ids (missing assignee → empty sequence, no throw).
4. Print one entry from `CarrierWeightDictionary`.
5. Compare deferred `ByCarrier()` count vs lookup Count after source list gains a new line (lookup stale until rebuilt).

## Constraints

- net8, LINQ GroupBy, ToLookup, ToDictionary
- Case-sensitive carrier keys unless noted

## Non-goals

GroupJoin, SQL

## Evaluation

[EVALUATION.md](EVALUATION.md)
