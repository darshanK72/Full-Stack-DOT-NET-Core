---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 03 Ordering
domain: Warehouse
---

# Warehouse Pick Sorter

Build a **.NET 8 console application from scratch** that sorts pick tickets with multi-key ordering and a custom zone comparer.

## Business context

Fulfillment prints pick lists sorted by warehouse zone then aisle bin. Zone labels like `A-12` and `A-3` must sort numerically within the same letter prefix, not lexically as strings.

## Definitions

**Record `PickTicket`**

- `TicketId` (int)
- `Zone` (string) — e.g. `"B-04"`, `"A-12"`
- `Priority` (int) — lower number = higher priority
- `Sku` (string)

**Class `ZoneComparer` : `IComparer<string>`**

- Parse `"Letter-Number"` zones; compare letter case-insensitively, then numeric suffix
- Malformed zones (no hyphen) fall back to `StringComparer.OrdinalIgnoreCase`

**Class `WarehousePickSorter`**

- Constructor accepts `IEnumerable<PickTicket> tickets`
- `IEnumerable<PickTicket> ByZoneThenPriority()` — `OrderBy` zone with `ZoneComparer`, **`ThenBy` Priority**, **`ThenBy` Sku**
- `IEnumerable<PickTicket> ByPriorityDescending()` — `OrderByDescending` Priority only
- `IEnumerable<PickTicket> ReverseOriginal()` — **`Reverse()`** on source order (not OrderByDescending)
- `IEnumerable<PickTicket> QuerySyntaxSort()` — query syntax `orderby Zone, Priority, Sku` using same zone logic via pre-projection or comparer in method syntax only for zone (query syntax has no comparer — document in comment)

## Demo Main

1. Seed tickets where `A-3` must appear before `A-12` when sorted by zone.
2. Print `ByZoneThenPriority` ticket ids in order.
3. Print `ByPriorityDescending` first ticket id.
4. Print `ReverseOriginal` vs source insertion order.
5. Demonstrate that a **second `OrderBy(Priority)`** without ThenBy would break zone sort — comment only or optional debug print.

## Constraints

- net8, LINQ ordering operators
- Multi-key sort must use **ThenBy**, not chained OrderBy

## Non-goals

PLINQ, EF Core SQL translation

## Evaluation

[EVALUATION.md](EVALUATION.md)
