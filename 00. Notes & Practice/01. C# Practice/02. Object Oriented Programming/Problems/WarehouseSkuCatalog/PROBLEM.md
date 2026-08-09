---
module: 02. Object Oriented Programming
difficulty: Medium
chapters: 02 Properties, 02 Indexers, 03 Copy Constructor, 04 Static Members
domain: Warehouse
---

# Warehouse SKU Catalog

Build a **.NET 8 console application from scratch** for SKU lookup with properties, indexers, and instance tracking.

## Business context

Warehouse staff locate products by aisle index or SKU string. Inventory service tracks how many catalog instances were created for diagnostics.

## Definitions

**Class `SkuItem`**

- Auto-properties: `Sku` (string), `Name` (string), `UnitPrice` (decimal)
- Full property `QuantityOnHand` with private backing field; setter rejects negative values
- Init-only `DateAdded` (DateTime) set in constructor
- Constructor `(sku, name, unitPrice, quantityOnHand, dateAdded)` with validation
- Copy constructor copying all fields into new instance
- Override `ToString()` → `"{Sku}: {Name} @ {UnitPrice:C} (qty {QuantityOnHand})"`

**Class `SkuCatalog`**

- Static field `_instancesCreated` incremented in instance constructor
- Static property `InstancesCreated` (read-only public)
- Static method `SkuCatalog CreateEmpty()` — factory returning new catalog
- Private list storage; indexer by int `this[int index]` get/set (set replaces slot; validate index)
- Indexer by string `this[string sku]` get only — case-insensitive SKU lookup; throw `KeyNotFoundException` if missing
- `void Add(SkuItem item)` — false if SKU already present (case-insensitive)
- `bool Remove(string sku)` — case-insensitive remove

## Demo Main

Create catalog via factory, add items, mutate via int indexer, fetch via string indexer, clone item via copy constructor, print `InstancesCreated`.

## Constraints

- net8, explicit usings
- Both indexers on SkuCatalog

## Non-goals

Persistence, concurrency

## Evaluation

[EVALUATION.md](EVALUATION.md)
