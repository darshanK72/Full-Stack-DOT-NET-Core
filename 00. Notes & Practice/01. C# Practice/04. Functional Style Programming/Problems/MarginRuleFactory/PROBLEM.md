---
module: 04. Functional Style Programming
difficulty: Hard
chapters: 06 Closures
domain: PricingRules
---

# Margin Rule Factory

Build a **.NET 8 console application from scratch** demonstrating closures, factory-returned delegates, loop capture pitfalls, and shared mutable capture.

## Business context

Pricing analysts configure margin guards dynamically: each rule remembers thresholds set at factory time and counters track how many quotes ran. Engineers must avoid classic for-loop closure bugs when building rule batches.

## Definitions

**Static class `CounterFactory`**

- `Func<int> MakeCounter(int start = 0)` — each call returns a **new** delegate; invoking it increments a captured int and returns the new value (1, 2, 3… from first invoke after creation)

**Static class `MarginRuleFactory`**

- `Func<decimal, decimal> MakeMarginRule(decimal costBasis, decimal minimumMarginPct)` — returned delegate maps `listPrice` to `listPrice` when margin `(listPrice - costBasis) / listPrice >= minimumMarginPct`; otherwise returns minimum acceptable price `costBasis / (1 - minimumMarginPct)` rounded to 2 decimals (`AwayFromZero`). Capture `costBasis` and `minimumMarginPct` at factory time.

**Static class `CaptureLab`**

- `Func<int>[] BuildBrokenIndexFuncs(int count)` — **intentional bug demo**: loop `for (int i = 0; i < count; i++)` storing `() => i` without local copy; return array length `count`
- `Func<int>[] BuildFixedIndexFuncs(int count)` — fixed version using local copy `int captured = i` inside loop body
- `void MutateSharedCaptureDemo()` — create single captured `int total = 0`; two `Action` delegates both doing `total++`; invoke each twice; expose final total via `out int finalTotal` parameter or return value documented in demo output (expect 4)

**Class `QuoteSession`**

- Field `Func<int> _quoteCounter` from `MakeCounter()`
- Field `Func<decimal, decimal> _marginRule`
- Constructor `(decimal costBasis, decimal minMarginPct)` assigns rule from factory
- `decimal Quote(decimal listPrice)` — increments counter via invoke; returns `_marginRule(listPrice)`
- `int QuotesRun => /* invoke counter logic or track — must reflect MakeCounter invocations */` — implement by keeping reference to same counter delegate created once in ctor

## Demo Main

1. Two independent counters; show separate sequences.
2. Margin rule: cost `80m`, min margin `20%`; quote `100m` (pass) and `90m` (bump to floor).
3. Print values from broken vs fixed index func arrays (same index should differ for broken).
4. Run `MutateSharedCaptureDemo`; print final shared total.
5. `QuoteSession` runs 3 quotes; print `QuotesRun`.

## Constraints

- net8, explicit usings, `decimal` for money
- Factories must use closures (no static globals for counter state)
- No LINQ required

## Non-goals

Thread safety, expression trees, async

## Evaluation

[EVALUATION.md](EVALUATION.md)
