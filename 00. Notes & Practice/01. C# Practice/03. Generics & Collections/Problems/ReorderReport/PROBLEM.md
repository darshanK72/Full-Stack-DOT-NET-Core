---
module: 03. Generics & Collections
difficulty: Medium
chapters: 07 SortedList & SortedDictionary
domain: Warehouse
---

# Reorder Report

Build a **.NET 8 console application from scratch** for SKU-ordered reorder quantities using sorted associative collections.

## Business context

Procurement prints nightly reorder lines in SKU order without an explicit sort step. Some reports need row index access; regional sales maps update often and need efficient inserts.

## Definitions

**Class `ReorderReport`** (uses `SortedList<string, int>`)

- `void SetLine(string sku, int quantity)` — indexer set
- `bool TryGetQuantity(string sku, out int qty)`
- `void PrintReport()` — loop index `0..Count-1`, print `Keys[i]` and `Values[i]`
- `string? LowestSku()` — `Keys[0]` if Count > 0 else null
- `void RemoveSku(string sku)` — Remove by key

**Class `RegionSalesMap`** (uses `SortedDictionary<string, int>`)

- Case-sensitive string keys (region codes)
- `AddRegion(string code, int units)` — Add (throws if duplicate — catch in demo or use guard)
- `void UpdateRegion(string code, int units)` — indexer set
- `IEnumerable<KeyValuePair<string,int>> OrderedEntries()` — foreach in key order
- `string? FirstRegionKey()` — first key from foreach on Keys

**Class `TagCountBoard`** — `SortedDictionary<string, int>` with `StringComparer.OrdinalIgnoreCase`

- Indexer set merges tag counts; `"CSharp"` and `"csharp"` same key

## Demo Main

1. ReorderReport: add SKUs out of order (Z, A, M); PrintReport shows A, M, Z; print LowestSku
2. RegionSalesMap: add/update/remove; print ordered entries
3. TagCountBoard: set dotnet=5, then CSharp=99 overwrites csharp key — print single csharp entry with 99

## Constraints

- net8
- Do not call `OrderBy` on keys for primary report — rely on sorted collection order

## Non-goals

Database export, concurrent updates

## Evaluation

[EVALUATION.md](EVALUATION.md)
