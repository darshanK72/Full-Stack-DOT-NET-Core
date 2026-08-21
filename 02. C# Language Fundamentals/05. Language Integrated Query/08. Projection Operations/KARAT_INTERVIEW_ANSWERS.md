# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `02. C# Language Fundamentals/05. Language Integrated Query/08. Projection Operations`

---

#### Q1. (R) An order-summary API is slow under load. SQL Profiler shows one query for all orders, then one query per order for lines. Review this EF Core service method. What causes the N+1 pattern, and how do you fix it?

**Answer:** Materializing orders with `ToListAsync()` before the projection, then touching `o.Lines` inside an in-memory `Select`, triggers lazy loading (or repeated explicit loads) — one SQL round-trip per order after the initial query. The fix is to project everything needed in a single `IQueryable` pipeline so EF translates one SELECT (with a subquery/join/COUNT for line count) before materialization.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| EF / query shape | `ToListAsync()` before `Select` that reads `Lines` | N+1 SQL — 1 + N queries under load |
| Projection timing | Navigation accessed on tracked/materialized entities | Line counts computed client-side; DB hit per order |
| Performance | `OrderTotal` may also re-walk lines per order | CPU + I/O multiply with page size |

**Fix (priority order):**

1. Keep the pipeline as `IQueryable` until after projection — project in SQL, then `ToListAsync()`:

```csharp
return await _db.Orders
    .Where(o => o.PlacedOn >= cutoff)
    .Select(o => $"{o.OrderId} ({o.Customer}): {o.Lines.Count} line(s), total {o.OrderTotal:C}")
    .ToListAsync();
```

2. If you need a DTO instead of a formatted string, project to `OrderHeaderDto` or an anonymous shape **inside** the query — still one round-trip.
3. If lines must be included for other reasons, use `.Include(o => o.Lines)` **before** `ToListAsync()` — but prefer projecting only `Lines.Count` in SQL rather than loading every line row.
4. Add integration test or SQL logging that asserts query count = 1 for a page of orders.

**Production takeaway:** `Select` deferred over `IEnumerable` in memory is fine for in-memory LINQ (see **Program.cs** Section 4); over `IQueryable` in EF, projection must stay in the query until the terminal operator — otherwise N+1 dominates latency. See foundation **LINQ** — deferred execution vs EF translation.

---

#### Q2. (R) A warehouse pick-list report shows the wrong row count and nested loops in code review. Review this projection. What is wrong with the LINQ, and what is the correct fix?

**Answer:** `Select(o => o.Lines)` produces `IEnumerable<IReadOnlyList<OrderLine>>` — one inner list per order, not a flat stream of lines. Calling `.Count()` on that outer sequence counts **orders**, not SKUs, and the second method treats each inner list as a single row instead of flattening.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| LINQ semantics | `Select` when flattening is required | Nested `IEnumerable<IEnumerable<…>>`; wrong KPI count |
| Correctness | `.Select(lines => lines.First().Sku)` on nested lists | Drops all but first line per order; throws if a order has zero lines |
| Design | Manual nested `foreach` would be needed | Verbose, error-prone — wrong operator choice |

**Fix (priority order):**

1. Replace flattening `Select` with `SelectMany`:

```csharp
public int CountPickRows(IEnumerable<Order> orders) =>
    orders.SelectMany(o => o.Lines).Count();

public IEnumerable<string> BuildPickLabels(IEnumerable<Order> orders) =>
    orders.SelectMany(o => o.Lines).Select(line => line.Sku);
```

2. When each flat row needs parent fields (`OrderId`, `Customer`), use the three-parameter overload (Q5) — `SelectMany(o => o.Lines, (o, line) => …)`.
3. Add unit test: three orders with 2, 3, and 1 lines → `CountPickRows` must return 6, not 3.

**Production takeaway:** The nested-sequence trap in **Program.cs** Section 5 (`Select(o => o.Lines)` → inner list count ≠ SKU count) is harmless in a console demo but breaks warehouse KPIs in production — Karat tests whether you reach for `SelectMany` instinctively.

---

#### Q3. (R) A shared reporting library exposes order headers to a Web API project. The API project fails to compile after the refactor. Review both sides. What breaks at the assembly boundary, and what projection target should replace it?

**Answer:** Anonymous types are **internal to the assembly** where they are created — the compiler synthesizes a type name that is not accessible from `OrderApi`. Returning `IEnumerable<object>` erases member names, so `row.OrderId` does not compile (CS1061). Cross-assembly contracts need a named type or value tuple declared in a shared contract, not an anonymous projection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | Anonymous type as cross-assembly return shape | Consumer cannot name type or access members |
| API contract | `IEnumerable<object>` erases structure | No compile-time safety; logging/DTO mapping breaks |
| Design | Violates **Program.cs** Section 6 rule — anonymous for local only | Shared lib must expose stable shapes (Section 2 / 8) |

**Fix (priority order):**

1. Declare a named record in a shared contracts project and project into it:

```csharp
public readonly record struct OrderHeaderDto(string OrderId, string Customer, decimal OrderTotal);

public static IEnumerable<OrderHeaderDto> GetHighValueHeaders(
    IEnumerable<Order> orders, decimal minimum) =>
    orders
        .Where(o => o.OrderTotal >= minimum)
        .Select(o => new OrderHeaderDto(o.OrderId, o.Customer, o.OrderTotal));
```

2. For small **internal** helpers within one assembly, value tuples `(string OrderId, string Customer, decimal OrderTotal)` are acceptable (Section 7).
3. Never use `object` or `dynamic` as a public return type to smuggle anonymous types across boundaries.
4. API layer maps `OrderHeaderDto` to JSON response models if serialization attributes differ.

**Production takeaway:** Anonymous types excel for local reports (`var orderHeaders = orders.Select(o => new { … })` in **Program.cs** Section 6a) but cannot cross assembly lines — a common refactor trap when extracting a "shared" reporting library.

---

#### Q4. (R) A paginated orders endpoint returns quickly in dev (small DB) but transfers megabytes per page in production. Review the repository. What is over-fetched, and how should projection change the SQL?

**Answer:** `Include(o => o.Lines)` loads every column of every `OrderLine` row for the page into memory before the DTO `Select` runs client-side. The API only needs header fields (`OrderId`, `Customer`, `PlacedOn`, `OrderTotal`), so SQL should project those columns only — `OrderTotal` can be translated as a subquery/SUM without materializing line entities.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| EF / data transfer | Full `Order` + all `Lines` materialized | Large payloads; memory pressure on web tier |
| Projection placement | `Select` to DTO **after** `ToListAsync()` | SQL returns wide rows; network + GC cost in prod |
| Pagination | `Skip`/`Take` on headers but lines fully loaded | Page size 50 might still pull thousands of line rows |

**Fix (priority order):**

1. Project in the database, then paginate and materialize:

```csharp
var query = _db.Orders
    .OrderByDescending(o => o.PlacedOn)
    .Select(o => new OrderHeaderDto(
        o.OrderId,
        o.Customer,
        o.PlacedOn,
        o.OrderTotal));

var dtos = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .AsNoTracking()
    .ToListAsync();
```

2. Remove `.Include(o => o.Lines)` — not needed when `OrderTotal` and counts are translated in projection.
3. Verify generated SQL selects only DTO columns (EF Core logging or `ToQueryString()`).
4. For CSV export that **does** need lines, use a separate query path with `SelectMany` + narrow line DTO — do not reuse the header endpoint's include-everything pattern.

**Production takeaway:** **Program.cs** Section 8 shows `Select`/`SelectMany` reshaping data cheaply in memory; in EF, the same operators belong **before** materialization so the database sends only the columns the response needs.

---

#### Q5. (R) Flattening order lines for a shipping-label printer loses parent context — labels print without OrderId. Review this SelectMany usage. What is missing, and what does the three-parameter overload fix?

**Answer:** The two-parameter `SelectMany(o => o.Lines)` flattens to `OrderLine` only — parent `Order` fields are out of scope in the subsequent `Select`. The three-parameter overload `(collectionSelector, resultSelector)` pairs each line with its parent so `OrderId` and `Customer` survive flattening — exactly the warehouse pick-list pattern in **Program.cs** Section 9b.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Flatten without `resultSelector` | Shipping labels missing order identity |
| LINQ semantics | Second `Select` only sees `OrderLine` | Cannot recover `OrderId` without re-query or join |
| Domain | `ShippingLabelRow` requires parent context | Silent data loss in fulfillment pipeline |

**Fix (priority order):**

1. Use `SelectMany` with `resultSelector` (or project to `OrderLineSummary` / `ShippingLabelRow` in one step):

```csharp
return orders.SelectMany(
    o => o.Lines,
    (o, line) => new ShippingLabelRow(
        o.OrderId,
        o.Customer,
        line.Sku,
        line.ProductName,
        line.Quantity));
```

2. Query-syntax equivalent: `from o in orders from line in o.Lines select new ShippingLabelRow(…)` — same translation (Section 12c).
3. Add test: two lines under one order → both labels share that order's `OrderId`.
4. Prefer named DTO/record (`OrderLineSummary` in **Program.cs** Section 2) when the shape crosses services or printers.

**Production takeaway:** "Flatten without resultSelector → lose parent fields" is listed in **Program.cs** quick reference — Karat embeds it in a fulfillment scenario where the bug ships to production as mislabeled cartons.

---

#### Q6. (D) Your team ships three endpoints that all project orders: a JSON API, a CSV export, and an internal admin grid. One developer wants anonymous types everywhere "because LINQ is shorter." Another wants `(string Id, decimal Total)` tuples in the contracts assembly. A third wants `OrderLineSummary` records. What would you standardize for each boundary, and why?

**Answer:** Use anonymous types only inside a single method or private local report where the shape never leaves the method; use named records/DTOs (`OrderHeaderDto`, `OrderLineSummary`) for API and export contracts; use value tuples sparingly for small private helpers within one assembly — not as public HTTP response types.

**JSON API (public contract):**

- Named records or classes in a contracts project — stable names for OpenAPI/Swagger, versioning, and JSON serializers.
- Project with `Select`/`SelectMany` in EF **before** materialization (Q4) into those DTOs.
- Anonymous types cannot be action return types; tuples serialize awkwardly and are hard to evolve.

**CSV export (file contract):**

- Named row type (`OrderLineSummary` or `CsvOrderLineRow`) with explicit column mapping — export pipelines, tests, and header rows depend on stable property names.
- `SelectMany` + `resultSelector` when flattening lines with parent columns (Q5).

**Internal admin grid (same solution, not public NuGet):**

- Still prefer named DTOs shared with the API where shapes overlap — avoids duplicate anonymous projections that drift.
- Anonymous `new { … }` acceptable for one-off LINQ in a Blazor page **if** the shape stays in that component and is not returned from a shared library (Q3).

**Tuples in contracts assembly:**

- Acceptable for internal service-to-service helpers with 2–3 fields and no serialization on the wire; replace with records before exposing to HTTP or cross-team packages.

**Production takeaway:** **Program.cs** Sections 6–8 map shape choice to boundary — anonymous (local), tuple (small private), named record (API/serialization). Karat tests prioritization: brevity in a tutorial `Main` method does not justify anonymous types in a shared reporting lib or EF repository.

---
