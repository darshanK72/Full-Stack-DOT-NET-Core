/*
 * PROBLEM: Sales Pipeline Analyzer
 *
 * Regional sales analysts preview daily order lines before exporting to finance.
 * They filter by region, project amounts, and must see when LINQ queries actually run.
 *
 * This exercise covers:
 *   ch01 — deferred execution, method vs query syntax, terminal Sum, ToList materialization
 *   ch10 — ToList for snapshot (preview)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace RetailSales
{
    /*
     * One order line in the regional sales feed.
     */
    record SalesLine(int OrderId, string Region, decimal Amount);

    /*
     * Composes LINQ pipelines over sales lines and tracks how often the source is enumerated.
     *
     * Stores the source reference — do not copy unless MaterializeRegion calls ToList.
     */
    class SalesPipelineAnalyzer
    {
        private readonly IEnumerable<SalesLine> _source;
        private int _enumerationCount;

        /*
         * Accepts the sales feed. Wrap or observe enumerations to increment _enumerationCount.
         */
        public SalesPipelineAnalyzer(IEnumerable<SalesLine> source)
        {
            _source = source;
            _enumerationCount = 0;
        }

        /*
         * Case-insensitive filter by region. Returns deferred IEnumerable — no ToList here.
         */
        public IEnumerable<SalesLine> FilterByRegion(string region)
        {
            // TODO: Where with StringComparison.OrdinalIgnoreCase on Region
            throw new NotImplementedException();
        }

        /*
         * Projects Amount only. Deferred.
         */
        public IEnumerable<decimal> SelectAmounts()
        {
            // TODO: Select Amount from _source (or chain from FilterByRegion in demo)
            throw new NotImplementedException();
        }

        /*
         * Returns how many times the underlying source was enumerated through this analyzer.
         */
        public int CountExecutedQueries() => _enumerationCount;

        /*
         * Filter by region then materialize immediately with ToList.
         */
        public IReadOnlyList<SalesLine> MaterializeRegion(string region)
        {
            // TODO: FilterByRegion(...).ToList()
            throw new NotImplementedException();
        }

        /*
         * Terminal Sum of Amount for one region.
         */
        public decimal TotalForRegion(string region)
        {
            // TODO: FilterByRegion then Sum on Amount
            throw new NotImplementedException();
        }

        /*
         * Same filter as FilterByRegion but using query syntax (from / where / select).
         */
        public IEnumerable<SalesLine> QuerySyntaxRegion(string region)
        {
            // TODO: query syntax equivalent to FilterByRegion
            throw new NotImplementedException();
        }
    }

    /*
     * Seed data for demos.
     */
    static class SalesDemoData
    {
        public static IEnumerable<SalesLine> SampleLines()
        {
            // TODO: yield at least 6 lines across 3 regions
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: wrap SampleLines in counting enumerable; build analyzer
            // TODO: build deferred pipeline without enumerating — print "Pipeline built"
            // TODO: TotalForRegion("east") + print CountExecutedQueries
            // TODO: foreach deferred pipeline — print amounts + updated count
            // TODO: MaterializeRegion("west") count
            // TODO: QuerySyntaxRegion("east") count matches method syntax
            throw new NotImplementedException();
        }
    }
}
