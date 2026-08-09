/*
 * PROBLEM: Warehouse Order Router
 *
 * Fulfillment routing classifies orders by priority and value, assigns aisles
 * via pattern matching, parses quantity overrides with out variables, and
 * updates shared order slots through ref returns.
 *
 * This exercise covers:
 *   ch05 — digit separators and expression-bodied members
 *   ch05 — switch pattern matching on tuples
 *   ch05 — out variables at TryParse call site
 *   ch05 — tuple returns and deconstruction
 *   ch05 — local functions and ref returns
 */

using System;
using System.Collections.Generic;

namespace WarehouseRouting
{
    enum OrderPriority
    {
        Standard,
        Express,
        Critical
    }

    /*
     * Warehouse constants with digit separators for readability.
     */
    static class WarehouseConstants
    {
        public const decimal HighValueThreshold = 10_000m;
        public const decimal ExpressSurcharge = 250.00m;
    }

    /*
     * Single fulfillment order line with computed total and high-value flag.
     */
    class Order
    {
        public int OrderId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public OrderPriority Priority { get; set; } = OrderPriority.Standard;

        public decimal LineTotal => Quantity * UnitPrice;

        public bool IsHighValue => LineTotal >= WarehouseConstants.HighValueThreshold;
    }

    /*
     * Routes orders to aisles and applies express surcharges.
     * No Console I/O in this class.
     */
    class OrderRouter
    {
        /*
         * Returns aisle code and surcharge for the order.
         *
         * Uses a local function ResolveAisle with switch on (Priority, IsHighValue).
         * Surcharge 250 for Express or Critical; 0 otherwise.
         */
        public (string Aisle, decimal Surcharge) Route(Order order)
        {
            // TODO: local function + switch patterns; compute surcharge
            throw new NotImplementedException();
        }

        /*
         * Parses text to int via TryParse with out variable.
         *
         * On success: sets order.Quantity and returns true.
         * On failure: leaves quantity unchanged, returns false.
         */
        public bool TryApplyQuantityOverride(Order order, string text, out int appliedQuantity)
        {
            appliedQuantity = 0;
            // TODO: int.TryParse with out var at call site pattern
            throw new NotImplementedException();
        }

        /*
         * Returns ref to order with matching OrderId in the array.
         *
         * Throws KeyNotFoundException when no match.
         */
        public ref Order FindOrderById(Order[] orders, int orderId)
        {
            // TODO: loop and ref return matching element
            throw new NotImplementedException();
        }

        /*
         * Counts express (Express + Critical) vs standard orders.
         * Returned tuple uses named elements ExpressCount and StandardCount.
         */
        public (int ExpressCount, int StandardCount) CountByPriority(IEnumerable<Order> orders)
        {
            // TODO: tally by priority
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed Order[] with mixed priorities/values
            // TODO: deconstruct CountByPriority result
            // TODO: route each order; print aisle and surcharge
            // TODO: TryApplyQuantityOverride valid and invalid text
            // TODO: FindOrderById ref return; set Critical; print updated priority
            throw new NotImplementedException();
        }
    }
}
