# Freight Rate Calculator — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| FreightQuoteService methods + validation | 25 |
| ApplyFuelSurcharge Theory + InlineData (≥4 rows) | 20 |
| CalculateSubtotal MemberData (≥3 rows) | 25 |
| ITestOutputHelper diagnostic test | 15 |
| Demo Main | 5 |
| Decimal precision in assertions | 10 |

## AI Review Prompt

Evaluate FreightRateCalculator against PROBLEM.md. Confirm [Theory]/[InlineData]/[MemberData] usage and ITestOutputHelper. Score /100, verdict.

---

## Model Answer Checklist

- [ ] Empty shipment subtotal is 0
- [ ] 100% surcharge doubles base (within precision)
- [ ] MemberData includes at least one multi-line row
- [ ] `dotnet test` all green
