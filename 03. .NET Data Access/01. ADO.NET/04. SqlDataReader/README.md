# SqlDataReader — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [SqlDataReader](#chapter-04-sqldatareader)
  - [Q1. Why is `SqlDataReader` described as a forward-only, read-only cursor?](#q1-why-is-sqldatareader-described-as-a-forward-only-read-only-cursor)
  - [Q2. What is the difference between connected streaming reads and loading everything into memory?](#q2-what-is-the-difference-between-connected-streaming-reads-and-loading-everything-into-memory)
  - [Q3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?](#q3-why-must-a-sqldatareader-be-closed-or-disposed-before-running-another-command-on-the-same-connection-without-mars)
  - [Q4. When is `SqlDataReader` the best choice for large result sets?](#q4-when-is-sqldatareader-the-best-choice-for-large-result-sets)
  - [Q5. How do you handle NULL database values when reading from a data reader?](#q5-how-do-you-handle-null-database-values-when-reading-from-a-data-reader)
  - [Q6. What performance advantage does a reader have over filling a `DataTable`?](#q6-what-performance-advantage-does-a-reader-have-over-filling-a-datatable)
  - [Q7. What happens if you do not dispose a data reader?](#q7-what-happens-if-you-do-not-dispose-a-data-reader)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 04. SqlDataReader

### Q1. Why is `SqlDataReader` described as a forward-only, read-only cursor?

**Concepts**
- `Read()`/`ReadAsync()` advances one row at a time
- No backward movement or random row access
- Values consumed as read-only column data
- Efficient wire protocol: rows arrive sequentially

**Answer**

`SqlDataReader` advances sequentially — each `Read()` call fetches the next row and there is no `Previous` or random-access API. It exposes column values as read-only; to mutate data you must issue separate INSERT/UPDATE commands. This forward-only model matches how SQL Server streams result sets efficiently over the wire.

---

### Q2. What is the difference between connected streaming reads and loading everything into memory?

**Concepts**
- Streaming: one row at a time, constant memory, connection open
- In-memory load: all rows materialized before processing
- OOM risk with large Fill or ToList on web servers
- Streaming: faster time-to-first-row

**Answer**

Streaming through `SqlDataReader` processes one row at a time while the connection stays open, keeping memory constant regardless of result set size. Loading into `List<T>`, `DataTable`, or `DataSet` materializes every row as managed objects before your code runs. Streaming suits export pipelines, large reports, and APIs that map-and-write rows; in-memory load suits random access, multiple passes, or offline editing.

---

### Q3. Why must a `SqlDataReader` be closed or disposed before running another command on the same connection (without MARS)?

**Concepts**
- One active batch per connection without MARS
- Open reader owns the batch, blocks second command
- MARS (`MultipleActiveResultSets=True`) relaxes the rule
- Nested `using` blocks enforce correct disposal order

**Answer**

A single SQL Server connection allows only one active batch at a time without MARS enabled. An open reader still owns that batch, so a second `ExecuteReader` or `ExecuteNonQuery` on the same connection throws "There is already an open DataReader associated with this Command." Dispose the first reader completely before issuing the next command; enabling MARS is an option but adds server overhead and should be used only when truly needed.

---

### Q4. When is `SqlDataReader` the best choice for large result sets?

**Concepts**
- Sequential row-by-row processing without full materialization
- `CommandBehavior.SequentialAccess` for large binary/text columns
- Combines well with server-side paging (OFFSET/FETCH)
- Avoids Gen2 GC pressure from large DataTable loads

**Answer**

Choose `SqlDataReader` when you must process many rows sequentially without storing the entire result set — exports, ETL transforms, log scanning, and streaming HTTP responses. Pair it with `CommandBehavior.SequentialAccess` for large binary or text columns to avoid unnecessary buffering, and combine with server-side `OFFSET`/`FETCH` paging when the consumer needs only a window of rows. Loading millions of rows into `DataTable` or EF Core entities risks Gen2 garbage collection pressure that `SqlDataReader` avoids.

---

### Q5. How do you handle NULL database values when reading from a data reader?

**Concepts**
- Database NULL maps to `DBNull.Value` in ADO.NET
- `reader.IsDBNull(ordinal)` check before typed getter
- `GetFieldValue<T?>` and extension methods reduce boilerplate
- COALESCE in SQL as alternative to C# null-handling

**Answer**

Database NULL maps to `DBNull.Value`; calling a typed getter like `GetInt32` on a NULL column throws an `InvalidCastException`. Use `reader.IsDBNull(ordinal)` to check first, then map to a nullable CLR type (`int?`, `string?`) or substitute a default. `GetFieldValue<T?>` reduces boilerplate, and pushing defaults into SQL with `COALESCE` is another option that shifts null semantics to the database layer.

---

### Q6. What performance advantage does a reader have over filling a `DataTable`?

**Concepts**
- No DataRow/DataColumn allocation per row
- Lower GC pressure on high-throughput APIs
- Flat working set vs linear DataTable memory growth
- Faster time-to-first-row vs buffered Fill

**Answer**

A reader avoids allocating `DataRow`, `DataColumn`, and internal indexing structures — you map directly from column getters into DTOs, generating far fewer managed objects. `DataTable.Fill` allocates a full in-memory relational snapshot with type metadata and row-state tracking, causing memory to grow linearly with row and column count. Readers also start returning data immediately, while `Fill` waits until the adapter buffers the result set.

---

### Q7. What happens if you do not dispose a data reader?

**Concepts**
- Connection stays in busy state blocking additional commands
- Pool slot stays checked out until finalization
- Contributes to pool exhaustion under concurrent load
- `using`/`await using` guarantees disposal on exceptions

**Answer**

An undisposed reader keeps the connection in a busy state, blocking additional commands on that connection and preventing the slot from returning to the pool. Under concurrent load this contributes to pool exhaustion — symptoms are sporadic "open DataReader" errors and pool timeouts even though connection objects appear created correctly. Always wrap readers in `using` or `await using` so disposal runs even when exceptions interrupt reading.

---

## Gotchas — SqlDataReader (Interview Traps)

---

#### Gotcha 1. Open SqlDataReader blocks a second command on the same connection

**Concepts**
- MARS disabled by default in SQL Server
- second `ExecuteReader()` throws `InvalidOperationException`
- dispose first reader before issuing next command
- `CommandBehavior.CloseConnection` for ownership transfer
- MARS as workaround vs proper disposal as fix

**Answer**

Issuing a second `ExecuteReader()` on the same open `SqlConnection` while the first reader is still open throws `InvalidOperationException` unless Multiple Active Result Sets (MARS) is enabled in the connection string. MARS adds server-side overhead and is rarely the right solution — the correct fix is to dispose the first reader before issuing the next command. The typical scenario is loading a parent row and then querying child rows on the same connection without closing the first reader.

---

#### Gotcha 2. Accessing columns before calling Read() — no data available yet

**Concepts**
- `Read()` must be called to position on first row
- reader starts before the first row after `ExecuteReader`
- accessing columns before `Read()` throws `InvalidOperationException`
- `while (reader.Read())` pattern for multiple rows
- `if (reader.Read())` pattern for single optional row

**Answer**

`SqlDataReader` is positioned before the first row immediately after `ExecuteReader()` — accessing any column before calling `Read()` throws `InvalidOperationException`. The correct pattern is `while (reader.Read())` for multi-row results and `if (reader.Read())` for a single optional row. A common mistake is calling `reader["Id"]` immediately after `ExecuteReader` expecting the first row, but without `Read()` there is no current row.

---

#### Gotcha 3. GetString()/GetInt32() on a NULL column throws InvalidCastException

**Concepts**
- `GetString()`, `GetInt32()` do not handle `DBNull`
- `IsDBNull(ordinal)` check required before typed getter
- `reader.GetValue(ordinal)` returns `DBNull.Value` for null
- nullable reference types and `int?` with null check pattern
- `GetFieldValue<T>` with nullable T as modern alternative

**Answer**

Calling `reader.GetString(ordinal)` or `reader.GetInt32(ordinal)` on a column containing a database NULL throws `InvalidCastException` because the typed getters cannot convert `DBNull`. Always check `reader.IsDBNull(ordinal)` before calling a typed getter, or use `reader.GetValue(ordinal)` and compare with `DBNull.Value`. For nullable value types, the pattern `reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)` correctly handles both present and absent values.

---

#### Gotcha 4. Column name indexer is slower than ordinal access in tight loops

**Concepts**
- `reader["ColumnName"]` performs string lookup per row
- ordinal (integer) indexer avoids per-row name lookup
- `reader.GetOrdinal("Name")` called once before loop
- performance difference visible at hundreds of thousands of rows
- `GetString(ordinal)` vs `GetString("Name")` throughput

**Answer**

Accessing columns by name (`reader["ProductName"]`) performs a string lookup in the column collection on every row, which adds up in tight loops over large result sets. The efficient pattern is to call `reader.GetOrdinal("ProductName")` once before the loop to resolve the integer ordinal, then use `reader.GetString(nameOrdinal)` inside the loop. For a few thousand rows the difference is negligible, but for bulk exports of hundreds of thousands of rows the ordinal pattern measurably reduces mapping overhead.

---

#### Gotcha 5. SqlDataReader is forward-only — you cannot seek backward

**Concepts**
- forward-only cursor architecture
- no `Seek()`, `Reset()`, or random access
- multiple passes require multiple queries or in-memory collection
- `DataTable.Load(reader)` for random-access after read
- streaming vs random-access trade-off

**Answer**

`SqlDataReader` is a forward-only cursor — once a row has been read, you cannot return to it without re-executing the query. Code that calls `reader.Read()` in a first pass and then tries to iterate from the beginning again will find the reader exhausted. If multiple passes over the data are required, either buffer the rows into a `List<T>` during the first pass, use `DataTable.Load(reader)` to fill an in-memory table, or redesign the query to produce the required data in a single forward pass.

---

#### Gotcha 6. Undisposed SqlDataReader holds the connection busy

**Concepts**
- open reader keeps connection in active state
- connection cannot be returned to pool until reader disposed
- `yield return` with reader leaks connection on caller abandonment
- `await using` for async reader disposal
- `CommandBehavior.CloseConnection` for ownership transfer

**Answer**

An undisposed `SqlDataReader` holds the connection in an active state, preventing it from being returned to the pool. When a reader is used inside a `yield return` iterator and the caller abandons enumeration early (via `break` or disposal), the reader and connection remain open until garbage collection. Always wrap readers in `using` or `await using`, and avoid `yield return` over an open reader without wrapping the whole iterator in a `try/finally` that disposes the reader and connection.

---

#### Gotcha 7. `HasRows` is true even if all rows are filtered out by business logic

**Concepts**
- `HasRows` reflects whether the result set has any rows
- `HasRows` does not change after `Read()` calls
- `HasRows` vs `Read()` return value distinction
- checking `HasRows` to avoid `Read()` call is misleading
- `Read()` return value is the reliable "more rows" indicator

**Answer**

`HasRows` returns `true` if the query returned at least one row to the reader, but it does not change as you consume rows with `Read()`. Code that checks `HasRows` expecting it to become `false` partway through a loop will loop infinitely or incorrectly. The reliable way to check if there are more rows is the return value of `Read()` itself — `while (reader.Read())` terminates naturally when the result set is exhausted. Use `HasRows` only for a quick initial check of whether the result set is empty before the loop starts.

---

#### Gotcha 8. Accessing the reader after `Close()` or `Dispose()` throws

**Concepts**
- reader is unusable after `Close()` or `Dispose()`
- `Close()` allows reopening; `Dispose()` does not
- late column access outside `using` block throws
- reader reference kept alive after `using` scope ends
- explicit null-out of reader reference after disposal

**Answer**

Calling `reader.Close()` or `Dispose()` closes the cursor and makes all column-access methods throw `InvalidOperationException` on subsequent calls. A common mistake is storing the reader in a broader-scoped variable, disposing it inside a helper method, and then attempting to read columns outside the scope. `Close()` allows the reader to be reopened with another `ExecuteReader` call on the same command, while `Dispose()` does not — but in practice, always let the `using` block manage disposal and do not retain reader references beyond the scope.

---

#### Gotcha 9. `CommandBehavior.CloseConnection` needed when transferring reader ownership

**Concepts**
- `CommandBehavior.CloseConnection` closes connection with reader
- required when reader outlives the method that opened the connection
- streaming APIs that return `IAsyncEnumerable<T>` over a reader
- connection leak when ownership transfer is done without the flag
- `ExecuteReaderAsync(CommandBehavior.CloseConnection, ct)`

**Answer**

When a method returns a `SqlDataReader` to a caller who will own disposal — such as a streaming endpoint that returns the reader to the serializer — `CommandBehavior.CloseConnection` must be passed to `ExecuteReaderAsync`. With this flag, disposing the reader also closes and returns the connection to the pool, preventing a connection leak when the caller disposes only the reader. Without it, the connection remains open until separately disposed, and a method that returned the reader without retaining the connection reference has no way to close it.

---

#### Gotcha 10. Async enumeration with `ReadAsync()` not called — synchronous read defeats async chain

**Concepts**
- `reader.Read()` vs `reader.ReadAsync()` for async contexts
- blocking thread pool during row-by-row reads
- `await reader.ReadAsync(ct)` inside `async` methods
- `while (await reader.ReadAsync(ct))` pattern
- cancellation token propagation through async read loop

**Answer**

Inside an `async` method, calling synchronous `reader.Read()` blocks the thread pool thread during each row fetch, defeating the benefit of `ExecuteReaderAsync`. The correct pattern is `while (await reader.ReadAsync(cancellationToken))` so that each row's network round-trip releases the thread to the pool. Also pass the `CancellationToken` through `ReadAsync` so that a client disconnect or request timeout can abort mid-stream rather than processing rows for a client that has already disconnected.

---

## Scenario-Based Questions (Karat Format)

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

---

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

---

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

---

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
int totalCount = 0;

do
{
    while (await reader.ReadAsync())
    {
        if (reader.FieldCount > 1)
            products.Add(MapProduct(reader));
        else if (reader.FieldCount == 1)
            totalCount = reader.GetInt32(0);
    }
}
while (await reader.ReadAsync()); // advance to next result set
```

In QA, the API hangs or returns `totalCount = 0` with only the first product row, and memory grows when the procedure returns 50k products. What did they misunderstand about `NextResult()`, and what is the correct consumption pattern?

---

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

---

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

---

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

---

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
