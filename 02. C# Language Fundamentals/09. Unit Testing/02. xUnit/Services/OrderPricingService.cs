using System;
using System.Collections.Generic;
using XUnit.Models;

namespace XUnit.Services;

/*
 * SECTION 3: ORDER PRICING SERVICE — SYSTEM UNDER TEST (SUT)
 *
 * Production logic lives here; XUnit.Tests/ references this project and asserts
 * on these methods. Validation throws ArgumentOutOfRangeException so tests can
 * demonstrate Assert.Throws and Assert.Contains on exception messages.
 *
 * | Method                    | Behavior                                      |
 * |---------------------------|-----------------------------------------------|
 * | CalculateSubtotal         | Sum of Quantity × UnitPrice; empty → 0        |
 * | ApplyDiscount             | Percent off subtotal; 0–100 inclusive           |
 * | CalculateTax              | Percent tax on post-discount amount           |
 * | CalculateGrandTotal       | Subtotal → discount → tax → total             |
 * | IsEligibleForBulkDiscount | true when subtotal >= 100                     |
 */
public sealed class OrderPricingService
{
    public decimal CalculateSubtotal(IReadOnlyList<OrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (lines.Count == 0)
        {
            return 0m;
        }

        decimal subtotal = 0m;
        foreach (OrderLine line in lines)
        {
            if (line.Quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lines), "Quantity cannot be negative.");
            }

            subtotal += line.UnitPrice * line.Quantity;
        }

        return subtotal;
    }

    public decimal ApplyDiscount(decimal subtotal, decimal discountPercent)
    {
        if (discountPercent < 0m || discountPercent > 100m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(discountPercent),
                "Discount must be between 0 and 100.");
        }

        return subtotal * (1m - discountPercent / 100m);
    }

    public decimal CalculateTax(decimal taxableAmount, decimal taxRatePercent)
    {
        if (taxableAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(taxableAmount));
        }

        if (taxRatePercent < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(taxRatePercent));
        }

        return taxableAmount * (taxRatePercent / 100m);
    }

    public decimal CalculateGrandTotal(Order order, decimal discountPercent, decimal taxRatePercent)
    {
        decimal subtotal = CalculateSubtotal(order.Lines);
        decimal afterDiscount = ApplyDiscount(subtotal, discountPercent);
        decimal tax = CalculateTax(afterDiscount, taxRatePercent);
        return afterDiscount + tax;
    }

    public bool IsEligibleForBulkDiscount(IReadOnlyList<OrderLine> lines)
    {
        return CalculateSubtotal(lines) >= 100m;
    }
}
