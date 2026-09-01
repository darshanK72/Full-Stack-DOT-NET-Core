# 02. Dapper — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 01. Introduction to Dapper](#chapter-01-introduction-to-dapper)
  - [Q1. What is Dapper?](#q1-what-is-dapper)
  - [Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?](#q2-what-type-of-library-is-dapper-orm-micro-orm-or-something-else)
  - [Q3. What problem does Dapper solve compared to raw ADO.NET?](#q3-what-problem-does-dapper-solve-compared-to-raw-adonet)
  - [Q4. What problem does Dapper solve compared to Entity Framework Core?](#q4-what-problem-does-dapper-solve-compared-to-entity-framework-core)
  - [Q5. When would you choose Dapper over EF Core?](#q5-when-would-you-choose-dapper-over-ef-core)
  - [Q6. When would you choose EF Core over Dapper?](#q6-when-would-you-choose-ef-core-over-dapper)
  - [Q7. What are the main advantages and limitations of Dapper?](#q7-what-are-the-main-advantages-and-limitations-of-dapper)
  - [Q8. Does Dapper generate SQL for you?](#q8-does-dapper-generate-sql-for-you)

- [Chapter 02. Queries, Execute & Async Methods](#chapter-02-queries-execute-async-methods)
  - [Q1. What is the difference between Dapper's `Query` and `Execute` methods?](#q1-what-is-the-difference-between-dappers-query-and-execute-methods)
  - [Q2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?](#q2-when-do-you-use-queryt-versus-queryfirstordefaultt)
  - [Q3. What does `Execute` return, and when is it used?](#q3-what-does-execute-return-and-when-is-it-used)
  - [Q4. Does Dapper open the connection if it is closed when you call `Query`?](#q4-does-dapper-open-the-connection-if-it-is-closed-when-you-call-query)
  - [Q5. What is the difference between buffered and unbuffered queries in Dapper?](#q5-what-is-the-difference-between-buffered-and-unbuffered-queries-in-dapper)
  - [Q6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?](#q6-what-async-methods-does-dapper-provide-queryasync-executeasync-etc)

- [Chapter 03. Parameters, Stored Procedures & QueryMultiple](#chapter-03-parameters-stored-procedures-querymultiple)
  - [Q1. How do you pass parameters to a Dapper query?](#q1-how-do-you-pass-parameters-to-a-dapper-query)
  - [Q2. How does Dapper prevent SQL injection?](#q2-how-does-dapper-prevent-sql-injection)
  - [Q3. How do you call a stored procedure with Dapper?](#q3-how-do-you-call-a-stored-procedure-with-dapper)
  - [Q4. What is `QueryMultiple`, and when is it used?](#q4-what-is-querymultiple-and-when-is-it-used)
  - [Q5. How do you read multiple result sets from `QueryMultiple`?](#q5-how-do-you-read-multiple-result-sets-from-querymultiple)
  - [Q6. When would you prefer `QueryMultiple` over separate round-trips?](#q6-when-would-you-prefer-querymultiple-over-separate-round-trips)

- [Chapter 04. Mapping, Multi-Mapping & Advanced Patterns](#chapter-04-mapping-multi-mapping-advanced-patterns)
  - [Q1. How does Dapper map column names to property names by default?](#q1-how-does-dapper-map-column-names-to-property-names-by-default)
  - [Q2. What happens when column names do not match property names?](#q2-what-happens-when-column-names-do-not-match-property-names)
  - [Q3. What is multi-mapping in Dapper?](#q3-what-is-multi-mapping-in-dapper)
  - [Q4. What is the `splitOn` parameter in multi-mapping?](#q4-what-is-the-spliton-parameter-in-multi-mapping)
  - [Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?](#q5-how-does-dapper-handle-nested-object-graphs-compared-to-ef-core-include)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to Dapper

---

## Q1. What is Dapper?

**Concepts**
- IDbConnection extension method surface
- ADO.NET abstraction layer
- result-row-to-POCO materialization
- micro-ORM positioning
- provider-agnostic design

**Answer**

Dapper is a lightweight .NET library that extends `IDbConnection` with extension methods for executing SQL and mapping result rows to plain C# objects. It sits directly on top of ADO.NET, which means it works with any ADO.NET provider — SQL Server, PostgreSQL, MySQL, SQLite, and others — without locking you into a specific database. Stack Overflow created and distributes it as the `Dapper` NuGet package. Its key design point is that you retain full control over SQL while it removes the manual `SqlDataReader` boilerplate of creating commands, indexing columns, and copying values property by property. It maps results to classes, structs, value tuples, and dynamic types with minimal configuration, and it intentionally leaves connection lifetime management to you — you open, pool, and dispose connections through the underlying provider.

---

## Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?

**Concepts**
- micro-ORM classification
- object-relational mapping scope boundary
- absence of change tracking and migrations
- contrast with full ORM
- SQL visibility as a design goal

**Answer**

Dapper is a micro-ORM — it maps result rows to objects but deliberately stops short of modeling relationships, tracking entity state across a unit of work, or managing schema migrations. You write the SQL; Dapper handles parameter binding and materialization. A full ORM like Entity Framework Core goes further by generating SQL from LINQ expressions and tracking which properties changed so it can emit targeted updates. Dapper avoids those features by design, keeping its behavior fast and predictable for hand-tuned queries. Teams often describe it as an object mapper rather than a complete persistence framework, and they reach for it when SQL visibility and throughput matter more than convention-based modeling.

---

## Q3. What problem does Dapper solve compared to raw ADO.NET?

**Concepts**
- SqlDataReader column-indexing elimination
- parameter binding from anonymous objects
- command and reader boilerplate reduction
- async surface area
- full ADO.NET feature access retained

**Answer**

Raw ADO.NET requires repetitive ceremony to create commands, add parameters, open readers, and copy column values into object properties row by row. Dapper collapses all of that into a single call like `connection.Query<Product>(sql, param)` while still using your SQL and your connection. It eliminates manual `SqlDataReader` indexing and null-checking for every column, and it binds anonymous objects, `DynamicParameters`, or expando objects to SQL parameters automatically. Async methods like `QueryAsync` and `ExecuteAsync` expose the same concise surface area. Underneath, you retain full access to transactions, stored procedures, and provider-specific features — Dapper simply removes the plumbing.

---

## Q4. What problem does Dapper solve compared to Entity Framework Core?

**Concepts**
- change tracker overhead on reads
- SQL transparency for tuning
- LINQ translation cost
- read-only result set efficiency
- schema management responsibility

**Answer**

Entity Framework Core adds layers — LINQ translation, change tracking, relationship fix-up, and migration infrastructure — that are unnecessary for simple, performance-sensitive read paths. Dapper executes the exact SQL you provide and returns mapped objects with near-ADO.NET overhead. Because you always see the SQL sent to the database, tuning indexes and diagnosing slow queries is straightforward. Removing the change tracker also reduces memory use significantly on large read-only result sets, since EF Core snapshots every property for potential dirty detection. The trade-off is that you lose EF Core's automation for joins, cascade updates, and relationship navigation — every query and data-modification statement becomes your responsibility.

---

## Q5. When would you choose Dapper over EF Core?

**Concepts**
- hot-path throughput requirements
- stored procedure integration
- inefficient LINQ translation scenarios
- read-heavy DTO-only APIs
- DBA-owned SQL management

**Answer**

I choose Dapper when I need hand-written SQL, maximum read throughput, or tight integration with legacy stored procedures where EF Core's LINQ translation adds little value. Hot paths where every round-trip and every millisecond are profiled and fixed at the SQL level benefit from Dapper's minimal overhead. Teams with strong database administrators who prefer stored procedures and tuned statements in source control also tend to prefer Dapper. Scenarios where EF Core would generate inefficient joins, cartesian explosions, or simply cannot translate certain expressions are exactly where Dapper's explicit SQL shines. Read-heavy APIs that only need flat DTOs — no entity graphs, no change tracking — are another natural fit.

---

## Q6. When would you choose EF Core over Dapper?

**Concepts**
- migration-driven schema evolution
- LINQ composability
- change tracking unit of work
- relationship navigation and eager loading
- provider-agnostic query translation

**Answer**

I choose Entity Framework Core when I want convention-based mapping, LINQ composability, automated migrations, and change tracking for typical CRUD workflows. Greenfield applications where C# entity classes should drive schema evolution through migrations are exactly where EF Core excels. Complex object graphs with relationships, eager loading, and automatic fix-up are far easier to manage with EF Core's navigation properties than with hand-written joins. When `SaveChanges` unit-of-work semantics and optimistic concurrency tokens simplify the code compared to writing `UPDATE` SQL per operation, EF Core is the better choice. Provider-agnostic LINQ — writing one query that EF Core translates to SQL Server, PostgreSQL, or SQLite — also avoids maintaining database-specific SQL dialects.

---

## Q7. What are the main advantages and limitations of Dapper?

**Concepts**
- benchmark performance advantage
- minimal API surface
- absence of dirty-entity detection
- manual relationship shaping requirement
- schema drift risk

**Answer**

Dapper's main advantage is speed: it is consistently among the fastest .NET mappers in benchmarks because it avoids the heavy metadata, proxy generation, and change-tracking layers that full ORMs carry. Its API is intentionally small — you learn `Query`, `Execute`, and how to pass parameters, rather than a complete ORM stack. On the limitation side, there is no built-in mechanism to detect dirty entities or batch updates without writing SQL yourself. Multi-table object graphs require manual joins, multi-mapping overloads, or multiple queries that you assemble in code. Schema drift is also not caught at compile time the way a strongly configured EF Core model can signal mismatches through failed migrations or model validation.

---

## Q8. Does Dapper generate SQL for you?

**Concepts**
- explicit SQL requirement
- no LINQ provider or expression tree translator
- Dapper.Contrib optional CRUD helpers
- predictable SQL as a design principle
- INSERT/UPDATE/DELETE require explicit text

**Answer**

Dapper never generates SQL. You supply the complete statement as a string or the name of a stored procedure, and Dapper binds parameters, executes the command through ADO.NET, and maps the result set to your types — nothing more. Inserts, updates, and deletes require explicit `INSERT`, `UPDATE`, or `DELETE` statements. There is no LINQ provider or expression tree translator in Dapper itself. Third-party extensions like Dapper.Contrib offer optional CRUD helpers that emit fixed SQL templates, but even those do not perform dynamic LINQ translation. This is a deliberate design choice: predictable, visible SQL is a feature that distinguishes Dapper from full ORMs.

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

## Chapter 03. Parameters, Stored Procedures & QueryMultiple

---

## Q1. How do you pass parameters to a Dapper query?

**Concepts**
- anonymous object parameter binding
- DynamicParameters for output and typed params
- case-insensitive name matching
- @-prefixed SQL placeholders
- property-to-parameter alignment

**Answer**

I pass parameters as the second argument to any Dapper method using an anonymous object, a `DynamicParameters` bag, a `Dictionary<string, object>`, or any object whose public properties match `@Name` placeholders in the SQL. Dapper maps property names to SQL parameter names case-insensitively. An anonymous object like `new { Id = 42, Name = "Widget" }` binds `@Id` and `@Name` without any additional configuration. `DynamicParameters` supports output parameters, table-valued parameters, and explicit database types for cases where the anonymous object pattern is insufficient. Property names must align with the SQL placeholder names exactly — `UserId` maps to `@UserId`, not `@user_id`, unless you alias the column or use a custom convention.

---

## Q2. How does Dapper prevent SQL injection?

**Concepts**
- ADO.NET parameterized execution
- user values separated from SQL text
- @-placeholder binding safety
- string interpolation as injection vector
- DynamicParameters also parameterizes

**Answer**

Dapper prevents SQL injection by sending user values as ADO.NET parameters separate from the SQL text, so input is never interpreted as executable SQL syntax by the database engine. As long as you use `@placeholders` with a parameter object and do not concatenate user input into the SQL string, injection is prevented — `"SELECT * FROM Users WHERE Email = @Email", new { Email = userInput }` is safe. The unsafe pattern is `$"SELECT * FROM Users WHERE Email = '{userInput}'"` — Dapper cannot protect against injected strings because it never sees user input embedded that way; it only parameterizes the values you pass in the param argument. `DynamicParameters.Add("Email", value)` still produces a proper parameter even when building SQL dynamically with a fixed structure. Stored procedure names should always be fixed string literals; only parameter values should come from user input.

---

## Q3. How do you call a stored procedure with Dapper?

**Concepts**
- CommandType.StoredProcedure flag
- procedure name as SQL argument
- DynamicParameters for output and return values
- Execute for non-query procedures
- result type matching from procedure output

**Answer**

I set `commandType: CommandType.StoredProcedure` and pass the procedure name as the SQL argument, with parameters bound the same way as ad hoc queries. For example: `connection.Query<Product>("usp_GetProductsByCategory", new { CategoryId = 5 }, commandType: CommandType.StoredProcedure)`. Output and return-value parameters require `DynamicParameters` with `ParameterDirection.Output` or `ParameterDirection.ReturnValue`. `Execute` and `ExecuteAsync` work for non-query procedures that do not return rowsets. The result shape must still match the mapped type's properties — Dapper uses the column names from the procedure's result set to bind, so the usual naming rules apply.

---

## Q4. What is `QueryMultiple`, and when is it used?

**Concepts**
- multi-result-set batch execution
- GridReader sequential consumption
- single round-trip for related datasets
- network latency reduction
- prompt GridReader disposal

**Answer**

`QueryMultiple` executes one batch or stored procedure that returns multiple result grids and exposes them through a `GridReader`. I use it when a single round-trip should return related datasets — for example, a header row plus detail lines — rather than two separate queries. It reduces network latency by combining multiple `SELECT` statements or a multi-result procedure into one database call. It returns `SqlMapper.GridReader`, which I hold open with `using var multi = connection.QueryMultiple(...)` while reading each set in order. Result sets are consumed sequentially — you cannot skip ahead — and I dispose the `GridReader` promptly to release the underlying reader and make the connection available for reuse.

---

## Q5. How do you read multiple result sets from `QueryMultiple`?

**Concepts**
- sequential Read calls per result set
- forward-only GridReader consumption
- result-set order dependency
- materialization before GridReader disposal
- ReadAsync equivalent

**Answer**

After `QueryMultiple`, I call `Read<T>()`, `ReadFirst<T>()`, or `ReadFirstOrDefault<T>()` on the `GridReader` once per result set, in the order the database returns them. The first `multi.Read<Order>()` consumes the first result set; the second `multi.Read<OrderLine>()` consumes the second. A mismatch between read order and SQL result order causes wrong-type mapping or empty sequences — the reader is forward-only and there is no way to rewind. I materialize each `Read` with `.ToList()` if I need the data after disposing the `GridReader`. The async equivalent is `QueryMultipleAsync` followed by `ReadAsync<T>()`.

---

## Q6. When would you prefer `QueryMultiple` over separate round-trips?

**Concepts**
- latency amplification on high-latency links
- always-needed related datasets
- parallel independent queries as alternative
- large second result set streaming consideration
- optional vs required data trade-off

**Answer**

I prefer `QueryMultiple` when two or more result sets are always needed together and combining them saves measurable latency, especially over high-latency network links to cloud databases. Dashboard endpoints that always load a summary alongside detail rows in one stored procedure are a natural fit. Separate round-trips are simpler when result sets are optional, independently cacheable, or can be fetched in parallel on different connections when the database supports concurrent execution. Very large second result sets may be better streamed in a dedicated query rather than held behind the first grid reader while it is being processed.

---

## Chapter 04. Mapping, Multi-Mapping & Advanced Patterns

---

## Q1. How does Dapper map column names to property names by default?

**Concepts**
- case-insensitive name equality matching
- public property and field targeting
- column order independence
- underscore naming not auto-mapped
- nullable value type NULL handling

**Answer**

Dapper matches result column names to public properties and fields on the target type using case-insensitive name equality. The first column value maps to the property with the same name regardless of column order in the SELECT list, so `ProductName` and `productname` both map to a `ProductName` property. Underscore naming such as `product_name` does not auto-map to `ProductName` without SQL aliases or custom type maps — the names must be equal after case normalization, not just similar. Value types map directly; nullable value types accept database NULL as `null`. No attributes are required unless you add a custom type map or use a mapping extension.

---

## Q2. What happens when column names do not match property names?

**Concepts**
- silent partial object population
- unmatched columns ignored
- unmatched properties at default values
- SQL alias as fix
- SetTypeMap custom mapping

**Answer**

Unmatched columns are silently ignored, and unmatched properties remain at their default values — `null`, `0`, or `false` — without any exception. This silent partial population is a common source of bugs because Dapper never throws for missing mappings; you get back objects that look valid but are missing data. The straightforward fix is a SQL alias: `SELECT FirstName AS GivenName` to match a `GivenName` property. For more systematic mapping, `SqlMapper.SetTypeMap` accepts a custom type map, or you can implement `ITypeHandler` for special conversions. Integration tests that assert specific property values are the most reliable way to catch name mismatches before production.

---

## Q3. What is multi-mapping in Dapper?

**Concepts**
- single-row multi-type decomposition
- join result graph assembly delegate
- Query generic type overloads
- splitOn boundary column
- N+1 avoidance pattern

**Answer**

Multi-mapping lets a single row with columns from a JOIN decompose into multiple nested objects in one `Query` call, using a delegate to assemble the parent-child graph. Overloads like `Query<Order, Customer, Order>` accept two or more type parameters plus a `Func` that combines them into the return type. A typical pattern is `SELECT o.*, c.* FROM Orders o JOIN Customers c ...` mapped to an `Order` instance with a nested `Customer` property. The split point between object types is controlled by the `splitOn` parameter. Dapper invokes the mapping function once per row, so you decide how to attach the child object to the parent. This approach avoids N+1 queries without forcing you to load flat DTOs and then manually group by parent ID in memory.

---

## Q4. What is the `splitOn` parameter in multi-mapping?

**Concepts**
- column boundary for object type split
- default "Id" splitOn behavior
- duplicate Id column aliasing requirement
- silent wrong-value risk on incorrect splitOn
- SELECT column order alignment

**Answer**

`splitOn` names the column in the result row where the next object type's properties begin. Dapper maps all columns before that column name to the first generic type, and all columns from that name onward to the second type, repeating for additional type parameters. The default is `"Id"` — this works when the second entity's first mapped column is literally named `Id` and appears in the SELECT after the first entity's columns. Duplicate `Id` columns in a join require explicit SQL aliases and a matching `splitOn` value such as `"CustomerId"` to point at the right split boundary. An incorrect `splitOn` silently maps NULL or wrong values into nested objects rather than throwing, making integration tests that assert nested property values essential.

---

## Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?

**Concepts**
- explicit SQL join requirement
- absence of navigation properties
- multi-mapping vs Include/ThenInclude
- no lazy loading
- precise control vs declarative relationship traversal

**Answer**

Dapper has no navigation properties or automatic graph loading — I shape graphs explicitly with SQL joins, multi-mapping overloads, or multiple queries. EF Core's `Include` and `ThenInclude` translate declared relationship metadata into join or split queries automatically, with the change tracker deduplicating parent instances during fix-up. With Dapper, I write the join, choose whether to use multi-map or separate queries, and merge duplicate parent rows in code when a one-parent-to-many-children join produces repeated parent columns. Dapper does not lazy-load — if columns are absent from the SELECT, the properties simply stay at default values with no hidden round-trips. EF Core's cartesian explosion from loading multiple collection includes has no Dapper equivalent unless I write the wide join myself, so Dapper gives me precise control over both what data is fetched and how it is assembled.

---

## Gotchas

---

## Gotcha 1. String concatenation instead of parameters

**Concepts**
- SQL injection via string interpolation
- parameterization bypass
- FromSqlInterpolated vs FromSqlRaw distinction
- ADO.NET and Dapper explicit parameter requirement

**Answer**

Building SQL with `$"WHERE Id = {id}"` or string concatenation sends user input as literal SQL text, bypassing parameterization and enabling SQL injection even when the rest of the application uses an ORM or micro-ORM. ADO.NET and Dapper require explicit parameters — `@Id` with a bound value — and never automatically sanitize concatenated strings. EF Core's `FromSqlInterpolated` is safe because it internally converts the interpolation holes to parameters; passing an ordinary interpolated string to `FromSqlRaw` is not safe. Code review should treat any dynamic SQL without parameter placeholders as a blocking defect.

---

## Gotcha 2. Open DataReader blocks second command

**Concepts**
- SqlDataReader exclusive connection use
- MARS opt-in requirement
- sequential reader disposal before next command
- EF Core internal reader management

**Answer**

Running another `SqlCommand` on the same connection while a `SqlDataReader` is still open fails on SQL Server unless Multiple Active Result Sets (MARS) is explicitly enabled in the connection string. A common version of this bug loads a header row and then tries to load detail rows on the same connection without closing the first reader. The fix is to always dispose or finish reading the `DataReader` before issuing the next command on that connection. EF Core manages its readers internally and largely shields you from this rule, but raw ADO.NET or Dapper code sharing a connection in the same request must respect it.

---

## Gotcha 3. `AddWithValue` and wrong SQL types

**Concepts**
- CLR-to-SQL type inference risk
- nvarchar length overestimation
- index scan vs seek degradation
- explicit SqlParameter type and size
- Dapper default typing behavior

**Answer**

`SqlParameter.AddWithValue` infers parameter types from CLR values, and the inference may not match the database column type — leading to implicit conversions, index scans, and poor plan cache behavior. String inference often picks oversized `nvarchar` lengths that prevent optimal index seeks on narrower columns. The fix is to prefer explicit `SqlParameter` declarations with `SqlDbType`, size, and precision matching the column definition. Dapper and EF Core parameterize with more predictable typing in most cases, but custom ADO.NET code still needs explicit type declarations for performance-critical queries.

---

## Gotcha 4. Leaked connections exhaust the pool

**Concepts**
- connection pool slot exhaustion
- undisposed SqlConnection leak
- await using disposal pattern
- DbContext long-lived instance
- load-only failure mode

**Answer**

Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots. Those slots remain occupied until they time out, eventually causing "timeout expired obtaining connection from pool" errors under concurrent load. The fix is to always use `await using` for connections and readers so disposal runs even when exceptions occur. The symptoms typically appear only under concurrent load — a classic production-only failure mode that passes all local tests. Long-lived undisposed `DbContext` instances cause the same pool exhaustion pattern.

---

## Gotcha 5. Transaction started after first command

**Concepts**
- autocommit implicit semantics
- BeginTransaction timing requirement
- unit-of-work atomicity boundary
- EF Core SaveChanges per-call commit
- integration test masking

**Answer**

Beginning a `SqlTransaction` only after the first statement has already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first. `BeginTransaction` must be called immediately after opening the connection, before any DML. EF Core's `SaveChanges` without an explicit transaction auto-commits each call — wrapping multi-step work in an explicit transaction is necessary when all-or-nothing semantics are required. Integration tests with single-user data often miss this race because implicit commits appear to work correctly in isolation.

---

## Gotcha 6. Dapper `Query` without `using` on connection

**Concepts**
- deferred IEnumerable execution timing
- connection lifetime vs enumeration lifetime
- ToList() materialization inside scope
- QueryAsync same requirement
- ObjectDisposedException on late iteration

**Answer**

Returning deferred `IEnumerable<T>` from Dapper before the connection is disposed postpones SQL execution until the caller iterates — which often happens after the `using` block has already closed the connection. The result is an `ObjectDisposedException` or "connection is closed" error at runtime, not at the point of the `Query` call. The fix is to materialize inside the connection scope with `.ToList()` or `.ToArray()` before returning from the method. Async variants like `QueryAsync` have the same requirement — `await` is not enough if the connection is disposed before the caller enumerates the returned `IEnumerable`.

---

## Gotcha 7. `QuerySingle` when zero or many rows exist

**Concepts**
- QuerySingle strict uniqueness assertion
- QueryFirstOrDefault optional absence handling
- unique key invariant requirement
- duplicate data producing hard failures
- EF Core SingleOrDefault vs FirstOrDefault parallel

**Answer**

Dapper's `QuerySingle` throws `InvalidOperationException` if zero rows or more than one row match. `QueryFirstOrDefault` returns `default(T)` when the result set is empty, making it the correct choice for lookups where absence is a valid outcome. I use `QuerySingle` only when exactly one row is a domain invariant enforced by a unique key. When duplicate data exists — even if it shouldn't — `QuerySingle` becomes a hard failure that `QueryFirstOrDefault` would handle differently. The choice should be driven by whether duplicates indicate a programming error or a legitimate business condition.

---

## Gotcha 8. Multi-map `splitOn` wrong column

**Concepts**
- forward-only splitOn matching
- default "Id" ambiguity in JOINs
- silent NULL or wrong value mapping
- SELECT column order requirement
- integration test assertion necessity

**Answer**

Dapper multi-mapping uses `splitOn` to name the column where the next object type begins. An incorrect column causes the split to land at the wrong boundary, silently mapping NULL or wrong values into nested objects without throwing an exception. The `splitOn` default of `"Id"` requires that the second type's first mapped column is literally named `Id` — duplicate column names in SELECT lists require explicit aliases and matching `splitOn` values. Column order in the SELECT must align with the generic type order in `Query<TFirst, TSecond, TReturn>`. Integration tests that assert nested property values are the reliable way to catch split errors.

---

## Gotcha 9. Scoped `DbContext` captured in a singleton

**Concepts**
- captive dependency anti-pattern
- singleton service lifetime
- scoped DbContext per HTTP request
- IDbContextFactory for long-lived services
- disposed context cross-request contamination

**Answer**

Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton remains alive, or its tracked state leaks across HTTP requests from different users. `DbContext` is scoped per request in ASP.NET Core, so singletons must never store it in fields. When a long-lived service genuinely needs database access, inject `IDbContextFactory<TContext>` instead, which lets the singleton create and dispose short-lived context instances on demand. Symptoms include "Cannot access a disposed context" exceptions or cross-user data contamination in tracked entities.

---

## Gotcha 10. Lazy loading after the context is disposed

**Concepts**
- lazy load SQL trigger on property access
- disposed context boundary
- JSON serialization as lazy load trigger
- eager loading as prevention
- proxy type interaction with disposal

**Answer**

Lazy loading triggers SQL when navigation properties are accessed. If that access happens after the request-scoped `DbContext` has been disposed — which frequently occurs when a JSON serializer traverses the entity graph near the end of the request pipeline — EF Core throws or the serializer triggers hidden queries that fail mid-response. The fix is to prefer explicit `Include` calls or DTO projections inside the request scope, ensuring all data is loaded before the context is disposed. Proxy types plus disposed contexts produce intermittent failures because the property access order determines which navigations are hit before vs after disposal.

---

## Gotcha 11. N+1 from lazy load or missing Include

**Concepts**
- N+1 query pattern
- navigation property loop access
- eager loading with Include/ThenInclude
- split queries
- EF Core command logging detection

**Answer**

Listing parent entities and then accessing navigation properties in a loop without eager loading fires one SQL query per parent row — the classic N+1 performance collapse. One query for N orders plus N queries for each order's lines equals N+1 round-trips per request, which compounds into catastrophic latency and database load at scale. The fix is `Include`/`ThenInclude`, split query mode, or `Select` projections that join the needed data in one statement. EF Core command logging revealing identical query templates with different ID parameters is the diagnostic signal that N+1 is occurring.

---

## Gotcha 12. Cartesian explosion with multiple Includes

**Concepts**
- cross-join row multiplication
- multiple collection include inflation
- AsSplitQuery separation
- EF Core change tracker deduplication
- DTO projection as avoidance strategy

**Answer**

Eager-loading two or more collection navigations in one SQL query multiplies result rows by the product of collection sizes — an order with 10 lines and 5 notes produces 50 rows, spiking memory and network use even though the parent entity count is modest. EF Core deduplicates parent instances during fix-up, but SQL Server has already transmitted the inflated rowset across the wire. The fix is `AsSplitQuery()`, which fetches collections with separate SELECT statements that each return only the rows they need. Projecting to DTOs is another avoidance strategy when only summaries or counts are needed rather than full collection graphs.

---

## Gotcha 13. Client-side evaluation of LINQ

**Concepts**
- ToList() before filter full-table pull
- non-translatable C# logic in Where
- EF Core 3+ exception on unsupported translation
- AsEnumerable() explicit in-memory switch
- database-side filtering alternatives

**Answer**

Calling `ToList()` before filtering or using non-translatable C# logic in `Where` forces EF Core to pull entire tables into application memory before filtering. This is acceptable in development with small seed data and catastrophic in production at scale. EF Core 3 and later throw on many accidental client evaluations instead of silently downloading whole tables, which helps catch the problem in development. `AsEnumerable()` explicitly switches to LINQ to Objects, meaning any following `Where` runs in memory — it should appear only when that is intentional. The fix for non-translatable expressions is to rewrite them using translatable predicates, `EF.Functions` helpers, database-side filtering, or raw SQL.

---

## Gotcha 14. Tracking overhead on read-only queries

**Concepts**
- change tracker snapshot per property
- AsNoTracking performance gain
- read-only GET endpoint pattern
- global QueryTrackingBehavior
- memory and CPU waste on tracked reads

**Answer**

Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on endpoints that only serve GET responses. The change tracker stores original and current values per property for each row materialized. Read services in ASP.NET Core should default to `AsNoTracking()` combined with DTO projection. Setting `QueryTrackingBehavior.NoTracking` globally and enabling explicit tracking only on command paths prevents accidental overhead from spreading as the codebase grows.

---

## Gotcha 15. `SaveChanges` without a transaction for multi-step updates

**Concepts**
- multiple SaveChanges independent commits
- partial update inconsistency risk
- BeginTransactionAsync explicit scope
- single SaveChanges unit of work
- retry logic assumption violation

**Answer**

Multiple `SaveChanges` calls or separate database operations that must succeed together commit independently by default, allowing partial updates that leave data in an inconsistent state when a later step fails. Wrapping related saves and raw SQL in `BeginTransactionAsync`/`CommitAsync` on one `DbContext` ensures they are atomic. When all changes are tracked on the same context, a single `SaveChanges` call is itself a transaction and is the simplest solution. Retry logic after a failure cannot assume earlier committed steps were rolled back unless they shared a transaction boundary.

---

## Scenario-Based Questions (Karat Format)

---

## Q1. (R) A junior ports `GetActiveProducts` into an ASP.NET Core API. Integration tests pass locally. What breaks under load or refactoring, and how would you fix it?

```csharp
public IEnumerable<Product> GetActiveProducts()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """;
    return connection.Query<Product>(sql);
}
```

**Concepts**
- undisposed SqlConnection pool leak
- deferred IEnumerable outliving connection
- pool exhaustion under concurrent load
- using block disposal guarantee
- ToList() materialization inside connection scope

**Answer**

The method returns a deferred `IEnumerable<Product>` tied to a connection that is never disposed. The connection stays open and un-pooled until garbage collection reclaims it, leaking a slot from the connection pool on every request. Under concurrent load this exhausts the pool and triggers "timeout expired obtaining connection" errors — a failure that local tests never expose because enumeration happens immediately on the same thread before GC pressure builds.

The fix is to wrap the connection in `using IDbConnection connection = new SqlConnection(_connectionString)` so disposal is guaranteed on any code path, and to call `.ToList()` before returning so the result is fully materialized inside the connection's lifetime. Changing the return type to `IReadOnlyList<Product>` signals to callers that the sequence is already materialized. There is also no need to call `Open()` manually — Dapper opens a closed connection automatically before executing.

```csharp
public IReadOnlyList<Product> GetActiveProducts()
{
    using IDbConnection connection = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """;
    return connection.Query<Product>(sql).ToList();
}
```

---

## Q2. (R) A search endpoint accepts a product name from the query string. What are the problems — security, correctness, maintainability — and what is the Dapper-native fix?

```csharp
public Product? FindByName(string productName)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    string sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName = '{productName}';
        """;
    return db.QueryFirstOrDefault<Product>(sql);
}
```

**Concepts**
- SQL injection via string interpolation
- @-placeholder parameterization pattern
- unescaped single-quote correctness failure
- Dapper anonymous object binding
- input validation as defense-in-depth only

**Answer**

Interpolating user input into the SQL string is a critical SQL injection vulnerability — an attacker can append arbitrary SQL after the closing quote to read, modify, or drop data. Beyond security, a legitimate product name containing a single quote — like `O'Brien` — breaks the query with a syntax error. The interpolation also undermines the primary reason to use Dapper, which is parameterized SQL.

The fix is to use a `@productName` placeholder and pass the value as a parameter:

```csharp
public Product? FindByName(string productName)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName = @productName;
        """;
    return db.QueryFirstOrDefault<Product>(sql, new { productName });
}
```

Dapper only parameterizes values you pass through its param argument — any string concatenated or interpolated into the SQL text before Dapper sees it is not protected. Input length validation at the API boundary adds defense-in-depth but is not a substitute for parameterization.

---

## Q3. (M) Two developers debate connection handling. Developer A uses a short-lived `using IDbConnection` per method and never calls `Open()`. Developer B opens once in a unit-of-work class and passes the open connection into repositories sharing a transaction. Under what conditions is each correct, and what is the connection state after `Query<T>` completes when it started closed?

**Concepts**
- per-operation short-lived connection pattern
- shared connection for transaction scope
- Dapper auto-open on closed connection
- connection state Open after execute
- ownership and disposal responsibility

**Answer**

Developer A's pattern is the default for standalone reads and writes — one short-lived connection per operation, disposed via `using`, which is pool-friendly and the standard pattern for independent queries. Developer B's pattern is correct when multiple commands must share one connection and one transaction; repositories accept the already-open `IDbConnection` instead of creating their own so all operations can commit or roll back together.

When Dapper executes on a closed connection it calls `Open()` before the command, and after `Query<T>` completes the connection state is `Open`. The `using` block's dispose then closes it and returns it to the pool. The wrong hybrid is opening per method inside a transaction scope but disposing between calls — that breaks atomicity. Creating a new connection per call inside a transaction block also fails because each call may autocommit separately unless explicitly enlisted. The deciding factor is transactional scope, not performance preference. Per-request `using` plus `.ToList()` scales cleanly in ASP.NET Core; a shared open connection belongs exclusively inside explicit transaction boundaries.

---

## Q4. (D) Your team builds an order microservice. Writes use EF Core with migrations. A product catalog read endpoint needs hand-tuned SQL with specific indexes and no change-tracker overhead. A teammate proposes using EF Core everywhere for consistency. What do you recommend?

**Concepts**
- hybrid EF Core and Dapper architecture
- write model vs read model tool selection
- change tracker overhead on read paths
- Dapper micro-ORM sweet spot
- consistency through boundaries not single-tool mandate

**Answer**

I recommend using EF Core for the write model and schema evolution while adding Dapper for the catalog read path with explicit SQL. EF Core simplifies order creation and inventory reservation through migrations, relationships, unit-of-work, and change tracking — all appropriate for a write model. The catalog read path has specific SQL, targeted indexes, and projections that Dapper executes with minimal overhead and no tracker allocations. Both paths share the same database and connection string configuration.

Forcing EF Core on the hand-tuned read path means fighting the LINQ translator for exact SQL, adding `AsNoTracking` everywhere, and losing fine-grained control over hints and covering index projections. Teams using EF Core on hot read paths often encounter N+1 issues or over-fetching that require progressively awkward workarounds. Consistency belongs in API contract design, error shapes, and logging conventions — not in forcing one ORM for every access pattern. Document when to reach for EF Core versus Dapper in an Architecture Decision Record so the team applies each tool intentionally.

---

## Q5. (R) A legacy `dbo.Products` table uses snake_case column names. A new Dapper DTO maps cleanly in tests against LocalDB seed data, but staging returns rows with empty names and zero prices. No exceptions are thrown. Diagnose the failure and give two production-safe fixes.

```csharp
public IReadOnlyList<ProductListItem> GetCatalog()
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = """
        SELECT product_id, product_name, unit_price
        FROM dbo.Products
        WHERE discontinued_date IS NULL;
        """;
    return db.Query<ProductListItem>(sql).ToList();
}
```

**Concepts**
- case-insensitive name equality mapping
- underscore vs PascalCase mismatch
- silent default value on unmapped properties
- SQL alias fix
- custom type map for global convention

**Answer**

Dapper maps by case-insensitive name equality. `product_id` may coincidentally match `ProductId` in some environments, but `product_name` does not match `ProductName` and `unit_price` does not match `UnitPrice`, so those properties stay at their default values — empty string and zero — with no exception. The LocalDB seed used PascalCase column names, hiding the mismatch; the staging schema uses legacy snake_case. No error is thrown because Dapper never requires all columns to map.

The SQL-side fix, preferred for reads, is to alias columns to match the property names:

```sql
SELECT product_id AS ProductId,
       product_name AS ProductName,
       unit_price AS UnitPrice
FROM dbo.Products
WHERE discontinued_date IS NULL;
```

The mapping-side fix for a codebase with many queries against the same legacy schema is to register a custom type map using `SqlMapper.SetTypeMap` or a naming convention handler so `product_name` maps globally to `ProductName`. Add integration tests against a schema that mirrors staging, asserting non-default `ProductName` and `UnitPrice` values for seeded rows, so schema drift is caught before production.

---

## Q6. (P) A teammate registers a Dapper `ProductRepository` in ASP.NET Core DI. What fails at startup or under concurrent traffic, and how should connection strings, repository lifetime, and `IDbConnection` be wired instead?

```csharp
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AdoNetTutorial")!));

public ProductRepository(IDbConnection connection) => _connection = connection;
```

**Concepts**
- singleton SqlConnection thread-safety violation
- captive dependency lifetime mismatch
- connection-string injection pattern
- scoped or transient repository lifetime
- per-method using IDbConnection creation

**Answer**

A singleton `IDbConnection` is not thread-safe for concurrent requests. Multiple threads executing Dapper calls on the same `SqlConnection` instance produce undefined behavior — interleaved commands, timeouts, and corrupted result reads. Even if thread safety were somehow handled, a long-lived connection bypasses connection pooling best practices and breaks the per-operation disposal semantics that make Dapper's pool behavior predictable.

The correct approach is to register `ProductRepository` as scoped (per request) or transient, never singleton, and to not register `IDbConnection` in DI at all for typical per-query usage. Instead, inject `IConfiguration` or an `IOptions<DatabaseOptions>` wrapper that holds the connection string, then create `using IDbConnection db = new SqlConnection(_connectionString)` inside each repository method:

```csharp
builder.Services.AddScoped<ProductRepository>();

public ProductRepository(IOptions<DatabaseOptions> options)
{
    _connectionString = options.Value.AdoNetTutorial
        ?? throw new InvalidOperationException("Connection string missing.");
}
```

For integration tests, inject a connection string pointing at LocalDB. Reserve `IDbConnection` injection only for test doubles where you need to control what the connection returns.

---

## Q7. (R) A developer coming from EF Core writes an update method. Identify compile/runtime/DI issues and explain why the price never persists.

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    var product = GetById(productId);
    if (product is null) return;
    product = product with { UnitPrice = newPrice }; // Product is not a record
    // no Execute call
}

public Product? GetById(int id)
{
    IDbConnection db = new SqlConnection(_connectionString);
    return db.QueryFirstOrDefault<Product>(
        "SELECT ... FROM dbo.Products WHERE ProductId = @id", new { id });
}
```

**Concepts**
- Dapper stateless micro-ORM design
- absence of change tracking
- with expression on non-record type compile error
- explicit UPDATE SQL requirement
- connection leak in GetById

**Answer**

The price never persists because Dapper is a stateless micro-ORM — it maps query rows to POCOs and executes explicit commands, but it has no change tracker. Modifying an in-memory object without calling `Execute` with an UPDATE statement makes no database round-trip at all. There is also a compile error: `product with { UnitPrice = newPrice }` is a `with` expression, which is only valid on `record` types — if `Product` is a sealed class, this does not build (CS8858). Additionally, `GetById` creates a `SqlConnection` without a `using` block, leaking a connection on every lookup.

The fix is to replace the EF mental model with explicit SQL:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = "UPDATE dbo.Products SET UnitPrice = @newPrice WHERE ProductId = @productId;";
    db.Execute(sql, new { productId, newPrice });
}
```

Fix `GetById` with `using IDbConnection` as well. If a read-then-modify pattern is genuinely required for business logic, wrap the read and update in a transaction on a single open connection to prevent concurrent price updates from racing.

---

## Chapter 02 Karat Scenarios

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

---

## Chapter 03 Karat Scenarios

---

## Q1. (R) A junior developer ships a product search endpoint. Security review flags it before deploy. What is wrong, and how do you fix it without changing the public method signature?

```csharp
public IEnumerable<Product> SearchByName(string searchTerm)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = $"""
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE '%{searchTerm}%'
        ORDER BY ProductName;
        """;
    return connection.Query<Product>(sql);
}
```

**Concepts**
- LIKE clause SQL injection via interpolation
- @Pattern parameter for LIKE predicate
- C#-side wildcard composition
- deferred IEnumerable without materialization
- parameter value vs SQL structure distinction

**Answer**

The SQL is built with C# string interpolation, so `searchTerm` is concatenated into the command text. An attacker can inject arbitrary SQL — `'; DROP TABLE dbo.Products; --` — to read, modify, or destroy data. Dapper parameterizes only what you pass through its param argument; interpolated strings bypass the parameter channel entirely. The method also returns a deferred `IEnumerable<Product>` from inside a `using` block, which will fail if the caller enumerates after disposal.

The fix uses a `@Pattern` placeholder and composes the wildcard in C# before passing it as a parameter value:

```csharp
public IReadOnlyList<Product> SearchByName(string searchTerm)
{
    using var connection = new SqlConnection(_connectionString);
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE ProductName LIKE @Pattern
        ORDER BY ProductName;
        """;
    return connection.Query<Product>(sql, new { Pattern = $"%{searchTerm}%" }).ToList();
}
```

The wildcard characters `%` are part of the parameter value, not SQL syntax, so they are fully parameterized. Input length validation at the API layer adds defense-in-depth but is not a substitute.

---

## Q2. (R) Code review on a stored-procedure insert path. The author claims it works in a one-off SSMS test. What breaks at runtime or in production?

```csharp
public int InsertProduct(string productName, decimal unitPrice, int stockQuantity)
{
    using var connection = new SqlConnection(_connectionString);
    var dp = new DynamicParameters();
    dp.Add("@ProductName", productName);
    dp.Add("@UnitPrice", unitPrice);
    dp.Add("@StockQuantity", stockQuantity);
    dp.Add("@NewProductId", direction: ParameterDirection.Output);

    int newId = dp.Get<int>("@NewProductId"); // read before execute

    connection.Execute("dbo.usp_InsertProduct", dp,
        commandType: CommandType.StoredProcedure);
    return newId;
}
```

**Concepts**
- OUTPUT parameter populated after execute
- DynamicParameters.Get read timing
- explicit DbType for output parameters
- zero-default return propagating as real ID
- ADO.NET execute-then-read semantics

**Answer**

OUTPUT parameters are populated by the database engine after the command executes — reading `@NewProductId` before calling `Execute` always returns the CLR default `0`. The method returns `0` as the new product ID on every call, regardless of what the stored procedure actually inserts. Any downstream code that uses this ID to build foreign key relationships will silently create corrupt data.

The `Add` call also omits `dbType: DbType.Int32`, which can cause provider inference issues for Output parameters on some configurations.

The correct order is to declare the output parameter with an explicit type, execute, then read:

```csharp
dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_InsertProduct", dp, commandType: CommandType.StoredProcedure);
return dp.Get<int>("@NewProductId");
```

SSMS manual runs hide this ordering bug because SSMS does not read output parameters before the batch executes — the ADO.NET model is explicit about the execute-then-read contract.

---

## Q3. (R) A dashboard API calls `usp_GetProductDashboard` but returns wrong totals after a DBA reorders the SELECT statements inside the procedure. What fails silently vs throws, and how do you harden it?

```csharp
public DashboardDto GetDashboard()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(
        "dbo.usp_GetProductDashboard", commandType: CommandType.StoredProcedure);

    int totalCount = multi.Read<int>().Single();
    var products = multi.Read<Product>().AsList();
    string topName = multi.ReadFirst<string>();
    return new DashboardDto(products, totalCount, topName);
}
```

**Concepts**
- GridReader forward-only order dependency
- result-set reorder silent wrong mapping
- CLR type mismatch on wrong set throwing
- proc-consumer contract versioning
- documented read-order integration test

**Answer**

`GridReader.Read<T>()` is forward-only and order-dependent — it always consumes the next result set in sequence. When the DBA reorders the stored procedure's SELECT statements, the consumer reads a product-shaped result set where it expects a count integer, and vice versa. Some type mismatches throw at read time; others produce garbage values silently — if the first set happens to be mappable to `int` by accident, `totalCount` gets a nonsense value and the dashboard shows wrong numbers without any exception.

To harden this, align the consumer read order with the documented procedure contract and never leave that contract implicit. Add an integration test that asserts column shapes and values for each `Read` call. Document the expected result-set order in the procedure's header comment, and treat any change to that order as a breaking API change requiring a coordinated consumer update. For dashboards where the order is likely to change, consider a single result set with fixed column aliases, separate endpoints, or a JSON-typed return value from the procedure.

---

## Q4. (M) A filter query returns every row instead of the intended price band. The SQL and anonymous object look correct at a glance. What is the binding bug?

```csharp
return connection.Query<Product>(sql, new { MinimumPrice = minPrice, MaximumPrice = maxPrice });
// SQL: WHERE UnitPrice >= @MinPrice AND UnitPrice <= @MaxPrice
```

**Concepts**
- property-to-placeholder exact name alignment
- case-insensitive but exact-name matching
- unbound parameters treated as unconstrained
- full table scan from missing predicate
- anonymous object property naming discipline

**Answer**

Dapper binds parameters by name: the SQL tokens `@MinPrice` and `@MaxPrice` require an object with properties named `MinPrice` and `MaxPrice`. The anonymous object uses `MinimumPrice` and `MaximumPrice`, so those parameters are never supplied to the SQL command. SQL Server may treat the unbound predicates as always-true, returning all rows — a full table scan with wrong business data and no error.

The fix is to rename the properties to match the SQL placeholders exactly: `new { MinPrice = minPrice, MaxPrice = maxPrice }`. Alternatively, rename the SQL tokens to `@MinimumPrice` and `@MaximumPrice` to match the object. An integration test with known seed data asserting the exact row count within a price band is the most reliable way to catch this class of name-alignment bug.

---

## Q5. (P) An ASP.NET Core endpoint wraps a long-running report stored procedure. The controller passes `HttpContext.RequestAborted` as cancellation. What is missing for timeout and cooperative cancellation?

```csharp
public async Task<IReadOnlyList<Product>> GetSlowReportAsync(CancellationToken cancellationToken)
{
    using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync(cancellationToken);
    return (await connection.QueryAsync<Product>(
        "dbo.usp_SlowProductReport",
        commandType: CommandType.StoredProcedure)).AsList();
}
```

**Concepts**
- CommandDefinition as Dapper cancellation carrier
- command timeout explicit override
- CancellationToken propagation to ADO.NET
- thread and connection exhaustion under slow procs
- OperationCanceledException mapping

**Answer**

The cancellation token is passed to `OpenAsync` but not to the `QueryAsync` call itself. If the client disconnects or the request is aborted after the connection opens, the SQL command continues running until the default connection timeout elapses. Under load, slow report procedures hold pool connections open and eventually exhaust the pool. `CommandDefinition` is the Dapper hook for supplying both a command timeout and a cancellation token to a single call:

```csharp
var cmd = new CommandDefinition(
    "dbo.usp_SlowProductReport",
    parameters: null,
    commandTimeout: 120,
    commandType: CommandType.StoredProcedure,
    cancellationToken: cancellationToken);
return (await connection.QueryAsync<Product>(cmd)).AsList();
```

Set the timeout to the report's measured SLA rather than relying on the connection default. Map `OperationCanceledException` and SQL timeout exceptions (number `-2`) to appropriate HTTP status codes (499 or 504) with structured logging. For very heavy reports, an async job-plus-polling pattern is preferable to stretching the synchronous timeout indefinitely.

---

## Q6. (D) A teammate refactors `GetProductCountViaReturnValue` to use `ExecuteScalar` and another method tries to read a T-SQL `RETURN` value via an `OUTPUT` parameter. Explain what each approach gets wrong about `RETURN` vs `OUTPUT`, and what Dapper API is correct for each.

```csharp
// Attempt 1 — proc uses RETURN, not SELECT
var count = connection.ExecuteScalar<int>("dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);

// Attempt 2 — proc uses RETURN 0/RETURN 1, not OUTPUT @Status
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status");
```

**Concepts**
- T-SQL RETURN value as ReturnValue parameter
- OUTPUT parameter distinct from RETURN
- ExecuteScalar reads first result set column only
- DynamicParameters ParameterDirection.ReturnValue
- anonymous object Input-only limitation

**Answer**

T-SQL `RETURN` sends an integer through a dedicated ReturnValue channel in ADO.NET — it does not go through result sets or arbitrary OUTPUT parameters. `ExecuteScalar` reads the first column of the first row of a result set; a procedure that only executes `RETURN COUNT(*)` with no SELECT produces no result set, so `ExecuteScalar` returns `null` or zero rather than the count.

Attempt 2 adds an `@Status` OUTPUT parameter, but the procedure uses `RETURN 0` and `RETURN 1` — those values flow through ReturnValue, not through any OUTPUT parameter. `dp.Get<int>("@Status")` always returns zero because nothing ever sets that output.

The correct Dapper approach for `RETURN` values is a `DynamicParameters` entry with `ParameterDirection.ReturnValue`:

```csharp
var dp = new DynamicParameters();
dp.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@ReturnValue");
```

For a procedure that SELECTs a single value, use `ExecuteScalar`. For OUTPUT parameters, use `ParameterDirection.Output` with an explicit `DbType` and size for strings. Anonymous objects can only supply Input parameters — directional parameters always require `DynamicParameters`.

---

## Q7. (R) A batch import service loads a dashboard in one round trip but intermittently throws `InvalidOperationException` under load. Identify lifetime and reader issues and the fix.

```csharp
public (IEnumerable<Product> Products, int Total, string TopName) GetDashboardLazy()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var multi = connection.QueryMultiple(sql);
    var products = multi.Read<Product>(); // deferred
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();
    return (products, total, top);
}
```

**Concepts**
- GridReader forward-only deferred sequence
- connection and GridReader undisposed
- deferred IEnumerable outliving reader
- materialization inside using scope
- IReadOnlyList as safe return type

**Answer**

The method returns a deferred `IEnumerable<Product>` from `multi.Read<Product>()`. When the caller enumerates `products`, the underlying `GridReader` and connection may already be in use by other concurrent calls or may have been released — causing "invalid operation" errors. The connection is also never disposed, leaking a pool slot on every invocation.

The three-part fix is to wrap both the connection and GridReader in `using` blocks, materialize every `Read` call before leaving the `using` scope, and return materialized collections:

```csharp
public (IReadOnlyList<Product> Products, int Total, string TopName) GetDashboardMaterialized()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(sql);
    var products = multi.Read<Product>().AsList();
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();
    return (products, total, top);
}
```

Complete all `Read` calls in order before the `using` block exits — the forward-only `GridReader` mirrors `SqlDataReader.NextResult` semantics, and leaving it mid-way puts the reader in an undefined state.

---

## Chapter 04 Karat Scenarios

---

## Q1. (R) A teammate removes the explicit `splitOn` from `GetOrdersWithCustomers` because "both sides have an Id-like key." Orders load in QA but `Customer.Name` is null and `Customer.CustomerId` equals `Order.OrderId`. What is wrong and how do you fix it?

```csharp
return connection.Query<Order, Customer, Order>(
    sql,
    (order, customer) => { order.Customer = customer; return order; }
).ToList(); // splitOn removed — defaults to "Id"
```

**Concepts**
- splitOn default "Id" column matching
- duplicate CustomerId boundary ambiguity
- wrong split silently mismapping columns
- explicit splitOn on unique second-type column
- SQL alias as alternative disambiguation

**Answer**

Dapper's default `splitOn` is `"Id"` — it splits at the first column matching that name. In the JOIN query, `CustomerId` appears twice: once as `o.CustomerId` (a foreign key on Order) and once as `c.CustomerId` (the primary key on Customer). Without an explicit `splitOn`, Dapper finds the first `CustomerId` column — which belongs to Order — and starts mapping Customer from there. Everything after Order's `CustomerId` maps into the Customer type, but the Customer primary key never appears at the correct boundary, so `Customer.Name` and `Customer.Email` never bind.

The fix is an explicit `splitOn` pointing to a column unique to the Customer portion of the row — for example, `splitOn: "Name"` if Orders have no `Name` column, or a SQL alias like `c.CustomerId AS CustCustomerId` with `splitOn: "CustCustomerId"`. Column order must be maintained: all Order columns first, then all Customer columns. Never rely on default `splitOn` for real JOIN queries with shared column name patterns.

---

## Q2. (R) Another developer models order lines without the dictionary lookup. The API returns three lines for order 1 but `TotalAmount` and `Lines.Count` disagree depending on which element the caller uses. Diagnose and describe the production-safe aggregation pattern.

```csharp
List<Order> orders = connection.Query<Order, OrderLine, Order>(
    sql,
    (order, line) => { order.Lines.Add(line); return order; },
    new { orderId }, splitOn: "OrderLineId").ToList();
return orders.FirstOrDefault();
```

**Concepts**
- delegate called once per JOIN row
- new Order instance per row in map delegate
- dictionary deduplication for parent aggregation
- one-to-many multi-map pattern
- FirstOrDefault hiding fragmentation

**Answer**

Dapper's map delegate runs once per row returned by the JOIN. Each invocation receives a freshly materialized `Order` and `OrderLine` — they are not the same `Order` instance across rows. Calling `order.Lines.Add(line)` modifies a brand-new `Order` object each time, so the list ultimately contains one partial `Order` per line rather than one aggregated Order with all lines. `FirstOrDefault()` returns the first of these partial objects, which has exactly one line regardless of how many lines the order has.

The production-safe pattern is a dictionary that tracks `Order` instances by primary key:

```csharp
var lookup = new Dictionary<int, Order>();
connection.Query<Order, OrderLine, Order>(
    sql,
    (order, line) =>
    {
        if (!lookup.TryGetValue(order.OrderId, out var existing))
            lookup[order.OrderId] = existing = order;
        existing.Lines.Add(line);
        return existing;
    },
    new { orderId }, splitOn: "OrderLineId");
return lookup.TryGetValue(orderId, out var result) ? result : null;
```

Multi-mapping handles column splitting, not aggregation — one-to-many relationships always need explicit parent deduplication.

---

## Q3. (R) A new microservice copies `ColumnMappedProduct` and `DateOnlyTypeHandler` but omits the `RegisterDapperExtensions` call. `Price` is always `0` in some test runs; `QuerySingleAsync<DateOnly>` throws `DataException` in others. What is missing and why does test order matter?

**Concepts**
- Dapper global static type registration
- SqlMapper.AddTypeHandler startup requirement
- SqlMapper.SetTypeMap column attribute mapping
- test fixture one-time registration
- test-order-dependent flakiness from shared static state

**Answer**

`SqlMapper.AddTypeHandler` and `SqlMapper.SetTypeMap` modify global static dictionaries in Dapper. They must be called once before any query that uses those types. Without the registration call in `Program.cs`, the `[Column("UnitPrice")]` attribute is never applied so `Price` stays at its default zero, and `DateTime` from a `DATE` column cannot convert to `DateOnly` without the type handler — causing `DataException` at read time.

Test order matters because Dapper's static state persists across tests in the same process. If a test that indirectly triggers registration runs first — perhaps through a `WebApplicationFactory` — the handler is already registered and the test passes. Run the tests in a different order and registration never happens, so the test fails. This is the classic shared-static-state flakiness pattern.

The fix is to call the registration in a single guaranteed-to-run location: `Program.cs` for production, and a shared `IClassFixture` or `[ModuleInitializer]` for tests. Where SQL aliases can cover the rename (`UnitPrice AS Price`), prefer them over global type maps to keep configuration local and explicit.

---

## Q4. (P) Production needs `DiscontinuedDate` mapped to `DateOnly?`. A junior registers a type handler, but under load some NULL rows throw and parameterized inserts sometimes fail. What should `Parse` and `SetValue` handle?

```csharp
public override DateOnly? Parse(object value) =>
    value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
    parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
```

**Concepts**
- DBNull.Value distinct from C# null in ADO.NET
- Parse DBNull explicit handling
- SetValue DBNull.Value for SQL NULL
- IDbDataParameter.Value null vs DBNull
- TypeHandler vs SQL CAST/CONVERT trade-off

**Answer**

ADO.NET surfaces database NULL values as `DBNull.Value`, not as C# `null`. The `Parse` method's `value is DateTime dt` pattern never matches `DBNull.Value`, so NULL database columns can fall through and throw `InvalidCastException` in some provider implementations. The explicit safe guard is:

```csharp
public override DateOnly? Parse(object value)
{
    if (value is null or DBNull) return null;
    return value is DateTime dt ? DateOnly.FromDateTime(dt) : DateOnly.Parse(value.ToString()!);
}
```

For `SetValue`, assigning `parameter.Value = null` (from `value?.ToDateTime(...)` when `value` is null) may not properly signal SQL NULL to the provider — `IDbDataParameter.Value` must be set to `DBNull.Value` explicitly:

```csharp
public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
    parameter.Value = value.HasValue ? (object)value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
```

Use a TypeHandler when the conversion recurs across many queries or has both read and write directions. For a one-off read-only DTO against a single query, a SQL `CAST(DiscontinuedDate AS date)` alias with a `DateTime?` property avoids the registration overhead entirely.

---

## Q5. (D) An order-details endpoint loops over 200 orders fetching customer and lines per order. A proposal replaces the loop with JOIN queries using multi-mapping. Compare N+1 queries, two JOIN queries, and one wide JOIN with dictionary aggregation. What would you ship for a paginated admin grid vs a single-order detail page?

**Concepts**
- N+1 latency and connection scaling
- JOIN multi-map eliminating round-trips
- cartesian row duplication in wide JOINs
- paginated grid shallow hydration pattern
- single-detail deep hydration pattern

**Answer**

N+1 is wrong for any hot endpoint: 200 orders triggering 400 additional queries produces 401 round-trips per request and collapses under concurrent load. The question is which JOIN strategy fits which page.

For a paginated admin grid showing 200 orders with customer name only, I ship one JOIN — `Query<Order, Customer, Order>` with explicit `splitOn` — and skip loading lines entirely. The grid never displays lines, so loading them wastes memory and widens rows unnecessarily.

For a single-order detail page, I ship two queries: one JOIN for order plus customer, and one multi-map with dictionary aggregation for that order's lines. Two queries keeps mapping simple, avoids duplicating order and customer columns per line row, and remains fast because the dataset is narrow. A single wide JOIN of order plus customer plus lines also works when line count is bounded — use the dictionary aggregation pattern and never return the duplicated flat rows directly to the client.

One wide JOIN for a list of 200 orders with lines is inappropriate: it duplicates order and customer columns once per line, multiplying data transfer and memory. Multi-mapping eliminates N+1 round-trips but does not replace pagination discipline — always constrain how deep each endpoint hydrates.

---

## Q6. (R) A developer migrates INSERT logic to Dapper.Contrib. `ProductId` is not IDENTITY — keys are assigned manually. Insert appears to succeed but `Get` returns null. SQL Profiler shows an INSERT without `ProductId`. What went wrong with Contrib attributes?

```csharp
[Table("Products")]
public sealed class ProductEntity
{
    [Key] // should be [ExplicitKey]
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}
```

**Concepts**
- Contrib [Key] vs [ExplicitKey] distinction
- IDENTITY column assumption in [Key]
- INSERT column exclusion for auto-generated keys
- [ExplicitKey] for manually assigned keys
- MAX+1 concurrency risk regardless of attribute

**Answer**

`[Key]` tells Dapper.Contrib that `ProductId` is an identity column — the database generates its value, so Contrib excludes it from the INSERT statement. When the application assigns the ID manually and marks it `[Key]`, Contrib emits an INSERT with no `ProductId` column. The insert either fails on a NOT NULL constraint, inserts a zero or default value, or inserts with the wrong key — hence `GetAsync` returning null because the row is not findable by the expected ID.

The fix is to change `[Key]` to `[ExplicitKey]`, which tells Contrib that the application supplies the key value and it must be included in the INSERT. Verify the generated SQL in SQL Profiler after the change to confirm `ProductId` appears in the INSERT.

The `MAX(ProductId) + 1` approach for ID generation remains a concurrency risk even after fixing the attribute: two concurrent transactions can compute the same next ID and race to insert, producing a primary key violation. For production, delegate ID generation to a database `SEQUENCE` or `IDENTITY` column, or wrap the max-computation and insert in a serialized transaction with appropriate locking.

---

## Q7. (R) A search API builds product filters with string concatenation. Security review flags SQL injection on `category` and `sortColumn`. Refactor toward Dapper.SqlBuilder with parameterized fragments.

```csharp
var sql = "SELECT ... WHERE 1=1";
if (!string.IsNullOrEmpty(category))
    sql += $" AND Category = '{category}'";
if (minPrice.HasValue)
    sql += $" AND UnitPrice >= {minPrice.Value}";
sql += $" ORDER BY {sortColumn}";
```

**Concepts**
- user value injection via filter concatenation
- sort column identifier injection
- SqlBuilder parameterized WHERE fragments
- column name whitelist for ORDER BY
- values vs identifiers in SQL injection model

**Answer**

Both `category` and `sortColumn` are injected directly into SQL as string literals. For `category`, an attacker can close the string with a quote and append arbitrary SQL. For `sortColumn`, identifier injection is equally dangerous — `'; DROP TABLE dbo.Products; --` appended to `ORDER BY` can execute arbitrary statements.

The solution splits responsibility between parameters and whitelisting. User-supplied values like `category` and `minPrice` flow through Dapper parameters; column names cannot be parameterized in T-SQL and must come from a fixed whitelist:

```csharp
var builder = new SqlBuilder();
var template = builder.AddTemplate(
    "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products /**where**/ /**orderby**/");

if (!string.IsNullOrEmpty(category))
    builder.Where("Category = @Category", new { Category = category });
if (minPrice.HasValue)
    builder.Where("UnitPrice >= @MinPrice", new { MinPrice = minPrice });

var sortCol = sortColumn switch
{
    "Name" => "ProductName",
    "Price" => "UnitPrice",
    _ => "ProductId"
};
builder.OrderBy(sortCol);

return conn.Query<ProductRow>(template.RawSql, template.Parameters).ToList();
```

The whitelist maps the user's sort request to a literal column name chosen by the application — an attacker who passes an unsupported sort key gets the default `ProductId` ordering, never their injected SQL. Parameters protect value predicates; the whitelist protects identifier-position values.
