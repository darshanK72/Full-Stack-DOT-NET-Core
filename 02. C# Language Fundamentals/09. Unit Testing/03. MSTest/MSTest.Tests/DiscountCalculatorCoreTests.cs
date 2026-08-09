using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Models;
using MSTest.Services;

namespace MSTest.Tests;

/*
 * SECTION 3: [TestClass], [TestMethod], AND TEST NAMING
 *
 * [TestClass] marks a container; only public instance methods tagged [TestMethod]
 * inside it are discovered as tests. One class per feature area keeps Test Explorer
 * readable.
 *
 * Naming convention (MethodUnderTest_ExpectedResult_WhenCondition):
 *   ApplyPercentDiscount_ReducesTotal_WhenRateIsTenPercent
 *
 * Each [TestMethod] follows Arrange-Act-Assert and must not depend on run order.
 *
 * xUnit comparison: no [TestClass] required; [Fact] replaces [TestMethod].
 * See Program.cs Section 9 for the full mapping table.
 */
[TestClass]
public class DiscountCalculatorCoreTests
{
    private DiscountCalculator _calculator = null!;
    private List<string> _diagnosticLog = null!;

    /*
     * SECTION 5: [TestInitialize] AND [TestCleanup] — PER-TEST LIFECYCLE
     *
     * [TestInitialize] runs BEFORE each [TestMethod] in this class.
     * [TestCleanup] runs AFTER each [TestMethod], even when the test fails.
     *
     * Use them for fresh SUT instances and resetting mutable per-test state.
     * xUnit equivalent: constructor (before) + IDisposable.Dispose (after).
     */
    [TestInitialize]
    public void SetUp()
    {
        _calculator = new DiscountCalculator();              // fresh SUT per test
        _diagnosticLog = new List<string>();                 // scratch list reset each run
    }

    [TestCleanup]
    public void TearDown()
    {
        _diagnosticLog.Clear();                            // explicit cleanup of mutable state
    }

    [TestMethod]
    public void ApplyPercentDiscount_ReducesTotal_WhenRateIsTenPercent()
    {
        // Arrange
        decimal orderTotal = 100.00m;
        decimal discountRate = 0.10m;
        _diagnosticLog.Add($"Testing {orderTotal:C} at {discountRate:P0}"); // optional trace

        // Act
        decimal result = _calculator.ApplyPercentDiscount(orderTotal, discountRate);

        // Assert
        Assert.AreEqual(90.00m, result);
    }

    [TestMethod]
    public void ApplyTierDiscount_ReturnsNinetyFive_WhenStandardTierOnOneHundred()
    {
        decimal result = _calculator.ApplyTierDiscount(100.00m, CustomerTier.Standard);

        Assert.AreEqual(95.00m, result);
    }
}
