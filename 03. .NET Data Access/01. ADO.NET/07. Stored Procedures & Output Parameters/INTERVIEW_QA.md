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

## Gotchas — Stored Procedures & Output Parameters (Interview Traps)

---

#### Gotcha 1. `CommandText` set to `"EXEC ProcName"` with `CommandType.StoredProcedure`

**Concepts**
- `CommandType.StoredProcedure` sends RPC call with bare procedure name
- `CommandText` must contain procedure name only, no `EXEC` keyword
- `SqlException: could not find stored procedure 'EXEC dbo.ProcName'`
- `CommandType.Text` + `EXEC ProcName @p` as text alternative
- RPC vs ad-hoc batch plan reuse distinction

**Answer**

With `CommandType.StoredProcedure`, the `CommandText` must be the bare schema-qualified procedure name only (e.g., `dbo.usp_GetOrder`) — adding `EXEC`, parentheses, or parameter placeholders causes `SqlException: could not find stored procedure 'EXEC dbo.usp_GetOrder'`. SQL Client sends an RPC call and binds parameters from the `Parameters` collection, not from the SQL text. Reserve `EXEC ProcName @p1, @p2` as a string for `CommandType.Text` only, and prefer `StoredProcedure` for production code because it generates more efficient plan reuse.

---

#### Gotcha 2. Output parameter `Direction` not set — value is never populated

**Concepts**
- `SqlParameter.Direction` defaults to `ParameterDirection.Input`
- output parameter with wrong direction is treated as input-only
- value not populated after `ExecuteNonQuery`/`ExecuteScalar`
- `ParameterDirection.Output` vs `ParameterDirection.InputOutput`
- `ParameterDirection.ReturnValue` for SP return codes

**Answer**

`SqlParameter.Direction` defaults to `ParameterDirection.Input` — if you forget to set it to `ParameterDirection.Output` for an output parameter, SQL Client sends the parameter as input-only and never reads the value back. After the command executes, `parameter.Value` will be `DBNull.Value` or the initial value you set rather than the value assigned by the stored procedure. Always explicitly set `Direction = ParameterDirection.Output` and also set `SqlDbType` and `Size` for string output parameters.

---

#### Gotcha 3. Reading output parameter value before closing the DataReader

**Concepts**
- output parameter values populated only after reader is closed
- reading output param while reader open returns stale/default value
- `ExecuteReader` + output params requires reader disposal first
- `ExecuteNonQuery` for non-result-set stored procedures
- reader `Close()`/`Dispose()` before accessing output param

**Answer**

When a stored procedure returns both a result set and output parameters, the output parameter values are not available until the `SqlDataReader` is closed. Reading `parameter.Value` while the reader is still open returns `DBNull.Value` or the initial default rather than the SP-assigned value. Always close or dispose the reader before accessing output parameter values: `using (var reader = cmd.ExecuteReader()) { while (reader.Read()) ... } var result = (int)outputParam.Value;`.

---

#### Gotcha 4. Return value parameter requires `ParameterDirection.ReturnValue` — not `Output`

**Concepts**
- SQL `RETURN @n` vs `OUTPUT` parameter distinction
- `ParameterDirection.ReturnValue` for SP integer return code
- return value always populated, even for scalar or no-result SPs
- conventional use for status codes (0 = success, non-zero = error)
- must be added to parameters before execute, not after

**Answer**

Stored procedures can return an integer via the `RETURN` statement, which is separate from `OUTPUT` parameters. To capture it, add a `SqlParameter` with `Direction = ParameterDirection.ReturnValue` and no name (or any name) to the command before executing. Using `ParameterDirection.Output` instead of `ReturnValue` fails to capture the RETURN value — output parameters only receive values from explicitly `SELECT`ed or `SET`-assigned variables in the procedure. The return value is typically used for status codes: 0 for success and non-zero for error conditions.

---

#### Gotcha 5. `AddWithValue` for output parameters — type inference cannot work

**Concepts**
- `AddWithValue` infers type from input value
- output parameter has no input value for inference
- zero-size string inferred for output `nvarchar` — truncation
- explicit `SqlDbType` and `Size` required for output params
- `AddWithValue` should never be used for output parameters

**Answer**

`SqlParameter.AddWithValue` infers the SQL type from the CLR value supplied at binding time — output parameters have no input value, so inference defaults to a zero-size or incorrect type, causing the output value to be truncated or empty. Always define output parameters explicitly with `new SqlParameter("@ResultName", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output }` — specifying `SqlDbType` and `Size` that match the stored procedure's declared parameter. This is one case where `AddWithValue` is simply not suitable.

---

#### Gotcha 6. CommandTimeout too short for long-running stored procedures

**Concepts**
- default `CommandTimeout` of 30 seconds
- long-running ETL, report, or bulk-operation stored procedures
- `SqlException: Execution Timeout Expired`
- server continues executing after timeout — no automatic rollback
- per-command timeout vs per-connection timeout distinction

**Answer**

The default `SqlCommand.CommandTimeout` of 30 seconds is too short for stored procedures that perform bulk operations, generate large reports, or execute long-running ETL logic. When the timeout fires, the `SqlException: Execution Timeout Expired` is thrown in the .NET client, but the SQL Server session may continue running the procedure until it completes or the session is killed separately. Always set `command.CommandTimeout` explicitly for long-running stored procedures, and consider adding a `CancellationToken` to allow the caller to cancel the operation cleanly.

---

#### Gotcha 7. Stored procedure name not schema-qualified — depends on caller's default schema

**Concepts**
- unqualified name (`ProcName`) resolved against caller's default schema
- different users with different default schemas call different procedures
- schema-qualified name (`dbo.ProcName`) is unambiguous
- case sensitivity on case-sensitive collation servers
- always use `schema.ProcedureName` in production code

**Answer**

Calling a stored procedure without schema qualification (e.g., `command.CommandText = "usp_GetOrders"` instead of `"dbo.usp_GetOrders"`) resolves against the executing user's default schema, which may differ between development and production database logins. In a case-sensitive collation environment, even `Dbo.usp_GetOrders` vs `dbo.usp_GetOrders` fails. Always use fully qualified names (`schema.ProcedureName`) in all `CommandText` values to ensure consistent resolution regardless of the database login used.

---

#### Gotcha 8. `ExecuteNonQuery` return value for stored procedures is the RETURN code, not rows affected

**Concepts**
- `ExecuteNonQuery` returns SP `RETURN` value, not rows affected
- `SET NOCOUNT ON` in SP makes row count unavailable
- confusion between `RETURN` code and rows affected
- `-1` returned when `SET NOCOUNT ON` is active
- output parameter or result set for actual row counts

**Answer**

For stored procedures, `ExecuteNonQuery` returns the SP's `RETURN` value (the integer returned by the `RETURN` statement), not the number of rows affected by DML inside the procedure. If the procedure uses `SET NOCOUNT ON` (common practice to suppress extra TDS result messages), `ExecuteNonQuery` returns `-1`. Do not rely on `ExecuteNonQuery`'s return value to determine rows affected inside a stored procedure — use an output parameter that the procedure explicitly sets, or check a dedicated `@@ROWCOUNT` output variable.

---

#### Gotcha 9. Stored procedure with dynamic SQL inside can still be vulnerable to injection

**Concepts**
- stored procedure encapsulation does not prevent injection inside SP
- `EXEC(@sql)` or `sp_executesql` with concatenation inside SP
- `sp_executesql` with typed parameters for dynamic SQL in SP
- calling SP parameterized from ADO.NET does not protect internal SP SQL
- security review must include SP code, not just ADO.NET callers

**Answer**

Wrapping a query in a stored procedure and calling it parameterized from ADO.NET prevents injection at the ADO.NET call boundary, but the procedure itself may concatenate user input into a dynamic `EXEC(@sql)` or `sp_executesql` call internally, creating injection inside the procedure. Parameterized calling from ADO.NET only protects the interface — the procedure's internal SQL construction must also use parameterized `sp_executesql` with properly typed parameters. Code review for injection must include the T-SQL source of any stored procedure that builds dynamic SQL internally.

---

#### Gotcha 10. Multiple result sets from a stored procedure — only first set accessible without `NextResult()`

**Concepts**
- `ExecuteReader` returns first result set by default
- `reader.NextResult()` advances to the next result set
- stored procedures returning multiple `SELECT` results
- `Dapper.QueryMultiple` vs manual `NextResult()` for multiple sets
- forgetting `NextResult()` leaves subsequent result sets unconsumed

**Answer**

A stored procedure can emit multiple `SELECT` result sets; `ExecuteReader` initially positions on the first set. To access the second and subsequent sets, call `reader.NextResult()` which advances the reader to the next result set and returns `true` if one exists. Forgetting to call `NextResult()` leaves subsequent result sets unconsumed on the network stream, which blocks the connection from processing further commands cleanly. With Dapper, `QueryMultiple` returns a `GridReader` with `Read<T>()` / `ReadAsync<T>()` calls for each successive result set.

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
