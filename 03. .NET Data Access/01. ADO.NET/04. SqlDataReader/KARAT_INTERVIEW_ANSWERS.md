# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/04. SqlDataReader`

---

#### Q1. (R) A production export job intermittently hangs with "Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool." Review this repository method copied from an internal tool:

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    var connection = new SqlConnection(connectionString);
    connection.Open();

    var command = new SqlCommand(
        "SELECT ProductId, ProductName, UnitPrice, StockQuantity, DiscontinuedDate FROM dbo.Products;",
        connection);

    SqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        yield return new ProductRow
        {
            ProductId = reader.GetInt32(0),
            ProductName = reader.GetString(1),
            UnitPrice = reader.GetDecimal(2),
            StockQuantity = reader.GetInt32(3),
            DiscontinuedDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4)
        };
    }
}
```

The caller does `foreach (var p in repo.StreamProducts(cs)) { await WriteCsvLine(p); }`. What breaks under load, and how do you fix it without loading the entire table into a `List<T>` first?

**Answer:** The iterator keeps the `SqlConnection` and `SqlDataReader` open for the entire `foreach` lifetime, and nothing ever disposes them — so under concurrent exports the pool exhausts and new requests time out waiting for a connection. Fix by owning disposal explicitly (typically `IAsyncEnumerable<T>` with `await using`, or pass ownership of a reader wrapped with `CommandBehavior.CloseConnection` to a caller that disposes promptly).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime / resource | `SqlConnection`, `SqlCommand`, and `SqlDataReader` never disposed | Server-side cursors and client handles leak; connection not returned to pool |
| Async | Caller `await`s I/O between `MoveNext()` calls while reader stays open | Long-held connections amplify pool starvation under parallel jobs |
| Design | `IEnumerable<T>` + `yield return` hides connected lifetime | Callers cannot see they must finish enumeration quickly |
| Correctness | No `try/finally` / `using` around reader or connection | Exception mid-loop leaks connection even if enumeration stops |

**Fix (priority order):**

1. **Dispose on all paths** — wrap connection, command, and reader in `using`/`await using` and only yield inside that scope, *or* switch to `IAsyncEnumerable<ProductRow>` so disposal runs when enumeration completes or is cancelled.
2. **Keep streaming** — do not materialize `List<ProductRow>`; read one row, map, yield, repeat — same memory profile as today but with guaranteed cleanup.
3. **Consider `CommandBehavior.CloseConnection`** when returning a reader to a dedicated exporter that owns disposal — see **CloseConnectionBehaviorDemo.cs**.
4. **Cap concurrency** — limit parallel export workers so even correct code cannot open more connections than `Max Pool Size` allows.
5. **Prefer async APIs** for the write side — `await reader.ReadAsync()` in ch.08 avoids blocking thread-pool threads while the connection stays open (see Q6).

Example shape (sync streaming with owned disposal):

```csharp
public IEnumerable<ProductRow> StreamProducts(string connectionString)
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();
    using var command = new SqlCommand(sql, connection);
    using var reader = command.ExecuteReader();
    while (reader.Read())
        yield return MapRow(reader);
}
```

**Production takeaway:** A `SqlDataReader` is a **leased connection**, not a lazy collection — whoever opens it must dispose it before the connection returns to the pool. See **ProperDisposalDemo.cs** and **Program.cs** quick-reference disposal table.

---

#### Q2. (R) Two teammates map the same `Products` query differently. Review both snippets:

```csharp
// Team A — BasicReadLoopDemo style
while (reader.Read())
{
    int id = reader.GetInt32(0);
    string name = reader.GetString(1);
    decimal price = reader.GetDecimal(2);
}

// Team B — ProductMapper style (called inside every Read())
while (reader.Read())
{
    products.Add(ProductMapper.MapFromReader(reader));
}

// ProductMapper.MapFromReader (current chapter code)
public static ProductRow MapFromReader(SqlDataReader reader)
{
    int ordId = reader.GetOrdinal("ProductId");
    int ordName = reader.GetOrdinal("ProductName");
    // … GetOrdinal for each column on every row …
}
```

A DBA adds `ModifiedAt` as the first column in `SELECT * FROM dbo.Products` for auditing. Team A's export still "works" but prices look like integers. Team B's page is slower on 200k rows. Diagnose both failure modes and describe the mapping approach you would standardize on.

**Answer:** Team A's magic ordinals silently shift when column order changes — `GetDecimal(2)` now reads `ModifiedAt`, not `UnitPrice`. Team B's name-based mapping survives column reorder but pays repeated `GetOrdinal` lookups every row; the chapter's **ProductMapper** should cache ordinals once per reader/result set, not inside `MapFromReader` on every call.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Magic indexes (`0`, `1`, `2`) bind to **position**, not meaning | Schema/query changes corrupt data without compile errors |
| Performance | `GetOrdinal` inside `MapFromReader` on every `Read()` | O(rows × columns) name lookups — noticeable at 200k rows |
| Maintainability | Two incompatible conventions on the same table | Code review cannot tell which queries are safe after SELECT changes |

**Fix (priority order):**

1. **Standardize on cached ordinals resolved once** after `ExecuteReader`, before the loop — pattern from **ColumnMetadataDemo.cs** and the quick reference in **Program.cs**.
2. **Explicit SELECT lists** — never rely on `SELECT *` column order in production readers; list columns in a stable contract.
3. **Refactor ProductMapper** — accept precomputed ordinals or resolve once via a small `ProductColumnOrdinals` struct:

```csharp
var ord = ProductColumnOrdinals.From(reader); // GetOrdinal once
while (reader.Read())
    products.Add(ProductMapper.Map(reader, ord));
```

4. **Reserve magic indexes** only for throwaway demos (**BasicReadLoopDemo.cs**) or proven-frozen micro-queries — not shared repositories.

**Production takeaway:** **GetOrdinal by name once + typed getters** gives schema-order resilience without per-row lookup cost. Magic indexes are fast but fragile — treat them like pointer arithmetic.

---

#### Q3. (R) A nightly job reads discontinued products and crashes on row 847 with `SqlNullValueException`. Review the mapping helper:

```csharp
while (reader.Read())
{
    var row = new ProductRow
    {
        ProductId = (int)reader["ProductId"],
        ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
        StockQuantity = reader.GetField<int>("StockQuantity"), // extension: (T)reader[name]
        DiscontinuedDate = (DateTime?)reader["DiscontinuedDate"]
    };
    rows.Add(row);
}
```

What is wrong with each nullable/value-type read, and how does this differ from the `IsDBNull` pattern in **NullHandlingDemo.cs**?

**Answer:** SQL `NULL` surfaces as `DBNull.Value`, not C# `null`, on value-type columns — casting or calling typed getters without an `IsDBNull` check throws `SqlNullValueException` or `InvalidCastException`. The chapter pattern branches on `IsDBNull` before `GetDateTime`, and never casts boxed `DBNull` to `DateTime?`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime | `(DateTime?)reader["DiscontinuedDate"]` when column is SQL NULL | `InvalidCastException` — `DBNull` does not unbox to nullable |
| Runtime | `GetDecimal` / `GetField<int>` on NULL numeric columns | `SqlNullValueException` — typed getters reject NULL |
| Performance | `GetOrdinal("ProductName")` inside every row | Unnecessary overhead (secondary to correctness bug) |
| Design | Indexer + cast hides null semantics | Looks like EF/Dapper mapping but lacks null guards |

**Fix (priority order):**

1. **DiscontinuedDate** — match **NullHandlingDemo.cs**:

```csharp
DateTime? discontinued = reader.IsDBNull(ordDisc)
    ? null
    : reader.GetDateTime(ordDisc);
```

2. **Other nullable numerics** — if a column can be NULL, use `IsDBNull` or `reader.GetFieldValue<int?>(ord)` (.NET typed helper) before assigning to `int?`.
3. **Cache ordinals** before the loop; use `GetInt32`/`GetString`/`GetDecimal` instead of `(int)reader["ProductId"]` to avoid double boxing.
4. **Optional columns** — if `ProductName` were NULL, `GetString` returns empty string on some providers but behavior varies; explicit `IsDBNull` is safer for reference types you want as C# `null`.

**Production takeaway:** ADO.NET NULL is **`DBNull.Value`**, not `null` — always gate typed accessors with `IsDBNull` (or `GetFieldValue<T>` for nullable value types). See **NullHandlingDemo.cs** and **Program.cs** common-mistakes table.

---

#### Q4. (M) A stored procedure returns three result sets: (1) product rows, (2) aggregate counts, (3) audit metadata. A junior developer adapts **MultipleResultSetsDemo.cs** like this:

```csharp
using var reader = await cmd.ExecuteReaderAsync();
var products = new List<ProductRow>();

while (await reader.ReadAsync())
    products.Add(MapProduct(reader));

int totalCount = 0;
if (reader.NextResult())
{
    if (reader.Read())
        totalCount = reader.GetInt32(reader.GetOrdinal("ProductCount"));
}

return new ProductPage(products, totalCount);
```

In QA, `totalCount` is always `0` and memory spikes when the procedure returns 50k products. What did they misunderstand about `NextResult()`, and what is the correct consumption pattern?

**Answer:** They call `ReadAsync()` where the API requires `NextResult()` — `ReadAsync` advances **rows within the current result set**, not to the next SELECT in the batch. After the inner loop drains set 1, the outer `while (await reader.ReadAsync())` either finds no more rows (stops early with wrong `totalCount`) or mis-reads rows from set 2 as if they were products. The chapter pattern is `do { … Read loop … } while (await reader.NextResultAsync())`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | `ReadAsync()` used to advance between result sets | Never reaches set 2/3 correctly — `ReadAsync` only moves to next **row** |
| Correctness | `FieldCount` heuristic mixes product rows and aggregate rows in one loop | Set 2 row with one column overwrites `totalCount`; product rows misclassified |
| Design | Loading 50k rows into `List<ProductRow>` | Memory spike — reader streaming benefit lost |
| Correctness | Third audit result set never consumed | Unread results can block some providers or leave batch incomplete |

**Fix (priority order):**

1. **Replace the outer `ReadAsync` with `NextResultAsync`** — use the chapter pattern from **MultipleResultSetsDemo.cs**:

```csharp
var products = new List<ProductRow>();
int totalCount = 0;
int setNumber = 0;

do
{
    setNumber++;
    while (await reader.ReadAsync())
    {
        if (setNumber == 1)
            products.Add(MapProduct(reader));
        else if (setNumber == 2)
            totalCount = reader.GetInt32(0);
        // set 3: drain or map audit columns
    }
}
while (await reader.NextResultAsync());
```

2. **Drain every set** — even unneeded sets need an empty `while (ReadAsync())` loop before the next `NextResult()`.
3. **Detect set shape** by column count/names, or use **separate commands** if the SP contract is unstable.
4. **Stream set 1** to response/file if 50k rows — do not require full materialization for export scenarios.

**Production takeaway:** `NextResult()` moves to the **next SELECT** in a batch — each set needs its own complete `Read` loop. Stored procedures with multiple selects are common; treat each set as a separate mini-reader.

---

#### Q5. (P) A data-access layer exposes streaming reads to upper layers:

```csharp
public SqlDataReader OpenProductStream()
{
    var connection = new SqlConnection(_connectionString);
    connection.Open();
    var cmd = new SqlCommand("SELECT * FROM dbo.Products ORDER BY ProductId", connection);
    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
}
```

The API controller does:

```csharp
public IActionResult Export()
{
    using var reader = _repo.OpenProductStream();
    // … write rows to response …
    return File(stream, "text/csv");
}
```

Under what conditions does this pattern work, and what connection-lifetime mistakes still cause "There is already an open DataReader associated with this Connection" or leaked connections in production?

**Answer:** `CommandBehavior.CloseConnection` works when the **caller disposes the reader** and that caller is the only code using the hidden connection — disposing the reader closes the connection automatically (**CloseConnectionBehaviorDemo.cs**). It fails when anything else tries to reuse the same `SqlConnection` while the reader is open, or when the reader is never disposed (aborted request, exception, missing `using`).

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Lifetime | Second operation on same connection before reader disposed | `InvalidOperationException`: open DataReader associated with Connection |
| Lifetime | Reader not disposed on early return / exception | Connection leak despite `CloseConnection` — behavior only runs on dispose |
| Design | Returning raw `SqlDataReader` leaks ADO.NET through repository boundary | Callers must know ADO.NET disposal rules |
| ASP.NET | Request cancelled mid-stream | Must pass `CancellationToken` to `ReadAsync` and dispose reader in `finally` |

**Fix (priority order):**

1. **Document ownership** — caller must `using`/ `await using` the reader; repository must not retain connection reference after handoff.
2. **Never share one `SqlConnection`** across concurrent readers or commands — one open reader per connection (MARS aside — separate topic).
3. **Prefer encapsulation** — `IAsyncEnumerable<ProductRow>`, callback `Action<SqlDataReader>`, or write directly to `Stream` inside the repository so connection lifetime stays internal (**ProperDisposalDemo.cs** pattern).
4. **If keeping raw reader** — pair with `CloseConnection` (as shown) *and* ensure `SqlCommand` is not disposed before reader finishes (command lifetime tied to reader on some providers).
5. **Monitor pool** — log when exports abort; use `try/finally` to dispose reader even when `OperationCanceledException` fires.

**Production takeaway:** `CloseConnection` transfers connection cleanup to reader disposal — it does not remove the rule that **one connection ↔ one active reader**. See **CloseConnectionBehaviorDemo.cs** and ch.02 connection lifetime.

---

#### Q6. (P) An ops team exports `DocumentBody varbinary(max)` for 10,000 rows (~5 MB each). A developer loads full rows like **BasicReadLoopDemo** and the app hits `OutOfMemoryException`. They switch to:

```csharp
using var reader = cmd.ExecuteReader(
    CommandBehavior.SequentialAccess | CommandBehavior.CloseConnection);

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);
    byte[] body = (byte[])reader[2]; // DocumentBody
    await WriteToBlobStorage(id, body);
}
```

Explain how `CommandBehavior.SequentialAccess` is meant to work, what is still wrong with `(byte[])reader[2]`, and sketch the production-safe read pattern for large LOBs.

**Answer:** `SequentialAccess` tells the provider to stream columns in **increasing ordinal order** without buffering the whole row — large values should be read with `GetBytes`/`GetChars` in chunks. Casting `reader[2]` to `byte[]` still materializes the entire LOB into memory, defeating the flag.

- **SequentialAccess rules** (**SequentialAccessPreviewDemo.cs**): read columns ordinally (0, 1, 2…); do not re-read an earlier column after moving forward; combine with `CloseConnection` when handing reader to exporter.
- **What's wrong with `(byte[])reader[2]`:** indexer loads full column value — ~50 GB total for 10k × 5 MB → OOM.
- **Production pattern:**

```csharp
const int chunkSize = 81920;
var buffer = new byte[chunkSize];
long offset = 0;

while (reader.Read())
{
    int id = reader.GetInt32(0);
    string title = reader.GetString(1);

    await using var blobStream = OpenBlobStream(id);
    long read;
    do
    {
        read = reader.GetBytes(2, offset, buffer, 0, buffer.Length);
        offset += read;
        if (read > 0)
            await blobStream.WriteAsync(buffer.AsMemory(0, (int)read));
    } while (read == chunkSize);

    offset = 0; // reset for next row
}
```

- **Async:** prefer `GetBytes`/`ReadAsync` (ch.08) so thread pool isn't blocked during network I/O.
- **Alternative:** `OPENROWSET`/blob storage URLs in SQL so the app never pulls multi-MB through the app tier.

**Production takeaway:** SequentialAccess is for **chunked LOB streaming**, not smaller magic indexes. Pair with bounded buffers and async writes — see **SequentialAccessPreviewDemo.cs** and **Program.cs** CommandBehavior table.

---

#### Q7. (D) `ProductMapper.MapFromReader` resolves columns by name (`GetOrdinal("ProductId")`, etc.) and is reused across three queries: a list screen, a search SP that aliases `ProductId AS Id`, and a reporting view that exposes `SKU` instead of `ProductName`. The chapter positions this mapper as the pattern "Dapper and EF Core automate." What fails in production as queries diverge, and how would you refactor for maintainability without jumping straight to EF Core?

**Answer:** A single name-based mapper assumes every query projects the **same column names** — aliasing or view renames cause `IndexOutOfRangeException` from `GetOrdinal` at runtime, not compile time. Reusing **ProductMapper** across heterogeneous queries is the fragility the chapter warns about when moving from demo to production.

- **What fails:** `GetOrdinal("ProductId")` on `SELECT Id …` throws; `GetOrdinal("ProductName")` on a view with `SKU` throws; each failure hits production on first request after deployment — no compiler signal.
- **Why it looked fine locally:** All demos use one explicit SELECT with canonical names (**ProperDisposalDemo.cs**).
- **Refactor options (short of EF Core):**
  1. **One mapper per query shape** — `MapFromProductListReader`, `MapFromSearchSpReader` with ordinals cached per shape.
  2. **Column ordinals struct built from the actual query** — factory method documents required names in one place.
  3. **Dapper** with explicit multi-mapping or default column matching — still requires consistent aliases (`AS ProductId`).
  4. **Integration tests** against real view/SP definitions to catch rename breaks in CI.
  5. **Views or SPs as stable contracts** — DB team owns column names; app mappers target the view, not ad-hoc SELECTs.

```csharp
// Stable contract: dbo.v_ProductList always exposes ProductId, ProductName, …
internal static class ProductListReader
{
    public static ProductColumnOrdinals Ordinals { get; private set; }

    public static void Init(SqlDataReader r) =>
        Ordinals = ProductColumnOrdinals.From(r, required: ["ProductId", "ProductName", …]);
}
```

**Production takeaway:** Manual mapping buys control at the cost of **schema coupling** — isolate that coupling per query contract or adopt a tool (Dapper/EF) with explicit configuration. See **ProductMapper.cs** as the teaching baseline, not a shared global mapper.

---
