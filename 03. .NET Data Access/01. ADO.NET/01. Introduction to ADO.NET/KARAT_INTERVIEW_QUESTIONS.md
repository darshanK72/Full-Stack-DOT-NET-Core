# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/01. Introduction to ADO.NET`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

---

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

#### Q3. (D) You inherit three services:

| Service | Workload |
|---|---|
| **A** | Nightly `SqlBulkCopy` of 5M CSV rows into a staging table, then MERGE |
| **B** | CRUD REST API over 12 related entities, schema evolves weekly |
| **C** | Payment microservice: 3 hand-written SQL statements, strict latency SLA |

Each team asks for "one data stack." Using the chapter's technology guide (`TechnologyChoiceGuide`), assign ADO.NET, Dapper, or EF Core per service and justify one sentence each. What mistake do teams make when they pick EF Core for all three?

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
