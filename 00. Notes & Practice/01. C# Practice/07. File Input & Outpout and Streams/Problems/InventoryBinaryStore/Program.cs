/*
 * PROBLEM: Inventory Binary Store
 *
 * POS devices load a compact binary product catalog with a magic signature guard.
 *
 * This exercise covers:
 *   ch03 — FileStream FileMode.Create/Open, BinaryWriter/BinaryReader
 *   ch03 — magic bytes, typed field order, InvalidDataException
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RetailCatalog
{
    /*
     * One product row in the binary catalog.
     */
    readonly struct ProductRecord
    {
        public int Id { get; }
        public string Name { get; }
        public decimal UnitPrice { get; }
        public bool InStock { get; }

        public ProductRecord(int id, string name, decimal unitPrice, bool inStock)
        {
            Id = id;
            Name = name;
            UnitPrice = unitPrice;
            InStock = inStock;
        }
    }

    /*
     * Writes and reads catalog.bin layout: CAT1 magic, count, typed records.
     *
     * No Console calls in this class.
     */
    static class InventoryBinaryStore
    {
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("CAT1");

        /*
         * Creates/truncates path and writes magic, count, and all records.
         */
        public static void Save(string path, IReadOnlyList<ProductRecord> products)
        {
            // TODO: FileStream Create, BinaryWriter UTF-8, write magic/count/fields, Flush
            throw new NotImplementedException();
        }

        /*
         * Reads catalog from path.
         *
         * Throws InvalidDataException("Invalid catalog signature.") when magic mismatches.
         */
        public static ProductRecord[] Load(string path)
        {
            // TODO: verify magic with ReadBytes, read records in write order
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Save three products to catalog.bin
            // TODO: Load and print each record
            // TODO: corrupt magic copy → catch InvalidDataException
            // TODO: cleanup demo files
            throw new NotImplementedException();
        }
    }
}
