---
module: 05. Language Integrated Query
difficulty: Medium
chapters: 07 Set Operations, 09 Quantifier Operations
domain: RetailCatalog
---

# Channel Catalog Sync

Build a **.NET 8 console application from scratch** that reconciles SKU feeds from web and store channels using set operators and validation quantifiers.

## Business context

Merchandising merges nightly SKU exports from two sales channels. They need union catalogs, intersection of shared SKUs, store-only exclusions, duplicate collapse by SKU key, and gate rules before publishing.

## Definitions

**Record `SkuEntry`**

- `Sku` (string)
- `Channel` (string) — `"Web"` or `"Store"`
- `ListPrice` (decimal)

**Class `SkuEqualityComparer` : `IEqualityComparer<SkuEntry>`**

- Equal when **`Sku`** matches with **`StringComparer.OrdinalIgnoreCase`**
- `GetHashCode` must align with `Equals`

**Class `ChannelCatalogSync`**

- Constructor accepts web and store sequences
- `IEnumerable<SkuEntry> DistinctSkus()` — **`DistinctBy(e => e.Sku, StringComparer.OrdinalIgnoreCase)`** (.NET 6+) or Distinct with custom comparer
- `IEnumerable<SkuEntry> UnionCatalog()` — **`Union`** with `SkuEqualityComparer`
- `IEnumerable<SkuEntry> SharedSkus()` — **`IntersectBy`** web SKU strings with store SKU strings (second arg is **key sequence**), return web entries that match — or Intersect with comparer on full entries
- `IEnumerable<SkuEntry> WebOnlySkus()` — **`Except`** store from web with comparer
- `bool CanPublish(IEnumerable<SkuEntry> catalog)` — **`catalog.Any()`** AND **`catalog.All(e => e.ListPrice > 0)`** — empty catalog fails (Any false)
- `bool MatchesManifest(IEnumerable<SkuEntry> left, IEnumerable<SkuEntry> right)` — **`SequenceEqual`** with `SkuEqualityComparer` (order matters — sort by Sku first in demo if comparing unordered feeds)

## Demo Main

1. Print distinct SKU count vs raw union count.
2. Print shared SKU codes.
3. Print web-only SKUs.
4. `CanPublish` on valid catalog → true; empty → false.
5. `All` on catalog with zero price → false.
6. SequenceEqual after both sides OrderBy Sku.

## Constraints

- net8, set + quantifier operators
- Custom comparer hash/equality contract

## Non-goals

Database sync, concurrent updates

## Evaluation

[EVALUATION.md](EVALUATION.md)
