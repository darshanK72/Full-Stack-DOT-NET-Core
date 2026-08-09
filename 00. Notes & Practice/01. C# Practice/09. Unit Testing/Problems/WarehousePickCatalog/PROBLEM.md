---
module: 09. Unit Testing
difficulty: Medium
chapters: 02 xUnit
domain: WarehouseOperations
---

# Warehouse Pick Catalog

Build a **.NET 8 solution** with a shared SKU catalog fixture and xUnit **class** and **collection** fixtures.

## Business context

Warehouse pick lists pull SKU metadata from a catalog loaded once per test run. Multiple test classes share the same catalog without reloading JSON or hitting a database in every test constructor.

## Solution layout

```
WarehousePickCatalog/
  Program.cs
  WarehousePickCatalog.Tests/
    Fixtures/
      SkuCatalogFixture.cs           ← implement TODO seed data
      SkuCatalogCollection.cs        ← CollectionDefinition
    PickListClassFixtureTests.cs
    PickListCollectionFixtureTestsA.cs
    PickListCollectionFixtureTestsB.cs
```

## Definitions (production)

**Class `SkuItem`:** `Sku`, `Description`, `PickZone` (all strings, non-empty when valid)

**Class `SkuCatalog`**

- Constructor loads nothing — use `Seed(IEnumerable<SkuItem> items)` to populate internal dictionary by SKU
- `bool TryGet(string sku, out SkuItem? item)`
- `int Count { get; }`

**Class `PickListBuilder`**

- Constructor `(SkuCatalog catalog)`
- `IReadOnlyList<string> BuildPickPath(IEnumerable<string> skus)` — returns pick zones in request order; skip unknown SKUs; empty input → empty list

## Fixtures (tests)

**`SkuCatalogFixture`** — implements seeding with at least 3 SKUs in `SeedCatalog()` called from fixture ctor

**`SkuCatalogCollection`** — `[CollectionDefinition("SkuCatalog")]` + `ICollectionFixture<SkuCatalogFixture>`

**PickListClassFixtureTests** — `IClassFixture<SkuCatalogFixture>`; test `BuildPickPath_ReturnsZonesForKnownSkus`

**PickListCollectionFixtureTestsA/B** — both `[Collection("SkuCatalog")]`; each has test asserting same `catalog.Count` from shared fixture

## Demo Main

Seed 3 SKUs; print pick path for two known SKUs.

## Constraints

- net8, explicit usings
- Fixture classes in Tests project may reference production types

## Non-goals

Moq, MSTest, file I/O for catalog load

## Evaluation

[EVALUATION.md](EVALUATION.md)
