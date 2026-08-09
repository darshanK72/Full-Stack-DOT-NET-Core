using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Models;
using MSTest.Services;

namespace MSTest.Tests;

/*
 * SECTION 7: Assert.* — VERIFY OUTCOMES
 *
 * Microsoft.VisualStudio.TestTools.UnitTesting.Assert centralizes checks.
 * Failed assertions throw AssertFailedException; dotnet test reports expected vs actual.
 *
 * Covered here:
 *   Assert.AreEqual(expected, actual)     — value equality (decimal money)
 *   Assert.IsTrue(condition)              — boolean must be true
 *   Assert.IsFalse(condition)             — boolean must be false
 *   Assert.ThrowsException<T>(() => ...)  — delegate must throw type T
 *   StringAssert.Contains(substring, text) — message / string checks
 *
 * xUnit mapping: Equal, True, False, Throws<T> — see Program.cs Section 9.
 */
[TestClass]
public class DiscountCalculatorAssertTests
{
    private DiscountCalculator _calculator = null!;

    [TestInitialize]
    public void SetUp()
    {
        _calculator = new DiscountCalculator();
    }

    [TestMethod]
    public void ApplyPercentDiscount_ThrowsArgumentOutOfRange_WhenOrderTotalIsNegative()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _calculator.ApplyPercentDiscount(-1.00m, 0.10m));
    }

    [TestMethod]
    public void ApplyPercentDiscount_ThrowsArgumentOutOfRange_WhenRateExceedsOne()
    {
        ArgumentOutOfRangeException exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _calculator.ApplyPercentDiscount(100.00m, 1.50m));

        StringAssert.Contains(exception.Message, "Discount rate");
    }

    [TestMethod]
    public void ApplyCoupon_ThrowsInvalidOperation_WhenOrderBelowMinimum()
    {
        InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(() =>
            _calculator.ApplyCoupon(5.00m, "WELCOME20"));

        StringAssert.Contains(exception.Message, "not eligible");
    }

    [TestMethod]
    public void ApplyCoupon_ReturnsDiscountedTotal_WhenCodeIsValid()
    {
        decimal result = _calculator.ApplyCoupon(50.00m, "welcome20");

        Assert.AreEqual(40.00m, result);
    }

    [TestMethod]
    public void IsEligibleForFreeShipping_IsTrue_WhenGoldTierRegardlessOfAmount()
    {
        bool eligible = _calculator.IsEligibleForFreeShipping(10.00m, CustomerTier.Gold);

        Assert.IsTrue(eligible);
    }

    [TestMethod]
    public void IsEligibleForFreeShipping_IsFalse_WhenStandardTierAndLowTotal()
    {
        bool eligible = _calculator.IsEligibleForFreeShipping(40.00m, CustomerTier.Standard);

        Assert.IsFalse(eligible);
    }

    [TestMethod]
    public void IsEligibleForFreeShipping_IsTrue_WhenOrderMeetsThreshold()
    {
        bool eligible = _calculator.IsEligibleForFreeShipping(75.00m, CustomerTier.Standard);

        Assert.IsTrue(eligible);
    }
}
