---
module: 05. Language Integrated Query
difficulty: Hard
chapters: 13 LINQ to XML
domain: RetailCatalog
---

# SKU Catalog XML Reader

Build a **.NET 8 console application from scratch** that parses a product catalog XML document, queries it with LINQ to Objects over axis methods, and projects domain rows.

## Business context

Suppliers deliver catalog updates as XML. Merchandising loads the file in-memory, filters active hardware SKUs, computes average list price, and updates stock flags without a database.

## Definitions

**Record `SkuRow`**

- `Sku` (string)
- `Name` (string)
- `Category` (string)
- `ListPrice` (decimal)
- `IsActive` (bool)

**Static class `CatalogXmlSample`**

- `string SampleDocument()` — returns XML string:

```xml
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
```

**Class `SkuCatalogXmlReader`**

- `SkuCatalogXmlReader(XDocument document)` — store document
- `IEnumerable<SkuRow> AllSkus()` — **`Descendants("sku")`**, project attributes `id`, `category`, `active`, child `name`/`price` elements
- `IEnumerable<SkuRow> ActiveHardware()` — LINQ **`Where`** Category Hardware and active true
- `decimal? AverageActiveHardwarePrice()` — **`Average`** on ListPrice; null when none
- `void DeactivateSku(string skuId)` — find **`Descendants("sku")`** where `@id` matches; set **`active`** attribute to `"false"` (mutates document)
- `XElement? SummaryElement()` — create and return new `<summary count="N" avgPrice="…"/>` with aggregate stats from ActiveHardware (add to document root optional)

## Demo Main

1. Parse sample with `XDocument.Parse`.
2. Print all SKU ids from `AllSkus()`.
3. Print active hardware names and average price.
4. Deactivate `A1`, re-query — count drops by one.
5. Print SummaryElement outer XML or count/avg attributes.

## Constraints

- net8, `System.Xml.Linq`, LINQ on `IEnumerable<XElement>`
- Use `(decimal)element` or `decimal.Parse` for price; `(bool)attribute` or string compare for active
- Null-safe when child elements missing (`?.`)

## Non-goals

Namespaces (unqualified names only in sample), file I/O

## Evaluation

[EVALUATION.md](EVALUATION.md)
