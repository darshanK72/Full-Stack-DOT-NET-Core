/*
 * PROBLEM: Flexible Feed Adapter
 *
 * Warehouse integration maps strongly typed SKU records and vendor-specific
 * dynamic import rows into a normalized snapshot, using nameof for safe
 * error messages when required fields are missing.
 *
 * This exercise covers:
 *   ch04 — dynamic and DynamicObject late binding
 *   ch04 — nameof for compile-time field names in exceptions
 *   ch04 — default literal and default(T)
 *   ch04 — var vs dynamic contrast (strong path vs dynamic path)
 */

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace WarehouseIntegration
{
    /*
     * Fixed-schema inventory record from the internal API.
     */
    class InventoryItem
    {
        public required string Sku { get; init; }
        public required string Name { get; init; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    /*
     * Dictionary-backed dynamic row for third-party feeds with varying column names.
     * Property access is case-insensitive via StringComparer.OrdinalIgnoreCase.
     */
    class FlexibleImportRow : DynamicObject
    {
        private readonly Dictionary<string, object?> _fields =
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            // TODO: lookup binder.Name in _fields
            throw new NotImplementedException();
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            // TODO: store value in _fields
            throw new NotImplementedException();
        }

        /*
         * Read-only view of all fields after dynamic assignment.
         */
        public IReadOnlyDictionary<string, object?> Snapshot()
        {
            return _fields;
        }
    }

    /*
     * Canonical row after adapter normalization.
     */
    class NormalizedRow
    {
        public NormalizedRow(string sku, string name, int quantity, decimal unitPrice)
        {
            Sku = sku;
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public string Sku { get; }
        public string Name { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }
    }

    /*
     * Maps strong and dynamic sources to NormalizedRow.
     * No Console I/O in this class.
     */
    class FeedAdapter
    {
        /*
         * Direct map from InventoryItem — no dynamic involved.
         */
        public NormalizedRow FromStrongTyped(InventoryItem item)
        {
            // TODO: construct NormalizedRow from item properties
            throw new NotImplementedException();
        }

        /*
         * Reads Sku, Name, Quantity, UnitPrice from dynamic row.
         *
         * Throws InvalidOperationException with "Missing required field: {nameof(...)}"
         * when a required value is null or absent.
         * Coerce Quantity and UnitPrice with Convert methods.
         */
        public NormalizedRow FromDynamic(dynamic row)
        {
            // TODO: read fields; validate; coerce numerics
            throw new NotImplementedException();
        }

        /*
         * Returns field value coerced to T, or default(T) when field is missing.
         */
        public T GetOrDefault<T>(dynamic row, string fieldName)
        {
            // TODO: try read fieldName; return default when absent
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: map InventoryItem via FromStrongTyped; print
            // TODO: populate FlexibleImportRow dynamically; map via FromDynamic
            // TODO: catch missing Sku exception; print message
            // TODO: GetOrDefault<int> on missing field; print 0
            throw new NotImplementedException();
        }
    }
}
