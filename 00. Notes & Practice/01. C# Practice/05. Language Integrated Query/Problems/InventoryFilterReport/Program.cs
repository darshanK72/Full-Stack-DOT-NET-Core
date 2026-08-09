/*
 * PROBLEM: Inventory Filter Report
 *
 * Mixed catalog feed with product rows and audit strings. Operations needs
 * category revenue, low-stock counts, and weighted average cost via LINQ aggregations.
 *
 * This exercise covers:
 *   ch02 — Where, OfType, Count, Sum, Average, Aggregate, empty-sequence guards
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarehouseInventory
{
    record ProductRow(string Sku, string Category, decimal UnitCost, int QuantityOnHand);

    /*
     * Filters mixed feed and computes inventory statistics with LINQ.
     *
     * All filtering/aggregation in this class uses LINQ — no manual foreach filters.
     */
    class InventoryFilterReport
    {
        private readonly IEnumerable<object> _mixedFeed;

        public InventoryFilterReport(IEnumerable<object> mixedFeed)
        {
            _mixedFeed = mixedFeed;
        }

        /*
         * Returns only ProductRow instances from the mixed feed (skips strings).
         */
        public IEnumerable<ProductRow> ProductsOnly()
        {
            // TODO: OfType<ProductRow>()
            throw new NotImplementedException();
        }

        /*
         * Products at or below the low-stock threshold.
         */
        public IEnumerable<ProductRow> LowStock(int threshold)
        {
            // TODO: ProductsOnly().Where(QuantityOnHand <= threshold)
            throw new NotImplementedException();
        }

        /*
         * Count products in category — use Count(predicate), not Where().Count().
         */
        public int CountInCategory(string category)
        {
            // TODO: Count on ProductsOnly with category match
            throw new NotImplementedException();
        }

        /*
         * Sum of UnitCost * QuantityOnHand across all products.
         */
        public decimal TotalInventoryValue()
        {
            // TODO: Sum with selector
            throw new NotImplementedException();
        }

        /*
         * Average UnitCost in category. Returns null when no products in category (no throw).
         */
        public decimal? AverageUnitCost(string category)
        {
            // TODO: filter category, guard empty, then Average or return null
            throw new NotImplementedException();
        }

        /*
         * Weighted average unit cost: sum(cost*qty)/sum(qty). Returns 0m when no products.
         */
        public decimal WeightedAverageCost()
        {
            // TODO: Aggregate for weighted sum numerator, divide by total qty
            throw new NotImplementedException();
        }
    }

    static class InventoryDemoData
    {
        public static IEnumerable<object> MixedFeed()
        {
            // TODO: at least 2 strings + 5 ProductRow across 2 categories
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: ProductsOnly count vs mixed count
            // TODO: LowStock(5) SKUs
            // TODO: TotalInventoryValue currency
            // TODO: AverageUnitCost with data and empty category
            // TODO: WeightedAverageCost
            // TODO: chained category Sum demo
            throw new NotImplementedException();
        }
    }
}
