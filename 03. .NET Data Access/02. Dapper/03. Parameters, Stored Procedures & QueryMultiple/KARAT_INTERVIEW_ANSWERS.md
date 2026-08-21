# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/02. Dapper/03. Parameters, Stored Procedures & QueryMultiple`

---

#### Q1. (R) A junior developer ships a product search endpoint backed by this Dapper repository method. Security review flags it before deploy. What is wrong, and how do you fix it without changing the public method signature?

**Answer:** The SQL is built with C# string interpolation, so user input is concatenated into the command text — classic SQL injection (`'; DROP TABLE dbo.Products; --`). Dapper only parameterizes when you pass a separate param object; interpolated values are literal T-SQL.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | `searchTerm` embedded in SQL via `$"..."` | SQL injection; attacker can read/modify data |
| Correctness | No `@searchTerm` token bound by Dapper | Provider sends one non-parameterized batch |
| Design | Looks like Dapper but bypasses parameter API | False sense of safety in code review |

**Fix (priority order):**

1. Replace interpolation with a parameterized predicate: `WHERE ProductName LIKE @Pattern` and pass `new { Pattern = $"%{searchTerm}%" }` (or `'%' + @term + '%'` pattern built in C# as the **parameter value**, not in SQL text).
2. Optionally add input length/character validation at the API layer — defense in depth, not a substitute for parameters.
3. Log and monitor for suspicious search strings; consider `CommandDefinition` with a fixed timeout for search endpoints.

**Production takeaway:** Dapper does not auto-sanitize interpolated SQL — only named parameters (`new { ... }`, `DynamicParameters`) get sent as `SqlParameter` values. See ADO.NET ch.03 SqlCommand & Parameters for the same rule on raw ADO.NET.

---

#### Q2. (R) Code review on a stored-procedure insert path that mirrors `DynamicParameterRepository.InsertProduct`. The author claims it works in a one-off SSMS test. What breaks at runtime or in production, and in what order do you fix it?

**Answer:** OUTPUT parameters are populated **after** `Execute` completes; reading `@NewProductId` beforehand returns default `0`. The `Add` call also omits `dbType: DbType.Int32`, which can cause provider inference issues for Output parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / correctness | `dp.Get<int>("@NewProductId")` before `Execute` | Always returns 0; wrong IDs returned to callers |
| ADO.NET semantics | Output without explicit `DbType` | Possible type/size mismatch on some providers |
| Data integrity | Caller may attach downstream FKs to id `0` | Silent corruption or constraint violations |

**Fix (priority order):**

1. Move `dp.Get<int>("@NewProductId")` to **after** `connection.Execute(...)`.
2. Declare Output explicitly: `dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output)`.
3. Add a guard: if `newId <= 0` after execute, treat as failure and log — do not propagate bogus keys.

Correct pattern (matches this chapter's `DynamicParameterRepository`):

```csharp
dp.Add("@NewProductId", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_InsertProduct", dp, commandType: CommandType.StoredProcedure);
return dp.Get<int>("@NewProductId");
```

**Production takeaway:** SSMS manual runs hide ordering bugs — Dapper mirrors ADO.NET "execute then read Output/ReturnValue." Karat often stacks direction + read-timing traps in one snippet.

---

#### Q3. (R) A dashboard API calls `usp_GetProductDashboard` but returns wrong totals after a DBA reorders the SELECT statements inside the procedure. Review this consumer — what fails silently vs throws, and how do you harden it?

**Answer:** `GridReader.Read<T>()` is **order-dependent** and forward-only — it always consumes the next result set in the batch. Reordering proc result sets maps columns to the wrong CLR types; some mismatches throw at read time, others produce garbage counts or empty strings silently.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Assumes fixed result-set order (count → products → name) | Wrong dashboard numbers after harmless proc change |
| Contract | No versioned proc/consumer agreement | DBA refactor breaks API without compile error |
| Resilience | `Read<int>()` on a product-shaped first set | Mapping exceptions or nonsense `int` values |

**Fix (priority order):**

1. Align consumer read order with the **documented** proc contract — match this chapter's `QueryMultipleRepository.GetDashboardFromProcedure` (products → count → name).
2. Document result-set order in proc header comment and integration test (assert column counts/shapes per `Read` call).
3. Prefer stable column aliases in each SELECT (`AS TotalCount`, `AS MostExpensive`) and consider separate procs or JSON single-result if order churn is frequent.
4. On shape mismatch, catch read exceptions, log proc name + set index, return 503 — do not silently show wrong totals.

**Production takeaway:** `QueryMultiple` saves round trips but couples client and server on **set order** — same constraint as `SqlDataReader.NextResult()` in ADO.NET ch.04. `StoredProcedureRepository.GetAllViaStoredProcedure` shows `Query<T>` only reads the **first** set — a related footgun.

---

#### Q4. (M) A filter query returns every row instead of the intended price band. The SQL and anonymous object look correct at a glance. What is the binding bug?

**Answer:** Dapper binds by **name**: SQL tokens `@MinPrice` and `@MaxPrice` require properties `MinPrice` and `MaxPrice`. The anonymous object uses `MinimumPrice` / `MaximumPrice`, so those parameters are never supplied — SQL Server may treat missing predicates as unconstrained or error depending on plan, often returning all rows.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Property names ≠ `@MinPrice` / `@MaxPrice` | Filter ignored; full table scan |
| Performance | Unfiltered query | Memory pressure, slow API, wrong business results |
| Maintainability | "Looks parameterized" but misbound | Hard to spot in review |

**Fix (priority order):**

1. Rename properties to match SQL: `new { MinPrice = minPrice, MaxPrice = maxPrice }` — as in `AnonymousParameterRepository.GetByPriceRange`.
2. Or alias in SQL: `@MinimumPrice` / `@MaximumPrice` to match the object.
3. Add an integration test with known seed data asserting row count within band.

**Production takeaway:** Parameterization prevents injection but **name alignment** is still required — case-insensitive match, exact token names. See ADO.NET ch.03 parameter naming and this chapter's Section 2 comments.

---

#### Q5. (P) An ASP.NET Core endpoint wraps a long-running report stored procedure. The controller passes `HttpContext.RequestAborted` as cancellation. Review the repository — what is missing for timeout and cooperative cancellation, and how would you wire `CommandDefinition` correctly?

**Answer:** The snippet uses default command timeout (often 30s on `SqlConnection`) and does not pass `CancellationToken` into Dapper — client disconnect will not cancel the SQL command, and long procs can hold pool connections until default timeout.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Hosting / scalability | No `commandTimeout` override | Report exceeds 30s → SqlException; pool exhaustion under load |
| Async / cancellation | `QueryAsync` without token in `CommandDefinition` | Request aborted but DB work continues |
| Operations | No explicit timeout policy per report | Unpredictable SLA; hung requests |

**Fix (priority order):**

1. Build a `CommandDefinition` with explicit timeout and cancellation:

```csharp
var cmd = new CommandDefinition(
    "dbo.usp_SlowProductReport",
    parameters: null,
    commandTimeout: 120, // seconds — tune to SLA
    commandType: CommandType.StoredProcedure,
    cancellationToken: cancellationToken);

return (await connection.QueryAsync<Product>(cmd)).AsList();
```

2. Open connection with `OpenAsync(cancellationToken)` (already present) and ensure the controller passes `HttpContext.RequestAborted`.
3. Map `OperationCanceledException` / `SqlException` timeout number `-2` to 499/504 with structured logging.
4. For very heavy reports, switch to async job + polling rather than stretching timeout indefinitely.

**Production takeaway:** `CommandDefinition` is the Dapper hook for timeout, `CommandType`, transaction, flags, and **cancellation** — same bundle this chapter's `CommandDefinitionRepository` uses for `commandTimeout: 30`. See Dapper ch.02 for async overloads.

---

#### Q6. (D) A teammate refactors `GetProductCountViaReturnValue` to use an anonymous object because "ReturnValue is just another int param." They change the proc caller to:

```csharp
var count = connection.ExecuteScalar<int>(
    "dbo.usp_GetProductCount",
    commandType: CommandType.StoredProcedure);
```

Separately, another method tries to read a T-SQL `RETURN` with:

```csharp
var dp = new DynamicParameters(new { ProductId = id });
dp.Add("@Status", dbType: DbType.Int32, direction: ParameterDirection.Output);
connection.Execute("dbo.usp_TryGetProductName", dp, commandType: CommandType.StoredProcedure);
int status = dp.Get<int>("@Status"); // proc uses RETURN 0 / RETURN 1, not OUTPUT @Status
```

Explain what each approach gets wrong about T-SQL `RETURN` vs `OUTPUT`, and what Dapper API you use for each.

**Answer:** T-SQL `RETURN` sends an integer through a **ReturnValue** parameter, not through result sets or arbitrary OUTPUT params. `ExecuteScalar` reads the first result set's first column — `usp_GetProductCount` has no SELECT, so scalar is wrong. The second snippet treats `RETURN 1` as `@Status OUTPUT`, which never receives the return code.

- **`RETURN` (status code):** `DynamicParameters` with `direction: ParameterDirection.ReturnValue` and a dummy name like `@ReturnValue`, then `dp.Get<int>("@ReturnValue")` after `Execute` — as in `DynamicParameterRepository.GetProductCountViaReturnValue` and `TryGetProductName`.
- **`OUTPUT` parameter:** `@NewProductId INT OUTPUT` or `@ProductName NVARCHAR(100) OUTPUT` — `dp.Add` with `ParameterDirection.Output`, explicit `DbType` and **size for strings** (`size: 100`), read after execute — as in `InsertProduct` / `GetProductName`.
- **`ExecuteScalar`:** Use when the proc or batch **SELECTs** a single value (e.g. `SELECT COUNT(*)`), not for `RETURN`.
- **Anonymous objects:** Fine for **Input** only — cannot declare Output/ReturnValue direction; use `DynamicParameters` or `AddDynamicParams` plus directional adds (see `TryGetProductName` merging input via `AddDynamicParams`).

**Production takeaway:** Karat tests whether you know ADO.NET parameter directions under Dapper syntax — `RETURN`, `OUTPUT`, and result sets are three different channels. Confusing them passes compile and fails in QA with status always 0.

---

#### Q7. (R) A batch import service loads a dashboard in one round trip but intermittently throws `InvalidOperationException` under load. Review the method — identify lifetime/reader issues and the fix.

**Answer:** The method returns a **deferred** `IEnumerable<Product>` from `multi.Read<Product>()` while the `GridReader`, connection, and reader are disposed when the method exits. When the caller enumerates `products`, the underlying reader is closed — classic "reader is closed" / invalid operation intermittently depending on timing.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | No `using` on connection / `GridReader` | Connection leak under load; pool starvation |
| Reader lifetime | Deferred `IEnumerable` outlives `multi` | `InvalidOperationException` on enumeration |
| QueryMultiple rules | Must finish all `Read` calls and materialize before dispose | Partial reads leave reader in bad state |

**Fix (priority order):**

1. Wrap connection and grid reader in `using` — pattern from `QueryMultipleRepository.GetDashboardFromInlineBatch`.
2. Materialize before return: `var products = multi.Read<Product>().AsList();` (or `.ToList()`).
3. Complete all `Read` calls in order before leaving the `using` block.
4. Return `IReadOnlyList<Product>` (or DTO) — not live `IEnumerable` tied to SQL reader.

```csharp
using var connection = new SqlConnection(_connectionString);
using var multi = connection.QueryMultiple(sql);
var products = multi.Read<Product>().AsList();
int total = multi.Read<int>().Single();
string top = multi.ReadFirst<string>();
return (products, total, top);
```

**Production takeaway:** Dapper's default `Query` buffering does not apply the same way if you return unmaterialized sequences from `QueryMultiple`. Forward-only `GridReader` semantics match ADO.NET `NextResult` — consume and buffer inside the `using` scope.
