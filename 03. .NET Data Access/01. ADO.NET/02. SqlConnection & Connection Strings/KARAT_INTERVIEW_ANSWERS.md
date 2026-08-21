# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/02. SqlConnection & Connection Strings`

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

**Answer:** Linux containers have no Windows identity for SSPI — replace `Integrated Security=true` with **SQL authentication** (secret in Key Vault) or **Azure AD authentication** via **workload identity / managed identity**, and load the connection string from Key Vault or Kubernetes secrets mounted at runtime, not from committed `appsettings.json`.

- **Why Integrated Security fails:** `IntegratedSecurity=true` maps to Windows auth (see `DemoConnectionBuilder` and key reference). AKS pods are Linux, not domain-joined — no Kerberos ticket, so SqlClient cannot authenticate as the IIS app pool identity.
- **Fix option A — SQL auth:** `User ID=…;Password=…;Encrypt=True;TrustServerCertificate=False` with password rotated and stored in **Azure Key Vault**, referenced via **AKS CSI driver** or App Configuration; inject with `ConnectionStrings__Default` env var (double underscore — `ConnectionSecurityPreview`).
- **Fix option B — Azure AD (preferred on Azure SQL):** Enable **managed identity** on the workload (or service principal), grant `CREATE USER … FROM EXTERNAL PROVIDER`, use `Authentication=Active Directory Managed Identity` (or DefaultAzureCredential chain) — no password to leak.
- **Do not use** `appsettings.json` in the container image for production secrets — image layers and git are untrusted stores; User Secrets are dev-only.
- **Network:** Ensure Azure SQL **firewall rules** allow AKS **outbound IP** (or deploy with **Private Link / VNet service endpoints**) — firewall blocks surface as connection timeouts (`SqlException` network-related), not login 18456; allow the cluster egress before debugging auth.

**Production takeaway:** Match auth mode to **host OS and cloud identity** — Integrated Security is for Windows/domain scenarios; containers need token- or secret-based SQL/Azure AD auth from a secret store.

---

#### Q7. (D) Production uses Azure SQL with geo-replication. During a regional outage, ops gives you a **failover connection string** pointing at the secondary server. Your app currently has one `ConnectionStrings:Default` in Key Vault. Describe how connection failover works with SqlClient (including `Failover Partner` / `ApplicationIntent` / Active Directory managed identity if relevant), what you change in configuration vs code, and what application-level behavior you still need (retries, idempotency, read-only routing).

**Answer:** Prefer **Azure SQL connection policies** (failover groups / `*.database.windows.net` endpoint) so SqlClient follows the current primary automatically; legacy `Failover Partner` applied to on-prem Always On. Configuration changes drive most of the behavior — code still needs transient retry and idempotent writes because failover drops in-flight sessions.

- **Azure SQL (modern):** Use the **failover group listener** hostname (`mygroup.database.windows.net`) in `Server=` — Azure redirects to the current primary after geo-failover. Often **no code change** beyond connection string; retire hard-coded regional server names.
- **Always On / legacy:** `Failover Partner=secondaryHost;` (or mirror partner keys in older docs) lets SqlClient redirect when primary is down — pair with **`ConnectRetryCount`** / **`ConnectRetryInterval`** (SqlClient connection string keys) for transient reconnects.
- **`ApplicationIntent=ReadOnly`:** Routes to readable secondary in AG/read scale-out — use separate connection string (`DefaultRead`) for reporting queries; writes stay on primary intent (default ReadWrite).
- **Managed identity:** Auth mode unchanged across failover — identity and SQL user must exist on **both** sides after replication; failover string swap alone fails if secondary lacks the same AAD user.
- **Configuration vs code:** Update **Key Vault secret** or App Configuration label — e.g. single listener URL instead of two regional secrets — avoid redeploying code for DR string swaps when possible.
- **Application still needs:** (1) **Retry** on `SqlException` numbers for connection broken / timeout (Polly or `EnableRetryOnFailure` in EF); (2) **Idempotent** writes — in-flight transactions are aborted at failover; (3) **Circuit breaker** to avoid stampede on recovering secondary; (4) **Read-your-writes** awareness — reads on secondary may lag; (5) drain or recycle **scoped** connections after failover event (pooled connections to dead primary throw until cleared — `SqlConnection.ClearAllPools()` only in controlled recovery scripts, not per request).

**Production takeaway:** Failover is a **connection routing + resilience** problem — SqlClient and Azure listeners handle server selection, but your app must tolerate broken connections and duplicate side effects. See chapter pooling preview (`Dispose` returns to pool; stale pool entries after DR are an ops play).

---
