# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/07. Stored Procedures & Output Parameters`

---

#### Q1. (R) A teammate "fixes" intermittent procedure-not-found errors by copying an SSMS snippet into the repository. Review:

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

#### Q2. (R) After a DBA widens `usp_GetProductName` to `NVARCHAR(200)`, production logs show truncated product names and occasional `SqlException` about string output size. Review the client:

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

#### Q3. (M) `dbo.usp_SearchProducts` returns a result set **and** sets `@TotalCount INT OUTPUT`. Two teammates ship different callers:

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

#### Q4. (R) A dashboard stored procedure returns two SELECT result sets plus an `@ErrorMessage NVARCHAR(500) OUTPUT`. Review:

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

#### Q5. (R) An admin API lets power users pick which reporting stored procedure to run. Review:

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

#### Q6. (D) Your team treats stored procedures as a versioned contract between the DBA and .NET services. Next sprint the DBA adds required `@CreatedBy` with no default and renames `@NewProductId` to `@ProductIdOut` before all app servers redeploy.

**Answer:** Required new parameters without defaults cause **`SqlException` (missing parameter)** on old clients still calling the previous signature. Renaming an `OUTPUT` parameter breaks clients that bind `@NewProductId` by name — the new param stays default/`DBNull` while the app reads zero or throws. Rolling and blue/green deploys amplify this when old and new binaries share one database during cutover.

- **Required `@CreatedBy`:** Any not-yet-deployed app instance sending the old parameter list fails every insert until redeploy completes; blue/green with a single shared DB means half your fleet can error until traffic shifts fully.
- **Rename `@NewProductId` → `@ProductIdOut`:** SqlClient binds by `ParameterName`; old code never receives identity — duplicate key retries or `0` ids corrupt downstream FKs.
- **Rolling deploy worst case:** Mixed versions + one proc version = nondeterministic failures by server; hardest to debug class of outage.
- **Contract practices to enforce:**
  - **Additive first:** new params `NULL`able or with defaults; deprecate old names over releases before removal.
  - **Never rename** in place — add `@ProductIdOut`, keep `@NewProductId` as synonym inside proc during transition.
  - **Deploy order:** expand (deploy proc tolerant of old clients) → migrate apps → contract (remove old params) — expand/contract pattern.
  - **Versioned names** for breaking changes: `usp_InsertProduct_v2` while v1 clients remain.
  - **Integration tests** against staging proc scripts (`LocalDbConnection.cs` setup) in CI on every migration.
- **Production takeaway:** Stored procedures are a **published API** — same compatibility rules as REST. OUTPUT param names and sizes are part of the contract documented alongside `CommandType.StoredProcedure` callers.

---

#### Q7. (R) A "try insert" path mirrors `ReturnValueDemo.TryGetProductName` but the author skips null checks "because OUTPUT is always set." Review:

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

#### Q8. (P) An ASP.NET Core endpoint wraps `usp_InsertProduct` with output identity. Review the async repository under load:

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
