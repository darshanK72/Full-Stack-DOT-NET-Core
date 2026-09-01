# Stored Procedures & Output Parameters — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [Stored Procedures & Output Parameters](#chapter-07-stored-procedures--output-parameters)
  - [Q1. What is a stored procedure, and why use one from ADO.NET?](#q1-what-is-a-stored-procedure-and-why-use-one-from-adonet)
  - [Q2. How do you execute a stored procedure with `SqlCommand`?](#q2-how-do-you-execute-a-stored-procedure-with-sqlcommand)
  - [Q3. What is the difference between output parameters and return values (`ReturnValue`)?](#q3-what-is-the-difference-between-output-parameters-and-return-values-returnvalue)
  - [Q4. When are stored procedures preferred over inline SQL in ADO.NET?](#q4-when-are-stored-procedures-preferred-over-inline-sql-in-adonet)
  - [Q5. What are the trade-offs of putting business logic in stored procedures versus C#?](#q5-what-are-the-trade-offs-of-putting-business-logic-in-stored-procedures-versus-c)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

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

## Gotchas

#### Gotcha 1. String concatenation instead of parameters

**Answer:** Building SQL with `$"WHERE Id = {id}"` or string concatenation sends user input as literal SQL text, bypassing parameterization and enabling SQL injection even when the rest of the application uses an ORM or micro-ORM.

- ADO.NET and Dapper require explicit parameters — never embed raw user strings in SQL text.
- EF Core `FromSqlInterpolated` is safe; passing an ordinary interpolated string to `FromSqlRaw` is not.
- Code review should treat any dynamic SQL without placeholders as a blocking defect.

---

#### Gotcha 3. `AddWithValue` and wrong SQL types

**Answer:** `SqlParameter.AddWithValue` infers parameter types from CLR values, which may not match the database column type — causing implicit conversions, index scans, and poor plan cache behavior.

- Prefer explicit `SqlParameter` with `SqlDbType`, size, and precision matching the column definition.
- String inference often picks oversized `nvarchar` lengths, preventing optimal index seeks on narrower columns.
- Dapper and EF Core parameterize with more predictable typing but custom ADO.NET still needs explicit types.

---

#### Gotcha 5. Transaction started after first command

**Answer:** Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first.

- Call `BeginTransaction` immediately after opening the connection, before any DML.
- EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly.
- Integration tests with single-user data often miss this race because implicit commits appear to "work."

---

## Scenario-Based Questions (Karat Format)

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
