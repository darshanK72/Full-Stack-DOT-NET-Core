/*
 * PROBLEM: Product Export Scanner
 *
 * A retail team exports SKU catalogs using reflection to honor DisplayLabel and
 * Exportable metadata on product properties, producing a flat-file manifest.
 *
 * This exercise covers:
 *   ch02 — AttributeUsage and custom attribute definitions
 *   ch02 — Type metadata and PropertyInfo.GetValue
 *   ch02 — GetCustomAttribute for declarative export rules
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace RetailCatalog
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    sealed class DisplayLabelAttribute : Attribute
    {
        public DisplayLabelAttribute(string label) => Label = label;
        public string Label { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    sealed class ExportableAttribute : Attribute
    {
        public ExportableAttribute(bool include = true) => Include = include;
        public bool Include { get; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    sealed class EntityTableAttribute : Attribute
    {
        public EntityTableAttribute(string tableName) => TableName = tableName;
        public string TableName { get; }
    }

    /*
     * Sample domain type decorated with export metadata.
     * Stock has DisplayLabel but no Exportable — excluded from manifest.
     */
    [EntityTable("Products")]
    class Product
    {
        [DisplayLabel("SKU Code")]
        [Exportable]
        public string Sku { get; set; } = string.Empty;

        [DisplayLabel("Product Name")]
        [Exportable]
        public string Name { get; set; } = string.Empty;

        [DisplayLabel("Unit Price")]
        [Exportable]
        public decimal Price { get; set; }

        [DisplayLabel("Stock Level")]
        public int Stock { get; set; }
    }

    /*
     * One export column: human header + backing property name.
     */
    class ExportColumn
    {
        public ExportColumn(string header, string propertyName)
        {
            Header = header;
            PropertyName = propertyName;
        }

        public string Header { get; }
        public string PropertyName { get; }
    }

    /*
     * Full export plan for a type: logical table name + ordered columns.
     */
    class ExportManifest
    {
        public ExportManifest(string tableName, IReadOnlyList<ExportColumn> columns)
        {
            TableName = tableName;
            Columns = columns;
        }

        public string TableName { get; }
        public IReadOnlyList<ExportColumn> Columns { get; }
    }

    /*
     * Reflection-driven exporter. No Console I/O in this class.
     */
    class ProductExportScanner
    {
        /*
         * Scans T for EntityTable and exportable properties.
         *
         * Property included when [Exportable] present and Include is not false.
         * Header from [DisplayLabel] or property name when label attribute missing.
         */
        public ExportManifest BuildManifest<T>() where T : class
        {
            // TODO: reflect on typeof(T), build column list
            throw new NotImplementedException();
        }

        /*
         * Reads property values in manifest column order.
         *
         * Throws ArgumentNullException when instance is null.
         * Decimals formatted with "F2"; other types use ToString().
         */
        public string[] ExportRow<T>(T instance) where T : class
        {
            // TODO: GetValue per column; format values
            throw new NotImplementedException();
        }

        /*
         * Exports every item; order matches input sequence.
         */
        public IReadOnlyList<string[]> ExportAll<T>(IEnumerable<T> items) where T : class
        {
            // TODO: loop items and call ExportRow
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: create two Product instances
            // TODO: build manifest; print table name and headers
            // TODO: export rows; print comma-separated lines
            throw new NotImplementedException();
        }
    }
}
