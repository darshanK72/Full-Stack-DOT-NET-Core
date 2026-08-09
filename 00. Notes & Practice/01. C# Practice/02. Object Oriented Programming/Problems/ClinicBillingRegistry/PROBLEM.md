---
module: 02. Object Oriented Programming
difficulty: Medium
chapters: 02 Indexers, 03 Constructors, 04 Static, 04 Enums, 07 Access Modifiers
domain: HealthcareBilling
---

# Clinic Billing Registry

Build a **.NET 8 console application from scratch** for procedure codes and billing lines with indexers, static configuration, and encapsulation.

## Business context

A clinic maps CPT procedure codes to descriptions and assembles visit invoices. Codes are fixed at startup; lines are indexed per visit.

## Definitions

**Enum `ProcedureCategory`:** `Consultation`, `Lab`, `Imaging`, `Procedure`

**Static class `ProcedureCatalog`** (static class rules)

- Private static readonly dictionary populated in static constructor with at least 6 codes (you define codes like `"99213"`)
- Static method `bool TryGetDescription(string code, out string? description)`
- Static property `int CodeCount`
- `public const int MaxLinesPerVisit = 20;`

**Class `BillingLine`**

- `Code`, `Description`, `Amount` (decimal > 0)
- Copy constructor `BillingLine(BillingLine other)` copying all fields
- Expression-bodied read-only property `FormattedAmount => Amount.ToString("C");`

**Class `VisitInvoice`**

- `VisitId` (int), private list of lines
- Parameterized constructor `(int visitId)`; default constructor chains `: this(0)` and is **private** — instances only via factory below
- Indexer `BillingLine this[int index]` get only; throw if out of range
- `int LineCount { get; }`
- `bool TryAddLine(string code, decimal amount)` — fail if at `MaxLinesPerVisit`, unknown code, or amount ≤ 0
- `decimal GetTotal()` — sum amounts

**Static factory on `VisitInvoice`**

- `public static VisitInvoice Create(int visitId)` — rejects visitId ≤ 0

## Demo Main

Add lines to two visits using catalog codes; clone one line via copy constructor into second visit; print indexed lines and totals.

## Constraints

- net8, explicit usings, decimal money
- ProcedureCatalog is static class (no instance)

## Non-goals

Insurance adjudication, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
