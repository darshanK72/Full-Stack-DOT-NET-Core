---
module: 08. Advanced C# Features
difficulty: Medium
chapters: 04 Var Dynamic & Special Keywords
domain: WarehouseIntegration
---

# Flexible Feed Adapter

Build a **.NET 8 console application from scratch** that maps strongly typed inventory items and dynamic import rows using `dynamic`, `nameof`, and `default`.

## Business context

A warehouse receives SKU updates from a fixed internal API (strong types) and from a third-party CSV plugin whose column names vary by vendor. The adapter normalizes both into a common snapshot dictionary and validates required fields using compile-time name strings in error messages.

## Definitions

**Class `InventoryItem`**

- `Sku`, `Name` (required string init properties)
- `Quantity` (int), `UnitPrice` (decimal)

**Class `FlexibleImportRow` : DynamicObject**

- Case-insensitive `Dictionary<string, object?>` backing store
- Override `TryGetMember` / `TrySetMember` for dynamic property access
- `IReadOnlyDictionary<string, object?> Snapshot()`

**Record `NormalizedRow`**

- `Sku`, `Name` (string), `Quantity` (int), `UnitPrice` (decimal)

**Class `FeedAdapter`**

- `NormalizedRow FromStrongTyped(InventoryItem item)` — direct mapping
- `NormalizedRow FromDynamic(dynamic row)` — read `Sku`, `Name`, `Quantity`, `UnitPrice`; throw `InvalidOperationException` with message `"Missing required field: {nameof(...)}"` when a required field is null/missing; coerce numeric fields with `Convert.ToInt32` / `Convert.ToDecimal`
- `T GetOrDefault<T>(dynamic row, string fieldName)` — returns `default(T)!` when field absent (use `default` literal where appropriate)

## Demo Main

1. Map one `InventoryItem` to `NormalizedRow`; print snapshot.
2. Build `FlexibleImportRow`, set fields via dynamic, map via `FromDynamic`.
3. Attempt dynamic row missing `Sku`; catch and print exception message (must contain `Sku` via nameof).
4. Call `GetOrDefault<int>` on missing field; print default 0.

## Constraints

- net8, explicit usings
- `FlexibleImportRow` must inherit `DynamicObject`
- Use `nameof` in exception messages, not magic strings

## Non-goals

Real CSV parsing, COM interop, reflection-based mapping

## Evaluation

[EVALUATION.md](EVALUATION.md)
