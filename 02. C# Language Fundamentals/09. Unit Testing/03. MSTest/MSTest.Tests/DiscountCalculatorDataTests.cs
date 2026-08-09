using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Models;
using MSTest.Services;

namespace MSTest.Tests;

/*
 * SECTION 6: [DataTestMethod] AND [DataRow] — PARAMETERIZED TESTS
 *
 * [DataTestMethod] marks a method that runs once per attached [DataRow(...)].
 * Prefer [DataTestMethod] over [TestMethod] when every row supplies arguments —
 * it makes data-driven intent explicit in Test Explorer.
 *
 * Rules:
 *   • Parameter count and types must match the method signature
 *   • Row values must be compile-time constants (enums and typeof(T) are valid)
 *   • [DataRow] does not accept decimal — use double and cast inside the test
 *
 * xUnit equivalent: [Theory] + [InlineData(...)] on the same method.
 * See Program.cs Section 9.
 */
[TestClass]
public class DiscountCalculatorDataTests
{
    private DiscountCalculator _calculator = null!;

    [TestInitialize]
    public void SetUp()
    {
        _calculator = new DiscountCalculator();
    }

    [DataTestMethod]
    [DataRow(100.00, 0.10, 90.00)]
    [DataRow(50.00, 0.20, 40.00)]
    [DataRow(0.00, 0.05, 0.00)]
    public void ApplyPercentDiscount_ReturnsExpectedTotal(
        double orderTotal,
        double discountRate,
        double expectedTotal)
    {
        decimal result = _calculator.ApplyPercentDiscount(
            (decimal)orderTotal,
            (decimal)discountRate);

        Assert.AreEqual((decimal)expectedTotal, result);
    }

    [DataTestMethod]
    [DataRow(CustomerTier.Standard, 95.00)]
    [DataRow(CustomerTier.Silver, 90.00)]
    [DataRow(CustomerTier.Gold, 85.00)]
    public void ApplyTierDiscount_AppliesCorrectRate(CustomerTier tier, double expectedTotal)
    {
        decimal result = _calculator.ApplyTierDiscount(100.00m, tier);

        Assert.AreEqual((decimal)expectedTotal, result);
    }

    [DataTestMethod]
    [DataRow(75.00, CustomerTier.Standard, true)]
    [DataRow(40.00, CustomerTier.Standard, false)]
    [DataRow(10.00, CustomerTier.Gold, true)]
    public void IsEligibleForFreeShipping_ReturnsExpected(
        double orderTotal,
        CustomerTier tier,
        bool expectedEligible)
    {
        bool eligible = _calculator.IsEligibleForFreeShipping((decimal)orderTotal, tier);

        Assert.AreEqual(expectedEligible, eligible);
    }
}
