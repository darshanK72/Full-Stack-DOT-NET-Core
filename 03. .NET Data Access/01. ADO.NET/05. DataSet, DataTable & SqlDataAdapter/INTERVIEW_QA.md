# DataSet, DataTable & SqlDataAdapter — Interview Q&A
> Back to [ADO.NET Interview Q&A](../INTERVIEW_QA.md)

## Table of Contents

- [DataSet, DataTable & SqlDataAdapter](#chapter-05-dataset-datatable--sqldataadapter)
  - [Q1. What is the disconnected model that `DataSet`/`DataTable` support?](#q1-what-is-the-disconnected-model-that-datasetdatatable-support)
  - [Q2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?](#q2-what-is-the-difference-between-datareader-streaming-and-dataadapterfill)
  - [Q3. When would you still use `DataSet`/`DataTable` in modern .NET applications?](#q3-when-would-you-still-use-datasetdatatable-in-modern-net-applications)
  - [Q4. What are the memory implications of filling a large table into a `DataSet`?](#q4-what-are-the-memory-implications-of-filling-a-large-table-into-a-dataset)
  - [Q5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?](#q5-why-are-datasetdatatable-less-common-in-aspnet-core-apis-than-in-older-winforms-apps)
- [Gotchas](#gotchas)
- [Scenario-Based Questions (Karat Format)](#scenario-based-questions-karat-format)

---

## Chapter 05. DataSet, DataTable & SqlDataAdapter

### Q1. What is the disconnected model that `DataSet`/`DataTable` support?

**Concepts**
- In-memory relational snapshot after connection closes
- `DataTable`: rows, columns, constraints, row state
- `DataSet`: multiple tables with `DataRelation` links
- `SqlDataAdapter.Fill` (pull) and `.Update` (push)

**Answer**

`DataSet` and `DataTable` hold relational data in memory after the database connection closes, letting code browse, filter, sort, and edit rows locally. `SqlDataAdapter.Fill` pulls rows into the in-memory structures; `adapter.Update` later pushes batched changes back through the adapter's insert, update, and delete commands. `DataSet` can hold multiple related tables with `DataRelation` objects modeling parent-child keys.

---

### Q2. What is the difference between `DataReader` streaming and `DataAdapter.Fill`?

**Concepts**
- Reader: connected, forward-only, minimal per-row allocation
- Fill: disconnected snapshot, all rows materialized upfront
- Fill supports random row access; reader does not
- `startRecord`/`maxRecords` for partial Fill loads

**Answer**

`DataReader` streams rows through a live connection with minimal allocation — you map columns into DTOs as each row arrives. `DataAdapter.Fill` executes the select command and loads the entire result into a `DataTable`, building column schema and row objects before your code runs. Readers suit large read-only sequential processing; `Fill` suits disconnected scenarios needing random row access or multi-pass processing.

---

### Q3. When would you still use `DataSet`/`DataTable` in modern .NET applications?

**Concepts**
- Ad hoc reporting and Excel-like grid editing
- Third-party controls or integration APIs requiring DataTable
- Untyped tabular payloads with runtime-variable schema
- Legacy interop and middle-tier merge scenarios

**Answer**

`DataSet` and `DataTable` remain useful for ad hoc reporting tools, legacy interop, admin utilities that let operators edit rows and push one batch update, and third-party controls that bind directly to `DataTable`. They also suit scenarios accepting untyped tabular payloads where schema varies at runtime. Greenfield ASP.NET Core REST APIs usually return `IEnumerable<T>` DTOs from readers or EF Core projections instead.

---

### Q4. What are the memory implications of filling a large table into a `DataSet`?

**Concepts**
- Each row is a `DataRow` with boxed values and version history
- Memory: row count × column count, often multiples of raw payload
- Gen2 GC pressure from large fills on server machines
- Streaming, paging, or projection as alternatives

**Answer**

Every row becomes a `DataRow` with boxed values, version history, and state flags; every column carries `DataColumn` metadata — memory grows proportional to row count × column count and is often several times larger than the raw SQL payload. Large text or binary columns duplicate fully in managed memory. Gen2 garbage collections from big fills cause long pauses under load, so prefer streaming, server-side paging, or DTO projection when result sets are large.

---

### Q5. Why are `DataSet`/`DataTable` less common in ASP.NET Core APIs than in older WinForms apps?

**Concepts**
- REST APIs expect JSON-serializable POCOs, not DataRow
- Stateless APIs don't hold in-memory DataSet per request
- EF Core and micro-ORMs replaced "load table, bind grid"
- Disconnected editing model doesn't match PUT/PATCH REST

**Answer**

ASP.NET Core APIs are stateless and JSON-centric — clients expect typed DTOs that JSON serializers map cleanly, not `DataRow` dictionaries with `DBNull` values. WinForms and WebForms bound grids directly to `DataTable` in session-heavy patterns that do not fit modern stateless REST design. EF Core and micro-ORMs replaced the "load table, bind grid" workflow with entity or DTO pipelines, making `DataSet` rarely needed in new web APIs.

---

## Gotchas

#### Gotcha 4. Leaked connections exhaust the pool

**Answer:** Failing to dispose `SqlConnection`, `SqlDataReader`, or abandoning a `using` block early leaks connection pool slots until timeout, eventually causing "timeout expired obtaining connection from pool" errors under load.

- Always use `await using` for connections and readers so disposal runs on exceptions too.
- Symptoms appear only under concurrent load, making this a classic production-only failure mode.
- Long-lived undisposed `DbContext` instances cause the same exhaustion pattern.

---

#### Gotcha 14. Tracking overhead on read-only queries

**Answer:** Omitting `AsNoTracking()` on large read-only lists makes EF Core snapshot every entity for change detection that will never run, wasting memory and CPU on GET endpoints.

- Tracking stores original and current values per property for each row materialized.
- ASP.NET Core read services should default to `AsNoTracking()` plus DTO projection.
- Global `QueryTrackingBehavior.NoTracking` with explicit tracking on command paths prevents accidental overhead.

---

#### Gotcha 5. Transaction started after first command

**Answer:** Beginning a `SqlTransaction` only after the first statement already executed means that statement committed under implicit autocommit, so later steps in the intended unit of work are not atomic with the first.

- Call `BeginTransaction` immediately after opening the connection, before any DML.
- EF Core `SaveChanges` without an explicit transaction auto-commits each call — wrap multi-step work explicitly.
- Integration tests with single-user data often miss this race because implicit commits appear to "work."

---

## Scenario-Based Questions (Karat Format)

#### Q1. (R) A legacy ASP.NET Core API endpoint loads an entire `production.orders` table into a `DataSet`, lets callers filter in memory, and returns JSON. Review the service method:

```csharp
public async Task<IActionResult> SearchOrders(string? statusFilter)
{
    using var connection = new SqlConnection(_connString);
    using var adapter = new SqlDataAdapter("SELECT * FROM production.orders", connection);
    var dataSet = new DataSet("OrdersSnapshot");
    adapter.Fill(dataSet); // ~400k rows, 40+ columns each

    DataTable orders = dataSet.Tables[0]!;
    if (!string.IsNullOrEmpty(statusFilter))
    {
        foreach (DataRow row in orders.Rows)
        {
            if (row["Status"]?.ToString() != statusFilter)
                row.Delete();
        }
        orders.AcceptChanges(); // "clean up" deleted rows before serialize
    }

    return Ok(orders); // serializes remaining rows to JSON
}
```

What breaks under load, and what would you change first?

---

**Answer:** This endpoint materializes the full orders table into RAM on every request, then mutates row state just to filter — a disconnected-model pattern suited to desktop grids, not HTTP APIs. Under concurrent traffic it will cause memory pressure, GC churn, and long latencies long before SQL becomes the bottleneck.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Scalability | `SELECT *` + `Fill` of ~400k rows per request | Large Gen2 heap allocations; OOM risk under parallel calls |
| API design | In-memory filter via `Delete()` + `AcceptChanges()` | Wastes CPU/RAM; filter belongs in SQL (`WHERE`) or keyed paging |
| Correctness | `async` action but synchronous `Fill` | Blocks thread pool; no cancellation |
| Serialization | Returning raw `DataTable` to JSON | Awkward shape (`RowError`, schema metadata); unbounded payload |
| Design | Disconnected snapshot for a read-only search | Wrong tool — see **DisconnectedModelNotes.cs** (reader = stream, table = edit snapshot) |

**Fix (priority order):**

1. Push filter to SQL: `WHERE Status = @status` with parameters; return only needed columns and a page (`OFFSET/FETCH` or keyset).
2. Replace `DataSet`/`DataTable` with `SqlDataReader`, Dapper, or EF Core projection to DTOs — stream or page, do not hold full table per request.
3. If results must be large, add export/async job — not a synchronous API snapshot.
4. Make the action truly async (`FillAsync` / EF `ToListAsync`) and accept `CancellationToken`.
5. Map to typed response models instead of serializing `DataTable`.

**Production takeaway:** `Fill` loads a **mutable offline copy** — ideal for **AcceptRejectDemo.cs** / grid edit cycles, not for stateless microservice reads. Memory bloat shows up only when traffic multiplies snapshot size.

---

#### Q2. (M) After `adapter.Update(table)` throws because two rows failed with a SQL error, a developer resets UI state so users can retry. They call `table.AcceptChanges()` on the whole table "to clear the error flags." Before that, one row was `Modified`, one was `Added`, and one was `Deleted`. What is wrong with calling `AcceptChanges()` here, and what should happen instead?

---

**Answer:** `AcceptChanges()` does not clear errors — it **commits** pending edits in memory as if the database save succeeded. After a failed `Update`, that discards the distinction between saved and unsaved work and can leave the database and UI permanently out of sync.

- **`Modified` → `Unchanged`:** Current values become the new Original baseline even though SQL never persisted the edit — the next `Update` may skip the row or generate wrong SQL.
- **`Added` → `Unchanged`:** The row looks persisted locally but has no matching INSERT success (or a partial insert elsewhere in the batch).
- **`Deleted` → removed from `Rows`:** The row vanishes from the grid while still alive in SQL — classic "row came back on refresh" bug.
- **`RejectChanges()`** (per row or table) is the undo path: Modified reverts to Original, Added rows drop out, Deleted rows restore — see **AcceptRejectDemo.cs** and **RowStateDemo.cs**.
- On partial `Update` failure, inspect `row.RowError` / `GetErrors()`, fix or reject failed rows only, and retry inside a **SqlTransaction** (chapter 06) so one failure rolls back the batch.
- Call **`AcceptChanges()` only after a successful `Update`** — the pipeline in **AdapterUpdateOverview.cs** step 4.

**Production takeaway:** RowState is your offline change log; `AcceptChanges()` is "commit session," not "clear exception." Misusing it after failure is worse than leaving rows dirty.

---

#### Q3. (R) Two users edit the same category row offline in a WinForms grid backed by `SqlDataAdapter`. User A changes `category_name` to "Mountain Bikes"; User B changes it to "MTB" and saves first. User A clicks Save. Review the update setup:

```csharp
const string selectSql = """
    SELECT category_id, category_name
    FROM production.categories
    WHERE category_id = @id
    """;
using var adapter = new SqlDataAdapter(selectSql, connection);
adapter.SelectCommand!.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = 1 });
using var builder = new SqlCommandBuilder(adapter);

// Fill → user edits row → Update
adapter.Update(categoriesTable);
categoriesTable.AcceptChanges();
```

`SqlCommandBuilder` generated an `UPDATE` with only `category_name` in the `SET` clause and `category_id` in the `WHERE` clause — no rowversion/timestamp check. What failure mode does User A hit, and how do you fix optimistic concurrency for this disconnected pattern?

---

**Answer:** User A's save succeeds with a **silent last-write-wins overwrite** — B's "MTB" is lost without error. Default `CommandBuilder` updates match on primary key only; disconnected editing has no built-in "someone else changed this since Fill" guard.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Concurrency | No `rowversion` / original-value predicate in `UPDATE` | Lost updates; no user-visible conflict |
| Disconnected model | Original values from Fill are stale after B saves | A's `Modified` row still looks valid |
| UX | No `DBConcurrencyException` handling | Users trust save confirmation incorrectly |

**Fix (priority order):**

1. Add **`rowversion`** (or `timestamp`) to SELECT; include `@Original_rowversion` in a custom `UpdateCommand` `WHERE` clause, or use `CommandBuilder.ConflictOption = ConflictOption.CompareAllSearchableValues` / compare original values for critical columns.
2. On `DBConcurrencyException`, offer **refresh + merge** or **RejectChanges** on that row — do not blanket `AcceptChanges()`.
3. Shorten offline window: re-`Fill` before edit, or move to online API with ETag/`If-Match` for services.
4. Wrap multi-row `Update` in a transaction so related edits stay consistent.

**Production takeaway:** Disconnected optimistic concurrency requires **you** to compare "what I read" vs "what DB has now" — RowState alone only tracks local edits. See **Program.cs** quick reference on PrimaryKey + Update matching.

---

#### Q4. (D) A microservices team proposes sharing a `DataSet` between an order API, a reporting worker, and a mobile sync service "so everyone has the same offline cache." When would you push back and recommend `SqlDataReader`, Dapper, or EF Core instead? Name at least two concrete reasons tied to deployment and API design.

---

**Answer:** A shared in-process `DataSet` is a monolith-era cache, not a service boundary. Each microservice should own its persistence and contract; shipping `DataTable` snapshots across HTTP or queues couples schemas, memory footprints, and deployment cadence.

- **No cross-process sharing:** `DataSet` is in-memory CLR state — it does not replicate across pods. Each instance would `Fill` its own copy (3× memory, 3× stale snapshots) unless you add Redis/DB anyway.
- **Unbounded RAM & GC:** Large `Fill` snapshots (see **LocalDbFillDemo.cs**) fight container memory limits; APIs should stream (`SqlDataReader`) or page typed DTOs.
- **Schema coupling:** Column renames break all consumers at once — JSON DTOs or explicit API versions decouple deploys.
- **Wrong change-tracking model:** `RowState` / `Update` batches fit **single-user grid sessions**, not concurrent REST handlers — EF Core change tracker or explicit commands per aggregate are clearer.
- **Serialization cost:** `DataTable` JSON is heavy and leaky; clients expect stable REST contracts, not ADO.NET wire formats.
- **When DataSet still fits:** Single desktop app, local report designer, or one-shot import staging table on one machine — not multi-service topology.

**Production takeaway:** Use disconnected ADO.NET where the user **edits a snapshot offline** (**AcceptRejectDemo.cs**); use connected/streaming access for **stateless, scaled-out services**.

---

#### Q5. (P) A nightly job must insert 2 million staging rows from a CSV import. One developer uses `SqlDataAdapter.Update` on a `DataTable` with 2M `Added` rows; another uses `SqlBulkCopy.WriteToServer(dataTable)` inside a transaction. Compare throughput, change tracking, and failure behavior. Which path matches this ETL job, and what from chapter 06 still applies?

---

**Answer:** For bulk append-only loads, **`SqlBulkCopy`** is the production path; `adapter.Update` issues per-row `InsertCommand` executions and drowns in round-trip overhead.

- **`SqlDataAdapter.Update`:** Inspects each row's `RowState` (`Added` → `InsertCommand` per **AdapterUpdateOverview.cs**); ~2M individual inserts; slow; useful when rows are heterogeneous (mix of Added/Modified/Deleted) or UI batch save.
- **`SqlBulkCopy`:** Streams rows to one destination table (**SqlBulkCopyPreview.cs**); minimal logging overhead; does not honor RowState — all rows append; no automatic UPDATE/DELETE.
- **Change tracking:** Adapter path needs `Added` rows and optional `AcceptChanges()` after success; bulk path treats table as dumb row bag — validate before `WriteToServer`.
- **Failure behavior:** Adapter can stop mid-batch with some rows committed unless wrapped in transaction; bulk copy supports transactional all-or-nothing with **`SqlTransaction`** (chapter 06), plus `BatchSize` tuning and `NotifyAfter` progress.
- **Pick bulk when:** append-only import to staging, no per-row business rules in SQL command text, volume > tens of thousands.
- **Pick adapter when:** small editable grid sync back to source with Insert/Update/Delete mix and CommandBuilder-generated commands.

**Production takeaway:** RowState-driven `Update` is for **interactive edit sessions**; **`SqlBulkCopy`** is for **volume ingest** — previewed in this chapter, detailed in chapter 06.

---

#### Q6. (R) Production deploys a DB migration that renames column `UnitPrice` → `ListPrice`. The API still `Fill`s into a cached `DataTable` schema created at startup (columns defined in code). After deploy, users edit prices in the grid and call save. Review the save path:

```csharp
// Startup: table schema built in memory with columns ProductId, ProductName, UnitPrice
adapter.Fill(products); // DB now returns ListPrice — Fill adds a second price column

foreach (DataRow row in products.Rows)
{
    if (row["UnitPrice"] is DBNull && row["ListPrice"] is not DBNull)
        row["UnitPrice"] = row["ListPrice"]; // "migrate" in memory
}

adapter.Update(products);
products.AcceptChanges();
```

What RowState and persistence bugs appear after schema drift, and how do you prevent silent data loss?

---

**Answer:** Schema drift plus manual column copying produces **wrong RowState**, **CommandBuilder SQL against stale column names**, and **silent null writes** — users think they saved `ListPrice` while the DB gets stale or empty `UnitPrice`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Schema | `Fill` **adds** `ListPrice` column; in-code schema still has `UnitPrice` | Two price columns; mapper confusion |
| RowState | Copying values may mark rows `Modified` even when user did not edit | Unintended UPDATEs on every row |
| Persistence | `CommandBuilder` UPDATE still targets `UnitPrice` column (removed/deprecated) | SQL error or writing to wrong column |
| Correctness | Rows loaded before copy: `UnitPrice` DBNull → user never touches price → UPDATE sets NULL | Silent price wipe |
| Caching | Singleton cached `DataTable` with old schema at startup | All instances diverge until restart |

**Fix (priority order):**

1. **Deploy app + DB together** — rename column in C# schema and regenerate commands; do not rely on Fill to merge schemas.
2. After schema change, **`Clear()` + redefine columns** or new `DataTable` before Fill; avoid dual-column hack loops.
3. Set **`PrimaryKey`** before Update (**Program.cs** mistakes table); verify generated SQL against migrated names.
4. Call `AcceptChanges()` after Fill if you need Unchanged baseline before user edits (**RowStateDemo.cs**).
5. For APIs, drop cached `DataTable` — use typed reads so migrations fail fast at compile time.

**Production takeaway:** `Fill` merges **rows**, not **contract ownership** — schema drift + RowState is a common post-migration outage; treat column lists like API version bumps.

---

#### Q7. (R) An internal admin API exposes search over an in-memory `DataTable` filled from `SqlDataAdapter`. The controller builds a filter from query string input:

```csharp
public IActionResult FindProducts(string nameContains)
{
    DataTable products = _catalogCache.GetProductsTable(); // shared singleton cache
    string filter = $"ProductName LIKE '%{nameContains}%'";
    DataRow[] matches = products.Select(filter);
    return Ok(matches.Select(r => r["ProductName"]));
}
```

Diagnose security and correctness issues (expression syntax, caching, concurrency). What pattern replaces string-built `DataTable.Select` filters?

---

**Answer:** `DataTable.Select` uses a **mini expression language** (not SQL), but embedding raw user input enables **filter injection**, **syntax DoS**, and **cross-request mutation** when the cached table is shared.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Security | Unescaped `nameContains` in expression string | `'` breaks out: `' OR ProductName LIKE '%` alters logic; malicious expressions can throw or scan all rows |
| Correctness | `LIKE` in Select syntax differs from SQL; special chars `[]%*` need escaping | Unexpected matches or `SyntaxErrorException` → 500 |
| Concurrency | Singleton cached `DataTable` is **mutable** | One request's edits/deletes affect others; RowState corruption under parallel calls |
| Design | Full-table scan in memory per search | Does not scale; bypasses indexed SQL |
| API | Returns live `DataRow` references | Hidden schema leakage; not a stable DTO contract |

**Fix (priority order):**

1. **Never** interpolate user input into `Select` — use **`DataView.RowFilter`** with escaped literals, or skip expressions entirely.
2. Prefer **LINQ over `AsEnumerable()`** with plain C# string checks, or push search to SQL with **`SqlParameter`** (chapter 03).
3. Cache **immutable snapshots** (`ReadOnly` DTO list) refreshed on timer — not a shared editable `DataTable`.
4. If expressions are required, sanitize with `Regex.Replace` for `[]%*` and double single-quotes per DataExpression rules — still inferior to parameterized SQL.
5. Register cache as **scoped** or reload copy per request if snapshot pattern must stay in-memory.

Example safe in-memory filter:

```csharp
var matches = products.AsEnumerable()
    .Where(r => r.RowState != DataRowState.Deleted)
    .Where(r => (r.Field<string>("ProductName") ?? "")
        .Contains(nameContains, StringComparison.OrdinalIgnoreCase));
```

**Production takeaway:** `DataTable.Select` looks like SQL but is **client-side expression eval** — treat user filters like any dynamic query: **parameterize or use code predicates**, and do not share mutable `DataSet` state across requests.
