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

## Gotchas — EF Core Raw SQL & Stored Procedures (Interview Traps)

---

#### Gotcha 1. `FromSqlRaw` with string interpolation — SQL injection vulnerability

**Concepts**
- `FromSqlRaw` passes the string to the database without parameterization
- C# interpolated string `$"WHERE Id = {id}"` in `FromSqlRaw` is injection risk
- `FromSqlInterpolated` converts interpolation holes to SQL parameters safely
- code review must flag all non-constant strings in `FromSqlRaw`
- `EF.Parameter(value)` for explicit parameterization in `FromSqlRaw`

**Answer**

`FromSqlRaw` passes the SQL string to the database as-is — interpolating a user-supplied value (`$"WHERE Name LIKE '%{term}%'"`) creates a critical SQL injection vulnerability. `FromSqlInterpolated` is the safe alternative: it converts each interpolation hole to a SQL parameter automatically. Always use `FromSqlInterpolated` for any SQL containing user-controlled values, and treat every `FromSqlRaw` call with a non-constant string as requiring explicit review for parameterization correctness.

---

#### Gotcha 2. `FromSqlRaw` result must be an entity type or keyless entity — no arbitrary shapes

**Concepts**
- `FromSqlRaw` maps columns to tracked entity properties
- result shape must match a registered entity type or keyless entity
- arbitrary DTO/anonymous type not directly supported by `FromSqlRaw`
- `context.Database.SqlQueryRaw<T>` (EF Core 8+) for arbitrary scalar types
- Dapper as the simpler alternative for arbitrary query shapes

**Answer**

`FromSqlRaw<T>` requires `T` to be a registered entity type or a keyless entity (`[Keyless]` or `HasNoKey()`) — it cannot map to arbitrary DTOs or anonymous types directly. Attempting to use `FromSqlRaw<ProductSummaryDto>` where `ProductSummaryDto` is not registered in the model throws `InvalidOperationException`. Use `context.Database.SqlQueryRaw<T>` (EF Core 8+) for scalar projections, or use Dapper's `QueryAsync<T>` which maps to any POCO by column name without requiring entity registration.

---

#### Gotcha 3. Raw SQL cannot be composed with `Where`/`OrderBy` unless result is `IQueryable<T>`

**Concepts**
- `FromSqlRaw` returns `IQueryable<T>` — LINQ operators can be appended
- composed query wraps raw SQL in a subquery
- inner raw SQL must not contain `ORDER BY` without `OFFSET-FETCH`
- `ExecuteSqlRaw` returns `int` and cannot be composed
- verify `ToQueryString()` to check how raw SQL and LINQ compose

**Answer**

`FromSqlRaw` returns `IQueryable<T>`, allowing LINQ operators like `.Where()` and `.OrderBy()` to be chained — EF Core wraps the raw SQL in a subquery and appends the LINQ clause. However, a raw SQL string with a top-level `ORDER BY` (without `OFFSET-FETCH`) inside a subquery causes a SQL syntax error in SQL Server. If the raw SQL needs composability with LINQ, do not include `ORDER BY` in the inner SQL; let the outer LINQ apply ordering. `ExecuteSqlRaw` returns `int` (rows affected) and cannot be composed with LINQ operators at all.

---

#### Gotcha 4. `ExecuteSqlRawAsync` bypasses change tracker — tracked entity values not updated

**Concepts**
- `ExecuteSqlRawAsync("UPDATE ...")` modifies the database directly
- tracked entities in the context still hold the old values
- `SaveChanges` with tracked modifications will overwrite the raw SQL update
- must reload tracked entities after `ExecuteSqlRawAsync` to reflect changes
- EF Core 7+ `ExecuteUpdateAsync` for set-based updates via LINQ

**Answer**

`context.Database.ExecuteSqlRawAsync("UPDATE Products SET Price = 0 WHERE IsDiscontinued = 1")` modifies rows in the database directly, bypassing the change tracker. Any `Product` entities already tracked by the context still hold the old `Price` values. If a subsequent `SaveChanges` call flushes tracked modifications, it can overwrite the raw SQL update with stale tracked values. After `ExecuteSqlRawAsync`, either reload affected entities with `ReloadAsync`, clear the change tracker, or avoid mixing raw SQL updates with tracked-entity modifications for the same rows.

---

#### Gotcha 5. Stored procedure output parameters not supported via `FromSqlRaw` — requires ADO.NET

**Concepts**
- `FromSqlRaw("EXEC dbo.Proc @p1 OUTPUT")` does not capture output parameters
- EF Core has no API for SP output parameter binding through `FromSqlRaw`
- raw `SqlParameter` with `Direction.Output` via `context.Database.GetDbConnection()`
- or use Dapper's `DynamicParameters` with output direction on the same connection
- `FromSqlRaw` for SP result set only; output params require lower-level access

**Answer**

`FromSqlRaw("EXEC dbo.GetOrderSummary @orderId, @total OUTPUT")` executes the procedure and maps the result set to entities, but EF Core has no mechanism to bind or read `OUTPUT` parameters — the `@total` value is inaccessible. To read stored procedure output parameters, use `context.Database.GetDbConnection()` to obtain the underlying `DbConnection` and execute a `SqlCommand` with `SqlParameter` of `Direction = ParameterDirection.Output`, or use Dapper with `DynamicParameters` on the same connection.

---

#### Gotcha 6. Raw SQL includes `ORDER BY` without `OFFSET-FETCH` inside a composed query — SQL syntax error

**Concepts**
- SQL Server requires `OFFSET-FETCH` with `ORDER BY` inside a subquery
- `FromSqlRaw` composing adds outer LINQ → raw SQL becomes inner subquery
- `ORDER BY` in inner subquery without `OFFSET-FETCH` is invalid SQL
- move `ORDER BY` to the outer LINQ: `.OrderBy(e => e.Name)`
- `ToQueryString()` reveals the composed SQL before execution

**Answer**

When `FromSqlRaw` is composed with LINQ operators (`.Where()`, `.OrderBy()`, `.Take()`), EF Core wraps the raw SQL as an inner subquery. SQL Server does not allow `ORDER BY` in a subquery without `OFFSET-FETCH`, so including `ORDER BY` in the raw SQL string causes a SQL syntax error at runtime. Remove `ORDER BY` from the raw SQL and add it as a LINQ operator: `.OrderBy(e => e.CreatedAt)` after `FromSqlRaw(...)`. Use `ToQueryString()` to inspect the full composed SQL before running in production.

---

#### Gotcha 7. `FromSqlRaw` does not load navigation properties — `Include` required for related data

**Concepts**
- `FromSqlRaw` maps column values to entity properties only
- no automatic navigation property loading from raw SQL result
- `FromSqlRaw(...).Include(e => e.Category)` for eager loading after raw SQL
- navigation loaded via separate EF query after materialization
- lazy loading after raw SQL: context must still be open

**Answer**

`FromSqlRaw` maps the SQL result set to entity properties but does not automatically load navigation properties. `product.Category` is null after `context.Products.FromSqlRaw(...)` unless `.Include(p => p.Category)` is chained. EF Core translates the `Include` into an additional join or split query wrapping the raw SQL. Always add required `Include` clauses after `FromSqlRaw` the same way you would after a LINQ query — the navigation loading behavior is identical.

---

#### Gotcha 8. Raw SQL bypasses EF Core concurrency token check — lost update possible

**Concepts**
- EF Core appends `WHERE RowVersion = @orig` for tracked entity UPDATE
- `ExecuteSqlRawAsync("UPDATE Products SET Price = @p WHERE Id = @id")` omits concurrency check
- concurrent write between read and raw SQL update not detected
- raw SQL with optimistic concurrency must manually check `@@ROWCOUNT`
- mixing tracked entity updates and raw SQL on same rows creates concurrency gaps

**Answer**

When an entity has a `[Timestamp]` concurrency token, EF Core's `SaveChanges` appends `WHERE RowVersion = @original` to the UPDATE to detect concurrent modifications. `ExecuteSqlRawAsync` bypasses EF Core entirely and does not apply this concurrency check — two concurrent raw SQL updates on the same row will silently overwrite each other. When using raw SQL for updates on entities with concurrency tokens, manually include the RowVersion check in the WHERE clause and check `@@ROWCOUNT` to detect conflicts.

---

#### Gotcha 9. `FromSqlInterpolated` parameter names auto-generated — debugging composed queries is harder

**Concepts**
- `FromSqlInterpolated` generates parameter names `p0`, `p1`, `p2` etc.
- generated SQL is less readable in profiler and logs
- `@p0 = 5, @p1 = 'Electronics'` with no named context
- `FromSqlRaw` with named `SqlParameter` objects for named parameters
- `TagWith("GetProductsByCategory")` to annotate the query in SQL logs

**Answer**

`FromSqlInterpolated` converts interpolation holes to parameters named `@p0`, `@p1`, etc. When examining this query in SQL Profiler or EF Core logs, the parameter names carry no semantic meaning — `@p0 = 5` is less debuggable than `@categoryId = 5`. For complex raw SQL queries that will be profiled heavily, use `FromSqlRaw` with explicitly named `SqlParameter` objects to produce readable parameter names. Add `.TagWith("GetProductsByCategory")` to annotate the query with a comment visible in SQL logs to correlate the SQL back to the code.

---

#### Gotcha 10. Database-generated column values from SP not reflected in tracked entity after `FromSqlRaw`

**Concepts**
- SP may update columns (triggers, defaults, computed) not in the result set
- tracked entity holds stale property values after SP execution
- EF Core does not re-read all columns after `FromSqlRaw`
- `ReloadAsync()` on the tracked entity to refresh from database
- `AsNoTracking()` on `FromSqlRaw` for read-only use to avoid stale tracking

**Answer**

A stored procedure executed via `FromSqlRaw` may modify columns (via triggers, updated-by timestamps, or side-effect updates) that are not in the returned result set. EF Core maps the returned columns to the tracked entity but does not re-read columns absent from the result. The tracked entity holds stale values for those columns after the SP runs. Call `await context.Entry(entity).ReloadAsync()` after `FromSqlRaw` execution to refresh all properties from the current database state, or use `AsNoTracking()` on the `FromSqlRaw` query for read-only scenarios to avoid tracking altogether.

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
