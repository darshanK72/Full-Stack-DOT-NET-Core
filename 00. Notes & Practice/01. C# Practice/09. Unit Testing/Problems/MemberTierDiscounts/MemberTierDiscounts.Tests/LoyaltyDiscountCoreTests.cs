using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RetailLoyalty;

namespace MemberTierDiscounts.Tests;

[TestClass]
public class LoyaltyDiscountCoreTests
{
    private LoyaltyDiscountService? _service;

    [TestInitialize]
    public void Setup()
    {
        // TODO: assign new LoyaltyDiscountService to _service
        throw new NotImplementedException();
    }

    [TestMethod]
    public void ApplyTierDiscount_ReducesTotal_WhenSilverTier()
    {
        // TODO: $120 Silver → $114.00
        throw new NotImplementedException();
    }

    [TestMethod]
    public void ApplyCoupon_ThrowsInvalidOperation_WhenOrderBelowMinimum()
    {
        // TODO: Assert.ThrowsException for $20 + WELCOME20
        throw new NotImplementedException();
    }
}
