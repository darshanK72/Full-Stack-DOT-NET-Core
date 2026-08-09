---
module: 04. Functional Style Programming
difficulty: Hard
chapters: 05 Func Action and Predicate
domain: Warehouse
---

# Warehouse Pick Processor

Build a **.NET 8 console application from scratch** that filters, transforms, and reports warehouse inventory using `Func`, `Action`, and `Predicate` with BCL list helpers.

## Business context

Warehouse pickers pull active SKUs with sufficient stock for today's orders. The inventory service removes discontinued lines, computes pick totals, and prints pick confirmations through injected callbacks.

## Definitions

**Record `Product`**

- `Sku` (string, non-empty)
- `Name` (string)
- `UnitPrice` (decimal > 0)
- `StockQty` (int ≥ 0)
- `IsActive` (bool)

**Class `InventoryBatch`**

- Wraps `List<Product>` (constructor copies incoming list defensively)
- `IReadOnlyList<Product> Items` — read-only view of current list
- `List<Product> FindActive(Predicate<Product> match)` — `List.FindAll` where `IsActive && match(p)`; null predicate throws
- `void ForEachLine(Action<Product> action)` — `List.ForEach`; **null-safe**: skip silently if action is null
- `int RemoveInactive(Predicate<Product> shouldRemove)` — `List.RemoveAll` removing items where `!IsActive || shouldRemove(p)`; returns removed count
- `decimal ProcessInventory(Predicate<Product> pickFilter, Func<Product, decimal> lineTotal, Action<string> report)`:
  1. Validate non-null `pickFilter` and `lineTotal`; `report` may be null
  2. Filter with `FindActive(pickFilter)`
  3. Sum `lineTotal(p)` for filtered items (manual loop)
  4. For each picked item, null-safe invoke `report` with `"PICK {Sku} qty {StockQty}"`
  5. Return total pick value

**Static class `ProductPredicates`**

- `Predicate<Product> MinimumStock(int minQty)` — returns lambda `p => p.StockQty >= minQty`
- `Predicate<Product> SkuPrefix(string prefix)` — case-insensitive `StartsWith`

**Static class `ProductCalculators`**

- `Func<Product, decimal> LineValue` — method group: `(Product p) => p.UnitPrice * p.StockQty`

## Demo Main

1. Seed batch with mix of active/inactive and varied stock.
2. `RemoveInactive` with always-false extra predicate; print removed count.
3. `ProcessInventory` with `MinimumStock(5)` and `LineValue`, plus report Action that prints messages.
4. Call `ForEachLine(null)` — must not throw.
5. Demonstrate `FindActive` combining active flag with `SkuPrefix("WH-")`.

## Constraints

- net8, explicit usings, `decimal` for money
- Use `List<T>.FindAll`, `ForEach`, `RemoveAll` as specified
- No LINQ operators elsewhere (manual sum in ProcessInventory)

## Non-goals

Persistence, threading, custom delegate types for filters (use Predicate/Func/Action)

## Evaluation

[EVALUATION.md](EVALUATION.md)
