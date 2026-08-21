# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/03. Entity Framework Core/10. Raw SQL & Stored Procedures`

---

#### Q1. (R) A junior developer adds a product search endpoint by adapting `GetProductsByNamePrefix` from **RawSqlQueryRepository.cs**. Review the repository method:

```csharp
public List<Product> SearchByName(string userInput)
{
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{userInput}%'
        ORDER BY ProductId
        """;

    return _context.Products
        .FromSqlRaw(sql)
        .AsNoTracking()
        .ToList();
}
```

They argue it is safe because they used `FromSqlRaw`, not ADO.NET string building. What is wrong, and how do you fix it?

**Answer:** `FromSqlRaw` only parameterizes values you pass as **separate arguments** with `{0}` placeholders — embedding `userInput` in the SQL string via C# interpolation inlines untrusted text into the command, which is classic SQL injection. The API name does not make concatenated strings safe.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `userInput` interpolated into SQL before `FromSqlRaw` | SQL injection — attacker can alter WHERE, UNION, or stack batches |
| Misconception | Assumes `FromSqlRaw` always parameterizes | `{0}` args are safe; pre-built strings with embedded values are not |
| API surface | Search exposed to HTTP query string | High-risk endpoint — injection reachable from production traffic |

**Fix (priority order):**

1. Replace with `FromSqlInterpolated` and pass the pattern as an interpolated value: `WHERE ProductName LIKE {$"%{userInput}%"}` — EF sends `userInput` as a parameter, not literal SQL text.
2. Or keep `FromSqlRaw` with `{0}`: `" ... WHERE ProductName LIKE {0}"` and pass `"%" + userInput + "%"` as the argument.
3. Validate/sanitize input length and reject obvious control characters at the API layer — defense in depth, not a substitute for parameters.
4. Never build SQL with `$"..."` or `+` when any segment comes from users — same rule as ADO.NET ch.03 and Dapper ch.01.

**Production takeaway:** Karat pairs EF Core APIs with injection traps — `FromSqlRaw` is safe **only** when user data never touches the SQL string literal. See **Program.cs** QUICK REFERENCE — "never concatenate user input."

---

#### Q2. (M) A teammate extends **StoredProcedureRepository.GetByMinStock** to filter expensive items in LINQ after the proc returns:

```csharp
public List<Product> GetExpensiveInStock(int minStock, decimal minPrice)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetProductsByMinStock @MinStock={0}", minStock)
        .Where(p => p.UnitPrice >= minPrice)
        .OrderBy(p => p.UnitPrice)
        .AsNoTracking()
        .ToList();
}
```

Tests pass on LocalDB with small data. What happens at the SQL layer, and what would you change for production?

**Answer:** `EXEC` stored-procedure SQL is **not composable** — EF Core cannot append a translated `WHERE UnitPrice >= @minPrice` to the procedure call. The provider typically executes the proc, materializes the full result set, then applies `.Where` and `.OrderBy` **in memory**, which silently becomes a client-side filter.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Performance | Client-side `.Where` after `EXEC` | Full proc result set shipped to the app — scales poorly on large catalogs |
| Correctness (hidden) | Appears like server-side LINQ | Reviewers assume SQL push-down; prod latency and memory spike under load |
| Design | Price filter belongs in proc or a composable SELECT | Duplicates business rules across C# and T-SQL |

**Fix (priority order):**

1. **Preferred:** Add `@MinPrice` to `usp_GetProductsByMinStock` (or a new proc) so filtering happens in SQL — matches how **SqlQueryRepository.GetLowStockReport** uses `EXEC` then **only** client sorts via `.AsEnumerable().OrderBy(...)`.
2. **Alternative:** Replace proc with a parameterized `FromSqlRaw` SELECT (composable) if you need LINQ composition: `SELECT ... FROM dbo.Products WHERE StockQuantity >= {0} AND UnitPrice >= {1}` then `.OrderBy` translates to SQL.
3. If client-side filtering is intentional, document it and call `.AsEnumerable()` before LINQ so intent is explicit — see **SqlQueryRepository.cs** Section 9 comment on non-composable SQL.
4. Load-test with realistic row counts — small LocalDB demos hide the trap.

**Production takeaway:** Composable raw SQL applies to plain `SELECT` fragments, not `EXEC`. Stacked LINQ after procs is a common Karat mechanism question — behavior is correct but often not what you want at scale. See foundation ADO.NET ch.07 — proc vs ad-hoc SQL trade-offs.

---

#### Q3. (R) A legacy reporting stored procedure returns a narrow shape — not full `Product` rows. A developer maps it to `DbSet<Product>` anyway:

```csharp
public List<Product> GetLowStockViaProc(int threshold)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)
        .AsNoTracking()
        .ToList();
}
```

`usp_GetLowStockReport` returns `ProductId`, `ProductName`, `StockQuantity`, and a computed `Status` column (see **LowStockRow.cs**). Review this mapping choice. What breaks at runtime or in maintenance, and what EF Core API fits this shape?

**Answer:** `DbSet<Product>.FromSqlRaw` expects a result shape that maps to **Product** columns — extra columns like `Status` are ignored, but missing required mapped columns (`UnitPrice`, `DiscontinuedDate`) leave properties at default values, producing silently wrong `Product` instances rather than a clear error.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Partial row mapped to full entity | `UnitPrice` = 0, `DiscontinuedDate` = null — consumers treat garbage as real inventory |
| Design | Report DTO forced into entity type | `Status` discarded; domain model polluted with read-model concerns |
| Maintainability | Proc column changes break entity assumptions | No compile-time check — wrong data in dashboards |

**Fix (priority order):**

1. Map to **`LowStockRow`** via `Database.SqlQueryRaw<LowStockRow>("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)` — as in **SqlQueryRepository.GetLowStockReport**.
2. Or register `LowStockRow` as a **keyless entity** (`HasNoKey()`) with `DbSet<LowStockRow>` if the shape participates in repeated LINQ graphs.
3. Reserve `DbSet<Product>.FromSqlRaw("EXEC ...")` for procs that SELECT the **same columns** as `dbo.Products` — like **StoredProcedureRepository.GetByMinStock**.
4. Add integration tests asserting column counts and non-default values for critical fields when mapping procs to entities.

**Production takeaway:** Stored-procedure mapping is not "any rows → any entity" — column contract must match. Karat tests whether you reach for `SqlQuery<T>` / keyless types vs misusing tracked entities. See **Program.cs** Section 8 vs Section 9 demos.

---

#### Q4. (R) Two developers argue about parameter safety. Compare these snippets from a refactored repository:

```csharp
// Developer A
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlRaw($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE '{prefix}%'
            """)
        .ToList();
}

// Developer B
public List<Product> ByPrefix(string prefix)
{
    return _context.Products
        .FromSqlInterpolated($"""
            SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
            FROM dbo.Products
            WHERE ProductName LIKE {prefix + "%"}
            """)
        .ToList();
}
```

Developer A says both use `$"""` so both are parameterized. Who is correct, and why does `FromSqlRaw` vs `FromSqlInterpolated` matter here?

**Answer:** Developer B is correct. Developer A passes a **fully formed string** to `FromSqlRaw` — C# interpolation runs first, inlining `prefix` as literal SQL. Developer B uses `FromSqlInterpolated`, which accepts a `FormattableString` and sends each interpolated expression as a **separate SqlParameter**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security (A) | `FromSqlRaw` + `$"..."` with embedded `{prefix}` | Equivalent to string concat — injection if `prefix` is user-controlled |
| Misconception (A) | `$"""` syntax confused with EF parameterization | `$` on `FromSqlRaw` does not invoke EF's param binder |
| Correct pattern (B) | `FromSqlInterpolated` + `{prefix + "%"}` | Values become `@p0` — matches **RawSqlQueryRepository.GetProductsByNamePrefix** |

**Fix (priority order):**

1. Delete Developer A's pattern; never pass interpolated strings to `FromSqlRaw`.
2. Safe `FromSqlRaw` form: `" ... WHERE ProductName LIKE {0}"` with `prefix + "%"` as the **second argument** — positional placeholders only.
3. Prefer `FromSqlInterpolated` when inline values read clearer — same safety as `{0}` args.
4. Code-review rule: if `FromSqlRaw` call has `$` before the string and no separate parameter arguments, flag it.

**Production takeaway:** The trap is syntactic — `$` string + `FromSqlRaw` looks modern but bypasses parameters. Karat stacks ADO.NET parameter lessons onto EF API names. See **RawSqlQueryRepository.cs** Section 6 parameter table.

---

#### Q5. (D) Your team needs inventory aggregate reports (**InventorySummary.cs**) in three services. One developer registers a keyless entity; another uses EF Core 8 `Database.SqlQuery<T>` only at call sites (as in **SqlQueryRepository.cs**). Compare:

```csharp
// Option A — OnModelCreating
modelBuilder.Entity<InventorySummary>().HasNoKey().ToView(null);
public DbSet<InventorySummary> InventorySummaries => Set<InventorySummary>();

// Option B — no DbSet registration (current chapter approach)
_context.Database.SqlQueryRaw<InventorySummary>("SELECT COUNT(*) AS ProductCount, ...").Single();
```

When would you pick keyless `DbSet<T>` vs `SqlQuery<T>`, and what pitfalls apply to each in a production API?

**Answer:** Both map read-only shapes without change tracking. Choose keyless `DbSet<T>` when the type is reused across queries, joins with entities, or global filters; choose `SqlQuery<T>` for one-off reports and EF Core 8+ ad-hoc DTO materialization without expanding the model.

- **Keyless `DbSet<T>` (`HasNoKey()`):** Good when `InventorySummary` appears in multiple repositories, needs `FromSqlRaw` on the set, or composes with entity LINQ. Pitfalls: must not call `Add`/`SaveChanges` on it; configure `ToView(null)` or explicit view name; team must know it is not a table — migrations won't create it.
- **`Database.SqlQuery<T>` / `SqlQueryRaw<T>`:** Good for isolated aggregates (**GetInventorySummary** in **SqlQueryRepository.cs**) — no model pollution, lighter for microservices that only need one report. Pitfalls: not on `DbSet` — no `Include`; composability same as raw SELECT; requires EF Core 8+.
- **Shared pitfall:** Property names must match column aliases (`ProductCount`, `TotalUnits`, `AveragePrice`); no parameterless constructor means materialization fails at runtime.
- **Production API:** Register keyless types in a shared `DbContext` when reports are first-class; use `SqlQuery` in vertical slices or read models to avoid bloating `OnModelCreating` for a single endpoint.
- **Avoid:** Treating either as an insert/update target — both are read-only; use entities or `ExecuteSqlRaw` for writes.

**Production takeaway:** Karat tests design judgment on read models — keyless entity is the EF-native reusable graph node; `SqlQuery<T>` is the EF 8 lightweight escape hatch. See **Models/InventorySummary.cs** comparison table and **EfCoreTutorialDbContext.cs** Section 4 note.

---

#### Q6. (P) **RawSqlQueryRepository.GetActiveProductsAbovePrice** appends LINQ after `FromSqlRaw`:

```csharp
return _context.Products
    .FromSqlRaw(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
          AND UnitPrice > {0}
        """,
        minimumPrice)
    .OrderBy(p => p.ProductName)
    .AsNoTracking()
    .ToList();
```

Explain how EF Core composes this query, when post-`FromSqlRaw` LINQ is safe vs when it pulls rows client-side, and one production scenario where you would **not** compose further LINQ on raw SQL.

**Answer:** For a composable `SELECT`, EF Core wraps the raw SQL as a subquery and appends translated LINQ (`ORDER BY ProductName`) in the same SQL command sent to SQL Server — so `.OrderBy` here is server-side, not an in-memory sort.

- **Composable (server-side):** Plain `SELECT` statements without `ORDER BY` in the raw fragment — EF can add `WHERE`, `ORDER BY`, `SKIP/TAKE` when the provider supports composition. This matches **RawSqlQueryRepository.cs** Section 6 — "You can append LINQ after FromSqlRaw."
- **Non-composable (client-side):** `EXEC` stored procedures, SQL with `ORDER BY` already in the raw string (provider-dependent), or vendor-specific batches — further LINQ may force client evaluation; use `.AsEnumerable()` explicitly when you accept that cost (**SqlQueryRepository.GetLowStockReport**).
- **Tracking note:** Without `.AsNoTracking()`, composed queries still return tracked entities if the raw SQL includes key columns (`ProductId`) — fine for updates, expensive for read-only APIs.
- **Scenario to avoid composition:** Legacy proc that already applies complex filtering — pushing extra `.Where` in LINQ hides that all rows cross the wire; fix the proc or use a composable SELECT instead.
- **Debugging:** Log `ToQueryString()` (EF Core 5+) or enable SQL logging to verify whether `ORDER BY` appears in the final batch — do not assume from LINQ syntax alone.

**Production takeaway:** Composable raw SQL is a strength of EF Core over hand-rolled ADO.NET for paginated/filtered reports — but only on plain SELECTs. Pair with Q2's `EXEC` trap: composition rules are the dividing line. See **Program.cs** QUICK REFERENCE — "When to stay on LINQ vs raw SQL."

---
