# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/01. Introduction to ADO.NET`

---

#### Q1. (R) Disconnected snapshot misuse in a high-volume API

**Answer:** This misapplies the disconnected `Fill → edit in memory` model from desktop scenarios to a stateless web API over a huge table, causing memory pressure, long open connections, and an unstable JSON contract. A read-only list endpoint should use the connected streaming model and return DTOs, not a mutated `DataTable`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Model choice | `SqlDataAdapter.Fill` loads all rows into a `DataTable` | OOM or Gen2 pressure with ~2M rows; GC pauses under load |
| Connected misuse | Connection open for full table pull | Holds pool slots; scales poorly vs streaming reader |
| API contract | Mutating rows then `Ok(table)` | Untyped JSON (`Column1`, nested schema); breaking changes when columns shift |
| Resource mgmt | `conn` not in `using` | Connection leak on exceptions before `Close()` |
| Design | In-memory markup in GET | Side effects on read; caching and auditing become unreliable |

**Fix (priority order):**

1. Replace with connected read: `using var conn`, parameterized `SqlCommand`, `ExecuteReader()`, map to `OrderDto` while `Read()` — one row at a time (`ModelComparison` connected path).
2. Add paging (`TOP`/`OFFSET`) or projection so the API never materializes millions of rows.
3. If markup is required, compute in SQL or map to DTO properties — do not mutate `DataRow` on a GET.
4. Wrap connection (and reader) in `using`; rely on pool return on dispose (ch.06).

**Production takeaway:** Disconnected `DataTable`/`DataSet` fits offline editing and snapshot merge (`WorkflowSteps.DisconnectedSnapshotSteps()`), not bulk REST reads — Karat tests whether you match model to workload.

---

#### Q2. (R) Provider abstraction leaks and dual SqlClient packages

**Answer:** The library is provider-specific despite the "abstraction" name: public surface uses `SqlConnection`, hard-coded connection strings, and the legacy `System.Data.SqlClient` namespace while the host uses `Microsoft.Data.SqlClient`. That prevents swapping providers, risks type identity bugs at runtime, and embeds secrets in a reusable package.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Abstraction leak | `IOrderRepository` returns `SqlConnection` | Callers compile against SQL Server; Npgsql/Sqlite impossible without breaking API |
| Provider package | `System.Data.SqlClient.SqlCommand` vs `Microsoft.Data.SqlClient` in host | Duplicate assemblies, binding redirects, subtle behavioral differences (Azure AD, encryption) |
| Configuration | Connection string inside repository | Secrets in library code; per-environment rotation requires redeploy |
| Security | Password in source | Credential exposure in git, logs, and crash dumps |

**Fix (priority order):**

1. Move connection strings to `IConfiguration` / secret store; inject `IOptions<DatabaseOptions>` or named connection factory — see ch.02 `ConnectionConcept` pitfalls.
2. Standardize on **`Microsoft.Data.SqlClient`** for all new SQL Server code (`ProviderGuidance`).
3. Expose **`DbConnection`** / **`DbCommand`** (or hide connections entirely behind repository methods returning DTOs) so provider-agnostic libraries target `System.Data.Common` abstractions (`AdoNetStackCatalog`).
4. Keep concrete `SqlConnection` only at the composition root (DI registration), not on public interfaces.

**Production takeaway:** Abstract types are for boundaries; leaking `Sql*` types and legacy packages turns a "shared data layer" into a frozen SQL Server fork.

---

#### Q3. (D) ADO.NET vs Dapper vs EF Core per service

**Answer:** Assign stacks by control vs model complexity: bulk/staging needs raw ADO.NET, domain CRUD needs EF Core, and the latency-sensitive payment path fits Dapper over explicit SQL — forcing one ORM everywhere trades away the right tool for each workload.

| Service | Choice | Why |
|---|---|---|
| **A** (bulk MERGE) | **ADO.NET** | `SqlBulkCopy`, staged MERGE, and fine-grained timeout/batch control — ORM change tracking adds no value on 5M-row loads |
| **B** (12 entities, evolving schema) | **EF Core** | Migrations, relationships, LINQ, and `SaveChanges` match weekly schema churn (`TechnologyChoiceGuide`) |
| **C** (3 SQL statements, strict SLA) | **Dapper** | Minimal overhead, parameterized SQL you own, fast POCO mapping without EF query compilation and tracking cost |

**Mistake when picking EF Core for all three:** Service A pays memory and time for change tracking and generated SQL ill-suited to bulk insert; Service C may miss latency targets and still requires hand-tuned SQL for edge cases — while Service A developers fight `SaveChanges` batching limits instead of using provider bulk APIs.

**Production takeaway:** All three sit on the same ADO.NET connection stack; the decision is how much mapping and lifecycle you delegate — not "ORM vs no ORM" in the abstract.

---

#### Q4. (R) DbDataReader disposal and second command on same connection

**Answer:** With MARS off, an undisposed `SqlDataReader` keeps the connection busy, so the second `ExecuteReader` throws; if the first mapping throws, both reader and connection can leak because nothing is in `using`. Each command/reader pair needs nested `using` blocks (or sequential scopes) before the next execute on that connection.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Reader lifetime | `reader.Close()` without `using`; not disposed on map exception | Connection stuck in "busy" state; pool exhaustion over time |
| Connected rule | Second `ExecuteReader` while first reader active | `SqlException`: open DataReader already exists |
| Resource chain | Connection not in `using` | Leaked connections if any step fails |
| Pattern | Manual `Close()` vs dispose | `Close()` is not exception-safe; `using` matches `WorkflowSteps` step 8 |

**Fix (priority order):**

1. Wrap each reader in `using`: load header inside one block, then lines in a second block on the same open connection.
2. Wrap connection in `using var conn` for the whole method.
3. Optionally enable MARS in connection string only if you truly need overlapping readers — default is off for a reason (ch.04 preview in `ReaderDemo` comments).

```csharp
using var conn = new SqlConnection(_connectionString);
await conn.OpenAsync();

OrderHeaderDto header;
using (var headerCmd = new SqlCommand("SELECT ...", conn))
{
    headerCmd.Parameters.AddWithValue("@id", orderId);
    using var reader = headerCmd.ExecuteReader();
    header = MapHeader(reader);
}

using (var linesCmd = new SqlCommand("SELECT ...", conn))
{
    linesCmd.Parameters.AddWithValue("@id", orderId);
    using var linesReader = linesCmd.ExecuteReader();
    return new OrderDetailDto(header, MapLines(linesReader));
}
```

**Production takeaway:** Connected model rule from the chapter — dispose reader before the next command on the same connection; `using` chains are not optional decoration.

---

#### Q5. (R) SQL injection at architecture level despite admin role

**Answer:** Role gating limits who can call the endpoint, not what SQL runs once invoked — any compromised admin account or CSRF-authenticated session can execute arbitrary T-SQL (read cross-tenant data, `DROP`, `xp_cmdshell` if enabled). Architecture must treat SQL as untrusted input even from privileged users.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Architecture | Arbitrary `CommandText` from HTTP body | Full database capability exposed through one endpoint |
| Trust model | "Only admins" ≠ safe SQL | Insider threat, stolen admin token, or SSRF-to-admin path |
| Command API | No parameterization boundary | Dynamic SQL bypasses all `SqlParameter` defenses from ch.03 |
| Operations | Ad hoc queries on production reporting DB | Unbounded scans, locks, and audit gaps |

**Fix (priority order):**

1. **Remove arbitrary SQL** — replace with a catalog of named, reviewed reports (whitelist of stored procedures or views with fixed parameters).
2. **Layered controls:** read-only replica + DB user with `db_datareader` only on approved schemas; deny DDL; optional query governor / timeout; separate reporting database synced from OLTP.
3. If dynamic filters are required, use a **constrained query builder** that only emits parameterized fragments for known columns — never concatenate raw user text into `CommandText` (`CommandConcept`: always `@Name` parameters).
4. Log report id + parameters, not free-form SQL, for audit.

**Production takeaway:** SQL injection is an architecture failure when the system accepts SQL as input — authentication does not parameterize statements.

---

#### Q6. (P) Hard-coded connection strings in a new worker service

**Answer:** A copied `const` connection string couples every environment to one server, embeds secrets in source control, and blocks rotation without rebuilds. Use `IConfiguration` with user secrets locally, environment variables or Key Vault in CI/production, and least-privilege SQL auth instead of `sa`.

- **Security:** Password in git history is a permanent leak; `sa` violates least privilege; static IP/server name wrong for DR and scale-out.
- **Operations:** Dev/staging/prod cannot diverge without code forks; secret rotation requires redeploy; scanners flag committed credentials immediately.
- **Modern pattern:** `builder.Configuration.GetConnectionString("Sales")` bound from `appsettings.json` (no secrets), `dotnet user-secrets` for local dev, Azure Key Vault / environment variables in deployment; prefer **managed identity** to SQL where possible (ch.02).
- **Worker-specific:** register `IDbConnection` factory or options type in DI; never a public static `Db.ConnectionString` consumed across assemblies.

**Production takeaway:** Connection strings are configuration, not constants — `ConnectionConcept` explicitly warns against hard-coded secrets; new services should not inherit monolith patterns.

---

#### Q7. (R) Transaction boundary placement

**Answer:** Patch B leaves the inventory debit outside the transaction, so a failure after `debit.ExecuteNonQuery()` commits a stock reduction without a matching order — Patch A wraps both commands in one `DbTransaction` on the same connection, which is the correct atomic unit per `TransactionConcept`.

**Issues (Patch B):**

| Category | Problem | Impact |
|---|---|---|
| Transaction scope | `BeginTransaction` after first `ExecuteNonQuery` | Debit is auto-committed; insert failure = lost inventory |
| Concurrency | Last-unit race | Two requests debit successfully; only one order may insert — oversell |
| Correctness | Non-atomic business operation | Violates "all commit or all roll back" rule from ch.06 preview |

**Fix (priority order):**

1. Open connection → **`BeginTransaction`** → assign **`cmd.Transaction = tx`** on **every** participating command → `Commit()` once at end (`TransactionConcept` sketch).
2. On any failure: `Rollback()` and rethrow — do not swallow exceptions.
3. For cross-resource scenarios later, evaluate **`TransactionScope`** or outbox pattern — but single-database order+inventory stays on one `SqlConnection` transaction.
4. Add appropriate isolation / row versioning for inventory (`UPDLOCK`, `HOLDLOCK`, or optimistic concurrency) — transaction boundary alone does not fix all races.

**Production takeaway:** The transaction must begin before the first write that must be paired with subsequent writes; "begin tx after partial success" is a classic production oversell bug.

---

#### Q8. (R) DataSet over gRPC with static caching

**Answer:** Returning and caching `DataSet` across a microservice boundary couples services to ADO.NET's in-memory relational model, duplicates untyped schema in a static cache, and makes versioning and memory behavior worse than DTOs or streaming reads.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API boundary | gRPC/`DataSet` serialization | Heavy, untyped payload; poor cross-language contracts; schema embedded in wire format |
| Memory | Static cache of `DataSet` graphs | Unbounded growth; stale snapshots; thread-safety bugs on shared `DataRow` mutation |
| Disconnected misuse | Full `SELECT *` into multi-table `DataSet` per call | High memory vs connected reader or projected DTO query |
| Evolution | DB column add/rename | Runtime breaks in consumers indexing `row["Col"]` — no compile-time check |
| Lifetime | `DataSet` not `IDisposable` but holds large object graphs | LOH pressure; difficult to evict compared to immutable DTOs |

**Fix (priority order):**

1. Return **typed DTOs** or protobuf messages mapped from a connected **`DbDataReader`** (or Dapper/EF projection) — one query per aggregate or explicit load pattern.
2. Replace static `DataSet` cache with **IMemoryCache** / distributed cache of **immutable DTOs** with TTL and size limits; key by customer id + schema version.
3. Reserve `DataSet`/`DataTable` for legacy tier interop or offline merge scenarios (`DisconnectedOrderCache` demo scope), not greenfield service contracts.
4. Project columns explicitly instead of `SELECT *` to reduce payload and coupling.

**Production takeaway:** `DataSet` is a disconnected editing container from the `System.Data` era — exposing it in new microservices reverses the abstraction gains ADO.NET common types were meant to enable.
