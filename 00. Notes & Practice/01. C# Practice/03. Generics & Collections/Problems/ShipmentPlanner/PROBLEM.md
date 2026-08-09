---
module: 03. Generics & Collections
difficulty: Medium
chapters: 03 List
domain: Warehouse
---

# Shipment Planner

Build a **.NET 8 console application from scratch** for a dock shipment queue using `List<T>` CRUD, sorting, and read-only exposure.

## Business context

The shipping desk maintains an ordered list of outbound lines. Supervisors sort by priority or SKU, filter heavy lines, and publish a read-only snapshot to floor displays.

## Definitions

**Class `ShipmentItem`**

- Read-only `Sku` (string), mutable `Quantity` (int), `Priority` (int) — lower number ships first
- Constructor `(sku, quantity, priority)`
- Implements `IComparable<ShipmentItem>` — compare by `Priority` ascending
- Override `ToString()` → `"{Sku} ×{Quantity} (P{Priority})"`

**Class `ShipmentItemSkuComparer`** — implements `IComparer<ShipmentItem>` — compare by `Sku` ordinal

**Class `ShipmentPlanner`**

- Private `List<ShipmentItem> _queue`
- `void Add(ShipmentItem item)` — append
- `void SortByPriority()` — parameterless `Sort()` (natural order)
- `void SortBySku()` — `Sort(new ShipmentItemSkuComparer())`
- `void SortByQuantityDescending()` — `Sort((a,b) => b.Quantity.CompareTo(a.Quantity))`
- `List<ShipmentItem> FindHeavy(int minQuantity)` — `FindAll` predicate
- `ReadOnlyCollection<ShipmentItem> PublishSnapshot()` — `AsReadOnly()` on internal list
- `int Count` — queue count

## Demo Main

Seed at least 3 items. Print after each sort mode. Print heavy lines (min qty 10). Publish snapshot, add one more item to internal queue, foreach snapshot — new item visible. Attempt comment that snapshot.Add would throw.

## Constraints

- net8
- Do not expose mutable list reference from PublishSnapshot

## Non-goals

Dictionary lookup, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
