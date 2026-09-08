# 03. .NET Data Access

ADO.NET, Dapper, and Entity Framework Core — connections, commands, ORMs, migrations, and performance.

## Topics

| # | Topic | Questions | Q&A File |
|---|-------|-----------|----------|
| 01 | 01. ADO.NET | 111 | [README.md](./01.%20ADO.NET/README.md) |
| 02 | 02. Dapper | 53 | [README.md](./02.%20Dapper/README.md) |
| 03 | 03. Entity Framework Core | 146 | [README.md](./03.%20Entity%20Framework%20Core/README.md) |

---

> Each Q&A file has a **Table of Contents** at the top linking to every question.
> Files that include scenario-based Karat questions have a **Scenario-Based Questions** section at the bottom.

---

# 03. .NET Data Access — Cross-Topic Interview Q&A
> Back to [Full Stack .NET Core](../README.md)

## Subfolders

| # | Topic | Summary |
|---|-------|---------|
| 01 | [ADO.NET](01.%20ADO.NET/INTERVIEW_QA.md) | Low-level SQL: connections, commands, readers, DataSet |
| 02 | [Dapper](02.%20Dapper/INTERVIEW_QA.md) | Micro-ORM: SQL-first thin mapping over ADO.NET |
| 03 | [Entity Framework Core](03.%20Entity%20Framework%20Core/INTERVIEW_QA.md) | Full ORM: LINQ, change tracking, migrations |

---

## Table of Contents

- [CQ1. Choosing ADO.NET, Dapper, or EF Core: decision framework and coexistence.](#cq1-choosing-adonet-dapper-or-ef-core-decision-framework-and-coexistence)
- [CQ2. N+1 query problem: how each layer hides or exposes it.](#cq2-n1-query-problem-how-each-layer-hides-or-exposes-it)
- [CQ3. Connection pool exhaustion: how ADO.NET, Dapper, and EF Core all share the same pool.](#cq3-connection-pool-exhaustion-how-adonet-dapper-and-ef-core-all-share-the-same-pool)
- [CQ4. Testing data access: unit and integration strategies across all three layers.](#cq4-testing-data-access-unit-and-integration-strategies-across-all-three-layers)
- [CQ5. Production scenario: EF Core and Dapper used without a shared transaction cause partial writes.](#cq5-production-scenario-ef-core-and-dapper-used-without-a-shared-transaction-cause-partial-writes)
- [CQ6. Logging SQL queries: tracing executed SQL across ADO.NET, Dapper, and EF Core.](#cq6-logging-sql-queries-tracing-executed-sql-across-adonet-dapper-and-ef-core)
- [CQ7. Pagination: OFFSET-FETCH in ADO.NET/Dapper vs Skip/Take in EF Core, and the keyset gotcha.](#cq7-pagination-offset-fetch-in-adonetdapper-vs-skiptake-in-ef-core-and-the-keyset-gotcha)
- [CQ8. Bulk insert performance: SqlBulkCopy vs Dapper multi-row INSERT vs EF Core batch.](#cq8-bulk-insert-performance-sqlbulkcopy-vs-dapper-multi-row-insert-vs-ef-core-batch)
- [CQ9. Optimistic concurrency: EF Core's rowversion vs manual implementation in Dapper and ADO.NET.](#cq9-optimistic-concurrency-ef-cores-rowversion-vs-manual-implementation-in-dapper-and-adonet)
- [CQ10. Repository pattern: when to abstract data access and what each layer sacrifices.](#cq10-repository-pattern-when-to-abstract-data-access-and-what-each-layer-sacrifices)
- [CQ11. Lazy loading anti-pattern in EF Core vs Dapper's explicit N+1 philosophy.](#cq11-lazy-loading-anti-pattern-in-ef-core-vs-dappers-explicit-n1-philosophy)
- [CQ12. Schema migration: EF Core migrations vs Flyway/DbUp alongside Dapper and ADO.NET.](#cq12-schema-migration-ef-core-migrations-vs-flywaydbup-alongside-dapper-and-adonet)
- [CQ13. Long-running query cancellation: threading CancellationToken through all three layers.](#cq13-long-running-query-cancellation-threading-cancellationtoken-through-all-three-layers)

---

## CQ1. Choosing ADO.NET, Dapper, or EF Core: decision framework and coexistence

**Concepts**
- ADO.NET: maximum control, maximum boilerplate, zero magic
- Dapper: thin mapping over hand-written SQL, near-ADO.NET throughput
- EF Core: full ORM — LINQ translation, change tracking, migrations
- all three coexist in one application, sharing the same connection string and connection
- team SQL fluency and schema ownership as the primary decision drivers

**Answer**

All three sit on top of ADO.NET drivers and ultimately issue SQL; they differ only in how much is automated. Raw ADO.NET gives complete control: you write every SQL string, parameterize every command, and map every reader column to a property — suitable for stored-procedure-heavy codebases, bulk inserts via `SqlBulkCopy`, or performance paths needing `CommandBehavior` control. Dapper adds `Query<T>`, `Execute`, and `QueryMultiple` extension methods on `IDbConnection` that automate result-to-POCO mapping while keeping your SQL visible, delivering near-ADO.NET throughput with a fraction of the mapping code — the right choice when SQL fluency is high, the schema is stable, and you want explicit queries. EF Core adds the LINQ-to-SQL engine, change tracker, identity map, loading strategies, concurrency tokens, and migration tooling; it maximises developer productivity for standard CRUD on a well-modelled entity graph but requires understanding the query translator and change tracker to avoid N+1 bugs and unnecessary roundtrips. In production, all three coexist routinely: EF Core for command-side entity CRUD, Dapper for read-model and reporting queries, raw ADO.NET for true bulk operations. All can share the same connection string and even the same open `DbConnection` and `DbTransaction`.

---

## CQ2. N+1 query problem: how each layer hides or exposes it

**Concepts**
- N+1: one parent query + one child query per parent row = N+1 total round-trips
- ADO.NET: explicit — N+1 only if you write a query-per-row loop
- Dapper: `QueryFirstOrDefault` inside a loop is the classic Dapper N+1 trap
- EF Core: lazy loading issues child queries silently behind navigation property access
- `Include` / `ThenInclude` and `AsSplitQuery` as EF Core remedies

**Answer**

N+1 is always caused by the same thing: one SQL query per element of a collection rather than one query for all of them. In raw ADO.NET there is nowhere to hide — you write every loop and every command explicitly, so N+1 only appears if you deliberately issue a new `SqlCommand` per row. In Dapper the same logic applies; your SQL is always visible, but the trap is calling `conn.QueryFirstOrDefault<Customer>("SELECT ... WHERE Id = @id", new { id = order.CustomerId })` inside a foreach — each call is a round-trip. The fix is a single `Query<Order>` with a JOIN or `QueryMultiple` for separate result sets, both returning all data in one or two round-trips. EF Core is where N+1 becomes most dangerous because lazy loading — enabled by default before EF Core 3.0, opt-in since — issues the child queries silently: `foreach (var o in db.Orders) { Console.WriteLine(o.Customer.Name); }` fires one query for all orders and then one query per order for its customer with no warning at the call site. The fix is `.Include(o => o.Customer)` which generates a single `JOIN`, or `AsSplitQuery()` when the Cartesian product from multiple Includes is too wide. `AsNoTracking()` eliminates change-tracker overhead but does not prevent N+1; only explicit loading strategies do.

---

## CQ3. Connection pool exhaustion: how ADO.NET, Dapper, and EF Core all share the same pool

**Concepts**
- single ADO.NET pool keyed per unique connection string
- all three layers borrow from and return to the same pool
- unreturned connections: missing `using`, unclosed readers, singleton `DbContext`
- default pool max 100; exhaustion throws `InvalidOperationException` after 15 s
- `AddDbContextPool` for EF Core context reuse without the singleton anti-pattern

**Answer**

ADO.NET's SQL Server driver maintains one connection pool per unique connection string, and all three layers draw from that pool regardless of abstraction layer. A raw ADO.NET `SqlConnection.Open()`, a Dapper `conn.Query<T>()`, and EF Core's `db.Products.ToList()` all borrow a connection and must return it via `Close()` / `Dispose()` — which means `using` or `await using` blocks throughout the stack. Three common mistakes simultaneously exhaust the pool: forgetting `await using` on a `SqlDataReader` in ADO.NET or Dapper code (the connection stays out of the pool until the GC finalizer, which may be seconds or minutes later); registering `DbContext` as a singleton in ASP.NET Core DI (every request shares one connection that is never cleanly released between concurrent operations, causing data races and `DbConcurrencyException`); and EF Core lazy loading inside a loop that keeps the outer connection open while issuing per-row child queries. When the pool reaches its maximum (default 100 connections), the next `Open()` call waits up to 15 seconds and then throws `InvalidOperationException: "Timeout expired... The timeout period elapsed prior to obtaining a connection from the pool."` Use `services.AddDbContextPool<AppDbContext>` (EF Core 2.0+) to reuse `DbContext` instances across requests and reduce physical connection churn without the concurrency risks of the singleton anti-pattern.

---

## CQ4. Testing data access: unit and integration strategies across all three layers

**Concepts**
- unit test: in-process fake data, no real DB — tests logic, not SQL
- integration test: real DB engine, full SQL path — tests queries and schema
- ADO.NET / Dapper: mock `IDbConnection` or SQLite in-process
- EF Core: `UseInMemoryDatabase` for logic, `UseSqlite(":memory:")` for query fidelity
- Respawn / Docker test containers for integration test database reset

**Answer**

Each layer needs a different testing approach because each has a different abstraction surface. For raw ADO.NET and Dapper, the cleanest unit-test path is programming against `IDbConnection` rather than `SqlConnection` directly: inject the interface into the repository and in tests pass an in-memory SQLite connection with the test schema pre-applied — Dapper works with any `IDbConnection`, so `new SqliteConnection("DataSource=:memory:")` gives near-real query execution in a fast in-process database. For EF Core, `UseInMemoryDatabase` is convenient for unit tests that validate business logic via the change tracker but does not execute real SQL, does not enforce FK constraints, and cannot run raw SQL calls — use it only for entity-graph logic tests. `UseSqlite(":memory:")` with `db.Database.EnsureCreated()` is the better choice for repository-level tests because it runs EF Core's LINQ-to-SQL translation against a real query engine in-process and catches translated-query bugs that InMemory misses. Full integration tests against SQL Server (or a Docker container using `Testcontainers`) remain essential for validating stored procedures, index usage, transaction isolation levels, and concurrency tokens. Respawn resets the database between test runs by truncating tables without recreating the schema, keeping integration test suites fast.

---

## CQ5. Production scenario: EF Core and Dapper used without a shared transaction cause partial writes

**Concepts**
- EF Core `SaveChanges`: implicit transaction on its own connection
- Dapper `Execute`: uses a separate connection unless explicitly shared
- two independent transactions = no atomicity guarantee across both
- `IDbContextTransaction.GetDbTransaction()` to bridge EF Core and Dapper
- partial write: order saved, audit entry missing — silent data inconsistency

**Answer**

A team builds an order-placement flow: EF Core saves the `Order` entity via `SaveChangesAsync`, then Dapper inserts an audit log row via `conn.ExecuteAsync`. In development everything works. In production the audit insert occasionally fails — a transient network error, a constraint violation, a deadlock retry — and the order is saved but the audit entry is missing. The root cause is that EF Core's `SaveChanges` opened its own connection and committed an implicit transaction before Dapper ran, and Dapper opened a second independent connection with no link to that commit. The audit failure rolls back only itself; the order is already final. The fix: obtain an explicit transaction from EF Core first, retrieve the underlying ADO.NET objects, and pass both to Dapper so they share one connection and one transaction. `await using var tx = await db.Database.BeginTransactionAsync(ct)` — then `var conn = db.Database.GetDbConnection()` — then `await conn.ExecuteAsync(auditSql, auditParam, transaction: tx.GetDbTransaction())` — then `await db.SaveChangesAsync(ct)` — then `await tx.CommitAsync(ct)`. A failure at any step rolls back both the order save and the audit insert atomically. This pattern generalises to any combination of EF Core, Dapper, and raw ADO.NET commands that must be atomic within the same request.

---

## CQ6. Logging SQL queries: tracing executed SQL across ADO.NET, Dapper, and EF Core

**Concepts**
- ADO.NET: no built-in SQL logging; `DiagnosticSource` / `EventSource` listener or decorator wrapper over `SqlCommand`
- Dapper: no built-in logging; MiniProfiler `ProfiledDbConnection` wraps `IDbConnection`; custom delegating connection intercepts every `Execute` call
- EF Core: `LogTo` / `ILoggerFactory` integration logs generated SQL to any sink
- `EnableSensitiveDataLogging`: includes parameter values in the log — safe in development, a compliance violation in production

**Answer**

Each layer offers a different logging surface. Raw ADO.NET has no built-in query log: to observe what SQL is executing you either write a decorator that wraps `SqlCommand`, logs `CommandText` and parameters before delegating to the real implementation, or you attach a `DiagnosticSource` listener using the `Microsoft.Data.SqlClient` activity source (`"SqlClientDiagnosticListener"`) which fires events around every command execution without changing any data-layer code. Dapper has no logging hook of its own because it delegates immediately to ADO.NET; the idiomatic approach is to give Dapper a wrapped `IDbConnection` — MiniProfiler ships a `ProfiledDbConnection` that does exactly this — so every `Query<T>` and `Execute` call is timed and captured before hitting the real driver. EF Core has first-class logging integration: `optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)` routes generated SQL to any sink, and plugging in an `ILoggerFactory` wires it into the application's structured logging pipeline (Serilog, NLog, etc.) automatically. `EnableSensitiveDataLogging()` additionally includes parameter values in the log output, which is essential during debugging but must never be enabled in a production environment because parameter values can contain PII, passwords, or payment card numbers — gate it with `app.Environment.IsDevelopment()`. Without that guard, a misconfigured deployment writes customer data to application logs, creating an audit and compliance problem that is harder to remediate than the original debugging need.

---

## CQ7. Pagination: OFFSET-FETCH in ADO.NET/Dapper vs Skip/Take in EF Core, and the keyset gotcha

**Concepts**
- `OFFSET n ROWS FETCH NEXT m ROWS ONLY` (SQL Server 2012+): the underlying clause all three layers produce
- EF Core: `.Skip(n).Take(m)` translates to OFFSET-FETCH; `.OrderBy()` is required or EF Core throws
- Offset pagination at scale: `OFFSET 100000` forces the engine to scan and discard 100,000 leading rows
- Keyset (cursor) pagination: `WHERE Id > @lastSeenId ORDER BY Id` — O(log n) index seek, no skip cost
- EF Core keyset: `.Where(o => o.Id > lastSeenId).OrderBy(o => o.Id).Take(size)` translates to a range seek

**Answer**

All three layers ultimately produce the same SQL Server pagination clause. In ADO.NET or Dapper you write it directly: `ORDER BY CreatedAt DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY`, passing `@skip = (page - 1) * pageSize`. In EF Core, `.OrderByDescending(o => o.CreatedAt).Skip(skip).Take(take)` translates to the identical clause, but EF Core enforces an `ORDER BY` before emitting `OFFSET` — calling `.Skip()` without `.OrderBy()` throws `InvalidOperationException`. The critical gotcha is scale: offset pagination forces SQL Server to scan and discard the first `skip` rows on every request. At page 1 that is fast; at page 1,000 with `pageSize = 20` the engine discards 19,980 rows before returning 20, and both CPU and I/O grow linearly with page depth even though the returned set stays constant. Keyset pagination eliminates this by filtering on a unique indexed column: `WHERE Id > @lastSeenId ORDER BY Id` returns the next page in O(log n) time regardless of how deep into the data set the caller is. In EF Core this is expressed as `.Where(o => o.Id > lastSeenId).OrderBy(o => o.Id).Take(pageSize)`, which translates to an efficient index seek. The trade-off is that keyset pagination cannot jump to an arbitrary page number — it is strictly "next page" navigation — so the choice depends on the UI: infinite-scroll and "next" UIs benefit from keyset; traditional page-number grids with arbitrary jumps require offset pagination with a capped maximum page enforcement to prevent runaway scans.

---

## CQ8. Bulk insert performance: SqlBulkCopy vs Dapper multi-row INSERT vs EF Core batch

**Concepts**
- `SqlBulkCopy`: streams rows over TDS bulk-load protocol, minimal transaction logging, fastest at scale
- Dapper list-parameter `ExecuteAsync`: one parameterized INSERT per item pipelined in one round-trip; SQL Server 2,100-parameter limit caps practical row count
- EF Core `SaveChangesAsync` batching: groups inserts into multi-row `INSERT … VALUES` statements (EF Core 7+); keys populated back on entities
- `EFCore.BulkExtensions`: third-party package wrapping `SqlBulkCopy` with EF Core entity mapping
- Rule of thumb: any approach under 500 rows; Dapper or EF Core batch to ~10k; `SqlBulkCopy` above that

**Answer**

The right bulk insert strategy depends on row count and whether you need the EF Core change tracker's side-effects such as auto-generated key values being populated back onto entity instances. For small inserts under a few hundred rows, EF Core's batched `SaveChangesAsync` is the most convenient: EF Core 7+ groups tracked-entity inserts into multi-row `INSERT … VALUES` statements in a single round-trip, and primary keys are reflected back. Dapper's `ExecuteAsync("INSERT INTO Orders …", listOfParams)` iterates the parameter list and issues one parameterized INSERT per item in a single pipelined batch — fast for a few thousand rows, but the SQL Server 2,100-parameter limit per batch caps this to roughly 2,000 rows for a table with a handful of columns before you must split the list manually. `SqlBulkCopy` bypasses the standard TDS command path entirely, streaming rows using SQL Server's bulk-load protocol which uses minimal transaction logging and can load millions of rows in seconds; the trade-off is that it ignores the EF Core change tracker entirely, does not fire DML triggers by default, and requires mapping to a `DataTable` or `IDataReader` by hand. Third-party libraries such as `EFCore.BulkExtensions` wrap `SqlBulkCopy` behind a fluent API that respects EF Core's entity model, giving `BulkInsert` and `BulkMerge` semantics with key population at near-`SqlBulkCopy` performance. The practical decision: reach for `SqlBulkCopy` or a wrapper of it once row counts exceed 10,000 or import latency becomes observable, and use EF Core or Dapper for everyday CRUD where convenience and change-tracker integration outweigh raw throughput.

---

## CQ9. Optimistic concurrency: EF Core's rowversion vs manual implementation in Dapper and ADO.NET

**Concepts**
- Optimistic concurrency: assume no conflict, detect and reject stale writes at commit time rather than holding locks
- EF Core `[Timestamp]` / `byte[] RowVersion`: maps to SQL Server `rowversion`; EF Core appends `WHERE RowVersion = @orig` to UPDATE automatically
- `DbUpdateConcurrencyException`: thrown when UPDATE matches 0 rows due to intervening write
- Dapper / ADO.NET: `UPDATE … WHERE Id = @id AND RowVer = @orig`; check `rowsAffected == 1` manually
- Gotcha: Dapper returns rows-affected as a plain integer — a silent zero means a lost-update unless the caller checks

**Answer**

Optimistic concurrency lets multiple users read the same row simultaneously and only detects a conflict at write time, rather than holding a database lock for the full duration of a user interaction. In EF Core the idiom is to add a `byte[] RowVersion` property annotated with `[Timestamp]` — or configured via `Property(e => e.RowVersion).IsRowVersion()` in Fluent API — which maps to SQL Server's `rowversion` type that auto-increments on every UPDATE to that row. When EF Core saves an entity, it appends `WHERE RowVersion = @orig` to the UPDATE statement. If another process modified the row in the meantime, the UPDATE matches 0 rows and EF Core throws `DbUpdateConcurrencyException`, giving the caller a chance to reload the current database values, apply a merge strategy, and retry. In Dapper and ADO.NET there is no automatic detection — you write the WHERE clause yourself: `UPDATE Orders SET Status = @status WHERE Id = @id AND RowVer = @origRowVer`, then inspect the integer rows-affected count returned by `ExecuteAsync` or `ExecuteNonQueryAsync`. If the count is 0, a concurrent write won the race and you must handle the conflict explicitly. The critical gotcha is that a zero rows-affected value from Dapper or ADO.NET is not an exception — it is a silent success return that callers frequently forget to check, resulting in a silent lost-update where one writer's changes are silently discarded. Always assert `rowsAffected == 1` immediately after the command and throw a domain exception on failure, reproducing the same fail-fast semantics that EF Core's `DbUpdateConcurrencyException` provides automatically.

---

## CQ10. Repository pattern: when to abstract data access and what each layer sacrifices

**Concepts**
- `IRepository<T>`: decouples business logic from data-access technology; enables fake or stub implementations in unit tests
- `IQueryable<T>` leak: exposing `IQueryable<T>` from a repository interface couples callers to EF Core's LINQ engine
- Dapper repositories: return `IEnumerable<T>` — no deferred execution, no post-return LINQ composition
- ADO.NET mock: inject `IDbConnection`; substitute an in-memory SQLite connection in tests for real SQL execution
- Anti-pattern: thin CRUD APIs where the repository adds a pass-through layer with no extractable logic

**Answer**

The repository pattern places an `IRepository<T>` interface between business-layer code and data access, so business logic calls `_orderRepo.GetByCustomerAsync(customerId)` instead of constructing EF Core queries directly. The main benefit is testability: the business layer can be unit-tested by injecting a fake implementation that returns in-memory lists, with no database involved. Each data-access layer sacrifices something different when wrapped in a repository. EF Core repositories face the `IQueryable<T>` dilemma: if the interface exposes `IQueryable<T>` as a return type, callers can compose arbitrary `.Where()` and `.Include()` calls, keeping the API flexible but leaking EF Core's LINQ engine through the abstraction — you cannot substitute a Dapper or ADO.NET implementation behind that interface without building a LINQ provider, which defeats the point. Repositories that return concrete result types or `IEnumerable<T>` are properly technology-agnostic but force all filtering into named query methods, which proliferates quickly for complex filter combinations. Dapper repositories naturally return `IEnumerable<T>` because Dapper has no deferred-execution layer — the query executes at the `QueryAsync` call site — so the `IQueryable` tension does not arise, but dynamic filter requirements push toward either raw dynamic SQL or a specification-object pattern. ADO.NET repositories work best when `IDbConnection` is injected, allowing tests to substitute an in-memory SQLite connection for real SQL execution without an external database. Avoid the repository pattern entirely in thin CRUD APIs where the abstraction adds a pass-through layer over a `DbContext` or connection with no extractable business logic that would actually benefit from faking.

---

## CQ11. Lazy loading anti-pattern in EF Core vs Dapper's explicit N+1 philosophy

**Concepts**
- EF Core lazy loading: navigation-property access on an unloaded proxy triggers a hidden `SELECT` per entity
- Opt-in since EF Core 3.0: requires `UseLazyLoadingProxies()`, `Microsoft.EntityFrameworkCore.Proxies`, and `virtual` navigation properties
- Dapper: no navigation properties, no proxy infrastructure — every related-row query is a deliberate line of code
- Making N+1 visible in EF Core: `LogTo` logs each emitted query; MiniProfiler highlights duplicate-query patterns
- Recommended default: keep lazy loading disabled; use `.Include()` / `.AsSplitQuery()` explicitly

**Answer**

EF Core's lazy loading works by replacing entity instances with proxy subclasses that intercept navigation-property access: the first time you read `order.Customer` on an unloaded proxy, EF Core silently fires `SELECT * FROM Customers WHERE Id = @id` against the still-open `DbContext`. This is convenient in exploratory interactive code but catastrophic in a tight loop — iterating 200 orders and reading `.Customer.Name` on each fires 201 queries with no warning at the call site unless `LogTo` or MiniProfiler is active. Before EF Core 3.0, lazy loading was enabled by default; from 3.0 onward it is strictly opt-in via `UseLazyLoadingProxies()` from the `Microsoft.EntityFrameworkCore.Proxies` package plus `virtual` navigation properties, so a team must make a conscious decision to enable it. Dapper's design philosophy is the inverse: because Dapper has no entity graph, proxy infrastructure, or navigation properties, there is simply no mechanism for a hidden query to occur — every related-row fetch is an explicit `QueryAsync` call visible in the source. N+1 in Dapper is always your code and always readable. This explicitness is what many teams value on read-heavy query paths: the performance profile of a Dapper method is entirely determined by the SQL written inside it, with no hidden behaviour. The practical recommendation for EF Core is to leave lazy loading disabled and instead use `.Include()` for small predictable entity graphs, `.AsSplitQuery()` when multiple `Include` paths cause Cartesian-product row explosion, and projection queries (`.Select(o => new OrderDto { … })`) when only a subset of columns is needed, since projections bypass the change tracker and proxy machinery entirely.

---

## CQ12. Schema migration: EF Core migrations vs Flyway/DbUp alongside Dapper and ADO.NET

**Concepts**
- EF Core migrations: code-first C# `Up/Down` methods generated from model snapshots; `__EFMigrationsHistory` table tracks applied migrations
- Flyway / DbUp: SQL-script-first, sequential versioned `.sql` files, technology-agnostic, works with any data-access layer
- Expand/contract (parallel-change) pattern: add nullable column → backfill → enforce NOT NULL across separate deployments
- Production race: code deployed before or after a migration can leave the schema and application out of sync
- Never run `context.Database.MigrateAsync()` from `Program.cs` in a multi-pod production deployment

**Answer**

EF Core's migration system generates C# `Up()` and `Down()` methods by diffing successive model snapshots, applies them in order, and records each in the `__EFMigrationsHistory` table. It is tightly coupled to EF Core's model: stored procedures, custom indexes, and computed columns created outside of EF Core's Fluent API do not appear in the snapshot and will not be managed by the migration tooling, creating a split-management problem in mixed codebases. Flyway and DbUp take the opposite approach: all schema changes are plain versioned `.sql` files checked into source control, applied in sequence, and tracked in their own history table. This is technology-agnostic — the same migration pipeline works whether the application layer is ADO.NET, Dapper, EF Core, or all three — and DBAs can review exact SQL before any production apply, which EF Core's generated C# is harder to audit at a glance. The critical production gotcha in either system is the race between code deployment and migration execution. If the new application code that reads column `Orders.TrackingCode` is deployed before the `ALTER TABLE` that adds it is applied, every request that touches that column throws until the migration completes. The expand/contract pattern avoids this: deploy one adds the column as nullable with no application code change; deploy two updates and reads the column; deploy three adds the NOT NULL constraint or drops the old column. Finally, never call `context.Database.MigrateAsync()` from `Program.cs` in a multi-instance production deployment — every pod races to apply migrations simultaneously, causing deadlocks on the history table and unpredictable partial-apply states.

---

## CQ13. Long-running query cancellation: threading CancellationToken through all three layers

**Concepts**
- ADO.NET: `SqlCommand.ExecuteReaderAsync(ct)` — token cancellation sends TDS Attention packet to the server
- Dapper: `QueryAsync(sql, param, cancellationToken: ct)` — passed through to the underlying `SqlCommand`
- EF Core: `ToListAsync(ct)`, `SaveChangesAsync(ct)` — EF Core wires `ct` into every generated command
- Server-side Attention packet: asks SQL Server to abort the query; server rolls back its current transaction unit
- Gotcha: Dapper unbuffered (`buffered: false`) streams rows — cancelling mid-stream leaves the connection occupied unless the reader is explicitly disposed

**Answer**

All three layers expose `CancellationToken` on every async method, and all three ultimately pass the token down to the ADO.NET `SqlCommand.ExecuteReaderAsync(ct)` call at the bottom of the stack. When the token fires — because an ASP.NET Core request was aborted, a timeout policy triggered, or the user navigated away — the ADO.NET driver sends a TDS Attention packet to SQL Server asking it to abort the currently executing query. The server-side effect depends on what the query was doing: a plain `SELECT` is simply stopped and its partial result discarded; a DML statement inside a transaction is rolled back on the server, leaving the database in a consistent state. In ADO.NET, the cancellation arrives as an `OperationCanceledException` on the awaiting `ExecuteReaderAsync` call, which you catch and rethrow or handle. In Dapper, `conn.QueryAsync<T>(sql, param, cancellationToken: ct)` threads the token through; the default buffered mode reads the entire result set into a `List<T>` before returning, so cancellation during the in-progress read leaves the connection cleanly closed once the Attention is processed and the exception propagates. In unbuffered mode (`buffered: false`), the returned `IAsyncEnumerable<T>` or `IEnumerable<T>` streams rows lazily — abandoning the enumeration after cancellation without disposing the reader keeps the connection occupied until the GC finalizer, reducing pool availability under high load; always wrap unbuffered results in `await using` and enumerate to completion or explicit abandonment. EF Core is the safest: `ToListAsync(ct)` is always buffered and EF Core owns the connection lifecycle, so a mid-query cancellation reliably cleans up the reader and returns the connection to the pool.
