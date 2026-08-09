---
module: 04. Functional Style Programming
difficulty: Medium
chapters: 02 Lambda Expressions
domain: RetailCatalog
---

# Catalog Pricing Engine

Build a **.NET 8 console application from scratch** that transforms and filters catalog prices using custom delegates assigned from lambdas and method groups.

## Business context

A retail merchandising team runs promotional pricing passes over SKU list prices: apply percentage discounts, strip items below a floor, and preview margin-safe prices before publishing to the storefront.

## Definitions

**Delegate `PriceTransform`**

```csharp
public delegate decimal PriceTransform(decimal listPrice);
```

**Delegate `PriceFilter`**

```csharp
public delegate bool PriceFilter(decimal price);
```

**Record `CatalogItem`**

- `Sku` (string)
- `Name` (string)
- `ListPrice` (decimal > 0)

**Class `PriceCatalog`**

- Private `List<CatalogItem> _items`
- Constructor seeds items (demo may construct externally and pass in, or add via `AddItem`)
- `void AddItem(CatalogItem item)` — reject null or non-positive list price
- `IReadOnlyList<decimal> ApplyToAll(PriceTransform transform)` — invoke transform on each item's `ListPrice`; return new list of results (order preserved)
- `IReadOnlyList<CatalogItem> FilterPrices(PriceFilter filter)` — return items whose `ListPrice` passes filter
- `decimal ApplyPipeline(decimal startPrice, params PriceTransform[] steps)` — fold transforms left-to-right; reject null steps array or null entry inside array

**Static class `PriceTransforms`**

- `PriceTransform TenPercentOff` — method group target: `listPrice => listPrice * 0.90m` as named method `TenPercentOff(decimal listPrice)`
- `PriceTransform RoundToCents` — expression-bodied method rounding to 2 decimals (`MidpointRounding.AwayFromZero`)

**Static class `PriceFilters`**

- `PriceFilter AboveMinimum(decimal minimum)` — returns a **statement lambda** (block body with `{ return ...; }`) testing `price >= minimum`

## Demo Main

1. Seed catalog with at least 4 items spanning prices `4.99m`–`49.99m`.
2. Apply `TenPercentOff` then `RoundToCents` via `ApplyPipeline` on one price; print before/after.
3. Build filter with `AboveMinimum(10m)` (statement lambda factory); print SKUs that survive `FilterPrices`.
4. Assign an **expression lambda** to `PriceTransform` inline (e.g. `p => p + 1m`) and run `ApplyToAll`; print transformed prices.
5. Demonstrate one **statement lambda** `PriceTransform` (block with local variable) for a flat `$2` markdown with floor at `$0.01`.

## Constraints

- net8, explicit usings, `decimal` for money
- At least one expression lambda and one statement lambda in your solution
- Manual loops in `ApplyToAll` / `FilterPrices` — no LINQ operators

## Non-goals

Persistence, concurrent updates, expression trees

## Evaluation

[EVALUATION.md](EVALUATION.md)
