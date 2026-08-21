# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/01. Introduction to LINQ`

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

**Answer:** `Where` and `Select` are deferred — building `deferredLabels` does not run the pipeline, so `projectionRuns` stays 0 until enumeration. The guard treats "not executed yet" as "no rows," short-circuits, and never calls `File.WriteAllLines`, which is the first place that would have consumed the query.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Execution model | Side-effect / count checked before terminal or `foreach` | False "empty" branch — audit never writes file |
| Correctness | Confuses query *construction* with query *execution* | Silent data loss in batch jobs |
| Design | Counter inside `Select` used as existence probe | Misleading telemetry; wrong control flow |

**Fix (priority order):**

1. Use an **immediate** terminal for the guard: `if (!deferredLabels.Any()) return;` or `var list = deferredLabels.ToList(); if (list.Count == 0) return;` then write `list`.
2. Do not infer row count from side effects in deferred operators — use `Any()`, `Count()`, or materialize once.
3. Keep deferral for composition until you need a snapshot; batch exports should materialize once then write (`ToList()` + `WriteAllLines`).
4. Align logging with execution: log after consumption ("Wrote N lines") not after building the recipe.

```csharp
var deferredLabels = catalog
    .Where(i => i.Status == StockStatus.Backordered)
    .Select(i => $"[{i.Sku}] {i.Name}");

if (!deferredLabels.Any())
{
    Console.WriteLine("WARNING: No backordered SKUs found — skipping file write.");
    return;
}

File.WriteAllLines("backordered.txt", deferredLabels);
```

**Production takeaway:** Deferred LINQ is a recipe until `foreach`, `ToList`, `Count`, `Any`, etc. — Karat tests whether you catch guards that run before the first terminal. See **Program.cs** Section 10 — deferred execution and QUICK REFERENCE — "Expect query to run at declaration."

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

**Answer:** The service returns a deferred `IEnumerable<CatalogItem>` and the caller runs two terminals (`Count()` then `Select` + string join), so the full filter pipeline executes twice over `liveFeed` — classic multiple enumeration.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Multiple enumeration | `Count()` then second pass for CSV | 2× CPU / 2× scans on hot path |
| API contract | `IEnumerable<T>` return hides laziness | Callers cannot know safe to enumerate once vs many |
| Scalability | Repeated work per request under load | Latency and allocation pressure |

**Fix (priority order):**

1. Materialize once at the boundary: `var premium = _catalogService.GetPremiumActiveSkus(liveFeed).ToList();` then use `premium.Count` and project from the list.
2. Better: change service to return `IReadOnlyList<CatalogItem>` or `List<CatalogItem>` when the result is meant to be consumed multiple times.
3. If only a count is needed early, use a single pass (`ToList()` once, or combine into one enumeration).
4. Document deferred returns — if keeping `IEnumerable`, XML doc should say "single-pass; call `ToList()` if enumerating more than once."

```csharp
var premium = _catalogService.GetPremiumActiveSkus(liveFeed).ToList();
_logger.LogInformation("Premium SKU count: {Count}", premium.Count);
var csv = string.Join(",", premium.Select(i => i.Sku));
await _cache.SetAsync("premium-skus", csv);
```

**Production takeaway:** Returning deferred sequences from services without materialization invites double enumeration — materialize at the seam or return concrete collections. See **Program.cs** Section 10 — "Enumerating a deferred query twice runs the work twice."

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

**Answer:** Assigning the composed query to `IEnumerable<CatalogItem>` can force early shift to LINQ to Objects (or obscure that execution is deferred until serialization after the request scope ends). Enumeration then runs against a disposed `DbContext`, and provider translation may be lost so filters run in memory after pulling too many rows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Type erasure | `IQueryable` → `IEnumerable` assignment | May drop `IQueryProvider` / expression tree — SQL not composed |
| Lifetime | Deferred execution after action returns | `DbContext` disposed before JSON serializer enumerates |
| Performance | Client-side evaluation of filters | Full table read + memory spike |
| API | `Ok(items)` on lazy sequence tied to scoped context | Intermittent `ObjectDisposedException` in prod |

**Fix (priority order):**

1. Keep the query as `IQueryable<CatalogItem>` through composition; execute before leaving the action: `var items = repo.Items.Where(...).Where(...).ToListAsync(ct); return Ok(items);`
2. Never return live `IQueryable`/`IEnumerable` tied to a scoped `DbContext` without materializing — ASP.NET serialization is a second execution phase.
3. Ensure filters stay translatable to SQL (no premature `.AsEnumerable()`).
4. Use `await` + `ToListAsync` / `AsNoTracking()` as appropriate for read endpoints.

```csharp
[HttpGet("electronics")]
public async Task<IActionResult> GetElectronics(
    [FromServices] ICatalogRepository repo,
    CancellationToken ct)
{
    var items = await repo.Items
        .Where(i => i.Category == "Electronics")
        .Where(i => i.Status != StockStatus.Discontinued)
        .AsNoTracking()
        .ToListAsync(ct);

    return Ok(items);
}
```

**Production takeaway:** `IQueryable` is for building remote queries; `IEnumerable` is for in-memory sequences — widening too early or deferring past `DbContext` lifetime breaks EF. See **Program.cs** Section 12 — LINQ to Entities preview (`IQueryable<T>` translated to SQL).

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

**Answer:** `.AsEnumerable()` switches the pipeline from LINQ to Entities to LINQ to Objects at that point — everything after runs in-process on whatever rows were already fetched. Only the first `Where` stays in SQL; `ComplexMarginRule` cannot translate, so the app pulls all Active SKUs then filters in memory.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Provider leak | `AsEnumerable()` before second filter | SQL returns wide row set; business filter not pushed down |
| Performance | Full Active set materialized | Memory + network blow up on large catalogs |
| Design | Non-translatable logic mixed into IQueryable chain without boundary | Looks like one query; behaves like table scan + client filter |

**Fix (priority order):**

1. Push translatable predicates before the provider switch: add `Where(i => i.UnitPrice > …)` or SQL-friendly filters in EF when possible.
2. Call `.AsEnumerable()` **immediately before** the non-translatable `Where(ComplexMarginRule)` — narrow in SQL first (`Active`, price floor, category, etc.).
3. Long-term: express `ComplexMarginRule` in SQL (computed column, view, raw SQL, or fetch only needed columns/ids then hydrate).
4. Profile with SQL + memory — treat every `AsEnumerable()` / `ToList()` in an EF chain as a explicit "client eval starts here" comment in review.

```csharp
return db.CatalogItems
    .Where(i => i.Status == StockStatus.Active)
    .Where(i => i.UnitPrice >= minPrice / 1.15m) // cheap SQL pre-filter when safe
    .AsEnumerable()
    .Where(i => ComplexMarginRule(i) > minPrice)
    .ToList();
```

**Production takeaway:** `AsEnumerable()` is not a performance trick — it **changes the LINQ provider** and stops expression translation. See **Program.cs** provider table — LINQ to Objects vs LINQ to Entities.

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

**Answer:** After `select … into label`, the range variable `i` is out of scope — the continuation only sees `label` (a `string`). The second `where i.Category == "Electronics"` references `i` after projection, which does not compile (CS0103). The original logic filtered on **both** status and category **before** projecting to a string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `i` used after `select`/`into` | CS0103 — name not in scope |
| Logic | Category filter applied after label projection | Even if rewritten, would filter on string content, not `Category` |
| Review | Mechanical syntax conversion without equivalence check | Broken build; wrong business rule if forced |

**Fix (priority order):**

1. Apply both filters **before** `select` in query syntax:

```csharp
from i in catalog
where i.Status == StockStatus.Active
where i.Category == "Electronics"
select $"{i.Sku}: {i.Name}";
```

2. Or keep method syntax (often clearer for short chains) — matches **Program.cs** Section 9 equivalence table.
3. Use `into` only when you need to filter/order/group on the **projected** shape, e.g. `where label.Contains("SKU-10")`, not on fields dropped by `select`.
4. In PR review, verify query and method forms with the same sample catalog (Active electronics count).

**Production takeaway:** Query syntax is sugar over method calls — clause order and range-variable scope matter. Karat uses bad refactors to test whether you map `from`/`where`/`select` to `Where`/`Select`. See **Program.cs** Sections 7–9 — method vs query syntax.

---

#### Q6. (P) Your API caches "active catalog snapshots" for five minutes. Two implementations are proposed:

```csharp
// A
IEnumerable<CatalogItem> snapshot = catalog.Where(i => i.Status == StockStatus.Active);

// B
List<CatalogItem> snapshot = catalog.Where(i => i.Status == StockStatus.Active).ToList();
```

The underlying `catalog` array is mutated when warehouse workers update SKU status between requests. Callers enumerate the cached value multiple times per HTTP request (validation, mapping, CSV export). Which approach do you ship, when do you materialize, and why?

**Answer:** Ship **B** — materialize with `ToList()` when caching or handing results to multiple consumers. A deferred `IEnumerable` rebinds to the live source on every enumeration, so mutations change results mid-request and repeated passes re-run the filter.

- **Snapshot semantics:** `ToList()` freezes Active items at cache-fill time — consistent validation, mapping, and export within the five-minute window even if the array mutates.
- **Multiple enumeration:** Callers run three passes per request — deferral triples work; a `List<T>` makes Count/indexing cheap (`Count` property, no re-filter).
- **Cache storage:** Memory cache entries should hold concrete collections (`List<CatalogItem>` or `IReadOnlyList<CatalogItem>`), not live queryables tied to mutable in-memory arrays.
- **When to stay deferred (A):** Single consumer, single pass, read-only source, and composition still ongoing (building a larger pipeline) — not this scenario.
- **EF variant:** Same rule at the `DbContext` boundary — `ToListAsync` inside the scope before caching.

**Production takeaway:** Defer for composition; materialize for stability, caching, and multi-pass APIs — matches **Program.cs** Section 11 immediate execution / snapshot pattern and Section 10 pitfall on double enumeration.
