# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/11. Partitioning Operations`

---

#### Q1. (R) A catalog API exposes paged search results. A developer reuses the chapter's `GetPage` helper but never sorts the query. Users report duplicate SKUs on page 1 and page 2, and missing items when they refresh. Review the endpoint:

```csharp
public async Task<PagedResult<ProductDto>> GetProductsAsync(int page, int pageSize, CancellationToken ct)
{
    IQueryable<Product> query = _db.Products.Where(p => p.IsActive);

    IEnumerable<Product> pageItems = GetPage(query, page, pageSize);

    return new PagedResult<ProductDto>
    {
        Items = await pageItems.Select(MapToDto).ToListAsync(ct),
        Page = page,
        PageSize = pageSize,
    };
}

static IEnumerable<T> GetPage<T>(IEnumerable<T> source, int pageNumber, int pageSize)
{
    int offset = (pageNumber - 1) * pageSize;
    return source.Skip(offset).Take(pageSize);
}
```

What is wrong, and how do you fix it for stable API paging?

**Answer:** `Skip` and `Take` slice whatever order the provider returns — without a deterministic `OrderBy`, SQL Server (and other databases) may return rows in different physical order between executions, so page boundaries shift and items appear on multiple pages or disappear entirely.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | No `OrderBy` before `Skip`/`Take` | Unstable page contents across requests and refreshes |
| API contract | Clients assume page 2 is disjoint from page 1 | Duplicate and missing SKUs in UI |
| Design | `GetPage` encapsulates offset math but not ordering | Copy-paste bug from tutorial helper without Section 10 rule |

**Fix (priority order):**

1. Apply a **stable sort** before paging: `.OrderBy(p => p.Sku)` or `.OrderBy(p => p.Id)` — tie-break with a unique key so order is total, not partial.
2. Keep `Skip`/`Take` (or `GetPage`) **after** `OrderBy` in the query chain so EF translates `ORDER BY … OFFSET … FETCH`.
3. Document that page numbers are only meaningful on a sorted, filtered query; changing sort between requests invalidates cached page indices.
4. Optionally return `TotalCount` from a separate `CountAsync()` on the filtered query (without `Skip`/`Take`) so clients know when a page is empty vs out of range.

```csharp
IQueryable<Product> query = _db.Products
    .Where(p => p.IsActive)
    .OrderBy(p => p.Sku);

List<ProductDto> items = await GetPage(query, page, pageSize)
    .Select(MapToDto)
    .ToListAsync(ct);
```

**Production takeaway:** Paging without ordering is the top partitioning mistake in **Program.cs** Quick Reference — Karat tests whether you treat `OrderBy` as part of the paging contract, not an optional nicety. See ch.03 Ordering.

---

#### Q2. (R) A warehouse export job pages through 2 million inventory rows. One teammate keeps paging in EF; another pulls everything into memory first. Review both approaches:

```csharp
// Approach A — stays on IQueryable until the end
public async Task<List<InventoryRow>> ExportPageAsync(int page, int size, CancellationToken ct)
{
    return await _db.Inventory
        .OrderBy(r => r.Sku)
        .Skip((page - 1) * size)
        .Take(size)
        .ToListAsync(ct);
}

// Approach B — "so Skip works on a list"
public async Task<List<InventoryRow>> ExportPageAsync(int page, int size, CancellationToken ct)
{
    List<InventoryRow> allRows = await _db.Inventory.ToListAsync(ct);
    return allRows
        .OrderBy(r => r.Sku)
        .Skip((page - 1) * size)
        .Take(size)
        .ToList();
}
```

What breaks in production with approach B, and when is in-memory `Skip` acceptable?

**Answer:** Approach B materializes the entire table on every page request — `Skip`/`Take` then run in memory on a full list — which blows heap and network for large tables; approach A keeps partitioning on `IQueryable` so the database applies `OFFSET`/`FETCH` and returns only one page.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Scalability | `ToListAsync()` before `Skip` on 2M rows | O(n) memory and I/O per page request |
| Latency | Full-table pull repeated for each export page | Timeouts, GC pressure, DB load |
| Misconception | "`Skip` needs a list" | True for plain `IEnumerable`, but `IQueryable` providers translate `Skip`/`Take` to SQL |

**Fix (priority order):**

1. Prefer approach A: `OrderBy` → `Skip` → `Take` → `ToListAsync` on `IQueryable` so EF Core emits server-side paging.
2. Never call `AsEnumerable()` or `ToList()` before `Skip` unless the filter **cannot** translate to SQL and the working set is provably small.
3. For exports of the full dataset, stream with batched `Skip`/`Take` loops (or keyset paging — Q5) rather than one giant `ToList`.
4. In-memory `Skip` is acceptable when the source is already bounded — e.g., a `List<T>` of 200 pick lines loaded for one ticket, an in-memory cache snapshot, or unit tests over `Product[]` as in **Program.cs** Section 10.

**Production takeaway:** `Skip` on `IQueryable` vs `IEnumerable` is a provider boundary question — Karat expects you to know where partitioning executes (SQL vs CLR). See **Program.cs** Section 11 — IQueryable preview.

---

#### Q3. (R) A pricing analyst asks for "all products under $30." A developer uses `TakeWhile` because the tutorial catalog demo sorted by SKU and used it for an affordable prefix. Review the query over an unsorted `List<Product>` feed (same shape as **Program.cs** Section 5):

```csharp
List<Product> catalog = await _catalogService.LoadAllAsync(); // order not guaranteed

List<Product> underThirty = catalog
    .TakeWhile(p => p.UnitPrice < 30m)
    .ToList();

return underThirty.Select(MapToDto);
```

The API returns four SKUs including `Steel Toe Boots` at $89.99 when that row appears early in the feed, but omits cheaper gloves listed later. What went wrong, and what operator should replace `TakeWhile`?

**Answer:** `TakeWhile` yields a **contiguous prefix** from the start and stops at the first element that fails the predicate — it does not scan the whole sequence for every match, so an expensive boot early in the list terminates the prefix and all later cheap items are excluded.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `TakeWhile` used for "all matching" semantics | Wrong SKU set — business rule violated |
| Order sensitivity | Unsorted feed makes prefix arbitrary | Non-deterministic API results |
| Operator choice | Confused prefix window with filter | Matches tutorial demo meant for "initial affordable run," not global filter |

**Fix (priority order):**

1. Replace `TakeWhile` with **`Where(p => p.UnitPrice < 30m)`** when the requirement is every product under $30 regardless of position (ch.02 Filtering).
2. If order matters for display, add `OrderBy` **after** `Where`, not as a substitute for `Where`.
3. Reserve `TakeWhile` for true prefix rules: "leading rows while still in stock," "header lines while line type == metadata," etc. — as in **Program.cs** Sections 5–6.
4. Add a test with the seeded catalog where `Steel Toe Boots` precedes cheap gloves; assert `Where` returns all sub-$30 SKUs.

**Production takeaway:** The chapter's `TakeWhile(p => p.UnitPrice < 30m)` demo is order-dependent on `OrderBy(p => p.Sku)` — Karat embeds the trap by dropping sort and changing the business question to "all." See Quick Reference — "TakeWhile for all matching."

---

#### Q4. (R) An inventory portal tries to hide leading out-of-stock rows at the top of a catalog list, then show everything else — including out-of-stock items buried deeper in the list. A junior dev copies **Program.cs** `SkipWhile` but applies it to a re-sorted list:

```csharp
IEnumerable<Product> displayList = catalog
    .OrderByDescending(p => p.UnitPrice)   // most expensive first — reshuffles rows
    .SkipWhile(p => !p.IsActiveInStock);   // drop prefix while not sellable

foreach (Product p in displayList)
    RenderRow(p);
```

Out-of-stock vests still appear mid-list (expected), but expensive in-stock hard hats at the top vanish when a single discontinued SKU was cheapest. Explain the bug in terms of `SkipWhile` semantics and list order.

**Answer:** `SkipWhile` only drops a **leading contiguous prefix** — it is not "remove every out-of-stock row." Re-sorting by price before `SkipWhile` redefines that prefix, so the portal no longer matches the SKU-ordered tutorial behavior and featured rows can disappear from the top of the UI.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Semantics | `SkipWhile` ≠ global filter | Out-of-stock vests **after** the first in-stock row still render mid-list — expected for `SkipWhile`, wrong if PM wanted all OOS hidden |
| Order | `OrderByDescending` reshuffles the prefix | Leading run is now highest-price rows; expensive **out-of-stock** SKUs at the top are skipped entirely, so premium listings vanish from the header |
| Misread | Copied Section 6 without SKU sort | `SkipWhile(!IsActiveInStock)` on `OrderBy(p => p.Sku)` drops SKU-001/002 then yields from gloves onward — different story after price sort |
| UX | In-stock hard hats sorted below a block of skipped OOS premium rows | Users see a gap where featured items were expected — "vanished" from the top band |

**Fix (priority order):**

1. Separate concerns: **`OrderBy` for display** vs **`SkipWhile` for prefix trimming** — only combine when the business rule is literally "drop leading dead stock in **this** sort order."
2. If the rule is "hide all out-of-stock," use **`Where(p => p.IsActiveInStock)`** (ch.02 Filtering), not `SkipWhile`.
3. To match **Program.cs** Section 6, keep **`OrderBy(p => p.Sku)`** before `SkipWhile(!IsActiveInStock)`.
4. Snapshot with `.ToList()` when the UI enumerates more than once (see Q6).

**Production takeaway:** `SkipWhile` answers "drop the initial run, then show the rest" — reshuffling breaks the assumed prefix exactly like `TakeWhile`. Karat pairs this with **Program.cs** catalog seed where OOS rows lead only in SKU order.

---

#### Q5. (M) Deep paging on a sorted EF query uses `Skip(50000).Take(20)`. DBAs complain the same endpoint gets slower on later pages even though page size is only 20. A teammate suggests switching to `AsEnumerable()` before `Skip` so "LINQ doesn't push OFFSET to SQL." What does EF Core actually translate today, and why does unbounded `Skip` hurt on large offsets?

```csharp
IQueryable<Order> query = _db.Orders
    .Where(o => o.Status == OrderStatus.Open)
    .OrderBy(o => o.CreatedUtc);

// page 2500 with pageSize 20 → Skip(49980).Take(20)
return await query.Skip(offset).Take(pageSize).ToListAsync(ct);
```

Walk through IQueryable vs in-memory `Skip` behavior and one production alternative for deep pages.

**Answer:** EF Core translates `OrderBy` + `Skip` + `Take` on `IQueryable` to SQL `ORDER BY … OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY` — moving `Skip` client-side with `AsEnumerable()` would load **all** matching rows into memory before slicing, which is far worse; the slowness on page 2500 is OFFSET scan cost in the database, not a failure of server-side translation.

- **`IQueryable` path:** Provider composes expression tree → SQL with `OFFSET/FETCH`; only 20 rows cross the wire; CPU work on DB still proportional to offset for many engines (skip N rows after sort).
- **`AsEnumerable()` path:** Terminates translation; `Skip(49980)` walks 49,980+ rows in CLR after materializing the filter — unacceptable at scale.
- **Why deep OFFSET hurts:** The engine typically sorts (or uses an index on `CreatedUtc`) then discards the first 49,980 rows to return 20 — cost grows with page number even though page size is constant.
- **Production alternative — keyset (seek) paging:** Pass last-seen `(CreatedUtc, Id)` from previous page: `.Where(o => o.CreatedUtc > lastUtc || (o.CreatedUtc == lastUtc && o.Id > lastId)).OrderBy(...).Take(20)` — index-friendly, stable next page without large OFFSET.
- **When OFFSET is fine:** Early pages, admin UIs with modest totals, or when users rarely jump to page 2500.

**Production takeaway:** Karat contrasts **correct** server-side `Skip` with the anti-pattern of client-side `Skip`, then tests whether you know OFFSET limits — not whether you avoid SQL translation altogether.

---

#### Q6. (P) A mobile client requests page 0 with `pageSize=100` to "load everything in one call." The shared helper throws. Review **Program.cs** `GetPage` and this caller:

```csharp
public IActionResult GetCatalogPage(int page, int pageSize)
{
    IEnumerable<Product> pageItems = GetPage(_catalog, page, pageSize);
    int countOnPage = pageItems.Count();           // first enumeration
    return Ok(new { Items = pageItems, Count = countOnPage }); // second enumeration — serializer walks again
}

public static IEnumerable<T> GetPage<T>(IEnumerable<T> source, int pageNumber, int pageSize)
{
    if (pageNumber < 1)
        throw new ArgumentOutOfRangeException(nameof(pageNumber));
    if (pageSize < 1)
        throw new ArgumentOutOfRangeException(nameof(pageSize));

    int offset = (pageNumber - 1) * pageSize;
    return source.Skip(offset).Take(pageSize);
}
```

What fails for the client, what fails at runtime for the response, and how would you shape production paging (validation, materialization, total counts)?

**Answer:** `pageNumber = 0` violates the helper's 1-based contract and throws `ArgumentOutOfRangeException` before any data returns; even with valid input, returning a deferred `IEnumerable` that gets enumerated twice can double database work or show inconsistent counts if the underlying catalog changes between passes.

- **Client failure:** Page 0 is invalid — **Program.cs** Section 12 requires `pageNumber >= 1`; map client "zero-based index" to `(index + 1)` at the API boundary or document 1-based pages explicitly.
- **Double enumeration:** `Count()` walks the page; JSON serialization walks `pageItems` again — for `IQueryable` sources that means two round-trips; for live `IEnumerable` feeds, counts can diverge if rows change mid-request.
- **Materialize once:** `List<ProductDto> items = GetPage(...).Select(Map).ToList();` then return `{ Items = items, Count = items.Count }`.
- **Total counts:** Expose `TotalCount` from `query.CountAsync()` on the filtered sorted query (no `Skip`/`Take`), plus `Page`, `PageSize`, and optionally `HasNextPage` — do not infer totals from `Take` returning fewer than `pageSize` alone (last page vs empty page — Section 11 edge cases).
- **Caps:** Enforce a max `pageSize` (e.g., 100) so "load everything" cannot bypass pagination by sending `pageSize=int.MaxValue`.

```csharp
if (page < 1 || pageSize is < 1 or > 100)
    return BadRequest(/* … */);

var items = GetPage(sortedQuery, page, pageSize).Select(Map).ToList();
return Ok(new { Items = items, Count = items.Count, TotalCount = total, Page = page });
```

**Production takeaway:** `GetPage` validates offset math but callers must still sort, materialize, and align page numbering with clients — deferred `Skip`/`Take` plus double enumeration is a common API footgun tied to **Program.cs** Sections 10–12.
