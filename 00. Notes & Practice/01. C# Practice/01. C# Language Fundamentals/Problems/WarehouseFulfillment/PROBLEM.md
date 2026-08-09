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

---

## Extended Scenarios

Implement these after the five core methods are working.

### EX1 — Multi-parcel batch summary

Add a method that accepts an array of unit counts (one per item in an order) and a
single case size, and returns the total full cases and total leftover units across
the entire batch.

This exercises iterating an array (ch09), accumulating integer totals (ch04), and
reusing SplitIntoCases inside a loop (ch07).

### EX2 — Stacked promotions

Extend the promo system so a list of percentage strings can be applied sequentially —
each promotion applies to the price after the previous discount, not to the original price.
Return the final price after all valid promotions have been applied.

This exercises looping over an array of strings (ch09), calling ApplyPromo repeatedly (ch07),
and chaining decimal computations (ch04).

### EX3 — Carrier selection

Add a method that takes the same weight and hold parameters as IsExpressEligible and
returns the name of the recommended carrier as a string: "Express", "Standard", or "Hold".
"Hold" is returned when holdActive is true regardless of weight.

This exercises chained conditional logic (ch04) and returning a string based on state,
similar to BuildLabel but with different branching rules.

---

## Implementation Guide

### Classes and their responsibilities

| Class | Responsibility |
|-------|----------------|
| `FulfillmentService` | Five pure gate methods — no state stored between calls |
| `Program` | Scripted demo that calls each method with representative inputs |

No model class is needed — every method works entirely with primitive parameters.

### Method contracts

| Method | Returns | Key guard |
|--------|---------|-----------|
| `SplitIntoCases(totalUnits, unitsPerCase)` | `(fullCases, leftover)` tuple | `unitsPerCase ≤ 0` → return `(0, 0)` |
| `CanRelease(inventoryOk, paymentOk, holdActive)` | bool | All three conditions must be true simultaneously |
| `IsExpressEligible(weightKg, holdActive)` | bool | Weight must be in range `(0, 20]` and no hold |
| `BuildLabel(zone, fallbackZone, priority)` | string | zone is null → use fallbackZone; normalise result |
| `ApplyPromo(subtotal, percentText)` | decimal | Invalid or out-of-range percent → return subtotal unchanged |

### Business rules to enforce

- Integer division with `/` truncates toward zero — `25 / 12` is `2`, not `2.08` (ch04)
- The `%` operator gives the remainder — `25 % 12` is `1` (ch04)
- `&&` short-circuits: in `CanRelease`, if `inventoryOk` is false, C# does not evaluate `paymentOk` (ch04)
- In `BuildLabel`, null resolution must happen before normalisation — you cannot call `.Trim()` on null (ch04 `??`)
- In `ApplyPromo`, use a decimal literal (`100m`) when dividing so the arithmetic stays in decimal, not integer (ch02, ch04)

### Concepts by chapter

| Chapter | Where it applies |
|---------|-----------------|
| ch04 | `/` and `%` for case calculation; `&&` and `!` for gates; `??` for zone fallback; `?:` for label prefix; decimal arithmetic |
| ch05 | `int.TryParse` to safely convert the promo percentage string |
| ch08 | `.Trim()` and `.ToUpperInvariant()` for zone normalisation in BuildLabel |
