using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Models;
using MSTest.Services;

namespace MSTest.Tests;

/*
 * SECTION 8: [ClassInitialize] AND [ClassCleanup] — ONCE PER TEST CLASS
 *
 * Static methods run once for the entire [TestClass], before the first test and
 * after the last test respectively. Use for expensive one-time setup (database seed,
 * loading reference data) that can be shared read-only across tests in the class.
 *
 * Signature requirements:
 *   • Method must be public static
 *   • [ClassInitialize] accepts TestContext context (optional but conventional)
 *   • [ClassCleanup] takes no parameters
 *
 * xUnit has no direct equivalent; IClassFixture<T> or ICollectionFixture<T>
 * cover shared setup across tests — see 02. xUnit.
 *
 * [AssemblyInitialize] runs once per test assembly (all classes in MSTest.Tests).
 * It requires a separate class and is heavier (deployment items, global config).
 * PREVIEW only in Program.cs Section 8 — defer full depth unless you need it.
 */
[TestClass]
public class DiscountCalculatorLifecycleTests
{
    private static IReadOnlyDictionary<CustomerTier, decimal> _tierRateTable = null!;
    private DiscountCalculator _calculator = null!;

    [ClassInitialize]
    public static void InitClass(TestContext context)
    {
        // Simulates one-time load of reference rates shared by all tests in this class
        _tierRateTable = new Dictionary<CustomerTier, decimal>
        {
            [CustomerTier.Standard] = 0.05m,
            [CustomerTier.Silver] = 0.10m,
            [CustomerTier.Gold] = 0.15m,
        };
    }

    [ClassCleanup]
    public static void CleanupClass()
    {
        _tierRateTable = null!; // release shared static state after last test
    }

    [TestInitialize]
    public void SetUp()
    {
        _calculator = new DiscountCalculator();
    }

    [DataTestMethod]
    [DataRow(CustomerTier.Standard, 100.00, 95.00)]
    [DataRow(CustomerTier.Silver, 200.00, 180.00)]
    [DataRow(CustomerTier.Gold, 80.00, 68.00)]
    public void ApplyTierDiscount_MatchesClassInitializedRateTable(
        CustomerTier tier,
        double orderTotal,
        double expectedTotal)
    {
        decimal expectedRate = _tierRateTable[tier]; // read shared class-level data
        decimal manualDiscount = (decimal)orderTotal - ((decimal)orderTotal * expectedRate);
        decimal actual = _calculator.ApplyTierDiscount((decimal)orderTotal, tier);

        Assert.AreEqual(manualDiscount, actual);
        Assert.AreEqual((decimal)expectedTotal, actual);
    }
}
