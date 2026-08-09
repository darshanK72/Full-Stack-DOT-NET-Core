using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RetailLoyalty;

namespace MemberTierDiscounts.Tests;

[TestClass]
public class LoyaltyDiscountDataTests
{
    private readonly LoyaltyDiscountService _service = new LoyaltyDiscountService();

    [DataTestMethod]
    [DataRow(100, MemberTier.Standard, 100)]
    // TODO: add DataRow for Silver and Gold tiers
    public void ApplyTierDiscount_DataRows_ReturnExpected(
        decimal orderTotal,
        MemberTier tier,
        decimal expected)
    {
        // TODO: Act + Assert.AreEqual
        throw new NotImplementedException();
    }
}
