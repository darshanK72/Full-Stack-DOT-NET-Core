---
module: 03. Generics & Collections
difficulty: Medium
chapters: 01 Generics
domain: Warehouse
---

# Generic Repository

Build a **.NET 8 console application from scratch** demonstrating generic classes, interfaces, methods, and constraints.

## Business context

A warehouse platform stores different payload types (SKU strings, bin counts, weight decimals) in one reusable in-memory pattern. The storage layer must be type-safe at compile time — no `object` casts.

## Definitions

**Interface `IRepository<TKey, TValue>`** where `TKey : notnull`

- `bool TryGet(TKey key, out TValue value)`
- `void Set(TKey key, TValue value)`
- `IReadOnlyCollection<TKey> Keys { get; }`

**Class `InMemoryRepository<TKey, TValue>`** — implements `IRepository<TKey, TValue>` with `Dictionary<TKey, TValue>` backing store.

**Generic struct `Quantity<TUnit>`** where `TUnit : struct`

- Properties: `Count` (int), `Unit` (TUnit)
- Method `Format()` → `"{Count} {Unit}"`

**Static helper class `RepositoryHelpers`**

- `static void Swap<T>(ref T left, ref T right)` — generic method
- `static T CreateDefault<T>() where T : new()` — returns `new T()`
- `static string DescribeDefault<T>()` — returns `typeof(T).Name` and `default(T)` display (`"null"` for null reference default)
- `static int CompareOrdered<T>(T left, T right) where T : IComparable<T>` — delegates to `CompareTo`

**Demo entity `StockLine`** (reference type for constraint demo)

- Properties: `Sku` (string), `Units` (int)
- Implements `IComparable<StockLine>` — compare by `Sku` ordinal

## Demo Main

1. `InMemoryRepository<string, int>` — set two SKUs, `TryGet` one found and one missing
2. `Quantity<int>` with `Count = 24`, `Unit = 1` — print `Format()`
3. `Swap` two SKU strings; print after swap
4. `CreateDefault<StockLine>()`, assign Sku, print `DescribeStockEntry`-style Sku via property
5. `CompareOrdered` on two `StockLine` instances
6. Print `DescribeDefault<int>()` and `DescribeDefault<string>()`

## Constraints

- net8, explicit usings
- No legacy `ArrayList` / `Hashtable`

## Non-goals

Persistence, async, DI container

## Evaluation

[EVALUATION.md](EVALUATION.md)
