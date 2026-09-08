# SqlCommand & Parameters — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [SqlCommand & Parameters](#chapter-03-sqlcommand--parameters)
  - [Q1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?](#q1-what-is-the-difference-between-executereader-executenonquery-and-executescalar)
  - [Q2. What is a parameterized query, and why is it preferred over string concatenation?](#q2-what-is-a-parameterized-query-and-why-is-it-preferred-over-string-concatenation)
  - [Q3. What is SQL injection, and how do parameters prevent it?](#q3-what-is-sql-injection-and-how-do-parameters-prevent-it)
  - [Q4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?](#q4-what-is-the-difference-between-addwithvalue-and-explicitly-typed-sqlparameter)
  - [Q5. What is the difference between `Text` and `StoredProcedure` command types?](#q5-what-is-the-difference-between-text-and-storedprocedure-command-types)
  - [Q6. When would you use `ExecuteScalar` instead of `ExecuteReader`?](#q6-when-would-you-use-executescalar-instead-of-executereader)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 03. SqlCommand & Parameters

### Q1. What is the difference between `ExecuteReader`, `ExecuteNonQuery`, and `ExecuteScalar`?

**Concepts**
- `ExecuteReader` — returns `DbDataReader` for rowsets
- `ExecuteNonQuery` — DML/DDL, returns rows affected
- `ExecuteScalar` — returns first column of first row
- All honor the same parameters, transaction, and timeout

**Answer**

`ExecuteReader` runs a SELECT and returns a forward-only `DbDataReader` for streaming one or more result sets. `ExecuteNonQuery` runs INSERT, UPDATE, DELETE, or DDL and returns the number of rows affected — not a rowset. `ExecuteScalar` returns the first column of the first row, making it ideal for aggregates like `COUNT(*)` or identity lookups such as `SCOPE_IDENTITY()`.

---

### Q2. What is a parameterized query, and why is it preferred over string concatenation?

**Concepts**
- SQL placeholders (`@Name`, `@Id`) with `SqlParameter` values
- User input treated as data, not executable SQL
- Consistent statement text enables plan caching
- Eliminates injection, escaping, and culture bugs

**Answer**

A parameterized query sends SQL with typed placeholders and supplies values separately through `SqlParameter` objects, so the database engine treats user input as data rather than executable text. The SQL shape stays constant across executions, enabling query plan caching and preventing SQL injection. Identifiers like table and column names still cannot be parameterized — those require strict server-side allowlists.

---

### Q3. What is SQL injection, and how do parameters prevent it?

**Concepts**
- Untrusted input concatenated into SQL as executable text
- Parameters send values out-of-band from command text
- Applies to ADO.NET, Dapper, and EF Core raw SQL
- Parameters protect values, not dynamic identifiers

**Answer**

SQL injection is an attack where untrusted input is concatenated into SQL and interpreted as commands — for example, `'; DROP TABLE Users;--` closing the original query and appending a destructive statement. Parameters send values separately from command text so the engine treats them as data literals and never parses them as SQL. Parameters protect values only; dynamic identifiers like table names still require strict server-side allowlists.

---

### Q4. What is the difference between `AddWithValue` and explicitly typed `SqlParameter`?

**Concepts**
- `AddWithValue` infers type/size from CLR value at runtime
- Explicit `SqlParameter` specifies `SqlDbType`, `Size`, `Precision`
- Type mismatch prevents index seeks and bloats plan cache
- Nullable values require `DBNull.Value` explicitly

**Answer**

`AddWithValue` infers the parameter type and size from the CLR value, which can mismatch the column — for example, sending `nvarchar(4000)` for a `varchar(50)` column, preventing index seeks. Explicit `SqlParameter` with `SqlDbType`, `Size`, and `Precision`/`Scale` ensures the provider sends exactly what the column expects, avoiding implicit conversions and plan cache bloat. Production code should use explicit parameters; `AddWithValue` is only acceptable for quick prototypes with well-known small types.

---

### Q5. What is the difference between `Text` and `StoredProcedure` command types?

**Concepts**
- `CommandType.Text` — sends raw SQL/T-SQL batch
- `CommandType.StoredProcedure` — invokes proc by name
- Output/return parameters cleaner with StoredProcedure mode
- Proc can encapsulate EXEC permissions on tables

**Answer**

`CommandType.Text` sends `CommandText` as raw SQL text executed by the server. `CommandType.StoredProcedure` sets `CommandText` to the procedure name and binds parameters to its signature, letting the provider generate the correct RPC call. Output parameters and return values work cleanly in `StoredProcedure` mode, and procedures can encapsulate permissions — granting `EXEC` without exposing underlying tables.

---

### Q6. When would you use `ExecuteScalar` instead of `ExecuteReader`?

**Concepts**
- Single-value result: COUNT, MAX, SCOPE_IDENTITY
- Avoids reader allocation and loop overhead
- Handle null/DBNull when zero rows possible
- Use ExecuteReader when multiple rows or columns needed

**Answer**

Use `ExecuteScalar` when the query is guaranteed to return exactly one value — a count, aggregate, flag, or newly generated identity — and iterating a reader would be unnecessary overhead. `SELECT COUNT(*) FROM Orders WHERE Status = @status` or `SELECT CAST(SCOPE_IDENTITY() AS int)` after an INSERT are classic cases. Cast the result carefully, since the return is `object?` and can be `null` or `DBNull` when no rows match.

---

## Gotchas — SqlCommand & Parameters (Interview Traps)

---

#### Gotcha 1. String concatenation in SQL enables injection

**Concepts**
- SQL injection via string interpolation or concatenation
- `@placeholder` parameterization as the prevention
- `ADO.NET` has no automatic sanitization
- plan cache pollution from dynamic literal strings
- blocking defect in code review

**Answer**

Building SQL with C# string interpolation (`$"WHERE Id = {id}"`) or concatenation sends user input as literal SQL text, enabling SQL injection and preventing query plan reuse. ADO.NET does not sanitize input automatically — every user-supplied value must be bound through a `SqlParameter` with an explicit `@placeholder` and the value set separately. Code review should flag any dynamic SQL string construction without parameter placeholders as a blocking security defect.

---

#### Gotcha 2. `AddWithValue` infers wrong SQL types — index scans and plan pollution

**Concepts**
- `AddWithValue` CLR-to-SQL type inference
- `string` inferred as `nvarchar(len)` — oversized, breaks seeks
- implicit conversion prevents index seek
- `decimal` precision and scale not inferred correctly
- explicit `SqlDbType`, size, and precision as the fix

**Answer**

`SqlParameter.AddWithValue` infers the SQL type from the CLR value, which frequently does not match the database column definition. A `string` parameter is typically inferred as `nvarchar(4000)` or `nvarchar(max)`, while the column may be `varchar(100)` — causing an implicit type conversion that prevents an index seek and forces a full scan. For `decimal` parameters the scale is often wrong. Always use an explicit `SqlParameter` specifying `SqlDbType`, `Size`, and `Precision`/`Scale` that exactly match the column, especially on columns involved in index seeks.

---

#### Gotcha 3. ExecuteNonQuery returns -1 for SELECT statements

**Concepts**
- `ExecuteNonQuery` return value for non-DML statements
- `-1` returned for SELECT, `SET NOCOUNT ON` stored procedures
- rows affected vs scalar result distinction
- `ExecuteScalar` for single-value results
- `ExecuteReader` for row result sets

**Answer**

`ExecuteNonQuery` returns the number of rows affected by INSERT, UPDATE, or DELETE, but returns `-1` for SELECT statements and for stored procedures that begin with `SET NOCOUNT ON`. Relying on the return value of `ExecuteNonQuery` to detect whether a SELECT found a row is always wrong — use `ExecuteScalar` for a single-value result or `ExecuteReader` for a result set. Check the `ExecuteNonQuery` return value only on DML commands where "0 rows affected" has explicit business meaning (e.g., optimistic concurrency conflict detection).

---

#### Gotcha 4. ExecuteScalar returns DBNull.Value — direct cast throws

**Concepts**
- `ExecuteScalar` returns `object?`
- `DBNull.Value` for `MAX`/`SUM`/`MIN` on empty result set
- `COUNT(*)` always returns integer, not null
- `InvalidCastException` on direct `(int)` cast of `DBNull`
- null-coalescing pattern with `as int?`

**Answer**

`ExecuteScalar` returns `object?` — casting it directly to `int` throws `InvalidCastException` when the result is `DBNull.Value`, which occurs for aggregate functions like `MAX` or `SUM` on an empty result set. `COUNT(*)` always returns a non-null integer, but scalar subqueries or aggregates with no matching rows return `DBNull`. Use `result is DBNull ? 0 : (int)result` or `(result as int?) ?? 0` to handle both cases safely. Always null-check before casting any value returned from `ExecuteScalar`.

---

#### Gotcha 5. CommandType.Text used to call a stored procedure — RPC vs batch

**Concepts**
- `CommandType.StoredProcedure` sends RPC call
- `CommandType.Text` with `EXEC ProcName` sends batch statement
- plan reuse difference between RPC and ad-hoc batch
- `CommandText` must be procedure name only with `StoredProcedure`
- `EXEC ProcName @p` as text vs RPC parameter binding

**Answer**

When `CommandType.StoredProcedure` is set, `CommandText` must contain only the schema-qualified procedure name (e.g., `dbo.usp_GetOrder`) — adding `EXEC` or parentheses causes `SqlException: could not find stored procedure 'EXEC dbo.usp_GetOrder'`. With `CommandType.Text` and `EXEC ProcName @p1, @p2`, SQL Server treats the whole string as an ad-hoc batch, which has inferior plan-reuse characteristics compared to the RPC call generated by `CommandType.StoredProcedure`. Choose `StoredProcedure` with bound `SqlParameter` objects for all production procedure calls.

---

#### Gotcha 6. Parameter collection not cleared when reusing SqlCommand

**Concepts**
- `SqlCommand.Parameters` accumulates across calls
- duplicate parameter names throw `ArgumentException`
- reuse pattern requires explicit `Parameters.Clear()`
- new `SqlCommand` per operation as safer pattern
- connection reuse vs command reuse distinction

**Answer**

If a `SqlCommand` instance is reused across multiple calls without clearing its `Parameters` collection, adding the same parameter name a second time throws `ArgumentException: The SqlParameter is already contained by another SqlParameterCollection`. The safest pattern is to create a new `SqlCommand` instance per operation rather than reusing one, which also avoids subtle bugs from stale parameter values. If reuse is required for performance, call `command.Parameters.Clear()` before adding parameters for the next execution.

---

#### Gotcha 7. CommandTimeout too short for bulk or reporting operations

**Concepts**
- `CommandTimeout` default of 30 seconds
- long-running bulk inserts, reports, and index operations
- `SqlException: Execution Timeout Expired`
- `CommandTimeout = 0` for unlimited (use with care)
- per-command timeout vs connection-level setting

**Answer**

The default `SqlCommand.CommandTimeout` is 30 seconds, which is appropriate for transactional CRUD but far too short for bulk inserts, complex aggregate reports, or maintenance operations like index rebuilds. When these commands time out in production they throw `SqlException: Execution Timeout Expired`, and the partial work already committed by the server is not automatically rolled back unless the command was inside a transaction. Set `command.CommandTimeout = 300` (or higher) for known long operations; use `0` for unlimited timeout only on maintenance commands that must run to completion without interference.

---

#### Gotcha 8. Using `SqlDbType.NVarChar` for `varchar` column causes implicit conversion

**Concepts**
- Unicode `nvarchar` vs non-Unicode `varchar` parameter mismatch
- implicit conversion prevents index seek
- collation-aware comparison change with Unicode input
- parameter type must match column type exactly
- performance regression detectable in execution plan

**Answer**

Passing a parameter with `SqlDbType.NVarChar` for a column defined as `varchar` forces SQL Server to perform an implicit conversion on every row comparison, which prevents an index seek and degrades to a full index scan. The execution plan will show an implicit conversion warning on the column predicate. Always match the parameter `SqlDbType` to the exact column type: use `SqlDbType.VarChar` for `varchar` columns and set `Size` to the column's defined length to ensure the parameter binding is identical to the column definition.

---

#### Gotcha 9. Missing `SqlCommand.Transaction` assignment — command runs outside the transaction

**Concepts**
- `SqlCommand.Transaction` property must be set explicitly
- command not enrolled in transaction auto-commits
- partial rollback when transaction fails
- each `SqlCommand` needs the same transaction reference
- `BeginTransaction` return value must be held and assigned

**Answer**

Creating a `SqlTransaction` via `connection.BeginTransaction()` does not automatically enroll subsequent `SqlCommand` objects — each command's `Transaction` property must be set explicitly to the same transaction instance. Forgetting to assign `command.Transaction = tx` causes that command to execute under implicit autocommit, meaning it commits immediately even if the transaction later rolls back. Always hold the `SqlTransaction` reference returned by `BeginTransaction` and assign it to every command that must participate in the unit of work.

---

#### Gotcha 10. `SqlParameter.Size` not set for output parameters — truncation at default 0

**Concepts**
- output parameter `Size` defaults to 0 for string/binary types
- zero-size output truncates or returns empty value
- `Direction = ParameterDirection.Output` requires explicit `Size`
- `AddWithValue` cannot be used for output parameters
- explicit `SqlDbType`, `Size`, and `Direction` required

**Answer**

For output parameters of type `nvarchar`, `varchar`, or `varbinary`, the `SqlParameter.Size` property must be set explicitly — the default value of 0 causes the parameter to receive an empty or truncated result even when the stored procedure writes a full string. `AddWithValue` cannot be used for output parameters because type inference only works for input binding. Always define output parameters as `new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Direction = ParameterDirection.Output }` with `Size` matching the maximum expected value length.

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A junior developer ships a product search endpoint. Review the repository method:

```csharp
public IReadOnlyList<ProductDto> SearchByName(SqlConnection connection, string searchTerm)
{
    string sql = """
        SELECT ProductId, Name, UnitPrice, Stock
        FROM dbo.Products
        WHERE IsActive = 1 AND Name LIKE '%" + searchTerm + "%'
        ORDER BY Name;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    using SqlDataReader reader = command.ExecuteReader();
    // ... map rows to ProductDto ...
}
```

The method works in QA with `"Mouse"` and `"Keyboard"`. What breaks in production, how would an attacker exploit it, and what is the minimal safe fix?

---

**Answer:** The method embeds user input directly into SQL text, which is a classic **SQL injection** vulnerability. Benign searches work in QA, but an attacker can alter query logic — returning every row, bypassing filters, or worse if the DB login is over-privileged.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | String concatenation of `searchTerm` into `CommandText` | Attacker input becomes executable SQL, not data |
| Correctness | Statement shape changes per request | Optimizer treats each string as a new ad-hoc batch; harder to audit and cache |
| Operational | No parameter binding | Logging/monitoring cannot safely redact or classify the value separately from SQL |

**Fix (priority order):**

1. **Never concatenate user input** — use a fixed SQL template with a placeholder, as in `ProductRepository.DemonstrateSqlInjectionPrevention`:
   ```csharp
   const string sql = """
       SELECT ProductId, Name, UnitPrice, Stock
       FROM dbo.Products
       WHERE IsActive = 1 AND Name LIKE @pattern
       ORDER BY Name;
       """;
   command.Parameters.Add(new SqlParameter("@pattern", SqlDbType.NVarChar, 100)
       { Value = $"%{searchTerm}%" });
   ```
2. Validate/limit `searchTerm` length at the API boundary (match column `NVARCHAR(100)`).
3. Ensure the DB account has least privilege — no `DROP`, no `xp_cmdshell` — defense in depth only; parameters are the real fix.

**Production takeaway:** Parameters send **values** separately from **command text**; SQL Server compiles a stable statement shape. See **ProductRepository.cs** Section 6 and **Program.cs** QUICK REFERENCE security block.

---

#### Q2. (R) A reporting query returns wrong rows for a price filter. Review:

```csharp
public decimal GetAverageAbovePrice(SqlConnection connection, decimal minPrice)
{
    const string sql = """
        SELECT AVG(UnitPrice)
        FROM dbo.Products
        WHERE IsActive = 1 AND UnitPrice >= @minPrice;
        """;

    using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql);
    command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Int) { Value = minPrice });

    object? scalar = command.ExecuteScalar();
    return scalar is null or DBNull ? 0m : Convert.ToDecimal(scalar);
}
```

`minPrice = 19.99m` is passed from the API. What is wrong, what symptom might QA miss, and how should the parameter be declared?

---

**Answer:** `@minPrice` is declared as `SqlDbType.Int` but the column and CLR value are **decimal/money** scale. SQL Server coerces or truncates the comparison, so the average can include or exclude rows incorrectly — QA might only test whole-dollar thresholds (`20`, `50`) and miss fractional prices.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `SqlDbType.Int` for a `DECIMAL(10,2)` column | Implicit conversion/truncation skews `>=` filter |
| Data integrity | No `Precision`/`Scale` on decimal param | Rounding mismatches vs column definition |
| Silent failure | Query executes without error | Wrong aggregates ship to dashboards |

**Fix (priority order):**

1. Match the column type — mirror `InsertProduct` in **ProductRepository.cs**:
   ```csharp
   command.Parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal)
   {
       Value = minPrice,
       Precision = 10,
       Scale = 2,
   });
   ```
2. Add tests with fractional boundaries (`19.99`, `19.995`, `20.00`).
3. Prefer explicit `SqlDbType` over `AddWithValue` for money/decimal paths.

**Production takeaway:** Wrong `SqlDbType` rarely throws — it **silently changes semantics**. Always align parameter type, precision, and scale with the table definition.

---

#### Q3. (P) Your API receives product names of varying length (5–80 characters). A teammate uses `AddWithValue` everywhere "because it works":

```csharp
command.Parameters.AddWithValue("@name", userSuppliedName);
// SQL: WHERE Name = @name  — column is NVARCHAR(100)
```

Explain what happens inside SQL Server's plan cache when thousands of distinct name lengths hit this query. Why does the chapter prefer explicit `SqlDbType.NVarChar` with `Size`, and when is `AddWithValue` still acceptable?

---

**Answer:** `AddWithValue` infers parameter type and **length from the runtime CLR value**, so each distinct string length can produce a **different parameter metadata signature** in cached plans — plan-cache **bloat** and **compile churn**. Explicit `NVarChar(100)` stabilizes the signature to match the column.

- **Mechanism:** For strings, the provider typically maps to `NVARCHAR(n)` where `n` is the string's current length (or `NVARCHAR(MAX)` for very long values). SQL Server auto-parameterizes by **type + length + precision**; `@name nvarchar(12)` and `@name nvarchar(47)` are different cache keys.
- **Performance impact:** Thousands of unique lengths → many near-identical plans, higher CPU on compilations, pressure on the plan cache — the pitfall demonstrated in **ProductRepository.DemonstrateAddWithValuePitfall**.
- **Index use:** If inference yields `NVARCHAR(4000)`/`MAX` while the column is `NVARCHAR(100)`, cardinality estimates and seek vs scan choices can degrade.
- **Chapter preference:** `new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = name }` — fixed size aligned to the schema, as in **InsertProduct** and **DemonstrateTypedParameters**.
- **When `AddWithValue` is OK:** One-off admin scripts, local prototypes, non-string types with unambiguous inference (e.g., `SqlDbType.Int` for a literal `42`), or parameters where length truly varies by design and you accept the cache trade-off.

**Production takeaway:** Production ADO.NET should treat parameter metadata as part of the **contract with SQL Server** — explicit `SqlDbType` + `Size`/`Precision`/`Scale`, not convenience inference.

---

#### Q4. (P) An ASP.NET Core admin export runs a heavy `SELECT` through ADO.NET. In staging it completes in 45 seconds; in production it intermittently throws:

```
SqlException: Execution Timeout Expired. The timeout period elapsed...
```

The developer sets `command.CommandTimeout = 0` on that command "so exports never fail." What are the risks of that fix, and what production pattern would you use instead?

---

**Answer:** `CommandTimeout = 0` means **wait indefinitely** — exports stop failing fast but can hold connections, thread-pool threads, and SQL Server resources until someone kills the session. That is a reliability anti-pattern under load.

- **Default behavior:** `CommandTimeout` defaults to **30 seconds** per command (**ProductRepository.DemonstrateCommandTimeout**); timeout throws `SqlException` (often error number **-2**).
- **Risk of `0`:** Hung queries tie up pooled `SqlConnection` instances → pool exhaustion → site-wide outages; no back-pressure for runaway reports.
- **Staging vs prod gap:** Prod has more data, blocking, and IO latency — 45s work exceeds default 30s intermittently; the right response is not "infinite wait."
- **Better patterns:**
  - Set a **bounded** timeout above p99 runtime (e.g., 120s) for that command only.
  - Move long exports to a **background job** (queue + blob storage) with cancellation.
  - Use **async** execute + `CancellationToken` (ch08) so client disconnect cancels the command.
  - Tune/index the query; consider read-only replica for reporting.
  - Surface **504/202 Accepted** to the client instead of blocking an HTTP request for minutes.

**Production takeaway:** Timeouts are **failure detectors**, not annoyances to disable — tune per command, fail fast on OLTP paths, offload unbounded work.

---

#### Q5. (R) A stored procedure `dbo.usp_ProductCount` exists (see `Program.cs` setup). A teammate copies the preview pattern but changes the call:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "EXEC dbo.usp_ProductCount";   // inline EXEC string
command.CommandType = CommandType.Text;              // default — not changed

object? count = command.ExecuteScalar();
```

Meanwhile another path calls the same proc correctly with `CommandType.StoredProcedure` and `CommandText = "dbo.usp_ProductCount"`. What differs at runtime, and what breaks when they add an `@CategoryId` filter parameter to the proc?

---

**Answer:** `CommandType.Text` with `EXEC ...` sends an **ad-hoc batch**; `CommandType.StoredProcedure` invokes the proc through the **RPC interface** with metadata lookup. Both may return the same scalar today, but parameter binding, plan reuse, and permissions diverge once the proc gains parameters.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| API contract | `Text` + `EXEC` vs `StoredProcedure` RPC | Different server handling, quoting, and parameter discovery |
| Maintainability | Inline `EXEC` string duplicates proc name | Refactors to signature break at runtime, not compile time |
| Correctness | Adding `@CategoryId` to proc | `Text` path may ignore unbound params or require manual `EXEC ... @CategoryId = @p` string; easy to mis-order or inject |
| Security/perf | Ad-hoc EXEC batch | Loses some RPC benefits; harder to grant `EXECUTE` on proc only vs ad-hoc SQL |

**Fix (priority order):**

1. Use the **StoredProcedurePreview** pattern from **CommandPreviews.cs**:
   ```csharp
   command.CommandText = "dbo.usp_ProductCount";
   command.CommandType = CommandType.StoredProcedure;
   ```
2. When `@CategoryId` is added, bind explicitly:
   ```csharp
   command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId });
   ```
3. Drop the `EXEC dbo.usp_ProductCount` string form in application code.

**Production takeaway:** Procedure name in `CommandText` + `CommandType.StoredProcedure` is the ADO.NET contract for procs; reserve `CommandType.Text` for ad-hoc SQL only.

---

#### Q6. (R) An inventory sync proc returns the new stock via an output parameter. Review:

```csharp
using SqlCommand command = connection.CreateCommand();
command.CommandText = "dbo.usp_AdjustStock";
command.CommandType = CommandType.StoredProcedure;

command.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = productId });
command.Parameters.Add(new SqlParameter("@Delta", SqlDbType.Int) { Value = delta });

var output = new SqlParameter("@NewStock", SqlDbType.Int) { Value = 0 };
command.Parameters.Add(output);

command.ExecuteNonQuery();
return (int)output.Value;
```

After deploy, `@NewStock` is always `0` even though the row updates correctly. Diagnose the defect and show the corrected parameter setup.

---

**Answer:** Output parameters must declare **`ParameterDirection.Output`** (or `InputOutput`). Without it, ADO.NET treats `@NewStock` as an **input-only** parameter; the initial `Value = 0` is sent to SQL Server and the returned output is never written back to `output.Value`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Missing `Direction = ParameterDirection.Output` | Client always reads the seeded `0`, not the proc's `SET @NewStock = …` |
| Runtime | No exception thrown | Silent wrong inventory counts in downstream systems |
| Design | Confusing input/output semantics | Same bug on `ReturnValue` if confused with output params |

**Fix (priority order):**

1. Set direction on the output parameter:
   ```csharp
   var output = new SqlParameter("@NewStock", SqlDbType.Int)
   {
       Direction = ParameterDirection.Output,
   };
   command.Parameters.Add(output);
   command.ExecuteNonQuery();
   return (int)output.Value!;  // Value populated after execute
   ```
2. Optionally use `ReturnValue` for proc return codes — separate parameter with `Direction = ParameterDirection.ReturnValue` (ch07 depth).
3. Integration-test the proc path, not only row side effects.

**Production takeaway:** For stored procedures, every non-input parameter needs an explicit **`ParameterDirection`** — output, input-output, or return value.

---

#### Q7. (R) A search builder constructs dynamic `WHERE` clauses at runtime:

```csharp
var sql = new StringBuilder("SELECT ProductId, Name FROM dbo.Products WHERE IsActive = 1");
var parameters = new List<SqlParameter>();

if (!string.IsNullOrEmpty(nameFilter))
{
    sql.Append(" AND Name LIKE @name");
    parameters.Add(new SqlParameter("name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
}
if (minPrice.HasValue)
{
    sql.Append(" AND UnitPrice >= @minPrice");
    parameters.Add(new SqlParameter("@minPrice", SqlDbType.Decimal) { Value = minPrice.Value, Precision = 10, Scale = 2 });
}

using SqlCommand command = CommandFactory.CreateTextCommand(connection, sql.ToString());
foreach (var p in parameters)
    command.Parameters.Add(p);
```

The query runs but `@name` never binds — every search returns unfiltered rows. What naming rule was violated, and how do you prevent this class of bug across dynamic SQL builders?

---

**Answer:** SQL placeholders use **`@name`** in `CommandText`, but the parameter was created as `"name"` without the **`@` prefix**. SQL Server parameter names must match; the orphan `@name` in SQL is unbound, so the `LIKE` predicate does not filter as intended (behavior depends on how SQL Server resolves unbound placeholders — often leading to ignored or defaulted logic while the query still executes).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `"name"` vs `"@name"` mismatch | Filter silently ineffective — returns all active products |
| Consistency | Mixed convention — `@minPrice` correct, `@name` param wrong | Copy-paste spreads the bug |
| Maintainability | Dynamic SQL + manual lists | Easy to desync placeholder names from parameter names |

**Fix (priority order):**

1. **Always use `@` in both SQL and C#** — per **ProductRepository.cs** Section 5:
   ```csharp
   parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 100) { Value = $"%{nameFilter}%" });
   ```
2. Centralize binding — helper `AddParameter(command, "@name", SqlDbType.NVarChar, 100, value)` used by the builder so names are single-sourced.
3. Add an integration test: filtered search returns strict subset; assert row count changes when filter applied.
4. Optional: static analysis or unit test that regex-matches `@\w+` tokens in SQL to `Parameters` collection keys.

**Production takeaway:** Treat `@ParameterName` as a **shared identifier** between SQL text and `SqlParameter.ParameterName` — case-insensitive on SQL Server, but the `@` prefix is required for reliable binding.

---

#### Q8. (R) To "reduce allocations," a singleton service caches one `SqlCommand` and reuses it across concurrent HTTP requests on pooled connections:

```csharp
public sealed class ProductLookupService
{
    private readonly SqlCommand _cachedCommand;

    public ProductLookupService(SqlConnection connection)
    {
        _cachedCommand = connection.CreateCommand();
        _cachedCommand.CommandText = "SELECT Name FROM dbo.Products WHERE ProductId = @id";
        _cachedCommand.Parameters.Add(new SqlParameter("@id", SqlDbType.Int));
    }

    public string? GetName(SqlConnection connection, int productId)
    {
        _cachedCommand.Connection = connection;
        _cachedCommand.Parameters["@id"].Value = productId;
        return _cachedCommand.ExecuteScalar() as string;
    }
}
```

Registered as `AddSingleton<ProductLookupService>()`. What fails under load, and what is the correct lifetime pattern for `SqlCommand` in a web app?

---

**Answer:** **`SqlCommand` is not thread-safe** and must not be shared across concurrent requests. A singleton holding one command causes race conditions on `Connection`, `Parameters`, and internal state — intermittent wrong names, exceptions, or corrupted reads under load.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | Singleton `_cachedCommand` mutated by all threads | Data races on `Parameters["@id"].Value` and `Connection` assignment |
| Lifetime | Command created with ctor `connection` then reassigned per call | Undefined behavior when two requests swap `Connection` concurrently |
| Design | Caching command to "reduce allocations" | `SqlCommand` allocation is cheap vs a round-trip; pooling is for **connections**, not commands |
| Connection rules | One active `Execute*` per connection (without MARS) | Shared command amplifies reader/command conflicts |

**Fix (priority order):**

1. **Create one command per operation** — `using var cmd = connection.CreateCommand()` or **CommandFactory.CreateTextCommand** inside the method (see **ProductRepository.DemonstrateCommandDisposal**).
2. Register the **repository/service as scoped** (per request), not singleton, if it holds no shared command state.
3. Let **connection pooling** handle reuse — open short-lived connections from the pool; do not cache commands.
4. If micro-optimizing, cache the **SQL string**, not the `SqlCommand` instance.

**Production takeaway:** Pattern from **Program.cs** QUICK REFERENCE — *"Share one SqlCommand across threads → undefined behavior."* Commands are per-operation; connections are pooled.
