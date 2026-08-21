# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/01. Introduction to LINQ`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A developer ports the deferred-execution demo from this chapter into a nightly catalog audit job. They expect the side-effect counter to increment when the pipeline is *built*, not when it is consumed. Review:

```csharp
public static void RunAudit(CatalogItem[] catalog)
{
    int projectionRuns = 0;

    IEnumerable<string> deferredLabels =
        catalog
            .Where(i => i.Status == StockStatus.Backordered)
            .Select(i =>
            {
                projectionRuns++;
                return $"[{i.Sku}] {i.Name}";
            });

    Console.WriteLine($"Audit prepared — projectionRuns = {projectionRuns}");

    if (projectionRuns == 0)
    {
        Console.WriteLine("WARNING: No backordered SKUs found — skipping file write.");
        return;
    }

    File.WriteAllLines("backordered.txt", deferredLabels);
}
```

The job always logs the warning and exits, even when backordered items exist. What is wrong, and how do you fix it while keeping deferred execution where it still helps?

---

#### Q2. (R) A pricing microservice exposes catalog metrics to callers. Review the service method and its caller:

```csharp
public IEnumerable<CatalogItem> GetPremiumActiveSkus(IEnumerable<CatalogItem> catalog)
{
    return catalog
        .Where(i => i.Status == StockStatus.Active)
        .Where(i => i.UnitPrice > 50m);
}

// Caller:
var premium = _catalogService.GetPremiumActiveSkus(liveFeed);
_logger.LogInformation("Premium SKU count: {Count}", premium.Count());
var csv = string.Join(",", premium.Select(i => i.Sku));
await _cache.SetAsync("premium-skus", csv);
```

Under load, logs show the filter running twice per request and latency doubles. What categories of issues are present, and what is the prioritized fix?

---

#### Q3. (R) An EF Core repository returns `IQueryable<CatalogItem>`. A controller action filters electronics and returns JSON. Review:

```csharp
public interface ICatalogRepository
{
    IQueryable<CatalogItem> Items { get; }
}

[HttpGet("electronics")]
public IActionResult GetElectronics([FromServices] ICatalogRepository repo)
{
    IEnumerable<CatalogItem> items = repo.Items
        .Where(i => i.Category == "Electronics")
        .Where(i => i.Status != StockStatus.Discontinued);

    return Ok(items);
}
```

The action compiles, but QA reports `(ObjectDisposedException)` from `DbContext` during serialization, and SQL profiling shows *all* catalog rows loaded before the Electronics filter in some builds. What went wrong with `IEnumerable` vs `IQueryable`, and how do you fix the action?

---

#### Q4. (R) A teammate "fixes" slow EF queries by pushing business rules client-side. Review:

```csharp
public List<CatalogItem> GetHighValueActive(AppDbContext db, decimal minPrice)
{
    return db.CatalogItems
        .Where(i => i.Status == StockStatus.Active)
        .AsEnumerable()                              // "run Active filter in SQL, rest in memory"
        .Where(i => ComplexMarginRule(i) > minPrice) // uses nav props + in-memory calc
        .ToList();
}

private static decimal ComplexMarginRule(CatalogItem i) =>
    i.UnitPrice * 1.15m + LookupOverhead(i.Category);
```

SQL trace shows every Active row hydrated into the app; memory spikes on large catalogs. What is the provider leak here, and what refactor preserves SQL filtering where possible?

---

#### Q5. (R) A PR converts method-syntax catalog queries to query syntax for "consistency." Review the refactor:

```csharp
// Before (method syntax — correct):
IEnumerable<string> GetActiveElectronicsLabels(CatalogItem[] catalog) =>
    catalog
        .Where(i => i.Status == StockStatus.Active && i.Category == "Electronics")
        .Select(i => $"{i.Sku}: {i.Name}");

// After (query syntax — merged by reviewer):
IEnumerable<string> GetActiveElectronicsLabels(CatalogItem[] catalog) =>
    from i in catalog
    where i.Status == StockStatus.Active
    select $"{i.Sku}: {i.Name}"
    into label
    where i.Category == "Electronics"
    select label;
```

The build fails. What is wrong with the query-syntax translation, and what is the correct equivalent query (either syntax)?

---

#### Q6. (P) Your API caches "active catalog snapshots" for five minutes. Two implementations are proposed:

```csharp
// A
IEnumerable<CatalogItem> snapshot = catalog.Where(i => i.Status == StockStatus.Active);

// B
List<CatalogItem> snapshot = catalog.Where(i => i.Status == StockStatus.Active).ToList();
```

The underlying `catalog` array is mutated when warehouse workers update SKU status between requests. Callers enumerate the cached value multiple times per HTTP request (validation, mapping, CSV export). Which approach do you ship, when do you materialize, and why?
