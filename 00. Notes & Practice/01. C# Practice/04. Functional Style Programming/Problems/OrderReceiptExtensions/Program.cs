/*
 * PROBLEM: Order Receipt Extensions
 *
 * An e-commerce receipt formatter adds display helpers via extension methods on
 * strings, dates, order lines, and line collections without modifying sealed types.
 *
 * This exercise covers:
 *   ch04 — extension method rules (static class, this parameter)
 *   ch04 — extending sealed BCL and custom types
 *   ch04 — generic extensions (Clamp)
 *   ch04 — extending IEnumerable (TotalAmount, ToReceiptBlock)
 */

using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrderFormatting
{
    /*
     * One immutable line on a customer order.
     * LineTotal is computed — quantity times unit price.
     */
    sealed class OrderLine
    {
        public string Sku { get; }
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }

        public decimal LineTotal => Quantity * UnitPrice;

        public OrderLine(string sku, string productName, int quantity, decimal unitPrice)
        {
            Sku = sku;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }

    /*
     * String display helpers — compiler rewrites instance calls to static invocations.
     */
    static class StringExtensions
    {
        /*
         * Wraps value in square brackets for display labels.
         *
         * Null/empty returns empty string or value unchanged per spec in PROBLEM.md.
         */
        public static string ToDisplayLabel(this string value)
        {
            // TODO: return bracketed label
            throw new NotImplementedException();
        }

        /*
         * Shortens value to maxLength characters, appending ellipsis when truncated.
         *
         * maxLength must be >= 0; throws ArgumentOutOfRangeException when negative.
         */
        public static string Truncate(this string value, int maxLength)
        {
            // TODO: return original or truncated with … suffix
            throw new NotImplementedException();
        }

        /*
         * True when value is null, empty, or whitespace only.
         */
        public static bool IsNullOrBlank(this string? value)
        {
            // TODO: delegate to string.IsNullOrWhiteSpace
            throw new NotImplementedException();
        }
    }

    /*
     * DateTime formatting helpers for order headers.
     */
    static class DateTimeExtensions
    {
        /*
         * Returns long date label using current culture (e.g. Monday, 07 Apr 2026).
         */
        public static string ToOrderDateLabel(this DateTime date)
        {
            // TODO: format with CultureInfo.CurrentCulture
            throw new NotImplementedException();
        }

        /*
         * True when date falls on Saturday or Sunday.
         */
        public static bool IsWeekend(this DateTime date)
        {
            // TODO: check DayOfWeek
            throw new NotImplementedException();
        }
    }

    /*
     * Receipt line formatting for a single OrderLine.
     */
    static class OrderLineExtensions
    {
        /*
         * Formats one receipt row: SKU | name x qty = line total (currency).
         */
        public static string ToReceiptLine(this OrderLine line)
        {
            // TODO: format sku, product, quantity, LineTotal as currency
            throw new NotImplementedException();
        }
    }

    /*
     * Generic clamp for any IComparable type — LINQ-style reusable extension.
     */
    static class ComparableExtensions
    {
        /*
         * Returns value clamped between min and max inclusive.
         *
         * When min > max, throw ArgumentException.
         */
        public static T Clamp<T>(this T value, T min, T max)
            where T : IComparable<T>
        {
            // TODO: compare and return min, max, or value
            throw new NotImplementedException();
        }
    }

    /*
     * Aggregations and blocks over a sequence of order lines.
     */
    static class OrderLineCollectionExtensions
    {
        /*
         * Sums LineTotal across all lines (manual loop or Sum extension preview).
         */
        public static decimal TotalAmount(this IEnumerable<OrderLine> lines)
        {
            // TODO: sum line totals
            throw new NotImplementedException();
        }

        /*
         * Joins ToReceiptLine() for each line with Environment.NewLine separators.
         */
        public static string ToReceiptBlock(this IEnumerable<OrderLine> lines)
        {
            // TODO: project each line to receipt line and join
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: build OrderLine[] sample cart
            // TODO: chain string extensions on customer name; format order date label
            // TODO: Clamp a unit price between min/max
            // TODO: print ToReceiptBlock and TotalAmount for the cart
            throw new NotImplementedException();
        }
    }
}
