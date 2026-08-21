# .NET Data Access — Interview Answers

Answers for [INTERVIEW_QUESTIONS.md](./INTERVIEW_QUESTIONS.md).  
Written for clarity and recall — concepts only, no interview coaching.

> **Scope:** Interview-focused — high-yield questions only

---

## ADO.NET

## Chapter 01. Introduction to ADO.NET

#### Q1. What is ADO.NET?

**Answer:** ADO.NET (ActiveX Data Objects for .NET) is the low-level data access API in .NET for talking to databases through provider-specific drivers. It exposes connections, commands, readers, and adapters so application code can execute SQL and move rows between the database and .NET without an object-relational mapper (ORM).

- It sits below higher-level libraries such as Dapper and Entity Framework Core (EF Core), which still use ADO.NET providers under the hood.
- Core types live in `System.Data.Common` for provider-agnostic code and in packages like `Microsoft.Data.SqlClient` for SQL Server.
- ADO.NET supports both connected streaming reads and disconnected in-memory snapshots depending on which types you use.
- You write and own the SQL, which gives maximum control over statements, batching, and provider-specific features.

---

#### Q2. What is the difference between connected and disconnected data access in ADO.NET?

**Answer:** Connected access keeps an open database connection while you stream or execute commands; disconnected access loads data into in-memory structures and then closes the connection. Connected mode is lean for read-heavy, forward-only scenarios; disconnected mode suits editing snapshots offline or binding UI grids without holding a live connection.

- Connected: open `SqlConnection`, run `SqlCommand`, read with `SqlDataReader` row by row, then close.
- Disconnected: use `SqlDataAdapter.Fill` to populate `DataTable`/`DataSet`, close the connection, work in memory, optionally `Update` changes back.
- Connected mode uses less memory for large result sets because only one row is live at a time.
- Disconnected mode trades memory for convenience — the entire result set (or related tables) lives in RAM until you release it.

---

#### Q3. What are the core building blocks of ADO.NET (connection, command, reader, adapter)?

**Answer:** The four pillars are `DbConnection` (network session to the database), `DbCommand` (SQL or stored procedure to execute), `DbDataReader` (forward-only read stream), and `DbDataAdapter` (bridge that fills and updates disconnected `DataSet`/`DataTable` objects). Together they cover opening a session, sending text, reading results, and synchronizing in-memory data back to the server.

- **Connection** — holds connection string settings and pooling state; `Open`/`OpenAsync` acquires a pooled or physical connection.
- **Command** — carries `CommandText`, parameters, timeout, and transaction; exposes `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`.
- **Reader** — returns a read-only, forward-only cursor over result sets while the connection stays open.
- **Adapter** — uses a `SelectCommand` (and optional insert/update/delete commands) to `Fill` tables and `Update` changed rows.

---

#### Q4. What is the difference between ADO.NET and an ORM like Entity Framework Core?

**Answer:** ADO.NET is a thin provider API: you write SQL, map columns manually, and manage connections and transactions explicitly. EF Core is an ORM that maps C# classes to tables, translates LINQ to SQL, tracks entity changes, and applies schema through migrations.

- ADO.NET gives full SQL control and minimal runtime overhead; EF Core trades some control for productivity and a unified domain model.
- With ADO.NET there is no built-in change tracking, relationship navigation, or migration pipeline — you implement those yourself.
- EF Core still opens ADO.NET connections internally when it executes generated SQL.
- ADO.NET fits hand-tuned queries, bulk operations, and legacy stored-procedure-heavy systems; EF Core fits CRUD-heavy apps with evolving models.

---

#### Q5. When would you choose ADO.NET over Dapper or EF Core?

**Answer:** Choose raw ADO.NET when you need maximum control over SQL, provider-specific features, or bulk throughput and want zero mapping or change-tracking overhead. It is the right tool for `SqlBulkCopy`, complex multi-statement batches, fine-grained timeouts, and scenarios where every execution plan must be predictable.

- Bulk load or ETL (extract, transform, load) pipelines where ORM change tracking adds no value.
- Legacy databases accessed almost entirely through stored procedures with output parameters and multiple result sets.
- Performance-critical paths where even micro-ORM reflection and materialization cost matters.
- Reporting or admin scripts that execute ad hoc SQL without maintaining entity classes or migrations.

---

#### Q6. What is the connected model, and which ADO.NET types does it primarily use?

**Answer:** The connected model keeps a live `DbConnection` open while commands run and results stream back. It primarily uses `SqlConnection`, `SqlCommand`, and `SqlDataReader` (or the provider equivalents) in a nested `using`/`await using` chain.

- Open the connection once per unit of work, execute one or more commands sequentially, dispose the reader before the next command on the same connection (unless Multiple Active Result Sets (MARS) is enabled).
- Ideal for HTTP APIs that map rows to DTOs (data transfer objects) while `Read()` advances — memory stays flat for large lists.
- Streaming exports, log tailing, and pagination that never materializes the full table fit this model.
- The connection returns to the pool when disposed, even though the model is "connected" during the method body.

---

#### Q7. What is the disconnected model, and which ADO.NET types does it primarily use?

**Answer:** The disconnected model pulls data into in-memory tabular objects, closes the database connection, and lets client code read or edit rows without an active session. It centers on `DataSet`, `DataTable`, `DataRow`, and `SqlDataAdapter` (with optional `CommandBuilder` for auto-generated update commands).

- `SqlDataAdapter.Fill` runs the select command and populates tables; the connection can close immediately afterward.
- Users or middle-tier code modify `DataRow` state (`Added`, `Modified`, `Deleted`); `adapter.Update` pushes changes in batches.
- `DataSet` can hold multiple related tables and relationships for client-side join-like operations.
- Common in older WinForms and ASP.NET WebForms apps; less common in modern stateless ASP.NET Core APIs.

---

#### Q8. What are the trade-offs of hand-written SQL versus a higher-level ORM?

**Answer:** Hand-written SQL through ADO.NET offers precise control, predictable execution plans, and access to every database feature, but shifts mapping, schema evolution, and relationship management to your team. An ORM accelerates development and keeps the object model and database in sync, yet can produce surprising SQL, tracking overhead, and harder-to-tune queries.

- SQL you write is reviewable and benchmarkable; ORM-generated SQL may need profiling to optimize.
- Schema changes in ADO.NET require manual script coordination; EF Core migrations version schema alongside code.
- ORMs reduce boilerplate for CRUD and relationships; ADO.NET requires explicit DTO mapping and join SQL.
- Many production systems combine both: EF Core for domain workflows, ADO.NET or Dapper for hot paths and reports.

---

## Chapter 02. SqlConnection & Connection Strings

#### Q1. What is connection pooling in ADO.NET?

**Answer:** Connection pooling is a provider-managed cache of open database connections keyed by an identical connection string. When you `Open` a `SqlConnection`, the provider reuses an idle pooled connection instead of always creating a new TCP session and logging in again.

- Pooling is enabled by default for SQL Server; `Pooling=false` in the connection string disables it.
- `Min Pool Size` and `Max Pool Size` control how many connections the pool keeps warm and the upper bound under load.
- `Close`/`Dispose` on the `SqlConnection` returns the physical connection to the pool — it does not necessarily tear down the network session.
- Identical connection strings share one pool; even small differences (extra spaces, different attribute order) create separate pools.

---

#### Q2. Does creating `new SqlConnection()` every time open a new physical database connection?

**Answer:** Creating `new SqlConnection()` only allocates a managed wrapper; no network connection opens until you call `Open` or `OpenAsync`. With default pooling, `Open` typically grabs an existing pooled physical connection rather than establishing a brand-new one each time.

- Instantiating many `SqlConnection` objects per request is normal and inexpensive when each is disposed promptly.
- A new physical connection is created only when the pool has no available connection and has not reached `Max Pool Size`.
- Without pooling, every `Open` would pay full login and handshake cost — rarely desirable in production.
- Always dispose the connection so the underlying slot returns to the pool for reuse.

---

#### Q3. Why should you use `using` or `await using` with connections?

**Answer:** `using` and `await using` guarantee `Dispose`/`DisposeAsync` runs even when exceptions occur, which returns the connection to the pool and releases the reader and command resources tied to it. Without disposal, connections leak until the garbage collector finalizes them — often too late under load.

- `Dispose` on `SqlConnection` closes the logical connection and returns the physical connection to the pool.
- Exception paths that skip manual `Close()` still dispose correctly inside a `using` block.
- `await using` pairs with async ADO.NET methods so disposal does not block a thread pool thread.
- Nested `using` for connection, command, and reader ensures the reader is released before the connection is returned.

---

#### Q4. How do you store connection strings securely in ASP.NET Core?

**Answer:** Keep secrets out of source control: store connection strings in environment-specific configuration backed by a secret store, not hard-coded in repositories. ASP.NET Core reads them via `IConfiguration.GetConnectionString("Name")`, with production values supplied by environment variables, Azure Key Vault, AWS Secrets Manager, or user secrets during local development.

- Commit only non-secret templates or placeholders in `appsettings.json`; override with `appsettings.Production.json` excluded from git or with env vars such as `ConnectionStrings__Default`.
- Use managed identity or integrated authentication where possible to avoid embedding passwords.
- Restrict database logins to least privilege — the API account should not be `db_owner` unless required.
- Rotate credentials in the secret store without redeploying code when the app reads configuration at startup or on reload.

---

#### Q5. What is the difference between `Microsoft.Data.SqlClient` and `System.Data.SqlClient`?

**Answer:** Both are SQL Server ADO.NET providers, but `Microsoft.Data.SqlClient` is the actively maintained, cross-platform package for modern .NET, while `System.Data.SqlClient` is the legacy assembly shipped with .NET Framework and kept in maintenance mode. New projects should reference `Microsoft.Data.SqlClient` exclusively to avoid duplicate types and missing features.

- `Microsoft.Data.SqlClient` receives fixes for Azure SQL, Always Encrypted, managed identity, and TLS behavior.
- Mixing both packages in one solution can cause type identity conflicts — `SqlConnection` from one assembly is not assignable to the other.
- .NET Core and later templates default to `Microsoft.Data.SqlClient`; Framework apps often still reference `System.Data.SqlClient`.
- Public libraries should target `System.Data.Common` abstractions or document a single SqlClient package dependency.

---

#### Q6. What is a pool exhaustion error, and what typically causes it?

**Answer:** Pool exhaustion occurs when every connection in the pool is in use and the provider cannot allocate another within the timeout — SQL Server clients often report "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool." The usual cause is failing to dispose connections or readers, leaving slots checked out until the pool hits `Max Pool Size`.

- Long-running queries or blocked transactions hold connections for extended periods under concurrent load.
- Opening a connection per row in a loop without disposal multiplies checkout time linearly with iteration count.
- Default `Max Pool Size` is 100; bursty traffic with leaks exhausts it quickly.
- Increasing `Max Pool Size` masks the symptom; fixing disposal and shortening connection lifetime addresses the root cause.

---

#### Q7. What symptoms indicate a misconfigured or exhausted connection pool?

**Answer:** Intermittent timeouts under load that clear after an app restart, rising request latency as concurrency increases, and errors mentioning "pool" or "timeout obtaining connection" point to pool stress. You may also see database-side counts of sleeping sessions matching your app's pool size ceiling while the app still cannot acquire a connection.

- Errors appear only under peak traffic because idle connections mask leaks during light testing.
- Thread pool starvation can accompany sync-over-async patterns that block while holding a connection.
- Monitoring shows open connection count pegged at `Max Pool Size` for sustained periods.
- Health checks that open connections without `using` on every probe accelerate exhaustion in Kubernetes or load-balanced farms.

---

#### Q8. How do unclosed connections affect pool availability?

**Answer:** An undisposed `SqlConnection` keeps its pooled slot checked out until the connection is finalized or the server-side session times out — neither is reliable for production throughput. Each leaked connection reduces the number of slots available to other requests, eventually causing new `Open` calls to block and fail.

- Finalizers may run minutes later under memory pressure, far too slow for a busy API.
- A leaked reader on an open connection also blocks reuse of that connection for additional commands.
- Under steady leak rate, the app reaches `Max Pool Size` and all database operations queue or timeout together.
- Proper `using`/`await using` on connection, command, and reader is the primary prevention.

---

## Chapter 03. SqlCommand & Parameters

#### Q1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?

**Answer:** `ExecuteReader` runs a query and returns a forward-only `DbDataReader` for one or more result sets. `ExecuteNonQuery` runs INSERT, UPDATE, DELETE, or DDL (data definition language) and returns the number of rows affected. `ExecuteScalar` returns the first column of the first row, or `null` if empty — useful for aggregates and identity lookups.

- Use `ExecuteReader` when you need multiple rows or columns streamed from a SELECT.
- Use `ExecuteNonQuery` for commands that do not return a rowset — including many stored procedures that only mutate data.
- Use `ExecuteScalar` for `SELECT COUNT(*)`, `SELECT MAX(...)`, or `SELECT SCOPE_IDENTITY()`-style single values.
- All three honor the same `SqlCommand` parameters, transaction, and timeout settings.

---

#### Q2. What is a parameterized query, and why is it preferred over string concatenation?

**Answer:** A parameterized query sends SQL with placeholders (`@Name`, `@Id`) and supplies values separately through `SqlParameter` objects. The database treats literals and user input as data, not executable text, which eliminates injection and improves plan reuse.

- The SQL shape stays constant; only parameter values change between executions.
- Query plans can be cached because the statement text does not vary with each user input.
- Formatting, culture, and escaping bugs from manual string building disappear.
- Dynamic SQL still parameterizes values even when table or column names are composed in code (identifiers cannot be parameterized — those require strict allowlists).

---

#### Q3. What is SQL injection, and how do parameters prevent it?

**Answer:** SQL injection is an attack where untrusted input is concatenated into SQL and interpreted as commands — for example, input `'; DROP TABLE Users;--` altering the intended statement. Parameters send values out-of-band from the command text, so the engine never parses user data as SQL syntax.

- Concatenation like `"WHERE Email = '" + email + "'"` lets attackers close quotes and append malicious clauses.
- With `cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = email`, the value is bound as a string literal only.
- Injection can occur in ADO.NET, Dapper, and EF Core whenever raw string interpolation bypasses parameter APIs.
- Parameters protect values; they do not sanitize dynamic identifiers — never parameterize table or column names from user input.

---

#### Q4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?

**Answer:** `AddWithValue(name, value)` infers the parameter type and size from the CLR (Common Language Runtime) value at runtime, which is convenient but can mismatch the database column. Explicit `SqlParameter` with `SqlDbType`, `Size`, and `Precision`/`Scale` ensures the provider sends exactly what the column expects.

- `AddWithValue("Name", longString)` may send `nvarchar(4000)` or `nvarchar(-1)` and prevent index seeks on `varchar(50)` columns.
- Explicit typing avoids implicit conversion in SQL Server that hides index use and bloats plan cache entries.
- For nullable value types, set `Value = DBNull.Value` explicitly when null.
- Production code often wraps explicit parameter creation in small helpers; `AddWithValue` is acceptable for quick prototypes with known small types.

---

#### Q5. What is the difference between `Text` and `StoredProcedure` command types?

**Answer:** `CommandType.Text` sends `CommandText` as raw SQL or T-SQL batch text. `CommandType.StoredProcedure` tells the provider to invoke a server-side procedure by name, binding parameters to the procedure signature rather than embedding them in a SQL string.

- Text mode: `cmd.CommandText = "SELECT * FROM Products WHERE Id = @id"`.
- Stored procedure mode: `cmd.CommandText = "dbo.GetProductById"` with parameters matching the proc definition.
- Return values and output parameters require `StoredProcedure` (or explicit `EXEC` in text mode, which is less clean).
- Stored procedures can encapsulate permissions — grant `EXEC` on the proc without exposing underlying tables.

---

#### Q6. When would you use `ExecuteScalar` instead of `ExecuteReader`?

**Answer:** Use `ExecuteScalar` when the query returns exactly one value — a count, sum, flag, or newly generated identity — and you do not need to iterate rows or columns. It avoids allocating a reader and the loop overhead for a single-cell result.

- `SELECT COUNT(*) FROM Orders WHERE Status = @status` maps naturally to `(int)cmd.ExecuteScalar()`.
- After INSERT, `SELECT CAST(SCOPE_IDENTITY() AS int)` retrieves the new key in one round trip.
- If the query might return zero rows, cast carefully and handle `null`/`DBNull`.
- If multiple columns or rows are possible, use `ExecuteReader` or `QueryFirstOrDefault` in Dapper instead.

---

## Chapter 04. SqlDataReader

#### Q1. Why is `SqlDataReader` described as a forward-only, read-only cursor?

**Answer:** The reader advances sequentially with `Read()`/`ReadAsync()` and cannot move backward or jump to arbitrary rows. It exposes data read-only — you consume column values but do not edit database rows through the reader itself.

- Each `Read()` call fetches the next row from the server stream (or from a client-side buffer depending on command behavior).
- There is no `Previous` or random access API — design loops as `while (reader.Read())`.
- To change data, issue separate INSERT/UPDATE commands; the reader is not an editable grid.
- This model matches how SQL Server sends result sets efficiently over the wire.

---

#### Q2. What is the difference between connected streaming reads and loading everything into memory?

**Answer:** Streaming through `SqlDataReader` processes one row at a time while the connection stays open, keeping memory usage roughly constant regardless of result set size. Loading into a `List<T>`, `DataTable`, or `DataSet` materializes every row into managed heap objects before processing begins.

- Streaming suits export pipelines, large reports, and APIs that map-and-write without retaining the full collection.
- In-memory load suits scenarios needing random access, multiple passes over rows, or offline editing disconnected from the server.
- A 2-million-row query may stream safely but cause out-of-memory (OOM) if `Fill` or `.ToList()` on the entire set runs on a web server.
- Time-to-first-row is faster with streaming because processing starts before the last row arrives.

---

#### Q3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?

**Answer:** A single SQL Server connection without Multiple Active Result Sets (MARS) allows only one active batch at a time. An open reader still owns that batch, so a second `ExecuteReader` or `ExecuteNonQuery` on the same connection throws: "There is already an open DataReader associated with this Command."

- Dispose the first reader completely before issuing the next command on the same connection.
- MARS (`MultipleActiveResultSets=True` in the connection string) relaxes this but adds complexity and server overhead — enable only when truly needed.
- Nested `using` blocks make the sequencing explicit: header query in one block, detail query in the next.
- This trap appears frequently when loading a master record then its line items on one shared connection.

---

#### Q4. When is `SqlDataReader` the best choice for large result sets?

**Answer:** Choose `SqlDataReader` when you must read many rows sequentially and can process each row without storing the entire set — exports, ETL transforms, log scanning, and streaming HTTP responses. It minimizes memory and starts processing as soon as the first row arrives.

- Pair with `CommandBehavior.SequentialAccess` when reading large binary or text columns to avoid buffering entire values unnecessarily.
- Combine with server-side paging (`OFFSET`/`FETCH`) when the consumer only needs a window of rows.
- Avoid loading into EF Core entities or `DataTable` when millions of rows would pressure Gen2 garbage collection.
- If the consumer needs the full set in memory anyway, the reader still helps peak memory during transfer but final footprint equals any other materialization approach.

---

#### Q5. How do you handle NULL database values when reading from a data reader?

**Answer:** Database NULL maps to `DBNull.Value` in ADO.NET; reading it directly into a value type throws. Use `reader.IsDBNull(ordinal)` or compare to `DBNull.Value`, then map to nullable CLR types or substitute defaults.

- Prefer `reader.GetInt32("Id")` only when the column is NOT NULL; otherwise use `reader.IsDBNull(i) ? null : reader.GetInt32(i)` for `int?`.
- Helper methods like `GetFieldValue<T?>(ordinal)` and extension methods reduce repetitive null checks.
- Strings may be `null` in C# when the column is NULL — distinguish empty string from NULL if the domain requires it.
- COALESCE in SQL can reduce null-handling in C# but shifts default semantics to the database layer.

---

#### Q6. What performance advantage does a reader have over filling a `DataTable`?

**Answer:** A reader avoids creating `DataRow`, `DataColumn`, and internal indexing structures for every row and column — it reads directly into your DTO mapping loop. `DataTable.Fill` allocates a full in-memory relational snapshot with type metadata and row state tracking.

- Lower allocations mean less garbage collection pressure on high-throughput APIs.
- Streaming keeps working set flat; `DataTable` memory grows linearly with row and column count.
- Readers start returning data immediately; `Fill` waits until the adapter buffers the result set (or a large portion).
- Use `DataTable` when you need disconnected editing, primary keys, and `AcceptChanges`/`RejectChanges` semantics — not for read-only bulk reads.

---

#### Q7. What happens if you do not dispose a data reader?

**Answer:** An undisposed reader keeps the underlying connection in a busy state, blocking further commands on that connection and delaying return to the pool. The connection may remain checked out until both reader and connection are finalized, contributing to pool exhaustion.

- Always wrap readers in `using` or `await using` alongside their command and connection.
- If an exception occurs mid-read, disposal still runs with `using`, releasing server-side cursor resources.
- Some providers close the reader when the connection disposes, but relying on that order is fragile — dispose inner objects first.
- Symptom under load: sporadic "open DataReader" errors and pool timeouts even when connection objects appear to be created correctly.

---

## Chapter 05. DataSet, DataTable & SqlDataAdapter

#### Q1. What is the disconnected model that `DataSet`/`DataTable` support?

**Answer:** `DataSet` and `DataTable` hold relational data in memory after the database connection closes, letting applications browse, filter, sort, and edit rows locally. `SqlDataAdapter` transfers data between the database and these structures via `Fill` (pull) and `Update` (push).

- A `DataTable` is a single table with rows, columns, constraints, and row state (`Unchanged`, `Modified`, `Added`, `Deleted`).
- A `DataSet` can contain multiple tables plus `DataRelation` objects modeling parent-child keys.
- After `Fill`, the connection can close while users or code work against the in-memory copy.
- `Update` applies batched changes through the adapter's insert, update, and delete commands.

---

#### Q2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?

**Answer:** `DataReader` streams rows through a live connection with minimal per-row allocation beyond your mapping. `DataAdapter.Fill` executes the select command and loads the entire result into a `DataTable`, building column schema and row objects before your code runs.

- Reader: connected, forward-only, best for large read-only sequential processing.
- Fill: disconnected snapshot, supports random access to any row via index or `Select` filter expressions.
- Fill pulls all rows unless you use `Fill` overloads with `startRecord`/`maxRecords` for partial loads.
- Adapter optionally generates commands with `SqlCommandBuilder`, though production apps often define commands explicitly.

---

#### Q3. When would you still use `DataSet`/`DataTable` in modern .NET applications?

**Answer:** They remain useful for ad hoc reporting tools, Excel-like grid editing, legacy interop, and scenarios requiring in-memory relational operations without pulling in EF Core. Some third-party controls and integration APIs still bind directly to `DataTable`.

- Quick admin utilities that load a table, let an operator edit cells, and push updates with one `Update` call.
- Accepting untyped tabular payloads where schema varies at runtime.
- Middle-tier merge scenarios comparing a client snapshot to server state before reconciling conflicts.
- Greenfield ASP.NET Core REST APIs more often return `IEnumerable<T>` DTOs from readers or EF Core projections instead.

---

#### Q4. What are the memory implications of filling a large table into a `DataSet`?

**Answer:** Every row becomes a `DataRow` with boxed values, version history, and state flags; every column carries `DataColumn` metadata. Memory usage is roughly proportional to row count × column count and often several times larger than the raw SQL payload.

- Large text and binary columns duplicate fully in managed memory.
- Multiple tables in one `DataSet` multiply footprint; relations do not share row storage.
- Gen2 collections from big fills cause long garbage collection pauses on server-class machines under load.
- Prefer streaming, paging, or projection when result sets exceed comfortable RAM for the process bitness and concurrent requests.

---

#### Q5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?

**Answer:** ASP.NET Core APIs are stateless and JSON-centric — clients expect typed DTOs, not ADO.NET tabular schema serialized with column names and `DBNull`. WinForms and WebForms kept connections short by binding grids to `DataTable` in desktop or session-heavy web UI patterns that do not match modern REST design.

- JSON serializers map cleanly to POCOs (plain old CLR objects), not `DataRow` dictionaries.
- Holding large `DataSet` objects in server memory per request does not scale across horizontal pods.
- EF Core and micro-ORMs replaced much of the "load table, bind grid" workflow with entity or DTO pipelines.
- Disconnected bulk editing is rarer in APIs; clients own UI state and send explicit PUT/PATCH payloads.

---

## Chapter 06. Transactions & Connection Pooling

#### Q1. What are the ACID properties of a transaction?

**Answer:** ACID stands for Atomicity, Consistency, Isolation, and Durability — the guarantees a transactional database provides for a group of operations. Either all statements in the transaction commit together or none do; concurrent sessions see controlled views of data; committed changes survive crashes.

- **Atomicity** — all commands succeed or all roll back; no partial application of a logical unit of work.
- **Consistency** — constraints and rules hold before and after the transaction (foreign keys, checks).
- **Isolation** — concurrent transactions do not interfere beyond the chosen isolation level's allowed phenomena.
- **Durability** — once committed, data persists even if power fails immediately afterward (via log flush).

---

#### Q2. How do you begin, commit, and rollback a transaction in ADO.NET?

**Answer:** Open a connection, call `connection.BeginTransaction()` (or `BeginTransactionAsync`) to get a `SqlTransaction`, assign it to each command's `Transaction` property, execute commands, then call `Commit()` on success or `Rollback()` on failure. Dispose the transaction object when finished.

```csharp
await using var conn = new SqlConnection(cs);
await conn.OpenAsync();
await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();
try
{
    // assign cmd.Transaction = tx; ExecuteNonQueryAsync...
    await tx.CommitAsync();
}
catch { await tx.RollbackAsync(); throw; }
```

- All participating commands must share the same connection and the same transaction instance.
- `Rollback` undoes work since `BeginTransaction` on that connection.
- Nested transactions use savepoints on some providers; SQL Server `BEGIN TRAN` nesting behaves differently from `TransactionScope`.

---

#### Q3. Why must all commands in a transaction share the same connection?

**Answer:** A transaction is bound to a single database session — SQL Server transaction context does not span two physical connections from the pool. Assigning the same `SqlTransaction` to commands on different connections is invalid and will fail or produce undefined behavior.

- Each pooled connection is an independent session; atomicity cannot cross sessions without a distributed transaction coordinator.
- Pattern: one `using` connection, one transaction, multiple commands on that connection.
- Parallel async commands on one connection still serialize at the protocol level unless MARS is enabled — design sequential steps for clarity.
- EF Core wraps the same rule: `BeginTransactionAsync` on the context's underlying connection applies to subsequent saves on that context instance.

---

#### Q4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?

**Answer:** `TransactionScope` is a `System.Transactions` API that marks a block of code as transactional and can escalate to a distributed transaction when multiple connections or resource managers enlist. `SqlTransaction` is a lightweight, single-connection transaction tied explicitly to one ADO.NET connection.

- `TransactionScope` supports ambient transactions across multiple databases if distributed transaction support is enabled — heavier and often discouraged in modern cloud apps.
- `SqlTransaction` is simpler, faster, and preferred for single-database ADO.NET work in ASP.NET Core.
- `TransactionScope` requires `Complete()` to commit; disposing without `Complete` rolls back.
- Many teams avoid distributed transactions in microservices; use sagas or outbox patterns instead.

---

#### Q5. What is a pool exhaustion error, and what typically causes it?

**Answer:** Pool exhaustion happens when no pooled connection is free within the timeout window — commonly because connections or readers were not disposed, or because long transactions hold slots during traffic spikes. The error surfaces at `Open`, not at the original leak site, which makes diagnosis harder.

- Same root causes as Chapter 02 Q6: leaks, long-running commands, and blocking locks extend checkout duration.
- Transactions that stay open across await points in request handlers tie up connections longer than necessary.
- Fixing disposal and keeping transactions short resolves most cases without raising `Max Pool Size`.
- Monitor correlation between active transactions, pool count, and application request concurrency.

---

#### Q6. What isolation levels exist, and why do they matter?

**Answer:** Isolation levels control how much one transaction sees of another's uncommitted or in-flight changes — trading consistency for concurrency. SQL Server supports READ UNCOMMITTED, READ COMMITTED (default), REPEATABLE READ, SNAPSHOT, and SERIALIZABLE, set via `BeginTransaction(IsolationLevel)`.

- Lower levels allow dirty reads, non-repeatable reads, or phantoms but reduce blocking.
- REPEATABLE READ and SERIALIZABLE increase lock duration and deadlock risk under write-heavy load.
- SNAPSHOT uses row versioning for consistent reads without shared locks on readers — popular for read-heavy apps when enabled at database level.
- Pick the weakest level that preserves business rules; default READ COMMITTED suits most OLTP (online transaction processing) CRUD.

---

#### Q7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?

**Answer:** Begin the transaction after the connection is open, execute all commands inside `try`, call `Commit` only when every step succeeds, and call `Rollback` in `catch` before rethrowing so callers know the operation failed. Use `finally` or `using` on the transaction object to dispose resources.

- Check `tx.Connection != null` before rollback — some providers null the connection after commit.
- Do not commit after partial failure; a single exception should undo the whole unit of work.
- Avoid swallowing exceptions without rollback — that leaves the connection in an aborted transaction state until rolled back.
- `await using var tx` plus explicit `RollbackAsync` in `catch` matches async ASP.NET Core request patterns.

---

## Chapter 07. Stored Procedures & Output Parameters

#### Q1. What is a stored procedure, and why use one from ADO.NET?

**Answer:** A stored procedure is a precompiled batch of T-SQL stored on the server and invoked by name. From ADO.NET it encapsulates complex SQL, centralizes performance tuning, and can expose a stable contract while underlying tables evolve.

- Grant EXEC on procedures instead of direct table access for tighter security boundaries.
- Plans may be reused efficiently; heavy reporting logic runs close to data reducing round trips.
- Output parameters and multiple result sets are natural fits for procedure result contracts.
- Version and deploy procedures with database migration scripts alongside application releases.

---

#### Q2. How do you execute a stored procedure with `SqlCommand`?

**Answer:** Set `CommandType = CommandType.StoredProcedure`, assign the procedure name to `CommandText`, add parameters matching the signature, then call `ExecuteReader`, `ExecuteNonQuery`, or `ExecuteScalar` as appropriate. Input parameters use `ParameterDirection.Input`; output and return values need explicit direction.

```csharp
cmd.CommandType = CommandType.StoredProcedure;
cmd.CommandText = "dbo.GetProductById";
cmd.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = id });
await using var reader = await cmd.ExecuteReaderAsync();
```

- Parameter names must match the procedure definition including `@` prefix conventions.
- Do not embed `EXEC dbo.Proc @x` as text when `StoredProcedure` mode is cleaner and safer.
- Async variants mirror sync: `ExecuteReaderAsync`, `ExecuteNonQueryAsync`.

---

#### Q3. What is the difference between output parameters and return values (`ReturnValue`)?

**Answer:** Output parameters are declared with `OUTPUT` in the procedure and pass values back through named `@param` slots after execution. The procedure return value is a separate integer channel accessed by adding a parameter with `Direction = ParameterDirection.ReturnValue` — often used for status codes, not business data.

- Output: `cmd.Parameters.Add("@Total", SqlDbType.Decimal).Direction = ParameterDirection.Output;` read `.Value` after execute.
- Return value: `var ret = new SqlParameter("@RET", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };` maps to T-SQL `RETURN 0`.
- Multiple outputs are supported; only one return value exists per procedure call.
- Prefer output parameters for result data; reserve RETURN for success/failure codes consistent with C-style APIs.

---

#### Q4. When are stored procedures preferred over inline SQL in ADO.NET?

**Answer:** Prefer procedures when DBAs own tuned access paths, when you need stable permissions on a narrow API surface, or when batches are large and benefit from server-side execution without shipping text every call. Inline SQL suits simple CRUD, rapidly changing queries, and teams that version SQL entirely in application code.

- High-volume operations already optimized as procedures with hints and indexed views.
- Security models that deny SELECT on tables but allow EXEC on vetted procedures.
- Applications sharing the same database with multiple clients — procedures define one contract.
- Simple parameterized SELECTs from repositories are often clearer as inline SQL or Dapper for transparency and code review.

---

#### Q5. What are the trade-offs of putting business logic in stored procedures versus C#?

**Answer:** Stored procedures keep logic close to data, reduce round trips, and can enforce rules regardless of which client connects, but they are harder to unit test, version with application code, and debug in typical .NET toolchains. C# logic benefits from source control cohesion, test frameworks, and domain-driven design at the cost of more network chatter if implemented naively.

- Complex branching in T-SQL is harder to refactor and review than equivalent C# services.
- Migrations and CI pipelines must deploy procedure changes separately or via embedded SQL scripts.
- Some rules (validation, authorization against external systems) cannot live purely in the database.
- Pragmatic split: data-intensive set operations in SQL; orchestration, external integration, and domain rules in application code.

---

## Chapter 08. Async ADO.NET

#### Q1. Why should database I/O be async in ASP.NET Core request handlers?

**Answer:** Database calls are network I/O-bound — threads should not block waiting for SQL Server to respond. Async ADO.NET (`OpenAsync`, `ExecuteReaderAsync`, etc.) frees thread pool threads to serve other requests while the current operation awaits the database, improving scalability under concurrent load.

- ASP.NET Core handles many simultaneous connections with a small thread pool; blocking threads on I/O reduces throughput.
- Async end-to-end from endpoint through repository lets Kestrel reuse threads during awaits.
- Sync ADO.NET under load increases latency and can cause thread pool starvation before CPU saturates.
- Async does not make a single query faster — it improves how many queries the server handles at once.

---

#### Q2. What does `await using` provide when working with connections and readers?

**Answer:** `await using` asynchronously disposes `IAsyncDisposable` resources such as `SqlConnection`, `SqlCommand`, and `SqlDataReader`, ensuring cleanup completes without blocking a thread on network flush operations. Combined with `try`/`finally` semantics, disposal runs even when exceptions interrupt reading.

- Prefer `await using var conn = new SqlConnection(cs)` in async methods throughout the stack.
- Nested `await using` for reader inside connection guarantees correct release order.
- Returning a deferred `IEnumerable` that still holds an open reader requires the caller to complete enumeration before disposal — materialize inside the method when possible.
- Mixing sync `using` works but `Dispose()` may block; async disposal is cleaner in async-only code paths.

---

#### Q3. What problems arise from calling `.Result` or `.Wait()` on async ADO.NET operations?

**Answer:** Blocking on incomplete async tasks causes sync-over-async: a thread pool thread waits idle while I/O could have released it, reducing throughput and increasing deadlock risk when an synchronization context captures the blocked thread. In ASP.NET Core, `.Result` on repository async methods under load is a common source of thread pool queue growth and timeouts.

- ASP.NET Core has no legacy `AspNetSynchronizationContext`, but blocking still wastes threads and can cascade under burst traffic.
- Deadlocks appear when blocked code waits on continuations that need the same blocked thread.
- Fix by making controllers and endpoints `async Task` and awaiting through to ADO.NET calls.
- Library code should use `ConfigureAwait(false)` internally; application code awaits normally without forcing sync.

---

#### Q4. What is `CancellationToken` support in async ADO.NET methods?

**Answer:** Async ADO.NET methods accept an optional `CancellationToken` that signals when the client disconnected or a timeout fired, allowing the provider to cancel the pending network operation and close the command. Pass `HttpContext.RequestAborted` from ASP.NET Core endpoints into repository methods.

- `OpenAsync(cancellationToken)`, `ExecuteReaderAsync(cancellationToken)`, and `ReadAsync(cancellationToken)` honor cooperative cancellation.
- Cancellation throws `OperationCanceledException` — distinguish from SQL errors in exception handling.
- Without a token, aborted HTTP requests may continue running SQL until command timeout completes.
- Tokens do not automatically roll back transactions — pair cancellation with explicit rollback in `catch`.

---

#### Q5. What is the recommended async pattern for opening a connection, executing a command, and reading results?

**Answer:** Use nested `await using` declarations: open the connection asynchronously, create the command, execute the reader asynchronously, then loop with `ReadAsync` until false — passing `CancellationToken` through each step. Map columns to DTOs inside the read loop and return materialized results before leaving the method scope.

```csharp
await using var conn = new SqlConnection(cs);
await conn.OpenAsync(ct);
await using var cmd = new SqlCommand(sql, conn);
await using var reader = await cmd.ExecuteReaderAsync(ct);
while (await reader.ReadAsync(ct)) { /* map row */ }
```

- Keep the entire chain async — no `.Result` at any layer.
- Parameterize the command before execution.
- Materialize to `List<T>` before returning if the caller needs data after the connection closes.
- One connection per unit of work unless transaction scope requires otherwise.

---

#### Q6. When is synchronous ADO.NET still acceptable?

**Answer:** Sync ADO.NET remains fine for console tools, one-off migrations, local scripts, and startup configuration where no concurrent request pressure exists and simplicity outweighs scalability concerns. Background workers with dedicated threads and low parallelism can also use sync APIs if they never block ASP.NET Core request threads.

- Local development utilities and integration test setup/teardown often use sync calls for brevity.
- Single-threaded batch jobs that process sequentially may not benefit from async overhead.
- Never call sync ADO.NET from ASP.NET Core request threads when async alternatives exist — that is where the scalability cost appears.
- If a sync API must run inside a web app, offload to `Task.Run` only as a last resort — prefer true async I/O instead.

---

## Dapper

## Chapter 01. Introduction to Dapper

#### Q1. What is Dapper?

**Answer:** Dapper is a lightweight .NET library that extends `IDbConnection` with extension methods for executing SQL and mapping result rows to plain C# objects. It sits on top of ADO.NET (ActiveX Data Objects for .NET) and keeps you in control of the SQL while removing most manual reader and parameter boilerplate.

- Created by Stack Overflow and distributed as the `Dapper` NuGet package.
- Works with any ADO.NET provider — SQL Server, PostgreSQL, MySQL, SQLite, and others.
- Maps query results to classes, structs, value tuples, and dynamic types with minimal configuration.
- Does not own connection lifetime — you still open, dispose, and pool connections through the underlying provider.

---

#### Q2. What type of library is Dapper (ORM, micro-ORM, or something else)?

**Answer:** Dapper is a micro-ORM (Object-Relational Mapper) — it maps rows to objects but does not model relationships, change tracking, or schema migrations like a full ORM. You write the SQL; Dapper handles parameter binding and materialization.

- A full ORM like Entity Framework Core generates SQL from LINQ and tracks entity state across a unit of work.
- Dapper deliberately avoids those features to stay fast and predictable for hand-tuned queries.
- It is often described as an object mapper rather than a complete persistence framework.
- Teams pick it when SQL visibility and performance matter more than convention-based modeling.

---

#### Q3. What problem does Dapper solve compared to raw ADO.NET?

**Answer:** Raw ADO.NET requires repetitive code to create commands, add parameters, open readers, and copy column values into object properties row by row. Dapper collapses that into one call such as `connection.Query<Product>(sql, param)` while still using your SQL and connection.

- Eliminates manual `SqlDataReader` indexing and null-checking for every column.
- Binds anonymous objects, `DynamicParameters`, or expando objects to SQL parameters automatically.
- Supports async methods (`QueryAsync`, `ExecuteAsync`) with the same concise surface area.
- Keeps full access to transactions, stored procedures, and provider-specific features underneath.

---

#### Q4. What problem does Dapper solve compared to Entity Framework Core?

**Answer:** Entity Framework Core adds abstraction overhead — LINQ translation, change tracking, migrations, and relationship fix-up — that can be unnecessary for simple, performance-sensitive read paths. Dapper gives you direct SQL execution with near-ADO.NET speed and no hidden queries.

- You always see the exact SQL sent to the database, which simplifies tuning and index work.
- No change tracker means lower memory use on large read-only result sets.
- No migration or model configuration layer — schema changes are managed in SQL or external tools.
- Trade-off: you manually handle joins, updates, and relationship shaping that EF Core automates.

---

#### Q5. When would you choose Dapper over EF Core?

**Answer:** Choose Dapper when you need hand-written SQL, maximum read throughput, or integration with legacy stored procedures where EF Core's LINQ translation adds little value. It fits reporting, bulk reads, and microservices that treat the database as the source of truth for complex queries.

- Hot paths where every millisecond and every round-trip are profiled and fixed in SQL.
- Teams with strong database administrators who prefer procedures and tuned statements in source control.
- Scenarios where EF Core would generate inefficient joins, cartesian explosions, or untranslatable LINQ.
- Read-heavy APIs that only need Data Transfer Objects (DTOs) without entity graphs or change tracking.

---

#### Q6. When would you choose EF Core over Dapper?

**Answer:** Choose Entity Framework Core when you want convention-based mapping, LINQ composability, migrations, and change tracking for typical create-read-update-delete (CRUD) workflows. EF Core 8 reduces the gap on bulk operations but still excels at model-driven application development.

- Greenfield applications where C# entity classes should drive schema evolution through migrations.
- Complex object graphs with relationships, eager loading, and automatic relationship fix-up.
- When `SaveChanges` unit-of-work semantics and concurrency tokens are simpler than manual SQL per operation.
- When you need provider-agnostic LINQ that EF Core translates rather than maintaining SQL per database.

---

#### Q7. What are the main advantages and limitations of Dapper?

**Answer:** Dapper's main advantages are speed, simplicity, and full SQL control with minimal mapping code. Its limitations are the absence of change tracking, migrations, relationship navigation, and automatic SQL generation — every query and schema change is your responsibility.

- Advantage: among the fastest mappers in .NET benchmarks because it avoids heavy metadata and proxy layers.
- Advantage: tiny API surface — learn `Query`, `Execute`, and parameters rather than an entire ORM stack.
- Limitation: no built-in way to detect dirty entities or batch updates without writing SQL yourself.
- Limitation: multi-table object graphs require manual joins, multi-mapping, or multiple queries.
- Limitation: schema drift is not caught at compile time the way a strongly configured EF Core model can be.

---

#### Q8. Does Dapper generate SQL for you?

**Answer:** No. Dapper never generates SQL — you supply the complete statement as a string (or procedure name). It only binds parameters, executes the command through ADO.NET, and maps the result set to your types.

- Inserts, updates, and deletes require explicit `INSERT`, `UPDATE`, or `DELETE` text or stored procedures.
- There is no LINQ provider or expression tree translator in Dapper itself.
- Third-party extensions such as Dapper.Contrib offer optional CRUD helpers but still emit fixed SQL templates, not dynamic LINQ translation.
- This design is intentional: predictable SQL is a feature, not a missing ORM capability.

---

## Chapter 02. Queries, Execute & Async Methods

#### Q1. What is the difference between Dapper's `Query` and `Execute` methods?

**Answer:** `Query` and its generic overloads run a statement that returns rows and materialize them into objects or scalars. `Execute` runs a statement that does not return a result set — inserts, updates, deletes — and returns the number of rows affected.

- `Query<T>(sql, param)` yields zero or more mapped instances of `T`.
- `Execute(sql, param)` maps to ADO.NET `ExecuteNonQuery` and returns an `int` row count.
- Use `Query` for `SELECT` statements and `Execute` for data manipulation language (DML) without result grids.
- Both accept the same parameter objects and honor the connection's current transaction.

---

#### Q2. When do you use `Query<T>` versus `QueryFirstOrDefault<T>`?

**Answer:** Use `Query<T>` when you expect zero or many rows and want a sequence to iterate or materialize. Use `QueryFirstOrDefault<T>` when you want at most one row — it returns the first match or `default(T)` if the result set is empty.

- `Query<T>` returns `IEnumerable<T>` (deferred unless buffered) for lists and filters.
- `QueryFirstOrDefault<T>` executes immediately for single-row lookups such as "get by id when row may not exist."
- `QuerySingle<T>` throws if zero or more than one row exists — use only when uniqueness is guaranteed.
- Picking `QuerySingle` for optional lookups is a common bug; prefer `QueryFirstOrDefault` when absence is valid.

---

#### Q3. What does `Execute` return, and when is it used?

**Answer:** `Execute` returns an integer count of rows affected by the command, matching ADO.NET `ExecuteNonQuery` semantics. Use it for inserts, updates, deletes, and any non-query stored procedure that does not return a grid.

- A return value of `0` means no rows matched the statement — often expected for idempotent deletes, sometimes a bug for updates.
- Does not return generated keys; use `ExecuteScalar` or an `OUTPUT` clause when you need the new identity value.
- Participates in an ambient transaction when the connection has an active `IDbTransaction`.
- Async counterpart `ExecuteAsync` is preferred in ASP.NET Core request handlers to avoid blocking threads.

---

#### Q4. Does Dapper open the connection if it is closed when you call `Query`?

**Answer:** Yes. Dapper checks `ConnectionState` and calls `Open()` (or `OpenAsync()` for async methods) before executing if the connection is closed. You are still responsible for disposing the connection so it returns to the pool.

- Opening inside Dapper does not bypass pooling — `SqlConnection` still draws from the pool when configured.
- If you manage connection lifetime externally, you may open once and reuse for multiple Dapper calls in the same scope.
- Dapper does not close the connection after the call unless you use helper overloads that accept `commandBehavior` with auto-close semantics in specific scenarios.
- Best practice remains `await using var connection = new SqlConnection(...)` and let disposal close the connection back to the pool.

---

#### Q5. What is the difference between buffered and unbuffered queries in Dapper?

**Answer:** By default Dapper buffers query results — it reads the entire result set into memory before returning, then closes the reader. With `buffered: false`, Dapper streams rows through a live `IDataReader` as you enumerate, using less memory but holding the connection open until enumeration completes.

- Buffered (default): safe to return results from the method and enumerate later; connection can be disposed after `.ToList()`.
- Unbuffered: lower memory for very large result sets but enumeration must finish before the connection is disposed.
- Returning deferred `IEnumerable<T>` from an unbuffered query after leaving the `using` block causes "connection is closed" errors.
- Always materialize with `.ToList()` inside the connection scope unless you deliberately stream within that scope.

---

#### Q6. What async methods does Dapper provide (`QueryAsync`, `ExecuteAsync`, etc.)?

**Answer:** Dapper mirrors its synchronous API with async counterparts that accept optional `CancellationToken` values: `QueryAsync`, `QueryFirstAsync`, `QueryFirstOrDefaultAsync`, `QuerySingleAsync`, `QuerySingleOrDefaultAsync`, `ExecuteAsync`, `ExecuteScalarAsync`, and `QueryMultipleAsync`. They use the underlying provider's async ADO.NET methods.

- Prefer async variants in ASP.NET Core so request threads are not blocked during network I/O to the database.
- `QueryAsync` still supports buffered and unbuffered modes — the same deferred-enumeration rules apply when unbuffered.
- `ExecuteScalarAsync` returns the first column of the first row for aggregates and identity retrieval.
- Cancellation tokens propagate to the provider when supported, allowing request aborts to cancel long-running queries.

---

## Chapter 03. Parameters, Stored Procedures & QueryMultiple

#### Q1. How do you pass parameters to a Dapper query?

**Answer:** Pass parameters as the second argument using an anonymous object, a `DynamicParameters` bag, a `Dictionary<string, object>`, or any object whose public properties match `@Name` placeholders in the SQL. Dapper maps property names to parameter names case-insensitively.

- Anonymous object: `new { Id = 42, Name = "Widget" }` binds `@Id` and `@Name`.
- `DynamicParameters` supports output parameters, table-valued parameters, and explicit database types.
- Positional names in SQL use `@` prefix for SQL Server; syntax follows the target provider.
- Property names must align with parameter names — `UserId` maps to `@UserId`, not `@user_id`, unless you alias in SQL.

---

#### Q2. How does Dapper prevent SQL injection?

**Answer:** Dapper sends user values as ADO.NET parameters separate from the SQL text, so input is never interpreted as executable SQL syntax. As long as you use `@placeholders` with parameter objects and do not concatenate user input into the SQL string, injection is prevented.

- Safe: `"SELECT * FROM Users WHERE Email = @Email", new { Email = userInput }`.
- Unsafe: `$"SELECT * FROM Users WHERE Email = '{userInput}'"` — Dapper cannot fix inlined strings.
- `DynamicParameters.Add("Email", value)` still parameterizes even when building SQL dynamically with fixed structure.
- Stored procedure names should be fixed literals; only parameter values come from user input.

---

#### Q3. How do you call a stored procedure with Dapper?

**Answer:** Set `commandType: CommandType.StoredProcedure` and pass the procedure name as the SQL argument, with parameters bound the same way as ad hoc queries. Dapper routes the call through ADO.NET's stored procedure execution path.

- Example: `connection.Query<Product>("usp_GetProductsByCategory", new { CategoryId = 5 }, commandType: CommandType.StoredProcedure)`.
- Output and return-value parameters require `DynamicParameters` with `ParameterDirection.Output` or `ReturnValue`.
- `Execute` and `ExecuteAsync` work for non-query procedures that do not return rowsets.
- Result shape must still match the mapped type's properties — Dapper does not infer procedure result metadata beyond column names.

---

#### Q4. What is `QueryMultiple`, and when is it used?

**Answer:** `QueryMultiple` executes one batch or stored procedure that returns multiple result grids and exposes them through a `GridReader`. Use it when a single round-trip should return related datasets — for example, a header row plus detail lines — instead of two separate queries.

- Reduces network latency by combining multiple `SELECT` statements or a multi-result procedure.
- Returns `SqlMapper.GridReader` (via `using var multi = connection.QueryMultiple(...)`).
- Each result set is read sequentially — you cannot skip ahead arbitrarily without reading or skipping rows in order.
- Dispose the `GridReader` promptly to release the underlying reader and connection for reuse.

---

#### Q5. How do you read multiple result sets from `QueryMultiple`?

**Answer:** After `QueryMultiple`, call `Read<T>()`, `ReadFirst<T>()`, or `ReadFirstOrDefault<T>()` on the `GridReader` once per result set, in the order SQL Server returns them. Each `Read` consumes one grid and maps rows to `T`.

- First `multi.Read<Order>()` maps the first result set; second `multi.Read<OrderLine>()` maps the second.
- Mismatch between read order and SQL result order causes wrong-type mapping or empty sequences.
- Materialize each `Read` with `.ToList()` if you need the data after disposing the grid reader.
- Async equivalent: `QueryMultipleAsync` followed by `ReadAsync<T>()`.

---

#### Q6. When would you prefer `QueryMultiple` over separate round-trips?

**Answer:** Prefer `QueryMultiple` when two or more result sets are always needed together and combining them saves measurable latency, especially over high-latency networks. Separate round-trips are simpler when result sets are optional, independently cacheable, or large enough that sequential reads would block connection reuse.

- Dashboard endpoints that always load summary plus detail in one stored procedure benefit from one trip.
- High-latency cloud database links amplify the cost of each additional round-trip.
- Separate queries allow parallel execution on different connections when the database supports it and logic is independent.
- Very large second result sets may be better streamed in a dedicated query rather than held behind the first grid reader.

---

## Chapter 04. Mapping, Multi-Mapping & Advanced Patterns

#### Q1. How does Dapper map column names to property names by default?

**Answer:** Dapper matches result column names to public properties (and fields) on the target type using case-insensitive name equality. The first column value maps to the property with the same name regardless of column order in the `SELECT` list.

- `ProductName` column maps to `ProductName` property; `productname` also matches.
- Underscore naming such as `product_name` does not auto-map to `ProductName` without aliases or custom maps.
- Value types map directly; nullable value types accept database `NULL` as `null`.
- Column-to-member mapping is convention-based — no attributes required unless you add a custom type map.

---

#### Q2. What happens when column names do not match property names?

**Answer:** Unmatched columns are ignored, and unmatched properties remain at their default values (`null`, `0`, `false`). Dapper does not throw for missing mappings — silent partial objects are a common source of bugs.

- Fix with SQL aliases: `SELECT FirstName AS GivenName` to match `GivenName` property.
- Register custom column maps with `SqlMapper.SetTypeMap` or implement `ITypeHandler` for special conversions.
- Use `[Column("DbColumnName")]` when using Dapper.FluentMap or similar mapping extensions.
- Verify mappings in integration tests — empty strings where data exists often indicate a name mismatch.

---

#### Q3. What is multi-mapping in Dapper?

**Answer:** Multi-mapping lets one row with columns from a join map into multiple nested objects in a single `Query` call, using a delegate to assemble the parent-child graph. Overloads like `Query<Order, Customer, Order>` accept two or more types plus a `Func` that combines them.

- Typical pattern: `SELECT o.*, c.* FROM Orders o JOIN Customers c ...` mapped to `Order` with nested `Customer`.
- The split point between object types is controlled by the `splitOn` parameter.
- Dapper invokes the mapping function once per row — you decide how to attach the child to the parent.
- Useful for avoiding N+1 queries without loading flat DTOs and grouping manually in memory.

---

#### Q4. What is the `splitOn` parameter in multi-mapping?

**Answer:** `splitOn` names the column where the next object's properties begin in each row. Dapper maps columns before that name to the first type, columns from that name onward to the second type, and so on for additional generic type parameters.

- Default `splitOn` is `"Id"` — works when the second entity's first mapped column is `Id` and appears after the first entity's columns.
- Duplicate `Id` columns in a join require explicit aliases and a matching `splitOn` such as `"CustomerId"`.
- Wrong `splitOn` silently maps NULL or wrong values into nested objects instead of throwing.
- Column order in the `SELECT` list must align with the generic type order and split boundaries.

---

#### Q5. How does Dapper handle nested object graphs compared to EF Core `Include`?

**Answer:** Dapper has no navigation properties or automatic graph loading — you shape graphs explicitly with SQL joins, multi-mapping, or multiple queries. EF Core `Include` and `ThenInclude` translate into join or split queries from declared relationships on the model.

- Dapper: you write the join, choose flat or multi-map, and merge duplicates in code if one parent row repeats per child.
- EF Core: relationship metadata drives eager loading; change tracker deduplicates parent instances during fix-up.
- Dapper does not lazy-load — absent columns mean absent data with no hidden round-trips.
- EF Core cartesian explosion from multiple collection includes has no Dapper equivalent unless you write the wide join yourself.
- Dapper offers precise control; EF Core offers declarative relationship traversal at the cost of translation complexity.

---

## Entity Framework Core

## Chapter 01. Introduction to Entity Framework Core

#### Q1. What is Entity Framework Core?

**Answer:** Entity Framework Core (EF Core) is Microsoft's modern, cross-platform Object-Relational Mapper (ORM) for .NET. It maps C# entity classes to database tables and lets you query and persist data using LINQ, with provider plugins for SQL Server, PostgreSQL, SQLite, and other databases.

- EF Core 8 is the current long-term support (LTS) line aligned with .NET 8.
- Replaces Entity Framework 6 for new development on .NET Core and modern .NET.
- Ships as NuGet packages (`Microsoft.EntityFrameworkCore`, provider packages, design-time tools).
- Supports LINQ queries, change tracking, migrations, raw SQL, and compiled models for production tuning.

---

#### Q2. What is an ORM, and how does EF Core fit that definition?

**Answer:** An Object-Relational Mapper bridges the impedance mismatch between object-oriented C# code and relational database tables by mapping classes to tables, properties to columns, and object references to foreign keys. EF Core fits that definition by translating LINQ to SQL, materializing rows as entities, and persisting changes through a unit of work.

- You work with `Product` and `Category` objects instead of manual `INSERT` and `JOIN` strings for routine CRUD.
- Relationships express as navigation properties; EF Core generates join SQL when you query or include them.
- Schema can evolve from C# model changes through the migrations pipeline.
- The ORM layer sits between application code and ADO.NET, which still executes the final commands.

---

#### Q3. What is the difference between EF Core and EF6 (Entity Framework Classic)?

**Answer:** EF Core is a ground-up rewrite for cross-platform .NET with a modular provider model, while EF6 is the Windows-only, .NET Framework stack built on `System.Data.Entity`. EF Core prioritizes performance and cloud deployment; EF6 remains in maintenance for legacy applications.

- EF Core runs on Linux, macOS, and containers; EF6 targets .NET Framework and Windows-centric hosting.
- EF Core uses `DbContext` with fluent configuration and lightweight dependencies; EF6 uses `ObjectContext`/`DbContext` with EDMX and heavier design-time tooling.
- Some EF6 features arrived in EF Core over releases — stored procedure mapping, richer SQL Server types, and bulk operations in EF Core 7+.
- New projects on .NET 8 use EF Core; EF6 is for maintaining existing .NET Framework codebases.

---

#### Q4. What is code-first versus database-first in EF Core?

**Answer:** Code-first starts from C# entity classes and uses migrations to create or update the database schema from the model. Database-first starts from an existing database and scaffolds entity classes and a `DbContext` from live tables using reverse engineering.

- Code-first: developers own the model in source control; `dotnet ef migrations add` generates schema scripts.
- Database-first: the database is the schema authority; `dotnet ef dbcontext scaffold` generates C# from tables.
- Both approaches use the same runtime EF Core APIs once the model exists.
- Hybrid workflows scaffold once then hand-edit partial classes, but the model and database must stay synchronized.

---

#### Q5. When would you choose EF Core over ADO.NET or Dapper?

**Answer:** Choose EF Core when application logic benefits from LINQ composability, automatic change tracking, migrations, and relationship management rather than hand-written SQL for every operation. It fits domain-driven CRUD services, admin applications, and teams that want the database schema driven from C# types.

- Rapid development of standard create-read-update-delete APIs with validation and concurrency built in.
- Complex querying where LINQ composes filters, sorting, and paging without string-building SQL.
- Applications that need migration history, seed data, and repeatable schema deployment across environments.
- When relationship graphs, global query filters, and interceptors provide more value than raw SQL control.

---

#### Q6. What are the trade-offs of using EF Core?

**Answer:** EF Core trades some performance predictability and SQL transparency for productivity, abstraction, and cross-cutting features like migrations and change tracking. Misused LINQ can generate inefficient SQL; correctly used, it reduces boilerplate substantially.

- Advantage: less manual mapping code, unified model for reads and writes, strong tooling in Visual Studio and CLI.
- Disadvantage: learning curve for tracking behavior, query translation limits, and migration merge conflicts on teams.
- Heavy includes or untracked-vs-tracked confusion cause production performance issues if not understood.
- Raw SQL and Dapper remain valid escape hatches inside EF Core projects for hot paths.

---

#### Q7. How does EF Core translate C# queries into SQL?

**Answer:** EF Core builds an expression tree from LINQ methods chained on `IQueryable<T>`, passes it through a database provider's query compiler, and generates provider-specific SQL at execution time. The provider translates supported expression nodes; unsupported nodes cause client evaluation errors or exceptions in EF Core 3+.

- `Where`, `Select`, `OrderBy`, `Join`, and provider-specific functions like `EF.Functions.Like` translate when possible.
- Calling `ToList()`, `First()`, or `Count()` on the query triggers execution and SQL generation.
- EF Core logs generated SQL through `LogTo`, `EnableSensitiveDataLogging`, or interceptors for debugging.
- Compiled queries and EF Core 8 compiled models reduce repeated translation overhead in high-throughput apps.

---

#### Q8. How does EF Core handle schema evolution?

**Answer:** EF Core uses migrations — versioned C# files with `Up` and `Down` methods that apply incremental schema changes to the database based on differences from the current model snapshot. Teams apply migrations with `dotnet ef database update` or automated deployment pipelines.

- Each migration captures model changes as operations: add column, create index, alter relationship, and similar.
- A model snapshot records the full current model for diffing the next migration.
- Alternative `EnsureCreated()` creates the database once from the model but does not support migration history or upgrades.
- Production deployments typically run migrations in controlled release steps rather than at arbitrary application startup.

---

## Chapter 02. DbContext & DbSet

#### Q1. What is a `DbContext` in EF Core?

**Answer:** `DbContext` is the primary session with the database in EF Core — it coordinates querying, change tracking, and saving through a configured model. Each instance represents a unit of work for a logical operation such as one HTTP request.

- Exposes `DbSet<T>` properties as entry points for entity operations.
- `OnModelCreating` and `OnConfiguring` define mapping and provider options.
- `SaveChanges` and `SaveChangesAsync` persist tracked changes in a single transaction by default.
- Should be short-lived and disposed to release connections back to the pool.

---

#### Q2. What is a `DbSet<T>`?

**Answer:** A `DbSet<T>` represents a collection of entities of type `T` mapped to a database table or view and exposes LINQ query methods plus add, update, and remove operations. It is the typed gateway for querying and mutating a specific entity type through the context.

- `context.Products` returns `DbSet<Product>` for LINQ: `Where`, `Include`, `AsNoTracking`.
- `Add`, `AddRange`, `Update`, `Remove`, and `RemoveRange` mark entities for `SaveChanges`.
- `DbSet` implements `IQueryable<T>`, so LINQ providers translate chained methods before execution.
- Under the hood, EF Core tracks entities added or queried through the context's change tracker.

---

#### Q3. How do you register `DbContext` in ASP.NET Core dependency injection?

**Answer:** Call `services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString))` in `Program.cs` (or `Startup.cs` on older templates). Inject `AppDbContext` into controllers, services, or minimal API handlers through constructor injection.

- Connection string comes from configuration: `builder.Configuration.GetConnectionString("Default")`.
- Provider-specific options configure retries, split queries, and command timeouts.
- Design-time factory (`IDesignTimeDbContextFactory`) supports migrations CLI without running the web host.
- Register related interceptors and `DbContext` options in the same configuration delegate when needed.

---

#### Q4. Why is `DbContext` typically registered as scoped?

**Answer:** `DbContext` is not thread-safe and caches state for one logical unit of work, so it aligns with ASP.NET Core's per-request scope. A scoped registration creates one context instance per HTTP request, shared by all services in that request, then disposes it when the request completes.

- Singleton registration would share tracked entities across requests — incorrect data and thread-safety violations.
- Transient registration works but creates multiple contexts per request, breaking a single unit of work and wasting connections.
- Scoped lifetime matches `SaveChanges` boundaries: one request, one context, one commit pattern.
- Background workers use `IDbContextFactory<TContext>` to create short-lived contexts outside request scope.

---

#### Q5. What is the difference between injecting `DbContext` and using `IDbContextFactory<TContext>`?

**Answer:** Injecting `DbContext` directly gives the request-scoped instance managed by dependency injection. `IDbContextFactory<TContext>` creates fresh `DbContext` instances on demand, which is required for singleton services, parallel work, or long-running background tasks.

- Register factory with `AddDbContextFactory<AppDbContext>` alongside or instead of scoped `AddDbContext`.
- Factory-created contexts must be disposed explicitly: `await using var context = await factory.CreateDbContextAsync()`.
- Avoid storing injected scoped `DbContext` in singleton fields — captive dependency anti-pattern.
- Factory supports multi-threaded scenarios where each thread needs its own isolated context.

---

#### Q6. What does `OnModelCreating` do in a `DbContext`?

**Answer:** `OnModelCreating` is the override where you configure the entity model using the Fluent API — keys, relationships, indexes, column types, and table names — beyond what conventions infer from property names. EF Core calls it once when building the model for that context type.

- `modelBuilder.Entity<Product>().HasKey(p => p.Sku)` sets explicit keys.
- `ApplyConfigurationsFromAssembly` loads `IEntityTypeConfiguration<T>` classes for modular mapping.
- Configuration here merges with data annotations; Fluent API wins conflicts on the same facet.
- Changes require a new migration in code-first workflows to update the database schema.

---

#### Q7. What is `EnsureCreated`, and how does it differ from migrations?

**Answer:** `Database.EnsureCreated()` creates the database and schema from the current model if they do not exist, without using the migrations history table. It is intended for prototypes and tests — it cannot upgrade an existing schema when the model changes and should not replace migrations in production.

- Migrations apply incremental, versioned changes and support rollbacks via `Down` methods.
- `EnsureCreated` skips migration history — combining both on the same database causes conflicts.
- `EnsureCreatedAsync` exists for async bootstrap in test fixtures.
- Production EF Core 8 applications rely on `dotnet ef database update` or scripted migration bundles instead.

---

#### Q8. Can you reuse one `DbContext` across multiple threads?

**Answer:** No. `DbContext` is not thread-safe — concurrent operations on the same instance cause undefined behavior, corrupted change tracking, and intermittent exceptions. Each parallel task needs its own context instance, typically from `IDbContextFactory<TContext>`.

- ASP.NET Core handles concurrency by scoping one context per request on a single thread.
- Parallel LINQ over one `DbSet` with shared context is unsafe even for read-only queries.
- Use separate contexts and merge results in memory, or serialize database access per context.
- Thread safety applies to all context operations including `SaveChanges`, queries, and explicit loading.

---

## Chapter 03. Code-First Models & Migrations

#### Q1. What is code-first in EF Core?

**Answer:** Code-first means your C# entity classes and Fluent API configuration are the source of truth for the database schema. EF Core compares the model to the database and generates migrations to create or alter tables, columns, keys, and indexes.

- Entity classes live in the application project under `Models/` or a dedicated domain layer.
- Conventions map `Id` or `{TypeName}Id` to primary keys and `{TypeName}` navigation to foreign keys.
- Data annotations (`[Required]`, `[MaxLength]`, `[Table]`) supplement or override conventions.
- The database is created or updated by applying migrations rather than manual SQL scripts in application code.

---

#### Q2. What is a migration in EF Core?

**Answer:** A migration is a named, timestamped folder containing a `Migration` class with `Up` and `Down` methods that describe schema changes, plus an updated model snapshot. It is the version-controlled record of how the database evolved to match the entity model.

- Generated by `dotnet ef migrations add MigrationName` using the design-time model diff.
- `Up` applies changes forward; `Down` reverses them for rollback scenarios.
- Applied to databases with `dotnet ef database update` or programmatic `context.Database.Migrate()`.
- Teams commit migration files to source control alongside the entity changes that caused them.

---

#### Q3. How do you create and apply migrations from the CLI?

**Answer:** Install the EF Core tools (`dotnet tool install --global dotnet-ef`), ensure a design-time factory or startup project is configured, then run `dotnet ef migrations add InitialCreate` to scaffold a migration and `dotnet ef database update` to apply pending migrations to the target database.

- `--project` points to the project containing the `DbContext`; `--startup-project` to the executable host.
- `dotnet ef migrations list` shows applied and pending migrations against a connection.
- `dotnet ef migrations script` generates idempotent SQL for deployment pipelines that cannot run the CLI.
- EF Core 8 supports migration bundles as self-contained executables for controlled production runs.

---

#### Q4. What is the difference between `Up` and `Down` in a migration?

**Answer:** The `Up` method contains operations applied when moving forward to this migration — creating tables, adding columns, creating indexes. The `Down` method reverses those operations so the database can roll back to the prior migration state.

- `dotnet ef database update` runs `Up` on each pending migration in order.
- `dotnet ef database update PreviousMigrationName` runs `Down` on migrations after the target.
- Hand-editing `Up`/`Down` is acceptable for data fixes or complex transformations not inferred automatically.
- Production rollbacks often prefer forward-fix migrations over running `Down` on live data.

---

#### Q5. What is a model snapshot in EF Core migrations?

**Answer:** The model snapshot (`*ModelSnapshot.cs`) is a serialized picture of the entire EF Core model after all migrations to date. The tooling diffs the current entity model against this snapshot to generate the next migration's operations.

- Updated automatically every time you add a migration — do not edit manually except in merge conflict resolution.
- Without an accurate snapshot, the next migration may emit duplicate or missing operations.
- Merge conflicts in snapshot files require careful resolution so the model matches the team's intended state.
- The snapshot is design-time metadata; it is not executed at application runtime.

---

#### Q6. What happens if you change a model without creating a migration?

**Answer:** The C# model and the database schema drift apart — runtime queries may fail against missing columns, or EF Core may read wrong types from stale columns. `SaveChanges` might succeed on some properties while others silently map to nonexistent or mismatched columns until a query exposes the error.

- Development databases appear to work until a new property is queried or persisted.
- Other developers' databases remain on the old schema until they apply pending migrations.
- Production deployments without migration steps cause startup failures or data corruption.
- Always add and apply a migration when changing keys, relationships, required fields, or column types.

---

#### Q7. What is the difference between `EnsureCreated` and migrations-based schema creation?

**Answer:** `EnsureCreated` builds the database from the current model in one step with no migration history table, suitable for throwaway tests. Migrations apply ordered, incremental changes tracked in `__EFMigrationsHistory`, supporting upgrades, team collaboration, and production deployment.

- `EnsureCreated` fails or behaves unpredictably if the database already exists with a different shape.
- Migrations support evolving a live database through dozens of releases without dropping data.
- Never mix `EnsureCreated` and migrations on the same database — EF Core documents this as unsupported.
- Integration tests often use `EnsureCreated` or `EnsureDeleted` plus `EnsureCreated`; production uses migrations.

---

#### Q8. When should migrations run automatically in production?

**Answer:** Migrations should run in controlled deployment steps — pipeline job, init container, or maintenance window — not blindly on every application instance startup. Automatic startup migration risks race conditions when multiple nodes start simultaneously and makes rollbacks harder to coordinate.

- Preferred: run `dotnet ef database update`, a migration bundle, or generated SQL script once per release before or during traffic switch.
- Acceptable: single-instance admin service calls `context.Database.Migrate()` when no other writer competes.
- Avoid: every pod in a Kubernetes deployment racing to migrate on boot without coordination.
- Always back up production databases before applying migrations that drop columns or reshape data.

---

## Chapter 04. Database-First & Reverse Engineering

#### Q1. What is database-first in EF Core?

**Answer:** Database-first in EF Core means the relational schema already exists — often maintained by database administrators or legacy systems — and you generate C# entity classes and a `DbContext` from live tables using reverse engineering (scaffolding). The database remains the authoritative schema definition.

- Common when integrating with existing enterprise databases or stored-procedure-heavy systems.
- Scaffolding reads table metadata through the provider's information schema queries.
- Generated code reflects current columns, keys, and relationships as EF Core interprets them.
- Ongoing schema changes require re-scaffolding or manual model updates to stay aligned.

---

#### Q2. How do you scaffold a `DbContext` from an existing database?

**Answer:** Run `dotnet ef dbcontext scaffold "<connection-string>" Microsoft.EntityFrameworkCore.SqlServer` (or another provider package) with options for output directory, context name, and table filters. The command generates entity classes and a `DbContext` with `DbSet` properties and Fluent configuration.

- `--output-dir Models` and `--context-dir Data` organize generated files.
- `--tables Orders,Products` limits scaffolding to specific tables; `--schema dbo` filters by schema.
- `--data-annotations` emits attributes instead of fluent calls in `OnModelCreating`.
- Use `--force` to overwrite prior scaffold output after intentional regeneration.

---

#### Q3. When is database-first preferred over code-first?

**Answer:** Database-first fits when the database predates the application, multiple clients share one schema, or organizational policy requires database administrators to own all structural changes. It also suits reporting over vendor databases where you cannot dictate schema from C#.

- Legacy modernization layers a .NET API over unchanged SQL Server schemas.
- Regulated environments where schema changes go through database change advisory boards, not application migrations alone.
- Read-heavy integration with third-party databases where scaffolding is faster than manual model authoring.
- Less ideal for greenfield apps where the team wants model-driven evolution entirely in the .NET repository.

---

#### Q4. What are the limitations of reverse-engineered models?

**Answer:** Scaffolded models reflect the database at one point in time and may include naming mismatches, missing navigation ergonomics, and no domain logic. They can misinterpret views, triggers, complex keys, or provider-specific types without manual correction.

- Table and column names map literally — `cust_nm` becomes awkward property names unless renamed in partial classes.
- Not all database constructs scaffold cleanly: table-valued functions, certain composite keys, or undocumented views.
- Re-scaffolding overwrites generated files unless you isolate custom code in partial classes.
- Scaffolding does not capture business rules enforced only in triggers or check constraints as C# validation.

---

#### Q5. How do you refresh a scaffolded model after database schema changes?

**Answer:** Re-run `dotnet ef dbcontext scaffold` with the same options and `--force` to regenerate entities and context, then merge any custom partial class extensions that were not overwritten. Alternatively, hand-edit the model and add a code-first migration if you have switched to owning schema from the application.

- Compare diffs carefully — regenerated files replace prior scaffold output entirely.
- Keep custom logic in `*.Partial.cs` files or separate configuration classes excluded from overwrite.
- For small changes, manual entity updates plus a migration may be less disruptive than full re-scaffold.
- Document the scaffold command in the repository so the team reproduces identical output.

---

#### Q6. Can you combine scaffolded models with manual partial classes?

**Answer:** Yes. EF Core scaffolding generates partial classes intentionally — you add `Product.Partial.cs` with the same `partial class Product` to attach computed properties, methods, or interfaces without touching regenerated files. Partial `DbContext` classes extend `OnModelCreating` with hand-written Fluent API.

- Regenerated scaffold files stay overwrite-safe; partials persist across re-scaffold.
- Use partials for `[NotMapped]` properties, validation helpers, and domain behavior.
- Additional `IEntityTypeConfiguration<T>` classes configure mapping without editing generated context code.
- This hybrid pattern is the standard way to maintain database-first models long term.

---

## Chapter 05. CRUD Operations & SaveChanges

#### Q1. How do you insert, update, and delete entities with EF Core?

**Answer:** Add new entities with `context.Set<T>().Add(entity)` or `AddRange`, mark updates by modifying tracked entities or calling `Update`, remove with `Remove` or `RemoveRange`, then persist with `await context.SaveChangesAsync()`. EF Core generates insert, update, and delete statements from change tracker state.

- Insert: create object, `Add`, `SaveChanges` — identity keys populate after save when configured as store-generated.
- Update: query entity, mutate properties, `SaveChanges` — only changed columns appear in `UPDATE` when tracking detects modifications.
- Delete: `Remove(entity)` marks `Deleted`; `SaveChanges` issues `DELETE`.
- Disconnected updates from APIs attach or use `Update` with caution to avoid overwriting unchanged columns.

---

#### Q2. What does `SaveChanges()` do?

**Answer:** `SaveChanges` (and `SaveChangesAsync`) commits all pending changes tracked by the context in a single database transaction by default. It generates SQL for added, modified, and deleted entities, executes it in dependency order, and updates store-generated values such as identity columns and row versions.

- Returns the number of state entries written to the database.
- Detects relationship changes and orders inserts to satisfy foreign key constraints.
- Raises `SavingChanges` and `SavedChanges` events and runs interceptors before and after persistence.
- If any statement fails, the transaction rolls back and no partial commit occurs within that `SaveChanges` call.

---

#### Q3. What is the difference between `Add`, `Update`, and `Remove` on a `DbSet`?

**Answer:** `Add` marks an entity as `Added` for insert on save. `Update` marks every mapped property as `Modified` for a full update (or attaches disconnected graphs as modified). `Remove` marks the entity as `Deleted` for delete on save.

- `Add` on an entity with an existing key value may throw or behave as update depending on key configuration — typically use `Add` only for new keys.
- `Update` is convenient for disconnected Data Transfer Objects from HTTP PUT but can overwrite columns not sent in the payload.
- `Remove` requires the entity to be known to the context — attach first if it came from the client with only an id.
- `Attach` plus setting `EntityState` manually offers finer control than blanket `Update`.

---

#### Q4. What is attach versus add when working with disconnected entities?

**Answer:** `Add` tells EF Core the entity is new and should be inserted. `Attach` registers an existing entity with the context as `Unchanged` without inserting — you then mark specific properties or the whole entity `Modified` for targeted updates.

- Web APIs often receive ids from clients — `Attach` plus `Modified` state updates without a prior query.
- `Add` on an entity with a non-zero key may attempt insert and violate primary key constraints.
- `context.Entry(entity).Property(e => e.Name).IsModified = true` updates one column after attach.
- `Update` is shorthand for attach-all-properties-as-modified on disconnected instances.

---

#### Q5. What is `ExecuteUpdate` / `ExecuteDelete` in EF Core 7+?

**Answer:** `ExecuteUpdate` and `ExecuteDelete` translate LINQ filters directly into SQL `UPDATE` and `DELETE` statements without loading entities into memory or running change tracking. EF Core 8 continues these APIs for high-performance bulk mutations on `IQueryable`.

- Example: `await context.Products.Where(p => p.Discontinued).ExecuteDeleteAsync()`.
- Example: `await context.Products.Where(p => p.Id == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.Price, newPrice))`.
- Bypasses the change tracker — no entity instances materialized, lower memory and CPU.
- Does not run domain logic on entities, cascade through application-level validators, or update navigation fix-up in memory.

---

#### Q6. What is the unit-of-work pattern in relation to `DbContext`?

**Answer:** The unit-of-work pattern groups multiple repository operations into one atomic persistence boundary. In EF Core, `DbContext` implements this naturally — all tracked changes across `DbSet` operations commit together in one `SaveChanges` call inside one transaction.

- One request handler accumulates adds, updates, and deletes then calls `SaveChangesAsync` once.
- Multiple `SaveChanges` calls in one context create separate transactions unless wrapped in an explicit transaction.
- Repositories sharing one injected context participate in the same unit of work automatically.
- Failure during `SaveChanges` rolls back the entire batch, preserving consistency across related tables.

---

#### Q7. How do you perform a bulk update without loading entities into memory?

**Answer:** Use `ExecuteUpdateAsync` (EF Core 7+) with a filtered `IQueryable` to push updates to the database in one SQL statement. For very large batches or provider-specific optimizations, raw SQL via `ExecuteSqlRawAsync` or third-party bulk extension libraries remain options.

- `ExecuteUpdateAsync` with `SetProperty` sets column values in SQL without selecting rows into the context.
- Ideal for flag updates, soft deletes, and price adjustments across many matching rows.
- Does not invoke interceptors or tracked-entity events per row the way individual updates would.
- When complex per-row logic is required in C#, batch with smaller queries or use keyset pagination instead of loading entire tables.

---


---

## Chapter 06. Relationships & Navigation Properties

#### Q1. What is a navigation property in EF Core?

**Answer:** A navigation property is a CLR property on an entity that represents a relationship to one or more related entities — a reference to a single related row or a collection of related rows. EF Core uses navigation properties to traverse associations in LINQ and to load related data through includes, lazy loading, or explicit loading.

- A reference navigation points to one related entity (for example `Order.Customer`).
- A collection navigation points to many related entities (for example `Customer.Orders`).
- Navigation properties are optional in the model but enable relationship traversal without manually joining foreign keys in every query.
- EF Core infers relationships from navigation properties paired with foreign key properties or configures them explicitly via Fluent API or data annotations.

---

#### Q2. What is the difference between one-to-many and many-to-many relationships?

**Answer:** A one-to-many relationship means one parent entity relates to many child entities through a foreign key on the child side. A many-to-many relationship means entities on both sides can relate to multiple rows on the other side, typically modeled with a join entity or implicit join table in EF Core 5+.

- One-to-many: the child table holds the foreign key (`Order.CustomerId` → `Customer.Id`).
- Many-to-many: neither side stores the other's key directly — EF Core 5+ can use a join entity or an implicit join table with two foreign keys.
- One-to-many navigation is usually a single reference on the many side and a collection on the one side.
- Many-to-many navigation is a collection on both sides (`Student.Courses` and `Course.Students`).

---

#### Q3. How do you configure a one-to-many relationship in code-first?

**Answer:** EF Core can infer one-to-many from navigation and foreign key properties by convention, or you configure it explicitly with Fluent API or data annotations when conventions are insufficient. The child entity must expose a foreign key property or shadow foreign key pointing to the parent primary key.

- By convention: add `public int CustomerId { get; set; }` and `public Customer Customer { get; set; }` on `Order` — EF Core wires the relationship automatically.
- Fluent API: `modelBuilder.Entity<Order>().HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId);`
- Data annotations: `[ForeignKey(nameof(Customer))]` on the FK property or `[InverseProperty]` when multiple relationships exist between the same types.
- Required versus optional relationships are controlled with `.IsRequired()` or nullable foreign key types.

---

#### Q4. What is a foreign key property in EF Core?

**Answer:** A foreign key property is the scalar column on the dependent entity that stores the primary key value of the related principal entity. EF Core maps it to a database FK constraint and uses it for relationship fix-up, cascade rules, and LINQ joins.

- It can be a CLR property (`public int CustomerId`) or a shadow property configured only in the model.
- The dependent entity is the side that holds the foreign key — in one-to-many, the "many" side typically owns the FK column.
- Changing the FK value reassigns the relationship without necessarily loading the related navigation object.
- Composite foreign keys require multiple properties and explicit configuration in Fluent API.

---

#### Q5. What is cascade delete in EF Core?

**Answer:** Cascade delete means when the principal (parent) entity is deleted, EF Core automatically deletes dependent (child) entities that reference it, matching `ON DELETE CASCADE` in the database. It is the default for required relationships in many configurations.

- Configured with `OnDelete(DeleteBehavior.Cascade)` in Fluent API or by convention for required FKs.
- When you call `Remove(parent)`, EF Core marks related children as `Deleted` if cascade is enabled.
- The database schema must also define the matching FK cascade rule for deletes executed outside EF Core.
- `DeleteBehavior.ClientCascade` deletes dependents in memory without relying on database cascade — used in specific scenarios.

---

#### Q6. When would you disable cascade delete?

**Answer:** Disable cascade delete when deleting a parent must not automatically remove children — for example when child records have independent business meaning, audit requirements, or soft-delete policies. Use `DeleteBehavior.Restrict` or `NoAction` to force explicit handling of dependents before deleting the principal.

- Lookup or reference tables where child rows should block parent deletion (`Restrict` throws on delete if dependents exist).
- Soft-delete patterns where children remain linked to an archived parent instead of being physically removed.
- Legacy databases where FK constraints use `NO ACTION` and database-enforced rules differ from EF defaults.
- Explicit orphan management gives clearer error messages and audit trails than silent cascade removal.

---

#### Q7. What is a many-to-many relationship in EF Core 5+?

**Answer:** In EF Core 5 and later, many-to-many can be modeled with skip navigation properties on both entities and an implicit join table managed by EF Core, without requiring a dedicated join entity class. EF Core creates and maps the join table automatically based on conventions.

- Both entities expose collection navigations (`Post.Tags` and `Tag.Posts`).
- EF Core generates a hidden join table (for example `PostTag`) with composite foreign keys to both sides.
- You can expose the join entity explicitly when you need extra columns on the link (for example `AssignedDate` on a `CourseEnrollment` entity).
- Migrations create the join table schema; querying either collection loads related entities through the join.

---

## Chapter 07. Fluent API & Data Annotations

#### Q1. What is the Fluent API in EF Core?

**Answer:** The Fluent API is a code-based configuration surface in `OnModelCreating` (or `IEntityTypeConfiguration<T>` classes) that describes the EF Core model using method chains instead of attributes on entity classes. It controls tables, keys, properties, relationships, indexes, and constraints with full expressiveness.

- Called on `ModelBuilder` — for example `modelBuilder.Entity<Product>().ToTable("Products").HasKey(p => p.Id);`
- Supports configurations that have no data annotation equivalent (composite keys, complex relationship rules, value conversions).
- Keeps persistence concerns out of domain entity classes when you prefer POCOs without attributes.
- `ApplyConfigurationsFromAssembly` discovers and applies all `IEntityTypeConfiguration<T>` implementations in an assembly.

---

#### Q2. When should you prefer Fluent API over data annotations?

**Answer:** Prefer Fluent API when configuration is complex, affects multiple types, has no annotation equivalent, or when you want to keep entity classes free of persistence attributes. Data annotations suit simple, visible constraints on individual properties.

- Composite keys, alternate keys, owned types, and detailed cascade/index tuning require Fluent API.
- Large models benefit from separate configuration classes rather than cluttering entities with `[Column]`, `[ForeignKey]`, and `[Index]` on every property.
- Cross-cutting rules (global naming, soft-delete filters, shared base configurations) belong in Fluent API or configuration classes.
- Data annotations remain fine for basic validation attributes (`[Required]`, `[MaxLength]`) that double as API validation in ASP.NET Core.

---

#### Q3. What is `IEntityTypeConfiguration<T>`?

**Answer:** `IEntityTypeConfiguration<T>` is an interface for encapsulating Fluent API configuration for a single entity type in its own class, implementing `Configure(EntityTypeBuilder<T> builder)`. It keeps `OnModelCreating` clean and groups all mapping rules for one entity in one place.

- Example: `public class OrderConfiguration : IEntityTypeConfiguration<Order> { public void Configure(EntityTypeBuilder<Order> builder) { ... } }`
- Register with `modelBuilder.ApplyConfiguration(new OrderConfiguration())` or `ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`.
- Each configuration class owns table name, keys, properties, relationships, and indexes for that entity.
- Supports testing and reuse — the same configuration applies across multiple `DbContext` types if needed.

---

#### Q4. How do Fluent API and annotations interact when both configure the same property?

**Answer:** When Fluent API and data annotations configure the same aspect of the model, Fluent API takes precedence and overrides the annotation. EF Core merges configurations but resolves conflicts in favor of explicit Fluent API calls.

- If `[MaxLength(100)]` is on a property and Fluent API sets `.HasMaxLength(200)`, the effective limit is 200.
- Avoid duplicating the same rule in both places — pick one source of truth per property to prevent confusion during maintenance.
- Some annotations (for example `[Key]`) and Fluent API can coexist when they agree; conflicting values silently favor Fluent API.
- Review generated migrations when changing configuration sources to ensure schema matches intent.

---

#### Q5. How do you configure indexes with Fluent API?

**Answer:** Define indexes in Fluent API with `HasIndex` on the entity builder, optionally marking them unique or naming them explicitly. Indexes speed up queries on filtered, sorted, or joined columns and enforce uniqueness at the database level.

- Single column: `builder.HasIndex(p => p.Sku);`
- Composite: `builder.HasIndex(p => new { p.LastName, p.FirstName });`
- Unique: `builder.HasIndex(p => p.Email).IsUnique();`
- Named index: `.HasDatabaseName("IX_Product_Sku")` — migrations emit `CREATE INDEX` statements accordingly.

---

## Chapter 08. LINQ to Entities & Query Patterns

#### Q1. What is the difference between LINQ to Objects and LINQ to Entities?

**Answer:** LINQ to Entities translates query expressions into SQL executed by the database provider, while LINQ to Objects runs operators in memory against CLR collections after data is already loaded. EF Core queries against `DbSet<T>` use LINQ to Entities until the query is materialized.

- LINQ to Entities: `context.Orders.Where(o => o.Total > 100)` becomes a SQL `WHERE` clause — filtering happens on the server.
- LINQ to Objects: after `ToList()`, subsequent `.Where` with non-translatable logic runs in application memory.
- Only expression-tree-compatible operations translate; arbitrary C# methods generally do not unless mapped via `EF.Functions` or user-defined function mapping.
- Accidentally switching to LINQ to Objects (via `AsEnumerable()` or early `ToList()`) pulls more data than intended.

---

#### Q2. What is `IQueryable<T>` versus `IEnumerable<T>` in EF Core queries?

**Answer:** `IQueryable<T>` represents a composable, deferred query that EF Core can still translate and combine into SQL before execution. `IEnumerable<T>` represents an in-memory sequence where further LINQ operators execute locally in .NET after materialization.

- `DbSet<T>` implements `IQueryable<T>` — each `.Where`, `.Select`, or `.OrderBy` extends the expression tree sent to the provider.
- Calling `ToList()`, `ToArray()`, or iterating with `foreach` executes the query and returns `IEnumerable<T>` backed by loaded objects.
- Returning `IQueryable<T>` from a repository allows callers to add filters that still translate to SQL — returning `IEnumerable<T>` does not.
- `AsQueryable()` on an in-memory list wraps it as `IQueryable` but uses LINQ to Objects, not EF Core translation.

---

#### Q3. When does query execution actually occur (deferred execution)?

**Answer:** EF Core defers execution until the query is enumerated or explicitly materialized — for example by calling `ToList()`, `First()`, `Count()`, `foreach`, or async equivalents like `ToListAsync()`. Building the query with chained operators does not hit the database until that terminal operation runs.

- `var q = context.Products.Where(p => p.Active);` — no SQL yet; `q` is an unevaluated `IQueryable`.
- `await q.ToListAsync()` — EF Core generates and executes SQL at this point.
- Multiple enumerations of the same `IEnumerable` result re-query unless cached; `IQueryable` re-executes on each materialization.
- `SaveChanges` is separate from query execution — it persists tracked changes, not read queries.

---

#### Q4. What is the N+1 query problem?

**Answer:** The N+1 problem occurs when one query loads a list of N parent entities and then accessing a related navigation property triggers one additional query per parent, totaling N+1 round-trips. It devastates API latency and database load under even moderate list sizes.

- Classic pattern: load 100 orders, then loop and read `order.Lines` — 1 + 100 queries.
- Caused by lazy loading, missing `Include`, or projection that omits needed related data.
- Fix with eager loading (`Include`), split queries, or a single `Select` projection that joins required fields.
- EF Core logging showing the same SQL template repeated with different parameter IDs is the usual diagnostic signal.

---

#### Q5. What causes client evaluation warnings or errors in EF Core?

**Answer:** Client evaluation happens when part of a LINQ expression cannot be translated to SQL and EF Core either throws (EF Core 3+) or warns and executes that portion in memory after fetching data. Non-translatable methods, custom delegates, and complex C# logic in `Where` or `Select` are common triggers.

- Calling instance methods, arbitrary lambdas, or .NET-only APIs inside queries often fails translation.
- `AsEnumerable()` before `Where` forces all prior data into memory, then filters client-side.
- EF Core 3+ throws `InvalidOperationException` for many patterns that EF6 silently client-evaluated — safer but requires query rewrites.
- Use `EF.Functions.Like`, provider-translatable methods, raw SQL, or database functions for logic the provider cannot express.

---

#### Q6. How do you filter, project, sort, and paginate with EF Core LINQ?

**Answer:** Compose standard LINQ operators on `IQueryable` so EF Core translates them to SQL: `Where` for filtering, `Select` for projection, `OrderBy`/`ThenBy` for sorting, and `Skip`/`Take` for pagination. Execute asynchronously in ASP.NET Core with `ToListAsync` or similar terminal methods.

- Filter: `context.Products.Where(p => p.CategoryId == id && p.Active)`
- Project: `.Select(p => new ProductDto { Id = p.Id, Name = p.Name })` — loads only needed columns.
- Sort: `.OrderBy(p => p.Name).ThenByDescending(p => p.CreatedUtc)`
- Paginate: `.Skip((page - 1) * pageSize).Take(pageSize)` — always pair with a stable `OrderBy` to avoid inconsistent pages.

---

#### Q7. What is the difference between `Select` projection and loading full entities?

**Answer:** `Select` projection translates to a SQL query that retrieves only the columns needed for the result shape, avoiding full entity materialization and change tracking. Loading full entities fetches all mapped columns and registers each row in the change tracker by default.

- Projection to DTOs or anonymous types reduces network I/O and memory — ideal for read-only API responses.
- Full entity queries return tracked `Product` instances suitable for updates via `SaveChanges`.
- Projection can flatten related data in one query: `.Select(o => new { o.Id, CustomerName = o.Customer.Name })`.
- `AsNoTracking()` on full-entity reads reduces tracking overhead but still loads all columns unlike targeted projection.

---

#### Q8. What is a global query filter in EF Core?

**Answer:** A global query filter is a LINQ predicate applied automatically to every query for an entity type, configured in `OnModelCreating` with `HasQueryFilter`. It enforces row-level rules such as soft delete, multi-tenancy, or active-record flags without repeating `Where` in every query.

- Example: `builder.HasQueryFilter(p => !p.IsDeleted);` — all queries exclude soft-deleted rows unless ignored.
- Tenant isolation: `builder.HasQueryFilter(o => o.TenantId == _tenantId);` when `_tenantId` is captured from a scoped service.
- Bypass with `IgnoreQueryFilters()` for admin or audit scenarios that must see all rows.
- Filters compose with user-specified `Where` clauses — both predicates appear in generated SQL.

---

#### Q9. How do you debug the SQL generated by EF Core?

**Answer:** Enable sensitive logging and log EF Core database commands to the console or your logging provider, or inspect queries with `ToQueryString()` on an `IQueryable` before execution. These tools show the exact SQL, parameters, and command timing during development.

- `optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)` or `EnableSensitiveDataLogging()` in development (never log parameter values in production indiscriminately).
- `var sql = query.ToQueryString();` — returns the translated SQL string for inspection without executing.
- Application Insights, Serilog, or `Microsoft.EntityFrameworkCore.Database.Command` log category capture command text at Information level.
- Tags and `TagWith("MyQuery")` annotate SQL comments so you can correlate logged statements to code locations.

---

## Chapter 09. Loading Related Data

#### Q1. What is the difference between eager loading, lazy loading, and explicit loading?

**Answer:** Eager loading fetches related data in the same query (or coordinated queries) up front using `Include`. Lazy loading fetches related data automatically when a navigation property is first accessed. Explicit loading runs a separate query on demand via `Entry(...).Collection(...).Load()` or `Reference(...).Load()`.

- Eager: `context.Orders.Include(o => o.Lines)` — related data available immediately, no extra queries on access.
- Lazy: requires proxies or lazy-loading proxies package; triggers SQL on first navigation access — risky after context disposal.
- Explicit: start with a stub entity or partial load, then call `Load()` when you know you need the related data.
- Eager and explicit give predictable query counts; lazy makes query count depend on code paths and property access order.

---

#### Q2. How do you use `Include` and `ThenInclude` for eager loading?

**Answer:** Chain `Include` on the root `DbSet` query to load related entities, and use `ThenInclude` to load deeper levels along a navigation path. EF Core translates includes into SQL JOINs or split queries depending on configuration.

- One level: `context.Orders.Include(o => o.Customer)`
- Deeper: `context.Orders.Include(o => o.Lines).ThenInclude(l => l.Product)`
- Multiple branches: repeat `Include` from the root — `Include(o => o.Customer).Include(o => o.Lines)`
- Filtered includes (EF Core 5+): `.Include(o => o.Lines.Where(l => l.Active))` loads only matching related rows.

---

#### Q3. What is the N+1 problem in the context of loading related data?

**Answer:** When listing parent entities without loading related navigations, each access to a child collection or reference in a loop fires a separate SQL query — one initial query plus N per-row queries. This is the relational-data manifestation of the N+1 anti-pattern.

- Occurs with lazy loading enabled or when developers forget `Include` on list endpoints.
- A 50-row list with one navigation access per row becomes 51 database round-trips per HTTP request.
- Detect via EF Core command logging or APM tools showing repeated identical queries with different keys.
- Resolve with `Include`, `AsSplitQuery`, projection, or batch explicit loading before the loop.

---

#### Q4. Why should you avoid lazy loading in ASP.NET Core applications?

**Answer:** Lazy loading triggers database queries during navigation property access, which often happens during JSON serialization or view rendering after the request-scoped `DbContext` is disposed or without the developer's explicit awareness. ASP.NET Core's stateless request model makes implicit loading unpredictable and expensive.

- Serializers accessing navigations cause "Cannot access a disposed context" or hidden N+1 query storms.
- Query count becomes data-dependent — endpoints perform differently based on which properties the client touches.
- Explicit `Include` or projection makes API contracts and performance predictable and testable.
- Lazy loading suits long-lived desktop contexts with UI-driven access patterns, not short HTTP request scopes.

---

#### Q5. What is a cartesian explosion when including multiple collections?

**Answer:** Cartesian explosion happens when a single SQL query JOINs two or more collection navigations, producing a rowset whose size is roughly the product of collection cardinalities — many duplicate parent rows over the wire before EF Core deduplicates in memory.

- Example: `Include(o => o.Lines).Include(o => o.Shipments)` on orders with 20 lines and 10 shipments can emit ~200 rows per order in SQL.
- Network and memory costs spike even though the final object graph has far fewer unique entities.
- EF Core fix-up reconstructs parents correctly, but the damage is already done at the SQL transport layer.
- Mitigate with `AsSplitQuery()`, separate queries, or projection to DTOs that avoid multi-collection joins.

---

#### Q6. What is `AsSplitQuery`, and when should you use it?

**Answer:** `AsSplitQuery()` tells EF Core to load included related data using multiple SQL queries instead of one large JOIN, avoiding cartesian explosion when including multiple collection navigations. Each include path gets its own SELECT while EF Core still assembles the object graph.

- Use when eager-loading two or more `Include` collection branches on the same root entity.
- Slightly more round-trips than a single join but far less data transferred when collections are large.
- Can be set globally via `UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)` or per query with `.AsSplitQuery()`.
- Single-reference includes (many-to-one) rarely need split queries — the problem is primarily multi-collection joins.

---

#### Q7. How do you choose between eager loading, explicit loading, and projection?

**Answer:** Choose based on how much related data you know you need at query time and whether the operation is read-only or tracked. Eager loading suits known graphs up front; explicit loading suits conditional related data; projection suits read-only API responses that need flat or partial shapes.

- Eager (`Include`): list/detail endpoints where the response always needs the same related entities.
- Explicit (`Load`): related data needed only in some branches — load when a flag or business rule triggers the need.
- Projection (`Select` to DTO): read-only APIs that never update entities — smallest payload, no tracking, no N+1.
- Avoid lazy loading in web APIs; combine split queries when eager-loading multiple collections.

---

## Chapter 10. Raw SQL & Stored Procedures

#### Q1. What is `FromSqlRaw` versus `FromSqlInterpolated`?

**Answer:** Both methods query entities with raw SQL instead of LINQ, but `FromSqlRaw` accepts a plain string (with optional `SqlParameter` objects) while `FromSqlInterpolated` accepts a `FormattableString` that EF Core converts into parameterized SQL. `FromSqlInterpolated` prevents accidental injection from interpolated values.

- `FromSqlRaw("SELECT * FROM Products WHERE CategoryId = {0}", id)` — placeholders become parameters when passed correctly.
- `FromSqlInterpolated($"SELECT * FROM Products WHERE CategoryId = {id}")` — compiler-generated `FormattableString` ensures parameterization.
- Never pass an ordinary interpolated string to `FromSqlRaw` — `$"..."` evaluated before the call embeds literals unsafely.
- Both require the SQL to map to the entity shape (column names compatible with the entity type).

---

#### Q2. Why is `FromSqlInterpolated` preferred over string interpolation for raw SQL?

**Answer:** `FromSqlInterpolated` treats each interpolated value as a separate SQL parameter, so user input never becomes part of the SQL text. Ordinary C# string interpolation builds a single literal string before EF Core sees it, reintroducing SQL injection risk identical to concatenation.

- `FormattableString` preserves argument boundaries — EF Core emits `WHERE Id = @p0` with bound parameters.
- `$"WHERE Name = '{name}'"` passed to `FromSqlRaw` embeds raw text — attackers can inject malicious SQL.
- Parameterization also improves plan cache reuse compared to ad hoc literal SQL per distinct value.
- Same safety model as parameterized ADO.NET commands and Dapper anonymous parameter objects.

---

#### Q3. When should you use raw SQL instead of LINQ in EF Core?

**Answer:** Use raw SQL when LINQ cannot express the query efficiently or at all — complex reporting queries, database-specific features, optimized hints, bulk reads of views, or legacy stored procedures. Raw SQL trades provider translation and compile-time checking for full control over the statement sent to the server.

- Window functions, recursive CTEs, or vendor-specific syntax unavailable in LINQ.
- Performance-critical queries where a hand-tuned plan outperforms generated EF SQL.
- Mapping to keyless entity types or database views for read-only reporting models.
- Calling existing stored procedures the organization already maintains in the database.

---

#### Q4. What are the security considerations for raw SQL in EF Core?

**Answer:** All dynamic values must reach the database as parameters, never as concatenated SQL text. Use `FromSqlInterpolated`, parameterized `FromSqlRaw`, or `ExecuteSqlRaw` with `SqlParameter` objects — and validate that no user-controlled input defines structural SQL elements (table or column names) without strict allowlisting.

- SQL injection remains possible if raw strings embed user input directly.
- `ExecuteSqlRaw` with `{0}` placeholders parameterizes values; string building before the call does not.
- Dynamic identifiers (sort columns, table names) cannot be parameterized — map user input to a fixed allowlist.
- Logging raw SQL in production should redact or avoid sensitive parameter values even when queries are parameterized.

---

#### Q5. How do you call stored procedures with EF Core?

**Answer:** Map stored procedure result sets to entity types or keyless types and invoke them with `FromSqlRaw` or `FromSqlInterpolated` using `EXEC` syntax, or use `ExecuteSqlRaw` for procedures that perform non-query work. Pass parameters as method arguments or `SqlParameter` instances.

- Query returning rows: `context.Products.FromSqlRaw("EXEC GetProductsByCategory @CategoryId = {0}", categoryId)`
- Interpolated: `context.Orders.FromSqlInterpolated($"EXEC usp_GetOrders @StartDate = {start}, @EndDate = {end}")`
- Non-query procedures: `context.Database.ExecuteSqlRaw("EXEC usp_ArchiveOrders @BeforeDate = {0}", date)`
- Result shape must match entity columns; keyless types suit procedures returning non-entity projections.

---

#### Q6. How do you execute non-query raw SQL (`ExecuteSqlRaw`)?

**Answer:** `ExecuteSqlRaw` and `ExecuteSqlInterpolated` run INSERT, UPDATE, DELETE, or DDL statements that do not return entity rows, returning the number of rows affected. They execute outside the change tracker — tracked entities in memory are not automatically updated.

- `await context.Database.ExecuteSqlRawAsync("UPDATE Products SET Active = 0 WHERE Discontinued = 1");`
- Parameterized: `ExecuteSqlRawAsync("DELETE FROM Logs WHERE CreatedUtc < {0}", cutoffDate)`
- Prefer `ExecuteUpdate`/`ExecuteDelete` (EF Core 7+) for bulk operations that EF can translate from LINQ when raw SQL is not required.
- Wrap multi-statement raw SQL in an explicit transaction when atomicity is required.

---

## Chapter 11. Change Tracking, Async & Transactions

#### Q1. What is change tracking in EF Core?

**Answer:** Change tracking is EF Core's mechanism for snapshotting entity state when loaded or attached, detecting modifications, and generating INSERT, UPDATE, and DELETE statements on `SaveChanges`. The `DbContext` maintains an entry per tracked entity with current and original values.

- Enabled by default for queries that return entity types without `AsNoTracking()`.
- The change tracker compares current property values to snapshots taken at query or attach time.
- Relationships and FK changes are tracked alongside scalar properties.
- Disabling tracking (`AsNoTracking`) skips snapshot overhead for read-only scenarios.

---

#### Q2. What entity states does EF Core track (`Added`, `Modified`, `Deleted`, `Unchanged`)?

**Answer:** EF Core assigns each tracked entity an `EntityState` describing what `SaveChanges` should do: `Added` inserts new rows, `Modified` updates changed rows, `Deleted` removes rows, and `Unchanged` skips entities with no detected changes. `Detached` means the entity is not in the tracker.

- `Add()` marks entities `Added` — INSERT on save even if they had a key value set manually.
- `Update()` marks all mapped properties `Modified` unless configured otherwise — issues a broad UPDATE.
- `Remove()` marks `Deleted` — DELETE on save; cascade rules apply to dependents.
- `Unchanged` entities are loaded but untouched; `Attach()` with unchanged values sets `Unchanged` until properties change.

---

#### Q3. What does `AsNoTracking` do, and when should you use it?

**Answer:** `AsNoTracking()` tells EF Core not to snapshot or track entities returned by a query, reducing memory and CPU for read-only operations. Use it on list endpoints, reports, and any query whose results will not be updated through the same `DbContext` instance.

- Tracked queries store original values for every property — unnecessary when you only serialize to JSON.
- `AsNoTrackingWithIdentityResolution()` deduplicates repeated instances in a single result without full change tracking.
- Set globally: `options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)` for read-heavy apps.
- Omit `AsNoTracking` when you load entities specifically to modify and call `SaveChanges` on the same context.

---

#### Q4. What is the performance impact of change tracking on read-heavy queries?

**Answer:** Tracking adds memory for original/current value snapshots and CPU for fix-up and change detection on every materialized entity. On large read-only lists, this overhead can significantly increase Gen2 pressure and request latency compared to equivalent `AsNoTracking` queries.

- A 10,000-row GET that tracks entities holds two value sets per property per row in the worst case.
- Identity resolution and relationship fix-up add cost beyond flat property snapshots.
- Projection to DTOs avoids both tracking and loading unused columns — often the best read-path optimization.
- The regression is silent until load testing — results are identical, only resource use differs.

---

#### Q5. Why use async EF Core methods in ASP.NET Core?

**Answer:** Async methods (`ToListAsync`, `SaveChangesAsync`, `FirstOrDefaultAsync`) release the request thread during I/O waits, improving scalability under concurrent load. ASP.NET Core thread pool threads are a shared resource — blocking them on database calls reduces throughput.

- Database latency is I/O-bound; async avoids thread starvation during waits for SQL Server responses.
- Pair with `CancellationToken` from `HttpContext.RequestAborted` so client disconnects cancel long queries.
- Never block async EF calls with `.Result` or `.Wait()` — causes deadlocks and defeats scalability benefits.
- Sync methods remain acceptable in console tools or single-threaded batch jobs with no concurrency pressure.

---

#### Q6. How do you begin and commit a transaction in EF Core?

**Answer:** Start a transaction on the context's database facade with `BeginTransactionAsync`, perform tracked changes and raw SQL, then call `CommitAsync` — or `RollbackAsync` on failure. All operations on the same `DbContext` instance participate in the transaction until commit or rollback.

- `await using var tx = await context.Database.BeginTransactionAsync();`
- Make changes, `await context.SaveChangesAsync();`, optionally run `ExecuteSqlRaw` on the same context.
- `await tx.CommitAsync();` — dispose rolls back if commit was not called.
- `SaveChanges` alone wraps each call in an implicit transaction for its own batch, not across multiple separate calls.

---

#### Q7. How does EF Core detect concurrency conflicts?

**Answer:** EF Core compares concurrency token values in the UPDATE or DELETE WHERE clause against the values read when the entity was loaded. If zero rows match because another transaction changed the row first, EF Core throws `DbUpdateConcurrencyException` on `SaveChanges`.

- Configure a concurrency token property with `[Timestamp]` / `rowversion` or `.IsConcurrencyToken()` on a property.
- Generated SQL includes `WHERE Id = @id AND RowVersion = @originalRowVersion`.
- No token configured means last-write-wins — later saves overwrite earlier changes silently.
- Optimistic concurrency suits web apps where simultaneous edits are possible but locking is undesirable.

---

#### Q8. What is a concurrency token or row version column?

**Answer:** A concurrency token is a property mapped to a database column whose value changes whenever the row is updated, used by EF Core to detect stale writes. SQL Server `rowversion` (`[Timestamp]` in EF) is the common choice — the database auto-increments it on every update.

- Mark with `[Timestamp]` on a `byte[]` property or Fluent API `.IsRowVersion()`.
- EF Core reads the token at query time and includes the original value in UPDATE/DELETE predicates.
- Any concurrent modification changes the token, causing the next save to affect zero rows and throw.
- Application-defined tokens (for example a manual `Version` integer) also work if incremented on each update.

---

#### Q9. How do you handle `DbUpdateConcurrencyException`?

**Answer:** Catch `DbUpdateConcurrencyException`, inspect `exception.Entries` for conflicting entities, and resolve by refreshing from the database, merging user changes, or returning a conflict response to the client. Production APIs typically return HTTP 409 with a message asking the user to reload and retry.

- Reload: `await entry.ReloadAsync()` discards stale client values and re-reads current database state.
- Client wins: reapply intended values after reload and retry `SaveChanges` if business rules allow overwriting.
- Server wins: return 409 Conflict with current row data so the UI can show what changed.
- Log concurrency conflicts for monitoring — frequent conflicts may indicate UX or workflow design issues.

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
