---
module: 01. C# Language Fundamentals
difficulty: Hard
chapters: 02 Data Types, 04 Operators, 06 Methods, 07 Control Flow, 08 Strings, 09 Loops
domain: WarehouseFulfillment
---

# Warehouse Fulfillment Gate

Build a **.NET 8 console application from scratch**.

## Business context

Before a parcel leaves the warehouse, multiple checks must pass. Operations also batch-packs units into cases and prints shipping labels.

## Core components

Implement `FulfillmentService` with:

### Pack units

`SplitIntoCases(int totalUnits, int unitsPerCase)` → `(fullCases, leftover)` using integer division and remainder. Guard: `unitsPerCase <= 0` → return `(0,0)`.

### Release gate

`CanRelease(bool inventoryOk, bool paymentOk, bool holdActive)` — all must pass; hold blocks.

### Express eligibility

`IsExpressEligible(decimal weightKg, bool holdActive)` — weight in (0, 20] and no hold.

### Label builder

`BuildLabel(string? zone, string fallbackZone, int priority)` — resolve zone with null-coalescing; if `priority >= 5` prefix `Express-` else `Standard-`; normalize zone trim + uppercase.

### Discount line

`ApplyPromo(decimal subtotal, string? percentText)` — if null/empty return subtotal; `TryParse` int percent 0–100; invalid → unchanged; else subtract percent using decimal math.

## CLI

Interactive menu calling each function with sample inputs OR demo `Main` printing a scripted scenario covering:

- 25 units / 12 per case
- Release blocked by hold
- Express label with null zone
- 20% promo on 100.00

## Constraints

- net8, explicit usings
- decimal for money and weight
- No LINQ required

## Evaluation

[EVALUATION.md](EVALUATION.md)
