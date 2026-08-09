---
module: 03. Generics & Collections
difficulty: Medium
chapters: 03 List, 04 Dictionary
domain: Warehouse
---

# SKU Catalog Registry

Build a **.NET 8 console application from scratch** for fast SKU lookup with `Dictionary<TKey,TValue>` and ordered listing with `List<T>`.

## Business context

Warehouse staff scan SKU barcodes constantly. The registry must resolve a code to product details in one step, while still supporting a printable list in insertion order for audits.

## Definitions

**Class `Product`**

- `Sku` (string), `Name` (string), `UnitPrice` (decimal)
- `ToString()` → `"{Sku} | {Name} | {UnitPrice:C}"`

**Class `SkuCatalogRegistry`**

- `Dictionary<string, Product> BySku` — keys are SKU, case-insensitive comparer (`StringComparer.OrdinalIgnoreCase`)
- `List<Product> InsertionOrder` — parallel audit trail
- `bool Register(Product product)`:
  - If SKU already in dictionary (case-insensitive) → return false
  - Else add to both structures → return true
- `bool TryGet(string sku, out Product? product)` — `TryGetValue`, case-insensitive key
- `bool UpdatePrice(string sku, decimal newPrice)` — false if missing; else update dictionary entry's price (same object in list reflects change)
- `Product GetRequired(string sku)` — indexer GET; let `KeyNotFoundException` propagate (document in demo)
- `IReadOnlyList<Product> AllInInsertionOrder()` — return list as read-only view or copy

**Static demo helper `FindBySkuScan(List<Product> list, string sku)`** — linear scan returning first match or null (for timing comparison message only)

## Demo Main

Register 3 products (one duplicate SKU attempt → false). TryGet hit/miss. UpdatePrice. Print insertion order. Call FindBySkuScan vs TryGet and print which approach is appropriate at scale (one sentence).

## Constraints

- net8
- Null SKU rejected on Register (ArgumentException or return false — document)

## Non-goals

SortedDictionary, file persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
