# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/13. LINQ to XML`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A partner-catalog API endpoint loads and parses a 12 MB XML feed on every request. Review the handler. What hurts performance and memory, and how would you fix it?

```csharp
app.MapGet("/catalog/summary", () =>
{
    var doc = XDocument.Load("/data/partner-catalog.xml"); // disk read every call
    var root = doc.Root!;
    var skus = root.Descendants("sku").ToList();
    var activeCount = skus.Count(s => (bool?)s.Attribute("active") == true);
    var totalValue = skus.Sum(s => (decimal)s.Element("price")! * (int)s.Element("qty")!);
    return Results.Ok(new { activeCount, totalValue });
});
```

---

#### Q2. (M) A developer logs "how many SKUs?" twice and gets different numbers from the same `XDocument`. Review the code. Explain lazy vs eager behavior with `Descendants`, and what you would change.

```csharp
XDocument catalog = XDocument.Parse(partnerXml);
XElement root = catalog.Root!;

// Mutate tree between the two reads
root.Add(new XElement("sku",
    new XAttribute("id", "WH-999"),
    new XElement("name", "Late arrival"),
    new XElement("price", 1.00m),
    new XElement("qty", 5)));

IEnumerable<XElement> allSkus = root.Descendants("sku"); // not materialized

Console.WriteLine($"Count pass 1: {allSkus.Count()}");
root.Elements("sku").First(s => (string?)s.Attribute("id") == "WH-100").Remove();
Console.WriteLine($"Count pass 2: {allSkus.Count()}");
```

---

#### Q3. (R) A pricing job throws `NullReferenceException` in production on incomplete partner rows. Review the projection. What is unsafe about attribute access here, and how would you harden it?

```csharp
var rows = catalogRoot.Descendants("sku")
    .Select(s => new SkuRow(
        Sku: s.Attribute("id").Value,                    // line A
        Name: s.Element("name")!.Value,
        Category: (string)s.Attribute("category"),        // line B
        UnitPrice: (decimal)s.Element("price")!,
        QtyOnHand: (int)s.Element("qty")!,
        IsActive: bool.Parse(s.Attribute("active").Value))) // line C
    .ToList();
```

---

#### Q4. (R) After a vendor adds a default `xmlns`, the import reports zero SKUs even though the file looks unchanged in a text editor. Review the query. What broke, and how do you fix lookups and LINQ filters?

```csharp
XNamespace wh = "http://contoso.com/warehouse/2024";
XDocument doc = XDocument.Load("wh-west-catalog.xml");
XElement root = doc.Root!;

int skuCount = root.Elements("sku").Count(); // returns 0

var hardware = root.Descendants("sku")
    .Where(s => s.Attribute("category")?.Value == "hardware")
    .Select(s => s.Element("name")!.Value)
    .ToList();
```

Sample root in the file:

```xml
<catalog xmlns="http://contoso.com/warehouse/2024" warehouse="WH-WEST">
  <sku id="NW-1" category="hardware"><name>Pallet Jack</name><price>899</price></sku>
</catalog>
```

---

#### Q5. (P) An integration service accepts arbitrary XML uploads from external partners and loads them with `XDocument.Load(stream)`. A security review flags XXE. What is the risk, and how should you load untrusted XML safely on .NET?

---

#### Q6. (D) Your team ingests warehouse catalogs: some arrive as files on disk, others as HTTP response bodies already in memory. When do you choose `XDocument.Load` vs `XDocument.Parse`, and what operational constraints (size, retries, temp files) would push you toward streaming with `XmlReader` instead of loading the whole tree?

---
