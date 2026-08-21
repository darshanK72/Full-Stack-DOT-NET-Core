# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/07. Set Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A catalog sync job builds a master SKU list by merging Web and Marketplace feeds. QA reports duplicate SKUs in the export even though both feeds were loaded into `HashSet<CatalogItem>` instances constructed with `CatalogItemBySkuComparer`. Review:

```csharp
IEqualityComparer<CatalogItem> bySku = new CatalogItemBySkuComparer();

HashSet<CatalogItem> webSet = new(webFeed, bySku);
HashSet<CatalogItem> marketSet = new(marketFeed, bySku);

// Developer assumes HashSet's comparer flows into LINQ:
IEnumerable<CatalogItem> master = webSet.Union(marketSet);

var export = master.ToList();
Console.WriteLine($"Unique SKUs: {export.Count}"); // higher than expected — SKU-300 twice
```

What comparer mismatch caused duplicate logical SKUs, and how do you merge with consistent equality end-to-end?

---

#### Q2. (R) An ops dashboard deduplicates a noisy Web import before pricing review. The developer expects one row per SKU. Review:

```csharp
IList<CatalogItem> webFeed = LoadWebImport(); // includes three separate SKU-300 rows

int uniqueCount = webFeed.Distinct().Count();
Console.WriteLine($"Distinct products: {uniqueCount}"); // 6 — expected 4

foreach (CatalogItem item in webFeed.Distinct())
    Console.WriteLine(item.Sku);
// SKU-300 printed three times
```

`CatalogItem` is a plain class (not a record) with no `IEquatable<CatalogItem>`. What equality rule is `Distinct()` using, and how do you collapse duplicate SKUs?

---

#### Q3. (R) A nightly ETL appends marketing tags from two channels into a single analytics table. The pipeline owner insists "we only need one copy of each tag." Review:

```csharp
string[] webTags = { "hardware", "FastShip", "linq", "hardware", "api", "linq" };
string[] marketTags = { "linq", "api", "azure", "fastship", "docker" };

IEnumerable<string> combined = webTags.Concat(marketTags);
int rowCount = combined.Count(); // 11 — stakeholder expected 8 unique tags

await BulkInsertTagsAsync(combined);
```

The job passes unit tests on small samples but loads duplicate tag rows in production. What operator mistake was made, and what change preserves unique membership while keeping first-seen order?

---

#### Q4. (R) After a "fix typo in SKU" feature ships, the Web-only listing report returns fewer rows than inventory expects. Review:

```csharp
public sealed class CatalogItem
{
    public string Sku { get; set; }  // mutable — used by comparer below
    public string Name { get; set; }
    public string Channel { get; set; }
}

public sealed class CatalogItemBySkuComparer : IEqualityComparer<CatalogItem>
{
    public bool Equals(CatalogItem? x, CatalogItem? y) =>
        x is not null && y is not null &&
        string.Equals(x.Sku, y.Sku, StringComparison.Ordinal);

    public int GetHashCode(CatalogItem obj) =>
        StringComparer.Ordinal.GetHashCode(obj.Sku);
}

var bySku = new CatalogItemBySkuComparer();
var webOnly = webFeed.Except(marketFeed, bySku).ToList();

var row = webOnly.First(i => i.Sku == "SKU-200");
row.Sku = "SKU-200-FIXED";  // corrected after Except materialized

bool stillListed = webOnly.Any(i => i.Sku == "SKU-200-FIXED"); // true in list
bool inExceptSet = webFeed.Except(marketFeed, bySku).Any(i => ReferenceEquals(i, row)); // false — re-query misses
```

What went wrong with mutability and deferred set semantics, and how do you fix the type and pipeline?

---

#### Q5. (R) A data-quality check compares two tag pipelines with `SequenceEqual` after a refactor. One pipeline uses `Union` with `StringComparer.OrdinalIgnoreCase`; the other calls `Union` with no comparer. Review:

```csharp
string[] webTags = { "hardware", "FastShip", "linq", "hardware" };
string[] marketTags = { "linq", "api", "azure", "fastship" };

IEnumerable<string> pipelineA = webTags.Union(marketTags, StringComparer.OrdinalIgnoreCase);
IEnumerable<string> pipelineB = webTags.Union(marketTags); // default Ordinal

bool pipelinesMatch = pipelineA.SequenceEqual(pipelineB); // false — counts differ
Console.WriteLine($"A: {pipelineA.Count()}, B: {pipelineB.Count()}");
```

What default equality does parameterless `Union` use for `string`, and when would `"FastShip"` and `"fastship"` split into two entries?

---

#### Q6. (R) A custom SKU comparer passes review but `Distinct` and `Union` intermittently keep duplicate SKUs. Review:

```csharp
public sealed class CatalogItemBySkuComparer : IEqualityComparer<CatalogItem>
{
    public bool Equals(CatalogItem? x, CatalogItem? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return string.Equals(x.Sku, y.Sku, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(CatalogItem obj) =>
        StringComparer.Ordinal.GetHashCode(obj.Sku); // Ordinal hash, case-sensitive
}

var items = webFeed.Distinct(new CatalogItemBySkuComparer()).ToList();
// SKU-100 and sku-100 (if present) both survive
```

What contract violation breaks LINQ set operators, and what is the corrected `GetHashCode`?
