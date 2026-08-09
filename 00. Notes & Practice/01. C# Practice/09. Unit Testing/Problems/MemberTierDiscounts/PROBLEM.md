---
module: 09. Unit Testing
difficulty: Medium
chapters: 03 MSTest
domain: RetailLoyalty
---

# Member Tier Discounts

Build a **.NET 8 solution** with loyalty discount logic tested using **MSTest** — lifecycle hooks, data-driven tests, and `Assert.*`.

## Business context

A retail loyalty program applies tier percentages, coupon codes, and free-shipping rules at checkout. Enterprise CI still runs MSTest in many solutions; mirror that vocabulary here.

## Solution layout

```
MemberTierDiscounts/
  MemberTierDiscounts.csproj
  Program.cs
  MemberTierDiscounts.Tests/
    LoyaltyDiscountCoreTests.cs      ← TestInitialize + core tests
    LoyaltyDiscountDataTests.cs      ← DataTestMethod + DataRow
    LoyaltyDiscountLifecycleTests.cs ← ClassInitialize counter
```

## Definitions (production)

**Enum `MemberTier`:** `Standard`, `Silver`, `Gold`

**Class `LoyaltyDiscountService`**

- `decimal ApplyTierDiscount(decimal orderTotal, MemberTier tier)` — Standard 0%, Silver 5%, Gold 10%; reject orderTotal < 0
- `decimal ApplyCoupon(decimal orderTotal, string couponCode)` — `"WELCOME20"` → 20% off; `"FLAT10"` → $10 off; unknown code throws `InvalidOperationException`; orderTotal must be ≥ 25 for coupons
- `bool IsEligibleForFreeShipping(decimal orderTotal, MemberTier tier)` — Gold always true; Silver/Standard when orderTotal ≥ 75

## Tests (MSTest)

**LoyaltyDiscountCoreTests.cs**

- `[TestClass]` with `[TestInitialize]` creating fresh `LoyaltyDiscountService` each test
- `[TestMethod]` `ApplyTierDiscount_ReducesTotal_WhenSilverTier` — $120 → $114
- `[TestMethod]` `ApplyCoupon_ThrowsInvalidOperation_WhenOrderBelowMinimum` — $20 + WELCOME20
- Use `Assert.ThrowsException<InvalidOperationException>` for coupon minimum

**LoyaltyDiscountDataTests.cs**

- `[DataTestMethod]` + `[DataRow]` for tier discount table (at least 3 rows covering Standard/Silver/Gold)

**LoyaltyDiscountLifecycleTests.cs**

- `[ClassInitialize]` increments static `TestsRunCount`; `[TestMethod]` asserts count ≥ 1 after init

Naming: `MethodUnderTest_ExpectedResult_WhenCondition`

## Demo Main

Print tier discount on $120 Silver, WELCOME20 on $50, free shipping at $80 Standard.

## Constraints

- MSTest packages in test project only
- net8, explicit usings

## Non-goals

xUnit migration, Moq

## Evaluation

[EVALUATION.md](EVALUATION.md)
