/*
 * PROBLEM: Catalog Pricing Engine
 *
 * A retail pricing team applies transforms and filters to SKU unit prices using
 * delegate pipelines before printing a formatted price list. Lambdas supply the
 * behavior at the call site without one-off named methods.
 *
 * This exercise covers:
 *   ch02 — lambda syntax assigned to custom delegate types
 *   ch02 — expression vs statement lambdas
 *   ch02 — target-typed lambdas at call sites
 *   ch02 — manual filter/map pipeline (LINQ preview)
 */

using System;
using System.Collections.Generic;

namespace RetailPricing
{
    /*
     * Maps one unit price to another (markup, discount, tax preview).
     */
    delegate decimal PriceTransform(decimal unitPrice);

    /*
     * Predicate over a single price — true means keep the price in the pipeline.
     */
    delegate bool PriceFilter(decimal unitPrice);

    /*
     * Static helpers that accept delegate parameters — reusable pipeline stages.
     * No Console I/O in this class.
     */
    static class PricePipeline
    {
        /*
         * Applies transform to every element; returns a new array of the same length.
         *
         * prices and transform must not be null.
         */
        public static decimal[] ApplyToAll(decimal[] prices, PriceTransform transform)
        {
            // TODO: loop and invoke transform on each price
            throw new NotImplementedException();
        }

        /*
         * Overload for read-only lists — same semantics as the array overload.
         */
        public static decimal[] ApplyToAll(IReadOnlyList<decimal> prices, PriceTransform transform)
        {
            // TODO: loop and invoke transform on each price
            throw new NotImplementedException();
        }

        /*
         * Returns a new array containing only prices where predicate returns true.
         *
         * Preserves relative order of matches.
         */
        public static decimal[] FilterPrices(decimal[] prices, PriceFilter predicate)
        {
            // TODO: two-pass or single-pass filter into new array
            throw new NotImplementedException();
        }

        /*
         * Overload for read-only lists — same semantics as the array overload.
         */
        public static decimal[] FilterPrices(IReadOnlyList<decimal> prices, PriceFilter predicate)
        {
            // TODO: filter into new array
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed decimal[] unitPrices (at least four SKUs)
            // TODO: ApplyToAll with lambda transform (e.g. ten percent markup)
            // TODO: FilterPrices with lambda predicate (e.g. price >= threshold)
            // TODO: print original, transformed, and filtered arrays formatted as currency
            throw new NotImplementedException();
        }
    }
}
