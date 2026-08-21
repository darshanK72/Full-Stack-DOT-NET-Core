# Karat — Interview Answers

Answers for [KARAT_INTERVIEW_QUESTIONS.md](./KARAT_INTERVIEW_QUESTIONS.md) in this folder.

> **Folder:** `04. .NET Data Access/01. ADO.NET/08. Async ADO.NET`

---

#### Q1. (R) A Minimal API endpoint "fixes" async by blocking on the repository. Review:

```csharp
app.MapGet("/products/count", () =>
{
    string cs = builder.Configuration.GetConnectionString("AdoNetTutorial")!;
    int count = ProductRepository.GetProductCountAsync(cs).Result;
    return Results.Ok(count);
});
```

`ProductRepository.GetProductCountAsync` uses `OpenAsync` and `ExecuteScalarAsync` with `ConfigureAwait(false)` (see **Repositories/ProductRepository.cs**). Under load in ASP.NET Core, what breaks, and how do you fix it end-to-end?

**Answer:** Blocking `.Result` on async ADO.NET turns non-blocking I/O back into **sync-over-async**: a thread-pool thread sits idle for the entire SQL round-trip while `OpenAsync`/`ExecuteScalarAsync` wait on the network. Under load this inflates latency, exhausts the thread pool, and can still deadlock when a captured context waits on itself. Fix by making the route `async`, `await` the repository, and thread `HttpContext.RequestAborted` through as `CancellationToken`.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `.Result` on `GetProductCountAsync()` | Blocks a thread for DB I/O; defeats purpose of async ADO.NET |
| Runtime/async | Sync lambda `MapGet` delegate | No cooperative cancellation; scales poorly vs `await` |
| Correctness | No `CancellationToken` passed | Client disconnect/timeouts cannot cancel `OpenAsync`/`ExecuteScalarAsync` |
| Design | Endpoints block while library correctly uses `ConfigureAwait(false)` | Repository is fine; caller undoes the benefit |

**Fix (priority order):**

1. Change the route to `async` and replace `.Result` with `await ProductRepository.GetProductCountAsync(cs, ctx.RequestAborted)`.
2. Extend **ProductRepository** callers (already accept optional token) to pass `RequestAborted` on every `OpenAsync`/`ExecuteScalarAsync` call.
3. Keep `ConfigureAwait(false)` **inside** the repository only — not in the endpoint.
4. Load-test before/after; watch thread-pool queue length and request latency under concurrent `/products/count` hits.

**Production takeaway:** Async ADO.NET only helps when the **call stack is async end-to-end** — see **Models/ProductRow.cs** rule: async controller → async service → async ADO.NET. See C# Module 06 — sync-over-async / `.Result` gotcha.

---

#### Q2. (R) A report endpoint streams products to the client. A teammate copied the **ProductStream** preview pattern but left the connection open on the caller:

```csharp
app.MapGet("/products/stream", async (HttpContext ctx) =>
{
    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    await foreach (ProductRow row in ProductStream.ReadProductsAsync(reader))
    {
        await ctx.Response.WriteAsJsonAsync(row);
    }
});
```

`ProductStream.ReadProductsAsync` passes a `CancellationToken` with `[EnumeratorCancellation]`. The endpoint does not pass `ctx.RequestAborted`, and a slow client disconnects mid-stream. Diagnose what keeps running after the client is gone and what you would change (token wiring + disposal).

**Answer:** Without linking `HttpContext.RequestAborted`, `ReadAsync` and the enumeration continue after the client disconnects — SQL Server keeps sending rows, the connection stays busy, and you waste pool slots until the query finishes or times out. Wire cancellation through the stream and ensure the `await using` chain disposes reader/connection when `OperationCanceledException` fires.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness/runtime | `ReadProductsAsync(reader)` — default `CancellationToken` | Disconnect does not cancel `ReadAsync`; DB work continues |
| Resource lifetime | Long-lived open reader + connection during slow response write | Holds SQL connection from pool for entire stream |
| HTTP | No abort handling on `WriteAsJsonAsync` | Server may throw mid-write without cooperative cancel on reader |
| Design | Token parameter exists on **ProductStream** but caller ignores it | `[EnumeratorCancellation]` never receives request abort |

**Fix (priority order):**

1. Pass the abort token: `await foreach (... in ProductStream.ReadProductsAsync(reader, ctx.RequestAborted))`.
2. Wrap the loop in `try/catch` for `OperationCanceledException` when the client drops — log at debug, do not treat as 500.
3. Keep `await using` on connection, command, and reader so `DisposeAsync` runs on cancel (same pattern as **Program.cs** `DemonstrateAwaitUsingAsync`).
4. For very large exports, consider `CommandBehavior.SequentialAccess`, chunked HTTP (NDJSON lines), and command timeout aligned with proxy limits.
5. Optionally use `Response.RegisterForDisposeAsync` if ownership moves into middleware — but `await using` in the handler is sufficient here.

**Production takeaway:** Streaming ADO.NET is a **lifetime problem** — the reader owns the connection until disposed; cancellation must flow from HTTP → `IAsyncEnumerable` → every `ReadAsync`. See **Services/ProductStream.cs** and **Program.cs** Section 13 preview.

---

#### Q3. (R) With `MultipleActiveResultSets=True` on the connection string, a service tries to update stock while a reader is still open:

```csharp
public async Task RefreshCatalogAsync(string connectionString)
{
    await using SqlConnection conn = new SqlConnection(connectionString); // MARS enabled in cs
    await conn.OpenAsync();

    await using SqlCommand select = conn.CreateCommand();
    select.CommandText = "SELECT ProductId, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await select.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        int id = reader.GetInt32(0);
        int stock = reader.GetInt32(1);

        if (stock == 0)
        {
            await using SqlCommand update = conn.CreateCommand();
            update.CommandText = "UPDATE dbo.Products SET Stock = 10 WHERE ProductId = @Id;";
            update.Parameters.Add(new SqlParameter("@Id", id));
            await update.ExecuteNonQueryAsync(); // second active command on same connection
        }
    }
}
```

What can go wrong with MARS + interleaved reader and command lifetime, and what pattern would you use instead in production?

**Answer:** MARS allows a second command on the same connection while a reader is open, but interleaving read and write on one connection is fragile: ordering bugs, long-held locks, unexpected transaction behavior, and hard-to-debug "There is already an open DataReader" errors when MARS is off in another environment. Production code should finish (or dispose) the reader first, or use separate connections / set-based SQL.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Correctness | Per-row `UPDATE` inside `ReadAsync` loop | N round-trips; race with other writers; slow catalog refresh |
| Resource lifetime | Reader open for entire loop while commands interleave | Extends lock duration on server; connection busy for minutes on large tables |
| Environment | Code assumes MARS in connection string | Same code throws in staging/prod if MARS omitted — "open DataReader" |
| Design | Row-at-a-time imperative update | Does not scale; bypasses set-based `UPDATE ... WHERE Stock = 0` |

**Fix (priority order):**

1. Prefer **set-based SQL**: single `UPDATE dbo.Products SET Stock = 10 WHERE Stock = 0` — one round-trip, no MARS needed.
2. If you must read then write, **buffer keys** from the reader into a `List<int>`, dispose the reader, then batch updates (or use a second connection for writes).
3. Do not rely on MARS as a default architecture — treat it as a narrow escape hatch; document if truly required.
4. Pass `CancellationToken` on `OpenAsync`, `ReadAsync`, and `ExecuteNonQueryAsync` for long catalogs.
5. Wrap multi-statement work in an explicit transaction when atomicity matters.

**Production takeaway:** Async does not fix **connection semantics** — one connection, one active reader unless MARS is explicitly enabled and understood. Prefer set-based commands like **ProductRepository.InsertProductAsync** over interleaved reader/command patterns.

---

#### Q4. (P) Your shared data-access library (same style as **ProductRepository**) is consumed by ASP.NET Core APIs and a WinForms desktop app. A junior adds `ConfigureAwait(false)` to every await in the **API controllers** "because the ADO.NET chapter says library code should." Another dev removes `ConfigureAwait(false)` from **ProductRepository** "because ASP.NET doesn't need it." Who is right in each case, and what is the production rule?

**Answer:** The repository author was closer to correct. **`ConfigureAwait(false)` belongs in library/repository code** that has no dependency on returning to a specific context; **application code** (Minimal API endpoints, WinForms event handlers) normally omits it so continuations can resume on the request or UI context when one exists.

- **Repository / ProductRepository:** Keep `ConfigureAwait(false)` on every `await` after `OpenAsync`, `ExecuteScalarAsync`, and `ExecuteNonQueryAsync` — as in **Repositories/ProductRepository.cs**. The library must not assume ASP.NET or WinForms context; false avoids unnecessary marshaling when a `SynchronizationContext` is present (legacy ASP.NET, UI apps).
- **ASP.NET Core Minimal API / controllers:** Do **not** sprinkle `ConfigureAwait(false)` — there is usually no captured context in modern ASP.NET Core, and you may need `HttpContext` after `await` without extra plumbing. Adding it everywhere in app code is noise and can confuse reviewers.
- **WinForms desktop:** Omit `ConfigureAwait(false)` in UI event handlers when you must touch controls after `await`; use it in downstream library calls (the repository already does).
- **Console apps (this chapter's demos):** No `SynchronizationContext` — behavior matches with or without `ConfigureAwait(false)`; the repository pattern is shown for copy-paste into services.

**Production takeaway:** Rule of thumb — **`ConfigureAwait(false)` in reusable data-access libraries; default `await` in app/host code.** See **Program.cs** Section 12 and **ProductRepository.cs** Section 3 comments.

---

#### Q5. (R) A legacy sync helper was merged into an async export job:

```csharp
public async Task ExportProductsAsync(string connectionString, Stream output)
{
    await using SqlConnection conn = new SqlConnection(connectionString);
    conn.Open(); // sync open — "it's just one line"

    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ProductId, Name, UnitPrice, Stock FROM dbo.Products;";

    await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

    while (reader.Read()) // sync Read inside async method
    {
        decimal price = reader.GetDecimal(2);
        await output.WriteAsync(Encoding.UTF8.GetBytes($"{price}\n"));
    }
}
```

List the defects (sync/async mixing, thread blocking, reader semantics) and prioritize fixes.

**Answer:** This method mixes async and sync ADO.NET on the same connection and reader — exactly the pitfall documented in **Program.cs** Section 9. `conn.Open()` and `reader.Read()` block thread-pool threads during network I/O, partially negating `ExecuteReaderAsync`, and can cause subtle ordering/state bugs with Microsoft.Data.SqlClient when sync and async APIs are interleaved.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `conn.Open()` sync after async setup | Blocks thread during connect; inconsistent async pattern |
| Runtime/async | `reader.Read()` in `while` with `ExecuteReaderAsync` | Blocks per row; **do not mix sync Read and async ReadAsync** on same reader |
| Correctness | Mixed sync/async on same `SqlConnection`/`SqlDataReader` | Undefined/problematic behavior; harder failures under load |
| Maintainability | `async` method that mostly blocks | Misleading signature; reviewers assume non-blocking export |

**Fix (priority order):**

1. Replace `conn.Open()` with `await conn.OpenAsync(cancellationToken)`.
2. Replace `while (reader.Read())` with `while (await reader.ReadAsync(cancellationToken))`.
3. Pass `CancellationToken` from the host job through open, execute, read, and `output.WriteAsync`.
4. Consider **ProductStream.ReadProductsAsync** + `await foreach` for clearer streaming semantics.
5. Add `ConfigureAwait(false)` if this lives in a library class consumed by multiple hosts.

**Production takeaway:** Pick one style per connection/reader — **async all the way** for server export paths. See **Program.cs** pitfall note under Section 9.

---

#### Q6. (R) A dashboard action needs average price and count. The developer avoids "async all the way" for the scalar:

```csharp
app.MapGet("/dashboard", async (IConfiguration config) =>
{
    string cs = config.GetConnectionString("AdoNetTutorial")!;

    Task<int> countTask = ProductRepository.GetProductCountAsync(cs);

    await using SqlConnection conn = new SqlConnection(cs);
    await conn.OpenAsync();
    await using SqlCommand cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT AVG(UnitPrice) FROM dbo.Products;";
    decimal avg = Convert.ToDecimal(cmd.ExecuteScalar()); // sync scalar on open async connection

    int count = await countTask;
    return Results.Ok(new { avg, count });
});
```

Two connections hit SQL Server concurrently, but throughput still collapses under load. Explain the thread-pool impact of `ExecuteScalar()` here and how you would rewrite this endpoint.

**Answer:** Starting `countTask` then calling sync `ExecuteScalar()` still blocks the **current request thread** until AVG returns, while another connection runs the count query. Under concurrency, each request ties up a thread-pool thread for both waits — parallel tasks do not help if every handler blocks on `ExecuteScalar()`. Replace sync scalar with `ExecuteScalarAsync`, pass `RequestAborted`, and prefer one round-trip or properly awaited parallel work.

**Issues:**

| Category | Problem | Impact |
|---|---|---|
| Runtime/async | `cmd.ExecuteScalar()` sync in async endpoint | Blocks thread during aggregate query — same class of bug as sync `Open()` |
| Resource | Two connections per dashboard hit | Doubles pool pressure; often unnecessary |
| Design | Fire-and-forget `countTask` without awaiting until after sync block | Overlaps I/O but request thread still blocked on scalar |
| Correctness | No cancellation token on either query | Slow aggregates hold threads after client abort |

**Fix (priority order):**

1. Replace `ExecuteScalar()` with `await cmd.ExecuteScalarAsync(ctx.RequestAborted)` — mirror **Program.cs** `DemonstrateExecuteScalarAsync`.
2. Pass `ctx.RequestAborted` into `GetProductCountAsync(cs, ctx.RequestAborted)`.
3. Await both with `Task.WhenAll` if keeping two queries, **or** combine: `SELECT COUNT(*), AVG(UnitPrice) FROM dbo.Products` in one command (best for dashboard).
4. Remove redundant second connection when a single batch/query suffices.
5. Load-test: thread-pool starvation shows up as growing queue latency even when SQL is "parallel."

**Production takeaway:** `ExecuteScalarAsync` exists for the same reason as `OpenAsync` — **release the thread while waiting on SQL Server**. Parallel `Task`s do not fix sync blocking on the request thread. See **Program.cs** Sections 7–8.

---

#### Q7. (D) You must expose a large product export from ADO.NET without loading a `List<ProductRow>`. Options: (A) buffer with `ExecuteReaderAsync` + `List<T>`, (B) `IAsyncEnumerable<ProductRow>` over `ReadAsync` like **Services/ProductStream.cs**, or (C) raw `SqlDataReader` returned from the repository. Compare memory, cancellation, connection lifetime, and ASP.NET response shaping — which do you ship and why?

**Answer:** Ship **(B) `IAsyncEnumerable<ProductRow>`** (or a thin wrapper around **ProductStream.ReadProductsAsync**) for ASP.NET Core exports. It streams rows with pull-based `ReadAsync`, composes with `await foreach`, accepts `[EnumeratorCancellation]` / `RequestAborted`, and keeps `SqlDataReader` + connection lifetime inside the handler or an scoped service method — without leaking ADO.NET types into controllers.

| Option | Memory | Cancellation | Connection / reader lifetime | ASP.NET shaping |
|---|---|---|---|---|
| **(A) List buffer** | O(n) — all rows in RAM | Easy but late — work mostly done before return | Short reader life; simple | Simple JSON array — bad for huge catalogs |
| **(B) IAsyncEnumerable** | O(1) per step — one row at a time | `[EnumeratorCancellation]` → `ReadAsync(ct)` | Caller must `await using` conn/reader for enumeration duration | `IResult` from `TypedResults`, NDJSON, or custom chunked body |
| **(C) Raw SqlDataReader** | O(1) if consumed immediately | Manual on every read | **Leaky** — caller must dispose; easy to double-dispose | Forces API layer to know ADO.NET — poor seam |

- **Reject (A)** for large exports — **Program.cs** Section 13 contrasts streaming vs loading a `List` first; fine only for small, bounded reports.
- **Reject (C)** in layered apps — returning `SqlDataReader` from a repository breaks abstraction and makes DI/scoping errors likely (disposed connection before controller reads).
- **Choose (B)** — align with **ProductStream.cs**: `while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) yield return ...`; handler owns `await using SqlConnection` / reader for the life of the response; pass `HttpContext.RequestAborted`.
- Add **timeouts** (`CommandTimeout`, client/proxy limits) and **SequentialAccess** if rows are wide; log row counts, not every row.

**Production takeaway:** `IAsyncEnumerable` is the bridge between **forward-only async ADO.NET** and **HTTP streaming** — keep connection disposal in one place, cancellation end-to-end, and ADO.NET types behind the repository/stream helper.
