/*
 * PROBLEM: Pick Ticket Enumerator
 *
 * Custom IEnumerable pick batch and lazy heavy-line filters.
 *
 * This exercise covers:
 *   ch08 — IEnumerable/IEnumerator, yield return, modify-during-foreach
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace PickTickets
{
    class PickLine
    {
        public PickLine(string sku, int quantity, decimal weightKg)
        {
            Sku = sku;
            Quantity = quantity;
            WeightKg = weightKg;
        }

        public string Sku { get; }
        public int Quantity { get; }
        public decimal WeightKg { get; }
        public decimal TotalWeightKg => Quantity * WeightKg;

        public override string ToString() =>
            $"{Sku} × {Quantity} ({TotalWeightKg:0.##} kg)";
    }

    class PickBatch : IEnumerable<PickLine>
    {
        private readonly PickLine[] _lines;

        public PickBatch(params PickLine[] lines) => _lines = lines;

        public int LineCount => _lines.Length;

        public IEnumerator<PickLine> GetEnumerator()
        {
            // TODO: return custom enumerator OR yield-based private iterator
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    static class PickLineFilters
    {
        public static IEnumerable<PickLine> HeavyLines(IEnumerable<PickLine> source, decimal minTotalKg)
        {
            // TODO: yield return lines where TotalWeightKg >= minTotalKg
            throw new NotImplementedException();
        }

        public static IEnumerable<string> SkuSegments(string sku)
        {
            // TODO: yield return non-empty segments split on '-'
            throw new NotImplementedException();
        }
    }

    class PickLineCollection
    {
        private readonly List<PickLine> _lines = new List<PickLine>();

        public IEnumerable<PickLine> Lines => _lines;

        public void Add(PickLine line) => _lines.Add(line);

        public decimal TotalWeight()
        {
            // TODO: foreach sum TotalWeightKg
            throw new NotImplementedException();
        }

        public string DemoModifyDuringForeach()
        {
            // TODO: foreach; on 2nd iteration Add to same list; catch InvalidOperationException
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: foreach PickBatch; manual IEnumerator with using
            // TODO: HeavyLines lazy demo; SkuSegments("PANEL-A")
            // TODO: DemoModifyDuringForeach message
            throw new NotImplementedException();
        }
    }
}
