/*
 * PROBLEM: Shipment Planner
 *
 * Dock shipment queue with multiple sort orders and read-only snapshot.
 *
 * This exercise covers:
 *   ch03 — List<T> CRUD, Sort, FindAll, AsReadOnly
 *   ch03 — IComparable<T>, IComparer<T>, Comparison<T>
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ShipmentPlanning
{
    sealed class ShipmentItem : IComparable<ShipmentItem>
    {
        public string Sku { get; }
        public int Quantity { get; set; }
        public int Priority { get; set; }

        public ShipmentItem(string sku, int quantity, int priority)
        {
            Sku = sku;
            Quantity = quantity;
            Priority = priority;
        }

        public int CompareTo(ShipmentItem? other)
        {
            // TODO: lower Priority value ships first
            throw new NotImplementedException();
        }

        public override string ToString() => $"{Sku} ×{Quantity} (P{Priority})";
    }

    sealed class ShipmentItemSkuComparer : IComparer<ShipmentItem>
    {
        public int Compare(ShipmentItem? x, ShipmentItem? y)
        {
            // TODO: compare Sku ordinal
            throw new NotImplementedException();
        }
    }

    class ShipmentPlanner
    {
        private readonly List<ShipmentItem> _queue = new List<ShipmentItem>();

        public int Count => _queue.Count;

        public void Add(ShipmentItem item) => _queue.Add(item);

        public void SortByPriority()
        {
            // TODO: parameterless Sort()
            throw new NotImplementedException();
        }

        public void SortBySku()
        {
            // TODO: Sort with ShipmentItemSkuComparer
            throw new NotImplementedException();
        }

        public void SortByQuantityDescending()
        {
            // TODO: Sort with Comparison lambda — largest Quantity first
            throw new NotImplementedException();
        }

        public List<ShipmentItem> FindHeavy(int minQuantity)
        {
            // TODO: FindAll
            throw new NotImplementedException();
        }

        public ReadOnlyCollection<ShipmentItem> PublishSnapshot()
        {
            // TODO: AsReadOnly()
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: seed 3+ items, demo each sort, heavy filter, snapshot + live update
            throw new NotImplementedException();
        }
    }
}
