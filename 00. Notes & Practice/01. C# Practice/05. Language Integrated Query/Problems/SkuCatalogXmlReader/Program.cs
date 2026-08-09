/*
 * PROBLEM: SKU Catalog XML Reader
 *
 * Parse supplier catalog XML, query with LINQ over axis methods, project SkuRow
 * records, and mutate active flags in memory.
 *
 * This exercise covers:
 *   ch13 — XDocument, Descendants, attributes, LINQ Where/Select/Average on XML
 *   ch02 — Average empty guard (preview via null return)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RetailCatalogXml
{
    record SkuRow(string Sku, string Name, string Category, decimal ListPrice, bool IsActive);

    /*
     * Embedded sample XML for demos without file I/O.
     */
    static class CatalogXmlSample
    {
        public static string SampleDocument()
        {
            return """
                <catalog>
                  <sku id="A1" category="Hardware" active="true">
                    <name>Widget</name>
                    <price>19.99</price>
                  </sku>
                  <sku id="B2" category="Software" active="false">
                    <name>License</name>
                    <price>49.00</price>
                  </sku>
                  <sku id="C3" category="Hardware" active="true">
                    <name>Gadget</name>
                    <price>9.50</price>
                  </sku>
                </catalog>
                """;
        }
    }

    /*
     * Reads and mutates catalog XML using LINQ to Objects over XElement sequences.
     */
    class SkuCatalogXmlReader
    {
        private readonly XDocument _document;

        public SkuCatalogXmlReader(XDocument document)
        {
            _document = document;
        }

        /*
         * All sku descendants projected to SkuRow.
         */
        public IEnumerable<SkuRow> AllSkus()
        {
            // TODO: Descendants("sku") select id, name, category, price, active
            throw new NotImplementedException();
        }

        /*
         * Active hardware SKUs only.
         */
        public IEnumerable<SkuRow> ActiveHardware()
        {
            // TODO: AllSkus().Where category Hardware and IsActive
            throw new NotImplementedException();
        }

        /*
         * Average list price of active hardware; null when none.
         */
        public decimal? AverageActiveHardwarePrice()
        {
            // TODO: ActiveHardware select price, guard empty, Average
            throw new NotImplementedException();
        }

        /*
         * Set active attribute to false for matching sku id.
         */
        public void DeactivateSku(string skuId)
        {
            // TODO: find Descendants sku where (string)Attribute("id") == skuId, set active false
            throw new NotImplementedException();
        }

        /*
         * Build summary element with count and average from ActiveHardware.
         */
        public XElement? SummaryElement()
        {
            // TODO: new XElement("summary", new XAttribute("count", ...), ...)
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: XDocument.Parse(SampleDocument())
            // TODO: print AllSkus ids
            // TODO: ActiveHardware names and average
            // TODO: DeactivateSku A1, recount
            // TODO: print SummaryElement
            throw new NotImplementedException();
        }
    }
}
