---
module: 09. Unit Testing
difficulty: Medium
chapters: 02 xUnit
domain: LogisticsPricing
---

# Freight Rate Calculator

Build a **.NET 8 solution** with a freight quoting service and **xUnit parameterized tests** using `[Theory]`, `[InlineData]`, and `[MemberData]`.

## Business context

A logistics portal quotes parcel shipping from weight tiers, distance bands, and fuel surcharges. Rate tables belong in data-driven tests so each bracket is verified without duplicating test methods.

## Solution layout

```
FreightRateCalculator/
  FreightRateCalculator.csproj
  Program.cs                         ← FreightQuoteService + ShipmentLine model
  FreightRateCalculator.Tests/
    FreightQuoteTheoryTests.cs       ← complete Theory TODOs
    FreightQuoteOutputTests.cs       ← ITestOutputHelper TODO
```

## Definitions (production)

**Record `ShipmentLine`**

- `Sku` (string), `WeightKg` (decimal > 0), `UnitRate` (decimal ≥ 0)

**Class `FreightQuoteService`**

- `decimal CalculateLineWeightCharge(ShipmentLine line)` — `WeightKg * UnitRate`
- `decimal ApplyFuelSurcharge(decimal baseAmount, decimal surchargePercent)` — percent of base; clamp surchargePercent to [0, 100]; return 0 when baseAmount ≤ 0
- `decimal CalculateSubtotal(IReadOnlyList<ShipmentLine> lines)` — sum of line weight charges; empty list → 0
- `bool QualifiesForBulkDiscount(IReadOnlyList<ShipmentLine> lines)` — true when subtotal ≥ 500m

## Tests (xUnit)

**FreightQuoteTheoryTests.cs**

| Test | Attributes |
|------|------------|
| `ApplyFuelSurcharge_ReturnsExpectedAmount` | `[Theory]` + at least 4 `[InlineData]` rows (include 0% and 100%) |
| `CalculateSubtotal_MemberDataRows_ReturnExpected` | `[Theory]` + `[MemberData(nameof(SubtotalMemberData))]` with at least 3 rows including empty list |

Provide static `SubtotalMemberData` returning `IEnumerable<object[]>` with `List<ShipmentLine>` and expected decimal.

**FreightQuoteOutputTests.cs**

| Test | Requirement |
|------|-------------|
| `QualifiesForBulkDiscount_LogsSubtotal` | Inject `ITestOutputHelper`; write subtotal via `output.WriteLine` before assert |

## Demo Main

Build a two-line sample shipment; print subtotal, 12% fuel surcharge, and bulk eligibility.

## Constraints

- net8, explicit usings, decimal money
- Use `Assert.Equal(expected, actual, precision: 2)` where rounding matters

## Non-goals

MSTest, Moq, persistence

## Evaluation

[EVALUATION.md](EVALUATION.md)
