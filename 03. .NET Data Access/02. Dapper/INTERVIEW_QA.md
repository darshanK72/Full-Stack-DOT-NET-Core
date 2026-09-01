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

### Q1. What is Dapper?

What is Dapper?

**Answer:** Dapper is a lightweight .NET library that extends `IDbConnection` with extension methods for executing SQL and mapping result rows to plain C# objects. It sits on top of ADO.NET (ActiveX Data Objects for .NET) and keeps you in control of the SQL while removing most manual reader and parameter boilerplate.

- Created by Stack Overflow and distributed as the `Dapper` NuGet package.
- Works with any ADO.NET provider — SQL Server, PostgreSQL, MySQL, SQLite, and others.
- Maps query results to classes, structs, value tuples, and dynamic types with minimal configuration.
- Does not own connection lifetime — you still open, dispose, and pool connections through the underlying provider.

---

### Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?

What type of library is Dapper (ORM, micro-ORM, or something else)?

**Answer:** Dapper is a micro-ORM (Object-Relational Mapper) — it maps rows to objects but does not model relationships, change tracking, or schema migrations like a full ORM. You write the SQL; Dapper handles parameter binding and materialization.

- A full ORM like Entity Framework Core generates SQL from LINQ and tracks entity state across a unit of work.
- Dapper deliberately avoids those features to stay fast and predictable for hand-tuned queries.
- It is often described as an object mapper rather than a complete persistence framework.
- Teams pick it when SQL visibility and performance matter more than convention-based modeling.

---

### Q3. What problem does Dapper solve compared to raw ADO.NET?

What problem does Dapper solve compared to raw ADO.NET?

**Answer:** Raw ADO.NET requires repetitive code to create commands, add parameters, open readers, and copy column values into object properties row by row. Dapper collapses that into one call such as `connection.Query<Product>(sql, param)` while still using your SQL and connection.

- Eliminates manual `SqlDataReader` indexing and null-checking for every column.
- Binds anonymous objects, `DynamicParameters`, or expando objects to SQL parameters automatically.
- Supports async methods (`QueryAsync`, `ExecuteAsync`) with the same concise surface area.
- Keeps full access to transactions, stored procedures, and provider-specific features underneath.

---

### Q4. What problem does Dapper solve compared to Entity Framework Core?

What problem does Dapper solve compared to Entity Framework Core?

**Answer:** Entity Framework Core adds abstraction overhead — LINQ translation, change tracking, migrations, and relationship fix-up — that can be unnecessary for simple, performance-sensitive read paths. Dapper gives you direct SQL execution with near-ADO.NET speed and no hidden queries.

- You always see the exact SQL sent to the database, which simplifies tuning and index work.
- No change tracker means lower memory use on large read-only result sets.
- No migration or model configuration layer — schema changes are managed in SQL or external tools.
- Trade-off: you manually handle joins, updates, and relationship shaping that EF Core automates.

---

### Q5. When would you choose Dapper over EF Core?

When would you choose Dapper over EF Core?

**Answer:** Choose Dapper when you need hand-written SQL, maximum read throughput, or integration with legacy stored procedures where EF Core's LINQ translation adds little value. It fits reporting, bulk reads, and microservices that treat the database as the source of truth for complex queries.

- Hot paths where every millisecond and every round-trip are profiled and fixed in SQL.
- Teams with strong database administrators who prefer procedures and tuned statements in source control.
- Scenarios where EF Core would generate inefficient joins, cartesian explosions, or untranslatable LINQ.
- Read-heavy APIs that only need Data Transfer Objects (DTOs) without entity graphs or change tracking.

---

### Q6. When would you choose EF Core over Dapper?

When would you choose EF Core over Dapper?

**Answer:** Choose Entity Framework Core when you want convention-based mapping, LINQ composability, migrations, and change tracking for typical create-read-update-delete (CRUD) workflows. EF Core 8 reduces the gap on bulk operations but still excels at model-driven application development.

- Greenfield applications where C# entity classes should drive schema evolution through migrations.
- Complex object graphs with relationships, eager loading, and automatic relationship fix-up.
- When `SaveChanges` unit-of-work semantics and concurrency tokens are simpler than manual SQL per operation.
- When you need provider-agnostic LINQ that EF Core translates rather than maintaining SQL per database.

---

### Q7. What are the main advantages and limitations of Dapper?

What are the main advantages and limitations of Dapper?

**Answer:** Dapper's main advantages are speed, simplicity, and full SQL control with minimal mapping code. Its limitations are the absence of change tracking, migrations, relationship navigation, and automatic SQL generation — every query and schema change is your responsibility.

- Advantage: among the fastest mappers in .NET benchmarks because it avoids heavy metadata and proxy layers.
- Advantage: tiny API surface — learn `Query`, `Execute`, and parameters rather than an entire ORM stack.
- Limitation: no built-in way to detect dirty entities or batch updates without writing SQL yourself.
- Limitation: multi-table object graphs require manual joins, multi-mapping, or multiple queries.
- Limitation: schema drift is not caught at compile time the way a strongly configured EF Core model can be.

---

### Q8. Does Dapper generate SQL for you?

Does Dapper generate SQL for you?

**Answer:** No. Dapper never generates SQL — you supply the complete statement as a string (or procedure name). It only binds parameters, executes the command through ADO.NET, and maps the result set to your types.

- Inserts, updates, and deletes require explicit `INSERT`, `UPDATE`, or `DELETE` text or stored procedures.
- There is no LINQ provider or expression tree translator in Dapper itself.
- Third-party extensions such as Dapper.Contrib offer optional CRUD helpers but still emit fixed SQL templates, not dynamic LINQ translation.
- This design is intentional: predictable SQL is a feature, not a missing ORM capability.

---

## Chapter 02. Queries, Execute & Async Methods

### Q1. What is the difference between Dapper's `Query` and `Execute` methods?

What is the difference between Dapper's `Query` and `Execute` methods?

**Answer:** `Query` and its generic overloads run a statement that returns rows and materialize them into objects or scalars. `Execute` runs a statement that does not return a result set — inserts, updates, deletes — and returns the number of rows affected.

- `Query<T>(sql, param)` yields zero or more mapped instances of `T`.
- `Execute(sql, param)` maps to ADO.NET `ExecuteNonQuery` and returns an `int` row count.
- Use `Query` for `SELECT` statements and `Execute` for data manipulation language (DML) without result grids.
- Both accept the same parameter objects and honor the connection's current transaction.

---

### Q2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?

When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?

**Answer:** Use `Query<T>` when you expect zero or many rows and want a sequence to iterate or materialize. Use `QueryFirstOrDefault<T>` when you want at most one row — it returns the first match or `default(T)` if the result set is empty.

- `Query<T>` returns `IEnumerable<T>` (deferred unless buffered) for lists and filters.
- `QueryFirstOrDefault<T>` executes immediately for single-row lookups such as "get by id when row may not exist."
- `QuerySingle<T>` throws if zero or more than one row exists — use only when uniqueness is guaranteed.
- Picking `QuerySingle` for optional lookups is a common bug; prefer `QueryFirstOrDefault` when absence is valid.

---

### Q3. What does `Execute` return, and when is it used?

What does `Execute` return, and when is it used?

**Answer:** `Execute` returns an integer count of rows affected by the command, matching ADO.NET `ExecuteNonQuery` semantics. Use it for inserts, updates, deletes, and any non-query stored procedure that does not return a grid.

- A return value of `0` means no rows matched the statement — often expected for idempotent deletes, sometimes a bug for updates.
- Does not return generated keys; use `ExecuteScalar` or an `OUTPUT` clause when you need the new identity value.
- Participates in an ambient transaction when the connection has an active `IDbTransaction`.
- Async counterpart `ExecuteAsync` is preferred in ASP.NET Core request handlers to avoid blocking threads.

---

### Q4. Does Dapper open the connection if it is closed when you call `Query`?

Does Dapper open the connection if it is closed when you call `Query`?

**Answer:** Yes. Dapper checks `ConnectionState` and calls `Open()` (or `OpenAsync()` for async methods) before executing if the connection is closed. You are still responsible for disposing the connection so it returns to the pool.

- Opening inside Dapper does not bypass pooling — `SqlConnection` still draws from the pool when configured.
- If you manage connection lifetime externally, you may open once and reuse for multiple Dapper calls in the same scope.
- Dapper does not close the connection after the call unless you use helper overloads that accept `commandBehavior` with auto-close semantics in specific scenarios.
- Best practice remains `await using var connection = new SqlConnection(...)` and let disposal close the connection back to the pool.

---

### Q5. What is the difference between buffered and unbuffered queries in Dapper?

What is the difference between buffered and unbuffered queries in Dapper?

**Answer:** By default Dapper buffers query results — it reads the entire result set into memory before returning, then closes the reader. With `buffered: false`, Dapper streams rows through a live `IDataReader` as you enumerate, using less memory but holding the connection open until enumeration completes.

- Buffered (default): safe to return results from the method and enumerate later; connection can be disposed after `.ToList()`.
- Unbuffered: lower memory for very large result sets but enumeration must finish before the connection is disposed.
- Returning deferred `IEnumerable<T>` from an unbuffered query after leaving the `using` block causes "connection is closed" errors.
- Always materialize with `.ToList()` inside the connection scope unless you deliberately stream within that scope.

---

### Q6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?

What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?

**Answer:** Dapper mirrors its synchronous API with async counterparts that accept optional `CancellationToken` values: `QueryAsync`, `QueryFirstAsync`, `QueryFirstOrDefaultAsync`, `QuerySingleAsync`, `QuerySingleOrDefaultAsync`, `ExecuteAsync`, `ExecuteScalarAsync`, and `QueryMultipleAsync`. They use the underlying provider's async ADO.NET methods.

- Prefer async variants in ASP.NET Core so request threads are not blocked during network I/O to the database.
- `QueryAsync` still supports buffered and unbuffered modes — the same deferred-enumeration rules apply when unbuffered.
- `ExecuteScalarAsync` returns the first column of the first row for aggregates and identity retrieval.
- Cancellation tokens propagate to the provider when supported, allowing request aborts to cancel long-running queries.

---

## Chapter 03. Parameters, Stored Procedures & QueryMultiple

### Q1. How do you pass parameters to a Dapper query?

How do you pass parameters to a Dapper query?

**Answer:** Pass parameters as the second argument using an anonymous object, a `DynamicParameters` bag, a `Dictionary<string, object>`, or any object whose public properties match `@Name` placeholders in the SQL. Dapper maps property names to parameter names case-insensitively.

- Anonymous object: `new { Id = 42, Name = "Widget" }` binds `@Id` and `@Name`.
- `DynamicParameters` supports output parameters, table-valued parameters, and explicit database types.
- Positional names in SQL use `@` prefix for SQL Server; syntax follows the target provider.
- Property names must align with parameter names — `UserId` maps to `@UserId`, not `@user_id`, unless you alias in SQL.

---

### Q2. How does Dapper prevent SQL injection?

How does Dapper prevent SQL injection?

**Answer:** Dapper sends user values as ADO.NET parameters separate from the SQL text, so input is never interpreted as executable SQL syntax. As long as you use `@placeholders` with parameter objects and do not concatenate user input into the SQL string, injection is prevented.

- Safe: `"SELECT * FROM Users WHERE Email = @Email", new { Email = userInput }`.
- Unsafe: `$"SELECT * FROM Users WHERE Email = '{userInput}'"` — Dapper cannot fix inlined strings.
- `DynamicParameters.Add("Email", value)` still parameterizes even when building SQL dynamically with fixed structure.
- Stored procedure names should be fixed literals; only parameter values come from user input.

---

### Q3. How do you call a stored procedure with Dapper?

How do you call a stored procedure with Dapper?

**Answer:** Set `commandType: CommandType.StoredProcedure` and pass the procedure name as the SQL argument, with parameters bound the same way as ad hoc queries. Dapper routes the call through ADO.NET's stored procedure execution path.

- Example: `connection.Query<Product>("usp_GetProductsByCategory", new { CategoryId = 5 }, commandType: CommandType.StoredProcedure)`.
- Output and return-value parameters require `DynamicParameters` with `ParameterDirection.Output` or `ReturnValue`.
- `Execute` and `ExecuteAsync` work for non-query procedures that do not return rowsets.
- Result shape must still match the mapped type's properties — Dapper does not infer procedure result metadata beyond column names.

---

### Q4. What is `QueryMultiple`, and when is it used?

What is `QueryMultiple`, and when is it used?

**Answer:** `QueryMultiple` executes one batch or stored procedure that returns multiple result grids and exposes them through a `GridReader`. Use it when a single round-trip should return related datasets — for example, a header row plus detail lines — instead of two separate queries.

- Reduces network latency by combining multiple `SELECT` statements or a multi-result procedure.
- Returns `SqlMapper.GridReader` (via `using var multi = connection.QueryMultiple(...)`).
- Each result set is read sequentially — you cannot skip ahead arbitrarily without reading or skipping rows in order.
- Dispose the `GridReader` promptly to release the underlying reader and connection for reuse.

---

### Q5. How do you read multiple result sets from `QueryMultiple`?

How do you read multiple result sets from `QueryMultiple`?

**Answer:** After `QueryMultiple`, call `Read<T>()`, `ReadFirst<T>()`, or `ReadFirstOrDefault<T>()` on the `GridReader` once per result set, in the order SQL Server returns them. Each `Read` consumes one grid and maps rows to `T`.

- First `multi.Read<Order>()` maps the first result set; second `multi.Read<OrderLine>()` maps the second.
- Mismatch between read order and SQL result order causes wrong-type mapping or empty sequences.
- Materialize each `Read` with `.ToList()` if you need the data after disposing the grid reader.
- Async equivalent: `QueryMultipleAsync` followed by `ReadAsync<T>()`.

---

### Q6. When would you prefer `QueryMultiple` over separate round-trips?

When would you prefer `QueryMultiple` over separate round-trips?

**Answer:** Prefer `QueryMultiple` when two or more result sets are always needed together and combining them saves measurable latency, especially over high-latency networks. Separate round-trips are simpler when result sets are optional, independently cacheable, or large enough that sequential reads would block connection reuse.

- Dashboard endpoints that always load summary plus detail in one stored procedure benefit from one trip.
- High-latency cloud database links amplify the cost of each additional round-trip.
- Separate queries allow parallel execution on different connections when the database supports it and logic is independent.
- Very large second result sets may be better streamed in a dedicated query rather than held behind the first grid reader.

---

## Chapter 04. Mapping, Multi-Mapping & Advanced Patterns

### Q1. How does Dapper map column names to property names by default?

How does Dapper map column names to property names by default?

**Answer:** Dapper matches result column names to public properties (and fields) on the target type using case-insensitive name equality. The first column value maps to the property with the same name regardless of column order in the `SELECT` list.

- `ProductName` column maps to `ProductName` property; `productname` also matches.
- Underscore naming such as `product_name` does not auto-map to `ProductName` without aliases or custom maps.
- Value types map directly; nullable value types accept database `NULL` as `null`.
- Column-to-member mapping is convention-based — no attributes required unless you add a custom type map.

---

### Q2. What happens when column names do not match property names?

What happens when column names do not match property names?

**Answer:** Unmatched columns are ignored, and unmatched properties remain at their default values (`null`, `0`, `false`). Dapper does not throw for missing mappings — silent partial objects are a common source of bugs.

- Fix with SQL aliases: `SELECT FirstName AS GivenName` to match `GivenName` property.
- Register custom column maps with `SqlMapper.SetTypeMap` or implement `ITypeHandler` for special conversions.
- Use `[Column("DbColumnName")]` when using Dapper.FluentMap or similar mapping extensions.
- Verify mappings in integration tests — empty strings where data exists often indicate a name mismatch.

---

### Q3. What is multi-mapping in Dapper?

What is multi-mapping in Dapper?

**Answer:** Multi-mapping lets one row with columns from a join map into multiple nested objects in a single `Query` call, using a delegate to assemble the parent-child graph. Overloads like `Query<Order, Customer, Order>` accept two or more types plus a `Func` that combines them.

- Typical pattern: `SELECT o.*, c.* FROM Orders o JOIN Customers c ...` mapped to `Order` with nested `Customer`.
- The split point between object types is controlled by the `splitOn` parameter.
- Dapper invokes the mapping function once per row — you decide how to attach the child to the parent.
- Useful for avoiding N+1 queries without loading flat DTOs and grouping manually in memory.

---

### Q4. What is the `splitOn` parameter in multi-mapping?

What is the `splitOn` parameter in multi-mapping?

**Answer:** `splitOn` names the column where the next object's properties begin in each row. Dapper maps columns before that name to the first type, columns from that name onward to the second type, and so on for additional generic type parameters.

- Default `splitOn` is `"Id"` — works when the second entity's first mapped column is `Id` and appears after the first entity's columns.
- Duplicate `Id` columns in a join require explicit aliases and a matching `splitOn` such as `"CustomerId"`.
- Wrong `splitOn` silently maps NULL or wrong values into nested objects instead of throwing.
- Column order in the `SELECT` list must align with the generic type order and split boundaries.

---

### Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?

How does Dapper handle nested object graphs compared to EF Core `Include`?

**Answer:** Dapper has no navigation properties or automatic graph loading — you shape graphs explicitly with SQL joins, multi-mapping, or multiple queries. EF Core `Include` and `ThenInclude` translate into join or split queries from declared relationships on the model.

- Dapper: you write the join, choose flat or multi-map, and merge duplicates in code if one parent row repeats per child.
- EF Core: relationship metadata drives eager loading; change tracker deduplicates parent instances during fix-up.
- Dapper does not lazy-load — absent columns mean absent data with no hidden round-trips.
- EF Core cartesian explosion from multiple collection includes has no Dapper equivalent unless you write the wide join yourself.
- Dapper offers precise control; EF Core offers declarative relationship traversal at the cost of translation complexity.

---

---

## Gotchas — .NET Data Access (Interview Traps)

#### Gotcha 1. String concatenation instead of parameters

**Answer:** Building SQL with `$"WHERE Id = {id}"` or string concatenation sends user input as literal SQL text, bypassing parameterization and enabling SQL injection even when the rest of the application uses an ORM or micro-ORM.

- ADO.NET and Dapper require explicit parameters — never embed raw user strings in SQL text.
- EF Core `FromSqlInterpolated` is safe; passing an ordinary interpolated string to `FromSqlRaw` is not.
- Code review should treat any dynamic SQL without placeholders as a blocking defect.

---

#### Gotcha 2. Open DataReader blocks second command

**Answer:** Running another `SqlCommand` on the same connection while a `SqlDataReader` is still open fails on SQL Server unless Multiple Active Result Sets (MARS) is enabled in the connection string.

- Always dispose or finish reading the `DataReader` before issuing the next command on that connection.
- A common bug loads a header row then tries to load detail rows on the same connection without closing the reader.
- EF Core manages readers internally, but raw ADO.NET code in the same request must respect this rule.

---

#### Gotcha 3. `AddWithValue` and wrong SQL types

**Answer:** `SqlParameter.AddWithValue` infers parameter types from CLR values, which may not match the database column type — causing implicit conversions, index scans, and poor plan cache behavior.

- Prefer explicit `SqlParameter` with `SqlDbType`, size, and precision matching the column definition.
- String inference often picks oversized `nvarchar` lengths, preventing optimal index seeks on narrower columns.
- Dapper and EF Core parameterize with more predictable typing but custom ADO.NET still needs explicit types.

---

#### Gotcha 4. Leaked connections exhaust the pool

**Answer:** Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots until timeout, eventually causing "timeout expired obtaining connection from pool" errors under load.

- Always use `await using` for connections and readers so disposal runs on exceptions too.
- Symptoms appear only under concurrent load, making this a classic production-only failure mode.
- Long-lived undisposed `DbContext` instances cause the same exhaustion pattern.

---

#### Gotcha 5. Transaction started after first command

**Answer:** Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first.

- Call `BeginTransaction` immediately after opening the connection, before any DML.
- EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly.
- Integration tests with single-user data often miss this race because implicit commits appear to "work."

---

#### Gotcha 6. Dapper `Query` without `using` on connection

**Answer:** Returning deferred `IEnumerable<T>` from Dapper before disposing the connection postpones execution until enumeration, failing at runtime or holding connections open until garbage collection.

- Materialize inside the connection scope with `.ToList()` or `.ToArray()` before returning from the method.
- Deferred execution means SQL runs when the caller iterates — often after the `using` block closed the connection.
- Async variants (`QueryAsync`) still require materialization before leaving the connection lifetime.

---

#### Gotcha 7. `QuerySingle` when zero or many rows exist

**Answer:** Dapper's `QuerySingle` throws if zero rows or more than one row match, while optional lookups typically need `QueryFirstOrDefault` which returns default when empty.

- Use `QuerySingle` only when exactly one row is a domain invariant enforced by a unique key.
- Duplicate data turns `QuerySingle` into a hard failure that `QueryFirstOrDefault` would handle differently — choose based on whether duplicates indicate bugs.
- EF Core mirrors the same distinction between `SingleOrDefault` and `FirstOrDefault`.

---

#### Gotcha 8. Multi-map `splitOn` wrong column

**Answer:** Dapper multi-mapping uses `splitOn` to name the column where the next object type begins; an incorrect column splits at the wrong boundary, silently mapping NULL or wrong values into nested objects.

- `splitOn` defaults to `"Id"` — duplicate column names in SELECT lists require explicit aliases and matching `splitOn` values.
- Align SELECT column order with the generic type order in `Query<TFirst, TSecond, TReturn>`.
- Integration tests asserting nested property values catch splitOn mistakes that unit tests on flat rows miss.

---

#### Gotcha 9. Scoped `DbContext` captured in a singleton

**Answer:** Registering a singleton service that holds a scoped `DbContext` creates a captive dependency — the context may be disposed while the singleton lives, or state leaks across HTTP requests.

- `DbContext` is scoped per request in ASP.NET Core — singletons must not store it in fields.
- Inject `IDbContextFactory<TContext>` into singletons when long-lived services need occasional database access.
- Symptoms include "Cannot access a disposed context" or cross-user data contamination in tracked entities.

---

#### Gotcha 10. Lazy loading after the context is disposed

**Answer:** Lazy loading triggers SQL when navigation properties are accessed — if that happens after the request-scoped `DbContext` is disposed, EF Core throws or the serializer triggers hidden queries that fail mid-response.

- ASP.NET Core disposes scoped contexts at the end of the request pipeline — serialization often runs near that boundary.
- Prefer explicit includes or projections inside the request scope instead of returning entity graphs with unresolved lazy navigations.
- Proxy types plus disposed contexts produce intermittent failures depending on property access order.

---

#### Gotcha 11. N+1 from lazy load or missing Include

**Answer:** Listing parent entities then accessing navigation properties in a loop without eager loading or projection fires one SQL query per parent row — classic N+1 performance collapse in EF Core APIs.

- One query for N orders plus N queries for each order's lines equals N+1 round-trips per request.
- Fix with `Include`/`ThenInclude`, split queries, or `Select` projections that join needed data in one statement.
- EF Core command logging revealing identical query templates with different IDs signals N+1 immediately.

---

#### Gotcha 12. Cartesian explosion with multiple Includes

**Answer:** Eager-loading two or more collection navigations in one SQL query multiplies result rows by the product of collection sizes, spiking memory and network use even though parent entity count is modest.

- EF Core deduplicates parents during fix-up, but SQL Server already sent the inflated rowset across the wire.
- Use `AsSplitQuery()` to fetch collections with separate SELECT statements instead of one giant join.
- Projection to DTOs avoids loading full collection graphs when only counts or summaries are needed.

---

#### Gotcha 13. Client-side evaluation of LINQ

**Answer:** Calling `ToList()` before filtering or using non-translatable C# logic in `Where` forces EF Core to pull entire tables into application memory — acceptable in development with small seeds, catastrophic in production at scale.

- EF Core 3+ throws on many accidental client evaluations instead of silently downloading whole tables.
- `AsEnumerable()` explicitly switches to LINQ to Objects — any following `Where` runs in memory.
- Rewrite with translatable expressions, `EF.Functions`, database-side filtering, or raw SQL for unsupported logic.

---

#### Gotcha 14. Tracking overhead on read-only queries

**Answer:** Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints.

- Tracking stores original and current values per property for each row materialized.
- ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection.
- Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

---

#### Gotcha 15. `SaveChanges` without a transaction for multi-step updates

**Answer:** Multiple `SaveChanges` calls or separate database operations that must succeed together commit independently by default, allowing partial updates that leave data in an inconsistent state when a later step fails.

- Wrap related saves and raw SQL in `BeginTransactionAsync`/`CommitAsync` on one `DbContext`.
- Prefer one `SaveChanges` per unit of work when all changes are tracked together on the same context.
- Retry logic after failure cannot assume earlier steps rolled back unless they shared a transaction boundary.

---

---

## Scenario-Based Answers (Karat Format)

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A junior ports the chapter's `GetActiveProducts` into an ASP.NET Core API. Review the repository method:

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

The controller calls `return Ok(_repo.GetActiveProducts());` and integration tests pass locally. What breaks under load or refactoring, and how would you fix it to match the pattern in this chapter's `ProductRepository`?

---

**Answer:**

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

The controller calls `return Ok(_repo.GetActiveProducts());` and integration tests pass locally. What breaks under load or refactoring, and how would you fix it to match the pattern in this chapter's `ProductRepository`?

**Answer:** The method returns a deferred `IEnumerable<Product>` tied to a connection that is never disposed and will eventually be closed or garbage-collected while enumeration may still be pending. That causes connection leaks, pool exhaustion under load, and intermittent `ObjectDisposedException` when the serializer enumerates after the connection is gone.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Connection lifetime | Connection opened but never disposed (`using` missing) | Leaked connections; SQL connection pool starvation under traffic |
| Runtime / correctness | Returns lazy `IEnumerable` without materializing inside the connection scope | Enumeration after dispose/close throws or returns partial data |
| Design | Caller owns enumeration timing | Fragile across controllers, middleware, and JSON serialization order |
| Scalability | One leaked connection per request | Production outages when pool max is hit; passes in low-concurrency local tests |

**Fix (priority order):**

1. Wrap the connection in `using IDbConnection connection = new SqlConnection(_connectionString);` so it returns to the pool reliably.
2. Materialize before return: `return connection.Query<Product>(sql).ToList();` — same pattern as this chapter's `ProductRepository.GetActiveProducts`.
3. Change the return type to `IReadOnlyList<Product>` so callers cannot accidentally defer enumeration past the connection lifetime.
4. Do not call `Open()` manually unless sharing an open connection inside a transaction; Dapper opens a closed connection automatically.

**Production takeaway:** Dapper's default `Query<T>` is buffered when you call `ToList()`, which is why the chapter materializes inside the `using` block. Returning raw `IEnumerable<T>` from a disposed connection is one of the most common Dapper production failures — local tests hide it because enumeration often happens immediately on the same thread.

---

---

#### Q2. (R) A search endpoint accepts a product name from the query string. Review:

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

What are the problems (security, correctness, maintainability), and what is the Dapper-native fix this chapter previews for `GetById`?

---

**Answer:**

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

What are the problems (security, correctness, maintainability), and what is the Dapper-native fix this chapter previews for `GetById`?

**Answer:** Interpolating user input into SQL is a critical SQL-injection vulnerability; Dapper's value is parameterized SQL with anonymous objects (or `DynamicParameters` in later chapters), exactly like `GetById`'s `new { productId }` pattern.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | String interpolation embeds `productName` in SQL | SQL injection — attacker can read, modify, or drop data |
| Correctness | Unescaped quotes in legitimate names (`O'Brien`) break the query | Syntax errors or wrong matches for valid input |
| Maintainability | Dynamic SQL string built by concatenation | Hard to audit, test, and reuse; bypasses Dapper's parameter binding |
| Design | Uses `$"""...'{productName}'..."""` despite Dapper parameter support | Negates a core reason to use Dapper over raw ADO.NET |

**Fix (priority order):**

1. Replace interpolation with a parameter placeholder and anonymous object:

```csharp
const string sql = """
    SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
    FROM dbo.Products
    WHERE ProductName = @productName;
    """;
return db.QueryFirstOrDefault<Product>(sql, new { productName });
```

2. Validate or length-limit `productName` at the API boundary if business rules require it — validation is not a substitute for parameterization.
3. Log and map "not found" at the service layer rather than treating SQL exceptions as control flow.

**Production takeaway:** Dapper does not prevent injection if you inject strings yourself — parameters are mandatory for any user-supplied value. The chapter's `GetById` already shows the correct pattern; search endpoints are the usual place teams regress to concatenation.

---

---

#### Q3. (M) Two developers debate connection handling before a code review. Developer A writes every repository method with `using IDbConnection db = new SqlConnection(_cs);` and never calls `Open()`. Developer B opens the connection once in a unit-of-work class and passes the open `IDbConnection` into repository methods that run inside a transaction. Both patterns appear in this chapter's demos (`ConnectionBehaviorDemo`). Under what conditions is each correct, and what state is the connection in after `Query<T>` completes when it started closed?

---

**Answer:**

**Answer:** Developer A's pattern is the default for standalone reads and writes — one short-lived connection per operation, disposed via `using`, which is pool-friendly and matches this chapter's `ProductRepository`. Developer B's pattern is correct when multiple commands must share one connection and one transaction; repositories accept the already-open `IDbConnection` instead of creating their own.

- **Developer A (closed start, per-method `using`):** Correct for independent queries with no shared transaction. Dapper opens a closed connection before executing. After `Query<T>` completes, the connection is **Open** (same ADO.NET command behavior noted in `ConnectionBehaviorDemo`). The `using` dispose closes it and returns it to the pool.
- **Developer B (caller-opened, shared connection):** Correct inside `IDbTransaction` or a unit-of-work where several `Execute`/`Query` calls must commit or roll back together. Repositories must **not** dispose a connection they did not create — only the owner disposes.
- **Wrong hybrid:** Opening per method inside a transaction scope but disposing between calls — breaks atomicity. Creating a new connection per call inside a transaction block — each call may auto-commit separately unless enlisted correctly.

**Production takeaway:** `ConnectionBehaviorDemo` shows both states are valid entry points; the decision is transactional scope, not performance superstition. Per-request `using` + `ToList()` scales cleanly in ASP.NET Core; shared open connections belong inside explicit transaction boundaries (covered further in ADO.NET ch06 and Dapper ch02 async/transaction patterns).

---

---

#### Q4. (D) Your team is building an order microservice. Writes (create order, reserve inventory) will use EF Core with migrations. A product catalog read endpoint must return hand-tuned SQL with specific indexes and no change-tracker overhead. A teammate proposes "just use EF Core everywhere for consistency." What do you recommend, how would Dapper fit, and what breaks if you force EF Core on the read path?

---

**Answer:**

**Answer:** Use EF Core for the write model and schema evolution; add Dapper for the catalog read path with explicit SQL — the hybrid pattern described in `DapperConcepts.ExplainStackComparison`. Forcing EF Core on hand-tuned reads sacrifices control and adds change-tracker overhead without benefit.

- **EF Core for writes:** Migrations, relationships, unit-of-work, and change tracking simplify order creation and inventory reservation — the team's existing choice is sound.
- **Dapper for catalog reads:** Inject the connection string (or factory), implement a thin repository like this chapter's `ProductRepository`, and keep SQL with the exact indexes and projections the read SLA requires. No tracker, minimal allocations — Dapper's micro-ORM sweet spot.
- **Shared infrastructure:** Same database, same connection string configuration; optional read replica connection string for reporting/catalog if scale demands it.
- **What breaks with EF-only reads:** LINQ may emit suboptimal SQL; `AsNoTracking` helps but you still lack fine-grained control over hints, covering indexes, and projections; teams often fight N+1 or over-fetching on hot paths.
- **Consistency argument:** Consistency belongs in boundaries (DTO contracts, error shape, logging), not in forcing one ORM for every access pattern. Document when to reach for EF vs Dapper in the repo README or ADR.

**Production takeaway:** Many production systems combine EF Core and Dapper on one database — this chapter's comparison table is the decision framework Karat expects, not a single-tool mandate.

---

---

#### Q5. (R) A legacy `dbo.Products` table uses snake_case column names. A new Dapper DTO maps cleanly in tests against LocalDB seed data, but staging returns rows with empty names and zero prices:

```csharp
public sealed class ProductListItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}

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

No exceptions are thrown. Diagnose the failure mode and give two production-safe fixes (one SQL-side, one mapping-side for later chapters).

---

**Answer:**

```csharp
public sealed class ProductListItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}

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

No exceptions are thrown. Diagnose the failure mode and give two production-safe fixes (one SQL-side, one mapping-side for later chapters).

**Answer:** Dapper maps by column name to property name (case-insensitive by default). `product_id` may map to `ProductId`, but `product_name` and `unit_price` do not match `ProductName` and `UnitPrice`, so those properties stay at default values — silent data loss with no exception, exactly the convention-mapping gotcha in `Models/Product.cs` and the chapter quick reference.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Mapping / correctness | Snake_case columns vs PascalCase properties | `ProductName` empty, `UnitPrice` 0 — API returns garbage data |
| Observability | No exception on mismatch | Bug reaches production; hard to spot without assertion tests |
| Environment | LocalDB seed used PascalCase columns; staging legacy schema differs | "Works on my machine" — schema drift between environments |
| Design | Assumes convention mapping without verifying column names | Fragile when integrating legacy databases |

**Fix (priority order):**

1. **SQL-side (preferred for reads):** Alias columns to match properties:

```sql
SELECT product_id AS ProductId,
       product_name AS ProductName,
       unit_price AS UnitPrice
FROM dbo.Products
WHERE discontinued_date IS NULL;
```

2. **Mapping-side (later chapter):** Custom column maps or underscore matching rules in Dapper ch04 — register a map so `product_name` → `ProductName` globally for legacy schemas.
3. Add integration tests against a schema that mirrors staging, asserting non-default `ProductName` and `UnitPrice` for seeded rows.
4. Optionally project to a dynamic or intermediate type during migration — short-term bridge only.

**Production takeaway:** Dapper's silent default on unmapped columns is worse than a thrown exception — always verify column/property alignment when connecting to legacy schemas. SQL aliases are the fastest production fix; custom maps pay off when many queries hit the same legacy naming.

---

---

#### Q6. (P) You register data access in ASP.NET Core DI for a Dapper-based `ProductRepository` like the one in this chapter. A teammate submits:

```csharp
// Program.cs
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AdoNetTutorial")!));

// ProductRepository.cs — refactored constructor
public ProductRepository(IDbConnection connection) => _connection = connection;
```

What fails at startup or under concurrent traffic, and how should connection strings, repository lifetime, and `IDbConnection` be wired instead?

---

**Answer:**

```csharp
// Program.cs
builder.Services.AddSingleton<ProductRepository>();
builder.Services.AddSingleton<IDbConnection>(_ =>
    new SqlConnection(builder.Configuration.GetConnectionString("AdoNetTutorial")!));

// ProductRepository.cs — refactored constructor
public ProductRepository(IDbConnection connection) => _connection = connection;
```

What fails at startup or under concurrent traffic, and how should connection strings, repository lifetime, and `IDbConnection` be wired instead?

**Answer:** A singleton `IDbConnection` is not thread-safe for concurrent requests — multiple threads executing Dapper calls on one shared `SqlConnection` cause undefined behavior, timeouts, and corrupted reads. Even if thread safety were solved, a long-lived connection bypasses pooling best practices and breaks per-operation dispose semantics this chapter teaches.

- **Singleton `SqlConnection`:** SQL Server connections are not designed for concurrent multi-threaded use on one instance. Under parallel API requests, queries interleave on shared state — intermittent failures that load tests expose immediately.
- **Singleton repository holding connection:** Same captive dependency problem — repository lifetime extends connection lifetime across all users and requests.
- **Missing dispose:** A singleton connection is never disposed until shutdown; if it drops, the whole app shares one broken connection.
- **Correct lifetime:** Register `ProductRepository` as **scoped** (per request) or **transient**; do **not** register `IDbConnection` in DI for typical per-query usage.
- **Correct wiring:** Inject `IConfiguration` or `IOptions<ConnectionStrings>` (or a small `IDbConnectionFactory`) and create `using IDbConnection db = new SqlConnection(_cs)` inside each method — identical to this chapter's repository. For tests, inject a connection string pointing at LocalDB or accept `IDbConnection` only in test doubles.

```csharp
builder.Services.AddScoped<ProductRepository>();

public ProductRepository(IOptions<DatabaseOptions> options)
{
    _connectionString = options.Value.AdoNetTutorial
        ?? throw new InvalidOperationException("Connection string missing.");
}
```

**Production takeaway:** Dapper's simplicity is "create connection, query, dispose per operation." DI should inject **configuration**, not a shared connection — the chapter's constructor-stores-connection-string pattern scales correctly into ASP.NET Core.

---

---

#### Q7. (R) A developer coming from EF Core "fixes" update logic using Dapper. Review:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    var product = GetById(productId);          // QueryFirstOrDefault<Product>
    if (product is null) return;
    product = product with { UnitPrice = newPrice };  // record copy — Product is not a record here
    // "Dapper tracks changes like EF" — no Execute call
}

public Product? GetById(int id)
{
    IDbConnection db = new SqlConnection(_connectionString);
    return db.QueryFirstOrDefault<Product>(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products WHERE ProductId = @id",
        new { id });
}
```

Identify compile/runtime/DI issues and explain why the price never persists — tying back to what Dapper actually is in this chapter.

---

### 02. Queries, Execute & Async Methods

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/02. Queries, Execute & Async Methods`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    var product = GetById(productId);          // QueryFirstOrDefault<Product>
    if (product is null) return;
    product = product with { UnitPrice = newPrice };  // record copy — Product is not a record here
    // "Dapper tracks changes like EF" — no Execute call
}

public Product? GetById(int id)
{
    IDbConnection db = new SqlConnection(_connectionString);
    return db.QueryFirstOrDefault<Product>(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products WHERE ProductId = @id",
        new { id });
}
```

Identify compile/runtime/DI issues and explain why the price never persists — tying back to what Dapper actually is in this chapter.

**Answer:** Dapper is a stateless micro-ORM — it maps query rows to POCOs and executes explicit commands; there is no change tracker, so modifying an in-memory object without an `Execute`/`UPDATE` never touches the database. The snippet also fails to compile (`with` on a non-record) and leaks connections in `GetById`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Compile | `product with { ... }` on `Product` sealed class (not `record`) | CS8858 — does not build |
| Correctness | No `Execute`/`UPDATE` after in-memory mutation | Price never persisted — functional bug invisible in unit tests that mock the repo |
| Design | Assumes EF-style change tracking | Violates Dapper's model — queries are read-only projections unless followed by explicit SQL |
| Connection lifetime | `GetById` creates `SqlConnection` without `using` | Connection leak on every read |
| Maintainability | Read-modify-write without transaction | Race conditions under concurrent price updates even after adding `Execute` |

**Fix (priority order):**

1. Replace the EF mental model with an explicit update:

```csharp
public void UpdatePrice(int productId, decimal newPrice)
{
    using IDbConnection db = new SqlConnection(_connectionString);
    const string sql = "UPDATE dbo.Products SET UnitPrice = @newPrice WHERE ProductId = @productId;";
    db.Execute(sql, new { productId, newPrice });
}
```

2. Fix `GetById` with `using IDbConnection` — same as this chapter's implementation.
3. If read-modify-write is required (validation rules on current row), wrap read + update in a transaction on one open connection.
4. On the mutation path, mutate properties directly if needed (`product.UnitPrice = newPrice` won't compile with `init` — another signal the EF pattern was copied blindly); prefer parameter-only updates without loading the entity when possible.

**Production takeaway:** The chapter quick reference lists "Expect EF-style change tracking" as a common mistake — Dapper returns disconnected POCOs. Updates require explicit `Execute` SQL; that is a feature (predictable SQL, no hidden round-trips), not a missing feature.

---

### 02. Queries, Execute & Async Methods

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/02. Queries, Execute & Async Methods`

---

---

#### Q1. (R) A catalog API exposes "get product by category." A teammate ships this repository method. Review it — what breaks in production as the catalog grows, and what Dapper API would you use instead?

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

---

**Answer:**

**Answer:** `QuerySingle` throws `InvalidOperationException` when more than one row matches — fine for PK lookups, wrong for a non-unique `CategoryId` filter once a category has two or more products.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | `QuerySingle` on a non-unique key | Intermittent 500s when a second product shares the category |
| API design | Returns one `Product` for a one-to-many relationship | Wrong contract — callers cannot list or choose among matches |
| Dapper semantics | Misapplied `QuerySingle` vs `QueryFirst` | `QueryFirst` would hide duplicates silently; neither fixes the domain model |

**Fix (priority order):**

1. Change the contract: return `IReadOnlyList<Product>` via `Query<Product>` (or `QueryAsync` + `ToList()`) when many rows are valid.
2. If the business rule is truly "one product per category," enforce it in the database (unique constraint on `CategoryId`) and keep `QuerySingle` — or use `QuerySingleOrDefault` when absence is acceptable.
3. If you only need an arbitrary representative row, use `QueryFirst` with explicit `TOP 1` and `ORDER BY` — document that choice in the API.

**Production takeaway:** Karat tests whether you match Dapper's single-row helpers to **SQL uniqueness guarantees** — PK/`QuerySingle`, `TOP 1`/`QueryFirst`, many rows/`Query<T>`.

---

---

#### Q2. (R) An export endpoint needs to stream millions of product rows with minimal memory. A developer refactors the repository like this. Review the full path — what fails at runtime, and why does the bug pass unit tests that mock `IEnumerable<Product>`?

```csharp
public IEnumerable<Product> StreamAllActive()
{
    using var connection = new SqlConnection(_connectionString);
    connection.Open();
    return connection.Query<Product>(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL;
        """,
        buffered: false);
}

// Controller
public IActionResult Export()
{
    var rows = _repository.StreamAllActive();
    return Ok(rows.Select(p => MapToDto(p))); // deferred LINQ over deferred Dapper query
}
```

---

**Answer:**

**Answer:** `buffered: false` keeps a live `SqlDataReader` tied to the connection; disposing the connection inside `StreamAllActive` before the controller enumerates causes read failures — mocks never open a real reader, so tests stay green.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Connection lifetime | `using` closes connection before deferred enumeration | `InvalidOperationException` or "Invalid attempt to call Read when reader is closed" in production |
| Deferred execution | Dapper SQL runs on first `MoveNext`, not at `Query` call | Failure happens in the controller, far from the repository — hard to diagnose |
| Testability | Mock `IEnumerable<Product>` returns an in-memory list | Hides the connection/reader coupling entirely |
| API / hosting | Returning deferred `IEnumerable` across layer boundaries | ASP.NET serialization may force full enumeration on a dead connection |

**Fix (priority order):**

1. Keep the connection open for the entire enumeration — e.g., pass an open scoped connection into the repository and enumerate inside the same request scope, **or** materialize inside the method with `buffered: true` / `.ToList()` when the set is bounded.
2. For true streaming at scale, use `buffered: false` but consume rows **inside** the repository or a dedicated streaming abstraction (`IAsyncEnumerable<Product>` with `await foreach`, connection open until the stream completes).
3. Add an integration test that uses a real `SqlConnection` and defers enumeration past the repository return — catches this class of bug.

**Production takeaway:** Unbuffered Dapper queries are a **connection-lifetime contract** — the tutorial's `DemonstrateUnbufferedConnectionRequirement` pitfall exactly; default `buffered: true` is safer unless you control the full read pipeline.

---

---

#### Q3. (R) A health-check endpoint reports inventory count. Review this action — identify compile-time, runtime, and scalability issues:

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

---

**Answer:**

**Answer:** Blocking on `.Result` for `ExecuteScalarAsync` defeats async I/O, risks thread-pool starvation under load, and offers no cancellation — the sync `using` connection also fights ASP.NET Core's async request model.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.Result` on `ExecuteScalarAsync` | Sync-over-async; thread blocked during network I/O |
| Scalability | Sync action + blocked thread per health probe | Orchestrator polling can saturate the thread pool |
| Hosting | `using var connection` in sync action | Works locally; composes poorly with async middleware and `CancellationToken` |
| Resilience | No `CancellationToken` passed to Dapper | Hung SQL calls cannot abort when the client disconnects |

**Fix (priority order):**

1. Make the action async: `public async Task<IActionResult> GetActiveProductCount(CancellationToken ct)` and `await connection.ExecuteScalarAsync<int>(sql, cancellationToken: ct)` (via `CommandDefinition` when you need token support — see Dapper ch03).
2. Prefer injecting a scoped repository/service rather than opening `SqlConnection` inline in the controller.
3. Remove `.Result` / `.Wait()` entirely — async end-to-end from controller through Dapper.

**Production takeaway:** Dapper's `*Async` siblings exist for the same reason as ADO.NET async — use `await ExecuteScalarAsync`, not `.Result`. See C# Module 06 — sync-over-async in ASP.NET actions.

---

---

#### Q4. (M) Two junior developers argue about the right Dapper call for these operations. For each snippet, name the **correct** Dapper method (`Query`, `QueryFirst`, `Execute`, or `ExecuteScalar`) and what goes wrong if they ship as written:

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
    """
    INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity)
    VALUES (@id, @name, @price, @stock);
    """,
    new { id = 99, name = "Cable", price = 9.99m, stock = 50 });
bool inserted = result.Any();
```

---

**Answer:**

**Answer:** Each snippet uses the wrong Dapper entry point — `Execute`/`ExecuteScalar`/`Query` are not interchangeable; picking the wrong one yields cast exceptions, empty sequences, or silent logic bugs.

- **A — UPDATE rows affected:** Use **`Execute`**. `ExecuteScalar` returns the first column of the first row — for a plain `UPDATE` that is often `NULL` or meaningless, and casting to `int` is fragile. `Execute` returns `rows affected` directly.
- **B — single aggregate:** Use **`ExecuteScalar<decimal>`** (or `ExecuteScalar<decimal?>` with null-coalescing when no rows match). `Query<decimal>` materializes a result set and `.First()` throws on empty sets; aggregates belong on the scalar path.
- **C — INSERT success:** Use **`Execute`** and compare `rowsAffected == 1`. `Query<int>` expects a `SELECT` result set mapping to `int`; a bare `INSERT` returns no rows to map — `result` is empty, so `inserted` is always `false` even when the insert succeeded.

**Production takeaway:** **`Execute`** = DML rows affected; **`ExecuteScalar<T>`** = one cell (COUNT, AVG, `SCOPE_IDENTITY()`); **`Query<T>`** = many mapped rows. Mixing them "because it compiles" is a common Karat trap.

---

---

#### Q5. (P) Your ASP.NET Core API uses scoped `SqlConnection` per request. A repository method mirrors the tutorial's async list load but returns `IEnumerable<Product>` directly from `QueryAsync`. Under load, callers intermittently see `InvalidOperationException` ("There is already an open DataReader…"). Explain the mechanism and show the production-safe pattern (including when to materialize vs stream).

```csharp
public async Task<IEnumerable<Product>> GetAllActiveAsync(SqlConnection connection)
{
    const string sql = """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
        FROM dbo.Products
        WHERE DiscontinuedDate IS NULL
        ORDER BY ProductName;
        """;

    return await connection.QueryAsync<Product>(sql);
}
```

---

**Answer:**

**Answer:** `QueryAsync` returns a deferred sequence backed by an open reader on the shared scoped connection; if anything else uses that connection (second query, retry, logging) before enumeration finishes, SQL Server rejects overlapping readers on one connection.

- **Mechanism:** Default MARS off on many connection strings — one active reader per connection. Returning deferred `IEnumerable` from a repository exports the reader lifetime to unknown callers.
- **Production-safe (typical API list):** Materialize before returning, matching the tutorial's `GetAllActiveAsync`:

```csharp
public async Task<IReadOnlyList<Product>> GetAllActiveAsync(SqlConnection connection)
{
    const string sql = "...";
    IEnumerable<Product> rows = await connection.QueryAsync<Product>(sql);
    return rows.ToList(); // reader closed before caller runs another command
}
```

- **When to stream:** Use `buffered: false` or `IAsyncEnumerable<Product>` only when the **same** code path owns the connection until enumeration completes (export job, manual `await foreach`), not when returning bare `IEnumerable` to controllers or other services.
- **Alternative:** Separate connection for the streaming read, or enable MARS deliberately — still prefer explicit materialization for small/medium result sets.

**Production takeaway:** Treat `Query`/`QueryAsync` return values like ADO.NET readers — either finish reading inside the repository or hand back a fully materialized collection.

---

---

#### Q6. (D) The tutorial's `InsertProductReturningId` uses `MAX(ProductId) + 1` inside a batch and returns the new id via `ExecuteScalar<int>`. A teammate says "works in dev, ship it." Two API instances insert products concurrently under load. What breaks, and what pattern replaces both the id generation **and** the Dapper call sequence?

```csharp
const string sql = """
    DECLARE @NextId INT = (SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products);
    INSERT INTO dbo.Products (ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
    VALUES (@NextId, @productName, @unitPrice, @stockQuantity, NULL);
    SELECT @NextId;
    """;
int newId = connection.ExecuteScalar<int>(sql, new { productName, unitPrice, stockQuantity });
```

---

**Answer:**

**Answer:** Concurrent transactions can compute the same `@NextId`, causing primary-key violations or lost updates — `ExecuteScalar` is fine for returning an id, but **`MAX + 1` is not a safe id strategy** under concurrency.

- **What breaks:** Two requests read the same `MAX(ProductId)` before either commits → duplicate `ProductId` insert → one request fails with PK violation, or worse behavior under weaker isolation.
- **Replace id generation:** Use an **`IDENTITY`** (or **`SEQUENCE`**) column on `ProductId` and let SQL Server allocate ids — remove manual `MAX + 1`.
- **Replace Dapper sequence:** Single batch with **`ExecuteScalar<int>`**:

```csharp
const string sql = """
    INSERT INTO dbo.Products (ProductName, UnitPrice, StockQuantity, DiscontinuedDate)
    VALUES (@productName, @unitPrice, @stockQuantity, NULL);
    SELECT CAST(SCOPE_IDENTITY() AS INT);
    """;
int newId = await connection.ExecuteScalarAsync<int>(sql, param);
```

- Prefer **`SCOPE_IDENTITY()`** over `@@IDENTITY` (trigger-safe in scope). Wrap insert + follow-up DML in an explicit **`IDbTransaction`** passed to `Execute`/`ExecuteScalar` when multiple statements must commit together.

**Production takeaway:** Dapper does not fix application-level race conditions — **`ExecuteScalar` returns whatever your SQL makes atomic**; id generation must be delegated to the database or a serialized sequence.

---

---

#### Q7. (P) A batch job must insert a product **and** read the new `ProductId` in one round trip, then update a related audit row — all-or-nothing. The developer runs two separate Dapper calls on the same open connection without an explicit transaction. When does that silently corrupt data, and how do you wire `Execute` / `ExecuteScalar` correctly with `IDbTransaction`?

---

### 03. Parameters, Stored Procedures & QueryMultiple

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/03. Parameters, Stored Procedures & QueryMultiple`
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

_Answer not found._

---

#### Q1. (R) A junior developer ships a product search endpoint backed by this Dapper repository method. Security review flags it before deploy. What is wrong, and how do you fix it without changing the public method signature?

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

---

**Answer:**

**Answer:** The SQL is built with C# string interpolation, so user input is concatenated into the command text — classic SQL injection (`'; DROP TABLE dbo.Products; --`). Dapper only parameterizes when you pass a separate param object; interpolated values are literal T-SQL.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `searchTerm` embedded in SQL via `$"..."` | SQL injection; attacker can read/modify data |
| Correctness | No `@searchTerm` token bound by Dapper | Provider sends one non-parameterized batch |
| Design | Looks like Dapper but bypasses parameter API | False sense of safety in code review |

**Fix (priority order):**

1. Replace interpolation with a parameterized predicate: `WHERE ProductName LIKE @Pattern` and pass `new { Pattern = $"%{searchTerm}%" }` (or `'%' + @term + '%'` pattern built in C# as the **parameter value**, not in SQL text).
2. Optionally add input length/character validation at the API layer — defense in depth, not a substitute for parameters.
3. Log and monitor for suspicious search strings; consider `CommandDefinition` with a fixed timeout for search endpoints.

**Production takeaway:** Dapper does not auto-sanitize interpolated SQL — only named parameters (`new { ... }`, `DynamicParameters`) get sent as `SqlParameter` values. See ADO.NET ch.03 SqlCommand & Parameters for the same rule on raw ADO.NET.

---

---

#### Q2. (R) Code review on a stored-procedure insert path that mirrors `DynamicParameterRepository.InsertProduct`. The author claims it works in a one-off SSMS test. What breaks at runtime or in production, and in what order do you fix it?

```csharp
public int InsertProduct(string productName, decimal unitPrice, int stockQuantity)
{
    using var connection = new SqlConnection(_connectionString);
    var dp = new DynamicParameters();
    dp.Add("@ProductName", productName);
    dp.Add("@UnitPrice", unitPrice);
    dp.Add("@StockQuantity", stockQuantity);
    dp.Add("@NewProductId", direction: ParameterDirection.Output);

    int newId = dp.Get<int>("@NewProductId"); // read identity before execute

    connection.Execute(
        "dbo.usp_InsertProduct",
        dp,
        commandType: CommandType.StoredProcedure);

    return newId;
}
```

---

**Answer:**

**Answer:** OUTPUT parameters are populated **after** `Execute` completes; reading `@NewProductId` beforehand returns default `0`. The `Add` call also omits `dbType: DbType.Int32`, which can cause provider inference issues for Output parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | `dp.Get<int>("@NewProductId")` before `Execute` | Always returns 0; wrong IDs returned to callers |
| ADO.NET semantics | Output without explicit `DbType` | Possible type/size mismatch on some providers |
| Data integrity | Caller may attach downstream FKs to id `0` | Silent corruption or constraint violations |

**Fix (priority order):**

1. Move `dp.Get<int>("@NewProductId")` to **after** `connection.Execute(...)`.
2. Declare Output explicitly: `dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output)`.
3. Add a guard: if `newId <= 0` after execute, treat as failure and log — do not propagate bogus keys.

Correct pattern (matches this chapter's `DynamicParameterRepository`):

```csharp
dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_InsertProduct", dp, commandType: CommandType.StoredProcedure);
return dp.Get<int>("@NewProductId");
```

**Production takeaway:** SSMS manual runs hide ordering bugs — Dapper mirrors ADO.NET "execute then read Output/ReturnValue." Karat often stacks direction + read-timing traps in one snippet.

---

---

#### Q3. (R) A dashboard API calls `usp_GetProductDashboard` but returns wrong totals after a DBA reorders the SELECT statements inside the procedure. Review this consumer — what fails silently vs throws, and how do you harden it?

```csharp
public DashboardDto GetDashboard()
{
    using var connection = new SqlConnection(_connectionString);
    using var multi = connection.QueryMultiple(
        "dbo.usp_GetProductDashboard",
        commandType: CommandType.StoredProcedure);

    int totalCount = multi.Read<int>().Single();           // expects COUNT(*) first
    var products = multi.Read<Product>().AsList();         // expects product rows second
    string topName = multi.ReadFirst<string>();            // expects TOP 1 name third

    return new DashboardDto(products, totalCount, topName);
}
```

---

**Answer:**

**Answer:** `GridReader.Read<T>()` is **order-dependent** and forward-only — it always consumes the next result set in the batch. Reordering proc result sets maps columns to the wrong CLR types; some mismatches throw at read time, others produce garbage counts or empty strings silently.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Assumes fixed result-set order (count → products → name) | Wrong dashboard numbers after harmless proc change |
| Contract | No versioned proc/consumer agreement | DBA refactor breaks API without compile error |
| Resilience | `Read<int>()` on a product-shaped first set | Mapping exceptions or nonsense `int` values |

**Fix (priority order):**

1. Align consumer read order with the **documented** proc contract — match this chapter's `QueryMultipleRepository.GetDashboardFromProcedure` (products → count → name).
2. Document result-set order in proc header comment and integration test (assert column counts/shapes per `Read` call).
3. Prefer stable column aliases in each SELECT (`AS TotalCount`, `AS MostExpensive`) and consider separate procs or JSON single-result if order churn is frequent.
4. On shape mismatch, catch read exceptions, log proc name + set index, return 503 — do not silently show wrong totals.

**Production takeaway:** `QueryMultiple` saves round trips but couples client and server on **set order** — same constraint as `SqlDataReader.NextResult()` in ADO.NET ch.04. `StoredProcedureRepository.GetAllViaStoredProcedure` shows `Query<T>` only reads the **first** set — a related footgun.

---

---

#### Q4. (M) A filter query returns every row instead of the intended price band. The SQL and anonymous object look correct at a glance. What is the binding bug?

```csharp
const string sql = """
    SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate
    FROM dbo.Products
    WHERE UnitPrice >= @MinPrice AND UnitPrice <= @MaxPrice
    ORDER BY UnitPrice;
    """;

using var connection = new SqlConnection(_connectionString);
return connection.Query<Product>(sql, new { MinimumPrice = minPrice, MaximumPrice = maxPrice });
```

---

**Answer:**

**Answer:** Dapper binds by **name**: SQL tokens `@MinPrice` and `@MaxPrice` require properties `MinPrice` and `MaxPrice`. The anonymous object uses `MinimumPrice` / `MaximumPrice`, so those parameters are never supplied — SQL Server may treat missing predicates as unconstrained or error depending on plan, often returning all rows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Property names ≠ `@MinPrice` / `@MaxPrice` | Filter ignored; full table scan |
| Performance | Unfiltered query | Memory pressure, slow API, wrong business results |
| Maintainability | "Looks parameterized" but misbound | Hard to spot in review |

**Fix (priority order):**

1. Rename properties to match SQL: `new { MinPrice = minPrice, MaxPrice = maxPrice }` — as in `AnonymousParameterRepository.GetByPriceRange`.
2. Or alias in SQL: `@MinimumPrice` / `@MaximumPrice` to match the object.
3. Add an integration test with known seed data asserting row count within band.

**Production takeaway:** Parameterization prevents injection but **name alignment** is still required — case-insensitive match, exact token names. See ADO.NET ch.03 parameter naming and this chapter's Section 2 comments.

---

---

#### Q5. (P) An ASP.NET Core endpoint wraps a long-running report stored procedure. The controller passes `HttpContext.RequestAborted` as cancellation. Review the repository — what is missing for timeout and cooperative cancellation, and how would you wire `CommandDefinition` correctly?

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

---

**Answer:**

**Answer:** The snippet uses default command timeout (often 30s on `SqlConnection`) and does not pass `CancellationToken` into Dapper — client disconnect will not cancel the SQL command, and long procs can hold pool connections until default timeout.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting / scalability | No `commandTimeout` override | Report exceeds 30s → SqlException; pool exhaustion under load |
| Async / cancellation | `QueryAsync` without token in `CommandDefinition` | Request aborted but DB work continues |
| Operations | No explicit timeout policy per report | Unpredictable SLA; hung requests |

**Fix (priority order):**

1. Build a `CommandDefinition` with explicit timeout and cancellation:

```csharp
var cmd = new CommandDefinition(
    "dbo.usp_SlowProductReport",
    parameters: null,
    commandTimeout: 120, // seconds — tune to SLA
    commandType: CommandType.StoredProcedure,
    cancellationToken: cancellationToken);

return (await connection.QueryAsync<Product>(cmd)).AsList();
```

2. Open connection with `OpenAsync(cancellationToken)` (already present) and ensure the controller passes `HttpContext.RequestAborted`.
3. Map `OperationCanceledException` / `SqlException` timeout number `-2` to 499/504 with structured logging.
4. For very heavy reports, switch to async job + polling rather than stretching timeout indefinitely.

**Production takeaway:** `CommandDefinition` is the Dapper hook for timeout, `CommandType`, transaction, flags, and **cancellation** — same bundle this chapter's `CommandDefinitionRepository` uses for `commandTimeout: 30`. See Dapper ch.02 for async overloads.

---

---

#### Q6. (D) A teammate refactors `GetProductCountViaReturnValue` to use an anonymous object because "ReturnValue is just another int param." They change the proc caller to:

```csharp
var count = connection.ExecuteScalar<int>(
    "dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);
```

Separately, another method tries to read a T-SQL `RETURN` with:

```csharp
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status"); // proc uses RETURN 0 / RETURN 1, not OUTPUT @Status
```

Explain what each approach gets wrong about T-SQL `RETURN` vs `OUTPUT`, and what Dapper API you use for each.

---

**Answer:**

```csharp
var count = connection.ExecuteScalar<int>(
    "dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);
```

Separately, another method tries to read a T-SQL `RETURN` with:

```csharp
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status"); // proc uses RETURN 0 / RETURN 1, not OUTPUT @Status
```

Explain what each approach gets wrong about T-SQL `RETURN` vs `OUTPUT`, and what Dapper API you use for each.

**Answer:** T-SQL `RETURN` sends an integer through a **ReturnValue** parameter, not through result sets or arbitrary OUTPUT params. `ExecuteScalar` reads the first result set's first column — `usp_GetProductCount` has no SELECT, so scalar is wrong. The second snippet treats `RETURN 1` as `@Status OUTPUT`, which never receives the return code.

- **`RETURN` (status code):** `DynamicParameters` with `direction: ParameterDirection.ReturnValue` and a dummy name like `@ReturnValue`, then `dp.Get<int>("@ReturnValue")` after `Execute` — as in `DynamicParameterRepository.GetProductCountViaReturnValue` and `TryGetProductName`.
- **`OUTPUT` parameter:** `@NewProductId INT OUTPUT` or `@ProductName NVARCHAR(100) OUTPUT` — `dp.Add` with `ParameterDirection.Output`, explicit `DbType` and **size for strings** (`size: 100`), read after execute — as in `InsertProduct` / `GetProductName`.
- **`ExecuteScalar`:** Use when the proc or batch **SELECTs** a single value (e.g. `SELECT COUNT(*)`), not for `RETURN`.
- **Anonymous objects:** Fine for **Input** only — cannot declare Output/ReturnValue direction; use `DynamicParameters` or `AddDynamicParams` plus directional adds (see `TryGetProductName` merging input via `AddDynamicParams`).

**Production takeaway:** Karat tests whether you know ADO.NET parameter directions under Dapper syntax — `RETURN`, `OUTPUT`, and result sets are three different channels. Confusing them passes compile and fails in QA with status always 0.

---

---

#### Q7. (R) A batch import service loads a dashboard in one round trip but intermittently throws `InvalidOperationException` under load. Review the method — identify lifetime/reader issues and the fix.

```csharp
public (IEnumerable<Product> Products, int Total, string TopName) GetDashboardLazy()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();

    var multi = connection.QueryMultiple(
        """
        SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;
        SELECT COUNT(*) FROM dbo.Products;
        SELECT TOP 1 ProductName FROM dbo.Products ORDER BY UnitPrice DESC;
        """);

    var products = multi.Read<Product>();  // deferred IEnumerable — connection still open
    int total = multi.Read<int>().Single();
    string top = multi.ReadFirst<string>();

    return (products, total, top); // caller enumerates products after method returns
}
```

---

### 04. Mapping, Multi-Mapping & Advanced Patterns

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/02. Dapper/04. Mapping, Multi-Mapping & Advanced Patterns`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** The method returns a **deferred** `IEnumerable<Product>` from `multi.Read<Product>()` while the `GridReader`, connection, and reader are disposed when the method exits. When the caller enumerates `products`, the underlying reader is closed — classic "reader is closed" / invalid operation intermittently depending on timing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | No `using` on connection / `GridReader` | Connection leak under load; pool starvation |
| Reader lifetime | Deferred `IEnumerable` outlives `multi` | `InvalidOperationException` on enumeration |
| QueryMultiple rules | Must finish all `Read` calls and materialize before dispose | Partial reads leave reader in bad state |

**Fix (priority order):**

1. Wrap connection and grid reader in `using` — pattern from `QueryMultipleRepository.GetDashboardFromInlineBatch`.
2. Materialize before return: `var products = multi.Read<Product>().AsList();` (or `.ToList()`).
3. Complete all `Read` calls in order before leaving the `using` block.
4. Return `IReadOnlyList<Product>` (or DTO) — not live `IEnumerable` tied to SQL reader.

```csharp
using var connection = new SqlConnection(_connectionString);
using var multi = connection.QueryMultiple(sql);
var products = multi.Read<Product>().AsList();
int total = multi.Read<int>().Single();
string top = multi.ReadFirst<string>();
return (products, total, top);
```

**Production takeaway:** Dapper's default `Query` buffering does not apply the same way if you return unmaterialized sequences from `QueryMultiple`. Forward-only `GridReader` semantics match ADO.NET `NextResult` — consume and buffer inside the `using` scope.

---

### 04. Mapping, Multi-Mapping & Advanced Patterns

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/04. Mapping, Multi-Mapping & Advanced Patterns`

---

---

#### Q1. (R) A teammate ships a "fix" to `GetOrdersWithCustomers` that removes the explicit `splitOn` because both sides have an `Id`-like key. Review the change:

```csharp
public IReadOnlyList<Order> GetOrdersWithCustomers()
{
    const string sql = """
        SELECT
            o.OrderId,
            o.OrderDate,
            o.TotalAmount,
            o.CustomerId,
            c.CustomerId,
            c.Name,
            c.Email
        FROM dbo.Orders o
        INNER JOIN dbo.Customers c ON o.CustomerId = c.CustomerId
        ORDER BY o.OrderId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    return connection.Query<Order, Customer, Order>(
        sql,
        (order, customer) =>
        {
            order.Customer = customer;
            return order;
        }).ToList(); // splitOn removed — Dapper defaults to "Id"
}
```

Orders load in QA, but `Customer.Name` is null and `Customer.CustomerId` equals `Order.OrderId` on several rows. What is wrong, and how do you fix it with minimal SQL change?

---

**Answer:**

```csharp
public IReadOnlyList<Order> GetOrdersWithCustomers()
{
    const string sql = """
        SELECT
            o.OrderId,
            o.OrderDate,
            o.TotalAmount,
            o.CustomerId,
            c.CustomerId,
            c.Name,
            c.Email
        FROM dbo.Orders o
        INNER JOIN dbo.Customers c ON o.CustomerId = c.CustomerId
        ORDER BY o.OrderId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    return connection.Query<Order, Customer, Order>(
        sql,
        (order, customer) =>
        {
            order.Customer = customer;
            return order;
        }).ToList(); // splitOn removed — Dapper defaults to "Id"
}
```

Orders load in QA, but `Customer.Name` is null and `Customer.CustomerId` equals `Order.OrderId` on several rows. What is wrong, and how do you fix it with minimal SQL change?

**Answer:** Dapper splits at the **first column whose name matches `splitOn`** — default `"Id"` is not present, so the next ambiguous match is the **first `CustomerId`**, which belongs to `Order`, not `Customer`. Everything after that column is mapped into `Customer`, so `Name`/`Email` never bind and `Customer.CustomerId` receives the wrong slice of the row.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / mapping | Omitted `splitOn` with duplicate `CustomerId` columns | Split point lands on Order's FK, not Customer's columns |
| Correctness | `Customer` object partially or wrongly populated | Silent data corruption in API responses |
| Maintainability | Implicit default `"Id"` on JOIN queries | Breaks when schema adds/removes `Id`-named columns |

**Fix (priority order):**

1. Restore an explicit `splitOn` on a **Customer-only** column — as in **MultiMapRepository.cs**: `splitOn: "Name"` (Orders have no `Name` column).
2. Alternatively, alias the second key: `c.CustomerId AS CustCustomerId` and use `splitOn: "CustCustomerId"`.
3. Keep column order: all `Order` columns first, then all `Customer` columns — never rely on default `splitOn` for JOINs.

**Production takeaway:** `splitOn` is not optional on real JOINs; duplicate column names are the most common multi-map production bug. Prefer a unique TSecond column or SQL alias over guessing defaults.

---

---

#### Q2. (R) Another developer models order lines without the dictionary lookup from this chapter:

```csharp
public Order? GetOrderWithLines(int orderId)
{
    const string sql = """
        SELECT o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount,
               ol.OrderLineId, ol.OrderId, ol.ProductId, ol.Quantity, ol.LineTotal
        FROM dbo.Orders o
        INNER JOIN dbo.OrderLines ol ON o.OrderId = ol.OrderId
        WHERE o.OrderId = @orderId
        ORDER BY ol.OrderLineId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    List<Order> orders = connection.Query<Order, OrderLine, Order>(
        sql,
        (order, line) =>
        {
            order.Lines.Add(line);
            return order;
        },
        new { orderId },
        splitOn: "OrderLineId").ToList();

    return orders.FirstOrDefault();
}
```

The API returns three lines for order 1, but `TotalAmount` and `Lines.Count` disagree depending on which list element the caller uses. Diagnose the bug and describe the production-safe aggregation pattern.

---

**Answer:**

```csharp
public Order? GetOrderWithLines(int orderId)
{
    const string sql = """
        SELECT o.OrderId, o.CustomerId, o.OrderDate, o.TotalAmount,
               ol.OrderLineId, ol.OrderId, ol.ProductId, ol.Quantity, ol.LineTotal
        FROM dbo.Orders o
        INNER JOIN dbo.OrderLines ol ON o.OrderId = ol.OrderId
        WHERE o.OrderId = @orderId
        ORDER BY ol.OrderLineId;
        """;

    using SqlConnection connection = new SqlConnection(_connectionString);

    List<Order> orders = connection.Query<Order, OrderLine, Order>(
        sql,
        (order, line) =>
        {
            order.Lines.Add(line);
            return order;
        },
        new { orderId },
        splitOn: "OrderLineId").ToList();

    return orders.FirstOrDefault();
}
```

The API returns three lines for order 1, but `TotalAmount` and `Lines.Count` disagree depending on which list element the caller uses. Diagnose the bug and describe the production-safe aggregation pattern.

**Answer:** Dapper's map delegate runs **once per JOIN row**. Each row materializes a **new** `Order` instance; mutating `order.Lines` on one row does not merge siblings. `.ToList()` therefore returns **one partial `Order` per line** — `FirstOrDefault()` keeps only the first line's parent shell.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | No `Dictionary<int, Order>` collapse | Duplicate parent objects; fragmented `Lines` collections |
| API contract | `FirstOrDefault()` hides duplicates | Callers see 1 line while DB has N |
| Design | Treating multi-map like single `Query<Order>` | One-to-many JOIN semantics misunderstood |

**Fix (priority order):**

1. Use the **lookup dictionary idiom** from **MultiMapRepository.GetOrderWithLines** — `TryGetValue(order.OrderId, …)`, initialize `Lines` once, `Add(line)` on the tracked instance.
2. Return `lookup.TryGetValue(orderId, out var result) ? result : null` — not the raw `IEnumerable` from `Query`.
3. Keep `splitOn: "OrderLineId"` — first column that starts `OrderLine`.

**Production takeaway:** Multi-map solves column splitting, not aggregation; one-to-many always needs explicit parent deduplication (dictionary, `Lookup`, or `GroupBy` after materialization).

---

---

#### Q3. (R) A new microservice copies `ColumnMappedProduct` and `DateOnlyTypeHandler` into a Web API project. Integration tests fail intermittently:

```csharp
// Program.cs — no RegisterDapperExtensions call
var app = builder.Build();
app.MapGet("/products/{id}", async (int id, IProductRepo repo) =>
    await repo.GetByIdAsync(id));

// ProductRepository.cs
public async Task<ColumnMappedProduct?> GetByIdAsync(int id)
{
    const string sql = "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products WHERE ProductId = @id;";
    await using var conn = new SqlConnection(_cs);
    return await conn.QuerySingleOrDefaultAsync<ColumnMappedProduct>(sql, new { id });
}

// OrderSummaryRepository.cs — added for a new endpoint
public async Task<OrderSummaryDto> GetSummaryAsync(int orderId)
{
    const string sql = "SELECT OrderDate FROM dbo.Orders WHERE OrderId = @orderId;";
    await using var conn = new SqlConnection(_cs);
    var date = await conn.QuerySingleAsync<DateOnly>(sql, new { orderId });
    return new OrderSummaryDto { OrderDate = date };
}
```

`Price` is always `0` in some test runs; `QuerySingleAsync<DateOnly>` throws `DataException` in others. What is missing, where should it live, and why does test order matter?

---

**Answer:**

```csharp
// Program.cs — no RegisterDapperExtensions call
var app = builder.Build();
app.MapGet("/products/{id}", async (int id, IProductRepo repo) =>
    await repo.GetByIdAsync(id));

// ProductRepository.cs
public async Task<ColumnMappedProduct?> GetByIdAsync(int id)
{
    const string sql = "SELECT ProductId, ProductName, UnitPrice FROM dbo.Products WHERE ProductId = @id;";
    await using var conn = new SqlConnection(_cs);
    return await conn.QuerySingleOrDefaultAsync<ColumnMappedProduct>(sql, new { id });
}

// OrderSummaryRepository.cs — added for a new endpoint
public async Task<OrderSummaryDto> GetSummaryAsync(int orderId)
{
    const string sql = "SELECT OrderDate FROM dbo.Orders WHERE OrderId = @orderId;";
    await using var conn = new SqlConnection(_cs);
    var date = await conn.QuerySingleAsync<DateOnly>(sql, new { orderId });
    return new OrderSummaryDto { OrderDate = date };
}
```

`Price` is always `0` in some test runs; `QuerySingleAsync<DateOnly>` throws `DataException` in others. What is missing, where should it live, and why does test order matter?

**Answer:** Dapper extension registration (`SqlMapper.AddTypeHandler`, `SqlMapper.SetTypeMap`) is **global static state** and must run **once before any query** using those types. Without **RegisterDapperExtensions** from **Program.cs**, `UnitPrice` never maps to `Price` (stays default `0`), and `DateTime` from SQL cannot convert to `DateOnly` without **DateOnlyTypeHandler**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Missing `ColumnAttributeTypeMap.RegisterOnce()` | `[Column("UnitPrice")]` ignored — silent zero prices |
| Runtime | Missing `SqlMapper.AddTypeHandler(new DateOnlyTypeHandler())` | `Parse` fails on `DateTime` from `DATE` columns |
| Startup / tests | Registration not in host startup | Tests pass only if another test registered handlers first — order-dependent flakiness |
| Design | Per-request registration absent but also not centralized | Easy to forget when copying repository code |

**Fix (priority order):**

1. Call a single `RegisterDapperExtensions()` at application startup — mirror **Program.cs** lines 67 and 93–97: TypeHandler first, then TypeMap.
2. In tests, invoke the same registration in a **one-time fixture** (`IClassFixture`, `[ModuleInitializer]`, or `WebApplicationFactory` host build) — not per test method.
3. Prefer **SQL aliases** (`UnitPrice AS Price`) for simple renames when you want zero global config; keep TypeHandlers for true type mismatches (`DateOnly`, enums, JSON).

**Production takeaway:** Global Dapper configuration belongs next to composition root setup (`Program.cs` / host builder), shared by production and tests — never assume a tutorial's `Main` ran.

---

---

#### Q4. (P) Production needs `DiscontinuedDate` mapped to `DateOnly?` on `ProductRow` — the column is often `NULL`. A junior registers this handler:

```csharp
public sealed class NullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value) =>
        value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

    public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
        parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
}
```

Under load, some rows with `DiscontinuedDate = NULL` still throw; parameterized inserts sometimes send `DBNull` incorrectly. What should `Parse` and `SetValue` handle, and when do you prefer a TypeHandler over a SQL `CAST`/`CONVERT` alias in the SELECT?

---

**Answer:**

```csharp
public sealed class NullableDateOnlyHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value) =>
        value is DateTime dt ? DateOnly.FromDateTime(dt) : null;

    public override void SetValue(IDbDataParameter parameter, DateOnly? value) =>
        parameter.Value = value?.ToDateTime(TimeOnly.MinValue);
}
```

Under load, some rows with `DiscontinuedDate = NULL` still throw; parameterized inserts sometimes send `DBNull` incorrectly. What should `Parse` and `SetValue` handle, and when do you prefer a TypeHandler over a SQL `CAST`/`CONVERT` alias in the SELECT?

**Answer:** The handler must treat **`DBNull.Value` and `null` explicitly** on read, and assign **`DBNull.Value`** (not C# `null`) to `IDbDataParameter.Value` when writing SQL NULLs. The ternary `value is DateTime` branch never runs for NULL database values if ADO.NET surfaces them as `DBNull`, and `parameter.Value = value?.…` leaves the parameter unset when `value` is null.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Parse` ignores `DBNull` | InvalidCastException or DataException on nullable columns |
| Runtime | `SetValue` uses C# null for parameter | SQL INSERT/UPDATE may omit NULL binding correctly only with `DBNull.Value` |
| Design | Handler registered for `DateOnly?` but entity uses non-nullable `DateOnly` elsewhere | Wrong handler generic — register matching open/nullable type |

**Fix (priority order):**

1. **Parse:** `if (value is null or DBNull) return null;` then handle `DateTime` / `DateOnly` like **DateOnlyTypeHandler.cs**.
2. **SetValue:** `parameter.Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;`
3. Register with `SqlMapper.AddTypeHandler(new NullableDateOnlyHandler())` before queries — same startup rule as Section 6.

**When to use TypeHandler vs SQL alias:**

- **TypeHandler:** reuse across many queries, both directions (read/write), domain types (`DateOnly`, value objects, bool as char).
- **SQL alias/CAST:** one-off reporting query, read-only DTO, no insert path — e.g. `SELECT CAST(DiscontinuedDate AS date) AS DiscontinuedDate` into a `DateTime?` property without custom handler.

**Production takeaway:** Nullable value-type mapping fails on `DBNull` edge cases more often than on happy-path dates — mirror ADO.NET null semantics explicitly.

---

---

#### Q5. (D) An order-details endpoint currently loads 200 orders, then loops:

```csharp
foreach (var order in orders)
{
    order.Customer = await _repo.GetCustomerAsync(order.CustomerId); // N round-trips
    order.Lines = (await _repo.GetLinesAsync(order.OrderId)).ToList();
}
```

A proposal replaces the loop with one JOIN query using the chapter's `Query<Order, Customer, Order>` plus a second `GetOrderWithLines`-style multi-map. Compare **N+1 queries**, **two JOIN queries** (header + lines), and **one wide JOIN** with dictionary aggregation. What would you ship for a paginated admin grid vs a single-order detail page?

---

**Answer:**

```csharp
foreach (var order in orders)
{
    order.Customer = await _repo.GetCustomerAsync(order.CustomerId); // N round-trips
    order.Lines = (await _repo.GetLinesAsync(order.OrderId)).ToList();
}
```

A proposal replaces the loop with one JOIN query using the chapter's `Query<Order, Customer, Order>` plus a second `GetOrderWithLines`-style multi-map. Compare **N+1 queries**, **two JOIN queries** (header + lines), and **one wide JOIN** with dictionary aggregation. What would you ship for a paginated admin grid vs a single-order detail page?

**Answer:** N+1 explodes latency and connection use under load; JOIN + multi-map collapses round-trips but trades **wider rows and mapping complexity**. The right shape depends on **payload depth** and **pagination**, not a universal rule.

- **Paginated admin grid (200 orders, customer name only):** Ship **one JOIN** — `Query<Order, Customer, Order>` with explicit `splitOn` (see **GetOrdersWithCustomers**). Skip lines entirely; do not multi-map children you will not display. Avoid N+1; also avoid loading all lines for every row.
- **Single-order detail page:** Ship **two queries** — (1) order + customer JOIN, (2) lines multi-map with dictionary for that `orderId` — or **one** order+lines JOIN if line count is bounded (tens, not thousands). Two queries keeps mapping simpler and avoids a cartesian explosion when lines grow.
- **One wide JOIN (order + customer + lines):** Acceptable for **one order id**; for lists it duplicates order/customer columns per line and increases memory — use dictionary aggregation and never return raw duplicated rows to the client.
- **N+1:** Reserve for small graphs, background jobs, or when cache hits dominate — not for hot list endpoints.

**Production takeaway:** Multi-map eliminates N+1 **when you fetch the graph in one SQL round-trip**; it does not replace pagination discipline or choosing how deep each endpoint hydrates.

---

---

#### Q6. (R) A developer migrates manual INSERT SQL to Dapper.Contrib on the same `Products` table (`ProductId` is **not** IDENTITY — keys are assigned manually):

```csharp
[Table("Products")]
public sealed class ProductEntity
{
    [Key] // was [ExplicitKey] in the tutorial sample
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}

// Insert path
entity.ProductId = await conn.ExecuteScalarAsync<int>(
    "SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products;");
await conn.InsertAsync(entity);
var loaded = await conn.GetAsync<ProductEntity>(entity.ProductId); // null
```

Insert appears to succeed but `Get` returns null; SQL Profiler shows an INSERT without `ProductId`. What went wrong with Contrib attributes, and what concurrency risk remains even after fixing the attribute?

---

**Answer:**

```csharp
[Table("Products")]
public sealed class ProductEntity
{
    [Key] // was [ExplicitKey] in the tutorial sample
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}

// Insert path
entity.ProductId = await conn.ExecuteScalarAsync<int>(
    "SELECT ISNULL(MAX(ProductId), 0) + 1 FROM dbo.Products;");
await conn.InsertAsync(entity);
var loaded = await conn.GetAsync<ProductEntity>(entity.ProductId); // null
```

Insert appears to succeed but `Get` returns null; SQL Profiler shows an INSERT without `ProductId`. What went wrong with Contrib attributes, and what concurrency risk remains even after fixing the attribute?

**Answer:** `[Key]` tells Contrib the column is an **identity** key — Contrib **excludes it from INSERT** and expects the database to generate it. Manual keys require **`[ExplicitKey]`** as in **ContribProduct.cs** so assigned `ProductId` is included in the INSERT statement.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `[Key]` on non-identity column | INSERT omits `ProductId`; row may fail constraints or get wrong key |
| Runtime | `Get<T>(id)` after bad insert | Returns null — key mismatch or no row |
| Concurrency | `MAX(ProductId) + 1` without transaction/sequence | Duplicate keys under concurrent inserts — race even with correct attribute |

**Fix (priority order):**

1. Change to `[ExplicitKey]` on `ProductId` — matches **ContribPreviewRepository** pattern.
2. Replace `MAX+1` with **`SEQUENCE`**, **`IDENTITY`**, or insert inside a **transaction** with **`UPDLOCK, HOLDLOCK`** on the key range — or use a dedicated key table.
3. Verify generated SQL in Profiler: INSERT must list `ProductId` when manually assigned.

**Production takeaway:** Contrib's `[Key]` vs `[ExplicitKey]` distinction is not cosmetic — it controls INSERT shape. Manual key generation still needs database-level uniqueness strategy.

---

---

#### Q7. (R) A search API builds product filters with string concatenation instead of the chapter's SqlBuilder preview pattern:

```csharp
public IReadOnlyList<ProductRow> SearchProducts(string? category, decimal? minPrice, string sortColumn)
{
    var sql = "SELECT ProductId, ProductName, UnitPrice, StockQuantity FROM dbo.Products WHERE 1=1";
    if (!string.IsNullOrEmpty(category))
        sql += $" AND Category = '{category}'";
    if (minPrice.HasValue)
        sql += $" AND UnitPrice >= {minPrice.Value}";
    sql += $" ORDER BY {sortColumn}";

    using var conn = new SqlConnection(_connectionString);
    return conn.Query<ProductRow>(sql).ToList();
}
```

Security review flags SQL injection on `category` and `sortColumn`. Refactor toward **Dapper.SqlBuilder** (or equivalent) with parameterized fragments. What must never be passed as a raw interpolated string, and how do whitelist + parameters split responsibility?

**Answer:**

```csharp
public IReadOnlyList<ProductRow> SearchProducts(string? category, decimal? minPrice, string sortColumn)
{
    var sql = "SELECT ProductId, ProductName, UnitPrice, StockQuantity FROM dbo.Products WHERE 1=1";
    if (!string.IsNullOrEmpty(category))
        sql += $" AND Category = '{category}'";
    if (minPrice.HasValue)
        sql += $" AND UnitPrice >= {minPrice.Value}";
    sql += $" ORDER BY {sortColumn}";

    using var conn = new SqlConnection(_connectionString);
    return conn.Query<ProductRow>(sql).ToList();
}
```

Security review flags SQL injection on `category` and `sortColumn`. Refactor toward **Dapper.SqlBuilder** (or equivalent) with parameterized fragments. What must never be passed as a raw interpolated string, and how do whitelist + parameters split responsibility?

**Answer:** **User-supplied values** (`category`, prices, ids) must always flow through **Dapper parameters** (`@category`, `@minPrice`). **Identifiers** (column names, sort direction, table names) cannot be parameterized in T-SQL — `sortColumn` must be chosen from a **fixed whitelist**, not concatenated from the request string.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `$" AND Category = '{category}'"` | Classic SQL injection on filter values |
| Security | `$" ORDER BY {sortColumn}"` | Injection via sort field — attacker can inject subqueries or stack statements |
| Maintainability | Ad-hoc string building | Optional clauses error-prone; hard to audit |

**Fix (priority order):**

1. Replace value filters with parameterized fragments — pattern from **SqlBuilderPreview.cs**:

```csharp
var builder = new SqlBuilder();
var template = builder.AddTemplate(
    "SELECT /**select**/ FROM dbo.Products /**where**/ /**orderby**/",
    new { Category = category, MinPrice = minPrice });

builder.Select("ProductId, ProductName, UnitPrice, StockQuantity");
if (!string.IsNullOrEmpty(category))
    builder.Where("Category = @Category", new { Category = category });
if (minPrice.HasValue)
    builder.Where("UnitPrice >= @MinPrice", new { MinPrice = minPrice });

var sort = sortColumn switch
{
    "Name" => "ProductName",
    "Price" => "UnitPrice",
    _ => "ProductId"
};
builder.OrderBy(sort);

return conn.Query<ProductRow>(template.RawSql, template.Parameters).ToList();
```

2. Never whitelist-sort with user raw strings — map `"price"` → `"UnitPrice"` internally.
3. Keep `minPrice` as a parameter even though decimal is less exploitable — plan cache and typing consistency.

**Production takeaway:** Dapper parameters secure **values**; SqlBuilder (or manual fragment lists) secure **optional SQL structure**. Identifier injection is still injection — whitelist columns, parameterize everything else.

---
