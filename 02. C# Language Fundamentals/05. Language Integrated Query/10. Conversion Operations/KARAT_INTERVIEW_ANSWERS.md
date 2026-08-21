# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/10. Conversion Operations`

---

#### Q1. (R) A warehouse sync service materializes inventory before filtering low-stock alerts. Review this method when `catalog` is an EF Core `IQueryable<InventoryItem>` from `_db.Inventory`:

```csharp
public List<InventoryItem> GetLowStockAlerts(IQueryable<InventoryItem> catalog)
{
    var snapshot = catalog.ToList(); // ensure we have a list
    return snapshot
        .Where(item => item.StockQty > 0 && item.StockQty <= 10)
        .OrderBy(item => item.StockQty)
        .ToList();
}
```

What is wrong with calling `ToList()` this early, and how would you fix it?

**Answer:** The first `ToList()` forces EF to pull **every inventory row** into the app before `Where`/`OrderBy` run locally — you lose SQL-side filtering and pay full-table memory and network cost just to get a `List<T>`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Query translation | `ToList()` on `IQueryable` terminates provider execution | Entire table materialized; filter runs in CLR |
| Performance | Unnecessary row transfer for a narrow alert query | Slow sync, high memory, DB pressure at scale |
| Design | "Ensure we have a list" habit copied from in-memory tutorials | Correct for `List<T>` APIs, wrong timing for EF |

**Fix (priority order):**

1. Keep the pipeline on `IQueryable` until predicates are applied: `_db.Inventory.Where(...).OrderBy(...).ToListAsync(ct)`.
2. Call `ToList()` **once**, at the end, when you need a concrete collection for the caller — not at the start.
3. If the method must accept both `IQueryable` and in-memory sources, overload or branch: EF path stays deferred; only materialize in-memory inputs when required.

```csharp
return await catalog
    .Where(item => item.StockQty > 0 && item.StockQty <= 10)
    .OrderBy(item => item.StockQty)
    .ToListAsync(ct);
```

**Production takeaway:** `ToList()` is a terminal operator — placement decides whether work runs in SQL or in your process. See **Program.cs** Section 2 — materialize after the pipeline, not before. See foundation **LINQ ch.01** — deferred vs immediate execution.

---

#### Q2. (R) An API endpoint reports low-stock metrics by reusing one deferred query three times:

```csharp
IEnumerable<InventoryItem> lowStock = liveCatalog.Where(item =>
{
    _logger.LogDebug("Filtering {Sku}", item.Sku);
    return item.StockQty > 0 && item.StockQty <= 10;
});

int alertCount = lowStock.Count();
List<string> alertSkus = lowStock.Select(item => item.Sku).ToList();
decimal alertValue = lowStock.Sum(item => item.UnitPrice * item.StockQty);
```

Under load the endpoint is slow and logs show the filter running many times per request. What went wrong, and where should `ToList()` appear?

**Answer:** `lowStock` is a deferred recipe — each of `Count()`, `Select().ToList()`, and `Sum()` re-enumerates the source and re-runs the `Where` predicate (and logging side effects), so one HTTP request executes the filter three full passes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| LINQ execution | Three consumers on one deferred `IEnumerable` | Filter pipeline runs 3× per request |
| Side effects | Logging inside `Where` predicate | Log spam; predicate must stay pure in production |
| Performance | `Count` + `Sum` each walk all matches | O(3n) work and repeated I/O if source is remote |

**Fix (priority order):**

1. Materialize **once** after the filter: `List<InventoryItem> lowStock = liveCatalog.Where(...).ToList();`
2. Derive `Count`, SKU list, and `Sum` from that list — single enumeration of the expensive pipeline.
3. Remove logging from the predicate; log once after materialization if needed.
4. If the source is `IQueryable`, prefer a single DB round-trip with aggregates (`CountAsync`, projection) instead of multiple enumerations.

```csharp
List<InventoryItem> lowStock = liveCatalog
    .Where(item => item.StockQty > 0 && item.StockQty <= 10)
    .ToList();

int alertCount = lowStock.Count;
List<string> alertSkus = lowStock.Select(item => item.Sku).ToList();
decimal alertValue = lowStock.Sum(item => item.UnitPrice * item.StockQty);
```

**Production takeaway:** `ToList()` too **late** (never caching) is as costly as `ToList()` too **early** on EF — cache when you need multiple passes on the same filtered set. See **Program.cs** Section 2b — deferred re-run vs ToList cache.

---

#### Q3. (R) After a bulk import, a developer builds a SKU lookup map directly from the raw feed (duplicate SKU rows are common in imports):

```csharp
InventoryItem[] importedRows = await _importReader.ReadAllAsync();

Dictionary<string, InventoryItem> skuLookup =
    importedRows.ToDictionary(row => row.Sku);

// later: validate order lines with skuLookup.TryGetValue(...)
```

Production throws `ArgumentException: An item with the same key has already been added.` What failed, and how do you build a safe lookup?

**Answer:** `ToDictionary` requires **unique** keys — duplicate SKU rows in the import (as in **Program.cs** catalog seed with two `WH-4412` rows) cause an immediate `ArgumentException`; unlike `ToLookup` or `GroupBy`, duplicates are not merged.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Duplicate `Sku` values in import batch | Request/job fails mid-validation |
| Data contract | Raw feed treated as already deduplicated | Intermittent failures when vendors send duplicate rows |
| Operator choice | `ToDictionary` used where one-to-many is possible | Wrong tool for ambiguous keys |

**Fix (priority order):**

1. **Deduplicate with explicit rule** before `ToDictionary`: e.g. `DistinctBy(row => row.Sku)` keeping latest/highest stock, or `GroupBy` + `First()`.
2. If duplicates must be preserved for audit, use **`ToLookup`** or `GroupBy` — not `ToDictionary`.
3. Detect duplicates early and surface a structured import error (row numbers, conflicting SKUs) instead of letting `ToDictionary` throw a generic message.
4. Pass an `IEqualityComparer<string>` if key normalization (case, trim) is required — comparer does **not** allow duplicate keys, only changes equality.

```csharp
Dictionary<string, InventoryItem> skuLookup = importedRows
    .GroupBy(row => row.Sku)
    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.StockQty).First());
```

**Production takeaway:** Choose `ToDictionary` only when the business rule guarantees one row per key; otherwise dedupe upstream or use `ToLookup`. See **Program.cs** Section 5c — duplicate key guard.

---

#### Q4. (R) A legacy COM import returns `IEnumerable` (non-generic) with mixed runtime types. Two teammates propose different approaches:

```csharp
// Teammate A — strict typing
foreach (InventoryItem item in legacyFeed.Cast<InventoryItem>())
{
    ProcessRow(item);
}

// Teammate B — tolerant extraction
List<InventoryItem> items = legacyFeed.OfType<InventoryItem>().ToList();
foreach (InventoryItem item in items)
{
    ProcessRow(item);
}
```

The feed occasionally contains corrupt string rows like `"CORRUPT-ROW-NOT-AN-ITEM"`. Which approach fits production import validation, and what breaks if you choose the other?

**Answer:** Use **`OfType<InventoryItem>()`** for a mixed legacy feed — it skips incompatible elements and completes processing; **`Cast<InventoryItem>()`** throws `InvalidCastException` on the first bad row and aborts the entire import.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime (Cast) | Strict cast on every element during enumeration | One corrupt row kills the batch |
| Resilience (OfType) | Non-inventory elements silently skipped | Must add explicit corrupt-row reporting |
| Operations | Teammate A assumes feed is 100% typed | Valid for clean internal APIs; wrong for vendor/COM data |

**Fix (priority order):**

1. Default import path: `OfType<InventoryItem>()` + compare input count vs extracted count to detect dropped rows.
2. Log or quarantine skipped elements (type name, raw value) for reconciliation — do not silently lose data in finance/inventory systems.
3. Use `Cast<InventoryItem>()` only when the contract guarantees every element is an `InventoryItem` (fail-fast is desired).
4. Materialize with `ToList()` after `OfType` if you iterate results multiple times or need a count before processing.

**Production takeaway:** `Cast` = "all must be T"; `OfType` = "give me the T rows from a mixed bag." See **Program.cs** Sections 7–8 — Cast failure vs OfType preview on the same mixed feed.

---

#### Q5. (R) A catalog search endpoint tries to apply a custom C# helper inside an EF Core query:

```csharp
public async Task<List<InventoryItem>> SearchExpensiveAsync(CancellationToken ct)
{
    return await _db.Inventory
        .AsEnumerable()
        .Where(item => MatchesPricingPolicy(item)) // instance method — not translatable to SQL
        .OrderBy(item => item.UnitPrice)
        .Take(20)
        .ToListAsync(ct);
}
```

The query compiles but loads the entire `Inventory` table into memory on every search. What happened, and how do you fix it without abandoning EF translation?

**Answer:** `AsEnumerable()` switches the pipeline from **`IQueryable` (EF expression trees → SQL)** to **`IEnumerable` (LINQ-to-Objects)** — everything after it runs client-side, so EF fetches all rows before `Where`/`OrderBy`/`Take` can shrink the result set.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Provider binding | `AsEnumerable()` after `_db.Inventory` | SQL translation stops; client eval begins |
| Performance | Full table load per search | Memory spikes, timeouts, DB bandwidth waste |
| API misuse | `ToListAsync` on client-side sequence | Works but does not restore server-side filtering |

**Fix (priority order):**

1. Push translatable filters **before** any client switch: `.Where(item => item.UnitPrice >= floor).OrderBy(...).Take(20)` stays on `IQueryable`.
2. Replace non-translatable logic: map `MatchesPricingPolicy` to SQL-expressible rules, a DB computed column, or a sproc — not an instance method in the query.
3. If client logic is unavoidable, **narrow on the server first** (`Where`/`Take` on columns EF can translate), then call `.AsEnumerable()` on the small set — never on the full DbSet.
4. Use `.AsQueryable()` only when you intentionally need expression trees; do not confuse it with `AsEnumerable()`.

```csharp
return await _db.Inventory
    .Where(item => item.UnitPrice >= 500m) // translatable pre-filter
    .OrderBy(item => item.UnitPrice)
    .Take(200)
    .AsEnumerable()
    .Where(item => MatchesPricingPolicy(item))
    .Take(20)
    .ToList();
```

**Production takeaway:** `AsEnumerable()` is for extension-method binding on concrete collections (see **Program.cs** Section 9 — `InventoryCatalogCollection`), not a general escape hatch on EF queries. On EF, it is the client-eval trap. See **Program.cs** Section 10 — real provider translation belongs in EF Core modules.

---

#### Q6. (M) A pricing job snapshots equipment rows, then mutates live catalog prices while reporting uses the snapshot:

```csharp
List<InventoryItem> equipmentSnapshot =
    liveCatalog
        .Where(item => item.Category == ItemCategory.Equipment)
        .OrderBy(item => item.UnitPrice)
        .ToList();

// ... hours later, batch job updates UnitPrice on liveCatalog items ...

decimal reportedTotal = equipmentSnapshot.Sum(item => item.UnitPrice);
```

The report total changes even though `equipmentSnapshot.Count` is unchanged. Explain the behavior and what a production snapshot must guarantee if finance needs immutable prices.

**Answer:** `ToList()` freezes **membership and order** (structural snapshot), not deep copies of reference-type elements — `equipmentSnapshot` and `liveCatalog` share the same `InventoryItem` instances, so mutating `UnitPrice` on live rows changes values seen through the list.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | Shallow materialization of reference types | Count stable; property values drift |
| Reporting | Finance assumes snapshot = frozen prices | Incorrect totals, audit failures |
| Concurrency | Shared mutable entities across jobs | Race between pricing updates and reports |

**Fix (priority order):**

1. For immutable financial snapshots, project to **value types or DTOs** at materialization: `.Select(item => new PriceSnapshot(item.Sku, item.UnitPrice)).ToList()`.
2. Or deep-clone entities if downstream code requires full objects — explicit, not implied by `ToList()`.
3. Document team convention: `ToList()` = structural snapshot; immutability requires projection or clone.
4. Consider snapshot timestamp + version table for audit rather than relying on in-memory lists across long-running jobs.

**Production takeaway:** The warehouse tutorial deliberately uses mutable `UnitPrice` to teach shallow snapshots — production reporting must materialize **values**, not shared entity graphs. See **Program.cs** Section 2a — structural snapshot vs shared instances.
