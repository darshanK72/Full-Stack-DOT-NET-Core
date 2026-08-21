# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/03. SqlCommand & Parameters`

---

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
