/*
 * PROBLEM: SKU Catalog Registry
 *
 * Fast SKU lookup with Dictionary plus insertion-order audit list.
 *
 * This exercise covers:
 *   ch03 — List<T> for ordered audit trail
 *   ch04 — Dictionary, TryGetValue, case-insensitive keys, List vs Dictionary
 */

using System;
using System.Collections.Generic;

namespace SkuCatalog
{
    class Product
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        public override string ToString() => $"{Sku} | {Name} | {UnitPrice:C}";
    }

    class SkuCatalogRegistry
    {
        public Dictionary<string, Product> BySku { get; } =
            new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

        public List<Product> InsertionOrder { get; } = new List<Product>();

        public bool Register(Product product)
        {
            // TODO: reject null/empty SKU; false if duplicate SKU
            throw new NotImplementedException();
        }

        public bool TryGet(string sku, out Product? product)
        {
            // TODO: TryGetValue
            throw new NotImplementedException();
        }

        public bool UpdatePrice(string sku, decimal newPrice)
        {
            // TODO: update dictionary entry; same object in list reflects change
            throw new NotImplementedException();
        }

        public Product GetRequired(string sku)
        {
            // TODO: indexer GET — KeyNotFoundException if missing
            throw new NotImplementedException();
        }

        public IReadOnlyList<Product> AllInInsertionOrder() => InsertionOrder;
    }

    static class CatalogLookupDemo
    {
        public static Product? FindBySkuScan(List<Product> list, string sku)
        {
            // TODO: linear scan for comparison demo
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: register 3 products, duplicate attempt, TryGet, UpdatePrice
            // TODO: compare scan vs TryGet in one sentence
            throw new NotImplementedException();
        }
    }
}
