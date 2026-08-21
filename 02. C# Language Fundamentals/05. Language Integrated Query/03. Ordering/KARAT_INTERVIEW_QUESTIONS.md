# Karat — Interview Questions

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/03. Ordering`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

#### Q1. (R) A warehouse pick-list API should sort by **Priority descending**, then **PlacedAt ascending** within the same priority. QA reports rush (`Priority == 3`) lines appear in random date order. Review the query. What is wrong, and what would you change?

```csharp
public IEnumerable<FulfillmentLineDto> BuildPickList(IEnumerable<FulfillmentLine> openLines)
{
    return openLines
        .Where(line => line.Priority >= 2)
        .OrderByDescending(line => line.Priority)
        .OrderBy(line => line.PlacedAt)
        .Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt, line.Zone));
}
```

---

#### Q2. (M) A developer unit-tests in-memory LINQ and ships this EF Core query. They assert priority-1 rows keep the same relative order as the import file when only `OrderBy(Priority)` is used — no `ThenBy`. What assumption fails in production, and how would you make ordering deterministic for the database?

```csharp
// In-memory test passes — LINQ to Objects stable sort preserves tie order
var expected = new[] { "ORD-1045", "ORD-1047", "ORD-1048" };

var actual = openLines
    .OrderBy(line => line.Priority)
    .Where(line => line.Priority == 1)
    .Select(line => line.OrderId)
    .ToArray();

Assert.That(actual, Is.EqualTo(expected));
```

```csharp
// Production repository — same shape, translated to SQL
public async Task<IReadOnlyList<string>> GetPriorityOneOrderIdsAsync(CancellationToken ct)
{
    return await _db.FulfillmentLines
        .OrderBy(line => line.Priority)
        .Where(line => line.Priority == 1)
        .Select(line => line.OrderId)
        .ToListAsync(ct);
}
```

---

#### Q3. (R) A nightly export job logs pick-list metrics, then writes every sorted line. Under load the job slows and occasionally logs a different "first order" between steps. Review the method. What does deferred `OrderBy` do here, and how would you fix it?

```csharp
public void ExportPickList(IEnumerable<FulfillmentLine> openLines, StreamWriter writer)
{
    IEnumerable<FulfillmentLine> sorted = openLines
        .OrderByDescending(line => line.Priority)
        .ThenBy(line => line.PlacedAt);

    _logger.LogInformation("Export row count: {Count}", sorted.Count());

    foreach (FulfillmentLine line in sorted)
        writer.WriteLine($"{line.OrderId},{line.Priority},{line.PlacedAt:O}");

    FulfillmentLine first = sorted.First();
    _metrics.RecordFirstOrder(first.OrderId);
}
```

---

#### Q4. (R) A customer directory endpoint returns lines sorted alphabetically by `Customer`. Sort order matches on a developer laptop but differs on the Linux API host; support tickets mention `"Acme Corp"` and `"acme corp"` appearing far apart. Review the handler. What comparison rules apply by default, and what would you change for a stable API contract?

```csharp
public IReadOnlyList<CustomerDirectoryRow> GetCustomersAlphabetical(
    IEnumerable<FulfillmentLine> openLines)
{
    return openLines
        .OrderBy(line => line.Customer)
        .Select(line => new CustomerDirectoryRow(line.OrderId, line.Customer))
        .ToList();
}
```

---

#### Q5. (D) A paginated fulfillment grid calls this repository method. Users report rows "jumping" between pages when they refresh — especially among lines that share the same priority. What ordering guarantee is missing, and how would you fix pagination?

```csharp
public async Task<PagedResult<FulfillmentLineDto>> GetPageAsync(
    int page,
    int pageSize,
    CancellationToken ct)
{
    var items = await _db.FulfillmentLines
        .OrderByDescending(line => line.Priority)
        .Skip(page * pageSize)
        .Take(pageSize)
        .Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt))
        .ToListAsync(ct);

    int total = await _db.FulfillmentLines.CountAsync(ct);
    return new PagedResult<FulfillmentLineDto>(items, page, pageSize, total);
}
```

---

#### Q6. (R) A teammate splits sorting across two private helpers to keep methods small. The project no longer builds. Review the chain. What type broke the `ThenBy` call, and how would you structure multi-key sorts in production code?

```csharp
private IEnumerable<FulfillmentLine> SortByPriority(IEnumerable<FulfillmentLine> lines) =>
    lines.OrderByDescending(line => line.Priority);

private IEnumerable<FulfillmentLine> AddPlacedAtTieBreak(IEnumerable<FulfillmentLine> lines) =>
    lines.ThenBy(line => line.PlacedAt);

public IEnumerable<FulfillmentLineDto> GetPickList(IEnumerable<FulfillmentLine> openLines)
{
    IEnumerable<FulfillmentLine> sorted = SortByPriority(openLines);
    sorted = AddPlacedAtTieBreak(sorted);
    return sorted.Select(line => new FulfillmentLineDto(line.OrderId, line.Priority, line.PlacedAt));
}
```
