# Karat — Interview Questions

> **Folder:** `04. .NET Data Access/01. ADO.NET/02. SqlConnection & Connection Strings`  
> **Answers:** [KARAT_INTERVIEW_ANSWERS.md](./KARAT_INTERVIEW_ANSWERS.md)  
> **Level:** Applied production readiness (Layer 2)

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

#### Q4. (P) Your team copies the LocalDB connection string from `ConnectionStringSamples.LocalDbIntegratedSecurity` into Azure App Service configuration:

```
Server=tcp:contoso.database.windows.net,1433;Database=OrdersDb;User ID=app_user;Password=***;Encrypt=true;TrustServerCertificate=true;Integrated Security=true
```

App Service logs show `Login failed for user ''` or SSL/certificate errors depending on environment. Walk through what each problematic key does in SqlClient 5.x on Azure SQL, what the production string should look like instead, and why `Encrypt=true` alone is not enough if `TrustServerCertificate=true` remains.

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

#### Q6. (P) You deploy the same API to **AKS Linux containers** (no domain join). The connection string still uses `Integrated Security=true` copied from the on-prem IIS app. Locally on Windows it works; in the cluster every `Open()` fails with login or SSPI errors. How do you fix auth for Linux containers, and where should the secret live instead of `appsettings.json`?

---

#### Q7. (D) Production uses Azure SQL with geo-replication. During a regional outage, ops gives you a **failover connection string** pointing at the secondary server. Your app currently has one `ConnectionStrings:Default` in Key Vault. Describe how connection failover works with SqlClient (including `Failover Partner` / `ApplicationIntent` / Active Directory managed identity if relevant), what you change in configuration vs code, and what application-level behavior you still need (retries, idempotency, read-only routing).

---
