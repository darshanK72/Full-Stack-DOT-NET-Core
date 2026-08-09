---
module: 03. Generics & Collections
difficulty: Medium
chapters: 01 Generics, 02 ArrayList
domain: Warehouse
---

# Legacy Inventory Migrator

Build a **.NET 8 console application from scratch** that reads legacy non-generic collections and migrates to generic equivalents.

## Business context

IT is decommissioning a .NET Framework inventory snippet that used `ArrayList` and `Hashtable`. Your tool proves migration correctness and documents boxing/unboxing behavior before cutover.

## Definitions

**Class `Product`** — `ProductNo` (int), `ProductName` (string), `ProductPrice` (decimal), `ToString()` → `"#{ProductNo} {ProductName} — {ProductPrice:C}"`

**Class `LegacyInventorySnapshot`**

- Builds sample data using `System.Collections.ArrayList` and `Hashtable`:
  - ArrayList: one boxed int count, one string SKU, one `Product` reference
  - Hashtable: SKU string → boxed int quantity (at least 2 pairs)
- Method `int UnboxFirstCount()` — returns `(int)arrayList[0]` (first element must be int)
- Method `string? LookupBin(string sku)` — `Hashtable[sku] as string` or null (bins stored as string values in separate demo entries)

**Class `ModernInventory`**

- `List<Product> Catalog { get; }`
- `Dictionary<string, int> QuantityBySku { get; }`
- `void ImportFromLegacy(ArrayList legacyLines, Hashtable legacySkus)`:
  - For each `object` in legacyLines: if `Product`, add to Catalog; skip others with count logged
  - For each key in legacySkus: if key is string and value is int (unbox), add to QuantityBySku
- `decimal CatalogTotal()` — sum `ProductPrice` in Catalog via typed foreach

## Demo Main

1. Create legacy snapshot; print unboxed count and one hashtable lookup
2. Import into `ModernInventory`; print catalog count, dictionary count, catalog total
3. Print one line explaining why `List<Product>` removed casts from the catalog loop

## Constraints

- net8; qualify `System.Collections.ArrayList` / `Hashtable` if namespace clashes
- Handle wrong-type legacy entries without crashing import (skip + continue)

## Non-goals

File I/O, full ERP migration UI

## Evaluation

[EVALUATION.md](EVALUATION.md)
