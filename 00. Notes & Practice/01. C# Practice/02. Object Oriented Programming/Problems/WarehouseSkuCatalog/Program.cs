/*
 * PROBLEM: Warehouse SKU Catalog
 *
 * Warehouse staff locate products by aisle index or SKU string. Inventory
 * service tracks how many catalog instances were created for diagnostics.
 *
 * This exercise covers:
 *   ch02 — auto-properties; full property with validation; override ToString
 *   ch02 — int and string indexers on SkuCatalog
 *   ch03 — copy constructor on SkuItem
 *   ch04 — static instance counter; static factory CreateEmpty
 */

using System;
using System.Collections.Generic;

namespace WarehouseOperations
{
    /*
     * One stock-keeping unit with validated quantity and init-only DateAdded.
     */
    class SkuItem
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }

        private int _quantityOnHand;
        public int QuantityOnHand
        {
            get => _quantityOnHand;
            set
            {
                // TODO: reject negative values
                throw new NotImplementedException();
            }
        }

        public DateTime DateAdded { get; init; }

        public SkuItem(string sku, string name, decimal unitPrice, int quantityOnHand, DateTime dateAdded)
        {
            // TODO: validate and assign all fields
            throw new NotImplementedException();
        }

        public SkuItem(SkuItem other)
        {
            // TODO: copy constructor — copy all fields into a new instance
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            // TODO: "{Sku}: {Name} @ {UnitPrice:C} (qty {QuantityOnHand})"
            throw new NotImplementedException();
        }
    }

    /*
     * In-memory catalog with slot index and SKU lookup indexers.
     */
    class SkuCatalog
    {
        private static int _instancesCreated;
        private readonly List<SkuItem> _items = new List<SkuItem>();

        public static int InstancesCreated => _instancesCreated;

        public static SkuCatalog CreateEmpty()
        {
            // TODO: factory returning new catalog
            throw new NotImplementedException();
        }

        public SkuCatalog()
        {
            // TODO: increment _instancesCreated
            throw new NotImplementedException();
        }

        public SkuItem this[int index]
        {
            get
            {
                // TODO: validate index; return item
                throw new NotImplementedException();
            }
            set
            {
                // TODO: validate index; replace slot
                throw new NotImplementedException();
            }
        }

        public SkuItem this[string sku]
        {
            get
            {
                // TODO: case-insensitive lookup; KeyNotFoundException if missing
                throw new NotImplementedException();
            }
        }

        public bool Add(SkuItem item)
        {
            // TODO: false if SKU already present (case-insensitive)
            throw new NotImplementedException();
        }

        public bool Remove(string sku)
        {
            // TODO: case-insensitive remove
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: CreateEmpty; add items; mutate via int indexer
            // TODO: fetch via string indexer; clone item via copy constructor
            // TODO: print InstancesCreated
            throw new NotImplementedException();
        }
    }
}
