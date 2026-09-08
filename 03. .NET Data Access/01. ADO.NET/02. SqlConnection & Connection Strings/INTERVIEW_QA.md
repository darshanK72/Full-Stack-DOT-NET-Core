# SqlConnection & Connection Strings — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [SqlConnection & Connection Strings](#chapter-02-sqlconnection--connection-strings)
  - [Q1. What is connection pooling in ADO.NET?](#q1-what-is-connection-pooling-in-adonet)
  - [Q2. Does creating `new SqlConnection()` every time open a new physical database connection?](#q2-does-creating-new-sqlconnection-every-time-open-a-new-physical-database-connection)
  - [Q3. Why should you use `using` or `await using` with connections?](#q3-why-should-you-use-using-or-await-using-with-connections)
  - [Q4. How do you store connection strings securely in ASP.NET Core?](#q4-how-do-you-store-connection-strings-securely-in-aspnet-core)
  - [Q5. What is the difference between `Microsoft.Data.SqlClient` and `System.Data.SqlClient`?](#q5-what-is-the-difference-between-microsoftdatasqlclient-and-systemdatasqlclient)
  - [Q6. What is a pool exhaustion error, and what typically causes it?](#q6-what-is-a-pool-exhaustion-error-and-what-typically-causes-it)
  - [Q7. What symptoms indicate a misconfigured or exhausted connection pool?](#q7-what-symptoms-indicate-a-misconfigured-or-exhausted-connection-pool)
  - [Q8. How do unclosed connections affect pool availability?](#q8-how-do-unclosed-connections-affect-pool-availability)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

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

## Gotchas — SqlConnection & Connection Strings (Interview Traps)

---

#### Gotcha 1. Hardcoding connection strings in source code

**Concepts**
- connection string in source commits credentials to version control
- environment-specific configuration via `appsettings.json` and secrets
- Azure Key Vault / environment variables for production credentials
- secret scanning in CI pipelines
- principle of least privilege for database logins

**Answer**

Hardcoding a connection string containing a username and password directly in source code commits credentials to version control, where they are visible to every contributor and any future audit. Use `IConfiguration` with `appsettings.json` for non-sensitive configuration, `.NET User Secrets` for local development credentials, and Azure Key Vault or environment variables for production. CI pipelines should run secret scanning tools to catch accidental credential commits before they reach the main branch.

---

#### Gotcha 2. Undisposed SqlConnection exhausts the connection pool

**Concepts**
- pool slot not returned until `Dispose` or GC finalization
- GC finalization too slow for production concurrency
- "timeout expired obtaining connection from pool" under load
- `using`/`await using` for guaranteed disposal
- undisposed reader on open connection also leaks

**Answer**

An undisposed `SqlConnection` keeps its pool slot checked out until garbage collection, which is far too slow under concurrent API traffic. Under a sustained leak rate the pool reaches `Max Pool Size` (default 100), and all subsequent attempts to borrow a connection block until timeout, producing "timeout expired obtaining connection from pool" errors that never appear in single-user local tests. Always wrap `SqlConnection` in `using` or `await using` so the slot is returned to the pool even when exceptions occur.

---

#### Gotcha 3. Minor connection string differences create separate pools

**Concepts**
- pool key is the exact connection string
- whitespace or case differences create new pools
- `Max Pool Size` applies per unique connection string
- fragmented pools reduce effective pool capacity
- connection string normalization before pooling

**Answer**

The connection pool uses the exact connection string as its key — even a trailing space or a different keyword casing creates a separate pool with its own `Max Pool Size` quota. Application code that builds connection strings dynamically by appending user-specific options can inadvertently fragment the pool into hundreds of tiny pools, each below the effective concurrency threshold. Normalize connection strings to a canonical form and store the single canonical string in configuration rather than constructing them at runtime.

---

#### Gotcha 4. `TrustServerCertificate=true` silently disables certificate validation

**Concepts**
- `TrustServerCertificate=true` bypasses TLS certificate check
- man-in-the-middle attack risk in production
- `Encrypt=true` required alongside certificate trust
- certificate validation as defense layer
- LocalDB exception for development-only trust

**Answer**

Setting `TrustServerCertificate=true` bypasses TLS certificate validation entirely, allowing any server to present any certificate without rejection — a man-in-the-middle vulnerability in production. This keyword is appropriate only for LocalDB development environments where a self-signed certificate is expected. In production, use a valid SQL Server certificate signed by a trusted CA and remove `TrustServerCertificate=true`; also ensure `Encrypt=true` is set since `Microsoft.Data.SqlClient` 4.0+ defaults to encrypted connections but earlier versions did not.

---

#### Gotcha 5. `Integrated Security=true` requires Kerberos and creates a separate pool per Windows identity

**Concepts**
- Windows authentication via Kerberos
- separate pool per impersonated identity
- pool fragmentation under per-user impersonation
- service account vs application-pool identity
- Kerberos double-hop in web applications

**Answer**

`Integrated Security=true` uses the current Windows identity for authentication, but in a web application each impersonated user identity gets its own connection pool, fragmenting capacity and increasing connection overhead dramatically under large user bases. Furthermore, Kerberos delegation required for double-hop scenarios (web server to SQL Server on behalf of the user) requires explicit SPN configuration and `ConstrainedDelegation` setup that is frequently missing. For most web APIs, use a dedicated SQL login or Managed Identity with a single pool rather than per-user Windows impersonation.

---

#### Gotcha 6. `Max Pool Size` default (100) is per connection string, not per application

**Concepts**
- `Max Pool Size` default of 100 slots per pool
- pool applies per unique connection string per process
- concurrent requests can exceed pool limit
- `Min Pool Size` keeps idle connections open
- pool tuning based on expected concurrency

**Answer**

The default `Max Pool Size=100` limits each pool to 100 simultaneous connections, which is enough for most applications but insufficient for high-concurrency APIs where many async requests overlap. Exceeding the pool size causes requests to queue — if the queue wait exceeds `Connection Timeout` (default 15 seconds) the connection attempt throws. Tune `Max Pool Size` to match the expected peak concurrency from profiling, and set `Min Pool Size` to a warm baseline (e.g. 5–10) so the first requests after an idle period do not bear connection establishment latency.

---

#### Gotcha 7. `OpenAsync` not awaited — synchronous open blocks thread pool

**Concepts**
- `SqlConnection.Open()` vs `OpenAsync()` distinction
- blocking thread during TCP/named-pipe handshake
- async-over-sync pattern wastes thread pool threads
- `await` required for all async ADO.NET methods
- ASP.NET Core thread pool starvation under load

**Answer**

Calling synchronous `Open()` on a `SqlConnection` from an ASP.NET Core request handler blocks a thread pool thread for the entire TCP handshake and authentication round-trip. Under concurrent load this inflates the active thread count, increases context-switching overhead, and can starve the thread pool. Always use `await connection.OpenAsync(cancellationToken)` inside async methods, and pass the request's `CancellationToken` so that a client disconnect can abort an in-progress connection attempt.

---

#### Gotcha 8. Password rotation invalidates all existing pooled connections silently

**Concepts**
- pooled connections authenticated with old credentials
- connection with stale auth fails on next use after password change
- pool clearing required after password rotation
- `SqlConnection.ClearPool` / `ClearAllPools` for forced invalidation
- resilient retry policy for transient authentication failures

**Answer**

When a SQL Server login password is rotated, existing pooled connections authenticated with the old password continue to work until the pool is drained or the server-side session is terminated. New connection attempts using the updated connection string fail if the pool still contains old-credential connections that are reused. Call `SqlConnection.ClearAllPools()` immediately after rotating credentials to discard stale pool entries, and implement a retry policy with exponential backoff to handle the brief window where old connections are being replaced.

---

#### Gotcha 9. Connection string `Async=true` is a legacy keyword for System.Data.SqlClient

**Concepts**
- `Async=true` keyword was required for `System.Data.SqlClient`
- `Microsoft.Data.SqlClient` enables async by default
- legacy keyword has no effect in modern client
- mixing old and new client namespaces in one project
- migration from `System.Data.SqlClient` to `Microsoft.Data.SqlClient`

**Answer**

The `Async=true` connection string keyword was required to enable asynchronous command execution in the older `System.Data.SqlClient` package — without it, async methods would fall back to synchronous behavior. `Microsoft.Data.SqlClient` (the modern recommended package) enables async support automatically and ignores this keyword. Projects migrating from the old package should verify they are referencing `Microsoft.Data.SqlClient` in their NuGet references and remove `Async=true` from connection strings, since its presence can mask a failure to complete the migration.

---

#### Gotcha 10. `Connection Timeout` vs `Command Timeout` confusion

**Concepts**
- `Connection Timeout` is in the connection string (default 15s)
- `Command Timeout` is on `SqlCommand` (default 30s)
- two distinct timeout stages in a single database operation
- `Connection Timeout=0` disables connection timeout — not recommended
- separate tuning needed for connection establishment vs query execution

**Answer**

ADO.NET has two independent timeouts that are commonly confused: `Connection Timeout` in the connection string controls how long `Open()` or `OpenAsync()` waits for a pool slot or network connection (default 15 seconds), while `SqlCommand.CommandTimeout` controls how long a command waits for the server to return results (default 30 seconds). Setting one does not affect the other — a long-running query that never connects will hit the connection timeout, while a slow query on an open connection hits the command timeout. Tune each separately based on observed behavior rather than setting either to 0 (unlimited), which can hide hung queries.

---

## Scenario-Based Questions (Karat Format)

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

**Answer:** SqlClient allows **one active reader per connection** unless **MARS** (`MultipleActiveResultSets=true`) is enabled. This snippet disposes each reader before opening the next, so it is usually fine — failures happen when a reader is still open (nested call, missed `await using`, or refactored code that overlaps readers). MARS can unblock overlapping readers but adds complexity; prefer sequential queries, a join, or two connections.

- **When it fails:** Any code path leaves `headerReader` open while starting `lineReader` — e.g. early `return` inside the first block without disposal, exception before dispose, or a helper that starts the second query while the first reader is still streaming. Dev "works" with small datasets and strict disposal; prod fails under different call paths or provider timing.
- **MARS fix:** Setting `MultipleActiveResultSets=true` (see `DemoConnectionBuilder.BuildLocalDbString` default `false`) tells SQL Server to multiplex batches on one connection so multiple readers can be active. It fixes legitimate overlap but masks design smells and can increase locking overhead.
- **Preferred production patterns:** (1) **One query** with join or JSON subquery; (2) **Two sequential commands** with first reader fully disposed (this sample's intent); (3) **Two connections** from the pool if you truly need parallel reads; (4) enable MARS **only** when overlapping readers are required and measured.
- **Verify:** Ensure `CommandBehavior.CloseConnection` is not mixed incorrectly; always `await using` readers; never pass an open connection into nested repositories that also open readers.

**Production takeaway:** MARS is a scalpel, not a default — chapter key reference lists `MultipleActiveResultSets` as opt-in for a reason. Default to one reader per connection lifetime slice.

---

#### Q4. (P) Your team copies the LocalDB connection string from `ConnectionStringSamples.LocalDbIntegratedSecurity` into Azure App Service configuration:

```
Server=tcp:contoso.database.windows.net,1433;Database=OrdersDb;User ID=app_user;Password=***;Encrypt=true;TrustServerCertificate=true;Integrated Security=true
```

App Service logs show `Login failed for user ''` or SSL/certificate errors depending on environment. Walk through what each problematic key does in SqlClient 5.x on Azure SQL, what the production string should look like instead, and why `Encrypt=true` alone is not enough if `TrustServerCertificate=true` remains.

---

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

#### Q6. (P) You deploy the same API to **AKS Linux containers** (no domain join). The connection string still uses `Integrated Security=true` copied from the on-prem IIS app. Locally on Windows it works; in the cluster every `Open()` fails with login or SSPI errors. How do you fix auth for Linux containers, and where should the secret live instead of `appsettings.json`?

---

**Answer:** Linux containers have no Windows identity for SSPI — replace `Integrated Security=true` with **SQL authentication** (secret in Key Vault) or **Azure AD authentication** via **workload identity / managed identity**, and load the connection string from Key Vault or Kubernetes secrets mounted at runtime, not from committed `appsettings.json`.

- **Why Integrated Security fails:** `IntegratedSecurity=true` maps to Windows auth (see `DemoConnectionBuilder` and key reference). AKS pods are Linux, not domain-joined — no Kerberos ticket, so SqlClient cannot authenticate as the IIS app pool identity.
- **Fix option A — SQL auth:** `User ID=…;Password=…;Encrypt=True;TrustServerCertificate=False` with password rotated and stored in **Azure Key Vault**, referenced via **AKS CSI driver** or App Configuration; inject with `ConnectionStrings__Default` env var (double underscore — `ConnectionSecurityPreview`).
- **Fix option B — Azure AD (preferred on Azure SQL):** Enable **managed identity** on the workload (or service principal), grant `CREATE USER … FROM EXTERNAL PROVIDER`, use `Authentication=Active Directory Managed Identity` (or DefaultAzureCredential chain) — no password to leak.
- **Do not use** `appsettings.json` in the container image for production secrets — image layers and git are untrusted stores; User Secrets are dev-only.
- **Network:** Ensure Azure SQL **firewall rules** allow AKS **outbound IP** (or deploy with **Private Link / VNet service endpoints**) — firewall blocks surface as connection timeouts (`SqlException` network-related), not login 18456; allow the cluster egress before debugging auth.

**Production takeaway:** Match auth mode to **host OS and cloud identity** — Integrated Security is for Windows/domain scenarios; containers need token- or secret-based SQL/Azure AD auth from a secret store.

---

#### Q7. (D) Production uses Azure SQL with geo-replication. During a regional outage, ops gives you a **failover connection string** pointing at the secondary server. Your app currently has one `ConnectionStrings:Default` in Key Vault. Describe how connection failover works with SqlClient (including `Failover Partner` / `ApplicationIntent` / Active Directory managed identity if relevant), what you change in configuration vs code, and what application-level behavior you still need (retries, idempotency, read-only routing).

---

**Answer:** Prefer **Azure SQL connection policies** (failover groups / `*.database.windows.net` endpoint) so SqlClient follows the current primary automatically; legacy `Failover Partner` applied to on-prem Always On. Configuration changes drive most of the behavior — code still needs transient retry and idempotent writes because failover drops in-flight sessions.

- **Azure SQL (modern):** Use the **failover group listener** hostname (`mygroup.database.windows.net`) in `Server=` — Azure redirects to the current primary after geo-failover. Often **no code change** beyond connection string; retire hard-coded regional server names.
- **Always On / legacy:** `Failover Partner=secondaryHost;` (or mirror partner keys in older docs) lets SqlClient redirect when primary is down — pair with **`ConnectRetryCount`** / **`ConnectRetryInterval`** (SqlClient connection string keys) for transient reconnects.
- **`ApplicationIntent=ReadOnly`:** Routes to readable secondary in AG/read scale-out — use separate connection string (`DefaultRead`) for reporting queries; writes stay on primary intent (default ReadWrite).
- **Managed identity:** Auth mode unchanged across failover — identity and SQL user must exist on **both** sides after replication; failover string swap alone fails if secondary lacks the same AAD user.
- **Configuration vs code:** Update **Key Vault secret** or App Configuration label — e.g. single listener URL instead of two regional secrets — avoid redeploying code for DR string swaps when possible.
- **Application still needs:** (1) **Retry** on `SqlException` numbers for connection broken / timeout (Polly or `EnableRetryOnFailure` in EF); (2) **Idempotent** writes — in-flight transactions are aborted at failover; (3) **Circuit breaker** to avoid stampede on recovering secondary; (4) **Read-your-writes** awareness — reads on secondary may lag; (5) drain or recycle **scoped** connections after failover event (pooled connections to dead primary throw until cleared — `SqlConnection.ClearAllPools()` only in controlled recovery scripts, not per request).

**Production takeaway:** Failover is a **connection routing + resilience** problem — SqlClient and Azure listeners handle server selection, but your app must tolerate broken connections and duplicate side effects. See chapter pooling preview (`Dispose` returns to pool; stale pool entries after DR are an ops play).
