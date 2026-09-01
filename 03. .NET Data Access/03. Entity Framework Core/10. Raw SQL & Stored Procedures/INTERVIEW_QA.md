# 10. Raw SQL & Stored Procedures — Interview Q&A
> Back to [Entity Framework Core Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Q66. What is `FromSqlRaw` versus `FromSqlInterpolated`?](#q66-what-is-fromsqlraw-versus-fromsqlinterpolated)
- [Q67. Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL?](#q67-why-is-fromsqlinterpolated-preferred-over-string-interpolation-for-raw-sql)
- [Q68. When should you use raw SQL instead of LINQ in EF Core?](#q68-when-should-you-use-raw-sql-instead-of-linq-in-ef-core)
- [Q69. What are the security considerations for raw SQL in EF Core?](#q69-what-are-the-security-considerations-for-raw-sql-in-ef-core)
- [Q70. How do you call stored procedures with EF Core?](#q70-how-do-you-call-stored-procedures-with-ef-core)
- [Q71. How do you execute non-query raw SQL (`ExecuteSqlRaw`)?](#q71-how-do-you-execute-non-query-raw-sql-executesqlraw)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 10. Raw SQL & Stored Procedures

---

## Q66. What is `FromSqlRaw` versus `FromSqlInterpolated`?

**Concepts**
- FromSqlRaw with SqlParameter for parameterization
- FromSqlInterpolated with FormattableString
- SQL injection risk from plain interpolation in FromSqlRaw
- entity shape alignment requirement

**Answer**

Both methods query entities with raw SQL instead of LINQ, but `FromSqlRaw` accepts a plain string (with optional `SqlParameter` objects) while `FromSqlInterpolated` accepts a `FormattableString` that EF Core converts into parameterized SQL. `FromSqlInterpolated` prevents accidental injection from interpolated values. `FromSqlRaw("SELECT * FROM Products WHERE CategoryId = {0}", id)` — placeholders become parameters when passed correctly; `FromSqlInterpolated($"SELECT * FROM Products WHERE CategoryId = {id}")` — compiler-generated `FormattableString` ensures parameterization. Never pass an ordinary interpolated string to `FromSqlRaw` — `$"..."` evaluated before the call embeds literals unsafely. Both require the SQL to map to the entity shape (column names compatible with the entity type).

---

## Q67. Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL?

**Concepts**
- FormattableString preserves argument boundaries
- parameterized plan cache reuse
- injection risk from string interpolation evaluated before call
- ADO.NET parameterized command analogy

**Answer**

`FromSqlInterpolated` treats each interpolated value as a separate SQL parameter, so user input never becomes part of the SQL text. Ordinary C# string interpolation builds a single literal string before EF Core sees it, reintroducing SQL injection risk identical to concatenation. `FormattableString` preserves argument boundaries — EF Core emits `WHERE Id = @p0` with bound parameters; `$"WHERE Name = '{name}'"` passed to `FromSqlRaw` embeds raw text — attackers can inject malicious SQL. Parameterization also improves plan cache reuse compared to ad hoc literal SQL per distinct value. Same safety model as parameterized ADO.NET commands and Dapper anonymous parameter objects.

---

## Q68. When should you use raw SQL instead of LINQ in EF Core?

**Concepts**
- window functions and recursive CTE coverage
- database-specific syntax and hints
- stored procedure result mapping
- performance-critical plan tuning

**Answer**

Use raw SQL when LINQ cannot express the query efficiently or at all — complex reporting queries, database-specific features, optimized hints, bulk reads of views, or legacy stored procedures. Raw SQL trades provider translation and compile-time checking for full control over the statement sent to the server. Window functions, recursive CTEs, or vendor-specific syntax unavailable in LINQ; Performance-critical queries where a hand-tuned plan outperforms generated EF SQL. Mapping to keyless entity types or database views for read-only reporting models. Calling existing stored procedures the organization already maintains in the database.

---

## Q69. What are the security considerations for raw SQL in EF Core?

**Concepts**
- parameterization requirement for all values
- SQL injection via concatenation
- dynamic identifier allowlisting
- production parameter logging caution

**Answer**

All dynamic values must reach the database as parameters, never as concatenated SQL text. Use `FromSqlInterpolated`, parameterized `FromSqlRaw`, or `ExecuteSqlRaw` with `SqlParameter` objects — and validate that no user-controlled input defines structural SQL elements (table or column names) without strict allowlisting. SQL injection remains possible if raw strings embed user input directly; `ExecuteSqlRaw` with `{0}` placeholders parameterizes values; string building before the call does not. Dynamic identifiers (sort columns, table names) cannot be parameterized — map user input to a fixed allowlist. Logging raw SQL in production should redact or avoid sensitive parameter values even when queries are parameterized.

---

## Q70. How do you call stored procedures with EF Core?

**Concepts**
- FromSqlRaw EXEC syntax for query procedures
- FromSqlInterpolated for safe parameter passing
- ExecuteSqlRaw for non-query procedures
- keyless entity mapping for non-entity result shapes

**Answer**

Map stored procedure result sets to entity types or keyless types and invoke them with `FromSqlRaw` or `FromSqlInterpolated` using `EXEC` syntax, or use `ExecuteSqlRaw` for procedures that perform non-query work. Pass parameters as method arguments or `SqlParameter` instances. Query returning rows: `context.Products.FromSqlRaw("EXEC GetProductsByCategory @CategoryId = {0}", categoryId)`; Interpolated: `context.Orders.FromSqlInterpolated($"EXEC usp_GetOrders @StartDate = {start}, @EndDate = {end}")`. Non-query procedures: `context.Database.ExecuteSqlRaw("EXEC usp_ArchiveOrders @BeforeDate = {0}", date)`. Result shape must match entity columns; keyless types suit procedures returning non-entity projections.

---

## Q71. How do you execute non-query raw SQL (`ExecuteSqlRaw`)?

**Concepts**
- rows-affected return value
- change tracker bypass
- ExecuteSqlInterpolated as safer variant
- ExecuteUpdate and ExecuteDelete as preferred alternatives

**Answer**

`ExecuteSqlRaw` and `ExecuteSqlInterpolated` run INSERT, UPDATE, DELETE, or DDL statements that do not return entity rows, returning the number of rows affected. They execute outside the change tracker — tracked entities in memory are not automatically updated. `await context.Database.ExecuteSqlRawAsync("UPDATE Products SET Active = 0 WHERE Discontinued = 1");`; Parameterized: `ExecuteSqlRawAsync("DELETE FROM Logs WHERE CreatedUtc < {0}", cutoffDate)`. Prefer `ExecuteUpdate`/`ExecuteDelete` (EF Core 7+) for bulk operations that EF can translate from LINQ when raw SQL is not required. Wrap multi-statement raw SQL in an explicit transaction when atomicity is required.

---

## Gotchas

---

## Gotcha 1. String concatenation instead of parameters

**Concepts**
- SQL injection via string concatenation
- parameterization bypass risk
- dynamic SQL as code review blocker

**Answer**

Building SQL with `$"WHERE Id = {id}"` or string concatenation sends user input as literal SQL text, bypassing parameterization and enabling SQL injection even when the rest of the application uses an ORM or micro-ORM. ADO.NET and Dapper require explicit parameters — never embed raw user strings in SQL text; EF Core `FromSqlInterpolated` is safe; passing an ordinary interpolated string to `FromSqlRaw` is not. Code review should treat any dynamic SQL without placeholders as a blocking defect.

---

## Gotcha 2. Open DataReader blocks second command

**Concepts**
- open DataReader blocking second command
- MARS requirement on shared connection
- reader disposal before next command

**Answer**

Running another `SqlCommand` on the same connection while a `SqlDataReader` is still open fails on SQL Server unless Multiple Active Result Sets (MARS) is enabled in the connection string. Always dispose or finish reading the `DataReader` before issuing the next command on that connection; A common bug loads a header row then tries to load detail rows on the same connection without closing the reader. EF Core manages readers internally, but raw ADO.NET code in the same request must respect this rule.

---

## Scenario-Based Questions (Karat Format)

---

## Q150. (R) A junior developer adds a product search endpoint by adapting `GetProductsByNamePrefix` from **RawSqlQueryRepository.cs**. Review the repository method:

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

**Concepts**
- string interpolation in FromSqlRaw causing SQL injection
- FormattableString parameterization in FromSqlInterpolated
- user input as literal SQL text risk
- code review detection of injection pattern

**Answer**

`FromSqlRaw` only parameterizes values you pass as separate arguments with `{0}` placeholders — embedding `userInput` in the SQL string via C# interpolation inlines untrusted text into the command, which is classic SQL injection. The API name does not make concatenated strings safe.

1. Replace with `FromSqlInterpolated` and pass the pattern as an interpolated value: `WHERE ProductName LIKE {$"%{userInput}%"}` — EF sends `userInput` as a parameter, not literal SQL text.
2. Or keep `FromSqlRaw` with `{0}`: `" ... WHERE ProductName LIKE {0}"` and pass `"%" + userInput + "%"` as the argument.
3. Validate/sanitize input length and reject obvious control characters at the API layer — defense in depth, not a substitute for parameters.
4. Never build SQL with `$"..."` or `+` when any segment comes from users — same rule as ADO.NET ch.03 and Dapper ch.01.

---

## Q151. (M) A teammate extends **StoredProcedureRepository.GetByMinStock** to filter expensive items in LINQ after the proc returns:

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

**Concepts**
- LINQ filter after stored procedure returning all rows
- client-side filtering vs server-side WHERE
- stored procedure result materialization before LINQ
- EF Core set composition limitation with FromSqlRaw

**Answer**

`EXEC` stored-procedure SQL is not composable — EF Core cannot append a translated `WHERE UnitPrice >= @minPrice` to the procedure call. The provider typically executes the proc, materializes the full result set, then applies `.Where` and `.OrderBy` in memory, which silently becomes a client-side filter.

1. **Preferred:** Add `@MinPrice` to `usp_GetProductsByMinStock` (or a new proc) so filtering happens in SQL — matches how **SqlQueryRepository.GetLowStockReport** uses `EXEC` then **only** client sorts via `.AsEnumerable().OrderBy(...)`.
2. **Alternative:** Replace proc with a parameterized `FromSqlRaw` SELECT (composable) if you need LINQ composition: `SELECT ... FROM dbo.Products WHERE StockQuantity >= {0} AND UnitPrice >= {1}` then `.OrderBy` translates to SQL.
3. If client-side filtering is intentional, document it and call `.AsEnumerable()` before LINQ so intent is explicit — see **SqlQueryRepository.cs** Section 9 comment on non-composable SQL.
4. Load-test with realistic row counts — small LocalDB demos hide the trap.

---

## Q152. (R) A legacy reporting stored procedure returns a narrow shape — not full `Product` rows. A developer maps it to `DbSet<Product>` anyway:

```csharp
public List<Product> GetLowStockViaProc(int threshold)
{
    return _context.Products
        .FromSqlRaw("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)
        .AsNoTracking()
        .ToList();
}
```

**Concepts**
- narrow proc result mapped to full entity DbSet
- column count mismatch and missing columns
- keyless entity type for non-entity proc shape
- SqlQuery<T> vs FromSqlRaw entity mapping

**Answer**

`DbSet<Product>.FromSqlRaw` expects a result shape that maps to Product columns — extra columns like `Status` are ignored, but missing required mapped columns (`UnitPrice`, `DiscontinuedDate`) leave properties at default values, producing silently wrong `Product` instances rather than a clear error.

1. Map to **`LowStockRow`** via `Database.SqlQueryRaw<LowStockRow>("EXEC dbo.usp_GetLowStockReport @Threshold={0}", threshold)` — as in **SqlQueryRepository.GetLowStockReport**.
2. Or register `LowStockRow` as a **keyless entity** (`HasNoKey()`) with `DbSet<LowStockRow>` if the shape participates in repeated LINQ graphs.
3. Reserve `DbSet<Product>.FromSqlRaw("EXEC ...")` for procs that SELECT the **same columns** as `dbo.Products` — like **StoredProcedureRepository.GetByMinStock**.
4. Add integration tests asserting column counts and non-default values for critical fields when mapping procs to entities.

---

## Q153. (R) Two developers argue about parameter safety. Compare these snippets from a refactored repository:

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

**Concepts**
- parameterized vs concatenated raw SQL safety
- FromSqlInterpolated vs FromSqlRaw string-build comparison
- DbParameter explicit parameterization
- injection risk in both approaches

**Answer**

Developer B is correct. Developer A passes a fully formed string to `FromSqlRaw` — C# interpolation runs first, inlining `prefix` as literal SQL. Developer B uses `FromSqlInterpolated`, which accepts a `FormattableString` and sends each interpolated expression as a separate SqlParameter.

1. Delete Developer A's pattern; never pass interpolated strings to `FromSqlRaw`.
2. Safe `FromSqlRaw` form: `" ... WHERE ProductName LIKE {0}"` with `prefix + "%"` as the **second argument** — positional placeholders only.
3. Prefer `FromSqlInterpolated` when inline values read clearer — same safety as `{0}` args.
4. Code-review rule: if `FromSqlRaw` call has `$` before the string and no separate parameter arguments, flag it.

---

## Q154. (D) Your team needs inventory aggregate reports (**InventorySummary.cs**) in three services. One developer registers a keyless entity; another uses EF Core 8 `Database.SqlQuery<T>` only at call sites (as in **SqlQueryRepository.cs**). Compare:

```csharp
// Option A — OnModelCreating
modelBuilder.Entity<InventorySummary>().HasNoKey().ToView(null);
public DbSet<InventorySummary> InventorySummaries => Set<InventorySummary>();

// Option B — no DbSet registration (current chapter approach)
_context.Database.SqlQueryRaw<InventorySummary>("SELECT COUNT(*) AS ProductCount, ...").Single();
```

**Concepts**
- keyless entity vs Database.SqlQuery<T> for aggregate report
- singleton registration of keyless vs call-site SqlQuery
- reuse across services trade-off
- EF Core 8 SqlQuery<T> API

**Answer**

Both map read-only shapes without change tracking. Choose keyless `DbSet<T>` when the type is reused across queries, joins with entities, or global filters; choose `SqlQuery<T>` for one-off reports and EF Core 8+ ad-hoc DTO materialization without expanding the model. - Keyless `DbSet<T>` (`HasNoKey()`): Good when `InventorySummary` appears in multiple repositories, needs `FromSqlRaw` on the set, or composes with entity LINQ. Pitfalls: must not call `Add`/`SaveChanges` on it; configure `ToView(null)` or explicit view name; team must know it is not a table — migrations won't create it. - `Database.SqlQuery<T>` / `SqlQueryRaw<T>`: Good for isolated aggregates (GetInventorySummary in SqlQueryRepository.cs) — no model pollution, lighter for microservices that only need one report. Pitfalls: not on `DbSet` — no `Include`; composability same as raw SELECT; requires EF Core 8+. - Shared pitfall: Property names must match column aliases (`ProductCount`, `TotalUnits`, `AveragePrice`); no parameterless constructor means materialization fails at runtime. - Production API: Register keyless types in a shared `DbContext` when reports are first-class; use `SqlQuery` in vertical slices or read models to avoid bloating `OnModelCreating` for a single endpoint. - Avoid: Treating either as an insert/update target — both are read-only; use entities or `ExecuteSqlRaw` for writes. Production takeaway: Karat tests design judgment on read models — keyless entity is the EF-native reusable graph node; `SqlQuery<T>` is the EF 8 lightweight escape hatch. See Models/InventorySummary.cs comparison table and EfCoreTutorialDbContext.cs Section 4 note.

---

## Q155. (P) **RawSqlQueryRepository.GetActiveProductsAbovePrice** appends LINQ after `FromSqlRaw`:

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

**Concepts**
- LINQ composition after FromSqlRaw for server-side filtering
- IQueryable chain on FromSqlRaw result
- additional Where translated to SQL WHERE
- FromSqlRaw as composable IQueryable source

**Answer**

For a composable `SELECT`, EF Core wraps the raw SQL as a subquery and appends translated LINQ (`ORDER BY ProductName`) in the same SQL command sent to SQL Server — so `.OrderBy` here is server-side, not an in-memory sort. - Composable (server-side): Plain `SELECT` statements without `ORDER BY` in the raw fragment — EF can add `WHERE`, `ORDER BY`, `SKIP/TAKE` when the provider supports composition. This matches RawSqlQueryRepository.cs Section 6 — "You can append LINQ after FromSqlRaw." - Non-composable (client-side): `EXEC` stored procedures, SQL with `ORDER BY` already in the raw string (provider-dependent), or vendor-specific batches — further LINQ may force client evaluation; use `.AsEnumerable()` explicitly when you accept that cost (SqlQueryRepository.GetLowStockReport). - Tracking note: Without `.AsNoTracking()`, composed queries still return tracked entities if the raw SQL includes key columns (`ProductId`) — fine for updates, expensive for read-only APIs. - Scenario to avoid composition: Legacy proc that already applies complex filtering — pushing extra `.Where` in LINQ hides that all rows cross the wire; fix the proc or use a composable SELECT instead. - Debugging: Log `ToQueryString()` (EF Core 5+) or enable SQL logging to verify whether `ORDER BY` appears in the final batch — do not assume from LINQ syntax alone. Production takeaway: Composable raw SQL is a strength of EF Core over hand-rolled ADO.NET for paginated/filtered reports — but only on plain SELECTs. Pair with Q2's `EXEC` trap: composition rules are the dividing line. See Program.cs QUICK REFERENCE — "When to stay on LINQ vs raw SQL."
