/*
 * PROBLEM: Legacy Inventory Migrator
 *
 * Migrate ArrayList and Hashtable snapshots to List<T> and Dictionary<K,V>.
 *
 * This exercise covers:
 *   ch01 — why generics replace object storage
 *   ch02 — ArrayList, Hashtable, boxing/unboxing, casts
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace LegacyInventory
{
    class Product
    {
        public int ProductNo { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }

        public override string ToString() =>
            $"#{ProductNo} {ProductName} — {ProductPrice:C}";
    }

    class LegacyInventorySnapshot
    {
        private readonly ArrayList _lines = new ArrayList();
        private readonly Hashtable _skuQuantities = new Hashtable();

        public LegacyInventorySnapshot()
        {
            // TODO: seed _lines with boxed int, string SKU, and one Product
            // TODO: seed _skuQuantities with at least 2 string→boxed int pairs
            throw new NotImplementedException();
        }

        public ArrayList Lines => _lines;
        public Hashtable SkuQuantities => _skuQuantities;

        public int UnboxFirstCount()
        {
            // TODO: (int)_lines[0]
            throw new NotImplementedException();
        }

        public string? LookupBin(string sku)
        {
            // TODO: optional bin lookup from hashtable (string values)
            throw new NotImplementedException();
        }
    }

    class ModernInventory
    {
        public List<Product> Catalog { get; } = new List<Product>();
        public Dictionary<string, int> QuantityBySku { get; } = new Dictionary<string, int>();

        public void ImportFromLegacy(ArrayList legacyLines, Hashtable legacySkus)
        {
            // TODO: Product → Catalog; string/int pairs → QuantityBySku; skip bad types
            throw new NotImplementedException();
        }

        public decimal CatalogTotal()
        {
            // TODO: sum ProductPrice via typed foreach
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: legacy snapshot, unbox count, import, print totals
            // TODO: one line on why List<Product> removed casts
            throw new NotImplementedException();
        }
    }
}
