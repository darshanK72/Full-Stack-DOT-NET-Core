# Karat — Interview Questions

> **Folder:** `02. C# Language Integrated Query/11. Partitioning Operations`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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
