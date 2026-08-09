/*
 * PROBLEM: Reorder Report
 *
 * SKU-ordered reorder lines and sorted regional sales maps.
 *
 * This exercise covers:
 *   ch07 — SortedList, SortedDictionary, IComparer on keys
 */

using System;
using System.Collections.Generic;

namespace ProcurementReports
{
    class ReorderReport
    {
        private readonly SortedList<string, int> _lines = new SortedList<string, int>();

        public void SetLine(string sku, int quantity) => _lines[sku] = quantity;

        public bool TryGetQuantity(string sku, out int qty) => _lines.TryGetValue(sku, out qty);

        public void RemoveSku(string sku) => _lines.Remove(sku);

        public string? LowestSku() =>
            _lines.Count > 0 ? _lines.Keys[0] : null;

        public void PrintReport()
        {
            // TODO: loop index 0..Count-1, print Keys[i] and Values[i]
            throw new NotImplementedException();
        }
    }

    class RegionSalesMap
    {
        private readonly SortedDictionary<string, int> _regions = new SortedDictionary<string, int>();

        public void AddRegion(string code, int units)
        {
            // TODO: Add — guard duplicate or document throw
            throw new NotImplementedException();
        }

        public void UpdateRegion(string code, int units) => _regions[code] = units;

        public IEnumerable<KeyValuePair<string, int>> OrderedEntries() => _regions;

        public string? FirstRegionKey()
        {
            // TODO: first key from foreach on Keys
            throw new NotImplementedException();
        }

        public void RemoveRegion(string code) => _regions.Remove(code);
    }

    class TagCountBoard
    {
        private readonly SortedDictionary<string, int> _tags =
            new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public int this[string tag]
        {
            get => _tags[tag];
            set => _tags[tag] = value;
        }

        public int Count => _tags.Count;
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: ReorderReport out-of-order SKUs → PrintReport A,M,Z
            // TODO: RegionSalesMap add/update/remove
            // TODO: TagCountBoard dotnet then CSharp overwrites csharp key
            throw new NotImplementedException();
        }
    }
}
