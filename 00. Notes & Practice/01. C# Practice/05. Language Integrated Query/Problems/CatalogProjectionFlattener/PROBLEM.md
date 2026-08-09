---
module: 05. Language Integrated Query
difficulty: Hard
chapters: 08 Projection Operations, 12 Generation Operations
domain: Ecommerce
---

# Catalog Projection Flattener

Build a **.NET 8 console application from scratch** that flattens nested order lines to pick rows, generates label sequences, and handles empty catalogs safely.

## Business context

E-commerce pick sheets list every line item across orders with parent order context. Label printers need sequential bin numbers; APIs return empty typed sequences instead of null when a category has no products.

## Definitions

**Record `OrderLine`**

- `Sku` (string)
- `Quantity` (int)

**Record `CustomerOrder`**

- `OrderId` (int)
- `Customer` (string)
- `Lines` (IReadOnlyList<OrderLine>)

**Record `PickRow`**

- `OrderId` (int)
- `Customer` (string)
- `Sku` (string)
- `Quantity` (int)

**Class `CatalogProjectionFlattener`**

- Constructor accepts `IEnumerable<CustomerOrder> orders`
- `IEnumerable<IEnumerable<OrderLine>> NestedLinesOnly()` — **`Select(o => o.Lines)`** — intentionally nested for demo contrast
- `IEnumerable<PickRow> FlatPickList()` — **`SelectMany`** with **result selector** `(order, line) => new PickRow(...)`
- `IEnumerable<string> SkuTagsFlat(IEnumerable<CustomerOrder> ordersWithTags)` — assume extended demo type or separate `OrderWithTags` record with `IReadOnlyList<string> Tags`; SelectMany tags with order id in anonymous projection
- `IEnumerable<int> BinLabels(int start, int count)` — **`Enumerable.Range(start, count)`** mapped with **`Select`**
- `IEnumerable<PickRow> LinesForCategory(string category, IEnumerable<(string Category, PickRow Row)> taggedRows)` — filter category; if none, return **`Enumerable.Empty<PickRow>()`** not null
- `IEnumerable<PickRow> TopPickRows(int n)` — **`OrderByDescending` Quantity** on flat list then **`Take(n)`**

**Record `OrderWithTags`** (optional helper)

- `OrderId`, `Tags` (`IReadOnlyList<string>`)

## Demo Main

1. Show `NestedLinesOnly` count vs flat row count (nested deeper).
2. Print all `FlatPickRow` SKUs with OrderId.
3. Print bin labels from Range(1, 5).
4. `LinesForCategory` for missing category returns empty enumeration (foreach prints nothing, no throw).
5. Print top 3 pick rows by quantity.

## Constraints

- net8, Select vs SelectMany distinction
- Range count parameter is **count**, not end index

## Non-goals

XML, joins

## Evaluation

[EVALUATION.md](EVALUATION.md)
