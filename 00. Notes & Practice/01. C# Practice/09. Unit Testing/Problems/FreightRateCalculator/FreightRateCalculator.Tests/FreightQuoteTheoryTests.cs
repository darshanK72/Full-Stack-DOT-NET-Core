using System;
using System.Collections.Generic;
using LogisticsPricing;
using Xunit;

namespace FreightRateCalculator.Tests;

public sealed class FreightQuoteTheoryTests
{
    private readonly FreightQuoteService _service = new FreightQuoteService();

    [Theory]
    [InlineData(100, 0, 0)]
    [InlineData(100, 12, 12)]
    // TODO: add at least two more InlineData rows including 100% surcharge
    public void ApplyFuelSurcharge_ReturnsExpectedAmount(
        decimal baseAmount,
        decimal surchargePercent,
        decimal expected)
    {
        // TODO: Act ApplyFuelSurcharge; Assert.Equal with precision 2
        throw new NotImplementedException();
    }

    // TODO: replace with >= 3 rows (include empty list) per PROBLEM.md
    public static IEnumerable<object[]> SubtotalMemberData =>
        Array.Empty<object[]>();

    [Theory]
    [MemberData(nameof(SubtotalMemberData))]
    public void CalculateSubtotal_MemberDataRows_ReturnExpected(
        IReadOnlyList<ShipmentLine> lines,
        decimal expectedSubtotal)
    {
        // TODO: Act CalculateSubtotal; Assert.Equal expected
        throw new NotImplementedException();
    }
}
