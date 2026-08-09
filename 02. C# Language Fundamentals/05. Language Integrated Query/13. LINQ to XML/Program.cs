/*
 * =============================================================================
 * 13. LINQ TO XML — COMPLETE TUTORIAL
 * =============================================================================
 *
 * TOPIC: System.Xml.Linq — create, load/parse, navigate, query with LINQ,
 *        modify, and save XML using XDocument, XElement, and XAttribute.
 *        Axis methods (Elements, Descendants, Attributes) expose XML as
 *        IEnumerable sequences so Where / Select / OrderBy apply directly.
 *
 * WHY IT MATTERS:
 *   Config files, partner feeds, SOAP-style payloads, and export/import
 *   formats still arrive as XML. LINQ to XML is the modern in-memory API —
 *   lighter and more idiomatic than the older XmlDocument DOM — and reuses
 *   the same LINQ operators from chapters 01–12.
 *
 * WHAT YOU WILL LEARN:
 *   1.  Core types — XDocument, XElement, XAttribute, XName, XNamespace
 *   2.  Create, Parse, and Load — build trees, parse strings, load from disk
 *   3.  Functional construction — nested constructors and LINQ-produced children
 *   4.  Axis methods — Element, Elements, Descendants, Attribute, Attributes
 *   5.  Reading values — Value, typed casts, null-safe navigation
 *   6.  LINQ queries over XML — method and query syntax, aggregation
 *   7.  XML namespaces — XNamespace + qualified XName
 *   8.  Modify and save — Add, Remove, SetValue, SetAttributeValue, ReplaceWith, Save
 *
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LinqToXml;

/*
 * =============================================================================
 * SECTION 1: DOMAIN ROW — PLAIN C# SHAPE MIRRORED IN XML
 * =============================================================================
 *
 * SkuRow is ordinary C# data. Later sections project SkuRow → XElement and
 * query XElement → anonymous/SkuRow shapes. Keeping a small domain type makes
 * the "objects ↔ XML" round-trip obvious.
 * -------------------------------------------------------------------------
 */
public readonly record struct SkuRow(
    string Sku,
    string Name,
    string Category,
    decimal UnitPrice,
    int QtyOnHand,
    bool IsActive);

public class Program
{
    /*
     * Embedded warehouse catalog — Parse demos need no external file.
     * Production code often uses XDocument.Load(path) or Load(Stream).
     */
    private const string CatalogXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <catalog warehouse="WH-EAST" currency="USD">
          <sku id="WH-100" category="hardware" active="true">
            <name>Hex Bolt M8</name>
            <price>0.45</price>
            <qty>1200</qty>
          </sku>
          <sku id="WH-200" category="hardware" active="true">
            <name>Steel Bracket</name>
            <price>4.25</price>
            <qty>340</qty>
          </sku>
          <sku id="WH-300" category="electrical" active="false">
            <name>LED Driver 24V</name>
            <price>18.90</price>
            <qty>0</qty>
          </sku>
          <sku id="WH-400" category="electrical" active="true">
            <name>Cable Tie Pack</name>
            <price>2.10</price>
            <qty>880</qty>
          </sku>
        </catalog>
        """;

    public static void Main(string[] args)
    {
        /*
         * =========================================================================
         * SECTION 2: CORE TYPES — XDocument, XElement, XAttribute
         * =========================================================================
         *
         * Namespace: System.Xml.Linq (BCL on net8.0 — no extra NuGet package).
         *
         *  Type          | Role
         *  --------------|--------------------------------------------------------
         *  XDocument     | Whole document; optional <?xml …?>; exactly one root
         *  XElement      | Element node (tag); also valid as a standalone fragment
         *  XAttribute    | Attribute on an element (id="WH-100")
         *  XName         | Qualified name (LocalName + optional namespace URI)
         *  XNamespace    | URI wrapper — ns + "local" builds an XName
         *
         * Inheritance (simplified):
         *   XNode → XContainer → XDocument
         *   XNode → XContainer → XElement
         *   XObject → XAttribute
         *
         * Both XDocument and XElement inherit XContainer, so both expose
         * Element / Elements / Descendants and work with LINQ.
         *
         * Key difference:
         *   XDocument  →  declaration + one root (use .Root)
         *   XElement   →  one node; fine alone when you only need a fragment
         *
         * --- 2a. Parse a string into XDocument ---
         * -------------------------------------------------------------------------
         */

        XDocument catalogDoc = XDocument.Parse(CatalogXml); // string → in-memory document
        XElement catalogRoot = catalogDoc.Root
            ?? throw new InvalidOperationException("Catalog XML has no root element.");

        Console.WriteLine("=== SECTION 2: Core types after Parse ===");
        Console.WriteLine($"Document root : <{catalogRoot.Name.LocalName}>"); // LocalName ignores xmlns
        Console.WriteLine($"Warehouse     : {catalogRoot.Attribute("warehouse")?.Value}"); // XAttribute?
        Console.WriteLine($"Direct <sku>  : {catalogRoot.Elements("sku").Count()}"); // children only


        /*
         * =========================================================================
         * SECTION 3: CREATE, PARSE, AND LOAD
         * =========================================================================
         *
         * Three common entry points:
         *
         *  API                         | Source
         *  ----------------------------|------------------------------------------
         *  new XElement / XDocument    | Build a tree in code (functional style)
         *  XDocument.Parse(string)     | XML already in memory as text
         *  XDocument.Load(path|stream) | File path, Stream, or TextReader
         *
         * XElement.Parse / XElement.Load exist too — they return a root element
         * without wrapping an XDocument (handy for fragments).
         *
         * Save pairs with Load:
         *   doc.Save(path)             Write UTF-8 XML to disk
         *   doc.ToString()             Serialize to string (pretty by default)
         *
         * --- 3a. Create a tiny XDocument from scratch ---
         * --- 3b. XElement.Parse for a fragment ---
         * --- 3c. Round-trip Save / Load via a temp file ---
         * -------------------------------------------------------------------------
         */

        // Functional construction of a whole document (declaration + root)
        XDocument tinyDoc = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("note",
                new XAttribute("priority", "high"),
                new XElement("to", "Ada"),
                new XElement("body", "Ship the WH-EAST catalog export.")));

        Console.WriteLine();
        Console.WriteLine("=== SECTION 3a: Created XDocument (ToString) ===");
        Console.WriteLine(tinyDoc.ToString()); // pretty-printed by default

        // Fragment parse — no XDocument wrapper; root IS the returned element
        XElement fragment = XElement.Parse(
            """<sku id="WH-500" category="packing" active="true"><name>Bubble Wrap Roll</name><price>9.75</price><qty>60</qty></sku>""");

        Console.WriteLine();
        Console.WriteLine("=== SECTION 3b: XElement.Parse fragment ===");
        Console.WriteLine($"Fragment root : <{fragment.Name.LocalName}> id={fragment.Attribute("id")?.Value}");

        string tempPath = Path.Combine(Path.GetTempPath(), "linq-to-xml-catalog.xml");
        catalogDoc.Save(tempPath); // write full document to disk
        XDocument loadedFromDisk = XDocument.Load(tempPath); // read it back
        int loadedSkuCount = loadedFromDisk.Root?.Elements("sku").Count() ?? 0;

        Console.WriteLine();
        Console.WriteLine("=== SECTION 3c: Save then Load ===");
        Console.WriteLine($"Saved path    : {tempPath}");
        Console.WriteLine($"Loaded <sku>  : {loadedSkuCount}");

        try { File.Delete(tempPath); } // clean up demo file
        catch (IOException) { /* ignore cleanup failures in demos */ }


        /*
         * =========================================================================
         * SECTION 4: FUNCTIONAL CONSTRUCTION
         * =========================================================================
         *
         * Prefer nested constructors over CreateElement / AppendChild loops:
         *
         *   new XElement("sku",
         *       new XAttribute("id", "WH-500"),
         *       new XElement("name", "Bubble Wrap Roll"),
         *       new XElement("price", 9.75m));
         *
         * Content rules for constructor args after the name:
         *
         *  Argument type              | Becomes
         *  ---------------------------|------------------------------------------
         *  XAttribute                 | Attribute on this element
         *  XElement / XNode           | Nested child node
         *  string / number / bool     | Text content (XText)
         *  IEnumerable<XElement>      | Many children (great with LINQ Select)
         *  null                       | Ignored (skipped)
         *
         * An XElement alone (no XDocument) is a valid fragment — insert later
         * with parent.Add(fragment).
         *
         * --- 4a. Build a standalone sku fragment ---
         * --- 4b. Project a C# sequence into child elements ---
         * -------------------------------------------------------------------------
         */

        XElement newSkuFragment = new XElement("sku",
            new XAttribute("id", "WH-500"),
            new XAttribute("category", "packing"),
            new XAttribute("active", "true"),
            new XElement("name", "Bubble Wrap Roll"),
            new XElement("price", 9.75m), // numeric content → text node "9.75"
            new XElement("qty", 60));

        Console.WriteLine();
        Console.WriteLine("=== SECTION 4a: Functionally constructed fragment ===");
        Console.WriteLine(newSkuFragment.ToString(SaveOptions.None));

        // LINQ Select → IEnumerable<XElement> accepted as constructor content
        SkuRow[] extras =
        {
            new SkuRow("WH-610", "Foam Corner", "packing", 1.15m, 400, true),
            new SkuRow("WH-620", "Shrink Film", "packing", 12.00m, 25, true),
        };

        XElement extrasBatch = new XElement("batch",
            new XAttribute("source", "receiving"),
            extras.Select(r => new XElement("sku",
                new XAttribute("id", r.Sku),
                new XAttribute("category", r.Category),
                new XAttribute("active", r.IsActive),
                new XElement("name", r.Name),
                new XElement("price", r.UnitPrice),
                new XElement("qty", r.QtyOnHand))));

        Console.WriteLine();
        Console.WriteLine("=== SECTION 4b: LINQ → child elements ===");
        Console.WriteLine($"Batch children : {extrasBatch.Elements("sku").Count()}");
        Console.WriteLine(extrasBatch.ToString(SaveOptions.None));


        /*
         * =========================================================================
         * SECTION 5: AXIS METHODS — Elements, Descendants, Attributes
         * =========================================================================
         *
         * Called on any XContainer (document or element):
         *
         *  Method                 | Returns
         *  -----------------------|-----------------------------------------------
         *  .Element("name")       | First *direct* child with that name (or null)
         *  .Elements("name")      | All *direct* children with that name
         *  .Elements()            | All direct child elements (any name)
         *  .Descendants("name")   | Matching nodes at *any* depth under this node
         *  .Descendants()         | All descendant elements (any name)
         *  .Attribute("name")     | Named attribute on *this* element (or null)
         *  .Attributes()          | All attributes on this element
         *  .Parent                | Parent XElement (or null at document root)
         *  .Ancestors("name")     | Matching ancestors walking upward
         *
         * Pitfall: foreach over an XElement iterates *direct children only*.
         * Use Descendants("sku") when nodes might nest deeper.
         *
         * --- 5a. Direct child navigation + attributes ---
         * --- 5b. Elements vs Descendants ---
         * --- 5c. Parent / Ancestors ---
         * -------------------------------------------------------------------------
         */

        XElement firstSku = catalogRoot.Element("sku")
            ?? throw new InvalidOperationException("Expected at least one sku.");

        string firstName = firstSku.Element("name")?.Value ?? "(no name)";
        decimal firstPrice = (decimal)(firstSku.Element("price")
            ?? throw new InvalidOperationException("Sku missing price."));
        int firstQty = (int)(firstSku.Element("qty")
            ?? throw new InvalidOperationException("Sku missing qty."));

        // Attributes() yields every XAttribute on this element
        IEnumerable<string> firstSkuAttrPairs = firstSku.Attributes()
            .Select(a => $"{a.Name.LocalName}={a.Value}");

        Console.WriteLine();
        Console.WriteLine("=== SECTION 5a: First sku (Element / Attribute) ===");
        Console.WriteLine($"  id     : {firstSku.Attribute("id")?.Value}");
        Console.WriteLine($"  name   : {firstName}");
        Console.WriteLine($"  price  : {firstPrice:C}");
        Console.WriteLine($"  qty    : {firstQty}");
        Console.WriteLine($"  attrs  : {string.Join(", ", firstSkuAttrPairs)}");

        // Elements("sku") = direct children; Descendants("name") walks any depth
        int directSkus = catalogRoot.Elements("sku").Count();
        int allNames = catalogRoot.Descendants("name").Count(); // names nest under sku
        int priceNodes = catalogRoot.Descendants("price").Count();

        Console.WriteLine();
        Console.WriteLine("=== SECTION 5b: Elements vs Descendants ===");
        Console.WriteLine($"  Direct <sku> children     : {directSkus}");
        Console.WriteLine($"  Descendant <name> nodes   : {allNames}");
        Console.WriteLine($"  Descendant <price> nodes  : {priceNodes}");

        XElement nameNode = firstSku.Element("name")
            ?? throw new InvalidOperationException("name missing.");
        string parentTag = nameNode.Parent?.Name.LocalName ?? "(none)"; // walk up one level
        string? catalogAncestorId = nameNode.Ancestors("catalog")
            .Attributes("warehouse")
            .Select(a => a.Value)
            .FirstOrDefault(); // Ancestors walks upward; Attributes filters those nodes

        Console.WriteLine();
        Console.WriteLine("=== SECTION 5c: Parent / Ancestors ===");
        Console.WriteLine($"  <name> parent tag         : {parentTag}");
        Console.WriteLine($"  warehouse via Ancestors   : {catalogAncestorId}");


        /*
         * =========================================================================
         * SECTION 6: READING VALUES — Value AND TYPED CASTS
         * =========================================================================
         *
         *  Expression                     | Meaning
         *  -------------------------------|----------------------------------------
         *  element.Value                  | Concatenated text of element + descendants
         *  (decimal)element               | Convert Value (throws if missing/bad)
         *  (int)element                   | Same for integers
         *  (string?)attribute             | Attribute text, or null if missing
         *  (bool?)attribute               | XML bool conversion, or null
         *  element.Element("x")?.Value    | Safe drill-down
         *
         * Prefer casts when the schema guarantees the node; use ?. / TryParse
         * patterns when data may be incomplete.
         * -------------------------------------------------------------------------
         */

        XElement priceEl = firstSku.Element("price")!;
        string rawPriceText = priceEl.Value; // always string text
        decimal typedPrice = (decimal)priceEl; // explicit conversion operators on XElement
        bool? isActive = (bool?)firstSku.Attribute("active"); // null if attribute absent
        string? missingAttr = (string?)firstSku.Attribute("doesNotExist");

        Console.WriteLine();
        Console.WriteLine("=== SECTION 6: Value and typed casts ===");
        Console.WriteLine($"  Value (string) : {rawPriceText}");
        Console.WriteLine($"  (decimal) cast : {typedPrice:C}");
        Console.WriteLine($"  (bool?) active : {isActive}");
        Console.WriteLine($"  missing attr   : {(missingAttr is null ? "(null)" : missingAttr)}");


        /*
         * =========================================================================
         * SECTION 7: QUERYING XML WITH LINQ
         * =========================================================================
         *
         * Start from Elements(...) or Descendants(...), then compose the same
         * operators as LINQ to Objects (Where, Select, OrderBy, Average, …).
         *
         *   // Method syntax
         *   catalog.Descendants("sku")
         *          .Where(s => (string?)s.Attribute("category") == "hardware")
         *          .Select(s => s.Element("name")!.Value);
         *
         *   // Query syntax
         *   from sku in catalog.Descendants("sku")
         *   where (string?)sku.Attribute("category") == "hardware"
         *   select sku.Element("name")!.Value;
         *
         * Casting tips:
         *   (string?)attr     → attribute text or null
         *   (decimal)element  → parse element's Value (throws if missing/bad)
         *   Prefer ?. / FirstOrDefault when nodes are optional
         *
         * --- 7a. Method syntax — active hardware with qty > 100 ---
         * --- 7b. Query syntax — electrical by price descending ---
         * --- 7c. Aggregation + project back to SkuRow ---
         * -------------------------------------------------------------------------
         */

        IEnumerable<XElement> allSkus = catalogRoot.Descendants("sku");

        var activeHardware = allSkus
            .Where(s => (string?)s.Attribute("category") == "hardware")
            .Where(s => (bool?)s.Attribute("active") == true)
            .Where(s => (int)s.Element("qty")! > 100)
            .Select(s => new
            {
                Id = (string?)s.Attribute("id"),
                Name = s.Element("name")!.Value,
                Qty = (int)s.Element("qty")!,
                Price = (decimal)s.Element("price")!,
            })
            .OrderBy(x => x.Name);

        var electricalByPrice = (
            from sku in catalogRoot.Descendants("sku")
            where (string?)sku.Attribute("category") == "electrical"
            let price = (decimal)sku.Element("price")!
            orderby price descending
            select new { Name = sku.Element("name")!.Value, Price = price }).ToList();

        Console.WriteLine();
        Console.WriteLine("=== SECTION 7a: Active hardware (qty > 100) ===");
        foreach (var item in activeHardware)
        {
            Console.WriteLine($"  [{item.Id}] {item.Name} — qty {item.Qty} @ {item.Price:C}");
        }

        Console.WriteLine();
        Console.WriteLine("=== SECTION 7b: Electrical by price (desc) — query syntax ===");
        foreach (var item in electricalByPrice)
        {
            Console.WriteLine($"  {item.Name} — {item.Price:C}");
        }

        decimal avgActivePrice = allSkus
            .Where(s => (bool?)s.Attribute("active") == true)
            .Average(s => (decimal)s.Element("price")!); // same Average as LINQ to Objects

        // Project XML rows into the domain type from section 1
        List<SkuRow> asRows = allSkus
            .Select(s => new SkuRow(
                Sku: (string?)s.Attribute("id") ?? "?",
                Name: s.Element("name")?.Value ?? "(unnamed)",
                Category: (string?)s.Attribute("category") ?? "?",
                UnitPrice: (decimal)s.Element("price")!,
                QtyOnHand: (int)s.Element("qty")!,
                IsActive: (bool?)s.Attribute("active") ?? false))
            .OrderBy(r => r.Sku)
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"=== SECTION 7c: Avg active price: {avgActivePrice:C} | rows: {asRows.Count} ===");
        foreach (SkuRow row in asRows)
        {
            Console.WriteLine($"  {row.Sku} | {row.Name,-18} | {row.Category,-10} | {row.UnitPrice,7:C} | qty={row.QtyOnHand}");
        }


        /*
         * =========================================================================
         * SECTION 8: XML NAMESPACES — XNamespace AND QUALIFIED NAMES
         * =========================================================================
         *
         * Real feeds often declare a default xmlns. Unqualified "sku" then
         * does NOT match namespaced children — Element("sku") returns null.
         *
         *   XNamespace ns = "http://contoso.com/warehouse/2024";
         *   new XElement(ns + "catalog", …)     // qualified XName
         *   root.Elements(ns + "sku")           // query with same namespace
         *
         * LocalName alone ignores the URI — useful for display, dangerous for
         * lookups when multiple namespaces coexist.
         * -------------------------------------------------------------------------
         */

        XNamespace whNs = "http://contoso.com/warehouse/2024";
        XDocument namespacedDoc = new XDocument(
            new XElement(whNs + "catalog",
                new XAttribute("warehouse", "WH-WEST"),
                new XElement(whNs + "sku",
                    new XAttribute("id", "NW-1"),
                    new XElement(whNs + "name", "Pallet Jack"),
                    new XElement(whNs + "price", 899m))));

        XElement? nsRoot = namespacedDoc.Root;
        int unqualifiedMiss = nsRoot?.Elements("sku").Count() ?? 0; // wrong — no namespace
        int qualifiedHit = nsRoot?.Elements(whNs + "sku").Count() ?? 0; // correct
        string? nsLocal = nsRoot?.Name.LocalName;
        string? nsUri = nsRoot?.Name.NamespaceName;

        Console.WriteLine();
        Console.WriteLine("=== SECTION 8: Namespaces ===");
        Console.WriteLine($"  LocalName / URI     : {nsLocal} | {nsUri}");
        Console.WriteLine($"  Elements(\"sku\")     : {unqualifiedMiss} (miss — no xmlns)");
        Console.WriteLine($"  Elements(ns + sku)  : {qualifiedHit}");
        Console.WriteLine(namespacedDoc.ToString(SaveOptions.None));


        /*
         * =========================================================================
         * SECTION 9: MODIFYING AND SAVING
         * =========================================================================
         *
         * Trees are mutable — changes apply immediately to the in-memory graph.
         *
         *  Operation                      | API
         *  -------------------------------|----------------------------------------
         *  Add child / attribute            | parent.Add(content)
         *  Remove node                    | node.Remove()
         *  Replace element text           | element.SetValue(newValue)
         *  Add or replace attribute       | element.SetAttributeValue(name, value)
         *  Replace entire node            | existing.ReplaceWith(newNode)
         *  Persist to disk                | doc.Save(path) or stream overload
         *
         * SetAttributeValue(name, null) removes the attribute.
         * There is no separate "commit" — Save / ToString just serialize the tree.
         *
         * --- 9a. Add, update stock/price/active ---
         * --- 9b. ReplaceWith, Remove, summary fragment, serialize ---
         * -------------------------------------------------------------------------
         */

        catalogRoot.Add(newSkuFragment); // attach the section-4a fragment

        XElement? ledDriver = catalogRoot
            .Elements("sku")
            .FirstOrDefault(s => s.Element("name")?.Value == "LED Driver 24V");

        if (ledDriver is not null)
        {
            ledDriver.SetAttributeValue("active", "true"); // add or replace attribute
            XElement priceElement = ledDriver.Element("price")
                ?? throw new InvalidOperationException("Price element missing.");
            priceElement.SetValue(17.50m); // replace text content of <price>
            ledDriver.Element("qty")?.SetValue(40); // restock
        }

        XElement? cableTies = catalogRoot
            .Elements("sku")
            .FirstOrDefault(s => (string?)s.Attribute("id") == "WH-400");

        cableTies?.SetAttributeValue("category", "packing"); // mutate attribute in place

        // ReplaceWith swaps a whole node (here: refresh Steel Bracket pricing node)
        XElement? bracket = catalogRoot
            .Elements("sku")
            .FirstOrDefault(s => (string?)s.Attribute("id") == "WH-200");
        bracket?.Element("price")?.ReplaceWith(new XElement("price", 4.50m));

        // Remove a discontinued low-stock line as a purge demo
        XElement? toRemove = catalogRoot
            .Elements("sku")
            .FirstOrDefault(s => (string?)s.Attribute("id") == "WH-300");
        toRemove?.Remove(); // detaches from parent; object still usable in memory

        Console.WriteLine();
        Console.WriteLine("=== SECTION 9a: After Add / Set* / ReplaceWith / Remove ===");
        PrintSkuSummary(catalogRoot);

        int activeCount = catalogRoot.Descendants("sku")
            .Count(s => (bool?)s.Attribute("active") == true);

        decimal inventoryValue = catalogRoot.Descendants("sku")
            .Sum(s => (decimal)s.Element("price")! * (int)s.Element("qty")!);

        // Functional construction + LINQ counts into a summary child
        XElement summary = new XElement("summary",
            new XAttribute("generated", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement("skuCount", catalogRoot.Elements("sku").Count()),
            new XElement("activeCount", activeCount),
            new XElement("inventoryValue", inventoryValue));

        catalogRoot.Add(summary);

        Console.WriteLine();
        Console.WriteLine("=== SECTION 9b: Summary element + compact document ===");
        Console.WriteLine(summary.ToString(SaveOptions.None));

        string serialized = catalogDoc.ToString(SaveOptions.DisableFormatting); // one long line
        Console.WriteLine(serialized.Length > 320
            ? serialized[..320] + "…"
            : serialized);

        // Final Save demo — write the mutated document, then delete
        string outPath = Path.Combine(Path.GetTempPath(), "linq-to-xml-final.xml");
        catalogDoc.Save(outPath);
        Console.WriteLine($"Saved final document → {outPath} ({new FileInfo(outPath).Length} bytes)");
        try { File.Delete(outPath); }
        catch (IOException) { /* ignore */ }
    }

    /*
     * Helper — one-line-per-sku table from the live tree (used by section 9).
     */
    private static void PrintSkuSummary(XElement catalogRoot)
    {
        foreach (XElement sku in catalogRoot.Elements("sku").OrderBy(s => (string?)s.Attribute("id")))
        {
            string id = (string?)sku.Attribute("id") ?? "?";
            string name = sku.Element("name")?.Value ?? "(unnamed)";
            string category = (string?)sku.Attribute("category") ?? "?";
            string active = (string?)sku.Attribute("active") ?? "?";
            decimal price = sku.Element("price") is XElement p ? (decimal)p : 0m;
            int qty = sku.Element("qty") is XElement q ? (int)q : 0;
            Console.WriteLine($"  {id} | {name,-18} | {category,-10} | active={active,-5} | {price,7:C} × {qty}");
        }
    }
}

/*
 * =========================================================================
 * QUICK REFERENCE — LINQ TO XML (System.Xml.Linq)
 * =========================================================================
 *
 * --- Core types ---
 *
 *   XDocument          Whole document; Parse / Load; .Root for top element
 *   XElement           Element node; also root of a fragment
 *   XAttribute         Attribute (id="1"); pass into XElement constructor
 *   XName              .LocalName, .Namespace / .NamespaceName
 *   XNamespace         XNamespace ns = "uri"; ns + "local" → XName
 *
 * --- Create / load / save ---
 *
 *   new XDocument(decl, root)        Build document in code
 *   new XElement(name, content…)     Functional construction
 *   XDocument.Parse(string)          In-memory from string
 *   XDocument.Load(path|stream)      From file or stream
 *   XElement.Parse / Load            Fragment without XDocument wrapper
 *   doc.Save(path)                   Write to disk
 *   doc.ToString(SaveOptions)        Serialize; DisableFormatting = one line
 *
 * --- Navigation (axis methods) ---
 *
 *   root.Element("child")            First direct child (or null)
 *   root.Elements("child")           All direct children named child
 *   root.Descendants("child")        All matching descendants (any depth)
 *   root.Attribute("id")             Single attribute on this element
 *   root.Attributes()                All attributes on this element
 *   el.Parent / el.Ancestors("x")    Walk upward
 *   el.Descendants("p").Attributes("currency")  Attributes on matching nodes
 *
 * --- Values ---
 *
 *   element.Value                    Combined text content
 *   (decimal)element / (int)element  Typed conversion of Value
 *   (string?)attribute / (bool?)attr Attribute text / bool or null
 *
 * --- Functional construction ---
 *
 *   new XElement("parent",
 *       new XAttribute("a", "1"),
 *       new XElement("child", "text"),
 *       items.Select(x => new XElement("row", x)));  // IEnumerable content
 *
 * --- LINQ queries ---
 *
 *   from x in doc.Descendants("item") where … select …
 *   doc.Descendants("item").Where(…).Select(…)
 *   Same operators as LINQ to Objects (OrderBy, Average, Sum, …)
 *
 * --- Namespaces ---
 *
 *   XNamespace ns = "http://example.com";
 *   new XElement(ns + "root", new XElement(ns + "child", "v"));
 *   root.Elements(ns + "child")      // unqualified "child" misses namespaced nodes
 *
 * --- Modify ---
 *
 *   parent.Add(childOrAttribute)     Append child / attribute
 *   node.Remove()                    Detach from parent
 *   el.SetValue("new")               Replace text content
 *   el.SetAttributeValue("k", v)     Add or replace (null removes)
 *   old.ReplaceWith(newNode)         Swap node
 *
 * --- Common mistakes ---
 *
 *  Mistake                           | Result / fix
 *  ----------------------------------|----------------------------------------
 *  foreach on XElement for deep tree | Yields direct children — use Descendants
 *  Missing null check on Element()   | NullReferenceException — use ?. / !
 *  Mixing XmlDocument with XElement  | Different APIs — prefer LINQ to XML
 *  Forgetting xmlns on namespaced XML| Need XNamespace + qualified XName
 *
 * --- Related chapters ---
 *
 *   LINQ ch.01–12              Query/method syntax used on XML sequences
 *   07. File Input & Output    Deeper file/stream I/O around Load / Save
 *
 * =========================================================================
 */
