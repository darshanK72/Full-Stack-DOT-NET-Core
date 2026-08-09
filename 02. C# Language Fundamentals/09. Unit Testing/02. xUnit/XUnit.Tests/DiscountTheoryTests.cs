using System.Collections.Generic;
using XUnit.Models;
using XUnit.Services;
using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 6: [Theory], [InlineData], AND [MemberData]
 *
 * [Theory] marks a test that runs once per data row. Two common sources:
 *
 *   [InlineData(...)]  — literal arguments on the attribute; best for small tables
 *   [MemberData(...)]  — static property or method returning IEnumerable<object[]>;
 *                        use when rows need objects, lists, or computed values
 *
 * Each row must match the theory method signature (count and types). A failing
 * row reports its arguments in the test output — ideal for discount/tax tables.
 *
 * MemberData signature: public static IEnumerable<object[]> Name =>
 *     new List<object[]> { new object[] { arg1, arg2, ... }, ... };
 */
public sealed class DiscountTheoryTests
{
    private readonly OrderPricingService _pricing = new OrderPricingService();

    [Theory]
    [InlineData(100, 0, 100)]
    [InlineData(100, 10, 90)]
    [InlineData(200, 25, 150)]
    [InlineData(50, 100, 0)]
    public void ApplyDiscount_ReturnsExpectedAmount(
        decimal subtotal,
        decimal discountPercent,
        decimal expected)
    {
        decimal result = _pricing.ApplyDiscount(subtotal, discountPercent);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(90, 8, 7.2)]
    [InlineData(125.091, 8, 10.00728)]
    [InlineData(0, 8, 0)]
    public void CalculateTax_ReturnsExpectedAmount(
        decimal taxableAmount,
        decimal taxRatePercent,
        decimal expected)
    {
        decimal result = _pricing.CalculateTax(taxableAmount, taxRatePercent);

        Assert.Equal(expected, result, precision: 5);
    }

    public static IEnumerable<object[]> SubtotalMemberData =>
        new List<object[]>
        {
            new object[]
            {
                new List<OrderLine>
                {
                    new OrderLine { Sku = "A", ProductName = "Alpha", Quantity = 2, UnitPrice = 25m },
                    new OrderLine { Sku = "B", ProductName = "Beta", Quantity = 1, UnitPrice = 10m },
                },
                60m,
            },
            new object[]
            {
                new List<OrderLine>
                {
                    new OrderLine { Sku = "KB-001", ProductName = "Keyboard", Quantity = 1, UnitPrice = 79.99m },
                    new OrderLine { Sku = "MS-010", ProductName = "Mouse", Quantity = 2, UnitPrice = 29.50m },
                },
                138.99m,
            },
            new object[]
            {
                new List<OrderLine>(),
                0m,
            },
        };

    [Theory]
    [MemberData(nameof(SubtotalMemberData))]
    public void CalculateSubtotal_MemberDataRows_ReturnExpected(
        IReadOnlyList<OrderLine> lines,
        decimal expectedSubtotal)
    {
        decimal subtotal = _pricing.CalculateSubtotal(lines);

        Assert.Equal(expectedSubtotal, subtotal);
    }
}
