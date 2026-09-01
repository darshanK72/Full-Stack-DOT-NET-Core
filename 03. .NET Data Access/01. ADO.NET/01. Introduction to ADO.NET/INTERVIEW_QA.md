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

## Gotchas

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
