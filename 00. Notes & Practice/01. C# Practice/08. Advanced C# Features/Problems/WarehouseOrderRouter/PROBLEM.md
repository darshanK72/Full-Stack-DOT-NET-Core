---
module: 08. Advanced C# Features
difficulty: Hard
chapters: 05 C# 7 Features
domain: WarehouseRouting
---

# Warehouse Order Router

Build a **.NET 8 console application from scratch** that routes warehouse orders using C# 7 pattern matching, tuple returns, out variables, local functions, and a ref return slot update.

## Business context

A fulfillment router assigns each order to an aisle and priority lane. High-value express orders need a different path than standard lines. The router parses quantity overrides from text, classifies orders with `switch` patterns, returns routing metadata as tuples, and updates in-place priority on a shared order array.

## Definitions

**Enum `OrderPriority`**

- `Standard`, `Express`, `Critical`

**Class `Order`**

- `OrderId`, `Sku`, `Quantity`, `UnitPrice`, `Priority`
- Expression-bodied: `LineTotal => Quantity * UnitPrice`
- Expression-bodied: `IsHighValue => LineTotal >= 10_000m`

**Static class `WarehouseConstants`**

- `HighValueThreshold = 10_000m` (use digit separator)
- `ExpressSurcharge = 250.00m`

**Class `OrderRouter`**

- `(string Aisle, decimal Surcharge) Route(Order order)` — local function `ResolveAisle` inside method:
  - `switch` on `(order.Priority, order.IsHighValue)` with patterns: Critical → `"A1"`, Express+high → `"B2"`, Express → `"B1"`, default → `"C1"`
  - Surcharge: Express or Critical adds `WarehouseConstants.ExpressSurcharge`, else 0
- `bool TryApplyQuantityOverride(Order order, string text, out int appliedQuantity)` — `int.TryParse` with **out variable** at call site; on success set `order.Quantity` and return true
- `ref Order FindOrderById(Order[] orders, int orderId)` — **ref return** reference to matching slot; throw `KeyNotFoundException` if missing
- `(int ExpressCount, int StandardCount) CountByPriority(IEnumerable<Order> orders)` — tuple return; use **deconstruction** at call site in Main

## Demo Main

1. Seed `Order[]` with mixed priorities and values (at least one high-value express).
2. Deconstruct `(express, standard) = CountByPriority(...)`.
3. Route each order; print id, aisle, surcharge.
4. `TryApplyQuantityOverride` on one order with valid/invalid text.
5. `FindOrderById` ref return: bump found order to `Critical` in place; print updated priority.

## Constraints

- net8, explicit usings, `decimal` for money
- Use digit separators in constants (`10_000m`)
- At least one local function, one out variable, one ref return, one tuple deconstruction

## Non-goals

async/ValueTask, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
