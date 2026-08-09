/*
 * PROBLEM: Freight Rate Calculator
 *
 * A logistics portal quotes parcel shipping from weight tiers and fuel surcharges.
 * Parameterized xUnit tests verify each rate bracket without duplicated methods.
 *
 * This exercise covers:
 *   ch02 — [Fact] vs [Theory]
 *   ch02 — [InlineData] for literal parameter rows
 *   ch02 — [MemberData] for complex shipment line collections
 *   ch02 — ITestOutputHelper for per-test diagnostics
 */

using System;
using System.Collections.Generic;

namespace LogisticsPricing
{
    /*
     * One line on a freight quote — weight and per-kg rate.
     */
    public sealed class ShipmentLine
    {
        public string Sku { get; init; } = string.Empty;
        public decimal WeightKg { get; init; }
        public decimal UnitRate { get; init; }
    }

    /*
     * Pure pricing logic for freight quotes. No I/O.
     */
    public sealed class FreightQuoteService
    {
        /*
         * Line charge = WeightKg * UnitRate.
         */
        public decimal CalculateLineWeightCharge(ShipmentLine line)
        {
            // TODO: multiply weight by unit rate
            throw new NotImplementedException();
        }

        /*
         * Applies surchargePercent (0–100) to baseAmount.
         * Returns 0 when baseAmount <= 0.
         */
        public decimal ApplyFuelSurcharge(decimal baseAmount, decimal surchargePercent)
        {
            // TODO: clamp percent to [0,100]; compute surcharge
            throw new NotImplementedException();
        }

        /*
         * Sum of CalculateLineWeightCharge for all lines.
         * Empty list returns 0.
         */
        public decimal CalculateSubtotal(IReadOnlyList<ShipmentLine> lines)
        {
            // TODO: sum line charges
            throw new NotImplementedException();
        }

        /*
         * True when CalculateSubtotal(lines) >= 500m.
         */
        public bool QualifiesForBulkDiscount(IReadOnlyList<ShipmentLine> lines)
        {
            // TODO: compare subtotal to 500
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: two-line sample; print subtotal, 12% fuel surcharge, bulk flag
            throw new NotImplementedException();
        }
    }
}
