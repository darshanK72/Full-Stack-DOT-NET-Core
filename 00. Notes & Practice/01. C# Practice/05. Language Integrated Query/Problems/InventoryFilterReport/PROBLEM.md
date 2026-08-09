---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 02 Filtering & Aggregation
domain: Warehouse
---

# Inventory Filter Report

Build a **.NET 8 console application from scratch** that filters mixed catalog entries and computes category revenue statistics with LINQ aggregations.

## Business context

A warehouse catalog feed mixes product rows and audit note strings in one list. Operations needs category revenue totals, low-stock counts, and a custom fold for weighted average cost without loading everything into SQL.

## Definitions

**Record `ProductRow`**

- `Sku` (string)
- `Category` (string)
- `UnitCost` (decimal > 0)
- `QuantityOnHand` (int >= 0)

**Class `InventoryFilterReport`**

- Constructor accepts `IEnumerable<object> mixedFeed` — may contain `ProductRow` and `string` audit lines
- `IEnumerable<ProductRow> ProductsOnly()` — **`OfType<ProductRow>`**
- `IEnumerable<ProductRow> LowStock(int threshold)` — `QuantityOnHand <= threshold`
- `int CountInCategory(string category)` — prefer **`Count(predicate)`** over `Where().Count()`
- `decimal TotalInventoryValue()` — `Sum` of `UnitCost * QuantityOnHand` for all products
- `decimal? AverageUnitCost(string category)` — `Average` on matching category; return **`null`** when category has zero products (do not throw)
- `decimal WeightedAverageCost()` — **`Aggregate`** with seed `0m` and accumulator `(running, p) => running + p.UnitCost * p.QuantityOnHand`, then divide by total quantity; return `0m` when no products

**Static class `InventoryDemoData`**

- `IEnumerable<object> MixedFeed()` — at least 2 strings + 5 `ProductRow` across 2 categories

## Demo Main

1. Print count from `ProductsOnly()` vs total mixed feed count.
2. Print SKUs from `LowStock(5)`.
3. Print `TotalInventoryValue` formatted as currency.
4. Print `AverageUnitCost` for a category with data and one with none (`null`).
5. Print `WeightedAverageCost`.
6. Chain: `ProductsOnly().Where(p => p.Category == "Hardware").Sum(...)` for one category subtotal.

## Constraints

- net8, explicit usings, LINQ for all filtering/aggregation in report class
- Guard empty averages — no `InvalidOperationException` from `Average()` on empty

## Non-goals

Persistence, `SelectMany`, grouping

## Evaluation

[EVALUATION.md](EVALUATION.md)
