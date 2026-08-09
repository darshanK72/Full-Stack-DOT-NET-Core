---
module: 03. Generics & Collections
difficulty: Hard
chapters: 08 IEnumerable & IEnumerator
domain: Warehouse
---

# Pick Ticket Enumerator

Build a **.NET 8 console application from scratch** with custom enumeration and lazy iterators for pick lines.

## Business context

Pick ticket PB-2201 exposes lines to scales and shipping APIs through standard iteration. Heavy lines (weight threshold) should stream lazily. Internal batch type must support `foreach` without exposing backing array.

## Definitions

**Class `PickLine`**

- `Sku` (string), `Quantity` (int), `WeightKg` (decimal) — init via constructor
- `decimal TotalWeightKg` => `Quantity * WeightKg`
- `ToString()` → `"{Sku} × {Quantity} ({TotalWeightKg:0.##} kg)"`

**Class `PickBatch`** — implements `IEnumerable<PickLine>`

- Backing storage: `PickLine[]` or private list
- `GetEnumerator()` returns custom nested class **or** yields via iterator block on a private method (either acceptable if MoveNext/Current correct)
- Explicit non-generic `IEnumerable.GetEnumerator()` forwarding

**Static class `PickLineFilters`**

- `static IEnumerable<PickLine> HeavyLines(IEnumerable<PickLine> source, decimal minTotalKg)` — **yield return** each line where `TotalWeightKg >= minTotalKg`
- `static IEnumerable<string> SkuSegments(string sku)` — yield return each non-empty segment split on `'-'`

**Class `PickLineCollection`** — wraps `List<PickLine>`

- `void Add(PickLine line)`
- `IEnumerable<PickLine> Lines` property returns list (IEnumerable)
- Method `decimal TotalWeight()` — foreach sum TotalWeightKg
- Method `string DemoModifyDuringForeach()` — foreach lines, on 2nd iteration call `Add` on same list; return exception message caught

## Demo Main

1. foreach PickBatch — print each line
2. Manual walk: `using IEnumerator<PickLine> e = batch.GetEnumerator();` while MoveNext — count lines
3. HeavyLines lazy demo — do not materialize full list first; print count over threshold
4. SkuSegments for `"PANEL-A"`
5. DemoModifyDuringForeach — print InvalidOperationException message
6. Optional: `Lines.Where(...).Count()` LINQ one-liner count comment

## Constraints

- net8
- PickBatch must be custom IEnumerable (not only exposing List directly as public API type)

## Non-goals

Full WMS, async streams

## Evaluation

[EVALUATION.md](EVALUATION.md)
