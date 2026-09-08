# Chapter 02. Queries, Execute & Async Methods — Interview Q&A
> Back to [Dapper Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Chapter 02. Queries, Execute & Async Methods](#chapter-02-queries-execute--async-methods)
  - [Q1. What is the difference between Dapper's `Query` and `Execute` methods?](#q1-what-is-the-difference-between-dappers-query-and-execute-methods)
  - [Q2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?](#q2-when-do-you-use-queryt-versus-queryfirstordefaultt)
  - [Q3. What does `Execute` return, and when is it used?](#q3-what-does-execute-return-and-when-is-it-used)
  - [Q4. Does Dapper open the connection if it is closed when you call `Query`?](#q4-does-dapper-open-the-connection-if-it-is-closed-when-you-call-query)
  - [Q5. What is the difference between buffered and unbuffered queries in Dapper?](#q5-what-is-the-difference-between-buffered-and-unbuffered-queries-in-dapper)
  - [Q6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?](#q6-what-async-methods-does-dapper-provide-queryasync-executeasync-etc)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 02. Queries, Execute & Async Methods

---

## Q1. What is the difference between Dapper's `Query` and `Execute` methods?

**Concepts**
- Query materializes result rows
- Execute maps to ExecuteNonQuery
- integer rows-affected return from Execute
- SELECT vs DML use case distinction
- shared parameter and transaction support

**Answer**

`Query` and its generic overloads run statements that return rows and materialize them into objects or scalars. `Execute` runs statements that do not return a result set — inserts, updates, deletes — and returns an integer count of rows affected, matching ADO.NET's `ExecuteNonQuery` semantics. `Query<T>(sql, param)` yields zero or more mapped instances of `T`, while `Execute(sql, param)` gives back a row count for diagnostic or validation purposes. Both accept the same parameter objects and honor the connection's current transaction, so the choice is purely about whether your statement produces rows.

---

## Q2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?

**Concepts**
- zero-or-many row expectation for Query
- single-row optional lookup for QueryFirstOrDefault
- IEnumerable deferred vs immediate execution
- QuerySingle strict uniqueness requirement
- absence as valid outcome

**Answer**

I use `Query<T>` when I expect zero or many rows and want a sequence to iterate or materialize into a list. I use `QueryFirstOrDefault<T>` when I want at most one row — it returns the first match or `default(T)` if the result set is empty. `Query<T>` returns `IEnumerable<T>` (deferred unless buffered) and suits filters, catalog queries, and any endpoint returning a collection. `QueryFirstOrDefault<T>` executes immediately for single-row lookups such as "get user by id when the row may not exist." `QuerySingle<T>` is a stricter variant that throws if zero or more than one row exists — use it only when exactly one row is guaranteed by a unique key. Reaching for `QuerySingle` on optional lookups is a common bug; prefer `QueryFirstOrDefault` when absence is a valid outcome.

---

## Q3. What does `Execute` return, and when is it used?

**Concepts**
- rows-affected integer return
- ExecuteNonQuery equivalence
- identity retrieval via OUTPUT clause
- transaction participation
- ExecuteAsync preference in ASP.NET Core

**Answer**

`Execute` returns an integer count of the rows affected by the command. I use it for inserts, updates, deletes, and any non-query stored procedure that does not return a grid. A return value of zero means no rows matched — often expected for idempotent deletes, but sometimes a bug for update statements where a matching row was assumed. `Execute` does not return generated keys; use `ExecuteScalar` or an `OUTPUT` clause in the SQL when you need the new identity value. It participates in an ambient transaction when the connection has an active `IDbTransaction`, and `ExecuteAsync` is preferred in ASP.NET Core request handlers so threads are not blocked during database I/O.

---

## Q4. Does Dapper open the connection if it is closed when you call `Query`?

**Concepts**
- ConnectionState auto-check before execute
- pool-transparent open behavior
- external connection lifetime management
- Dapper non-close guarantee after execute
- await using disposal pattern

**Answer**

Yes — Dapper checks `ConnectionState` before executing and calls `Open()` (or `OpenAsync()` for async variants) if the connection is closed. This does not bypass ADO.NET connection pooling; `SqlConnection.Open` still draws from the pool as configured. If you manage connection lifetime externally — for example, sharing one open connection across multiple Dapper calls inside a transaction — you can open once and reuse without interference. Dapper does not close the connection after the call finishes. Best practice is to use `await using var connection = new SqlConnection(...)` so disposal handles returning the connection to the pool reliably even when exceptions occur.

---

## Q5. What is the difference between buffered and unbuffered queries in Dapper?

**Concepts**
- buffered default reads entire result set into memory
- unbuffered streams through live IDataReader
- connection lifetime during enumeration
- deferred enumeration risk after disposal
- safe ToList() materialization pattern

**Answer**

By default Dapper buffers query results, reading the entire result set into memory before returning and then closing the reader. The returned `IEnumerable<T>` is safe to enumerate multiple times and the connection can be disposed immediately after the call. With `buffered: false`, Dapper streams rows through a live `IDataReader` as the caller enumerates, using significantly less memory for very large result sets but keeping the connection open until enumeration completes. The danger with unbuffered queries is returning a deferred `IEnumerable<T>` from a method after the `using` block has closed the connection — enumeration then fails with "connection is closed" errors. Always materialize with `.ToList()` inside the connection scope unless you deliberately stream within that scope and control the full enumeration lifetime.

---

## Q6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?

**Concepts**
- async API mirroring sync surface
- CancellationToken support
- provider async ADO.NET delegation
- thread release during I/O
- QueryAsync buffered and unbuffered modes

**Answer**

Dapper mirrors its synchronous API with async counterparts that accept optional `CancellationToken` values: `QueryAsync`, `QueryFirstAsync`, `QueryFirstOrDefaultAsync`, `QuerySingleAsync`, `QuerySingleOrDefaultAsync`, `ExecuteAsync`, `ExecuteScalarAsync`, and `QueryMultipleAsync`. They delegate to the underlying provider's async ADO.NET methods, so database I/O releases the thread to the pool while waiting. I prefer async variants in ASP.NET Core so request threads are not blocked during network round-trips. `QueryAsync` still supports both buffered and unbuffered modes — the deferred-enumeration rules apply equally. Cancellation tokens propagate to the provider when supported, allowing a client disconnect or request timeout to abort long-running queries at the database level. `ExecuteScalarAsync` returns the first column of the first row, which is useful for aggregates and identity retrieval.

---

## Gotchas — Dapper Queries, Execute & Async Methods (Interview Traps)

---

#### Gotcha 1. `QueryAsync` without `await` — fire-and-forget discards results

**Concepts**
- unawaited `Task<IEnumerable<T>>` runs but result is never consumed
- `QueryAsync` returning `IEnumerable<T>` requires `await` to get data
- compiler warning for unawaited task may be suppressed accidentally
- exceptions from unawaited tasks are swallowed silently
- always `await` and materialize with `.ToList()` inside connection scope

**Answer**

Calling `QueryAsync<T>` without `await` returns a `Task<IEnumerable<T>>` that runs in the background and whose result is never consumed — the caller gets an empty or null variable and no exception, making this a silent data-loss bug. The compiler typically warns about unawaited tasks, but if the variable is discarded with `_` or the warning is suppressed, it compiles cleanly. Always `await` and immediately materialize: `var rows = (await conn.QueryAsync<Product>(sql, param)).ToList()` inside the `using` connection scope.

---

#### Gotcha 2. `Execute` returning 0 rows — not an exception, may be ignored

**Concepts**
- `Execute`/`ExecuteAsync` returns `int` rows affected
- 0 rows affected is a valid SQL outcome, not an error
- optimistic update with no matching WHERE — silently does nothing
- caller must check `rowsAffected == 1` for critical updates
- stale primary key or soft-deleted row as common cause

**Answer**

`ExecuteAsync` returns the number of rows affected — if the UPDATE or DELETE WHERE clause matches no rows, it returns 0 without throwing. Code that calls `ExecuteAsync` and ignores the return value silently performs a no-op update, which can mean a stale key, a soft-deleted row, or an incorrect WHERE clause. Always check `var affected = await conn.ExecuteAsync(sql, param); if (affected == 0) throw new NotFoundException()` for critical writes where "0 rows" is a business error.

---

#### Gotcha 3. `QueryFirst` throws on empty result — use `QueryFirstOrDefault` for optional rows

**Concepts**
- `QueryFirst<T>` throws `InvalidOperationException` on empty result
- `QueryFirstOrDefault<T>` returns `default(T)` on empty
- `QuerySingle<T>` throws on 0 or 2+ rows
- `QuerySingleOrDefault<T>` returns `default(T)` on 0, throws on 2+
- correct choice driven by business rule, not personal preference

**Answer**

`QueryFirst<T>` throws `InvalidOperationException` when the query returns no rows — it asserts that at least one row must exist. `QueryFirstOrDefault<T>` returns `default(T)` (null for reference types) when no rows match, making it appropriate for lookups where absence is a valid outcome. Using `QueryFirst` where absence is expected converts a legitimate "not found" condition into a hard exception. Choose by the business rule: required existence → `QueryFirst`; optional existence → `QueryFirstOrDefault`; exactly one → `QuerySingle`/`QuerySingleOrDefault`.

---

#### Gotcha 4. Buffered vs unbuffered `Query` — unbuffered keeps connection open during iteration

**Concepts**
- buffered (default): all rows loaded into `List<T>` before returning
- unbuffered (`buffered: false`): rows streamed lazily, connection stays open
- caller must iterate to completion for connection to be released
- abandoned enumeration leaves connection in pool-occupied state
- use unbuffered only with `await using` and complete enumeration

**Answer**

By default, Dapper's `Query<T>` buffers all rows into a `List<T>` before returning — the connection is used only for the query duration and can be disposed immediately after. With `buffered: false`, Dapper returns a lazy `IEnumerable<T>` that streams rows from the live reader — the connection remains open until enumeration completes. If the caller abandons the enumeration early without disposing the connection, the pool slot is leaked. Use unbuffered only inside a `using` connection scope and ensure complete enumeration or explicit connection disposal on any exit path.

---

#### Gotcha 5. `ExecuteScalarAsync<T>` silently returns `default(T)` on null

**Concepts**
- `ExecuteScalarAsync<T>` casts result to `T`
- returns `default(T)` when result is `null` or `DBNull`
- `int` `default` is 0 — silently wrong for missing aggregate
- `T?` as the return type to distinguish null from zero
- explicit null check before using the returned value

**Answer**

`ExecuteScalarAsync<int>` returns `0` when the scalar result is `null` or `DBNull.Value`, which is `default(int)`. A `MAX()` on an empty table returns `DBNull` from SQL Server — mapped to `0` silently, which may be a valid or invalid value in your domain. Use `ExecuteScalarAsync<int?>()` to get a nullable result that is `null` when no rows exist and `0` only when the actual aggregate value is zero. Always handle the null case explicitly rather than relying on the default value to indicate "no data."

---

#### Gotcha 6. Multiple result sets — `Query()` only reads the first; use `QueryMultiple`

**Concepts**
- `Query<T>` consumes only the first result set from a multi-set SP
- subsequent result sets are silently ignored
- `QueryMultiple` returns `GridReader` for consuming each set in order
- grid reader must be consumed in the same order as SP emits sets
- `using` on `GridReader` to dispose after all grids read

**Answer**

If a stored procedure or batch emits multiple `SELECT` result sets, calling `QueryAsync<T>` reads only the first set and silently ignores all subsequent ones. To consume multiple result sets, use `conn.QueryMultipleAsync(sql, param)` which returns a `GridReader`. Call `await grid.ReadAsync<T>()` in the same order as the procedure emits its SELECTs. Always wrap `GridReader` in `using`, and never call `Read` on a grid position that has already been consumed.

---

#### Gotcha 7. Async methods without `CancellationToken` — client disconnect cannot abort query

**Concepts**
- Dapper async methods accept optional `CancellationToken`
- token propagated to underlying ADO.NET `ExecuteReaderAsync`
- SQL Server receives TDS Attention packet on cancellation
- `HttpContext.RequestAborted` as natural cancellation source
- un-cancelled queries waste server resources after client disconnect

**Answer**

All Dapper async methods (`QueryAsync`, `ExecuteAsync`, `ExecuteScalarAsync`) accept an optional `CancellationToken` that propagates to the underlying ADO.NET command and sends a TDS Attention packet to SQL Server when fired. Omitting the token means a client disconnect or request timeout cannot abort the in-progress query — the database continues executing while the HTTP response is already gone. Pass `HttpContext.RequestAborted` as the `cancellationToken` argument on every Dapper async call in controller or service methods.

---

#### Gotcha 8. `Connection Timeout` exceeded when borrowing a pool slot — not a query timeout

**Concepts**
- "timeout expired obtaining connection from pool" is a pool timeout
- `Connection Timeout` in connection string (default 15s)
- separate from `command.CommandTimeout` (query execution timeout)
- pool starvation from connection leaks, not slow queries
- diagnostic: measure pool wait time vs query execution time

**Answer**

The "timeout expired obtaining connection from pool" error is a connection pool wait timeout (controlled by `Connection Timeout` in the connection string, default 15 seconds), not a query execution timeout (`Command Timeout`). It fires when all pool slots are occupied and no slot becomes available within the wait window. This is almost always caused by connection leaks (undisposed connections) rather than slow queries — diagnose by checking for undisposed connections, measuring pool wait time with `SqlClientEventSource`, and reviewing connection lifetimes in the code.

---

#### Gotcha 9. `CommandTimeout` on Dapper calls — must be passed explicitly per call

**Concepts**
- Dapper uses `IDbCommand.CommandTimeout` default (30s) when not specified
- no global Dapper `CommandTimeout` setting exists
- passed as optional `commandTimeout` parameter per method call
- long-running reports and batch operations need explicit override
- `SqlMapper.Settings.CommandTimeout` as global default in some versions

**Answer**

Dapper uses the default `IDbCommand.CommandTimeout` (30 seconds) when no `commandTimeout` is specified. Unlike EF Core, which lets you configure a global command timeout on `DbContext`, Dapper requires explicit per-call timeout overrides: `conn.QueryAsync<T>(sql, param, commandTimeout: 300)`. For long-running reports or batch operations called via Dapper, always pass `commandTimeout` explicitly — relying on the 30-second default will cause `SqlException: Execution Timeout Expired` on any query that takes more than half a minute.

---

#### Gotcha 10. `SET NOCOUNT ON` in stored procedure causes `Execute` to return -1

**Concepts**
- `SET NOCOUNT ON` suppresses row-count TDS messages
- `ExecuteAsync` returns `-1` when `NOCOUNT ON` is active
- rows-affected check breaks for stored procedures with `NOCOUNT`
- use output parameter for row count when SP uses `NOCOUNT ON`
- common DBA practice that breaks ADO.NET row-count assumptions

**Answer**

`SET NOCOUNT ON` inside a stored procedure suppresses the TDS "done in proc" row-count messages that normally populate the rows-affected return value. When a Dapper `ExecuteAsync` calls such a procedure, it returns `-1` regardless of how many rows were actually affected. Code that checks `if (affected == 0)` to detect missing rows will fail to fire, silently treating a successful operation with `NOCOUNT ON` as a "not found" condition. Use a stored procedure output parameter or a `SELECT @@ROWCOUNT` result set to reliably capture the row count when `NOCOUNT ON` is active.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) A catalog API exposes "get product by category." A teammate ships this repository method. What breaks in production as the catalog grows, and what Dapper API would you use instead?

```csharp
public Product GetByCategory(SqlConnection connection, int categoryId)
{
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE CategoryId = @categoryId;
        """;
    return connection.QuerySingle<Product>(sql, new { categoryId });
}
```

**Concepts**
- QuerySingle strict one-row assertion
- non-unique CategoryId filter
- one-to-many relationship contract mismatch
- Query<T> for collection return
- QueryFirst as duplicate-hiding alternative

**Answer**

`QuerySingle` throws `InvalidOperationException` when more than one row matches — it is correct for primary key lookups but wrong for a `CategoryId` filter once any category has two or more products. As the catalog grows and categories accumulate products, every call to this method for a populated category crashes with a 500 error. The return type `Product` also signals a broken contract: a category-to-products relationship is one-to-many, so the method should return `IReadOnlyList<Product>`.

The fix is to change the return type and use `Query<Product>` (or `QueryAsync` plus `.ToList()`). If the business rule genuinely requires at most one product per category, enforce it with a unique constraint in the database and keep `QuerySingle` — or use `QuerySingleOrDefault` when absence is acceptable. `QueryFirst` with `TOP 1 ORDER BY` is a valid choice only when you explicitly want an arbitrary representative row and document that intent.

---

## Q2. (R) An export endpoint needs to stream millions of product rows with minimal memory. A developer refactors the repository to use `buffered: false`. The bug passes unit tests that mock `IEnumerable<Product>`. What fails at runtime?

```csharp
public IEnumerable<Product> StreamAllActive()
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();
    return connection.Query<Product>(sql, buffered: false);
}

// Controller
public IActionResult Export()
{
    var rows = _repository.StreamAllActive();
    return Ok(rows.Select(p => MapToDto(p)));
}
```

**Concepts**
- unbuffered query live SqlDataReader dependency
- using block disposal before enumeration
- deferred LINQ over deferred Dapper sequence
- mock IEnumerable hiding real connection coupling
- streaming within vs across connection lifetime

**Answer**

`buffered: false` keeps a live `SqlDataReader` tied to the connection. The `using var connection` block in `StreamAllActive` disposes the connection when the method returns, before the controller enumerates the deferred sequence. When the JSON serializer later calls `MoveNext` on the LINQ chain, the underlying reader is closed — producing `InvalidOperationException` or "Invalid attempt to call Read when reader is closed." Mocks return an in-memory list that never opens a real reader, so tests stay green while the production path fails.

For typical API list endpoints, the fix is to materialize inside the method with `.ToList()` and return `IReadOnlyList<Product>`. For genuine large-result streaming, use `buffered: false` with `IAsyncEnumerable<Product>` and `await foreach`, keeping the connection open throughout the entire enumeration by managing it at the same level as the consumer so the connection is not disposed until the async stream completes.

---

## Q3. (R) A health-check endpoint reports inventory count. Identify compile-time, runtime, and scalability issues.

```csharp
[HttpGet("inventory/count")]
public IActionResult GetActiveProductCount()
{
    using var connection = new SqlConnection(_configuration.GetConnectionString("AdoNetTutorial"));
    connection.Open();
    int count = connection.ExecuteScalarAsync<int>(
        "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;").Result;
    return Ok(new { count });
}
```

**Concepts**
- sync-over-async deadlock risk
- thread-pool starvation under load
- async action method requirement
- CancellationToken for cooperative abort
- ExecuteScalarAsync proper await pattern

**Answer**

Calling `.Result` on `ExecuteScalarAsync` blocks the calling thread while waiting for database I/O — this is sync-over-async. In ASP.NET Core under load, blocking the thread pool with synchronous waits can cause thread starvation: orchestrators polling a health check endpoint repeatedly can exhaust available threads before requests for application work can be serviced. There is also no cancellation support, so a slow database call continues running even after the client disconnects or the health check times out.

The fix is to make the action async, `await` the Dapper call, and pass the cancellation token:

```csharp
[HttpGet("inventory/count")]
public async Task<IActionResult> GetActiveProductCount(CancellationToken ct)
{
    using var connection = new SqlConnection(_configuration.GetConnectionString("AdoNetTutorial"));
    var cmd = new CommandDefinition(
        "SELECT COUNT(*) FROM dbo.Products WHERE DiscontinuedDate IS NULL;",
        cancellationToken: ct);
    int count = await connection.ExecuteScalarAsync<int>(cmd);
    return Ok(new { count });
}
```

Prefer injecting a scoped repository or service rather than opening `SqlConnection` inline in the controller for better testability and separation of concerns.

---

## Q4. (M) For each snippet, name the correct Dapper method and explain what goes wrong if shipped as written.

```csharp
// A — needs rows affected after UPDATE
int changed = (int)connection.ExecuteScalar(
    "UPDATE dbo.Products SET StockQuantity = @qty WHERE ProductId = @id;",
    new { id = 1, qty = 10 });

// B — needs a single aggregate value
decimal avg = connection.Query<decimal>(
    "SELECT AVG(UnitPrice) FROM dbo.Products WHERE DiscontinuedDate IS NULL;")
    .First();

// C — needs to confirm INSERT succeeded (1 row)
var result = connection.Query<int>(
    "INSERT INTO dbo.Products ... VALUES ...;",
    new { ... });
bool inserted = result.Any();
```

**Concepts**
- Execute for DML rows-affected return
- ExecuteScalar<T> for single-cell aggregates
- Query<T> for SELECT result sets only
- INSERT returns no rows to map
- method-to-SQL-semantics alignment

**Answer**

Each snippet uses the wrong Dapper entry point. For snippet A, `ExecuteScalar` returns the first column of the first row from a result set — a plain `UPDATE` produces no result set, so the return is often `null` and casting to `int` throws a null reference exception. The correct method is `Execute`, which returns the integer count of rows affected directly.

For snippet B, `Query<decimal>` materializes a result set and `.First()` throws `InvalidOperationException` on an empty table. Single aggregate values belong on the scalar path — `ExecuteScalar<decimal>` returns the value directly and handles the empty case correctly.

For snippet C, a bare `INSERT` statement returns no rows. `Query<int>` expects a SELECT result set mapped to `int` — since the insert returns nothing, `result` is always an empty sequence, so `inserted` is always `false` even when the row was successfully inserted. The correct approach is `Execute` and comparing `rowsAffected == 1`.

The rule: `Execute` is for DML row counts; `ExecuteScalar<T>` is for one cell (COUNT, AVG, `SCOPE_IDENTITY()`); `Query<T>` is for many mapped rows from SELECT statements.

---

## Q5. (P) An ASP.NET Core API uses scoped `SqlConnection` per request. A repository method returns `IEnumerable<Product>` from `QueryAsync`. Under load, callers see `InvalidOperationException` ("There is already an open DataReader…"). Explain the mechanism and show the production-safe pattern.

```csharp
public async Task<IEnumerable<Product>> GetAllActiveAsync(SqlConnection connection)
{
    return await connection.QueryAsync<Product>(sql);
}
```

**Concepts**
- deferred IEnumerable from QueryAsync
- open DataReader holding connection
- MARS disabled by default
- materialization before second command
- IReadOnlyList return type enforcement

**Answer**

`QueryAsync` returns a deferred sequence backed by an open `SqlDataReader` on the shared scoped connection. When the caller receives the `IEnumerable<Product>` and does not enumerate it before issuing another Dapper call on the same connection — a second query, a logging interceptor, or middleware — SQL Server rejects the overlapping reader because MARS is disabled by default. Mocks return in-memory lists with no real reader, hiding the coupling entirely in tests.

The production-safe fix is to materialize before returning, so the reader is closed before the method gives up control:

```csharp
public async Task<IReadOnlyList<Product>> GetAllActiveAsync(SqlConnection connection)
{
    IEnumerable<Product> rows = await connection.QueryAsync<Product>(sql);
    return rows.ToList();
}
```

Returning `IReadOnlyList<Product>` instead of `IEnumerable<Product>` signals to callers that the sequence is already materialized. Use `buffered: false` with `IAsyncEnumerable` only when the same code path owns the connection until enumeration completes — not when returning across layer boundaries.

---

## Q6. (D) A tutorial method uses `MAX(ProductId) + 1` for ID generation and `ExecuteScalar<int>` to return the new ID. A teammate says "works in dev, ship it." Two API instances insert concurrently. What breaks, and what pattern replaces both the ID generation and the Dapper call?

```csharp
const string sql = """
    DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
    INSERT INTO dbo.Products (ProductId, ...) VALUES (@NextId, ...);
    SELECT @NextId;
    """;
int newId = connection.ExecuteScalar<int>(sql, param);
```

**Concepts**
- concurrent MAX+1 race condition
- primary key violation under parallel inserts
- IDENTITY column for database-assigned IDs
- SCOPE_IDENTITY() for post-insert retrieval
- ExecuteScalar correct use for identity return

**Answer**

Two concurrent requests can both read the same `MAX(ProductId)` before either commits, then both attempt to insert the same `ProductId`. One request fails with a primary key violation — or worse, under weaker isolation the insert silently produces duplicate IDs. The `MAX + 1` pattern is not safe under any concurrent workload.

`ExecuteScalarAsync` is the right Dapper call for returning a single identity value from an insert, but the ID generation must be delegated to the database. The fix is to use an `IDENTITY` column on `ProductId` and let SQL Server allocate IDs atomically, then return the new value with `SCOPE_IDENTITY()`:

```csharp
const string sql = """
    INSERT INTO dbo.Products (ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
    VALUES (@productName, @unitPrice, @stockQuantity, NULL);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
    """;
int newId = await connection.ExecuteScalarAsync<int>(sql, param);
```

Prefer `SCOPE_IDENTITY()` over `@@IDENTITY` because it is scope-safe when triggers exist. Wrap the insert plus any follow-up DML in an explicit `IDbTransaction` when multiple statements must succeed atomically.

---

## Q7. (P) A batch job must insert a product and read the new `ProductId` in one round trip, then update a related audit row — all-or-nothing. The developer runs two separate Dapper calls on the same open connection without an explicit transaction. When does that silently corrupt data, and how do you wire `Execute`/`ExecuteScalar` correctly with `IDbTransaction`?

**Concepts**
- implicit autocommit between separate calls
- IDbTransaction shared across Dapper calls
- atomic insert-then-update pattern
- rollback on partial failure
- connection reuse with transaction parameter

**Answer**

Without an explicit transaction, each Dapper call commits independently under autocommit semantics. If the insert succeeds and the audit update then fails — due to a constraint violation, a network error, or a concurrent modification — the product row is permanently committed with no corresponding audit entry, leaving the data in an inconsistent state that no retry can detect or fix retroactively.

The fix is to begin a transaction before either call and pass it to both Dapper methods via the `transaction` parameter:

```csharp
using IDbConnection connection = new SqlConnection(_connectionString);
connection.Open();
using IDbTransaction tx = connection.BeginTransaction();
try
{
    int newId = connection.ExecuteScalar<int>(insertSql, insertParams, tx);
    connection.Execute(auditSql, new { ProductId = newId }, tx);
    tx.Commit();
    return newId;
}
catch
{
    tx.Rollback();
    throw;
}
```

Both calls share the same open connection and transaction, so if either fails the rollback undoes both. The Dapper `transaction` overload parameter is available on all `Execute`, `ExecuteScalar`, and `Query` variants.
