# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/02. Filtering & Aggregation`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A nightly audit job is supposed to log every line checked, then report whether any high-value Electronics rows exist. Review the service method. What breaks at runtime or in observability, and how would you fix it?

```csharp
public bool LogAndDetectHighValueElectronics(IEnumerable<OrderLine> lines)
{
    var auditEntries = new List<string>();

    IEnumerable<OrderLine> candidates = lines.Where(line =>
    {
        auditEntries.Add($"Scanned {line.Sku} [{line.Category}]");
        return line.Category == "Electronics"
            && line.Quantity * line.UnitPrice > 500m;
    });

    bool found = candidates.Any();
    _logger.LogInformation("Audit trail ({Count} entries): {Trail}",
        auditEntries.Count, string.Join("; ", auditEntries));
    return found;
}
```

---

#### Q2. (R) A category dashboard API returns revenue and average line total per category. For a category with **no matching order lines**, the endpoint returns HTTP 500. Review the handler. What throws, what misleading value might callers already accept, and how would you fix it?

```csharp
public CategoryMetricsDto GetCategoryMetrics(string category, IEnumerable<OrderLine> lines)
{
    IEnumerable<OrderLine> categoryLines = lines.Where(l => l.Category == category);

    decimal revenue = categoryLines.Sum(l => l.Quantity * l.UnitPrice);
    decimal averageLineTotal = categoryLines.Average(l => l.Quantity * l.UnitPrice);

    return new CategoryMetricsDto(category, revenue, averageLineTotal);
}
```

---

#### Q3. (R) A validation gate runs before applying surcharges on large orders. Review the checks. What is inefficient, what still walks the whole sequence unnecessarily, and what would you change?

```csharp
public void ApplyBusinessRules(IEnumerable<OrderLine> lines)
{
    if (lines.Count(line => line.Quantity <= 0) > 0)
        throw new InvalidOperationException("Quantity must be positive.");

    if (lines.Where(line => line.Category == "Electronics").Count() > 0)
        ApplyElectronicsComplianceFee(lines);

    if (lines.Count() == 0)
        throw new InvalidOperationException("Order has no lines.");
}
```

---

#### Q4. (R) An order-ingestion service caches lines in memory and exposes a filtered view to callers. After a refresh, callers still see stale Electronics rows. Review the cache and query shape. What misconception about deferred execution caused this, and how would you fix it?

```csharp
private List<OrderLine> _cache = new();

public void Refresh(OrderLine[] incoming)
{
    _cache = incoming.ToList();
}

public IEnumerable<OrderLine> GetElectronicsOver(decimal minimumLineTotal)
{
    List<OrderLine> snapshot = _cache.ToList();
    return snapshot
        .Where(line => line.Category == "Electronics")
        .Where(line => line.Quantity * line.UnitPrice > minimumLineTotal);
}

// Caller:
Refresh(updatedLinesFromDb);
var query = GetElectronicsOver(500m);
Thread.Sleep(100);
Refresh(newerLinesFromDb);   // second refresh before enumeration
foreach (var line in query)  // still reflects first snapshot only
    Console.WriteLine(line.Sku);
```

---

#### Q5. (R) A fee-reporting job aggregates nullable surcharge columns from imported rows. Review the aggregation. What do `Sum` and `Average` each do with `null` values, what happens on an all-`null` or empty fee list, and how would you make the report safe for operations?

```csharp
decimal?[] surchargeFees =
[
    12.50m,
    null,
    8.00m,
    null,
    5.25m,
];

decimal totalFees = surchargeFees.Sum();
double averageFee = surchargeFees.Average(f => (double)f!); // developer added cast

decimal?[] emptyImport = Array.Empty<decimal?>();
decimal emptySum = emptyImport.Sum();
double emptyAvg = emptyImport.Average(f => (double)f!);
```

---

#### Q6. (R) A report helper mirrors the tutorial's `PrintCategorySummary` pattern. Under load it becomes slow and occasionally throws when a category has no lines. Review the method. What enumerates the deferred filter more than once, and what empty-sequence trap remains?

```csharp
public CategorySummaryRow SummarizeCategory(string category, IEnumerable<OrderLine> lines)
{
    IEnumerable<OrderLine> categoryLines = lines.Where(l => l.Category == category);

    int lineCount = categoryLines.Count();
    decimal revenue = categoryLines.Sum(l => l.Quantity * l.UnitPrice);
    decimal topLine = categoryLines.Max(l => l.Quantity * l.UnitPrice);

    return new CategorySummaryRow(category, lineCount, revenue, topLine);
}
```
