using System;
using System.Collections.Generic;
using XUnit.Models;
using XUnit.Services;
using XUnit.Tests.Fixtures;
using Xunit;

namespace XUnit.Tests;

/*
 * SECTION 5: [Fact] — SINGLE-SCENARIO TESTS
 *
 * [Fact] marks one test method with no parameters. Use it when a single
 * Arrange-Act-Assert path is enough. AAA structure is assumed from ch.01.
 *
 * This file also demonstrates common xUnit Assert helpers:
 *
 *   | Assert member   | Use case                                      |
 *   |-----------------|-----------------------------------------------|
 *   | Equal           | Exact value match (optional precision for decimal) |
 *   | True / False    | Boolean conditions                            |
 *   | Contains        | Substring in string or item in collection     |
 *   | Throws<T>       | Action must throw exception type T            |
 *   | InRange         | Value between low and high (inclusive)        |
 *
 * MSTest uses [TestMethod] instead of [Fact] — COVERED IN DETAIL LATER → 03. MSTest
 */
public sealed class FactTests
{
    private readonly OrderPricingService _pricing = new OrderPricingService();

    [Fact]
    public void CalculateSubtotal_EmptyLines_ReturnsZero()
    {
        IReadOnlyList<OrderLine> lines = [];

        decimal subtotal = _pricing.CalculateSubtotal(lines);

        Assert.Equal(0m, subtotal);
    }

    [Fact]
    public void CalculateSubtotal_TwoLines_ReturnsSumOfLineTotals()
    {
        IReadOnlyList<OrderLine> lines =
        [
            new OrderLine { Sku = "A", ProductName = "Item A", Quantity = 2, UnitPrice = 25m },
            new OrderLine { Sku = "B", ProductName = "Item B", Quantity = 1, UnitPrice = 10m },
        ];

        decimal subtotal = _pricing.CalculateSubtotal(lines);

        Assert.Equal(60m, subtotal);
    }

    [Fact]
    public void IsEligibleForBulkDiscount_SubtotalAtLeast100_ReturnsTrue()
    {
        IReadOnlyList<OrderLine> lines =
        [
            new OrderLine { Sku = "BULK", ProductName = "Bulk Pack", Quantity = 1, UnitPrice = 100m },
        ];

        bool eligible = _pricing.IsEligibleForBulkDiscount(lines);

        Assert.True(eligible);
    }

    [Fact]
    public void IsEligibleForBulkDiscount_SubtotalBelow100_ReturnsFalse()
    {
        IReadOnlyList<OrderLine> lines =
        [
            new OrderLine { Sku = "SMALL", ProductName = "Small Item", Quantity = 1, UnitPrice = 49.99m },
        ];

        bool eligible = _pricing.IsEligibleForBulkDiscount(lines);

        Assert.False(eligible);
    }

    [Fact]
    public void ApplyDiscount_InvalidPercent_ThrowsWithExpectedMessage()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => _pricing.ApplyDiscount(100m, 150m));

        Assert.Contains("Discount must be between 0 and 100", exception.Message);
    }

    [Fact]
    public void CalculateSubtotal_NegativeQuantity_ThrowsArgumentOutOfRange()
    {
        IReadOnlyList<OrderLine> lines =
        [
            new OrderLine { Sku = "BAD", ProductName = "Bad Line", Quantity = -1, UnitPrice = 10m },
        ];

        Assert.Throws<ArgumentOutOfRangeException>(() => _pricing.CalculateSubtotal(lines));
    }

    [Fact]
    public void CalculateGrandTotal_SampleOrder_IsWithinExpectedRange()
    {
        Order order = OrderCatalogFixture.CreateStandardOrder();
        const decimal discountPercent = 10m;
        const decimal taxRatePercent = 8m;

        decimal grandTotal = _pricing.CalculateGrandTotal(order, discountPercent, taxRatePercent);

        // subtotal 138.99 → 10% off → 125.091 → 8% tax → ~135.10
        Assert.InRange(grandTotal, 135.09m, 135.11m);
    }

    [Fact]
    public void StandardOrder_OrderId_ContainsTestPrefix()
    {
        Order order = OrderCatalogFixture.CreateStandardOrder();

        Assert.Contains("ORD-TEST", order.OrderId);
    }
}
