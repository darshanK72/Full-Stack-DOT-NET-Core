# 01. ADO.NET — Interview Q&A
> Back to [README](../README.md)

## Table of Contents

- [Chapter 01. Introduction to ADO.NET](#chapter-01-introduction-to-adonet)
  - [Q1. What is ADO.NET?](#q1-what-is-adonet)
  - [Q2. What is the difference between connected and disconnected data access in ADO.NET?](#q2-what-is-the-difference-between-connected-and-disconnected-data-access-in-adonet)
  - [Q3. What are the core building blocks of ADO.NET (connection, command, reader, adapter)?](#q3-what-are-the-core-building-blocks-of-adonet-connection-command-reader-adapter)
  - [Q4. What is the difference between ADO.NET and an ORM like Entity Framework Core?](#q4-what-is-the-difference-between-adonet-and-an-orm-like-entity-framework-core)
  - [Q5. When would you choose ADO.NET over Dapper or EF Core?](#q5-when-would-you-choose-adonet-over-dapper-or-ef-core)
  - [Q6. What is the connected model, and which ADO.NET types does it primarily use?](#q6-what-is-the-connected-model-and-which-adonet-types-does-it-primarily-use)
  - [Q7. What is the disconnected model, and which ADO.NET types does it primarily use?](#q7-what-is-the-disconnected-model-and-which-adonet-types-does-it-primarily-use)
  - [Q8. What are the trade-offs of hand-written SQL versus a higher-level ORM?](#q8-what-are-the-trade-offs-of-hand-written-sql-versus-a-higher-level-orm)

- [Chapter 02. SqlConnection & Connection Strings](#chapter-02-sqlconnection-connection-strings)
  - [Q1. What is connection pooling in ADO.NET?](#q1-what-is-connection-pooling-in-adonet)
  - [Q2. Does creating `new SqlConnection()` every time open a new physical database connection?](#q2-does-creating-new-sqlconnection-every-time-open-a-new-physical-database-connection)
  - [Q3. Why should you use `using` or `await using` with connections?](#q3-why-should-you-use-using-or-await-using-with-connections)
  - [Q4. How do you store connection strings securely in ASP.NET Core?](#q4-how-do-you-store-connection-strings-securely-in-aspnet-core)
  - [Q5. What is the difference between `Microsoft.Data.SqlClient` and `System.Data.SqlClient`?](#q5-what-is-the-difference-between-microsoftdatasqlclient-and-systemdatasqlclient)
  - [Q6. What is a pool exhaustion error, and what typically causes it?](#q6-what-is-a-pool-exhaustion-error-and-what-typically-causes-it)
  - [Q7. What symptoms indicate a misconfigured or exhausted connection pool?](#q7-what-symptoms-indicate-a-misconfigured-or-exhausted-connection-pool)
  - [Q8. How do unclosed connections affect pool availability?](#q8-how-do-unclosed-connections-affect-pool-availability)

- [Chapter 03. SqlCommand & Parameters](#chapter-03-sqlcommand-parameters)
  - [Q1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?](#q1-what-is-the-difference-between-executereader-executenonquery-and-executescalar)
  - [Q2. What is a parameterized query, and why is it preferred over string concatenation?](#q2-what-is-a-parameterized-query-and-why-is-it-preferred-over-string-concatenation)
  - [Q3. What is SQL injection, and how do parameters prevent it?](#q3-what-is-sql-injection-and-how-do-parameters-prevent-it)
  - [Q4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?](#q4-what-is-the-difference-between-addwithvalue-and-explicitly-typed-sqlparameter)
  - [Q5. What is the difference between `Text` and `StoredProcedure` command types?](#q5-what-is-the-difference-between-text-and-storedprocedure-command-types)
  - [Q6. When would you use `ExecuteScalar` instead of `ExecuteReader`?](#q6-when-would-you-use-executescalar-instead-of-executereader)

- [Chapter 04. SqlDataReader](#chapter-04-sqldatareader)
  - [Q1. Why is `SqlDataReader` described as a forward-only, read-only cursor?](#q1-why-is-sqldatareader-described-as-a-forward-only-read-only-cursor)
  - [Q2. What is the difference between connected streaming reads and loading everything into memory?](#q2-what-is-the-difference-between-connected-streaming-reads-and-loading-everything-into-memory)
  - [Q3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?](#q3-why-must-a-sqldatareader-be-closed-or-disposed-before-running-another-command-on-the-same-connection-without-mars)
  - [Q4. When is `SqlDataReader` the best choice for large result sets?](#q4-when-is-sqldatareader-the-best-choice-for-large-result-sets)
  - [Q5. How do you handle NULL database values when reading from a data reader?](#q5-how-do-you-handle-null-database-values-when-reading-from-a-data-reader)
  - [Q6. What performance advantage does a reader have over filling a `DataTable`?](#q6-what-performance-advantage-does-a-reader-have-over-filling-a-datatable)
  - [Q7. What happens if you do not dispose a data reader?](#q7-what-happens-if-you-do-not-dispose-a-data-reader)

- [Chapter 05. DataSet, DataTable & SqlDataAdapter](#chapter-05-dataset-datatable-sqldataadapter)
  - [Q1. What is the disconnected model that `DataSet`/`DataTable` support?](#q1-what-is-the-disconnected-model-that-datasetdatatable-support)
  - [Q2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?](#q2-what-is-the-difference-between-datareader-streaming-and-dataadapterfill)
  - [Q3. When would you still use `DataSet`/`DataTable` in modern .NET applications?](#q3-when-would-you-still-use-datasetdatatable-in-modern-net-applications)
  - [Q4. What are the memory implications of filling a large table into a `DataSet`?](#q4-what-are-the-memory-implications-of-filling-a-large-table-into-a-dataset)
  - [Q5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?](#q5-why-are-datasetdatatable-less-common-in-aspnet-core-apis-than-in-older-winforms-apps)

- [Chapter 06. Transactions & Connection Pooling](#chapter-06-transactions-connection-pooling)
  - [Q1. What are the ACID properties of a transaction?](#q1-what-are-the-acid-properties-of-a-transaction)
  - [Q2. How do you begin, commit, and rollback a transaction in ADO.NET?](#q2-how-do-you-begin-commit-and-rollback-a-transaction-in-adonet)
  - [Q3. Why must all commands in a transaction share the same connection?](#q3-why-must-all-commands-in-a-transaction-share-the-same-connection)
  - [Q4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?](#q4-what-is-transactionscope-and-how-does-it-differ-from-sqltransaction)
  - [Q5. What is a pool exhaustion error, and what typically causes it?](#q5-what-is-a-pool-exhaustion-error-and-what-typically-causes-it)
  - [Q6. What isolation levels exist, and why do they matter?](#q6-what-isolation-levels-exist-and-why-do-they-matter)
  - [Q7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?](#q7-what-is-the-correct-pattern-for-rollback-in-a-trycatch-around-adonet-transactions)

- [Chapter 07. Stored Procedures & Output Parameters](#chapter-07-stored-procedures-output-parameters)
  - [Q1. What is a stored procedure, and why use one from ADO.NET?](#q1-what-is-a-stored-procedure-and-why-use-one-from-adonet)
  - [Q2. How do you execute a stored procedure with `SqlCommand`?](#q2-how-do-you-execute-a-stored-procedure-with-sqlcommand)
  - [Q3. What is the difference between output parameters and return values (`ReturnValue`)?](#q3-what-is-the-difference-between-output-parameters-and-return-values-returnvalue)
  - [Q4. When are stored procedures preferred over inline SQL in ADO.NET?](#q4-when-are-stored-procedures-preferred-over-inline-sql-in-adonet)
  - [Q5. What are the trade-offs of putting business logic in stored procedures versus C#?](#q5-what-are-the-trade-offs-of-putting-business-logic-in-stored-procedures-versus-c)

- [Chapter 08. Async ADO.NET](#chapter-08-async-adonet)
  - [Q1. Why should database I/O be async in ASP.NET Core request handlers?](#q1-why-should-database-io-be-async-in-aspnet-core-request-handlers)
  - [Q2. What does `await using` provide when working with connections and readers?](#q2-what-does-await-using-provide-when-working-with-connections-and-readers)
  - [Q3. What problems arise from calling `.Result` or `.Wait()` on async ADO.NET operations?](#q3-what-problems-arise-from-calling-result-or-wait-on-async-adonet-operations)
  - [Q4. What is `CancellationToken` support in async ADO.NET methods?](#q4-what-is-cancellationtoken-support-in-async-adonet-methods)
  - [Q5. What is the recommended async pattern for opening a connection, executing a command, and reading results?](#q5-what-is-the-recommended-async-pattern-for-opening-a-connection-executing-a-command-and-reading-results)
  - [Q6. When is synchronous ADO.NET still acceptable?](#q6-when-is-synchronous-adonet-still-acceptable)
- [Gotchas](#gotchas)
- [Scenario-Based Questions](#scenario-based-questions-karat-format)

---

## Chapter 01. Introduction to ADO.NET

### Q1. What is ADO.NET?

**Concepts**
- Low-level .NET data access API over provider-specific drivers
- Provider packages such as `Microsoft.Data.SqlClient`
- Connected and disconnected access models
- Foundation beneath Dapper and EF Core

**Answer**

ADO.NET is .NET's low-level data access API that communicates with databases through provider-specific packages. You write SQL explicitly, manage connections and transactions, and map result columns to objects manually — there is no ORM layer. Higher-level libraries like Dapper and EF Core are built on top of ADO.NET providers and use them internally.

---

### Q2. What is the difference between connected and disconnected data access in ADO.NET?

**Concepts**
- Connected model: live connection while streaming rows
- Disconnected model: fill DataSet then close connection
- Memory trade-off: streaming vs in-memory snapshot
- Use cases: APIs vs offline editing or grid binding

**Answer**

Connected access keeps a live `SqlConnection` open while rows stream through a `SqlDataReader`, using minimal memory since only one row is active at a time. Disconnected access fills a `DataTable` or `DataSet` via `SqlDataAdapter.Fill`, closes the connection, and lets code work against an in-memory copy. Connected mode suits read-heavy sequential processing; disconnected mode suits offline editing or binding UI grids.

---

### Q3. What are the core building blocks of ADO.NET (connection, command, reader, adapter)?

**Concepts**
- `DbConnection` — network session and pooling state
- `DbCommand` — SQL/stored procedure, parameters, timeout, transaction
- `DbDataReader` — forward-only, read-only row stream
- `DbDataAdapter` — disconnected bridge for Fill and Update

**Answer**

The four ADO.NET pillars are `DbConnection` (network session), `DbCommand` (SQL or stored procedure plus parameters), `DbDataReader` (forward-only row stream), and `DbDataAdapter` (fills and updates disconnected `DataTable`/`DataSet`). Provider-specific implementations — `SqlConnection`, `SqlCommand`, etc. — wrap these base types. Together they cover every pattern from streaming reads to batched disconnected updates.

---

### Q4. What is the difference between ADO.NET and an ORM like Entity Framework Core?

**Concepts**
- ADO.NET as thin provider API (manual SQL and mapping)
- EF Core ORM: LINQ-to-SQL, change tracking, migrations
- Control vs productivity trade-off
- EF Core uses ADO.NET internally

**Answer**

ADO.NET is a thin provider API where you write SQL, map columns manually, and manage connections and transactions explicitly. EF Core is an ORM that maps C# classes to tables, translates LINQ to SQL, tracks entity state, and manages schema through migrations. ADO.NET offers full SQL control with minimal overhead; EF Core trades some control for faster CRUD development — and still opens ADO.NET connections internally when executing generated SQL.

---

### Q5. When would you choose ADO.NET over Dapper or EF Core?

**Concepts**
- Bulk operations (`SqlBulkCopy`) with zero ORM overhead
- Provider-specific features not exposed by higher layers
- ETL pipelines and stored-procedure-heavy legacy systems
- Hot paths needing predictable execution plans

**Answer**

Choose raw ADO.NET when you need `SqlBulkCopy`, fine-grained command timeouts, or provider-specific APIs that ORMs do not expose. It is the right fit for ETL pipelines, legacy databases driven almost entirely through stored procedures, and performance-critical paths where even micro-ORM reflection overhead matters. Reporting or admin scripts that run ad hoc SQL without maintaining entity classes also benefit from the simplicity of plain ADO.NET.

---

### Q6. What is the connected model, and which ADO.NET types does it primarily use?

**Concepts**
- Live `DbConnection` open during command execution
- `SqlConnection`, `SqlCommand`, `SqlDataReader` trio
- Dispose reader before next command (no MARS)
- Connection returned to pool on Dispose

**Answer**

The connected model keeps a live `SqlConnection` open while commands execute and rows stream back through `SqlDataReader`. You open once per unit of work, run commands sequentially, and dispose the reader before issuing the next command on the same connection — unless MARS is enabled. This model is ideal for HTTP APIs that map rows to DTOs while `Read()` advances, keeping memory flat regardless of result set size.

---

### Q7. What is the disconnected model, and which ADO.NET types does it primarily use?

**Concepts**
- `DataSet`/`DataTable`/`DataRow` as in-memory snapshot
- `SqlDataAdapter.Fill` pulls data, `.Update` pushes changes
- Row states: Added, Modified, Deleted
- Connection closed after Fill

**Answer**

The disconnected model fills `DataTable` or `DataSet` with `SqlDataAdapter.Fill`, then closes the connection so client code can read or edit rows offline. Row state (`Added`, `Modified`, `Deleted`) tracks changes, and `adapter.Update` pushes them back in batches later. This pattern was common in WinForms and WebForms apps; modern stateless REST APIs typically use reader-based streaming instead.

---

### Q8. What are the trade-offs of hand-written SQL versus a higher-level ORM?

**Concepts**
- SQL: precise control, predictable plans, no mapping magic
- ORM: productivity, migrations, change tracking overhead
- Schema evolution: manual scripts vs automated migrations
- Hybrid approach in production systems

**Answer**

Hand-written SQL offers precise control, predictable execution plans, and access to every database feature, but the team owns mapping, schema versioning, and relationship queries. An ORM like EF Core accelerates CRUD, automates migrations, and reduces boilerplate, yet can produce surprising SQL and adds tracking overhead. Most production systems combine both: EF Core for domain workflows and ADO.NET or Dapper for hot paths and reports.

---

## Chapter 02. SqlConnection & Connection Strings

### Q1. What is connection pooling in ADO.NET?

**Concepts**
- Provider-managed cache of physical connections
- Keyed by identical connection string
- `Min Pool Size` / `Max Pool Size` settings
- Dispose returns slot to pool (no teardown)

**Answer**

Connection pooling is a provider-managed cache of open physical connections keyed by an identical connection string. When you call `Open`, the provider returns an idle pooled connection rather than paying the cost of a new TCP session and login. `Dispose` on `SqlConnection` returns the physical connection to the pool — even minor connection string differences create separate pools.

---

### Q2. Does creating `new SqlConnection()` every time open a new physical database connection?

**Concepts**
- `new SqlConnection()` allocates only a managed wrapper
- Physical connection established on `Open`/`OpenAsync`
- Pooling reuses existing idle physical connections
- New physical connection only when pool has no idle slot

**Answer**

Creating `new SqlConnection()` allocates only a managed wrapper — no network connection opens until `Open` or `OpenAsync` is called. With default pooling, `Open` typically reuses an existing idle pooled connection rather than establishing a new one. Dispose promptly so the pooled slot is returned for other callers; without pooling every `Open` would pay full login and handshake cost.

---

### Q3. Why should you use `using` or `await using` with connections?

**Concepts**
- Guaranteed Dispose on exception paths
- Returns connection slot to pool on Dispose
- `await using` for async disposal without blocking
- Nested using for connection, command, and reader

**Answer**

`using` and `await using` guarantee `Dispose`/`DisposeAsync` runs even when exceptions occur, returning the pooled connection and releasing command and reader resources. Without disposal, connections leak until the GC finalizes them — often too late under concurrent load. `await using` pairs with async ADO.NET methods so disposal does not block a thread pool thread.

---

### Q4. How do you store connection strings securely in ASP.NET Core?

**Concepts**
- `IConfiguration.GetConnectionString` as the read point
- Environment variables, Azure Key Vault, user secrets
- Managed identity / integrated auth to avoid passwords
- Least-privilege database accounts

**Answer**

Keep connection strings out of source control and supply them through environment variables, Azure Key Vault, or user secrets — all consumed via `IConfiguration.GetConnectionString`. Use managed identity or integrated authentication to avoid embedding passwords entirely when the environment supports it. Restrict the database login to least privilege so a leaked credential limits the blast radius.

---

### Q5. What is the difference between `Microsoft.Data.SqlClient` and `System.Data.SqlClient`?

**Concepts**
- `Microsoft.Data.SqlClient` — active, cross-platform, modern .NET
- `System.Data.SqlClient` — legacy .NET Framework, maintenance mode
- Type identity conflicts when mixing both packages
- New code targets `Microsoft.Data.SqlClient` exclusively

**Answer**

`Microsoft.Data.SqlClient` is the actively maintained, cross-platform SQL Server provider for modern .NET, receiving ongoing fixes for Azure SQL, Always Encrypted, and managed identity. `System.Data.SqlClient` ships with .NET Framework and is in maintenance mode only. Mixing both packages in one solution causes type identity conflicts — `SqlConnection` from one assembly is not assignable to the other — so new code should reference only `Microsoft.Data.SqlClient`.

---

### Q6. What is a pool exhaustion error, and what typically causes it?

**Concepts**
- All pool slots checked out past timeout window
- Undisposed connections/readers as primary cause
- "Timeout expired obtaining connection from pool" message
- Raising Max Pool Size masks; fixing disposal cures

**Answer**

Pool exhaustion occurs when every pooled connection is checked out and a new `Open` call times out — SQL Server clients receive "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool." The usual cause is failing to dispose connections or readers, leaving slots occupied until the pool hits `Max Pool Size`. Raising `Max Pool Size` masks the symptom; fixing disposal and shortening connection lifetime addresses the root cause.

---

### Q7. What symptoms indicate a misconfigured or exhausted connection pool?

**Concepts**
- Intermittent timeouts under load, clear on restart
- Connection count pegged at Max Pool Size
- "Pool" / "timeout obtaining connection" error messages
- Errors absent under light load, visible under peak traffic

**Answer**

Pool stress shows as intermittent connection timeouts under load that clear after an app restart, with database-side sleeping session counts matching your pool ceiling. The errors appear only at peak traffic because idle connections mask leaks during light testing. Monitoring connection count pegged at `Max Pool Size` for sustained periods confirms exhaustion rather than a transient spike.

---

### Q8. How do unclosed connections affect pool availability?

**Concepts**
- Undisposed slot stays checked out until finalization
- GC finalization too slow for production throughput
- Leaked reader also blocks that connection slot
- Steady leaks exhaust Max Pool Size

**Answer**

An undisposed `SqlConnection` keeps its pooled slot checked out until finalization or server-side session timeout — neither is fast enough for a busy API. Each leaked connection reduces available slots, and under a steady leak rate the app exhausts `Max Pool Size`, causing all database operations to queue or timeout. A leaked reader on an open connection also prevents additional commands on that connection until disposed.

---

## Chapter 03. SqlCommand & Parameters

### Q1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?

**Concepts**
- `ExecuteReader` — returns `DbDataReader` for rowsets
- `ExecuteNonQuery` — DML/DDL, returns rows affected
- `ExecuteScalar` — returns first column of first row
- All honor the same parameters, transaction, and timeout

**Answer**

`ExecuteReader` runs a SELECT and returns a forward-only `DbDataReader` for streaming one or more result sets. `ExecuteNonQuery` runs INSERT, UPDATE, DELETE, or DDL and returns the number of rows affected — not a rowset. `ExecuteScalar` returns the first column of the first row, making it ideal for aggregates like `COUNT(*)` or identity lookups such as `SCOPE_IDENTITY()`.

---

### Q2. What is a parameterized query, and why is it preferred over string concatenation?

**Concepts**
- SQL placeholders (`@Name`, `@Id`) with `SqlParameter` values
- User input treated as data, not executable SQL
- Consistent statement text enables plan caching
- Eliminates injection, escaping, and culture bugs

**Answer**

A parameterized query sends SQL with typed placeholders and supplies values separately through `SqlParameter` objects, so the database engine treats user input as data rather than executable text. The SQL shape stays constant across executions, enabling query plan caching and preventing SQL injection. Identifiers like table and column names still cannot be parameterized — those require strict server-side allowlists.

---

### Q3. What is SQL injection, and how do parameters prevent it?

**Concepts**
- Untrusted input concatenated into SQL as executable text
- Parameters send values out-of-band from command text
- Applies to ADO.NET, Dapper, and EF Core raw SQL
- Parameters protect values, not dynamic identifiers

**Answer**

SQL injection is an attack where untrusted input is concatenated into SQL and interpreted as commands — for example, `'; DROP TABLE Users;--` closing the original query and appending a destructive statement. Parameters send values separately from command text so the engine treats them as data literals and never parses them as SQL. Parameters protect values only; dynamic identifiers like table names still require strict server-side allowlists.

---

### Q4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?

**Concepts**
- `AddWithValue` infers type/size from CLR value at runtime
- Explicit `SqlParameter` specifies `SqlDbType`, `Size`, `Precision`
- Type mismatch prevents index seeks and bloats plan cache
- Nullable values require `DBNull.Value` explicitly

**Answer**

`AddWithValue` infers the parameter type and size from the CLR value, which can mismatch the column — for example, sending `nvarchar(4000)` for a `varchar(50)` column, preventing index seeks. Explicit `SqlParameter` with `SqlDbType`, `Size`, and `Precision`/`Scale` ensures the provider sends exactly what the column expects, avoiding implicit conversions and plan cache bloat. Production code should use explicit parameters; `AddWithValue` is only acceptable for quick prototypes with well-known small types.

---

### Q5. What is the difference between `Text` and `StoredProcedure` command types?

**Concepts**
- `CommandType.Text` — sends raw SQL/T-SQL batch
- `CommandType.StoredProcedure` — invokes proc by name
- Output/return parameters cleaner with StoredProcedure mode
- Proc can encapsulate EXEC permissions on tables

**Answer**

`CommandType.Text` sends `CommandText` as raw SQL text executed by the server. `CommandType.StoredProcedure` sets `CommandText` to the procedure name and binds parameters to its signature, letting the provider generate the correct RPC call. Output parameters and return values work cleanly in `StoredProcedure` mode, and procedures can encapsulate permissions — granting `EXEC` without exposing underlying tables.

---

### Q6. When would you use `ExecuteScalar` instead of `ExecuteReader`?

**Concepts**
- Single-value result: COUNT, MAX, SCOPE_IDENTITY
- Avoids reader allocation and loop overhead
- Handle null/DBNull when zero rows possible
- Use ExecuteReader when multiple rows or columns needed

**Answer**

Use `ExecuteScalar` when the query is guaranteed to return exactly one value — a count, aggregate, flag, or newly generated identity — and iterating a reader would be unnecessary overhead. `SELECT COUNT(*) FROM Orders WHERE Status = @status` or `SELECT CAST(SCOPE_IDENTITY() AS int)` after an INSERT are classic cases. Cast the result carefully, since the return is `object?` and can be `null` or `DBNull` when no rows match.

---

## Chapter 04. SqlDataReader

### Q1. Why is `SqlDataReader` described as a forward-only, read-only cursor?

**Concepts**
- `Read()`/`ReadAsync()` advances one row at a time
- No backward movement or random row access
- Values consumed as read-only column data
- Efficient wire protocol: rows arrive sequentially

**Answer**

`SqlDataReader` advances sequentially — each `Read()` call fetches the next row and there is no `Previous` or random-access API. It exposes column values as read-only; to mutate data you must issue separate INSERT/UPDATE commands. This forward-only model matches how SQL Server streams result sets efficiently over the wire.

---

### Q2. What is the difference between connected streaming reads and loading everything into memory?

**Concepts**
- Streaming: one row at a time, constant memory, connection open
- In-memory load: all rows materialized before processing
- OOM risk with large Fill or ToList on web servers
- Streaming: faster time-to-first-row

**Answer**

Streaming through `SqlDataReader` processes one row at a time while the connection stays open, keeping memory constant regardless of result set size. Loading into `List<T>`, `DataTable`, or `DataSet` materializes every row as managed objects before your code runs. Streaming suits export pipelines, large reports, and APIs that map-and-write rows; in-memory load suits random access, multiple passes, or offline editing.

---

### Q3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?

**Concepts**
- One active batch per connection without MARS
- Open reader owns the batch, blocks second command
- MARS (`MultipleActiveResultSets=True`) relaxes the rule
- Nested `using` blocks enforce correct disposal order

**Answer**

A single SQL Server connection allows only one active batch at a time without MARS enabled. An open reader still owns that batch, so a second `ExecuteReader` or `ExecuteNonQuery` on the same connection throws "There is already an open DataReader associated with this Command." Dispose the first reader completely before issuing the next command; enabling MARS is an option but adds server overhead and should be used only when truly needed.

---

### Q4. When is `SqlDataReader` the best choice for large result sets?

**Concepts**
- Sequential row-by-row processing without full materialization
- `CommandBehavior.SequentialAccess` for large binary/text columns
- Combines well with server-side paging (OFFSET/FETCH)
- Avoids Gen2 GC pressure from large DataTable loads

**Answer**

Choose `SqlDataReader` when you must process many rows sequentially without storing the entire result set — exports, ETL transforms, log scanning, and streaming HTTP responses. Pair it with `CommandBehavior.SequentialAccess` for large binary or text columns to avoid unnecessary buffering, and combine with server-side `OFFSET`/`FETCH` paging when the consumer needs only a window of rows. Loading millions of rows into `DataTable` or EF Core entities risks Gen2 garbage collection pressure that `SqlDataReader` avoids.

---

### Q5. How do you handle NULL database values when reading from a data reader?

**Concepts**
- Database NULL maps to `DBNull.Value` in ADO.NET
- `reader.IsDBNull(ordinal)` check before typed getter
- `GetFieldValue<T?>` and extension methods reduce boilerplate
- COALESCE in SQL as alternative to C# null-handling

**Answer**

Database NULL maps to `DBNull.Value`; calling a typed getter like `GetInt32` on a NULL column throws an `InvalidCastException`. Use `reader.IsDBNull(ordinal)` to check first, then map to a nullable CLR type (`int?`, `string?`) or substitute a default. `GetFieldValue<T?>` reduces boilerplate, and pushing defaults into SQL with `COALESCE` is another option that shifts null semantics to the database layer.

---

### Q6. What performance advantage does a reader have over filling a `DataTable`?

**Concepts**
- No DataRow/DataColumn allocation per row
- Lower GC pressure on high-throughput APIs
- Flat working set vs linear DataTable memory growth
- Faster time-to-first-row vs buffered Fill

**Answer**

A reader avoids allocating `DataRow`, `DataColumn`, and internal indexing structures — you map directly from column getters into DTOs, generating far fewer managed objects. `DataTable.Fill` allocates a full in-memory relational snapshot with type metadata and row-state tracking, causing memory to grow linearly with row and column count. Readers also start returning data immediately, while `Fill` waits until the adapter buffers the result set.

---

### Q7. What happens if you do not dispose a data reader?

**Concepts**
- Connection stays in busy state blocking additional commands
- Pool slot stays checked out until finalization
- Contributes to pool exhaustion under concurrent load
- `using`/`await using` guarantees disposal on exceptions

**Answer**

An undisposed reader keeps the connection in a busy state, blocking additional commands on that connection and preventing the slot from returning to the pool. Under concurrent load this contributes to pool exhaustion — symptoms are sporadic "open DataReader" errors and pool timeouts even though connection objects appear created correctly. Always wrap readers in `using` or `await using` so disposal runs even when exceptions interrupt reading.

---

## Chapter 05. DataSet, DataTable & SqlDataAdapter

### Q1. What is the disconnected model that `DataSet`/`DataTable` support?

**Concepts**
- In-memory relational snapshot after connection closes
- `DataTable`: rows, columns, constraints, row state
- `DataSet`: multiple tables with `DataRelation` links
- `SqlDataAdapter.Fill` (pull) and `.Update` (push)

**Answer**

`DataSet` and `DataTable` hold relational data in memory after the database connection closes, letting code browse, filter, sort, and edit rows locally. `SqlDataAdapter.Fill` pulls rows into the in-memory structures; `adapter.Update` later pushes batched changes back through the adapter's insert, update, and delete commands. `DataSet` can hold multiple related tables with `DataRelation` objects modeling parent-child keys.

---

### Q2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?

**Concepts**
- Reader: connected, forward-only, minimal per-row allocation
- Fill: disconnected snapshot, all rows materialized upfront
- Fill supports random row access; reader does not
- `startRecord`/`maxRecords` for partial Fill loads

**Answer**

`DataReader` streams rows through a live connection with minimal allocation — you map columns into DTOs as each row arrives. `DataAdapter.Fill` executes the select command and loads the entire result into a `DataTable`, building column schema and row objects before your code runs. Readers suit large read-only sequential processing; `Fill` suits disconnected scenarios needing random row access or multi-pass processing.

---

### Q3. When would you still use `DataSet`/`DataTable` in modern .NET applications?

**Concepts**
- Ad hoc reporting and Excel-like grid editing
- Third-party controls or integration APIs requiring DataTable
- Untyped tabular payloads with runtime-variable schema
- Legacy interop and middle-tier merge scenarios

**Answer**

`DataSet` and `DataTable` remain useful for ad hoc reporting tools, legacy interop, admin utilities that let operators edit rows and push one batch update, and third-party controls that bind directly to `DataTable`. They also suit scenarios accepting untyped tabular payloads where schema varies at runtime. Greenfield ASP.NET Core REST APIs usually return `IEnumerable<T>` DTOs from readers or EF Core projections instead.

---

### Q4. What are the memory implications of filling a large table into a `DataSet`?

**Concepts**
- Each row is a `DataRow` with boxed values and version history
- Memory: row count × column count, often multiples of raw payload
- Gen2 GC pressure from large fills on server machines
- Streaming, paging, or projection as alternatives

**Answer**

Every row becomes a `DataRow` with boxed values, version history, and state flags; every column carries `DataColumn` metadata — memory grows proportional to row count × column count and is often several times larger than the raw SQL payload. Large text or binary columns duplicate fully in managed memory. Gen2 garbage collections from big fills cause long pauses under load, so prefer streaming, server-side paging, or DTO projection when result sets are large.

---

### Q5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?

**Concepts**
- REST APIs expect JSON-serializable POCOs, not DataRow
- Stateless APIs don't hold in-memory DataSet per request
- EF Core and micro-ORMs replaced "load table, bind grid"
- Disconnected editing model doesn't match PUT/PATCH REST

**Answer**

ASP.NET Core APIs are stateless and JSON-centric — clients expect typed DTOs that JSON serializers map cleanly, not `DataRow` dictionaries with `DBNull` values. WinForms and WebForms bound grids directly to `DataTable` in session-heavy patterns that do not fit modern stateless REST design. EF Core and micro-ORMs replaced the "load table, bind grid" workflow with entity or DTO pipelines, making `DataSet` rarely needed in new web APIs.

---

## Chapter 06. Transactions & Connection Pooling

### Q1. What are the ACID properties of a transaction?

**Concepts**
- Atomicity: all-or-nothing commit
- Consistency: constraints hold before and after
- Isolation: concurrent transactions see controlled views
- Durability: committed data survives crashes (log flush)

**Answer**

ACID describes the four guarantees a transactional database provides: Atomicity (all commands commit or all roll back), Consistency (constraints and rules remain valid), Isolation (concurrent transactions see controlled data views per the chosen isolation level), and Durability (committed changes persist even if power fails immediately afterward). Together these properties make multi-statement database operations reliable and predictable.

---

### Q2. How do you begin, commit, and rollback a transaction in ADO.NET?

**Concepts**
- `connection.BeginTransaction()` returns `SqlTransaction`
- Assign `cmd.Transaction = tx` to every participating command
- `tx.Commit()` on success, `tx.Rollback()` in catch
- All commands share the same connection and transaction instance

**Answer**

Open a connection, call `BeginTransaction()` to get a `SqlTransaction`, assign it to each command's `Transaction` property, execute all commands inside `try`, then call `Commit()` on success or `Rollback()` in `catch` before rethrowing. Dispose the transaction object when finished — `await using` handles this cleanly in async code. All participating commands must share both the same connection and the same transaction instance.

---

### Q3. Why must all commands in a transaction share the same connection?

**Concepts**
- Transaction bound to a single database session
- Separate pooled connections are independent sessions
- Atomicity across sessions requires distributed transaction coordinator
- EF Core enforces the same rule on its underlying connection

**Answer**

A transaction is bound to a single database session — SQL Server's transaction context does not span multiple physical connections from the pool. Each pooled connection is an independent session, so atomicity cannot cross sessions without an expensive distributed transaction coordinator. The standard pattern is one `using` connection, one transaction, and multiple sequential commands all assigned to that transaction.

---

### Q4. What is `TransactionScope`, and how does it differ from `SqlTransaction`?

**Concepts**
- `TransactionScope`: ambient transaction, can escalate to distributed
- `SqlTransaction`: lightweight, single-connection, explicit
- `Complete()` to commit; dispose without Complete = rollback
- Distributed transactions discouraged in modern cloud/microservices

**Answer**

`TransactionScope` is a `System.Transactions` API that marks a code block as transactional and can escalate to a distributed transaction when multiple connections or resource managers enlist. `SqlTransaction` is a lightweight, single-connection transaction tied explicitly to one ADO.NET connection. `SqlTransaction` is simpler and faster for single-database work in ASP.NET Core; distributed `TransactionScope` is discouraged in cloud apps — use sagas or outbox patterns instead.

---

### Q5. What is a pool exhaustion error, and what typically causes it?

**Concepts**
- All pool slots occupied past the timeout window
- Undisposed connections/readers and long-held transactions
- Error surfaces at `Open`, not at the original leak
- Fix: prompt disposal and short transaction lifetime

**Answer**

Pool exhaustion happens when no pooled connection is free within the timeout window — commonly because connections or readers were not disposed, or because long transactions hold slots during traffic spikes. The error surfaces at `Open` rather than at the original leak site, which makes diagnosis harder. Fixing disposal and keeping transactions short resolves most cases without raising `Max Pool Size`.

---

### Q6. What isolation levels exist, and why do they matter?

**Concepts**
- READ UNCOMMITTED, READ COMMITTED (default), REPEATABLE READ, SNAPSHOT, SERIALIZABLE
- Lower levels allow dirty reads / phantoms but reduce blocking
- SNAPSHOT uses row versioning, no shared read locks
- Set via `BeginTransaction(IsolationLevel)` in ADO.NET

**Answer**

Isolation levels control how much one transaction sees of another's uncommitted or in-flight changes, trading consistency for concurrency. SQL Server offers READ UNCOMMITTED, READ COMMITTED (default), REPEATABLE READ, SNAPSHOT, and SERIALIZABLE. Lower levels reduce blocking but allow phenomena like dirty reads; SERIALIZABLE prevents all anomalies at the cost of lock contention; SNAPSHOT provides consistent reads via row versioning without shared locks and is popular for read-heavy OLTP workloads.

---

### Q7. What is the correct pattern for rollback in a `try/catch` around ADO.NET transactions?

**Concepts**
- Begin transaction after connection opens, before first command
- `Commit()` only when all steps succeed
- `Rollback()` in `catch` before rethrowing
- `await using var tx` for async disposal

**Answer**

Begin the transaction after the connection is open, execute all commands inside `try`, and call `Commit()` only when every step succeeds. In `catch`, call `Rollback()` before rethrowing so callers know the operation failed — swallowing exceptions without rollback leaves the connection in an aborted state. Use `await using var tx` and explicit `RollbackAsync` in `catch` for the cleanest async pattern in ASP.NET Core.

---

## Chapter 07. Stored Procedures & Output Parameters

### Q1. What is a stored procedure, and why use one from ADO.NET?

**Concepts**
- Precompiled T-SQL batch stored and invoked by name
- Encapsulates complex SQL and centralizes tuning
- EXEC permission without exposing underlying tables
- Output parameters and multiple result sets as result contracts

**Answer**

A stored procedure is a precompiled T-SQL batch stored on the server and invoked by name from ADO.NET. It encapsulates complex SQL, centralizes performance tuning close to the data, and enables granting `EXEC` permission without exposing underlying tables. Output parameters and multiple result sets integrate naturally with ADO.NET's `StoredProcedure` command type.

---

### Q2. How do you execute a stored procedure with `SqlCommand`?

**Concepts**
- `CommandType.StoredProcedure` + procedure name in `CommandText`
- Parameters match proc signature with explicit `SqlDbType`
- `ParameterDirection.Input`/`Output`/`ReturnValue` for each param
- Async variants: `ExecuteReaderAsync`, `ExecuteNonQueryAsync`

**Answer**

Set `CommandType = CommandType.StoredProcedure`, assign the procedure name to `CommandText`, add `SqlParameter` objects matching the signature with explicit `SqlDbType`, then call the appropriate `Execute*` method. Input parameters use `ParameterDirection.Input`; output parameters and return values require explicit direction set before execution and their `.Value` read afterward. Parameter names must match the procedure definition including the `@` prefix.

---

### Q3. What is the difference between output parameters and return values (`ReturnValue`)?

**Concepts**
- Output parameters: `OUTPUT` keyword, named `@param` slots
- `ReturnValue`: separate integer channel, T-SQL `RETURN n`
- Multiple output params supported; only one return value per call
- Convention: outputs for data, return value for status codes

**Answer**

Output parameters are declared `OUTPUT` in the procedure signature and pass values back through named `@param` slots — read from `.Value` after execution. The procedure return value is a separate integer channel (`RETURN 0`) accessed via `ParameterDirection.ReturnValue`, conventionally used for status codes rather than business data. Multiple output parameters are supported; there is only one return value per procedure call.

---

### Q4. When are stored procedures preferred over inline SQL in ADO.NET?

**Concepts**
- DBA-owned tuned access paths and stable permission surface
- High-volume batches benefit from server-side execution
- Multi-client database contracts via procedure signatures
- Inline SQL suits simple CRUD and rapidly changing queries

**Answer**

Prefer stored procedures when DBAs own optimized access paths, when a narrow `EXEC`-only permission model is required, or when large batches benefit from server-side execution without shipping SQL text every call. Inline SQL is cleaner for simple CRUD, rapidly changing queries, and teams that version SQL entirely in application code for transparency and code review.

---

### Q5. What are the trade-offs of putting business logic in stored procedures versus C#?

**Concepts**
- Procs: close to data, multi-client enforcement, hard to unit test
- C#: testable, source-controlled, integrated with DI and domain model
- T-SQL branching harder to refactor than C# service classes
- Pragmatic split: set operations in SQL, orchestration in C#

**Answer**

Stored procedures run logic close to data, reduce round trips, and enforce rules regardless of which client connects, but they are harder to unit test, version alongside application code, and debug in typical .NET toolchains. C# business logic benefits from test frameworks, dependency injection, and domain-driven design, though naively implemented it can generate more network round trips. The pragmatic split is data-intensive set operations in SQL and orchestration, validation, and external integration in application code.

---

## Chapter 08. Async ADO.NET

### Q1. Why should database I/O be async in ASP.NET Core request handlers?

**Concepts**
- Database calls are network I/O-bound, not CPU-bound
- Async frees thread pool threads during the await
- Kestrel reuses threads; blocking reduces throughput
- Sync-over-async causes thread pool starvation under load

**Answer**

Database calls are network I/O-bound — a thread blocking on SQL Server response is wasted when it could serve other requests. Async ADO.NET (`OpenAsync`, `ExecuteReaderAsync`) frees the thread pool thread during the database wait, letting Kestrel reuse it for concurrent requests. Sync ADO.NET under load causes thread pool starvation and rising latency before CPU saturates — async does not speed up a single query but improves how many the server handles concurrently.

---

### Q2. What does `await using` provide when working with connections and readers?

**Concepts**
- Asynchronously disposes `IAsyncDisposable` without blocking
- Guaranteed cleanup on exception paths
- Nested `await using` enforces correct connection → reader release order
- Materialize results before leaving `await using` scope

**Answer**

`await using` asynchronously disposes `IAsyncDisposable` resources such as `SqlConnection`, `SqlCommand`, and `SqlDataReader`, ensuring cleanup completes without blocking a thread pool thread on network flush. Combined with `try`/`finally` semantics, disposal runs even when exceptions interrupt reading. Materialize results to `List<T>` inside the `await using` scope before returning — returning a deferred enumerable that holds an open reader causes failures when the caller iterates after disposal.

---

### Q3. What problems arise from calling `.Result` or `.Wait()` on async ADO.NET operations?

**Concepts**
- Sync-over-async: thread blocks idle while I/O waits
- Thread pool queue growth and cascading latency under load
- Deadlock when continuation needs the blocked thread
- Fix: make entire call stack `async Task` and await through

**Answer**

Calling `.Result` or `.Wait()` on async ADO.NET operations blocks a thread pool thread while I/O waits — that thread is wasted and unavailable for other requests. Under burst traffic this causes thread pool queue growth and cascading latency. Deadlocks can occur when a blocked thread awaits a continuation that needs the same thread. The fix is making controllers, repositories, and all intermediaries `async Task` and awaiting through to the ADO.NET calls.

---

### Q4. What is `CancellationToken` support in async ADO.NET methods?

**Concepts**
- `OpenAsync`, `ExecuteReaderAsync`, `ReadAsync` all accept a token
- Pass `HttpContext.RequestAborted` from ASP.NET Core endpoints
- Cancellation throws `OperationCanceledException`
- Tokens do not auto-rollback transactions

**Answer**

Async ADO.NET methods accept an optional `CancellationToken` that signals client disconnect or timeout, allowing the provider to cancel the pending network operation. Pass `HttpContext.RequestAborted` from ASP.NET Core endpoints through repository methods into `OpenAsync`, `ExecuteReaderAsync`, and `ReadAsync`. Cancellation throws `OperationCanceledException` — handle it separately from SQL errors, and remember that cancellation does not automatically roll back an in-flight transaction.

---

### Q5. What is the recommended async pattern for opening a connection, executing a command, and reading results?

**Concepts**
- Nested `await using` for connection, command, reader
- `OpenAsync` → `ExecuteReaderAsync` → `ReadAsync` chain
- Pass `CancellationToken` through each step
- Materialize to `List<T>` before connection closes

**Answer**

Use nested `await using` declarations: open the connection with `OpenAsync(ct)`, create and parameterize the command, execute with `ExecuteReaderAsync(ct)`, then loop with `while (await reader.ReadAsync(ct))` mapping columns to DTOs. Materialize results to `List<T>` before the method scope closes so the caller has data after the connection is disposed. Keep the entire chain async — no `.Result` at any layer.

---

### Q6. When is synchronous ADO.NET still acceptable?

**Concepts**
- Console tools, one-off migrations, local scripts
- Single-threaded batch jobs with no concurrent pressure
- Integration test setup/teardown for brevity
- Never sync from ASP.NET Core request threads

**Answer**

Sync ADO.NET is fine for console tools, one-off migrations, local scripts, and integration test setup where no concurrent request pressure exists. Single-threaded batch jobs that process sequentially also see minimal benefit from async overhead. Never call sync ADO.NET from ASP.NET Core request threads when async alternatives exist — that is where the scalability cost appears; use `Task.Run` only as a last resort for legacy APIs that cannot be made async.

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

#### Q1. (R) A team ports a WinForms order editor into an ASP.NET Core API. They reuse the disconnected pattern from `WorkflowSteps.DisconnectedSnapshotSteps()`:

```csharp
public async Task<IActionResult> GetOrders()
{
    var conn = new SqlConnection(_connectionString);
    conn.Open();

    var adapter = new SqlDataAdapter("SELECT OrderId, Customer, Amount, Status FROM dbo.Orders", conn);
    var table = new DataTable("Orders");
    adapter.Fill(table); // entire table into memory

    conn.Close();

    foreach (DataRow row in table.Rows)
    {
        row["Amount"] = (decimal)row["Amount"]! * 1.1m; // "display markup" in API layer
    }

    return Ok(table); // serialize DataTable to JSON
}
```

Traffic is moderate; the table has ~2M rows. What fails at runtime or in operations, and what connected/disconnected pattern should replace this for a read-only list API?

---

**Answer:**

_Answer not found._

---

#### Q2. (R) A "database abstraction" library exposes this interface for multi-database support:

```csharp
public interface IOrderRepository
{
    SqlConnection OpenConnection();
    List<OrderDto> GetRecent(SqlConnection connection);
}

public sealed class SqlServerOrderRepository : IOrderRepository
{
    public SqlConnection OpenConnection()
        => new SqlConnection("Server=prod-sql;Database=Sales;User Id=app;Password=***;");

    public List<OrderDto> GetRecent(SqlConnection connection)
    {
        using var cmd = new System.Data.SqlClient.SqlCommand(
            "SELECT TOP 100 OrderId, Amount FROM dbo.Orders ORDER BY OrderId DESC",
            connection);
        // ... ExecuteReader, map to DTOs ...
    }
}
```

The same solution also references `Microsoft.Data.SqlClient` in the web project. What breaks or drifts over time, and how would you fix the abstraction without losing SQL Server–specific features where needed?

---

**Answer:**

_Answer not found._

---

#### Q3. (D) You inherit three services:

| Service | Workload |
|---|---|
| **A** | Nightly `SqlBulkCopy` of 5M CSV rows into a staging table, then MERGE |
| **B** | CRUD REST API over 12 related entities, schema evolves weekly |
| **C** | Payment microservice: 3 hand-written SQL statements, strict latency SLA |

Each team asks for "one data stack." Using the chapter's technology guide (`TechnologyChoiceGuide`), assign ADO.NET, Dapper, or EF Core per service and justify one sentence each. What mistake do teams make when they pick EF Core for all three?

---

**Answer:**

_Answer not found._

---

#### Q4. (R) A repository method loads order header then line items on one open connection:

```csharp
public OrderDetailDto GetOrderDetail(int orderId)
{
    var conn = new SqlConnection(_connectionString);
    conn.Open();

    var headerCmd = new SqlCommand("SELECT ... FROM dbo.Orders WHERE OrderId = @id", conn);
    headerCmd.Parameters.AddWithValue("@id", orderId);
    var reader = headerCmd.ExecuteReader();
    var header = MapHeader(reader);
    reader.Close(); // developer assumes this is enough

    var linesCmd = new SqlCommand("SELECT ... FROM dbo.OrderLines WHERE OrderId = @id", conn);
    linesCmd.Parameters.AddWithValue("@id", orderId);
    var linesReader = linesCmd.ExecuteReader(); // SqlException: already an open DataReader

    return new OrderDetailDto(header, MapLines(linesReader));
}
```

MARS is disabled (default). Diagnose the failure, identify any resource leaks if the first reader path throws mid-map, and show the corrected disposal pattern aligned with `WorkflowSteps.ConnectedReadSteps()`.

---

**Answer:**

_Answer not found._

---

#### Q5. (R) An internal admin tool adds a "custom report SQL" feature. Architecture:

```csharp
public class ReportController : ControllerBase
{
    [HttpPost("run")]
    public IActionResult Run([FromBody] string sql)
    {
        using var conn = new SqlConnection(_config.GetConnectionString("Reporting"));
        conn.Open();
        using var cmd = new SqlCommand(sql, conn); // admin-supplied text
        using var reader = cmd.ExecuteReader();
        return Ok(ReadAllToJson(reader));
    }
}
```

Authentication is locked to the `ReportAdmin` role. A pentest still flags critical SQL injection. Explain why role gating is not sufficient at the architecture level, and describe two layered controls you would require before this reaches production.

---

**Answer:**

_Answer not found._

---

#### Q6. (P) A legacy monolith stores the connection string in source:

```csharp
public static class Db
{
    public const string ConnectionString =
        "Server=10.0.0.12;Database=Sales;User Id=sa;Password=Pr0d_Sales_2021!;TrustServerCertificate=true;";
}
```

You are splitting out a new .NET 8 worker service that still copies this constant. What operational and security problems does this create, and what configuration pattern replaces it for local dev, CI, and Azure deployment?

---

**Answer:**

_Answer not found._

---

#### Q7. (R) An order-placement service must debit inventory and insert the order atomically. Two developers submit patches:

**Patch A — ADO.NET on one connection:**

```csharp
using var conn = new SqlConnection(cs);
conn.Open();
using var tx = conn.BeginTransaction();
try
{
    var debit = new SqlCommand("UPDATE dbo.Inventory SET Qty = Qty - @q WHERE ProductId = @id", conn, tx);
    // ... execute ...
    var insert = new SqlCommand("INSERT INTO dbo.Orders (...) VALUES (...)", conn, tx);
    // ... execute ...
    tx.Commit();
}
catch { tx.Rollback(); throw; }
```

**Patch B — same logic but transaction started after first command:**

```csharp
using var conn = new SqlConnection(cs);
conn.Open();
var debit = new SqlCommand("UPDATE dbo.Inventory ...", conn);
debit.ExecuteNonQuery(); // succeeds
using var tx = conn.BeginTransaction();
var insert = new SqlCommand("INSERT INTO dbo.Orders ...", conn, tx);
insert.ExecuteNonQuery();
tx.Commit();
```

Under concurrent orders for the last unit in stock, which patch leaves inconsistent data, and where should the transaction boundary sit relative to the commands (per `TransactionConcept` / ch.06)?

---

**Answer:**

_Answer not found._

---

#### Q8. (R) A new microservice exposes gRPC methods that return `DataSet` built via `SqlDataAdapter.Fill`:

```csharp
public DataSet GetCustomerSnapshot(int customerId)
{
    var ds = new DataSet("CustomerSnapshot");
    using var conn = new SqlConnection(_connectionString);
    using var adapter = new SqlDataAdapter("SELECT * FROM dbo.Customers WHERE Id = @id", conn);
    adapter.SelectCommand!.Parameters.AddWithValue("@id", customerId);
    adapter.Fill(ds, "Customer");
    // second adapter fills Orders, OrderLines into same DataSet ...
    return ds;
}
```

Callers cache the `DataSet` in a static dictionary keyed by customer id. What problems appear at the API boundary, in memory, and under schema change compared to returning typed DTOs or a connected `DbDataReader` pipeline?

---

### 02. SqlConnection & Connection Strings

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/02. SqlConnection & Connection Strings`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

_Answer not found._

---

#### Q1. (R) A teammate ships this ASP.NET Core API to Azure. The PR passes CI and works on their laptop. Review the connection setup:

```csharp
// appsettings.json (committed to git)
{
  "ConnectionStrings": {
    "Default": "Server=tcp:contoso-sql.database.windows.net,1433;Database=OrdersDb;User ID=app_api;Password=Pr0d!Api#2024;Encrypt=true;TrustServerCertificate=true"
  }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OrderRepository>();
var app = builder.Build();

app.MapGet("/health/db", async (OrderRepository repo) =>
{
    await repo.PingAsync();
    return Results.Ok("db ok");
});
```

```csharp
public class OrderRepository
{
    private readonly string _cs;
    public OrderRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("Default")!;

    public async Task PingAsync()
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        _ = await cmd.ExecuteScalarAsync();
    }
}
```

What breaks in production (security, reliability, pooling), and what do you change first?

---

**Answer:**

```csharp
// appsettings.json (committed to git)
{
  "ConnectionStrings": {
    "Default": "Server=tcp:contoso-sql.database.windows.net,1433;Database=OrdersDb;User ID=app_api;Password=Pr0d!Api#2024;Encrypt=true;TrustServerCertificate=true"
  }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OrderRepository>();
var app = builder.Build();

app.MapGet("/health/db", async (OrderRepository repo) =>
{
    await repo.PingAsync();
    return Results.Ok("db ok");
});
```

```csharp
public class OrderRepository
{
    private readonly string _cs;
    public OrderRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("Default")!;

    public async Task PingAsync()
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1";
        _ = await cmd.ExecuteScalarAsync();
    }
}
```

What breaks in production (security, reliability, pooling), and what you change first?

**Answer:** The deployment leaks credentials in git, disables meaningful TLS validation, and leaks pooled connections on every health check because `SqlConnection` is never disposed. Fix secret storage and disposal before tuning anything else — otherwise you ship a credential incident and exhaust the pool under steady traffic.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | SQL password committed in `appsettings.json` | Credential in repo history; scanners and insiders get production access |
| Security | `TrustServerCertificate=true` against Azure SQL | Encrypted channel but no server identity validation — MITM risk if network path is compromised |
| Runtime / pooling | `SqlConnection` not disposed (`using` / `await using` missing) | Physical connection not returned to pool; repeated `/health/db` calls exhaust `Max Pool Size` |
| Design | Health endpoint opens real DB every probe | Amplifies leak; prefer dedicated health check package or scoped ping with guaranteed dispose |
| Config | Plaintext secret in config file instead of Key Vault / managed identity | Rotation requires redeploy; audit trail weak |

**Fix (priority order):**

1. **Rotate** the exposed password immediately and remove it from source — store in **Azure Key Vault** (`AddAzureKeyVault`) or use **managed identity** + Azure AD auth (`Authentication=Active Directory Managed Identity`) so no password exists in config.
2. Set **`TrustServerCertificate=false`** (or omit — default validates cert) and keep **`Encrypt=true`** for Azure SQL; use proper CA-trusted endpoints only.
3. Wrap connection usage: `await using var conn = new SqlConnection(_cs);` in `PingAsync` (same pattern as `DisposalPatternDemo.AwaitUsingPatternAsync` in this chapter).
4. Move connection string to **User Secrets** locally and **Key Vault / App Service settings** in cloud — never commit secrets (see `ConnectionSecurityPreview.cs`).
5. Optional: use **`AddDbContext`** or a factory if the stack moves to EF Core; for raw ADO.NET, a small `IDbConnectionFactory` that returns opened connections inside `using` scopes keeps pooling correct.

**Production takeaway:** Configuration holds **references** to secrets, not secrets themselves; `Dispose()` returns connections to the pool — skipping it is a production outage under load. See chapter `Program.cs` mistakes table (password in git, `TrustServerCertificate` in prod, forgotten `using`).

---

---

#### Q2. (R) Under load, the API starts returning `SqlException: Timeout expired. The timeout period elapsed…` and Azure SQL shows sessions near `Max Pool Size`. Review this service used on every request:

```csharp
public class ReportService
{
    private readonly string _cs;
    public ReportService(IConfiguration config) =>
        _cs = config.GetConnectionString("Default")!;

    public async Task<IReadOnlyList<ReportRow>> GetMonthlyReportAsync(int year, int month)
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var rows = new List<ReportRow>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT … FROM dbo.Orders WHERE …";
            cmd.CommandTimeout = 120;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                rows.Add(Map(reader));
        }

        // 30–90s CPU work: PDF render + email attachment build — connection stays Open
        var pdf = await _pdfGenerator.BuildAsync(rows);
        await _emailSender.SendWithAttachmentAsync(pdf);

        return rows;
    }
}
```

Diagnose why pool exhaustion happens here and how you would refactor without disabling pooling.

---

**Answer:**

```csharp
public class ReportService
{
    private readonly string _cs;
    public ReportService(IConfiguration config) =>
        _cs = config.GetConnectionString("Default")!;

    public async Task<IReadOnlyList<ReportRow>> GetMonthlyReportAsync(int year, int month)
    {
        var conn = new SqlConnection(_cs);
        await conn.OpenAsync();

        var rows = new List<ReportRow>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT … FROM dbo.Orders WHERE …";
            cmd.CommandTimeout = 120;
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                rows.Add(Map(reader));
        }

        // 30–90s CPU work: PDF render + email attachment build — connection stays Open
        var pdf = await _pdfGenerator.BuildAsync(rows);
        await _emailSender.SendWithAttachmentAsync(pdf);

        return rows;
    }
}
```

Diagnose why pool exhaustion happens here and how you would refactor without disabling pooling.

**Answer:** The service holds an open `SqlConnection` through slow non-database work after the reader finishes, so each concurrent report occupies a pool slot for up to two minutes. Under traffic, new requests block on `Open()` until timeout — classic **pool exhaustion** from **opening too early and closing too late**, not from pooling being broken.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling / lifetime | Connection opened at start, not disposed until method exit | Pool slot held during PDF + email (30–90s) while SQL is idle |
| Pooling / lifetime | No `using` / `await using` on `SqlConnection` | If an exception escapes after `Open`, leak is guaranteed |
| Design | DB work and report generation not separated | Scales poorly; N concurrent reports ≈ N long-lived sessions |
| Reliability | `ConnectTimeout` / pool wait surfaces as generic timeout | Hard to distinguish from slow query without metrics |

**Fix (priority order):**

1. **Shrink the open window** — load data inside `await using (var conn = new SqlConnection(_cs)) { … }`, materialize `rows`, then **dispose** before PDF/email work (chapter guidance: open late, close early; `PoolingPreview.PrintPoolingNote`).
2. Wrap the entire method in `await using var conn` if you must keep one block, but **do not** await non-SQL work while the connection is open.
3. Keep **pooling enabled** (`Pooling=true` is default) — do not set `Pooling=false` as a "fix"; that increases latency and server load.
4. If reports are heavy, **queue** generation (background worker) so HTTP requests do not pin connections.
5. Monitor **`NumberOfFreeConnections`** / Azure SQL session count; tune **`Max Pool Size`** only after fixing hold time, not before.

**Production takeaway:** Pooling assumes connections are borrowed for milliseconds to low seconds per operation — holding `SqlConnection` across I/O-bound or CPU-bound orchestration is the most common ADO.NET production trap after secret mishandling.

---

---

#### Q3. (M) A data-access helper tries to load order header + line items with two active readers on one connection. Production throws *"There is already an open DataReader associated with this Connection…"*

```csharp
public async Task<OrderDetailDto> GetOrderDetailAsync(int orderId, string connectionString)
{
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    OrderHeaderDto? header = null;
    await using (var headerCmd = conn.CreateCommand())
    {
        headerCmd.CommandText = "SELECT OrderId, CustomerId, OrderDate FROM dbo.Orders WHERE OrderId = @id";
        headerCmd.Parameters.Add(new SqlParameter("@id", orderId));
        await using var headerReader = await headerCmd.ExecuteReaderAsync();
        if (await headerReader.ReadAsync())
            header = MapHeader(headerReader);
    }

    var lines = new List<OrderLineDto>();
    await using (var lineCmd = conn.CreateCommand())
    {
        lineCmd.CommandText = "SELECT Sku, Qty FROM dbo.OrderLines WHERE OrderId = @id";
        lineCmd.Parameters.Add(new SqlParameter("@id", orderId));
        await using var lineReader = await lineCmd.ExecuteReaderAsync(); // fails intermittently
        while (await lineReader.ReadAsync())
            lines.Add(MapLine(lineReader));
    }

    return new OrderDetailDto(header!, lines);
}
```

The bug is not always reproduced in dev. Explain when this fails, whether `MultipleActiveResultSets=true` is the right fix, and what you would prefer in production.

---

**Answer:**

```csharp
public async Task<OrderDetailDto> GetOrderDetailAsync(int orderId, string connectionString)
{
    await using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    OrderHeaderDto? header = null;
    await using (var headerCmd = conn.CreateCommand())
    {
        headerCmd.CommandText = "SELECT OrderId, CustomerId, OrderDate FROM dbo.Orders WHERE OrderId = @id";
        headerCmd.Parameters.Add(new SqlParameter("@id", orderId));
        await using var headerReader = await headerCmd.ExecuteReaderAsync();
        if (await headerReader.ReadAsync())
            header = MapHeader(headerReader);
    }

    var lines = new List<OrderLineDto>();
    await using (var lineCmd = conn.CreateCommand())
    {
        lineCmd.CommandText = "SELECT Sku, Qty FROM dbo.OrderLines WHERE OrderId = @id";
        lineCmd.Parameters.Add(new SqlParameter("@id", orderId));
        await using var lineReader = await lineCmd.ExecuteReaderAsync(); // fails intermittently
        while (await lineReader.ReadAsync())
            lines.Add(MapLine(lineReader));
    }

    return new OrderDetailDto(header!, lines);
}
```

The bug is not always reproduced in dev. Explain when this fails, whether `MultipleActiveResultSets=true` is the right fix, and what you would prefer in production.

**Answer:** SqlClient allows **one active reader per connection** unless **MARS** (`MultipleActiveResultSets=true`) is enabled. This snippet disposes each reader before opening the next, so it is usually fine — failures happen when a reader is still open (nested call, missed `await using`, or refactored code that overlaps readers). MARS can unblock overlapping readers but adds complexity; prefer sequential queries, a join, or two connections.

- **When it fails:** Any code path leaves `headerReader` open while starting `lineReader` — e.g. early `return` inside the first block without disposal, exception before dispose, or a helper that starts the second query while the first reader is still streaming. Dev "works" with small datasets and strict disposal; prod fails under different call paths or provider timing.
- **MARS fix:** Setting `MultipleActiveResultSets=true` (see `DemoConnectionBuilder.BuildLocalDbString` default `false`) tells SQL Server to multiplex batches on one connection so multiple readers can be active. It fixes legitimate overlap but masks design smells and can increase locking overhead.
- **Preferred production patterns:** (1) **One query** with join or JSON subquery; (2) **Two sequential commands** with first reader fully disposed (this sample's intent); (3) **Two connections** from the pool if you truly need parallel reads; (4) enable MARS **only** when overlapping readers are required and measured.
- **Verify:** Ensure `CommandBehavior.CloseConnection` is not mixed incorrectly; always `await using` readers; never pass an open connection into nested repositories that also open readers.

**Production takeaway:** MARS is a scalpel, not a default — chapter key reference lists `MultipleActiveResultSets` as opt-in for a reason. Default to one reader per connection lifetime slice.

---

---

#### Q4. (P) Your team copies the LocalDB connection string from `ConnectionStringSamples.LocalDbIntegratedSecurity` into Azure App Service configuration:

```
Server=tcp:contoso.database.windows.net,1433;Database=OrdersDb;User ID=app_user;Password=***;Encrypt=true;TrustServerCertificate=true;Integrated Security=true
```

App Service logs show `Login failed for user ''` or SSL/certificate errors depending on environment. Walk through what each problematic key does in SqlClient 5.x on Azure SQL, what the production string should look like instead, and why `Encrypt=true` alone is not enough if `TrustServerCertificate=true` remains.

---

**Answer:**

```
Server=tcp:contoso.database.windows.net,1433;Database=OrdersDb;User ID=app_user;Password=***;Encrypt=true;TrustServerCertificate=true;Integrated Security=true
```

App Service logs show `Login failed for user ''` or SSL/certificate errors depending on environment. Walk through what each problematic key does in SqlClient 5.x on Azure SQL, what the production string should look like instead, and why `Encrypt=true` alone is not enough if `TrustServerCertificate=true` remains.

**Answer:** The string mixes **Windows auth**, **SQL auth**, and **dev-only TLS bypass** — SqlClient 5.x defaults `Encrypt=true`, but `TrustServerCertificate=true` skips chain validation, and `Integrated Security=true` ignores `User ID`/`Password` on Linux/non-domain hosts. Production Azure SQL should use encrypted, validated TLS plus a single auth mode (prefer Azure AD managed identity or SQL auth with secrets in Key Vault).

- **`Integrated Security=true`:** Uses Windows/SSPI (Kerberos). Works on domain-joined Windows (LocalDB sample in `ConnectionStringSamples.LocalDbIntegratedSecurity`); on App Service/Linux it does not use `User ID`/`Password` — login appears as empty or wrong principal → **18456 login failed**.
- **`User ID` / `Password`:** SQL authentication — valid on Azure SQL **only when** `Integrated Security` is false. Mixed keys create confusing provider behavior.
- **`Encrypt=true`:** Required for Azure SQL; encrypts traffic (default in Microsoft.Data.SqlClient 5.x — see `DemoConnectionBuilder` explicit `Encrypt = true`).
- **`TrustServerCertificate=true`:** Skips validating the server certificate chain — fine for LocalDB/dev (`PrintSecurityGuidance` warns dev only); in prod it weakens TLS to encryption-without-trust and can hide misconfigured hostnames or rogue endpoints.
- **Production shape (SQL auth example):** `Server=tcp:contoso.database.windows.net,1433;Database=OrdersDb;User ID=app_user;Password={from Key Vault};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30`
- **Better (Azure):** `Authentication=Active Directory Managed Identity;User Id=<client-id>;` with managed identity enabled on App Service — no password in configuration.
- **Why Encrypt alone is insufficient with TrustServerCertificate:** You still encrypt bytes on the wire but **do not authenticate the server** — a MITM with any cert is accepted, defeating the purpose of TLS in zero-trust networks.

**Production takeaway:** Treat `TrustServerCertificate=true` like `ConnectionStringSamples` and `ConnectionSecurityPreview` say — local dev only. Pick **one** authentication model per environment and align with hosting OS.

---

---

#### Q5. (R) A "performance optimization" opens SQL at application startup and registers the connection for the app lifetime:

```csharp
builder.Services.AddSingleton<SqlConnection>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")!;
    var conn = new SqlConnection(cs);
    conn.Open(); // "warm the pool once"
    return conn;
});

builder.Services.AddScoped<IOrderQueries, OrderQueries>();

public class OrderQueries
{
    private readonly SqlConnection _conn;
    public OrderQueries(SqlConnection conn) => _conn = conn;

    public async Task<int> CountOpenOrdersAsync()
    {
        await using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dbo.Orders WHERE Status = 'Open'";
        return (int)(await cmd.ExecuteScalarAsync())!;
    }
}
```

What fails at runtime (threading, pooling, scope), and what pattern replaces this while still benefiting from connection pooling?

---

**Answer:**

```csharp
builder.Services.AddSingleton<SqlConnection>(sp =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetConnectionString("Default")!;
    var conn = new SqlConnection(cs);
    conn.Open(); // "warm the pool once"
    return conn;
});

builder.Services.AddScoped<IOrderQueries, OrderQueries>();

public class OrderQueries
{
    private readonly SqlConnection _conn;
    public OrderQueries(SqlConnection conn) => _conn = conn;

    public async Task<int> CountOpenOrdersAsync()
    {
        await using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM dbo.Orders WHERE Status = 'Open'";
        return (int)(await cmd.ExecuteScalarAsync())!;
    }
}
```

What fails at runtime (threading, pooling, scope), and what pattern replaces this while still benefiting from connection pooling?

**Answer:** A singleton `SqlConnection` shared across scoped requests is not thread-safe, defeats pooling semantics, and breaks when the server closes idle sessions. Open per operation inside `await using` — pooling already amortizes TCP+login cost without holding one connection for the process lifetime.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Threading | One `SqlConnection` used by concurrent HTTP requests | Undefined behavior, intermittent reader/command errors |
| DI lifetime | Singleton connection injected into scoped services | Captive dependency; violates ASP.NET Core scope rules |
| Pooling | Long-lived open connection bypasses normal borrow/return | Pool starved; reconnect logic never runs per request |
| Reliability | Firewalls/Azure idle timeout drop the socket | Next command fails until manual reconnect — singleton has no refresh |
| Design | Mistaken "warm pool" — pooling warms automatically on first `Open()` | No measurable gain; high operational risk |

**Fix (priority order):**

1. Remove `AddSingleton<SqlConnection>()` entirely.
2. Inject **`IConfiguration`** or **`IDbConnectionFactory`**; in each method: `await using var conn = new SqlConnection(_cs); await conn.OpenAsync();` — matches `DisposalPatternDemo` patterns in this chapter.
3. If you need startup validation, run a **one-shot** `using` open in `IHostedService` or health check at startup, then dispose — do not keep the instance.
4. For EF Core stacks, use **`AddDbContext`** scoped lifetime; for ADO.NET, register a **scoped factory** that creates connections on demand.
5. Trust default **connection pooling** (`PoolingPreview`) — first request pays connect cost; subsequent requests reuse pooled sockets.

**Production takeaway:** `SqlConnection` is cheap to construct; the pool owns physical connections. **Open late, close early** per operation — never singleton.

---

---

#### Q6. (P) You deploy the same API to **AKS Linux containers** (no domain join). The connection string still uses `Integrated Security=true` copied from the on-prem IIS app. Locally on Windows it works; in the cluster every `Open()` fails with login or SSPI errors. How do you fix auth for Linux containers, and where should the secret live instead of `appsettings.json`?

---

**Answer:**

**Answer:** Linux containers have no Windows identity for SSPI — replace `Integrated Security=true` with **SQL authentication** (secret in Key Vault) or **Azure AD authentication** via **workload identity / managed identity**, and load the connection string from Key Vault or Kubernetes secrets mounted at runtime, not from committed `appsettings.json`.

- **Why Integrated Security fails:** `IntegratedSecurity=true` maps to Windows auth (see `DemoConnectionBuilder` and key reference). AKS pods are Linux, not domain-joined — no Kerberos ticket, so SqlClient cannot authenticate as the IIS app pool identity.
- **Fix option A — SQL auth:** `User ID=…;Password=…;Encrypt=True;TrustServerCertificate=False` with password rotated and stored in **Azure Key Vault**, referenced via **AKS CSI driver** or App Configuration; inject with `ConnectionStrings__Default` env var (double underscore — `ConnectionSecurityPreview`).
- **Fix option B — Azure AD (preferred on Azure SQL):** Enable **managed identity** on the workload (or service principal), grant `CREATE USER … FROM EXTERNAL PROVIDER`, use `Authentication=Active Directory Managed Identity` (or DefaultAzureCredential chain) — no password to leak.
- **Do not use** `appsettings.json` in the container image for production secrets — image layers and git are untrusted stores; User Secrets are dev-only.
- **Network:** Ensure Azure SQL **firewall rules** allow AKS **outbound IP** (or deploy with **Private Link / VNet service endpoints**) — firewall blocks surface as connection timeouts (`SqlException` network-related), not login 18456; allow the cluster egress before debugging auth.

**Production takeaway:** Match auth mode to **host OS and cloud identity** — Integrated Security is for Windows/domain scenarios; containers need token- or secret-based SQL/Azure AD auth from a secret store.

---

---

#### Q7. (D) Production uses Azure SQL with geo-replication. During a regional outage, ops gives you a **failover connection string** pointing at the secondary server. Your app currently has one `ConnectionStrings:Default` in Key Vault. Describe how connection failover works with SqlClient (including `Failover Partner` / `ApplicationIntent` / Active Directory managed identity if relevant), what you change in configuration vs code, and what application-level behavior you still need (retries, idempotency, read-only routing).

---

---

### 03. SqlCommand & Parameters

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/03. SqlCommand & Parameters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** Prefer **Azure SQL connection policies** (failover groups / `*.database.windows.net` endpoint) so SqlClient follows the current primary automatically; legacy `Failover Partner` applied to on-prem Always On. Configuration changes drive most of the behavior — code still needs transient retry and idempotent writes because failover drops in-flight sessions.

- **Azure SQL (modern):** Use the **failover group listener** hostname (`mygroup.database.windows.net`) in `Server=` — Azure redirects to the current primary after geo-failover. Often **no code change** beyond connection string; retire hard-coded regional server names.
- **Always On / legacy:** `Failover Partner=secondaryHost;` (or mirror partner keys in older docs) lets SqlClient redirect when primary is down — pair with **`ConnectRetryCount`** / **`ConnectRetryInterval`** (SqlClient connection string keys) for transient reconnects.
- **`ApplicationIntent=ReadOnly`:** Routes to readable secondary in AG/read scale-out — use separate connection string (`DefaultRead`) for reporting queries; writes stay on primary intent (default ReadWrite).
- **Managed identity:** Auth mode unchanged across failover — identity and SQL user must exist on **both** sides after replication; failover string swap alone fails if secondary lacks the same AAD user.
- **Configuration vs code:** Update **Key Vault secret** or App Configuration label — e.g. single listener URL instead of two regional secrets — avoid redeploying code for DR string swaps when possible.
- **Application still needs:** (1) **Retry** on `SqlException` numbers for connection broken / timeout (Polly or `EnableRetryOnFailure` in EF); (2) **Idempotent** writes — in-flight transactions are aborted at failover; (3) **Circuit breaker** to avoid stampede on recovering secondary; (4) **Read-your-writes** awareness — reads on secondary may lag; (5) drain or recycle **scoped** connections after failover event (pooled connections to dead primary throw until cleared — `SqlConnection.ClearAllPools()` only in controlled recovery scripts, not per request).

**Production takeaway:** Failover is a **connection routing + resilience** problem — SqlClient and Azure listeners handle server selection, but your app must tolerate broken connections and duplicate side effects. See chapter pooling preview (`Dispose` returns to pool; stale pool entries after DR are an ops play).

---

---

### 03. SqlCommand & Parameters

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/03. SqlCommand & Parameters`

---

---

#### Q1. (R) A junior developer ships a product search endpoint. Review the repository method:

```csharp
public IReadOnlyList<ProductDto> SearchByName(SqlConnection connection, string searchTerm)
{
    string sql = """
        SELECT ProductId, Name, UnitPrice, Stock
        FROM dbo.Products
        WHERE IsActive = 1 AND Name LIKE '%" + searchTerm + "%'
        ORDER BY Name;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    using SqlDataReader reader = command.ExecuteReader();
    // ... map rows to ProductDto ...
}
```

The method works in QA with `"Mouse"` and `"Keyboard"`. What breaks in production, how would an attacker exploit it, and what is the minimal safe fix?

---

**Answer:**

```csharp
public IReadOnlyList<ProductDto> SearchByName(SqlConnection connection, string searchTerm)
{
    string sql = """
        SELECT ProductId, Name, UnitPrice, Stock
        FROM dbo.Products
        WHERE IsActive = 1 AND Name LIKE '%" + searchTerm + "%'
        ORDER BY Name;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    using SqlDataReader reader = command.ExecuteReader();
    // ... map rows to ProductDto ...
}
```

The method works in QA with `"Mouse"` and `"Keyboard"`. What breaks in production, how would an attacker exploit it, and what is the minimal safe fix?

**Answer:** The method embeds user input directly into SQL text, which is a classic **SQL injection** vulnerability. Benign searches work in QA, but an attacker can alter query logic — returning every row, bypassing filters, or worse if the DB login is over-privileged.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | String concatenation of `searchTerm` into `CommandText` | Attacker input becomes executable SQL, not data |
| Correctness | Statement shape changes per request | Optimizer treats each string as a new ad-hoc batch; harder to audit and cache |
| Operational | No parameter binding | Logging/monitoring cannot safely redact or classify the value separately from SQL |

**Fix (priority order):**

1. **Never concatenate user input** — use a fixed SQL template with a placeholder, as in `ProductRepository.DemonstrateSqlInjectionPrevention`:
   ```csharp
   const string sql = """
       SELECT ProductId, Name, UnitPrice, Stock
       FROM dbo.Products
       WHERE IsActive = 1 AND Name LIKE @pattern
       ORDER BY Name;
       """;
   command.Parameters.Add(new SqlParameter("@pattern", SqlDbType.NVarChar, 100)
       { Value = $"%{searchTerm}%" });
   ```
2. Validate/limit `searchTerm` length at the API boundary (match column `NVARCHAR(100)`).
3. Ensure the DB account has least privilege — no `DROP`, no `xp_cmdshell` — defense in depth only; parameters are the real fix.

**Production takeaway:** Parameters send **values** separately from **command text**; SQL Server compiles a stable statement shape. See **ProductRepository.cs** Section 6 and **Program.cs** QUICK REFERENCE security block.

---

---

#### Q2. (R) A reporting query returns wrong rows for a price filter. Review:

```csharp
public decimal GetAverageAbovePrice(SqlConnection connection, decimal minPrice)
{
    const string sql = """
        SELECT AVG(UnitPrice)
        FROM dbo.Products
        WHERE IsActive = 1 AND UnitPrice >= @minPrice;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Int) { Value = minPrice });

    object? scalar = command.ExecuteScalar();
    return scalar is null or DBNull ? 0m : Convert.ToDecimal(scalar);
}
```

`minPrice = 19.99m` is passed from the API. What is wrong, what symptom might QA miss, and how should the parameter be declared?

---

**Answer:**

```csharp
public decimal GetAverageAbovePrice(SqlConnection connection, decimal minPrice)
{
    const string sql = """
        SELECT AVG(UnitPrice)
        FROM dbo.Products
        WHERE IsActive = 1 AND UnitPrice >= @minPrice;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Int) { Value = minPrice });

    object? scalar = command.ExecuteScalar();
    return scalar is null or DBNull ? 0m : Convert.ToDecimal(scalar);
}
```

`minPrice = 19.99m` is passed from the API. What is wrong, what symptom might QA miss, and how should the parameter be declared?

**Answer:** `@minPrice` is declared as `SqlDbType.Int` but the column and CLR value are **decimal/money** scale. SQL Server coerces or truncates the comparison, so the average can include or exclude rows incorrectly — QA might only test whole-dollar thresholds (`20`, `50`) and miss fractional prices.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `SqlDbType.Int` for a `DECIMAL(10,2)` column | Implicit conversion/truncation skews `>=` filter |
| Data integrity | No `Precision`/`Scale` on decimal param | Rounding mismatches vs column definition |
| Silent failure | Query executes without error | Wrong aggregates ship to dashboards |

**Fix (priority order):**

1. Match the column type — mirror `InsertProduct` in **ProductRepository.cs**:
   ```csharp
   command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal)
   {
       Value = minPrice,
       Precision = 10,
       Scale = 2,
   });
   ```
2. Add tests with fractional boundaries (`19.99`, `19.995`, `20.00`).
3. Prefer explicit `SqlDbType` over `AddWithValue` for money/decimal paths.

**Production takeaway:** Wrong `SqlDbType` rarely throws — it **silently changes semantics**. Always align parameter type, precision, and scale with the table definition.

---

---

#### Q3. (P) Your API receives product names of varying length (5–80 characters). A teammate uses `AddWithValue` everywhere "because it works":

```csharp
command.Parameters.AddWithValue("@name", userSuppliedName);
// SQL: WHERE Name = @name  — column is NVARCHAR(100)
```

Explain what happens inside SQL Server's plan cache when thousands of distinct name lengths hit this query. Why does the chapter prefer explicit `SqlDbType.NVarChar` with `Size`, and when is `AddWithValue` still acceptable?

---

**Answer:**

```csharp
command.Parameters.AddWithValue("@name", userSuppliedName);
// SQL: WHERE Name = @name  — column is NVARCHAR(100)
```

Explain what happens inside SQL Server's plan cache when thousands of distinct name lengths hit this query. Why does the chapter prefer explicit `SqlDbType.NVarChar` with `Size`, and when is `AddWithValue` still acceptable?

**Answer:** `AddWithValue` infers parameter type and **length from the runtime CLR value**, so each distinct string length can produce a **different parameter metadata signature** in cached plans — plan-cache **bloat** and **compile churn**. Explicit `NVarChar(100)` stabilizes the signature to match the column.

- **Mechanism:** For strings, the provider typically maps to `NVARCHAR(n)` where `n` is the string's current length (or `NVARCHAR(MAX)` for very long values). SQL Server auto-parameterizes by **type + length + precision**; `@name nvarchar(12)` and `@name nvarchar(47)` are different cache keys.
- **Performance impact:** Thousands of unique lengths → many near-identical plans, higher CPU on compilations, pressure on the plan cache — the pitfall demonstrated in **ProductRepository.DemonstrateAddWithValuePitfall**.
- **Index use:** If inference yields `NVARCHAR(4000)`/`MAX` while the column is `NVARCHAR(100)`, cardinality estimates and seek vs scan choices can degrade.
- **Chapter preference:** `new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = name }` — fixed size aligned to the schema, as in **InsertProduct** and **DemonstrateTypedParameters**.
- **When `AddWithValue` is OK:** One-off admin scripts, local prototypes, non-string types with unambiguous inference (e.g., `SqlDbType.Int` for a literal `42`), or parameters where length truly varies by design and you accept the cache trade-off.

**Production takeaway:** Production ADO.NET should treat parameter metadata as part of the **contract with SQL Server** — explicit `SqlDbType` + `Size`/`Precision`/`Scale`, not convenience inference.

---

---

#### Q4. (P) An ASP.NET Core admin export runs a heavy `SELECT` through ADO.NET. In staging it completes in 45 seconds; in production it intermittently throws:

```
SqlException: Execution Timeout Expired. The timeout period elapsed...
```

The developer sets `command.CommandTimeout = 0` on that command "so exports never fail." What are the risks of that fix, and what production pattern would you use instead?

---

**Answer:**

```
SqlException: Execution Timeout Expired. The timeout period elapsed...
```

The developer sets `command.CommandTimeout = 0` on that command "so exports never fail." What are the risks of that fix, and what production pattern would you use instead?

**Answer:** `CommandTimeout = 0` means **wait indefinitely** — exports stop failing fast but can hold connections, thread-pool threads, and SQL Server resources until someone kills the session. That is a reliability anti-pattern under load.

- **Default behavior:** `CommandTimeout` defaults to **30 seconds** per command (**ProductRepository.DemonstrateCommandTimeout**); timeout throws `SqlException` (often error number **-2**).
- **Risk of `0`:** Hung queries tie up pooled `SqlConnection` instances → pool exhaustion → site-wide outages; no back-pressure for runaway reports.
- **Staging vs prod gap:** Prod has more data, blocking, and IO latency — 45s work exceeds default 30s intermittently; the right response is not "infinite wait."
- **Better patterns:**
  - Set a **bounded** timeout above p99 runtime (e.g., 120s) for that command only.
  - Move long exports to a **background job** (queue + blob storage) with cancellation.
  - Use **async** execute + `CancellationToken` (ch08) so client disconnect cancels the command.
  - Tune/index the query; consider read-only replica for reporting.
  - Surface **504/202 Accepted** to the client instead of blocking an HTTP request for minutes.

**Production takeaway:** Timeouts are **failure detectors**, not annoyances to disable — tune per command, fail fast on OLTP paths, offload unbounded work.

---

---

#### Q5. (R) A stored procedure `dbo.usp_ProductCount` exists (see `Program.cs` setup). A teammate copies the preview pattern but changes the call:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "EXEC dbo.usp_ProductCount";   // inline EXEC string
command.CommandType = CommandType.Text;              // default — not changed

object? count = command.ExecuteScalar();
```

Meanwhile another path calls the same proc correctly with `CommandType.StoredProcedure` and `CommandText = "dbo.usp_ProductCount"`. What differs at runtime, and what breaks when they add an `@CategoryId` filter parameter to the proc?

---

**Answer:**

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "EXEC dbo.usp_ProductCount";   // inline EXEC string
command.CommandType = CommandType.Text;              // default — not changed

object? count = command.ExecuteScalar();
```

Meanwhile another path calls the same proc correctly with `CommandType.StoredProcedure` and `CommandText = "dbo.usp_ProductCount"`. What differs at runtime, and what breaks when they add an `@CategoryId` filter parameter to the proc?

**Answer:** `CommandType.Text` with `EXEC ...` sends an **ad-hoc batch**; `CommandType.StoredProcedure` invokes the proc through the **RPC interface** with metadata lookup. Both may return the same scalar today, but parameter binding, plan reuse, and permissions diverge once the proc gains parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API contract | `Text` + `EXEC` vs `StoredProcedure` RPC | Different server handling, quoting, and parameter discovery |
| Maintainability | Inline `EXEC` string duplicates proc name | Refactors to signature break at runtime, not compile time |
| Correctness | Adding `@CategoryId` to proc | `Text` path may ignore unbound params or require manual `EXEC ... @CategoryId = @p` string; easy to mis-order or inject |
| Security/perf | Ad-hoc EXEC batch | Loses some RPC benefits; harder to grant `EXECUTE` on proc only vs ad-hoc SQL |

**Fix (priority order):**

1. Use the **StoredProcedurePreview** pattern from **CommandPreviews.cs**:
   ```csharp
   command.CommandText = "dbo.usp_ProductCount";
   command.CommandType = CommandType.StoredProcedure;
   ```
2. When `@CategoryId` is added, bind explicitly:
   ```csharp
   command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
   ```
3. Drop the `EXEC dbo.usp_ProductCount` string form in application code.

**Production takeaway:** Procedure name in `CommandText` + `CommandType.StoredProcedure` is the ADO.NET contract for procs; reserve `CommandType.Text` for ad-hoc SQL only.

---

---

#### Q6. (R) An inventory sync proc returns the new stock via an output parameter. Review:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "dbo.usp_AdjustStock";
command.CommandType = CommandType.StoredProcedure;

command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
command.Parameters.Add(new SqlParameter("@Delta", SqlDbType.Int) { Value = delta });

var output = new SqlParameter("@NewStock", SqlDbType.Int) { Value = 0 };
command.Parameters.Add(output);

command.ExecuteNonQuery();
return (int)output.Value;
```

After deploy, `@NewStock` is always `0` even though the row updates correctly. Diagnose the defect and show the corrected parameter setup.

---

**Answer:**

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "dbo.usp_AdjustStock";
command.CommandType = CommandType.StoredProcedure;

command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
command.Parameters.Add(new SqlParameter("@Delta", SqlDbType.Int) { Value = delta });

var output = new SqlParameter("@NewStock", SqlDbType.Int) { Value = 0 };
command.Parameters.Add(output);

command.ExecuteNonQuery();
return (int)output.Value;
```

After deploy, `@NewStock` is always `0` even though the row updates correctly. Diagnose the defect and show the corrected parameter setup.

**Answer:** Output parameters must declare **`ParameterDirection.Output`** (or `InputOutput`). Without it, ADO.NET treats `@NewStock` as an **input-only** parameter; the initial `Value = 0` is sent to SQL Server and the returned output is never written back to `output.Value`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing `Direction = ParameterDirection.Output` | Client always reads the seeded `0`, not the proc's `SET @NewStock = …` |
| Runtime | No exception thrown | Silent wrong inventory counts in downstream systems |
| Design | Confusing input/output semantics | Same bug on `ReturnValue` if confused with output params |

**Fix (priority order):**

1. Set direction on the output parameter:
   ```csharp
   var output = new SqlParameter("@NewStock", SqlDbType.Int)
   {
       Direction = ParameterDirection.Output,
   };
   command.Parameters.Add(output);
   command.ExecuteNonQuery();
   return (int)output.Value!;  // Value populated after execute
   ```
2. Optionally use `ReturnValue` for proc return codes — separate parameter with `Direction = ParameterDirection.ReturnValue` (ch07 depth).
3. Integration-test the proc path, not only row side effects.

**Production takeaway:** For stored procedures, every non-input parameter needs an explicit **`ParameterDirection`** — output, input-output, or return value.

---

---

#### Q7. (R) A search builder constructs dynamic `WHERE` clauses at runtime:

```csharp
var sql = new StringBuilder("SELECT ProductId, Name FROM dbo.Products WHERE IsActive = 1");
var parameters = new List<SqlParameter>();

if (!string.IsNullOrEmpty(nameFilter))
{
    sql.Append(" AND Name LIKE @name");
    parameters.Add(new SqlParameter("name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
}
if (minPrice.HasValue)
{
    sql.Append(" AND UnitPrice >= @minPrice");
    parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal) { Value = minPrice.Value, Precision = 10, Scale = 2 });
}

using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql.ToString());
foreach (var p in parameters)
    command.Parameters.Add(p);
```

The query runs but `@name` never binds — every search returns unfiltered rows. What naming rule was violated, and how do you prevent this class of bug across dynamic SQL builders?

---

**Answer:**

```csharp
var sql = new StringBuilder("SELECT ProductId, Name FROM dbo.Products WHERE IsActive = 1");
var parameters = new List<SqlParameter>();

if (!string.IsNullOrEmpty(nameFilter))
{
    sql.Append(" AND Name LIKE @name");
    parameters.Add(new SqlParameter("name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
}
if (minPrice.HasValue)
{
    sql.Append(" AND UnitPrice >= @minPrice");
    parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal) { Value = minPrice.Value, Precision = 10, Scale = 2 });
}

using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql.ToString());
foreach (var p in parameters)
    command.Parameters.Add(p);
```

The query runs but `@name` never binds — every search returns unfiltered rows. What naming rule was violated, and how do you prevent this class of bug across dynamic SQL builders?

**Answer:** SQL placeholders use **`@name`** in `CommandText`, but the parameter was created as `"name"` without the **`@` prefix**. SQL Server parameter names must match; the orphan `@name` in SQL is unbound, so the `LIKE` predicate does not filter as intended (behavior depends on how SQL Server resolves unbound placeholders — often leading to ignored or defaulted logic while the query still executes).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `"name"` vs `"@name"` mismatch | Filter silently ineffective — returns all active products |
| Consistency | Mixed convention — `@minPrice` correct, `@name` param wrong | Copy-paste spreads the bug |
| Maintainability | Dynamic SQL + manual lists | Easy to desync placeholder names from parameter names |

**Fix (priority order):**

1. **Always use `@` in both SQL and C#** — per **ProductRepository.cs** Section 5:
   ```csharp
   parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
   ```
2. Centralize binding — helper `AddParameter(command, "@name", SqlDbType.NVarChar, 100, value)` used by the builder so names are single-sourced.
3. Add an integration test: filtered search returns strict subset; assert row count changes when filter applied.
4. Optional: static analysis or unit test that regex-matches `@\w+` tokens in SQL to `Parameters` collection keys.

**Production takeaway:** Treat `@ParameterName` as a **shared identifier** between SQL text and `SqlParameter.ParameterName` — case-insensitive on SQL Server, but the `@` prefix is required for reliable binding.

---

---

#### Q8. (R) To "reduce allocations," a singleton service caches one `SqlCommand` and reuses it across concurrent HTTP requests on pooled connections:

```csharp
public sealed class ProductLookupService
{
    private readonly SqlCommand _cachedCommand;

    public ProductLookupService(SqlConnection connection)
    {
        _cachedCommand = connection.CreateCommand();
        _cachedCommand.CommandText = "SELECT Name FROM dbo.Products WHERE ProductId = @id";
        _cachedCommand.Parameters.Add(new SqlParameter("@id", SqlDbType.Int));
    }

    public string? GetName(SqlConnection connection, int productId)
    {
        _cachedCommand.Connection = connection;
        _cachedCommand.Parameters["@id"].Value = productId;
        return _cachedCommand.ExecuteScalar() as string;
    }
}
```

Registered as `AddSingleton<ProductLookupService>()`. What fails under load, and what is the correct lifetime pattern for `SqlCommand` in a web app?

---

### 04. SqlDataReader

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/04. SqlDataReader`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public sealed class ProductLookupService
{
    private readonly SqlCommand _cachedCommand;

    public ProductLookupService(SqlConnection connection)
    {
        _cachedCommand = connection.CreateCommand();
        _cachedCommand.CommandText = "SELECT Name FROM dbo.Products WHERE ProductId = @id";
        _cachedCommand.Parameters.Add(new SqlParameter("@id", SqlDbType.Int));
    }

    public string? GetName(SqlConnection connection, int productId)
    {
        _cachedCommand.Connection = connection;
        _cachedCommand.Parameters["@id"].Value = productId;
        return _cachedCommand.ExecuteScalar() as string;
    }
}
```

Registered as `AddSingleton<ProductLookupService>()`. What fails under load, and what is the correct lifetime pattern for `SqlCommand` in a web app?

**Answer:** **`SqlCommand` is not thread-safe** and must not be shared across concurrent requests. A singleton holding one command causes race conditions on `Connection`, `Parameters`, and internal state — intermittent wrong names, exceptions, or corrupted reads under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Singleton `_cachedCommand` mutated by all threads | Data races on `Parameters["@id"].Value` and `Connection` assignment |
| Lifetime | Command created with ctor `connection` then reassigned per call | Undefined behavior when two requests swap `Connection` concurrently |
| Design | Caching command to "reduce allocations" | `SqlCommand` allocation is cheap vs a round-trip; pooling is for **connections**, not commands |
| Connection rules | One active `Execute*` per connection (without MARS) | Shared command amplifies reader/command conflicts |

**Fix (priority order):**

1. **Create one command per operation** — `using var cmd = connection.CreateCommand()` or **CommandFactory.CreateTextCommand** inside the method (see **ProductRepository.DemonstrateCommandDisposal**).
2. Register the **repository/service as scoped** (per request), not singleton, if it holds no shared command state.
3. Let **connection pooling** handle reuse — open short-lived connections from the pool; do not cache commands.
4. If micro-optimizing, cache the **SQL string**, not the `SqlCommand` instance.

**Production takeaway:** Pattern from **Program.cs** QUICK REFERENCE — *"Share one SqlCommand across threads → undefined behavior."* Commands are per-operation; connections are pooled.

---

### 04. SqlDataReader

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/04. SqlDataReader`

---

---

#### Q1. (R) A production export job intermittently hangs with "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool." Review this repository method copied from an internal tool:

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    var connection = new SqlConnection(connectionString);
    connection.Open();

    var command = new SqlCommand(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;",
        connection);

    SqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        yield return new ProductRow
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            UnitPrice = reader.GetDecimal(2),
            StockQuantity = reader.GetInt32(3),
            DiscontinuedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
        };
    }
}
```

The caller does `foreach (var p in repo.StreamProducts(cs)) { await WriteCsvLine(p); }`. What breaks under load, and how do you fix it without loading the entire table into a `List<T>` first?

---

**Answer:**

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    var connection = new SqlConnection(connectionString);
    connection.Open();

    var command = new SqlCommand(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;",
        connection);

    SqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        yield return new ProductRow
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            UnitPrice = reader.GetDecimal(2),
            StockQuantity = reader.GetInt32(3),
            DiscontinuedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
        };
    }
}
```

The caller does `foreach (var p in repo.StreamProducts(cs)) { await WriteCsvLine(p); }`. What breaks under load, and how do you fix it without loading the entire table into a `List<T>` first?

**Answer:** The iterator keeps the `SqlConnection` and `SqlDataReader` open for the entire `foreach` lifetime, and nothing ever disposes them — so under concurrent exports the pool exhausts and new requests time out waiting for a connection. Fix by owning disposal explicitly (typically `IAsyncEnumerable<T>` with `await using`, or pass ownership of a reader wrapped with `CommandBehavior.CloseConnection` to a caller that disposes promptly).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / resource | `SqlConnection`, `SqlCommand`, and `SqlDataReader` never disposed | Server-side cursors and client handles leak; connection not returned to pool |
| Async | Caller `await`s I/O between `MoveNext()` calls while reader stays open | Long-held connections amplify pool starvation under parallel jobs |
| Design | `IEnumerable<T>` + `yield return` hides connected lifetime | Callers cannot see they must finish enumeration quickly |
| Correctness | No `try/finally` / `using` around reader or connection | Exception mid-loop leaks connection even if enumeration stops |

**Fix (priority order):**

1. **Dispose on all paths** — wrap connection, command, and reader in `using`/`await using` and only yield inside that scope, *or* switch to `IAsyncEnumerable<ProductRow>` so disposal runs when enumeration completes or is cancelled.
2. **Keep streaming** — do not materialize `List<ProductRow>`; read one row, map, yield, repeat — same memory profile as today but with guaranteed cleanup.
3. **Consider `CommandBehavior.CloseConnection`** when returning a reader to a dedicated exporter that owns disposal — see **CloseConnectionBehaviorDemo.cs**.
4. **Cap concurrency** — limit parallel export workers so even correct code cannot open more connections than `Max Pool Size` allows.
5. **Prefer async APIs** for the write side — `await reader.ReadAsync()` in ch.08 avoids blocking thread-pool threads while the connection stays open (see Q6).

Example shape (sync streaming with owned disposal):

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    using var command = new SqlCommand(sql, connection);
    using var reader = command.ExecuteReader();
    while (reader.Read())
        yield return MapRow(reader);
}
```

**Production takeaway:** A `SqlDataReader` is a **leased connection**, not a lazy collection — whoever opens it must dispose it before the connection returns to the pool. See **ProperDisposalDemo.cs** and **Program.cs** quick-reference disposal table.

---

---

#### Q2. (R) Two teammates map the same `Products` query differently. Review both snippets:

```csharp
// Team A — BasicReadLoopDemo style
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string name = reader.GetString(1);
    decimal price = reader.GetDecimal(2);
}

// Team B — ProductMapper style (called inside every Read())
while (reader.Read())
{
    products.Add(ProductMapper.MapFromReader(reader));
}

// ProductMapper.MapFromReader (current chapter code)
public static ProductRow MapFromReader(SqlDataReader reader)
{
    int ordId = reader.GetOrdinal("ProductId");
    int ordName = reader.GetOrdinal("ProductName");
    // … GetOrdinal for each column on every row …
}
```

A DBA adds `ModifiedAt` as the first column in `SELECT * FROM dbo.Products` for auditing. Team A's export still "works" but prices look like integers. Team B's page is slower on 200k rows. Diagnose both failure modes and describe the mapping approach you would standardize on.

---

**Answer:**

```csharp
// Team A — BasicReadLoopDemo style
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string name = reader.GetString(1);
    decimal price = reader.GetDecimal(2);
}

// Team B — ProductMapper style (called inside every Read())
while (reader.Read())
{
    products.Add(ProductMapper.MapFromReader(reader));
}

// ProductMapper.MapFromReader (current chapter code)
public static ProductRow MapFromReader(SqlDataReader reader)
{
    int ordId = reader.GetOrdinal("ProductId");
    int ordName = reader.GetOrdinal("ProductName");
    // … GetOrdinal for each column on every row …
}
```

A DBA adds `ModifiedAt` as the first column in `SELECT * FROM dbo.Products` for auditing. Team A's export still "works" but prices look like integers. Team B's page is slower on 200k rows. Diagnose both failure modes and describe the mapping approach you would standardize on.

**Answer:** Team A's magic ordinals silently shift when column order changes — `GetDecimal(2)` now reads `ModifiedAt`, not `UnitPrice`. Team B's name-based mapping survives column reorder but pays repeated `GetOrdinal` lookups every row; the chapter's **ProductMapper** should cache ordinals once per reader/result set, not inside `MapFromReader` on every call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Magic indexes (`0`, `1`, `2`) bind to **position**, not meaning | Schema/query changes corrupt data without compile errors |
| Performance | `GetOrdinal` inside `MapFromReader` on every `Read()` | O(rows × columns) name lookups — noticeable at 200k rows |
| Maintainability | Two incompatible conventions on the same table | Code review cannot tell which queries are safe after SELECT changes |

**Fix (priority order):**

1. **Standardize on cached ordinals resolved once** after `ExecuteReader`, before the loop — pattern from **ColumnMetadataDemo.cs** and the quick reference in **Program.cs**.
2. **Explicit SELECT lists** — never rely on `SELECT *` column order in production readers; list columns in a stable contract.
3. **Refactor ProductMapper** — accept precomputed ordinals or resolve once via a small `ProductColumnOrdinals` struct:

```csharp
var ord = ProductColumnOrdinals.From(reader); // GetOrdinal once
while (reader.Read())
    products.Add(ProductMapper.Map(reader, ord));
```

4. **Reserve magic indexes** only for throwaway demos (**BasicReadLoopDemo.cs**) or proven-frozen micro-queries — not shared repositories.

**Production takeaway:** **GetOrdinal by name once + typed getters** gives schema-order resilience without per-row lookup cost. Magic indexes are fast but fragile — treat them like pointer arithmetic.

---

---

#### Q3. (R) A nightly job reads discontinued products and crashes on row 847 with `SqlNullValueException`. Review the mapping helper:

```csharp
while (reader.Read())
{
    var row = new ProductRow
    {
        ProductId = (int)reader["ProductId"],
        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
        StockQuantity = reader.GetField<int>("StockQuantity"), // extension: (T)reader[name]
        DiscontinuedDate = (DateTime?)reader["DiscontinuedDate"]
    };
    rows.Add(row);
}
```

What is wrong with each nullable/value-type read, and how does this differ from the `IsDBNull` pattern in **NullHandlingDemo.cs**?

---

**Answer:**

```csharp
while (reader.Read())
{
    var row = new ProductRow
    {
        ProductId = (int)reader["ProductId"],
        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
        StockQuantity = reader.GetField<int>("StockQuantity"), // extension: (T)reader[name]
        DiscontinuedDate = (DateTime?)reader["DiscontinuedDate"]
    };
    rows.Add(row);
}
```

What is wrong with each nullable/value-type read, and how does this differ from the `IsDBNull` pattern in **NullHandlingDemo.cs**?

**Answer:** SQL `NULL` surfaces as `DBNull.Value`, not C# `null`, on value-type columns — casting or calling typed getters without an `IsDBNull` check throws `SqlNullValueException` or `InvalidCastException`. The chapter pattern branches on `IsDBNull` before `GetDateTime`, and never casts boxed `DBNull` to `DateTime?`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `(DateTime?)reader["DiscontinuedDate"]` when column is SQL NULL | `InvalidCastException` — `DBNull` does not unbox to nullable |
| Runtime | `GetDecimal` / `GetField<int>` on NULL numeric columns | `SqlNullValueException` — typed getters reject NULL |
| Performance | `GetOrdinal("ProductName")` inside every row | Unnecessary overhead (secondary to correctness bug) |
| Design | Indexer + cast hides null semantics | Looks like EF/Dapper mapping but lacks null guards |

**Fix (priority order):**

1. **DiscontinuedDate** — match **NullHandlingDemo.cs**:

```csharp
DateTime? discontinued = reader.IsDBNull(ordDisc)
    ? null
    : reader.GetDateTime(ordDisc);
```

2. **Other nullable numerics** — if a column can be NULL, use `IsDBNull` or `reader.GetFieldValue<int?>(ord)` (.NET typed helper) before assigning to `int?`.
3. **Cache ordinals** before the loop; use `GetInt32`/`GetString`/`GetDecimal` instead of `(int)reader["ProductId"]` to avoid double boxing.
4. **Optional columns** — if `ProductName` were NULL, `GetString` returns empty string on some providers but behavior varies; explicit `IsDBNull` is safer for reference types you want as C# `null`.

**Production takeaway:** ADO.NET NULL is **`DBNull.Value`**, not `null` — always gate typed accessors with `IsDBNull` (or `GetFieldValue<T>` for nullable value types). See **NullHandlingDemo.cs** and **Program.cs** common-mistakes table.

---

---

#### Q4. (M) A stored procedure returns three result sets: (1) product rows, (2) aggregate counts, (3) audit metadata. A junior developer adapts **MultipleResultSetsDemo.cs** like this:

```csharp
using var reader = await cmd.ExecuteReaderAsync();
var products = new List<ProductRow>();
int totalCount = 0;

do
{
    while (await reader.ReadAsync())
    {
        if (reader.FieldCount > 1)
            products.Add(MapProduct(reader));
        else if (reader.FieldCount == 1)
            totalCount = reader.GetInt32(0);
    }
}
while (await reader.ReadAsync()); // advance to next result set

return new ProductPage(products, totalCount);
```

In QA, the API hangs or returns `totalCount = 0` with only the first product row, and memory grows when the procedure returns 50k products. What did they misunderstand about `NextResult()`, and what is the correct consumption pattern?

---

**Answer:**

```csharp
using var reader = await cmd.ExecuteReaderAsync();
var products = new List<ProductRow>();

while (await reader.ReadAsync())
    products.Add(MapProduct(reader));

int totalCount = 0;
if (reader.NextResult())
{
    if (reader.Read())
        totalCount = reader.GetInt32(reader.GetOrdinal("ProductCount"));
}

return new ProductPage(products, totalCount);
```

In QA, `totalCount` is always `0` and memory spikes when the procedure returns 50k products. What did they misunderstand about `NextResult()`, and what is the correct consumption pattern?

**Answer:** They call `ReadAsync()` where the API requires `NextResult()` — `ReadAsync` advances **rows within the current result set**, not to the next SELECT in the batch. After the inner loop drains set 1, the outer `while (await reader.ReadAsync())` either finds no more rows (stops early with wrong `totalCount`) or mis-reads rows from set 2 as if they were products. The chapter pattern is `do { … Read loop … } while (await reader.NextResultAsync())`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ReadAsync()` used to advance between result sets | Never reaches set 2/3 correctly — `ReadAsync` only moves to next **row** |
| Correctness | `FieldCount` heuristic mixes product rows and aggregate rows in one loop | Set 2 row with one column overwrites `totalCount`; product rows misclassified |
| Design | Loading 50k rows into `List<ProductRow>` | Memory spike — reader streaming benefit lost |
| Correctness | Third audit result set never consumed | Unread results can block some providers or leave batch incomplete |

**Fix (priority order):**

1. **Replace the outer `ReadAsync` with `NextResultAsync`** — use the chapter pattern from **MultipleResultSetsDemo.cs**:

```csharp
var products = new List<ProductRow>();
int totalCount = 0;
int setNumber = 0;

do
{
    setNumber++;
    while (await reader.ReadAsync())
    {
        if (setNumber == 1)
            products.Add(MapProduct(reader));
        else if (setNumber == 2)
            totalCount = reader.GetInt32(0);
        // set 3: drain or map audit columns
    }
}
while (await reader.NextResultAsync());
```

2. **Drain every set** — even unneeded sets need an empty `while (ReadAsync())` loop before the next `NextResult()`.
3. **Detect set shape** by column count/names, or use **separate commands** if the SP contract is unstable.
4. **Stream set 1** to response/file if 50k rows — do not require full materialization for export scenarios.

**Production takeaway:** `NextResult()` moves to the **next SELECT** in a batch — each set needs its own complete `Read` loop. Stored procedures with multiple selects are common; treat each set as a separate mini-reader.

---

---

#### Q5. (P) A data-access layer exposes streaming reads to upper layers:

```csharp
public SqlDataReader OpenProductStream()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var cmd = new SqlCommand("SELECT * FROM dbo.Products ORDER BY ProductId", connection);
    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
}
```

The API controller does:

```csharp
public IActionResult Export()
{
    using var reader = _repo.OpenProductStream();
    // … write rows to response …
    return File(stream, "text/csv");
}
```

Under what conditions does this pattern work, and what connection-lifetime mistakes still cause "There is already an open DataReader associated with this Connection" or leaked connections in production?

---

**Answer:**

```csharp
public SqlDataReader OpenProductStream()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var cmd = new SqlCommand("SELECT * FROM dbo.Products ORDER BY ProductId", connection);
    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
}
```

The API controller does:

```csharp
public IActionResult Export()
{
    using var reader = _repo.OpenProductStream();
    // … write rows to response …
    return File(stream, "text/csv");
}
```

Under what conditions does this pattern work, and what connection-lifetime mistakes still cause "There is already an open DataReader associated with this Connection" or leaked connections in production?

**Answer:** `CommandBehavior.CloseConnection` works when the **caller disposes the reader** and that caller is the only code using the hidden connection — disposing the reader closes the connection automatically (**CloseConnectionBehaviorDemo.cs**). It fails when anything else tries to reuse the same `SqlConnection` while the reader is open, or when the reader is never disposed (aborted request, exception, missing `using`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | Second operation on same connection before reader disposed | `InvalidOperationException`: open DataReader associated with Connection |
| Lifetime | Reader not disposed on early return / exception | Connection leak despite `CloseConnection` — behavior only runs on dispose |
| Design | Returning raw `SqlDataReader` leaks ADO.NET through repository boundary | Callers must know ADO.NET disposal rules |
| ASP.NET | Request cancelled mid-stream | Must pass `CancellationToken` to `ReadAsync` and dispose reader in `finally` |

**Fix (priority order):**

1. **Document ownership** — caller must `using`/ `await using` the reader; repository must not retain connection reference after handoff.
2. **Never share one `SqlConnection`** across concurrent readers or commands — one open reader per connection (MARS aside — separate topic).
3. **Prefer encapsulation** — `IAsyncEnumerable<ProductRow>`, callback `Action<SqlDataReader>`, or write directly to `Stream` inside the repository so connection lifetime stays internal (**ProperDisposalDemo.cs** pattern).
4. **If keeping raw reader** — pair with `CloseConnection` (as shown) *and* ensure `SqlCommand` is not disposed before reader finishes (command lifetime tied to reader on some providers).
5. **Monitor pool** — log when exports abort; use `try/finally` to dispose reader even when `OperationCanceledException` fires.

**Production takeaway:** `CloseConnection` transfers connection cleanup to reader disposal — it does not remove the rule that **one connection ↔ one active reader**. See **CloseConnectionBehaviorDemo.cs** and ch.02 connection lifetime.

---

---

#### Q6. (P) An ops team exports `DocumentBody varbinary(max)` for 10,000 rows (~5 MB each). A developer loads full rows like **BasicReadLoopDemo** and the app hits `OutOfMemoryException`. They switch to:

```csharp
using var reader = cmd.ExecuteReader(
    CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection);

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);
    byte[] body = (byte[])reader[2]; // DocumentBody
    await WriteToBlobStorage(id, body);
}
```

Explain how `CommandBehavior.SequentialAccess` is meant to work, what is still wrong with `(byte[])reader[2]`, and sketch the production-safe read pattern for large LOBs.

---

**Answer:**

```csharp
using var reader = cmd.ExecuteReader(
    CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection);

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);
    byte[] body = (byte[])reader[2]; // DocumentBody
    await WriteToBlobStorage(id, body);
}
```

Explain how `CommandBehavior.SequentialAccess` is meant to work, what is still wrong with `(byte[])reader[2]`, and sketch the production-safe read pattern for large LOBs.

**Answer:** `SequentialAccess` tells the provider to stream columns in **increasing ordinal order** without buffering the whole row — large values should be read with `GetBytes`/`GetChars` in chunks. Casting `reader[2]` to `byte[]` still materializes the entire LOB into memory, defeating the flag.

- **SequentialAccess rules** (**SequentialAccessPreviewDemo.cs**): read columns ordinally (0, 1, 2…); do not re-read an earlier column after moving forward; combine with `CloseConnection` when handing reader to exporter.
- **What's wrong with `(byte[])reader[2]`:** indexer loads full column value — ~50 GB total for 10k × 5 MB → OOM.
- **Production pattern:**

```csharp
const int chunkSize = 81920;
var buffer = new byte[chunkSize];
long offset = 0;

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);

    await using var blobStream = OpenBlobStream(id);
    long read;
    do
    {
        read = reader.GetBytes(2, offset, buffer, 0, buffer.Length);
        offset += read;
        if (read > 0)
            await blobStream.WriteAsync(buffer.AsMemory(0, (int)read));
    } while (read == chunkSize);

    offset = 0; // reset for next row
}
```

- **Async:** prefer `GetBytes`/`ReadAsync` (ch.08) so thread pool isn't blocked during network I/O.
- **Alternative:** `OPENROWSET`/blob storage URLs in SQL so the app never pulls multi-MB through the app tier.

**Production takeaway:** SequentialAccess is for **chunked LOB streaming**, not smaller magic indexes. Pair with bounded buffers and async writes — see **SequentialAccessPreviewDemo.cs** and **Program.cs** CommandBehavior table.

---

---

#### Q7. (D) `ProductMapper.MapFromReader` resolves columns by name (`GetOrdinal("ProductId")`, etc.) and is reused across three queries: a list screen, a search SP that aliases `ProductId AS Id`, and a reporting view that exposes `SKU` instead of `ProductName`. The chapter positions this mapper as the pattern "Dapper and EF Core automate." What fails in production as queries diverge, and how would you refactor for maintainability without jumping straight to EF Core?

---

---

### 05. DataSet, DataTable & SqlDataAdapter

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/05. DataSet, DataTable & SqlDataAdapter`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

**Answer:** A single name-based mapper assumes every query projects the **same column names** — aliasing or view renames cause `IndexOutOfRangeException` from `GetOrdinal` at runtime, not compile time. Reusing **ProductMapper** across heterogeneous queries is the fragility the chapter warns about when moving from demo to production.

- **What fails:** `GetOrdinal("ProductId")` on `SELECT Id …` throws; `GetOrdinal("ProductName")` on a view with `SKU` throws; each failure hits production on first request after deployment — no compiler signal.
- **Why it looked fine locally:** All demos use one explicit SELECT with canonical names (**ProperDisposalDemo.cs**).
- **Refactor options (short of EF Core):**
  1. **One mapper per query shape** — `MapFromProductListReader`, `MapFromSearchSpReader` with ordinals cached per shape.
  2. **Column ordinals struct built from the actual query** — factory method documents required names in one place.
  3. **Dapper** with explicit multi-mapping or default column matching — still requires consistent aliases (`AS ProductId`).
  4. **Integration tests** against real view/SP definitions to catch rename breaks in CI.
  5. **Views or SPs as stable contracts** — DB team owns column names; app mappers target the view, not ad-hoc SELECTs.

```csharp
// Stable contract: dbo.v_ProductList always exposes ProductId, ProductName, …
internal static class ProductListReader
{
    public static ProductColumnOrdinals Ordinals { get; private set; }

    public static void Init(SqlDataReader r) =>
        Ordinals = ProductColumnOrdinals.From(r, required: ["ProductId", "ProductName", …]);
}
```

**Production takeaway:** Manual mapping buys control at the cost of **schema coupling** — isolate that coupling per query contract or adopt a tool (Dapper/EF) with explicit configuration. See **ProductMapper.cs** as the teaching baseline, not a shared global mapper.

---

---

### 05. DataSet, DataTable & SqlDataAdapter

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/05. DataSet, DataTable & SqlDataAdapter`

---

---

#### Q1. (R) A legacy ASP.NET Core API endpoint loads an entire `production.orders` table into a `DataSet`, lets callers filter in memory, and returns JSON. Review the service method:

```csharp
public async Task<IActionResult> SearchOrders(string? statusFilter)
{
    using var connection = new SqlConnection(_connString);
    using var adapter = new SqlDataAdapter("SELECT * FROM production.orders", connection);
    var dataSet = new DataSet("OrdersSnapshot");
    adapter.Fill(dataSet); // ~400k rows, 40+ columns each

    DataTable orders = dataSet.Tables[0]!;
    if (!string.IsNullOrEmpty(statusFilter))
    {
        foreach (DataRow row in orders.Rows)
        {
            if (row["Status"]?.ToString() != statusFilter)
                row.Delete();
        }
        orders.AcceptChanges(); // "clean up" deleted rows before serialize
    }

    return Ok(orders); // serializes remaining rows to JSON
}
```

What breaks under load, and what would you change first?

---

**Answer:**

```csharp
public async Task<IActionResult> SearchOrders(string? statusFilter)
{
    using var connection = new SqlConnection(_connString);
    using var adapter = new SqlDataAdapter("SELECT * FROM production.orders", connection);
    var dataSet = new DataSet("OrdersSnapshot");
    adapter.Fill(dataSet); // ~400k rows, 40+ columns each

    DataTable orders = dataSet.Tables[0]!;
    if (!string.IsNullOrEmpty(statusFilter))
    {
        foreach (DataRow row in orders.Rows)
        {
            if (row["Status"]?.ToString() != statusFilter)
                row.Delete();
        }
        orders.AcceptChanges(); // "clean up" deleted rows before serialize
    }

    return Ok(orders); // serializes remaining rows to JSON
}
```

What breaks under load, and what would you change first?

**Answer:** This endpoint materializes the full orders table into RAM on every request, then mutates row state just to filter — a disconnected-model pattern suited to desktop grids, not HTTP APIs. Under concurrent traffic it will cause memory pressure, GC churn, and long latencies long before SQL becomes the bottleneck.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Scalability | `SELECT *` + `Fill` of ~400k rows per request | Large Gen2 heap allocations; OOM risk under parallel calls |
| API design | In-memory filter via `Delete()` + `AcceptChanges()` | Wastes CPU/RAM; filter belongs in SQL (`WHERE`) or keyed paging |
| Correctness | `async` action but synchronous `Fill` | Blocks thread pool; no cancellation |
| Serialization | Returning raw `DataTable` to JSON | Awkward shape (`RowError`, schema metadata); unbounded payload |
| Design | Disconnected snapshot for a read-only search | Wrong tool — see **DisconnectedModelNotes.cs** (reader = stream, table = edit snapshot) |

**Fix (priority order):**

1. Push filter to SQL: `WHERE Status = @status` with parameters; return only needed columns and a page (`OFFSET/FETCH` or keyset).
2. Replace `DataSet`/`DataTable` with `SqlDataReader`, Dapper, or EF Core projection to DTOs — stream or page, do not hold full table per request.
3. If results must be large, add export/async job — not a synchronous API snapshot.
4. Make the action truly async (`FillAsync` / EF `ToListAsync`) and accept `CancellationToken`.
5. Map to typed response models instead of serializing `DataTable`.

**Production takeaway:** `Fill` loads a **mutable offline copy** — ideal for **AcceptRejectDemo.cs** / grid edit cycles, not for stateless microservice reads. Memory bloat shows up only when traffic multiplies snapshot size.

---

---

#### Q2. (M) After `adapter.Update(table)` throws because two rows failed with a SQL error, a developer resets UI state so users can retry. They call `table.AcceptChanges()` on the whole table "to clear the error flags." Before that, one row was `Modified`, one was `Added`, and one was `Deleted`. What is wrong with calling `AcceptChanges()` here, and what should happen instead?

---

**Answer:**

**Answer:** `AcceptChanges()` does not clear errors — it **commits** pending edits in memory as if the database save succeeded. After a failed `Update`, that discards the distinction between saved and unsaved work and can leave the database and UI permanently out of sync.

- **`Modified` → `Unchanged`:** Current values become the new Original baseline even though SQL never persisted the edit — the next `Update` may skip the row or generate wrong SQL.
- **`Added` → `Unchanged`:** The row looks persisted locally but has no matching INSERT success (or a partial insert elsewhere in the batch).
- **`Deleted` → removed from `Rows`:** The row vanishes from the grid while still alive in SQL — classic "row came back on refresh" bug.
- **`RejectChanges()`** (per row or table) is the undo path: Modified reverts to Original, Added rows drop out, Deleted rows restore — see **AcceptRejectDemo.cs** and **RowStateDemo.cs**.
- On partial `Update` failure, inspect `row.RowError` / `GetErrors()`, fix or reject failed rows only, and retry inside a **SqlTransaction** (chapter 06) so one failure rolls back the batch.
- Call **`AcceptChanges()` only after a successful `Update`** — the pipeline in **AdapterUpdateOverview.cs** step 4.

**Production takeaway:** RowState is your offline change log; `AcceptChanges()` is "commit session," not "clear exception." Misusing it after failure is worse than leaving rows dirty.

---

---

#### Q3. (R) Two users edit the same category row offline in a WinForms grid backed by `SqlDataAdapter`. User A changes `category_name` to "Mountain Bikes"; User B changes it to "MTB" and saves first. User A clicks Save. Review the update setup:

```csharp
const string selectSql = """
    SELECT category_id, category_name
    FROM production.categories
    WHERE category_id = @id
    """;
using var adapter = new SqlDataAdapter(selectSql, connection);
adapter.SelectCommand!.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 1 });
using var builder = new SqlCommandBuilder(adapter);

// Fill → user edits row → Update
adapter.Update(categoriesTable);
categoriesTable.AcceptChanges();
```

`SqlCommandBuilder` generated an `UPDATE` with only `category_name` in the `SET` clause and `category_id` in the `WHERE` clause — no rowversion/timestamp check. What failure mode does User A hit, and how do you fix optimistic concurrency for this disconnected pattern?

---

**Answer:**

```csharp
const string selectSql = """
    SELECT category_id, category_name
    FROM production.categories
    WHERE category_id = @id
    """;
using var adapter = new SqlDataAdapter(selectSql, connection);
adapter.SelectCommand!.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 1 });
using var builder = new SqlCommandBuilder(adapter);

// Fill → user edits row → Update
adapter.Update(categoriesTable);
categoriesTable.AcceptChanges();
```

`SqlCommandBuilder` generated an `UPDATE` with only `category_name` in the `SET` clause and `category_id` in the `WHERE` clause — no rowversion/timestamp check. What failure mode does User A hit, and how do you fix optimistic concurrency for this disconnected pattern?

**Answer:** User A's save succeeds with a **silent last-write-wins overwrite** — B's "MTB" is lost without error. Default `CommandBuilder` updates match on primary key only; disconnected editing has no built-in "someone else changed this since Fill" guard.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | No `rowversion` / original-value predicate in `UPDATE` | Lost updates; no user-visible conflict |
| Disconnected model | Original values from Fill are stale after B saves | A's `Modified` row still looks valid |
| UX | No `DBConcurrencyException` handling | Users trust save confirmation incorrectly |

**Fix (priority order):**

1. Add **`rowversion`** (or `timestamp`) to SELECT; include `@Original_rowversion` in a custom `UpdateCommand` `WHERE` clause, or use `CommandBuilder.ConflictOption = ConflictOption.CompareAllSearchableValues` / compare original values for critical columns.
2. On `DBConcurrencyException`, offer **refresh + merge** or **RejectChanges** on that row — do not blanket `AcceptChanges()`.
3. Shorten offline window: re-`Fill` before edit, or move to online API with ETag/`If-Match` for services.
4. Wrap multi-row `Update` in a transaction so related edits stay consistent.

**Production takeaway:** Disconnected optimistic concurrency requires **you** to compare "what I read" vs "what DB has now" — RowState alone only tracks local edits. See **Program.cs** quick reference on PrimaryKey + Update matching.

---

---

#### Q4. (D) A microservices team proposes sharing a `DataSet` between an order API, a reporting worker, and a mobile sync service "so everyone has the same offline cache." When would you push back and recommend `SqlDataReader`, Dapper, or EF Core instead? Name at least two concrete reasons tied to deployment and API design.

---

**Answer:**

**Answer:** A shared in-process `DataSet` is a monolith-era cache, not a service boundary. Each microservice should own its persistence and contract; shipping `DataTable` snapshots across HTTP or queues couples schemas, memory footprints, and deployment cadence.

- **No cross-process sharing:** `DataSet` is in-memory CLR state — it does not replicate across pods. Each instance would `Fill` its own copy (3× memory, 3× stale snapshots) unless you add Redis/DB anyway.
- **Unbounded RAM & GC:** Large `Fill` snapshots (see **LocalDbFillDemo.cs**) fight container memory limits; APIs should stream (`SqlDataReader`) or page typed DTOs.
- **Schema coupling:** Column renames break all consumers at once — JSON DTOs or explicit API versions decouple deploys.
- **Wrong change-tracking model:** `RowState` / `Update` batches fit **single-user grid sessions**, not concurrent REST handlers — EF Core change tracker or explicit commands per aggregate are clearer.
- **Serialization cost:** `DataTable` JSON is heavy and leaky; clients expect stable REST contracts, not ADO.NET wire formats.
- **When DataSet still fits:** Single desktop app, local report designer, or one-shot import staging table on one machine — not multi-service topology.

**Production takeaway:** Use disconnected ADO.NET where the user **edits a snapshot offline** (**AcceptRejectDemo.cs**); use connected/streaming access for **stateless, scaled-out services**.

---

---

#### Q5. (P) A nightly job must insert 2 million staging rows from a CSV import. One developer uses `SqlDataAdapter.Update` on a `DataTable` with 2M `Added` rows; another uses `SqlBulkCopy.WriteToServer(dataTable)` inside a transaction. Compare throughput, change tracking, and failure behavior. Which path matches this ETL job, and what from chapter 06 still applies?

---

**Answer:**

**Answer:** For bulk append-only loads, **`SqlBulkCopy`** is the production path; `adapter.Update` issues per-row `InsertCommand` executions and drowns in round-trip overhead.

- **`SqlDataAdapter.Update`:** Inspects each row's `RowState` (`Added` → `InsertCommand` per **AdapterUpdateOverview.cs**); ~2M individual inserts; slow; useful when rows are heterogeneous (mix of Added/Modified/Deleted) or UI batch save.
- **`SqlBulkCopy`:** Streams rows to one destination table (**SqlBulkCopyPreview.cs**); minimal logging overhead; does not honor RowState — all rows append; no automatic UPDATE/DELETE.
- **Change tracking:** Adapter path needs `Added` rows and optional `AcceptChanges()` after success; bulk path treats table as dumb row bag — validate before `WriteToServer`.
- **Failure behavior:** Adapter can stop mid-batch with some rows committed unless wrapped in transaction; bulk copy supports transactional all-or-nothing with **`SqlTransaction`** (chapter 06), plus `BatchSize` tuning and `NotifyAfter` progress.
- **Pick bulk when:** append-only import to staging, no per-row business rules in SQL command text, volume > tens of thousands.
- **Pick adapter when:** small editable grid sync back to source with Insert/Update/Delete mix and CommandBuilder-generated commands.

**Production takeaway:** RowState-driven `Update` is for **interactive edit sessions**; **`SqlBulkCopy`** is for **volume ingest** — previewed in this chapter, detailed in chapter 06.

---

---

#### Q6. (R) Production deploys a DB migration that renames column `UnitPrice` → `ListPrice`. The API still `Fill`s into a cached `DataTable` schema created at startup (columns defined in code). After deploy, users edit prices in the grid and call save. Review the save path:

```csharp
// Startup: table schema built in memory with columns ProductId, ProductName, UnitPrice
adapter.Fill(products); // DB now returns ListPrice — Fill adds a second price column

foreach (DataRow row in products.Rows)
{
    if (row["UnitPrice"] is DBNull && row["ListPrice"] is not DBNull)
        row["UnitPrice"] = row["ListPrice"]; // "migrate" in memory
}

adapter.Update(products);
products.AcceptChanges();
```

What RowState and persistence bugs appear after schema drift, and how do you prevent silent data loss?

---

**Answer:**

```csharp
// Startup: table schema built in memory with columns ProductId, ProductName, UnitPrice
adapter.Fill(products); // DB now returns ListPrice — Fill adds a second price column

foreach (DataRow row in products.Rows)
{
    if (row["UnitPrice"] is DBNull && row["ListPrice"] is not DBNull)
        row["UnitPrice"] = row["ListPrice"]; // "migrate" in memory
}

adapter.Update(products);
products.AcceptChanges();
```

What RowState and persistence bugs appear after schema drift, and how do you prevent silent data loss?

**Answer:** Schema drift plus manual column copying produces **wrong RowState**, **CommandBuilder SQL against stale column names**, and **silent null writes** — users think they saved `ListPrice` while the DB gets stale or empty `UnitPrice`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema | `Fill` **adds** `ListPrice` column; in-code schema still has `UnitPrice` | Two price columns; mapper confusion |
| RowState | Copying values may mark rows `Modified` even when user did not edit | Unintended UPDATEs on every row |
| Persistence | `CommandBuilder` UPDATE still targets `UnitPrice` column (removed/deprecated) | SQL error or writing to wrong column |
| Correctness | Rows loaded before copy: `UnitPrice` DBNull → user never touches price → UPDATE sets NULL | Silent price wipe |
| Caching | Singleton cached `DataTable` with old schema at startup | All instances diverge until restart |

**Fix (priority order):**

1. **Deploy app + DB together** — rename column in C# schema and regenerate commands; do not rely on Fill to merge schemas.
2. After schema change, **`Clear()` + redefine columns** or new `DataTable` before Fill; avoid dual-column hack loops.
3. Set **`PrimaryKey`** before Update (**Program.cs** mistakes table); verify generated SQL against migrated names.
4. Call `AcceptChanges()` after Fill if you need Unchanged baseline before user edits (**RowStateDemo.cs**).
5. For APIs, drop cached `DataTable` — use typed reads so migrations fail fast at compile time.

**Production takeaway:** `Fill` merges **rows**, not **contract ownership** — schema drift + RowState is a common post-migration outage; treat column lists like API version bumps.

---

---

#### Q7. (R) An internal admin API exposes search over an in-memory `DataTable` filled from `SqlDataAdapter`. The controller builds a filter from query string input:

```csharp
public IActionResult FindProducts(string nameContains)
{
    DataTable products = _catalogCache.GetProductsTable(); // shared singleton cache
    string filter = $"ProductName LIKE '%{nameContains}%'";
    DataRow[] matches = products.Select(filter);
    return Ok(matches.Select(r => r["ProductName"]));
}
```

Diagnose security and correctness issues (expression syntax, caching, concurrency). What pattern replaces string-built `DataTable.Select` filters?

---

---

### 06. Transactions & Connection Pooling

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/06. Transactions & Connection Pooling`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public IActionResult FindProducts(string nameContains)
{
    DataTable products = _catalogCache.GetProductsTable(); // shared singleton cache
    string filter = $"ProductName LIKE '%{nameContains}%'";
    DataRow[] matches = products.Select(filter);
    return Ok(matches.Select(r => r["ProductName"]));
}
```

Diagnose security and correctness issues (expression syntax, caching, concurrency). What pattern replaces string-built `DataTable.Select` filters?

**Answer:** `DataTable.Select` uses a **mini expression language** (not SQL), but embedding raw user input enables **filter injection**, **syntax DoS**, and **cross-request mutation** when the cached table is shared.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Unescaped `nameContains` in expression string | `'` breaks out: `' OR ProductName LIKE '%` alters logic; malicious expressions can throw or scan all rows |
| Correctness | `LIKE` in Select syntax differs from SQL; special chars `[]%*` need escaping | Unexpected matches or `SyntaxErrorException` → 500 |
| Concurrency | Singleton cached `DataTable` is **mutable** | One request's edits/deletes affect others; RowState corruption under parallel calls |
| Design | Full-table scan in memory per search | Does not scale; bypasses indexed SQL |
| API | Returns live `DataRow` references | Hidden schema leakage; not a stable DTO contract |

**Fix (priority order):**

1. **Never** interpolate user input into `Select` — use **`DataView.RowFilter`** with escaped literals, or skip expressions entirely.
2. Prefer **LINQ over `AsEnumerable()`** with plain C# string checks, or push search to SQL with **`SqlParameter`** (chapter 03).
3. Cache **immutable snapshots** (`ReadOnly` DTO list) refreshed on timer — not a shared editable `DataTable`.
4. If expressions are required, sanitize with `Regex.Replace` for `[]%*` and double single-quotes per DataExpression rules — still inferior to parameterized SQL.
5. Register cache as **scoped** or reload copy per request if snapshot pattern must stay in-memory.

Example safe in-memory filter:

```csharp
var matches = products.AsEnumerable()
    .Where(r => r.RowState != DataRowState.Deleted)
    .Where(r => (r.Field<string>("ProductName") ?? "")
        .Contains(nameContains, StringComparison.OrdinalIgnoreCase));
```

**Production takeaway:** `DataTable.Select` looks like SQL but is **client-side expression eval** — treat user filters like any dynamic query: **parameterize or use code predicates**, and do not share mutable `DataSet` state across requests.

---

---

### 06. Transactions & Connection Pooling

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/06. Transactions & Connection Pooling`

---

---

#### Q1. (R) A teammate refactors `AccountTransferService` to "reuse one connection per request" and adds a nested transaction for audit logging. Review:

```csharp
public async Task TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    _connection.Open(); // static SqlConnection field, opened once at startup

    using SqlTransaction outer = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
    try
    {
        Debit(fromId, amount); // SqlCommand without cmd.Transaction set

        using SqlTransaction inner = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
        InsertAuditRow(fromId, toId, amount); // cmd.Transaction = inner
        inner.Commit();

        Credit(toId, amount); // cmd.Transaction = outer
        outer.Commit();
    }
    catch
    {
        outer.Rollback();
        throw;
    }
}
```

What breaks at runtime, what happens to the connection pool under load, and what is the prioritized fix?

---

**Answer:**

```csharp
public async Task TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    _connection.Open(); // static SqlConnection field, opened once at startup

    using SqlTransaction outer = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
    try
    {
        Debit(fromId, amount); // SqlCommand without cmd.Transaction set

        using SqlTransaction inner = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
        InsertAuditRow(fromId, toId, amount); // cmd.Transaction = inner
        inner.Commit();

        Credit(toId, amount); // cmd.Transaction = outer
        outer.Commit();
    }
    catch
    {
        outer.Rollback();
        throw;
    }
}
```

What breaks at runtime, what happens to the connection pool under load, and what is the prioritized fix?

**Answer:** This stacks three production failures: a **cached static connection** (not returned to the pool), **un-enlisted commands** that auto-commit outside the outer transaction, and an **invalid nested `BeginTransaction`** on SQL Server without savepoints. Under load the single connection becomes a cross-request bottleneck and pool metrics lie — most threads still open new connections elsewhere while this one never closes.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling / design | Static `SqlConnection` opened at startup | Connection never `Close`/`Dispose` → not returned to pool; violates **ConnectionPoolingDemo** rule "borrow, use, return" |
| Correctness | `Debit` omits `cmd.Transaction = outer` | UPDATE runs in implicit auto-commit — debit persists even if outer rolls back |
| Runtime | Nested `BeginTransaction` on same connection | SQL Server throws *"The connection does not support this method"* (or similar) — no true nested transactions without **`Save`** savepoints |
| Concurrency | One shared connection across requests | Race on shared `SqlConnection` / `SqlTransaction`; undefined behavior under parallel calls |
| Async | Method is `async` but sync ADO.NET inside | Misleading signature; thread blocked during I/O (see chapter 08 preview) |

**Fix (priority order):**

1. **Remove static connection** — `using SqlConnection connection = new SqlConnection(_cs); await connection.OpenAsync(ct);` per operation (or per scoped unit of work).
2. **Enlist every command** — pass `connection` and `transaction` into constructors or set `cmd.Transaction = tx` on **Debit**, **Credit**, and audit (see **AccountTransferService** `ReadBalance` / `ExecuteBalanceUpdate`).
3. **Replace nested `BeginTransaction`** with one transaction + optional **`transaction.Save("Audit")`** / rollback to savepoint, or commit audit in the same transaction before debit/credit.
4. **Use `using` for transaction** — let `Dispose` roll back on failure; explicit `Rollback()` in `catch` is fine but redundant if rethrowing after dispose.
5. Wire **async end-to-end** when the method is async (`ExecuteNonQueryAsync`, pass `ct`).

**Production takeaway:** One open transaction + one pooled connection per request is normal; **one static connection for the app** is a pool leak and a concurrency bug. Nested transactions in ADO.NET mean **savepoints**, not a second `BeginTransaction`.

---

---

#### Q2. (R) Production intermittently reports `SqlException: Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool.` Review this "safe transfer" path adapted from the chapter's debit/credit pattern:

```csharp
public bool TransferWithSideEffects(int fromId, int toId, decimal amount)
{
    SqlConnection connection = new SqlConnection(_connectionString);
    connection.Open();
    SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = ReadBalance(connection, tx, fromId);
    if (balance < amount)
    {
        tx.Rollback();
        return false; // early exit — "handled"
    }

    ExecuteBalanceUpdate(connection, tx, fromId, -amount);
    SendFraudCheckEmail(fromId, toId, amount); // synchronous SMTP — 2–8 seconds
    ExecuteBalanceUpdate(connection, tx, toId, amount);

    tx.Commit();
    connection.Close();
    return true;
}
```

Identify correctness, pooling, and isolation issues. What would you change first for a high-traffic API?

---

**Answer:**

```csharp
public bool TransferWithSideEffects(int fromId, int toId, decimal amount)
{
    SqlConnection connection = new SqlConnection(_connectionString);
    connection.Open();
    SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = ReadBalance(connection, tx, fromId);
    if (balance < amount)
    {
        tx.Rollback();
        return false; // early exit — "handled"
    }

    ExecuteBalanceUpdate(connection, tx, fromId, -amount);
    SendFraudCheckEmail(fromId, toId, amount); // synchronous SMTP — 2–8 seconds
    ExecuteBalanceUpdate(connection, tx, toId, amount);

    tx.Commit();
    connection.Close();
    return true;
}
```

Identify correctness, pooling, and isolation issues. What would you change first for a high-traffic API?

**Answer:** The transaction **holds locks and a pooled connection** while waiting on SMTP, which starves the pool (`Max Pool Size=100` in **ConnectionStrings.AdoNetTutorial**) and increases deadlock risk. Early `return false` after rollback **leaks the connection** because `Close()` is skipped, and there is no `using`/`finally` — the opposite of **TransactionUsingPatternDemo**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pool exhaustion | Long-running txn while connection is open | Each in-flight transfer occupies a pool slot for seconds → `Timeout expired ... obtaining a connection from the pool` |
| Resource leak | `return false` after rollback without `Close`/`Dispose` | Connection not returned to pool — permanent slot loss until GC finalizer |
| Design | Non-transactional I/O inside SQL transaction | Extends lock duration on account rows; other transfers block or deadlock |
| Maintainability | No `using` / `try-finally` | Any exception path may skip `Rollback`/`Close` — relies on finalizer |
| Isolation | `ReadCommitted` + long txn | Writers on same accounts queue; deadlock probability rises |

**Fix (priority order):**

1. **`using` for connection and transaction** — or `try/finally` with `connection?.Dispose()` so every path returns the connection ( **Program.cs** quick reference: Dispose → auto-rollback if not committed).
2. **Shorten transaction scope** — open txn → read balance → debit → credit → commit; **move `SendFraudCheckEmail` after commit** (or outbox/event after success).
3. **Insufficient funds** — `return false` inside `using` block so dispose runs; no manual leak.
4. **Optional** — retry policy for transient deadlocks **outside** a held connection, not inside a multi-second txn.
5. Monitor **pool counters** and average txn duration; tune `Max Pool Size` only after fixing hold time.

**Production takeaway:** A transaction should cover **database work only**. Holding a pooled connection during network I/O is a common cause of pool timeouts — see **ConnectionPoolingDemo** (connections must be Closed/Disposed to return).

---

---

#### Q3. (D) You must transfer funds in SQL **and** publish a message to a separate SQL database (legacy reporting DB). A developer proposes `TransactionScope` with `Required` so both commits roll back together. Another proposes `BeginTransaction` on the primary DB only and "best-effort" reporting insert after commit.

Compare the approaches: distributed transaction pitfalls (MSDTC, cloud PaaS, latency), failure modes, and what you would ship in Azure SQL / containerized .NET 8.

---

**Answer:**

Compare the approaches: distributed transaction pitfalls (MSDTC, cloud PaaS, latency), failure modes, and what you would ship in Azure SQL / containerized .NET 8.

**Answer:** **`TransactionScope` across two SQL databases** enlist a **distributed transaction (MSDTC)** — strong atomicity but fragile in modern cloud and container hosts. **`BeginTransaction` on the ledger only** keeps the money path simple; reporting consistency is handled with **outbox, idempotent consumers, or eventual sync** — the pattern most teams ship on Azure SQL / .NET 8.

- **`TransactionScope` + two databases:** Both connections enlist in DTC; commit is two-phase. **Pitfalls:** MSDTC must be enabled and reachable (often **not** available or discouraged on Linux containers, Azure App Service, serverless); higher latency; harder observability; one participant failure aborts all — including the primary transfer.
- **`BeginTransaction` (primary only) + post-commit insert:** Transfer is ACID on the ledger. If reporting insert fails after commit, **money moved but report missing** — requires reconciliation job or retry queue.
- **Production pattern (.NET 8 / Azure SQL):** Single-db **`SqlTransaction`** (as in **AccountTransferService**) for the transfer; write an **outbox row in the same transaction**; background worker publishes to reporting DB with **idempotency keys**. Avoid cross-db DTC unless compliance mandates it and infra supports MSDTC.
- **When `TransactionScope` is reasonable:** Single database (scope still works but **`connection.BeginTransaction`** is clearer), or explicit DTC with tested Windows/MSDTC topology — rare in greenfield cloud.
- **Failure modes:** DTC — partial enlist / timeout / firewall; best-effort — duplicate or missing reporting rows without idempotency.

**Production takeaway:** Karat tests whether you know **`TransactionScope` ≠ free nested local txn** — multi-resource scopes are **distributed** and often the wrong default in PaaS. Prefer **one SQL transaction + outbox** over two-phase commit across servers.

---

---

#### Q4. (M) Two concurrent transfers between accounts 1 and 2 run at `IsolationLevel.ReadCommitted` (the chapter default in `AccountTransferService.TransferWithSqlTransaction`). Transfer A: 1 → 2 for $500. Transfer B: 2 → 1 for $400. Both read balances, both pass the funds check, both commit.

Explain whether this scenario can lose money or deadlock, how `Snapshot` differs from `ReadCommitted` here, and when you would enable database `ALLOW_SNAPSHOT_ISOLATION` vs bumping to `Serializable`.

---

**Answer:**

Explain whether this scenario can lose money or deadlock, how `Snapshot` differs from `ReadCommitted` here, and when you would enable database `ALLOW_SNAPSHOT_ISOLATION` vs bumping to `Serializable`.

**Answer:** With the chapter's **read-check-update** pattern at **ReadCommitted**, both transfers can pass balance checks on stale reads and **overdraw an account** (lost update / race) — not prevented by ReadCommitted alone. They can also **deadlock** when each holds a lock on one account and waits on the other. **Snapshot** gives statement-level consistent reads without blocking writers (with row versioning); **Serializable** prevents the race but increases blocking.

- **ReadCommitted (default in `IsolationLevelReference`):** No dirty reads, but **non-repeatable reads** allowed — two concurrent txns can each read $1000 on account 1, both debit, one commits overdraft.
- **Deadlock:** A locks account 1 then waits on 2; B locks 2 then waits on 1 → SQL Server picks a **1205 deadlock victim** (see Q5). Ordering accounts by `AccountId` before locking mitigates.
- **Snapshot:** Uses **row versions** in `tempdb`; readers don't block writers. Still need **optimistic concurrency** (`WHERE Balance = @expected` or rowversion) to detect write conflicts on update — snapshot alone doesn't replace application checks.
- **`ALLOW_SNAPSHOT_ISOLATION`:** Database option + `BeginTransaction(IsolationLevel.Snapshot)` — good for **read-heavy reporting** and reducing reader/writer deadlocks; watch **tempdb** sizing.
- **Serializable:** Prevents phantom/non-repeatable issues for the transfer pattern but **holds range locks** — higher blocking; use sparingly for short units of work.
- **Practical fix for transfers:** **`UPDLOCK, ROWLOCK`** hint or single `UPDATE ... WHERE Balance >= @amount` with rows-affected check — keeps txn short as **IsolationLevelReference** recommends.

**Production takeaway:** **ReadCommitted + read-then-write** is a classic production bug; **Snapshot** improves read consistency but **doesn't replace atomic conditional updates**. Prefer **short transactions + consistent lock order + optimistic rowversion** over default Serializable.

---

---

#### Q5. (R) A catch block "matches the tutorial" but omits rollback on some paths. Review:

```csharp
public void Transfer(int fromId, int toId, decimal amount)
{
    using SqlConnection connection = new SqlConnection(_cs);
    connection.Open();
    using SqlTransaction tx = connection.BeginTransaction();

    try
    {
        ExecuteBalanceUpdate(connection, tx, fromId, -amount);
        ExecuteBalanceUpdate(connection, tx, toId, amount);
        tx.Commit();
    }
    catch (SqlException ex) when (ex.Number == 1205) // deadlock victim
    {
        _logger.LogWarning(ex, "Deadlock — retry later");
        // no Rollback()
    }
    catch (Exception ex)
    {
        tx.Rollback();
        throw;
    }
}
```

What state is the connection/transaction in after the deadlock catch, and how should retry + cleanup be structured? Relate to the chapter note that `Dispose` without `Commit` rolls back.

---

**Answer:**

```csharp
public void Transfer(int fromId, int toId, decimal amount)
{
    using SqlConnection connection = new SqlConnection(_cs);
    connection.Open();
    using SqlTransaction tx = connection.BeginTransaction();

    try
    {
        ExecuteBalanceUpdate(connection, tx, fromId, -amount);
        ExecuteBalanceUpdate(connection, tx, toId, amount);
        tx.Commit();
    }
    catch (SqlException ex) when (ex.Number == 1205) // deadlock victim
    {
        _logger.LogWarning(ex, "Deadlock — retry later");
        // no Rollback()
    }
    catch (Exception ex)
    {
        tx.Rollback();
        throw;
    }
}
```

What state is the connection/transaction in after the deadlock catch, and how should retry + cleanup be structured? Relate to the chapter note that `Dispose` without `Commit` rolls back.

**Answer:** After the **1205 catch without rethrow**, execution falls through with a **doomed transaction** still open until `tx.Dispose()` — which **does roll back** per **AccountTransferService** / **TransactionUsingPatternDemo** notes, but the method **returns success implicitly** (void, no retry). Callers believe the transfer may have succeeded; the connection returns to the pool only after `using` ends — acceptable only if no further commands run on that txn.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Deadlock path swallowed — no retry, no failure signal | Caller thinks transfer completed; business event lost |
| Transaction state | Uncommitted txn until `Dispose` | Further commands on same `tx` would fail (*"Transaction has completed"*) |
| Design | Selective rollback only in generic `catch` | Inconsistent with **AccountTransferService** explicit rollback + result tuple |
| Reliability | No idempotency / max retry | Repeated deadlocks under load silently drop transfers |

**Fix (priority order):**

1. **Deadlock catch:** either **`throw`** after log (let caller retry) or **`tx.Rollback()`** + bounded retry with **exponential backoff** on a **new connection/transaction** — never continue using the victim txn.
2. Rely on **`using SqlTransaction`** — `Dispose` without `Commit` **always rolls back** (chapter note); explicit `Rollback()` documents intent but is optional before dispose.
3. Return **`(bool Success, ...)`** or throw **`TransferFailedException`** so callers don't assume commit.
4. **Retry policy:** only for transient errors (1205, -2 timeout); cap retries; **consistent account lock order** to reduce deadlocks (Q4).
5. Do **not** leave empty catch that completes normally — worst of both worlds (silent failure + unclear txn state).

**Production takeaway:** **`Dispose` saves you from a committed-when-you-meant-rollback bug**, but it **does not replace retry semantics or API contracts** — deadlocks must propagate or retry explicitly.

---

---

#### Q6. (P) Chapter 06 previews async transactions (`AsyncTransactionPreview`); chapter 08 covers full async ADO.NET. A service method is marked `async` but implemented like this:

```csharp
public async Task<bool> TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    using SqlConnection connection = new SqlConnection(_cs);
    await connection.OpenAsync(ct);
    using SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = (decimal)(await new SqlCommand(
        "SELECT Balance FROM dbo.Accounts WHERE AccountId = @id", connection, tx)
    {
        Parameters = { { "@id", fromId } }
    }.ExecuteScalarAsync()); // no ct passed

    ExecuteBalanceUpdate(connection, tx, fromId, -amount); // sync ExecuteNonQuery
    ExecuteBalanceUpdate(connection, tx, toId, amount);    // sync ExecuteNonQuery

    tx.Commit();
    return true;
}
```

What scalability and cancellation gaps remain, and what is the correct async transaction pattern end-to-end?

---

**Answer:**

```csharp
public async Task<bool> TransferAsync(int fromId, int toId, decimal amount, CancellationToken ct)
{
    using SqlConnection connection = new SqlConnection(_cs);
    await connection.OpenAsync(ct);
    using SqlTransaction tx = connection.BeginTransaction(IsolationLevel.ReadCommitted);

    decimal balance = (decimal)(await new SqlCommand(
        "SELECT Balance FROM dbo.Accounts WHERE AccountId = @id", connection, tx)
    {
        Parameters = { { "@id", fromId } }
    }.ExecuteScalarAsync()); // no ct passed

    ExecuteBalanceUpdate(connection, tx, fromId, -amount); // sync ExecuteNonQuery
    ExecuteBalanceUpdate(connection, tx, toId, amount);    // sync ExecuteNonQuery

    tx.Commit();
    return true;
}
```

What scalability and cancellation gaps remain, and what is the correct async transaction pattern end-to-end?

**Answer:** The method **async only at the door** — sync **`ExecuteNonQuery`** blocks thread-pool threads during writes, and **`ExecuteScalarAsync()` without `ct`** ignores client abort. **`BeginTransaction()`** remains synchronous (acceptable); everything enlisted in the txn should use **`*Async` + `CancellationToken`** and stay **short**, matching **AsyncTransactionPreview** forward reference to chapter 08.

- **Pass `ct` to every async execute** — `ExecuteScalarAsync(ct)`, `ExecuteNonQueryAsync(ct)`; cancellation aborts waiting I/O and should trigger dispose → rollback.
- **Replace sync updates** with async counterparts inside the same `SqlTransaction` — enlistment rules unchanged (**AccountTransferService** Section 6).
- **No sync-over-async** inside ASP.NET request — mixed pattern still starves the pool under concurrent load.
- **Structure:** `await OpenAsync` → `using var tx = BeginTransaction()` → await reads/writes → `Commit()`; on exception, dispose tx (rollback) and optionally rethrow.
- **`BeginTransactionAsync`** exists on some providers; **`Microsoft.Data.SqlClient`** — use sync `BeginTransaction` after `OpenAsync` (documented pattern).
- **Business rules unchanged** — insufficient funds check before debit; keep txn scope minimal (Q2).

**Production takeaway:** Async transactions mean **async I/O on every command in the unit of work**, not `async` on the method signature alone — see **AsyncTransactionPreview** and chapter 08 for full coverage.

---

---

#### Q7. (R) After a deploy, the API exhausts `Max Pool Size=100` within minutes. Review the repository registered as **Singleton**:

```csharp
public sealed class AccountRepository
{
    private readonly SqlConnection _connection = new SqlConnection(_cs);

    public decimal GetBalance(int accountId)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // SELECT ... — no transaction
        return balance;
    }

    public void UpdateBalance(int accountId, decimal delta)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // UPDATE ... — auto-commit each call
    }
}
```

Two `AccountTransferService` instances (scoped) call this singleton concurrently. Explain pool starvation vs "only one connection," and the fix aligned with `ConnectionPoolingDemo` rules.

---

### 07. Stored Procedures & Output Parameters

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/07. Stored Procedures & Output Parameters`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public sealed class AccountRepository
{
    private readonly SqlConnection _connection = new SqlConnection(_cs);

    public decimal GetBalance(int accountId)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // SELECT ... — no transaction
        return balance;
    }

    public void UpdateBalance(int accountId, decimal delta)
    {
        if (_connection.State != ConnectionState.Open)
            _connection.Open();
        // UPDATE ... — auto-commit each call
    }
}
```

Two `AccountTransferService` instances (scoped) call this singleton concurrently. Explain pool starvation vs "only one connection," and the fix aligned with `ConnectionPoolingDemo` rules.

**Answer:** A **singleton holding one open `SqlConnection`** removes that physical connection from the pool **for the app lifetime** and makes all requests **share one non-thread-safe connection** — concurrent scoped services corrupt reader/state and serialize DB access. Pool exhaustion still happens elsewhere because **other code paths correctly open pooled connections** that never return when leaked; this pattern is both a **logical leak** and a **concurrency defect**.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Pooling | Long-lived open connection on singleton | One slot permanently checked out; not returned until app shutdown — violates **ConnectionPoolingDemo** "Do not cache SqlConnection in static fields" |
| Concurrency | `SqlConnection` not thread-safe | Interleaved `GetBalance`/`UpdateBalance` from two scoped transfers → mixed results, exceptions |
| Transactions | Auto-commit per call | **Debit + credit not atomic** — crash between calls loses money; can't assign shared `SqlTransaction` safely across callers |
| DI lifetime | Singleton repo + scoped service | Classic captive dependency — stateful infra in wrong lifetime |

**Fix (priority order):**

1. **Register repository as scoped** (or transient) — **no instance fields holding `SqlConnection`**.
2. **Per-operation pattern:** `using var connection = new SqlConnection(_cs); await connection.OpenAsync(ct);` — matches **AccountTransferService** and **ConnectionPoolingDemo** open/close cycle.
3. **Transfers:** one connection, one **`BeginTransaction`**, enlist all commands — not two singleton auto-commit updates.
4. **Optional factory:** `IDbConnection` factory registered singleton creating **new** connections per use — factory is singleton, connections are not.
5. Diagnose leaks with pool timeout errors + ensure **`using`** on every `SqlConnection` (Q2 early return lesson).

**Production takeaway:** Pool exhaustion is often **leaked or hoarded connections**, not "need Max Pool Size=500." **Borrow → use → dispose** every time; singleton + open connection is a dual bug (pool + thread safety).

---

### 07. Stored Procedures & Output Parameters

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/07. Stored Procedures & Output Parameters`

---

---

#### Q1. (R) A teammate "fixes" intermittent procedure-not-found errors by copying an SSMS snippet into the repository. Review:

```csharp
public int RunUpdateProductPrice(SqlConnection connection, int productId, decimal newPrice)
{
    using SqlCommand command = new SqlCommand("EXEC dbo.usp_UpdateProductPrice", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
    command.Parameters.Add(new SqlParameter("@NewPrice", SqlDbType.Decimal)
    {
        Value = newPrice,
        Precision = 10,
        Scale = 2
    });

    return command.ExecuteNonQuery();
}
```

It passes locally against `(localdb)\MSSQLLocalDB` but fails in staging with `Could not find stored procedure 'EXEC dbo.usp_UpdateProductPrice'`. What is wrong, and what is the correct `CommandText` / `CommandType` pairing per this chapter's pattern in `StoredProcedureCommandBasics.cs`?

---

**Answer:**

```csharp
public int RunUpdateProductPrice(SqlConnection connection, int productId, decimal newPrice)
{
    using SqlCommand command = new SqlCommand("EXEC dbo.usp_UpdateProductPrice", connection);
    command.CommandType = CommandType.StoredProcedure;
    // ...
}
```

**Answer:** With `CommandType.StoredProcedure`, `CommandText` must be the **procedure name only** — schema-qualified, no `EXEC`, no parentheses. SqlClient sends an RPC call; embedding `EXEC` makes SQL Server look for a procedure literally named `EXEC dbo.usp_UpdateProductPrice`, which does not exist.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `CommandText = "EXEC dbo.usp_UpdateProductPrice"` with `StoredProcedure` | `SqlException`: could not find stored procedure |
| Convention | SSMS batch syntax copied into ADO.NET | Works in SSMS, fails in client RPC path |
| Maintainability | Unqualified name would also risk wrong-object resolution in multi-schema databases | Silent call to unintended proc in some environments |

**Fix (priority order):**

1. Set `CommandText = "dbo.usp_UpdateProductPrice"` — matches `StoredProcedureCommandBasics.CreateUpdatePriceCommand` in this chapter.
2. Keep `CommandType = CommandType.StoredProcedure` (required; default `Text` would treat the name as invalid SQL).
3. Do **not** append `()` or parameter placeholders to `CommandText`; bind via `SqlParameter` collection instead.
4. Use schema-qualified names (`dbo.usp_...`) so resolution does not depend on the caller's default schema.

**Production takeaway:** Treat `CommandType.StoredProcedure` + bare procedure name as the standard client contract. Reserve `EXEC ...` strings for `CommandType.Text` ad-hoc scripts — not for production procedure calls. See ADO.NET ch 03 Q5 for the inverse mistake (`EXEC` with `Text` vs RPC with `StoredProcedure`).

---

---

#### Q2. (R) After a DBA widens `usp_GetProductName` to `NVARCHAR(200)`, production logs show truncated product names and occasional `SqlException` about string output size. Review the client:

```csharp
public string? GetProductName(SqlConnection connection, int productId)
{
    using SqlCommand command = new SqlCommand("dbo.usp_GetProductName", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });

    var nameParameter = new SqlParameter("@ProductName", SqlDbType.NVarChar)
    {
        Direction = ParameterDirection.Output
        // Size not set — "works" for short names in QA
    };
    command.Parameters.Add(nameParameter);

    command.ExecuteNonQuery();
    return nameParameter.Value == DBNull.Value ? null : (string)nameParameter.Value;
}
```

What fails at runtime for string `OUTPUT` parameters, and how must `Size` relate to the T-SQL declaration?

---

**Answer:**

```csharp
var nameParameter = new SqlParameter("@ProductName", SqlDbType.NVarChar)
{
    Direction = ParameterDirection.Output
    // Size not set
};
```

**Answer:** SqlClient must pre-allocate a buffer for string `OUTPUT` parameters. Omitting `Size` on `NVarChar`/`VarChar` output params causes runtime errors or silent truncation when the server writes more characters than the client buffer allows. Client `Size` must be **at least** the T-SQL parameter length.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `Size` omitted on string `OUTPUT` | `SqlException` ("String or binary data would be truncated") or truncated `.Value` |
| Contract drift | Client still sized for old `NVARCHAR(100)` while server is `NVARCHAR(200)` | Long names cut off in prod after DBA change |
| Correctness | Assumes "short names in QA" means Size is optional | Failures only appear with real catalog data |

**Fix (priority order):**

1. Declare explicit size matching (or exceeding) T-SQL: `new SqlParameter("@ProductName", SqlDbType.NVarChar, 200) { Direction = Output, Size = 200 }` — same pattern as `OutputParameterDemo.GetProductName`.
2. Update client and proc **together** in one release, or widen client buffer first (200 accepts 100-byte server output safely).
3. Add integration test with a 150+ character product name to catch truncation before deploy.
4. For `InputOutput` string params, the same `Size` rule applies on both directions.

**Production takeaway:** Integer/decimal `OUTPUT` params do not need `Size`; **all string output types do**. Treat `Size` as part of the procedure contract, versioned with the proc definition.

---

---

#### Q3. (M) `dbo.usp_SearchProducts` returns a result set **and** sets `@TotalCount INT OUTPUT`. Two teammates ship different callers:

```csharp
// Path A — "we only need the count in the OUTPUT param"
command.ExecuteNonQuery();
int total = (int)totalCountParam.Value;

// Path B — "we need the rows"
using SqlDataReader reader = command.ExecuteReader();
while (reader.Read()) { /* map ProductDto */ }
int total = (int)totalCountParam.Value;
```

Path A reports `@TotalCount` correctly but never surfaces product rows. Path B throws or returns `0` for `@TotalCount` depending on timing. Explain when to use `ExecuteNonQuery`, `ExecuteReader`, or `ExecuteScalar` for procedures that mix result sets and output parameters, and when `@TotalCount.Value` is valid to read.

---

**Answer:**

```csharp
// Path A
command.ExecuteNonQuery();
int total = (int)totalCountParam.Value;

// Path B
using SqlDataReader reader = command.ExecuteReader();
while (reader.Read()) { /* map rows */ }
int total = (int)totalCountParam.Value;
```

**Answer:** Use **`ExecuteReader`** when the procedure returns rows you need. `ExecuteNonQuery` discards result sets but still runs the proc — so `@TotalCount` may populate while product rows are never read. Output/return parameters are only guaranteed after the command **fully completes**, which for `ExecuteReader` means consuming all result sets and closing/disposing the reader.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness (Path A) | `ExecuteNonQuery` on a proc that `SELECT`s rows | `@TotalCount` may work; product data silently dropped |
| Correctness (Path B) | Reading `@TotalCount.Value` before reader is closed | Stale `0`, `DBNull`, or race depending on provider timing |
| Design | Treating `ExecuteNonQuery` return as business row count when `SET NOCOUNT ON` | Chapter notes `-1` rows affected — not a substitute for result sets |

**Fix (priority order):**

1. **Need rows + output:** `ExecuteReader` → read all rows on each result set (`NextResult` if multiple) → dispose reader → read `@TotalCount.Value`.
2. **Need only output, no rows:** refactor proc to remove the `SELECT` (or use `SET NOCOUNT ON` and no result sets) → `ExecuteNonQuery` is appropriate.
3. **Single scalar only:** if proc returns one cell and no output params, `ExecuteScalar` works; if both scalar-like `SELECT` and `OUTPUT` exist, prefer reader pattern.
4. Never read `SqlParameter.Value` for `Output`/`ReturnValue` until execute method completes (same rule as `ReadAfterExecuteDemo` section 6).

**Production takeaway:** Pick execute method by **what the proc returns**, not convenience. Mixed result-set + `OUTPUT` procs are common for paged search — always drain the reader first.

---

---

#### Q4. (R) A dashboard stored procedure returns two SELECT result sets plus an `@ErrorMessage NVARCHAR(500) OUTPUT`. Review:

```csharp
using SqlDataReader reader = command.ExecuteReader();

var summaryRows = new List<SummaryRow>();
while (reader.Read())
    summaryRows.Add(MapSummary(reader));

int errCode = (int)errorCodeParam.Value;  // read OUTPUT early

while (reader.NextResult())
{
    while (reader.Read())
        detailRows.Add(MapDetail(reader));
}

string? message = errorCodeParam.Value == DBNull.Value ? null : (string)errorMessageParam.Value;
```

QA passes when the proc succeeds; staging intermittently shows `InvalidCastException` or stale `@ErrorMessage`. Diagnose the ordering bug and show the correct read sequence for multiple result sets **and** output parameters.

---

**Answer:**

```csharp
using SqlDataReader reader = command.ExecuteReader();
// ... read summary ...
int errCode = (int)errorCodeParam.Value;  // read OUTPUT early
while (reader.NextResult()) { /* details */ }
string? message = ... errorMessageParam.Value;
```

**Answer:** Output and return parameters are not reliably readable while a `SqlDataReader` is open. The reader must fully consume **every** result set (or be disposed) before reading `OUTPUT`/`ReturnValue` `.Value`. Reading `@ErrorCode` mid-stream causes stale or invalid values.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `errorCodeParam.Value` read before reader closed | `InvalidCastException`, `0`, or stale data |
| Correctness | `@ErrorMessage` read after partial `NextResult` traversal | Wrong message when proc sets OUTPUT on failure path |
| Async variant | Same bug with `await ExecuteReaderAsync` without full consumption | Intermittent failures under load |

**Fix (priority order):**

1. Read all result sets sequentially:

```csharp
using SqlDataReader reader = command.ExecuteReader();
var summary = ReadAll(summaryMapper, reader);
if (reader.NextResult())
    details = ReadAll(detailMapper, reader);
// reader disposed here
int errCode = errorCodeParam.Value == DBNull.Value ? 0 : (int)errorCodeParam.Value;
string? message = errorMessageParam.Value == DBNull.Value ? null : (string)errorMessageParam.Value;
```

2. Do not interleave `param.Value` reads between `Read()` / `NextResult()` loops.
3. Check `DBNull.Value` before casting string/int outputs (`OutputParameterDemo` pattern).
4. If the proc can return early with no second result set, guard `NextResult()` with its boolean return.

**Production takeaway:** Order is always **inputs → execute → drain all result sets → read output/return params**. This mirrors Dapper `QueryMultiple` ordering rules (see Dapper ch 03 Q3).

---

---

#### Q5. (R) An admin API lets power users pick which reporting stored procedure to run. Review:

```csharp
public IReadOnlyList<ReportRow> RunReport(string procedureName, int tenantId)
{
    using SqlConnection connection = new SqlConnection(_connectionString);
    using SqlCommand command = new SqlCommand(procedureName, connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@TenantId", SqlDbType.Int) { Value = tenantId });

    using SqlDataReader reader = command.ExecuteReader();
    return MapRows(reader);
}
```

`procedureName` comes from a query string. Parameters are bound safely — why is this still a critical vulnerability, and what production pattern replaces dynamic `CommandText` for procedure selection?

---

**Answer:**

```csharp
command.CommandText = procedureName;  // from query string
command.CommandType = CommandType.StoredProcedure;
command.Parameters.Add(new SqlParameter("@TenantId", SqlDbType.Int) { Value = tenantId });
```

**Answer:** Parameter binding prevents injection **inside** the batch, but **`CommandText` itself is not parameterized**. An attacker can supply `procedureName` values that invoke privileged procs (`dbo.sp_executesql` wrappers, `xp_cmdshell` if exposed, or cross-tenant report procs) if the login has `EXEC` rights. This is identifier injection, not value injection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | User-controlled procedure name | Unauthorized proc execution within DB permissions |
| Security | No schema qualification + dynamic name | May resolve to attacker-chosen object in search path |
| Design | "Parameters are safe" misconception | Values bound; proc **identity** is not |

**Fix (priority order):**

1. **Whitelist** allowed report keys server-side: `Dictionary<string, string>` mapping `"sales" → "dbo.usp_Report_Sales"` — never pass raw client strings to `CommandText`.
2. Grant `EXEC` only on approved procs (chapter section 7 — EXEC-only security model); deny direct table access.
3. Validate tenant in proc logic (or RLS) — `@TenantId` alone does not stop calling `dbo.usp_Report_AllTenants` if the name is attacker-controlled.
4. Log resolved procedure name from whitelist, not client input.

**Production takeaway:** Dynamic SQL safety rules apply to **procedure names** too. Whitelist + least-privilege `EXEC` grants; see `StoredProcedureVsInlineSql.PrintDecisionGuide` — never embed user input in `CommandText`.

---

---

#### Q6. (D) Your team treats stored procedures as a versioned contract between the DBA and .NET services (`usp_InsertProduct` with `@NewProductId OUTPUT` today). Next sprint the DBA adds a required `@CreatedBy NVARCHAR(128)` with no default, and renames `@NewProductId` to `@ProductIdOut` in one environment before all app servers redeploy. What breaks at runtime, how do rolling deploys and blue/green make this worse, and what contract practices (naming, defaults, deployment order) would you enforce?

---

**Answer:**

_Answer not found._

---

#### Q7. (R) A "try insert" path mirrors `ReturnValueDemo.TryGetProductName` but the author skips null checks "because OUTPUT is always set." Review:

```csharp
public (int NewId, int Status) TryInsertProduct(string name, decimal price)
{
    using SqlCommand command = new SqlCommand("dbo.usp_TryInsertProduct", connection);
    command.CommandType = CommandType.StoredProcedure;

    command.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 100) { Value = name });
    command.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Value = price, Precision = 10, Scale = 2 });

    var newIdParam = new SqlParameter("@NewProductId", SqlDbType.Int) { Direction = ParameterDirection.Output };
    var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
    command.Parameters.Add(newIdParam);
    command.Parameters.Add(returnParam);

    command.ExecuteNonQuery();

    int newId = (int)newIdParam.Value;
    int status = (int)returnParam.Value;
    return (newId, status);
}
```

When duplicate names violate a unique constraint, the proc sets `@NewProductId = NULL`, `RETURN 2`, and does not insert. What exception or wrong data reaches the caller, and how should the method handle `DBNull.Value` vs `RETURN` status (see `OutputParameterDemo.InsertProduct` and `ReturnValueDemo.TryGetProductName`)?

---

**Answer:**

```csharp
int newId = (int)newIdParam.Value;
int status = (int)returnParam.Value;
return (newId, status);
```

**Answer:** When the proc sets `@NewProductId = NULL` on failure, `(int)newIdParam.Value` throws **`InvalidCastException`** because `DBNull.Value` is not convertible to `int`. The `RETURN 2` status is only reachable if you cast return value **after** safely handling nullable output — the pattern in `ReturnValueDemo.TryGetProductName` checks `DBNull` and uses `RETURN` as the authoritative status.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | Direct `(int)` cast on `OUTPUT` without `DBNull` check | `InvalidCastException` on duplicate / validation failure |
| Correctness | Ignores `RETURN` code when output is null | Caller cannot distinguish "id 0" from failure without exception |
| Design | Assumes success path only | Violates try-insert contract (`RETURN 2`, null identity) |

**Fix (priority order):**

1. Read status first: `int status = Convert.ToInt32(returnParam.Value);`
2. Guard output: `int newId = newIdParam.Value == DBNull.Value ? 0 : Convert.ToInt32(newIdParam.Value);`
3. Branch on **`RETURN`** (or status enum), not `newId == 0` alone — identity `0` is ambiguous vs unset.
4. Return `(NewId: null, Status: status)` or throw domain exception based on status — mirror `TryGetProductName`'s `(string? Name, int StatusCode)` tuple.

```csharp
command.ExecuteNonQuery();
int status = Convert.ToInt32(returnParam.Value);
int? newId = newIdParam.Value == DBNull.Value ? null : Convert.ToInt32(newIdParam.Value);
return (newId, status);
```

**Production takeaway:** Treat all `OUTPUT` params as nullable at the client unless the proc documents otherwise. Pair `RETURN` codes with null outputs for try-pattern procs — same semantics as `usp_TryGetProductName` in `LocalDbConnection.cs`.

---

---

#### Q8. (P) An ASP.NET Core endpoint wraps `usp_InsertProduct` with output identity. Review the async repository under load:

```csharp
public int InsertProductAsync(string productName, decimal unitPrice)
{
    using SqlConnection connection = new SqlConnection(_connectionString);
    using SqlCommand command = new SqlCommand("dbo.usp_InsertProduct", connection);
    command.CommandType = CommandType.StoredProcedure;
    // ... input + @NewProductId OUTPUT params ...

    connection.Open();
    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();

    return (int)newIdParameter.Value;
}
```

Registered as scoped and called from `Task`-returning controllers. What thread-pool and correctness issues appear in production, and how should this method be rewritten (including when to read `Output`/`ReturnValue` and how to pass cancellation)?

---

### 08. Async ADO.NET

# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/08. Async ADO.NET`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

**Answer:**

```csharp
public int InsertProductAsync(string productName, decimal unitPrice)
{
    // ...
    command.ExecuteNonQueryAsync().GetAwaiter().GetResult();
    return (int)newIdParameter.Value;
}
```

**Answer:** Sync-over-async via `GetAwaiter().GetResult()` on ASP.NET Core request threads causes **thread-pool starvation** and can deadlock under load when the continuation waits for a blocked thread. The method name promises async but blocks. Output params are read correctly **only after** the async execute completes — but the blocking wrapper negates scalability benefits from chapter 08's async ADO.NET path.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Async | `.GetAwaiter().GetResult()` in request path | Latency spikes, thread-pool exhaustion, potential deadlocks |
| API honesty | Method suffixed `Async` but synchronous | Misleading call sites; hidden blocking |
| Missing | No `CancellationToken` forwarded to `OpenAsync` / `ExecuteNonQueryAsync` | Client disconnects do not cancel SQL work |
| Correctness | `(int)newIdParameter.Value` without `DBNull` check | Same failure as Q7 on constraint violations |

**Fix (priority order):**

1. Rewrite as true async end-to-end:

```csharp
public async Task<int> InsertProductAsync(
    string productName,
    decimal unitPrice,
    CancellationToken cancellationToken = default)
{
    await using SqlConnection connection = new SqlConnection(_connectionString);
    await using SqlCommand command = BuildInsertCommand(connection, productName, unitPrice);

    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

    object raw = newIdParameter.Value;
    return raw == DBNull.Value ? 0 : Convert.ToInt32(raw);
}
```

2. Pass `HttpContext.RequestAborted` (or `CancellationToken`) from controller → repository → `OpenAsync` / `ExecuteNonQueryAsync`.
3. Read `@NewProductId` **after** `await ExecuteNonQueryAsync` completes — same rule as sync; preview in `AsyncPreview.cs`.
4. Use `ConfigureAwait(false)` in library/repository code unless you need request context after await.

**Production takeaway:** Async stored procedure calls follow the same parameter rules as sync; the production win is **non-blocking I/O** through the stack. Never block on `ExecuteNonQueryAsync`; full patterns in ADO.NET ch 08 Async ADO.NET.

---

### 08. Async ADO.NET

# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/08. Async ADO.NET`

---

---

#### Q1. (R) A Minimal API endpoint "fixes" async by blocking on the repository. Review:

```csharp
app.MapGet("/products/count", () =>
{
    string cs = builder.Configuration.GetConnectionString("AdoNetTutorial")!;
    int count = ProductRepository.GetProductCountAsync(cs).Result;
    return Results.Ok(count);
});
```

`ProductRepository.GetProductCountAsync` uses `OpenAsync` and `ExecuteScalarAsync` with `ConfigureAwait(false)` (see **Repositories/ProductRepository.cs**). Under load in ASP.NET Core, what breaks, and how do you fix it end-to-end?

---

**Answer:**

```csharp
app.MapGet("/products/count", () =>
{
    string cs = builder.Configuration.GetConnectionString("AdoNetTutorial")!;
    int count = ProductRepository.GetProductCountAsync(cs).Result;
    return Results.Ok(count);
});
```

`ProductRepository.GetProductCountAsync` uses `OpenAsync` and `ExecuteScalarAsync` with `ConfigureAwait(false)` (see **Repositories/ProductRepository.cs**). Under load in ASP.NET Core, what breaks, and how do you fix it end-to-end?

**Answer:** Blocking `.Result` on async ADO.NET turns non-blocking I/O back into **sync-over-async**: a thread-pool thread sits idle for the entire SQL round-trip while `OpenAsync`/`ExecuteScalarAsync` wait on the network. Under load this inflates latency, exhausts the thread pool, and can still deadlock when a captured context waits on itself. Fix by making the route `async`, `await` the repository, and thread `HttpContext.RequestAborted` through as `CancellationToken`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `.Result` on `GetProductCountAsync()` | Blocks a thread for DB I/O; defeats purpose of async ADO.NET |
| Runtime/async | Sync lambda `MapGet` delegate | No cooperative cancellation; scales poorly vs `await` |
| Correctness | No `CancellationToken` passed | Client disconnect/timeouts cannot cancel `OpenAsync`/`ExecuteScalarAsync` |
| Design | Endpoints block while library correctly uses `ConfigureAwait(false)` | Repository is fine; caller undoes the benefit |

**Fix (priority order):**

1. Change the route to `async` and replace `.Result` with `await ProductRepository.GetProductCountAsync(cs, ctx.RequestAborted)`.
2. Extend **ProductRepository** callers (already accept optional token) to pass `RequestAborted` on every `OpenAsync`/`ExecuteScalarAsync` call.
3. Keep `ConfigureAwait(false)` **inside** the repository only — not in the endpoint.
4. Load-test before/after; watch thread-pool queue length and request latency under concurrent `/products/count` hits.

**Production takeaway:** Async ADO.NET only helps when the **call stack is async end-to-end** — see **Models/ProductRow.cs** rule: async controller → async service → async ADO.NET. See C# Module 06 — sync-over-async / `.Result` gotcha.

---

---

#### Q2. (R) A report endpoint streams products to the client. A teammate copied the **ProductStream** preview pattern but left the connection open on the caller:

```csharp
app.MapGet("/products/stream", async (HttpContext ctx) =>
{
    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    await foreach (ProductRow row in ProductStream.ReadProductsAsync(reader))
    {
        await ctx.Response.WriteAsJsonAsync(row);
    }
});
```

`ProductStream.ReadProductsAsync` passes a `CancellationToken` with `[EnumeratorCancellation]`. The endpoint does not pass `ctx.RequestAborted`, and a slow client disconnects mid-stream. Diagnose what keeps running after the client is gone and what you would change (token wiring + disposal).

---

**Answer:**

```csharp
app.MapGet("/products/stream", async (HttpContext ctx) =>
{
    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    await foreach (ProductRow row in ProductStream.ReadProductsAsync(reader))
    {
        await ctx.Response.WriteAsJsonAsync(row);
    }
});
```

`ProductStream.ReadProductsAsync` passes a `CancellationToken` with `[EnumeratorCancellation]`. The endpoint does not pass `ctx.RequestAborted`, and a slow client disconnects mid-stream. Diagnose what keeps running after the client is gone and what you would change (token wiring + disposal).

**Answer:** Without linking `HttpContext.RequestAborted`, `ReadAsync` and the enumeration continue after the client disconnects — SQL Server keeps sending rows, the connection stays busy, and you waste pool slots until the query finishes or times out. Wire cancellation through the stream and ensure the `await using` chain disposes reader/connection when `OperationCanceledException` fires.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness/runtime | `ReadProductsAsync(reader)` — default `CancellationToken` | Disconnect does not cancel `ReadAsync`; DB work continues |
| Resource lifetime | Long-lived open reader + connection during slow response write | Holds SQL connection from pool for entire stream |
| HTTP | No abort handling on `WriteAsJsonAsync` | Server may throw mid-write without cooperative cancel on reader |
| Design | Token parameter exists on **ProductStream** but caller ignores it | `[EnumeratorCancellation]` never receives request abort |

**Fix (priority order):**

1. Pass the abort token: `await foreach (... in ProductStream.ReadProductsAsync(reader, ctx.RequestAborted))`.
2. Wrap the loop in `try/catch` for `OperationCanceledException` when the client drops — log at debug, do not treat as 500.
3. Keep `await using` on connection, command, and reader so `DisposeAsync` runs on cancel (same pattern as **Program.cs** `DemonstrateAwaitUsingAsync`).
4. For very large exports, consider `CommandBehavior.SequentialAccess`, chunked HTTP (NDJSON lines), and command timeout aligned with proxy limits.
5. Optionally use `Response.RegisterForDisposeAsync` if ownership moves into middleware — but `await using` in the handler is sufficient here.

**Production takeaway:** Streaming ADO.NET is a **lifetime problem** — the reader owns the connection until disposed; cancellation must flow from HTTP → `IAsyncEnumerable` → every `ReadAsync`. See **Services/ProductStream.cs** and **Program.cs** Section 13 preview.

---

---

#### Q3. (R) With `MultipleActiveResultSets=True` on the connection string, a service tries to update stock while a reader is still open:

```csharp
public async Task RefreshCatalogAsync(string connectionString)
{
    await using SqlConnection conn = new SqlConnection(connectionString); // MARS enabled in cs
    await conn.OpenAsync();

    await using SqlCommand select = conn.CreateCommand();
    select.CommandText = "SELECT ProductId, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await select.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        int stock = reader.GetInt32(1);

        if (stock == 0)
        {
            await using SqlCommand update = conn.CreateCommand();
            update.CommandText = "UPDATE dbo.Products SET Stock = 10 WHERE ProductId = @Id;";
            update.Parameters.Add(new SqlParameter("@Id", id));
            await update.ExecuteNonQueryAsync(); // second active command on same connection
        }
    }
}
```

What can go wrong with MARS + interleaved reader and command lifetime, and what pattern would you use instead in production?

---

**Answer:**

```csharp
public async Task RefreshCatalogAsync(string connectionString)
{
    await using SqlConnection conn = new SqlConnection(connectionString); // MARS enabled in cs
    await conn.OpenAsync();

    await using SqlCommand select = conn.CreateCommand();
    select.CommandText = "SELECT ProductId, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await select.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        int stock = reader.GetInt32(1);

        if (stock == 0)
        {
            await using SqlCommand update = conn.CreateCommand();
            update.CommandText = "UPDATE dbo.Products SET Stock = 10 WHERE ProductId = @Id;";
            update.Parameters.Add(new SqlParameter("@Id", id));
            await update.ExecuteNonQueryAsync(); // second active command on same connection
        }
    }
}
```

What can go wrong with MARS + interleaved reader and command lifetime, and what pattern would you use instead in production?

**Answer:** MARS allows a second command on the same connection while a reader is open, but interleaving read and write on one connection is fragile: ordering bugs, long-held locks, unexpected transaction behavior, and hard-to-debug "There is already an open DataReader" errors when MARS is off in another environment. Production code should finish (or dispose) the reader first, or use separate connections / set-based SQL.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Per-row `UPDATE` inside `ReadAsync` loop | N round-trips; race with other writers; slow catalog refresh |
| Resource lifetime | Reader open for entire loop while commands interleave | Extends lock duration on server; connection busy for minutes on large tables |
| Environment | Code assumes MARS in connection string | Same code throws in staging/prod if MARS omitted — "open DataReader" |
| Design | Row-at-a-time imperative update | Does not scale; bypasses set-based `UPDATE ... WHERE Stock = 0` |

**Fix (priority order):**

1. Prefer **set-based SQL**: single `UPDATE dbo.Products SET Stock = 10 WHERE Stock = 0` — one round-trip, no MARS needed.
2. If you must read then write, **buffer keys** from the reader into a `List<int>`, dispose the reader, then batch updates (or use a second connection for writes).
3. Do not rely on MARS as a default architecture — treat it as a narrow escape hatch; document if truly required.
4. Pass `CancellationToken` on `OpenAsync`, `ReadAsync`, and `ExecuteNonQueryAsync` for long catalogs.
5. Wrap multi-statement work in an explicit transaction when atomicity matters.

**Production takeaway:** Async does not fix **connection semantics** — one connection, one active reader unless MARS is explicitly enabled and understood. Prefer set-based commands like **ProductRepository.InsertProductAsync** over interleaved reader/command patterns.

---

---

#### Q4. (P) Your shared data-access library (same style as **ProductRepository**) is consumed by ASP.NET Core APIs and a WinForms desktop app. A junior adds `ConfigureAwait(false)` to every await in the **API controllers** "because the ADO.NET chapter says library code should." Another dev removes `ConfigureAwait(false)` from **ProductRepository** "because ASP.NET doesn't need it." Who is right in each case, and what is the production rule?

---

**Answer:**

**Answer:** The repository author was closer to correct. **`ConfigureAwait(false)` belongs in library/repository code** that has no dependency on returning to a specific context; **application code** (Minimal API endpoints, WinForms event handlers) normally omits it so continuations can resume on the request or UI context when one exists.

- **Repository / ProductRepository:** Keep `ConfigureAwait(false)` on every `await` after `OpenAsync`, `ExecuteScalarAsync`, and `ExecuteNonQueryAsync` — as in **Repositories/ProductRepository.cs**. The library must not assume ASP.NET or WinForms context; false avoids unnecessary marshaling when a `SynchronizationContext` is present (legacy ASP.NET, UI apps).
- **ASP.NET Core Minimal API / controllers:** Do **not** sprinkle `ConfigureAwait(false)` — there is usually no captured context in modern ASP.NET Core, and you may need `HttpContext` after `await` without extra plumbing. Adding it everywhere in app code is noise and can confuse reviewers.
- **WinForms desktop:** Omit `ConfigureAwait(false)` in UI event handlers when you must touch controls after `await`; use it in downstream library calls (the repository already does).
- **Console apps (this chapter's demos):** No `SynchronizationContext` — behavior matches with or without `ConfigureAwait(false)`; the repository pattern is shown for copy-paste into services.

**Production takeaway:** Rule of thumb — **`ConfigureAwait(false)` in reusable data-access libraries; default `await` in app/host code.** See **Program.cs** Section 12 and **ProductRepository.cs** Section 3 comments.

---

---

#### Q5. (R) A legacy sync helper was merged into an async export job:

```csharp
public async Task ExportProductsAsync(string connectionString, Stream output)
{
    await using SqlConnection conn = new SqlConnection(connectionString);
    conn.Open(); // sync open — "it's just one line"

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    while (reader.Read()) // sync Read inside async method
    {
        decimal price = reader.GetDecimal(2);
        await output.WriteAsync(Encoding.UTF8.GetBytes($"{price}\n"));
    }
}
```

List the defects (sync/async mixing, thread blocking, reader semantics) and prioritize fixes.

---

**Answer:**

```csharp
public async Task ExportProductsAsync(string connectionString, Stream output)
{
    await using SqlConnection conn = new SqlConnection(connectionString);
    conn.Open(); // sync open — "it's just one line"

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    while (reader.Read()) // sync Read inside async method
    {
        decimal price = reader.GetDecimal(2);
        await output.WriteAsync(Encoding.UTF8.GetBytes($"{price}\n"));
    }
}
```

List the defects (sync/async mixing, thread blocking, reader semantics) and prioritize fixes.

**Answer:** This method mixes async and sync ADO.NET on the same connection and reader — exactly the pitfall documented in **Program.cs** Section 9. `conn.Open()` and `reader.Read()` block thread-pool threads during network I/O, partially negating `ExecuteReaderAsync`, and can cause subtle ordering/state bugs with Microsoft.Data.SqlClient when sync and async APIs are interleaved.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `conn.Open()` sync after async setup | Blocks thread during connect; inconsistent async pattern |
| Runtime/async | `reader.Read()` in `while` with `ExecuteReaderAsync` | Blocks per row; **do not mix sync Read and async ReadAsync** on same reader |
| Correctness | Mixed sync/async on same `SqlConnection`/`SqlDataReader` | Undefined/problematic behavior; harder failures under load |
| Maintainability | `async` method that mostly blocks | Misleading signature; reviewers assume non-blocking export |

**Fix (priority order):**

1. Replace `conn.Open()` with `await conn.OpenAsync(cancellationToken)`.
2. Replace `while (reader.Read())` with `while (await reader.ReadAsync(cancellationToken))`.
3. Pass `CancellationToken` from the host job through open, execute, read, and `output.WriteAsync`.
4. Consider **ProductStream.ReadProductsAsync** + `await foreach` for clearer streaming semantics.
5. Add `ConfigureAwait(false)` if this lives in a library class consumed by multiple hosts.

**Production takeaway:** Pick one style per connection/reader — **async all the way** for server export paths. See **Program.cs** pitfall note under Section 9.

---

---

#### Q6. (R) A dashboard action needs average price and count. The developer avoids "async all the way" for the scalar:

```csharp
app.MapGet("/dashboard", async (IConfiguration config) =>
{
    string cs = config.GetConnectionString("AdoNetTutorial")!;

    Task<int> countTask = ProductRepository.GetProductCountAsync(cs);

    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();
    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT AVG(UnitPrice) FROM dbo.Products;";
    decimal avg = Convert.ToDecimal(cmd.ExecuteScalar()); // sync scalar on open async connection

    int count = await countTask;
    return Results.Ok(new { avg, count });
});
```

Two connections hit SQL Server concurrently, but throughput still collapses under load. Explain the thread-pool impact of `ExecuteScalar()` here and how you would rewrite this endpoint.

---

**Answer:**

```csharp
app.MapGet("/dashboard", async (IConfiguration config) =>
{
    string cs = config.GetConnectionString("AdoNetTutorial")!;

    Task<int> countTask = ProductRepository.GetProductCountAsync(cs);

    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();
    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT AVG(UnitPrice) FROM dbo.Products;";
    decimal avg = Convert.ToDecimal(cmd.ExecuteScalar()); // sync scalar on open async connection

    int count = await countTask;
    return Results.Ok(new { avg, count });
});
```

Two connections hit SQL Server concurrently, but throughput still collapses under load. Explain the thread-pool impact of `ExecuteScalar()` here and how you would rewrite this endpoint.

**Answer:** Starting `countTask` then calling sync `ExecuteScalar()` still blocks the **current request thread** until AVG returns, while another connection runs the count query. Under concurrency, each request ties up a thread-pool thread for both waits — parallel tasks do not help if every handler blocks on `ExecuteScalar()`. Replace sync scalar with `ExecuteScalarAsync`, pass `RequestAborted`, and prefer one round-trip or properly awaited parallel work.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `cmd.ExecuteScalar()` sync in async endpoint | Blocks thread during aggregate query — same class of bug as sync `Open()` |
| Resource | Two connections per dashboard hit | Doubles pool pressure; often unnecessary |
| Design | Fire-and-forget `countTask` without awaiting until after sync block | Overlaps I/O but request thread still blocked on scalar |
| Correctness | No cancellation token on either query | Slow aggregates hold threads after client abort |

**Fix (priority order):**

1. Replace `ExecuteScalar()` with `await cmd.ExecuteScalarAsync(ctx.RequestAborted)` — mirror **Program.cs** `DemonstrateExecuteScalarAsync`.
2. Pass `ctx.RequestAborted` into `GetProductCountAsync(cs, ctx.RequestAborted)`.
3. Await both with `Task.WhenAll` if keeping two queries, **or** combine: `SELECT COUNT(*), AVG(UnitPrice) FROM dbo.Products` in one command (best for dashboard).
4. Remove redundant second connection when a single batch/query suffices.
5. Load-test: thread-pool starvation shows up as growing queue latency even when SQL is "parallel."

**Production takeaway:** `ExecuteScalarAsync` exists for the same reason as `OpenAsync` — **release the thread while waiting on SQL Server**. Parallel `Task`s do not fix sync blocking on the request thread. See **Program.cs** Sections 7–8.

---

---

#### Q7. (D) You must expose a large product export from ADO.NET without loading a `List<ProductRow>`. Options: (A) buffer with `ExecuteReaderAsync` + `List<T>`, (B) `IAsyncEnumerable<ProductRow>` over `ReadAsync` like **Services/ProductStream.cs**, or (C) raw `SqlDataReader` returned from the repository. Compare memory, cancellation, connection lifetime, and ASP.NET response shaping — which do you ship and why?



**Answer:**

**Answer:** Ship **(B) `IAsyncEnumerable<ProductRow>`** (or a thin wrapper around **ProductStream.ReadProductsAsync**) for ASP.NET Core exports. It streams rows with pull-based `ReadAsync`, composes with `await foreach`, accepts `[EnumeratorCancellation]` / `RequestAborted`, and keeps `SqlDataReader` + connection lifetime inside the handler or an scoped service method — without leaking ADO.NET types into controllers.

| Option | Memory | Cancellation | Connection / reader lifetime | ASP.NET shaping |
|---|---|---|---|---|
| **(A) List buffer** | O(n) — all rows in RAM | Easy but late — work mostly done before return | Short reader life; simple | Simple JSON array — bad for huge catalogs |
| **(B) IAsyncEnumerable** | O(1) per step — one row at a time | `[EnumeratorCancellation]` → `ReadAsync(ct)` | Caller must `await using` conn/reader for enumeration duration | `IResult` from `TypedResults`, NDJSON, or custom chunked body |
| **(C) Raw SqlDataReader** | O(1) if consumed immediately | Manual on every read | **Leaky** — caller must dispose; easy to double-dispose | Forces API layer to know ADO.NET — poor seam |

- **Reject (A)** for large exports — **Program.cs** Section 13 contrasts streaming vs loading a `List` first; fine only for small, bounded reports.
- **Reject (C)** in layered apps — returning `SqlDataReader` from a repository breaks abstraction and makes DI/scoping errors likely (disposed connection before controller reads).
- **Choose (B)** — align with **ProductStream.cs**: `while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) yield return ...`; handler owns `await using SqlConnection` / reader for the life of the response; pass `HttpContext.RequestAborted`.
- Add **timeouts** (`CommandTimeout`, client/proxy limits) and **SequentialAccess** if rows are wide; log row counts, not every row.

**Production takeaway:** `IAsyncEnumerable` is the bridge between **forward-only async ADO.NET** and **HTTP streaming** — keep connection disposal in one place, cancellation end-to-end, and ADO.NET types behind the repository/stream helper.

---
