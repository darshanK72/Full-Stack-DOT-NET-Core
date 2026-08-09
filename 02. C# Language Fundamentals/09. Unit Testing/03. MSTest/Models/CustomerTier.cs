namespace MSTest.Models;

/*
 * SECTION 2: CUSTOMER TIER — domain enum for tier-based discounts
 *
 * Enums are valid [DataRow] arguments in MSTest (compile-time constants).
 * DiscountCalculator.ApplyTierDiscount maps each tier to a fixed percentage.
 *
 * Used by:
 *   Services/DiscountCalculator.cs
 *   MSTest.Tests/DiscountCalculatorDataTests.cs — [DataRow(CustomerTier.Gold, ...)]
 */
public enum CustomerTier
{
    Standard, // 5% tier discount
    Silver,   // 10% tier discount
    Gold      // 15% tier discount + free shipping at any order total
}
