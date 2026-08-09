---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 01 Introduction to LINQ, 10 Conversion Operations
domain: RetailSales
---

# Sales Pipeline Analyzer

Build a **.NET 8 console application from scratch** that composes LINQ pipelines over in-memory sales lines and demonstrates deferred vs immediate execution.

## Business context

Regional sales analysts preview daily order lines before exporting to finance. They filter by region, project lightweight DTOs, and must understand when a query actually runs versus when it only builds a recipe.

## Definitions

**Record `SalesLine`**

- `OrderId` (int)
- `Region` (string)
- `Amount` (decimal > 0)

**Class `SalesPipelineAnalyzer`**

- Constructor accepts `IEnumerable<SalesLine> source` — store reference (do not copy unless materializing)
- `IEnumerable<SalesLine> FilterByRegion(string region)` — case-insensitive region match; **deferred** (return LINQ query, not `ToList`)
- `IEnumerable<decimal> SelectAmounts()` — project amounts only; deferred
- `int CountExecutedQueries()` — returns how many times the underlying source was **enumerated** by this analyzer's queries (increment a counter in a wrapper or side-effect `Select`; demo uses a counting enumerable)
- `IReadOnlyList<SalesLine> MaterializeRegion(string region)` — filter then **`ToList`** immediately
- `decimal TotalForRegion(string region)` — terminal `Sum` on filtered amounts for one region
- `IEnumerable<SalesLine> QuerySyntaxRegion(string region)` — **same filter as `FilterByRegion`** but written with **query syntax** (`from … where … select`)

**Static class `SalesDemoData`**

- `IEnumerable<SalesLine> SampleLines()` — yields at least 6 lines across 3 regions with mixed amounts

## Demo Main

1. Build analyzer from `SampleLines()` wrapped in a counting enumerable that increments on each `MoveNext`.
2. Build deferred pipeline: `FilterByRegion("east").SelectAmounts()` — **do not enumerate yet**; print "Pipeline built".
3. Call `TotalForRegion("east")` — print total and `CountExecutedQueries`.
4. Enumerate the same deferred pipeline from step 2 with `foreach`; print amounts and updated execute count (proves second enumeration).
5. Compare `MaterializeRegion("west")` count vs deferred filter count without double-charging if student snapshots correctly.
6. Show `QuerySyntaxRegion("east")` returns same count as method syntax for same seed.

## Constraints

- net8, explicit usings, `using System.Linq`
- Use LINQ extension methods / query syntax — no manual `foreach` filters in analyzer methods (Main may use `foreach` for display)
- `decimal` for money

## Non-goals

Database providers, `IQueryable`, PLINQ

## Evaluation

[EVALUATION.md](EVALUATION.md)
