# Member Tier Discounts — Evaluation

## Rubric (100 points)

| Criterion | Points |
|-----------|--------|
| LoyaltyDiscountService rules | 25 |
| TestInitialize fresh service | 10 |
| Core TestMethods + ThrowsException | 20 |
| DataTestMethod + DataRow (≥3) | 20 |
| ClassInitialize lifecycle test | 15 |
| Demo Main | 10 |

## AI Review Prompt

Evaluate MemberTierDiscounts against PROBLEM.md for MSTest attributes and AAA. Score /100, verdict.

---

## Model Answer Checklist

- [ ] Order below $25 rejects coupon
- [ ] Gold tier free shipping regardless of amount
- [ ] ClassInitialize runs once per class
- [ ] `dotnet test MemberTierDiscounts.Tests` green
