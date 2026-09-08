# Introduction to ADO.NET — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Introduction to ADO.NET](#chapter-01-introduction-to-adonet)
  - [Q1. What is ADO.NET?](#q1-what-is-adonet)
  - [Q2. What is the difference between connected and disconnected data access in ADO.NET?](#q2-what-is-the-difference-between-connected-and-disconnected-data-access-in-adonet)
  - [Q3. What are the core building blocks of ADO.NET (connection, command, reader, adapter)?](#q3-what-are-the-core-building-blocks-of-adonet-connection-command-reader-adapter)
  - [Q4. What is the difference between ADO.NET and an ORM like Entity Framework Core?](#q4-what-is-the-difference-between-adonet-and-an-orm-like-entity-framework-core)
  - [Q5. When would you choose ADO.NET over Dapper or EF Core?](#q5-when-would-you-choose-adonet-over-dapper-or-ef-core)
  - [Q6. What is the connected model, and which ADO.NET types does it primarily use?](#q6-what-is-the-connected-model-and-which-adonet-types-does-it-primarily-use)
  - [Q7. What is the disconnected model, and which ADO.NET types does it primarily use?](#q7-what-is-the-disconnected-model-and-which-adonet-types-does-it-primarily-use)
  - [Q8. What are the trade-offs of hand-written SQL versus a higher-level ORM?](#q8-what-are-the-trade-offs-of-hand-written-sql-versus-a-higher-level-orm)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

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

## Gotchas — Introduction to ADO.NET (Interview Traps)

---

#### Gotcha 1. Using DataSet where SqlDataReader should be used

**Concepts**
- connected vs disconnected model mismatch
- DataSet in-memory materialization overhead
- SqlDataReader forward-only streaming efficiency
- server-side resource hold time trade-off
- full-table memory load under concurrent traffic

**Answer**

Using `SqlDataAdapter.Fill(dataSet)` for large result sets pulls every row into application memory before any response is sent, causing excessive heap allocations and GC pressure under concurrent traffic. `SqlDataReader` streams rows forward-only and releases the connection as soon as the reader is disposed, which is far more efficient for stateless REST APIs. Reserve `DataSet` for disconnected editing workflows where in-memory snapshot navigation and batch updates are genuinely needed.

---

#### Gotcha 2. ADO.NET objects not disposed — connection pool exhaustion

**Concepts**
- SqlConnection, SqlDataReader disposal requirement
- `using`/`await using` disposal guarantee on exceptions
- pool slot exhaustion under concurrent load
- GC finalization too slow for production
- undisposed reader blocking additional commands

**Answer**

Failing to wrap `SqlConnection`, `SqlCommand`, and `SqlDataReader` in `using` or `await using` blocks leaves database connections checked out from the pool until garbage collection, which is far too slow under concurrent load. Under sustained traffic this exhausts the pool and causes "timeout expired obtaining connection from pool" errors that only surface in production, never in single-user local tests. Every ADO.NET object implementing `IDisposable` must be disposed deterministically even when exceptions occur.

---

#### Gotcha 3. String concatenation instead of parameterized queries

**Concepts**
- SQL injection via string interpolation
- SqlParameter with `@placeholder` as prevention
- query plan cache pollution from literal SQL strings
- ADO.NET has no automatic sanitization
- blocking defect in code review

**Answer**

Building SQL with string concatenation or C# interpolation (`$"WHERE Id = {id}"`) sends user input as literal SQL text, enabling SQL injection and preventing query plan reuse. ADO.NET provides no automatic sanitization — every user-supplied value must be bound through a `SqlParameter` with an explicit `@placeholder`. Code review should treat any dynamic SQL without parameter placeholders as a blocking security defect regardless of where the input originates.

---

#### Gotcha 4. SqlCommand.CommandTimeout units are seconds, not milliseconds

**Concepts**
- CommandTimeout unit confusion (seconds vs milliseconds)
- default 30-second timeout for standard queries
- long-running batch operations needing extended timeout
- `SqlException: Execution Timeout Expired`
- timeout is per command, not per connection

**Answer**

`SqlCommand.CommandTimeout` is measured in seconds, not milliseconds — a common unit confusion that sets timeouts 1000× shorter than intended when developers assume milliseconds. The default is 30 seconds, which is too short for long-running batch imports, aggregate reports, or index-heavy queries. Set `command.CommandTimeout = 300` for heavy operations and `0` for commands that must run to completion without any timeout, such as scheduled maintenance jobs.

---

#### Gotcha 5. ExecuteScalar returns DBNull.Value on empty aggregate results

**Concepts**
- `ExecuteScalar` return type is `object?`
- `DBNull.Value` vs C# `null` vs integer `0`
- `COUNT(*)` always returns 0, not null
- `MAX`/`MIN`/`SUM` return `DBNull.Value` on empty set
- direct cast to `int` throws `InvalidCastException`

**Answer**

`ExecuteScalar` returns `object?` — casting the result directly to `int` throws `InvalidCastException` when the value is `DBNull.Value`, which happens for aggregate functions like `MAX` or `SUM` applied to an empty result set. `COUNT(*)` always returns a non-null integer, but `MAX`, `MIN`, and `SUM` return `DBNull.Value` when no rows match the filter. Always check with `result is DBNull` or cast via `result as int?` before using the value, and supply an explicit default for the null case.

---

#### Gotcha 6. Open SqlDataReader blocks a second command on the same connection

**Concepts**
- MARS disabled by default in SQL Server
- forward-only cursor holds connection busy state
- second `ExecuteReader` throws `InvalidOperationException`
- dispose first reader before issuing next command
- MARS as workaround vs proper disposal as fix

**Answer**

Issuing a second `SqlCommand.ExecuteReader()` on the same open `SqlConnection` while the first `SqlDataReader` is still open throws `InvalidOperationException` unless Multiple Active Result Sets (MARS) is enabled in the connection string. Enabling MARS adds overhead and is rarely the right solution — the correct fix is to close and dispose the first reader before issuing the next command. The typical bug is loading a header row and then loading detail rows on the same connection without closing the reader in between.

---

#### Gotcha 7. Calling Open() on an already-open connection throws

**Concepts**
- `InvalidOperationException` on double-open
- connection state check before `Open()`
- connection pooling returns ready-to-use connection object
- `connection.State` property for conditional open
- short-lived per-operation connection pattern

**Answer**

Calling `Open()` on a `SqlConnection` that is already open throws `InvalidOperationException` — a common bug when a connection is shared across helper methods without tracking its state. The safest pattern is to create and open a new `SqlConnection` per operation inside a `using` block, allowing the pool to manage reuse transparently. If a single connection must span multiple commands, check `connection.State != ConnectionState.Open` before calling `Open()` defensively.

---

#### Gotcha 8. Transaction begun after first command — first DML auto-commits

**Concepts**
- implicit autocommit when no transaction is active
- `BeginTransaction` must precede first DML statement
- unit-of-work atomicity boundary
- partial commit leaves database in inconsistent state
- integration test masking in single-user scenarios

**Answer**

Beginning a `SqlTransaction` after the first `ExecuteNonQuery` has already executed means that command committed under implicit autocommit and is not enrolled in the transaction. When subsequent commands fail and the transaction rolls back, the first command's changes persist, leaving the database inconsistent. Always call `connection.BeginTransaction()` immediately after opening the connection, before any DML, and assign the transaction instance to every subsequent `SqlCommand.Transaction` property.

---

#### Gotcha 9. DataSet is not thread-safe under concurrent modification

**Concepts**
- `DataSet` and `DataTable` not thread-safe
- concurrent row modification causes data corruption
- shared static `DataSet` as application-level cache anti-pattern
- explicit locking required for concurrent shared tables
- immutable DTO snapshots as safer cache alternative

**Answer**

`DataSet` and `DataTable` are not thread-safe — concurrent reads and writes from multiple threads without synchronization cause data corruption and unpredictable exceptions. Unlike `SqlDataReader`, which is used sequentially on a single connection thread, a `DataSet` held as a shared instance (such as in a static field or an application-level cache) requires explicit locking for any concurrent modification. For high-concurrency read caches, prefer `IMemoryCache` with immutable DTO snapshots over a shared mutable `DataTable`.

---

#### Gotcha 10. Returning DataTable directly from a Web API produces non-standard JSON

**Concepts**
- `DataTable` JSON serialization includes schema metadata
- `DBNull.Value` serialization inconsistency across serializers
- API contract pollution from `RowState`, `RowError`, `TableName`
- DTO projection as the correct pattern for API responses
- `System.Text.Json` and Newtonsoft.Json handle `DataSet` differently

**Answer**

Returning a `DataSet` or `DataTable` directly from a Web API action produces schema-heavy, non-standard JSON that includes metadata like `TableName`, `RowState`, and `RowError` rather than clean DTO fields. `DBNull.Value` in rows serializes as `{}` in some serializers and as `null` in others, creating inconsistent API contracts. Always project query results into typed DTOs before returning from a controller action — never serialize `DataSet` or `DataRow` objects directly over an HTTP response.

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

**Answer:**

_Answer not found._
