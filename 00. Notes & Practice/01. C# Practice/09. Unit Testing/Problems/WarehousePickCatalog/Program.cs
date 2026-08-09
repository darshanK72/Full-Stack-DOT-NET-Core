/*
 * PROBLEM: Warehouse Pick Catalog
 *
 * Warehouse pick lists pull SKU metadata from a catalog loaded once per test run.
 * xUnit class and collection fixtures share catalog setup across test classes.
 *
 * This exercise covers:
 *   ch02 — IClassFixture<T> one instance per test class
 *   ch02 — ICollectionFixture<T> + [CollectionDefinition] shared across classes
 *   ch02 — [Collection("Name")] assigns classes to same fixture scope
 */

using System;
using System.Collections.Generic;

namespace WarehouseOperations
{
    public sealed class SkuItem
    {
        public string Sku { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string PickZone { get; init; } = string.Empty;
    }

    /*
     * In-memory SKU lookup keyed by SKU code.
     */
    public sealed class SkuCatalog
    {
        private readonly Dictionary<string, SkuItem> _items = new Dictionary<string, SkuItem>();

        public int Count => _items.Count;

        /*
         * Replaces catalog contents with provided items (keyed by Sku).
         */
        public void Seed(IEnumerable<SkuItem> items)
        {
            // TODO: clear and populate _items
            throw new NotImplementedException();
        }

        /*
         * Lookup by SKU; false when missing.
         */
        public bool TryGet(string sku, out SkuItem? item)
        {
            // TODO: dictionary lookup
            throw new NotImplementedException();
        }
    }

    /*
     * Builds ordered pick-zone path from requested SKUs.
     */
    public sealed class PickListBuilder
    {
        private readonly SkuCatalog _catalog;

        public PickListBuilder(SkuCatalog catalog)
        {
            _catalog = catalog;
        }

        /*
         * Returns pick zones in request order; skips unknown SKUs.
         * Empty input returns empty list.
         */
        public IReadOnlyList<string> BuildPickPath(IEnumerable<string> skus)
        {
            // TODO: map known SKUs to PickZone
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed 3 SKUs; print pick path for two known SKUs
            throw new NotImplementedException();
        }
    }
}
